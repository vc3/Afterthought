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
using System.Text;

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// 
  /// </summary>
  public sealed class NestedUnitNamespace : UnitNamespace, INestedUnitNamespace, ICopyFrom<INestedUnitNamespace> {

    /// <summary>
    /// 
    /// </summary>
    public NestedUnitNamespace() {
      this.containingUnitNamespace = Dummy.RootUnitNamespace;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedUnitNamespace"></param>
    /// <param name="internFactory"></param>
    public void Copy(INestedUnitNamespace nestedUnitNamespace, IInternFactory internFactory) {
      ((ICopyFrom<IUnitNamespace>)this).Copy(nestedUnitNamespace, internFactory);
      this.containingUnitNamespace = nestedUnitNamespace.ContainingUnitNamespace;
    }

    /// <summary>
    /// The unit namespace that contains this member.
    /// </summary>
    /// <value></value>
    public IUnitNamespace ContainingUnitNamespace {
      get { return this.containingUnitNamespace; }
      set { this.containingUnitNamespace = value; }
    }
    IUnitNamespace containingUnitNamespace;

    /// <summary>
    /// Calls visitor.Visit(INestedUnitNamespace).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(INestedUnitNamespaceReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INestedUnitNamespaceReference)this);
    }

    internal override IUnit GetUnit() {
      return this.ContainingUnitNamespace.Unit;
    }

    #region INamespaceMember Members

    INamespaceDefinition INamespaceMember.ContainingNamespace {
      get { return this.ContainingUnitNamespace; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    /// <summary>
    /// The scope instance with a Members collection that includes this instance.
    /// </summary>
    /// <value></value>
    public IScope<INamespaceMember> ContainingScope {
      get { return this.ContainingUnitNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    /// <summary>
    /// The container instance with a Members collection that includes this instance.
    /// </summary>
    /// <value></value>
    public INamespaceDefinition Container {
      get { return this.ContainingUnitNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region INestedUnitNamespaceReference Members

    IUnitNamespaceReference INestedUnitNamespaceReference.ContainingUnitNamespace {
      get { return this.ContainingUnitNamespace; }
    }

    INestedUnitNamespace INestedUnitNamespaceReference.ResolvedNestedUnitNamespace {
      get { return this; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class NestedUnitNamespaceReference : UnitNamespaceReference, INestedUnitNamespaceReference, ICopyFrom<INestedUnitNamespaceReference> {

    /// <summary>
    /// 
    /// </summary>
    public NestedUnitNamespaceReference() {
      Contract.Ensures(!this.IsFrozen);
      this.containingUnitNamespace = Dummy.RootUnitNamespace;
      this.name = Dummy.Name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedUnitNamespaceReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(INestedUnitNamespaceReference nestedUnitNamespaceReference, IInternFactory internFactory) {
      ((ICopyFrom<IUnitNamespaceReference>)this).Copy(nestedUnitNamespaceReference, internFactory);
      this.containingUnitNamespace = nestedUnitNamespaceReference.ContainingUnitNamespace;
      this.name = nestedUnitNamespaceReference.Name;
    }

    /// <summary>
    /// A reference to the unit namespace that contains the referenced nested unit namespace.
    /// </summary>
    /// <value></value>
    public IUnitNamespaceReference ContainingUnitNamespace {
      get { return this.containingUnitNamespace; }
      set { this.containingUnitNamespace = value; this.resolvedNestedUnitNamespace = null; }
    }
    IUnitNamespaceReference containingUnitNamespace;

    /// <summary>
    /// Calls visitor.Visit(INestedUnitNamespaceReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Gets the unit.
    /// </summary>
    /// <returns></returns>
    internal override IUnitReference GetUnit() {
      return this.Unit;
    }

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

    private INestedUnitNamespace Resolve() {
      this.isFrozen = true;
      foreach (INamespaceMember member in this.containingUnitNamespace.ResolvedUnitNamespace.GetMembersNamed(this.Name, false)) {
        INestedUnitNamespace/*?*/ ns = member as INestedUnitNamespace;
        if (ns != null) return ns;
      }
      return Dummy.NestedUnitNamespace;
    }

    /// <summary>
    /// The namespace definition being referred to.
    /// </summary>
    /// <value></value>
    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get {
        if (this.resolvedNestedUnitNamespace == null)
          this.resolvedNestedUnitNamespace = this.Resolve();
        return this.resolvedNestedUnitNamespace;
      }
    }
    INestedUnitNamespace/*?*/ resolvedNestedUnitNamespace;

    /// <summary>
    /// The namespace definition being referred to.
    /// </summary>
    /// <value></value>
    public override IUnitNamespace ResolvedUnitNamespace {
      get { return this.ResolvedNestedUnitNamespace; }
    }

    /// <summary>
    /// A reference to the unit that defines the referenced namespace.
    /// </summary>
    /// <value></value>
    public IUnitReference Unit {
      get { return this.containingUnitNamespace.Unit; }
    }


  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class RootUnitNamespace : UnitNamespace, IRootUnitNamespace, ICopyFrom<IRootUnitNamespace> {

    /// <summary>
    /// 
    /// </summary>
    public RootUnitNamespace() {
      this.unit = Dummy.Unit;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootUnitNamespace"></param>
    /// <param name="internFactory"></param>
    public void Copy(IRootUnitNamespace rootUnitNamespace, IInternFactory internFactory) {
      ((ICopyFrom<IUnitNamespace>)this).Copy(rootUnitNamespace, internFactory);
      this.unit = rootUnitNamespace.Unit;
    }

    /// <summary>
    /// Calls visitor.Visit(IRootUnitNamespace).
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Calls visitor.Visit(IRootUnitNamespaceReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IRootUnitNamespaceReference)this);
    }

    internal override IUnit GetUnit() {
      return this.unit;
    }

    /// <summary>
    /// The IUnit instance associated with this namespace.
    /// </summary>
    /// <value></value>
    public new IUnit Unit {
      get { return this.unit; }
      set { this.unit = value; }
    }
    IUnit unit;

  }
  /// <summary>
  /// 
  /// </summary>
  public sealed class RootUnitNamespaceReference : UnitNamespaceReference, IRootUnitNamespaceReference, ICopyFrom<IRootUnitNamespaceReference> {

    /// <summary>
    /// 
    /// </summary>
    public RootUnitNamespaceReference() {
      Contract.Ensures(!this.IsFrozen);
      this.unit = Dummy.Unit;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootUnitNamespaceReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IRootUnitNamespaceReference rootUnitNamespaceReference, IInternFactory internFactory) {
      ((ICopyFrom<IUnitNamespaceReference>)this).Copy(rootUnitNamespaceReference, internFactory);
      this.unit = rootUnitNamespaceReference.Unit;
    }

    /// <summary>
    /// Calls visitor.Visit(IRootUnitNamespaceReference).
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    internal override IUnitReference GetUnit() {
      return this.Unit;
    }

    /// <summary>
    /// The namespace definition being referred to.
    /// </summary>
    /// <value></value>
    public override IUnitNamespace ResolvedUnitNamespace {
      get {
        if (this.resolvedUnitNamespace == null) {
          this.isFrozen = true;
          this.resolvedUnitNamespace = this.Unit.ResolvedUnit.UnitNamespaceRoot;
        }
        return this.resolvedUnitNamespace;
      }
    }
    IUnitNamespace resolvedUnitNamespace;

    /// <summary>
    /// A reference to the unit that defines the referenced namespace.
    /// </summary>
    /// <value></value>
    public IUnitReference Unit {
      get { return this.unit; }
      set {
        Contract.Requires(!this.IsFrozen);
        Contract.Requires(value != null);
        this.unit = value; 
      }
    }
    IUnitReference unit;

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class UnitNamespace : IUnitNamespace, ICopyFrom<IUnitNamespace> {

    /// <summary>
    /// 
    /// </summary>
    internal UnitNamespace() {
      this.attributes = null;
      this.locations = null;
      this.members = null;
      this.name = Dummy.Name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitNamespace"></param>
    /// <param name="internFactory"></param>
    public virtual void Copy(IUnitNamespace unitNamespace, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(unitNamespace.Attributes))
        this.attributes = new List<ICustomAttribute>(unitNamespace.Attributes);
      else
        this.attributes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(unitNamespace.Locations))
        this.locations = new List<ILocation>(unitNamespace.Locations);
      else
        this.locations = null;
      this.members = new List<INamespaceMember>(unitNamespace.Members);
      this.name = unitNamespace.Name;
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

    //^ [Pure]
    /// <summary>
    /// Return true if the given member instance is a member of this scope.
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public bool Contains(INamespaceMember member) {
      foreach (INamespaceMember nsmem in this.Members)
        if (member == nsmem) return true;
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

    //^ [Pure]
    /// <summary>
    /// Returns the list of members with the given name that also satisfy the given predicate.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      foreach (INamespaceMember nsmem in this.Members) {
        if (nsmem.Name.UniqueKey == name.UniqueKey || ignoreCase && (name.UniqueKeyIgnoringCase == nsmem.Name.UniqueKeyIgnoringCase)) {
          if (predicate(nsmem)) yield return nsmem;
        }
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members that satisfy the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      foreach (INamespaceMember nsmem in this.Members) {
        if (predicate(nsmem)) yield return nsmem;
      }
    }

    //^ [Pure]
    /// <summary>
    /// Returns the list of members with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      foreach (INamespaceMember nsmem in this.Members) {
        if (nsmem.Name.UniqueKey == name.UniqueKey || ignoreCase && (name.UniqueKeyIgnoringCase == nsmem.Name.UniqueKeyIgnoringCase)) {
          yield return nsmem;
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
    /// The collection of member objects comprising the namespaces.
    /// </summary>
    /// <value></value>
    public List<INamespaceMember> Members {
      get {
        Contract.Ensures(Contract.Result<List<INamespaceMember>>() != null);
        if (this.members == null) this.members = new List<INamespaceMember>();
        return this.members; 
      }
      set { this.members = value; }
    }
    List<INamespaceMember>/*?*/ members;

    /// <summary>
    /// The name of the entity.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set { this.name = value; }
    }
    IName name;

    internal abstract IUnit GetUnit();

    /// <summary>
    /// The IUnit instance associated with this namespace.
    /// </summary>
    /// <value></value>
    public IUnit Unit {
      get { return this.GetUnit(); }
    }

    #region INamespaceDefinition Members

    INamespaceRootOwner INamespaceDefinition.RootOwner {
      get { return this.Unit; }
    }

    #endregion

    #region INamespaceDefinition Members


    IEnumerable<INamespaceMember> INamespaceDefinition.Members {
      get { return this.Members.AsReadOnly(); }
    }

    #endregion

    #region IContainer<INamespaceMember> Members

    IEnumerable<INamespaceMember> IContainer<INamespaceMember>.Members {
      get { return this.Members.AsReadOnly(); }
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

    #region IScope<INamespaceMember> Members

    IEnumerable<INamespaceMember> IScope<INamespaceMember>.Members {
      get { return this.Members.AsReadOnly(); }
    }

    #endregion

    #region IUnitNamespaceReference Members

    IUnitReference IUnitNamespaceReference.Unit {
      get { return this.Unit; }
    }

    IUnitNamespace IUnitNamespaceReference.ResolvedUnitNamespace {
      get { return this; }
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() {
      return TypeHelper.GetNamespaceName(this, NameFormattingOptions.SmartNamespaceName);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class UnitNamespaceReference : IUnitNamespaceReference, ICopyFrom<IUnitNamespaceReference> {

    /// <summary>
    /// 
    /// </summary>
    internal UnitNamespaceReference() {
      this.attributes = null;
      this.locations = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitNamespaceReference"></param>
    /// <param name="internFactory"></param>
    public virtual void Copy(IUnitNamespaceReference unitNamespaceReference, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(unitNamespaceReference.Attributes))
        this.attributes = new List<ICustomAttribute>(unitNamespaceReference.Attributes);
      else
        this.attributes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(unitNamespaceReference.Locations))
        this.locations = new List<ILocation>(unitNamespaceReference.Locations);
      else
        this.locations = null;
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
      set { this.locations = value; }
    }
    List<ILocation>/*?*/ locations;

    /// <summary>
    /// The namespace definition being referred to.
    /// </summary>
    /// <value></value>
    public abstract IUnitNamespace ResolvedUnitNamespace {
      get;
    }

    internal abstract IUnitReference GetUnit();

    IUnitReference IUnitNamespaceReference.Unit {
      get { return this.GetUnit(); }
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() {
      return TypeHelper.GetNamespaceName(this, NameFormattingOptions.None);
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
