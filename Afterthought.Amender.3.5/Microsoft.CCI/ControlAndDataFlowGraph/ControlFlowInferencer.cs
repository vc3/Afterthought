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

namespace Microsoft.Cci.ControlAndDataFlowGraph {

  internal class ControlFlowInferencer<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.BasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Instruction, new() {

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
      this.edges = new List<BasicBlock>(size);
      this.cdfg = new ControlAndDataFlowGraph<BasicBlock, Instruction>(methodBody, this.edges, allBlocks, rootBlocks, blockFor);
      this.instructions = new List<Instruction>(size);
    }

    IPlatformType platformType;
    IInternFactory internFactory;
    IMethodBody methodBody;
    ILocalScopeProvider/*?*/ localScopeProvider;
    ControlAndDataFlowGraph<BasicBlock, Instruction> cdfg;
    List<BasicBlock> edges;
    List<Instruction> instructions;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.platformType != null);
      Contract.Invariant(this.internFactory != null);
      Contract.Invariant(this.methodBody != null);
      Contract.Invariant(this.cdfg != null);
      Contract.Invariant(this.edges != null);
      Contract.Invariant(this.instructions != null);
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
      this.CreateEdges(firstBlock);

      this.edges.TrimExcess();
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
        result = new BasicBlock();
        this.cdfg.BlockFor[targetAddress] = result;
      }
      return result;
    }

    private void CreateEdges(BasicBlock currentBlock) {
      Contract.Requires(currentBlock != null);

      this.cdfg.AllBlocks.Add(currentBlock);
      int startingEdge = 0;
      int startingInstruction = 0;
      bool lastInstructionWasUnconditionalTransfer = false;
      foreach (var ilOperation in this.methodBody.Operations) {
        Contract.Assume(ilOperation != null); //This is formally specified in the Metadata model, but the checker does not yet understand it well enough to prove this.
        Contract.Assume(startingInstruction <= instructions.Count); //due to the limitations of the contract language and checker
        Contract.Assume(startingEdge <= edges.Count); //due to the limitations of the contract language and checker
        var newBlock = this.cdfg.BlockFor.Find(ilOperation.Offset);
        if (newBlock != null && currentBlock != newBlock) {
          this.cdfg.AllBlocks.Add(newBlock);
          currentBlock.Instructions = new Sublist<Instruction>(instructions, startingInstruction, instructions.Count-startingInstruction);
          if (!lastInstructionWasUnconditionalTransfer)
            AddToSuccessorListIfNotAlreadyInIt(edges, startingEdge, newBlock);
          currentBlock.firstSuccessorEdge = startingEdge;
          currentBlock.successorCount = edges.Count-startingEdge;
          startingEdge = edges.Count;
          startingInstruction = instructions.Count;
          currentBlock = newBlock;
        }
        instructions.Add(this.GetInstruction(ilOperation, edges, out lastInstructionWasUnconditionalTransfer));
      }
      if (instructions.Count > startingInstruction)
        currentBlock.Instructions = new Sublist<Instruction>(instructions, startingInstruction, instructions.Count-startingInstruction);
      if (edges.Count > startingEdge) {
        currentBlock.firstSuccessorEdge = startingEdge;
        currentBlock.successorCount = edges.Count-startingEdge;
      }
    }

    private static void AddToSuccessorListIfNotAlreadyInIt(List<BasicBlock> edges, int startingEdge, BasicBlock target) {
      Contract.Requires(edges != null);
      Contract.Requires(startingEdge >= 0);
      Contract.Requires(target != null);
      Contract.Ensures(Contract.OldValue(edges.Count) <= edges.Count);

      for (int i = startingEdge, n = edges.Count; i < n; i++) {
        if (edges[i] == target) return;
      }
      edges.Add(target);
    }

    private Instruction GetInstruction(IOperation ilOperation, List<BasicBlock> edges, out bool isUnconditionalTransfer) {
      Contract.Requires(ilOperation != null);
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
          edges.Add(this.cdfg.BlockFor[(uint)ilOperation.Value]);
          break;

        case OperationCode.Br:
        case OperationCode.Br_S:
        case OperationCode.Leave:
        case OperationCode.Leave_S:
          Contract.Assume(ilOperation.Value is uint); //This is an informally specified property of the Metadata model.
          edges.Add(this.cdfg.BlockFor[(uint)ilOperation.Value]);
          isUnconditionalTransfer = true;
          break;

        case OperationCode.Brfalse:
        case OperationCode.Brfalse_S:
        case OperationCode.Brtrue:
        case OperationCode.Brtrue_S:
          Contract.Assume(ilOperation.Value is uint); //This is an informally specified property of the Metadata model.
          edges.Add(this.cdfg.BlockFor[(uint)ilOperation.Value]);
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
          this.AddEdgesForSwitch(ilOperation, edges, instruction);
          break;
      }
      return instruction;
    }

    private void AddEdgesForSwitch(IOperation ilOperation, List<BasicBlock> edges, Instruction instruction) {
      Contract.Requires(ilOperation != null);
      Contract.Requires(ilOperation.OperationCode == OperationCode.Switch);
      Contract.Requires(edges != null);
      Contract.Requires(instruction != null);

      Contract.Assume(ilOperation.Value is uint[]);  //This is an informally specified property of the Metadata model.
      uint[] branches = (uint[])ilOperation.Value;
      SetOfObjects currentSuccesors = new SetOfObjects((uint)branches.Length);
      foreach (uint targetAddress in branches) {
        var target = this.cdfg.BlockFor[targetAddress];
        Contract.Assume(target != null); //All branch targets must have blocks, but we can't put that in a contract that satisfies the checker.
        if (currentSuccesors.Contains(target)) continue;
        currentSuccesors.Add(target);
        edges.Add(target);
      }
    }
  }
}
