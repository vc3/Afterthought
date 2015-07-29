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
using System.Diagnostics;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.MetadataReader.PEFileFlags {

  internal enum Characteristics : ushort {
    RelocsStripped = 0x0001,         // Relocation info stripped from file.
    ExecutableImage = 0x0002,        // File is executable  (i.e. no unresolved external references).
    LineNumsStripped = 0x0004,       // Line numbers stripped from file.
    LocalSymsStripped = 0x0008,      // Local symbols stripped from file.
    AggressiveWsTrim = 0x0010,       // Agressively trim working set
    LargeAddressAware = 0x0020,      // App can handle >2gb addresses
    BytesReversedLo = 0x0080,        // Bytes of machine word are reversed.
    Bit32Machine = 0x0100,           // 32 bit word machine.
    DebugStripped = 0x0200,          // Debugging info stripped from file in .DBG file
    RemovableRunFromSwap = 0x0400,   // If Image is on removable media, copy and run from the swap file.
    NetRunFromSwap = 0x0800,         // If Image is on Net, copy and run from the swap file.
    System = 0x1000,                 // System File.
    Dll = 0x2000,                    // File is a DLL.
    UpSystemOnly = 0x4000,           // File should only be run on a UP machine
    BytesReversedHi = 0x8000,        // Bytes of machine word are reversed.
  }

  internal enum PEMagic : ushort {
    PEMagic32 = 0x010B,
    PEMagic64 = 0x020B,
  }

  internal enum Directories : ushort {
    Export,
    Import,
    Resource,
    Exception,
    Certificate,
    BaseRelocation,
    Debug,
    Copyright,
    GlobalPointer,
    ThreadLocalStorage,
    LoadConfig,
    BoundImport,
    ImportAddress,
    DelayImport,
    COR20Header,
    Reserved,
    Cor20HeaderMetaData,
    Cor20HeaderResources,
    Cor20HeaderStrongNameSignature,
    Cor20HeaderCodeManagerTable,
    Cor20HeaderVtableFixups,
    Cor20HeaderExportAddressTableJumps,
    Cor20HeaderManagedNativeHeader,
  }

  internal enum Subsystem : ushort {
    Unknown = 0,                // Unknown subsystem.
    Native = 1,                 // Image doesn't require a subsystem.
    WindowsGUI = 2,             // Image runs in the Windows GUI subsystem.
    WindowsCUI = 3,             // Image runs in the Windows character subsystem.
    OS2CUI = 5,                 // image runs in the OS/2 character subsystem.
    POSIXCUI = 7,               // image runs in the Posix character subsystem.
    NativeWindows = 8,          // image is a native Win9x driver.
    WindowsCEGUI = 9,           // Image runs in the Windows CE subsystem.
    EFIApplication = 10,
    EFIBootServiceDriver = 11,
    EFIRuntimeDriver = 12,
    EFIROM = 13,
    XBOX = 14,
  }

  internal enum DllCharacteristics : ushort {
    ProcessInit = 0x0001,   // Reserved.
    ProcessTerm = 0x0002,   // Reserved.
    ThreadInit = 0x0004,    // Reserved.
    ThreadTerm = 0x0008,    // Reserved.
    DynamicBase = 0x0040,   //
    NxCompatible = 0x0100,  //
    NoIsolation = 0x0200,   // Image understands isolation and doesn't want it
    NoSEH = 0x0400,         // Image does not use SEH.  No SE handler may reside in this image
    NoBind = 0x0800,        // Do not bind this image.
    AppContainer = 0x1000,  // The image must run inside an AppContainer
    WDM_Driver = 0x2000,    // Driver uses WDM model
    //                      0x4000     // Reserved.
    TerminalServerAware = 0x8000,
  }

  internal enum SectionCharacteristics : uint {
    TypeReg = 0x00000000,               // Reserved.
    TypeDSect = 0x00000001,             // Reserved.
    TypeNoLoad = 0x00000002,            // Reserved.
    TypeGroup = 0x00000004,             // Reserved.
    TypeNoPad = 0x00000008,             // Reserved.
    TypeCopy = 0x00000010,              // Reserved.

    CNTCode = 0x00000020,               // Section contains code.
    CNTInitializedData = 0x00000040,    // Section contains initialized data.
    CNTUninitializedData = 0x00000080,  // Section contains uninitialized data.

    LNKOther = 0x00000100,            // Reserved.
    LNKInfo = 0x00000200,             // Section contains comments or some other type of information.
    TypeOver = 0x00000400,            // Reserved.
    LNKRemove = 0x00000800,           // Section contents will not become part of image.
    LNKCOMDAT = 0x00001000,           // Section contents comdat.
    //                                0x00002000  // Reserved.
    MemProtected = 0x00004000,
    No_Defer_Spec_Exc = 0x00004000,   // Reset speculative exceptions handling bits in the TLB entries for this section.
    GPRel = 0x00008000,               // Section content can be accessed relative to GP
    MemFardata = 0x00008000,
    MemSysheap = 0x00010000,
    MemPurgeable = 0x00020000,
    Mem16Bit = 0x00020000,
    MemLocked = 0x00040000,
    MemPreload = 0x00080000,

    Align1Bytes = 0x00100000,     //
    Align2Bytes = 0x00200000,     //
    Align4Bytes = 0x00300000,     //
    Align8Bytes = 0x00400000,     //
    Align16Bytes = 0x00500000,    // Default alignment if no others are specified.
    Align32Bytes = 0x00600000,    //
    Align64Bytes = 0x00700000,    //
    Align128Bytes = 0x00800000,   //
    Align256Bytes = 0x00900000,   //
    Align512Bytes = 0x00A00000,   //
    Align1024Bytes = 0x00B00000,  //
    Align2048Bytes = 0x00C00000,  //
    Align4096Bytes = 0x00D00000,  //
    Align8192Bytes = 0x00E00000,  //
    // Unused                     0x00F00000
    AlignMask = 0x00F00000,

    LNKNRelocOvfl = 0x01000000,   // Section contains extended relocations.
    MemDiscardable = 0x02000000,  // Section can be discarded.
    MemNotCached = 0x04000000,    // Section is not cachable.
    MemNotPaged = 0x08000000,     // Section is not pageable.
    MemShared = 0x10000000,       // Section is shareable.
    MemExecute = 0x20000000,      // Section is executable.
    MemRead = 0x40000000,         // Section is readable.
    MemWrite = 0x80000000,        // Section is writeable.
  }

  internal enum COR20Flags : uint {
    ILOnly = 0x00000001,
    Bit32Required = 0x00000002,
    ILLibrary = 0x00000004,
    StrongNameSigned = 0x00000008,
    Prefers32bits = 0x00000010,
    TrackDebugData = 0x00010000,
  }

  internal enum MetadataStreamKind {
    Illegal,
    Compressed,
    UnCompressed,
  }

  internal enum TableIndices : byte {
    Module = 0x00,
    TypeRef = 0x01,
    TypeDef = 0x02,
    FieldPtr = 0x03,
    Field = 0x04,
    MethodPtr = 0x05,
    Method = 0x06,
    ParamPtr = 0x07,
    Param = 0x08,
    InterfaceImpl = 0x09,
    MemberRef = 0x0A,
    Constant = 0x0B,
    CustomAttribute = 0x0C,
    FieldMarshal = 0x0D,
    DeclSecurity = 0x0E,
    ClassLayout = 0x0F,
    FieldLayout = 0x10,
    StandAloneSig = 0x11,
    EventMap = 0x12,
    EventPtr = 0x13,
    Event = 0x14,
    PropertyMap = 0x15,
    PropertyPtr = 0x16,
    Property = 0x17,
    MethodSemantics = 0x18,
    MethodImpl = 0x19,
    ModuleRef = 0x1A,
    TypeSpec = 0x1B,
    ImplMap = 0x1C,
    FieldRva = 0x1D,
    EnCLog = 0x1E,
    EnCMap = 0x1F,
    Assembly = 0x20,
    AssemblyProcessor = 0x21,
    AssemblyOS = 0x22,
    AssemblyRef = 0x23,
    AssemblyRefProcessor = 0x24,
    AssemblyRefOS = 0x25,
    File = 0x26,
    ExportedType = 0x27,
    ManifestResource = 0x28,
    NestedClass = 0x29,
    GenericParam = 0x2A,
    MethodSpec = 0x2B,
    GenericParamConstraint = 0x2C,
    Count,
  }

  internal enum TableMask : ulong {
    Module = 0x0000000000000001UL << 0x00,
    TypeRef = 0x0000000000000001UL << 0x01,
    TypeDef = 0x0000000000000001UL << 0x02,
    FieldPtr = 0x0000000000000001UL << 0x03,
    Field = 0x0000000000000001UL << 0x04,
    MethodPtr = 0x0000000000000001UL << 0x05,
    Method = 0x0000000000000001UL << 0x06,
    ParamPtr = 0x0000000000000001UL << 0x07,
    Param = 0x0000000000000001UL << 0x08,
    InterfaceImpl = 0x0000000000000001UL << 0x09,
    MemberRef = 0x0000000000000001UL << 0x0A,
    Constant = 0x0000000000000001UL << 0x0B,
    CustomAttribute = 0x0000000000000001UL << 0x0C,
    FieldMarshal = 0x0000000000000001UL << 0x0D,
    DeclSecurity = 0x0000000000000001UL << 0x0E,
    ClassLayout = 0x0000000000000001UL << 0x0F,
    FieldLayout = 0x0000000000000001UL << 0x10,
    StandAloneSig = 0x0000000000000001UL << 0x11,
    EventMap = 0x0000000000000001UL << 0x12,
    EventPtr = 0x0000000000000001UL << 0x13,
    Event = 0x0000000000000001UL << 0x14,
    PropertyMap = 0x0000000000000001UL << 0x15,
    PropertyPtr = 0x0000000000000001UL << 0x16,
    Property = 0x0000000000000001UL << 0x17,
    MethodSemantics = 0x0000000000000001UL << 0x18,
    MethodImpl = 0x0000000000000001UL << 0x19,
    ModuleRef = 0x0000000000000001UL << 0x1A,
    TypeSpec = 0x0000000000000001UL << 0x1B,
    ImplMap = 0x0000000000000001UL << 0x1C,
    FieldRva = 0x0000000000000001UL << 0x1D,
    EnCLog = 0x0000000000000001UL << 0x1E,
    EnCMap = 0x0000000000000001UL << 0x1F,
    Assembly = 0x0000000000000001UL << 0x20,
    AssemblyProcessor = 0x0000000000000001UL << 0x21,
    AssemblyOS = 0x0000000000000001UL << 0x22,
    AssemblyRef = 0x0000000000000001UL << 0x23,
    AssemblyRefProcessor = 0x0000000000000001UL << 0x24,
    AssemblyRefOS = 0x0000000000000001UL << 0x25,
    File = 0x0000000000000001UL << 0x26,
    ExportedType = 0x0000000000000001UL << 0x27,
    ManifestResource = 0x0000000000000001UL << 0x28,
    NestedClass = 0x0000000000000001UL << 0x29,
    GenericParam = 0x0000000000000001UL << 0x2A,
    MethodSpec = 0x0000000000000001UL << 0x2B,
    GenericParamConstraint = 0x0000000000000001UL << 0x2C,

    SortedTablesMask =
      TableMask.ClassLayout
      | TableMask.Constant
      | TableMask.CustomAttribute
      | TableMask.DeclSecurity
      | TableMask.FieldLayout
      | TableMask.FieldMarshal
      | TableMask.FieldRva
      | TableMask.GenericParam
      | TableMask.GenericParamConstraint
      | TableMask.ImplMap
      | TableMask.InterfaceImpl
      | TableMask.MethodImpl
      | TableMask.MethodSemantics
      | TableMask.NestedClass,
    CompressedStreamNotAllowedMask =
      TableMask.FieldPtr
      | TableMask.MethodPtr
      | TableMask.ParamPtr
      | TableMask.EventPtr
      | TableMask.PropertyPtr
      | TableMask.EnCLog
      | TableMask.EnCMap,
    V1_0_TablesMask =
      TableMask.Module
      | TableMask.TypeRef
      | TableMask.TypeDef
      | TableMask.FieldPtr
      | TableMask.Field
      | TableMask.MethodPtr
      | TableMask.Method
      | TableMask.ParamPtr
      | TableMask.Param
      | TableMask.InterfaceImpl
      | TableMask.MemberRef
      | TableMask.Constant
      | TableMask.CustomAttribute
      | TableMask.FieldMarshal
      | TableMask.DeclSecurity
      | TableMask.ClassLayout
      | TableMask.FieldLayout
      | TableMask.StandAloneSig
      | TableMask.EventMap
      | TableMask.EventPtr
      | TableMask.Event
      | TableMask.PropertyMap
      | TableMask.PropertyPtr
      | TableMask.Property
      | TableMask.MethodSemantics
      | TableMask.MethodImpl
      | TableMask.ModuleRef
      | TableMask.TypeSpec
      | TableMask.ImplMap
      | TableMask.FieldRva
      | TableMask.EnCLog
      | TableMask.EnCMap
      | TableMask.Assembly
      | TableMask.AssemblyRef
      | TableMask.File
      | TableMask.ExportedType
      | TableMask.ManifestResource
      | TableMask.NestedClass,
    V1_1_TablesMask =
      TableMask.Module
      | TableMask.TypeRef
      | TableMask.TypeDef
      | TableMask.FieldPtr
      | TableMask.Field
      | TableMask.MethodPtr
      | TableMask.Method
      | TableMask.ParamPtr
      | TableMask.Param
      | TableMask.InterfaceImpl
      | TableMask.MemberRef
      | TableMask.Constant
      | TableMask.CustomAttribute
      | TableMask.FieldMarshal
      | TableMask.DeclSecurity
      | TableMask.ClassLayout
      | TableMask.FieldLayout
      | TableMask.StandAloneSig
      | TableMask.EventMap
      | TableMask.EventPtr
      | TableMask.Event
      | TableMask.PropertyMap
      | TableMask.PropertyPtr
      | TableMask.Property
      | TableMask.MethodSemantics
      | TableMask.MethodImpl
      | TableMask.ModuleRef
      | TableMask.TypeSpec
      | TableMask.ImplMap
      | TableMask.FieldRva
      | TableMask.EnCLog
      | TableMask.EnCMap
      | TableMask.Assembly
      | TableMask.AssemblyRef
      | TableMask.File
      | TableMask.ExportedType
      | TableMask.ManifestResource
      | TableMask.NestedClass,
    V2_0_TablesMask =
      TableMask.Module
      | TableMask.TypeRef
      | TableMask.TypeDef
      | TableMask.FieldPtr
      | TableMask.Field
      | TableMask.MethodPtr
      | TableMask.Method
      | TableMask.ParamPtr
      | TableMask.Param
      | TableMask.InterfaceImpl
      | TableMask.MemberRef
      | TableMask.Constant
      | TableMask.CustomAttribute
      | TableMask.FieldMarshal
      | TableMask.DeclSecurity
      | TableMask.ClassLayout
      | TableMask.FieldLayout
      | TableMask.StandAloneSig
      | TableMask.EventMap
      | TableMask.EventPtr
      | TableMask.Event
      | TableMask.PropertyMap
      | TableMask.PropertyPtr
      | TableMask.Property
      | TableMask.MethodSemantics
      | TableMask.MethodImpl
      | TableMask.ModuleRef
      | TableMask.TypeSpec
      | TableMask.ImplMap
      | TableMask.FieldRva
      | TableMask.EnCLog
      | TableMask.EnCMap
      | TableMask.Assembly
      | TableMask.AssemblyRef
      | TableMask.File
      | TableMask.ExportedType
      | TableMask.ManifestResource
      | TableMask.NestedClass
      | TableMask.GenericParam
      | TableMask.MethodSpec
      | TableMask.GenericParamConstraint,
  }

  internal enum HeapSizeFlag : byte {
    StringHeapLarge = 0x01, //  4 byte uint indexes used for string heap offsets
    GUIDHeapLarge = 0x02,   //  4 byte uint indexes used for GUID heap offsets
    BlobHeapLarge = 0x04,   //  4 byte uint indexes used for Blob heap offsets
    EnCDeltas = 0x20,       //  Indicates only EnC Deltas are present
    DeletedMarks = 0x80,    //  Indicates metadata might contain items marked deleted
  }

  internal static class TokenTypeIds {
    internal const uint Module = 0x00000000;
    internal const uint TypeRef = 0x01000000;
    internal const uint TypeDef = 0x02000000;
    internal const uint FieldDef = 0x04000000;
    internal const uint MethodDef = 0x06000000;
    internal const uint ParamDef = 0x08000000;
    internal const uint InterfaceImpl = 0x09000000;
    internal const uint MemberRef = 0x0a000000;
    internal const uint CustomAttribute = 0x0c000000;
    internal const uint Permission = 0x0e000000;
    internal const uint Signature = 0x11000000;
    internal const uint Event = 0x14000000;
    internal const uint Property = 0x17000000;
    internal const uint ModuleRef = 0x1a000000;
    internal const uint TypeSpec = 0x1b000000;
    internal const uint Assembly = 0x20000000;
    internal const uint AssemblyRef = 0x23000000;
    internal const uint File = 0x26000000;
    internal const uint ExportedType = 0x27000000;
    internal const uint ManifestResource = 0x28000000;
    internal const uint GenericParam = 0x2a000000;
    internal const uint MethodSpec = 0x2b000000;
    internal const uint GenericParamConstraint = 0x2c000000;
    internal const uint String = 0x70000000;
    internal const uint Name = 0x71000000;
    internal const uint BaseType = 0x72000000;       // Leave this on the high end value. This does not correspond to metadata table???

    internal const uint RIDMask = 0x00FFFFFF;
    internal const uint TokenTypeMask = 0xFF000000;
  }

  internal enum AssemblyHashAlgorithmFlags : uint {
    None = 0x00000000,
    MD5 = 0x00008003,
    SHA1 = 0x00008004
  }

  internal enum TypeDefFlags : uint {
    PrivateAccess = 0x00000000,
    PublicAccess = 0x00000001,
    NestedPublicAccess = 0x00000002,
    NestedPrivateAccess = 0x00000003,
    NestedFamilyAccess = 0x00000004,
    NestedAssemblyAccess = 0x00000005,
    NestedFamilyAndAssemblyAccess = 0x00000006,
    NestedFamilyOrAssemblyAccess = 0x00000007,
    AccessMask = 0x0000007,
    NestedMask = 0x00000006,

    AutoLayout = 0x00000000,
    SeqentialLayout = 0x00000008,
    ExplicitLayout = 0x00000010,
    LayoutMask = 0x00000018,

    ClassSemantics = 0x00000000,
    InterfaceSemantics = 0x00000020,
    AbstractSemantics = 0x00000080,
    SealedSemantics = 0x00000100,
    SpecialNameSemantics = 0x00000400,
    RTSpecialNameReserved = 0x00000800,

    ImportImplementation = 0x00001000,
    SerializableImplementation = 0x00002000,
    IsForeign = 0x00004000,

    AnsiString = 0x00000000,
    UnicodeString = 0x00010000,
    AutoCharString = 0x00020000,
    StringMask = 0x00030000,
    HasSecurityReserved = 0x00040000,

    BeforeFieldInitImplementation = 0x00100000,
    ForwarderImplementation = 0x00200000,
    DoesNotInheritTypeParameters = 0x10000000
  }

  internal enum FieldFlags : ushort {
    CompilerControlledAccess = 0x0000,
    PrivateAccess = 0x0001,
    FamilyAndAssemblyAccess = 0x0002,
    AssemblyAccess = 0x0003,
    FamilyAccess = 0x0004,
    FamilyOrAssemblyAccess = 0x0005,
    PublicAccess = 0x0006,
    AccessMask = 0x0007,

    StaticContract = 0x0010,
    InitOnlyContract = 0x0020,
    LiteralContract = 0x0040,
    NotSerializedContract = 0x0080,

    SpecialNameImpl = 0x0200,
    PInvokeImpl = 0x2000,

    RTSpecialNameReserved = 0x0400,
    HasFieldMarshalReserved = 0x1000,
    HasDefaultReserved = 0x8000,
    HasFieldRVAReserved = 0x0100,

    //  Load flags
    FieldLoaded = 0x4000,
  }

  internal enum MethodFlags : ushort {
    CompilerControlledAccess = 0x0000,
    PrivateAccess = 0x0001,
    FamilyAndAssemblyAccess = 0x0002,
    AssemblyAccess = 0x0003,
    FamilyAccess = 0x0004,
    FamilyOrAssemblyAccess = 0x0005,
    PublicAccess = 0x0006,
    AccessMask = 0x0007,

    StaticContract = 0x0010,
    FinalContract = 0x0020,
    VirtualContract = 0x0040,
    HideBySignatureContract = 0x0080,

    ReuseSlotVTable = 0x0000,
    NewSlotVTable = 0x0100,

    CheckAccessOnOverrideImpl = 0x0200,
    AbstractImpl = 0x0400,
    SpecialNameImpl = 0x0800,

    PInvokeInterop = 0x2000,
    UnmanagedExportInterop = 0x0008,

    RTSpecialNameReserved = 0x1000,
    HasSecurityReserved = 0x4000,
    RequiresSecurityObjectReserved = 0x8000,
  }

  internal enum ParamFlags : ushort {
    InSemantics = 0x0001,
    OutSemantics = 0x0002,
    OptionalSemantics = 0x0010,

    HasDefaultReserved = 0x1000,
    HasFieldMarshalReserved = 0x2000,

    //  Comes from signature...
    ByReference = 0x0100,
    ParamArray = 0x0200,
  }

  internal enum PropertyFlags : ushort {
    SpecialNameImpl = 0x0200,

    RTSpecialNameReserved = 0x0400,
    HasDefaultReserved = 0x1000,

    //  Comes from signature...
    HasThis = 0x0001,
    ReturnValueIsByReference = 0x0002,
    //  Load flags
    GetterLoaded = 0x0004,
    SetterLoaded = 0x0008,
  }

  internal enum EventFlags : ushort {
    SpecialNameImpl = 0x0200,

    RTSpecialNameReserved = 0x0400,

    //  Load flags
    AdderLoaded = 0x0001,
    RemoverLoaded = 0x0002,
    FireLoaded = 0x0004,
  }

  internal enum MethodSemanticsFlags : ushort {
    Setter = 0x0001,
    Getter = 0x0002,
    Other = 0x0004,
    AddOn = 0x0008,
    RemoveOn = 0x0010,
    Fire = 0x0020,
  }

  internal enum DeclSecurityActionFlags : ushort {
    ActionNil = 0x0000,
    Request = 0x0001,
    Demand = 0x0002,
    Assert = 0x0003,
    Deny = 0x0004,
    PermitOnly = 0x0005,
    LinktimeCheck = 0x0006,
    InheritanceCheck = 0x0007,
    RequestMinimum = 0x0008,
    RequestOptional = 0x0009,
    RequestRefuse = 0x000A,
    PrejitGrant = 0x000B,
    PrejitDenied = 0x000C,
    NonCasDemand = 0x000D,
    NonCasLinkDemand = 0x000E,
    NonCasInheritance = 0x000F,
    MaximumValue = 0x000F,
    ActionMask = 0x001F,
  }

  internal enum MethodImplFlags : ushort {
    ILCodeType = 0x0000,
    NativeCodeType = 0x0001,
    OPTILCodeType = 0x0002,
    RuntimeCodeType = 0x0003,
    CodeTypeMask = 0x0003,

    Unmanaged = 0x0004,
    NoInlining = 0x0008,
    ForwardRefInterop = 0x0010,
    Synchronized = 0x0020,
    NoOptimization = 0x0040,
    PreserveSigInterop = 0x0080,
    AggressiveInlining = 0x0100,
    InternalCall = 0x1000,

  }

  internal enum PInvokeMapFlags : ushort {
    NoMangle = 0x0001,

    DisabledBestFit = 0x0020,
    EnabledBestFit = 0x0010,
    UseAssemblyBestFit = 0x0000,
    BestFitMask = 0x0030,

    CharSetNotSpec = 0x0000,
    CharSetAnsi = 0x0002,
    CharSetUnicode = 0x0004,
    CharSetAuto = 0x0006,
    CharSetMask = 0x0006,

    EnabledThrowOnUnmappableChar = 0x1000,
    DisabledThrowOnUnmappableChar = 0x2000,
    UseAssemblyThrowOnUnmappableChar = 0x0000,
    ThrowOnUnmappableCharMask = 0x3000,

    SupportsLastError = 0x0040,

    WinAPICallingConvention = 0x0100,
    CDeclCallingConvention = 0x0200,
    StdCallCallingConvention = 0x0300,
    ThisCallCallingConvention = 0x0400,
    FastCallCallingConvention = 0x0500,
    CallingConventionMask = 0x0700,
  }

  internal enum AssemblyFlags : uint {
    PublicKey = 0x00000001,
    Retargetable = 0x00000100,
    ContainsForeignTypes = 0x00000200
  }

  internal enum ManifestResourceFlags : uint {
    PublicVisibility = 0x00000001,
    PrivateVisibility = 0x00000002,
    VisibilityMask = 0x00000007,

    InExternalFile = 0x00000010,
  }

  internal enum FileFlags : uint {
    ContainsMetadata = 0x00000000,
    ContainsNoMetadata = 0x00000001,
  }

  internal enum GenericParamFlags : ushort {
    NonVariant = 0x0000,
    Covariant = 0x0001,
    Contravariant = 0x0002,
    VarianceMask = 0x0003,

    ReferenceTypeConstraint = 0x0004,
    ValueTypeConstraint = 0x0008,
    DefaultConstructorConstraint = 0x0010,
  }

  #region Signature Specific data

  internal static class ElementType {
    internal const byte End = 0x00;
    internal const byte Void = 0x01;
    internal const byte Boolean = 0x02;
    internal const byte Char = 0x03;
    internal const byte Int8 = 0x04;
    internal const byte UInt8 = 0x05;
    internal const byte Int16 = 0x06;
    internal const byte UInt16 = 0x07;
    internal const byte Int32 = 0x08;
    internal const byte UInt32 = 0x09;
    internal const byte Int64 = 0x0a;
    internal const byte UInt64 = 0x0b;
    internal const byte Single = 0x0c;
    internal const byte Double = 0x0d;
    internal const byte String = 0x0e;

    internal const byte Pointer = 0x0f;
    internal const byte ByReference = 0x10;

    internal const byte ValueType = 0x11;
    internal const byte Class = 0x12;
    internal const byte GenericTypeParameter = 0x13;
    internal const byte Array = 0x14;
    internal const byte GenericTypeInstance = 0x15;
    internal const byte TypedReference = 0x16;

    internal const byte IntPtr = 0x18;
    internal const byte UIntPtr = 0x19;
    internal const byte FunctionPointer = 0x1b;
    internal const byte Object = 0x1c;
    internal const byte SzArray = 0x1d;

    internal const byte GenericMethodParameter = 0x1e;

    internal const byte RequiredModifier = 0x1f;
    internal const byte OptionalModifier = 0x20;

    internal const byte Internal = 0x21;

    internal const byte Max = 0x22;

    internal const byte Modifier = 0x40;
    internal const byte Sentinel = 0x41;
    internal const byte Pinned = 0x45;
    internal const byte SingleHFA = 0x54; //  What is this?
    internal const byte DoubleHFA = 0x55; //  What is this?
  }

  internal static class SignatureHeader {
    internal const byte DefaultCall = 0x00;
    internal const byte CCall = 0x01;
    internal const byte StdCall = 0x02;
    internal const byte ThisCall = 0x03;
    internal const byte FastCall = 0x04;
    internal const byte VarArgCall = 0x05;
    internal const byte Field = 0x06;
    internal const byte LocalVar = 0x07;
    internal const byte Property = 0x08;
    //internal const byte UnManaged = 0x09;  //  Not used as of now in CLR
    internal const byte GenericInstance = 0x0A;
    //internal const byte NativeVarArg = 0x0B;  //  Not used as of now in CLR
    internal const byte Max = 0x0C;
    internal const byte CallingConventionMask = 0x0F;


    internal const byte HasThis = 0x20;
    internal const byte ExplicitThis = 0x40;
    internal const byte Generic = 0x10;

    internal static bool IsMethodSignature(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.CallingConventionMask) <= SignatureHeader.VarArgCall;
    }
    internal static bool IsVarArgCallSignature(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.VarArgCall;
    }
    internal static bool IsFieldSignature(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.Field;
    }
    internal static bool IsLocalVarSignature(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.LocalVar;
    }
    internal static bool IsPropertySignature(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.Property;
    }
    internal static bool IsGenericInstanceSignature(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.GenericInstance;
    }
    internal static bool IsExplicitThis(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.ExplicitThis) == SignatureHeader.ExplicitThis;
    }
    internal static bool IsGeneric(
      byte signatureHeader
    ) {
      return (signatureHeader & SignatureHeader.Generic) == SignatureHeader.Generic;
    }
  }

  internal static class SerializationType {
    internal const ushort CustomAttributeStart = 0x0001;
    internal const byte SecurityAttribute20Start = 0x2E;  //  '.'
    internal const byte Undefined = 0x00;
    internal const byte Boolean = ElementType.Boolean;
    internal const byte Char = ElementType.Char;
    internal const byte Int8 = ElementType.Int8;
    internal const byte UInt8 = ElementType.UInt8;
    internal const byte Int16 = ElementType.Int16;
    internal const byte UInt16 = ElementType.UInt16;
    internal const byte Int32 = ElementType.Int32;
    internal const byte UInt32 = ElementType.UInt32;
    internal const byte Int64 = ElementType.Int64;
    internal const byte UInt64 = ElementType.UInt64;
    internal const byte Single = ElementType.Single;
    internal const byte Double = ElementType.Double;
    internal const byte String = ElementType.String;
    internal const byte SZArray = ElementType.SzArray;
    internal const byte Type = 0x50;
    internal const byte TaggedObject = 0x51;
    internal const byte Field = 0x53;
    internal const byte Property = 0x54;
    internal const byte Enum = 0x55;
  }

  #endregion

}

namespace Microsoft.Cci.MetadataReader.PEFile {
  using Microsoft.Cci.MetadataReader.PEFileFlags;
  using Microsoft.Cci.UtilityDataStructures;

  #region PEFile specific data

  internal static class PEFileConstants {
    internal const ushort DosSignature = 0x5A4D;     // MZ
    internal const int PESignatureOffsetLocation = 0x3C;
    internal const uint PESignature = 0x00004550;    // PE00
    internal const int BasicPEHeaderSize = PEFileConstants.PESignatureOffsetLocation;
    internal const int SizeofCOFFFileHeader = 20;
    internal const int SizeofOptionalHeaderStandardFields32 = 28;
    internal const int SizeofOptionalHeaderStandardFields64 = 24;
    internal const int SizeofOptionalHeaderNTAdditionalFields32 = 68;
    internal const int SizeofOptionalHeaderNTAdditionalFields64 = 88;
    internal const int NumberofOptionalHeaderDirectoryEntries = 16;
    internal const int SizeofOptionalHeaderDirectoriesEntries = 16*8;
    internal const int SizeofSectionHeader = 40;
    internal const int SizeofSectionName = 8;
    internal const int SizeofResourceDirectory = 16;
    internal const int SizeofResourceDirectoryEntry = 8;
  }

  internal struct COFFFileHeader {
    internal Machine Machine;
    internal short NumberOfSections;
    internal int TimeDateStamp;
    internal int PointerToSymbolTable;
    internal int NumberOfSymbols;
    internal short SizeOfOptionalHeader;
    internal Characteristics Characteristics;
  }

  internal class PeDebugDirectory {
    public uint Characteristics;
    public uint TimeDateStamp;
    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint Type;
    public uint SizeOfData;
    public uint AddressOfRawData;
    public uint PointerToRawData;
  }

  internal struct OptionalHeaderStandardFields {
    internal PEMagic PEMagic;
    internal byte MajorLinkerVersion;
    internal byte MinorLinkerVersion;
    internal int SizeOfCode;
    internal int SizeOfInitializedData;
    internal int SizeOfUninitializedData;
    internal int RVAOfEntryPoint;
    internal int BaseOfCode;
    internal int BaseOfData;
  }

  internal struct OptionalHeaderNTAdditionalFields {
    internal ulong ImageBase;
    internal int SectionAlignment;
    internal uint FileAlignment;
    internal ushort MajorOperatingSystemVersion;
    internal ushort MinorOperatingSystemVersion;
    internal ushort MajorImageVersion;
    internal ushort MinorImageVersion;
    internal ushort MajorSubsystemVersion;
    internal ushort MinorSubsystemVersion;
    internal uint Win32VersionValue;
    internal int SizeOfImage;
    internal int SizeOfHeaders;
    internal uint CheckSum;
    internal Subsystem Subsystem;
    internal DllCharacteristics DllCharacteristics;
    internal ulong SizeOfStackReserve;
    internal ulong SizeOfStackCommit;
    internal ulong SizeOfHeapReserve;
    internal ulong SizeOfHeapCommit;
    internal uint LoaderFlags;
    internal int NumberOfRvaAndSizes;
  }

  internal struct DirectoryEntry {
    internal int RelativeVirtualAddress;
    internal uint Size;
  }

  internal struct OptionalHeaderDirectoryEntries {
    internal DirectoryEntry ExportTableDirectory;
    internal DirectoryEntry ImportTableDirectory;
    internal DirectoryEntry ResourceTableDirectory;
    internal DirectoryEntry ExceptionTableDirectory;
    internal DirectoryEntry CertificateTableDirectory;
    internal DirectoryEntry BaseRelocationTableDirectory;
    internal DirectoryEntry DebugTableDirectory;
    internal DirectoryEntry CopyrightTableDirectory;
    internal DirectoryEntry GlobalPointerTableDirectory;
    internal DirectoryEntry ThreadLocalStorageTableDirectory;
    internal DirectoryEntry LoadConfigTableDirectory;
    internal DirectoryEntry BoundImportTableDirectory;
    internal DirectoryEntry ImportAddressTableDirectory;
    internal DirectoryEntry DelayImportTableDirectory;
    internal DirectoryEntry COR20HeaderTableDirectory;
    internal DirectoryEntry ReservedDirectory;
  }

  internal struct SectionHeader {
    internal string Name;
    internal int VirtualSize;
    internal int VirtualAddress;
    internal int SizeOfRawData;
    internal int OffsetToRawData;
    internal int RVAToRelocations;
    internal int PointerToLineNumbers;
    internal ushort NumberOfRelocations;
    internal ushort NumberOfLineNumbers;
    internal SectionCharacteristics SectionCharacteristics;
  }

  internal struct SubSection {
    internal readonly string SectionName;
    internal readonly uint Offset;
    internal readonly MemoryBlock MemoryBlock;
    internal SubSection(
      string sectionName,
      uint offset,
      MemoryBlock memoryBlock
    ) {
      this.SectionName = sectionName;
      this.Offset = offset;
      this.MemoryBlock = memoryBlock;
    }
    internal SubSection(
      string sectionName,
      int offset,
      MemoryBlock memoryBlock
    ) {
      this.SectionName = sectionName;
      this.Offset = (uint)offset;
      this.MemoryBlock = memoryBlock;
    }
  }

  internal struct ResourceDirectory {
    internal uint Charecteristics;
    internal uint TimeDateStamp;
    internal short MajorVersion;
    internal short MinorVersion;
    internal short NumberOfNamedEntries;
    internal short NumberOfIdEntries;
  }

  internal struct ResourceDirectoryEntry {
    internal readonly int NameOrId;
    readonly int DataOffset;
    internal bool IsDirectory {
      get {
        return (this.DataOffset & 0x80000000) == 0x80000000;
      }
    }
    internal int OffsetToDirectory {
      get {
        return this.DataOffset & 0x7FFFFFFF;
      }
    }
    internal int OffsetToData {
      get {
        return this.DataOffset & 0x7FFFFFFF;
      }
    }
    internal ResourceDirectoryEntry(
      int nameOrId,
      int dataOffset
    ) {
      this.NameOrId = nameOrId;
      this.DataOffset = dataOffset;
    }
  }

  internal struct ResourceDataEntry {
    internal readonly int RVAToData;
    internal readonly int Size;
    internal readonly int CodePage;
    internal readonly int Reserved;

    internal ResourceDataEntry(
      int rvaToData,
      int size,
      int codePage,
      int reserved
    ) {
      this.RVAToData = rvaToData;
      this.Size = size;
      this.CodePage = codePage;
      this.Reserved = reserved;
    }

  }

  #endregion PEFile specific data


  #region CLR Header Specific data

  internal static class COR20Constants {
    internal const int SizeOfCOR20Header = 72;
    internal const uint COR20MetadataSignature = 0x424A5342;
    internal const int MinimumSizeofMetadataHeader = 16;
    internal const int SizeofStorageHeader = 4;
    internal const int MinimumSizeofStreamHeader = 8;
    internal const string StringStreamName = "#Strings";
    internal const string BlobStreamName = "#Blob";
    internal const string GUIDStreamName = "#GUID";
    internal const string UserStringStreamName = "#US";
    internal const string CompressedMetadataTableStreamName = "#~";
    internal const string UncompressedMetadataTableStreamName = "#-";
    internal const int LargeStreamHeapSize = 0x0001000;
  }

  internal struct COR20Header {
    internal int CountBytes;
    internal ushort MajorRuntimeVersion;
    internal ushort MinorRuntimeVersion;
    internal DirectoryEntry MetaDataDirectory;
    internal COR20Flags COR20Flags;
    internal uint EntryPointTokenOrRVA;
    internal DirectoryEntry ResourcesDirectory;
    internal DirectoryEntry StrongNameSignatureDirectory;
    internal DirectoryEntry CodeManagerTableDirectory;
    internal DirectoryEntry VtableFixupsDirectory;
    internal DirectoryEntry ExportAddressTableJumpsDirectory;
    internal DirectoryEntry ManagedNativeHeaderDirectory;
  }

  internal struct MetadataHeader {
    internal uint Signature;
    internal ushort MajorVersion;
    internal ushort MinorVersion;
    internal uint ExtraData;
    internal int VersionStringSize;
    internal string VersionString;
  }

  internal struct StorageHeader {
    internal ushort Flags;
    internal short NumberOfStreams;
  }

  internal struct StreamHeader {
    internal uint Offset;
    internal int Size;
    internal string Name;
  }

  #endregion CLR Header Specific data


  #region Metadata Stream Specific data

  internal static class MetadataStreamConstants {
    internal const int SizeOfMetadataTableHeader = 24;
    internal const uint LargeTableRowCount = 0x00010000;
  }

  internal struct MetadataTableHeader {
    internal uint Reserved;
    internal byte MajorVersion;
    internal byte MinorVersion;
    internal HeapSizeFlag HeapSizeFlags;
    internal byte RowId;
    internal TableMask ValidTables;
    internal TableMask SortedTables;
    //  Helper methods
    internal int GetNumberOfTablesPresent() {
      const ulong MASK_01010101010101010101010101010101 = 0x5555555555555555UL;
      const ulong MASK_00110011001100110011001100110011 = 0x3333333333333333UL;
      const ulong MASK_00001111000011110000111100001111 = 0x0F0F0F0F0F0F0F0FUL;
      const ulong MASK_00000000111111110000000011111111 = 0x00FF00FF00FF00FFUL;
      const ulong MASK_00000000000000001111111111111111 = 0x0000FFFF0000FFFFUL;
      const ulong MASK_11111111111111111111111111111111 = 0x00000000FFFFFFFFUL;
      ulong count = (ulong)this.ValidTables;
      count = (count & MASK_01010101010101010101010101010101) + ((count >> 1) & MASK_01010101010101010101010101010101);
      count = (count & MASK_00110011001100110011001100110011) + ((count >> 2) & MASK_00110011001100110011001100110011);
      count = (count & MASK_00001111000011110000111100001111) + ((count >> 4) & MASK_00001111000011110000111100001111);
      count = (count & MASK_00000000111111110000000011111111) + ((count >> 8) & MASK_00000000111111110000000011111111);
      count = (count & MASK_00000000000000001111111111111111) + ((count >> 16) & MASK_00000000000000001111111111111111);
      count = (count & MASK_11111111111111111111111111111111) + ((count >> 32) & MASK_11111111111111111111111111111111);
      return (int)count;
    }
  }

  internal static class TypeDefOrRefTag {
    internal const int NumberOfBits = 2;
    internal const uint LargeRowSize = 0x00000001 << (16 - TypeDefOrRefTag.NumberOfBits);
    internal const uint TypeDef = 0x00000000;
    internal const uint TypeRef = 0x00000001;
    internal const uint TypeSpec = 0x00000002;
    internal const uint TagMask = 0x00000003;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.TypeDef, TokenTypeIds.TypeRef, TokenTypeIds.TypeSpec };
    internal const TableMask TablesReferenced =
      TableMask.TypeDef
      | TableMask.TypeRef
      | TableMask.TypeSpec;
    internal static uint ConvertToToken(uint typeDefOrRefTag) {
      return TypeDefOrRefTag.TagToTokenTypeArray[typeDefOrRefTag & TypeDefOrRefTag.TagMask] | typeDefOrRefTag >> TypeDefOrRefTag.NumberOfBits;
    }
  }

  internal static class HasConstantTag {
    internal const int NumberOfBits = 2;
    internal const uint LargeRowSize = 0x00000001 << (16 - HasConstantTag.NumberOfBits);
    internal const uint Field = 0x00000000;
    internal const uint Param = 0x00000001;
    internal const uint Property = 0x00000002;
    internal const uint TagMask = 0x00000003;
    internal const TableMask TablesReferenced =
      TableMask.Field
      | TableMask.Param
      | TableMask.Property;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.FieldDef, TokenTypeIds.ParamDef, TokenTypeIds.Property };
    internal static uint ConvertToToken(uint hasConstant) {
      return HasConstantTag.TagToTokenTypeArray[hasConstant & HasConstantTag.TagMask] | hasConstant >> HasConstantTag.NumberOfBits;
    }
    internal static uint ConvertToTag(uint token) {
      uint tokenKind = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      if (tokenKind == TokenTypeIds.FieldDef) {
        return rowId << HasConstantTag.NumberOfBits | HasConstantTag.Field;
      } else if (tokenKind == TokenTypeIds.ParamDef) {
        return rowId << HasConstantTag.NumberOfBits | HasConstantTag.Param;
      } else if (tokenKind == TokenTypeIds.Property) {
        return rowId << HasConstantTag.NumberOfBits | HasConstantTag.Property;
      }
      return 0;
    }
  }

  internal static class HasCustomAttributeTag {
    internal const int NumberOfBits = 5;
    internal const uint LargeRowSize = 0x00000001 << (16 - HasCustomAttributeTag.NumberOfBits);
    internal const uint Method = 0x00000000;
    internal const uint Field = 0x00000001;
    internal const uint TypeRef = 0x00000002;
    internal const uint TypeDef = 0x00000003;
    internal const uint Param = 0x00000004;
    internal const uint InterfaceImpl = 0x00000005;
    internal const uint MemberRef = 0x00000006;
    internal const uint Module = 0x00000007;
    internal const uint DeclSecurity = 0x00000008;
    internal const uint Property = 0x00000009;
    internal const uint Event = 0x0000000A;
    internal const uint StandAloneSig = 0x0000000B;
    internal const uint ModuleRef = 0x0000000C;
    internal const uint TypeSpec = 0x0000000D;
    internal const uint Assembly = 0x0000000E;
    internal const uint AssemblyRef = 0x0000000F;
    internal const uint File = 0x00000010;
    internal const uint ExportedType = 0x00000011;
    internal const uint ManifestResource = 0x00000012;
    internal const uint GenericParameter = 0x00000013;
    internal const uint GenericParameterConstraint = 0x00000014;
    internal const uint MethodSpec = 0x00000015;
    internal const uint TagMask = 0x0000001F;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.MethodDef, TokenTypeIds.FieldDef, TokenTypeIds.TypeRef, TokenTypeIds.TypeDef, TokenTypeIds.ParamDef,
          TokenTypeIds.InterfaceImpl, TokenTypeIds.MemberRef, TokenTypeIds.Module, TokenTypeIds.Permission, TokenTypeIds.Property, TokenTypeIds.Event,
          TokenTypeIds.Signature, TokenTypeIds.ModuleRef, TokenTypeIds.TypeSpec, TokenTypeIds.Assembly, TokenTypeIds.AssemblyRef, TokenTypeIds.File, TokenTypeIds.ExportedType,
          TokenTypeIds.ManifestResource, TokenTypeIds.GenericParam, TokenTypeIds.GenericParamConstraint, TokenTypeIds.MethodSpec };
    internal const TableMask TablesReferenced =
      TableMask.Method
      | TableMask.Field
      | TableMask.TypeRef
      | TableMask.TypeDef
      | TableMask.Param
      | TableMask.InterfaceImpl
      | TableMask.MemberRef
      | TableMask.Module
      | TableMask.DeclSecurity
      | TableMask.Property
      | TableMask.Event
      | TableMask.StandAloneSig
      | TableMask.ModuleRef
      | TableMask.TypeSpec
      | TableMask.Assembly
      | TableMask.AssemblyRef
      | TableMask.File
      | TableMask.ExportedType
      | TableMask.ManifestResource
      | TableMask.GenericParam
      | TableMask.GenericParamConstraint
      | TableMask.MethodSpec;
    internal static uint ConvertToToken(
      uint hasCustomAttribute
    ) {
      return HasCustomAttributeTag.TagToTokenTypeArray[hasCustomAttribute & HasCustomAttributeTag.TagMask] | hasCustomAttribute >> HasCustomAttributeTag.NumberOfBits;
    }
    internal static uint ConvertToTag(
      uint token
    ) {
      uint tokenType = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      switch (tokenType) {
        case TokenTypeIds.MethodDef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Method;
        case TokenTypeIds.FieldDef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Field;
        case TokenTypeIds.TypeRef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.TypeRef;
        case TokenTypeIds.TypeDef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.TypeDef;
        case TokenTypeIds.ParamDef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Param;
        case TokenTypeIds.InterfaceImpl:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.InterfaceImpl;
        case TokenTypeIds.MemberRef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.MemberRef;
        case TokenTypeIds.Module:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Module;
        case TokenTypeIds.Permission:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.DeclSecurity;
        case TokenTypeIds.Property:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Property;
        case TokenTypeIds.Event:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Event;
        case TokenTypeIds.Signature:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.StandAloneSig;
        case TokenTypeIds.ModuleRef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.ModuleRef;
        case TokenTypeIds.TypeSpec:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.TypeSpec;
        case TokenTypeIds.Assembly:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Assembly;
        case TokenTypeIds.AssemblyRef:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.AssemblyRef;
        case TokenTypeIds.File:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.File;
        case TokenTypeIds.ExportedType:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.ExportedType;
        case TokenTypeIds.ManifestResource:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.ManifestResource;
        case TokenTypeIds.GenericParam:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.GenericParameter;
        case TokenTypeIds.GenericParamConstraint:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.GenericParameterConstraint;
        case TokenTypeIds.MethodSpec:
          return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.MethodSpec;
      }
      return 0;
    }
  }

  internal static class HasFieldMarshalTag {
    internal const int NumberOfBits = 1;
    internal const uint LargeRowSize = 0x00000001 << (16 - HasFieldMarshalTag.NumberOfBits);
    internal const uint Field = 0x00000000;
    internal const uint Param = 0x00000001;
    internal const uint TagMask = 0x00000001;
    internal const TableMask TablesReferenced =
      TableMask.Field
      | TableMask.Param;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.FieldDef, TokenTypeIds.ParamDef };
    internal static uint ConvertToToken(uint hasFieldMarshal) {
      return HasFieldMarshalTag.TagToTokenTypeArray[hasFieldMarshal & HasFieldMarshalTag.TagMask] | hasFieldMarshal >> HasFieldMarshalTag.NumberOfBits;
    }
    internal static uint ConvertToTag(uint token) {
      uint tokenKind = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      if (tokenKind == TokenTypeIds.FieldDef) {
        return rowId << HasFieldMarshalTag.NumberOfBits | HasFieldMarshalTag.Field;
      } else if (tokenKind == TokenTypeIds.ParamDef) {
        return rowId << HasFieldMarshalTag.NumberOfBits | HasFieldMarshalTag.Param;
      }
      return 0;
    }
  }

  internal static class HasDeclSecurityTag {
    internal const int NumberOfBits = 2;
    internal const uint LargeRowSize = 0x00000001 << (16 - HasDeclSecurityTag.NumberOfBits);
    internal const uint TypeDef = 0x00000000;
    internal const uint Method = 0x00000001;
    internal const uint Assembly = 0x00000002;
    internal const uint TagMask = 0x00000003;
    internal const TableMask TablesReferenced =
      TableMask.TypeDef
      | TableMask.Method
      | TableMask.Assembly;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.TypeDef, TokenTypeIds.MethodDef, TokenTypeIds.Assembly };
    internal static uint ConvertToToken(uint hasDeclSecurity) {
      return HasDeclSecurityTag.TagToTokenTypeArray[hasDeclSecurity & HasDeclSecurityTag.TagMask] | hasDeclSecurity >> HasDeclSecurityTag.NumberOfBits;
    }
    internal static uint ConvertToTag(uint token) {
      uint tokenKind = token & TokenTypeIds.TokenTypeMask;
      uint rowId = token & TokenTypeIds.RIDMask;
      if (tokenKind == TokenTypeIds.TypeDef) {
        return rowId << HasDeclSecurityTag.NumberOfBits | HasDeclSecurityTag.TypeDef;
      } else if (tokenKind == TokenTypeIds.MethodDef) {
        return rowId << HasDeclSecurityTag.NumberOfBits | HasDeclSecurityTag.Method;
      } else if (tokenKind == TokenTypeIds.Assembly) {
        return rowId << HasDeclSecurityTag.NumberOfBits | HasDeclSecurityTag.Assembly;
      }
      return 0;
    }
  }

  internal static class MemberRefParentTag {
    internal const int NumberOfBits = 3;
    internal const uint LargeRowSize = 0x00000001 << (16 - MemberRefParentTag.NumberOfBits);
    internal const uint TypeDef = 0x00000000;
    internal const uint TypeRef = 0x00000001;
    internal const uint ModuleRef = 0x00000002;
    internal const uint Method = 0x00000003;
    internal const uint TypeSpec = 0x00000004;
    internal const uint TagMask = 0x00000007;
    internal const TableMask TablesReferenced =
      TableMask.TypeDef
      | TableMask.TypeRef
      | TableMask.ModuleRef
      | TableMask.Method
      | TableMask.TypeSpec;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.TypeDef, TokenTypeIds.TypeRef, TokenTypeIds.ModuleRef,
      TokenTypeIds.MethodDef, TokenTypeIds.TypeSpec };
    internal static uint ConvertToToken(uint memberRef) {
      return MemberRefParentTag.TagToTokenTypeArray[memberRef & MemberRefParentTag.TagMask] | memberRef >> MemberRefParentTag.NumberOfBits;
    }
  }

  internal static class HasSemanticsTag {
    internal const int NumberOfBits = 1;
    internal const uint LargeRowSize = 0x00000001 << (16 - HasSemanticsTag.NumberOfBits);
    internal const uint Event = 0x00000000;
    internal const uint Property = 0x00000001;
    internal const uint TagMask = 0x00000001;
    internal const TableMask TablesReferenced =
      TableMask.Event
      | TableMask.Property;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.Event, TokenTypeIds.Property };
    internal static uint ConvertToToken(uint hasSemantic) {
      return HasSemanticsTag.TagToTokenTypeArray[hasSemantic & HasSemanticsTag.TagMask] | hasSemantic >> HasSemanticsTag.NumberOfBits;
    }
    internal static uint ConvertEventRowIdToTag(uint eventRowId) {
      return eventRowId << HasSemanticsTag.NumberOfBits | HasSemanticsTag.Event;
    }
    internal static uint ConvertPropertyRowIdToTag(uint propertyRowId) {
      return propertyRowId << HasSemanticsTag.NumberOfBits | HasSemanticsTag.Property;
    }
  }

  internal static class MethodDefOrRefTag {
    internal const int NumberOfBits = 1;
    internal const uint LargeRowSize = 0x00000001 << (16 - MethodDefOrRefTag.NumberOfBits);
    internal const uint Method = 0x00000000;
    internal const uint MemberRef = 0x00000001;
    internal const uint TagMask = 0x00000001;
    internal const TableMask TablesReferenced =
      TableMask.Method
      | TableMask.MemberRef;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.MethodDef, TokenTypeIds.MemberRef };
    internal static uint ConvertToToken(uint methodDefOrRef) {
      return MethodDefOrRefTag.TagToTokenTypeArray[methodDefOrRef & MethodDefOrRefTag.TagMask] | methodDefOrRef >> MethodDefOrRefTag.NumberOfBits;
    }
  }

  internal static class MemberForwardedTag {
    internal const int NumberOfBits = 1;
    internal const uint LargeRowSize = 0x00000001 << (16 - MemberForwardedTag.NumberOfBits);
    internal const uint Field = 0x00000000;
    internal const uint Method = 0x00000001;
    internal const uint TagMask = 0x00000001;
    internal const TableMask TablesReferenced =
      TableMask.Field
      | TableMask.Method;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.FieldDef, TokenTypeIds.MethodDef };
    internal static uint ConvertToToken(uint memberForwarded) {
      return MemberForwardedTag.TagToTokenTypeArray[memberForwarded & MethodDefOrRefTag.TagMask] | memberForwarded >> MethodDefOrRefTag.NumberOfBits;
    }
    internal static uint ConvertMethodDefRowIdToTag(uint methodDefRowId) {
      return methodDefRowId << MemberForwardedTag.NumberOfBits | MemberForwardedTag.Method;
    }
#if false
    internal static uint ConvertFieldDefRowIdToTag(uint fieldDefRowId) {
      return fieldDefRowId << MemberForwardedTag.NumberOfBits | MemberForwardedTag.Field;
    }
#endif
  }

  internal static class ImplementationTag {
    internal const int NumberOfBits = 2;
    internal const uint LargeRowSize = 0x00000001 << (16 - ImplementationTag.NumberOfBits);
    internal const uint File = 0x00000000;
    internal const uint AssemblyRef = 0x00000001;
    internal const uint ExportedType = 0x00000002;
    internal const uint TagMask = 0x00000003;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.File, TokenTypeIds.AssemblyRef, TokenTypeIds.ExportedType };
    internal const TableMask TablesReferenced =
      TableMask.File
      | TableMask.AssemblyRef
      | TableMask.ExportedType;
    internal static uint ConvertToToken(uint implementation) {
      if (implementation == 0) return 0;
      return ImplementationTag.TagToTokenTypeArray[implementation & ImplementationTag.TagMask] | implementation >> ImplementationTag.NumberOfBits;
    }
  }

  internal static class CustomAttributeTypeTag {
    internal const int NumberOfBits = 3;
    internal const uint LargeRowSize = 0x00000001 << (16 - CustomAttributeTypeTag.NumberOfBits);
    internal const uint Method = 0x00000002;
    internal const uint MemberRef = 0x00000003;
    internal const uint TagMask = 0x0000007;
    internal static uint[] TagToTokenTypeArray = { 0, 0, TokenTypeIds.MethodDef, TokenTypeIds.MemberRef, 0 };
    internal const TableMask TablesReferenced =
      TableMask.Method
      | TableMask.MemberRef;
    internal static uint ConvertToToken(uint customAttributeType) {
      return CustomAttributeTypeTag.TagToTokenTypeArray[customAttributeType & CustomAttributeTypeTag.TagMask] | customAttributeType >> CustomAttributeTypeTag.NumberOfBits;
    }
  }

  internal static class ResolutionScopeTag {
    internal const int NumberOfBits = 2;
    internal const uint LargeRowSize = 0x00000001 << (16 - ResolutionScopeTag.NumberOfBits);
    internal const uint Module = 0x00000000;
    internal const uint ModuleRef = 0x00000001;
    internal const uint AssemblyRef = 0x00000002;
    internal const uint TypeRef = 0x00000003;
    internal const uint TagMask = 0x00000003;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.Module, TokenTypeIds.ModuleRef, TokenTypeIds.AssemblyRef, TokenTypeIds.TypeRef };
    internal const TableMask TablesReferenced =
      TableMask.Module
      | TableMask.ModuleRef
      | TableMask.AssemblyRef
      | TableMask.TypeRef;
    internal static uint ConvertToToken(uint resolutionScope) {
      return ResolutionScopeTag.TagToTokenTypeArray[resolutionScope & ResolutionScopeTag.TagMask] | resolutionScope >> ResolutionScopeTag.NumberOfBits;
    }
  }

  internal static class TypeOrMethodDefTag {
    internal const int NumberOfBits = 1;
    internal const uint LargeRowSize = 0x00000001 << (16 - TypeOrMethodDefTag.NumberOfBits);
    internal const uint TypeDef = 0x00000000;
    internal const uint MethodDef = 0x00000001;
    internal const uint TagMask = 0x0000001;
    internal static uint[] TagToTokenTypeArray = { TokenTypeIds.TypeDef, TokenTypeIds.MethodDef };
    internal const TableMask TablesReferenced =
      TableMask.TypeDef
      | TableMask.Method;
    internal static uint ConvertToToken(uint typeOrMethodDef) {
      return TypeOrMethodDefTag.TagToTokenTypeArray[typeOrMethodDef & TypeOrMethodDefTag.TagMask] | typeOrMethodDef >> TypeOrMethodDefTag.NumberOfBits;
    }
    internal static uint ConvertTypeDefRowIdToTag(uint typeDefRowId) {
      return typeDefRowId << TypeOrMethodDefTag.NumberOfBits | TypeOrMethodDefTag.TypeDef;
    }
    internal static uint ConvertMethodDefRowIdToTag(uint methodDefRowId) {
      return methodDefRowId << TypeOrMethodDefTag.NumberOfBits | TypeOrMethodDefTag.MethodDef;
    }
  }

  //  0x00
  internal struct ModuleRow {
    internal readonly ushort Generation;
    internal readonly uint Name;
    internal readonly uint MVId;
    internal readonly uint EnCId;
    internal readonly uint EnCBaseId;
    internal ModuleRow(
      ushort generation,
      uint name,
      uint mvId,
      uint encId,
      uint encBaseId
    ) {
      this.Generation = generation;
      this.Name = name;
      this.MVId = mvId;
      this.EnCId = encId;
      this.EnCBaseId = encBaseId;
    }
  }
  //  0x01
  internal struct TypeRefRow {
    internal readonly uint ResolutionScope;
    internal readonly uint Name;
    internal readonly uint Namespace;
    internal TypeRefRow(
      uint resolutionScope,
      uint name,
      uint @namespace
    ) {
      this.ResolutionScope = resolutionScope;
      this.Name = name;
      this.Namespace = @namespace;
    }
  }
  //  0x02
  internal struct TypeDefRow {
    internal readonly TypeDefFlags Flags;
    internal readonly uint Name;
    internal readonly uint Namespace;
    internal readonly uint Extends;
    internal readonly uint FieldList;
    internal readonly uint MethodList;
    internal TypeDefRow(
      TypeDefFlags flags,
      uint name,
      uint @namespace,
      uint extends,
      uint fieldList,
      uint methodList
    ) {
      this.Flags = flags;
      this.Name = name;
      this.Namespace = @namespace;
      this.Extends = extends;
      this.FieldList = fieldList;
      this.MethodList = methodList;
    }
    internal bool IsNested {
      get {
        return (this.Flags & TypeDefFlags.NestedMask) != 0;
      }
    }
  }
  //  0x03
  internal struct FieldPtrRow {
#if false
    internal readonly uint Field;
    internal FieldPtrRow(
      uint field
    ) {
      this.Field = field;
    }
#endif
  }
  //  0x04
  internal struct FieldRow {
    internal readonly FieldFlags Flags;
    internal readonly uint Name;
    internal readonly uint Signature;
    internal FieldRow(
      FieldFlags flags,
      uint name,
      uint signature
    ) {
      this.Flags = flags;
      this.Name = name;
      this.Signature = signature;
    }
  }
  //  0x05
  internal struct MethodPtrRow {
#if false
    internal readonly uint Method;
    internal MethodPtrRow(
      uint method
    ) {
      this.Method = method;
    }
#endif
  }
  //  0x06
  internal struct MethodRow {
    internal readonly int RVA;
    internal readonly MethodImplFlags ImplFlags;
    internal readonly MethodFlags Flags;
    internal readonly uint Name;
    internal readonly uint Signature;
    internal readonly uint ParamList;
    internal MethodRow(
      int rva,
      MethodImplFlags implFlags,
      MethodFlags flags,
      uint name,
      uint signature,
      uint paramList
    ) {
      this.RVA = rva;
      this.ImplFlags = implFlags;
      this.Flags = flags;
      this.Name = name;
      this.Signature = signature;
      this.ParamList = paramList;
    }
  }
  //  0x07
  internal struct ParamPtrRow {
#if false
    internal readonly uint Param;
    internal ParamPtrRow(
      uint param
    ) {
      this.Param = param;
    }
#endif
  }
  //  0x08
  internal struct ParamRow {
    internal readonly ParamFlags Flags;
    internal readonly ushort Sequence;
    internal readonly uint Name;
    internal ParamRow(
      ParamFlags flags,
      ushort sequence,
      uint name
    ) {
      this.Flags = flags;
      this.Sequence = sequence;
      this.Name = name;
    }
  }
  //  0x09
  internal struct InterfaceImplRow {
#if false
    internal readonly uint Class;
    internal readonly uint Interface;
    internal InterfaceImplRow(
      uint @class,
      uint @interface
    ) {
      this.Class = @class;
      this.Interface = @interface;
    }
#endif
  }
  //  0x0A
  internal struct MemberRefRow {
    internal readonly uint Class;
    internal readonly uint Name;
    internal readonly uint Signature;
    internal MemberRefRow(
      uint @class,
      uint name,
      uint signature
    ) {
      this.Class = @class;
      this.Name = name;
      this.Signature = signature;
    }
  }
  //  0x0B
  internal struct ConstantRow {
    internal readonly byte Type;
    internal readonly uint Parent;
    internal readonly uint Value;
    internal ConstantRow(
      byte type,
      uint parent,
      uint value
    ) {
      this.Type = type;
      this.Parent = parent;
      this.Value = value;
    }
  }
  //  0x0C
  internal struct CustomAttributeRow {
    internal readonly uint Parent;
    internal readonly uint Type;
    internal readonly uint Value;
    internal CustomAttributeRow(
      uint parent,
      uint type,
      uint value
    ) {
      this.Parent = parent;
      this.Type = type;
      this.Value = value;
    }
  }
  //  0x0D
  internal struct FieldMarshalRow {
    internal readonly uint Parent;
    internal readonly uint NativeType;
    internal FieldMarshalRow(
      uint parent,
      uint nativeType
    ) {
      this.Parent = parent;
      this.NativeType = nativeType;
    }
  }
  //  0x0E
  internal struct DeclSecurityRow {
    internal readonly DeclSecurityActionFlags ActionFlags;
    internal readonly uint Parent;
    internal readonly uint PermissionSet;
    internal DeclSecurityRow(
      DeclSecurityActionFlags actionFlags,
      uint parent,
      uint permissionSet
    ) {
      this.ActionFlags = actionFlags;
      this.Parent = parent;
      this.PermissionSet = permissionSet;
    }
  }
  //  0x0F
  internal struct ClassLayoutRow {
#if false
    internal readonly ushort PackingSize;
    internal readonly uint ClassSize;
    internal readonly uint Parent;
    internal ClassLayoutRow(
      ushort packingSize,
      uint classSize,
      uint parent
    ) {
      this.PackingSize = packingSize;
      this.ClassSize = classSize;
      this.Parent = parent;
    }
#endif
  }
  //  0x10
  internal struct FieldLayoutRow {
#if false
    internal readonly uint Offset;
    internal readonly uint Field;
    internal FieldLayoutRow(
      uint offset,
      uint field
    ) {
      this.Offset = offset;
      this.Field = field;
    }
#endif
  }
  //  0x11
  internal struct StandAloneSigRow {
    internal readonly uint Signature;
    internal StandAloneSigRow(
      uint signature
    ) {
      this.Signature = signature;
    }
  }
  //  0x12
  internal struct EventMapRow {
#if false
    internal readonly uint Parent;
    internal readonly uint EventList;
    internal EventMapRow(
      uint parent,
      uint eventList
    ) {
      this.Parent = parent;
      this.EventList = eventList;
    }
#endif
  }
  //  0x13
  internal struct EventPtrRow {
#if false
    internal readonly uint Event;
    internal EventPtrRow(
      uint @event
    ) {
      this.Event = @event;
    }
#endif
  }
  //  0x14
  internal struct EventRow {
    internal readonly EventFlags Flags;
    internal readonly uint Name;
    internal readonly uint EventType;
    internal EventRow(
      EventFlags flags,
      uint name,
      uint eventType
    ) {
      this.Flags = flags;
      this.Name = name;
      this.EventType = eventType;
    }
  }
  //  0x15
  internal struct PropertyMapRow {
#if false
    internal readonly uint Parent;
    internal readonly uint PropertyList;
    internal PropertyMapRow(
      uint parent,
      uint propertyList
    ) {
      this.Parent = parent;
      this.PropertyList = propertyList;
    }
#endif
  }
  //  0x16
  internal struct PropertyPtrRow {
#if false
    internal readonly uint Property;
    internal PropertyPtrRow(
      uint property
    ) {
      this.Property = property;
    }
#endif
  }
  //  0x17
  internal struct PropertyRow {
    internal readonly PropertyFlags Flags;
    internal readonly uint Name;
    internal readonly uint Signature;
    internal PropertyRow(
      PropertyFlags flags,
      uint name,
      uint signature
    ) {
      this.Flags = flags;
      this.Name = name;
      this.Signature = signature;
    }
  }
  //  0x18
  internal struct MethodSemanticsRow {
    internal readonly MethodSemanticsFlags SemanticsFlag;
    internal readonly uint Method;
    internal readonly uint Association;
    internal MethodSemanticsRow(
      MethodSemanticsFlags semanticsFlag,
      uint method,
      uint association
    ) {
      this.SemanticsFlag = semanticsFlag;
      this.Method = method;
      this.Association = association;
    }
  }
  //  0x19
  internal struct MethodImplRow {
    internal readonly uint Class;
    internal readonly uint MethodBody;
    internal readonly uint MethodDeclaration;
    internal MethodImplRow(
      uint @class,
      uint methodBody,
      uint methodDeclaration
    ) {
      this.Class = @class;
      this.MethodBody = methodBody;
      this.MethodDeclaration = methodDeclaration;
    }
  }
  //  0x1A
  internal struct ModuleRefRow {
    internal readonly uint Name;
    internal ModuleRefRow(
      uint name
    ) {
      this.Name = name;
    }
  }
  //  0x1B
  internal struct TypeSpecRow {
#if false
    internal readonly uint Signature;
    internal TypeSpecRow(
      uint signature
    ) {
      this.Signature = signature;
    }
#endif
  }
  //  0x1C
  internal struct ImplMapRow {
    internal readonly PInvokeMapFlags PInvokeMapFlags;
    internal readonly uint MemberForwarded;
    internal readonly uint ImportName;
    internal readonly uint ImportScope;
    internal ImplMapRow(
      PInvokeMapFlags pInvokeMapFlags,
      uint memberForwarded,
      uint importName,
      uint importScope
    ) {
      this.PInvokeMapFlags = pInvokeMapFlags;
      this.MemberForwarded = memberForwarded;
      this.ImportName = importName;
      this.ImportScope = importScope;
    }
  }
  //  0x1D
  internal struct FieldRVARow {
#if false
    internal readonly int RVA;
    internal readonly uint Field;
    internal FieldRVARow(
      int rva,
      uint field
    ) {
      this.RVA = rva;
      this.Field = field;
    }
#endif
  }
  //  0x1E
  internal struct EnCLogRow {
#if false
    internal readonly uint Token;
    internal readonly uint FuncCode;
    internal EnCLogRow(
      uint token,
      uint funcCode
    ) {
      this.Token = token;
      this.FuncCode = funcCode;
    }
#endif
  }
  //  0x1F
  internal struct EnCMapRow {
#if false
    internal readonly uint Token;
    internal EnCMapRow(
      uint token
    ) {
      this.Token = token;
    }
#endif
  }
  //  0x20
  internal struct AssemblyRow {
    internal readonly uint HashAlgId;
    internal readonly ushort MajorVersion;
    internal readonly ushort MinorVersion;
    internal readonly ushort BuildNumber;
    internal readonly ushort RevisionNumber;
    internal readonly AssemblyFlags Flags;
    internal readonly uint PublicKey;
    internal readonly uint Name;
    internal readonly uint Culture;
    internal AssemblyRow(
      uint hashAlgId,
      ushort majorVersion,
      ushort minorVersion,
      ushort buildNumber,
      ushort revisionNumber,
      AssemblyFlags flags,
      uint publicKey,
      uint name,
      uint culture
    ) {
      this.HashAlgId = hashAlgId;
      this.MajorVersion = majorVersion;
      this.MinorVersion = minorVersion;
      this.BuildNumber = buildNumber;
      this.RevisionNumber = revisionNumber;
      this.Flags = flags;
      this.PublicKey = publicKey;
      this.Name = name;
      this.Culture = culture;
    }
  }
  //  0x21
  internal struct AssemblyProcessorRow {
#if false
    internal readonly uint Processor;
    internal AssemblyProcessorRow(
      uint processor
    ) {
      this.Processor = processor;
    }
#endif
  }
  //  0x22
  internal struct AssemblyOSRow {
#if false
    internal readonly uint OSPlatformId;
    internal readonly uint OSMajorVersionId;
    internal readonly uint OSMinorVersionId;
    internal AssemblyOSRow(
      uint osPlatformId,
      uint osMajorVersionId,
      uint osMinorVersionId
    ) {
      this.OSPlatformId = osPlatformId;
      this.OSMajorVersionId = osMajorVersionId;
      this.OSMinorVersionId = osMinorVersionId;
    }
#endif
  }
  //  0x23
  internal struct AssemblyRefRow {
    internal readonly ushort MajorVersion;
    internal readonly ushort MinorVersion;
    internal readonly ushort BuildNumber;
    internal readonly ushort RevisionNumber;
    internal readonly AssemblyFlags Flags;
    internal readonly uint PublicKeyOrToken;
    internal readonly uint Name;
    internal readonly uint Culture;
    internal readonly uint HashValue;
    internal AssemblyRefRow(
      ushort majorVersion,
      ushort minorVersion,
      ushort buildNumber,
      ushort revisionNumber,
      AssemblyFlags flags,
      uint publicKeyOrToken,
      uint name,
      uint culture,
      uint hashValue
    ) {
      this.MajorVersion = majorVersion;
      this.MinorVersion = minorVersion;
      this.BuildNumber = buildNumber;
      this.RevisionNumber = revisionNumber;
      this.Flags = flags;
      this.PublicKeyOrToken = publicKeyOrToken;
      this.Name = name;
      this.Culture = culture;
      this.HashValue = hashValue;
    }
  }
  //  0x24
  internal struct AssemblyRefProcessorRow {
#if false
    internal readonly uint Processor;
    internal readonly uint AssemblyRef;
    internal AssemblyRefProcessorRow(
      uint processor,
      uint assemblyRef
    ) {
      this.Processor = processor;
      this.AssemblyRef = assemblyRef;
    }
#endif
  }
  //  0x25
  internal struct AssemblyRefOSRow {
#if false
    internal readonly uint OSPlatformId;
    internal readonly uint OSMajorVersionId;
    internal readonly uint OSMinorVersionId;
    internal readonly uint AssemblyRef;
    internal AssemblyRefOSRow(
      uint osPlatformId,
      uint osMajorVersionId,
      uint osMinorVersionId,
      uint assemblyRef
    ) {
      this.OSPlatformId = osPlatformId;
      this.OSMajorVersionId = osMajorVersionId;
      this.OSMinorVersionId = osMinorVersionId;
      this.AssemblyRef = assemblyRef;
    }
#endif
  }
  //  0x26
  internal struct FileRow {
    internal readonly FileFlags Flags;
    internal readonly uint Name;
    internal readonly uint HashValue;
    internal FileRow(
      FileFlags flags,
      uint name,
      uint hashValue
    ) {
      this.Flags = flags;
      this.Name = name;
      this.HashValue = hashValue;
    }
  }
  //  0x27
  internal struct ExportedTypeRow {
    internal readonly TypeDefFlags Flags;
    internal readonly uint TypeDefId;
    internal readonly uint TypeName;
    internal readonly uint TypeNamespace;
    internal readonly uint Implementation;
    internal ExportedTypeRow(
      TypeDefFlags typeDefFlags,
      uint TypeDefId,
      uint typeName,
      uint typeNamespace,
      uint implementation
    ) {
      this.Flags = typeDefFlags;
      this.TypeDefId = TypeDefId;
      this.TypeName = typeName;
      this.TypeNamespace = typeNamespace;
      this.Implementation = implementation;
    }
    internal bool IsNested {
      get {
        return (this.Flags & TypeDefFlags.NestedMask) != 0;
      }
    }
  }
  //  0x28
  internal struct ManifestResourceRow {
    internal readonly uint Offset;
    internal readonly ManifestResourceFlags Flags;
    internal readonly uint Name;
    internal readonly uint Implementation;
    internal ManifestResourceRow(
      uint offset,
      ManifestResourceFlags flags,
      uint name,
      uint implementation
    ) {
      this.Offset = offset;
      this.Flags = flags;
      this.Name = name;
      this.Implementation = implementation;
    }
  }
  //  0x29
  internal struct NestedClassRow {
    internal readonly uint NestedClass;
    internal readonly uint EnclosingClass;
    internal NestedClassRow(
      uint nestedClass,
      uint enclosingClass
    ) {
      this.NestedClass = nestedClass;
      this.EnclosingClass = enclosingClass;
    }
  }
  //  0x2A
  internal struct GenericParamRow {
    internal readonly ushort Number;
    internal readonly GenericParamFlags Flags;
    internal readonly uint Owner;
    internal readonly uint Name;
    internal GenericParamRow(
      ushort number,
      GenericParamFlags flags,
      uint owner,
      uint name
    ) {
      this.Number = number;
      this.Flags = flags;
      this.Owner = owner;
      this.Name = name;
    }
  }
  //  0x2B
  internal struct MethodSpecRow {
    internal readonly uint Method;
    internal readonly uint Instantiation;
    internal MethodSpecRow(
      uint method,
      uint instantiation
    ) {
      this.Method = method;
      this.Instantiation = instantiation;
    }
  }
  //  0x2C
  internal struct GenericParamConstraintRow {
#if false
    internal readonly uint Owner;
    internal readonly uint Constraint;
    internal GenericParamConstraintRow(
      uint owner,
      uint constraint
    ) {
      this.Owner = owner;
      this.Constraint = constraint;
    }
#endif
  }

  #endregion  Metadata Stream Specific data


  #region IL Specific data

  internal static class CILMethodFlags {
    internal const byte ILTinyFormat = 0x02;
    internal const byte ILFatFormat = 0x03;
    internal const byte ILFormatMask = 0x03;
    internal const int ILTinyFormatSizeShift = 2;
    internal const byte ILMoreSects = 0x08;
    internal const byte ILInitLocals = 0x10;
    internal const byte ILFatFormatHeaderSize = 0x03;
    internal const int ILFatFormatHeaderSizeShift = 4;

    internal const byte SectEHTable = 0x01;
    internal const byte SectOptILTable = 0x02;
    internal const byte SectFatFormat = 0x40;
    internal const byte SectMoreSects = 0x40;
  }

  internal enum SEHFlags : uint {
    Catch = 0x0000,
    Filter = 0x0001,
    Finally = 0x0002,
    Fault = 0x0004,
  }

  internal struct SEHTableEntry {
    internal readonly SEHFlags SEHFlags;
    internal readonly uint TryOffset;
    internal readonly uint TryLength;
    internal readonly uint HandlerOffset;
    internal readonly uint HandlerLength;
    internal readonly uint ClassTokenOrFilterOffset;
    internal SEHTableEntry(
      SEHFlags sehFlags,
      uint tryOffset,
      uint tryLength,
      uint handlerOffset,
      uint handlerLength,
      uint classTokenOrFilterOffset
    ) {
      this.SEHFlags = sehFlags;
      this.TryOffset = tryOffset;
      this.TryLength = tryLength;
      this.HandlerOffset = handlerOffset;
      this.HandlerLength = handlerLength;
      this.ClassTokenOrFilterOffset = classTokenOrFilterOffset;
    }
  }

  internal sealed class MethodIL {
    internal readonly bool LocalVariablesInited;
    internal readonly ushort MaxStack;
    internal readonly uint LocalSignatureToken;
    internal readonly MemoryBlock EncodedILMemoryBlock;
    internal readonly SEHTableEntry[]/*?*/ SEHTable;
    internal MethodIL(
      bool localVariablesInited,
      ushort maxStack,
      uint localSignatureToken,
      MemoryBlock encodedILMemoryBlock,
      SEHTableEntry[]/*?*/ sehTable
    ) {
      this.LocalVariablesInited = localVariablesInited;
      this.MaxStack = maxStack;
      this.LocalSignatureToken = localSignatureToken;
      this.EncodedILMemoryBlock = encodedILMemoryBlock;
      this.SEHTable = sehTable;
    }
  }

  #endregion IL Specific Data

}
