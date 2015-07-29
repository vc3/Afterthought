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
using System.Text;
using System.IO;

using Microsoft.Cci.MetadataReader.PEFileFlags;
using Microsoft.Cci.UtilityDataStructures;
using Microsoft.Cci.MetadataReader.Errors;
//^ using Microsoft.Contracts;


//  Notes: We use 3 different things. 
//  Tokens: Includes the Table number, but used only for "public" tokens
//  RowNumber: 0 based. Used only internally within the methods.
//  RowId: 1 based. Used in external communication when the Table referring to is known

namespace Microsoft.Cci.MetadataReader.PEFile {

  #region Metadata Stream Readers
  internal struct StringStreamReader {
    internal MemoryReader MemoryReader;
    internal string this[uint offset] {
      get
        //^ requires offset >= 0 && offset < this.MemoryReader.Length;
      {
        int bytesRead;
        string str = this.MemoryReader.PeekUTF8NullTerminated((int)offset, out bytesRead);
        //^ assert offset + bytesRead <= this.MemoryReader.Length;
        return str;
      }
    }
  }

  internal struct BlobStreamReader {
    static internal byte[] Empty = new byte[0];
    internal MemoryReader MemoryReader;
    internal byte[] this[uint offset] {
      get
        //^ requires offset >= 0 && offset < this.MemoryReader.Length;
      {
        int bytesRead;
        int numberOfBytes = this.MemoryReader.PeekCompressedInt32((int)offset, out bytesRead);
        if (numberOfBytes == -1)
          return BlobStreamReader.Empty;
        //^ assert offset + bytesRead + numberOfBytes <= this.MemoryReader.Length;
        return this.MemoryReader.PeekBytes((int)offset + bytesRead, numberOfBytes);
      }
    }
    //  Read index'th Byte from the blob at offset...
    internal byte GetByteAt(uint offset, int index)
      //^ requires offset >= 0 && offset < this.MemoryReader.Length;
    {
      int bytesRead;
      int numberOfBytes = this.MemoryReader.PeekCompressedInt32((int)offset, out bytesRead);
      if (index >= numberOfBytes)
        return 0;
      return this.MemoryReader.PeekByte((int)offset + bytesRead + index);
    }
    internal MemoryBlock GetMemoryBlockAt(uint offset)
      //^ requires offset >= 0 && offset < this.MemoryReader.Length;
    {
      uint bytesRead;
      uint numberOfBytes = this.MemoryReader.PeekCompressedUInt32(offset, out bytesRead);
      //^ assume offset + bytesRead + numberOfBytes <= this.MemoryReader.Length;
      //  otherwise its an error...
      return this.MemoryReader.GetMemoryBlockAt(offset + bytesRead, numberOfBytes);
    }
  }

  internal struct GUIDStreamReader {
    internal MemoryReader MemoryReader;
    internal Guid this[uint offset] {
      get
        //^ requires offset >= 0 && offset + sizeof(Guid) <= this.MemoryReader.Length;
      {
        if (offset == 0) {
          //TODO: error
          return Guid.Empty;
        }
        return this.MemoryReader.PeekGuid((int)(offset-1));
      }
    }
  }

  internal struct UserStringStreamReader {
    internal MemoryReader MemoryReader;
    internal string this[uint offset] {
      get
        //^ requires offset >= 0 && offset < this.MemoryReader.Length;
      {
        int bytesRead;
        int numberOfBytes = this.MemoryReader.PeekCompressedInt32((int)offset, out bytesRead);
        if (numberOfBytes == -1)
          return string.Empty;
        //^ assert offset + bytesRead + numberOfBytes <= this.MemoryReader.Length;
        return this.MemoryReader.PeekUTF16WithSize((int)offset + bytesRead, numberOfBytes & ~1);
      }
    }

    internal IEnumerable<string> GetStrings() {
      int i = 1;
      while (i < this.MemoryReader.Length) {
        int bytesRead;
        int numberOfBytes = this.MemoryReader.PeekCompressedInt32(i, out bytesRead);
        if (numberOfBytes < 1) {
          i += bytesRead;
        } else {
          yield return this.MemoryReader.PeekUTF16WithSize(i + bytesRead, numberOfBytes & ~1);
          i += bytesRead + numberOfBytes;
        }
      }
    }
  }
  #endregion Metadata Stream Readers

  #region Metadata Table Readers

  internal unsafe struct ModuleTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsGUIDHeapRefSizeSmall;
    internal readonly uint GenerationOffset;
    internal readonly uint NameOffset;
    internal readonly uint MVIdOffset;
    internal readonly uint EnCIdOffset;
    internal readonly uint EnCBaseIdOffset;
    internal readonly uint RowSize;
    internal readonly MemoryReader ModuleTableMemoryReader;

    internal ModuleTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      int guidHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsGUIDHeapRefSizeSmall = guidHeapRefSize == 2;
      this.GenerationOffset = 0;
      this.NameOffset = this.GenerationOffset + sizeof(UInt16);
      this.MVIdOffset = this.NameOffset + (uint)stringHeapRefSize;
      this.EnCIdOffset = this.MVIdOffset + (uint)guidHeapRefSize;
      this.EnCBaseIdOffset = this.EnCIdOffset + (uint)guidHeapRefSize;
      this.RowSize = this.EnCBaseIdOffset + (uint)guidHeapRefSize;
      this.ModuleTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ModuleRow this[uint rowId]  //  This is 1 based...
    {
      get
        //^ requires rowId > 0 && rowId <= this.NumberOfRows;
      {
        uint rowOffset = (rowId - 1) * this.RowSize;
        ushort generation = this.ModuleTableMemoryReader.PeekUInt16(rowOffset + this.GenerationOffset);
        uint name = this.ModuleTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint mvId = this.ModuleTableMemoryReader.PeekReference(rowOffset + this.MVIdOffset, this.IsGUIDHeapRefSizeSmall);
        uint encId = this.ModuleTableMemoryReader.PeekReference(rowOffset + this.EnCIdOffset, this.IsGUIDHeapRefSizeSmall);
        uint encBaseId = this.ModuleTableMemoryReader.PeekReference(rowOffset + this.EnCBaseIdOffset, this.IsGUIDHeapRefSizeSmall);
        ModuleRow moduleRow = new ModuleRow(generation, name, mvId, encId, encBaseId);
        return moduleRow;
      }
    }
    internal uint GetName(uint rowId) {
      uint rowOffset = (rowId - 1) * this.RowSize;
      uint name = this.ModuleTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
      return name;
    }
  }

  internal unsafe struct TypeRefTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsResolutionScopeRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int ResolutionScopeOffset;
    internal readonly int NameOffset;
    internal readonly int NamespaceOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader TypeRefTableMemoryReader;
    internal TypeRefTableReader(
      uint numberOfRows,
      int resolutionScopeRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsResolutionScopeRefSizeSmall = resolutionScopeRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.ResolutionScopeOffset = 0;
      this.NameOffset = this.ResolutionScopeOffset + resolutionScopeRefSize;
      this.NamespaceOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.NamespaceOffset + stringHeapRefSize;
      this.TypeRefTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal TypeRefRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId > 0 && rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint resolutionScope = this.TypeRefTableMemoryReader.PeekReference(rowOffset + this.ResolutionScopeOffset, this.IsResolutionScopeRefSizeSmall);
        resolutionScope = ResolutionScopeTag.ConvertToToken(resolutionScope);
        uint name = this.TypeRefTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint @namespace = this.TypeRefTableMemoryReader.PeekReference(rowOffset + this.NamespaceOffset, this.IsStringHeapRefSizeSmall);
        TypeRefRow typeRefRow = new TypeRefRow(resolutionScope, name, @namespace);
        return typeRefRow;
      }
    }
  }

  internal unsafe struct TypeDefTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsFieldRefSizeSmall;
    internal readonly bool IsMethodRefSizeSmall;
    internal readonly bool IsTypeDefOrRefRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int NamespaceOffset;
    internal readonly int ExtendsOffset;
    internal readonly int FieldListOffset;
    internal readonly int MethodListOffset;
    internal int RowSize;
    internal MemoryReader TypeDefTableMemoryReader;
    internal TypeDefTableReader(
      uint numberOfRows,
      int fieldRefSize,
      int methodRefSize,
      int typeDefOrRefRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsFieldRefSizeSmall = fieldRefSize == 2;
      this.IsMethodRefSizeSmall = methodRefSize == 2;
      this.IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.NameOffset = this.FlagsOffset + sizeof(UInt32);
      this.NamespaceOffset = this.NameOffset + stringHeapRefSize;
      this.ExtendsOffset = this.NamespaceOffset + stringHeapRefSize;
      this.FieldListOffset = this.ExtendsOffset + typeDefOrRefRefSize;
      this.MethodListOffset = this.FieldListOffset + fieldRefSize;
      this.RowSize = this.MethodListOffset + methodRefSize;
      this.TypeDefTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal TypeDefRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        TypeDefFlags flags = (TypeDefFlags)this.TypeDefTableMemoryReader.PeekUInt32(rowOffset + this.FlagsOffset);
        uint name = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint @namespace = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.NamespaceOffset, this.IsStringHeapRefSizeSmall);
        uint extends = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.ExtendsOffset, this.IsTypeDefOrRefRefSizeSmall);
        extends = TypeDefOrRefTag.ConvertToToken(extends);
        uint fieldList = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.FieldListOffset, this.IsFieldRefSizeSmall);
        uint methodList = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.MethodListOffset, this.IsMethodRefSizeSmall);
        TypeDefRow typeDefRow = new TypeDefRow(flags, name, @namespace, extends, fieldList, methodList);
        return typeDefRow;
      }
    }
    internal uint GetNamespace(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      return this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.NamespaceOffset, this.IsStringHeapRefSizeSmall);
    }
    internal uint GetName(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      return this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
    }
    internal uint GetExtends(uint rowId) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint extends = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.ExtendsOffset, this.IsTypeDefOrRefRefSizeSmall);
      return TypeDefOrRefTag.ConvertToToken(extends);
    }
    internal uint GetFieldStart(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint fieldListStart = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.FieldListOffset, this.IsFieldRefSizeSmall);
      return fieldListStart;
    }
    internal uint GetMethodStart(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint methodListStart = this.TypeDefTableMemoryReader.PeekReference(rowOffset + this.MethodListOffset, this.IsMethodRefSizeSmall);
      return methodListStart;
    }
    internal uint FindTypeContainingMethod(uint methodDefOrPtrRowId, int numberOfMethods) {
      uint numOfRows = this.NumberOfRows;
      int slot = this.TypeDefTableMemoryReader.BinarySearchForSlot(numOfRows, this.RowSize, this.MethodListOffset, methodDefOrPtrRowId, this.IsMethodRefSizeSmall);
      uint row = (uint)(slot + 1);
      if (row == 0) return 0;
      if (row > numOfRows) {
        if (methodDefOrPtrRowId <= numberOfMethods) return numOfRows;
        return 0;
      }
      uint value = this.GetMethodStart(row);
      if (value == methodDefOrPtrRowId) {
        while (row < numOfRows) {
          uint newRow = row + 1;
          value = this.GetMethodStart(newRow);
          if (value == methodDefOrPtrRowId)
            row = newRow;
          else
            break;
        }
      }
      return row;
    }
    internal uint FindTypeContainingField(uint fieldDefOrPtrRowId, int numberOfFields) {
      uint numOfRows = this.NumberOfRows;
      int slot = this.TypeDefTableMemoryReader.BinarySearchForSlot(numOfRows, this.RowSize, this.FieldListOffset, fieldDefOrPtrRowId, this.IsFieldRefSizeSmall);
      uint row = (uint)(slot + 1);
      if (row == 0) return 0;
      if (row > numOfRows) {
        if (fieldDefOrPtrRowId <= numberOfFields) return numOfRows;
        return 0;
      }
      uint value = this.GetFieldStart(row);
      if (value == fieldDefOrPtrRowId) {
        while (row < numOfRows) {
          uint newRow = row + 1;
          value = this.GetFieldStart(newRow);
          if (value == fieldDefOrPtrRowId)
            row = newRow;
          else
            break;
        }
      }
      return row;
    }
  }

  internal unsafe struct FieldPtrTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsFieldTableRowRefSizeSmall;
    internal readonly int FieldOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader FieldPtrTableMemoryReader;
    internal FieldPtrTableReader(
      uint numberOfRows,
      int fieldTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsFieldTableRowRefSizeSmall = fieldTableRowRefSize == 2;
      this.FieldOffset = 0;
      this.RowSize = this.FieldOffset + fieldTableRowRefSize;
      this.FieldPtrTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal FieldPtrRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint field = this.FieldPtrTableMemoryReader.PeekReference(rowOffset + this.FieldOffset, this.IsFieldTableRowRefSizeSmall);
        FieldPtrRow fieldPtrRow = new FieldPtrRow(field);
        return fieldPtrRow;
      }
    }
#endif
    internal uint GetFieldFor(uint rowId) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint field = this.FieldPtrTableMemoryReader.PeekReference(rowOffset + this.FieldOffset, this.IsFieldTableRowRefSizeSmall);
      return field;
    }
    internal uint GetRowIdForFieldDefRow(uint fieldDefRowId) {
      return (uint)(this.FieldPtrTableMemoryReader.LinearSearchReference(this.RowSize, this.FieldOffset, fieldDefRowId, this.IsFieldTableRowRefSizeSmall) + 1);
    }
  }

  internal unsafe struct FieldTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int SignatureOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader FieldTableMemoryReader;
    internal FieldTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.NameOffset = this.FlagsOffset + sizeof(UInt16);
      this.SignatureOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.SignatureOffset + blobHeapRefSize;
      this.FieldTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal FieldRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        FieldFlags flags = (FieldFlags)this.FieldTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        uint name = this.FieldTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint signature = this.FieldTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
        FieldRow fieldRow = new FieldRow(flags, name, signature);
        return fieldRow;
      }
    }
    internal uint GetSignature(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint signature = this.FieldTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
      return signature;
    }
  }

  internal unsafe struct MethodPtrTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsMethodTableRowRefSizeSmall;
    internal readonly int MethodOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader MethodPtrTableMemoryReader;
    internal MethodPtrTableReader(
      uint numberOfRows,
      int methodTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsMethodTableRowRefSizeSmall = methodTableRowRefSize == 2;
      this.MethodOffset = 0;
      this.RowSize = this.MethodOffset + methodTableRowRefSize;
      this.MethodPtrTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal MethodPtrRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint method = this.MethodPtrTableMemoryReader.PeekReference(rowOffset + this.MethodOffset, this.IsMethodTableRowRefSizeSmall);
        MethodPtrRow methodPtrRow = new MethodPtrRow(method);
        return methodPtrRow;
      }
    }
#endif
    internal uint GetMethodFor(uint rowId) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint method = this.MethodPtrTableMemoryReader.PeekReference(rowOffset + this.MethodOffset, this.IsMethodTableRowRefSizeSmall);
      return method;
    }
    internal uint GetRowIdForMethodDefRow(uint methodDefRowId) {
      return (uint)(this.MethodPtrTableMemoryReader.LinearSearchReference(this.RowSize, this.MethodOffset, methodDefRowId, this.IsMethodTableRowRefSizeSmall) + 1);
    }
  }

  internal unsafe struct MethodTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsParamRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int RVAOffset;
    internal readonly int ImplFlagsOffset;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int SignatureOffset;
    internal readonly int ParamListOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader MethodTableMemoryReader;
    internal MethodTableReader(
      uint numberOfRows,
      int paramRefSize,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsParamRefSizeSmall = paramRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.RVAOffset = 0;
      this.ImplFlagsOffset = this.RVAOffset + sizeof(UInt32);
      this.FlagsOffset = this.ImplFlagsOffset + sizeof(UInt16);
      this.NameOffset = this.FlagsOffset + sizeof(UInt16);
      this.SignatureOffset = this.NameOffset + stringHeapRefSize;
      this.ParamListOffset = this.SignatureOffset + blobHeapRefSize;
      this.RowSize = this.ParamListOffset + paramRefSize;
      this.MethodTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal MethodRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        int rva = this.MethodTableMemoryReader.PeekInt32(rowOffset + this.RVAOffset);
        MethodImplFlags implFlags = (MethodImplFlags)this.MethodTableMemoryReader.PeekUInt16(rowOffset + this.ImplFlagsOffset);
        MethodFlags flags = (MethodFlags)this.MethodTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        uint name = this.MethodTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint signature = this.MethodTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
        uint paramList = this.MethodTableMemoryReader.PeekReference(rowOffset + this.ParamListOffset, this.IsParamRefSizeSmall);
        MethodRow methodRow = new MethodRow(rva, implFlags, flags, name, signature, paramList);
        return methodRow;
      }
    }
    internal uint GetParamStart(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint paramListStart = this.MethodTableMemoryReader.PeekReference(rowOffset + this.ParamListOffset, this.IsParamRefSizeSmall);
      return paramListStart;
    }
    internal uint GetSignature(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint signature = this.MethodTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
      return signature;
    }
    internal int GetNextRVA(
      int rva
    ) {
      int nextRVA = int.MaxValue;
      int endOffset = (int)this.NumberOfRows * this.RowSize;
      for (int iterOffset = this.RVAOffset; iterOffset < endOffset; iterOffset += this.RowSize) {
        int currentRVA = this.MethodTableMemoryReader.PeekInt32(iterOffset);
        if (currentRVA > rva && currentRVA < nextRVA) {
          nextRVA = currentRVA;
        }
      }
      return nextRVA == int.MaxValue ? -1 : nextRVA;
    }
  }

  internal unsafe struct ParamPtrTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsParamTableRowRefSizeSmall;
    internal readonly int ParamOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ParamPtrTableMemoryReader;
    internal ParamPtrTableReader(
      uint numberOfRows,
      int paramTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsParamTableRowRefSizeSmall = paramTableRowRefSize == 2;
      this.ParamOffset = 0;
      this.RowSize = this.ParamOffset + paramTableRowRefSize;
      this.ParamPtrTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal ParamPtrRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint param = this.ParamPtrTableMemoryReader.PeekReference(rowOffset + this.ParamOffset, this.IsParamTableRowRefSizeSmall);
        ParamPtrRow paramPtrRow = new ParamPtrRow(param);
        return paramPtrRow;
      }
    }
#endif
    internal uint GetParamFor(uint rowId) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint param = this.ParamPtrTableMemoryReader.PeekReference(rowOffset + this.ParamOffset, this.IsParamTableRowRefSizeSmall);
      return param;
    }
  }

  internal unsafe struct ParamTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int SequenceOffset;
    internal readonly int NameOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ParamTableMemoryReader;
    internal ParamTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.SequenceOffset = this.FlagsOffset + sizeof(UInt16);
      this.NameOffset = this.SequenceOffset + sizeof(UInt16);
      this.RowSize = this.NameOffset + stringHeapRefSize;
      this.ParamTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ParamRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        ParamFlags flags = (ParamFlags)this.ParamTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        ushort sequence = this.ParamTableMemoryReader.PeekUInt16(rowOffset + this.SequenceOffset);
        uint name = this.ParamTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        ParamRow paramRow = new ParamRow(flags, sequence, name);
        return paramRow;
      }
    }
  }

  internal unsafe struct InterfaceImplTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsTypeDefTableRowRefSizeSmall;
    internal readonly bool IsTypeDefOrRefRefSizeSmall;
    internal readonly int ClassOffset;
    internal readonly int InterfaceOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader InterfaceImplTableMemoryReader;
    internal InterfaceImplTableReader(
      uint numberOfRows,
      int typeDefTableRowRefSize,
      int typeDefOrRefRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
      this.IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
      this.ClassOffset = 0;
      this.InterfaceOffset = this.ClassOffset + typeDefTableRowRefSize;
      this.RowSize = this.InterfaceOffset + typeDefOrRefRefSize;
      this.InterfaceImplTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal InterfaceImplRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint @class = this.InterfaceImplTableMemoryReader.PeekReference(rowOffset + this.ClassOffset, this.IsTypeDefTableRowRefSizeSmall);
        uint @interface = this.InterfaceImplTableMemoryReader.PeekReference(rowOffset + this.InterfaceOffset, this.IsTypeDefOrRefRefSizeSmall);
        @interface = TypeDefOrRefTag.ConvertToToken(@interface);
        InterfaceImplRow interfaceImplRow = new InterfaceImplRow(@class, @interface);
        return interfaceImplRow;
      }
    }
#endif
    internal uint FindInterfaceImplForType(
      uint typeDefRowId,
      out uint interfaceCount
    ) {
      interfaceCount = 0;
      int foundRowNumber =
        this.InterfaceImplTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ClassOffset,
          typeDefRowId,
          this.IsTypeDefOrRefRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.InterfaceImplTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.ClassOffset, this.IsTypeDefOrRefRefSizeSmall) == typeDefRowId
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.InterfaceImplTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.ClassOffset, this.IsTypeDefOrRefRefSizeSmall) == typeDefRowId
      ) {
        endRowNumber++;
      }
      interfaceCount = (uint)(endRowNumber - startRowNumber + 1);
      return (uint)startRowNumber + 1;
    }
    internal uint GetInterface(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint @interface = this.InterfaceImplTableMemoryReader.PeekReference(rowOffset + this.InterfaceOffset, this.IsTypeDefOrRefRefSizeSmall);
      @interface = TypeDefOrRefTag.ConvertToToken(@interface);
      return @interface;
    }
  }

  internal unsafe struct MemberRefTableReader {
    internal uint NumberOfRows;
    internal readonly bool IsMemberRefParentRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int ClassOffset;
    internal readonly int NameOffset;
    internal readonly int SignatureOffset;
    internal int RowSize;
    internal MemoryReader MemberRefTableMemoryReader;
    internal MemberRefTableReader(
      uint numberOfRows,
      int memberRefParentRefSize,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsMemberRefParentRefSizeSmall = memberRefParentRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.ClassOffset = 0;
      this.NameOffset = this.ClassOffset + memberRefParentRefSize;
      this.SignatureOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.SignatureOffset + blobHeapRefSize;
      this.MemberRefTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal MemberRefRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint @class = this.MemberRefTableMemoryReader.PeekReference(rowOffset + this.ClassOffset, this.IsMemberRefParentRefSizeSmall);
        @class = MemberRefParentTag.ConvertToToken(@class);
        uint name = this.MemberRefTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint signature = this.MemberRefTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
        return new MemberRefRow(@class, name, signature);
      }
    }
    internal uint GetSignature(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint signature = this.MemberRefTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
      return signature;
    }
  }

  internal unsafe struct ConstantTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsHasConstantRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int TypeOffset;
    internal readonly int ParentOffset;
    internal readonly int ValueOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ConstantTableMemoryReader;
    internal ConstantTableReader(
      uint numberOfRows,
      int hasConstantRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsHasConstantRefSizeSmall = hasConstantRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.TypeOffset = 0;
      this.ParentOffset = this.TypeOffset + sizeof(Byte) + 1; //  Alignment here (+1)...
      this.ValueOffset = this.ParentOffset + hasConstantRefSize;
      this.RowSize = this.ValueOffset + blobHeapRefSize;
      this.ConstantTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ConstantRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        byte type = this.ConstantTableMemoryReader.PeekByte(rowOffset + this.TypeOffset);
        uint parent = this.ConstantTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsHasConstantRefSizeSmall);
        parent = HasConstantTag.ConvertToToken(parent);
        uint value = this.ConstantTableMemoryReader.PeekReference(rowOffset + this.ValueOffset, this.IsBlobHeapRefSizeSmall);
        ConstantRow constantRow = new ConstantRow(type, parent, value);
        return constantRow;
      }
    }
    internal uint GetConstantRowId(
      uint parentToken
    ) {
      int foundRowNumber =
        this.ConstantTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ParentOffset,
          HasConstantTag.ConvertToTag(parentToken),
          this.IsHasConstantRefSizeSmall
        );
      return (uint)(foundRowNumber + 1);
    }
  }

  internal unsafe struct CustomAttributeTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsHasCustomAttributeRefSizeSmall;
    internal readonly bool IsCustomAttriubuteTypeRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int ParentOffset;
    internal readonly int TypeOffset;
    internal readonly int ValueOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader CustomAttributeTableMemoryReader;
    internal CustomAttributeTableReader(
      uint numberOfRows,
      int hasCustomAttributeRefSize,
      int customAttributeTypeRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsHasCustomAttributeRefSizeSmall = hasCustomAttributeRefSize == 2;
      this.IsCustomAttriubuteTypeRefSizeSmall = customAttributeTypeRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.ParentOffset = 0;
      this.TypeOffset = this.ParentOffset + hasCustomAttributeRefSize;
      this.ValueOffset = this.TypeOffset + customAttributeTypeRefSize;
      this.RowSize = this.ValueOffset + blobHeapRefSize;
      this.CustomAttributeTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal CustomAttributeRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint parent = this.CustomAttributeTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsHasCustomAttributeRefSizeSmall);
        parent = HasCustomAttributeTag.ConvertToToken(parent);
        uint type = this.CustomAttributeTableMemoryReader.PeekReference(rowOffset + this.TypeOffset, this.IsCustomAttriubuteTypeRefSizeSmall);
        type = CustomAttributeTypeTag.ConvertToToken(type);
        uint value = this.CustomAttributeTableMemoryReader.PeekReference(rowOffset + this.ValueOffset, this.IsBlobHeapRefSizeSmall);
        CustomAttributeRow customAttributeRow = new CustomAttributeRow(parent, type, value);
        return customAttributeRow;
      }
    }
    internal uint FindCustomAttributesForToken(
      uint token,
      out uint customAttributeCount
    ) {
      customAttributeCount = 0;
      uint searchCodedTag = HasCustomAttributeTag.ConvertToTag(token);
      return this.BinarySearchTag(searchCodedTag, ref customAttributeCount);
    }
    private uint BinarySearchTag(uint searchCodedTag, ref uint customAttributeCount) {
      int foundRowNumber =
        this.CustomAttributeTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ParentOffset,
          searchCodedTag,
          this.IsHasCustomAttributeRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.CustomAttributeTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.ParentOffset, this.IsHasCustomAttributeRefSizeSmall) == searchCodedTag
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.CustomAttributeTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.ParentOffset, this.IsHasCustomAttributeRefSizeSmall) == searchCodedTag
      ) {
        endRowNumber++;
      }
      customAttributeCount = (uint)(endRowNumber - startRowNumber + 1);
      return (uint)(startRowNumber + 1);
    }
  }

  internal unsafe struct FieldMarshalTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsHasFieldMarshalRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int ParentOffset;
    internal readonly int NativeTypeOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader FieldMarshalTableMemoryReader;
    internal FieldMarshalTableReader(
      uint numberOfRows,
      int hasFieldMarshalRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsHasFieldMarshalRefSizeSmall = hasFieldMarshalRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.ParentOffset = 0;
      this.NativeTypeOffset = this.ParentOffset + hasFieldMarshalRefSize;
      this.RowSize = this.NativeTypeOffset + blobHeapRefSize;
      this.FieldMarshalTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal FieldMarshalRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint parent = this.FieldMarshalTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsHasFieldMarshalRefSizeSmall);
        parent = HasFieldMarshalTag.ConvertToToken(parent);
        uint nativeType = this.FieldMarshalTableMemoryReader.PeekReference(rowOffset + this.NativeTypeOffset, this.IsBlobHeapRefSizeSmall);
        FieldMarshalRow fieldMarshalRow = new FieldMarshalRow(parent, nativeType);
        return fieldMarshalRow;
      }
    }
    internal uint GetFieldMarshalRowId(
      uint token
    ) {
      int foundRowNumber =
        this.FieldMarshalTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ParentOffset,
          HasFieldMarshalTag.ConvertToTag(token),
          this.IsHasFieldMarshalRefSizeSmall
        );
      return (uint)(foundRowNumber+1);
    }
  }

  internal unsafe struct DeclSecurityTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsHasDeclSecurityRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int ActionOffset;
    internal readonly int ParentOffset;
    internal readonly int PermissionSetOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader DeclSecurityTableMemoryReader;
    internal DeclSecurityTableReader(
      uint numberOfRows,
      int hasDeclSecurityRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsHasDeclSecurityRefSizeSmall = hasDeclSecurityRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.ActionOffset = 0;
      this.ParentOffset = this.ActionOffset + sizeof(UInt16);
      this.PermissionSetOffset = this.ParentOffset + hasDeclSecurityRefSize;
      this.RowSize = this.PermissionSetOffset + blobHeapRefSize;
      this.DeclSecurityTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal DeclSecurityRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        DeclSecurityActionFlags actionFlags = (DeclSecurityActionFlags)this.DeclSecurityTableMemoryReader.PeekUInt16(rowOffset + this.ActionOffset);
        uint parent = this.DeclSecurityTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsHasDeclSecurityRefSizeSmall);
        parent = HasDeclSecurityTag.ConvertToToken(parent);
        uint permissionSet = this.DeclSecurityTableMemoryReader.PeekReference(rowOffset + this.PermissionSetOffset, this.IsBlobHeapRefSizeSmall);
        DeclSecurityRow declSecurityRow = new DeclSecurityRow(actionFlags, parent, permissionSet);
        return declSecurityRow;
      }
    }
    internal uint FindSecurityAttributesForToken(
      uint token,
      out uint securityAttributeCount
    ) {
      securityAttributeCount = 0;
      uint searchCodedTag = HasDeclSecurityTag.ConvertToTag(token);
      return this.BinarySearchTag(searchCodedTag, ref securityAttributeCount);
    }
    private uint BinarySearchTag(uint searchCodedTag, ref uint securityAttributeCount) {
      int foundRowNumber =
        this.DeclSecurityTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ParentOffset,
          searchCodedTag,
          this.IsHasDeclSecurityRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.DeclSecurityTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.ParentOffset, this.IsHasDeclSecurityRefSizeSmall) == searchCodedTag
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.DeclSecurityTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.ParentOffset, this.IsHasDeclSecurityRefSizeSmall) == searchCodedTag
      ) {
        endRowNumber++;
      }
      securityAttributeCount = (uint)(endRowNumber - startRowNumber + 1);
      return (uint)(startRowNumber + 1);
    }
  }

  internal unsafe struct ClassLayoutTableReader {
    internal uint NumberOfRows;
    internal readonly bool IsTypeDefTableRowRefSizeSmall;
    internal readonly int PackagingSizeOffset;
    internal readonly int ClassSizeOffset;
    internal readonly int ParentOffset;
    internal int RowSize;
    internal MemoryReader ClassLayoutTableMemoryReader;
    internal ClassLayoutTableReader(
      uint numberOfRows,
      int typeDefTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
      this.PackagingSizeOffset = 0;
      this.ClassSizeOffset = this.PackagingSizeOffset + sizeof(UInt16);
      this.ParentOffset = this.ClassSizeOffset + sizeof(UInt32);
      this.RowSize = this.ParentOffset + typeDefTableRowRefSize;
      this.ClassLayoutTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal ClassLayoutRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        ushort packingSize = this.ClassLayoutTableMemoryReader.PeekUInt16(rowOffset + this.PackagingSizeOffset);
        uint classSize = this.ClassLayoutTableMemoryReader.PeekUInt32(rowOffset + this.ClassSizeOffset);
        uint parent = this.ClassLayoutTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsTypeDefTableRowRefSizeSmall);
        ClassLayoutRow classLayoutRow = new ClassLayoutRow(packingSize, classSize, parent);
        return classLayoutRow;
      }
    }
#endif
    internal ushort GetPackingSize(
      uint typeRowId
    ) {
      int foundRowNumber =
        this.ClassLayoutTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ParentOffset,
          typeRowId,
          this.IsTypeDefTableRowRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int rowOffset = foundRowNumber * this.RowSize;
      ushort packingSize = this.ClassLayoutTableMemoryReader.PeekUInt16(rowOffset + this.PackagingSizeOffset);
      return packingSize;
    }
    internal uint GetClassSize(
      uint typeRowId
    ) {
      int foundRowNumber =
        this.ClassLayoutTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ParentOffset,
          typeRowId,
          this.IsTypeDefTableRowRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int rowOffset = foundRowNumber * this.RowSize;
      uint classSize = this.ClassLayoutTableMemoryReader.PeekUInt32(rowOffset + this.ClassSizeOffset);
      return classSize;
    }
  }

  internal unsafe struct FieldLayoutTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsFieldTableRowRefSizeSmall;
    internal readonly int OffsetOffset;
    internal readonly int FieldOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader FieldLayoutTableMemoryReader;
    internal FieldLayoutTableReader(
      uint numberOfRows,
      int fieldTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsFieldTableRowRefSizeSmall = fieldTableRowRefSize == 2;
      this.OffsetOffset = 0;
      this.FieldOffset = this.OffsetOffset + sizeof(UInt32);
      this.RowSize = this.FieldOffset + fieldTableRowRefSize;
      this.FieldLayoutTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal FieldLayoutRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint offset = this.FieldLayoutTableMemoryReader.PeekUInt32(rowOffset + this.OffsetOffset);
        uint field = this.FieldLayoutTableMemoryReader.PeekReference(rowOffset + this.FieldOffset, this.IsFieldTableRowRefSizeSmall);
        FieldLayoutRow fieldLayoutRow = new FieldLayoutRow(offset, field);
        return fieldLayoutRow;
      }
    }
#endif
    internal uint GetOffset(uint rowId) {
      int foundRowNumber =
        this.FieldLayoutTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.FieldOffset,
          rowId,
          this.IsFieldTableRowRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int rowOffset = foundRowNumber * this.RowSize;
      uint offset = this.FieldLayoutTableMemoryReader.PeekUInt32(rowOffset + this.OffsetOffset);
      return offset;
    }
  }

  internal unsafe struct StandAloneSigTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int SignatureOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader StandAloneSigTableMemoryReader;
    internal StandAloneSigTableReader(
      uint numberOfRows,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.SignatureOffset = 0;
      this.RowSize = this.SignatureOffset + blobHeapRefSize;
      this.StandAloneSigTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal StandAloneSigRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint signature = this.StandAloneSigTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
        StandAloneSigRow standAloneRow = new StandAloneSigRow(signature);
        return standAloneRow;
      }
    }
  }

  internal unsafe struct EventMapTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsTypeDefTableRowRefSizeSmall;
    internal readonly bool IsEventRefSizeSmall;
    internal readonly int ParentOffset;
    internal readonly int EventListOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader EventMapTableMemoryReader;
    internal EventMapTableReader(
      uint numberOfRows,
      int typeDefTableRowRefSize,
      int eventRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
      this.IsEventRefSizeSmall = eventRefSize == 2;
      this.ParentOffset = 0;
      this.EventListOffset = this.ParentOffset + typeDefTableRowRefSize;
      this.RowSize = this.EventListOffset + eventRefSize;
      this.EventMapTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal EventMapRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint parent = this.EventMapTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsTypeDefTableRowRefSizeSmall);
        uint eventList = this.EventMapTableMemoryReader.PeekReference(rowOffset + this.EventListOffset, this.IsEventRefSizeSmall);
        EventMapRow eventMapRow = new EventMapRow(parent, eventList);
        return eventMapRow;
      }
    }
#endif
    internal uint FindEventMapRowIdFor(
      uint typeDefRowId
    ) {
      //  We do a linear scan here because we dont have these tables sorted
      int rowNumber =
        this.EventMapTableMemoryReader.LinearSearchReference(
          this.RowSize,
          this.ParentOffset,
          typeDefRowId,
          this.IsTypeDefTableRowRefSizeSmall
        );
      return (uint)(rowNumber + 1);
    }
    internal uint GetEventListStartFor(
      uint rowId
    )
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint eventList = this.EventMapTableMemoryReader.PeekReference(rowOffset + this.EventListOffset, this.IsEventRefSizeSmall);
      return eventList;
    }
  }

  internal unsafe struct EventPtrTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsEventTableRowRefSizeSmall;
    internal readonly int EventOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader EventPtrTableMemoryReader;
    internal EventPtrTableReader(
      uint numberOfRows,
      int eventTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsEventTableRowRefSizeSmall = eventTableRowRefSize == 2;
      this.EventOffset = 0;
      this.RowSize = this.EventOffset + eventTableRowRefSize;
      this.EventPtrTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal EventPtrRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint @event = this.EventPtrTableMemoryReader.PeekReference(rowOffset + this.EventOffset, this.IsEventTableRowRefSizeSmall);
        EventPtrRow eventPtrRow = new EventPtrRow(@event);
        return eventPtrRow;
      }
    }
#endif
    internal uint GetEventFor(
      uint rowId
    )
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint @event = this.EventPtrTableMemoryReader.PeekReference(rowOffset + this.EventOffset, this.IsEventTableRowRefSizeSmall);
      return @event;
    }
  }

  internal unsafe struct EventTableReader {
    internal uint NumberOfRows;
    internal readonly bool IsTypeDefOrRefRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int EventTypeOffset;
    internal int RowSize;
    internal MemoryReader EventTableMemoryReader;
    internal EventTableReader(
      uint numberOfRows,
      int typeDefOrRefRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.NameOffset = this.FlagsOffset + sizeof(UInt16);
      this.EventTypeOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.EventTypeOffset + typeDefOrRefRefSize;
      this.EventTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal EventRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        EventFlags flags = (EventFlags)this.EventTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        uint name = this.EventTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint eventType = this.EventTableMemoryReader.PeekReference(rowOffset + this.EventTypeOffset, this.IsTypeDefOrRefRefSizeSmall);
        eventType = TypeDefOrRefTag.ConvertToToken(eventType);
        EventRow eventRow = new EventRow(flags, name, eventType);
        return eventRow;
      }
    }
    internal uint GetEventType(
      uint rowId
    )
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint eventType = this.EventTableMemoryReader.PeekReference(rowOffset + this.EventTypeOffset, this.IsTypeDefOrRefRefSizeSmall);
      eventType = TypeDefOrRefTag.ConvertToToken(eventType);
      return eventType;
    }
  }

  internal unsafe struct PropertyMapTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsTypeDefTableRowRefSizeSmall;
    internal readonly bool IsPropertyRefSizeSmall;
    internal readonly int ParentOffset;
    internal readonly int PropertyListOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader PropertyMapTableMemoryReader;
    internal PropertyMapTableReader(
      uint numberOfRows,
      int typeDefTableRowRefSize,
      int propertyRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
      this.IsPropertyRefSizeSmall = propertyRefSize == 2;
      this.ParentOffset = 0;
      this.PropertyListOffset = this.ParentOffset + typeDefTableRowRefSize;
      this.RowSize = this.PropertyListOffset + propertyRefSize;
      this.PropertyMapTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal PropertyMapRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint parent = this.PropertyMapTableMemoryReader.PeekReference(rowOffset + this.ParentOffset, this.IsTypeDefTableRowRefSizeSmall);
        uint propertyList = this.PropertyMapTableMemoryReader.PeekReference(rowOffset + this.PropertyListOffset, this.IsPropertyRefSizeSmall);
        PropertyMapRow propertyMapRow = new PropertyMapRow(parent, propertyList);
        return propertyMapRow;
      }
    }
#endif
    internal uint FindPropertyMapRowIdFor(
      uint typeDefRowId
    ) {
      //  We do a linear scan here because we dont have these tables sorted
      int rowNumber =
        this.PropertyMapTableMemoryReader.LinearSearchReference(
          this.RowSize,
          this.ParentOffset,
          typeDefRowId,
          this.IsTypeDefTableRowRefSizeSmall
        );
      return (uint)(rowNumber + 1);
    }
    internal uint GetPropertyListStartFor(
      uint rowId
    )
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint propertyList = this.PropertyMapTableMemoryReader.PeekReference(rowOffset + this.PropertyListOffset, this.IsPropertyRefSizeSmall);
      return propertyList;
    }
  }

  internal unsafe struct PropertyPtrTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsPropertyTableRowRefSizeSmall;
    internal readonly int PropertyOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader PropertyPtrTableMemoryReader;
    internal PropertyPtrTableReader(
      uint numberOfRows,
      int propertyTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsPropertyTableRowRefSizeSmall = propertyTableRowRefSize == 2;
      this.PropertyOffset = 0;
      this.RowSize = this.PropertyOffset + propertyTableRowRefSize;
      this.PropertyPtrTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal PropertyPtrRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint property = this.PropertyPtrTableMemoryReader.PeekReference(rowOffset + this.PropertyOffset, this.IsPropertyTableRowRefSizeSmall);
        PropertyPtrRow propertyPtrRow = new PropertyPtrRow(property);
        return propertyPtrRow;
      }
    }
#endif
    internal uint GetPropertyFor(
      uint rowId
    )
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint property = this.PropertyPtrTableMemoryReader.PeekReference(rowOffset + this.PropertyOffset, this.IsPropertyTableRowRefSizeSmall);
      return property;
    }
  }

  internal unsafe struct PropertyTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int SignatureOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader PropertyTableMemoryReader;
    internal PropertyTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.NameOffset = this.FlagsOffset + sizeof(UInt16);
      this.SignatureOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.SignatureOffset + blobHeapRefSize;
      this.PropertyTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal PropertyRow this[uint rowId] //  This is 1 based...
    {
      get
        //^ requires rowId <= this.NumberOfRows;
      {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        PropertyFlags flags = (PropertyFlags)this.PropertyTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        uint name = this.PropertyTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint signature = this.PropertyTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
        PropertyRow propertyRow = new PropertyRow(flags, name, signature);
        return propertyRow;
      }
    }
    internal uint GetSignature(uint rowId)
      //^ requires rowId <= this.NumberOfRows;
    {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint signature = this.PropertyTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
      return signature;
    }
  }

  internal unsafe struct MethodSemanticsTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsMethodTableRowRefSizeSmall;
    internal readonly bool IsHasSemanticRefSizeSmall;
    internal readonly int SemanticsFlagOffset;
    internal readonly int MethodOffset;
    internal readonly int AssociationOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader MethodSemanticsTableMemoryReader;
    internal MethodSemanticsTableReader(
      uint numberOfRows,
      int methodTableRowRefSize,
      int hasSemanticRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsMethodTableRowRefSizeSmall = methodTableRowRefSize == 2;
      this.IsHasSemanticRefSizeSmall = hasSemanticRefSize == 2;
      this.SemanticsFlagOffset = 0;
      this.MethodOffset = this.SemanticsFlagOffset + sizeof(UInt16);
      this.AssociationOffset = this.MethodOffset + methodTableRowRefSize;
      this.RowSize = this.AssociationOffset + hasSemanticRefSize;
      this.MethodSemanticsTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal MethodSemanticsRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        MethodSemanticsFlags semanticsFlag = (MethodSemanticsFlags)this.MethodSemanticsTableMemoryReader.PeekUInt16(rowOffset + this.SemanticsFlagOffset);
        uint method = this.MethodSemanticsTableMemoryReader.PeekReference(rowOffset + this.MethodOffset, this.IsMethodTableRowRefSizeSmall);
        uint association = this.MethodSemanticsTableMemoryReader.PeekReference(rowOffset + this.AssociationOffset, this.IsHasSemanticRefSizeSmall);
        association = HasSemanticsTag.ConvertToToken(association);
        MethodSemanticsRow methodSemanticsRow = new MethodSemanticsRow(semanticsFlag, method, association);
        return methodSemanticsRow;
      }
    }
    //  returns rowID
    internal uint FindSemanticMethodsForEvent(
      uint eventRowId,
      out ushort methodCount
    ) {
      methodCount = 0;
      uint searchCodedTag = HasSemanticsTag.ConvertEventRowIdToTag(eventRowId);
      return this.BinarySearchTag(searchCodedTag, ref methodCount);
    }
    internal uint FindSemanticMethodsForProperty(
      uint propertyRowId,
      out ushort methodCount
    ) {
      methodCount = 0;
      uint searchCodedTag = HasSemanticsTag.ConvertPropertyRowIdToTag(propertyRowId);
      return this.BinarySearchTag(searchCodedTag, ref methodCount);
    }
    private uint BinarySearchTag(uint searchCodedTag, ref ushort methodCount) {
      int foundRowNumber =
        this.MethodSemanticsTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.AssociationOffset,
          searchCodedTag,
          this.IsHasSemanticRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.MethodSemanticsTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.AssociationOffset, this.IsHasSemanticRefSizeSmall) == searchCodedTag
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.MethodSemanticsTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.AssociationOffset, this.IsHasSemanticRefSizeSmall) == searchCodedTag
      ) {
        endRowNumber++;
      }
      methodCount = (ushort)(endRowNumber - startRowNumber + 1);
      return (uint)(startRowNumber + 1);
    }

  }

  internal unsafe struct MethodImplTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsTypeDefTableRowRefSizeSmall;
    internal readonly bool IsMethodDefOrRefRefSizeSmall;
    internal readonly int ClassOffset;
    internal readonly int MethodBodyOffset;
    internal readonly int MethodDeclarationOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader MethodImplTableMemoryReader;
    internal MethodImplTableReader(
      uint numberOfRows,
      int typeDefTableRowRefSize,
      int methodDefOrRefRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
      this.IsMethodDefOrRefRefSizeSmall = methodDefOrRefRefSize == 2;
      this.ClassOffset = 0;
      this.MethodBodyOffset = this.ClassOffset + typeDefTableRowRefSize;
      this.MethodDeclarationOffset = this.MethodBodyOffset + methodDefOrRefRefSize;
      this.RowSize = this.MethodDeclarationOffset + methodDefOrRefRefSize;
      this.MethodImplTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal MethodImplRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint @class = this.MethodImplTableMemoryReader.PeekReference(rowOffset + this.ClassOffset, this.IsTypeDefTableRowRefSizeSmall);
        uint methodBody = this.MethodImplTableMemoryReader.PeekReference(rowOffset + this.MethodBodyOffset, this.IsMethodDefOrRefRefSizeSmall);
        methodBody = MethodDefOrRefTag.ConvertToToken(methodBody);
        uint methodDeclaration = this.MethodImplTableMemoryReader.PeekReference(rowOffset + this.MethodDeclarationOffset, this.IsMethodDefOrRefRefSizeSmall);
        methodDeclaration = MethodDefOrRefTag.ConvertToToken(methodDeclaration);
        MethodImplRow methodImplRow = new MethodImplRow(@class, methodBody, methodDeclaration);
        return methodImplRow;
      }
    }
    internal uint FindMethodsImplForClass(
      uint typeDefRowId,
      out ushort methodImplCount
    ) {
      methodImplCount = 0;
      int foundRowNumber =
        this.MethodImplTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.ClassOffset,
          typeDefRowId,
          this.IsTypeDefTableRowRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.MethodImplTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.ClassOffset, this.IsTypeDefTableRowRefSizeSmall) == typeDefRowId
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.MethodImplTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.ClassOffset, this.IsTypeDefTableRowRefSizeSmall) == typeDefRowId
      ) {
        endRowNumber++;
      }
      methodImplCount = (ushort)(endRowNumber - startRowNumber + 1);
      return (uint)(startRowNumber + 1);
    }
  }

  internal unsafe struct ModuleRefTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int NameOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ModuleRefTableMemoryReader;
    internal ModuleRefTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.NameOffset = 0;
      this.RowSize = this.NameOffset + stringHeapRefSize;
      this.ModuleRefTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ModuleRefRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint name = this.ModuleRefTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        ModuleRefRow moduleRefRow = new ModuleRefRow(name);
        return moduleRefRow;
      }
    }
  }

  internal unsafe struct TypeSpecTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int SignatureOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader TypeSpecTableMemoryReader;
    internal TypeSpecTableReader(
      uint numberOfRows,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.SignatureOffset = 0;
      this.RowSize = this.SignatureOffset + blobHeapRefSize;
      this.TypeSpecTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal TypeSpecRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint signature = this.TypeSpecTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
        TypeSpecRow typeSpecRow = new TypeSpecRow(signature);
        return typeSpecRow;
      }
    }
#endif
    internal uint GetSignature(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint signature = this.TypeSpecTableMemoryReader.PeekReference(rowOffset + this.SignatureOffset, this.IsBlobHeapRefSizeSmall);
      return signature;
    }
  }

  internal unsafe struct ImplMapTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsModuleRefTableRowRefSizeSmall;
    internal readonly bool IsMemberForwardRowRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int MemberForwardedOffset;
    internal readonly int ImportNameOffset;
    internal readonly int ImportScopeOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ImplMapTableMemoryReader;
    internal ImplMapTableReader(
      uint numberOfRows,
      int moduleRefTableRowRefSize,
      int memberForwardedRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsModuleRefTableRowRefSizeSmall = moduleRefTableRowRefSize == 2;
      this.IsMemberForwardRowRefSizeSmall = memberForwardedRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.MemberForwardedOffset = this.FlagsOffset + sizeof(UInt16);
      this.ImportNameOffset = this.MemberForwardedOffset + memberForwardedRefSize;
      this.ImportScopeOffset = this.ImportNameOffset + stringHeapRefSize;
      this.RowSize = this.ImportScopeOffset + moduleRefTableRowRefSize;
      this.ImplMapTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ImplMapRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        PInvokeMapFlags pInvokeMapFlags = (PInvokeMapFlags)this.ImplMapTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        uint memberForwarded = this.ImplMapTableMemoryReader.PeekReference(rowOffset + this.MemberForwardedOffset, this.IsMemberForwardRowRefSizeSmall);
        memberForwarded = MemberForwardedTag.ConvertToToken(memberForwarded);
        uint importName = this.ImplMapTableMemoryReader.PeekReference(rowOffset + this.ImportNameOffset, this.IsStringHeapRefSizeSmall);
        uint importScope = this.ImplMapTableMemoryReader.PeekReference(rowOffset + this.ImportScopeOffset, this.IsModuleRefTableRowRefSizeSmall);
        ImplMapRow implMapRow = new ImplMapRow(pInvokeMapFlags, memberForwarded, importName, importScope);
        return implMapRow;
      }
    }
    internal uint FindImplForMethod(
      uint methodRowId
    ) {
      uint searchCodedTag = MemberForwardedTag.ConvertMethodDefRowIdToTag(methodRowId);
      return this.BinarySearchTag(searchCodedTag);
    }
#if false
    internal uint FindImplForField(
      uint fieldRowId
    ) {
      uint searchCodedTag = MemberForwardedTag.ConvertFieldDefRowIdToTag(fieldRowId);
      return this.BinarySearchTag(searchCodedTag);
    }
#endif
    private uint BinarySearchTag(uint searchCodedTag) {
      int foundRowNumber =
        this.ImplMapTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.MemberForwardedOffset,
          searchCodedTag,
          this.IsMemberForwardRowRefSizeSmall
        );
      return (uint)(foundRowNumber + 1);
    }
  }

  internal unsafe struct FieldRVATableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsFieldTableRowRefSizeSmall;
    internal readonly int RVAOffset;
    internal readonly int FieldOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader FieldRVATableMemoryReader;
    internal FieldRVATableReader(
      uint numberOfRows,
      int fieldTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsFieldTableRowRefSizeSmall = fieldTableRowRefSize == 2;
      this.RVAOffset = 0;
      this.FieldOffset = this.RVAOffset + sizeof(UInt32);
      this.RowSize = this.FieldOffset + fieldTableRowRefSize;
      this.FieldRVATableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal FieldRVARow this[uint rowId]  //  This is 1 based
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        int rva = this.FieldRVATableMemoryReader.PeekInt32(rowOffset + this.RVAOffset);
        uint field = this.FieldRVATableMemoryReader.PeekReference(rowOffset + this.FieldOffset, this.IsFieldTableRowRefSizeSmall);
        FieldRVARow fieldRVARow = new FieldRVARow(rva, field);
        return fieldRVARow;
      }
    }
#endif
    internal int GetFieldRVA(
      uint fieldDefRowId
    ) {
      int foundRowNumber =
        this.FieldRVATableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.FieldOffset,
          fieldDefRowId,
          this.IsFieldTableRowRefSizeSmall
        );
      if (foundRowNumber == -1)
        return -1;
      int rowOffset = foundRowNumber * this.RowSize;
      int rva = this.FieldRVATableMemoryReader.PeekInt32(rowOffset + this.RVAOffset);
      return rva;
    }
    internal int GetNextRVA(
      int rva
    ) {
      int nextRVA = int.MaxValue;
      int endOffset = (int)this.NumberOfRows * this.RowSize;
      for (int iterOffset = this.RVAOffset; iterOffset < endOffset; iterOffset += this.RowSize) {
        int currentRVA = this.FieldRVATableMemoryReader.PeekInt32(iterOffset);
        if (currentRVA > rva && currentRVA < nextRVA) {
          nextRVA = currentRVA;
        }
      }
      return nextRVA == int.MaxValue ? -1 : nextRVA;
    }
  }

  internal unsafe struct EnCLogTableReader {
    internal readonly uint NumberOfRows;
    internal readonly int TokenOffset;
    internal readonly int FuncCodeOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader EnCLogTableMemoryReader;
    internal EnCLogTableReader(
      uint numberOfRows,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.TokenOffset = 0;
      this.FuncCodeOffset = this.TokenOffset + sizeof(UInt32);
      this.RowSize = this.FuncCodeOffset + sizeof(UInt32);
      this.EnCLogTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal EnCLogRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint token = this.EnCLogTableMemoryReader.PeekUInt32(rowOffset + this.TokenOffset);
        uint funcCode = this.EnCLogTableMemoryReader.PeekUInt32(rowOffset + this.FuncCodeOffset);
        EnCLogRow encLogRow = new EnCLogRow(token, funcCode);
        return encLogRow;
      }
    }
#endif
  }

  internal unsafe struct EnCMapTableReader {
    internal readonly uint NumberOfRows;
    internal readonly int TokenOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader EnCMapTableMemoryReader;
    internal EnCMapTableReader(
      uint numberOfRows,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.TokenOffset = 0;
      this.RowSize = this.TokenOffset + sizeof(UInt32);
      this.EnCMapTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal EnCMapRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint token = this.EnCMapTableMemoryReader.PeekUInt32(rowOffset + this.TokenOffset);
        EnCMapRow encMapRow = new EnCMapRow(token);
        return encMapRow;
      }
    }
#endif
  }

  internal unsafe struct AssemblyTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int HashAlgIdOffset;
    internal readonly int MajorVersionOffset;
    internal readonly int MinorVersionOffset;
    internal readonly int BuildNumberOffset;
    internal readonly int RevisionNumberOffset;
    internal readonly int FlagsOffset;
    internal readonly int PublicKeyOffset;
    internal readonly int NameOffset;
    internal readonly int CultureOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader AssemblyTableMemoryReader;
    internal AssemblyTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.HashAlgIdOffset = 0;
      this.MajorVersionOffset = this.HashAlgIdOffset + sizeof(UInt32);
      this.MinorVersionOffset = this.MajorVersionOffset + sizeof(UInt16);
      this.BuildNumberOffset = this.MinorVersionOffset + sizeof(UInt16);
      this.RevisionNumberOffset = this.BuildNumberOffset + sizeof(UInt16);
      this.FlagsOffset = this.RevisionNumberOffset + sizeof(UInt16);
      this.PublicKeyOffset = this.FlagsOffset + sizeof(UInt32);
      this.NameOffset = this.PublicKeyOffset + blobHeapRefSize;
      this.CultureOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.CultureOffset + stringHeapRefSize;
      this.AssemblyTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal AssemblyRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint hashAlgId = this.AssemblyTableMemoryReader.PeekUInt32(rowOffset + this.HashAlgIdOffset);
        ushort majorVersion = this.AssemblyTableMemoryReader.PeekUInt16(rowOffset + this.MajorVersionOffset);
        ushort minorVersion = this.AssemblyTableMemoryReader.PeekUInt16(rowOffset + this.MinorVersionOffset);
        ushort buildNumber = this.AssemblyTableMemoryReader.PeekUInt16(rowOffset + this.BuildNumberOffset);
        ushort revisionNumber = this.AssemblyTableMemoryReader.PeekUInt16(rowOffset + this.RevisionNumberOffset);
        AssemblyFlags flags = (AssemblyFlags)this.AssemblyTableMemoryReader.PeekUInt32(rowOffset + this.FlagsOffset);
        uint publicKey = this.AssemblyTableMemoryReader.PeekReference(rowOffset + this.PublicKeyOffset, this.IsBlobHeapRefSizeSmall);
        uint name = this.AssemblyTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint culture = this.AssemblyTableMemoryReader.PeekReference(rowOffset + this.CultureOffset, this.IsStringHeapRefSizeSmall);
        AssemblyRow assemblyRow = new AssemblyRow(hashAlgId, majorVersion, minorVersion, buildNumber, revisionNumber, flags, publicKey, name, culture);
        return assemblyRow;
      }
    }
  }

  internal unsafe struct AssemblyProcessorTableReader {
    internal readonly uint NumberOfRows;
    internal readonly int ProcessorOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader AssemblyProcessorTableMemoryReader;
    internal AssemblyProcessorTableReader(
      uint numberOfRows,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.ProcessorOffset = 0;
      this.RowSize = this.ProcessorOffset + sizeof(UInt32);
      this.AssemblyProcessorTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal AssemblyProcessorRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint processor = this.AssemblyProcessorTableMemoryReader.PeekUInt32(rowOffset + this.ProcessorOffset);
        AssemblyProcessorRow assemblyProcessorRow = new AssemblyProcessorRow(processor);
        return assemblyProcessorRow;
      }
    }
#endif
  }

  internal unsafe struct AssemblyOSTableReader {
    internal readonly uint NumberOfRows;
    internal readonly int OSPlatformIdOffset;
    internal readonly int OSMajorVersionIdOffset;
    internal readonly int OSMinorVersionIdOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader AssemblyOSTableMemoryReader;
    internal AssemblyOSTableReader(
      uint numberOfRows,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.OSPlatformIdOffset = 0;
      this.OSMajorVersionIdOffset = this.OSPlatformIdOffset + sizeof(UInt32);
      this.OSMinorVersionIdOffset = this.OSMajorVersionIdOffset + sizeof(UInt32);
      this.RowSize = this.OSMinorVersionIdOffset + sizeof(UInt32);
      this.AssemblyOSTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal AssemblyOSRow this[uint rowId]   //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint osPlatformId = this.AssemblyOSTableMemoryReader.PeekUInt32(rowOffset + this.OSPlatformIdOffset);
        uint osMajorVersionId = this.AssemblyOSTableMemoryReader.PeekUInt32(rowOffset + this.OSMajorVersionIdOffset);
        uint osMinorVersionId = this.AssemblyOSTableMemoryReader.PeekUInt32(rowOffset + this.OSMinorVersionIdOffset);
        AssemblyOSRow assemblyOSRow = new AssemblyOSRow(osPlatformId, osMajorVersionId, osMinorVersionId);
        return assemblyOSRow;
      }
    }
#endif
  }

  internal unsafe struct AssemblyRefTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int MajorVersionOffset;
    internal readonly int MinorVersionOffset;
    internal readonly int BuildNumberOffset;
    internal readonly int RevisionNumberOffset;
    internal readonly int FlagsOffset;
    internal readonly int PublicKeyOrTokenOffset;
    internal readonly int NameOffset;
    internal readonly int CultureOffset;
    internal readonly int HashValueOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader AssemblyRefTableMemoryReader;
    internal AssemblyRefTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.MajorVersionOffset = 0;
      this.MinorVersionOffset = this.MajorVersionOffset + sizeof(UInt16);
      this.BuildNumberOffset = this.MinorVersionOffset + sizeof(UInt16);
      this.RevisionNumberOffset = this.BuildNumberOffset + sizeof(UInt16);
      this.FlagsOffset = this.RevisionNumberOffset + sizeof(UInt16);
      this.PublicKeyOrTokenOffset = this.FlagsOffset + sizeof(UInt32);
      this.NameOffset = this.PublicKeyOrTokenOffset + blobHeapRefSize;
      this.CultureOffset = this.NameOffset + stringHeapRefSize;
      this.HashValueOffset = this.CultureOffset + stringHeapRefSize;
      this.RowSize = this.HashValueOffset + blobHeapRefSize;
      this.AssemblyRefTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal AssemblyRefRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        ushort majorVersion = this.AssemblyRefTableMemoryReader.PeekUInt16(rowOffset + this.MajorVersionOffset);
        ushort minorVersion = this.AssemblyRefTableMemoryReader.PeekUInt16(rowOffset + this.MinorVersionOffset);
        ushort buildNumber = this.AssemblyRefTableMemoryReader.PeekUInt16(rowOffset + this.BuildNumberOffset);
        ushort revisionNumber = this.AssemblyRefTableMemoryReader.PeekUInt16(rowOffset + this.RevisionNumberOffset);
        AssemblyFlags flags = (AssemblyFlags)this.AssemblyRefTableMemoryReader.PeekUInt32(rowOffset + this.FlagsOffset);
        uint publicKeyOrToken = this.AssemblyRefTableMemoryReader.PeekReference(rowOffset + this.PublicKeyOrTokenOffset, this.IsBlobHeapRefSizeSmall);
        uint name = this.AssemblyRefTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint culture = this.AssemblyRefTableMemoryReader.PeekReference(rowOffset + this.CultureOffset, this.IsStringHeapRefSizeSmall);
        uint hashValue = this.AssemblyRefTableMemoryReader.PeekReference(rowOffset + this.HashValueOffset, this.IsBlobHeapRefSizeSmall);
        AssemblyRefRow assemblyRefRow = new AssemblyRefRow(majorVersion, minorVersion, buildNumber, revisionNumber, flags, publicKeyOrToken, name, culture, hashValue);
        return assemblyRefRow;
      }
    }
  }

  internal unsafe struct AssemblyRefProcessorTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsAssemblyRefTableRowSizeSmall;
    internal readonly int ProcessorOffset;
    internal readonly int AssemblyRefOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader AssemblyRefProcessorTableMemoryReader;
    internal AssemblyRefProcessorTableReader(
      uint numberOfRows,
      int assembyRefTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsAssemblyRefTableRowSizeSmall = assembyRefTableRowRefSize == 2;
      this.ProcessorOffset = 0;
      this.AssemblyRefOffset = this.ProcessorOffset + sizeof(UInt32);
      this.RowSize = this.AssemblyRefOffset + assembyRefTableRowRefSize;
      this.AssemblyRefProcessorTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal AssemblyRefProcessorRow this[uint rowId]   //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint processor = this.AssemblyRefProcessorTableMemoryReader.PeekUInt32(rowOffset + this.ProcessorOffset);
        uint assemblyRef = this.AssemblyRefProcessorTableMemoryReader.PeekReference(rowOffset + this.AssemblyRefOffset, this.IsAssemblyRefTableRowSizeSmall);
        AssemblyRefProcessorRow assemblyRefProcessorRow = new AssemblyRefProcessorRow(processor, assemblyRef);
        return assemblyRefProcessorRow;
      }
    }
#endif
  }

  internal unsafe struct AssemblyRefOSTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsAssemblyRefTableRowRefSizeSmall;
    internal readonly int OSPlatformIdOffset;
    internal readonly int OSMajorVersionIdOffset;
    internal readonly int OSMinorVersionIdOffset;
    internal readonly int AssemblyRefOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader AssemblyRefOSTableMemoryReader;
    internal AssemblyRefOSTableReader(
      uint numberOfRows,
      int assembyRefTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsAssemblyRefTableRowRefSizeSmall = assembyRefTableRowRefSize == 2;
      this.OSPlatformIdOffset = 0;
      this.OSMajorVersionIdOffset = this.OSPlatformIdOffset + sizeof(UInt32);
      this.OSMinorVersionIdOffset = this.OSMajorVersionIdOffset + sizeof(UInt32);
      this.AssemblyRefOffset = this.OSMinorVersionIdOffset + sizeof(UInt32);
      this.RowSize = this.AssemblyRefOffset + assembyRefTableRowRefSize;
      this.AssemblyRefOSTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal AssemblyRefOSRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint osPlatformId = this.AssemblyRefOSTableMemoryReader.PeekUInt32(rowOffset + this.OSPlatformIdOffset);
        uint osMajorVersionId = this.AssemblyRefOSTableMemoryReader.PeekUInt32(rowOffset + this.OSMajorVersionIdOffset);
        uint osMinorVersionId = this.AssemblyRefOSTableMemoryReader.PeekUInt32(rowOffset + this.OSMinorVersionIdOffset);
        uint assemblyRef = this.AssemblyRefOSTableMemoryReader.PeekReference(rowOffset + this.AssemblyRefOffset, this.IsAssemblyRefTableRowRefSizeSmall);
        AssemblyRefOSRow assemblyRefOSRow = new AssemblyRefOSRow(osPlatformId, osMajorVersionId, osMinorVersionId, assemblyRef);
        return assemblyRefOSRow;
      }
    }
#endif
  }

  internal unsafe struct FileTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int HashValueOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader FileTableMemoryReader;
    internal FileTableReader(
      uint numberOfRows,
      int stringHeapRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.NameOffset = this.FlagsOffset + sizeof(UInt32);
      this.HashValueOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.HashValueOffset + blobHeapRefSize;
      this.FileTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal FileRow this[uint rowId]  //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        FileFlags flags = (FileFlags)this.FileTableMemoryReader.PeekUInt32(rowOffset + this.FlagsOffset);
        uint name = this.FileTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint hashValue = this.FileTableMemoryReader.PeekReference(rowOffset + this.HashValueOffset, this.IsBlobHeapRefSizeSmall);
        FileRow fileRow = new FileRow(flags, name, hashValue);
        return fileRow;
      }
    }
    internal uint GetHashValue(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint hashValue = this.FileTableMemoryReader.PeekReference(rowOffset + this.HashValueOffset, this.IsBlobHeapRefSizeSmall);
      return hashValue;
    }
  }

  internal unsafe struct ExportedTypeTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsImplementationRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int FlagsOffset;
    internal readonly int TypeDefIdOffset;
    internal readonly int TypeNameOffset;
    internal readonly int TypeNamespaceOffset;
    internal readonly int ImplementationOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ExportedTypeTableMemoryReader;
    internal ExportedTypeTableReader(
      uint numberOfRows,
      int implementationRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsImplementationRefSizeSmall = implementationRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.FlagsOffset = 0;
      this.TypeDefIdOffset = this.FlagsOffset + sizeof(UInt32);
      this.TypeNameOffset = this.TypeDefIdOffset + sizeof(UInt32);
      this.TypeNamespaceOffset = this.TypeNameOffset + stringHeapRefSize;
      this.ImplementationOffset = this.TypeNamespaceOffset + stringHeapRefSize;
      this.RowSize = this.ImplementationOffset + implementationRefSize;
      this.ExportedTypeTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ExportedTypeRow this[uint rowId] {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        TypeDefFlags typeDefFlags = (TypeDefFlags)this.ExportedTypeTableMemoryReader.PeekUInt32(rowOffset + this.FlagsOffset);
        uint typeDefId = this.ExportedTypeTableMemoryReader.PeekUInt32(rowOffset + this.TypeDefIdOffset);
        uint typeName = this.ExportedTypeTableMemoryReader.PeekReference(rowOffset + this.TypeNameOffset, this.IsStringHeapRefSizeSmall);
        uint typeNamespace = this.ExportedTypeTableMemoryReader.PeekReference(rowOffset + this.TypeNamespaceOffset, this.IsStringHeapRefSizeSmall);
        uint implementation = this.ExportedTypeTableMemoryReader.PeekReference(rowOffset + this.ImplementationOffset, this.IsImplementationRefSizeSmall);
        implementation = ImplementationTag.ConvertToToken(implementation);
        ExportedTypeRow exportedTypeRow = new ExportedTypeRow(typeDefFlags, typeDefId, typeName, typeNamespace, implementation);
        return exportedTypeRow;
      }
    }

    internal uint GetNamespace(uint rowId) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint typeNamespace = this.ExportedTypeTableMemoryReader.PeekReference(rowOffset + this.TypeNamespaceOffset, this.IsStringHeapRefSizeSmall);
      return typeNamespace;
    }
  }

  internal unsafe struct ManifestResourceTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsImplementationRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int OffsetOffset;
    internal readonly int FlagsOffset;
    internal readonly int NameOffset;
    internal readonly int ImplementationOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader ManifestResourceTableMemoryReader;
    internal ManifestResourceTableReader(
      uint numberOfRows,
      int implementationRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsImplementationRefSizeSmall = implementationRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.OffsetOffset = 0;
      this.FlagsOffset = this.OffsetOffset + sizeof(UInt32);
      this.NameOffset = this.FlagsOffset + sizeof(UInt32);
      this.ImplementationOffset = this.NameOffset + stringHeapRefSize;
      this.RowSize = this.ImplementationOffset + implementationRefSize;
      this.ManifestResourceTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal ManifestResourceRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint offset = this.ManifestResourceTableMemoryReader.PeekUInt32(rowOffset + this.OffsetOffset);
        ManifestResourceFlags flags = (ManifestResourceFlags)this.ManifestResourceTableMemoryReader.PeekUInt32(rowOffset + this.FlagsOffset);
        uint name = this.ManifestResourceTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        uint implementation = this.ManifestResourceTableMemoryReader.PeekReference(rowOffset + this.ImplementationOffset, this.IsImplementationRefSizeSmall);
        implementation = ImplementationTag.ConvertToToken(implementation);
        ManifestResourceRow manifestResourceRow = new ManifestResourceRow(offset, flags, name, implementation);
        return manifestResourceRow;
      }
    }
    internal uint GetImplementation(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint implementation = this.ManifestResourceTableMemoryReader.PeekReference(rowOffset + this.ImplementationOffset, this.IsImplementationRefSizeSmall);
      implementation = ImplementationTag.ConvertToToken(implementation);
      return implementation;
    }
    internal uint GetOffset(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint offset = this.ManifestResourceTableMemoryReader.PeekUInt32(rowOffset + this.OffsetOffset);
      return offset;
    }
  }

  internal unsafe struct NestedClassTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsTypeDefTableRowRefSizeSmall;
    internal readonly int NestedClassOffset;
    internal readonly int EnclosingClassOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader NestedClassTableMemoryReader;
    internal NestedClassTableReader(
      uint numberOfRows,
      int typeDefTableRowRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
      this.NestedClassOffset = 0;
      this.EnclosingClassOffset = this.NestedClassOffset + typeDefTableRowRefSize;
      this.RowSize = this.EnclosingClassOffset + typeDefTableRowRefSize;
      this.NestedClassTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal NestedClassRow this[uint rowId] { //  This is 1 based...
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint nestedClass = this.NestedClassTableMemoryReader.PeekReference(rowOffset + this.NestedClassOffset, this.IsTypeDefTableRowRefSizeSmall);
        uint enclosingClass = this.NestedClassTableMemoryReader.PeekReference(rowOffset + this.EnclosingClassOffset, this.IsTypeDefTableRowRefSizeSmall);
        NestedClassRow nestedClassRow = new NestedClassRow(nestedClass, enclosingClass);
        return nestedClassRow;
      }
    }
    internal uint FindParentTypeDefRowId(
      uint nestedTypeRowId
    ) {
      int rowNumber =
        this.NestedClassTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.NestedClassOffset,
          nestedTypeRowId,
          this.IsTypeDefTableRowRefSizeSmall
        );
      if (rowNumber == -1)
        return 0;
      return this.NestedClassTableMemoryReader.PeekReference(rowNumber * this.RowSize + this.EnclosingClassOffset, this.IsTypeDefTableRowRefSizeSmall);
    }
  }

  internal unsafe struct GenericParamTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsTypeOrMethodDefRefSizeSmall;
    internal readonly bool IsStringHeapRefSizeSmall;
    internal readonly int NumberOffset;
    internal readonly int FlagsOffset;
    internal readonly int OwnerOffset;
    internal readonly int NameOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader GenericParamTableMemoryReader;
    internal GenericParamTableReader(
      uint numberOfRows,
      int typeOrMethodDefRefSize,
      int stringHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsTypeOrMethodDefRefSizeSmall = typeOrMethodDefRefSize == 2;
      this.IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
      this.NumberOffset = 0;
      this.FlagsOffset = this.NumberOffset + sizeof(UInt16);
      this.OwnerOffset = this.FlagsOffset + sizeof(UInt16);
      this.NameOffset = this.OwnerOffset + typeOrMethodDefRefSize;
      this.RowSize = this.NameOffset + stringHeapRefSize;
      this.GenericParamTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal GenericParamRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        ushort number = this.GenericParamTableMemoryReader.PeekUInt16(rowOffset + this.NumberOffset);
        GenericParamFlags flags = (GenericParamFlags)this.GenericParamTableMemoryReader.PeekUInt16(rowOffset + this.FlagsOffset);
        uint owner = this.GenericParamTableMemoryReader.PeekReference(rowOffset + this.OwnerOffset, this.IsTypeOrMethodDefRefSizeSmall);
        owner = TypeOrMethodDefTag.ConvertToToken(owner);
        uint name = this.GenericParamTableMemoryReader.PeekReference(rowOffset + this.NameOffset, this.IsStringHeapRefSizeSmall);
        GenericParamRow genericParamRow = new GenericParamRow(number, flags, owner, name);
        return genericParamRow;
      }
    }
    //  returns rowID
    internal uint FindGenericParametersForType(
      uint typeDefRowId,
      out ushort genericParamCount
    ) {
      genericParamCount = 0;
      uint searchCodedTag = TypeOrMethodDefTag.ConvertTypeDefRowIdToTag(typeDefRowId);
      return this.BinarySearchTag(searchCodedTag, ref genericParamCount);
    }
    internal uint FindGenericParametersForMethod(
      uint typeDefRowId,
      out ushort genericParamCount
    ) {
      genericParamCount = 0;
      uint searchCodedTag = TypeOrMethodDefTag.ConvertMethodDefRowIdToTag(typeDefRowId);
      return this.BinarySearchTag(searchCodedTag, ref genericParamCount);
    }
    private uint BinarySearchTag(uint searchCodedTag, ref ushort genericParamCount) {
      int foundRowNumber =
        this.GenericParamTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.OwnerOffset,
          searchCodedTag,
          this.IsTypeOrMethodDefRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.GenericParamTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.OwnerOffset, this.IsTypeOrMethodDefRefSizeSmall) == searchCodedTag
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.GenericParamTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.OwnerOffset, this.IsTypeOrMethodDefRefSizeSmall) == searchCodedTag
      ) {
        endRowNumber++;
      }
      genericParamCount = (ushort)(endRowNumber - startRowNumber + 1);
      return (uint)(startRowNumber + 1);
    }
  }

  internal unsafe struct MethodSpecTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsMethodDefOrRefRefSizeSmall;
    internal readonly bool IsBlobHeapRefSizeSmall;
    internal readonly int MethodOffset;
    internal readonly int InstantiationOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader MethodSpecTableMemoryReader;
    internal MethodSpecTableReader(
      uint numberOfRows,
      int methodDefOrRefRefSize,
      int blobHeapRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsMethodDefOrRefRefSizeSmall = methodDefOrRefRefSize == 2;
      this.IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
      this.MethodOffset = 0;
      this.InstantiationOffset = this.MethodOffset + methodDefOrRefRefSize;
      this.RowSize = this.InstantiationOffset + blobHeapRefSize;
      this.MethodSpecTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
    internal MethodSpecRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint method = this.MethodSpecTableMemoryReader.PeekReference(rowOffset + this.MethodOffset, this.IsMethodDefOrRefRefSizeSmall);
        method = MethodDefOrRefTag.ConvertToToken(method);
        uint instantiation = this.MethodSpecTableMemoryReader.PeekReference(rowOffset + this.InstantiationOffset, this.IsBlobHeapRefSizeSmall);
        MethodSpecRow methodSpecRow = new MethodSpecRow(method, instantiation);
        return methodSpecRow;
      }
    }
  }

  internal unsafe struct GenericParamConstraintTableReader {
    internal readonly uint NumberOfRows;
    internal readonly bool IsGenericParamTableRowRefSizeSmall;
    internal readonly bool IsTypeDefOrRefRefSizeSmall;
    internal readonly int OwnerOffset;
    internal readonly int ConstraintOffset;
    internal readonly int RowSize;
    internal readonly MemoryReader GenericParamConstraintTableMemoryReader;
    internal GenericParamConstraintTableReader(
      uint numberOfRows,
      int genericParamTableRowRefSize,
      int typeDefOrRefRefSize,
      byte* buffer
    ) {
      this.NumberOfRows = numberOfRows;
      this.IsGenericParamTableRowRefSizeSmall = genericParamTableRowRefSize == 2;
      this.IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
      this.OwnerOffset = 0;
      this.ConstraintOffset = this.OwnerOffset + genericParamTableRowRefSize;
      this.RowSize = this.ConstraintOffset + typeDefOrRefRefSize;
      this.GenericParamConstraintTableMemoryReader = new MemoryReader(buffer, (int)(this.RowSize * numberOfRows));
    }
#if false
    internal GenericParamConstraintRow this[uint rowId] //  This is 1 based...
    {
      get {
        int rowOffset = (int)(rowId - 1) * this.RowSize;
        uint owner = this.GenericParamConstraintTableMemoryReader.PeekReference(rowOffset + this.OwnerOffset, this.IsGenericParamTableRowRefSizeSmall);
        uint constraint = this.GenericParamConstraintTableMemoryReader.PeekReference(rowOffset + this.ConstraintOffset, this.IsTypeDefOrRefRefSizeSmall);
        constraint = TypeDefOrRefTag.ConvertToToken(constraint);
        GenericParamConstraintRow genericParamConstraintRow = new GenericParamConstraintRow(owner, constraint);
        return genericParamConstraintRow;
      }
    }
#endif
    internal uint FindConstraintForGenericParam(
      uint genericParamRowId,
      out uint genericParamConstraintCount
    ) {
      genericParamConstraintCount = 0;
      int foundRowNumber =
        this.GenericParamConstraintTableMemoryReader.BinarySearchReference(
          this.NumberOfRows,
          this.RowSize,
          this.OwnerOffset,
          genericParamRowId,
          this.IsGenericParamTableRowRefSizeSmall
        );
      if (foundRowNumber == -1)
        return 0;
      int startRowNumber = foundRowNumber;
      while (
        startRowNumber > 0
        && this.GenericParamConstraintTableMemoryReader.PeekReference((startRowNumber - 1) * this.RowSize + this.OwnerOffset, this.IsGenericParamTableRowRefSizeSmall) == genericParamRowId
      ) {
        startRowNumber--;
      }
      int endRowNumber = foundRowNumber;
      while (
        endRowNumber + 1 < this.NumberOfRows
        && this.GenericParamConstraintTableMemoryReader.PeekReference((endRowNumber + 1) * this.RowSize + this.OwnerOffset, this.IsGenericParamTableRowRefSizeSmall) == genericParamRowId
      ) {
        endRowNumber++;
      }
      genericParamConstraintCount = (ushort)(endRowNumber - startRowNumber + 1);
      return (uint)(startRowNumber + 1);
    }
    internal uint GetConstraint(
      uint rowId
    ) {
      int rowOffset = (int)(rowId - 1) * this.RowSize;
      uint constraint = this.GenericParamConstraintTableMemoryReader.PeekReference(rowOffset + this.ConstraintOffset, this.IsTypeDefOrRefRefSizeSmall);
      constraint = TypeDefOrRefTag.ConvertToToken(constraint);
      return constraint;
    }
  }
  #endregion Metadata Table Readers

  internal enum ReaderState {
    Initialized,
    PEFile,
    CORModule,
    Metadata,
  }

  internal sealed class PEFileDebugInformation {
    public PEFileDebugInformation() {
      this.Signature = Guid.Empty;
      this.Age = 0;
      this.PdbVersion = string.Empty;
      this.PdbFileName = string.Empty;
    }
    public Guid Signature { get; set; }
    public uint Age { get; set; }
    public string PdbVersion { get; set; }
    public string PdbFileName { get; set; }
  }

  //  For reading the binary file.
  //  Understands the binary file format.
  //  Understands the metadata storage format.
  //  Understands the IL storage format.
  //  Does not understand the object model/Instructions.
  internal unsafe sealed class PEFileReader {
    #region Fields [Inited]
    internal ReaderState ReaderState;
    internal readonly MetadataErrorContainer ErrorContainer;
    internal readonly IBinaryDocumentMemoryBlock BinaryDocumentMemoryBlock;
    #endregion Fields [Inited]

    #region Constructors and Utils

    internal PEFileReader(PeReader moduleReadWriteFactory, IBinaryDocumentMemoryBlock binaryDocumentMemoryBlock, bool snapshot = false) {
      this.ErrorContainer = new MetadataErrorContainer(moduleReadWriteFactory, binaryDocumentMemoryBlock.BinaryDocument);
      this.ReaderState = ReaderState.Initialized;
      this.BinaryDocumentMemoryBlock = binaryDocumentMemoryBlock;
      if (!snapshot) {
        if (!this.ReadPEFileLevelData()) return;
        if (!this.ReadCORModuleLevelData()) return;
      } else {
        this.SectionHeaders = new SectionHeader[0];
        if (!this.ReadMetadataRoot()) return;
      }
      if (!this.ReadMetadataLevelData()) return;
      //  TODO: Add phase for Metadata validation of offsets type row ids etc...
    }

    #endregion Constructors and Utils


    #region Fields and Properties [PEFile]
    COFFFileHeader COFFFileHeader;
    OptionalHeaderStandardFields OptionalHeaderStandardFields;
    internal OptionalHeaderNTAdditionalFields OptionalHeaderNTAdditionalFields;
    OptionalHeaderDirectoryEntries OptionalHeaderDirectoryEntries;
    internal SectionHeader[] SectionHeaders;
    internal MemoryReader Win32ResourceMemoryReader;

    internal PEFileDebugInformation DebugInformation {
      get {
        if (this.debugInformation == null)
          this.debugInformation = this.ReadDebugInformationLocationFromDebugTableDirectoryData();
        return this.debugInformation;
      }
    }
    PEFileDebugInformation debugInformation;

    internal DllCharacteristics DllCharacteristics {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.DllCharacteristics;
      }
    }
    internal uint FileAlignment {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.FileAlignment;
      }
    }
    internal ulong ImageBase {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.ImageBase;
      }
    }
    internal bool IsDll {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return (this.COFFFileHeader.Characteristics & Characteristics.Dll) != 0;
      }
    }
    internal bool IsExe {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return (this.COFFFileHeader.Characteristics & Characteristics.Dll) == 0;
      }
    }
    internal bool IsConsoleApplication {
      get {
        return this.OptionalHeaderNTAdditionalFields.Subsystem == Subsystem.WindowsCUI;
      }
    }
    internal bool IsUnmanaged {
      get {
        return this.OptionalHeaderDirectoryEntries.COR20HeaderTableDirectory.Size == 0;
      }
    }
    internal byte LinkerMajorVersion {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderStandardFields.MajorLinkerVersion;
      }
    }
    internal byte LinkerMinorVersion {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderStandardFields.MinorLinkerVersion;
      }
    }

    internal Machine Machine {
      get {
        return this.COFFFileHeader.Machine;
      }
    }

    internal bool RequiresAmdInstructionSet {
      get {
        return this.COFFFileHeader.Machine == Microsoft.Cci.Machine.AMD64;
      }
    }

    internal bool RequiresStartupStub {
      get {
        return this.OptionalHeaderStandardFields.RVAOfEntryPoint != 0;
      }
    }

    internal bool Requires64Bits {
      get {
        return this.OptionalHeaderStandardFields.PEMagic == PEMagic.PEMagic64
          || this.COFFFileHeader.Machine == Microsoft.Cci.Machine.AMD64
          || this.COFFFileHeader.Machine == Microsoft.Cci.Machine.IA64;
      }
    }

    internal ulong SizeOfHeapCommit {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.SizeOfHeapCommit;
      }
    }
    internal ulong SizeOfHeapReserve {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.SizeOfHeapReserve;
      }
    }
    internal ulong SizeOfStackCommit {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.SizeOfStackCommit;
      }
    }
    internal ulong SizeOfStackReserve {
      get
        //^ requires this.ReaderState >= ReaderState.PEFile;
      {
        return this.OptionalHeaderNTAdditionalFields.SizeOfStackReserve;
      }
    }

    #endregion Fields and Properties [PEFile]

    #region Methods [PEFile]

    private PEFileDebugInformation ReadDebugInformationLocationFromDebugTableDirectoryData() {
      if (this.OptionalHeaderDirectoryEntries.DebugTableDirectory.Size != 0x1c) return new PEFileDebugInformation();
      var debugDirectoryReader = new MemoryReader(this.DirectoryToMemoryBlock(this.OptionalHeaderDirectoryEntries.DebugTableDirectory));
      PeDebugDirectory debugDir = new PeDebugDirectory();
      debugDir.Characteristics = debugDirectoryReader.ReadUInt32();
      debugDir.TimeDateStamp = debugDirectoryReader.ReadUInt32();
      debugDir.MajorVersion = debugDirectoryReader.ReadUInt16();
      debugDir.MinorVersion = debugDirectoryReader.ReadUInt16();
      debugDir.Type = debugDirectoryReader.ReadUInt32();
      debugDir.SizeOfData = debugDirectoryReader.ReadUInt32();
      debugDir.AddressOfRawData = debugDirectoryReader.ReadUInt32();
      debugDir.PointerToRawData = debugDirectoryReader.ReadUInt32();
      if (debugDir.SizeOfData == 0) return new PEFileDebugInformation();
      var dataBlock = new MemoryBlock(this.BinaryDocumentMemoryBlock.Pointer + debugDir.PointerToRawData, debugDir.SizeOfData);
      var ptrToDebugInfo = this.BinaryDocumentMemoryBlock.Pointer + debugDir.PointerToRawData;
      var ptrToDebugInfoEnd = this.BinaryDocumentMemoryBlock.Pointer + debugDir.PointerToRawData + debugDir.SizeOfData;
      if (ptrToDebugInfo >= this.BinaryDocumentMemoryBlock.Pointer + this.BinaryDocumentMemoryBlock.Length - 28) {
        //TODO: error
        return new PEFileDebugInformation();
      }
      if (ptrToDebugInfoEnd > this.BinaryDocumentMemoryBlock.Pointer + this.BinaryDocumentMemoryBlock.Length) {
        //TODO: error
        return new PEFileDebugInformation();
      }
      var debugDataReader = new MemoryReader(dataBlock);
      var magic = debugDataReader.ReadUInt32();
      if (magic != 0x53445352) { //RSDS in little endian format
        //TODO: error
        return new PEFileDebugInformation();
      }

      var guid = debugDataReader.PeekGuid(0);
      debugDataReader.SkipBytes(16);
      UInt32 age = debugDataReader.ReadUInt32();
      string fileName = debugDataReader.ReadASCIINullTerminated();

      var guidHex = guid.ToString("N");
      string ageHex = age.ToString("X");
      string version = guidHex + ageHex;

      PEFileDebugInformation information = new PEFileDebugInformation();
      information.Signature = guid;
      information.Age = age;
      information.PdbVersion = version;
      information.PdbFileName = fileName;
      return information;
    }

    bool ReadCOFFFileHeader(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      if (memReader.RemainingBytes < PEFileConstants.SizeofCOFFFileHeader) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.COFFHeaderTooSmall);
        return false;
      }
      this.COFFFileHeader.Machine = (Machine)memReader.ReadUInt16();
      this.COFFFileHeader.NumberOfSections = memReader.ReadInt16();
      this.COFFFileHeader.TimeDateStamp = memReader.ReadInt32();
      this.COFFFileHeader.PointerToSymbolTable = memReader.ReadInt32();
      this.COFFFileHeader.NumberOfSymbols = memReader.ReadInt32();
      this.COFFFileHeader.SizeOfOptionalHeader = memReader.ReadInt16();
      this.COFFFileHeader.Characteristics = (Characteristics)memReader.ReadUInt16();
      return true;
    }
    bool ReadOptionalHeaderStandardFields32(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      if (memReader.RemainingBytes < PEFileConstants.SizeofOptionalHeaderStandardFields32) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.OptionalHeaderStandardFields32TooSmall);
        return false;
      }
      this.OptionalHeaderStandardFields.PEMagic = (PEMagic)memReader.ReadUInt16();
      this.OptionalHeaderStandardFields.MajorLinkerVersion = memReader.ReadByte();
      this.OptionalHeaderStandardFields.MinorLinkerVersion = memReader.ReadByte();
      this.OptionalHeaderStandardFields.SizeOfCode = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.SizeOfInitializedData = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.SizeOfUninitializedData = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.RVAOfEntryPoint = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.BaseOfCode = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.BaseOfData = memReader.ReadInt32();
      return true;
    }
    bool ReadOptionalHeaderNTAdditionalFields32(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      if (memReader.RemainingBytes < PEFileConstants.SizeofOptionalHeaderNTAdditionalFields32) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.OptionalHeaderNTAdditionalFields32TooSmall);
        return false;
      }
      this.OptionalHeaderNTAdditionalFields.ImageBase = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.SectionAlignment = memReader.ReadInt32();
      this.OptionalHeaderNTAdditionalFields.FileAlignment = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.MajorOperatingSystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MinorOperatingSystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MajorImageVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MinorImageVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MajorSubsystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MinorSubsystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.Win32VersionValue = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfImage = memReader.ReadInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfHeaders = memReader.ReadInt32();
      this.OptionalHeaderNTAdditionalFields.CheckSum = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.Subsystem = (Subsystem)memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.DllCharacteristics = (DllCharacteristics)memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.SizeOfStackReserve = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfStackCommit = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfHeapReserve = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfHeapCommit = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.LoaderFlags = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.NumberOfRvaAndSizes = memReader.ReadInt32();
      return true;
    }
    bool ReadOptionalHeaderStandardFields64(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      if (memReader.RemainingBytes < PEFileConstants.SizeofOptionalHeaderStandardFields64) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.OptionalHeaderStandardFields64TooSmall);
        return false;
      }
      this.OptionalHeaderStandardFields.PEMagic = (PEMagic)memReader.ReadUInt16();
      this.OptionalHeaderStandardFields.MajorLinkerVersion = memReader.ReadByte();
      this.OptionalHeaderStandardFields.MinorLinkerVersion = memReader.ReadByte();
      this.OptionalHeaderStandardFields.SizeOfCode = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.SizeOfInitializedData = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.SizeOfUninitializedData = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.RVAOfEntryPoint = memReader.ReadInt32();
      this.OptionalHeaderStandardFields.BaseOfCode = memReader.ReadInt32();
      return true;
    }
    bool ReadOptionalHeaderNTAdditionalFields64(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      if (memReader.RemainingBytes < PEFileConstants.SizeofOptionalHeaderNTAdditionalFields64) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.OptionalHeaderNTAdditionalFields64TooSmall);
        return false;
      }
      this.OptionalHeaderNTAdditionalFields.ImageBase = memReader.ReadUInt64();
      this.OptionalHeaderNTAdditionalFields.SectionAlignment = memReader.ReadInt32();
      this.OptionalHeaderNTAdditionalFields.FileAlignment = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.MajorOperatingSystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MinorOperatingSystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MajorImageVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MinorImageVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MajorSubsystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.MinorSubsystemVersion = memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.Win32VersionValue = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfImage = memReader.ReadInt32();
      this.OptionalHeaderNTAdditionalFields.SizeOfHeaders = memReader.ReadInt32();
      this.OptionalHeaderNTAdditionalFields.CheckSum = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.Subsystem = (Subsystem)memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.DllCharacteristics = (DllCharacteristics)memReader.ReadUInt16();
      this.OptionalHeaderNTAdditionalFields.SizeOfStackReserve = memReader.ReadUInt64();
      this.OptionalHeaderNTAdditionalFields.SizeOfStackCommit = memReader.ReadUInt64();
      this.OptionalHeaderNTAdditionalFields.SizeOfHeapReserve = memReader.ReadUInt64();
      this.OptionalHeaderNTAdditionalFields.SizeOfHeapCommit = memReader.ReadUInt64();
      this.OptionalHeaderNTAdditionalFields.LoaderFlags = memReader.ReadUInt32();
      this.OptionalHeaderNTAdditionalFields.NumberOfRvaAndSizes = memReader.ReadInt32();
      return true;
    }
    bool ReadOptionalHeaderDirectoryEntries(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      if (memReader.RemainingBytes < PEFileConstants.SizeofOptionalHeaderDirectoriesEntries) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.OptionalHeaderDirectoryEntriesTooSmall);
        return false;
      }
      this.OptionalHeaderDirectoryEntries.ExportTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ExportTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.ImportTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ImportTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.ResourceTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ResourceTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.ExceptionTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ExceptionTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.CertificateTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.CertificateTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.BaseRelocationTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.BaseRelocationTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.DebugTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.DebugTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.CopyrightTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.CopyrightTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.GlobalPointerTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.GlobalPointerTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.ThreadLocalStorageTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ThreadLocalStorageTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.LoadConfigTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.LoadConfigTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.BoundImportTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.BoundImportTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.ImportAddressTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ImportAddressTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.DelayImportTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.DelayImportTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.COR20HeaderTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.COR20HeaderTableDirectory.Size = memReader.ReadUInt32();
      this.OptionalHeaderDirectoryEntries.ReservedDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.OptionalHeaderDirectoryEntries.ReservedDirectory.Size = memReader.ReadUInt32();
      return true;
    }
    bool ReadSectionHeaders(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      int numberOfSections = this.COFFFileHeader.NumberOfSections;
      if (memReader.RemainingBytes < numberOfSections * PEFileConstants.SizeofSectionHeader) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.SectionHeadersTooSmall);
        return false;
      }
      this.SectionHeaders = new SectionHeader[numberOfSections];
      SectionHeader[] sectionHeaderArray = this.SectionHeaders;
      for (int i = 0; i < numberOfSections; ++i) {
        sectionHeaderArray[i].Name = memReader.ReadASCIIWithSize(PEFileConstants.SizeofSectionName);
        sectionHeaderArray[i].VirtualSize = memReader.ReadInt32();
        sectionHeaderArray[i].VirtualAddress = memReader.ReadInt32();
        sectionHeaderArray[i].SizeOfRawData = memReader.ReadInt32();
        sectionHeaderArray[i].OffsetToRawData = memReader.ReadInt32();
        sectionHeaderArray[i].RVAToRelocations = memReader.ReadInt32();
        sectionHeaderArray[i].PointerToLineNumbers = memReader.ReadInt32();
        sectionHeaderArray[i].NumberOfRelocations = memReader.ReadUInt16();
        sectionHeaderArray[i].NumberOfLineNumbers = memReader.ReadUInt16();
        sectionHeaderArray[i].SectionCharacteristics = (SectionCharacteristics)memReader.ReadUInt32();
      }
      return true;
    }
    bool ReadPEFileLevelData()
      //^ requires this.ReaderState >= ReaderState.Initialized;
    {
      MemoryReader memReader = new MemoryReader(this.BinaryDocumentMemoryBlock.Pointer, this.BinaryDocumentMemoryBlock.Length);
      if (memReader.RemainingBytes < PEFileConstants.BasicPEHeaderSize) {
        this.ErrorContainer.AddBinaryError(0, MetadataReaderErrorKind.FileSizeTooSmall);
        return false;
      }
      //  Look for DOS Signature "MZ"
      ushort dosSig = memReader.PeekUInt16(0);
      if (dosSig != PEFileConstants.DosSignature) {
        this.ErrorContainer.AddBinaryError(0, MetadataReaderErrorKind.DosHeader);
        return false;
      }
      //  Skip the DOS Header
      int ntHeaderOffset = memReader.PeekInt32(PEFileConstants.PESignatureOffsetLocation);
      if (!memReader.SeekOffset(ntHeaderOffset)) {
        this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.FileSizeTooSmall);
        return false;
      }
      //  Look for PESignature "PE\0\0"
      uint NTSignature = memReader.ReadUInt32();
      if (NTSignature != PEFileConstants.PESignature) {
        this.ErrorContainer.AddBinaryError(memReader.Offset - sizeof(uint), MetadataReaderErrorKind.PESignature);
        return false;
      }
      //  Read the COFF Header
      if (!this.ReadCOFFFileHeader(ref memReader)) {
        return false;
      }
      //  Read the magic to determine if its PE or PE+
      PEMagic magic = (PEMagic)memReader.PeekUInt16(0);
      switch (magic) {
        case PEMagic.PEMagic32:
          if (
            !this.ReadOptionalHeaderStandardFields32(ref memReader)
            || !this.ReadOptionalHeaderNTAdditionalFields32(ref memReader)
          ) {
            return false;
          }
          break;
        case PEMagic.PEMagic64:
          if (
            !this.ReadOptionalHeaderStandardFields64(ref memReader)
            || !this.ReadOptionalHeaderNTAdditionalFields64(ref memReader)
          ) {
            return false;
          }
          break;
        default:
          this.ErrorContainer.AddBinaryError(memReader.Offset, MetadataReaderErrorKind.UnknownPEMagic);
          return false;
      }
      if (!this.ReadOptionalHeaderDirectoryEntries(ref memReader)) {
        return false;
      }
      if (!this.ReadSectionHeaders(ref memReader)) {
        return false;
      }
      this.ReaderState = ReaderState.PEFile;
      this.Win32ResourceMemoryReader = new MemoryReader(this.DirectoryToMemoryBlock(this.OptionalHeaderDirectoryEntries.ResourceTableDirectory));
      return true;
    }

    internal ResourceDirectory GetResourceDirectoryAt(
      int offset
    ) {
      ResourceDirectory retResourceDir = new ResourceDirectory();
      int currOffset = offset;
      retResourceDir.Charecteristics = this.Win32ResourceMemoryReader.PeekUInt32(currOffset);
      currOffset += sizeof(Int32);
      retResourceDir.TimeDateStamp = this.Win32ResourceMemoryReader.PeekUInt32(currOffset);
      currOffset += sizeof(Int32);
      retResourceDir.MajorVersion = this.Win32ResourceMemoryReader.PeekInt16(currOffset);
      currOffset += sizeof(Int16);
      retResourceDir.MinorVersion = this.Win32ResourceMemoryReader.PeekInt16(currOffset);
      currOffset += sizeof(Int16);
      retResourceDir.NumberOfNamedEntries = this.Win32ResourceMemoryReader.PeekInt16(currOffset);
      currOffset += sizeof(Int16);
      retResourceDir.NumberOfIdEntries = this.Win32ResourceMemoryReader.PeekInt16(currOffset);
      return retResourceDir;
    }
    internal ResourceDirectoryEntry GetResourceDirectoryEntryAt(
      int offset
    ) {
      int nameOrId = this.Win32ResourceMemoryReader.PeekInt32(offset);
      int dataOffset = this.Win32ResourceMemoryReader.PeekInt32(offset + sizeof(Int32));
      return new ResourceDirectoryEntry(nameOrId, dataOffset);
    }
    internal ResourceDataEntry GetResourceDataEntryAt(
      int offset
    ) {
      int currOffset = offset;
      int offsetToData = this.Win32ResourceMemoryReader.PeekInt32(currOffset);
      currOffset += sizeof(Int32);
      int size = this.Win32ResourceMemoryReader.PeekInt32(currOffset);
      currOffset += sizeof(Int32);
      int codePage = this.Win32ResourceMemoryReader.PeekInt32(currOffset);
      currOffset += sizeof(Int32);
      int reserved = this.Win32ResourceMemoryReader.PeekInt32(currOffset);
      return new ResourceDataEntry(offsetToData, size, codePage, reserved);
    }
    internal MemoryBlock DirectoryToMemoryBlock(
      DirectoryEntry directory
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (sectionHeaderIter.VirtualAddress <= directory.RelativeVirtualAddress && directory.RelativeVirtualAddress < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) {
          int relativeOffset = directory.RelativeVirtualAddress - sectionHeaderIter.VirtualAddress;
          if (directory.Size > sectionHeaderIter.VirtualSize - relativeOffset) {
            //  PEFile Error.
          }
          MemoryBlock retMemBlock =
            new MemoryBlock(
              this.BinaryDocumentMemoryBlock.Pointer + sectionHeaderIter.OffsetToRawData + relativeOffset,
              directory.Size
            );
          return retMemBlock;
        }
      }
      //  Error section not found...
      return new MemoryBlock();
    }
    internal MemoryBlock RVAToMemoryBlockWithSize(
      int RVA,
      int size
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (sectionHeaderIter.VirtualAddress <= RVA && RVA < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) {
          int relativeOffset = RVA - sectionHeaderIter.VirtualAddress;
          if (size > sectionHeaderIter.VirtualSize - relativeOffset) {
            //  PEFile Error.
          }
          MemoryBlock retMemBlock =
            new MemoryBlock(
              this.BinaryDocumentMemoryBlock.Pointer + sectionHeaderIter.OffsetToRawData + relativeOffset,
              (int)size
            );
          return retMemBlock;
        }
      }
      //  Error section not found...
      return new MemoryBlock();
    }
    internal MemoryBlock RVAToMemoryBlock(
      int RVA
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (sectionHeaderIter.VirtualAddress <= RVA && RVA < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) {
          int relativeOffset = RVA - sectionHeaderIter.VirtualAddress;
          MemoryBlock retMemBlock =
            new MemoryBlock(
              this.BinaryDocumentMemoryBlock.Pointer + sectionHeaderIter.OffsetToRawData + relativeOffset,
              sectionHeaderIter.VirtualSize - relativeOffset
            );
          return retMemBlock;
        }
      }
      //  Error section not found...
      return new MemoryBlock();
    }
    internal bool RVAsInSameSection(
      int RVA1,
      int RVA2
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (
          sectionHeaderIter.VirtualAddress <= RVA1 && RVA1 < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize
          && sectionHeaderIter.VirtualAddress <= RVA2 && RVA2 < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize
        ) {
          return true;
        }
      }
      return false;
    }
    internal SubSection RVAToSubSection(
      int RVA,
      int size
    ) {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (sectionHeaderIter.VirtualAddress <= RVA && RVA + size <= sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) {
          int relativeOffset = RVA - sectionHeaderIter.VirtualAddress;
          MemoryBlock memBlock =
            new MemoryBlock(
              this.BinaryDocumentMemoryBlock.Pointer + sectionHeaderIter.OffsetToRawData + relativeOffset,
              size
            );
          SubSection retSubSection = new SubSection(sectionHeaderIter.Name, relativeOffset, memBlock);
          return retSubSection;
        }
      }
      //  Error section not found...
      return new SubSection();
    }
    internal string RVAToSubSectionName(
      int RVA
    ) {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (sectionHeaderIter.VirtualAddress <= RVA && RVA < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) {
          return sectionHeaderIter.Name;
        }
      }
      return "";
    }
    internal int GetSizeOfRemainderOfSectionContaining(
      int RVA
    ) {
      foreach (SectionHeader sectionHeaderIter in this.SectionHeaders) {
        if (sectionHeaderIter.VirtualAddress <= RVA && RVA < sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) {
          return (sectionHeaderIter.VirtualAddress + sectionHeaderIter.VirtualSize) - RVA;
        }
      }
      return 0;
    }
    #endregion Methods [PEFile]


    #region Fields and Properties [CORModule]
    internal COR20Header COR20Header;
    internal MetadataHeader MetadataHeader;
    StorageHeader StorageHeader;
    StreamHeader[] StreamHeaders;
    internal StringStreamReader StringStream;
    internal BlobStreamReader BlobStream;
    internal GUIDStreamReader GUIDStream;
    internal UserStringStreamReader UserStringStream;
    internal MetadataStreamKind MetadataStreamKind;
    MemoryBlock MetadataTableStream;
    internal MemoryReader ResourceMemoryReader;
    internal MemoryBlock StrongNameSignature;
    string MetadataStreamName {
      get {
        if (this.MetadataStreamKind == MetadataStreamKind.Compressed) {
          return COR20Constants.CompressedMetadataTableStreamName;
        } else {
          return COR20Constants.UncompressedMetadataTableStreamName;
        }
      }
    }
    #endregion Fields and Properties [CORModule]

    #region Methods [CORModule]
    bool ReadCOR20Header()
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      MemoryBlock memBlock = this.DirectoryToMemoryBlock(this.OptionalHeaderDirectoryEntries.COR20HeaderTableDirectory);
      if (memBlock.Length < this.OptionalHeaderDirectoryEntries.COR20HeaderTableDirectory.Size) {
        this.ErrorContainer.AddDirectoryError(Directories.COR20Header, 0, MetadataReaderErrorKind.NotEnoughSpaceForCOR20HeaderTableDirectory);
        return false;
      }
      MemoryReader memReader = new MemoryReader(memBlock);
      if (memReader.RemainingBytes < COR20Constants.SizeOfCOR20Header) {
        this.ErrorContainer.AddDirectoryError(Directories.COR20Header, 0, MetadataReaderErrorKind.COR20HeaderTooSmall);
        return false;
      }
      this.COR20Header.CountBytes = memReader.ReadInt32();
      this.COR20Header.MajorRuntimeVersion = memReader.ReadUInt16();
      this.COR20Header.MinorRuntimeVersion = memReader.ReadUInt16();
      this.COR20Header.MetaDataDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.MetaDataDirectory.Size = memReader.ReadUInt32();
      this.COR20Header.COR20Flags = (COR20Flags)memReader.ReadUInt32();
      this.COR20Header.EntryPointTokenOrRVA = memReader.ReadUInt32();
      this.COR20Header.ResourcesDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.ResourcesDirectory.Size = memReader.ReadUInt32();
      this.COR20Header.StrongNameSignatureDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.StrongNameSignatureDirectory.Size = memReader.ReadUInt32();
      this.COR20Header.CodeManagerTableDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.CodeManagerTableDirectory.Size = memReader.ReadUInt32();
      this.COR20Header.VtableFixupsDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.ExportAddressTableJumpsDirectory.Size = memReader.ReadUInt32();
      this.COR20Header.ExportAddressTableJumpsDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.ExportAddressTableJumpsDirectory.Size = memReader.ReadUInt32();
      this.COR20Header.ManagedNativeHeaderDirectory.RelativeVirtualAddress = memReader.ReadInt32();
      this.COR20Header.ManagedNativeHeaderDirectory.Size = memReader.ReadUInt32();
      return true;
    }
    bool ReadMetadataHeader(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      if (memReader.RemainingBytes < COR20Constants.MinimumSizeofMetadataHeader) {
        this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, 0, MetadataReaderErrorKind.MetadataHeaderTooSmall);
        return false;
      }
      this.MetadataHeader.Signature = memReader.ReadUInt32();
      if (this.MetadataHeader.Signature != COR20Constants.COR20MetadataSignature) {
        this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, memReader.Offset - sizeof(uint), MetadataReaderErrorKind.MetadataSignature);
        return false;
      }
      this.MetadataHeader.MajorVersion = memReader.ReadUInt16();
      this.MetadataHeader.MinorVersion = memReader.ReadUInt16();
      this.MetadataHeader.ExtraData = memReader.ReadUInt32();
      this.MetadataHeader.VersionStringSize = memReader.ReadInt32();
      if (memReader.RemainingBytes < this.MetadataHeader.VersionStringSize) {
        this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, memReader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForVersionString);
        return false;
      }
      int numberOfBytesRead;
      this.MetadataHeader.VersionString = memReader.PeekUTF8NullTerminated(0, out numberOfBytesRead);
      memReader.SkipBytes(this.MetadataHeader.VersionStringSize);
      return true;
    }
    bool ReadStorageHeader(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      if (memReader.RemainingBytes < COR20Constants.SizeofStorageHeader) {
        this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, memReader.Offset, MetadataReaderErrorKind.StorageHeaderTooSmall);
        return false;
      }
      this.StorageHeader.Flags = memReader.ReadUInt16();
      this.StorageHeader.NumberOfStreams = memReader.ReadInt16();
      return true;
    }
    bool ReadStreamHeaders(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      int numberOfStreams = this.StorageHeader.NumberOfStreams;
      this.StreamHeaders = new StreamHeader[numberOfStreams];
      StreamHeader[] streamHeaders = this.StreamHeaders;
      for (int i = 0; i < numberOfStreams; ++i) {
        if (memReader.RemainingBytes < COR20Constants.MinimumSizeofStreamHeader) {
          this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, memReader.Offset, MetadataReaderErrorKind.StreamHeaderTooSmall);
          return false;
        }
        streamHeaders[i].Offset = memReader.ReadUInt32();
        streamHeaders[i].Size = memReader.ReadInt32();
        //  Review: Oh well there is no way i can test if we will read correctly. However we can check it after reading and aligning...
        streamHeaders[i].Name = memReader.ReadASCIINullTerminated();
        memReader.Align(4);
        if (memReader.RemainingBytes < 0) {
          this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, memReader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForStreamHeaderName);
          return false;
        }
      }
      return true;
    }
    bool ProcessAndCacheStreams(
      ref MemoryBlock metadataRoot
    )
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      foreach (StreamHeader streamHeader in this.StreamHeaders) {
        switch (streamHeader.Name) {
          case COR20Constants.StringStreamName:
            if (metadataRoot.Length < streamHeader.Offset + streamHeader.Size) {
              this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForStringStream);
              return false;
            }
            this.StringStream.MemoryReader =
              new MemoryReader(
                metadataRoot.Buffer + streamHeader.Offset,
                streamHeader.Size
              );
            break;
          case COR20Constants.BlobStreamName:
            if (metadataRoot.Length < streamHeader.Offset + streamHeader.Size) {
              this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForBlobStream);
              return false;
            }
            this.BlobStream.MemoryReader =
              new MemoryReader(
                metadataRoot.Buffer + streamHeader.Offset,
                streamHeader.Size
              );
            break;
          case COR20Constants.GUIDStreamName:
            if (metadataRoot.Length < streamHeader.Offset + streamHeader.Size) {
              this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForGUIDStream);
              return false;
            }
            this.GUIDStream.MemoryReader =
              new MemoryReader(
                metadataRoot.Buffer + streamHeader.Offset,
                streamHeader.Size
              );
            break;
          case COR20Constants.UserStringStreamName:
            if (metadataRoot.Length < streamHeader.Offset + streamHeader.Size) {
              this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForBlobStream);
              return false;
            }
            this.UserStringStream.MemoryReader =
              new MemoryReader(
                metadataRoot.Buffer + streamHeader.Offset,
                streamHeader.Size
              );
            break;
          case COR20Constants.CompressedMetadataTableStreamName:
            if (metadataRoot.Length < streamHeader.Offset + streamHeader.Size) {
              this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForMetadataStream);
              return false;
            }
            this.MetadataStreamKind = MetadataStreamKind.Compressed;
            this.MetadataTableStream =
              new MemoryBlock(
                metadataRoot.Buffer + streamHeader.Offset,
                streamHeader.Size
              );
            break;
          case COR20Constants.UncompressedMetadataTableStreamName:
            if (metadataRoot.Length < streamHeader.Offset + streamHeader.Size) {
              this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.NotEnoughSpaceForMetadataStream);
              return false;
            }
            this.MetadataStreamKind = MetadataStreamKind.UnCompressed;
            this.MetadataTableStream =
              new MemoryBlock(
                metadataRoot.Buffer + streamHeader.Offset,
                streamHeader.Size
              );
            break;
          default:
            this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, streamHeader.Offset, MetadataReaderErrorKind.UnknownMetadataStream);
            break;
        }
      }
      return true;
    }
    bool ReadCORModuleLevelData()
      //^ requires this.ReaderState >= ReaderState.PEFile;
    {
      if (!this.ReadCOR20Header()) {
        return false;
      }
      MemoryBlock metadataRoot = this.DirectoryToMemoryBlock(this.COR20Header.MetaDataDirectory);
      if (metadataRoot.Length < this.COR20Header.MetaDataDirectory.Size) {
        this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, 0, MetadataReaderErrorKind.NotEnoughSpaceForMetadataDirectory);
        return false;
      }
      MemoryReader memReader = new MemoryReader(metadataRoot);
      if (
        !this.ReadMetadataHeader(ref memReader)
        || !this.ReadStorageHeader(ref memReader)
        || !this.ReadStreamHeaders(ref memReader)
        || !this.ProcessAndCacheStreams(ref metadataRoot)
      ) {
        return false;
      }
      this.ReaderState = ReaderState.CORModule;
      this.ResourceMemoryReader = new MemoryReader(this.DirectoryToMemoryBlock(this.COR20Header.ResourcesDirectory));
      this.StrongNameSignature = this.DirectoryToMemoryBlock(this.COR20Header.StrongNameSignatureDirectory);
      return true;
    }

    bool ReadMetadataRoot() {
      var metadataRoot = new MemoryBlock(this.BinaryDocumentMemoryBlock.Pointer, (int)this.BinaryDocumentMemoryBlock.Length);
      MemoryReader memReader = new MemoryReader(metadataRoot);
      if (
        !this.ReadMetadataHeader(ref memReader)
        || !this.ReadStorageHeader(ref memReader)
        || !this.ReadStreamHeaders(ref memReader)
        || !this.ProcessAndCacheStreams(ref metadataRoot)
      ) {
        return false;
      }
      this.ReaderState = ReaderState.CORModule;
      return true;
    }

    #endregion Methods [CORModule]

    #region Fields and Properties [Metadata]
    internal MetadataTableHeader MetadataTableHeader;
    uint[] MetadataTableRowCount;
    internal ModuleTableReader ModuleTable;
    internal TypeRefTableReader TypeRefTable;
    internal TypeDefTableReader TypeDefTable;
    internal FieldPtrTableReader FieldPtrTable;
    internal FieldTableReader FieldTable;
    internal MethodPtrTableReader MethodPtrTable;
    internal MethodTableReader MethodTable;
    internal ParamPtrTableReader ParamPtrTable;
    internal ParamTableReader ParamTable;
    internal InterfaceImplTableReader InterfaceImplTable;
    internal MemberRefTableReader MemberRefTable;
    internal ConstantTableReader ConstantTable;
    internal CustomAttributeTableReader CustomAttributeTable;
    internal FieldMarshalTableReader FieldMarshalTable;
    internal DeclSecurityTableReader DeclSecurityTable;
    internal ClassLayoutTableReader ClassLayoutTable;
    internal FieldLayoutTableReader FieldLayoutTable;
    internal StandAloneSigTableReader StandAloneSigTable;
    internal EventMapTableReader EventMapTable;
    internal EventPtrTableReader EventPtrTable;
    internal EventTableReader EventTable;
    internal PropertyMapTableReader PropertyMapTable;
    internal PropertyPtrTableReader PropertyPtrTable;
    internal PropertyTableReader PropertyTable;
    internal MethodSemanticsTableReader MethodSemanticsTable;
    internal MethodImplTableReader MethodImplTable;
    internal ModuleRefTableReader ModuleRefTable;
    internal TypeSpecTableReader TypeSpecTable;
    internal ImplMapTableReader ImplMapTable;
    internal FieldRVATableReader FieldRVATable;
    internal EnCLogTableReader EnCLogTable;
    internal EnCMapTableReader EnCMapTable;
    internal AssemblyTableReader AssemblyTable;
    internal AssemblyProcessorTableReader AssemblyProcessorTable;
    internal AssemblyOSTableReader AssemblyOSTable;
    internal AssemblyRefTableReader AssemblyRefTable;
    internal AssemblyRefProcessorTableReader AssemblyRefProcessorTable;
    internal AssemblyRefOSTableReader AssemblyRefOSTable;
    internal FileTableReader FileTable;
    internal ExportedTypeTableReader ExportedTypeTable;
    internal ManifestResourceTableReader ManifestResourceTable;
    internal NestedClassTableReader NestedClassTable;
    internal GenericParamTableReader GenericParamTable;
    internal MethodSpecTableReader MethodSpecTable;
    internal GenericParamConstraintTableReader GenericParamConstraintTable;
    internal Guid ModuleGuidIdentifier {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        ModuleRow moduleRow = this.ModuleTable[1];
        return this.GUIDStream[moduleRow.MVId];
      }
    }
    internal bool IsAssembly {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        return this.AssemblyTable.NumberOfRows == 1;
      }
    }
    internal bool UseFieldPtrTable {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        return this.FieldPtrTable.NumberOfRows > 0;
      }
    }
    internal bool UseMethodPtrTable {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        return this.MethodPtrTable.NumberOfRows > 0;
      }
    }
    internal bool UseParamPtrTable {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        return this.ParamPtrTable.NumberOfRows > 0;
      }
    }
    internal bool UseEventPtrTable {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        return this.EventPtrTable.NumberOfRows > 0;
      }
    }
    internal bool UsePropertyPtrTable {
      get
        //^ requires this.ReaderState >= ReaderState.Metadata;
      {
        return this.PropertyPtrTable.NumberOfRows > 0;
      }
    }
    #endregion Fields and Properties [Metadata]

    #region Methods [Metadata]
    bool ReadMetadataTableInformation(
      ref MemoryReader memReader
    )
      //^ requires this.ReaderState >= ReaderState.CORModule;
    {
      if (memReader.RemainingBytes < MetadataStreamConstants.SizeOfMetadataTableHeader) {
        this.ErrorContainer.AddMetadataStreamError(this.MetadataStreamName, 0, MetadataReaderErrorKind.MetadataTableHeaderTooSmall);
        return false;
      }
      this.MetadataTableHeader.Reserved = memReader.ReadUInt32();
      this.MetadataTableHeader.MajorVersion = memReader.ReadByte();
      this.MetadataTableHeader.MinorVersion = memReader.ReadByte();
      this.MetadataTableHeader.HeapSizeFlags = (HeapSizeFlag)memReader.ReadByte();
      this.MetadataTableHeader.RowId = memReader.ReadByte();
      this.MetadataTableHeader.ValidTables = (TableMask)memReader.ReadUInt64();
      this.MetadataTableHeader.SortedTables = (TableMask)memReader.ReadUInt64();
      ulong presentTables = (ulong)this.MetadataTableHeader.ValidTables;
      ulong validTablesForVersion = 0;
      int version = this.MetadataTableHeader.MajorVersion << 16 | this.MetadataTableHeader.MinorVersion;
      switch (version) {
        case 0x00010000:
          validTablesForVersion = (ulong)TableMask.V1_0_TablesMask;
          break;
        case 0x00010001:
          validTablesForVersion = (ulong)TableMask.V1_1_TablesMask;
          break;
        case 0x00020000:
          validTablesForVersion = (ulong)TableMask.V2_0_TablesMask;
          break;
        default:
          this.ErrorContainer.AddMetadataStreamError(this.MetadataStreamName, 4, MetadataReaderErrorKind.UnknownVersionOfMetadata);
          return false;
      }
      if ((presentTables & ~validTablesForVersion) != 0) {
        this.ErrorContainer.AddMetadataStreamError(this.MetadataStreamName, 8, MetadataReaderErrorKind.UnknownTables);
        return false;
      }
      if (this.MetadataStreamKind == MetadataStreamKind.Compressed && (presentTables & (ulong)TableMask.CompressedStreamNotAllowedMask) != 0) {
        this.ErrorContainer.AddMetadataStreamError(this.MetadataStreamName, 8, MetadataReaderErrorKind.IllegalTablesInCompressedMetadataStream);
        return false;
      }
      ulong requiredSortedTables = presentTables & validTablesForVersion & (ulong)TableMask.SortedTablesMask;
      if ((requiredSortedTables & (ulong)this.MetadataTableHeader.SortedTables) != requiredSortedTables) {
        this.ErrorContainer.AddMetadataStreamError(this.MetadataStreamName, 16, MetadataReaderErrorKind.SomeRequiredTablesNotSorted);
        //Carry on regardless. There are/were compiler out there that sort the required tables, but fail to set the bit in SortedTables.
      }
      int numberOfTables = this.MetadataTableHeader.GetNumberOfTablesPresent();
      if (memReader.RemainingBytes < numberOfTables * sizeof(Int32)) {
        this.ErrorContainer.AddMetadataStreamError(this.MetadataStreamName, memReader.Offset, MetadataReaderErrorKind.TableRowCountSpaceTooSmall);
        return false;
      }
      this.MetadataTableRowCount = new uint[numberOfTables];
      uint[] metadataTableRowCount = this.MetadataTableRowCount;
      for (int i = 0; i < numberOfTables; ++i) {
        metadataTableRowCount[i] = memReader.ReadUInt32();
      }
      return true;
    }
    static int ComputeCodedTokenSize(
      uint largeRowSize,
      uint[] rowCountArray,
      TableMask tablesReferenced
    ) {
      bool isAllReferencedTablesSmall = true;
      ulong tablesReferencedMask = (ulong)tablesReferenced;
      for (int tableIndex = 0; tableIndex < (int)TableIndices.Count; tableIndex++) {
        if ((tablesReferencedMask & 0x0000000000000001UL) != 0) {
          isAllReferencedTablesSmall &= (rowCountArray[tableIndex] < largeRowSize);
        }
        tablesReferencedMask >>= 1;
      }
      return isAllReferencedTablesSmall ? 2 : 4;
    }
    bool ProcessAndCacheMetadataTableBlocks(
      MemoryBlock metadataTablesMemoryBlock
    )
      //^ requires this.ReaderState >= ReaderState.CORModule;
    {
      uint[] rowCountArray = new uint[(int)TableIndices.Count];
      int[] rowRefSizeArray = new int[(int)TableIndices.Count];
      ulong validTables = (ulong)this.MetadataTableHeader.ValidTables;
      uint[] rowCountCompressedArray = this.MetadataTableRowCount;
      //  Fill in the row count and table reference sizes...
      for (int tableIndex = 0, arrayIndex = 0; tableIndex < (int)TableIndices.Count; tableIndex++) {
        if ((validTables & 0x0000000000000001UL) != 0) {
          uint rowCount = rowCountCompressedArray[arrayIndex++];
          rowCountArray[tableIndex] = rowCount;
          rowRefSizeArray[tableIndex] = rowCount < MetadataStreamConstants.LargeTableRowCount ? 2 : 4;
        } else {
          rowRefSizeArray[tableIndex] = 2;
        }
        validTables >>= 1;
      }
      //  Compute ref sizes for tables that can have pointer tables for it
      int fieldRefSize = rowRefSizeArray[(int)TableIndices.FieldPtr] > 2 ? 4 : rowRefSizeArray[(int)TableIndices.Field];
      int methodRefSize = rowRefSizeArray[(int)TableIndices.MethodPtr] > 2 ? 4 : rowRefSizeArray[(int)TableIndices.Method];
      int paramRefSize = rowRefSizeArray[(int)TableIndices.ParamPtr] > 2 ? 4 : rowRefSizeArray[(int)TableIndices.Param];
      int eventRefSize = rowRefSizeArray[(int)TableIndices.EventPtr] > 2 ? 4 : rowRefSizeArray[(int)TableIndices.Event];
      int propertyRefSize = rowRefSizeArray[(int)TableIndices.PropertyPtr] > 2 ? 4 : rowRefSizeArray[(int)TableIndices.Property];
      //  Compute the coded token ref sizes
      int typeDefOrRefRefSize = PEFileReader.ComputeCodedTokenSize(TypeDefOrRefTag.LargeRowSize, rowCountArray, TypeDefOrRefTag.TablesReferenced);
      int hasConstantRefSize = PEFileReader.ComputeCodedTokenSize(HasConstantTag.LargeRowSize, rowCountArray, HasConstantTag.TablesReferenced);
      int hasCustomAttributeRefSize = PEFileReader.ComputeCodedTokenSize(HasCustomAttributeTag.LargeRowSize, rowCountArray, HasCustomAttributeTag.TablesReferenced);
      int hasFieldMarshalRefSize = PEFileReader.ComputeCodedTokenSize(HasFieldMarshalTag.LargeRowSize, rowCountArray, HasFieldMarshalTag.TablesReferenced);
      int hasDeclSecurityRefSize = PEFileReader.ComputeCodedTokenSize(HasDeclSecurityTag.LargeRowSize, rowCountArray, HasDeclSecurityTag.TablesReferenced);
      int memberRefParentRefSize = PEFileReader.ComputeCodedTokenSize(MemberRefParentTag.LargeRowSize, rowCountArray, MemberRefParentTag.TablesReferenced);
      int hasSemanticsRefSize = PEFileReader.ComputeCodedTokenSize(HasSemanticsTag.LargeRowSize, rowCountArray, HasSemanticsTag.TablesReferenced);
      int methodDefOrRefRefSize = PEFileReader.ComputeCodedTokenSize(MethodDefOrRefTag.LargeRowSize, rowCountArray, MethodDefOrRefTag.TablesReferenced);
      int memberForwardedRefSize = PEFileReader.ComputeCodedTokenSize(MemberForwardedTag.LargeRowSize, rowCountArray, MemberForwardedTag.TablesReferenced);
      int implementationRefSize = PEFileReader.ComputeCodedTokenSize(ImplementationTag.LargeRowSize, rowCountArray, ImplementationTag.TablesReferenced);
      int customAttributeTypeRefSize = PEFileReader.ComputeCodedTokenSize(CustomAttributeTypeTag.LargeRowSize, rowCountArray, CustomAttributeTypeTag.TablesReferenced);
      int resolutionScopeRefSize = PEFileReader.ComputeCodedTokenSize(ResolutionScopeTag.LargeRowSize, rowCountArray, ResolutionScopeTag.TablesReferenced);
      int typeOrMethodDefRefSize = PEFileReader.ComputeCodedTokenSize(TypeOrMethodDefTag.LargeRowSize, rowCountArray, TypeOrMethodDefTag.TablesReferenced);
      //  Compute HeapRef Sizes
      int stringHeapRefSize = (this.MetadataTableHeader.HeapSizeFlags & HeapSizeFlag.StringHeapLarge) == HeapSizeFlag.StringHeapLarge ? 4 : 2;
      int guidHeapRefSize = (this.MetadataTableHeader.HeapSizeFlags & HeapSizeFlag.GUIDHeapLarge) == HeapSizeFlag.GUIDHeapLarge ? 4 : 2;
      int blobHeapRefSize = (this.MetadataTableHeader.HeapSizeFlags & HeapSizeFlag.BlobHeapLarge) == HeapSizeFlag.BlobHeapLarge ? 4 : 2;
      //  Populate the Table blocks
      int totalRequiredSize = 0;
      int currentTableSize = 0;
      byte* currentPointer = metadataTablesMemoryBlock.Buffer;
      this.ModuleTable = new ModuleTableReader(rowCountArray[(int)TableIndices.Module], stringHeapRefSize, guidHeapRefSize, currentPointer);
      currentTableSize = this.ModuleTable.ModuleTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.TypeRefTable = new TypeRefTableReader(rowCountArray[(int)TableIndices.TypeRef], resolutionScopeRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.TypeRefTable.TypeRefTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.TypeDefTable = new TypeDefTableReader(rowCountArray[(int)TableIndices.TypeDef], fieldRefSize, methodRefSize, typeDefOrRefRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.TypeDefTable.TypeDefTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.FieldPtrTable = new FieldPtrTableReader(rowCountArray[(int)TableIndices.FieldPtr], rowRefSizeArray[(int)TableIndices.Field], currentPointer);
      currentTableSize = this.FieldPtrTable.FieldPtrTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.FieldTable = new FieldTableReader(rowCountArray[(int)TableIndices.Field], stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.FieldTable.FieldTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.MethodPtrTable = new MethodPtrTableReader(rowCountArray[(int)TableIndices.MethodPtr], rowRefSizeArray[(int)TableIndices.Method], currentPointer);
      currentTableSize = this.MethodPtrTable.MethodPtrTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.MethodTable = new MethodTableReader(rowCountArray[(int)TableIndices.Method], paramRefSize, stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.MethodTable.MethodTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ParamPtrTable = new ParamPtrTableReader(rowCountArray[(int)TableIndices.ParamPtr], rowRefSizeArray[(int)TableIndices.Param], currentPointer);
      currentTableSize = this.ParamPtrTable.ParamPtrTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ParamTable = new ParamTableReader(rowCountArray[(int)TableIndices.Param], stringHeapRefSize, currentPointer);
      currentTableSize = this.ParamTable.ParamTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.InterfaceImplTable = new InterfaceImplTableReader(rowCountArray[(int)TableIndices.InterfaceImpl], rowRefSizeArray[(int)TableIndices.TypeDef], typeDefOrRefRefSize, currentPointer);
      currentTableSize = this.InterfaceImplTable.InterfaceImplTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.MemberRefTable = new MemberRefTableReader(rowCountArray[(int)TableIndices.MemberRef], memberRefParentRefSize, stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.MemberRefTable.MemberRefTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ConstantTable = new ConstantTableReader(rowCountArray[(int)TableIndices.Constant], hasConstantRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.ConstantTable.ConstantTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.CustomAttributeTable = new CustomAttributeTableReader(rowCountArray[(int)TableIndices.CustomAttribute], hasCustomAttributeRefSize, customAttributeTypeRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.CustomAttributeTable.CustomAttributeTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.FieldMarshalTable = new FieldMarshalTableReader(rowCountArray[(int)TableIndices.FieldMarshal], hasFieldMarshalRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.FieldMarshalTable.FieldMarshalTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.DeclSecurityTable = new DeclSecurityTableReader(rowCountArray[(int)TableIndices.DeclSecurity], hasDeclSecurityRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.DeclSecurityTable.DeclSecurityTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ClassLayoutTable = new ClassLayoutTableReader(rowCountArray[(int)TableIndices.ClassLayout], rowRefSizeArray[(int)TableIndices.TypeDef], currentPointer);
      currentTableSize = this.ClassLayoutTable.ClassLayoutTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.FieldLayoutTable = new FieldLayoutTableReader(rowCountArray[(int)TableIndices.FieldLayout], rowRefSizeArray[(int)TableIndices.Field], currentPointer);
      currentTableSize = this.FieldLayoutTable.FieldLayoutTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.StandAloneSigTable = new StandAloneSigTableReader(rowCountArray[(int)TableIndices.StandAloneSig], blobHeapRefSize, currentPointer);
      currentTableSize = this.StandAloneSigTable.StandAloneSigTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.EventMapTable = new EventMapTableReader(rowCountArray[(int)TableIndices.EventMap], rowRefSizeArray[(int)TableIndices.TypeDef], eventRefSize, currentPointer);
      currentTableSize = this.EventMapTable.EventMapTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.EventPtrTable = new EventPtrTableReader(rowCountArray[(int)TableIndices.EventPtr], rowRefSizeArray[(int)TableIndices.Event], currentPointer);
      currentTableSize = this.EventPtrTable.EventPtrTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.EventTable = new EventTableReader(rowCountArray[(int)TableIndices.Event], typeDefOrRefRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.EventTable.EventTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.PropertyMapTable = new PropertyMapTableReader(rowCountArray[(int)TableIndices.PropertyMap], rowRefSizeArray[(int)TableIndices.TypeDef], propertyRefSize, currentPointer);
      currentTableSize = this.PropertyMapTable.PropertyMapTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.PropertyPtrTable = new PropertyPtrTableReader(rowCountArray[(int)TableIndices.PropertyPtr], rowRefSizeArray[(int)TableIndices.Property], currentPointer);
      currentTableSize = this.PropertyPtrTable.PropertyPtrTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.PropertyTable = new PropertyTableReader(rowCountArray[(int)TableIndices.Property], stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.PropertyTable.PropertyTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.MethodSemanticsTable = new MethodSemanticsTableReader(rowCountArray[(int)TableIndices.MethodSemantics], rowRefSizeArray[(int)TableIndices.Method], hasSemanticsRefSize, currentPointer);
      currentTableSize = this.MethodSemanticsTable.MethodSemanticsTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.MethodImplTable = new MethodImplTableReader(rowCountArray[(int)TableIndices.MethodImpl], rowRefSizeArray[(int)TableIndices.TypeDef], methodDefOrRefRefSize, currentPointer);
      currentTableSize = this.MethodImplTable.MethodImplTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ModuleRefTable = new ModuleRefTableReader(rowCountArray[(int)TableIndices.ModuleRef], stringHeapRefSize, currentPointer);
      currentTableSize = this.ModuleRefTable.ModuleRefTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.TypeSpecTable = new TypeSpecTableReader(rowCountArray[(int)TableIndices.TypeSpec], blobHeapRefSize, currentPointer);
      currentTableSize = this.TypeSpecTable.TypeSpecTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ImplMapTable = new ImplMapTableReader(rowCountArray[(int)TableIndices.ImplMap], rowRefSizeArray[(int)TableIndices.ModuleRef], memberForwardedRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.ImplMapTable.ImplMapTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.FieldRVATable = new FieldRVATableReader(rowCountArray[(int)TableIndices.FieldRva], rowRefSizeArray[(int)TableIndices.Field], currentPointer);
      currentTableSize = this.FieldRVATable.FieldRVATableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.EnCLogTable = new EnCLogTableReader(rowCountArray[(int)TableIndices.EnCLog], currentPointer);
      currentTableSize = this.EnCLogTable.EnCLogTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.EnCMapTable = new EnCMapTableReader(rowCountArray[(int)TableIndices.EnCMap], currentPointer);
      currentTableSize = this.EnCMapTable.EnCMapTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.AssemblyTable = new AssemblyTableReader(rowCountArray[(int)TableIndices.Assembly], stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.AssemblyTable.AssemblyTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.AssemblyProcessorTable = new AssemblyProcessorTableReader(rowCountArray[(int)TableIndices.AssemblyProcessor], currentPointer);
      currentTableSize = this.AssemblyProcessorTable.AssemblyProcessorTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.AssemblyOSTable = new AssemblyOSTableReader(rowCountArray[(int)TableIndices.AssemblyOS], currentPointer);
      currentTableSize = this.AssemblyOSTable.AssemblyOSTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.AssemblyRefTable = new AssemblyRefTableReader(rowCountArray[(int)TableIndices.AssemblyRef], stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.AssemblyRefTable.AssemblyRefTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.AssemblyRefProcessorTable = new AssemblyRefProcessorTableReader(rowCountArray[(int)TableIndices.AssemblyRefProcessor], rowRefSizeArray[(int)TableIndices.AssemblyRef], currentPointer);
      currentTableSize = this.AssemblyRefProcessorTable.AssemblyRefProcessorTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.AssemblyRefOSTable = new AssemblyRefOSTableReader(rowCountArray[(int)TableIndices.AssemblyRefOS], rowRefSizeArray[(int)TableIndices.AssemblyRef], currentPointer);
      currentTableSize = this.AssemblyRefOSTable.AssemblyRefOSTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.FileTable = new FileTableReader(rowCountArray[(int)TableIndices.File], stringHeapRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.FileTable.FileTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ExportedTypeTable = new ExportedTypeTableReader(rowCountArray[(int)TableIndices.ExportedType], implementationRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.ExportedTypeTable.ExportedTypeTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.ManifestResourceTable = new ManifestResourceTableReader(rowCountArray[(int)TableIndices.ManifestResource], implementationRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.ManifestResourceTable.ManifestResourceTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.NestedClassTable = new NestedClassTableReader(rowCountArray[(int)TableIndices.NestedClass], rowRefSizeArray[(int)TableIndices.TypeDef], currentPointer);
      currentTableSize = this.NestedClassTable.NestedClassTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.GenericParamTable = new GenericParamTableReader(rowCountArray[(int)TableIndices.GenericParam], typeOrMethodDefRefSize, stringHeapRefSize, currentPointer);
      currentTableSize = this.GenericParamTable.GenericParamTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.MethodSpecTable = new MethodSpecTableReader(rowCountArray[(int)TableIndices.MethodSpec], methodDefOrRefRefSize, blobHeapRefSize, currentPointer);
      currentTableSize = this.MethodSpecTable.MethodSpecTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      this.GenericParamConstraintTable = new GenericParamConstraintTableReader(rowCountArray[(int)TableIndices.GenericParamConstraint], rowRefSizeArray[(int)TableIndices.GenericParam], typeDefOrRefRefSize, currentPointer);
      currentTableSize = this.GenericParamConstraintTable.GenericParamConstraintTableMemoryReader.Length;
      totalRequiredSize += currentTableSize;
      currentPointer += currentTableSize;
      if (totalRequiredSize > metadataTablesMemoryBlock.Length) {
        this.ErrorContainer.AddDirectoryError(Directories.Cor20HeaderMetaData, 0, MetadataReaderErrorKind.MetadataTablesTooSmall);
        return false;
      }
      return true;
    }
    bool ReadMetadataLevelData()
      //^ requires this.ReaderState >= ReaderState.CORModule;
    {
      MemoryReader memReader = new MemoryReader(this.MetadataTableStream);
      if (
        !this.ReadMetadataTableInformation(ref memReader)
        || !this.ProcessAndCacheMetadataTableBlocks(memReader.RemainingMemoryBlock)
      ) {
        return false;
      }
      if (this.ModuleTable.NumberOfRows != 1)
        return false;
      this.ReaderState = ReaderState.Metadata;
      return true;
    }
    internal uint GetFieldInformation(
      uint typeDefRowId,
      out uint fieldCount
    )
      //^ requires typeDefRowId <= this.TypeDefTable.NumberOfRows;
      //^ requires this.ReaderState >= ReaderState.Metadata;
    {
      uint fieldStartRow = this.TypeDefTable.GetFieldStart(typeDefRowId);
      if (typeDefRowId == this.TypeDefTable.NumberOfRows) {
        fieldCount = (this.UseFieldPtrTable ? this.FieldPtrTable.NumberOfRows : this.FieldTable.NumberOfRows) - fieldStartRow + 1;
      } else {
        fieldCount = this.TypeDefTable.GetFieldStart(typeDefRowId + 1) - fieldStartRow;
      }
      return fieldStartRow;
    }
    internal uint GetMethodInformation(
      uint typeDefRowId,
      out uint methodCount
    )
      //^ requires typeDefRowId <= this.TypeDefTable.NumberOfRows;
      //^ requires this.ReaderState >= ReaderState.Metadata;
    {
      uint methodStartRow = this.TypeDefTable.GetMethodStart(typeDefRowId);
      if (typeDefRowId == this.TypeDefTable.NumberOfRows) {
        methodCount = (this.UseMethodPtrTable ? this.MethodPtrTable.NumberOfRows : this.MethodTable.NumberOfRows) - methodStartRow + 1;
      } else {
        methodCount = this.TypeDefTable.GetMethodStart(typeDefRowId + 1) - methodStartRow;
      }
      return methodStartRow;
    }
    internal uint GetEventInformation(
      uint typeDefRowId,
      out uint eventCount
    )
      //^ requires typeDefRowId <= this.TypeDefTable.NumberOfRows;
      //^ requires this.ReaderState >= ReaderState.Metadata;
    {
      eventCount = 0;
      uint eventMapRowId = this.EventMapTable.FindEventMapRowIdFor(typeDefRowId);
      if (eventMapRowId == 0)
        return 0;
      uint eventStartRow = this.EventMapTable.GetEventListStartFor(eventMapRowId);
      if (eventMapRowId == this.EventMapTable.NumberOfRows) {
        eventCount = (this.UseEventPtrTable ? this.EventPtrTable.NumberOfRows : this.EventTable.NumberOfRows) - eventStartRow + 1;
      } else {
        eventCount = this.EventMapTable.GetEventListStartFor(eventMapRowId + 1) - eventStartRow;
      }
      return eventStartRow;
    }
    internal uint GetPropertyInformation(
      uint typeDefRowId,
      out uint propertyCount
    )
      //^ requires typeDefRowId <= this.TypeDefTable.NumberOfRows;
      //^ requires this.ReaderState >= ReaderState.Metadata;
    {
      propertyCount = 0;
      uint propertyMapRowId = this.PropertyMapTable.FindPropertyMapRowIdFor(typeDefRowId);
      if (propertyMapRowId == 0)
        return 0;
      uint propertyStartRow = this.PropertyMapTable.GetPropertyListStartFor(propertyMapRowId);
      if (propertyMapRowId == this.PropertyMapTable.NumberOfRows) {
        propertyCount = (this.UsePropertyPtrTable ? this.PropertyPtrTable.NumberOfRows : this.PropertyTable.NumberOfRows) - propertyStartRow + 1;
      } else {
        propertyCount = this.PropertyMapTable.GetPropertyListStartFor(propertyMapRowId + 1) - propertyStartRow;
      }
      return propertyStartRow;
    }
    internal uint GetParamInformation(
      uint methodDefRowId,
      out uint paramRowCount
    )
      //^ requires methodDefRowId <= this.MethodTable.NumberOfRows;
      //^ requires this.ReaderState >= ReaderState.Metadata;
    {
      uint paramStartRow = this.MethodTable.GetParamStart(methodDefRowId);
      if (methodDefRowId == this.MethodTable.NumberOfRows) {
        paramRowCount = (this.UseParamPtrTable ? this.ParamPtrTable.NumberOfRows : this.ParamTable.NumberOfRows) - paramStartRow + 1;
      } else {
        paramRowCount = this.MethodTable.GetParamStart(methodDefRowId + 1) - paramStartRow;
      }
      return paramStartRow;
    }
    #endregion Methods [Metadata]

    internal static SEHTableEntry[] GetSmallSEHEntries(
      MemoryReader memReader,
      int numEntries
    ) {
      SEHTableEntry[] retSEHEntries = new SEHTableEntry[numEntries];
      for (int i = 0; i < numEntries; ++i) {
        SEHFlags sehFlags = (SEHFlags)memReader.ReadUInt16();
        uint tryOffset = memReader.ReadUInt16();
        uint tryLength = memReader.ReadByte();
        uint handlerOffset = memReader.ReadUInt16();
        uint handlerLength = memReader.ReadByte();
        uint classTokenOrFilterOffset = memReader.ReadUInt32();
        retSEHEntries[i] = new SEHTableEntry(sehFlags, tryOffset, tryLength, handlerOffset, handlerLength, classTokenOrFilterOffset);
      }
      return retSEHEntries;
    }

    internal static SEHTableEntry[] GetFatSEHEntries(
      MemoryReader memReader,
      int numEntries
    ) {
      SEHTableEntry[] retSEHEntries = new SEHTableEntry[numEntries];
      for (int i = 0; i < numEntries; ++i) {
        SEHFlags sehFlags = (SEHFlags)memReader.ReadUInt32();
        uint tryOffset = memReader.ReadUInt32();
        uint tryLength = memReader.ReadUInt32();
        uint handlerOffset = memReader.ReadUInt32();
        uint handlerLength = memReader.ReadUInt32();
        uint classTokenOrFilterOffset = memReader.ReadUInt32();
        retSEHEntries[i] = new SEHTableEntry(sehFlags, tryOffset, tryLength, handlerOffset, handlerLength, classTokenOrFilterOffset);
      }
      return retSEHEntries;
    }

    internal MethodIL/*?*/ GetMethodIL(uint methodDefRowId) {
      MethodRow methodRow = this.MethodTable[methodDefRowId];
      if ((methodRow.ImplFlags & MethodImplFlags.CodeTypeMask) != MethodImplFlags.ILCodeType || methodRow.RVA == 0) {
        return null;
      }
      if (this.SectionHeaders.Length == 0) return null; //reading a snapshot
      MemoryBlock memBlock = this.RVAToMemoryBlock(methodRow.RVA);
      //  Error need to check if the Memory Block is empty. This is calse for all the calls...
      MemoryReader memReader = new MemoryReader(memBlock);
      byte headByte = memReader.ReadByte();
      if ((headByte & CILMethodFlags.ILFormatMask) == CILMethodFlags.ILTinyFormat) {
        int size = headByte >> CILMethodFlags.ILTinyFormatSizeShift;
        return new MethodIL(
          true,
          8,
          0x00000000,
          memReader.GetMemoryBlockAt(0, size),
          null
        );
      } else if ((headByte & CILMethodFlags.ILFormatMask) != CILMethodFlags.ILFatFormat) {
        //  PEFileFormat Error...
        return null;
      }
      //  FatILFormat
      byte headByte2 = memReader.ReadByte();
      if ((headByte2 >> CILMethodFlags.ILFatFormatHeaderSizeShift) != CILMethodFlags.ILFatFormatHeaderSize) {
        //  PEFile Format Error...
        return null;
      }
      bool localVarInited = (headByte & CILMethodFlags.ILInitLocals) == CILMethodFlags.ILInitLocals;
      bool moreSectsPresent = (headByte & CILMethodFlags.ILMoreSects) == CILMethodFlags.ILMoreSects;
      ushort maxStack = memReader.ReadUInt16();
      int codeSize = memReader.ReadInt32();
      uint localSignatureToken = memReader.ReadUInt32();
      MemoryBlock ilCodeMemBlock = memReader.GetMemoryBlockAt(0, codeSize);
      SEHTableEntry[]/*?*/ sehTableEntries = null;
      if (moreSectsPresent) {
        memReader.SkipBytes(codeSize);
        memReader.Align(4);
        byte sectHeader = memReader.ReadByte();
        if ((sectHeader & CILMethodFlags.SectEHTable) != CILMethodFlags.SectEHTable) {
          //  PEFile Format Error...
          return null;
        }
        bool sectFatFormat = (sectHeader & CILMethodFlags.SectFatFormat) == CILMethodFlags.SectFatFormat;
        int dataSize = memReader.ReadByte();
        if (sectFatFormat) {
          dataSize += (int)memReader.ReadUInt16() << 8;
          sehTableEntries = PEFileReader.GetFatSEHEntries(memReader, dataSize / 24);
        } else {
          memReader.SkipBytes(2); //skip over reserved field
          sehTableEntries = PEFileReader.GetSmallSEHEntries(memReader, dataSize / 12);
        }
      }
      return new MethodIL(
        localVarInited,
        maxStack,
        localSignatureToken,
        ilCodeMemBlock,
        sehTableEntries
      );
    }
  }
}
