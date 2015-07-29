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
using System.Text;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci.Analysis {

  /// <summary>
  /// Presents information derived from a simple control flow graph. For example, traversal orders, predecessors, dominators and dominance frontiers.
  /// </summary>
  public class ControlGraphQueries<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.Analysis.EnhancedBasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Analysis.Instruction, new() {

    /// <summary>
    /// Presents information derived from a simple control flow graph. For example, traversal orders, predecessors, dominators and dominance frontiers.
    /// </summary>
    /// <param name="controlFlowGraph">The simple control flow graph from which to derive the information.</param>
    public ControlGraphQueries(ControlAndDataFlowGraph<BasicBlock, Instruction> controlFlowGraph) {
      Contract.Requires(controlFlowGraph != null);

      this.cfg = controlFlowGraph;
    }

    ControlAndDataFlowGraph<BasicBlock, Instruction> cfg;
    BasicBlock[] preOrder;
    BasicBlock[] postOrder;
    List<BasicBlock> predecessorEdges;
    List<BasicBlock> dominanceFrontier;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.cfg != null);
    }

    /// <summary>
    /// Contains the same nodes as the AllBlocks property of the control flow graph, but in the order they will be visited by a depth first, post order traversal of successor nodes.
    /// </summary>
    public BasicBlock[] BlocksInPostorder {
      get {
        Contract.Ensures(Contract.Result<BasicBlock[]>() != null);
        if (this.postOrder == null)
          this.SetupTraversalOrders();
        return this.postOrder; 
      }
    }

    /// <summary>
    /// Contains the same nodes as the AllBlocks property of the control flow graph, but in the order they will be visited by a depth first, pre order traversal of successor nodes.
    /// </summary>
    public BasicBlock[] BlocksInPreorder {
      get {
        Contract.Ensures(Contract.Result<BasicBlock[]>() != null);
        if (this.preOrder == null)
          this.SetupTraversalOrders();
        return this.preOrder; 
      }
    }

    /// <summary>
    /// Returns zero or more nodes that are reachable from the given basic block, but are not dominated by the given basic block.
    /// </summary>
    public Sublist<BasicBlock> DominanceFrontierFor(BasicBlock basicBlock) {
      Contract.Requires(basicBlock != null);
      if (this.dominanceFrontier == null)
        this.SetupDominanceFrontier();

      if (basicBlock.firstDominanceFrontierNode+basicBlock.dominanceFrontierCount > this.dominanceFrontier.Count)
        throw new InvalidOperationException(); //can only happen if the basic block does not belong to this graph.
      Contract.Assume(basicBlock.firstDominanceFrontierNode >= 0);
      Contract.Assume(basicBlock.dominanceFrontierCount >= 0);
      return new Sublist<BasicBlock>(this.dominanceFrontier, basicBlock.firstDominanceFrontierNode, basicBlock.dominanceFrontierCount);
    }

    /// <summary>
    /// Returns true if the first block dominates the second block. That is, if all control paths from the applicable root node
    /// lead to the second block only via the first block.
    /// </summary>
    public bool Dominates(BasicBlock block1, BasicBlock block2) {
        Contract.Requires(block1 != null);
        Contract.Requires(block2 != null);

        if (block1 == block2) return true;
        var block2dominator = ImmediateDominator(block2);
        while (true) {
            if (block1 == block2dominator) return true;
            if (block2 == block2dominator) return false;
            block2 = block2dominator;
            block2dominator = ImmediateDominator(block2);
        }
    }

    /// <summary>
    /// Returns the last block through which all control flows from a root must pass in order to reach the given block. 
    /// This block can be a root, however, it will not be the given block, except when the given block is a root.
    /// </summary>
    public BasicBlock ImmediateDominator(BasicBlock basicBlock) {
      Contract.Requires(basicBlock != null);
      Contract.Ensures(Contract.Result<BasicBlock>() != null);

      if (!this.immediateDominatorsAreInitialized)
        this.SetupImmediateDominators();
      Contract.Assume(basicBlock.immediateDominator is BasicBlock);
      return (BasicBlock)basicBlock.immediateDominator;
    }
    bool immediateDominatorsAreInitialized;

    /// <summary>
    /// All basic blocks from which control can flow to the given basic block.
    /// </summary>
    public Sublist<BasicBlock> PredeccessorsFor(BasicBlock basicBlock) {
      Contract.Requires(basicBlock != null);
      if (this.predecessorEdges == null)
        this.SetupPredecessorEdges();

      if (basicBlock.firstPredecessorEdge+basicBlock.predeccessorCount > this.predecessorEdges.Count)
        throw new InvalidOperationException(); //can only happen if the basic block does not belong to this graph.
      Contract.Assume(basicBlock.firstPredecessorEdge >= 0);
      Contract.Assume(basicBlock.predeccessorCount >= 0);
      return new Sublist<BasicBlock>(this.predecessorEdges, basicBlock.firstPredecessorEdge, basicBlock.predeccessorCount);
    }

    private void SetupDominanceFrontier() {
      Contract.Ensures(this.dominanceFrontier != null);
      MultiHashtable<BasicBlock> frontierFor = new MultiHashtable<BasicBlock>();

      if (!this.immediateDominatorsAreInitialized)
        this.SetupImmediateDominators();
      var predecessorEdges = this.predecessorEdges;
      Contract.Assume(predecessorEdges != null);

      var dominanceFrontier = this.dominanceFrontier = new List<BasicBlock>(this.cfg.AllBlocks.Count*2);
      foreach (var block in this.cfg.AllBlocks) {
        Contract.Assume(block != null);
        var n = block.predeccessorCount;
        if (n < 2) continue;
        for (int i = 0; i < n; i++) {
          Contract.Assume(block.firstPredecessorEdge+i >= 0);
          Contract.Assume(block.firstPredecessorEdge+i < predecessorEdges.Count);
          var pred = predecessorEdges[block.firstPredecessorEdge+i];
          Contract.Assume(pred != null);
          var a = pred;
          while (true) {
            if (a == block.immediateDominator) break; //Any node that dominates node a will also dominate node block and hence block will not be in its dominance frontier.
            frontierFor.Add(a.Offset, block);
            if (a == a.immediateDominator) break; //Since there are multiple roots, block can be its own immediate dominator while still having predecessors.
            a = (BasicBlock)a.immediateDominator;
            Contract.Assume(a != null);
          }
        }
      }
      foreach (var block in this.cfg.AllBlocks) {
        Contract.Assume(block != null);
        block.firstDominanceFrontierNode = dominanceFrontier.Count;
        foreach (var frontierNode in frontierFor.GetValuesFor(block.Offset)) {
          dominanceFrontier.Add(frontierNode);
        }
        block.dominanceFrontierCount = dominanceFrontier.Count-block.firstDominanceFrontierNode;
      }
      dominanceFrontier.TrimExcess();
    }

    private void SetupImmediateDominators() {
      Contract.Ensures(this.immediateDominatorsAreInitialized);
      //Note this is an adaptation of the algorithm in Cooper, Keith D.; Harvey, Timothy J.; and Kennedy, Ken (2001). A Simple, Fast Dominance Algorithm
      //The big difference is that we deal with multiple roots at the same time.

      if (this.postOrder == null)
        this.SetupTraversalOrders();
      var postOrder = this.postOrder;
      if (this.predecessorEdges == null)
        this.SetupPredecessorEdges();
      foreach (var rootBlock in this.cfg.RootBlocks) {
        Contract.Assume(rootBlock != null);
        rootBlock.immediateDominator = rootBlock;
      }
      var predecessorEdges = this.predecessorEdges;
      var n = postOrder.Length;
      var changed = true;
      while (changed) {
        changed = false;
        for (int i = n-1; i >= 0; i--) { //We iterate in reverse post order so that a block always has its immediateDominator field filled in before we get to any of its successors.
          var b = postOrder[i];
          Contract.Assume(b != null);
          if (b.immediateDominator == b) continue;
          if (b.predeccessorCount == 0) {
            b.immediateDominator = b;
            continue; 
          }
          Contract.Assume(b.firstPredecessorEdge >= 0);
          Contract.Assume(b.firstPredecessorEdge < predecessorEdges.Count);
          var predecessors = new HashSet<BasicBlock>();
          for (int j = 0, m = b.predeccessorCount; j < m; j++) {
              predecessors.Add(predecessorEdges[b.firstPredecessorEdge + j]);
          }
          // newIDom <- first (processed) predecessor of b
          BasicBlock newIDom = null;
          foreach (var p in predecessors) {
            if (p.immediateDominator != null) {
              newIDom = p;
              break;
            }
	      }
          Contract.Assume(newIDom != null);
          predecessors.Remove(newIDom);
          foreach (var predecessor in predecessors) {
            Contract.Assume(predecessor != null);
            if (predecessor.immediateDominator != null) {
              var intersection = Intersect(predecessor, newIDom);
              if (intersection != null) {
                newIDom = intersection;
              } else {
                //This can happen when predecessor and newIDom are only reachable via distinct roots.
                //We now have two distinct paths from a root to b. This means b is its own dominator.
                b.immediateDominator = newIDom = b;
                break;
              }
            }
          }
          if (b.immediateDominator != newIDom) {
            b.immediateDominator = newIDom;
            changed = true;
          }
        }
      }
      this.immediateDominatorsAreInitialized = true;
    }

    private BasicBlock/*?*/ Intersect(BasicBlock block1, BasicBlock block2) {
      Contract.Requires(block1 != null);
      Contract.Requires(block2 != null);

      while (block1 != block2) {
        while (block1.postOrderNumber < block2.postOrderNumber) {
          var block1dominator = block1.immediateDominator;
          if (block1dominator == block1) return null; //block2 is its own dominator, which means it has no predecessors
          block1 = (BasicBlock)block1dominator; //The block with the smaller post order number cannot be a predecessor of the other block.
          if (block1 == null) return null;
        }
        while (block2.postOrderNumber < block1.postOrderNumber) {
          var block2dominator = block2.immediateDominator;
          if (block2dominator == block2) return null; //block2 is its own dominator, which means it has no predecessors
          block2 = (BasicBlock)block2dominator; //The block with the smaller post order number cannot be a predecessor of the other block.
          if (block2 == null) return null;
        }
      }
      return block1;
    }

    private void SetupPredecessorEdges() {
      Contract.Ensures(this.predecessorEdges != null);

      var predecessorEdges = this.predecessorEdges = new List<BasicBlock>(this.cfg.SuccessorEdges.Count);
      MultiHashtable<BasicBlock> blocksThatTarget = new MultiHashtable<BasicBlock>();
      foreach (var block in this.cfg.AllBlocks) {
        Contract.Assume(block != null);
        foreach (var successor in this.cfg.SuccessorsFor(block)) {
          blocksThatTarget.Add(successor.Offset, block);
        }
      }
      foreach (var block in this.cfg.AllBlocks) {
        Contract.Assume(block != null);
        block.firstPredecessorEdge = predecessorEdges.Count;
        foreach (var predecessor in blocksThatTarget.GetValuesFor(block.Offset)) {
          predecessorEdges.Add(predecessor);
        }
        block.predeccessorCount = predecessorEdges.Count-block.firstPredecessorEdge;
      }
    }

    private void SetupTraversalOrders() {
      Contract.Ensures(this.postOrder != null);
      Contract.Ensures(this.preOrder != null);

      var n = cfg.AllBlocks.Count;
      this.postOrder = new BasicBlock[n];
      this.preOrder = new BasicBlock[n];
      uint preorderCounter = 0;
      uint postorderCounter = 0;
      var alreadyTraversed = new SetOfObjects((uint)n);
      foreach (var rootBlock in cfg.RootBlocks) {
        Contract.Assume(rootBlock != null);
        this.SetupTraversalOrders(rootBlock, alreadyTraversed, ref preorderCounter, ref postorderCounter);
      }
      Contract.Assume(preorderCounter == postorderCounter);
      if (preorderCounter != n) {      
        //Add unreachable blocks to traversal order, treating them as if they were roots.
        foreach (var block in cfg.AllBlocks) {
          Contract.Assume(block != null);
          if (alreadyTraversed.Contains(block)) continue;
          this.SetupTraversalOrders(block, alreadyTraversed, ref preorderCounter, ref postorderCounter);
        }
      }
      Contract.Assume(this.postOrder != null);
      Contract.Assume(this.preOrder != null);
    }

    private void SetupTraversalOrders(BasicBlock root, SetOfObjects alreadyTraversed, ref uint preOrderIndex, ref uint postOrderIndex) {
      Contract.Requires(root != null);
      Contract.Requires(alreadyTraversed != null);

      if (!alreadyTraversed.Add(root)) return;
      Contract.Assume(this.preOrder != null);
      Contract.Assume(this.postOrder != null);
      Contract.Assume(preOrderIndex < this.preOrder.Length);
      this.preOrder[preOrderIndex++] = root;
      foreach (var successor in this.cfg.SuccessorsFor(root)) {
        Contract.Assume(successor != null);
        this.SetupTraversalOrders(successor, alreadyTraversed, ref preOrderIndex, ref postOrderIndex);
      }
      Contract.Assume(this.postOrder != null);
      Contract.Assume(postOrderIndex < this.postOrder.Length);
      root.postOrderNumber = postOrderIndex;
      this.postOrder[postOrderIndex++] = root;
    }


  }

  /// <summary>
  /// A basic block with additional fields to help compute things such as predecessor edges, dominance and dominance frontiers.
  /// </summary>
  /// <typeparam name="Instruction"></typeparam>
  public class EnhancedBasicBlock<Instruction> : BasicBlock<Instruction> where Instruction : Microsoft.Cci.Analysis.Instruction {
    /// <summary>
    /// The first block in a list of blocks that are reachable from, but not dominated by this block.
    /// </summary>
    internal int firstDominanceFrontierNode;

    /// <summary>
    /// The number of blocks that are reachable from, but not dominated by this block.
    /// </summary>
    internal int dominanceFrontierCount;

    /// <summary>
    /// The first edge that enters this block. The edges are a contiguous sublist of the the PredeccessorEdges list of the ControlAndDataFlowGraph that contains this block.
    /// </summary>
    internal int firstPredecessorEdge;

    /// <summary>
    /// The number of edges that enter this block. The edges are a contiguous sublist of the the PredeccessorEdges list of the ControlAndDataFlowGraph that contains this block.
    /// </summary>
    internal int predeccessorCount;

    /// <summary>
    /// The block through which all control flows from a root must pass in order to reach this block. Can be a root. Will not be the block itself, except when the block is a root.
    /// </summary>
    internal EnhancedBasicBlock<Instruction> immediateDominator; 

    /// <summary>
    /// The position of the node in a depth first, post order traversal of successor edges.
    /// </summary>
    internal uint postOrderNumber;

  }

}
