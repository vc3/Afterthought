//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Cci.MetadataReader;
using Microsoft.Cci.MetadataReader.PEFile;
using Microsoft.Cci.MetadataReader.ObjectModelImplementation;
using Microsoft.Cci.UtilityDataStructures;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Contracts;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// Generic exception thrown by the internal implementation. This exception is not meant to be leaked outside, hence all the
  /// public classes where this exception can be thrown needs to catch this.
  /// </summary>
  internal sealed class MetadataReaderException : Exception {
    internal MetadataReaderException(
      string message
    )
      : base(message) {
    }
  }

  /// <summary>
  /// This interface is implemented by providers of Module read write errors. That is, errors discovered while reading the metadata/il.
  /// Many of these errors will be discovered incrementally and as part of background activities.
  /// </summary>
  public interface IMetadataReaderErrorsReporter {
  }

  /// <summary>
  /// Dummy class to identify the error reporter.
  /// </summary>
  internal sealed class MetadataReaderErrorsReporter : IMetadataReaderErrorsReporter {
  }

  /// <summary>
  /// Factory for loading assemblies and modules persisted as portable executable (pe) files. 
  /// </summary>
  public sealed class PeReader {
    internal readonly MetadataReaderErrorsReporter ErrorsReporter;
    internal readonly IMetadataReaderHost metadataReaderHost;
    //  In Multimodule case only the main module is added to this map.
    readonly Hashtable<Module> InternedIdToModuleMap;
    Assembly/*?*/ coreAssembly;

    internal readonly IName Value__;
    internal readonly IName AsyncCallback;
    internal readonly IName ParamArrayAttribute;
    internal readonly IName IAsyncResult;
    internal readonly IName ICloneable;
    internal readonly IName RuntimeArgumentHandle;
    internal readonly IName RuntimeFieldHandle;
    internal readonly IName RuntimeMethodHandle;
    internal readonly IName RuntimeTypeHandle;
    internal readonly IName ArgIterator;
    internal readonly IName IList;
    internal readonly IName Mscorlib;
    internal readonly IName System_Runtime;
    internal readonly IName _Deleted_;

    /*^
    #pragma warning disable 2669
    ^*/
    /// <summary>
    /// Allocates a factory for loading assemblies and modules persisted as portable executable (pe) files.
    /// </summary>
    /// <param name="metadataReaderHost">
    /// The host is used for providing access to pe files (OpenBinaryDocument),
    /// applying host specific unification policies (UnifyAssembly, UnifyAssemblyReference, UnifyModuleReference) and for deciding
    /// whether and how to load referenced assemblies and modules (ResolvingAssemblyReference, ResolvingModuleReference).    
    /// </param>
    public PeReader(
      IMetadataReaderHost metadataReaderHost
    ) {
      this.ErrorsReporter = new MetadataReaderErrorsReporter();
      this.metadataReaderHost = metadataReaderHost;
      this.InternedIdToModuleMap = new Hashtable<Module>();
      INameTable nameTable = metadataReaderHost.NameTable;
      this.Value__ = nameTable.GetNameFor("value__");
      this.AsyncCallback = nameTable.GetNameFor("AsyncCallback");
      this.ParamArrayAttribute = nameTable.GetNameFor("ParamArrayAttribute");
      this.IAsyncResult = nameTable.GetNameFor("IAsyncResult");
      this.ICloneable = nameTable.GetNameFor("ICloneable");
      this.RuntimeArgumentHandle = nameTable.GetNameFor("RuntimeArgumentHandle");
      this.RuntimeFieldHandle = nameTable.GetNameFor("RuntimeFieldHandle");
      this.RuntimeMethodHandle = nameTable.GetNameFor("RuntimeMethodHandle");
      this.RuntimeTypeHandle = nameTable.GetNameFor("RuntimeTypeHandle");
      this.ArgIterator = nameTable.GetNameFor("ArgIterator");
      this.IList = nameTable.GetNameFor("IList");
      this.Mscorlib = nameTable.GetNameFor("mscorlib");
      this.System_Runtime = nameTable.GetNameFor("System.Runtime");
      this._Deleted_ = nameTable.GetNameFor("_Deleted*");
    }
    /*^
    #pragma warning restore 2669
    ^*/

    /// <summary>
    /// Registers the core assembly. This is called by PEFileToObjectModel when it recognizes that assembly being loaded is the Core assembly as
    /// identified by the Compilation Host.
    /// </summary>
    /// <param name="coreAssembly"></param>
    internal void RegisterCoreAssembly(Assembly/*?*/ coreAssembly) {
      if (coreAssembly == null) {
        //  Error... Core assembly is not proper.
      }
      if (this.coreAssembly != null) {
        //  Error Core Assembly Already loaded?!?
      }
      this.coreAssembly = coreAssembly;
    }

    internal Assembly/*?*/ CoreAssembly {
      get {
        if (this.coreAssembly == null)
          this.coreAssembly = this.LookupAssembly(null, this.metadataReaderHost.CoreAssemblySymbolicIdentity) as Assembly;
        return this.coreAssembly;
      }
    }

    /// <summary>
    /// This method is called when an assembly is loaded. This makes sure that all the member modules of the assembly are loaded.
    /// </summary>
    /// <param name="binaryDocument"></param>
    /// <param name="assembly"></param>
    void OpenMemberModules(IBinaryDocument binaryDocument, Assembly assembly) {
      List<Module> memberModuleList = new List<Module>();
      AssemblyIdentity assemblyIdentity = assembly.AssemblyIdentity;
      foreach (IFileReference fileRef in assembly.PEFileToObjectModel.GetFiles()) {
        if (!fileRef.HasMetadata)
          continue;
        IBinaryDocumentMemoryBlock/*?*/ binaryDocumentMemoryBlock = this.metadataReaderHost.OpenBinaryDocument(binaryDocument, fileRef.FileName.Value);
        if (binaryDocumentMemoryBlock == null) {
          //  Error...
          continue;
        }
        try {
          PEFileReader peFileReader = new PEFileReader(this, binaryDocumentMemoryBlock);
          if (peFileReader.ReaderState < ReaderState.Metadata) {
            //  Error...
            continue;
          }
          if (peFileReader.IsAssembly) {
            //  Error...
            continue;
          }
          ModuleIdentity moduleIdentity = this.GetModuleIdentifier(peFileReader, assemblyIdentity);
          PEFileToObjectModel peFileToObjectModel = new PEFileToObjectModel(this, peFileReader, moduleIdentity, assembly, this.metadataReaderHost.PointerSize);
          memberModuleList.Add(peFileToObjectModel.Module);
        } catch (MetadataReaderException) {
          continue;
        }
      }
      if (memberModuleList.Count == 0)
        return;
      assembly.SetMemberModules(memberModuleList.ToArray());
    }

    void LoadedModule(Module module) {
      this.InternedIdToModuleMap.Add(module.InternedModuleId, module);
    }

    /// <summary>
    /// Method to open the assembly in MetadataReader. This method loads the assembly and returns the object corresponding to the
    /// opened assembly. Also returns the AssemblyIdentifier corresponding to the assembly as the out parameter.
    /// Only assemblies that unify to themselves can be opened i.e. if the unification policy of the compilation host says that mscorlib 1.0 unifies to mscorlib 2.0
    /// then only mscorlib 2.0 can be loaded.
    /// </summary>
    /// <param name="binaryDocument">The binary document that needes to be opened as an assembly.</param>
    /// <param name="assemblyIdentity">Contains the assembly identifier of the binary document in case it is an assembly.</param>
    /// <returns>Assembly that is loaded or Dummy.Assembly in case assembly could not be loaded.</returns>
    public IAssembly OpenAssembly(IBinaryDocument binaryDocument, out AssemblyIdentity/*?*/ assemblyIdentity) {
      Contract.Requires(binaryDocument != null);
      Contract.Ensures(Contract.Result<IAssembly>() != null);

      assemblyIdentity = null;
      lock (GlobalLock.LockingObject) {
        IBinaryDocumentMemoryBlock/*?*/ binaryDocumentMemoryBlock = this.metadataReaderHost.OpenBinaryDocument(binaryDocument);
        if (binaryDocumentMemoryBlock == null) {
          //  Error...
          return Dummy.Assembly;
        }
        PEFileReader peFileReader = new PEFileReader(this, binaryDocumentMemoryBlock);
        if (peFileReader.ReaderState < ReaderState.Metadata) {
          //  Error...
          return Dummy.Assembly;
        }
        //^ assert peFileReader.ReaderState >= ReaderState.Metadata;
        if (!peFileReader.IsAssembly) {
          //  Error...
          return Dummy.Assembly;
        }
        assemblyIdentity = this.GetAssemblyIdentifier(peFileReader);
        Assembly/*?*/ lookupAssembly = this.LookupAssembly(null, assemblyIdentity);
        if (lookupAssembly != null) {
          return lookupAssembly;
        }
        try {
          PEFileToObjectModel peFileToObjectModel = new PEFileToObjectModel(this, peFileReader, assemblyIdentity, null, this.metadataReaderHost.PointerSize);
          Assembly/*?*/ assembly = peFileToObjectModel.Module as Assembly;
          //^ assert assembly != null;
          this.LoadedModule(assembly);
          this.OpenMemberModules(binaryDocument, assembly);
          return assembly;
        } catch (MetadataReaderException) {
          return Dummy.Assembly;
        }
      }
    }

    /// <summary>
    /// Method to open the module in the MetadataReader. This method loads the module and returns the object corresponding to the opened module.
    /// Also returns the ModuleIDentifier corresponding to the module as the out parameter. Modules are opened as if they are not contained in any assembly.
    /// </summary>
    /// <param name="binaryDocument">The binary document that needes to be opened as an module.</param>
    /// <param name="moduleIdentity">Contains the module identity of the binary document in case it is an module.</param>
    /// <returns>Module that is loaded or Dummy.Module in case module could not be loaded.</returns>
    public IModule OpenModule(IBinaryDocument binaryDocument, out ModuleIdentity/*?*/ moduleIdentity) {
      Contract.Requires(binaryDocument != null);
      Contract.Ensures(Contract.Result<IModule>() != null);

      moduleIdentity = null;
      lock (GlobalLock.LockingObject) {
        IBinaryDocumentMemoryBlock/*?*/ binaryDocumentMemoryBlock = this.metadataReaderHost.OpenBinaryDocument(binaryDocument);
        if (binaryDocumentMemoryBlock == null) {
          //  Error...
          return Dummy.Module;
        }
        PEFileReader peFileReader = new PEFileReader(this, binaryDocumentMemoryBlock);
        if (peFileReader.ReaderState < ReaderState.Metadata) {
          //  Error...
          return Dummy.Module;
        }
        //^ assert peFileReader.ReaderState >= ReaderState.Metadata;
        if (peFileReader.IsAssembly) {
          AssemblyIdentity assemblyIdentity = this.GetAssemblyIdentifier(peFileReader);
          moduleIdentity = assemblyIdentity;
          Assembly/*?*/ lookupAssembly = this.LookupAssembly(null, assemblyIdentity);
          if (lookupAssembly != null) {
            return lookupAssembly;
          }
        } else {
          moduleIdentity = this.GetModuleIdentifier(peFileReader);
          Module/*?*/ lookupModule = this.LookupModule(null, moduleIdentity);
          if (lookupModule != null) {
            return lookupModule;
          }
        }
        try {
          PEFileToObjectModel peFileToObjectModel = new PEFileToObjectModel(this, peFileReader, moduleIdentity, null, this.metadataReaderHost.PointerSize);
          this.LoadedModule(peFileToObjectModel.Module);
          Assembly/*?*/ assembly = peFileToObjectModel.Module as Assembly;
          if (assembly != null) {
            this.OpenMemberModules(binaryDocument, assembly);
          }
          return peFileToObjectModel.Module;
        } catch (MetadataReaderException) {
          //  Error...
        }
      }
      return Dummy.Module;
    }

    /// <summary>
    /// Method to open the assembly in MetadataReader. This method loads the assembly and returns the object corresponding to the
    /// opened assembly. Also returns the AssemblyIdentifier corresponding to the assembly as the out parameter.
    /// Only assemblies that unify to themselves can be opened i.e. if the unification policy of the compilation host says that mscorlib 1.0 unifies to mscorlib 2.0
    /// then only mscorlib 2.0 can be loaded.
    /// </summary>
    /// <param name="binaryDocument">The binary document that needes to be opened as an assembly.</param>
    /// <returns>Assembly that is loaded or Dummy.Assembly in case assembly could not be loaded.</returns>
    public IAssembly OpenAssembly(IBinaryDocument binaryDocument) {
      Contract.Requires(binaryDocument != null);
      Contract.Ensures(Contract.Result<IAssembly>() != null);

      AssemblyIdentity/*?*/ retAssemblyIdentity;
      return this.OpenAssembly(binaryDocument, out retAssemblyIdentity);
    }

    /// <summary>
    /// Method to open the module in the MetadataReader. This method loads the module and returns the object corresponding to the opened module.
    /// Also returns the ModuleIDentifier corresponding to the module as the out parameter. Modules are opened as if they are not contained in any assembly.
    /// </summary>
    /// <param name="binaryDocument">The binary document that needes to be opened as an module.</param>
    /// <returns>Module that is loaded or Dummy.Module in case module could not be loaded.</returns>
    public IModule OpenModule(IBinaryDocument binaryDocument) {
      Contract.Requires(binaryDocument != null);
      Contract.Ensures(Contract.Result<IModule>() != null);

      ModuleIdentity/*?*/ retModuleIdentity;
      return this.OpenModule(binaryDocument, out retModuleIdentity);
    }

    /// <summary>
    /// This method loads a module from a file containing only the metadata section of a PE file. (No headers and no IL.)
    /// </summary>
    /// <param name="binaryDocument">The binary document that needes to be opened as an module.</param>
    /// <returns>Module that is loaded or Dummy.Module in case module could not be loaded.</returns>
    public IModule OpenSnapshot(IBinaryDocument binaryDocument) {
      ModuleIdentity moduleIdentity;
      lock (GlobalLock.LockingObject) {
        IBinaryDocumentMemoryBlock/*?*/ binaryDocumentMemoryBlock = this.metadataReaderHost.OpenBinaryDocument(binaryDocument);
        if (binaryDocumentMemoryBlock == null) {
          //  Error...
          return Dummy.Module;
        }
        PEFileReader peFileReader = new PEFileReader(this, binaryDocumentMemoryBlock, snapshot: true);
        if (peFileReader.ReaderState < ReaderState.Metadata) {
          //  Error...
          return Dummy.Module;
        }
        if (peFileReader.IsAssembly) {
          AssemblyIdentity assemblyIdentity = this.GetAssemblyIdentifier(peFileReader);
          moduleIdentity = assemblyIdentity;
          Assembly/*?*/ lookupAssembly = this.LookupAssembly(null, assemblyIdentity);
          if (lookupAssembly != null) {
            return lookupAssembly;
          }
        } else {
          moduleIdentity = this.GetModuleIdentifier(peFileReader);
          Module/*?*/ lookupModule = this.LookupModule(null, moduleIdentity);
          if (lookupModule != null) {
            return lookupModule;
          }
        }
        if (peFileReader.ReaderState < ReaderState.Metadata) {
          //  Error...
          return Dummy.Module;
        }
        try {
          PEFileToObjectModel peFileToObjectModel = new PEFileToObjectModel(this, peFileReader, moduleIdentity, null, this.metadataReaderHost.PointerSize);
          this.LoadedModule(peFileToObjectModel.Module);
          return peFileToObjectModel.Module;
        } catch (MetadataReaderException) {
          //  Error...
        }
      }
      return Dummy.Module;
    }

    /// <summary>
    /// Does a look up in the loaded assemblies if the given assembly identified by assemblyIdentifier is loaded. This also gives a chance to MetadataReaderHost to
    /// delay load the assembly if needed.
    /// </summary>
    /// <param name="referringModule"></param>
    /// <param name="unifiedAssemblyIdentity"></param>
    /// <returns></returns>
    internal Assembly/*?*/ LookupAssembly(IModule/*?*/ referringModule, AssemblyIdentity unifiedAssemblyIdentity) {
      lock (GlobalLock.LockingObject) {
        uint internedModuleId = (uint)this.metadataReaderHost.InternFactory.GetAssemblyInternedKey(unifiedAssemblyIdentity);
        Module/*?*/ module = this.InternedIdToModuleMap.Find(internedModuleId);
        if (module == null && referringModule != null) {
          this.metadataReaderHost.ResolvingAssemblyReference(referringModule, unifiedAssemblyIdentity);
          // See if the host loaded the assembly using this PeReader (loading indirectly causes the map to be updated)
          module = this.InternedIdToModuleMap.Find(internedModuleId);
          if (module == null) {
            // One last chance, it might have been already loaded by a different instance of PeReader
            var a = this.metadataReaderHost.FindAssembly(unifiedAssemblyIdentity);
            Module m = a as Module;
            if (m != null) {
              this.LoadedModule(m);
              module = m;
            }
          }
        }
        return module as Assembly;
      }
    }

    /// <summary>
    /// Does a look up in the loaded modules if the given module identified by moduleIdentifier is loaded. This also gives a chance to MetadataReaderHost to
    /// delay load the module if needed.
    /// </summary>
    /// <param name="referringModule"></param>
    /// <param name="moduleIdentity"></param>
    /// <returns></returns>
    internal Module/*?*/ LookupModule(IModule/*?*/ referringModule, ModuleIdentity moduleIdentity) {
      lock (GlobalLock.LockingObject) {
        uint internedModuleId = (uint)this.metadataReaderHost.InternFactory.GetModuleInternedKey(moduleIdentity);
        Module/*?*/ module = this.InternedIdToModuleMap.Find(internedModuleId);
        if (module == null && referringModule != null) {
          this.metadataReaderHost.ResolvingModuleReference(referringModule, moduleIdentity);
          module = this.InternedIdToModuleMap.Find(internedModuleId);
        }
        return module;
      }
    }

    /// <summary>
    /// If the given binary document contains a CLR assembly, return the identity of the assembly. Otherwise, return null.
    /// </summary>
    public AssemblyIdentity/*?*/ GetAssemblyIdentifier(IBinaryDocument binaryDocument) {
      IBinaryDocumentMemoryBlock/*?*/ binaryDocumentMemoryBlock = this.metadataReaderHost.OpenBinaryDocument(binaryDocument);
      if (binaryDocumentMemoryBlock == null) return null;
      PEFileReader peFileReader = new PEFileReader(this, binaryDocumentMemoryBlock);
      if (peFileReader.ReaderState < ReaderState.Metadata) return null;
      if (!peFileReader.IsAssembly) return null;
      return this.GetAssemblyIdentifier(peFileReader);
    }

    /// <summary>
    /// Computes the AssemblyIdentifier of the PE File. This requires that peFile is an assembly.
    /// </summary>
    /// <param name="peFileReader"></param>
    /// <returns></returns>
    internal AssemblyIdentity GetAssemblyIdentifier(PEFileReader peFileReader)
      //^ requires peFileReader.ReaderState >= ReaderState.Metadata && peFileReader.IsAssembly;
      //^ ensures (result.Location != null && result.Location.Length != 0);
    {
      AssemblyRow assemblyRow = peFileReader.AssemblyTable[1];
      IName assemblyName = this.metadataReaderHost.NameTable.GetNameFor(peFileReader.StringStream[assemblyRow.Name]);
      string cultureName = peFileReader.StringStream[assemblyRow.Culture];
      Version version = new Version(assemblyRow.MajorVersion, assemblyRow.MinorVersion, assemblyRow.BuildNumber, assemblyRow.RevisionNumber);
      byte[] publicKeyArray = TypeCache.EmptyByteArray;
      byte[] publicKeyTokenArray = TypeCache.EmptyByteArray;
      if (assemblyRow.PublicKey != 0) {
        publicKeyArray = peFileReader.BlobStream[assemblyRow.PublicKey];
        if (publicKeyArray.Length > 0) {
          publicKeyTokenArray = UnitHelper.ComputePublicKeyToken(publicKeyArray);
        }
      }
      return new AssemblyIdentity(assemblyName, cultureName, version, publicKeyTokenArray, peFileReader.BinaryDocumentMemoryBlock.BinaryDocument.Location);
    }

    /// <summary>
    /// Computes the ModuleIdentifier of the PE File as if the module did not belong to any assembly.
    /// </summary>
    /// <param name="peFileReader"></param>
    /// <returns></returns>
    internal ModuleIdentity GetModuleIdentifier(PEFileReader peFileReader)
      //^ requires peFileReader.ReaderState >= ReaderState.Metadata;
      //^ ensures (result.Location != null && result.Location.Length != 0);
    {
      ModuleRow moduleRow = peFileReader.ModuleTable[1];
      IName moduleName = this.metadataReaderHost.NameTable.GetNameFor(peFileReader.StringStream[moduleRow.Name]);
      return new ModuleIdentity(moduleName, peFileReader.BinaryDocumentMemoryBlock.BinaryDocument.Location);
    }

    /// <summary>
    /// Computes the ModuleIdentifier of the PE File as if the module belong to given assembly.
    /// </summary>
    /// <param name="peFileReader"></param>
    /// <param name="containingAssemblyIdentity"></param>
    /// <returns></returns>
    internal ModuleIdentity GetModuleIdentifier(PEFileReader peFileReader, AssemblyIdentity containingAssemblyIdentity)
      //^ requires peFileReader.ReaderState >= ReaderState.Metadata;
      //^ ensures (result.Location != null && result.Location.Length != 0);
    {
      ModuleRow moduleRow = peFileReader.ModuleTable[1];
      IName moduleName = this.metadataReaderHost.NameTable.GetNameFor(peFileReader.StringStream[moduleRow.Name]);
      return new ModuleIdentity(moduleName, peFileReader.BinaryDocumentMemoryBlock.BinaryDocument.Location, containingAssemblyIdentity);
    }

    /// <summary>
    /// Lists all the opened modules.
    /// </summary>
    public IEnumerable<IModule> OpenedModules {
      get {
        foreach (Module module in this.InternedIdToModuleMap.Values) {
          yield return module;
        }
      }
    }

    /// <summary>
    /// Returns the module corresponding to passed moduleIdentifier if it was loaded.
    /// </summary>
    /// <param name="moduleIdentity"></param>
    /// <returns></returns>
    public IModule/*?*/ FindModule(ModuleIdentity moduleIdentity) {
      return this.LookupModule(null, moduleIdentity);
    }

    /// <summary>
    /// Returns the assembly corresponding to passed assemblyIdentifier if it was loaded.
    /// </summary>
    /// <param name="unifiedAssemblyIdentity">THe assembly Identifier that is unified with respect to the compilation host.</param>
    /// <returns></returns>
    public IAssembly/*?*/ FindAssembly(AssemblyIdentity unifiedAssemblyIdentity) {
      lock (GlobalLock.LockingObject) {
        uint internedModuleId = (uint)this.metadataReaderHost.InternFactory.GetAssemblyInternedKey(unifiedAssemblyIdentity);
        Module/*?*/ module = this.InternedIdToModuleMap.Find(internedModuleId);
        return module as IAssembly;
      }
    }

    /// <summary>
    /// Resolves the serialized type name as if it belonged to the passed assembly.
    /// </summary>
    /// <param name="typeName">Serialized type name.</param>
    /// <param name="assembly">Assembly in which this needs to be resolved. If null then it is to be resolved in mscorlib.</param>
    /// <returns></returns>
    public ITypeDefinition ResolveSerializedTypeName(string typeName, IAssembly/*?*/ assembly) {
      if (assembly == null) {
        assembly = this.CoreAssembly;
      }
      Assembly/*?*/ internalAssembly = assembly as Assembly;
      if (internalAssembly == null) {
        return Dummy.Type;
      }
      ITypeReference/*?*/ moduleTypeRef = internalAssembly.PEFileToObjectModel.GetSerializedTypeNameAsTypeReference(typeName);
      if (moduleTypeRef == null) {
        return Dummy.Type;
      }
      return moduleTypeRef.ResolvedType;
    }

    /// <summary>
    /// A simple host environment using default settings inherited from MetadataReaderHost and that
    /// uses PeReader as its metadata reader.
    /// </summary>
    public class DefaultHost : MetadataReaderHost {
      /// <summary>
      /// The PeReader instance that is used to implement LoadUnitFrom.
      /// </summary>
      protected PeReader peReader;

      /// <summary>
      /// Allocates a simple host environment using default settings inherited from MetadataReaderHost and that
      /// uses PeReader as its metadata reader.
      /// </summary>
      public DefaultHost()
        : base(new NameTable(), new InternFactory(), 0, null, true) {
        this.peReader = new PeReader(this);
      }

      /// <summary>
      /// Allocates a simple host environment using default settings inherited from MetadataReaderHost and that
      /// uses PeReader as its metadata reader.
      /// </summary>
      /// <param name="nameTable">
      /// A collection of IName instances that represent names that are commonly used during compilation.
      /// This is a provided as a parameter to the host environment in order to allow more than one host
      /// environment to co-exist while agreeing on how to map strings to IName instances.
      /// </param>
      public DefaultHost(INameTable nameTable)
        : base(nameTable, new InternFactory(), 0, null, false) {
        this.peReader = new PeReader(this);
      }

      /// <summary>
      /// Returns the unit that is stored at the given location, or a dummy unit if no unit exists at that location or if the unit at that location is not accessible.
      /// </summary>
      /// <param name="location">A path to the file that contains the unit of metdata to load.</param>
      public override IUnit LoadUnitFrom(string location) {
        IUnit result = this.peReader.OpenModule(
          BinaryDocument.GetBinaryDocumentForFile(location, this));
        this.RegisterAsLatest(result);
        return result;
      }
    }
  }

}
