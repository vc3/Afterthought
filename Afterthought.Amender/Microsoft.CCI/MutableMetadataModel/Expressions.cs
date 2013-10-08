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

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// An expression that does not change its value at runtime and can be evaluated at compile time.
  /// </summary>
  public sealed class MetadataConstant : MetadataExpression, IMetadataConstant, ICopyFrom<IMetadataConstant> {

    /// <summary>
    /// An expression that does not change its value at runtime and can be evaluated at compile time.
    /// </summary>
    public MetadataConstant() {
      this.value = null;
    }

    /// <summary>
    /// An expression that does not change its value at runtime and can be evaluated at compile time.
    /// </summary>
    /// <param name="metadataConstant">A template from which to construct a shallow copy.</param>
    /// <param name="internFactory">The intern factory to use for computing the interned identity (if applicable) of this mutable object.</param>
    public void Copy(IMetadataConstant metadataConstant, IInternFactory internFactory) {
      ((ICopyFrom<IMetadataExpression>)this).Copy(metadataConstant, internFactory);
      this.value = metadataConstant.Value;
    }

    /// <summary>
    /// Calls visitor(IMetadataConstant).
    /// </summary>
    /// <param name="visitor"></param>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The compile time value of the expression. Can be null.
    /// </summary>
    /// <value></value>
    public object Value {
      get { return this.value; }
      set { this.value = value; }
    }
    object value;


  }

  /// <summary>
  /// An expression that creates an array instance in metadata. Only for use in custom attributes.
  /// </summary>
  public sealed class MetadataCreateArray : MetadataExpression, IMetadataCreateArray, ICopyFrom<IMetadataCreateArray> {

    /// <summary>
    /// An expression that creates an array instance in metadata. Only for use in custom attributes.
    /// </summary>
    public MetadataCreateArray() {
      this.elementType = Dummy.TypeReference;
      this.initializers = null;
      this.lowerBounds = null;
      this.rank = 0;
      this.sizes = null;
    }

    /// <summary>
    /// Makes a shallow copy of an expression that creates an array instance in metadata. Only for use in custom attributes.
    /// </summary>
    /// <param name="createArray"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMetadataCreateArray createArray, IInternFactory internFactory) {
      ((ICopyFrom<IMetadataExpression>)this).Copy(createArray, internFactory);
      this.elementType = createArray.ElementType;
      if (!IteratorHelper.EnumerableIsEmpty(createArray.Initializers))
        this.initializers = new List<IMetadataExpression>(createArray.Initializers);
      if (!IteratorHelper.EnumerableIsEmpty(createArray.LowerBounds))
        this.lowerBounds = new List<int>(createArray.LowerBounds);
      this.rank = createArray.Rank;
      if (!IteratorHelper.EnumerableIsEmpty(createArray.Sizes))
        this.sizes = new List<ulong>(createArray.Sizes);
    }

    /// <summary>
    /// Calls visitor.Visit(IMetadataCreateArray).
    /// </summary>
    /// <param name="visitor"></param>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The element type of the array.
    /// </summary>
    /// <value></value>
    public ITypeReference ElementType {
      get { return this.elementType; }
      set { this.elementType = value; }
    }
    ITypeReference elementType;

    /// <summary>
    /// The initial values of the array elements. May be empty or null.
    /// </summary>
    /// <value></value>
    public List<IMetadataExpression>/*?*/ Initializers {
      get { return this.initializers; }
      set { this.initializers = value; }
    }
    List<IMetadataExpression>/*?*/ initializers;

    /// <summary>
    /// The index value of the first element in each dimension. May be null.
    /// </summary>
    /// <value></value>
    public List<int>/*?*/ LowerBounds {
      get { return this.lowerBounds; }
      set { this.lowerBounds = value; }
    }
    List<int>/*?*/ lowerBounds;

    /// <summary>
    /// The number of dimensions of the array.
    /// </summary>
    /// <value></value>
    public uint Rank {
      get { return this.rank; }
      set { this.rank = value; }
    }
    uint rank;

    /// <summary>
    /// The number of elements allowed in each dimension. May be null.
    /// </summary>
    /// <value></value>
    public List<ulong>/*?*/ Sizes {
      get { return this.sizes; }
      set { this.sizes = value; }
    }
    List<ulong>/*?*/ sizes;


    #region IMetadataCreateArray Members


    IEnumerable<IMetadataExpression> IMetadataCreateArray.Initializers {
      get {
        if (this.initializers == null) return Enumerable<IMetadataExpression>.Empty;
        return this.initializers.AsReadOnly(); 
      }
    }

    IEnumerable<int> IMetadataCreateArray.LowerBounds {
      get {
        if (this.lowerBounds == null) return Enumerable<int>.Empty;
        return this.lowerBounds.AsReadOnly(); 
      }
    }

    IEnumerable<ulong> IMetadataCreateArray.Sizes {
      get {
        if (this.sizes == null) return Enumerable<ulong>.Empty;
        return this.sizes.AsReadOnly(); 
      }
    }

    #endregion
  }

  /// <summary>
  /// An expression that can be represented directly in metadata.
  /// </summary>
  public abstract class MetadataExpression : IMetadataExpression, ICopyFrom<IMetadataExpression> {

    /// <summary>
    /// An expression that can be represented directly in metadata.
    /// </summary>
    internal MetadataExpression() {
      this.locations = null;
      this.type = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of an expression that can be represented directly in metadata.
    /// </summary>
    /// <param name="metadataExpression"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMetadataExpression metadataExpression, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(metadataExpression.Locations))
        this.locations = new List<ILocation>(metadataExpression.Locations);
      this.type = metadataExpression.Type;
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IStatement. The dispatch method does not invoke Dispatch on any child objects. If child traversal
    /// is desired, the implementations of the Visit methods should do the subsequent dispatching.
    /// </summary>
    /// <param name="visitor"></param>
    public abstract void Dispatch(IMetadataVisitor visitor);

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
    /// The type of value the expression represents.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set { this.type = value; }
    }
    ITypeReference type;

    #region IMetadataExpression Members

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      get {
        if (this.locations == null) return Enumerable<ILocation>.Empty;
        return this.locations.AsReadOnly(); 
      }
    }

    #endregion
  }

  /// <summary>
  /// An expression that represents a (name, value) pair and that is typically used in method calls, custom attributes and object initializers.
  /// </summary>
  public sealed class MetadataNamedArgument : MetadataExpression, IMetadataNamedArgument, ICopyFrom<IMetadataNamedArgument> {

    /// <summary>
    /// An expression that represents a (name, value) pair and that is typically used in method calls, custom attributes and object initializers.
    /// </summary>
    public MetadataNamedArgument() {
      this.argumentName = Dummy.Name;
      this.argumentValue = Dummy.Expression;
      this.isField = false;
      this.resolvedDefinition = null;
    }

    /// <summary>
    /// Makes a shallow copy of an expression that represents a (name, value) pair and that is typically used in method calls, custom attributes and object initializers.
    /// </summary>
    /// <param name="namedArgument"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMetadataNamedArgument namedArgument, IInternFactory internFactory) {
      ((ICopyFrom<IMetadataExpression>)this).Copy(namedArgument, internFactory);
      this.argumentName = namedArgument.ArgumentName;
      this.argumentValue = namedArgument.ArgumentValue;
      this.isField = namedArgument.IsField;
      this.resolvedDefinition = namedArgument.ResolvedDefinition;
    }

    /// <summary>
    /// The name of the parameter or property or field that corresponds to the argument.
    /// </summary>
    /// <value></value>
    public IName ArgumentName {
      get { return this.argumentName; }
      set { this.argumentName = value; }
    }
    IName argumentName;

    /// <summary>
    /// The value of the argument.
    /// </summary>
    /// <value></value>
    public IMetadataExpression ArgumentValue {
      get { return this.argumentValue; }
      set { this.argumentValue = value; }
    }
    IMetadataExpression argumentValue;

    /// <summary>
    /// Calls visitor.Visit(IMetadataNamedArgument).
    /// </summary>
    /// <param name="visitor"></param>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// True if the named argument provides the value of a field.
    /// </summary>
    /// <value></value>
    public bool IsField {
      get { return this.isField; }
      set { this.isField = value; }
    }
    bool isField;

    /// <summary>
    /// Returns either null or the parameter or property or field that corresponds to this argument.
    /// </summary>
    /// <value></value>
    public object/*?*/ ResolvedDefinition {
      get { return this.resolvedDefinition; }
      set { this.resolvedDefinition = value; }
    }
    object/*?*/ resolvedDefinition;

  }

  /// <summary>
  /// An expression that results in a System.Type instance.
  /// </summary>
  public sealed class MetadataTypeOf : MetadataExpression, IMetadataTypeOf, ICopyFrom<IMetadataTypeOf> {

    /// <summary>
    /// An expression that results in a System.Type instance.
    /// </summary>
    public MetadataTypeOf() {
      this.typeToGet = Dummy.TypeReference;
    }

    /// <summary>
    /// Makes a shallow copy of an expression that results in a System.Type instance.
    /// </summary>
    /// <param name="typeOf"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMetadataTypeOf typeOf, IInternFactory internFactory) {
      ((ICopyFrom<IMetadataExpression>)this).Copy(typeOf, internFactory);
      this.typeToGet = typeOf.TypeToGet;
    }

    /// <summary>
    /// Calls visitor.Visit(IMetadataTypeOf).
    /// </summary>
    /// <param name="visitor"></param>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The type that will be represented by the System.Type instance.
    /// </summary>
    /// <value></value>
    public ITypeReference TypeToGet {
      get { return this.typeToGet; }
      set { this.typeToGet = value; }
    }
    ITypeReference typeToGet;

  }

}