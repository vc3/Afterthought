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
  /// A set of basic blocks, each of which has a list of successor blocks and some other information.
  /// Each block consists of a list of instructions, each of which can point to previous instructions that compute the operands it consumes.
  /// </summary>
  public class ControlAndDataFlowGraph<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.Analysis.BasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Analysis.Instruction, new() {

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
    /// Returns the pc for the first block
    /// </summary>
    public BlockPC StartPC { get { return new BlockPC(0u.Singleton()); } }

    /// <summary>
    /// Returns the successor BlockPCs from the given BlockPC, properly taking into account finally blocks
    /// </summary>
    public IEnumerable<BlockPC> Successors(BlockPC pc)
    {
      var current = this.BlockFor[pc.Current];
      var succs = this.SuccessorsFor(current);
      if (succs.Count > 0)
      {
        foreach (var succ in succs)
        {
          var finallyBlocks = this.FinallyBlocksOnEdge(current, succ);
          if (finallyBlocks != null)
          {
            var to = pc.Stack.Tail; // pop current
            to = to.Cons(succ.Offset); // push ultimate successor
            while (finallyBlocks != null)
            {
              to = to.Cons(finallyBlocks.Head.Offset); // push each finally block to execute
              finallyBlocks = finallyBlocks.Tail;
            }
            yield return new BlockPC(to);
          }
          else
          {
            yield return pc.Replace(succ.Offset);
          }
        }
      }
      else
      {
        // no direct successors

        // find the next pending block
        var to = pc.Stack.Tail;
        if (to != null) yield return new BlockPC(to);
      }
    }

    /// <summary>
    /// Return the Block corresponding to the offset on top of the execution stack
    /// </summary>
    public BasicBlock CurrentBlock(BlockPC pc)
    {
      var current = this.BlockFor[pc.Current];
      return current;
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
      HandlerInferencer<BasicBlock, Instruction>.FillInHandlers(host, cdfg);

      return cdfg;
    }

    /// <summary>
    /// Returns the finally blocks on this control flow edge in reverse execution order on a forward traversal.
    /// </summary>
    public FList<BasicBlock> FinallyBlocksOnEdge(BasicBlock from, BasicBlock to)
    {
      Contract.Requires(from != null);
      Contract.Requires(to != null);

      var fromHandlers = from.Handlers;
      var toHandlers = to.Handlers;
      var commonTail = fromHandlers.LongestCommonTail(toHandlers);

      var result = FList<BasicBlock>.Empty;

      while (fromHandlers != commonTail)
      {
        Contract.Assume(fromHandlers != null);

        var handler = fromHandlers.Head;
        if (handler.HandlerKind == HandlerKind.Finally)
        {
          result = result.Cons(this.BlockFor[fromHandlers.Head.HandlerStartOffset]);
        }
        fromHandlers = fromHandlers.Tail;
      }

      return result;
    }

    /// <summary>
    /// Given an instruction representing an address (byref), find the local or parameter it corresponds to or null
    /// </summary>
    public object LocalOrParameter(Analysis.Instruction address)
    {
      if (address == null) return null;
      while (true)
      {
        if (address.IsMergeNode) { address = address.Operand1; } // all merges should be same address
        else
        {
          switch (address.Operation.OperationCode)
          {
            case OperationCode.Ldarg:
            case OperationCode.Ldarg_0:
            case OperationCode.Ldarg_1:
            case OperationCode.Ldarg_2:
            case OperationCode.Ldarg_3:
            case OperationCode.Ldarg_S:
            case OperationCode.Ldarga:
            case OperationCode.Ldarga_S:
              return address.Operation.Value; // the parameter definition

            case OperationCode.Ldloca:
            case OperationCode.Ldloca_S:
              return address.Operation.Value; // the local definition

            default:
              return null;
          }
        }
      }
    }

  }

  /// <summary>
  /// A block of instructions of which only the first instruction can be reached via explicit control flow.
  /// </summary>
  public class BasicBlock<Instruction> where Instruction : Microsoft.Cci.Analysis.Instruction {

    /// <summary>
    /// The first edge that leaves this block. The edges are a contiguous sublist of the the SuccessorEdges list of the ControlAndDataFlowGraph that contains this block.
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
    public Sublist<Instruction> OperandStack;

    /// <summary>
    /// The instructions making up this block.
    /// </summary>
    public Sublist<Instruction> Instructions;

    private uint offset;

    /// <summary>
    /// The IL offset of the first instruction in this basic block. If the block is empty, it is the same as the Offset of the following block. If there is no following block, 
    /// it is the offset where the next instruction would have appeared.
    /// </summary>
    public uint Offset 
    {
        get
        {
            if (this.offset == 0)
            {
                if (this.Instructions.Count == 0) return 0; else return this.Instructions[0].Operation.Offset;
            }
            return this.offset;
        }
        set
        {
            this.offset = value;
        }
    }

    /// <summary>
    /// The enclosing handlers of this block in innermost to outermost order
    /// </summary>
    public FList<IOperationExceptionInformation> Handlers;

    /// <summary>
    /// If this block is physically inside a catch, fault, finally, or filter handler, then 
    /// ContainingHandler points to the closest enclosing such handler.
    /// </summary>
    public IOperationExceptionInformation/*?*/ ContainingHandler;

    private FMap<ILocalDefinition, Microsoft.Cci.Analysis.Instruction> localDefs;

    /// <summary>
    /// Maps local variables at the beginning of the block to the corresponding defining instruction
    /// </summary>
    public FMap<ILocalDefinition, Microsoft.Cci.Analysis.Instruction> LocalDefs
    {
      get
      {
        Contract.Ensures(Contract.Result<FMap<ILocalDefinition, Microsoft.Cci.Analysis.Instruction>>() != null);

        if (this.localDefs == null)
        {
          this.localDefs = new FMap<ILocalDefinition, Microsoft.Cci.Analysis.Instruction>(l => l.GetHashCode());
        }
        return this.localDefs;
      }
      set {
        Contract.Requires(value != null);
        this.localDefs = value;
      }

    }

    private FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction> paramDefs;

    /// <summary>
    /// Maps parameters at the beginning of the block to the corresponding defining instruction
    /// </summary>
    public FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction> ParamDefs
    {
      get
      {
        Contract.Ensures(Contract.Result<FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction>>() != null);
        if (this.paramDefs == null)
        {
          this.paramDefs = new FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction>(l => l.Index);
        }
        return this.paramDefs;
      }
      set
      {
        Contract.Requires(value != null);
        this.paramDefs = value;
      }
    }

    /// <summary>
    /// If this instruction is a constrained call virt, return the corresponding preceeding type constraint.
    /// </summary>
    public ITypeReference CallConstraint(Instruction instruction)
    {
      Contract.Requires(instruction != null);

      if (instruction.Operation.OperationCode != OperationCode.Callvirt) return null;

      Instruction pred = null;
      for (int i = 0; i < this.Instructions.Count; i++)
      {
        if (this.Instructions[i] == instruction) break;
        pred = this.Instructions[i];
      }
      if (pred != null && pred.Operation.OperationCode == OperationCode.Constrained_)
      {
        return pred.Operation.Value as ITypeReference;
      }
      return null;
    }

    /// <summary>
    /// Returns a string describing the basic block.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
      if (this.Instructions.Count == 0) return "Empty BasicBlock";
      return "BasicBlock at "+this.Offset.ToString("x4");
    }

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

      if (instruction.Operation is Dummy)
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
        Contract.Assume(!(value is Dummy)); //It is a bit too onerous on the client code to prove this statically, but it does seem a very desirable check.
        type = value;
      }
    }
    private ITypeReference type;

    /// <summary>
    /// Extra dataflow information for Ldloc, Ldarg, Ldind. It contains the actual result value that was loaded
    /// </summary>
    public Instruction Aux;

    /// <summary>
    /// The local definitions after the instruction
    /// </summary>
    public FMap<ILocalDefinition, Instruction> PostLocalDefs;
    /// <summary>
    /// The parameter definitions after the instruction
    /// </summary>
    public FMap<IParameterDefinition, Instruction> PostParamDefs;

    /// <summary>
    /// Return true if the instruction is a synthetic merge node (aka Phi node) at block entry
    /// </summary>
    public bool IsMergeNode { get { return this.operation is Dummy; } }

    /// <summary>
    /// Returns the defining instruction of the local or parameter definition after this instruction or null
    /// </summary>
    public Instruction this[object localOrParameter]
    {
      get
      {
        Instruction result;
        var local = localOrParameter as ILocalDefinition;
        if (this.PostLocalDefs != null && local != null)
        {
          this.PostLocalDefs.TryGetValue(local, out result);
          return result;
        }
        var param = localOrParameter as IParameterDefinition;
        if (this.PostParamDefs != null && param != null)
        {
          this.PostParamDefs.TryGetValue(param, out result);
          return result;
        }
        return null;
      }
    }

    /// <summary>
    /// If the instruction is a merge node, then return all in-flowing defs
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Instruction> InFlows()
    {
      if (!this.IsMergeNode) yield break;
      if (this.Operand1 != null)
      {
        yield return this.Operand1;
      }
      var second = this.Operand2 as Instruction;
      if (second != null) yield return second;
      var rest = this.Operand2 as List<Instruction>;
      if (rest != null)
      {
        for (int i = 0; i < rest.Count; i++)
        {
          yield return rest[i];
        }
      }
    }

  }

  internal class Stack<Instruction> where Instruction : class {

    internal Stack(int maxStack) {
      if (maxStack <= 0) maxStack = 8;
      this.elements = new Instruction[maxStack];
      this.top = -1;
    }

    Instruction[] elements;
    private int top;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
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

  /// <summary>
  /// A generalized program counter that contains the current block (and blocks to execute after that)
  /// 
  /// Equality and hashing is based on content (the blocks) rather than the list pointers, so they are value equal
  /// </summary>
  public struct BlockPC : IEquatable<BlockPC>
  {
    /// <summary>
    /// List of blocks (identified by start instruction offset) that are form a stack of execution contexts
    /// </summary>
    public readonly FList<uint> Stack;

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(this.Stack != null);
    }

    /// <summary>
    /// Produce a block PC with the given control stack
    /// </summary>
    public BlockPC(FList<uint> stack)
    {
      Contract.Requires(stack != null);

      this.Stack = stack;
    }

    /// <summary>
    /// Return the block offset of the current block in the PC
    /// </summary>
    public uint Current { get { return this.Stack.Head; } }

    /// <summary>
    /// Produce a new BlockPC with the current block replaced by the given one. The rest of the stack is unchanged.
    /// </summary>
    public BlockPC Replace(uint newCurrent)
    {
      return new BlockPC(this.Stack.Tail.Cons(newCurrent));
    }

    /// <summary>
    /// Produce a new BlockPC starting at the given Block.
    /// </summary>
    public static BlockPC For<Block, Instruction>(Block b)
      where Block : Analysis.BasicBlock<Instruction>
      where Instruction : Analysis.Instruction
    {
      return new BlockPC(b.Offset.Singleton());
    }

    /// <summary>
    /// Push the given block on top of the BlockPC call stack
    /// </summary>
    public BlockPC Push<Block, Instruction>(Block b)
      where Block : Analysis.BasicBlock<Instruction>
      where Instruction : Analysis.Instruction
    {
      return new BlockPC(this.Stack.Cons(b.Offset));
    }

    #region IEquatable<BlockPC> Members

    /// <summary>
    /// Compare two BlockPCs for value equality
    /// </summary>
    public bool Equals(BlockPC other)
    {
      return EqualLists(this.Stack, other.Stack);
    }

    private static bool EqualLists(FList<uint> tl, FList<uint> ol)
    {
      while (tl != null && ol != null)
      {
        if (tl.Head != ol.Head) return false;
        tl = tl.Tail;
        ol = ol.Tail;
      }
      if (ol != tl) return false; // both must be null here

      return true;
    }

    /// <summary>
    /// Return the hash code for this BlockPC
    /// </summary>
    public override int GetHashCode()
    {
      return HashList(0, this.Stack);
    }

    private static int HashList(int hash, FList<uint> l)
    {
      while (l != null) { hash = 2 * hash + (int)l.Head; l = l.Tail; }
      return hash;
    }

    /// <summary>
    /// Return a string representation of this BlockPC
    /// </summary>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append("Block ");
      sb.AppendFormat("{0:x3}", this.Stack.Head);
      sb.Append(" (");
      var to = this.Stack.Tail;
      while (to != null)
      {
        sb.AppendFormat("{0:x3}", to.Head);
        sb.Append(", ");
        to = to.Tail;
      }
      sb.Append(")");
      return sb.ToString();
    }
    #endregion
  }

}
