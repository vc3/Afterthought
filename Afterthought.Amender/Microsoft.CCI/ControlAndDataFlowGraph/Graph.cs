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

namespace Microsoft.Cci {
  using Microsoft.Cci.ControlAndDataFlowGraph;

  /// <summary>
  /// A set of basic blocks, each of which has a list of successor blocks and some other information.
  /// Each block consists of a list of instructions, each of which can point to previous instructions that compute the operands it consumes.
  /// </summary>
  public class ControlAndDataFlowGraph<BasicBlock, Instruction> 
    where BasicBlock : Microsoft.Cci.BasicBlock<Instruction>, new ()
    where Instruction : Microsoft.Cci.Instruction, new () {

    internal ControlAndDataFlowGraph(IMethodBody body, List<BasicBlock> successorEdges, List<BasicBlock> allBlocks, List<BasicBlock> rootBlocks, Hashtable<BasicBlock> blockFor) {
      Contract.Requires(body != null);
      Contract.Requires(successorEdges != null);
      Contract.Requires(allBlocks != null);
      Contract.Requires(rootBlocks != null);
      Contract.Requires(blockFor != null);

      this.methodBody = body;
      this.successorEdges = successorEdges;
      this.allBlocks = allBlocks;
      this.rootBlocks = rootBlocks;
      this.blockFor = blockFor;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.methodBody != null);
      Contract.Invariant(this.successorEdges != null);
      Contract.Invariant(this.allBlocks != null);
      Contract.Invariant(this.rootBlocks != null);
      Contract.Invariant(this.blockFor != null);
    }

    /// <summary>
    /// The method body for which this instance is a Control and Data Flow Graph.
    /// </summary>
    public IMethodBody MethodBody {
      get {
        Contract.Ensures(Contract.Result<IMethodBody>() != null);
        return this.methodBody; 
      }
      set {
        Contract.Requires(value != null);
        this.methodBody = value; 
      }
    }
    private IMethodBody methodBody;

    /// <summary>
    /// The first block in the method as well as the first blocks of any exception handlers, fault handlers and finally clauses.
    /// </summary>
    public List<BasicBlock> RootBlocks {
      get {
        Contract.Ensures(Contract.Result<List<BasicBlock>>() != null);
        return this.rootBlocks; 
      }
      set {
        Contract.Requires(value != null);
        this.rootBlocks = value; 
      }
    }
    List<BasicBlock> rootBlocks;

    /// <summary>
    /// A list of all basic blocks in the graph, ordered so that any block that ends on a conditional branch immediately precedes the block
    /// to which it falls through and so that all blocks that make up a try body or handler are contiguous.
    /// </summary>
    public List<BasicBlock> AllBlocks {
      get {
        Contract.Ensures(Contract.Result<List<BasicBlock>>() != null);
        return this.allBlocks;
      }
      set {
        Contract.Requires(value != null);
        this.allBlocks = value;
      }
    }
    List<BasicBlock> allBlocks;

    /// <summary>
    /// A map from IL offset to corresponding basic block.
    /// </summary>
    public Hashtable<BasicBlock> BlockFor {
      get {
        Contract.Ensures(Contract.Result<Hashtable<BasicBlock>>() != null);
        return this.blockFor; 
      }
      set {
        Contract.Requires(value != null);
        this.blockFor = value; 
      }
    }
    private Hashtable<BasicBlock> blockFor;

    /// <summary>
    /// The master list of all successor edges. The successor list for each basic block is a sublist of this list.
    /// </summary>
    public List<BasicBlock> SuccessorEdges {
      get {
        Contract.Ensures(Contract.Result<List<BasicBlock>>() != null);
        return this.successorEdges;
      }
      set {
        Contract.Requires(value != null);
        this.successorEdges = value;
      }
    }
    List<BasicBlock> successorEdges;

    /// <summary>
    /// All basic blocks that can be reached via control flow out of the given basic block.
    /// </summary>
    public Sublist<BasicBlock> SuccessorsFor(BasicBlock basicBlock) {
      Contract.Requires(basicBlock != null);

      if (basicBlock.firstSuccessorEdge+basicBlock.successorCount > this.SuccessorEdges.Count)
        throw new InvalidOperationException(); //can only happen if the basic block does not belong to this graph.
      Contract.Assume(basicBlock.firstSuccessorEdge >= 0);
      Contract.Assume(basicBlock.successorCount >= 0);
      return new Sublist<BasicBlock>(this.SuccessorEdges, basicBlock.firstSuccessorEdge, basicBlock.successorCount);
    }

    /// <summary>
    /// Constructs a control and data flow graph for the given method body.
    /// </summary>
    public static ControlAndDataFlowGraph<BasicBlock, Instruction> GetControlAndDataFlowGraphFor(IMetadataHost host, IMethodBody methodBody, ILocalScopeProvider/*?*/ localScopeProvider = null) {
      Contract.Requires(host != null);
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<ControlAndDataFlowGraph<BasicBlock, Instruction>>() != null);
      
      var cdfg = ControlFlowInferencer<BasicBlock, Instruction>.SetupControlFlow(host, methodBody, localScopeProvider);
      DataFlowInferencer<BasicBlock, Instruction>.SetupDataFlow(host, methodBody, cdfg);
      TypeInferencer<BasicBlock, Instruction>.FillInTypes(host, cdfg);

      return cdfg;
    }

  }

  /// <summary>
  /// A block of instructions of which only the first instruction can be reached via explicit control flow.
  /// </summary>
  public class BasicBlock<Instruction> {

    /// <summary>
    /// The first edge in the ControlAndDataFlowGraph that contains the basic block, which represents a control flow from this block to a successor block.
    /// </summary>
    internal int firstSuccessorEdge;

    /// <summary>
    /// The number of edges that leave this block. The edges are a contiguous sublist of the the SuccessorEdges list of the ControlAndDataFlowGraph that contains this block.
    /// </summary>
    internal int successorCount;

    /// <summary>
    /// A list of pseudo instructions that initialize the operand stack when the block is entered. No actual code should be generated for these instructions
    /// as the actual stack will be set up by the code transferring control to this block.
    /// </summary>
    public Sublist<Instruction> OperandStack; //TODO: should we have explicit Phi nodes?

    /// <summary>
    /// The instructions making up this block.
    /// </summary>
    public Sublist<Instruction> Instructions;

  }

  /// <summary>
  /// A model of an IL operation, but with the implicit operand stack made explicit via the properties Operand1 and Operand2
  /// that point to the previous instructions that computed the operands, if any, that the instruction consumes.
  /// </summary>
  public class Instruction {

    /// <summary>
    /// A model of an IL operation, but with the implicit operand stack made explicit via the properties Operand1 and Operand2
    /// that point to the previous instructions that computed the operands, if any, that the instruction consumes.
    /// </summary>
    public Instruction() {
      this.operation = Dummy.Operation;
      this.type = Dummy.Type;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.operation != null);
      Contract.Invariant(this.type != null);
    }

    /// <summary>
    /// The operation this instruction carries out.
    /// </summary>
    public IOperation Operation {
      get {
        Contract.Ensures(Contract.Result<IOperation>() != null);
        return operation; 
      }
      set {
        Contract.Requires(value != null);
        operation = value; 
      }
    }
    private IOperation operation;

    /// <summary>
    /// The instruction that results in the first operand of the operation, if an operand is required.
    /// </summary>
    public Instruction/*?*/ Operand1;

    /// <summary>
    /// The instruction that results in the second operand of the operation, if a second operand is required.
    /// Could also be an array of instructions if the instruction is n-ary for n > 2.
    /// </summary>
    public object/*?*/ Operand2;

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append(this.Operation.Offset.ToString("x4"));
      stringBuilder.Append(", ");
      stringBuilder.Append(this.Operation.OperationCode.ToString());

      if (this.Operation.Value is uint)
        stringBuilder.Append(" "+((uint)this.Operation.Value).ToString("x4"));
      stringBuilder.Append(", ");
      stringBuilder.Append(TypeHelper.GetTypeName(this.Type));
      if (this.Operand1 != null) {
        stringBuilder.Append(", ");
        this.AppendFlowFrom(this.Operand1, stringBuilder);
      }
      var i2 = this.Operand2 as Instruction;
      if (i2 != null) {
        stringBuilder.Append(", ");
        this.AppendFlowFrom(i2, stringBuilder);
      } else {
        var i2a = this.Operand2 as Instruction[];
        if (i2a != null) {
          foreach (var i2e in i2a) {
            Contract.Assume(i2e != null); //Assumed because of the informal specification of the ControlFlowInferencer
            stringBuilder.Append(", ");
            this.AppendFlowFrom(i2e, stringBuilder);
          }
        }
      }
      return stringBuilder.ToString();
    }

    private void AppendFlowFrom(Instruction instruction, StringBuilder stringBuilder) {
      Contract.Requires(instruction != null);
      Contract.Requires(stringBuilder != null);

      if (instruction.Operation == Dummy.Operation)
        stringBuilder.Append("stack");
      else
        stringBuilder.Append(instruction.Operation.Offset.ToString("x4"));
    }

    /// <summary>
    /// The type of the result this instruction pushes onto the stack. Void if none.
    /// </summary>
    public ITypeReference Type {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        return type; 
      }
      set {
        Contract.Requires(value != null);
        type = value; 
      }
    }
    private ITypeReference type;

  }

}

namespace Microsoft.Cci.ControlAndDataFlowGraph {
  internal class Stack<Instruction> where Instruction : class {

    internal Stack(int maxStack, List<Instruction> operandStackSetup) {
      Contract.Requires(operandStackSetup != null);
      Contract.Assume(Contract.ForAll(operandStackSetup, x => x != null)); //Making this an explicit requirement creates too many proof obligations.

      if (maxStack <= 0) maxStack = 8;
      this.operandStackSetup = operandStackSetup;
      this.elements = new Instruction[maxStack];
      this.top = -1;
    }

    List<Instruction> operandStackSetup;
    Instruction[] elements;
    private int top;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.operandStackSetup != null);
      Contract.Invariant(this.elements != null);
      Contract.Invariant(this.elements.Length > 0);
      Contract.Invariant(this.top < this.elements.Length);
      Contract.Invariant(Contract.ForAll(0, this.top+1, (i) => this.elements[i] != null));
      Contract.Invariant(this.top >= -1);
    }

    internal void Clear() {
      this.top = -1;
    }

    internal void Push(Instruction instruction) {
      Contract.Requires(instruction != null);

      if (this.top >= this.elements.Length-1) {
        Array.Resize(ref this.elements, this.elements.Length*2);
        Contract.Assume(Contract.ForAll(0, this.top+1, (i) => this.elements[i] != null)); //this the expected behavior of Array.Resize
      }
      this.elements[++this.top] = instruction;
    }

    internal Instruction Peek(int i) {
      Contract.Requires(0 <= i);
      Contract.Requires(i <= this.Top);
      Contract.Ensures(Contract.Result<Instruction>() != null);
      Contract.Ensures(this.Top == Contract.OldValue<int>(this.Top));

      return this.elements[i];
    }

    internal Instruction Pop() {
      Contract.Ensures(Contract.Result<Instruction>() != null);
      
      Contract.Assume(this.top >= 0); //This is an optimistic assumption. Clients have to match their Pop and Push calls, but enforcing this convention via contracts is too verbose.
      return this.elements[this.top--];
    }

    internal int Top {
      get { return this.top; }
    }
  }
}
