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
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Microsoft.Cci.Immutable;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci {

  /// <summary>
  /// Provides a standard abstraction over the applications that host components that provide or consume objects from the metadata model.
  /// </summary>
  [ContractVerification(true)]
  public abstract class MetadataHostEnvironment : IMetadataHost {

    /// <summary>
    /// Allocates an object that provides an abstraction over the application hosting compilers based on this framework.
    /// </summary>
    /// <param name="nameTable">
    /// A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.
    /// </param>
    /// <param name="factory">
    /// The intern factory to use when generating keys. When comparing two or more assemblies using
    /// TypeHelper, MemberHelper, etc. it is necessary to make the hosts use the same intern factory.
    /// </param>
    /// <param name="pointerSize">The size of a pointer on the runtime that is the target of the metadata units to be loaded
    /// into this metadta host. This parameter only matters if the host application wants to work out what the exact layout
    /// of a struct will be on the target runtime. The framework uses this value in methods such as TypeHelper.SizeOfType and
    /// TypeHelper.TypeAlignment. If the host application does not care about the pointer size it can provide 0 as the value
    /// of this parameter. In that case, the first reference to IMetadataHost.PointerSize will probe the list of loaded assemblies
    /// to find an assembly that either requires 32 bit pointers or 64 bit pointers. If no such assembly is found, the default is 32 bit pointers.
    /// </param>
    /// <param name="searchPaths">
    /// A collection of strings that are interpreted as valid paths which are used to search for units. May be null.
    /// </param>
    /// <param name="searchInGAC">
    /// Whether the GAC (Global Assembly Cache) should be searched when resolving references.
    /// </param>
    protected MetadataHostEnvironment(INameTable nameTable, IInternFactory factory, byte pointerSize, IEnumerable<string>/*?*/ searchPaths, bool searchInGAC) {
      Contract.Requires(nameTable != null);
      Contract.Requires(factory != null);
      Contract.Requires(pointerSize == 0 || pointerSize == 4 || pointerSize == 8);

      this.nameTable = nameTable;
      this.internFactory = factory;
      this.pointerSize = pointerSize;
      this.libPaths = searchPaths == null ? new List<string>(0) : new List<string>(searchPaths);
      this.SearchInGAC = searchInGAC;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.nameTable != null);
      Contract.Invariant(this.internFactory != null);
      Contract.Invariant(this.coreIdentities != null);
      //Contract.Invariant(this.pointerSize == 0 || this.pointerSize == 4 || this.pointerSize == 8);
    }

    /// <summary>
    /// Adds a new directory (path) to the list of search paths for which
    /// to look in when searching for a unit to load.
    /// </summary>
    /// <param name="path"></param>
    public virtual void AddLibPath(string path) {
      this.LibPaths.Add(path);
    }

    /// <summary>
    /// A potentially empty list of directory paths that will be searched when probing for an assembly reference.
    /// </summary>
    protected List<string> LibPaths {
      get {
        Contract.Ensures(Contract.Result<List<String>>() != null);
        if (this.libPaths == null)
          this.libPaths = new List<string>();
        return this.libPaths;
      }
    }
    List<string>/*?*/ libPaths;

    /// <summary>
    /// Sets or gets the boolean that determines if lookups of assemblies searches the GAC by default.
    /// </summary>
    public bool SearchInGAC { get; protected set; }

    /// <summary>
    /// The errors reported by this event are discovered in background threads by an opend ended
    /// set of error reporters. Listeners to this event should thus be prepared to be called at abitrary times from arbitrary threads.
    /// Each occurrence of the event concerns a particular source location and a particular error reporter.
    /// The reported error collection (possibly empty) supercedes any errors previously reported by the same error reporter for the same source location.
    /// A source location can be an entire ISourceDocument, or just a part of it (the latter would apply to syntax errors discovered by an incremental
    /// parser after an edit to the source document).
    /// </summary>
    public event EventHandler<Microsoft.Cci.ErrorEventArgs> Errors;

    /// <summary>
    /// The identity of the assembly containing Microsoft.Contracts.Contract.
    /// </summary>
    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        if (this.contractAssemblySymbolicIdentity == null)
          this.contractAssemblySymbolicIdentity = this.GetContractAssemblySymbolicIdentity();
        return this.contractAssemblySymbolicIdentity;
      }
    }
    AssemblyIdentity/*?*/ contractAssemblySymbolicIdentity;

    /// <summary>
    /// The set of identities that results from the individual opinions of all of the units loaded into this host.
    /// </summary>
    protected SetOfObjects CoreIdentities {
      get {
        Contract.Ensures(Contract.Result<SetOfObjects>() != null);
        return this.coreIdentities; 
      }
    }
    readonly SetOfObjects coreIdentities = new SetOfObjects();

    /// <summary>
    /// Returns the identity of the assembly containing the Microsoft.Contracts.Contract, by asking
    /// each of the loaded units for its opinion on the matter and returning the opinion with the highest version number.
    /// If none of the loaded units have an opinion, the result is the same as CoreAssemblySymbolicIdentity.
    /// </summary>
    protected virtual AssemblyIdentity GetContractAssemblySymbolicIdentity() {
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
      if (this.unitCache.Count > 0) {
        AssemblyIdentity/*?*/ result = null;
        lock (GlobalLock.LockingObject) {
          foreach (IUnit unit in this.unitCache.Values) {
            Contract.Assume(unit != null);
            AssemblyIdentity contractId = unit.ContractAssemblySymbolicIdentity;
            if (contractId.Name.Value.Length == 0) continue;
            if (result == null || result.Version < contractId.Version) result = contractId;
          }
        }
        if (result != null) return result;
      }
      return this.CoreAssemblySymbolicIdentity;
    }

    /// <summary>
    /// The identity of the assembly containing the core system types such as System.Object.
    /// </summary>
    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get {
        if (this.coreAssemblySymbolicIdentity == null)
          this.coreAssemblySymbolicIdentity = this.GetCoreAssemblySymbolicIdentity();
        return this.coreAssemblySymbolicIdentity;
      }
    }
    AssemblyIdentity/*?*/ coreAssemblySymbolicIdentity;

    /// <summary>
    /// Returns the identity of the assembly containing the core system types such as System.Object, by asking
    /// each of the loaded units for its opinion on the matter and returning the opinion with the highest version number.
    /// If none of the loaded units have an opinion, the identity of the runtime executing the compiler itself is returned.
    /// </summary>
    protected virtual AssemblyIdentity GetCoreAssemblySymbolicIdentity() {
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
      AssemblyIdentity/*?*/ result = null;
      IUnit referringUnit = Dummy.Unit;
      if (this.unitCache.Count > 0) {
        var dummyVersion = new Version(255, 255, 255, 255);
        lock (GlobalLock.LockingObject) {
          foreach (IUnit unit in this.unitCache.Values) {
            Contract.Assume(unit != null);
            AssemblyIdentity coreId = unit.CoreAssemblySymbolicIdentity;
            if (coreId.Name.Value.Length == 0) continue;
            this.coreIdentities.Add(coreId);
            if (result == null || result.Version == dummyVersion ||
               (result.Version < coreId.Version && coreId.Version != dummyVersion) ||
                result.Version == coreId.Version && unit.UnitIdentity.Equals(coreId)) {
              result = coreId;
              referringUnit = unit;
            }
          }
        }
      }
      if (result == null) {
        //If we get here, none of the assemblies in the unit cache has an opinion on the identity of the core assembly.
        //Usually this will be because this method was called before any assemblies have been loaded.
        //In this case, we have little option but to choose the identity of the core assembly of the platform we are running on.
        var coreAssemblyName = typeof(object).Assembly.GetName();
        var version = coreAssemblyName.Version;
        Contract.Assume(version != null);
        var publicKeyToken = coreAssemblyName.GetPublicKeyToken();
        Contract.Assume(publicKeyToken != null);
        result = new AssemblyIdentity(this.NameTable.GetNameFor(coreAssemblyName.Name), "", version, publicKeyToken, "");
      }
      if (result.Location.Length == 0) {
        //We either found a plausible identity by polling the assemblies in the unit cache, or we used the identity of our own core assembly.
        //However, we defer to ProbeAssemblyReference to find an actual location for the assembly.
        //(Note that if result.Location.Length > 0, then the core assembly has already been loaded and we thus know the location and don't have to probe.)
        this.coreAssemblySymbolicIdentity = result; //in case ProbeAssemblyReference wants to know the core identity
        result = this.ProbeAssemblyReference(referringUnit, result);
      }
      return this.coreAssemblySymbolicIdentity = result;
    }

    /// <summary>
    /// The identity of the System.Core assembly.
    /// </summary>
    public AssemblyIdentity SystemCoreAssemblySymbolicIdentity {
      get {
        if (this.systemCoreAssemblySymbolicIdentity == null)
          this.systemCoreAssemblySymbolicIdentity = this.GetSystemCoreAssemblySymbolicIdentity();
        return this.systemCoreAssemblySymbolicIdentity;
      }
    }
    AssemblyIdentity/*?*/ systemCoreAssemblySymbolicIdentity;

    /// <summary>
    /// Returns an identity that is the same as CoreAssemblyIdentity, except that the name is "System.Core" and the version is at least 3.5.
    /// </summary>
    protected virtual AssemblyIdentity GetSystemCoreAssemblySymbolicIdentity() {
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
      var core = this.CoreAssemblySymbolicIdentity;
      var name = this.NameTable.GetNameFor("System.Core");
      var location = "";
      if (core.Location.Length > 0)
        location = Path.Combine(Path.GetDirectoryName(core.Location)??"", "System.Core.dll");
      var version = new Version(3, 5, 0, 0);
      if (version < core.Version) version = core.Version;
      return new AssemblyIdentity(name, core.Culture, version, core.PublicKeyToken, location);
    }

    /// <summary>
    /// Finds the assembly that matches the given identifier among the already loaded set of assemblies,
    /// or a dummy assembly if no matching assembly can be found.
    /// </summary>
    public IAssembly FindAssembly(AssemblyIdentity assemblyIdentity) {
      IUnit/*?*/ unit;
      lock (GlobalLock.LockingObject) {
        this.unitCache.TryGetValue(assemblyIdentity, out unit);
      }
      IAssembly/*?*/ result = unit as IAssembly;
      if (result != null)
        return result;
      return Dummy.Assembly;
    }

    /// <summary>
    /// Finds the module that matches the given identifier among the already loaded set of modules,
    /// or a dummy module if no matching module can be found.
    /// </summary>
    public IModule FindModule(ModuleIdentity moduleIdentity) {
      IUnit/*?*/ unit;
      lock (GlobalLock.LockingObject) {
        this.unitCache.TryGetValue(moduleIdentity, out unit);
      }
      IModule/*?*/ result = unit as IModule;
      if (result != null)
        return result;
      return Dummy.Module;
    }

    /// <summary>
    /// Finds the unit that matches the given identifier, or a dummy unit if no matching unit can be found.
    /// </summary>
    public IUnit FindUnit(UnitIdentity unitIdentity) {
      IUnit/*?*/ unit;
      lock (GlobalLock.LockingObject) {
        this.unitCache.TryGetValue(unitIdentity, out unit);
      }
      if (unit != null)
        return unit;
      return Dummy.Unit;
    }

    /// <summary>
    /// Returns the CodeBase of the named assembly (which is a URL), except if the URL has the file scheme.
    /// In that case the URL is converted to a local file path that can be used by System.IO.Path methods.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly whose location is desired.</param>
    public static string GetLocalPath(System.Reflection.AssemblyName assemblyName) {
      Contract.Requires(assemblyName != null);

      var loc = assemblyName.CodeBase;
      if (loc == null) loc = "";
      if (loc.StartsWith("file://", StringComparison.OrdinalIgnoreCase)) {
        Uri u = new Uri(loc, UriKind.Absolute);
        loc = u.LocalPath;
      }
      return loc;
    }

    /// <summary>
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    public IInternFactory InternFactory {
      get { return this.internFactory; }
    }
    readonly IInternFactory internFactory;

    /// <summary>
    /// The assembly that matches the given reference, or a dummy assembly if no matching assembly can be found.
    /// </summary>
    public virtual IAssembly LoadAssembly(AssemblyIdentity assemblyIdentity) {
      IUnit/*?*/ unit;
      lock (GlobalLock.LockingObject) {
        this.unitCache.TryGetValue(assemblyIdentity, out unit);
      }
      IAssembly/*?*/ result;
      if (unit != null)
        result = unit as IAssembly;
      else {
        if (string.IsNullOrEmpty(assemblyIdentity.Location) || string.Equals(assemblyIdentity.Location, "unknown://location", StringComparison.OrdinalIgnoreCase)) {
          result = Dummy.Assembly;
          lock (GlobalLock.LockingObject) {
            this.unitCache.Add(assemblyIdentity, result);
          }
        } else {
          unit = this.LoadUnitFrom(assemblyIdentity.Location);
          result = unit as IAssembly;
          if (result != null && this.UnifyAssembly(result).Equals(assemblyIdentity))
            lock (GlobalLock.LockingObject) {
              this.unitCache[assemblyIdentity] = result;
              this.coreIdentities.Add(result.CoreAssemblySymbolicIdentity);
            }
        }
      }
      if (result == null) result = Dummy.Assembly;
      return result;
    }

    /// <summary>
    /// The module that matches the given reference, or a dummy module if no matching module can be found.
    /// </summary>
    public virtual IModule LoadModule(ModuleIdentity moduleIdentity) {
      IUnit/*?*/ unit;
      lock (GlobalLock.LockingObject) {
        this.unitCache.TryGetValue(moduleIdentity, out unit);
      }
      IModule/*?*/ result;
      if (unit != null)
        result = unit as IModule;
      else {
        if (moduleIdentity.Location == "" || moduleIdentity.Location == "unknown://location") {
          result = Dummy.Module;
          lock (GlobalLock.LockingObject) {
            this.unitCache.Add(moduleIdentity, result);
          }
        } else {
          unit = this.LoadUnitFrom(moduleIdentity.Location);
          result = unit as IModule;
          if (result != null)
            lock (GlobalLock.LockingObject) {
              this.unitCache.Add(moduleIdentity, result);
              this.coreIdentities.Add(result.CoreAssemblySymbolicIdentity);
            }
        }
      }
      if (result == null) result = Dummy.Module;
      return result;
    }

    /// <summary>
    /// The unit that matches the given identity, or a dummy unit if no matching unit can be found.
    /// </summary>
    public IUnit LoadUnit(UnitIdentity unitIdentity) {
      AssemblyIdentity/*?*/ assemblyIdentity = unitIdentity as AssemblyIdentity;
      if (assemblyIdentity != null) return this.LoadAssembly(assemblyIdentity);
      ModuleIdentity/*?*/ moduleIdentity = unitIdentity as ModuleIdentity;
      if (moduleIdentity != null) return this.LoadModule(moduleIdentity);
      return this.LoadUnitFrom(unitIdentity.Location);
    }

    /// <summary>
    /// Returns the unit that is stored at the given location, or a dummy unit if no unit exists at that location or if the unit at that location is not accessible.
    /// Implementations should do enough caching to avoid repeating work: this method gets called very often for already loaded units as part of the probing logic.
    /// </summary>
    public abstract IUnit LoadUnitFrom(string location);

    /// <summary>
    /// Returns enumeration of all the units loaded so far.
    /// </summary>
    public IEnumerable<IUnit> LoadedUnits {
      get {
        lock (GlobalLock.LockingObject) {
          return new List<IUnit>(this.unitCache.Values).AsReadOnly();
        }
      }
    }

    /// <summary>
    /// A table used to intern strings used as names.
    /// </summary>
    public INameTable NameTable {
      get { return this.nameTable; }
    }
    readonly INameTable nameTable;

    /// <summary>
    /// A collection of references to types from the core platform, such as System.Object and System.String.
    /// </summary>
    public IPlatformType PlatformType {
      get {
        if (this.platformType == null)
          this.platformType = this.GetPlatformType();
        return this.platformType;
      }
    }
    IPlatformType/*?*/ platformType;

    /// <summary>
    /// Returns an object that provides a collection of references to types from the core platform, such as System.Object and System.String.
    /// </summary>
    protected virtual IPlatformType GetPlatformType() {
      Contract.Ensures(Contract.Result<IPlatformType>() != null);
      return new PlatformType(this);
    }

    /// <summary>
    /// The size (in bytes) of a pointer on the platform on which the host is targetting.
    /// The value of this property is either 4 (32-bits) or 8 (64-bit).
    /// </summary>
    public byte PointerSize {
      get {
        Contract.Assume(this.pointerSize == 0 || this.pointerSize == 4 || this.pointerSize == 8);
        if (this.pointerSize == 0)
          this.pointerSize = this.GetTargetPlatformPointerSize();
        return this.pointerSize;
      }
    }
    byte pointerSize;

    /// <summary>
    /// Returns an opinion about the size of a pointer on the target runtime for the set of modules
    /// currently in this.unitCache. If none of the modules requires either 32 bit pointers or 64 bit pointers
    /// the result is 4 (i.e. 32 bit pointers). This method is only called if a host application has not
    /// explicitly provided the pointer size of the target platform.
    /// </summary>
    protected virtual byte GetTargetPlatformPointerSize() {
      Contract.Ensures(Contract.Result<byte>() == 4 || Contract.Result<byte>() == 8);
      lock (GlobalLock.LockingObject) {
        if (this.unitCache.Count > 0) {
          foreach (IUnit unit in this.unitCache.Values) {
            IModule/*?*/ module = unit as IModule;
            if (module == null) continue;
            if (module.Requires32bits) return 4;
            if (module.Requires64bits) return 8;
          }
        }
      }
      return 4;
    }

    /// <summary>
    /// Looks in the specified <paramref name="probeDir"/> to see if a file
    /// exists, first with the extension "dll" and then with the extensions "winmd" and "exe".
    /// Returns null if not found, otherwise constructs a new AssemblyIdentity
    /// </summary>
    protected virtual AssemblyIdentity/*?*/ Probe(string probeDir, AssemblyIdentity referencedAssembly) {
      Contract.Requires(probeDir != null);
      Contract.Requires(referencedAssembly != null);

      string path = Path.Combine(probeDir, referencedAssembly.Name.Value + ".dll");
      if (!File.Exists(path)) path = Path.Combine(probeDir, referencedAssembly.Name.Value + ".winmd");
      if (!File.Exists(path)) path = Path.Combine(probeDir, referencedAssembly.Name.Value + ".exe");
      if (!File.Exists(path)) return null;
      var assembly = this.LoadUnitFrom(path) as IAssembly;
      if (assembly == null) return null;
      if (!assembly.AssemblyIdentity.Equals(referencedAssembly)) return null;
      return assembly.AssemblyIdentity;
    }

    /// <summary>
    /// Given the identity of a referenced assembly (but not its location), apply host specific policies for finding the location
    /// of the referenced assembly.
    /// Returns an assembly identity that matches the given referenced assembly identity, but which includes a location.
    /// If the probe failed to find the location of the referenced assembly, the location will be "unknown://location".
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the assembly. It will have been loaded from somewhere and thus
    /// has a known location, which will typically be probed for the referenced assembly.</param>
    /// <param name="referencedAssembly">The assembly being referenced. This will not have a location since there is no point in probing
    /// for the location of an assembly when you already know its location.</param>
    /// <returns>
    /// An assembly identity that matches the given referenced assembly identity, but which includes a location.
    /// If the probe failed to find the location of the referenced assembly, the location will be "unknown://location".
    /// </returns>
    /// <remarks>
    /// Looks for the referenced assembly first in the same directory as the referring unit, then
    /// in any search paths provided to the constructor, then finally the GAC.
    /// </remarks>
    [Pure]
    public virtual AssemblyIdentity ProbeAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly) {
      AssemblyIdentity result;

      if (!string.IsNullOrEmpty(referringUnit.Location)) {
        // probe for in the same directory as the referring unit
        var referringDir = Path.GetDirectoryName(Path.GetFullPath(referringUnit.Location));
        result = this.Probe(referringDir, referencedAssembly);
        if (result != null) return result;
      }

      // Probe in the libPaths directories
      foreach (string libPath in this.LibPaths) {
        Contract.Assume(libPath != null);
        result = this.Probe(libPath, referencedAssembly);
        if (result != null) return result;
      }

      // Check GAC
#if !COMPACTFX
      if (this.SearchInGAC) {
        string/*?*/ gacLocation = GlobalAssemblyCache.GetLocation(referencedAssembly, this);
        if (gacLocation != null) {
          return new AssemblyIdentity(referencedAssembly, gacLocation);
        }
      }
#endif

      // Check platform location
      var platformDir = Path.GetDirectoryName(Path.GetFullPath(GetLocalPath(typeof(object).Assembly.GetName())))??"";
      var coreVersion = this.CoreAssemblySymbolicIdentity.Version;
      if (coreVersion.Major == 1) {
        if (coreVersion.Minor == 0)
          platformDir = Path.Combine(Path.GetDirectoryName(platformDir)??"", "v1.0.3705");
        else if (coreVersion.Minor == 1)
          platformDir = Path.Combine(Path.GetDirectoryName(platformDir)??"", "v1.1.4322");
      } else if (coreVersion.Major == 2) {
        platformDir = Path.Combine(Path.GetDirectoryName(platformDir)??"", "v3.5");
        result = this.Probe(platformDir, referencedAssembly);
        if (result != null) return result;
        platformDir = Path.Combine(Path.GetDirectoryName(platformDir)??"", "v3.0");
        result = this.Probe(platformDir, referencedAssembly);
        if (result != null) return result;
        platformDir = Path.Combine(Path.GetDirectoryName(platformDir)??"", "v2.0.50727");
      } else if (coreVersion.Major == 4) {
        platformDir = Path.Combine(Path.GetDirectoryName(platformDir)??"", "v4.0.30319");
      }

      result = this.Probe(platformDir, referencedAssembly);
      if (result != null) return result;

      // Give up
      return new AssemblyIdentity(referencedAssembly, "unknown://location");
    }

    /// <summary>
    /// Given the identity of a referenced module (but not its location), apply host specific policies for finding the location
    /// of the referenced module.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the module. It will have been loaded from somewhere and thus
    /// has a known location, which will typically be probed for the referenced module.</param>
    /// <param name="referencedModule">Module being referenced.</param>
    /// <returns>
    /// A module identity that matches the given referenced module identity, but which includes a location.
    /// If the probe failed to find the location of the referenced assembly, the location will be "unknown://location".
    /// </returns>
    /// <remarks>
    /// Default implementation of ProbeModuleReference. Override this method to change the behavior.
    /// </remarks>
    [Pure]
    public virtual ModuleIdentity ProbeModuleReference(IUnit referringUnit, ModuleIdentity referencedModule) {
      return new ModuleIdentity(referencedModule.Name, "unknown://location", referencedModule.ContainingAssembly);
    }

    /// <summary>
    /// Registers the given unit as the latest one associated with the unit's location.
    /// Such units can then be discovered by clients via GetUnit. 
    /// </summary>
    /// <param name="unit">The unit to register.</param>
    public void RegisterAsLatest(IUnit unit) {
      Contract.Requires(unit != null);

      lock (GlobalLock.LockingObject) {
        this.unitCache[unit.UnitIdentity] = unit;
      }
    }

    /// <summary>
    /// Removes the unit with the given identity.
    /// Returns true iff the unitIdentity is found in the loaded units.
    /// </summary>
    public bool RemoveUnit(UnitIdentity unitIdentity) {
      lock (GlobalLock.LockingObject) {
        return this.unitCache.Remove(unitIdentity);
      }
    }

    /// <summary>
    /// Raises the Errors event with the given error event arguments.
    /// The event is raised on a separate thread.
    /// </summary>
    public virtual void ReportErrors(Microsoft.Cci.ErrorEventArgs errorEventArguments) {
      if (this.Errors != null)
        ThreadPool.QueueUserWorkItem(this.SynchronousReportErrors, errorEventArguments);
    }

    /// <summary>
    /// Raises the Errors event with the given error event arguments.
    /// </summary>
    /// <param name="state">The error event arguments.</param>
    protected void SynchronousReportErrors(object/*?*/ state)
      //^ requires state is Microsoft.Cci.ErrorEventArgs;
    {
      var errorEventArguments = (ErrorEventArgs)state;
      if (this.Errors != null)
        this.Errors(this, errorEventArguments);
    }

    /// <summary>
    /// Raises the CompilationErrors event with the given error wrapped up in an error event arguments object.
    /// The event is raised on a separate thread.
    /// </summary>
    /// <param name="error">The error to report.</param>
    public void ReportError(IErrorMessage error) {
      if (this.Errors != null) {
        List<IErrorMessage> errors = new List<IErrorMessage>(1);
        errors.Add(error);
        Microsoft.Cci.ErrorEventArgs errorEventArguments = new Microsoft.Cci.ErrorEventArgs(error.ErrorReporter, error.Location, errors.AsReadOnly());
        this.ReportErrors(errorEventArguments);
      }
    }

    /// <summary>
    /// A cache containing all of the units of metadata that have been loaded via this host.
    /// Entries may be added to this cache as a side effect of a metadata reader that lazily fills in the object model.
    /// As a result, any access to this cache must be protected by the global lock, as it cannot be assumed that
    /// all mutations to this object will happen during setup while the host object is exclusive to a single thread.
    /// </summary>
    readonly Dictionary<UnitIdentity, IUnit> unitCache = new Dictionary<UnitIdentity, IUnit>();

    /// <summary>
    /// Default implementation of UnifyAssembly. Override this method to change the behavior.
    /// </summary>
    public virtual AssemblyIdentity UnifyAssembly(AssemblyIdentity assemblyIdentity) {
      if (assemblyIdentity.Name.UniqueKeyIgnoringCase == this.CoreAssemblySymbolicIdentity.Name.UniqueKeyIgnoringCase &&
        assemblyIdentity.Culture == this.CoreAssemblySymbolicIdentity.Culture && 
        IteratorHelper.EnumerablesAreEqual(assemblyIdentity.PublicKeyToken, this.CoreAssemblySymbolicIdentity.PublicKeyToken))
        return this.CoreAssemblySymbolicIdentity;
      if (this.CoreIdentities.Contains(assemblyIdentity)) return this.CoreAssemblySymbolicIdentity;
      return assemblyIdentity;
    }

    /// <summary>
    /// Default implementation of UnifyAssembly. Override this method to change the behavior.
    /// </summary>
    public virtual AssemblyIdentity UnifyAssembly(IAssemblyReference assemblyReference) {
      return this.UnifyAssembly(assemblyReference.AssemblyIdentity);
    }

    /// <summary>
    /// True if IL locations should be preserved up into the code model by decompilers using this host.
    /// </summary>
    public virtual bool PreserveILLocations { get { return false; } }

  }

  /// <summary>
  /// Static class encasulating the global lock object.
  /// </summary>
  public static class GlobalLock {
    /// <summary>
    /// All synchronization code should exclusively use this lock object,
    /// hence making it trivial to ensure that there are no deadlocks.
    /// It also means that the lock should never be held for long.
    /// In particular, no code holding this lock should ever wait on another thread.
    /// </summary>
    public static readonly object LockingObject = new object();
  }

  /// <summary>
  /// An interface provided by the application hosting the metadata reader. The interface allows the host application
  /// to control how assembly references are unified, where files are found and so on.
  /// </summary>
  [ContractClass(typeof(IMetadataReaderHostContract))]
  public interface IMetadataReaderHost : IMetadataHost {
    /// <summary>
    /// Open the binary document as a memory block in host dependent fashion.
    /// </summary>
    /// <param name="sourceDocument">The binary document that is to be opened.</param>
    /// <returns>The unmanaged memory block corresponding to the source document.</returns>
    IBinaryDocumentMemoryBlock/*?*/ OpenBinaryDocument(IBinaryDocument sourceDocument);

    /// <summary>
    /// Open the child binary document within the context of parent source document.as a memory block in host dependent fashion 
    /// For example: in multimodule assemblies the main module will be parentSourceDocument, where as other modules will be child
    /// docuements.
    /// </summary>
    /// <param name="parentSourceDocument">The source document indicating the child document location.</param>
    /// <param name="childDocumentName">The name of the child document.</param>
    /// <returns>The unmanaged memory block corresponding to the child document.</returns>
    IBinaryDocumentMemoryBlock/*?*/ OpenBinaryDocument(IBinaryDocument parentSourceDocument, string childDocumentName);

    /// <summary>
    /// Provides the host with an opportunity to add, remove or substitute assembly references in the given list.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that contains these references.</param>
    /// <param name="assemblyReferences">The assembly references to substitute.</param>
    /// <returns>Usually assemblyReferences, but occasionally a modified enumeration.</returns>
    IEnumerable<IAssemblyReference> Redirect(IUnit referringUnit, IEnumerable<IAssemblyReference> assemblyReferences);

    /// <summary>
    /// Provides the host with an opportunity to substitute one type reference for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that contains the reference.</param>
    /// <param name="typeReference">A named type reference encountered during metadata reading.</param>
    /// <returns>Usually the value in typeReference, but occassionally something else.</returns>
    INamedTypeReference Redirect(IUnit referringUnit, INamedTypeReference typeReference);

    /// <summary>
    /// Provides the host with an opportunity to substitute a custom attribute with another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that contains the custom attribute.</param>
    /// <param name="customAttribute">The custom attribute to rewrite (fix up).</param>
    /// <returns>Usually the value in customAttribute, but occassionally another custom attribute.</returns>
    ICustomAttribute Rewrite(IUnit containingUnit, ICustomAttribute customAttribute);

    /// <summary>
    /// Provides the host with an opportunity to substitute one method definition for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that is defines the method.</param>
    /// <param name="methodDefinition">A method definition encountered during metadata reading.</param>
    /// <returns>Usually the value in methodDefinition, but occassionally something else.</returns>
    IMethodDefinition Rewrite(IUnit containingUnit, IMethodDefinition methodDefinition);

    /// <summary>
    /// This method is called when the assembly reference is being resolved and its not already loaded by the host.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the assembly.</param>
    /// <param name="referencedAssembly">Assembly identifier for the assembly being referenced.</param>
    void ResolvingAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly);

    /// <summary>
    /// This method is called when the module reference is being resolved and its not already loaded by the host.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the module.</param>
    /// <param name="referencedModule">Module identifier for the assembly being referenced.</param>
    void ResolvingModuleReference(IUnit referringUnit, ModuleIdentity referencedModule);

    /// <summary>
    /// Called by the metadata reader when it is about to start parsing a custom attribute blob.
    /// </summary>
    void StartGuessingGame();

    /// <summary>
    /// Called by the metadata reader when it has unsucessfully tried to parse a custom attribute blob and it now needs to try a new permutation.
    /// Returns false if no more perumations are possible.
    /// </summary>
    bool TryNextPermutation();

    /// <summary>
    /// Called by the metadata reader when it has successfully parsed a custom attribute blob.
    /// </summary>
    void WinGuessingGame();

    /// <summary>
    /// Returns a guess of the size of the underlying type of the given type reference to an enum type, which is assumed to be unresolvable
    /// because it is defined an assembly that is not loaded into this host. Successive calls to the method will cycle through these values
    /// with a periodicity determined by the number of types in the game and the successful guesses made in earlier games.
    /// </summary>
    /// <param name="reference">A type reference that cannot be resolved.</param>
    /// <returns>1, 2, 4 or 8.</returns>
    byte GuessUnderlyingTypeSizeOfUnresolvableReferenceToEnum(ITypeReference reference);

  }

  #region IMetadataReaderHost contract binding
  [ContractClassFor(typeof(IMetadataReaderHost))]
  abstract class IMetadataReaderHostContract : IMetadataReaderHost {
    #region IMetadataReaderHost Members

    /// <summary>
    /// Open the binary document as a memory block in host dependent fashion.
    /// </summary>
    /// <param name="sourceDocument">The binary document that is to be opened.</param>
    /// <returns>
    /// The unmanaged memory block corresponding to the source document.
    /// </returns>
    public IBinaryDocumentMemoryBlock OpenBinaryDocument(IBinaryDocument sourceDocument) {
      Contract.Requires(sourceDocument != null);
      // MaF: too strong a contract here Contract.Ensures(Contract.Result<IBinaryDocumentMemoryBlock>() != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Open the child binary document within the context of parent source document.as a memory block in host dependent fashion
    /// For example: in multimodule assemblies the main module will be parentSourceDocument, where as other modules will be child
    /// docuements.
    /// </summary>
    /// <param name="parentSourceDocument">The source document indicating the child document location.</param>
    /// <param name="childDocumentName">The name of the child document.</param>
    /// <returns>
    /// The unmanaged memory block corresponding to the child document.
    /// </returns>
    public IBinaryDocumentMemoryBlock OpenBinaryDocument(IBinaryDocument parentSourceDocument, string childDocumentName) {
      Contract.Requires(parentSourceDocument != null);
      Contract.Requires(childDocumentName != null);
      Contract.Ensures(Contract.Result<IBinaryDocumentMemoryBlock>() != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Provides the host with an opportunity to add, remove or substitute assembly references in the given list.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that contains these references.</param>
    /// <param name="assemblyReferences">The assembly references to substitute.</param>
    /// <returns>
    /// Usually assemblyReferences, but occasionally a modified enumeration.
    /// </returns>
    public IEnumerable<IAssemblyReference> Redirect(IUnit referringUnit, IEnumerable<IAssemblyReference> assemblyReferences) {
      Contract.Requires(referringUnit != null);
      Contract.Requires(assemblyReferences != null);
      Contract.Ensures(Contract.Result<IEnumerable<IAssemblyReference>>() != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute one type reference for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that contains the reference.</param>
    /// <param name="typeReference">A named type reference encountered during metadata reading.</param>
    /// <returns>
    /// Usually the value in typeReference, but occassionally something else.
    /// </returns>
    public INamedTypeReference Redirect(IUnit referringUnit, INamedTypeReference typeReference) {
      Contract.Requires(referringUnit != null);
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<INamedTypeReference>() != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute a custom attribute with another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that contains the custom attribute.</param>
    /// <param name="customAttribute">The custom attribute to rewrite (fix up).</param>
    /// <returns>
    /// Usually the value in customAttribute, but occassionally another custom attribute.
    /// </returns>
    public ICustomAttribute Rewrite(IUnit containingUnit, ICustomAttribute customAttribute) {
      Contract.Requires(containingUnit != null);
      Contract.Requires(customAttribute != null);
      Contract.Ensures(Contract.Result<ICustomAttribute>() != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute one method definition for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that is defines the method.</param>
    /// <param name="methodDefinition">A method definition encountered during metadata reading.</param>
    /// <returns>
    /// Usually the value in methodDefinition, but occassionally something else.
    /// </returns>
    public IMethodDefinition Rewrite(IUnit containingUnit, IMethodDefinition methodDefinition) {
      Contract.Requires(containingUnit != null);
      Contract.Requires(methodDefinition != null);
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// This method is called when the assembly reference is being resolved and its not already loaded by the host.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the assembly.</param>
    /// <param name="referencedAssembly">Assembly identifier for the assembly being referenced.</param>
    public void ResolvingAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly) {
      Contract.Requires(referringUnit != null);
      Contract.Requires(referencedAssembly != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// This method is called when the module reference is being resolved and its not already loaded by the host.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the module.</param>
    /// <param name="referencedModule">Module identifier for the assembly being referenced.</param>
    public void ResolvingModuleReference(IUnit referringUnit, ModuleIdentity referencedModule) {
      Contract.Requires(referringUnit != null);
      Contract.Requires(referencedModule != null);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Called by the metadata reader when it is about to start parsing a custom attribute blob.
    /// </summary>
    public void StartGuessingGame() {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Called by the metadata reader when it has unsucessfully tried to parse a custom attribute blob and it now needs to try a new permutation.
    /// Returns false if no more perumations are possible.
    /// </summary>
    /// <returns></returns>
    public bool TryNextPermutation() {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Called by the metadata reader when it has successfully parsed a custom attribute blob.
    /// </summary>
    public void WinGuessingGame() {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a guess of the size of the underlying type of the given type reference to an enum type, which is assumed to be unresolvable
    /// because it is defined an assembly that is not loaded into this host. Successive calls to the method will cycle through these values
    /// with a periodicity determined by the number of types in the game and the successful guesses made in earlier games.
    /// </summary>
    /// <param name="reference">A type reference that cannot be resolved.</param>
    /// <returns>
    /// 1, 2, 4 or 8.
    /// </returns>
    public byte GuessUnderlyingTypeSizeOfUnresolvableReferenceToEnum(ITypeReference reference) {
      Contract.Requires(reference != null);
      Contract.Ensures(Contract.Result<byte>() == 1 || Contract.Result<byte>() == 2 || Contract.Result<byte>() == 4 || Contract.Result<byte>() == 8);
      throw new NotImplementedException();
    }

    #endregion

    #region IMetadataHost Members

#pragma warning disable 67
    public event EventHandler<ErrorEventArgs> Errors;
#pragma warning restore 67

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity SystemCoreAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public IAssembly FindAssembly(AssemblyIdentity assemblyIdentity) {
      throw new NotImplementedException();
    }

    public IModule FindModule(ModuleIdentity moduleIdentity) {
      throw new NotImplementedException();
    }

    public IUnit FindUnit(UnitIdentity unitIdentity) {
      throw new NotImplementedException();
    }

    public IInternFactory InternFactory {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public IAssembly LoadAssembly(AssemblyIdentity assemblyIdentity) {
      throw new NotImplementedException();
    }

    public IModule LoadModule(ModuleIdentity moduleIdentity) {
      throw new NotImplementedException();
    }

    public IUnit LoadUnit(UnitIdentity unitIdentity) {
      throw new NotImplementedException();
    }

    public IUnit LoadUnitFrom(string location) {
      throw new NotImplementedException();
    }

    public IEnumerable<IUnit> LoadedUnits {
      get { throw new NotImplementedException(); }
    }

    public INameTable NameTable {
      get { throw new NotImplementedException(); }
    }

    public byte PointerSize {
      get { throw new NotImplementedException(); }
    }

    public void ReportErrors(ErrorEventArgs errorEventArguments) {
      throw new NotImplementedException();
    }

    public void ReportError(IErrorMessage error) {
      throw new NotImplementedException();
    }

    public AssemblyIdentity ProbeAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly) {
      throw new NotImplementedException();
    }

    public ModuleIdentity ProbeModuleReference(IUnit referringUnit, ModuleIdentity referencedModule) {
      throw new NotImplementedException();
    }

    public AssemblyIdentity UnifyAssembly(AssemblyIdentity assemblyIdentity) {
      throw new NotImplementedException();
    }

    public AssemblyIdentity UnifyAssembly(IAssemblyReference assemblyReference) {
      throw new NotImplementedException();
    }

    public bool PreserveILLocations {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A base class for an object provided by the application hosting the metadata reader. The object allows the host application
  /// to control how assembly references are unified, where files are found and so on. The object also controls the lifetime
  /// of things such as memory mapped files and blocks of unmanaged memory. Be sure to call Dispose on the object when
  /// it is no longer needed and the associated locks and/or memory must be released immediately.
  /// </summary>
  public abstract class MetadataReaderHost : MetadataHostEnvironment, IMetadataReaderHost, IDisposable {

    /// <summary>
    /// Allocates an object that provides an abstraction over the application hosting compilers based on this framework.
    /// Remember to call the Dispose method when the resulting object is no longer needed.
    /// </summary>
    /// <param name="nameTable">
    /// A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.
    /// </param>
    /// <param name="factory">
    /// The intern factory to use when generating keys. When comparing two or more assemblies using
    /// TypeHelper, MemberHelper, etc. it is necessary to make the hosts use the same intern factory.
    /// </param>
    /// <param name="pointerSize">The size of a pointer on the runtime that is the target of the metadata units to be loaded
    /// into this metadta host. This parameter only matters if the host application wants to work out what the exact layout
    /// of a struct will be on the target runtime. The framework uses this value in methods such as TypeHelper.SizeOfType and
    /// TypeHelper.TypeAlignment. If the host application does not care about the pointer size it can provide 0 as the value
    /// of this parameter. In that case, the first reference to IMetadataHost.PointerSize will probe the list of loaded assemblies
    /// to find an assembly that either requires 32 bit pointers or 64 bit pointers. If no such assembly is found, the default is 32 bit pointers.
    /// </param>
    /// <param name="searchPaths">
    /// A collection of strings that are interpreted as valid paths which are used to search for units.
    /// </param>
    /// <param name="searchInGAC">
    /// Whether the GAC (Global Assembly Cache) should be searched when resolving references.
    /// </param>
    protected MetadataReaderHost(INameTable nameTable, IInternFactory factory, byte pointerSize, IEnumerable<string> searchPaths, bool searchInGAC)
      : base(nameTable, factory, pointerSize, searchPaths, searchInGAC) {
      Contract.Requires(pointerSize == 0 || pointerSize == 4 || pointerSize == 8);
    }

    /// <summary>
    /// Calls IDiposable.Dispose on any disposable objects allocated by this host environment.
    /// </summary>
    ~MetadataReaderHost() {
      this.Close();
    }

    /// <summary>
    /// Calls IDiposable.Dispose on any disposable objects allocated by this host environment.
    /// </summary>
    public virtual void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Calls IDiposable.Dispose on any disposable objects allocated by this host environment.
    /// </summary>
    private void Close() {
      foreach (var disposable in this.disposableObjectAllocatedByThisHost)
        disposable.Dispose();
    }

    /// <summary>
    /// A list of all of the IDisposable object that have been allocated by this host.
    /// </summary>
    protected List<IDisposable> disposableObjectAllocatedByThisHost = new List<IDisposable>();

    /// <summary>
    /// Open the binary document as a memory block in host dependent fashion.
    /// IMPORTANT: The lifetime of the memory block is the same as the lifetime of this host object.
    /// Be sure to call Dispose on the host object when it is no longer needed, otherwise files may stay locked,
    /// or resources such as large blocks of memory may remain allocated until the host's finalizer method runs.
    /// </summary>
    /// <param name="sourceDocument">The binary document that is to be opened.</param>
    /// <returns>The unmanaged memory block corresponding to the source document.</returns>
    /// <remarks>When overridding this method, be sure to add any disposable objects to this.disposableObjectAllocatedByThisHost.</remarks>
    public virtual IBinaryDocumentMemoryBlock/*?*/ OpenBinaryDocument(IBinaryDocument sourceDocument) {
      try {
#if !COMPACTFX && !__MonoCS__
        IBinaryDocumentMemoryBlock binDocMemoryBlock = MemoryMappedFile.CreateMemoryMappedFile(sourceDocument.Location, sourceDocument);
#else
        IBinaryDocumentMemoryBlock binDocMemoryBlock = UnmanagedBinaryMemoryBlock.CreateUnmanagedBinaryMemoryBlock(sourceDocument.Location, sourceDocument);
#endif
        this.disposableObjectAllocatedByThisHost.Add((IDisposable)binDocMemoryBlock);
        return binDocMemoryBlock;
      } catch (IOException) {
        return null;
      }
    }

    /// <summary>
    /// Open the child binary document within the context of parent source document.as a memory block in host dependent fashion 
    /// For example: in multimodule assemblies the main module will be parentSourceDocument, where as other modules will be child
    /// documents.
    /// IMPORTANT: The lifetime of the memory block is the same as the lifetime of this host object.
    /// Be sure to call Dispose on the host object when it is no longer needed, otherwise files may stay locked,
    /// or resources such as large blocks of memory may remain allocated until the host's finalizer method runs.
    /// </summary>
    /// <param name="parentSourceDocument">The source document indicating the child document location.</param>
    /// <param name="childDocumentName">The name of the child document.</param>
    /// <returns>The unmanaged memory block corresponding to the child document.</returns>
    /// <remarks>When overridding this method, be sure to add any disposable objects to this.disposableObjectAllocatedByThisHost.</remarks>
    public virtual IBinaryDocumentMemoryBlock/*?*/ OpenBinaryDocument(IBinaryDocument parentSourceDocument, string childDocumentName) {
      try {
        var directory = Path.GetDirectoryName(parentSourceDocument.Location)??"";
        var fullPath = Path.Combine(directory, childDocumentName);
        IBinaryDocument newBinaryDocument = BinaryDocument.GetBinaryDocumentForFile(fullPath, this);
#if !COMPACTFX && !__MonoCS__
        IBinaryDocumentMemoryBlock binDocMemoryBlock = MemoryMappedFile.CreateMemoryMappedFile(newBinaryDocument.Location, newBinaryDocument);
#else
        IBinaryDocumentMemoryBlock binDocMemoryBlock = UnmanagedBinaryMemoryBlock.CreateUnmanagedBinaryMemoryBlock(newBinaryDocument.Location, newBinaryDocument);
#endif
        this.disposableObjectAllocatedByThisHost.Add((IDisposable)binDocMemoryBlock);
        return binDocMemoryBlock;
      } catch (IOException) {
        return null;
      }
    }

    /// <summary>
    /// Provides the host with an opportunity to add, remove or substitute assembly references in the given list.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that contains these references.</param>
    /// <param name="assemblyReferences">The assembly references to substitute.</param>
    /// <returns>Usually assemblyReferences, but occasionally a modified enumeration.</returns>
    public virtual IEnumerable<IAssemblyReference> Redirect(IUnit referringUnit, IEnumerable<IAssemblyReference> assemblyReferences) {
      return assemblyReferences;
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute one named type reference for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that contains the reference.</param>
    /// <param name="typeReference">A named type reference encountered during metadata reading.</param>
    /// <returns>
    /// Usually the value in typeReference, but occassionally something else.
    /// </returns>
    public virtual INamedTypeReference Redirect(IUnit referringUnit, INamedTypeReference typeReference) {
      return typeReference;
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute a custom attribute with another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that contains the custom attribute.</param>
    /// <param name="customAttribute">The custom attribute to rewrite (fix up).</param>
    /// <returns>
    /// Usually the value in customAttribute, but occassionally another custom attribute.
    /// </returns>
    public virtual ICustomAttribute Rewrite(IUnit containingUnit, ICustomAttribute customAttribute) {
      return customAttribute;
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute one method definition for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that is defines the method.</param>
    /// <param name="methodDefinition">A method definition encountered during metadata reading.</param>
    /// <returns>
    /// Usually the value in methodDefinition, but occassionally something else.
    /// </returns>
    public virtual IMethodDefinition Rewrite(IUnit containingUnit, IMethodDefinition methodDefinition) {
      return methodDefinition;
    }

    /// <summary>
    /// This method is called when the assembly reference is being resolved and its not already loaded by the Read/Write host.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the assembly.</param>
    /// <param name="referencedAssembly">Assembly identity for the assembly being referenced.</param>
    public virtual void ResolvingAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly) {
      if (!string.IsNullOrEmpty(referencedAssembly.Location)) {
        this.LoadUnit(referencedAssembly);
      } else {
        AssemblyIdentity ai = this.ProbeAssemblyReference(referringUnit, referencedAssembly);
        if (ai != null && !String.IsNullOrEmpty(ai.Location)) {
          this.LoadUnit(ai);
        }
      }
    }

    /// <summary>
    /// This method is called when the module reference is being resolved and its not already loaded by the Read/Write host.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the module.</param>
    /// <param name="referencedModule">Module identity for the assembly being referenced.</param>
    public virtual void ResolvingModuleReference(IUnit referringUnit, ModuleIdentity referencedModule) {
    }

    /// <summary>
    /// Called by the metadata reader when it is about to start parsing a custom attribute blob.
    /// </summary>
    public void StartGuessingGame() {
      this.currentWildGuesses = null;
      this.currentGoodGuesses = null;
    }

    /// <summary>
    /// Called by the metadata reader when it has unsucessfully tried to parse a custom attribute blob and it now needs to try a new permutation.
    /// Returns false if no more perumations are possible.
    /// </summary>
    public bool TryNextPermutation() {
      bool allPermutationsHaveBeenTried = true;
      if (this.currentWildGuesses != null) {
        var keys = new List<uint>(this.currentWildGuesses.Keys);
        foreach (var key in keys) {
          var oldValue = this.currentWildGuesses[key];
          if (oldValue == 4)
            this.currentWildGuesses[key] = 1;
          else if (oldValue == 1)
            this.currentWildGuesses[key] = 2;
          else if (oldValue == 2)
            this.currentWildGuesses[key] = 8;
          else {
            this.currentWildGuesses[key] = 4;
            continue;
          }
          allPermutationsHaveBeenTried = false;
          break;
        }
      }
      if (allPermutationsHaveBeenTried && this.currentGoodGuesses != null) {
        if (this.currentWildGuesses == null) this.currentWildGuesses = new Dictionary<uint, byte>();
        foreach (var key in this.currentGoodGuesses.Keys) {
          allPermutationsHaveBeenTried = false;
          this.currentWildGuesses[key] = 4;
        }
        this.currentGoodGuesses = null;
      }
      return !allPermutationsHaveBeenTried;
    }

    /// <summary>
    /// Called by the metadata reader when it has successfully parsed a custom attribute blob.
    /// </summary>
    public void WinGuessingGame() {
      if (this.currentWildGuesses == null) return;
      if (this.successfulGuesses == null) this.successfulGuesses = new Dictionary<uint, byte>();
      foreach (var pair in this.currentWildGuesses)
        this.successfulGuesses[pair.Key] = pair.Value;
      this.currentWildGuesses = null;
      if (this.currentGoodGuesses == null) return;
      foreach (var pair in this.currentGoodGuesses)
        this.successfulGuesses[pair.Key] = pair.Value;
      this.currentGoodGuesses = null;
    }

    /// <summary>
    /// Returns a guess of the size of the underlying type of the given type reference to an enum type, which is assumed to be unresolvable
    /// because it is defined an assembly that is not loaded into this host. Successive calls to the method will cycle through these values
    /// with a periodicity determined by the number of types in the game and the successful guesses made in earlier games.
    /// </summary>
    /// <param name="reference">A type reference that cannot be resolved.</param>
    /// <returns>1, 2, 4 or 8.</returns>
    public byte GuessUnderlyingTypeSizeOfUnresolvableReferenceToEnum(ITypeReference reference) {
      uint rkey = reference.InternedKey;
      byte guess;
      if (this.currentGoodGuesses != null && this.currentGoodGuesses.TryGetValue(rkey, out guess))
        return guess;
      if (this.currentWildGuesses != null && this.currentWildGuesses.TryGetValue(rkey, out guess))
        return guess;
      if (this.successfulGuesses != null && this.successfulGuesses.TryGetValue(rkey, out guess)) {
        if (this.currentGoodGuesses == null) this.currentGoodGuesses = new Dictionary<uint, byte>();
        this.currentGoodGuesses[rkey] = guess;
        return guess;
      }
      if (this.currentWildGuesses == null) this.currentWildGuesses = new Dictionary<uint, byte>();
      this.currentWildGuesses[rkey] = 4;
      return 4;
    }

    Dictionary<uint, byte> successfulGuesses;
    Dictionary<uint, byte> currentWildGuesses;
    Dictionary<uint, byte> currentGoodGuesses;

  }

  /// <summary>
  /// A collection of methods that associate unique integers with metadata model entities.
  /// The association is based on the identities of the entities and the factory does not retain
  /// references to the given metadata model objects.
  /// </summary>
  [ContractVerification(true)]
  public sealed class InternFactory : IInternFactory {

    sealed class AssemblyStore {
      internal readonly AssemblyIdentity AssemblyIdentity;
      internal uint InternedIdWithCount;
      internal readonly uint RootNamespaceInternedId;
      internal AssemblyStore(
        AssemblyIdentity assemblyIdentity,
        uint internedId,
        uint rootNamespaceInternedId
      ) {
        this.AssemblyIdentity = assemblyIdentity;
        this.InternedIdWithCount = internedId;
        this.RootNamespaceInternedId = rootNamespaceInternedId;
      }
      internal uint InternedId {
        get {
          return this.InternedIdWithCount & 0xFFFFF000;
        }
      }
    }

    sealed class ModuleStore {
      internal readonly ModuleIdentity ModuleIdentitity;
      internal readonly uint InternedId;
      internal readonly uint RootNamespaceInternedId;
      internal ModuleStore(
        ModuleIdentity moduleIdentitity,
        uint internedId,
        uint rootNamespaceInternedId
      ) {
        this.ModuleIdentitity = moduleIdentitity;
        this.InternedId = internedId;
        this.RootNamespaceInternedId = rootNamespaceInternedId;
      }
    }

    sealed class NamespaceTypeStore {
      internal readonly uint ContainingNamespaceInternedId;
      internal readonly uint GenericParameterCount;
      internal readonly uint InternedId;
      internal NamespaceTypeStore(
        uint containingNamespaceInternedId,
        uint genericParameterCount,
        uint internedId
      ) {
        this.ContainingNamespaceInternedId = containingNamespaceInternedId;
        this.GenericParameterCount = genericParameterCount;
        this.InternedId = internedId;
      }
    }

    sealed class NestedTypeStore {
      internal readonly uint ContainingTypeInternedId;
      internal readonly uint GenericParameterCount;
      internal readonly uint InternedId;
      internal NestedTypeStore(
        uint containingTypeInternedId,
        uint genericParameterCount,
        uint internedId
      ) {
        this.ContainingTypeInternedId = containingTypeInternedId;
        this.GenericParameterCount = genericParameterCount;
        this.InternedId = internedId;
      }
    }

    sealed class MatrixTypeStore {
      internal readonly int Rank;
      internal readonly int[] LowerBounds;
      internal readonly ulong[] Sizes;
      internal readonly uint InternedId;
      internal MatrixTypeStore(
        int rank,
        int[] lowerBounds,
        ulong[] sizes,
        uint internedId
      ) {
        this.Rank = rank;
        this.LowerBounds = lowerBounds;
        this.Sizes = sizes;
        this.InternedId = internedId;
      }
    }

    sealed class ParameterTypeStore {
      internal readonly bool IsByReference;
      internal readonly uint CustomModifiersInternId;
      internal readonly uint InternedId;
      internal ParameterTypeStore(
        bool isByReference,
        uint customModifiersInternId,
        uint internedId
      ) {
        this.IsByReference = isByReference;
        this.CustomModifiersInternId = customModifiersInternId;
        this.InternedId = internedId;
      }
    }

    sealed class SignatureStore {
      internal readonly CallingConvention CallingConvention;
      internal readonly uint RequiredParameterListInternedId;
      internal readonly uint ExtraParameterListInternedId;
      internal readonly bool ReturnValueIsByRef;
      internal readonly uint ReturnValueCustomModifiersListInteredId;
      internal readonly uint ReturnTypeReferenceInternedId;
      internal readonly uint GenericParameterCount;
      internal readonly uint InternedId;
      internal SignatureStore(
       CallingConvention callingConvention,
       uint requiredParameterListInteredId,
       uint extraParameterListInteredId,
       bool returnValueIsByRef,
       uint returnValueCustomModifiersListInteredId,
       uint returnTypeReferenceInteredId,
       uint genericParameterCount,
       uint internedId
      ) {
        this.CallingConvention = callingConvention;
        this.RequiredParameterListInternedId = requiredParameterListInteredId;
        this.ExtraParameterListInternedId = extraParameterListInteredId;
        this.ReturnValueIsByRef = returnValueIsByRef;
        this.ReturnValueCustomModifiersListInteredId = returnValueCustomModifiersListInteredId;
        this.ReturnTypeReferenceInternedId = returnTypeReferenceInteredId;
        this.GenericParameterCount = genericParameterCount;
        this.InternedId = internedId;
      }
    }

    uint CurrentAssemblyInternValue;
    uint CurrentModuleInternValue;
    uint CurrentNamespaceInternValue;
    uint CurrentTypeInternValue;
    uint CurrentTypeListInternValue;
    uint CurrentCustomModifierInternValue;
    uint CurrentCustomModifierListInternValue;
    uint CurrentParameterTypeInternValue;
    uint CurrentParameterTypeListInternValue;
    uint CurrentSignatureInternValue;
    uint CurrentMethodReferenceInternValue;
    uint CurrentFieldReferenceInternValue;
    IMethodReference CurrentMethodReference; //The method reference currently being interned
    readonly MultiHashtable<AssemblyStore> AssemblyHashtable;
    readonly MultiHashtable<ModuleStore> ModuleHashtable;
    readonly DoubleHashtable NestedNamespaceHashtable;
    readonly MultiHashtable<NamespaceTypeStore> NamespaceTypeHashtable;
    readonly MultiHashtable<NestedTypeStore> NestedTypeHashtable;
    readonly Hashtable VectorTypeHashTable;
    readonly Hashtable PointerTypeHashTable;
    readonly Hashtable ManagedPointerTypeHashTable;
    readonly MultiHashtable<MatrixTypeStore> MatrixTypeHashtable;
    readonly DoubleHashtable TypeListHashtable;
    readonly DoubleHashtable GenericTypeInstanceHashtable;
    readonly DoubleHashtable GenericMethodInstanceHashtable;
    readonly DoubleHashtable GenericTypeParameterHashtable;
    readonly DoubleHashtable GenericMethodTypeParameterHashTable;
    readonly DoubleHashtable CustomModifierHashTable;
    readonly DoubleHashtable CustomModifierListHashTable;
    readonly MultiHashtable<ParameterTypeStore> ParameterTypeHashtable;
    readonly DoubleHashtable ParameterTypeListHashtable;
    readonly MultiHashtable<SignatureStore> SignatureHashtable;
    readonly Hashtable FunctionTypeHashTable;
    readonly DoubleHashtable ModifiedTypeHashtable;
    readonly Hashtable<MultiHashtable<SignatureStore>> MethodReferenceHashtable;
    readonly Hashtable<DoubleHashtable> FieldReferenceHashtable;

    /// <summary>
    /// 
    /// </summary>
    public InternFactory() {
      this.CurrentAssemblyInternValue = 0x00001000;
      this.CurrentMethodReference = Dummy.MethodReference;
      this.CurrentModuleInternValue = 0x00000001;
      this.CurrentNamespaceInternValue = 0x00000001;
      this.CurrentTypeInternValue = 0x00000100;
      this.CurrentTypeListInternValue = 0x00000001;
      this.CurrentCustomModifierInternValue = 0x00000001;
      this.CurrentCustomModifierListInternValue = 0x00000001;
      this.CurrentMethodReferenceInternValue = 0x00000001;
      this.CurrentFieldReferenceInternValue = 0x00000001;
      this.CurrentParameterTypeInternValue = 0x00000001;
      this.CurrentParameterTypeListInternValue = 0x00000001;
      this.CurrentSignatureInternValue = 0x00000001;
      this.AssemblyHashtable = new MultiHashtable<AssemblyStore>();
      this.ModuleHashtable = new MultiHashtable<ModuleStore>();
      this.NestedNamespaceHashtable = new DoubleHashtable();
      this.NamespaceTypeHashtable = new MultiHashtable<NamespaceTypeStore>();
      this.NestedTypeHashtable = new MultiHashtable<NestedTypeStore>();
      this.VectorTypeHashTable = new Hashtable();
      this.PointerTypeHashTable = new Hashtable();
      this.ManagedPointerTypeHashTable = new Hashtable();
      this.MatrixTypeHashtable = new MultiHashtable<MatrixTypeStore>();
      this.TypeListHashtable = new DoubleHashtable();
      this.GenericTypeInstanceHashtable = new DoubleHashtable();
      this.GenericMethodInstanceHashtable = new DoubleHashtable();
      this.GenericTypeParameterHashtable = new DoubleHashtable();
      this.GenericMethodTypeParameterHashTable = new DoubleHashtable();
      this.CustomModifierHashTable = new DoubleHashtable();
      this.CustomModifierListHashTable = new DoubleHashtable();
      this.ParameterTypeHashtable = new MultiHashtable<ParameterTypeStore>();
      this.ParameterTypeListHashtable = new DoubleHashtable();
      this.SignatureHashtable = new MultiHashtable<SignatureStore>();
      this.FunctionTypeHashTable = new Hashtable();
      this.ModifiedTypeHashtable = new DoubleHashtable();
      this.MethodReferenceHashtable = new Hashtable<MultiHashtable<SignatureStore>>();
      this.FieldReferenceHashtable = new Hashtable<DoubleHashtable>();
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.AssemblyHashtable != null);
      Contract.Invariant(this.ModuleHashtable != null);
      Contract.Invariant(this.NestedNamespaceHashtable != null);
      Contract.Invariant(this.NamespaceTypeHashtable != null);
      Contract.Invariant(this.NestedTypeHashtable != null);
      Contract.Invariant(this.VectorTypeHashTable != null);
      Contract.Invariant(this.PointerTypeHashTable != null);
      Contract.Invariant(this.ManagedPointerTypeHashTable != null);
      Contract.Invariant(this.MatrixTypeHashtable != null);
      Contract.Invariant(this.TypeListHashtable != null);
      Contract.Invariant(this.GenericTypeInstanceHashtable != null);
      Contract.Invariant(this.GenericMethodInstanceHashtable != null);
      Contract.Invariant(this.GenericTypeParameterHashtable != null);
      Contract.Invariant(this.GenericMethodTypeParameterHashTable != null);
      Contract.Invariant(this.CustomModifierHashTable != null);
      Contract.Invariant(this.CustomModifierListHashTable != null);
      Contract.Invariant(this.ParameterTypeHashtable != null);
      Contract.Invariant(this.ParameterTypeListHashtable != null);
      Contract.Invariant(this.SignatureHashtable != null);
      Contract.Invariant(this.FunctionTypeHashTable != null);
      Contract.Invariant(this.ModifiedTypeHashtable != null);
      Contract.Invariant(this.MethodReferenceHashtable != null);
      Contract.Invariant(this.FieldReferenceHashtable != null);
    }

    AssemblyStore GetAssemblyStore(AssemblyIdentity assemblyIdentity) {
      Contract.Requires(assemblyIdentity != null);
      Contract.Ensures(Contract.Result<AssemblyStore>() != null);

      IName assemblyName = assemblyIdentity.Name;
      foreach (AssemblyStore aStore in this.AssemblyHashtable.GetValuesFor((uint)assemblyName.UniqueKey)) {
        if (assemblyIdentity.Equals(aStore.AssemblyIdentity)) return aStore;
      }
      uint value = this.CurrentAssemblyInternValue;
      this.CurrentAssemblyInternValue += 0x00001000;
      AssemblyStore aStore1 = new AssemblyStore(assemblyIdentity, value, this.CurrentNamespaceInternValue++);
      this.AssemblyHashtable.Add((uint)assemblyName.UniqueKey, aStore1);
      return aStore1;
    }

    ModuleStore GetModuleStore(ModuleIdentity moduleIdentity) {
      Contract.Requires(moduleIdentity != null);
      Contract.Ensures(Contract.Result<ModuleStore>() != null);

      IName moduleName = moduleIdentity.Name;
      foreach (ModuleStore mStore in this.ModuleHashtable.GetValuesFor((uint)moduleName.UniqueKey)) {
        if (moduleIdentity.Equals(mStore.ModuleIdentitity)) return mStore;
      }
      uint value;
      if (moduleIdentity.ContainingAssembly != null) {
        AssemblyStore assemblyStore = this.GetAssemblyStore(moduleIdentity.ContainingAssembly);
        assemblyStore.InternedIdWithCount++;
        value = assemblyStore.InternedIdWithCount;
      } else {
        value = this.CurrentModuleInternValue++;
      }
      ModuleStore mStore1 = new ModuleStore(moduleIdentity, value, this.CurrentNamespaceInternValue++);
      this.ModuleHashtable.Add((uint)moduleName.UniqueKey, mStore1);
      return mStore1;
    }

    uint GetUnitRootNamespaceInternId(IUnitReference unitReference) {
      Contract.Requires(unitReference != null);

      IAssemblyReference/*?*/ assemblyReference = unitReference as IAssemblyReference;
      if (assemblyReference != null) {
        AssemblyStore assemblyStore = this.GetAssemblyStore(assemblyReference.UnifiedAssemblyIdentity);
        return assemblyStore.RootNamespaceInternedId;
      }
      IModuleReference/*?*/ moduleReference = unitReference as IModuleReference;
      if (moduleReference != null) {
        ModuleStore moduleStore = this.GetModuleStore(moduleReference.ModuleIdentity);
        return moduleStore.RootNamespaceInternedId;
      }
      return 0;
    }

    uint GetNestedNamespaceInternId(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      Contract.Requires(nestedUnitNamespaceReference != null);

      uint parentNamespaceInternedId = this.GetUnitNamespaceInternId(nestedUnitNamespaceReference.ContainingUnitNamespace);
      uint value = this.NestedNamespaceHashtable.Find(parentNamespaceInternedId, (uint)nestedUnitNamespaceReference.Name.UniqueKey);
      if (value == 0) {
        value = this.CurrentNamespaceInternValue++;
        this.NestedNamespaceHashtable.Add(parentNamespaceInternedId, (uint)nestedUnitNamespaceReference.Name.UniqueKey, value);
      }
      return value;
    }

    uint GetUnitNamespaceInternId(IUnitNamespaceReference unitNamespaceReference) {
      Contract.Requires(unitNamespaceReference != null);

      INestedUnitNamespaceReference/*?*/ nestedUnitNamespaceReference = unitNamespaceReference as INestedUnitNamespaceReference;
      if (nestedUnitNamespaceReference != null) {
        return this.GetNestedNamespaceInternId(nestedUnitNamespaceReference);
      }
      return this.GetUnitRootNamespaceInternId(unitNamespaceReference.Unit);
    }

    uint GetNamespaceTypeReferenceInternId(IUnitNamespaceReference containingUnitNamespace, IName typeName, uint genericParameterCount) {
      Contract.Requires(containingUnitNamespace != null);
      Contract.Requires(typeName != null);

      uint containingUnitNamespaceInteredId = this.GetUnitNamespaceInternId(containingUnitNamespace);
      foreach (NamespaceTypeStore nsTypeStore in this.NamespaceTypeHashtable.GetValuesFor((uint)typeName.UniqueKey)) {
        if (nsTypeStore.ContainingNamespaceInternedId == containingUnitNamespaceInteredId && nsTypeStore.GenericParameterCount == genericParameterCount) {
          return nsTypeStore.InternedId;
        }
      }
      NamespaceTypeStore nsTypeStore1 = new NamespaceTypeStore(containingUnitNamespaceInteredId, genericParameterCount, this.CurrentTypeInternValue++);
      this.NamespaceTypeHashtable.Add((uint)typeName.UniqueKey, nsTypeStore1);
      return nsTypeStore1.InternedId;
    }

    uint GetNestedTypeReferenceInternId(ITypeReference containingTypeReference, IName typeName, uint genericParameterCount) {
      Contract.Requires(containingTypeReference != null);
      Contract.Requires(typeName != null);

      uint containingTypeReferenceInteredId = this.GetTypeReferenceInternId(containingTypeReference);
      foreach (NestedTypeStore nstTypeStore in this.NestedTypeHashtable.GetValuesFor((uint)typeName.UniqueKey)) {
        if (
          nstTypeStore.ContainingTypeInternedId == containingTypeReferenceInteredId
          && nstTypeStore.GenericParameterCount == genericParameterCount
        ) {
          return nstTypeStore.InternedId;
        }
      }
      NestedTypeStore nstTypeStore1 = new NestedTypeStore(containingTypeReferenceInteredId, genericParameterCount, this.CurrentTypeInternValue++);
      this.NestedTypeHashtable.Add((uint)typeName.UniqueKey, nstTypeStore1);
      return nstTypeStore1.InternedId;
    }

    uint GetVectorTypeReferenceInternId(ITypeReference elementTypeReference) {
      Contract.Requires(elementTypeReference != null);

      uint elementTypeReferenceInternId = this.GetTypeReferenceInternId(elementTypeReference);
      uint value = this.VectorTypeHashTable.Find(elementTypeReferenceInternId);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.VectorTypeHashTable.Add(elementTypeReferenceInternId, value);
      }
      return value;
    }

    uint GetMatrixTypeReferenceInternId(ITypeReference elementTypeReference, int rank, IEnumerable<ulong> sizes, IEnumerable<int> lowerBounds) {
      Contract.Requires(elementTypeReference != null);
      Contract.Requires(sizes != null);
      Contract.Requires(lowerBounds != null);

      uint elementTypeReferenceInternId = this.GetTypeReferenceInternId(elementTypeReference);
      foreach (MatrixTypeStore matrixTypeStore in this.MatrixTypeHashtable.GetValuesFor(elementTypeReferenceInternId)) {
        if (matrixTypeStore.Rank == rank && IteratorHelper.EnumerablesAreEqual<ulong>(matrixTypeStore.Sizes, sizes) && IteratorHelper.EnumerablesAreEqual<int>(matrixTypeStore.LowerBounds, lowerBounds))
          return matrixTypeStore.InternedId;
      }
      MatrixTypeStore matrixTypeStore1 = new MatrixTypeStore(rank, new List<int>(lowerBounds).ToArray(), new List<ulong>(sizes).ToArray(), this.CurrentTypeInternValue++);
      this.MatrixTypeHashtable.Add(elementTypeReferenceInternId, matrixTypeStore1);
      return matrixTypeStore1.InternedId;
    }

    uint GetTypeReferenceListInternedId(IEnumerator<ITypeReference> typeReferences) {
      Contract.Requires(typeReferences != null);

      if (!typeReferences.MoveNext()) return 0;
      ITypeReference currentTypeRef = typeReferences.Current;
      Contract.Assume(currentTypeRef != null);
      uint currentTypeRefInternedId = this.GetTypeReferenceInternId(currentTypeRef);
      uint tailInternedId = this.GetTypeReferenceListInternedId(typeReferences);
      uint value = this.TypeListHashtable.Find(currentTypeRefInternedId, tailInternedId);
      if (value == 0) {
        value = this.CurrentTypeListInternValue++;
        this.TypeListHashtable.Add(currentTypeRefInternedId, tailInternedId, value);
      }
      return value;
    }

    uint GetGenericTypeInstanceReferenceInternId(ITypeReference genericTypeReference, IEnumerable<ITypeReference> genericArguments) {
      Contract.Requires(genericTypeReference != null);
      Contract.Requires(genericArguments != null);

      uint genericTypeInternedId = this.GetTypeReferenceInternId(genericTypeReference);
      uint genericArgumentsInternedId = this.GetTypeReferenceListInternedId(genericArguments.GetEnumerator());
      uint value = this.GenericTypeInstanceHashtable.Find(genericTypeInternedId, genericArgumentsInternedId);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.GenericTypeInstanceHashtable.Add(genericTypeInternedId, genericArgumentsInternedId, value);
      }
      return value;
    }

    uint GetPointerTypeReferenceInternId(ITypeReference targetTypeReference) {
      Contract.Requires(targetTypeReference != null);

      uint targetTypeReferenceInternId = this.GetTypeReferenceInternId(targetTypeReference);
      uint value = this.PointerTypeHashTable.Find(targetTypeReferenceInternId);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.PointerTypeHashTable.Add(targetTypeReferenceInternId, value);
      }
      return value;
    }

    uint GetManagedPointerTypeReferenceInternId(ITypeReference targetTypeReference) {
      Contract.Requires(targetTypeReference != null);

      uint targetTypeReferenceInternId = this.GetTypeReferenceInternId(targetTypeReference);
      uint value = this.ManagedPointerTypeHashTable.Find(targetTypeReferenceInternId);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.ManagedPointerTypeHashTable.Add(targetTypeReferenceInternId, value);
      }
      return value;
    }
    
    uint GetGenericTypeParameterReferenceInternId(ITypeReference definingTypeReference, int index) {
      Contract.Requires(definingTypeReference != null);

      uint definingTypeReferenceInternId = this.GetTypeReferenceInternId(GetUninstantiatedGenericType(definingTypeReference));
      uint value = this.GenericTypeParameterHashtable.Find(definingTypeReferenceInternId, (uint)index);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.GenericTypeParameterHashtable.Add(definingTypeReferenceInternId, (uint)index, value);
      }
      return value;
    }

    private static ITypeReference GetUninstantiatedGenericType(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null) return genericTypeInstanceReference.GenericType;
      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      if (nestedTypeReference != null) {
        ISpecializedNestedTypeReference/*?*/ specializedNestedType = nestedTypeReference as ISpecializedNestedTypeReference;
        if (specializedNestedType != null) return specializedNestedType.UnspecializedVersion;
        return nestedTypeReference;
      }
      return typeReference;
    }

    /// <summary>
    /// Returns the interned key for the generic method parameter constructed with the given index
    /// </summary>
    /// <param name="definingMethodReference">A reference to the method defining the referenced generic parameter.</param>
    /// <param name="index">The index of the referenced generic parameter. This is an index rather than a name because metadata in CLR
    /// PE files contain only the index, not the name.</param>
    uint GetGenericMethodParameterReferenceInternId(IMethodReference definingMethodReference, uint index) {
      Contract.Requires(definingMethodReference != null);

      if (!(this.CurrentMethodReference is Dummy)) {
        //this happens when the defining method reference contains a type in its signature which either is, or contains,
        //a reference to this generic method type parameter. In that case we break the cycle by just using the index of 
        //the generic parameter. Only method references that refer to their own type parameters will ever
        //get this version of the interned id.
        return index+1000000; //provide a big offset to minimize the chances of a structural type in the 
        //signature of the method interning onto some other type that is parameterized by a type whose intern key is index.
      }
      this.CurrentMethodReference = definingMethodReference; //short circuit recursive calls back to this method
      uint definingMethodReferenceInternId = this.GetMethodReferenceInternedId(definingMethodReference);
      this.CurrentMethodReference = Dummy.MethodReference;
      uint value = this.GenericMethodTypeParameterHashTable.Find(definingMethodReferenceInternId, index);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.GenericMethodTypeParameterHashTable.Add(definingMethodReferenceInternId, index, value);
      }
      return value;
    }

    uint GetParameterTypeInternId(IParameterTypeInformation parameterTypeInformation) {
      Contract.Requires(parameterTypeInformation != null);

      uint typeReferenceInternId = this.GetTypeReferenceInternId(parameterTypeInformation.Type);
      uint customModifiersInternId = 0;
      if (parameterTypeInformation.IsModified)
        customModifiersInternId = this.GetCustomModifierListInternId(parameterTypeInformation.CustomModifiers.GetEnumerator());
      foreach (ParameterTypeStore parameterTypeStore in this.ParameterTypeHashtable.GetValuesFor(typeReferenceInternId)) {
        if (
          parameterTypeStore.IsByReference == parameterTypeInformation.IsByReference
          && parameterTypeStore.CustomModifiersInternId == customModifiersInternId
        ) {
          return parameterTypeStore.InternedId;
        }
      }
      ParameterTypeStore parameterTypeStore1 = new ParameterTypeStore(parameterTypeInformation.IsByReference, customModifiersInternId, this.CurrentParameterTypeInternValue++);
      this.ParameterTypeHashtable.Add(typeReferenceInternId, parameterTypeStore1);
      return parameterTypeStore1.InternedId;
    }

    uint GetParameterTypeListInternId(IEnumerator<IParameterTypeInformation> parameterTypeInformations) {
      Contract.Requires(parameterTypeInformations != null);

      if (!parameterTypeInformations.MoveNext()) return 0;
      Contract.Assume(parameterTypeInformations.Current != null);
      uint currentParameterInternedId = this.GetParameterTypeInternId(parameterTypeInformations.Current);
      uint tailInternedId = this.GetParameterTypeListInternId(parameterTypeInformations);
      uint value = this.ParameterTypeListHashtable.Find(currentParameterInternedId, tailInternedId);
      if (value == 0) {
        value = this.CurrentParameterTypeListInternValue++;
        this.ParameterTypeListHashtable.Add(currentParameterInternedId, tailInternedId, value);
      }
      return value;
    }

    uint GetSignatureInternId(CallingConvention callingConvention, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation> extraArgumentTypes,
      IEnumerable<ICustomModifier> returnValueCustomModifiers, bool returnValueIsByRef, ITypeReference returnType) {
      Contract.Requires(parameters != null);
      Contract.Requires(extraArgumentTypes != null);
      Contract.Requires(returnValueCustomModifiers != null);
      Contract.Requires(returnType != null);

      uint requiredParameterTypesInternedId = this.GetParameterTypeListInternId(parameters.GetEnumerator());
      uint extraArgumentTypesInteredId = this.GetParameterTypeListInternId(extraArgumentTypes.GetEnumerator());
      uint returnValueCustomModifiersInternedId = this.GetCustomModifierListInternId(returnValueCustomModifiers.GetEnumerator());
      uint returnTypeReferenceInternedId = this.GetTypeReferenceInternId(returnType);
      foreach (SignatureStore signatureStore in this.SignatureHashtable.GetValuesFor(requiredParameterTypesInternedId)) {
        if (
          signatureStore.CallingConvention == callingConvention
          && signatureStore.RequiredParameterListInternedId == requiredParameterTypesInternedId
          && signatureStore.ExtraParameterListInternedId == extraArgumentTypesInteredId
          && signatureStore.ReturnValueCustomModifiersListInteredId == returnValueCustomModifiersInternedId
          && signatureStore.ReturnValueIsByRef == returnValueIsByRef
          && signatureStore.ReturnTypeReferenceInternedId == returnTypeReferenceInternedId
        ) {
          return signatureStore.InternedId;
        }
      }
      SignatureStore signatureStore1 = new SignatureStore(callingConvention, requiredParameterTypesInternedId, extraArgumentTypesInteredId, returnValueIsByRef, returnValueCustomModifiersInternedId, returnTypeReferenceInternedId, 0, this.CurrentSignatureInternValue++);
      this.SignatureHashtable.Add(requiredParameterTypesInternedId, signatureStore1);
      return signatureStore1.InternedId;
    }

    uint GetMethodReferenceInternedId(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);

      var genInstanceRef = methodReference as IGenericMethodInstanceReference;
      if (genInstanceRef != null) return this.GetGenericMethodInstanceReferenceInternedKey(genInstanceRef);
      uint containingTypeReferenceInternedId = this.GetTypeReferenceInternId(methodReference.ContainingType);
      uint requiredParameterTypesInternedId = this.GetParameterTypeListInternId(methodReference.Parameters.GetEnumerator());
      uint returnValueCustomModifiersInternedId = 0;
      uint genericParameterCount = methodReference.GenericParameterCount;
      if (methodReference.ReturnValueIsModified)
        returnValueCustomModifiersInternedId = this.GetCustomModifierListInternId(methodReference.ReturnValueCustomModifiers.GetEnumerator());
      uint returnTypeReferenceInternedId = this.GetTypeReferenceInternId(methodReference.Type);
      MultiHashtable<SignatureStore>/*?*/ methods = this.MethodReferenceHashtable.Find(containingTypeReferenceInternedId);
      if (methods == null) {
        methods = new MultiHashtable<SignatureStore>();
        this.MethodReferenceHashtable.Add(containingTypeReferenceInternedId, methods);
      }
      foreach (SignatureStore signatureStore in methods.GetValuesFor((uint)methodReference.Name.UniqueKey)) {
        if (
          signatureStore.CallingConvention == methodReference.CallingConvention
          && signatureStore.RequiredParameterListInternedId == requiredParameterTypesInternedId
          && signatureStore.ReturnValueCustomModifiersListInteredId == returnValueCustomModifiersInternedId
          && signatureStore.ReturnValueIsByRef == methodReference.ReturnValueIsByRef
          && signatureStore.ReturnTypeReferenceInternedId == returnTypeReferenceInternedId
          && signatureStore.GenericParameterCount == genericParameterCount
        ) {
          return signatureStore.InternedId;
        }
      }
      SignatureStore signatureStore1 = new SignatureStore(methodReference.CallingConvention, requiredParameterTypesInternedId,
        0, methodReference.ReturnValueIsByRef, returnValueCustomModifiersInternedId, returnTypeReferenceInternedId, genericParameterCount,
        this.CurrentMethodReferenceInternValue++);
      methods.Add((uint)methodReference.Name.UniqueKey, signatureStore1);
      return signatureStore1.InternedId;
    }

    uint GetGenericMethodInstanceReferenceInternedKey(IGenericMethodInstanceReference genericMethodInstanceReference) {
      Contract.Requires(genericMethodInstanceReference != null);

      var genericMethodInternedId = genericMethodInstanceReference.GenericMethod.InternedKey;
      uint genericArgumentsInternedId = this.GetTypeReferenceListInternedId(genericMethodInstanceReference.GenericArguments.GetEnumerator());
      uint value = this.GenericMethodInstanceHashtable.Find(genericMethodInternedId, genericArgumentsInternedId);
      if (value == 0) {
        value = this.CurrentMethodReferenceInternValue++;
        this.GenericMethodInstanceHashtable.Add(genericMethodInternedId, genericArgumentsInternedId, value);
      }
      return value;
    }

    uint GetFieldReferenceInternedId(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);

      uint containingTypeReferenceInternedId = this.GetTypeReferenceInternId(fieldReference.ContainingType);
      uint fieldTypeInternedId;
      if (fieldReference.IsModified)
        fieldTypeInternedId = this.GetModifiedTypeReferenceInternId(fieldReference.Type, fieldReference.CustomModifiers);
      else
        fieldTypeInternedId = this.GetTypeReferenceInternId(fieldReference.Type);
      uint fieldNameId = (uint)fieldReference.Name.UniqueKey;
      var fieldsForType = this.FieldReferenceHashtable.Find(containingTypeReferenceInternedId);
      if (fieldsForType == null) {
        fieldsForType = new DoubleHashtable();
        this.FieldReferenceHashtable.Add(containingTypeReferenceInternedId, fieldsForType);
      }
      uint result = fieldsForType.Find(fieldNameId, fieldTypeInternedId);
      if (result == 0) {
        result = this.CurrentFieldReferenceInternValue++;
        fieldsForType.Add(fieldNameId, fieldTypeInternedId, result);
      }
      return result;
    }

    uint GetFunctionPointerTypeReferenceInternId(CallingConvention callingConvention, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation> extraArgumentTypes,
      IEnumerable<ICustomModifier> returnValueCustomModifiers, bool returnValueIsByRef, ITypeReference returnType) {
      Contract.Requires(parameters != null);
      Contract.Requires(extraArgumentTypes != null);
      Contract.Requires(returnValueCustomModifiers != null);
      Contract.Requires(returnType != null);

      uint signatureInternedId = this.GetSignatureInternId(callingConvention, parameters, extraArgumentTypes, returnValueCustomModifiers, returnValueIsByRef, returnType);
      uint value = this.FunctionTypeHashTable.Find(signatureInternedId);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.FunctionTypeHashTable.Add(signatureInternedId, value);
      }
      return value;
    }

    uint GetCustomModifierInternId(ICustomModifier customModifier) {
      Contract.Requires(customModifier != null);

      uint currentTypeRefInternedId = this.GetTypeReferenceInternId(customModifier.Modifier);
      uint isOptionalIntneredId = customModifier.IsOptional ? 0xF0F0F0F0 : 0x0F0F0F0F;  //  Just for the heck of it...
      uint value = this.CustomModifierHashTable.Find(currentTypeRefInternedId, isOptionalIntneredId);
      if (value == 0) {
        value = this.CurrentCustomModifierInternValue++;
        this.CustomModifierHashTable.Add(currentTypeRefInternedId, isOptionalIntneredId, value);
      }
      return value;
    }

    uint GetCustomModifierListInternId(IEnumerator<ICustomModifier> customModifiers) {
      Contract.Requires(customModifiers != null);

      if (!customModifiers.MoveNext()) return 0;
      var current = customModifiers.Current;
      Contract.Assume(current != null);
      uint currentCustomModifierInternedId = this.GetCustomModifierInternId(current);
      uint tailInternedId = this.GetCustomModifierListInternId(customModifiers);
      uint value = this.CustomModifierListHashTable.Find(currentCustomModifierInternedId, tailInternedId);
      if (value == 0) {
        value = this.CurrentCustomModifierListInternValue++;
        this.CustomModifierListHashTable.Add(currentCustomModifierInternedId, tailInternedId, value);
      }
      return value;
    }

    uint GetTypeReferenceInterendIdIgnoringCustomModifiers(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);

      INamespaceTypeReference/*?*/ namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference != null) {
        return this.GetNamespaceTypeReferenceInternId(
          namespaceTypeReference.ContainingUnitNamespace,
          namespaceTypeReference.Name,
          namespaceTypeReference.GenericParameterCount
        );
      }
      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      if (nestedTypeReference != null) {
        return this.GetNestedTypeReferenceInternId(
          nestedTypeReference.ContainingType,
          nestedTypeReference.Name,
          nestedTypeReference.GenericParameterCount
        );
      }
      IArrayTypeReference/*?*/ arrayTypeReference = typeReference as IArrayTypeReference;
      if (arrayTypeReference != null) {
        if (arrayTypeReference.IsVector) {
          return this.GetVectorTypeReferenceInternId(arrayTypeReference.ElementType);
        } else {
          return this.GetMatrixTypeReferenceInternId(
            arrayTypeReference.ElementType,
            (int)arrayTypeReference.Rank,
            arrayTypeReference.Sizes,
            arrayTypeReference.LowerBounds
          );
        }
      }
      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null) {
        return this.GetGenericTypeInstanceReferenceInternId(
          genericTypeInstanceReference.GenericType,
          genericTypeInstanceReference.GenericArguments
        );
      }
      IPointerTypeReference/*?*/ pointerTypeReference = typeReference as IPointerTypeReference;
      if (pointerTypeReference != null) {
        return this.GetPointerTypeReferenceInternId(pointerTypeReference.TargetType);
      }
      IManagedPointerTypeReference managedPointerTypeReference = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeReference != null) {
        return this.GetManagedPointerTypeReferenceInternId(managedPointerTypeReference.TargetType);
      }
      IGenericTypeParameterReference/*?*/ genericTypeParameterReference = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterReference != null) {
        return this.GetGenericTypeParameterReferenceInternId(
          genericTypeParameterReference.DefiningType,
          (int)genericTypeParameterReference.Index
        );
      }
      IGenericMethodParameterReference/*?*/ genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
      if (genericMethodParameterReference != null) {
        return this.GetGenericMethodParameterReferenceInternId(genericMethodParameterReference.DefiningMethod, genericMethodParameterReference.Index);
      }
      IFunctionPointerTypeReference/*?*/ functionPointerTypeReference = typeReference as IFunctionPointerTypeReference;
      if (functionPointerTypeReference != null) {
        IEnumerable<ICustomModifier> returnValueCustomModifiers;
        if (functionPointerTypeReference.ReturnValueIsModified)
          returnValueCustomModifiers = functionPointerTypeReference.ReturnValueCustomModifiers;
        else
          returnValueCustomModifiers = Enumerable<ICustomModifier>.Empty;
        return this.GetFunctionPointerTypeReferenceInternId(
          functionPointerTypeReference.CallingConvention,
          functionPointerTypeReference.Parameters,
          functionPointerTypeReference.ExtraArgumentTypes,
          returnValueCustomModifiers,
          functionPointerTypeReference.ReturnValueIsByRef,
          functionPointerTypeReference.Type
        );
      }
      //^ assume false; //It is an informal requirement that all classes implementing ITypeReference should produce a non null result for one of the calls above.
      return 0;
    }

    uint GetModifiedTypeReferenceInternId(ITypeReference typeReference, IEnumerable<ICustomModifier> customModifiers) {
      Contract.Requires(typeReference != null);
      Contract.Requires(customModifiers != null);

      uint typeReferenceInternId = this.GetTypeReferenceInterendIdIgnoringCustomModifiers(typeReference);
      uint customModifiersInternId = this.GetCustomModifierListInternId(customModifiers.GetEnumerator());
      uint value = this.ModifiedTypeHashtable.Find(typeReferenceInternId, customModifiersInternId);
      if (value == 0) {
        value = this.CurrentTypeInternValue++;
        this.ModifiedTypeHashtable.Add(typeReferenceInternId, customModifiersInternId, value);
      }
      return value;
    }

    uint GetTypeReferenceInternId(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);

      IModifiedTypeReference/*?*/ modifiedTypeReference = typeReference as IModifiedTypeReference;
      if (modifiedTypeReference != null)
        return this.GetModifiedTypeReferenceInternId(modifiedTypeReference.UnmodifiedType, modifiedTypeReference.CustomModifiers);
      return this.GetTypeReferenceInterendIdIgnoringCustomModifiers(typeReference);
    }

    #region IInternFactory Members

    // Interning of module and assembly
    // enables fast comparision of the nominal types. The interned module id takes into account the unification policy applied by the host.
    // For example if mscorlib 1.0 and mscorlib 2.0 is unified by the host both of them will have same intered id, otherwise not.
    // Interned id is 32 bit integer. it is split into 22 bit part and 10 bit part. First part represents the assembly and other part represents module.
    // Simple module which are not part of any assembly is represented by 0 in assembly part and number in module part.
    // Main module of the assembly is represented with something in the assembly part and 0 in the module part.
    // Other modules of the multimodule assembly are represented with assembly part being containing assembly and module part distinct for each module.
    // Note that this places limit on number of modules that can be loaded to be 2^20 and number of modules loaded at 2^12

    uint IInternFactory.GetAssemblyInternedKey(AssemblyIdentity assemblyIdentity) {
      lock (GlobalLock.LockingObject) {
        AssemblyStore assemblyStore = this.GetAssemblyStore(assemblyIdentity);
        return assemblyStore.InternedId;
      }
    }

    uint IInternFactory.GetFieldInternedKey(IFieldReference methodReference) {
      lock (GlobalLock.LockingObject) {
        return this.GetFieldReferenceInternedId(methodReference);
      }
    }

    uint IInternFactory.GetMethodInternedKey(IMethodReference methodReference) {
      lock (GlobalLock.LockingObject) {
        return this.GetMethodReferenceInternedId(methodReference);
      }
    }

    uint IInternFactory.GetModuleInternedKey(ModuleIdentity moduleIdentity) {
      lock (GlobalLock.LockingObject) {
        ModuleStore moduleStore = this.GetModuleStore(moduleIdentity);
        return moduleStore.InternedId;
      }
    }

    uint IInternFactory.GetVectorTypeReferenceInternedKey(ITypeReference elementTypeReference) {
      lock (GlobalLock.LockingObject) {
        return this.GetVectorTypeReferenceInternId(elementTypeReference);
      }
    }

    uint IInternFactory.GetMatrixTypeReferenceInternedKey(ITypeReference elementTypeReference, int rank, IEnumerable<ulong> sizes, IEnumerable<int> lowerBounds) {
      lock (GlobalLock.LockingObject) {
        return this.GetMatrixTypeReferenceInternId(elementTypeReference, rank, sizes, lowerBounds);
      }
    }

    uint IInternFactory.GetGenericTypeInstanceReferenceInternedKey(ITypeReference genericTypeReference, IEnumerable<ITypeReference> genericArguments) {
      lock (GlobalLock.LockingObject) {
        return this.GetGenericTypeInstanceReferenceInternId(genericTypeReference, genericArguments);
      }
    }

    uint IInternFactory.GetPointerTypeReferenceInternedKey(ITypeReference targetTypeReference) {
      lock (GlobalLock.LockingObject) {
        return this.GetPointerTypeReferenceInternId(targetTypeReference);
      }
    }

    uint IInternFactory.GetManagedPointerTypeReferenceInternedKey(ITypeReference targetTypeReference) {
      lock (GlobalLock.LockingObject) {
        return this.GetManagedPointerTypeReferenceInternId(targetTypeReference);
      }
    }

    uint IInternFactory.GetFunctionPointerTypeReferenceInternedKey(CallingConvention callingConvention, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation> extraArgumentTypes, IEnumerable<ICustomModifier> returnValueCustomModifiers, bool returnValueIsByRef, ITypeReference returnType) {
      lock (GlobalLock.LockingObject) {
        return this.GetFunctionPointerTypeReferenceInternId(callingConvention, parameters, extraArgumentTypes, returnValueCustomModifiers, returnValueIsByRef, returnType);
      }
    }

    uint IInternFactory.GetTypeReferenceInternedKey(ITypeReference typeReference) {
      lock (GlobalLock.LockingObject) {
        return this.GetTypeReferenceInternId(typeReference);
      }
    }

    uint IInternFactory.GetNamespaceTypeReferenceInternedKey(IUnitNamespaceReference containingUnitNamespace, IName typeName, uint genericParameterCount) {
      lock (GlobalLock.LockingObject) {
        return this.GetNamespaceTypeReferenceInternId(containingUnitNamespace, typeName, genericParameterCount);
      }
    }

    uint IInternFactory.GetNestedTypeReferenceInternedKey(ITypeReference containingTypeReference, IName typeName, uint genericParameterCount) {
      lock (GlobalLock.LockingObject) {
        return this.GetNestedTypeReferenceInternId(containingTypeReference, typeName, genericParameterCount);
      }
    }

    uint IInternFactory.GetGenericTypeParameterReferenceInternedKey(ITypeReference definingTypeReference, int index) {
      lock (GlobalLock.LockingObject) {
        return this.GetGenericTypeParameterReferenceInternId(definingTypeReference, index);
      }
    }

    uint IInternFactory.GetModifiedTypeReferenceInternedKey(ITypeReference typeReference, IEnumerable<ICustomModifier> customModifiers) {
      lock (GlobalLock.LockingObject) {
        return this.GetModifiedTypeReferenceInternId(typeReference, customModifiers);
      }
    }

    uint IInternFactory.GetGenericMethodParameterReferenceInternedKey(IMethodReference methodReference, int index) {
      lock (GlobalLock.LockingObject) {
        return this.GetGenericMethodParameterReferenceInternId(methodReference, (uint)index);
      }
    }

    #endregion
  }

  /// <summary>
  /// A collection of IName instances that represent names that are commonly used during compilation.
  /// </summary>
  public sealed class NameTable : INameTable {
    //TODO: replace BCL Dictionary with a private implementation that is thread safe and does not need a new list to be allocated for each name
    Dictionary<string, int> caseInsensitiveTable = new Dictionary<string, int>(1024*16, StringComparer.OrdinalIgnoreCase);
    //^ invariant forall{int i in caseInsensitiveTable.Values; i > 0};

    Dictionary<string, IName> caseSensitiveTable = new Dictionary<string, IName>(1024*16);

    int caseInsensitiveCounter = 1; //^ invariant caseInsensitiveCounter >= 0;
    int caseSensitiveCounter = 3; //^ invariant caseSensitiveCounter >= 0;

    /// <summary>
    /// Allocates a collection of IName instances that represent names that are commonly used during compilation.
    /// </summary>
    //^ [NotDelayed]
    public NameTable() {
      emptyName = Dummy.Name;
      //^ base();
      emptyName = this.GetNameFor("");
    }

    IName INameTable.Address {
      get {
        if (this.address == null)
          this.address = this.GetNameFor("Address");
        return this.address;
      }
    }
    IName/*?*/ address;

    /// <summary>
    /// The Empty name.
    /// </summary>
    public IName EmptyName {
      get {
        return this.emptyName;
      }
    }
    readonly IName emptyName;

    IName INameTable.Get {
      get {
        if (this.get == null)
          this.get = this.GetNameFor("Get");
        return this.get;
      }
    }
    IName/*?*/ get;

    /// <summary>
    /// Gets a cached IName instance corresponding to the given string. If no cached instance exists, a new instance is created.
    /// The method is only available to fully trusted code since it allows the caller to cause new objects to be added to the cache.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [Pure]
    public IName GetNameFor(string name)
      //^^ ensures result.Value == name;
    {
      lock (this.caseInsensitiveTable) {
        IName/*?*/ result;
        if (this.caseSensitiveTable.TryGetValue(name, out result)) {
          //^ assume result != null; //Follows from the way Dictionary is instantiated, but the verifier is ignorant of this.
          //^ assume result.Value == name; //Only this routine ever adds entries to the table and it only ever adds entries for which this is true. TODO: it would be nice to be able express this as an invariant.
          return result;
        }
        //string lowerCaseName = name.ToLower(CultureInfo.InvariantCulture); //REVIEW: is it safer to use ToUpperInvariant, or does it make no difference?
        int caseInsensitiveCounter;
        if (!this.caseInsensitiveTable.TryGetValue(name, out caseInsensitiveCounter)) {
          caseInsensitiveCounter = this.caseInsensitiveCounter;
          caseInsensitiveCounter += 17;
          if (caseInsensitiveCounter <= 0) {
            caseInsensitiveCounter = (caseInsensitiveCounter - int.MinValue) + 1000000;
          }
          this.caseInsensitiveCounter = caseInsensitiveCounter;
          this.caseInsensitiveTable.Add(name, caseInsensitiveCounter);
        }
        //^ assume caseInsensitiveCounter > 0; //Follows from the invariant, but this is beyond the verifier right now.
        int caseSensitiveCounter = this.caseSensitiveCounter;
        caseSensitiveCounter += 17;
        if (caseSensitiveCounter <= 0) {
          caseSensitiveCounter = (caseSensitiveCounter - int.MinValue) + 1000000;
          //^ assume caseSensitiveCounter > 0;
        }
        result = new Name(caseInsensitiveCounter, caseSensitiveCounter, name);
        //^ assume result.Value == name;
        this.caseSensitiveCounter = caseSensitiveCounter;
        this.caseSensitiveTable.Add(name, result);
        return result;
      }
    }

    IName INameTable.global {
      get {
        if (this.globalCache == null)
          this.globalCache = this.GetNameFor("global");
        return this.globalCache;
      }
    }
    IName/*?*/ globalCache;

    class Name : IName {
      string value;
      int caseInsensitiveUniqueKey; //^ invariant caseInsensitiveUniqueKey > 0;
      int uniqueKey; //^ invariant uniqueKey > 0;

      internal Name(int caseInsensitiveUniqueKey, int uniqueKey, string value)
        //^ requires caseInsensitiveUniqueKey > 0 && uniqueKey > 0;
      {
        this.caseInsensitiveUniqueKey = caseInsensitiveUniqueKey;
        this.uniqueKey = uniqueKey;
        this.value = value;
      }

      public int UniqueKeyIgnoringCase {
        get { return this.caseInsensitiveUniqueKey; }
      }

      public int UniqueKey {
        get { return this.uniqueKey; }
      }

      public string Value {
        get { return this.value; }
      }

      //^ [Confined]
      public override string ToString() {
        return this.Value;
      }
    }

    IName INameTable.AllowMultiple {
      get {
        if (this.allowMultiple == null)
          this.allowMultiple = this.GetNameFor("AllowMultiple");
        return this.allowMultiple;
      }
    }
    IName/*?*/ allowMultiple;

    IName INameTable.BeginInvoke {
      get {
        if (this.beginInvoke == null)
          this.beginInvoke = this.GetNameFor("BeginInvoke");
        return this.beginInvoke;
      }
    }
    IName/*?*/ beginInvoke;

    IName INameTable.BoolOpBool {
      get {
        if (this.boolOpBool == null)
          this.boolOpBool = this.GetNameFor("bool op bool");
        return this.boolOpBool;
      }
    }
    IName/*?*/ boolOpBool;

    IName INameTable.DecimalOpDecimal {
      get {
        if (this.decimalOpAddition == null)
          this.decimalOpAddition = this.GetNameFor("decimal op decimal");
        return this.decimalOpAddition;
      }
    }
    IName/*?*/ decimalOpAddition;

    IName INameTable.DelegateOpDelegate {
      get {
        if (this.delegateOpAddition == null)
          this.delegateOpAddition = this.GetNameFor("delegate op delegate");
        return this.delegateOpAddition;
      }
    }
    IName/*?*/ delegateOpAddition;

    IName INameTable.EndInvoke {
      get {
        if (this.endInvoke == null)
          this.endInvoke = this.GetNameFor("EndInvoke");
        return this.endInvoke;
      }
    }
    IName/*?*/ endInvoke;

    IName INameTable.EnumOpEnum {
      get {
        if (this.enumOpEnum == null)
          this.enumOpEnum = this.GetNameFor("enum op enum");
        return this.enumOpEnum;
      }
    }
    IName/*?*/ enumOpEnum;

    IName INameTable.EnumOpNum {
      get {
        if (this.enumOpNum == null)
          this.enumOpNum = this.GetNameFor("enum op num");
        return this.enumOpNum;
      }
    }
    IName/*?*/ enumOpNum;

    IName INameTable.Equals {
      get {
        if (this.equals == null)
          this.equals = this.GetNameFor("Equals");
        return this.equals;
      }
    }
    IName/*?*/ equals;

    IName INameTable.Float32OpFloat32 {
      get {
        if (this.float32OpAddition == null)
          this.float32OpAddition = this.GetNameFor("float32 op float32");
        return this.float32OpAddition;
      }
    }
    IName/*?*/ float32OpAddition;

    IName INameTable.Float64OpFloat64 {
      get {
        if (this.float64OpAddition == null)
          this.float64OpAddition = this.GetNameFor("float64 op float64");
        return this.float64OpAddition;
      }
    }
    IName/*?*/ float64OpAddition;

    IName INameTable.HasValue {
      get {
        if (this.hasValue == null)
          this.hasValue = this.GetNameFor("HasValue");
        return this.hasValue;
      }
    }
    IName/*?*/ hasValue;

    IName INameTable.Inherited {
      get {
        if (this.inherited == null)
          this.inherited = this.GetNameFor("Inherited");
        return this.inherited;
      }
    }
    IName/*?*/ inherited;

    IName INameTable.Invoke {
      get {
        if (this.invoke == null)
          this.invoke = this.GetNameFor("Invoke");
        return this.invoke;
      }
    }
    IName/*?*/ invoke;

    IName INameTable.Int16OpInt16 {
      get {
        if (this.int16OpInt16 == null)
          this.int16OpInt16 = this.GetNameFor("int16 op int16");
        return this.int16OpInt16;
      }
    }
    IName/*?*/ int16OpInt16;

    IName INameTable.Int32OpInt32 {
      get {
        if (this.int32OpInt32 == null)
          this.int32OpInt32 = this.GetNameFor("int32 op int32");
        return this.int32OpInt32;
      }
    }
    IName/*?*/ int32OpInt32;

    IName INameTable.Int32OpUInt32 {
      get {
        if (this.int32OpUInt32 == null)
          this.int32OpUInt32 = this.GetNameFor("int32 op uint32");
        return this.int32OpUInt32;
      }
    }
    IName/*?*/ int32OpUInt32;

    IName INameTable.Int64OpInt32 {
      get {
        if (this.int64OpInt32 == null)
          this.int64OpInt32 = this.GetNameFor("int64 op int32");
        return this.int64OpInt32;
      }
    }
    IName/*?*/ int64OpInt32;

    IName INameTable.Int64OpUInt32 {
      get {
        if (this.int64OpUInt32 == null)
          this.int64OpUInt32 = this.GetNameFor("int64 op uint32");
        return this.int64OpUInt32;
      }
    }
    IName/*?*/ int64OpUInt32;

    IName INameTable.Int64OpUInt64 {
      get {
        if (this.int64OpUInt64 == null)
          this.int64OpUInt64 = this.GetNameFor("int64 op uint64");
        return this.int64OpUInt64;
      }
    }
    IName/*?*/ int64OpUInt64;

    IName INameTable.Int64OpInt64 {
      get {
        if (this.int64OpInt64 == null)
          this.int64OpInt64 = this.GetNameFor("int64 op int64");
        return this.int64OpInt64;
      }
    }
    IName/*?*/ int64OpInt64;

    IName INameTable.Int8OpInt8 {
      get {
        if (this.int8OpInt8 == null)
          this.int8OpInt8 = this.GetNameFor("int8 op int8");
        return this.int8OpInt8;
      }
    }
    IName/*?*/ int8OpInt8;

    IName INameTable.NullCoalescing {
      get {
        if (this.nullCoalescing == null)
          this.nullCoalescing = this.GetNameFor("operator ??(object, object)");
        return this.nullCoalescing;
      }
    }
    IName/*?*/ nullCoalescing;

    IName INameTable.NumOpEnum {
      get {
        if (this.numOpEnum == null)
          this.numOpEnum = this.GetNameFor("num op enum");
        return this.numOpEnum;
      }
    }
    IName/*?*/ numOpEnum;

    IName INameTable.ObjectOpObject {
      get {
        if (this.objectOpObject == null)
          this.objectOpObject = this.GetNameFor("object op object");
        return this.objectOpObject;
      }
    }
    IName/*?*/ objectOpObject;

    IName INameTable.ObjectOpString {
      get {
        if (this.objectOpString == null)
          this.objectOpString = this.GetNameFor("object op string");
        return this.objectOpString;
      }
    }
    IName/*?*/ objectOpString;

    IName INameTable.OpAddition {
      get {
        if (this.opAddition == null)
          this.opAddition = this.GetNameFor("op_Addition");
        return this.opAddition;
      }
    }
    IName/*?*/ opAddition;

    IName INameTable.OpBoolean {
      get {
        if (this.opBoolean == null)
          this.opBoolean = this.GetNameFor("op boolean");
        return this.opBoolean;
      }
    }
    IName/*?*/ opBoolean;

    IName INameTable.OpChar {
      get {
        if (this.opChar == null)
          this.opChar = this.GetNameFor("op char");
        return this.opChar;
      }
    }
    IName/*?*/ opChar;

    IName INameTable.OpDecimal {
      get {
        if (this.opDecimal == null)
          this.opDecimal = this.GetNameFor("op decimal");
        return this.opDecimal;
      }
    }
    IName/*?*/ opDecimal;

    IName INameTable.OpEnum {
      get {
        if (this.opEnum == null)
          this.opEnum = this.GetNameFor("op enum");
        return this.opEnum;
      }
    }
    IName/*?*/ opEnum;

    IName INameTable.OpEquality {
      get {
        if (this.opEquality == null)
          this.opEquality = this.GetNameFor("op_Equality");
        return this.opEquality;
      }
    }
    IName/*?*/ opEquality;

    IName INameTable.OpExplicit {
      get {
        if (this.opExplicit == null)
          this.opExplicit = this.GetNameFor("op_Explicit");
        return this.opExplicit;
      }
    }
    IName/*?*/ opExplicit;

    IName INameTable.OpImplicit {
      get {
        if (this.opImplicit == null)
          this.opImplicit = this.GetNameFor("op_Implicit");
        return this.opImplicit;
      }
    }
    IName/*?*/ opImplicit;

    IName INameTable.OpInequality {
      get {
        if (this.opInequality == null)
          this.opInequality = this.GetNameFor("op_Inequality");
        return this.opInequality;
      }
    }
    IName/*?*/ opInequality;

    IName INameTable.OpInt8 {
      get {
        if (this.opInt8 == null)
          this.opInt8 = this.GetNameFor("op int8");
        return this.opInt8;
      }
    }
    IName/*?*/ opInt8;

    IName INameTable.OpInt16 {
      get {
        if (this.opInt16 == null)
          this.opInt16 = this.GetNameFor("op int16");
        return this.opInt16;
      }
    }
    IName/*?*/ opInt16;

    IName INameTable.OpInt32 {
      get {
        if (this.opInt32 == null)
          this.opInt32 = this.GetNameFor("op int32");
        return this.opInt32;
      }
    }
    IName/*?*/ opInt32;

    IName INameTable.OpInt64 {
      get {
        if (this.opInt64 == null)
          this.opInt64 = this.GetNameFor("op int64");
        return this.opInt64;
      }
    }
    IName/*?*/ opInt64;

    IName INameTable.OpBitwiseAnd {
      get {
        if (this.opBitwiseAnd == null)
          this.opBitwiseAnd = this.GetNameFor("op_BitwiseAnd");
        return this.opBitwiseAnd;
      }
    }
    IName/*?*/ opBitwiseAnd;

    IName INameTable.OpBitwiseOr {
      get {
        if (this.opBitwiseOr == null)
          this.opBitwiseOr = this.GetNameFor("op_BitwiseOr");
        return this.opBitwiseOr;
      }
    }
    IName/*?*/ opBitwiseOr;

    IName INameTable.OpComma {
      get {
        if (this.opComma == null)
          this.opComma = this.GetNameFor("op_Comma");
        return this.opComma;
      }
    }
    IName/*?*/ opComma;

    IName INameTable.OpConcatentation {
      get {
        if (this.opConcatentation == null)
          this.opConcatentation = this.GetNameFor("op_Concatentation");
        return this.opConcatentation;
      }
    }
    IName/*?*/ opConcatentation;

    IName INameTable.OpDivision {
      get {
        if (this.opDivision == null)
          this.opDivision = this.GetNameFor("op_Division");
        return this.opDivision;
      }
    }
    IName/*?*/ opDivision;

    IName INameTable.OpExclusiveOr {
      get {
        if (this.opExclusiveOr == null)
          this.opExclusiveOr = this.GetNameFor("op_ExclusiveOr");
        return this.opExclusiveOr;
      }
    }
    IName/*?*/ opExclusiveOr;

    IName INameTable.OpExponentiation {
      get {
        if (this.opExponentiation == null)
          this.opExponentiation = this.GetNameFor("op_Exponentiation");
        return this.opExponentiation;
      }
    }
    IName/*?*/ opExponentiation;

    IName INameTable.OpFalse {
      get {
        if (this.opFalse == null)
          this.opFalse = this.GetNameFor("op_False");
        return this.opFalse;
      }
    }
    IName/*?*/ opFalse;

    IName INameTable.OpFloat32 {
      get {
        if (this.opFloat32 == null)
          this.opFloat32 = this.GetNameFor("op float32");
        return this.opFloat32;
      }
    }
    IName/*?*/ opFloat32;

    IName INameTable.OpFloat64 {
      get {
        if (this.opFloat64 == null)
          this.opFloat64 = this.GetNameFor("op float64");
        return this.opFloat64;
      }
    }
    IName/*?*/ opFloat64;

    IName INameTable.OpGreaterThan {
      get {
        if (this.opGreaterThan == null)
          this.opGreaterThan = this.GetNameFor("op_GreaterThan");
        return this.opGreaterThan;
      }
    }
    IName/*?*/ opGreaterThan;

    IName INameTable.OpGreaterThanOrEqual {
      get {
        if (this.opGreaterThanOrEqual == null)
          this.opGreaterThanOrEqual = this.GetNameFor("op_GreaterThanOrEqual");
        return this.opGreaterThanOrEqual;
      }
    }
    IName/*?*/ opGreaterThanOrEqual;

    IName INameTable.OpIntegerDivision {
      get {
        if (this.opIntegerDivision == null)
          this.opIntegerDivision = this.GetNameFor("op_IntegerDivision");
        return this.opIntegerDivision;
      }
    }
    IName/*?*/ opIntegerDivision;

    IName INameTable.OpLeftShift {
      get {
        if (this.opLeftShift == null)
          this.opLeftShift = this.GetNameFor("op_LeftShift");
        return this.opLeftShift;
      }
    }
    IName/*?*/ opLeftShift;

    IName INameTable.OpLessThan {
      get {
        if (this.opLessThan == null)
          this.opLessThan = this.GetNameFor("op_LessThan");
        return this.opLessThan;
      }
    }
    IName/*?*/ opLessThan;

    IName INameTable.OpLessThanOrEqual {
      get {
        if (this.opLessThanOrEqual == null)
          this.opLessThanOrEqual = this.GetNameFor("op_LessThanOrEqual");
        return this.opLessThanOrEqual;
      }
    }
    IName/*?*/ opLessThanOrEqual;

    IName INameTable.OpLike {
      get {
        if (this.opLogicalAnd == null)
          this.opLogicalAnd = this.GetNameFor("op_Like");
        return this.opLogicalAnd;
      }
    }
    IName/*?*/ opLogicalAnd;

    IName INameTable.OpLogicalNot {
      get {
        if (this.opLogicalNot == null)
          this.opLogicalNot = this.GetNameFor("op_LogicalNot");
        return this.opLogicalNot;
      }
    }
    IName/*?*/ opLogicalNot;

    IName INameTable.OpLogicalOr {
      get {
        if (this.opLogicalOr == null)
          this.opLogicalOr = this.GetNameFor("op_LogicalOr");
        return this.opLogicalOr;
      }
    }
    IName/*?*/ opLogicalOr;

    IName INameTable.OpModulus {
      get {
        if (this.opModulus == null)
          this.opModulus = this.GetNameFor("op_Modulus");
        return this.opModulus;
      }
    }
    IName/*?*/ opModulus;

    IName INameTable.OpMultiply {
      get {
        if (this.opMultiplication == null)
          this.opMultiplication = this.GetNameFor("op_Multiply");
        return this.opMultiplication;
      }
    }
    IName/*?*/ opMultiplication;

    IName INameTable.OpOnesComplement {
      get {
        if (this.opOnesComplement == null)
          this.opOnesComplement = this.GetNameFor("op_OnesComplement");
        return this.opOnesComplement;
      }
    }
    IName/*?*/ opOnesComplement;

    IName INameTable.OpDecrement {
      get {
        if (this.opDecrement == null)
          this.opDecrement = this.GetNameFor("op_Decrement");
        return this.opDecrement;
      }
    }
    IName/*?*/ opDecrement;

    IName INameTable.OpIncrement {
      get {
        if (this.opIncrement == null)
          this.opIncrement = this.GetNameFor("op_Increment");
        return this.opIncrement;
      }
    }
    IName/*?*/ opIncrement;

    IName INameTable.OpRightShift {
      get {
        if (this.opRightShift == null)
          this.opRightShift = this.GetNameFor("op_RightShift");
        return this.opRightShift;
      }
    }
    IName/*?*/ opRightShift;

    IName INameTable.OpSubtraction {
      get {
        if (this.opSubtraction == null)
          this.opSubtraction = this.GetNameFor("op_Subtraction");
        return this.opSubtraction;
      }
    }
    IName/*?*/ opSubtraction;

    IName INameTable.OpTrue {
      get {
        if (this.opTrue == null)
          this.opTrue = this.GetNameFor("op_True");
        return this.opTrue;
      }
    }
    IName/*?*/ opTrue;

    IName INameTable.OpUInt8 {
      get {
        if (this.opUInt8 == null)
          this.opUInt8 = this.GetNameFor("op uint8");
        return this.opUInt8;
      }
    }
    IName/*?*/ opUInt8;

    IName INameTable.OpUInt16 {
      get {
        if (this.opUInt16 == null)
          this.opUInt16 = this.GetNameFor("op uint16");
        return this.opUInt16;
      }
    }
    IName/*?*/ opUInt16;

    IName INameTable.OpUInt32 {
      get {
        if (this.opUInt32 == null)
          this.opUInt32 = this.GetNameFor("op uint32");
        return this.opUInt32;
      }
    }
    IName/*?*/ opUInt32;

    IName INameTable.OpUInt64 {
      get {
        if (this.opUInt64 == null)
          this.opUInt64 = this.GetNameFor("op uint64");
        return this.opUInt64;
      }
    }
    IName/*?*/ opUInt64;

    IName INameTable.OpUnaryNegation {
      get {
        if (this.opUnaryNegation == null)
          this.opUnaryNegation = this.GetNameFor("op_UnaryNegation");
        return this.opUnaryNegation;
      }
    }
    IName/*?*/ opUnaryNegation;

    IName INameTable.OpUnaryPlus {
      get {
        if (this.opUnaryPlus == null)
          this.opUnaryPlus = this.GetNameFor("op_UnaryPlus");
        return this.opUnaryPlus;
      }
    }
    IName/*?*/ opUnaryPlus;

    IName INameTable.Remove {
      get {
        if (this.remove == null)
          this.remove = this.GetNameFor("Remove");
        return this.remove;
      }
    }
    IName/*?*/ remove;

    IName INameTable.Result {
      get {
        if (this.result == null)
          this.result = this.GetNameFor("result");
        return this.result;
      }
    }
    IName/*?*/ result;

    IName INameTable.StringOpString {
      get {
        if (this.stringOpString == null)
          this.stringOpString = this.GetNameFor("string op string");
        return this.stringOpString;
      }
    }
    IName/*?*/ stringOpString;

    IName INameTable.StringOpObject {
      get {
        if (this.stringOpObject == null)
          this.stringOpObject = this.GetNameFor("string op object");
        return this.stringOpObject;
      }
    }
    IName/*?*/ stringOpObject;

    IName INameTable.UInt32OpInt32 {
      get {
        if (this.uint32OpInt32 == null)
          this.uint32OpInt32 = this.GetNameFor("uint32 op int32");
        return this.uint32OpInt32;
      }
    }
    IName/*?*/ uint32OpInt32;

    IName INameTable.UInt32OpUInt32 {
      get {
        if (this.uint32OpUInt32 == null)
          this.uint32OpUInt32 = this.GetNameFor("uint32 op uint32");
        return this.uint32OpUInt32;
      }
    }
    IName/*?*/ uint32OpUInt32;

    IName INameTable.UInt64OpInt32 {
      get {
        if (this.uint64OpInt32 == null)
          this.uint64OpInt32 = this.GetNameFor("uint64 op int32");
        return this.uint64OpInt32;
      }
    }
    IName/*?*/ uint64OpInt32;

    IName INameTable.UInt64OpUInt32 {
      get {
        if (this.uint64OpUInt32 == null)
          this.uint64OpUInt32 = this.GetNameFor("uint64 op uint32");
        return this.uint64OpUInt32;
      }
    }
    IName/*?*/ uint64OpUInt32;

    IName INameTable.UInt64OpUInt64 {
      get {
        if (this.uint64OpUInt64 == null)
          this.uint64OpUInt64 = this.GetNameFor("uint64 op uint64");
        return this.uint64OpUInt64;
      }
    }
    IName/*?*/ uint64OpUInt64;

    IName INameTable.UIntPtrOpUIntPtr {
      get {
        if (this.uintPtrOpUIntPtr == null)
          this.uintPtrOpUIntPtr = this.GetNameFor("uintPtr op uintPtr");
        return this.uintPtrOpUIntPtr;
      }
    }
    IName/*?*/ uintPtrOpUIntPtr;

    IName INameTable.value {
      get {
        if (this.valueCache == null)
          this.valueCache = this.GetNameFor("value");
        return this.valueCache;
      }
    }
    IName/*?*/ valueCache;

    IName INameTable.VoidPtrOpVoidPtr {
      get {
        if (this.voidPtrOpVoidPtr == null)
          this.voidPtrOpVoidPtr = this.GetNameFor("void* op void*");
        return this.voidPtrOpVoidPtr;
      }
    }
    IName/*?*/ voidPtrOpVoidPtr;

    IName INameTable.System {
      get {
        if (this.systemCache == null)
          this.systemCache = this.GetNameFor("System");
        return this.systemCache;
      }
    }
    IName/*?*/ systemCache;

    IName INameTable.Void {
      get {
        if (this.voidCache == null)
          this.voidCache = this.GetNameFor("Void");
        return this.voidCache;
      }
    }
    IName/*?*/ voidCache;

    IName INameTable.Boolean {
      get {
        if (this.booleanCache == null)
          this.booleanCache = this.GetNameFor("Boolean");
        return this.booleanCache;
      }
    }
    IName/*?*/ booleanCache;

    IName INameTable.Cctor {
      get {
        if (this.cctorCache == null)
          this.cctorCache = this.GetNameFor(".cctor");
        return this.cctorCache;
      }
    }
    IName/*?*/ cctorCache;

    IName INameTable.Char {
      get {
        if (this.charCache == null)
          this.charCache = this.GetNameFor("Char");
        return this.charCache;
      }
    }
    IName/*?*/ charCache;

    IName INameTable.Ctor {
      get {
        if (this.ctorCache == null)
          this.ctorCache = this.GetNameFor(".ctor");
        return this.ctorCache;
      }
    }
    IName/*?*/ ctorCache;

    IName INameTable.Byte {
      get {
        if (this.byteCache == null)
          this.byteCache = this.GetNameFor("Byte");
        return this.byteCache;
      }
    }
    IName/*?*/ byteCache;

    IName INameTable.SByte {
      get {
        if (this.sbyteCache == null)
          this.sbyteCache = this.GetNameFor("SByte");
        return this.sbyteCache;
      }
    }
    IName/*?*/ sbyteCache;

    IName INameTable.Int16 {
      get {
        if (this.int16Cache == null)
          this.int16Cache = this.GetNameFor("Int16");
        return this.int16Cache;
      }
    }
    IName/*?*/ int16Cache;

    IName INameTable.UInt16 {
      get {
        if (this.uint16Cache == null)
          this.uint16Cache = this.GetNameFor("UInt16");
        return this.uint16Cache;
      }
    }
    IName/*?*/ uint16Cache;

    IName INameTable.Int32 {
      get {
        if (this.int32Cache == null)
          this.int32Cache = this.GetNameFor("Int32");
        return this.int32Cache;
      }
    }
    IName/*?*/ int32Cache;

    IName INameTable.UInt32 {
      get {
        if (this.uint32Cache == null)
          this.uint32Cache = this.GetNameFor("UInt32");
        return this.uint32Cache;
      }
    }
    IName/*?*/ uint32Cache;

    IName INameTable.Int64 {
      get {
        if (this.int64Cache == null)
          this.int64Cache = this.GetNameFor("Int64");
        return this.int64Cache;
      }
    }
    IName/*?*/ int64Cache;

    IName INameTable.UInt64 {
      get {
        if (this.uint64Cache == null)
          this.uint64Cache = this.GetNameFor("UInt64");
        return this.uint64Cache;
      }
    }
    IName/*?*/ uint64Cache;

    IName INameTable.String {
      get {
        if (this.stringCache == null)
          this.stringCache = this.GetNameFor("String");
        return this.stringCache;
      }
    }
    IName/*?*/ stringCache;

    IName INameTable.IntPtr {
      get {
        if (this.intPtrCache == null)
          this.intPtrCache = this.GetNameFor("IntPtr");
        return this.intPtrCache;
      }
    }
    IName/*?*/ intPtrCache;

    IName INameTable.UIntPtr {
      get {
        if (this.uintPtrCache == null)
          this.uintPtrCache = this.GetNameFor("UIntPtr");
        return this.uintPtrCache;
      }
    }
    IName/*?*/ uintPtrCache;

    IName INameTable.Object {
      get {
        if (this.objectCache == null)
          this.objectCache = this.GetNameFor("Object");
        return this.objectCache;
      }
    }
    IName/*?*/ objectCache;

    IName INameTable.Set {
      get {
        if (this.@set == null)
          this.@set = this.GetNameFor("Set");
        return this.@set;
      }
    }
    IName/*?*/ @set;

    IName INameTable.Single {
      get {
        if (this.singleCache == null)
          this.singleCache = this.GetNameFor("Single");
        return this.singleCache;
      }
    }
    IName/*?*/ singleCache;

    IName INameTable.Double {
      get {
        if (this.doubleCache == null)
          this.doubleCache = this.GetNameFor("Double");
        return this.doubleCache;
      }
    }
    IName/*?*/ doubleCache;

    IName INameTable.TypedReference {
      get {
        if (this.typedReferenceCache == null)
          this.typedReferenceCache = this.GetNameFor("TypedReference");
        return this.typedReferenceCache;
      }
    }
    IName/*?*/ typedReferenceCache;

    IName INameTable.Enum {
      get {
        if (this.enumCache == null)
          this.enumCache = this.GetNameFor("Enum");
        return this.enumCache;
      }
    }
    IName/*?*/ enumCache;

    IName INameTable.MulticastDelegate {
      get {
        if (this.multicastDelegateCache == null)
          this.multicastDelegateCache = this.GetNameFor("MulticastDelegate");
        return this.multicastDelegateCache;
      }
    }
    IName/*?*/ multicastDelegateCache;

    IName INameTable.ValueType {
      get {
        if (this.valueTypeCache == null)
          this.valueTypeCache = this.GetNameFor("ValueType");
        return this.valueTypeCache;
      }
    }
    IName/*?*/ valueTypeCache;

    IName INameTable.Type {
      get {
        if (this.type == null)
          this.type = this.GetNameFor("Type");
        return this.type;
      }
    }
    IName/*?*/ type;

    IName INameTable.Array {
      get {
        if (this.array == null)
          this.array = this.GetNameFor("Array");
        return this.array;
      }
    }
    IName/*?*/ array;

    IName INameTable.AttributeUsageAttribute {
      get {
        if (this.attributeUsage == null)
          this.attributeUsage = this.GetNameFor("AttributeUsageAttribute");
        return this.attributeUsage;
      }
    }
    IName/*?*/ attributeUsage;

    IName INameTable.Attribute {
      get {
        if (this.attribute == null)
          this.attribute = this.GetNameFor("Attribute");
        return this.attribute;
      }
    }
    IName/*?*/ attribute;

    IName INameTable.DateTime {
      get {
        if (this.dateTime == null)
          this.dateTime = this.GetNameFor("DateTime");
        return this.dateTime;
      }
    }
    IName/*?*/ dateTime;

    IName INameTable.DebuggerHiddenAttribute {
      get {
        if (this.debuggerHiddenAttribute == null)
          this.debuggerHiddenAttribute = this.GetNameFor("DebuggerHiddenAttribute");
        return this.debuggerHiddenAttribute;
      }
    }
    IName/*?*/ debuggerHiddenAttribute;

    IName INameTable.Decimal {
      get {
        if (this.@decimal == null)
          this.@decimal = this.GetNameFor("Decimal");
        return this.@decimal;
      }
    }
    IName/*?*/ @decimal;

    IName INameTable.Delegate {
      get {
        if (this.@delegate == null)
          this.@delegate = this.GetNameFor("Delegate");
        return this.@delegate;
      }
    }
    IName/*?*/ @delegate;

    IName INameTable.Diagnostics {
      get {
        if (this.diagnostics == null)
          this.diagnostics = this.GetNameFor("Diagnostics");
        return this.diagnostics;
      }
    }
    IName/*?*/ diagnostics;

    IName INameTable.DBNull {
      get {
        if (this.dbNull == null)
          this.dbNull = this.GetNameFor("DBNull");
        return this.dbNull;
      }
    }
    IName/*?*/dbNull;

    IName INameTable.Length {
      get {
        if (this.length == null)
          this.length = this.GetNameFor("Length");
        return this.length;
      }
    }
    IName/*?*/ length;

    IName INameTable.LongLength {
      get {
        if (this.longLength == null)
          this.longLength = this.GetNameFor("LongLength");
        return this.longLength;
      }
    }
    IName/*?*/ longLength;

    IName INameTable.Nullable {
      get {
        if (this.nullable == null)
          this.nullable = this.GetNameFor("Nullable");
        return this.nullable;
      }
    }
    IName/*?*/ nullable;

    IName INameTable.Combine {
      get {
        if (this.combine == null)
          this.combine = this.GetNameFor("Combine");
        return this.combine;
      }
    }
    IName/*?*/ combine;

    IName INameTable.Concat {
      get {
        if (this.concat == null)
          this.concat = this.GetNameFor("Concat");
        return this.concat;
      }
    }
    IName/*?*/ concat;
  }
}
