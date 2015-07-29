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
using System.Text;
using System.Diagnostics.Contracts;

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// An event is a member that enables an object or class to provide notifications. Clients can attach executable code for events by supplying event handlers.
  /// This interface models the metadata representation of an event.
  /// </summary>
  public class EventDefinition : TypeDefinitionMember, IEventDefinition, ICopyFrom<IEventDefinition> {

    /// <summary>
    /// An event is a member that enables an object or class to provide notifications. Clients can attach executable code for events by supplying event handlers.
    /// This interface models the metadata representation of an event.
    /// </summary>
    public EventDefinition() {
      this.accessors = new List<IMethodReference>();
      this.adder = Dummy.MethodReference;
      this.caller = null;
      this.remover = Dummy.MethodReference;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of an event. An event is a member that enables an object or class to provide notifications.
    /// Clients can attach executable code for events by supplying event handlers.
    /// This interface models the metadata representation of an event.
    /// </summary>
    /// <param name="eventDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IEventDefinition eventDefinition, IInternFactory internFactory) {
      ((ICopyFrom<ITypeDefinitionMember>)this).Copy(eventDefinition, internFactory);
      this.accessors = new List<IMethodReference>(eventDefinition.Accessors);
      this.adder = eventDefinition.Adder;
      this.caller = eventDefinition.Caller;
      this.IsRuntimeSpecial = eventDefinition.IsRuntimeSpecial;
      this.IsSpecialName = eventDefinition.IsSpecialName;
      this.remover = eventDefinition.Remover;
      this.type = eventDefinition.Type;
    }

    /// <summary>
    /// A list of methods that are associated with the event.
    /// </summary>
    /// <value></value>
    public List<IMethodReference> Accessors {
      get { return this.accessors; }
      set { this.accessors = value; }
    }
    List<IMethodReference> accessors;

    /// <summary>
    /// The method used to add a handler to the event.
    /// </summary>
    /// <value></value>
    public IMethodReference Adder {
      get { return this.adder; }
      set { this.adder = value; }
    }
    IMethodReference adder;

    /// <summary>
    /// The method used to call the event handlers when the event occurs. May be null.
    /// </summary>
    /// <value></value>
    public IMethodReference/*?*/ Caller {
      get { return this.caller; }
      set { this.caller = value; }
    }
    IMethodReference/*?*/ caller;

    /// <summary>
    /// Calls visitor.Visit(IEventDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Throws an InvalidOperation exception since valid Metadata never refers directly to an event.
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// True if the event gets special treatment from the runtime.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeSpecial {
      get { return (this.flags & 0x40000000) != 0; }
      set {
        if (value)
          this.flags |= 0x40000000;
        else
          this.flags &= ~0x40000000;
      }
    }

    /// <summary>
    /// This event is special in some way, as specified by the name.
    /// </summary>
    /// <value></value>
    public bool IsSpecialName {
      get { return (this.flags & 0x20000000) != 0; }
      set {
        if (value)
          this.flags |= 0x20000000;
        else
          this.flags &= ~0x20000000;
      }
    }

    /// <summary>
    /// The method used to add a handler to the event.
    /// </summary>
    /// <value></value>
    public IMethodReference Remover {
      get { return this.remover; }
      set { this.remover = value; }
    }
    IMethodReference remover;

    /// <summary>
    /// The (delegate) type of the handlers that will handle the event.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IEventDefinition Members

    IEnumerable<IMethodReference> IEventDefinition.Accessors {
      get { return this.accessors.AsReadOnly(); }
    }

    #endregion
  }

  /// <summary>
  /// A field is a member that represents a variable associated with an object or class.
  /// This interface models the metadata representation of a field.
  /// </summary>
  public class FieldDefinition : TypeDefinitionMember, IFieldDefinition, ICopyFrom<IFieldDefinition> {

    /// <summary>
    /// A field is a member that represents a variable associated with an object or class.
    /// This interface models the metadata representation of a field.
    /// </summary>
    public FieldDefinition() {
      this.compileTimeValue = Dummy.Constant;
      this.customModifiers = null;
      this.fieldMapping = Dummy.SectionBlock;
      this.internFactory = Dummy.InternFactory;
      this.marshallingInformation = Dummy.MarshallingInformation;
      this.bitLength = -1;
      this.offset = 0;
      this.sequenceNumber = 0;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of a field.
    /// A field is a member that represents a variable associated with an object or class.
    /// This interface models the metadata representation of a field.
    /// </summary>
    /// <param name="fieldDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IFieldDefinition fieldDefinition, IInternFactory internFactory) {
      ((ICopyFrom<ITypeDefinitionMember>)this).Copy(fieldDefinition, internFactory);
      if (fieldDefinition.IsBitField)
        this.bitLength = (int)fieldDefinition.BitLength;
      else
        this.bitLength = -1;
      this.compileTimeValue = fieldDefinition.CompileTimeValue;
      this.IsCompileTimeConstant = fieldDefinition.IsCompileTimeConstant;
      if (fieldDefinition.IsModified)
        this.customModifiers = new List<ICustomModifier>(fieldDefinition.CustomModifiers);
      else
        this.customModifiers = null;
      if (fieldDefinition.IsMapped)
        this.fieldMapping = fieldDefinition.FieldMapping;
      else
        this.fieldMapping = Dummy.SectionBlock;
      if (fieldDefinition.IsMarshalledExplicitly)
        this.marshallingInformation = fieldDefinition.MarshallingInformation;
      else
        this.marshallingInformation = Dummy.MarshallingInformation;
      if (fieldDefinition.ContainingTypeDefinition.Layout == LayoutKind.Explicit)
        this.offset = fieldDefinition.Offset;
      else
        this.offset = 0;
      if (fieldDefinition.ContainingTypeDefinition.Layout == LayoutKind.Sequential)
        this.sequenceNumber = fieldDefinition.SequenceNumber;
      else
        this.sequenceNumber = 0;
      this.internFactory = internFactory;
      this.type = fieldDefinition.Type;
      //^ base;
      this.IsNotSerialized = fieldDefinition.IsNotSerialized;
      this.IsReadOnly = fieldDefinition.IsReadOnly;
      this.IsSpecialName = fieldDefinition.IsSpecialName;
      if (fieldDefinition.IsRuntimeSpecial) {
        //^ assume this.IsSpecialName;
        this.IsRuntimeSpecial = fieldDefinition.IsRuntimeSpecial;
      } else
        this.IsRuntimeSpecial = false;
      this.IsStatic = fieldDefinition.IsStatic;
    }

    /// <summary>
    /// The number of bits that form part of the value of the field.
    /// </summary>
    /// <value></value>
    public uint BitLength {
      get { return (uint)this.bitLength; }
      set { this.bitLength = (int)value; }
    }
    int bitLength;

    /// <summary>
    /// The compile time value of the field. This value should be used directly in IL, rather than a reference to the field.
    /// If the field does not have a valid compile time value, Dummy.Constant is returned.
    /// </summary>
    /// <value></value>
    public IMetadataConstant CompileTimeValue {
      get { return this.compileTimeValue; }
      set { this.compileTimeValue = value; }
    }
    IMetadataConstant compileTimeValue;

    /// <summary>
    /// Custom modifiers associated with the referenced field. May be null.
    /// </summary>
    public List<ICustomModifier>/*?*/ CustomModifiers {
      get { return this.customModifiers; }
      set { this.customModifiers = value; }
    }
    List<ICustomModifier>/*?*/ customModifiers;

    /// <summary>
    /// Calls visitor.Visit(IFieldDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IFieldReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IFieldReference)this);
    }

    /// <summary>
    /// Information of the location where this field is mapped to
    /// </summary>
    /// <value></value>
    public ISectionBlock FieldMapping {
      get { return this.fieldMapping; }
      set
        //^ requires this.IsStatic;
      {
        this.fieldMapping = value;
      }
    }
    ISectionBlock fieldMapping;

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

    /// <summary>
    /// Returns a key that is computed from the information in this reference and that distinguishes
    /// this.ResolvedField from all other fields obtained from the same metadata host.
    /// </summary>
    public uint InternedKey {
      get {
        if (this.internedKey == 0)
          this.internedKey = this.InternFactory.GetFieldInternedKey(this);
        return this.internedKey;
      }
    }
    uint internedKey;

    /// <summary>
    /// The field is aligned on a bit boundary and uses only the BitLength number of least significant bits of the representation of a Type value.
    /// </summary>
    public bool IsBitField {
      get { return this.bitLength >= 0; }
    }

    /// <summary>
    /// This field is a compile-time constant. The field has no runtime location and cannot be directly addressed from IL.
    /// </summary>
    /// <value></value>
    public bool IsCompileTimeConstant {
      get { return (this.flags & 0x40000000) != 0; }
      set {
        if (value)
          this.flags |= 0x40000000;
        else
          this.flags &= ~0x40000000;
      }
    }

    /// <summary>
    /// This field is mapped to an explicitly initialized (static) memory location.
    /// </summary>
    /// <value></value>
    public bool IsMapped {
      get { return !(this.fieldMapping is Dummy); }
    }

    /// <summary>
    /// This field has associated field marshalling information.
    /// </summary>
    /// <value></value>
    public bool IsMarshalledExplicitly {
      get { return !(this.marshallingInformation is Dummy); }
    }

    /// <summary>
    /// The referenced field has custom modifiers.
    /// </summary>
    public bool IsModified {
      get { return this.CustomModifiers != null && this.CustomModifiers.Count > 0; }
    }

    /// <summary>
    /// The field does not have to be serialized when its containing instance is serialized.
    /// </summary>
    /// <value></value>
    public bool IsNotSerialized {
      get { return (this.flags & 0x10000000) != 0; }
      set {
        if (value)
          this.flags |= 0x10000000;
        else
          this.flags &= ~0x10000000;
      }
    }

    /// <summary>
    /// This field can only be read. Initialization takes place in a constructor.
    /// </summary>
    /// <value></value>
    public bool IsReadOnly {
      get { return (this.flags & 0x08000000) != 0; }
      set {
        if (value)
          this.flags |= 0x08000000;
        else
          this.flags &= ~0x08000000;
      }
    }

    /// <summary>
    /// True if the field gets special treatment from the runtime.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeSpecial {
      get { return (this.flags & 0x04000000) != 0; }
      set
        //^ requires !value || this.IsSpecialName;
      {
        if (value)
          this.flags |= 0x04000000;
        else
          this.flags &= ~0x04000000;
      }
    }

    /// <summary>
    /// This field is special in some way, as specified by the name.
    /// </summary>
    /// <value></value>
    public bool IsSpecialName {
      get { return (this.flags & 0x02000000) != 0; }
      set {
        if (value)
          this.flags |= 0x02000000;
        else
          this.flags &= ~0x02000000;
      }
    }

    /// <summary>
    /// This field is static (shared by all instances of its declaring type).
    /// </summary>
    /// <value></value>
    public bool IsStatic {
      get { return (this.flags & 0x01000000) != 0; }
      set {
        if (value)
          this.flags |= 0x01000000;
        else
          this.flags &= ~0x01000000;
      }
    }

    /// <summary>
    /// Specifies how this field is marshalled when it is accessed from unmanaged code.
    /// </summary>
    /// <value></value>
    public IMarshallingInformation MarshallingInformation {
      get { return this.marshallingInformation; }
      set { this.marshallingInformation = value; }
    }
    IMarshallingInformation marshallingInformation;

    /// <summary>
    /// Offset of the field.
    /// </summary>
    /// <value></value>
    public uint Offset {
      get { return this.offset; }
      set { this.offset = value; }
    }
    uint offset;

    /// <summary>
    /// The position of the field starting from 0 within the class.
    /// </summary>
    /// <value></value>
    public int SequenceNumber {
      get { return this.sequenceNumber; }
      set { this.sequenceNumber = value; }
    }
    int sequenceNumber;

    /// <summary>
    /// The type of value that is stored in this field.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IFieldReference Members

    IEnumerable<ICustomModifier> IFieldReference.CustomModifiers {
      get {
        if (this.customModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.customModifiers.AsReadOnly();
      }
    }

    /// <summary>
    /// The Field being referred to.
    /// </summary>
    /// <value></value>
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

  /// <summary>
  /// A reference to a field.
  /// </summary>
  public class FieldReference : IFieldReference, ICopyFrom<IFieldReference> {

    /// <summary>
    /// A reference to a field.
    /// </summary>
    public FieldReference() {
      Contract.Ensures(!this.IsFrozen);
      this.containingType = Dummy.TypeReference;
      this.internFactory = Dummy.InternFactory;
      this.name = Dummy.Name;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of a reference to a field.
    /// </summary>
    /// <param name="fieldReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IFieldReference fieldReference, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(fieldReference.Attributes))
        this.attributes = new List<ICustomAttribute>(fieldReference.Attributes);
      else
        this.attributes = null;
      this.containingType = fieldReference.ContainingType;
      if (fieldReference.IsModified)
        this.customModifiers = new List<ICustomModifier>(fieldReference.CustomModifiers);
      else
        this.customModifiers = null;
      this.isStatic = fieldReference.IsStatic;
      this.internFactory = internFactory;
      if (IteratorHelper.EnumerableIsNotEmpty(fieldReference.Locations))
        this.locations = new List<ILocation>(fieldReference.Locations);
      else
        this.locations = null;
      this.name = fieldReference.Name;
      this.type = fieldReference.Type;
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
    /// A reference to the containing type of the referenced type member.
    /// </summary>
    /// <value></value>
    public ITypeReference ContainingType {
      get { return this.containingType; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.containingType = value;
      }
    }
    ITypeReference containingType;

    /// <summary>
    /// Custom modifiers associated with the referenced field. May be null.
    /// </summary>
    public List<ICustomModifier>/*?*/ CustomModifiers {
      get { return this.customModifiers; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.customModifiers = value;
      }
    }
    List<ICustomModifier>/*?*/ customModifiers;

    /// <summary>
    /// Calls visitor.Visit(IFieldReference).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    /// <summary>
    /// Calls visitor.Visit(IFieldReference).
    /// </summary>
    public virtual void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    public IInternFactory InternFactory {
      get { return this.internFactory; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.internFactory = value;
      }
    }
    IInternFactory internFactory;

    /// <summary>
    /// Returns a key that is computed from the information in this reference and that distinguishes
    /// this.ResolvedField from all other fields obtained from the same metadata host.
    /// </summary>
    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.internedKey = this.InternFactory.GetFieldInternedKey(this);
          this.isFrozen = true;
        }
        return this.internedKey;
      }
    }
    uint internedKey;

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
    bool isFrozen;

    /// <summary>
    /// The referenced field has custom modifiers.
    /// </summary>
    public bool IsModified {
      get { return this.CustomModifiers != null && this.CustomModifiers.Count > 0; }
    }

    /// <summary>
    /// This field is static (shared by all instances of its declaring type).
    /// </summary>
    /// <value></value>
    public bool IsStatic {
      get { return this.isStatic; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.isStatic = value;
      }
    }
    bool isStatic;

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
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.name = value;
      }
    }
    IName name;

    /// <summary>
    /// The Field being referred to.
    /// </summary>
    /// <value></value>
    public IFieldDefinition ResolvedField {
      get {
        if (this.resolvedField == null) {
          this.resolvedField = TypeHelper.GetField(this.ContainingType.ResolvedType, this, true);
          this.isFrozen = true;
        }
        return this.resolvedField;
      }
    }
    IFieldDefinition/*?*/ resolvedField;

    /// <summary>
    /// The type definition member this reference resolves to.
    /// </summary>
    /// <value></value>
    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this.ResolvedField; }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() {
      return MemberHelper.GetMemberSignature(this, NameFormattingOptions.None);
    }

    /// <summary>
    /// The type of value that is stored in this field.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.type = value;
      }
    }
    ITypeReference type;

    IEnumerable<ICustomModifier> IFieldReference.CustomModifiers {
      get {
        if (this.customModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.CustomModifiers.AsReadOnly();
      }
    }

    #region IReference Members

    IEnumerable<ICustomAttribute> IReference.Attributes {
      get {
        if (this.attributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.attributes.AsReadOnly();
      }
    }

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      get {
        if (this.locations == null) return Enumerable<ILocation>.Empty;
        return this.locations.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class GlobalFieldDefinition : FieldDefinition, IGlobalFieldDefinition, ICopyFrom<IGlobalFieldDefinition> {

    /// <summary>
    /// 
    /// </summary>
    public GlobalFieldDefinition() {
      this.containingNamespace = Dummy.NamespaceDefinition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalFieldDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IGlobalFieldDefinition globalFieldDefinition, IInternFactory internFactory) {
      ((ICopyFrom<IFieldDefinition>)this).Copy(globalFieldDefinition, internFactory);
      this.containingNamespace = globalFieldDefinition.ContainingNamespace;
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
    /// 
    /// </summary>
    /// <param name="visitor"></param>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #region IContainerMember<INamespaceDefinition> Members

    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ContainingNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ContainingNamespace; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class GlobalMethodDefinition : MethodDefinition, IGlobalMethodDefinition, ICopyFrom<IGlobalMethodDefinition> {

    /// <summary>
    /// 
    /// </summary>
    public GlobalMethodDefinition() {
      this.containingNamespace = Dummy.NamespaceDefinition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalMethodDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IGlobalMethodDefinition globalMethodDefinition, IInternFactory internFactory) {
      ((ICopyFrom<IMethodDefinition>)this).Copy(globalMethodDefinition, internFactory);
      this.containingNamespace = globalMethodDefinition.ContainingNamespace;
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
    /// 
    /// </summary>
    /// <param name="visitor"></param>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #region IContainerMember<INamespaceDefinition> Members

    INamespaceDefinition IContainerMember<INamespaceDefinition>.Container {
      get { return this.ContainingNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    IScope<INamespaceMember> IScopeMember<IScope<INamespaceMember>>.ContainingScope {
      get { return this.ContainingNamespace; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class GenericMethodInstanceReference : MethodReference, IGenericMethodInstanceReference, ICopyFrom<IGenericMethodInstanceReference> {

    /// <summary>
    /// 
    /// </summary>
    public GenericMethodInstanceReference() {
      this.genericArguments = null;
      this.genericMethod = Dummy.MethodReference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IGenericMethodInstanceReference genericMethodInstanceReference, IInternFactory internFactory) {
      ((ICopyFrom<IMethodReference>)this).Copy(genericMethodInstanceReference, internFactory);
      this.genericArguments = new List<ITypeReference>(genericMethodInstanceReference.GenericArguments);
      this.genericMethod = genericMethodInstanceReference.GenericMethod;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="visitor"></param>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The type arguments that were used to instantiate this.GenericMethod in order to create this method. May be null.
    /// </summary>
    /// <value></value>
    public List<ITypeReference>/*?*/ GenericArguments {
      get { return this.genericArguments; }
      set { this.genericArguments = value; }
    }
    List<ITypeReference>/*?*/ genericArguments;

    /// <summary>
    /// Returns the generic method of which this method is an instance.
    /// </summary>
    /// <value></value>
    public IMethodReference GenericMethod {
      get { return this.genericMethod; }
      set { this.genericMethod = value; }
    }
    IMethodReference genericMethod;

    /// <summary>
    /// Resolves the reference to find the method being referred to.
    /// </summary>
    protected override IMethodDefinition Resolve() {
      return new Immutable.GenericMethodInstance(this.GenericMethod.ResolvedMethod, ((IGenericMethodInstanceReference)this).GenericArguments, this.InternFactory);
    }

    #region IGenericMethodInstanceReference Members

    IEnumerable<ITypeReference> IGenericMethodInstanceReference.GenericArguments {
      get {
        if (this.genericArguments == null) return Enumerable<ITypeReference>.Empty;
        return this.genericArguments.AsReadOnly();
      }
    }

    #endregion

  }

  /// <summary>
  /// The definition of a type parameter of a generic method.
  /// </summary>
  public sealed class GenericMethodParameter : GenericParameter, IGenericMethodParameter, ICopyFrom<IGenericMethodParameter> {

    /// <summary>
    /// The definition of a type parameter of a generic method.
    /// </summary>
    public GenericMethodParameter() {
      this.definingMethod = Dummy.MethodDefinition;
    }

    /// <summary>
    /// Makes a shallow copy of the definition of a type parameter of a generic method.
    /// </summary>
    /// <param name="genericMehodParameter"></param>
    /// <param name="internFactory"></param>
    public void Copy(IGenericMethodParameter genericMehodParameter, IInternFactory internFactory) {
      ((ICopyFrom<IGenericParameter>)this).Copy(genericMehodParameter, internFactory);
      this.definingMethod = genericMehodParameter.DefiningMethod;
    }

    /// <summary>
    /// The generic method that defines this type parameter.
    /// </summary>
    /// <value></value>
    public IMethodDefinition DefiningMethod {
      get {
        return this.definingMethod;
      }
      set {
        this.definingMethod = value;
      }
    }
    IMethodDefinition definingMethod;

    /// <summary>
    /// Calls visitor.Visit(IGenericMethodParameter).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IGenericMethodParameterReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericMethodParameterReference)this);
    }

    #region IGenericMethodParameterReference Members

    IMethodReference IGenericMethodParameterReference.DefiningMethod {
      get { return this.DefiningMethod; }
    }

    IGenericMethodParameter IGenericMethodParameterReference.ResolvedType {
      get { return this; }
    }

    #endregion

  }

  /// <summary>
  /// A mutable object that represents a local variable or constant.
  /// </summary>
  public class LocalDefinition : ILocalDefinition, ICopyFrom<ILocalDefinition> {

    /// <summary>
    /// Allocates a mutable object that represents a local variable or constant.
    /// </summary>
    public LocalDefinition() {
      this.compileTimeValue = Dummy.Constant;
      this.customModifiers = null;
      this.isModified = false;
      this.isPinned = false;
      this.isReference = false;
      this.locations = null;
      this.methodDefinition = Dummy.MethodDefinition;
      this.name = Dummy.Name;
      this.type = Dummy.TypeReference;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.compileTimeValue != null);
      Contract.Invariant(this.methodDefinition != null);
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.type != null);
    }


    /// <summary>
    /// Makes this mutable local definition a copy of the given immutable local definition.
    /// </summary>
    /// <param name="localVariableDefinition">An immutable local definition.</param>
    /// <param name="internFactory">The intern factory to use for computing the interned identity (if applicable) of this mutable object.</param>
    public void Copy(ILocalDefinition localVariableDefinition, IInternFactory internFactory) {
      if (localVariableDefinition.IsConstant)
        this.compileTimeValue = localVariableDefinition.CompileTimeValue;
      else
        this.compileTimeValue = Dummy.Constant;
      if (localVariableDefinition.IsModified && IteratorHelper.EnumerableIsNotEmpty(localVariableDefinition.CustomModifiers))
        this.customModifiers = new List<ICustomModifier>(localVariableDefinition.CustomModifiers);
      else
        this.customModifiers = null;
      this.isModified = localVariableDefinition.IsModified;
      this.isPinned = localVariableDefinition.IsPinned;
      this.isReference = localVariableDefinition.IsReference;
      if (IteratorHelper.EnumerableIsNotEmpty(localVariableDefinition.Locations))
        this.locations = new List<ILocation>(localVariableDefinition.Locations);
      else
        this.locations = null;
      this.methodDefinition = localVariableDefinition.MethodDefinition;
      this.name = localVariableDefinition.Name;
      this.type = localVariableDefinition.Type;
    }

    /// <summary>
    /// Returns a shallow copy of this LocalDeclarationStatement.
    /// </summary>
    public virtual LocalDefinition Clone() {
      var newLocal = new LocalDefinition();
      newLocal.Copy(this, Dummy.InternFactory);
      return newLocal;
    }

    /// <summary>
    /// The compile time value of the definition, if it is a local constant.
    /// </summary>
    /// <value></value>
    public IMetadataConstant CompileTimeValue {
      get { return this.compileTimeValue; }
      set { this.compileTimeValue = value; }
    }
    IMetadataConstant compileTimeValue;

    /// <summary>
    /// Custom modifiers associated with local variable definition. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier>/*?*/ CustomModifiers {
      get { return this.customModifiers; }
      set { this.customModifiers = value; }
    }
    List<ICustomModifier>/*?*/ customModifiers;

    /// <summary>
    /// True if this local definition is readonly and initialized with a compile time constant value.
    /// </summary>
    /// <value></value>
    public bool IsConstant {
      get { return !(this.compileTimeValue is Dummy); }
    }

    /// <summary>
    /// The local variable has custom modifiers.
    /// </summary>
    /// <value></value>
    public bool IsModified {
      get { return this.isModified; }
      set { this.isModified = value; }
    }
    bool isModified;

    /// <summary>
    /// True if the value referenced by the local must not be moved by the actions of the garbage collector.
    /// </summary>
    /// <value></value>
    public bool IsPinned {
      get { return this.isPinned; }
      set { this.isPinned = value; }
    }
    bool isPinned;

    /// <summary>
    /// True if the local contains a managed pointer (for example a reference to a local variable or a reference to a field of an object).
    /// </summary>
    /// <value></value>
    public bool IsReference {
      get { return this.isReference; }
      set { this.isReference = value; }
    }
    bool isReference;

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
    /// A potentially empty collection of locations that correspond to this instance. May be null.
    /// </summary>
    /// <value></value>
    public List<ILocation>/*?*/ Locations {
      get { return this.locations; }
      set { this.locations = value; }
    }
    List<ILocation>/*?*/ locations;

    /// <summary>
    /// The definition of the method in which this local is defined.
    /// </summary>
    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
      set { this.methodDefinition = value; }
    }
    IMethodDefinition methodDefinition;

    /// <summary>
    /// Returns the Name property of the LocalDefinition.
    /// </summary>
    public override string ToString() {
      var x = this.Name.Value;
      return x == null ? ":no name" : x;
    }

    /// <summary>
    /// The type of the local.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region ILocalDefinition Members

    IEnumerable<ICustomModifier> ILocalDefinition.CustomModifiers {
      get {
        if (this.customModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.customModifiers.AsReadOnly();
      }
    }

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      get {
        if (this.locations == null) return Enumerable<ILocation>.Empty;
        return this.locations.AsReadOnly();
      }
    }

    #endregion

  }

  /// <summary>
  /// A metadata (IL) level represetation of the body of a method or of a property/event accessor.
  /// </summary>
  public class MethodBody : IMethodBody, ICopyFrom<IMethodBody> {

    /// <summary>
    /// A metadata (IL) level represetation of the body of a method or of a property/event accessor.
    /// </summary>
    public MethodBody() {
      this.localsAreZeroed = true;
      this.localVariables = null;
      this.maxStack = 0;
      this.methodDefinition = Dummy.MethodDefinition;
      this.operationExceptionInformation = null;
      this.operations = null;
      this.privateHelperTypes = null;
    }

    /// <summary>
    /// Makes a shallow copy of a metadata (IL) level represetation of the body of a method or of a property/event accessor.
    /// </summary>
    /// <param name="methodBody"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMethodBody methodBody, IInternFactory internFactory) {
      this.localsAreZeroed = methodBody.LocalsAreZeroed;
      if (IteratorHelper.EnumerableIsNotEmpty(methodBody.LocalVariables))
        this.localVariables = new List<ILocalDefinition>(methodBody.LocalVariables);
      else
        this.localVariables = null;
      this.maxStack = methodBody.MaxStack;
      this.methodDefinition = methodBody.MethodDefinition;
      if (!methodBody.MethodDefinition.IsAbstract && !methodBody.MethodDefinition.IsExternal && methodBody.MethodDefinition.IsCil)
        this.operationExceptionInformation = new List<IOperationExceptionInformation>(methodBody.OperationExceptionInformation);
      else
        this.operationExceptionInformation = null;
      if (!methodBody.MethodDefinition.IsAbstract && !methodBody.MethodDefinition.IsExternal && methodBody.MethodDefinition.IsCil)
        this.operations = new List<IOperation>(methodBody.Operations);
      else
        this.operations = null;
      if (IteratorHelper.EnumerableIsNotEmpty(methodBody.PrivateHelperTypes))
        this.privateHelperTypes = new List<ITypeDefinition>(methodBody.PrivateHelperTypes);
      else
        this.privateHelperTypes = null;
      this.size = methodBody.Size;
    }

    /// <summary>
    /// Calls visitor.Visit(IMethodBody).
    /// </summary>
    /// <param name="visitor"></param>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// True if the locals are initialized by zeroeing the stack upon method entry.
    /// </summary>
    /// <value></value>
    public bool LocalsAreZeroed {
      get { return this.localsAreZeroed; }
      set { this.localsAreZeroed = value; }
    }
    bool localsAreZeroed;

    /// <summary>
    /// The local variables of the method. May be null.
    /// </summary>
    public List<ILocalDefinition>/*?*/ LocalVariables {
      get { return this.localVariables; }
      set { this.localVariables = value; }
    }
    List<ILocalDefinition>/*?*/ localVariables;

    /// <summary>
    /// The maximum number of elements on the evaluation stack during the execution of the method.
    /// </summary>
    public ushort MaxStack {
      get { return this.maxStack; }
      set { this.maxStack = value; }
    }
    ushort maxStack;

    /// <summary>
    /// The definition of the method whose body this is.
    /// If this is the body of an event or property accessor, this will hold the corresponding adder/remover/setter or getter method.
    /// </summary>
    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
      set { this.methodDefinition = value; }
    }
    IMethodDefinition methodDefinition;

    /// <summary>
    /// A list CLR IL operations that implement this method body. May be null.
    /// </summary>
    public List<IOperation>/*?*/ Operations {
      get { return this.operations; }
      set { this.operations = value; }
    }
    List<IOperation>/*?*/ operations;

    /// <summary>
    /// A list exception data within the method body IL. May be null.
    /// </summary>
    public List<IOperationExceptionInformation>/*?*/ OperationExceptionInformation {
      get { return this.operationExceptionInformation; }
      set { this.operationExceptionInformation = value; }
    }
    List<IOperationExceptionInformation>/*?*/ operationExceptionInformation;

    /// <summary>
    /// Any types that are implicitly defined in order to implement the body semantics.
    /// In case of AST to instructions conversion this lists the types produced.
    /// In case of instructions to AST decompilation this should ideally be list of all types
    /// which are local to method. May be null.
    /// </summary>
    public List<ITypeDefinition>/*?*/ PrivateHelperTypes {
      get { return this.privateHelperTypes; }
      set { this.privateHelperTypes = value; }
    }
    List<ITypeDefinition>/*?*/ privateHelperTypes;

    /// <summary>
    /// The size in bytes of the method body when serialized.
    /// </summary>
    public uint Size {
      get { return this.size; }
      set { this.size = value; }
    }
    uint size;

    #region IMethodBody Members

    IEnumerable<IOperationExceptionInformation> IMethodBody.OperationExceptionInformation {
      get {
        if (this.operationExceptionInformation == null) return Enumerable<IOperationExceptionInformation>.Empty;
        return this.operationExceptionInformation.AsReadOnly();
      }
    }

    IEnumerable<ILocalDefinition> IMethodBody.LocalVariables {
      get {
        if (this.localVariables == null) return Enumerable<ILocalDefinition>.Empty;
        return this.localVariables.AsReadOnly();
      }
    }

    IEnumerable<IOperation> IMethodBody.Operations {
      get {
        if (this.operations == null) return Enumerable<IOperation>.Empty;
        return this.operations.AsReadOnly();
      }
    }

    IEnumerable<ITypeDefinition> IMethodBody.PrivateHelperTypes {
      get {
        if (this.privateHelperTypes == null) return Enumerable<ITypeDefinition>.Empty;
        return this.privateHelperTypes.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// The metadata representation of a method.
  /// </summary>
  public class MethodDefinition : TypeDefinitionMember, IMethodDefinition, ICopyFrom<IMethodDefinition> {

    /// <summary>
    /// The metadata representation of a method.
    /// </summary>
    public MethodDefinition() {
      this.body = Dummy.MethodBody;
      this.callingConvention = CallingConvention.Default;
      this.genericParameters = null;
      this.internFactory = Dummy.InternFactory;
      this.parameters = null;
      this.platformInvokeData = Dummy.PlatformInvokeInformation;
      this.returnValueAttributes = new List<ICustomAttribute>();
      this.returnValueCustomModifiers = new List<ICustomModifier>();
      this.returnValueMarshallingInformation = Dummy.MarshallingInformation;
      this.returnValueName = Dummy.Name;
      this.securityAttributes = null;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of the metadata representation of a method.
    /// </summary>
    /// <param name="methodDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMethodDefinition methodDefinition, IInternFactory internFactory) {
      ((ICopyFrom<ITypeDefinitionMember>)this).Copy(methodDefinition, internFactory);
      if (!methodDefinition.IsAbstract && !methodDefinition.IsExternal)
        this.body = methodDefinition.Body;
      else
        this.body = Dummy.MethodBody;
      this.callingConvention = methodDefinition.CallingConvention;
      if (methodDefinition.IsGeneric)
        this.genericParameters = new List<IGenericMethodParameter>(methodDefinition.GenericParameters);
      else
        this.genericParameters = null;
      this.internFactory = internFactory;
      if (methodDefinition.ParameterCount > 0)
        this.parameters = new List<IParameterDefinition>(methodDefinition.Parameters);
      else
        this.parameters = null;
      if (methodDefinition.IsPlatformInvoke)
        this.platformInvokeData = methodDefinition.PlatformInvokeData;
      else
        this.platformInvokeData = Dummy.PlatformInvokeInformation;
      this.returnValueAttributes = new List<ICustomAttribute>(methodDefinition.ReturnValueAttributes);
      if (methodDefinition.ReturnValueIsModified)
        this.returnValueCustomModifiers = new List<ICustomModifier>(methodDefinition.ReturnValueCustomModifiers);
      else
        this.returnValueCustomModifiers = new List<ICustomModifier>(0);
      if (methodDefinition.ReturnValueIsMarshalledExplicitly)
        this.returnValueMarshallingInformation = methodDefinition.ReturnValueMarshallingInformation;
      else
        this.returnValueMarshallingInformation = Dummy.MarshallingInformation;
      if (methodDefinition.HasDeclarativeSecurity && IteratorHelper.EnumerableIsNotEmpty(methodDefinition.SecurityAttributes))
        this.securityAttributes = new List<ISecurityAttribute>(methodDefinition.SecurityAttributes);
      else
        this.securityAttributes = null;
      this.type = methodDefinition.Type;
      //^ base;
      this.AcceptsExtraArguments = methodDefinition.AcceptsExtraArguments;
      this.HasDeclarativeSecurity = methodDefinition.HasDeclarativeSecurity;
      this.HasExplicitThisParameter = methodDefinition.HasExplicitThisParameter;
      this.IsAbstract = methodDefinition.IsAbstract;
      this.IsAccessCheckedOnOverride = methodDefinition.IsAccessCheckedOnOverride;
      this.IsCil = methodDefinition.IsCil;
      this.IsExternal = methodDefinition.IsExternal;
      this.IsForwardReference = methodDefinition.IsForwardReference;
      this.IsHiddenBySignature = methodDefinition.IsHiddenBySignature;
      this.IsNativeCode = methodDefinition.IsNativeCode;
      this.IsNewSlot = methodDefinition.IsNewSlot;
      this.IsNeverInlined = methodDefinition.IsNeverInlined;
      this.IsAggressivelyInlined = methodDefinition.IsAggressivelyInlined;
      this.IsNeverOptimized = methodDefinition.IsNeverOptimized;
      this.IsPlatformInvoke = methodDefinition.IsPlatformInvoke;
      this.IsRuntimeImplemented = methodDefinition.IsRuntimeImplemented;
      this.IsRuntimeInternal = methodDefinition.IsRuntimeInternal;
      this.IsRuntimeSpecial = methodDefinition.IsRuntimeSpecial;
      this.IsSealed = methodDefinition.IsSealed;
      this.IsSpecialName = methodDefinition.IsSpecialName;
      this.IsStatic = methodDefinition.IsStatic;
      this.IsSynchronized = methodDefinition.IsSynchronized;
      this.IsUnmanaged = methodDefinition.IsUnmanaged;
      if (this.IsStatic)
        this.IsVirtual = false;
      else
        this.IsVirtual = methodDefinition.IsVirtual;
      this.PreserveSignature = methodDefinition.PreserveSignature;
      this.RequiresSecurityObject = methodDefinition.RequiresSecurityObject;
      this.ReturnValueIsByRef = methodDefinition.ReturnValueIsByRef;
      this.ReturnValueIsMarshalledExplicitly = methodDefinition.ReturnValueIsMarshalledExplicitly;
      this.returnValueName = methodDefinition.ReturnValueName;
    }

    /// <summary>
    /// True if the call sites that references the method with this object supply extra arguments.
    /// </summary>
    /// <value></value>
    public bool AcceptsExtraArguments {
      get { return (this.flags & 0x40000000) != 0; }
      set {
        if (value)
          this.flags |= 0x40000000;
        else
          this.flags &= ~0x40000000;
      }
    }

    /// <summary>
    /// A container for a list of IL instructions providing the implementation (if any) of this method.
    /// </summary>
    /// <value></value>
    public IMethodBody Body {
      get { return this.body; }
      set { this.body = value; }
    }
    IMethodBody body;

    /// <summary>
    /// Calling convention of the signature.
    /// </summary>
    /// <value></value>
    public CallingConvention CallingConvention {
      get { return this.callingConvention; }
      set { this.callingConvention = value; }
    }
    CallingConvention callingConvention;

    /// <summary>
    /// Calls visitor.Visit(IMethodDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IMethodReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IMethodReference)this);
    }

    /// <summary>
    /// If the method is generic then this list contains the type parameters. May be null.
    /// </summary>
    /// <value></value>
    public List<IGenericMethodParameter>/*?*/ GenericParameters {
      get { return this.genericParameters; }
      set { this.genericParameters = value; }
    }
    List<IGenericMethodParameter>/*?*/ genericParameters;

    //^ [Pure]
    /// <summary>
    /// The number of generic parameters of the method. Zero if the referenced method is not generic.
    /// </summary>
    /// <value></value>
    public ushort GenericParameterCount {
      get {
        if (this.genericParameters == null) return 0;
        return (ushort)this.genericParameters.Count;
      }
    }

    /// <summary>
    /// True if this method has a non empty collection of SecurityAttributes or the System.Security.SuppressUnmanagedCodeSecurityAttribute.
    /// </summary>
    /// <value></value>
    public bool HasDeclarativeSecurity {
      get { return (this.flags & 0x20000000) != 0; }
      set {
        if (value)
          this.flags |= 0x20000000;
        else
          this.flags &= ~0x20000000;
      }
    }

    /// <summary>
    /// True if this an instance method that explicitly declares the type and name of its first parameter (the instance).
    /// </summary>
    /// <value></value>
    public bool HasExplicitThisParameter {
      get { return (this.flags & 0x10000000) != 0; }
      set {
        if (value)
          this.flags |= 0x10000000;
        else
          this.flags &= ~0x10000000;
      }
    }

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

    /// <summary>
    /// Returns a key that is computed from the information in this reference and that distinguishes
    /// this.ResolvedMethod from all other methods obtained from the same metadata host.
    /// </summary>
    public uint InternedKey {
      get {
        if (this.internedKey == 0)
          this.internedKey = this.InternFactory.GetMethodInternedKey(this);
        return this.internedKey;
      }
    }
    uint internedKey;

    /// <summary>
    /// True if the method does not provide an implementation.
    /// </summary>
    /// <value></value>
    public bool IsAbstract {
      get { return (this.flags & 0x08000000) != 0; }
      set {
        if (value)
          this.flags |= 0x08000000;
        else
          this.flags &= ~0x08000000;
      }
    }

    /// <summary>
    /// True if the method can only be overridden when it is also accessible.
    /// </summary>
    /// <value></value>
    public bool IsAccessCheckedOnOverride {
      get { return (this.flags & 0x04000000) != 0; }
      set {
        if (value)
          this.flags |= 0x04000000;
        else
          this.flags &= ~0x04000000;
      }
    }

    /// <summary>
    /// True if the the runtime is asked to inline this method.
    /// </summary>
    /// <value></value>
    public bool IsAggressivelyInlined {
      get { return (this.flags & 0x02000000) != 0; }
      set {
        if (value)
          this.flags |= 0x02000000;
        else
          this.flags &= ~0x02000000;
      }
    }

    /// <summary>
    /// True if the method is implemented in the CLI Common Intermediate Language.
    /// </summary>
    /// <value></value>
    public bool IsCil {
      get { return (this.flags & 0x01000000) != 0; }
      set {
        if (value)
          this.flags |= 0x01000000;
        else
          this.flags &= ~0x01000000;
      }
    }

    /// <summary>
    /// True if the method is a constructor.
    /// </summary>
    /// <value></value>
    public bool IsConstructor {
      get { return this.IsRuntimeSpecial && this.Name.Value.Equals(".ctor"); }
    }

    /// <summary>
    /// True if the method has an external implementation (i.e. not supplied by this definition).
    /// </summary>
    /// <value></value>
    public bool IsExternal {
      get { return (this.flags & 0x00800000) != 0; }
      set {
        if (value)
          this.flags |= 0x00800000;
        else
          this.flags &= ~0x00800000;
      }
    }

    /// <summary>
    /// True if the method implementation is defined by another method definition (to be supplied at a later time).
    /// </summary>
    /// <value></value>
    public bool IsForwardReference {
      get { return (this.flags & 0x00400000) != 0; }
      set {
        if (value)
          this.flags |= 0x00400000;
        else
          this.flags &= ~0x00400000;
      }
    }

    /// <summary>
    /// True if the method has generic parameters;
    /// </summary>
    /// <value></value>
    public bool IsGeneric {
      get { return this.genericParameters != null && this.genericParameters.Count > 0; }
    }

    /// <summary>
    /// True if this method is hidden if a derived type declares a method with the same name and signature.
    /// If false, any method with the same name hides this method. This flag is ignored by the runtime and is only used by compilers.
    /// </summary>
    /// <value></value>
    public bool IsHiddenBySignature {
      get { return (this.flags & 0x00200000) != 0; }
      set {
        if (value)
          this.flags |= 0x00200000;
        else
          this.flags &= ~0x00200000;
      }
    }

    /// <summary>
    /// True if the method is implemented in native (platform-specific) code.
    /// </summary>
    /// <value></value>
    public bool IsNativeCode {
      get { return (this.flags & 0x00100000) != 0; }
      set {
        if (value)
          this.flags |= 0x00100000;
        else
          this.flags &= ~0x00100000;
      }
    }

    /// <summary>
    /// The method always gets a new slot in the virtual method table.
    /// This means the method will hide (not override) a base type method with the same name and signature.
    /// </summary>
    /// <value></value>
    public bool IsNewSlot {
      get { return (this.flags & 0x00080000) != 0; }
      set {
        if (value)
          this.flags |= 0x00080000;
        else
          this.flags &= ~0x00080000;
      }
    }

    /// <summary>
    /// True if the the runtime is not allowed to inline this method.
    /// </summary>
    /// <value></value>
    public bool IsNeverInlined {
      get { return (this.flags & 0x00040000) != 0; }
      set {
        if (value)
          this.flags |= 0x00040000;
        else
          this.flags &= ~0x00040000;
      }
    }

    /// <summary>
    /// True if the runtime is not allowed to optimize this method.
    /// </summary>
    /// <value></value>
    public bool IsNeverOptimized {
      get { return (this.flags & 0x00020000) != 0; }
      set {
        if (value)
          this.flags |= 0x00020000;
        else
          this.flags &= ~0x00020000;
      }
    }

    /// <summary>
    /// True if the method is implemented via the invocation of an underlying platform method.
    /// </summary>
    /// <value></value>
    public bool IsPlatformInvoke {
      get { return (this.flags & 0x00010000) != 0; }
      set {
        if (value)
          this.flags |= 0x00010000;
        else
          this.flags &= ~0x00010000;
      }
    }

    /// <summary>
    /// True if the implementation of this method is supplied by the runtime.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeImplemented {
      get { return (this.flags & 0x00008000) != 0; }
      set {
        if (value)
          this.flags |= 0x00008000;
        else
          this.flags &= ~0x00008000;
      }
    }

    /// <summary>
    /// True if the method is an internal part of the runtime and must be called in a special way.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeInternal {
      get { return (this.flags & 0x00004000) != 0; }
      set {
        if (value)
          this.flags |= 0x00004000;
        else
          this.flags &= ~0x00004000;
      }
    }

    /// <summary>
    /// True if the method gets special treatment from the runtime. For example, it might be a constructor.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeSpecial {
      get { return (this.flags & 0x00002000) != 0; }
      set {
        if (value)
          this.flags |= 0x00002000;
        else
          this.flags &= ~0x00002000;
      }
    }

    /// <summary>
    /// True if the method may not be overridden.
    /// </summary>
    /// <value></value>
    public bool IsSealed {
      get { return (this.flags & 0x00001000) != 0; }
      set {
        if (value)
          this.flags |= 0x00001000;
        else
          this.flags &= ~0x00001000;
      }
    }

    /// <summary>
    /// True if the method is special in some way for tools. For example, it might be a property getter or setter.
    /// </summary>
    /// <value></value>
    public bool IsSpecialName {
      get { return (this.flags & 0x00000800) != 0; }
      set {
        if (value)
          this.flags |= 0x00000800;
        else
          this.flags &= ~0x00000800;
      }
    }

    /// <summary>
    /// True if the method does not require an instance of its declaring type as its first argument.
    /// </summary>
    /// <value></value>
    public bool IsStatic {
      get { return (this.flags & 0x00000400) != 0; }
      set {
        if (value)
          this.flags |= 0x00000400;
        else
          this.flags &= ~0x00000400;
      }
    }

    /// <summary>
    /// True if the method is a static constructor.
    /// </summary>
    /// <value></value>
    public bool IsStaticConstructor {
      get { return this.IsSpecialName && this.Name.Value.Equals(".cctor"); }
    }

    /// <summary>
    /// True if only one thread at a time may execute this method.
    /// </summary>
    /// <value></value>
    public bool IsSynchronized {
      get { return (this.flags & 0x00000200) != 0; }
      set {
        if (value)
          this.flags |= 0x00000200;
        else
          this.flags &= ~0x00000200;
      }
    }

    /// <summary>
    /// True if the implementation of this method is not managed by the runtime.
    /// </summary>
    /// <value></value>
    public bool IsUnmanaged {
      get { return (this.flags & 0x00000100) != 0; }
      set {
        if (value)
          this.flags |= 0x00000100;
        else
          this.flags &= ~0x00000100;
      }
    }

    /// <summary>
    /// True if the method may be overridden (or if it is an override).
    /// </summary>
    /// <value></value>
    public bool IsVirtual {
      get {
        //^ assume !this.IsStatic;
        return (this.flags & 0x00000080) != 0;
      }
      set
        //^ requires value ==> !this.IsStatic;
      {
        if (value)
          this.flags |= 0x00000080;
        else
          this.flags &= ~0x00000080;
      }
    }

    /// <summary>
    /// The parameters forming part of this signature. May be null.
    /// </summary>
    /// <value></value>
    public List<IParameterDefinition>/*?*/ Parameters {
      get { return this.parameters; }
      set { this.parameters = value; }
    }
    List<IParameterDefinition>/*?*/ parameters;

    /// <summary>
    /// The number of required parameters of the method.
    /// </summary>
    /// <value></value>
    public ushort ParameterCount {
      get {
        if (this.parameters == null) return 0;
        return (ushort)this.parameters.Count;
      }
    }

    /// <summary>
    /// Detailed information about the PInvoke stub. Identifies which method to call, which module has the method and the calling convention among other things.
    /// </summary>
    /// <value></value>
    public IPlatformInvokeInformation PlatformInvokeData {
      get { return this.platformInvokeData; }
      set { this.platformInvokeData = value; }
    }
    IPlatformInvokeInformation platformInvokeData;

    /// <summary>
    /// True if the method signature must not be mangled during the interoperation with COM code.
    /// </summary>
    /// <value></value>
    public bool PreserveSignature {
      get { return (this.flags & 0x00000040) != 0; }
      set {
        if (value)
          this.flags |= 0x00000040;
        else
          this.flags &= ~0x00000040;
      }
    }

    /// <summary>
    /// True if the method calls another method containing security code. If this flag is set, the method
    /// should have System.Security.DynamicSecurityMethodAttribute present in its list of custom attributes.
    /// </summary>
    /// <value></value>
    public bool RequiresSecurityObject {
      get { return (this.flags & 0x00000020) != 0; }
      set {
        if (value)
          this.flags |= 0x00000020;
        else
          this.flags &= ~0x00000020;
      }
    }

    /// <summary>
    /// Custom attributes associated with the method's return value.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute> ReturnValueAttributes {
      get { return this.returnValueAttributes; }
      set { this.returnValueAttributes = value; }
    }
    List<ICustomAttribute> returnValueAttributes;

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the returned value. Evaluate this property only if ReturnValueIsModified is true.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier> ReturnValueCustomModifiers {
      get { return this.returnValueCustomModifiers; }
      set { this.returnValueCustomModifiers = value; }
    }
    List<ICustomModifier> returnValueCustomModifiers;

    /// <summary>
    /// True if the return value is passed by reference (using a managed pointer).
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsByRef {
      get { return (this.flags & 0x00000010) != 0; }
      set {
        if (value)
          this.flags |= 0x00000010;
        else
          this.flags &= ~0x00000010;
      }
    }

    /// <summary>
    /// The return value has associated marshalling information.
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsMarshalledExplicitly {
      get { return (this.flags & int.MinValue) != 0; }
      set {
        if (value)
          this.flags |= int.MinValue;
        else
          this.flags &= ~int.MinValue;
      }
    }

    /// <summary>
    /// True if the return value has one or more custom modifiers associated with it.
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsModified {
      get { return this.returnValueCustomModifiers.Count > 0; }
    }

    /// <summary>
    /// Specifies how the return value is marshalled when the method is called from unmanaged code.
    /// </summary>
    /// <value></value>
    public IMarshallingInformation ReturnValueMarshallingInformation {
      get { return this.returnValueMarshallingInformation; }
      set { this.returnValueMarshallingInformation = value; }
    }
    IMarshallingInformation returnValueMarshallingInformation;

    /// <summary>
    /// Specifies the name of the return value parameter.
    /// </summary>
    /// <value></value>
    public IName ReturnValueName {
      get { return this.returnValueName; }
      set { this.returnValueName = value; }
    }
    IName returnValueName;

    /// <summary>
    /// Declarative security actions for this method. May be null.
    /// </summary>
    /// <value></value>
    public List<ISecurityAttribute>/*?*/ SecurityAttributes {
      get { return this.securityAttributes; }
      set { this.securityAttributes = value; }
    }
    List<ISecurityAttribute>/*?*/ securityAttributes;

    /// <summary>
    /// The return type of the method or type of the property.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IMethodDefinition Members

    IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters {
      get {
        if (this.genericParameters == null) return Enumerable<IGenericMethodParameter>.Empty;
        return this.genericParameters.AsReadOnly();
      }
    }

    IEnumerable<ISecurityAttribute> IMethodDefinition.SecurityAttributes {
      get {
        if (this.securityAttributes == null) return Enumerable<ISecurityAttribute>.Empty;
        return this.securityAttributes.AsReadOnly();
      }
    }

    #endregion

    #region ISignature Members

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get {
        if (this.parameters == null) return Enumerable<IParameterTypeInformation>.Empty;
        return IteratorHelper.GetConversionEnumerable<IParameterDefinition, IParameterTypeInformation>(this.parameters);
      }
    }

    IEnumerable<ICustomModifier> ISignature.ReturnValueCustomModifiers {
      get { return this.returnValueCustomModifiers.AsReadOnly(); }
    }

    #endregion

    #region IMethodDefinition Members

    IEnumerable<IParameterDefinition> IMethodDefinition.Parameters {
      get {
        if (this.parameters == null) return Enumerable<IParameterDefinition>.Empty;
        return this.parameters.AsReadOnly();
      }
    }

    IEnumerable<ICustomAttribute> IMethodDefinition.ReturnValueAttributes {
      get { return this.returnValueAttributes.AsReadOnly(); }
    }

    #endregion

    #region IMethodReference Members

    /// <summary>
    /// The method being referred to.
    /// </summary>
    /// <value></value>
    public IMethodDefinition ResolvedMethod {
      get { return this; }
    }

    /// <summary>
    /// Information about this types of the extra arguments supplied at the call sites that references the method with this object.
    /// </summary>
    /// <value></value>
    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

  }

  /// <summary>
  /// A reference to a method.
  /// </summary>
  public class MethodReference : IMethodReference, ICopyFrom<IMethodReference> {

    /// <summary>
    /// A reference to a method.
    /// </summary>
    public MethodReference() {
      Contract.Ensures(!this.IsFrozen);
      this.attributes = null;
      this.callingConvention = (CallingConvention)0;
      this.containingType = Dummy.TypeReference;
      this.extraParameters = null;
      this.genericParameterCount = 0;
      this.internFactory = Dummy.InternFactory;
      this.locations = null;
      this.name = Dummy.Name;
      this.parameters = null;
      this.returnValueCustomModifiers = null;
      this.returnValueIsByRef = false;
      this.returnValueIsModified = false;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of a reference to a method.
    /// </summary>
    /// <param name="methodReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMethodReference methodReference, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(methodReference.Attributes))
        this.attributes = new List<ICustomAttribute>(methodReference.Attributes);
      else
        this.attributes = null;
      this.callingConvention = methodReference.CallingConvention;
      this.containingType = methodReference.ContainingType;
      if (methodReference.AcceptsExtraArguments)
        this.extraParameters = new List<IParameterTypeInformation>(methodReference.ExtraParameters);
      else
        this.extraParameters = null;
      this.genericParameterCount = methodReference.GenericParameterCount;
      this.internFactory = internFactory;
      if (IteratorHelper.EnumerableIsNotEmpty(methodReference.Locations))
        this.locations = new List<ILocation>(methodReference.Locations);
      else
        this.locations = null;
      this.name = methodReference.Name;
      if (methodReference.ParameterCount > 0)
        this.parameters = new List<IParameterTypeInformation>(methodReference.Parameters);
      else
        this.parameters = null;
      if (methodReference.ReturnValueIsModified)
        this.returnValueCustomModifiers = new List<ICustomModifier>(methodReference.ReturnValueCustomModifiers);
      else
        this.returnValueCustomModifiers = null;
      this.returnValueIsByRef = methodReference.ReturnValueIsByRef;
      this.returnValueIsModified = methodReference.ReturnValueIsModified;
      this.type = methodReference.Type;
    }

    /// <summary>
    /// True if the call sites that references the method with this object supply extra arguments.
    /// </summary>
    /// <value></value>
    public bool AcceptsExtraArguments {
      get { return (this.callingConvention & (CallingConvention)0x7) == CallingConvention.ExtraArguments; }
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
    /// A reference to the containing type of the referenced type member.
    /// </summary>
    /// <value></value>
    public ITypeReference ContainingType {
      get { return this.containingType; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.containingType = value;
      }
    }
    ITypeReference containingType;

    /// <summary>
    /// Calls visitor.Visit(IMethodReference).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      this.DispatchAsReference(visitor);
    }

    /// <summary>
    /// Calls visitor.Visit(IMethodReference).
    /// </summary>
    public virtual void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Information about this types of the extra arguments supplied at the call sites that references the method with this object. May be null.
    /// </summary>
    /// <value></value>
    public List<IParameterTypeInformation>/*?*/ ExtraParameters {
      get { return this.extraParameters; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.extraParameters = value;
      }
    }
    List<IParameterTypeInformation>/*?*/ extraParameters;

    /// <summary>
    /// The number of generic parameters of the method. Zero if the referenced method is not generic.
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
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    public IInternFactory InternFactory {
      get { return this.internFactory; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.internFactory = value;
      }
    }
    IInternFactory internFactory;

    /// <summary>
    /// Returns a key that is computed from the information in this reference and that distinguishes
    /// this.ResolvedMethod from all other methods obtained from the same metadata host.
    /// </summary>
    public uint InternedKey {
      get {
        if (this.internedKey == 0) {
          this.isFrozen = true;
          this.internedKey = this.InternFactory.GetMethodInternedKey(this);
        }
        return this.internedKey;
      }
    }
    uint internedKey;

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
    bool isFrozen;

    /// <summary>
    /// True if the method has generic parameters;
    /// </summary>
    /// <value></value>
    public bool IsGeneric {
      get { return this.genericParameterCount > 0; }
    }

    /// <summary>
    /// True if the referenced method does not require an instance of its declaring type as its first argument.
    /// </summary>
    public bool IsStatic {
      get { return (this.CallingConvention & CallingConvention.HasThis) == 0; }
    }

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
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.name = value;
      }
    }
    IName name;

    /// <summary>
    /// The parameters forming part of this signature. May be null.
    /// </summary>
    /// <value></value>
    public List<IParameterTypeInformation>/*?*/ Parameters {
      get { return this.parameters; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.parameters = value;
      }
    }
    List<IParameterTypeInformation>/*?*/ parameters;

    /// <summary>
    /// The number of required parameters of the method.
    /// </summary>
    /// <value></value>
    public ushort ParameterCount {
      get {
        if (this.Parameters == null) return 0;
        return (ushort)this.Parameters.Count;
      }
    }

    /// <summary>
    /// The method being referred to.
    /// </summary>
    /// <value></value>
    public IMethodDefinition ResolvedMethod {
      get {
        if (this.resolvedMethod == null) {
          this.isFrozen = true;
          this.resolvedMethod = this.Resolve();
        }
        return this.resolvedMethod;
      }
    }
    IMethodDefinition/*?*/ resolvedMethod;

    /// <summary>
    /// Resolves the reference to find the method being referred to.
    /// </summary>
    protected virtual IMethodDefinition Resolve() {
      return MemberHelper.ResolveMethod(this);
    }

    /// <summary>
    /// The type definition member this reference resolves to.
    /// </summary>
    /// <value></value>
    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this.ResolvedMethod; }
    }

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the returned value. Evaluate this property only if ReturnValueIsModified is true. May be null.
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
      get { return this.returnValueIsModified; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.returnValueIsModified = value;
      }
    }
    bool returnValueIsModified;

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() {
      return MemberHelper.GetMethodSignature(this,
        NameFormattingOptions.ReturnType|NameFormattingOptions.Signature|NameFormattingOptions.ParameterModifiers|NameFormattingOptions.ParameterName);
    }

    /// <summary>
    /// The return type of the method or type of the property.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set {
        Contract.Requires(!this.IsFrozen);
        this.type = value;
      }
    }
    ITypeReference type;


    #region IMethodReference Members


    IEnumerable<IParameterTypeInformation> IMethodReference.ExtraParameters {
      get {
        if (this.extraParameters == null) return Enumerable<IParameterTypeInformation>.Empty;
        return this.extraParameters.AsReadOnly();
      }
    }

    #endregion

    #region ISignature Members


    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get {
        if (this.Parameters == null) return Enumerable<IParameterTypeInformation>.Empty;
        return this.Parameters.AsReadOnly();
      }
    }

    IEnumerable<ICustomModifier> ISignature.ReturnValueCustomModifiers {
      get {
        if (this.ReturnValueCustomModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.ReturnValueCustomModifiers.AsReadOnly();
      }
    }

    #endregion

    #region IReference Members

    IEnumerable<ICustomAttribute> IReference.Attributes {
      get {
        if (this.attributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.attributes.AsReadOnly();
      }
    }

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      get {
        if (this.locations == null) return Enumerable<ILocation>.Empty;
        return this.locations.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// The metadata representation of a method or property parameter.
  /// </summary>
  public sealed class ParameterDefinition : IParameterDefinition, ICopyFrom<IParameterDefinition> {

    /// <summary>
    /// The metadata representation of a method or property parameter.
    /// </summary>
    public ParameterDefinition() {
      this.attributes = null;
      this.containingSignature = Dummy.Signature;
      this.customModifiers = null;
      this.defaultValue = Dummy.Constant;
      this.index = 0;
      this.locations = null;
      this.marshallingInformation = Dummy.MarshallingInformation;
      this.paramArrayElementType = Dummy.TypeReference;
      this.name = Dummy.Name;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of the metadata representation of a method or property parameter.
    /// </summary>
    /// <param name="parameterDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IParameterDefinition parameterDefinition, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(parameterDefinition.Attributes))
        this.attributes = new List<ICustomAttribute>(parameterDefinition.Attributes);
      else
        this.attributes = null;
      this.containingSignature = parameterDefinition.ContainingSignature;
      if (parameterDefinition.IsModified)
        this.customModifiers = new List<ICustomModifier>(parameterDefinition.CustomModifiers);
      else
        this.customModifiers = null;
      if (parameterDefinition.HasDefaultValue)
        this.defaultValue = parameterDefinition.DefaultValue;
      else
        this.defaultValue = Dummy.Constant;
      this.index = parameterDefinition.Index;
      if (IteratorHelper.EnumerableIsNotEmpty(parameterDefinition.Locations))
        this.locations = new List<ILocation>(parameterDefinition.Locations);
      else
        this.locations = null;
      if (parameterDefinition.IsMarshalledExplicitly)
        this.marshallingInformation = parameterDefinition.MarshallingInformation;
      else
        this.marshallingInformation = Dummy.MarshallingInformation;
      if (parameterDefinition.IsParameterArray)
        this.paramArrayElementType = parameterDefinition.ParamArrayElementType;
      else
        this.paramArrayElementType = Dummy.TypeReference;
      this.name = parameterDefinition.Name;
      this.type = parameterDefinition.Type;
      this.IsByReference = parameterDefinition.IsByReference;
      this.IsIn = parameterDefinition.IsIn;
      this.IsOptional = parameterDefinition.IsOptional;
      this.IsOut = parameterDefinition.IsOut;
    }

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
    /// The method or property that defines this parameter.
    /// </summary>
    /// <value></value>
    public ISignature ContainingSignature {
      get { return this.containingSignature; }
      set { this.containingSignature = value; }
    }
    ISignature containingSignature;

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the parameter. Evaluate this property only if IsModified is true. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier>/*?*/ CustomModifiers {
      get { return this.customModifiers; }
      set { this.customModifiers = value; }
    }
    List<ICustomModifier>/*?*/ customModifiers;

    /// <summary>
    /// A compile time constant value that should be supplied as the corresponding argument value by callers that do not explicitly specify an argument value for this parameter.
    /// </summary>
    /// <value></value>
    public IMetadataConstant DefaultValue {
      get { return this.defaultValue; }
      set { this.defaultValue = value; }
    }
    IMetadataConstant defaultValue;

    /// <summary>
    /// Calls visitor.Visit(IParameterDefinition).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.VisitReference(IParameterDefinition).
    /// </summary>
    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.VisitReference(this);
    }

    private int flags;

    /// <summary>
    /// True if the parameter has a default value that should be supplied as the argument value by a caller for which the argument value has not been explicitly specified.
    /// </summary>
    /// <value></value>
    public bool HasDefaultValue {
      get { return !(this.defaultValue is Dummy); }
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
    /// True if the parameter is passed by reference (using a managed pointer).
    /// </summary>
    /// <value></value>
    public bool IsByReference {
      get { return (this.flags & 0x40000000) != 0; }
      set {
        if (value)
          this.flags |= 0x40000000;
        else
          this.flags &= ~0x40000000;
      }
    }

    /// <summary>
    /// True if the argument value must be included in the marshalled arguments passed to a remote callee.
    /// </summary>
    /// <value></value>
    public bool IsIn {
      get { return (this.flags & 0x20000000) != 0; }
      set {
        if (value)
          this.flags |= 0x20000000;
        else
          this.flags &= ~0x20000000;
      }
    }

    /// <summary>
    /// This parameter has associated marshalling information.
    /// </summary>
    /// <value></value>
    public bool IsMarshalledExplicitly {
      get { return !(this.marshallingInformation is Dummy); }
    }

    /// <summary>
    /// This parameter has one or more custom modifiers associated with it.
    /// </summary>
    /// <value></value>
    public bool IsModified {
      get { return this.CustomModifiers != null && this.CustomModifiers.Count > 0; }
    }

    /// <summary>
    /// True if the argument value must be included in the marshalled arguments passed to a remote callee only if it is different from the default value (if there is one).
    /// </summary>
    /// <value></value>
    public bool IsOptional {
      get { return (this.flags & 0x10000000) != 0; }
      set {
        if (value)
          this.flags |= 0x10000000;
        else
          this.flags &= ~0x10000000;
      }
    }

    /// <summary>
    /// True if the final value assigned to the parameter will be marshalled with the return values passed back from a remote callee.
    /// </summary>
    /// <value></value>
    public bool IsOut {
      get { return (this.flags & 0x08000000) != 0; }
      set {
        if (value)
          this.flags |= 0x08000000;
        else
          this.flags &= ~0x08000000;
      }
    }

    /// <summary>
    /// True if the parameter has the ParamArrayAttribute custom attribute.
    /// </summary>
    /// <value></value>
    public bool IsParameterArray {
      get { return !(this.paramArrayElementType is Dummy); }
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
    /// Specifies how this parameter is marshalled when it is accessed from unmanaged code.
    /// </summary>
    /// <value></value>
    public IMarshallingInformation MarshallingInformation {
      get { return this.marshallingInformation; }
      set { this.marshallingInformation = value; }
    }
    IMarshallingInformation marshallingInformation;

    /// <summary>
    /// The element type of the parameter array.
    /// </summary>
    /// <value></value>
    public ITypeReference ParamArrayElementType {
      get { return this.paramArrayElementType; }
      set { this.paramArrayElementType = value; }
    }
    ITypeReference paramArrayElementType;

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
    /// The type of argument value that corresponds to this parameter.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IParameterDefinition Members

    IEnumerable<ICustomModifier> IParameterTypeInformation.CustomModifiers {
      get {
        if (this.CustomModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.CustomModifiers.AsReadOnly();
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

    #region IMetadataConstantContainer

    IMetadataConstant IMetadataConstantContainer.Constant {
      get { return this.DefaultValue; }
    }

    #endregion

    /// <summary>
    /// Returns the Name property of the ParameterDefinition.
    /// </summary>
    public override string ToString() {
      var x = this.Name.Value;
      return x == null ? ":no name" : x;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class ParameterTypeInformation : IParameterTypeInformation, ICopyFrom<IParameterTypeInformation> {

    /// <summary>
    /// 
    /// </summary>
    public ParameterTypeInformation() {
      this.containingSignature = Dummy.Signature;
      this.customModifiers = null;
      this.index = 0;
      this.isByReference = false;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterTypeInformation"></param>
    /// <param name="internFactory"></param>
    public void Copy(IParameterTypeInformation parameterTypeInformation, IInternFactory internFactory) {
      this.containingSignature = parameterTypeInformation.ContainingSignature;
      if (parameterTypeInformation.IsModified)
        this.customModifiers = new List<ICustomModifier>(parameterTypeInformation.CustomModifiers);
      else
        this.customModifiers = null;
      this.index = parameterTypeInformation.Index;
      this.isByReference = parameterTypeInformation.IsByReference;
      this.type = parameterTypeInformation.Type;
    }

    /// <summary>
    /// The method or property that defines this parameter.
    /// </summary>
    /// <value></value>
    public ISignature ContainingSignature {
      get { return this.containingSignature; }
      set { this.containingSignature = value; }
    }
    ISignature containingSignature;

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the parameter. Evaluate this property only if IsModified is true. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier>/*?*/ CustomModifiers {
      get { return this.customModifiers; }
      set { this.customModifiers = value; }
    }
    List<ICustomModifier>/*?*/ customModifiers;

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
    /// True if the parameter is passed by reference (using a managed pointer).
    /// </summary>
    /// <value></value>
    public bool IsByReference {
      get { return this.isByReference; }
      set { this.isByReference = value; }
    }
    bool isByReference;

    /// <summary>
    /// This parameter has one or more custom modifiers associated with it.
    /// </summary>
    /// <value></value>
    public bool IsModified {
      get { return this.CustomModifiers != null && this.CustomModifiers.Count > 0; }
    }

    /// <summary>
    /// The type of argument value that corresponds to this parameter.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IParameterTypeInformation Members

    IEnumerable<ICustomModifier> IParameterTypeInformation.CustomModifiers {
      get {
        if (this.CustomModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.CustomModifiers.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public class PropertyDefinition : TypeDefinitionMember, IPropertyDefinition, ICopyFrom<IPropertyDefinition> {

    /// <summary>
    /// 
    /// </summary>
    public PropertyDefinition() {
      this.accessors = null;
      this.callingConvention = CallingConvention.Default;
      this.defaultValue = Dummy.Constant;
      this.getter = null;
      this.parameters = null;
      this.returnValueCustomModifiers = null;
      this.setter = null;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(IPropertyDefinition propertyDefinition, IInternFactory internFactory) {
      ((ICopyFrom<ITypeDefinitionMember>)this).Copy(propertyDefinition, internFactory);
      if (IteratorHelper.EnumerableIsNotEmpty(propertyDefinition.Accessors))
        this.accessors = new List<IMethodReference>(propertyDefinition.Accessors);
      else
        this.accessors = null;
      this.callingConvention = propertyDefinition.CallingConvention;
      if (propertyDefinition.HasDefaultValue)
        this.defaultValue = propertyDefinition.DefaultValue;
      else
        this.defaultValue = Dummy.Constant;
      this.getter = propertyDefinition.Getter;
      if (IteratorHelper.EnumerableIsNotEmpty(propertyDefinition.Parameters))
        this.parameters = new List<IParameterDefinition>(propertyDefinition.Parameters);
      else
        this.parameters = null;
      if (propertyDefinition.ReturnValueIsModified)
        this.returnValueCustomModifiers = new List<ICustomModifier>(propertyDefinition.ReturnValueCustomModifiers);
      else
        this.returnValueCustomModifiers = null;
      this.setter = propertyDefinition.Setter;
      this.type = propertyDefinition.Type;
      //^ base;
      this.IsRuntimeSpecial = propertyDefinition.IsRuntimeSpecial;
      this.IsSpecialName = propertyDefinition.IsSpecialName;
      this.ReturnValueIsByRef = propertyDefinition.ReturnValueIsByRef;
    }

    /// <summary>
    /// A list of methods that are associated with the property. May be null.
    /// </summary>
    /// <value></value>
    public List<IMethodReference>/*?*/ Accessors {
      get { return this.accessors; }
      set { this.accessors = value; }
    }
    List<IMethodReference>/*?*/ accessors;

    /// <summary>
    /// Calling convention of the signature.
    /// </summary>
    /// <value></value>
    public CallingConvention CallingConvention {
      get { return this.callingConvention; }
      set { this.callingConvention = value; }
    }
    CallingConvention callingConvention;

    /// <summary>
    /// A compile time constant value that provides the default value for the property. (Who uses this and why?)
    /// </summary>
    /// <value></value>
    public IMetadataConstant DefaultValue {
      get { return this.defaultValue; }
      set { this.defaultValue = value; }
    }
    IMetadataConstant defaultValue;

    /// <summary>
    /// Calls visitor.Visit(IPropertyDefinition).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Throw an InvalidOperation exception since valid Metadata never references a property.
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// The method used to get the value of this property. May be absent (null).
    /// </summary>
    /// <value></value>
    public IMethodReference/*?*/ Getter {
      get { return this.getter; }
      set { this.getter = value; }
    }
    IMethodReference/*?*/ getter;

    /// <summary>
    /// True if this property has a compile time constant associated with that serves as a default value for the property. (Who uses this and why?)
    /// </summary>
    /// <value></value>
    public bool HasDefaultValue {
      get { return !(this.defaultValue is Dummy); }
    }

    /// <summary>
    /// True if this property gets special treatment from the runtime.
    /// </summary>
    /// <value></value>
    public bool IsRuntimeSpecial {
      get { return (this.flags & 0x40000000) != 0; }
      set {
        if (value)
          this.flags |= 0x40000000;
        else
          this.flags &= ~0x40000000;
      }
    }

    /// <summary>
    /// True if this property is special in some way, as specified by the name.
    /// </summary>
    /// <value></value>
    public bool IsSpecialName {
      get { return (this.flags & 0x20000000) != 0; }
      set {
        if (value)
          this.flags |= 0x20000000;
        else
          this.flags &= ~0x20000000;
      }
    }

    /// <summary>
    /// True if the referenced property does not require an instance of its declaring type as its first argument.
    /// </summary>
    public bool IsStatic {
      get { return (this.CallingConvention & CallingConvention.HasThis) == 0; }
    }

    /// <summary>
    /// The parameters forming part of this signature. May be null.
    /// </summary>
    /// <value></value>
    public List<IParameterDefinition>/*?*/ Parameters {
      get { return this.parameters; }
      set { this.parameters = value; }
    }
    List<IParameterDefinition>/*?*/ parameters;

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the returned value. Evaluate this property only if ReturnValueIsModified is true. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier>/*?*/ ReturnValueCustomModifiers {
      get { return this.returnValueCustomModifiers; }
      set { this.returnValueCustomModifiers = value; }
    }
    List<ICustomModifier>/*?*/ returnValueCustomModifiers;

    /// <summary>
    /// True if the return value is passed by reference (using a managed pointer).
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsByRef {
      get { return (this.flags & 0x10000000) != 0; }
      set {
        if (value)
          this.flags |= 0x10000000;
        else
          this.flags &= ~0x10000000;
      }
    }

    /// <summary>
    /// True if the return value has one or more custom modifiers associated with it.
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsModified {
      get { return this.ReturnValueCustomModifiers != null && this.ReturnValueCustomModifiers.Count > 0; }
    }

    /// <summary>
    /// The method used to set the value of this property. May be absent (null).
    /// </summary>
    /// <value></value>
    public IMethodReference/*?*/ Setter {
      get { return this.setter; }
      set { this.setter = value; }
    }
    IMethodReference/*?*/ setter;

    /// <summary>
    /// The return type of the method or type of the property.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IPropertyDefinition Members

    IEnumerable<IMethodReference> IPropertyDefinition.Accessors {
      get {
        if (this.Accessors == null) return Enumerable<IMethodReference>.Empty;
        return this.Accessors.AsReadOnly();
      }
    }

    IEnumerable<IParameterDefinition> IPropertyDefinition.Parameters {
      get {
        if (this.Parameters == null) return Enumerable<IParameterDefinition>.Empty;
        return this.Parameters.AsReadOnly();
      }
    }

    #endregion

    #region ISignature Members

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get {
        if (this.Parameters == null) return Enumerable<IParameterTypeInformation>.Empty;
        return IteratorHelper.GetConversionEnumerable<IParameterDefinition, IParameterTypeInformation>(this.Parameters);
      }
    }

    IEnumerable<ICustomModifier> ISignature.ReturnValueCustomModifiers {
      get {
        if (this.ReturnValueCustomModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.ReturnValueCustomModifiers.AsReadOnly();
      }
    }

    #endregion

    #region IMetadataConstantContainer

    IMetadataConstant IMetadataConstantContainer.Constant {
      get { return this.DefaultValue; }
    }

    #endregion
  }

  /// <summary>
  /// The parameters and return type that makes up a method or property signature.
  /// </summary>
  public sealed class SignatureDefinition : ISignature, ICopyFrom<ISignature> {

    /// <summary>
    /// The parameters and return type that makes up a method or property signature.
    /// </summary>
    public SignatureDefinition() {
      this.callingConvention = CallingConvention.Default;
      this.parameters = null;
      this.returnValueCustomModifiers = null;
      this.returnValueIsByRef = false;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of the parameters and return type that makes up a method or property signature.
    /// </summary>
    /// <param name="signatureDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISignature signatureDefinition, IInternFactory internFactory) {
      this.callingConvention = signatureDefinition.CallingConvention;
      if (IteratorHelper.EnumerableIsNotEmpty(signatureDefinition.Parameters))
        this.parameters = new List<IParameterTypeInformation>(signatureDefinition.Parameters);
      else
        this.parameters = null;
      if (signatureDefinition.ReturnValueIsModified)
        this.returnValueCustomModifiers = new List<ICustomModifier>(signatureDefinition.ReturnValueCustomModifiers);
      else
        this.returnValueCustomModifiers = null;
      this.returnValueIsByRef = signatureDefinition.ReturnValueIsByRef;
      this.type = signatureDefinition.Type;
    }

    /// <summary>
    /// Calling convention of the signature.
    /// </summary>
    /// <value></value>
    public CallingConvention CallingConvention {
      get { return this.callingConvention; }
      set { this.callingConvention = value; }
    }
    CallingConvention callingConvention;

    /// <summary>
    /// True if the referenced method or property does not require an instance of its declaring type as its first argument.
    /// </summary>
    public bool IsStatic {
      get { return (this.CallingConvention & CallingConvention.HasThis) == 0; }
    }

    /// <summary>
    /// The parameters forming part of this signature. May be null.
    /// </summary>
    /// <value></value>
    public List<IParameterTypeInformation>/*?*/ Parameters {
      get { return this.parameters; }
      set { this.parameters = value; }
    }
    List<IParameterTypeInformation>/*?*/ parameters;

    /// <summary>
    /// Returns the list of custom modifiers, if any, associated with the returned value. Evaluate this property only if ReturnValueIsModified is true. May be null.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier>/*?*/ ReturnValueCustomModifiers {
      get { return this.returnValueCustomModifiers; }
      set { this.returnValueCustomModifiers = value; }
    }
    List<ICustomModifier>/*?*/ returnValueCustomModifiers;

    /// <summary>
    /// True if the return value is passed by reference (using a managed pointer).
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsByRef {
      get { return this.returnValueIsByRef; }
      set { this.returnValueIsByRef = value; }
    }
    bool returnValueIsByRef;

    /// <summary>
    /// True if the return value has one or more custom modifiers associated with it.
    /// </summary>
    /// <value></value>
    public bool ReturnValueIsModified {
      get { return this.ReturnValueCustomModifiers != null && this.ReturnValueCustomModifiers.Count > 0; }
    }

    /// <summary>
    /// The return type of the method or type of the property.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region ISignature Members


    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get {
        if (this.Parameters == null) return Enumerable<IParameterTypeInformation>.Empty;
        return this.Parameters.AsReadOnly();
      }
    }

    IEnumerable<ICustomModifier> ISignature.ReturnValueCustomModifiers {
      get {
        if (this.ReturnValueCustomModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.ReturnValueCustomModifiers.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// An event defined inside a generic type instance or inside a specialized nested type.
  /// An event is a member that enables an object or class to provide notifications. Clients can attach executable code for events by supplying event handlers.
  /// </summary>
  public sealed class SpecializedEventDefinition : EventDefinition, ISpecializedEventDefinition, ICopyFrom<ISpecializedEventDefinition> {

    /// <summary>
    /// An event defined inside a generic type instance or inside a specialized nested type.
    /// An event is a member that enables an object or class to provide notifications. Clients can attach executable code for events by supplying event handlers.
    /// </summary>
    public SpecializedEventDefinition() {
      this.unspecializedVersion = Dummy.EventDefinition;
    }

    /// <summary>
    /// Makes a shallow copy of a specialized event. An event is a member that enables an object or class to provide notifications.
    /// Clients can attach executable code for events by supplying event handlers.
    /// This interface models the metadata representation of an event.
    /// </summary>
    public void Copy(ISpecializedEventDefinition specializedEventDefinition, IInternFactory internFactory) {
      ((ICopyFrom<IEventDefinition>)this).Copy(specializedEventDefinition, internFactory);
      this.unspecializedVersion = specializedEventDefinition.UnspecializedVersion;
    }

    #region ISpecializedEventDefinition Members

    /// <summary>
    /// The event that has been specialized to obtain this event. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    public IEventDefinition UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    IEventDefinition unspecializedVersion;

    #endregion

  }

  /// <summary>
  /// A field defined inside a generic type instance or inside a specialized nested type.
  /// A field is a member that represents a variable associated with an object or class.
  /// </summary>
  public sealed class SpecializedFieldDefinition : FieldDefinition, ISpecializedFieldDefinition, ICopyFrom<ISpecializedFieldDefinition> {

    /// <summary>
    /// A field is a member that represents a variable associated with an object or class.
    /// This interface models the metadata representation of a field.
    /// </summary>
    public SpecializedFieldDefinition() {
      this.unspecializedVersion = Dummy.FieldDefinition;
    }

    /// <summary>
    /// Makes a shallow copy of a specialized field.
    /// A field is a member that represents a variable associated with an object or class.
    /// This interface models the metadata representation of a field.
    /// </summary>
    /// <param name="specializedFieldDefinition"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISpecializedFieldDefinition specializedFieldDefinition, IInternFactory internFactory) {
      ((ICopyFrom<IFieldDefinition>)this).Copy(specializedFieldDefinition, internFactory);
      this.unspecializedVersion = specializedFieldDefinition.UnspecializedVersion;
    }

    #region ISpecializedFieldDefinition Members

    /// <summary>
    /// The field that has been specialized to obtain this field. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    public IFieldDefinition UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    IFieldDefinition unspecializedVersion;

    #endregion

    #region ISpecializedFieldReference Members

    IFieldReference ISpecializedFieldReference.UnspecializedVersion {
      get { return this.UnspecializedVersion; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class SpecializedFieldReference : FieldReference, ISpecializedFieldReference, ICopyFrom<ISpecializedFieldReference> {

    /// <summary>
    /// 
    /// </summary>
    public SpecializedFieldReference() {
      this.unspecializedVersion = Dummy.FieldReference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedFieldReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISpecializedFieldReference specializedFieldReference, IInternFactory internFactory) {
      ((ICopyFrom<IFieldReference>)this).Copy(specializedFieldReference, internFactory);
      this.unspecializedVersion = specializedFieldReference.UnspecializedVersion;
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedFieldReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A reference to the field definition that has been specialized to obtain the field definition referred to by this field reference.
    /// When the containing type of the referenced specialized field definition is itself a specialized nested type of a generic type instance,
    /// then the unspecialized field reference refers to the corresponding field definition from the unspecialized containing type definition.
    /// (I.e. the unspecialized field reference always refers to a field definition that is not obtained via specialization.)
    /// </summary>
    /// <value></value>
    public IFieldReference UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    IFieldReference unspecializedVersion;
  }

  /// <summary>
  /// A method defined inside a generic type instance or inside a specialized nested type.
  /// </summary>
  public sealed class SpecializedMethodDefinition : MethodDefinition, ISpecializedMethodDefinition, ICopyFrom<ISpecializedMethodDefinition> {

    /// <summary>
    /// A method defined inside a generic type instance or inside a specialized nested type.
    /// </summary>
    public SpecializedMethodDefinition() {
      this.unspecializedVersion = Dummy.MethodDefinition;
    }

    /// <summary>
    /// Makes a shallow copy of a specialized method.
    /// </summary>
    public void Copy(ISpecializedMethodDefinition specializedMethodDefinition, IInternFactory internFactory) {
      ((ICopyFrom<IMethodDefinition>)this).Copy(specializedMethodDefinition, internFactory);
      this.unspecializedVersion = specializedMethodDefinition.UnspecializedVersion;
    }

    #region ISpecializedMethodDefinition Members

    /// <summary>
    /// The method that has been specialized to obtain this method. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    public IMethodDefinition UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    IMethodDefinition unspecializedVersion;

    #endregion

    #region ISpecializedMethodReference Members

    IMethodReference ISpecializedMethodReference.UnspecializedVersion {
      get { return this.UnspecializedVersion; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class SpecializedMethodReference : MethodReference, ISpecializedMethodReference, ICopyFrom<ISpecializedMethodReference> {

    /// <summary>
    /// 
    /// </summary>
    public SpecializedMethodReference() {
      this.unspecializedVersion = Dummy.MethodReference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedMethodReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISpecializedMethodReference specializedMethodReference, IInternFactory internFactory) {
      ((ICopyFrom<IMethodReference>)this).Copy(specializedMethodReference, internFactory);
      this.unspecializedVersion = specializedMethodReference.UnspecializedVersion;
    }

    /// <summary>
    /// Calls visitor.Visit(ISpecializedMethodReference).
    /// </summary>
    /// <param name="visitor"></param>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// A reference to the method definition that has been specialized to obtain the method definition referred to by this method reference.
    /// When the containing type of the referenced specialized method definition is itself a specialized nested type of a generic type instance,
    /// then the unspecialized method reference refers to the corresponding method definition from the unspecialized containing type definition.
    /// (I.e. the unspecialized method reference always refers to a method definition that is not obtained via specialization.)
    /// </summary>
    /// <value></value>
    public IMethodReference UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    IMethodReference unspecializedVersion;

  }

  /// <summary>
  /// A property defined inside a generic type instance or inside a specialized nested type.
  /// </summary>
  public sealed class SpecializedPropertyDefinition : PropertyDefinition, ISpecializedPropertyDefinition, ICopyFrom<ISpecializedPropertyDefinition> {

    /// <summary>
    /// A property defined inside a generic type instance or inside a specialized nested type.
    /// </summary>
    public SpecializedPropertyDefinition() {
      this.unspecializedVersion = Dummy.PropertyDefinition;
    }

    /// <summary>
    /// Makes a shallow copy of a specialized property definition.
    /// </summary>
    public void Copy(ISpecializedPropertyDefinition specializedPropertyDefinition, IInternFactory internFactory) {
      ((ICopyFrom<IPropertyDefinition>)this).Copy(specializedPropertyDefinition, internFactory);
      this.unspecializedVersion = specializedPropertyDefinition.UnspecializedVersion;
    }

    #region ISpecializedPropertyDefinition Members

    /// <summary>
    /// The property that has been specialized to obtain this property. When the containing type is an instance of type which is itself a specialized member (i.e. it is a nested
    /// type of a generic type instance), then the unspecialized member refers to a member from the unspecialized containing type. (I.e. the unspecialized member always
    /// corresponds to a definition that is not obtained via specialization.)
    /// </summary>
    public IPropertyDefinition UnspecializedVersion {
      get { return this.unspecializedVersion; }
      set { this.unspecializedVersion = value; }
    }
    IPropertyDefinition unspecializedVersion;

    #endregion

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class TypeDefinitionMember : ITypeDefinitionMember, ICopyFrom<ITypeDefinitionMember> {

    /// <summary>
    /// 
    /// </summary>
    internal TypeDefinitionMember() {
      this.attributes = null;
      this.containingTypeDefinition = Dummy.TypeDefinition;
      this.locations = null;
      this.name = Dummy.Name;
      this.flags = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeDefinitionMember"></param>
    /// <param name="internFactory"></param>
    public void Copy(ITypeDefinitionMember typeDefinitionMember, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinitionMember.Attributes))
        this.attributes = new List<ICustomAttribute>(typeDefinitionMember.Attributes);
      else
        this.attributes = null;
      this.containingTypeDefinition = typeDefinitionMember.ContainingTypeDefinition;
      if (IteratorHelper.EnumerableIsNotEmpty(typeDefinitionMember.Locations))
        this.locations = new List<ILocation>(typeDefinitionMember.Locations);
      else
        this.locations = null;
      this.name = typeDefinitionMember.Name;
      this.flags = (int)typeDefinitionMember.Visibility;
    }

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
    /// A reference to the containing type of the referenced type member.
    /// </summary>
    /// <value></value>
    public ITypeDefinition ContainingTypeDefinition {
      get { return this.containingTypeDefinition; }
      set { this.containingTypeDefinition = value; }
    }
    ITypeDefinition containingTypeDefinition;

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

    internal int flags;

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
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set { this.name = value; }
    }
    IName name;

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() {
      return MemberHelper.GetMemberSignature(this,
        NameFormattingOptions.ParameterModifiers|NameFormattingOptions.TypeParameters|NameFormattingOptions.ParameterName|NameFormattingOptions.ReturnType|NameFormattingOptions.Signature);
    }

    /// <summary>
    /// Indicates if the member is public or confined to its containing type, derived types and/or declaring assembly.
    /// </summary>
    /// <value></value>
    public TypeMemberVisibility Visibility {
      get { return (TypeMemberVisibility)this.flags & TypeMemberVisibility.Mask; }
      set {
        this.flags &= (int)~TypeMemberVisibility.Mask;
        this.flags |= (int)(value & TypeMemberVisibility.Mask);
      }
    }

    #region ITypeMemberReference Members

    /// <summary>
    /// The type definition that contains this member.
    /// </summary>
    /// <value></value>
    ITypeReference ITypeMemberReference.ContainingType {
      get {
        if (this.ContainingTypeDefinition is Dummy) return Dummy.TypeReference;
        return this.ContainingTypeDefinition;
      }
    }

    /// <summary>
    /// The type definition member this reference resolves to.
    /// </summary>
    /// <value></value>
    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
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

    #region IContainerMember<ITypeDefinition> Members

    ITypeDefinition IContainerMember<ITypeDefinition>.Container {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    IScope<ITypeDefinitionMember> IScopeMember<IScope<ITypeDefinitionMember>>.ContainingScope {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion
  }

}
