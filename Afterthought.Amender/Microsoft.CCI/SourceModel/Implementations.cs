//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.Cci.Immutable;
using Microsoft.Cci.UtilityDataStructures;
using System.Diagnostics.Contracts;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// A trivial class that implements the ISemanticErrorsReporter interface and exposes a singleton instance.
  /// </summary>
  public sealed class SemanticErrorReporter : ISemanticErrorsReporter {
    private SemanticErrorReporter() { }

    /// <summary>
    /// A singleton instance of an object that implements the ISemanticErrorsReporter interface.
    /// </summary>
    public readonly static SemanticErrorReporter Instance = new SemanticErrorReporter();
  }

  /// <summary>
  /// Provides an abstraction over the application hosting compilers based on this framework.
  /// </summary>
  [ContractVerification(false)]
  public abstract class SourceEditHostEnvironment : MetadataReaderHost, ISourceEditHost {

    /// <summary>
    /// Allocates an object that provides an abstraction over the application hosting compilers based on this framework.
    /// </summary>
    /// <param name="nameTable">
    /// A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.
    /// </param>
    /// <param name="factory">
    /// The intern factory to use when generating keys. When comparing two or more assemblies using
    /// TypeHelper, MemberHelper, etc. it is necessary to make the hosts use the same intern factory.
    /// </param>
    /// <param name="pointerSize">The size of a pointer on the runtime that is the target of the metadata units to be loaded
    /// into this metadta host. This parameter only matters if the host application wants to work out what the exact layout
    /// of a struct will be on the target runtime. The framework uses this value in methods such as TypeHelper.SizeOfType and
    /// TypeHelper.TypeAlignment. If the host application does not care about the pointer size it can provide 0 as the value
    /// of this parameter. In that case, the first reference to IMetadataHost.PointerSize will probe the list of loaded assemblies
    /// to find an assembly that either requires 32 bit pointers or 64 bit pointers. If no such assembly is found, the default is 32 bit pointers.
    /// </param>
    /// <param name="searchPaths">
    /// A collection of strings that are interpreted as valid paths which are used to search for units.
    /// </param>
    /// <param name="searchInGAC">
    /// Whether the GAC (Global Assembly Cache) should be searched when resolving references.
    /// </param>
    protected SourceEditHostEnvironment(INameTable nameTable, IInternFactory factory, byte pointerSize, IEnumerable<string> searchPaths, bool searchInGAC)
      : base(nameTable, factory, pointerSize, searchPaths, searchInGAC) {
      Contract.Requires(pointerSize == 0 || pointerSize == 4 || pointerSize == 8);
    }

    /// <summary>
    /// Gets a unit set corresponding to the referenced unit set.
    /// </summary>
    public IUnitSet GetUnitSetFor(UnitSetIdentity referencedUnitSet) {
      Contract.Requires(referencedUnitSet != null);

      IUnitSet/*?*/ result = null;
      WeakReference/*?*/ entry;
      lock (GlobalLock.LockingObject) {
        if (this.unitSetCache.TryGetValue(referencedUnitSet, out entry)) {
          if (entry != null) result = entry.Target as IUnitSet;
        }
      }
      List<IUnit> units = new List<IUnit>();
      foreach (UnitIdentity unitId in referencedUnitSet.Units) {
        Contract.Assume(unitId != null);
        IUnit unit = this.LoadUnit(unitId);
        if (unit is Dummy) return Dummy.UnitSet;
        units.Add(unit);
      }
      if (result != null && UnitsAreTheSameObjects(units, result.Units))
        return result;
      lock (GlobalLock.LockingObject) {
        if (this.unitSetCache.TryGetValue(referencedUnitSet, out entry)) {
          if (entry != null) result = entry.Target as IUnitSet;
          if (result != null && UnitsAreTheSameObjects(units, result.Units)) return result;
        }
        result = new UnitSet(units);
        this.unitSetCache[referencedUnitSet] = new WeakReference(result);
      }
      return result;
    }
    readonly Dictionary<UnitSetIdentity, WeakReference> unitSetCache = new Dictionary<UnitSetIdentity, WeakReference>();

    /// <summary>
    /// Returns true if the given list of units has the same number of elements as the given enumeration of units and
    /// each corresponding element is the object.
    /// </summary>
    private static bool UnitsAreTheSameObjects(List<IUnit> unitList, IEnumerable<IUnit> unitEnumeration) {
      Contract.Requires(unitList != null);
      Contract.Requires(unitEnumeration != null);

      var enumerator = unitEnumeration.GetEnumerator();
      for (int i = 0, n = unitList.Count; i < n; i++)
        if (!enumerator.MoveNext() || !object.ReferenceEquals(unitList[i], enumerator.Current)) return false;
      return !enumerator.MoveNext();
    }

    /// <summary>
    /// When a source document that forms part of a compilation that is registered with this environment has been edited a new compilation is computed
    /// and the difference between the new compilation and its previous version is reported via this event. If the change involves a statement or expression
    /// the change shows up as a modified method declaration (or field declaration).
    /// </summary>
    public event EventHandler<EditEventArgs>/*?*/ Edits;
    //TODO: implement these explicitly so that an explicit list of listeners can be kept.
    //Use a separate thread for each listener. Use weak references to hold on to listeners.

    /// <summary>
    /// Registers the output of the given compilation as the latest unit associated with the given location.
    /// Such units can then be discovered by clients via GetUnit. 
    /// </summary>
    public void RegisterAsLatest(ICompilation compilation) {
      this.RegisterAsLatest(compilation.Result);
    }

    /// <summary>
    /// Raises the Edits event with the given edit event arguments. 
    /// The events are raised on different thread.
    /// </summary>
    public void ReportEdits(EditEventArgs editEventArguments) {
      if (this.Edits != null)
        ThreadPool.QueueUserWorkItem(this.ReportEditsUsingDifferentThread, editEventArguments);
    }

    /// <summary>
    /// Raises the Edits event with the given edit event arguments. 
    /// </summary>
    /// <param name="state">The edit event arguments.</param>
    private void ReportEditsUsingDifferentThread(object/*?*/ state) {
      Contract.Requires(state is EditEventArgs);

      EditEventArgs editEventArguments = (EditEventArgs)state;
      if (this.Edits != null)
        this.Edits(this, editEventArguments);
    }

    /// <summary>
    /// Raises the SymbolTableEdits event with the given edit event arguments.
    /// The events are raised on different thread.
    /// </summary>
    public void ReportSymbolTableEdits(EditEventArgs editEventArguments) {
      if (this.SymbolTableEdits != null)
        ThreadPool.QueueUserWorkItem(this.ReportSymbolTableEditsUsingDifferentThread, editEventArguments);
    }

    /// <summary>
    /// Raises the SymbolTableEdits event with the given edit event arguments.
    /// </summary>
    /// <param name="state">The edit event arguments.</param>
    private void ReportSymbolTableEditsUsingDifferentThread(object/*?*/ state) {
      Contract.Requires(state is EditEventArgs);

      EditEventArgs editEventArguments = (EditEventArgs)state;
      if (this.SymbolTableEdits != null)
        this.SymbolTableEdits(this, editEventArguments);
    }

    /// <summary>
    /// When an edit transaction has been completed, new compilations are computed for all affected compilations registered with this environment.
    /// For each affected compilation the difference between the new compilation and its previous version is reported via this event. 
    /// Changes that are confined to method bodies (and thus do not affect the symbol table) are not reported via this event.
    /// </summary>
    public event EventHandler<EditEventArgs>/*?*/ SymbolTableEdits;
  }

  /// <summary>
  /// An object that describes an edit to a source file.
  /// </summary>
  [ContractVerification(false)]
  public abstract class SourceDocumentEdit : ISourceDocumentEdit {

    /// <summary>
    /// Allocates an object that describes an edit to a source file.
    /// </summary>
    protected SourceDocumentEdit(ISourceLocation sourceLocationBeforeEdit, ISourceDocument sourceDocumentAfterEdit) {
      Contract.Requires(sourceLocationBeforeEdit != null);
      Contract.Requires(sourceDocumentAfterEdit != null);
      Contract.Requires(sourceDocumentAfterEdit.IsUpdatedVersionOf(sourceLocationBeforeEdit.SourceDocument));

      this.sourceLocationBeforeEdit = sourceLocationBeforeEdit;
      this.sourceDocumentAfterEdit = sourceDocumentAfterEdit;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.sourceDocumentAfterEdit.IsUpdatedVersionOf(this.sourceLocationBeforeEdit.SourceDocument));
    }

    /// <summary>
    /// The location in the source document that is being replaced by this edit.
    /// </summary>
    public ISourceLocation SourceLocationBeforeEdit {
      get { return this.sourceLocationBeforeEdit; }
    }
    readonly ISourceLocation sourceLocationBeforeEdit;

    /// <summary>
    /// The source document that is the result of applying this edit.
    /// </summary>
    public ISourceDocument SourceDocumentAfterEdit {
      get
        //^^ ensures result.IsUpdatedVersionOf(this.SourceLocationBeforeEdit.SourceDocument);
      {
        return this.sourceDocumentAfterEdit;
      }
    }
    readonly ISourceDocument sourceDocumentAfterEdit;

  }

  /// <summary>
  /// Error information relating to a portion of a source document.
  /// </summary>
  [DebuggerDisplay("Message = {Message}")]
  public abstract class ErrorMessage : ISourceErrorMessage {

    /// <summary>
    /// Initializes an object providing error information relating to a portion of a source document.
    /// </summary>
    /// <param name="sourceLocation">The location of the error in the source document.</param>
    /// <param name="errorCode">A code that corresponds to this error. This code is the same for all cultures.</param>
    /// <param name="messageKey">A string that is used as the key when looking for the localized error message using a resource manager.</param>
    /// <param name="messageArguments">Zero or more strings that are to be subsituted for "{i}" sequences in the message string return by GetMessage.</param>
    protected ErrorMessage(ISourceLocation sourceLocation, long errorCode, string messageKey, params string[] messageArguments) {
      Contract.Requires(sourceLocation != null);
      Contract.Requires(messageKey != null);
      Contract.Requires(messageArguments != null);

      this.sourceLocation = sourceLocation;
      this.errorCode = errorCode;
      this.messageKey = messageKey;
      this.relatedLocations = EmptyLocations;
      this.messageArguments = messageArguments;
    }

    /// <summary>
    /// Initializes an object providing error information relating to a portion of a source document.
    /// </summary>
    /// <param name="sourceLocation">The location of the error in the source document.</param>
    /// <param name="errorCode">A code that corresponds to this error. This code is the same for all cultures.</param>
    /// <param name="messageKey">A string that is used as the key when looking for the localized error message using a resource manager.</param>
    /// <param name="relatedLocations">Zero ore more locations that are related to this error.</param>
    /// <param name="messageArguments">Zero or more strings that are to be subsituted for "{i}" sequences in the message string return by GetMessage.</param>
    protected ErrorMessage(ISourceLocation sourceLocation, long errorCode, string messageKey, IEnumerable<ILocation> relatedLocations, params string[] messageArguments) {
      Contract.Requires(sourceLocation != null);
      Contract.Requires(messageKey != null);
      Contract.Requires(relatedLocations != null);
      Contract.Requires(messageArguments != null);

      this.sourceLocation = sourceLocation;
      this.errorCode = errorCode;
      this.messageKey = messageKey;
      this.relatedLocations = relatedLocations;
      this.messageArguments = messageArguments;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.sourceLocation != null);
      Contract.Invariant(this.relatedLocations != null);
      Contract.Invariant(this.messageKey != null);
      Contract.Invariant(this.messageArguments != null);
    }

    /// <summary>
    /// A readonly list of ILocations with exactly 0 entries.
    /// </summary>
    static IEnumerable<ILocation> EmptyLocations = new List<ILocation>(0).AsReadOnly();

    /// <summary>
    /// The object reporting the error. This can be used to filter out errors coming from non interesting sources.
    /// </summary>
    public abstract object ErrorReporter {
      get;
    }

    /// <summary>
    /// A short identifier for the reporter of the error, suitable for use in human interfaces. For example "CS" in the case of a C# language error.
    /// </summary>
    public abstract string ErrorReporterIdentifier {
      get;
    }

    /// <summary>
    /// A code that corresponds to this error. This code is the same for all cultures.
    /// </summary>
    public long Code {
      get { return this.errorCode; }
    }
    readonly long errorCode;

    /// <summary>
    /// Obtains a localized message from the given resource manager and formats it using the message arguments associated with this error message.
    /// If no localized message corresponds to the message key of this message, the invariant culture is used.
    /// If no message corresponding to this error can be found then the message key itself is returned.
    /// </summary>
    /// <param name="rm">A resource manager corresponding to the current locale.</param>
    protected string GetMessage(System.Resources.ResourceManager rm) {
      Contract.Requires(rm != null);

      string/*?*/ localizedString = null;
      try {
        localizedString = rm.GetString(this.messageKey);
      } catch (System.Resources.MissingManifestResourceException) {
#if !COMPACTFX
      } catch (System.Resources.MissingSatelliteAssemblyException) {
#endif
      }
      try {
        if (localizedString == null)
          localizedString = rm.GetString(this.messageKey, System.Globalization.CultureInfo.InvariantCulture);
      } catch (System.Resources.MissingManifestResourceException) {
      }
      if (localizedString == null) localizedString = this.messageKey;
      if (this.messageArguments.Length == 0) return localizedString;
      try {
        return string.Format(localizedString, this.messageArguments);
      } catch (FormatException) {
        return localizedString;
      }
    }

    /// <summary>
    /// True if the error message should be treated as an informational warning rather than as an indication that the associated
    /// compilation has failed and no useful executable output has been generated. The value of this property does
    /// not depend solely on this.Code but can be influenced by compiler options such as the csc /warnaserror option.
    /// </summary>
    public virtual bool IsWarning {
      get { return false; }
    }

    /// <summary>
    /// Makes a copy of this error message, changing only Location and SourceLocation to come from the
    /// given source document. Returns the same instance if the given source document is the same
    /// as this.SourceLocation.SourceDocument.
    /// </summary>
    /// <param name="targetDocument">The document to which the resulting error message must refer.</param>
    public abstract ISourceErrorMessage MakeShallowCopy(ISourceDocument targetDocument);
    //^^ requires targetDocument == this.SourceLocation.SourceDocument || targetDocument.IsUpdatedVersionOf(this.SourceLocation.SourceDocument);
    //^^ ensures targetDocument == this.SourceLocation.SourceDocument ==> result == this;

    /// <summary>
    /// A description of the error suitable for user interaction. Localized to the current culture.
    /// </summary>
    public abstract string Message {
      get;
    }

    /// <summary>
    /// Zero or more strings that are to be subsituted for "{i}" sequences in the message string return by GetMessage.
    /// </summary>
    protected string[] MessageArguments() {
      Contract.Ensures(Contract.Result<string[]>() != null);

      return this.messageArguments;
    }
    readonly string[] messageArguments;

    /// <summary>
    /// A string that is used as the key when looking for the localized error message using a resource manager.
    /// </summary>
    protected string MessageKey {
      get {
        Contract.Ensures(Contract.Result<string>() != null);

        return this.messageKey; 
      }
    }
    readonly string messageKey;

    /// <summary>
    /// Zero ore more locations that are related to this error.
    /// </summary>
    public IEnumerable<ILocation> RelatedLocations {
      get { return this.relatedLocations; }
    }
    IEnumerable<ILocation> relatedLocations;

    /// <summary>
    /// The location of the error in the source document.
    /// </summary>
    public ISourceLocation SourceLocation {
      get { return this.sourceLocation; }
    }
    readonly ISourceLocation sourceLocation;

    #region IErrorMessage Members

    ILocation IErrorMessage.Location {
      get { return this.SourceLocation; }
    }

    #endregion

  }

  /// <summary>
  /// An object that represents a source document that is the composition of an ordered enumeration of fragments from other source document.
  /// The document is parsed according to the rules of a particular language, such as C#, to produce an object model that can be obtained via the CompilationPart property.
  /// </summary>
  [ContractVerification(false)]
  public abstract class CompositeSourceDocument : SourceDocument, IDerivedSourceDocument {

    /// <summary>
    /// Allocates an object that represents a source document that is the composition of an ordered enumeration of fragments from other source document.
    /// The document is parsed according to the rules of a particular language, such as C#, to produce an object model that can be obtained via the CompilationPart property.
    /// </summary>
    /// <param name="name">The name of the document. Used to identify the document in user interaction.</param>
    protected CompositeSourceDocument(IName name)
      : base(name) {
    }

    /// <summary>
    /// Allocates an object that represents a composite source document that is derived from another source document by replacing one substring with another.
    /// </summary>
    /// <param name="previousVersion">The source document on which the newly allocated document will be based.</param>
    /// <param name="position">The first character in the previous version of the new document that will be changed in the new document.</param>
    /// <param name="oldLength">The number of characters in the previous verion of the new document that will be changed in the new document.</param>
    /// <param name="newLength">The number of replacement characters in the new document. 
    /// (The length of the string that replaces the substring from position to position+length in the previous version of the new document.)</param>
    protected CompositeSourceDocument(SourceDocument previousVersion, int position, int oldLength, int newLength)
      : base(previousVersion, position, oldLength, newLength) {
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(!this.enumeratorIsValid || this.fragmentEnumerator != null);
      Contract.Invariant(currentFragmentOffset <= this.Length);
      Contract.Invariant(!this.enumeratorIsValid || this.fragmentEnumerator != null);
    }

    /// <summary>
    /// Copies no more than the specified number of characters to the destination character array, starting at the specified position in the source document.
    /// Returns the actual number of characters that were copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="position">The starting index to copy from. Must be greater than or equal to zero and position+length must be less than or equal to this.Length;</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Must be greater than 0 and less than or equal to the number elements of the destination array.</param>
    public override int CopyTo(int position, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= position && 0 <= length && 0 <= position+length && position <= this.Length;
      //^^ requires 0 <= destinationOffset && 0 <= destinationOffset+length && destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && position+result <= this.Length;
    {
      if (this.length != null && position >= (int)this.length) return 0;
      if (!this.enumeratorIsValid || position < this.currentFragmentOffset) {
        this.fragmentEnumerator = this.GetFragments().GetEnumerator();
        this.currentFragmentOffset = 0;
        this.enumeratorIsValid = this.fragmentEnumerator.MoveNext();
      }
      Contract.Assert(this.fragmentEnumerator != null);
      while (this.enumeratorIsValid) {
        int fragmentLength = this.fragmentEnumerator.Current.Length;
        if (this.currentFragmentOffset + fragmentLength > position) break;
        this.currentFragmentOffset += fragmentLength;
        this.enumeratorIsValid = this.fragmentEnumerator.MoveNext();
      }
      int charsCopied = 0;
      Contract.Assume(0 <= position && position <= this.Length); //follows from the precondition
      while (this.enumeratorIsValid && charsCopied < length) {
        Contract.Assert(this.fragmentEnumerator != null);
        Contract.Assert(this.currentFragmentOffset+this.fragmentEnumerator.Current.Length > position);
        Contract.Assert(0 <= position && position <= this.Length);
        Contract.Assert(0 <= charsCopied && charsCopied <= length);
        Contract.Assert(position+charsCopied <= this.Length);

        int fragmentLength = this.fragmentEnumerator.Current.Length;
        int fragmentStart = 0;
        if (position > this.currentFragmentOffset) fragmentStart = position - this.currentFragmentOffset;
        Contract.Assume(fragmentStart <= this.Length); //position < this.Length as per the loop invariant
        int fragmentCharsToCopy = fragmentLength - fragmentStart;
        if (charsCopied + fragmentCharsToCopy > length) fragmentCharsToCopy = length - charsCopied;
        //Contract.Assume(fragmentStart <= this.Length);
        int fragCharsCopied = this.fragmentEnumerator.Current.CopyTo(fragmentStart, destination, destinationOffset, fragmentCharsToCopy);
        if (fragCharsCopied == 0) {
          this.enumeratorIsValid = false;
          break;
        }
        charsCopied += fragCharsCopied;
        Contract.Assume(position + charsCopied <= this.Length);
        destinationOffset += fragmentCharsToCopy;
        if (fragmentStart + fragmentCharsToCopy >= fragmentLength) {
          this.currentFragmentOffset += fragmentLength;
          this.enumeratorIsValid = this.fragmentEnumerator.MoveNext();
        }
      }
      if (!this.enumeratorIsValid) this.length = this.currentFragmentOffset;
      Contract.Assert(0 <= charsCopied && charsCopied <= length && position+charsCopied <= this.Length);
      return charsCopied;
    }

    /// <summary>
    /// The total number of characters in the fragments the precede this.fragmentEnumerator.Current.
    /// </summary>
    int currentFragmentOffset;
    //^ invariant currentFragmentOffset <= this.Length;

    /// <summary>
    /// True if this.fragmentEnumerator is non null and if this.fragmentEnumerator.Current can be evaluated.
    /// </summary>
    bool enumeratorIsValid;
    //^ invariant enumeratorIsValid ==> fragmentEnumerator != null;

    /// <summary>
    /// An enumerator instance that is used to traverse through the enumerable returned by this.GetFragments(), during
    /// the course of multiple calls to CopyTo or ToLineColumn.
    /// </summary>
    IEnumerator<ISourceLocation>/*?*/ fragmentEnumerator;

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    public IDerivedSourceLocation GetDerivedSourceLocation(int position, int length)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      IDerivedSourceLocation result = new DerivedSourceLocation(this, position, length);
      //^ assume result.SourceDocument == this;
      //^ assume result.StartIndex == position;
      //^ assume result.Length == length;
      return result;
    }

    /// <summary>
    /// Returns an enumeration of fragments from other documents. The source of the enumeration could be a computation, such as a pre-processor.
    /// </summary>
    protected virtual IEnumerable<ISourceLocation> GetFragments() {
      Contract.Ensures(Contract.Result<IEnumerable<ISourceLocation>>() != null);
      throw new InvalidOperationException(); //This method would be abstract but for the contract.
    }

    /// <summary>
    /// Returns an enumeration of fragments from other documents that together make up the given source location in this document.
    /// </summary>
    /// <param name="sourceLocation">A location in this document.</param>
    public IEnumerable<ISourceLocation> GetFragmentLocationsFor(ISourceLocation sourceLocation)
      //^ requires sourceLocation.Document == this;
    {
      int startIndex = sourceLocation.StartIndex;
      int length = sourceLocation.Length;
      int fragmentOffset = 0;
      foreach (ISourceLocation fragment in this.GetFragments()) {
        int fragmentLength = fragment.Length;
        if (fragmentOffset > startIndex) {
          int len = fragmentLength;
          if (len > length) len = length;
          length -= len;
          yield return fragment.SourceDocument.GetSourceLocation(0 + fragment.StartIndex, len);
          if (length == 0) yield break;
          fragmentOffset += fragmentLength;
          continue;
        }
        if (fragmentOffset + fragmentLength > startIndex) {
          int fragOffset = startIndex - fragmentOffset;
          int fragLen = fragmentLength - fragOffset;
          if (fragLen > length) fragLen = length;
          length -= fragLen;
          yield return fragment.SourceDocument.GetSourceLocation(fragOffset + fragment.StartIndex, fragLen);
          if (length == 0) yield break;
        }
        fragmentOffset += fragmentLength;
      }
    }

    /// <summary>
    /// Returns zero or more primary source locations that correspond to the given derived location.
    /// </summary>
    /// <param name="derivedSourceLocation">A source location in this derived document</param>
    //^ [Pure]
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IDerivedSourceLocation derivedSourceLocation)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      foreach (ISourceLocation sourceLocation in this.GetFragmentLocationsFor(derivedSourceLocation)) {
        IPrimarySourceLocation/*?*/ primarySourceLocation = sourceLocation as IPrimarySourceLocation;
        if (primarySourceLocation != null)
          yield return primarySourceLocation;
        else {
          IDerivedSourceLocation/*?*/ dLoc = sourceLocation as IDerivedSourceLocation;
          if (dLoc != null) {
            foreach (IPrimarySourceLocation pLoc in dLoc.PrimarySourceLocations)
              yield return pLoc;
          }
        }
      }
    }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    //^ [Pure]
    public override ISourceLocation GetSourceLocation(int position, int length)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      return this.GetDerivedSourceLocation(position, length);
    }

    /// <summary>
    /// Returns the source text of the document in string form. This call is expensive in time and space. Rather use CopyTo if at all possible.
    /// </summary>
    public override string GetText()
      //^^ ensures result.Length == this.Length;
    {
      StringBuilder sb = new StringBuilder();
      foreach (ISourceLocation fragment in this.GetFragments())
        sb.Append(fragment.Source);
      this.length = sb.Length;
      string result = sb.ToString();
      //^ assume result.Length == this.Length;
      return result;
    }

    /// <summary>
    /// The length of the source string. Evaluating this property is expensive in time and space. Avoid evaluating it if at all possible. It is not necessary
    /// to evaluate this property if the text of this document is obtained via successive calls to CopyTo.
    /// </summary>
    public override int Length {
      get
        //^^ ensures result >= 0;
      {
        if (this.length == null) {
          lock (GlobalLock.LockingObject) {
            if (this.length == null) {
              int length = 0;
              foreach (ISourceLocation fragment in this.GetFragments())
                length += fragment.Length;
              //^ assume 0 <= length; //no overflow
              this.length = length;
            }
          }
        }
        return (int)this.length;
      }
    }
    int? length; //^ invariant length == null || (int)length >= 0;

    /// <summary>
    /// The location where this document was found, or where it should be stored. 
    /// In this case, since the document is a composite of other documents, the result is always the empty string.
    /// </summary>
    public override string Location {
      get { return ""; }
    }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    public override SourceLocation SourceLocation {
      get
        //^ ensures result is SourceLocationSpanningEntireDerivedSourceDocument;
      {
        if (this.sourceLocation == null) {
          SourceLocationSpanningEntireDerivedSourceDocument dloc = new SourceLocationSpanningEntireDerivedSourceDocument(this);
          if (this.sourceLocation == null)
            this.sourceLocation = dloc;
        }
        return this.sourceLocation;
      }
    }
    //^ [Once]
    SourceLocationSpanningEntireDerivedSourceDocument/*?*/ sourceLocation;

    /// <summary>
    /// Maps the given (zero based) source position to a (one based) line and column, by scanning the source character by character, counting
    /// new lines until the given source position is reached. The source position and corresponding line+column are remembered and scanning carries
    /// on where it left off when this routine is called next. If the given position precedes the last given position, scanning restarts from the start.
    /// Optimal use of this method requires the client to sort calls in order of position.
    /// </summary>
    public void ToLineColumn(int position, out int line, out int column)
      //^^ requires position >= 0 && position <= Length;
      //^^ ensures line >= 1 && column >= 1;
    {
      if (!this.enumeratorIsValid || position < this.currentFragmentOffset || position < this.lastPosition) {
        this.fragmentEnumerator = this.GetFragments().GetEnumerator();
        this.currentFragmentOffset = 0;
        this.enumeratorIsValid = this.fragmentEnumerator.MoveNext();
        this.lastPosition = 0;
        this.lineCounter = 1;
        this.columnCounter = 1;
      }
      line = this.lineCounter;
      column = this.columnCounter;
      Contract.Assert(this.fragmentEnumerator != null);
      while (this.enumeratorIsValid) {
        Contract.Assume((this.lastPosition >= position - this.currentFragmentOffset || this.lastPosition >= this.fragmentEnumerator.Current.Source.Length) || 0 <= this.lastPosition);
        this.ToLineColumn(position - this.currentFragmentOffset, ref line, ref column, this.fragmentEnumerator.Current.Source);
        int fragmentLength = this.fragmentEnumerator.Current.Length;
        if (this.currentFragmentOffset + fragmentLength > position) break;
        this.currentFragmentOffset += fragmentLength;
        this.enumeratorIsValid = this.fragmentEnumerator.MoveNext();
        this.lastPosition = 0;
      }
      this.lineCounter = line;
      this.columnCounter = column;
    }

    /// <summary>
    /// Scans the given text string, incrementing line every time a new line sequence is encountered and incrementing column
    /// for every character scanned (resetting column to 1 every time line is incremented).
    /// </summary>
    /// <param name="position">The position at which to stop scanning.</param>
    /// <param name="line">The line counter to increment every time a new line sequence is encountered.</param>
    /// <param name="column">The column counter to increment for every character scanned and to reset to 1 every time a new line sequence is encountered.</param>
    /// <param name="text">The text to scan.</param>
    private void ToLineColumn(int position, ref int line, ref int column, string text) {
      Contract.Requires(text != null);
      Contract.Requires(((this.lastPosition >= position || this.lastPosition >= text.Length) || 0 <= this.lastPosition));

      int i = this.lastPosition;
      int n = text.Length;
      while (i < position && i < n) {
        switch (text[i++]) {
          case '\r':
            if (i < n && text[i] != '\n') {
              line++;
              column = 1;
            }
            break;
          case '\n':
          case (char)0x2028:
          case (char)0x2029:
            line++;
            column = 1;
            break;
          default:
            column++;
            break;
        }
      }
      this.lastPosition = i;
    }

    /// <summary>
    /// The line number computed during the last call to the routine. Initially set to 1.
    /// </summary>
    private int lineCounter = 1;

    /// <summary>
    /// The column number computed during the last call to the routine. Initially set to 1.
    /// </summary>
    private int columnCounter = 1;

    /// <summary>
    /// The position supplied to the last call of this routine. Initially set to 0.
    /// </summary>
    private int lastPosition;

    #region IDerivedSourceDocument Members

    IDerivedSourceLocation IDerivedSourceDocument.DerivedSourceLocation {
      get { return (IDerivedSourceLocation)this.SourceLocation; }
    }

    #endregion
  }

  /// <summary>
  /// A source location that falls inside a region of text that originally came from another source document.
  /// </summary>
  [ContractVerification(false)]
  public class IncludedSourceLocation : IIncludedSourceLocation {

    /// <summary>
    /// Allocates a source location that falls inside a region of text that originally came from another source document.
    /// </summary>
    /// <param name="document">A document with a region of text that orignally came from another source document.</param>
    /// <param name="startIndex">The character index of the first character of this location, when treating the source document as a single string.</param>
    /// <param name="length">The number of characters in this source location.</param>
    internal IncludedSourceLocation(SourceDocumentWithInclusion document, int startIndex, int length) {
      Contract.Requires(document != null);
      Contract.Requires(startIndex >= 0 && (startIndex < document.Length || startIndex == 0));
      Contract.Requires(length >= 0 && length <= document.Length);
      Contract.Requires((startIndex + length) <= document.Length);

      this.sourceDocument = document;
      this.startIndex = startIndex;
      this.length = length;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.sourceDocument != null);
      Contract.Invariant(length >= 0);
      Contract.Invariant(length <= this.SourceDocument.Length);
      Contract.Invariant(0 <= this.StartIndex+length);
      Contract.Invariant(this.StartIndex+length <= this.SourceDocument.Length);
    }

    /// <summary>
    /// True if this source at the given location is completely contained by the source at this location.
    /// </summary>
    [Pure]
    public bool Contains(ISourceLocation location) {
      if (location.SourceDocument != this.SourceDocument) return false;
      int otherPosition = location.StartIndex;
      int otherLength = location.Length;
      return otherPosition >= this.StartIndex && (otherPosition + otherLength) <= (this.StartIndex + this.Length);
    }

    /// <summary>
    /// Copies the specified number of characters to the destination character array, starting at the specified offset from the start if the source location.
    /// </summary>
    /// <param name="offset">The starting index to copy from. Must be greater than zero and less than this.Length.</param>
    /// <param name="destination">The destination array. Must have at least destinationOffset+length elements.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Cannot be more than this.Length-position.</param>
    [Pure]
    public int CopyTo(int offset, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= offset;
      //^^ requires 0 <= destinationOffset;
      //^^ requires 0 <= length;
      //^^ requires 0 <= offset+length;
      //^^ requires offset+length <= this.Length;
      //^^ requires 0 <= destinationOffset+length;
      //^^ requires destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && offset+result <= this.Length;
      //^^ ensures result < length ==> offset+result == this.Length;
    {
      if (offset + length > this.Length) length = this.Length - offset;
      IPrimarySourceDocument wrappedDocument = this.SourceDocument.WrappedDocument;
      //^ assume 0 <= length; //follows from the preconditions
      int position = this.startIndex + offset;
      //^ assume position <= wrappedDocument.Length; //follows from invariant 
      int result = wrappedDocument.CopyTo(position, destination, destinationOffset, length);
      //^ assert 0 <= result;
      //^ assume offset+length <= this.Length; //follows from the precondition
      //^ assert result <= length;
      //^ assert offset+result <= this.Length;
      //^ assume false; //unsatisfied postcondition: ensures 0 <= result && result <= length && offset+result <= this.Length;
      return result;
    }

    /// <summary>
    /// The last column in the last line of the range.
    /// </summary>
    public int EndColumn {
      get {
        int line, column;
        int position = this.StartIndex + this.Length;
        //^ assume position >= 0; //assume no overflow
        //^ assume position <= this.SourceDocument.Length; //follows from invariant: startIndex + this.Length <= SourceDocument.Length;
        //^ assume false; //unsatisfied precondition: requires position >= 0 && position <= this.Length;
        this.SourceDocument.ToLineColumn(position, out line, out column);
        return column;
      }
    }

    /// <summary>
    /// The character index after the last character of this location, when treating the source document as a single string.
    /// </summary>
    public int EndIndex {
      get
        //^^ ensures result >= 0 && result <= this.SourceDocument.Length;
        //^^ ensures result == this.StartIndex + this.Length;
      {
        return this.StartIndex + this.Length;
      }
    }

    /// <summary>
    /// The last line of the range.
    /// </summary>
    public int EndLine {
      get {
        int line, column;
        this.SourceDocument.ToLineColumn(this.EndIndex, out line, out column);
        return line;
      }
    }

    /// <summary>
    /// The line number in the source document where the included region of text starts.
    /// </summary>
    private int InclusionStartLine {
      get {
        int line, column;
        int position = this.SourceDocument.StartingPositionOfIncludedRegion;
        this.SourceDocument.ToLineColumn(position, out line, out column);
        //^ assume line <= this.EndLine;
        return line;
      }
    }

    /// <summary>
    /// The number of characters in this source location.
    /// </summary>
    public int Length {
      get { return this.length; }
    }
    readonly int length;

    /// <summary>
    /// The name of the document from which this text in this source location was originally obtained.
    /// </summary>
    public string OriginalSourceDocumentName {
      get { return this.SourceDocument.OriginalDocumentName; }
    }

    /// <summary>
    /// The last line of the source location in the original document.
    /// </summary>
    public int OriginalEndLine {
      get {
        return this.EndLine - this.InclusionStartLine + this.SourceDocument.OriginalLineNumber;
      }
    }

    /// <summary>
    /// The first line of the source location in the original document.
    /// </summary>
    public int OriginalStartLine {
      get {
        return this.StartLine - this.InclusionStartLine + this.SourceDocument.OriginalLineNumber;
      }
    }

    /// <summary>
    /// The source text corresponding to this location.
    /// </summary>
    public string Source {
      get
        //^^ ensures result.Length == this.Length;
      {
        string sourceText = this.SourceDocument.GetText();
        //^ assume sourceText.Length == this.SourceDocument.Length; //follows from the post condition of GetText
        string result = sourceText.Substring(this.startIndex, this.Length);
        //^ assert result.Length == this.Length;
        //^ assume false; //otherwise Zap complains that the post condition, which is identical to the previous assertion, does not hold.
        return result;
      }
    }

    /// <summary>
    /// The first column in the first line of the range.
    /// </summary>
    public int StartColumn {
      get {
        int line, column;
        int position = this.startIndex;
        //^ assert position >= 0;
        //^ assert position <= this.SourceDocument.Length; //follows from invariant: startIndex < SourceDocument.Length || startIndex == 0;
        //^ assume false; //unsatisfied precondition: requires position >= 0 && position <= this.Length;
        this.SourceDocument.ToLineColumn(position, out line, out column);
        return column;
      }
    }

    /// <summary>
    /// The character index of the first character of this location, when treating the source document as a single string.
    /// </summary>
    public int StartIndex
      //^^ ensures result >= 0 && (result < this.SourceDocument.Length || result == 0);
    {
      get { return this.startIndex; }
    }
    readonly int startIndex;
    //^ invariant startIndex >= 0;
    //^ invariant startIndex < this.SourceDocument.Length || startIndex == 0;
    //^ invariant startIndex + this.Length >= 0;
    //^ invariant startIndex + this.Length <= this.SourceDocument.Length;

    /// <summary>
    /// The first line of the range.
    /// </summary>
    public int StartLine {
      get {
        int line, column;
        int position = this.StartIndex;
        //^ assert position >= 0;
        //^ assert position <= this.SourceDocument.Length; //follows from invariant: startIndex < SourceDocument.Length || startIndex == 0;
        //^ assume false; //unsatisfied precondition: requires position >= 0 && position <= this.Length;
        this.SourceDocument.ToLineColumn(position, out line, out column);
        //^ assume line <= this.EndLine;
        return line;
      }
    }

    /// <summary>
    /// The document (with inclusion), containing the source text of which this location is a subrange.
    /// </summary>
    public SourceDocumentWithInclusion SourceDocument {
      get { return this.sourceDocument; }
    }
    readonly SourceDocumentWithInclusion sourceDocument;


    #region IPrimarySourceLocation Members

    IPrimarySourceDocument IPrimarySourceLocation.PrimarySourceDocument {
      get { return this.SourceDocument; }
    }

    #endregion

    #region ISourceLocation Members

    ISourceDocument ISourceLocation.SourceDocument {
      get { return this.SourceDocument; }
    }

    #endregion

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.SourceDocument; }
    }

    #endregion
  }

  /// <summary>
  /// A wrapper for a source location that is obtained from a region inside a primary source document that was
  /// in fact derived from another document, typically via a preprocessor #include directive. The wrapper
  /// makes the wrapped source location appear as if it were a location in the document that was included.
  /// This is useful for error reporting. For editing, the wrapped location is better.
  /// </summary>
  [ContractVerification(false)]
  public sealed class OriginalSourceLocation : IPrimarySourceLocation {

    /// <summary>
    /// Allocates a wrapper for a source location that is obtained from a region inside a primary source document that was
    /// in fact derived from another document, typically via a preprocessor #include directive. The wrapper
    /// makes the wrapped source location appear as if it were a location in the document that was included.
    /// This is useful for error reporting. For editing, the wrapped location is better.
    /// </summary>
    /// <param name="includedSourceLocation">A source location that falls inside a region of text that originally came from another source document.</param>
    public OriginalSourceLocation(IIncludedSourceLocation includedSourceLocation) {
      this.includedSourceLocation = includedSourceLocation;
    }

    IIncludedSourceLocation includedSourceLocation;

    #region IPrimarySourceLocation Members

    /// <summary>
    /// The last column in the last line of the range.
    /// </summary>
    public int EndColumn {
      get { return this.includedSourceLocation.EndColumn; }
    }

    /// <summary>
    /// The last line of the range.
    /// </summary>
    public int EndLine {
      get { return this.includedSourceLocation.OriginalEndLine; }
    }

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// In this case, the result is always SourceDummy.PrimarySourceDocument.
    /// </summary>
    public IPrimarySourceDocument PrimarySourceDocument {
      get { return SourceDummy.PrimarySourceDocument; }
    }

    /// <summary>
    /// The first column in the first line of the range.
    /// </summary>
    public int StartColumn {
      get { return this.includedSourceLocation.StartColumn; }
    }

    /// <summary>
    /// The first line of the range.
    /// </summary>
    public int StartLine {
      get { return this.includedSourceLocation.OriginalStartLine; }
    }

    #endregion

    #region ISourceLocation Members

    bool ISourceLocation.Contains(ISourceLocation location) {
      return this.includedSourceLocation.Contains(location);
    }

    int ISourceLocation.CopyTo(int offset, char[] destination, int destinationOffset, int length) {
      return this.includedSourceLocation.CopyTo(offset, destination, destinationOffset, length);
    }

    int ISourceLocation.EndIndex {
      get { return this.includedSourceLocation.EndIndex; }
    }

    int ISourceLocation.Length {
      get { return this.includedSourceLocation.Length; }
    }

    ISourceDocument ISourceLocation.SourceDocument {
      get { return this.PrimarySourceDocument; }
    }

    string ISourceLocation.Source {
      get { return this.includedSourceLocation.Source; }
    }

    int ISourceLocation.StartIndex {
      get { return this.includedSourceLocation.StartIndex; }
    }

    #endregion

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.PrimarySourceDocument; }
    }

    #endregion
  }

  /// <summary>
  /// An object that represents a source document that corresponds to a persistable artifact such as a file or an editor buffer.
  /// The document is parsed according to the rules of a particular language, such as C#, to produce an object model that can be obtained via the CompilationPart property.
  /// </summary>
  [ContractVerification(false)]
  public abstract class PrimarySourceDocument : SourceDocument, IPrimarySourceDocument {

    /// <summary>
    /// Allocates an object that represents a source document, such as file, which is parsed according to the rules of a particular langauge, 
    /// such as C#, to produce an object model.
    /// </summary>
    /// <param name="name">The name of the document. Used to identify the document in user interaction.</param>
    /// <param name="location">The location where the document was found or where it will be stored.</param>
    /// <param name="streamReader">A StreamReader instance whose BaseStream produces the contents of the document.</param>
    [ContractVerification(false)]
    protected PrimarySourceDocument(IName name, string location, StreamReader streamReader)
      : base(name) {
      Contract.Requires(name != null);
      Contract.Requires(location != null);
      Contract.Requires(streamReader != null);

      this.location = location;
      this.GetOwnStreamReaderAndReadAhead(streamReader);
    }

    private void GetOwnStreamReaderAndReadAhead(StreamReader streamReader) {
      Contract.Requires(streamReader != null);

      streamReader.BaseStream.Position = 0;
      StreamReader sr = new StreamReader(streamReader.BaseStream, streamReader.CurrentEncoding, false, 128);
      char[] buffer = new char[8192];
      int charsInBuffer = sr.ReadBlock(buffer, 0, buffer.Length);
      //^ assume 0 <= charsInBuffer && charsInBuffer < buffer.Length; //TODO: add a contract to ReadBlock
      this.buffer = buffer;
      this.charsInBuffer = charsInBuffer;
      if (sr.EndOfStream) this.length = charsInBuffer;
      this.streamReader = sr;
      this.streamReaderPosition = charsInBuffer;
    }

    /// <summary>
    /// Allocates an object that represents a source document, such as file, which is parsed according to the rules of a particular langauge, 
    /// such as C#, to produce an object model.
    /// </summary>
    /// <param name="name">The name of the document. Used to identify the document in user interaction.</param>
    /// <param name="location">The location where the document was found or where it will be stored.</param>
    /// <param name="text">The source text of the document.</param>
    [ContractVerification(false)]
    protected PrimarySourceDocument(IName name, string location, string text)
      : base(name) {
      Contract.Requires(name != null);
      Contract.Requires(location != null);
      Contract.Requires(text != null);

      this.location = location;
      this.text = text;
    }

    /// <summary>
    /// Allocates an object that represents a primary source document that is derived from another source document by replacing one substring with another.
    /// </summary>
    /// <param name="text">The text content of new document.</param>
    /// <param name="previousVersion">The source document on which the newly allocated document will be based.</param>
    /// <param name="position">The first character in the previous version of the new document that will be changed in the new document.</param>
    /// <param name="oldLength">The number of characters in the previous verion of the new document that will be changed in the new document.</param>
    /// <param name="newLength">The number of replacement characters in the new document. 
    /// (The length of the string that replaces the substring from position to position+length in the previous version of the new document.)</param>
    [ContractVerification(false)]
    protected PrimarySourceDocument(string text, SourceDocument previousVersion, int position, int oldLength, int newLength)
      : base(previousVersion, position, oldLength, newLength) {
      Contract.Requires(text != null);
      Contract.Requires(previousVersion != null);

      this.location = previousVersion.Location;
      this.text = text;
    }

    /// <summary>
    /// Allocates a copy of the given source document.
    /// </summary>
    [ContractVerification(false)]
    protected PrimarySourceDocument(PrimarySourceDocument template)
      : base(template.Name) {
      Contract.Requires(template != null);

      this.location = template.location;
      if (template.streamReader != null)
        this.GetOwnStreamReaderAndReadAhead(template.streamReader);
      else
        this.text = template.GetText();
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.streamReader == null || this.buffer != null);
      Contract.Invariant(0 <= this.charsInBuffer);
      //Contract.Invariant(this.buffer == null || this.charsInBuffer < this.buffer.Length);
      Contract.Invariant(this.lastPositions != null);
      //Contract.Invariant(this.lastPositions.Length > 0);
    }


    /// <summary>
    /// Copies no more than the specified number of characters to the destination character array, starting at the specified position in the source document.
    /// Returns the actual number of characters that were copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="position">The starting index to copy from. Must be greater than or equal to zero and position+length must be less than or equal to this.Length;</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Must be greater than 0 and less than or equal to the number elements of the destination array.</param>
    public override int CopyTo(int position, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= position && 0 <= length && 0 <= position+length && position <= this.Length;
      //^^ requires 0 <= destinationOffset && 0 <= destinationOffset+length && destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && position+result <= this.Length;
    {
      if (this.text != null) {
        if (position >= this.Length) return 0;
        if (position + length > this.Length) length = this.Length - position;
        this.text.CopyTo(position, destination, destinationOffset, length);
        return length;
      } else if (this.streamReader != null) {
        lock (GlobalLock.LockingObject) {
          int result = 0;
          while (result < length) {
            if (!(this.streamReaderPosition - this.charsInBuffer <= position && position < this.streamReaderPosition))
              this.SetStreamReaderPositionTo(position);
            int charsToCopy = this.streamReaderPosition - position;
            if (charsToCopy <= 0) break;
            int offsetInBuffer = this.charsInBuffer - charsToCopy;
            if (charsToCopy > length-result) charsToCopy = length-result;
            //^ assert this.buffer != null;
            Array.Copy(this.buffer, offsetInBuffer, destination, destinationOffset, charsToCopy);
            result += charsToCopy;
            destinationOffset += charsToCopy;
            position += charsToCopy;
          }
          return result;
        }
      }
      //^ assume false; //Either this.text or this.streamReader should be non null
      return length;
    }

    /// <summary>
    /// A Guid that identifies the kind of document to applications such as a debugger. Typically System.Diagnostics.SymbolStore.SymDocumentType.Text.
    /// </summary>
    public abstract Guid DocumentType {
      get;
    }

    /// <summary>
    /// Obtains a primary source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    public IPrimarySourceLocation GetPrimarySourceLocation(int position, int length)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      return new PrimarySourceLocation(this, position, length);
    }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    //^ [Pure]
    public override ISourceLocation GetSourceLocation(int position, int length)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      return this.GetPrimarySourceLocation(position, length);
    }

    /// <summary>
    /// Returns the source text of the document in string form. Each call may do significant work, so be sure to cache this.
    /// </summary>
    public override string GetText()
      //^^ ensures result.Length == this.Length;
    {
      if (this.text != null) {
        //^ assume this.text.Length == this.Length; //follows from the post-condition of this.Length;
        //^ assume false;
        return this.text;
      }
      if (this.streamReader != null) {
        lock (GlobalLock.LockingObject) {
          long savedPosition = this.streamReader.BaseStream.Position;
          this.streamReader.BaseStream.Position = 0;
          string text = new StreamReader(this.streamReader.BaseStream, this.streamReader.CurrentEncoding, false).ReadToEnd();
          this.text = text;
          this.streamReader.BaseStream.Position = savedPosition;
          this.length = text.Length;
          //^ assume text.Length == this.Length;
          //^ assume false;
          return text;
        }
      }
      //^ assume false; //Either this.text or this.streamReader should be non null
      return string.Empty;
    }
    /// <summary>Caches the entire text of the source document.</summary>
    string/*?*/ text;

    /// <summary>
    /// A Guid that identifies the programming language used in the source document. Typically used by a debugger to locate language specific logic.
    /// </summary>
    public abstract Guid Language {
      get;
    }

    /// <summary>
    /// A Guid that identifies the compiler vendor programming language used in the source document. Typically used by a debugger to locate vendor specific logic.
    /// </summary>
    public abstract Guid LanguageVendor {
      get;
    }

    /// <summary>
    /// The length of the source string.
    /// </summary>
    public override int Length {
      get
        //^^ ensures result >= 0;
        //^^ ensures result.Length == this.Length;
      {
        Contract.Ensures(this.text == null || Contract.Result<int>() == this.text.Length);

        int result = 0;
        if (this.length == null) {
          if (this.text != null)
            result = this.text.Length;
          else if (this.streamReader != null)
            result = this.GetText().Length;
          //^ assert result >= 0;
          //^ assert this.text != null ==> result == this.text.Length;
          this.length = result;
        } else {
          result = (int)this.length;
          //^ assume result >= 0;
          //^ assume this.text != null ==> result == this.text.Length;
        }
        return result;
      }
    }
    int? length;

    /// <summary>
    /// The location where this document was found, or where it should be stored. 
    /// </summary>
    public override string Location {
      get { return this.location; }
    }
    readonly string location;

    /// <summary>
    /// A stream reader that is used to obtain the contents of the document in piecemeal fashion. A stream reader can only advance.
    /// </summary>
    System.IO.StreamReader/*?*/ streamReader;

    /// <summary>
    /// A buffer that serves as a convenient window on the last 8096 (or fewer) characters that were read in from the stream reader. 
    /// Provides an approximation of random access on the underlying stream.
    /// </summary>
    char[]/*?*/ buffer; //^ invariant streamReader != null ==> buffer != null;

    /// <summary>
    /// The number of characters in the buffer that came from the stream. Normally 8096, but will be fewer when the end of the stream has been reached.
    /// </summary>
    private int charsInBuffer;
    //^ invariant 0 <= charsInBuffer;
    //^ invariant buffer != null ==> charsInBuffer < buffer.Length; 

    /// <summary>
    /// The number of characters that have already been read from the stream reader. 
    /// I.e. the index of the next character that will be returned by a call to this.streamReader.Peek().
    /// </summary>
    private int streamReaderPosition;
    //^ invariant 0 <= streamReaderPosition && charsInBuffer <= streamReaderPosition;

    /// <summary>
    /// Advances or resets the stream reader so that this.buffer (the current window on the stream) spans the given position.
    /// </summary>
    private void SetStreamReaderPositionTo(int position)
      //^ requires this.streamReader != null;
      //^ ensures this.streamReaderPosition-this.charsInBuffer <= position && position <= this.streamReaderPosition;
    {
      Contract.Requires(this.streamReader != null);
      Contract.Ensures(this.streamReaderPosition-this.charsInBuffer <= position && position <= this.streamReaderPosition);

      if (position < this.streamReaderPosition - this.charsInBuffer) {
        this.streamReader.BaseStream.Position = 0;
        this.streamReader = new StreamReader(this.streamReader.BaseStream, this.streamReader.CurrentEncoding, false, 128);
        this.charsInBuffer = this.streamReader.ReadBlock(this.buffer, 0, this.buffer.Length);
        this.streamReaderPosition = this.charsInBuffer;
        if (this.streamReader.EndOfStream) {
          this.length = this.streamReaderPosition;
          return;
        }
      }
      while (this.streamReaderPosition <= position) {
        this.charsInBuffer = this.streamReader.ReadBlock(this.buffer, 0, this.buffer.Length);
        this.streamReaderPosition += this.charsInBuffer;
        if (this.streamReader.EndOfStream) {
          this.length = this.streamReaderPosition;
          break;
        }
      }
    }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    public override SourceLocation SourceLocation {
      get {
        Contract.Ensures(Contract.Result<SourceLocation>() is PrimarySourceLocation);

        if (this.sourceLocation == null) {
          lock (GlobalLock.LockingObject) {
            if (this.sourceLocation == null)
              this.sourceLocation = new SourceLocationSpanningEntirePrimaryDocument(this);
          }
        }
        return this.sourceLocation;
      }
    }
    SourceLocationSpanningEntirePrimaryDocument/*?*/ sourceLocation;

    /// <summary>
    /// Maps the given (zero based) source position to a (one based) line and column, by scanning the source character by character, counting
    /// new lines until the given source position is reached. The source position and corresponding line+column are remembered and scanning carries
    /// on where it left off when this routine is called next. If the given position precedes the last given position, we use backward scanning.
    /// Optimal use of this method requires the client to sort calls in order of position.
    /// </summary>
    /// <remarks>This method behaves badly when applied to a really large file since it loads the entire file in memory as a UTF16 unicode string.
    /// In such cases, a slower implementation based on streaming would be more appropriate. However, it is assumed that getting source context
    /// information from such really large files is an extremely rare event and that bad performance in such cases is better than degraded performance
    /// in the common case.</remarks>
    public virtual void ToLineColumn(int position, out int line, out int column)
      //^^ requires position >= 0 && position <= Length;
      //^^ ensures line >= 1 && column >= 1;
    {
      int n = this.Length;
      int offset = position == n ? 1 : 0;
      position -= offset;
      int best = FindBestStartPointForPosition(position);
      line = this.lineCounters[best];
      column = this.columnCounters[best];
      int i = this.lastPositions[best];
      string text = this.GetText();
      if (position < i) {
        if (position < (i - position)) {
          // we need to scan more characters when going backwards from here than starting from the beginning, so do that
          line = 1;
          column = 1;
          i = 0;
          goto forwardSearch;
        }
        column = 1;
        while (i > 0) {
          switch (text[--i]) {
            case '\n':
              if (i > 0 && text[i-1] == '\r')
                i--;
              goto case '\r';
            case '\r':
            case (char)0x2028:
            case (char)0x2029:
              line--;
              if (i <= position) goto forwardSearch;
              break;
          }
        }
      }
    forwardSearch:
      while (i < position) {
        switch (text[i++]) {
          case '\r':
            if (i < n && text[i] == '\n')
              i++;
            goto case '\n';
          case '\n':
          case (char)0x2028:
          case (char)0x2029:
            line++; //TODO: Boogie crashes here, presumably because it does not like ++ on an out parameter
            column = 1;
            //this.positionJumpList[i] = line;
            break;
          default:
            column++;
            break;
        }
      }
      this.lineCounters[best] = line;
      this.columnCounters[best] = column+offset;
      this.lastPositions[best] = position;
    }

    /// <summary>
    /// Maps a one based line and column pair to a zero based character position, by scanning the source character by character, counting
    /// new lines until the given source position is reached. The source position and corresponding line+column are remembered and scanning carries
    /// on where it left off when this routine is called next. If the given position precedes the last given position, scanning restarts from the start.
    /// Optimal use of this method requires the client to sort calls in order of position.
    /// </summary>
    /// <remarks>This method behaves badly when applied to a really large file since it loads the entire file in memory as a UTF16 unicode string.
    /// In such cases, a slower implementation based on streaming would be more appropriate. However, it is assumed that getting source context
    /// information from such really large files is an extremely rare event and that bad performance in such cases is better than degraded performance
    /// in the common case.</remarks>
    public int ToPosition(int line, int column) {
      int best = FindBestStartPointForLine(line);
      int i = this.lastPositions[best];
      int n = this.Length;
      int l = this.lineCounters[best];
      int c = this.columnCounters[best];
      string text = this.GetText();
      while (i > 0) {
        switch (text[--i]) {
          case '\n':
            if (i > 0 && text[i-1] == '\r')
              i--;
            goto case '\r';
          case '\r':
          case (char)0x2028:
          case (char)0x2029:
            l--;
            if (l < line) goto forwardSearch;
            break;
        }
      }
    forwardSearch:
      while (i < n) {
        switch (text[i++]) {
          case '\r':
            if (i < n && text[i] == '\n')
              i++;
            goto case '\n';
          case '\n':
          case (char)0x2028:
          case (char)0x2029:
            l++;
            c = 1;
            break;
          default:
            c++;
            break;
        }
        if (l == line && c == column) break;
      }
      this.lineCounters[best] = l;
      this.columnCounters[best] = c;
      this.lastPositions[best] = i;
      return i;
    }

    /// <summary>
    /// From the set of stored states for position/(line,column) conversion, find the one that is closed to position
    /// </summary>
    private int FindBestStartPointForPosition(int position) {
      int best = 0;
      int bestDistance = int.MaxValue;

      for (int i = 0; i < this.lastPositions.Length; i++) {
        int distance = Math.Abs(this.lastPositions[i] - position);
        if (distance < bestDistance) {
          bestDistance = distance;
          best = i;
        }
      }

      return best;
    }


    /// <summary>
    /// From the set of stored states for position/(line,column) conversion, find the one that is closest to line.
    /// </summary>
    private int FindBestStartPointForLine(int line) {
      int best = 0;
      int bestDistance = int.MaxValue;

      for (int i = 0; i < lineCounters.Length; i++) {
        int distance = Math.Abs(lineCounters[i] - line);
        if (distance < bestDistance) {
          bestDistance = distance;
          best = i;
        }
      }

      return best;
    }

    /// <summary>
    /// The line number computed during the last call to the routine. Initially set to 1.
    /// For non-linear access to position information, keep multiple state sets to reduce
    /// jumping around in the file.
    /// </summary>
    private readonly int[] lineCounters = { 1, 1 };

    /// <summary>
    /// The column number computed during the last call to the routine. Initially set to 1.
    /// For non-linear access to position information, keep multiple state sets to reduce
    /// jumping around in the file.
    /// </summary>
    private readonly int[] columnCounters = { 1, 1 };

    /// <summary>
    /// The position supplied to the last call of this routine. Initially set to 0.
    /// For non-linear access to position information, keep multiple state sets to reduce
    /// jumping around in the file.
    /// </summary>
    private readonly int[] lastPositions = new int[2];

    #region IPrimarySourceDocument Members

    IPrimarySourceLocation IPrimarySourceDocument.PrimarySourceLocation {
      get { return (IPrimarySourceLocation)this.SourceLocation; }
    }

    #endregion

  }

  /// <summary>
  /// An object that represents a source document.
  /// The document is parsed according to the rules of a particular language, such as C#, to produce an object model that can be obtained via the CompilationPart property.
  /// </summary>
  [ContractVerification(false)]
  public abstract class SourceDocument : ISourceDocument {

    /// <summary>
    /// Allocates an object that represents a source document, such as a file, which is parsed according to the rules of a particular langauge, 
    /// such as C#, to produce an object model.
    /// </summary>
    /// <param name="name">The name of the document. Used to identify the document in user interaction.</param>
    protected SourceDocument(IName name) {
      Contract.Requires(name != null);

      this.name = name;
    }

    /// <summary>
    /// Allocates an object that represents a source document that is derived from another source document by replacing one substring with another.
    /// </summary>
    /// <param name="previousVersion">The source document on which the newly allocated document will be based.</param>
    /// <param name="position">The first character in the previous version of the new document that will be changed in the new document.</param>
    /// <param name="oldLength">The number of characters in the previous verion of the new document that will be changed in the new document.</param>
    /// <param name="newLength">The number of replacement characters in the new document. 
    /// (The length of the string that replaces the substring from position to position+length in the previous version of the new document.)</param>
    protected SourceDocument(SourceDocument previousVersion, int position, int oldLength, int newLength) {
      Contract.Requires(previousVersion != null);

      this.name = previousVersion.Name;
      WeakReference wr = new WeakReference(previousVersion);
      //^ assume wr.Target is SourceDocument;
      this.previousVersion = wr;
      this.editStartIndex = position;
      this.editOldLength = oldLength;
      this.editNewLength = newLength;
    }

    /// <summary>
    /// Returns a source location in this document that corresponds to the given source location in a previous version
    /// of this document.
    /// </summary>
    public virtual ISourceLocation GetCorrespondingSourceLocation(ISourceLocation sourceLocationInPreviousVersionOfDocument)
      //^^ requires this.IsUpdatedVersionOf(sourceLocationInPreviousVersionOfDocument.SourceDocument);
    {
      SourceDocument/*?*/ prev = this.previousVersion.Target as SourceDocument;
      Contract.Assume(prev != null); //follows from the precondition because older versions keep younger versions alive.
      if (sourceLocationInPreviousVersionOfDocument.SourceDocument != prev)
        sourceLocationInPreviousVersionOfDocument = prev.GetCorrespondingSourceLocation(sourceLocationInPreviousVersionOfDocument);
      int startIndex = sourceLocationInPreviousVersionOfDocument.StartIndex;
      int length = sourceLocationInPreviousVersionOfDocument.Length;
      int delta = this.editNewLength - this.editOldLength;
      if (startIndex + length < this.editStartIndex) {
        //do nothing since the edit does not affect this location
      } else if (startIndex > this.editStartIndex + this.editOldLength) {
        //The location does not overlap the edit, but may have to move to the left or right, depending on whether the edit deleted or inserted characters
        startIndex += delta;
      } else if (startIndex >= this.editStartIndex) {
        length += delta;
        if (startIndex + length <= this.editStartIndex + this.editOldLength) {
          //The location is inside the edited region.
          if (delta < 0) {
            //The edit deleted characters. Make length correspondingly smaller, but no smaller than 0.
            if (length < 0) length = 0;
          } else {
            //The edit inserted characters. Make length correspondingly larger, but not do not go beyond the edit.
            if (startIndex + length > this.editStartIndex + this.editNewLength)
              length = (this.editStartIndex + this.editNewLength) - startIndex;
          }
        } else {
          //The location starts inside the edit region but carries on.
          if (delta < 0) {
            //The edit deleted characters. Make length correspondingly smaller, but no smaller than 0.
            if (length < 0) length = 0;
          } else {
            //The edit inserted characters. Make length correspondingly larger, but not do not go beyond the end of the document.
            if (startIndex + length > this.Length)
              length = this.Length - startIndex;
          }

        }
      } else if (startIndex + length < this.editStartIndex + this.editOldLength) {
        //The location starts before the edit region and ends inside it.
        if (delta < 0) {
          //The edit deleted characters. Make length correspondingly smaller, but no smaller than 0.
          if (startIndex + length < this.editStartIndex) {
            length = this.editStartIndex - startIndex;
            //Contract.Assume(length >= 0);
          }
        }
      } else {
        //The location contains the edit.
        length += delta;
      }
      Contract.Assume(length <= this.Length);
      Contract.Assume(length >= 0);
      return this.GetSourceLocation(startIndex, length);
    }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    //^ [Pure]
    public abstract ISourceLocation GetSourceLocation(int position, int length);
    //^^ requires 0 <= position && (position < this.Length || position == 0);
    //^^ requires 0 <= length;
    //^^ requires length <= this.Length;
    //^^ requires position+length <= this.Length;
    //^^ ensures result.SourceDocument == this;
    //^^ ensures result.StartIndex == position;
    //^^ ensures result.Length == length;

    /// <summary>
    /// Returns true if this source document has been created by editing the given source document (or a updated
    /// version of the given source document).
    /// </summary>
    //^ [Confined]
    public virtual bool IsUpdatedVersionOf(ISourceDocument sourceDocument) {
      WeakReference/*?*/ previousDocument = this.previousVersion;
      while (previousDocument != null) {
        SourceDocument/*?*/ prev = previousDocument.Target as SourceDocument;
        if (prev == null) return false;
        if (prev == sourceDocument) return true;
        previousDocument = prev.previousVersion;
      }
      return false;
    }

    /// <summary>
    /// The length of the source string.
    /// </summary>
    public abstract int Length {
      get;
    }

    /// <summary>
    /// The location where this document was found, or where it should be stored. 
    /// </summary>
    public abstract string Location {
      get;
    }

    /// <summary>
    /// The name of the document. For example the name of the file if the document corresponds to a file.
    /// </summary>
    public virtual IName Name {
      get { return this.name; }
    }
    readonly IName name;

    /// <summary>
    /// A weak reference to the previous version of this document. It can differ by at most one edit from this version.
    /// </summary>
    private WeakReference/*?*/ previousVersion;
    //^ invariant previousVersion != null && previousVersion.Target != null ==> previousVersion.Target is SourceDocument;

    /// <summary>
    /// The index of the first character of the string that has been replaced in the previous version in order to create this version.
    /// </summary>
    private int editStartIndex;

    /// <summary>
    /// The length of the string that has replaced editStartIndex through editOldLength in the previous version in order to create this version.
    /// </summary>
    private int editNewLength;

    /// <summary>
    /// The length of the string that has been replaced in the previous version in order to create this version.
    /// </summary>
    private int editOldLength;

    /// <summary>
    /// The language that determines how the document is parsed and what it means.
    /// </summary>
    public abstract string SourceLanguage { get; }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    public abstract SourceLocation SourceLocation {
      get;
    }

    /// <summary>
    /// Copies no more than the specified number of characters to the destination character array, starting at the specified position in the source document.
    /// Returns the actual number of characters that were copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="position">The starting index to copy from. Must be greater than or equal to zero and position+length must be less than or equal to this.Length;</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Must be greater than 0 and less than or equal to the number elements of the destination array.</param>
    public abstract int CopyTo(int position, char[] destination, int destinationOffset, int length);
    //^^ requires 0 <= position && 0 <= length && 0 <= position+length && position <= this.Length;
    //^^ requires 0 <= destinationOffset && 0 <= destinationOffset+length && destinationOffset+length <= destination.Length;
    //^^ ensures 0 <= result && result <= length && position+result <= this.Length;

    /// <summary>
    /// Returns the source text of the document in string form. Each call may do significant work, so be sure to cache this.
    /// </summary>
    public abstract string GetText();
    //^^ ensures result.Length == this.Length;

    #region ISourceDocument Members

    ISourceLocation ISourceDocument.SourceLocation {
      get { return this.SourceLocation; }
    }

    #endregion
  }

  /// <summary>
  /// An object that represents a primary source document that has a region of text that originally came from another source document.
  /// </summary>
  [ContractVerification(false)]
  public sealed class SourceDocumentWithInclusion : IPrimarySourceDocument {

    /// <summary>
    /// Allocates an object that represents a primary source document that has a region of text that originally came from another source document.
    /// </summary>
    /// <param name="wrappedDocument">A primary source document that has an inclusion (but does not know about it).</param>
    /// <param name="originalLineNumber">The starting line number that the included region of text has in the the document it originated from.</param>
    /// <param name="originalDocumentName">The name of the document from which the included region of text has originated.</param>
    /// <param name="startingPositionOfIncludedRegion">The position in the wrapped document where the included region starts.</param>
    public SourceDocumentWithInclusion(IPrimarySourceDocument wrappedDocument, int originalLineNumber, string originalDocumentName, int startingPositionOfIncludedRegion) {
      Contract.Requires(wrappedDocument != null);
      Contract.Requires(0 <= startingPositionOfIncludedRegion && startingPositionOfIncludedRegion < wrappedDocument.Length);

      this.wrappedDocument = wrappedDocument;
      this.originalLineNumber = originalLineNumber;
      this.originalDocumentName = originalDocumentName;
      this.startingPositionOfIncludedRegion = startingPositionOfIncludedRegion;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(0 <= this.startingPositionOfIncludedRegion && this.startingPositionOfIncludedRegion < this.Length);
    }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    public IPrimarySourceLocation GetPrimarySourceLocation(int position, int length)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      return new IncludedSourceLocation(this, position, length);
    }

    /// <summary>
    /// Obtains a source location instance that corresponds to the substring of the document specified by the given start position and length.
    /// </summary>
    [Pure]
    public ISourceLocation GetSourceLocation(int position, int length)
      //^^ requires 0 <= position && (position < this.Length || position == 0);
      //^^ requires 0 <= length;
      //^^ requires length <= this.Length;
      //^^ requires position+length <= this.Length;
      //^^ ensures result.SourceDocument == this;
      //^^ ensures result.StartIndex == position;
      //^^ ensures result.Length == length;
    {
      return this.GetPrimarySourceLocation(position, length);
    }

    /// <summary>
    /// The name of the document from which the included region of text has originated.
    /// </summary>
    public string OriginalDocumentName {
      get {
        return this.originalDocumentName;
      }
    }
    readonly string originalDocumentName;

    /// <summary>
    /// The starting line number that the included region of text has in the in the document it originated from.
    /// </summary>
    public int OriginalLineNumber {
      get {
        return this.originalLineNumber;
      }
    }
    readonly int originalLineNumber;

    /// <summary>
    /// The position in the document where the included region of text starts.
    /// </summary>
    public int StartingPositionOfIncludedRegion {
      get {
        Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < this.Length);
        return this.startingPositionOfIncludedRegion;
      }
    }
    //^ invariant 0 <= startingPositionOfIncludedRegion && startingPositionOfIncludedRegion < this.Length;
    readonly int startingPositionOfIncludedRegion;

    /// <summary>
    /// A source document that has an inclusion (but does not know about it).
    /// </summary>
    public IPrimarySourceDocument WrappedDocument {
      get { return this.wrappedDocument; }
    }
    readonly IPrimarySourceDocument wrappedDocument;

    #region IPrimarySourceDocument Members

    /// <summary>
    /// A Guid that identifies the kind of document to applications such as a debugger. Typically System.Diagnostics.SymbolStore.SymDocumentType.Text.
    /// </summary>
    public Guid DocumentType {
      get { return this.WrappedDocument.DocumentType; }
    }

    /// <summary>
    /// A Guid that identifies the programming language used in the source document. Typically used by a debugger to locate language specific logic.
    /// </summary>
    public Guid Language {
      get { return this.WrappedDocument.Language; }
    }

    /// <summary>
    /// A Guid that identifies the compiler vendor programming language used in the source document. Typically used by a debugger to locate vendor specific logic.
    /// </summary>
    public Guid LanguageVendor {
      get { return this.WrappedDocument.LanguageVendor; }
    }

    #endregion

    #region ISourceDocument Members

    /// <summary>
    /// Copies no more than the specified number of characters to the destination character array, starting at the specified position in the source document.
    /// Returns the actual number of characters that were copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="position">The starting index to copy from. Must be greater than or equal to zero and position+length must be less than or equal to this.Length;</param>
    /// <param name="destination">The destination array.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Must be greater than 0 and less than or equal to the number elements of the destination array.</param>
    public int CopyTo(int position, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= position && 0 <= length && 0 <= position+length && position <= this.Length;
      //^^ requires 0 <= destinationOffset && 0 <= destinationOffset+length && destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && position+result <= this.Length;
    {
      return this.WrappedDocument.CopyTo(position, destination, destinationOffset, length);
    }

    /// <summary>
    /// Returns a source location in this document that corresponds to the given source location from a previous version
    /// of this document.
    /// </summary>
    public ISourceLocation GetCorrespondingSourceLocation(ISourceLocation sourceLocationInPreviousVersionOfDocument)
      //^^ requires this.IsUpdatedVersionOf(sourceLocationInPreviousVersionOfDocument.SourceDocument);
    {
      ISourceDocument previousSourceDocument = sourceLocationInPreviousVersionOfDocument.SourceDocument;
      //^ assume previousSourceDocument is SourceDocumentWithInclusion; //otherwise this could not be an updated version of previousSourceDocument
      ISourceDocument previousWrappedDocument = ((SourceDocumentWithInclusion)previousSourceDocument).WrappedDocument;
      int position = sourceLocationInPreviousVersionOfDocument.StartIndex;
      //^ assert position >= 0 && (position < sourceLocationInPreviousVersionOfDocument.SourceDocument.Length || position == 0); //follows from post condition
      //^ assume position >= 0 && (position < previousWrappedDocument.Length || position == 0); //assume that things stay the same when unwrapping
      int prevLength = sourceLocationInPreviousVersionOfDocument.Length;
      //^ assert prevLength >= 0 && sourceLocationInPreviousVersionOfDocument.StartIndex+prevLength <= sourceLocationInPreviousVersionOfDocument.SourceDocument.Length;
      //^ assume prevLength <= previousWrappedDocument.Length && position+prevLength <= previousWrappedDocument.Length; //assume that things stay the same when unwrapping
      ISourceLocation previousWrappedLocation = previousWrappedDocument.GetSourceLocation(position, prevLength);
      ISourceDocument wrappedDocument = this.WrappedDocument;
      //^ assume wrappedDocument.IsUpdatedVersionOf(previousWrappedLocation.SourceDocument);
      ISourceLocation newWrappedLocation = wrappedDocument.GetCorrespondingSourceLocation(previousWrappedLocation);
      int newLength = newWrappedLocation.Length;
      //^ assume newLength <= this.Length; //assume that things stay the same when unwrapping
      //^ assume false;
      return this.GetSourceLocation(newWrappedLocation.StartIndex, newLength);
    }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    public ISourceLocation SourceLocation {
      get { return this.PrimarySourceLocation; }
    }

    /// <summary>
    /// Returns the source text of the document in string form. Each call may do significant work, so be sure to cache this.
    /// </summary>
    public string GetText()
      //^ ensures result.Length == this.Length;
    {
      string result = this.WrappedDocument.GetText();
      //^ assume result.Length == this.Length; //assume that things stay the same when unwrapping
      //^ assume false;
      return result;
    }

    /// <summary>
    /// Returns true if this source document has been created by editing the given source document (or a updated
    /// version of the given source document).
    /// </summary>
    //^ [Confined]
    public bool IsUpdatedVersionOf(ISourceDocument sourceDocument) {
      return this.WrappedDocument.IsUpdatedVersionOf(sourceDocument);
    }

    /// <summary>
    /// The length of the source string.
    /// </summary>
    public int Length {
      get { return this.WrappedDocument.Length; }
    }

    /// <summary>
    /// The language that determines how the document is parsed and what it means.
    /// </summary>
    public string SourceLanguage {
      get { return this.WrappedDocument.SourceLanguage; }
    }

    /// <summary>
    /// A source location corresponding to the entire document.
    /// </summary>
    public IPrimarySourceLocation PrimarySourceLocation {
      get {
        if (this.primarySourceLocation == null)
          this.primarySourceLocation = new SourceLocationSpanningEntirePrimaryDocument(this);
        return this.primarySourceLocation;
      }
    }
    //^ [Once]
    IPrimarySourceLocation/*?*/ primarySourceLocation;

    /// <summary>
    /// Maps the given (zero based) source position to a (one based) line and column, by scanning the source character by character, counting
    /// new lines until the given source position is reached. The source position and corresponding line+column are remembered and scanning carries
    /// on where it left off when this routine is called next. If the given position precedes the last given position, scanning restarts from the start.
    /// Optimal use of this method requires the client to sort calls in order of position.
    /// </summary>
    public void ToLineColumn(int position, out int line, out int column)
      //^^ requires position >= 0 && position <= this.Length;
      //^^ ensures line >= 1 && column >= 1;
    {

      //^ assume position <= this.wrappedDocument.Length; //assume that things stay the same when unwrapping
      this.wrappedDocument.ToLineColumn(position, out line, out column);
    }

    #endregion

    #region IDocument Members

    /// <summary>
    /// The location where this document was found, or where it should be stored.
    /// This will also uniquely identify the source docuement within an instance of compilation host.
    /// </summary>
    public string Location {
      get { return this.WrappedDocument.Location; }
    }

    /// <summary>
    /// The name of the document. For example the name of the file if the document corresponds to a file.
    /// </summary>
    public IName Name {
      get { return this.WrappedDocument.Name; }
    }

    #endregion
  }

  /// <summary>
  /// A range of source text that corresponds to an identifiable entity.
  /// </summary>
  [ContractVerification(false)]
  public abstract class SourceLocation : ISourceLocation {

    /// <summary>
    /// Allocates a range of source text that corresponds to an identifiable entity.
    /// </summary>
    /// <param name="startIndex">The character index of the first character of this location, when treating the source document as a single string.</param>
    /// <param name="length">The number of characters in this source location.</param>
    protected SourceLocation(int startIndex, int length)
      //^ requires startIndex >= 0;
      //^ requires length >= 0;
      //^ requires startIndex + length >= 0;
    {
      this.startIndex = startIndex;
      this.length = length;
    }

    /// <summary>
    /// True if this source at the given location is completely contained by the source at this location.
    /// </summary>
    //^ [Pure]
    public bool Contains(ISourceLocation location) {
      if (location.SourceDocument != this.SourceDocument) {
        CompositeSourceDocument/*?*/ cdoc = this.SourceDocument as CompositeSourceDocument;
        if (cdoc == null) return false;
        IDerivedSourceLocation/*?*/ dloc = this as DerivedSourceLocation;
        if (dloc == null) return false;
        bool containsStart = false;
        bool containsEnd = false;
        //^ assume dloc.DerivedSourceDocument == cdoc;
        foreach (ISourceLocation sloc in cdoc.GetPrimarySourceLocationsFor(dloc)) {
          if (sloc.SourceDocument != location.SourceDocument) continue;
          if (sloc.StartIndex <= location.StartIndex && sloc.EndIndex >= location.StartIndex)
            containsStart = true;
          if (sloc.StartIndex <= location.EndIndex && sloc.EndIndex >= location.EndIndex)
            containsEnd = true;
        }
        return containsStart && containsEnd;
      }
      int otherPosition = location.StartIndex;
      int otherLength = location.Length;
      return otherPosition >= this.StartIndex && (otherPosition + otherLength) <= (this.StartIndex + this.Length);
    }

    /// <summary>
    /// Copies the specified number of characters to the destination character array, starting at the specified offset from the start if the source location.
    /// Returns the number of characters actually copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="offset">The starting index to copy from. Must be greater than zero and less than this.Length.</param>
    /// <param name="destination">The destination array. Must have at least destinationOffset+length elements.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Cannot be more than this.Length-position.</param>
    //^ [Pure]
    public virtual int CopyTo(int offset, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= offset;
      //^^ requires 0 <= destinationOffset;
      //^^ requires 0 <= length;
      //^^ requires 0 <= offset+length;
      //^^ requires 0 <= destinationOffset+length;
      //^^ requires destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && offset+result <= this.Length;
      //^^ ensures result < length ==> offset+result == this.Length;
    {
      if (offset + length > this.Length) length = this.Length - offset;
      ISourceDocument sourceDocument = this.SourceDocument;
      //^ assume 0 <= length; //follows from the preconditions
      int position = this.startIndex + offset;
      //^ assume position <= sourceDocument.Length; //follows from invariant 
      int result = sourceDocument.CopyTo(position, destination, destinationOffset, length);
      //^ assert 0 <= result;
      //^ assume offset+length <= this.Length; //follows from the precondition
      //^ assert result <= length;
      //^ assert offset+result <= this.Length;
      //^ assume false; //unsatisfied postcondition: ensures 0 <= result && result <= length && offset+result <= this.Length;
      return result;
    }

    /// <summary>
    /// The character index after the last character of this location, when treating the source document as a single string.
    /// </summary>
    public int EndIndex
      //^^ ensures result >= 0 && result <= this.SourceDocument.Length;
      //^^ ensures result == this.StartIndex + this.Length;
    {
      get {
        int result = this.StartIndex + this.Length;
        //^ assume false;
        return result;
      }
    }

    /// <summary>
    /// The number of characters in this source location.
    /// </summary>
    public virtual int Length {
      get
        //^^ ensures result >= 0;
        //^^ ensures this.StartIndex+result <= this.SourceDocument.Length;
      {
        int result = this.length;
        //^ assume this.StartIndex+result <= this.SourceDocument.Length; //follows from the post condition of this.SourceDocument
        return result;
      }
    }
    internal int length;
    //^ invariant length >= 0;

    /// <summary>
    /// The source text corresponding to this location.
    /// </summary>
    public string Source {
      get
        //^^ ensures result.Length == this.Length;
      {
        char[] chars = new char[this.Length];
        //^ assume 0+this.Length <= chars.Length;
        ISourceDocument sourceDocument = this.SourceDocument;
        //^ assume this.StartIndex + this.Length <= sourceDocument.Length;
        sourceDocument.CopyTo(this.StartIndex, chars, 0, this.Length);
        string result = new string(chars);
        //^ assume result.Length == ((ISourceLocation)this).Length; //TODO: provide contract for new string(char[])
        return result;
      }
    }

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    public abstract ISourceDocument SourceDocument {
      get;
      //^ ensures this.StartIndex < result.Length || this.StartIndex == 0;
      //^ ensures this.StartIndex + this.Length <= result.Length;
    }

    /// <summary>
    /// The character index of the first character of this location, when treating the source document as a single string.
    /// </summary>
    public int StartIndex {
      get
        //^^ ensures result >= 0 && (result < this.SourceDocument.Length || result == 0);
      {
        int result = this.startIndex;
        //^ assume result >= 0 && (result < ((ISourceLocation)this).SourceDocument.Length || result == 0); //follows from invariant and post condition of this.SourceDocument
        return result;
      }
    }
    readonly int startIndex;
    //^ invariant this.startIndex >= 0;
    //^ invariant this.startIndex + this.length >= 0;

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.SourceDocument; }
    }

    #endregion
  }

  /// <summary>
  /// A range of source text that corresponds to an identifiable entity.
  /// </summary>
  public class DerivedSourceLocation : SourceLocation, IDerivedSourceLocation {

    /// <summary>
    /// Allocates a range of source text that corresponds to an identifiable entity.
    /// </summary>
    /// <param name="derivedSourceDocument">The document containing the source text of which this location is a subrange.</param>
    /// <param name="startIndex">The character index of the first character of this location, when treating the source document as a single string.</param>
    /// <param name="length">The number of characters in this source location.</param>
    public DerivedSourceLocation(IDerivedSourceDocument derivedSourceDocument, int startIndex, int length)
      : base(startIndex, length) {
      Contract.Requires(derivedSourceDocument != null);
      //Contract.Requires(startIndex >= 0 && startIndex <= derivedSourceDocument.Length);
      //Contract.Requires(length >= 0 && length <= derivedSourceDocument.Length);
      //Contract.Requires((startIndex + length) <= derivedSourceDocument.Length);

      this.derivedSourceDocument = derivedSourceDocument;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.derivedSourceDocument != null);
      //Contract.Invariant(this.Length <= this.SourceDocument.Length);
      //Contract.Invariant(this.StartIndex <= this.SourceDocument.Length);
      //Contract.Invariant(this.StartIndex+this.Length <= this.SourceDocument.Length);
    }

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    public IDerivedSourceDocument DerivedSourceDocument {
      get { return this.derivedSourceDocument; }
    }
    readonly IDerivedSourceDocument derivedSourceDocument;

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    public override ISourceDocument SourceDocument {
      get { return this.DerivedSourceDocument; }
    }

    #region IDerivedSourceLocation Members

    IEnumerable<IPrimarySourceLocation> IDerivedSourceLocation.PrimarySourceLocations {
      get { return this.DerivedSourceDocument.GetPrimarySourceLocationsFor(this); }
    }

    #endregion
  }

  /// <summary>
  /// A range of source text that corresponds to an identifiable entity.
  /// </summary>
  public class PrimarySourceLocation : SourceLocation, IPrimarySourceLocation {

    /// <summary>
    /// Allocates a range of source text that corresponds to an identifiable entity.
    /// </summary>
    /// <param name="primarySourceDocument">The document containing the source text of which this location is a subrange.</param>
    /// <param name="startIndex">The character index of the first character of this location, when treating the source document as a single string.</param>
    /// <param name="length">The number of characters in this source location.</param>
    [ContractVerification(false)]
    public PrimarySourceLocation(IPrimarySourceDocument primarySourceDocument, int startIndex, int length)
      : base(startIndex, length) {
      Contract.Requires(primarySourceDocument != null);
      Contract.Requires(startIndex >= 0 && startIndex <= primarySourceDocument.Length);
      Contract.Requires(length >= 0 && length <= primarySourceDocument.Length);
      //Contract.Requires(0 <= startIndex + length);
      Contract.Requires(startIndex + length <= primarySourceDocument.Length);

      this.primarySourceDocument = primarySourceDocument;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.primarySourceDocument != null);
      Contract.Invariant(this.Length <= this.PrimarySourceDocument.Length);
      Contract.Invariant(this.StartIndex <= this.PrimarySourceDocument.Length);
      Contract.Invariant(0 <= this.StartIndex+this.Length);
      Contract.Invariant(this.StartIndex+this.Length <= this.PrimarySourceDocument.Length);
    }

    /// <summary>
    /// The last column in the last line of the range.
    /// </summary>
    public int EndColumn {
      get
        //^^ ensures result >= 0;
      {
        int line, column;
        int position = this.StartIndex + this.Length;
        IPrimarySourceDocument primarySourceDocument = this.PrimarySourceDocument;
        primarySourceDocument.ToLineColumn(position, out line, out column);
        return column;
      }
    }

    /// <summary>
    /// The last line of the range.
    /// </summary>
    public int EndLine {
      get
        //^^ ensures result >= 0;
      {
        int line, column;
        int position = this.StartIndex + this.Length;
        IPrimarySourceDocument primarySourceDocument = this.PrimarySourceDocument;
        primarySourceDocument.ToLineColumn(position, out line, out column);
        return line;
      }
    }

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    public IPrimarySourceDocument PrimarySourceDocument {
      get { return this.primarySourceDocument; }
    }
    readonly IPrimarySourceDocument primarySourceDocument;

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    //^ [Pure]
    public override ISourceDocument SourceDocument {
      get { return this.PrimarySourceDocument; }
    }

    /// <summary>
    /// The first column in the first line of the range.
    /// </summary>
    public int StartColumn {
      get {
        int line, column;
        int position = this.StartIndex;
        IPrimarySourceDocument primarySourceDocument = this.PrimarySourceDocument;
        primarySourceDocument.ToLineColumn(position, out line, out column);
        return column;
      }
    }

    /// <summary>
    /// The first line of the range.
    /// </summary>
    public int StartLine {
      get {
        int line, column;
        int position = this.StartIndex;
        IPrimarySourceDocument primarySourceDocument = this.PrimarySourceDocument;
        primarySourceDocument.ToLineColumn(position, out line, out column);
        //^ IPrimarySourceLocation ithis = (IPrimarySourceLocation)this;
        //^ assume line <= ithis.EndLine;
        //^ assume line == ithis.EndLine ==> ithis.StartColumn <= ithis.EndColumn;
        return line;
      }
    }

  }

  /// <summary>
  /// A source location that spans entire primary source document, but that delays computing this.Length until needed so that the document can be streamed.
  /// </summary>
  [ContractVerification(false)]
  public sealed class SourceLocationSpanningEntirePrimaryDocument : PrimarySourceLocation {

    /// <summary>
    /// Allocates source location that spans entire primary source document, but that delays computing this.Length until needed so that the document can be streamed.
    /// </summary>
    /// <param name="primarySourceDocument">The document that the resulting source location spans.</param>
    public SourceLocationSpanningEntirePrimaryDocument(IPrimarySourceDocument primarySourceDocument)
      : base(primarySourceDocument, 0, 0) {
      Contract.Requires(primarySourceDocument != null);
    }

    /// <summary>
    /// Copies the specified number of characters to the destination character array, starting at the specified offset from the start if the source location.
    /// Returns the number of characters actually copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="offset">The starting index to copy from. Must be greater than zero and less than this.Length.</param>
    /// <param name="destination">The destination array. Must have at least destinationOffset+length elements.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Cannot be more than this.Length-position.</param>
    //^ [Pure]
    public override int CopyTo(int offset, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= offset;
      //^^ requires 0 <= destinationOffset;
      //^^ requires 0 <= length;
      //^^ requires 0 <= offset+length;
      //^^ requires 0 <= destinationOffset+length;
      //^^ requires destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && offset+result <= this.Length;
      //^^ ensures result < length ==> offset+result == this.Length;
    {
      ISourceDocument sourceDocument = this.SourceDocument;
      //^ assume 0 <= length; //follows from the preconditions
      int position = this.StartIndex + offset;
      //^ assume position <= sourceDocument.Length; //follows from invariant 
      int result = sourceDocument.CopyTo(position, destination, destinationOffset, length);
      //^ assert 0 <= result;
      //^ assume offset+length <= this.Length; //follows from the precondition
      //^ assert result <= length;
      //^ assert offset+result <= this.Length;
      //^ assume false; //unsatisfied postcondition: ensures 0 <= result && result <= length && offset+result <= this.Length;
      return result;
    }

    /// <summary>
    /// The number of characters in this source location.
    /// </summary>
    public override int Length {
      get {
        if (this.length == 0)
          this.length = this.SourceDocument.Length;
        return this.length;
      }
    }
  }

  /// <summary>
  /// A source location that spans entire derived source document, but that delays computing this.Length until needed so that the document can be streamed.
  /// </summary>
  [ContractVerification(false)]
  public sealed class SourceLocationSpanningEntireDerivedSourceDocument : DerivedSourceLocation {

    /// <summary>
    /// Allocates a source location that spans entire derived source document, but that delays computing this.Length until needed so that the document can be streamed.
    /// </summary>
    /// <param name="derivedSourceDocument"></param>
    public SourceLocationSpanningEntireDerivedSourceDocument(IDerivedSourceDocument derivedSourceDocument)
      : base(derivedSourceDocument, 0, 0) {
      Contract.Requires(derivedSourceDocument != null);
    }

    /// <summary>
    /// Copies the specified number of characters to the destination character array, starting at the specified offset from the start if the source location.
    /// Returns the number of characters actually copied. This number will be greater than zero as long as position is less than this.Length.
    /// The number will be precisely the number asked for unless there are not enough characters left in the document.
    /// </summary>
    /// <param name="offset">The starting index to copy from. Must be greater than zero and less than this.Length.</param>
    /// <param name="destination">The destination array. Must have at least destinationOffset+length elements.</param>
    /// <param name="destinationOffset">The starting index where the characters must be copied to in the destination array.</param>
    /// <param name="length">The maximum number of characters to copy. Cannot be more than this.Length-position.</param>
    //^ [Pure]
    public override int CopyTo(int offset, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= offset;
      //^^ requires 0 <= destinationOffset;
      //^^ requires 0 <= length;
      //^^ requires 0 <= offset+length;
      //^^ requires 0 <= destinationOffset+length;
      //^^ requires destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && offset+result <= this.Length;
      //^^ ensures result < length ==> offset+result == this.Length;
    {
      ISourceDocument sourceDocument = this.SourceDocument;
      //^ assume 0 <= length; //follows from the preconditions
      int position = this.StartIndex + offset;
      //^ assume position <= sourceDocument.Length; //follows from invariant 
      int result = sourceDocument.CopyTo(position, destination, destinationOffset, length);
      //^ assert 0 <= result;
      //^ assert result <= length;
      //^ assert offset+result <= this.Length;
      //^ assume false; //unsatisfied postcondition: ensures 0 <= result && result <= length && offset+result <= this.Length;
      return result;
    }

    /// <summary>
    /// The number of characters in this source location.
    /// </summary>
    public override int Length {
      get {
        if (this.length == 0)
          this.length = this.SourceDocument.Length;
        return this.length;
      }
    }
  }

  /// <summary>
  /// Describes an edit to a compilation as being either the addition, deletion or modification of a definition.
  /// </summary>
  [ContractVerification(false)]
  public sealed class EditDescriptor : IEditDescriptor {

    /// <summary>
    /// Allocates an object that describes an edit to a compilation as being either the addition, deletion or modification of a definition.
    /// </summary>
    /// <param name="kind">The kind of edit that has been performed (addition, deletion or modification).</param>
    /// <param name="affectedDefinition">The definition that has been added, deleted or modified.</param>
    /// <param name="modifiedParent">The new version of the parent of the affected definition (see also this.OriginalParent).
    /// If the edit is an addition or modification, this.ModifiedParent is the actual parent of this.AffectedDefinition.
    /// If this.AffectedDefinition does not have a parent then this.ModifiedParent is the same as this.AffectedDefinition.</param>
    /// <param name="originalParent">The original parent of the affected definition (see also this.ModifiedParent). 
    /// If the edit is a deletion, this.OriginalParent is the parent of this.AffectedDefinition.
    /// If this.AffectedDefinition does not have a parent then this.OriginalParent is the same as this.AffectedDefinition.</param>
    /// <param name="modifiedSourceDocument">The source document that is the result of the edit described by this edit instance.</param>
    /// <param name="originalSourceDocument">The source document that has been edited as described by this edit instance.</param>
    public EditDescriptor(EditEventKind kind, IDefinition affectedDefinition,
      IDefinition modifiedParent, IDefinition originalParent, ISourceDocument modifiedSourceDocument, ISourceDocument originalSourceDocument) {
      Contract.Requires(affectedDefinition != null);
      Contract.Requires(modifiedParent != null);
      Contract.Requires(originalParent != null);
      Contract.Requires(modifiedSourceDocument != null);
      Contract.Requires(originalSourceDocument != null);
      Contract.Requires(modifiedSourceDocument.IsUpdatedVersionOf(originalSourceDocument));

      this.kind = kind;
      this.affectedDefinition = affectedDefinition;
      this.modifiedSourceDocument = modifiedSourceDocument;
      this.modifiedParent = modifiedParent;
      this.originalSourceDocument = originalSourceDocument;
      this.originalParent = originalParent;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.affectedDefinition != null);
      Contract.Invariant(this.modifiedParent != null);
      Contract.Invariant(this.modifiedSourceDocument != null);
      Contract.Invariant(this.originalParent != null);
      Contract.Invariant(this.originalSourceDocument != null);
      Contract.Invariant(this.modifiedSourceDocument.IsUpdatedVersionOf(this.originalSourceDocument));
    }

    /// <summary>
    /// The definition that has been added, deleted or modified.
    /// </summary>
    public IDefinition AffectedDefinition {
      get { return this.affectedDefinition; }
    }
    readonly IDefinition affectedDefinition;

    /// <summary>
    /// The kind of edit that has been performed (addition, deletion or modification).
    /// </summary>
    public EditEventKind Kind {
      get { return this.kind; }
    }
    readonly EditEventKind kind;

    /// <summary>
    /// The compilation part that is the result of the edit described by this edit instance.
    /// </summary>
    public ISourceDocument ModifiedSourceDocument {
      get {
        return this.modifiedSourceDocument;
      }
    }
    readonly ISourceDocument modifiedSourceDocument;
    //^ invariant modifiedSourceDocument.IsUpdatedVersionOf(originalSourceDocument);

    /// <summary>
    /// The new version of the parent of the affected definition (see also this.OriginalParent).
    /// If the edit is an addition or modification, this.ModifiedParent is the actual parent of this.AffectedDefinition.
    /// If this.AffectedDefinition does not have a parent then this.ModifiedParent is the same as this.AffectedDefinition.
    /// </summary>
    public IDefinition ModifiedParent {
      get { return this.modifiedParent; }
    }
    readonly IDefinition modifiedParent;

    /// <summary>
    /// The source document that has been edited as described by this edit instance.
    /// </summary>
    public ISourceDocument OriginalSourceDocument {
      get { return this.originalSourceDocument; }
    }
    readonly ISourceDocument originalSourceDocument;

    /// <summary>
    /// The original parent of the affected definition (see also this.ModifiedParent). 
    /// If the edit is a deletion, this.OriginalParent is the parent of this.AffectedDefinition.
    /// If this.AffectedDefinition does not have a parent then this.OriginalParent is the same as this.AffectedDefinition.
    /// </summary>
    public IDefinition OriginalParent {
      get { return this.originalParent; }
    }
    readonly IDefinition originalParent;

  }

  /// <summary>
  /// A mutable class that allows a source location to be built in an incremental fashion.
  /// </summary>
  public sealed class SourceLocationBuilder : ISourceLocation {

    /// <summary>
    /// Allocates a mutable class that allows a source location to be built in an incremental fashion.
    /// </summary>
    /// <param name="sourceLocation">An initial source location.</param>
    public SourceLocationBuilder(ISourceLocation sourceLocation) {
      Contract.Requires(sourceLocation != null);
      Contract.Ensures(this.SourceDocument == sourceLocation.SourceDocument);

      this.sourceDocument = sourceLocation.SourceDocument;
      this.length = sourceLocation.Length;
      this.startIndex = sourceLocation.StartIndex;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(0 <= this.length);
      Contract.Invariant(this.length <= this.sourceDocument.Length);
      Contract.Invariant(this.startIndex+this.length <= this.sourceDocument.Length);
      Contract.Invariant(0 <= this.startIndex);
      Contract.Invariant(this.startIndex < this.sourceDocument.Length || this.startIndex == 0);
    }


    /// <summary>
    /// The current length of the source location being built.
    /// </summary>
    int length;
    //^ invariant 0 <= this.length;
    //^ invariant this.length <= this.sourceDocument.Length;
    //^ invariant this.startIndex+this.length <= this.sourceDocument.Length;

    /// <summary>
    /// The current startIndex of the source location being built.
    /// </summary>
    int startIndex;
    //^ invariant 0 <= this.startIndex;
    //^ invariant this.startIndex < this.sourceDocument.Length || this.startIndex == 0;

    /// <summary>
    /// The document containing the source text of which this location is a subrange.
    /// </summary>
    public ISourceDocument SourceDocument {
      get { return this.sourceDocument; }
    }
    ISourceDocument sourceDocument;

    /// <summary>
    /// Make the smallest update to the current start index and/or length so that the value of this.GetSourceLocation resulting from a subsequent call will span the given source location.
    /// </summary>
    /// <param name="sourceLocation"></param>
    [ContractVerification(false)]
    public void UpdateToSpan(ISourceLocation sourceLocation) {
      Contract.Requires(sourceLocation != null);

      int slStartIndex = sourceLocation.StartIndex;
      int slLength = sourceLocation.Length;
      int myStartIndex = this.startIndex;
      int myLength = this.length;
      int newStartIndex;
      int newLength;
      if (slStartIndex < myStartIndex) {
        newLength = myLength + (myStartIndex - slStartIndex);
        newStartIndex = slStartIndex;
      } else if (slStartIndex == myStartIndex) {
        newLength = slLength > myLength ? slLength : myLength;
        newStartIndex = myStartIndex;
      } else {
        newLength = (slStartIndex + slLength) - myStartIndex;
        newStartIndex = myStartIndex;
      }
      this.length = newLength;
      this.startIndex = newStartIndex;
    }

    /// <summary>
    /// Returns a source location that spans the initial source location and any other source locations subsequently provided via calls to this.UpdateToSpan.
    /// </summary>
    [Pure]
    public ISourceLocation GetSourceLocation() {
      Contract.Ensures(Contract.Result<ISourceLocation>() != null);
      Contract.Ensures(Contract.Result<ISourceLocation>().StartIndex == this.startIndex);
      Contract.Ensures(Contract.Result<ISourceLocation>().Length == this.length);
      Contract.Ensures(Contract.Result<ISourceLocation>().SourceDocument == this.sourceDocument);

      return this.sourceDocument.GetSourceLocation(this.startIndex, this.length);
    }

    #region ISourceLocation Members

    bool ISourceLocation.Contains(ISourceLocation location) {
      return this.GetSourceLocation().Contains(location);
    }

    int ISourceLocation.CopyTo(int offset, char[] destination, int destinationOffset, int length)
      //^^ requires 0 <= offset;
      //^^ requires 0 <= destinationOffset;
      //^^ requires 0 <= length;
      //^^ requires 0 <= offset+length;
      //^^ requires 0 <= destinationOffset+length;
      //^^ requires destinationOffset+length <= destination.Length;
      //^^ ensures 0 <= result && result <= length && offset+result <= this.Length;
      //^^ ensures result < length ==> offset+result == this.Length;
    {
      ISourceLocation sloc = this.GetSourceLocation();
      //^ assert sloc.StartIndex == this.startIndex;
      //^ assert sloc.Length == this.length;
      Contract.Assume(this.length == ((ISourceLocation)this).Length);
      return sloc.CopyTo(offset, destination, destinationOffset, length);
    }

    int ISourceLocation.EndIndex {
      get {
        ISourceLocation sloc = this.GetSourceLocation();
        //^ assert sloc.StartIndex == this.startIndex;
        //^ assert sloc.Length == this.length;
        int result = sloc.EndIndex;
        //^ assert result == this.startIndex + this.length;
        //^ assume result == ((ISourceLocation)this).StartIndex + ((ISourceLocation)this).Length;
        //^ assert result <= this.SourceDocument.Length;
        Contract.Assume(result <= ((ISourceLocation)this).SourceDocument.Length);
        return result;
      }
    }

    int ISourceLocation.Length {
      get
        //^^ ensures result >= 0;
        //^^ ensures this.StartIndex+result <= this.SourceDocument.Length;
        //^ ensures result == this.GetSourceLocation().Length; 
      {
        ISourceLocation sloc = this.GetSourceLocation();
        //^ assert sloc.StartIndex == this.startIndex;
        //^ assert sloc.Length == this.length;
        int result = sloc.Length;
        //^ assert this.startIndex + result <= this.SourceDocument.Length;
        Contract.Assume(((ISourceLocation)this).StartIndex + result <= ((ISourceLocation)this).SourceDocument.Length);
        return result;
      }
    }

    ISourceDocument ISourceLocation.SourceDocument {
      get { return this.SourceDocument; }
    }

    string ISourceLocation.Source {
      get
        //^^ ensures result.Length == this.Length;
      {
        ISourceLocation sloc = this.GetSourceLocation();
        //^ assert sloc.Length == this.length;
        Contract.Assume(this.length == ((ISourceLocation)this).Length);
        string result = sloc.Source;
        //^ assert result.Length == sloc.Length;
        Contract.Assume(result.Length == ((ISourceLocation)this).Length); //result.Length == sloc.Length == this.Length == ((ISourceLocation)this).Length;
        return result;
      }
    }

    int ISourceLocation.StartIndex {
      get
        //^^ ensures result >= 0 && (result < this.SourceDocument.Length || result == 0);
        //^ ensures result == this.GetSourceLocation().StartIndex; 
      {
        ISourceLocation sloc = this.GetSourceLocation();
        //^ assert sloc.StartIndex == this.startIndex;
        //^ assert sloc.Length == this.length;
        //^ assert sloc.SourceDocument == this.sourceDocument;
        int result = sloc.StartIndex;
        //^ assert result >= 0 && (result < sloc.SourceDocument.Length || result == 0);
        //^ assert result == this.startIndex;
        //^ assert result < this.sourceDocument.Length || result == 0;
        Contract.Assume(result < ((ISourceLocation)this).SourceDocument.Length || result == 0);
        return result;
      }
    }

    #endregion

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.SourceDocument; }
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class SynchronizationPointLocation : ILocation {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="document"></param>
    /// <param name="synchronizationPoint"></param>
    public SynchronizationPointLocation(IDocument document, ISynchronizationPoint synchronizationPoint) {
      Contract.Requires(document != null);
      Contract.Requires(synchronizationPoint != null);

      this.document = document;
      this.synchronizationPoint = synchronizationPoint;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.document != null);
      Contract.Invariant(this.synchronizationPoint != null);
    }

    /// <summary>
    /// 
    /// </summary>
    public ISynchronizationPoint SynchronizationPoint {
      get { return this.synchronizationPoint; }
    }
    ISynchronizationPoint synchronizationPoint;

    /// <summary>
    /// The document containing this location.
    /// </summary>
    public IDocument Document {
      get { return this.document; }
    }
    IDocument document;

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class ContinuationLocation : ILocation {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="synchronizationPointLocation"></param>
    public ContinuationLocation(SynchronizationPointLocation synchronizationPointLocation) {
      Contract.Requires(synchronizationPointLocation != null);

      this.synchronizationPointLocation = synchronizationPointLocation;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.synchronizationPointLocation != null);
    }

    /// <summary>
    /// 
    /// </summary>
    public SynchronizationPointLocation SynchronizationPointLocation {
      get { return this.synchronizationPointLocation; }
    }
    SynchronizationPointLocation synchronizationPointLocation;

    /// <summary>
    /// The document containing this location.
    /// </summary>
    public IDocument Document {
      get { return this.synchronizationPointLocation.Document; }
    }

  }

}
