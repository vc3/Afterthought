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

namespace Microsoft.Cci {
  using Microsoft.Cci.ILGeneratorImplementation;
  /// <summary>
  /// A metadata (IL) level represetation of the body of a method or of a property/event accessor.
  /// </summary>
  public class ILGeneratorMethodBody : IMethodBody {

    /// <summary>
    /// Allocates an object that is the metadata (IL) level represetation of the body of a method or of a property/event accessor.
    /// </summary>
    /// <param name="generator">An object that provides a way to construct the information needed by a method body. Construction should
    /// be completed by the time the generator is passed to this constructor. The generator is not referenced by the resulting method body.</param>
    /// <param name="localsAreZeroed">True if the locals are initialized by zeroeing the stack upon method entry.</param>
    /// <param name="maxStack">The maximum number of elements on the evaluation stack during the execution of the method.</param>
    /// <param name="methodDefinition">The definition of the method whose body this is.
    /// If this is the body of an event or property accessor, this will hold the corresponding adder/remover/setter or getter method.</param>
    /// <param name="localVariables"></param>
    /// <param name="privateHelperTypes">Any types that are implicitly defined in order to implement the body semantics.
    /// In case of AST to instructions conversion this lists the types produced.
    /// In case of instructions to AST decompilation this should ideally be list of all types
    /// which are local to method.</param>
    public ILGeneratorMethodBody(ILGenerator generator, bool localsAreZeroed, ushort maxStack, IMethodDefinition methodDefinition,
      IEnumerable<ILocalDefinition> localVariables, IEnumerable<ITypeDefinition> privateHelperTypes) {
      Contract.Requires(generator != null);
      Contract.Requires(methodDefinition != null);
      Contract.Requires(localVariables != null);
      Contract.Requires(privateHelperTypes != null);

      this.localsAreZeroed = localsAreZeroed;
      this.operationExceptionInformation = generator.GetOperationExceptionInformation();
      this.operations = generator.GetOperations();
      this.privateHelperTypes = privateHelperTypes;
      this.generatorIteratorScopes = generator.GetIteratorScopes();
      this.generatorLocalScopes = generator.GetLocalScopes();
      this.localVariables = localVariables;
      this.maxStack = maxStack;
      this.methodDefinition = methodDefinition;
      this.size = generator.CurrentOffset;
      this.synchronizationInformation = generator.GetSynchronizationInformation();
    }

    /// <summary>
    /// Calls visitor.Visit(IMethodBody).
    /// </summary>
    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    readonly IEnumerable<ILocalScope>/*?*/ generatorIteratorScopes;
    readonly IEnumerable<ILGeneratorScope> generatorLocalScopes;
    readonly ISynchronizationInformation/*?*/ synchronizationInformation;

    /// <summary>
    /// Returns a block scope associated with each local variable in the iterator for which this is the generator for its MoveNext method.
    /// May return null.
    /// </summary>
    /// <remarks>The PDB file model seems to be that scopes are duplicated if necessary so that there is a separate scope for each
    /// local variable in the original iterator and the mapping from local to scope is done by position.</remarks>
    public IEnumerable<ILocalScope>/*?*/ GetIteratorScopes() {
      return this.generatorIteratorScopes;
    }

    /// <summary>
    /// Returns zero or more local (block) scopes into which the CLR IL operations of this method body is organized.
    /// </summary>
    public IEnumerable<ILocalScope> GetLocalScopes() {
      foreach (var generatorScope in this.generatorLocalScopes) {
        if (generatorScope.locals.Count > 0)
          yield return generatorScope;
      }
    }

    /// <summary>
    /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
    /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
    /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
    /// is for the y.
    /// </summary>
    public IEnumerable<INamespaceScope> GetNamespaceScopes() {
      foreach (var generatorScope in this.generatorLocalScopes) {
        if (generatorScope.usedNamespaces.Count > 0)
          yield return generatorScope;
      }
    }

    /// <summary>
    /// Returns an object that describes where synchronization points occur in the IL operations of the "MoveNext" method of
    /// the state class of an async method. Returns null otherwise.
    /// </summary>
    public ISynchronizationInformation/*?*/ GetSynchronizationInformation() {
      return this.synchronizationInformation;
    }

    /// <summary>
    /// A list exception data within the method body IL.
    /// </summary>
    public IEnumerable<IOperationExceptionInformation> OperationExceptionInformation {
      [ContractVerification(false)]
      get { return this.operationExceptionInformation; }
    }
    readonly IEnumerable<IOperationExceptionInformation> operationExceptionInformation;

    /// <summary>
    /// True if the locals are initialized by zeroeing the stack upon method entry.
    /// </summary>
    public bool LocalsAreZeroed {
      get { return this.localsAreZeroed; }
    }
    readonly bool localsAreZeroed;

    /// <summary>
    /// The local variables of the method.
    /// </summary>
    public IEnumerable<ILocalDefinition> LocalVariables {
      [ContractVerification(false)]
      get { return this.localVariables; }
    }
    readonly IEnumerable<ILocalDefinition> localVariables;

    /// <summary>
    /// The definition of the method whose body this is.
    /// If this is the body of an event or property accessor, this will hold the corresponding adder/remover/setter or getter method.
    /// </summary>
    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
    }
    readonly IMethodDefinition methodDefinition;

    /// <summary>
    /// A list CLR IL operations that implement this method body.
    /// </summary>
    public IEnumerable<IOperation> Operations {
      [ContractVerification(false)]
      get { return this.operations; }
    }
    readonly IEnumerable<IOperation> operations;

    /// <summary>
    /// The maximum number of elements on the evaluation stack during the execution of the method.
    /// </summary>
    public ushort MaxStack {
      get { return this.maxStack; }
    }
    readonly ushort maxStack;

    /// <summary>
    /// Any types that are implicitly defined in order to implement the body semantics.
    /// In case of AST to instructions conversion this lists the types produced.
    /// In case of instructions to AST decompilation this should ideally be list of all types
    /// which are local to method.
    /// </summary>
    public IEnumerable<ITypeDefinition> PrivateHelperTypes {
      [ContractVerification(false)]
      get { return this.privateHelperTypes; }
    }
    readonly IEnumerable<ITypeDefinition> privateHelperTypes;

    /// <summary>
    /// The size in bytes of the method body when serialized.
    /// </summary>
    public uint Size {
      get { return this.size; }
    }
    readonly uint size;
  }

  /// <summary>
  /// An object that can provide information about the local scopes of a method and that can map ILocation objects
  /// to IPrimarySourceLocation objects.
  /// </summary>
  public class ILGeneratorSourceInformationProvider : ILocalScopeProvider, ISourceLocationProvider {

    /// <summary>
    /// Returns zero or more local (block) scopes, each defining an IL range in which an iterator local is defined.
    /// The scopes are returned by the MoveNext method of the object returned by the iterator method.
    /// The index of the scope corresponds to the index of the local. Specifically local scope i corresponds
    /// to the local stored in field &lt;localName&gt;x_i of the class used to store the local values in between
    /// calls to MoveNext.
    /// </summary>
    public virtual IEnumerable<ILocalScope> GetIteratorScopes(IMethodBody methodBody) {
      return Enumerable<ILocalScope>.Empty;
    }

    /// <summary>
    /// Returns zero or more local (block) scopes into which the CLR IL operations in the given method body is organized.
    /// </summary>
    public virtual IEnumerable<ILocalScope> GetLocalScopes(IMethodBody methodBody) {
      var ilGeneratorMethodBody = methodBody as ILGeneratorMethodBody;
      if (ilGeneratorMethodBody == null) return Enumerable<ILocalScope>.Empty;
      return ilGeneratorMethodBody.GetLocalScopes();
    }

    /// <summary>
    /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
    /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
    /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
    /// is for the y.
    /// </summary>
    public virtual IEnumerable<INamespaceScope> GetNamespaceScopes(IMethodBody methodBody) {
      var ilGeneratorMethodBody = methodBody as ILGeneratorMethodBody;
      if (ilGeneratorMethodBody == null) return Enumerable<INamespaceScope>.Empty;
      return ilGeneratorMethodBody.GetNamespaceScopes();
    }

    /// <summary>
    /// Returns zero or more local constant definitions that are local to the given scope.
    /// </summary>
    public virtual IEnumerable<ILocalDefinition> GetConstantsInScope(ILocalScope scope) {
      var ilGeneratorScope = scope as ILGeneratorScope;
      if (ilGeneratorScope == null) return Enumerable<ILocalDefinition>.Empty;
      return ilGeneratorScope.Constants;
    }

    /// <summary>
    /// Returns zero or more local variable definitions that are local to the given scope.
    /// </summary>
    public virtual IEnumerable<ILocalDefinition> GetVariablesInScope(ILocalScope scope) {
      var ilGeneratorScope = scope as ILGeneratorScope;
      if (ilGeneratorScope == null) return Enumerable<ILocalDefinition>.Empty;
      return ilGeneratorScope.Locals;
    }

    /// <summary>
    /// Returns true if the method body is an iterator.
    /// </summary>
    public virtual bool IsIterator(IMethodBody methodBody) {
      return false;
    }

    /// <summary>
    /// If the given method body is the "MoveNext" method of the state class of an asynchronous method, the returned
    /// object describes where synchronization points occur in the IL operations of the "MoveNext" method. Otherwise
    /// the result is null.
    /// </summary>
    public ISynchronizationInformation/*?*/ GetSynchronizationInformation(IMethodBody methodBody) {
      return null;
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to one or more of the given derived (non primary) document locations.
    /// </summary>
    public virtual IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IEnumerable<ILocation> locations) {
      foreach (var location in locations) {
        IPrimarySourceLocation psloc = location as IPrimarySourceLocation;
        if (psloc != null) yield return psloc;
      }
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the given derived (non primary) document location.
    /// </summary>
    public virtual IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(ILocation location) {
      IPrimarySourceLocation psloc = location as IPrimarySourceLocation;
      if (psloc != null) yield return psloc;
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the definition of the given local.
    /// </summary>
    public virtual IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsForDefinitionOf(ILocalDefinition localDefinition) {
      return Enumerable<IPrimarySourceLocation>.Empty;
    }

    /// <summary>
    /// Returns the source name of the given local definition, if this is available.
    /// Otherwise returns the value of the Name property and sets isCompilerGenerated to true.
    /// </summary>
    public virtual string GetSourceNameFor(ILocalDefinition localDefinition, out bool isCompilerGenerated) {
      isCompilerGenerated = false;
      var generatorLocal = localDefinition as GeneratorLocal;
      if (generatorLocal != null) isCompilerGenerated = generatorLocal.IsCompilerGenerated;
      return localDefinition.Name.Value;
    }

  }
}