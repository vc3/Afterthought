//-----------------------------------------------------------------------------
//
// Copyright (c) VC3, Inc. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace Afterthought.Amender
{
	/// <summary>
	/// Custom IL generator that supports amending in IL instructions into an existing <see cref="MethodBody"/>.
	/// </summary>
	public class ILAmender : ILGenerator
	{
		MethodBody methodBody;
		List<IOperation> operations;
		Dictionary<uint, bool> offsetsUsedInExceptionInformation = new Dictionary<uint, bool>();
		Dictionary<uint, ILGeneratorLabel> offset2Label = new Dictionary<uint, ILGeneratorLabel>();

		public ILAmender(IMetadataHost host, MethodBody methodBody)
			: base(host, methodBody.MethodDefinition)
		{
			this.methodBody = methodBody;
			this.operations = methodBody.Operations ?? new List<IOperation>();

			// Track existing offsets used by exception handlers
			if (methodBody.OperationExceptionInformation != null)
			{
				foreach (var exceptionInfo in methodBody.OperationExceptionInformation)
				{
					uint x = exceptionInfo.TryStartOffset;
					if (!offsetsUsedInExceptionInformation.ContainsKey(x)) offsetsUsedInExceptionInformation.Add(x, true);
					x = exceptionInfo.TryEndOffset;
					if (!offsetsUsedInExceptionInformation.ContainsKey(x)) offsetsUsedInExceptionInformation.Add(x, true);
					x = exceptionInfo.HandlerStartOffset;
					if (!offsetsUsedInExceptionInformation.ContainsKey(x)) offsetsUsedInExceptionInformation.Add(x, true);
					x = exceptionInfo.HandlerEndOffset;
					if (!offsetsUsedInExceptionInformation.ContainsKey(x)) offsetsUsedInExceptionInformation.Add(x, true);
					if (exceptionInfo.HandlerKind == HandlerKind.Filter)
					{
						x = exceptionInfo.FilterDecisionStartOffset;
						if (!offsetsUsedInExceptionInformation.ContainsKey(x)) offsetsUsedInExceptionInformation.Add(x, true);
					}
				}
			}

			// Create and cache labels for each branch statement
			foreach (var op in operations)
			{
				switch (op.OperationCode)
				{
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
					case OperationCode.Br_S:
					case OperationCode.Brfalse_S:
					case OperationCode.Brtrue_S:
					case OperationCode.Leave_S:
						uint x = (uint)op.Value;
						if (!offset2Label.ContainsKey(x))
							offset2Label.Add(x, new ILGeneratorLabel());
						break;
					case OperationCode.Switch:
						uint[] offsets = op.Value as uint[];
						foreach (var offset in offsets)
						{
							if (!offset2Label.ContainsKey(offset))
								offset2Label.Add(offset, new ILGeneratorLabel());
						}
						break;
					default:
						break;
				}
			}

			// Automatically emit the first operation if it is a NOP (debug statement)
			if (operations.Count > 0 && operations[0].OperationCode == OperationCode.Nop)
				EmitOperations(1);
		}

		/// <summary>
		/// Gets the set of remaining operations from the original method body
		/// that have not yet been emitted.
		/// </summary>
		public ICollection<IOperation> Operations
		{
			get
			{
				return operations;
			}
		}

		/// <summary>
		/// Emits the remaining operations except the final return operation.
		/// </summary>
		public void EmitUntilReturn()
		{
			EmitOperations(operations.Count - 1);
		}

		/// <summary>
		/// Emits the specified number of operations from the original method body.
		/// </summary>
		/// <param name="count"></param>
		public void EmitOperations(int count)
		{
			for (int i = 0; i < count; i++)
			{
				IOperation op = operations[0];
				operations.RemoveAt(0);
				ILGeneratorLabel label;
				if (op.Location is IILLocation)
					MarkSequencePoint(op.Location);

				// Mark operation if it is a label for a branch
				if (offset2Label.TryGetValue(op.Offset, out label))
					MarkLabel(label);

				// Mark operation if it is pointed to by an exception handler
				bool ignore;
				uint offset = op.Offset;
				if (offsetsUsedInExceptionInformation.TryGetValue(offset, out ignore))
				{
					foreach (var exceptionInfo in methodBody.OperationExceptionInformation)
					{
						if (offset == exceptionInfo.TryStartOffset)
							BeginTryBody();

						// Never need to do anthing when offset == exceptionInfo.TryEndOffset because
						// we pick up an EndTryBody from the HandlerEndOffset below
						//  EndTryBody();

						if (offset == exceptionInfo.HandlerStartOffset)
						{
							switch (exceptionInfo.HandlerKind)
							{
								case HandlerKind.Catch:
									BeginCatchBlock(exceptionInfo.ExceptionType);
									break;
								case HandlerKind.Fault:
									BeginFaultBlock();
									break;
								case HandlerKind.Filter:
									BeginFilterBody();
									break;
								case HandlerKind.Finally:
									BeginFinallyBlock();
									break;
							}
						}

						if (exceptionInfo.HandlerKind == HandlerKind.Filter && offset == exceptionInfo.FilterDecisionStartOffset)
							BeginFilterBlock();

						if (offset == exceptionInfo.HandlerEndOffset)
							EndTryBody();
					}
				}

				// Emit operation along with any injection
				switch (op.OperationCode)
				{
					// Branches
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
					case OperationCode.Br_S:
					case OperationCode.Brfalse_S:
					case OperationCode.Brtrue_S:
					case OperationCode.Leave_S:
						Emit(ILGenerator.LongVersionOf(op.OperationCode), offset2Label[(uint)op.Value]);
						break;
					case OperationCode.Switch:
						uint[] offsets = op.Value as uint[];
						ILGeneratorLabel[] labels = new ILGeneratorLabel[offsets.Length];
						for (int j = 0, n = offsets.Length; j < n; j++)
						{
							labels[j] = offset2Label[offsets[j]];
						}
						Emit(OperationCode.Switch, labels);
						break;

					// Everything else
					default:
						if (op.Value == null)
						{
							Emit(op.OperationCode);
							break;
						}
						var typeCode = System.Convert.GetTypeCode(op.Value);
						switch (typeCode)
						{
							case TypeCode.Byte:
								Emit(op.OperationCode, (byte)op.Value);
								break;
							case TypeCode.Double:
								Emit(op.OperationCode, (double)op.Value);
								break;
							case TypeCode.Int16:
								Emit(op.OperationCode, (short)op.Value);
								break;
							case TypeCode.Int32:
								Emit(op.OperationCode, (int)op.Value);
								break;
							case TypeCode.Int64:
								Emit(op.OperationCode, (long)op.Value);
								break;
							case TypeCode.Object:
								IFieldReference fieldReference = op.Value as IFieldReference;
								if (fieldReference != null)
								{
									Emit(op.OperationCode, fieldReference);
									break;
								}
								ILocalDefinition localDefinition = op.Value as ILocalDefinition;
								if (localDefinition != null)
								{
									Emit(op.OperationCode, localDefinition);
									break;
								}
								IMethodReference methodReference = op.Value as IMethodReference;
								if (methodReference != null)
								{
									Emit(op.OperationCode, methodReference);
									break;
								}
								IParameterDefinition parameterDefinition = op.Value as IParameterDefinition;
								if (parameterDefinition != null)
								{
									Emit(op.OperationCode, parameterDefinition);
									break;
								}
								ISignature signature = op.Value as ISignature;
								if (signature != null)
								{
									Emit(op.OperationCode, signature);
									break;
								}
								ITypeReference typeReference = op.Value as ITypeReference;
								if (typeReference != null)
								{
									Emit(op.OperationCode, typeReference);
									break;
								}
								throw new Exception("Should never get here: no other IOperation argument types should exist");
							case TypeCode.SByte:
								Emit(op.OperationCode, (sbyte)op.Value);
								break;
							case TypeCode.Single:
								Emit(op.OperationCode, (float)op.Value);
								break;
							case TypeCode.String:
								Emit(op.OperationCode, (string)op.Value);
								break;
							default:
								throw new Exception("Should never get here: no other IOperation argument types should exist");
						}
						break;
				}
			}
		}

		public void UpdateMethodBody(ushort minStack)
		{
			// Emit remaining operations
			EmitOperations(operations.Count);
			while (InTryBody)
				EndTryBody();
			AdjustBranchSizesToBestFit();
			methodBody.OperationExceptionInformation = new List<IOperationExceptionInformation>(GetOperationExceptionInformation());
			methodBody.Operations = new List<IOperation>(GetOperations());
			methodBody.MaxStack = (ushort)(Math.Max(minStack, (ushort)methodBody.MaxStack) + 1);
		}
	}
}
