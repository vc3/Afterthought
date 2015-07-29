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

  internal class ControlFlowInferencer<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.Analysis.BasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Analysis.Instruction, new() {

    private ControlFlowInferencer(IMetadataHost host, IMethodBody methodBody, ILocalScopeProvider/*?*/ localScopeProvider = null) {
      Contract.Requires(host != null);
      Contract.Requires(methodBody != null);

      this.platformType = host.PlatformType;
      this.internFactory = host.InternFactory;
      this.methodBody = methodBody;
      this.localScopeProvider = localScopeProvider;

      int size = 1024;
      var ops = methodBody.Operations as ICollection<IOperation>;
      if (ops != null) size = ops.Count;
      Hashtable<BasicBlock> blockFor = new Hashtable<BasicBlock>((uint)size);
      List<BasicBlock> allBlocks = new List<BasicBlock>(size);
      List<BasicBlock> rootBlocks = new List<BasicBlock>(1+(int)IteratorHelper.EnumerableCount(methodBody.OperationExceptionInformation));
      this.successorEdges = new List<BasicBlock>(size);
      this.cdfg = new ControlAndDataFlowGraph<BasicBlock, Instruction>(methodBody, this.successorEdges, allBlocks, rootBlocks, blockFor);
      this.instructions = new List<Instruction>(size);
      this.blocksThatTarget = new MultiHashtable<BasicBlock>((uint)size);
    }

    IPlatformType platformType;
    IInternFactory internFactory;
    IMethodBody methodBody;
    ILocalScopeProvider/*?*/ localScopeProvider;
    ControlAndDataFlowGraph<BasicBlock, Instruction> cdfg;
    List<BasicBlock> successorEdges;
    List<Instruction> instructions;
    MultiHashtable<BasicBlock> blocksThatTarget;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.platformType != null);
      Contract.Invariant(this.internFactory != null);
      Contract.Invariant(this.methodBody != null);
      Contract.Invariant(this.cdfg != null);
      Contract.Invariant(this.successorEdges != null);
      Contract.Invariant(this.instructions != null);
      Contract.Invariant(this.blocksThatTarget != null);
    }

    /// <summary>
    /// 
    /// </summary>
    internal static ControlAndDataFlowGraph<BasicBlock, Instruction> SetupControlFlow(IMetadataHost host, IMethodBody methodBody, ILocalScopeProvider/*?*/ localScopeProvider = null) {
      Contract.Requires(host != null);
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<ControlAndDataFlowGraph<BasicBlock, Instruction>>() != null);

      var inferencer = new ControlFlowInferencer<BasicBlock, Instruction>(host, methodBody, localScopeProvider);
      return inferencer.CreateBlocksAndEdges();
    }

    private ControlAndDataFlowGraph<BasicBlock, Instruction> CreateBlocksAndEdges() {
      Contract.Ensures(Contract.Result<ControlAndDataFlowGraph<BasicBlock, Instruction>>() != null);

      var firstBlock = new BasicBlock();
      this.cdfg.BlockFor[0] = firstBlock;
      this.cdfg.RootBlocks.Add(firstBlock);
      this.CreateBlocksForLocalScopes();
      this.CreateBlocksForBranchTargetsAndFallthroughs();
      this.CreateBlocksForExceptionHandlers();
      this.CreateSuccessorEdges(firstBlock);

      this.successorEdges.TrimExcess();
      this.instructions.TrimExcess();
      this.cdfg.AllBlocks.TrimExcess();
      this.cdfg.RootBlocks.TrimExcess();
      return this.cdfg;
    }

    private void CreateBlocksForLocalScopes() {
      if (this.localScopeProvider == null) return;
      foreach (var scope in this.localScopeProvider.GetLocalScopes(this.methodBody)) {
        Contract.Assume(scope != null);
        this.CreateBlock(scope.Offset);
        this.CreateBlock(scope.Offset+scope.Length);
      }
    }

    private void CreateBlocksForBranchTargetsAndFallthroughs() {
      bool lastInstructionWasBranch = false;
      foreach (var ilOperation in this.methodBody.Operations) {
        if (lastInstructionWasBranch) {
          this.CreateBlock(ilOperation.Offset);
          lastInstructionWasBranch = false;
        }
        switch (ilOperation.OperationCode) {
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
          case OperationCode.Br:
          case OperationCode.Br_S:
          case OperationCode.Brfalse:
          case OperationCode.Brfalse_S:
          case OperationCode.Brtrue:
          case OperationCode.Brtrue_S:
          case OperationCode.Leave:
          case OperationCode.Leave_S:
            Contract.Assume(ilOperation.Value is uint); //This is an informally specified property of the Metadata model.
            this.CreateBlock((uint)ilOperation.Value);
            lastInstructionWasBranch = true;
            break;
          case OperationCode.Ret:
          case OperationCode.Throw:
          case OperationCode.Jmp:
            //The code following these instructions will be dead unless its a branch target, but we may as well end the basic block with the transfer.
            lastInstructionWasBranch = true;
            break;
          case OperationCode.Switch: {
              Contract.Assume(ilOperation.Value is uint[]); //This is an informally specified property of the Metadata model.
              uint[] branches = (uint[])ilOperation.Value;
              foreach (uint targetAddress in branches)
                this.CreateBlock(targetAddress);
            }
            lastInstructionWasBranch = true;
            break;
          default:
            break;
        }
      }
    }

    private void CreateBlocksForExceptionHandlers() {
      foreach (IOperationExceptionInformation exinfo in this.methodBody.OperationExceptionInformation) {
        this.CreateBlock(exinfo.TryStartOffset);
        var block = CreateBlock(exinfo.HandlerStartOffset);
        this.cdfg.RootBlocks.Add(block);
        if (exinfo.HandlerKind == HandlerKind.Filter) {
          block = this.CreateBlock(exinfo.FilterDecisionStartOffset);
          this.cdfg.RootBlocks.Add(block);
        }
        this.CreateBlock(exinfo.HandlerEndOffset);
      }
    }

    private BasicBlock CreateBlock(uint targetAddress) {
      var result = this.cdfg.BlockFor[targetAddress];
      if (result == null) {
        result = new BasicBlock() { Offset = targetAddress };
        this.cdfg.BlockFor[targetAddress] = result;
      }
      return result;
    }

    private void CreateSuccessorEdges(BasicBlock currentBlock) {
      Contract.Requires(currentBlock != null);

      this.cdfg.AllBlocks.Add(currentBlock);
      int startingEdge = 0;
      int startingInstruction = 0;
      bool lastInstructionWasUnconditionalTransfer = false;
      foreach (var ilOperation in this.methodBody.Operations) {
        Contract.Assert(ilOperation != null); //This is formally specified in the Metadata model, but the checker does not yet understand it well enough to prove this.
        Contract.Assume(startingInstruction <= instructions.Count); //due to the limitations of the contract language and checker
        Contract.Assume(startingEdge <= successorEdges.Count); //due to the limitations of the contract language and checker
        var newBlock = this.cdfg.BlockFor.Find(ilOperation.Offset);
        if (newBlock != null && currentBlock != newBlock) {
          this.cdfg.AllBlocks.Add(newBlock);
          currentBlock.Instructions = new Sublist<Instruction>(instructions, startingInstruction, instructions.Count-startingInstruction);
          if (!lastInstructionWasUnconditionalTransfer)
            this.AddToSuccessorListIfNotAlreadyInIt(successorEdges, startingEdge, newBlock, currentBlock);
          currentBlock.firstSuccessorEdge = startingEdge;
          currentBlock.successorCount = successorEdges.Count-startingEdge;
          startingEdge = successorEdges.Count;
          startingInstruction = instructions.Count;
          currentBlock = newBlock;
        }
        instructions.Add(this.GetInstruction(ilOperation, currentBlock, successorEdges, out lastInstructionWasUnconditionalTransfer));
      }
      if (instructions.Count > startingInstruction)
        currentBlock.Instructions = new Sublist<Instruction>(instructions, startingInstruction, instructions.Count-startingInstruction);
      if (successorEdges.Count > startingEdge) {
        currentBlock.firstSuccessorEdge = startingEdge;
        currentBlock.successorCount = successorEdges.Count-startingEdge;
      }
    }

    private void AddToSuccessorListIfNotAlreadyInIt(List<BasicBlock> edges, int startingEdge, BasicBlock target, BasicBlock current) {
      Contract.Requires(edges != null);
      Contract.Requires(startingEdge >= 0);
      Contract.Requires(target != null);
      Contract.Requires(current != null);
      Contract.Ensures(Contract.OldValue(edges.Count) <= edges.Count);

      for (int i = startingEdge, n = edges.Count; i < n; i++) {
        if (edges[i] == target) return;
      }
      edges.Add(target);
      this.blocksThatTarget.Add(target.Offset, current);
    }

    private Instruction GetInstruction(IOperation ilOperation, BasicBlock currentBlock, List<BasicBlock> edges, out bool isUnconditionalTransfer) {
      Contract.Requires(ilOperation != null);
      Contract.Requires(currentBlock != null);
      Contract.Requires(edges != null);

      isUnconditionalTransfer = false;
      var instruction = new Instruction() { Operation = ilOperation };
      switch (ilOperation.OperationCode) {
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
          Contract.Assume(ilOperation.Value is uint); //This is an informally specified property of the Metadata model.
          var targetOffset = (uint)ilOperation.Value;
          this.blocksThatTarget.Add(targetOffset, currentBlock);
          edges.Add(this.cdfg.BlockFor[targetOffset]);
          break;

        case OperationCode.Br:
        case OperationCode.Br_S:
        case OperationCode.Leave:
        case OperationCode.Leave_S:
          Contract.Assume(ilOperation.Value is uint); //This is an informally specified property of the Metadata model.
          targetOffset = (uint)ilOperation.Value;
          this.blocksThatTarget.Add(targetOffset, currentBlock);
          edges.Add(this.cdfg.BlockFor[targetOffset]);
          isUnconditionalTransfer = true;
          break;

        case OperationCode.Brfalse:
        case OperationCode.Brfalse_S:
        case OperationCode.Brtrue:
        case OperationCode.Brtrue_S:
          Contract.Assume(ilOperation.Value is uint); //This is an informally specified property of the Metadata model.
          targetOffset = (uint)ilOperation.Value;
          this.blocksThatTarget.Add(targetOffset, currentBlock);
          edges.Add(this.cdfg.BlockFor[targetOffset]);
          break;

        case OperationCode.Endfilter:
        case OperationCode.Endfinally:
        case OperationCode.Jmp:
        case OperationCode.Ret:
        case OperationCode.Rethrow:
        case OperationCode.Throw:
          isUnconditionalTransfer = true;
          break;

        case OperationCode.Switch:
          this.AddEdgesForSwitch(ilOperation, currentBlock, edges, instruction);
          break;
      }
      return instruction;
    }

    private void AddEdgesForSwitch(IOperation ilOperation, BasicBlock currentBlock, List<BasicBlock> edges, Instruction instruction) {
      Contract.Requires(ilOperation != null);
      Contract.Requires(currentBlock != null);
      Contract.Requires(ilOperation.OperationCode == OperationCode.Switch);
      Contract.Requires(edges != null);
      Contract.Requires(instruction != null);

      Contract.Assume(ilOperation.Value is uint[]);  //This is an informally specified property of the Metadata model.
      uint[] branches = (uint[])ilOperation.Value;
      SetOfObjects currentSuccesors = new SetOfObjects((uint)branches.Length);
      foreach (uint targetAddress in branches) {
        this.blocksThatTarget.Add(targetAddress, currentBlock);
        var target = this.cdfg.BlockFor[targetAddress];
        Contract.Assume(target != null); //All branch targets must have blocks, but we can't put that in a contract that satisfies the checker.
        if (currentSuccesors.Contains(target)) continue;
        currentSuccesors.Add(target);
        edges.Add(target);
      }
    }


  }
}
