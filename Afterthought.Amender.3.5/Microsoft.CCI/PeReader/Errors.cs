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
using Microsoft.Cci.MetadataReader.PEFileFlags;
using Microsoft.Cci.UtilityDataStructures;
using System.Globalization;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.MetadataReader.Errors {
  /// <summary>
  /// Represents a location in a directory of the PE File.
  /// </summary>
  public interface IDirectoryLocation : ILocation {
    /// <summary>
    /// The binary document corresponding to the PE File.
    /// </summary>
    IBinaryDocument BinaryDocument { get; }

    /// <summary>
    /// The name of the directory of the PE File.
    /// </summary>
    string DirectoryName { get; }

    /// <summary>
    /// Offset into the directory.
    /// </summary>
    uint Offset { get; }
  }

  /// <summary>
  /// Represents a location in the Metadata stream.
  /// </summary>
  public interface IMetadataStreamLocation : ILocation {
    /// <summary>
    /// The binary document corresponding to the metadata stream.
    /// </summary>
    IBinaryDocument BinaryDocument { get; }

    /// <summary>
    /// The name of the metadata stream corresponding to the location.
    /// </summary>
    string StreamName { get; }

    /// <summary>
    /// Offset into the IL Stream.
    /// </summary>
    uint Offset { get; }
  }

  /// <summary>
  /// Represents a location in the Metadata tables.
  /// </summary>
  public interface IMetadataLocation : ILocation {
    /// <summary>
    /// The binary document corresponding to the metadata tables.
    /// </summary>
    IBinaryDocument BinaryDocument { get; }

    /// <summary>
    /// The name of the table represented by the location.
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// The row number corresponding to the location.
    /// </summary>
    int RowId { get; }
  }

  internal sealed class DirectoryLocation : IDirectoryLocation {
    internal readonly IBinaryDocument binaryDocument;
    internal readonly Directories directory;
    internal readonly uint offset;

    internal DirectoryLocation(
      IBinaryDocument binaryDocument,
      Directories directory,
      uint offset
    ) {
      this.binaryDocument = binaryDocument;
      this.directory = directory;
      this.offset = offset;
    }

    #region IMetadataStreamLocation Members

    public IBinaryDocument BinaryDocument {
      get { return this.binaryDocument; }
    }

    public string DirectoryName {
      get { return this.directory.ToString(); }
    }

    public uint Offset {
      get { return this.offset; }
    }

    #endregion

    #region ILocation Members

    public IDocument Document {
      get { return this.binaryDocument; }
    }

    #endregion

    //^ [Confined, MustOverride]
    public override bool Equals(object/*?*/ obj) {
      DirectoryLocation/*?*/ directoryLocation = obj as DirectoryLocation;
      if (directoryLocation == null)
        return false;
      if (this.offset != directoryLocation.offset)
        return false;
      if (this.directory == directoryLocation.directory)
        return false;
      return this.binaryDocument.Location.Equals(directoryLocation.binaryDocument.Location);
    }

    //^ [Confined, MustOverride]
    public override int GetHashCode() {
      return this.offset.GetHashCode() ^ this.directory.GetHashCode();
    }

    //^ [Confined, MustOverride]
    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat(CultureInfo.InvariantCulture, "DirectoryLocation({0},{1},{2})", this.binaryDocument.Location, this.directory.ToString(), this.offset);
      return sb.ToString();
    }
  }

  internal sealed class MetadataStreamLocation : IMetadataStreamLocation {
    internal readonly IBinaryDocument binaryDocument;
    internal readonly string streamName;
    internal readonly uint offset;

    internal MetadataStreamLocation(
      IBinaryDocument binaryDocument,
      string streamName,
      uint offset
    ) {
      this.binaryDocument = binaryDocument;
      this.streamName = streamName;
      this.offset = offset;
    }

    #region IMetadataStreamLocation Members

    public IBinaryDocument BinaryDocument {
      get { return this.binaryDocument; }
    }

    public string StreamName {
      get { return this.streamName; }
    }

    public uint Offset {
      get { return this.offset; }
    }

    #endregion

    #region ILocation Members

    public IDocument Document {
      get { return this.binaryDocument; }
    }

    #endregion

    //^ [Confined, MustOverride]
    public override bool Equals(object/*?*/ obj) {
      MetadataStreamLocation/*?*/ metadataStreamLocation = obj as MetadataStreamLocation;
      if (metadataStreamLocation == null)
        return false;
      if (this.offset != metadataStreamLocation.offset)
        return false;
      if (!this.streamName.Equals(metadataStreamLocation.streamName))
        return false;
      return this.binaryDocument.Location.Equals(metadataStreamLocation.binaryDocument.Location);
    }

    //^ [Confined, MustOverride]
    public override int GetHashCode() {
      return this.offset.GetHashCode() ^ this.streamName.GetHashCode();
    }

    //^ [Confined, MustOverride]
    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat(CultureInfo.InvariantCulture, "MetadataStreamLocation({0},{1},{2})", this.binaryDocument.Location, this.streamName, this.offset);
      return sb.ToString();
    }
  }

  internal sealed class MetadataLocation : IMetadataLocation {
    internal readonly IBinaryDocument binaryDocument;
    internal readonly TableIndices tableIndex;
    internal readonly uint rowId;

    internal MetadataLocation(
      IBinaryDocument binaryDocument,
      TableIndices tableIndex,
      uint rowId
    ) {
      this.binaryDocument = binaryDocument;
      this.tableIndex = tableIndex;
      this.rowId = rowId;
    }

    #region IMetadataLocation Members

    public IBinaryDocument BinaryDocument {
      get { return this.binaryDocument; }
    }

    public string TableName {
      get { return this.tableIndex.ToString(); }
    }

    public int RowId {
      get { return (int)this.rowId; }
    }

    #endregion

    #region ILocation Members

    public IDocument Document {
      get { return this.binaryDocument; }
    }

    #endregion

    //^ [Confined, MustOverride]
    public override bool Equals(object/*?*/ obj) {
      MetadataLocation/*?*/ metadataLocation = obj as MetadataLocation;
      if (metadataLocation == null)
        return false;
      if (this.rowId != metadataLocation.rowId)
        return false;
      if (this.tableIndex != metadataLocation.tableIndex)
        return false;
      return this.binaryDocument.Location.Equals(metadataLocation.binaryDocument.Location);
    }

    //^ [Confined, MustOverride]
    public override int GetHashCode() {
      return this.rowId.GetHashCode() ^ this.tableIndex.GetHashCode();
    }

    //^ [Confined, MustOverride]
    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat(CultureInfo.InvariantCulture, "MetadataLocation({0},{1},{2})", this.binaryDocument.Location, this.tableIndex.ToString(), this.rowId);
      return sb.ToString();
    }
  }

  internal sealed class MetadataReaderErrorMessage : IErrorMessage {
    private readonly object errorReporter;
    internal readonly ILocation location;
    internal readonly MetadataReaderErrorKind mrwErrorKind;

    internal MetadataReaderErrorMessage(
      object errorReporter,
      ILocation location,
      MetadataReaderErrorKind mrwErrorKind
    ) {
      this.errorReporter = errorReporter;
      this.location = location;
      this.mrwErrorKind = mrwErrorKind;
    }

    #region IErrorMessage Members

    public object ErrorReporter {
      get { return this.errorReporter; }
    }

    public string ErrorReporterIdentifier {
      get { return "MR"; }
    }

    public long Code {
      get { return (long)mrwErrorKind; }
    }

    public bool IsWarning {
      get { return false; }
    }

    public string Message {
      get {
        System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("Microsoft.Cci.PeReaderErrorMessages", typeof(MetadataReaderErrorMessage).Assembly);
        string messageKey = this.mrwErrorKind.ToString();
        string/*?*/ localizedString = null;
        try {
          localizedString = resourceManager.GetString(messageKey);
        } catch (System.Resources.MissingManifestResourceException) {
        }
        try {
          if (localizedString == null) {
            localizedString = resourceManager.GetString(messageKey, System.Globalization.CultureInfo.InvariantCulture);
          }
        } catch (System.Resources.MissingManifestResourceException) {
        }
        if (localizedString == null) {
          localizedString = messageKey;
        }
        return localizedString;
      }
    }

    public ILocation Location {
      get { return this.location; }
    }

    public IEnumerable<ILocation> RelatedLocations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

  }

  internal enum MetadataReaderErrorKind : int {
    FileSizeTooSmall,
    DosHeader,
    PESignature,
    COFFHeaderTooSmall,
    UnknownPEMagic,
    OptionalHeaderStandardFields32TooSmall,
    OptionalHeaderStandardFields64TooSmall,
    OptionalHeaderNTAdditionalFields32TooSmall,
    OptionalHeaderNTAdditionalFields64TooSmall,
    OptionalHeaderDirectoryEntriesTooSmall,
    SectionHeadersTooSmall,
    NotEnoughSpaceForCOR20HeaderTableDirectory,
    COR20HeaderTooSmall,
    NotEnoughSpaceForMetadataDirectory,
    MetadataHeaderTooSmall,
    MetadataSignature,
    NotEnoughSpaceForVersionString,
    StorageHeaderTooSmall,
    StreamHeaderTooSmall,
    NotEnoughSpaceForStreamHeaderName,
    NotEnoughSpaceForStringStream,
    NotEnoughSpaceForBlobStream,
    NotEnoughSpaceForGUIDStream,
    NotEnoughSpaceForUserStringStream,
    NotEnoughSpaceForMetadataStream,
    UnknownMetadataStream,
    MetadataTableHeaderTooSmall,
    UnknownVersionOfMetadata,
    UnknownTables,
    SomeRequiredTablesNotSorted,
    IllegalTablesInCompressedMetadataStream,
    TableRowCountSpaceTooSmall,
    MetadataTablesTooSmall,
    NestedClassParentError,
    UnknownILInstruction,
  }

  internal sealed class MetadataErrorContainer {
    readonly PeReader MetadataReader;
    readonly IBinaryDocument BinaryDocument;
    readonly MultiHashtable<MetadataReaderErrorMessage> ErrorList;

    internal MetadataErrorContainer(
      PeReader metadataReader,
      IBinaryDocument binaryDocument
    ) {
      this.MetadataReader = metadataReader;
      this.BinaryDocument = binaryDocument;
      this.ErrorList = new MultiHashtable<MetadataReaderErrorMessage>();
    }
    void AddMetadataReaderErrorMessage(MetadataReaderErrorMessage errMessage) {
      this.ErrorList.Add((uint)errMessage.mrwErrorKind, errMessage);
      this.MetadataReader.metadataReaderHost.ReportError(errMessage);
    }
    internal void AddBinaryError(
      uint offset,
      MetadataReaderErrorKind errorKind
    ) {
      foreach (MetadataReaderErrorMessage errMessage in this.ErrorList.GetValuesFor((uint)errorKind)) {
        IBinaryLocation/*?*/ binaryLocation = errMessage.Location as IBinaryLocation;
        if (binaryLocation == null) {
          continue;
        }
        if (binaryLocation.Offset == offset) {
          return;
        }
      }
      this.AddMetadataReaderErrorMessage(new MetadataReaderErrorMessage(this.MetadataReader.ErrorsReporter, new BinaryLocation(this.BinaryDocument, offset), errorKind));
    }
    internal void AddDirectoryError(
      Directories directory,
      uint offset,
      MetadataReaderErrorKind errorKind
    ) {
      foreach (MetadataReaderErrorMessage errMessage in this.ErrorList.GetValuesFor((uint)errorKind)) {
        DirectoryLocation/*?*/ directoryLocation = errMessage.Location as DirectoryLocation;
        if (directoryLocation == null) {
          continue;
        }
        if (directoryLocation.offset == offset && directoryLocation.directory == directory) {
          return;
        }
      }
      this.AddMetadataReaderErrorMessage(new MetadataReaderErrorMessage(this.MetadataReader.ErrorsReporter, new DirectoryLocation(this.BinaryDocument, directory, offset), errorKind));
    }
    internal void AddMetadataStreamError(
      string streamName,
      uint offset,
      MetadataReaderErrorKind errorKind
    ) {
      foreach (MetadataReaderErrorMessage errMessage in this.ErrorList.GetValuesFor((uint)errorKind)) {
        MetadataStreamLocation/*?*/ mdStreamLocation = errMessage.Location as MetadataStreamLocation;
        if (mdStreamLocation == null) {
          continue;
        }
        if (mdStreamLocation.offset == offset && mdStreamLocation.streamName.Equals(streamName)) {
          return;
        }
      }
      this.AddMetadataReaderErrorMessage(new MetadataReaderErrorMessage(this.MetadataReader.ErrorsReporter, new MetadataStreamLocation(this.BinaryDocument, streamName, offset), errorKind));
    }
    internal void AddMetadataError(
      TableIndices tableIndex,
      uint rowId,
      MetadataReaderErrorKind errorKind
    ) {
      foreach (MetadataReaderErrorMessage errMessage in this.ErrorList.GetValuesFor((uint)errorKind)) {
        MetadataLocation/*?*/ mdLocation = errMessage.Location as MetadataLocation;
        if (mdLocation == null) {
          continue;
        }
        if (mdLocation.rowId == rowId && mdLocation.tableIndex == tableIndex) {
          return;
        }
      }
      this.AddMetadataReaderErrorMessage(new MetadataReaderErrorMessage(this.MetadataReader.ErrorsReporter, new MetadataLocation(this.BinaryDocument, tableIndex, rowId), errorKind));
    }
    internal void AddILError(
      IMethodDefinition methodDefinition,
      uint offset,
      MetadataReaderErrorKind errorKind
    ) {
      foreach (MetadataReaderErrorMessage errMessage in this.ErrorList.GetValuesFor((uint)errorKind)) {
        IILLocation/*?*/ ilLocation = errMessage.Location as IILLocation;
        if (ilLocation == null) {
          continue;
        }
        if (ilLocation.Offset == offset && ilLocation.MethodDefinition.Equals(methodDefinition)) {
          return;
        }
      }
      this.AddMetadataReaderErrorMessage(new MetadataReaderErrorMessage(this.MetadataReader.ErrorsReporter, new ILLocation(this.BinaryDocument, methodDefinition, offset), errorKind));
    }
  }
}
