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
using Microsoft.Cci.MetadataReader.Errors;
using Microsoft.Cci.MetadataReader.PEFile;
using Microsoft.Cci.MetadataReader.PEFileFlags;
using Microsoft.Cci.UtilityDataStructures;
using Microsoft.Cci.Immutable;

namespace Microsoft.Cci.MetadataReader.MethodBody {
  using Microsoft.Cci.MetadataReader.ObjectModelImplementation;

  internal sealed class MethodBody : IMethodBody {
    internal readonly MethodDefinition MethodDefinition;
    internal ILocalDefinition[]/*?*/ LocalVariables;
    IEnumerable<IOperation>/*?*/ cilInstructions;
    IEnumerable<IOperationExceptionInformation>/*?*/ cilExceptionInformation;
    internal readonly bool IsLocalsInited;
    internal readonly ushort StackSize;
    readonly uint bodySize;

    internal MethodBody(MethodDefinition methodDefinition, bool isLocalsInited, ushort stackSize, uint bodySize) {
      this.MethodDefinition = methodDefinition;
      this.IsLocalsInited = isLocalsInited;
      this.LocalVariables = null;
      this.StackSize = stackSize;
      this.bodySize = bodySize;
    }

    internal void SetLocalVariables(ILocalDefinition[] localVariables) {
      this.LocalVariables = localVariables;
    }

    internal void SetCilInstructions(IOperation[] cilInstructions) {
      this.cilInstructions = IteratorHelper.GetReadonly(cilInstructions);
    }

    internal void SetExceptionInformation(IOperationExceptionInformation[] cilExceptionInformation) {
      this.cilExceptionInformation = IteratorHelper.GetReadonly(cilExceptionInformation);
    }

    #region IMethodBody Members

    IMethodDefinition IMethodBody.MethodDefinition {
      get { return this.MethodDefinition; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    IEnumerable<ILocalDefinition> IMethodBody.LocalVariables {
      get {
        if (this.LocalVariables == null) return Enumerable<ILocalDefinition>.Empty;
        return IteratorHelper.GetReadonly(this.LocalVariables);
      }
    }

    bool IMethodBody.LocalsAreZeroed {
      get { return this.IsLocalsInited; }
    }

    public IEnumerable<IOperation> Operations {
      get {
        if (this.cilInstructions == null) return Enumerable<IOperation>.Empty;
        return this.cilInstructions;
      }
    }

    public IEnumerable<ITypeDefinition> PrivateHelperTypes {
      get { return Enumerable<ITypeDefinition>.Empty; }
    }

    public ushort MaxStack {
      get { return this.StackSize; }
    }

    public IEnumerable<IOperationExceptionInformation> OperationExceptionInformation {
      get {
        if (this.cilExceptionInformation == null) return Enumerable<IOperationExceptionInformation>.Empty;
        return this.cilExceptionInformation;
      }
    }

    public uint Size {
      get {
        return this.bodySize;
      }
    }

    #endregion

  }

  internal sealed class LocalVariableDefinition : ILocalDefinition {

    internal LocalVariableDefinition(MethodBody methodBody, IEnumerable<ICustomModifier>/*?*/ customModifiers, bool isPinned,
      bool isReference, uint index, ITypeReference typeReference) {
      this.methodBody = methodBody;
      this.customModifiers = customModifiers;
      this.isPinned = isPinned;
      this.isReference = isReference;
      this.index = index;
      this.typeReference = typeReference;
    }

    readonly MethodBody methodBody;
    readonly IEnumerable<ICustomModifier>/*?*/ customModifiers;
    readonly bool isPinned;
    readonly bool isReference;
    readonly uint index;
    readonly ITypeReference typeReference;

    public override string ToString() {
      return this.Name.Value;
    }

    #region ILocalDefinition Members

    IMetadataConstant ILocalDefinition.CompileTimeValue {
      get { return Dummy.Constant; }
    }

    IEnumerable<ICustomModifier> ILocalDefinition.CustomModifiers {
      get {
        if (this.customModifiers == null) return Enumerable<ICustomModifier>.Empty;
        return this.customModifiers;
      }
    }

    bool ILocalDefinition.IsConstant {
      get { return false; }
    }

    bool ILocalDefinition.IsModified {
      get { return this.customModifiers != null; }
    }

    bool ILocalDefinition.IsPinned {
      get { return this.isPinned; }
    }

    bool ILocalDefinition.IsReference {
      get { return this.isReference; }
    }

    public IEnumerable<ILocation> Locations {
      get {
        MethodBodyLocation mbLoc = new MethodBodyLocation(new MethodBodyDocument(this.methodBody.MethodDefinition), this.index);
        return IteratorHelper.GetSingletonEnumerable<ILocation>(mbLoc);
      }
    }

    public IMethodDefinition MethodDefinition {
      get { return this.methodBody.MethodDefinition; }
    }

    public ITypeReference Type {
      get {
        return this.typeReference;
      }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get {
        if (this.name == null)
          this.name = this.methodBody.MethodDefinition.PEFileToObjectModel.NameTable.GetNameFor("local_" + this.index);
        return this.name;
      }
    }
    IName/*?*/ name;

    #endregion

  }

  internal sealed class CilInstruction : IOperation, IILLocation {
    internal readonly OperationCode CilOpCode;
    MethodBodyDocument document;
    uint offset;
    internal readonly object/*?*/ Value;

    internal CilInstruction(OperationCode cilOpCode, MethodBodyDocument document, uint offset, object/*?*/ value) {
      this.CilOpCode = cilOpCode;
      this.document = document;
      this.offset = offset;
      this.Value = value;
    }

    #region ICilInstruction Members

    public OperationCode OperationCode {
      get { return this.CilOpCode; }
    }

    uint IOperation.Offset {
      get { return this.offset; }
    }

    ILocation IOperation.Location {
      get { return this; }
    }

    object/*?*/ IOperation.Value {
      get { return this.Value; }
    }

    #endregion

    #region IILLocation Members

    public IMethodDefinition MethodDefinition {
      get { return this.document.method; }
    }

    uint IILLocation.Offset {
      get { return this.offset; }
    }

    #endregion

    #region ILocation Members

    public IDocument Document {
      get { return this.document; }
    }

    #endregion
  }

  internal sealed class CilExceptionInformation : IOperationExceptionInformation {
    internal readonly HandlerKind HandlerKind;
    internal readonly ITypeReference ExceptionType;
    internal readonly uint TryStartOffset;
    internal readonly uint TryEndOffset;
    internal readonly uint FilterDecisionStartOffset;
    internal readonly uint HandlerStartOffset;
    internal readonly uint HandlerEndOffset;

    internal CilExceptionInformation(
      HandlerKind handlerKind,
      ITypeReference exceptionType,
      uint tryStartOffset,
      uint tryEndOffset,
      uint filterDecisionStartOffset,
      uint handlerStartOffset,
      uint handlerEndOffset
    ) {
      this.HandlerKind = handlerKind;
      this.ExceptionType = exceptionType;
      this.TryStartOffset = tryStartOffset;
      this.TryEndOffset = tryEndOffset;
      this.FilterDecisionStartOffset = filterDecisionStartOffset;
      this.HandlerStartOffset = handlerStartOffset;
      this.HandlerEndOffset = handlerEndOffset;
    }

    #region IOperationExceptionInformation Members

    HandlerKind IOperationExceptionInformation.HandlerKind {
      get { return this.HandlerKind; }
    }

    ITypeReference IOperationExceptionInformation.ExceptionType {
      get { return this.ExceptionType; }
    }

    uint IOperationExceptionInformation.TryStartOffset {
      get { return this.TryStartOffset; }
    }

    uint IOperationExceptionInformation.TryEndOffset {
      get { return this.TryEndOffset; }
    }

    uint IOperationExceptionInformation.FilterDecisionStartOffset {
      get { return this.FilterDecisionStartOffset; }
    }

    uint IOperationExceptionInformation.HandlerStartOffset {
      get { return this.HandlerStartOffset; }
    }

    uint IOperationExceptionInformation.HandlerEndOffset {
      get { return this.HandlerEndOffset; }
    }

    #endregion
  }

  internal sealed class LocalVariableSignatureConverter : SignatureConverter {
    internal readonly ILocalDefinition[] LocalVariables;
    readonly MethodBody OwningMethodBody;

    LocalVariableDefinition GetLocalVariable(uint index) {
      bool isPinned = false;
      bool isByReferenece = false;
      IEnumerable<ICustomModifier>/*?*/ customModifiers = null;
      byte currByte = this.SignatureMemoryReader.PeekByte(0);
      ITypeReference/*?*/ typeReference;
      if (currByte == ElementType.TypedReference) {
        this.SignatureMemoryReader.SkipBytes(1);
        typeReference = this.PEFileToObjectModel.PlatformType.SystemTypedReference;
      } else {
        customModifiers = this.GetCustomModifiers(out isPinned);
        currByte = this.SignatureMemoryReader.PeekByte(0);
        if (currByte == ElementType.ByReference) {
          this.SignatureMemoryReader.SkipBytes(1);
          isByReferenece = true;
        }
        typeReference = this.GetTypeReference();
      }
      if (typeReference == null) typeReference = Dummy.TypeReference;
      return new LocalVariableDefinition(this.OwningMethodBody, customModifiers, isPinned, isByReferenece, index, typeReference);
    }

    internal LocalVariableSignatureConverter(
      PEFileToObjectModel peFileToObjectModel,
      MethodBody owningMethodBody,
      MemoryReader signatureMemoryReader
    )
      : base(peFileToObjectModel, signatureMemoryReader, owningMethodBody.MethodDefinition) {
      this.OwningMethodBody = owningMethodBody;
      byte firstByte = this.SignatureMemoryReader.ReadByte();
      if (!SignatureHeader.IsLocalVarSignature(firstByte)) {
        //  MDError
      }
      int locVarCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      LocalVariableDefinition[] locVarArr = new LocalVariableDefinition[locVarCount];
      for (int i = 0; i < locVarCount; ++i) {
        locVarArr[i] = this.GetLocalVariable((uint)i);
      }
      this.LocalVariables = locVarArr;
    }
  }

  internal sealed class StandAloneMethodSignatureConverter : SignatureConverter {
    internal readonly byte FirstByte;
    internal readonly IEnumerable<ICustomModifier>/*?*/ ReturnCustomModifiers;
    internal readonly ITypeReference/*?*/ ReturnTypeReference;
    internal readonly bool IsReturnByReference;
    internal readonly IEnumerable<IParameterTypeInformation> RequiredParameters;
    internal readonly IEnumerable<IParameterTypeInformation> VarArgParameters;

    internal StandAloneMethodSignatureConverter(PEFileToObjectModel peFileToObjectModel, MethodDefinition moduleMethodDef, MemoryReader signatureMemoryReader)
      : base(peFileToObjectModel, signatureMemoryReader, moduleMethodDef) {
      this.RequiredParameters = Enumerable<IParameterTypeInformation>.Empty;
      this.VarArgParameters = Enumerable<IParameterTypeInformation>.Empty;
      //  TODO: Check minimum required size of the signature...
      this.FirstByte = this.SignatureMemoryReader.ReadByte();
      int paramCount = this.SignatureMemoryReader.ReadCompressedUInt32();
      bool dummyPinned;
      this.ReturnCustomModifiers = this.GetCustomModifiers(out dummyPinned);
      byte retByte = this.SignatureMemoryReader.PeekByte(0);
      if (retByte == ElementType.Void) {
        this.ReturnTypeReference = peFileToObjectModel.PlatformType.SystemVoid;
        this.SignatureMemoryReader.SkipBytes(1);
      } else if (retByte == ElementType.TypedReference) {
        this.ReturnTypeReference = peFileToObjectModel.PlatformType.SystemTypedReference;
        this.SignatureMemoryReader.SkipBytes(1);
      } else {
        if (retByte == ElementType.ByReference) {
          this.IsReturnByReference = true;
          this.SignatureMemoryReader.SkipBytes(1);
        }
        this.ReturnTypeReference = this.GetTypeReference();
      }
      if (paramCount > 0) {
        IParameterTypeInformation[] reqModuleParamArr = this.GetModuleParameterTypeInformations(Dummy.Signature, paramCount);
        if (reqModuleParamArr.Length > 0) this.RequiredParameters = IteratorHelper.GetReadonly(reqModuleParamArr);
        IParameterTypeInformation[] varArgModuleParamArr = this.GetModuleParameterTypeInformations(Dummy.Signature, paramCount - reqModuleParamArr.Length);
        if (varArgModuleParamArr.Length > 0) this.VarArgParameters = IteratorHelper.GetReadonly(varArgModuleParamArr);
      }
    }
  }

  internal sealed class ILReader {
    internal static readonly EnumerableArrayWrapper<LocalVariableDefinition, ILocalDefinition> EmptyLocalVariables = new EnumerableArrayWrapper<LocalVariableDefinition, ILocalDefinition>(new LocalVariableDefinition[0], Dummy.LocalVariable);
    static readonly HandlerKind[] HandlerKindMap =
      new HandlerKind[] {
        HandlerKind.Catch,  //  0
        HandlerKind.Filter, //  1
        HandlerKind.Finally,  //  2
        HandlerKind.Illegal,  //  3
        HandlerKind.Fault,  //  4
        HandlerKind.Illegal,  //  5+
      };
    internal readonly PEFileToObjectModel PEFileToObjectModel;
    internal readonly MethodDefinition MethodDefinition;
    internal readonly MethodBody MethodBody;
    readonly MethodIL MethodIL;
    internal readonly uint EndOfMethodOffset;

    internal ILReader(
      MethodDefinition methodDefinition,
      MethodIL methodIL
    ) {
      this.MethodDefinition = methodDefinition;
      this.PEFileToObjectModel = methodDefinition.PEFileToObjectModel;
      this.MethodIL = methodIL;
      this.EndOfMethodOffset = (uint)methodIL.EncodedILMemoryBlock.Length;
      this.MethodBody = new MethodBody(methodDefinition, methodIL.LocalVariablesInited, methodIL.MaxStack, this.EndOfMethodOffset);
    }

    bool LoadLocalSignature() {
      uint locVarRID = this.MethodIL.LocalSignatureToken & TokenTypeIds.RIDMask;
      if (locVarRID != 0x00000000) {
        StandAloneSigRow sigRow = this.PEFileToObjectModel.PEFileReader.StandAloneSigTable[locVarRID];
        //  TODO: error checking offset in range
        MemoryBlock signatureMemoryBlock = this.PEFileToObjectModel.PEFileReader.BlobStream.GetMemoryBlockAt(sigRow.Signature);
        //  TODO: Error checking enough space in signature memoryBlock.
        MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
        //  TODO: Check if this is really local var signature there.
        LocalVariableSignatureConverter locVarSigConv = new LocalVariableSignatureConverter(this.PEFileToObjectModel, this.MethodBody, memoryReader);
        this.MethodBody.SetLocalVariables(locVarSigConv.LocalVariables);
      }
      return true;
    }

    string GetUserStringForToken(
      uint token
    ) {
      if ((token & TokenTypeIds.TokenTypeMask) != TokenTypeIds.String) {
        //  Error...
        return string.Empty;
      }
      return this.PEFileToObjectModel.PEFileReader.UserStringStream[token & TokenTypeIds.RIDMask];
    }

    FunctionPointerType/*?*/ GetStandAloneMethodSignature(uint standAloneMethodToken) {
      StandAloneSigRow sigRow = this.PEFileToObjectModel.PEFileReader.StandAloneSigTable[standAloneMethodToken & TokenTypeIds.RIDMask];
      uint signatureBlobOffset = sigRow.Signature;
      //  TODO: error checking offset in range
      MemoryBlock signatureMemoryBlock = this.PEFileToObjectModel.PEFileReader.BlobStream.GetMemoryBlockAt(signatureBlobOffset);
      //  TODO: Error checking enough space in signature memoryBlock.
      MemoryReader memoryReader = new MemoryReader(signatureMemoryBlock);
      //  TODO: Check if this is really field signature there.
      StandAloneMethodSignatureConverter standAloneSigConv = new StandAloneMethodSignatureConverter(this.PEFileToObjectModel, this.MethodDefinition, memoryReader);
      if (standAloneSigConv.ReturnTypeReference == null) return null;
      return new FunctionPointerType((CallingConvention)standAloneSigConv.FirstByte, standAloneSigConv.IsReturnByReference, standAloneSigConv.ReturnTypeReference,
        standAloneSigConv.ReturnCustomModifiers, standAloneSigConv.RequiredParameters, standAloneSigConv.VarArgParameters, this.PEFileToObjectModel.InternFactory);
    }

    IParameterDefinition/*?*/ GetParameter(uint rawParamNum) {
      if (!this.MethodDefinition.IsStatic) {
        if (rawParamNum == 0) return null; //this
        rawParamNum--;
      }
      IParameterDefinition[] mpa = this.MethodDefinition.RequiredModuleParameters;
      if (mpa != null && rawParamNum < mpa.Length) return mpa[rawParamNum];
      //  Error...
      return Dummy.ParameterDefinition;
    }

    ILocalDefinition GetLocal(
      uint rawLocNum
    ) {
      var locVarDef = this.MethodBody.LocalVariables;
      if (locVarDef != null && rawLocNum < locVarDef.Length)
        return locVarDef[rawLocNum];
      //  Error...
      return Dummy.LocalVariable;
    }

    IMethodReference GetMethod(
      uint methodToken
    ) {
      IMethodReference mmr = this.PEFileToObjectModel.GetMethodReferenceForToken(this.MethodDefinition, methodToken);
      return mmr;
    }

    IFieldReference GetField(
      uint fieldToken
    ) {
      IFieldReference mfr = this.PEFileToObjectModel.GetFieldReferenceForToken(this.MethodDefinition, fieldToken);
      return mfr;
    }

    ITypeReference GetType(
      uint typeToken
    ) {
      ITypeReference/*?*/ mtr = this.PEFileToObjectModel.GetTypeReferenceForToken(this.MethodDefinition, typeToken);
      if (mtr != null)
        return mtr;
      //  Error...
      return Dummy.TypeReference;
    }

    IFunctionPointerTypeReference GetFunctionPointerType(
      uint standAloneSigToken
    ) {
      FunctionPointerType/*?*/ fpt = this.GetStandAloneMethodSignature(standAloneSigToken);
      if (fpt != null)
        return fpt;
      //  Error...
      return Dummy.FunctionPointer;
    }

    object/*?*/ GetRuntimeHandleFromToken(
      uint token
    ) {
      return this.PEFileToObjectModel.GetReferenceForToken(this.MethodDefinition, token);
    }

    bool PopulateCilInstructions() {
      MethodBodyDocument document = new MethodBodyDocument(this.MethodDefinition);
      MemoryReader memReader = new MemoryReader(this.MethodIL.EncodedILMemoryBlock);
      var numInstructions = CountCilInstructions(memReader);
      if (numInstructions == 0) return true;
      CilInstruction[] instrList = new CilInstruction[numInstructions];
      int instructionNumber = 0;
      while (memReader.NotEndOfBytes) {
        object/*?*/ value = null;
        uint offset = (uint)memReader.Offset;
        OperationCode cilOpCode = memReader.ReadOpcode();
        switch (cilOpCode) {
          case OperationCode.Nop:
          case OperationCode.Break:
            break;
          case OperationCode.Ldarg_0:
          case OperationCode.Ldarg_1:
          case OperationCode.Ldarg_2:
          case OperationCode.Ldarg_3:
            value = this.GetParameter((uint)(cilOpCode - OperationCode.Ldarg_0));
            break;
          case OperationCode.Ldloc_0:
          case OperationCode.Ldloc_1:
          case OperationCode.Ldloc_2:
          case OperationCode.Ldloc_3:
            value = this.GetLocal((uint)(cilOpCode - OperationCode.Ldloc_0));
            break;
          case OperationCode.Stloc_0:
          case OperationCode.Stloc_1:
          case OperationCode.Stloc_2:
          case OperationCode.Stloc_3:
            value = this.GetLocal((uint)(cilOpCode - OperationCode.Stloc_0));
            break;
          case OperationCode.Ldarg_S:
          case OperationCode.Ldarga_S:
          case OperationCode.Starg_S:
            value = this.GetParameter(memReader.ReadByte());
            break;
          case OperationCode.Ldloc_S:
          case OperationCode.Ldloca_S:
          case OperationCode.Stloc_S:
            value = this.GetLocal(memReader.ReadByte());
            break;
          case OperationCode.Ldnull:
            break;
          case OperationCode.Ldc_I4_M1:
            value = -1;
            break;
          case OperationCode.Ldc_I4_0:
            value = 0;
            break;
          case OperationCode.Ldc_I4_1:
            value = 1;
            break;
          case OperationCode.Ldc_I4_2:
            value = 2;
            break;
          case OperationCode.Ldc_I4_3:
            value = 3;
            break;
          case OperationCode.Ldc_I4_4:
            value = 4;
            break;
          case OperationCode.Ldc_I4_5:
            value = 5;
            break;
          case OperationCode.Ldc_I4_6:
            value = 6;
            break;
          case OperationCode.Ldc_I4_7:
            value = 7;
            break;
          case OperationCode.Ldc_I4_8:
            value = 8;
            break;
          case OperationCode.Ldc_I4_S:
            value = (int)memReader.ReadSByte();
            break;
          case OperationCode.Ldc_I4:
            value = memReader.ReadInt32();
            break;
          case OperationCode.Ldc_I8:
            value = memReader.ReadInt64();
            break;
          case OperationCode.Ldc_R4:
            value = memReader.ReadSingle();
            break;
          case OperationCode.Ldc_R8:
            value = memReader.ReadDouble();
            break;
          case OperationCode.Dup:
          case OperationCode.Pop:
            break;
          case OperationCode.Jmp:
            value = this.GetMethod(memReader.ReadUInt32());
            break;
          case OperationCode.Call: {
              IMethodReference methodReference = this.GetMethod(memReader.ReadUInt32());
              IArrayTypeReference/*?*/ arrayType = methodReference.ContainingType as IArrayTypeReference;
              if (arrayType != null) {
                // For Get(), Set() and Address() on arrays, the runtime provides method implementations.
                // Hence, CCI2 replaces these with pseudo instructions Array_Set, Array_Get and Array_Addr.
                // All other methods on arrays will not use pseudo instruction and will have methodReference as their operand. 
                if (methodReference.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Set.UniqueKey) {
                  cilOpCode = OperationCode.Array_Set;
                  value = arrayType;
                } else if (methodReference.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Get.UniqueKey) {
                  cilOpCode = OperationCode.Array_Get;
                  value = arrayType;
                } else if (methodReference.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Address.UniqueKey) {
                  cilOpCode = OperationCode.Array_Addr;
                  value = arrayType;
                } else {
                  value = methodReference;
                }
              } else {
                value = methodReference;
              }
            }
            break;
          case OperationCode.Calli:
            value = this.GetFunctionPointerType(memReader.ReadUInt32());
            break;
          case OperationCode.Ret:
            break;
          case OperationCode.Br_S:
          case OperationCode.Brfalse_S:
          case OperationCode.Brtrue_S:
          case OperationCode.Beq_S:
          case OperationCode.Bge_S:
          case OperationCode.Bgt_S:
          case OperationCode.Ble_S:
          case OperationCode.Blt_S:
          case OperationCode.Bne_Un_S:
          case OperationCode.Bge_Un_S:
          case OperationCode.Bgt_Un_S:
          case OperationCode.Ble_Un_S:
          case OperationCode.Blt_Un_S: {
              uint jumpOffset = (uint)(memReader.Offset + 1 + memReader.ReadSByte());
              if (jumpOffset >= this.EndOfMethodOffset) {
                //  Error...
              }
              value = jumpOffset;
            }
            break;
          case OperationCode.Br:
          case OperationCode.Brfalse:
          case OperationCode.Brtrue:
          case OperationCode.Beq:
          case OperationCode.Bge:
          case OperationCode.Bgt:
          case OperationCode.Ble:
          case OperationCode.Blt:
          case OperationCode.Bne_Un:
          case OperationCode.Bge_Un:
          case OperationCode.Bgt_Un:
          case OperationCode.Ble_Un:
          case OperationCode.Blt_Un: {
              uint jumpOffset = (uint)(memReader.Offset + 4 + memReader.ReadInt32());
              if (jumpOffset >= this.EndOfMethodOffset) {
                //  Error...
              }
              value = jumpOffset;
            }
            break;
          case OperationCode.Switch: {
              uint numTargets = memReader.ReadUInt32();
              uint[] result = new uint[numTargets];
              uint asOffset = memReader.Offset + numTargets * 4;
              for (int i = 0; i < numTargets; i++) {
                uint targetAddress = memReader.ReadUInt32() + asOffset;
                if (targetAddress >= this.EndOfMethodOffset) {
                  //  Error...
                }
                result[i] = targetAddress;
              }
              value = result;
            }
            break;
          case OperationCode.Ldind_I1:
          case OperationCode.Ldind_U1:
          case OperationCode.Ldind_I2:
          case OperationCode.Ldind_U2:
          case OperationCode.Ldind_I4:
          case OperationCode.Ldind_U4:
          case OperationCode.Ldind_I8:
          case OperationCode.Ldind_I:
          case OperationCode.Ldind_R4:
          case OperationCode.Ldind_R8:
          case OperationCode.Ldind_Ref:
          case OperationCode.Stind_Ref:
          case OperationCode.Stind_I1:
          case OperationCode.Stind_I2:
          case OperationCode.Stind_I4:
          case OperationCode.Stind_I8:
          case OperationCode.Stind_R4:
          case OperationCode.Stind_R8:
          case OperationCode.Add:
          case OperationCode.Sub:
          case OperationCode.Mul:
          case OperationCode.Div:
          case OperationCode.Div_Un:
          case OperationCode.Rem:
          case OperationCode.Rem_Un:
          case OperationCode.And:
          case OperationCode.Or:
          case OperationCode.Xor:
          case OperationCode.Shl:
          case OperationCode.Shr:
          case OperationCode.Shr_Un:
          case OperationCode.Neg:
          case OperationCode.Not:
          case OperationCode.Conv_I1:
          case OperationCode.Conv_I2:
          case OperationCode.Conv_I4:
          case OperationCode.Conv_I8:
          case OperationCode.Conv_R4:
          case OperationCode.Conv_R8:
          case OperationCode.Conv_U4:
          case OperationCode.Conv_U8:
            break;
          case OperationCode.Callvirt: {
              IMethodReference methodReference = this.GetMethod(memReader.ReadUInt32());
              IArrayTypeReference/*?*/ arrayType = methodReference.ContainingType as IArrayTypeReference;
              if (arrayType != null) {
                // For Get(), Set() and Address() on arrays, the runtime provides method implementations.
                // Hence, CCI2 replaces these with pseudo instructions Array_Set, Array_Get and Array_Addr.
                // All other methods on arrays will not use pseudo instruction and will have methodReference as their operand. 
                if (methodReference.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Set.UniqueKey) {
                  cilOpCode = OperationCode.Array_Set;
                  value = arrayType;
                } else if (methodReference.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Get.UniqueKey) {
                  cilOpCode = OperationCode.Array_Get;
                  value = arrayType;
                } else if (methodReference.Name.UniqueKey == this.PEFileToObjectModel.NameTable.Address.UniqueKey) {
                  cilOpCode = OperationCode.Array_Addr;
                  value = arrayType;
                } else {
                  value = methodReference;
                }
              } else {
                value = methodReference;
              }
            }
            break;
          case OperationCode.Cpobj:
          case OperationCode.Ldobj:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Ldstr:
            value = this.GetUserStringForToken(memReader.ReadUInt32());
            break;
          case OperationCode.Newobj: {
              IMethodReference methodReference = this.GetMethod(memReader.ReadUInt32());
              IArrayTypeReference/*?*/ arrayType = methodReference.ContainingType as IArrayTypeReference;
              if (arrayType != null && !arrayType.IsVector) {
                uint numParam = IteratorHelper.EnumerableCount(methodReference.Parameters);
                if (numParam != arrayType.Rank)
                  cilOpCode = OperationCode.Array_Create_WithLowerBound;
                else
                  cilOpCode = OperationCode.Array_Create;
                value = arrayType;
              } else {
                value = methodReference;
              }
            }
            break;
          case OperationCode.Castclass:
          case OperationCode.Isinst:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Conv_R_Un:
            break;
          case OperationCode.Unbox:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Throw:
            break;
          case OperationCode.Ldfld:
          case OperationCode.Ldflda:
          case OperationCode.Stfld:
            value = this.GetField(memReader.ReadUInt32());
            break;
          case OperationCode.Ldsfld:
          case OperationCode.Ldsflda:
          case OperationCode.Stsfld:
            value = this.GetField(memReader.ReadUInt32());
            var fieldRef = value as FieldReference;
            if (fieldRef != null) fieldRef.isStatic = true;
            break;
          case OperationCode.Stobj:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Conv_Ovf_I1_Un:
          case OperationCode.Conv_Ovf_I2_Un:
          case OperationCode.Conv_Ovf_I4_Un:
          case OperationCode.Conv_Ovf_I8_Un:
          case OperationCode.Conv_Ovf_U1_Un:
          case OperationCode.Conv_Ovf_U2_Un:
          case OperationCode.Conv_Ovf_U4_Un:
          case OperationCode.Conv_Ovf_U8_Un:
          case OperationCode.Conv_Ovf_I_Un:
          case OperationCode.Conv_Ovf_U_Un:
            break;
          case OperationCode.Box:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Newarr: {
              var elementType = this.GetType(memReader.ReadUInt32());
              if (elementType != null)
                value = Vector.GetVector(elementType, PEFileToObjectModel.InternFactory);
              else
                value = Dummy.ArrayType;
            }
            break;
          case OperationCode.Ldlen:
            break;
          case OperationCode.Ldelema:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Ldelem_I1:
          case OperationCode.Ldelem_U1:
          case OperationCode.Ldelem_I2:
          case OperationCode.Ldelem_U2:
          case OperationCode.Ldelem_I4:
          case OperationCode.Ldelem_U4:
          case OperationCode.Ldelem_I8:
          case OperationCode.Ldelem_I:
          case OperationCode.Ldelem_R4:
          case OperationCode.Ldelem_R8:
          case OperationCode.Ldelem_Ref:
          case OperationCode.Stelem_I:
          case OperationCode.Stelem_I1:
          case OperationCode.Stelem_I2:
          case OperationCode.Stelem_I4:
          case OperationCode.Stelem_I8:
          case OperationCode.Stelem_R4:
          case OperationCode.Stelem_R8:
          case OperationCode.Stelem_Ref:
            break;
          case OperationCode.Ldelem:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Stelem:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Unbox_Any:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Conv_Ovf_I1:
          case OperationCode.Conv_Ovf_U1:
          case OperationCode.Conv_Ovf_I2:
          case OperationCode.Conv_Ovf_U2:
          case OperationCode.Conv_Ovf_I4:
          case OperationCode.Conv_Ovf_U4:
          case OperationCode.Conv_Ovf_I8:
          case OperationCode.Conv_Ovf_U8:
            break;
          case OperationCode.Refanyval:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Ckfinite:
            break;
          case OperationCode.Mkrefany:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Ldtoken:
            value = this.GetRuntimeHandleFromToken(memReader.ReadUInt32());
            break;
          case OperationCode.Conv_U2:
          case OperationCode.Conv_U1:
          case OperationCode.Conv_I:
          case OperationCode.Conv_Ovf_I:
          case OperationCode.Conv_Ovf_U:
          case OperationCode.Add_Ovf:
          case OperationCode.Add_Ovf_Un:
          case OperationCode.Mul_Ovf:
          case OperationCode.Mul_Ovf_Un:
          case OperationCode.Sub_Ovf:
          case OperationCode.Sub_Ovf_Un:
          case OperationCode.Endfinally:
            break;
          case OperationCode.Leave: {
              uint leaveOffset = (uint)(memReader.Offset + 4 + memReader.ReadInt32());
              if (leaveOffset >= this.EndOfMethodOffset) {
                //  Error...
              }
              value = leaveOffset;
            }
            break;
          case OperationCode.Leave_S: {
              uint leaveOffset = (uint)(memReader.Offset + 1 + memReader.ReadSByte());
              if (leaveOffset >= this.EndOfMethodOffset) {
                //  Error...
              }
              value = leaveOffset;
            }
            break;
          case OperationCode.Stind_I:
          case OperationCode.Conv_U:
          case OperationCode.Arglist:
          case OperationCode.Ceq:
          case OperationCode.Cgt:
          case OperationCode.Cgt_Un:
          case OperationCode.Clt:
          case OperationCode.Clt_Un:
            break;
          case OperationCode.Ldftn:
          case OperationCode.Ldvirtftn:
            value = this.GetMethod(memReader.ReadUInt32());
            break;
          case OperationCode.Ldarg:
          case OperationCode.Ldarga:
          case OperationCode.Starg:
            value = this.GetParameter(memReader.ReadUInt16());
            break;
          case OperationCode.Ldloc:
          case OperationCode.Ldloca:
          case OperationCode.Stloc:
            value = this.GetLocal(memReader.ReadUInt16());
            break;
          case OperationCode.Localloc:
            break;
          case OperationCode.Endfilter:
            break;
          case OperationCode.Unaligned_:
            value = memReader.ReadByte();
            break;
          case OperationCode.Volatile_:
          case OperationCode.Tail_:
            break;
          case OperationCode.Initobj:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Constrained_:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Cpblk:
          case OperationCode.Initblk:
            break;
          case OperationCode.No_:
            value = (OperationCheckFlags)memReader.ReadByte();
            break;
          case OperationCode.Rethrow:
            break;
          case OperationCode.Sizeof:
            value = this.GetType(memReader.ReadUInt32());
            break;
          case OperationCode.Refanytype:
          case OperationCode.Readonly_:
            break;
          default:
            this.PEFileToObjectModel.PEFileReader.ErrorContainer.AddILError(this.MethodDefinition, offset, MetadataReaderErrorKind.UnknownILInstruction);
            break;
        }
        instrList[instructionNumber++] = new CilInstruction(cilOpCode, document, offset, value);
      }
      this.MethodBody.SetCilInstructions(instrList);
      return true;
    }

    static int CountCilInstructions(MemoryReader memReader) {
      int count = 0;
      while (memReader.NotEndOfBytes) {
        count++;
        OperationCode cilOpCode = memReader.ReadOpcode();
        switch (cilOpCode) {
          case OperationCode.Ldarg_S:
          case OperationCode.Ldarga_S:
          case OperationCode.Starg_S:
          case OperationCode.Ldloc_S:
          case OperationCode.Ldloca_S:
          case OperationCode.Stloc_S:
          case OperationCode.Ldc_I4_S:
          case OperationCode.Br_S:
          case OperationCode.Brfalse_S:
          case OperationCode.Brtrue_S:
          case OperationCode.Beq_S:
          case OperationCode.Bge_S:
          case OperationCode.Bgt_S:
          case OperationCode.Ble_S:
          case OperationCode.Blt_S:
          case OperationCode.Bne_Un_S:
          case OperationCode.Bge_Un_S:
          case OperationCode.Bgt_Un_S:
          case OperationCode.Ble_Un_S:
          case OperationCode.Blt_Un_S:
          case OperationCode.Leave_S:
          case OperationCode.Unaligned_:
          case OperationCode.No_:
            memReader.SkipBytes(1);
            break;
          case OperationCode.Ldarg:
          case OperationCode.Ldarga:
          case OperationCode.Starg:
          case OperationCode.Ldloc:
          case OperationCode.Ldloca:
          case OperationCode.Stloc:
            memReader.SkipBytes(2);
            break;
          case OperationCode.Ldc_I4:
          case OperationCode.Jmp:
          case OperationCode.Call:
          case OperationCode.Calli:
          case OperationCode.Br:
          case OperationCode.Brfalse:
          case OperationCode.Brtrue:
          case OperationCode.Beq:
          case OperationCode.Bge:
          case OperationCode.Bgt:
          case OperationCode.Ble:
          case OperationCode.Blt:
          case OperationCode.Bne_Un:
          case OperationCode.Bge_Un:
          case OperationCode.Bgt_Un:
          case OperationCode.Ble_Un:
          case OperationCode.Blt_Un:
          case OperationCode.Callvirt:
          case OperationCode.Cpobj:
          case OperationCode.Ldobj:
          case OperationCode.Ldstr:
          case OperationCode.Newobj:
          case OperationCode.Castclass:
          case OperationCode.Isinst:
          case OperationCode.Unbox:
          case OperationCode.Ldfld:
          case OperationCode.Ldflda:
          case OperationCode.Stfld:
          case OperationCode.Ldsfld:
          case OperationCode.Ldsflda:
          case OperationCode.Stsfld:
          case OperationCode.Stobj:
          case OperationCode.Box:
          case OperationCode.Newarr:
          case OperationCode.Ldelema:
          case OperationCode.Ldelem:
          case OperationCode.Stelem:
          case OperationCode.Unbox_Any:
          case OperationCode.Refanyval:
          case OperationCode.Mkrefany:
          case OperationCode.Ldtoken:
          case OperationCode.Leave:
          case OperationCode.Ldftn:
          case OperationCode.Ldvirtftn:
          case OperationCode.Initobj:
          case OperationCode.Constrained_:
          case OperationCode.Sizeof:
          case OperationCode.Ldc_R4:
            memReader.SkipBytes(4);
            break;
          case OperationCode.Ldc_I8:
          case OperationCode.Ldc_R8:
            memReader.SkipBytes(8);
            break;
          case OperationCode.Switch:
            int numTargets = (int)memReader.ReadUInt32();
            memReader.SkipBytes(4*numTargets);
            break;
          default:
            break;
        }
      }
      memReader.SeekOffset(0);
      return count;
    }
    bool PopulateExceptionInformation() {
      SEHTableEntry[]/*?*/ sehTable = this.MethodIL.SEHTable;
      if (sehTable != null) {
        int n = sehTable.Length;
        var exceptions = new IOperationExceptionInformation[n];
        for (int i = 0; i < n; i++) {
          SEHTableEntry sehTableEntry = sehTable[i];
          int sehFlag = (int)sehTableEntry.SEHFlags;
          int handlerKindIndex = sehFlag >= ILReader.HandlerKindMap.Length ? ILReader.HandlerKindMap.Length - 1 : sehFlag;
          ITypeReference exceptionType = Dummy.TypeReference;
          uint filterDecisionStart = 0;
          HandlerKind handlerKind = ILReader.HandlerKindMap[handlerKindIndex];
          uint tryStart = sehTableEntry.TryOffset;
          uint tryEnd = sehTableEntry.TryOffset + sehTableEntry.TryLength;
          uint handlerStart = sehTableEntry.HandlerOffset;
          uint handlerEnd = sehTableEntry.HandlerOffset + sehTableEntry.HandlerLength;

          if (sehTableEntry.SEHFlags == SEHFlags.Catch) {
            ITypeReference/*?*/ typeRef = this.PEFileToObjectModel.GetTypeReferenceForToken(this.MethodDefinition, sehTableEntry.ClassTokenOrFilterOffset);
            if (typeRef == null) {
              //  Error
              return false;
            } else {
              exceptionType = typeRef;
            }
          } else if (sehTableEntry.SEHFlags == SEHFlags.Filter) {
            exceptionType = this.PEFileToObjectModel.PlatformType.SystemObject;
            filterDecisionStart = sehTableEntry.ClassTokenOrFilterOffset;
          }
          exceptions[i] = new CilExceptionInformation(handlerKind, exceptionType, tryStart, tryEnd, filterDecisionStart, handlerStart, handlerEnd);
        }
        this.MethodBody.SetExceptionInformation(exceptions);
      }
      return true;
    }

    internal bool ReadIL() {
      if (!this.LoadLocalSignature())
        return false;
      if (!this.PopulateCilInstructions())
        return false;
      if (!this.PopulateExceptionInformation())
        return false;
      return true;
    }
  }
}

namespace Microsoft.Cci.MetadataReader {
  using Microsoft.Cci.MetadataReader.ObjectModelImplementation;

  /// <summary>
  /// 
  /// </summary>
  public sealed class MethodBodyDocument : IDocument {

    /// <summary>
    /// 
    /// </summary>
    internal MethodBodyDocument(MethodDefinition method) {
      this.method = method;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="standAloneSignatureToken"></param>
    /// <returns></returns>
    public ITypeReference GetTypeFromToken(uint standAloneSignatureToken) {
      ITypeReference/*?*/ result = this.method.PEFileToObjectModel.GetTypeReferenceFromStandaloneSignatureToken(this.method, standAloneSignatureToken);
      if (result != null) return result;
      return Dummy.TypeReference;
    }

    /// <summary>
    /// The location where this document was found, or where it should be stored.
    /// This will also uniquely identify the source document within an instance of compilation host.
    /// </summary>
    public string Location {
      get { return this.method.PEFileToObjectModel.Module.ModuleIdentity.Location; }
    }

    internal MethodDefinition method;

    /// <summary>
    /// 
    /// </summary>
    public uint MethodToken {
      get { return this.method.TokenValue; }
    }

    /// <summary>
    /// The name of the document. For example the name of the file if the document corresponds to a file.
    /// </summary>
    public IName Name {
      get { return this.method.PEFileToObjectModel.Module.ModuleIdentity.Name; }
    }

    /// <summary>
    /// Returns a token decoder for the method associated with this document.
    /// </summary>
    public ITokenDecoder TokenDecoder {
      get {
        return this.method;
      }
    }

  }

  /// <summary>
  /// Represents a location in IL operation stream.
  /// </summary>
  internal sealed class MethodBodyLocation : IILLocation {

    /// <summary>
    /// Allocates an object that represents a location in IL operation stream.
    /// </summary>
    /// <param name="document">The document containing this method whose body contains this location.</param>
    /// <param name="offset">Offset into the IL Stream.</param>
    internal MethodBodyLocation(MethodBodyDocument document, uint offset) {
      this.document = document;
      this.offset = offset;
    }

    /// <summary>
    /// The document containing this method whose body contains this location.
    /// </summary>
    public MethodBodyDocument Document {
      get { return this.document; }
    }
    readonly MethodBodyDocument document;

    /// <summary>
    /// The method whose body contains this IL operation whose location this is.
    /// </summary>
    /// <value></value>
    public IMethodDefinition MethodDefinition {
      get { return this.document.method; }
    }

    /// <summary>
    /// Offset into the IL Stream.
    /// </summary>
    public uint Offset {
      get { return this.offset; }
    }
    readonly uint offset;

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.Document; }
    }

    #endregion

  }
}
