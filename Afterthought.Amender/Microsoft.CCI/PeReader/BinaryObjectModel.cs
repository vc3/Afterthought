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
using Microsoft.Cci.MetadataReader.PEFileFlags;
using Microsoft.Cci.UtilityDataStructures;
using Microsoft.Cci.MetadataReader.PEFile;

namespace Microsoft.Cci.MetadataReader.ObjectModelImplementation {

  #region Base Objects for Object Model

  internal interface IMetadataReaderModuleReference : IModuleReference {
    uint InternedModuleId { get; }
  }

  /// <summary>
  /// Represents a metadata entity. This has an associated Token Value...
  /// This is used in maintaining type spec cache.
  /// </summary>
  internal abstract class MetadataObject : IReference, IMetadataObjectWithToken, ITokenDecoder {

    internal PEFileToObjectModel PEFileToObjectModel;

    protected MetadataObject(PEFileToObjectModel peFileToObjectModel) {
      this.PEFileToObjectModel = peFileToObjectModel;
    }

    internal abstract uint TokenValue { get; }

    public IPlatformType PlatformType {
      get { return this.PEFileToObjectModel.PlatformType; }
    }

    #region IReference Members

    public virtual IEnumerable<ICustomAttribute> Attributes {
      get {
        if (this.attributes == null)
          this.attributes = this.GetAttributes();
        return this.attributes;
      }
    }
    IEnumerable<ICustomAttribute>/*?*/ attributes;

    protected virtual IEnumerable<ICustomAttribute> GetAttributes() {
      uint customAttributeRowIdStart;
      uint customAttributeRowIdEnd;
      this.PEFileToObjectModel.GetCustomAttributeInfo(this, out customAttributeRowIdStart, out customAttributeRowIdEnd);
      if (customAttributeRowIdStart == customAttributeRowIdEnd) return Enumerable<ICustomAttribute>.Empty;
      uint count = customAttributeRowIdEnd - customAttributeRowIdStart;
      ICustomAttribute[] attributes = new ICustomAttribute[count];
      for (uint i = 0; i < count; i++)
        attributes[i] = this.PEFileToObjectModel.GetCustomAttributeAtRow(this, this.TokenValue, customAttributeRowIdStart+i);
      return IteratorHelper.GetReadonly(attributes);
    }

    public abstract void Dispatch(IMetadataVisitor visitor);

    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    public virtual IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IMetadataObjectWithToken Members

    uint IMetadataObjectWithToken.TokenValue {
      get { return this.TokenValue; }
    }

    #endregion

    #region ITokenDecoder Members

    public object GetObjectForToken(uint token) {
      return this.PEFileToObjectModel.GetReferenceForToken(this, token);
    }

    #endregion

  }

  /// <summary>
  /// Base class of Namespaces/Types/TypeMembers.
  /// </summary>
  internal abstract class MetadataDefinitionObject : MetadataObject, IDefinition {

    protected MetadataDefinitionObject(PEFileToObjectModel peFileToObjectModel)
      : base(peFileToObjectModel) {
      this.locations = IteratorHelper.GetSingletonEnumerable<ILocation>(new MetadataLocation(peFileToObjectModel.document, this));
    }

    public override IEnumerable<ILocation> Locations {
      get {
        return this.locations;
      }
    }
    IEnumerable<ILocation> locations;

  }

  /// <summary>
  /// 
  /// </summary>
  internal sealed class MetadataObjectDocument : IDocument {

    internal MetadataObjectDocument(PEFileToObjectModel peFileToObjectModel) {
      this.peFileToObjectModel = peFileToObjectModel;
    }

    PEFileToObjectModel peFileToObjectModel;

    /// <summary>
    /// The location where this document was found, or where it should be stored.
    /// This will also uniquely identify the source document within an instance of compilation host.
    /// </summary>
    public string Location {
      get { return this.peFileToObjectModel.Module.ModuleIdentity.Location; }
    }

    /// <summary>
    /// The name of the document. For example the name of the file if the document corresponds to a file.
    /// </summary>
    public IName Name {
      get { return this.peFileToObjectModel.Module.ModuleIdentity.Name; }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  internal sealed class MetadataLocation : IMetadataLocation {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="document"></param>
    /// <param name="definition"></param>
    internal MetadataLocation(IDocument document, IMetadataObjectWithToken definition) {
      this.document = document;
      this.definition = definition;
    }

    #region IMetadataLocation Members

    /// <summary>
    /// The metadata object whose definition contains this location.
    /// </summary>
    public IMetadataObjectWithToken Definition {
      get { return this.definition; }
    }
    IMetadataObjectWithToken definition;

    #endregion

    #region ILocation Members

    /// <summary>
    /// The document containing this location.
    /// </summary>
    public IDocument Document {
      get { return this.document; }
    }
    IDocument document;

    #endregion
  }

  internal enum ContainerState : byte {
    Initialized,
    StartedLoading,
    Loaded,
  }

  /// <summary>
  /// Contains generic implementation of being a container as well as a scope.
  /// </summary>
  /// <typeparam name="InternalMemberType">The type of actual objects that are stored</typeparam>
  /// <typeparam name="ExternalMemberType">The type of objects as they are exposed outside</typeparam>
  /// <typeparam name="ExternalContainerType">Externally visible container type</typeparam>
  internal abstract class ScopedContainerMetadataObject<InternalMemberType, ExternalMemberType, ExternalContainerType> : MetadataDefinitionObject, IContainer<ExternalMemberType>, IScope<ExternalMemberType>
    where InternalMemberType : class, ExternalMemberType
    where ExternalMemberType : class, IScopeMember<IScope<ExternalMemberType>>, IContainerMember<ExternalContainerType> {

    MultiHashtable<InternalMemberType>/*?*/  caseSensitiveMemberHashTable;
    MultiHashtable<InternalMemberType>/*?*/ caseInsensitiveMemberHashTable;
    //^ [SpecPublic]
    protected ContainerState ContainerState;
    //^ invariant this.ContainerState != ContainerState.Initialized ==> this.caseSensitiveMemberHashTable != null;
    //^ invariant this.ContainerState != ContainerState.Initialized ==> this.caseInsensitiveMemberHashTable != null;

    protected ScopedContainerMetadataObject(
      PEFileToObjectModel peFileToObjectModel
    )
      : base(peFileToObjectModel) {
      this.ContainerState = ContainerState.Initialized;
    }

    internal void StartLoadingMembers()
      //^ ensures this.ContainerState == ContainerState.StartedLoading;
    {
      if (this.ContainerState == ContainerState.Initialized) {
        this.caseSensitiveMemberHashTable = new MultiHashtable<InternalMemberType>();
        this.caseInsensitiveMemberHashTable = new MultiHashtable<InternalMemberType>();
        this.ContainerState = ContainerState.StartedLoading;
      }
    }

    internal void AddMember(InternalMemberType/*!*/ member)
      //^ requires this.ContainerState != ContainerState.Loaded;
    {
      Debug.Assert(this.ContainerState != ContainerState.Loaded);
      if (this.ContainerState == ContainerState.Initialized)
        this.StartLoadingMembers();
      //^ assert this.caseSensitiveMemberHashTable != null;
      //^ assert this.caseInsensitiveMemberHashTable != null;
      IName name = ((IContainerMember<ExternalContainerType>)member).Name;
      this.caseSensitiveMemberHashTable.Add((uint)name.UniqueKey, member);
      this.caseInsensitiveMemberHashTable.Add((uint)name.UniqueKeyIgnoringCase, member);
    }

    protected void DoneLoadingMembers()
      //^ requires this.ContainerState == ContainerState.StartedLoading;
      //^ ensures this.ContainerState == ContainerState.Loaded;
    {
      Debug.Assert(this.ContainerState == ContainerState.StartedLoading);
      this.ContainerState = ContainerState.Loaded;
      //^ assert this.caseSensitiveMemberHashTable != null;
      //^ assert this.caseInsensitiveMemberHashTable != null;
    }

    internal abstract void LoadMembers()
      //^ requires this.ContainerState == ContainerState.StartedLoading;
      //^ ensures this.ContainerState == ContainerState.Loaded;
      ;

    internal MultiHashtable<InternalMemberType>.ValuesEnumerable InternalMembers {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        //^ assert this.caseSensitiveMemberHashTable != null;
        return this.caseSensitiveMemberHashTable.Values;
      }
    }

    #region IContainer<ExternalMemberType> Members

    [Pure]
    public bool Contains(ExternalMemberType/*!*/ member) {
      if (this.ContainerState != ContainerState.Loaded) {
        this.LoadMembers();
      }
      //^ assert this.caseSensitiveMemberHashTable != null;
      InternalMemberType/*?*/ internalMember = member as InternalMemberType;
      if (internalMember == null)
        return false;
      return this.caseSensitiveMemberHashTable.Contains((uint)member.Name.UniqueKey, internalMember);
    }

    #endregion

    #region IScope<ExternalMemberType> Members

    [Pure]
    public IEnumerable<ExternalMemberType> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ExternalMemberType, bool> predicate) {
      if (this.ContainerState != ContainerState.Loaded) {
        this.LoadMembers();
      }
      int key = ignoreCase ? name.UniqueKeyIgnoringCase : name.UniqueKey;
      //^ assert this.caseSensitiveMemberHashTable != null;
      //^ assert this.caseInsensitiveMemberHashTable != null;
      MultiHashtable<InternalMemberType> hashTable = ignoreCase ? this.caseInsensitiveMemberHashTable : this.caseSensitiveMemberHashTable;
      foreach (ExternalMemberType member in hashTable.GetValuesFor((uint)key)) {
        if (predicate(member))
          yield return member;
      }
    }

    [Pure]
    public IEnumerable<ExternalMemberType> GetMatchingMembers(Function<ExternalMemberType, bool> predicate) {
      if (this.ContainerState != ContainerState.Loaded) {
        this.LoadMembers();
      }
      //^ assert this.caseSensitiveMemberHashTable != null;
      foreach (ExternalMemberType member in this.caseSensitiveMemberHashTable.Values)
        if (predicate(member))
          yield return member;
    }

    [Pure]
    public IEnumerable<ExternalMemberType> GetMembersNamed(IName name, bool ignoreCase) {
      if (this.ContainerState != ContainerState.Loaded) {
        this.LoadMembers();
      }
      int key = ignoreCase ? name.UniqueKeyIgnoringCase : name.UniqueKey;
      //^ assert this.caseSensitiveMemberHashTable != null;
      //^ assert this.caseInsensitiveMemberHashTable != null;
      MultiHashtable<InternalMemberType> hashTable = ignoreCase ? this.caseInsensitiveMemberHashTable : this.caseSensitiveMemberHashTable;
      foreach (ExternalMemberType member in hashTable.GetValuesFor((uint)key)) {
        yield return member;
      }
    }

    public IEnumerable<ExternalMemberType> Members {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        //^ assert this.caseSensitiveMemberHashTable != null;
        foreach (ExternalMemberType member in this.caseSensitiveMemberHashTable.Values)
          yield return member;
      }
    }

    #endregion
  }
  #endregion Base Objects for Object Model


  #region Assembly/Module Level Object Model

  internal class Module : MetadataObject, IModule, IMetadataReaderModuleReference {
    internal readonly IName ModuleName;
    readonly COR20Flags Cor20Flags;
    internal readonly uint InternedModuleId;
    internal readonly ModuleIdentity ModuleIdentity;
    IMethodReference/*?*/ entryPointMethodReference;

    internal Module(
      PEFileToObjectModel peFileToObjectModel,
      IName moduleName,
      COR20Flags cor20Flags,
      uint internedModuleId,
      ModuleIdentity moduleIdentity
    )
      : base(peFileToObjectModel) {
      this.ModuleName = moduleName;
      this.Cor20Flags = cor20Flags;
      this.InternedModuleId = internedModuleId;
      this.ModuleIdentity = moduleIdentity;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IModuleReference)this);
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.Module | (uint)0x00000001; }
    }

    public override string ToString() {
      return this.ModuleIdentity.ToString();
    }

    #region IModule Members

    ulong IModule.BaseAddress {
      get {
        return this.PEFileToObjectModel.PEFileReader.ImageBase;
      }
    }

    IAssembly/*?*/ IModule.ContainingAssembly {
      get {
        return this.PEFileToObjectModel.ContainingAssembly;
      }
    }

    IEnumerable<IAssemblyReference> IModule.AssemblyReferences {
      get {
        return this.PEFileToObjectModel.GetAssemblyReferences();
      }
    }

    string IModule.DebugInformationLocation {
      get {
        return this.PEFileToObjectModel.GetDebugInformationLocation();
      }
    }

    string IModule.DebugInformationVersion {
      get {
        return this.PEFileToObjectModel.GetDebugInformationVersion();
      }
    }

    ushort IModule.DllCharacteristics {
      get { return (ushort)this.PEFileToObjectModel.GetDllCharacteristics(); }
    }

    IMethodReference IModule.EntryPoint {
      get {
        if (this.entryPointMethodReference == null) {
          this.entryPointMethodReference = this.PEFileToObjectModel.GetEntryPointMethod();
        }
        return this.entryPointMethodReference;
      }
    }

    uint IModule.FileAlignment {
      get { return this.PEFileToObjectModel.PEFileReader.FileAlignment; }
    }

    bool IModule.ILOnly {
      get { return (this.Cor20Flags & COR20Flags.ILOnly) == COR20Flags.ILOnly; }
    }

    bool IModule.StrongNameSigned {
      get { return (this.Cor20Flags & COR20Flags.StrongNameSigned) == COR20Flags.StrongNameSigned; }
    }

    bool IModule.Prefers32bits {
      get { return (this.Cor20Flags & (COR20Flags.Bit32Required|COR20Flags.Prefers32bits)) == (COR20Flags.Bit32Required|COR20Flags.Prefers32bits); }
    }

    ModuleKind IModule.Kind {
      get { return this.PEFileToObjectModel.ModuleKind; }
    }

    byte IModule.LinkerMajorVersion {
      get { return this.PEFileToObjectModel.PEFileReader.LinkerMajorVersion; }
    }

    byte IModule.LinkerMinorVersion {
      get { return this.PEFileToObjectModel.PEFileReader.LinkerMinorVersion; }
    }

    byte IModule.MetadataFormatMajorVersion {
      get { return this.PEFileToObjectModel.MetadataFormatMajorVersion; }
    }

    byte IModule.MetadataFormatMinorVersion {
      get { return this.PEFileToObjectModel.MetadataFormatMinorVersion; }
    }

    IName IModule.ModuleName {
      get { return this.ModuleName; }
    }

    IEnumerable<IModuleReference> IModule.ModuleReferences {
      get { return this.PEFileToObjectModel.GetModuleReferences(); }
    }

    Guid IModule.PersistentIdentifier {
      get { return this.PEFileToObjectModel.ModuleGuidIdentifier; }
    }

    Machine IModule.Machine {
      get { return this.PEFileToObjectModel.Machine; }
    }

    bool IModule.RequiresAmdInstructionSet {
      get { return this.PEFileToObjectModel.RequiresAmdInstructionSet; }
    }

    bool IModule.RequiresStartupStub {
      get { return this.PEFileToObjectModel.RequiresStartupStub; }
    }

    bool IModule.Requires32bits {
      get { return (this.Cor20Flags & (COR20Flags.Bit32Required|COR20Flags.Prefers32bits)) == COR20Flags.Bit32Required; }
    }

    bool IModule.Requires64bits {
      get { return this.PEFileToObjectModel.Requires64Bits; }
    }

    ulong IModule.SizeOfHeapCommit {
      get { return this.PEFileToObjectModel.PEFileReader.SizeOfHeapCommit; }
    }

    ulong IModule.SizeOfHeapReserve {
      get { return this.PEFileToObjectModel.PEFileReader.SizeOfHeapReserve; }
    }

    ulong IModule.SizeOfStackCommit {
      get { return this.PEFileToObjectModel.PEFileReader.SizeOfStackCommit; }
    }

    ulong IModule.SizeOfStackReserve {
      get { return this.PEFileToObjectModel.PEFileReader.SizeOfStackReserve; }
    }

    ushort IModule.SubsystemMajorVersion {
      get { return this.PEFileToObjectModel.SubsystemMajorVersion; }
    }

    ushort IModule.SubsystemMinorVersion {
      get { return this.PEFileToObjectModel.SubsystemMinorVersion; }
    }

    string IModule.TargetRuntimeVersion {
      get { return this.PEFileToObjectModel.TargetRuntimeVersion; }
    }

    bool IModule.TrackDebugData {
      get { return (this.Cor20Flags & COR20Flags.TrackDebugData) == COR20Flags.TrackDebugData; }
    }

    bool IModule.UsePublicKeyTokensForAssemblyReferences {
      get { return true; }
    }

    IEnumerable<IWin32Resource> IModule.Win32Resources {
      get {
        return this.PEFileToObjectModel.GetWin32Resources();
      }
    }

    IEnumerable<ICustomAttribute> IModule.ModuleAttributes {
      get {
        return this.PEFileToObjectModel.GetModuleCustomAttributes();
      }
    }

    IEnumerable<string> IModule.GetStrings() {
      return this.PEFileToObjectModel.PEFileReader.UserStringStream.GetStrings();
    }

    IEnumerable<INamedTypeDefinition> IModule.GetAllTypes() {
      return this.PEFileToObjectModel.GetAllTypes();
    }

    IEnumerable<IGenericMethodInstanceReference> IModule.GetGenericMethodInstances() {
      return Enumerable<IGenericMethodInstanceReference>.Empty;
    }

    IEnumerable<ITypeReference> IModule.GetStructuralTypeInstances() {
      return Enumerable<ITypeReference>.Empty;
    }

    IEnumerable<ITypeMemberReference> IModule.GetStructuralTypeInstanceMembers() {
      return Enumerable<ITypeMemberReference>.Empty;
    }

    IEnumerable<ITypeMemberReference> IModule.GetTypeMemberReferences() {
      return this.PEFileToObjectModel.GetMemberReferences();
    }

    IEnumerable<ITypeReference> IModule.GetTypeReferences() {
      return this.PEFileToObjectModel.GetTypeReferences();
    }

    #endregion

    #region IUnit Members


    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { return this.PEFileToObjectModel.ContractAssemblySymbolicIdentity; }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { return this.PEFileToObjectModel.CoreAssemblySymbolicIdentity; }
    }

    IPlatformType IUnit.PlatformType {
      get { return this.PEFileToObjectModel.PlatformType; }
    }

    string IUnit.Location {
      get { return this.ModuleIdentity.Location; }
    }

    IEnumerable<IPESection> IUnit.UninterpretedSections {
      get { return this.PEFileToObjectModel.GetUninterpretedPESections(); }
    }

    IRootUnitNamespace IUnit.UnitNamespaceRoot {
      get {
        return this.PEFileToObjectModel.RootModuleNamespace;
      }
    }

    IEnumerable<IUnitReference> IUnit.UnitReferences {
      get {
        foreach (IUnitReference ur in this.PEFileToObjectModel.GetAssemblyReferences()) {
          yield return ur;
        }
        foreach (IUnitReference ur in this.PEFileToObjectModel.GetModuleReferences()) {
          yield return ur;
        }
      }
    }

    #endregion

    #region INamespaceRootOwner Members

    INamespaceDefinition INamespaceRootOwner.NamespaceRoot {
      get {
        return this.PEFileToObjectModel.RootModuleNamespace;
      }
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.ModuleName; }
    }

    #endregion

    #region IModuleReference Members

    ModuleIdentity IModuleReference.ModuleIdentity {
      get {
        return this.ModuleIdentity;
      }
    }

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get {
        return this.PEFileToObjectModel.ContainingAssembly;
      }
    }

    IModule IModuleReference.ResolvedModule {
      get { return this; }
    }

    #endregion

    #region IUnitReference Members

    public UnitIdentity UnitIdentity {
      get {
        return this.ModuleIdentity;
      }
    }

    public IUnit ResolvedUnit {
      get { return this; }
    }

    #endregion

    #region IMetadataReaderModuleReference Members

    uint IMetadataReaderModuleReference.InternedModuleId {
      get { return this.InternedModuleId; }
    }

    #endregion
  }

  internal sealed class Assembly : Module, IAssembly, IMetadataReaderModuleReference {
    readonly IName AssemblyName;
    readonly AssemblyFlags AssemblyFlags;
    readonly byte[] publicKey;
    internal readonly AssemblyIdentity AssemblyIdentity;
    internal IModule[]/*?*/ MemberModules;

    internal Assembly(
      PEFileToObjectModel peFileToObjectModel,
      IName moduleName,
      COR20Flags corFlags,
      uint internedModuleId,
      AssemblyIdentity assemblyIdentity,
      IName assemblyName,
      AssemblyFlags assemblyFlags,
      byte[] publicKey
    )
      : base(peFileToObjectModel, moduleName, corFlags, internedModuleId, assemblyIdentity)
      //^ requires peFileToObjectModel.PEFileReader.IsAssembly;
    {
      this.AssemblyName = assemblyName;
      this.AssemblyFlags = assemblyFlags;
      this.publicKey= publicKey;
      this.AssemblyIdentity = assemblyIdentity;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IAssemblyReference)this);
    }

    public bool IsRetargetable {
      get { return (this.AssemblyFlags & AssemblyFlags.Retargetable) != 0; }
    }

    public bool ContainsForeignTypes {
      get { return (this.AssemblyFlags & AssemblyFlags.ContainsForeignTypes) != 0; }
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.Assembly | (uint)0x00000001; }
    }

    internal IModule/*?*/ FindMemberModuleNamed(IName moduleName) {
      IModule[]/*?*/ memberModuleArray = this.MemberModules;
      if (memberModuleArray == null) return null;
      for (int i = 0, n = memberModuleArray.Length; i < n; i++) {
        if (memberModuleArray[i].ModuleName.UniqueKeyIgnoringCase != moduleName.UniqueKeyIgnoringCase) continue;
        return memberModuleArray[i];
      }
      return null;
    }

    internal void SetMemberModules(IModule[] memberModules) {
      this.MemberModules = memberModules;
    }

    public override string ToString() {
      return this.AssemblyIdentity.ToString();
    }

    #region IAssembly Members

    IEnumerable<IAliasForType> IAssembly.ExportedTypes {
      get { return this.PEFileToObjectModel.GetEnumberableForExportedTypes(); }
    }

    public IEnumerable<byte> HashValue {
      get { return new EnumerableMemoryBlockWrapper(this.PEFileToObjectModel.PEFileReader.StrongNameSignature); }
    }

    IEnumerable<IResourceReference> IAssembly.Resources {
      get {
        return this.PEFileToObjectModel.GetResources();
      }
    }

    IEnumerable<IFileReference> IAssembly.Files {
      get {
        return this.PEFileToObjectModel.GetFiles();
      }
    }

    IEnumerable<IModule> IAssembly.MemberModules {
      get { return IteratorHelper.GetReadonly(this.MemberModules)??Enumerable<IModule>.Empty; }
    }

    IEnumerable<ISecurityAttribute> IAssembly.SecurityAttributes {
      get {
        uint secAttributeRowIdStart;
        uint secAttributeRowIdEnd;
        this.PEFileToObjectModel.GetSecurityAttributeInfo(this, out secAttributeRowIdStart, out secAttributeRowIdEnd);
        for (uint secAttributeIter = secAttributeRowIdStart; secAttributeIter < secAttributeRowIdEnd; ++secAttributeIter) {
          yield return this.PEFileToObjectModel.GetSecurityAttributeAtRow(this, secAttributeIter);
        }
      }
    }

    uint IAssembly.Flags {
      get { return (uint)this.AssemblyFlags; }
    }

    IEnumerable<byte> IAssemblyReference.PublicKey {
      get {
        return IteratorHelper.GetReadonly(this.publicKey);
      }
    }

    IEnumerable<ICustomAttribute> IAssembly.AssemblyAttributes {
      get {
        return this.PEFileToObjectModel.GetAssemblyCustomAttributes();
      }
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get {
        return this.AssemblyName;
      }
    }

    #endregion

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return this; }
    }

    #endregion

    #region IAssemblyReference Members

    AssemblyIdentity IAssemblyReference.AssemblyIdentity {
      get {
        return this.AssemblyIdentity;
      }
    }

    AssemblyIdentity IAssemblyReference.UnifiedAssemblyIdentity {
      get {
        return this.AssemblyIdentity;
      }
    }

    IEnumerable<IName> IAssemblyReference.Aliases {
      get { return Enumerable<IName>.Empty; }
    }

    IAssembly IAssemblyReference.ResolvedAssembly {
      get { return this; }
    }

    string IAssemblyReference.Culture {
      get { return this.AssemblyIdentity.Culture; }
    }

    IEnumerable<byte> IAssemblyReference.PublicKeyToken {
      get { return this.AssemblyIdentity.PublicKeyToken; }
    }

    Version IAssemblyReference.Version {
      get { return this.AssemblyIdentity.Version; }
    }

    #endregion
  }

  internal sealed class ModuleReference : MetadataObject, IMetadataReaderModuleReference {
    readonly uint ModuleRefRowId;
    internal readonly uint InternedId;
    internal readonly ModuleIdentity ModuleIdentity;
    IModule/*?*/ resolvedModule;

    internal ModuleReference(
      PEFileToObjectModel peFileToObjectModel,
      uint moduleRefRowId,
      uint internedId,
      ModuleIdentity moduleIdentity
    )
      : base(peFileToObjectModel) {
      this.ModuleRefRowId = moduleRefRowId;
      this.InternedId = internedId;
      this.ModuleIdentity = moduleIdentity;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.ModuleRef | this.ModuleRefRowId; }
    }

    internal IModule ResolvedModule {
      get {
        if (this.resolvedModule == null) {
          var resModule = this.PEFileToObjectModel.ResolveModuleRefReference(this);
          if (resModule == null) {
            //  Cant resolve error...
            this.resolvedModule = Dummy.Module;
          } else {
            this.resolvedModule = resModule;
          }
        }
        return this.resolvedModule;
      }
    }

    public override string ToString() {
      return this.ModuleIdentity.ToString();
    }

    #region IUnitReference Members

    UnitIdentity IUnitReference.UnitIdentity {
      get { return this.ModuleIdentity; }
    }

    IUnit IUnitReference.ResolvedUnit {
      get { return this.ResolvedModule; }
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.ModuleIdentity.Name; }
    }

    #endregion

    #region IModuleReference Members

    ModuleIdentity IModuleReference.ModuleIdentity {
      get { return this.ModuleIdentity; }
    }

    IModule IModuleReference.ResolvedModule {
      get { return this.ResolvedModule; }
    }

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return this.PEFileToObjectModel.ContainingAssembly; }
    }

    #endregion

    #region IMetadataReaderModuleReference Members

    public uint InternedModuleId {
      get { return this.InternedId; }
    }

    #endregion
  }

  internal sealed class AssemblyReference : MetadataObject, IAssemblyReference, IMetadataReaderModuleReference {
    readonly uint AssemblyRefRowId;
    internal readonly AssemblyIdentity AssemblyIdentity;
    AssemblyFlags AssemblyFlags;

    internal AssemblyReference(
      PEFileToObjectModel peFileToObjectModel,
      uint assemblyRefRowId,
      AssemblyIdentity assemblyIdentity,
      AssemblyFlags assemblyFlags
    )
      : base(peFileToObjectModel) {
      this.AssemblyRefRowId = assemblyRefRowId;
      this.AssemblyIdentity = assemblyIdentity;
      this.AssemblyFlags = assemblyFlags;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    internal uint InternedId {
      get {
        if (this.internedId == 0) {
          this.internedId = (uint)this.PEFileToObjectModel.InternFactory.GetAssemblyInternedKey(this.UnifiedAssemblyIdentity);
        }
        return this.internedId;
      }
    }
    private uint internedId;

    public bool IsRetargetable {
      get { return (this.AssemblyFlags & AssemblyFlags.Retargetable) != 0; }
    }

    public bool ContainsForeignTypes {
      get { return (this.AssemblyFlags & AssemblyFlags.ContainsForeignTypes) != 0; }
    }

    internal IAssembly ResolvedAssembly {
      get {
        if (this.resolvedAssembly == null) {
          Assembly/*?*/ assembly = this.PEFileToObjectModel.ResolveAssemblyRefReference(this);
          if (assembly == null) {
            //  Cant resolve error...
            this.resolvedAssembly = Dummy.Assembly;
          } else {
            this.resolvedAssembly = assembly;
          }
        }
        return this.resolvedAssembly;
      }
    }
    IAssembly/*?*/ resolvedAssembly;

    internal override uint TokenValue {
      get { return TokenTypeIds.AssemblyRef | this.AssemblyRefRowId; }
    }

    internal AssemblyIdentity UnifiedAssemblyIdentity {
      get {
        if (this.unifiedAssemblyIdentity == null)
          this.unifiedAssemblyIdentity = this.PEFileToObjectModel.ModuleReader.metadataReaderHost.UnifyAssembly(this);
        return this.unifiedAssemblyIdentity;
      }
    }
    AssemblyIdentity/*?*/ unifiedAssemblyIdentity;

    public override string ToString() {
      return this.AssemblyIdentity.ToString();
    }

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.AssemblyIdentity.Name; }
    }

    #endregion

    #region IUnitReference Members

    UnitIdentity IUnitReference.UnitIdentity {
      get { return this.AssemblyIdentity; }
    }

    IUnit IUnitReference.ResolvedUnit {
      get { return this.ResolvedAssembly; }
    }

    #endregion

    #region IModuleReference Members

    ModuleIdentity IModuleReference.ModuleIdentity {
      get { return this.AssemblyIdentity; }
    }

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return this; }
    }

    IModule IModuleReference.ResolvedModule {
      get { return this.ResolvedAssembly; }
    }

    #endregion

    #region IAssemblyReference Members

    AssemblyIdentity IAssemblyReference.AssemblyIdentity {
      get { return this.AssemblyIdentity; }
    }

    AssemblyIdentity IAssemblyReference.UnifiedAssemblyIdentity {
      get { return this.UnifiedAssemblyIdentity; }
    }

    IAssembly IAssemblyReference.ResolvedAssembly {
      get { return this.ResolvedAssembly; }
    }

    IEnumerable<IName> IAssemblyReference.Aliases {
      get { return Enumerable<IName>.Empty; }
    }

    string IAssemblyReference.Culture {
      get { return this.AssemblyIdentity.Culture; }
    }

    IEnumerable<byte> IAssemblyReference.HashValue {
      get {
        var hashBlob = this.PEFileToObjectModel.PEFileReader.AssemblyRefTable[this.AssemblyRefRowId].HashValue;
        if (hashBlob == 0) return Enumerable<byte>.Empty;
        return IteratorHelper.GetReadonly(this.PEFileToObjectModel.PEFileReader.BlobStream[hashBlob]);
      }
    }

    IEnumerable<byte> IAssemblyReference.PublicKey {
      get {
        if ((this.AssemblyFlags & PEFileFlags.AssemblyFlags.PublicKey) == 0) return Enumerable<byte>.Empty;
        var keyOrTokBlob = this.PEFileToObjectModel.PEFileReader.AssemblyRefTable[this.AssemblyRefRowId].PublicKeyOrToken;
        if (keyOrTokBlob == 0) return Enumerable<byte>.Empty;
        return IteratorHelper.GetReadonly(this.PEFileToObjectModel.PEFileReader.BlobStream[keyOrTokBlob]);
      }
    }

    IEnumerable<byte> IAssemblyReference.PublicKeyToken {
      get { return this.AssemblyIdentity.PublicKeyToken; }
    }

    Version IAssemblyReference.Version {
      get { return this.AssemblyIdentity.Version; }
    }

    #endregion

    #region IMetadataReaderModuleReference Members

    public uint InternedModuleId {
      get { return this.InternedId; }
    }

    #endregion
  }

  #endregion Assembly/Module Level Object Model


  #region Namespace Level Object Model

  internal abstract class Namespace : ScopedContainerMetadataObject<INamespaceMember, INamespaceMember, INamespaceDefinition>, IUnitNamespace {
    internal readonly IName NamespaceName;
    internal readonly IName NamespaceFullName;
    uint namespaceNameOffset;
    protected Namespace(
      PEFileToObjectModel peFileToObjectModel,
      IName namespaceName,
      IName namespaceFullName
    )
      : base(peFileToObjectModel) {
      this.NamespaceName = namespaceName;
      this.NamespaceFullName = namespaceFullName;
      this.namespaceNameOffset = 0xFFFFFFFF;
    }

    internal void SetNamespaceNameOffset(
      uint namespaceNameOffset
    ) {
      this.namespaceNameOffset = namespaceNameOffset;
    }

    internal uint NamespaceNameOffset {
      get {
        return this.namespaceNameOffset;
      }
    }

    internal override uint TokenValue {
      get { return 0xFFFFFFFF; }
    }

    internal override void LoadMembers() {
      //  Part of double check pattern. This method should be called after checking the flag FillMembers.
      lock (GlobalLock.LockingObject) {
        if (this.ContainerState == ContainerState.Loaded)
          return;
        this.StartLoadingMembers();
        if (this.namespaceNameOffset != 0xFFFFFFFF)
          this.PEFileToObjectModel.LoadTypesInNamespace(this);
        this.PEFileToObjectModel._Module_.LoadMembers();
        this.DoneLoadingMembers();
      }
    }

    public override string ToString() {
      return TypeHelper.GetNamespaceName((IUnitNamespaceReference)this, NameFormattingOptions.None);
    }

    #region IUnitNamespace Members

    public IUnit Unit {
      get { return this.PEFileToObjectModel.Module; }
    }

    #endregion

    #region INamespaceDefinition Members

    /// <summary>
    /// The object associated with the namespace. For example an IUnit or IUnitSet instance. This namespace is either the root namespace of that object
    /// or it is a nested namespace that is directly of indirectly nested in the root namespace.
    /// </summary>
    public INamespaceRootOwner RootOwner {
      get { return this.PEFileToObjectModel.Module; }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.NamespaceName; }
    }

    #endregion

    #region IUnitNamespaceReference Members

    IUnitReference IUnitNamespaceReference.Unit {
      get { return this.PEFileToObjectModel.Module; }
    }

    IUnitNamespace IUnitNamespaceReference.ResolvedUnitNamespace {
      get { return this; }
    }

    #endregion
  }

  internal sealed class RootNamespace : Namespace, IRootUnitNamespace {
    internal RootNamespace(
      PEFileToObjectModel peFileToObjectModel
    )
      : base(peFileToObjectModel, peFileToObjectModel.NameTable.EmptyName, peFileToObjectModel.NameTable.EmptyName) {
      this.SetNamespaceNameOffset(0);
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IRootUnitNamespaceReference)this);
    }

  }

  internal sealed class NestedNamespace : Namespace, INestedUnitNamespace {
    readonly Namespace ParentModuleNamespace;

    internal NestedNamespace(
      PEFileToObjectModel peFileToObjectModel,
      IName namespaceName,
      IName namespaceFullName,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, namespaceName, namespaceFullName) {
      this.ParentModuleNamespace = parentModuleNamespace;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INestedUnitNamespaceReference)this);
    }

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return this.ParentModuleNamespace; }
    }

    public IUnitNamespace ContainingUnitNamespace {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return this.ParentModuleNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

    #region INestedUnitNamespaceReference Members

    IUnitNamespaceReference INestedUnitNamespaceReference.ContainingUnitNamespace {
      get { return this.ParentModuleNamespace; }
    }

    INestedUnitNamespace INestedUnitNamespaceReference.ResolvedNestedUnitNamespace {
      get { return this; }
    }

    #endregion
  }

  internal abstract class NamespaceReference : MetadataObject, IUnitNamespaceReference {
    internal readonly IName NamespaceName;
    internal readonly IName NamespaceFullName;
    internal readonly IMetadataReaderModuleReference ModuleReference;

    protected NamespaceReference(
      PEFileToObjectModel peFileToObjectModel,
      IMetadataReaderModuleReference moduleReference,
      IName namespaceName,
      IName namespaceFullName
    )
      : base(peFileToObjectModel) {
      this.NamespaceName = namespaceName;
      this.ModuleReference = moduleReference;
      this.NamespaceFullName = namespaceFullName;
    }

    internal override uint TokenValue {
      get { return 0xFFFFFFFF; }
    }

    public override string ToString() {
      return TypeHelper.GetNamespaceName(this, NameFormattingOptions.None);
    }

    public sealed override void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    #region IUnitNamespaceReference Members

    public IUnitReference Unit {
      get { return this.ModuleReference; }
    }

    public abstract IUnitNamespace ResolvedUnitNamespace {
      get;
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.NamespaceName; }
    }

    #endregion
  }

  internal sealed class RootNamespaceReference : NamespaceReference, IRootUnitNamespaceReference {
    internal RootNamespaceReference(
      PEFileToObjectModel peFileToObjectModel,
      IMetadataReaderModuleReference moduleReference
    )
      : base(peFileToObjectModel, moduleReference, peFileToObjectModel.NameTable.EmptyName, peFileToObjectModel.NameTable.EmptyName) {
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override IUnitNamespace ResolvedUnitNamespace {
      get {
        return this.ModuleReference.ResolvedModule.UnitNamespaceRoot;
      }
    }
  }

  internal sealed class NestedNamespaceReference : NamespaceReference, INestedUnitNamespaceReference {
    readonly NamespaceReference ParentModuleNamespaceReference;
    INestedUnitNamespace/*?*/ resolvedNamespace;

    internal NestedNamespaceReference(
      PEFileToObjectModel peFileToObjectModel,
      IName namespaceName,
      IName namespaceFullName,
      NamespaceReference parentModuleNamespaceReference
    )
      : base(peFileToObjectModel, parentModuleNamespaceReference.ModuleReference, namespaceName, namespaceFullName) {
      this.ParentModuleNamespaceReference = parentModuleNamespaceReference;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override IUnitNamespace ResolvedUnitNamespace {
      get { return this.ResolvedNestedUnitNamespace; }
    }

    #region INestedUnitNamespaceReference Members

    public IUnitNamespaceReference ContainingUnitNamespace {
      get { return this.ParentModuleNamespaceReference; }
    }

    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get {
        if (this.resolvedNamespace == null) {
          foreach (INestedUnitNamespace nestedUnitNamespace
            in IteratorHelper.GetFilterEnumerable<INamespaceMember, INestedUnitNamespace>(
              this.ParentModuleNamespaceReference.ResolvedUnitNamespace.GetMembersNamed(this.NamespaceName, false)
            )
          ) {
            return this.resolvedNamespace = nestedUnitNamespace;
          }
          this.resolvedNamespace = Dummy.NestedUnitNamespace;
        }
        return this.resolvedNamespace;
      }
    }

    #endregion

    #region INamedEntity

    IName INamedEntity.Name { get { return this.Name; } }

    #endregion
  }

  #endregion  Namespace Level Object Model


  #region TypeMember Level Object Model

  internal abstract class TypeMember : MetadataDefinitionObject, ITypeDefinitionMember {
    protected readonly IName MemberName;
    internal readonly TypeBase OwningModuleType;

    protected TypeMember(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase owningModuleType
    )
      : base(peFileToObjectModel) {
      this.MemberName = memberName;
      this.OwningModuleType = owningModuleType;
    }

    public override string ToString() {
      return MemberHelper.GetMemberSignature(this, NameFormattingOptions.None);
    }

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get {
        return this.OwningModuleType;
      }
    }

    public abstract TypeMemberVisibility Visibility { get; }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return this.ContainingTypeDefinition; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    ITypeDefinition IContainerMember<ITypeDefinition>.Container {
      get { return this.OwningModuleType; }
    }

    IName IContainerMember<ITypeDefinition>.Name {
      get { return this.MemberName; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    IScope<ITypeDefinitionMember> IScopeMember<IScope<ITypeDefinitionMember>>.ContainingScope {
      get { return this.OwningModuleType; }
    }

    #endregion

    #region INamedEntity Members

    public virtual IName Name {
      get { return this.MemberName; }
    }

    #endregion
  }

  internal class FieldDefinition : TypeMember, IFieldDefinition {
    internal readonly uint FieldDefRowId;
    FieldFlags FieldFlags;
    IEnumerable<ICustomModifier>/*?*/ customModifiers;
    ITypeReference/*?*/ fieldType;
    //^ invariant ((this.FieldFlags & FieldFlags.FieldLoaded) == FieldFlags.FieldLoaded) ==> this.FieldType != null;

    internal FieldDefinition(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase parentModuleType,
      uint fieldDefRowId,
      FieldFlags fieldFlags
    )
      : base(peFileToObjectModel, memberName, parentModuleType) {
      this.FieldDefRowId = fieldDefRowId;
      this.FieldFlags = fieldFlags;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.FieldDef | this.FieldDefRowId; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IFieldReference)this);
    }

    void InitFieldSignature()
      //^ ensures (this.FieldFlags & FieldFlags.FieldLoaded) == FieldFlags.FieldLoaded;
    {
      lock (GlobalLock.LockingObject) {
        if ((this.FieldFlags & FieldFlags.FieldLoaded) != FieldFlags.FieldLoaded) {
          FieldSignatureConverter fieldSignature = this.PEFileToObjectModel.GetFieldSignature(this);
          this.fieldType = fieldSignature.TypeReference;
          this.customModifiers = fieldSignature.customModifiers;
          this.FieldFlags |= FieldFlags.FieldLoaded;
        }
      }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetFieldInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public override TypeMemberVisibility Visibility {
      get {
        //  IF this becomes perf bottle neck use array...
        switch (this.FieldFlags & FieldFlags.AccessMask) {
          case FieldFlags.CompilerControlledAccess:
            return TypeMemberVisibility.Other;
          case FieldFlags.PrivateAccess:
            return TypeMemberVisibility.Private;
          case FieldFlags.FamilyAndAssemblyAccess:
            return TypeMemberVisibility.FamilyAndAssembly;
          case FieldFlags.AssemblyAccess:
            return TypeMemberVisibility.Assembly;
          case FieldFlags.FamilyAccess:
            return TypeMemberVisibility.Family;
          case FieldFlags.FamilyOrAssemblyAccess:
            return TypeMemberVisibility.FamilyOrAssembly;
          case FieldFlags.PublicAccess:
            return TypeMemberVisibility.Public;
          default:
            return TypeMemberVisibility.Private;
        }
      }
    }

    #region IFieldDefinition Members

    public uint BitLength {
      get { return 1; }
    }

    public bool IsBitField {
      get { return false; }
    }

    public bool IsCompileTimeConstant {
      get { return (this.FieldFlags & FieldFlags.LiteralContract) == FieldFlags.LiteralContract; }
    }

    public bool IsMapped {
      get { return (this.FieldFlags & FieldFlags.HasFieldRVAReserved) == FieldFlags.HasFieldRVAReserved; }
    }

    public bool IsMarshalledExplicitly {
      get { return (this.FieldFlags & FieldFlags.HasFieldMarshalReserved) == FieldFlags.HasFieldMarshalReserved; }
    }

    public bool IsNotSerialized {
      get { return (this.FieldFlags & FieldFlags.NotSerializedContract) == FieldFlags.NotSerializedContract; }
    }

    public bool IsReadOnly {
      get { return (this.FieldFlags & FieldFlags.InitOnlyContract) == FieldFlags.InitOnlyContract; }
    }

    public bool IsRuntimeSpecial {
      get { return (this.FieldFlags & FieldFlags.RTSpecialNameReserved) == FieldFlags.RTSpecialNameReserved; }
    }

    public bool IsSpecialName {
      get { return (this.FieldFlags & FieldFlags.SpecialNameImpl) == FieldFlags.SpecialNameImpl; }
    }

    public bool IsStatic {
      get { return (this.FieldFlags & FieldFlags.StaticContract) == FieldFlags.StaticContract; }
    }

    public uint Offset {
      get { return this.PEFileToObjectModel.GetFieldOffset(this); }
    }

    public int SequenceNumber {
      get { return this.PEFileToObjectModel.GetFieldSequenceNumber(this); }
    }

    public IMetadataConstant CompileTimeValue {
      get { return this.PEFileToObjectModel.GetDefaultValue(this); }
    }

    public IMarshallingInformation MarshallingInformation {
      get { return this.PEFileToObjectModel.GetMarshallingInformation(this); }
    }

    public ITypeReference Type {
      get {
        ITypeReference/*?*/ fieldType = this.FieldType;
        if (fieldType == null) return Dummy.TypeReference;
        return fieldType;
      }
    }

    public ISectionBlock FieldMapping {
      get { return this.PEFileToObjectModel.GetFieldMapping(this); }
    }

    #endregion

    #region IMetadataReaderTypeMemberReference Members

    public ITypeReference/*?*/ OwningTypeReference {
      get { return this.OwningModuleType; }
    }

    #endregion

    #region IMetadataReaderFieldReference Members

    public ITypeReference/*?*/ FieldType {
      get {
        if ((this.FieldFlags & FieldFlags.FieldLoaded) != FieldFlags.FieldLoaded) {
          this.InitFieldSignature();
        }
        //^ assert (this.FieldFlags & FieldFlags.FieldLoaded) == FieldFlags.FieldLoaded;
        //^ assert this.fieldType != null;
        return this.fieldType;
      }
    }

    #endregion

    #region IFieldReference Members

    public IEnumerable<ICustomModifier> CustomModifiers {
      get {
        if ((this.FieldFlags & FieldFlags.FieldLoaded) != FieldFlags.FieldLoaded) this.InitFieldSignature();
        return this.customModifiers??Enumerable<ICustomModifier>.Empty;
      }
    }

    public bool IsModified {
      get {
        if ((this.FieldFlags & FieldFlags.FieldLoaded) != FieldFlags.FieldLoaded) this.InitFieldSignature();
        return this.customModifiers != null;
      }
    }

    public IFieldDefinition ResolvedField {
      get { return this; }
    }

    #endregion

    #region IMetadataConstantContainer

    IMetadataConstant IMetadataConstantContainer.Constant {
      get { return this.CompileTimeValue; }
    }

    #endregion

  }

  internal sealed class GlobalFieldDefinition : FieldDefinition, IGlobalFieldDefinition {
    readonly Namespace ParentModuleNamespace;
    readonly IName NamespaceMemberName;

    internal GlobalFieldDefinition(
      PEFileToObjectModel peFileToObjectModel,
      IName typeMemberName,
      TypeBase parentModuleType,
      uint fieldDefRowId,
      FieldFlags fieldFlags,
      IName namespaceMemberName,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, typeMemberName, parentModuleType, fieldDefRowId, fieldFlags) {
      this.NamespaceMemberName = namespaceMemberName;
      this.ParentModuleNamespace = parentModuleNamespace;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IFieldReference)this);
    }

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ParentModuleNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.NamespaceMemberName; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ParentModuleNamespace; }
    }

    #endregion
  }

  internal sealed class SectionBlock : ISectionBlock {
    readonly PESectionKind PESectionKind;
    readonly uint Offset;
    readonly MemoryBlock MemoryBlock;
    internal SectionBlock(
      PESectionKind peSectionKind,
      uint offset,
      MemoryBlock memoryBlock
    ) {
      this.PESectionKind = peSectionKind;
      this.Offset = offset;
      this.MemoryBlock = memoryBlock;
    }

    #region ISectionBlock Members

    PESectionKind ISectionBlock.PESectionKind {
      get { return this.PESectionKind; }
    }

    uint ISectionBlock.Offset {
      get { return this.Offset; }
    }

    uint ISectionBlock.Size {
      get { return (uint)this.MemoryBlock.Length; }
    }

    IEnumerable<byte> ISectionBlock.Data {
      get { return new EnumerableMemoryBlockWrapper(this.MemoryBlock); }
    }

    #endregion
  }

  internal sealed class ReturnParameter : MetadataObject {
    private readonly IName name;
    internal readonly ParamFlags ReturnParamFlags;
    internal readonly uint ReturnParamRowId;
    internal override uint TokenValue {
      get { return TokenTypeIds.ParamDef | this.ReturnParamRowId; }
    }
    internal ReturnParameter(
      PEFileToObjectModel peFileToObjectModel,
      IName name,
      ParamFlags returnParamFlags,
      uint returnParamRowId
    )
      : base(peFileToObjectModel) {
      this.name = name;
      this.ReturnParamFlags = returnParamFlags;
      this.ReturnParamRowId = returnParamRowId;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
    }

    [Pure]
    public bool IsMarshalledExplicitly {
      get { return (this.ReturnParamFlags & ParamFlags.HasFieldMarshalReserved) == ParamFlags.HasFieldMarshalReserved; }
    }

    public IMarshallingInformation MarshallingInformation {
      get { return this.PEFileToObjectModel.GetMarshallingInformation(this); }
    }

    public IName Name {
      get { return this.name; }
    }
  }

  internal sealed class PESection : IPESection {

    SectionHeader[] sectionHeaders;
    int index;
    IName sectionName;
    PEFileToObjectModel peFileToObjectModel;

    internal PESection(SectionHeader[] sectionHeaders, int index, IName sectionName, PEFileToObjectModel peFileToObjectModel) {
      this.sectionHeaders = sectionHeaders;
      this.index = index;
      this.sectionName = sectionName;
      this.peFileToObjectModel = peFileToObjectModel;
    }

    #region IPESection Members

    public IName SectionName {
      get { return this.sectionName; }
    }

    public PESectionCharacteristics Characteristics {
      get { return (PESectionCharacteristics)this.sectionHeaders[this.index].SectionCharacteristics; }
    }

    public int VirtualAddress {
      get { return this.sectionHeaders[this.index].VirtualAddress; }
    }

    public int VirtualSize {
      get { return this.sectionHeaders[this.index].VirtualSize; }
    }

    public int SizeOfRawData {
      get { return this.sectionHeaders[this.index].SizeOfRawData; }
    }

    public IEnumerable<byte> Rawdata {
      get {
        unsafe {
          var size = this.sectionHeaders[this.index].SizeOfRawData;
          MemoryBlock block =
            new MemoryBlock(
              this.peFileToObjectModel.PEFileReader.BinaryDocumentMemoryBlock.Pointer + this.sectionHeaders[this.index].OffsetToRawData + 0, size);
          return new EnumerableMemoryBlockWrapper(block);
        }
      }
    }

    #endregion
  }

  internal sealed class PlatformInvokeInformation : IPlatformInvokeInformation {
    readonly PInvokeMapFlags PInvokeMapFlags;
    readonly IName ImportName;
    readonly ModuleReference ImportModule;

    internal PlatformInvokeInformation(
      PInvokeMapFlags pInvokeMapFlags,
      IName importName,
      ModuleReference importModule
    ) {
      this.PInvokeMapFlags = pInvokeMapFlags;
      this.ImportName = importName;
      this.ImportModule = importModule;
    }

    #region IPlatformInvokeInformation Members

    IName IPlatformInvokeInformation.ImportName {
      get { return this.ImportName; }
    }

    IModuleReference IPlatformInvokeInformation.ImportModule {
      get { return this.ImportModule; }
    }

    StringFormatKind IPlatformInvokeInformation.StringFormat {
      get {
        switch (this.PInvokeMapFlags & PInvokeMapFlags.CharSetMask) {
          case PInvokeMapFlags.CharSetAnsi:
            return StringFormatKind.Ansi;
          case PInvokeMapFlags.CharSetUnicode:
            return StringFormatKind.Unicode;
          case PInvokeMapFlags.CharSetAuto:
            return StringFormatKind.AutoChar;
          case PInvokeMapFlags.CharSetNotSpec:
          default:
            return StringFormatKind.Unspecified;
        }
      }
    }

    bool IPlatformInvokeInformation.NoMangle {
      get { return (this.PInvokeMapFlags & PInvokeMapFlags.NoMangle) == PInvokeMapFlags.NoMangle; }
    }

    bool IPlatformInvokeInformation.SupportsLastError {
      get { return (this.PInvokeMapFlags & PInvokeMapFlags.SupportsLastError) == PInvokeMapFlags.SupportsLastError; }
    }

    public PInvokeCallingConvention PInvokeCallingConvention {
      get {
        switch (this.PInvokeMapFlags & PInvokeMapFlags.CallingConventionMask) {
          case PInvokeMapFlags.WinAPICallingConvention:
          default:
            return PInvokeCallingConvention.WinApi;
          case PInvokeMapFlags.CDeclCallingConvention:
            return PInvokeCallingConvention.CDecl;
          case PInvokeMapFlags.StdCallCallingConvention:
            return PInvokeCallingConvention.StdCall;
          case PInvokeMapFlags.ThisCallCallingConvention:
            return PInvokeCallingConvention.ThisCall;
          case PInvokeMapFlags.FastCallCallingConvention:
            return PInvokeCallingConvention.FastCall;
        }
      }
    }

    bool? IPlatformInvokeInformation.ThrowExceptionForUnmappableChar {
      get {
        switch (this.PInvokeMapFlags & PInvokeMapFlags.ThrowOnUnmappableCharMask) {
          case PInvokeMapFlags.EnabledThrowOnUnmappableChar: return true;
          case PInvokeMapFlags.DisabledThrowOnUnmappableChar: return false;
          default: return null;
        }
      }
    }

    bool? IPlatformInvokeInformation.UseBestFit {
      get {
        switch (this.PInvokeMapFlags & PInvokeMapFlags.BestFitMask) {
          case PInvokeMapFlags.EnabledBestFit: return true;
          case PInvokeMapFlags.DisabledBestFit: return false;
          default: return null;
        }
      }
    }

    #endregion
  }

  internal abstract class MethodDefinition : TypeMember, IMethodDefinition {
    internal readonly uint MethodDefRowId;
    internal MethodFlags MethodFlags;
    internal MethodImplFlags MethodImplFlags;
    IEnumerable<ICustomModifier>/*?*/ returnValueCustomModifiers;
    volatile ITypeReference/*?*/ returnType;
    byte FirstSignatureByte;
    IParameterDefinition[]/*?*/ moduleParameters;
    ReturnParameter/*?*/ returnParameter;

    internal MethodDefinition(PEFileToObjectModel peFileToObjectModel, IName memberName, TypeBase parentModuleType,
      uint methodDefRowId, MethodFlags methodFlags, MethodImplFlags methodImplFlags)
      : base(peFileToObjectModel, memberName, parentModuleType) {
      this.MethodDefRowId = methodDefRowId;
      this.MethodFlags = methodFlags;
      this.MethodImplFlags = methodImplFlags;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.MethodDef | this.MethodDefRowId; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IMethodReference)this);
    }

    public override TypeMemberVisibility Visibility {
      get {
        switch (this.MethodFlags & MethodFlags.AccessMask) {
          case MethodFlags.CompilerControlledAccess:
            return TypeMemberVisibility.Other;
          case MethodFlags.PrivateAccess:
            return TypeMemberVisibility.Private;
          case MethodFlags.FamilyAndAssemblyAccess:
            return TypeMemberVisibility.FamilyAndAssembly;
          case MethodFlags.AssemblyAccess:
            return TypeMemberVisibility.Assembly;
          case MethodFlags.FamilyAccess:
            return TypeMemberVisibility.Family;
          case MethodFlags.FamilyOrAssemblyAccess:
            return TypeMemberVisibility.FamilyOrAssembly;
          case MethodFlags.PublicAccess:
            return TypeMemberVisibility.Public;
          default:
            return TypeMemberVisibility.Private;
        }
      }
    }

    void InitMethodSignature() {
      Contract.Ensures(this.returnType != null);
      Contract.Ensures(this.returnParameter != null);
      lock (GlobalLock.LockingObject) {
        if (this.returnType == null) {
          MethodDefSignatureConverter methodSignature = this.PEFileToObjectModel.GetMethodSignature(this);
          this.FirstSignatureByte = methodSignature.FirstByte;
          this.moduleParameters = methodSignature.Parameters;
          this.returnParameter = methodSignature.ReturnParameter;
          this.returnValueCustomModifiers = methodSignature.ReturnCustomModifiers;
          this.returnType = methodSignature.ReturnTypeReference??Dummy.TypeReference;
        }
      }
    }

    public override IEnumerable<ILocation> Locations {
      get {
        MethodBodyLocation mbLoc = new MethodBodyLocation(new MethodBodyDocument(this), 0);
        return IteratorHelper.GetSingletonEnumerable<ILocation>(mbLoc);
      }
    }

    public override string ToString() {
      return MemberHelper.GetMethodSignature(this, NameFormattingOptions.ReturnType|NameFormattingOptions.Signature|NameFormattingOptions.TypeParameters);
    }

    #region IMethodDefinition Members

    public bool AcceptsExtraArguments {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        return SignatureHeader.IsVarArgCallSignature(this.FirstSignatureByte);
      }
    }

    public IMethodBody Body {
      get {
        var result = this.body == null ? null : this.body.Target as IMethodBody;
        if (result == null)
          result = this.PEFileToObjectModel.GetMethodBody(this);
        if (this.body == null)
          this.body = new WeakReference(result);
        else
          this.body.Target = result;
        return result;
      }
    }
    WeakReference body;

    public abstract IEnumerable<IGenericMethodParameter> GenericParameters { get; }

    public abstract ushort GenericParameterCount { get; }

    public bool HasDeclarativeSecurity {
      get { return (this.MethodFlags & MethodFlags.HasSecurityReserved) == MethodFlags.HasSecurityReserved; }
    }

    public bool HasExplicitThisParameter {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        return SignatureHeader.IsExplicitThis(this.FirstSignatureByte);
      }
    }

    public bool IsAbstract {
      get { return (this.MethodFlags & MethodFlags.AbstractImpl) == MethodFlags.AbstractImpl; }
    }

    public bool IsAccessCheckedOnOverride {
      get { return (this.MethodFlags & MethodFlags.CheckAccessOnOverrideImpl) == MethodFlags.CheckAccessOnOverrideImpl; }
    }

    public bool IsCil {
      get { return (this.MethodImplFlags & MethodImplFlags.CodeTypeMask) == MethodImplFlags.ILCodeType; }
    }

    public bool IsExternal {
      get {
        return this.IsPlatformInvoke || this.IsRuntimeInternal || this.IsRuntimeImplemented || 
        this.PEFileToObjectModel.PEFileReader.GetMethodIL(this.MethodDefRowId) == null;
      }
    }

    public bool IsForwardReference {
      get { return (this.MethodImplFlags & MethodImplFlags.ForwardRefInterop) == MethodImplFlags.ForwardRefInterop; }
    }

    public abstract bool IsGeneric { get; }

    public bool IsHiddenBySignature {
      get { return (this.MethodFlags & MethodFlags.HideBySignatureContract) == MethodFlags.HideBySignatureContract; }
    }

    public bool IsNativeCode {
      get { return (this.MethodImplFlags & MethodImplFlags.CodeTypeMask) == MethodImplFlags.NativeCodeType; }
    }

    public bool IsNewSlot {
      get { return (this.MethodFlags & MethodFlags.NewSlotVTable) == MethodFlags.NewSlotVTable; }
    }

    public bool IsNeverInlined {
      get { return (this.MethodImplFlags & MethodImplFlags.NoInlining) == MethodImplFlags.NoInlining; }
    }

    public bool IsNeverOptimized {
      get { return (this.MethodImplFlags & MethodImplFlags.NoOptimization) == MethodImplFlags.NoOptimization; }
    }

    public bool IsAggressivelyInlined {
      get { return (this.MethodImplFlags & MethodImplFlags.AggressiveInlining) == MethodImplFlags.AggressiveInlining; }
    }

    public bool IsPlatformInvoke {
      get { return (this.MethodFlags & MethodFlags.PInvokeInterop) == MethodFlags.PInvokeInterop; }
    }

    public bool IsRuntimeImplemented {
      get { return (this.MethodImplFlags & MethodImplFlags.CodeTypeMask) == MethodImplFlags.RuntimeCodeType; }
    }

    public bool IsRuntimeInternal {
      get { return (this.MethodImplFlags & MethodImplFlags.InternalCall) == MethodImplFlags.InternalCall; }
    }

    public bool IsRuntimeSpecial {
      get { return (this.MethodFlags & MethodFlags.RTSpecialNameReserved) == MethodFlags.RTSpecialNameReserved; }
    }

    public bool IsSealed {
      get { return (this.MethodFlags & MethodFlags.FinalContract) == MethodFlags.FinalContract; }
    }

    public bool IsSpecialName {
      get { return (this.MethodFlags & MethodFlags.SpecialNameImpl) == MethodFlags.SpecialNameImpl; }
    }

    public bool IsStatic {
      get { return (this.MethodFlags & MethodFlags.StaticContract) == MethodFlags.StaticContract; }
    }

    public bool IsSynchronized {
      get { return (this.MethodImplFlags & MethodImplFlags.Synchronized) == MethodImplFlags.Synchronized; }
    }

    public bool IsVirtual {
      get { return (this.MethodFlags & MethodFlags.VirtualContract) == MethodFlags.VirtualContract; }
    }

    public bool IsUnmanaged {
      get { return (this.MethodImplFlags & MethodImplFlags.Unmanaged) == MethodImplFlags.Unmanaged; }
    }

    public bool PreserveSignature {
      get { return (this.MethodImplFlags & MethodImplFlags.PreserveSigInterop) == MethodImplFlags.PreserveSigInterop; }
    }

    public bool RequiresSecurityObject {
      get { return (this.MethodFlags & MethodFlags.RequiresSecurityObjectReserved) == MethodFlags.RequiresSecurityObjectReserved; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get {
        uint secAttributeRowIdStart;
        uint secAttributeRowIdEnd;
        this.PEFileToObjectModel.GetSecurityAttributeInfo(this, out secAttributeRowIdStart, out secAttributeRowIdEnd);
        for (uint secAttributeIter = secAttributeRowIdStart; secAttributeIter < secAttributeRowIdEnd; ++secAttributeIter) {
          yield return this.PEFileToObjectModel.GetSecurityAttributeAtRow(this, secAttributeIter);
        }
      }
    }

    public bool IsConstructor {
      get { return this.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Ctor.UniqueKey && this.IsRuntimeSpecial; }
    }

    public bool IsStaticConstructor {
      get { return this.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Cctor.UniqueKey && this.IsRuntimeSpecial; }
    }

    public IPlatformInvokeInformation PlatformInvokeData {
      get { return this.PEFileToObjectModel.GetPlatformInvokeInformation(this); }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get {
        if (this.parameters == null)
          this.parameters = IteratorHelper.GetReadonly(this.RequiredModuleParameters)??Enumerable<IParameterDefinition>.Empty;
        return this.parameters;
      }
    }
    IEnumerable<IParameterDefinition> parameters;

    public ushort ParameterCount {
      get {
        if (this.returnType == null) {
          return this.PEFileToObjectModel.GetMethodParameterCount(this);
        }
        return (ushort)(this.moduleParameters == null ? 0 : this.moduleParameters.Length);
      }
    }

    public IEnumerable<ICustomAttribute> ReturnValueAttributes {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        //^ assert this.returnParameter != null;
        uint customAttributeRowIdStart;
        uint customAttributeRowIdEnd;
        this.PEFileToObjectModel.GetCustomAttributeInfo(this.returnParameter, out customAttributeRowIdStart, out customAttributeRowIdEnd);
        for (uint customAttributeIter = customAttributeRowIdStart; customAttributeIter < customAttributeRowIdEnd; ++customAttributeIter) {
          //^ assert this.returnParameter != null;
          yield return this.PEFileToObjectModel.GetCustomAttributeAtRow(this.returnParameter, this.returnParameter.TokenValue, customAttributeIter);
        }
      }
    }

    public bool ReturnValueIsMarshalledExplicitly {
      get {
        return this.returnParameter != null && this.returnParameter.IsMarshalledExplicitly;
      }
    }

    public IMarshallingInformation ReturnValueMarshallingInformation {
      get {
        return this.returnParameter == null ? Dummy.MarshallingInformation : this.returnParameter.MarshallingInformation;
      }
    }

    public IName ReturnValueName {
      get {
        return this.returnParameter == null ? Dummy.Name : this.returnParameter.Name;
      }
    }

    #endregion

    #region ISignature Members

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { return IteratorHelper.GetConversionEnumerable<IParameterDefinition, IParameterTypeInformation>(this.Parameters); }
    }

    public bool ReturnValueIsByRef {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        return (this.returnParameter.ReturnParamFlags & ParamFlags.ByReference) == ParamFlags.ByReference;
      }
    }

    public bool ReturnValueIsModified {
      get {
        if (this.returnType == null) this.InitMethodSignature();
        return this.returnValueCustomModifiers != null;
      }
    }

    public ITypeReference Type {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        return this.returnType;
      }
    }

    public CallingConvention CallingConvention {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        return (CallingConvention)this.FirstSignatureByte;
      }
    }

    #endregion

    #region IMetadataReaderMethodReference Members

    public ITypeReference/*?*/ OwningTypeReference {
      get { return this.OwningModuleType; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get {
        if (this.returnType == null) this.InitMethodSignature();
        return this.returnValueCustomModifiers??Enumerable<ICustomModifier>.Empty;
      }
    }

    internal IParameterDefinition[]/*?*/ RequiredModuleParameters {
      get {
        if (this.returnType == null) {
          this.InitMethodSignature();
        }
        //^ assert this.moduleParameters != null;
        return this.moduleParameters;
      }
    }

    #endregion

    #region IMethodReference Members

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetMethodInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public IMethodDefinition ResolvedMethod {
      get { return this; }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

  }

  internal class NonGenericMethod : MethodDefinition {
    internal NonGenericMethod(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase parentModuleType,
      uint methodDefRowId,
      MethodFlags methodFlags,
      MethodImplFlags methodImplFlags
    )
      : base(peFileToObjectModel, memberName, parentModuleType, methodDefRowId, methodFlags, methodImplFlags) {
    }

    public override IEnumerable<IGenericMethodParameter> GenericParameters {
      get { return Enumerable<IGenericMethodParameter>.Empty; }
    }

    public override bool IsGeneric {
      get { return false; }
    }

    public override ushort GenericParameterCount {
      get { return 0; }
    }

    public override string ToString() {
      return MemberHelper.GetMethodSignature(this, NameFormattingOptions.ReturnType|NameFormattingOptions.Signature);
    }

  }

  internal sealed class GlobalNonGenericMethod : NonGenericMethod, IGlobalMethodDefinition {
    readonly Namespace ParentModuleNamespace;
    readonly IName NamespaceMemberName;

    internal GlobalNonGenericMethod(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase parentModuleType,
      uint methodDefRowId,
      MethodFlags methodFlags,
      MethodImplFlags methodImplFlags,
      IName namespaceMemberName,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, memberName, parentModuleType, methodDefRowId, methodFlags, methodImplFlags) {
      this.NamespaceMemberName = namespaceMemberName;
      this.ParentModuleNamespace = parentModuleNamespace;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IMethodReference)this);
    }

    #region IMethodDefinition

    IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters {
      get {
        return this.GenericParameters;
      }
    }

    #endregion

    #region IMethodReference

    ushort IMethodReference.GenericParameterCount {
      get {
        return this.GenericParameterCount;
      }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ParentModuleNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.NamespaceMemberName; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

  }

  internal class GenericMethod : MethodDefinition {
    internal readonly uint GenericParamRowIdStart;
    internal readonly uint GenericParamRowIdEnd;

    internal GenericMethod(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase parentModuleType,
      uint methodDefRowId,
      MethodFlags methodFlags,
      MethodImplFlags methodImplFlags,
      uint genericParamRowIdStart,
      uint genericParamRowIdEnd
    )
      : base(peFileToObjectModel, memberName, parentModuleType, methodDefRowId, methodFlags, methodImplFlags) {
      this.GenericParamRowIdStart = genericParamRowIdStart;
      this.GenericParamRowIdEnd = genericParamRowIdEnd;
    }

    public override IEnumerable<IGenericMethodParameter> GenericParameters {
      get {
        uint genericRowIdEnd = this.GenericParamRowIdEnd;
        for (uint genericParamIter = this.GenericParamRowIdStart; genericParamIter < genericRowIdEnd; ++genericParamIter) {
          GenericMethodParameter/*?*/ mgmp = this.PEFileToObjectModel.GetGenericMethodParamAtRow(genericParamIter, this);
          if (mgmp == null)
            yield return Dummy.GenericMethodParameter;
          else
            yield return mgmp;
        }
      }
    }

    public override ushort GenericParameterCount {
      get {
        return (ushort)(this.GenericParamRowIdEnd - this.GenericParamRowIdStart);
      }
    }

    public override bool IsGeneric {
      get {
        return true;
      }
    }

    #region IMetadataReaderGenericMethod Members

    public ushort GenericMethodParameterCardinality {
      get {
        return (ushort)(this.GenericParamRowIdEnd - this.GenericParamRowIdStart);
      }
    }

    public ITypeReference/*?*/ GetGenericMethodParameterFromOrdinal(
      ushort genericParamOrdinal
    ) {
      if (genericParamOrdinal >= this.GenericMethodParameterCardinality)
        return null;
      uint genericRowId = this.GenericParamRowIdStart + genericParamOrdinal;
      return this.PEFileToObjectModel.GetGenericMethodParamAtRow(genericRowId, this);
    }

    #endregion
  }

  internal sealed class GlobalGenericMethod : GenericMethod, IGlobalMethodDefinition {
    readonly Namespace ParentModuleNamespace;
    readonly IName NamespaceMemberName;

    internal GlobalGenericMethod(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase parentModuleType,
      uint methodDefRowId,
      MethodFlags methodFlags,
      MethodImplFlags methodImplFlags,
      uint genericParamRowIdStart,
      uint genericParamRowIdEnd,
      IName namespaceMemberName,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, memberName, parentModuleType, methodDefRowId, methodFlags, methodImplFlags, genericParamRowIdStart, genericParamRowIdEnd) {
      this.NamespaceMemberName = namespaceMemberName;
      this.ParentModuleNamespace = parentModuleNamespace;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IMethodReference)this);
    }

    #region IMethodDefinition

    IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters {
      get {
        return this.GenericParameters;
      }
    }

    #endregion

    #region IMethodReference

    ushort IMethodReference.GenericParameterCount {
      get {
        return this.GenericParameterCount;
      }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ParentModuleNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.NamespaceMemberName; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ParentModuleNamespace; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  internal sealed class GenericMethodInstanceReferenceWithToken : GenericMethodInstanceReference, IMetadataObjectWithToken {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethod"></param>
    /// <param name="genericArguments"></param>
    /// <param name="internFactory"></param>
    /// <param name="tokenValue">
    /// The most significant byte identifies a metadata table, using the values specified by ECMA-335.
    /// The least significant three bytes represent the row number in the table, with the first row being numbered as one.
    /// If, for some implemenation reason, a metadata object implements this interface but was not obtained from a metadata table
    /// (for example it might be an array type reference that only occurs in a signature blob), the the value is UInt32.MaxValue.
    /// </param>
    internal GenericMethodInstanceReferenceWithToken(IMethodReference genericMethod, IEnumerable<ITypeReference> genericArguments, IInternFactory internFactory, uint tokenValue)
      : base(genericMethod, genericArguments, internFactory) {
      Contract.Requires(genericMethod.IsGeneric);
      this.tokenValue = tokenValue;
    }

    /// <summary>
    /// The most significant byte identifies a metadata table, using the values specified by ECMA-335.
    /// The least significant three bytes represent the row number in the table, with the first row being numbered as one.
    /// If, for some implemenation reason, a metadata object implements this interface but was not obtained from a metadata table
    /// (for example it might be an array type reference that only occurs in a signature blob), the the value is UInt32.MaxValue.
    /// </summary>
    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal sealed class EventDefinition : TypeMember, IEventDefinition {
    internal readonly uint EventRowId;
    EventFlags EventFlags;
    bool eventTypeInited;
    ITypeReference/*?*/ eventType;
    IMethodDefinition/*?*/ adderMethod;
    IMethodDefinition/*?*/ removerMethod;
    IMethodDefinition/*?*/ fireMethod;
    TypeMemberVisibility visibility;
    //^ invariant ((this.EventFlags & EventFlags.AdderLoaded) == EventFlags.AdderLoaded) ==> this.adderMethod != null;
    //^ invariant ((this.EventFlags & EventFlags.RemoverLoaded) == EventFlags.RemoverLoaded) ==> this.removerMethod != null;

    internal EventDefinition(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      TypeBase parentModuleType,
      uint eventRowId,
      EventFlags eventFlags
    )
      : base(peFileToObjectModel, memberName, parentModuleType) {
      this.EventRowId = eventRowId;
      this.EventFlags = eventFlags;
      this.visibility = TypeMemberVisibility.Mask;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.Event | this.EventRowId; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    internal IMethodDefinition AdderMethod {
      get {
        if ((this.EventFlags & EventFlags.AdderLoaded) != EventFlags.AdderLoaded) {
          this.adderMethod = this.PEFileToObjectModel.GetEventAddOrRemoveOrFireMethod(this, MethodSemanticsFlags.AddOn);
          if (this.adderMethod == null) {
            //  MDError
            this.adderMethod = Dummy.MethodDefinition;
          }
          this.EventFlags |= EventFlags.AdderLoaded;
        }
        //^ assert this.adderMethod != null;
        return this.adderMethod;
      }
    }

    internal IMethodDefinition RemoverMethod {
      get {
        if ((this.EventFlags & EventFlags.RemoverLoaded) != EventFlags.RemoverLoaded) {
          this.removerMethod = this.PEFileToObjectModel.GetEventAddOrRemoveOrFireMethod(this, MethodSemanticsFlags.RemoveOn);
          if (this.removerMethod == null) {
            //  MDError
            this.removerMethod = Dummy.MethodDefinition;
          }
          this.EventFlags |= EventFlags.RemoverLoaded;
        }
        //^ assert this.removerMethod != null;
        return this.removerMethod;
      }
    }

    internal IMethodDefinition/*?*/ FireMethod {
      get {
        if ((this.EventFlags & EventFlags.FireLoaded) != EventFlags.FireLoaded) {
          this.fireMethod = this.PEFileToObjectModel.GetEventAddOrRemoveOrFireMethod(this, MethodSemanticsFlags.Fire);
          this.EventFlags |= EventFlags.FireLoaded;
        }
        return this.fireMethod;
      }
    }

    public override TypeMemberVisibility Visibility {
      get {
        if (this.visibility == TypeMemberVisibility.Mask) {
          TypeMemberVisibility adderVisibility = this.AdderMethod.Visibility;
          TypeMemberVisibility removerVisibility = this.RemoverMethod.Visibility;
          this.visibility = TypeCache.LeastUpperBound(adderVisibility, removerVisibility);
        }
        return this.visibility;
      }
    }

    internal ITypeReference/*?*/ EventType {
      get {
        if (!this.eventTypeInited) {
          this.eventTypeInited = true;
          this.eventType = this.PEFileToObjectModel.GetEventType(this);
        }
        return this.eventType;
      }
    }

    #region IEventDefinition Members

    public IEnumerable<IMethodReference> Accessors {
      get { return this.PEFileToObjectModel.GetEventAccessorMethods(this); }
    }

    public IMethodReference Adder {
      get {
        if (this.AdderMethod is Dummy) return Dummy.MethodReference;
        return this.AdderMethod;
      }
    }

    public IMethodReference/*?*/ Caller {
      get { return this.FireMethod; }
    }

    public bool IsRuntimeSpecial {
      get { return (this.EventFlags & EventFlags.RTSpecialNameReserved) == EventFlags.RTSpecialNameReserved; }
    }

    public bool IsSpecialName {
      get { return (this.EventFlags & EventFlags.SpecialNameImpl) == EventFlags.SpecialNameImpl; }
    }

    public IMethodReference Remover {
      get {
        if (this.RemoverMethod is Dummy) return Dummy.MethodReference;
        return this.RemoverMethod;
      }
    }

    public ITypeReference Type {
      get {
        ITypeReference/*?*/ moduleTypeRef = this.EventType;
        if (moduleTypeRef == null) return Dummy.TypeReference;
        return moduleTypeRef;
      }
    }

    #endregion

  }

  internal sealed class PropertyDefinition : TypeMember, IPropertyDefinition {
    internal readonly uint PropertyRowId;
    PropertyFlags PropertyFlags;
    byte FirstSignatureByte;
    IEnumerable<ICustomModifier>/*?*/ returnValueCustomModifiers;
    ITypeReference/*?*/ returnType;
    IEnumerable<IParameterDefinition>/*?*/ parameters;
    IMethodDefinition/*?*/ getterMethod;
    IMethodDefinition/*?*/ setterMethod;
    TypeMemberVisibility visibility;

    internal PropertyDefinition(PEFileToObjectModel peFileToObjectModel, IName memberName, TypeBase containingType, uint propertyRowId, PropertyFlags propertyFlags)
      : base(peFileToObjectModel, memberName, containingType) {
      this.PropertyRowId = propertyRowId;
      this.PropertyFlags = propertyFlags;
      this.visibility = TypeMemberVisibility.Mask;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.Property | this.PropertyRowId; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    public override TypeMemberVisibility Visibility {
      get {
        if (this.visibility == TypeMemberVisibility.Mask) {
          var getterMethod = this.GetterMethod;
          var setterMethod = this.SetterMethod;
          TypeMemberVisibility getterVisibility = getterMethod == null ? TypeMemberVisibility.Other : getterMethod.Visibility;
          TypeMemberVisibility setterVisibility = setterMethod == null ? TypeMemberVisibility.Other : setterMethod.Visibility;
          this.visibility = TypeCache.LeastUpperBound(getterVisibility, setterVisibility);
        }
        return this.visibility;
      }
    }

    void InitPropertySignature()
      //^ ensures this.returnModuleCustomModifiers != null;
    {
      lock (GlobalLock.LockingObject) {
        if (this.returnValueCustomModifiers == null) {
          PropertySignatureConverter propertySignature = this.PEFileToObjectModel.GetPropertySignature(this);
          this.FirstSignatureByte = propertySignature.firstByte;
          this.returnType = propertySignature.type;
          this.parameters = propertySignature.parameters;
          if (propertySignature.returnValueIsByReference)
            this.PropertyFlags |= PropertyFlags.ReturnValueIsByReference;
          // Keep this last make sure we don't have the race in the double-check lock pattern.
          this.returnValueCustomModifiers = propertySignature.returnCustomModifiers??Enumerable<ICustomModifier>.Empty;
        }
      }
    }

    internal IEnumerable<ICustomModifier> ReturnModuleCustomModifiers {
      get {
        if (this.returnValueCustomModifiers == null) {
          this.InitPropertySignature();
        }
        //^ assert this.returnModuleCustomModifiers != null;
        return this.returnValueCustomModifiers;
      }
    }

    internal ITypeReference ReturnType {
      get {
        if (this.returnValueCustomModifiers == null) {
          this.InitPropertySignature();
        }
        //^ assert this.returnType != null;
        return this.returnType;
      }
    }

    internal IMethodDefinition/*?*/ GetterMethod {
      get {
        if ((this.PropertyFlags & PropertyFlags.GetterLoaded) != PropertyFlags.GetterLoaded) {
          this.getterMethod = this.PEFileToObjectModel.GetPropertyGetterOrSetterMethod(this, MethodSemanticsFlags.Getter);
          this.PropertyFlags |= PropertyFlags.GetterLoaded;
        }
        return this.getterMethod;
      }
    }

    internal IMethodDefinition/*?*/ SetterMethod {
      get {
        if ((this.PropertyFlags & PropertyFlags.SetterLoaded) != PropertyFlags.SetterLoaded) {
          this.setterMethod = this.PEFileToObjectModel.GetPropertyGetterOrSetterMethod(this, MethodSemanticsFlags.Setter);
          this.PropertyFlags |= PropertyFlags.SetterLoaded;
        }
        return this.setterMethod;
      }
    }

    #region IPropertyDefinition Members

    public IEnumerable<IMethodReference> Accessors {
      get { return this.PEFileToObjectModel.GetPropertyAccessorMethods(this); }
    }

    public IMetadataConstant DefaultValue {
      get { return this.PEFileToObjectModel.GetDefaultValue(this); }
    }

    public IMethodReference/*?*/ Getter {
      get { return this.GetterMethod; }
    }

    public bool HasDefaultValue {
      get { return (this.PropertyFlags & PropertyFlags.HasDefaultReserved) == PropertyFlags.HasDefaultReserved; }
    }

    public bool IsRuntimeSpecial {
      get { return (this.PropertyFlags & PropertyFlags.RTSpecialNameReserved) == PropertyFlags.RTSpecialNameReserved; }
    }

    public bool IsSpecialName {
      get { return (this.PropertyFlags & PropertyFlags.SpecialNameImpl) == PropertyFlags.SpecialNameImpl; }
    }

    public IMethodReference/*?*/ Setter {
      get { return this.SetterMethod; }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get {
        if (this.returnValueCustomModifiers == null) {
          this.InitPropertySignature();
        }
        //^ assert this.moduleParameters != null;
        return this.parameters;
      }
    }

    #endregion

    #region ISignature Members

    public bool IsStatic {
      get { return (this.CallingConvention & CallingConvention.HasThis) == 0; }
    }

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get {
        return IteratorHelper.GetConversionEnumerable<IParameterDefinition, IParameterTypeInformation>(this.Parameters);
      }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get {
        return this.ReturnModuleCustomModifiers;
      }
    }

    public bool ReturnValueIsByRef {
      get {
        if (this.returnValueCustomModifiers == null) {
          this.InitPropertySignature();
        }
        return (this.PropertyFlags & PropertyFlags.ReturnValueIsByReference) != 0;
      }
    }

    public bool ReturnValueIsModified {
      get { return IteratorHelper.EnumerableIsNotEmpty(this.ReturnModuleCustomModifiers); }
    }

    public ITypeReference Type {
      get {
        if (this.ReturnType == null) {
          //TODO: error
          return Dummy.TypeReference;
        }
        return this.ReturnType;
      }
    }

    public CallingConvention CallingConvention {
      get {
        if (this.returnValueCustomModifiers == null) {
          this.InitPropertySignature();
        }
        return (CallingConvention)(this.FirstSignatureByte&~0x08);
      }
    }

    #endregion

    #region IMetadataConstantContainer

    IMetadataConstant IMetadataConstantContainer.Constant {
      get { return this.DefaultValue; }
    }

    #endregion
  }

  #endregion TypeMember Level Object Model

  #region Member Ref level Object Model

  internal abstract class MemberReference : MetadataObject, ITypeMemberReference {
    internal readonly uint MemberRefRowId;
    internal readonly IName Name;
    internal readonly ITypeReference/*?*/ ParentTypeReference;

    internal MemberReference(
      PEFileToObjectModel peFileToObjectModel,
      uint memberRefRowId,
      ITypeReference/*?*/ parentTypeReference,
      IName name
    )
      : base(peFileToObjectModel) {
      this.MemberRefRowId = memberRefRowId;
      this.ParentTypeReference = parentTypeReference;
      this.Name = name;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.MemberRef | this.MemberRefRowId; }
    }

    public override string ToString() {
      return MemberHelper.GetMemberSignature(this, NameFormattingOptions.None);
    }

    public sealed override void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    #region IMetadataReaderTypeMemberReference Members

    public ITypeReference/*?*/ OwningTypeReference {
      get { return this.ParentTypeReference; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get {
        if (this.OwningTypeReference == null)
          return Dummy.TypeReference;
        return this.OwningTypeReference;
      }
    }

    public abstract ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get;
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.Name; }
    }

    #endregion
  }

  internal class FieldReference : MemberReference, IFieldReference {
    protected bool signatureLoaded;
    protected IEnumerable<ICustomModifier>/*?*/ customModifiers;
    protected ITypeReference/*?*/ typeReference;
    internal bool isStatic;
    internal FieldReference(
      PEFileToObjectModel peFileToObjectModel,
      uint memberRefRowId,
      ITypeReference/*?*/ parentTypeReference,
      IName name
    )
      : base(peFileToObjectModel, memberRefRowId, parentTypeReference, name) {
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    protected virtual void InitFieldSignature()
      //^ ensures this.signatureLoaded;
    {
      FieldSignatureConverter fieldSignature = this.PEFileToObjectModel.GetFieldRefSignature(this);
      this.typeReference = fieldSignature.TypeReference;
      this.customModifiers = fieldSignature.customModifiers;
      this.signatureLoaded = true;
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetFieldInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public override ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this.ResolvedField; }
    }

    #region IMetadataReaderFieldReference Members

    public ITypeReference/*?*/ FieldType {
      get {
        if (!this.signatureLoaded) {
          this.InitFieldSignature();
        }
        //^ assert this.typeReference != null;
        return this.typeReference;
      }
    }

    #endregion

    #region IFieldReference Members

    public IEnumerable<ICustomModifier> CustomModifiers {
      get {
        if (!this.signatureLoaded) this.InitFieldSignature();
        return this.customModifiers??Enumerable<ICustomModifier>.Empty;
      }
    }

    public bool IsModified {
      get {
        if (!this.signatureLoaded) this.InitFieldSignature();
        return this.customModifiers != null;
      }
    }

    public bool IsStatic {
      get { return this.isStatic; }
    }

    public virtual IFieldDefinition ResolvedField {
      get {
        var parent = this.ParentTypeReference;
        if (parent == null) return Dummy.FieldDefinition;
        return TypeHelper.GetField(parent.ResolvedType, this, true);
      }
    }

    public ITypeReference Type {
      get {
        ITypeReference/*?*/ result = this.FieldType;
        if (result == null) result = Dummy.TypeReference;
        return result;
      }
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.Name; }
    }

    #endregion
  }

  internal class MethodReference : MemberReference, IMethodReference {
    internal readonly byte FirstByte;
    protected ushort genericParameterCount;
    protected IEnumerable<ICustomModifier>/*?*/ returnCustomModifiers;
    protected ITypeReference/*?*/ returnTypeReference;
    protected bool isReturnByReference;
    protected IParameterTypeInformation[]/*?*/ requiredParameters;
    protected IParameterTypeInformation[]/*?*/ varArgParameters;

    internal MethodReference(PEFileToObjectModel peFileToObjectModel, uint memberRefRowId, ITypeReference/*?*/ parentTypeReference, IName name, byte firstByte)
      : base(peFileToObjectModel, memberRefRowId, parentTypeReference, name) {
      this.FirstByte = firstByte;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    protected virtual void InitMethodSignature() {
      lock (GlobalLock.LockingObject) {
        if (this.returnCustomModifiers == null) {
          MethodRefSignatureConverter methodSignature = this.PEFileToObjectModel.GetMethodRefSignature(this);
          this.genericParameterCount = methodSignature.GenericParamCount;
          this.returnTypeReference = methodSignature.ReturnTypeReference;
          this.isReturnByReference = methodSignature.IsReturnByReference;
          this.requiredParameters = methodSignature.RequiredParameters;
          this.varArgParameters = methodSignature.VarArgParameters;
          this.returnCustomModifiers = methodSignature.ReturnCustomModifiers??Enumerable<ICustomModifier>.Empty;
        }
      }
    }

    public override ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this.ResolvedMethod; }
    }

    public override string ToString() {
      return MemberHelper.GetMethodSignature(this, NameFormattingOptions.ReturnType|NameFormattingOptions.TypeParameters|NameFormattingOptions.Signature);
    }

    #region IMetadataReaderMethodReference Members

    public IEnumerable<ICustomModifier> ReturnCustomModifiers {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        //^ assert this.returnCustomModifiers != null;
        return this.returnCustomModifiers;
      }
    }

    public IParameterTypeInformation[]/*?*/ RequiredModuleParameterInfos {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        //^ assert this.requiredParameters != null;
        return this.requiredParameters;
      }
    }

    public IParameterTypeInformation[]/*?*/ VarArgModuleParameterInfos {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        //^ assert this.varArgParameters != null;
        return this.varArgParameters;
      }
    }

    #endregion

    #region IMethodReference Members

    public bool AcceptsExtraArguments {
      get { return (this.CallingConvention & (CallingConvention)0x7) == CallingConvention.ExtraArguments; }
    }

    public ushort GenericParameterCount {
      get {
        if (this.returnCustomModifiers == null) {
          return (ushort)this.PEFileToObjectModel.GetMethodRefGenericParameterCount(this);
        }
        return this.genericParameterCount;
      }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetMethodInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public bool IsGeneric {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        return this.genericParameterCount > 0;
      }
    }

    public bool IsStatic {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        return (this.CallingConvention & Cci.CallingConvention.HasThis) == 0;
      }
    }

    /// <summary>
    /// The method being referred to.
    /// </summary>
    public IMethodDefinition ResolvedMethod {
      get {
        if (this.resolvedMethod == null)
          this.resolvedMethod = MemberHelper.ResolveMethod(this);
        return this.resolvedMethod;
      }
    }
    IMethodDefinition/*?*/ resolvedMethod;

    public ushort ParameterCount {
      get {
        if (this.returnCustomModifiers == null) {
          return (ushort)this.PEFileToObjectModel.GetMethodRefParameterCount(this);
        }
        if (this.RequiredModuleParameterInfos == null) return 0;
        if (this.VarArgModuleParameterInfos == null) return (ushort)this.RequiredModuleParameterInfos.Length;
        return (ushort)(this.VarArgModuleParameterInfos.Length+this.RequiredModuleParameterInfos.Length);
      }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return IteratorHelper.GetReadonly(this.VarArgModuleParameterInfos)??Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region ISignature Members

    public CallingConvention CallingConvention {
      get { return (CallingConvention)this.FirstByte; }
    }

    public IEnumerable<IParameterTypeInformation> Parameters {
      get { return IteratorHelper.GetReadonly(this.RequiredModuleParameterInfos)??Enumerable<IParameterTypeInformation>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return this.ReturnCustomModifiers; }
    }

    public bool ReturnValueIsByRef {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        return this.isReturnByReference;
      }
    }

    public bool ReturnValueIsModified {
      get { return IteratorHelper.EnumerableIsNotEmpty(this.ReturnCustomModifiers); }
    }

    public ITypeReference Type {
      get {
        if (this.returnCustomModifiers == null) {
          this.InitMethodSignature();
        }
        if (this.returnTypeReference == null) {
          return Dummy.TypeReference;
        }
        return this.returnTypeReference;
      }
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.Name; }
    }

    #endregion
  }

  #endregion Member Ref level Object Model


  #region Miscellaneous Stuff

  internal sealed class ByValArrayMarshallingInformation : IMarshallingInformation {
    readonly System.Runtime.InteropServices.UnmanagedType arrayElementType;
    readonly uint numberOfElements;

    internal ByValArrayMarshallingInformation(
      System.Runtime.InteropServices.UnmanagedType arrayElementType,
      uint numberOfElements
    ) {
      this.arrayElementType = arrayElementType;
      this.numberOfElements = numberOfElements;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return this.arrayElementType; }
    }

    System.Runtime.InteropServices.UnmanagedType IMarshallingInformation.UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.ByValArray; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public uint NumberOfElements {
      get { return this.numberOfElements; }
    }

    public uint? ParamIndex {
      get { return null; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_EMPTY; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  internal sealed class ByValTStrMarshallingInformation : IMarshallingInformation {
    readonly uint numberOfElements;

    internal ByValTStrMarshallingInformation(
      uint numberOfElements
    ) {
      this.numberOfElements = numberOfElements;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return System.Runtime.InteropServices.UnmanagedType.AsAny; }
    }

    public System.Runtime.InteropServices.UnmanagedType UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.ByValTStr; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public uint NumberOfElements {
      get { return this.numberOfElements; }
    }

    public uint? ParamIndex {
      get { return null; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_EMPTY; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  internal sealed class IidParameterIndexMarshallingInformation : IMarshallingInformation {
    readonly uint iidParameterIndex;

    internal IidParameterIndexMarshallingInformation(
      uint iidParameterIndex
    ) {
      this.iidParameterIndex = iidParameterIndex;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return System.Runtime.InteropServices.UnmanagedType.AsAny; }
    }

    public System.Runtime.InteropServices.UnmanagedType UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.Interface; }
    }

    public uint IidParameterIndex {
      get { return this.iidParameterIndex; }
    }

    public uint NumberOfElements {
      get { return 0; }
    }

    public uint? ParamIndex {
      get { return null; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_EMPTY; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  internal sealed class LPArrayMarshallingInformation : IMarshallingInformation {
    readonly System.Runtime.InteropServices.UnmanagedType ArrayElementType;
    int paramIndex;
    uint numElement;

    internal LPArrayMarshallingInformation(System.Runtime.InteropServices.UnmanagedType arrayElementType, int paramIndex, uint numElement) {
      this.ArrayElementType = arrayElementType;
      this.paramIndex = paramIndex;
      this.numElement = numElement;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return this.ArrayElementType; }
    }

    System.Runtime.InteropServices.UnmanagedType IMarshallingInformation.UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.LPArray; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public uint NumberOfElements {
      get { return this.numElement; }
    }

    public uint? ParamIndex {
      get { return this.paramIndex < 0 ? (uint?)null : (uint)this.paramIndex; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_EMPTY; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  internal sealed class SafeArrayMarshallingInformation : IMarshallingInformation {
    readonly System.Runtime.InteropServices.VarEnum ArrayElementType;
    readonly ITypeReference safeArrayElementUserDefinedSubType;

    internal SafeArrayMarshallingInformation(
      System.Runtime.InteropServices.VarEnum arrayElementType,
      ITypeReference safeArrayElementUserDefinedSubType
    ) {
      this.ArrayElementType = arrayElementType;
      this.safeArrayElementUserDefinedSubType = safeArrayElementUserDefinedSubType;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return System.Runtime.InteropServices.UnmanagedType.AsAny; }
    }

    System.Runtime.InteropServices.UnmanagedType IMarshallingInformation.UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.SafeArray; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public uint NumberOfElements {
      get { return 0; }
    }

    public uint? ParamIndex {
      get { return null; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return this.ArrayElementType; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return this.safeArrayElementUserDefinedSubType; }
    }

    #endregion
  }

  internal sealed class SimpleMarshallingInformation : IMarshallingInformation {
    readonly System.Runtime.InteropServices.UnmanagedType unmanagedType;

    internal SimpleMarshallingInformation(
      System.Runtime.InteropServices.UnmanagedType unmanagedType
    ) {
      this.unmanagedType = unmanagedType;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return System.Runtime.InteropServices.UnmanagedType.AsAny; }
    }

    public System.Runtime.InteropServices.UnmanagedType UnmanagedType {
      get { return this.unmanagedType; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public uint NumberOfElements {
      get { return 0; }
    }

    public uint? ParamIndex {
      get { return null; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_EMPTY; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  internal sealed class CustomMarshallingInformation : IMarshallingInformation {
    readonly ITypeReference Marshaller;
    readonly string MarshallerRuntimeArgument;

    internal CustomMarshallingInformation(
      ITypeReference marshaller,
      string marshallerRuntimeArgument
    ) {
      this.Marshaller = marshaller;
      this.MarshallerRuntimeArgument = marshallerRuntimeArgument;
    }

    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return this.Marshaller; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return this.MarshallerRuntimeArgument; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return System.Runtime.InteropServices.UnmanagedType.AsAny; }
    }

    System.Runtime.InteropServices.UnmanagedType IMarshallingInformation.UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.CustomMarshaler; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public uint NumberOfElements {
      get { return 0; }
    }

    public uint? ParamIndex {
      get { return null; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_EMPTY; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  internal sealed class Win32Resource : IWin32Resource {
    internal readonly PEFileToObjectModel PEFileToObjectModel;
    internal readonly int TypeIdOrName;
    internal readonly int IdOrName;
    internal readonly int LanguageIdOrName;
    internal readonly int RVAToData;
    internal readonly uint Size;
    internal readonly uint CodePage;

    internal Win32Resource(
      PEFileToObjectModel peFileTOObjectModel,
      int typeIdOrName,
      int idOrName,
      int languageIdOrName,
      int rvaToData,
      uint size,
      uint codePage
    ) {
      this.PEFileToObjectModel = peFileTOObjectModel;
      this.TypeIdOrName = typeIdOrName;
      this.IdOrName = idOrName;
      this.LanguageIdOrName = languageIdOrName;
      this.RVAToData = rvaToData;
      this.Size = size;
      this.CodePage = codePage;
    }

    #region IWin32Resource Members

    public string TypeName {
      get {
        return this.PEFileToObjectModel.GetWin32ResourceName(this.TypeIdOrName);
      }
    }

    public int TypeId {
      get { return this.TypeIdOrName; }
    }

    public string Name {
      get {
        return this.PEFileToObjectModel.GetWin32ResourceName(this.IdOrName);
      }
    }

    public int Id {
      get { return this.IdOrName; }
    }

    public uint LanguageId {
      get { return (uint)this.LanguageIdOrName; }
    }

    uint IWin32Resource.CodePage {
      get {
        return this.CodePage;
      }
    }

    public IEnumerable<byte> Data {
      get {
        return this.PEFileToObjectModel.GetWin32ResourceBytes(this.RVAToData, (int)this.Size);
      }
    }

    #endregion
  }

  internal sealed class FileReference : MetadataObject, IFileReference {
    internal readonly uint FileRowId;
    internal readonly FileFlags FileFlags;
    internal readonly IName Name;
    internal FileReference(
      PEFileToObjectModel peFileToObjectModel,
      uint fileRowId,
      FileFlags fileFlags,
      IName name
    )
      : base(peFileToObjectModel) {
      this.FileRowId = fileRowId;
      this.FileFlags = fileFlags;
      this.Name = name;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    internal override uint TokenValue {
      get {
        return TokenTypeIds.File | this.FileRowId;
      }
    }

    #region IFileReference Members

    public IAssembly ContainingAssembly {
      get {
        IAssembly/*?*/ assem = this.PEFileToObjectModel.Module as IAssembly;
        return assem == null ? Dummy.Assembly : assem;
      }
    }

    public bool HasMetadata {
      get { return (this.FileFlags & FileFlags.ContainsNoMetadata) != FileFlags.ContainsNoMetadata; }
    }

    public IName FileName {
      get { return this.Name; }
    }

    public IEnumerable<byte> HashValue {
      get {
        return this.PEFileToObjectModel.GetFileHash(this.FileRowId);
      }
    }

    #endregion
  }

  internal class ResourceReference : MetadataObject, IResourceReference {
    internal readonly uint ResourceRowId;
    readonly IAssemblyReference DefiningAssembly;
    protected readonly ManifestResourceFlags Flags;
    internal readonly IName Name;
    IResource/*?*/ resolvedResource;

    internal ResourceReference(
      PEFileToObjectModel peFileToObjectModel,
      uint resourceRowId,
      IAssemblyReference definingAssembly,
      ManifestResourceFlags flags,
      IName name
    )
      : base(peFileToObjectModel) {
      this.ResourceRowId = resourceRowId;
      this.DefiningAssembly = definingAssembly;
      this.Flags = flags;
      this.Name = name;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.ManifestResource | this.ResourceRowId; }
    }

    #region IResourceReference Members

    IEnumerable<ICustomAttribute> IResourceReference.Attributes {
      get { return this.Attributes; }
    }

    IAssemblyReference IResourceReference.DefiningAssembly {
      get { return this.DefiningAssembly; }
    }


    public bool IsPublic {
      get { return (this.Flags & ManifestResourceFlags.PublicVisibility) == ManifestResourceFlags.PublicVisibility; }
    }

    IName IResourceReference.Name {
      get { return this.Name; }
    }

    public IResource Resource {
      get {
        if (this.resolvedResource == null) {
          this.resolvedResource = this.PEFileToObjectModel.ResolveResource(this, this);
        }
        return this.resolvedResource;
      }
    }

    #endregion
  }

  internal sealed class Resource : ResourceReference, IResource {

    internal Resource(
      PEFileToObjectModel peFileToObjectModel,
      uint resourceRowId,
      IName name,
      ManifestResourceFlags flags,
      bool inExternalFile
    )
      : base(peFileToObjectModel, resourceRowId, Dummy.AssemblyReference, inExternalFile ? flags | ManifestResourceFlags.InExternalFile : flags, name) {
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.ManifestResource | this.ResourceRowId; }
    }

    #region IResource Members

    public IEnumerable<byte> Data {
      get {
        return this.PEFileToObjectModel.GetResourceData(this);
      }
    }

    public IFileReference ExternalFile {
      get { return this.PEFileToObjectModel.GetExternalFileForResource(this.ResourceRowId); }
    }

    public bool IsInExternalFile {
      get { return (this.Flags & ManifestResourceFlags.InExternalFile) == ManifestResourceFlags.InExternalFile; }
    }

    #endregion

    #region IResourceReference Members

    IAssemblyReference IResourceReference.DefiningAssembly {
      get {
        IAssembly/*?*/ assem = this.PEFileToObjectModel.Module as IAssembly;
        return assem == null ? Dummy.AssemblyReference : assem;
      }
    }

    IResource IResourceReference.Resource {
      get { return this; }
    }

    #endregion
  }

  #endregion Miscellaneous Stuff

}
