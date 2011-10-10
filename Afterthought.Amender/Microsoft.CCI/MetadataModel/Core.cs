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
  /// Supplies information about errors discovered by a particular error reporter in a particular source location.
  /// The event it describes may occur at arbitrary times on arbitrary threads.
  /// </summary>
  public class ErrorEventArgs : EventArgs {

    /// <summary>
    /// Constructs an object that supplies information about the presence or absence of errors discovered by a particular error reporter in a particular location.
    /// The event it describes may occur at arbitrary times on arbitrary threads.
    /// </summary>
    /// <param name="errorReporter">
    /// The object reporting the errors. This can be used to filter out events coming from non interesting sources.
    /// For example, all top level syntax errors will be reported by an object that implements ISymbolSyntaxErrorsReporter.
    /// A listener that is only interested to find out if the symbol table is derived from a syntactically correct source
    /// can ignore any events that come from a reporter that does not implement this interface.
    /// </param>
    /// <param name="location">
    /// Identifies the portion of the document that was analyzed to arrive at the error list.
    /// </param>
    /// <param name="errors">
    /// A possibly empty collection of errors found by ErrorReporter in Location. Any errors previously found by the same reporter
    /// in the same location should be replaced with this collection.
    /// </param>
    public ErrorEventArgs(object errorReporter, ILocation location, IEnumerable<IErrorMessage> errors) {
      this.errorReporter = errorReporter;
      this.location = location;
      this.errors = errors;
    }

    /// <summary>
    /// The object reporting the errors. This can be used to filter out events coming from non interesting sources.
    /// For example, all top level syntax errors will be reported by an object that implements ISymbolSyntaxErrorsReporter.
    /// A listener that is only interested to find out if the symbol table is derived from a syntactically correct source
    /// can ignore any events that come from a reporter that does not implement this interface.
    /// </summary>
    public object ErrorReporter {
      get { return this.errorReporter; }
    }
    readonly object errorReporter;

    /// <summary>
    /// Identifies the portion of the document that was analyzed to arrive at the error list.
    /// </summary>
    public ILocation Location {
      get { return this.location; }
    }
    readonly ILocation location;

    /// <summary>
    /// A possibly empty collection of errors found by ErrorReporter in SourceLocation. Any errors previously found by the same reporter
    /// in the same location should be replaced with this collection.
    /// </summary>
    public IEnumerable<IErrorMessage> Errors {
      get { return this.errors; }
    }
    readonly IEnumerable<IErrorMessage> errors;
  }

  /// <summary>
  /// An object that represents a binary document, such as dll, compiled resources.
  /// </summary>
  public interface IBinaryDocument : IDocument {
    /// <summary>
    /// The length of the Binary Document.
    /// </summary>
    uint Length { get; }
  }

  /// <summary>
  /// Provides efficient readonly access to the content of an IBinaryDocument instance via an unsafe byte pointer.
  /// </summary>
  public unsafe interface IBinaryDocumentMemoryBlock {
    /// <summary>
    /// The binary document for which this is the memory block
    /// </summary>
    IBinaryDocument BinaryDocument { get; }

    /// <summary>
    /// The pointer to the start of Memory block
    /// </summary>
    byte* Pointer { get; }

    /// <summary>
    /// Length of the memory block
    /// </summary>
    uint Length { get; }
  }

  /// <summary>
  /// Represents the location in binary document.
  /// </summary>
  public interface IBinaryLocation : ILocation {
    /// <summary>
    /// The binary document containing this location range.
    /// </summary>
    IBinaryDocument BinaryDocument {
      get;
    }

    /// <summary>
    /// The offset of the location.
    /// </summary>
    uint Offset {
      get;
      //^ ensures result <= this.BinaryDocument.Length;
    }
  }

  /// <summary>
  /// Provides a standard abstraction over the applications that host components that provide or consume objects from the metadata model.
  /// </summary>
  [ContractClass(typeof(IMetadataHostContract))]
  public interface IMetadataHost {

    /// <summary>
    /// The errors reported by this event are discovered in background threads by an open ended
    /// set of error reporters. Listeners to this event should thus be prepared to be called at arbitrary times from arbitrary threads.
    /// Each occurrence of the event concerns a particular error location and a particular error reporter.
    /// The reported error collection (possibly empty) supercedes any errors previously reported by the same error reporter for the same location.
    /// A location can be an entire IDocument, or just a part of it (the latter would apply, for example, to syntax errors discovered by an incremental
    /// parser after an edit to the source document).
    /// </summary>
    event EventHandler<ErrorEventArgs> Errors;

    /// <summary>
    /// The identity of the assembly containing Microsoft.Contracts.Contract.
    /// </summary>
    AssemblyIdentity ContractAssemblySymbolicIdentity { get; }

    /// <summary>
    /// The identity of the assembly containing the core system types such as System.Object.
    /// </summary>
    AssemblyIdentity CoreAssemblySymbolicIdentity { get; }

    /// <summary>
    /// The identity of the System.Core assembly.
    /// </summary>
    AssemblyIdentity SystemCoreAssemblySymbolicIdentity { get; }

    /// <summary>
    /// Finds the assembly that matches the given identifier among the already loaded set of assemblies,
    /// or a dummy assembly if no matching assembly can be found.
    /// </summary>
    IAssembly FindAssembly(AssemblyIdentity assemblyIdentity);

    /// <summary>
    /// Finds the module that matches the given identifier among the already loaded set of modules,
    /// or a dummy module if no matching module can be found.
    /// </summary>
    IModule FindModule(ModuleIdentity moduleIdentity);

    /// <summary>
    /// Finds the unit that matches the given identifier among the already loaded set of units,
    /// or a dummy unit if no matching unit can be found.
    /// </summary>
    IUnit FindUnit(UnitIdentity unitIdentity);

    /// <summary>
    /// A collection of methods that associate unique integers with metadata model entities.
    /// The association is based on the identities of the entities and the factory does not retain
    /// references to the given metadata model objects.
    /// </summary>
    IInternFactory InternFactory { get; }

    /// <summary>
    /// A collection of references to types from the core platform, such as System.Object and System.String.
    /// </summary>
    IPlatformType PlatformType { get; }

    /// <summary>
    /// The assembly that matches the given identifier, or a dummy assembly if no matching assembly can be found.
    /// </summary>
    IAssembly LoadAssembly(AssemblyIdentity assemblyIdentity);

    /// <summary>
    /// The module that matches the given identifier, or a dummy module if no matching module can be found.
    /// </summary>
    IModule LoadModule(ModuleIdentity moduleIdentity);

    /// <summary>
    /// The unit that matches the given identifier, or a dummy unit if no matching unit can be found.
    /// </summary>
    IUnit LoadUnit(UnitIdentity unitIdentity);

    /// <summary>
    /// Returns the unit that is stored at the given location, or a dummy unit if no unit exists at that location or if the unit at that location is not accessible.
    /// </summary>
    IUnit LoadUnitFrom(string location);

    /// <summary>
    /// Returns enumeration of all the units loaded so far.
    /// </summary>
    IEnumerable<IUnit> LoadedUnits {
      get;
    }

    /// <summary>
    /// A table used to intern strings used as names.
    /// </summary>
    INameTable NameTable { get; }

    /// <summary>
    /// The size (in bytes) of a pointer on the platform on which the host is targetting.
    /// The value of this property is either 4 (32-bits) or 8 (64-bit).
    /// </summary>
    byte PointerSize {
      get;
      //^ ensures result == 4 || result == 8;
    }

    /// <summary>
    /// Raises the CompilationErrors event with the given error event arguments.
    /// </summary>
    void ReportErrors(ErrorEventArgs errorEventArguments);

    /// <summary>
    /// Raises the CompilationErrors event with the given error wrapped up in an error event arguments object.
    /// </summary>
    /// <param name="error">The error to report.</param>
    void ReportError(IErrorMessage error);

    /// <summary>
    /// Given the identity of a referenced assembly (but not its location), apply host specific policies for finding the location
    /// of the referenced assembly.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the assembly. It will have been loaded from somewhere and thus
    /// has a known location, which will typically be probed for the referenced assembly.</param>
    /// <param name="referencedAssembly">The assembly being referenced. This will not have a location since there is no point in probing
    /// for the location of an assembly when you already know its location.</param>
    /// <returns>An assembly identity that matches the given referenced assembly identity, but which includes a location.
    /// If the probe failed to find the location of the referenced assembly, the location will be "unknown://location".</returns>
    [Pure]
    AssemblyIdentity ProbeAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly);

    /// <summary>
    /// Given the identity of a referenced module (but not its location), apply host specific policies for finding the location
    /// of the referenced module.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the module. It will have been loaded from somewhere and thus
    /// has a known location, which will typically be probed for the referenced module.</param>
    /// <param name="referencedModule">Module being referenced.</param>
    /// <returns>A module identity that matches the given referenced module identity, but which includes a location.
    /// If the probe failed to find the location of the referenced assembly, the location will be "unknown://location".</returns>
    [Pure]
    ModuleIdentity ProbeModuleReference(IUnit referringUnit, ModuleIdentity referencedModule);

    /// <summary>
    /// Returns an assembly identifier of an assembly that is the same, or a later (compatible) version of the given assembly identity.
    /// </summary>
    /// <param name="assemblyIdentity">The identity of the assembly that needs to be unified.</param>
    /// <returns>The identity of the unified assembly.</returns>
    /// <remarks>If an assembly A references assembly B as well as version 2 of assembly C, and assembly B references version 1 of assembly C then any
    /// reference to type C.T that is obtained from assembly A will resolve to a different type definition from a reference to type C.T that is obtained
    /// from assembly B, unless the host declares that any reference to version 1 of assembly should be treated as if it were a reference to 
    /// version 2 of assembly C. This call is how the host gets to make this declaration.
    /// </remarks>
    [Pure]
    AssemblyIdentity UnifyAssembly(AssemblyIdentity assemblyIdentity);

    /// <summary>
    /// Returns an assembly identifier of an assembly that is the same, or a later (compatible) version of the given referenced assembly.
    /// </summary>
    /// <param name="assemblyReference">A reference to the assembly that needs to be unified.</param>
    /// <returns>The identity of the unified assembly.</returns>
    /// <remarks>If an assembly A references assembly B as well as version 2 of assembly C, and assembly B references version 1 of assembly C then any
    /// reference to type C.T that is obtained from assembly A will resolve to a different type definition from a reference to type C.T that is obtained
    /// from assembly B, unless the host declares that any reference to version 1 of assembly should be treated as if it were a reference to 
    /// version 2 of assembly C. This call is how the host gets to make this declaration.
    /// </remarks>
    [Pure]
    AssemblyIdentity UnifyAssembly(IAssemblyReference assemblyReference);

    /// <summary>
    /// True if IL locations should be preserved up into the code model by decompilers using this host.
    /// </summary>
    bool PreserveILLocations { get; }
  }

  [ContractClassFor(typeof(IMetadataHost))]
  abstract class IMetadataHostContract : IMetadataHost {
    public event EventHandler<ErrorEventArgs> Errors;

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        if (this.Errors == null)
          throw new NotImplementedException();
        else
          throw new NotImplementedException();
      }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    public AssemblyIdentity SystemCoreAssemblySymbolicIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        throw new NotImplementedException();
      }
    }

    public IAssembly FindAssembly(AssemblyIdentity assemblyIdentity) {
      Contract.Requires(assemblyIdentity != null);
      Contract.Ensures(Contract.Result<IAssembly>() != null);
      throw new NotImplementedException();
    }

    public IModule FindModule(ModuleIdentity moduleIdentity) {
      Contract.Requires(moduleIdentity != null);
      Contract.Ensures(Contract.Result<IModule>() != null);
      throw new NotImplementedException();
    }

    public IUnit FindUnit(UnitIdentity unitIdentity) {
      Contract.Requires(unitIdentity != null);
      Contract.Ensures(Contract.Result<IUnit>() != null);
      throw new NotImplementedException();
    }

    public IInternFactory InternFactory {
      get {
        Contract.Ensures(Contract.Result<IInternFactory>() != null);
        throw new NotImplementedException();
      }
    }

    public IPlatformType PlatformType {
      get {
        Contract.Ensures(Contract.Result<IPlatformType>() != null);
        throw new NotImplementedException();
      }
    }

    public IAssembly LoadAssembly(AssemblyIdentity assemblyIdentity) {
      Contract.Requires(assemblyIdentity != null);
      Contract.Ensures(Contract.Result<IAssembly>() != null);
      throw new NotImplementedException();
    }

    public IModule LoadModule(ModuleIdentity moduleIdentity) {
      Contract.Requires(moduleIdentity != null);
      Contract.Ensures(Contract.Result<IModule>() != null);
      throw new NotImplementedException();
    }

    public IUnit LoadUnit(UnitIdentity unitIdentity) {
      Contract.Requires(unitIdentity != null);
      Contract.Ensures(Contract.Result<IUnit>() != null);
      throw new NotImplementedException();
    }

    public IUnit LoadUnitFrom(string location) {
      Contract.Requires(location != null);
      Contract.Ensures(Contract.Result<IUnit>() != null);
      throw new NotImplementedException();
    }

    public IEnumerable<IUnit> LoadedUnits {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IUnit>>() != null);
        throw new NotImplementedException();
      }
    }

    public INameTable NameTable {
      get {
        Contract.Ensures(Contract.Result<INameTable>() != null);
        throw new NotImplementedException();
      }
    }

    public byte PointerSize {
      get {
        Contract.Ensures(Contract.Result<byte>() == 4 || Contract.Result<byte>() == 8);
        throw new NotImplementedException();
      }
    }

    public void ReportErrors(ErrorEventArgs errorEventArguments) {
      Contract.Requires(errorEventArguments != null);
      throw new NotImplementedException();
    }

    public void ReportError(IErrorMessage error) {
      Contract.Requires(error != null);
      throw new NotImplementedException();
    }

    public AssemblyIdentity ProbeAssemblyReference(IUnit referringUnit, AssemblyIdentity referencedAssembly) {
      Contract.Requires(referringUnit != null);
      Contract.Requires(referencedAssembly != null);
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
      throw new NotImplementedException();
    }

    public ModuleIdentity ProbeModuleReference(IUnit referringUnit, ModuleIdentity referencedModule) {
      Contract.Requires(referringUnit != null);
      Contract.Requires(referencedModule != null);
      Contract.Ensures(Contract.Result<ModuleIdentity>() != null);
      throw new NotImplementedException();
    }

    public AssemblyIdentity UnifyAssembly(AssemblyIdentity assemblyIdentity) {
      Contract.Requires(assemblyIdentity != null);
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
      throw new NotImplementedException();
    }

    public AssemblyIdentity UnifyAssembly(IAssemblyReference assemblyReference) {
      Contract.Requires(assemblyReference != null);
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
      throw new NotImplementedException();
    }

    public bool PreserveILLocations {
      get { throw new NotImplementedException(); }
    }
  }

  /// <summary>
  /// Implemented by types that contain a collection of members of type MemberType. For example a namespace contains a collection of INamespaceMember instances.
  /// </summary>
  /// <typeparam name="MemberType">The type of member contained by the Members collection of this container.</typeparam>
  [ContractClass(typeof(IContainerContract<>))]
  public interface IContainer<MemberType>
    where MemberType : class {
    /// <summary>
    /// The collection of contained members.
    /// </summary>
    IEnumerable<MemberType> Members { get; }
  }

  [ContractClassFor(typeof(IContainer<>))]
  abstract class IContainerContract<MemberType> : IContainer<MemberType>
    where MemberType : class {
    public IEnumerable<MemberType> Members {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<MemberType>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => x != null));
        throw new NotImplementedException();
      }
    }
  }

  /// <summary>
  /// Implemented by types whose instances belong to a specific type of container (see IContainer&lt;MemberType&gt;).
  /// </summary>
  /// <typeparam name="ContainerType">The type of the container that has members of this type.</typeparam>
  public interface IContainerMember<ContainerType> : INamedEntity {
    /// <summary>
    /// The container instance with a Members collection that includes this instance.
    /// </summary>
    ContainerType Container { get; }

    /// <summary>
    /// The name of the entity.
    /// </summary>
    new IName Name { get; }
  }

  /// <summary>
  /// An object corresponding to a metadata entity such as a type or a field.
  /// </summary>
  public interface IDefinition : IReference {
  }

  /// <summary>
  /// An object corresponding to reference to a metadata entity such as a type or a field.
  /// </summary>
  public interface IReference : IObjectWithLocations {

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition.
    /// </summary>
    IEnumerable<ICustomAttribute> Attributes { get; }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference. The dispatch method does nothing else.
    /// </summary>
    void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference, which is not derived from IDefinition. For example an object implemeting IArrayType will
    /// call visitor.Visit(IArrayTypeReference) and not visitor.Visit(IArrayType).
    /// The dispatch method does nothing else.
    /// </summary>
    void DispatchAsReference(IMetadataVisitor visitor);

  }

  /// <summary>
  /// An object that represents a document. This can be either source or binary or designer surface etc
  /// </summary>
  public interface IDocument {

    /// <summary>
    /// The location where this document was found, or where it should be stored.
    /// This will also uniquely identify the source document within an instance of compilation host.
    /// </summary>
    string Location { get; }

    /// <summary>
    /// The name of the document. For example the name of the file if the document corresponds to a file.
    /// </summary>
    IName Name { get; }

  }

  /// <summary>
  /// Error information relating to a portion of a document.
  /// </summary>
  public interface IErrorMessage {

    /// <summary>
    /// The object reporting the error. This can be used to filter out errors coming from non interesting sources.
    /// </summary>
    object ErrorReporter { get; }

    /// <summary>
    /// A short identifier for the reporter of the error, suitable for use in human interfaces. For example "CS" in the case of a C# language error.
    /// </summary>
    string ErrorReporterIdentifier { get; }

    /// <summary>
    /// A code that corresponds to this error. This code is the same for all cultures.
    /// </summary>
    long Code { get; }

    /// <summary>
    /// True if the error message should be treated as an informational warning rather than as an indication that the associated
    /// compilation has failed and no useful executable output has been generated. The value of this property does
    /// not depend solely on this.Code but can be influenced by compiler options such as the csc /warnaserror option.
    /// </summary>
    bool IsWarning { get; }

    /// <summary>
    /// A description of the error suitable for user interaction. Localized to the current culture.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// The location of the error.
    /// </summary>
    ILocation Location { get; }

    /// <summary>
    /// Zero ore more locations that are related to this error.
    /// </summary>
    IEnumerable<ILocation> RelatedLocations { get; }

  }

  /// <summary>
  /// Implemented by metadata objects that have been obtained from a CLR PE file.
  /// </summary>
  public interface IMetadataObjectWithToken {
    /// <summary>
    /// The most significant byte identifies a metadata table, using the values specified by ECMA-335.
    /// The least significant three bytes represent the row number in the table, with the first row being numbered as one.
    /// If, for some implementation reason, a metadata object implements this interface but was not obtained from a metadata table
    /// (for example it might be an array type reference that only occurs in a signature blob), the value is UInt32.MaxValue.
    /// </summary>
    uint TokenValue { get; }
  }

  /// <summary>
  /// Implemented by methods that can turn tokens into metadata objects. For example, a method definition implemented
  /// by a metadata reader might implement this interface.
  /// </summary>
  [ContractClass(typeof(ITokenDecoderContract))]
  public interface ITokenDecoder : IMethodDefinition {
    /// <summary>
    /// Returns an instance of IMetadataObjectWithToken whose TokenValue property is the given token value.
    /// If no such object can be found then the result is null.
    /// </summary>
    IMetadataObjectWithToken/*?*/ GetObjectForToken(uint token);
  }

  #region ITokenDecoder contract binding
  [ContractClassFor(typeof(ITokenDecoder))]
  abstract class ITokenDecoderContract : ITokenDecoder {
    public IMetadataObjectWithToken GetObjectForToken(uint token) {
      Contract.Ensures(Contract.Result<IMetadataObjectWithToken>() == null || Contract.Result<IMetadataObjectWithToken>().TokenValue == token);
      throw new NotImplementedException();
    }

    #region IMethodDefinition Members

    public IMethodBody Body {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IGenericMethodParameter> GenericParameters {
      get { throw new NotImplementedException(); }
    }

    public bool HasDeclarativeSecurity {
      get { throw new NotImplementedException(); }
    }

    public bool HasExplicitThisParameter {
      get { throw new NotImplementedException(); }
    }

    public bool IsAbstract {
      get { throw new NotImplementedException(); }
    }

    public bool IsAccessCheckedOnOverride {
      get { throw new NotImplementedException(); }
    }

    public bool IsAggressivelyInlined {
      get { throw new NotImplementedException(); }
    }

    public bool IsCil {
      get { throw new NotImplementedException(); }
    }

    public bool IsConstructor {
      get { throw new NotImplementedException(); }
    }

    public bool IsExternal {
      get { throw new NotImplementedException(); }
    }

    public bool IsForwardReference {
      get { throw new NotImplementedException(); }
    }

    public bool IsHiddenBySignature {
      get { throw new NotImplementedException(); }
    }

    public bool IsNativeCode {
      get { throw new NotImplementedException(); }
    }

    public bool IsNewSlot {
      get { throw new NotImplementedException(); }
    }

    public bool IsNeverInlined {
      get { throw new NotImplementedException(); }
    }

    public bool IsNeverOptimized {
      get { throw new NotImplementedException(); }
    }

    public bool IsPlatformInvoke {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeImplemented {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeInternal {
      get { throw new NotImplementedException(); }
    }

    public bool IsRuntimeSpecial {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed {
      get { throw new NotImplementedException(); }
    }

    public bool IsSpecialName {
      get { throw new NotImplementedException(); }
    }

    public bool IsStaticConstructor {
      get { throw new NotImplementedException(); }
    }

    public bool IsSynchronized {
      get { throw new NotImplementedException(); }
    }

    public bool IsVirtual {
      get { throw new NotImplementedException(); }
    }

    public bool IsUnmanaged {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get { throw new NotImplementedException(); }
    }

    public bool PreserveSignature {
      get { throw new NotImplementedException(); }
    }

    public IPlatformInvokeInformation PlatformInvokeData {
      get { throw new NotImplementedException(); }
    }

    public bool RequiresSecurityObject {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomAttribute> ReturnValueAttributes {
      get { throw new NotImplementedException(); }
    }

    public bool ReturnValueIsMarshalledExplicitly {
      get { throw new NotImplementedException(); }
    }

    public IMarshallingInformation ReturnValueMarshallingInformation {
      get { throw new NotImplementedException(); }
    }

    public IName ReturnValueName {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { throw new NotImplementedException(); }
    }

    public TypeMemberVisibility Visibility {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { throw new NotImplementedException(); }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
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

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IMethodReference Members

    public bool AcceptsExtraArguments {
      get { throw new NotImplementedException(); }
    }

    public ushort GenericParameterCount {
      get { throw new NotImplementedException(); }
    }

    public uint InternedKey {
      get { throw new NotImplementedException(); }
    }

    public bool IsGeneric {
      get { throw new NotImplementedException(); }
    }

    public ushort ParameterCount {
      get { throw new NotImplementedException(); }
    }

    public IMethodDefinition ResolvedMethod {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ISignature Members

    public CallingConvention CallingConvention {
      get { throw new NotImplementedException(); }
    }

    public bool IsStatic {
      get { throw new NotImplementedException(); }
    }

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { throw new NotImplementedException(); }
    }

    public bool ReturnValueIsByRef {
      get { throw new NotImplementedException(); }
    }

    public bool ReturnValueIsModified {
      get { throw new NotImplementedException(); }
    }

    public ITypeReference Type {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A collection of named members, with routines to search the collection.
  /// </summary>
  [ContractClass(typeof(ISCopeContract<>))]
  public interface IScope<MemberType>
    where MemberType : class, INamedEntity {

    /// <summary>
    /// Return true if the given member instance is a member of this scope.
    /// </summary>
    [Pure]
    bool Contains(MemberType/*!*/ member);
    // ^ ensures result == exists{MemberType mem in this.Members; mem == member};

    /// <summary>
    /// Returns the list of members with the given name that also satisfy the given predicate.
    /// </summary>
    [Pure]
    IEnumerable<MemberType> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<MemberType, bool> predicate);
    // ^ ensures forall{MemberType member in result; member.Name == name && predicate(member) && this.Contains(member)};
    // ^ ensures forall{MemberType member in this.Members; member.Name == name && predicate(member) ==> 
    // ^                                                            exists{INamespaceMember mem in result; mem == member}};

    /// <summary>
    /// Returns the list of members that satisfy the given predicate.
    /// </summary>
    [Pure]
    IEnumerable<MemberType> GetMatchingMembers(Function<MemberType, bool> predicate);
    // ^ ensures forall{MemberType member in result; predicate(member) && this.Contains(member)};
    // ^ ensures forall{MemberType member in this.Members; predicate(member) ==> exists{MemberType mem in result; mem == member}};

    /// <summary>
    /// Returns the list of members with the given name.
    /// </summary>
    [Pure]
    IEnumerable<MemberType> GetMembersNamed(IName name, bool ignoreCase);
    // ^ ensures forall{MemberType member in result; member.Name == name && this.Contains(member)};
    // ^ ensures forall{MemberType member in this.Members; member.Name == name ==> 
    // ^                                                            exists{INamespaceMember mem in result; mem == member}};

    /// <summary>
    /// The collection of member instances that are members of this scope.
    /// </summary>
    IEnumerable<MemberType> Members { get; }
  }

  [ContractClassFor(typeof(IScope<>))]
  abstract class ISCopeContract<MemberType> : IScope<MemberType>
    where MemberType : class, INamedEntity {

    public bool Contains(MemberType member) {
      //Contract.Ensures(!Contract.Result<bool>() || Contract.Exists(this.Members, x => x == (object)this));
      throw new NotImplementedException();
    }

    public IEnumerable<MemberType> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<MemberType, bool> predicate) {
      Contract.Requires(name != null);
      Contract.Requires(predicate != null);
      Contract.Ensures(Contract.Result<IEnumerable<MemberType>>() != null);
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => x != null));
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => this.Contains(x)));
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(),
        x => ignoreCase ? x.Name.UniqueKeyIgnoringCase == x.Name.UniqueKeyIgnoringCase : x.Name == name));
      //Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => predicate(x)));
      throw new NotImplementedException();
    }

    public IEnumerable<MemberType> GetMatchingMembers(Function<MemberType, bool> predicate) {
      Contract.Requires(predicate != null);
      Contract.Ensures(Contract.Result<IEnumerable<MemberType>>() != null);
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => x != null));
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => this.Contains(x)));
      //Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => predicate(x)));
      throw new NotImplementedException();
    }

    public IEnumerable<MemberType> GetMembersNamed(IName name, bool ignoreCase) {
      Contract.Requires(name != null);
      Contract.Ensures(Contract.Result<IEnumerable<MemberType>>() != null);
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => x != null));
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => this.Contains(x)));
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(),
        x => ignoreCase ? x.Name.UniqueKeyIgnoringCase == x.Name.UniqueKeyIgnoringCase : x.Name == name));
      throw new NotImplementedException();
    }

    public IEnumerable<MemberType> Members {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<MemberType>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => x != null));
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<MemberType>>(), x => this.Contains(x)));
        throw new NotImplementedException();
      }
    }
  }


  /// <summary>
  /// Implemented by types whose instances belong to a specific type of scope (see IScope&lt;MemberType&gt;).
  /// </summary>
  /// <typeparam name="ScopeType">The type of the scope that has members of this type.</typeparam>
  public interface IScopeMember<ScopeType> : INamedEntity {
    /// <summary>
    /// The scope instance with a Members collection that includes this instance.
    /// </summary>
    ScopeType ContainingScope { get; }
  }

  /// <summary>
  /// Implemented by types whose instances are usually derived from documents.
  /// </summary>
  [ContractClass(typeof(IObjectWithLocationsContract))]
  public interface IObjectWithLocations {

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance.
    /// </summary>
    IEnumerable<ILocation> Locations { get; }

  }

  [ContractClassFor(typeof(IObjectWithLocations))]
  abstract class IObjectWithLocationsContract : IObjectWithLocations {
    public IEnumerable<ILocation> Locations {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ILocation>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ILocation>>(), x => x != null));
        throw new NotImplementedException();
      }
    }
  }

  /// <summary>
  /// Represents a location in IL operation stream.
  /// </summary>
  public interface IILLocation : ILocation {

    /// <summary>
    /// The method whose body contains this IL operation whose location this is.
    /// </summary>
    IMethodDefinition MethodDefinition { get; }

    /// <summary>
    /// Offset into the IL Stream.
    /// </summary>
    uint Offset { get; }
  }

  /// <summary>
  /// Represents a location in some document.
  /// </summary>
  public interface ILocation {
    /// <summary>
    /// The document containing this location.
    /// </summary>
    IDocument Document {
      get;
    }
  }

  /// <summary>
  /// A collection of methods that associate unique integers with metadata model entities.
  /// The association is based on the identities of the entities and the factory does not retain
  /// references to the given metadata model objects.
  /// </summary>
  public interface IInternFactory {
    /// <summary>
    /// Returns the interned key corresponding to given assembly. The assembly is unified using ICompilationHostEnvironment.UnifyAssembly.
    /// </summary>
    uint GetAssemblyInternedKey(AssemblyIdentity assemblyIdentity);

    /// <summary>
    /// Returns the interned key corresponding to the referenced field.
    /// </summary>
    uint GetFieldInternedKey(IFieldReference fieldReference);

    /// <summary>
    /// Returns the interned key corresponding to the referenced method.
    /// </summary>
    uint GetMethodInternedKey(IMethodReference methodReference);

    /// <summary>
    /// Returns the interned key corresponding to given module.
    /// </summary>
    uint GetModuleInternedKey(ModuleIdentity moduleIdentity);

    /// <summary>
    /// Returns the interned key for the namespace type constructed with the given parameters
    /// </summary>
    uint GetNamespaceTypeReferenceInternedKey(IUnitNamespaceReference containingUnitNamespace, IName typeName, uint genericParameterCount);

    /// <summary>
    /// Returns the interned key for the nested type constructed with the given parameters
    /// </summary>
    uint GetNestedTypeReferenceInternedKey(ITypeReference containingTypeReference, IName typeName, uint genericParameterCount);

    /// <summary>
    /// Returns the interned key for vector type with given element type reference
    /// </summary>
    uint GetVectorTypeReferenceInternedKey(ITypeReference elementTypeReference);

    /// <summary>
    /// Returns the interned key for the matrix type constructed with the given parameters
    /// </summary>
    uint GetMatrixTypeReferenceInternedKey(ITypeReference elementTypeReference, int rank, IEnumerable<ulong> sizes, IEnumerable<int> lowerBounds);

    /// <summary>
    /// Returns the interned key for the generic type instance constructed with the given parameters
    /// </summary>
    uint GetGenericTypeInstanceReferenceInternedKey(ITypeReference genericTypeReference, IEnumerable<ITypeReference> genericArguments);

    /// <summary>
    /// Returns the interned key for the pointer type constructed with the given target type reference
    /// </summary>
    uint GetPointerTypeReferenceInternedKey(ITypeReference targetTypeReference);

    /// <summary>
    /// Returns the interned key for the managed pointer type constructed with the given target type reference
    /// </summary>
    uint GetManagedPointerTypeReferenceInternedKey(ITypeReference targetTypeReferece);

    /// <summary>
    /// Returns the interned key for the generic type parameter constructed with the given definingType and index
    /// </summary>
    uint GetGenericTypeParameterReferenceInternedKey(ITypeReference definingTypeReference, int index);

    /// <summary>
    /// Returns the interned key for the generic method parameter constructed with the given index
    /// </summary>
    uint GetGenericMethodParameterReferenceInternedKey(IMethodReference defininingMethodReference, int index);

    /// <summary>
    /// Returns the interned key for the function pointer type constructed with the given parameters
    /// </summary>
    uint GetFunctionPointerTypeReferenceInternedKey(CallingConvention callingConvention, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation> extraArgumentTypes, IEnumerable<ICustomModifier> returnValueCustomModifiers, bool returnValueIsByRef, ITypeReference returnType);

    /// <summary>
    /// Returns the interned key for the given modified type reference
    /// </summary>
    uint GetModifiedTypeReferenceInternedKey(ITypeReference typeReference, IEnumerable<ICustomModifier> customModifiers);

    /// <summary>
    /// Returns the interned key for the given type reference
    /// </summary>
    uint GetTypeReferenceInternedKey(ITypeReference typeReference);
  }

  /// <summary>
  /// Implemented by classes that visit nodes of object graphs via a double dispatch mechanism, usually performing some computation of a subset of the nodes in the graph.
  /// Contains a specialized Visit routine for each standard type of object defined in this object model. 
  /// </summary>
  public interface IMetadataVisitor {
    /// <summary>
    /// Performs some computation with the given array type reference.
    /// </summary>
    void Visit(IArrayTypeReference arrayTypeReference);
    /// <summary>
    /// Performs some computation with the given assembly.
    /// </summary>
    void Visit(IAssembly assembly);
    /// <summary>
    /// Performs some computation with the given assembly reference.
    /// </summary>
    void Visit(IAssemblyReference assemblyReference);
    /// <summary>
    /// Performs some computation with the given custom attribute.
    /// </summary>
    void Visit(ICustomAttribute customAttribute);
    /// <summary>
    /// Performs some computation with the given custom modifier.
    /// </summary>
    void Visit(ICustomModifier customModifier);
    /// <summary>
    /// Performs some computation with the given event definition.
    /// </summary>
    void Visit(IEventDefinition eventDefinition);
    /// <summary>
    /// Performs some computation with the given field definition.
    /// </summary>
    void Visit(IFieldDefinition fieldDefinition);
    /// <summary>
    /// Performs some computation with the given field reference.
    /// </summary>
    void Visit(IFieldReference fieldReference);
    /// <summary>
    /// Performs some computation with the given file reference.
    /// </summary>
    void Visit(IFileReference fileReference);
    /// <summary>
    /// Performs some computation with the given function pointer type reference.
    /// </summary>
    void Visit(IFunctionPointerTypeReference functionPointerTypeReference);
    /// <summary>
    /// Performs some computation with the given generic method instance reference.
    /// </summary>
    void Visit(IGenericMethodInstanceReference genericMethodInstanceReference);
    /// <summary>
    /// Performs some computation with the given generic method parameter.
    /// </summary>
    void Visit(IGenericMethodParameter genericMethodParameter);
    /// <summary>
    /// Performs some computation with the given generic method parameter reference.
    /// </summary>
    void Visit(IGenericMethodParameterReference genericMethodParameterReference);
    /// <summary>
    /// Performs some computation with the given global field definition.
    /// </summary>
    void Visit(IGlobalFieldDefinition globalFieldDefinition);
    /// <summary>
    /// Performs some computation with the given global method definition.
    /// </summary>
    void Visit(IGlobalMethodDefinition globalMethodDefinition);
    /// <summary>
    /// Performs some computation with the given generic type instance reference.
    /// </summary>
    void Visit(IGenericTypeInstanceReference genericTypeInstanceReference);
    /// <summary>
    /// Performs some computation with the given generic parameter.
    /// </summary>
    void Visit(IGenericTypeParameter genericTypeParameter);
    /// <summary>
    /// Performs some computation with the given generic type parameter reference.
    /// </summary>
    void Visit(IGenericTypeParameterReference genericTypeParameterReference);
    /// <summary>
    /// Performs some computation with the given local definition.
    /// </summary>
    void Visit(ILocalDefinition localDefinition);
    /// <summary>
    /// Performs some computation with the given local definition.
    /// </summary>
    void VisitReference(ILocalDefinition localDefinition);
    /// <summary>
    /// Performs some computation with the given managed pointer type reference.
    /// </summary>
    void Visit(IManagedPointerTypeReference managedPointerTypeReference);
    /// <summary>
    /// Performs some computation with the given marshalling information.
    /// </summary>
    void Visit(IMarshallingInformation marshallingInformation);
    /// <summary>
    /// Performs some computation with the given metadata constant.
    /// </summary>
    void Visit(IMetadataConstant constant);
    /// <summary>
    /// Performs some computation with the given metadata array creation expression.
    /// </summary>
    void Visit(IMetadataCreateArray createArray);
    /// <summary>
    /// Performs some computation with the given metadata expression.
    /// </summary>
    void Visit(IMetadataExpression expression);
    /// <summary>
    /// Performs some computation with the given metadata named argument expression.
    /// </summary>
    void Visit(IMetadataNamedArgument namedArgument);
    /// <summary>
    /// Performs some computation with the given metadata typeof expression.
    /// </summary>
    void Visit(IMetadataTypeOf typeOf);
    /// <summary>
    /// Performs some computation with the given method body.
    /// </summary>
    void Visit(IMethodBody methodBody);
    /// <summary>
    /// Performs some computation with the given method definition.
    /// </summary>
    void Visit(IMethodDefinition method);
    /// <summary>
    /// Performs some computation with the given method implementation.
    /// </summary>
    void Visit(IMethodImplementation methodImplementation);
    /// <summary>
    /// Performs some computation with the given method reference.
    /// </summary>
    void Visit(IMethodReference methodReference);
    /// <summary>
    /// Performs some computation with the given modified type reference.
    /// </summary>
    void Visit(IModifiedTypeReference modifiedTypeReference);
    /// <summary>
    /// Performs some computation with the given module.
    /// </summary>
    void Visit(IModule module);
    /// <summary>
    /// Performs some computation with the given module reference.
    /// </summary>
    void Visit(IModuleReference moduleReference);
    /// <summary>
    /// Performs some computation with the given alias for a namespace type definition.
    /// </summary>
    void Visit(INamespaceAliasForType namespaceAliasForType);
    /// <summary>
    /// Performs some computation with the given namespace type definition.
    /// </summary>
    void Visit(INamespaceTypeDefinition namespaceTypeDefinition);
    /// <summary>
    /// Performs some computation with the given namespace type reference.
    /// </summary>
    void Visit(INamespaceTypeReference namespaceTypeReference);
    /// <summary>
    /// Performs some computation with the given alias to a nested type definition.
    /// </summary>
    void Visit(INestedAliasForType nestedAliasForType);
    /// <summary>
    /// Performs some computation with the given nested type definition.
    /// </summary>
    void Visit(INestedTypeDefinition nestedTypeDefinition);
    /// <summary>
    /// Performs some computation with the given nested type reference.
    /// </summary>
    void Visit(INestedTypeReference nestedTypeReference);
    /// <summary>
    /// Performs some computation with the given nested unit namespace.
    /// </summary>
    void Visit(INestedUnitNamespace nestedUnitNamespace);
    /// <summary>
    /// Performs some computation with the given nested unit namespace reference.
    /// </summary>
    void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference);
    /// <summary>
    /// Performs some computation with the given nested unit set namespace.
    /// </summary>
    void Visit(INestedUnitSetNamespace nestedUnitSetNamespace);
    /// <summary>
    /// Performs some computation with the given IL operation.
    /// </summary>
    void Visit(IOperation operation);
    /// <summary>
    /// Performs some computation with the given IL operation exception information instance.
    /// </summary>
    /// <param name="operationExceptionInformation"></param>
    void Visit(IOperationExceptionInformation operationExceptionInformation);
    /// <summary>
    /// Performs some computation with the given parameter definition.
    /// </summary>
    void Visit(IParameterDefinition parameterDefinition);
    /// <summary>
    /// Performs some computation with the given parameter definition.
    /// </summary>
    void VisitReference(IParameterDefinition parameterDefinition);
    /// <summary>
    /// Performs some computation with the given parameter type information.
    /// </summary>
    void Visit(IParameterTypeInformation parameterTypeInformation);
    /// <summary>
    /// Performs some computation with the given PE section.
    /// </summary>
    void Visit(IPESection peSection);
    /// <summary>
    /// Performs some compuation with the given platoform invoke information.
    /// </summary>
    void Visit(IPlatformInvokeInformation platformInvokeInformation);
    /// <summary>
    /// Performs some computation with the given pointer type reference.
    /// </summary>
    void Visit(IPointerTypeReference pointerTypeReference);
    /// <summary>
    /// Performs some computation with the given property definition.
    /// </summary>
    void Visit(IPropertyDefinition propertyDefinition);
    /// <summary>
    /// Performs some computation with the given reference to a manifest resource.
    /// </summary>
    void Visit(IResourceReference resourceReference);
    /// <summary>
    /// Performs some computation with the given root unit namespace.
    /// </summary>
    void Visit(IRootUnitNamespace rootUnitNamespace);
    /// <summary>
    /// Performs some computation with the given root unit namespace reference.
    /// </summary>
    void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference);
    /// <summary>
    /// Performs some computation with the given root unit set namespace.
    /// </summary>
    void Visit(IRootUnitSetNamespace rootUnitSetNamespace);
    /// <summary>
    /// Performs some computation with the given security attribute.
    /// </summary>
    void Visit(ISecurityAttribute securityAttribute);
    /// <summary>
    /// Performs some computation with the given specialized event definition.
    /// </summary>
    void Visit(ISpecializedEventDefinition specializedEventDefinition);
    /// <summary>
    /// Performs some computation with the given specialized field definition.
    /// </summary>
    void Visit(ISpecializedFieldDefinition specializedFieldDefinition);
    /// <summary>
    /// Performs some computation with the given specialized field reference.
    /// </summary>
    void Visit(ISpecializedFieldReference specializedFieldReference);
    /// <summary>
    /// Performs some computation with the given specialized method definition.
    /// </summary>
    void Visit(ISpecializedMethodDefinition specializedMethodDefinition);
    /// <summary>
    /// Performs some computation with the given specialized method reference.
    /// </summary>
    void Visit(ISpecializedMethodReference specializedMethodReference);
    /// <summary>
    /// Performs some computation with the given specialized propperty definition.
    /// </summary>
    void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition);
    /// <summary>
    /// Performs some computation with the given specialized nested type definition.
    /// </summary>
    void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition);
    /// <summary>
    /// Performs some computation with the given specialized nested type reference.
    /// </summary>
    void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference);
    /// <summary>
    /// Performs some computation with the given unit set.
    /// </summary>
    void Visit(IUnitSet unitSet);
    /// <summary>
    /// Performs some computation with the given Win32 resource.
    /// </summary>
    void Visit(IWin32Resource win32Resource);
  }

}
