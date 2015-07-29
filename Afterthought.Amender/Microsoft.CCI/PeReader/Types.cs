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

namespace Microsoft.Cci.MetadataReader.ObjectModelImplementation {

  /// <summary>
  /// Enumeration to identify various type kinds
  /// </summary>
  internal enum MetadataReaderTypeKind {
    Dummy,
    Nominal,
    TypeSpec,
    GenericInstance,
    Vector,
    Matrix,
    FunctionPointer,
    Pointer,
    ManagedPointer,
    GenericTypeParameter,
    GenericMethodParameter,
    ModifiedType,
  }

  /// <summary>
  /// A enumeration of all of the types that can be used in IL operations.
  /// </summary>
  internal enum MetadataReaderSignatureTypeCode {
    SByte,
    Int16,
    Int32,
    Int64,
    Byte,
    UInt16,
    UInt32,
    UInt64,
    Single,
    Double,
    IntPtr,
    UIntPtr,
    Void,
    Boolean,
    Char,
    Object,
    String,
    TypedReference,
    ValueType,
    NotModulePrimitive,
  }

  /// <summary>
  /// This represents either a namespace or nested type. This supports fast comparision of nominal types using interned module id, namespace name, type name
  /// and parent type reference in case of nested type.
  /// </summary>
  internal interface IMetadataReaderNamedTypeReference : INamedTypeReference {
    IMetadataReaderModuleReference ModuleReference { get; }
    IName/*?*/ NamespaceFullName { get; }
    IName MangledTypeName { get; }
    /// <summary>
    /// Type references resolve to type definitions. The resolution process traverses zero or more entries in exported type tables, also known as type aliases.
    /// If a reference is indirected via a type alias, ITypeReference.IsAlias is true and ITypeReference.AliasForType exposes the information in the relevant row of the exported type table.
    /// This method returns an object with that information, if available. If not available, it returns null.
    /// </summary>
    ExportedTypeAliasBase/*?*/ TryResolveAsExportedType();
  }

  /// <summary>
  /// Represents the core types such as int, float, object etc from the core assembly.
  /// These are created if these types are not directly referenced by the assembly being loaded.
  /// </summary>
  internal sealed class CoreTypeReference : MetadataObject, IMetadataReaderNamedTypeReference, INamespaceTypeReference {
    internal readonly IMetadataReaderModuleReference moduleReference;
    internal readonly NamespaceReference namespaceReference;
    internal readonly IName mangledTypeName;
    internal readonly IName name;
    internal readonly ushort genericParamCount;
    internal readonly MetadataReaderSignatureTypeCode signatureTypeCode;
    bool isResolved;
    ITypeDefinition/*?*/ resolvedModuleTypeDefintion;

    internal CoreTypeReference(
      PEFileToObjectModel peFileToObjectModel,
      IMetadataReaderModuleReference moduleReference,
      NamespaceReference namespaceReference,
      IName typeName,
      ushort genericParamCount,
      MetadataReaderSignatureTypeCode signatureTypeCode
    )
      : base(peFileToObjectModel) {
      this.moduleReference = moduleReference;
      this.namespaceReference = namespaceReference;
      this.signatureTypeCode = signatureTypeCode;
      this.name = typeName;
      this.genericParamCount = genericParamCount;
      if (genericParamCount > 0)
        this.mangledTypeName = peFileToObjectModel.NameTable.GetNameFor(typeName.Value + "`" + genericParamCount);
      else
        this.mangledTypeName = typeName;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    internal override uint TokenValue {
      get {
        return 0xFFFFFFFF;
      }
    }

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public bool IsEnum {
      get { return false; }
    }

    public bool IsValueType {
      get {
        switch (this.signatureTypeCode) {
          case MetadataReaderSignatureTypeCode.NotModulePrimitive:
          case MetadataReaderSignatureTypeCode.Object:
          case MetadataReaderSignatureTypeCode.String: return false;
          default: return true;
        }
      }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this.ResolvedType; }
    }

    IPlatformType ITypeReference.PlatformType {
      get { return this.PlatformType; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return TypeCache.PrimitiveTypeCodeConv[(int)this.signatureTypeCode]; }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this, NameFormattingOptions.None);
    }

    #endregion

    #region IMetadataReaderNamedTypeReference Members

    public IMetadataReaderModuleReference ModuleReference {
      get { return this.moduleReference; }
    }

    public IName/*?*/ NamespaceFullName {
      get { return this.namespaceReference.NamespaceFullName; }
    }

    public IName MangledTypeName {
      get { return this.mangledTypeName; }
    }

    public ExportedTypeAliasBase/*?*/ TryResolveAsExportedType() {
      return null;
    }

    #endregion

    #region INamespaceTypeReference Members

    public ushort GenericParameterCount {
      get { return this.genericParamCount; }
    }

    public IUnitNamespaceReference ContainingUnitNamespace {
      get { return this.namespaceReference; }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get {
        INamespaceTypeDefinition/*?*/ nsTypeDef = this.ResolvedType as INamespaceTypeDefinition;
        if (nsTypeDef == null || nsTypeDef is Dummy)
          return Dummy.NamespaceTypeDefinition;
        return nsTypeDef;
      }
    }

    public bool KeepDistinctFromDefinition {
      get { return false; }
    }

    #endregion

    #region INamedEntity Members

    IName INamedEntity.Name {
      get { return this.name; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return this.genericParamCount > 0; }
    }

    public INamedTypeDefinition ResolvedType {
      get {
        if (!this.isResolved) {
          this.isResolved = true;
          Assembly/*?*/ coreAssembly = this.PEFileToObjectModel.ModuleReader.CoreAssembly;
          if (coreAssembly != null) {
            this.resolvedModuleTypeDefintion = coreAssembly.PEFileToObjectModel.FindCoreTypeReference(this);
          }
        }
        if (this.resolvedModuleTypeDefintion == null) return Dummy.NamedTypeDefinition;
        return (INamedTypeDefinition)this.resolvedModuleTypeDefintion;
      }
    }

    #endregion

  }

  internal sealed class ModifiedTypeReference : ITypeReference, IModifiedTypeReference {

    internal ModifiedTypeReference(PEFileToObjectModel peFileToObjectModel, ITypeReference unmodifiedType, IEnumerable<ICustomModifier>/*?*/ customModifiers) {
      this.peFileToObjectModel = peFileToObjectModel;
      this.unmodifiedType = unmodifiedType;
      this.customModifiers = customModifiers;
    }

    readonly PEFileToObjectModel peFileToObjectModel;
    readonly ITypeReference unmodifiedType;
    readonly IEnumerable<ICustomModifier>/*?*/ customModifiers;

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #region ITypeReference Members

    public IPlatformType PlatformType {
      get { return this.peFileToObjectModel.PlatformType; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return this.unmodifiedType.TypeCode; }
    }

    public bool IsAlias {
      get { return false; }
    }

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public bool IsEnum {
      get { return this.unmodifiedType.IsEnum; }
    }

    public bool IsValueType {
      get { return this.unmodifiedType.IsValueType; }
    }

    public ITypeDefinition ResolvedType {
      get { return this.unmodifiedType.ResolvedType; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.peFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IModifiedTypeReference Members

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.customModifiers??Enumerable<ICustomModifier>.Empty; }
    }

    public ITypeReference UnmodifiedType {
      get { return this.unmodifiedType; }
    }

    #endregion
  }

  internal abstract class NamespaceTypeNameNamespaceReference : IUnitNamespaceReference {
    protected readonly NamespaceTypeNameTypeReference NamespaceTypeNameTypeReference;

    protected NamespaceTypeNameNamespaceReference(NamespaceTypeNameTypeReference namespaceTypeNameTypeReference) {
      this.NamespaceTypeNameTypeReference = namespaceTypeNameTypeReference;
    }

    #region IUnitNamespaceReference Members

    public IUnitReference Unit {
      get { return this.NamespaceTypeNameTypeReference.ModuleReference; }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get { return this.NamespaceTypeNameTypeReference.ResolvedType.ContainingUnitNamespace; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

  }

  internal sealed class NestedNamespaceTypeNameNamespaceReference : NamespaceTypeNameNamespaceReference, INestedUnitNamespaceReference {

    readonly NamespaceName NamespaceName;

    internal NestedNamespaceTypeNameNamespaceReference(NamespaceName namespaceName, NamespaceTypeNameTypeReference namespaceTypeNameTypeReference)
      : base(namespaceTypeNameTypeReference) {
      this.NamespaceName = namespaceName;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #region INestedUnitNamespaceReference Members

    public IUnitNamespaceReference ContainingUnitNamespace {
      get {
        if (this.NamespaceName.ParentNamespaceName == null)
          return new RootNamespaceTypeNameNamespaceReference(this.NamespaceTypeNameTypeReference);
        else
          return new NestedNamespaceTypeNameNamespaceReference(this.NamespaceName.ParentNamespaceName, this.NamespaceTypeNameTypeReference);
      }
    }

    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get {
        IUnitNamespace resolvedParent = this.ContainingUnitNamespace.ResolvedUnitNamespace;
        foreach (INamespaceMember member in resolvedParent.GetMembersNamed(this.Name, false)) {
          INestedUnitNamespace/*?*/ result = member as INestedUnitNamespace;
          if (result != null) return result;
        }
        return Dummy.NestedUnitNamespace;
      }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.NamespaceName.Name; }
    }

    #endregion
  }

  internal sealed class RootNamespaceTypeNameNamespaceReference : NamespaceTypeNameNamespaceReference, IRootUnitNamespaceReference {

    internal RootNamespaceTypeNameNamespaceReference(NamespaceTypeNameTypeReference namespaceTypeNameTypeReference)
      : base(namespaceTypeNameTypeReference) {
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

  }

  internal abstract class TypeNameTypeReference : IMetadataReaderNamedTypeReference {

    internal readonly IMetadataReaderModuleReference Module;
    internal readonly PEFileToObjectModel PEFileToObjectModel;

    internal TypeNameTypeReference(IMetadataReaderModuleReference module, PEFileToObjectModel peFileToObjectModel) {
      this.Module = module;
      this.PEFileToObjectModel = peFileToObjectModel;
    }

    public void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public bool IsAlias {
      get { return false; }
    }

    public bool IsEnum {
      get { return this.isEnum; }
      set { this.isEnum = value; }
    }
    bool isEnum;

    public bool IsValueType {
      get { return false; }
    }

    public IPlatformType PlatformType {
      get { return this.PEFileToObjectModel.PlatformType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get {
        var result = this.GetResolvedType();
        if (result is Dummy) return Dummy.TypeDefinition;
        return result;
      }
    }

    public virtual PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IMetadataReaderNamedTypeReference Members

    public IMetadataReaderModuleReference ModuleReference {
      get { return this.Module; }
    }

    public abstract IName/*?*/ NamespaceFullName { get; }

    public abstract IName MangledTypeName { get; }

    public ExportedTypeAliasBase/*?*/ TryResolveAsExportedType() {
      return null; //only type references that have entries in the TypeRef table can be redirected via the ExportedType table.
    }

    #endregion

    #region INamedTypeReference Members

    public abstract ushort GenericParameterCount { get; }

    public abstract bool MangleName { get; }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return this.GetResolvedType(); }
    }

    internal abstract INamedTypeDefinition GetResolvedType();

    #endregion

    #region INamedEntity Members

    public abstract IName Name { get; }

    #endregion
  }

  internal sealed class NamespaceTypeNameTypeReference : TypeNameTypeReference, INamespaceTypeReference {

    internal readonly NamespaceTypeName NamespaceTypeName;

    internal NamespaceTypeNameTypeReference(IMetadataReaderModuleReference module, NamespaceTypeName namespaceTypeName, PEFileToObjectModel peFileToObjectModel)
      : base(module, peFileToObjectModel) {
      this.NamespaceTypeName = namespaceTypeName;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    INamedTypeDefinition/*?*/ Resolve() {
      if (this.Module != this.PEFileToObjectModel.Module) {
        AssemblyReference assemRef = this.Module as AssemblyReference;
        if (assemRef == null) return null;
        var internalAssembly = assemRef.ResolvedAssembly as Assembly;
        if (internalAssembly == null) return null;
        PEFileToObjectModel assemblyPEFileToObjectModel = internalAssembly.PEFileToObjectModel;
        var retModuleType = 
          assemblyPEFileToObjectModel.ResolveNamespaceTypeDefinition(this.NamespaceFullName, this.MangledTypeName);
        if (retModuleType != null) return retModuleType;
        return null;
      }
      return this.NamespaceTypeName.ResolveNominalTypeName(this.Module);
    }

    public override PrimitiveTypeCode TypeCode {
      get {
        if (this.typeCode == PrimitiveTypeCode.Invalid) {
          this.typeCode = PrimitiveTypeCode.NotPrimitive;
          if (this.Module.ContainingAssembly.AssemblyIdentity.Equals(this.PEFileToObjectModel.ModuleReader.metadataReaderHost.CoreAssemblySymbolicIdentity)) {
            var td = this.ResolvedType;
            if (!(td is Dummy))
              this.typeCode = td.TypeCode;
            else
              this.typeCode = this.UseNameToResolveTypeCode();
          }
        }
        return this.typeCode;
      }
    }
    PrimitiveTypeCode typeCode = PrimitiveTypeCode.Invalid;


    private PrimitiveTypeCode UseNameToResolveTypeCode() {
      var ns = this.ContainingUnitNamespace as INestedUnitNamespaceReference;
      if (ns == null) return PrimitiveTypeCode.NotPrimitive;
      var pe = this.PEFileToObjectModel;
      if (ns.Name.UniqueKey != pe.NameTable.System.UniqueKey) return PrimitiveTypeCode.NotPrimitive;
      var rs = ns.ContainingUnitNamespace as IRootUnitNamespaceReference;
      if (rs == null) return PrimitiveTypeCode.NotPrimitive;
      var key = this.Name.UniqueKey;
      if (key == pe.PlatformType.SystemBoolean.Name.UniqueKey) return PrimitiveTypeCode.Boolean;
      if (key == pe.PlatformType.SystemInt8.Name.UniqueKey) return PrimitiveTypeCode.UInt8;
      if (key == pe.PlatformType.SystemChar.Name.UniqueKey) return PrimitiveTypeCode.Char;
      if (key == pe.PlatformType.SystemFloat64.Name.UniqueKey) return PrimitiveTypeCode.Float64;
      if (key == pe.PlatformType.SystemInt16.Name.UniqueKey) return PrimitiveTypeCode.Int16;
      if (key == pe.PlatformType.SystemInt32.Name.UniqueKey) return PrimitiveTypeCode.Int32;
      if (key == pe.PlatformType.SystemInt64.Name.UniqueKey) return PrimitiveTypeCode.Int64;
      if (key == pe.PlatformType.SystemInt8.Name.UniqueKey) return PrimitiveTypeCode.Int8;
      if (key == pe.PlatformType.SystemIntPtr.Name.UniqueKey) return PrimitiveTypeCode.IntPtr;
      if (key == pe.PlatformType.SystemFloat32.Name.UniqueKey) return PrimitiveTypeCode.Float32;
      if (key == pe.PlatformType.SystemString.Name.UniqueKey) return PrimitiveTypeCode.String;
      if (key == pe.PlatformType.SystemUInt16.Name.UniqueKey) return PrimitiveTypeCode.UInt16;
      if (key == pe.PlatformType.SystemUInt32.Name.UniqueKey) return PrimitiveTypeCode.UInt32;
      if (key == pe.PlatformType.SystemUInt64.Name.UniqueKey) return PrimitiveTypeCode.UInt64;
      if (key == pe.PlatformType.SystemUIntPtr.Name.UniqueKey) return PrimitiveTypeCode.UIntPtr;
      if (key == pe.PlatformType.SystemVoid.Name.UniqueKey) return PrimitiveTypeCode.Void;
      return PrimitiveTypeCode.NotPrimitive;
    }

    #region INamespaceTypeReference Members

    public override ushort GenericParameterCount {
      get { return (ushort)this.NamespaceTypeName.GenericParameterCount; }
    }

    public IUnitNamespaceReference ContainingUnitNamespace {
      get {
        if (this.NamespaceTypeName.NamespaceName == null)
          return new RootNamespaceTypeNameNamespaceReference(this);
        else
          return new NestedNamespaceTypeNameNamespaceReference(this.NamespaceTypeName.NamespaceName, this);
      }
    }

    internal override INamedTypeDefinition GetResolvedType() {
      return this.ResolvedType;
    }

    public INamespaceTypeDefinition ResolvedType {
      get {
        INamespaceTypeDefinition/*?*/ result = this.resolvedType;
        if (result == null) {
          result = this.Resolve() as INamespaceTypeDefinition;
          if (result == null) result = Dummy.NamespaceTypeDefinition;
          this.resolvedType = result;
        }
        return result;
      }
    }
    INamespaceTypeDefinition resolvedType;

    public bool KeepDistinctFromDefinition {
      get { return false; }
    }

    #endregion

    #region INamedEntity Members

    public override IName Name {
      get { return this.NamespaceTypeName.UnmangledTypeName; }
    }

    #endregion

    #region IMetadataReaderNamedTypeReference Members

    public override IName MangledTypeName {
      get { return this.NamespaceTypeName.Name; }
    }

    public override IName/*?*/ NamespaceFullName {
      get {
        if (this.NamespaceTypeName.NamespaceName == null)
          return this.PEFileToObjectModel.NameTable.EmptyName;
        else
          return this.NamespaceTypeName.NamespaceName.FullyQualifiedName;
      }
    }

    #endregion

    #region INamedTypeReference Members

    public override bool MangleName {
      get { return this.NamespaceTypeName.MangleName; }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return this.ResolvedType; }
    }

    #endregion

  }

  internal sealed class NestedTypeNameTypeReference : TypeNameTypeReference, INestedTypeReference {

    internal readonly NestedTypeName NestedTypeName;

    internal NestedTypeNameTypeReference(IMetadataReaderModuleReference module, NestedTypeName nestedTypeName, PEFileToObjectModel peFileToObjectModel)
      : base(module, peFileToObjectModel) {
      this.NestedTypeName = nestedTypeName;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    INamedTypeDefinition Resolve() {
      return this.NestedTypeName.ResolveNominalTypeName(this.Module);
    }

    #region INestedTypeReference Members

    public override ushort GenericParameterCount {
      get { return (ushort)this.NestedTypeName.GenericParameterCount; }
    }

    internal override INamedTypeDefinition GetResolvedType() {
      return this.ResolvedType;
    }

    public INestedTypeDefinition ResolvedType {
      get {
        INestedTypeDefinition/*?*/ result = this.resolvedType;
        if (result == null) {
          result = this.Resolve() as INestedTypeDefinition;
          if (result == null) result = Dummy.NestedTypeDefinition;
          this.resolvedType = result;
        }
        return result;
      }
    }
    INestedTypeDefinition resolvedType;

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return this.NestedTypeName.ContainingTypeName.GetAsTypeReference(this.PEFileToObjectModel, this.Module); }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this.ResolvedType; }
    }

    #endregion

    #region INamedEntity Members

    public override IName Name {
      get { return this.NestedTypeName.UnmangledTypeName; }
    }

    #endregion

    #region IMetadataReaderNamedTypeReference Members

    public override IName MangledTypeName {
      get { return this.NestedTypeName.Name; }
    }

    public override IName/*?*/ NamespaceFullName {
      get { return null; }
    }

    #endregion

    #region INamedTypeReference Members

    public override bool MangleName {
      get { return this.NestedTypeName.MangleName; }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return this.ResolvedType; }
    }

    #endregion

  }

  /// <summary>
  /// Represents type reference to types in TypeRef table. This could either be Namespace type reference or nested type reference.
  /// </summary>
  internal abstract class TypeRefReference : MetadataObject, IMetadataReaderNamedTypeReference {
    internal readonly uint TypeRefRowId;
    readonly IMetadataReaderModuleReference moduleReference;
    protected readonly IName typeName;
    bool isResolved;
    bool isAliasIsInitialized;
    protected internal bool isValueType;
    INamedTypeDefinition resolvedTypeDefinition;
    ExportedTypeAliasBase/*?*/ exportedAliasBase;

    internal TypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      bool isValueType
    )
      : base(peFileToObjectModel) {
      this.TypeRefRowId = typeRefRowId;
      this.typeName = typeName;
      this.moduleReference = moduleReference;
      this.isValueType = isValueType;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.TypeRef | this.TypeRefRowId; }
    }

    /// <summary>
    /// Type references resolve to type definitions. The resolution process traverses zero or more entries in exported type tables, also known as type aliases.
    /// If a reference is indirected via a type alias, ITypeReference.IsAlias is true and ITypeReference.AliasForType exposes the information in the relevant row of the exported type table.
    /// This method returns an object with that information, if available. If not available, it returns null.
    /// </summary>
    public abstract ExportedTypeAliasBase/*?*/ TryResolveAsExportedType();

    internal void InitResolvedModuleType() {
      var moduleType = this.PEFileToObjectModel.ResolveModuleTypeRefReference(this);
      if (moduleType != null) {
        this.resolvedTypeDefinition = moduleType;
        this.isResolved = true;
        return;
      }
      if (!this.IsAlias) {
        this.resolvedTypeDefinition = Dummy.NamedTypeDefinition;
        this.isResolved = true;
        return;
      }
      this.resolvedTypeDefinition = this.exportedAliasBase.AliasedType.ResolvedType;
      this.isResolved = true;
    }

    internal void InitExportedAliasBase() {
      if (this.isAliasIsInitialized) return;
      this.isAliasIsInitialized = true;
      this.exportedAliasBase = this.TryResolveAsExportedType();
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    #region IMetadataReaderNamedTypeReference Members

    public IMetadataReaderModuleReference ModuleReference {
      get { return this.moduleReference; }
    }

    public abstract IName/*?*/ NamespaceFullName { get; }

    public abstract IName MangledTypeName { get; }

    public INamedTypeDefinition ResolvedType {
      get {
        if (!this.isResolved) {
          this.InitResolvedModuleType();
        }
        return this.resolvedTypeDefinition;
      }
    }

    #endregion

    #region ITypeReference Members

    ITypeDefinition ITypeReference.ResolvedType {
      get {
        var resolvedTypeDefRef = this.ResolvedType;
        if (resolvedTypeDefRef is Dummy) return Dummy.TypeDefinition;
        return resolvedTypeDefRef;
      }
    }

    //  Consider A.B aliases to C.D, and C.D aliases to E.F.
    //  Then:
    //  typereference(A.B).IsAlias == true && typereference(A.B).AliasForType == aliasfortype(A.B).
    //  aliasfortype(A.B).AliasedType == typereference(C.D).
    //  typereference(C.D).IsAlias == true && typereference(C.D).AliasForType == aliasfortype(C.D).
    //  aliasfortype(C.D).AliasedType == typereference(E.F)
    //  typereference(E.F).IsAlias == false
    //  Also, typereference(A.B).ResolvedType == typereference(C.D).ResolvedType == typereference(E.F).ResolvedType

    public bool IsAlias {
      get {
        if (!this.isAliasIsInitialized) {
          this.InitExportedAliasBase();
        }
        return this.exportedAliasBase != null;
      }
    }

    public IAliasForType AliasForType {
      get {
        if (!this.isAliasIsInitialized) {
          this.InitExportedAliasBase();
        }
        return this.exportedAliasBase == null ? Dummy.AliasForType : this.exportedAliasBase;
      }
    }

    public bool IsEnum {
      get { return this.ResolvedType.IsEnum; }
    }

    public virtual bool IsValueType {
      get { return this.isValueType; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    IPlatformType ITypeReference.PlatformType {
      get { return this.PlatformType; }
    }

    public abstract PrimitiveTypeCode TypeCode {
      get;
    }


    #endregion

    #region INamedEntity Members

    public IName Name {
      get {
        return this.typeName;
      }
    }

    #endregion

    #region INamedTypeReference Members

    public abstract bool MangleName {
      get;
    }

    public abstract ushort GenericParameterCount {
      get;
    }

    #endregion
  }

  internal abstract class NamespaceTypeRefReference : TypeRefReference, IMetadataReaderNamedTypeReference, INamespaceTypeReference {
    readonly NamespaceReference namespaceReference;

    internal NamespaceTypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      NamespaceReference namespaceReference,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, isValueType) {
      this.namespaceReference = namespaceReference;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Type references resolve to type definitions. The resolution process traverses zero or more entries in exported type tables, also known as type aliases.
    /// If a reference is indirected via a type alias, ITypeReference.IsAlias is true and ITypeReference.AliasForType exposes the information in the relevant row of the exported type table.
    /// This method returns an object with that information, if available. If not available, it returns null.
    /// </summary>
    public override ExportedTypeAliasBase/*?*/ TryResolveAsExportedType() {
      return this.PEFileToObjectModel.TryToResolveNamespaceTypeReferenceAsExportedType(this);
    }

    public override IName/*?*/ NamespaceFullName {
      get {
        return this.namespaceReference.NamespaceFullName;
      }
    }

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    #region INamespaceTypeReference Members

    public IUnitNamespaceReference ContainingUnitNamespace {
      get {
        return this.namespaceReference;
      }
    }

    public new INamespaceTypeDefinition ResolvedType {
      get {
        INamespaceTypeDefinition/*?*/ nsTypeDef = base.ResolvedType as INamespaceTypeDefinition;
        if (nsTypeDef == null || nsTypeDef is Dummy)
          return Dummy.NamespaceTypeDefinition;
        return nsTypeDef;
      }
    }

    public bool KeepDistinctFromDefinition {
      get { return this.ModuleReference.InternedModuleId == this.PEFileToObjectModel.Module.InternedModuleId; }
    }

    #endregion

    #region INamedTypeReference Members

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get {
        var result = this.ResolvedType;
        if (result is Dummy) return Dummy.NamedTypeDefinition;
        return result;
      }
    }

    #endregion
  }

  internal abstract class NonGenericNamespaceTypeRefReference : NamespaceTypeRefReference {

    internal NonGenericNamespaceTypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      NamespaceReference namespaceReference,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, namespaceReference, isValueType) {
    }

    public override ushort GenericParameterCount {
      get { return 0; }
    }

    public override IName MangledTypeName {
      get { return this.typeName; }
    }

    #region INamedTypeReference Members

    public sealed override bool MangleName {
      get { return false; }
    }

    #endregion
  }

  internal sealed class GenericNamespaceTypeRefReference : NamespaceTypeRefReference {
    readonly IName mangledTypeName;
    readonly ushort genericParamCount;

    internal GenericNamespaceTypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      NamespaceReference namespaceReference,
      IName mangledTypeName,
      ushort genericParamCount,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, namespaceReference, isValueType) {
      this.mangledTypeName = mangledTypeName;
      this.genericParamCount = genericParamCount;
    }

    public override ushort GenericParameterCount {
      get { return this.genericParamCount; }
    }

    public override IName MangledTypeName {
      get { return this.mangledTypeName; }
    }

    #region INamedTypeReference Members

    public override bool MangleName {
      get { return this.MangledTypeName.UniqueKey != this.Name.UniqueKey; }
    }

    #endregion
  }

  internal sealed class NamespaceTypeRefReferenceWithoutPrimitiveTypeCode : NonGenericNamespaceTypeRefReference {
    internal NamespaceTypeRefReferenceWithoutPrimitiveTypeCode(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      NamespaceReference namespaceReference,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, namespaceReference, isValueType) {
    }

  }

  internal sealed class NamespaceTypeRefReferenceWithPrimitiveTypeCode : NonGenericNamespaceTypeRefReference {
    readonly MetadataReaderSignatureTypeCode signatureTypeCode;

    internal NamespaceTypeRefReferenceWithPrimitiveTypeCode(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      NamespaceReference namespaceReference,
      MetadataReaderSignatureTypeCode signatureTypeCode
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, namespaceReference, signatureTypeCode == MetadataReaderSignatureTypeCode.ValueType) {
      this.signatureTypeCode = signatureTypeCode;
      switch (signatureTypeCode) {
        case MetadataReaderSignatureTypeCode.NotModulePrimitive:
        case MetadataReaderSignatureTypeCode.Object:
        case MetadataReaderSignatureTypeCode.String: break;
        default: this.isValueType = true; break;
      }
    }

    public override PrimitiveTypeCode TypeCode {
      get { return TypeCache.PrimitiveTypeCodeConv[(int)this.signatureTypeCode]; }
    }

  }

  internal abstract class NestedTypeRefReference : TypeRefReference, INestedTypeReference {
    readonly IMetadataReaderNamedTypeReference parentTypeReference;

    internal NestedTypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      IMetadataReaderNamedTypeReference parentTypeReference,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, isValueType) {
      this.parentTypeReference = parentTypeReference;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override ExportedTypeAliasBase/*?*/ TryResolveAsExportedType() {
      ExportedTypeAliasBase/*?*/ parentExportedType = this.parentTypeReference.TryResolveAsExportedType();
      if (parentExportedType == null) return null;
      return parentExportedType.PEFileToObjectModel.ResolveExportedNestedType(parentExportedType, this.MangledTypeName);
    }

    public override IName/*?*/ NamespaceFullName {
      get {
        return null;
      }
    }

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    #region INestedTypeReference Members

    public new INestedTypeDefinition ResolvedType {
      get {
        INestedTypeDefinition/*?*/ nstTypeDef = base.ResolvedType as INestedTypeDefinition;
        if (nstTypeDef == null || nstTypeDef is Dummy)
          return Dummy.NestedTypeDefinition;
        return nstTypeDef;
      }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return this.parentTypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this.ResolvedType; }
    }

    #endregion

    #region INamedTypeReference Members

    public override bool MangleName {
      get { return false; }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return this.ResolvedType; }
    }

    #endregion

  }

  internal sealed class NonGenericNestedTypeRefReference : NestedTypeRefReference {

    internal NonGenericNestedTypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      IMetadataReaderNamedTypeReference parentTypeReference,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, parentTypeReference, isValueType) {
    }

    public override ushort GenericParameterCount {
      get { return 0; }
    }

    public override IName MangledTypeName {
      get { return this.typeName; }
    }
  }

  internal sealed class GenericNestedTypeRefReference : NestedTypeRefReference {
    readonly IName mangledTypeName;
    readonly ushort genericParamCount;

    internal GenericNestedTypeRefReference(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeRefRowId,
      IMetadataReaderModuleReference moduleReference,
      IMetadataReaderNamedTypeReference parentTypeReference,
      IName mangledTypeName,
      ushort genericParamCount,
      bool isValueType
    )
      : base(peFileToObjectModel, typeName, typeRefRowId, moduleReference, parentTypeReference, isValueType) {
      this.mangledTypeName = mangledTypeName;
      this.genericParamCount = genericParamCount;
    }

    public override ushort GenericParameterCount {
      get { return this.genericParamCount; }
    }

    public override IName MangledTypeName {
      get { return this.mangledTypeName; }
    }

    public override bool MangleName {
      get {
        return this.typeName.UniqueKey != this.mangledTypeName.UniqueKey;
      }
    }
  }

  internal sealed class TypeSpecReference : MetadataObject, ITypeReference {
    internal readonly uint TypeSpecRowId;
    internal readonly MetadataObject TypeSpecOwner;
    bool underlyingTypeInited;
    ITypeReference/*?*/ underlyingModuleTypeReference;

    internal TypeSpecReference(
      PEFileToObjectModel peFileToObjectModel,
      uint typeSpecRowId,
      MetadataObject typeSpecOwner
    )
      : base(peFileToObjectModel) {
      this.TypeSpecRowId = typeSpecRowId;
      this.TypeSpecOwner = typeSpecOwner;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      this.UnderlyingModuleTypeReference.Dispatch(visitor);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      this.UnderlyingModuleTypeReference.DispatchAsReference(visitor);
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.TypeSpec | this.TypeSpecRowId; }
    }

    internal ITypeReference/*?*/ UnderlyingModuleTypeReference {
      get {
        if (!this.underlyingTypeInited) {
          this.underlyingTypeInited = true;
          this.underlyingModuleTypeReference = this.PEFileToObjectModel.UnderlyingModuleTypeSpecReference(this);
        }
        return this.underlyingModuleTypeReference;
      }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public ITypeDefinition ResolvedType {
      get { return this.UnderlyingModuleTypeReference.ResolvedType; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsValueType {
      get {
        ITypeReference/*?*/ underlyingModuleTypeReference = this.UnderlyingModuleTypeReference;
        if (underlyingModuleTypeReference == null) return false;
        return underlyingModuleTypeReference.IsValueType;
      }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public PrimitiveTypeCode TypeCode {
      get {
        ITypeReference/*?*/ underlyingModuleTypeReference = this.UnderlyingModuleTypeReference;
        if (underlyingModuleTypeReference == null) return PrimitiveTypeCode.NotPrimitive;
        return underlyingModuleTypeReference.TypeCode;
      }
    }

    IPlatformType ITypeReference.PlatformType {
      get { return this.PlatformType; }
    }

    #endregion
  }

  internal sealed class MethodImplementation : IMethodImplementation {
    readonly ITypeDefinition containingType;
    readonly IMethodReference methodDeclaration;
    readonly IMethodReference methodBody;

    internal MethodImplementation(
      ITypeDefinition containingType,
      IMethodReference methodDeclaration,
      IMethodReference methodBody
    ) {
      this.containingType = containingType;
      this.methodDeclaration = methodDeclaration;
      this.methodBody = methodBody;
    }

    #region IMethodImplementation Members

    public ITypeDefinition ContainingType {
      get { return this.containingType; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IMethodReference ImplementedMethod {
      get { return this.methodDeclaration; }
    }

    public IMethodReference ImplementingMethod {
      get { return this.methodBody; }
    }

    #endregion
  }

  internal abstract class TypeBase : ScopedContainerMetadataObject<ITypeDefinitionMember, ITypeDefinitionMember, ITypeDefinition>, IMetadataReaderNamedTypeReference, INamedTypeDefinition {
    internal readonly IName TypeName;
    internal readonly uint TypeDefRowId;
    internal TypeDefFlags TypeDefFlags;
    internal ITypeReference/*?*/ baseTypeReference;
    uint interfaceRowIdStart;
    uint interfaceRowIdEnd;
    protected byte initFlags;
    internal const byte BaseInitFlag = 0x01;
    internal const byte EnumInited = 0x02;
    internal const byte InheritTypeParametersInited = 0x04;

    protected TypeBase(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags
    )
      : base(peFileToObjectModel) {
      this.TypeName = typeName;
      this.TypeDefRowId = typeDefRowId;
      this.interfaceRowIdStart = 0xFFFFFFFF;
      this.TypeDefFlags = typeDefFlags;
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.TypeDef | this.TypeDefRowId; }
    }

    internal override void LoadMembers() {
      lock (GlobalLock.LockingObject) {
        if (this.ContainerState == ContainerState.Loaded)
          return;
        this.StartLoadingMembers();
        this.PEFileToObjectModel.LoadMembersOfType(this);
        this.DoneLoadingMembers();
      }
    }

    public IEnumerable<IEventDefinition> Events {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        return this.PEFileToObjectModel.GetEventsOfType(this);
      }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        return this.PEFileToObjectModel.GetFieldsOfType(this);
      }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        return this.PEFileToObjectModel.GetMethodsOfType(this);
      }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        return this.nestedTypes;
      }
    }
    internal IEnumerable<INestedTypeDefinition> nestedTypes = Enumerable<INestedTypeDefinition>.Empty;

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get {
        return Enumerable<ITypeDefinitionMember>.Empty;
      }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get {
        if (this.ContainerState != ContainerState.Loaded) {
          this.LoadMembers();
        }
        return this.PEFileToObjectModel.GetPropertiesOfType(this);
      }
    }

    internal ITypeReference/*?*/ BaseTypeReference {
      get {
        if ((this.initFlags & TypeBase.BaseInitFlag) != TypeBase.BaseInitFlag) {
          this.initFlags |= TypeBase.BaseInitFlag;
          this.baseTypeReference = this.PEFileToObjectModel.GetBaseTypeForType(this);
        }
        return this.baseTypeReference;
      }
    }

    internal uint InterfaceRowIdStart {
      get {
        if (this.interfaceRowIdStart == 0xFFFFFFFF) {
          this.PEFileToObjectModel.GetInterfaceInfoForType(this, out this.interfaceRowIdStart, out this.interfaceRowIdEnd);
        }
        return this.interfaceRowIdStart;
      }
    }

    internal uint InterfaceRowIdEnd {
      get {
        if (this.interfaceRowIdStart == 0xFFFFFFFF) {
          this.PEFileToObjectModel.GetInterfaceInfoForType(this, out this.interfaceRowIdStart, out this.interfaceRowIdEnd);
        }
        return this.interfaceRowIdEnd;
      }
    }

    internal uint InterfaceCount {
      get {
        if (this.interfaceRowIdStart == 0xFFFFFFFF) {
          this.PEFileToObjectModel.GetInterfaceInfoForType(this, out this.interfaceRowIdStart, out this.interfaceRowIdEnd);
        }
        return this.interfaceRowIdEnd - this.interfaceRowIdStart;
      }
    }

    #region ITypeDefinition Members

    public ushort Alignment {
      get {
        return this.PEFileToObjectModel.GetAlignment(this);
      }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get {
        ITypeReference/*?*/ baseType = this.BaseTypeReference;
        if (baseType == null)
          return Enumerable<ITypeReference>.Empty;
        //^ assert baseType != null;
        return IteratorHelper.GetSingletonEnumerable<ITypeReference>(baseType);
      }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get {
        uint methodImplStart;
        uint methodImplEnd;
        this.PEFileToObjectModel.GetMethodImplInfoForType(this, out methodImplStart, out methodImplEnd);
        for (uint methodImplIter = methodImplStart; methodImplIter < methodImplEnd; ++methodImplIter) {
          yield return this.PEFileToObjectModel.GetMethodImplementation(this, methodImplIter);
        }
      }
    }

    public abstract IEnumerable<IGenericTypeParameter> GenericParameters { get; }

    public abstract ushort GenericParameterCount { get; }

    public IEnumerable<ITypeReference> Interfaces {
      get {
        uint ifaceRowIdEnd = this.InterfaceRowIdEnd;
        for (uint interfaceIter = this.InterfaceRowIdStart; interfaceIter < ifaceRowIdEnd; ++interfaceIter) {
          ITypeReference/*?*/ typeRef = this.PEFileToObjectModel.GetInterfaceForInterfaceRowId(this, interfaceIter);
          if (typeRef == null) typeRef = Dummy.TypeReference;
          yield return typeRef;
        }
      }
    }

    public abstract IGenericTypeInstanceReference InstanceType { get; }

    public bool IsAbstract {
      get {
        return (this.TypeDefFlags & TypeDefFlags.AbstractSemantics) == TypeDefFlags.AbstractSemantics;
      }
    }

    public bool IsClass {
      get { return !this.IsInterface && !this.IsValueType && !this.IsDelegate; }
    }

    public bool IsDelegate {
      get {
        return TypeHelper.TypesAreEquivalent(this.BaseTypeReference, this.PEFileToObjectModel.SystemMulticastDelegate);
      }
    }

    public bool IsEnum {
      get {
        return TypeHelper.TypesAreEquivalent(this.BaseTypeReference, this.PEFileToObjectModel.SystemEnum) && this.IsSealed && !this.IsGeneric;
      }
    }

    public abstract bool IsGeneric { get; }

    public bool IsInterface {
      get { return (this.TypeDefFlags & TypeDefFlags.InterfaceSemantics) == TypeDefFlags.InterfaceSemantics; }
    }

    public bool IsReferenceType {
      get { return !this.IsStatic && !this.IsValueType; }
    }

    public bool IsSealed {
      get { return (this.TypeDefFlags & TypeDefFlags.SealedSemantics) == TypeDefFlags.SealedSemantics; }
    }

    public bool IsStatic {
      get { return this.IsAbstract && this.IsSealed; }
    }

    public bool IsValueType {
      get {
        return (TypeHelper.TypesAreEquivalent(this.BaseTypeReference, this.PEFileToObjectModel.SystemValueType)
            || TypeHelper.TypesAreEquivalent(this.BaseTypeReference, this.PEFileToObjectModel.SystemEnum))
          && this.IsSealed;
      }
    }

    public bool IsStruct {
      get { return TypeHelper.TypesAreEquivalent(this.BaseTypeReference, this.PEFileToObjectModel.SystemValueType) && this.IsSealed; }
    }

    public uint SizeOf {
      get { return this.PEFileToObjectModel.GetClassSize(this); }
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

    public abstract ITypeReference UnderlyingType {
      get;
    }

    public abstract PrimitiveTypeCode TypeCode { get; }

    public LayoutKind Layout {
      get {
        switch (this.TypeDefFlags & TypeDefFlags.LayoutMask) {
          case TypeDefFlags.ExplicitLayout:
            return LayoutKind.Explicit;
          case TypeDefFlags.SeqentialLayout:
            return LayoutKind.Sequential;
          case TypeDefFlags.AutoLayout:
          default:
            return LayoutKind.Auto;
        }
      }
    }

    public bool IsSpecialName {
      get { return (this.TypeDefFlags & TypeDefFlags.SpecialNameSemantics) == TypeDefFlags.SpecialNameSemantics; }
    }

    public bool IsComObject {
      get { return (this.TypeDefFlags & TypeDefFlags.ImportImplementation) == TypeDefFlags.ImportImplementation; }
    }

    public bool IsSerializable {
      get { return (this.TypeDefFlags & TypeDefFlags.SerializableImplementation) == TypeDefFlags.SerializableImplementation; }
    }

    public bool IsBeforeFieldInit {
      get { return (this.TypeDefFlags & TypeDefFlags.BeforeFieldInitImplementation) == TypeDefFlags.BeforeFieldInitImplementation; }
    }

    public StringFormatKind StringFormat {
      get {
        switch (this.TypeDefFlags & TypeDefFlags.StringMask) {
          case TypeDefFlags.AnsiString:
            return StringFormatKind.Ansi;
          case TypeDefFlags.AutoCharString:
            return StringFormatKind.AutoChar;
          case TypeDefFlags.UnicodeString:
          default:
            return StringFormatKind.Unicode;
        }
      }
    }

    public bool IsRuntimeSpecial {
      get { return (this.TypeDefFlags & TypeDefFlags.RTSpecialNameReserved) == TypeDefFlags.RTSpecialNameReserved; }
    }

    public bool HasDeclarativeSecurity {
      get { return (this.TypeDefFlags & TypeDefFlags.HasSecurityReserved) == TypeDefFlags.HasSecurityReserved; }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.TypeName; }
    }

    #endregion

    #region IMetadataReaderNamedTypeReference Members

    public IMetadataReaderModuleReference ModuleReference {
      get { return this.PEFileToObjectModel.Module; }
    }

    public abstract IName/*?*/ NamespaceFullName { get; }

    public virtual IName MangledTypeName {
      get { return this.TypeName; }
    }

    public ExportedTypeAliasBase/*?*/ TryResolveAsExportedType() {
      return null;
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    IPlatformType ITypeReference.PlatformType {
      get { return this.PlatformType; }
    }

    #endregion

    #region IMetadataReaderGenericType Members

    public abstract ushort GenericTypeParameterCardinality { get; }

    public abstract ushort ParentGenericTypeParameterCardinality { get; }

    public abstract ITypeReference/*?*/ GetGenericTypeParameterFromOrdinal(ushort genericParamOrdinal);

    #endregion

    #region INamedTypeReference Members

    public virtual bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion
  }

  internal abstract class NamespaceType : TypeBase, IMetadataReaderNamedTypeReference, INamespaceTypeDefinition {
    readonly Namespace ParentModuleNamespace;

    protected NamespaceType(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, typeName, typeDefRowId, typeDefFlags) {
      this.ParentModuleNamespace = parentModuleNamespace;
    }

    public IEnumerable<ICustomAttribute> AttributesFor(ITypeReference implementedInterface) {
      uint ifaceRowIdEnd = this.InterfaceRowIdEnd;
      for (uint interfaceIter = this.InterfaceRowIdStart; interfaceIter < ifaceRowIdEnd; ++interfaceIter) {
        ITypeReference/*?*/ typeRef = this.PEFileToObjectModel.GetInterfaceForInterfaceRowId(this, interfaceIter);
        if (typeRef == null || typeRef.InternedKey != implementedInterface.InternedKey) continue;
        uint count = 0;
        var table = this.PEFileToObjectModel.PEFileReader.CustomAttributeTable;
        var token = TokenTypeIds.InterfaceImpl | interfaceIter;
        uint customAttributeStart = table.FindCustomAttributesForToken(token, out count);
        for (uint i = 0; i < count; i++) {
          yield return this.PEFileToObjectModel.GetCustomAttributeAtRow(this, token, customAttributeStart+i);
        }
      }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INamespaceTypeReference)this);
    }

    public override IName/*?*/ NamespaceFullName {
      get { return this.ParentModuleNamespace.NamespaceFullName; }
    }

    #region INamespaceTypeDefinition Members

    public IUnitNamespace ContainingUnitNamespace {
      get {
        return this.ParentModuleNamespace;
      }
    }

    public bool IsPublic {
      get { return (this.TypeDefFlags & TypeDefFlags.PublicAccess) == TypeDefFlags.PublicAccess; }
    }

    public bool IsForeignObject {
      get { return (this.TypeDefFlags & TypeDefFlags.IsForeign) == TypeDefFlags.IsForeign; }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get {
        return this.ParentModuleNamespace;
      }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get {
        return this.ParentModuleNamespace;
      }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get {
        return this.ParentModuleNamespace;
      }
    }

    #endregion

    #region INamespaceTypeReference Members

    IUnitNamespaceReference INamespaceTypeReference.ContainingUnitNamespace {
      get { return this.ContainingUnitNamespace; }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get { return this; }
    }

    public bool KeepDistinctFromDefinition {
      get { return false; }
    }

    #endregion

  }

  internal abstract class NonGenericNamespaceType : NamespaceType {
    ITypeReference/*?*/ enumUnderlyingType;
    internal NonGenericNamespaceType(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, memberName, typeDefRowId, typeDefFlags, parentModuleNamespace) {
    }

    public override IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public override ushort GenericParameterCount {
      get { return 0; }
    }

    public override bool IsGeneric {
      get { return false; }
    }

    public override IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public override ushort GenericTypeParameterCardinality {
      get { return 0; }
    }

    public override ushort ParentGenericTypeParameterCardinality {
      get { return 0; }
    }

    public override ITypeReference/*?*/ GetGenericTypeParameterFromOrdinal(ushort genericParamOrdinal) {
      return null;
    }

    public override ITypeReference UnderlyingType {
      get {
        if ((this.initFlags & TypeBase.EnumInited) != TypeBase.EnumInited) {
          if (this.IsEnum) {
            foreach (ITypeDefinitionMember tdm in this.GetMembersNamed(this.PEFileToObjectModel.ModuleReader.Value__, false)) {
              var mf = tdm as IFieldDefinition;
              if (mf == null) continue;
              this.enumUnderlyingType = mf.Type;
              break;
            }
            if (this.enumUnderlyingType == null) {
              //TODO: emit error. The module is invalid.
              this.enumUnderlyingType = this.PEFileToObjectModel.PlatformType.SystemInt32;
            }
          } else
            this.enumUnderlyingType = Dummy.TypeReference;
          this.initFlags |= TypeBase.EnumInited;
        }
        return this.enumUnderlyingType;
      }
    }

    #region INamedTypeReference Members

    public sealed override bool MangleName {
      get { return false; }
    }

    #endregion

  }

  internal sealed class NonGenericNamespaceTypeWithoutPrimitiveType : NonGenericNamespaceType {
    internal NonGenericNamespaceTypeWithoutPrimitiveType(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, memberName, typeDefRowId, typeDefFlags, parentModuleNamespace) {
    }

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

  }

  internal sealed class _Module_Type : NonGenericNamespaceType {
    internal _Module_Type(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      Namespace parentModuleNamespace
    )
      : base(peFileToObjectModel, memberName, typeDefRowId, typeDefFlags, parentModuleNamespace) {
    }

    internal override void LoadMembers() {
      lock (GlobalLock.LockingObject) {
        if (this.ContainerState == ContainerState.Loaded)
          return;
        this.StartLoadingMembers();
        Debug.Assert(this == this.PEFileToObjectModel._Module_);
        this.PEFileToObjectModel.LoadMembersOf_Module_Type();
        this.DoneLoadingMembers();
      }
    }

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }
  }

  internal sealed class NonGenericNamespaceTypeWithPrimitiveType : NonGenericNamespaceType {
    MetadataReaderSignatureTypeCode signatureTypeCode;
    internal NonGenericNamespaceTypeWithPrimitiveType(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      Namespace parentModuleNamespace,
      MetadataReaderSignatureTypeCode signatureTypeCode
    )
      : base(peFileToObjectModel, memberName, typeDefRowId, typeDefFlags, parentModuleNamespace) {
      this.signatureTypeCode = signatureTypeCode;
    }

    public override string ToString() {
      switch (this.signatureTypeCode) {
        case MetadataReaderSignatureTypeCode.Boolean: return "System.Boolean";
        case MetadataReaderSignatureTypeCode.Byte: return "System.Byte";
        case MetadataReaderSignatureTypeCode.Char: return "System.Char";
        case MetadataReaderSignatureTypeCode.Double: return "System.Double";
        case MetadataReaderSignatureTypeCode.Int16: return "System.Int16";
        case MetadataReaderSignatureTypeCode.Int32: return "System.Int32";
        case MetadataReaderSignatureTypeCode.Int64: return "System.Int64";
        case MetadataReaderSignatureTypeCode.IntPtr: return "System.IntPtr";
        case MetadataReaderSignatureTypeCode.Object: return "System.Object";
        case MetadataReaderSignatureTypeCode.SByte: return "System.SByte";
        case MetadataReaderSignatureTypeCode.Single: return "System.Single";
        case MetadataReaderSignatureTypeCode.String: return "System.String";
        case MetadataReaderSignatureTypeCode.TypedReference: return "System.TypedReference";
        case MetadataReaderSignatureTypeCode.UInt16: return "System.UInt16";
        case MetadataReaderSignatureTypeCode.UInt32: return "System.UInt32";
        case MetadataReaderSignatureTypeCode.UInt64: return "System.UInt64";
        case MetadataReaderSignatureTypeCode.UIntPtr: return "System.UIntPtr";
        case MetadataReaderSignatureTypeCode.Void: return "System.Void";
      }
      return "unknown primitive type";
    }

    public override PrimitiveTypeCode TypeCode {
      get { return TypeCache.PrimitiveTypeCodeConv[(int)this.signatureTypeCode]; }
    }
  }

  internal sealed class GenericNamespaceType : NamespaceType {
    readonly IName MangledName;
    readonly uint GenericParamRowIdStart;
    readonly uint GenericParamRowIdEnd;
    IGenericTypeInstanceReference/*?*/ genericTypeInstance;

    internal GenericNamespaceType(
      PEFileToObjectModel peFileToObjectModel,
      IName unmangledName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      Namespace parentModuleNamespace,
      IName mangledName,
      uint genericParamRowIdStart,
      uint genericParamRowIdEnd
    )
      : base(peFileToObjectModel, unmangledName, typeDefRowId, typeDefFlags, parentModuleNamespace) {
      this.MangledName = mangledName;
      this.GenericParamRowIdStart = genericParamRowIdStart;
      this.GenericParamRowIdEnd = genericParamRowIdEnd;
    }

    public override IName MangledTypeName {
      get {
        return this.MangledName;
      }
    }

    public override IEnumerable<IGenericTypeParameter> GenericParameters {
      get {
        uint genericRowIdEnd = this.GenericParamRowIdEnd;
        for (uint genericParamIter = this.GenericParamRowIdStart; genericParamIter < genericRowIdEnd; ++genericParamIter) {
          GenericTypeParameter/*?*/ mgtp = this.PEFileToObjectModel.GetGenericTypeParamAtRow(genericParamIter, this);
          yield return mgtp == null ? Dummy.GenericTypeParameter : mgtp;
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

    public override IGenericTypeInstanceReference InstanceType {
      get {
        if (this.genericTypeInstance == null) {
          lock (GlobalLock.LockingObject) {
            if (this.genericTypeInstance == null) {
              ushort genParamCard = this.GenericTypeParameterCardinality;
              var genericParameters = new ITypeReference/*?*/[genParamCard];
              for (ushort i = 0; i < genParamCard; ++i)
                genericParameters[i] = this.GetGenericTypeParameterFromOrdinal(i);
              this.genericTypeInstance = new GenericTypeInstanceReference(this, IteratorHelper.GetReadonly(genericParameters), this.PEFileToObjectModel.InternFactory);
            }
          }
        }
        return this.genericTypeInstance;
      }
    }

    public override ushort GenericTypeParameterCardinality {
      get { return (ushort)(this.GenericParamRowIdEnd - this.GenericParamRowIdStart); }
    }

    public override ushort ParentGenericTypeParameterCardinality {
      get { return 0; }
    }

    public override ITypeReference/*?*/ GetGenericTypeParameterFromOrdinal(ushort genericParamOrdinal) {
      if (genericParamOrdinal >= this.GenericTypeParameterCardinality) {
        //  TODO: MD Error
        return null;
      }
      uint genericRowId = this.GenericParamRowIdStart + genericParamOrdinal;
      return this.PEFileToObjectModel.GetGenericTypeParamAtRow(genericRowId, this);
    }

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    public override ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    #region INamedTypeReference Members

    public override bool MangleName {
      get { return this.Name.UniqueKey != this.MangledName.UniqueKey; }
    }

    #endregion
  }

  internal abstract class NestedType : TypeBase, INestedTypeDefinition {
    internal readonly TypeBase OwningModuleType;
    ITypeReference/*?*/ enumUnderlyingType;
    protected NestedType(
      PEFileToObjectModel peFileToObjectModel,
      IName typeName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      TypeBase parentModuleType
    )
      : base(peFileToObjectModel, typeName, typeDefRowId, typeDefFlags) {
      this.OwningModuleType = parentModuleType;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INestedTypeReference)this);
    }

    public virtual bool DoesNotInheritGenericParameters {
      get { return false; }
    }

    public override ITypeReference/*?*/ UnderlyingType {
      get {
        if ((this.initFlags & TypeBase.EnumInited) != TypeBase.EnumInited) {
          if (this.IsEnum) {
            foreach (ITypeDefinitionMember tdm in this.GetMembersNamed(this.PEFileToObjectModel.ModuleReader.Value__, false)) {
              var/*?*/ mf = tdm as IFieldDefinition;
              if (mf == null)
                continue;
              this.enumUnderlyingType = mf.Type;
              break;
            }
            if (this.enumUnderlyingType == null) {
              //TODO: emit error. The module is invalid.
              this.enumUnderlyingType = this.PEFileToObjectModel.PlatformType.SystemInt32;
            }
          } else
            this.enumUnderlyingType = Dummy.TypeReference;
          this.initFlags |= TypeBase.EnumInited;
        }
        return this.enumUnderlyingType;
      }
    }

    public override IName/*?*/ NamespaceFullName {
      get {
        return null;
      }
    }

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    ushort INestedTypeDefinition.GenericParameterCount {
      get { return this.GenericParameterCount; }
    }

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return this.OwningModuleType; }
    }

    public TypeMemberVisibility Visibility {
      get {
        switch (this.TypeDefFlags & TypeDefFlags.AccessMask) {
          case TypeDefFlags.NestedPublicAccess:
            return TypeMemberVisibility.Public;
          case TypeDefFlags.NestedFamilyAccess:
            return TypeMemberVisibility.Family;
          case TypeDefFlags.NestedAssemblyAccess:
            return TypeMemberVisibility.Assembly;
          case TypeDefFlags.NestedFamilyAndAssemblyAccess:
            return TypeMemberVisibility.FamilyAndAssembly;
          case TypeDefFlags.NestedFamilyOrAssemblyAccess:
            return TypeMemberVisibility.FamilyOrAssembly;
          case TypeDefFlags.NestedPrivateAccess:
          default:
            return TypeMemberVisibility.Private;
        }
      }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return this.OwningModuleType; }
    }

    IName IContainerMember<ITypeDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.OwningModuleType; }
    }

    #endregion

    #region INestedTypeReference Members

    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return this.ContainingTypeDefinition; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }

    #endregion

  }

  internal sealed class NonGenericNestedType : NestedType {

    internal NonGenericNestedType(
      PEFileToObjectModel peFileToObjectModel,
      IName memberName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      TypeBase parentModuleType
    )
      : base(peFileToObjectModel, memberName, typeDefRowId, typeDefFlags, parentModuleType) {
    }

    public override IEnumerable<IGenericTypeParameter> GenericParameters {
      get {
        return Enumerable<IGenericTypeParameter>.Empty;
      }
    }

    public override ushort GenericParameterCount {
      get {
        return 0;
      }
    }

    public override bool IsGeneric {
      get {
        return false;
      }
    }

    public override IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public override ushort GenericTypeParameterCardinality {
      get { return 0; }
    }

    public override ushort ParentGenericTypeParameterCardinality {
      get { return 0; }
    }

    public override ITypeReference/*?*/ GetGenericTypeParameterFromOrdinal(
      ushort genericParamOrdinal
    ) {
      return null;
    }

  }

  internal sealed class GenericNestedType : NestedType {
    readonly IName MangledName;
    internal readonly uint GenericParamRowIdStart;
    internal readonly uint GenericParamRowIdEnd;
    IGenericTypeInstanceReference/*?*/ genericTypeInstance;

    internal GenericNestedType(
      PEFileToObjectModel peFileToObjectModel,
      IName unmangledName,
      uint typeDefRowId,
      TypeDefFlags typeDefFlags,
      TypeBase parentModuleType,
      IName mangledName,
      uint genericParamRowIdStart,
      uint genericParamRowIdEnd
    )
      : base(peFileToObjectModel, unmangledName, typeDefRowId, typeDefFlags, parentModuleType) {
      this.MangledName = mangledName;
      this.GenericParamRowIdStart = genericParamRowIdStart;
      this.GenericParamRowIdEnd = genericParamRowIdEnd;
    }

    public override bool DoesNotInheritGenericParameters {
      get {
        if ((this.initFlags & TypeBase.InheritTypeParametersInited) != TypeBase.InheritTypeParametersInited) {
          this.initFlags |= TypeBase.InheritTypeParametersInited;
          if (this.ContainingParametersAreNotAPrefixOfOwnParameters())
            this.TypeDefFlags |= TypeDefFlags.DoesNotInheritTypeParameters;
        }
        return (this.TypeDefFlags & TypeDefFlags.DoesNotInheritTypeParameters) == TypeDefFlags.DoesNotInheritTypeParameters;
      }
    }

    private bool ContainingParametersAreNotAPrefixOfOwnParameters() {
      var parentCount = this.ParentGenericTypeParameterCardinality;
      if (parentCount == 0) return false;
      var thisCount = this.GenericTypeParameterCardinality;
      if (thisCount < parentCount) return true;
      for (ushort i = 0; i < parentCount; i++) {
        var ownPar = this.PEFileToObjectModel.GetGenericTypeParamAtRow(this.GenericParamRowIdStart+i, this);
        var parentPar = this.OwningModuleType.GetGenericTypeParameterFromOrdinal(i) as GenericParameter;
        if (ownPar == null || parentPar == null) return true;
        if (ownPar.GenericParameterFlags != parentPar.GenericParameterFlags) return true;
        var ownParamConstraintCount = ownPar.GenericParamConstraintCount;
        if (ownParamConstraintCount != parentPar.GenericParamConstraintCount) return true;
      }
      return false;
    }
    
    public override IName MangledTypeName {
      get {
        return this.MangledName;
      }
    }

    public override IEnumerable<IGenericTypeParameter> GenericParameters {
      get {
        ushort offset = this.DoesNotInheritGenericParameters ? (ushort)0 : this.OwningModuleType.GenericTypeParameterCardinality;
        uint genericRowIdEnd = this.GenericParamRowIdEnd;
        for (uint genericParamIter = this.GenericParamRowIdStart + offset; genericParamIter < genericRowIdEnd; ++genericParamIter) {
          GenericTypeParameter/*?*/ mgtp = this.PEFileToObjectModel.GetGenericTypeParamAtRow(genericParamIter, this);
          yield return mgtp == null ? Dummy.GenericTypeParameter : mgtp;
        }
      }
    }

    public override ushort GenericParameterCount {
      get {
        return (ushort)(this.GenericParamRowIdEnd - this.GenericParamRowIdStart - this.OwningModuleType.GenericTypeParameterCardinality);
      }
    }

    public override bool IsGeneric {
      get {
        return this.GenericParameterCount > 0;
      }
    }

    public override IGenericTypeInstanceReference InstanceType {
      get {
        if (this.genericTypeInstance == null) {
          lock (GlobalLock.LockingObject) {
            if (this.genericTypeInstance == null) {
              int argumentsUsed;
              this.genericTypeInstance = (IGenericTypeInstanceReference)this.GetSpecializedTypeReference(this, out argumentsUsed, outer: true);
            }
          }
        }
        return this.genericTypeInstance;
      }
    }

    private ITypeReference GetSpecializedTypeReference(INamedTypeReference nominalType, out int argumentsUsed, bool outer) {
      argumentsUsed = 0;
      int len = this.GenericTypeParameterCardinality;
      var nestedType = nominalType as INestedTypeReference;
      if (nestedType != null) {
        var parentTemplate = this.GetSpecializedTypeReference((INamedTypeReference)nestedType.ContainingType, out argumentsUsed, outer: false);
        if (parentTemplate != nestedType.ContainingType)
          nominalType = new SpecializedNestedTypeReference(nestedType, parentTemplate, this.PEFileToObjectModel.InternFactory);
      }
      var argsToUse = outer ? len-argumentsUsed : nominalType.GenericParameterCount;
      if (argsToUse == 0) return nominalType;
      var genericArgumentsReferences = new ITypeReference[argsToUse];
      for (int i = 0; i < argsToUse; ++i)
        genericArgumentsReferences[i] = this.GetGenericTypeParameterFromOrdinal((ushort)(i+argumentsUsed))??Dummy.TypeReference;
      argumentsUsed += argsToUse;
      return new GenericTypeInstanceReference(nominalType, IteratorHelper.GetReadonly(genericArgumentsReferences), this.PEFileToObjectModel.InternFactory);
    }

    public override ushort GenericTypeParameterCardinality {
      get { return (ushort)(this.GenericParamRowIdEnd - this.GenericParamRowIdStart); }
    }

    public override ushort ParentGenericTypeParameterCardinality {
      get { return this.OwningModuleType.GenericTypeParameterCardinality; }
    }

    public override ITypeReference/*?*/ GetGenericTypeParameterFromOrdinal(
      ushort genericParamOrdinal
    ) {
      if (genericParamOrdinal >= this.GenericTypeParameterCardinality) {
        //  TODO: MD Error
        return null;
      }
      if (genericParamOrdinal < this.ParentGenericTypeParameterCardinality && !this.DoesNotInheritGenericParameters)
        return this.OwningModuleType.GetGenericTypeParameterFromOrdinal(genericParamOrdinal);
      uint genericRowId = this.GenericParamRowIdStart + genericParamOrdinal;
      return this.PEFileToObjectModel.GetGenericTypeParamAtRow(genericRowId, this);
    }

    #region INamedTypeReference Members

    public override bool MangleName {
      get { return this.Name.UniqueKey != this.MangledName.UniqueKey; }
    }

    #endregion
  }

  internal abstract class SignatureGenericParameter : IGenericParameterReference {
    internal readonly PEFileToObjectModel PEFileToObjectModel;

    protected SignatureGenericParameter(PEFileToObjectModel peFileToObjectModel) {
      this.PEFileToObjectModel = peFileToObjectModel;
    }

    #region IGenericParameter Members

    public IEnumerable<ITypeReference> Constraints {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool MustBeReferenceType {
      get { return false; }
    }

    public bool MustBeValueType {
      get { return false; }
    }

    public bool MustHaveDefaultConstructor {
      get { return false; }
    }

    public TypeParameterVariance Variance {
      get { return TypeParameterVariance.Mask; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public bool IsValueType {
      get { return this.MustBeValueType; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Unicode; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasSecurityAttributes {
      get { return false; }
    }

    public IPlatformType PlatformType {
      get { return this.PEFileToObjectModel.PlatformType; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    [Pure]
    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region IParameterListEntry Members

    public abstract ushort Index { get; }

    #endregion

    #region INamedEntity Members

    public abstract IName Name {
      get;
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public abstract ITypeDefinition ResolvedType {
      get;
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    #endregion
  }

  internal sealed class SignatureGenericTypeParameter : SignatureGenericParameter, IGenericTypeParameterReference {
    readonly INamedTypeReference TypeReference;
    readonly ushort GenericParameterIndex;

    internal SignatureGenericTypeParameter(PEFileToObjectModel peFileToObjectModel, ITypeReference typeReference, ushort genericParameterOrdinality)
      : base(peFileToObjectModel) {
      //We get here while parsing the type signature of a member reference. The signature is unspecialized, so this reference
      //must end up referencing a type parameter from the unspecialized containing type of the referenced member.
      //The typeReference parameter, however, references a generic type instance, so we must unspecialize that to get the right value in this.TypeReference.
      this.TypeReference = TypeCache.Unspecialize(typeReference);
      this.GenericParameterIndex = genericParameterOrdinality;

      //Most of the time we should be done now, but....
      //if the namedType is a nested type, then this reference could actually be to a generic type parameter of an ancestor, 
      //so we'll need to update this.TypeReference.
      //On the other hand, this reference might be to a parameter of this.TypeReference, but if any ancestor types also
      //have type parameters, the sum of their type parameter counts must be subtracted from this.GenericParameterIndex
      //because the Metadata model indices are not cummulative like in the PE metadata format.

      //First get the sum of the type parameters of the ancestors
      ushort parentGenericParameterCount = 0;
      var nestedType = this.TypeReference as INestedTypeReference; //Fully unspecialized at this point, so no more generic instances to worry about
      while (nestedType != null) {
        var namespaceContainer = nestedType.ContainingType as INamespaceTypeReference;
        if (namespaceContainer != null) { parentGenericParameterCount += namespaceContainer.GenericParameterCount; break; }
        nestedType = (INestedTypeReference)nestedType.ContainingType;
        parentGenericParameterCount += nestedType.GenericParameterCount;
      }
      while (parentGenericParameterCount > this.GenericParameterIndex) {
        //if we get here, this reference cannot be referring to a parameter of this.TypeReference
        //and this.TypeReference had better be a nested type reference.
        nestedType = (INestedTypeReference)this.TypeReference;
        this.TypeReference = (INamedTypeReference)nestedType.ContainingType;
        parentGenericParameterCount -= this.TypeReference.GenericParameterCount;
      }
      this.GenericParameterIndex -= parentGenericParameterCount;
    }

    private INamedTypeReference Unspecialize(ITypeReference typeReference) {
      var genericTypeInstance = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) {
        var genericType = genericTypeInstance.GenericType;
        var specializedNestedType = genericType as ISpecializedNestedTypeReference;
        if (specializedNestedType != null)
          return specializedNestedType.UnspecializedVersion;
        else
          return genericType;
      } else {
        var specializedNestedType = typeReference as ISpecializedNestedTypeReference;
        if (specializedNestedType != null)
          return specializedNestedType.UnspecializedVersion;
        else
          return (INamedTypeReference)typeReference;
      }
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericTypeParameterReference)this);
    }

    public override ushort Index {
      get { return this.GenericParameterIndex; }
    }

    public override IName Name {
      get {
        if (this.name == null) {
          ITypeDefinition/*?*/ definingType = this.DefiningType;
          IGenericTypeInstanceReference/*?*/ genericInst = definingType as IGenericTypeInstanceReference;
          if (genericInst != null) definingType = genericInst.GenericType.ResolvedType;
          if (definingType.GenericParameterCount <= this.GenericParameterIndex)
            this.name = this.PEFileToObjectModel.NameTable.GetNameFor("!"+this.GenericParameterIndex);
          else {
            int i = 0;
            this.name = Dummy.Name;
            foreach (IGenericTypeParameter par in definingType.GenericParameters) {
              if (i++ < this.GenericParameterIndex) continue;
              this.name = par.Name;
              break;
            }
          }
        }
        return this.name;
      }
    }
    IName name;

    public override ITypeDefinition ResolvedType {
      get {
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        if (this.resolvedType is Dummy) return Dummy.TypeDefinition;
        return this.resolvedType;
      }
    }
    IGenericTypeParameter resolvedType;

    IGenericTypeParameter Resolve() {
      ITypeDefinition definingType = this.DefiningType;
      if (definingType.IsGeneric) {
        ushort index = 0;
        foreach (IGenericTypeParameter genTypePar in definingType.GenericParameters) {
          if (index++ == this.Index) return genTypePar;
        }
      }
      return Dummy.GenericTypeParameter;
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #region IGenericTypeParameter Members

    public ITypeDefinition DefiningType {
      get { return this.TypeReference.ResolvedType; }
    }

    #endregion

    #region IGenericTypeParameterReference Members

    ITypeReference IGenericTypeParameterReference.DefiningType {
      get { return this.TypeReference; }
    }

    IGenericTypeParameter IGenericTypeParameterReference.ResolvedType {
      get {
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        return this.resolvedType;
      }
    }

    #endregion
  }

  internal sealed class SignatureGenericMethodParameter : SignatureGenericParameter, IGenericMethodParameterReference {
    readonly IMethodReference ModuleMethodReference;
    readonly ushort GenericParameterOrdinality;

    internal SignatureGenericMethodParameter(
      PEFileToObjectModel peFileToObjectModel,
      IMethodReference moduleMethodReference,
      ushort genericParameterOrdinality
    )
      : base(peFileToObjectModel) {
      this.ModuleMethodReference = moduleMethodReference;
      this.GenericParameterOrdinality = genericParameterOrdinality;
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericMethodParameterReference)this);
    }

    public override ushort Index {
      get { return this.GenericParameterOrdinality; }
    }

    public override IName Name {
      get {
        if (this.name == null) {
          IMethodDefinition/*?*/ definingMethod = this.DefiningMethod;
          IGenericMethodInstance/*?*/ genericInst = definingMethod as IGenericMethodInstance;
          if (genericInst != null) definingMethod = genericInst.GenericMethod.ResolvedMethod;
          if (definingMethod.GenericParameterCount <= this.GenericParameterOrdinality)
            this.name = this.PEFileToObjectModel.NameTable.GetNameFor("!!"+this.Index);
          else {
            int i = 0;
            this.name = Dummy.Name;
            foreach (IGenericMethodParameter par in definingMethod.GenericParameters) {
              if (i++ < this.GenericParameterOrdinality) continue;
              this.name = par.Name;
              break;
            }
          }
        }
        return this.name;
      }
    }
    IName name;

    public override ITypeDefinition ResolvedType {
      get {
        if (this.resolvedType == null)
          this.Resolve();
        if (this.resolvedType is Dummy) return Dummy.TypeDefinition;
        return this.resolvedType;
      }
    }
    IGenericMethodParameter resolvedType;

    void Resolve() {
      this.resolvedType = Dummy.GenericMethodParameter;
      var definingMethod = this.DefiningMethod;
      if (!definingMethod.IsGeneric || this.Index >= definingMethod.GenericParameterCount) return;
      foreach (var genMethPar in definingMethod.GenericParameters) {
        if (genMethPar.Index == this.Index) { this.resolvedType = genMethPar; return; }
      }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #region IGenericMethodParameter Members

    public IMethodDefinition DefiningMethod {
      get { return this.ModuleMethodReference.ResolvedMethod; }
    }

    #endregion

    #region IGenericMethodParameterReference Members

    IMethodReference IGenericMethodParameterReference.DefiningMethod {
      get { return this.ModuleMethodReference; }
    }

    IGenericMethodParameter IGenericMethodParameterReference.ResolvedType {
      get {
        if (this.resolvedType == null)
          this.Resolve();
        return this.resolvedType;
      }
    }

    #endregion
  }

  internal abstract class SimpleStructuralType : MetadataDefinitionObject, ITypeDefinition {
    uint TypeSpecToken;

    protected SimpleStructuralType(
      PEFileToObjectModel peFileToObjectModel,
      uint typeSpecToken
    )
      : base(peFileToObjectModel) {
      this.TypeSpecToken = typeSpecToken;
    }

    internal override uint TokenValue {
      get { return this.TypeSpecToken; }
    }

    internal void UpdateTypeSpecToken(
      uint typeSpecToken
    )
      //^ requires this.TokenValue == 0xFFFFFFFF;
    {
      this.TypeSpecToken = typeSpecToken;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public virtual IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public virtual IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public virtual bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public virtual bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public virtual PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public virtual bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.AutoChar; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    //^ [Pure]
    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    //^ [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    //^ [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    //^ [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public ITypeDefinition ResolvedType {
      get { return this; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.PEFileToObjectModel.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    IPlatformType ITypeReference.PlatformType {
      get { return this.PlatformType; }
    }

    #endregion

  }

  internal abstract class GenericParameter : SimpleStructuralType, IGenericParameter {
    protected readonly ushort GenericParameterOrdinality;
    internal readonly GenericParamFlags GenericParameterFlags;
    protected readonly IName GenericParameterName;
    internal readonly uint GenericParameterRowId;
    uint genericParamConstraintRowIDStart;
    uint genericParamConstraintRowIDEnd;

    internal GenericParameter(
      PEFileToObjectModel peFileToObjectModel,
      ushort genericParameterOrdinality,
      GenericParamFlags genericParamFlags,
      IName genericParamName,
      uint genericParameterRowId
    )
      : base(peFileToObjectModel, TokenTypeIds.GenericParam | genericParameterRowId) {
      this.GenericParameterOrdinality = genericParameterOrdinality;
      this.GenericParameterFlags = genericParamFlags;
      this.GenericParameterName = genericParamName;
      this.GenericParameterRowId = genericParameterRowId;
      this.genericParamConstraintRowIDStart = 0xFFFFFFFF;
      this.genericParamConstraintRowIDEnd = 0xFFFFFFFF;
    }

    internal uint GenericParamConstraintRowIDStart {
      get {
        if (this.genericParamConstraintRowIDStart == 0xFFFFFFFF) {
          this.PEFileToObjectModel.GetConstraintInfoForGenericParam(this, out this.genericParamConstraintRowIDStart, out this.genericParamConstraintRowIDEnd);
        }
        return this.genericParamConstraintRowIDStart;
      }
    }

    internal uint GenericParamConstraintRowIDEnd {
      get {
        if (this.genericParamConstraintRowIDStart == 0xFFFFFFFF) {
          this.PEFileToObjectModel.GetConstraintInfoForGenericParam(this, out this.genericParamConstraintRowIDStart, out this.genericParamConstraintRowIDEnd);
        }
        return this.genericParamConstraintRowIDEnd;
      }
    }

    internal uint GenericParamConstraintCount {
      get {
        if (this.genericParamConstraintRowIDStart == 0xFFFFFFFF) {
          this.PEFileToObjectModel.GetConstraintInfoForGenericParam(this, out this.genericParamConstraintRowIDStart, out this.genericParamConstraintRowIDEnd);
        }
        return this.genericParamConstraintRowIDEnd - this.genericParamConstraintRowIDStart;
      }
    }

    ushort INamedTypeDefinition.GenericParameterCount {
      get { return this.GenericParameterCount; }
    }

    public override bool IsReferenceType {
      get {
        //  TODO: Do we want to cache the result?
        if (this.MustBeReferenceType) {
          return true;
        }
        if (this.MustBeValueType) {
          return false;
        }
        uint genParamRowIdEnd = this.GenericParamConstraintRowIDEnd;
        for (uint genParamIter = this.GenericParamConstraintRowIDStart; genParamIter < genParamRowIdEnd; ++genParamIter) {
          ITypeReference/*?*/ modTypeRef = this.PEFileToObjectModel.GetTypeReferenceForGenericConstraintRowId(this, genParamIter);
          if (
            modTypeRef == null
            || TypeHelper.TypesAreEquivalent(modTypeRef, this.PEFileToObjectModel.SystemEnum)
            || TypeHelper.TypesAreEquivalent(modTypeRef, this.PEFileToObjectModel.PlatformType.SystemObject)
            || TypeHelper.TypesAreEquivalent(modTypeRef, this.PEFileToObjectModel.SystemValueType)
            || modTypeRef.ResolvedType.IsInterface
          ) {
            continue;
          }
          if (modTypeRef.ResolvedType.IsReferenceType)
            return true;
        }
        return false;
      }
    }

    public override bool IsValueType {
      get { return this.MustBeValueType; }
    }

    #region IGenericParameter Members

    public IEnumerable<ITypeReference> Constraints {
      get {
        uint genParamRowIdEnd = this.GenericParamConstraintRowIDEnd;
        for (uint genParamIter = this.GenericParamConstraintRowIDStart; genParamIter < genParamRowIdEnd; ++genParamIter) {
          ITypeReference/*?*/ typeRef = this.PEFileToObjectModel.GetTypeReferenceForGenericConstraintRowId(this, genParamIter);
          if (typeRef == null) typeRef = Dummy.TypeReference;
          yield return typeRef;
        }
      }
    }

    public bool MustBeReferenceType {
      get { return (this.GenericParameterFlags & GenericParamFlags.ReferenceTypeConstraint) == GenericParamFlags.ReferenceTypeConstraint; }
    }

    public bool MustBeValueType {
      get { return (this.GenericParameterFlags & GenericParamFlags.ValueTypeConstraint) == GenericParamFlags.ValueTypeConstraint; }
    }

    public bool MustHaveDefaultConstructor {
      get { return (this.GenericParameterFlags & GenericParamFlags.DefaultConstructorConstraint) == GenericParamFlags.DefaultConstructorConstraint; }
    }

    public TypeParameterVariance Variance {
      get {
        switch (this.GenericParameterFlags & GenericParamFlags.VarianceMask) {
          case GenericParamFlags.Contravariant:
            return TypeParameterVariance.Contravariant;
          case GenericParamFlags.Covariant:
            return TypeParameterVariance.Covariant;
          case GenericParamFlags.NonVariant:
          default:
            return TypeParameterVariance.NonVariant;
        }
      }
    }

    #endregion

    #region IParameterListEntry Members

    public abstract ushort Index { get; }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.GenericParameterName; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return this; }
    }

    #endregion
  }

  internal sealed class GenericTypeParameter : GenericParameter, IGenericTypeParameter {
    internal readonly TypeBase OwningGenericType;

    internal GenericTypeParameter(
      PEFileToObjectModel peFileToObjectModel,
      ushort genericParameterOrdinality,
      GenericParamFlags genericParamFlags,
      IName genericParamName,
      uint genericParameterRowId,
      TypeBase owningGenericType
    )
      : base(peFileToObjectModel, genericParameterOrdinality, genericParamFlags, genericParamName, genericParameterRowId) {
      this.OwningGenericType = owningGenericType;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericTypeParameterReference)this);
    }

    public override ushort Index {
      get { return (ushort)(this.GenericParameterOrdinality - this.OwningGenericType.ParentGenericTypeParameterCardinality); }
    }

    #region IGenericTypeParameter Members

    public ITypeDefinition DefiningType {
      get {
        return this.OwningGenericType;
      }
    }

    #endregion

    #region IGenericTypeParameterReference Members

    ITypeReference IGenericTypeParameterReference.DefiningType {
      get { return this.DefiningType; }
    }

    IGenericTypeParameter IGenericTypeParameterReference.ResolvedType {
      get { return this; }
    }

    #endregion
  }

  internal sealed class GenericMethodParameter : GenericParameter, IGenericMethodParameter {
    internal readonly GenericMethod OwningGenericMethod;

    internal GenericMethodParameter(
      PEFileToObjectModel peFileToObjectModel,
      ushort genericParameterOrdinality,
      GenericParamFlags genericParamFlags,
      IName genericParamName,
      uint genericParameterRowId,
      GenericMethod owningGenericMethod
    )
      : base(peFileToObjectModel, genericParameterOrdinality, genericParamFlags, genericParamName, genericParameterRowId) {
      this.OwningGenericMethod = owningGenericMethod;
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericMethodParameterReference)this);
    }

    public override ushort Index {
      get { return this.GenericParameterOrdinality; }
    }

    #region IGenericMethodParameter Members

    public IMethodDefinition DefiningMethod {
      get {
        return this.OwningGenericMethod;
      }
    }

    #endregion

    #region IGenericMethodParameterReference Members

    IMethodReference IGenericMethodParameterReference.DefiningMethod {
      get { return this.OwningGenericMethod; }
    }

    IGenericMethodParameter IGenericMethodParameterReference.ResolvedType {
      get { return this; }
    }

    #endregion
  }

  internal sealed class PointerTypeWithToken : PointerType, IMetadataObjectWithToken {

    internal PointerTypeWithToken(uint tokenValue, ITypeReference targetType, IInternFactory internFactory)
      : base(targetType, internFactory) {
      this.tokenValue = tokenValue;
    }

    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal sealed class ManagedPointerTypeWithToken : ManagedPointerType, IMetadataObjectWithToken {

    internal ManagedPointerTypeWithToken(uint tokenValue, ITypeReference targetType, IInternFactory internFactory)
      : base(targetType, internFactory) {
      this.tokenValue = tokenValue;
    }

    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal sealed class VectorWithToken : Vector, IMetadataObjectWithToken {

    internal VectorWithToken(uint tokenValue, ITypeReference elementType, IInternFactory internFactory)
      : base(elementType, internFactory) {
      this.tokenValue = tokenValue;
    }

    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal sealed class MatrixWithToken : Matrix, IMetadataObjectWithToken {

    internal MatrixWithToken(uint tokenValue, ITypeReference elementType, uint rank, IEnumerable<int>/*?*/ lowerBounds, IEnumerable<ulong>/*?*/ sizes, IInternFactory internFactory)
      : base(elementType, rank, lowerBounds, sizes, internFactory) {
      this.tokenValue = tokenValue;
    }

    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal sealed class FunctionPointerTypeWithToken : FunctionPointerType, IMetadataObjectWithToken {

    public FunctionPointerTypeWithToken(uint tokenValue, CallingConvention callingConvention, bool returnValueIsByRef, ITypeReference type,
      IEnumerable<ICustomModifier>/*?*/ returnValueCustomModifiers, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation>/*?*/ extraArgumentTypes,
      IInternFactory internFactory)
      : base(callingConvention, returnValueIsByRef, type, returnValueCustomModifiers, parameters, extraArgumentTypes, internFactory) {
      this.tokenValue = tokenValue;
    }

    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal sealed class GenericTypeInstanceReferenceWithToken : GenericTypeInstanceReference, IMetadataObjectWithToken, ITypeReference {

    public GenericTypeInstanceReferenceWithToken(uint tokenValue, INamedTypeReference genericType, IEnumerable<ITypeReference> genericArguments, IInternFactory internFactory)
      : base(genericType, genericArguments, internFactory) {
      Contract.Requires(!(genericType is Dummy));
      this.tokenValue = tokenValue;
    }

    public uint TokenValue {
      get { return this.tokenValue; }
    }
    uint tokenValue;

  }

  internal abstract class ExportedTypeAliasBase : ScopedContainerMetadataObject<IAliasMember, IAliasMember, IAliasForType>, IAliasForType {
    internal readonly uint ExportedTypeRowId;
    internal readonly TypeDefFlags TypeDefFlags;
    INamedTypeReference/*?*/ aliasTypeReference;

    internal ExportedTypeAliasBase(PEFileToObjectModel peFileToObjectModel, uint exportedTypeDefRowId, TypeDefFlags typeDefFlags)
      : base(peFileToObjectModel) {
      this.ExportedTypeRowId = exportedTypeDefRowId;
      this.TypeDefFlags = typeDefFlags;
    }

    internal override void LoadMembers() {
      lock (GlobalLock.LockingObject) {
        if (this.ContainerState == ContainerState.Loaded)
          return;
        this.StartLoadingMembers();
        this.PEFileToObjectModel.LoadNestedExportedTypesOfAlias(this);
        this.DoneLoadingMembers();
      }
    }

    public ushort GenericParameterCount {
      get { return this.AliasedType.GenericParameterCount; }
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.ExportedType | this.ExportedTypeRowId; }
    }

    #region IAliasForType Members

    public INamedTypeReference AliasedType {
      get {
        if (this.aliasTypeReference == null || this.aliasTypeReference is Dummy) { //if it is a Dummy, perhaps it is being resolved by another thread.
          lock (GlobalLock.LockingObject) {
            if (this.aliasTypeReference == null) { //if it is a Dummy, it can't be resolved.
              this.aliasTypeReference = Dummy.NamedTypeReference; //guard against circular alias chains
              this.aliasTypeReference = this.PEFileToObjectModel.GetReferenceToAliasedType(this)??Dummy.NamedTypeReference;
            }
          }
        }
        return this.aliasTypeReference;
      }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.AliasedType.Name; }
    }

    #endregion

  }

  internal sealed class ExportedTypeNamespaceAlias : ExportedTypeAliasBase, INamespaceAliasForType {
    readonly Namespace ParentModuleNamespace;

    internal ExportedTypeNamespaceAlias(PEFileToObjectModel peFileToObjectModel, uint exportedTypeDefRowId, TypeDefFlags typeDefFlags, Namespace parentModuleNamespace)
      : base(peFileToObjectModel, exportedTypeDefRowId, typeDefFlags) {
      this.ParentModuleNamespace = parentModuleNamespace;
    }

    #region INamespaceAliasForType Members

    public bool IsPublic {
      get { return (this.TypeDefFlags & TypeDefFlags.PublicAccess) == TypeDefFlags.PublicAccess; }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return this.ParentModuleNamespace; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return this.ParentModuleNamespace; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.ParentModuleNamespace; }
    }

    #endregion
  }

  internal sealed class ExportedTypeNestedAlias : ExportedTypeAliasBase, INestedAliasForType {
    readonly ExportedTypeAliasBase ParentExportedTypeAlias;

    internal ExportedTypeNestedAlias(PEFileToObjectModel peFileToObjectModel, uint exportedTypeDefRowId, TypeDefFlags typeDefFlags, ExportedTypeAliasBase parentExportedTypeAlias)
      : base(peFileToObjectModel, exportedTypeDefRowId, typeDefFlags) {
      this.ParentExportedTypeAlias = parentExportedTypeAlias;
    }

    #region IAliasMember Members

    public IAliasForType ContainingAlias {
      get { return this.ParentExportedTypeAlias; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    public TypeMemberVisibility Visibility {
      get {
        switch (this.TypeDefFlags & TypeDefFlags.AccessMask) {
          case TypeDefFlags.NestedPublicAccess:
            return TypeMemberVisibility.Public;
          case TypeDefFlags.NestedFamilyAccess:
            return TypeMemberVisibility.Family;
          case TypeDefFlags.NestedAssemblyAccess:
            return TypeMemberVisibility.Assembly;
          case TypeDefFlags.NestedFamilyAndAssemblyAccess:
            return TypeMemberVisibility.FamilyAndAssembly;
          case TypeDefFlags.NestedFamilyOrAssemblyAccess:
            return TypeMemberVisibility.FamilyOrAssembly;
          case TypeDefFlags.NestedPrivateAccess:
          default:
            return TypeMemberVisibility.Private;
        }
      }
    }

    #endregion

    #region IContainerMember<IAliasForType> Members

    public IAliasForType Container {
      get { return this.ParentExportedTypeAlias; }
    }

    #endregion

    #region IScopeMember<IScope<IAliasMember>> Members

    public IScope<IAliasMember> ContainingScope {
      get { return this.ParentExportedTypeAlias; }
    }

    #endregion
  }

  internal sealed class CustomModifier : ICustomModifier {
    internal readonly bool IsOptional;
    internal readonly ITypeReference Modifier;
    internal CustomModifier(
      bool isOptional,
      ITypeReference modifier
    ) {
      this.IsOptional = isOptional;
      this.Modifier = modifier;
    }

    #region ICustomModifier Members

    bool ICustomModifier.IsOptional {
      get {
        return this.IsOptional;
      }
    }

    ITypeReference ICustomModifier.Modifier {
      get {
        return this.Modifier;
      }
    }

    #endregion
  }

  internal abstract class Parameter : MetadataDefinitionObject, IParameterDefinition {

    internal Parameter(PEFileToObjectModel peFileToObjectModel, int index, IEnumerable<ICustomModifier>/*?*/ customModifiers,
      ITypeReference/*?*/ type, ISignature containingSignature)
      : base(peFileToObjectModel) {
      this.index = (ushort)index;
      this.customModifiers = customModifiers;
      this.type = type;
      this.containingSignature = containingSignature;
    }

    readonly ushort index;
    readonly IEnumerable<ICustomModifier>/*?*/ customModifiers;
    readonly ITypeReference/*?*/ type;
    readonly ISignature containingSignature;

    public override string ToString() {
      return this.Name.Value;
    }

    #region IParameterDefinition Members

    public ISignature ContainingSignature {
      get { return this.containingSignature; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.customModifiers??Enumerable<ICustomModifier>.Empty; }
    }

    public abstract IMetadataConstant DefaultValue { get; }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.VisitReference(this);
    }

    public abstract bool HasDefaultValue { get; }

    public abstract bool IsByReference { get; }

    public abstract bool IsIn { get; }

    [Pure]
    public abstract bool IsMarshalledExplicitly { get; }

    public bool IsModified { get { return this.customModifiers != null; } }

    public abstract bool IsOptional { get; }

    public abstract bool IsOut { get; }

    public abstract bool IsParameterArray { get; }

    public abstract IMarshallingInformation MarshallingInformation { get; }

    public abstract ITypeReference ParamArrayElementType { get; }

    public ITypeReference Type {
      get { return this.type??Dummy.TypeReference; }
    }

    #endregion

    #region INamedEntity Members

    public abstract IName Name { get; }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return this.index; }
    }

    #endregion

    #region IMetadataConstantContainer

    IMetadataConstant IMetadataConstantContainer.Constant {
      get { return this.DefaultValue; }
    }

    #endregion

  }

  internal sealed class ParameterInfo : MetadataObject, IParameterTypeInformation {

    internal ParameterInfo(PEFileToObjectModel peFileToObjectModel, int parameterIndex, IEnumerable<ICustomModifier>/*?*/ moduleCustomModifiers,
      ITypeReference/*?*/ typeReference, ISignature containingSignatureDefinition, bool isByReference)
      : base(peFileToObjectModel) {
      this.index = (ushort)parameterIndex;
      this.customModifiers = moduleCustomModifiers;
      this.type = typeReference;
      this.containingSignature = containingSignatureDefinition;
      this.isByReference = isByReference;
    }

    readonly ushort index;
    readonly IEnumerable<ICustomModifier>/*?*/ customModifiers;
    readonly ITypeReference/*?*/ type;
    readonly ISignature containingSignature;
    readonly bool isByReference;

    #region IParameterTypeInformation Members

    public ISignature ContainingSignature {
      get { return this.containingSignature; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.customModifiers??Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsByReference {
      get { return this.isByReference; }
    }

    public bool IsModified {
      get { return this.customModifiers != null; }
    }

    public ITypeReference Type {
      get { return this.type??Dummy.TypeReference; }
    }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return this.index; }
    }

    #endregion

    internal override uint TokenValue {
      get { return 0xFFFFFFFF; }
    }

    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }
  }

  internal sealed class ParameterWithMetadata : Parameter {
    ParamFlags ParameterFlags;
    IName ParameterName;
    uint ParamRowId;

    internal ParameterWithMetadata(PEFileToObjectModel peFileToObjectModel, int parameterIndex, IEnumerable<ICustomModifier>/*?*/ moduleCustomModifiers,
      ITypeReference/*?*/ typeReference, ISignature containingSignatureDefinition, bool isByReference, bool possibleParamArray,  //  Means that this is last parameter && type is array...
      uint paramRowId, IName parameterName, ParamFlags parameterFlags)
      : base(peFileToObjectModel, parameterIndex, moduleCustomModifiers, typeReference, containingSignatureDefinition) {
      this.ParameterName = parameterName;
      if (isByReference) {
        this.ParameterFlags |= ParamFlags.ByReference;
      }
      this.ParamRowId = paramRowId;
      this.ParameterFlags |= parameterFlags;
      if (possibleParamArray) {
        foreach (ICustomAttribute ica in this.Attributes) {
          CustomAttribute/*?*/ ca = ica as CustomAttribute;
          if (ca == null || !TypeHelper.TypesAreEquivalent(ca.Constructor.ContainingType, peFileToObjectModel.SystemParamArrayAttribute))
            continue;
          this.ParameterFlags |= ParamFlags.ParamArray;
          break;
        }
      }
    }

    internal override uint TokenValue {
      get { return TokenTypeIds.ParamDef | this.ParamRowId; }
    }

    public override IMetadataConstant DefaultValue {
      get { return this.PEFileToObjectModel.GetDefaultValue(this); }
    }

    public override bool HasDefaultValue {
      get { return (this.ParameterFlags & ParamFlags.HasDefaultReserved) == ParamFlags.HasDefaultReserved; }
    }

    public override bool IsByReference {
      get { return (this.ParameterFlags & ParamFlags.ByReference) == ParamFlags.ByReference; }
    }

    public override bool IsIn {
      get { return (this.ParameterFlags & ParamFlags.InSemantics) == ParamFlags.InSemantics; }
    }

    [Pure]
    public override bool IsMarshalledExplicitly {
      get { return (this.ParameterFlags & ParamFlags.HasFieldMarshalReserved) == ParamFlags.HasFieldMarshalReserved; }
    }

    public override bool IsOptional {
      get { return (this.ParameterFlags & ParamFlags.OptionalSemantics) == ParamFlags.OptionalSemantics; }
    }

    public override bool IsOut {
      get { return (this.ParameterFlags & ParamFlags.OutSemantics) == ParamFlags.OutSemantics; }
    }

    public override bool IsParameterArray {
      get { return (this.ParameterFlags & ParamFlags.ParamArray) == ParamFlags.ParamArray; }
    }

    public override IMarshallingInformation MarshallingInformation {
      get { return this.PEFileToObjectModel.GetMarshallingInformation(this); }
    }

    public override ITypeReference ParamArrayElementType {
      get {
        IArrayTypeReference/*?*/ arrayTypeReference = this.Type as IArrayTypeReference;
        if (arrayTypeReference == null || !arrayTypeReference.IsVector)
          return Dummy.TypeReference;
        return arrayTypeReference.ElementType;
      }
    }

    public override IName Name {
      get { return this.ParameterName; }
    }
  }

  internal sealed class ParameterWithoutMetadata : Parameter {

    internal ParameterWithoutMetadata(PEFileToObjectModel peFileToObjectModel, int index, IEnumerable<ICustomModifier>/*?*/ moduleCustomModifiers,
      ITypeReference/*?*/ typeReference, ISignature containingSignature, bool isByReference)
      : base(peFileToObjectModel, index, moduleCustomModifiers, typeReference, containingSignature) {
      this.isByReference = isByReference;
    }

    readonly bool isByReference;

    internal override uint TokenValue {
      get { return 0xFFFFFFFF; }
    }

    public override IMetadataConstant DefaultValue {
      get { return Dummy.Constant; }
    }

    public override bool HasDefaultValue {
      get { return false; }
    }

    public override bool IsByReference {
      get { return this.isByReference; }
    }

    public override bool IsIn {
      get { return false; }
    }

    public override bool IsMarshalledExplicitly {
      get { return false; }
    }

    public override bool IsOptional {
      get { return false; }
    }

    public override bool IsOut {
      get { return false; }
    }

    public override bool IsParameterArray {
      get { return false; }
    }

    public override IMarshallingInformation MarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public override ITypeReference ParamArrayElementType {
      get { return Dummy.TypeReference; }
    }

    public override IName Name {
      get { return Dummy.Name; }
    }
  }

  internal sealed class SpecializedParameter : IParameterDefinition {
    internal readonly IParameterDefinition RawTemplateParameter;
    internal readonly ISignature ContainingSignatureDefinition;
    internal readonly ITypeReference/*?*/ TypeReference;

    internal SpecializedParameter(
      IParameterDefinition rawTemplateParameter,
      ISignature containingSignatureDefinition,
      ITypeReference/*?*/ typeReference
    ) {
      this.RawTemplateParameter = rawTemplateParameter;
      this.ContainingSignatureDefinition = containingSignatureDefinition;
      this.TypeReference = typeReference;
    }

    #region IParameterDefinition Members

    public ISignature ContainingSignature {
      get { return this.ContainingSignatureDefinition; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.RawTemplateParameter.CustomModifiers; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.VisitReference(this);
    }

    public IMetadataConstant DefaultValue {
      get { return this.RawTemplateParameter.DefaultValue; }
    }

    public bool HasDefaultValue {
      get { return this.RawTemplateParameter.HasDefaultValue; }
    }

    public bool IsByReference {
      get { return this.RawTemplateParameter.IsByReference; }
    }

    public bool IsIn {
      get { return this.RawTemplateParameter.IsIn; }
    }

    public bool IsMarshalledExplicitly {
      get { return this.RawTemplateParameter.IsMarshalledExplicitly; }
    }

    public bool IsModified {
      get { return this.RawTemplateParameter.IsModified; }
    }

    public bool IsOptional {
      get { return this.RawTemplateParameter.IsOptional; }
    }

    public bool IsOut {
      get { return this.RawTemplateParameter.IsOut; }
    }

    public bool IsParameterArray {
      get { return this.RawTemplateParameter.IsParameterArray; }
    }

    public IMarshallingInformation MarshallingInformation {
      get { return this.RawTemplateParameter.MarshallingInformation; }
    }

    public ITypeReference ParamArrayElementType {
      get {
        IArrayTypeReference/*?*/ arrayTypeRef = this.TypeReference as IArrayTypeReference;
        if (arrayTypeRef == null || arrayTypeRef is Dummy || !arrayTypeRef.IsVector)
          return Dummy.TypeReference;
        return arrayTypeRef.ElementType;
      }
    }

    public ITypeReference Type {
      get {
        ITypeReference/*?*/ moduleTypeRef = this.TypeReference;
        if (moduleTypeRef == null || moduleTypeRef is Dummy)
          return Dummy.TypeReference;
        return moduleTypeRef;
      }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return this.RawTemplateParameter.Attributes; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return RawTemplateParameter.Name; }
    }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return this.RawTemplateParameter.Index; }
    }

    #endregion

    #region IMetadataConstantContainer

    IMetadataConstant IMetadataConstantContainer.Constant {
      get { return this.DefaultValue; }
    }

    #endregion

  }

  internal sealed class SpecializedParameterInfo : IParameterTypeInformation {
    internal readonly IParameterTypeInformation RawTemplateParameterInfo;
    internal readonly ISignature ContainingSignatureDefinition;
    internal readonly ITypeReference/*?*/ TypeReference;

    internal SpecializedParameterInfo(
      IParameterTypeInformation rawTemplateParameterInfo,
      ISignature containingSignatureDefinition,
      ITypeReference/*?*/ typeReference
    ) {
      this.RawTemplateParameterInfo = rawTemplateParameterInfo;
      this.ContainingSignatureDefinition = containingSignatureDefinition;
      this.TypeReference = typeReference;
    }

    #region IParameterListEntry Members

    public ushort Index {
      get { return this.RawTemplateParameterInfo.Index; }
    }

    #endregion

    #region IParameterTypeInformation Members

    public ISignature ContainingSignature {
      get { return this.ContainingSignatureDefinition; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.RawTemplateParameterInfo.CustomModifiers; }
    }

    public bool IsByReference {
      get { return this.RawTemplateParameterInfo.IsByReference; }
    }

    public bool IsModified {
      get { return this.RawTemplateParameterInfo.IsModified; }
    }

    public ITypeReference Type {
      get {
        if (this.TypeReference == null) return Dummy.TypeReference;
        return this.TypeReference;
      }
    }

    #endregion
  }

  internal sealed class TypeCache {
    internal static readonly byte[] EmptyByteArray = new byte[0];
    internal static PrimitiveTypeCode[] PrimitiveTypeCodeConv = {
      PrimitiveTypeCode.Int8,     //SByte,
      PrimitiveTypeCode.Int16,    //Int16,
      PrimitiveTypeCode.Int32,    //Int32,
      PrimitiveTypeCode.Int64,    //Int64,
      PrimitiveTypeCode.UInt8,    //Byte,
      PrimitiveTypeCode.UInt16,   //UInt16,
      PrimitiveTypeCode.UInt32,   //UInt32,
      PrimitiveTypeCode.UInt64,   //UInt64,
      PrimitiveTypeCode.Float32,  //Single,
      PrimitiveTypeCode.Float64,  //Double,
      PrimitiveTypeCode.IntPtr,   //IntPtr,
      PrimitiveTypeCode.UIntPtr,  //UIntPtr,
      PrimitiveTypeCode.Void,     //Void,
      PrimitiveTypeCode.Boolean,  //Boolean,
      PrimitiveTypeCode.Char,     //Char,
      PrimitiveTypeCode.NotPrimitive,  //Object,
      PrimitiveTypeCode.String,  //String,
      PrimitiveTypeCode.NotPrimitive,  //TypedReference,
      PrimitiveTypeCode.NotPrimitive, //ValueType
      PrimitiveTypeCode.NotPrimitive,  //NotModulePrimitive,
    };
    internal readonly PEFileToObjectModel PEFileToObjectModel;
    Hashtable<ITypeReference> ModuleTypeHashTable;

    internal TypeCache(
      PEFileToObjectModel peFileToObjectModel
    ) {
      this.PEFileToObjectModel = peFileToObjectModel;
      this.ModuleTypeHashTable = new Hashtable<ITypeReference>();
    }

    internal CoreTypeReference CreateCoreTypeReference(
      AssemblyReference coreAssemblyReference,
      NamespaceReference namespaceReference,
      IName typeName,
      MetadataReaderSignatureTypeCode signatureTypeCode
    ) {
      //  No need to look in cache or cache or anything becuase this is called by the constructor.
      return new CoreTypeReference(this.PEFileToObjectModel, coreAssemblyReference, namespaceReference, typeName, 0, signatureTypeCode);
    }
    internal CoreTypeReference CreateCoreTypeReference(
      AssemblyReference coreAssemblyReference,
      NamespaceReference namespaceReference,
      IName typeName,
      ushort genericParameterCount,
      MetadataReaderSignatureTypeCode signatureTypeCode
    ) {
      //  No need to look in cache or cache or anything because this is called by the constructor.
      return new CoreTypeReference(this.PEFileToObjectModel, coreAssemblyReference, namespaceReference, typeName, genericParameterCount, signatureTypeCode);
    }
    static TypeMemberVisibility[,] LUB = {
      //  TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Other,             TypeMemberVisibility.Private,           TypeMemberVisibility.Public
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default  },   //  Default
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Assembly,          TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Assembly,          TypeMemberVisibility.Assembly,          TypeMemberVisibility.Public   },   //  Assembly
      {   TypeMemberVisibility.Default, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Family,            TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Family,            TypeMemberVisibility.Family,            TypeMemberVisibility.Public   },   //  Family
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.Public   },   //  FamilyAndAssembly
      {   TypeMemberVisibility.Default, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Public   },   //  FamilyOrAssembly
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Other,             TypeMemberVisibility.Private,           TypeMemberVisibility.Public   },   //  Other
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Private,           TypeMemberVisibility.Private,           TypeMemberVisibility.Public   },   //  Private
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Public,            TypeMemberVisibility.Public,            TypeMemberVisibility.Public,            TypeMemberVisibility.Public,            TypeMemberVisibility.Public,            TypeMemberVisibility.Public,            TypeMemberVisibility.Public   },   //  Public
    };

    static TypeMemberVisibility[,] GLB = {
      //  TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.Public
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default,           TypeMemberVisibility.Default, TypeMemberVisibility.Default, TypeMemberVisibility.Default            },   //  Default
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.Assembly           },   //  Assembly
      {   TypeMemberVisibility.Default, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.Family,            TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.Family             },   //  Family
      {   TypeMemberVisibility.Default, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.FamilyAndAssembly  },   //  FamilyAndAssembly
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.FamilyOrAssembly   },   //  FamilyOrAssembly
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Other,             TypeMemberVisibility.Other,             TypeMemberVisibility.Other,             TypeMemberVisibility.Other,             TypeMemberVisibility.Other,   TypeMemberVisibility.Other,   TypeMemberVisibility.Other              },   //  Other
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Private,           TypeMemberVisibility.Private,           TypeMemberVisibility.Private,           TypeMemberVisibility.Private,           TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.Private            },   //  Private
      {   TypeMemberVisibility.Default, TypeMemberVisibility.Assembly,          TypeMemberVisibility.Family,            TypeMemberVisibility.FamilyAndAssembly, TypeMemberVisibility.FamilyOrAssembly,  TypeMemberVisibility.Other,   TypeMemberVisibility.Private, TypeMemberVisibility.Public             },   //  Public
    };

    /// <summary>
    /// Least upper bound of the Type member visibility considered as the following lattice:
    ///          Public
    ///      FamilyOrAssembly
    ///    Family        Assembly
    ///      FamilyAndAssembly
    ///          Private
    ///          Other
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    internal static TypeMemberVisibility LeastUpperBound(TypeMemberVisibility left, TypeMemberVisibility right) {
      return TypeCache.LUB[(int)left, (int)right];
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    internal static void SplitMangledTypeName(
      string mangledTypeName,
      out string typeName,
      out ushort genericParamCount
    ) {
      typeName = mangledTypeName;
      genericParamCount = 0;
      int index = mangledTypeName.LastIndexOf('`');
      if (index == -1 || index == mangledTypeName.Length - 1)
        return;
      typeName = mangledTypeName.Substring(0, index);
#if COMPACTFX
      try {
          genericParamCount = ushort.Parse(mangledTypeName.Substring(index + 1, mangledTypeName.Length - index - 1), System.Globalization.NumberStyles.Integer,
              System.Globalization.CultureInfo.InvariantCulture);
      } catch {
      }
#else
      ushort.TryParse(mangledTypeName.Substring(index + 1, mangledTypeName.Length - index - 1), System.Globalization.NumberStyles.Integer,
          System.Globalization.CultureInfo.InvariantCulture, out genericParamCount);
#endif
    }

    internal static INamedTypeReference Unspecialize(ITypeReference typeReference) {
      var genericTypeInstance = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) {
        var genericType = genericTypeInstance.GenericType;
        var specializedNestedType = genericType as ISpecializedNestedTypeReference;
        if (specializedNestedType != null)
          return specializedNestedType.UnspecializedVersion;
        else
          return genericType;
      } else {
        var specializedNestedType = typeReference as ISpecializedNestedTypeReference;
        if (specializedNestedType != null)
          return specializedNestedType.UnspecializedVersion;
        else
          return (INamedTypeReference)typeReference;
      }
    }


  }
}
