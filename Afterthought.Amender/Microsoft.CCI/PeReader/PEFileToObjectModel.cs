//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.Cci.Immutable;
using Microsoft.Cci.MetadataReader.Errors;
using Microsoft.Cci.MetadataReader.MethodBody;
using Microsoft.Cci.MetadataReader.PEFile;
using Microsoft.Cci.MetadataReader.PEFileFlags;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci.MetadataReader {
  using Microsoft.Cci.MetadataReader.ObjectModelImplementation;

  internal enum LoadState : byte {
    Uninitialized=0,
    Loading,
    Loaded,
  }

  //  Properties/Properties like methods (Like GetTypeDefinitionAtRow) lock here.
  //  rest of the methods are locked by the callers
  //  Private methods must always be locked by the calling methods
  internal sealed partial class PEFileToObjectModel {
    internal readonly PeReader ModuleReader;
    internal readonly Assembly/*?*/ ContainingAssembly;
    internal readonly IInternFactory InternFactory;
    internal readonly PEFileReader PEFileReader;
    internal readonly INameTable NameTable;
    internal readonly TypeCache typeCache;
    internal readonly byte pointerSize;
    internal readonly MetadataObjectDocument document;
    internal readonly Hashtable<IGenericTypeInstanceReference> genericInstanceCache = new Hashtable<IGenericTypeInstanceReference>();

    readonly Hashtable<IName> StringIndexToNameTable;
    readonly Hashtable<IName> StringIndexToUnmangledNameTable;
    //^ invariant this.PEFileReader.ReaderState >= ReaderState.Metadata;
    internal readonly _Module_Type _Module_;

    internal AssemblyReference/*?*/ SystemEnumAssembly;
    internal AssemblyReference/*?*/ SystemValueTypeAssembly;
    internal AssemblyReference/*?*/ SystemMulticastDelegateAssembly;
    internal AssemblyReference/*?*/ SystemTypeAssembly;
    internal AssemblyReference/*?*/ SystemParamArrayAttributeAssembly;

    CoreTypes CoreTypes {
      get {
        if (this.coreTypes == null) {
          lock (GlobalLock.LockingObject) {
            if (this.coreTypes == null)
              this.coreTypes = new CoreTypes(this);
          }
        }
        return this.coreTypes;
      }
    }
    CoreTypes/*?*/ coreTypes;

    internal IMetadataReaderNamedTypeReference SystemEnum {
      get { return this.CoreTypes.SystemEnum; }
    }
    internal IMetadataReaderNamedTypeReference SystemValueType {
      get { return this.CoreTypes.SystemValueType; }
    }
    internal IMetadataReaderNamedTypeReference SystemMulticastDelegate {
      get { return this.CoreTypes.SystemMulticastDelegate; }
    }
    internal IMetadataReaderNamedTypeReference SystemType {
      get { return this.CoreTypes.SystemType; }
    }
    internal IMetadataReaderNamedTypeReference SystemParamArrayAttribute {
      get { return this.CoreTypes.SystemParamArrayAttribute; }
    }

    /*^
    #pragma warning disable 2666, 2669, 2677, 2674
    ^*/
    internal PEFileToObjectModel(
      PeReader peReader,
      PEFileReader peFileReader,
      ModuleIdentity moduleIdentity,
      Assembly/*?*/ containingAssembly,
      byte pointerSize
    )
      //^ requires peFileReader.IsAssembly ==> moduleIdentity.ContainingAssembly != null;
      //^ requires peFileReader.IsAssembly ==> containingAssembly == null;
      //^ requires !(moduleIdentity.Location != null && moduleIdentity.Location.Length != 0);
    {
      this.pointerSize = pointerSize;
      this.document = new MetadataObjectDocument(this);
      this.ModuleReader = peReader;
      this.PEFileReader = peFileReader;
      this.NameTable = peReader.metadataReaderHost.NameTable;
      this.InternFactory = peReader.metadataReaderHost.InternFactory;
      this.StringIndexToNameTable = new Hashtable<IName>();
      this.StringIndexToUnmangledNameTable = new Hashtable<IName>();
      this.typeCache = new TypeCache(this);
      uint moduleNameOffset = peFileReader.ModuleTable.GetName(1);
      IName moduleName = this.GetNameFromOffset(moduleNameOffset);
      AssemblyIdentity/*?*/ assemblyIdentity = moduleIdentity as AssemblyIdentity;
      if (peFileReader.IsAssembly) {
        //^ assert assemblyIdentity != null;
        AssemblyRow assemblyRow = peFileReader.AssemblyTable[1];
        IName assemblyName = this.GetNameFromOffset(assemblyRow.Name);
        byte[] publicKeyArray = TypeCache.EmptyByteArray;
        if (assemblyRow.PublicKey != 0) {
          publicKeyArray = peFileReader.BlobStream[assemblyRow.PublicKey];
        }
        uint internedModuleId = (uint)peReader.metadataReaderHost.InternFactory.GetAssemblyInternedKey(assemblyIdentity);
        Assembly assem = new Assembly(this, moduleName, peFileReader.COR20Header.COR20Flags, internedModuleId, assemblyIdentity, assemblyName, assemblyRow.Flags, publicKeyArray);
        this.ContainingAssembly = assem;
        this.Module = assem;
      } else {
        uint internedModuleId = (uint)peReader.metadataReaderHost.InternFactory.GetModuleInternedKey(moduleIdentity);
        this.ContainingAssembly = containingAssembly;
        this.Module = new Module(this, moduleName, peFileReader.COR20Header.COR20Flags, internedModuleId, moduleIdentity);
      }

      this.LoadAssemblyReferences();
      this.LoadModuleReferences();
      this.RootModuleNamespace = new RootNamespace(this);
      this.NamespaceINameHashtable = new Hashtable<Namespace>();
      this.LoadNamespaces();
      this.NamespaceReferenceINameHashtable = new DoubleHashtable<NamespaceReference>();
      this.NamespaceTypeTokenTable = new DoubleHashtable(peFileReader.TypeDefTable.NumberOfRows + peFileReader.ExportedTypeTable.NumberOfRows);
      this.NestedTypeTokenTable = new DoubleHashtable(peFileReader.NestedClassTable.NumberOfRows + peFileReader.ExportedTypeTable.NumberOfRows);
      this.PreLoadTypeDefTableLookup();
      this.ModuleTypeDefArray = new TypeBase/*?*/[peFileReader.TypeDefTable.NumberOfRows + 1];
      this.ModuleTypeDefLoadState = new LoadState[peFileReader.TypeDefTable.NumberOfRows + 1];
      this.RedirectedTypeDefArray = new INamedTypeReference/*?*/[peFileReader.TypeDefTable.NumberOfRows + 1];
      this.ModuleTypeDefLoadState[0] = LoadState.Loaded;

      this.ExportedTypeArray = new ExportedTypeAliasBase/*?*/[peFileReader.ExportedTypeTable.NumberOfRows + 1];
      this.ExportedTypeLoadState = new LoadState[peFileReader.ExportedTypeTable.NumberOfRows + 1];
      this.ExportedTypeLoadState[0] = LoadState.Loaded;

      this.ModuleGenericParamArray = new GenericParameter[peFileReader.GenericParamTable.NumberOfRows + 1];
      if (peFileReader.MethodSpecTable.NumberOfRows > 0) {
        this.ModuleMethodSpecHashtable = new DoubleHashtable<IGenericMethodInstanceReference>(peFileReader.MethodSpecTable.NumberOfRows + 1);
      }

      this.ModuleTypeRefReferenceArray = new INamedTypeReference[peFileReader.TypeRefTable.NumberOfRows + 1];
      this.ModuleTypeRefReferenceLoadState = new LoadState[peFileReader.TypeRefTable.NumberOfRows + 1];
      this.ModuleTypeRefReferenceLoadState[0] = LoadState.Loaded;
      if (peFileReader.TypeSpecTable.NumberOfRows > 0) {
        this.ModuleTypeSpecHashtable = new DoubleHashtable<TypeSpecReference>(peFileReader.TypeSpecTable.NumberOfRows + 1);
      }

      this.ModuleFieldArray = new FieldDefinition[peFileReader.FieldTable.NumberOfRows + 1];
      this.ModuleMethodArray = new IMethodDefinition[peFileReader.MethodTable.NumberOfRows + 1];
      this.ModuleEventArray = new EventDefinition[peFileReader.EventTable.NumberOfRows + 1];
      this.ModulePropertyArray = new PropertyDefinition[peFileReader.PropertyTable.NumberOfRows + 1];

      this.ModuleMemberReferenceArray = new MemberReference/*?*/[peFileReader.MemberRefTable.NumberOfRows + 1];
      this.UnspecializedMemberReferenceArray = new MemberReference/*?*/[peFileReader.MemberRefTable.NumberOfRows + 1];
      this.SpecializedFieldHashtable = new DoubleHashtable<ISpecializedFieldReference>();
      this.SpecializedMethodHashtable = new DoubleHashtable<ISpecializedMethodReference>();

      this.CustomAttributeArray = new ICustomAttribute/*?*/[peFileReader.CustomAttributeTable.NumberOfRows + 1];

      this.DeclSecurityArray = new ISecurityAttribute/*?*/[peFileReader.DeclSecurityTable.NumberOfRows + 1];

      this._Module_ = this.Create_Module_Type();
    }

    /*^
    #pragma warning restore 2666, 2669, 2677, 2674
    ^*/

    internal AssemblyIdentity ContractAssemblySymbolicIdentity {
      get {
        if (this.contractAssemblySymbolicIdentity == null)
          this.contractAssemblySymbolicIdentity = this.GetContractAssemblySymbolicIdentity();
        return this.contractAssemblySymbolicIdentity;
      }
    }
    AssemblyIdentity/*?*/ contractAssemblySymbolicIdentity;

    internal AssemblyIdentity CoreAssemblySymbolicIdentity {
      get {
        if (this.coreAssemblySymbolicIdentity == null)
          this.coreAssemblySymbolicIdentity = this.GetCoreAssemblySymbolicIdentity();
        return this.coreAssemblySymbolicIdentity;
      }
    }
    AssemblyIdentity/*?*/ coreAssemblySymbolicIdentity;

    private AssemblyIdentity GetContractAssemblySymbolicIdentity() {
      lock (GlobalLock.LockingObject) {
        INameTable nameTable = this.NameTable;
        PeReader peReader = this.ModuleReader;
        int systemDiagnosticsContractsKey = nameTable.GetNameFor("System.Diagnostics.Contracts").UniqueKey;
        int codeContractKey = nameTable.GetNameFor("Contract").UniqueKey;
        TypeRefTableReader trTable = this.PEFileReader.TypeRefTable;
        for (uint i = 1; i <= trTable.NumberOfRows; i++) {
          TypeRefRow tr = trTable[i];
          IName nsName = this.GetNameFromOffset(tr.Namespace);
          if (nsName.UniqueKey != systemDiagnosticsContractsKey) continue;
          int tKey = this.GetNameFromOffset(tr.Name).UniqueKey;
          if (tKey != codeContractKey) continue;
          uint resolutionScopeKind = (tr.ResolutionScope & TokenTypeIds.TokenTypeMask);
          if (resolutionScopeKind != TokenTypeIds.AssemblyRef) continue;
          uint resolutionScopeRowId = (tr.ResolutionScope & TokenTypeIds.RIDMask);
          return this.AssemblyReferenceArray[resolutionScopeRowId].AssemblyIdentity;
        }
        TypeDefTableReader tdTable = this.PEFileReader.TypeDefTable;
        for (uint i = 1; i <= tdTable.NumberOfRows; i++) {
          TypeDefRow td = tdTable[i];
          IName nsName = this.GetNameFromOffset(td.Namespace);
          if (nsName.UniqueKey != systemDiagnosticsContractsKey) continue;
          int tKey = this.GetNameFromOffset(td.Name).UniqueKey;
          if (tKey != codeContractKey) continue;
          AssemblyIdentity/*?*/ result = this.Module.ModuleIdentity as AssemblyIdentity;
          if (result != null) return result;
          //TODO: error
          break;
        }
      }
      return Dummy.AssemblyIdentity;
    }

    private AssemblyIdentity GetCoreAssemblySymbolicIdentity() {
      lock (GlobalLock.LockingObject) {
        INameTable nameTable = this.NameTable;
        PeReader peReader = this.ModuleReader;
        int systemKey = nameTable.System.UniqueKey;
        int objectKey = nameTable.Object.UniqueKey;
        int valueTypeKey = nameTable.ValueType.UniqueKey;
        int enumKey = nameTable.Enum.UniqueKey;
        int multicastDelegateKey = nameTable.MulticastDelegate.UniqueKey;
        int arrayKey = nameTable.Array.UniqueKey;
        int attributeKey = nameTable.Attribute.UniqueKey;
        int delegateKey = nameTable.Delegate.UniqueKey;
        int iAsyncResultKey = peReader.IAsyncResult.UniqueKey;
        int iCloneableKey = peReader.ICloneable.UniqueKey;
        int asyncCallbackKey = peReader.AsyncCallback.UniqueKey;
        int attributeUsageAttributeKey = nameTable.AttributeUsageAttribute.UniqueKey;
        int paramArrayAttributeKey = peReader.ParamArrayAttribute.UniqueKey;
        int booleanKey = nameTable.Boolean.UniqueKey;
        int byteKey = nameTable.Byte.UniqueKey;
        int charKey = nameTable.Char.UniqueKey;
        int sByteKey = nameTable.SByte.UniqueKey;
        int int16Key = nameTable.Int16.UniqueKey;
        int uint16Key = nameTable.UInt16.UniqueKey;
        int int32Key = nameTable.Int32.UniqueKey;
        int uint32Key = nameTable.UInt32.UniqueKey;
        int int64Key = nameTable.Int64.UniqueKey;
        int uint64Key = nameTable.UInt64.UniqueKey;
        int stringKey = nameTable.String.UniqueKey;
        int intPtrKey = nameTable.IntPtr.UniqueKey;
        int uintPtrKey = nameTable.UIntPtr.UniqueKey;
        int singleKey = nameTable.Single.UniqueKey;
        int doubleKey = nameTable.Double.UniqueKey;
        int typedReferenceKey = nameTable.TypedReference.UniqueKey;
        int typeKey = nameTable.Type.UniqueKey;
        int dateTimeKey = nameTable.DateTime.UniqueKey;
        int decimalKey = nameTable.Decimal.UniqueKey;
        int dbNullKey = nameTable.DBNull.UniqueKey;
        int runtimeArgumentHandleKey = peReader.RuntimeArgumentHandle.UniqueKey;
        int runtimeFieldHandleKey = peReader.RuntimeFieldHandle.UniqueKey;
        int runtimeMethodHandleKey = peReader.RuntimeMethodHandle.UniqueKey;
        int runtimeTypeHandleKey = peReader.RuntimeTypeHandle.UniqueKey;
        int argIteratorKey = peReader.ArgIterator.UniqueKey;
        int voidKey = nameTable.Void.UniqueKey;
        int mscorlibKey = peReader.Mscorlib.UniqueKey;
        int systemRuntimeKey = peReader.System_Runtime.UniqueKey;
        TypeRefTableReader trTable = this.PEFileReader.TypeRefTable;
        var hitCounts = new uint[trTable.NumberOfRows];
        for (uint i = 1; i <= trTable.NumberOfRows; i++) {
          TypeRefRow tr = trTable[i];
          IName nsName = this.GetNameFromOffset(tr.Namespace);
          if (nsName.UniqueKey != systemKey) continue;
          int tKey = this.GetNameFromOffset(tr.Name).UniqueKey;
          //Look for a lot of different mscorlib types, since an assembly need not reference System.Object or any particular type.
          if (tKey != objectKey && tKey != valueTypeKey && tKey != enumKey && tKey != multicastDelegateKey && tKey != arrayKey &&
            tKey != attributeKey && tKey != delegateKey && tKey != iAsyncResultKey && tKey != iCloneableKey && tKey != asyncCallbackKey &&
            tKey != attributeUsageAttributeKey && tKey != paramArrayAttributeKey && tKey != booleanKey && tKey != byteKey && tKey != charKey &&
            tKey != sByteKey && tKey != int16Key && tKey != uint16Key && tKey != int32Key && tKey != uint32Key && tKey != int64Key &&
            tKey != uint64Key && tKey != stringKey && tKey != intPtrKey && tKey != uintPtrKey && tKey != singleKey && tKey != doubleKey &&
            tKey != typedReferenceKey && tKey != typeKey && tKey != dateTimeKey && tKey != decimalKey && tKey != dbNullKey &&
            tKey != runtimeArgumentHandleKey && tKey != runtimeFieldHandleKey && tKey != runtimeMethodHandleKey &&
            tKey != runtimeTypeHandleKey && tKey != argIteratorKey && tKey != voidKey) continue;
          uint resolutionScopeKind = (tr.ResolutionScope & TokenTypeIds.TokenTypeMask);
          if (resolutionScopeKind != TokenTypeIds.AssemblyRef) continue;
          uint resolutionScopeRowId = (tr.ResolutionScope & TokenTypeIds.RIDMask);
          //Just because this reference looks like a mscorlib type, does not mean that it actually is one. Badly behaved assemblies might reuse mscorlib names.
          //We therefore count the number of references and hope that mscorlib has a majority.
          hitCounts[resolutionScopeRowId-1]++;
          if (tKey == enumKey)
            this.SystemEnumAssembly = this.AssemblyReferenceArray[resolutionScopeRowId];
          else if (tKey == valueTypeKey)
            this.SystemValueTypeAssembly = this.AssemblyReferenceArray[resolutionScopeRowId];
          else if (tKey == multicastDelegateKey)
            this.SystemMulticastDelegateAssembly = this.AssemblyReferenceArray[resolutionScopeRowId];
          else if (tKey == typeKey)
            this.SystemTypeAssembly = this.AssemblyReferenceArray[resolutionScopeRowId];
          else if (tKey == paramArrayAttributeKey)
            this.SystemParamArrayAttributeAssembly = this.AssemblyReferenceArray[resolutionScopeRowId];
        }
        uint maxHits = 0;
        uint rowWithMaxHits = 0;
        for (uint i = 0; i < hitCounts.Length; i++) {
          if (hitCounts[i] > maxHits) {
            maxHits = hitCounts[i];
            rowWithMaxHits = i;
          }
        }
        if (maxHits > 0) {
          return this.AssemblyReferenceArray[rowWithMaxHits+1].AssemblyIdentity;
        }

        TypeDefTableReader tdTable = this.PEFileReader.TypeDefTable;
        for (uint i = 1; i <= tdTable.NumberOfRows; i++) {
          TypeDefRow td = tdTable[i];
          IName nsName = this.GetNameFromOffset(td.Namespace);
          if (nsName.UniqueKey != systemKey) continue;
          int tKey = this.GetNameFromOffset(td.Name).UniqueKey;
          //if you're mscorlib, you have to define System.Object
          if (tKey != objectKey) continue;
          AssemblyIdentity/*?*/ result = this.Module.ModuleIdentity as AssemblyIdentity;
          if (result != null) return result;
          //TODO: error
          break;
        }
        AssemblyRefTableReader arTable = this.PEFileReader.AssemblyRefTable;
        for (uint i = 1; i <= arTable.NumberOfRows; i++) {
          AssemblyRefRow ar = arTable[i];
          int key = this.GetNameFromOffset(ar.Name).UniqueKey;
          if (key != systemRuntimeKey) continue;
          var identity = this.AssemblyReferenceArray[i].AssemblyIdentity;
          if (identity.Version.Major >= 4) return identity;
        }
        for (uint i = 1; i <= arTable.NumberOfRows; i++) {
          AssemblyRefRow ar = arTable[i];
          int key = this.GetNameFromOffset(ar.Name).UniqueKey;
          if (key != mscorlibKey) continue;
          return this.AssemblyReferenceArray[i].AssemblyIdentity;
        }
      }
      return Dummy.AssemblyIdentity;
    }

    //  Caller should lock this
    internal IName GetNameFromOffset(
      uint offset
    ) {
      IName/*?*/ name = this.StringIndexToNameTable.Find(offset);
      if (name == null) {
        string nameStr = this.PEFileReader.StringStream[offset];
        name = this.NameTable.GetNameFor(nameStr);
        this.StringIndexToNameTable.Add(offset, name);
      }
      return name;
    }

    //  Caller should lock this.
    IName GetUnmangledNameFromOffset(
      uint offset
    ) {
      IName/*?*/ name = this.StringIndexToUnmangledNameTable.Find(offset);
      if (name == null) {
        string nameStr = this.PEFileReader.StringStream[offset];
        int indx = nameStr.LastIndexOf('`');
        if (indx != -1) {
          nameStr = nameStr.Substring(0, indx);
        }
        name = this.NameTable.GetNameFor(nameStr);
        this.StringIndexToUnmangledNameTable.Add(offset, name);
      }
      return name;
    }

    #region Assembly/Module level Loading/Convertions

    /// <summary>
    /// The module which this PEFile corresponds to.
    /// </summary>
    internal readonly Module Module;
    /// <summary>
    /// Cache for assembly references. This indexes from row id to assembly reference.
    /// </summary>
    internal AssemblyReference[] AssemblyReferenceArray;
    /// <summary>
    /// The value to return for IModule.AssemblyReferences. May have been modified by the host, so
    /// this is not the same as AssemblyReferenceArray.
    /// </summary>
    internal IEnumerable<IAssemblyReference> AssemblyReferences;
    /// <summary>
    /// Cache for module references. This indexes from row id to module reference.
    /// </summary>
    ModuleReference[] ModuleReferenceArray;

    //  These are loaded fully by calling on the properties.
    internal byte MetadataFormatMajorVersion {
      get {
        return this.PEFileReader.MetadataTableHeader.MajorVersion;
      }
    }

    internal byte MetadataFormatMinorVersion {
      get {
        return this.PEFileReader.MetadataTableHeader.MinorVersion;
      }
    }

    internal Machine Machine {
      get {
        return this.PEFileReader.Machine;
      }
    }

    internal bool RequiresAmdInstructionSet {
      get {
        return this.PEFileReader.RequiresAmdInstructionSet;
      }
    }

    internal bool RequiresStartupStub {
      get {
        return this.PEFileReader.RequiresStartupStub;
      }
    }

    internal bool Requires64Bits {
      get {
        return this.PEFileReader.Requires64Bits;
      }
    }

    internal Guid ModuleGuidIdentifier {
      get {
        return this.PEFileReader.ModuleGuidIdentifier;
      }
    }

    internal ushort SubsystemMajorVersion {
      get { return this.PEFileReader.OptionalHeaderNTAdditionalFields.MajorSubsystemVersion; }
    }

    internal ushort SubsystemMinorVersion {
      get { return this.PEFileReader.OptionalHeaderNTAdditionalFields.MinorSubsystemVersion; }
    }

    internal string TargetRuntimeVersion {
      get {
        return this.PEFileReader.MetadataHeader.VersionString;
      }
    }

    internal ModuleKind ModuleKind {
      get {
        if (this.PEFileReader.IsDll) {
          if (this.PEFileReader.IsUnmanaged) {
            return ModuleKind.UnmanagedDynamicallyLinkedLibrary;
          } else {
            return ModuleKind.DynamicallyLinkedLibrary;
          }
        } else if (this.PEFileReader.IsExe) {
          if (this.PEFileReader.IsConsoleApplication)
            return ModuleKind.ConsoleApplication;
          else
            return ModuleKind.WindowsApplication;
        }
        return ModuleKind.ManifestResourceFile;
      }
    }

    internal IPlatformType PlatformType {
      get { return this.ModuleReader.metadataReaderHost.PlatformType; }
    }

    /// <summary>
    /// Populates the list of assembly references.
    /// </summary>
    void LoadAssemblyReferences()
      //^ ensures this.AssemblyReferenceArray != null;
    {
      uint numberOfAssemblyReferences = this.PEFileReader.AssemblyRefTable.NumberOfRows;
      AssemblyReference[] assemblyRefList = new AssemblyReference[numberOfAssemblyReferences + 1];
      for (uint i = 1; i <= numberOfAssemblyReferences; ++i) {
        AssemblyRefRow assemblyRefRow = this.PEFileReader.AssemblyRefTable[i];
        IName assemblyRefName = this.GetNameFromOffset(assemblyRefRow.Name);
        IName cultureName = this.GetNameFromOffset(assemblyRefRow.Culture);
        Version version = new Version(assemblyRefRow.MajorVersion, assemblyRefRow.MinorVersion, assemblyRefRow.BuildNumber, assemblyRefRow.RevisionNumber);
        byte[] publicKeyTokenArray = TypeCache.EmptyByteArray;
        if (assemblyRefRow.PublicKeyOrToken != 0) {
          var publicKeyOrTokenArray = this.PEFileReader.BlobStream[assemblyRefRow.PublicKeyOrToken];
          if ((assemblyRefRow.Flags & AssemblyFlags.PublicKey) == AssemblyFlags.PublicKey && publicKeyOrTokenArray.Length > 0) {
            publicKeyTokenArray = UnitHelper.ComputePublicKeyToken(publicKeyOrTokenArray);
          } else {
            publicKeyTokenArray = publicKeyOrTokenArray;
          }
          if (publicKeyTokenArray.Length != 8) {
            //  Error
          }
        }
        AssemblyIdentity assemblyIdentity = new AssemblyIdentity(assemblyRefName, cultureName.Value, version, publicKeyTokenArray, string.Empty);
        AssemblyReference assemblyReference = new AssemblyReference(this, i, assemblyIdentity, assemblyRefRow.Flags);
        assemblyRefList[i] = assemblyReference;
      }
      this.AssemblyReferenceArray = assemblyRefList;
    }

    /// <summary>
    /// Populates the list of module references.
    /// </summary>
    void LoadModuleReferences()
      //^ ensures this.ModuleReferenceArray != null;
    {
      uint numberOfModuleReferences = this.PEFileReader.ModuleRefTable.NumberOfRows;
      ModuleReference[] moduleRefList = new ModuleReference[numberOfModuleReferences + 1];
      for (uint i = 1; i <= numberOfModuleReferences; ++i) {
        ModuleRefRow moduleRefRow = this.PEFileReader.ModuleRefTable[i];
        IName moduleRefName = this.GetNameFromOffset(moduleRefRow.Name);
        ModuleIdentity moduleIdentity;
        if (this.ContainingAssembly == null) {
          moduleIdentity = new ModuleIdentity(moduleRefName, string.Empty);
        } else {
          moduleIdentity = new ModuleIdentity(moduleRefName, string.Empty, this.ContainingAssembly.AssemblyIdentity);
        }
        ModuleIdentity probedModuleIdentity = this.ModuleReader.metadataReaderHost.ProbeModuleReference(this.Module, moduleIdentity);
        uint internedModuleId = (uint)this.ModuleReader.metadataReaderHost.InternFactory.GetModuleInternedKey(probedModuleIdentity);
        ModuleReference moduleReference = new ModuleReference(this, i, internedModuleId, probedModuleIdentity);
        moduleRefList[i] = moduleReference;
      }
      //^ NonNullType.AssertInitialized(moduleRefList);
      this.ModuleReferenceArray = moduleRefList;
    }

    internal AssemblyReference/*?*/ GetAssemblyReferenceAt(
      uint assemRefRowId
    ) {
      if (assemRefRowId < this.AssemblyReferenceArray.Length) {
        return this.AssemblyReferenceArray[assemRefRowId];
      }
      return null;
    }

    internal ModuleReference/*?*/ GetModuleReferenceAt(
      uint moduleRefRowId
    ) {
      if (moduleRefRowId < this.ModuleReferenceArray.Length) {
        return this.ModuleReferenceArray[moduleRefRowId];
      }
      return null;
    }

    internal IEnumerable<IAssemblyReference> GetAssemblyReferences() {
      if (this.AssemblyReferences == null) {
        var numberOfAssemblyReferences = this.AssemblyReferenceArray.Length-1;
        var arefArray = new IAssemblyReference[numberOfAssemblyReferences];
        for (int i = 0; i < numberOfAssemblyReferences; i++) arefArray[i] = this.AssemblyReferenceArray[i+1];
        this.AssemblyReferences = this.ModuleReader.metadataReaderHost.Redirect(this.Module, IteratorHelper.GetReadonly(arefArray));
      }
      return this.AssemblyReferences;
    }

    internal IEnumerable<IModuleReference> GetModuleReferences() {
      ModuleReference[] moduleRefList = this.ModuleReferenceArray;
      for (int i = 1; i < moduleRefList.Length; ++i) {
        yield return moduleRefList[i];
      }
    }

    //  Caller should lock this.
    internal Assembly/*?*/ ResolveAssemblyRefReference(
      AssemblyReference assemblyReference
    ) {
      Assembly/*?*/ assem = this.ModuleReader.LookupAssembly(this.Module, assemblyReference.UnifiedAssemblyIdentity);
      return assem;
    }

    //  Caller should lock this.
    internal IModule/*?*/ ResolveModuleRefReference(ModuleReference moduleReference) {
      //  If this is an assembly try to find in referred modules
      if (this.PEFileReader.IsAssembly) {
        Assembly/*?*/ assem = this.Module as Assembly;
        //^ assert assem != null;
        int moduleKey = moduleReference.ModuleIdentity.Name.UniqueKeyIgnoringCase;
        IModule[] containigModules = assem.MemberModules;
        var n = containigModules == null ? 0 : containigModules.Length;
        for (int i = 0; i < n; i++) {
          IModule mod = containigModules[i];
          if (mod.ModuleName.UniqueKeyIgnoringCase == moduleKey) return mod;
        }
      }
      //  If not found or its not an assembly look else where...
      var module = this.ModuleReader.LookupModule(this.Module, moduleReference.ModuleIdentity);
      return module;
    }

    /// <summary>
    /// Finds the assembly ref token corresponding to the given assembly identifier.
    /// </summary>
    /// <param name="assemblyIdentity"></param>
    /// <returns></returns>
    internal AssemblyReference/*?*/ FindAssemblyReference(AssemblyIdentity assemblyIdentity) {
      uint assemblyInternedId = (uint)this.ModuleReader.metadataReaderHost.InternFactory.GetAssemblyInternedKey(assemblyIdentity);
      AssemblyReference[] assemblyRefList = this.AssemblyReferenceArray;
      int numberOfAssemblyReferences = assemblyRefList.Length;
      for (int i = 1; i < numberOfAssemblyReferences; ++i) {
        AssemblyReference assemblyRef = assemblyRefList[i];
        uint refInternedId = (uint)this.ModuleReader.metadataReaderHost.InternFactory.GetAssemblyInternedKey(assemblyRef.AssemblyIdentity);
        if (refInternedId == assemblyInternedId) {
          return assemblyRef;
        }
      }
      return null;
    }

    #endregion Assembly/Module level Loading/Convertions

    #region Assembly/Module Level Information

    /// <summary>
    /// Cache for file referneces of the assembly.
    /// </summary>
    FileReference[]/*?*/ FileReferenceArray;

    /// <summary>
    /// Cache for Resource referneces of the assembly.
    /// </summary>
    ResourceReference[]/*?*/ ResourceReferenceArray;

    internal IEnumerable<IPESection> GetUninterpretedPESections() {
      for (int i = 0, n = this.PEFileReader.SectionHeaders.Length; i < n; i++) {
        var nameStr = this.PEFileReader.SectionHeaders[i].Name;
        switch (nameStr) {
          case ".text":
          case ".sdata":
          case ".tls":
          case ".rdata":
          case ".cover":
          case ".rsrc":
          case ".reloc": continue;
        }
        var name = this.ModuleReader.metadataReaderHost.NameTable.GetNameFor(nameStr);
        yield return new PESection(this.PEFileReader.SectionHeaders, i, name, this);
      }
    }

    internal string GetWin32ResourceName(
      int idOrName
    ) {
      if (idOrName >= 0)
        return string.Empty;
      int numBytes;
      return this.PEFileReader.Win32ResourceMemoryReader.PeekUTF16WithShortSize(idOrName & 0x7FFFFFFF, out numBytes);
    }

    internal EnumerableMemoryBlockWrapper GetWin32ResourceBytes(
      int dataRVA,
      int size
    ) {
      return new EnumerableMemoryBlockWrapper(this.PEFileReader.RVAToMemoryBlockWithSize(dataRVA, size));
    }

    internal EnumerableMemoryBlockWrapper GetWin32ResourceBytes(
      uint dataRVA,
      uint size
    ) {
      return new EnumerableMemoryBlockWrapper(this.PEFileReader.RVAToMemoryBlockWithSize((int)dataRVA, (int)size));
    }

    internal IEnumerable<IWin32Resource> GetWin32Resources() {
      if (this.PEFileReader.Win32ResourceMemoryReader.Length <= 0)
        yield break;
      int typeOffset = 0;
      ResourceDirectory rootResourceDirectory = this.PEFileReader.GetResourceDirectoryAt(typeOffset);
      typeOffset += PEFileConstants.SizeofResourceDirectory;
      int totalTypeEntries = rootResourceDirectory.NumberOfNamedEntries + rootResourceDirectory.NumberOfIdEntries;
      for (int typeIter = 0; typeIter < totalTypeEntries; ++typeIter) {
        ResourceDirectoryEntry typeResourceDirEntry = this.PEFileReader.GetResourceDirectoryEntryAt(typeOffset);
        typeOffset += PEFileConstants.SizeofResourceDirectoryEntry;
        if (!typeResourceDirEntry.IsDirectory) {
          //  TODO: MD Error
        }
        int typeIdOrName = typeResourceDirEntry.NameOrId;
        int nameOffset = typeResourceDirEntry.OffsetToDirectory;
        ResourceDirectory typeResourceDirectory = this.PEFileReader.GetResourceDirectoryAt(nameOffset);
        nameOffset += PEFileConstants.SizeofResourceDirectory;
        int totalNameEntries = typeResourceDirectory.NumberOfNamedEntries + typeResourceDirectory.NumberOfIdEntries;
        for (int nameIter = 0; nameIter < totalNameEntries; ++nameIter) {
          ResourceDirectoryEntry nameResourceDirEntry = this.PEFileReader.GetResourceDirectoryEntryAt(nameOffset);
          nameOffset += PEFileConstants.SizeofResourceDirectoryEntry;
          if (!nameResourceDirEntry.IsDirectory) {
            //  TODO: MD Error
          }
          int idOrName = nameResourceDirEntry.NameOrId;
          int langOffset = nameResourceDirEntry.OffsetToDirectory;
          ResourceDirectory nameResourceDirectory = this.PEFileReader.GetResourceDirectoryAt(langOffset);
          langOffset += PEFileConstants.SizeofResourceDirectory;
          int totalLangEntries = nameResourceDirectory.NumberOfNamedEntries + nameResourceDirectory.NumberOfIdEntries;
          for (int langIter = 0; langIter < totalLangEntries; ++langIter) {
            ResourceDirectoryEntry langResourceDirEntry = this.PEFileReader.GetResourceDirectoryEntryAt(langOffset);
            langOffset += PEFileConstants.SizeofResourceDirectoryEntry;
            if (langResourceDirEntry.IsDirectory) {
              //  TODO: MD Error
            }
            int langIdOrName = langResourceDirEntry.NameOrId;
            ResourceDataEntry resourceData = this.PEFileReader.GetResourceDataEntryAt(langResourceDirEntry.OffsetToData);
            yield return new Win32Resource(this, typeIdOrName, idOrName, langIdOrName, resourceData.RVAToData, (uint)resourceData.Size, (uint)resourceData.CodePage);
          }
        }
      }
    }

    internal EnumerableMemoryBlockWrapper GetFileHash(
      uint fileRowId
    ) {
      uint blobOffset = this.PEFileReader.FileTable.GetHashValue(fileRowId);
      return new EnumerableMemoryBlockWrapper(this.PEFileReader.BlobStream.GetMemoryBlockAt(blobOffset));
    }

    /// <summary>
    /// Populates the File reference cache.
    /// </summary>
    void InitFileReferenceArray()
      //^ ensures this.FileReferenceArray != null;
    {
      lock (GlobalLock.LockingObject) {
        if (this.FileReferenceArray == null) {
          uint num = this.PEFileReader.FileTable.NumberOfRows;
          FileReference[] fileReferenceArray = new FileReference[num+1];
          for (uint i = 1; i <= num; ++i) {
            FileRow fileRow = this.PEFileReader.FileTable[i];
            IName name = this.GetNameFromOffset(fileRow.Name);
            fileReferenceArray[i] = new FileReference(this, i, fileRow.Flags, name);
          }
          this.FileReferenceArray = fileReferenceArray;
        }
      }
    }

    internal FileReference/*?*/ GetFileReferenceAt(
      uint fileRefRowId
    ) {
      if (this.FileReferenceArray == null) {
        this.InitFileReferenceArray();
      }
      //^ assert this.FileReferenceArray != null;
      if (fileRefRowId < this.FileReferenceArray.Length) {
        return this.FileReferenceArray[fileRefRowId];
      }
      return null;
    }

    internal IEnumerable<IFileReference> GetFiles() {
      if (this.FileReferenceArray == null) {
        this.InitFileReferenceArray();
      }
      //^ assert this.FileReferenceArray != null;
      FileReference[] fileRefList = this.FileReferenceArray;
      for (int i = 1; i < fileRefList.Length; ++i) {
        yield return fileRefList[i];
      }
    }

    /// <summary>
    /// Populates the Resource reference cache
    /// </summary>
    void InitResourceReferenceArray()
      //^ ensures this.ResourceReferenceArray != null;
    {
      lock (GlobalLock.LockingObject) {
        if (this.ResourceReferenceArray == null) {
          uint num = this.PEFileReader.ManifestResourceTable.NumberOfRows;
          ResourceReference[] resourceReferenceArray = new ResourceReference[num + 1];
          for (uint i = 1; i <= num; ++i) {
            ManifestResourceRow resRow = this.PEFileReader.ManifestResourceTable[i];
            IName name = this.GetNameFromOffset(resRow.Name);
            uint tokenType = resRow.Implementation & TokenTypeIds.TokenTypeMask;
            IAssemblyReference defAssemRef = Dummy.AssemblyReference;
            if (tokenType == TokenTypeIds.File || resRow.Implementation == 0) {
              resourceReferenceArray[i] = new Resource(this, i, name, resRow.Flags, tokenType == TokenTypeIds.File);
            } else if (tokenType == TokenTypeIds.AssemblyRef) {
              AssemblyReference/*?*/ assemblyRef = this.GetAssemblyReferenceAt(resRow.Implementation & TokenTypeIds.RIDMask);
              if (assemblyRef == null) {
                //  MDError
              } else {
                resourceReferenceArray[i] = new ResourceReference(this, i, assemblyRef, resRow.Flags, name);
              }
            } else {
              //  MDError
            }
          }
          this.ResourceReferenceArray = resourceReferenceArray;
        }
      }
    }

    internal ResourceReference/*?*/ LookupResourceReference(
      IName name
    ) {
      if (this.ResourceReferenceArray == null) {
        this.InitResourceReferenceArray();
      }
      //^ assert this.ResourceReferenceArray != null;
      int uniKey = name.UniqueKey;
      int num = this.ResourceReferenceArray.Length;
      for (int i = 1; i <= num; ++i) {
        if (this.ResourceReferenceArray[i].Name.UniqueKey == uniKey) {
          return this.ResourceReferenceArray[i];
        }
      }
      return null;
    }

    internal IResource ResolveResource(
      ResourceReference resourceReference,
      ResourceReference originalReference
    ) {
      ManifestResourceRow resRow = this.PEFileReader.ManifestResourceTable[resourceReference.ResourceRowId];
      uint tokenType = resRow.Implementation & TokenTypeIds.TokenTypeMask;
      if (tokenType == TokenTypeIds.AssemblyRef) {
        AssemblyReference/*?*/ assemblyReference = this.GetAssemblyReferenceAt(resRow.Implementation & TokenTypeIds.RIDMask);
        if (assemblyReference == null) {
          //  MDError
          return Dummy.Resource;
        }
        var assembly = assemblyReference.ResolvedAssembly as Assembly;
        if (assembly == null) {
          //  MDError
          return Dummy.Resource;
        }
        ResourceReference/*?*/ resRef = assembly.PEFileToObjectModel.LookupResourceReference(resourceReference.Name);
        if (resRef == originalReference) {
          //  MDError
          return Dummy.Resource;
        }
        if (resRef == null)
          return Dummy.Resource;
        return assembly.PEFileToObjectModel.ResolveResource(resRef, originalReference);
      } else {
        return new Resource(this, resourceReference.ResourceRowId, resourceReference.Name, resRow.Flags, (resRow.Implementation & TokenTypeIds.RIDMask) != 0);
      }
    }

    internal IFileReference GetExternalFileForResource(
      uint resourceRowId
    ) {
      uint implementation = this.PEFileReader.ManifestResourceTable.GetImplementation(resourceRowId);
      if ((implementation & TokenTypeIds.File) == TokenTypeIds.File) {
        uint fileRowId = implementation & TokenTypeIds.RIDMask;
        if (fileRowId != 0) {
          FileReference/*?*/ fileRef = this.GetFileReferenceAt(fileRowId);
          if (fileRef == null) {
            //  MDError
            return Dummy.FileReference;
          }
          return fileRef;
        }
      }
      return Dummy.FileReference;
    }

    internal IEnumerable<byte> GetResourceData(
      Resource resource
    ) {
      if (resource.IsInExternalFile) {
        IBinaryDocumentMemoryBlock/*?*/ binaryDocumentMemoryBlock = this.ModuleReader.metadataReaderHost.OpenBinaryDocument(this.PEFileReader.BinaryDocumentMemoryBlock.BinaryDocument, resource.ExternalFile.FileName.Value);
        if (binaryDocumentMemoryBlock == null) {
          //  Error. File not present
          return TypeCache.EmptyByteArray;
        }
        return new EnumerableBinaryDocumentMemoryBlockWrapper(binaryDocumentMemoryBlock);
      } else {
        uint resOffset = this.PEFileReader.ManifestResourceTable.GetOffset(resource.ResourceRowId);
        if (this.PEFileReader.ResourceMemoryReader.Length < resOffset + 4) {
          //  MDError:
          return TypeCache.EmptyByteArray;
        }
        uint len = this.PEFileReader.ResourceMemoryReader.PeekUInt32((int)resOffset);
        if (this.PEFileReader.ResourceMemoryReader.Length < resOffset + 4 + len) {
          //  MDError:
          return TypeCache.EmptyByteArray;
        }
        return new EnumerableMemoryBlockWrapper(this.PEFileReader.ResourceMemoryReader.GetMemoryBlockAt(resOffset + sizeof(Int32), len));
      }
    }

    internal IEnumerable<IResourceReference> GetResources() {
      if (this.ResourceReferenceArray == null) {
        this.InitResourceReferenceArray();
      }
      //^ assert this.ResourceReferenceArray != null;
      ResourceReference[] resRefArr = this.ResourceReferenceArray;
      for (int i = 1; i < resRefArr.Length; ++i) {
        yield return resRefArr[i];
      }
    }

    internal string GetDebugInformationLocation() {
      return this.PEFileReader.DebugInformation.PdbFileName;
    }

    internal string GetDebugInformationVersion() {
      return this.PEFileReader.DebugInformation.PdbVersion;
    }

    internal DllCharacteristics GetDllCharacteristics() {
      return this.PEFileReader.DllCharacteristics;
    }

    internal IMethodReference GetEntryPointMethod() {
      //  TODO: Do we ever want to take care of Native methods?
      if ((this.PEFileReader.COR20Header.COR20Flags & COR20Flags.Prefers32bits) != 0)
        return Dummy.MethodReference;
      uint tokenType = this.PEFileReader.COR20Header.EntryPointTokenOrRVA & TokenTypeIds.TokenTypeMask;
      if (tokenType == TokenTypeIds.File) {
        Assembly/*?*/ assem = this.Module as Assembly;
        FileReference/*?*/ file = this.GetFileReferenceAt(this.PEFileReader.COR20Header.EntryPointTokenOrRVA & TokenTypeIds.RIDMask);
        if (assem == null || file == null)
          return Dummy.MethodReference;
        var mod = assem.FindMemberModuleNamed(file.Name) as Module;
        if (mod == null)
          return Dummy.MethodReference;
        return mod.PEFileToObjectModel.GetEntryPointMethod();
      } else {
        return this.GetMethodReferenceForToken(this.Module, this.PEFileReader.COR20Header.EntryPointTokenOrRVA);
      }
    }

    #endregion Assembly/Module Level Information

    #region Namespace Level Loading/Convertions
    internal readonly RootNamespace RootModuleNamespace;
    readonly Hashtable<Namespace> NamespaceINameHashtable;
    readonly DoubleHashtable<NamespaceReference> NamespaceReferenceINameHashtable;

    //  Loaded by the constructor when this object is created
    Namespace GetNamespaceForString(
      string namespaceFullName
    ) {
      IName iNamespaceFullName = this.NameTable.GetNameFor(namespaceFullName);
      Namespace/*?*/ retNamespace = this.NamespaceINameHashtable.Find((uint)iNamespaceFullName.UniqueKey);
      if (retNamespace != null)
        return retNamespace;
      int lastDot = namespaceFullName.LastIndexOf('.');
      if (lastDot == -1) {
        NestedNamespace nestedNamespace = new NestedNamespace(this, iNamespaceFullName, iNamespaceFullName, this.RootModuleNamespace);
        this.RootModuleNamespace.AddMember(nestedNamespace);
        retNamespace = nestedNamespace;
      } else {
        string namespacePrefix = namespaceFullName.Substring(0, lastDot);
        Namespace parentNamespace = this.GetNamespaceForString(namespacePrefix);
        string namespaceName = namespaceFullName.Substring(lastDot + 1, namespaceFullName.Length - lastDot - 1);
        IName iNamespaceName = this.NameTable.GetNameFor(namespaceName);
        NestedNamespace nestedNamespace = new NestedNamespace(this, iNamespaceName, iNamespaceFullName, parentNamespace);
        parentNamespace.AddMember(nestedNamespace);
        retNamespace = nestedNamespace;
      }
      this.NamespaceINameHashtable.Add((uint)iNamespaceFullName.UniqueKey, retNamespace);
      return retNamespace;
    }
    void BuildNamespaceForNameOffset(
      uint namespaceNameOffset,
      Hashtable<Namespace> namespaceOffsetHashtable
    ) {
      if (namespaceNameOffset == 0 || namespaceOffsetHashtable.Find(namespaceNameOffset) != null)
        return;
      string namespaceNameStr = this.PEFileReader.StringStream[namespaceNameOffset];
      Namespace moduleNamespace = this.GetNamespaceForString(namespaceNameStr);
      moduleNamespace.SetNamespaceNameOffset(namespaceNameOffset);
      namespaceOffsetHashtable.Add(namespaceNameOffset, moduleNamespace);
    }
    void LoadNamespaces()
      //^ requires this.RootModuleNamespace != null;
      //^ requires this.NamespaceINameHashtable != null;
    {
      this.NamespaceINameHashtable.Add((uint)NameTable.EmptyName.UniqueKey, this.RootModuleNamespace);
      Hashtable<Namespace> namespaceOffsetHashtable = new Hashtable<Namespace>();
      namespaceOffsetHashtable.Add(0, this.RootModuleNamespace);
      uint numberOfTypeDefs = this.PEFileReader.TypeDefTable.NumberOfRows;
      for (uint i = 1; i <= numberOfTypeDefs; ++i) {
        uint namespaceName = this.PEFileReader.TypeDefTable.GetNamespace(i);
        this.BuildNamespaceForNameOffset(namespaceName, namespaceOffsetHashtable);
      }
      uint numberOfExportedTypes = this.PEFileReader.ExportedTypeTable.NumberOfRows;
      for (uint i = 1; i <= numberOfExportedTypes; ++i) {
        uint namespaceName = this.PEFileReader.ExportedTypeTable.GetNamespace(i);
        this.BuildNamespaceForNameOffset(namespaceName, namespaceOffsetHashtable);
      }
    }
    Namespace GetNamespaceForNameOffset(
      uint namespaceNameOffset
    ) {
      IName iNamespaceFullName = this.GetNameFromOffset(namespaceNameOffset);
      Namespace/*?*/ retNamespace = this.NamespaceINameHashtable.Find((uint)iNamespaceFullName.UniqueKey);
      //^ assert retNamespace != null;  //  because this is called for def/exported types which have it prepopulated.
      return retNamespace;
    }
    internal NamespaceReference GetNamespaceReferenceForString(
      IMetadataReaderModuleReference moduleReference,
      IName iNamespaceFullName
    ) {
      NamespaceReference/*?*/ retNamespaceReference = this.NamespaceReferenceINameHashtable.Find(moduleReference.InternedModuleId, (uint)iNamespaceFullName.UniqueKey);
      if (retNamespaceReference != null)
        return retNamespaceReference;
      string namespaceFullName = iNamespaceFullName.Value;
      if (namespaceFullName.Length == 0) {
        retNamespaceReference = new RootNamespaceReference(this, moduleReference);
      } else {
        int lastDot = namespaceFullName.LastIndexOf('.');
        string namespaceName = namespaceFullName;
        string namespacePrefix = "";
        if (lastDot != -1) {
          namespacePrefix = namespaceFullName.Substring(0, lastDot);
          namespaceName = namespaceFullName.Substring(lastDot + 1, namespaceFullName.Length - lastDot - 1);
        }
        IName iNamespacePrefix = this.NameTable.GetNameFor(namespacePrefix);
        NamespaceReference parentNamespaceReference = this.GetNamespaceReferenceForString(moduleReference, iNamespacePrefix);
        IName iNamespaceName = this.NameTable.GetNameFor(namespaceName);
        retNamespaceReference = new NestedNamespaceReference(this, iNamespaceName, iNamespaceFullName, parentNamespaceReference);
      }
      this.NamespaceReferenceINameHashtable.Add(moduleReference.InternedModuleId, (uint)iNamespaceFullName.UniqueKey, retNamespaceReference);
      return retNamespaceReference;
    }

    #endregion Namespace Level Loading/Convertions

    #region Type Definition Level Loading/Conversions
    //  <NSName, TypeName> -> TypeDefToken/ExportedTypeToken
    readonly DoubleHashtable NamespaceTypeTokenTable;
    //  <ParentTypeToken(Def/Exported), TypeName> -> TypeDefRowId/ExportedTypeToken
    readonly DoubleHashtable NestedTypeTokenTable;
    readonly TypeBase/*?*/[] ModuleTypeDefArray;
    readonly INamedTypeReference/*?*/[] RedirectedTypeDefArray;
    readonly LoadState[] ModuleTypeDefLoadState;
    readonly ExportedTypeAliasBase/*?*/[] ExportedTypeArray;
    readonly LoadState[] ExportedTypeLoadState;
    //^ invariant this.ModuleTypeDefArray.Length == this.ModuleTypeDefLoadState.Length;
    // ^ invariant forall{int i in (0:this.ModuleTypeDefArray.Length-1); this.ModuleTypeDefLoadState[i] == LoadState.Loaded ==> this.ModuleTypeDefArray[i] != null};

    void PreLoadTypeDefTableLookup() {
      uint numberOfTypeDefs = this.PEFileReader.TypeDefTable.NumberOfRows;
      for (uint i = 1; i <= numberOfTypeDefs; ++i) {
        TypeDefRow typeDefRow = this.PEFileReader.TypeDefTable[i];
        if (!typeDefRow.IsNested) {
          IName namespaceName = this.GetNameFromOffset(typeDefRow.Namespace);
          IName typeName = this.GetNameFromOffset(typeDefRow.Name);
          this.NamespaceTypeTokenTable.Add((uint)namespaceName.UniqueKey, (uint)typeName.UniqueKey, TokenTypeIds.TypeDef | i);
        }
      }
      uint numberOfNestedTypes = this.PEFileReader.NestedClassTable.NumberOfRows;
      for (uint i = 1; i <= numberOfNestedTypes; ++i) {
        NestedClassRow nestedClassRow = this.PEFileReader.NestedClassTable[i];
        uint nameStrId = this.PEFileReader.TypeDefTable.GetName(nestedClassRow.NestedClass);
        IName typeName = this.GetNameFromOffset(nameStrId);
        this.NestedTypeTokenTable.Add(TokenTypeIds.TypeDef | nestedClassRow.EnclosingClass, (uint)typeName.UniqueKey, TokenTypeIds.TypeDef | nestedClassRow.NestedClass);
      }
      uint numberOfExportedTypes = this.PEFileReader.ExportedTypeTable.NumberOfRows;
      for (uint i = 1; i <= numberOfExportedTypes; ++i) {
        ExportedTypeRow expTypeRow = this.PEFileReader.ExportedTypeTable[i];
        IName typeName = this.GetNameFromOffset(expTypeRow.TypeName);
        if (expTypeRow.IsNested) {
          this.NestedTypeTokenTable.Add(expTypeRow.Implementation, (uint)typeName.UniqueKey, TokenTypeIds.ExportedType | i);
        } else {
          IName namespaceName = this.GetNameFromOffset(expTypeRow.TypeNamespace);
          this.NamespaceTypeTokenTable.Add((uint)namespaceName.UniqueKey, (uint)typeName.UniqueKey, TokenTypeIds.ExportedType | i);
        }
      }
    }

    NamespaceType CreateModuleNamespaceType(
      uint typeDefRowId,
      TypeDefRow typeDefRow,
      Namespace moduleNamespace,
      MetadataReaderSignatureTypeCode signatureTypeCode
    ) {
      IName typeName = this.GetNameFromOffset(typeDefRow.Name);
      uint genericParamRowIdStart;
      uint genericParamRowIdEnd;
      this.GetGenericParamInfoForType(typeDefRowId, out genericParamRowIdStart, out genericParamRowIdEnd);
      NamespaceType type;
      if (genericParamRowIdStart == 0) {
        if (signatureTypeCode == MetadataReaderSignatureTypeCode.NotModulePrimitive)
          type = new NonGenericNamespaceTypeWithoutPrimitiveType(this, typeName, typeDefRowId, typeDefRow.Flags, moduleNamespace);
        else
          type = new NonGenericNamespaceTypeWithPrimitiveType(this, typeName, typeDefRowId, typeDefRow.Flags, moduleNamespace, signatureTypeCode);
      } else {
        IName unmangledTypeName = this.GetUnmangledNameFromOffset(typeDefRow.Name);
        type = new GenericNamespaceType(this, unmangledTypeName, typeDefRowId, typeDefRow.Flags, moduleNamespace, typeName, genericParamRowIdStart, genericParamRowIdEnd);
      }
      return type;
    }

    NestedType CreateModuleNestedType(
      uint typeDefRowId,
      TypeDefRow typeDefRow,
      TypeBase parentModuleType
    ) {
      IName typeName = this.GetNameFromOffset(typeDefRow.Name);
      if (typeDefRow.Namespace != 0) {
        IName namespaceName = this.GetNameFromOffset(typeDefRow.Namespace);
        typeName = this.NameTable.GetNameFor(namespaceName.Value+"."+typeName.Value);
      }
      uint genericParamRowIdStart;
      uint genericParamRowIdEnd;
      this.GetGenericParamInfoForType(typeDefRowId, out genericParamRowIdStart, out genericParamRowIdEnd);
      NestedType type;
      if (genericParamRowIdStart == 0) {
        type = new NonGenericNestedType(this, typeName, typeDefRowId, typeDefRow.Flags, parentModuleType);
      } else {
        IName unmangledTypeName = this.GetUnmangledNameFromOffset(typeDefRow.Name);
        if (typeDefRow.Namespace != 0) {
          IName namespaceName = this.GetNameFromOffset(typeDefRow.Namespace);
          unmangledTypeName = this.NameTable.GetNameFor(namespaceName.Value+"."+unmangledTypeName.Value);
        }
        type = new GenericNestedType(this, unmangledTypeName, typeDefRowId, typeDefRow.Flags, parentModuleType, typeName, genericParamRowIdStart, genericParamRowIdEnd);
      }
      return type;
    }

    ExportedTypeNamespaceAlias CreateExportedNamespaceType(uint exportedTypeRowId, ExportedTypeRow exportedTypeRow, Namespace moduleNamespace) {
      ExportedTypeNamespaceAlias exportedType = new ExportedTypeNamespaceAlias(this, exportedTypeRowId, exportedTypeRow.Flags, moduleNamespace);
      moduleNamespace.AddMember(exportedType);
      return exportedType;
    }

    ExportedTypeNestedAlias CreateExportedNestedType(uint exportedTypeRowId, ExportedTypeRow exportedTypeRow, ExportedTypeAliasBase parentExportedType) {
      ExportedTypeNestedAlias exportedType = new ExportedTypeNestedAlias(this, exportedTypeRowId, exportedTypeRow.Flags, parentExportedType);
      parentExportedType.AddMember(exportedType);
      return exportedType;
    }

    //  Caller must take lock on this
    internal void LoadTypesInNamespace(
      Namespace moduleNamespace
    )
      //^ requires moduleNamespace.NamespaceNameOffset != 0xFFFFFFFF;
    {
      CoreTypes dummy = this.CoreTypes; //force core types to be initialized
      uint namespaceNameOffset = moduleNamespace.NamespaceNameOffset;
      Debug.Assert(namespaceNameOffset != 0xFFFFFFFF);
      uint numberOfTypeDefs = this.PEFileReader.TypeDefTable.NumberOfRows;
      var moduleTypeArray = this.ModuleTypeDefArray;
      for (uint i = 1; i <= numberOfTypeDefs; ++i) {
        TypeDefRow typeDefRow = this.PEFileReader.TypeDefTable[i];
        //  Check if its in the same namespace
        if (typeDefRow.Namespace != namespaceNameOffset || typeDefRow.IsNested)
          continue;
        //  Check if its already created by someone else
        NamespaceType/*?*/ type = (NamespaceType)moduleTypeArray[i];
        if (type == null) {
          type = this.CreateModuleNamespaceType(i, typeDefRow, moduleNamespace, MetadataReaderSignatureTypeCode.NotModulePrimitive);
          var redirectedType = this.ModuleReader.metadataReaderHost.Redirect(this.Module, type);
          if (redirectedType == type) moduleNamespace.AddMember(type);
          moduleTypeArray[i] = type;
          this.RedirectedTypeDefArray[i] = redirectedType;
          this.ModuleTypeDefLoadState[i] = LoadState.Loaded;
        }
      }
      uint numberOfExportedTypes = this.PEFileReader.ExportedTypeTable.NumberOfRows;
      ExportedTypeAliasBase/*?*/[] exportedTypeArray = this.ExportedTypeArray;
      for (uint i = 1; i <= numberOfExportedTypes; ++i) {
        ExportedTypeRow exportedTypeRow = this.PEFileReader.ExportedTypeTable[i];
        //  Check if its in the same namespace
        if (exportedTypeRow.TypeNamespace != namespaceNameOffset || exportedTypeRow.IsNested)
          continue;
        //  Check if its already created by someone else
        ExportedTypeAliasBase/*?*/ type = exportedTypeArray[i];
        if (type == null) {
          type = this.CreateExportedNamespaceType(i, exportedTypeRow, moduleNamespace);
          this.ExportedTypeArray[i] = type;
          this.ExportedTypeLoadState[i] = LoadState.Loaded;
        }
      }
    }

    internal NamespaceType GetPredefinedTypeDefinitionAtRowWorker(
      uint typeDefRowId,
      MetadataReaderSignatureTypeCode signatureTypeCode
    ) {
      if (this.ModuleTypeDefLoadState[typeDefRowId] == LoadState.Uninitialized) {
        Debug.Assert(this.ModuleTypeDefArray[typeDefRowId] == null);
        TypeDefRow typeDefRow = this.PEFileReader.TypeDefTable[typeDefRowId];
        Debug.Assert(!typeDefRow.IsNested); //  TODO: MD Error if this happens
        Namespace parentNamespace = this.GetNamespaceForNameOffset(typeDefRow.Namespace);
        var type = this.CreateModuleNamespaceType(typeDefRowId, typeDefRow, parentNamespace, signatureTypeCode);
        parentNamespace.AddMember(type);
        this.ModuleTypeDefArray[typeDefRowId] = type;
        this.RedirectedTypeDefArray[typeDefRowId] = type;
        this.ModuleTypeDefLoadState[typeDefRowId] = LoadState.Loaded;
      }
      return (NamespaceType)this.ModuleTypeDefArray[typeDefRowId];
    }

    internal _Module_Type/*?*/ Create_Module_Type()
      //^ ensures this.ModuleTypeDefLoadState[typeDefRowId] == LoadState.Loaded;
    {
      this.ModuleTypeDefLoadState[1] = LoadState.Loading;
      TypeDefRow typeDefRow = this.PEFileReader.TypeDefTable[1];
      if (typeDefRow.IsNested) {
        //TODO: error
      }
      var nspace = this.GetNamespaceForNameOffset(typeDefRow.Namespace);
      var typeName = this.GetNameFromOffset(typeDefRow.Name);
      _Module_Type moduleType = new _Module_Type(this, typeName, 1, typeDefRow.Flags, nspace);
      this.ModuleTypeDefArray[1] = moduleType;
      this.RedirectedTypeDefArray[1] = moduleType;
      this.ModuleTypeDefLoadState[1] = LoadState.Loaded;
      return moduleType;
    }

    internal TypeBase/*?*/ GetTypeDefinitionAtRowWorker(
      uint typeDefRowId
    )
      //^ ensures this.ModuleTypeDefLoadState[typeDefRowId] == LoadState.Loaded;
    {
      if (typeDefRowId == 0 || typeDefRowId > this.PEFileReader.TypeDefTable.NumberOfRows) {
        return null;
      }
      if (this.ModuleTypeDefLoadState[typeDefRowId] == LoadState.Uninitialized) {
        CoreTypes dummy = this.CoreTypes; //force core types to be initialized
        var cachedCopy = this.ModuleTypeDefArray[typeDefRowId];
        if (cachedCopy != null) return cachedCopy;
        this.ModuleTypeDefLoadState[typeDefRowId] = LoadState.Loading;
        TypeDefRow typeDefRow = this.PEFileReader.TypeDefTable[typeDefRowId];
        if (typeDefRow.IsNested) {
          uint parentTypeRowId = this.PEFileReader.NestedClassTable.FindParentTypeDefRowId(typeDefRowId);
          var parentModuleType = this.GetTypeDefinitionAtRowWorker(parentTypeRowId);
          if (parentModuleType != null) {
            var type = this.CreateModuleNestedType(typeDefRowId, typeDefRow, parentModuleType);
            var redirectedType = this.ModuleReader.metadataReaderHost.Redirect(this.Module, type);
            if (redirectedType == type) parentModuleType.AddMember(type);
            this.ModuleTypeDefArray[typeDefRowId] = type;
            this.RedirectedTypeDefArray[typeDefRowId] = redirectedType;
          } else {
            this.PEFileReader.ErrorContainer.AddMetadataError(TableIndices.NestedClass, typeDefRowId, MetadataReaderErrorKind.NestedClassParentError);
          }
        } else {
          Namespace parentNamespace = this.GetNamespaceForNameOffset(typeDefRow.Namespace);
          var type = this.CreateModuleNamespaceType(typeDefRowId, typeDefRow, parentNamespace, MetadataReaderSignatureTypeCode.NotModulePrimitive);
          var redirectedType = this.ModuleReader.metadataReaderHost.Redirect(this.Module, type);
          if (redirectedType == type) parentNamespace.AddMember(type);
          this.ModuleTypeDefArray[typeDefRowId] = type;
          this.RedirectedTypeDefArray[typeDefRowId] = redirectedType;
        }
        this.ModuleTypeDefLoadState[typeDefRowId] = LoadState.Loaded;
      }
      return this.ModuleTypeDefArray[typeDefRowId];
    }

    internal INamedTypeReference/*?*/ GetTypeDefinitionAtRow(
      uint typeDefRowId
    ) {
      if (typeDefRowId == 0 || typeDefRowId > this.PEFileReader.TypeDefTable.NumberOfRows) {
        return null;
      }
      if (this.ModuleTypeDefLoadState[typeDefRowId] == LoadState.Uninitialized) {
        lock (GlobalLock.LockingObject) {
          if (this.ModuleTypeDefLoadState[typeDefRowId] == LoadState.Uninitialized) {
            this.GetTypeDefinitionAtRowWorker(typeDefRowId);
          }
        }
      }
      return this.RedirectedTypeDefArray[typeDefRowId];
    }

    internal ExportedTypeAliasBase/*?*/ GetExportedTypeAtRowWorker(
      uint exportedTypeRowId
    )
      //^ ensures this.ExportedTypeLoadState[exportedTypeRowId] == LoadState.Loaded;
    {
      if (exportedTypeRowId == 0 || exportedTypeRowId > this.PEFileReader.ExportedTypeTable.NumberOfRows) {
        return null;
      }
      if (this.ExportedTypeLoadState[exportedTypeRowId] == LoadState.Uninitialized) {
        Debug.Assert(this.ExportedTypeArray[exportedTypeRowId] == null);
        this.ExportedTypeLoadState[exportedTypeRowId] = LoadState.Loading;
        ExportedTypeRow exportedTypeRow = this.PEFileReader.ExportedTypeTable[exportedTypeRowId];
        ExportedTypeAliasBase/*?*/ expType =  null;
        if (exportedTypeRow.IsNested) {
          if ((exportedTypeRow.Implementation & TokenTypeIds.TokenTypeMask) == TokenTypeIds.ExportedType) {
            uint parentExportedTypeRowId = exportedTypeRow.Implementation & TokenTypeIds.RIDMask;
            ExportedTypeAliasBase/*?*/ parentExportedType = this.GetExportedTypeAtRowWorker(parentExportedTypeRowId);
            if (parentExportedType != null) {
              expType = this.CreateExportedNestedType(exportedTypeRowId, exportedTypeRow, parentExportedType);
            } else {
              this.PEFileReader.ErrorContainer.AddMetadataError(TableIndices.ExportedType, exportedTypeRowId, MetadataReaderErrorKind.NestedClassParentError);
            }
          } else {
            //  MD Error
            return null;
          }
        } else {
          Namespace parentNamespace = this.GetNamespaceForNameOffset(exportedTypeRow.TypeNamespace);
          expType = this.CreateExportedNamespaceType(exportedTypeRowId, exportedTypeRow, parentNamespace);
        }
        this.ExportedTypeArray[exportedTypeRowId] = expType;
        this.ExportedTypeLoadState[exportedTypeRowId] = LoadState.Loaded;
      }
      ExportedTypeAliasBase/*?*/ et = this.ExportedTypeArray[exportedTypeRowId];
      return et;
    }

    internal ExportedTypeAliasBase/*?*/ GetExportedTypeAtRow(
      uint exportedTypeRowId
    ) {
      if (exportedTypeRowId == 0 || exportedTypeRowId > this.PEFileReader.ExportedTypeTable.NumberOfRows) {
        return null;
      }
      if (this.ExportedTypeLoadState[exportedTypeRowId] == LoadState.Uninitialized) {
        lock (GlobalLock.LockingObject) {
          if (this.ExportedTypeLoadState[exportedTypeRowId] == LoadState.Uninitialized) {
            this.GetExportedTypeAtRowWorker(exportedTypeRowId);
          }
        }
      }
      ExportedTypeAliasBase /*?*/ et = this.ExportedTypeArray[exportedTypeRowId];
      return et;
    }

    internal IEnumerable<IAliasForType> GetEnumberableForExportedTypes() {
      uint numberOfExportedTypes = this.PEFileReader.ExportedTypeTable.NumberOfRows;
      for (uint i = 1; i <= numberOfExportedTypes; ++i) {
        ExportedTypeAliasBase/*?*/ expTypeAlias = this.GetExportedTypeAtRow(i);
        if (expTypeAlias != null) {
          yield return expTypeAlias;
        } else {
          yield return Dummy.AliasForType;
        }
      }
    }

    internal void LoadNestedExportedTypesOfAlias(ExportedTypeAliasBase exportedType) {
      uint currentToken = exportedType.TokenValue;
      uint numberOfExportedTypes = this.PEFileReader.ExportedTypeTable.NumberOfRows;
      ExportedTypeAliasBase/*?*/[] exportedTypeArray = this.ExportedTypeArray;
      for (uint i = 1; i <= numberOfExportedTypes; ++i) {
        ExportedTypeRow exportedTypeRow = this.PEFileReader.ExportedTypeTable[i];
        //  Check if its in the same namespace
        if (exportedTypeRow.Implementation != currentToken || !exportedTypeRow.IsNested)
          continue;
        //  This will not be loaded by anyone other than 
        ExportedTypeNestedAlias/*?*/ type = (ExportedTypeNestedAlias)exportedTypeArray[i];
        if (type == null) {
          type = this.CreateExportedNestedType(i, exportedTypeRow, exportedType);
          this.ExportedTypeArray[i] = type;
          this.ExportedTypeLoadState[i] = LoadState.Loaded;
        }
      }
    }

    /// <summary>
    /// Given a namespace full name and type's mangled name this method resolves it to a TypeBase.
    /// </summary>
    internal INamedTypeDefinition/*?*/ ResolveNamespaceTypeDefinition(IName namespaceName, IName mangledTypeName) {
      uint typeToken = this.NamespaceTypeTokenTable.Find((uint)namespaceName.UniqueKey, (uint)mangledTypeName.UniqueKey);
      //if (typeToken == 0 || ((typeToken & TokenTypeIds.TokenTypeMask) != TokenTypeIds.TypeDef)) return null;
      var tokenType = typeToken & TokenTypeIds.TokenTypeMask;
      if (tokenType == TokenTypeIds.TypeDef)
        return this.GetTypeDefinitionAtRow(typeToken & TokenTypeIds.RIDMask) as INamedTypeDefinition;
      if (tokenType == TokenTypeIds.ExportedType) {
        var alias = this.GetExportedTypeAtRowWorker(typeToken & 0xFFFFFF);
        if (alias != null) return alias.AliasedType.ResolvedType;
      }
      return null;
    }

    /// <summary>
    /// Given a parent type and mangled type name of the nested type, this method resolves it.
    /// </summary>
    /// <param name="parentType"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    internal INamedTypeDefinition/*?*/ ResolveNestedTypeDefinition(INamedTypeDefinition parentType, IName typeName) {
      var mdParentType = parentType as TypeBase;
      if (mdParentType != null) {
        uint typeToken = this.NestedTypeTokenTable.Find(TokenTypeIds.TypeDef | mdParentType.TypeDefRowId, (uint)typeName.UniqueKey);
        if (typeToken == 0 || ((typeToken & TokenTypeIds.TokenTypeMask) != TokenTypeIds.TypeDef)) return null;
        return this.GetTypeDefinitionAtRowWorker(typeToken & TokenTypeIds.RIDMask);
      }
      ushort genericParamCount;
      string unmangledTypeName;
      TypeCache.SplitMangledTypeName(typeName.Value, out unmangledTypeName, out genericParamCount);
      return TypeHelper.GetNestedType(parentType, this.NameTable.GetNameFor(unmangledTypeName), genericParamCount);
    }

    /// <summary>
    /// Given a namespace full name and type's mangled name this method resolves it to a type alias, if such an alias exists in this module.
    /// </summary>
    internal ExportedTypeAliasBase/*?*/ TryToResolveAsNamespaceTypeAlias(IName namespaceName, IName mangledTypeName) {
      uint exportedTypeToken = this.NamespaceTypeTokenTable.Find((uint)namespaceName.UniqueKey, (uint)mangledTypeName.UniqueKey);
      if (exportedTypeToken == 0 || ((exportedTypeToken & TokenTypeIds.TokenTypeMask) != TokenTypeIds.ExportedType)) return null;
      return this.GetExportedTypeAtRowWorker(exportedTypeToken & TokenTypeIds.RIDMask);
    }

    /// <summary>
    /// Given a alias type and type's mangled name this method resolves it to a nested aliased type.
    /// Aliased type can further be walked to find the exact type it resolved to.
    /// </summary>
    /// <param name="parentType"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    internal ExportedTypeAliasBase/*?*/ ResolveExportedNestedType(
      ExportedTypeAliasBase parentType,
      IName typeName
    ) {
      uint exportedTypeToken = this.NestedTypeTokenTable.Find(TokenTypeIds.ExportedType | parentType.ExportedTypeRowId, (uint)typeName.UniqueKey);
      if (exportedTypeToken == 0 || ((exportedTypeToken & TokenTypeIds.TokenTypeMask) != TokenTypeIds.ExportedType)) {
        return null;
      }
      return this.GetExportedTypeAtRowWorker(exportedTypeToken & TokenTypeIds.RIDMask);
    }

    internal IEnumerable<INamedTypeDefinition> GetAllTypes() {
      uint lastTypeDefRow = this.PEFileReader.TypeDefTable.NumberOfRows;
      for (uint typeDefRowId = 1; typeDefRowId <= lastTypeDefRow; ++typeDefRowId) {
        var redirectedType = this.GetTypeDefinitionAtRow(typeDefRowId);
        var typedef = this.ModuleTypeDefArray[typeDefRowId];
        if (typedef != null && typedef == redirectedType) yield return typedef;
      }
    }

    #endregion Type Definition Level Loading/Conversions

    #region Type Information
    internal void GetGenericParamInfoForType(
      uint typeDefRowId,
      out uint genericParamRowIdStart,
      out uint genericParamRowIdEnd
    ) {
      ushort genericParamCount;
      genericParamRowIdStart = this.PEFileReader.GenericParamTable.FindGenericParametersForType(typeDefRowId, out genericParamCount);
      genericParamRowIdEnd = genericParamRowIdStart + genericParamCount;
    }
    internal ITypeReference/*?*/ GetBaseTypeForType(
      TypeBase moduleType
    ) {
      uint baseTypeToken = this.PEFileReader.TypeDefTable.GetExtends(moduleType.TypeDefRowId);
      if ((baseTypeToken & TokenTypeIds.RIDMask) == 0) {
        return null;
      }
      return this.GetTypeReferenceForToken(moduleType, baseTypeToken);
    }
    internal void GetInterfaceInfoForType(
      TypeBase moduleType,
      out uint interfaceRowIdStart,
      out uint interfaceRowIdEnd
    ) {
      uint interfaceCount;
      interfaceRowIdStart = this.PEFileReader.InterfaceImplTable.FindInterfaceImplForType(moduleType.TypeDefRowId, out interfaceCount);
      interfaceRowIdEnd = interfaceRowIdStart + interfaceCount;
    }
    internal void GetMethodImplInfoForType(
      TypeBase moduleType,
      out uint methodImplRowIdStart,
      out uint methodImplRowIdEnd
    ) {
      ushort methodImplCount;
      methodImplRowIdStart = this.PEFileReader.MethodImplTable.FindMethodsImplForClass(moduleType.TypeDefRowId, out methodImplCount);
      methodImplRowIdEnd = methodImplRowIdStart + methodImplCount;
    }
    internal void GetConstraintInfoForGenericParam(
      GenericParameter genericParam,
      out uint genericParamConstraintRowIdStart,
      out uint genericParamConstraintRowIdEnd
    ) {
      uint constraintCount;
      genericParamConstraintRowIdStart = this.PEFileReader.GenericParamConstraintTable.FindConstraintForGenericParam(genericParam.GenericParameterRowId, out constraintCount);
      genericParamConstraintRowIdEnd = genericParamConstraintRowIdStart + constraintCount;
    }
    internal ITypeReference/*?*/ GetInterfaceForInterfaceRowId(
      TypeBase moduleType,
      uint interfaceRowId
    ) {
      uint interfaceToken = this.PEFileReader.InterfaceImplTable.GetInterface(interfaceRowId);
      return this.GetTypeReferenceForToken(moduleType, interfaceToken);
    }
    internal MethodImplementation GetMethodImplementation(
      TypeBase moduleType,
      uint methodImplRowId
    ) {
      MethodImplRow methodImplRow = this.PEFileReader.MethodImplTable[methodImplRowId];
      IMethodReference methodDecl = this.GetMethodReferenceForToken(moduleType, methodImplRow.MethodDeclaration);
      IMethodReference methodBody = this.GetMethodReferenceForToken(moduleType, methodImplRow.MethodBody);
      return new MethodImplementation(moduleType, methodDecl, methodBody);
    }
    internal ITypeReference/*?*/ GetTypeReferenceForGenericConstraintRowId(
      GenericParameter genParam,
      uint interfaceRowId
    ) {
      uint constraintTypeToken = this.PEFileReader.GenericParamConstraintTable.GetConstraint(interfaceRowId);
      return this.GetTypeReferenceForToken(genParam, constraintTypeToken);
    }
    internal ITypeReference/*?*/ GetSerializedTypeNameAsTypeReference(
      TypeName typeName
    ) {
      ITypeReference/*?*/ typeRef = typeName.GetAsTypeReference(this, this.ContainingAssembly);
      if (typeRef != null)
        return typeRef;
      Assembly/*?*/ assem = this.ModuleReader.CoreAssembly;
      if (assem == null) //  MDError...
        return null;
      return typeName.GetAsTypeReference(assem.PEFileToObjectModel, assem);
    }
    internal ITypeReference/*?*/ GetSerializedTypeNameAsTypeReference(
      string serializedTypeName
    ) {
      TypeNameParser typeNameParser = new TypeNameParser(this.NameTable, serializedTypeName);
      TypeName/*?*/ typeName = typeNameParser.ParseTypeName();
      if (typeName == null) //  MDError...
        return null;
      return this.GetSerializedTypeNameAsTypeReference(typeName);
    }
    internal ushort GetAlignment(
      TypeBase type
    ) {
      return this.PEFileReader.ClassLayoutTable.GetPackingSize(type.TypeDefRowId);
    }
    internal uint GetClassSize(
      TypeBase type
    ) {
      return this.PEFileReader.ClassLayoutTable.GetClassSize(type.TypeDefRowId);
    }
    #endregion Type Information

    #region Generic Param Loading/Conversions
    readonly GenericParameter[] ModuleGenericParamArray;

    internal GenericTypeParameter/*?*/ GetGenericTypeParamAtRow(
      uint genericParamRowId,
      TypeBase moduleTypeOwner
    )
      //^ requires genericParamRowId >= 1;
    {
      if (this.ModuleGenericParamArray[genericParamRowId] == null) {
        lock (GlobalLock.LockingObject) {
          if (this.ModuleGenericParamArray[genericParamRowId] == null) {
            GenericParamRow genericParamRow = this.PEFileReader.GenericParamTable[genericParamRowId];
            IName genericParamName = this.GetNameFromOffset(genericParamRow.Name);
            this.ModuleGenericParamArray[genericParamRowId] = new GenericTypeParameter(this, genericParamRow.Number, genericParamRow.Flags, genericParamName, genericParamRowId, moduleTypeOwner);
          }
        }
      }
      return this.ModuleGenericParamArray[genericParamRowId] as GenericTypeParameter;
    }

    internal GenericMethodParameter/*?*/ GetGenericMethodParamAtRow(
      uint genericParamRowId,
      GenericMethod moduleMethodOwner
    )
      //^ requires genericParamRowId >= 1;
    {
      if (this.ModuleGenericParamArray[genericParamRowId] == null) {
        lock (GlobalLock.LockingObject) {
          if (this.ModuleGenericParamArray[genericParamRowId] == null) {
            GenericParamRow genericParamRow = this.PEFileReader.GenericParamTable[genericParamRowId];
            IName genericParamName = this.GetNameFromOffset(genericParamRow.Name);
            this.ModuleGenericParamArray[genericParamRowId] = new GenericMethodParameter(this, genericParamRow.Number, genericParamRow.Flags, genericParamName, genericParamRowId, moduleMethodOwner);
          }
        }
      }
      return this.ModuleGenericParamArray[genericParamRowId] as GenericMethodParameter;
    }
    #endregion Generic Param Loading/Conversions

    #region Type Reference Level Loading/Converstions

    /// <summary>
    /// Cache for type ref's in the module.
    /// </summary>
    readonly INamedTypeReference[] ModuleTypeRefReferenceArray;
    readonly LoadState[] ModuleTypeRefReferenceLoadState;

    readonly DoubleHashtable<TypeSpecReference>/*?*/ ModuleTypeSpecHashtable;
    //^ invariant this.PEFileReader.TypeSpecTable.NumberOfRows >= 1 ==> this.ModuleTypeSpecHashtable != null;

    TypeRefReference CreateTypeRefReference(
      uint typeRefRowId,
      TypeRefRow typeRefRow,
      IMetadataReaderNamedTypeReference/*?*/ parentModuleTypeReference,
      IMetadataReaderModuleReference moduleReference,
      MetadataReaderSignatureTypeCode signatureTypeCode
    ) {
      IName mangledTypeName = this.GetNameFromOffset(typeRefRow.Name);
      ushort genericParamCount;
      string typeName;
      TypeCache.SplitMangledTypeName(mangledTypeName.Value, out typeName, out genericParamCount);
      TypeRefReference moduleTypeRefReference;
      if (parentModuleTypeReference == null) {
        IName namespaceFullName = this.GetNameFromOffset(typeRefRow.Namespace);
        NamespaceReference namespaceReference = this.GetNamespaceReferenceForString(moduleReference, namespaceFullName);
        if (genericParamCount == 0) {
          if (signatureTypeCode == MetadataReaderSignatureTypeCode.NotModulePrimitive) {
            moduleTypeRefReference = new NamespaceTypeRefReferenceWithoutPrimitiveTypeCode(
              this,
              mangledTypeName,
              typeRefRowId,
              moduleReference,
              namespaceReference,
              signatureTypeCode == MetadataReaderSignatureTypeCode.ValueType
            );
          } else {
            moduleTypeRefReference = new NamespaceTypeRefReferenceWithPrimitiveTypeCode(
              this,
              mangledTypeName,
              typeRefRowId,
              moduleReference,
              namespaceReference,
              signatureTypeCode
            );
          }
        } else {
          IName iTypeName = this.NameTable.GetNameFor(typeName);
          moduleTypeRefReference = new GenericNamespaceTypeRefReference(
            this,
            iTypeName,
            typeRefRowId,
            moduleReference,
            namespaceReference,
            mangledTypeName,
            genericParamCount,
            signatureTypeCode == MetadataReaderSignatureTypeCode.ValueType
          );
        }
      } else {
        if (genericParamCount == 0) {
          moduleTypeRefReference = new NonGenericNestedTypeRefReference(
            this,
            mangledTypeName,
            typeRefRowId,
            moduleReference,
            parentModuleTypeReference,
            signatureTypeCode == MetadataReaderSignatureTypeCode.ValueType
          );
        } else {
          IName iTypeName = this.NameTable.GetNameFor(typeName);
          moduleTypeRefReference = new GenericNestedTypeRefReference(
            this,
            iTypeName,
            typeRefRowId,
            moduleReference,
            parentModuleTypeReference,
            mangledTypeName,
            genericParamCount,
            signatureTypeCode == MetadataReaderSignatureTypeCode.ValueType
          );
        }
      }
      return moduleTypeRefReference;
    }

    internal IMetadataReaderNamedTypeReference GetPredefinedTypeRefReferenceAtRowWorker(
      uint typeRefRowId,
      MetadataReaderSignatureTypeCode signatureTypeCode
    ) {
      Debug.Assert(typeRefRowId > 0 && typeRefRowId <= this.PEFileReader.TypeRefTable.NumberOfRows);
      Debug.Assert(
        this.ModuleTypeRefReferenceLoadState[typeRefRowId] == LoadState.Uninitialized
        && this.ModuleTypeRefReferenceArray[typeRefRowId] == null
      );
      this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loading;
      TypeRefRow typeRefRow = this.PEFileReader.TypeRefTable[typeRefRowId];
      uint resolutionScopeKind = (typeRefRow.ResolutionScope & TokenTypeIds.TokenTypeMask);
      uint resolutionScopeRowId = (typeRefRow.ResolutionScope & TokenTypeIds.RIDMask);
      Debug.Assert(resolutionScopeKind == TokenTypeIds.AssemblyRef);  //  TODO: MD Error
      var result = this.CreateTypeRefReference(typeRefRowId, typeRefRow, null, this.AssemblyReferenceArray[resolutionScopeRowId], signatureTypeCode);
      this.ModuleTypeRefReferenceArray[typeRefRowId] = result;
      this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loaded;
      return result;
    }

    IMetadataReaderNamedTypeReference/*?*/ GetTypeRefReferenceAtRowWorker(
      uint typeRefRowId
    ) {
      return this.GetTypeRefReferenceAtRowWorker(typeRefRowId, false) as IMetadataReaderNamedTypeReference;
    }
    INamedTypeReference/*?*/ GetTypeRefReferenceAtRowWorker(
      uint typeRefRowId,
      bool mustBeStruct
    ) {
      if (typeRefRowId > this.PEFileReader.TypeRefTable.NumberOfRows || typeRefRowId == 0) {
        return null;
      }
      if (this.ModuleTypeRefReferenceLoadState[typeRefRowId] == LoadState.Uninitialized) {
        Debug.Assert(this.ModuleTypeRefReferenceArray[typeRefRowId] == null);
        CoreTypes dummy = this.CoreTypes; //force core types to be initialized
        this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loading;
        TypeRefRow typeRefRow = this.PEFileReader.TypeRefTable[typeRefRowId];
        TypeRefReference typeRefReference;
        uint resolutionScopeKind = (typeRefRow.ResolutionScope & TokenTypeIds.TokenTypeMask);
        uint resolutionScopeRowId = typeRefRow.ResolutionScope & TokenTypeIds.RIDMask;
        if (resolutionScopeKind == TokenTypeIds.TypeRef) {
          var parentModuleTypeReference = this.GetTypeRefReferenceAtRowWorker(resolutionScopeRowId);
          if (parentModuleTypeReference == null) {
            this.PEFileReader.ErrorContainer.AddMetadataError(TableIndices.TypeRef, typeRefRowId, MetadataReaderErrorKind.NestedClassParentError);
            this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loaded;
            return null;
          }
          typeRefReference = this.CreateTypeRefReference(typeRefRowId, typeRefRow, parentModuleTypeReference, parentModuleTypeReference.ModuleReference,
            mustBeStruct ? MetadataReaderSignatureTypeCode.ValueType : MetadataReaderSignatureTypeCode.NotModulePrimitive);
        } else {
          IMetadataReaderModuleReference moduleReference;
          if (resolutionScopeKind == TokenTypeIds.AssemblyRef) {
            AssemblyReference/*?*/ assemblyRef = this.GetAssemblyReferenceAt(resolutionScopeRowId);
            if (assemblyRef == null) {
              //  TODO: MDError
              this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loaded;
              return null;
            }
            moduleReference = assemblyRef;
          } else if (resolutionScopeKind == TokenTypeIds.ModuleRef) {
            ModuleReference/*?*/ moduleRef = this.GetModuleReferenceAt(resolutionScopeRowId);
            if (moduleRef == null) {
              //  TODO: MDError
              this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loaded;
              return null;
            }
            moduleReference = moduleRef;
          } else {
            moduleReference = this.Module;
          }
          typeRefReference = this.CreateTypeRefReference(typeRefRowId, typeRefRow, null, moduleReference,
            mustBeStruct ? MetadataReaderSignatureTypeCode.ValueType : MetadataReaderSignatureTypeCode.NotModulePrimitive);
        }
        var redirectedTypeReference = this.ModuleReader.metadataReaderHost.Redirect(this.Module, typeRefReference) as INamedTypeReference;
        this.ModuleTypeRefReferenceArray[typeRefRowId] = redirectedTypeReference??typeRefReference;
        this.ModuleTypeRefReferenceLoadState[typeRefRowId] = LoadState.Loaded;
      }
      return this.ModuleTypeRefReferenceArray[typeRefRowId];
    }

    internal IEnumerable<ITypeMemberReference> GetMemberReferences() {
      for (uint i = 1; i <= this.PEFileReader.MemberRefTable.NumberOfRows; i++) {
        MemberRefRow mrr = this.PEFileReader.MemberRefTable[i];
        if ((mrr.Class & TokenTypeIds.TokenTypeMask) == TokenTypeIds.TypeSpec) continue;
        var mr = this.GetModuleMemberReferenceAtRow(this.Module, i);
        if (mr != null) yield return mr;
      }
    }

    internal IEnumerable<ITypeReference> GetTypeReferences() {
      for (uint i = 1; i <= this.PEFileReader.TypeRefTable.NumberOfRows; i++) {
        var tr = this.GetTypeRefReferenceAtRow(i);
        if (tr != null) yield return tr; //could be null if the module is malformed (which happens with some obfuscators).
      }
    }

    internal INamedTypeReference/*?*/ GetTypeRefReferenceAtRow(
      uint typeRefRowId
    )
      //^ requires typeRefRowId >=1;
    {
      return this.GetTypeRefReferenceAtRow(typeRefRowId, false);
    }
    internal INamedTypeReference/*?*/ GetTypeRefReferenceAtRow(
      uint typeRefRowId,
      bool mustBeStruct
    )
      //^ requires typeRefRowId >=1;
    {
      if (typeRefRowId > this.PEFileReader.TypeRefTable.NumberOfRows || typeRefRowId == 0) {
        return null;
      }
      if (this.ModuleTypeRefReferenceLoadState[typeRefRowId] == LoadState.Uninitialized) {
        lock (GlobalLock.LockingObject) {
          this.GetTypeRefReferenceAtRowWorker(typeRefRowId, mustBeStruct);
        }
      }
      var typeRefReference = this.ModuleTypeRefReferenceArray[typeRefRowId] as TypeRefReference;
      if (mustBeStruct && typeRefReference != null) typeRefReference.isValueType = true;
      return this.ModuleTypeRefReferenceArray[typeRefRowId];
    }
    internal TypeSpecReference/*?*/ GetTypeSpecReferenceAtRow(
      MetadataObject owningObject,
      uint typeSpecRowId
    )
      //^ requires this.PEFileReader.TypeSpecTable.NumberOfRows >= 1;
      //^ requires typeSpecRowId >= 1 && typeSpecRowId <= this.PEFileReader.TypeSpecTable.NumberOfRows;
    {
      if (typeSpecRowId > this.PEFileReader.TypeSpecTable.NumberOfRows || typeSpecRowId == 0) {
        return null;
      }
      uint ownerId = owningObject.TokenValue;
      //^ assert this.ModuleTypeSpecHashtable != null;
      TypeSpecReference/*?*/ typeSpecReference = this.ModuleTypeSpecHashtable.Find(ownerId, typeSpecRowId);
      if (typeSpecReference == null) {
        lock (GlobalLock.LockingObject) {
          typeSpecReference = this.ModuleTypeSpecHashtable.Find(ownerId, typeSpecRowId);
          if (typeSpecReference == null) {
            typeSpecReference = new TypeSpecReference(this, typeSpecRowId, owningObject);
          }
          this.ModuleTypeSpecHashtable.Add(ownerId, typeSpecRowId, typeSpecReference);
        }
      }
      return typeSpecReference;
    }
    internal ITypeReference/*?*/ GetTypeReferenceForToken(
      MetadataObject owningObject,
      uint token
    ) {
      return this.GetTypeReferenceForToken(owningObject, token, false);
    }
    internal ITypeReference/*?*/ GetTypeReferenceForToken(
      MetadataObject owningObject,
      uint token,
      bool mustBeStruct
    ) {
      uint tokenType = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      switch (tokenType) {
        case TokenTypeIds.TypeDef: {
            if (rowId == 0 || rowId > this.PEFileReader.TypeDefTable.NumberOfRows) {
              //  handle Error
            }
            return this.GetTypeDefinitionAtRow(rowId);
          }
        case TokenTypeIds.TypeRef: {
            if (rowId == 0 || rowId > this.PEFileReader.TypeRefTable.NumberOfRows) {
              //  handle Error
            }
            return this.GetTypeRefReferenceAtRow(rowId, mustBeStruct);
          }
        case TokenTypeIds.TypeSpec: {
            if (rowId == 0 || rowId > this.PEFileReader.TypeSpecTable.NumberOfRows) {
              //  handle Error
            }
            return this.GetTypeSpecReferenceAtRow(owningObject, rowId).UnderlyingModuleTypeReference;
          }
      }
      return null;
    }

    /// <summary>
    ///  This method resolves TypeRef as a non exported type. i.e. the said type reference refers to the type
    ///  in the type def table direcly rather than exported type table of the assembly.
    /// </summary>
    /// <param name="namedTypeReference"></param>
    /// <returns></returns>
    internal INamedTypeDefinition/*?*/ ResolveModuleTypeRefReference(IMetadataReaderNamedTypeReference namedTypeReference) {
      var moduleTypeRefReference = namedTypeReference as TypeRefReference;
      if (moduleTypeRefReference == null) return namedTypeReference.ResolvedType;
      uint typeRefRowId = moduleTypeRefReference.TypeRefRowId;
      if (typeRefRowId == 0) return null;
      TypeRefRow typeRefRow = this.PEFileReader.TypeRefTable[typeRefRowId];
      IName namespaceName = this.GetNameFromOffset(typeRefRow.Namespace);
      IName typeName = moduleTypeRefReference.MangledTypeName;
      uint resolutionScope = typeRefRow.ResolutionScope;
      uint tokenType = resolutionScope & TokenTypeIds.TokenTypeMask;
      uint rowId = resolutionScope & TokenTypeIds.RIDMask;
      INamedTypeDefinition/*?*/ retModuleType = null;
      switch (tokenType) {
        case TokenTypeIds.Module:
          retModuleType = this.ResolveNamespaceTypeDefinition(namespaceName, typeName);
          break;
        case TokenTypeIds.ModuleRef: {
            ModuleReference/*?*/ modRef = this.GetModuleReferenceAt(rowId);
            if (modRef != null) {
              var module = this.ResolveModuleRefReference(modRef) as Module;
              if (module != null) {
                PEFileToObjectModel modulePEFileToObjectModel = module.PEFileToObjectModel;
                retModuleType = modulePEFileToObjectModel.ResolveNamespaceTypeDefinition(namespaceName, typeName);
                if (retModuleType != null) return retModuleType;
              }
            }
            break;
          }
        case TokenTypeIds.AssemblyRef: {
            AssemblyReference/*?*/ assemRef = this.GetAssemblyReferenceAt(rowId);
            if (assemRef == null) return null;
            var internalAssembly = assemRef.ResolvedAssembly as Assembly;
            if (internalAssembly != null) {
              PEFileToObjectModel assemblyPEFileToObjectModel = internalAssembly.PEFileToObjectModel;
              retModuleType = assemblyPEFileToObjectModel.ResolveNamespaceTypeDefinition(namespaceName, typeName);
              if (retModuleType != null) return retModuleType;
            }
            break;
          }
        case TokenTypeIds.TypeRef: {
            var parentTypeReference = this.GetTypeRefReferenceAtRowWorker(rowId);
            if (parentTypeReference == null) return null;
            INamedTypeDefinition parentType;
            if (parentTypeReference.IsAlias)
              parentType = parentTypeReference.AliasForType.AliasedType.ResolvedType;
            else
              parentType = this.ResolveModuleTypeRefReference(parentTypeReference);
            if (parentType != null) {
              var parentModuleType = parentType as TypeBase;
              if (parentModuleType != null)
                retModuleType = parentModuleType.PEFileToObjectModel.ResolveNestedTypeDefinition(parentModuleType, typeName);
              else
                retModuleType = this.ResolveNestedTypeDefinition(parentType, typeName);
            }
            break;
          }
        default:
          break;
      }
      return retModuleType;
    }

    /// <summary>
    /// This method tries to resolves the give namespace type reference as an exported type.
    /// </summary>
    /// <param name="namespaceTypeReference"></param>
    internal ExportedTypeAliasBase/*?*/ TryToResolveNamespaceTypeReferenceAsExportedType(NamespaceTypeRefReference namespaceTypeReference) {
      uint typeRefRowId = namespaceTypeReference.TypeRefRowId;
      if (typeRefRowId == 0) return null;
      TypeRefRow typeRefRow = this.PEFileReader.TypeRefTable[typeRefRowId];
      IName namespaceName = this.GetNameFromOffset(typeRefRow.Namespace);
      IName typeName = this.GetNameFromOffset(typeRefRow.Name);
      uint resolutionScope = typeRefRow.ResolutionScope;
      if (resolutionScope == 0) //The reference is to something in this module
        return this.TryToResolveAsNamespaceTypeAlias(namespaceName, typeName);
      uint tokenType = resolutionScope & TokenTypeIds.TokenTypeMask;
      if (tokenType == TokenTypeIds.AssemblyRef) { //Only assemblies can have aliases
        uint rowId = resolutionScope & TokenTypeIds.RIDMask;
        AssemblyReference/*?*/ assemRef = this.GetAssemblyReferenceAt(rowId);
        if (assemRef == null) return null;
        var internalAssembly = assemRef.ResolvedAssembly as Assembly;
        if (internalAssembly != null) {
          PEFileToObjectModel assemblyPEFileToObjectModel = internalAssembly.PEFileToObjectModel;
          return assemblyPEFileToObjectModel.TryToResolveAsNamespaceTypeAlias(namespaceName, typeName);
        }
        //Since we are not able to resolve the assembly reference, we cannot know if the referenced assembly has an alias, so we just give up and return null.
      }
      return null;
    }

    /// <summary>
    /// Returns a reference to the type that the given alias stands for. For example, if alias is a type forwarder, return a reference to the forwarded type (in another assembly).
    /// </summary>
    internal INamedTypeReference/*?*/ GetReferenceToAliasedType(ExportedTypeAliasBase alias) {
      Assembly/*?*/ thisAssembly = this.Module as Assembly;
      if (thisAssembly == null) return null;
      uint exportedTypeRowId = alias.ExportedTypeRowId;
      if (exportedTypeRowId == 0) return null;
      ExportedTypeRow exportedTypeRow = this.PEFileReader.ExportedTypeTable[exportedTypeRowId];
      uint tokenType = exportedTypeRow.Implementation & TokenTypeIds.TokenTypeMask;
      uint rowId = exportedTypeRow.Implementation & TokenTypeIds.RIDMask;
      IName namespaceName = this.GetNameFromOffset(exportedTypeRow.TypeNamespace);
      IName mangledTypeName = this.GetNameFromOffset(exportedTypeRow.TypeName);
      IName unmangledTypeName = this.GetUnmangledNameFromOffset(exportedTypeRow.TypeName);
      switch (tokenType) {
        case TokenTypeIds.File: {
            FileReference/*?*/ fileRef = this.GetFileReferenceAt(rowId);
            if (fileRef == null) return null;
            var module = thisAssembly.FindMemberModuleNamed(fileRef.Name) as Module;
            if (module == null) return null;
            var foundType = module.PEFileToObjectModel.ResolveNamespaceTypeDefinition(namespaceName, mangledTypeName);
            if (foundType == null) return null;
            return foundType;
          }
        case TokenTypeIds.ExportedType: {
            ExportedTypeAliasBase/*?*/ parentExportedType = this.GetExportedTypeAtRowWorker(rowId);
            if (parentExportedType == null) return null;
            var parentModuleType = this.GetReferenceToAliasedType(parentExportedType);
            if (parentModuleType == null) return null;
            ITypeDefinition parentType = parentModuleType.ResolvedType;
            if (!(parentType is Dummy)) {
              foreach (ITypeDefinitionMember tdm in parentModuleType.ResolvedType.GetMembersNamed(unmangledTypeName, false)) {
                var modTypeRef = tdm as IMetadataReaderNamedTypeReference;
                if (modTypeRef != null)
                  return modTypeRef;
              }
            } else {
              NamespaceTypeNameTypeReference/*?*/ nstr = parentModuleType as NamespaceTypeNameTypeReference;
              if (nstr != null) {
                var nestedTypeName = new NestedTypeName(this.NameTable, nstr.NamespaceTypeName, mangledTypeName);
                return nestedTypeName.GetAsNamedTypeReference(this, nstr.Module);
              }
              NestedTypeNameTypeReference/*?*/ netr = parentModuleType as NestedTypeNameTypeReference;
              if (netr != null) {
                var nestedTypeName = new NestedTypeName(this.NameTable, netr.NestedTypeName, mangledTypeName);
                return nestedTypeName.GetAsNamedTypeReference(this, netr.Module);
              }
            }
            return null;
          }
        case TokenTypeIds.AssemblyRef: {
            AssemblyReference/*?*/ assemRef = this.GetAssemblyReferenceAt(rowId);
            if (assemRef == null) return null;
            var internalAssembly = assemRef.ResolvedAssembly as Assembly;
            if (internalAssembly != null) {
              //Since we have already loaded the assembly that is supposed to hold this type, we may as well try and resolve it.
              PEFileToObjectModel assemblyPEFileToObjectModel = internalAssembly.PEFileToObjectModel;
              var type = assemblyPEFileToObjectModel.ResolveNamespaceTypeDefinition(namespaceName, mangledTypeName);
              if (type != null) return type;
              //The other assembly (internalAssembly) does not have a namespace type def for this reference.
              //Perhaps it has an alias that forwards to somewhere else... Not very likely happen in practice, I would hope.
              ExportedTypeAliasBase/*?*/ aliasType = assemblyPEFileToObjectModel.TryToResolveAsNamespaceTypeAlias(namespaceName, mangledTypeName);
              if (aliasType != null && aliasType != alias) return assemblyPEFileToObjectModel.GetReferenceToAliasedType(aliasType);
              //Although we can resolve the target assembly, we can neither resolve the aliased type, nor find a secondary alias.
              //This is mighty strange. Probably the host has fluffed assembly resolution and internalAssembly isn't really the
              //assembly we are looking for. We now have to give up and simply return an unresolved reference.
            }
            string fullTypeName = mangledTypeName.Value;
            if (namespaceName.Value.Length > 0) fullTypeName = namespaceName.Value+"."+fullTypeName;
            var parser = new TypeNameParser(this.NameTable, fullTypeName);
            return parser.ParseTypeName().GetAsTypeReference(this, assemRef) as INamedTypeReference;
          }
      }
      return null;
    }

    internal ITypeReference/*?*/ UnderlyingModuleTypeSpecReference(
      TypeSpecReference moduleTypeSpecReference
    )
      //^ requires moduleTypeSpecReference.TypeSpecRowId >= 1;
    {
      uint typeSpecRowId = moduleTypeSpecReference.TypeSpecRowId;
      if (typeSpecRowId == 0 || typeSpecRowId > this.PEFileReader.TypeSpecTable.NumberOfRows) {
        //  TODO: Error...
        return null;
      }
      uint signatureBlobOffset = this.PEFileReader.TypeSpecTable.GetSignature(typeSpecRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      TypeSpecSignatureConverter typeSpecSignatureConverter = new TypeSpecSignatureConverter(this, moduleTypeSpecReference, memoryReader);
      return typeSpecSignatureConverter.TypeReference;
    }

    internal TypeBase FindCoreTypeReference(CoreTypeReference coreTypeReference) {
      //  This method must be called on only Core Assembly's PEFileToObjectModel. How so we state this?
      var retModuleType = this.ResolveNamespaceTypeDefinition(coreTypeReference.NamespaceFullName, coreTypeReference.mangledTypeName) as TypeBase;
      Contract.Assume(retModuleType != null);
      return retModuleType;
    }

    #endregion Type Reference Level Loading/Converstions

    #region Type Member Definition Level loading/Conversion
    readonly FieldDefinition[] ModuleFieldArray;
    readonly IMethodDefinition[] ModuleMethodArray;
    readonly EventDefinition[] ModuleEventArray;
    readonly PropertyDefinition[] ModulePropertyArray;

    FieldDefinition CreateField(
      uint fieldDefRowId,
      TypeBase parentModuleType
    ) {
      Debug.Assert(fieldDefRowId > 0 && fieldDefRowId <= this.PEFileReader.FieldTable.NumberOfRows);
      FieldRow fieldRow = this.PEFileReader.FieldTable[fieldDefRowId];
      IName fieldName = this.GetNameFromOffset(fieldRow.Name);
      if ((fieldRow.Flags & FieldFlags.AccessMask) == FieldFlags.CompilerControlledAccess) {
        //Methods that are compiler controlled access are excempted from duplicate checking and may thus have names that cause
        //their intern keys to clash with other methods. Avoid this by mangling the names of such methods.
        //compiler controlled methods are always referred to via their tokens, so this renaming is safe to do.
        fieldName = NameTable.GetNameFor(fieldName.Value+"$PST"+((int)TableIndices.Field+fieldDefRowId));
      }
      FieldDefinition moduleField = new FieldDefinition(this, fieldName, parentModuleType, fieldDefRowId, fieldRow.Flags);
      if (fieldName.UniqueKey != this.ModuleReader._Deleted_.UniqueKey) {
        parentModuleType.AddMember(moduleField);
      }
      return moduleField;
    }
    IMethodDefinition CreateMethod(
      uint methodDefRowId,
      TypeBase parentModuleType
    ) {
      Debug.Assert(methodDefRowId > 0 && methodDefRowId <= this.PEFileReader.MethodTable.NumberOfRows);
      MethodRow methodRow = this.PEFileReader.MethodTable[methodDefRowId];
      IName methodName = this.GetNameFromOffset(methodRow.Name);
      if ((methodRow.Flags & MethodFlags.AccessMask) == MethodFlags.CompilerControlledAccess) {
        //Methods that are compiler controlled access are excempted from duplicate checking and may thus have names that cause
        //their intern keys to clash with other methods. Avoid this by mangling the names of such methods.
        //compiler controlled methods are always referred to via their tokens, so this renaming is safe to do.
        methodName = NameTable.GetNameFor(methodName.Value+"$PST"+((int)TableIndices.Method+methodDefRowId));
      }
      uint genericParamRowIdStart;
      uint genericParamRowIdEnd;
      this.GetGenericParamInfoForMethod(methodDefRowId, out genericParamRowIdStart, out genericParamRowIdEnd);
      IMethodDefinition moduleMethod;
      if (genericParamRowIdStart == 0) {
        moduleMethod = new NonGenericMethod(this, methodName, parentModuleType, methodDefRowId, methodRow.Flags, methodRow.ImplFlags);
      } else {
        moduleMethod = new GenericMethod(this, methodName, parentModuleType, methodDefRowId, methodRow.Flags, methodRow.ImplFlags, genericParamRowIdStart, genericParamRowIdEnd);
      }
      if (methodName.UniqueKey != this.ModuleReader._Deleted_.UniqueKey) {
        moduleMethod = this.ModuleReader.metadataReaderHost.Rewrite(this.Module, moduleMethod);
        parentModuleType.AddMember(moduleMethod);
      }
      return moduleMethod;
    }
    EventDefinition CreateEvent(
      uint eventDefRowId,
      TypeBase parentModuleType
    ) {
      Debug.Assert(eventDefRowId > 0 && eventDefRowId <= this.PEFileReader.EventTable.NumberOfRows);
      EventRow eventRow = this.PEFileReader.EventTable[eventDefRowId];
      IName eventName = this.GetNameFromOffset(eventRow.Name);
      EventDefinition moduleEvent = new EventDefinition(this, eventName, parentModuleType, eventDefRowId, eventRow.Flags);
      parentModuleType.AddMember(moduleEvent);
      return moduleEvent;
    }
    PropertyDefinition CreateProperty(
      uint propertyDefRowId,
      TypeBase parentModuleType
    ) {
      Debug.Assert(propertyDefRowId > 0 && propertyDefRowId <= this.PEFileReader.PropertyTable.NumberOfRows);
      PropertyRow propertyRow = this.PEFileReader.PropertyTable[propertyDefRowId];
      IName propertyName = this.GetNameFromOffset(propertyRow.Name);
      PropertyDefinition moduleProperty = new PropertyDefinition(this, propertyName, parentModuleType, propertyDefRowId, propertyRow.Flags);
      parentModuleType.AddMember(moduleProperty);
      return moduleProperty;
    }
    void LoadNestedTypesOfType(
      TypeBase moduleType
    ) {
      uint currentClassRowId = moduleType.TypeDefRowId;
      uint numberOfNestedClassRows = this.PEFileReader.NestedClassTable.NumberOfRows;
      var moduleTypeArray = this.ModuleTypeDefArray;
      uint firstNestedClassRow = 0;
      uint nestedClassCounter = 0;
      for (uint i = 1; i <= numberOfNestedClassRows; ++i) {
        NestedClassRow nestedClassRow = this.PEFileReader.NestedClassTable[i];
        if (nestedClassRow.EnclosingClass != currentClassRowId) continue;
        if (firstNestedClassRow == 0) firstNestedClassRow = i;
        nestedClassCounter++;
        //  Check if its already created by someone else
        NestedType/*?*/ currType = moduleTypeArray[nestedClassRow.NestedClass] as NestedType;
        if (currType == null) {
          currType = this.CreateModuleNestedType(nestedClassRow.NestedClass, this.PEFileReader.TypeDefTable[nestedClassRow.NestedClass], moduleType);
          var redirectedType = this.ModuleReader.metadataReaderHost.Redirect(this.Module, currType);
          if (redirectedType == currType) moduleType.AddMember(currType);
          moduleTypeArray[nestedClassRow.NestedClass] = currType;
          this.RedirectedTypeDefArray[nestedClassRow.NestedClass] = redirectedType;
          this.ModuleTypeDefLoadState[nestedClassRow.NestedClass] = LoadState.Loaded;
        }
      }
      if (nestedClassCounter > 0) {
        var nestedTypes = new INestedTypeDefinition[nestedClassCounter];
        uint j = 0;
        for (uint i = firstNestedClassRow; j < nestedClassCounter && i <= numberOfNestedClassRows; ++i) {
          NestedClassRow nestedClassRow = this.PEFileReader.NestedClassTable[i];
          if (nestedClassRow.EnclosingClass != currentClassRowId) continue;
          nestedTypes[j++] = (INestedTypeDefinition)moduleTypeArray[nestedClassRow.NestedClass];
          Contract.Assume(nestedTypes[j-1] != null);
        }
        Contract.Assume(j == nestedClassCounter);
        moduleType.nestedTypes = IteratorHelper.GetReadonly(nestedTypes);
      }
    }
    void LoadFieldsOfType(
      TypeBase moduleType
    ) {
      uint fieldCount;
      uint fieldStart = this.PEFileReader.GetFieldInformation(moduleType.TypeDefRowId, out fieldCount);
      uint fieldEnd = fieldStart + fieldCount;
      if (this.PEFileReader.UseFieldPtrTable) {
        uint numberOfFieldPtrRows = this.PEFileReader.FieldPtrTable.NumberOfRows;
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd && fieldIter <= numberOfFieldPtrRows; ++fieldIter) {
          uint fieldRowId = this.PEFileReader.FieldPtrTable.GetFieldFor(fieldIter);
          FieldDefinition moduleField = this.ModuleFieldArray[fieldRowId];
          if (moduleField == null) {
            moduleField = this.CreateField(fieldRowId, moduleType);
            this.ModuleFieldArray[fieldRowId] = moduleField;
          }
        }
      } else {
        uint numberOfFieldRows = this.PEFileReader.FieldTable.NumberOfRows;
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd && fieldIter <= numberOfFieldRows; ++fieldIter) {
          FieldDefinition moduleField = this.ModuleFieldArray[fieldIter];
          if (moduleField == null) {
            moduleField = this.CreateField(fieldIter, moduleType);
            this.ModuleFieldArray[fieldIter] = moduleField;
          }
        }
      }
    }
    internal IEnumerable<IFieldDefinition> GetFieldsOfType(
      TypeBase moduleType
    ) {
      uint fieldCount;
      uint fieldStart = this.PEFileReader.GetFieldInformation(moduleType.TypeDefRowId, out fieldCount);
      uint fieldEnd = fieldStart + fieldCount;
      if (this.PEFileReader.UseFieldPtrTable) {
        uint numberOfFieldPtrRows = this.PEFileReader.FieldPtrTable.NumberOfRows;
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd && fieldIter <= numberOfFieldPtrRows; ++fieldIter) {
          uint fieldRowId = this.PEFileReader.FieldPtrTable.GetFieldFor(fieldIter);
          yield return this.ModuleFieldArray[fieldRowId];
        }
      } else {
        uint numberOfFieldRows = this.PEFileReader.FieldTable.NumberOfRows;
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd && fieldIter <= numberOfFieldRows; ++fieldIter) {
          yield return this.ModuleFieldArray[fieldIter];
        }
      }
    }
    void LoadMethodsOfType(
      TypeBase moduleType
    ) {
      uint methodCount;
      uint methodStart = this.PEFileReader.GetMethodInformation(moduleType.TypeDefRowId, out methodCount);
      uint methodEnd = methodStart + methodCount;
      if (this.PEFileReader.UseMethodPtrTable) {
        uint numberOfMethodPtrRows = this.PEFileReader.MethodPtrTable.NumberOfRows;
        for (uint methodIter = methodStart; methodIter < methodEnd && methodIter <= numberOfMethodPtrRows; ++methodIter) {
          uint methodRowId = this.PEFileReader.MethodPtrTable.GetMethodFor(methodIter);
          var moduleMethod = this.ModuleMethodArray[methodRowId];
          if (moduleMethod == null) {
            moduleMethod = this.CreateMethod(methodRowId, moduleType);
            this.ModuleMethodArray[methodRowId] = moduleMethod;
          }
        }
      } else {
        uint numberOfMethodRows = this.PEFileReader.MethodTable.NumberOfRows;
        for (uint methodIter = methodStart; methodIter < methodEnd && methodIter <= numberOfMethodRows; ++methodIter) {
          var moduleMethod = this.ModuleMethodArray[methodIter];
          if (moduleMethod == null) {
            moduleMethod = this.CreateMethod(methodIter, moduleType);
            this.ModuleMethodArray[methodIter] = moduleMethod;
          }
        }
      }
    }
    internal IEnumerable<IMethodDefinition> GetMethodsOfType(
      TypeBase moduleType
    ) {
      uint methodCount;
      uint methodStart = this.PEFileReader.GetMethodInformation(moduleType.TypeDefRowId, out methodCount);
      uint methodEnd = methodStart + methodCount;
      if (this.PEFileReader.UseMethodPtrTable) {
        uint numberOfMethodPtrRows = this.PEFileReader.MethodPtrTable.NumberOfRows;
        for (uint methodIter = methodStart; methodIter < methodEnd && methodIter <= numberOfMethodPtrRows; ++methodIter) {
          uint methodRowId = this.PEFileReader.MethodPtrTable.GetMethodFor(methodIter);
          yield return this.ModuleMethodArray[methodRowId];
        }
      } else {
        uint numberOfMethodRows = this.PEFileReader.MethodTable.NumberOfRows;
        for (uint methodIter = methodStart; methodIter < methodEnd && methodIter <= numberOfMethodRows; ++methodIter) {
          yield return this.ModuleMethodArray[methodIter];
        }
      }
    }
    void LoadEventsOfType(
      TypeBase moduleType
    ) {
      uint eventCount;
      uint eventStart = this.PEFileReader.GetEventInformation(moduleType.TypeDefRowId, out eventCount);
      uint eventEnd = eventStart + eventCount;
      if (this.PEFileReader.UseEventPtrTable) {
        uint numberOfEventPtrRows = this.PEFileReader.EventPtrTable.NumberOfRows;
        for (uint eventIter = eventStart; eventIter < eventEnd && eventIter <= numberOfEventPtrRows; ++eventIter) {
          uint eventRowId = this.PEFileReader.EventPtrTable.GetEventFor(eventIter);
          EventDefinition moduleEvent = this.ModuleEventArray[eventRowId];
          if (moduleEvent == null) {
            moduleEvent = this.CreateEvent(eventRowId, moduleType);
            this.ModuleEventArray[eventRowId] = moduleEvent;
          }
        }
      } else {
        uint numberOfEventRows = this.PEFileReader.EventTable.NumberOfRows;
        for (uint eventIter = eventStart; eventIter < eventEnd && eventIter <= numberOfEventRows; ++eventIter) {
          EventDefinition moduleEvent = this.ModuleEventArray[eventIter];
          if (moduleEvent == null) {
            moduleEvent = this.CreateEvent(eventIter, moduleType);
            this.ModuleEventArray[eventIter] = moduleEvent;
          }
        }
      }
    }
    internal IEnumerable<IEventDefinition> GetEventsOfType(
      TypeBase moduleType
    ) {
      uint eventCount;
      uint eventStart = this.PEFileReader.GetEventInformation(moduleType.TypeDefRowId, out eventCount);
      uint eventEnd = eventStart + eventCount;
      if (this.PEFileReader.UseEventPtrTable) {
        uint numberOfEventPtrRows = this.PEFileReader.EventPtrTable.NumberOfRows;
        for (uint eventIter = eventStart; eventIter < eventEnd && eventIter <= numberOfEventPtrRows; ++eventIter) {
          uint eventRowId = this.PEFileReader.EventPtrTable.GetEventFor(eventIter);
          yield return this.ModuleEventArray[eventRowId];
        }
      } else {
        uint numberOfEventRows = this.PEFileReader.EventTable.NumberOfRows;
        for (uint eventIter = eventStart; eventIter < eventEnd && eventIter <= numberOfEventRows; ++eventIter) {
          yield return this.ModuleEventArray[eventIter];
        }
      }
    }
    void LoadPropertiesOfType(
      TypeBase moduleType
    ) {
      uint propertyCount;
      uint propertyStart = this.PEFileReader.GetPropertyInformation(moduleType.TypeDefRowId, out propertyCount);
      uint propertyEnd = propertyStart + propertyCount;
      if (this.PEFileReader.UsePropertyPtrTable) {
        uint numberOfPropertyPtrRows = this.PEFileReader.PropertyPtrTable.NumberOfRows;
        for (uint propertyIter = propertyStart; propertyIter < propertyEnd && propertyIter <= numberOfPropertyPtrRows; ++propertyIter) {
          uint propertyRowId = this.PEFileReader.PropertyPtrTable.GetPropertyFor(propertyIter);
          PropertyDefinition moduleProperty = this.ModulePropertyArray[propertyRowId];
          if (moduleProperty == null) {
            moduleProperty = this.CreateProperty(propertyRowId, moduleType);
            this.ModulePropertyArray[propertyRowId] = moduleProperty;
          }
        }
      } else {
        uint numberOfPropertyRows = this.PEFileReader.PropertyTable.NumberOfRows;
        for (uint propertyIter = propertyStart; propertyIter < propertyEnd && propertyIter <= numberOfPropertyRows; ++propertyIter) {
          PropertyDefinition moduleProperty = this.ModulePropertyArray[propertyIter];
          if (moduleProperty == null) {
            moduleProperty = this.CreateProperty(propertyIter, moduleType);
            this.ModulePropertyArray[propertyIter] = moduleProperty;
          }
        }
      }
    }
    internal IEnumerable<IPropertyDefinition> GetPropertiesOfType(
      TypeBase moduleType
    ) {
      uint propertyCount;
      uint propertyStart = this.PEFileReader.GetPropertyInformation(moduleType.TypeDefRowId, out propertyCount);
      uint propertyEnd = propertyStart + propertyCount;
      if (this.PEFileReader.UsePropertyPtrTable) {
        uint numberOfPropertyPtrRows = this.PEFileReader.PropertyPtrTable.NumberOfRows;
        for (uint propertyIter = propertyStart; propertyIter < propertyEnd && propertyIter <= numberOfPropertyPtrRows; ++propertyIter) {
          uint propertyRowId = this.PEFileReader.PropertyPtrTable.GetPropertyFor(propertyIter);
          yield return this.ModulePropertyArray[propertyRowId];
        }
      } else {
        uint numberOfPropertyRows = this.PEFileReader.PropertyTable.NumberOfRows;
        for (uint propertyIter = propertyStart; propertyIter < propertyEnd && propertyIter <= numberOfPropertyRows; ++propertyIter) {
          yield return this.ModulePropertyArray[propertyIter];
        }
      }
    }
    //   Caller must lock this
    internal void LoadMembersOfType(
      TypeBase moduleType
    ) {
      this.LoadNestedTypesOfType(moduleType);
      this.LoadFieldsOfType(moduleType);
      this.LoadMethodsOfType(moduleType);
      this.LoadEventsOfType(moduleType);
      this.LoadPropertiesOfType(moduleType);
    }
    internal IMethodDefinition/*?*/ GetMethodDefAtRow(
      uint methodDefRowId
    ) {
      if (methodDefRowId == 0 || methodDefRowId > this.PEFileReader.MethodTable.NumberOfRows) {
        return null;
      }
      if (this.ModuleMethodArray[methodDefRowId] == null) {
        lock (GlobalLock.LockingObject) {
          if (this.ModuleMethodArray[methodDefRowId] == null) {
            uint methodRow = methodDefRowId;
            if (this.PEFileReader.UseMethodPtrTable) {
              methodRow = this.PEFileReader.MethodPtrTable.GetRowIdForMethodDefRow(methodDefRowId);
            }
            uint typeDefRowId = this.PEFileReader.TypeDefTable.FindTypeContainingMethod(methodRow, this.ModuleMethodArray.Length);
            if (typeDefRowId == 0 || typeDefRowId > this.PEFileReader.TypeDefTable.NumberOfRows) {
              // TODO: MD ERRor
            }
            TypeBase/*?*/ typeDef = this.GetTypeDefinitionAtRowWorker(typeDefRowId);
            if (typeDef == null) {
              return null;
            }
            typeDef.LoadMembers();
          }
        }
      }
      IMethodDefinition/*?*/ mm = this.ModuleMethodArray[methodDefRowId];
      return mm;
    }
    internal FieldDefinition/*?*/ GetFieldDefAtRow(
      uint fieldDefRowId
    ) {
      if (fieldDefRowId == 0 || fieldDefRowId > this.PEFileReader.FieldTable.NumberOfRows) {
        return null;
      }
      if (this.ModuleFieldArray[fieldDefRowId] == null) {
        lock (GlobalLock.LockingObject) {
          if (this.ModuleFieldArray[fieldDefRowId] == null) {
            uint fieldRow = fieldDefRowId;
            if (this.PEFileReader.UseFieldPtrTable) {
              fieldRow = this.PEFileReader.FieldPtrTable.GetRowIdForFieldDefRow(fieldDefRowId);
            }
            uint typeDefRowId = this.PEFileReader.TypeDefTable.FindTypeContainingField(fieldRow, this.ModuleFieldArray.Length);
            if (typeDefRowId == 0 || typeDefRowId > this.PEFileReader.TypeDefTable.NumberOfRows) {
              // TODO: MD ERRor
            }
            TypeBase/*?*/ typeDef = this.GetTypeDefinitionAtRowWorker(typeDefRowId);
            if (typeDef == null) {
              return null;
            }
            typeDef.LoadMembers();
          }
        }
      }
      FieldDefinition/*?*/ fd = this.ModuleFieldArray[fieldDefRowId];
      return fd;
    }
    #endregion Type Member Definition Level loading/Conversion

    #region Global Methods/Fields
    FieldDefinition CreateGlobalField(
      uint fieldDefRowId
    ) {
      Debug.Assert(fieldDefRowId > 0 && fieldDefRowId <= this.PEFileReader.FieldTable.NumberOfRows);
      FieldRow fieldRow = this.PEFileReader.FieldTable[fieldDefRowId];
      string fieldNameStr = this.PEFileReader.StringStream[fieldRow.Name];
      IName fieldTypeMembName = this.NameTable.GetNameFor(fieldNameStr);
      int lastDot = fieldNameStr.LastIndexOf('.');
      Namespace containingNamespace;
      if (lastDot <= 0) {   //  .cctor etc should be preserved. Thus not == -1...
        containingNamespace = this.RootModuleNamespace;
      } else {
        string namespacePrefix = fieldNameStr.Substring(0, lastDot);
        containingNamespace = this.GetNamespaceForString(namespacePrefix);
        fieldNameStr = fieldNameStr.Substring(lastDot + 1, fieldNameStr.Length - lastDot - 1);
      }
      IName globalFieldName = this.NameTable.GetNameFor(fieldNameStr);
      GlobalFieldDefinition globalField = new GlobalFieldDefinition(this, fieldTypeMembName, this._Module_, fieldDefRowId, fieldRow.Flags, globalFieldName, containingNamespace);
      if (fieldTypeMembName.UniqueKey != this.ModuleReader._Deleted_.UniqueKey) {
        this._Module_.AddMember(globalField);
        containingNamespace.AddMember(globalField);
      }
      return globalField;
    }
    MethodDefinition CreateGlobalMethod(
      uint methodDefRowId
    ) {
      Debug.Assert(methodDefRowId > 0 && methodDefRowId <= this.PEFileReader.MethodTable.NumberOfRows);
      MethodRow methodRow = this.PEFileReader.MethodTable[methodDefRowId];
      string methodNameStr = this.PEFileReader.StringStream[methodRow.Name];
      IName methodTypeMembName = this.NameTable.GetNameFor(methodNameStr);
      int lastDot = methodNameStr.LastIndexOf('.');
      Namespace containingNamespace;
      if (lastDot <= 0) {   //  .cctor etc should be preserved. Thus not == -1...
        containingNamespace = this.RootModuleNamespace;
      } else {
        string namespacePrefix = methodNameStr.Substring(0, lastDot);
        containingNamespace = this.GetNamespaceForString(namespacePrefix);
        methodNameStr = methodNameStr.Substring(lastDot + 1, methodNameStr.Length - lastDot - 1);
      }
      IName globalMethodName = this.NameTable.GetNameFor(methodNameStr);
      uint genericParamRowIdStart;
      uint genericParamRowIdEnd;
      this.GetGenericParamInfoForMethod(methodDefRowId, out genericParamRowIdStart, out genericParamRowIdEnd);
      MethodDefinition globalMethod;
      INamespaceMember nsMember;
      if (genericParamRowIdStart == 0) {
        GlobalNonGenericMethod globalNGMethod = new GlobalNonGenericMethod(this, methodTypeMembName, this._Module_, methodDefRowId, methodRow.Flags, methodRow.ImplFlags, globalMethodName, containingNamespace);
        globalMethod = globalNGMethod;
        nsMember = globalNGMethod;
      } else {
        GlobalGenericMethod globalGMethod = new GlobalGenericMethod(this, methodTypeMembName, this._Module_, methodDefRowId, methodRow.Flags, methodRow.ImplFlags, genericParamRowIdStart, genericParamRowIdEnd, globalMethodName, containingNamespace);
        globalMethod = globalGMethod;
        nsMember = globalGMethod;
      }
      if (methodTypeMembName.UniqueKey != this.ModuleReader._Deleted_.UniqueKey) {
        this._Module_.AddMember(globalMethod);
        containingNamespace.AddMember(nsMember);
      }
      return globalMethod;
    }
    internal void LoadMembersOf_Module_Type() {
      //  Load all global fields...
      uint fieldCount;
      uint fieldStart = this.PEFileReader.GetFieldInformation(this._Module_.TypeDefRowId, out fieldCount);
      uint fieldEnd = fieldStart + fieldCount;
      if (this.PEFileReader.UseFieldPtrTable) {
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd; ++fieldIter) {
          uint fieldRowId = this.PEFileReader.FieldPtrTable.GetFieldFor(fieldIter);
          FieldDefinition globalField = this.ModuleFieldArray[fieldRowId];
          if (globalField == null) {
            globalField = this.CreateGlobalField(fieldRowId);
            this.ModuleFieldArray[fieldRowId] = globalField;
          }
        }
      } else {
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd; ++fieldIter) {
          FieldDefinition globalField = this.ModuleFieldArray[fieldIter];
          if (globalField == null) {
            globalField = this.CreateGlobalField(fieldIter);
            this.ModuleFieldArray[fieldIter] = globalField;
          }
        }
      }
      //  Load all global methods...
      uint methodCount;
      uint methodStart = this.PEFileReader.GetMethodInformation(this._Module_.TypeDefRowId, out methodCount);
      uint methodEnd = methodStart + methodCount;
      if (this.PEFileReader.UseMethodPtrTable) {
        for (uint methodIter = methodStart; methodIter < methodEnd; ++methodIter) {
          uint methodRowId = this.PEFileReader.MethodPtrTable.GetMethodFor(methodIter);
          var globalMethod = this.ModuleMethodArray[methodRowId];
          if (globalMethod == null) {
            globalMethod = this.CreateGlobalMethod(methodRowId);
            this.ModuleMethodArray[methodRowId] = globalMethod;
          }
        }
      } else {
        for (uint methodIter = methodStart; methodIter < methodEnd; ++methodIter) {
          var globalMethod = this.ModuleMethodArray[methodIter];
          if (globalMethod == null) {
            globalMethod = this.CreateGlobalMethod(methodIter);
            this.ModuleMethodArray[methodIter] = globalMethod;
          }
        }
      }


    }
    #endregion Global Methods/Fields

    #region Type Member Information
    internal void GetGenericParamInfoForMethod(
      uint methodDefRowId,
      out uint genericParamRowIdStart,
      out uint genericParamRowIdEnd
    ) {
      ushort genericParamCount;
      genericParamRowIdStart = this.PEFileReader.GenericParamTable.FindGenericParametersForMethod(methodDefRowId, out genericParamCount);
      genericParamRowIdEnd = genericParamRowIdStart + genericParamCount;
    }
    internal FieldSignatureConverter GetFieldSignature(
      FieldDefinition moduleField
    ) {
      uint signatureBlobOffset = this.PEFileReader.FieldTable.GetSignature(moduleField.FieldDefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really field signature there.
      FieldSignatureConverter fieldSigConv = new FieldSignatureConverter(this, moduleField, memoryReader);
      return fieldSigConv;
    }
    internal PropertySignatureConverter GetPropertySignature(
      PropertyDefinition moduleProperty
    ) {
      uint signatureBlobOffset = this.PEFileReader.PropertyTable.GetSignature(moduleProperty.PropertyRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really field signature there.
      PropertySignatureConverter propertySigConv = new PropertySignatureConverter(this, moduleProperty, memoryReader);
      return propertySigConv;
    }
    internal MethodDefSignatureConverter GetMethodSignature(
      MethodDefinition moduleMethod
    ) {
      uint signatureBlobOffset = this.PEFileReader.MethodTable.GetSignature(moduleMethod.MethodDefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really field signature there.
      MethodDefSignatureConverter methodSigConv = new MethodDefSignatureConverter(this, moduleMethod, memoryReader);
      if (methodSigConv.GenericParamCount != moduleMethod.GenericParameterCount) {
        //  Error...
      }
      return methodSigConv;
    }
    internal ushort GetMethodParameterCount(
      MethodDefinition moduleMethod
    ) {
      uint signatureBlobOffset = this.PEFileReader.MethodTable.GetSignature(moduleMethod.MethodDefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      byte firstByte = memoryReader.ReadByte();
      if (SignatureHeader.IsGeneric(firstByte)) {
        memoryReader.ReadCompressedUInt32();
      }
      return (ushort)memoryReader.ReadCompressedUInt32();
    }
    internal ITypeReference/*?*/ GetEventType(
      EventDefinition moduleEvent
    ) {
      uint typeToken = this.PEFileReader.EventTable.GetEventType(moduleEvent.EventRowId);
      return this.GetTypeReferenceForToken(moduleEvent, typeToken);
    }
    internal uint GetFieldOffset(
      FieldDefinition fieldDefinition
    ) {
      return this.PEFileReader.FieldLayoutTable.GetOffset(fieldDefinition.FieldDefRowId);
    }
    internal int GetFieldSequenceNumber(
      FieldDefinition fieldDefinition
    ) {
      uint fieldCount;
      uint fieldStart = this.PEFileReader.GetFieldInformation(fieldDefinition.OwningModuleType.TypeDefRowId, out fieldCount);
      if (this.PEFileReader.UseFieldPtrTable) {
        uint fieldDefRowId = fieldDefinition.FieldDefRowId;
        uint fieldEnd = fieldStart + fieldCount;
        for (uint fieldIter = fieldStart; fieldIter < fieldEnd; ++fieldIter) {
          uint currFieldRowId = this.PEFileReader.FieldPtrTable.GetFieldFor(fieldIter);
          if (currFieldRowId == fieldDefRowId) {
            return (int)(fieldIter - fieldStart);
          }
        }
        //  Error...
        return 0;
      } else {
        return (int)(fieldDefinition.FieldDefRowId - fieldStart);
      }
    }
    internal ISectionBlock GetFieldMapping(
      FieldDefinition fieldDefinition
    ) {
      int rva = this.PEFileReader.FieldRVATable.GetFieldRVA(fieldDefinition.FieldDefRowId);
      if (rva == -1) {
        return Dummy.SectionBlock;
      }
      PESectionKind sectionKind = PESectionKind.Illegal;
      switch (this.PEFileReader.RVAToSubSectionName(rva)) {
        case ".text": sectionKind = PESectionKind.Text; break;
        case ".sdata": sectionKind = PESectionKind.StaticData; break;
        case ".tls": sectionKind = PESectionKind.ThreadLocalStorage; break;
        case ".rdata": sectionKind = PESectionKind.ConstantData; break;
        case ".cover": sectionKind = PESectionKind.CoverageData; break;
        case ".datax": sectionKind = PESectionKind.ExtendedData; break;
        case ".data": sectionKind = PESectionKind.ExtendedData; break; //TODO: instead of an enum, use an IName.
      }
      int sizeOfField = (int)this.GetFieldSizeIfPossibleToDoSoWithoutResolving(fieldDefinition.Type);
      if (sizeOfField == 0) {
        int nextNearestRVA = this.PEFileReader.FieldRVATable.GetNextRVA(rva);
        if (sectionKind == PESectionKind.Text) {
          int nextNearestMethodRVA = this.PEFileReader.MethodTable.GetNextRVA(rva);
          if (nextNearestMethodRVA != -1 && nextNearestMethodRVA < nextNearestRVA)
            nextNearestRVA = nextNearestMethodRVA;
          else {
            var metadataRVA = this.PEFileReader.COR20Header.MetaDataDirectory.RelativeVirtualAddress;
            if (rva < metadataRVA && metadataRVA < nextNearestRVA)
              nextNearestRVA = metadataRVA;
          }
        }
        if (nextNearestRVA == -1 || !this.PEFileReader.RVAsInSameSection(rva, nextNearestRVA)) {
          sizeOfField = this.PEFileReader.GetSizeOfRemainderOfSectionContaining(rva);
        } else {
          sizeOfField = nextNearestRVA - rva;
        }
      }
      SubSection subSection = this.PEFileReader.RVAToSubSection(rva, (int)sizeOfField);
      if (sectionKind == PESectionKind.Illegal) {
        return Dummy.SectionBlock;
      }
      return new SectionBlock(sectionKind, subSection.Offset, subSection.MemoryBlock);
    }
    private uint GetFieldSizeIfPossibleToDoSoWithoutResolving(ITypeReference typeReference) {
      switch (typeReference.TypeCode) {
        case PrimitiveTypeCode.Boolean:
          return sizeof(Boolean);
        case PrimitiveTypeCode.Char:
          return sizeof(Char);
        case PrimitiveTypeCode.Int16:
          return sizeof(Int16);
        case PrimitiveTypeCode.Int32:
          return sizeof(Int32);
        case PrimitiveTypeCode.Int8:
          return sizeof(SByte);
        case PrimitiveTypeCode.UInt16:
          return sizeof(UInt16);
        case PrimitiveTypeCode.UInt32:
          return sizeof(UInt32);
        case PrimitiveTypeCode.UInt8:
          return sizeof(Byte);
        case PrimitiveTypeCode.Int64:
          return sizeof(Int64);
        case PrimitiveTypeCode.UInt64:
          return sizeof(UInt64);
        case PrimitiveTypeCode.IntPtr:
          return this.pointerSize;
        case PrimitiveTypeCode.UIntPtr:
          return this.pointerSize;
        case PrimitiveTypeCode.Float32:
          return sizeof(Single);
        case PrimitiveTypeCode.Float64:
          return sizeof(Double);
        case PrimitiveTypeCode.Pointer:
          return this.pointerSize;
      }
      var modifiedTypeReference = typeReference as IModifiedTypeReference;
      if (modifiedTypeReference != null) typeReference = modifiedTypeReference.UnmodifiedType;
      ITypeDefinition/*?*/ typeDefinition = typeReference as ITypeDefinition;
      if (typeDefinition != null) return TypeHelper.SizeOfType(typeDefinition);
      return 0;
    }
    internal void GetSemanticInfoForProperty(
      uint propertyRowId,
      out uint methodSemanticRowIdStart,
      out uint methodSemanticRowIdEnd
    ) {
      ushort methodCount;
      methodSemanticRowIdStart = this.PEFileReader.MethodSemanticsTable.FindSemanticMethodsForProperty(propertyRowId, out methodCount);
      methodSemanticRowIdEnd = methodSemanticRowIdStart + methodCount;
    }
    internal void GetSemanticInfoForEvent(
      uint eventRowId,
      out uint methodSemanticRowIdStart,
      out uint methodSemanticRowIdEnd
    ) {
      ushort methodCount;
      methodSemanticRowIdStart = this.PEFileReader.MethodSemanticsTable.FindSemanticMethodsForEvent(eventRowId, out methodCount);
      methodSemanticRowIdEnd = methodSemanticRowIdStart + methodCount;
    }
    internal IMethodDefinition/*?*/ GetPropertyGetterOrSetterMethod(
      PropertyDefinition propertyDefinition,
      MethodSemanticsFlags getterOrSetterFlag
    ) {
      uint methodSemanticRowIdStart;
      uint methodSemanticRowIdEnd;
      this.GetSemanticInfoForProperty(propertyDefinition.PropertyRowId, out methodSemanticRowIdStart, out methodSemanticRowIdEnd);
      for (uint methSemRowIdIter = methodSemanticRowIdStart; methSemRowIdIter < methodSemanticRowIdEnd; ++methSemRowIdIter) {
        MethodSemanticsRow methodSemanticRow = this.PEFileReader.MethodSemanticsTable[methSemRowIdIter];
        if (methodSemanticRow.SemanticsFlag == getterOrSetterFlag) {
          return this.GetMethodDefAtRow(methodSemanticRow.Method);
        }
      }
      return null;
    }
    internal IEnumerable<IMethodReference> GetPropertyAccessorMethods(
      PropertyDefinition propertyDefinition
    ) {
      uint methodSemanticRowIdStart;
      uint methodSemanticRowIdEnd;
      this.GetSemanticInfoForProperty(propertyDefinition.PropertyRowId, out methodSemanticRowIdStart, out methodSemanticRowIdEnd);
      for (uint methSemRowIdIter = methodSemanticRowIdStart; methSemRowIdIter < methodSemanticRowIdEnd; ++methSemRowIdIter) {
        MethodSemanticsRow methodSemanticRow = this.PEFileReader.MethodSemanticsTable[methSemRowIdIter];
        IMethodDefinition/*?*/ methodDef = this.GetMethodDefAtRow(methodSemanticRow.Method);
        if (methodDef != null) {
          yield return methodDef;
        } else {
          yield return Dummy.MethodReference;
        }
      }
    }
    internal IMethodDefinition/*?*/ GetEventAddOrRemoveOrFireMethod(
      EventDefinition eventDefinition,
      MethodSemanticsFlags addOrRemoveOrFireFlag
    ) {
      uint methodSemanticRowIdStart;
      uint methodSemanticRowIdEnd;
      this.GetSemanticInfoForEvent(eventDefinition.EventRowId, out methodSemanticRowIdStart, out methodSemanticRowIdEnd);
      for (uint methSemRowIdIter = methodSemanticRowIdStart; methSemRowIdIter < methodSemanticRowIdEnd; ++methSemRowIdIter) {
        MethodSemanticsRow methodSemanticRow = this.PEFileReader.MethodSemanticsTable[methSemRowIdIter];
        if (methodSemanticRow.SemanticsFlag == addOrRemoveOrFireFlag) {
          return this.GetMethodDefAtRow(methodSemanticRow.Method);
        }
      }
      return null;
    }
    internal IEnumerable<IMethodReference> GetEventAccessorMethods(
      EventDefinition eventDefinition
    ) {
      uint methodSemanticRowIdStart;
      uint methodSemanticRowIdEnd;
      this.GetSemanticInfoForEvent(eventDefinition.EventRowId, out methodSemanticRowIdStart, out methodSemanticRowIdEnd);
      for (uint methSemRowIdIter = methodSemanticRowIdStart; methSemRowIdIter < methodSemanticRowIdEnd; ++methSemRowIdIter) {
        MethodSemanticsRow methodSemanticRow = this.PEFileReader.MethodSemanticsTable[methSemRowIdIter];
        IMethodDefinition/*?*/ methodDef = this.GetMethodDefAtRow(methodSemanticRow.Method);
        if (methodDef != null) {
          yield return methodDef;
        } else {
          yield return Dummy.MethodReference;
        }
      }
    }
    internal IMetadataConstant GetDefaultValue(
      MetadataObject metadataObject
    ) {
      uint constRowId = this.PEFileReader.ConstantTable.GetConstantRowId(metadataObject.TokenValue);
      if (constRowId == 0)
        return Dummy.Constant;
      ConstantRow constRow = this.PEFileReader.ConstantTable[constRowId];
      MemoryBlock constValueMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(constRow.Value);
      MemoryReader memoryReader = new MemoryReader(constValueMemoryBlock);
      switch (constRow.Type) {
        case ElementType.Boolean: {
            byte val = memoryReader.ReadByte();
            return new ConstantExpression(this.PlatformType.SystemBoolean, val != 0);
          }
        case ElementType.Char:
          return new ConstantExpression(this.PlatformType.SystemChar, memoryReader.ReadChar());
        case ElementType.Int8:
          return new ConstantExpression(this.PlatformType.SystemInt8, memoryReader.ReadSByte());
        case ElementType.Int16:
          return new ConstantExpression(this.PlatformType.SystemInt16, memoryReader.ReadInt16());
        case ElementType.Int32:
          return new ConstantExpression(this.PlatformType.SystemInt32, memoryReader.ReadInt32());
        case ElementType.Int64:
          return new ConstantExpression(this.PlatformType.SystemInt64, memoryReader.ReadInt64());
        case ElementType.UInt8:
          return new ConstantExpression(this.PlatformType.SystemUInt8, memoryReader.ReadByte());
        case ElementType.UInt16:
          return new ConstantExpression(this.PlatformType.SystemUInt16, memoryReader.ReadUInt16());
        case ElementType.UInt32:
          return new ConstantExpression(this.PlatformType.SystemUInt32, memoryReader.ReadUInt32());
        case ElementType.UInt64:
          return new ConstantExpression(this.PlatformType.SystemUInt64, memoryReader.ReadUInt64());
        case ElementType.Single:
          return new ConstantExpression(this.PlatformType.SystemFloat32, memoryReader.ReadSingle());
        case ElementType.Double:
          return new ConstantExpression(this.PlatformType.SystemFloat64, memoryReader.ReadDouble());
        case ElementType.String: {
            int byteLen = memoryReader.Length;
            string/*?*/ value;
            if (byteLen == -1) {
              value = null;
            } else if (byteLen == 0) {
              value = string.Empty;
            } else {
              value = memoryReader.ReadUTF16WithSize(byteLen);
            }
            return new ConstantExpression(this.PlatformType.SystemString, value);
          }
        case ElementType.Class:
          return new ConstantExpression(this.PlatformType.SystemObject, null);
      }
      //  MDError...
      return Dummy.Constant;
    }
    internal IMarshallingInformation GetMarshallingInformation(
      MetadataObject metadataObject
    ) {
      uint fieldMarshalRowId = this.PEFileReader.FieldMarshalTable.GetFieldMarshalRowId(metadataObject.TokenValue);
      if (fieldMarshalRowId == 0)
        return Dummy.MarshallingInformation;
      FieldMarshalRow fieldMarshalRow = this.PEFileReader.FieldMarshalTable[fieldMarshalRowId];
      MemoryBlock fieldMarshalMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(fieldMarshalRow.NativeType);
      MemoryReader memoryReader = new MemoryReader(fieldMarshalMemoryBlock);
      System.Runtime.InteropServices.UnmanagedType unmanagedType = (System.Runtime.InteropServices.UnmanagedType)memoryReader.ReadByte();
      if (memoryReader.NotEndOfBytes) {
        if (unmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray) {
          uint numElements = (uint)memoryReader.ReadCompressedUInt32();
          System.Runtime.InteropServices.UnmanagedType elementType = System.Runtime.InteropServices.UnmanagedType.AsAny;
          if (memoryReader.NotEndOfBytes)
            elementType = (System.Runtime.InteropServices.UnmanagedType)memoryReader.ReadByte();
          return new ByValArrayMarshallingInformation(elementType, numElements);
        } else if (unmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler) {
          string marshallerName;
          string marshallerArgument;
          memoryReader.ReadInt16(); //  Deliberate Skip...
          int byteLen = memoryReader.ReadCompressedUInt32();
          if (byteLen == -1 || byteLen == 0)
            marshallerName = string.Empty;
          else
            marshallerName = memoryReader.ReadUTF8WithSize(byteLen);
          ITypeReference/*?*/ marshaller = this.GetSerializedTypeNameAsTypeReference(marshallerName);
          if (marshaller == null) marshaller = Dummy.TypeReference;
          byteLen = memoryReader.ReadCompressedUInt32();
          if (byteLen == -1 || byteLen == 0)
            marshallerArgument = string.Empty;
          else
            marshallerArgument = memoryReader.ReadUTF8WithSize(byteLen);
          return new CustomMarshallingInformation(marshaller, marshallerArgument);
        } else if (unmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray) {
          System.Runtime.InteropServices.UnmanagedType elementType = (System.Runtime.InteropServices.UnmanagedType)memoryReader.ReadByte();
          int paramIndex = -1;
          uint flag = 0;
          uint numElements = 0;
          if (memoryReader.NotEndOfBytes)
            paramIndex = (int)memoryReader.ReadCompressedUInt32();
          if (memoryReader.NotEndOfBytes)
            numElements = (uint)memoryReader.ReadCompressedUInt32();
          if (memoryReader.NotEndOfBytes) {
            flag = (uint)memoryReader.ReadCompressedUInt32();
            if (flag == 0) {
              //TODO: check that paramIndex is 0
              paramIndex = -1; //paramIndex is just a place holder so that numElements can be present
            }
          }
          return new LPArrayMarshallingInformation(elementType, paramIndex, numElements);
        } else if (unmanagedType == System.Runtime.InteropServices.UnmanagedType.SafeArray) {
          System.Runtime.InteropServices.VarEnum elementType = (System.Runtime.InteropServices.VarEnum)memoryReader.ReadByte();
          string subType = string.Empty;
          if (memoryReader.NotEndOfBytes) {
            int byteLen = memoryReader.ReadCompressedUInt32();
            if (byteLen > 0)
              subType = memoryReader.ReadUTF8WithSize(byteLen);
          }
          ITypeReference/*?*/ subTypeRef = this.GetSerializedTypeNameAsTypeReference(subType);
          if (subTypeRef == null) subTypeRef = Dummy.TypeReference;
          return new SafeArrayMarshallingInformation(elementType, subTypeRef);
        } else if (unmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValTStr) {
          uint numElements = (uint)memoryReader.ReadCompressedUInt32();
          return new ByValTStrMarshallingInformation(numElements);
        } else if (unmanagedType == System.Runtime.InteropServices.UnmanagedType.Interface) {
          uint iidParameterIndex = (uint)memoryReader.ReadCompressedUInt32();
          return new IidParameterIndexMarshallingInformation(iidParameterIndex);
        } else {
          //TODO: error blob should not have extra info unless one of the above types.
        }
      }
      return new SimpleMarshallingInformation(unmanagedType);
    }
    internal IPlatformInvokeInformation GetPlatformInvokeInformation(
      MethodDefinition methodDefinition
    ) {
      uint methodImplRowId = this.PEFileReader.ImplMapTable.FindImplForMethod(methodDefinition.MethodDefRowId);
      if (methodImplRowId == 0) {
        return Dummy.PlatformInvokeInformation;
      }
      ImplMapRow implMapRow = this.PEFileReader.ImplMapTable[methodImplRowId];
      ModuleReference/*?*/ moduleReference = this.GetModuleReferenceAt(implMapRow.ImportScope);
      if (moduleReference == null) {
        return Dummy.PlatformInvokeInformation;
      }
      return new PlatformInvokeInformation(implMapRow.PInvokeMapFlags, this.GetNameFromOffset(implMapRow.ImportName), moduleReference);
    }
    internal IMethodBody GetMethodBody(
      MethodDefinition methodDefinition
    ) {
      MethodIL/*?*/ methodIL = this.PEFileReader.GetMethodIL(methodDefinition.MethodDefRowId);
      if (methodIL == null)
        return Dummy.MethodBody;
      ILReader ilReader = new ILReader(methodDefinition, methodIL);
      bool ret = ilReader.ReadIL();
      if (!ret)
        return Dummy.MethodBody;
      return ilReader.MethodBody;
    }
    #endregion Type Member Information

    #region Member Reference Information
    readonly MemberReference/*?*/[] ModuleMemberReferenceArray;
    readonly MemberReference/*?*/[] UnspecializedMemberReferenceArray;
    readonly DoubleHashtable<ISpecializedFieldReference>/*?*/ SpecializedFieldHashtable;
    readonly DoubleHashtable<ISpecializedMethodReference>/*?*/ SpecializedMethodHashtable;
    readonly DoubleHashtable<IGenericMethodInstanceReference>/*?*/ ModuleMethodSpecHashtable;
    //^ invariant this.PEFileReader.MethodSpecTable.NumberOfRows >= 1 ==> this.MethodSpecHashtable != null;

    internal ITypeMemberReference/*?*/ GetModuleMemberReferenceAtRowWorker(
      MetadataObject owningObject,
      uint memberRefRowId
    ) {
      if (memberRefRowId == 0 || memberRefRowId > this.PEFileReader.MemberRefTable.NumberOfRows) {
        return null;
      }
      if (this.ModuleMemberReferenceArray[memberRefRowId] == null) {
        MemberRefRow memberRefRow = this.PEFileReader.MemberRefTable[memberRefRowId];
        uint classTokenType = memberRefRow.Class & TokenTypeIds.TokenTypeMask;
        uint classRowId = memberRefRow.Class & TokenTypeIds.RIDMask;
        ITypeReference/*?*/ parentTypeReference = null;
        switch (classTokenType) {
          case TokenTypeIds.TypeDef:
            parentTypeReference = this.GetTypeDefinitionAtRow(classRowId);
            break;
          case TokenTypeIds.TypeRef:
            parentTypeReference = this.GetTypeRefReferenceAtRow(classRowId);
            break;
          case TokenTypeIds.TypeSpec:
            parentTypeReference = this.GetTypeSpecReferenceAtRow(owningObject, classRowId).UnderlyingModuleTypeReference;
            break;
          case TokenTypeIds.MethodDef: {
              var/*?*/ methodDef = this.GetMethodDefAtRow(classRowId);
              if (methodDef == null) {
                //  Error...
                return null;
              }
              parentTypeReference = methodDef.ContainingType;
              break;
            }
          case TokenTypeIds.ModuleRef: {
              ModuleReference/*?*/ modRef = this.GetModuleReferenceAt(classRowId);
              if (modRef == null) {
                //  MDError
                return null;
              }
              var module = this.ResolveModuleRefReference(modRef) as Module;
              if (module == null) {
                //TODO: MDError...
                return null;
              }
              PEFileToObjectModel modulePEFileToObjectModel = module.PEFileToObjectModel;
              parentTypeReference = modulePEFileToObjectModel._Module_;
              break;
            }
          default: {
              //  MDError...
              return null;
            }
        }
        if (parentTypeReference == null) {
          //  Error...
          return null;
        }
        MemberReference retModuleMemberReference;
        IName name = this.GetNameFromOffset(memberRefRow.Name);
        byte firstByte = this.PEFileReader.BlobStream.GetByteAt(memberRefRow.Signature, 0);
        var genericTypeInstance = parentTypeReference as IGenericTypeInstanceReference;
        var specializedNestedTypeReference = parentTypeReference as ISpecializedNestedTypeReference;
        if (SignatureHeader.IsFieldSignature(firstByte)) {
          if (genericTypeInstance != null || specializedNestedTypeReference != null) {
            //The same memberRef token can be shared by distinct instance references, therefore special caching is required
            FieldReference unspecializedFieldReference = this.UnspecializedMemberReferenceArray[memberRefRowId] as FieldReference;
            if (unspecializedFieldReference == null) {
              unspecializedFieldReference = new FieldReference(this, memberRefRowId, TypeCache.Unspecialize(parentTypeReference), name);
              this.UnspecializedMemberReferenceArray[memberRefRowId] = unspecializedFieldReference;
            }
            uint key1 = parentTypeReference.InternedKey;
            uint key2 = unspecializedFieldReference.InternedKey;
            var specializedField = this.SpecializedFieldHashtable.Find(key1, key2);
            if (specializedField == null) {
              specializedField = new SpecializedFieldReference(parentTypeReference, unspecializedFieldReference, this.InternFactory);
              this.SpecializedFieldHashtable.Add(key1, key2, specializedField);
            }
            return specializedField;
          } else {
            retModuleMemberReference = new FieldReference(this, memberRefRowId, parentTypeReference, name);
          }
        } else if (SignatureHeader.IsMethodSignature(firstByte)) {
          if (genericTypeInstance != null || specializedNestedTypeReference != null) {
            //The same memberRef token can be shared by distinct instance references, therefore special caching is required
            MethodReference unspecializedMethodReference = this.UnspecializedMemberReferenceArray[memberRefRowId] as MethodReference;
            if (unspecializedMethodReference == null) {
              unspecializedMethodReference = new MethodReference(this, memberRefRowId, TypeCache.Unspecialize(parentTypeReference), name, firstByte);
              this.UnspecializedMemberReferenceArray[memberRefRowId] = unspecializedMethodReference;
            }
            uint key1 = parentTypeReference.InternedKey;
            uint key2 = unspecializedMethodReference.InternedKey;
            var specializedMethod = this.SpecializedMethodHashtable.Find(key1, key2);
            if (specializedMethod == null) {
              specializedMethod = new SpecializedMethodReference(parentTypeReference, unspecializedMethodReference, this.InternFactory);
              this.SpecializedMethodHashtable.Add(key1, key2, specializedMethod);
            }
            return specializedMethod;
          } else {
            retModuleMemberReference = new MethodReference(this, memberRefRowId, parentTypeReference, name, firstByte);
          }
        } else {
          //  MD Error
          return null;
        }
        this.ModuleMemberReferenceArray[memberRefRowId] = retModuleMemberReference;
      }
      MemberReference/*?*/ ret = this.ModuleMemberReferenceArray[memberRefRowId];
      return ret;
    }

    internal ITypeMemberReference/*?*/ GetModuleMemberReferenceAtRow(
      MetadataObject owningObject,
      uint memberRefRowId
    ) {
      if (memberRefRowId == 0 || memberRefRowId > this.PEFileReader.MemberRefTable.NumberOfRows) {
        return null;
      }
      if (this.ModuleMemberReferenceArray[memberRefRowId] == null) {
        lock (GlobalLock.LockingObject) {
          //The same memberRef token can sometimes be shared by references that are distinct in the object model.
          //Hence the worker decides whether to cache or not.
          return this.GetModuleMemberReferenceAtRowWorker(owningObject, memberRefRowId);
        }
      }
      MemberReference/*?*/ ret = this.ModuleMemberReferenceArray[memberRefRowId];
      return ret;
    }
    internal MethodRefSignatureConverter GetMethodRefSignature(
      MethodReference moduleMethodReference
    ) {
      uint signatureBlobOffset = this.PEFileReader.MemberRefTable.GetSignature(moduleMethodReference.MemberRefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really method signature there.
      MethodRefSignatureConverter methodRefSigConv = new MethodRefSignatureConverter(this, moduleMethodReference, memoryReader);
      return methodRefSigConv;
    }
    internal int GetMethodRefParameterCount(
      MethodReference moduleMethodReference
    ) {
      uint signatureBlobOffset = this.PEFileReader.MemberRefTable.GetSignature(moduleMethodReference.MemberRefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really method signature there.
      byte firstByte = memoryReader.ReadByte();
      if (SignatureHeader.IsGeneric(firstByte)) {
        memoryReader.ReadCompressedUInt32(); //generic param count
      }
      return memoryReader.ReadCompressedUInt32();
    }
    internal int GetMethodRefGenericParameterCount(
      MethodReference moduleMethodReference
    ) {
      uint signatureBlobOffset = this.PEFileReader.MemberRefTable.GetSignature(moduleMethodReference.MemberRefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really method signature there.
      byte firstByte = memoryReader.ReadByte();
      if (SignatureHeader.IsGeneric(firstByte)) {
        return memoryReader.ReadCompressedUInt32();
      }
      return 0;
    }
    internal FieldSignatureConverter GetFieldRefSignature(
      FieldReference moduleFieldReference
    ) {
      uint signatureBlobOffset = this.PEFileReader.MemberRefTable.GetSignature(moduleFieldReference.MemberRefRowId);
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really field signature there.
      FieldSignatureConverter fieldSigConv = new FieldSignatureConverter(this, moduleFieldReference, memoryReader);
      return fieldSigConv;
    }
    internal IMethodReference/*?*/ GetMethodSpecAtRow(
      MetadataObject owningObject,
      uint methodSpecRowId
    ) {
      if (methodSpecRowId == 0 || methodSpecRowId > this.PEFileReader.MethodSpecTable.NumberOfRows) {
        return null;
      }
      uint ownerId = owningObject.TokenValue;
      IGenericMethodInstanceReference/*?*/ methodSpecReference = this.ModuleMethodSpecHashtable.Find(ownerId, methodSpecRowId);
      if (methodSpecReference == null) {
        lock (GlobalLock.LockingObject) {
          methodSpecReference = this.ModuleMethodSpecHashtable.Find(ownerId, methodSpecRowId);
          if (methodSpecReference == null) {
            MethodSpecRow methodSpecRow = this.PEFileReader.MethodSpecTable[methodSpecRowId];
            uint methToken = methodSpecRow.Method;
            uint tokenKind = methToken & TokenTypeIds.TokenTypeMask;
            uint rowId = methToken & TokenTypeIds.RIDMask;
            IMethodReference/*?*/ moduleMethod;
            if (tokenKind == TokenTypeIds.MethodDef) {
              moduleMethod = this.GetMethodDefAtRow(rowId);
            } else if (tokenKind == TokenTypeIds.MemberRef) {
              moduleMethod = this.GetModuleMemberReferenceAtRow(owningObject, rowId) as IMethodReference;
            } else {
              //  MDError...
              return null;
            }
            if (moduleMethod == null) {
              //  MDError...
              return null;
            }
            //  TODO: error checking offset in range
            MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(methodSpecRow.Instantiation);
            //  TODO: Error checking enough space in signature memoryBlock.
            MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
            //  TODO: Check if this is really a method spec signature there.
            MethodSpecSignatureConverter methodSpecSigConv = new MethodSpecSignatureConverter(this, owningObject, memoryReader);
            methodSpecReference = new GenericMethodInstanceReferenceWithToken(moduleMethod, methodSpecSigConv.GenericTypeArguments, this.InternFactory,
              methodSpecRowId | TokenTypeIds.MethodSpec);
            this.ModuleMethodSpecHashtable.Add(ownerId, methodSpecRowId, methodSpecReference);
          }
        }
      }
      return methodSpecReference;
    }
    //  Caller should lock this...
    internal IMethodReference GetMethodReferenceForToken(
      MetadataObject owningObject,
      uint methodRefToken
    ) {
      uint tokenKind = methodRefToken & TokenTypeIds.TokenTypeMask;
      uint rowId = methodRefToken & TokenTypeIds.RIDMask;
      IMethodReference/*?*/ methRef = null;
      switch (tokenKind) {
        case TokenTypeIds.MethodDef:
          methRef = this.GetMethodDefAtRow(rowId);
          break;
        case TokenTypeIds.MethodSpec:
          methRef = this.GetMethodSpecAtRow(owningObject, rowId);
          break;
        case TokenTypeIds.MemberRef:
          methRef = this.GetModuleMemberReferenceAtRow(owningObject, rowId) as IMethodReference;
          break;
      }
      if (methRef == null) {
        //  Error...
        methRef = Dummy.MethodReference;
      }
      return methRef;
    }
    //  Caller should lock this...
    internal IFieldReference GetFieldReferenceForToken(
      MetadataObject owningObject,
      uint fieldRefToken
    ) {
      uint tokenKind = fieldRefToken & TokenTypeIds.TokenTypeMask;
      uint rowId = fieldRefToken & TokenTypeIds.RIDMask;
      switch (tokenKind) {
        case TokenTypeIds.FieldDef:
          FieldDefinition/*?*/ fieldDef = this.GetFieldDefAtRow(rowId);
          if (fieldDef == null)
            return Dummy.FieldReference;
          else
            return fieldDef;
        case TokenTypeIds.MemberRef: {
            var fieldRef = this.GetModuleMemberReferenceAtRow(owningObject, rowId) as IFieldReference;
            if (fieldRef == null) {
              //  MDError/ILError?
              return Dummy.FieldReference;
            } else {
              return fieldRef;
            }
          }
        default:
          return Dummy.FieldReference;
      }
    }

    internal ITypeReference/*?*/ GetTypeReferenceFromStandaloneSignatureToken(
      MethodDefinition owningObject,
      uint token
    ) {
      uint tokenKind = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      if (tokenKind != TokenTypeIds.Signature) {
        //TODO: error
        return Dummy.TypeReference;
      }
      if (rowId == 0 || rowId > this.PEFileReader.StandAloneSigTable.NumberOfRows) {
        //TODO: error
        return Dummy.TypeReference;
      }
      StandAloneSigRow sigRow = this.PEFileReader.StandAloneSigTable[rowId];
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(sigRow.Signature);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      byte kind = memoryReader.PeekByte(0);
      if (SignatureHeader.IsFieldSignature(kind)) {
        FieldSignatureConverter converter = new FieldSignatureConverter(this, owningObject, memoryReader);
        return converter.TypeReference;
      }
      if (SignatureHeader.IsLocalVarSignature(kind)) {
        MethodBody.MethodBody/*?*/ body = owningObject.Body as MethodBody.MethodBody;
        if (body == null) {
          //TODO: error
          return Dummy.TypeReference;
        }
        LocalVariableSignatureConverter converter = new LocalVariableSignatureConverter(this, body, memoryReader);
        foreach (var loc in converter.LocalVariables) {
          return loc.Type;
        }
      }
      //TODO: error
      return Dummy.TypeReference;
    }

    //  Caller must lock this...
    internal object/*?*/ GetReferenceForToken(
      MetadataObject owningObject,
      uint token
    ) {
      uint tokenKind = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      switch (tokenKind) {
        case TokenTypeIds.TypeDef: {
            if (rowId == 0 || rowId > this.PEFileReader.TypeDefTable.NumberOfRows) {
              //  handle Error
              return null;
            }
            return this.GetTypeDefinitionAtRow(rowId);
          }
        case TokenTypeIds.TypeRef: {
            if (rowId == 0 || rowId > this.PEFileReader.TypeRefTable.NumberOfRows) {
              //  handle Error
              return null;
            }
            return this.GetTypeRefReferenceAtRow(rowId);
          }
        case TokenTypeIds.TypeSpec: {
            if (rowId == 0 || rowId > this.PEFileReader.TypeSpecTable.NumberOfRows) {
              //  handle Error
              return null;
            }
            return this.GetTypeSpecReferenceAtRow(owningObject, rowId).UnderlyingModuleTypeReference;
          }
        case TokenTypeIds.MethodDef:
          if (rowId == 0 || rowId > this.PEFileReader.MethodTable.NumberOfRows) {
            //  handle Error
            return null;
          }
          return this.GetMethodDefAtRow(rowId);
        case TokenTypeIds.FieldDef:
          if (rowId == 0 || rowId > this.PEFileReader.FieldTable.NumberOfRows) {
            //  handle Error
            return null;
          }
          return this.GetFieldDefAtRow(rowId);
        case TokenTypeIds.MemberRef:
          if (rowId == 0 || rowId > this.PEFileReader.MemberRefTable.NumberOfRows) {
            //  handle Error
            return null;
          }
          return this.GetModuleMemberReferenceAtRow(owningObject, rowId);
        case TokenTypeIds.MethodSpec:
          if (rowId == 0 || rowId > this.PEFileReader.MethodSpecTable.NumberOfRows) {
            //  handle Error
            return null;
          }
          return this.GetMethodSpecAtRow(owningObject, rowId);
        default:
          return null;
      }
    }
    #endregion Member Reference Information

    #region Attribute Information
    ICustomAttribute/*?*/[] CustomAttributeArray;
    ISecurityAttribute/*?*/[] DeclSecurityArray;
    MetadataObject currentOwningObject;

    internal void GetCustomAttributeInfo(
      MetadataObject metadataObject,
      out uint customAttributeRowIdStart,
      out uint customAttributeRowIdEnd
    ) {
      customAttributeRowIdStart = 0;
      customAttributeRowIdEnd = 0;
      if (metadataObject.TokenValue == 0xFFFFFFFF)
        return;
      uint customAttributeCount;
      customAttributeRowIdStart = this.PEFileReader.CustomAttributeTable.FindCustomAttributesForToken(metadataObject.TokenValue, out customAttributeCount);
      customAttributeRowIdEnd = customAttributeRowIdStart + customAttributeCount;
    }
    internal ICustomAttribute GetCustomAttributeAtRow(
      MetadataObject owningObject,
      uint token,
      uint customAttributeRowId
    ) {
      if (customAttributeRowId == 0 || customAttributeRowId > this.PEFileReader.CustomAttributeTable.NumberOfRows) {
        //  MD Error
        return Dummy.CustomAttribute;
      }
      if (this.CustomAttributeArray[customAttributeRowId] == null) {
        lock (GlobalLock.LockingObject) {
          if (this.CustomAttributeArray[customAttributeRowId] == null) {
            CustomAttributeRow customAttribute = this.PEFileReader.CustomAttributeTable[customAttributeRowId];
            if (customAttribute.Parent == token || (customAttribute.Parent == 1 && token == TokenTypeIds.Assembly+1)) {
              uint ctorTokenType = customAttribute.Type & TokenTypeIds.TokenTypeMask;
              uint ctorRowId = customAttribute.Type & TokenTypeIds.RIDMask;
              IMethodReference/*?*/ moduleMethodReference = null;
              if (ctorTokenType == TokenTypeIds.MethodDef) {
                moduleMethodReference = this.GetMethodDefAtRow(ctorRowId);
              } else if (ctorTokenType == TokenTypeIds.MemberRef) {
                moduleMethodReference = this.GetModuleMemberReferenceAtRow(owningObject, ctorRowId) as IMethodReference;
              }
              if (moduleMethodReference == null) {
                //  TODO: MDError
                this.CustomAttributeArray[customAttributeRowId] = Dummy.CustomAttribute;
                return Dummy.CustomAttribute;
              }
              this.currentOwningObject = owningObject;
              if (customAttribute.Value == 0) {
                this.CustomAttributeArray[customAttributeRowId] = 
                  this.ModuleReader.metadataReaderHost.Rewrite(this.Module, new CustomAttribute(this, customAttributeRowId, moduleMethodReference, null, null));
              } else {
                //  TODO: Check if customAttribute.Value is within the range
                MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(customAttribute.Value);
                //  TODO: Error checking enough space in signature memoryBlock.
                MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
                this.ModuleReader.metadataReaderHost.StartGuessingGame();
                CustomAttributeDecoder customAttrDecoder = new CustomAttributeDecoder(this, memoryReader, customAttributeRowId, moduleMethodReference);
                while (customAttrDecoder.decodeFailed && this.ModuleReader.metadataReaderHost.TryNextPermutation())
                  customAttrDecoder = new CustomAttributeDecoder(this, memoryReader, customAttributeRowId, moduleMethodReference);
                if (!customAttrDecoder.decodeFailed)
                  this.ModuleReader.metadataReaderHost.WinGuessingGame();
                //else
                //TODO: error
                this.CustomAttributeArray[customAttributeRowId] = customAttrDecoder.CustomAttribute;
              }
            } else {
              //  MD Error
              this.CustomAttributeArray[customAttributeRowId] = Dummy.CustomAttribute;
            }
          }
        }
      }
      ICustomAttribute/*?*/ ret = this.CustomAttributeArray[customAttributeRowId];
      //^ assert ret != null;
      return ret;
    }
    internal IEnumerable<ICustomAttribute> GetModuleCustomAttributes() {
      uint customAttributeCount;
      uint customAttributeRowIdStart = this.PEFileReader.CustomAttributeTable.FindCustomAttributesForToken(TokenTypeIds.Module | (uint)0x00000001, out customAttributeCount);
      uint customAttributeRowIdEnd = customAttributeRowIdStart + customAttributeCount;
      for (uint customAttributeIter = customAttributeRowIdStart; customAttributeIter < customAttributeRowIdEnd; ++customAttributeIter) {
        yield return this.GetCustomAttributeAtRow(this.Module, this.Module.TokenValue, customAttributeIter);
      }
    }
    internal IEnumerable<ICustomAttribute> GetAssemblyCustomAttributes() {
      uint customAttributeCount;
      uint customAttributeRowIdStart = this.PEFileReader.CustomAttributeTable.FindCustomAttributesForToken(TokenTypeIds.Assembly | (uint)0x00000001, out customAttributeCount);
      uint customAttributeRowIdEnd = customAttributeRowIdStart + customAttributeCount;
      for (uint customAttributeIter = customAttributeRowIdStart; customAttributeIter < customAttributeRowIdEnd; ++customAttributeIter) {
        yield return this.GetCustomAttributeAtRow(this.Module, this.Module.TokenValue, customAttributeIter);
      }
    }
    internal void GetSecurityAttributeInfo(
      MetadataObject metadataObject,
      out uint securityAttributeRowIdStart,
      out uint securityAttributeRowIdEnd
    ) {
      securityAttributeRowIdStart = 0;
      securityAttributeRowIdEnd = 0;
      if (metadataObject.TokenValue == 0xFFFFFFFF)
        return;
      uint securityAttributeCount;
      securityAttributeRowIdStart = this.PEFileReader.DeclSecurityTable.FindSecurityAttributesForToken(metadataObject.TokenValue, out securityAttributeCount);
      securityAttributeRowIdEnd = securityAttributeRowIdStart + securityAttributeCount;
    }
    internal ISecurityAttribute GetSecurityAttributeAtRow(
      MetadataObject owningObject,
      uint securityAttributeRowId
    ) {
      if (securityAttributeRowId >= this.DeclSecurityArray.Length) {
        //  MD Error
        return Dummy.SecurityAttribute;
      }
      if (this.DeclSecurityArray[securityAttributeRowId] == null) {
        lock (GlobalLock.LockingObject) {
          if (this.DeclSecurityArray[securityAttributeRowId] == null) {
            DeclSecurityRow declSecurity = this.PEFileReader.DeclSecurityTable[securityAttributeRowId];
            if (declSecurity.Parent == owningObject.TokenValue) {
              this.DeclSecurityArray[securityAttributeRowId] = new SecurityAttribute(this, securityAttributeRowId, (SecurityAction)declSecurity.ActionFlags);
            } else {
              //  MD Error
              this.DeclSecurityArray[securityAttributeRowId] = Dummy.SecurityAttribute;
            }
          }
        }
      }
      ISecurityAttribute/*?*/ ret = this.DeclSecurityArray[securityAttributeRowId];
      //^ assert ret != null;
      return ret;
    }
    internal IEnumerable<ICustomAttribute> GetSecurityAttributeData(SecurityAttribute securityAttribute) {
      DeclSecurityRow declSecurity = this.PEFileReader.DeclSecurityTable[securityAttribute.DeclSecurityRowId];
      //  TODO: Check is securityAttribute.Value is within the range
      MemoryBlock signatureMemoryBlock = this.PEFileReader.BlobStream.GetMemoryBlockAt(declSecurity.PermissionSet);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: 1.0 etc later on...

      this.ModuleReader.metadataReaderHost.StartGuessingGame();
      SecurityAttributeDecoder20 securityAttrDecoder = new SecurityAttributeDecoder20(this, memoryReader, securityAttribute);
      while (securityAttrDecoder.decodeFailed && this.ModuleReader.metadataReaderHost.TryNextPermutation())
        securityAttrDecoder = new SecurityAttributeDecoder20(this, memoryReader, securityAttribute);
      if (!securityAttrDecoder.decodeFailed)
        this.ModuleReader.metadataReaderHost.WinGuessingGame();
      //else
      //TODO: error

      return securityAttrDecoder.SecurityAttributes;
    }
    #endregion Attribute Information


  }

  #region Signature Converters
  //  These are short term objects. All the work happens in the constructor and fields are copied into containing structure...
  internal abstract class SignatureConverter {
    protected readonly PEFileToObjectModel PEFileToObjectModel;
    protected readonly MetadataObject MetadataOwnerObject;
    protected readonly TypeBase/*?*/ ModuleGenericType;
    protected readonly GenericMethod/*?*/ ModuleGenericMethod;
    protected readonly IMethodReference/*?*/ ModuleMethodReference;
    protected readonly ITypeMemberReference/*?*/ ModuleMemberReference;
    protected MemoryReader SignatureMemoryReader;
    internal SignatureConverter(
      PEFileToObjectModel peFileToObjectModel,
      MemoryReader signatureMemoryReader,
      MetadataObject metadataOwnerObject
    )
      //^ requires signatureMemoryReader.Length > 0;
    {
      this.PEFileToObjectModel = peFileToObjectModel;
      this.SignatureMemoryReader = signatureMemoryReader;
      this.MetadataOwnerObject = metadataOwnerObject;
      this.ModuleMethodReference = metadataOwnerObject as IMethodReference;
      this.ModuleMemberReference = metadataOwnerObject as ITypeMemberReference;
      TypeMember/*?*/ moduleTypeMember = metadataOwnerObject as TypeMember;
      if (moduleTypeMember != null) {
        this.ModuleGenericType = moduleTypeMember.ContainingTypeDefinition as TypeBase;
        this.ModuleGenericMethod = moduleTypeMember as GenericMethod;
        return;
      }
      var moduleGenericType = metadataOwnerObject as TypeBase;
      if (moduleGenericType != null) {
        this.ModuleGenericType = moduleGenericType;
        return;
      }
      GenericTypeParameter/*?*/ genericTypeParam = metadataOwnerObject as GenericTypeParameter;
      if (genericTypeParam != null) {
        this.ModuleGenericType = genericTypeParam.OwningGenericType;
        return;
      }
      GenericMethodParameter/*?*/ genericMethodParam = metadataOwnerObject as GenericMethodParameter;
      if (genericMethodParam != null) {
        this.ModuleGenericType = genericMethodParam.OwningGenericMethod.ContainingTypeDefinition as TypeBase;
        this.ModuleGenericMethod = genericMethodParam.OwningGenericMethod;
        return;
      }
    }

    protected IEnumerable<ICustomModifier>/*?*/ GetCustomModifiers(out bool isPinned) {
      isPinned = false;
      List<ICustomModifier> customModifierList = null;
      ICustomModifier customModifier = null;
      while (this.SignatureMemoryReader.NotEndOfBytes) {
        byte header = this.SignatureMemoryReader.PeekByte(0);
        if (header == ElementType.Pinned) {
          this.SignatureMemoryReader.SkipBytes(1);
          isPinned = true;
          continue;
        }
        if (header != ElementType.RequiredModifier && header != ElementType.OptionalModifier)
          break;
        this.SignatureMemoryReader.SkipBytes(1);
        uint typeDefOrRefEncoded = (uint)this.SignatureMemoryReader.ReadCompressedUInt32();
        uint typeToken = TypeDefOrRefTag.ConvertToToken(typeDefOrRefEncoded);
        uint tokenType = typeToken & TokenTypeIds.TokenTypeMask;
        uint typeRID = typeToken & TokenTypeIds.RIDMask;
        ITypeReference/*?*/ typeRef = null;
        if (tokenType == TokenTypeIds.TypeDef)
          typeRef = this.PEFileToObjectModel.GetTypeDefinitionAtRow(typeRID);
        else if (tokenType == TokenTypeIds.TypeRef)
          typeRef = this.PEFileToObjectModel.GetTypeRefReferenceAtRow(typeRID);
        else
          typeRef = this.PEFileToObjectModel.GetTypeSpecReferenceAtRow(this.MetadataOwnerObject, typeRID).UnderlyingModuleTypeReference;
        if (typeRef == null) {
          //  Error...
          continue;
        }
        if (customModifier != null && customModifierList == null) {
          customModifierList = new List<ICustomModifier>(4);
          customModifierList.Add(customModifier);
        }
        customModifier = new CustomModifier(header == ElementType.OptionalModifier, typeRef);
        if (customModifierList != null) customModifierList.Add(customModifier);
      }
      if (this.SignatureMemoryReader.RemainingBytes <= 0) {
        //  TODO: Handle error...
      }
      if (customModifierList == null) {
        if (customModifier == null) return null;
        return IteratorHelper.GetSingletonEnumerable(customModifier);
      } else {
        return IteratorHelper.GetReadonly(customModifierList.ToArray());
      }
    }

    protected ITypeReference/*?*/ GetModuleGenericTypeInstanceReference(uint typeSpecToken) {
      byte headByte = this.SignatureMemoryReader.ReadByte();
      uint templateTypeEncoded = (uint)this.SignatureMemoryReader.ReadCompressedUInt32();
      uint templateTypeToken = TypeDefOrRefTag.ConvertToToken(templateTypeEncoded);
      uint templateTypeTokenKind = templateTypeToken & TokenTypeIds.TokenTypeMask;
      uint templateTypeRowId = templateTypeToken & TokenTypeIds.RIDMask;
      INamedTypeReference/*?*/ templateTypeReference = null;
      if (templateTypeTokenKind == TokenTypeIds.TypeDef) {
        templateTypeReference = this.PEFileToObjectModel.GetTypeDefinitionAtRow(templateTypeRowId);
      } else if (templateTypeTokenKind == TokenTypeIds.TypeRef) {
        templateTypeReference = this.PEFileToObjectModel.GetTypeRefReferenceAtRow(templateTypeRowId, headByte == ElementType.ValueType);
      } else {
        //  TODO: Error case
        return null;
      }
      if (templateTypeReference == null) {
        //  Error...
        return null;
      }
      var genericArgumentCount = (ushort)this.SignatureMemoryReader.ReadCompressedUInt32();
      var typeNamesAreMangled = SeeIfTypeNamesAreMangled(templateTypeReference);
      if (typeNamesAreMangled)
        return SpecializeAndOrInstantiate(typeSpecToken, templateTypeReference, ref genericArgumentCount, outer: true);
      else {
        //Add in the generic argument count
        var nestedTemplate = templateTypeReference as INestedTypeReference;
        if (nestedTemplate != null) {
          templateTypeReference = new NestedTypeReference(this.PEFileToObjectModel.ModuleReader.metadataReaderHost, nestedTemplate.ContainingType, nestedTemplate.Name,
            genericArgumentCount, nestedTemplate.IsEnum, nestedTemplate.IsValueType, mangleName: false);
        } else {
          var namespaceTemplate = templateTypeReference as INamespaceTypeReference;
          if (namespaceTemplate != null) {
            templateTypeReference = new NamespaceTypeReference(this.PEFileToObjectModel.ModuleReader.metadataReaderHost, namespaceTemplate.ContainingUnitNamespace, namespaceTemplate.Name,
              genericArgumentCount, namespaceTemplate.IsEnum, namespaceTemplate.IsValueType, mangleName: false);
          }
        }
        return this.Instantiate(typeSpecToken, templateTypeReference, genericArgumentCount);
      }        
    }

    private static bool SeeIfTypeNamesAreMangled(INamedTypeReference/*?*/ templateTypeReference) {
      if (templateTypeReference == null) return false;
      if (templateTypeReference.MangleName) return true;
      var nestedTypeReference = templateTypeReference as INestedTypeReference;
      if (nestedTypeReference != null) return SeeIfTypeNamesAreMangled(nestedTypeReference.ContainingType as INamedTypeReference);
      return false;
    }

    private ITypeReference SpecializeAndOrInstantiate(uint typeSpecToken, INamedTypeReference namedTypeReference, ref ushort genericArgumentCount, bool outer) {
      if (genericArgumentCount == 0) return namedTypeReference;
      var nestedTypeReference = namedTypeReference as INestedTypeReference;
      if (nestedTypeReference != null) {
        Contract.Assume(!(nestedTypeReference is ISpecializedNestedTypeReference)); //the type reference comes from the metadata, which is always fully unspecialized
        var containingType = this.SpecializeAndOrInstantiate(0, (INamedTypeReference)nestedTypeReference.ContainingType, ref genericArgumentCount, outer: false);
        if (containingType != nestedTypeReference.ContainingType)
          namedTypeReference = new SpecializedNestedTypeReference(nestedTypeReference, containingType, this.PEFileToObjectModel.InternFactory);
      }
      if (genericArgumentCount <= 0) return namedTypeReference;
      var genericParametersCount = namedTypeReference.GenericParameterCount;
      genericArgumentCount -= genericParametersCount;
      if (genericArgumentCount < 0) { genericParametersCount += genericArgumentCount; };
      if (genericParametersCount == 0) return namedTypeReference;
      return this.Instantiate(typeSpecToken, namedTypeReference, genericParametersCount);
    }

    private ITypeReference Instantiate(uint typeSpecToken, INamedTypeReference templateTypeReference, ushort genericArgumentCount) {
      //It would be very desirable to cache these objects by structure so that we can reuse them.
      //However, it is not safe at this point to use the intern table because we might still be reading the
      //signature of a generic method whose generic parameters feature in the arguments to the generic type.
      //We cannot compute the intern key of a generic method type parameter before we are able to compute the intern key of the generic method.
      var genericArgumentArray = new ITypeReference[genericArgumentCount];
      for (int i = 0; i < genericArgumentCount; ++i) genericArgumentArray[i] = this.GetTypeReference()??Dummy.TypeReference;
      if (typeSpecToken != 0xFFFFFFFF)
        return new GenericTypeInstanceReferenceWithToken(typeSpecToken, templateTypeReference, IteratorHelper.GetReadonly(genericArgumentArray), this.PEFileToObjectModel.InternFactory);
      else
        return new GenericTypeInstanceReference(templateTypeReference, IteratorHelper.GetReadonly(genericArgumentArray), this.PEFileToObjectModel.InternFactory);
    }

    protected ManagedPointerType/*?*/ GetModuleManagedPointerType(uint typeSpecToken) {
      ITypeReference/*?*/ targetType = this.GetTypeReference();
      if (targetType == null) return null;
      if (typeSpecToken != 0xFFFFFFFF)
        return new ManagedPointerTypeWithToken(typeSpecToken, targetType, this.PEFileToObjectModel.InternFactory);
      else
        return ManagedPointerType.GetManagedPointerType(targetType, this.PEFileToObjectModel.InternFactory);
    }

    protected PointerType/*?*/ GetModulePointerType(uint typeSpecToken) {
      ITypeReference/*?*/ targetType = this.GetTypeReference();
      if (targetType == null) return null;
      return new PointerTypeWithToken(typeSpecToken, targetType, this.PEFileToObjectModel.InternFactory);
    }

    protected Matrix/*?*/ GetModuleMatrixType(uint typeSpecToken) {
      ITypeReference/*?*/ elementType = this.GetTypeReference();
      if (elementType == null) return null;
      uint rank = (uint)this.SignatureMemoryReader.ReadCompressedUInt32();
      int numSizes = this.SignatureMemoryReader.ReadCompressedUInt32();
      ulong[] sizes = numSizes == 0 ? null : new ulong[numSizes];
      for (int i = 0; i < numSizes; ++i)
        sizes[i] = (ulong)this.SignatureMemoryReader.ReadCompressedUInt32();
      int numLowerBounds = this.SignatureMemoryReader.ReadCompressedUInt32();
      int[] lowerBounds = numLowerBounds == 0 ? null : new int[numLowerBounds];
      for (int i = 0; i < numLowerBounds; ++i)
        lowerBounds[i] = this.SignatureMemoryReader.ReadCompressedInt32();
      return new MatrixWithToken(typeSpecToken, elementType, rank, IteratorHelper.GetReadonly(lowerBounds), IteratorHelper.GetReadonly(sizes), this.PEFileToObjectModel.InternFactory);
    }

    protected Vector/*?*/ GetModuleVectorType(uint typeSpecToken) {
      var elementType = this.GetTypeReference();
      if (elementType == null) return null;
      return new VectorWithToken(typeSpecToken, elementType, this.PEFileToObjectModel.InternFactory);
    }

    protected FunctionPointerType/*?*/ GetModuleFuntionPointer(uint typeSpecToken) {
      byte firstByte = this.SignatureMemoryReader.ReadByte();
      if ((firstByte & SignatureHeader.GenericInstance) == SignatureHeader.GenericInstance) {
        this.SignatureMemoryReader.ReadCompressedUInt32();
        Debug.Fail("Please mail this PE file to hermanv@microsoft.com");
      }
      int paramCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      bool dummyPinned;
      var returnCustomModifiers = this.GetCustomModifiers(out dummyPinned);
      ITypeReference/*?*/ returnTypeReference;
      bool isReturnByReference = false;
      byte retByte = this.SignatureMemoryReader.PeekByte(0);
      if (retByte == ElementType.Void) {
        returnTypeReference = this.PEFileToObjectModel.PlatformType.SystemVoid;
        this.SignatureMemoryReader.SkipBytes(1);
      } else if (retByte == ElementType.TypedReference) {
        returnTypeReference = this.PEFileToObjectModel.PlatformType.SystemTypedReference;
        this.SignatureMemoryReader.SkipBytes(1);
      } else {
        if (retByte == ElementType.ByReference) {
          isReturnByReference = true;
          this.SignatureMemoryReader.SkipBytes(1);
        }
        returnTypeReference = this.GetTypeReference();
      }
      if (returnTypeReference == null) return null;
      int methodParamCount = 0;
      IEnumerable<IParameterTypeInformation> moduleParameters = Enumerable<IParameterTypeInformation>.Empty;
      if (paramCount > 0) {
        IParameterTypeInformation[] moduleParameterArr = this.GetModuleParameterTypeInformations(Dummy.Signature, paramCount);
        methodParamCount = moduleParameterArr.Length;
        if (methodParamCount > 0) moduleParameters = IteratorHelper.GetReadonly(moduleParameterArr);
      }
      IEnumerable<IParameterTypeInformation> moduleVarargsParameters = Enumerable<IParameterTypeInformation>.Empty;
      if (paramCount > methodParamCount) {
        IParameterTypeInformation[] moduleParameterArr = this.GetModuleParameterTypeInformations(Dummy.Signature, paramCount - methodParamCount);
        if (moduleParameterArr.Length > 0) moduleVarargsParameters = IteratorHelper.GetReadonly(moduleParameterArr);
      }
      if (typeSpecToken != 0xFFFFFFFF)
        return new FunctionPointerTypeWithToken(typeSpecToken, (CallingConvention)firstByte, isReturnByReference, returnTypeReference, returnCustomModifiers, moduleParameters,
          moduleVarargsParameters, this.PEFileToObjectModel.InternFactory);
      else
        return new FunctionPointerType((CallingConvention)firstByte, isReturnByReference, returnTypeReference, returnCustomModifiers, moduleParameters,
          moduleVarargsParameters, this.PEFileToObjectModel.InternFactory);
    }

    protected ITypeReference/*?*/ GetTypeReference() {
      byte headByte = this.SignatureMemoryReader.ReadByte();
      switch (headByte) {
        case ElementType.Void:
          return this.PEFileToObjectModel.PlatformType.SystemVoid;
        case ElementType.Boolean:
          return this.PEFileToObjectModel.PlatformType.SystemBoolean;
        case ElementType.Char:
          return this.PEFileToObjectModel.PlatformType.SystemChar;
        case ElementType.Int8:
          return this.PEFileToObjectModel.PlatformType.SystemInt8;
        case ElementType.Int16:
          return this.PEFileToObjectModel.PlatformType.SystemInt16;
        case ElementType.Int32:
          return this.PEFileToObjectModel.PlatformType.SystemInt32;
        case ElementType.Int64:
          return this.PEFileToObjectModel.PlatformType.SystemInt64;
        case ElementType.UInt8:
          return this.PEFileToObjectModel.PlatformType.SystemUInt8;
        case ElementType.UInt16:
          return this.PEFileToObjectModel.PlatformType.SystemUInt16;
        case ElementType.UInt32:
          return this.PEFileToObjectModel.PlatformType.SystemUInt32;
        case ElementType.UInt64:
          return this.PEFileToObjectModel.PlatformType.SystemUInt64;
        case ElementType.Single:
          return this.PEFileToObjectModel.PlatformType.SystemFloat32;
        case ElementType.Double:
          return this.PEFileToObjectModel.PlatformType.SystemFloat64;
        case ElementType.IntPtr:
          return this.PEFileToObjectModel.PlatformType.SystemIntPtr;
        case ElementType.UIntPtr:
          return this.PEFileToObjectModel.PlatformType.SystemUIntPtr;
        case ElementType.Object:
          return this.PEFileToObjectModel.PlatformType.SystemObject;
        case ElementType.String:
          return this.PEFileToObjectModel.PlatformType.SystemString;
        case ElementType.ByReference:
          return this.GetModuleManagedPointerType(0xFFFFFFFF);
        case ElementType.Pointer:
          return PointerType.GetPointerType(this.GetTypeReference()??Dummy.TypeReference, this.PEFileToObjectModel.InternFactory);
        case ElementType.Array:
          return this.GetModuleMatrixType(0xFFFFFFFF);
        case ElementType.SzArray:
          return Vector.GetVector(this.GetTypeReference()??Dummy.TypeReference, this.PEFileToObjectModel.InternFactory);
        case ElementType.Class:
        case ElementType.ValueType: {
            uint typeEncoded = (uint)this.SignatureMemoryReader.ReadCompressedUInt32();
            uint typeToken = TypeDefOrRefTag.ConvertToToken(typeEncoded);
            return this.PEFileToObjectModel.GetTypeReferenceForToken(this.MetadataOwnerObject, typeToken, headByte == ElementType.ValueType);
          }
        case ElementType.GenericTypeParameter: {
            var ordinal = (ushort)this.SignatureMemoryReader.ReadCompressedUInt32();
            if (this.ModuleGenericType == null) {
              if (this.ModuleMemberReference == null) {
                //  Error
                return null;
              }
              var typeRef = this.ModuleMemberReference.ContainingType;
              if (typeRef == null) {
                //  Error
                return null;
              }
              var arrayTypeRef = typeRef as IArrayTypeReference;
              if (arrayTypeRef != null) typeRef = arrayTypeRef.ElementType;
              //if we get here, we are parsing the signature of member reference
              return new SignatureGenericTypeParameter(this.PEFileToObjectModel, typeRef, ordinal);
            }
            //if we get here, we are parsing the signature of member definition
            return this.ModuleGenericType.GetGenericTypeParameterFromOrdinal(ordinal);
          }
        case ElementType.GenericMethodParameter: {
            ushort ordinal = (ushort)this.SignatureMemoryReader.ReadCompressedUInt32();
            if (this.ModuleGenericMethod == null) {
              if (this.ModuleMethodReference != null) {
                //if we get here, we are parsing the signature of generic method instance reference
                return new SignatureGenericMethodParameter(this.PEFileToObjectModel, this.ModuleMethodReference, ordinal);
              }
              //  TODO: Error
              return null;
            }
            //if we get here we are parsing the signature of a generic method definition
            return this.ModuleGenericMethod.GetGenericMethodParameterFromOrdinal(ordinal);
          }
        case ElementType.GenericTypeInstance:
          return this.GetModuleGenericTypeInstanceReference(0xFFFFFFFF);
        case ElementType.FunctionPointer:
          return this.GetModuleFuntionPointer(0xFFFFFFFF);
        case ElementType.RequiredModifier:
        case ElementType.OptionalModifier: {
            bool dummyPinned;
            this.SignatureMemoryReader.SkipBytes(-1);
            var customModifiers = this.GetCustomModifiers(out dummyPinned);
            ITypeReference/*?*/ typeReference = this.GetTypeReference();
            if (typeReference == null)
              return null;
            return new ModifiedTypeReference(this.PEFileToObjectModel, typeReference, customModifiers);
          }
        default:
          Debug.Fail("?!?");
          break;
      }
      return null;
    }

    internal struct ParamInfo {
      internal readonly uint ParamRowId;
      internal readonly uint ParamSequence;
      internal readonly IName ParamName;
      internal readonly ParamFlags ParamFlags;
      internal ParamInfo(
        uint paramRowId,
        uint paramSequence,
        IName paramName,
        ParamFlags paramFlags
      ) {
        this.ParamSequence = paramSequence;
        this.ParamRowId = paramRowId;
        this.ParamName = paramName;
        this.ParamFlags = paramFlags;
      }
    }

    protected virtual ParamInfo? GetParamInfo(
      int paramSequence
    ) {
      return null;
    }

    protected IParameterDefinition[] GetModuleParameters(bool useParamInfo, ISignature signatureDefinition, int paramCount) {
      MethodDefinition/*?*/ moduleMethod = signatureDefinition as MethodDefinition;
      int paramIndex = 0;
      var parameters = new IParameterDefinition[paramCount];
      while (paramIndex < paramCount) {
        bool dummyPinned;
        var customModifiers = this.GetCustomModifiers(out dummyPinned);
        byte currByte = this.SignatureMemoryReader.PeekByte(0);
        if (currByte == ElementType.Sentinel) {
          this.SignatureMemoryReader.SkipBytes(1);
          var requiredParameters = new IParameterDefinition[paramIndex];
          for (int i = 0; i < paramIndex; i++) requiredParameters[i] = parameters[i];
          return requiredParameters;
        }
        bool isByReference = false;
        ITypeReference/*?*/ typeReference;
        if (currByte == ElementType.TypedReference) {
          this.SignatureMemoryReader.SkipBytes(1);
          typeReference = this.PEFileToObjectModel.PlatformType.SystemTypedReference;
        } else {
          if (currByte == ElementType.ByReference) {
            this.SignatureMemoryReader.SkipBytes(1);
            isByReference = true;
          }
          typeReference = this.GetTypeReference();
        }
        ParamInfo? paramInfo = useParamInfo ? this.GetParamInfo(paramIndex + 1) : null;
        IParameterDefinition moduleParameter;
        if (paramInfo.HasValue) {
          var paramArrayType = typeReference as IArrayTypeReference;
          moduleParameter = new ParameterWithMetadata(this.PEFileToObjectModel, paramIndex, customModifiers, typeReference, moduleMethod, isByReference,
            (paramIndex == paramCount - 1) && paramArrayType != null && paramArrayType.IsVector, paramInfo.Value.ParamRowId, paramInfo.Value.ParamName, paramInfo.Value.ParamFlags);
        } else
          moduleParameter = new ParameterWithoutMetadata(this.PEFileToObjectModel, paramIndex, customModifiers, typeReference, signatureDefinition, isByReference);
        parameters[paramIndex++] = moduleParameter;
      }
      return parameters;
    }

    protected IParameterTypeInformation[] GetModuleParameterTypeInformations(ISignature signature, int paramCount) {
      var parameterTypes = new IParameterTypeInformation[paramCount];
      for (var index = 0; index < paramCount; index++) {
        bool dummyPinned;
        var customModifiers = this.GetCustomModifiers(out dummyPinned);
        byte currByte = this.SignatureMemoryReader.PeekByte(0);
        if (currByte == ElementType.Sentinel) {
          this.SignatureMemoryReader.SkipBytes(1);
          if (index < paramCount) Array.Resize(ref parameterTypes, index);
          break;
        }
        bool isByReference = false;
        ITypeReference/*?*/ typeReference;
        if (currByte == ElementType.TypedReference) {
          this.SignatureMemoryReader.SkipBytes(1);
          typeReference = this.PEFileToObjectModel.PlatformType.SystemTypedReference;
        } else {
          if (currByte == ElementType.ByReference) {
            this.SignatureMemoryReader.SkipBytes(1);
            isByReference = true;
          }
          typeReference = this.GetTypeReference();
        }
        var parameterType = new ParameterInfo(this.PEFileToObjectModel, index, customModifiers, typeReference, signature, isByReference);
        parameterTypes[index] = parameterType;
      }
      return parameterTypes;
    }

  }

  internal sealed class FieldSignatureConverter : SignatureConverter {

    internal FieldSignatureConverter(PEFileToObjectModel peFileToObjectModel, MetadataObject moduleField, MemoryReader signatureMemoryReader)
      : base(peFileToObjectModel, signatureMemoryReader, moduleField) {
      //^ base;
      //^ this.SignatureMemoryReader = signatureMemoryReader; //TODO: Spec# bug. This assignment should not be necessary.
      this.FirstByte = this.SignatureMemoryReader.ReadByte();
      if (!SignatureHeader.IsFieldSignature(this.FirstByte)) {
        //  Error...
      }
      bool isPinned;
      this.customModifiers = this.GetCustomModifiers(out isPinned);
      this.TypeReference = this.GetTypeReference();
    }

    internal readonly byte FirstByte;
    internal readonly ITypeReference/*?*/ TypeReference;
    internal readonly IEnumerable<ICustomModifier>/*?*/ customModifiers;
  }

  internal sealed class PropertySignatureConverter : SignatureConverter {

    internal PropertySignatureConverter(PEFileToObjectModel peFileToObjectModel, PropertyDefinition moduleProperty, MemoryReader signatureMemoryReader)
      : base(peFileToObjectModel, signatureMemoryReader, moduleProperty) {
      this.parameters = Enumerable<IParameterDefinition>.Empty;
      //  TODO: Check minimum required size of the signature...
      this.firstByte = this.SignatureMemoryReader.ReadByte();
      if (!SignatureHeader.IsPropertySignature(this.firstByte)) {
        //  Error...
      }
      int paramCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      bool dummyPinned;
      this.returnCustomModifiers = this.GetCustomModifiers(out dummyPinned);
      if (this.SignatureMemoryReader.PeekByte(0) == ElementType.ByReference) {
        this.returnValueIsByReference = true;
        this.SignatureMemoryReader.SkipBytes(1);
      }
      this.type = this.GetTypeReference();
      if (paramCount > 0) {
        IParameterDefinition[] moduleParamArr = this.GetModuleParameters(false, moduleProperty, paramCount);
        if (moduleParamArr.Length > 0) this.parameters = IteratorHelper.GetReadonly(moduleParamArr);
      }
    }

    internal readonly byte firstByte;
    internal readonly IEnumerable<ICustomModifier>/*?*/ returnCustomModifiers;
    internal readonly bool returnValueIsByReference;
    internal readonly ITypeReference/*?*/ type;
    internal readonly IEnumerable<IParameterDefinition> parameters;

  }

  internal sealed class MethodDefSignatureConverter : SignatureConverter {
    internal readonly byte FirstByte;
    internal readonly uint GenericParamCount;
    internal int paramCount;
    internal readonly IEnumerable<ICustomModifier>/*?*/ ReturnCustomModifiers;
    internal readonly ITypeReference/*?*/ ReturnTypeReference;
    internal readonly IParameterDefinition[]/*?*/ Parameters;
    internal readonly ReturnParameter ReturnParameter;
    readonly ParamInfo[] ParamInfoArray;

    internal MethodDefSignatureConverter(PEFileToObjectModel peFileToObjectModel, MethodDefinition moduleMethod, MemoryReader signatureMemoryReader)
      : base(peFileToObjectModel, signatureMemoryReader, moduleMethod) {
      //  TODO: Check minimum required size of the signature...
      this.FirstByte = this.SignatureMemoryReader.ReadByte();
      if (SignatureHeader.IsGeneric(this.FirstByte)) {
        this.GenericParamCount = (uint)this.SignatureMemoryReader.ReadCompressedUInt32();
      }
      this.paramCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      bool dummyPinned;
      this.ReturnCustomModifiers = this.GetCustomModifiers(out dummyPinned);
      byte retByte = this.SignatureMemoryReader.PeekByte(0);
      bool isReturnByReference = false;
      if (retByte == ElementType.Void) {
        this.ReturnTypeReference = peFileToObjectModel.PlatformType.SystemVoid;
        this.SignatureMemoryReader.SkipBytes(1);
      } else if (retByte == ElementType.TypedReference) {
        this.ReturnTypeReference = peFileToObjectModel.PlatformType.SystemTypedReference;
        this.SignatureMemoryReader.SkipBytes(1);
      } else {
        if (retByte == ElementType.ByReference) {
          isReturnByReference = true;
          this.SignatureMemoryReader.SkipBytes(1);
        }
        this.ReturnTypeReference = this.GetTypeReference();
      }
      PEFileReader peFileReader = peFileToObjectModel.PEFileReader;
      uint paramRowCount;
      uint paramRowStart = peFileReader.GetParamInformation(moduleMethod.MethodDefRowId, out paramRowCount);
      uint paramRowEnd = paramRowStart + paramRowCount;
      ParamInfo[] paramInfoArray = new ParamInfo[paramRowCount];
      if (peFileReader.UseParamPtrTable) {
        for (uint paramRowIter = paramRowStart; paramRowIter < paramRowEnd; ++paramRowIter) {
          uint paramRowId = peFileReader.ParamPtrTable.GetParamFor(paramRowIter);
          ParamRow paramRow = peFileReader.ParamTable[paramRowId];
          //  TODO: Error check if seqence is in proper range...
          paramInfoArray[paramRowId - paramRowStart] = new ParamInfo(paramRowId, paramRow.Sequence, peFileToObjectModel.GetNameFromOffset(paramRow.Name), paramRow.Flags);
        }
      } else {
        for (uint paramRowId = paramRowStart; paramRowId < paramRowEnd; ++paramRowId) {
          ParamRow paramRow = peFileReader.ParamTable[paramRowId];
          //  TODO: Error check if seqence is in proper range...
          paramInfoArray[paramRowId - paramRowStart] = new ParamInfo(paramRowId, paramRow.Sequence, peFileToObjectModel.GetNameFromOffset(paramRow.Name), paramRow.Flags);
        }
      }
      if (paramRowCount > 0 && paramInfoArray[0].ParamSequence == 0) {
        ParamFlags paramFlag = paramInfoArray[0].ParamFlags;
        if (isReturnByReference) {
          paramFlag |= ParamFlags.ByReference;
        }
        this.ReturnParameter = new ReturnParameter(this.PEFileToObjectModel, paramInfoArray[0].ParamName, paramFlag, paramInfoArray[0].ParamRowId);
      } else {
        this.ReturnParameter = new ReturnParameter(this.PEFileToObjectModel, Dummy.Name, isReturnByReference ? ParamFlags.ByReference : 0, 0);
      }
      this.ParamInfoArray = paramInfoArray;
      if (this.paramCount > 0) {
        IParameterDefinition[] moduleParamArr = this.GetModuleParameters(true, moduleMethod, this.paramCount);
        this.paramCount = moduleParamArr.Length;
        if (this.paramCount > 0) this.Parameters = moduleParamArr;
      }
    }
    protected override ParamInfo? GetParamInfo(
      int paramSequence
    ) {
      for (int i = 0; i < this.ParamInfoArray.Length; ++i) {
        if (paramSequence == this.ParamInfoArray[i].ParamSequence)
          return this.ParamInfoArray[i];
      }
      return null;
    }
  }

  internal sealed class TypeSpecSignatureConverter : SignatureConverter {
    internal readonly ITypeReference/*?*/ TypeReference;
    //^ [NotDelayed]
    internal TypeSpecSignatureConverter(
      PEFileToObjectModel peFileToObjectModel,
      TypeSpecReference moduleTypeSpecReference,
      MemoryReader signatureMemoryReader
    )
      : base(peFileToObjectModel, signatureMemoryReader, moduleTypeSpecReference.TypeSpecOwner) {
      //^ base;
      //^ this.SignatureMemoryReader = signatureMemoryReader; //TODO: Spec# bug. This assignment should not be necessary.
      byte firstByte = this.SignatureMemoryReader.ReadByte();
      switch (firstByte) {
        case ElementType.GenericTypeInstance:
          this.TypeReference = this.GetModuleGenericTypeInstanceReference(TokenTypeIds.TypeSpec | moduleTypeSpecReference.TypeSpecRowId);
          break;
        case ElementType.ByReference:
          this.TypeReference = this.GetModuleManagedPointerType(TokenTypeIds.TypeSpec | moduleTypeSpecReference.TypeSpecRowId);
          break;
        case ElementType.Pointer:
          this.TypeReference = this.GetModulePointerType(TokenTypeIds.TypeSpec | moduleTypeSpecReference.TypeSpecRowId);
          break;
        case ElementType.Array:
          this.TypeReference = this.GetModuleMatrixType(TokenTypeIds.TypeSpec | moduleTypeSpecReference.TypeSpecRowId);
          break;
        case ElementType.SzArray:
          this.TypeReference = this.GetModuleVectorType(TokenTypeIds.TypeSpec | moduleTypeSpecReference.TypeSpecRowId);
          break;
        case ElementType.FunctionPointer:
          this.TypeReference = this.GetModuleFuntionPointer(TokenTypeIds.TypeSpec | moduleTypeSpecReference.TypeSpecRowId);
          break;
        case ElementType.Class:
        case ElementType.ValueType: {
            uint typeEncoded = (uint)this.SignatureMemoryReader.ReadCompressedUInt32();
            uint typeToken = TypeDefOrRefTag.ConvertToToken(typeEncoded);
            this.TypeReference = this.PEFileToObjectModel.GetTypeReferenceForToken(this.MetadataOwnerObject, typeToken, firstByte == ElementType.ValueType);
          }
          break;
        case ElementType.GenericTypeParameter: {
            ushort ordinal = (ushort)this.SignatureMemoryReader.ReadCompressedUInt32();
            if (this.ModuleGenericType == null) {
              //  TODO: Error
            } else {
              this.TypeReference = this.ModuleGenericType.GetGenericTypeParameterFromOrdinal(ordinal);
            }
            break;
          }
        case ElementType.GenericMethodParameter: {
            ushort ordinal = (ushort)this.SignatureMemoryReader.ReadCompressedUInt32();
            if (this.ModuleGenericMethod == null) {
              //  TODO: Error
            } else {
              this.TypeReference = this.ModuleGenericMethod.GetGenericMethodParameterFromOrdinal(ordinal);
            }
            break;
          }
        case ElementType.RequiredModifier:
        case ElementType.OptionalModifier: {
            bool dummyPinned;
            this.SignatureMemoryReader.SkipBytes(-1);
            var customModifiers = this.GetCustomModifiers(out dummyPinned);
            ITypeReference/*?*/ typeReference = this.GetTypeReference();
            if (typeReference == null) {
              //  TODO: Error
            } else {
              this.TypeReference = new ModifiedTypeReference(this.PEFileToObjectModel, typeReference, customModifiers);
            }
            break;
          }
        case ElementType.Boolean:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemBoolean;
          break;
        case ElementType.Char:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemChar;
          break;
        case ElementType.Double:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemFloat64;
          break;
        case ElementType.Int16:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemInt16;
          break;
        case ElementType.Int32:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemInt32;
          break;
        case ElementType.Int64:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemInt64;
          break;
        case ElementType.Int8:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemInt8;
          break;
        case ElementType.IntPtr:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemIntPtr;
          break;
        case ElementType.Object:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemObject;
          break;
        case ElementType.Single:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemFloat32;
          break;
        case ElementType.String:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemString;
          break;
        case ElementType.UInt16:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemUInt16;
          break;
        case ElementType.UInt32:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemUInt32;
          break;
        case ElementType.UInt64:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemUInt64;
          break;
        case ElementType.UInt8:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemUInt8;
          break;
        case ElementType.UIntPtr:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemUIntPtr;
          break;
        case ElementType.Void:
          this.TypeReference = this.PEFileToObjectModel.PlatformType.SystemVoid;
          break;
        default:
          //  Error...
          break;
      }
    }
  }

  internal sealed class MethodRefSignatureConverter : SignatureConverter {

    internal readonly ushort GenericParamCount;
    internal readonly IEnumerable<ICustomModifier>/*?*/ ReturnCustomModifiers;
    internal readonly ITypeReference/*?*/ ReturnTypeReference;
    internal readonly bool IsReturnByReference;
    internal readonly IParameterTypeInformation[]/*?*/ RequiredParameters;
    internal readonly IParameterTypeInformation[]/*?*/ VarArgParameters;

    internal MethodRefSignatureConverter(PEFileToObjectModel peFileToObjectModel, MethodReference moduleMethodRef, MemoryReader signatureMemoryReader)
      : base(peFileToObjectModel, signatureMemoryReader, moduleMethodRef) {
      //  TODO: Check minimum required size of the signature...
      byte firstByte = this.SignatureMemoryReader.ReadByte();
      if (SignatureHeader.IsGeneric(firstByte)) {
        this.GenericParamCount = (ushort)this.SignatureMemoryReader.ReadCompressedUInt32();
      }
      int paramCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      bool dummyPinned;
      this.ReturnCustomModifiers = this.GetCustomModifiers(out dummyPinned);
      byte retByte = this.SignatureMemoryReader.PeekByte(0);
      if (retByte == ElementType.Void) {
        this.ReturnTypeReference = peFileToObjectModel.PlatformType.SystemVoid;
        this.SignatureMemoryReader.SkipBytes(1);
      } else if (retByte == ElementType.TypedReference) {
        this.ReturnTypeReference = peFileToObjectModel.PlatformType.SystemTypedReference;
        this.SignatureMemoryReader.SkipBytes(1);
      } else {
        if (retByte == ElementType.ByReference) {
          this.IsReturnByReference = true;
          this.SignatureMemoryReader.SkipBytes(1);
        }
        this.ReturnTypeReference = this.GetTypeReference();
      }
      if (paramCount > 0) {
        this.RequiredParameters = this.GetModuleParameterTypeInformations(moduleMethodRef, paramCount);
        if (this.RequiredParameters.Length < paramCount)
          this.VarArgParameters = this.GetModuleParameterTypeInformations(moduleMethodRef, paramCount - this.RequiredParameters.Length);
      }
    }

  }

  internal sealed class MethodSpecSignatureConverter : SignatureConverter {
    internal readonly EnumerableArrayWrapper<ITypeReference/*?*/, ITypeReference> GenericTypeArguments;
    //^ [NotDelayed]
    internal MethodSpecSignatureConverter(
      PEFileToObjectModel peFileToObjectModel,
      MetadataObject owningObject,
      MemoryReader signatureMemoryReader
    )
      : base(peFileToObjectModel, signatureMemoryReader, owningObject) {
      //^ this.GenericTypeArguments = TypeCache.EmptyTypeArray;
      //^ this.SignatureMemoryReader = signatureMemoryReader;
      //^ base;
      byte firstByte = this.SignatureMemoryReader.ReadByte();
      if (!SignatureHeader.IsGenericInstanceSignature(firstByte)) {
        //  MDError
      }
      int typeArgCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      ITypeReference/*?*/[] typeRefArr = new ITypeReference/*?*/[typeArgCount];
      for (int i = 0; i < typeArgCount; ++i) {
        typeRefArr[i] = this.GetTypeReference();
      }
      this.GenericTypeArguments = new EnumerableArrayWrapper<ITypeReference/*?*/, ITypeReference>(typeRefArr, Dummy.TypeReference);
    }
  }
  #endregion Signature Converters

}
