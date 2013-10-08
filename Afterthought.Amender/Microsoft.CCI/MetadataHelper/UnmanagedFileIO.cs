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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

#if !COMPACTFX && !__MonoCS__
  /// <summary>
  /// A managed wrapper for an unmanaged memory mapped file.
  /// Important: each instance of this class holds a read-lock on a file. Instances should be explicitly disposed as soon as they become inactive.
  /// </summary>
  public unsafe sealed class MemoryMappedFile : IBinaryDocumentMemoryBlock, IDisposable {

    private MemoryMappedFile(
      IBinaryDocument binaryDocument,
      byte* buffer,
      uint length
    ) {
      this.binaryDocument = binaryDocument;
      this.buffer = buffer;
      this.length = length;
    }

    /// <summary>
    /// Finalizer for the Memory mapped file. Calls the CloseMap.
    /// </summary>
    ~MemoryMappedFile() {
      this.Close();
    }

    /// <summary>
    /// Frees the memory mapped file.
    /// </summary>
    public void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    private void Close() {
      if (buffer != null) {
        MemoryMappedFile.UnmapViewOfFile(buffer);
        buffer = null;
      }
    }

    #region IBinaryDocumentMemoryBlock Members

    byte* IBinaryDocumentMemoryBlock.Pointer {
      get { return this.buffer; }
    }
    private byte* buffer;

    uint IBinaryDocumentMemoryBlock.Length {
      get { return this.length; }
    }
    private uint length;

    IBinaryDocument IBinaryDocumentMemoryBlock.BinaryDocument {
      get { return this.binaryDocument; }
    }
    private IBinaryDocument binaryDocument;

    #endregion

    /// <summary>
    /// Factory method for opening the memory mapped file. The content of the map is assumed to come from localFileName.
    /// This can throw FileLoadException in case of error.
    /// </summary>
    /// <param name="localFileName">Name of the file from where the binary document needs to be opened.
    /// This is useful in case we want to copy the file to temporary location and then open or when we want to open document on the network.</param>
    /// <param name="binaryDocument">The binary document for which the memory mapping is requested.</param>
    public static MemoryMappedFile CreateMemoryMappedFile(
      string localFileName,
      IBinaryDocument binaryDocument
    ) {
      uint length;
      Byte* buffer;
      MemoryMappedFile.OpenFileMemoryMap(localFileName, out buffer, out length);
      if (length != binaryDocument.Length)
        throw new IOException("File size difference: " + localFileName);
      return new MemoryMappedFile(
        binaryDocument,
        buffer,
        length
      );
    }

    private static void OpenFileMemoryMap(string filename, out Byte* buffer, out uint length) {
      IntPtr hmap;
      using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        if (stream.Length > Int32.MaxValue)
          throw new IOException("File too Big: " + filename);
        length = unchecked((uint)stream.Length);
        Microsoft.Win32.SafeHandles.SafeFileHandle/*?*/ safeHandle = stream.SafeFileHandle;
        if (safeHandle == null) {
          throw new IOException("Unable to create Memory map: " + filename);
        }
        hmap = MemoryMappedFile.CreateFileMapping(safeHandle.DangerousGetHandle(), IntPtr.Zero, PageAccess.PAGE_READONLY, 0, length, null);
        if (hmap == IntPtr.Zero) {
          int rc = Marshal.GetLastWin32Error();
          throw new IOException("Unable to create Memory map: " + filename + " - " + rc.ToString("X", CultureInfo.InvariantCulture));
        }
      }
      buffer = (byte*)MemoryMappedFile.MapViewOfFile(hmap, FileMapAccess.FILE_MAP_READ, 0, 0, (IntPtr)length);
      MemoryMappedFile.CloseHandle(hmap);
      if (buffer == null) {
        int rc = Marshal.GetLastWin32Error();
        throw new IOException("Unable to create Memory map: " + filename + " - " +rc.ToString("X", CultureInfo.InvariantCulture));
      }
    }

  #region Interop stuff
    private enum PageAccess : int { PAGE_READONLY = 0x02 };
    private enum FileMapAccess : int { FILE_MAP_READ = 0x0004 };

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CreateFileMapping(
      IntPtr hFile,           // handle to file
      IntPtr lpAttributes,    // security
      PageAccess flProtect,   // protection
      uint dwMaximumSizeHigh,  // high-order DWORD of size
      uint dwMaximumSizeLow,   // low-order DWORD of size
      string/*?*/ lpName           // object name
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern void* MapViewOfFile(
      IntPtr hFileMappingObject,      // handle to file-mapping object
      FileMapAccess dwDesiredAccess,  // access mode
      int dwFileOffsetHigh,           // high-order DWORD of offset
      int dwFileOffsetLow,            // low-order DWORD of offset
      IntPtr dwNumberOfBytesToMap        // number of bytes to map
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnmapViewOfFile(
      void* lpBaseAddress // starting address
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(
      IntPtr hObject  // handle to object
    );

  #endregion Interop stuff
  }
#endif

  /// <summary>
  /// A managed wrapper for a block of memory allocated from the unmanaged heap.
  /// Important: each instance of this class holds a read-lock on a file. Instances should be explicitly disposed as soon as they become inactive.
  /// </summary>
  public unsafe sealed class UnmanagedBinaryMemoryBlock : IBinaryDocumentMemoryBlock, IDisposable {
    IBinaryDocument binaryDocument;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
    IntPtr Pointer;

    private UnmanagedBinaryMemoryBlock(IBinaryDocument binaryDocument) {
      this.binaryDocument = binaryDocument;
      this.Pointer = Marshal.AllocHGlobal((int)binaryDocument.Length);
      if (this.Pointer == IntPtr.Zero) {
        throw new OutOfMemoryException();
      }
    }

    /// <summary>
    /// Frees the umanaged memory block.
    /// </summary>
    ~UnmanagedBinaryMemoryBlock() {
      this.Close();
    }

    /// <summary>
    /// Frees the umanaged memory block.
    /// </summary>
    public void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    private void Close() {
      if (this.Pointer != IntPtr.Zero)
        Marshal.FreeHGlobal(this.Pointer);
      this.Pointer = IntPtr.Zero;
    }

    #region IBinaryDocumentMemoryBlock Members

    IBinaryDocument IBinaryDocumentMemoryBlock.BinaryDocument {
      get { return this.binaryDocument; }
    }

    byte* IBinaryDocumentMemoryBlock.Pointer {
      get { return (byte*)this.Pointer; }
    }

    uint IBinaryDocumentMemoryBlock.Length {
      get { return this.binaryDocument.Length; }
    }

    #endregion

    /// <summary>
    /// Creates an unmanaged binary memory block and copies the contents of the file at the given location into the block.
    /// </summary>
    /// <param name="localFileName">The path to the file to read.</param>
    /// <param name="binaryDocument">The binary document whose contents are stored in the given file.</param>
    /// <exception cref="System.ArgumentException">localFileName is an empty string (""), contains only white space, or contains one
    /// or more invalid characters. -or- localFileName refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
    /// <exception cref="System.NotSupportedException">localFileName refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
    /// <exception cref="System.IO.FileNotFoundException">The file specified by localFileName does not exist.</exception>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="System.UnauthorizedAccessException">The file cannot be be read, for example because it is already being accessed exclusively by another process.</exception>
    /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms,
    /// paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
    public static UnmanagedBinaryMemoryBlock CreateUnmanagedBinaryMemoryBlock(string localFileName, IBinaryDocument binaryDocument) {
      using (FileStream stream = new FileStream(localFileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        return CreateUnmanagedBinaryMemoryBlock(stream, binaryDocument);
      }
    }

    /// <summary>
    /// Creates an unmanaged binary memory block and copies the contents of the given stream into the block.
    /// </summary>
    /// <param name="stream">A stream of bytes that are to be copied into the resulting memory block.</param>
    /// <param name="binaryDocument">The binary document whose contents are stored in the given file.</param>
    /// <exception cref="System.IO.IOException">The length of the stream is not the same as the length of the binary document, or the stream length is greater than Int32.MaxValue.</exception>
    public static UnmanagedBinaryMemoryBlock CreateUnmanagedBinaryMemoryBlock(Stream stream, IBinaryDocument binaryDocument) {
      if (stream.Length != binaryDocument.Length)
        throw new IOException("stream.Length != binaryDocument.Length: " + binaryDocument.Location);
      if (stream.Length > Int32.MaxValue)
        throw new IOException("stream.Length > Int32.MaxValue: " + binaryDocument.Location);
      UnmanagedBinaryMemoryBlock unmanagedBinaryMemoryBlock = new UnmanagedBinaryMemoryBlock(binaryDocument);
      byte* pMainBuffer = (byte*)unmanagedBinaryMemoryBlock.Pointer;
      //Read a fixed length block at a time, so that the GC does not come under pressure from lots of large byte arrays.
      int remainingLength = (int)binaryDocument.Length;
      int copyBufferLength = 8096;
      byte[] tempBuffer = new byte[copyBufferLength];
      fixed (byte* tempBufferPtr = tempBuffer) {
        while (remainingLength > 0) {
          if (remainingLength < copyBufferLength) {
            copyBufferLength = remainingLength;
          }
          stream.Read(tempBuffer, 0, copyBufferLength);
          byte* iterBuffer = tempBufferPtr;
          byte* endBuffer = tempBufferPtr + copyBufferLength;
          while (iterBuffer < endBuffer) {
            *pMainBuffer++ = *iterBuffer++;
          }
          remainingLength -= copyBufferLength;
        }
      }
      return unmanagedBinaryMemoryBlock;
    }

    /// <summary>
    /// Creates an unmanaged binary memory block and copies the contents of the given byte enumeration into the block.
    /// </summary>
    /// <param name="stream">A stream of bytes that are to be copied into the resulting memory block.</param>
    /// <param name="binaryDocument">The binary document whose contents are stored in the given file.</param>
    /// <exception cref="System.IO.IOException">The length of the stream is not the same as the length of the binary document, or the stream length is greater than Int32.MaxValue.</exception>
    public static UnmanagedBinaryMemoryBlock CreateUnmanagedBinaryMemoryBlock(IEnumerable<byte> stream, IBinaryDocument binaryDocument) {
      UnmanagedBinaryMemoryBlock unmanagedBinaryMemoryBlock = new UnmanagedBinaryMemoryBlock(binaryDocument);
      byte* pMainBuffer = (byte*)unmanagedBinaryMemoryBlock.Pointer;
      byte* endOfBuffer = pMainBuffer + binaryDocument.Length;
      foreach (var b in stream) {
        if (pMainBuffer == endOfBuffer) throw new IOException("stream length != binaryDocument.Length: " + binaryDocument.Location);
        *pMainBuffer++ = b;
      }
      if (pMainBuffer != endOfBuffer) throw new IOException("stream length != binaryDocument.Length: " + binaryDocument.Location);
      return unmanagedBinaryMemoryBlock;
    }

  }

  /// <summary>
  /// Class representing a binary document
  /// </summary>
  public sealed class BinaryDocument : IBinaryDocument {

    /// <summary>
    /// Constructor for the Binay Document.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="name"></param>
    /// <param name="length"></param>
    public BinaryDocument(string location, IName name, uint length) {
      this.location = location;
      this.name = name;
      this.length = length;
    }

    #region IBinaryDocument Members

    uint IBinaryDocument.Length {
      get { return this.length; }
    }
    uint length;

    #endregion

    #region IDocument Members

    string IDocument.Location {
      get { return this.location; }
    }
    string location;

    IName IDocument.Name {
      get { return this.name; }
    }
    IName name;

    #endregion

    /// <summary>
    /// Static factory method for getting the Binary document given full file path. Note this reads the file on the disk hence can throw some of the IO exceptions.
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="compilationHost"></param>
    public static BinaryDocument GetBinaryDocumentForFile(string fullFilePath, IMetadataHost compilationHost) {
      Contract.Requires(fullFilePath != null);
      Contract.Requires(compilationHost != null);
      Contract.Ensures(Contract.Result<BinaryDocument>() != null);
      
      IName name = compilationHost.NameTable.GetNameFor(Path.GetFileName(fullFilePath));
      FileInfo fileInfo = new FileInfo(fullFilePath);
      uint length = 0;
      if (fileInfo.Exists) {
        //TODO: error if file too large
        length = (uint)fileInfo.Length;
      }
      return new BinaryDocument(fullFilePath, name, length);
    }
  }

  /// <summary>
  /// Class representing the Binary location.
  /// </summary>
  public sealed class BinaryLocation : IBinaryLocation {
    //^ [SpecPublic]
    IBinaryDocument binaryDocument;
    uint offset;
    //^ invariant offset >= 0 && offset <= binaryDocument.Length;

    /// <summary>
    /// Constructor for the Binary location
    /// </summary>
    /// <param name="binaryDocument"></param>
    /// <param name="offset"></param>
    public BinaryLocation(
      IBinaryDocument binaryDocument,
      uint offset
    ) {
      this.binaryDocument = binaryDocument;
      this.offset = offset;
    }

    #region IBinaryLocation Members

    IBinaryDocument IBinaryLocation.BinaryDocument {
      get 
        //^ ensures result == this.binaryDocument;
      { 
        return this.binaryDocument; 
      }
    }

    uint IBinaryLocation.Offset {
      get { 
        //^ assume ((IBinaryLocation)this).BinaryDocument == this.binaryDocument; //see above
        return this.offset; 
      }
    }

    #endregion

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.binaryDocument; }
    }

    #endregion

    /// <summary>
    /// Compares the equality of two locations.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    //^ [Confined, MustOverride]
    public override bool Equals(object/*?*/ obj) {
      BinaryLocation/*?*/ binaryLocation = obj as BinaryLocation;
      if (binaryLocation == null)
        return false;
      if (this.offset != binaryLocation.offset)
        return false;
      return this.binaryDocument.Location.Equals(binaryLocation.binaryDocument.Location);
    }

    /// <summary>
    /// Gives the hash code of the location
    /// </summary>
    /// <returns></returns>
    //^ [Confined, MustOverride]
    public override int GetHashCode() {
      return this.offset.GetHashCode();
    }

    /// <summary>
    /// Gives the string representing the location
    /// </summary>
    /// <returns></returns>
    //^ [Confined, MustOverride]
    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat(CultureInfo.InvariantCulture, "BinaryLocation({0},{1})", this.binaryDocument.Location, this.offset);
      return sb.ToString();
    }
  }

  /// <summary>
  /// Class representing the location in IL stream.
  /// </summary>
  public sealed class ILLocation : IILLocation {
    readonly IBinaryDocument binaryDocument;
    readonly IMethodDefinition methodDefinition;
    readonly uint offset;

    /// <summary>
    /// Constructor for IL location
    /// </summary>
    /// <param name="binaryDocument"></param>
    /// <param name="methodDefinition"></param>
    /// <param name="offset"></param>
    public ILLocation(
      IBinaryDocument binaryDocument,
      IMethodDefinition methodDefinition,
      uint offset
    ) {
      this.binaryDocument = binaryDocument;
      this.methodDefinition = methodDefinition;
      this.offset = offset;
    }

    #region IILLocation Members

    IMethodDefinition IILLocation.MethodDefinition {
      get { return this.methodDefinition; }
    }

    uint IILLocation.Offset {
      get { return this.offset; }
    }

    #endregion

    #region ILocation Members

    IDocument ILocation.Document {
      get { return this.binaryDocument; }
    }

    #endregion

    /// <summary>
    /// Compares the equality of two locations.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    //^ [Confined, MustOverride]
    public override bool Equals(object/*?*/ obj) {
      ILLocation/*?*/ ilLocation = obj as ILLocation;
      if (ilLocation == null)
        return false;
      if (this.offset != ilLocation.offset)
        return false;
      if (this.methodDefinition.Equals(ilLocation.methodDefinition))
        return false;
      return this.binaryDocument.Location.Equals(ilLocation.binaryDocument.Location);
    }

    /// <summary>
    /// Gives the hash code of the location
    /// </summary>
    /// <returns></returns>
    //^ [Confined, MustOverride]
    public override int GetHashCode() {
      return this.offset.GetHashCode() ^ this.methodDefinition.GetHashCode();
    }

    /// <summary>
    /// Gives the string representing the location
    /// </summary>
    /// <returns></returns>
    //^ [Confined, MustOverride]
    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat(CultureInfo.InvariantCulture, "ILLocation({0},0x{1})", this.methodDefinition.ToString(), this.offset.ToString("X8", CultureInfo.InvariantCulture));
      return sb.ToString();
    }
  }
}
