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
  /// A definition that is a member of a namespace. Typically a nested namespace or a namespace type definition.
  /// </summary>
  [ContractClass(typeof(INamespaceMemberContract))]
  public interface INamespaceMember : IContainerMember<INamespaceDefinition>, IDefinition, IScopeMember<IScope<INamespaceMember>> {
    /// <summary>
    /// The namespace that contains this member.
    /// </summary>
    INamespaceDefinition ContainingNamespace { get; }

  }

  [ContractClassFor(typeof(INamespaceMember))]
  abstract class INamespaceMemberContract : INamespaceMember {
    public INamespaceDefinition ContainingNamespace {
      get {
        Contract.Ensures(Contract.Result<INamespaceDefinition>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceDefinition Container {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
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

    public IScope<INamespaceMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Implemented by objects that are associated with a root INamespace object.
  /// </summary>
  [ContractClass(typeof(INamespaceRootOwnerContract))]
  public interface INamespaceRootOwner {
    /// <summary>
    /// The associated root namespace.
    /// </summary>
    INamespaceDefinition NamespaceRoot {
      get;
      //^ ensures result.RootOwner == this;
    }
  }

  [ContractClassFor(typeof(INamespaceRootOwner))]
  abstract class INamespaceRootOwnerContract : INamespaceRootOwner {
    public INamespaceDefinition NamespaceRoot {
      get {
        Contract.Ensures(Contract.Result<INamespaceDefinition>() != null);
        throw new NotImplementedException();
      }
    }
  }

  /// <summary>
  /// A unit namespace that is nested inside another unit namespace.
  /// </summary>
  [ContractClass(typeof(INestedUnitNamespaceContract))]
  public interface INestedUnitNamespace : IUnitNamespace, INamespaceMember, INestedUnitNamespaceReference {

    /// <summary>
    /// The unit namespace that contains this member.
    /// </summary>
    new IUnitNamespace ContainingUnitNamespace { get; }

  }

  [ContractClassFor(typeof(INestedUnitNamespace))]
  abstract class INestedUnitNamespaceContract : INestedUnitNamespace {
    public IUnitNamespace ContainingUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespace>() != null);
        throw new NotImplementedException();
      }
    }

    public IUnit Unit {
      get { throw new NotImplementedException(); }
    }

    public INamespaceRootOwner RootOwner {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INamespaceMember> Members {
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

    public bool Contains(INamespaceMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    IUnitReference IUnitNamespaceReference.Unit {
      get { throw new NotImplementedException(); }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get { throw new NotImplementedException(); }
    }

    public INamespaceDefinition ContainingNamespace {
      get { throw new NotImplementedException(); }
    }

    public INamespaceDefinition Container {
      get { throw new NotImplementedException(); }
    }

    public IScope<INamespaceMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }

    IUnitNamespaceReference INestedUnitNamespaceReference.ContainingUnitNamespace {
      get { throw new NotImplementedException(); }
    }

    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to a nested unit namespace.
  /// </summary>
  [ContractClass(typeof(INestedUnitNamespaceReferenceContract))]
  public interface INestedUnitNamespaceReference : IUnitNamespaceReference, INamedEntity {

    /// <summary>
    /// A reference to the unit namespace that contains the referenced nested unit namespace.
    /// </summary>
    IUnitNamespaceReference ContainingUnitNamespace { get; }

    /// <summary>
    /// The namespace definition being referred to.
    /// </summary>
    INestedUnitNamespace ResolvedNestedUnitNamespace { get; }
  }

  [ContractClassFor(typeof(INestedUnitNamespaceReference))]
  abstract class INestedUnitNamespaceReferenceContract : INestedUnitNamespaceReference {
    public IUnitNamespaceReference ContainingUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespaceReference>() != null);
        Contract.Ensures(Contract.Result<IUnitNamespaceReference>() != this);
        throw new NotImplementedException();
      }
    }

    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<INestedUnitNamespace>() != null);
        throw new NotImplementedException();
      }
    }

    public IUnitReference Unit {
      get { throw new NotImplementedException(); }
    }

    public IUnitNamespace ResolvedUnitNamespace {
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
  /// A unit set namespace that is nested inside another unit set namespace.
  /// </summary>
  [ContractClass(typeof(INestedUnitSetNamespaceContract))]
  public interface INestedUnitSetNamespace : IUnitSetNamespace, INamespaceMember {

    /// <summary>
    /// The unit set namespace that contains this member.
    /// </summary>
    IUnitSetNamespace ContainingUnitSetNamespace { get; }

  }

  [ContractClassFor(typeof(INestedUnitSetNamespace))]
  abstract class INestedUnitSetNamespaceContract : INestedUnitSetNamespace {
    public IUnitSetNamespace ContainingUnitSetNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitSetNamespace>() != null);
        throw new NotImplementedException();
      }
    }

    public IUnitSet UnitSet {
      get { throw new NotImplementedException(); }
    }

    public INamespaceRootOwner RootOwner {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INamespaceMember> Members {
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

    public bool Contains(INamespaceMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public INamespaceDefinition ContainingNamespace {
      get { throw new NotImplementedException(); }
    }

    public INamespaceDefinition Container {
      get { throw new NotImplementedException(); }
    }

    public IScope<INamespaceMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A named collection of namespace members, with routines to search and maintain the collection.
  /// </summary>
  [ContractClass(typeof(INamespaceDefinitionContract))]
  public interface INamespaceDefinition : IContainer<INamespaceMember>, IDefinition, INamedEntity, IScope<INamespaceMember> {

    /// <summary>
    /// The object associated with the namespace. For example an IUnit or IUnitSet instance. This namespace is either the root namespace of that object
    /// or it is a nested namespace that is directly of indirectly nested in the root namespace.
    /// </summary>
    INamespaceRootOwner RootOwner {
      get;
    }

    /// <summary>
    /// The collection of member objects comprising the namespaces.
    /// </summary>
    new IEnumerable<INamespaceMember> Members {
      get;
    }
  }

  [ContractClassFor(typeof(INamespaceDefinition))]
  abstract class INamespaceDefinitionContract : INamespaceDefinition {
    public INamespaceRootOwner RootOwner {
      get {
        Contract.Ensures(Contract.Result<INamespaceRootOwner>() != null);
        throw new NotImplementedException();
      }
    }

    IEnumerable<INamespaceMember> INamespaceDefinition.Members {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<INamespaceMember>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<INamespaceMember>>(), x => x != null));
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

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public bool Contains(INamespaceMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> Members {
      get { throw new NotImplementedException(); }
    }
  }

  /// <summary>
  /// A unit namespace that is not nested inside another namespace.
  /// </summary>
  public interface IRootUnitNamespace : IUnitNamespace, IRootUnitNamespaceReference {
  }

  /// <summary>
  /// A reference to a root unit namespace.
  /// </summary>
  public interface IRootUnitNamespaceReference : IUnitNamespaceReference {
  }

  /// <summary>
  /// A named collection of namespace members, with routines to search and maintain the collection. All the members belong to an associated
  /// IUnit instance.
  /// </summary>
  [ContractClass(typeof(IUnitNamespaceContract))]
  public interface IUnitNamespace : INamespaceDefinition, IUnitNamespaceReference {

    /// <summary>
    /// The IUnit instance associated with this namespace.
    /// </summary>
    new IUnit Unit {
      get;
    }
  }

  [ContractClassFor(typeof(IUnitNamespace))]
  abstract class IUnitNamespaceContract : IUnitNamespace {
    public IUnit Unit {
      get {
        Contract.Ensures(Contract.Result<IUnit>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceRootOwner RootOwner {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INamespaceMember> Members {
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

    public bool Contains(INamespaceMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    IUnitReference IUnitNamespaceReference.Unit {
      get { throw new NotImplementedException(); }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to an unit namespace.
  /// </summary>
  public partial interface IUnitNamespaceReference : IReference {

    /// <summary>
    /// A reference to the unit that defines the referenced namespace.
    /// </summary>
    IUnitReference Unit {
      get;
    }

    /// <summary>
    /// The namespace definition being referred to.
    /// </summary>
    IUnitNamespace ResolvedUnitNamespace { get; }
  }

  #region IUnitNamespaceReference contract binding
  [ContractClass(typeof(IUnitNamespaceReferenceContract))]
  public partial interface IUnitNamespaceReference {

  }

  [ContractClassFor(typeof(IUnitNamespaceReference))]
  abstract class IUnitNamespaceReferenceContract : IUnitNamespaceReference {
    public IUnitReference Unit {
      get {
        Contract.Ensures(Contract.Result<IUnitReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespace>() != null);
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

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  #endregion

  /// <summary>
  /// A named collection of namespace members, with routines to search and maintain the collection. The collection of members
  /// is the union of the individual members collections of one of more IUnit instances making up the IUnitSet instance associated
  /// with this namespace.
  /// </summary>
  //  Issue: If we just want to model Metadata/Language independent model faithfully, why do we need unit sets etc? This seems to be more of an
  //  Symbol table lookup helper interface.
  [ContractClass(typeof(IUnitSetNamespaceContract))]
  public interface IUnitSetNamespace : INamespaceDefinition {

    /// <summary>
    /// The IUnitSet instance associated with the namespace.
    /// </summary>
    IUnitSet UnitSet {
      get;
    }
  }

  [ContractClassFor(typeof(IUnitSetNamespace))]
  abstract class IUnitSetNamespaceContract : IUnitSetNamespace {
    public IUnitSet UnitSet {
      get {
        Contract.Ensures(Contract.Result<IUnitSet>() != null);
        throw new NotImplementedException();
      }
    }

    public INamespaceRootOwner RootOwner {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<INamespaceMember> Members {
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

    public bool Contains(INamespaceMember member) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A unit set namespace that is not nested inside another namespace.
  /// </summary>
  public interface IRootUnitSetNamespace : IUnitSetNamespace {
  }
}

