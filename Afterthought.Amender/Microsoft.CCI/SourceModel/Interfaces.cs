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
using Microsoft.Cci;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// Supplies information about an edit that has been performed on a source document that forms part of a compilation that is registered with this environment.
  /// The information is supplied in the form of a list of namespace or type declaration members that have been modified, added to or deleted from
  /// their containing namespace or type declarations.
  /// </summary>
  public class EditEventArgs : EventArgs {

    /// <summary>
    /// Allocates an object that supplies information about an edit that has been performed on a source document that forms part of a compilation that is registered with this environment.
    /// The information is supplied in the form of a list of namespace or type declaration members that have been modified, added to or deleted from
    /// their containing namespace or type declarations.
    /// </summary>
    public EditEventArgs(IEnumerable<IEditDescriptor> edits) {
      this.edits = edits;
    }

    /// <summary>
    /// A list of descriptors that collectively describe the edit that has caused this event.
    /// </summary>
    public IEnumerable<IEditDescriptor> Edits {
      get { return this.edits; }
    }
    readonly IEnumerable<IEditDescriptor> edits;

  }

  /// <summary>
  /// An object that represents a source document that has been derived from other source documents. 
  /// A derived source document does not have to correspond to a user accessible entity, in which case its
  /// name and location should not be used in user interaction.
  /// </summary>
  public interface IDerivedSourceDocument : ISourceDocument {

    /// <summary>
    /// A location corresponding to the entire document.
    /// </summary>
    IDerivedSourceLocation DerivedSourceLocation { get; }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    IDerivedSourceLocation GetDerivedSourceLocation(int position, int length);
    //^ requires 0 <= position && (position < this.Length || position == 0);
    //^ requires 0 <= length;
    //^ requires length <= this.Length;
    //^ requires position+length <= this.Length;
    //^ ensures result.SourceDocument == this;
    //^ ensures result.StartIndex == position;
    //^ ensures result.Length == length;

    /// <summary>
    /// Returns zero or more primary source locations that correspond to the given derived location.
    /// </summary>
    /// <param name="derivedSourceLocation">A source location in this derived document</param>
    IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IDerivedSourceLocation derivedSourceLocation);
    //^ requires derivedSourceLocation.DerivedSourceDocument == this;

  }

  /// <summary>
  /// A range of derived source text that corresponds to an identifiable entity.
  /// </summary>
  public interface IDerivedSourceLocation : ISourceLocation {

    /// <summary>
    /// The document containing the derived source text of which this location is a subrange.
    /// </summary>
    IDerivedSourceDocument DerivedSourceDocument {
      get;
    }

    /// <summary>
    /// A non empty collection of locations in primary source documents that together constitute this source location.
    /// The text of this location is the concatenation of the texts of each of the primary source locations.
    /// </summary>
    IEnumerable<IPrimarySourceLocation> PrimarySourceLocations { get; }

  }

  /// <summary>
  /// Describes an edit to a compilation as being either the addition, deletion or modification of a definition.
  /// </summary>
  public interface IEditDescriptor {
    /// <summary>
    /// The definition that has been added, deleted or modified.
    /// </summary>
    IDefinition AffectedDefinition {
      get;
    }

    //TODO: need a previous version of the affected definition

    /// <summary>
    /// The kind of edit that has been performed (addition, deletion or modification).
    /// </summary>
    EditEventKind Kind { get; }

    /// <summary>
    /// The source document that is the result of the edit described by this edit instance.
    /// </summary>
    ISourceDocument ModifiedSourceDocument {
      get;
      //^ ensures result.IsUpdatedVersionOf(this.OriginalSourceDocument);
    }

    /// <summary>
    /// The new version of the parent of the affected definition (see also this.OriginalParent).
    /// If the edit is an addition or modification, this.ModifiedParent is the actual parent of this.AffectedDefinition.
    /// If this.AffectedDefinition does not have a parent then this.ModifiedParent is the same as this.AffectedDefinition.
    /// </summary>
    IDefinition ModifiedParent {
      get;
    }

    /// <summary>
    /// The source document that has been edited as described by this edit instance.
    /// </summary>
    ISourceDocument OriginalSourceDocument {
      get;
    }

    /// <summary>
    /// The original parent of the affected definition (see also this.ModifiedParent). 
    /// If the edit is a deletion, this.OriginalParent is the parent of this.AffectedDefinition.
    /// If this.AffectedDefinition does not have a parent then this.OriginalParent is the same as this.AffectedDefinition.
    /// </summary>
    IDefinition OriginalParent {
      get;
    }

  }

  /// <summary>
  /// Describes the kind of edit that has been performed on a unit of metadata (also known as a symbol table).
  /// </summary>
  public enum EditEventKind {

    /// <summary>
    /// The affected namespace or type member has been added to its parent namespace or type.
    /// Of necessity, the (immutable) affected member is the result of the edit.
    /// </summary>
    Addition,

    /// <summary>
    /// The affected namespace or type member has been deleted from its parent namespace or type.
    /// Of necessity, the (immutable) affected member is from a model that precedes the edit and will be absent
    /// from the model that resulted from the edit.
    /// </summary>
    Deletion,

    /// <summary>
    /// The edit has resulted in a change to the affected namespace or type member, such as a change its name or visibility.
    /// The affeced member of the edit descriptor is the member after the edit has been applied.
    /// Note: a namespace or type declaration member is not considered to be modified if the change is confined to a child member. 
    /// In that case, the change event will be generated only for the child member.
    /// </summary>
    Modification
  }

  /// <summary>
  /// The root of an AST that represents the inputs, options and output of a compilation.
  /// </summary>
  public interface ICompilation {

    /// <summary>
    /// Returns true if the given source document forms a part of the compilation.
    /// </summary>
    bool Contains(ISourceDocument sourceDocument);

    /// <summary>
    /// Gets a unit set defined by the given name as specified by the compilation options. For example, the name could be an external alias
    /// in C# and the compilation options will specify which referenced assemblies correspond to the external alias.
    /// </summary>
    IUnitSet GetUnitSetFor(IName unitSetName);

    /// <summary>
    /// A collection of well known types that must be part of every target platform and that are fundamental to modeling compiled code.
    /// The types are obtained by querying the unit set of the compilation and thus can include types that are defined by the compilation itself.
    /// </summary>
    IPlatformType PlatformType {
      get;
    }

    /// <summary>
    /// The root of an AST that represents the output of a compilation. This can serve as an input to another compilation.
    /// </summary>
    IUnit Result { get; }

    /// <summary>
    /// A set of units comprised by the result of the compilation along with all of the units referenced by this compilation.
    /// </summary>
    IUnitSet UnitSet { get; }

  }

  /// <summary>
  /// Provides a standard abstraction over the applications that host editing of source files based on this object model.
  /// </summary>
  public interface ISourceEditHost : IMetadataHost {

    /// <summary>
    /// When an edit transaction has been completed, new compilations are computed for all affected compilations registered with this environment.
    /// For each affected compilation the difference between the new compilation and its previous version is reported via this event. 
    /// If the change involves a statement or expression the change shows up as a modified method declaration (or field declaration).
    /// </summary>
    event EventHandler<EditEventArgs> Edits;

    /// <summary>
    /// Registers the output of the given compilation as the latest unit associated with unit's location.
    /// Such units can then be discovered by clients via GetUnit.
    /// </summary>
    void RegisterAsLatest(ICompilation compilation);

    /// <summary>
    /// Raises the Edits event with the given edit event arguments. 
    /// </summary>
    void ReportEdits(EditEventArgs editEventArguments);

    /// <summary>
    /// Raises the SymbolTableEdits event with the given edit event arguments. 
    /// </summary>
    void ReportSymbolTableEdits(EditEventArgs editEventArguments);

    /// <summary>
    /// When an edit transaction has been completed, new compilations are computed for all affected compilations registered with this environment.
    /// For each affected compilation the difference between the new compilation and its previous version is reported via this event. 
    /// Changes that are confined to method bodies (and thus do not affect the symbol table) are not reported via this event.
    /// </summary>
    event EventHandler<EditEventArgs> SymbolTableEdits;
  }

  /// <summary>
  /// This interface is implemented by providers of semantic errors. That is, errors discovered by analysis of a constructed object model.
  /// Many of these errors will be discovered incrementally and as part of background activities.
  /// </summary>
  public interface ISemanticErrorsReporter {
  }

  /// <summary>
  /// Interface implemented by providers of syntax (parse) errors that occur in the symbol table level constructs
  /// of a source file. In particular, syntax errors that occur inside method bodies are not reported.
  /// </summary>
  public interface ISymbolSyntaxErrorsReporter : ISyntaxErrorsReporter {
  }

  /// <summary>
  /// Interface implemented by providers of syntax (parse) errors.
  /// </summary>
  public interface ISyntaxErrorsReporter {
  }

  /// <summary>
  /// A source location that falls inside a region of text that originally came from another source document.
  /// </summary>
  public interface IIncludedSourceLocation : IPrimarySourceLocation {

    /// <summary>
    /// The last line of the source location in the original document.
    /// </summary>
    int OriginalEndLine { get; }

    /// <summary>
    /// The name of the document from which this text in this source location was orignally obtained.
    /// </summary>
    string OriginalSourceDocumentName { get; }

    /// <summary>
    /// The first line of the source location in the original document.
    /// </summary>
    int OriginalStartLine { get; }
  }


  /// <summary>
  /// An object that represents a source document corresponding to a user accessible entity such as file.
  /// </summary>
  public interface IPrimarySourceDocument : ISourceDocument {

    /// <summary>
    /// A Guid that identifies the kind of document to applications such as a debugger. Typically System.Diagnostics.SymbolStore.SymDocumentType.Text.
    /// </summary>
    Guid DocumentType { get; }

    /// <summary>
    /// A Guid that identifies the programming language used in the source document. Typically used by a debugger to locate language specific logic.
    /// </summary>
    Guid Language { get; }

    /// <summary>
    /// A Guid that identifies the compiler vendor programming language used in the source document. Typically used by a debugger to locate vendor specific logic.
    /// </summary>
    Guid LanguageVendor { get; }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    IPrimarySourceLocation PrimarySourceLocation { get; }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    IPrimarySourceLocation GetPrimarySourceLocation(int position, int length);
    //^^ requires 0 <= position && (position < this.Length || position == 0);
    //^^ requires 0 <= length;
    //^^ requires length <= this.Length;
    //^^ requires position+length <= this.Length;
    //^^ ensures result.SourceDocument == this;
    //^^ ensures result.StartIndex == position;
    //^^ ensures result.Length == length;

    /// <summary>
    /// Maps the given (zero based) source position to a (one based) line and column, by scanning the source character by character, counting
    /// new lines until the given source position is reached. The source position and corresponding line+column are remembered and scanning carries
    /// on where it left off when this routine is called next. If the given position precedes the last given position, scanning restarts from the start.
    /// Optimal use of this method requires the client to sort calls in order of position.
    /// </summary>
    void ToLineColumn(int position, out int line, out int column);
    //^ requires position >= 0;
    //^ requires position <= this.Length;
    //^ ensures line >= 1 && column >= 1;

  }

  /// <summary>
  /// A range of source text that corresponds to an identifiable entity.
  /// </summary>
  public interface IPrimarySourceLocation : ISourceLocation {
    /// <summary>
    /// The last column in the last line of the range.
    /// </summary>
    int EndColumn {
      get;
      //^ ensures result >= 0;
    }

    /// <summary>
    /// The last line of the range.
    /// </summary>
    int EndLine {
      get;
      //^ ensures result >= 0;
    }

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    IPrimarySourceDocument PrimarySourceDocument {
      get;
    }

    /// <summary>
    /// The first column in the first line of the range.
    /// </summary>
    int StartColumn {
      get;
      //^ ensures result >= 0;
    }

    /// <summary>
    /// The first line of the range.
    /// </summary>
    int StartLine {
      get;
      //^ ensures result >= 0;
      //^ ensures result <= this.EndLine;
      //^ ensures result == this.EndLine ==> this.StartColumn <= this.EndColumn;
    }

  }

  /// <summary>
  /// An object that represents a source document, such as a text file containing C# source code.
  /// </summary>
  public interface ISourceDocument : IDocument {

    /// <summary>
    /// Copies no more than the specified number of characters to the destination character array, starting at the specified position in the source document.
    /// Returns the actual number of characters that were copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="position">The starting index to copy from. Must be greater than or equal to zero and position+length must be less than or equal to this.Length;</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Must be greater than 0 and less than or equal to the number elements of the destination array.</param>
    int CopyTo(int position, char[] destination, int destinationOffset, int length);
    //^ requires 0 <= position;
    //^ requires 0 <= length;
    //^ requires 0 <= position+length;
    //^ requires position <= this.Length;
    //^ requires 0 <= destinationOffset;
    //^ requires 0 <= destinationOffset+length;
    //^ requires destinationOffset+length <= destination.Length;
    //^ ensures 0 <= result;
    //^ ensures result <= length;
    //^ ensures position+result <= this.Length;

    /// <summary>
    /// Returns a source location in this document that corresponds to the given source location from a previous version
    /// of this document.
    /// </summary>
    ISourceLocation GetCorrespondingSourceLocation(ISourceLocation sourceLocationInPreviousVersionOfDocument);
    //^ requires this.IsUpdatedVersionOf(sourceLocationInPreviousVersionOfDocument.SourceDocument);

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    //^ [Pure]
    ISourceLocation GetSourceLocation(int position, int length);
    //^ requires 0 <= position && (position < this.Length || position == 0);
    //^ requires 0 <= length;
    //^ requires length <= this.Length;
    //^ requires position+length <= this.Length;
    //^ ensures result.SourceDocument == this;
    //^ ensures result.StartIndex == position;
    //^ ensures result.Length == length;

    /// <summary>
    /// Returns the source text of the document in string form. Each call may do significant work, so be sure to cache this.
    /// </summary>
    string GetText();
    //^ ensures result.Length == this.Length;

    /// <summary>
    /// Returns true if this source document has been created by editing the given source document (or an updated
    /// version of the given source document).
    /// </summary>
    //^ [Confined]
    bool IsUpdatedVersionOf(ISourceDocument sourceDocument);

    /// <summary>
    /// The length of the source string.
    /// </summary>
    int Length {
      get;
      //^ ensures result >= 0;
    }

    /// <summary>
    /// The language that determines how the document is parsed and what it means.
    /// </summary>
    string SourceLanguage { get; }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    ISourceLocation SourceLocation { get; }

  }

  /// <summary>
  /// An object that describes an edit to a source file.
  /// </summary>
  public interface ISourceDocumentEdit {

    /// <summary>
    /// The location in the source document that is being replaced by this edit.
    /// </summary>
    ISourceLocation SourceLocationBeforeEdit { get; }

    /// <summary>
    /// The source document that is the result of applying this edit.
    /// </summary>
    ISourceDocument SourceDocumentAfterEdit {
      get;
      //^ ensures result.IsUpdatedVersionOf(this.SourceLocationBeforeEdit.SourceDocument);
    }

  }

  /// <summary>
  /// Error information relating to a portion of a source document.
  /// </summary>
  public interface ISourceErrorMessage : IErrorMessage {

    /// <summary>
    /// The location of the error in the source document.
    /// </summary>
    ISourceLocation SourceLocation { get; }

    /// <summary>
    /// Makes a copy of this error message, changing only Location and SourceLocation to come from the
    /// given source document. Returns the same instance if the given source document is the same
    /// as this.SourceLocation.SourceDocument.
    /// </summary>
    /// <param name="targetDocument">The document to which the resulting error message must refer.</param>
    ISourceErrorMessage MakeShallowCopy(ISourceDocument targetDocument);
    //^ requires targetDocument == this.SourceLocation.SourceDocument || targetDocument.IsUpdatedVersionOf(this.SourceLocation.SourceDocument);
    //^ ensures targetDocument == this.SourceLocation.SourceDocument ==> result == this;

  }

  /// <summary>
  /// A range of source text that corresponds to an identifiable entity.
  /// </summary>
  public interface ISourceLocation : ILocation {
    /// <summary>
    /// True if the source at the given location is completely contained by the source at this location.
    /// </summary>
    //^ [Pure]
    bool Contains(ISourceLocation location);

    /// <summary>
    /// Copies the specified number of characters to the destination character array, starting at the specified offset from the start if the source location.
    /// Returns the number of characters actually copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="offset">The starting index to copy from. Must be greater than zero and less than this.Length.</param>
    /// <param name="destination">The destination array. Must have at least destinationOffset+length elements.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy.</param>
    //^ [Pure]
    int CopyTo(int offset, char[] destination, int destinationOffset, int length);
    //^ requires 0 <= offset;
    //^ requires 0 <= destinationOffset;
    //^ requires 0 <= length;
    //^ requires 0 <= offset+length;
    //^ requires 0 <= destinationOffset+length;
    //^ requires offset <= this.Length;
    //^ requires destinationOffset+length <= destination.Length;
    //^ ensures 0 <= result && result <= length && offset+result <= this.Length;
    //^ ensures result < length ==> offset+result == this.Length;

    /// <summary>
    /// The character index after the last character of this location, when treating the source document as a single string.
    /// </summary>
    int EndIndex {
      get;
      //^ ensures result >= 0 && result <= this.SourceDocument.Length;
      //^ ensures result == this.StartIndex + this.Length;
    }

    /// <summary>
    /// The number of characters in this source location.
    /// </summary>
    int Length {
      get;
      //^ ensures result >= 0;
      //^ ensures this.StartIndex+result <= this.SourceDocument.Length;
    }

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    ISourceDocument SourceDocument {
      get;
    }

    /// <summary>
    /// The source text corresponding to this location.
    /// </summary>
    string Source {
      get;
      //^ ensures result.Length == this.Length;
    }

    /// <summary>
    /// The character index of the first character of this location, when treating the source document as a single string.
    /// </summary>
    int StartIndex {
      get;
      //^ ensures result >= 0 && (result < this.SourceDocument.Length || result == 0);
    }

  }

  /// <summary>
  /// An object that can map some kinds of ILocation objects to IPrimarySourceLocation objects. 
  /// For example, a PDB reader that maps offsets in an IL stream to source locations.
  /// </summary>
  [ContractClass(typeof(ISourceLocationProviderContract))]
  public interface ISourceLocationProvider {
    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to one or more of the given derived (non primary) document locations.
    /// </summary>
    /// <param name="locations">Zero or more locations in documents that have been derived from one or more source documents.</param>
    IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IEnumerable<ILocation> locations);

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the given derived (non primary) document location.
    /// </summary>
    /// <param name="location">A location in a document that have been derived from one or more source documents.</param>
    IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(ILocation location);

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the definition of the given local.
    /// </summary>
    IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsForDefinitionOf(ILocalDefinition localDefinition);

    /// <summary>
    /// Returns the source name of the given local definition, if this is available. 
    /// Otherwise returns the value of the Name property and sets isCompilerGenerated to true.
    /// </summary>
    string GetSourceNameFor(ILocalDefinition localDefinition, out bool isCompilerGenerated);
  }

  #region ISourceLocationProvider contract binding
  [ContractClassFor(typeof(ISourceLocationProvider))]
  abstract class ISourceLocationProviderContract : ISourceLocationProvider {
    #region ISourceLocationProvider Members

    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IEnumerable<ILocation> locations) {
      Contract.Ensures(Contract.Result<IEnumerable<IPrimarySourceLocation>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(ILocation location) {
      Contract.Ensures(Contract.Result<IEnumerable<IPrimarySourceLocation>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsForDefinitionOf(ILocalDefinition localDefinition) {
      Contract.Ensures(Contract.Result<IEnumerable<IPrimarySourceLocation>>() != null);
      throw new NotImplementedException();
    }

    public string GetSourceNameFor(ILocalDefinition localDefinition, out bool isCompilerGenerated) {
      Contract.Ensures(Contract.Result<string>() != null);
      throw new NotImplementedException();
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A range of CLR IL operations that comprise a lexical scope, specified as an IL offset and a length.
  /// </summary>
  public interface ILocalScope {
    /// <summary>
    /// The offset of the first operation in the scope.
    /// </summary>
    uint Offset { get; }

    /// <summary>
    /// The length of the scope. Offset+Length equals the offset of the first operation outside the scope, or equals the method body length.
    /// </summary>
    uint Length { get; }

    /// <summary>
    /// The definition of the method in which this local scope is defined.
    /// </summary>
    IMethodDefinition MethodDefinition {
      get;
    }

  }

  /// <summary>
  /// An object that can provide information about the local scopes of a method.
  /// </summary>
  [ContractClass(typeof(ILocalScopeProviderContract))]
  public interface ILocalScopeProvider {

    /// <summary>
    /// Returns zero or more local (block) scopes, each defining an IL range in which an iterator local is defined.
    /// The scopes are returned by the MoveNext method of the object returned by the iterator method.
    /// The index of the scope corresponds to the index of the local. Specifically local scope i corresponds
    /// to the local stored in field &lt;localName&gt;x_i of the class used to store the local values in between
    /// calls to MoveNext.
    /// </summary>
    IEnumerable<ILocalScope> GetIteratorScopes(IMethodBody methodBody);

    /// <summary>
    /// Returns zero or more local (block) scopes into which the CLR IL operations in the given method body is organized.
    /// </summary>
    IEnumerable<ILocalScope> GetLocalScopes(IMethodBody methodBody);

    /// <summary>
    /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
    /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
    /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
    /// is for the y.
    /// </summary>
    IEnumerable<INamespaceScope> GetNamespaceScopes(IMethodBody methodBody);

    /// <summary>
    /// Returns zero or more local constant definitions that are local to the given scope.
    /// </summary>
    IEnumerable<ILocalDefinition> GetConstantsInScope(ILocalScope scope);

    /// <summary>
    /// Returns zero or more local variable definitions that are local to the given scope.
    /// </summary>
    IEnumerable<ILocalDefinition> GetVariablesInScope(ILocalScope scope);

    /// <summary>
    /// Returns true if the method body is an iterator.
    /// </summary>
    bool IsIterator(IMethodBody methodBody);

    /// <summary>
    /// If the given method body is the "MoveNext" method of the state class of an asynchronous method, the returned
    /// object describes where synchronization points occur in the IL operations of the "MoveNext" method. Otherwise
    /// the result is null.
    /// </summary>
    ISynchronizationInformation/*?*/ GetSynchronizationInformation(IMethodBody methodBody);
  }

  [ContractClassFor(typeof(ILocalScopeProvider))]
  abstract class ILocalScopeProviderContract : ILocalScopeProvider {
    public IEnumerable<ILocalScope> GetIteratorScopes(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<IEnumerable<ILocalScope>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ILocalScope> GetLocalScopes(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<IEnumerable<ILocalScope>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<INamespaceScope> GetNamespaceScopes(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<IEnumerable<INamespaceScope>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ILocalDefinition> GetConstantsInScope(ILocalScope scope) {
      Contract.Requires(scope != null);
      Contract.Ensures(Contract.Result<IEnumerable<ILocalDefinition>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ILocalDefinition> GetVariablesInScope(ILocalScope scope) {
      Contract.Requires(scope != null);
      Contract.Ensures(Contract.Result<IEnumerable<ILocalDefinition>>() != null);
      throw new NotImplementedException();
    }

    public bool IsIterator(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      throw new NotImplementedException();
    }

    public ISynchronizationInformation/*?*/ GetSynchronizationInformation(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// An object that describes where synchronization points occur in the IL operations of the "MoveNext" method of the state class of
  /// an asynchronous method.
  /// </summary>
  [ContractClass(typeof(ISynchronizationInformationContract))]
  public interface ISynchronizationInformation {
    /// <summary>
    /// The async method for which this object provides information about where its synchronization points are.
    /// </summary>
    IMethodDefinition AsyncMethod { get; }

    /// <summary>
    /// The "MoveNext" method of the state class of the asynchronous method for which this object provides information about where its synchronization points are.
    /// </summary>
    IMethodDefinition MoveNextMethod { get; }

    /// <summary>
    /// The offset of the first operation of the compiler generated catch all handler of the async method, if that method returns void.
    /// Otherwise the value must be uint.MaxValue.
    /// </summary>
    /// <remarks>Exceptions propagated out of void async methods are rethrown in the caller's thread from deep inside
    /// system code. A debugger that wants to stop execution only when an exception goes unhandled, might prefer to stop execution
    /// when the exception is captured by the catch all handler, in which case the offending code is still on the stack, rather than when
    /// the captured exception is rethrown.</remarks>
    uint GeneratedCatchHandlerOffset { get; }

    /// <summary>
    /// Zero or more objects that describe points where synchronization occurs in an async method. 
    /// </summary>
    IEnumerable<ISynchronizationPoint> SynchronizationPoints { get; }
  }

  #region ISynchronizationInformation contract binding
  [ContractClassFor(typeof(ISynchronizationInformation))]
  abstract class ISynchronizationInformationContract : ISynchronizationInformation {
    #region ISynchronizationInformation Members

    public IMethodDefinition AsyncMethod {
      get {
        Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IMethodDefinition MoveNextMethod {
      get {
        Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
        throw new NotImplementedException();
      }
    }

    public uint GeneratedCatchHandlerOffset {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISynchronizationPoint> SynchronizationPoints {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ISynchronizationPoint>>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion
  }
  #endregion

  /// <summary>
  /// An object that provides the offset of the first IL operation that initiates a synchronization operation inside an async method, as well as the offset 
  /// (and containing method) of the first IL operation that will execute after synchronization has been achieved.
  /// </summary>
  [ContractClass(typeof(ISynchronizationPointContract))]
  public interface ISynchronizationPoint {
    /// <summary>
    /// The offset of the first IL operation of a sequence of instructions that will cause the thread executing the async method to synchronize with
    /// the thread executing another async method. (This can cause execution of the async method to be temporarily suspended.) 
    /// In C#, this sequence is generated for the await operation.
    /// </summary>
    uint SynchronizeOffset { get; }

    /// <summary>
    /// The method in which execution continues after synchronization has been achieved, if this is not the same method as the one
    /// containing the operations that achieves synchronization (i.e. the "MoveNext" method of the state class of the async method).
    /// </summary>
    IMethodDefinition/*?*/ ContinuationMethod { get; }

    /// <summary>
    /// The offset of the first IL operation that will be executed after synchronization has been achieved. In C# this will be the offset of 
    /// the first instruction following an await operation.
    /// </summary>
    uint ContinuationOffset { get; }
  }

  #region ISynchronizationPoint contract binding
  [ContractClassFor(typeof(ISynchronizationPoint))]
  abstract class ISynchronizationPointContract : ISynchronizationPoint {
    #region ISynchronizationPoint Members

    public uint SynchronizeOffset {
      get { throw new NotImplementedException(); }
    }

    public IMethodDefinition/*?*/ ContinuationMethod {
      get { throw new NotImplementedException(); }
    }

    public uint ContinuationOffset {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion

  /// <summary>
  /// A description of the lexical scope in which a namespace type has been nested. This scope is tied to a particular
  /// method body, so that partial types can be accommodated.
  /// </summary>
  [ContractClass(typeof(INamespaceScopeContract))]
  public interface INamespaceScope {

    /// <summary>
    /// Zero or more used namespaces. These correspond to using clauses in C#.
    /// </summary>
    IEnumerable<IUsedNamespace> UsedNamespaces { get; }

  }

  #region INamespaceScope contract binding
  [ContractClassFor(typeof(INamespaceScope))]
  abstract class INamespaceScopeContract : INamespaceScope {
    #region INamespaceScope Members

    public IEnumerable<IUsedNamespace> UsedNamespaces {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IUsedNamespace>>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion
  }
  #endregion

  /// <summary>
  /// A namespace that is used (imported) inside a namespace scope.
  /// </summary>
  [ContractClass(typeof(IUsedNamespaceContract))]
  public interface IUsedNamespace {
    /// <summary>
    /// An alias for a namespace. For example the "x" of "using x = y.z;" in C#. Empty if no alias is present.
    /// </summary>
    IName Alias { get; }

    /// <summary>
    /// The name of a namepace that has been aliased.  For example the "y.z" of "using x = y.z;" or "using y.z" in C#.
    /// </summary>
    IName NamespaceName { get; }
  }

  #region IUsedNamedspace contract binding
  [ContractClassFor(typeof(IUsedNamespace))]
  abstract class IUsedNamespaceContract : IUsedNamespace {
    #region IUsedNamespace Members

    public IName Alias {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IName NamespaceName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// Supplies information about edits that have been performed on source documents that form part of compilations that are registered with this environment.
  /// </summary>
  public class SourceEditEventArgs : EventArgs {

    /// <summary>
    /// Allocates an object that supplies information about edits that have been performed on source documents that form part of compilations that are registered with this environment.
    /// </summary>
    public SourceEditEventArgs(IEnumerable<ISourceDocumentEdit> edits) {
      this.edits = edits;
    }

    /// <summary>
    /// A list of edits to source documents that have occurred as a single event.
    /// </summary>
    public IEnumerable<ISourceDocumentEdit> Edits {
      get { return this.edits; }
    }
    readonly IEnumerable<ISourceDocumentEdit> edits;

  }
}
