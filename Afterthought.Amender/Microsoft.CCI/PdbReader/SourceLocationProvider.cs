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
using System.Diagnostics.SymbolStore;
using System.IO;
using Microsoft.Cci.MetadataReader;

namespace Microsoft.Cci {
  using Microsoft.Cci.Pdb;
  using System.Diagnostics.Contracts;

  /// <summary>
  /// An object that can map offsets in an IL stream to source locations and block scopes.
  /// </summary>
  public sealed class PdbReader : ISourceLocationProvider, ILocalScopeProvider, IDisposable {

    Stream pdbStream;
    IMetadataHost host;
    Dictionary<uint, PdbFunction> pdbFunctionMap = new Dictionary<uint, PdbFunction>();
    List<StreamReader> sourceFilesOpenedByReader = new List<StreamReader>();
    Dictionary<uint, PdbTokenLine> tokenToSourceMapping;
    string sourceServerData;

    /// <summary>
    /// Allocates an object that can map some kinds of ILocation objects to IPrimarySourceLocation objects. 
    /// For example, a PDB reader that maps offsets in an IL stream to source locations.
    /// </summary>
    public PdbReader(Stream pdbStream, IMetadataHost host) {
      Contract.Requires(pdbStream != null);
      Contract.Requires(host != null);

      this.pdbStream = pdbStream;
      this.host = host;
      foreach (PdbFunction pdbFunction in PdbFile.LoadFunctions(pdbStream, out this.tokenToSourceMapping, out this.sourceServerData))
        this.pdbFunctionMap[pdbFunction.token] = pdbFunction;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.pdbStream != null);
      Contract.Invariant(this.host != null);
      Contract.Invariant(this.pdbFunctionMap != null);
      Contract.Invariant(this.sourceFilesOpenedByReader != null);
      Contract.Invariant(this.sourceServerData != null);
    }


    /// <summary>
    /// Closes all of the source files that have been opened to provide the contents source locations corresponding to IL offsets.
    /// </summary>
    public void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Closes all of the source files that have been opened to provide the contents source locations corresponding to IL offsets.
    /// </summary>
    ~PdbReader() {
      this.Close();
    }

    private void Close() {
      foreach (var source in this.sourceFilesOpenedByReader)
        source.Dispose();
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to one or more of the given derived (non primary) document locations.
    /// </summary>
    /// <param name="locations">Zero or more locations in documents that have been derived from one or more source documents.</param>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IEnumerable<ILocation> locations) {
      foreach (ILocation location in locations) {
        IPrimarySourceLocation/*?*/ psloc = location as IPrimarySourceLocation;
        if (psloc != null) yield return psloc;
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          psloc = this.MapMethodBodyLocationToSourceLocation(mbLocation, true);
          if (psloc != null) yield return psloc;
        } else {
          var mdLocation = location as IMetadataLocation;
          if (mdLocation != null) {
            PdbTokenLine lineInfo;
            if (!this.tokenToSourceMapping.TryGetValue(mdLocation.Definition.TokenValue, out lineInfo)) yield break;
            PdbSourceDocument psDoc = this.GetPrimarySourceDocumentFor(lineInfo.sourceFile);
            yield return new PdbSourceLineLocation(psDoc, (int)lineInfo.line, (int)lineInfo.column, (int)lineInfo.endLine, (int)lineInfo.endColumn);
          }
        }
      }
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that are the closest to corresponding to one or more of the given derived (non primary) document locations.
    /// </summary>
    /// <param name="locations">Zero or more locations in documents that have been derived from one or more source documents.</param>
    public IEnumerable<IPrimarySourceLocation> GetClosestPrimarySourceLocationsFor(IEnumerable<ILocation> locations) {
      foreach (ILocation location in locations) {
        IPrimarySourceLocation/*?*/ psloc = location as IPrimarySourceLocation;
        if (psloc != null) yield return psloc;
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          psloc = this.MapMethodBodyLocationToSourceLocation(mbLocation, false);
          if (psloc != null) yield return psloc;
        }
      }
    }


    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the given derived (non primary) document location.
    /// </summary>
    /// <param name="location">A location in a document that have been derived from one or more source documents.</param>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(ILocation location) {
      var psloc = location as IPrimarySourceLocation;
      if (psloc != null)
        yield return psloc;
      else {
        var mbLocation = location as IILLocation;
        if (mbLocation != null) {
          psloc = this.MapMethodBodyLocationToSourceLocation(mbLocation, true);
          if (psloc != null) yield return psloc;
        } else {
          var mdLocation = location as IMetadataLocation;
          if (mdLocation != null) {
            PdbTokenLine lineInfo;
            if (!this.tokenToSourceMapping.TryGetValue(mdLocation.Definition.TokenValue, out lineInfo)) yield break;
            do {
              PdbSourceDocument psDoc = this.GetPrimarySourceDocumentFor(lineInfo.sourceFile);
              yield return new PdbSourceLineLocation(psDoc, (int)lineInfo.line, (int)lineInfo.column, (int)lineInfo.endLine, (int)lineInfo.endColumn);
              lineInfo = lineInfo.nextLine;
            } while (lineInfo != null);
          }
        }
      }
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that are the closest to corresponding to the given derived (non primary) document location.
    /// </summary>
    /// <param name="location">A location in a document that have been derived from one or more source documents.</param>
    public IEnumerable<IPrimarySourceLocation> GetClosestPrimarySourceLocationsFor(ILocation location) {
      var psloc = location as IPrimarySourceLocation;
      if (psloc != null)
        yield return psloc;
      else {
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          psloc = this.MapMethodBodyLocationToSourceLocation(mbLocation, false);
          if (psloc != null) yield return psloc;
        }
      }
    }

    /// <summary>
    /// Returns zero or more locations in primary source documents that correspond to the definition with the given token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsForToken(uint token) {
      PdbTokenLine lineInfo;
      if (!this.tokenToSourceMapping.TryGetValue(token, out lineInfo)) yield break;
      PdbSourceDocument psDoc = this.GetPrimarySourceDocumentFor(lineInfo.sourceFile);
      yield return new PdbSourceLineLocation(psDoc, (int)lineInfo.line, (int)lineInfo.column, (int)lineInfo.endLine, (int)lineInfo.endColumn);
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the definition of the given local.
    /// </summary>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsForDefinitionOf(ILocalDefinition localDefinition) {
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(localDefinition);
      if (pdbFunction != null) {
        uint index = 0;
        foreach (ILocation location in localDefinition.Locations) {
          IILLocation/*?*/ mbLocation = location as IILLocation;
          if (mbLocation != null) {
            index = mbLocation.Offset;
            break;
          }
        }
        PdbSlot/*?*/ slot = this.GetSlotFor(pdbFunction.scopes, index);
        if (slot != null && (slot.flags & 0x4) == 0)
          return IteratorHelper.GetSingletonEnumerable<IPrimarySourceLocation>(new LocalNameSourceLocation(slot.name));
      }
      return Enumerable<IPrimarySourceLocation>.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="localDefinition"></param>
    /// <param name="isCompilerGenerated"></param>
    /// <returns></returns>
    public string GetSourceNameFor(ILocalDefinition localDefinition, out bool isCompilerGenerated) {
      isCompilerGenerated = true;
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(localDefinition);
      if (pdbFunction != null) {
        uint index = 0;
        foreach (ILocation location in localDefinition.Locations) {
          IILLocation/*?*/ mbLocation = location as IILLocation;
          if (mbLocation != null) {
            index = mbLocation.Offset;
            break;
          }
        }
        PdbSlot/*?*/ slot = this.GetSlotFor(pdbFunction.scopes, index);
        if (slot != null) {
          isCompilerGenerated = (slot.flags & 0x4) != 0;
          if (!isCompilerGenerated)
            isCompilerGenerated = slot.name.StartsWith("<");
          return slot.name;
        }
      }
      return localDefinition.Name.Value;
    }

    private PdbSlot/*?*/ GetSlotFor(PdbScope[] pdbScopes, uint index) {
      PdbSlot/*?*/ result = null;
      foreach (PdbScope scope in pdbScopes) {
        foreach (PdbSlot slot in scope.slots) {
          if ((slot.flags & 1) != 0) continue;
          if (slot.slot == index) return slot;
        }
        result = this.GetSlotFor(scope.scopes, index);
        if (result != null) return result;
      }
      return result;
    }

    /// <summary>
    /// Returns zero or more local (block) scopes, each defining an IL range in which an iterator local is defined.
    /// The scopes are returned by the MoveNext method of the object returned by the iterator method.
    /// The index of the scope corresponds to the index of the local. Specifically local scope i corresponds
    /// to the local stored in field &lt;localName&gt;x_i of the class used to store the local values in between
    /// calls to MoveNext.
    /// </summary>
    public IEnumerable<ILocalScope> GetIteratorScopes(IMethodBody methodBody) {
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(methodBody);
      if (pdbFunction == null || pdbFunction.iteratorScopes == null)
        return Enumerable<ILocalScope>.Empty;
      foreach (var i in pdbFunction.iteratorScopes) {
        PdbIteratorScope pis = i as PdbIteratorScope;
        if (pis != null)
          pis.MethodDefinition = methodBody.MethodDefinition;
      }
      return pdbFunction.iteratorScopes.AsReadOnly();
    }

    /// <summary>
    /// Returns zero or more lexical scopes into which the CLR IL operations in the given method body is organized.
    /// </summary>
    public IEnumerable<ILocalScope> GetLocalScopes(IMethodBody methodBody) {
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(methodBody);
      if (pdbFunction == null) return Enumerable<ILocalScope>.Empty;
      List<ILocalScope> scopes = new List<ILocalScope>();
      this.FillInScopesAndSubScopes(methodBody, pdbFunction.scopes, scopes);
      return scopes.AsReadOnly();
    }

    private PdbFunction/*?*/ GetPdbFunctionFor(IMethodBody methodBody) {
      PdbFunction result = null;
      uint methodToken = GetTokenFor(methodBody);
      this.pdbFunctionMap.TryGetValue(methodToken, out result);
      return result;
    }

    private static uint GetTokenFor(IMethodBody methodBody) {
      foreach (ILocation location in methodBody.MethodDefinition.Locations) {
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          var doc = mbLocation.Document as MethodBodyDocument;
          if (doc != null) return doc.MethodToken;
        }
      }
      return 0;
    }

    private static ITokenDecoder/*?*/ GetTokenDecoderFor(IMethodBody methodBody, out uint methodToken) {
      foreach (ILocation location in methodBody.MethodDefinition.Locations) {
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          var doc = mbLocation.Document as MethodBodyDocument;
          if (doc != null) { methodToken = doc.MethodToken; return doc.TokenDecoder; }
        }
      }
      methodToken = 0;
      return null;
    }

    private PdbFunction GetPdbFunctionFor(ILocalDefinition localDefinition) {
      PdbFunction/*?*/ result = null;
      foreach (ILocation location in localDefinition.Locations) {
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          var doc = mbLocation.Document as MethodBodyDocument;
          if (doc != null) {
            this.pdbFunctionMap.TryGetValue(doc.MethodToken, out result);
            break;
          }
        }
      }
      return result;
    }

    private void FillInScopesAndSubScopes(IMethodBody methodBody, PdbScope[] pdbScopes, List<ILocalScope> scopes) {
      foreach (PdbScope scope in pdbScopes) {
        scopes.Add(new PdbLocalScope(methodBody, scope));
        this.FillInScopesAndSubScopes(methodBody, scope.scopes, scopes);
      }
    }

    /// <summary>
    /// Returns zero or more local constant definitions that are local to the given scope.
    /// </summary>
    public IEnumerable<ILocalDefinition> GetConstantsInScope(ILocalScope scope) {
      PdbLocalScope/*?*/ pdbLocalScope = scope as PdbLocalScope;
      if (pdbLocalScope == null) yield break;
      foreach (PdbConstant constant in pdbLocalScope.pdbScope.constants) {
        yield return new PdbLocalConstant(constant, this.host, pdbLocalScope.methodBody.MethodDefinition);
      }
    }

    /// <summary>
    /// Returns zero or more local variable definitions that are local to the given scope.
    /// </summary>
    public IEnumerable<ILocalDefinition> GetVariablesInScope(ILocalScope scope) {
      PdbLocalScope/*?*/ pdbLocalScope = scope as PdbLocalScope;
      if (pdbLocalScope == null) yield break;
      foreach (PdbSlot slot in pdbLocalScope.pdbScope.slots) {
        if ((slot.flags & 1) != 0) continue;
        if (slot.slot == 0 && slot.name.StartsWith("$VB")) 
          yield return new PdbLocalVariable(slot, this.host, pdbLocalScope.methodBody.MethodDefinition);
        uint index = 0;
        foreach (ILocalDefinition localDefinition in pdbLocalScope.methodBody.LocalVariables) {
          if (localDefinition.IsConstant) continue;
          if (slot.slot == index) {
            yield return localDefinition;
            break;
          }
          index++;
        }
      }
    }

    /// <summary>
    /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
    /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
    /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
    /// is for the y.
    /// </summary>
    public IEnumerable<INamespaceScope> GetNamespaceScopes(IMethodBody methodBody) {
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(methodBody);
      if (pdbFunction != null) {
        if (pdbFunction.usingCounts != null) {
          if (pdbFunction.namespaceScopes != null) return pdbFunction.namespaceScopes;
          foreach (PdbScope pdbScope in pdbFunction.scopes) {
            return pdbFunction.namespaceScopes = this.GetNamespaceScopes(pdbFunction.usingCounts, pdbScope).AsReadOnly();
          }
        } else if (pdbFunction.tokenOfMethodWhoseUsingInfoAppliesToThisMethod > 0) {
          PdbFunction func = null;
          if (this.pdbFunctionMap.TryGetValue(pdbFunction.tokenOfMethodWhoseUsingInfoAppliesToThisMethod, out func)) {
            if (func.usingCounts != null) {
              if (func.namespaceScopes != null) return func.namespaceScopes;
              foreach (PdbScope pdbScope in func.scopes) {
                return func.namespaceScopes = this.GetNamespaceScopes(func.usingCounts, pdbScope).AsReadOnly();
              }
            }
          }
        }
      }
      return Enumerable<INamespaceScope>.Empty;
    }

    private List<INamespaceScope> GetNamespaceScopes(ushort[] usingCounts, PdbScope pdbScope) {
      int usedNamespaceCount = 0;
      int numScopes = usingCounts.Length;
      List<INamespaceScope> result = new List<INamespaceScope>(numScopes);
      for (int i = 0; i < numScopes; i++) {
        int numUsings = usingCounts[i];
        List<IUsedNamespace> usings = new List<IUsedNamespace>(numUsings);
        while (numUsings-- > 0 && usedNamespaceCount < pdbScope.usedNamespaces.Length) {
          usings.Add(this.GetUsedNamespace(pdbScope.usedNamespaces[usedNamespaceCount++]));
        }
        result.Add(new NamespaceScope(usings.AsReadOnly()));
      }
      return result;
    }

    private IUsedNamespace GetUsedNamespace(string namespaceName) {
      if (namespaceName.Length > 0 && namespaceName[0] == 'A') {
        string[] parts = namespaceName.Split(' ');
        if (parts.Length == 2) {
          IName alias = this.host.NameTable.GetNameFor(parts[0].Substring(1));
          IName nsName = this.host.NameTable.GetNameFor(parts[1].Substring(1));
          return new UsedNamespace(alias, nsName);
        }
      } else if (namespaceName.Length > 0 && namespaceName[0] == 'U') {
        IName nsName = this.host.NameTable.GetNameFor(namespaceName.Substring(1));
        return new UsedNamespace(this.host.NameTable.EmptyName, nsName);
      }
      return new UsedNamespace(this.host.NameTable.EmptyName, this.host.NameTable.GetNameFor(namespaceName));
    }

    /// <summary>
    /// Returns true if the method body is an iterator, in which case the scope information should be retrieved from the object
    /// returned by the method.
    /// </summary>
    public bool IsIterator(IMethodBody methodBody) {
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(methodBody);
      return pdbFunction != null && pdbFunction.iteratorClass != null;
    }

    /// <summary>
    /// If the given method body is the "MoveNext" method of the state class of an asynchronous method, the returned
    /// object describes where synchronization points occur in the IL operations of the "MoveNext" method. Otherwise
    /// the result is null.
    /// </summary>
    public ISynchronizationInformation/*?*/ GetSynchronizationInformation(IMethodBody methodBody) {
      PdbFunction/*?*/ pdbFunction = this.GetPdbFunctionFor(methodBody);
      if (pdbFunction == null) return null;
      var info = pdbFunction.synchronizationInformation;
      if (info == null) return null;
      uint moveNextToken;
      var decoder = GetTokenDecoderFor(methodBody, out moveNextToken);
      if (decoder != null) {
        info.asyncMethod = (decoder.GetObjectForToken(info.kickoffMethodToken) as IMethodDefinition)??Dummy.MethodDefinition;
        info.moveNextMethod = (decoder.GetObjectForToken(moveNextToken) as IMethodDefinition)??Dummy.MethodDefinition;
        if (info.synchronizationPoints != null) {
          foreach (var synchPoint in info.synchronizationPoints)
            synchPoint.continuationMethod = decoder.GetObjectForToken(synchPoint.continuationMethodToken) as IMethodDefinition;
        }
      }
      return info;
    }

    /// <summary>
    /// Returns the location that has the smaller IL offset. If only one of the two locations
    /// is a PdbReader supplied location that one is returned. If neither is a PdbReader supplied location, the first
    /// location is returned.
    /// </summary>
    /// <param name="location1">A document location. Typically one obtained from the PdbReader.</param>
    /// <param name="location2">A document location. Typically one obtained from the PdbReader.</param>
    public ILocation LocationWithSmallerOffset(ILocation location1, ILocation location2) {
      var smbl = location2 as IILLocation;
      if (smbl == null) return location1;
      var tmbl = location1 as IILLocation;
      if (tmbl == null || tmbl.Offset > smbl.Offset) return location2;
      return location1;
    }

    private IPrimarySourceLocation/*?*/ MapMethodBodyLocationToSourceLocation(IILLocation mbLocation, bool exact) {
      PdbFunction/*?*/ pdbFunction;
      var doc = mbLocation.Document as MethodBodyDocument;
      if (doc == null || !this.pdbFunctionMap.TryGetValue(doc.MethodToken, out pdbFunction)) return null;
      if (pdbFunction.lines == null) return null;
      foreach (PdbLines pdbLines in pdbFunction.lines) {
        PdbSource pdbSourceFile = pdbLines.file;
        if (pdbSourceFile == null) return null;

        PdbLine[] array = pdbLines.lines;
        int minIndex = 0;
        int maxIndex = array.Length - 1;

        uint desiredOffset = mbLocation.Offset;

        while (minIndex <= maxIndex) {
          int midPointIndex = (minIndex + maxIndex) >> 1;
          PdbLine mid = array[midPointIndex];
          if (midPointIndex == maxIndex ||
            (mid.offset <= desiredOffset && desiredOffset < array[midPointIndex + 1].offset)) {
            if (exact && desiredOffset != mid.offset) break;
            PdbLine line = mid;
            PdbSourceDocument psDoc = this.GetPrimarySourceDocumentFor(pdbSourceFile);
            return new PdbSourceLineLocation(psDoc, (int)line.lineBegin, line.colBegin, (int)line.lineEnd, line.colEnd);
          }
          if (mid.offset < desiredOffset)
            minIndex = midPointIndex + 1;
          else
            maxIndex = midPointIndex - 1;
        }
      }
      return null;
    }

    private PdbSourceDocument GetPrimarySourceDocumentFor(PdbSource pdbSourceFile) {
      PdbSourceDocument/*?*/ result = null;
      if (this.documentCache.TryGetValue(pdbSourceFile, out result)) return result;
      IName name = this.host.NameTable.GetNameFor(Path.GetFileName(pdbSourceFile.name));
      if (File.Exists(pdbSourceFile.name)) {
        var sourceFileReader = new StreamReader(pdbSourceFile.name);
        this.sourceFilesOpenedByReader.Add(sourceFileReader);
        result = new PdbSourceDocument(name, pdbSourceFile, sourceFileReader);
      } else
        result = new PdbSourceDocument(name, pdbSourceFile);
      this.documentCache.Add(pdbSourceFile, result);
      return result;
    }

    Dictionary<PdbSource, PdbSourceDocument> documentCache = new Dictionary<PdbSource, PdbSourceDocument>();

    ///<summary>
    /// Retrieves the Source Server Data block, if present.
    /// Otherwise the empty string is returned.
    ///</summary>
    public string SourceServerData {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        return this.sourceServerData;
      }
    }

  }

}

namespace Microsoft.Cci.Pdb {
  internal sealed class UsedNamespace : IUsedNamespace {

    internal UsedNamespace(IName alias, IName namespaceName) {
      this.alias = alias;
      this.namespaceName = namespaceName;
    }

    public IName Alias {
      get { return this.alias; }
    }
    readonly IName alias;

    public IName NamespaceName {
      get { return this.namespaceName; }
    }
    readonly IName namespaceName;

  }

  internal class NamespaceScope : INamespaceScope {

    internal NamespaceScope(IEnumerable<IUsedNamespace> usedNamespaces) {
      this.usedNamespaces = usedNamespaces;
    }

    public IEnumerable<IUsedNamespace> UsedNamespaces {
      get { return this.usedNamespaces; }
    }
    readonly IEnumerable<IUsedNamespace> usedNamespaces;

  }

  internal sealed class LocalNameSourceLocation : IPrimarySourceLocation {

    internal LocalNameSourceLocation(string source) {
      this.source = source;
    }

    #region IPrimarySourceLocation Members

    public int EndColumn {
      get { return 0; }
    }

    public int EndLine {
      get { return 0; }
    }

    public IPrimarySourceDocument PrimarySourceDocument {
      get { return SourceDummy.PrimarySourceDocument; }
    }

    public int StartColumn {
      get { return 0; }
    }

    public int StartLine {
      get { return 0; }
    }

    #endregion

    #region ISourceLocation Members

    public bool Contains(ISourceLocation location) {
      return false;
    }

    public int CopyTo(int offset, char[] destination, int destinationOffset, int length) {
      return 0;
    }

    public int EndIndex {
      get { return 0; }
    }

    public int Length {
      get { return 0; }
    }

    public ISourceDocument SourceDocument {
      get { return this.PrimarySourceDocument; }
    }

    public string Source {
      get { return this.source; }
    }
    readonly string source;

    public int StartIndex {
      get { return 0; }
    }

    #endregion

    #region ILocation Members

    public IDocument Document {
      get { return this.PrimarySourceDocument; }
    }

    #endregion
  }

  /// <summary>
  /// A primary source document that is referenced by a pdb file and that is used to provide source context to lines from compiled CLR modules with
  /// associated PDB files.
  /// </summary>
  internal sealed class PdbSourceDocument : PrimarySourceDocument {

    /// <summary>
    /// Allocates an object that represents a source document, such as file, which is parsed according to the rules of a particular langauge, 
    /// such as C#, to produce an object model.
    /// </summary>
    /// <param name="name">The name of the document. Used to identify the document in user interaction.</param>
    /// <param name="pdbSourceFile">Information about the document, such as its location.</param>
    /// <param name="streamReader">A StreamReader instance whose BaseStream produces the contents of the document.</param>
    internal PdbSourceDocument(IName name, PdbSource pdbSourceFile, StreamReader streamReader)
      : base(name, pdbSourceFile.name, streamReader) {
      this.pdbSourceFile = pdbSourceFile;
    }

    /// <summary>
    /// Allocates an object that represents a source document, such as file, which is parsed according to the rules of a particular langauge, 
    /// such as C#, to produce an object model.
    /// </summary>
    /// <param name="name">The name of the document. Used to identify the document in user interaction.</param>
    /// <param name="pdbSourceFile">Information about the document, such as its location.</param>
    internal PdbSourceDocument(IName name, PdbSource pdbSourceFile)
      : base(name, pdbSourceFile.name, "") {
      this.pdbSourceFile = pdbSourceFile;
    }

    PdbSource pdbSourceFile;

    public override string SourceLanguage {
      get {
        if (this.Language == SymLanguageType.Basic) return "Basic";
        if (this.Language == SymLanguageType.C) return "C";
        if (this.Language == SymLanguageType.Cobol) return "Cobol";
        if (this.Language == SymLanguageType.CPlusPlus) return "C++";
        if (this.Language == SymLanguageType.CSharp) return "C#";
        if (this.Language == SymLanguageType.ILAssembly) return "ILAssembly";
        if (this.Language == SymLanguageType.Java) return "Java";
        if (this.Language == SymLanguageType.JScript) return "JScript";
        if (this.Language == SymLanguageType.MCPlusPlus) return "MC++";
        if (this.Language == SymLanguageType.Pascal) return "Pascal";
        if (this.Language == SymLanguageType.SMC) return "SMC";
        return ""; //TODO: search registry based on file extension
      }
    }

    public override Guid DocumentType {
      get { return this.pdbSourceFile.doctype; }
    }

    public override Guid Language {
      get { return this.pdbSourceFile.language; }
    }

    public override Guid LanguageVendor {
      get { return this.pdbSourceFile.vendor; }
    }

  }

  /// <summary>
  /// A range of source text that corresponds to a source line.
  /// </summary>
  internal sealed class PdbSourceLineLocation : IPrimarySourceLocation {

    /// <summary>
    /// Allocates a range of source text that corresponds to a source line.
    /// </summary>
    internal PdbSourceLineLocation(PdbSourceDocument primarySourceDocument, int startLine, int startColumn, int endLine, int endColumn) {
      this.primarySourceDocument = primarySourceDocument;
      this.startLine = startLine;
      this.startColumn = startColumn;
      this.endLine = endLine;
      this.endColumn = endColumn;
    }

    /// <summary>
    /// The last column in the last line of the range.
    /// </summary>
    public int EndColumn {
      get { return this.endColumn; }
    }
    readonly int endColumn;

    /// <summary>
    /// The last line of the range.
    /// </summary>
    public int EndLine {
      get { return this.endLine; }
    }
    readonly int endLine;

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    public IPrimarySourceDocument PrimarySourceDocument {
      get { return this.primarySourceDocument; }
    }
    readonly PdbSourceDocument primarySourceDocument;

    /// <summary>
    /// The first column in the first line of the range.
    /// </summary>
    public int StartColumn {
      get { return this.startColumn; }
    }
    readonly int startColumn;

    /// <summary>
    /// The first line of the range.
    /// </summary>
    public int StartLine {
      get { return this.startLine; }
    }
    readonly int startLine;

    #region ISourceLocation Members

    bool ISourceLocation.Contains(ISourceLocation location) {
      return this.primarySourceDocument.GetSourceLocation(this.StartIndex, this.Length).Contains(location);
    }

    public int CopyTo(int offset, char[] destination, int destinationOffset, int length) {
      return this.primarySourceDocument.CopyTo(this.StartIndex+offset, destination, destinationOffset, length);
    }

    public int EndIndex {
      get {
        if (this.endIndex == -1)
          this.endIndex = this.primarySourceDocument.ToPosition(this.endLine, this.endColumn);
        return this.endIndex;
      }
    }
    int endIndex = -1;

    public int Length {
      get {
        int result = this.EndIndex - this.StartIndex;
        if (result < 0) result = this.EndColumn - this.StartColumn;
        return result;
      }
    }

    ISourceDocument ISourceLocation.SourceDocument {
      get { return this.PrimarySourceDocument; }
    }

    public string Source {
      get {
        if (this.Length == 0) return string.Empty;
        return this.primarySourceDocument.GetSourceLocation(this.StartIndex, this.Length).Source;
      }
    }

    public int StartIndex {
      get {
        if (this.startIndex == -1)
          this.startIndex = this.primarySourceDocument.ToPosition(this.startLine, this.startColumn);
        return this.startIndex;
      }
    }
    int startIndex = -1;

    #endregion

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.PrimarySourceDocument; }
    }

    #endregion
  }

  internal sealed class PdbLocalVariable : ILocalDefinition {
    PdbSlot pdbSlot;
    IMetadataHost host;
    IMethodDefinition methodDefinition;

    internal PdbLocalVariable(PdbSlot pdbSlot, IMetadataHost host, IMethodDefinition methodDefinition) {
      this.pdbSlot = pdbSlot;
      this.host = host;
      this.methodDefinition = methodDefinition;
    }

    #region ILocalDefinition Members

    public IMetadataConstant CompileTimeValue {
      get { return Dummy.Constant; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    private ITypeReference GetTypeForVariable() {
      foreach (ILocation location in this.methodDefinition.Locations) {
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          var doc = mbLocation.Document as MethodBodyDocument;
          if (doc != null) {
            ITypeReference result = doc.GetTypeFromToken(this.pdbSlot.typeToken);
            if (result is Dummy) {
              //TODO: error
              continue;
            }
            return result;
          }
        }
      }
      return this.host.PlatformType.SystemBoolean;
    }

    public bool IsConstant {
      get { return false; }
    }

    public bool IsModified {
      get { return false; }
    }

    public bool IsPinned {
      get { return false; }
    }

    public bool IsReference {
      get { return false; } //TODO: figure out how to determine this
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
    }

    public ITypeReference Type {
      get {
        if (this.type == null)
          this.type = this.GetTypeForVariable();
        return this.type;
      }
    }
    ITypeReference/*?*/ type;

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.host.NameTable.GetNameFor(this.pdbSlot.name); }
    }

    #endregion
  }

  internal sealed class PdbLocalConstant : ILocalDefinition {
    PdbConstant pdbConstant;
    IMetadataHost host;
    IMethodDefinition methodDefinition;

    internal PdbLocalConstant(PdbConstant pdbConstant, IMetadataHost host, IMethodDefinition methodDefinition) {
      this.pdbConstant = pdbConstant;
      this.host = host;
      this.methodDefinition = methodDefinition;
    }

    #region ILocalDefinition Members

    public IMetadataConstant CompileTimeValue {
      get {
        if (this.compileTimeValue == null)
          this.compileTimeValue = new PdbMetadataConstant(this.pdbConstant.value, this.Type);
        return this.compileTimeValue;
      }
    }
    IMetadataConstant/*?*/ compileTimeValue;

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; } //TODO: get from token
    }

    private ITypeReference GetTypeForConstant() {
      foreach (ILocation location in this.methodDefinition.Locations) {
        IILLocation/*?*/ mbLocation = location as IILLocation;
        if (mbLocation != null) {
          var doc = mbLocation.Document as MethodBodyDocument;
          if (doc != null) {
            ITypeReference result = doc.GetTypeFromToken(this.pdbConstant.token);
            if (result is Dummy) {
              //TODO: error
              continue;
            }
            return result;
          }
        }
      }
      IPlatformType platformType = this.methodDefinition.Type.PlatformType;
      IConvertible ic = this.pdbConstant.value as IConvertible;
      if (ic == null) return platformType.SystemObject;
      switch (ic.GetTypeCode()) {
        case TypeCode.Boolean: return platformType.SystemBoolean;
        case TypeCode.Byte: return platformType.SystemUInt8;
        case TypeCode.Char: return platformType.SystemChar;
        case TypeCode.Decimal: return platformType.SystemDecimal;
        case TypeCode.Double: return platformType.SystemFloat64;
        case TypeCode.Int16: return platformType.SystemInt16;
        case TypeCode.Int32: return platformType.SystemInt32;
        case TypeCode.Int64: return platformType.SystemInt64;
        case TypeCode.SByte: return platformType.SystemInt8;
        case TypeCode.Single: return platformType.SystemFloat64;
        case TypeCode.String: return platformType.SystemString;
        case TypeCode.UInt16: return platformType.SystemUInt16;
        case TypeCode.UInt32: return platformType.SystemUInt32;
        case TypeCode.UInt64: return platformType.SystemUInt64;
        default: return platformType.SystemObject;
      }
    }

    public bool IsConstant {
      get { return true; }
    }

    public bool IsModified {
      get { return false; }
    }

    public bool IsPinned {
      get { return false; }
    }

    public bool IsReference {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; } //TODO: return a method body location or some such thing
    }

    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
    }

    public ITypeReference Type {
      get {
        if (this.type == null)
          this.type = this.GetTypeForConstant();
        return this.type;
      }
    }
    ITypeReference/*?*/ type;

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { return this.host.NameTable.GetNameFor(this.pdbConstant.name); }
    }

    #endregion
  }

  internal sealed class PdbMetadataConstant : IMetadataConstant {

    internal PdbMetadataConstant(object value, ITypeReference type) {
      this.value = value;
      this.type = type;
    }

    #region IMetadataConstant Members

    public object Value {
      get { return this.value; }
    }
    readonly object value;

    #endregion

    #region IMetadataExpression Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public ITypeReference Type {
      get { return this.type; }
    }
    readonly ITypeReference type;

    #endregion
  }

  internal sealed class PdbIteratorScope : ILocalScope {

    internal PdbIteratorScope(uint offset, uint length) {
      this.offset = offset;
      this.length = length;
    }

    public uint Offset {
      get { return this.offset; }
    }
    uint offset;

    public uint Length {
      get { return this.length; }
    }
    uint length;

    public IMethodDefinition MethodDefinition {
      get { return this.methodDefinition; }
      set { this.methodDefinition = value; }
    }
    IMethodDefinition methodDefinition;
  }

  /// <summary>
  /// A range of CLR IL operations that comprise a lexical scope, specified as an IL offset and a length.
  /// </summary>
  internal sealed class PdbLocalScope : ILocalScope {

    /// <summary>
    /// Allocates a range of CLR IL operations that comprise a lexical scope, specified as an IL offset and a length.
    /// </summary>
    internal PdbLocalScope(IMethodBody methodBody, PdbScope pdbScope) {
      this.methodBody = methodBody;
      this.pdbScope = pdbScope;
    }

    internal readonly IMethodBody methodBody;

    internal readonly PdbScope pdbScope;

    /// <summary>
    /// The offset of the first operation in the scope.
    /// </summary>
    public uint Offset {
      get { return this.pdbScope.offset; }
    }

    /// <summary>
    /// The length of the scope. Offset+Length equals the offset of the first operation outside the scope, or equals the method body length.
    /// </summary>
    public uint Length {
      get { return this.pdbScope.length; }
    }

    /// <summary>
    /// The definition of the method in which this local scope is defined.
    /// </summary>
    public IMethodDefinition MethodDefinition { get { return this.methodBody.MethodDefinition; } }

  }

}