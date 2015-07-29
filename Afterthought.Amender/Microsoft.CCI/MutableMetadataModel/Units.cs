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
using System.Text;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// Represents a .NET assembly.
  /// </summary>
  public sealed class Assembly : Module, IAssembly, ICopyFrom<IAssembly> {

    /// <summary>
    /// 
    /// </summary>
    public Assembly() {
      this.assemblyAttributes = null;
      this.culture = "";
      this.exportedTypes = null;
      this.flags = 0;
      this.files = null;
      this.hashValue = Enumerable<byte>.Empty;
      this.memberModules = null;
      this.moduleName = Dummy.Name;
      this.publicKey = null;
      this.resources = null;
      this.securityAttributes = null;
      this.version = new Version(0, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="internFactory"></param>
    public void Copy(IAssembly assembly, IInternFactory internFactory) {
      ((ICopyFrom<IModule>)this).Copy(assembly, internFactory);
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.AssemblyAttributes))
        this.assemblyAttributes = new List<ICustomAttribute>(assembly.AssemblyAttributes);
      else
        this.assemblyAttributes = null;
      this.culture = assembly.Culture;
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.ExportedTypes))
        this.exportedTypes = new List<IAliasForType>(assembly.ExportedTypes);
      else
        this.exportedTypes = null;
      this.flags = assembly.Flags;
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.Files))
        this.files = new List<IFileReference>(assembly.Files);
      else
        this.files = null;
      this.hashValue = assembly.HashValue;
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.MemberModules))
        this.memberModules = new List<IModule>(assembly.MemberModules);
      else
        this.memberModules = null;
      this.moduleName = assembly.ModuleName;
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.PublicKey))
        this.publicKey = new List<byte>(assembly.PublicKey);
      else
        this.publicKey = null;
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.Resources))
        this.resources = new List<IResourceReference>(assembly.Resources);
      else
        this.resources = null;
      if (IteratorHelper.EnumerableIsNotEmpty(assembly.SecurityAttributes))
        this.securityAttributes = new List<ISecurityAttribute>(assembly.SecurityAttributes);
      else
        this.securityAttributes = null;
      this.version = assembly.Version;
    }

    /// <summary>
    /// A list of objects representing persisted instances of types that extend System.Attribute. Provides an extensible way to associate metadata
    /// with this assembly. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ AssemblyAttributes {
      get { return this.assemblyAttributes; }
      set { this.assemblyAttributes = value; }
    }
    List<ICustomAttribute>/*?*/ assemblyAttributes;

    /// <summary>
    /// The Assembly that contains this module. If this module is main module then this returns this. May be null.
    /// </summary>
    /// <value></value>
    public override IAssembly/*?*/ ContainingAssembly {
      get { return this; }
    }

    /// <summary>
    /// Identifies the culture associated with the assembly. Typically specified for sattelite assemblies with localized resources.
    /// Empty if not specified.
    /// </summary>
    public string Culture {
      get { return this.culture; }
      set { this.culture = value; this.assemblyIdentity = null; }
    }
    string culture;

    /// <summary>
    /// Calls visitor.Visit(IAssembly).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IAssemblyReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IAssemblyReference)this);
    }

    /// <summary>
    /// Public types defined in other modules making up this assembly and to which other assemblies may refer to via this assembly. May be null.
    /// </summary>
    public List<IAliasForType>/*?*/ ExportedTypes {
      get { return this.exportedTypes; }
      set { this.exportedTypes = value; }
    }
    List<IAliasForType>/*?*/ exportedTypes;

    /// <summary>
    /// A set of bits and bit ranges representing properties of the assembly. The value of <see cref="Flags"/> can be set
    /// from source code via the AssemblyFlags assembly custom attribute. The interpretation of the property depends on the target platform.
    /// </summary>
    public uint Flags {
      get { return this.flags; }
      set { this.flags = value; }
    }
    uint flags;

    /// <summary>
    /// A list of the files that constitute the assembly. These are not the source language files that may have been
    /// used to compile the assembly, but the files that contain constituent modules of a multi-module assembly as well
    /// as any external resources. It corresonds to the File table of the .NET assembly file format. May be null.
    /// </summary>
    /// <value></value>
    public List<IFileReference>/*?*/ Files {
      get { return this.files; }
      set { this.files = value; }
    }
    List<IFileReference>/*?*/ files;

    /// <summary>
    /// True if the implementation of the referenced assembly used at runtime is not expected to match the version seen at compile time.
    /// </summary>
    public bool IsRetargetable {
      get { return (this.Flags & 0x100) != 0; }
      set {
        if (value)
          this.Flags |= 0x100u;
        else
          this.Flags &= ~0x100u;
      }
    }

    /// <summary>
    /// True if the referenced assembly contains types that describe objects that are neither COM objects nor objects that are managed by the CLR.
    /// Instances of such types are created and managed by another runtime and are accessed by CLR objects via some form of interoperation mechanism.
    /// </summary>
    public bool ContainsForeignTypes {
      get { return (this.Flags & 0x200) != 0; }
      set {
        if (value)
          this.Flags |= 0x200u;
        else
          this.Flags &= ~0x200u;
      }
    }

    /// <summary>
    /// An indication of the location where the assembly is or will be stored. This need not be a file system path and may be empty.
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to this unit.
    /// </summary>
    /// <value></value>
    public override string Location {
      get { return base.Location; }
      set { base.Location = value; this.assemblyIdentity = null; }
    }

    /// <summary>
    /// A list of the modules that constitute the assembly. May be null.
    /// </summary>
    /// <value></value>
    public List<IModule>/*?*/ MemberModules {
      get { return this.memberModules; }
      set { this.memberModules = value; }
    }
    List<IModule>/*?*/ memberModules;

    /// <summary>
    /// The name of the module containing the assembly manifest. This can be different from the name of the assembly itself.
    /// </summary>
    public override IName ModuleName {
      get { return this.moduleName; }
      set { this.moduleName = value; }
    }
    IName moduleName;

    /// <summary>
    /// Gets the module identity.
    /// </summary>
    /// <value>The module identity.</value>
    public override ModuleIdentity ModuleIdentity {
      get {
        return this.AssemblyIdentity;
      }
    }

    /// <summary>
    /// The name of the assembly.
    /// </summary>
    /// <value></value>
    public override IName Name {
      get { return base.Name; }
      set { base.Name = value; this.assemblyIdentity = null; }
    }

    /// <summary>
    /// The public part of the key used to encrypt the SHA1 hash over the persisted form of this assembly. Null if not specified.
    /// This value is used by the loader to decrypt HashValue which it then compares with a freshly computed hash value to verify the
    /// integrity of the assembly. May be null.
    /// </summary>
    public List<byte>/*?*/ PublicKey {
      get { return this.publicKey; }
      set {
        if (value == null || value.Count == 0)
          this.flags &= ~0x0001u;
        else
          this.flags |= 0x0001u;
        this.publicKey = value;
        this.assemblyIdentity = null;
        this.publicKeyToken = null;
      }
    }
    List<byte>/*?*/ publicKey;

    /// <summary>
    /// A list of named byte sequences persisted with the assembly and used during execution, typically via .NET Framework helper classes. May be null.
    /// </summary>
    /// <value></value>
    public List<IResourceReference>/*?*/ Resources {
      get { return this.resources; }
      set { this.resources = value; }
    }
    List<IResourceReference>/*?*/ resources;

    /// <summary>
    /// A list of objects representing persisted instances of pairs of security actions and sets of security permissions.
    /// These apply by default to every method reachable from the module. May be null.
    /// </summary>
    /// <value></value>
    public List<ISecurityAttribute>/*?*/ SecurityAttributes {
      get { return this.securityAttributes; }
      set { this.securityAttributes = value; }
    }
    List<ISecurityAttribute>/*?*/ securityAttributes;

    /// <summary>
    /// The version of the assembly.
    /// </summary>
    public Version Version {
      get { return this.version; }
      set { this.version = value; this.assemblyIdentity = null; }
    }
    Version version;

    #region IAssembly Members

    IEnumerable<ICustomAttribute> IAssembly.AssemblyAttributes {
      get {
        if (this.AssemblyAttributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.AssemblyAttributes.AsReadOnly();
      }
    }

    IEnumerable<IAliasForType> IAssembly.ExportedTypes {
      get {
        if (this.ExportedTypes == null) return Enumerable<IAliasForType>.Empty;
        return this.ExportedTypes.AsReadOnly();
      }
    }

    IEnumerable<IResourceReference> IAssembly.Resources {
      get {
        if (this.Resources == null) return Enumerable<IResourceReference>.Empty;
        return this.resources.AsReadOnly();
      }
    }

    IEnumerable<IFileReference> IAssembly.Files {
      get {
        if (this.Files == null) return Enumerable<IFileReference>.Empty;
        return this.Files.AsReadOnly();
      }
    }

    IEnumerable<IModule> IAssembly.MemberModules {
      get {
        if (this.MemberModules == null) return Enumerable<IModule>.Empty;
        return this.MemberModules.AsReadOnly();
      }
    }

    IEnumerable<ISecurityAttribute> IAssembly.SecurityAttributes {
      get {
        if (this.SecurityAttributes == null) return Enumerable<ISecurityAttribute>.Empty;
        return this.SecurityAttributes.AsReadOnly();
      }
    }

    #endregion

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return this; }
    }

    #endregion

    #region IAssemblyReference Members

    /// <summary>
    /// The identity of the referenced assembly.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might be empty.</remarks>
    public AssemblyIdentity AssemblyIdentity {
      get {
        if (this.assemblyIdentity == null) {
          this.assemblyIdentity = UnitHelper.GetAssemblyIdentity(this);
        }
        return this.assemblyIdentity;
      }
    }
    AssemblyIdentity/*?*/ assemblyIdentity;

    /// <summary>
    /// Returns the identity of the assembly reference to which this assembly reference has been unified.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public AssemblyIdentity UnifiedAssemblyIdentity {
      get { return this.AssemblyIdentity; }
    }

    /// <summary>
    /// A list of aliases for the root namespace of the referenced assembly.
    /// </summary>
    /// <value></value>
    public IEnumerable<IName> Aliases {
      get { return Enumerable<IName>.Empty; }
    }

    /// <summary>
    /// The referenced assembly, or Dummy.Assembly if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public IAssembly ResolvedAssembly {
      get { return this; }
    }

    IEnumerable<byte> IAssemblyReference.PublicKey {
      get {
        if (this.PublicKey == null) return Enumerable<byte>.Empty;
        return this.PublicKey.AsReadOnly();
      }
    }

    /// <summary>
    /// The encrypted SHA1 hash of the persisted form of the assembly.
    /// </summary>
    public IEnumerable<byte> HashValue {
      get { return this.hashValue; }
      set { this.hashValue = value; }
    }
    IEnumerable<byte> hashValue;

    /// <summary>
    /// The hashed 8 bytes of the public key of the referenced assembly. This is empty if the referenced assembly does not have a public key.
    /// </summary>
    /// <value></value>
    public IEnumerable<byte> PublicKeyToken {
      get {
        if (this.PublicKey == null) return Enumerable<byte>.Empty;
        if (this.publicKeyToken == null) {
          var pkt = new List<byte>(UnitHelper.ComputePublicKeyToken(this.PublicKey));
          this.publicKeyToken = pkt.AsReadOnly();
        }
        return this.publicKeyToken;
      }
    }
    IEnumerable<byte>/*?*/ publicKeyToken;

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class AssemblyReference : ModuleReference, IAssemblyReference, ICopyFrom<IAssemblyReference> {

    /// <summary>
    /// 
    /// </summary>
    public AssemblyReference() {
      Contract.Ensures(!this.IsFrozen);
      this.aliases = null;
      this.resolvedAssembly = null;
      this.containsForeignTypes = false;
      this.culture = string.Empty;
      this.hashValue = Enumerable<byte>.Empty;
      this.isRetargetable = false;
      this.publicKey = null;
      this.publicKeyToken = null;
      this.version = new Version(0, 0);
      this.ModuleIdentity = this.assemblyIdentity = Dummy.Assembly.AssemblyIdentity;
      this.unifiedAssemblyIdentity = this.assemblyIdentity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IAssemblyReference assemblyReference, IInternFactory internFactory) {
      ((ICopyFrom<IModuleReference>)this).Copy(assemblyReference, internFactory);
      if (IteratorHelper.EnumerableIsNotEmpty(assemblyReference.Aliases))
        this.aliases = new List<IName>(assemblyReference.Aliases);
      else
        this.aliases = null;
      this.resolvedAssembly = null;
      this.culture = assemblyReference.Culture;
      this.hashValue = assemblyReference.HashValue;
      this.isRetargetable = assemblyReference.IsRetargetable;
      this.containsForeignTypes = assemblyReference.ContainsForeignTypes;
      if (IteratorHelper.EnumerableIsNotEmpty(assemblyReference.PublicKey))
        this.publicKey = assemblyReference.PublicKey;
      else {
        if (IteratorHelper.EnumerableIsNotEmpty(assemblyReference.PublicKeyToken))
          this.publicKeyToken = new List<byte>(assemblyReference.PublicKeyToken);
        else
          this.publicKeyToken = null;
      }
      this.version = assemblyReference.Version;
      this.ModuleIdentity = this.assemblyIdentity = assemblyReference.AssemblyIdentity;
      this.unifiedAssemblyIdentity = assemblyReference.UnifiedAssemblyIdentity;
    }

    /// <summary>
    /// Calls vistor.Visit(IAssemblyReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A list of aliases for the root namespace of the referenced assembly. May be null.
    /// </summary>
    /// <value></value>
    public List<IName>/*?*/ Aliases {
      get { return this.aliases; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.aliases = value;
      }
    }
    List<IName>/*?*/ aliases;

    /// <summary>
    /// Tries to resolves the reference with the aid of this.Host. If resolution fails, the result is Dummy.Module.
    /// </summary>
    protected override IModule Resolve() {
      var result = this.ResolvedAssembly;
      if (result is Dummy) return Dummy.Module;
      return result;
    }

    private IAssembly ResolveAssembly() {
      this.isFrozen = true;
      if (this.Host == null) return Dummy.Assembly;
      var unifiedIdentity = this.UnifiedAssemblyIdentity;
      var result = this.Host.FindAssembly(unifiedIdentity);
      if (!(result is Dummy)) return result;
      if (this.ReferringUnit != null && (String.IsNullOrEmpty(unifiedIdentity.Location) || unifiedIdentity.Location.Equals("unknown://location")))
        unifiedIdentity = this.Host.ProbeAssemblyReference(this.ReferringUnit, unifiedIdentity);
      return this.Host.LoadAssembly(unifiedIdentity);
    }

    /// <summary>
    /// The referenced assembly, or Dummy.Assembly if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public IAssembly ResolvedAssembly {
      get {
        if (this.resolvedAssembly == null)
          this.resolvedAssembly = this.ResolveAssembly();
        return this.resolvedAssembly;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        this.resolvedAssembly = value;
      }
    }
    IAssembly resolvedAssembly;

    /// <summary>
    /// Identifies the culture associated with the assembly reference. Typically specified for sattelite assemblies with localized resources.
    /// Empty if not specified.
    /// </summary>
    /// <value></value>
    public string Culture {
      get { return this.culture; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.culture = value;
      }
    }
    string culture;

    /// <summary>
    /// True if the implementation of the referenced assembly used at runtime is not expected to match the version seen at compile time.
    /// </summary>
    public bool IsRetargetable {
      get { return this.isRetargetable; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.isRetargetable = value;
      }
    }
    bool isRetargetable;

    /// <summary>
    /// True if the referenced assembly contains types that describe objects that are neither COM objects nor objects that are managed by the CLR.
    /// Instances of such types are created and managed by another runtime and are accessed by CLR objects via some form of interoperation mechanism.
    /// </summary>
    public bool ContainsForeignTypes {
      get { return this.containsForeignTypes; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.containsForeignTypes = value;
      }
    }
    bool containsForeignTypes;

    /// <summary>
    /// The encrypted SHA1 hash of the persisted form of the referenced assembly.
    /// </summary>
    public IEnumerable<byte> HashValue {
      get { return this.hashValue; }
      set { this.hashValue = value; }
    }
    IEnumerable<byte> hashValue;

    /// <summary>
    /// The public part of the key used to encrypt the SHA1 hash over the persisted form of the referenced assembly. Null if not specified.
    /// This value is used by the loader to decrypt HashValue which it then compares with a freshly computed hash value to verify the
    /// integrity of the assembly.
    /// </summary>
    public IEnumerable<byte> PublicKey {
      get {
        return this.publicKey??Enumerable<byte>.Empty;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        this.publicKey = value;
        this.assemblyIdentity = null;
        this.publicKeyToken = null;
      }
    }
    IEnumerable<byte>/*?*/ publicKey;

    /// <summary>
    /// The hashed 8 bytes of the public key of the referenced assembly. This is empty if the referenced assembly does not have a public key. May be null.
    /// </summary>
    /// <value></value>
    public List<byte>/*?*/ PublicKeyToken {
      get {
        if (this.publicKeyToken == null && this.publicKey != null)
          this.publicKeyToken = new List<byte>(UnitHelper.ComputePublicKeyToken(this.PublicKey));
        return this.publicKeyToken;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        this.publicKeyToken = value;
        this.publicKey = null;
        this.assemblyIdentity = null;
      }
    }
    List<byte>/*?*/ publicKeyToken;

    /// <summary>
    /// The version of the assembly reference.
    /// </summary>
    /// <value></value>
    public Version Version {
      get { return this.version; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.version = value;
      }
    }
    Version version;

    /// <summary>
    /// The location of the referenced assembly.
    /// </summary>
    public string Location {
      get { return this.location; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.location = value;
      }
    }
    string location;

    /// <summary>
    /// The identity of the referenced assembly. Has the same Culture, Name, PublicKeyToken and Version as the reference.
    /// </summary>
    /// <value></value>
    /// <remarks>Also has a location, which may might be empty. Although mostly redundant, the object returned by this
    /// property is useful because it derives from System.Object and therefore can be used as a hash table key. It may be more efficient
    /// to use the properties defined directly on the reference, since the object returned by this property may be allocated lazily
    /// and the allocation can thus be avoided by using the reference's properties.</remarks>
    public AssemblyIdentity AssemblyIdentity {
      get {
        if (this.assemblyIdentity == null) {
          this.isFrozen = true;
          this.assemblyIdentity = new AssemblyIdentity(this.Name, this.Culture, this.Version, ((IAssemblyReference)this).PublicKeyToken, this.Location);
        }
        return this.assemblyIdentity;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        this.assemblyIdentity = this.unifiedAssemblyIdentity = value;
        this.culture = value.Culture;
        base.Name = value.Name;
        this.publicKeyToken = new List<byte>(value.PublicKeyToken);
        this.version = value.Version;
        this.location = value.Location;
        base.ModuleIdentity = assemblyIdentity;
      }
    }
    AssemblyIdentity assemblyIdentity;

    /// <summary>
    /// Returns the identity of the assembly reference to which this assembly reference has been unified.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public AssemblyIdentity UnifiedAssemblyIdentity {
      get {
        if (this.unifiedAssemblyIdentity == null) {
          this.isFrozen = true;
          this.unifiedAssemblyIdentity = this.Host.UnifyAssembly(this);
        }
        return this.unifiedAssemblyIdentity;
      }
    }
    AssemblyIdentity unifiedAssemblyIdentity;

    #region IAssemblyReference Members

    IEnumerable<IName> IAssemblyReference.Aliases {
      get {
        if (this.Aliases == null) return Enumerable<IName>.Empty;
        return this.Aliases.AsReadOnly();
      }
    }

    IEnumerable<byte> IAssemblyReference.PublicKeyToken {
      get {
        if (this.PublicKeyToken == null) return Enumerable<byte>.Empty;
        return this.PublicKeyToken.AsReadOnly();
      }
    }

    #endregion

  }

  /// <summary>
  /// 
  /// </summary>
  public class Module : Unit, IModule, ICopyFrom<IModule> {

    /// <summary>
    /// 
    /// </summary>
    public Module() {
      this.allTypes = null;
      this.assemblyReferences = null;
      this.baseAddress = 0x400000;
      this.containingAssembly = Dummy.Assembly;
      this.dllCharacteristics = 0;
      this.entryPoint = Dummy.MethodReference;
      this.fileAlignment = 512;
      this.ilOnly = true;
      this.StrongNameSigned = false;
      this.Prefers32bits = false;
      this.kind = ModuleKind.DynamicallyLinkedLibrary;
      this.linkerMajorVersion = 6;
      this.linkerMinorVersion = 0;
      this.metadataFormatMajorVersion = 2; // assume generics
      this.metadataFormatMinorVersion = 0;
      this.moduleAttributes = null;
      this.moduleReferences = null;
      this.persistentIdentifier = Guid.NewGuid();
      this.machine = Machine.Unknown;
      this.requiresAmdInstructionSet = false;
      this.requiresStartupStub = false;
      this.requires32bits = false;
      this.requires64bits = false;
      this.sizeOfHeapCommit = 0x1000;
      this.sizeOfHeapReserve = 0x100000;
      this.sizeOfStackCommit = 0x1000;
      this.sizeOfStackReserve = 0x100000;
      this.strings = null;
      this.subsystemMajorVersion = 4;
      this.subsystemMinorVersion = 0;
      this.targetRuntimeVersion = "";
      this.trackDebugData = false;
      this.typeMemberReferences = null;
      this.typeReferences = null;
      this.usePublicKeyTokensForAssemblyReferences = false;
      this.win32Resources = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="module"></param>
    /// <param name="internFactory"></param>
    public void Copy(IModule module, IInternFactory internFactory) {
      ((ICopyFrom<IUnit>)this).Copy(module, internFactory);
      var allTypes = module.GetAllTypes();
      if (IteratorHelper.EnumerableIsNotEmpty(allTypes))
        this.allTypes = new List<INamedTypeDefinition>(allTypes);
      else
        this.allTypes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(module.AssemblyReferences))
        this.assemblyReferences = new List<IAssemblyReference>(module.AssemblyReferences);
      else
        this.assemblyReferences = null;
      this.baseAddress = module.BaseAddress;
      this.containingAssembly = module.ContainingAssembly;
      this.debugInformationLocation = module.DebugInformationLocation;
      this.debugInformationVersion = module.DebugInformationVersion;
      this.dllCharacteristics = module.DllCharacteristics;
      if (module.Kind == ModuleKind.ConsoleApplication || module.Kind == ModuleKind.WindowsApplication)
        this.entryPoint = module.EntryPoint;
      else
        this.entryPoint = Dummy.MethodReference;
      this.fileAlignment = module.FileAlignment;
      var genericMethodInstances = module.GetGenericMethodInstances();
      if (IteratorHelper.EnumerableIsNotEmpty(genericMethodInstances))
        this.genericMethodInstances = new List<IGenericMethodInstanceReference>(genericMethodInstances);
      else
        this.genericMethodInstances = null;
      this.ilOnly = module.ILOnly;
      this.strongNameSigned = module.StrongNameSigned;
      this.prefers32bits = module.Prefers32bits;
      this.kind = module.Kind;
      this.linkerMajorVersion = module.LinkerMajorVersion;
      this.linkerMinorVersion = module.LinkerMinorVersion;
      this.metadataFormatMajorVersion = module.MetadataFormatMajorVersion;
      this.metadataFormatMinorVersion = module.MetadataFormatMinorVersion;
      if (IteratorHelper.EnumerableIsNotEmpty(module.ModuleAttributes))
        this.moduleAttributes = new List<ICustomAttribute>(module.ModuleAttributes);
      else
        this.moduleAttributes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(module.ModuleReferences))
        this.moduleReferences = new List<IModuleReference>(module.ModuleReferences);
      else
        this.moduleReferences = null;
      this.persistentIdentifier = Guid.NewGuid();
      this.machine = module.Machine;
      this.requiresAmdInstructionSet = module.RequiresAmdInstructionSet;
      this.requiresStartupStub = module.RequiresStartupStub;
      this.requires32bits = module.Requires32bits;
      this.requires64bits = module.Requires64bits;
      this.sizeOfHeapCommit = module.SizeOfHeapCommit;
      this.sizeOfHeapReserve = module.SizeOfHeapReserve;
      this.sizeOfStackCommit = module.SizeOfStackCommit;
      this.sizeOfStackReserve = module.SizeOfStackReserve;
      var strs = module.GetStrings();
      if (IteratorHelper.EnumerableIsNotEmpty(strs))
        this.strings = new List<string>(strs);
      else
        this.strings = null;
      this.subsystemMajorVersion = module.SubsystemMajorVersion;
      this.subsystemMinorVersion = module.SubsystemMinorVersion;
      var structuralTypeInstances = module.GetStructuralTypeInstances();
      if (IteratorHelper.EnumerableIsNotEmpty(structuralTypeInstances))
        this.structuralTypeInstances = new List<ITypeReference>(structuralTypeInstances);
      else
        this.structuralTypeInstances = null;
      var structuralTypeInstanceMembers = module.GetStructuralTypeInstanceMembers();
      if (IteratorHelper.EnumerableIsNotEmpty(structuralTypeInstanceMembers))
        this.structuralTypeInstanceMembers = new List<ITypeMemberReference>(structuralTypeInstanceMembers);
      else
        this.structuralTypeInstanceMembers = null;
      this.targetRuntimeVersion = module.TargetRuntimeVersion;
      this.trackDebugData = module.TrackDebugData;
      this.typeMemberReferences = null;
      this.typeReferences = null;
      this.usePublicKeyTokensForAssemblyReferences = module.UsePublicKeyTokensForAssemblyReferences;
      if (IteratorHelper.EnumerableIsNotEmpty(module.Win32Resources))
        this.win32Resources = new List<IWin32Resource>(module.Win32Resources);
      else
        this.win32Resources = null;
    }

    /// <summary>
    /// Gets or sets all types.
    /// </summary>
    /// <value>All types.</value>
    public List<INamedTypeDefinition> AllTypes {
      get {
        Contract.Ensures(Contract.Result<List<INamedTypeDefinition>>() != null);
        if (this.allTypes == null) this.allTypes = new List<INamedTypeDefinition>();
        return this.allTypes;
      }
      set { this.allTypes = value; }
    }
    List<INamedTypeDefinition>/*?*/ allTypes;

    /// <summary>
    /// A list of the assemblies that are referenced by this module.
    /// </summary>
    /// <value></value>
    public List<IAssemblyReference> AssemblyReferences {
      get {
        if (this.assemblyReferences == null) this.assemblyReferences = new List<IAssemblyReference>();
        return this.assemblyReferences;
      }
      set { this.assemblyReferences = value; }
    }
    List<IAssemblyReference>/*?*/ assemblyReferences;

    /// <summary>
    /// The preferred memory address at which the module is to be loaded at runtime.
    /// </summary>
    /// <value></value>
    public ulong BaseAddress {
      get { return this.baseAddress; }
      set { this.baseAddress = value; }
    }
    ulong baseAddress;

    /// <summary>
    /// The Assembly that contains this module. If this module is main module then this returns this. May be null.
    /// </summary>
    /// <value></value>
    public virtual IAssembly/*?*/ ContainingAssembly {
      get { return this.containingAssembly; }
      set { this.containingAssembly = value; }
    }
    IAssembly/*?*/ containingAssembly;

    /// <summary>
    /// A path to the debug information corresponding to this module. Can be absolute or relative to the file path of the module. Empty if not specified.
    /// </summary>
    public virtual string DebugInformationLocation {
      get { return this.debugInformationLocation; }
      set { this.debugInformationLocation = value; }
    }
    string debugInformationLocation;

    /// <summary>
    /// A hexadecimal string that is used to store and retrieve the debugging symbols from a symbol store.
    /// </summary>
    public virtual string DebugInformationVersion {
      get { return this.debugInformationVersion; }
      set { this.debugInformationVersion = value; }
    }
    string debugInformationVersion;

    /// <summary>
    /// Flags that control the behavior of the target operating system. CLI implementations are supposed to ignore this, but some operating system pay attention.
    /// </summary>
    /// <value></value>
    public virtual ushort DllCharacteristics {
      get { return this.dllCharacteristics; }
      set { this.dllCharacteristics = value; }
    }
    ushort dllCharacteristics;

    /// <summary>
    /// Calls visitor.Visit(IModule).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IModuleReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IModuleReference)this);
    }

    /// <summary>
    /// The method that will be called to start execution of this executable module.
    /// </summary>
    /// <value></value>
    public IMethodReference EntryPoint {
      get { return this.entryPoint; }
      set { this.entryPoint = value; }
    }
    IMethodReference entryPoint;

    /// <summary>
    /// The alignment of sections in the module's image file.
    /// </summary>
    /// <value></value>
    public uint FileAlignment {
      get { return this.fileAlignment; }
      set { this.fileAlignment = value; }
    }
    uint fileAlignment;

    /// <summary>
    /// A usually empty (null) collection of generic method instances that are directly or indirectly used by this module.
    /// </summary>
    public List<IGenericMethodInstanceReference>/*?*/ GenericMethodInstances {
      get { return this.genericMethodInstances; }
      set { this.genericMethodInstances = value; }
    }
    List<IGenericMethodInstanceReference>/*?*/ genericMethodInstances;

    /// <summary>
    /// True if the module contains only IL and is processor independent.
    /// </summary>
    /// <value></value>
    public bool ILOnly {
      get { return this.ilOnly; }
      set { this.ilOnly = value; }
    }
    bool ilOnly;

    /// <summary>
    /// True if the module contains a hash of its contents, encrypted with the private key of an assembly strong name.
    /// </summary>
    public bool StrongNameSigned {
      get { return this.strongNameSigned; }
      set { this.strongNameSigned = value; }
    }
    bool strongNameSigned;

    /// <summary>
    /// A usually empty (null) collection of structural type instances that are directly or indirectly used by this module.
    /// </summary>
    public List<ITypeReference>/*?*/ StructuralTypeInstances {
      get { return this.structuralTypeInstances; }
      set { this.structuralTypeInstances = value; }
    }
    List<ITypeReference>/*?*/ structuralTypeInstances;

    /// <summary>
    /// A usually empty (null) collection of structural type instance members that are directly or indirectly used by this module.
    /// </summary>
    public List<ITypeMemberReference>/*?*/ StructuralTypeInstanceMembers {
      get { return this.structuralTypeInstanceMembers; }
      set { this.structuralTypeInstanceMembers = value; }
    }
    List<ITypeMemberReference>/*?*/ structuralTypeInstanceMembers;

    /// <summary>
    /// If set, the module is platform independent but prefers to be loaded in a 32-bit process for performance reasons.
    /// </summary>
    /// <value></value>
    public bool Prefers32bits {
      get { return this.prefers32bits; }
      set { this.prefers32bits = value; }
    }
    bool prefers32bits;

    /// <summary>
    /// The kind of metadata stored in this module. For example whether this module is an executable or a manifest resource file.
    /// </summary>
    /// <value></value>
    public ModuleKind Kind {
      get { return this.kind; }
      set { this.kind = value; }
    }
    ModuleKind kind;

    /// <summary>
    /// The first part of a two part version number indicating the version of the linker that produced this module. For example, the 8 in 8.0.
    /// </summary>
    /// <value></value>
    public byte LinkerMajorVersion {
      get { return this.linkerMajorVersion; }
      set { this.linkerMajorVersion = value; }
    }
    byte linkerMajorVersion;

    /// <summary>
    /// The first part of a two part version number indicating the version of the linker that produced this module. For example, the 0 in 8.0.
    /// </summary>
    /// <value></value>
    public byte LinkerMinorVersion {
      get { return this.linkerMinorVersion; }
      set { this.linkerMinorVersion = value; }
    }
    byte linkerMinorVersion;

    /// <summary>
    /// An indication of the location where the assembly is or will be stored. This need not be a file system path and may be empty.
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to this unit.
    /// </summary>
    /// <value></value>
    public override string Location {
      get { return base.Location; }
      set { base.Location = value; this.moduleIdentity = null; }
    }

    /// <summary>
    /// The first part of a two part version number indicating the version of the format used to persist this module. For example, the 1 in 1.0.
    /// </summary>
    /// <value></value>
    public byte MetadataFormatMajorVersion {
      get { return this.metadataFormatMajorVersion; }
      set { this.metadataFormatMajorVersion = value; }
    }
    byte metadataFormatMajorVersion;

    /// <summary>
    /// The second part of a two part version number indicating the version of the format used to persist this module. For example, the 0 in 1.0.
    /// </summary>
    /// <value></value>
    public byte MetadataFormatMinorVersion {
      get { return this.metadataFormatMinorVersion; }
      set { this.metadataFormatMinorVersion = value; }
    }
    byte metadataFormatMinorVersion = 0;

    /// <summary>
    /// A list of objects representing persisted instances of types that extend System.Attribute. Provides an extensible way to associate metadata
    /// with this module. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ ModuleAttributes {
      get { return this.moduleAttributes; }
      set { this.moduleAttributes = value; }
    }
    List<ICustomAttribute>/*?*/ moduleAttributes;

    /// <summary>
    /// The name of the module.
    /// </summary>
    /// <value></value>
    public virtual IName ModuleName {
      get { return this.Name; }
      set { this.Name = value; }
    }

    /// <summary>
    /// A list of the modules that are referenced by this module. May be null.
    /// </summary>
    /// <value></value>
    public List<IModuleReference>/*?*/ ModuleReferences {
      get { return this.moduleReferences; }
      set { this.moduleReferences = value; }
    }
    List<IModuleReference>/*?*/ moduleReferences;

    /// <summary>
    /// The name of the module.
    /// </summary>
    /// <value></value>
    public override IName Name {
      get { return base.Name; }
      set { base.Name = value; this.moduleIdentity = null; }
    }

    /// <summary>
    /// A globally unique persistent identifier for this module.
    /// </summary>
    /// <value></value>
    public Guid PersistentIdentifier {
      get { return this.persistentIdentifier; }
      set { this.persistentIdentifier = value; }
    }
    Guid persistentIdentifier;

    /// <summary>
    /// Specifies the target CPU. 
    /// </summary>
    /// <value></value>
    public Machine Machine {
      get { return this.machine; }
      set { this.machine = value; }
    }
    Machine machine;


    /// <summary>
    /// If set, the module contains instructions or assumptions that are specific to the AMD 64 bit instruction set. Setting this flag to
    /// true also sets Requires64bits to true.
    /// </summary>
    /// <value></value>
    public bool RequiresAmdInstructionSet {
      get { return this.requiresAmdInstructionSet; }
      set { this.requiresAmdInstructionSet = value; }
    }
    bool requiresAmdInstructionSet;

    /// <summary>
    /// If set, the module must include a machine code stub that transfers control to the virtual execution system.
    /// </summary>
    public bool RequiresStartupStub {
      get { return this.requiresStartupStub; }
      set { this.requiresStartupStub = value; }
    }
    bool requiresStartupStub;

    /// <summary>
    /// If set, the module contains instructions that assume a 32 bit instruction set. For example it may depend on an address being 32 bits.
    /// This may be true even if the module contains only IL instructions because of PlatformInvoke and COM interop.
    /// </summary>
    /// <value></value>
    public bool Requires32bits {
      get { return this.requires32bits; }
      set { this.requires32bits = value; }
    }
    bool requires32bits;

    /// <summary>
    /// If set, the module contains instructions that assume a 64 bit instruction set. For example it may depend on an address being 64 bits.
    /// This may be true even if the module contains only IL instructions because of PlatformInvoke and COM interop.
    /// </summary>
    /// <value></value>
    public bool Requires64bits {
      get { return this.requires64bits; }
      set { this.requires64bits = value; }
    }
    bool requires64bits;

    /// <summary>
    /// The size of the virtual memory initially committed for the initial process heap.
    /// </summary>
    /// <value></value>
    public ulong SizeOfHeapCommit {
      get { return this.sizeOfHeapCommit; }
      set { this.sizeOfHeapCommit = value; }
    }
    ulong sizeOfHeapCommit;

    /// <summary>
    /// The size of the virtual memory to reserve for the initial process heap.
    /// </summary>
    /// <value></value>
    public ulong SizeOfHeapReserve {
      get { return this.sizeOfHeapReserve; }
      set { this.sizeOfHeapReserve = value; }
    }
    ulong sizeOfHeapReserve;

    /// <summary>
    /// The size of the virtual memory initially committed for the initial thread's stack.
    /// </summary>
    /// <value></value>
    public ulong SizeOfStackCommit {
      get { return this.sizeOfStackCommit; }
      set { this.sizeOfStackCommit = value; }
    }
    ulong sizeOfStackCommit;

    /// <summary>
    /// The size of the virtual memory to reserve for the initial thread's stack.
    /// </summary>
    /// <value></value>
    public ulong SizeOfStackReserve {
      get { return this.sizeOfStackReserve; }
      set { this.sizeOfStackReserve = value; }
    }
    ulong sizeOfStackReserve;

    /// <summary>
    /// Gets or sets the strings. May be null.
    /// </summary>
    /// <value>The strings.</value>
    public List<string>/*?*/ Strings {
      get { return this.strings; }
      set { this.strings = value; }
    }
    List<string>/*?*/ strings;

    /// <summary>
    /// The first part of a two part version number indicating the operating subsystem that is expected to be the target environment for this module.
    /// </summary>
    public ushort SubsystemMajorVersion {
      get { return this.subsystemMajorVersion; }
      set { this.subsystemMajorVersion = value; }
    }
    ushort subsystemMajorVersion;

    /// <summary>
    /// The second part of a two part version number indicating the operating subsystem that is expected to be the target environment for this module.
    /// </summary>
    public ushort SubsystemMinorVersion {
      get { return this.subsystemMinorVersion; }
      set { this.subsystemMinorVersion = value; }
    }
    ushort subsystemMinorVersion;

    /// <summary>
    /// Identifies the version of the CLR that is required to load this module or assembly.
    /// </summary>
    /// <value></value>
    public string TargetRuntimeVersion {
      get { return this.targetRuntimeVersion; }
      set { this.targetRuntimeVersion = value; }
    }
    string targetRuntimeVersion;

    /// <summary>
    /// True if the instructions in this module must be compiled in such a way that the debugging experience is not compromised.
    /// To set the value of this property, add an instance of System.Diagnostics.DebuggableAttribute to the MetadataAttributes list.
    /// </summary>
    /// <value></value>
    public bool TrackDebugData {
      get { return this.trackDebugData; }
      set { this.trackDebugData = value; }
    }
    bool trackDebugData;

    /// <summary>
    /// Zero or more type references used in the module. May be null. If the module is produced by reading in a CLR PE file, then this will be the contents
    /// of the type reference table. If the module is produced some other way, the method may return an empty enumeration or an enumeration that is a
    /// subset of the type references actually used in the module. May be null.
    /// </summary>
    public List<ITypeReference>/*?*/ TypeReferences {
      get { return this.typeReferences; }
      set { this.typeReferences = value; }
    }
    List<ITypeReference>/*?*/ typeReferences;

    /// <summary>
    /// Returns zero or more type member references used in the module. May be null. If the module is produced by reading in a CLR PE file, then this will be the contents
    /// of the member reference table (which only contains entries for fields and methods). If the module is produced some other way, 
    /// the method may return an empty enumeration or an enumeration that is a subset of the member references actually used in the module. May be null. 
    /// </summary>
    public List<ITypeMemberReference>/*?*/ TypeMemberReferences {
      get { return this.typeMemberReferences; }
      set { this.typeMemberReferences = value; }
    }
    List<ITypeMemberReference>/*?*/ typeMemberReferences;

    /// <summary>
    /// A list of other units that are referenced by this unit.
    /// </summary>
    /// <value></value>
    public override IEnumerable<IUnitReference> UnitReferences {
      get {
        foreach (IAssemblyReference assemblyReference in this.AssemblyReferences)
          yield return assemblyReference;
        if (this.ModuleReferences != null) {
          foreach (IModuleReference moduleReference in this.ModuleReferences)
            yield return moduleReference;
        }
      }
    }

    /// <summary>
    /// True if the module will be persisted with a list of assembly references that include only tokens derived from the public keys
    /// of the referenced assemblies, rather than with references that include the full public keys of referenced assemblies as well
    /// as hashes over the contents of the referenced assemblies. Setting this property to true is appropriate during development.
    /// When building for deployment it is safer to set this property to false.
    /// </summary>
    /// <value></value>
    public bool UsePublicKeyTokensForAssemblyReferences {
      get { return this.usePublicKeyTokensForAssemblyReferences; }
      set { this.usePublicKeyTokensForAssemblyReferences = value; }
    }
    bool usePublicKeyTokensForAssemblyReferences;

    /// <summary>
    /// A list of named byte sequences persisted with the module and used during execution, typically via the Win32 API.
    /// A module will define Win32 resources rather than "managed" resources mainly to present metadata to legacy tools
    /// and not typically use the data in its own code. May be null.
    /// </summary>
    /// <value></value>
    public List<IWin32Resource>/*?*/ Win32Resources {
      get { return this.win32Resources; }
      set { this.win32Resources = value; }
    }
    List<IWin32Resource>/*?*/ win32Resources;

    #region IModule Members

    IEnumerable<IAssemblyReference> IModule.AssemblyReferences {
      get {
        if (this.assemblyReferences == null) return Enumerable<IAssemblyReference>.Empty;
        return this.assemblyReferences.AsReadOnly();
      }
    }

    IEnumerable<string> IModule.GetStrings() {
      if (this.Strings == null) return Enumerable<string>.Empty;
      return this.Strings.AsReadOnly();
    }

    IEnumerable<INamedTypeDefinition> IModule.GetAllTypes() {
      if (this.allTypes == null) return Enumerable<INamedTypeDefinition>.Empty;
      return this.allTypes.AsReadOnly();
    }

    IEnumerable<IGenericMethodInstanceReference> IModule.GetGenericMethodInstances() {
      if (this.genericMethodInstances == null) return Enumerable<IGenericMethodInstanceReference>.Empty;
      return this.genericMethodInstances.AsReadOnly();
    }

    IEnumerable<ITypeReference> IModule.GetStructuralTypeInstances() {
      if (this.structuralTypeInstances == null) return Enumerable<ITypeReference>.Empty;
      return this.structuralTypeInstances.AsReadOnly();
    }

    IEnumerable<ITypeMemberReference> IModule.GetStructuralTypeInstanceMembers() {
      if (this.structuralTypeInstanceMembers == null) return Enumerable<ITypeMemberReference>.Empty;
      return this.structuralTypeInstanceMembers.AsReadOnly();
    }

    IEnumerable<ITypeReference> IModule.GetTypeReferences() {
      if (this.typeReferences == null) return Enumerable<ITypeReference>.Empty;
      return this.typeReferences.AsReadOnly();
    }

    IEnumerable<ITypeMemberReference> IModule.GetTypeMemberReferences() {
      if (this.typeMemberReferences == null) return Enumerable<ITypeMemberReference>.Empty;
      return this.typeMemberReferences.AsReadOnly();
    }

    IEnumerable<ICustomAttribute> IModule.ModuleAttributes {
      get {
        if (this.ModuleAttributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.ModuleAttributes.AsReadOnly();
      }
    }

    IEnumerable<IModuleReference> IModule.ModuleReferences {
      get {
        if (this.ModuleReferences == null) return Enumerable<IModuleReference>.Empty;
        return this.ModuleReferences.AsReadOnly();
      }
    }

    IEnumerable<IWin32Resource> IModule.Win32Resources {
      get {
        if (this.Win32Resources == null) return Enumerable<IWin32Resource>.Empty;
        return this.Win32Resources.AsReadOnly();
      }
    }

    #endregion

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public override UnitIdentity UnitIdentity {
      get {
        return this.ModuleIdentity;
      }
    }

    #region IModuleReference Members

    /// <summary>
    /// The identity of the referenced module.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public virtual ModuleIdentity ModuleIdentity {
      get {
        if (this.moduleIdentity == null) {
          this.moduleIdentity = UnitHelper.GetModuleIdentity(this);
        }
        return this.moduleIdentity;
      }
    }
    ModuleIdentity/*?*/ moduleIdentity;

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return this.ContainingAssembly; }
    }

    /// <summary>
    /// The referenced module, or Dummy.Module if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public IModule ResolvedModule {
      get { return this; }
    }

    #endregion

    /// <summary>
    /// The referenced unit, or Dummy.Unit if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public override IUnit ResolvedUnit {
      get { return this; }
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public class ModuleReference : UnitReference, IModuleReference, ICopyFrom<IModuleReference> {

    /// <summary>
    /// 
    /// </summary>
    public ModuleReference() {
      this.containingAssembly = Dummy.Assembly;
      this.moduleIdentity = Dummy.ModuleReference.ModuleIdentity;
      this.resolvedModule = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moduleReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IModuleReference moduleReference, IInternFactory internFactory) {
      ((ICopyFrom<IUnitReference>)this).Copy(moduleReference, internFactory);
      var mutableModuleReference = moduleReference as ModuleReference;
      if (mutableModuleReference != null) {
        this.host = mutableModuleReference.Host;
        this.referringUnit = mutableModuleReference.ReferringUnit;
      }
      this.containingAssembly = moduleReference.ContainingAssembly;
      this.moduleIdentity = moduleReference.ModuleIdentity;
      this.resolvedModule = null;
    }

    /// <summary>
    /// The Assembly that contains this module. May be null if the module is not part of an assembly.
    /// </summary>
    /// <value></value>
    public IAssemblyReference ContainingAssembly {
      get { return this.containingAssembly; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.containingAssembly = value;
      }
    }
    IAssemblyReference containingAssembly;

    /// <summary>
    /// Calls vistor.Visit(IModuleReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// An object representing the application that is hosting the object model that this reference forms a part of. May be null.
    /// </summary>
    public IMetadataHost/*?*/ Host {
      get { return this.host; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.host = value;
      }
    }
    IMetadataHost host;

    /// <summary>
    /// The identity of the referenced module.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public ModuleIdentity ModuleIdentity {
      get {
        if (this.moduleIdentity == null) {
          this.isFrozen = true;
          this.moduleIdentity = new ModuleIdentity(this.Name, "");
        }
        return this.moduleIdentity;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        this.moduleIdentity = value;
        base.Name = value.Name;
      }
    }
    ModuleIdentity moduleIdentity;

    /// <summary>
    /// The unit that is the root of a graph that contains this reference instance. This is not the same as ContainingAssembly, which
    /// is a reference to the assembly that contains the module which is referenced by this instance. May be null.
    /// </summary>
    public IUnit/*?*/ ReferringUnit {
      get { return this.referringUnit; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.referringUnit = value;
      }
    }
    IUnit/*?*/ referringUnit;

    /// <summary>
    /// Tries to resolves the reference with the aid of this.Host. If resolution fails, the result is Dummy.Module.
    /// </summary>
    protected virtual IModule Resolve() {
      this.isFrozen = true;
      if (this.Host == null) return Dummy.Module;
      var identity = this.ModuleIdentity;
      var result = this.Host.FindModule(identity);
      if (!(result is Dummy)) return result;
      if (identity.Location == null && this.ReferringUnit != null)
        identity = this.Host.ProbeModuleReference(this.ReferringUnit, identity);
      return this.Host.LoadModule(identity);
    }

    /// <summary>
    /// The referenced module, or Dummy.Module if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public IModule ResolvedModule {
      get {
        if (this.resolvedModule == null)
          this.resolvedModule = this.Resolve();
        return this.resolvedModule;
      }
      set {
        this.resolvedModule = value;
      }
    }
    IModule/*?*/ resolvedModule;

    /// <summary>
    /// The referenced unit, or Dummy.Unit if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public override IUnit ResolvedUnit {
      get { return this.ResolvedModule; }
    }

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public override UnitIdentity UnitIdentity {
      get { return this.ModuleIdentity; }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class Unit : UnitReference, IUnit, ICopyFrom<IUnit> {

    /// <summary>
    /// 
    /// </summary>
    internal Unit() {
      this.contractAssemblySymbolicIdentity = Dummy.Assembly.AssemblyIdentity;
      this.coreAssemblySymbolicIdentity = Dummy.Assembly.AssemblyIdentity;
      this.location = "";
      this.platformType = Dummy.PlatformType;
      this.unitNamespaceRoot = Dummy.RootUnitNamespace;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="internFactory"></param>
    public void Copy(IUnit unit, IInternFactory internFactory) {
      ((ICopyFrom<IUnitReference>)this).Copy(unit, internFactory);
      this.contractAssemblySymbolicIdentity = unit.ContractAssemblySymbolicIdentity;
      this.coreAssemblySymbolicIdentity = unit.CoreAssemblySymbolicIdentity;
      this.location = unit.Location;
      if (IteratorHelper.EnumerableIsNotEmpty(unit.UninterpretedSections))
        this.uninterpretedSections = new List<IPESection>(unit.UninterpretedSections);
      else
        this.uninterpretedSections = null;
      this.platformType = unit.PlatformType;
      this.unitNamespaceRoot = unit.UnitNamespaceRoot;
    }

    /// <summary>
    /// The identity of the assembly corresponding to the target platform contract assembly at the time this unit was compiled.
    /// This property will be used to implement IMetadataHost.ContractAssemblySymbolicIdentity and its implementation must
    /// consequently not use the latter.
    /// </summary>
    /// <value></value>
    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { return this.contractAssemblySymbolicIdentity; }
      set { this.contractAssemblySymbolicIdentity = value; }
    }
    AssemblyIdentity contractAssemblySymbolicIdentity;

    /// <summary>
    /// The identity of the assembly corresponding to the target platform core assembly at the time this unit was compiled.
    /// This property will be used to implement IMetadataHost.CoreAssemblySymbolicIdentity and its implementation must
    /// consequently not use the latter.
    /// </summary>
    /// <value></value>
    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { return this.coreAssemblySymbolicIdentity; }
      set { this.coreAssemblySymbolicIdentity = value; }
    }
    AssemblyIdentity coreAssemblySymbolicIdentity;

    /// <summary>
    /// An indication of the location where the unit is or will be stored. This need not be a file system path and may be empty.
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to this unit.
    /// </summary>
    /// <value></value>
    public virtual string Location {
      get { return this.location; }
      set { this.location = value; }
    }
    string location;

    /// <summary>
    /// A sequence of PE sections that are not well known to PE readers and thus have not been decompiled into 
    /// other parts of the Metadata Model. These sections may have meaning to other tools.  May be null.
    /// </summary>
    public virtual List<IPESection>/*?*/ UninterpretedSections {
      get { return this.uninterpretedSections; }
      set { this.uninterpretedSections = value; }
    }
    List<IPESection>/*?*/ uninterpretedSections;

    /// <summary>
    /// A collection of well known types that must be part of every target platform and that are fundamental to modeling compiled code.
    /// The types are obtained by querying the unit set of the compilation and thus can include types that are defined by the compilation itself.
    /// </summary>
    /// <value></value>
    public IPlatformType PlatformType {
      get { return this.platformType; }
      set { this.platformType = value; }
    }
    IPlatformType platformType;

    /// <summary>
    /// A root namespace that contains nested namespaces as well as top level types and anything else that implements INamespaceMember.
    /// </summary>
    /// <value></value>
    public IRootUnitNamespace UnitNamespaceRoot {
      get { return this.unitNamespaceRoot; }
      set { this.unitNamespaceRoot = value; }
    }
    IRootUnitNamespace unitNamespaceRoot;

    /// <summary>
    /// A list of other units that are referenced by this unit.
    /// </summary>
    /// <value></value>
    public abstract IEnumerable<IUnitReference> UnitReferences {
      get;
    }

    #region IUnit Members

    IEnumerable<IPESection> IUnit.UninterpretedSections {
      get {
        if (this.UninterpretedSections == null)
          return Enumerable<IPESection>.Empty;
        else
          return this.UninterpretedSections.AsReadOnly();
      }
    }

    #endregion
    #region INamespaceRootOwner Members

    /// <summary>
    /// The associated root namespace.
    /// </summary>
    /// <value></value>
    public INamespaceDefinition NamespaceRoot {
      get { return this.UnitNamespaceRoot; }
    }

    #endregion

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class UnitReference : IUnitReference, ICopyFrom<IUnitReference> {

    /// <summary>
    /// 
    /// </summary>
    internal UnitReference() {
      this.attributes = null;
      this.locations = null;
      this.name = Dummy.Name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IUnitReference unitReference, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(unitReference.Attributes))
        this.attributes = new List<ICustomAttribute>(unitReference.Attributes);
      else
        this.attributes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(unitReference.Locations))
        this.locations = new List<ILocation>(unitReference.Locations);
      else
        this.locations = null;
      this.name = unitReference.Name;
    }

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ Attributes {
      get { return this.attributes; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.attributes = value;
      }
    }
    List<ICustomAttribute>/*?*/ attributes;

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference. The dispatch method does nothing else.
    /// </summary>
    /// <param name="visitor"></param>
    public virtual void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference, which is not derived from IDefinition. For example an object implemeting IArrayType will
    /// call visitor.Visit(IArrayTypeReference) and not visitor.Visit(IArrayType).
    /// The dispatch method does nothing else.
    /// </summary>
    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    /// <summary>
    /// True if the reference has been frozen and can no longer be modified. A reference becomes frozen
    /// as soon as it is resolved or interned. An unfrozen reference can also explicitly be set to be frozen.
    /// It is recommended that any code constructing a type reference freezes it immediately after construction is complete.
    /// </summary>
    public bool IsFrozen {
      get { return this.isFrozen; }
      set {
        Contract.Requires(!this.IsFrozen && value);
        this.isFrozen = value;
      }
    }
    /// <summary>
    /// True if the reference has been frozen and can no longer be modified. A reference becomes frozen
    /// as soon as it is resolved or interned. An unfrozen reference can also explicitly be set to be frozen.
    /// It is recommended that any code constructing a type reference freezes it immediately after construction is complete.
    /// </summary>
    protected bool isFrozen;

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance. May be null.
    /// </summary>
    /// <value></value>
    public List<ILocation>/*?*/ Locations {
      get { return this.locations; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.locations = value;
      }
    }
    List<ILocation>/*?*/ locations;

    /// <summary>
    /// The name of the referenced unit.
    /// </summary>
    /// <value></value>
    public virtual IName Name {
      get { return this.name; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.name = value;
      }
    }
    IName name;

    /// <summary>
    /// The referenced unit, or Dummy.Unit if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public abstract IUnit ResolvedUnit {
      get;
    }

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    /// <value></value>
    /// <remarks>The location might not be set.</remarks>
    public abstract UnitIdentity UnitIdentity {
      get;
    }

    #region IReference Members

    IEnumerable<ICustomAttribute> IReference.Attributes {
      get {
        if (this.Attributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.Attributes.AsReadOnly();
      }
    }

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      get {
        if (this.Locations == null) return Enumerable<ILocation>.Empty;
        return this.Locations.AsReadOnly();
      }
    }

    #endregion
  }

}
