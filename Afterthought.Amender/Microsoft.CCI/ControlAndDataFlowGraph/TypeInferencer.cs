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
using Microsoft.Cci.Immutable;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci.Analysis {
  /// <summary>
  /// 
  /// </summary>
  internal class TypeInferencer<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.Analysis.BasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Analysis.Instruction, new() {

    private TypeInferencer(IMetadataHost host, ControlAndDataFlowGraph<BasicBlock, Instruction> cfg, Stack<Instruction> stack, Queue<BasicBlock> blocksToVisit, SetOfObjects blocksAlreadyVisited) {
      Contract.Requires(host != null);
      Contract.Requires(cfg != null);
      Contract.Requires(stack != null);
      Contract.Requires(blocksToVisit != null);
      Contract.Requires(blocksAlreadyVisited != null);

      this.platformType = host.PlatformType;
      this.cfg = cfg;
      this.stack = stack;
      this.blocksToVisit = blocksToVisit;
      this.blocksAlreadyVisited = blocksAlreadyVisited;
      this.internFactory = host.InternFactory;
    }

    IPlatformType platformType;
    ControlAndDataFlowGraph<BasicBlock, Instruction> cfg;
    Stack<Instruction> stack;
    Queue<BasicBlock> blocksToVisit;
    SetOfObjects blocksAlreadyVisited;
    IInternFactory internFactory;
    bool codeIsUnreachable;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.platformType != null);
      Contract.Invariant(this.cfg != null);
      Contract.Invariant(this.stack != null);
      Contract.Invariant(this.blocksToVisit != null);
      Contract.Invariant(this.blocksAlreadyVisited != null);
      Contract.Invariant(this.internFactory != null);
    }

    /// <summary>
    /// 
    /// </summary>
    internal static void FillInTypes(IMetadataHost host, ControlAndDataFlowGraph<BasicBlock, Instruction> cfg) {
      Contract.Requires(host != null);
      Contract.Requires(cfg != null);

      //If this is a dummy body, do nothing.
      if (cfg.AllBlocks.Count == 1 && cfg.AllBlocks[0] != null && cfg.AllBlocks[0].Instructions.Count <= 1) return;

      var stack = new Stack<Instruction>(cfg.MethodBody.MaxStack);
      var numberOfBlocks = cfg.BlockFor.Count;
      var blocksToVisit = new Queue<BasicBlock>((int)numberOfBlocks);
      var blocksAlreadyVisited = new SetOfObjects(numberOfBlocks);
      var inferencer = new TypeInferencer<BasicBlock, Instruction>(host, cfg, stack, blocksToVisit, blocksAlreadyVisited);

      foreach (var root in cfg.RootBlocks) {
        blocksToVisit.Enqueue(root);
        while (blocksToVisit.Count != 0)
          inferencer.DequeueBlockAndFillInItsTypes();
      }

      //At this point, all reachable code blocks have had their types inferred. Now look for unreachable blocks.
      inferencer.codeIsUnreachable = true;
      foreach (var block in cfg.AllBlocks) {
        if (blocksAlreadyVisited.Contains(block)) continue;
        blocksToVisit.Enqueue(block);
        while (blocksToVisit.Count != 0)
          inferencer.DequeueBlockAndFillInItsTypes();
      }
    }

    private void DequeueBlockAndFillInItsTypes() {
      var block = this.blocksToVisit.Dequeue();
      Contract.Assume(block != null); //this.blocksToVisit only has non null elements, but we can't put that in a contract that satisfies the checker
      if (!this.blocksAlreadyVisited.Add(block)) return; //The same block can be added multiple times to the queue.

      //The block either has no operand stack setup instructions, or we presume that a predecessor block has already assigned types to them.
      foreach (var stackSetupInstruction in block.OperandStack) {
        Contract.Assume(stackSetupInstruction != null); //block.OperandStack only has non null elements, but we can't put that in a contract that satisfies the checker
        this.stack.Push(stackSetupInstruction);
      }

      foreach (var instruction in block.Instructions) {
        Contract.Assume(instruction != null); //block.Instructions only has non null elements, but we can't put that in a contract that satisfies the checker
        this.InferTypeAndUpdateStack(instruction);
      }

      foreach (var successor in this.cfg.SuccessorsFor(block)) {
        Contract.Assume(successor != null); //block.Successors only has non null elements, but we can't put that in a contract that satisfies the checker
        this.TransferTypesFromStackTo(successor);
        if (blocksAlreadyVisited.Contains(successor)) continue;
        blocksToVisit.Enqueue(successor); //The block might already be in the queue, but we can deal with this more efficiently by checking blocksAlreadyVisited when dequeueing.
      }

      this.stack.Clear();
    }

    private void TransferTypesFromStackTo(BasicBlock successor) {
      Contract.Requires(successor != null);
      Contract.Assume(this.stack.Top+1 == successor.OperandStack.Count); //We assume that the DataFlowInferencer sets things up this way.
      for (int i = 0, n = this.stack.Top; i <= n; i++) {
        var producer = this.stack.Peek(i);
        var consumer = successor.OperandStack[i];
        if (consumer.Type is Dummy)
          consumer.Type = producer.Type;
        else
          consumer.Type = TypeHelper.MergedType(consumer.Type, producer.Type);
      }
    }

    private void InferTypeAndUpdateStack(Instruction instruction) {
      Contract.Requires(instruction != null);
      switch (instruction.Operation.OperationCode) {
        case OperationCode.Add:
        case OperationCode.Add_Ovf:
        case OperationCode.Div:
        case OperationCode.Mul:
        case OperationCode.Mul_Ovf:
        case OperationCode.Rem:
        case OperationCode.Sub:
        case OperationCode.Sub_Ovf:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.GetBinaryNumericOperationType(instruction);
          this.stack.Push(instruction);
          break;
        case OperationCode.And:
        case OperationCode.Or:
        case OperationCode.Xor:
          Contract.Assume(instruction.Operand1 != null);
          Contract.Assume(instruction.Operand2 is Instruction);
          if (instruction.Operand1.Type.TypeCode == PrimitiveTypeCode.Boolean && ((Instruction)instruction.Operand2).Type.TypeCode == PrimitiveTypeCode.Boolean) {
            this.stack.Pop();
            this.stack.Pop();
            instruction.Type = this.platformType.SystemBoolean;
            this.stack.Push(instruction);
            break;
          }
          goto case OperationCode.Add;

        case OperationCode.Add_Ovf_Un:
        case OperationCode.Div_Un:
        case OperationCode.Mul_Ovf_Un:
        case OperationCode.Rem_Un:
        case OperationCode.Sub_Ovf_Un:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.GetUnsignedBinaryNumericOperationType(instruction);
          this.stack.Push(instruction);
          break;
        case OperationCode.Arglist:
          instruction.Type = this.platformType.SystemRuntimeArgumentHandle;
          this.stack.Push(instruction);
          break;
        case OperationCode.Array_Addr:
          var arrayType = instruction.Operation.Value as IArrayTypeReference;
          Contract.Assume(arrayType != null); //This is an informally specified property of the Metadata model.
          for (var i = arrayType.Rank; i > 0; i--)
            this.stack.Pop();
          this.stack.Pop();
          instruction.Type = ManagedPointerType.GetManagedPointerType(arrayType.ElementType, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Array_Create:
          arrayType = instruction.Operation.Value as IArrayTypeReference;
          Contract.Assume(arrayType != null); //This is an informally specified property of the Metadata model.
          for (var i = arrayType.Rank; i > 0; i--)
            this.stack.Pop();
          instruction.Type = arrayType;
          this.stack.Push(instruction);
          break;
        case OperationCode.Array_Create_WithLowerBound:
          arrayType = instruction.Operation.Value as IArrayTypeReference;
          Contract.Assume(arrayType != null); //This is an informally specified property of the Metadata model.
          for (var i = arrayType.Rank*2; i > 0; i--)
            this.stack.Pop();
          instruction.Type = arrayType;
          this.stack.Push(instruction);
          break;
        case OperationCode.Array_Get:
          arrayType = instruction.Operation.Value as IArrayTypeReference;
          Contract.Assume(arrayType != null); //This is an informally specified property of the Metadata model.
          for (var i = arrayType.Rank; i > 0; i--)
            this.stack.Pop();
          this.stack.Pop();
          instruction.Type = arrayType.ElementType;
          this.stack.Push(instruction);
          break;
        case OperationCode.Array_Set:
          arrayType = instruction.Operation.Value as IArrayTypeReference;
          Contract.Assume(arrayType != null); //This is an informally specified property of the Metadata model.
          this.stack.Pop(); //The value to set
          for (var i = arrayType.Rank; i > 0; i--)
            this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemVoid;
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
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemVoid;
          break;
        case OperationCode.Box:
          this.stack.Pop();
          var typeInBox = instruction.Operation.Value as ITypeReference;
          Contract.Assume(typeInBox != null);
          if (typeInBox.ResolvedType.IsReferenceType)
            instruction.Type = typeInBox.ResolvedType; //This typically happens when typeInBox is a type parameter.
          else
            instruction.Type = this.platformType.SystemObject;
          this.stack.Push(instruction);
          break;
        case OperationCode.Brfalse:
        case OperationCode.Brfalse_S:
        case OperationCode.Brtrue:
        case OperationCode.Brtrue_S:
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
          this.stack.Pop();
          instruction.Type = this.platformType.SystemVoid;
          break;
        case OperationCode.Call:
        case OperationCode.Callvirt:
          var signature = instruction.Operation.Value as ISignature;
          Contract.Assume(signature != null); //This is an informally specified property of the Metadata model.
          var methodRef = signature as IMethodReference;
          uint numArguments = IteratorHelper.EnumerableCount(signature.Parameters);
          if (methodRef != null && methodRef.AcceptsExtraArguments) numArguments += IteratorHelper.EnumerableCount(methodRef.ExtraParameters);
          for (var i = numArguments; i > 0; i--)
            this.stack.Pop();
          if (!signature.IsStatic)
            this.stack.Pop();
          instruction.Type = signature.Type;
          if (signature.Type.TypeCode != PrimitiveTypeCode.Void)
            this.stack.Push(instruction);
          break;
        case OperationCode.Calli:
          var funcPointer = instruction.Operation.Value as IFunctionPointerTypeReference;
          Contract.Assume(funcPointer != null); //This is an informally specified property of the Metadata model.
          var fp = this.stack.Pop(); //The function pointer
          fp.Type = funcPointer;
          numArguments = IteratorHelper.EnumerableCount(funcPointer.Parameters);
          for (var i = numArguments; i > 0; i--)
            this.stack.Pop();
          if (!funcPointer.IsStatic)
            this.stack.Pop();
          instruction.Type = funcPointer.Type;
          if (funcPointer.Type.TypeCode != PrimitiveTypeCode.Void)
            this.stack.Push(instruction);
          break;
        case OperationCode.Castclass:
        case OperationCode.Isinst:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = (ITypeReference)instruction.Operation.Value;
          if (instruction.Type.ResolvedType.IsValueType || instruction.Type is IGenericParameterReference)
            instruction.Type = this.platformType.SystemObject;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ceq:
        case OperationCode.Cgt:
        case OperationCode.Cgt_Un:
        case OperationCode.Clt:
        case OperationCode.Clt_Un:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemBoolean;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ckfinite:
        case OperationCode.Neg:
        case OperationCode.Not:
          this.stack.Pop();
          Contract.Assume(instruction.Operand1 != null); //Assumed because of the informal specification of the DataFlowInferencer
          instruction.Type = instruction.Operand1.Type;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_I:
        case OperationCode.Conv_Ovf_I:
        case OperationCode.Conv_Ovf_I_Un:
        case OperationCode.Ldind_I:
        case OperationCode.Localloc:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemIntPtr;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_I1:
        case OperationCode.Conv_Ovf_I1:
        case OperationCode.Conv_Ovf_I1_Un:
        case OperationCode.Ldind_I1:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt8;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_I2:
        case OperationCode.Conv_Ovf_I2:
        case OperationCode.Conv_Ovf_I2_Un:
        case OperationCode.Ldind_I2:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt16;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_I4:
        case OperationCode.Conv_Ovf_I4:
        case OperationCode.Conv_Ovf_I4_Un:
        case OperationCode.Ldind_I4:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_I8:
        case OperationCode.Conv_Ovf_I8:
        case OperationCode.Conv_Ovf_I8_Un:
        case OperationCode.Ldind_I8:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_Ovf_U:
        case OperationCode.Conv_Ovf_U_Un:
        case OperationCode.Conv_U:
        case OperationCode.Ldlen:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUIntPtr;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_Ovf_U1:
        case OperationCode.Conv_Ovf_U1_Un:
        case OperationCode.Conv_U1:
        case OperationCode.Ldind_U1:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt8;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_Ovf_U2:
        case OperationCode.Conv_Ovf_U2_Un:
        case OperationCode.Conv_U2:
        case OperationCode.Ldind_U2:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt16;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_Ovf_U4:
        case OperationCode.Conv_Ovf_U4_Un:
        case OperationCode.Conv_U4:
        case OperationCode.Ldind_U4:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_Ovf_U8:
        case OperationCode.Conv_Ovf_U8_Un:
        case OperationCode.Conv_U8:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_R_Un:
          this.stack.Pop();
          Contract.Assume(instruction.Operand1 != null); //Assumed because of the informal specification of the DataFlowInferencer
          if (TypeHelper.SizeOfType(instruction.Operand1.Type) < 4)
            instruction.Type = this.platformType.SystemFloat32;
          else
            instruction.Type = this.platformType.SystemFloat64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_R4:
        case OperationCode.Ldind_R4:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemFloat32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Conv_R8:
        case OperationCode.Ldind_R8:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemFloat64;
          this.stack.Push(instruction);
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
          this.stack.Pop();
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemVoid;
          break;
        case OperationCode.Dup:
          Contract.Assume(instruction.Operand1 != null); //Assumed because of the informal specification of the DataFlowInferencer
          instruction.Type = instruction.Operand1.Type;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldarg:
        case OperationCode.Ldarg_0:
        case OperationCode.Ldarg_1:
        case OperationCode.Ldarg_2:
        case OperationCode.Ldarg_3:
        case OperationCode.Ldarg_S:
          var parameter = instruction.Operation.Value as IParameterDefinition;
          if (parameter == null) { //this arg
            var containingType = this.cfg.MethodBody.MethodDefinition.ContainingTypeDefinition;
            var namedType = containingType as INamedTypeDefinition;
            if (namedType != null && namedType.IsGeneric)
              instruction.Type = namedType.InstanceType;
            else
              instruction.Type = containingType;
            if (instruction.Type.IsValueType)
              instruction.Type = ManagedPointerType.GetManagedPointerType(instruction.Type, this.internFactory);
          } else {
            instruction.Type = parameter.Type;
            if (parameter.IsByReference)
              instruction.Type = ManagedPointerType.GetManagedPointerType(instruction.Type, this.internFactory);
          }
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldarga:
        case OperationCode.Ldarga_S:
          parameter = instruction.Operation.Value as IParameterDefinition;
          if (parameter == null) { //this arg
            instruction.Type = ManagedPointerType.GetManagedPointerType(this.cfg.MethodBody.MethodDefinition.ContainingType, this.internFactory);
          } else {
            instruction.Type = ManagedPointerType.GetManagedPointerType(parameter.Type, this.internFactory);
            if (parameter.IsByReference)
              instruction.Type = ManagedPointerType.GetManagedPointerType(instruction.Type, this.internFactory);
          }
          this.stack.Push(instruction);
          break;
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
          instruction.Type = this.platformType.SystemInt32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldc_I8:
          instruction.Type = this.platformType.SystemInt64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldc_R4:
          instruction.Type = this.platformType.SystemFloat32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldc_R8:
          instruction.Type = this.platformType.SystemFloat64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem:
          this.stack.Pop();
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = (ITypeReference)instruction.Operation.Value;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldobj:
        case OperationCode.Unbox_Any:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = (ITypeReference)instruction.Operation.Value;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_I:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemIntPtr;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_I1:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt8;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_I2:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt16;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_I4:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_I8:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemInt64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_Ref:
          this.stack.Pop();
          this.stack.Pop();
          Contract.Assume(instruction.Operand1 != null); //Assumed because of the informal specification of the DataFlowInferencer
          arrayType = instruction.Operand1.Type as IArrayTypeReference;
          if (arrayType != null)
            instruction.Type = arrayType.ElementType;
          else //Should only get here if the IL is bad.
            instruction.Type = this.platformType.SystemObject;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_R4:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemFloat32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_R8:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemFloat64;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_U1:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt8;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_U2:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt16;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelem_U4:
          this.stack.Pop();
          this.stack.Pop();
          instruction.Type = this.platformType.SystemUInt32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldelema:
          this.stack.Pop();
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ManagedPointerType.GetManagedPointerType((ITypeReference)instruction.Operation.Value, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldfld:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is IFieldReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ((IFieldReference)instruction.Operation.Value).Type;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldsfld:
          Contract.Assume(instruction.Operation.Value is IFieldReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ((IFieldReference)instruction.Operation.Value).Type;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldflda:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is IFieldReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ManagedPointerType.GetManagedPointerType(((IFieldReference)instruction.Operation.Value).Type, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldsflda:
          Contract.Assume(instruction.Operation.Value is IFieldReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ManagedPointerType.GetManagedPointerType(((IFieldReference)instruction.Operation.Value).Type, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldftn:
          Contract.Assume(instruction.Operation.Value is IMethodReference); //This is an informally specified property of the Metadata model.
          instruction.Type = new FunctionPointerType((IMethodReference)instruction.Operation.Value, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldvirtftn:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is IMethodReference); //This is an informally specified property of the Metadata model.
          instruction.Type = new FunctionPointerType((IMethodReference)instruction.Operation.Value, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldind_Ref:
          this.stack.Pop();
          Contract.Assume(instruction.Operand1 != null); //Assumed because of the informal specification of the DataFlowInferencer
          var ptr = instruction.Operand1.Type as IPointerTypeReference;
          if (ptr != null)
            instruction.Type = ptr.TargetType;
          else {
            Contract.Assume(instruction.Operand1.Type is IManagedPointerTypeReference); //This is an informally specified property of the Metadata model.
            instruction.Type = ((IManagedPointerTypeReference)instruction.Operand1.Type).TargetType;
          }
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldloc:
        case OperationCode.Ldloc_0:
        case OperationCode.Ldloc_1:
        case OperationCode.Ldloc_2:
        case OperationCode.Ldloc_3:
        case OperationCode.Ldloc_S:
          var local = instruction.Operation.Value as ILocalDefinition;
          Contract.Assume(local != null); //This is an informally specified property of the Metadata model.
          instruction.Type = local.Type;
          if (local.IsReference)
            instruction.Type = ManagedPointerType.GetManagedPointerType(instruction.Type, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldloca:
        case OperationCode.Ldloca_S:
          local = instruction.Operation.Value as ILocalDefinition;
          Contract.Assume(local != null); //This is an informally specified property of the Metadata model.
          instruction.Type = ManagedPointerType.GetManagedPointerType(local.Type, this.internFactory);
          if (local.IsReference)
            instruction.Type = ManagedPointerType.GetManagedPointerType(instruction.Type, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldnull:
          instruction.Type = this.platformType.SystemObject;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldstr:
          instruction.Type = this.platformType.SystemString;
          this.stack.Push(instruction);
          break;
        case OperationCode.Ldtoken:
          if (instruction.Operation.Value is IMethodReference)
            instruction.Type = this.platformType.SystemRuntimeMethodHandle;
          else if (instruction.Operation.Value is ITypeReference)
            instruction.Type = this.platformType.SystemRuntimeTypeHandle;
          else if (instruction.Operation.Value is IFieldReference)
            instruction.Type = this.platformType.SystemRuntimeFieldHandle;
          else {
            //this should never happen in well formed IL.
            instruction.Type = this.platformType.SystemVoid;
          }
          this.stack.Push(instruction);
          break;
        case OperationCode.Leave:
        case OperationCode.Leave_S:
          this.stack.Clear();
          break;
        case OperationCode.Mkrefany:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemTypedReference;
          this.stack.Push(instruction);
          break;
        case OperationCode.Newarr:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = (ITypeReference)instruction.Operation.Value;
          this.stack.Push(instruction);
          break;
        case OperationCode.Newobj:
          var constructorReference = instruction.Operation.Value as IMethodReference;
          Contract.Assume(constructorReference != null); //This is an informally specified property of the Metadata model.
          for (var i = constructorReference.ParameterCount; i > 0; i--)
            this.stack.Pop();
          instruction.Type = constructorReference.ContainingType;
          this.stack.Push(instruction);
          break;
        case OperationCode.Refanytype:
          this.stack.Pop();
          instruction.Type = this.platformType.SystemRuntimeTypeHandle;
          this.stack.Push(instruction);
          break;
        case OperationCode.Refanyval:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ManagedPointerType.GetManagedPointerType((ITypeReference)instruction.Operation.Value, this.internFactory);
          this.stack.Push(instruction);
          break;
        case OperationCode.Ret:
          if (this.codeIsUnreachable && this.stack.Top < 0) break;
          if (this.cfg.MethodBody.MethodDefinition.Type.TypeCode != PrimitiveTypeCode.Void)
            instruction.Operand1 = this.stack.Pop();
          instruction.Type = this.platformType.SystemVoid;
          break;
        case OperationCode.Shl:
        case OperationCode.Shr:
        case OperationCode.Shr_Un:
          this.stack.Pop();
          this.stack.Pop();
          Contract.Assume(instruction.Operand1 != null); //Assumed because of the informal specification of the DataFlowInferencer
          instruction.Type = instruction.Operand1.Type;
          this.stack.Push(instruction);
          break;
        case OperationCode.Sizeof:
          instruction.Type = this.platformType.SystemUInt32;
          this.stack.Push(instruction);
          break;
        case OperationCode.Unbox:
          this.stack.Pop();
          Contract.Assume(instruction.Operation.Value is ITypeReference); //This is an informally specified property of the Metadata model.
          instruction.Type = ManagedPointerType.GetManagedPointerType((ITypeReference)instruction.Operation.Value, this.internFactory);
          this.stack.Push(instruction);
          break;
        default:
          instruction.Type = this.platformType.SystemVoid;
          break;
      }
    }

    private ITypeReference GetUnsignedBinaryNumericOperationType(Instruction instruction) {
      Contract.Requires(instruction != null);
      return TypeHelper.UnsignedEquivalent(this.GetBinaryNumericOperationType(instruction));
    }

    private ITypeReference GetBinaryNumericOperationType(Instruction instruction) {
      Contract.Requires(instruction != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      var leftOperand = instruction.Operand1;
      var rightOperand = (Instruction)instruction.Operand2;
      Contract.Assume(leftOperand != null); //Assumed because of the informal specification of the DataFlowInferencer
      PrimitiveTypeCode leftTypeCode = leftOperand.Type.TypeCode;
      PrimitiveTypeCode rightTypeCode = rightOperand.Type.TypeCode;
      switch (leftTypeCode) {
        case PrimitiveTypeCode.Boolean:
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt8:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt8:
              return this.platformType.SystemUInt32;

            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
              return this.platformType.SystemUInt32; //code generators will tend to make both operands be of the same type. Assume this happened because the right operand is a polymorphic constant.

            //The cases below are not expected to happen in practice
            case PrimitiveTypeCode.UInt64:
            case PrimitiveTypeCode.Int64:
              return this.platformType.SystemUInt64;

            case PrimitiveTypeCode.UIntPtr:
            case PrimitiveTypeCode.IntPtr:
              return this.platformType.SystemUIntPtr;

            case PrimitiveTypeCode.Float32:
              return this.platformType.SystemFloat32;

            case PrimitiveTypeCode.Float64:
              return this.platformType.SystemFloat64;

            default:
              return rightOperand.Type; //code generators will tend to make both operands be of the same type. Assume this happened because the right operand is an enum.
          }

        case PrimitiveTypeCode.Int8:
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt8:
              return this.platformType.SystemUInt32; //code generators will tend to make both operands be of the same type. Assume this happened because the left operand is a polymorphic constant.

            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
              return this.platformType.SystemInt32;

            //The cases below are not expected to happen in practice
            case PrimitiveTypeCode.UInt64:
              return this.platformType.SystemUInt64;

            case PrimitiveTypeCode.Int64:
              return this.platformType.SystemInt64;

            case PrimitiveTypeCode.UIntPtr:
              return this.platformType.SystemUIntPtr;

            case PrimitiveTypeCode.IntPtr:
              return this.platformType.SystemIntPtr;

            case PrimitiveTypeCode.Float32:
              return this.platformType.SystemFloat32;

            case PrimitiveTypeCode.Float64:
              return this.platformType.SystemFloat64;

            default:
              return rightOperand.Type; //code generators will tend to make both operands be of the same type. Assume this happened because the right operand is an enum.
          }

        case PrimitiveTypeCode.UInt64:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt8:
            case PrimitiveTypeCode.UInt64:
              return this.platformType.SystemUInt64;

            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.Int64:
              return this.platformType.SystemUInt64; //code generators will tend to make both operands be of the same type. Assume this happened because the right operand is a polymorphic constant.

            case PrimitiveTypeCode.UIntPtr:
              return this.platformType.SystemUIntPtr;

            case PrimitiveTypeCode.IntPtr:
              return this.platformType.SystemIntPtr;

            case PrimitiveTypeCode.Float32:
              return this.platformType.SystemFloat32;

            case PrimitiveTypeCode.Float64:
              return this.platformType.SystemFloat64;

            default:
              return rightOperand.Type; //code generators will tend to make both operands be of the same type. Assume this happened because the right operand is an enum.
          }

        case PrimitiveTypeCode.Int64:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt8:
            case PrimitiveTypeCode.UInt64:
              return this.platformType.SystemUInt64; //code generators will tend to make both operands be of the same type. Assume this happened because the left operand is a polymorphic constant.

            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.Int64:
              return this.platformType.SystemInt64;

            case PrimitiveTypeCode.UIntPtr:
            case PrimitiveTypeCode.IntPtr:
              return this.platformType.SystemIntPtr;

            case PrimitiveTypeCode.Float32:
              return this.platformType.SystemFloat32;

            case PrimitiveTypeCode.Float64:
              return this.platformType.SystemFloat64;

            default:
              return rightOperand.Type; //code generators will tend to make both operands be of the same type. Assume this happened because the right operand is an enum.
          }

        case PrimitiveTypeCode.UIntPtr:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt8:
            case PrimitiveTypeCode.UInt64:
            case PrimitiveTypeCode.UIntPtr:
              return this.platformType.SystemUIntPtr;

            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.Int64:
            case PrimitiveTypeCode.IntPtr:
              return this.platformType.SystemUIntPtr;

            case PrimitiveTypeCode.Float32:
              return this.platformType.SystemFloat32;

            case PrimitiveTypeCode.Float64:
              return this.platformType.SystemFloat64;

            case PrimitiveTypeCode.Pointer:
            case PrimitiveTypeCode.Reference:
              return rightOperand.Type;

            default:
              Contract.Assume(false);
              return Dummy.TypeReference;
          }

        case PrimitiveTypeCode.IntPtr:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt8:
            case PrimitiveTypeCode.UInt64:
            case PrimitiveTypeCode.UIntPtr:
              return this.platformType.SystemUIntPtr;

            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.Int64:
            case PrimitiveTypeCode.IntPtr:
              return this.platformType.SystemIntPtr;

            case PrimitiveTypeCode.Float32:
              return this.platformType.SystemFloat32;

            case PrimitiveTypeCode.Float64:
              return this.platformType.SystemFloat64;

            case PrimitiveTypeCode.Pointer:
            case PrimitiveTypeCode.Reference:
              return rightOperand.Type;

            default:
              Contract.Assume(false);
              return Dummy.TypeReference;
          }

        case PrimitiveTypeCode.Float32:
        case PrimitiveTypeCode.Float64:
          return rightOperand.Type;

        case PrimitiveTypeCode.Pointer:
        case PrimitiveTypeCode.Reference:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Pointer:
            case PrimitiveTypeCode.Reference:
              return this.platformType.SystemUIntPtr;
            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.Int64:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt8:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt64:
            case PrimitiveTypeCode.IntPtr:
            case PrimitiveTypeCode.UIntPtr:
              return leftOperand.Type;
            case PrimitiveTypeCode.NotPrimitive:
              return leftOperand.Type; //assume rh type is an enum.
            default:
              Contract.Assume(false);
              return Dummy.TypeReference;
          }

        default:
          switch (rightTypeCode) {
            case PrimitiveTypeCode.Int8:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.Int64:
            case PrimitiveTypeCode.Boolean:
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.UInt8:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.UInt32:
            case PrimitiveTypeCode.UInt64:
              //assume that the left operand has an enum type.
              return leftOperand.Type;
          }
          return leftOperand.Type; //assume they are both enums
      }
    }

  }

}