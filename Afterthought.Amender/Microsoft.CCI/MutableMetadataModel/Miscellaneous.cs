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
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// A metadata custom attribute.
  /// </summary>
  public sealed class CustomAttribute : ICustomAttribute, ICopyFrom<ICustomAttribute> {

    /// <summary>
    /// Allocates a metadata custom attribute.
    /// </summary>
    public CustomAttribute() {
      this.arguments = null;
      this.constructor = Dummy.MethodReference;
      this.namedArguments = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customAttribute"></param>
    /// <param name="internFactory"></param>
    public void Copy(ICustomAttribute customAttribute, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(customAttribute.Arguments))
        this.arguments = new List<IMetadataExpression>(customAttribute.Arguments);
      else
        this.arguments = null;
      this.constructor = customAttribute.Constructor;
      if (customAttribute.NumberOfNamedArguments > 0)
        this.namedArguments = new List<IMetadataNamedArgument>(customAttribute.NamedArguments);
      else
        this.namedArguments = null;
    }

    /// <summary>
    /// Zero or more positional arguments for the attribute constructor.
    /// </summary>
    /// <value></value>
    public List<IMetadataExpression>/*?*/ Arguments {
      get { return this.arguments; }
      set { this.arguments = value; }
    }
    List<IMetadataExpression>/*?*/ arguments;

    /// <summary>
    /// A reference to the constructor that will be used to instantiate this custom attribute during execution (if the attribute is inspected via Reflection).
    /// </summary>
    /// <value></value>
    public IMethodReference Constructor {
      get { return this.constructor; }
      set { this.constructor = value; }
    }
    IMethodReference constructor;

    /// <summary>
    /// Zero or more named arguments that specify values for fields and properties of the attribute.
    /// </summary>
    /// <value></value>
    public List<IMetadataNamedArgument>/*?*/ NamedArguments {
      get { return this.namedArguments; }
      set { this.namedArguments = value; }
    }
    List<IMetadataNamedArgument>/*?*/ namedArguments;

    /// <summary>
    /// The number of named arguments.
    /// </summary>
    /// <value></value>
    public ushort NumberOfNamedArguments {
      get {
        if (this.NamedArguments == null) return 0;
        return (ushort)this.NamedArguments.Count; 
      }
    }

    /// <summary>
    /// The type of the attribute. For example System.AttributeUsageAttribute.
    /// </summary>
    /// <value></value>
    public ITypeReference Type {
      get { return this.Constructor.ContainingType; }
    }

    #region ICustomAttribute Members

    IEnumerable<IMetadataExpression> ICustomAttribute.Arguments {
      get {
        if (this.Arguments == null) return Enumerable<IMetadataExpression>.Empty;
        return this.Arguments.AsReadOnly(); 
      }
    }

    IEnumerable<IMetadataNamedArgument> ICustomAttribute.NamedArguments {
      get {
        if (this.namedArguments == null) return Enumerable<IMetadataNamedArgument>.Empty;
        return this.NamedArguments.AsReadOnly(); 
      }
    }

    #endregion
  }

  /// <summary>
  /// Represents a file referenced by an assembly.
  /// </summary>
  public sealed class FileReference : IFileReference, ICopyFrom<IFileReference> {

    /// <summary>
    /// Allocates an object that represents a file referenced by an assembly.
    /// </summary>
    public FileReference() {
      this.containingAssembly = Dummy.Assembly;
      this.fileName = Dummy.Name;
      this.hashValue = new List<byte>();
      this.hasMetadata = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IFileReference fileReference, IInternFactory internFactory) {
      this.containingAssembly = fileReference.ContainingAssembly;
      this.fileName = fileReference.FileName;
      this.hashValue = new List<byte>(fileReference.HashValue);
      this.hasMetadata = fileReference.HasMetadata;
    }

    /// <summary>
    /// The assembly that references this file.
    /// </summary>
    /// <value></value>
    public IAssembly ContainingAssembly {
      get {
        Contract.Ensures(Contract.Result<IAssembly>() != null);
        return this.containingAssembly; 
      }
      set {
        Contract.Requires(value != null);
        this.containingAssembly = value; 
      }
    }
    IAssembly containingAssembly;

    /// <summary>
    /// Name of the file.
    /// </summary>
    /// <value></value>
    public IName FileName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        return this.fileName; 
      }
      set {
        Contract.Requires(value != null);
        this.fileName = value; 
      }
    }
    IName fileName;

    /// <summary>
    /// A hash of the file contents.
    /// </summary>
    /// <value></value>
    public List<byte> HashValue {
      get {
        Contract.Ensures(Contract.Result<List<byte>>() != null);
        return this.hashValue; 
      }
      set {
        Contract.Requires(value != null);
        this.hashValue = value; 
      }
    }
    List<byte> hashValue;

    /// <summary>
    /// True if the file has metadata.
    /// </summary>
    /// <value></value>
    public bool HasMetadata {
      get { return this.hasMetadata; }
      set { this.hasMetadata = value; }
    }
    bool hasMetadata;

    #region IFileReference Members

    IEnumerable<byte> IFileReference.HashValue {
      get { return this.hashValue.AsReadOnly(); }
    }

    #endregion
  }

  /// <summary>
  /// Information about how values of managed types should be marshalled to and from unmanaged types.
  /// </summary>
  public sealed class MarshallingInformation : IMarshallingInformation, ICopyFrom<IMarshallingInformation> {

    /// <summary>
    /// Allocates an object containig information about how values of managed types should be marshalled to and from unmanaged types.
    /// </summary>
    public MarshallingInformation() {
      this.customMarshaller = Dummy.TypeReference;
      this.customMarshallerRuntimeArgument = "";
      this.elementType = (UnmanagedType)0;
      this.iidParameterIndex = 0;
      this.numberOfElements = 0;
      this.paramIndex = 0;
      this.safeArrayElementSubType = (VarEnum)0;
      this.safeArrayElementUserDefinedSubType = Dummy.TypeReference;
      this.unmanagedType = (UnmanagedType)0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="marshallingInformation"></param>
    /// <param name="internFactory"></param>
    public void Copy(IMarshallingInformation marshallingInformation, IInternFactory internFactory) {
      if (marshallingInformation.UnmanagedType == UnmanagedType.CustomMarshaler)
        this.customMarshaller = marshallingInformation.CustomMarshaller;
      else
        this.customMarshaller = Dummy.TypeReference;
      if (marshallingInformation.UnmanagedType == UnmanagedType.CustomMarshaler)
        this.customMarshallerRuntimeArgument = marshallingInformation.CustomMarshallerRuntimeArgument;
      else
        this.customMarshallerRuntimeArgument = "";
      if (marshallingInformation.UnmanagedType == UnmanagedType.ByValArray || marshallingInformation.UnmanagedType == UnmanagedType.LPArray)
        this.elementType = marshallingInformation.ElementType;
      else
        this.elementType = (UnmanagedType)0;
      if (marshallingInformation.UnmanagedType == UnmanagedType.Interface)
        this.iidParameterIndex = marshallingInformation.IidParameterIndex;
      else
        this.iidParameterIndex = 0;
      if (marshallingInformation.UnmanagedType == UnmanagedType.ByValArray || marshallingInformation.UnmanagedType == UnmanagedType.ByValTStr || 
        marshallingInformation.UnmanagedType == UnmanagedType.LPArray)
        this.numberOfElements = marshallingInformation.NumberOfElements;
      else
        this.numberOfElements = 0;
      if (marshallingInformation.UnmanagedType == UnmanagedType.LPArray)
        this.paramIndex = marshallingInformation.ParamIndex;
      else
        this.paramIndex = 0;
      if (marshallingInformation.UnmanagedType == UnmanagedType.SafeArray)
        this.safeArrayElementSubType = marshallingInformation.SafeArrayElementSubtype;
      else
        this.safeArrayElementSubType = (VarEnum)0;
      if (marshallingInformation.UnmanagedType == UnmanagedType.SafeArray && 
      (marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_DISPATCH || marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_UNKNOWN || 
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_RECORD))
        this.safeArrayElementUserDefinedSubType = marshallingInformation.SafeArrayElementUserDefinedSubtype;
      else
        this.safeArrayElementUserDefinedSubType = Dummy.TypeReference;
      this.unmanagedType = marshallingInformation.UnmanagedType;
    }

    /// <summary>
    /// A reference to the type implementing the custom marshaller.
    /// </summary>
    /// <value></value>
    public ITypeReference CustomMarshaller {
      get { return this.customMarshaller; }
      set { this.customMarshaller = value; }
    }
    ITypeReference customMarshaller;

    /// <summary>
    /// An argument string (cookie) passed to the custom marshaller at run time.
    /// </summary>
    /// <value></value>
    public string CustomMarshallerRuntimeArgument {
      get { return this.customMarshallerRuntimeArgument; }
      set { this.customMarshallerRuntimeArgument = value; }
    }
    string customMarshallerRuntimeArgument;

    /// <summary>
    /// The unmanged element type of the unmanaged array.
    /// </summary>
    /// <value></value>
    public UnmanagedType ElementType {
      get { return this.elementType; }
      set { this.elementType = value; }
    }
    UnmanagedType elementType;

    /// <summary>
    /// Specifies the index of the parameter that contains the value of the Inteface Identifier (IID) of the marshalled object.
    /// </summary>
    /// <value></value>
    public uint IidParameterIndex {
      get { return this.iidParameterIndex; }
      set { this.iidParameterIndex = value; }
    }
    uint iidParameterIndex;

    /// <summary>
    /// The unmanaged type to which the managed type will be marshalled. This can be be UnmanagedType.CustomMarshaler, in which case the unmanaged type
    /// is decided at runtime.
    /// </summary>
    /// <value></value>
    public UnmanagedType UnmanagedType {
      get { return this.unmanagedType; }
      set { this.unmanagedType = value; }
    }
    UnmanagedType unmanagedType;

    /// <summary>
    /// The number of elements in the fixed size portion of the unmanaged array.
    /// </summary>
    /// <value></value>
    public uint NumberOfElements {
      get { return this.numberOfElements; }
      set { this.numberOfElements = value; }
    }
    uint numberOfElements;

    /// <summary>
    /// The zero based index of the parameter in the unmanaged method that contains the number of elements in the variable portion of unmanaged array.
    /// If the index is null, the variable portion is of size zero, or the caller conveys the size of the variable portion of the array to the unmanaged method in some other way.
    /// </summary>
    /// <value></value>
    public uint? ParamIndex {
      get { return this.paramIndex; }
      set { this.paramIndex = value; }
    }
    uint? paramIndex;

    /// <summary>
    /// The type to which the variant values of all elements of the safe array must belong. See also SafeArrayElementUserDefinedSubtype.
    /// (The element type of a safe array is VARIANT. The "sub type" specifies the value of all of the tag fields (vt) of the element values. )
    /// </summary>
    /// <value></value>
    public VarEnum SafeArrayElementSubtype {
      get { return this.safeArrayElementSubType; }
      set { this.safeArrayElementSubType = value; }
    }
    VarEnum safeArrayElementSubType;

    /// <summary>
    /// A reference to the user defined type to which the variant values of all elements of the safe array must belong.
    /// (The element type of a safe array is VARIANT. The tag fields will all be either VT_DISPATCH or VT_UNKNOWN or VT_RECORD.
    /// The "user defined sub type" specifies the type of value the ppdispVal/ppunkVal/pvRecord fields of the element values may point to.)
    /// </summary>
    /// <value></value>
    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return this.safeArrayElementUserDefinedSubType; }
      set { this.safeArrayElementUserDefinedSubType = value; }
    }
    ITypeReference safeArrayElementUserDefinedSubType;

  }

  /// <summary>
  /// A single CLR IL operation.
  /// </summary>
  public sealed class Operation : IOperation, ICopyFrom<IOperation> {

    /// <summary>
    /// Allocates a single CLR IL operation.
    /// </summary>
    public Operation() {
      this.location = Dummy.Location;
      this.offset = 0;
      this.operationCode = (OperationCode)0;
      this.value = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="internFactory"></param>
    public void Copy(IOperation operation, IInternFactory internFactory) {
      this.location = operation.Location;
      this.offset = operation.Offset;
      this.operationCode = operation.OperationCode;
      this.value = operation.Value;
    }

    /// <summary>
    /// The location that corresponds to this instruction.
    /// </summary>
    /// <value></value>
    public ILocation Location {
      get { return this.location; }
      set { this.location = value; }
    }
    ILocation location;

    /// <summary>
    /// The offset from the start of the operation stream of a method
    /// </summary>
    /// <value></value>
    public uint Offset {
      get { return this.offset; }
      set { this.offset = value; }
    }
    uint offset;

    /// <summary>
    /// The actual value of the operation code
    /// </summary>
    /// <value></value>
    public OperationCode OperationCode {
      get { return this.operationCode; }
      set { this.operationCode = value; }
    }
    OperationCode operationCode;

    /// <summary>
    /// Immediate data such as a string, the address of a branch target, or a metadata reference, such as a Field
    /// </summary>
    /// <value></value>
    public object Value {
      get { return this.value; }
      set { this.value = value; }
    }
    object value;

  }

  /// <summary>
  /// Exception information of the method body expressed in terms of offsets in CLR IL.
  /// </summary>
  public sealed class OperationExceptionInformation : IOperationExceptionInformation, ICopyFrom<IOperationExceptionInformation> {

    /// <summary>
    /// Allocates an object that provides exception information of the method body expressed in terms of offsets in CLR IL.
    /// </summary>
    public OperationExceptionInformation() {
      this.exceptionType = Dummy.TypeReference;
      this.filterDecisionStartOffset = 0;
      this.handlerEndOffset = 0;
      this.handlerKind = (HandlerKind)0;
      this.handlerStartOffset = 0;
      this.tryEndOffset = 0;
      this.tryStartOffset = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operationExceptionInformation"></param>
    /// <param name="internFactory"></param>
    public void Copy(IOperationExceptionInformation operationExceptionInformation, IInternFactory internFactory) {
      this.exceptionType = operationExceptionInformation.ExceptionType;
      this.filterDecisionStartOffset = operationExceptionInformation.FilterDecisionStartOffset;
      this.handlerEndOffset = operationExceptionInformation.HandlerEndOffset;
      this.handlerKind = operationExceptionInformation.HandlerKind;
      this.handlerStartOffset = operationExceptionInformation.HandlerStartOffset;
      this.tryEndOffset = operationExceptionInformation.TryEndOffset;
      this.tryStartOffset = operationExceptionInformation.TryStartOffset;
    }

    /// <summary>
    /// If HandlerKind == HandlerKind.Catch, this is the type of expection to catch. If HandlerKind == HandlerKind.Filter, this is System.Object.
    /// Otherwise this is a Dummy.TypeReference.
    /// </summary>
    /// <value></value>
    public ITypeReference ExceptionType {
      get { return this.exceptionType; }
      set { this.exceptionType = value; }
    }
    ITypeReference exceptionType;

    /// <summary>
    /// Label instruction corresponding to the start of filter decision block
    /// </summary>
    /// <value></value>
    public uint FilterDecisionStartOffset {
      get { return this.filterDecisionStartOffset; }
      set { this.filterDecisionStartOffset = value; }
    }
    uint filterDecisionStartOffset;

    /// <summary>
    /// Label instruction corresponding to the end of handler block
    /// </summary>
    /// <value></value>
    public uint HandlerEndOffset {
      get { return this.handlerEndOffset; }
      set { this.handlerEndOffset = value; }
    }
    uint handlerEndOffset;

    /// <summary>
    /// Handler kind for this SEH info
    /// </summary>
    /// <value></value>
    public HandlerKind HandlerKind {
      get { return this.handlerKind; }
      set { this.handlerKind = value; }
    }
    HandlerKind handlerKind;

    /// <summary>
    /// Label instruction corresponding to the start of handler block
    /// </summary>
    /// <value></value>
    public uint HandlerStartOffset {
      get { return this.handlerStartOffset; }
      set { this.handlerStartOffset = value; }
    }
    uint handlerStartOffset;

    /// <summary>
    /// Label instruction corresponding to the end of try block
    /// </summary>
    /// <value></value>
    public uint TryEndOffset {
      get { return this.tryEndOffset; }
      set { this.tryEndOffset = value; }
    }
    uint tryEndOffset;

    /// <summary>
    /// Label instruction corresponding to the start of try block
    /// </summary>
    /// <value></value>
    public uint TryStartOffset {
      get { return this.tryStartOffset; }
      set { this.tryStartOffset = value; }
    }
    uint tryStartOffset;

  }

  /// <summary>
  /// Information that describes how a method from the underlying Platform is to be invoked.
  /// </summary>
  public sealed class PlatformInvokeInformation : IPlatformInvokeInformation, ICopyFrom<IPlatformInvokeInformation> {

    /// <summary>
    /// Allocates an object that provides information that describes how a method from the underlying Platform is to be invoked.
    /// </summary>
    public PlatformInvokeInformation() {
      this.importModule = Dummy.ModuleReference;
      this.importName = Dummy.Name;
      this.noMangle = false;
      this.pinvokeCallingConvention = (PInvokeCallingConvention)0;
      this.stringFormat = StringFormatKind.Unspecified;
      this.supportsLastError = false;
      this.useBestFit = null;
      this.throwExceptionForUnmappableChar = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformInvokeInformation"></param>
    /// <param name="internFactory"></param>
    public void Copy(IPlatformInvokeInformation platformInvokeInformation, IInternFactory internFactory) {
      this.importModule = platformInvokeInformation.ImportModule;
      this.importName = platformInvokeInformation.ImportName;
      this.noMangle = platformInvokeInformation.NoMangle;
      this.pinvokeCallingConvention = platformInvokeInformation.PInvokeCallingConvention;
      this.stringFormat = platformInvokeInformation.StringFormat;
      this.supportsLastError = platformInvokeInformation.SupportsLastError;
      this.useBestFit = platformInvokeInformation.UseBestFit;
      this.throwExceptionForUnmappableChar = platformInvokeInformation.ThrowExceptionForUnmappableChar;
    }

    /// <summary>
    /// Module providing the method/field.
    /// </summary>
    /// <value></value>
    public IModuleReference ImportModule {
      get { return this.importModule; }
      set { this.importModule = value; }
    }
    IModuleReference importModule;

    /// <summary>
    /// Name of the method/field providing the implementation.
    /// </summary>
    /// <value></value>
    public IName ImportName {
      get { return this.importName; }
      set { this.importName = value; }
    }
    IName importName;

    /// <summary>
    /// If the PInvoke should use the name specified as is.
    /// </summary>
    /// <value></value>
    public bool NoMangle {
      get { return this.noMangle; }
      set { this.noMangle = value; }
    }
    bool noMangle;

    /// <summary>
    /// The calling convention of the PInvoke call.
    /// </summary>
    /// <value></value>
    public PInvokeCallingConvention PInvokeCallingConvention {
      get { return this.pinvokeCallingConvention; }
      set { this.pinvokeCallingConvention = value; }
    }
    PInvokeCallingConvention pinvokeCallingConvention;

    /// <summary>
    /// Marshalling of the Strings for this method.
    /// </summary>
    /// <value></value>
    public StringFormatKind StringFormat {
      get { return this.stringFormat; }
      set { this.stringFormat = value; }
    }
    StringFormatKind stringFormat;

    /// <summary>
    /// If the target function supports getting last error.
    /// </summary>
    /// <value></value>
    public bool SupportsLastError {
      get { return this.supportsLastError; }
      set { this.supportsLastError = value; }
    }
    bool supportsLastError;

    /// <summary>
    /// Enables or disables best-fit mapping behavior when converting Unicode characters to ANSI characters.
    /// </summary>
    /// <value></value>
    public bool? UseBestFit {
      get { return this.useBestFit; }
      set { this.useBestFit = value; }
    }
    bool? useBestFit;

    /// <summary>
    /// Enables or disables the throwing of an exception on an unmappable Unicode character that is converted to an ANSI "?" character.
    /// </summary>
    /// <value></value>
    public bool? ThrowExceptionForUnmappableChar {
      get { return this.throwExceptionForUnmappableChar; }
      set { this.throwExceptionForUnmappableChar = value; }
    }
    bool? throwExceptionForUnmappableChar;

  }

  /// <summary>
  /// A section of the PE file that does not contain any objects that can be referenced in another way
  /// using this object model. The data of such a section is not decompiled by metadata reader and 
  /// is written out unchanged by metadata writers. Presumably it is meaningful in some way and is 
  /// thus exposed for use by other tools.
  /// </summary>
  public sealed class PESection : IPESection {


    /// <summary>
    /// A section of the PE file that does not contain any objects that can be referenced in another way
    /// using this object model. The data of such a section is not decompiled by metadata reader and 
    /// is written out unchanged by metadata writers. Presumably it is meaningful in some way and is 
    /// thus exposed for use by other tools.
    /// </summary>
    public PESection() {
      this.sectionName = Dummy.Name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="peSection"></param>
    /// <param name="internFactory"></param>
    public void Copy(IPESection peSection, IInternFactory internFactory) {
      this.sectionName = peSection.SectionName;
      this.characteristics = peSection.Characteristics;
      this.virtualAddress = peSection.VirtualAddress;
      this.virtualSize = peSection.VirtualSize;
      this.sizeOfRawData = peSection.SizeOfRawData;
      if (peSection.SizeOfRawData > 0)
        this.Rawdata = new List<byte>(peSection.Rawdata);
    }

    /// <summary>
    /// The name of the section. Should be exactly eight characters long.
    /// </summary>
    public IName SectionName {
      get { return this.sectionName; }
      set { this.sectionName = value; }
    }
    IName sectionName;

    /// <summary>
    /// A set of bits that describes the purpose of a section and how it behaves when loaded.
    /// </summary>
    public PESectionCharacteristics Characteristics {
      get { return this.characteristics; }
      set { this.characteristics = value; }
    }
    PESectionCharacteristics characteristics;

    /// <summary>
    /// For executable images this is the address of the first byte of the section, when loaded into memory, relative to the image base.
    /// </summary>
    public int VirtualAddress {
      get { return this.virtualAddress; }
      set { this.virtualAddress = value; }
    }
    int virtualAddress;

    /// <summary>
    /// Total size of the section in bytes. If this value is greater than SizeOfRawData, the section is zero-padded.
    /// </summary>
    public int VirtualSize {
      get { return this.virtualSize; }
      set { this.virtualSize = value; }
    }
    int virtualSize;

    /// <summary>
    /// Size of the initialized data on disk in bytes, shall be a multiple of FileAlignment from the PE header.
    /// If this is less than VirtualSize the remainder of the section is zero filled.
    /// Because this field is rounded while the VirtualSize field is not, it is possible for this to be greater than VirtualSize.
    /// When a section contains only uninitialized data, this field should be 0.
    /// </summary>
    public int SizeOfRawData {
      get { return this.sizeOfRawData; }
      set { this.sizeOfRawData = value; }
    }
    int sizeOfRawData;

    /// <summary>
    /// The data, if any, with which the section will be initialized when loaded. May be null.
    /// </summary>
    public List<byte>/*?*/ Rawdata {
      get { return this.rawdata; }
      set { this.rawdata = value; }
    }
    List<byte>/*?*/ rawdata;

    #region IPESection Members

    IEnumerable<byte> IPESection.Rawdata {
      get {
        if (this.Rawdata == null) return Enumerable<byte>.Empty;
        return this.Rawdata.AsReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// A named data resource that is stored as part of CLR metadata.
  /// </summary>
  public sealed class Resource : IResource, ICopyFrom<IResource> {

    /// <summary>
    /// Allocates a named data resource that is stored as part of CLR metadata.
    /// </summary>
    public Resource() {
      this.attributes = null;
      this.data = new List<byte>();
      this.definingAssembly = Dummy.Assembly;
      this.externalFile = Dummy.FileReference;
      this.isInExternalFile = false;
      this.isPublic = false;
      this.name = Dummy.Name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="internFactory"></param>
    public void Copy(IResource resource, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(resource.Attributes))
        this.attributes = new List<ICustomAttribute>(resource.Attributes);
      else
        this.attributes = null;
      this.data = new List<byte>(resource.Data);
      this.definingAssembly = resource.DefiningAssembly;
      if (resource.IsInExternalFile)
        this.externalFile = resource.ExternalFile;
      else
        this.externalFile = Dummy.FileReference;
      this.isInExternalFile = resource.IsInExternalFile;
      this.isPublic = resource.IsPublic;
      this.name = resource.Name;
    }

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this resource.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ Attributes {
      get { return this.attributes; }
      set { this.attributes = value; }
    }
    List<ICustomAttribute>/*?*/ attributes;

    /// <summary>
    /// The resource data.
    /// </summary>
    /// <value></value>
    public List<byte> Data {
      get { return this.data; }
      set { this.data = value; }
    }
    List<byte> data;

    /// <summary>
    /// A symbolic reference to the IAssembly that defines the resource.
    /// </summary>
    /// <value></value>
    public IAssemblyReference DefiningAssembly {
      get { return this.definingAssembly; }
      set { this.definingAssembly = value; }
    }
    IAssemblyReference definingAssembly;

    /// <summary>
    /// The external file that contains the resource.
    /// </summary>
    /// <value></value>
    public IFileReference ExternalFile {
      get { return this.externalFile; }
      set { this.externalFile = value; }
    }
    IFileReference externalFile;

    /// <summary>
    /// The resource is in external file
    /// </summary>
    /// <value></value>
    public bool IsInExternalFile {
      get { return this.isInExternalFile; }
      set { this.isInExternalFile = value; }
    }
    bool isInExternalFile;

    /// <summary>
    /// Specifies whether other code from other assemblies may access this resource.
    /// </summary>
    /// <value></value>
    public bool IsPublic {
      get { return this.isPublic; }
      set { this.isPublic = value; }
    }
    bool isPublic;

    /// <summary>
    /// The name of the resource.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set { this.name = value; }
    }
    IName name;

    #region IResource Members

    IEnumerable<byte> IResource.Data {
      get { return this.data.AsReadOnly(); }
    }

    #endregion

    #region IResourceReference Members

    IEnumerable<ICustomAttribute> IResourceReference.Attributes {
      get {
        if (this.Attributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.Attributes.AsReadOnly(); 
      }
    }

    IResource IResourceReference.Resource {
      get { return this; }
    }

    #endregion
  }

  /// <summary>
  /// A reference to an IResource instance.
  /// </summary>
  public sealed class ResourceReference : IResourceReference, ICopyFrom<IResourceReference> {

    /// <summary>
    /// Allocates a reference to an IResource instance.
    /// </summary>
    public ResourceReference() {
      this.attributes = null;
      this.definingAssembly = Dummy.Assembly;
      this.isPublic = false;
      this.name = Dummy.Name;
      this.resource = Dummy.Resource;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceReference"></param>
    /// <param name="internFactory"></param>
    public void Copy(IResourceReference resourceReference, IInternFactory internFactory) {
      if (IteratorHelper.EnumerableIsNotEmpty(resourceReference.Attributes))
        this.attributes = new List<ICustomAttribute>(resourceReference.Attributes);
      else
        this.attributes = null;
      this.definingAssembly = resourceReference.DefiningAssembly;
      this.isPublic = resourceReference.IsPublic;
      this.name = resourceReference.Name;
      this.resource = resourceReference.Resource;
    }

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this resource.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ Attributes {
      get { return this.attributes; }
      set { this.attributes = value; }
    }
    List<ICustomAttribute>/*?*/ attributes;

    /// <summary>
    /// A symbolic reference to the IAssembly that defines the resource.
    /// </summary>
    /// <value></value>
    public IAssemblyReference DefiningAssembly {
      get { return this.definingAssembly; }
      set { this.definingAssembly = value; }
    }
    IAssemblyReference definingAssembly;

    /// <summary>
    /// Specifies whether other code from other assemblies may access this resource.
    /// </summary>
    /// <value></value>
    public bool IsPublic {
      get { return this.isPublic; }
      set { this.isPublic = value; }
    }
    bool isPublic;

    /// <summary>
    /// The name of the resource.
    /// </summary>
    /// <value></value>
    public IName Name {
      get { return this.name; }
      set { this.name = value; }
    }
    IName name;

    /// <summary>
    /// The referenced resource.
    /// </summary>
    /// <value></value>
    public IResource Resource {
      get { return this.resource; }
      set { this.resource = value; }
    }
    IResource resource;

    #region IResourceReference Members

    IEnumerable<ICustomAttribute> IResourceReference.Attributes {
      get {
        if (this.Attributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.Attributes.AsReadOnly(); 
      }
    }

    #endregion
  }

  /// <summary>
  /// Represents a block of data stored at a given offset within a specified section of the PE file.
  /// </summary>
  public sealed class SectionBlock : ISectionBlock, ICopyFrom<ISectionBlock> {

    /// <summary>
    /// Allocates an object that represents a block of 
    /// </summary>
    public SectionBlock() {
      this.data = new List<byte>();
    }

    /// <summary>
    /// </summary>
    /// <param name="sectionBlock"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISectionBlock sectionBlock, IInternFactory internFactory) {
      this.data = new List<byte>(sectionBlock.Data);
      this.offset = sectionBlock.Offset;
      this.peSectionKind = sectionBlock.PESectionKind;
      this.size = sectionBlock.Size;
    }

    /// <summary>
    /// Byte information stored in the block.
    /// </summary>
    public List<byte> Data {
      get { return this.data; }
      set { this.data = value; }
    }
    List<byte> data;

    /// <summary>
    /// Offset into section where the block resides.
    /// </summary>
    public uint Offset {
      get { return this.offset; }
      set { this.offset = value; }
    }
    uint offset;

    /// <summary>
    /// Section where the block resides.
    /// </summary>
    public PESectionKind PESectionKind {
      get { return this.peSectionKind; }
      set { this.peSectionKind = value; }
    }
    PESectionKind peSectionKind;

    /// <summary>
    /// Size of the block.
    /// </summary>
    public uint Size {
      get { return this.size; }
      set { this.size = value; }
    }
    uint size;


    #region ISectionBlock Members

    IEnumerable<byte> ISectionBlock.Data {
      get { return this.Data.AsReadOnly(); }
    }

    #endregion
  }

  /// <summary>
  /// A declarative specification of a security action applied to a set of permissions. Used by the CLR loader to enforce security restrictions.
  /// </summary>
  public sealed class SecurityAttribute : ISecurityAttribute, ICopyFrom<ISecurityAttribute> {

    /// <summary>
    /// Allocates a declarative specification of a security action applied to a set of permissions. Used by the CLR loader to enforce security restrictions.
    /// </summary>
    public SecurityAttribute() {
      this.action = (SecurityAction)0;
      this.attributes = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="securityAttribute"></param>
    /// <param name="internFactory"></param>
    public void Copy(ISecurityAttribute securityAttribute, IInternFactory internFactory) {
      this.action = securityAttribute.Action;
      if (IteratorHelper.EnumerableIsNotEmpty(securityAttribute.Attributes))
        this.attributes = new List<ICustomAttribute>(securityAttribute.Attributes);
      else
        this.attributes = null;
    }

    /// <summary>
    /// Specifies the security action that can be performed using declarative security. For example the action could be Deny.
    /// </summary>
    /// <value></value>
    public SecurityAction Action {
      get { return this.action; }
      set { this.action = value; }
    }
    SecurityAction action;

    /// <summary>
    /// Custom attributes that collectively define the permission set to which the action is applied. Each attribute represents a serialized permission
    /// or permission set. The union of the sets, together with the individual permissions, define the set to which the action applies.
    /// </summary>
    /// <value></value>
    public List<ICustomAttribute>/*?*/ Attributes {
      get { return this.attributes; }
      set { this.attributes = value; }
    }
    List<ICustomAttribute>/*?*/ attributes;

    #region ISecurityAttribute Members


    IEnumerable<ICustomAttribute> ISecurityAttribute.Attributes {
      get {
        if (this.Attributes == null) return Enumerable<ICustomAttribute>.Empty;
        return this.Attributes.AsReadOnly(); 
      }
    }

    #endregion
  }

  /// <summary>
  /// A resource file formatted according to Win32 API conventions and typically obtained from a Portable Executable (PE) file.
  /// </summary>
  public sealed class Win32Resource : IWin32Resource, ICopyFrom<IWin32Resource> {

    /// <summary>
    /// Allocates a resource file formatted according to Win32 API conventions and typically obtained from a Portable Executable (PE) file.
    /// </summary>
    public Win32Resource() {
      this.codePage = 0;
      this.data = new List<byte>();
      this.id = 0;
      this.languageId = 0;
      this.name = "";
      this.typeId = 0;
      this.typeName = "";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="win32Resource"></param>
    /// <param name="internFactory"></param>
    public void Copy(IWin32Resource win32Resource, IInternFactory internFactory) {
      this.codePage = win32Resource.CodePage;
      this.data = new List<byte>(win32Resource.Data);
      this.id = win32Resource.Id;
      this.languageId = win32Resource.LanguageId;
      this.name = win32Resource.Name;
      this.typeId = win32Resource.TypeId;
      this.typeName = win32Resource.TypeName;
    }

    /// <summary>
    /// The code page for which this resource is appropriate.
    /// </summary>
    /// <value></value>
    public uint CodePage {
      get { return this.codePage; }
      set { this.codePage = value; }
    }
    uint codePage;

    /// <summary>
    /// The data of the resource.
    /// </summary>
    /// <value></value>
    public List<byte> Data {
      get { return this.data; }
      set { this.data = value; }
    }
    List<byte> data;

    /// <summary>
    /// An integer tag that identifies this resource. If the value is less than 0, this.Name should be used instead.
    /// </summary>
    /// <value></value>
    public int Id {
      get { return this.id; }
      set { this.id = value; }
    }
    int id;

    /// <summary>
    /// The language for which this resource is appropriate.
    /// </summary>
    /// <value></value>
    public uint LanguageId {
      get { return this.languageId; }
      set { this.languageId = value; }
    }
    uint languageId;

    /// <summary>
    /// The name of the resource. Only valid if this.Id &lt; 0.
    /// </summary>
    /// <value></value>
    public string Name {
      get { return this.name; }
      set { this.name = value; }
    }
    string name;

    /// <summary>
    /// An integer tag that identifies what type of resource this is. If the value is less than 0, this.TypeName should be used instead.
    /// </summary>
    /// <value></value>
    public int TypeId {
      get { return this.typeId; }
      set { this.typeId = value; }
    }
    int typeId;

    /// <summary>
    /// A string that identifies what type of resource this is. Only valid if this.TypeId &lt; 0.
    /// </summary>
    /// <value></value>
    public string TypeName {
      get { return this.typeName; }
      set { this.typeName = value; }
    }
    string typeName;

    #region IWin32Resource Members


    IEnumerable<byte> IWin32Resource.Data {
      get { return this.data.AsReadOnly(); }
    }

    #endregion
  }
}