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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci.Analysis {

  internal class DataFlowInferencer<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.Analysis.BasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Analysis.Instruction, new() {

    private DataFlowInferencer(IMetadataHost host, ControlAndDataFlowGraph<BasicBlock, Instruction> cdfg) {
      Contract.Requires(host != null);
      Contract.Requires(cdfg != null);

      var numberOfBlocks = cdfg.BlockFor.Count;
      this.platformType = host.PlatformType;
      this.cdfg = cdfg;
      this.operandStackSetupInstructions = new List<Instruction>(cdfg.MethodBody.MaxStack);
      this.stack = new Stack<Instruction>(cdfg.MethodBody.MaxStack);
      this.blocksToVisit = new Queue<BasicBlock>((int)numberOfBlocks);
      this.blocksAlreadyVisited = new SetOfObjects(numberOfBlocks); ;
      this.internFactory = host.InternFactory;
    }

    IPlatformType platformType;
    ControlAndDataFlowGraph<BasicBlock, Instruction> cdfg;
    Stack<Instruction> stack;
    List<Instruction> operandStackSetupInstructions;
    Queue<BasicBlock> blocksToVisit;
    SetOfObjects blocksAlreadyVisited;
    IInternFactory internFactory;
    bool codeIsUnreachable;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.platformType != null);
      Contract.Invariant(this.cdfg != null);
      Contract.Invariant(this.stack != null);
      Contract.Invariant(this.operandStackSetupInstructions != null);
      Contract.Invariant(this.blocksToVisit != null);
      Contract.Invariant(this.blocksAlreadyVisited != null);
      Contract.Invariant(this.internFactory != null);
    }

    /// <summary>
    /// 
    /// </summary>
    internal static void SetupDataFlow(IMetadataHost host, IMethodBody methodBody, ControlAndDataFlowGraph<BasicBlock, Instruction> cdfg) {
      Contract.Requires(host != null);
      Contract.Requires(methodBody != null);
      Contract.Requires(cdfg != null);

      var dataFlowInferencer = new DataFlowInferencer<BasicBlock, Instruction>(host, cdfg);
      dataFlowInferencer.SetupDataFlowFor(methodBody);
    }

    private void SetupDataFlowFor(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);

      //If this is a dummy body, do nothing.
      if (this.cdfg.AllBlocks.Count == 1 && this.cdfg.AllBlocks[0] != null && this.cdfg.AllBlocks[0].Instructions.Count <= 1) return;

      this.AddStackSetupForExceptionHandlers(methodBody);
      foreach (var root in this.cdfg.RootBlocks) {
        this.blocksToVisit.Enqueue(root);
        while (this.blocksToVisit.Count != 0)
          this.DequeueBlockAndSetupDataFlow();
      }
      //At this point, all reachable code blocks have had their data flow inferred. Now look for unreachable blocks.
      this.codeIsUnreachable = true; //unreachable code might not satisfy invariants.
      foreach (var block in this.cdfg.AllBlocks) {
        if (this.blocksAlreadyVisited.Contains(block)) continue;
        blocksToVisit.Enqueue(block);
        while (blocksToVisit.Count != 0)
          this.DequeueBlockAndSetupDataFlow();
      }
      this.operandStackSetupInstructions.TrimExcess();
    }

    private void AddStackSetupForExceptionHandlers(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);

      foreach (var exinfo in methodBody.OperationExceptionInformation) {
        Contract.Assert(exinfo != null); //The checker can't work out that all collection elements are non null, even though there is a contract to that effect
        if (exinfo.HandlerKind == HandlerKind.Filter) {
          var block = this.cdfg.BlockFor[exinfo.FilterDecisionStartOffset];
          Contract.Assume(block != null); //All branch targets must have blocks, but we can't put that in a contract that satisfies the checker.
          this.AddStackSetup(block, exinfo.ExceptionType);
          block = this.cdfg.BlockFor[exinfo.HandlerStartOffset];
          Contract.Assume(block != null); //All branch targets must have blocks, but we can't put that in a contract that satisfies the checker.
          this.AddStackSetup(block, exinfo.ExceptionType);
        } else if (exinfo.HandlerKind == HandlerKind.Catch) {
          var block = this.cdfg.BlockFor[exinfo.HandlerStartOffset];
          Contract.Assume(block != null); //All branch targets must have blocks, but we can't put that in a contract that satisfies the checker.
          this.AddStackSetup(block, exinfo.ExceptionType);
        }
      }
    }

    private void AddStackSetup(BasicBlock block, ITypeReference operandType) {
      Contract.Requires(block != null);
      Contract.Requires(operandType != null);

      this.operandStackSetupInstructions.Add(new Instruction() { Type = operandType });
      block.OperandStack = new Sublist<Instruction>(this.operandStackSetupInstructions, this.operandStackSetupInstructions.Count-1, 1);
    }

    private void DequeueBlockAndSetupDataFlow() {
      var block = this.blocksToVisit.Dequeue();
      Contract.Assume(block != null); //this.blocksToVisit only has non null elements, but we can't put that in a contract that satisfies the checker
      if (!this.blocksAlreadyVisited.Add(block)) return; //The same block can be added multiple times to the queue.

      foreach (var instruction in block.OperandStack) {
        Contract.Assume(instruction != null); //block.OperandStack only has non null elements, but we can't put that in a contract that satisfies the checker
        this.stack.Push(instruction);
      }

      foreach (var instruction in block.Instructions) {
        Contract.Assume(instruction != null); //block.Instructions only has non null elements, but we can't put that in a contract that satisfies the checker
        this.SetupDataFlowFor(instruction);
      }

      foreach (var successor in this.cdfg.SuccessorsFor(block)) {
        Contract.Assume(successor != null); //block.Successors only has non null elements, but we can't put that in a contract that satisfies the checker
        this.SetupStackFor(successor);
        if (blocksAlreadyVisited.Contains(successor)) continue;
        blocksToVisit.Enqueue(successor); //The block might already be in the queue, but we can deal with this more efficiently by checking blocksAlreadyVisited when dequeueing.
      }

      this.stack.Clear();

    }

    private void SetupStackFor(BasicBlock successor) {
      Contract.Requires(successor != null);

      if (successor.OperandStack.Count == 0) {
        int n = this.stack.Top;
        if (n < 0) return;
        int startingCount = this.operandStackSetupInstructions.Count;
        for (int i = 0; i <= n; i++) {
          var pushInstruction = this.stack.Peek(i);
          this.operandStackSetupInstructions.Add(new Instruction() { Operand1 = pushInstruction });
        }
        successor.OperandStack = new Sublist<Instruction>(this.operandStackSetupInstructions, startingCount, operandStackSetupInstructions.Count-startingCount);
      } else {
        int n = this.stack.Top;
        Contract.Assume(n == successor.OperandStack.Count-1); //This is an optimistic assumption. It should be true for any well formed PE file. We are content to crash given bad input.
        for (int i = 0; i <= n; i++) {
          var pushInstruction = this.stack.Peek(i);
          var setupInstruction = successor.OperandStack[i];
          if (setupInstruction.Operand2 == null)
            setupInstruction.Operand2 = pushInstruction;
          else {
            var list = setupInstruction.Operand2 as List<Instruction>;
            if (list == null) {
              Contract.Assume(setupInstruction.Operand2 is Instruction);
              list = new List<Instruction>(4);
              list.Add((Instruction)setupInstruction.Operand2);
            }
            list.Add(pushInstruction);
          }
        }
      }
    }

    private void SetupDataFlowFor(Instruction instruction) {
      Contract.Requires(instruction != null);

      switch (instruction.Operation.OperationCode) {
        case OperationCode.Add:
        case OperationCode.Add_Ovf:
        case OperationCode.Add_Ovf_Un:
        case OperationCode.And:
        case OperationCode.Ceq:
        case OperationCode.Cgt:
        case OperationCode.Cgt_Un:
        case OperationCode.Clt:
        case OperationCode.Clt_Un:
        case OperationCode.Div:
        case OperationCode.Div_Un:
        case OperationCode.Ldelema:
        case OperationCode.Ldelem:
        case OperationCode.Ldelem_I:
        case OperationCode.Ldelem_I1:
        case OperationCode.Ldelem_I2:
        case OperationCode.Ldelem_I4:
        case OperationCode.Ldelem_I8:
        case OperationCode.Ldelem_R4:
        case OperationCode.Ldelem_R8:
        case OperationCode.Ldelem_Ref:
        case OperationCode.Ldelem_U1:
        case OperationCode.Ldelem_U2:
        case OperationCode.Ldelem_U4:
        case OperationCode.Mul:
        case OperationCode.Mul_Ovf:
        case OperationCode.Mul_Ovf_Un:
        case OperationCode.Or:
        case OperationCode.Rem:
        case OperationCode.Rem_Un:
        case OperationCode.Shl:
        case OperationCode.Shr:
        case OperationCode.Shr_Un:
        case OperationCode.Sub:
        case OperationCode.Sub_Ovf:
        case OperationCode.Sub_Ovf_Un:
        case OperationCode.Xor:
          instruction.Operand2 = this.stack.Pop();
          instruction.Operand1 = this.stack.Pop();
          this.stack.Push(instruction);
          break;

        case OperationCode.Arglist:
        case OperationCode.Ldarg:
        case OperationCode.Ldarg_0:
        case OperationCode.Ldarg_1:
        case OperationCode.Ldarg_2:
        case OperationCode.Ldarg_3:
        case OperationCode.Ldarg_S:
        case OperationCode.Ldloc:
        case OperationCode.Ldloc_0:
        case OperationCode.Ldloc_1:
        case OperationCode.Ldloc_2:
        case OperationCode.Ldloc_3:
        case OperationCode.Ldloc_S:
        case OperationCode.Ldsfld:
        case OperationCode.Ldarga:
        case OperationCode.Ldarga_S:
        case OperationCode.Ldsflda:
        case OperationCode.Ldloca:
        case OperationCode.Ldloca_S:
        case OperationCode.Ldftn:
        case OperationCode.Ldc_I4:
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
        case OperationCode.Ldc_I4_S:
        case OperationCode.Ldc_I8:
        case OperationCode.Ldc_R4:
        case OperationCode.Ldc_R8:
        case OperationCode.Ldnull:
        case OperationCode.Ldstr:
        case OperationCode.Ldtoken:
        case OperationCode.Sizeof:
          this.stack.Push(instruction);
          break;

        case OperationCode.Array_Addr:
        case OperationCode.Array_Get:
          Contract.Assume(instruction.Operation.Value is IArrayTypeReference); //This is an informally specified property of the Metadata model.
          InitializeArrayIndexerInstruction(instruction, this.stack, (IArrayTypeReference)instruction.Operation.Value);
          break;

        case OperationCode.Array_Create:
        case OperationCode.Array_Create_WithLowerBound:
        case OperationCode.Newarr:
          InitializeArrayCreateInstruction(instruction, this.stack, instruction.Operation);
          break;

        case OperationCode.Array_Set:
          Contract.Assume(instruction.Operation.Value is IArrayTypeReference); //This is an informally specified property of the Metadata model.
          InitializeArraySetInstruction(instruction, this.stack, (IArrayTypeReference)instruction.Operation.Value);
          break;

        case OperationCode.Beq:
        case OperationCode.Beq_S:
        case OperationCode.Bge:
        case OperationCode.Bge_S:
        case OperationCode.Bge_Un:
        case OperationCode.Bge_Un_S:
        case OperationCode.Bgt:
        case OperationCode.Bgt_S:
        case OperationCode.Bgt_Un:
        case OperationCode.Bgt_Un_S:
        case OperationCode.Ble:
        case OperationCode.Ble_S:
        case OperationCode.Ble_Un:
        case OperationCode.Ble_Un_S:
        case OperationCode.Blt:
        case OperationCode.Blt_S:
        case OperationCode.Blt_Un:
        case OperationCode.Blt_Un_S:
        case OperationCode.Bne_Un:
        case OperationCode.Bne_Un_S:
          instruction.Operand2 = this.stack.Pop();
          instruction.Operand1 = this.stack.Pop();
          break;

        case OperationCode.Box:
        case OperationCode.Castclass:
        case OperationCode.Ckfinite:
        case OperationCode.Conv_I:
        case OperationCode.Conv_I1:
        case OperationCode.Conv_I2:
        case OperationCode.Conv_I4:
        case OperationCode.Conv_I8:
        case OperationCode.Conv_Ovf_I:
        case OperationCode.Conv_Ovf_I_Un:
        case OperationCode.Conv_Ovf_I1:
        case OperationCode.Conv_Ovf_I1_Un:
        case OperationCode.Conv_Ovf_I2:
        case OperationCode.Conv_Ovf_I2_Un:
        case OperationCode.Conv_Ovf_I4:
        case OperationCode.Conv_Ovf_I4_Un:
        case OperationCode.Conv_Ovf_I8:
        case OperationCode.Conv_Ovf_I8_Un:
        case OperationCode.Conv_Ovf_U:
        case OperationCode.Conv_Ovf_U_Un:
        case OperationCode.Conv_Ovf_U1:
        case OperationCode.Conv_Ovf_U1_Un:
        case OperationCode.Conv_Ovf_U2:
        case OperationCode.Conv_Ovf_U2_Un:
        case OperationCode.Conv_Ovf_U4:
        case OperationCode.Conv_Ovf_U4_Un:
        case OperationCode.Conv_Ovf_U8:
        case OperationCode.Conv_Ovf_U8_Un:
        case OperationCode.Conv_R_Un:
        case OperationCode.Conv_R4:
        case OperationCode.Conv_R8:
        case OperationCode.Conv_U:
        case OperationCode.Conv_U1:
        case OperationCode.Conv_U2:
        case OperationCode.Conv_U4:
        case OperationCode.Conv_U8:
        case OperationCode.Isinst:
        case OperationCode.Ldind_I:
        case OperationCode.Ldind_I1:
        case OperationCode.Ldind_I2:
        case OperationCode.Ldind_I4:
        case OperationCode.Ldind_I8:
        case OperationCode.Ldind_R4:
        case OperationCode.Ldind_R8:
        case OperationCode.Ldind_Ref:
        case OperationCode.Ldind_U1:
        case OperationCode.Ldind_U2:
        case OperationCode.Ldind_U4:
        case OperationCode.Ldobj:
        case OperationCode.Ldflda:
        case OperationCode.Ldfld:
        case OperationCode.Ldlen:
        case OperationCode.Ldvirtftn:
        case OperationCode.Localloc:
        case OperationCode.Mkrefany:
        case OperationCode.Neg:
        case OperationCode.Not:
        case OperationCode.Refanytype:
        case OperationCode.Refanyval:
        case OperationCode.Unbox:
        case OperationCode.Unbox_Any:
          instruction.Operand1 = this.stack.Pop();
          this.stack.Push(instruction);
          break;

        case OperationCode.Brfalse:
        case OperationCode.Brfalse_S:
        case OperationCode.Brtrue:
        case OperationCode.Brtrue_S:
          instruction.Operand1 = this.stack.Pop();
          break;

        case OperationCode.Call:
        case OperationCode.Callvirt:
          var signature = instruction.Operation.Value as ISignature;
          Contract.Assume(signature != null); //This is an informally specified property of the Metadata model.
          InitializeArgumentsAndPushReturnResult(instruction, this.stack, signature);
          break;

        case OperationCode.Calli:
          var funcPointer = instruction.Operation.Value as IFunctionPointerTypeReference;
          Contract.Assume(funcPointer != null); //This is an informally specified property of the Metadata model.
          InitializeArgumentsAndPushReturnResult(instruction, this.stack, funcPointer);
          break;

        case OperationCode.Cpobj:
        case OperationCode.Stfld:
        case OperationCode.Stind_I:
        case OperationCode.Stind_I1:
        case OperationCode.Stind_I2:
        case OperationCode.Stind_I4:
        case OperationCode.Stind_I8:
        case OperationCode.Stind_R4:
        case OperationCode.Stind_R8:
        case OperationCode.Stind_Ref:
        case OperationCode.Stobj:
          instruction.Operand2 = this.stack.Pop();
          instruction.Operand1 = this.stack.Pop();
          break;

        case OperationCode.Cpblk:
        case OperationCode.Initblk:
        case OperationCode.Stelem:
        case OperationCode.Stelem_I:
        case OperationCode.Stelem_I1:
        case OperationCode.Stelem_I2:
        case OperationCode.Stelem_I4:
        case OperationCode.Stelem_I8:
        case OperationCode.Stelem_R4:
        case OperationCode.Stelem_R8:
        case OperationCode.Stelem_Ref:
          var indexAndValue = new Instruction[2];
          indexAndValue[1] = this.stack.Pop();
          indexAndValue[0] = this.stack.Pop();
          instruction.Operand2 = indexAndValue;
          instruction.Operand1 = this.stack.Pop();
          break;

        case OperationCode.Dup:
          var dupop = this.stack.Pop();
          instruction.Operand1 = dupop;
          this.stack.Push(instruction);
          this.stack.Push(instruction);
          break;

        case OperationCode.Endfilter:
        case OperationCode.Initobj:
        case OperationCode.Pop:
        case OperationCode.Starg:
        case OperationCode.Starg_S:
        case OperationCode.Stloc:
        case OperationCode.Stloc_0:
        case OperationCode.Stloc_1:
        case OperationCode.Stloc_2:
        case OperationCode.Stloc_3:
        case OperationCode.Stloc_S:
        case OperationCode.Stsfld:
        case OperationCode.Throw:
        case OperationCode.Switch:
          instruction.Operand1 = this.stack.Pop();
          break;

        case OperationCode.Leave:
        case OperationCode.Leave_S:
          this.stack.Clear();
          break;

        case OperationCode.Newobj:
          Contract.Assume(instruction.Operation.Value is ISignature); //This is an informally specified property of the Metadata model.
          signature = (ISignature)instruction.Operation.Value;
          var numArguments = (int)IteratorHelper.EnumerableCount(signature.Parameters);
          if (numArguments > 0) {
            if (numArguments > 1) {
              numArguments--;
              var arguments = new Instruction[numArguments];
              instruction.Operand2 = arguments;
              for (var i = numArguments-1; i >= 0; i--)
                arguments[i] = stack.Pop();
            }
            instruction.Operand1 = stack.Pop();
          }
          this.stack.Push(instruction);
          break;

        case OperationCode.Ret:
          if (this.codeIsUnreachable && this.stack.Top < 0) break;
          if (this.cdfg.MethodBody.MethodDefinition.Type.TypeCode != PrimitiveTypeCode.Void)
            instruction.Operand1 = this.stack.Pop();
          break;
      }
    }

    private static void InitializeArgumentsAndPushReturnResult(Instruction instruction, Stack<Instruction> stack, ISignature signature) {
      Contract.Requires(instruction != null);
      Contract.Requires(stack != null);
      Contract.Requires(signature != null);

      var methodRef = signature as IMethodReference;
      uint numArguments = IteratorHelper.EnumerableCount(signature.Parameters);
      if (methodRef != null && methodRef.AcceptsExtraArguments) numArguments += IteratorHelper.EnumerableCount(methodRef.ExtraParameters);
      if (!signature.IsStatic) numArguments++;
      if (numArguments > 0) {
        numArguments--;
        if (numArguments > 0) {
          var arguments = new Instruction[numArguments];
          instruction.Operand2 = arguments;
          for (var i = numArguments; i > 0; i--)
            arguments[i-1] = stack.Pop();
        }
        instruction.Operand1 = stack.Pop();
      }
      if (signature.Type.TypeCode != PrimitiveTypeCode.Void)
        stack.Push(instruction);
    }

    private static void InitializeArgumentsAndPushReturnResult(Instruction instruction, Stack<Instruction> stack, IFunctionPointerTypeReference funcPointer) {
      Contract.Requires(instruction != null);
      Contract.Requires(stack != null);
      Contract.Requires(funcPointer != null);

      instruction.Operand1 = stack.Pop(); //the function pointer
      var numArguments = IteratorHelper.EnumerableCount(funcPointer.Parameters);
      if (!funcPointer.IsStatic) numArguments++;
      var arguments = new Instruction[numArguments];
      instruction.Operand2 = arguments;
      for (var i = numArguments; i > 0; i--)
        arguments[i-1] = stack.Pop();
      if (funcPointer.Type.TypeCode != PrimitiveTypeCode.Void)
        stack.Push(instruction);
    }

    private static void InitializeArrayCreateInstruction(Instruction instruction, Stack<Instruction> stack, IOperation currentOperation) {
      Contract.Requires(instruction != null);
      Contract.Requires(stack != null);
      Contract.Requires(currentOperation != null);
      IArrayTypeReference arrayType = (IArrayTypeReference)currentOperation.Value;
      Contract.Assume(arrayType != null); //This is an informally specified property of the Metadata model.
      var rank = arrayType.Rank;
      if (rank > 0) {
        if (currentOperation.OperationCode == OperationCode.Array_Create_WithLowerBound) rank *= 2;
        rank--;
        if (rank > 0) {
          var indices = new Instruction[rank];
          instruction.Operand2 = indices;
          for (var i = rank; i > 0; i--)
            indices[i-1] = stack.Pop();
        }
        instruction.Operand1 = stack.Pop();
      }
      stack.Push(instruction);
    }

    private static void InitializeArrayIndexerInstruction(Instruction instruction, Stack<Instruction> stack, IArrayTypeReference arrayType) {
      Contract.Requires(instruction != null);
      Contract.Requires(stack != null);
      Contract.Requires(arrayType != null);
      var rank = arrayType.Rank;
      var indices = new Instruction[rank];
      instruction.Operand2 = indices;
      for (var i = rank; i > 0; i--)
        indices[i-1] = stack.Pop();
      instruction.Operand1 = stack.Pop();
      stack.Push(instruction);
    }

    private static void InitializeArraySetInstruction(Instruction instruction, Stack<Instruction> stack, IArrayTypeReference arrayType) {
      Contract.Requires(instruction != null);
      Contract.Requires(stack != null);
      Contract.Requires(arrayType != null);
      var rank = arrayType.Rank;
      var indices = new Instruction[rank+1];
      instruction.Operand2 = indices;
      for (var i = rank+1; i > 0; i--)
        indices[i-1] = stack.Pop();
      instruction.Operand1 = stack.Pop();
    }

  }

}