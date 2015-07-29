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
using Microsoft.Cci;
using System.Runtime.InteropServices;
using System.Security;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006")]
  internal interface ISymUnmanagedDocumentWriter {
    void SetSource(uint sourceSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] source);
    void SetCheckSum(ref Guid algorithmId, uint checkSumSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] checkSum);
  };

  [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0B97726E-9E6D-4f05-9A26-424022093CAA")]
  internal interface ISymUnmanagedWriter2 {
    ISymUnmanagedDocumentWriter DefineDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);
    void SetUserEntryPoint(uint entryMethod);
    void OpenMethod(uint method);
    void CloseMethod();
    uint OpenScope(uint startOffset);
    void CloseScope(uint endOffset);
    void SetScopeRange(uint scopeID, uint startOffset, uint endOffset);
    void DefineLocalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint startOffset, uint endOffset);
    void DefineParameter(string name, uint attributes, uint sequence, uint addrKind, uint addr1, uint addr2, uint addr3);
    void DefineField(uint parent, string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
    void DefineGlobalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
    void Close();
    void SetSymAttribute(uint parent, string name, uint cData, IntPtr signature);
    void OpenNamespace(string name);
    void CloseNamespace();
    void UsingNamespace(string fullName);
    void SetMethodSourceRange(ISymUnmanagedDocumentWriter startDoc, uint startLine, uint startColumn, object endDoc, uint endLine, uint endColumn);
    void Initialize([MarshalAs(UnmanagedType.IUnknown)]object emitter, string filename, [MarshalAs(UnmanagedType.IUnknown)]object pIStream, bool fFullBuild);
    void GetDebugInfo(ref ImageDebugDirectory pIDD, uint cData, out uint pcData, IntPtr data);
    void DefineSequencePoints(ISymUnmanagedDocumentWriter document, uint spCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] offsets,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] lines,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] columns,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] endLines,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] endColumns);
    void RemapToken(uint oldToken, uint newToken);
    void Initialize2([MarshalAs(UnmanagedType.IUnknown)]object emitter, string tempfilename, [MarshalAs(UnmanagedType.IUnknown)]object pIStream, bool fFullBuild, string finalfilename);
    void DefineConstant(string name, object value, uint cSig, IntPtr signature);
    void Abort();
    void DefineLocalVariable2(string name, uint attributes, uint sigToken, uint addrKind, uint addr1, uint addr2, uint addr3, uint startOffset, uint endOffset);
    void DefineGlobalVariable2(string name, uint attributes, uint sigToken, uint addrKind, uint addr1, uint addr2, uint addr3);
    void DefineConstant2(string name, object value, uint sigToken);
  }

  [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("DCF7780D-BDE9-45DF-ACFE-21731A32000C")]
  interface ISymUnmanagedWriter5  {
    ISymUnmanagedDocumentWriter DefineDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);
    void SetUserEntryPoint(uint entryMethod);
    void OpenMethod(uint method);
    void CloseMethod();
    uint OpenScope(uint startOffset);
    void CloseScope(uint endOffset);
    void SetScopeRange(uint scopeID, uint startOffset, uint endOffset);
    void DefineLocalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint startOffset, uint endOffset);
    void DefineParameter(string name, uint attributes, uint sequence, uint addrKind, uint addr1, uint addr2, uint addr3);
    void DefineField(uint parent, string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
    void DefineGlobalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
    void Close();
    void SetSymAttribute(uint parent, string name, uint cData, IntPtr signature);
    void OpenNamespace(string name);
    void CloseNamespace();
    void UsingNamespace(string fullName);
    void SetMethodSourceRange(ISymUnmanagedDocumentWriter startDoc, uint startLine, uint startColumn, object endDoc, uint endLine, uint endColumn);
    void Initialize([MarshalAs(UnmanagedType.IUnknown)]object emitter, string filename, [MarshalAs(UnmanagedType.IUnknown)]object pIStream, bool fFullBuild);
    void GetDebugInfo(ref ImageDebugDirectory pIDD, uint cData, out uint pcData, IntPtr data);
    void DefineSequencePoints(ISymUnmanagedDocumentWriter document, uint spCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] offsets,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] lines,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] columns,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] endLines,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] endColumns);
    void RemapToken(uint oldToken, uint newToken);
    void Initialize2([MarshalAs(UnmanagedType.IUnknown)]object emitter, string tempfilename, [MarshalAs(UnmanagedType.IUnknown)]object pIStream, bool fFullBuild, string finalfilename);
    void DefineConstant(string name, object value, uint cSig, IntPtr signature);
    void Abort();
    void DefineLocalVariable2(string name, uint attributes, uint sigToken, uint addrKind, uint addr1, uint addr2, uint addr3, uint startOffset, uint endOffset);
    void DefineGlobalVariable2(string name, uint attributes, uint sigToken, uint addrKind, uint addr1, uint addr2, uint addr3);
    void DefineConstant2(string name, object value, uint sigToken);

    void OpenMethod2(uint method, uint isect, uint offset);
    void Commit();
    /*
     * Functions the same as ISymUnmanagedWriter::GetDebugInfo with the exception
     * that the path string is padded with zeros following the terminating null
     * character to make the string data a fixed size of MAX_PATH. Padding is only
     * given if the path string length itself is less than MAX_PATH.
     *
     * This makes writing tools that difference PE files easier.
     */
    void GetDebugInfoWithPadding(ref ImageDebugDirectory pIDD, uint cData, out uint pcData, IntPtr data);
    
    void OpenMapTokensToSourceSpans();
    void CloseMapTokensToSourceSpans();
    /// <summary>
    /// Maps the given metadata token to the given source line span in the specified source file. 
    /// Must be called between calls to OpenMapTokensToSourceSpans() and CloseMapTokensToSourceSpans().
    /// </summary>
    void MapTokenToSourceSpan(uint token, ISymUnmanagedDocumentWriter document, uint line, uint column, uint endLine, uint endColumn);

  }

  internal struct ImageDebugDirectory {
    internal int Characteristics;
    internal int TimeDateStamp;
    internal short MajorVersion;
    internal short MinorVersion;
    internal int Type;
    internal int SizeOfData;
    internal int AddressOfRawData;
    internal int PointerToRawData;

    //only here to shut up warnings
    internal ImageDebugDirectory(object dummy) {
      this.Characteristics = 0;
      this.TimeDateStamp = 0;
      this.MajorVersion = 0;
      this.MinorVersion = 0;
      this.Type = 0;
      this.SizeOfData = 0;
      this.AddressOfRawData = 0;
      this.PointerToRawData = 0;
    }

  }

}