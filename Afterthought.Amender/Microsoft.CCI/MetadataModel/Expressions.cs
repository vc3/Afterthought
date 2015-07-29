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
  /// An expression that does not change its value at runtime and can be evaluated at compile time.
  /// </summary>
  public interface IMetadataConstant : IMetadataExpression {

    /// <summary>
    /// The compile time value of the expression. Can be null.
    /// </summary>
    object/*?*/ Value { get; }
  }

  /// <summary>
  /// Implemented by IFieldDefinition, IParameterDefinition and IPropertyDefinition.
  /// </summary>
  public interface IMetadataConstantContainer {

    /// <summary>
    /// The constant value associated with this metadata object. For example, the default value of a parameter.
    /// </summary>
    IMetadataConstant Constant { get; }
  }

  /// <summary>
  /// An expression that creates an array instance in metadata. Only for use in custom attributes.
  /// </summary>
  [ContractClass(typeof(IMetadataCreateArrayContract))]
  public interface IMetadataCreateArray : IMetadataExpression {
    /// <summary>
    /// The element type of the array.
    /// </summary>
    ITypeReference ElementType { get; }

    /// <summary>
    /// The initial values of the array elements. May be empty.
    /// </summary>
    IEnumerable<IMetadataExpression> Initializers {
      get;
    }

    /// <summary>
    /// The index value of the first element in each dimension.
    /// </summary>
    IEnumerable<int> LowerBounds {
      get;
      // ^ ensures count{int lb in result} == Rank;
    }

    /// <summary>
    /// The number of dimensions of the array.
    /// </summary>
    uint Rank {
      get;
      //^ ensures result > 0;
    }

    /// <summary>
    /// The number of elements allowed in each dimension.
    /// </summary>
    IEnumerable<ulong> Sizes {
      get;
      // ^ ensures count{int size in result} == Rank;
    }

  }

  #region IMetadataCreateArray contract binding
  [ContractClassFor(typeof(IMetadataCreateArray))]
  abstract class IMetadataCreateArrayContract : IMetadataCreateArray {
    public ITypeReference ElementType {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IMetadataExpression> Initializers {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IMetadataExpression>>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<int> LowerBounds {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);
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
        throw new NotImplementedException();
      }
    }

    public ITypeReference Type {
      get {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ILocation> Locations {
      get {
        throw new NotImplementedException();
      }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }
  #endregion

  /// <summary>
  /// An expression that can be represented directly in metadata.
  /// </summary>
  [ContractClass(typeof(IMetadataExpressionContract))]
  public interface IMetadataExpression : IObjectWithLocations {

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IStatement. The dispatch method does not invoke Dispatch on any child objects. If child traversal
    /// is desired, the implementations of the Visit methods should do the subsequent dispatching.
    /// </summary>
    void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// The type of value the expression represents.
    /// </summary>
    ITypeReference Type { get; }
  }

  #region IMetadataExpression contract binding
  [ContractClassFor(typeof(IMetadataExpression))]
  abstract class IMetadataExpressionContract : IMetadataExpression {
    public void Dispatch(IMetadataVisitor visitor) {
      Contract.Requires(visitor != null);
      throw new NotImplementedException();
    }

    public ITypeReference Type {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IEnumerable<ILocation> Locations {
      get {
        throw new NotImplementedException(); 
      }
    }
  }
  #endregion


  /// <summary>
  /// An expression that represents a (name, value) pair and that is typically used in method calls, custom attributes and object initializers.
  /// </summary>
  [ContractClass(typeof(IMetadataNamedArgumentContract))]
  public interface IMetadataNamedArgument : IMetadataExpression {
    /// <summary>
    /// The name of the parameter or property or field that corresponds to the argument.
    /// </summary>
    IName ArgumentName { get; }

    /// <summary>
    /// The value of the argument.
    /// </summary>
    IMetadataExpression ArgumentValue { get; }

    /// <summary>
    /// True if the named argument provides the value of a field.
    /// </summary>
    bool IsField { get; }

    /// <summary>
    /// Returns either null or the parameter or property or field that corresponds to this argument.
    /// Obsolete, please do not use.
    /// </summary>
    object/*?*/ ResolvedDefinition { //TODO: remove this
      get;
      //^ ensures result == null || (IsField <==> result is IFieldDefinition) || result is IPropertyDefinition;
    }
  }

  [ContractClassFor(typeof(IMetadataNamedArgument))]
  abstract class IMetadataNamedArgumentContract : IMetadataNamedArgument {
    public IName ArgumentName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IMetadataExpression ArgumentValue {
      get {
        Contract.Ensures(Contract.Result<IMetadataExpression>() != null);
        throw new NotImplementedException(); 
      }
    }

    public bool IsField {
      get { 
        throw new NotImplementedException();
      }
    }

    public object ResolvedDefinition {
      get {
        Contract.Ensures(Contract.Result<object>() == null || (this.IsField && Contract.Result<object>() is IFieldDefinition) ||
          (!this.IsField && Contract.Result<object>() is IPropertyDefinition));
        throw new NotImplementedException(); 
      }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public ITypeReference Type {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }
  }

  /// <summary>
  /// An expression that results in a System.Type instance.
  /// </summary>
  public interface IMetadataTypeOf : IMetadataExpression {
    /// <summary>
    /// The type that will be represented by the System.Type instance.
    /// </summary>
    ITypeReference TypeToGet { get; }
  }

}
