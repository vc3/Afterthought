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

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  public interface IPdbWriter {
    void CloseMethod(uint offset);
    void CloseScope(uint offset);
    void CloseTokenSourceLocationsScope();
    void DefineCustomMetadata(string name, byte[] metadata);
    void DefineLocalConstant(string name, object value, uint contantSignatureToken);
    void DefineLocalVariable(uint index, string name, bool isCompilerGenerated, uint localVariablesSignatureToken);
    void DefineSequencePoint(ILocation location, uint offset);
    void DefineTokenSourceLocation(uint token, ILocation location);
    PeDebugDirectory GetDebugDirectory();
    void OpenMethod(uint methodToken);
    void OpenScope(uint offset);
    void OpenTokenSourceLocationsScope();
    void SetEntryPoint(uint entryMethodToken);
    void UsingNamespace(string fullName);
  }

  public interface IUnmanagedPdbWriter : IPdbWriter, IDisposable {
    void SetMetadataEmitter(object metadataEmitter);
  }

  public class PeDebugDirectory {
    public uint Characteristics;
    public uint TimeDateStamp;
    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint Type;
    public uint SizeOfData;
    public uint AddressOfRawData;
    public uint PointerToRawData;
    public byte[] Data;
  }

}