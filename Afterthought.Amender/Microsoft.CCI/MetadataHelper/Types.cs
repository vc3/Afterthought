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
using System.Text;
using System.Diagnostics.Contracts;

#pragma warning disable 1591

namespace Microsoft.Cci.Immutable {

  public abstract class ArrayType : SystemDefinedStructuralType, IArrayType {

    internal ArrayType(ITypeReference elementType, IInternFactory internFactory)
      : base(internFactory) {
      this.elementType = elementType;
    }

    public override IEnumerable<ITypeReference> BaseClasses {
      get { return IteratorHelper.GetSingletonEnumerable<ITypeReference>(this.PlatformType.SystemArray); }
    }

    [Pure]
    public override bool Contains(ITypeDefinitionMember member) {
      foreach (ITypeDefinitionMember mem in this.Members)
        if (mem == member) return true;
      return false;
    }

    /// <summary>
    /// Calls visitor.Visit(IArrayTypeReference)
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IArrayTypeReference)
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public ITypeReference ElementType {
      get { return this.elementType; }
    }
    readonly ITypeReference elementType;

    protected virtual IEnumerable<ITypeReference> GetInterfaceList() {
      var coreAssembly = TypeHelper.GetDefiningUnitReference(this.PlatformType.SystemArray) as IAssemblyReference;
      var version = coreAssembly == null ? new Version(4, 0) : coreAssembly.AssemblyIdentity.Version;
      List<ITypeReference> interfaces = new List<ITypeReference>(6);
      interfaces.Add(this.PlatformType.SystemICloneable);
      interfaces.Add(this.PlatformType.SystemCollectionsIEnumerable);
      interfaces.Add(this.PlatformType.SystemCollectionsICollection);
      interfaces.Add(this.PlatformType.SystemCollectionsIList);
      if (version.Major >= 2) {
        interfaces.Add(this.PlatformType.SystemCollectionsIStructuralComparable);
        interfaces.Add(this.PlatformType.SystemCollectionsIStructuralEquatable);
      }
      return interfaces.AsReadOnly();
    }

    public override IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      foreach (ITypeDefinitionMember member in this.Members) {
        if (name.UniqueKey != member.Name.UniqueKey || (ignoreCase && name.UniqueKeyIgnoringCase == member.Name.UniqueKeyIgnoringCase)) {
          if (predicate(member)) yield return member;
        }
      }
    }

    public override IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      foreach (ITypeDefinitionMember member in this.Members) {
        if (predicate(member)) yield return member;
      }
    }

    public override IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      foreach (ITypeDefinitionMember member in this.Members) {
        if (name.UniqueKey != member.Name.UniqueKey || (ignoreCase && name.UniqueKeyIgnoringCase == member.Name.UniqueKeyIgnoringCase)) {
          yield return member;
        }
      }
    }

    public override IEnumerable<ITypeReference> Interfaces {
      get {
        if (this.interfaces == null) {
          lock (GlobalLock.LockingObject) {
            if (this.interfaces == null) {
              this.interfaces = this.GetInterfaceList();
            }
          }
        }
        return this.interfaces;
      }
    }
    IEnumerable<ITypeReference>/*?*/ interfaces;

    public override bool IsReferenceType {
      get { return true; }
    }

    public virtual bool IsVector {
      get { return this.Rank == 1; }
    }

    public virtual IEnumerable<int> LowerBounds {
      get { return Enumerable<int>.Empty; }
    }

    public override IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public override IPlatformType PlatformType {
      get { return this.ElementType.PlatformType; }
    }

    public virtual uint Rank {
      get { return 1; }
    }

    public virtual IEnumerable<ulong> Sizes {
      get { return Enumerable<ulong>.Empty; }
    }

    #region ITypeDefinition Members

    IEnumerable<ITypeReference> ITypeDefinition.BaseClasses {
      get {
        return this.BaseClasses;
      }
    }

    IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    ushort ITypeDefinition.GenericParameterCount {
      get {
        //^ assume this.IsGeneric == ((ITypeDefinition)this).IsGeneric;
        return 0;
      }
    }

    #endregion

    #region IContainer<ITypeDefinitionMember> Members

    IEnumerable<ITypeDefinitionMember> IContainer<ITypeDefinitionMember>.Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    #endregion

    #region IDefinition Members

    IEnumerable<ICustomAttribute> IReference.Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    [Pure]
    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    #endregion
  }

  public class CustomModifier : ICustomModifier {

    public CustomModifier(bool isOptional, ITypeReference modifier) {
      this.isOptional = isOptional;
      this.modifier = modifier;
    }

    public bool IsOptional {
      get { return this.isOptional; }
    }
    readonly bool isOptional;

    public ITypeReference Modifier {
      get { return this.modifier; }
    }
    readonly ITypeReference modifier;

    /// <summary>
    /// Returns a deep copy of a customer modifier. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="customModifier">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="customModifier"/>.</param>
    /// <param name="internFactory">An intern factory. </param>
    internal static ICustomModifier CopyModifierToNewContainer(ICustomModifier customModifier, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      ITypeReference copiedModifier = TypeDefinition.DeepCopyTypeReference(customModifier.Modifier, targetContainer, internFactory);
      if (copiedModifier == customModifier.Modifier) return customModifier;
      return new CustomModifier(customModifier.IsOptional, copiedModifier);
    }

    internal static ICustomModifier Specialize(ICustomModifier customModifier, ITypeReference targetContainer, IInternFactory internFactory) {
      ITypeReference copiedModifier = TypeHelper.SpecializeTypeReference(customModifier.Modifier, targetContainer, internFactory);
      if (copiedModifier == customModifier.Modifier) return customModifier;
      return new CustomModifier(customModifier.IsOptional, copiedModifier);
    }

    internal static ICustomModifier Specialize(ICustomModifier customModifier, IMethodReference targetContainer, IInternFactory internFactory) {
      ITypeReference copiedModifier = TypeHelper.SpecializeTypeReference(customModifier.Modifier, targetContainer, internFactory);
      if (copiedModifier == customModifier.Modifier) return customModifier;
      return new CustomModifier(customModifier.IsOptional, copiedModifier);
    }

    /// <summary>
    /// If the given custom modifier has a modifier that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new custom modifier using a modifier type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ICustomModifier SpecializeIfConstructedFromApplicableTypeParameter(ICustomModifier customModifier, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      ITypeReference copiedModifier = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(customModifier.Modifier, containingMethodInstance, internFactory);
      if (copiedModifier == customModifier.Modifier) return customModifier;
      return new CustomModifier(customModifier.IsOptional, copiedModifier);
    }

    /// <summary>
    /// If the given custom modifier has a modifier that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new custom modifier using a modifier type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ICustomModifier SpecializeIfConstructedFromApplicableTypeParameter(ICustomModifier customModifier, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      ITypeReference copiedModifier = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(customModifier.Modifier, containingTypeInstance, internFactory);
      if (copiedModifier == customModifier.Modifier) return customModifier;
      return new CustomModifier(customModifier.IsOptional, copiedModifier);
    }

    /// <summary>
    /// If the given custom modifier has a modifier that involves a method type parameter of the partially specialized version of specializedMethodReference,
    /// then return a new custom modifier using a modifier type that is the corresponding method type parameter from specializedMethodReference.
    /// </summary>
    internal static ICustomModifier SpecializeIfConstructedFromApplicableMethodTypeParameter(ICustomModifier customModifier, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      ITypeReference copiedModifier = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(customModifier.Modifier, specializedMethodReference, internFactory);
      if (copiedModifier == customModifier.Modifier) return customModifier;
      return new CustomModifier(customModifier.IsOptional, copiedModifier);
    }
  }

  public class FunctionPointerType : SystemDefinedStructuralType, IFunctionPointer {

    public FunctionPointerType(ISignature signature, IInternFactory internFactory)
      : base(internFactory) {
      this.callingConvention = signature.CallingConvention;
      if (signature.ReturnValueIsModified)
        this.returnValueCustomModifiers = signature.ReturnValueCustomModifiers;
      this.returnValueIsByRef = signature.ReturnValueIsByRef;
      this.type = signature.Type;
      this.parameters = signature.Parameters;
      this.extraArgumentTypes = Enumerable<IParameterTypeInformation>.Empty;
    }

    public FunctionPointerType(CallingConvention callingConvention, bool returnValueIsByRef, ITypeReference type,
      IEnumerable<ICustomModifier>/*?*/ returnValueCustomModifiers, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation>/*?*/ extraArgumentTypes,
      IInternFactory internFactory)
      : base(internFactory) {
      this.callingConvention = callingConvention;
      this.returnValueCustomModifiers = returnValueCustomModifiers;
      this.returnValueIsByRef = returnValueIsByRef;
      this.type = type;
      this.parameters = parameters;
      if (extraArgumentTypes == null)
        this.extraArgumentTypes = Enumerable<IParameterTypeInformation>.Empty;
      else
        this.extraArgumentTypes = extraArgumentTypes;
    }

    public CallingConvention CallingConvention {
      get { return this.callingConvention; }
    }
    readonly CallingConvention callingConvention;

    /// <summary>
    /// Calls visitor.Visit(IFunctionPointerTypeReference)
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IFunctionPointerTypeReference)
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<IParameterTypeInformation> ExtraArgumentTypes {
      get { return this.extraArgumentTypes; }
    }
    readonly IEnumerable<IParameterTypeInformation> extraArgumentTypes;

    public override IPlatformType PlatformType {
      get { return this.Type.PlatformType; }
    }

    public IEnumerable<IParameterTypeInformation> Parameters {
      get { return this.parameters; }
    }
    readonly IEnumerable<IParameterTypeInformation> parameters;

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get {
        if (this.returnValueCustomModifiers == null) return Enumerable<ICustomModifier>.Empty; //shouldn't get here if the precondition is respected.
        return this.returnValueCustomModifiers;
      }
    }
    readonly IEnumerable<ICustomModifier>/*?*/ returnValueCustomModifiers;

    public bool ReturnValueIsByRef {
      get { return this.returnValueIsByRef; }
    }
    readonly bool returnValueIsByRef;

    public bool ReturnValueIsModified {
      get { return this.returnValueCustomModifiers != null; }
    }

    public ITypeReference Type {
      get { return this.type; }
    }
    readonly ITypeReference type;

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Pointer; }
    }

    #region ISignature Members

    bool ISignature.IsStatic {
      get { return (this.CallingConvention & Cci.CallingConvention.HasThis) == 0; }
    }

    ITypeReference ISignature.Type {
      get { return this.Type; }
    }

    #endregion

  }

  public class GenericTypeInstance : Scope<ITypeDefinitionMember>, IGenericTypeInstance {

    public static GenericTypeInstance GetGenericTypeInstance(INamedTypeDefinition genericType, IEnumerable<ITypeReference> genericArguments, IInternFactory internFactory) {
      Contract.Requires(genericType != null);
      Contract.Requires(genericArguments != null);
      Contract.Requires(internFactory != null);
      Contract.Requires(!(genericType is Dummy));
      Contract.Requires(genericType.IsGeneric);
      Contract.Ensures(Contract.Result<GenericTypeInstance>() != null);

      return new GenericTypeInstance(genericType, genericArguments, internFactory);
    }

    private GenericTypeInstance(INamedTypeDefinition genericType, IEnumerable<ITypeReference> genericArguments, IInternFactory internFactory) {
      Contract.Requires(!(genericType is Dummy));
      Contract.Requires(genericArguments != null);
      Contract.Requires(internFactory != null);
      Contract.Requires(!(genericType is Dummy));
      Contract.Requires(genericType.IsGeneric);

      this.genericType = genericType;
      this.genericArguments = genericArguments;
      this.internFactory = internFactory;
    }

    public ushort Alignment {
      get { return this.GenericType.ResolvedType.Alignment; }
    }

    public virtual IEnumerable<ICustomAttribute> Attributes {
      get { return this.GenericType.Attributes; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get {
        foreach (ITypeReference baseClassRef in this.GenericType.ResolvedType.BaseClasses) {
          ITypeReference specializedBaseClass = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(baseClassRef, this, this.InternFactory);
          yield return specializedBaseClass;
        }
      }
    }

    /// <summary>
    /// Calls this.Visit(IGenericTypeInstanceReference).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls this.Visit(IGenericTypeInstanceReference).
    /// </summary>
    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<IEventDefinition> Events {
      get {
        this.InitializeIfNecessary();
        return this.events;
      }
    }
    IEnumerable<IEventDefinition> events;

    public IEnumerable<IFieldDefinition> Fields {
      get {
        this.InitializeIfNecessary();
        return this.fields;
      }
    }
    IEnumerable<IFieldDefinition> fields;

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get {
        foreach (var methodImplementation in this.GenericType.ResolvedType.ExplicitImplementationOverrides)
          yield return new SpecializedMethodImplementation(this, methodImplementation, this.InternFactory);
      }
    }

    public IEnumerable<ITypeReference> GenericArguments {
      get { return this.genericArguments; }
    }
    readonly IEnumerable<ITypeReference> genericArguments;

    public INamedTypeReference GenericType {
      get { return this.genericType; }
    }
    readonly INamedTypeReference genericType; //^ invariant genericType.ResolvedType.IsGeneric;

    protected override void InitializeIfNecessary() {
      if (this.initialized) return;
      lock (GlobalLock.LockingObject) {
        if (this.initialized) return;
        var template = this.GenericType.ResolvedType;
        List<IEventDefinition> eventList = null;
        foreach (var unspecializedEvent in template.Events) {
          var specializedEvent = (IEventDefinition)this.SpecializeMember(unspecializedEvent, this.InternFactory);
          this.AddMemberToCache(specializedEvent);
          if (eventList == null) eventList = new List<IEventDefinition>();
          eventList.Add(specializedEvent);
        }
        if (eventList != null) {
          eventList.TrimExcess();
          this.events = eventList.AsReadOnly();
        } else {
          this.events = Enumerable<IEventDefinition>.Empty;
        }
        List<IFieldDefinition> fieldList = null;
        foreach (var unspecializedField in template.Fields) {
          var specializedField = (IFieldDefinition)this.SpecializeMember(unspecializedField, this.InternFactory);
          this.AddMemberToCache(specializedField);
          if (fieldList == null) fieldList = new List<IFieldDefinition>();
          fieldList.Add(specializedField);
        }
        if (fieldList != null) {
          fieldList.TrimExcess();
          this.fields = fieldList.AsReadOnly();
        } else {
          this.fields = Enumerable<IFieldDefinition>.Empty;
        }
        List<IMethodDefinition> methodList = null;
        foreach (var unspecializedMethod in template.Methods) {
          var specializedMethod = (IMethodDefinition)this.SpecializeMember(unspecializedMethod, this.InternFactory);
          this.AddMemberToCache(specializedMethod);
          if (methodList == null) methodList = new List<IMethodDefinition>();
          methodList.Add(specializedMethod);
        }
        if (methodList != null) {
          methodList.TrimExcess();
          this.methods = methodList.AsReadOnly();
        } else {
          this.methods = Enumerable<IMethodDefinition>.Empty;
        }
        List<INestedTypeDefinition> nestedTypeList = null;
        foreach (var unspecializedNestedType in template.NestedTypes) {
          var specializedNestedType = (INestedTypeDefinition)this.SpecializeMember(unspecializedNestedType, this.InternFactory);
          this.AddMemberToCache(specializedNestedType);
          if (nestedTypeList == null) nestedTypeList = new List<INestedTypeDefinition>();
          nestedTypeList.Add(specializedNestedType);
        }
        if (nestedTypeList != null) {
          nestedTypeList.TrimExcess();
          this.nestedTypes = nestedTypeList.AsReadOnly();
        } else {
          this.nestedTypes = Enumerable<INestedTypeDefinition>.Empty;
        }
        List<IPropertyDefinition> propertyList = null;
        foreach (var unspecializedProperty in template.Properties) {
          var specializedProperty = (IPropertyDefinition)this.SpecializeMember(unspecializedProperty, this.InternFactory);
          this.AddMemberToCache(specializedProperty);
          if (propertyList == null) propertyList = new List<IPropertyDefinition>();
          propertyList.Add(specializedProperty);
        }
        if (propertyList != null) {
          propertyList.TrimExcess();
          this.properties = propertyList.AsReadOnly();
        } else {
          this.properties = Enumerable<IPropertyDefinition>.Empty;
        }
        this.initialized = true;
      }
    }
    private bool initialized;

    public IGenericTypeInstanceReference InstanceType {
      get { return this; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get {
        foreach (ITypeReference ifaceRef in this.GenericType.ResolvedType.Interfaces) {
          ITypeReference specializedIface = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(ifaceRef, this, this.InternFactory);
          yield return specializedIface;
        }
      }
    }

    public bool IsAbstract {
      get { return this.GenericType.ResolvedType.IsAbstract; }
    }

    public bool IsClass {
      get { return this.GenericType.ResolvedType.IsClass; }
    }

    public bool IsDelegate {
      get { return this.GenericType.ResolvedType.IsDelegate; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get
        //^ ensures result == false;
      {
        return false;
      }
    }

    public bool IsInterface {
      get { return this.GenericType.ResolvedType.IsInterface; }
    }

    public bool IsReferenceType {
      get { return this.GenericType.ResolvedType.IsReferenceType; }
    }

    public bool IsSealed {
      get { return this.GenericType.ResolvedType.IsSealed; }
    }

    public bool IsStatic {
      get { return this.GenericType.ResolvedType.IsStatic; }
    }

    public bool IsValueType {
      get { return this.GenericType.IsValueType; }
    }

    public bool IsStruct {
      get { return this.GenericType.ResolvedType.IsStruct; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get {
        this.InitializeIfNecessary();
        return this.methods;
      }
    }
    IEnumerable<IMethodDefinition> methods;

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get {
        this.InitializeIfNecessary();
        return this.nestedTypes;
      }
    }
    IEnumerable<INestedTypeDefinition> nestedTypes;

    public IPlatformType PlatformType {
      get { return this.GenericType.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get {
        //TODO: specialize and cache the private helper members of the generic type template.
        return Enumerable<ITypeDefinitionMember>.Empty;
      }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get {
        this.InitializeIfNecessary();
        return this.properties;
      }
    }
    IEnumerable<IPropertyDefinition> properties;

    /// <summary>
    /// Returns a deep copy of a generic type instance reference. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="genericTypeInstance">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="genericTypeInstance"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference DeepCopyTypeReference(IGenericTypeInstanceReference genericTypeInstance, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      var copiedGenericType = (INamedTypeReference)TypeDefinition.DeepCopyTypeReference(genericTypeInstance.GenericType, targetContainer, internFactory);
      List<ITypeReference>/*?*/ copiedArguments = null;
      int i = 0;
      foreach (ITypeReference argType in genericTypeInstance.GenericArguments) {
        ITypeReference copiedArgType = TypeDefinition.DeepCopyTypeReference(argType, targetContainer, internFactory);
        if (argType != copiedArgType) {
          if (copiedArguments == null) copiedArguments = new List<ITypeReference>(genericTypeInstance.GenericArguments);
          //^ assume 0 <= i && i < specializedArguments.Count;  //Since genericTypeInstance.GenericArguments is immutable
          copiedArguments[i] = copiedArgType;
        }
        i++;
      }
      if (copiedArguments == null) {
        if (copiedGenericType == genericTypeInstance.GenericType) return genericTypeInstance;
        return new GenericTypeInstanceReference(copiedGenericType, genericTypeInstance.GenericArguments, internFactory);
      }
      return new GenericTypeInstanceReference(copiedGenericType, copiedArguments, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IGenericTypeInstanceReference genericTypeInstance, ITypeReference targetContainer, IInternFactory internFactory) {
      var copiedGenericType = (INamedTypeReference)TypeHelper.SpecializeTypeReference(genericTypeInstance.GenericType, targetContainer, internFactory);
      List<ITypeReference>/*?*/ copiedArguments = null;
      int i = 0;
      foreach (ITypeReference argType in genericTypeInstance.GenericArguments) {
        ITypeReference copiedArgType = TypeHelper.SpecializeTypeReference(argType, targetContainer, internFactory);
        if (argType != copiedArgType) {
          if (copiedArguments == null) copiedArguments = new List<ITypeReference>(genericTypeInstance.GenericArguments);
          //^ assume 0 <= i && i < specializedArguments.Count;  //Since genericTypeInstance.GenericArguments is immutable
          copiedArguments[i] = copiedArgType;
        }
        i++;
      }
      if (copiedArguments == null) {
        if (copiedGenericType == genericTypeInstance.GenericType) return genericTypeInstance;
        return new GenericTypeInstanceReference(copiedGenericType, genericTypeInstance.GenericArguments, internFactory);
      }
      return new GenericTypeInstanceReference(copiedGenericType, copiedArguments, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IGenericTypeInstanceReference genericTypeInstance, IMethodReference targetContainer, IInternFactory internFactory) {
      var copiedGenericType = (INamedTypeReference)TypeHelper.SpecializeTypeReference(genericTypeInstance.GenericType, targetContainer, internFactory);
      List<ITypeReference>/*?*/ copiedArguments = null;
      int i = 0;
      foreach (ITypeReference argType in genericTypeInstance.GenericArguments) {
        ITypeReference copiedArgType = TypeHelper.SpecializeTypeReference(argType, targetContainer, internFactory);
        if (argType != copiedArgType) {
          if (copiedArguments == null) copiedArguments = new List<ITypeReference>(genericTypeInstance.GenericArguments);
          //^ assume 0 <= i && i < specializedArguments.Count;  //Since genericTypeInstance.GenericArguments is immutable
          copiedArguments[i] = copiedArgType;
        }
        i++;
      }
      if (copiedArguments == null) {
        if (copiedGenericType == genericTypeInstance.GenericType) return genericTypeInstance;
        return new GenericTypeInstanceReference(copiedGenericType, genericTypeInstance.GenericArguments, internFactory);
      }
      return new GenericTypeInstanceReference(copiedGenericType, copiedArguments, internFactory);
    }

    /// <summary>
    /// Specialize component type references of genericTypeInstance and (if necessary) return a new instance of the 
    /// specialized version of genericTypeInstance.GenericType using the specialized type arguments. Specialization here
    /// means replacing any references to the generic type parameters of containingMethodInstance.GenericMethod with the
    /// corresponding values of containingMethodInstance.GenericArguments.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IGenericTypeInstanceReference genericTypeInstance, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      var specializedGenericType = (INamedTypeReference)TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(genericTypeInstance.GenericType, containingMethodInstance, internFactory);
      List<ITypeReference>/*?*/ specializedArguments = null;
      int i = 0;
      foreach (ITypeReference argType in genericTypeInstance.GenericArguments) {
        ITypeReference specializedArgType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(argType, containingMethodInstance, internFactory);
        if (argType != specializedArgType) {
          if (specializedArguments == null) specializedArguments = new List<ITypeReference>(genericTypeInstance.GenericArguments);
          //^ assume 0 <= i && i < specializedArguments.Count; //Since genericTypeInstance.GenericArguments is immutable
          specializedArguments[i] = specializedArgType;
        }
        i++;
      }
      if (specializedArguments == null) {
        if (specializedGenericType == genericTypeInstance.GenericType) return genericTypeInstance;
        else return new GenericTypeInstanceReference(specializedGenericType, genericTypeInstance.GenericArguments, internFactory);
      }
      return new GenericTypeInstanceReference(specializedGenericType, specializedArguments, internFactory);
    }

    /// <summary>
    /// Specialize the type arguments of genericTypeIntance and (if necessary) return a new instance of containingTypeInstance.GenericType using
    /// the specialized type arguments. Specialization means replacing any references to the type parameters of containingTypeInstance.GenericType with the
    /// corresponding values of containingTypeInstance.GenericArguments.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IGenericTypeInstanceReference genericTypeInstance, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      var specializedGenericType = (INamedTypeReference)TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(genericTypeInstance.GenericType, containingTypeInstance, internFactory);
      List<ITypeReference>/*?*/ specializedArguments = null;
      int i = 0;
      foreach (ITypeReference argType in genericTypeInstance.GenericArguments) {
        ITypeReference specializedArgType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(argType, containingTypeInstance, internFactory);
        if (argType != specializedArgType) {
          if (specializedArguments == null) specializedArguments = new List<ITypeReference>(genericTypeInstance.GenericArguments);
          //^ assume 0 <= i && i < specializedArguments.Count;  //Since genericTypeInstance.GenericArguments is immutable
          specializedArguments[i] = specializedArgType;
        }
        i++;
      }
      if (specializedArguments == null) {
        if (specializedGenericType == genericTypeInstance.GenericType) return genericTypeInstance;
        else return new GenericTypeInstanceReference(specializedGenericType, genericTypeInstance.GenericArguments, internFactory);
      }
      return new GenericTypeInstanceReference(specializedGenericType, specializedArguments, internFactory);
    }

    /// <summary>
    /// Specialize the type arguments of genericTypeIntance and (if necessary) return a new instance of containingTypeInstance.GenericType using
    /// the specialized type arguments. Specialization means replacing any references to the method type parameters of 
    /// the partially specialized version of specializedMethodReference with the corresponding references to method type parameters of specializedMethodReference.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IGenericTypeInstanceReference genericTypeInstance, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      var specializedGenericType = (INamedTypeReference)TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(genericTypeInstance.GenericType, specializedMethodReference, internFactory);
      List<ITypeReference>/*?*/ specializedArguments = null;
      int i = 0;
      foreach (ITypeReference argType in genericTypeInstance.GenericArguments) {
        ITypeReference specializedArgType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(argType, specializedMethodReference, internFactory);
        if (argType != specializedArgType) {
          if (specializedArguments == null) specializedArguments = new List<ITypeReference>(genericTypeInstance.GenericArguments);
          //^ assume 0 <= i && i < specializedArguments.Count;  //Since genericTypeInstance.GenericArguments is immutable
          specializedArguments[i] = specializedArgType;
        }
        i++;
      }
      if (specializedArguments == null) {
        if (specializedGenericType == genericTypeInstance.GenericType) return genericTypeInstance;
        else return new GenericTypeInstanceReference(specializedGenericType, genericTypeInstance.GenericArguments, internFactory);
      }
      return new GenericTypeInstanceReference(specializedGenericType, specializedArguments, internFactory);
    }

    public ITypeDefinitionMember SpecializeMember(ITypeDefinitionMember unspecializedMember, IInternFactory internFactory)
      //^ requires unspecializedMember is IEventDefinition || unspecializedMember is IFieldDefinition || unspecializedMember is IMethodDefinition ||
      //^   unspecializedMember is IPropertyDefinition || unspecializedMember is INestedTypeDefinition;
      //^ ensures unspecializedMember is IEventDefinition ==> result is IEventDefinition;
      //^ ensures unspecializedMember is IFieldDefinition ==> result is IFieldDefinition;
      //^ ensures unspecializedMember is IMethodDefinition ==> result is IMethodDefinition;
      //^ ensures unspecializedMember is IPropertyDefinition ==> result is IPropertyDefinition;
      //^ ensures unspecializedMember is INestedTypeDefinition ==> result is INestedTypeDefinition;
    {
      IEventDefinition/*?*/ eventDef = unspecializedMember as IEventDefinition;
      if (eventDef != null) {
        var unspecializedEventDef = eventDef;
        var specializedEventDef = eventDef as ISpecializedEventDefinition;
        if (specializedEventDef != null) unspecializedEventDef = specializedEventDef.UnspecializedVersion;
        return new SpecializedEventDefinition(unspecializedEventDef, eventDef, this, this);
      }
      IFieldDefinition/*?*/ fieldDef = unspecializedMember as IFieldDefinition;
      if (fieldDef != null) {
        var unspecializedFieldDef = fieldDef;
        var specializedFieldDef = fieldDef as ISpecializedFieldDefinition;
        if (specializedFieldDef != null) unspecializedFieldDef = specializedFieldDef.UnspecializedVersion;
        return new SpecializedFieldDefinition(unspecializedFieldDef, fieldDef, this, this);
      }
      IMethodDefinition/*?*/ methodDef = unspecializedMember as IMethodDefinition;
      if (methodDef != null) {
        var unspecializedMethodDef = methodDef;
        var specializedMethodDef = methodDef as ISpecializedMethodDefinition;
        if (specializedMethodDef != null) unspecializedMethodDef = specializedMethodDef.UnspecializedVersion;
        return new SpecializedMethodDefinition(unspecializedMethodDef, methodDef, this, this);
      }
      IPropertyDefinition/*?*/ propertyDef = unspecializedMember as IPropertyDefinition;
      if (propertyDef != null) {
        var unspecializedPropertyDef = propertyDef;
        var specializedPropertyDef = propertyDef as ISpecializedPropertyDefinition;
        if (specializedPropertyDef != null) unspecializedPropertyDef = specializedPropertyDef.UnspecializedVersion;
        return new SpecializedPropertyDefinition(unspecializedPropertyDef, propertyDef, this, this);
      }
      //^ assert unspecializedMember is INestedTypeDefinition;
      INestedTypeDefinition nestedTypeDef = (INestedTypeDefinition)unspecializedMember;
      var unspecializedTypeDef = nestedTypeDef;
      var specializedTypeDef = nestedTypeDef as ISpecializedNestedTypeDefinition;
      if (specializedTypeDef != null) unspecializedTypeDef = specializedTypeDef.UnspecializedVersion;
      return new SpecializedNestedTypeDefinition(unspecializedTypeDef, nestedTypeDef, this, this, internFactory);
    }

    public uint SizeOf {
      get { return this.GenericType.ResolvedType.SizeOf; }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    public ITypeReference UnderlyingType {
      get { return this; }
    }

    public LayoutKind Layout {
      get
        //^ ensures result == this.GenericType.ResolvedType.Layout;
      {
        return this.GenericType.ResolvedType.Layout;
      }
    }

    public bool IsSpecialName {
      get { return this.GenericType.ResolvedType.IsSpecialName; }
    }

    public bool IsComObject {
      get { return this.GenericType.ResolvedType.IsComObject; }
    }

    public bool IsSerializable {
      get { return this.GenericType.ResolvedType.IsSerializable; }
    }

    public bool IsBeforeFieldInit {
      get { return this.GenericType.ResolvedType.IsBeforeFieldInit; }
    }

    public StringFormatKind StringFormat {
      get { return this.GenericType.ResolvedType.StringFormat; }
    }

    public bool IsRuntimeSpecial {
      get { return this.GenericType.ResolvedType.IsRuntimeSpecial; }
    }

    public bool HasDeclarativeSecurity {
      get { return this.GenericType.ResolvedType.HasDeclarativeSecurity; }
    }

    #region ITypeDefinition Members

    IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    ushort ITypeDefinition.GenericParameterCount {
      get {
        return 0;
      }
    }

    IEnumerable<ITypeDefinitionMember> ITypeDefinition.Members {
      get {
        return this.Members;
      }
    }

    IEnumerable<ISecurityAttribute> ITypeDefinition.SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    #endregion

    #region IContainer<ITypeDefinitionMember>

    IEnumerable<ITypeDefinitionMember> IContainer<ITypeDefinitionMember>.Members {
      get {
        return this.Members;
      }
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

    /// <summary>
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    public IInternFactory InternFactory {
      get { return this.internFactory; }
    }
    readonly IInternFactory internFactory;

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    #endregion
  }

  public class GenericTypeInstanceReference : IGenericTypeInstanceReference {

    public GenericTypeInstanceReference(INamedTypeReference genericType, IEnumerable<ITypeReference> genericArguments, IInternFactory internFactory) {
      Contract.Requires(genericType != null);
      Contract.Requires(genericArguments != null);
      Contract.Requires(internFactory != null);

      this.genericType = genericType;
      this.genericArguments = genericArguments;
      this.internFactory = internFactory;
    }

    #region IGenericTypeInstanceReference Members

    public IEnumerable<ITypeReference> GenericArguments {
      get { return this.genericArguments; }
    }
    readonly IEnumerable<ITypeReference> genericArguments;

    public INamedTypeReference GenericType {
      get { return this.genericType; }
    }
    readonly INamedTypeReference genericType;

    #endregion

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
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

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public bool IsAlias {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsValueType {
      get { return this.GenericType.IsValueType; }
    }

    public IPlatformType PlatformType {
      get { return this.GenericType.PlatformType; }
    }

    public ITypeDefinition ResolvedType {
      get {
        if (this.resolvedType == null) {
          var template = this.GenericType.ResolvedType;
          if (template is Dummy || !template.IsGeneric)
            this.resolvedType = Dummy.TypeDefinition;
          else
            this.resolvedType = GenericTypeInstance.GetGenericTypeInstance(template, this.GenericArguments, this.InternFactory);
        }
        return this.resolvedType;
      }
    }
    ITypeDefinition/*?*/ resolvedType;

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #endregion

    #region IReference Members

    public virtual IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region IObjectWithLocations Members

    public virtual IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion
  }

  internal static class GenericParameter {

    /// <summary>
    /// If the genericTypeParameter is a type parameter of the targetContainer, or a type parameter of a containing, generic, specialized
    /// nested type of the targetContainer, return the specialized version of the type parameter. 
    /// </summary>
    /// <remarks>: 
    /// Example of how a type parameter is from the containing type of the targetContainer:
    /// class Outer[A] {
    ///   class Mid[T] {
    ///     class Inner {
    ///       T f;
    ///     }
    ///   }
    /// }
    /// Consider Outer[char].Mid[int].Inner.f. It is a specialized field, whose ContainingGenericTypeInstance = Outer[char].Mid[int]
    /// and whose partiallySpecializedVersion is another specialized field, which we call SF1.
    /// 
    /// SF1's ContainingGenericTypeInstance is Outer[char]; its ContainingTypeDefinition is Outer[char].Mid.Inner. Its type should be 
    /// a (specialized) copy of T defined by Outer[char].Mid, which is a specialized nested type definition. Note that the targetContainer
    /// for SF1 is Outer[char].Mid.Inner. To look for specialized version of T, we need to go to the parent of the targetContainer.
    /// </remarks>
    /// <param name="genericTypeParameter">A reference to a generic type parameter that occurs inside orginal container.</param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parent's (specialized) type parameters
    /// are used to replace <paramref name="genericTypeParameter"/>. </param>
    public static ITypeReference DeepCopyTypeReference(IGenericTypeParameterReference genericTypeParameter, SpecializedNestedTypeDefinition targetContainer) {
      var nestedTypeDefinition = targetContainer;
      while (nestedTypeDefinition != null) {
        if (genericTypeParameter.DefiningType.InternedKey == nestedTypeDefinition.partiallySpecializedVersion.InternedKey) {
          int i = 0;
          var genericParameters = nestedTypeDefinition.GenericParameters.GetEnumerator();
          while (genericParameters.MoveNext()) {
            if (i++ == genericTypeParameter.Index) {
              return genericParameters.Current;
            }
          }
        }
        nestedTypeDefinition = nestedTypeDefinition.ContainingTypeDefinition as SpecializedNestedTypeDefinition;
      }
      return genericTypeParameter;
    }

    internal static ITypeReference SpecializeTypeReference(IGenericTypeParameterReference genericTypeParameter, ITypeReference targetContainer) {
      var index = genericTypeParameter.Index;
      while (true) {
        var specializedNestedType = targetContainer as ISpecializedNestedTypeReference;
        if (specializedNestedType != null) { targetContainer = specializedNestedType.ContainingType; continue; }
        var genericTypeInstance = targetContainer as IGenericTypeInstanceReference;
        if (genericTypeInstance == null) return genericTypeParameter;
        var genericType = genericTypeInstance.GenericType;
        specializedNestedType = genericType as ISpecializedNestedTypeReference;
        if (specializedNestedType != null) genericType = specializedNestedType.UnspecializedVersion;
        if (!TypeHelper.TypesAreEquivalent(genericType, genericTypeParameter.DefiningType)) {
          targetContainer = genericTypeInstance.GenericType; continue;
        }
        var i = 0;
        foreach (var typeRef in genericTypeInstance.GenericArguments) if (index == i++) return typeRef;
        Contract.Assume(false); //The type parameter claims it is defined by genericTypeInstance.GenericType, so we should never get here legitimately.
        return Dummy.TypeReference;
      }
    }

    internal static ITypeReference SpecializeTypeReference(IGenericTypeParameterReference genericTypeParameter, IMethodReference targetContainer) {
      return SpecializeTypeReference(genericTypeParameter, targetContainer.ContainingType);
    }

    internal static ITypeReference SpecializeTypeReference(IGenericMethodParameterReference genericMethodParameter, IMethodReference targetContainer, IInternFactory internFactory) {
      var genMethInstance = targetContainer as IGenericMethodInstanceReference;
      if (genMethInstance != null) {
        var genMethInstanceArg = SpecializeIfConstructedFromApplicableTypeParameter(genericMethodParameter, genMethInstance);
        if (genMethInstanceArg != genericMethodParameter) return genMethInstanceArg;
      }
      return new SpecializedGenericMethodParameterReference(targetContainer, genericMethodParameter, internFactory);
    }

    /// <summary>
    /// If the given generic parameter is a generic parameter of the generic method of which the given method is an instance, then return the corresponding type argument that
    /// was used to create the method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IGenericMethodParameterReference genericMethodParameter, IGenericMethodInstanceReference containingMethodInstance) {
      var methodReference = containingMethodInstance.GenericMethod;
      var specializedMethod = methodReference as ISpecializedMethodReference;
      if (genericMethodParameter.DefiningMethod.InternedKey == methodReference.InternedKey || (specializedMethod != null && genericMethodParameter.DefiningMethod.InternedKey == specializedMethod.UnspecializedVersion.InternedKey)) {
        ushort i = 0;
        ushort n = genericMethodParameter.Index;
        IEnumerator<ITypeReference> genericArguments = containingMethodInstance.GenericArguments.GetEnumerator();
        while (genericArguments.MoveNext()) {
          if (i++ == n) return genericArguments.Current;
        }
      }
      return genericMethodParameter;
    }

    /// <summary>
    /// If the given generic parameter is a generic parameter of the generic type of which the given type is an instance, then return the corresponding type argument that
    /// was used to create the type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IGenericTypeParameterReference genericTypeParameter, IGenericTypeInstanceReference containingTypeInstance) {
      if (genericTypeParameter.DefiningType.InternedKey == containingTypeInstance.GenericType.InternedKey) {
        ushort i = 0;
        ushort n = genericTypeParameter.Index;
        IEnumerator<ITypeReference> genericArguments = containingTypeInstance.GenericArguments.GetEnumerator();
        while (genericArguments.MoveNext()) {
          if (i++ == n) return genericArguments.Current;
        }
      }
      return genericTypeParameter;
    }

    /// <summary>
    /// If the given genericMethodParameter is a generic method parameter of the unspecialized version of specializedMethodDefinition,
    /// then return the corresponding generic method parameter of specializedMethodDefinition.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IGenericMethodParameterReference genericMethodParameter, ISpecializedMethodReference specializedMethodReference) {
      var specializedMethodDefinition = specializedMethodReference as SpecializedMethodDefinition;
      if (specializedMethodDefinition != null && genericMethodParameter.DefiningMethod.InternedKey == specializedMethodDefinition.PartiallySpecializedVersion.InternedKey) {
        ushort i = 0;
        ushort n = genericMethodParameter.Index;
        IEnumerator<IGenericMethodParameter> genericParameters = specializedMethodDefinition.GenericParameters.GetEnumerator();
        while (genericParameters.MoveNext()) {
          if (i++ == n) return genericParameters.Current;
        }
      }
      return genericMethodParameter;
    }

    /// <summary>
    /// If the given genericTypeParameter is a generic type parameter
    /// of a containing specialized nested type definition of the specializedMethodReference, then return the specialized version 
    /// of the type parameter. 
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IGenericTypeParameterReference genericTypeParameter, ISpecializedMethodReference specializedMethodReference) {
      var specializedNestedType = specializedMethodReference.ContainingType as SpecializedNestedTypeDefinition;
      if (specializedNestedType != null) return DeepCopyTypeReference(genericTypeParameter, specializedNestedType);
      return genericTypeParameter;
    }

  }

  public class ManagedPointerType : SystemDefinedStructuralType, IManagedPointerType {

    protected ManagedPointerType(ITypeReference targetType, IInternFactory internFactory)
      : base(internFactory) {
      this.targetType = targetType;
    }

    /// <summary>
    /// Calls visitor.Visit(IManagedPointerTypeReference)
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IManagedPointerTypeReference)
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public static ManagedPointerType GetManagedPointerType(ITypeReference targetType, IInternFactory internFactory) {
      Contract.Ensures(Contract.Result<ManagedPointerType>() != null);
      ManagedPointerType result = new ManagedPointerType(targetType, internFactory);
      return result;
    }

    public override IPlatformType PlatformType {
      get { return this.TargetType.PlatformType; }
    }

    /// <summary>
    /// Returns a deep copy of a managed pointer type reference. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="pointer">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="pointer"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference DeepCopyTypeReference(IManagedPointerTypeReference pointer, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.DeepCopyTypeReference(targetType, targetContainer, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetManagedPointerType(specializedtargetType, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IManagedPointerTypeReference pointer, ITypeReference targetContainer, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeHelper.SpecializeTypeReference(targetType, targetContainer, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetManagedPointerType(specializedtargetType, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IManagedPointerTypeReference pointer, IMethodReference targetContainer, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeHelper.SpecializeTypeReference(targetType, targetContainer, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetManagedPointerType(specializedtargetType, internFactory);
    }

    /// <summary>
    /// If the given managed pointer has a target type that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IManagedPointerTypeReference pointer, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(targetType, containingMethodInstance, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetManagedPointerType(specializedtargetType, internFactory);
    }

    /// <summary>
    /// If the given managed pointer has a target type that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IManagedPointerTypeReference pointer, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(targetType, containingTypeInstance, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetManagedPointerType(specializedtargetType, internFactory);
    }

    /// <summary>
    /// If the given managed pointer has a target type that involves a method type parameter of the partially specialized version of specializedMethodReference,
    /// then return a new pointer using a target type that is the corresponding method type parameter from specializedMethodReference.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IManagedPointerTypeReference pointer, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(targetType, specializedMethodReference, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetManagedPointerType(specializedtargetType, internFactory);
    }

    public ITypeReference TargetType {
      get { return this.targetType; }
    }
    readonly ITypeReference targetType;

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Reference; }
    }

  }

  public class Matrix : ArrayType {

    protected Matrix(ITypeReference elementType, uint rank, IEnumerable<int>/*?*/ lowerBounds, IEnumerable<ulong>/*?*/ sizes, IInternFactory internFactory)
      : base(elementType, internFactory) {
      this.rank = rank;
      this.lowerBounds = lowerBounds;
      this.sizes = sizes;
    }

    public static Matrix GetMatrix(ITypeReference elementType, uint rank, IInternFactory internFactory) {
      Contract.Ensures(Contract.Result<Matrix>() != null);
      return new Matrix(elementType, rank, null, null, internFactory);
    }

    public static Matrix GetMatrix(ITypeReference elementType, uint rank, IEnumerable<int>/*?*/ lowerBounds, IEnumerable<ulong>/*?*/ sizes, IInternFactory internFactory) {
      Contract.Ensures(Contract.Result<Matrix>() != null);
      return new Matrix(elementType, rank, lowerBounds, sizes, internFactory);
    }

    public override bool IsVector {
      get { return false; }
    }

    public override IEnumerable<int> LowerBounds {
      get {
        if (this.lowerBounds == null) return base.LowerBounds;
        return this.lowerBounds;
      }
    }
    IEnumerable<int>/*?*/ lowerBounds;

    public override uint Rank {
      get { return this.rank; }
    }
    readonly uint rank;

    public override IEnumerable<ulong> Sizes {
      get {
        if (this.sizes == null) return base.Sizes;
        return this.sizes;
      }
    }
    IEnumerable<ulong>/*?*/ sizes;

    /// <summary>
    /// Returns a deep copy of an array type (a vector). In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="array">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="array"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference DeepCopyTypeReference(IArrayTypeReference array, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory)
      //^ requires !array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.DeepCopyTypeReference(elementType, targetContainer, internFactory);
      if (elementType == specializedElementType) return array;
      return GetMatrix(specializedElementType, array.Rank, array.LowerBounds, array.Sizes, internFactory);
    }

    /// <summary>
    /// Returns a deep copy of an array type (a vector). In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="array">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="array"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference SpecializeTypeReference(IArrayTypeReference array, ITypeReference targetContainer, IInternFactory internFactory)
      //^ requires !array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeHelper.SpecializeTypeReference(elementType, targetContainer, internFactory);
      if (elementType == specializedElementType) return array;
      return GetMatrix(specializedElementType, array.Rank, array.LowerBounds, array.Sizes, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IArrayTypeReference array, IMethodReference targetContainer, IInternFactory internFactory)
      //^ requires !array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeHelper.SpecializeTypeReference(elementType, targetContainer, internFactory);
      if (elementType == specializedElementType) return array;
      return GetMatrix(specializedElementType, array.Rank, array.LowerBounds, array.Sizes, internFactory);
    }

    /// <summary>
    /// If the given matrix has an element type that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new matrix using an element type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IArrayTypeReference array, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory)
      //^ requires !array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(elementType, containingMethodInstance, internFactory);
      if (elementType == specializedElementType) return array;
      return GetMatrix(specializedElementType, array.Rank, array.LowerBounds, array.Sizes, internFactory);
    }

    /// <summary>
    /// If the given matrix has an element type that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new matrix using an element type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IArrayTypeReference array, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory)
      //^ requires !array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(elementType, containingTypeInstance, internFactory);
      if (elementType == specializedElementType) return array;
      return GetMatrix(specializedElementType, array.Rank, array.LowerBounds, array.Sizes, internFactory);
    }

    /// <summary>
    /// If the given matrix has an element type that involves a method type parameter from the partially specialized version of specializedMethodReference,
    /// then return a new matrix using an element type that has been specialized with the corresponding method type parameter from specializedMethodReference.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IArrayTypeReference array, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory)
      //^ requires !array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(elementType, specializedMethodReference, internFactory);
      if (elementType == specializedElementType) return array;
      return GetMatrix(specializedElementType, array.Rank, array.LowerBounds, array.Sizes, internFactory);
    }
  }

  public class PointerType : SystemDefinedStructuralType, IPointerType {

    protected PointerType(ITypeReference targetType, IInternFactory internFactory)
      : base(internFactory) {
      this.targetType = targetType;
    }

    /// <summary>
    /// Calls visitor.Visit(IPointerTypeReference)
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IPointerTypeReference)
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public static PointerType GetPointerType(ITypeReference targetType, IInternFactory internFactory) {
      Contract.Ensures(Contract.Result<PointerType>() != null);
      return new PointerType(targetType, internFactory);
    }

    public override IPlatformType PlatformType {
      get { return this.TargetType.PlatformType; }
    }

    /// <summary>
    /// Returns a deep copy of a pointer type reference. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="pointer">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="pointer"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference DeepCopyTypeReference(IPointerTypeReference pointer, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.DeepCopyTypeReference(targetType, targetContainer, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetPointerType(specializedtargetType, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IPointerTypeReference pointer, ITypeReference targetContainer, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeHelper.SpecializeTypeReference(targetType, targetContainer, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetPointerType(specializedtargetType, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IPointerTypeReference pointer, IMethodReference targetContainer, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeHelper.SpecializeTypeReference(targetType, targetContainer, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetPointerType(specializedtargetType, internFactory);
    }

    /// <summary>
    /// If the given pointer has a target type that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IPointerTypeReference pointer, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(targetType, containingMethodInstance, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetPointerType(specializedtargetType, internFactory);
    }

    /// <summary>
    /// If the given pointer has a target type that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IPointerTypeReference pointer, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(targetType, containingTypeInstance, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetPointerType(specializedtargetType, internFactory);
    }

    /// <summary>
    /// If the given pointer has a target type that involves a method type parameter of the partially specialized version of specializedMethodReference,
    /// then return a new pointer using a target type that is the corresponding method type parameter from specializedMethodReference.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceReplacingGenericMethodParameter(IPointerTypeReference pointer, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      ITypeReference targetType = pointer.TargetType;
      ITypeReference specializedtargetType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(targetType, specializedMethodReference, internFactory);
      if (targetType == specializedtargetType) return pointer;
      return GetPointerType(specializedtargetType, internFactory);
    }

    public ITypeReference TargetType {
      get { return this.targetType; }
    }
    readonly ITypeReference targetType;

    public override PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Pointer; }
    }

  }

  public class ModifiedPointerType : PointerType, IModifiedTypeReference {

    private ModifiedPointerType(ITypeReference targetType, IEnumerable<ICustomModifier> customModifiers, IInternFactory internFactory)
      : base(targetType, internFactory) {
      this.customModifiers = customModifiers;
    }

    public override bool IsModified {
      get { return true; }
    }

    /// <summary>
    /// Returns a deep copy of a modified pointer type. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="modifiedPointer">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="modifiedPointer"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference DeepCopyTypeReference(ModifiedPointerType modifiedPointer, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      var copiedTargetType = TypeDefinition.DeepCopyTypeReference(modifiedPointer.TargetType, targetContainer, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedPointer.CustomModifiers) {
        var copiedModifier = CustomModifier.CopyModifierToNewContainer(modifier, targetContainer, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedPointer.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedTargetType == modifiedPointer.TargetType) return modifiedPointer;
        return GetModifiedPointerType(copiedTargetType, modifiedPointer.CustomModifiers, internFactory);
      }
      return GetModifiedPointerType(copiedTargetType, copiedModifiers, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(ModifiedPointerType modifiedPointer, ITypeReference targetContainer, IInternFactory internFactory) {
      var copiedTargetType = TypeHelper.SpecializeTypeReference(modifiedPointer.TargetType, targetContainer, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedPointer.CustomModifiers) {
        var copiedModifier = CustomModifier.Specialize(modifier, targetContainer, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedPointer.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedTargetType == modifiedPointer.TargetType) return modifiedPointer;
        return GetModifiedPointerType(copiedTargetType, modifiedPointer.CustomModifiers, internFactory);
      }
      return GetModifiedPointerType(copiedTargetType, copiedModifiers, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(ModifiedPointerType modifiedPointer, IMethodReference targetContainer, IInternFactory internFactory) {
      var copiedTargetType = TypeHelper.SpecializeTypeReference(modifiedPointer.TargetType, targetContainer, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedPointer.CustomModifiers) {
        var copiedModifier = CustomModifier.Specialize(modifier, targetContainer, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedPointer.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedTargetType == modifiedPointer.TargetType) return modifiedPointer;
        return GetModifiedPointerType(copiedTargetType, modifiedPointer.CustomModifiers, internFactory);
      }
      return GetModifiedPointerType(copiedTargetType, copiedModifiers, internFactory);
    }

    /// <summary>
    /// If the given pointer has a target type that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(ModifiedPointerType modifiedPointer, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      var copiedTargetType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(modifiedPointer.TargetType, containingMethodInstance, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedPointer.CustomModifiers) {
        var copiedModifier = CustomModifier.SpecializeIfConstructedFromApplicableTypeParameter(modifier, containingMethodInstance, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedPointer.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedTargetType == modifiedPointer.TargetType) return modifiedPointer;
        return GetModifiedPointerType(copiedTargetType, modifiedPointer.CustomModifiers, internFactory);
      }
      return GetModifiedPointerType(copiedTargetType, copiedModifiers, internFactory);
    }

    /// <summary>
    /// If the given modified pointer has a target type that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(ModifiedPointerType modifiedPointer, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      var copiedTargetType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(modifiedPointer.TargetType, containingTypeInstance, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedPointer.CustomModifiers) {
        var copiedModifier = CustomModifier.SpecializeIfConstructedFromApplicableTypeParameter(modifier, containingTypeInstance, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedPointer.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedTargetType == modifiedPointer.TargetType) return modifiedPointer;
        return GetModifiedPointerType(copiedTargetType, modifiedPointer.CustomModifiers, internFactory);
      }
      return GetModifiedPointerType(copiedTargetType, copiedModifiers, internFactory);
    }

    /// <summary>
    /// If the given modified pointer has a target type that involves a method type parameter of the partially specialized version of specializedMethodReference,
    /// then return a new pointer using a target type that is the corresponding method type parameter from specializedMethodReference.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(ModifiedPointerType modifiedPointer, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      var copiedTargetType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(modifiedPointer.TargetType, specializedMethodReference, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedPointer.CustomModifiers) {
        var copiedModifier = CustomModifier.SpecializeIfConstructedFromApplicableMethodTypeParameter(modifier, specializedMethodReference, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedPointer.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedTargetType == modifiedPointer.TargetType) return modifiedPointer;
        return GetModifiedPointerType(copiedTargetType, modifiedPointer.CustomModifiers, internFactory);
      }
      return GetModifiedPointerType(copiedTargetType, copiedModifiers, internFactory);
    }

    public static ModifiedPointerType GetModifiedPointerType(ITypeReference targetType, IEnumerable<ICustomModifier> customModifiers, IInternFactory internFactory) {
      return new ModifiedPointerType(targetType, customModifiers, internFactory);
    }

    public override IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.customModifiers; }
    }
    readonly IEnumerable<ICustomModifier> customModifiers;

    public ITypeReference UnmodifiedType {
      get { return this; }
    }
  }

  public class ModifiedTypeReference : IModifiedTypeReference {

    private ModifiedTypeReference(IInternFactory internFactory, ITypeReference unmodifiedType, IEnumerable<ICustomModifier> customModifiers) {
      this.internFactory = internFactory;
      this.unmodifiedType = unmodifiedType;
      this.customModifiers = customModifiers;
    }

    IInternFactory internFactory;

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return this.customModifiers; }
    }
    IEnumerable<ICustomModifier> customModifiers;

    public ITypeReference UnmodifiedType {
      get { return this.unmodifiedType; }
    }
    readonly ITypeReference unmodifiedType;

    public static ModifiedTypeReference GetModifiedTypeReference(ITypeReference unmodifiedType, IEnumerable<ICustomModifier> customModifiers, IInternFactory internFactory) {
      return new ModifiedTypeReference(internFactory, unmodifiedType, customModifiers);
    }

    /// <summary>
    /// Returns a deep copy of a modified type reference. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="modifiedTypeReference">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="modifiedTypeReference"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    internal static ITypeReference DeepCopyTypeReference(IModifiedTypeReference modifiedTypeReference, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      ITypeReference copiedUnmodifiedType = TypeDefinition.DeepCopyTypeReference(modifiedTypeReference.UnmodifiedType, targetContainer, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedTypeReference.CustomModifiers) {
        var copiedModifier = CustomModifier.CopyModifierToNewContainer(modifier, targetContainer, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedUnmodifiedType == modifiedTypeReference.UnmodifiedType) return modifiedTypeReference;
        return GetModifiedTypeReference(copiedUnmodifiedType, modifiedTypeReference.CustomModifiers, internFactory);
      }
      return GetModifiedTypeReference(copiedUnmodifiedType, copiedModifiers, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IModifiedTypeReference modifiedTypeReference, ITypeReference targetContainer, IInternFactory internFactory) {
      ITypeReference copiedUnmodifiedType = TypeHelper.SpecializeTypeReference(modifiedTypeReference.UnmodifiedType, targetContainer, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedTypeReference.CustomModifiers) {
        var copiedModifier = CustomModifier.Specialize(modifier, targetContainer, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedUnmodifiedType == modifiedTypeReference.UnmodifiedType) return modifiedTypeReference;
        return GetModifiedTypeReference(copiedUnmodifiedType, modifiedTypeReference.CustomModifiers, internFactory);
      }
      return GetModifiedTypeReference(copiedUnmodifiedType, copiedModifiers, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IModifiedTypeReference modifiedTypeReference, IMethodReference targetContainer, IInternFactory internFactory) {
      ITypeReference copiedUnmodifiedType = TypeHelper.SpecializeTypeReference(modifiedTypeReference.UnmodifiedType, targetContainer, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedTypeReference.CustomModifiers) {
        var copiedModifier = CustomModifier.Specialize(modifier, targetContainer, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedUnmodifiedType == modifiedTypeReference.UnmodifiedType) return modifiedTypeReference;
        return GetModifiedTypeReference(copiedUnmodifiedType, modifiedTypeReference.CustomModifiers, internFactory);
      }
      return GetModifiedTypeReference(copiedUnmodifiedType, copiedModifiers, internFactory);
    }

    /// <summary>
    /// If the given pointer has a target type that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IModifiedTypeReference modifiedTypeReference, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      ITypeReference copiedUnmodifiedType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(modifiedTypeReference.UnmodifiedType, containingMethodInstance, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedTypeReference.CustomModifiers) {
        var copiedModifier = CustomModifier.SpecializeIfConstructedFromApplicableTypeParameter(modifier, containingMethodInstance, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedUnmodifiedType == modifiedTypeReference.UnmodifiedType) return modifiedTypeReference;
        return GetModifiedTypeReference(copiedUnmodifiedType, modifiedTypeReference.CustomModifiers, internFactory);
      }
      return GetModifiedTypeReference(copiedUnmodifiedType, copiedModifiers, internFactory);
    }

    /// <summary>
    /// If the given modified pointer has a target type that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new pointer using a target type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IModifiedTypeReference modifiedTypeReference, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      ITypeReference copiedUnmodifiedType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(modifiedTypeReference.UnmodifiedType, containingTypeInstance, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedTypeReference.CustomModifiers) {
        var copiedModifier = CustomModifier.SpecializeIfConstructedFromApplicableTypeParameter(modifier, containingTypeInstance, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedUnmodifiedType == modifiedTypeReference.UnmodifiedType) return modifiedTypeReference;
        return GetModifiedTypeReference(copiedUnmodifiedType, modifiedTypeReference.CustomModifiers, internFactory);
      }
      return GetModifiedTypeReference(copiedUnmodifiedType, copiedModifiers, internFactory);
    }

    /// <summary>
    /// If the given modified type reference has a target type that involves a method type parameter of the partially specialized version of specializedMethodReference,
    /// then return a new type reference using a target type that is the corresponding method type parameter from specializedMethodReference.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IModifiedTypeReference modifiedTypeReference, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      ITypeReference copiedUnmodifiedType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(modifiedTypeReference.UnmodifiedType, specializedMethodReference, internFactory);
      List<ICustomModifier>/*?*/ copiedModifiers = null;
      int i = 0;
      foreach (var modifier in modifiedTypeReference.CustomModifiers) {
        var copiedModifier = CustomModifier.SpecializeIfConstructedFromApplicableMethodTypeParameter(modifier, specializedMethodReference, internFactory);
        if (modifier != copiedModifier) {
          if (copiedModifiers == null) copiedModifiers = new List<ICustomModifier>(modifiedTypeReference.CustomModifiers);
          copiedModifiers[i] = copiedModifier;
        }
        i++;
      }
      if (copiedModifiers == null) {
        if (copiedUnmodifiedType == modifiedTypeReference.UnmodifiedType) return modifiedTypeReference;
        return GetModifiedTypeReference(copiedUnmodifiedType, modifiedTypeReference.CustomModifiers, internFactory);
      }
      return GetModifiedTypeReference(copiedUnmodifiedType, copiedModifiers, internFactory);
    }

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.internFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    public bool IsAlias {
      get { return false; }
    }

    public bool IsEnum {
      get { return this.UnmodifiedType.IsEnum; }
    }

    public bool IsValueType {
      get { return this.UnmodifiedType.IsValueType; }
    }

    public IPlatformType PlatformType {
      get { return this.UnmodifiedType.PlatformType; }
    }

    public ITypeDefinition ResolvedType {
      get { return this.UnmodifiedType.ResolvedType; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return this.UnmodifiedType.TypeCode; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    /// <summary>
    /// Calls visitor.Visit(IModifiedTypeReference).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IModifiedTypeReference).
    /// </summary>
    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion
  }

  /// <summary>
  /// A collection of named members, with routines to search and maintain the collection. The search routines have sublinear complexity, typically close to constant time.
  /// </summary>
  /// <typeparam name="MemberType">The type of the members of this scope.</typeparam>
  public abstract class Scope<MemberType> : IScope<MemberType>
    where MemberType : class, INamedEntity {

    private Dictionary<int, List<MemberType>> caseSensitiveMemberNameToMemberListMap = new Dictionary<int, List<MemberType>>();
    private Dictionary<int, List<MemberType>> caseInsensitiveMemberNameToMemberListMap = new Dictionary<int, List<MemberType>>();
    //TODO: replace BCL Dictionary with a private implementation that is thread safe and does not need a new list to be allocated for each name

    /// <summary>
    /// Adds a member to the scope. Does nothing if the member is already in the scope.
    /// </summary>
    /// <param name="member">The member to add to the scope.</param>
    protected void AddMemberToCache(MemberType/*!*/ member)
      //^ ensures this.Contains(member);
    {
      List<MemberType>/*?*/ members;
      if (this.caseInsensitiveMemberNameToMemberListMap.TryGetValue(member.Name.UniqueKeyIgnoringCase, out members)) {
        //^ assume members != null; //Follows from the way Dictionary is instantiated, but the verifier is ignorant of this.
        if (!members.Contains(member)) members.Add(member);
      } else {
        this.caseInsensitiveMemberNameToMemberListMap[member.Name.UniqueKeyIgnoringCase] = members = new List<MemberType>();
        members.Add(member);
      }
      if (this.caseSensitiveMemberNameToMemberListMap.TryGetValue(member.Name.UniqueKey, out members)) {
        //^ assume members != null; //Follows from the way Dictionary is instantiated, but the verifier is ignorant of this.
        if (!members.Contains(member)) members.Add(member);
      } else {
        this.caseSensitiveMemberNameToMemberListMap[member.Name.UniqueKey] = members = new List<MemberType>();
        members.Add(member);
      }
      //^ assume this.Contains(member);
    }

    /// <summary>
    /// Return true if the given member instance is a member of this scope.
    /// </summary>
    [Pure]
    public bool Contains(MemberType/*!*/ member)
      // ^ ensures result == exists{MemberType mem in this.Members; mem == member};
    {
      foreach (MemberType mem in this.GetMembersNamed(member.Name, false))
        if (mem == member) return true;
      return false;
    }

    /// <summary>
    /// Returns the list of members with the given name that also satisfy the given predicate.
    /// </summary>
    [Pure]
    public IEnumerable<MemberType> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<MemberType, bool> predicate) {
      foreach (MemberType member in this.GetMembersNamed(name, ignoreCase))
        if (predicate(member)) yield return member;
    }

    /// <summary>
    /// Returns the list of members that satisfy the given predicate.
    /// </summary>
    [Pure]
    public IEnumerable<MemberType> GetMatchingMembers(Function<MemberType, bool> predicate)
      // ^ ensures forall{MemberType member in result; member.Name.UniqueKey == name.UniqueKey && predicate(member) && this.Contains(member)};
      // ^ ensures forall{MemberType member in this.Members; member.Name.UniqueKey == name.UniqueKey && predicate(member) ==> 
      // ^                                                            exists{INamespaceMember mem in result; mem == member}};
    {
      foreach (MemberType member in this.Members)
        if (predicate(member)) yield return member;
    }

    /// <summary>
    /// Returns the list of members with the given name.
    /// </summary>
    /// <param name="name">The name of the members to retrieve.</param>
    /// <param name="ignoreCase">True if the case of the name must be ignored when retrieving the members.</param>
    [Pure]
    public IEnumerable<MemberType> GetMembersNamed(IName name, bool ignoreCase)
      // ^ ensures forall{MemberType member in result; member.Name.UniqueKey == name.UniqueKey && this.Contains(member)};
      // ^ ensures forall{MemberType member in this.Members; member.Name.UniqueKey == name.UniqueKey ==> 
      // ^                                                            exists{INamespaceMember mem in result; mem == member}};
    {
      this.InitializeIfNecessary();
      Dictionary<int, List<MemberType>> nameToMemberListMap = ignoreCase ? this.caseInsensitiveMemberNameToMemberListMap : this.caseSensitiveMemberNameToMemberListMap;
      int key = ignoreCase ? name.UniqueKeyIgnoringCase : name.UniqueKey;
      List<MemberType>/*?*/ members;
      if (!nameToMemberListMap.TryGetValue(key, out members)) return emptyList;
      //^ assume members != null; //Follows from the way Dictionary is instantiated, but the verifier is ignorant of this.
      return members.AsReadOnly();
    }
    private static readonly IEnumerable<MemberType> emptyList = (new List<MemberType>(0)).AsReadOnly();

    /// <summary>
    /// Provides a derived class with an opportunity to lazily initialize the scope's data structures via calls to AddMemberToCache.
    /// </summary>
    protected virtual void InitializeIfNecessary() { }

    /// <summary>
    /// The collection of member instances that are members of this scope.
    /// </summary>
    public virtual IEnumerable<MemberType> Members {
      get {
        this.InitializeIfNecessary();
        foreach (IEnumerable<MemberType> namedMemberList in this.caseSensitiveMemberNameToMemberListMap.Values)
          foreach (MemberType member in namedMemberList)
            yield return member;
      }
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="ParameterType"></typeparam>
  public abstract class SpecializedGenericParameter<ParameterType> : IGenericParameter
    where ParameterType : IGenericParameter {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="partiallySpecializedParameter"></param>
    /// <param name="internFactory"></param>
    protected SpecializedGenericParameter(ParameterType/*!*/ partiallySpecializedParameter, IInternFactory internFactory) {
      this.unspecializedParameter = partiallySpecializedParameter;
      this.internFactory = internFactory;
    }

    /// <summary>
    /// Zero or more classes from which this type is derived.
    /// For CLR types this collection is empty for interfaces and System.Object and populated with exactly one base type for all other types.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    /// <summary>
    /// A list of classes or interfaces. All type arguments matching this parameter must be derived from all of the classes and implement all of the interfaces.
    /// </summary>
    /// <value></value>
    public abstract IEnumerable<ITypeReference> Constraints { get; }

    /// <summary>
    /// Zero or more parameters that can be used as type annotations.
    /// </summary>
    /// <value></value>
    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    /// <summary>
    /// Zero or more interfaces implemented by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    /// <summary>
    /// An instance of this generic type that has been obtained by using the generic parameters as the arguments.
    /// Use this instance to look up members
    /// </summary>
    /// <value></value>
    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    /// <summary>
    /// A way to get to platform types such as System.Object.
    /// </summary>
    /// <value></value>
    public IPlatformType PlatformType {
      get { return this.PartiallySpecializedParameter.PlatformType; }
    }

    public ParameterType/*!*/ PartiallySpecializedParameter {
      get {
        return this.unspecializedParameter;
      }
    }
    readonly ParameterType/*!*/ unspecializedParameter;

    /// <summary>
    /// Zero or more implementation overrides provided by the class.
    /// </summary>
    /// <value></value>
    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    #region IGenericParameter Members

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be reference types.
    /// </summary>
    /// <value></value>
    public bool MustBeReferenceType {
      get { return this.PartiallySpecializedParameter.MustBeReferenceType; }
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be value types.
    /// </summary>
    /// <value></value>
    public bool MustBeValueType {
      get { return this.PartiallySpecializedParameter.MustBeValueType; }
    }

    /// <summary>
    /// True if all type arguments matching this parameter are constrained to be value types or concrete classes with visible default constructors.
    /// </summary>
    /// <value></value>
    public bool MustHaveDefaultConstructor {
      get { return this.PartiallySpecializedParameter.MustHaveDefaultConstructor; }
    }

    /// <summary>
    /// Indicates if the generic type or method with this type parameter is co-, contra-, or non variant with respect to this type parameter.
    /// </summary>
    /// <value></value>
    public TypeParameterVariance Variance {
      get { return this.PartiallySpecializedParameter.Variance; }
    }

    #endregion

    #region ITypeDefinition Members

    /// <summary>
    /// The byte alignment that values of the given type ought to have. Must be a power of 2. If zero, the alignment is decided at runtime.
    /// </summary>
    /// <value></value>
    public ushort Alignment {
      get { return this.PartiallySpecializedParameter.Alignment; }
    }

    /// <summary>
    /// Zero or more events defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    /// <summary>
    /// Zero or more fields defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    /// <value></value>
    public ushort GenericParameterCount {
      get { return this.PartiallySpecializedParameter.GenericParameterCount; }
    }

    /// <summary>
    /// True if the type may not be instantiated.
    /// </summary>
    /// <value></value>
    public bool IsAbstract {
      get { return this.PartiallySpecializedParameter.IsAbstract; }
    }

    /// <summary>
    /// True if the type is a class (it is not an interface or type parameter and does not extend a special base class).
    /// Corresponds to C# class.
    /// </summary>
    /// <value></value>
    public bool IsClass {
      get { return this.PartiallySpecializedParameter.IsClass; }
    }

    /// <summary>
    /// True if the type is a delegate (it extends System.MultiCastDelegate). Corresponds to C# delegate
    /// </summary>
    /// <value></value>
    public bool IsDelegate {
      get { return this.PartiallySpecializedParameter.IsDelegate; }
    }

    /// <summary>
    /// True if the type is an enumeration (it extends System.Enum and is sealed). Corresponds to C# enum.
    /// </summary>
    /// <value></value>
    public bool IsEnum {
      get { return this.PartiallySpecializedParameter.IsEnum; }
    }

    /// <summary>
    /// True if this type is parameterized (this.GenericParameters is a non empty collection).
    /// </summary>
    /// <value></value>
    public bool IsGeneric {
      get { return this.PartiallySpecializedParameter.IsGeneric; }
    }

    /// <summary>
    /// True if the type is an interface.
    /// </summary>
    /// <value></value>
    public bool IsInterface {
      get { return this.PartiallySpecializedParameter.IsInterface; }
    }

    /// <summary>
    /// True if the type is a reference type. A reference type is non static class or interface or a suitably constrained type parameter.
    /// A type parameter for which MustBeReferenceType (the class constraint in C#) is true returns true for this property
    /// as does a type parameter with a constraint that is a class.
    /// </summary>
    /// <value></value>
    public bool IsReferenceType {
      get { return this.PartiallySpecializedParameter.IsReferenceType; }
    }

    /// <summary>
    /// True if the type may not be subtyped.
    /// </summary>
    /// <value></value>
    public bool IsSealed {
      get { return this.PartiallySpecializedParameter.IsSealed; }
    }

    /// <summary>
    /// True if the type is an abstract sealed class that directly extends System.Object and declares no constructors.
    /// </summary>
    /// <value></value>
    public bool IsStatic {
      get { return this.PartiallySpecializedParameter.IsStatic; }
    }

    /// <summary>
    /// True if the type is a value type.
    /// Value types are sealed and extend System.ValueType or System.Enum.
    /// A type parameter for which MustBeValueType (the struct constraint in C#) is true also returns true for this property.
    /// </summary>
    /// <value></value>
    public bool IsValueType {
      get { return this.PartiallySpecializedParameter.IsValueType; }
    }

    /// <summary>
    /// True if the type is a struct (its not Primitive, is sealed and base is System.ValueType).
    /// </summary>
    /// <value></value>
    public bool IsStruct {
      get { return this.PartiallySpecializedParameter.IsStruct; }
    }

    /// <summary>
    /// The collection of member instances that are members of this scope.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    /// <summary>
    /// Zero or more methods defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    /// <summary>
    /// Zero or more nested types defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    /// <summary>
    /// Zero or more properties defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    /// <summary>
    /// Size of an object of this type. In bytes. If zero, the size is unspecified and will be determined at runtime.
    /// </summary>
    /// <value></value>
    public uint SizeOf {
      get { return this.PartiallySpecializedParameter.SizeOf; }
    }

    /// <summary>
    /// Declarative security actions for this type. Will be empty if this.HasSecurity is false.
    /// </summary>
    /// <value></value>
    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return this.PartiallySpecializedParameter.SecurityAttributes; }
    }

    /// <summary>
    /// Returns a reference to the underlying (integral) type on which this (enum) type is based.
    /// </summary>
    /// <value></value>
    public ITypeReference UnderlyingType {
      get { return this.PartiallySpecializedParameter.UnderlyingType; }
    }

    /// <summary>
    /// Unless the value of TypeCode is PrimitiveTypeCode.NotPrimitive, the type corresponds to a "primitive" CLR type (such as System.Int32) and
    /// the type code identifies which of the primitive types it corresponds to.
    /// </summary>
    /// <value></value>
    public PrimitiveTypeCode TypeCode {
      get { return this.PartiallySpecializedParameter.TypeCode; }
    }

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance.
    /// </summary>
    /// <value></value>
    public IEnumerable<ILocation> Locations {
      get { return this.PartiallySpecializedParameter.Locations; }
    }

    /// <summary>
    /// Layout of the type.
    /// </summary>
    /// <value></value>
    public LayoutKind Layout {
      get { return this.PartiallySpecializedParameter.Layout; }
    }

    /// <summary>
    /// True if the type has special name.
    /// </summary>
    /// <value></value>
    public bool IsSpecialName {
      get { return this.PartiallySpecializedParameter.IsSpecialName; }
    }

    /// <summary>
    /// Is this imported from COM type library
    /// </summary>
    /// <value></value>
    public bool IsComObject {
      get { return this.PartiallySpecializedParameter.IsComObject; }
    }

    /// <summary>
    /// True if this type is serializable.
    /// </summary>
    /// <value></value>
    public bool IsSerializable {
      get { return this.PartiallySpecializedParameter.IsSerializable; }
    }

    /// <summary>
    /// Is type initialized anytime before first access to static field
    /// </summary>
    /// <value></value>
    public bool IsBeforeFieldInit {
      get { return this.PartiallySpecializedParameter.IsBeforeFieldInit; }
    }

    /// <summary>
    /// Default marshalling of the Strings in this class.
    /// </summary>
    /// <value></value>
    public StringFormatKind StringFormat {
      get { return this.PartiallySpecializedParameter.StringFormat; }
    }

    /// <summary>
    /// True if this type gets special treatment from the runtime.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeSpecial {
      get { return this.PartiallySpecializedParameter.IsRuntimeSpecial; }
    }

    /// <summary>
    /// True if this type has a non empty collection of SecurityAttributes or the System.Security.SuppressUnmanagedCodeSecurityAttribute.
    /// </summary>
    /// <value></value>
    public bool HasDeclarativeSecurity {
      get { return this.PartiallySpecializedParameter.HasDeclarativeSecurity; }
    }

    /// <summary>
    /// Zero or more private type members generated by the compiler for implementation purposes. These members
    /// are only available after a complete visit of all of the other members of the type, including the bodies of methods.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.PartiallySpecializedParameter.PrivateHelperMembers; }
    }

    #endregion

    #region IDefinition Members

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition.
    /// </summary>
    /// <value></value>
    public IEnumerable<ICustomAttribute> Attributes {
      get { return this.PartiallySpecializedParameter.Attributes; }
    }

    #endregion

    #region IDoubleDispatcher Members

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

    #endregion

    #region IParameterListEntry Members

    /// <summary>
    /// The position in the parameter list where this instance can be found.
    /// </summary>
    /// <value></value>
    public ushort Index {
      get { return this.PartiallySpecializedParameter.Index; }
    }

    #endregion

    #region INamedEntity Members

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.PartiallySpecializedParameter.Name; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    /// <summary>
    /// Return true if the given member instance is a member of this scope.
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    [Pure]
    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    /// <summary>
    /// Returns the list of members with the given name that also satisfy the given predicate.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    /// <summary>
    /// Returns the list of members that satisfy the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    /// <summary>
    /// Returns the list of members with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    [Pure]
    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
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

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsModified {
      get { return false; }
    }

    /// <summary>
    /// The type definition being referred to.
    /// In case this type was alias, this is also the type of the aliased type
    /// </summary>
    /// <value></value>
    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
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
    /// Returns the unique interned key associated with the type. This takes unification/aliases/custom modifiers into account.
    /// </summary>
    /// <value></value>
    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    #endregion

    #region INamedTypeReference Members

    /// <summary>
    /// If true, the persisted type name is mangled by appending "`n" where n is the number of type parameters, if the number of type parameters is greater than 0.
    /// </summary>
    /// <value></value>
    public bool MangleName {
      get { return false; }
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
  }

  /// <summary>
  /// 
  /// </summary>
  public class SpecializedGenericTypeParameter : SpecializedGenericParameter<IGenericTypeParameter>, IGenericTypeParameter {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="partiallySpecializedParameter"></param>
    /// <param name="definingTypeInstance"></param>
    /// <param name="internFactory"></param>
    public SpecializedGenericTypeParameter(IGenericTypeParameter partiallySpecializedParameter, ISpecializedNestedTypeReference definingTypeInstance, IInternFactory internFactory)
      : base(partiallySpecializedParameter, internFactory) {
      this.definingType = definingTypeInstance;
    }

    /// <summary>
    /// Return the innermost containing generic type instance.
    /// </summary>
    public IGenericTypeInstanceReference ContainingGenericTypeInstance {
      get {
        if (this.containingGenericTypeInstance == null) {
          ITypeReference typeReference = this.DefiningType.ContainingType;
          var containingGenericTypeInstance = typeReference as IGenericTypeInstanceReference;
          if (containingGenericTypeInstance == null) {
            ISpecializedNestedTypeReference nested = typeReference as ISpecializedNestedTypeReference;
            while (containingGenericTypeInstance == null && nested != null) {
              containingGenericTypeInstance = nested.ContainingType as IGenericTypeInstanceReference;
              nested = nested.ContainingType as ISpecializedNestedTypeReference;
            }
            Debug.Assert(containingGenericTypeInstance != null);
          }
          this.containingGenericTypeInstance = containingGenericTypeInstance;
        }
        return this.containingGenericTypeInstance;
      }
    }

    private IGenericTypeInstanceReference containingGenericTypeInstance;

    /// <summary>
    /// A list of classes or interfaces. All type arguments matching this parameter must be derived from all of the classes and implement all of the interfaces.
    /// </summary>
    /// <value></value>
    public override IEnumerable<ITypeReference> Constraints {
      get {
        if (this.constraints == null) {
          var constrs = new List<ITypeReference>();
          foreach (ITypeReference partiallySpecializedConstraint in this.PartiallySpecializedParameter.Constraints)
            constrs.Add(TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(partiallySpecializedConstraint, this.ContainingGenericTypeInstance, this.InternFactory));
          constrs.TrimExcess();
          this.constraints = constrs.AsReadOnly();
        }
        return this.constraints;
      }
    }
    IEnumerable<ITypeReference>/*?*/ constraints;

    /// <summary>
    /// Calls the visitor.Visit(IGenericTypeParameter) method.
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls the visitor.Visit(IGenericTypeParameterReference) method.
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericTypeParameterReference)this);
    }

    /// <summary>
    /// The generic type that defines this type parameter.
    /// </summary>
    /// <value></value>
    public ISpecializedNestedTypeReference DefiningType {
      get { return this.definingType; }
    }
    readonly ISpecializedNestedTypeReference definingType;

    #region IGenericTypeParameter Members

    ITypeDefinition IGenericTypeParameter.DefiningType {
      get { return this.DefiningType.ResolvedType; }
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

  /// <summary>
  /// A type definition that is a specialized nested type. That is, the type definition is a member of a generic type instance, or of another specialized nested type.
  /// It is specialized, because if it had any references to the type parameters of the generic type, then those references have been replaced with the type arguments of the instance.
  /// In other words, it may be less generic than before, and hence it has been "specialized".
  /// </summary>
  public class SpecializedNestedTypeDefinition : Scope<ITypeDefinitionMember>, ISpecializedNestedTypeDefinition, ISpecializedNestedTypeReference {

    /// <summary>
    /// Allocates a type definition that is a specialized nested type. That is, the type definition is a member of a generic type instance, or of another specialized nested type.
    /// It is specialized, because if it had any references to the type parameters of the generic type, then those references have been replaced with the type arguments of the instance.
    /// In other words, it may be less generic than before, and hence it has been "specialized".
    /// </summary>
    /// <param name="unspecializedVersion">The most generic version of the nested type. In other words, the one that the programmer wrote. In persisted metadata, type references are 
    /// always to instantiations of the unspecialized version, with all inherited type arguments repeated inside the reference.</param>
    /// <param name="partiallySpecializedVersion">If containingGenericTypeInstance is an instance of a specialized nested generic type, then its members already have been specialized as
    /// part of the specialization of the generic (template) type. In that case, partiallySpecializedVersion is different from unspecialized version. At any rate, the thing to actually
    /// specialize if partiallySpecializedVersion. unspecializedVersion is used mainly for mapping to the CLR PE file format.</param>
    /// <param name="containingGenericTypeInstance">The generic type instance that supplies the type arguments that will be substituted for type parameters to create the specialized nested
    /// type created by this construtor.</param>
    /// <param name="containingTypeDefinition">The actual type that contains the specialized member constructed by this constructor. It can either be a generic type instance, or specialized
    /// nested type.</param>
    /// <param name="internFactory">The intern factory to use for computing the interned identity of this type and any types and members referenced by it.</param>
    public SpecializedNestedTypeDefinition(INestedTypeDefinition unspecializedVersion, INestedTypeDefinition partiallySpecializedVersion, ITypeDefinition containingTypeDefinition, GenericTypeInstance containingGenericTypeInstance, IInternFactory internFactory) {
      this.unspecializedVersion = unspecializedVersion;
      this.partiallySpecializedVersion = partiallySpecializedVersion;
      this.containingGenericTypeInstance = containingGenericTypeInstance;
      this.containingTypeDefinition = containingTypeDefinition;
      this.internFactory = internFactory;
    }

    /// <summary>
    /// Zero or more classes from which this type is derived.
    /// For CLR types this collection is empty for interfaces and System.Object and populated with exactly one base type for all other types.
    /// </summary>
    public IEnumerable<ITypeReference> BaseClasses {
      get {
        if (this.baseClasses == null) {
          var bclasses = new List<ITypeReference>(1);
          foreach (var partiallySpecializedBaseClassRef in this.partiallySpecializedVersion.BaseClasses)
            bclasses.Add(this.CopyAndSpecialize(partiallySpecializedBaseClassRef));
          bclasses.TrimExcess();
          this.baseClasses = bclasses.AsReadOnly();
        }
        return this.baseClasses;
      }
    }
    IEnumerable<ITypeReference>/*?*/ baseClasses;

    /// <summary>
    /// If true, the type does not inherit generic parameters from its containing type.
    /// </summary>
    public bool DoesNotInheritGenericParameters {
      get { return this.UnspecializedVersion.DoesNotInheritGenericParameters; }
    }

    internal static ITypeReference DeepCopyTypeReference(INestedTypeReference nestedType, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      var parentCopy = TypeDefinition.DeepCopyTypeReference(nestedType.ContainingType, targetContainer, internFactory);
      if (parentCopy == nestedType.ContainingType) return nestedType;
      return new SpecializedNestedTypeReference(TypeHelper.Unspecialize(nestedType), parentCopy, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(INestedTypeReference nestedType, ITypeReference targetContainer, IInternFactory internFactory) {
      var parentCopy = TypeHelper.SpecializeTypeReference(nestedType.ContainingType, targetContainer, internFactory);
      if (parentCopy == nestedType.ContainingType) return nestedType;
      return new SpecializedNestedTypeReference(TypeHelper.Unspecialize(nestedType), parentCopy, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(INestedTypeReference nestedType, IMethodReference targetContainer, IInternFactory internFactory) {
      var parentCopy = TypeHelper.SpecializeTypeReference(nestedType.ContainingType, targetContainer, internFactory);
      if (parentCopy == nestedType.ContainingType) return nestedType;
      return new SpecializedNestedTypeReference(TypeHelper.Unspecialize(nestedType), parentCopy, internFactory);
    }

    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(INestedTypeReference nestedType, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      var specializedParent = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(nestedType.ContainingType, specializedMethodReference, internFactory);
      if (specializedParent == nestedType.ContainingType) return nestedType;
      return new SpecializedNestedTypeReference(TypeHelper.Unspecialize(nestedType), specializedParent, internFactory);
    }

    internal static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(INestedTypeReference nestedType, IGenericMethodInstanceReference genericMethodInstance, IInternFactory internFactory) {
      var specializedParent = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(nestedType.ContainingType, genericMethodInstance, internFactory);
      if (specializedParent == nestedType.ContainingType) return nestedType;
      return new SpecializedNestedTypeReference(TypeHelper.Unspecialize(nestedType), specializedParent, internFactory);
    }

    internal static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(INestedTypeReference nestedType, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      var specializedParent = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(nestedType.ContainingType, containingTypeInstance, internFactory);
      if (specializedParent == nestedType.ContainingType) return nestedType;
      return new SpecializedNestedTypeReference(TypeHelper.Unspecialize(nestedType), specializedParent, internFactory);
    }

    /// <summary>
    /// Zero or more parameters that can be used as type annotations.
    /// </summary>
    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get {
        if (this.genericParameters == null) {
          lock (GlobalLock.LockingObject) {
            if (this.genericParameters == null) {
              var gpars = new List<IGenericTypeParameter>(this.GenericParameterCount);
              foreach (IGenericTypeParameter parameter in this.partiallySpecializedVersion.GenericParameters)
                gpars.Add(new SpecializedGenericTypeParameter(parameter, this, this.InternFactory));
              this.genericParameters = gpars.AsReadOnly();
            }
          }
        }
        return this.genericParameters;
      }
    }
    IEnumerable<IGenericTypeParameter>/*?*/ genericParameters;

    protected override void InitializeIfNecessary() {
      if (this.initialized) return;
      lock (GlobalLock.LockingObject) {
        if (this.initialized) return;
        foreach (ITypeDefinitionMember partiallySpecializedMember in this.partiallySpecializedVersion.Members) {
          //^ assume unspecializedMember is IEventDefinition || unspecializedMember is IFieldDefinition || unspecializedMember is IMethodDefinition ||
          //^   unspecializedMember is IPropertyDefinition || unspecializedMember is INestedTypeDefinition; //follows from informal post condition on Members property.
          this.AddMemberToCache(this.SpecializeMember(partiallySpecializedMember, this.InternFactory));
        }
        this.initialized = true;
      }
    }
    private bool initialized;

    /// <summary>
    /// The generic type instance that supplies the mapping from type parameters to type arguments that is used to create this specialized type definition.
    /// Any type references made by this nested type, such as to its base class, interfaces, or made by members of this nested type, are created by
    /// specializing the corresponding reference from the partially specialized version of this type, using the mapping defined by this instance.
    /// </summary>
    public GenericTypeInstance ContainingGenericTypeInstance {
      get { return this.containingGenericTypeInstance; }
    }
    readonly GenericTypeInstance containingGenericTypeInstance;

    /// <summary>
    /// Zero or more implementation overrides provided by the class.
    /// </summary>
    /// <value></value>
    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get {
        foreach (var methodImplementation in this.UnspecializedVersion.ExplicitImplementationOverrides)
          yield return new SpecializedMethodImplementation(this, methodImplementation, this.InternFactory);
      }
    }

    /// <summary>
    /// Zero or more interfaces implemented by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeReference> Interfaces {
      get {
        if (this.interfaces == null) {
          var ifaces = new List<ITypeReference>();
          foreach (ITypeReference partiallySpecializedInterfaceRef in this.partiallySpecializedVersion.Interfaces)
            ifaces.Add(this.CopyAndSpecialize(partiallySpecializedInterfaceRef));
          ifaces.TrimExcess();
          this.interfaces = ifaces.AsReadOnly();
        }
        return this.interfaces;
      }
    }
    IEnumerable<ITypeReference>/*?*/ interfaces;

    /// <summary>
    /// An instance of this generic type that has been obtained by using the generic parameters as the arguments.
    /// Use this instance to look up members
    /// </summary>
    /// <value></value>
    public IGenericTypeInstanceReference InstanceType {
      get {
        //^ requires this.IsGeneric;
        if (instanceType == null) {
          lock (GlobalLock.LockingObject) {
            if (this.instanceType == null) {
              List<ITypeReference> arguments = new List<ITypeReference>();
              foreach (IGenericTypeParameter gpar in this.GenericParameters) arguments.Add(gpar);
              this.instanceType = GenericTypeInstance.GetGenericTypeInstance(this, arguments, this.InternFactory);
            }
          }
        }
        return instanceType;
      }
    }
    IGenericTypeInstanceReference/*?*/ instanceType= null;

    /// <summary>
    /// Zero or more methods defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IMethodDefinition> Methods {
      get { return IteratorHelper.GetFilterEnumerable<ITypeDefinitionMember, IMethodDefinition>(this.Members); }
    }

    internal readonly INestedTypeDefinition partiallySpecializedVersion;

    /// <summary>
    /// A way to get to platform types such as System.Object.
    /// </summary>
    /// <value></value>
    public IPlatformType PlatformType {
      get { return this.partiallySpecializedVersion.PlatformType; }
    }

    /// <summary>
    /// Zero or more nested types defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return IteratorHelper.GetFilterEnumerable<ITypeDefinitionMember, INestedTypeDefinition>(this.Members); }
    }

    /// <summary>
    /// Zero or more private type members generated by the compiler for implementation purposes. These members
    /// are only available after a complete visit of all of the other members of the type, including the bodies of methods.
    /// </summary>
    /// <value></value>
    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    /// <summary>
    /// Zero or more properties defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IPropertyDefinition> Properties {
      get { return IteratorHelper.GetFilterEnumerable<ITypeDefinitionMember, IPropertyDefinition>(this.Members); }
    }

    /// <summary>
    /// Makes a copy of the given type reference, making sure that any references to this.partiallySpecializedVersion.ContainingType or something defined, directly or indirectly,
    /// by this.partiallySpecializedVersion.Containing type are replaced with the equivalent reference to this.ContainingType or something defined, directly or indirectly
    /// by this.ContainingType. Also replaces all references to type parameters of this.ContainingGenericTypeInstance with the corresponding type arguments.
    /// </summary>
    /// <param name="partiallySpecializedTypeReference">A type reference obtained from some part of this.unspecializedVersion.</param>
    private ITypeReference CopyAndSpecialize(ITypeReference partiallySpecializedTypeReference) {
      partiallySpecializedTypeReference = TypeDefinition.DeepCopyTypeReference(partiallySpecializedTypeReference, this, this.InternFactory);
      return TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(partiallySpecializedTypeReference, this.ContainingGenericTypeInstance, this.InternFactory);
    }

    /// <summary>
    /// Returns a reference to the underlying (integral) type on which this (enum) type is based.
    /// </summary>
    /// <value></value>
    public ITypeReference UnderlyingType {
      get { return this.UnspecializedVersion.UnderlyingType; }
    }

    /// <summary>
    /// The nested type that has been specialized to obtain this nested type. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    /// <value></value>
    public INestedTypeDefinition UnspecializedVersion {
      get { return this.unspecializedVersion; }
    }
    readonly INestedTypeDefinition unspecializedVersion;

    /// <summary>
    /// Indicates if the member is public or confined to its containing type, derived types and/or declaring assembly.
    /// </summary>
    /// <value></value>
    public TypeMemberVisibility Visibility {
      get {
        if (this.visibility == TypeMemberVisibility.Default) {
          this.visibility = TypeHelper.VisibilityIntersection(this.partiallySpecializedVersion.Visibility,
            TypeHelper.TypeVisibilityAsTypeMemberVisibility(this.ContainingGenericTypeInstance));
        }
        return this.visibility;
      }
    }
    TypeMemberVisibility visibility = TypeMemberVisibility.Default;

    #region INestedTypeDefinition Members

    /// <summary>
    /// The type definition that contains this member.
    /// </summary>
    /// <value></value>
    public ITypeDefinition ContainingTypeDefinition {
      get { return this.containingTypeDefinition; }
    }
    ITypeDefinition containingTypeDefinition;

    #endregion

    #region ITypeDefinition Members

    /// <summary>
    /// The byte alignment that values of the given type ought to have. Must be a power of 2. If zero, the alignment is decided at runtime.
    /// </summary>
    /// <value></value>
    public ushort Alignment {
      get { return this.UnspecializedVersion.Alignment; }
    }

    /// <summary>
    /// Zero or more events defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IEventDefinition> Events {
      get { return IteratorHelper.GetFilterEnumerable<ITypeDefinitionMember, IEventDefinition>(this.Members); }
    }

    /// <summary>
    /// Zero or more fields defined by this type.
    /// </summary>
    /// <value></value>
    public IEnumerable<IFieldDefinition> Fields {
      get { return IteratorHelper.GetFilterEnumerable<ITypeDefinitionMember, IFieldDefinition>(this.Members); }
    }

    /// <summary>
    /// The number of generic parameters. Zero if the type is not generic.
    /// </summary>
    /// <value></value>
    public ushort GenericParameterCount {
      get { return this.UnspecializedVersion.GenericParameterCount; }
    }

    /// <summary>
    /// True if the type may not be instantiated.
    /// </summary>
    /// <value></value>
    public bool IsAbstract {
      get { return this.UnspecializedVersion.IsAbstract; }
    }

    /// <summary>
    /// True if the type is a class (it is not an interface or type parameter and does not extend a special base class).
    /// Corresponds to C# class.
    /// </summary>
    /// <value></value>
    public bool IsClass {
      get { return this.UnspecializedVersion.IsClass; }
    }

    /// <summary>
    /// True if the type is a delegate (it extends System.MultiCastDelegate). Corresponds to C# delegate
    /// </summary>
    /// <value></value>
    public bool IsDelegate {
      get { return this.UnspecializedVersion.IsDelegate; }
    }

    /// <summary>
    /// True if the type is an enumeration (it extends System.Enum and is sealed). Corresponds to C# enum.
    /// </summary>
    /// <value></value>
    public bool IsEnum {
      get { return this.UnspecializedVersion.IsEnum; }
    }

    /// <summary>
    /// True if this type is parameterized (this.GenericParameters is a non empty collection).
    /// </summary>
    /// <value></value>
    public bool IsGeneric {
      get { return this.UnspecializedVersion.IsGeneric; }
    }

    /// <summary>
    /// True if the type is an interface.
    /// </summary>
    /// <value></value>
    public bool IsInterface {
      get { return this.UnspecializedVersion.IsInterface; }
    }

    /// <summary>
    /// True if the type is a reference type. A reference type is non static class or interface or a suitably constrained type parameter.
    /// A type parameter for which MustBeReferenceType (the class constraint in C#) is true returns true for this property
    /// as does a type parameter with a constraint that is a class.
    /// </summary>
    /// <value></value>
    public bool IsReferenceType {
      get { return this.UnspecializedVersion.IsReferenceType; }
    }

    /// <summary>
    /// True if the type may not be subtyped.
    /// </summary>
    /// <value></value>
    public bool IsSealed {
      get { return this.UnspecializedVersion.IsSealed; }
    }

    /// <summary>
    /// True if the type is an abstract sealed class that directly extends System.Object and declares no constructors.
    /// </summary>
    /// <value></value>
    public bool IsStatic {
      get { return this.UnspecializedVersion.IsStatic; }
    }

    /// <summary>
    /// True if the type is a value type.
    /// Value types are sealed and extend System.ValueType or System.Enum.
    /// A type parameter for which MustBeValueType (the struct constraint in C#) is true also returns true for this property.
    /// </summary>
    /// <value></value>
    public bool IsValueType {
      get { return this.UnspecializedVersion.IsValueType; }
    }

    /// <summary>
    /// True if the type is a struct (its not Primitive, is sealed and base is System.ValueType).
    /// </summary>
    /// <value></value>
    public bool IsStruct {
      get { return this.UnspecializedVersion.IsStruct; }
    }

    /// <summary>
    /// Size of an object of this type. In bytes. If zero, the size is unspecified and will be determined at runtime.
    /// </summary>
    /// <value></value>
    public uint SizeOf {
      get { return this.UnspecializedVersion.SizeOf; }
    }

    /// <summary>
    /// Declarative security actions for this type. Will be empty if this.HasSecurity is false.
    /// </summary>
    /// <value></value>
    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get
        //^^ requires this.HasSecurityAttributes;
      {
        return this.UnspecializedVersion.SecurityAttributes;
      }
    }

    public ITypeDefinitionMember SpecializeMember(ITypeDefinitionMember unspecializedMember, IInternFactory internFactory)
      //^ requires unspecializedMember is IEventDefinition || unspecializedMember is IFieldDefinition || unspecializedMember is IMethodDefinition ||
      //^   unspecializedMember is IPropertyDefinition || unspecializedMember is INestedTypeDefinition;
      //^ ensures unspecializedMember is IEventDefinition ==> result is IEventDefinition;
      //^ ensures unspecializedMember is IFieldDefinition ==> result is IFieldDefinition;
      //^ ensures unspecializedMember is IMethodDefinition ==> result is IMethodDefinition;
      //^ ensures unspecializedMember is IPropertyDefinition ==> result is IPropertyDefinition;
      //^ ensures unspecializedMember is INestedTypeDefinition ==> result is INestedTypeDefinition;
    {
      IEventDefinition/*?*/ eventDef = unspecializedMember as IEventDefinition;
      if (eventDef != null) {
        var unspecializedEventDef = eventDef;
        var specializedEventDef = eventDef as ISpecializedEventDefinition;
        if (specializedEventDef != null) unspecializedEventDef = specializedEventDef.UnspecializedVersion;
        return new SpecializedEventDefinition(unspecializedEventDef, eventDef, this, this.ContainingGenericTypeInstance);
      }
      IFieldDefinition/*?*/ fieldDef = unspecializedMember as IFieldDefinition;
      if (fieldDef != null) {
        var unspecializedFieldDef = fieldDef;
        var specializedFieldDef = fieldDef as ISpecializedFieldDefinition;
        if (specializedFieldDef != null) unspecializedFieldDef = specializedFieldDef.UnspecializedVersion;
        return new SpecializedFieldDefinition(unspecializedFieldDef, fieldDef, this, this.ContainingGenericTypeInstance);
      }
      IMethodDefinition/*?*/ methodDef = unspecializedMember as IMethodDefinition;
      if (methodDef != null) {
        var unspecializedMethodDef = methodDef;
        var specializedMethodDef = methodDef as ISpecializedMethodDefinition;
        if (specializedMethodDef != null) unspecializedMethodDef = specializedMethodDef.UnspecializedVersion;
        return new SpecializedMethodDefinition(unspecializedMethodDef, methodDef, this, this.ContainingGenericTypeInstance);
      }
      IPropertyDefinition/*?*/ propertyDef = unspecializedMember as IPropertyDefinition;
      if (propertyDef != null) {
        var unspecializedPropertyDef = propertyDef;
        var specializedPropertyDef = propertyDef as ISpecializedPropertyDefinition;
        if (specializedPropertyDef != null) unspecializedPropertyDef = specializedPropertyDef.UnspecializedVersion;
        return new SpecializedPropertyDefinition(unspecializedPropertyDef, propertyDef, this, this.ContainingGenericTypeInstance);
      }
      //^ assert unspecializedMember is INestedTypeDefinition;
      INestedTypeDefinition nestedTypeDef = (INestedTypeDefinition)unspecializedMember;
      var unspecializedTypeDef = nestedTypeDef;
      var specializedTypeDef = nestedTypeDef as ISpecializedNestedTypeDefinition;
      if (specializedTypeDef != null) unspecializedTypeDef = specializedTypeDef.UnspecializedVersion;
      return new SpecializedNestedTypeDefinition(unspecializedTypeDef, nestedTypeDef, this, this.ContainingGenericTypeInstance, internFactory);
    }

    /// <summary>
    /// Unless the value of TypeCode is PrimitiveTypeCode.NotPrimitive, the type corresponds to a "primitive" CLR type (such as System.Int32) and
    /// the type code identifies which of the primitive types it corresponds to.
    /// </summary>
    /// <value></value>
    public PrimitiveTypeCode TypeCode {
      get { return this.UnspecializedVersion.TypeCode; }
    }

    public LayoutKind Layout {
      get { return this.UnspecializedVersion.Layout; }
    }

    public bool IsSpecialName {
      get { return this.UnspecializedVersion.IsSpecialName; }
    }

    public bool IsComObject {
      get { return this.UnspecializedVersion.IsComObject; }
    }

    public bool IsSerializable {
      get { return this.UnspecializedVersion.IsSerializable; }
    }

    public bool IsBeforeFieldInit {
      get { return this.UnspecializedVersion.IsBeforeFieldInit; }
    }

    public StringFormatKind StringFormat {
      get { return this.UnspecializedVersion.StringFormat; }
    }

    public bool IsRuntimeSpecial {
      get { return this.UnspecializedVersion.IsRuntimeSpecial; }
    }

    public bool HasDeclarativeSecurity {
      get
        //^ ensures result == this.UnspecializedVersion.HasDeclarativeSecurity;
      {
        return this.UnspecializedVersion.HasDeclarativeSecurity;
      }
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return this.UnspecializedVersion.Attributes; }
    }

    public IEnumerable<ILocation> Locations {
      get { return this.UnspecializedVersion.Locations; }
    }

    #endregion

    #region IDoubleDispatcher Members

    /// <summary>
    /// Calls visitor.Visit(INestedTypeDefinition).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(INestedTypeReference).
    /// </summary>
    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((ISpecializedNestedTypeReference)this);
    }

    #endregion

    #region IContainer<ITypeDefinitionMember>

    IEnumerable<ITypeDefinitionMember> IContainer<ITypeDefinitionMember>.Members {
      get {
        return this.Members;
      }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.UnspecializedVersion.Name; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.ContainingTypeDefinition; }
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

    #endregion

    #region ITypeMemberReference Members

    ITypeReference ITypeMemberReference.ContainingType {
      get { return this.ContainingTypeDefinition; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }
    #endregion

    #region INestedTypeReference Members

    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region ITypeReference Members

    /// <summary>
    /// The intern factory to use for computing the interned identity of this type and any types and members referenced by it.
    /// </summary>
    public IInternFactory InternFactory {
      get { return this.internFactory; }
    }
    readonly IInternFactory internFactory;

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    #endregion

    #region ISpecializedNestedTypeReference Members

    INestedTypeReference ISpecializedNestedTypeReference.UnspecializedVersion {
      get { return this.UnspecializedVersion; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return this.UnspecializedVersion.MangleName; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion
  }

  public class SpecializedNestedTypeReference : ISpecializedNestedTypeReference {

    /// <summary>
    /// A reference to a nested type of a generic type instance. It is specialized because any occurrences of the type parameters have been replaced with the
    /// corresponding type arguments from the instance.
    /// </summary>
    /// <param name="unspecializedVersion"></param>
    /// <param name="containingType"></param>
    /// <param name="internFactory"></param>
    public SpecializedNestedTypeReference(INestedTypeReference unspecializedVersion, ITypeReference containingType, IInternFactory internFactory) {
      Contract.Requires(!(unspecializedVersion is ISpecializedNestedTypeReference));
      Contract.Requires(!(unspecializedVersion.ContainingType is ISpecializedNestedTypeReference));
      Contract.Requires(!(unspecializedVersion.ContainingType is IGenericTypeInstanceReference));
      this.unspecializedVersion = unspecializedVersion;
      this.containingType = containingType;
      this.internFactory = internFactory;
    }

    IInternFactory internFactory;

    /// <summary>
    /// The corresponding (unspecialized) member from the generic type (template) that was instantiated to obtain the containing type
    /// of this member.
    /// </summary>
    public INestedTypeReference/*!*/ UnspecializedVersion {
      get { return this.unspecializedVersion; }
    }
    readonly INestedTypeReference/*!*/ unspecializedVersion;

    #region INestedTypeReference Members

    public ushort GenericParameterCount {
      get { return this.UnspecializedVersion.GenericParameterCount; }
    }

    public INestedTypeDefinition ResolvedType {
      get {
        if (this.resolvedType == null)
          this.resolvedType = TypeHelper.GetNestedType(this.ContainingType.ResolvedType, this.Name, this.GenericParameterCount);
        return this.resolvedType;
      }
    }
    INestedTypeDefinition resolvedType;

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    #endregion

    #region INamedTypeReference Members


    public bool MangleName {
      get { return this.UnspecializedVersion.MangleName; }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get {
        if (this.ResolvedType is Dummy) return Dummy.NamedTypeDefinition;
        return this.ResolvedType;
      }
    }

    #endregion

    #region ITypeMemberReference Members

    /// <summary>
    /// A reference to the containing type of the referenced type member.
    /// </summary>
    /// <value></value>
    public ITypeReference ContainingType {
      get { return this.containingType; }
    }
    ITypeReference containingType;

    /// <summary>
    /// The type definition member this reference resolves to.
    /// </summary>
    /// <value></value>
    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get {
        return this.ResolvedType;
      }
    }
    #endregion

    #region ITypeReference Members

    public IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public uint InternedKey {
      get {
        if (this.internedKey == 0)
          this.internedKey = this.internFactory.GetNestedTypeReferenceInternedKey(this.ContainingType, this.Name, this.GenericParameterCount);
        return this.internedKey;
      }
    }
    uint internedKey;

    public bool IsAlias {
      get { return false; }
    }

    public bool IsEnum {
      get { return this.UnspecializedVersion.IsEnum; }
    }

    public bool IsValueType {
      get { return this.UnspecializedVersion.IsValueType; }
    }

    public IPlatformType PlatformType {
      get { return this.UnspecializedVersion.PlatformType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get {
        if (this.ResolvedType is Dummy) return Dummy.TypeDefinition;
        return this.ResolvedType;
      }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    #endregion

    #region IReference Members

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition.
    /// </summary>
    /// <value></value>
    public IEnumerable<ICustomAttribute> Attributes {
      get { return this.UnspecializedVersion.Attributes; }
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedNestedTypeReference).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedNestedTypeReference).
    /// </summary>
    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance.
    /// </summary>
    /// <value></value>
    public IEnumerable<ILocation> Locations {
      get { return this.UnspecializedVersion.Locations; }
    }

    #endregion

    #region INamedEntity Members

    /// <summary>
    /// The name of the entity.
    /// </summary>
    public IName Name {
      get { return this.UnspecializedVersion.Name; }
    }

    #endregion

  }

  public abstract class SystemDefinedStructuralType : ITypeDefinition {

    protected SystemDefinedStructuralType(IInternFactory internFactory) {
      this.internFactory = internFactory;
    }

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public virtual IEnumerable<ITypeReference> BaseClasses {
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

    public bool HasDeclarativeSecurity {
      get { return true; }
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

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get
        //^ ensures result == false;
      {
        return false;
      }
    }

    public bool IsInterface {
      get { return false; }
    }

    public virtual bool IsReferenceType {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public virtual IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public abstract IPlatformType PlatformType { get; }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.AutoChar; }
    }

    public override string ToString() {
      return TypeHelper.GetTypeName(this);
    }

    public virtual PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.NotPrimitive; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
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

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    [Pure]
    public virtual bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public virtual IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public virtual IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public virtual IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    [Pure]
    IEnumerable<ITypeDefinitionMember> IScope<ITypeDefinitionMember>.GetMembersNamed(IName name, bool ignoreCase) {
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

    public virtual IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public virtual bool IsModified {
      get { return false; }
    }

    public ITypeDefinition ResolvedType {
      get { return this; }
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

    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.InternFactory.GetTypeReferenceInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

    #endregion
  }

  internal static class TypeDefinition {

    /// <summary>
    /// Returns a deep copy of partiallySpecializedTypeReference. In the copy, every reference to a partially specialized type parameter 
    /// of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition and generic) is 
    /// replaced with the specialized type parameter defined by targetContainer or its parents. 
    /// </summary>
    /// <remarks>
    /// The deep copy happens when we create a specialized member, such as a specialized method, property, field, parameters and so on.
    /// We must obtain a copy of any type reference in the member, which should not share nodes with the original type reference (called
    /// the partially specialized version) if the node contains generic parameters that may get specialized. Without such a copy, 
    /// the instantiation of a generic type or method will not work. In the new copy, the references to (partially specialized) generic 
    /// parameters must be replaced by the specialized version defined by the specialized parents of the specialized member. 
    /// 
    /// For example, consider A[int].B[T1 => T1+], where A[int] is a generic type instance which contains a specialized nested type
    /// B, which has a generic type parameter T1 that is specialized to T1+. Now any type reference inside B[T1=>T1+], such as types 
    /// of fields, methods, properties, and so on, must be copied and have any reference to T1 replaced by T1+. 
    ///
    /// Similar deep copy happens when specializing a method definition. <seealso cref="DeepCopyTypeReferenceWRTSpecializedMethod"/>.
    /// </remarks>
    /// <param name="partiallySpecializedTypeReference">A type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will replace the occurrences
    /// of matching type parameters in partiallySpecializedTypeReference. </param>
    /// <param name="internFactory">An intern factory.</param>
    internal static ITypeReference DeepCopyTypeReference(ITypeReference partiallySpecializedTypeReference, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory) {
      var arrayType = partiallySpecializedTypeReference as IArrayTypeReference;
      if (arrayType != null) {
        if (arrayType.IsVector) return Vector.DeepCopyTypeReference(arrayType, targetContainer, internFactory);
        return Matrix.DeepCopyTypeReference(arrayType, targetContainer, internFactory);
      }
      var genericTypeParameter = partiallySpecializedTypeReference as IGenericTypeParameterReference;
      if (genericTypeParameter != null) return GenericParameter.DeepCopyTypeReference(genericTypeParameter, targetContainer);
      var genericTypeInstance = partiallySpecializedTypeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return GenericTypeInstance.DeepCopyTypeReference(genericTypeInstance, targetContainer, internFactory);
      var managedPointerType = partiallySpecializedTypeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return ManagedPointerType.DeepCopyTypeReference(managedPointerType, targetContainer, internFactory);
      var modifiedPointer = partiallySpecializedTypeReference as ModifiedPointerType;
      if (modifiedPointer != null) return ModifiedPointerType.DeepCopyTypeReference(modifiedPointer, targetContainer, internFactory);
      var modifiedType = partiallySpecializedTypeReference as IModifiedTypeReference;
      if (modifiedType != null) return ModifiedTypeReference.DeepCopyTypeReference(modifiedType, targetContainer, internFactory);
      var nestedType = partiallySpecializedTypeReference as INestedTypeReference;
      if (nestedType != null) return SpecializedNestedTypeDefinition.DeepCopyTypeReference(nestedType, targetContainer, internFactory);
      var pointerType = partiallySpecializedTypeReference as IPointerTypeReference;
      if (pointerType != null) return PointerType.DeepCopyTypeReference(pointerType, targetContainer, internFactory);
      return partiallySpecializedTypeReference;
    }

    /// <summary>
    /// If the given partially specialized type reference is a constructed type, such as an instance of IArrayTypeReference or IPointerTypeReference 
    /// or IGenericTypeInstanceReference or INestedTypeReference, then return a new instance (if necessary) in which all refererences to the type 
    /// parameters of containingMethodInstance.GenericMethod.GenericParameters have been replaced with the corresponding values from containingMethodInstance.GenericArguments. 
    /// If the type is not a constructed type the method just returns the type. For the purpose of this method, an instance of IGenericParameter is regarded as a constructed type.
    /// </summary>
    internal static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(ITypeReference partiallySpecializedTypeReference, IGenericMethodInstanceReference containingMethodInstance, IInternFactory internFactory) {
      var arrayType = partiallySpecializedTypeReference as IArrayTypeReference;
      if (arrayType != null) {
        if (arrayType.IsVector) return Vector.SpecializeIfConstructedFromApplicableTypeParameter(arrayType, containingMethodInstance, internFactory);
        return Matrix.SpecializeIfConstructedFromApplicableTypeParameter(arrayType, containingMethodInstance, internFactory);
      }
      var genericMethodParameter = partiallySpecializedTypeReference as IGenericMethodParameterReference;
      if (genericMethodParameter != null) return GenericParameter.SpecializeIfConstructedFromApplicableTypeParameter(genericMethodParameter, containingMethodInstance);
      var genericTypeInstance = partiallySpecializedTypeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return GenericTypeInstance.SpecializeIfConstructedFromApplicableTypeParameter(genericTypeInstance, containingMethodInstance, internFactory);
      var managedPointerType = partiallySpecializedTypeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return ManagedPointerType.SpecializeIfConstructedFromApplicableTypeParameter(managedPointerType, containingMethodInstance, internFactory);
      var modifiedPointer = partiallySpecializedTypeReference as ModifiedPointerType;
      if (modifiedPointer != null) return ModifiedPointerType.SpecializeIfConstructedFromApplicableTypeParameter(modifiedPointer, containingMethodInstance, internFactory);
      var modifiedType = partiallySpecializedTypeReference as IModifiedTypeReference;
      if (modifiedType != null) return ModifiedTypeReference.SpecializeIfConstructedFromApplicableTypeParameter(modifiedType, containingMethodInstance, internFactory);
      var nestedType = partiallySpecializedTypeReference as INestedTypeReference;
      if (nestedType != null) return SpecializedNestedTypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(nestedType, containingMethodInstance, internFactory);
      var pointerType = partiallySpecializedTypeReference as IPointerTypeReference;
      if (pointerType != null) return PointerType.SpecializeIfConstructedFromApplicableTypeParameter(pointerType, containingMethodInstance, internFactory);
      return partiallySpecializedTypeReference;
    }

    /// <summary>
    /// If the given partially specialized type reference is a constructed type, such as an instance of IArrayType or IPointerType or 
    /// IGenericTypeInstance or INestedTypeReference, then return a new instance (if necessary) in which all refererences to the type 
    /// parameters of containingTypeInstance.GenericType have been replaced with the corresponding values from containingTypeInstance.GenericArguments. 
    /// If the type is not a constructed type the method just returns the type. For the purpose of this method, an instance of IGenericTypeParameterReference is regarded as a constructed type.
    /// </summary>
    internal static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(ITypeReference partiallySpecializedTypeReference, IGenericTypeInstanceReference containingTypeInstance, IInternFactory internFactory) {
      var arrayType = partiallySpecializedTypeReference as IArrayTypeReference;
      if (arrayType != null) {
        if (arrayType.IsVector) return Vector.SpecializeIfConstructedFromApplicableTypeParameter(arrayType, containingTypeInstance, internFactory);
        return Matrix.SpecializeIfConstructedFromApplicableTypeParameter(arrayType, containingTypeInstance, internFactory);
      }
      var genericTypeParameter = partiallySpecializedTypeReference as IGenericTypeParameterReference;
      if (genericTypeParameter != null) return GenericParameter.SpecializeIfConstructedFromApplicableTypeParameter(genericTypeParameter, containingTypeInstance);
      var genericTypeInstance = partiallySpecializedTypeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return GenericTypeInstance.SpecializeIfConstructedFromApplicableTypeParameter(genericTypeInstance, containingTypeInstance, internFactory);
      var managedPointerType = partiallySpecializedTypeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return ManagedPointerType.SpecializeIfConstructedFromApplicableTypeParameter(managedPointerType, containingTypeInstance, internFactory);
      var modifiedPointer = partiallySpecializedTypeReference as ModifiedPointerType;
      if (modifiedPointer != null) return ModifiedPointerType.SpecializeIfConstructedFromApplicableTypeParameter(modifiedPointer, containingTypeInstance, internFactory);
      var modifiedType = partiallySpecializedTypeReference as IModifiedTypeReference;
      if (modifiedType != null) return ModifiedTypeReference.SpecializeIfConstructedFromApplicableTypeParameter(modifiedType, containingTypeInstance, internFactory);
      var nestedType = partiallySpecializedTypeReference as INestedTypeReference;
      if (nestedType != null) return SpecializedNestedTypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(nestedType, containingTypeInstance, internFactory);
      var pointerType = partiallySpecializedTypeReference as IPointerTypeReference;
      if (pointerType != null) return PointerType.SpecializeIfConstructedFromApplicableTypeParameter(pointerType, containingTypeInstance, internFactory);
      return partiallySpecializedTypeReference;
    }

    /// <summary>
    /// Make a copy of partiallySpecializedTypeReference so that the references to the partially specialized version of generic parameters defined in 
    /// specializedMethodDefinition and its containing specialized nested type definition (if any) are replaced with the specialized version. 
    /// </summary>
    /// <remarks>
    /// Consider specialized method A[T1 -> T1].B[T2=> T2+].bar[T3 => T3+], where A[T1->T1] is the generic instance obtained from A using its generic parameter
    /// as the generic argument, A[T1->T1].B[T2=>T2+] is a specialized nested type whose containing type is A[T1->T1] and whose generic type parameter
    /// T2 is specialized to T2+, and bar[T3=>T3] is further a specialized member of A[T1->T1].B[T2=>T2+] with its generic parameter T3 specialized to 
    /// T3+. 
    /// 
    /// Suppose bar has a parameter whose type is X[T2, T3] for some generic type X. The corresponding parameter of of A[T1 -> T1].B[T2=> T2+].bar[T3 => T3+]
    /// is "specialized". The specialized parameter's type will participate future resolution and should not share nodes that may have been specialized with 
    /// the original type reference X[T2, T3] (called partially specialized version). We must obtain a copy of the partially specialized version in which we replace 
    /// the references of T2, T3 with their corresponding specialized versions, namely T2+ and T3+. 
    /// </remarks>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(ITypeReference partiallySpecializedTypeReference, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      var arrayType = partiallySpecializedTypeReference as IArrayTypeReference;
      if (arrayType != null) {
        if (arrayType.IsVector) return Vector.DeepCopyTypeReferenceWRTSpecializedMethod(arrayType, specializedMethodReference, internFactory);
        return Matrix.DeepCopyTypeReferenceWRTSpecializedMethod(arrayType, specializedMethodReference, internFactory);
      }
      var genericMethodParameter = partiallySpecializedTypeReference as IGenericMethodParameterReference;
      if (genericMethodParameter != null) return GenericParameter.DeepCopyTypeReferenceWRTSpecializedMethod(genericMethodParameter, specializedMethodReference);
      var genericTypeParameter = partiallySpecializedTypeReference as IGenericTypeParameterReference;
      if (genericTypeParameter != null) return GenericParameter.DeepCopyTypeReferenceWRTSpecializedMethod(genericTypeParameter, specializedMethodReference);
      var genericTypeInstance = partiallySpecializedTypeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return GenericTypeInstance.DeepCopyTypeReferenceWRTSpecializedMethod(genericTypeInstance, specializedMethodReference, internFactory);
      var managedPointerType = partiallySpecializedTypeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return ManagedPointerType.DeepCopyTypeReferenceWRTSpecializedMethod(managedPointerType, specializedMethodReference, internFactory);
      var modifiedPointer = partiallySpecializedTypeReference as ModifiedPointerType;
      if (modifiedPointer != null) return ModifiedPointerType.DeepCopyTypeReferenceWRTSpecializedMethod(modifiedPointer, specializedMethodReference, internFactory);
      var modifiedType = partiallySpecializedTypeReference as IModifiedTypeReference;
      if (modifiedType != null) return ModifiedTypeReference.DeepCopyTypeReferenceWRTSpecializedMethod(modifiedType, specializedMethodReference, internFactory);
      var pointerType = partiallySpecializedTypeReference as IPointerTypeReference;
      if (pointerType != null) return PointerType.DeepCopyTypeReferenceReplacingGenericMethodParameter(pointerType, specializedMethodReference, internFactory);
      var nestedType = partiallySpecializedTypeReference as INestedTypeReference;
      if (nestedType != null) return SpecializedNestedTypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(nestedType, specializedMethodReference, internFactory);
      return partiallySpecializedTypeReference;
    }
  }

  public class Vector : ArrayType {

    protected Vector(ITypeReference elementType, IInternFactory internFactory)
      : base(elementType, internFactory) {
    }

    protected override IEnumerable<ITypeReference> GetInterfaceList() {
      var coreAssembly = TypeHelper.GetDefiningUnitReference(this.PlatformType.SystemArray) as IAssemblyReference;
      var version = coreAssembly == null ? new Version(4, 0) : coreAssembly.AssemblyIdentity.Version;
      List<ITypeReference> interfaces = new List<ITypeReference>(9);
      interfaces.Add(this.PlatformType.SystemICloneable);
      interfaces.Add(this.PlatformType.SystemCollectionsIEnumerable);
      interfaces.Add(this.PlatformType.SystemCollectionsICollection);
      interfaces.Add(this.PlatformType.SystemCollectionsIList);
      if (version.Major >= 4) {
        interfaces.Add(this.PlatformType.SystemCollectionsIStructuralComparable);
        interfaces.Add(this.PlatformType.SystemCollectionsIStructuralEquatable);
      }
      if (version.Major >= 2) {
        var argTypes = IteratorHelper.GetSingletonEnumerable<ITypeReference>(this.ElementType);
        interfaces.Add(new GenericTypeInstanceReference(this.PlatformType.SystemCollectionsGenericIList, argTypes, this.InternFactory));
        interfaces.Add(new GenericTypeInstanceReference(this.PlatformType.SystemCollectionsGenericICollection, argTypes, this.InternFactory));
        interfaces.Add(new GenericTypeInstanceReference(this.PlatformType.SystemCollectionsGenericIEnumerable, argTypes, this.InternFactory));
      }
      return interfaces.AsReadOnly();
    }

    /// <summary>
    /// Returns a deep copy the array type reference. In the copy, every reference to a partially specialized type parameter defined by
    /// the partially specialized version of targetContainer or of one of targetContainer's parents (if the parent is a SpecializedNestedTypeDefinition 
    /// and generic) will be replaced with the specialized type parameter, defined by targetContainer or its parents.
    /// </summary>
    /// <param name="array">An array type reference to be deep copied. </param>
    /// <param name="targetContainer">A specialized nested type definition whose or whose parents' (specialized) type parameters will
    /// replace the occurrences of matching type parameters in <paramref name="array"/>.</param>
    ///  /// <param name="internFactory">An intern factory. </param>
    public static ITypeReference DeepCopyTypeReference(IArrayTypeReference array, SpecializedNestedTypeDefinition targetContainer, IInternFactory internFactory)
      //^ requires array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.DeepCopyTypeReference(elementType, targetContainer, internFactory);
      if (elementType == specializedElementType) return array;
      return GetVector(specializedElementType, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IArrayTypeReference array, ITypeReference targetContainer, IInternFactory internFactory)
      //^ requires array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeHelper.SpecializeTypeReference(elementType, targetContainer, internFactory);
      if (elementType == specializedElementType) return array;
      return GetVector(specializedElementType, internFactory);
    }

    internal static ITypeReference SpecializeTypeReference(IArrayTypeReference array, IMethodReference targetContainer, IInternFactory internFactory)
      //^ requires array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeHelper.SpecializeTypeReference(elementType, targetContainer, internFactory);
      if (elementType == specializedElementType) return array;
      return GetVector(specializedElementType, internFactory);
    }

    public static Vector GetVector(ITypeReference elementType, IInternFactory internFactory) {
      Contract.Ensures(Contract.Result<Vector>() != null);

      return new Vector(elementType, internFactory);
    }

    /// <summary>
    /// If the given vector has an element type that involves a type parameter from the generic method from which the given method was instantiated,
    /// then return a new vector using an element type that has been specialized with the type arguments of the given generic method instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IArrayTypeReference array, IGenericMethodInstanceReference method, IInternFactory internFactory)
      //^ requires array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(elementType, method, internFactory);
      if (elementType == specializedElementType) return array;
      return GetVector(specializedElementType, internFactory);
    }

    /// <summary>
    /// If the given vector has an element type that involves a type parameter from the generic type from which the given type was instantiated,
    /// then return a new vector using an element type that has been specialized with the type arguments of the given generic type instance.
    /// </summary>
    public static ITypeReference SpecializeIfConstructedFromApplicableTypeParameter(IArrayTypeReference array, IGenericTypeInstanceReference type, IInternFactory internFactory)
      //^ requires array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.SpecializeIfConstructedFromApplicableTypeParameter(elementType, type, internFactory);
      if (elementType == specializedElementType) return array;
      return GetVector(specializedElementType, internFactory);
    }

    /// <summary>
    /// If the given vector has an element type that involves a method type parameter from the partially specialized version of specializedMethodReference,
    /// then return a new vector using an element type that is the corresponding method type parameter from specializedMethodDefinition.
    /// </summary>
    internal static ITypeReference DeepCopyTypeReferenceWRTSpecializedMethod(IArrayTypeReference array, ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory)
      //^ requires array.IsVector;
    {
      ITypeReference elementType = array.ElementType;
      ITypeReference specializedElementType = TypeDefinition.DeepCopyTypeReferenceWRTSpecializedMethod(elementType, specializedMethodReference, internFactory);
      if (elementType == specializedElementType) return array;
      return GetVector(specializedElementType, internFactory);
    }
  }
}
#pragma warning restore 1591