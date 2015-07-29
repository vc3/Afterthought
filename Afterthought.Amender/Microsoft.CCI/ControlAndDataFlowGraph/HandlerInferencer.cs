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
using System;

namespace Microsoft.Cci.Analysis
{
  /// <summary>
  /// Infer protecting local handlers for each block.
  /// 
  /// With that information we can then traverse finally and exception handler blocks in the proper order to infer data flow for locals and parameters
  /// </summary>
  internal class HandlerInferencer<BasicBlock, Instruction>
    where BasicBlock : Microsoft.Cci.Analysis.BasicBlock<Instruction>, new()
    where Instruction : Microsoft.Cci.Analysis.Instruction, new()
  {
    internal static void FillInHandlers(IMetadataHost host, ControlAndDataFlowGraph<BasicBlock, Instruction> cdfg)
    {
      var method = cdfg.MethodBody;

      var handlers = new List<IOperationExceptionInformation>(method.OperationExceptionInformation).ToArray();

      #region Compute enclosing handlers for each block
      FList<IOperationExceptionInformation> currentHandlers = null;
      FList<IOperationExceptionInformation> containingHandlers = null;

      foreach (var current in cdfg.AllBlocks)
      {
        //traceFile.WriteLine("Block at: {0:x3}", current.Offset);
        currentHandlers = PopPushHandlers(current, currentHandlers, handlers, ref containingHandlers);

      }
      #endregion

      // now compute extra block information:
      //  - local def maps
      //  - parameter def maps

      ComputeDataFlowThroughLocals(cdfg);
    }
    private static FList<IOperationExceptionInformation> PopPushHandlers(BasicBlock block, FList<IOperationExceptionInformation> currentHandlers, IOperationExceptionInformation[] handlers, ref FList<IOperationExceptionInformation> containingHandlers)
    {
      Contract.Requires(block != null);
      Contract.Requires(handlers != null);
      Contract.Requires(Contract.ForAll(handlers, h => h != null));

      // pop fault/finally subroutines off subroutine stack whose scope ends here
      var blockOffset = block.Offset;

      #region Pop protecting handlers off stack whose scope ends here
      for (int i = 0; i < handlers.Length; i++)
      {
        if (handlers[i].TryEndOffset == blockOffset)
        {
          // must be head
          if (currentHandlers != null && Object.Equals(handlers[i], currentHandlers.Head))
          {
            currentHandlers = currentHandlers.Tail;
          }
          else
          {
            throw new ApplicationException("bad order of handlers");
          }
        }
      }
      #endregion

      #region Push protecting handlers on stack whose scope starts here

      // reverse order
      for (int i = handlers.Length - 1; i >= 0; i--)
      {
        if (handlers[i].TryStartOffset == blockOffset)
        {
          // push this handler on top of current block enclosing handlers
          currentHandlers = FList<IOperationExceptionInformation>.Cons(handlers[i], currentHandlers); // Push handler
        }
      }
      #endregion

      #region Pop containing handlers off containing handler stack whose scope ends here
      for (int i = 0; i < handlers.Length; i++)
      {
        if (handlers[i].HandlerEndOffset == blockOffset)
        {
          // must be head
          if (containingHandlers != null && Object.Equals(handlers[i], containingHandlers.Head))
          {
            containingHandlers = containingHandlers.Tail;
          }
          else
          {
            throw new ApplicationException("bad order of handlers");
          }
        }
      }
      #endregion

      #region Push containing handler on stack whose scope starts here

      // reverse order
      for (int i = handlers.Length - 1; i >= 0; i--)
      {
        if (handlers[i].HandlerStartOffset == blockOffset)
        {
          // push this handler on top of containing handlers
          containingHandlers = FList<IOperationExceptionInformation>.Cons(handlers[i], containingHandlers); // Push handler
        }
      }
      #endregion


      // record handlers for this block
      block.Handlers = currentHandlers;

      block.ContainingHandler = (containingHandlers != null) ? containingHandlers.Head : null;

      return currentHandlers;
    }

    private static void ComputeDataFlowThroughLocals(ControlAndDataFlowGraph<BasicBlock, Instruction> cdg)
    {
      FMap<ILocalDefinition, Microsoft.Cci.Analysis.Instruction> currentLocals;

      var todo = new Queue<BlockPC>();
      var seen = new HashSet<BlockPC>();
      var startBlock = cdg.RootBlocks[0];

      FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction> currentParameters = new FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction>(k => k.Index);

      var initialLocation = GetStartLocation(startBlock);

      // push parameters onto start block
      foreach (var arg in cdg.MethodBody.MethodDefinition.Parameters)
      {
        var initialOp = new InitialParameterAssignment(arg, initialLocation);
        var initialDef = new Instruction() { Operation = initialOp };
        currentParameters = currentParameters.Insert(arg, initialDef);
      }
      startBlock.ParamDefs = currentParameters;
      todo.Enqueue(new BlockPC(startBlock.Offset.Singleton()));

      while (todo.Count > 0)
      {
        var currentPC = todo.Dequeue();
        if (seen.Contains(currentPC)) continue;
        seen.Add(currentPC);

        var block = cdg.CurrentBlock(currentPC);
        Contract.Assume(block != null);

        currentLocals = block.LocalDefs;
        currentParameters = block.ParamDefs;

        foreach (var instr in block.Instructions)
        {
          if (instr.IsMergeNode) continue;
          switch (instr.Operation.OperationCode)
          {
            case OperationCode.Starg:
            case OperationCode.Starg_S:
              // without pdb we seem to have no parameter info.
              var pdef = (IParameterDefinition)instr.Operation.Value;
              if (pdef != null)
              {
                currentParameters = currentParameters.Insert(pdef, instr.Operand1);
              }
              break;
            case OperationCode.Stloc:
            case OperationCode.Stloc_0:
            case OperationCode.Stloc_1:
            case OperationCode.Stloc_2:
            case OperationCode.Stloc_3:
            case OperationCode.Stloc_S:
              var ldef = (ILocalDefinition)instr.Operation.Value;
              currentLocals = currentLocals.Insert(ldef, instr.Operand1);
              break;

            case OperationCode.Ldloc:
            case OperationCode.Ldloc_0:
            case OperationCode.Ldloc_1:
            case OperationCode.Ldloc_2:
            case OperationCode.Ldloc_3:
            case OperationCode.Ldloc_S:
              // save the source in Aux
              {
                currentLocals.TryGetValue((ILocalDefinition)instr.Operation.Value, out instr.Aux);
                break;
              }
            case OperationCode.Ldarg:
            case OperationCode.Ldarg_0:
            case OperationCode.Ldarg_1:
            case OperationCode.Ldarg_2:
            case OperationCode.Ldarg_3:
            case OperationCode.Ldarg_S:
              // save the source in Aux
              var pdef2 = (IParameterDefinition)instr.Operation.Value;
              if (pdef2 == null)
              {
                // this parameter. Assume it's never overwritten
              }
              else
              {
                currentParameters.TryGetValue(pdef2, out instr.Aux);
              }
              break;

            case OperationCode.Stind_I:
            case OperationCode.Stind_I1:
            case OperationCode.Stind_I2:
            case OperationCode.Stind_I4:
            case OperationCode.Stind_I8:
            case OperationCode.Stind_R4:
            case OperationCode.Stind_R8:
            case OperationCode.Stind_Ref:
            {
              var location = cdg.LocalOrParameter(instr.Operand1);
              UpdateLocation(ref currentLocals, ref currentParameters, location, (Analysis.Instruction)instr.Operand2);

              break;
            }
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
            {
              // save the read value in Aux
              var location = cdg.LocalOrParameter(instr.Operand1);
              instr.Aux = ReadLocation(currentLocals, currentParameters, location);
              break;
            }

            case OperationCode.Call:
            case OperationCode.Callvirt:
            {
              // update byref / out parameters
              var methodRef = instr.Operation.Value as IMethodReference;
              var args = instr.Operand2 as Instruction[];
              if (args != null && methodRef != null)
              {
                foreach (var p in methodRef.Parameters)
                {
                  if (p.IsByReference && p.Index < args.Length)
                  {
                    var arg = args[p.Index];
                    if (arg != null)
                    {
                      var loc = cdg.LocalOrParameter(arg);
                      var syntheticOp = new CallByRefAssignment(instr, p);
                      UpdateLocation(ref currentLocals, ref currentParameters, loc, new Instruction() { Operation = syntheticOp, Type = p.Type });
                    }
                  }
                }
              }
              break;
            }
          }
          instr.PostLocalDefs = currentLocals;
          instr.PostParamDefs = currentParameters;
        }
        foreach (var succ in cdg.Successors(currentPC))
        {
          MergeLocalsAndParameters(cdg.CurrentBlock(succ), currentLocals, currentParameters);
          todo.Enqueue(succ);
        }

      }
    }

    private static Analysis.Instruction ReadLocation(FMap<ILocalDefinition, Analysis.Instruction> currentLocals, FMap<IParameterDefinition, Analysis.Instruction> currentParameters, object localOrParameter)
    {
      Analysis.Instruction result;
      var local = localOrParameter as ILocalDefinition;
      if (local != null)
      {
        currentLocals.TryGetValue(local, out result);
        return result;
      }
      var param = localOrParameter as IParameterDefinition;
      if (param != null)
      {
        currentParameters.TryGetValue(param, out result);
        return result;
      }
      return null;
    }

    private static ILocation GetStartLocation(BasicBlock startBlock)
    {
      foreach (var i in startBlock.Instructions)
      {
        if (i.Operation.Location != null && !(i.Operation.Location is Dummy)) return i.Operation.Location;
      }
      return null;
    }

    private static void UpdateLocation(ref FMap<ILocalDefinition, Analysis.Instruction> currentLocals, ref FMap<IParameterDefinition, Analysis.Instruction> currentParameters, object localOrParameter, Analysis.Instruction newValue)
    {
      Contract.Requires(currentParameters != null);
      Contract.Requires(currentLocals != null);
      Contract.Ensures(Contract.ValueAtReturn(out currentParameters) != null);
      Contract.Ensures(Contract.ValueAtReturn(out currentLocals) != null);

      var local = localOrParameter as ILocalDefinition;
      if (local != null)
      {
        currentLocals = currentLocals.Insert(local, newValue);
        return;
      }
      var param = localOrParameter as IParameterDefinition;
      if (param != null)
      {
        currentParameters = currentParameters.Insert(param, newValue);
        return;
      }
    }


    private static void MergeLocalsAndParameters(BasicBlock succ, FMap<ILocalDefinition, Microsoft.Cci.Analysis.Instruction> currentLocals, FMap<IParameterDefinition, Microsoft.Cci.Analysis.Instruction> currentParameters)
    {
      foreach (var p in currentLocals)
      {
        Microsoft.Cci.Analysis.Instruction existing;
        if (succ.LocalDefs.TryGetValue(p.Key, out existing))
        {
          PushMerge(existing, p.Value);
        }
        else
        {
          var mergeInstruction = new Instruction() { Operand1 = p.Value };
          succ.LocalDefs = succ.LocalDefs.Insert(p.Key, mergeInstruction);
        }
      }
      foreach (var p in currentParameters)
      {
        Microsoft.Cci.Analysis.Instruction existing;
        if (succ.ParamDefs.TryGetValue(p.Key, out existing))
        {
          PushMerge(existing, p.Value);
        }
        else
        {
          var mergeInstruction = new Instruction() { Operand1 = p.Value };
          succ.ParamDefs = succ.ParamDefs.Insert(p.Key, mergeInstruction);
        }
      }
    }

    private static void PushMerge(Microsoft.Cci.Analysis.Instruction target, Microsoft.Cci.Analysis.Instruction inflow)
    {
      if (target.Operand2 == null)
        target.Operand2 = inflow;
      else
      {
        var list = target.Operand2 as List<Microsoft.Cci.Analysis.Instruction>;
        if (list == null)
        {
          //Contract.Assume(target.Operand2 is Instruction);
          list = new List<Microsoft.Cci.Analysis.Instruction>(4);
          list.Add((Microsoft.Cci.Analysis.Instruction)target.Operand2);
        }
        list.Add(inflow);
      }

    }


  }

  /// <summary>
  /// Dummy instruction to give definition of initial parameter values
  /// </summary>
  public class InitialParameterAssignment : IOperation
  {
    private  ILocation location;
    private  IParameterDefinition parameter;

    internal InitialParameterAssignment(IParameterDefinition p, ILocation locs) {
      this.location = locs;
      this.parameter = p;
    }

    #region IOperation Members

    OperationCode IOperation.OperationCode
    {
      get { return Cci.OperationCode.Starg; }
    }

    uint IOperation.Offset
    {
      get { return 0; }
    }

    ILocation IOperation.Location
    {
      get { return this.location; }
    }

    object IOperation.Value
    {
      get { return this.parameter; }
    }

    #endregion
  }

  /// <summary>
  /// Dummy instruction to give definition of byref/out parameter values at a call
  /// </summary>
  public class CallByRefAssignment : IOperation
  {
    Instruction original;

    /// <summary>
    /// The parameter assigned by this operation
    /// </summary>
    public readonly IParameterTypeInformation Parameter;

    internal CallByRefAssignment(Instruction original, IParameterTypeInformation p)
    {
      this.original = original;
      this.Parameter = p;
    }

    /// <summary>
    /// The original call instruction giving rise to this by-ref assignment
    /// </summary>
    public Instruction Call { get { return this.original; } }

    #region IOperation Members

    OperationCode IOperation.OperationCode
    {
      get { return Cci.OperationCode.Starg; }
    }

    uint IOperation.Offset
    {
      get { return original.Operation.Offset; }
    }

    ILocation IOperation.Location
    {
      get { return this.original.Operation.Location; }
    }

    object IOperation.Value
    {
      get { return this.original.Operation.Value; }
    }

    #endregion
  }

}
