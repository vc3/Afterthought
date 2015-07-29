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
using System.Diagnostics.Contracts;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  public sealed class PdbWriter : IUnmanagedPdbWriter {

    string fileName;
    ISourceLocationProvider sourceLocationProvider;
    uint currentMethodToken;
    ISymUnmanagedWriter5/*?*/ symWriter5;
    bool emitTokenSourceInfo;

    public PdbWriter(string fileName, ISourceLocationProvider sourceLocationProvider, bool emitTokenSourceInfo = false) {
      this.fileName = fileName;
      this.sourceLocationProvider = sourceLocationProvider;
      this.emitTokenSourceInfo = emitTokenSourceInfo;
    }

    public void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    ~PdbWriter() {
      this.Close();
    }

    private void Close() {
      if (this.symWriter != null)
      {
        try
        {
          this.symWriter.Close();
        }
        catch
        {
          // TODO: warn about failed pdb write. 
        }
      }
       
    }

    public void CloseMethod(uint offset) {
      if (this.currentDocument != null && this.currentDocument != SourceDummy.PrimarySourceDocument)
        this.DefineSequencePointsForCurrentDocument();
      this.SymWriter.CloseScope(offset);
      this.SymWriter.CloseMethod();
    }

    public void CloseScope(uint offset) {
      this.SymWriter.CloseScope(offset);
    }

    public void CloseTokenSourceLocationsScope() {
      if (this.symWriter5 != null)
        this.symWriter5.CloseMapTokensToSourceSpans();
      this.symWriter5 = null;
    }

    public void DefineTokenSourceLocation(uint token, ILocation location) {
      if (this.symWriter5 == null) return;
      IPrimarySourceLocation ploc = null;
      foreach (IPrimarySourceLocation psloc in this.sourceLocationProvider.GetPrimarySourceLocationsFor(location)) {
        ploc = psloc;
        break;
      }
      if (ploc == null) return;
      ISymUnmanagedDocumentWriter document = this.GetDocumentWriterFor(ploc.PrimarySourceDocument);
      this.symWriter5.MapTokenToSourceSpan(token, document, (uint)ploc.StartLine, (uint)ploc.StartColumn, (uint)ploc.EndLine, (uint)ploc.EndColumn);
    }

    public void OpenTokenSourceLocationsScope() {
      if (this.symWriter5 != null)
        this.symWriter5.OpenMapTokensToSourceSpans();
    }

    public unsafe void DefineCustomMetadata(string name, byte[] metadata) {
      fixed (byte* pb = metadata) {
        this.SymWriter.SetSymAttribute(this.currentMethodToken, name, (uint)metadata.Length, (IntPtr)pb);
      }
    }

    public void DefineLocalConstant(string name, object value, uint contantSignatureToken) {
      this.symWriter.DefineConstant2(name, value, contantSignatureToken);
    }

    public void DefineLocalVariable(uint index, string name, bool isCompilerGenerated, uint localVariablesSignatureToken) {
      uint attributes = isCompilerGenerated ? 1u : 0u;
      this.SymWriter.DefineLocalVariable2(name, attributes, localVariablesSignatureToken, 1, index, 0, 0, 0, 0);
    }

    public void DefineSequencePoint(ILocation location, uint offset) {
      IPrimarySourceLocation ploc = null;
      foreach (IPrimarySourceLocation psloc in this.sourceLocationProvider.GetPrimarySourceLocationsFor(location)) {
        ploc = psloc;
        break;
      }
      if (ploc == null) return;
      if (ploc.Document != this.currentDocument && this.currentDocument != null && ploc.Document != SourceDummy.PrimarySourceDocument)
        this.DefineSequencePointsForCurrentDocument();
      if (ploc.Document != SourceDummy.PrimarySourceDocument)
        this.currentDocument = ploc.PrimarySourceDocument;
      this.offsets.Add(offset);
      this.startLines.Add((uint)ploc.StartLine);
      this.startColumns.Add((uint)ploc.StartColumn);
      this.endLines.Add((uint)ploc.EndLine);
      this.endColumns.Add((uint)ploc.EndColumn);
    }

    /// <summary>
    /// Null is the sentinel value because the document of a FeeFee source context
    /// is the dummy document which must not be confused with the sentinel value.
    /// It stays null until the first non-FeeFee source context is encountered.
    /// It is updated only when another context is encountered that has a
    /// different document *and* is not a FeeFee source context itself.
    /// </summary>
    IPrimarySourceDocument/*?*/ currentDocument = null;
    List<uint> offsets = new List<uint>();
    List<uint> startLines = new List<uint>();
    List<uint> startColumns = new List<uint>();
    List<uint> endLines = new List<uint>();
    List<uint> endColumns = new List<uint>();

    /// <summary>
    /// Flushes accumulated sequence points and re-initializes sequence point state.
    /// </summary>
    private void DefineSequencePointsForCurrentDocument() {
      //^ requires this.currentDocument != null && this.currentDocument != SourceDummy.PrimarySourceDocument
      ISymUnmanagedDocumentWriter document = this.GetDocumentWriterFor(this.currentDocument);
      uint seqPointCount = (uint)this.offsets.Count;
      if (seqPointCount > 0) {
        uint[] offsets = this.offsets.ToArray();
        uint[] startLines = this.startLines.ToArray();
        uint[] startColumns = this.startColumns.ToArray();
        uint[] endLines = this.endLines.ToArray();
        uint[] endColumns = this.endColumns.ToArray();
        this.SymWriter.DefineSequencePoints(document, seqPointCount, offsets, startLines, startColumns, endLines, endColumns);
      }
      this.currentDocument = null;
      this.offsets.Clear();
      this.startLines.Clear();
      this.startColumns.Clear();
      this.endLines.Clear();
      this.endColumns.Clear();
    }

    private ISymUnmanagedDocumentWriter GetDocumentWriterFor(IPrimarySourceDocument document) {
      Contract.Requires(document != null);
      Contract.Requires(document != SourceDummy.PrimarySourceDocument);
      
      ISymUnmanagedDocumentWriter writer;
      if (!this.documentMap.TryGetValue(document, out writer)) {
        Guid language = document.Language;
        Guid vendor = document.LanguageVendor;
        Guid type = document.DocumentType;
        writer = this.SymWriter.DefineDocument(document.Location, ref language, ref vendor, ref type);
        this.documentMap.Add(document, writer);
      }
      return writer;
    }

    Dictionary<IPrimarySourceDocument, ISymUnmanagedDocumentWriter> documentMap = new Dictionary<IPrimarySourceDocument, ISymUnmanagedDocumentWriter>();

    public unsafe PeDebugDirectory GetDebugDirectory() {
      ImageDebugDirectory debugDir = new ImageDebugDirectory();
      uint pcData = 0;
      this.SymWriter.GetDebugInfo(ref debugDir, 0, out pcData, IntPtr.Zero);
      byte[] data = new byte[pcData];
      fixed (byte* pb = data) {
        this.SymWriter.GetDebugInfo(ref debugDir, pcData, out pcData, (IntPtr)pb);
      }
      PeDebugDirectory result = new PeDebugDirectory();
      result.AddressOfRawData = (uint)debugDir.AddressOfRawData;
      result.Characteristics = (uint)debugDir.Characteristics;
      result.Data = data;
      result.MajorVersion = (ushort)debugDir.MajorVersion;
      result.MinorVersion = (ushort)debugDir.MinorVersion;
      result.PointerToRawData = (uint)debugDir.PointerToRawData;
      result.SizeOfData = (uint)debugDir.SizeOfData;
      result.TimeDateStamp = (uint)debugDir.TimeDateStamp;
      result.Type = (uint)debugDir.Type;
      return result;
    }

    public void OpenMethod(uint methodToken) {
      this.currentMethodToken = methodToken;
      this.SymWriter.OpenMethod(methodToken);
      this.SymWriter.OpenScope(0);
    }

    public void OpenScope(uint offset) {
      this.SymWriter.OpenScope(offset);
    }

    public void SetEntryPoint(uint entryMethodToken) {
      this.SymWriter.SetUserEntryPoint(entryMethodToken);
    }

    public void SetMetadataEmitter(object metadataEmitter) {
      Type t = Type.GetTypeFromProgID("CorSymWriter_SxS", false);
      if (t != null) {
        this.symWriter = (ISymUnmanagedWriter2)Activator.CreateInstance(t);
        this.symWriter.Initialize(metadataEmitter, this.fileName, null, true);
        if (this.emitTokenSourceInfo)
          this.symWriter5 = this.symWriter as ISymUnmanagedWriter5;
      }
    }

    ISymUnmanagedWriter2 SymWriter {
      get {
        //^ assume this.symWriter != null;
        return this.symWriter;
      }
    }
    ISymUnmanagedWriter2/*?*/ symWriter;

    public void UsingNamespace(string fullName) {
      this.SymWriter.UsingNamespace(fullName);
    }

  }
}

