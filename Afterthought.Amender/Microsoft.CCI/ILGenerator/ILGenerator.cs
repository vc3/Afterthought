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

namespace Microsoft.Cci {
  using Microsoft.Cci.ILGeneratorImplementation;
  using System.Diagnostics.Contracts;

  /// <summary>
  /// Generates Microsoft intermediate language (MSIL) instructions.
  /// </summary>
  /// <remarks>
  /// A typical use of the ILGenerator class is to produce the values of the properties of an IMethodBody object.
  /// This could be a SourceMethodBody from the mutable Code Model or an instance of ILGeneratorMethodBody (or perhaps a derived class).
  /// In the former case, the ILGenerator is a part of the private state of an instance of CodeModelToILConverter (or a derived class) that
  /// is usually part of a code model mutator. In the latter case, the ILGenerator may have been used to generate a method body for which no
  /// Code Model has been built.
  /// </remarks>
  public class ILGenerator {

    List<ExceptionHandler> handlers = new List<ExceptionHandler>();
    IMetadataHost host;
    IMethodDefinition method;
    ILocation location = Dummy.Location;
    ILocation expressionLocation;
    uint offset;
    List<Operation> operations = new List<Operation>();
    List<ILocalScope>/*?*/ iteratorScopes;
    List<ILGeneratorScope> scopes = new List<ILGeneratorScope>();
    Stack<ILGeneratorScope> scopeStack = new Stack<ILGeneratorScope>();
    Stack<TryBody> tryBodyStack = new Stack<TryBody>();
    List<SynchronizationPoint>/*?*/ synchronizationPoints;
    IMethodDefinition/*?*/ asyncMethodDefinition;

    /// <summary>
    /// Allocates an object that helps with the generation of Microsoft intermediate language (MSIL) instructions corresponding to a method body.
    /// </summary>
    /// <param name="host">Provides a standard abstraction over the applications that host components that provide or consume objects from the metadata model.</param>
    /// <param name="methodDefinition">The method to generate MSIL for.</param>
    /// <param name="asyncMethodDefinition">The async method for which this generator will generate the "MoveNext" method of its state class.</param>
    public ILGenerator(IMetadataHost host, IMethodDefinition methodDefinition, IMethodDefinition/*?*/ asyncMethodDefinition = null) {
      Contract.Requires(host != null);
      Contract.Requires(methodDefinition != null);

      this.host = host;
      this.method = methodDefinition;
      this.asyncMethodDefinition = asyncMethodDefinition;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.host != null);
      Contract.Invariant(this.method != null);
      Contract.Invariant(this.location != null);
      Contract.Invariant(this.handlers != null);
      Contract.Invariant(this.operations != null);
      Contract.Invariant(this.scopes != null);
      Contract.Invariant(this.scopeStack != null);
      Contract.Invariant(this.tryBodyStack != null);
    }

    /// <summary>
    /// Adds the given local constant to the current lexical scope.
    /// </summary>
    /// <param name="local">The local constant to add to the current scope.</param>
    public void AddConstantToCurrentScope(ILocalDefinition local) {
      Contract.Requires(local != null);
      //Contract.Assume(local.MethodDefinition == this.Method);

      if (this.scopeStack.Count == 0) this.BeginScope();
      var topScope = this.scopeStack.Peek();
      Contract.Assume(topScope != null);
      Contract.Assume(topScope.constants != null);
      topScope.constants.Add(local);
    }

    /// <summary>
    /// Adds the given local variable to the current lexical scope.
    /// </summary>
    /// <param name="local">The local variable to add to the current scope.</param>
    public void AddVariableToCurrentScope(ILocalDefinition local) {
      Contract.Requires(local != null);
      //Contract.Assume(local.MethodDefinition == this.Method);

      if (this.scopeStack.Count == 0) this.BeginScope();
      var topScope = this.scopeStack.Peek();
      Contract.Assume(topScope != null);
      Contract.Assume(topScope.locals != null);
      topScope.locals.Add(local);
    }

    /// <summary>
    /// Adds an exception handler entry to the generated body. This intended for IL rewriting scenarios and should not be used in conjunction with methods such as BeginCatchBlock,
    /// which are intended for CodeModel to IL generation scenarios. The calls to AddExceptionHandler should result in a list of handlers that 
    /// satisfies CLI rules with respect to ordering and nesting.
    /// </summary>
    /// <param name="kind">The kind of handler being added.</param>
    /// <param name="exceptionType">The type of exception this handler will handle (may be Dummy.TypeReference).</param>
    /// <param name="tryStart">A label identifying the first instruction in the try body.</param>
    /// <param name="tryEnd">A label identifying the first instruction following the try body.</param>
    /// <param name="handlerStart">A label identifying the first instruction in the handler body.</param>
    /// <param name="handlerEnd">A label identifying the first instruction following the handler body.</param>
    /// <param name="filterStart">A label identifying the first instruction of the filter decision block. May be null.</param>
    public void AddExceptionHandlerInformation(HandlerKind kind, ITypeReference exceptionType, ILGeneratorLabel tryStart, ILGeneratorLabel tryEnd,
      ILGeneratorLabel handlerStart, ILGeneratorLabel handlerEnd, ILGeneratorLabel/*?*/ filterStart) {
      Contract.Requires(exceptionType != null);
      Contract.Requires(tryStart != null);
      Contract.Requires(tryEnd != null);
      Contract.Requires(handlerStart != null);
      Contract.Requires(handlerEnd != null);

      var handler = new ExceptionHandler() {
        Kind = kind, ExceptionType = exceptionType, TryStart = tryStart, TryEnd = tryEnd,
        HandlerStart = handlerStart, HandlerEnd = handlerEnd, FilterDecisionStart = filterStart
      };
      this.handlers.Add(handler);
    }

    /// <summary>
    /// Performs one or more extra passes over the list of operations, changing long branches to short if possible and short branches to
    /// long branches if necessary.
    /// </summary>
    /// <remarks>If any long branches in this.operations could have been short, they are adjusted to be short. 
    /// This can result in an updated version of this.operations where some branches that had to be long in the previous
    /// version can now be short as well. Consequently, the adjustment process iterates until no further changes are possible.
    /// Note that all decisions are made based on the offsets at the start of an iteration. </remarks>
    public void AdjustBranchSizesToBestFit(bool eliminateBranchesToNext = false) {
      int adjustment;
      uint numberOfAdjustments;
      do {
        adjustment = 0;
        numberOfAdjustments = 0;
        for (int i = 0, n = this.operations.Count; i < n; i++) {
          Operation operation = this.operations[i];
          Contract.Assume(operation != null);
          uint oldOffset = operation.offset;
          uint newOffset = (uint)(((int)oldOffset) + adjustment);
          operation.offset = newOffset;
          ILGeneratorLabel/*?*/ label = operation.value as ILGeneratorLabel;
          if (label != null) {
            if (operation.OperationCode == OperationCode.Invalid) {
              //Dummy operation that serves as label definition.
              label.Offset = operation.offset;
              continue;
            }
            //For backward branches, this test will compare the new offset of the label with the old offset of the current
            //instruction. This is OK, because the new offset of the label will be less than or equal to its old offset.
            bool isForwardBranch = label.Offset >= oldOffset;
            // Short offsets are calculated from the start of the instruction *after* the current instruction, which takes up 2 bytes
            // (1 for the opcode and 1 for the signed byte).
            bool shortOffsetOk = isForwardBranch ? label.Offset-oldOffset <= 129 : newOffset-label.Offset <= 126;
            OperationCode oldOpCode = operation.OperationCode;
            if (shortOffsetOk) {
              operation.operationCode = ShortVersionOf(operation.OperationCode);
              if (operation.operationCode != oldOpCode) { numberOfAdjustments++; adjustment -= 3; }
            } else {
              if (operation.operationCode != LongVersionOf(operation.operationCode))
                throw new InvalidOperationException(); //A short branch was specified for an offset that is long.
              //The test for isForwardBranch depends on label offsets only decreasing, so it is not an option to replace the short branch with a long one.
            }
            if (eliminateBranchesToNext && operation.OperationCode == OperationCode.Br_S && operation.offset+2 == label.Offset) {
              //eliminate branch to the next instruction
              operation.operationCode = OperationCode.Invalid;
              numberOfAdjustments++; adjustment -= 2;
            }
          }
        }
      } while (numberOfAdjustments > 0);
    }

    /// <summary>
    /// Begins a catch block.
    /// </summary>
    /// <param name="exceptionType">The Type object that represents the exception.</param>
    public void BeginCatchBlock(ITypeReference exceptionType) {
      Contract.Requires(exceptionType != null);
      Contract.Requires(this.InTryBody);

      ExceptionHandler handler = this.BeginHandler(HandlerKind.Catch);
      handler.ExceptionType = exceptionType;
    }

    /// <summary>
    /// Begins an exception block for a filtered exception. See also BeginFilterBody.
    /// </summary>
    public void BeginFilterBlock() {
      Contract.Requires(this.InTryBody);

      ExceptionHandler handler = this.BeginHandler(HandlerKind.Filter);
      handler.FilterDecisionStart = handler.HandlerStart;
    }

    /// <summary>
    /// Begins the part of a filter handler that is invoked on the second pass if the filter condition returns true on the first pass.
    /// </summary>
    public void BeginFilterBody() {
      Contract.Requires(this.InTryBody);

      this.Emit(OperationCode.Endfilter);
      ILGeneratorLabel handlerStart = new ILGeneratorLabel(false);
      this.MarkLabel(handlerStart);
      Contract.Assume(this.handlers.Count > 0);
      var handler = this.handlers[this.handlers.Count-1];
      Contract.Assume(handler != null);
      handler.HandlerStart = handlerStart;
    }

    private ExceptionHandler BeginHandler(HandlerKind kind) {
      Contract.Requires(this.InTryBody);
      Contract.Ensures(Contract.Result<ExceptionHandler>() != null);

      ILGeneratorLabel handlerStart = new ILGeneratorLabel(false);
      this.MarkLabel(handlerStart);
      TryBody currentTryBody = this.tryBodyStack.Peek();
      Contract.Assume(currentTryBody != null);
      ExceptionHandler handler = new ExceptionHandler(kind, currentTryBody, handlerStart);
      if (currentTryBody.end == null)
        currentTryBody.end = handlerStart;
      else if (this.handlers.Count > 0) {
        for (int i = this.handlers.Count-1; i >= 0; i--) {
          var handleri = this.handlers[i];
          Contract.Assume(handleri != null);
          if (handleri.HandlerEnd == null) {
            handleri.HandlerEnd = handlerStart;
            break;
          }
        }
      }
      this.handlers.Add(handler);
      return handler;
    }

    /// <summary>
    /// Begins the body of a try statement.
    /// </summary>
    public void BeginTryBody() {
      ILGeneratorLabel tryBodyStart = new ILGeneratorLabel(false);
      this.MarkLabel(tryBodyStart);
      this.tryBodyStack.Push(new TryBody(tryBodyStart));
    }

    /// <summary>
    ///  Begins an exception fault block in the Microsoft intermediate language (MSIL) stream.
    /// </summary>
    public void BeginFaultBlock() {
      Contract.Requires(this.InTryBody);

      this.BeginHandler(HandlerKind.Fault);
    }

    /// <summary>
    /// Begins a finally block in the Microsoft intermediate language (MSIL) instruction stream.
    /// </summary>
    public void BeginFinallyBlock() {
      Contract.Requires(this.InTryBody);

      this.BeginHandler(HandlerKind.Finally);
    }

    /// <summary>
    /// Begins a lexical scope.
    /// </summary>
    public void BeginScope() {
      var startLabel = new ILGeneratorLabel();
      this.MarkLabel(startLabel);
      ILGeneratorScope scope = new ILGeneratorScope(startLabel, this.host.NameTable, this.method);
      this.scopeStack.Push(scope);
      this.scopes.Add(scope);
    }

    /// <summary>
    /// Begins a lexical scope.
    /// </summary>
    public void BeginScope(uint numberOfIteratorLocalsInScope) {
      var startLabel = new ILGeneratorLabel();
      this.MarkLabel(startLabel);
      ILGeneratorScope scope = new ILGeneratorScope(startLabel, this.host.NameTable, this.method);
      this.scopeStack.Push(scope);
      this.scopes.Add(scope);
      if (numberOfIteratorLocalsInScope == 0) return;
      if (this.iteratorScopes == null) this.iteratorScopes = new List<ILocalScope>();
      while (numberOfIteratorLocalsInScope-- > 0) {
        this.iteratorScopes.Add(scope);
      }
    }

    /// <summary>
    /// The offset in the IL stream where the next instruction will be emitted.
    /// </summary>
    public uint CurrentOffset {
      get { return this.offset; }
    }

    /// <summary>
    /// Puts the specified instruction onto the stream of instructions.
    /// </summary>
    /// <param name="opcode">The Intermediate Language (IL) instruction to be put onto the stream.</param>
    public void Emit(OperationCode opcode) {
      var loc = this.GetCurrentSequencePoint();
      if (opcode == OperationCode.Ret) {
        int i = this.operations.Count;
        while (--i >= 0) {
          Operation previousOp = this.operations[i];
          Contract.Assume(previousOp != null);
          if (previousOp.OperationCode != OperationCode.Invalid) break;
          Contract.Assume(previousOp.value is ILGeneratorLabel);
          ILGeneratorLabel labelOfBranch = (ILGeneratorLabel)previousOp.value;
          labelOfBranch.locationOfReturnInstruction = loc;
        }
      }
      this.operations.Add(new Operation(opcode, this.offset, loc, null));
      this.offset += SizeOfOperationCode(opcode);
    }

    /// <summary>
    /// Calls a type specific overload of Emit, based on the runtime type of the given object. Use this when copying IL operations.
    /// </summary>
    /// <param name="opcode">The Intermediate Language (IL) instruction to be put onto the stream.</param>
    /// <param name="value">An argument that parameterizes the IL instruction at compile time (not a runtime operand for the instruction).</param>
    [ContractVerification(false)]
    public void Emit(OperationCode opcode, object value) {
      // For the following one-byte opcodes, the reader supplies a value to make it easier for clients.
      // But that messes up the dispatch in the succeeding switch statement.
      switch (opcode) {
        case OperationCode.Ldc_I4_0:
        case OperationCode.Ldc_I4_1:
        case OperationCode.Ldc_I4_2:
        case OperationCode.Ldc_I4_3:
        case OperationCode.Ldc_I4_4:
        case OperationCode.Ldc_I4_5:
        case OperationCode.Ldc_I4_6:
        case OperationCode.Ldc_I4_7:
        case OperationCode.Ldc_I4_8:
        case OperationCode.Ldc_I4_M1:
          this.Emit(opcode);
          return;
      }
      switch (System.Convert.GetTypeCode(value)) {
        case TypeCode.Byte: this.Emit(opcode, (byte)value); break;
        case TypeCode.Double: this.Emit(opcode, (double)value); break;
        case TypeCode.Single: this.Emit(opcode, (float)value); break;
        case TypeCode.Int32: this.Emit(opcode, (int)value); break;
        case TypeCode.Int64: this.Emit(opcode, (long)value); break;
        case TypeCode.SByte: this.Emit(opcode, (sbyte)value); break;
        case TypeCode.Int16: this.Emit(opcode, (short)value); break;
        case TypeCode.String: this.Emit(opcode, (string)value); break;
        case TypeCode.Empty: this.Emit(opcode); break;

        default:
          var fieldReference = value as IFieldReference;
          if (fieldReference != null) {
            this.Emit(opcode, fieldReference);
            break;
          }
          var label = value as ILGeneratorLabel;
          if (label != null) {
            this.Emit(opcode, label);
            break;
          }
          var labels = value as ILGeneratorLabel[];
          if (labels != null) {
            this.Emit(opcode, labels);
            break;
          }
          var local = value as ILocalDefinition;
          if (local != null) {
            this.Emit(opcode, local);
            break;
          }
          var meth = value as IMethodReference;
          if (meth != null) {
            this.Emit(opcode, meth);
            break;
          }
          var param = value as IParameterDefinition;
          if (param != null) {
            this.Emit(opcode, param);
            break;
          }
          var sig = value as ISignature;
          if (sig != null) {
            this.Emit(opcode, sig);
            break;
          }
          var type = value as ITypeReference;
          if (type != null) {
            this.Emit(opcode, type);
            break;
          }
          throw new InvalidOperationException();
      }
    }

    /// <summary>
    /// Puts the specified instruction and unsigned 8 bit integer argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The unsigned 8 bit integer argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, byte arg) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), arg));
      this.offset += SizeOfOperationCode(opcode)+1;
    }

    /// <summary>
    /// Puts the specified instruction and 64 bit floating point argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The 64 bit floating point argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, double arg) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), arg));
      this.offset += SizeOfOperationCode(opcode)+8;
    }

    /// <summary>
    /// Puts the specified instruction and a field reference onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="field">A reference to a field.</param>
    public void Emit(OperationCode opcode, IFieldReference field) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), field));
      this.offset += SizeOfOperationCode(opcode)+4;
    }

    /// <summary>
    /// Puts the specified instruction and 32 bit floating point argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The 32 bit floating point argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, float arg) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), arg));
      this.offset += SizeOfOperationCode(opcode)+4;
    }

    /// <summary>
    /// Puts the specified instruction and 32 bit integer argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The 32 bit integer argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, int arg) {
      if (opcode == OperationCode.Ldc_I4_S) {
        sbyte b = (sbyte)arg;
        if (b == arg) {
          this.Emit(opcode, b);
          return;
        }
        opcode = OperationCode.Ldc_I4;
      }
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), arg));
      this.offset += SizeOfOperationCode(opcode)+4;
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream and leaves space to include a label when fixes are done.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="label">The label to which to branch from this location.</param>
    public void Emit(OperationCode opcode, ILGeneratorLabel label) {
      Contract.Requires(label != null);

      if (opcode == OperationCode.Br && this.operations.Count > 0) {
        Operation previousOp = this.operations[this.operations.Count-1];
        Contract.Assume(previousOp != null);
        if (previousOp.OperationCode == OperationCode.Invalid) {
          Contract.Assume(previousOp.value is ILGeneratorLabel);
          ILGeneratorLabel labelOfBranch = (ILGeneratorLabel)previousOp.value;
          if (labelOfBranch.mayAlias) labelOfBranch.alias = label;
        }
      }
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), label));
      this.offset += SizeOfOperationCode(opcode)+SizeOfOffset(opcode);
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream and leaves space to include an array of labels when fixes are done.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="labels">An array of labels to which to branch from this location.</param>
    public void Emit(OperationCode opcode, params ILGeneratorLabel[] labels) {
      Contract.Requires(labels != null);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), labels));
      this.offset += SizeOfOperationCode(opcode)+4*((uint)labels.Length+1);
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the index of the given local variable.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="local">A local variable.</param>
    public void Emit(OperationCode opcode, ILocalDefinition local) {
      Contract.Requires(local != null);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), local));
      this.offset += SizeOfOperationCode(opcode);
      if (opcode == OperationCode.Ldloc_S || opcode == OperationCode.Ldloca_S || opcode == OperationCode.Stloc_S)
        this.offset += 1;
      else if (opcode == OperationCode.Ldloc || opcode == OperationCode.Ldloca || opcode == OperationCode.Stloc)
        this.offset += 2;
    }

    /// <summary>
    /// Puts the specified instruction and 64 bit integer argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The 64 bit integer argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, long arg) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), arg));
      this.offset += SizeOfOperationCode(opcode)+8;
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by a token for the given method reference.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="meth">A reference to a method. Generic methods can only be referenced via instances.</param>
    public void Emit(OperationCode opcode, IMethodReference meth) {
      Contract.Requires(meth != null);
      Contract.Requires(meth.GenericParameterCount == 0 || meth is IGenericMethodInstanceReference);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), meth));
      this.offset += SizeOfOperationCode(opcode)+4;
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the index of the given local variable.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="parameter">A parameter definition. May be null.</param>
    public void Emit(OperationCode opcode, IParameterDefinition/*?*/ parameter) {
      //Contract.Requires(parameter == null || parameter.ContainingSignature == this.Method);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), parameter));
      this.offset += SizeOfOperationCode(opcode);
      if (opcode == OperationCode.Ldarg_S || opcode == OperationCode.Ldarga_S || opcode == OperationCode.Starg_S)
        this.offset += 1;
      else if (opcode == OperationCode.Ldarg || opcode == OperationCode.Ldarga || opcode == OperationCode.Starg)
        this.offset += 2;
    }

    /// <summary>
    /// Puts the specified instruction and signed 8 bit integer argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The signed 8 bit integer argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, sbyte arg) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), (int)arg));
      this.offset += SizeOfOperationCode(opcode)+1;
    }

    /// <summary>
    /// Puts the specified instruction and signed 16 bit integer argument onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="arg">The signed 8 bit integer argument pushed onto the stream immediately after the instruction.</param>
    public void Emit(OperationCode opcode, short arg) {
      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), arg));
      this.offset += SizeOfOperationCode(opcode)+2;
    }

    /// <summary>
    /// Puts the specified instruction and a token for the given signature onto the Microsoft intermediate language (MSIL) stream of instructions.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="signature">The signature of the method or function pointer to call. Can include information about extra arguments.</param>
    public void Emit(OperationCode opcode, ISignature signature) {
      Contract.Requires(signature != null);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), signature));
      this.offset += SizeOfOperationCode(opcode)+4;
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the a token for the given string.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="str">The String to be emitted.</param>
    public void Emit(OperationCode opcode, string str) {
      Contract.Requires(str != null);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), str));
      this.offset += SizeOfOperationCode(opcode)+4;
    }

    /// <summary>
    /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the a token for the referenced type.
    /// </summary>
    /// <param name="opcode">The Microsoft intermediate language (MSIL) instruction to be put onto the stream.</param>
    /// <param name="cls">The referenced type.</param>
    public void Emit(OperationCode opcode, ITypeReference cls) {
      Contract.Requires(cls != null);

      this.operations.Add(new Operation(opcode, this.offset, this.GetCurrentSequencePoint(), cls));
      this.offset += SizeOfOperationCode(opcode)+SizeOfOffset(opcode);
    }

    /// <summary>
    /// Ends a try body.
    /// </summary>
    public void EndTryBody() {
      Contract.Requires(this.InTryBody);

      this.tryBodyStack.Pop();
      if (this.handlers.Count > 0) {
        ILGeneratorLabel handlerEnd = new ILGeneratorLabel(false);
        this.MarkLabel(handlerEnd);
        for (int i = this.handlers.Count-1; i >= 0; i--) {
          var handler = this.handlers[i];
          Contract.Assume(handler != null);
          if (handler.HandlerEnd == null) {
            handler.HandlerEnd = handlerEnd;
            if (i < this.handlers.Count-1) {
              this.handlers.RemoveAt(i);
              this.handlers.Add(handler);
            }
            break;
          }
        }
      }
    }

    /// <summary>
    /// Ends a lexical scope.
    /// </summary>
    public void EndScope() {
      if (this.scopeStack.Count > 0) {
        var endLabel = new ILGeneratorLabel();
        this.MarkLabel(endLabel);
        var topScope = this.scopeStack.Pop();
        Contract.Assume(topScope != null);
        topScope.CloseScope(endLabel);
      }
    }

    private ILocation GetCurrentSequencePoint() {
      Contract.Ensures(Contract.Result<ILocation>() != null);

      ILocation result;
      if (this.expressionLocation != null) {
        result = this.expressionLocation;
        this.expressionLocation = null;
      } else {
        result = this.location;
        this.location = Dummy.Location;
      }
      return result;
    }

    /// <summary>
    /// True if the ILGenerator is currently inside the body of a try statement.
    /// </summary>
    public bool InTryBody {
      get { return this.tryBodyStack.Count > 0; }
    }

    /// <summary>
    /// Marks the next IL operation as the final instruction of an expression, whose location is known and is provided as the argument.
    /// </summary>
    /// <param name="expressionLocation">The location of the expression whose value will be computed by the next IL instruction.</param>
    public void MarkExpressionLocation(ILocation expressionLocation) {
      Contract.Requires(expressionLocation != null);

      this.expressionLocation = expressionLocation;
    }

    /// <summary>
    ///  Marks the Microsoft intermediate language (MSIL) stream's current position with the given label.
    /// </summary>
    public void MarkLabel(ILGeneratorLabel label) {
      Contract.Requires(label != null);

      Contract.Assume(label.Offset == 0);
      label.Offset = this.offset;
      this.operations.Add(new Operation(OperationCode.Invalid, this.offset, Dummy.Location, label));
    }

    /// <summary>
    /// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
    /// </summary>
    /// <param name="location">The location of the sequence point.</param>
    public void MarkSequencePoint(ILocation location) {
      Contract.Requires(location != null);

      this.location = location;
    }

    /// <summary>
    /// Marks the offset of the next IL operation as the first of a sequence of instructions that will cause the thread executing the "MoveNext" method
    /// of the state class of an async method to await the completion of a call to another async method.
    /// </summary>
    /// <param name="continuationMethod">The helper method that will execute once the execution of the current async method resumes.
    /// Usually this will be the same method as the one for which this generator is generating a body, but this is not required.</param>
    /// <param name="continuationLabel">The label inside the continuation method where execution will resume. It should be marked by the
    /// time this generator is being used to construct a method body.</param>
    public void MarkSynchronizationPoint(IMethodDefinition continuationMethod, ILGeneratorLabel continuationLabel) {
      Contract.Requires(continuationMethod != null);
      Contract.Requires(continuationLabel != null);

      var syncPoints = this.synchronizationPoints;
      if (syncPoints == null) this.synchronizationPoints = syncPoints = new List<SynchronizationPoint>();
      var labelForCurrentOffset = new ILGeneratorLabel();
      this.MarkLabel(labelForCurrentOffset);
      syncPoints.Add(
        new SynchronizationPoint() {
          startOfSynchronize = labelForCurrentOffset,
          continuationMethod = continuationMethod != this.method ? continuationMethod : null,
          startOfContinuation = continuationLabel
        });
    }

    /// <summary>
    /// The method for which this generator helps to produce a method body.
    /// </summary>
    public IMethodDefinition Method {
      get {
        Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
        return this.method;
      }
    }

    /// <summary>
    /// If the given operation code is a short branch, return the corresponding long branch. Otherwise return the given operation code.
    /// </summary>
    /// <param name="operationCode">An operation code.</param>
    public static OperationCode LongVersionOf(OperationCode operationCode) {
      switch (operationCode) {
        case OperationCode.Beq_S: return OperationCode.Beq;
        case OperationCode.Bge_S: return OperationCode.Bge;
        case OperationCode.Bge_Un_S: return OperationCode.Bge_Un;
        case OperationCode.Bgt_S: return OperationCode.Bgt;
        case OperationCode.Bgt_Un_S: return OperationCode.Bgt_Un;
        case OperationCode.Ble_S: return OperationCode.Ble;
        case OperationCode.Ble_Un_S: return OperationCode.Ble_Un;
        case OperationCode.Blt_S: return OperationCode.Blt;
        case OperationCode.Blt_Un_S: return OperationCode.Blt_Un;
        case OperationCode.Bne_Un_S: return OperationCode.Bne_Un;
        case OperationCode.Br_S: return OperationCode.Br;
        case OperationCode.Brfalse_S: return OperationCode.Brfalse;
        case OperationCode.Brtrue_S: return OperationCode.Brtrue;
        case OperationCode.Leave_S: return OperationCode.Leave;
        default: return operationCode;
      }
    }

    /// <summary>
    /// If the given operation code is a long branch, return the corresponding short branch. Otherwise return the given operation code.
    /// </summary>
    /// <param name="operationCode">An operation code.</param>
    public static OperationCode ShortVersionOf(OperationCode operationCode) {
      switch (operationCode) {
        case OperationCode.Beq: return OperationCode.Beq_S;
        case OperationCode.Bge: return OperationCode.Bge_S;
        case OperationCode.Bge_Un: return OperationCode.Bge_Un_S;
        case OperationCode.Bgt: return OperationCode.Bgt_S;
        case OperationCode.Bgt_Un: return OperationCode.Bgt_Un_S;
        case OperationCode.Ble: return OperationCode.Ble_S;
        case OperationCode.Ble_Un: return OperationCode.Ble_Un_S;
        case OperationCode.Blt: return OperationCode.Blt_S;
        case OperationCode.Blt_Un: return OperationCode.Blt_Un_S;
        case OperationCode.Bne_Un: return OperationCode.Bne_Un_S;
        case OperationCode.Br: return OperationCode.Br_S;
        case OperationCode.Brfalse: return OperationCode.Brfalse_S;
        case OperationCode.Brtrue: return OperationCode.Brtrue_S;
        case OperationCode.Leave: return OperationCode.Leave_S;
        default: return operationCode;
      }
    }

    private static uint SizeOfOffset(OperationCode opcode) {
      switch (opcode) {
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
        case OperationCode.Br_S:
        case OperationCode.Brfalse_S:
        case OperationCode.Brtrue_S:
        case OperationCode.Leave_S:
          return 1;
        default:
          return 4;
      }
    }

    private static uint SizeOfOperationCode(OperationCode opcode) {
      if (((int)opcode) > 0xff && (opcode < OperationCode.Array_Create)) return 2;
      return 1;
    }

    /// <summary>
    /// Specifies a namespace to be search when evaluating expressions while stopped in the debugger at a sequence point in the current lexical scope.
    /// </summary>
    public void UseNamespace(string namespaceToUse) {
      if (this.scopeStack.Count == 0) this.BeginScope();
      var topScope = this.scopeStack.Peek();
      Contract.Assume(topScope != null);
      Contract.Assume(topScope.usedNamespaces != null);
      topScope.usedNamespaces.Add(namespaceToUse);
    }

    /// <summary>
    /// Returns a block scope associated with each local variable in the iterator for which this is the generator for its MoveNext method.
    /// May return null.
    /// </summary>
    /// <remarks>The PDB file model seems to be that scopes are duplicated if necessary so that there is a separate scope for each
    /// local variable in the original iterator and the mapping from local to scope is done by position.</remarks>
    public IEnumerable<ILocalScope>/*?*/ GetIteratorScopes() {
      if (this.iteratorScopes == null) return null;
      return this.iteratorScopes.AsReadOnly();
    }

    /// <summary>
    /// Returns a sequence of all of the block scopes that have been defined for this method body. Includes nested block scopes.
    /// </summary>
    public IEnumerable<ILGeneratorScope> GetLocalScopes() {
      Contract.Ensures(Contract.Result<IEnumerable<ILGeneratorScope>>() != null);
      return this.scopes.AsReadOnly();
    }

    /// <summary>
    /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
    /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
    /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
    /// is for the y.
    /// </summary>
    public IEnumerable<INamespaceScope> GetNamespaceScopes() {
      Contract.Ensures(Contract.Result<IEnumerable<INamespaceScope>>() != null);
      foreach (var generatorScope in this.scopes) {
        if (generatorScope.usedNamespaces.Count > 0)
          yield return generatorScope;
      }
    }

    /// <summary>
    /// Returns a sequence of all of the IL operations that make up this method body.
    /// </summary>
    public IEnumerable<IOperation> GetOperations() {
      Contract.Ensures(Contract.Result<IEnumerable<IOperation>>() != null);
      foreach (Operation operation in this.operations) {
        if (operation.OperationCode == OperationCode.Invalid) continue; //dummy operation for label
        yield return operation;
      }
    }

    /// <summary>
    /// Returns a sequence of descriptors that define where try blocks and their associated handlers can be found in the instruction sequence.
    /// </summary>
    [ContractVerification(false)]
    public IEnumerable<IOperationExceptionInformation> GetOperationExceptionInformation() {
      Contract.Ensures(Contract.Result<IEnumerable<IOperationExceptionInformation>>() != null);
      return IteratorHelper.GetConversionEnumerable<ExceptionHandler, IOperationExceptionInformation>(this.handlers);
    }

    /// <summary>
    /// Returns an object that describes where synchronization points occur in the IL operations of the "MoveNext" method of the state class of
    /// an asynchronous method. This returns null unless the generator has been supplied with an non null value for asyncMethodDefinition parameter
    /// during construction.
    /// </summary>
    public ISynchronizationInformation/*?*/ GetSynchronizationInformation() {
      if (this.asyncMethodDefinition == null) return null;
      if (this.synchronizationPoints != null) this.synchronizationPoints.TrimExcess();
      uint generatedCatchHandlerOffset = uint.MaxValue;
      if (this.asyncMethodDefinition.Type.TypeCode == PrimitiveTypeCode.Void && this.handlers.Count > 0) {
        Contract.Assume(this.handlers[0] != null && this.handlers[0].HandlerStart != null);
        generatedCatchHandlerOffset = this.handlers[0].HandlerStart.Offset;
      }
      return new SynchronizationInformation() {
        asyncMethod = this.asyncMethodDefinition,
        moveNextMethod = this.method,
        generatedCatchHandlerOffset = generatedCatchHandlerOffset,
        synchronizationPoints = this.synchronizationPoints == null ? Enumerable<ISynchronizationPoint>.Empty : this.synchronizationPoints.AsReadOnly()
      };
    }

    /// <summary>
    /// An object that can provide information about the local scopes of a method.
    /// </summary>
    public class LocalScopeProvider : ILocalScopeProvider {

      /// <summary>
      /// An object that can provide information about the local scopes of a method.
      /// </summary>
      /// <param name="originalLocalScopeProvider">The local scope provider to use for methods that have not been decompiled.</param>
      public LocalScopeProvider(ILocalScopeProvider originalLocalScopeProvider) {
        Contract.Requires(originalLocalScopeProvider != null);

        this.originalLocalScopeProvider = originalLocalScopeProvider;
      }

      ILocalScopeProvider originalLocalScopeProvider;

      [ContractInvariantMethod]
      private void ObjectInvariant() {
        Contract.Invariant(this.originalLocalScopeProvider != null);
      }

      #region ILocalScopeProvider Members

      /// <summary>
      /// Returns zero or more local (block) scopes, each defining an IL range in which an iterator local is defined.
      /// The scopes are returned by the MoveNext method of the object returned by the iterator method.
      /// The index of the scope corresponds to the index of the local. Specifically local scope i corresponds
      /// to the local stored in field &lt;localName&gt;x_i of the class used to store the local values in between
      /// calls to MoveNext.
      /// </summary>
      public IEnumerable<ILocalScope> GetIteratorScopes(IMethodBody methodBody) {
        var sourceMethodBody = methodBody as ILGeneratorMethodBody;
        if (sourceMethodBody == null) return this.originalLocalScopeProvider.GetIteratorScopes(methodBody);
        return sourceMethodBody.GetIteratorScopes()??Enumerable<ILocalScope>.Empty;
      }

      /// <summary>
      /// Returns zero or more local (block) scopes into which the CLR IL operations in the given method body is organized.
      /// </summary>
      /// <param name="methodBody"></param>
      /// <returns></returns>
      public IEnumerable<ILocalScope> GetLocalScopes(IMethodBody methodBody) {
        var sourceMethodBody = methodBody as ILGeneratorMethodBody;
        if (sourceMethodBody == null) return this.originalLocalScopeProvider.GetLocalScopes(methodBody);
        return sourceMethodBody.GetLocalScopes();
      }

      /// <summary>
      /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
      /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
      /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
      /// is for the y.
      /// </summary>
      [ContractVerification(false)]
      public IEnumerable<INamespaceScope> GetNamespaceScopes(IMethodBody methodBody) {
        var sourceMethodBody = methodBody as ILGeneratorMethodBody;
        if (sourceMethodBody == null) return this.originalLocalScopeProvider.GetNamespaceScopes(methodBody);
        return sourceMethodBody.GetNamespaceScopes();
      }

      /// <summary>
      /// Returns zero or more local constant definitions that are local to the given scope.
      /// </summary>
      public IEnumerable<ILocalDefinition> GetConstantsInScope(ILocalScope scope) {
        var generatorScope = scope as ILGeneratorScope;
        if (generatorScope == null) return this.originalLocalScopeProvider.GetConstantsInScope(scope);
        return generatorScope.Constants;
      }

      /// <summary>
      /// Returns zero or more local variable definitions that are local to the given scope.
      /// </summary>
      /// <param name="scope"></param>
      /// <returns></returns>
      public IEnumerable<ILocalDefinition> GetVariablesInScope(ILocalScope scope) {
        var generatorScope = scope as ILGeneratorScope;
        if (generatorScope == null) return this.originalLocalScopeProvider.GetVariablesInScope(scope);
        return generatorScope.Locals;
      }

      /// <summary>
      /// Returns true if the method body is an iterator.
      /// </summary>
      public bool IsIterator(IMethodBody methodBody) {
        var generatorMethodBody = methodBody as ILGeneratorMethodBody;
        if (generatorMethodBody == null) return this.originalLocalScopeProvider.IsIterator(methodBody);
        return generatorMethodBody.GetIteratorScopes() != null;
      }

      /// <summary>
      /// If the given method body is the "MoveNext" method of the state class of an asynchronous method, the returned
      /// object describes where synchronization points occur in the IL operations of the "MoveNext" method. Otherwise
      /// the result is null.
      /// </summary>
      public ISynchronizationInformation/*?*/ GetSynchronizationInformation(IMethodBody methodBody) {
        var generatorMethodBody = methodBody as ILGeneratorMethodBody;
        if (generatorMethodBody == null) return this.originalLocalScopeProvider.GetSynchronizationInformation(methodBody);
        return generatorMethodBody.GetSynchronizationInformation();
      }
      #endregion
    }

  }

  /// <summary>
  /// An object that is used to mark a location in an IL stream and that is used to indicate where branches go to.
  /// </summary>
  public sealed class ILGeneratorLabel {

    /// <summary>
    /// Initializes an object that is used to mark a location in an IL stream and that is used to indicate where branches go to.
    /// </summary>
    public ILGeneratorLabel() {
    }

    internal ILGeneratorLabel(bool mayAlias) {
      this.mayAlias = mayAlias;
    }

    /// <summary>
    /// 
    /// </summary>
    public uint Offset {
      get {
        if (this.alias != null) return this.alias.Offset;
        return this.offset;
      }
      set { this.offset = value; }
    }
    private uint offset;

    internal ILGeneratorLabel/*?*/ alias;
    internal bool mayAlias;
    /// <summary>
    /// Non-null only when labelsReturnInstruction is true.
    /// </summary>
    internal ILocation/*?*/ locationOfReturnInstruction;
  }

  /// <summary>
  /// A mutable object that represents a local variable or constant.
  /// </summary>
  public class GeneratorLocal : ILocalDefinition {

    /// <summary>
    /// Allocates a mutable object that represents a local variable or constant.
    /// </summary>
    public GeneratorLocal() {
      this.compileTimeValue = Dummy.Constant;
      this.customModifiers = new List<ICustomModifier>();
      this.isModified = false;
      this.isPinned = false;
      this.isReference = false;
      this.locations = new List<ILocation>();
      this.name = Dummy.Name;
      this.methodDefinition = Dummy.MethodDefinition;
      this.type = Dummy.TypeReference;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant() {
      Contract.Invariant(this.compileTimeValue != null);
      Contract.Invariant(this.customModifiers != null);
      Contract.Invariant(this.locations != null);
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.methodDefinition != null);
      Contract.Invariant(this.type != null);
    }


    /// <summary>
    /// The compile time value of the definition, if it is a local constant.
    /// </summary>
    /// <value></value>
    public IMetadataConstant CompileTimeValue {
      get { return this.compileTimeValue; }
      set {
        Contract.Requires(value != null);
        this.compileTimeValue = value; 
      }
    }
    IMetadataConstant compileTimeValue;

    /// <summary>
    /// Custom modifiers associated with local variable definition.
    /// </summary>
    /// <value></value>
    public List<ICustomModifier> CustomModifiers {
      get { return this.customModifiers; }
      set {
        Contract.Requires(value != null);
        this.customModifiers = value; 
      }
    }
    List<ICustomModifier> customModifiers;

    /// <summary>
    /// True if the local is a temporary variable generated by the compiler and thus does not have a corresponding source declaration.
    /// </summary>
    /// <remarks>The Visual Studio debugger will hide the existance of compiler generated locals.</remarks>
    public bool IsCompilerGenerated {
      get { return this.isCompilerGenerated; }
      set { this.isCompilerGenerated = value; }
    }
    bool isCompilerGenerated;

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
      set {
        Contract.Requires(value != null);
        this.name = value; 
      }
    }
    IName name;

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance.
    /// </summary>
    /// <value></value>
    public List<ILocation> Locations {
      get { return this.locations; }
      set {
        Contract.Requires(value != null);
        this.locations = value; 
      }
    }
    List<ILocation> locations;

    /// <summary>
    /// The definition of the method in which this local is defined.
    /// </summary>
    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
      set {
        Contract.Requires(value != null);
        this.methodDefinition = value; 
      }
    }
    IMethodDefinition methodDefinition;

    /// <summary>
    /// The type of the local.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.type; }
      set {
        Contract.Requires(value != null);
        this.type = value; 
      }
    }
    ITypeReference type;

    #region ILocalDefinition Members

    IEnumerable<ICustomModifier> ILocalDefinition.CustomModifiers {
      get { return this.customModifiers.AsReadOnly(); }
    }

    IEnumerable<ILocation> IObjectWithLocations.Locations {
      [ContractVerification(false)]
      get { return this.locations.AsReadOnly(); }
    }

    #endregion
  }

  /// <summary>
  /// An object that keeps track of a set of local definitions (variables) and used (imported) namespaces that appear in the
  /// source code corresponding to the IL operations from Offset to Offset+Length.
  /// </summary>
  public class ILGeneratorScope : ILocalScope, INamespaceScope {

    internal ILGeneratorScope(ILGeneratorLabel startLabel, INameTable nameTable, IMethodDefinition containingMethod) {
      Contract.Requires(startLabel != null);
      Contract.Requires(nameTable != null);
      Contract.Requires(containingMethod != null);

      this.startLabel = startLabel;
      this.nameTable = nameTable;
      this.methodDefinition = containingMethod;
    }

    ILGeneratorLabel startLabel;
    ILGeneratorLabel endLabel;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.startLabel != null);
      Contract.Invariant(this.constants != null);
      Contract.Invariant(this.locals != null);
      Contract.Invariant(this.usedNamespaces != null);
    }

    internal void CloseScope(ILGeneratorLabel endLabel) {
      this.endLabel = endLabel;
    }

    /// <summary>
    /// The local definitions (constants) defined in the source code corresponding to this scope.(A debugger can use this when evaluating expressions in a program
    /// point that falls inside this scope.)
    /// </summary>
    public IEnumerable<ILocalDefinition> Constants {
      get { return this.constants.AsReadOnly(); }
    }
    internal readonly List<ILocalDefinition> constants = new List<ILocalDefinition>();

    /// <summary>
    /// The length of the scope. Offset+Length equals the offset of the first operation outside the scope, or equals the method body length.
    /// </summary>
    /// <value></value>
    public uint Length {
      get {
        Contract.Assume(this.endLabel != null);
        return this.endLabel.Offset - this.startLabel.Offset; 
      }
    }

    /// <summary>
    /// The local definitions (variables) defined in the source code corresponding to this scope.(A debugger can use this when evaluating expressions in a program
    /// point that falls inside this scope.)
    /// </summary>
    public IEnumerable<ILocalDefinition> Locals {
      get { return this.locals.AsReadOnly(); }
    }
    internal readonly List<ILocalDefinition> locals = new List<ILocalDefinition>();

    readonly INameTable nameTable;

    /// <summary>
    /// The method definition.
    /// </summary>
    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
    }
    readonly IMethodDefinition methodDefinition;

    /// <summary>
    /// The offset of the first operation in the scope.
    /// </summary>
    public uint Offset {
      get { return this.startLabel.Offset; }
    }

    /// <summary>
    /// The namespaces that are used (imported) into this scope. (A debugger can use this when evaluating expressions in a program
    /// point that falls inside this scope.)
    /// </summary>
    /// <value>The used namespace names.</value>
    public IEnumerable<string> UsedNamespaceNames {
      get { return this.usedNamespaces.AsReadOnly(); }
    }
    internal readonly List<string> usedNamespaces = new List<string>(0);

    /// <summary>
    /// Zero or more used namespaces. These correspond to using clauses in C#.
    /// </summary>
    public IEnumerable<IUsedNamespace> UsedNamespaces {
      get {
        foreach (var usedNamespaceName in this.usedNamespaces)
          yield return new UsedNamespace(this.nameTable.GetNameFor(usedNamespaceName));
      }
    }
  }

}

namespace Microsoft.Cci.ILGeneratorImplementation {
  internal class TryBody {

    internal TryBody(ILGeneratorLabel start) {
      this.start = start;
    }

    internal readonly ILGeneratorLabel start;
    internal ILGeneratorLabel/*?*/ end;
  }

  internal class ExceptionHandler : IOperationExceptionInformation {

    internal ExceptionHandler() {
    }

    internal ExceptionHandler(HandlerKind kind, TryBody tryBlock, ILGeneratorLabel handlerStart) {
      this.ExceptionType = Dummy.TypeReference;
      this.Kind = kind;
      this.HandlerStart = handlerStart;
      this.tryBlock = tryBlock;
    }

    internal HandlerKind Kind;
    internal ITypeReference ExceptionType;
    internal ILGeneratorLabel/*?*/ TryStart;
    internal ILGeneratorLabel/*?*/ TryEnd;
    internal ILGeneratorLabel HandlerEnd;
    internal ILGeneratorLabel HandlerStart;
    internal ILGeneratorLabel/*?*/ FilterDecisionStart;
    TryBody/*?*/ tryBlock;

    #region IOperationExceptionInformation Members

    HandlerKind IOperationExceptionInformation.HandlerKind {
      get { return this.Kind; }
    }

    ITypeReference IOperationExceptionInformation.ExceptionType {
      get { return this.ExceptionType; }
    }

    uint IOperationExceptionInformation.TryStartOffset {
      [ContractVerification(false)]
      get {
        if (this.TryStart != null) return this.TryStart.Offset;
        return this.tryBlock.start.Offset;
      }
    }

    uint IOperationExceptionInformation.TryEndOffset {
      [ContractVerification(false)]
      get {
        if (this.TryEnd != null) return this.TryEnd.Offset;
        return this.tryBlock.end.Offset;
      }
    }

    uint IOperationExceptionInformation.FilterDecisionStartOffset {
      get {
        if (this.FilterDecisionStart == null) return 0;
        return this.FilterDecisionStart.Offset;
      }
    }

    uint IOperationExceptionInformation.HandlerStartOffset {
      get { return this.HandlerStart.Offset; }
    }

    uint IOperationExceptionInformation.HandlerEndOffset {
      get { return this.HandlerEnd.Offset; }
    }

    #endregion
  }

  internal class Operation : IOperation {

    internal Operation(OperationCode operationCode, uint offset, ILocation location, object/*?*/ value) {
      Contract.Requires(location != null);

      this.operationCode = operationCode;
      this.offset = offset;
      this.location = location;
      this.value = value;
    }

    public OperationCode OperationCode {
      get { return this.operationCode; }
    }
    internal OperationCode operationCode;

    public uint Offset {
      get { return this.offset; }
    }
    internal uint offset;

    public ILocation Location {
      get { return this.location; }
    }
    readonly ILocation location;

    public object/*?*/ Value {
      get {
        ILGeneratorLabel/*?*/ label = this.value as ILGeneratorLabel;
        if (label != null) return label.Offset;
        ILGeneratorLabel[]/*?*/ labels = this.value as ILGeneratorLabel[];
        if (labels != null) {
          uint[] labelOffsets = new uint[labels.Length];
          for (int i = 0; i < labels.Length; i++) {
            var labeli = labels[i];
            Contract.Assume(labeli != null);
            labelOffsets[i] = labeli.Offset;
          }
          this.value = labelOffsets;
          return labelOffsets;
        }
        return this.value;
      }
    }
    internal object/*?*/ value;

  }

  internal class SynchronizationInformation : ISynchronizationInformation {

    internal IMethodDefinition asyncMethod;
    internal IMethodDefinition moveNextMethod;
    internal uint generatedCatchHandlerOffset;
    internal IEnumerable<ISynchronizationPoint> synchronizationPoints;

    #region ISynchronizationInformation Members

    public IMethodDefinition AsyncMethod {
      get { return this.asyncMethod; }
    }

    public IMethodDefinition MoveNextMethod {
      get { return this.moveNextMethod; }
    }

    public uint GeneratedCatchHandlerOffset {
      get { return this.generatedCatchHandlerOffset; }
    }

    public IEnumerable<ISynchronizationPoint> SynchronizationPoints {
      get { return this.synchronizationPoints; }
    }

    #endregion
  }

  internal class SynchronizationPoint : ISynchronizationPoint {

    internal ILGeneratorLabel startOfSynchronize;
    internal ILGeneratorLabel startOfContinuation;
    internal IMethodDefinition/*?*/ continuationMethod;

    #region ISynchronizationPoint Members

    public uint SynchronizeOffset {
      get { return this.startOfSynchronize.Offset; }
    }

    public IMethodDefinition/*?*/ ContinuationMethod {
      get { return this.continuationMethod; }
    }

    public uint ContinuationOffset {
      get { return this.startOfContinuation.Offset; }
    }

    #endregion
  }

  /// <summary>
  ///  A namespace that is used (imported) inside a namespace scope.
  /// </summary>
  internal class UsedNamespace : IUsedNamespace {

    /// <summary>
    /// Allocates a namespace that is used (imported) inside a namespace scope.
    /// </summary>
    /// <param name="namespaceName">The name of a namepace that has been aliased.  For example the "y.z" of "using x = y.z;" or "using y.z" in C#.</param>
    internal UsedNamespace(IName namespaceName) {
      Contract.Requires(namespaceName != null);

      this.namespaceName = namespaceName;
    }

    /// <summary>
    /// An alias for a namespace. For example the "x" of "using x = y.z;" in C#. Empty if no alias is present.
    /// </summary>
    /// <value></value>
    public IName Alias {
      get { return Dummy.Name; }
    }

    /// <summary>
    /// The name of a namepace that has been aliased.  For example the "y.z" of "using x = y.z;" or "using y.z" in C#.
    /// </summary>
    /// <value></value>
    public IName NamespaceName {
      get { return this.namespaceName; }
    }
    readonly IName namespaceName;

  }

  internal class Stack<T> {

    T[] elements = new T[16];

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.elements != null);
      Contract.Invariant(this.elements.Length > 0);
    }

    internal int Count { get; set; }

    internal T Peek() {
      Contract.Assume(this.Count > 0);
      Contract.Assume(this.Count <= this.elements.Length);
      return this.elements[this.Count-1];
    }

    internal T Pop() {
      Contract.Assume(this.Count > 0);
      Contract.Assume(this.Count <= this.elements.Length);
      var i = this.Count-1;
      this.Count = i;
      return this.elements[i];
    }

    internal void Push(T element) {
      Contract.Assume(this.Count >= 0);
      if (this.Count == this.elements.Length)
        Array.Resize(ref this.elements, this.elements.Length*2);
      Contract.Assume(this.Count < this.elements.Length);
      var i = this.Count;
      this.Count = i+1;
      this.elements[i] = element;
    }
  }

}
