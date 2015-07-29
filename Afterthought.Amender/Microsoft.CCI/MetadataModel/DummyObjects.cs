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
using System.Diagnostics;
using System.Threading;
using System.Diagnostics.Contracts;

namespace Microsoft.Cci {

#pragma warning disable 1591

  [ContractVerification(false)]
  public abstract class Dummy {

    public static IAliasForType AliasForType {
      get {
        Contract.Ensures(Contract.Result<IAliasForType>() != null);
        if (Dummy.aliasForType == null)
          Interlocked.CompareExchange(ref Dummy.aliasForType, new DummyNamespaceAliasForType(), null);
        Contract.Assume(Dummy.aliasForType != null);
        return Dummy.aliasForType;
      }
    }
    private static IAliasForType/*?*/ aliasForType;

    public static IAssembly Assembly {
      get {
        Contract.Ensures(Contract.Result<IAssembly>() != null);
        if (Dummy.assembly == null)
          Interlocked.CompareExchange(ref Dummy.assembly, new DummyAssembly(), null);
        return Dummy.assembly;
      }
    }
    private static IAssembly/*?*/ assembly;

    public static AssemblyIdentity AssemblyIdentity {
      get {
        Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);
        if (Dummy.assemblyIdentity == null)
          Interlocked.CompareExchange(ref Dummy.assemblyIdentity, new AssemblyIdentity(Dummy.name, "", new Version(0, 0), Enumerable<byte>.Empty, ""), null);
        return Dummy.assemblyIdentity;
      }
    }
    private static AssemblyIdentity/*?*/ assemblyIdentity;

    public static IMetadataHost CompilationHostEnvironment {
      get {
        Contract.Ensures(Contract.Result<IMetadataHost>() != null);
        if (Dummy.compilationHostEnvironment == null)
          Interlocked.CompareExchange(ref Dummy.compilationHostEnvironment, new DummyMetadataHost(), null);
        return Dummy.compilationHostEnvironment;
      }
    }
    private static IMetadataHost/*?*/ compilationHostEnvironment;

    public static IMetadataConstant Constant {
      get {
        Contract.Ensures(Contract.Result<IMetadataConstant>() != null);
        if (Dummy.constant == null)
          Interlocked.CompareExchange(ref Dummy.constant, new DummyMetadataConstant(), null);
        return Dummy.constant;
      }
    }
    private static IMetadataConstant/*?*/ constant;

    public static ICustomModifier CustomModifier {
      get {
        Contract.Ensures(Contract.Result<ICustomModifier>() != null);
        if (Dummy.customModifier == null)
          Interlocked.CompareExchange(ref Dummy.customModifier, new DummyCustomModifier(), null);
        return Dummy.customModifier;
      }
    }
    private static ICustomModifier/*?*/ customModifier;

    public static IEventDefinition Event {
      get {
        Contract.Ensures(Contract.Result<IEventDefinition>() != null);
        return Dummy.EventDefinition;
      }
    }

    public static IEventDefinition EventDefinition {
      get {
        Contract.Ensures(Contract.Result<IEventDefinition>() != null);
        if (Dummy.eventDefinition == null)
          Interlocked.CompareExchange(ref Dummy.eventDefinition, new DummyEventDefinition(), null);
        return Dummy.eventDefinition;
      }
    }
    private static IEventDefinition/*?*/ eventDefinition;

    public static IFieldDefinition Field {
      get {
        Contract.Ensures(Contract.Result<IFieldDefinition>() != null);
        return Dummy.FieldDefinition;
      }
    }

    public static IFieldDefinition FieldDefinition {
      get {
        Contract.Ensures(Contract.Result<IFieldDefinition>() != null);
        if (Dummy.field == null)
          Interlocked.CompareExchange(ref Dummy.field, new DummyFieldDefinition(), null);
        return Dummy.field;
      }
    }
    private static IFieldDefinition/*?*/ field;

    public static IMetadataExpression Expression {
      get {
        Contract.Ensures(Contract.Result<IMetadataExpression>() != null);
        if (Dummy.expression == null)
          Interlocked.CompareExchange(ref Dummy.expression, new DummyMetadataExpression(), null);
        return Dummy.expression;
      }
    }
    private static IMetadataExpression/*?*/ expression;

    public static IFunctionPointer FunctionPointer {
      get {
        Contract.Ensures(Contract.Result<IFunctionPointer>() != null);
        if (Dummy.functionPointer == null)
          Interlocked.CompareExchange(ref Dummy.functionPointer, new DummyFunctionPointerType(), null);
        return Dummy.functionPointer;
      }
    }
    private static IFunctionPointer/*?*/ functionPointer;

    public static IGenericMethodParameter GenericMethodParameter {
      get {
        Contract.Ensures(Contract.Result<IGenericMethodParameter>() != null);
        if (Dummy.genericMethodParameter == null)
          Interlocked.CompareExchange(ref Dummy.genericMethodParameter, new DummyGenericMethodParameter(), null);
        return Dummy.genericMethodParameter;
      }
    }
    private static DummyGenericMethodParameter/*?*/ genericMethodParameter;

    public static IGenericTypeInstance GenericTypeInstance {
      get {
        Contract.Ensures(Contract.Result<IGenericTypeInstance>() != null);
        if (Dummy.genericTypeInstance == null)
          Interlocked.CompareExchange(ref Dummy.genericTypeInstance, new DummyGenericTypeInstance(), null);
        return Dummy.genericTypeInstance;
      }
    }
    private static DummyGenericTypeInstance/*?*/ genericTypeInstance;

    public static IGenericTypeInstanceReference GenericTypeInstanceReference {
      get {
        Contract.Ensures(Contract.Result<IGenericTypeInstanceReference>() != null);
        if (Dummy.genericTypeInstanceReference == null)
          Interlocked.CompareExchange(ref Dummy.genericTypeInstanceReference, new DummyGenericTypeInstance(), null);
        return Dummy.genericTypeInstanceReference;
      }
    }
    private static IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference;

    public static IGenericTypeParameter GenericTypeParameter {
      get {
        Contract.Ensures(Contract.Result<IGenericTypeParameter>() != null);
        if (Dummy.genericTypeParameter == null)
          Interlocked.CompareExchange(ref Dummy.genericTypeParameter, new DummyGenericTypeParameter(), null);
        return Dummy.genericTypeParameter;
      }
    }
    private static IGenericTypeParameter/*?*/ genericTypeParameter;

    public static IMethodDefinition Method {
      get {
        Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
        return Dummy.MethodDefinition;
      }
    }

    public static IMethodDefinition MethodDefinition {
      get {
        Contract.Ensures(Contract.Result<IMethodDefinition>() != null);
        if (Dummy.methodDefinition == null)
          Interlocked.CompareExchange(ref Dummy.methodDefinition, new DummyMethodDefinition(), null);
        return Dummy.methodDefinition;
      }
    }
    private static IMethodDefinition/*?*/ methodDefinition;

    public static IMethodBody MethodBody {
      get {
        Contract.Ensures(Contract.Result<IMethodBody>() != null);
        if (Dummy.methodBody == null)
          Interlocked.CompareExchange(ref Dummy.methodBody, new DummyMethodBody(), null);
        return Dummy.methodBody;
      }
    }
    private static IMethodBody/*?*/ methodBody;

    public static IName Name {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        if (Dummy.name == null)
          Interlocked.CompareExchange(ref Dummy.name, new DummyName(), null);
        return Dummy.name;
      }
    }
    private static IName/*?*/ name;

    public static IMetadataNamedArgument NamedArgument {
      get {
        Contract.Ensures(Contract.Result<IMetadataNamedArgument>() != null);
        if (Dummy.namedArgument == null)
          Interlocked.CompareExchange(ref Dummy.namedArgument, new DummyNamedArgument(), null);
        return Dummy.namedArgument;
      }
    }
    private static IMetadataNamedArgument/*?*/ namedArgument;

    public static INamedTypeReference NamedTypeReference {
      get {
        Contract.Ensures(Contract.Result<INamedTypeReference>() != null);
        if (Dummy.namedTypeReference == null)
          Interlocked.CompareExchange(ref Dummy.namedTypeReference, new DummyNamespaceTypeReference(), null);
        return Dummy.namedTypeReference;
      }
    }
    private static INamedTypeReference/*?*/ namedTypeReference;

    public static INameTable NameTable {
      get {
        Contract.Ensures(Contract.Result<INameTable>() != null);
        if (Dummy.nameTable == null)
          Interlocked.CompareExchange(ref Dummy.nameTable, new DummyNameTable(), null);
        return Dummy.nameTable;
      }
    }
    private static INameTable/*?*/ nameTable;

    public static INestedTypeDefinition NestedType {
      get {
        Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);
        return Dummy.NestedTypeDefinition;
      }
    }

    public static INestedTypeDefinition NestedTypeDefinition {
      get {
        Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);
        if (Dummy.nestedTypeDefinition == null)
          Interlocked.CompareExchange(ref Dummy.nestedTypeDefinition, new DummyNestedType(), null);
        return Dummy.nestedTypeDefinition;
      }
    }
    private static INestedTypeDefinition/*?*/ nestedTypeDefinition;

    public static INestedTypeReference NestedTypeReference {
      get {
        Contract.Ensures(Contract.Result<INestedTypeReference>() != null);
        if (Dummy.nestedTypeReference == null)
          Interlocked.CompareExchange(ref Dummy.nestedTypeReference, new DummyNestedType(), null);
        return Dummy.nestedTypeReference;
      }
    }
    private static INestedTypeDefinition/*?*/ nestedTypeReference;

    public static INestedUnitNamespace NestedUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<INestedUnitNamespace>() != null);
        if (Dummy.nestedUnitNamespace == null)
          Interlocked.CompareExchange(ref Dummy.nestedUnitNamespace, new DummyNestedUnitNamespace(), null);
        return Dummy.nestedUnitNamespace;
      }
    }
    private static INestedUnitNamespace/*?*/ nestedUnitNamespace;

    public static IPlatformType PlatformType {
      get {
        Contract.Ensures(Contract.Result<IPlatformType>() != null);
        if (Dummy.platformType == null)
          Interlocked.CompareExchange(ref Dummy.platformType, new DummyPlatformType(), null);
        return Dummy.platformType;
      }
    }
    private static IPlatformType/*?*/ platformType;

    public static IPropertyDefinition Property {
      get {
        Contract.Ensures(Contract.Result<IPropertyDefinition>() != null);
        return Dummy.PropertyDefinition;
      }
    }

    public static IPropertyDefinition PropertyDefinition {
      get {
        Contract.Ensures(Contract.Result<IPropertyDefinition>() != null);
        if (Dummy.propertyDefinition == null)
          Interlocked.CompareExchange(ref Dummy.propertyDefinition, new DummyPropertyDefinition(), null);
        return Dummy.propertyDefinition;
      }
    }
    private static IPropertyDefinition/*?*/ propertyDefinition;

    public static IRootUnitNamespace RootUnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IRootUnitNamespace>() != null);
        if (Dummy.rootUnitNamespace == null)
          Interlocked.CompareExchange(ref Dummy.rootUnitNamespace, new DummyRootUnitNamespace(), null);
        return Dummy.rootUnitNamespace;
      }
    }
    private static IRootUnitNamespace/*?*/ rootUnitNamespace;

    public static ITypeDefinition Type {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
        if (Dummy.type == null)
          Interlocked.CompareExchange(ref Dummy.type, new DummyNamespaceTypeDefinition(), null);
        return Dummy.type;
      }
    }
    private static ITypeDefinition/*?*/ type;

    public static ITypeDefinition TypeDefinition {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
        return Dummy.Type;
      }
    }

    public static ITypeReference TypeReference {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        if (Dummy.typeReference == null)
          Interlocked.CompareExchange(ref Dummy.typeReference, new DummyNamespaceTypeReference(), null);
        return Dummy.typeReference;
      }
    }
    private static ITypeReference/*?*/ typeReference;

    public static IUnit Unit {
      get {
        Contract.Ensures(Contract.Result<IUnit>() != null);
        if (Dummy.unit == null)
          Interlocked.CompareExchange(ref Dummy.unit, new DummyModule(), null);
        return Dummy.unit;
      }
    }
    private static IUnit/*?*/ unit;

    public static UnitIdentity UnitIdentity {
      get {
        Contract.Ensures(Contract.Result<UnitIdentity>() != null);
        if (Dummy.unitIdentity == null)
          Interlocked.CompareExchange(ref Dummy.unitIdentity, new ModuleIdentity(Dummy.Name, ""), null);
        return Dummy.unitIdentity;
      }
    }
    private static UnitIdentity/*?*/ unitIdentity;

    public static IUnitNamespace UnitNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespace>() != null);
        if (Dummy.unitNamespace == null)
          Interlocked.CompareExchange(ref Dummy.unitNamespace, new DummyRootUnitNamespace(), null);
        return Dummy.unitNamespace;
      }
    }
    private static IUnitNamespace/*?*/ unitNamespace;

    public static IUnitNamespaceReference UnitNamespaceReference {
      get {
        Contract.Ensures(Contract.Result<IUnitNamespaceReference>() != null);
        if (Dummy.unitNamespaceReference == null)
          Interlocked.CompareExchange(ref Dummy.unitNamespaceReference, new DummyRootUnitNamespace(), null);
        return Dummy.unitNamespaceReference;
      }
    }
    private static IUnitNamespaceReference/*?*/ unitNamespaceReference;

    public static IUnitReference UnitReference {
      get {
        Contract.Ensures(Contract.Result<IUnitReference>() != null);
        if (Dummy.unitReference == null)
          Interlocked.CompareExchange(ref Dummy.unitReference, new DummyModuleReference(), null);
        return Dummy.unitReference;
      }
    }
    private static IUnitReference/*?*/ unitReference;

    public static IUnitSet UnitSet {
      get {
        Contract.Ensures(Contract.Result<IUnitSet>() != null);
        if (Dummy.unitSet == null)
          Interlocked.CompareExchange(ref Dummy.unitSet, new DummyUnitSet(), null);
        return Dummy.unitSet;
      }
    }
    private static IUnitSet/*?*/ unitSet;

    public static IUnitSetNamespace UnitSetNamespace {
      get {
        Contract.Ensures(Contract.Result<IUnitSetNamespace>() != null);
        if (Dummy.unitSetNamespace == null)
          Interlocked.CompareExchange(ref Dummy.unitSetNamespace, new DummyRootUnitSetNamespace(), null);
        return Dummy.unitSetNamespace;
      }
    }
    private static IUnitSetNamespace/*?*/ unitSetNamespace;

    public static IRootUnitSetNamespace RootUnitSetNamespace {
      get {
        Contract.Ensures(Contract.Result<IRootUnitSetNamespace>() != null);
        if (Dummy.rootUnitSetNamespace == null)
          Interlocked.CompareExchange(ref Dummy.rootUnitSetNamespace, new DummyRootUnitSetNamespace(), null);
        return Dummy.rootUnitSetNamespace;
      }
    }
    private static IRootUnitSetNamespace/*?*/ rootUnitSetNamespace;

    public static IModule Module {
      get {
        Contract.Ensures(Contract.Result<IModule>() != null);
        if (Dummy.module == null)
          Interlocked.CompareExchange(ref Dummy.module, new DummyModule(), null);
        return Dummy.module;
      }
    }
    private static IModule/*?*/ module;

    public static ModuleIdentity ModuleIdentity {
      get {
        Contract.Ensures(Contract.Result<ModuleIdentity>() != null);
        if (Dummy.moduleIdentity == null)
          Interlocked.CompareExchange(ref Dummy.moduleIdentity, new ModuleIdentity(Dummy.Name, ""), null);
        return Dummy.moduleIdentity;
      }
    }
    static ModuleIdentity moduleIdentity;

    public static IMethodReference MethodReference {
      get {
        Contract.Ensures(Contract.Result<IMethodReference>() != null);
        if (Dummy.methodReference == null)
          Interlocked.CompareExchange(ref Dummy.methodReference, new DummyMethodReference(), null);
        return Dummy.methodReference;
      }
    }
    private static IMethodReference/*?*/ methodReference;

    public static Version Version {
      get {
        Contract.Ensures(Contract.Result<Version>() != null);
        if (Dummy.version == null)
          Interlocked.CompareExchange(ref Dummy.version, new Version(0, 0), null);
        return Dummy.version;
      }
    }
    private static Version/*?*/ version;

    public static ICustomAttribute CustomAttribute {
      get {
        Contract.Ensures(Contract.Result<ICustomAttribute>() != null);
        if (Dummy.customAttribute == null)
          Interlocked.CompareExchange(ref Dummy.customAttribute, new DummyCustomAttribute(), null);
        return Dummy.customAttribute;
      }
    }
    private static ICustomAttribute/*?*/ customAttribute;

    public static IFileReference FileReference {
      get {
        Contract.Ensures(Contract.Result<IFileReference>() != null);
        if (Dummy.fileReference == null)
          Interlocked.CompareExchange(ref Dummy.fileReference, new DummyFileReference(), null);
        return Dummy.fileReference;
      }
    }
    private static IFileReference/*?*/ fileReference;

    public static IResource Resource {
      get {
        Contract.Ensures(Contract.Result<IResource>() != null);
        if (Dummy.resource == null)
          Interlocked.CompareExchange(ref Dummy.resource, new DummyResource(), null);
        return Dummy.resource;
      }
    }
    private static IResource/*?*/ resource;

    public static IModuleReference ModuleReference {
      get {
        Contract.Ensures(Contract.Result<IModuleReference>() != null);
        if (Dummy.moduleReference == null)
          Interlocked.CompareExchange(ref Dummy.moduleReference, new DummyModuleReference(), null);
        return Dummy.moduleReference;
      }
    }
    private static IModuleReference/*?*/ moduleReference;

    public static IAssemblyReference AssemblyReference {
      get {
        Contract.Ensures(Contract.Result<IAssemblyReference>() != null);
        if (Dummy.assemblyReference == null)
          Interlocked.CompareExchange(ref Dummy.assemblyReference, new DummyAssemblyReference(), null);
        return Dummy.assemblyReference;
      }
    }
    private static IAssemblyReference/*?*/ assemblyReference;

    public static IMarshallingInformation MarshallingInformation {
      get {
        Contract.Ensures(Contract.Result<IMarshallingInformation>() != null);
        if (Dummy.marshallingInformation == null)
          Interlocked.CompareExchange(ref Dummy.marshallingInformation, new DummyMarshallingInformation(), null);
        return Dummy.marshallingInformation;
      }
    }
    private static IMarshallingInformation/*?*/ marshallingInformation;

    public static IParameterTypeInformation ParameterTypeInformation {
      get {
        Contract.Ensures(Contract.Result<IParameterTypeInformation>() != null);
        if (Dummy.parameterTypeInformation == null)
          Interlocked.CompareExchange(ref Dummy.parameterTypeInformation, new DummyParameterTypeInformation(), null);
        return Dummy.parameterTypeInformation;
      }
    }
    private static IParameterTypeInformation/*?*/ parameterTypeInformation;

    public static INamedTypeDefinition NamedTypeDefinition {
      get {
        Contract.Ensures(Contract.Result<INamedTypeDefinition>() != null);
        if (Dummy.namedTypeDefinition == null)
          Interlocked.CompareExchange(ref Dummy.namedTypeDefinition, new DummyNamespaceTypeDefinition(), null);
        return Dummy.namedTypeDefinition;
      }
    }
    private static INamedTypeDefinition/*?*/ namedTypeDefinition;

    public static INamespaceDefinition NamespaceDefinition {
      get {
        Contract.Ensures(Contract.Result<INamespaceDefinition>() != null);
        if (Dummy.namespaceDefinition == null)
          Interlocked.CompareExchange(ref Dummy.namespaceDefinition, new DummyRootUnitNamespace(), null);
        return Dummy.namespaceDefinition;
      }
    }
    private static INamespaceDefinition/*?*/ namespaceDefinition;

    public static INamespaceRootOwner NamespaceRootOwner {
      get {
        Contract.Ensures(Contract.Result<INamespaceRootOwner>() != null);
        if (Dummy.namespaceRootOwner == null)
          Interlocked.CompareExchange(ref Dummy.namespaceRootOwner, new DummyModule(), null);
        return Dummy.namespaceRootOwner;
      }
    }
    private static INamespaceRootOwner/*?*/ namespaceRootOwner;

    public static INamespaceTypeDefinition NamespaceTypeDefinition {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeDefinition>() != null);
        if (Dummy.namespaceTypeDefinition == null)
          Interlocked.CompareExchange(ref Dummy.namespaceTypeDefinition, new DummyNamespaceTypeDefinition(), null);
        return Dummy.namespaceTypeDefinition;
      }
    }
    private static INamespaceTypeDefinition/*?*/ namespaceTypeDefinition;

    public static INamespaceTypeReference NamespaceTypeReference {
      get {
        Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);
        if (Dummy.namespaceTypeReference == null)
          Interlocked.CompareExchange(ref Dummy.namespaceTypeReference, new DummyNamespaceTypeReference(), null);
        return Dummy.namespaceTypeReference;
      }
    }
    private static INamespaceTypeReference/*?*/ namespaceTypeReference;

    public static ISectionBlock SectionBlock {
      get {
        Contract.Ensures(Contract.Result<ISectionBlock>() != null);
        if (Dummy.sectionBlock == null)
          Interlocked.CompareExchange(ref Dummy.sectionBlock, new DummySectionBlock(), null);
        return Dummy.sectionBlock;
      }
    }
    private static ISectionBlock/*?*/ sectionBlock;

    public static ISecurityAttribute SecurityAttribute {
      get {
        Contract.Ensures(Contract.Result<ISecurityAttribute>() != null);
        if (Dummy.securityAttribute == null)
          Interlocked.CompareExchange(ref Dummy.securityAttribute, new DummySecurityAttribute(), null);
        return Dummy.securityAttribute;
      }
    }
    private static ISecurityAttribute/*?*/ securityAttribute;

    public static ISignature Signature {
      get {
        Contract.Ensures(Contract.Result<ISignature>() != null);
        if (Dummy.signature == null)
          Interlocked.CompareExchange(ref Dummy.signature, new DummyMethodDefinition(), null);
        return Dummy.signature;
      }
    }
    private static ISignature/*?*/ signature;

    public static ISpecializedNestedTypeDefinition SpecializedNestedTypeDefinition {
      get {
        Contract.Ensures(Contract.Result<ISpecializedNestedTypeDefinition>() != null);
        if (Dummy.specializedNestedTypeDefinition == null)
          Interlocked.CompareExchange(ref Dummy.specializedNestedTypeDefinition, new DummySpecializedNestedTypeDefinition(), null);
        return Dummy.specializedNestedTypeDefinition;
      }
    }
    private static ISpecializedNestedTypeDefinition/*?*/ specializedNestedTypeDefinition;

    public static ISpecializedFieldDefinition SpecializedFieldDefinition {
      get {
        Contract.Ensures(Contract.Result<ISpecializedFieldDefinition>() != null);
        if (Dummy.specializedFieldDefinition == null)
          Interlocked.CompareExchange(ref Dummy.specializedFieldDefinition, new DummySpecializedFieldDefinition(), null);
        return Dummy.specializedFieldDefinition;
      }
    }
    private static ISpecializedFieldDefinition/*?*/ specializedFieldDefinition;

    public static ISpecializedMethodDefinition SpecializedMethodDefinition {
      get {
        Contract.Ensures(Contract.Result<ISpecializedMethodDefinition>() != null);
        if (Dummy.specializedMethodDefinition == null)
          Interlocked.CompareExchange(ref Dummy.specializedMethodDefinition, new DummySpecializedMethodDefinition(), null);
        return Dummy.specializedMethodDefinition;
      }
    }
    private static ISpecializedMethodDefinition/*?*/ specializedMethodDefinition;

    public static ISpecializedPropertyDefinition SpecializedPropertyDefinition {
      get {
        Contract.Ensures(Contract.Result<ISpecializedPropertyDefinition>() != null);
        if (Dummy.specializedPropertyDefinition == null)
          Interlocked.CompareExchange(ref Dummy.specializedPropertyDefinition, new DummySpecializedPropertyDefinition(), null);
        return Dummy.specializedPropertyDefinition;
      }
    }
    private static ISpecializedPropertyDefinition/*?*/ specializedPropertyDefinition;

    public static ILocalDefinition LocalVariable {
      get {
        Contract.Ensures(Contract.Result<ILocalDefinition>() != null);
        if (Dummy.localVariable == null)
          Interlocked.CompareExchange(ref Dummy.localVariable, new DummyLocalVariable(), null);
        return Dummy.localVariable;
      }
    }
    private static ILocalDefinition/*?*/ localVariable;

    public static IFieldReference FieldReference {
      get {
        Contract.Ensures(Contract.Result<IFieldReference>() != null);
        if (Dummy.fieldReference == null)
          Interlocked.CompareExchange(ref Dummy.fieldReference, new DummyFieldReference(), null);
        return Dummy.fieldReference;
      }
    }
    private static IFieldReference/*?*/ fieldReference;

    public static IParameterDefinition ParameterDefinition {
      get {
        Contract.Ensures(Contract.Result<IParameterDefinition>() != null);
        if (Dummy.parameterDefinition == null)
          Interlocked.CompareExchange(ref Dummy.parameterDefinition, new DummyParameterDefinition(), null);
        return Dummy.parameterDefinition;
      }
    }
    private static IParameterDefinition/*?*/ parameterDefinition;

    public static IPlatformInvokeInformation PlatformInvokeInformation {
      get {
        Contract.Ensures(Contract.Result<IPlatformInvokeInformation>() != null);
        if (Dummy.platformInvokeInformation == null)
          Interlocked.CompareExchange(ref Dummy.platformInvokeInformation, new DummyPlatformInvokeInformation(), null);
        return Dummy.platformInvokeInformation;
      }
    }
    private static IPlatformInvokeInformation/*?*/ platformInvokeInformation;

    public static IGlobalMethodDefinition GlobalMethod {
      get {
        Contract.Ensures(Contract.Result<IGlobalMethodDefinition>() != null);
        if (Dummy.globalMethodDefinition == null)
          Interlocked.CompareExchange(ref Dummy.globalMethodDefinition, new DummyGlobalMethodDefinition(), null);
        return Dummy.globalMethodDefinition;
      }
    }
    private static IGlobalMethodDefinition/*?*/ globalMethodDefinition;

    public static IGlobalFieldDefinition GlobalField {
      get {
        Contract.Ensures(Contract.Result<IGlobalFieldDefinition>() != null);
        if (Dummy.globalFieldDefinition == null)
          Interlocked.CompareExchange(ref Dummy.globalFieldDefinition, new DummyGlobalFieldDefinition(), null);
        return Dummy.globalFieldDefinition;
      }
    }
    private static IGlobalFieldDefinition/*?*/ globalFieldDefinition;

    public static IOperation Operation {
      get {
        Contract.Ensures(Contract.Result<IOperation>() != null);
        if (Dummy.operation == null)
          Interlocked.CompareExchange(ref Dummy.operation, new DummyOperation(), null);
        return Dummy.operation;
      }
    }
    private static IOperation/*?*/ operation;

    public static ILocation Location {
      get {
        Contract.Ensures(Contract.Result<ILocation>() != null);
        if (Dummy.location == null)
          Interlocked.CompareExchange(ref Dummy.location, new DummyLocation(), null);
        return Dummy.location;
      }
    }
    private static ILocation/*?*/ location;

    public static IDocument Document {
      get {
        Contract.Ensures(Contract.Result<IDocument>() != null);
        if (Dummy.document == null)
          Interlocked.CompareExchange(ref Dummy.document, new DummyDocument(), null);
        return Dummy.document;
      }
    }
    private static IDocument/*?*/ document;

    public static IOperationExceptionInformation OperationExceptionInformation {
      get {
        Contract.Ensures(Contract.Result<IOperationExceptionInformation>() != null);
        if (Dummy.operationExceptionInformation == null)
          Interlocked.CompareExchange(ref Dummy.operationExceptionInformation, new DummyOperationExceptionInformation(), null);
        return Dummy.operationExceptionInformation;
      }
    }
    private static IOperationExceptionInformation/*?*/ operationExceptionInformation;

    public static IInternFactory InternFactory {
      get {
        Contract.Ensures(Contract.Result<IInternFactory>() != null);
        if (Dummy.internFactory == null)
          Interlocked.CompareExchange(ref Dummy.internFactory, new DummyInternFactory(), null);
        return Dummy.internFactory;
      }
    }
    private static IInternFactory/*?*/ internFactory;

    public static IArrayType ArrayType {
      get {
        Contract.Ensures(Contract.Result<IArrayType>() != null);
        if (Dummy.arrayType == null)
          Interlocked.CompareExchange(ref Dummy.arrayType, new DummyArrayType(), null);
        return Dummy.arrayType;
      }
    }
    private static IArrayType/*?*/ arrayType;

    public static ITypeDefinitionMember TypeDefinitionMember {
      get {
        Contract.Ensures(Contract.Result<ITypeDefinitionMember>() != null);
        if (Dummy.typeDefinitionMember == null)
          Interlocked.CompareExchange(ref Dummy.typeDefinitionMember, new DummyFieldDefinition(), null);
        return Dummy.typeDefinitionMember;
      }
    }
    private static ITypeDefinitionMember/*?*/ typeDefinitionMember;
  }

  [ContractVerification(false)]
  internal sealed class DummyNamespaceAliasForType : Dummy, INamespaceAliasForType {
    #region IAliasForType Members

    public INamedTypeReference AliasedType {
      get { return Dummy.NamedTypeReference; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    #endregion

    #region IContainer<IAliasMember> Members

    public IEnumerable<IAliasMember> Members {
      get { return Enumerable<IAliasMember>.Empty; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IScope<IAliasMember> Members

    public bool Contains(IAliasMember member) {
      return false;
    }

    public IEnumerable<IAliasMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<IAliasMember, bool> predicate) {
      return Enumerable<IAliasMember>.Empty;
    }

    public IEnumerable<IAliasMember> GetMatchingMembers(Function<IAliasMember, bool> predicate) {
      return Enumerable<IAliasMember>.Empty;
    }

    public IEnumerable<IAliasMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<IAliasMember>.Empty;
    }

    #endregion

    #region INamespaceAliasForType Members

    public bool IsPublic {
      get { return false; }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return Dummy.NamespaceDefinition; }
    }

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyAssembly : Dummy, IAssembly {
    #region IAssembly Members

    public IEnumerable<ICustomAttribute> AssemblyAttributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public string Culture {
      get { return string.Empty; }
    }

    public IEnumerable<IAliasForType> ExportedTypes {
      get { return Enumerable<IAliasForType>.Empty; }
    }

    public IEnumerable<IResourceReference> Resources {
      get { return Enumerable<IResourceReference>.Empty; }
    }

    public IEnumerable<IFileReference> Files {
      get { return Enumerable<IFileReference>.Empty; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public IEnumerable<IModule> MemberModules {
      get { return Enumerable<IModule>.Empty; }
    }

    public uint Flags {
      get { return 0; }
    }

    public bool ContainsForeignTypes {
      get { return false; }
    }

    public IEnumerable<byte> PublicKey {
      get { return Enumerable<byte>.Empty; }
    }

    public new Version Version {
      get { return Dummy.Version; }
    }

    public new AssemblyIdentity AssemblyIdentity {
      get {
        return Dummy.AssemblyIdentity;
      }
    }

    #endregion

    #region IModule Members

    public IName ModuleName {
      get {
        return Dummy.Name;
      }
    }

    public IAssembly/*?*/ ContainingAssembly {
      get {
        return this;
      }
    }

    public IEnumerable<IAssemblyReference> AssemblyReferences {
      get { return Enumerable<IAssemblyReference>.Empty; }
    }

    public ulong BaseAddress {
      get { return 0; }
    }

    public string DebugInformationLocation {
      get { return string.Empty; }
    }

    public string DebugInformationVersion {
      get { return string.Empty; }
    }

    public ushort DllCharacteristics {
      get { return 0; }
    }

    public IMethodReference EntryPoint {
      get { return Dummy.MethodReference; }
    }

    public uint FileAlignment {
      get { return 0; }
    }

    public bool ILOnly {
      get { return false; }
    }

    public bool StrongNameSigned {
      get { return false; }
    }

    public bool Prefers32bits {
      get { return false; }
    }

    public ModuleKind Kind {
      get { return ModuleKind.ConsoleApplication; }
    }

    public byte LinkerMajorVersion {
      get { return 0; }
    }

    public byte LinkerMinorVersion {
      get { return 0; }
    }

    public byte MetadataFormatMajorVersion {
      get { return 0; }
    }

    public byte MetadataFormatMinorVersion {
      get { return 0; }
    }

    public IEnumerable<ICustomAttribute> ModuleAttributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<IModuleReference> ModuleReferences {
      get { return Enumerable<IModuleReference>.Empty; }
    }

    public Guid PersistentIdentifier {
      get { return Guid.Empty; }
    }

    public Machine Machine {
      get { return Machine.Unknown; }
    }

    public bool RequiresAmdInstructionSet {
      get { return false; }
    }

    public bool RequiresStartupStub {
      get { return false; }
    }

    public bool Requires32bits {
      get { return false; }
    }

    public bool Requires64bits {
      get { return false; }
    }

    public ulong SizeOfHeapReserve {
      get { return 0; }
    }

    public ulong SizeOfHeapCommit {
      get { return 0; }
    }

    public ulong SizeOfStackReserve {
      get { return 0; }
    }

    public ulong SizeOfStackCommit {
      get { return 0; }
    }

    public ushort SubsystemMajorVersion {
      get { return 0; }
    }

    public ushort SubsystemMinorVersion {
      get { return 0; }
    }

    public string TargetRuntimeVersion {
      get { return string.Empty; }
    }

    public bool TrackDebugData {
      get { return false; }
    }

    public bool UsePublicKeyTokensForAssemblyReferences {
      get { return false; }
    }

    public IEnumerable<IWin32Resource> Win32Resources {
      get { return Enumerable<IWin32Resource>.Empty; }
    }

    public IEnumerable<string> GetStrings() {
      return Enumerable<string>.Empty;
    }

    public IEnumerable<INamedTypeDefinition> GetAllTypes() {
      return Enumerable<INamedTypeDefinition>.Empty;
    }

    public IEnumerable<IGenericMethodInstanceReference> GetGenericMethodInstances() {
      return Enumerable<IGenericMethodInstanceReference>.Empty;
    }

    public IEnumerable<ITypeReference> GetStructuralTypeInstances() {
      return Enumerable<ITypeReference>.Empty;
    }

    public IEnumerable<ITypeMemberReference> GetStructuralTypeInstanceMembers() {
      return Enumerable<ITypeMemberReference>.Empty;
    }

    public IEnumerable<ITypeReference> GetTypeReferences() {
      return Enumerable<ITypeReference>.Empty;
    }

    public IEnumerable<ITypeMemberReference> GetTypeMemberReferences() {
      return Enumerable<ITypeMemberReference>.Empty;
    }

    public new ModuleIdentity ModuleIdentity {
      get { return Dummy.ModuleIdentity; }
    }

    #endregion

    #region IUnit Members

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public new string Location {
      get { return string.Empty; }
    }

    public IEnumerable<IPESection> UninterpretedSections {
      get { return Enumerable<IPESection>.Empty; }
    }

    public new IName Name {
      get { return Dummy.Name; }
    }

    public IRootUnitNamespace UnitNamespaceRoot {
      get { return Dummy.RootUnitNamespace; }
    }

    public IEnumerable<IUnitReference> UnitReferences {
      get { return Enumerable<IUnitReference>.Empty; }
    }

    public new UnitIdentity UnitIdentity {
      get { return Dummy.UnitIdentity; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IAssemblyReference)this);
    }

    #endregion

    #region INamespaceRootOwner Members

    public INamespaceDefinition NamespaceRoot {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region IUnitReference Members

    public IUnit ResolvedUnit {
      get { return this; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return null; }
    }

    public IModule ResolvedModule {
      get { return this; }
    }

    #endregion

    #region IAssemblyReference Members

    public IEnumerable<IName> Aliases {
      get { return Enumerable<IName>.Empty; }
    }

    public bool IsRetargetable {
      get { return false; }
    }

    public IAssembly ResolvedAssembly {
      get { return this; }
    }

    public IEnumerable<byte> HashValue {
      get { return Enumerable<byte>.Empty; }
    }

    public IEnumerable<byte> PublicKeyToken {
      get { return Enumerable<byte>.Empty; }
    }

    public AssemblyIdentity UnifiedAssemblyIdentity {
      get { return this.AssemblyIdentity; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyMetadataHost : Dummy, IMetadataHost {

    #region ICompilationHostEnvironment Members

    public event EventHandler<ErrorEventArgs> Errors;

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public AssemblyIdentity SystemCoreAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public IAssembly FindAssembly(AssemblyIdentity assemblyIdentity) {
      return Dummy.Assembly;
    }

    public IModule FindModule(ModuleIdentity moduleIdentity) {
      return Dummy.Module;
    }

    public IUnit FindUnit(UnitIdentity unitIdentity) {
      return Dummy.Unit;
    }

    public IAssembly LoadAssembly(AssemblyIdentity assemblyIdentity) {
      return Dummy.Assembly;
    }

    public IModule LoadModule(ModuleIdentity moduleIdentity) {
      return Dummy.Module;
    }

    public IUnit LoadUnit(UnitIdentity unitIdentity) {
      return Dummy.Unit;
    }

    public IUnit LoadUnitFrom(string location) {
      return Dummy.Unit;
    }

    public new INameTable NameTable {
      get { return Dummy.NameTable; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public byte PointerSize {
      get { return 4; }
    }

    public void ReportErrors(ErrorEventArgs errorEventArguments) {
      if (this.Errors != null)
        this.Errors(this, errorEventArguments); //Do this only to shut up warning about not using this.Errors
    }

    public void ReportError(IErrorMessage error) {
    }

    [Pure]
    public AssemblyIdentity ProbeAssemblyReference(IUnit unit, AssemblyIdentity referedAssemblyIdentity) {
      return referedAssemblyIdentity;
    }

    [Pure]
    public ModuleIdentity ProbeModuleReference(IUnit unit, ModuleIdentity referedModuleIdentity) {
      return referedModuleIdentity;
    }

    [Pure]
    public AssemblyIdentity UnifyAssembly(AssemblyIdentity assemblyIdentity) {
      return assemblyIdentity;
    }

    [Pure]
    public AssemblyIdentity UnifyAssembly(IAssemblyReference assemblyReference) {
      return assemblyReference.AssemblyIdentity;
    }

    public IEnumerable<IUnit> LoadedUnits {
      get { return Enumerable<IUnit>.Empty; }
    }

    public new IInternFactory InternFactory {
      get { return Dummy.InternFactory; }
    }

    public bool PreserveILLocations {
      get { return false; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyMetadataConstant : Dummy, IMetadataConstant {

    #region IMetadataConstant Members

    public object/*?*/ Value {
      get { return null; }
    }

    #endregion

    #region IMetadataExpression Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyCustomAttribute : Dummy, ICustomAttribute {
    #region ICustomAttribute Members

    public IEnumerable<IMetadataExpression> Arguments {
      get { return Enumerable<IMetadataExpression>.Empty; }
    }

    public IMethodReference Constructor {
      get { return Dummy.MethodReference; }
    }

    public IEnumerable<IMetadataNamedArgument> NamedArguments {
      get { return Enumerable<IMetadataNamedArgument>.Empty; }
    }

    public ushort NumberOfNamedArguments {
      get { return 0; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyCustomModifier : Dummy, ICustomModifier {

    #region ICustomModifier Members

    public bool IsOptional {
      get { return false; }
    }

    public ITypeReference Modifier {
      get { return Dummy.TypeReference; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyEventDefinition : Dummy, IEventDefinition {

    #region IEventDefinition Members

    public IEnumerable<IMethodReference> Accessors {
      get { return Enumerable<IMethodReference>.Empty; }
    }

    public IMethodReference Adder {
      get { return Dummy.MethodReference; }
    }

    public IMethodReference/*?*/ Caller {
      get { return null; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public IMethodReference Remover {
      get { return Dummy.MethodReference; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Other; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyMetadataExpression : Dummy, IMetadataExpression {

    #region IMetadataExpression Members

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyFieldDefinition : Dummy, IFieldDefinition {

    #region IFieldDefinition Members

    public uint BitLength {
      get { return 0; }
    }

    public bool IsBitField {
      get { return false; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsCompileTimeConstant {
      get { return false; }
    }

    public bool IsMapped {
      get { return false; }
    }

    public bool IsMarshalledExplicitly {
      get { return false; }
    }

    public bool IsModified {
      get { return false; }
    }

    public bool IsNotSerialized {
      get { return true; }
    }

    public bool IsReadOnly {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public ISectionBlock FieldMapping {
      get { return Dummy.SectionBlock; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public uint Offset {
      get { return 0; }
    }

    public int SequenceNumber {
      get { return 0; }
    }

    public IMetadataConstant CompileTimeValue {
      get { return Dummy.Constant; }
    }

    public new IMarshallingInformation MarshallingInformation {
      get {
        //^ assume false;
        IMarshallingInformation/*?*/ dummyValue = null;
        //^ assume dummyValue != null;
        return dummyValue;
      }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Other; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IFieldReference)this);
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region IFieldReference Members

    public uint InternedKey {
      get { return 0; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public IFieldDefinition ResolvedField {
      get { return this; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }

    #endregion

    #region IMetadataConstantContainer

    public new IMetadataConstant Constant {
      get { return Dummy.Constant; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyFileReference : Dummy, IFileReference {
    #region IFileReference Members

    public IAssembly ContainingAssembly {
      get { return Dummy.Assembly; }
    }

    public bool HasMetadata {
      get { return false; }
    }

    public IName FileName {
      get { return Dummy.Name; }
    }

    public IEnumerable<byte> HashValue {
      get { return Enumerable<byte>.Empty; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyGenericTypeInstance : Dummy, IGenericTypeInstance {

    #region IGenericTypeInstance Members

    public IEnumerable<ITypeReference> GenericArguments {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public INamedTypeReference GenericType {
      get { return Dummy.NamedTypeReference; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get {
        //^ assume false;
        return 0;
      }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get
        //^ ensures result == false;
      {
        return false;
      }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region ITypeReference Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericTypeInstanceReference)this);
    }

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyGenericTypeParameter : Dummy, IGenericTypeParameter {

    #region IGenericTypeParameter Members

    public ITypeDefinition DefiningType {
      get { return Dummy.TypeDefinition; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IGenericParameter Members

    public IEnumerable<ITypeReference> Constraints {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool MustBeReferenceType {
      get { return false; }
    }

    public bool MustBeValueType {
      get { return false; }
    }

    public bool MustHaveDefaultConstructor {
      get { return false; }
    }

    public TypeParameterVariance Variance {
      get { return TypeParameterVariance.NonVariant; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get {
        //^ assume false;
        return 0;
      }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericTypeParameterReference)this);
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return 0; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion

    #region IGenericTypeParameterReference Members

    ITypeReference IGenericTypeParameterReference.DefiningType {
      get { return Dummy.TypeReference; }
    }

    IGenericTypeParameter IGenericTypeParameterReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyGenericMethodParameter : Dummy, IGenericMethodParameter {
    #region IGenericMethodParameter Members

    public IMethodDefinition DefiningMethod {
      get { return Dummy.MethodDefinition; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IGenericParameter Members

    public IEnumerable<ITypeReference> Constraints {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool MustBeReferenceType {
      get { return false; }
    }

    public bool MustBeValueType {
      get { return false; }
    }

    public bool MustHaveDefaultConstructor {
      get { return false; }
    }

    public TypeParameterVariance Variance {
      get { return TypeParameterVariance.NonVariant; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get {
        //^ assume false;
        return 0;
      }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IGenericMethodParameterReference)this);
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return 0; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion

    #region IGenericMethodParameterReference Members

    IMethodReference IGenericMethodParameterReference.DefiningMethod {
      get { return Dummy.MethodReference; }
    }

    IGenericMethodParameter IGenericMethodParameterReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyMethodBody : Dummy, IMethodBody {

    #region IMethodBody Members

    public new IMethodDefinition MethodDefinition {
      get { return Dummy.MethodDefinition; }
    }

    //public IOperation GetOperationAt(int offset, out int offsetOfNextOperation) {
    //  offsetOfNextOperation = -1;
    //  return Dummy.Operation;
    //}

    public IEnumerable<ILocalDefinition> LocalVariables {
      get { return Enumerable<ILocalDefinition>.Empty; }
    }

    public bool LocalsAreZeroed {
      get { return false; }
    }

    public IEnumerable<IOperation> Operations {
      get { return Enumerable<IOperation>.Empty; }
    }

    public IEnumerable<ITypeDefinition> PrivateHelperTypes {
      get { return Enumerable<ITypeDefinition>.Empty; }
    }

    public ushort MaxStack {
      get { return 0; }
    }

    public new IEnumerable<IOperationExceptionInformation> OperationExceptionInformation {
      get { return Enumerable<IOperationExceptionInformation>.Empty; }
    }

    public uint Size {
      get { return 0; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyMethodDefinition : Dummy, IMethodDefinition {

    #region IMethodDefinition Members

    public IMethodBody Body {
      get { return Dummy.MethodBody; }
    }

    public IEnumerable<IGenericMethodParameter> GenericParameters {
      get { return Enumerable<IGenericMethodParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get {
        //^ assume false;
        return 0;
      }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    public bool HasExplicitThisParameter {
      get { return false; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsAccessCheckedOnOverride {
      get { return false; }
    }

    public bool IsCil {
      get { return false; }
    }

    public bool IsConstructor {
      get { return false; }
    }

    public bool IsStaticConstructor {
      get { return false; }
    }

    public bool IsExternal {
      get { return false; }
    }

    public bool IsForwardReference {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsHiddenBySignature {
      get { return false; }
    }

    public bool IsNativeCode {
      get { return false; }
    }

    public bool IsNewSlot {
      get { return false; }
    }

    public bool IsNeverInlined {
      get { return false; }
    }

    public bool IsAggressivelyInlined {
      get { return false; }
    }

    public bool IsNeverOptimized {
      get { return false; }
    }

    public bool IsPlatformInvoke {
      get { return false; }
    }

    public bool IsRuntimeImplemented {
      get { return false; }
    }

    public bool IsRuntimeInternal {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsSynchronized {
      get { return false; }
    }

    public bool IsVirtual {
      get { return false; }
    }

    public bool IsUnmanaged {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public CallingConvention CallingConvention {
      get { return CallingConvention.Default; }
    }

    public bool PreserveSignature {
      get { return false; }
    }

    public IPlatformInvokeInformation PlatformInvokeData {
      get { return Dummy.PlatformInvokeInformation; }
    }

    public bool RequiresSecurityObject {
      get { return false; }
    }

    public IEnumerable<ICustomAttribute> ReturnValueAttributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    public bool ReturnValueIsMarshalledExplicitly {
      get { return false; }
    }

    public IMarshallingInformation ReturnValueMarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public IName ReturnValueName {
      get { return Dummy.Name; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    #endregion

    #region ISignature Members

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get { return Enumerable<IParameterDefinition>.Empty; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Default; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IMethodReference)this);
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ISignature Members


    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region IMethodReference Members

    public bool AcceptsExtraArguments {
      get { return false; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    public ushort ParameterCount {
      get { return 0; }
    }

    public IMethodDefinition ResolvedMethod {
      get { return this; }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyMethodReference : Dummy, IMethodReference {
    #region IMethodReference Members

    public bool AcceptsExtraArguments {
      get { return false; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public ushort ParameterCount {
      get { return 0; }
    }

    public IMethodDefinition ResolvedMethod {
      get { return Dummy.MethodDefinition; }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region ISignature Members

    public CallingConvention CallingConvention {
      get { return CallingConvention.C; }
    }

    public IEnumerable<IParameterTypeInformation> Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyModule : Dummy, IModule {
    #region IModule Members

    public IName ModuleName {
      get { return Dummy.Name; }
    }

    public IAssembly/*?*/ ContainingAssembly {
      get { return null; }
    }

    public IEnumerable<IAssemblyReference> AssemblyReferences {
      get { return Enumerable<IAssemblyReference>.Empty; }
    }

    public ulong BaseAddress {
      get { return 0; }
    }

    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public string DebugInformationLocation {
      get { return string.Empty; }
    }

    public string DebugInformationVersion {
      get { return string.Empty; }
    }

    public ushort DllCharacteristics {
      get { return 0; }
    }

    public IMethodReference EntryPoint {
      get { return Dummy.MethodReference; }
    }

    public uint FileAlignment {
      get { return 0; }
    }

    public bool ILOnly {
      get { return false; }
    }

    public bool StrongNameSigned {
      get { return false; }
    }

    public bool Prefers32bits {
      get { return false; }
    }

    public ModuleKind Kind {
      get { return ModuleKind.ConsoleApplication; }
    }

    public byte LinkerMajorVersion {
      get { return 0; }
    }

    public byte LinkerMinorVersion {
      get { return 0; }
    }

    public byte MetadataFormatMajorVersion {
      get { return 0; }
    }

    public byte MetadataFormatMinorVersion {
      get { return 0; }
    }

    public IEnumerable<ICustomAttribute> ModuleAttributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<IModuleReference> ModuleReferences {
      get { return Enumerable<IModuleReference>.Empty; }
    }

    public Guid PersistentIdentifier {
      get { return Guid.Empty; }
    }

    public Machine Machine {
      get { return Machine.Unknown; }
    }

    public bool RequiresAmdInstructionSet {
      get { return false; }
    }

    public bool RequiresStartupStub {
      get { return false; }
    }

    public bool Requires32bits {
      get { return false; }
    }

    public bool Requires64bits {
      get { return false; }
    }

    public ulong SizeOfHeapReserve {
      get { return 0; }
    }

    public ulong SizeOfHeapCommit {
      get { return 0; }
    }

    public ulong SizeOfStackReserve {
      get { return 0; }
    }

    public ulong SizeOfStackCommit {
      get { return 0; }
    }

    public ushort SubsystemMajorVersion {
      get { return 0; }
    }

    public ushort SubsystemMinorVersion {
      get { return 0; }
    }

    public string TargetRuntimeVersion {
      get { return string.Empty; }
    }

    public bool TrackDebugData {
      get { return false; }
    }

    public bool UsePublicKeyTokensForAssemblyReferences {
      get { return false; }
    }

    public IEnumerable<IWin32Resource> Win32Resources {
      get { return Enumerable<IWin32Resource>.Empty; }
    }

    public IEnumerable<string> GetStrings() {
      return Enumerable<string>.Empty;
    }

    public IEnumerable<INamedTypeDefinition> GetAllTypes() {
      return Enumerable<INamedTypeDefinition>.Empty;
    }

    public IEnumerable<IGenericMethodInstanceReference> GetGenericMethodInstances() {
      return Enumerable<IGenericMethodInstanceReference>.Empty;
    }

    public IEnumerable<ITypeReference> GetStructuralTypeInstances() {
      return Enumerable<ITypeReference>.Empty;
    }

    public IEnumerable<ITypeMemberReference> GetStructuralTypeInstanceMembers() {
      return Enumerable<ITypeMemberReference>.Empty;
    }

    public IEnumerable<ITypeReference> GetTypeReferences() {
      return Enumerable<ITypeReference>.Empty;
    }

    public IEnumerable<ITypeMemberReference> GetTypeMemberReferences() {
      return Enumerable<ITypeMemberReference>.Empty;
    }

    public new ModuleIdentity ModuleIdentity {
      get { return Dummy.ModuleIdentity; }
    }

    #endregion

    #region IUnit Members

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public new string Location {
      get { return string.Empty; }
    }

    public IEnumerable<IPESection> UninterpretedSections {
      get { return Enumerable<IPESection>.Empty; }
    }

    public new IName Name {
      get { return Dummy.Name; }
    }

    public IRootUnitNamespace UnitNamespaceRoot {
      get { return Dummy.RootUnitNamespace; }
    }

    public IEnumerable<IUnitReference> UnitReferences {
      get { return Enumerable<IUnitReference>.Empty; }
    }

    public new UnitIdentity UnitIdentity {
      get { return Dummy.UnitIdentity; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IModuleReference)this);
    }

    #endregion

    #region INamespaceRootOwner Members

    public INamespaceDefinition NamespaceRoot {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region IUnitReference Members

    public IUnit ResolvedUnit {
      get { return Dummy.Unit; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      get { return null; }
    }

    public IModule ResolvedModule {
      get { return Dummy.Module; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyModuleReference : Dummy, IModuleReference {

    #region IUnitReference Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IModuleReference Members

    public IAssemblyReference/*?*/ ContainingAssembly {
      get { return null; }
    }

    public IModule ResolvedModule {
      get { return Dummy.Module; }
    }

    #endregion

    #region IUnitReference Members

    public IUnit ResolvedUnit {
      get { return Dummy.Unit; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IModuleReference Members

    public new ModuleIdentity ModuleIdentity {
      get { return Dummy.ModuleIdentity; }
    }

    #endregion

    #region IUnitReference Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public new UnitIdentity UnitIdentity {
      get { return Dummy.UnitIdentity; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyName : Dummy, IName {

    #region IName Members

    public int UniqueKey {
      get { return 1; }
    }

    public int UniqueKeyIgnoringCase {
      get { return 1; }
    }

    public string Value {
      get { return string.Empty; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyNamedArgument : Dummy, IMetadataNamedArgument {
    #region IMetadataNamedArgument Members

    public IName ArgumentName {
      get { return Dummy.Name; }
    }

    public IMetadataExpression ArgumentValue {
      get { return Dummy.Expression; }
    }

    public bool IsField {
      get { return false; }
    }

    public object ResolvedDefinition {
      get { return Dummy.PropertyDefinition; }
    }

    #endregion

    #region IMetadataExpression Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNamespaceTypeDefinition : Dummy, INamespaceTypeDefinition {
    #region INamespaceTypeDefinition Members

    public IUnitNamespace ContainingUnitNamespace {
      get { return Dummy.UnitNamespace; }
    }

    public bool IsPublic {
      get { return false; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get {
        //^ assume false;
        return 0;
      }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return true; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return Dummy.NamespaceDefinition; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INamespaceTypeReference)this);
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
    }


    public uint InternedKey {
      get { return 0; }
    }

    #endregion

    #region INamespaceTypeReference Members

    IUnitNamespaceReference INamespaceTypeReference.ContainingUnitNamespace {
      get { return this.ContainingUnitNamespace; }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get { return this; }
    }

    public bool KeepDistinctFromDefinition {
      get { return false; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion

    #region INamedTypeDefinition Members

    public IEnumerable<ICustomAttribute> AttributesFor(ITypeReference implementedInterface) {
      return Enumerable<ICustomAttribute>.Empty;
    }

    public bool IsForeignObject {
      get { return false; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNamespaceTypeReference : Dummy, INamespaceTypeReference {

    #region INamespaceTypeReference Members

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public IUnitNamespaceReference ContainingUnitNamespace {
      get { return Dummy.UnitNamespaceReference; }
    }

    INamespaceTypeDefinition INamespaceTypeReference.ResolvedType {
      get { return Dummy.NamespaceTypeDefinition; }
    }

    public bool KeepDistinctFromDefinition {
      get { return false; }
    }

    #endregion

    #region ITypeReference Members

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return Dummy.TypeDefinition; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    public bool IsAlias {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return Dummy.NamedTypeDefinition; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNameTable : Dummy, INameTable {

    #region INameTable Members

    public IName Address {
      get { return Dummy.Name; }
    }

    public IName AllowMultiple {
      get { return Dummy.Name; }
    }

    public IName BeginInvoke {
      get { return Dummy.Name; }
    }

    public IName BoolOpBool {
      get { return Dummy.Name; }
    }

    public IName DecimalOpDecimal {
      get { return Dummy.Name; }
    }

    public IName DelegateOpDelegate {
      get { return Dummy.Name; }
    }

    public IName Cctor {
      get { return Dummy.Name; }
    }

    public IName Ctor {
      get { return Dummy.Name; }
    }

    public IName EmptyName {
      get { return Dummy.Name; }
    }

    public IName EndInvoke {
      get { return Dummy.Name; }
    }

    public IName EnumOpEnum {
      get { return Dummy.Name; }
    }

    public IName EnumOpNum {
      get { return Dummy.Name; }
    }

    public new IName Equals {
      get { return Dummy.Name; }
    }

    public IName Float32OpFloat32 {
      get { return Dummy.Name; }
    }

    public IName Float64OpFloat64 {
      get { return Dummy.Name; }
    }

    public IName Get {
      get { return Dummy.Name; }
    }

    [Pure]
    public IName GetNameFor(string name) {
      return Dummy.Name;
    }

    public IName global {
      get { return Dummy.Name; }
    }

    public IName HasValue {
      get { return Dummy.Name; }
    }

    public IName Inherited {
      get { return Dummy.Name; }
    }

    public IName Invoke {
      get { return Dummy.Name; }
    }

    public IName Int16OpInt16 {
      get { return Dummy.Name; }
    }

    public IName Int32OpInt32 {
      get { return Dummy.Name; }
    }

    public IName Int32OpUInt32 {
      get { return Dummy.Name; }
    }

    public IName Int64OpInt32 {
      get { return Dummy.Name; }
    }

    public IName Int64OpUInt32 {
      get { return Dummy.Name; }
    }

    public IName Int64OpUInt64 {
      get { return Dummy.Name; }
    }

    public IName Int64OpInt64 {
      get { return Dummy.Name; }
    }

    public IName Int8OpInt8 {
      get { return Dummy.Name; }
    }

    public IName NullCoalescing {
      get { return Dummy.Name; }
    }

    public IName NumOpEnum {
      get { return Dummy.Name; }
    }

    public IName ObjectOpObject {
      get { return Dummy.Name; }
    }

    public IName ObjectOpString {
      get { return Dummy.Name; }
    }

    public IName OpAddition {
      get { return Dummy.Name; }
    }

    public IName OpBoolean {
      get { return Dummy.Name; }
    }

    public IName OpChar {
      get { return Dummy.Name; }
    }

    public IName OpDecimal {
      get { return Dummy.Name; }
    }

    public IName OpEnum {
      get { return Dummy.Name; }
    }

    public IName OpEquality {
      get { return Dummy.Name; }
    }

    public IName OpInequality {
      get { return Dummy.Name; }
    }

    public IName OpInt8 {
      get { return Dummy.Name; }
    }

    public IName OpInt16 {
      get { return Dummy.Name; }
    }

    public IName OpInt32 {
      get { return Dummy.Name; }
    }

    public IName OpInt64 {
      get { return Dummy.Name; }
    }

    public IName OpBitwiseAnd {
      get { return Dummy.Name; }
    }

    public IName OpBitwiseOr {
      get { return Dummy.Name; }
    }

    public IName OpComma {
      get { return Dummy.Name; }
    }

    public IName OpConcatentation {
      get { return Dummy.Name; }
    }

    public IName OpDivision {
      get { return Dummy.Name; }
    }

    public IName OpExclusiveOr {
      get { return Dummy.Name; }
    }

    public IName OpExplicit {
      get { return Dummy.Name; }
    }

    public IName OpExponentiation {
      get { return Dummy.Name; }
    }

    public IName OpFalse {
      get { return Dummy.Name; }
    }

    public IName OpFloat32 {
      get { return Dummy.Name; }
    }

    public IName OpFloat64 {
      get { return Dummy.Name; }
    }

    public IName OpGreaterThan {
      get { return Dummy.Name; }
    }

    public IName OpGreaterThanOrEqual {
      get { return Dummy.Name; }
    }

    public IName OpImplicit {
      get { return Dummy.Name; }
    }

    public IName OpIntegerDivision {
      get { return Dummy.Name; }
    }

    public IName OpLeftShift {
      get { return Dummy.Name; }
    }

    public IName OpLessThan {
      get { return Dummy.Name; }
    }

    public IName OpLessThanOrEqual {
      get { return Dummy.Name; }
    }

    public IName OpLike {
      get { return Dummy.Name; }
    }

    public IName OpLogicalNot {
      get { return Dummy.Name; }
    }

    public IName OpLogicalOr {
      get { return Dummy.Name; }
    }

    public IName OpModulus {
      get { return Dummy.Name; }
    }

    public IName OpMultiply {
      get { return Dummy.Name; }
    }

    public IName OpOnesComplement {
      get { return Dummy.Name; }
    }

    public IName OpDecrement {
      get { return Dummy.Name; }
    }

    public IName OpIncrement {
      get { return Dummy.Name; }
    }

    public IName OpRightShift {
      get { return Dummy.Name; }
    }

    public IName OpSubtraction {
      get { return Dummy.Name; }
    }

    public IName OpTrue {
      get { return Dummy.Name; }
    }

    public IName OpUInt8 {
      get { return Dummy.Name; }
    }

    public IName OpUInt16 {
      get { return Dummy.Name; }
    }

    public IName OpUInt32 {
      get { return Dummy.Name; }
    }

    public IName OpUInt64 {
      get { return Dummy.Name; }
    }

    public IName OpUnaryNegation {
      get { return Dummy.Name; }
    }

    public IName OpUnaryPlus {
      get { return Dummy.Name; }
    }

    public IName StringOpObject {
      get { return Dummy.Name; }
    }

    public IName StringOpString {
      get { return Dummy.Name; }
    }

    public IName value {
      get { return Dummy.Name; }
    }

    public IName UIntPtrOpUIntPtr {
      get { return Dummy.Name; }
    }

    public IName UInt32OpInt32 {
      get { return Dummy.Name; }
    }

    public IName UInt32OpUInt32 {
      get { return Dummy.Name; }
    }

    public IName UInt64OpInt32 {
      get { return Dummy.Name; }
    }

    public IName UInt64OpUInt32 {
      get { return Dummy.Name; }
    }

    public IName UInt64OpUInt64 {
      get { return Dummy.Name; }
    }

    public IName System {
      get { return Dummy.Name; }
    }

    public IName Void {
      get { return Dummy.Name; }
    }

    public IName VoidPtrOpVoidPtr {
      get { return Dummy.Name; }
    }

    public IName Boolean {
      get { return Dummy.Name; }
    }

    public IName Char {
      get { return Dummy.Name; }
    }

    public IName Byte {
      get { return Dummy.Name; }
    }

    public IName SByte {
      get { return Dummy.Name; }
    }

    public IName Int16 {
      get { return Dummy.Name; }
    }

    public IName UInt16 {
      get { return Dummy.Name; }
    }

    public IName Int32 {
      get { return Dummy.Name; }
    }

    public IName UInt32 {
      get { return Dummy.Name; }
    }

    public IName Int64 {
      get { return Dummy.Name; }
    }

    public IName UInt64 {
      get { return Dummy.Name; }
    }

    public IName String {
      get { return Dummy.Name; }
    }

    public IName IntPtr {
      get { return Dummy.Name; }
    }

    public IName UIntPtr {
      get { return Dummy.Name; }
    }

    public IName Object {
      get { return Dummy.Name; }
    }

    public IName Set {
      get { return Dummy.Name; }
    }

    public IName Single {
      get { return Dummy.Name; }
    }

    public IName Double {
      get { return Dummy.Name; }
    }

    public IName TypedReference {
      get { return Dummy.Name; }
    }

    public IName Enum {
      get { return Dummy.Name; }
    }

    public IName MulticastDelegate {
      get { return Dummy.Name; }
    }

    public IName ValueType {
      get { return Dummy.Name; }
    }

    public new IName Type {
      get { return Dummy.Name; }
    }

    public IName Array {
      get { return Dummy.Name; }
    }

    public IName AttributeUsageAttribute {
      get { return Dummy.Name; }
    }

    public IName Attribute {
      get { return Dummy.Name; }
    }

    public IName Combine {
      get { return Dummy.Name; }
    }

    public IName Concat {
      get { return Dummy.Name; }
    }

    public IName DateTime {
      get { return Dummy.Name; }
    }

    public IName DebuggerHiddenAttribute {
      get { return Dummy.Name; }
    }

    public IName Decimal {
      get { return Dummy.Name; }
    }

    public IName Delegate {
      get { return Dummy.Name; }
    }

    public IName Diagnostics {
      get { return Dummy.Name; }
    }

    public IName DBNull {
      get { return Dummy.Name; }
    }

    public IName Length {
      get { return Dummy.Name; }
    }

    public IName LongLength {
      get { return Dummy.Name; }
    }

    public IName Nullable {
      get { return Dummy.Name; }
    }

    public IName Remove {
      get { return Dummy.Name; }
    }

    public IName Result {
      get { return Dummy.Name; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNestedType : Dummy, INestedTypeDefinition {

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool DoesNotInheritGenericParameters {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INestedTypeReference)this);
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return true; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Public; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return Dummy.TypeDefinition; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion

    #region ITypeMemberReference Members

    ITypeReference ITypeMemberReference.ContainingType {
      get { return Dummy.TypeReference; }
    }
    #endregion

    #region INestedTypeReference Members

    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { return this; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return this; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNestedTypeReference : Dummy, INestedTypeReference {
    #region INestedTypeReference Members

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public INestedTypeDefinition ResolvedType {
      get { return Dummy.NestedTypeDefinition; }
    }

    #endregion

    #region INamedTypeReference Members


    public bool MangleName {
      get { return false; }
    }

    INamedTypeDefinition INamedTypeReference.ResolvedType {
      get { return Dummy.NamedTypeDefinition; }
    }

    #endregion

    #region ITypeReference Members

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    public bool IsAlias {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return Dummy.TypeDefinition; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyPlatformType : Dummy, IPlatformType {

    #region IPlatformType Members

    public INamespaceTypeReference SystemDiagnosticsContractsContract {
      get { return Dummy.NamespaceTypeReference; }
    }

    public byte PointerSize {
      get { return 4; }
    }

    public INamespaceTypeReference SystemArgIterator {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemArray {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemAttributeUsageAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemAsyncCallback {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemBoolean {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemChar {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsGenericDictionary {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsGenericICollection {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsGenericIEnumerable {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsGenericIEnumerator {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsGenericIList {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsICollection {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsIEnumerable {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsIEnumerator {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsIList {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsIStructuralComparable {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemCollectionsIStructuralEquatable {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemContextStaticAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemIAsyncResult {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemICloneable {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemDateTime {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemDateTimeOffset {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemDecimal {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemDelegate {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemDBNull {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemEnum {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemException {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemFloat32 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemFloat64 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemGlobalizationCultureInfo {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemInt16 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemInt32 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemInt64 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemInt8 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemIntPtr {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemMulticastDelegate {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemNullable {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemObject {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemReflectionAssemblySignatureKeyAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeArgumentHandle {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeFieldHandle {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeMethodHandle {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeTypeHandle {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesCallConvCdecl {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesExtensionAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesInternalsVisibleToAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesIsConst {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesIsVolatile {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeCompilerServicesReferenceAssemblyAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeInteropServicesDllImportAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemRuntimeInteropServicesTypeIdentifierAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemSecurityPermissionsSecurityAction {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemSecuritySecurityCriticalAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemSecuritySecuritySafeCriticalAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemSecuritySuppressUnmanagedCodeSecurityAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemString {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemThreadStaticAttribute {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemType {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemTypedReference {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemUInt16 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemUInt32 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemUInt64 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemUInt8 {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemUIntPtr {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemValueType {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference SystemVoid {
      get { return Dummy.NamespaceTypeReference; }
    }

    public INamespaceTypeReference GetTypeFor(PrimitiveTypeCode typeCode) {
      return Dummy.NamespaceTypeReference;
    }

    #endregion


  }

  [ContractVerification(false)]
  internal sealed class DummyPropertyDefinition : Dummy, IPropertyDefinition {

    #region IPropertyDefinition Members

    public IEnumerable<IMethodReference> Accessors {
      get { return Enumerable<IMethodReference>.Empty; }
    }

    public IMetadataConstant DefaultValue {
      get { return Dummy.Constant; }
    }

    public IMethodReference/*?*/ Getter {
      get { return null; }
    }

    public bool HasDefaultValue {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public IMethodReference/*?*/ Setter {
      get { return null; }
    }

    #endregion

    #region ISignature Members

    public bool IsStatic {
      get { return true; }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get { return Enumerable<IParameterDefinition>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    public CallingConvention CallingConvention {
      get { return CallingConvention.Default; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Other; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ISignature Members


    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region IMetadataConstantContainer

    public new IMetadataConstant Constant {
      get { return Dummy.Constant; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyRootUnitNamespace : Dummy, IRootUnitNamespace {

    #region IUnitNamespace Members

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public new IUnit Unit {
      get { return Dummy.Unit; }
    }

    #endregion

    #region INamespaceDefinition Members

    public INamespaceRootOwner RootOwner {
      get { return Dummy.NamespaceRootOwner; }
    }

    public IEnumerable<INamespaceMember> Members {
      get { return Enumerable<INamespaceMember>.Empty; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IRootUnitNamespaceReference)this);
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScope<INamespaceMember> Members

    public bool Contains(INamespaceMember member) {
      return false;
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      return Enumerable<INamespaceMember>.Empty;
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      return Enumerable<INamespaceMember>.Empty;
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<INamespaceMember>.Empty;
    }

    #endregion

    #region IUnitNamespaceReference Members

    IUnitReference IUnitNamespaceReference.Unit {
      get { return Dummy.UnitReference; }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get { return Dummy.UnitNamespace; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNestedUnitNamespace : Dummy, INestedUnitNamespace {

    #region IUnitNamespace Members

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public new IUnit Unit {
      get { return Dummy.Unit; }
    }

    #endregion

    #region INamespaceDefinition Members

    public INamespaceRootOwner RootOwner {
      get { return Dummy.NamespaceRootOwner; }
    }

    public IEnumerable<INamespaceMember> Members {
      get { return Enumerable<INamespaceMember>.Empty; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScope<INamespaceMember> Members

    public bool Contains(INamespaceMember member) {
      return false;
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      return Enumerable<INamespaceMember>.Empty;
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      return Enumerable<INamespaceMember>.Empty;
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<INamespaceMember>.Empty;
    }

    #endregion

    #region IUnitNamespaceReference Members

    IUnitReference IUnitNamespaceReference.Unit {
      get { return Dummy.UnitReference; }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get { return Dummy.UnitNamespace; }
    }

    #endregion

    #region INestedUnitNamespace Members

    public IUnitNamespace ContainingUnitNamespace {
      get { return Dummy.UnitNamespace; }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return Dummy.NamespaceDefinition; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((INestedUnitNamespaceReference)this);
    }

    #endregion

    #region INestedUnitNamespaceReference Members

    IUnitNamespaceReference INestedUnitNamespaceReference.ContainingUnitNamespace {
      get { return Dummy.UnitNamespaceReference; }
    }

    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get { return this; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyNestedUnitNamespaceReference : Dummy, INestedUnitNamespaceReference {
    #region INestedUnitNamespaceReference Members

    public IUnitNamespaceReference ContainingUnitNamespace {
      get { return Dummy.UnitNamespaceReference; }
    }

    public INestedUnitNamespace ResolvedNestedUnitNamespace {
      get { return Dummy.NestedUnitNamespace; }
    }

    #endregion

    #region IUnitNamespaceReference Members

    public new IUnitReference Unit {
      get { return Dummy.UnitReference; }
    }

    public IUnitNamespace ResolvedUnitNamespace {
      get { return Dummy.UnitNamespace; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region IObjectWithLocations Members

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyUnitSet : Dummy, IUnitSet {

    #region IUnitSet Members

    public bool Contains(IUnit unit) {
      return false;
    }

    public IEnumerable<IUnit> Units {
      get { return Enumerable<IUnit>.Empty; }
    }

    public IUnitSetNamespace UnitSetNamespaceRoot {
      get { return Dummy.UnitSetNamespace; }
    }

    #endregion

    #region INamespaceRootOwner Members

    public INamespaceDefinition NamespaceRoot {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyRootUnitSetNamespace : Dummy, IRootUnitSetNamespace {

    #region IUnitSetNamespace Members

    public new IUnitSet UnitSet {
      get { return Dummy.UnitSet; }
    }

    #endregion

    #region INamespaceDefinition Members

    public INamespaceRootOwner RootOwner {
      get { return Dummy.NamespaceRootOwner; }
    }

    public IEnumerable<INamespaceMember> Members {
      get { return Enumerable<INamespaceMember>.Empty; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScope<INamespaceMember> Members

    public bool Contains(INamespaceMember member) {
      return false;
    }

    public IEnumerable<INamespaceMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<INamespaceMember, bool> predicate) {
      return Enumerable<INamespaceMember>.Empty;
    }

    public IEnumerable<INamespaceMember> GetMatchingMembers(Function<INamespaceMember, bool> predicate) {
      return Enumerable<INamespaceMember>.Empty;
    }

    public IEnumerable<INamespaceMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<INamespaceMember>.Empty;
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyResource : Dummy, IResource {
    #region IResource Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<byte> Data {
      get { return Enumerable<byte>.Empty; }
    }

    public IAssemblyReference DefiningAssembly {
      get { return Dummy.AssemblyReference; }
    }

    public bool IsInExternalFile {
      get { return false; }
    }

    public IFileReference ExternalFile {
      get { return Dummy.FileReference; }
    }

    public bool IsPublic {
      get { return false; }
    }

    public new IName Name {
      get { return Dummy.Name; }
    }

    public new IResource Resource {
      get { return this; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyAssemblyReference : Dummy, IAssemblyReference {
    #region IAssemblyReference Members

    public IEnumerable<IName> Aliases {
      get { return Enumerable<IName>.Empty; }
    }

    public string Culture {
      get { return string.Empty; }
    }

    public IEnumerable<byte> HashValue {
      get { return Enumerable<byte>.Empty; }
    }

    public IEnumerable<byte> PublicKey {
      get { return Enumerable<byte>.Empty; }
    }

    public IEnumerable<byte> PublicKeyToken {
      get { return Enumerable<byte>.Empty; }
    }

    public new Version Version {
      get { return new Version(0, 0); }
    }

    #endregion

    #region IUnitReference Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IAssemblyReference Members

    public IAssembly ResolvedAssembly {
      get { return Dummy.Assembly; }
    }

    public new AssemblyIdentity AssemblyIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    public bool IsRetargetable {
      get { return false; }
    }

    public bool ContainsForeignTypes {
      get { return false; }
    }

    public AssemblyIdentity UnifiedAssemblyIdentity {
      get { return Dummy.AssemblyIdentity; }
    }

    #endregion

    #region IModuleReference Members

    public new ModuleIdentity ModuleIdentity {
      get { return Dummy.ModuleIdentity; }
    }

    public IAssemblyReference/*?*/ ContainingAssembly {
      get { return null; }
    }

    public IModule ResolvedModule {
      get { return Dummy.Module; }
    }

    #endregion

    #region IUnitReference Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public new UnitIdentity UnitIdentity {
      get { return Dummy.UnitIdentity; }
    }

    public IUnit ResolvedUnit {
      get { return Dummy.Unit; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyMarshallingInformation : Dummy, IMarshallingInformation {
    #region IMarshallingInformation Members

    public ITypeReference CustomMarshaller {
      get { return Dummy.TypeReference; }
    }

    public string CustomMarshallerRuntimeArgument {
      get { return string.Empty; }
    }

    public System.Runtime.InteropServices.UnmanagedType ElementType {
      get { return System.Runtime.InteropServices.UnmanagedType.Error; }
    }

    public uint IidParameterIndex {
      get { return 0; }
    }

    public System.Runtime.InteropServices.UnmanagedType UnmanagedType {
      get { return System.Runtime.InteropServices.UnmanagedType.Error; }
    }

    public uint NumberOfElements {
      get { return 0; }
    }

    public uint? ParamIndex {
      get { return 0; }
    }

    public System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get { return System.Runtime.InteropServices.VarEnum.VT_VOID; }
    }

    public ITypeReference SafeArrayElementUserDefinedSubtype {
      get { return Dummy.TypeReference; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummySecurityAttribute : Dummy, ISecurityAttribute {
    #region ISecurityAttribute Members

    public SecurityAction Action {
      get { return SecurityAction.LinkDemand; }
    }

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyParameterTypeInformation : Dummy, IParameterTypeInformation {
    #region IParameterTypeInformation Members

    public ISignature ContainingSignature {
      get { return Dummy.Signature; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsByReference {
      get { return false; }
    }

    public bool IsModified {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return 0; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummySpecializedNestedTypeDefinition : Dummy, ISpecializedNestedTypeDefinition {
    #region ISpecializedNestedTypeDefinition Members

    public INestedTypeDefinition UnspecializedVersion {
      get { return Dummy.NestedTypeDefinition; }
    }

    #endregion

    #region ISpecializedNestedTypeReference Members

    INestedTypeReference ISpecializedNestedTypeReference.UnspecializedVersion {
      get { return Dummy.NestedTypeReference; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool DoesNotInheritGenericParameters {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((ISpecializedNestedTypeReference)this);
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return true; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Public; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ITypeReference Members


    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return this; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion

    #region ITypeMemberReference Members

    ITypeReference ITypeMemberReference.ContainingType {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region INestedTypeReference Members

    INestedTypeDefinition INestedTypeReference.ResolvedType {
      get { return Dummy.NestedTypeDefinition; }
    }

    #endregion

    #region ITypeMemberReference Members


    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region INamedTypeReference Members

    public bool MangleName {
      get { return false; }
    }

    public INamedTypeDefinition ResolvedType {
      get { return Dummy.NamedTypeDefinition; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummySpecializedFieldDefinition : Dummy, ISpecializedFieldDefinition {

    #region IFieldDefinition Members

    public uint BitLength {
      get { return 0; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsBitField {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public bool IsCompileTimeConstant {
      get { return false; }
    }

    public bool IsMapped {
      get { return false; }
    }

    public bool IsMarshalledExplicitly {
      get { return false; }
    }

    public bool IsModified {
      get { return false; }
    }

    public bool IsNotSerialized {
      get { return false; }
    }

    public bool IsReadOnly {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public ISectionBlock FieldMapping {
      get { return Dummy.SectionBlock; }
    }

    public uint Offset {
      get { return 0; }
    }

    public int SequenceNumber {
      get { return 0; }
    }

    public IMetadataConstant CompileTimeValue {
      get { return Dummy.Constant; }
    }

    public new IMarshallingInformation MarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((ISpecializedFieldReference)this);
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Default; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ISpecializedFieldDefinition Members

    public IFieldDefinition UnspecializedVersion {
      get { return Dummy.FieldDefinition; }
    }

    #endregion

    #region ISpecializedFieldReference Members

    IFieldReference ISpecializedFieldReference.UnspecializedVersion {
      get { return Dummy.FieldReference; }
    }

    #endregion

    #region IFieldReference Members

    public uint InternedKey {
      get { return 0; }
    }

    public IFieldDefinition ResolvedField {
      get { return Dummy.FieldDefinition; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return this; }
    }

    #endregion

    #region IMetadataConstantContainer

    public new IMetadataConstant Constant {
      get { return Dummy.Constant; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummySpecializedMethodDefinition : Dummy, ISpecializedMethodDefinition {
    #region ISpecializedMethodDefinition Members

    public IMethodDefinition UnspecializedVersion {
      get { return Dummy.MethodDefinition; }
    }

    #endregion

    #region ISpecializedMethodReference Members

    IMethodReference ISpecializedMethodReference.UnspecializedVersion {
      get { return Dummy.MethodReference; }
    }

    #endregion

    #region IMethodDefinition Members

    public bool AcceptsExtraArguments {
      get { return false; }
    }

    public IMethodBody Body {
      get { return Dummy.MethodBody; }
    }

    public IEnumerable<IGenericMethodParameter> GenericParameters {
      get { return Enumerable<IGenericMethodParameter>.Empty; }
    }

    [Pure]
    public ushort GenericParameterCount {
      get { return 0; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    public bool HasExplicitThisParameter {
      get { return false; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsAccessCheckedOnOverride {
      get { return false; }
    }

    public bool IsCil {
      get { return false; }
    }

    public bool IsConstructor {
      get { return false; }
    }

    public bool IsStaticConstructor {
      get { return false; }
    }

    public bool IsExternal {
      get { return false; }
    }

    public bool IsForwardReference {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsHiddenBySignature {
      get { return false; }
    }

    public bool IsNativeCode {
      get { return false; }
    }

    public bool IsNewSlot {
      get { return false; }
    }

    public bool IsNeverInlined {
      get { return false; }
    }

    public bool IsAggressivelyInlined {
      get { return false; }
    }

    public bool IsNeverOptimized {
      get { return false; }
    }

    public bool IsPlatformInvoke {
      get { return false; }
    }

    public bool IsRuntimeImplemented {
      get { return false; }
    }

    public bool IsRuntimeInternal {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsSynchronized {
      get { return false; }
    }

    public bool IsVirtual {
      get { return false; }
    }

    public bool IsUnmanaged {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public CallingConvention CallingConvention {
      get { return CallingConvention.Default; }
    }

    public bool PreserveSignature {
      get { return false; }
    }

    public IPlatformInvokeInformation PlatformInvokeData {
      get { return Dummy.PlatformInvokeInformation; }
    }

    public bool RequiresSecurityObject {
      get { return false; }
    }

    public IEnumerable<ICustomAttribute> ReturnValueAttributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    public bool ReturnValueIsMarshalledExplicitly {
      get { return false; }
    }

    public IMarshallingInformation ReturnValueMarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public IName ReturnValueName {
      get { return Dummy.Name; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    #endregion

    #region ISignature Members

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get { return Enumerable<IParameterDefinition>.Empty; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Default; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((ISpecializedMethodReference)this);
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ISignature Members


    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region IMethodReference Members

    public uint InternedKey {
      get { return 0; }
    }

    public ushort ParameterCount {
      get { return 0; }
    }

    public IMethodDefinition ResolvedMethod {
      get { return Dummy.MethodDefinition; }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummySpecializedPropertyDefinition : Dummy, ISpecializedPropertyDefinition {
    #region ISpecializedPropertyDefinition Members

    public IPropertyDefinition UnspecializedVersion {
      get { return Dummy.PropertyDefinition; }
    }

    #endregion

    #region IPropertyDefinition Members

    public IEnumerable<IMethodReference> Accessors {
      get { return Enumerable<IMethodReference>.Empty; }
    }

    public IMetadataConstant DefaultValue {
      get { return Dummy.Constant; }
    }

    public IMethodReference/*?*/ Getter {
      get { return null; }
    }

    public bool HasDefaultValue {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public IMethodReference/*?*/ Setter {
      get { return null; }
    }

    #endregion

    #region ISignature Members

    public bool IsStatic {
      get { return true; }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get { return Enumerable<IParameterDefinition>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    public CallingConvention CallingConvention {
      get { return CallingConvention.C; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Other; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    public ITypeDefinition Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IDoubleDispatcher Members

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    public IScope<ITypeDefinitionMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ISignature Members

    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region IMetadataConstantContainer

    public new IMetadataConstant Constant {
      get { return Dummy.Constant; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyFunctionPointerType : Dummy, IFunctionPointer {
    #region IFunctionPointer Members

    public CallingConvention CallingConvention {
      get { return CallingConvention.Default; }
    }

    public IEnumerable<IParameterTypeInformation> Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    public IEnumerable<IParameterTypeInformation> ExtraArgumentTypes {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return false; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return true; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IFunctionPointerTypeReference)this);
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region IFunctionPointerTypeReference Members


    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    ITypeDefinition ITypeReference.ResolvedType {
      get { return Dummy.TypeDefinition; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyLocalVariable : Dummy, ILocalDefinition {

    #region ILocalDefinition Members

    public IMetadataConstant CompileTimeValue {
      get { return Dummy.Constant; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
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
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public new IMethodDefinition MethodDefinition {
      get { return Dummy.MethodDefinition; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyFieldReference : Dummy, IFieldReference {
    #region IFieldReference Members

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    public bool IsModified {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    public IFieldDefinition ResolvedField {
      get { return Dummy.FieldDefinition; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region IReference Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyParameterDefinition : Dummy, IParameterDefinition {
    #region IParameterDefinition Members

    public ISignature ContainingSignature {
      get { return Dummy.Signature; }
    }

    public IMetadataConstant DefaultValue {
      get { return Dummy.Constant; }
    }

    public bool HasDefaultValue {
      get { return false; }
    }

    public bool IsIn {
      get { return false; }
    }

    public bool IsMarshalledExplicitly {
      get { return false; }
    }

    public bool IsOptional {
      get { return false; }
    }

    public bool IsOut {
      get { return false; }
    }

    public bool IsParameterArray {
      get { return false; }
    }

    public new IMarshallingInformation MarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public ITypeReference ParamArrayElementType {
      get { return Dummy.TypeReference; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.VisitReference(this);
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region IParameterListEntry Members

    public ushort Index {
      get { return 0; }
    }

    #endregion

    #region IParameterTypeInformation Members

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsByReference {
      get { return false; }
    }

    public bool IsModified {
      get { return false; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region IMetadataConstantContainer

    public new IMetadataConstant Constant {
      get { return Dummy.Constant; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummySectionBlock : Dummy, ISectionBlock {
    #region ISectionBlock Members

    public PESectionKind PESectionKind {
      get { return PESectionKind.Illegal; }
    }

    public uint Offset {
      get { return 0; }
    }

    public uint Size {
      get { return 0; }
    }

    public IEnumerable<byte> Data {
      get { return Enumerable<byte>.Empty; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyPlatformInvokeInformation : Dummy, IPlatformInvokeInformation {

    #region IPlatformInvokeInformation Members

    public IName ImportName {
      get { return Dummy.Name; }
    }

    public IModuleReference ImportModule {
      get { return Dummy.ModuleReference; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Unspecified; }
    }

    public bool NoMangle {
      get { return false; }
    }

    public bool SupportsLastError {
      get { return false; }
    }

    public PInvokeCallingConvention PInvokeCallingConvention {
      get { return PInvokeCallingConvention.CDecl; }
    }

    public bool? UseBestFit {
      get { return null; }
    }

    public bool? ThrowExceptionForUnmappableChar {
      get { return null; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyGlobalMethodDefinition : Dummy, IGlobalMethodDefinition {

    #region ISignature Members

    public bool ReturnValueIsByRef {
      get { return false; }
    }

    public IEnumerable<IParameterDefinition> Parameters {
      get { return Enumerable<IParameterDefinition>.Empty; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public IEnumerable<ICustomAttribute> ReturnValueAttributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public IEnumerable<ICustomModifier> ReturnValueCustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool ReturnValueIsModified {
      get { return false; }
    }

    #endregion

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return Dummy.RootUnitNamespace; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IMethodReference)this);
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IMethodDefinition Members

    public bool AcceptsExtraArguments {
      get { return false; }
    }

    public IMethodBody Body {
      get { return Dummy.MethodBody; }
    }

    public IEnumerable<IGenericMethodParameter> GenericParameters {
      get { return Enumerable<IGenericMethodParameter>.Empty; }
    }

    [Pure]
    public ushort GenericParameterCount {
      get { return 0; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    public bool HasExplicitThisParameter {
      get { return false; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsAccessCheckedOnOverride {
      get { return false; }
    }

    public bool IsCil {
      get { return false; }
    }

    public bool IsConstructor {
      get { return false; }
    }

    public bool IsStaticConstructor {
      get { return false; }
    }

    public bool IsExternal {
      get { return false; }
    }

    public bool IsForwardReference {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsHiddenBySignature {
      get { return false; }
    }

    public bool IsNativeCode {
      get { return false; }
    }

    public bool IsNewSlot {
      get { return false; }
    }

    public bool IsNeverInlined {
      get { return false; }
    }

    public bool IsAggressivelyInlined {
      get { return false; }
    }

    public bool IsNeverOptimized {
      get { return false; }
    }

    public bool IsPlatformInvoke {
      get { return false; }
    }

    public bool IsRuntimeImplemented {
      get { return false; }
    }

    public bool IsRuntimeInternal {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSealed {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsSynchronized {
      get { return false; }
    }

    public bool IsVirtual {
      get { return false; }
    }

    public bool IsUnmanaged {
      get { return false; }
    }

    public CallingConvention CallingConvention {
      get { return CallingConvention.Default; }
    }

    public bool PreserveSignature {
      get { return false; }
    }

    public IPlatformInvokeInformation PlatformInvokeData {
      get { return Dummy.PlatformInvokeInformation; }
    }

    public bool RequiresSecurityObject {
      get { return false; }
    }

    public bool ReturnValueIsMarshalledExplicitly {
      get { return false; }
    }

    public IMarshallingInformation ReturnValueMarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public IName ReturnValueName {
      get { return Dummy.Name; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Other; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    ITypeDefinition IContainerMember<ITypeDefinition>.Container {
      get { return Dummy.TypeDefinition; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    IScope<ITypeDefinitionMember> IScopeMember<IScope<ITypeDefinitionMember>>.ContainingScope {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region ISignature Members


    IEnumerable<IParameterTypeInformation> ISignature.Parameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region IMethodReference Members

    public uint InternedKey {
      get { return 0; }
    }

    public ushort ParameterCount {
      get { return 0; }
    }

    public IMethodDefinition ResolvedMethod {
      get { return this; }
    }

    public IEnumerable<IParameterTypeInformation> ExtraParameters {
      get { return Enumerable<IParameterTypeInformation>.Empty; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyGlobalFieldDefinition : Dummy, IGlobalFieldDefinition {

    #region INamedEntity Members

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion

    #region INamespaceMember Members

    public INamespaceDefinition ContainingNamespace {
      get { return Dummy.NamespaceDefinition; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IFieldReference)this);
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    public INamespaceDefinition Container {
      get { return Dummy.NamespaceDefinition; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    public IScope<INamespaceMember> ContainingScope {
      get { return this.Container; }
    }

    #endregion

    #region IFieldDefinition Members

    public uint BitLength {
      get { return 0; }
    }

    public IEnumerable<ICustomModifier> CustomModifiers {
      get { return Enumerable<ICustomModifier>.Empty; }
    }

    public bool IsBitField {
      get { return false; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public bool IsCompileTimeConstant {
      get { return false; }
    }

    public bool IsMapped {
      get { return false; }
    }

    public bool IsMarshalledExplicitly {
      get { return false; }
    }

    public bool IsModified {
      get { return false; }
    }

    public bool IsNotSerialized {
      get { return true; }
    }

    public bool IsReadOnly {
      get { return false; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsStatic {
      get { return false; }
    }

    public ISectionBlock FieldMapping {
      get { return Dummy.SectionBlock; }
    }

    public uint Offset {
      get { return 0; }
    }

    public int SequenceNumber {
      get { return 0; }
    }

    public IMetadataConstant CompileTimeValue {
      get { return Dummy.Constant; }
    }

    public new IMarshallingInformation MarshallingInformation {
      get { return Dummy.MarshallingInformation; }
    }

    public new ITypeReference Type {
      get { return Dummy.TypeReference; }
    }

    #endregion

    #region ITypeDefinitionMember Members

    public ITypeDefinition ContainingTypeDefinition {
      get { return Dummy.TypeDefinition; }
    }

    public TypeMemberVisibility Visibility {
      get { return TypeMemberVisibility.Other; }
    }

    #endregion

    #region IContainerMember<ITypeDefinition> Members

    ITypeDefinition IContainerMember<ITypeDefinition>.Container {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region IScopeMember<IScope<ITypeDefinitionMember>> Members

    IScope<ITypeDefinitionMember> IScopeMember<IScope<ITypeDefinitionMember>>.ContainingScope {
      get { return this.ContainingTypeDefinition; }
    }

    #endregion

    #region IFieldReference Members

    public uint InternedKey {
      get { return 0; }
    }

    public IFieldDefinition ResolvedField {
      get { return Dummy.FieldDefinition; }
    }

    #endregion

    #region ITypeMemberReference Members

    public ITypeReference ContainingType {
      get { return Dummy.TypeReference; }
    }

    public ITypeDefinitionMember ResolvedTypeDefinitionMember {
      get { return Dummy.TypeDefinitionMember; }
    }

    #endregion

    #region IMetadataConstantContainer

    public new IMetadataConstant Constant {
      get { return Dummy.Constant; }
    }

    #endregion

  }

  [ContractVerification(false)]
  internal sealed class DummyOperation : Dummy, IOperation {
    #region IOperation Members

    public OperationCode OperationCode {
      get { return OperationCode.Nop; }
    }

    public uint Offset {
      get { return 0; }
    }

    public new ILocation Location {
      get { return Dummy.Location; }
    }

    public object/*?*/ Value {
      get { return null; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyDocument : Dummy, IDocument {
    #region IDocument Members

    public new string Location {
      get { return string.Empty; }
    }

    public new IName Name {
      get { return Dummy.Name; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyLocation : Dummy, ILocation {
    #region ILocation Members

    public new IDocument Document {
      get { return Dummy.Document; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyOperationExceptionInformation : Dummy, IOperationExceptionInformation {
    #region IOperationExceptionInformation Members

    public HandlerKind HandlerKind {
      get { return HandlerKind.Illegal; }
    }

    public ITypeReference ExceptionType {
      get { return Dummy.TypeReference; }
    }

    public uint TryStartOffset {
      get { return 0; }
    }

    public uint TryEndOffset {
      get { return 0; }
    }

    public uint FilterDecisionStartOffset {
      get { return 0; }
    }

    public uint HandlerStartOffset {
      get { return 0; }
    }

    public uint HandlerEndOffset {
      get { return 0; }
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyInternFactory : Dummy, IInternFactory {
    #region IInternFactory Members

    public uint GetAssemblyInternedKey(AssemblyIdentity assemblyIdentity) {
      return 0;
    }

    public uint GetModuleInternedKey(ModuleIdentity moduleIdentity) {
      return 0;
    }

    public uint GetFieldInternedKey(IFieldReference fieldReference) {
      return 0;
    }

    public uint GetMethodInternedKey(IMethodReference methodReference) {
      return 0;
    }

    public uint GetVectorTypeReferenceInternedKey(ITypeReference elementTypeReference) {
      return 0;
    }

    public uint GetMatrixTypeReferenceInternedKey(ITypeReference elementTypeReference, int rank, IEnumerable<ulong> sizes, IEnumerable<int> lowerBounds) {
      return 0;
    }

    public uint GetGenericTypeInstanceReferenceInternedKey(ITypeReference genericTypeReference, IEnumerable<ITypeReference> genericArguments) {
      return 0;
    }

    public uint GetPointerTypeReferenceInternedKey(ITypeReference targetTypeReference) {
      return 0;
    }

    public uint GetManagedPointerTypeReferenceInternedKey(ITypeReference targetTypeReference) {
      return 0;
    }

    public uint GetFunctionPointerTypeReferenceInternedKey(CallingConvention callingConvention, IEnumerable<IParameterTypeInformation> parameters, IEnumerable<IParameterTypeInformation> extraArgumentTypes, IEnumerable<ICustomModifier> returnValueCustomModifiers, bool returnValueIsByRef, ITypeReference returnType) {
      return 0;
    }

    public uint GetTypeReferenceInternedKey(ITypeReference typeReference) {
      return 0;
    }

    public uint GetNamespaceTypeReferenceInternedKey(IUnitNamespaceReference containingUnitNamespace, IName typeName, uint genericParameterCount) {
      return 0;
    }

    public uint GetNestedTypeReferenceInternedKey(ITypeReference containingTypeReference, IName typeName, uint genericParameterCount) {
      return 0;
    }

    public uint GetGenericTypeParameterReferenceInternedKey(ITypeReference definingTypeReference, int index) {
      return 0;
    }

    public uint GetModifiedTypeReferenceInternedKey(ITypeReference typeReference, IEnumerable<ICustomModifier> customModifiers) {
      return 0;
    }

    public uint GetGenericMethodParameterReferenceInternedKey(IMethodReference definingMethodReference, int index) {
      return 0;
    }

    #endregion
  }

  [ContractVerification(false)]
  internal sealed class DummyArrayType : Dummy, IArrayType {

    #region ITypeDefinition Members

    public ushort Alignment {
      get { return 0; }
    }

    public IEnumerable<ITypeReference> BaseClasses {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public IEnumerable<IEventDefinition> Events {
      get { return Enumerable<IEventDefinition>.Empty; }
    }

    public IEnumerable<IFieldDefinition> Fields {
      get { return Enumerable<IFieldDefinition>.Empty; }
    }

    public IEnumerable<IMethodDefinition> Methods {
      get { return Enumerable<IMethodDefinition>.Empty; }
    }

    public IEnumerable<INestedTypeDefinition> NestedTypes {
      get { return Enumerable<INestedTypeDefinition>.Empty; }
    }

    public IEnumerable<IPropertyDefinition> Properties {
      get { return Enumerable<IPropertyDefinition>.Empty; }
    }

    public IEnumerable<IMethodImplementation> ExplicitImplementationOverrides {
      get { return Enumerable<IMethodImplementation>.Empty; }
    }

    public IEnumerable<IGenericTypeParameter> GenericParameters {
      get { return Enumerable<IGenericTypeParameter>.Empty; }
    }

    public ushort GenericParameterCount {
      get { return 0; }
    }

    public IGenericTypeInstanceReference InstanceType {
      get { return Dummy.GenericTypeInstanceReference; }
    }

    public IEnumerable<ITypeReference> Interfaces {
      get { return Enumerable<ITypeReference>.Empty; }
    }

    public bool IsAbstract {
      get { return false; }
    }

    public bool IsClass {
      get { return true; }
    }

    public bool IsDelegate {
      get { return false; }
    }

    public bool IsEnum {
      get { return false; }
    }

    public bool IsGeneric {
      get { return false; }
    }

    public bool IsInterface {
      get { return false; }
    }

    public bool IsReferenceType {
      get { return false; }
    }

    public bool IsSealed {
      get { return true; }
    }

    public bool IsStatic {
      get { return true; }
    }

    public bool IsValueType {
      get { return false; }
    }

    public bool IsStruct {
      get { return false; }
    }

    public IEnumerable<ITypeDefinitionMember> Members {
      get { return Enumerable<ITypeDefinitionMember>.Empty; }
    }

    public new IPlatformType PlatformType {
      get { return Dummy.PlatformType; }
    }

    public IEnumerable<ITypeDefinitionMember> PrivateHelperMembers {
      get { return this.Members; }
    }

    public uint SizeOf {
      get { return 0; }
    }

    public IEnumerable<ISecurityAttribute> SecurityAttributes {
      get { return Enumerable<ISecurityAttribute>.Empty; }
    }

    public ITypeReference UnderlyingType {
      get { return Dummy.TypeReference; }
    }

    public PrimitiveTypeCode TypeCode {
      get { return PrimitiveTypeCode.Invalid; }
    }

    public IEnumerable<ILocation> Locations {
      get { return Enumerable<ILocation>.Empty; }
    }

    public LayoutKind Layout {
      get { return LayoutKind.Auto; }
    }

    public bool IsSpecialName {
      get { return false; }
    }

    public bool IsComObject {
      get { return false; }
    }

    public bool IsSerializable {
      get { return false; }
    }

    public bool IsBeforeFieldInit {
      get { return false; }
    }

    public StringFormatKind StringFormat {
      get { return StringFormatKind.Ansi; }
    }

    public bool IsRuntimeSpecial {
      get { return false; }
    }

    public bool HasDeclarativeSecurity {
      get { return false; }
    }

    #endregion

    #region IDefinition Members

    public IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    public void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    public void DispatchAsReference(IMetadataVisitor visitor) {
      visitor.Visit((IArrayTypeReference)this);
    }

    #endregion

    #region IScope<ITypeDefinitionMember> Members

    public bool Contains(ITypeDefinitionMember member) {
      return false;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembersNamed(IName name, bool ignoreCase, Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMatchingMembers(Function<ITypeDefinitionMember, bool> predicate) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    public IEnumerable<ITypeDefinitionMember> GetMembersNamed(IName name, bool ignoreCase) {
      return Enumerable<ITypeDefinitionMember>.Empty;
    }

    #endregion

    #region ITypeReference Members

    public bool IsAlias {
      get { return false; }
    }

    public new IAliasForType AliasForType {
      get { return Dummy.AliasForType; }
    }

    public ITypeDefinition ResolvedType {
      get { return Dummy.TypeDefinition; }
    }

    public uint InternedKey {
      get { return 0; }
    }

    #endregion

    #region IArrayTypeReference Members

    public ITypeReference ElementType {
      get { return Dummy.TypeReference; }
    }

    public bool IsVector {
      get { return true; }
    }

    public IEnumerable<int> LowerBounds {
      get { return Enumerable<int>.Empty; }
    }

    public uint Rank {
      get { return 0; }
    }

    public IEnumerable<ulong> Sizes {
      get { return Enumerable<ulong>.Empty; }
    }

    #endregion
  }
#pragma warning restore 1591
}
