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

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// 
  /// </summary>
  public abstract class AliasForType : IAliasForType, ICopyFrom<IAliasForType> {

    /// <summary>
    /// 
    /// </summary>
    internal AliasForType() {
      this.aliasedType = Dummy.NamedTypeReference;
      this.attributes = null;
      this.locations = null;
      this.members = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aliasForType"></param>
    /// <param name="internFactory"></param>
    public void Copy(IAliasForType aliasForType, IInternFactory internFactory) {
      this.aliasedType = aliasForType.AliasedType;
      if (IteratorHelper.EnumerableIsNotEmpty(aliasForType.Attributes))
        this.attributes = new List<ICustomAttribute>(aliasForType.Attributes);
      else
        this.attributes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(aliasForType.Locations))
        this.locations = new List<ILocation>(aliasForType.Locations);
      else
        this.locations = null;
      if (IteratorHelper.EnumerableIsNotEmpty(aliasForType.Members))
        this.members = new List<IAliasMember>(aliasForType.Members);
      else
        this.members = null;
    }

    /// <summary>
    /// Type reference of the type for which this is the alias
    /// </summary>
    /// <value></value>
    public INamedTypeReference AliasedType {
      get { return this.aliasedType; }
      set { this.aliasedType = value; }
    }
    INamedTypeReference aliasedType;

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ Attributes {
      get { return this.attributes; }
      set { this.attributes = value; }
    }
    List<ICustomAttribute>/*?*/ attributes;

    //^ [Pure]
    /// <summary>
    /// Return true if the given member instance is a member of this scope.
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public bool Contains(IAliasMember member) {
      if (this.Members == null) return false;
      foreach (IAliasMember tdmem in this.Members)
        if (member == tdmem) return true;
      return false;
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference. The dispatch method does nothing else.
    /// </summary>
    public abstract void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// This throws an InvalidOperation exception since valid metadata never has a reference to a type alias.
    /// </summary>
    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    public ushort GenericParameterCount {
      get { return this.AliasedType.GenericParameterCount; }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members with the given name that also satisfy the given predicate.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<IAliasMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<IAliasMember, bool> predicate) {
      if (this.Members == null) yield break;
      foreach (IAliasMember tdmem in this.Members) {
        if (tdmem.Name.UniqueKey == name.UniqueKey || ignoreCase && (name.UniqueKeyIgnoringCase == tdmem.Name.UniqueKeyIgnoringCase)) {
          if (predicate(tdmem)) yield return tdmem;
        }
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members that satisfy the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<IAliasMember> GetMatchingMembers(Function<IAliasMember, bool> predicate) {
      if (this.Members == null) yield break;
      foreach (IAliasMember tdmem in this.Members) {
        if (predicate(tdmem)) yield return tdmem;
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public IEnumerable<IAliasMember> GetMembersNamed(IName name, bool ignoreCase) {
      if (this.Members == null) yield break;
      foreach (IAliasMember tdmem in this.Members) {
        if (tdmem.Name.UniqueKey == name.UniqueKey || ignoreCase && (name.UniqueKeyIgnoringCase == tdmem.Name.UniqueKeyIgnoringCase)) {
          yield return tdmem;
        }
      }
    }

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance. May be null.
    /// </summary>
    /// <value></value>
    public List<ILocation>/*?*/ Locations {
      get { return this.locations; }
      set { this.locations = value; }
    }
    List<ILocation>/*?*/ locations;

    /// <summary>
    /// The collection of member objects comprising the type. May be null.
    /// </summary>
    /// <value></value>
    public List<IAliasMember>/*?*/ Members {
      get { return this.members; }
      set { this.members = value; }
    }
    List<IAliasMember>/*?*/ members;

    /// <summary>
    /// The name of the aliased type.
    /// </summary>
    public IName Name {
      get { return this.AliasedType.Name; }
    }

    #region IAliasForType

    IEnumerable<IAliasMember> IAliasForType.Members {
      get {
        if (this.Members == null) return Enumerable<IAliasMember>.Empty;
        return this.Members.AsReadOnly();
      }
    }

    #endregion

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

    #region IContainer<IAliasMember> Members

    IEnumerable<IAliasMember> IContainer<IAliasMember>.Members {
      get { return ((IAliasForType)this).Members; }
    }

    #endregion

    #region IScope<IAliasMember> Members

    IEnumerable<IAliasMember> IScope<IAliasMember>.Members {
      get { return ((IAliasForType)this).Members; }
    }

    #endregion

  }

  /// <summary>
  /// 
  /// </summary>
  [ContractVerification(true)]
  public abstract class ArrayTypeReference : TypeReference, IArrayTypeReference {

    /// <summary>
    /// A reference to an array type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public ArrayTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.elementType = Dummy.TypeReference;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.elementType != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
      //Contract.Invariant(this.resolvedType == null || this.resolvedType.InternedKey == this.InternedKey);
    }

    /// <summary>
    /// Calls visitor.Visit(IArrayTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The type of the elements of this array.
    /// </summary>
    public ITypeReference ElementType {
      get { return this.elementType; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.elementType = value;
      }
    }
    ITypeReference elementType;

    /// <summary>
    /// This type of array is a single dimensional array with zero lower bound for index values.
    /// </summary>
    public abstract bool IsVector {
      get;
    }

    /// <summary>
    /// A possibly empty list of lower bounds for dimension indices. When not explicitly specified, a lower bound defaults to zero.
    /// The first lower bound in the list corresponds to the first dimension. Dimensions cannot be skipped. May be null.
    /// </summary>
    public virtual List<int>/*?*/ LowerBounds {
      get {
        return null;
      }
      set {
        Contract.Requires(!this.IsVector);
        Contract.Requires(!this.IsFrozen);
      }
    }

    /// <summary>
    /// The number of array dimensions.
    /// </summary>
    public virtual uint Rank {
      get {
        return 1;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value > 0);
      }
    }

    /// <summary>
    /// A possible empty list of upper bounds for dimension indices.
    /// The first upper bound in the list corresponds to the first dimension. Dimensions cannot be skipped.
    /// An unspecified upper bound means that instances of this type can have an arbitrary upper bound for that dimension. May be null.
    /// </summary>
    /// <value></value>
    public virtual List<ulong>/*?*/ Sizes {
      get {
        return null;
      }
      set {
        Contract.Requires(!this.IsVector);
        Contract.Requires(!this.IsFrozen);
      }

    }

    /// <summary>
    /// Gets the type of the resolved.
    /// </summary>
    /// <value>The type of the resolved.</value>
    public override ITypeDefinition ResolvedType {
      [ContractVerification(false)]
      get {
        return this.ResolvedArrayType;
      }
    }

    /// <summary>
    /// Gets the type of the resolved array.
    /// </summary>
    /// <value>The type of the resolved array.</value>
    public IArrayType ResolvedArrayType {
      get {
        if (this.resolvedType == null) {
          this.isFrozen = true;
          this.resolvedType = this.Resolve();
        }
        return this.resolvedType;
      }
    }
    IArrayType/*?*/ resolvedType;

    /// <summary>
    /// Returns the type of the resolved array.
    /// </summary>
    protected abstract IArrayType Resolve();

    #region IArrayTypeReference Members


    IEnumerable<int> IArrayTypeReference.LowerBounds {
      get {
        if (this.LowerBounds == null) return Dummy.ArrayType.LowerBounds;
        return this.LowerBounds.AsReadOnly();
      }
    }

    IEnumerable<ulong> IArrayTypeReference.Sizes {
      get {
        if (this.Sizes == null) return Dummy.ArrayType.Sizes;
        return this.Sizes.AsReadOnly();
      }
    }

    #endregion

  }
  /// <summary>
  /// 
  /// </summary>
  public sealed class CustomModifier : ICustomModifier, ICopyFrom<ICustomModifier> {

    /// <summary>
    /// 
    /// </summary>
    public CustomModifier() {
      this.isOptional = false;
      this.modifier = Dummy.TypeReference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customModifier"></param>
    /// <param name="internFactory"></param>
    public void Copy(ICustomModifier customModifier, IInternFactory internFactory) {
      this.isOptional = customModifier.IsOptional;
      this.modifier = customModifier.Modifier;
    }

    /// <summary>
    /// If true, a language may use the modified storage location without being aware of the meaning of the modification.
    /// </summary>
    /// <value></value>
    public bool IsOptional {
      get { return this.isOptional; }
      set { this.isOptional = value; }
    }
    bool isOptional;

    /// <summary>
    /// A type used as a tag that indicates which type of modification applies to the storage location.
    /// </summary>
    /// <value></value>
    public ITypeReference Modifier {
      get { return this.modifier; }
      set { this.modifier = value; }
    }
    ITypeReference modifier;

  }

  /// <summary>
  /// A reference to a function pointer type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class FunctionPointerTypeReference : TypeReference, IFunctionPointerTypeReference, ICopyFrom<IFunctionPointerTypeReference> {

    /// <summary>
    /// A reference to a function pointer type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public FunctionPointerTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.callingConvention = (CallingConvention)0;
      this.type = Dummy.TypeReference;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.type != null);
      Contract.Invariant(this.resolvedFunctionPointer == null || this.IsFrozen);
      //Contract.Invariant(this.resolvedFunctionPointer == null || this.resolvedFunctionPointer.InternedKey == this.InternedKey);
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="functionPointerTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IFunctionPointerTypeReference functionPointerTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(functionPointerTypeReference, internFactory);
      this.callingConvention = functionPointerTypeReference.CallingConvention;
      if (IteratorHelper.EnumerableIsNotEmpty(functionPointerTypeReference.ExtraArgumentTypes))
        this.extraArgumentTypes = new List<IParameterTypeInformation>(functionPointerTypeReference.ExtraArgumentTypes);
      else
        this.extraArgumentTypes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(functionPointerTypeReference.Parameters))
        this.parameters = new List<IParameterTypeInformation>(functionPointerTypeReference.Parameters);
      else
        this.parameters = null;
      if (functionPointerTypeReference.ReturnValueIsModified)
        this.returnValueCustomModifiers = new List<ICustomModifier>(functionPointerTypeReference.ReturnValueCustomModifiers);
      else
        this.returnValueCustomModifiers = null;
      this.returnValueIsByRef = functionPointerTypeReference.ReturnValueIsByRef;
      this.type = functionPointerTypeReference.Type;
    }

    /// <summary>
    /// Calling convention of the signature.
    /// </summary>
    /// <value></value>
    public CallingConvention CallingConvention {
      get { return this.callingConvention; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.callingConvention = value;
      }
    }
    CallingConvention callingConvention;

    /// <summary>
    /// True if the referenced function does not require an instance of its declaring type as its first argument.
    /// </summary>
    public bool IsStatic {
      get { return (this.CallingConvention & CallingConvention.HasThis) == 0; }
    }

    /// <summary>
    /// Calls visitor.Visit(IFunctionPointerTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The types and modifiers of extra arguments that the caller will pass to the methods that are pointed to by this pointer. May be null.
    /// </summary>
    public List<IParameterTypeInformation>/*?*/ ExtraArgumentTypes {
      get { return this.extraArgumentTypes; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.extraArgumentTypes = value;
      }
    }
    List<IParameterTypeInformation>/*?*/ extraArgumentTypes;

    /// <summary>
    /// The parameters forming part of this signature. May be null.
    /// </summary>
    public List<IParameterTypeInformation>/*?*/ Parameters {
      get { return this.parameters; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.parameters = value;
      }
    }
    List<IParameterTypeInformation>/*?*/ parameters;

    /// <summary>
    /// The type definition being referred to.
    /// </summary>
    public override ITypeDefinition ResolvedType {
      get { return this.ResolvedFunctionPointer; }
    }

    /// <summary>
    /// The function pointer type being referred to. Note that function pointers are structural types and that two separate instances with the
    /// same structure will be equivalent and thus have the same value for InternedKey.
    /// </summary>
    public IFunctionPointer ResolvedFunctionPointer {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
        //Contract.Ensures(Contract.Result<ITypeDefinition>() == Dummy.TypeDefinition || this.IsAlias ||
        //    Contract.Result<ITypeDefinition>().InternedKey == this.InternedKey);
        Contract.Ensures(this.IsFrozen);

        if (this.resolvedFunctionPointer == null) {
          this.isFrozen = true;
          var self = (IFunctionPointerTypeReference)this;
          this.resolvedFunctionPointer = new Immutable.FunctionPointerType(this.callingConvention, this.returnValueIsByRef, this.type, self.ReturnValueCustomModifiers, self.Parameters,
          self.ExtraArgumentTypes, this.InternFactory);
          //Contract.Assume(this.resolvedFunctionPointer.InternedKey == this.InternedKey);
        }
        return this.resolvedFunctionPointer;
      }
    }
    IFunctionPointer/*?*/ resolvedFunctionPointer;

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the returned value. Evaluate this property only if ReturnValueIsModified is true.
    /// May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier>/*?*/ ReturnValueCustomModifiers {
      get { return this.returnValueCustomModifiers; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.returnValueCustomModifiers = value;
      }
    }
    List<ICustomModifier>/*?*/ returnValueCustomModifiers;

    /// <summary>
    /// True if the return value is passed by reference (using a managed pointer).
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsByRef {
      get { return this.returnValueIsByRef; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.returnValueIsByRef = value;
      }
    }
    bool returnValueIsByRef;

    /// <summary>
    /// True if the return value has one or more custom modifiers associated with it.
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsModified {
      get { return this.returnValueCustomModifiers != null && this.returnValueCustomModifiers.Count > 0; }
    }

    /// <summary>
    /// The return type of the method or type of the property.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.type = value;
      }
    }
    ITypeReference type;

    #region IFunctionPointerTypeReference Members

    IEnumerable<IParameterTypeInformation> IFunctionPointerTypeReference.ExtraArgumentTypes {
      [ContractVerification(false)]
      get {
        if (this.extraArgumentTypes == null) return Dummy.FunctionPointer.ExtraArgumentTypes;
        return this.extraArgumentTypes.AsReadOnly();
      }
    }

    #endregion

    #region ISignature Members

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      [ContractVerification(false)]
      get {
        if (this.parameters == null) return Dummy.FunctionPointer.Parameters;
        return this.parameters.AsReadOnly();
      }
    }

    IEnumerable<ICustomModifier> ISignature.ReturnValueCustomModifiers {
      [ContractVerification(false)]
      get {
        if (this.returnValueCustomModifiers == null) return Dummy.FunctionPointer.ReturnValueCustomModifiers;
        return this.returnValueCustomModifiers.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a generic method parameter type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class GenericMethodParameterReference : TypeReference, IGenericMethodParameterReference, ICopyFrom<IGenericMethodParameterReference> {

    /// <summary>
    /// A reference to a generic method parameter type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public GenericMethodParameterReference() {
      Contract.Ensures(!this.IsFrozen);

      this.definingMethod = Dummy.MethodReference;
      this.name = Dummy.Name;
      this.index = 0;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.definingMethod != null);
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
    }


    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="genericMethodParameterReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IGenericMethodParameterReference genericMethodParameterReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(genericMethodParameterReference, internFactory);
      this.definingMethod = genericMethodParameterReference.DefiningMethod;
      this.name = genericMethodParameterReference.Name;
      this.index = genericMethodParameterReference.Index;
    }

    /// <summary>
    /// A reference to the generic method that defines the referenced type parameter.
    /// </summary>
    /// <value></value>
    public IMethodReference DefiningMethod {
      get { return this.definingMethod; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.definingMethod = value;
      }
    }
    IMethodReference definingMethod;

    /// <summary>
    /// Calls visitor.Visit(IGenericMethodParameterReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.name = value;
      }
    }
    IName name;

    /// <summary>
    /// The position in the parameter list where this instance can be found.
    /// </summary>
    /// <value></value>
    public ushort Index {
      get { return this.index; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.index = value;
      }
    }
    ushort index;

    private void Resolve() {
      Contract.Ensures(this.IsFrozen);
      this.isFrozen = true;
      this.resolvedType = Dummy.GenericMethodParameter;
      var definingMethod = this.definingMethod.ResolvedMethod;
      if (!definingMethod.IsGeneric || this.index >= definingMethod.GenericParameterCount) return;
      foreach (var par in definingMethod.GenericParameters) {
        if (par.Index == this.index) { this.resolvedType = par; return; }
      }
    }

    IGenericMethodParameter IGenericMethodParameterReference.ResolvedType {
      get {
        Contract.Ensures(this.IsFrozen);
        if (this.resolvedType == null)
          this.Resolve();
        return this.resolvedType;
      }
    }
    IGenericMethodParameter/*?*/ resolvedType;

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public override ITypeDefinition ResolvedType {
      get {
        var result = ((IGenericMethodParameterReference)this).ResolvedType;
        if (result is Dummy) return Dummy.TypeDefinition;
        return result;
      }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class GenericParameter : NamedTypeDefinition, IGenericParameter, ICopyFrom<IGenericParameter> {

    //^ [NotDelayed]
    /// <summary>
    /// 
    /// </summary>
    internal GenericParameter() {
      this.constraints = null;
      this.index = 0;
      //^ base;
      this.MustBeReferenceType = false;
      this.MustBeValueType = false;
      this.MustHaveDefaultConstructor = false;
      this.Variance = TypeParameterVariance.NonVariant;
    }

    /// <summary>
    /// Makes this type be a shallow copy of the given type.
    /// </summary>
    /// <param name="genericParameter">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IGenericParameter genericParameter, IInternFactory internFactory) {
      ((ICopyFrom<INamedTypeDefinition>)this).Copy(genericParameter, internFactory);
      if (IteratorHelper.EnumerableIsNotEmpty(genericParameter.Constraints))
        this.constraints = new List<ITypeReference>(genericParameter.Constraints);
      else
        this.constraints = null;
      this.index = genericParameter.Index;
      this.MustBeReferenceType = genericParameter.MustBeReferenceType;
      this.MustBeValueType = genericParameter.MustBeValueType;
      this.MustHaveDefaultConstructor = genericParameter.MustHaveDefaultConstructor;
      this.Variance = genericParameter.Variance;
    }

    /// <summary>
    /// A list of classes or interfaces. All type arguments matching this parameter must be derived from all of the classes and implement all of the interfaces. May be null.
    /// </summary>
    /// <value></value>
    public List<ITypeReference>/*?*/ Constraints {
      get { return this.constraints; }
      set { this.constraints = value; }
    }
    List<ITypeReference>/*?*/ constraints;

    private ITypeDefinition GetEffectiveBaseClass() {
      ITypeDefinition mostDerivedBaseClass = this.PlatformType.SystemObject.ResolvedType;
      if (this.Constraints != null) {
        foreach (ITypeReference constraint in this.Constraints) {
          ITypeDefinition constraintType = constraint.ResolvedType;
          if (constraintType.IsClass && TypeHelper.Type1DerivesFromType2(constraintType, mostDerivedBaseClass))
            mostDerivedBaseClass = constraintType;
        }
      }
      return mostDerivedBaseClass;
    }

    /// <summary>
    /// The position in the parameter list where this instance can be found.
    /// </summary>
    /// <value></value>
    public ushort Index {
      get { return this.index; }
      set { this.index = value; }
    }
    ushort index;

    /// <summary>
    /// True if the type is a reference type. A reference type is non static class or interface or a suitably constrained type parameter.
    /// A type parameter for which MustBeReferenceType (the class constraint in C#) is true returns true for this property
    /// as does a type parameter with a constraint that is a class.
    /// </summary>
    /// <value></value>
    public override bool IsReferenceType {
      get {
        if (((int)this.flags & 0x00000800) == 0) {
          this.flags |= (NamedTypeDefinition.Flags)0x00000800;
          if (this.MustBeReferenceType)
            this.flags |= (NamedTypeDefinition.Flags)0x00000400;
          else {
            ITypeDefinition baseClass = this.GetEffectiveBaseClass();
            if (!TypeHelper.TypesAreEquivalent(baseClass, this.PlatformType.SystemObject) && !(baseClass is Dummy)) {
              if (baseClass.IsClass)
                this.flags |= (NamedTypeDefinition.Flags)0x00000400;
              else if (baseClass.IsValueType)
                this.flags |= (NamedTypeDefinition.Flags)0x00000200;
            }
          }
        }
        return ((int)this.flags & 0x00000400) != 0;
      }
    }

    /// <summary>
    /// True if the type is a value type.
    /// Value types are sealed and extend System.ValueType or System.Enum.
    /// A type parameter for which MustBeValueType (the struct constraint in C#) is true also returns true for this property.
    /// </summary>
    /// <value></value>
    public override bool IsValueType {
      get {
        if (((int)this.flags & 0x00000800) == 0) {
          this.flags |= (NamedTypeDefinition.Flags)0x00000800;
          if (this.MustBeReferenceType)
            this.flags |= (NamedTypeDefinition.Flags)0x00000400;
        }
        return ((int)this.flags & 0x00000200) != 0;
      }
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be reference types.
    /// </summary>
    /// <value></value>
    public bool MustBeReferenceType {
      get { return (this.flags & NamedTypeDefinition.Flags.MustBeReferenceType) != 0; }
      set
        //^ requires value ==> !this.MustBeValueType;
      {
        if (value)
          this.flags |= NamedTypeDefinition.Flags.MustBeReferenceType;
        else
          this.flags &= ~NamedTypeDefinition.Flags.MustBeReferenceType;
      }
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be value types.
    /// </summary>
    /// <value></value>
    public bool MustBeValueType {
      get { return (this.flags & NamedTypeDefinition.Flags.MustBeValueType) != 0; }
      set
        //^ requires value ==> !this.MustBeReferenceType;
      {
        if (value)
          this.flags |= NamedTypeDefinition.Flags.MustBeValueType;
        else
          this.flags &= ~NamedTypeDefinition.Flags.MustBeValueType;
      }
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be value types or concrete classes with visible default constructors.
    /// </summary>
    /// <value></value>
    public bool MustHaveDefaultConstructor {
      get { return (this.flags & NamedTypeDefinition.Flags.MustHaveDefaultConstructor) != 0; }
      set {
        if (value)
          this.flags |= NamedTypeDefinition.Flags.MustHaveDefaultConstructor;
        else
          this.flags &= ~NamedTypeDefinition.Flags.MustHaveDefaultConstructor;
      }
    }

    /// <summary>
    /// Indicates if the generic type or method with this type parameter is co-, contra-, or non variant with respect to this type parameter.
    /// </summary>
    /// <value></value>
    public TypeParameterVariance Variance {
      get { return (TypeParameterVariance)((int)this.flags>>4) & TypeParameterVariance.Mask; }
      set {
        this.flags &= (NamedTypeDefinition.Flags)~((int)TypeParameterVariance.Mask<<4);
        this.flags |= (NamedTypeDefinition.Flags)((int)(value&TypeParameterVariance.Mask)<<4);
      }
    }

    #region IGenericParameter Members

    IEnumerable<ITypeReference> IGenericParameter.Constraints {
      get {
        if (this.Constraints == null) return Enumerable<ITypeReference>.Empty;
        return this.Constraints.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a generic type instance. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class GenericTypeInstanceReference : TypeReference, IGenericTypeInstanceReference, ICopyFrom<IGenericTypeInstanceReference> {

    /// <summary>
    /// A reference to a generic type instance. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public GenericTypeInstanceReference() {
      Contract.Ensures(!this.IsFrozen);

      this.genericType = Dummy.NamedTypeReference;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.genericType != null);
      //Contract.Invariant(this.genericType.ResolvedType == Dummy.TypeDefinition || this.genericType.ResolvedType.IsGeneric);
      //Contract.Invariant(this.genericArguments == null || Contract.ForAll(genericArguments, x => x != null));
      Contract.Invariant(this.IsFrozen || this.resolvedGenericTypeInstance == null);
    }


    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="genericTypeInstanceReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IGenericTypeInstanceReference genericTypeInstanceReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(genericTypeInstanceReference, internFactory);
      this.genericArguments = new List<ITypeReference>(genericTypeInstanceReference.GenericArguments);
      this.genericType = genericTypeInstanceReference.GenericType;
    }

    /// <summary>
    /// Calls visitor.Visit(IGenericTypeInstanceReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The type arguments that were used to instantiate this.GenericType in order to create this type.
    /// </summary>
    /// <value></value>
    public List<ITypeReference> GenericArguments {
      get {
        if (this.genericArguments == null)
          this.genericArguments = new List<ITypeReference>();
        return this.genericArguments;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.genericArguments = value;
      }
    }
    List<ITypeReference>/*?*/ genericArguments;

    /// <summary>
    /// Returns the generic type of which this type is an instance.
    /// </summary>
    /// <value></value>
    public INamedTypeReference GenericType {
      get { return this.genericType; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        //Contract.Requires(value.ResolvedType == Dummy.TypeDefinition || value.ResolvedType.IsGeneric);
        this.genericType = value;
      }
    }
    INamedTypeReference genericType;

    /// <summary>
    /// The generic type instance being referred to. Note that generic type instances are structural types and that two separate instances with the
    /// same structure will be equivalent and thus have the same value for InternedKey.
    /// </summary>
    public IGenericTypeInstance ResolvedGenericTypeInstance {
      get {
        Contract.Ensures(Contract.Result<IGenericTypeInstance>() != null);
        Contract.Ensures(this.IsFrozen);
        if (this.resolvedGenericTypeInstance == null) {
          this.isFrozen = true;
          var self = (IGenericTypeInstanceReference)this;
          var template = this.genericType.ResolvedType;
          if (template is Dummy || !template.IsGeneric)
            this.resolvedGenericTypeInstance = Dummy.GenericTypeInstance;
          else
            this.resolvedGenericTypeInstance = Immutable.GenericTypeInstance.GetGenericTypeInstance(template, self.GenericArguments, this.InternFactory);
        }
        return this.resolvedGenericTypeInstance;
      }
    }
    IGenericTypeInstance/*?*/ resolvedGenericTypeInstance;

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    public override ITypeDefinition ResolvedType {
      get { return this.ResolvedGenericTypeInstance; }
    }

    #region IGenericTypeInstanceReference Members

    IEnumerable<ITypeReference> IGenericTypeInstanceReference.GenericArguments {
      [ContractVerification(false)]
      get {
        if (this.genericArguments == null) return Enumerable<ITypeReference>.Empty;
        return this.genericArguments.AsReadOnly();
      }
    }

    #endregion

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class GenericTypeParameter : GenericParameter, IGenericTypeParameter, ICopyFrom<IGenericTypeParameter> {

    /// <summary>
    /// 
    /// </summary>
    public GenericTypeParameter() {
      this.definingType = Dummy.TypeDefinition;
    }

    /// <summary>
    /// Makes this type be a shallow copy of the given type.
    /// </summary>
    /// <param name="genericTypeParameter">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IGenericTypeParameter genericTypeParameter, IInternFactory internFactory) {
      ((ICopyFrom<IGenericParameter>)this).Copy(genericTypeParameter, internFactory);
      this.definingType = genericTypeParameter.DefiningType;
    }

    /// <summary>
    /// The generic type that defines this type parameter.
    /// </summary>
    /// <value></value>
    public ITypeDefinition DefiningType {
      get { return this.definingType; }
      set { this.definingType = value; }
    }
    ITypeDefinition definingType;

    /// <summary>
    /// Calls visitor.Visit(IGenericTypeParameter).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IGenericTypeParameterReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericTypeParameterReference)this);
    }

    #region IGenericTypeParameterReference Members

    ITypeReference IGenericTypeParameterReference.DefiningType {
      get { return this.DefiningType; }
    }

    IGenericTypeParameter IGenericTypeParameterReference.ResolvedType {
      get { return this; }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a type parameter of a generic type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class GenericTypeParameterReference : TypeReference, IGenericTypeParameterReference, ICopyFrom<IGenericTypeParameterReference> {

    /// <summary>
    /// A reference to a type parameter of a generic type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public GenericTypeParameterReference() {
      Contract.Ensures(!this.IsFrozen);

      this.definingType = Dummy.TypeReference;
      this.name = Dummy.Name;
      this.index = 0;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.definingType != null);
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
    }


    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="genericTypeParameterReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IGenericTypeParameterReference genericTypeParameterReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(genericTypeParameterReference, internFactory);
      this.definingType = genericTypeParameterReference.DefiningType;
      this.name = genericTypeParameterReference.Name;
      this.index = genericTypeParameterReference.Index;
    }

    /// <summary>
    /// A reference to the generic type that defines the referenced type parameter.
    /// </summary>
    /// <value></value>
    public ITypeReference DefiningType {
      get { return this.definingType; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.definingType = value;
      }
    }
    ITypeReference definingType;

    /// <summary>
    /// Calls visitor.Visit(IGenericTypeParameterReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.name = value;
      }
    }
    IName name;

    /// <summary>
    /// The position in the parameter list where this instance can be found.
    /// </summary>
    /// <value></value>
    public ushort Index {
      get { return this.index; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.index = value;
      }
    }
    ushort index;

    private IGenericTypeParameter Resolve() {
      Contract.Ensures(this.IsFrozen);
      Contract.Ensures(Contract.Result<IGenericTypeParameter>() != null);

      this.isFrozen = true;
      ITypeDefinition definingType = this.definingType.ResolvedType;
      if (definingType.IsGeneric && definingType.GenericParameterCount > this.index) {
        foreach (IGenericTypeParameter par in definingType.GenericParameters) {
          if (par.Index == this.index) return par;
        }
      }
      return Dummy.GenericTypeParameter;
    }


    IGenericTypeParameter IGenericTypeParameterReference.ResolvedType {
      get {
        Contract.Ensures(this.IsFrozen);
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        return this.resolvedType;
      }
    }
    IGenericTypeParameter/*?*/ resolvedType;

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public override ITypeDefinition ResolvedType {
      get {
        var result = ((IGenericTypeParameterReference)this).ResolvedType;
        return result is Dummy ? Dummy.TypeDefinition : result;
      }
    }

  }

  /// <summary>
  /// A reference to a managed pointer type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class ManagedPointerTypeReference : TypeReference, IManagedPointerTypeReference, ICopyFrom<IManagedPointerTypeReference> {

    /// <summary>
    /// A reference to a managed pointer type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public ManagedPointerTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.targetType = Dummy.TypeReference;
      this.TypeCode = PrimitiveTypeCode.Reference;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.targetType != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
    }


    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="managedPointerTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IManagedPointerTypeReference managedPointerTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(managedPointerTypeReference, internFactory);
      this.targetType = managedPointerTypeReference.TargetType;
    }

    /// <summary>
    /// Calls visitor.Visit(IManagedPointerTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Gets the type of the resolved pointer.
    /// </summary>
    /// <value>The type of the resolved pointer.</value>
    IManagedPointerType ResolvedManagedPointerType {
      get {
        Contract.Ensures(Contract.Result<IManagedPointerType>() != null);
        Contract.Ensures(this.IsFrozen);
        if (this.resolvedType == null) {
          this.isFrozen = true;
          this.resolvedType = Immutable.ManagedPointerType.GetManagedPointerType(this.targetType, this.InternFactory);
        }
        return this.resolvedType;
      }
    }
    IManagedPointerType/*?*/ resolvedType;

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public override ITypeDefinition ResolvedType {
      get { return this.ResolvedManagedPointerType; }
    }

    /// <summary>
    /// The type of value stored at the target memory location.
    /// </summary>
    /// <value></value>
    public ITypeReference TargetType {
      get { return this.targetType; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.targetType = value;
      }
    }
    ITypeReference targetType;

  }

  /// <summary>
  /// A reference to a multi-dimensional array type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  public sealed class MatrixTypeReference : ArrayTypeReference, ICopyFrom<IArrayTypeReference> {

    /// <summary>
    /// A reference to a multi-dimensional array type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public MatrixTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.rank = 1;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.rank > 0);
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="matrixTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IArrayTypeReference matrixTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(matrixTypeReference, internFactory);
      this.ElementType = matrixTypeReference.ElementType;
      if (IteratorHelper.EnumerableIsNotEmpty(matrixTypeReference.LowerBounds))
        this.lowerBounds = new List<int>(matrixTypeReference.LowerBounds);
      else
        this.lowerBounds = null;
      this.rank = matrixTypeReference.Rank;
      if (IteratorHelper.EnumerableIsNotEmpty(matrixTypeReference.Sizes))
        this.sizes = new List<ulong>(matrixTypeReference.Sizes);
      else
        this.sizes = null;
    }

    /// <summary>
    /// This type of array is a single dimensional array with zero lower bound for index values.
    /// </summary>
    /// <value></value>
    public override bool IsVector {
      get { return false; }
    }

    /// <summary>
    /// A possible empty list of lower bounds for dimension indices. When not explicitly specified, a lower bound defaults to zero.
    /// The first lower bound in the list corresponds to the first dimension. Dimensions cannot be skipped. May be null.
    /// </summary>
    /// <value></value>
    public override List<int>/*?*/ LowerBounds {
      get { return this.lowerBounds; }
      set { this.lowerBounds = value; }
    }
    List<int>/*?*/ lowerBounds;

    /// <summary>
    /// The number of array dimensions.
    /// </summary>
    /// <value></value>
    public override uint Rank {
      get { return this.rank; }
      set {
        this.rank = value;
      }
    }
    uint rank;

    /// <summary>
    /// A possible empty list of upper bounds for dimension indices.
    /// The first upper bound in the list corresponds to the first dimension. Dimensions cannot be skipped.
    /// An unspecified upper bound means that instances of this type can have an arbitrary upper bound for that dimension. May be null.
    /// </summary>
    /// <value></value>
    public override List<ulong>/*?*/ Sizes {
      get { return this.sizes; }
      set {
        this.sizes = value;
      }
    }
    List<ulong>/*?*/ sizes;

    /// <summary>
    /// Returns the type of the resolved array.
    /// </summary>
    /// <returns></returns>
    protected override IArrayType Resolve() {
      var self = (IArrayTypeReference)this;
      return Immutable.Matrix.GetMatrix(this.ElementType, this.Rank, self.LowerBounds, self.Sizes, this.InternFactory);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class MethodImplementation : IMethodImplementation, ICopyFrom<IMethodImplementation> {

    /// <summary>
    /// 
    /// </summary>
    public MethodImplementation() {
      this.containingType = Dummy.TypeDefinition;
      this.implementedMethod = Dummy.MethodReference;
      this.implementingMethod = Dummy.MethodReference;
    }

    /// <summary>
    /// Makes this implementation be a shallow copy of the given implementation.
    /// </summary>
    /// <param name="methodImplementation">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IMethodImplementation methodImplementation, IInternFactory internFactory) {
      this.containingType = methodImplementation.ContainingType;
      this.implementedMethod = methodImplementation.ImplementedMethod;
      this.implementingMethod = methodImplementation.ImplementingMethod;
    }

    /// <summary>
    /// The type that is explicitly implementing or overriding the base class virtual method or explicitly implementing an interface method.
    /// </summary>
    /// <value></value>
    public ITypeDefinition ContainingType {
      get { return this.containingType; }
      set { this.containingType = value; }
    }
    ITypeDefinition containingType;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="visitor"></param>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A reference to the method whose implementation is being provided or overridden.
    /// </summary>
    /// <value></value>
    public IMethodReference ImplementedMethod {
      get { return this.implementedMethod; }
      set { this.implementedMethod = value; }
    }
    IMethodReference implementedMethod;

    /// <summary>
    /// A reference to the method that provides the implementation.
    /// </summary>
    /// <value></value>
    public IMethodReference ImplementingMethod {
      get { return this.implementingMethod; }
      set { this.implementingMethod = value; }
    }
    IMethodReference implementingMethod;

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class NamespaceAliasForType : AliasForType, INamespaceAliasForType, ICopyFrom<INamespaceAliasForType> {

    /// <summary>
    /// 
    /// </summary>
    public NamespaceAliasForType() {
      this.containingNamespace = Dummy.NamespaceDefinition;
      this.isPublic = false;
    }


    /// <summary>
    /// Makes this alias be a shallow copy of the given alias.
    /// </summary>
    /// <param name="namespaceAliasForType">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INamespaceAliasForType namespaceAliasForType, IInternFactory internFactory) {
      ((ICopyFrom<IAliasForType>)this).Copy(namespaceAliasForType, internFactory);
      this.containingNamespace = namespaceAliasForType.ContainingNamespace;
      this.isPublic = namespaceAliasForType.IsPublic;
    }

    /// <summary>
    /// The namespace that contains this member.
    /// </summary>
    /// <value></value>
    public INamespaceDefinition ContainingNamespace {
      get { return this.containingNamespace; }
      set { this.containingNamespace = value; }
    }
    INamespaceDefinition containingNamespace;

    /// <summary>
    /// Calls visitor.Visit(INamespaceAliasForType).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// True if the type can be accessed from other assemblies.
    /// </summary>
    /// <value></value>
    public bool IsPublic {
      get { return this.isPublic; }
      set { this.isPublic = value; }
    }
    bool isPublic;


    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ContainingNamespace; }
    }

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ContainingNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.AliasedType.Name; }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class NamespaceTypeDefinition : NamedTypeDefinition, INamespaceTypeDefinition, ICopyFrom<INamespaceTypeDefinition> {

    /// <summary>
    /// 
    /// </summary>
    public NamespaceTypeDefinition() {
      this.containingUnitNamespace = Dummy.UnitNamespace;
    }

    /// <summary>
    /// Makes this type be a shallow copy of the given type.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INamespaceTypeDefinition namespaceTypeDefinition, IInternFactory internFactory) {
      ((ICopyFrom<INamedTypeDefinition>)this).Copy(namespaceTypeDefinition, internFactory);
      this.containingUnitNamespace = namespaceTypeDefinition.ContainingUnitNamespace;
      this.IsPublic = namespaceTypeDefinition.IsPublic;
      this.IsForeignObject = namespaceTypeDefinition.IsForeignObject;
    }

    /// <summary>
    /// A map from type references to potentially empty enumerations of attributes.
    /// The references are expected to be references to interface types implemented by this type
    /// and the attributes are meant to provide additional information about the relationship between this type and its interface.
    /// May be null.
    /// </summary>
    public Dictionary<ITypeReference, IEnumerable<ICustomAttribute>>/*?*/ AttributesFor {
      get { return this.attributesFor; }
      set { this.attributesFor = value; }
    }
    Dictionary<ITypeReference, IEnumerable<ICustomAttribute>>/*?*/ attributesFor;

    /// <summary>
    /// The namespace that contains this member.
    /// </summary>
    /// <value></value>
    public IUnitNamespace ContainingUnitNamespace {
      get { return containingUnitNamespace; }
      set { this.containingUnitNamespace = value; }
    }
    IUnitNamespace containingUnitNamespace;

    /// <summary>
    /// Calls visitor.Visit(INamespaceTypeDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(INamespaceTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INamespaceTypeReference)this);
    }

    /// <summary>
    /// True if the type can be accessed from other assemblies.
    /// </summary>
    /// <value></value>
    public bool IsPublic {
      get {
        return (((TypeMemberVisibility)this.flags) & TypeMemberVisibility.Mask) == TypeMemberVisibility.Public;
      }
      set {
        this.flags &= (NamedTypeDefinition.Flags)~TypeMemberVisibility.Mask;
        if (value)
          this.flags |= (NamedTypeDefinition.Flags)TypeMemberVisibility.Public;
      }
    }

    /// <summary>
    /// True if this reference should be kept distinct from the definition it refers to. That is, when copied or persisted,
    /// this object should not be unified with the referenced type, even if the referenced type is defined in the same
    /// module as the reference to the type.
    /// </summary>
    public bool KeepDistinctFromDefinition {
      get { return (this.flags & Flags.KeepReferencesDistinctFromDefinition) != 0; }
      set {
        if (value)
          this.flags |= Flags.KeepReferencesDistinctFromDefinition;
        else
          this.flags &= ~Flags.KeepReferencesDistinctFromDefinition;
      }
    }

    /// <summary>
    /// True if objects of this type are neither COM objects nor native to the CLR and are accessed via some kind of interoperation mechanism.
    /// </summary>
    public bool IsForeignObject {
      get { return (this.flags & Flags.IsForeignObject) != 0; }
      set {
        if (value)
          this.flags |= Flags.IsForeignObject;
        else
          this.flags &= ~Flags.IsForeignObject;
      }
    }

    #region INamespaceMember Members

    INamespaceDefinition INamespaceMember.ContainingNamespace {
      get { return this.ContainingUnitNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ContainingUnitNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ContainingUnitNamespace; }
    }

    #endregion

    #region INamespaceTypeReference Members

    IUnitNamespaceReference INamespaceTypeReference.ContainingUnitNamespace {
      get { return this.ContainingUnitNamespace; }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region INamespaceTypeDefinition Members

    IEnumerable<ICustomAttribute> INamespaceTypeDefinition.AttributesFor(ITypeReference implementedInterface) {
      IEnumerable<ICustomAttribute> result;
      if (this.AttributesFor != null && this.AttributesFor.TryGetValue(implementedInterface, out result)) return result;
      return Dummy.Type.Attributes;
    }

    #endregion

  }

  /// <summary>
  /// A reference to a namespace type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class NamespaceTypeReference : TypeReference, INamespaceTypeReference, ICopyFrom<INamespaceTypeReference> {

    /// <summary>
    /// A reference to a namespace type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public NamespaceTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.containingUnitNamespace = Dummy.UnitNamespaceReference;
      this.genericParameterCount = 0;
      this.mangleName = true;
      this.name = Dummy.Name;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.containingUnitNamespace != null);
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="namespaceTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INamespaceTypeReference namespaceTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(namespaceTypeReference, internFactory);
      this.containingUnitNamespace = namespaceTypeReference.ContainingUnitNamespace;
      this.genericParameterCount = namespaceTypeReference.GenericParameterCount;
      this.mangleName = namespaceTypeReference.MangleName;
      this.name = namespaceTypeReference.Name;
      this.keepDistinctFromDefinition = namespaceTypeReference.KeepDistinctFromDefinition;
    }

    /// <summary>
    /// If this type reference can be resolved and it resolves to a type alias, the resolution continues on
    /// to resolve the reference to the aliased type. This property provides a way to discover how that resolution
    /// proceeded, by exposing the alias concerned. Think of this as a version of ResolvedType that does not
    /// traverse aliases.
    /// </summary>
    public override IAliasForType AliasForType {
      get {
        if (this.aliasForType == null)
          this.resolvedType = this.Resolve(); //Also initializes this.aliasForType
        return this.aliasForType;
      }
    }
    IAliasForType/*?*/ aliasForType;

    /// <summary>
    /// The namespace that contains the referenced type.
    /// </summary>
    /// <value></value>
    public IUnitNamespaceReference ContainingUnitNamespace {
      get { return this.containingUnitNamespace; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.containingUnitNamespace = value;
      }
    }
    IUnitNamespaceReference containingUnitNamespace;

    /// <summary>
    /// Calls visitor.Visit(INamespaceTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    /// <value></value>
    public ushort GenericParameterCount {
      get { return this.genericParameterCount; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.genericParameterCount = value;
      }
    }
    ushort genericParameterCount;

    /// <summary>
    /// The namespace type this reference resolves to.
    /// </summary>
    private INamespaceTypeDefinition Resolve() {
      Contract.Ensures(this.IsFrozen);
      Contract.Ensures(Contract.Result<INamespaceTypeDefinition>() != null);
      Contract.Ensures(this.aliasForType != null);

      this.isFrozen = true;
      this.aliasForType = Dummy.AliasForType;
      foreach (INamespaceMember member in this.ContainingUnitNamespace.ResolvedUnitNamespace.GetMembersNamed(this.name, false)) {
        var nsTypeDef = member as INamespaceTypeDefinition;
        if (nsTypeDef != null) {
          if (nsTypeDef.GenericParameterCount == this.GenericParameterCount) return nsTypeDef;
        } else {
          var nsAlias = member as INamespaceAliasForType;
          if (nsAlias != null && nsAlias.GenericParameterCount == this.GenericParameterCount) this.aliasForType = nsAlias;
        }
      }
      if (this.aliasForType != null) {
        var resolvedType = this.aliasForType.AliasedType.ResolvedType as INamespaceTypeDefinition;
        if (resolvedType != null && !(resolvedType is Dummy) && resolvedType.GenericParameterCount == this.GenericParameterCount) return resolvedType;
      }
      return Dummy.NamespaceTypeDefinition;
    }

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    public override ITypeDefinition ResolvedType {
      get {
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        return this.resolvedType is Dummy ? Dummy.TypeDefinition : this.resolvedType;
      }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return ((INamespaceTypeReference)this).ResolvedType; }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get {
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        return this.resolvedType;
      }
    }
    INamespaceTypeDefinition/*?*/ resolvedType;

    /// <summary>
    /// If true, the persisted type name is mangled by appending "`n" where n is the number of type parameters, if the number of type parameters is greater than 0.
    /// </summary>
    /// <value></value>
    public bool MangleName {
      get { return this.mangleName; }
      set { this.mangleName = value; }
    }
    bool mangleName;

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.name = value;
      }
    }
    IName name;

    /// <summary>
    /// True if this reference should be kept distinct from the definition it refers to. That is, when copied or persisted,
    /// this object should not be unified with the referenced type, even if the referenced type is defined in the same
    /// module as the reference to the type.
    /// </summary>
    public bool KeepDistinctFromDefinition {
      get { return this.keepDistinctFromDefinition; }
      set { this.keepDistinctFromDefinition = value; }
    }
    bool keepDistinctFromDefinition;

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class NestedAliasForType : AliasForType, INestedAliasForType, ICopyFrom<INestedAliasForType> {

    /// <summary>
    /// 
    /// </summary>
    public NestedAliasForType() {
      this.containingAlias = Dummy.AliasForType;
      this.visibility = TypeMemberVisibility.Default;
    }

    /// <summary>
    /// Makes this alias be a shallow copy of the given alias.
    /// </summary>
    /// <param name="nestedAliasForType">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INestedAliasForType nestedAliasForType, IInternFactory internFactory) {
      ((ICopyFrom<IAliasForType>)this).Copy(nestedAliasForType, internFactory);
      this.containingAlias = nestedAliasForType.ContainingAlias;
      this.visibility = nestedAliasForType.Visibility;
    }

    /// <summary>
    /// The alias that contains this member.
    /// </summary>
    /// <value></value>
    public IAliasForType ContainingAlias {
      get { return this.containingAlias; }
      set { this.containingAlias = value; }
    }
    IAliasForType containingAlias;

    /// <summary>
    /// Calls visitor.Visit(INestedAliasForType).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Indicates if the member is public or confined to its containing type, derived types and/or declaring assembly.
    /// </summary>
    /// <value></value>
    public TypeMemberVisibility Visibility {
      get { return this.visibility; }
      set { this.visibility = value; }
    }
    TypeMemberVisibility visibility;


    IScope<IAliasMember> IScopeMember<IScope<IAliasMember>>.ContainingScope {
      get { return this.ContainingAlias; }
    }

    IAliasForType IContainerMember<IAliasForType>.Container {
      get { return this.ContainingAlias; }
    }

    IName IContainerMember<IAliasForType>.Name {
      get { return this.AliasedType.Name; }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public class NestedTypeDefinition : NamedTypeDefinition, INestedTypeDefinition, ICopyFrom<INestedTypeDefinition> {

    /// <summary>
    /// 
    /// </summary>
    public NestedTypeDefinition() {
      this.containingTypeDefinition = Dummy.TypeDefinition;
      this.doesNotInheritGenericParameters = false;
    }

    /// <summary>
    /// Makes this type be a shallow copy of the given type.
    /// </summary>
    /// <param name="nestedTypeDefinition">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INestedTypeDefinition nestedTypeDefinition, IInternFactory internFactory) {
      ((ICopyFrom<INamedTypeDefinition>)this).Copy(nestedTypeDefinition, internFactory);
      this.containingTypeDefinition = nestedTypeDefinition.ContainingTypeDefinition;
      this.doesNotInheritGenericParameters = nestedTypeDefinition.DoesNotInheritGenericParameters;
      this.Visibility = nestedTypeDefinition.Visibility;
    }

    /// <summary>
    /// The type definition that contains this member.
    /// </summary>
    /// <value></value>
    public ITypeDefinition ContainingTypeDefinition {
      get { return this.containingTypeDefinition; }
      set { this.containingTypeDefinition = value; }
    }
    ITypeDefinition containingTypeDefinition;

    /// <summary>
    /// If true, the type does not inherit generic parameters from its containing type.
    /// </summary>
    public bool DoesNotInheritGenericParameters {
      get { return this.doesNotInheritGenericParameters; }
      set { this.doesNotInheritGenericParameters = value; }
    }
    bool doesNotInheritGenericParameters;

    /// <summary>
    /// Indicates if the member is public or confined to its containing type, derived types and/or declaring assembly.
    /// </summary>
    /// <value></value>
    public TypeMemberVisibility Visibility {
      get { return ((TypeMemberVisibility)this.flags) & TypeMemberVisibility.Mask; }
      set {
        this.flags &= (NamedTypeDefinition.Flags)~TypeMemberVisibility.Mask;
        this.flags |= (NamedTypeDefinition.Flags)(value & TypeMemberVisibility.Mask);
      }
    }

    /// <summary>
    /// Calls visitor.Visit(INestedTypeDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(INestedTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INestedTypeReference)this);
    }

    ushort INamedTypeDefinition.GenericParameterCount {
      get { return this.GenericParameterCount; }
    }

    ushort INestedTypeDefinition.GenericParameterCount {
      get { return this.GenericParameterCount; }
    }

    #region IContainerMember<ITypeDefinition> Members

    ITypeDefinition IContainerMember<ITypeDefinition>.Container {
      get { return this.ContainingTypeDefinition; }
    }

    IName IContainerMember<ITypeDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    IScope<ITypeDefinitionMember> IScopeMember<IScope<ITypeDefinitionMember>>.ContainingScope {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region ITypeMemberReference Members

    ITypeReference ITypeMemberReference.ContainingType {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region INestedTypeReference Members

    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region ITypeMemberReference Members

    /// <summary>
    /// The type definition member this reference resolves to.
    /// </summary>
    /// <value></value>
    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class SpecializedNestedTypeDefinition : NestedTypeDefinition, ISpecializedNestedTypeDefinition, ICopyFrom<ISpecializedNestedTypeDefinition> {

    /// <summary>
    /// 
    /// </summary>
    public SpecializedNestedTypeDefinition() {
      this.unspecializedVersion = Dummy.NestedTypeDefinition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedNestedTypeDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition, IInternFactory internFactory) {
      ((ICopyFrom<INestedTypeDefinition>)this).Copy(specializedNestedTypeDefinition, internFactory);
      this.unspecializedVersion = specializedNestedTypeDefinition.UnspecializedVersion;
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedNestedTypeDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedNestedTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((ISpecializedNestedTypeReference)this);
    }

    /// <summary>
    /// The property that has been specialized to obtain this event. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    public INestedTypeDefinition UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    INestedTypeDefinition unspecializedVersion;

    INestedTypeReference ISpecializedNestedTypeReference.UnspecializedVersion {
      get { return this.UnspecializedVersion; }
    }

  }

  /// <summary>
  /// A reference to a nested type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public class NestedTypeReference : TypeReference, INestedTypeReference, ICopyFrom<INestedTypeReference> {

    /// <summary>
    /// A reference to a nested type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public NestedTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.containingType = Dummy.TypeReference;
      this.genericParameterCount = 0;
      this.mangleName = true;
      this.name = Dummy.Name;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.containingType != null);
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
    }


    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="nestedTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INestedTypeReference nestedTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(nestedTypeReference, internFactory);
      this.containingType = nestedTypeReference.ContainingType;
      this.genericParameterCount = nestedTypeReference.GenericParameterCount;
      this.mangleName = nestedTypeReference.MangleName;
      this.name = nestedTypeReference.Name;
    }

    /// <summary>
    /// If this type reference can be resolved and it resolves to a type alias, the resolution continues on
    /// to resolve the reference to the aliased type. This property provides a way to discover how that resolution
    /// proceeded, by exposing the alias concerned. Think of this as a version of ResolvedType that does not
    /// traverse aliases.
    /// </summary>
    public override IAliasForType AliasForType {
      get {
        if (this.aliasForType == null)
          this.resolvedType = this.Resolve(); //Also initializes this.aliasForType
        return this.aliasForType;
      }
    }
    IAliasForType/*?*/ aliasForType;

    /// <summary>
    /// A reference to the containing type of the referenced type member.
    /// </summary>
    /// <value></value>
    public ITypeReference ContainingType {
      get { return this.containingType; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.containingType = value;
      }
    }
    ITypeReference containingType;

    /// <summary>
    /// Calls visitor.Visit(INestedTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    /// <value></value>
    public ushort GenericParameterCount {
      get { return this.genericParameterCount; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.genericParameterCount = value;
      }
    }
    ushort genericParameterCount;

    /// <summary>
    /// If true, the persisted type name is mangled by appending "`n" where n is the number of type parameters, if the number of type parameters is greater than 0.
    /// </summary>
    /// <value></value>
    public bool MangleName {
      get { return this.mangleName; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.mangleName = value;
      }
    }
    bool mangleName;

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.name = value;
      }
    }
    IName name;

    private INestedTypeDefinition Resolve() {
      Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);
      Contract.Ensures(this.IsFrozen);
      Contract.Ensures(this.aliasForType != null);

      this.isFrozen = true;
      this.aliasForType = Dummy.AliasForType;
      foreach (ITypeDefinitionMember member in this.ContainingType.ResolvedType.GetMembersNamed(this.name, false)) {
        INestedTypeDefinition/*?*/ neType = member as INestedTypeDefinition;
        if (neType != null && neType.GenericParameterCount == this.genericParameterCount) {
          if (this.ContainingType.IsAlias) {
            //Then there must be an entry for this nested type in the exported types collection.
            var assembly = TypeHelper.GetDefiningUnitReference(this).ResolvedUnit as IAssembly;
            if (assembly != null) {
              foreach (var alias in assembly.ExportedTypes) {
                var neAlias = alias as INestedAliasForType;
                if (neAlias == null) continue;
                if (neAlias.Name.UniqueKey != this.Name.UniqueKey) continue;
                if (neAlias.GenericParameterCount != this.GenericParameterCount) continue;
                if (neAlias.ContainingAlias != this.ContainingType.AliasForType) continue;
                this.aliasForType = neAlias;
                break;
              }
            }
          }
          return neType;
        }
      }
      return Dummy.NestedTypeDefinition;
    }

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public override ITypeDefinition ResolvedType {
      [ContractVerification(false)]
      get {
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        Contract.Assume(!(this is ITypeDefinition));
        if (this.resolvedType is Dummy) return Dummy.TypeDefinition;
        return this.resolvedType;
      }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get {
        var result = this.ResolvedType as INamedTypeDefinition;
        if (result == null || result is Dummy) return Dummy.NamedTypeDefinition;
        return result;
      }
    }

    INestedTypeDefinition INestedTypeReference.ResolvedType {
      [ContractVerification(false)]
      get {
        if (this.resolvedType == null)
          this.resolvedType = this.Resolve();
        Contract.Assume(!(this is ITypeDefinition));
        return this.resolvedType;
      }
    }
    INestedTypeDefinition/*?*/ resolvedType;


    #region ITypeMemberReference Members

    /// <summary>
    /// The type definition member this reference resolves to.
    /// </summary>
    /// <value></value>
    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return ((INestedTypeReference)this).ResolvedType; }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a pointer to unmanaged type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class PointerTypeReference : TypeReference, IPointerTypeReference, ICopyFrom<IPointerTypeReference> {

    /// <summary>
    /// A reference to a pointer to unmanaged type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public PointerTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.targetType = Dummy.TypeReference;
      this.TypeCode = PrimitiveTypeCode.Pointer;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.targetType != null);
      Contract.Invariant(this.resolvedType == null || this.IsFrozen);
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="pointerTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IPointerTypeReference pointerTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(pointerTypeReference, internFactory);
      this.targetType = pointerTypeReference.TargetType;
    }

    /// <summary>
    /// Calls visitor.Visit(IPointerTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Gets the type of the resolved pointer.
    /// </summary>
    /// <value>The type of the resolved pointer.</value>
    IPointerType ResolvedPointerType {
      get {
        Contract.Ensures(Contract.Result<IPointerType>() != null);
        if (this.resolvedType == null) {
          this.isFrozen = true;
          this.resolvedType = Immutable.PointerType.GetPointerType(this.targetType, this.InternFactory);
        }
        return this.resolvedType;
      }
    }
    IPointerType/*?*/ resolvedType;

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public override ITypeDefinition ResolvedType {
      get { return this.ResolvedPointerType; }
    }

    /// <summary>
    /// The type of value stored at the target memory location.
    /// </summary>
    /// <value></value>
    public ITypeReference TargetType {
      get { return this.targetType; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.targetType = value;
      }
    }
    ITypeReference targetType;

  }

  /// <summary>
  /// A reference to a specialized nested type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class SpecializedNestedTypeReference : NestedTypeReference, ISpecializedNestedTypeReference, ICopyFrom<ISpecializedNestedTypeReference> {

    /// <summary>
    /// A reference to a specialized nested type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public SpecializedNestedTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.unspecializedVersion = Dummy.NestedTypeDefinition;
      Contract.Assume(!(this.unspecializedVersion.ContainingType is ISpecializedNestedTypeReference));
      Contract.Assume(!(this.unspecializedVersion.ContainingType is IGenericTypeInstanceReference));
      Contract.Assume(!(this.unspecializedVersion is ISpecializedNestedTypeReference));
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.unspecializedVersion != null);
      Contract.Invariant(!(this.unspecializedVersion.ContainingType is ISpecializedNestedTypeReference));
      Contract.Invariant(!(this.unspecializedVersion.ContainingType is IGenericTypeInstanceReference));
      Contract.Invariant(!(this.unspecializedVersion is ISpecializedNestedTypeReference));
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="specializedNestedTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(ISpecializedNestedTypeReference specializedNestedTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<INestedTypeReference>)this).Copy(specializedNestedTypeReference, internFactory);
      this.unspecializedVersion = specializedNestedTypeReference.UnspecializedVersion;
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedNestedTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A reference to the nested type that has been specialized to obtain this nested type reference. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    /// <value></value>
    public INestedTypeReference UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(!(value.ContainingType is ISpecializedNestedTypeReference));
        Contract.Requires(!(value.ContainingType is IGenericTypeInstanceReference));
        Contract.Requires(!(value is ISpecializedNestedTypeReference));
        this.unspecializedVersion = value;
      }
    }
    INestedTypeReference unspecializedVersion;

    /// <summary>
    /// A specialized type reference should always defer to its unspecialized version for whether it is a value type or not: that can't change.
    /// </summary>
    public new bool IsValueType {
      get { return this.unspecializedVersion.IsValueType; }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class NamedTypeDefinition : INamedTypeDefinition, ICopyFrom<INamedTypeDefinition> {

    /// <summary>
    /// 
    /// </summary>
    internal NamedTypeDefinition() {
      this.alignment = 0;
      this.attributes = null;
      this.baseClasses = null;
      this.explicitImplementationOverrides = null;
      this.events = null;
      this.fields = null;
      this.genericParameters = null;
      this.interfaces = null;
      this.internFactory = Dummy.InternFactory;
      this.layout = LayoutKind.Auto;
      this.locations = null;
      this.MangleName = true;
      this.methods = null;
      this.name = Dummy.Name;
      this.nestedTypes = null;
      this.platformType = Dummy.PlatformType;
      this.privateHelperMembers = null;
      this.properties = null;
      this.securityAttributes = null;
      this.sizeOf = 0;
      this.stringFormat = StringFormatKind.Ansi;
      this.template = Dummy.TypeDefinition;
      this.typeCode = PrimitiveTypeCode.NotPrimitive;
      this.underlyingType = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes this type be a shallow copy of the given type.
    /// </summary>
    /// <param name="typeDefinition">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(INamedTypeDefinition typeDefinition, IInternFactory internFactory) {
      this.alignment = typeDefinition.Alignment;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Attributes))
        this.attributes = new List<ICustomAttribute>(typeDefinition.Attributes);
      else
        this.attributes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.BaseClasses))
        this.baseClasses = new List<ITypeReference>(typeDefinition.BaseClasses);
      else
        this.baseClasses = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Events))
        this.events = new List<IEventDefinition>(typeDefinition.Events);
      else
        this.events = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.ExplicitImplementationOverrides))
        this.explicitImplementationOverrides = new List<IMethodImplementation>(typeDefinition.ExplicitImplementationOverrides);
      else
        this.explicitImplementationOverrides = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Fields))
        this.fields = new List<IFieldDefinition>(typeDefinition.Fields);
      else
        this.fields = null;
      if (typeDefinition.IsGeneric)
        this.genericParameters = new List<IGenericTypeParameter>(typeDefinition.GenericParameters);
      else
        this.genericParameters = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Interfaces))
        this.interfaces = new List<ITypeReference>(typeDefinition.Interfaces);
      else
        this.interfaces = null;
      this.internFactory = internFactory;
      this.layout = typeDefinition.Layout;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Locations))
        this.locations = new List<ILocation>(typeDefinition.Locations);
      else
        this.locations = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Methods))
        this.methods = new List<IMethodDefinition>(typeDefinition.Methods);
      else
        this.methods = null;
      this.name = typeDefinition.Name;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.NestedTypes))
        this.nestedTypes = new List<INestedTypeDefinition>(typeDefinition.NestedTypes);
      else
        this.nestedTypes = null;
      this.platformType = typeDefinition.PlatformType;
      this.privateHelperMembers = null;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinition.Properties))
        this.properties = new List<IPropertyDefinition>(typeDefinition.Properties);
      else
        this.properties = null;
      if (typeDefinition.HasDeclarativeSecurity)
        this.securityAttributes = new List<ISecurityAttribute>(typeDefinition.SecurityAttributes);
      else
        this.securityAttributes = null;
      this.sizeOf = typeDefinition.SizeOf;
      this.stringFormat = typeDefinition.StringFormat;
      this.template = typeDefinition;
      this.typeCode = typeDefinition.TypeCode;
      if (typeDefinition.IsEnum)
        this.underlyingType = typeDefinition.UnderlyingType;
      else
        this.underlyingType = Dummy.TypeReference;
      //^ base();
      this.HasDeclarativeSecurity = typeDefinition.HasDeclarativeSecurity;
      this.IsAbstract = typeDefinition.IsAbstract;
      this.IsBeforeFieldInit = typeDefinition.IsBeforeFieldInit;
      this.IsClass = typeDefinition.IsClass;
      this.IsComObject = typeDefinition.IsComObject;
      this.IsDelegate = typeDefinition.IsDelegate;
      this.IsEnum = typeDefinition.IsEnum;
      this.IsInterface = typeDefinition.IsInterface;
      this.IsRuntimeSpecial = typeDefinition.IsRuntimeSpecial;
      this.IsSealed = typeDefinition.IsSealed;
      this.IsSerializable = typeDefinition.IsSerializable;
      this.IsSpecialName = typeDefinition.IsSpecialName;
      this.IsStatic = typeDefinition.IsStatic;
      this.IsStruct = typeDefinition.IsStruct;
      this.MangleName = typeDefinition.MangleName;
      if (typeDefinition.IsValueType) this.flags |= Flags.ValueType;
    }

    /// <summary>
    /// The byte alignment that values of the given type ought to have. Must be a power of 2. If zero, the alignment is decided at runtime.
    /// </summary>
    /// <value></value>
    public virtual ushort Alignment {
      get { return this.alignment; }
      set { this.alignment = value; }
    }
    ushort alignment;

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ Attributes {
      get { return this.attributes; }
      set { this.attributes = value; }
    }
    List<ICustomAttribute>/*?*/ attributes;

    /// <summary>
    /// Zero or more classes from which this type is derived.
    /// For CLR types this collection is empty for interfaces and System.Object and populated with exactly one base type for all other types. May be null.
    /// </summary>
    /// <value></value>
    public virtual List<ITypeReference>/*?*/ BaseClasses {
      get { return this.baseClasses; }
      set { this.baseClasses = value; }
    }
    List<ITypeReference>/*?*/ baseClasses;

    //^ [Pure]
    /// <summary>
    /// Return true if the given member instance is a member of this scope.
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public bool Contains(ITypeDefinitionMember member) {
      foreach (ITypeDefinitionMember tdmem in this.Members)
        if (member == tdmem) return true;
      return false;
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference. The dispatch method does nothing else.
    /// </summary>
    public abstract void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference, which is not derived from IDefinition. For example an object implemeting IArrayType will
    /// call visitor.Visit(IArrayTypeReference) and not visitor.Visit(IArrayType).
    /// The dispatch method does nothing else.
    /// </summary>
    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    /// <summary>
    /// Zero or more events defined by this type. May be null.
    /// </summary>
    /// <value></value>
    public List<IEventDefinition>/*?*/ Events {
      get { return this.events; }
      set { this.events = value; }
    }
    List<IEventDefinition>/*?*/ events;

    /// <summary>
    /// Zero or more implementation overrides provided by the class. May be null.
    /// </summary>
    /// <value></value>
    public List<IMethodImplementation>/*?*/ ExplicitImplementationOverrides {
      get { return this.explicitImplementationOverrides; }
      set { this.explicitImplementationOverrides = value; }
    }
    List<IMethodImplementation>/*?*/ explicitImplementationOverrides;

    /// <summary>
    /// Zero or more fields defined by this type. May be null.
    /// </summary>
    /// <value></value>
    public List<IFieldDefinition>/*?*/ Fields {
      get { return this.fields; }
      set { this.fields = value; }
    }
    List<IFieldDefinition>/*?*/ fields;

    [Flags]
    internal enum Flags {
      Abstract=0x40000000,
      Class=0x20000000,
      Delegate=0x10000000,
      Enum=0x08000000,
      HasDeclarativeSecurity=0x04000000,
      Interface=0x02000000,
      Sealed=0x01000000,
      Static=0x00800000,
      Struct=0x00400000,
      ValueType=0x00200000,
      IsRuntimeSpecialName=0x00100000,
      IsSpecialName=0x00080000,
      IsComObject=0x00040000,
      IsSerializable=0x00020000,
      IsBeforeFieldInit=0x00010000,
      MustBeReferenceType=0x00008000,
      MustBeValueType=0x00004000,
      MustHaveDefaultConstructor=0x00002000,
      MangleName=0x00001000,
      KeepReferencesDistinctFromDefinition=0x0000800,
      IsForeignObject=0x00000400,
      None=0x00000000,
    }
    internal Flags flags;

    /// <summary>
    /// Get the reference to fully specialized/instantiated version of typeDefinition. 
    /// </summary>
    /// <param name="typeDefinition">Unspecialized type definition to be specialized/instantiated.</param>
    /// <param name="internFactory">An internfactory. </param>
    public static ITypeDefinition SelfInstance(INamedTypeDefinition typeDefinition, IInternFactory internFactory) {
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);

      INamespaceTypeDefinition namespaceTypeDefinition = typeDefinition as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) {
        if (typeDefinition.IsGeneric)
          return typeDefinition.InstanceType.ResolvedType;
        else
          return typeDefinition;
      }
      INestedTypeDefinition nestedTypeDefinition = typeDefinition as INestedTypeDefinition;
      INamedTypeDefinition result = typeDefinition;
      if (nestedTypeDefinition != null) {
        var containingTypeDefinition = SelfInstance((INamedTypeDefinition)nestedTypeDefinition.ContainingTypeDefinition, internFactory);
        var ctDef = containingTypeDefinition;
        var genericTypeInstance = containingTypeDefinition as Immutable.GenericTypeInstance;
        while (genericTypeInstance == null) {
          var specializedNestedTypeRef = ctDef as ISpecializedNestedTypeReference;
          if (specializedNestedTypeRef != null) {
            ctDef = specializedNestedTypeRef.ContainingType.ResolvedType;
            genericTypeInstance = ctDef as Immutable.GenericTypeInstance;
          } else {
            break;
          }
        }
        if (genericTypeInstance != null) {
          result = new Immutable.SpecializedNestedTypeDefinition(nestedTypeDefinition, nestedTypeDefinition, containingTypeDefinition, genericTypeInstance, internFactory);
        }
      }
      if (typeDefinition.IsGeneric) {
        var args = new List<ITypeReference>();
        foreach (var gpar in typeDefinition.GenericParameters)
          args.Add(gpar);
        return Immutable.GenericTypeInstance.GetGenericTypeInstance(result, args, internFactory);
      }
      return result;
    }

    /// <summary>
    /// Zero or more parameters that can be used as type annotations. May be null.
    /// </summary>
    /// <value></value>
    public virtual List<IGenericTypeParameter>/*?*/ GenericParameters {
      get { return this.genericParameters; }
      set { this.genericParameters = value; }
    }
    List<IGenericTypeParameter>/*?*/ genericParameters;

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    /// <value></value>
    public ushort GenericParameterCount {
      get {
        if (this.GenericParameters == null) return 0;
        return (ushort)this.GenericParameters.Count;
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members with the given name that also satisfy the given predicate.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      foreach (ITypeDefinitionMember tdmem in this.Members) {
        if (tdmem.Name.UniqueKey == name.UniqueKey || ignoreCase && (name.UniqueKeyIgnoringCase == tdmem.Name.UniqueKeyIgnoringCase)) {
          if (predicate(tdmem)) yield return tdmem;
        }
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members that satisfy the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      foreach (ITypeDefinitionMember tdmem in this.Members) {
        if (predicate(tdmem)) yield return tdmem;
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      foreach (ITypeDefinitionMember tdmem in this.Members) {
        if (tdmem.Name.UniqueKey == name.UniqueKey || ignoreCase && (name.UniqueKeyIgnoringCase == tdmem.Name.UniqueKeyIgnoringCase)) {
          yield return tdmem;
        }
      }
    }

    /// <summary>
    /// True if this type has a non empty collection of SecurityAttributes or the System.Security.SuppressUnmanagedCodeSecurityAttribute.
    /// </summary>
    /// <value></value>
    public bool HasDeclarativeSecurity {
      get { return (this.flags & Flags.HasDeclarativeSecurity) != 0; }
      set {
        if (value)
          this.flags |= Flags.HasDeclarativeSecurity;
        else
          this.flags &= ~Flags.HasDeclarativeSecurity;
      }
    }

    /// <summary>
    /// An instance of this generic type that has been obtained by using the generic parameters as the arguments.
    /// Use this instance to look up members
    /// </summary>
    /// <value></value>
    public IGenericTypeInstanceReference InstanceType {
      get {
        if (this.instanceType == null) {
          lock (GlobalLock.LockingObject) {
            if (this.instanceType == null) {
              List<ITypeReference> arguments = new List<ITypeReference>();
              foreach (IGenericTypeParameter gpar in this.GenericParameters) arguments.Add(gpar);
              this.instanceType = new Immutable.GenericTypeInstanceReference(this.GetSpecializedType(this), arguments, this.InternFactory);
            }
          }
        }
        return this.instanceType;
      }
    }
    IGenericTypeInstanceReference/*?*/ instanceType;
    //^ invariant instanceType == null || !instanceType.IsGeneric;

    /// <summary>
    /// Return a specialized nested type obtained from the specialized instance of its containing type if this type is a nested type, or this otherwise.
    /// </summary>
    /// <returns></returns>
    protected INamedTypeReference GetSpecializedType(INamedTypeDefinition typeDef) {
      var nestedType = typeDef as INestedTypeDefinition;
      if (nestedType != null) {
        ITypeReference containingTypeReference = null;
        if (nestedType.ContainingTypeDefinition.IsGeneric)
          containingTypeReference = nestedType.ContainingTypeDefinition.InstanceType;
        else {
          containingTypeReference = this.GetSpecializedType((INamedTypeDefinition)nestedType.ContainingTypeDefinition);
          if (containingTypeReference == nestedType.ContainingTypeDefinition) return typeDef;
        }
        return new Immutable.SpecializedNestedTypeReference(nestedType, containingTypeReference, this.InternFactory);
      }
      return typeDef;
    }

    /// <summary>
    /// Zero or more interfaces implemented by this type. May be null.
    /// </summary>
    /// <value></value>
    public virtual List<ITypeReference>/*?*/ Interfaces {
      get { return this.interfaces; }
      set { this.interfaces = value; }
    }
    List<ITypeReference>/*?*/ interfaces;

    /// <summary>
    /// True if the type may not be instantiated.
    /// </summary>
    /// <value></value>
    public bool IsAbstract {
      get { return (this.flags & Flags.Abstract) != 0; }
      set {
        if (value)
          this.flags |= Flags.Abstract;
        else
          this.flags &= ~Flags.Abstract;
      }
    }

    /// <summary>
    /// True if the type is a class (it is not an interface or type parameter and does not extend a special base class).
    /// Corresponds to C# class.
    /// </summary>
    /// <value></value>
    public bool IsClass {
      get { return (this.flags & Flags.Class) != 0; }
      set {
        if (value)
          this.flags |= Flags.Class;
        else
          this.flags &= ~Flags.Class;
      }
    }

    /// <summary>
    /// True if the type is a delegate (it extends System.MultiCastDelegate). Corresponds to C# delegate
    /// </summary>
    /// <value></value>
    public bool IsDelegate {
      get { return (this.flags & Flags.Delegate) != 0; }
      set {
        if (value)
          this.flags |= Flags.Delegate;
        else
          this.flags &= ~Flags.Delegate;
      }
    }

    /// <summary>
    /// True if the type is an enumeration (it extends System.Enum and is sealed). Corresponds to C# enum.
    /// </summary>
    /// <value></value>
    public bool IsEnum {
      get { return (this.flags & Flags.Enum) != 0; }
      set {
        if (value)
          this.flags |= Flags.Enum;
        else
          this.flags &= ~Flags.Enum;
      }
    }

    /// <summary>
    /// True if this type is parameterized (this.GenericParameters is a non empty collection).
    /// </summary>
    /// <value></value>
    public bool IsGeneric {
      get { return this.GenericParameters != null && this.GenericParameters.Count > 0; }
    }

    /// <summary>
    /// True if the type is an interface.
    /// </summary>
    /// <value></value>
    public bool IsInterface {
      get { return (this.flags & Flags.Interface) != 0; }
      set {
        if (value)
          this.flags |= Flags.Interface;
        else
          this.flags &= ~Flags.Interface;
      }
    }

    /// <summary>
    /// True if the type is a reference type. A reference type is non static class or interface or a suitably constrained type parameter.
    /// A type parameter for which MustBeReferenceType (the class constraint in C#) is true returns true for this property
    /// as does a type parameter with a constraint that is a class.
    /// </summary>
    /// <value></value>
    public virtual bool IsReferenceType {
      get { return (this.flags & (Flags.Enum|Flags.ValueType|Flags.Static)) == 0; }
    }

    /// <summary>
    /// True if the type may not be subtyped.
    /// </summary>
    /// <value></value>
    public bool IsSealed {
      get { return (this.flags & Flags.Sealed) != 0; }
      set {
        if (value)
          this.flags |= Flags.Sealed;
        else
          this.flags &= ~Flags.Sealed;
      }
    }

    /// <summary>
    /// True if the type is an abstract sealed class that directly extends System.Object and declares no constructors.
    /// </summary>
    /// <value></value>
    public bool IsStatic {
      get { return (this.flags & Flags.Static) != 0; }
      set {
        if (value)
          this.flags |= Flags.Static;
        else
          this.flags &= ~Flags.Static;
      }
    }

    /// <summary>
    /// True if the type is a value type.
    /// Value types are sealed and extend System.ValueType or System.Enum.
    /// A type parameter for which MustBeValueType (the struct constraint in C#) is true also returns true for this property.
    /// </summary>
    /// <value></value>
    public virtual bool IsValueType {
      get { return (this.flags & Flags.ValueType) != 0; }
      set {
        if (value)
          this.flags |= Flags.ValueType;
        else
          this.flags &= ~Flags.ValueType;
      }
    }

    /// <summary>
    /// True if this type gets special treatment from the runtime.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeSpecial {
      get { return (this.flags & Flags.IsRuntimeSpecialName) != 0; }
      set {
        if (value)
          this.flags |= Flags.IsRuntimeSpecialName;
        else
          this.flags &= ~Flags.IsRuntimeSpecialName;
      }
    }

    /// <summary>
    /// True if the type is a struct (its not Primitive, is sealed and base is System.ValueType).
    /// </summary>
    /// <value></value>
    public bool IsStruct {
      get { return (this.flags & Flags.Struct) != 0; }
      set {
        if (value)
          this.flags |= Flags.Struct;
        else
          this.flags &= ~Flags.Struct;
      }
    }

    /// <summary>
    /// True if the type has special name.
    /// </summary>
    /// <value></value>
    public bool IsSpecialName {
      get { return (this.flags & Flags.IsSpecialName) != 0; }
      set {
        if (value)
          this.flags |= Flags.IsSpecialName;
        else
          this.flags &= ~Flags.IsSpecialName;
      }
    }

    /// <summary>
    /// Is this imported from COM type library
    /// </summary>
    /// <value></value>
    public bool IsComObject {
      get { return (this.flags & Flags.IsComObject) != 0; }
      set {
        if (value)
          this.flags |= Flags.IsComObject;
        else
          this.flags &= ~Flags.IsComObject;
      }
    }

    /// <summary>
    /// True if this type is serializable.
    /// </summary>
    /// <value></value>
    public bool IsSerializable {
      get { return (this.flags & Flags.IsSerializable) != 0; }
      set {
        if (value)
          this.flags |= Flags.IsSerializable;
        else
          this.flags &= ~Flags.IsSerializable;
      }
    }

    /// <summary>
    /// Is type initialized anytime before first access to static field
    /// </summary>
    /// <value></value>
    public bool IsBeforeFieldInit {
      get { return (this.flags & Flags.IsBeforeFieldInit) != 0; }
      set {
        if (value)
          this.flags |= Flags.IsBeforeFieldInit;
        else
          this.flags &= ~Flags.IsBeforeFieldInit;
      }
    }

    /// <summary>
    /// Layout of the type.
    /// </summary>
    /// <value></value>
    public LayoutKind Layout {
      get { return this.layout; }
      set { this.layout = value; }
    }
    LayoutKind layout;

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance. May be null.
    /// </summary>
    /// <value></value>
    public List<ILocation>/*?*/ Locations {
      get { return this.locations; }
      set { this.locations = value; }
    }
    List<ILocation>/*?*/ locations;

    /// <summary>
    /// If true, the persisted type name is mangled by appending "`n" where n is the number of type parameters, if the number of type parameters is greater than 0.
    /// </summary>
    /// <value></value>
    public bool MangleName {
      get { return (this.flags & Flags.MangleName) != 0; }
      set {
        if (value)
          this.flags |= Flags.MangleName;
        else
          this.flags &= ~Flags.MangleName;
      }
    }

    /// <summary>
    /// The collection of member instances that are members of this scope.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeDefinitionMember> Members {
      get {
        if (this.Events != null) {
          foreach (var eventDefinition in this.Events)
            yield return eventDefinition;
        }
        if (this.Fields != null) {
          foreach (var fieldDefinition in this.Fields)
            yield return fieldDefinition;
        }
        if (this.Methods != null) {
          foreach (var methodDefinition in this.Methods)
            yield return methodDefinition;
        }
        if (this.NestedTypes != null) {
          foreach (var nestedTypeDefinition in this.NestedTypes)
            yield return nestedTypeDefinition;
        }
        if (this.Properties != null) {
          foreach (var propertyDefinition in this.Properties)
            yield return propertyDefinition;
        }
      }
    }

    /// <summary>
    /// Zero or more methods defined by this type. May be null.
    /// </summary>
    /// <value></value>
    public List<IMethodDefinition>/*?*/ Methods {
      get { return this.methods; }
      set { this.methods = value; }
    }
    List<IMethodDefinition>/*?*/ methods;

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set { this.name = value; }
    }
    IName name;

    /// <summary>
    /// Zero or more nested types defined by this type. May be null.
    /// </summary>
    /// <value></value>
    public List<INestedTypeDefinition>/*?*/ NestedTypes {
      get { return this.nestedTypes; }
      set { this.nestedTypes = value; }
    }
    List<INestedTypeDefinition>/*?*/ nestedTypes;

    /// <summary>
    /// A way to get to platform types such as System.Object.
    /// </summary>
    /// <value></value>
    public IPlatformType PlatformType {
      get { return this.platformType; }
      set { this.platformType = value; }
    }
    IPlatformType platformType;

    /// <summary>
    /// Zero or more private type members generated by the compiler for implementation purposes. These members
    /// are only available after a complete visit of all of the other members of the type, including the bodies of methods.
    /// </summary>
    /// <value></value>
    public List<ITypeDefinitionMember> PrivateHelperMembers {
      get {
        if (this.privateHelperMembers == null) {
          this.privateHelperMembers = new List<ITypeDefinitionMember>(this.template.PrivateHelperMembers);
          this.template = Dummy.TypeDefinition;
        }
        return this.privateHelperMembers;
      }
      set { this.privateHelperMembers = value; }
    }
    List<ITypeDefinitionMember>/*?*/ privateHelperMembers;
    ITypeDefinition template;

    /// <summary>
    /// Zero or more properties defined by this type. May be null.
    /// </summary>
    /// <value></value>
    public List<IPropertyDefinition>/*?*/ Properties {
      get { return this.properties; }
      set { this.properties = value; }
    }
    List<IPropertyDefinition>/*?*/ properties;

    /// <summary>
    /// Declarative security actions for this type. Will be empty if this.HasSecurity is false. May be null.
    /// </summary>
    /// <value></value>
    public List<ISecurityAttribute>/*?*/ SecurityAttributes {
      get { return this.securityAttributes; }
      set { this.securityAttributes = value; }
    }
    List<ISecurityAttribute>/*?*/ securityAttributes;

    /// <summary>
    /// Size of an object of this type. In bytes. If zero, the size is unspecified and will be determined at runtime.
    /// </summary>
    /// <value></value>
    public virtual uint SizeOf {
      get { return this.sizeOf; }
      set { this.sizeOf = value; }
    }
    uint sizeOf;

    /// <summary>
    /// Default marshalling of the Strings in this class.
    /// </summary>
    /// <value></value>
    public StringFormatKind StringFormat {
      get { return this.stringFormat; }
      set { this.stringFormat = value; }
    }
    StringFormatKind stringFormat;

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    /// <summary>
    /// Unless the value of TypeCode is PrimitiveTypeCode.NotPrimitive, the type corresponds to a "primitive" CLR type (such as System.Int32) and
    /// the type code identifies which of the primitive types it corresponds to.
    /// </summary>
    /// <value></value>
    public virtual PrimitiveTypeCode TypeCode {
      get { return this.typeCode; }
      set { this.typeCode = value; }
    }
    PrimitiveTypeCode typeCode;

    /// <summary>
    /// Returns a reference to the underlying (integral) type on which this (enum) type is based.
    /// </summary>
    /// <value></value>
    public ITypeReference UnderlyingType {
      get { return this.underlyingType; }
      set { this.underlyingType = value; }
    }
    ITypeReference underlyingType;

    #region ITypeDefinition Members

    IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters {
      get {
        if (this.GenericParameters == null) return Enumerable<IGenericTypeParameter>.Empty;
        return this.GenericParameters.AsReadOnly();
      }
    }

    IEnumerable<ITypeReference> ITypeDefinition.BaseClasses {
      get {
        if (this.BaseClasses == null) return Enumerable<ITypeReference>.Empty;
        return this.BaseClasses.AsReadOnly();
      }
    }

    IEnumerable<IEventDefinition> ITypeDefinition.Events {
      get {
        if (this.Events == null) return Enumerable<IEventDefinition>.Empty;
        return this.Events.AsReadOnly();
      }
    }

    IEnumerable<IMethodImplementation> ITypeDefinition.ExplicitImplementationOverrides {
      get {
        if (this.ExplicitImplementationOverrides == null) return Enumerable<IMethodImplementation>.Empty;
        return this.ExplicitImplementationOverrides.AsReadOnly();
      }
    }

    IEnumerable<IFieldDefinition> ITypeDefinition.Fields {
      get {
        if (this.Fields == null) return Enumerable<IFieldDefinition>.Empty;
        return this.Fields.AsReadOnly();
      }
    }

    IEnumerable<ITypeReference> ITypeDefinition.Interfaces {
      get {
        if (this.Interfaces == null) return Enumerable<ITypeReference>.Empty;
        return this.Interfaces.AsReadOnly();
      }
    }

    IEnumerable<IMethodDefinition> ITypeDefinition.Methods {
      get {
        if (this.Methods == null) return Enumerable<IMethodDefinition>.Empty;
        return this.Methods.AsReadOnly();
      }
    }

    IEnumerable<INestedTypeDefinition> ITypeDefinition.NestedTypes {
      get {
        if (this.NestedTypes == null) return Enumerable<INestedTypeDefinition>.Empty;
        return this.NestedTypes.AsReadOnly();
      }
    }

    IEnumerable<ITypeDefinitionMember> ITypeDefinition.PrivateHelperMembers {
      get {
        if (this.privateHelperMembers == null) return Enumerable<ITypeDefinitionMember>.Empty;
        return EnumerateTolerantly(this.privateHelperMembers);
      }
    }

    private static IEnumerable<ITypeDefinitionMember> EnumerateTolerantly(List<ITypeDefinitionMember> list) {
      for (int i = 0; i < list.Count; i++) yield return list[i];
    }

    IEnumerable<IPropertyDefinition> ITypeDefinition.Properties {
      get {
        if (this.Properties == null) return Enumerable<IPropertyDefinition>.Empty;
        return this.Properties.AsReadOnly();
      }
    }

    IEnumerable<ISecurityAttribute> ITypeDefinition.SecurityAttributes {
      get {
        if (this.SecurityAttributes == null) return Enumerable<ISecurityAttribute>.Empty;
        return this.SecurityAttributes.AsReadOnly();
      }
    }
    #endregion

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

    #region ITypeReference Members

    /// <summary>
    /// Indicates if this type reference resolved to an alias rather than a type
    /// </summary>
    /// <value></value>
    public bool IsAlias {
      get { return false; }
    }

    /// <summary>
    /// Gives the alias for the type
    /// </summary>
    /// <value></value>
    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion

    #region ITypeReference Members

    /// <summary>
    /// Returns the unique interned key associated with the type. This takes unification/aliases/custom modifiers into account.
    /// </summary>
    public uint InternedKey {
      get {
        if (this.internedKey == 0)
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        return this.internedKey;
      }
    }
    uint internedKey;

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this.ResolvedType; }
    }

    #endregion

    /// <summary>
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    public IInternFactory InternFactory {
      get { return this.internFactory; }
      set { this.internFactory = value; }
    }
    IInternFactory internFactory;

  }

  /// <summary>
  /// A modified reference to a type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class ModifiedTypeReference : TypeReference, IModifiedTypeReference, ICopyFrom<IModifiedTypeReference> {

    /// <summary>
    /// A modified reference to a type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public ModifiedTypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.customModifiers = new List<ICustomModifier>(1);
      this.unmodifiedType = Dummy.TypeReference;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.customModifiers != null);
      Contract.Invariant(this.unmodifiedType != null);
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="modifiedTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IModifiedTypeReference modifiedTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(modifiedTypeReference, internFactory);
      this.customModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
      this.unmodifiedType = modifiedTypeReference.UnmodifiedType;
    }

    /// <summary>
    /// Returns the list of custom modifiers associated with the type reference. Evaluate this property only if IsModified is true.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier> CustomModifiers {
      get { return this.customModifiers; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.customModifiers = value;
      }
    }
    List<ICustomModifier> customModifiers;

    /// <summary>
    /// Calls visitor.Visit(IModifiedTypeReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    public override ITypeDefinition ResolvedType {
      get {
        this.isFrozen = true;
        return this.unmodifiedType.ResolvedType;
      }
    }

    /// <summary>
    /// An unmodified type reference.
    /// </summary>
    /// <value></value>
    public ITypeReference UnmodifiedType {
      get { return this.unmodifiedType; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.unmodifiedType = value;
      }
    }
    ITypeReference unmodifiedType;

    #region IModifiedTypeReference Members

    IEnumerable<ICustomModifier> IModifiedTypeReference.CustomModifiers {
      [ContractVerification(false)]
      get { return this.customModifiers.AsReadOnly(); }
    }

    #endregion

  }

  /// <summary>
  /// A reference to a type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractClass(typeof(TypeReference.TypeReferenceAbstractMethodContracts))]
  [ContractVerification(true)]
  public abstract class TypeReference : ITypeReference, ICopyFrom<ITypeReference> {

    /// <summary>
    /// A reference to a type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    internal TypeReference() {
      Contract.Ensures(!this.IsFrozen);

      this.attributes = null;
      this.internFactory = Dummy.InternFactory;
      this.isEnum = false;
      this.isValueType = false;
      this.locations = null;
      this.platformType = Dummy.PlatformType;
      this.typeCode = PrimitiveTypeCode.Invalid;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.internFactory != null);
      Contract.Invariant(this.platformType != null);
      Contract.Invariant(this.internedKey == 0 || this.IsFrozen);
      //Contract.Invariant(this.IsAlias || this.AliasForType.AliasedType.ResolvedType == this.ResolvedType);
    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="typeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(ITypeReference typeReference, IInternFactory internFactory) {
      if (typeReference is ITypeDefinition)
        this.attributes = null; //the attributes of a type definition are not the same as the attributes of a type reference
      //so when a definition is being copied as a reference, it should get not attributes of its own.
      else if (IteratorHelper.EnumerableIsNotEmpty(typeReference.Attributes))
        this.attributes = new List<ICustomAttribute>(typeReference.Attributes);
      else
        this.attributes = null;
      this.internFactory = internFactory;
      this.isEnum = typeReference.IsEnum;
      this.isValueType = typeReference.IsValueType;
      if (IteratorHelper.EnumerableIsNotEmpty(typeReference.Locations))
        this.locations = new List<ILocation>(typeReference.Locations);
      else
        this.locations = null;
      this.platformType = typeReference.PlatformType;
      this.typeCode = typeReference.TypeCode;
      this.originalReference = typeReference;
    }

    ITypeReference/*?*/ originalReference;

    /// <summary>
    /// If this type reference can be resolved and it resolves to a type alias, the resolution continues on
    /// to resolve the reference to the aliased type. This property provides a way to discover how that resolution
    /// proceeded, by exposing the alias concerned. Think of this as a version of ResolvedType that does not
    /// traverse aliases.
    /// </summary>
    public virtual IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition. May be null.
    /// </summary>
    public List<ICustomAttribute>/*?*/ Attributes {
      get {
        return this.attributes;
      }
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
    public void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference, which is not derived from IDefinition. For example an object implemeting IArrayType will
    /// call visitor.Visit(IArrayTypeReference) and not visitor.Visit(IArrayType).
    /// The dispatch method does nothing else.
    /// </summary>
    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    [ContractClassFor(typeof(TypeReference))]
    abstract partial class TypeReferenceAbstractMethodContracts : TypeReference {
      public override void DispatchAsReference(IMetadataVisitor visitor) {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    public IInternFactory InternFactory {
      get {
        Contract.Ensures(Contract.Result<IInternFactory>() != null);
        return this.internFactory;
      }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.internFactory = value;
      }
    }
    IInternFactory internFactory;

    /// <summary>
    /// Returns the unique interned key associated with the type. This takes unification/aliases/custom modifiers into account.
    /// </summary>
    /// <value></value>
    public uint InternedKey {
      get {
        Contract.Ensures(this.IsFrozen);
        if (this.internedKey == 0) {
          this.isFrozen = true;
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    /// <summary>
    /// Indicates if this type reference resolved to an alias rather than a type
    /// </summary>
    public bool IsAlias {
      get {
        Contract.Assume(!(this is ITypeDefinition));
        return !(this.AliasForType is Dummy);
      }
    }

    /// <summary>
    /// True if the type is an enumeration (it extends System.Enum and is sealed). Corresponds to C# enum.
    /// </summary>
    /// <value></value>
    public bool IsEnum {
      get { return this.isEnum; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.isEnum = value;
      }
    }
    bool isEnum;

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
    /// True if the type is a value type.
    /// Value types are sealed and extend System.ValueType or System.Enum.
    /// A type parameter for which MustBeValueType (the struct constraint in C#) is true also returns true for this property.
    /// </summary>
    /// <value></value>
    public bool IsValueType {
      get {
        if (this.originalReference != null) return this.originalReference.IsValueType;
        return this.isValueType;
      }
      set {
        Contract.Requires(!this.IsFrozen);
        this.originalReference = null;
        this.isValueType = value;
      }
    }
    bool isValueType;

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type.
    /// If the type reference cannot be resolved, the result is Dummy.TypeDefinition.
    /// </summary>
    public abstract ITypeDefinition ResolvedType {
      get;
    }

    partial class TypeReferenceAbstractMethodContracts : TypeReference {
      public override ITypeDefinition ResolvedType {
        get {
          Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
          //Contract.Ensures(Contract.Result<ITypeDefinition>() == Dummy.TypeDefinition || this.IsAlias ||
          //  Contract.Result<ITypeDefinition>().InternedKey == this.InternedKey);
          Contract.Ensures(this.IsFrozen);
          throw new NotImplementedException();
        }
      }
    }

    /// <summary>
    /// A potentially null, potentially empty collection of locations that correspond to this instance.
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
    /// A way to get to platform types such as System.Object.
    /// </summary>
    /// <value></value>
    public IPlatformType PlatformType {
      get { return this.platformType; }
      set {
        Contract.Requires(value != null);
        Contract.Requires(!this.IsFrozen);
        this.platformType = value;
      }
    }
    IPlatformType platformType;

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    /// <summary>
    /// Unless the value of TypeCode is PrimitiveTypeCode.NotPrimitive, the type corresponds to a "primitive" CLR type (such as System.Int32) and
    /// the type code identifies which of the primitive types it corresponds to.
    /// </summary>
    public virtual PrimitiveTypeCode TypeCode {
      get { return this.typeCode; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.typeCode = value;
      }
    }
    PrimitiveTypeCode typeCode;

    #region IReference Members

    IEnumerable<ICustomAttribute> IReference.Attributes {
      get {
        if (this.Attributes == null) return Dummy.TypeReference.Attributes;
        return this.Attributes.AsReadOnly();
      }
    }

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      [ContractVerification(false)]
      get {
        if (this.Locations == null) return Dummy.TypeReference.Locations;
        return this.Locations.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a zero based single dimensional array type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
  /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
  /// </summary>
  [ContractVerification(true)]
  public sealed class VectorTypeReference : ArrayTypeReference, ICopyFrom<IArrayTypeReference> {

    /// <summary>
    /// A reference to a zero based single dimensional array type. Type references can be initialized incrementally, but once a reference is frozen, or resolved
    /// or once the InternedKey of a reference has been computed, no further initialization is permitted.
    /// </summary>
    public VectorTypeReference() {
      Contract.Ensures(!this.IsFrozen);

    }

    /// <summary>
    /// Makes this reference be a shallow copy of the given type reference.
    /// </summary>
    /// <param name="vectorTypeReference">The type referenced to shallow copy onto this reference.</param>
    /// <param name="internFactory">
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.   
    /// </param>
    public void Copy(IArrayTypeReference vectorTypeReference, IInternFactory internFactory) {
      ((ICopyFrom<ITypeReference>)this).Copy(vectorTypeReference, internFactory);
      Contract.Assume(!this.IsFrozen);
      this.ElementType = vectorTypeReference.ElementType;
    }

    /// <summary>
    /// This type of array is a single dimensional array with zero lower bound for index values.
    /// </summary>
    public override bool IsVector {
      get { return true; }
    }

    /// <summary>
    /// Returns the type of the resolved array.
    /// </summary>
    /// <returns></returns>
    protected override IArrayType Resolve() {
      return Immutable.Vector.GetVector(this.ElementType, this.InternFactory);
    }

  }

}
