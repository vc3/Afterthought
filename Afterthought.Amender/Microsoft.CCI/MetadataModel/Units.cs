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
using System.Diagnostics.Contracts;
using System.Text;
using System.Globalization;

namespace Microsoft.Cci {
  /// <summary>
  /// The kind of metadata stored in the module. For example whether the module is an executable or a manifest resource file.
  /// </summary>
  public enum ModuleKind {
    /// <summary>
    /// The module is an executable with an entry point and has a console.
    /// </summary>
    ConsoleApplication,

    /// <summary>
    /// The module is an executable with an entry point and does not have a console.
    /// </summary>
    WindowsApplication,

    /// <summary>
    /// The module is a library of executable code that is dynamically linked into an application and called via the application.
    /// </summary>
    DynamicallyLinkedLibrary,

    /// <summary>
    /// The module contains no executable code. Its contents is a resource stream for the modules that reference it.
    /// </summary>
    ManifestResourceFile,

    /// <summary>
    /// The module is a library of executable code but contains no .NET metadata and is specific to a processor instruction set.
    /// </summary>
    UnmanagedDynamicallyLinkedLibrary
  }

  /// <summary>
  /// Represents a .NET assembly.
  /// </summary>
  [ContractClass(typeof(IAssemblyContract))]
  public interface IAssembly : IModule, IAssemblyReference {

    /// <summary>
    /// A list of objects representing persisted instances of types that extend System.Attribute. Provides an extensible way to associate metadata
    /// with this assembly.
    /// </summary>
    IEnumerable<ICustomAttribute> AssemblyAttributes { get; }

    /// <summary>
    /// Public types defined in other modules making up this assembly and to which other assemblies may refer to via this assembly.
    /// </summary>
    IEnumerable<IAliasForType> ExportedTypes { get; } //TODO: shouldn't these be just namespace type aliases?

    //TODO: introduce a separate collection for forwarded types.

    /// <summary>
    /// A list of the files that constitute the assembly. These are not the source language files that may have been
    /// used to compile the assembly, but the files that contain constituent modules of a multi-module assembly as well
    /// as any external resources. It corresonds to the File table of the .NET assembly file format.
    /// </summary>
    IEnumerable<IFileReference> Files { get; }

    /// <summary>
    /// A set of bits and bit ranges representing properties of the assembly. The value of <see cref="Flags"/> can be set
    /// from source code via the AssemblyFlags assembly custom attribute. The interpretation of the property depends on the target platform.
    /// </summary>
    uint Flags { get; }

    /// <summary>
    /// A list of the modules that constitute the assembly.
    /// </summary>
    IEnumerable<IModule> MemberModules { get; }

    /// <summary>
    /// A list of named byte sequences persisted with the assembly and used during execution, typically via .NET Framework helper classes.
    /// </summary>
    IEnumerable<IResourceReference> Resources { get; }

    /// <summary>
    /// A list of objects representing persisted instances of pairs of security actions and sets of security permissions.
    /// These apply by default to every method reachable from the module.
    /// </summary>
    IEnumerable<ISecurityAttribute> SecurityAttributes { get; }

  }

  [ContractClassFor(typeof(IAssembly))]
  abstract class IAssemblyContract : IAssembly {
    public IEnumerable<ICustomAttribute> AssemblyAttributes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ICustomAttribute>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICustomAttribute>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IAliasForType> ExportedTypes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IAliasForType>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IAliasForType>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IFileReference> Files {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IFileReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IFileReference>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public uint Flags {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IModule> MemberModules {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IModule>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IModule>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<byte> HashValue {
      get {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<byte> PublicKey {
      get {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IResourceReference> Resources {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IResourceReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IResourceReference>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ISecurityAttribute>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ISecurityAttribute>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IAssemblyReference> AssemblyReferences {
      get { throw new NotImplementedException(); }
    }

    public ulong BaseAddress {
      get { throw new NotImplementedException(); }
    }

    public IAssembly ContainingAssembly {
      get { throw new NotImplementedException(); }
    }

    public string DebugInformationLocation {
      get { throw new NotImplementedException(); }
    }

    public string DebugInformationVersion {
      get { throw new NotImplementedException(); }
    }

    public ushort DllCharacteristics {
      get { throw new NotImplementedException(); }
    }

    public IMethodReference EntryPoint {
      get { throw new NotImplementedException(); }
    }

    public uint FileAlignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<string> GetStrings() {
      throw new NotImplementedException();
    }

    public IEnumerable<INamedTypeDefinition> GetAllTypes() {
      throw new NotImplementedException();
    }

    public IEnumerable<IGenericMethodInstanceReference> GetGenericMethodInstances() {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeReference> GetStructuralTypeInstances() {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeMemberReference> GetStructuralTypeInstanceMembers() {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeReference> GetTypeReferences() {
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeMemberReference> GetTypeMemberReferences() {
      throw new NotImplementedException();
    }

    public bool ILOnly {
      get { throw new NotImplementedException(); }
    }

    public bool StrongNameSigned {
      get { throw new NotImplementedException(); }
    }

    public bool Prefers32bits {
      get { throw new NotImplementedException(); }
    }

    public ModuleKind Kind {
      get { throw new NotImplementedException(); }
    }

    public byte LinkerMajorVersion {
      get { throw new NotImplementedException(); }
    }

    public byte LinkerMinorVersion {
      get { throw new NotImplementedException(); }
    }

    public byte MetadataFormatMajorVersion {
      get { throw new NotImplementedException(); }
    }

    public byte MetadataFormatMinorVersion {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> ModuleAttributes {
      get { throw new NotImplementedException(); }
    }

    public IName ModuleName {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IModuleReference> ModuleReferences {
      get { throw new NotImplementedException(); }
    }

    public Guid PersistentIdentifier {
      get { throw new NotImplementedException(); }
    }

    public Machine Machine {
      get { throw new NotImplementedException(); }
    }

    public bool RequiresAmdInstructionSet {
      get { throw new NotImplementedException(); }
    }

    public bool RequiresStartupStub {
      get { throw new NotImplementedException(); }
    }

    public bool Requires32bits {
      get { throw new NotImplementedException(); }
    }

    public bool Requires64bits {
      get { throw new NotImplementedException(); }
    }

    public ulong SizeOfHeapCommit {
      get { throw new NotImplementedException(); }
    }

    public ulong SizeOfHeapReserve {
      get { throw new NotImplementedException(); }
    }

    public ulong SizeOfStackCommit {
      get { throw new NotImplementedException(); }
    }

    public ulong SizeOfStackReserve {
      get { throw new NotImplementedException(); }
    }

    public ushort SubsystemMajorVersion {
      get { throw new NotImplementedException(); }
    }

    public ushort SubsystemMinorVersion {
      get { throw new NotImplementedException(); }
    }

    public string TargetRuntimeVersion {
      get { throw new NotImplementedException(); }
    }

    public bool TrackDebugData {
      get { throw new NotImplementedException(); }
    }

    public bool UsePublicKeyTokensForAssemblyReferences {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IWin32Resource> Win32Resources {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public string Location {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPESection> UninterpretedSections {
      get { throw new NotImplementedException(); }
    }

    public IRootUnitNamespace UnitNamespaceRoot {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IUnitReference> UnitReferences {
      get { throw new NotImplementedException(); }
    }

    public INamespaceDefinition NamespaceRoot {
      get { throw new NotImplementedException(); }
    }

    public IUnit ResolvedUnit {
      get { throw new NotImplementedException(); }
    }

    public UnitIdentity UnitIdentity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    IAssemblyReference IModuleReference.ContainingAssembly {
      get { throw new NotImplementedException(); }
    }

    public IModule ResolvedModule {
      get { throw new NotImplementedException(); }
    }

    public ModuleIdentity ModuleIdentity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IName> Aliases {
      get { throw new NotImplementedException(); }
    }

    public IAssembly ResolvedAssembly {
      get { throw new NotImplementedException(); }
    }

    public string Culture {
      get { throw new NotImplementedException(); }
    }

    public bool IsRetargetable {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<byte> PublicKeyToken {
      get { throw new NotImplementedException(); }
    }

    public Version Version {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity AssemblyIdentity {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity UnifiedAssemblyIdentity {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public bool ContainsForeignTypes {
      get { throw new NotImplementedException(); }
    }
  }

  /// <summary>
  /// A reference to a .NET assembly.
  /// </summary>
  [ContractClass(typeof(IAssemblyReferenceContract))]
  public interface IAssemblyReference : IModuleReference {

    /// <summary>
    /// A list of aliases for the root namespace of the referenced assembly.
    /// </summary>
    IEnumerable<IName> Aliases { get; } //TODO: make this go away, it does not exist in metadata.

    /// <summary>
    /// The referenced assembly, or Dummy.Assembly if the reference cannot be resolved.
    /// </summary>
    IAssembly ResolvedAssembly { get; }

    /// <summary>
    /// Identifies the culture associated with the assembly reference. Typically specified for sattelite assemblies with localized resources.
    /// Empty if not specified.
    /// </summary>
    string Culture { get; }

    /// <summary>
    /// The encrypted SHA1 hash of the persisted form of the referenced assembly.
    /// </summary>
    IEnumerable<byte> HashValue { get; }

    /// <summary>
    /// True if the implementation of the referenced assembly used at runtime is not expected to match the version seen at compile time.
    /// </summary>
    bool IsRetargetable { get; }

    /// <summary>
    /// True if the referenced assembly contains types that describe objects that are neither COM objects nor objects that are managed by the CLR.
    /// Instances of such types are created and managed by another runtime and are accessed by CLR objects via some form of interoperation mechanism.
    /// </summary>
    bool ContainsForeignTypes { get; }

    /// <summary>
    /// The public part of the key used to encrypt the SHA1 hash over the persisted form of the referenced assembly. Empty if not specified.
    /// This value is used by the loader to decrypt an encrypted hash value stored in the assembly, which it then compares with a freshly computed hash value
    /// in order to verify the integrity of the assembly.
    /// </summary>
    IEnumerable<byte> PublicKey { get; }

    /// <summary>
    /// The hashed 8 bytes of the public key of the referenced assembly. This is empty if the referenced assembly does not have a public key.
    /// </summary>
    IEnumerable<byte> PublicKeyToken { get; }

    /// <summary>
    /// The version of the assembly reference.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// The identity of the referenced assembly. Has the same Culture, Name, PublicKeyToken and Version as the reference.
    /// </summary>
    /// <remarks>Also has a location, which may might be empty. Although mostly redundant, the object returned by this
    /// property is useful because it derives from System.Object and therefore can be used as a hash table key. It may be more efficient
    /// to use the properties defined directly on the reference, since the object returned by this property may be allocated lazily
    /// and the allocation can thus be avoided by using the reference's properties.</remarks>
    AssemblyIdentity AssemblyIdentity { get; }

    /// <summary>
    /// Returns the identity of the assembly reference to which this assembly reference has been unified.
    /// </summary>
    /// <remarks>The location might not be set.</remarks>
    AssemblyIdentity UnifiedAssemblyIdentity { get; }
  }

  [ContractClassFor(typeof(IAssemblyReference))]
  abstract class IAssemblyReferenceContract : IAssemblyReference {
    public IEnumerable<IName> Aliases {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IName>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IName>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IAssembly ResolvedAssembly {
      get {
        Contract.Ensures(Contract.Result<IAssembly>() != null);
        throw new NotImplementedException();
      }
    }

    public string Culture {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<byte> HashValue {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<byte>>() != null);
        throw new NotImplementedException();
      }
    }

    public bool IsRetargetable {
      get {
        throw new NotImplementedException();
      }
    }

    public bool ContainsForeignTypes {
      get {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<byte> PublicKey {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<byte>>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<byte> PublicKeyToken {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<byte>>() != null);
        throw new NotImplementedException();
      }
    }

    public Version Version {
      get {
        Contract.Ensures(Contract.Result<Version>() != null);
        throw new NotImplementedException();
      }
    }

    public AssemblyIdentity AssemblyIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    public AssemblyIdentity UnifiedAssemblyIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    public IAssemblyReference ContainingAssembly {
      get { throw new NotImplementedException(); }
    }

    public IModule ResolvedModule {
      get { throw new NotImplementedException(); }
    }

    public ModuleIdentity ModuleIdentity {
      get { throw new NotImplementedException(); }
    }

    public IUnit ResolvedUnit {
      get { throw new NotImplementedException(); }
    }

    public UnitIdentity UnitIdentity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Target CPU types.
  /// </summary>
  public enum Machine : ushort {
    /// <summary>
    /// The target CPU is unknown or not specified.
    /// </summary>
    Unknown = 0x0000,
    /// <summary>
    /// Intel 386.
    /// </summary>
    I386 = 0x014C,
    /// <summary>
    /// MIPS little-endian
    /// </summary>
    R3000 = 0x0162,
    /// <summary>
    /// MIPS little-endian
    /// </summary>
    R4000 = 0x0166,
    /// <summary>
    /// MIPS little-endian
    /// </summary>
    R10000 = 0x0168,
    /// <summary>
    /// MIPS little-endian WCE v2
    /// </summary>
    WCEMIPSV2 = 0x0169,
    /// <summary>
    /// Alpha_AXP
    /// </summary>
    Alpha = 0x0184,
    /// <summary>
    /// SH3 little-endian
    /// </summary>
    SH3 = 0x01a2,
    /// <summary>
    /// SH3 little-endian. DSP.
    /// </summary>
    SH3DSP = 0x01a3,
    /// <summary>
    /// SH3E little-endian.
    /// </summary>
    SH3E = 0x01a4,
    /// <summary>
    /// SH4 little-endian.
    /// </summary>
    SH4 = 0x01a6,
    /// <summary>
    /// SH5.
    /// </summary>
    SH5 = 0x01a8,
    /// <summary>
    /// ARM Little-Endian
    /// </summary>
    ARM = 0x01c0,
    /// <summary>
    /// Thumb.
    /// </summary>
    Thumb = 0x01c2,
    /// <summary>
    /// AM33
    /// </summary>
    AM33 = 0x01d3,
    /// <summary>
    /// IBM PowerPC Little-Endian
    /// </summary>
    PowerPC = 0x01F0,
    /// <summary>
    /// PowerPCFP
    /// </summary>
    PowerPCFP = 0x01f1,
    /// <summary>
    /// Intel 64
    /// </summary>
    IA64 = 0x0200,
    /// <summary>
    /// MIPS
    /// </summary>
    MIPS16 = 0x0266,
    /// <summary>
    /// ALPHA64
    /// </summary>
    Alpha64 = 0x0284,
    /// <summary>
    /// MIPS
    /// </summary>
    MIPSFPU = 0x0366,
    /// <summary>
    /// MIPS
    /// </summary>
    MIPSFPU16 = 0x0466,
    /// <summary>
    /// AXP64
    /// </summary>
    AXP64 = Alpha64,
    /// <summary>
    /// Infineon
    /// </summary>
    Tricore = 0x0520,
    /// <summary>
    /// CEF
    /// </summary>
    CEF = 0x0CEF,
    /// <summary>
    /// EFI Byte Code
    /// </summary>
    EBC = 0x0EBC,
    /// <summary>
    /// AMD64 (K8)
    /// </summary>
    AMD64 = 0x8664,
    /// <summary>
    /// M32R little-endian
    /// </summary>
    M32R = 0x9041,
    /// <summary>
    /// CEE
    /// </summary>
    CEE = 0xC0EE,
  }


  /// <summary>
  /// An object that represents a .NET module.
  /// </summary>
  [ContractClass(typeof(IModuleContract))]
  public interface IModule : IUnit, IModuleReference {

    /// <summary>
    /// A list of the assemblies that are referenced by this module.
    /// </summary>
    IEnumerable<IAssemblyReference> AssemblyReferences { get; }

    /// <summary>
    /// The preferred memory address at which the module is to be loaded at runtime.
    /// </summary>
    ulong BaseAddress {
      get;
      //^ ensures result > uint.MaxValue ==> this.Requires64bits;
    }

    /// <summary>
    /// The Assembly that contains this module. If this module is main module then this returns this.
    /// </summary>
    new IAssembly/*?*/ ContainingAssembly { get; }

    /// <summary>
    /// A path to the debug information corresponding to this module. Can be absolute or relative to the file path of the module. Empty if not specified.
    /// </summary>
    string DebugInformationLocation { get; }

    /// <summary>
    /// A hexadecimal string that is used to store and retrieve the debugging symbols from a symbol store.
    /// </summary>
    string DebugInformationVersion { get; }

    /// <summary>
    /// Flags that control the behavior of the target operating system. CLI implementations are supposed to ignore this, but some operating system pay attention.
    /// </summary>
    ushort DllCharacteristics { get; }

    /// <summary>
    /// The method that will be called to start execution of this executable module. If there is no entry point, the result is Dummy.MethodReference.
    /// </summary>
    IMethodReference EntryPoint { get; }

    /// <summary>
    /// The alignment of sections in the module's image file.
    /// </summary>
    uint FileAlignment { get; }

    /// <summary>
    /// Returns zero or more strings used in the module. If the module is produced by reading in a CLR PE file, then this will be the contents
    /// of the user string heap. If the module is produced some other way, the method may return an empty enumeration or an enumeration that is a
    /// subset of the strings actually used in the module. The main purpose of this method is to provide a way to control the order of strings in a
    /// prefix of the user string heap when writing out a module as a PE file.
    /// </summary>
    IEnumerable<string> GetStrings();

    /// <summary>
    /// Returns all of the types defined in the current module. These are always named types, in other words: INamespaceTypeDefinition or INestedTypeDefinition instances.
    /// </summary>
    IEnumerable<INamedTypeDefinition> GetAllTypes();

    /// <summary>
    /// Returns zero or more generic method instance references used directly or indirectly by the module.
    /// Note that this collection is always empty when the module is produced by reading a CLR PE file.
    /// </summary>
    IEnumerable<IGenericMethodInstanceReference> GetGenericMethodInstances();

    /// <summary>
    /// Returns zero or more structural type instance references used in directly or indirectly by module (arrays, pointers and generic type instances are examples of structural type instances).
    /// Note that this collection is always empty when the module is produced by reading a CLR PE file.
    /// </summary>
    IEnumerable<ITypeReference> GetStructuralTypeInstances();

    /// <summary>
    /// Returns zero or more type members whose containing types are structural type instances used directly or indirectly by the module.
    /// Note that this collection is always empty when the module is produced by reading a CLR PE file.
    /// </summary>
    IEnumerable<ITypeMemberReference> GetStructuralTypeInstanceMembers();

    /// <summary>
    /// Returns zero or more type references used in the module. If the module is produced by reading in a CLR PE file, then this will be the contents
    /// of the type reference table. If the module is produced some other way, the method may return an empty enumeration or an enumeration that is a
    /// subset of the type references actually used in the module. 
    /// </summary>
    IEnumerable<ITypeReference> GetTypeReferences();

    /// <summary>
    /// Returns zero or more type member references used in the module. If the module is produced by reading in a CLR PE file, then this will be the contents
    /// of the member reference table (which only contains entries for fields and methods). If the module is produced some other way, 
    /// the method may return an empty enumeration or an enumeration that is a subset of the member references actually used in the module. 
    /// </summary>
    IEnumerable<ITypeMemberReference> GetTypeMemberReferences();

    /// <summary>
    /// True if the module contains only IL and is processor independent.
    /// </summary>
    bool ILOnly { get; }

    /// <summary>
    /// True if the module contains a hash of its contents, encrypted with the private key of an assembly strong name.
    /// </summary>
    bool StrongNameSigned { get; }

    /// <summary>
    /// The kind of metadata stored in this module. For example whether this module is an executable or a manifest resource file.
    /// </summary>
    ModuleKind Kind { get; }

    /// <summary>
    /// The first part of a two part version number indicating the version of the linker that produced this module. For example, the 8 in 8.0.
    /// </summary>
    byte LinkerMajorVersion { get; }

    /// <summary>
    /// The first part of a two part version number indicating the version of the linker that produced this module. For example, the 0 in 8.0.
    /// </summary>
    byte LinkerMinorVersion { get; }

    /// <summary>
    /// Specifies the target CPU. 
    /// </summary>
    Machine Machine { get; }

    /// <summary>
    /// The first part of a two part version number indicating the version of the format used to persist this module. For example, the 1 in 1.0.
    /// </summary>
    byte MetadataFormatMajorVersion { get; }

    /// <summary>
    /// The second part of a two part version number indicating the version of the format used to persist this module. For example, the 0 in 1.0.
    /// </summary>
    byte MetadataFormatMinorVersion { get; }

    /// <summary>
    /// A list of objects representing persisted instances of types that extend System.Attribute. Provides an extensible way to associate metadata
    /// with this module.
    /// </summary>
    IEnumerable<ICustomAttribute> ModuleAttributes { get; }

    /// <summary>
    /// The name of the module.
    /// </summary>
    IName ModuleName { get; }

    /// <summary>
    /// A list of the modules that are referenced by this module.
    /// </summary>
    IEnumerable<IModuleReference> ModuleReferences { get; }

    /// <summary>
    /// A globally unique persistent identifier for this module.
    /// </summary>
    System.Guid PersistentIdentifier { get; }

    /// <summary>
    /// If set, the module is platform independent but prefers to be loaded in a 32-bit process for performance reasons.
    /// </summary>
    bool Prefers32bits { get; }

    /// <summary>
    /// If set, the module contains instructions or assumptions that are specific to the AMD 64 bit instruction set. Setting this flag to
    /// true also sets Requires64bits to true.
    /// </summary>
    bool RequiresAmdInstructionSet { get; }

    /// <summary>
    /// If set, the module must include a machine code stub that transfers control to the virtual execution system.
    /// </summary>
    bool RequiresStartupStub { get; }

    /// <summary>
    /// If set, the module contains instructions that assume a 32 bit instruction set. For example it may depend on an address being 32 bits.
    /// This may be true even if the module contains only IL instructions because of PlatformInvoke and COM interop.
    /// </summary>
    bool Requires32bits { get; }

    /// <summary>
    /// If set, the module contains instructions that assume a 64 bit instruction set. For example it may depend on an address being 64 bits.
    /// This may be true even if the module contains only IL instructions because of PlatformInvoke and COM interop.
    /// </summary>
    bool Requires64bits { get; }

    /// <summary>
    /// The size of the virtual memory initially committed for the initial process heap.
    /// </summary>
    ulong SizeOfHeapCommit {
      get;
      //^ ensures result > uint.MaxValue ==> this.Requires64bits;
    }

    /// <summary>
    /// The size of the virtual memory to reserve for the initial process heap.
    /// </summary>
    ulong SizeOfHeapReserve {
      get;
      //^ ensures result > uint.MaxValue ==> this.Requires64bits;
    }

    /// <summary>
    /// The size of the virtual memory initially committed for the initial thread's stack.
    /// </summary>
    ulong SizeOfStackCommit {
      get;
      //^ ensures result > uint.MaxValue ==> this.Requires64bits;
    }

    /// <summary>
    /// The size of the virtual memory to reserve for the initial thread's stack.
    /// </summary>
    ulong SizeOfStackReserve {
      get;
      //^ ensures result > uint.MaxValue ==> this.Requires64bits;
    }

    /// <summary>
    /// The first part of a two part version number indicating the operating subsystem that is expected to be the target environment for this module.
    /// </summary>
    ushort SubsystemMajorVersion { get; }

    /// <summary>
    /// The second part of a two part version number indicating the operating subsystem that is expected to be the target environment for this module.
    /// </summary>
    ushort SubsystemMinorVersion { get; }

    /// <summary>
    /// Identifies the version of the CLR that is required to load this module or assembly.
    /// </summary>
    string TargetRuntimeVersion { get; }

    /// <summary>
    /// True if the instructions in this module must be compiled in such a way that the debugging experience is not compromised.
    /// To set the value of this property, add an instance of System.Diagnostics.DebuggableAttribute to the MetadataAttributes list.
    /// </summary>
    bool TrackDebugData { get; }

    /// <summary>
    /// True if the module will be persisted with a list of assembly references that include only tokens derived from the public keys
    /// of the referenced assemblies, rather than with references that include the full public keys of referenced assemblies as well
    /// as hashes over the contents of the referenced assemblies. Setting this property to true is appropriate during development.
    /// When building for deployment it is safer to set this property to false.
    /// </summary>
    //  Issue: Should not this be option to the writer rather than a property of the module?
    bool UsePublicKeyTokensForAssemblyReferences { get; }

    /// <summary>
    /// A list of named byte sequences persisted with the module and used during execution, typically via the Win32 API.
    /// A module will define Win32 resources rather than "managed" resources mainly to present metadata to legacy tools
    /// and not typically use the data in its own code.
    /// </summary>
    IEnumerable<IWin32Resource> Win32Resources { get; }
  }

  #region IModule contract binding
  [ContractClassFor(typeof(IModule))]
  abstract class IModuleContract : IModule {
    #region IModule Members

    public IEnumerable<IAssemblyReference> AssemblyReferences {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IAssemblyReference>>() != null);
        throw new NotImplementedException();
      }
    }

    public ulong BaseAddress {
      get {
        Contract.Ensures(Contract.Result<ulong>() <= uint.MaxValue || this.Requires64bits);
        throw new NotImplementedException();
      }
    }

    public IAssembly ContainingAssembly {
      get { throw new NotImplementedException(); }
    }

    public string DebugInformationLocation {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        throw new NotImplementedException();
      }
    }

    public string DebugInformationVersion {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        throw new NotImplementedException();
      }
    }

    public ushort DllCharacteristics {
      get { throw new NotImplementedException(); }
    }

    public IMethodReference EntryPoint {
      get {
        Contract.Ensures(Contract.Result<IMethodReference>() != null);
        throw new NotImplementedException();
      }
    }

    public uint FileAlignment {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<string> GetStrings() {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<INamedTypeDefinition> GetAllTypes() {
      Contract.Ensures(Contract.Result<IEnumerable<INamedTypeDefinition>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<IGenericMethodInstanceReference> GetGenericMethodInstances() {
      Contract.Ensures(Contract.Result<IEnumerable<IGenericMethodInstanceReference>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeReference> GetStructuralTypeInstances() {
      Contract.Ensures(Contract.Result<IEnumerable<ITypeReference>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeMemberReference> GetStructuralTypeInstanceMembers() {
      Contract.Ensures(Contract.Result<IEnumerable<ITypeMemberReference>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeReference> GetTypeReferences() {
      Contract.Ensures(Contract.Result<IEnumerable<ITypeReference>>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<ITypeMemberReference> GetTypeMemberReferences() {
      Contract.Ensures(Contract.Result<IEnumerable<ITypeMemberReference>>() != null);
      throw new NotImplementedException();
    }

    public bool ILOnly {
      get { throw new NotImplementedException(); }
    }

    public bool StrongNameSigned {
      get { throw new NotImplementedException(); }
    }

    public bool Prefers32bits {
      get { throw new NotImplementedException(); }
    }

    public ModuleKind Kind {
      get { throw new NotImplementedException(); }
    }

    public byte LinkerMajorVersion {
      get { throw new NotImplementedException(); }
    }

    public byte LinkerMinorVersion {
      get { throw new NotImplementedException(); }
    }

    public byte MetadataFormatMajorVersion {
      get { throw new NotImplementedException(); }
    }

    public byte MetadataFormatMinorVersion {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> ModuleAttributes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ICustomAttribute>>() != null);
        throw new NotImplementedException();
      }
    }

    public IName ModuleName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IModuleReference> ModuleReferences {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IModuleReference>>() != null);
        throw new NotImplementedException();
      }
    }

    public Guid PersistentIdentifier {
      get { throw new NotImplementedException(); }
    }

    public Machine Machine {
      get { throw new NotImplementedException(); }
    }

    public bool RequiresAmdInstructionSet {
      get { throw new NotImplementedException(); }
    }

    public bool RequiresStartupStub {
      get { throw new NotImplementedException(); }
    }

    public bool Requires32bits {
      get { throw new NotImplementedException(); }
    }

    public bool Requires64bits {
      get { throw new NotImplementedException(); }
    }

    public ulong SizeOfHeapCommit {
      get {
        Contract.Ensures(Contract.Result<ulong>() <= uint.MaxValue || this.Requires64bits);
        throw new NotImplementedException();
      }
    }

    public ulong SizeOfHeapReserve {
      get {
        Contract.Ensures(Contract.Result<ulong>() <= uint.MaxValue || this.Requires64bits);
        throw new NotImplementedException();
      }
    }

    public ulong SizeOfStackCommit {
      get {
        Contract.Ensures(Contract.Result<ulong>() <= uint.MaxValue || this.Requires64bits);
        throw new NotImplementedException();
      }
    }

    public ulong SizeOfStackReserve {
      get {
        Contract.Ensures(Contract.Result<ulong>() <= uint.MaxValue || this.Requires64bits);
        throw new NotImplementedException();
      }
    }

    public ushort SubsystemMajorVersion {
      get { throw new NotImplementedException(); }
    }

    public ushort SubsystemMinorVersion {
      get { throw new NotImplementedException(); }
    }

    public string TargetRuntimeVersion {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        throw new NotImplementedException();
      }
    }

    public bool TrackDebugData {
      get { throw new NotImplementedException(); }
    }

    public bool UsePublicKeyTokensForAssemblyReferences {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IWin32Resource> Win32Resources {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IWin32Resource>>() != null);
        throw new NotImplementedException();
      }
    }

    #endregion

    #region IUnit Members

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { throw new NotImplementedException(); }
    }

    public IPlatformType PlatformType {
      get { throw new NotImplementedException(); }
    }

    public string Location {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPESection> UninterpretedSections {
      get { throw new NotImplementedException(); }
    }

    public IRootUnitNamespace UnitNamespaceRoot {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IUnitReference> UnitReferences {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamespaceRootOwner Members

    public INamespaceDefinition NamespaceRoot {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IUnitReference Members

    public IUnit ResolvedUnit {
      get { throw new NotImplementedException(); }
    }

    public UnitIdentity UnitIdentity {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IModuleReference Members

    IAssemblyReference IModuleReference.ContainingAssembly {
      get { throw new NotImplementedException(); }
    }

    public IModule ResolvedModule {
      get { throw new NotImplementedException(); }
    }

    public ModuleIdentity ModuleIdentity {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A reference to a .NET module.
  /// </summary>
  [ContractClass(typeof(IModuleReferenceContract))]
  public interface IModuleReference : IUnitReference {

    /// <summary>
    /// The Assembly that contains this module. May be null if the module is not part of an assembly.
    /// </summary>
    IAssemblyReference/*?*/ ContainingAssembly { get; }

    /// <summary>
    /// The referenced module, or Dummy.Module if the reference cannot be resolved.
    /// </summary>
    IModule ResolvedModule { get; }

    /// <summary>
    /// The identity of the referenced module.
    /// </summary>
    /// <remarks>The location might not be set.</remarks>
    ModuleIdentity ModuleIdentity { get; }

  }

  #region IModuleReference contract binding
  [ContractClassFor(typeof(IModuleReference))]
  abstract class IModuleReferenceContract : IModuleReference {

    #region IModuleReference Members

    public IAssemblyReference ContainingAssembly {
      get { throw new NotImplementedException(); }
    }

    public IModule ResolvedModule {
      get {
        Contract.Ensures(Contract.Result<IModule>() != null);
        throw new NotImplementedException(); 
      }
    }

    public ModuleIdentity ModuleIdentity {
      get {
        Contract.Ensures(Contract.Result<ModuleIdentity>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion

    #region IUnitReference Members

    public IUnit ResolvedUnit {
      get { throw new NotImplementedException(); }
    }

    public UnitIdentity UnitIdentity {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A unit of metadata stored as a single artifact and potentially produced and revised independently from other units.
  /// Examples of units include .NET assemblies and modules, as well C++ object files and compiled headers.
  /// </summary>
  [ContractClass(typeof(IUnitContract))]
  public interface IUnit : INamespaceRootOwner, IUnitReference, IDefinition {

    /// <summary>
    /// The identity of the assembly corresponding to the target platform contract assembly at the time this unit was compiled.
    /// This property will be used to implement IMetadataHost.ContractAssemblySymbolicIdentity and its implementation must
    /// consequently not use the latter.
    /// </summary>
    AssemblyIdentity ContractAssemblySymbolicIdentity { get; }

    /// <summary>
    /// The identity of the assembly corresponding to the target platform core assembly at the time this unit was compiled.
    /// This property will be used to implement IMetadataHost.CoreAssemblySymbolicIdentity and its implementation must
    /// consequently not use the latter.
    /// </summary>
    AssemblyIdentity CoreAssemblySymbolicIdentity { get; }

    /// <summary>
    /// A collection of well known types that must be part of every target platform and that are fundamental to modeling compiled code.
    /// The types are obtained by querying the unit set of the compilation and thus can include types that are defined by the compilation itself.
    /// </summary>
    IPlatformType PlatformType { get; }

    /// <summary>
    /// An indication of the location where the unit is or will be stored. This need not be a file system path and may be empty. 
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to this unit.
    /// </summary>
    string Location { get; }

    /// <summary>
    /// A sequence of PE sections that are not well known to PE readers and thus have not been decompiled into 
    /// other parts of the Metadata Model. These sections may have meaning to other tools. 
    /// </summary>
    IEnumerable<IPESection> UninterpretedSections { get; }

    /// <summary>
    /// A root namespace that contains nested namespaces as well as top level types and anything else that implements INamespaceMember.
    /// </summary>
    IRootUnitNamespace UnitNamespaceRoot { get; }

    /// <summary>
    /// A list of other units that are referenced by this unit. 
    /// </summary>
    IEnumerable<IUnitReference> UnitReferences { get; }

  }

  [ContractClassFor(typeof(IUnit))]
  abstract class IUnitContract : IUnit {
    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    public IPlatformType PlatformType {
      get {
        Contract.Ensures(Contract.Result<IPlatformType>() != null);
        throw new NotImplementedException();
      }
    }

    public string Location {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IPESection> UninterpretedSections {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IPESection>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IPESection>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IRootUnitNamespace UnitNamespaceRoot {
      get {
        Contract.Ensures(Contract.Result<IRootUnitNamespace>() != null);
        throw new NotImplementedException();
      }
    }

    public IEnumerable<IUnitReference> UnitReferences {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IUnitReference>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IUnitReference>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public INamespaceDefinition NamespaceRoot {
      get { throw new NotImplementedException(); }
    }

    public IUnit ResolvedUnit {
      get { throw new NotImplementedException(); }
    }

    public UnitIdentity UnitIdentity {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// A reference to a instance of <see cref="IUnit"/>.
  /// </summary>
  [ContractClass(typeof(IUnitReferenceContract))]
  public interface IUnitReference : IReference, INamedEntity {

    /// <summary>
    /// The referenced unit, or Dummy.Unit if the reference cannot be resolved.
    /// </summary>
    IUnit ResolvedUnit { get; }

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    /// <remarks>The location might not be set.</remarks>
    UnitIdentity UnitIdentity { get; }
  }

  #region IUnitReference contract binding
  [ContractClassFor(typeof(IUnitReference))]
  abstract class IUnitReferenceContract : IUnitReference {
    #region IUnitReference Members

    public IUnit ResolvedUnit {
      get {
        Contract.Ensures(Contract.Result<IUnit>() != null);
        throw new NotImplementedException();
      }
    }

    public UnitIdentity UnitIdentity {
      get {
        Contract.Ensures(Contract.Result<UnitIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { throw new NotImplementedException(); }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      throw new NotImplementedException();
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region INamedEntity Members

    public IName Name {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A set of units that all contribute to a unified root namespace. For example the set of assemblies referenced by a C# project.
  /// </summary>
  public interface IUnitSet : INamespaceRootOwner {

    /// <summary>
    /// Determines if the given unit belongs to this set of units.
    /// </summary>
    bool Contains(IUnit unit);
    // ^ ensures result == exists{IUnit u in this.Units; u == unit};

    /// <summary>
    /// Enumerates the units making up this set of units.
    /// </summary>
    IEnumerable<IUnit> Units {
      get;
      // ^ ensures forall{IUnit unit in result; exists unique{IUnit u in result; u == unit}};
    }

    /// <summary>
    /// A unified root namespace for this set of units. It contains nested namespaces as well as top level types and anything else that implements INamespaceMember.
    /// </summary>
    IUnitSetNamespace UnitSetNamespaceRoot {
      get;
      //^ ensures result.UnitSet == this;
    }

  }

  /// <summary>
  /// An object containing information that identifies a unit of metadata such as an assembly or a module.
  /// </summary>
  public abstract class UnitIdentity {

    /// <summary>
    /// Allocates an object that identifies a unit of metadata. Can be just the name of the module, but can also include the location where the module is stored.
    /// </summary>
    /// <param name="name">The name of the identified unit.</param>
    /// <param name="location">The location where the unit is stored. Can be the empty string if the location is not known. The location need not be a file path.</param>
    internal UnitIdentity(IName name, string location) {
      Contract.Requires(name != null);
      Contract.Requires(location != null);
      this.name = name;
      this.location = location;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.name != null);
      Contract.Invariant(this.location != null);
    }

    /// <summary>
    /// Returns true if the given object is an identifier that identifies the same object as this identifier.
    /// </summary>
    public abstract override bool Equals(object/*?*/ obj);

    /// <summary>
    /// Computes a hashcode based on the information in the identifier.
    /// </summary>
    internal abstract int ComputeHashCode();

    /// <summary>
    /// Returns a hashcode based on the information in the identifier.
    /// </summary>
    public override int GetHashCode() {
      if (this.hashCode == null)
        this.hashCode = this.ComputeHashCode();
      return (int)this.hashCode;
    }
    int? hashCode = null;


    /// <summary>
    /// An indication of the location where the unit is or will be stored. Can be the empty string if the location is not known. This need not be a file system path. 
    /// </summary>
    public string Location {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        return this.location;
      }
    }
    readonly string location;

    /// <summary>
    /// The name of the unit being identified.
    /// </summary>
    public IName Name {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        return this.name;
      }
    }
    readonly IName name;

    /// <summary>
    /// Returns a string that contains the information in the identifier.
    /// </summary>
    public abstract override string ToString();
  }

  /// <summary>
  /// An object that identifies a .NET assembly, using its name, culture, version, public key token, and location.
  /// </summary>
  public sealed class AssemblyIdentity : ModuleIdentity {

    /// <summary>
    /// Allocates an object that identifies a .NET assembly, using its name, culture, version, public key token, and location.
    /// </summary>
    /// <param name="name">The name of the identified assembly.</param>
    /// <param name="culture">Identifies the culture associated with the identified assembly. Typically used to identify sattelite assemblies with localized resources. 
    /// If the assembly is culture neutral, an empty string should be supplied as argument.</param>
    /// <param name="version">The version of the identified assembly.</param>
    /// <param name="publicKeyToken">The public part of the key used to sign the referenced assembly. May be empty if the identified assembly is not signed.</param>
    /// <param name="location">The location where the assembly is stored. Can be the empty string if the location is not known. The location need not be a file path.</param>
    public AssemblyIdentity(IName name, string culture, Version version, IEnumerable<byte> publicKeyToken, string location)
      : base(name, location) {
      Contract.Requires(name != null);
      Contract.Requires(culture != null);
      Contract.Requires(version != null);
      Contract.Requires(publicKeyToken != null);
      Contract.Requires(location != null);
      this.culture = culture;
      this.version = version;
      this.publicKeyToken = publicKeyToken;
    }

    /// <summary>
    /// Allocates an object that identifies a .NET assembly, using its name, culture, version, public key token, and location.
    /// </summary>
    /// <param name="template">An assembly identity to use a template for the new identity.</param>
    /// <param name="location">A location that should replace the location from the template.</param>
    public AssemblyIdentity(AssemblyIdentity template, string location)
      : base(template.Name, location) {
      Contract.Requires(template != null);
      Contract.Requires(location != null);
      this.culture = template.Culture;
      this.version = template.Version;
      this.publicKeyToken = template.PublicKeyToken;
    }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(this.culture != null);
      Contract.Invariant(this.publicKeyToken != null);
      Contract.Invariant(this.version != null);
    }


    /// <summary>
    /// The identity of the assembly to which the identified module belongs. May be null in the case of a module that is not part of an assembly.
    /// </summary>
    public override AssemblyIdentity/*?*/ ContainingAssembly {
      get {
        return this;
      }
    }

    /// <summary>
    /// Identifies the culture associated with the identified assembly. Typically used to identify sattelite assemblies with localized resources.
    /// Empty if not specified.
    /// </summary>
    public string Culture {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        return this.culture;
      }
    }
    readonly string culture;

    /// <summary>
    /// Returns true if the given object is an identifier that identifies the same object as this identifier.
    /// </summary>
    //^ [Confined]
    public sealed override bool Equals(object/*?*/ obj) {
      if (obj == (object)this) return true;
      AssemblyIdentity/*?*/ otherAssembly = obj as AssemblyIdentity;
      if (otherAssembly == null) return false;
      if (this.Name.UniqueKeyIgnoringCase != otherAssembly.Name.UniqueKeyIgnoringCase) return false;
      if (this.Version != otherAssembly.Version) return false;
      if (string.Compare(this.Culture, otherAssembly.Culture, StringComparison.OrdinalIgnoreCase) != 0) return false;
      if (IteratorHelper.EnumerableIsNotEmpty(this.PublicKeyToken))
        return IteratorHelper.EnumerablesAreEqual(this.PublicKeyToken, otherAssembly.PublicKeyToken);
      else {
        // This can be dangerous! Returning true here means that weakly named assemblies are assumed to be the
        // same just because their name is the same. So two assemblies from different locations but the same name
        // should *NOT* be allowed.
        return true;
      }
    }

    /// <summary>
    /// Computes a hashcode from the name, version, culture and public key token of the assembly identifier.
    /// </summary>
    internal sealed override int ComputeHashCode() {
      int hash = this.Name.UniqueKeyIgnoringCase;
      hash = (hash << 8) ^ (this.version.Major << 6) ^ (this.version.Minor << 4) ^ (this.version.MajorRevision << 2) ^ this.version.MinorRevision;
      if (this.Culture.Length > 0)
        hash = (hash << 4) ^ ObjectModelHelper.CaseInsensitiveStringHash(this.Culture);
      foreach (byte b in this.PublicKeyToken)
        hash = (hash << 1) ^ b;
      return hash;
    }

    /// <summary>
    /// Returns a hashcode based on the information in the assembly identity.
    /// </summary>
    public sealed override int GetHashCode() {
      return base.GetHashCode();
    }

    /// <summary>
    /// The public part of the key used to sign the referenced assembly. Empty if not specified.
    /// </summary>
    public IEnumerable<byte> PublicKeyToken {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<byte>>() != null);
        return this.publicKeyToken;
      }
    }
    readonly IEnumerable<byte> publicKeyToken;

    /// <summary>
    /// Returns a string that contains the information in the identifier.
    /// </summary>
    //^ [Confined]
    public sealed override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append("Assembly(Name=");
      sb.Append(this.Name.Value);
      sb.AppendFormat(CultureInfo.InvariantCulture, ", Version={0}.{1}.{2}.{3}", this.Version.Major, this.Version.Minor, this.Version.Build, this.Version.Revision);
      if (this.Culture.Length > 0)
        sb.AppendFormat(CultureInfo.InvariantCulture, ", Culture={0}", this.Culture);
      else
        sb.Append(", Culture=neutral");
      StringBuilder tokStr = new StringBuilder();
      foreach (byte b in this.PublicKeyToken)
        tokStr.Append(b.ToString("x2", null));
      if (tokStr.Length == 0) {
        sb.Append(", PublicKeyToken=null");
        if (this.Location.Length > 0)
          sb.AppendFormat(CultureInfo.InvariantCulture, ", Location={0}", this.Location);
      } else
        sb.AppendFormat(CultureInfo.InvariantCulture, ", PublicKeyToken={0}", tokStr.ToString());
      sb.Append(")");
      return sb.ToString();
    }

    /// <summary>
    /// The version of the identified assembly.
    /// </summary>
    public Version Version {
      get {
        Contract.Ensures(Contract.Result<Version>() != null);
        return this.version;
      }
    }
    Version version;
  }

  /// <summary>
  /// An object that identifies a .NET module. Can be just the name of the module, but can also include the location where the module is stored.
  /// If the module forms part of an assembly, the identifier of the assembly is also included.
  /// </summary>
  public class ModuleIdentity : UnitIdentity {

    /// <summary>
    /// Allocates an object that identifies a .NET module. Can be just the name of the module, but can also include the location where the module is stored.
    /// </summary>
    /// <param name="name">The name of the identified module.</param>
    /// <param name="location">The location where the module is stored. Can be the empty string if the location is not known. The location need not be a file path.</param>
    public ModuleIdentity(IName name, string location)
      : base(name, location) {
      Contract.Requires(name != null);
      Contract.Requires(location != null);
    }

    /// <summary>
    /// Allocates an object that identifies a .NET module that forms part of an assembly.
    /// Can be just the name of the module along with the identifier of the assembly, but can also include the location where the module is stored.
    /// </summary>
    /// <param name="name">The name of the identified module.</param>
    /// <param name="location">The location where the module is stored. Can be the empty string if the location is not known. The location need not be a file path.</param>
    /// <param name="containingAssembly">The identifier of the assembly to which the identified module belongs. May be null.</param>
    public ModuleIdentity(IName name, string location, AssemblyIdentity/*?*/ containingAssembly)
      : base(name, location) {
      Contract.Requires(name != null);
      Contract.Requires(location != null);
      this.containingAssembly = containingAssembly;
    }

    /// <summary>
    /// The identity of the assembly to which the identified module belongs. May be null in the case of a module that is not part of an assembly.
    /// </summary>
    public virtual AssemblyIdentity/*?*/ ContainingAssembly {
      get { return this.containingAssembly; }
    }
    readonly AssemblyIdentity/*?*/ containingAssembly;

    /// <summary>
    /// Returns true if the given object is an identifier that identifies the same object as this identifier.
    /// </summary>
    //^ [Confined]
    public override bool Equals(object/*?*/ obj) {
      if (obj == (object)this) return true;
      ModuleIdentity/*?*/ otherMod = obj as ModuleIdentity;
      if (otherMod == null) return false;
      if (this.containingAssembly == null) {
        if (otherMod.ContainingAssembly != null) return false;
      } else {
        if (otherMod.ContainingAssembly == null) return false;
        if (!this.containingAssembly.Equals(otherMod.containingAssembly)) return false;
      }
      if (this.Name.UniqueKeyIgnoringCase != otherMod.Name.UniqueKeyIgnoringCase) return false;
      if (this.containingAssembly != null) return true;
      return string.Compare(this.Location, otherMod.Location, StringComparison.OrdinalIgnoreCase) == 0;
    }

    /// <summary>
    /// Computes a hashcode from the name of the modules and the containing assembly (if applicable) or the location (if specified).
    /// </summary>
    internal override int ComputeHashCode() {
      int hash = this.Name.UniqueKeyIgnoringCase;
      if (this.ContainingAssembly != null)
        hash = (hash << 4) ^ this.ContainingAssembly.GetHashCode();
      else if (this.Location.Length > 0)
        hash = (hash << 4) ^ ObjectModelHelper.CaseInsensitiveStringHash(this.Location);
      return hash;
    }

    /// <summary>
    /// Returns a hashcode based on the information in the module identity.
    /// </summary>
    public override int GetHashCode() {
      return base.GetHashCode();
    }

    /// <summary>
    /// Returns a string that contains the information in the identifier.
    /// </summary>
    //^ [Confined]
    public override string ToString() {
      if (this.ContainingAssembly == null)
        return "Module(Location=\"" + this.Location + "\" Name=" + this.Name.Value + ")";
      else
        return "Module(Name=" + this.Name.Value + " ContainingAssembly=" + this.ContainingAssembly.ToString() + ")";
    }
  }

  /// <summary>
  /// An object containing information that identifies a set of metadata units.
  /// </summary>
  public sealed class UnitSetIdentity {

    /// <summary>
    /// Allocates an object containing information that identifies a set of metadata units.
    /// </summary>
    /// <param name="units">An enumeration of identifiers of the units making up the identified set of units.</param>
    public UnitSetIdentity(IEnumerable<UnitIdentity> units) {
      Contract.Requires(units != null);

      this.units = units;
    }

    /// <summary>
    /// Enumerates the identifiers of the units making up the identified set of units.
    /// </summary>
    public IEnumerable<UnitIdentity> Units {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<UnitIdentity>>() != null);
        return this.units; 
      }
    }
    readonly IEnumerable<UnitIdentity> units;

  }

}
