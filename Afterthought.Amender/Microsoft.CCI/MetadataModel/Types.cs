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

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// Alias types represent exported and forwarded types, and correspond to entries in the Exported Type table in the ECMA-335 metadata format.
  /// An exported type is an alias for a type defined a member module of the assembly containing the type table, whereas a forwarded type
  /// is an alias for a type defined in another assembly.
  /// </summary>
  //  Consider A.B aliases to C.D, and C.D aliases to E.F.
  //  Then:
  //  typereference(A.B).IsAlias == true && typereference(A.B).AliasForType == aliasfortype(A.B).
  //  aliasfortype(A.B).AliasedType == typereference(C.D).
  //  typereference(C.D).IsAlias == true && typereference(C.D).AliasForType == aliasfortype(C.D).
  //  aliasfortype(C.D).AliasedType == typereference(E.F)
  //  typereference(E.F).IsAlias == false
  //  Also, typereference(A.B).ResolvedType == typereference(C.D).ResolvedType == typereference(E.F).ResolvedType
  [ContractClass(typeof(IAliasForTypeContract))]
  public interface IAliasForType : IContainer<IAliasMember>, IDefinition, INamedEntity, IScope<IAliasMember> {
    /// <summary>
    /// A reference to the type for which this is an alias.
    /// </summary>
    INamedTypeReference AliasedType { get; }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    ushort GenericParameterCount { get; }

    /// <summary>
    /// The collection of member objects comprising the type.
    /// </summary>
    new IEnumerable<IAliasMember> Members { //TODO: this seems to actually be a collection of INestedAliasForType objects.
      get;
    }
  }

  [ContractClassFor(typeof(IAliasForType))]
  abstract class IAliasForTypeContract : IAliasForType {
    public INamedTypeReference AliasedType {
      get {
        Contract.Ensures(Contract.Result<INamedTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    IEnumerable<IAliasMember> IAliasForType.Members {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IAliasMember>>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IAliasMember> Members {
      get {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(IAliasMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<IAliasMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<IAliasMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<IAliasMember> GetMatchingMembers(Function<IAliasMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<IAliasMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }
  }

  /// <summary>
  /// This interface models the metadata representation of an array type.
  /// </summary>
  public interface IArrayType : ITypeDefinition, IArrayTypeReference {
  }

  /// <summary>
  /// This interface models the metadata representation of an array type reference.
  /// </summary>
  [ContractClass(typeof(IArrayTypeReferenceContract))]
  public interface IArrayTypeReference : ITypeReference { //TODO: expose element type custom modifiers

    /// <summary>
    /// The type of the elements of this array.
    /// </summary>
    ITypeReference ElementType { get; }

    /// <summary>
    /// This type of array is a single dimensional array with zero lower bound for index values.
    /// </summary>
    bool IsVector {
      get;
      //^ ensures result ==> Rank == 1;
    }

    /// <summary>
    /// A possibly empty list of lower bounds for dimension indices. When not explicitly specified, a lower bound defaults to zero.
    /// The first lower bound in the list corresponds to the first dimension. Dimensions cannot be skipped.
    /// </summary>
    IEnumerable<int> LowerBounds {
      get;
      // ^ ensures count(result) <= Rank;
    }

    /// <summary>
    /// The number of array dimensions.
    /// </summary>
    uint Rank {
      get;
      //^ ensures result > 0;
    }

    /// <summary>
    /// A possible empty list of upper bounds for dimension indices.
    /// The first upper bound in the list corresponds to the first dimension. Dimensions cannot be skipped.
    /// An unspecified upper bound means that instances of this type can have an arbitrary upper bound for that dimension.
    /// </summary>
    IEnumerable<ulong> Sizes {
      get;
      // ^ ensures count(result) <= Rank;
    }

  }

  [ContractClassFor(typeof(IArrayTypeReference))]
  abstract class IArrayTypeReferenceContract : IArrayTypeReference {

    public ITypeReference ElementType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public bool IsVector {
      get {
        Contract.Ensures(!Contract.Result<bool>() || this.Rank == 1);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<int> LowerBounds {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);
        //Contract.Ensures(IteratorHelper.EnumerableCount(Contract.Result<IEnumerable<int>>()) <= this.Rank);
        throw new NotImplementedException();
      }
    }

    public uint Rank {
      get {
        Contract.Ensures(Contract.Result<uint>() > 0);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ulong> Sizes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ulong>>() != null);
        //Contract.Ensures(IteratorHelper.EnumerableCount(Contract.Result<IEnumerable<int>>()) <= this.Rank);
        throw new NotImplementedException();
      }
    }


    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Modifies the set of allowed values for a type, or the semantics of operations allowed on those values. 
  /// Custom modifiers are not associated directly with types, but rather with typed storage locations for values.
  /// </summary>
  [ContractClass(typeof(ICustomModifierContract))]
  public interface ICustomModifier {

    /// <summary>
    /// If true, a language may use the modified storage location without being aware of the meaning of the modification.
    /// </summary>
    bool IsOptional { get; }

    /// <summary>
    /// A type used as a tag that indicates which type of modification applies to the storage location.
    /// </summary>
    ITypeReference Modifier { get; }

  }

  #region ICustomModifier contract binding
  [ContractClassFor(typeof(ICustomModifier))]
  abstract class ICustomModifierContract : ICustomModifier {
    public bool IsOptional {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference Modifier {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException(); 
      }
    }
  }
  #endregion

  /// <summary>
  /// Information that describes a method or property parameter, but does not include all the information in a IParameterDefinition.
  /// </summary>
  [ContractClass(typeof(IParameterTypeInformationContract))]
  public interface IParameterTypeInformation : IParameterListEntry {

    /// <summary>
    /// The method or property that defines this parameter.
    /// </summary>
    ISignature ContainingSignature { get; }

    /// <summary>
    /// The list of custom modifiers, if any, associated with the parameter. Evaluate this property only if IsModified is true.
    /// </summary>
    IEnumerable<ICustomModifier> CustomModifiers {
      get;
      //^ requires this.IsModified;
    }

    /// <summary>
    /// True if the parameter is passed by reference (using a managed pointer).
    /// </summary>
    bool IsByReference { get; }

    /// <summary>
    /// This parameter has one or more custom modifiers associated with it.
    /// </summary>
    bool IsModified { get; }

    /// <summary>
    /// The type of argument value that corresponds to this parameter.
    /// </summary>
    ITypeReference Type {
      get;
    }
  }

  [ContractClassFor(typeof(IParameterTypeInformation))]
  abstract class IParameterTypeInformationContract : IParameterTypeInformation {
    public ISignature ContainingSignature {
      get {
        Contract.Ensures(Contract.Result<ISignature>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get {
        Contract.Requires(this.IsModified);
        Contract.Ensures(Contract.Result<IEnumerable<ICustomModifier>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICustomModifier>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public bool IsByReference {
      get { throw new NotImplementedException(); }
    }

    public bool IsModified {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference Type {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public ushort Index {
      get { throw new NotImplementedException(); }
    }
  }

  /// <summary>
  /// This interface models the metadata representation of a function pointer type.
  /// </summary>
  public interface IFunctionPointer : IFunctionPointerTypeReference, ITypeDefinition {
  }

  /// <summary>
  /// This interface models the metadata representation of a function pointer type reference.
  /// </summary>
  [ContractClass(typeof(IFunctionPointerTypeReferenceContract))]
  public interface IFunctionPointerTypeReference : ITypeReference, ISignature {

    /// <summary>
    /// The types and modifiers of extra arguments that the caller will pass to the methods that are pointed to by this pointer.
    /// </summary>
    IEnumerable<IParameterTypeInformation> ExtraArgumentTypes { get; }

  }

  [ContractClassFor(typeof(IFunctionPointerTypeReference))]
  abstract class IFunctionPointerTypeReferenceContract : IFunctionPointerTypeReference {
    public IEnumerable<IParameterTypeInformation> ExtraArgumentTypes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IParameterTypeInformation>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IParameterTypeInformation>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public CallingConvention CallingConvention {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IParameterTypeInformation> Parameters {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { throw new NotImplementedException(); }
    }

    public bool ReturnValueIsByRef {
      get { throw new NotImplementedException(); }
    }

    public bool ReturnValueIsModified {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference Type {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// The definition of a type parameter of a generic type or method.
  /// </summary>
  [ContractClass(typeof(IGenericParameterContract))]
  public interface IGenericParameter : INamedTypeDefinition, IParameterListEntry, INamedEntity {
    /// <summary>
    /// A list of classes or interfaces. All type arguments matching this parameter must be derived from all of the classes and implement all of the interfaces.
    /// </summary>
    IEnumerable<ITypeReference> Constraints { get; }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    new ushort GenericParameterCount { get; }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be reference types.
    /// </summary>
    bool MustBeReferenceType {
      get;
      //^ ensures result ==> !this.MustBeValueType;
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be value types.
    /// </summary>
    bool MustBeValueType {
      get;
      //^ ensures result ==> !this.MustBeReferenceType;
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be value types or concrete classes with visible default constructors.
    /// </summary>
    bool MustHaveDefaultConstructor { get; }

    /// <summary>
    /// Indicates if the generic type or method with this type parameter is co-, contra-, or non variant with respect to this type parameter.
    /// </summary>
    TypeParameterVariance Variance { get; }
  }

  [ContractClassFor(typeof(IGenericParameter))]
  abstract class IGenericParameterContract : IGenericParameter {
    public IEnumerable<ITypeReference> Constraints {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ITypeReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeReference>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public bool MustBeReferenceType {
      get {
        Contract.Ensures(!Contract.Result<bool>() || !this.MustBeValueType);
        throw new NotImplementedException();
      }
    }

    public bool MustBeValueType {
      get {
        Contract.Ensures(!Contract.Result<bool>() || !this.MustBeReferenceType);
        throw new NotImplementedException();
      }
    }

    public bool MustHaveDefaultConstructor {
      get { throw new NotImplementedException(); }
    }

    public TypeParameterVariance Variance {
      get { throw new NotImplementedException(); }
    }

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ushort Index {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to the definition of a type parameter of a generic type or method.
  /// </summary>
  public interface IGenericParameterReference : ITypeReference, INamedEntity, IParameterListEntry {
  }

  /// <summary>
  /// The definition of a type parameter of a generic method.
  /// </summary>
  [ContractClass(typeof(IGenericMethodParameterContract))]
  public interface IGenericMethodParameter : IGenericParameter, IGenericMethodParameterReference {

    /// <summary>
    /// The generic method that defines this type parameter.
    /// </summary>
    new IMethodDefinition DefiningMethod {
      get;
      //^ ensures result.IsGeneric;
    }

  }

  [ContractClassFor(typeof(IGenericMethodParameter))]
  abstract class IGenericMethodParameterContract : IGenericMethodParameter {
    public IMethodDefinition DefiningMethod {
      get {
        Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ITypeReference> Constraints {
      get { throw new NotImplementedException(); }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public bool MustBeReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool MustBeValueType {
      get { throw new NotImplementedException(); }
    }

    public bool MustHaveDefaultConstructor {
      get { throw new NotImplementedException(); }
    }

    public TypeParameterVariance Variance {
      get { throw new NotImplementedException(); }
    }

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ushort Index {
      get { throw new NotImplementedException(); }
    }

    IMethodReference IGenericMethodParameterReference.DefiningMethod {
      get { throw new NotImplementedException(); }
    }

    IGenericMethodParameter IGenericMethodParameterReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to a type parameter of a generic method.
  /// </summary>
  [ContractClass(typeof(IGenericMethodParameterReferenceContract))]
  public interface IGenericMethodParameterReference : IGenericParameterReference {

    /// <summary>
    /// A reference to the generic method that defines the referenced type parameter.
    /// </summary>
    IMethodReference DefiningMethod { get; }

    /// <summary>
    /// The generic method parameter this reference resolves to.
    /// </summary>
    new IGenericMethodParameter ResolvedType { get; }
  }

  [ContractClassFor(typeof(IGenericMethodParameterReference))]
  abstract class IGenericMethodParameterReferenceContract : IGenericMethodParameterReference {
    public IMethodReference DefiningMethod {
      get {
        Contract.Ensures(Contract.Result<IMethodReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IGenericMethodParameter ResolvedType {
      get {
        Contract.Ensures(Contract.Result<IGenericMethodParameter>() != null);
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ushort Index {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A generic type instantiated with a list of type arguments
  /// </summary>
  public interface IGenericTypeInstance : IGenericTypeInstanceReference, ITypeDefinition {
  }

  /// <summary>
  /// A generic type instantiated with a list of type arguments
  /// </summary>
  [ContractClass(typeof(IGenericTypeInstanceReferenceContract))]
  public interface IGenericTypeInstanceReference : ITypeReference {

    /// <summary>
    /// The type arguments that were used to instantiate this.GenericType in order to create this type.
    /// </summary>
    IEnumerable<ITypeReference> GenericArguments {
      get;
      // ^ ensures result.GetEnumerator().MoveNext(); //The collection is always non empty.
    }

    /// <summary>
    /// Returns the generic type of which this type is an instance.
    /// </summary>
    INamedTypeReference GenericType {
      get;
      //^ ensures result.ResolveType == Dummy.NamedTypeReference || result.ResolvedType.IsGeneric;
    }

  }

  [ContractClassFor(typeof(IGenericTypeInstanceReference))]
  abstract class IGenericTypeInstanceReferenceContract : IGenericTypeInstanceReference {
    public IEnumerable<ITypeReference> GenericArguments {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ITypeReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeReference>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public INamedTypeReference GenericType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        //Contract.Ensures(Contract.Result<ITypeReference>().ResolvedType == Dummy.NamedTypeReference || Contract.Result<ITypeReference>().ResolvedType.IsGeneric);
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// The definition of a type parameter of a generic type.
  /// </summary>
  [ContractClass(typeof(IGenericTypeParameterContract))]
  public interface IGenericTypeParameter : IGenericParameter, IGenericTypeParameterReference {

    /// <summary>
    /// The generic type that defines this type parameter.
    /// </summary>
    new ITypeDefinition DefiningType { get; }

  }

  #region IGenericTypeParameter contract binding
  [ContractClassFor(typeof(IGenericTypeParameter))]
  abstract class IGenericTypeParameterContract : IGenericTypeParameter {
    public ITypeDefinition DefiningType {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IEnumerable<ITypeReference> Constraints {
      get { throw new NotImplementedException(); }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public bool MustBeReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool MustBeValueType {
      get { throw new NotImplementedException(); }
    }

    public bool MustHaveDefaultConstructor {
      get { throw new NotImplementedException(); }
    }

    public TypeParameterVariance Variance {
      get { throw new NotImplementedException(); }
    }

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ushort Index {
      get { throw new NotImplementedException(); }
    }

    ITypeReference IGenericTypeParameterReference.DefiningType {
      get { throw new NotImplementedException(); }
    }

    IGenericTypeParameter IGenericTypeParameterReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }
  }
  #endregion


  /// <summary>
  /// A reference to a type parameter of a generic type.
  /// </summary>
  [ContractClass(typeof(IGenericTypeParameterReferenceContract))]
  public interface IGenericTypeParameterReference : IGenericParameterReference {

    /// <summary>
    /// A reference to the generic type that defines the referenced type parameter.
    /// </summary>
    ITypeReference DefiningType { get; }

    /// <summary>
    /// The generic type parameter this reference resolves to.
    /// </summary>
    new IGenericTypeParameter ResolvedType { get; }

  }

  [ContractClassFor(typeof(IGenericTypeParameterReference))]
  abstract class IGenericTypeParameterReferenceContract : IGenericTypeParameterReference {
    public ITypeReference DefiningType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IGenericTypeParameter ResolvedType {
      get {
        Contract.Ensures(Contract.Result<IGenericTypeParameter>() != null);
        Contract.Ensures(!(this is IGenericTypeParameter) || Contract.Result<IGenericTypeParameter>() == this);
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ushort Index {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to a named type, such as an INamespaceTypeReference or an INestedTypeReference.
  /// </summary>
  [ContractClass(typeof(INamedTypeReferenceContract))]
  public interface INamedTypeReference : ITypeReference, INamedEntity {

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    ushort GenericParameterCount { get; }

    /// <summary>
    /// If true, the persisted type name is mangled by appending "`n" where n is the number of type parameters, if the number of type parameters is greater than 0.
    /// </summary>
    bool MangleName { get; }

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    new INamedTypeDefinition ResolvedType {
      get;
      //^ ensures this.IsAlias ==> result == this.AliasForType.AliasedType.ResolvedType;
      //^ ensures (this is INamedTypeDefinition) ==> result == this;
    }

  }

  #region INamedTypeReference contract binding
  [ContractClassFor(typeof(INamedTypeReference))]
  abstract class INamedTypeReferenceContract : INamedTypeReference {
    #region INamedTypeReference Members

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    public INamedTypeDefinition ResolvedType {
      get {
        Contract.Ensures(Contract.Result<INamedTypeDefinition>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A named type definition, such as an INamespaceTypeDefinition or an INestedTypeDefinition.
  /// </summary>
  [ContractClass(typeof(INamedTypeDefinitionContract))]
  public interface INamedTypeDefinition : ITypeDefinition, INamedTypeReference {

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    new ushort GenericParameterCount { get; }

  }

  #region INamedTypeDefinition contract binding
  [ContractClassFor(typeof(INamedTypeDefinition))]
  abstract class INamedTypeDefinitionContract : INamedTypeDefinition {
    ushort INamedTypeDefinition.GenericParameterCount {
      get { 
        Contract.Ensures(this.IsGeneric || Contract.Result<ushort>() == 0);
        Contract.Ensures(!this.IsGeneric || Contract.Result<ushort>() > 0);
        throw new NotImplementedException();
      }
    }

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #region ITypeDefinition Members


    ushort ITypeDefinition.GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedTypeReference Members

    ushort INamedTypeReference.GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion

  /// <summary>
  /// A type definition that is a member of a namespace definition.
  /// </summary>
  [ContractClass(typeof(INamespaceTypeDefinitionContract))]
  public interface INamespaceTypeDefinition : INamedTypeDefinition, INamespaceMember, INamespaceTypeReference {

    /// <summary>
    /// Returns a potentially empty enumeration of custom attributes that describe how this type implements the given interface.
    /// </summary>
    IEnumerable<ICustomAttribute> AttributesFor(ITypeReference implementedInterface);
    //^ requires IteratorHelper.EnumerableContains(this.Interfaces, implementedInterface);

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    new ushort GenericParameterCount { get; }

    /// <summary>
    /// The namespace that contains this member.
    /// </summary>
    new IUnitNamespace ContainingUnitNamespace { get; }

    /// <summary>
    /// True if the type can be accessed from other assemblies.
    /// </summary>
    bool IsPublic { get; }

    /// <summary>
    /// True if objects of this type are neither COM objects nor native to the CLR and are accessed via some kind of interoperation mechanism.
    /// </summary>
    bool IsForeignObject { get; }

  }

  [ContractClassFor(typeof(INamespaceTypeDefinition))]
  abstract class INamespaceTypeDefinitionContract : INamespaceTypeDefinition {
    #region INamespaceTypeDefinition Members

    public IEnumerable<ICustomAttribute> AttributesFor(ITypeReference implementedInterface) {
      Contract.Requires(IteratorHelper.EnumerableContains(this.Interfaces, implementedInterface));
      Contract.Ensures(Contract.Result<IEnumerable<ICustomAttribute>>() != null);
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICustomAttribute>>(), x => x != null));
      throw new NotImplementedException();
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public IUnitNamespace ContainingUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespace>() != null);
        throw new NotImplementedException(); 
      }
    }

    public bool IsPublic {
      get { throw new NotImplementedException(); }
    }

    public bool IsForeignObject {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    #endregion

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public bool KeepDistinctFromDefinition {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedTypeReference Members


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamespaceTypeReference Members

    IUnitNamespaceReference INamespaceTypeReference.ContainingUnitNamespace {
      get { throw new NotImplementedException(); }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a type definition that is a member of a namespace definition.
  /// </summary>
  [ContractClass(typeof(INamespaceTypeReferenceContract))]
  public interface INamespaceTypeReference : INamedTypeReference {

    /// <summary>
    /// The namespace that contains the referenced type.
    /// </summary>
    IUnitNamespaceReference ContainingUnitNamespace { get; }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    new ushort GenericParameterCount { get; }

    /// <summary>
    /// The namespace type this reference resolves to.
    /// </summary>
    new INamespaceTypeDefinition ResolvedType { get; }

    /// <summary>
    /// True if this reference should be kept distinct from the definition it refers to. That is, when copied or persisted,
    /// this object should not be unified with the referenced type, even if the referenced type is defined in the same
    /// module as the reference to the type.
    /// </summary>
    /// <remarks>
    /// This is a somewhat strange property and there is no corresponding bit in the metadata. It serves to
    /// constrain the way in which metadata is persisted in order to enable a particular implementation technique for
    /// substituting one type for another at runtime. This bit needs to be set only if there is a need to exploit this
    /// implementation technique. Metadata readers will set it whenever they encounter a distinct reference (a TypeRef token)
    /// when the definition (a TypeDef token) could have been used instead. Metadata writers will not substitute
    /// TypeDef tokens when they persist references with this property set to true.
    /// </remarks>
    bool KeepDistinctFromDefinition { get; }

  }

  [ContractClassFor(typeof(INamespaceTypeReference))]
  abstract class INamespaceTypeReferenceContract : INamespaceTypeReference {
    public IUnitNamespaceReference ContainingUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespaceReference>() != null);
        throw new NotImplementedException();
      }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public INamespaceTypeDefinition ResolvedType {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeDefinition>() != null);
        throw new NotImplementedException();
      }
    }

    public bool KeepDistinctFromDefinition {
      get { throw new NotImplementedException(); }
    }

    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Represents an alias type in a namespace
  /// </summary>
  public interface INamespaceAliasForType : IAliasForType, INamespaceMember {
    /// <summary>
    /// True if the type can be accessed from other assemblies.
    /// </summary>
    bool IsPublic { get; }
  }

  /// <summary>
  /// A type definition that is a member of another type definition.
  /// </summary>
  [ContractClass(typeof(INestedTypeDefinitionContract))]
  public interface INestedTypeDefinition : INamedTypeDefinition, ITypeDefinitionMember, INestedTypeReference {

    /// <summary>
    /// If true, the type does not inherit generic parameters from its containing type.
    /// </summary>
    bool DoesNotInheritGenericParameters {
      get;
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    new ushort GenericParameterCount { get; }
  }

  #region INestedTypeDefinition contract binding
  [ContractClassFor(typeof(INestedTypeDefinition))]
  abstract class INestedTypeDefinitionContract : INestedTypeDefinition {

    #region INestedTypeDefinition Members

    ushort INestedTypeDefinition.GenericParameterCount {
      get {
        Contract.Ensures(this.IsGeneric || Contract.Result<ushort>() == 0);
        Contract.Ensures(!this.IsGeneric || Contract.Result<ushort>() > 0);
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public bool DoesNotInheritGenericParameters {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    #endregion

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedTypeReference Members


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { throw new NotImplementedException(); }
    }

    public TypeMemberVisibility Visibility {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INestedTypeReference Members


    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedTypeDefinition Members

    ushort INamedTypeDefinition.GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ITypeDefinition Members


    ushort ITypeDefinition.GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedTypeReference Members

    ushort INamedTypeReference.GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INestedTypeReference Members

    ushort INestedTypeReference.GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion

  /// <summary>
  /// A type definition that is a member of another type definition.
  /// </summary>
  [ContractClass(typeof(INestedTypeReferenceContract))]
  public interface INestedTypeReference : INamedTypeReference, ITypeMemberReference {

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    new ushort GenericParameterCount { get; }

    /// <summary>
    /// The nested type this reference resolves to.
    /// </summary>
    new INestedTypeDefinition ResolvedType { get; }

  }

  [ContractClassFor(typeof(INestedTypeReference))]
  abstract class INestedTypeReferenceContract : INestedTypeReference {
    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public INestedTypeDefinition ResolvedType {
      get {
        Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);
        Contract.Ensures(!(this is ITypeDefinition) || Contract.Result<ITypeDefinition>() == this);
        throw new NotImplementedException();
      }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference ContainingType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Represents an alias type in a type.
  /// </summary>
  public interface INestedAliasForType : IAliasForType, IAliasMember {
  }

  /// <summary>
  /// A type definition that is a specialized nested type. That is, the type definition is a member of a generic type instance, or of another specialized nested type.
  /// It is specialized, because if it had any references to the type parameters of the generic type, then those references have been replaced with the type arguments of the instance.
  /// In other words, it may be less generic than before, and hence it has been "specialized".
  /// </summary>
  [ContractClass(typeof(ISpecializedNestedTypeDefinitionContract))]
  public interface ISpecializedNestedTypeDefinition : INestedTypeDefinition, ISpecializedNestedTypeReference {

    /// <summary>
    /// The nested type that has been specialized to obtain this nested type. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    new INestedTypeDefinition/*!*/ UnspecializedVersion {
      get;
    }

  }

  #region ISpecializedNestedTypeDefinition contract binding
  [ContractClassFor(typeof(ISpecializedNestedTypeDefinition))]
  abstract class ISpecializedNestedTypeDefinitionContract : ISpecializedNestedTypeDefinition {
    public INestedTypeDefinition UnspecializedVersion {
      get {
        Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);
        throw new NotImplementedException(); 
      }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { throw new NotImplementedException(); }
    }

    public bool DoesNotInheritGenericParameters {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { throw new NotImplementedException(); }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ContainingTypeDefinition {
      get { throw new NotImplementedException(); }
    }

    public TypeMemberVisibility Visibility {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference ContainingType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition Container {
      get { throw new NotImplementedException(); }
    }

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }


    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    INestedTypeReference ISpecializedNestedTypeReference.UnspecializedVersion {
      get { throw new NotImplementedException(); }
    }
  }
  #endregion

  /// <summary>
  /// A reference to a type definition that is a specialized nested type.
  /// </summary>
  [ContractClass(typeof(ISpecializedNestedTypeReferenceContract))]
  public interface ISpecializedNestedTypeReference : INestedTypeReference {

    /// <summary>
    /// A reference to the nested type that has been specialized to obtain this nested type reference. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    INestedTypeReference UnspecializedVersion {
      get;
    }

  }

  [ContractClassFor(typeof(ISpecializedNestedTypeReference))]
  abstract class ISpecializedNestedTypeReferenceContract : ISpecializedNestedTypeReference {
    public INestedTypeReference UnspecializedVersion {
      get {
        Contract.Ensures(Contract.Result<INestedTypeReference>() != null);
        Contract.Ensures(!(Contract.Result<INestedTypeReference>() is ISpecializedNestedTypeReference));
        Contract.Ensures(!(Contract.Result<INestedTypeReference>().ContainingType is ISpecializedNestedTypeReference));
        Contract.Ensures(!(Contract.Result<INestedTypeReference>().ContainingType is IGenericTypeInstanceReference));
        throw new NotImplementedException();
      }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public INestedTypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }


    public bool MangleName {
      get { throw new NotImplementedException(); }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference ContainingType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Models an explicit implementation or override of a base class virtual method or an explicit implementation of an interface method.
  /// </summary>
  [ContractClass(typeof(IMethodImplementationContract))]
  public interface IMethodImplementation {

    /// <summary>
    /// The type that is explicitly implementing or overriding the base class virtual method or explicitly implementing an interface method.
    /// </summary>
    ITypeDefinition ContainingType { get; }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IDefinition. The dispatch method does not invoke Dispatch on any child objects. If child traversal
    /// is desired, the implementations of the Visit methods should do the subsequent dispatching.
    /// </summary>
    void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// A reference to the method whose implementation is being provided or overridden.
    /// </summary>
    IMethodReference ImplementedMethod { get; }

    /// <summary>
    /// A reference to the method that provides the implementation.
    /// </summary>
    IMethodReference ImplementingMethod { get; }
  }

  #region IMethodImplementation contract binding
  [ContractClassFor(typeof(IMethodImplementation))]
  abstract class IMethodImplementationContract : IMethodImplementation {
    #region IMethodImplementation Members

    public ITypeDefinition ContainingType {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
        throw new NotImplementedException(); 
      }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      Contract.Requires(visitor != null);
      throw new NotImplementedException();
    }

    public IMethodReference ImplementedMethod {
      get {
        Contract.Ensures(Contract.Result<IMethodReference>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IMethodReference ImplementingMethod {
      get {
        Contract.Ensures(Contract.Result<IMethodReference>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion
  }
  #endregion

  /// <summary>
  /// A type reference that has custom modifiers associated with it. For example a reference to the target type of a managed pointer to a constant.
  /// </summary>
  [ContractClass(typeof(IModifiedTypeReferenceContract))]
  public interface IModifiedTypeReference : ITypeReference {

    /// <summary>
    /// Returns the list of custom modifiers associated with the type reference. Evaluate this property only if IsModified is true.
    /// </summary>
    IEnumerable<ICustomModifier> CustomModifiers { get; }

    /// <summary>
    /// An unmodified type reference.
    /// </summary>
    ITypeReference UnmodifiedType { get; }

  }

  [ContractClassFor(typeof(IModifiedTypeReference))]
  abstract class IModifiedTypeReferenceContract : IModifiedTypeReference {
    public IEnumerable<ICustomModifier> CustomModifiers {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ICustomModifier>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICustomModifier>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public ITypeReference UnmodifiedType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A collection of references to types from the core platform, such as System.Object and System.String.
  /// </summary>
  [ContractClass(typeof(IPlatformTypeContract))]
  public interface IPlatformType {

    /// <summary>
    /// A reference to the class that contains the standard contract methods, such as System.Diagnostics.Contracts.Contract.Requires.
    /// </summary>
    INamespaceTypeReference SystemDiagnosticsContractsContract { get; }

    /// <summary>
    /// The size (in bytes) of a pointer on the platform on which these types are implemented.
    /// The value of this property is either 4 (32-bits) or 8 (64-bit).
    /// </summary>
    byte PointerSize {
      get;
      //^ ensures result == 4 || result == 8;
    }

    /// <summary>
    /// System.ArgIterator
    /// </summary>
    INamespaceTypeReference SystemArgIterator { get; }

    /// <summary>
    /// System.Array
    /// </summary>
    INamespaceTypeReference SystemArray { get; }

    /// <summary>
    /// System.AsyncCallBack
    /// </summary>
    INamespaceTypeReference SystemAsyncCallback { get; }

    /// <summary>
    /// System.Attribute
    /// </summary>
    INamespaceTypeReference SystemAttribute { get; }

    /// <summary>
    /// System.AttributeUsageAttribute
    /// </summary>
    INamespaceTypeReference SystemAttributeUsageAttribute { get; }

    /// <summary>
    /// System.Boolean
    /// </summary>
    INamespaceTypeReference SystemBoolean { get; }

    /// <summary>
    /// System.Char
    /// </summary>
    INamespaceTypeReference SystemChar { get; }

    /// <summary>
    /// System.Collections.Generic.Dictionary
    /// </summary>
    INamespaceTypeReference SystemCollectionsGenericDictionary { get; }

    /// <summary>
    /// System.Collections.Generic.ICollection
    /// </summary>
    INamespaceTypeReference SystemCollectionsGenericICollection { get; }

    /// <summary>
    /// System.Collections.Generic.IEnumerable
    /// </summary>
    INamespaceTypeReference SystemCollectionsGenericIEnumerable { get; }

    /// <summary>
    /// System.Collections.Generic.IEnumerator
    /// </summary>
    INamespaceTypeReference SystemCollectionsGenericIEnumerator { get; }

    /// <summary>
    /// System.Collections.Generic.IList
    /// </summary>
    INamespaceTypeReference SystemCollectionsGenericIList { get; }

    /// <summary>
    /// System.Collections.ICollection
    /// </summary>
    INamespaceTypeReference SystemCollectionsICollection { get; }

    /// <summary>
    /// System.Collections.IEnumerable
    /// </summary>
    INamespaceTypeReference SystemCollectionsIEnumerable { get; }

    /// <summary>
    /// System.Collections.IEnumerator
    /// </summary>
    INamespaceTypeReference SystemCollectionsIEnumerator { get; }

    /// <summary>
    /// System.Collections.IList
    /// </summary>
    INamespaceTypeReference SystemCollectionsIList { get; }

    /// <summary>
    /// System.Collections.IStructuralComparable
    /// </summary>
    INamespaceTypeReference SystemCollectionsIStructuralComparable { get; }

    /// <summary>
    /// System.Collections.IStructuralEquatable
    /// </summary>
    INamespaceTypeReference SystemCollectionsIStructuralEquatable { get; }

    /// <summary>
    /// System.DateTime
    /// </summary>
    INamespaceTypeReference SystemDateTime { get; }

    /// <summary>
    /// System.DateTimeOffset
    /// </summary>
    INamespaceTypeReference SystemDateTimeOffset { get; }

    /// <summary>
    /// System.Decimal
    /// </summary>
    INamespaceTypeReference SystemDecimal { get; }

    /// <summary>
    /// System.Delegate
    /// </summary>
    INamespaceTypeReference SystemDelegate { get; }

    /// <summary>
    /// System.DBNull
    /// </summary>
    INamespaceTypeReference SystemDBNull { get; }

    /// <summary>
    /// System.Enum
    /// </summary>
    INamespaceTypeReference SystemEnum { get; }

    /// <summary>
    /// System.Exception
    /// </summary>
    INamespaceTypeReference SystemException { get; }

    /// <summary>
    /// System.Float32
    /// </summary>
    INamespaceTypeReference SystemFloat32 { get; }

    /// <summary>
    /// System.Float64
    /// </summary>
    INamespaceTypeReference SystemFloat64 { get; }

    /// <summary>
    /// System.Globalization.CultureInfo
    /// </summary>
    INamespaceTypeReference SystemGlobalizationCultureInfo { get; }

    /// <summary>
    /// System.IAsyncResult
    /// </summary>
    INamespaceTypeReference SystemIAsyncResult { get; }

    /// <summary>
    /// System.ICloneable
    /// </summary>
    INamespaceTypeReference SystemICloneable { get; }

    /// <summary>
    /// System.ContextStaticAttribute
    /// </summary>
    INamespaceTypeReference SystemContextStaticAttribute { get; }

    /// <summary>
    /// System.Int16
    /// </summary>
    INamespaceTypeReference SystemInt16 { get; }

    /// <summary>
    /// System.Int32
    /// </summary>
    INamespaceTypeReference SystemInt32 { get; }

    /// <summary>
    /// System.Int64
    /// </summary>
    INamespaceTypeReference SystemInt64 { get; }

    /// <summary>
    /// System.Int8
    /// </summary>
    INamespaceTypeReference SystemInt8 { get; }

    /// <summary>
    /// System.IntPtr
    /// </summary>
    INamespaceTypeReference SystemIntPtr { get; }

    /// <summary>
    /// System.MulticastDelegate
    /// </summary>
    INamespaceTypeReference SystemMulticastDelegate { get; }

    /// <summary>
    /// System.Nullable&lt;T&gt;
    /// </summary>
    INamespaceTypeReference SystemNullable { get; }

    /// <summary>
    /// System.Object
    /// </summary>
    INamespaceTypeReference SystemObject { get; }

    /// <summary>
    /// System.Reflection.AssemblySignatureKeyAttribute
    /// </summary>
    INamespaceTypeReference SystemReflectionAssemblySignatureKeyAttribute { get; }

    /// <summary>
    /// System.RuntimeArgumentHandle
    /// </summary>
    INamespaceTypeReference SystemRuntimeArgumentHandle { get; }

    /// <summary>
    /// System.RuntimeFieldHandle
    /// </summary>
    INamespaceTypeReference SystemRuntimeFieldHandle { get; }

    /// <summary>
    /// System.RuntimeMethodHandle
    /// </summary>
    INamespaceTypeReference SystemRuntimeMethodHandle { get; }

    /// <summary>
    /// System.RuntimeTypeHandle
    /// </summary>
    INamespaceTypeReference SystemRuntimeTypeHandle { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.CallConvCdecl
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesCallConvCdecl { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.CompilerGeneratedAttribute
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.ExtensionAttribute
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesExtensionAttribute { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.InternalsVisibleToAttribute
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesInternalsVisibleToAttribute { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.IsCont
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesIsConst { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.IsVolatile
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesIsVolatile { get; }

    /// <summary>
    /// System.Runtime.CompilerServices.ReferenceAssemblyAttribute
    /// </summary>
    INamespaceTypeReference SystemRuntimeCompilerServicesReferenceAssemblyAttribute { get; }

    /// <summary>
    /// System.Runtime.InteropServices.DllImportAttribute
    /// </summary>
    INamespaceTypeReference SystemRuntimeInteropServicesDllImportAttribute { get; }

    /// <summary>
    /// System.Runtime.InteropServices.TypeIdentifierAttribute
    /// </summary>
    INamespaceTypeReference SystemRuntimeInteropServicesTypeIdentifierAttribute { get; }

    /// <summary>
    /// System.Security.Permissions.SecurityAction
    /// </summary>
    INamespaceTypeReference SystemSecurityPermissionsSecurityAction { get; }

    /// <summary>
    /// System.Security.SecurityCriticalAttribute
    /// </summary>
    INamespaceTypeReference SystemSecuritySecurityCriticalAttribute { get; }

    /// <summary>
    /// System.Security.SecuritySafeCriticalAttribute
    /// </summary>
    INamespaceTypeReference SystemSecuritySecuritySafeCriticalAttribute { get; }

    /// <summary>
    /// System.Security.SuppressUnmanagedCodeSecurityAttribute
    /// </summary>
    INamespaceTypeReference SystemSecuritySuppressUnmanagedCodeSecurityAttribute { get; }

    /// <summary>
    /// System.String
    /// </summary>
    INamespaceTypeReference SystemString { get; }

    /// <summary>
    /// System.ThreadStaticAttribute
    /// </summary>
    INamespaceTypeReference SystemThreadStaticAttribute { get; }

    /// <summary>
    /// System.Type
    /// </summary>
    INamespaceTypeReference SystemType { get; }

    /// <summary>
    /// System.TypedReference
    /// </summary>
    INamespaceTypeReference SystemTypedReference { get; }

    /// <summary>
    /// System.UInt16
    /// </summary>
    INamespaceTypeReference SystemUInt16 { get; }

    /// <summary>
    /// System.UInt32
    /// </summary>
    INamespaceTypeReference SystemUInt32 { get; }

    /// <summary>
    /// System.UInt64
    /// </summary>
    INamespaceTypeReference SystemUInt64 { get; }

    /// <summary>
    /// System.UInt8
    /// </summary>
    INamespaceTypeReference SystemUInt8 { get; }

    /// <summary>
    /// System.UIntPtr
    /// </summary>
    INamespaceTypeReference SystemUIntPtr { get; }

    /// <summary>
    /// System.ValueType
    /// </summary>
    INamespaceTypeReference SystemValueType { get; }

    /// <summary>
    /// System.Void
    /// </summary>
    INamespaceTypeReference SystemVoid { get; }

    /// <summary>
    /// Maps a PrimitiveTypeCode value (other than Pointer, Reference and NotPrimitive) to a corresponding ITypeDefinition instance.
    /// </summary>
    INamespaceTypeReference GetTypeFor(PrimitiveTypeCode typeCode);
    //^ requires typeCode != PrimitiveTypeCode.Pointer && typeCode != PrimitiveTypeCode.Reference && typeCode != PrimitiveTypeCode.NotPrimitive;

  }

  [ContractClassFor(typeof(IPlatformType))]
  abstract class IPlatformTypeContract : IPlatformType {
    public INamespaceTypeReference SystemDiagnosticsContractsContract {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public byte PointerSize {
      get {
        Contract.Ensures(Contract.Result<byte>() == 4 || Contract.Result<byte>() == 8);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemArgIterator {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemArray {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemAsyncCallback {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemAttributeUsageAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemBoolean {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemChar {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsGenericDictionary {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsGenericICollection {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsGenericIEnumerable {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsGenericIEnumerator {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsGenericIList {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsICollection {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsIEnumerable {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsIEnumerator {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsIList {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsIStructuralComparable {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemCollectionsIStructuralEquatable {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemDateTime {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemDateTimeOffset {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemDecimal {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemDelegate {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemDBNull {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemEnum {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemException {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemFloat32 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemFloat64 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemGlobalizationCultureInfo {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemIAsyncResult {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemICloneable {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemInt16 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemInt32 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemInt64 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemInt8 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemIntPtr {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemMulticastDelegate {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemNullable {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemObject {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemReflectionAssemblySignatureKeyAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        return Dummy.NamespaceTypeReference; 
      }
    }

    public INamespaceTypeReference SystemRuntimeArgumentHandle {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeFieldHandle {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeMethodHandle {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeTypeHandle {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesCallConvCdecl {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesExtensionAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesInternalsVisibleToAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesIsConst {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesIsVolatile {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesReferenceAssemblyAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeInteropServicesDllImportAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemRuntimeInteropServicesTypeIdentifierAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemSecurityPermissionsSecurityAction {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemSecuritySecurityCriticalAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemSecuritySecuritySafeCriticalAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemSecuritySuppressUnmanagedCodeSecurityAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemString {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemType {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemTypedReference {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemUInt16 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemUInt32 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemUInt64 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemUInt8 {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemUIntPtr {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemValueType {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference SystemVoid {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceTypeReference GetTypeFor(PrimitiveTypeCode typeCode) {
      Contract.Requires(typeCode != PrimitiveTypeCode.Pointer && typeCode != PrimitiveTypeCode.Reference && typeCode != PrimitiveTypeCode.NotPrimitive);
      Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
      throw new NotImplementedException();
    }


    public INamespaceTypeReference SystemContextStaticAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException(); 
      }
    }

    public INamespaceTypeReference SystemThreadStaticAttribute {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        throw new NotImplementedException(); 
      }
    }
  }

  /// <summary>
  /// This interface models the metadata representation of a pointer to a location in unmanaged memory.
  /// </summary>
  public interface IPointerType : IPointerTypeReference, ITypeDefinition {
  }

  /// <summary>
  /// This interface models the metadata representation of a pointer to a location in unmanaged memory.
  /// </summary>
  [ContractClass(typeof(IPointerTypeReferenceContract))]
  public interface IPointerTypeReference : ITypeReference {

    /// <summary>
    /// The type of value stored at the target memory location.
    /// </summary>
    ITypeReference TargetType { get; }

  }

  [ContractClassFor(typeof(IPointerTypeReference))]
  abstract class IPointerTypeReferenceContract : IPointerTypeReference {
    public ITypeReference TargetType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// This interface models the metadata representation of a managed pointer.
  /// Remark: This should be only used in attributes. For other objects like Local variables etc
  /// there is explicit IsReference field that should be used.
  /// </summary>
  public interface IManagedPointerType : IManagedPointerTypeReference, ITypeDefinition {
  }

  /// <summary>
  /// This interface models the metadata representation of a managed pointer.
  /// Remark: This should be only used in attributes. For other objects like Local variables etc
  /// there is explicit IsReference field that should be used.
  /// </summary>
  [ContractClass(typeof(IManagedPointerTypeReferenceContract))]
  public interface IManagedPointerTypeReference : ITypeReference {

    /// <summary>
    /// The type of value stored at the target memory location.
    /// </summary>
    ITypeReference TargetType { get; }

  }

  [ContractClassFor(typeof(IManagedPointerTypeReference))]
  abstract class IManagedPointerTypeReferenceContract : IManagedPointerTypeReference {
    public ITypeReference TargetType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// This interface models the metadata representation of a type.
  /// </summary>
  [ContractClass(typeof(ITypeDefinitionContract))]
  public interface ITypeDefinition : IContainer<ITypeDefinitionMember>, IDefinition, IScope<ITypeDefinitionMember>, ITypeReference {

    /// <summary>
    /// The byte alignment that values of the given type ought to have. Must be a power of 2. If zero, the alignment is decided at runtime.
    /// </summary>
    ushort Alignment { get; }

    /// <summary>
    /// Zero or more classes from which this type is derived.
    /// For CLR types this collection is empty for interfaces and System.Object and populated with exactly one base type for all other types.
    /// </summary>
    IEnumerable<ITypeReference> BaseClasses {
      get;
      // ^ ensures forall{ITypeReference baseClassReference in result; baseClassReference.ResolvedType.IsClass};
    }

    /// <summary>
    /// Zero or more events defined by this type.
    /// </summary>
    IEnumerable<IEventDefinition> Events { get; }

    /// <summary>
    /// Zero or more implementation overrides provided by the class.
    /// </summary>
    IEnumerable<IMethodImplementation> ExplicitImplementationOverrides { get; }

    /// <summary>
    /// Zero or more fields defined by this type.
    /// </summary>
    IEnumerable<IFieldDefinition> Fields { get; }

    /// <summary>
    /// Zero or more parameters that can be used as type annotations.
    /// </summary>
    IEnumerable<IGenericTypeParameter> GenericParameters {
      get;
      //^ requires this.IsGeneric;
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    ushort GenericParameterCount {
      get;
      //^ ensures !this.IsGeneric ==> result == 0;
      //^ ensures this.IsGeneric ==> result > 0;
    }

    /// <summary>
    /// True if this type has a non empty collection of SecurityAttributes or the System.Security.SuppressUnmanagedCodeSecurityAttribute.
    /// </summary>
    bool HasDeclarativeSecurity { get; }

    /// <summary>
    /// Zero or more interfaces implemented by this type.
    /// </summary>
    IEnumerable<ITypeReference> Interfaces { get; }

    /// <summary>
    /// An instance of this generic type that has been obtained by using the generic parameters as the arguments. 
    /// Use this instance to look up members
    /// </summary>
    IGenericTypeInstanceReference InstanceType {
      get;
      //^ requires this.IsGeneric;
    }

    /// <summary>
    /// True if the type may not be instantiated.
    /// </summary>
    bool IsAbstract { get; }

    /// <summary>
    /// If true, the type need not be initialized as soon as any method defined by the type starts executing.
    /// In either case, the type initializer (if provided) will run before the first access to a static field
    /// of the type.
    /// </summary>
    bool IsBeforeFieldInit { get; }

    /// <summary>
    /// True if the type is a class (it is not an interface or type parameter and does not extend a special base class).
    /// Corresponds to C# class.
    /// </summary>
    bool IsClass { get; }

    /// <summary>
    /// Is this imported from COM type library
    /// </summary>
    bool IsComObject { get; }

    /// <summary>
    /// True if the type is a delegate (it extends System.MultiCastDelegate). Corresponds to C# delegate
    /// </summary>
    bool IsDelegate { get; }

    /// <summary>
    /// True if this type is parameterized (this.GenericParameters is a non empty collection).
    /// </summary>
    bool IsGeneric { get; }

    /// <summary>
    /// True if the type is an interface.
    /// </summary>
    bool IsInterface { get; }

    /// <summary>
    /// True if the type is a reference type. A reference type is non static class or interface or a suitably constrained type parameter.
    /// A type parameter for which MustBeReferenceType (the class constraint in C#) is true returns true for this property
    /// as does a type parameter with a constraint that is a class.
    /// </summary>
    bool IsReferenceType { get; }

    /// <summary>
    /// True if this type gets special treatment from the runtime.
    /// </summary>
    bool IsRuntimeSpecial { get; }

    /// <summary>
    /// True if this type is serializable.
    /// </summary>
    bool IsSerializable { get; }

    /// <summary>
    /// True if the type has special name.
    /// </summary>
    bool IsSpecialName { get; }

    /// <summary>
    /// True if the type is a struct (its not Primitive, is sealed and base is System.ValueType).
    /// </summary>
    bool IsStruct { get; }

    /// <summary>
    /// True if the type may not be subtyped.
    /// </summary>
    bool IsSealed { get; }

    /// <summary>
    /// True if the type is an abstract sealed class that directly extends System.Object and declares no constructors.
    /// </summary>
    bool IsStatic { get; }

    /// <summary>
    /// Layout of the type.
    /// </summary>
    LayoutKind Layout { get; }

    /// <summary>
    /// The collection of member instances that are members of this scope.
    /// </summary>
    new IEnumerable<ITypeDefinitionMember> Members {
      get;
      // ^ ensures forall{ITypeDefinitionMember member in result; member.ContainingTypeDefinition == this && 
      // ^ (member is IEventDefinition || member is IFieldDefinition || member is IMethodDefinition || member is INestedTypeDefinition || member is IPropertyDefinition)};
    }

    /// <summary>
    /// Zero or more methods defined by this type.
    /// </summary>
    IEnumerable<IMethodDefinition> Methods { get; }

    /// <summary>
    /// Zero or more nested types defined by this type.
    /// </summary>
    IEnumerable<INestedTypeDefinition> NestedTypes { get; }

    /// <summary>
    /// Zero or more private type members generated by the compiler for implementation purposes. These members
    /// are only available after a complete visit of all of the other members of the type, including the bodies of methods.
    /// </summary>
    IEnumerable<ITypeDefinitionMember> PrivateHelperMembers { get; }

    /// <summary>
    /// Zero or more properties defined by this type.
    /// </summary>
    IEnumerable<IPropertyDefinition> Properties { get; }

    /// <summary>
    /// Declarative security actions for this type. Will be empty if this.HasSecurity is false.
    /// </summary>
    IEnumerable<ISecurityAttribute> SecurityAttributes { get; }

    /// <summary>
    /// Size of an object of this type. In bytes. If zero, the size is unspecified and will be determined at runtime.
    /// </summary>
    uint SizeOf { get; }

    /// <summary>
    /// Default marshalling of the Strings in this class.
    /// </summary>
    StringFormatKind StringFormat { get; }

    /// <summary>
    /// Returns a reference to the underlying (integral) type on which this (enum) type is based.
    /// </summary>
    ITypeReference UnderlyingType {
      get;
      //^ requires this.IsEnum;
    }

  }

  [ContractClassFor(typeof(ITypeDefinition))]
  abstract class ITypeDefinitionContract : ITypeDefinition {
    public ushort Alignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ITypeReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeReference>>(), x => x != null));
        //Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeReference>>(), x => x.ResolvedType.IsClass));

        throw new NotImplementedException();
      }
    }

    public IEnumerable<IEventDefinition> Events {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IEventDefinition>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IEventDefinition>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IMethodImplementation>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IMethodImplementation>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IFieldDefinition>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IFieldDefinition>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IGenericTypeParameter>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IGenericTypeParameter>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public ushort GenericParameterCount {
      get {
        Contract.Ensures(this.IsGeneric || Contract.Result<ushort>() == 0);
        Contract.Ensures(!this.IsGeneric || Contract.Result<ushort>() > 0);
        throw new NotImplementedException();
      }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ITypeReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeReference>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IGenericTypeInstanceReference InstanceType {
      get {
        Contract.Requires(this.IsGeneric);
        Contract.Ensures(Contract.Result<IGenericTypeInstanceReference>() != null);
        throw new NotImplementedException();
      }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsBeforeFieldInit {
      get { throw new NotImplementedException(); }
    }

    public bool IsClass {
      get { throw new NotImplementedException(); }
    }

    public bool IsComObject {
      get { throw new NotImplementedException(); }
    }

    public bool IsDelegate {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public bool IsInterface {
      get { throw new NotImplementedException(); }
    }

    public bool IsReferenceType {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSerializable {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStruct {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    public LayoutKind Layout {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get {
        //Contract.Ensures(Contract.Result<IEnumerable<ITypeDefinitionMember>>() != null);
        //Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeDefinitionMember>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IMethodDefinition>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IMethodDefinition>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<INestedTypeDefinition>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<INestedTypeDefinition>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ITypeDefinitionMember>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ITypeDefinitionMember>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IPropertyDefinition>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IPropertyDefinition>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ISecurityAttribute>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ISecurityAttribute>>(), x => x != null));
        Contract.Ensures(this.HasDeclarativeSecurity || IteratorHelper.EnumerableIsEmpty(Contract.Result<IEnumerable<ISecurityAttribute>>()));
        throw new NotImplementedException();
      }
    }

    public uint SizeOf {
      get { throw new NotImplementedException(); }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference UnderlyingType {
      get {
        Contract.Requires(this.IsEnum);
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(ITypeDefinitionMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public IAliasForType AliasForType {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsAlias {
      get { throw new NotImplementedException(); }
    }

    public bool IsEnum {
      get { throw new NotImplementedException(); }
    }

    public bool IsValueType {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinition ResolvedType {
      get { throw new NotImplementedException(); }
    }

    public PrimitiveTypeCode TypeCode {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to a type.
  /// </summary>
  [ContractClass(typeof(ITypeReferenceContract))]
  public interface ITypeReference : IReference, IInternedKey {

    /// <summary>
    /// If this type reference can be resolved and it resolves to a type alias, the resolution continues on
    /// to resolve the reference to the aliased type. This property provides a way to discover how that resolution
    /// proceeded, by exposing the alias concerned. Think of this as a version of ResolvedType that does not
    /// traverse aliases.
    /// </summary>
    IAliasForType AliasForType { get; }

    /// <summary>
    /// True if this reference can be resolved and it resolves to a type alias. The type alias can be retrieved
    /// via the AliasForType property. The value of this.ResolvedType will be this.AliasForType.AliasedType.ResolvedType.
    /// </summary>
    bool IsAlias {
      get;
      //^ ensures result ==> !(this is ITypeDefinition);
    }

    /// <summary>
    /// True if the type is an enumeration (it extends System.Enum and is sealed). Corresponds to C# enum.
    /// A type reference may report this value as false, even though the referenced type definition reports it as true.
    /// However, any type reference used in a custom attribute must report the correct value for this property or the
    /// resulting metadata would be invalid.
    /// Important, do not cache this property. Its value may change from false to true (but not the other way round) in some situations.
    /// To be on the safe side, resolve all type references whenever possible.
    /// </summary>
    /// <remarks>
    /// Metadata readers have trouble with this property because an ITypeReference instance is a model for the TypeRef table in the ECMA-335 metadata format.
    /// Unfortunately, a TypeRef table entry does not distinguish between enum types and other types. The distinction is made in some signatures that
    /// include TypeRef tokens. A more accurate model of the metadata format would encode the information obtained from those signatures in a separate property of
    /// the object containing the type reference, or would introduce a new kind of type reference such as INamespaceEnumTypeReference and INestedEnumTypeReference.
    /// However, for historical/usability reasons this object model oversimplifies the situation by pretending that all type references can tell if they refer to enum types
    /// or not. When consuming metadata, a type reference starts off with the value being false, but may change it to true as soon as its token is encountered
    /// in a signature that indicates that it is an enum type. In practice this means that a type reference encountered in a part of the object model where it is
    /// important to know if the referenced type is an enum type, will get the right value from this property. However, if the value of this property is cached
    /// as soon as it is encountered for the first time, the wrong value may get cached.
    /// </remarks>
    bool IsEnum { get; }

    /// <summary>
    /// True if the type is a value type. 
    /// Value types are sealed and extend System.ValueType or System.Enum.
    /// A type parameter for which MustBeValueType (the struct constraint in C#) is true also returns true for this property.
    /// A type reference may report this value as false, even though the referenced type definition reports it as true.
    /// However, any type reference used in an IL instruction other than ldtoken (used for the C# typeof expression) must report the correct value.
    /// Important, do not cache this property. Its value may change from false to true (but not the other way round) in some situations.
    /// To be on the safe side, resolve all type references whenever possible.
    /// </summary>
    /// <remarks>
    /// Metadata readers have trouble with this property because an ITypeReference instance is a model for the TypeRef table in the ECMA-335 metadata format.
    /// Unfortunately, a TypeRef table entry does not distinguish between value types and reference types. The distinction is made in some signatures that
    /// include TypeRef tokens. A more accurate model of the metadata format would encode the information obtained from those signatures in a separate property of
    /// the object containing the type reference, or would introduce a new kind of type reference such as INamespaceValueTypeReference and INestedValueTypeReference.
    /// However, for historical/usability reasons this object model oversimplifies the situation by pretending that all type references can tell if they refer to value types
    /// or not. When consuming metadata, a type reference starts off with the value being false, but may change it to true as soon as its token is encountered
    /// in a signature that indicates that it is a value type. In practice this means that a type reference encountered in a part of the object model where it is
    /// important to know if the referenced type is a value type, will get the right value from this property. However, if the value of this property is cached
    /// as soon as it is encountered for the first time, the wrong value may get cached.
    /// </remarks>
    bool IsValueType { get; }

    /// <summary>
    /// A way to get to platform types such as System.Object.
    /// </summary>
    IPlatformType PlatformType { get; }

    /// <summary>
    /// The type definition being referred to.
    /// In case the reference is to an alias, then this is resolved type of the alias, which
    /// could have a different InternedKey from this reference.
    /// </summary>
    /// <remarks>
    /// If this.IsAlias then this.ResolvedType == this.AliasForType.AliasedType.ResolvedType.
    /// </remarks>
    ITypeDefinition ResolvedType {
      get;
      //^ ensures this.IsAlias ==> result == this.AliasForType.AliasedType.ResolvedType;
      //^ ensures (this is ITypeDefinition) ==> result == this;
    }

    /// <summary>
    /// Unless the value of TypeCode is PrimitiveTypeCode.NotPrimitive, the type corresponds to a "primitive" CLR type (such as System.Int32) and
    /// the type code identifies which of the primitive types it corresponds to.
    /// </summary>
    PrimitiveTypeCode TypeCode { get; }

  }

  [ContractClassFor(typeof(ITypeReference))]
  abstract class ITypeReferenceContract : ITypeReference {

    public IAliasForType AliasForType {
      get {
        Contract.Ensures(Contract.Result<IAliasForType>() != null);
        //Contract.Ensures(this.IsAlias || Contract.Result<IAliasForType>().AliasedType.ResolvedType == this.ResolvedType);
        throw new NotImplementedException();
      }
    }

    public uint InternedKey {
      get {
        throw new NotImplementedException();
      }
    }

    public bool IsAlias {
      get {
        Contract.Ensures(!Contract.Result<bool>() || !(this is ITypeDefinition));
        throw new NotImplementedException();
      }
    }

    public bool IsEnum {
      get {
        throw new NotImplementedException();
      }
    }

    public bool IsValueType {
      get {
        throw new NotImplementedException();
      }
    }

    public IPlatformType PlatformType {
      get {
        Contract.Ensures(Contract.Result<IPlatformType>() != null);
        throw new NotImplementedException();
      }
    }

    public ITypeDefinition ResolvedType {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
        Contract.Ensures(!(this is ITypeDefinition) || Contract.Result<ITypeDefinition>() == this);
        //Contract.Ensures(Contract.Result<ITypeDefinition>() == Dummy.TypeDefinition || this.IsAlias ||
        //    Contract.Result<ITypeDefinition>().InternedKey == this.InternedKey);
        throw new NotImplementedException();
      }
    }

    public PrimitiveTypeCode TypeCode {
      get {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get {
        throw new NotImplementedException();
      }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get {
        throw new NotImplementedException();
      }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A enumeration of all of the value types that are built into the Runtime (and thus have specialized IL instructions that manipulate them).
  /// </summary>
  public enum PrimitiveTypeCode {
    /// <summary>
    /// A single bit.
    /// </summary>
    Boolean,
    /// <summary>
    /// An usigned 16 bit integer representing a Unicode UTF16 code point.
    /// </summary>
    Char,
    /// <summary>
    /// A signed 8 bit integer.
    /// </summary>
    Int8,
    /// <summary>
    /// A 32 bit IEEE floating point number.
    /// </summary>
    Float32,
    /// <summary>
    /// A 64 bit IEEE floating point number.
    /// </summary>
    Float64,
    /// <summary>
    /// A signed 16 bit integer.
    /// </summary>
    Int16,
    /// <summary>
    /// A signed 32 bit integer.
    /// </summary>
    Int32,
    /// <summary>
    /// A signed 64 bit integer.
    /// </summary>
    Int64,
    /// <summary>
    /// A signed 32 bit integer or 64 bit integer, depending on the native word size of the underlying processor.
    /// </summary>
    IntPtr,
    /// <summary>
    /// A pointer to fixed or unmanaged memory.
    /// </summary>
    Pointer,
    /// <summary>
    /// A reference to managed memory.
    /// </summary>
    Reference,
    /// <summary>
    /// A string.
    /// </summary>
    String,
    /// <summary>
    /// An unsigned 8 bit integer.
    /// </summary>
    UInt8,
    /// <summary>
    /// An unsigned 16 bit integer.
    /// </summary>
    UInt16,
    /// <summary>
    /// An unsigned 32 bit integer.
    /// </summary>
    UInt32,
    /// <summary>
    /// An unsigned 64 bit integer.
    /// </summary>
    UInt64,
    /// <summary>
    /// An unsigned 32 bit integer or 64 bit integer, depending on the native word size of the underlying processor.
    /// </summary>
    UIntPtr,
    /// <summary>
    /// A type that denotes the absense of a value.
    /// </summary>
    Void,
    /// <summary>
    /// Not a primitive type.
    /// </summary>
    NotPrimitive,
    /// <summary>
    /// Type is a dummy type.
    /// </summary>
    Invalid,
  }

  /// <summary>
  /// Enumerates the different kinds of levels of visibility a type member can have.
  /// </summary>
  public enum TypeMemberVisibility {
    /// <summary>
    /// The visibility has not been specified. Use the applicable default.
    /// </summary>
    Default,
    /// <summary>
    /// The member is visible only within its own assembly.
    /// </summary>
    Assembly,
    /// <summary>
    /// The member is visible only within its own type and any subtypes.
    /// </summary>
    Family,
    /// <summary>
    /// The member is visible only within the intersection of its family (its own type and any subtypes) and assembly. 
    /// </summary>
    FamilyAndAssembly,
    /// <summary>
    /// The member is visible only within the union of its family and assembly. 
    /// </summary>
    FamilyOrAssembly,
    /// <summary>
    /// The member is visible only to the compiler producing its assembly.
    /// </summary>
    Other,
    /// <summary>
    /// The member is visible only within its own type.
    /// </summary>
    Private,
    /// <summary>
    /// The member is visible everywhere its declaring type is visible.
    /// </summary>
    Public,
    /// <summary>
    /// A mask that can be used to mask out flag bits when the latter are stored in the same memory word as this enumeration.
    /// </summary>
    Mask=0xF
  }

  /// <summary>
  /// Enumerates the different kinds of variance a generic method or generic type parameter may have.
  /// </summary>
  public enum TypeParameterVariance {
    /// <summary>
    /// Two type or method instances are compatible only if they have exactly the same type argument for this parameter.
    /// </summary>
    NonVariant,
    /// <summary>
    /// A type or method instance will match another instance if it has a type for this parameter that is the same or a subtype of the type the
    /// other instance has for this parameter.
    /// </summary>
    Covariant,
    /// <summary>
    /// A type or method instance will match another instance if it has a type for this parameter that is the same or a supertype of the type the
    /// other instance has for this parameter.
    /// </summary>
    Contravariant,

    /// <summary>
    /// A mask that can be used to mask out flag bits when the latter are stored in the same memory word as the enumeration.
    /// </summary>
    Mask=3,

  }

  /// <summary>
  /// The layout on the type
  /// </summary>
  public enum LayoutKind {
    /// <summary>
    /// Layout is determines at runtime.
    /// </summary>
    Auto,
    /// <summary>
    /// Layout is sequential.
    /// </summary>
    Sequential,
    /// <summary>
    /// Layout is specified explicitly.
    /// </summary>
    Explicit,
  }

  /// <summary>
  /// Enum indicating the default string formatting in the type
  /// </summary>
  public enum StringFormatKind {
    /// <summary>
    /// Managed string marshalling is unspecified
    /// </summary>
    Unspecified,
    /// <summary>
    /// Managed strings are marshaled to and from Ansi.
    /// </summary>
    Ansi,
    /// <summary>
    /// Managed strings are marshaled to and from Unicode
    /// </summary>
    Unicode,
    /// <summary>
    /// Defined by underlying platform.
    /// </summary>
    AutoChar,
  }

}
