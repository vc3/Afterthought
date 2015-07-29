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
using Microsoft.Cci.ILGeneratorImplementation;
using System.Diagnostics.Contracts;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci {

  /// <summary>
  /// Rewrites the IL of method bodies.
  /// </summary>
  public class ILRewriter {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="host"></param>
    /// <param name="localScopeProvider"></param>
    /// <param name="sourceLocationProvider"></param>
    public ILRewriter(IMetadataHost host, ILocalScopeProvider/*?*/ localScopeProvider, ISourceLocationProvider/*?*/ sourceLocationProvider) {
      Contract.Requires(host != null);

      this.host = host;
      this.generator = new ILGenerator(host, Dummy.MethodDefinition);
      this.localScopeProvider = localScopeProvider;
      this.sourceLocationProvider = sourceLocationProvider;
    }

    /// <summary>
    /// 
    /// </summary>
    protected readonly IMetadataHost host;
    /// <summary>
    /// 
    /// </summary>
    protected readonly ILocalScopeProvider/*?*/ localScopeProvider;
    /// <summary>
    /// 
    /// </summary>
    protected readonly ISourceLocationProvider/*?*/ sourceLocationProvider;

    ILGenerator generator;
    Hashtable<ILGeneratorLabel> labelFor = new Hashtable<ILGeneratorLabel>();
    readonly HashtableForUintValues<ILocalDefinition> localIndex = new HashtableForUintValues<ILocalDefinition>();
    readonly List<ILocalDefinition> localVariables = new List<ILocalDefinition>();
    readonly Microsoft.Cci.ILGeneratorImplementation.Stack<ILocalScope> scopeStack = new Microsoft.Cci.ILGeneratorImplementation.Stack<ILocalScope>();
    IEnumerator<ILocalScope>/*?*/ scopeEnumerator;
    bool scopeEnumeratorIsValid;
    IEnumerator<ILocalScope>/*?*/ iteratorScopeEnumerator;
    bool iteratorScopeEnumeratorIsValid;
    ISynchronizationInformation/*?*/ synchronizationInfo;
    IEnumerator<ISynchronizationPoint>/*?*/ syncPointEnumerator;
    bool syncPointEnumeratorIsValid;

    /// <summary>
    /// 
    /// </summary>
    protected ushort maxStack;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.host != null);
      Contract.Invariant(this.generator != null);
      Contract.Invariant(this.labelFor != null);
      Contract.Invariant(this.localIndex != null);
      Contract.Invariant(this.localVariables != null);
      Contract.Invariant(this.scopeStack != null);
    }

    /// <summary>
    /// 
    /// </summary>
    protected ILGenerator Generator { 
      get {
        Contract.Ensures(Contract.Result<ILGenerator>() != null);
        return this.generator; 
      } 
    }

    /// <summary>
    /// 
    /// </summary>
    protected IMetadataHost Host {
      get {
        Contract.Ensures(Contract.Result<IMetadataHost>() != null);
        return this.host;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodBody"></param>
    /// <returns></returns>
    public virtual IMethodBody Rewrite(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);

      IMethodDefinition asyncMethodDefinition = null;
      if (this.localScopeProvider != null) {
        this.synchronizationInfo = this.localScopeProvider.GetSynchronizationInformation(methodBody);
        if (this.synchronizationInfo != null) asyncMethodDefinition = this.synchronizationInfo.AsyncMethod;
      }
      this.generator = new ILGenerator(this.host, methodBody.MethodDefinition, asyncMethodDefinition);
      this.maxStack = methodBody.MaxStack;
      this.labelFor.Clear();
      this.localIndex.Clear();
      this.localVariables.Clear();
      this.scopeStack.Count = 0;

      this.EmitMethodBody(methodBody);
      return new ILGeneratorMethodBody(this.Generator, methodBody.LocalsAreZeroed, this.maxStack, methodBody.MethodDefinition,
        this.localVariables.ToArray(), Enumerable<ITypeDefinition>.Empty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodBody"></param>
    [ContractVerification(false)]
    protected virtual void EmitMethodBody(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);

      var savedLabelFor = this.labelFor;
      this.labelFor = new Hashtable<ILGeneratorLabel>();
      var initialScopeStackCount = this.scopeStack.Count;

      foreach (var exceptionInfo in methodBody.OperationExceptionInformation) {
        Contract.Assume(exceptionInfo != null);
        this.Generator.AddExceptionHandlerInformation(exceptionInfo.HandlerKind, exceptionInfo.ExceptionType,
          this.GetLabelFor(exceptionInfo.TryStartOffset), this.GetLabelFor(exceptionInfo.TryEndOffset),
          this.GetLabelFor(exceptionInfo.HandlerStartOffset), this.GetLabelFor(exceptionInfo.HandlerEndOffset),
          exceptionInfo.HandlerKind == HandlerKind.Filter ? this.GetLabelFor(exceptionInfo.FilterDecisionStartOffset) : null);
      }

      if (this.localScopeProvider == null) {
        foreach (var localDef in methodBody.LocalVariables) {
          Contract.Assume(localDef != null);
          this.Generator.AddVariableToCurrentScope(localDef);
        }
      } else {
        foreach (var ns in this.localScopeProvider.GetNamespaceScopes(methodBody)) {
          Contract.Assume(ns != null);
          foreach (var uns in ns.UsedNamespaces) {
            Contract.Assume(uns != null);
            this.Generator.UseNamespace(uns.NamespaceName.Value);
          }
        }
        this.scopeEnumerator = this.localScopeProvider.GetLocalScopes(methodBody).GetEnumerator();
        this.scopeEnumeratorIsValid = this.scopeEnumerator.MoveNext();
        this.iteratorScopeEnumerator = this.localScopeProvider.GetIteratorScopes(methodBody).GetEnumerator();
        this.iteratorScopeEnumeratorIsValid = this.iteratorScopeEnumerator.MoveNext();
        if (this.synchronizationInfo != null) {
          this.syncPointEnumerator = this.synchronizationInfo.SynchronizationPoints.GetEnumerator();
          this.syncPointEnumeratorIsValid = this.syncPointEnumerator.MoveNext();
        }
      }

      foreach (var operation in methodBody.Operations) {
        switch (operation.OperationCode) {
          case OperationCode.Beq:
          case OperationCode.Bge:
          case OperationCode.Bge_Un:
          case OperationCode.Bgt:
          case OperationCode.Bgt_Un:
          case OperationCode.Ble:
          case OperationCode.Ble_Un:
          case OperationCode.Blt:
          case OperationCode.Blt_Un:
          case OperationCode.Bne_Un:
          case OperationCode.Br:
          case OperationCode.Br_S:
          case OperationCode.Brfalse:
          case OperationCode.Brtrue:
          case OperationCode.Leave:
          case OperationCode.Beq_S:
          case OperationCode.Bge_S:
          case OperationCode.Bge_Un_S:
          case OperationCode.Bgt_S:
          case OperationCode.Bgt_Un_S:
          case OperationCode.Ble_S:
          case OperationCode.Ble_Un_S:
          case OperationCode.Blt_S:
          case OperationCode.Blt_Un_S:
          case OperationCode.Bne_Un_S:
          case OperationCode.Brfalse_S:
          case OperationCode.Brtrue_S:
          case OperationCode.Leave_S:
            Contract.Assume(operation.Value is uint);
            this.GetLabelFor((uint)operation.Value);
            break;
          case OperationCode.Switch:
            uint[] offsets = operation.Value as uint[];
            Contract.Assume(offsets != null);
            foreach (var offset in offsets) {
              this.GetLabelFor(offset);
            }
            break;
        }
      }

      foreach (var operation in methodBody.Operations) {
        Contract.Assume(operation != null);
        Contract.Assume(this.labelFor != null);
        var label = this.labelFor.Find(operation.Offset);
        if (label != null) this.Generator.MarkLabel(label);
        this.EmitDebugInformationFor(operation);
        this.EmitOperation(operation);
        this.TrackLocal(operation.Value);
      }

      while (this.scopeStack.Count > initialScopeStackCount) {
        this.Generator.EndScope();
        this.scopeStack.Pop();
      }

      this.labelFor = savedLabelFor;
      Contract.Assume(this.generator != null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operation"></param>
    protected virtual void EmitDebugInformationFor(IOperation operation) {
      Contract.Requires(operation != null);

      this.Generator.MarkSequencePoint(operation.Location);
      if (this.scopeEnumerator == null) return;
      ILocalScope/*?*/ currentScope = null;
      while (this.scopeStack.Count > 0) {
        currentScope = this.scopeStack.Peek();
        Contract.Assume(currentScope != null);
        if (operation.Offset < currentScope.Offset+currentScope.Length) break;
        this.scopeStack.Pop();
        this.Generator.EndScope();
        currentScope = null;
      }
      while (this.scopeEnumeratorIsValid) {
        currentScope = this.scopeEnumerator.Current;
        Contract.Assume(currentScope != null);
        if (currentScope.Offset <= operation.Offset && operation.Offset < currentScope.Offset+currentScope.Length) {
          this.scopeStack.Push(currentScope);
          uint iteratorLocalsInScope = 0;
          if (this.iteratorScopeEnumerator != null) {
            while (this.iteratorScopeEnumeratorIsValid) {
              var iteratorScope = this.iteratorScopeEnumerator.Current;
              Contract.Assume(iteratorScope != null);
              if (iteratorScope.Offset >= currentScope.Offset && iteratorScope.Offset+iteratorScope.Length <= currentScope.Offset+currentScope.Length) {
                iteratorLocalsInScope++;
                this.iteratorScopeEnumeratorIsValid = this.iteratorScopeEnumerator.MoveNext();
              } else {
                break;
              }
            }
          }
          this.Generator.BeginScope(iteratorLocalsInScope);
          Contract.Assume(this.localScopeProvider != null);
          foreach (var local in this.localScopeProvider.GetVariablesInScope(currentScope)) {
            Contract.Assume(local != null);
            Contract.Assume(local.MethodDefinition == this.generator.Method);
            if (!this.localIndex.ContainsKey(local)) {
              this.localIndex.Add(local, (uint)this.localVariables.Count);
              this.localVariables.Add(local);
            }
            this.Generator.AddVariableToCurrentScope(local);
          }
          foreach (var constant in this.localScopeProvider.GetConstantsInScope(currentScope)) {
            Contract.Assume(constant != null);
            Contract.Assume(constant.MethodDefinition == this.generator.Method);
            this.Generator.AddConstantToCurrentScope(constant);
          }
          this.scopeEnumeratorIsValid = this.scopeEnumerator.MoveNext();
        } else
          break;
      }
      if (this.syncPointEnumeratorIsValid) {
        Contract.Assume(this.syncPointEnumerator != null);
        var syncPoint = this.syncPointEnumerator.Current;
        Contract.Assume(syncPoint != null);
        if (syncPoint.SynchronizeOffset == operation.Offset) {
          if (syncPoint.ContinuationMethod == null)
            this.generator.MarkSynchronizationPoint(this.generator.Method, this.GetLabelFor(syncPoint.ContinuationOffset));
          else
            this.generator.MarkSynchronizationPoint(syncPoint.ContinuationMethod, new ILGeneratorLabel() { Offset = syncPoint.ContinuationOffset });
          Contract.Assume(this.syncPointEnumerator != null);
          this.syncPointEnumeratorIsValid = this.syncPointEnumerator.MoveNext();
        }
      }
    }

    /// <summary>
    /// Emits the given operation at the current position of the new IL stream. Also tracks any referenced local definitions,
    /// so that this.localVariables will contain the exact list of locals used in the new method body.
    /// </summary>
    /// <param name="operation"></param>
    protected virtual void EmitOperation(IOperation operation) {
      Contract.Requires(operation != null);

      var operationCode = operation.OperationCode;
      var value = operation.Value;
      switch (operationCode) {
        case OperationCode.Beq:
        case OperationCode.Bge:
        case OperationCode.Bge_Un:
        case OperationCode.Bgt:
        case OperationCode.Bgt_Un:
        case OperationCode.Ble:
        case OperationCode.Ble_Un:
        case OperationCode.Blt:
        case OperationCode.Blt_Un:
        case OperationCode.Bne_Un:
        case OperationCode.Br:
        case OperationCode.Br_S:
        case OperationCode.Brfalse:
        case OperationCode.Brtrue:
        case OperationCode.Leave:
        case OperationCode.Beq_S:
        case OperationCode.Bge_S:
        case OperationCode.Bge_Un_S:
        case OperationCode.Bgt_S:
        case OperationCode.Bgt_Un_S:
        case OperationCode.Ble_S:
        case OperationCode.Ble_Un_S:
        case OperationCode.Blt_S:
        case OperationCode.Blt_Un_S:
        case OperationCode.Bne_Un_S:
        case OperationCode.Brfalse_S:
        case OperationCode.Brtrue_S:
        case OperationCode.Leave_S:
          operationCode = ILGenerator.LongVersionOf(operationCode);
          Contract.Assume(operation.Value is uint);
          value = this.GetLabelFor(+(uint)operation.Value);
          break;
        case OperationCode.Switch:
          uint[] offsets = operation.Value as uint[];
          Contract.Assume(offsets != null);
          var n = offsets.Length;
          ILGeneratorLabel[] labels = new ILGeneratorLabel[n];
          for (int i = 0; i < n; i++) {
            var offset = offsets[i];
            labels[i] = this.GetLabelFor(offset);
          }
          value = labels;
          break;

        //Avoid the short forms because the locals can get reordered.
        case OperationCode.Ldloc_0:
        case OperationCode.Ldloc_1:
        case OperationCode.Ldloc_2:
        case OperationCode.Ldloc_3:
        case OperationCode.Ldloc_S:
          operationCode = OperationCode.Ldloc;
          break;

        case OperationCode.Ldloca_S:
          operationCode = OperationCode.Ldloca;
          break;

        case OperationCode.Stloc_0:
        case OperationCode.Stloc_1:
        case OperationCode.Stloc_2:
        case OperationCode.Stloc_3:
        case OperationCode.Stloc_S:
          operationCode = OperationCode.Stloc;
          break;
      }
      this.generator.Emit(operationCode, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operationValue"></param>
    protected void TrackLocal(object operationValue) {
      var local = operationValue as ILocalDefinition;
      if (local != null) {
        if (!this.localIndex.ContainsKey(local)) {
          this.localIndex.Add(local, (uint)this.localVariables.Count);
          this.localVariables.Add(local);
        }
      }
    }

    /// <summary>
    /// Returns a label that represents the given offset in the original IL. This label must be marked
    /// at the corresponding location in the rewritten IL.
    /// </summary>
    protected virtual ILGeneratorLabel GetLabelFor(uint offset) {
      Contract.Ensures(Contract.Result<ILGeneratorLabel>() != null);

      var result = this.labelFor[offset];
      if (result == null)
        this.labelFor[offset] = result = new ILGeneratorLabel();
      return result;
    }    

  }

}
