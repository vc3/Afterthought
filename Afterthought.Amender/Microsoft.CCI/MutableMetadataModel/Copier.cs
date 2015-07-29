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
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci.MutableCodeModel {

  /// <summary>
  /// 
  /// </summary>
  public class MetadataShallowCopier {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetHost">An object representing the application that will host the copies made by this copier.</param>
    public MetadataShallowCopier(IMetadataHost targetHost) {
      this.targetHost = targetHost;
      this.internFactory = targetHost.InternFactory;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetHost">An object representing the application that will host the copies made by this copier.</param>
    /// <param name="targetUnit">The unit of metadata into which copies made by this copier will be inserted.</param>
    public MetadataShallowCopier(IMetadataHost targetHost, IUnit targetUnit) {
      this.targetHost = targetHost;
      this.targetUnit = targetUnit;
      this.internFactory = targetHost.InternFactory;
    }

    /// <summary>
    /// An object representing the application that will host the copies made by this copier.
    /// </summary>
    protected IMetadataHost targetHost;

    IUnit/*?*/ targetUnit;
    IInternFactory internFactory;

    MetadataDispatcher Dispatcher {
      get {
        if (this.dispatcher == null)
          this.dispatcher = new MetadataDispatcher() { copier = this };
        return this.dispatcher;
      }
    }
    MetadataDispatcher dispatcher;

#pragma warning disable 1591
    protected class MetadataDispatcher : MetadataVisitor {
      internal MetadataShallowCopier copier;
      internal object result;

      public override void Visit(IArrayTypeReference arrayTypeReference) {
        this.result = this.copier.Copy(arrayTypeReference);
      }

      public override void Visit(IAssemblyReference assemblyReference) {
        this.result = this.copier.Copy(assemblyReference);
      }

      public override void Visit(IEventDefinition eventDefinition) {
        this.result = this.copier.CopyUnspecialized(eventDefinition);
      }

      public override void Visit(IFieldDefinition fieldDefinition) {
        this.result = this.copier.CopyUnspecialized(fieldDefinition);
      }

      public override void Visit(IFieldReference fieldReference) {
        this.result = this.copier.CopyUnspecialized(fieldReference);
      }

      public override void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
        this.result = this.copier.Copy(functionPointerTypeReference);
      }

      public override void Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
        this.result = this.copier.Copy(genericMethodInstanceReference);
      }

      public override void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
        this.result = this.copier.Copy(genericMethodParameterReference);
      }

      public override void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
        this.result = this.copier.Copy(genericTypeInstanceReference);
      }

      public override void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
        this.result = this.copier.Copy(genericTypeParameterReference);
      }

      public override void Visit(IGlobalFieldDefinition globalFieldDefinition) {
        this.result = this.copier.Copy(globalFieldDefinition);
      }

      public override void Visit(IGlobalMethodDefinition globalMethodDefinition) {
        this.result = this.copier.Copy(globalMethodDefinition);
      }

      public override void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
        this.result = this.copier.Copy(managedPointerTypeReference);
      }

      public override void Visit(IMetadataConstant constant) {
        this.result = this.copier.Copy(constant);
      }

      public override void Visit(IMetadataCreateArray createArray) {
        this.result = this.copier.Copy(createArray);
      }

      public override void Visit(IMetadataNamedArgument namedArgument) {
        this.result = this.copier.Copy(namedArgument);
      }

      public override void Visit(IMetadataTypeOf typeOf) {
        this.result = this.copier.Copy(typeOf);
      }

      public override void Visit(IMethodDefinition method) {
        this.result = this.copier.CopyUnspecialized(method);
      }

      public override void Visit(IMethodReference methodReference) {
        this.result = this.copier.CopyUnspecialized(methodReference);
      }

      public override void Visit(IModifiedTypeReference modifiedTypeReference) {
        this.result = this.copier.Copy(modifiedTypeReference);
      }

      public override void Visit(IModuleReference moduleReference) {
        this.result = this.copier.Copy(moduleReference);
      }

      public override void Visit(INamespaceAliasForType namespaceAliasForType) {
        this.result = this.copier.Copy(namespaceAliasForType);
      }

      public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
        this.result = this.copier.Copy(namespaceTypeDefinition);
      }

      public override void Visit(INamespaceTypeReference namespaceTypeReference) {
        this.result = this.copier.Copy(namespaceTypeReference);
      }

      public override void Visit(INestedAliasForType nestedAliasForType) {
        this.result = this.copier.Copy(nestedAliasForType);
      }

      public override void Visit(INestedTypeDefinition nestedTypeDefinition) {
        this.result = this.copier.CopyUnspecialized(nestedTypeDefinition);
      }

      public override void Visit(INestedTypeReference nestedTypeReference) {
        this.result = this.copier.CopyUnspecialized(nestedTypeReference);
      }

      public override void Visit(INestedUnitNamespace nestedUnitNamespace) {
        this.result = this.copier.Copy(nestedUnitNamespace);
      }

      public override void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
        this.result = this.copier.Copy(nestedUnitNamespaceReference);
      }

      public override void Visit(IPointerTypeReference pointerTypeReference) {
        this.result = this.copier.Copy(pointerTypeReference);
      }

      public override void Visit(IPropertyDefinition propertyDefinition) {
        this.result = this.copier.CopyUnspecialized(propertyDefinition);
      }

      public override void Visit(IRootUnitNamespace rootUnitNamespace) {
        this.result = this.copier.Copy(rootUnitNamespace);
      }

      public override void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
        this.result = this.copier.Copy(rootUnitNamespaceReference);
      }

      public override void Visit(ISpecializedEventDefinition specializedEventDefinition) {
        this.result = this.copier.Copy(specializedEventDefinition);
      }

      public override void Visit(ISpecializedFieldDefinition specializedFieldDefinition) {
        this.result = this.copier.Copy(specializedFieldDefinition);
      }

      public override void Visit(ISpecializedFieldReference specializedFieldReference) {
        this.result = this.copier.Copy(specializedFieldReference);
      }

      public override void Visit(ISpecializedMethodDefinition specializedMethodDefinition) {
        this.result = this.copier.Copy(specializedMethodDefinition);
      }

      public override void Visit(ISpecializedMethodReference specializedMethodReference) {
        this.result = this.copier.Copy(specializedMethodReference);
      }

      public override void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
        this.result = this.copier.Copy(specializedNestedTypeDefinition);
      }

      public override void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
        this.result = this.copier.Copy(specializedNestedTypeReference);
      }

      public override void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition) {
        this.result = this.copier.Copy(specializedPropertyDefinition);
      }

    }
#pragma warning restore 1591

    /// <summary>
    /// Returns a shallow copy of the specified alias for type.
    /// </summary>
    public AliasForType Copy(IAliasForType aliasForType) {
      Contract.Requires(aliasForType != null);
      Contract.Ensures(Contract.Result<AliasForType>() != null);

      aliasForType.Dispatch(this.Dispatcher);
      return (AliasForType)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given array type reference.
    /// </summary>
    public ArrayTypeReference Copy(IArrayTypeReference arrayTypeReference) {
      Contract.Requires(arrayTypeReference != null);
      Contract.Ensures(Contract.Result<ArrayTypeReference>() != null);

      if (arrayTypeReference.IsVector) {
        var copy = new VectorTypeReference();
        copy.Copy(arrayTypeReference, this.internFactory);
        return copy;
      } else {
        var copy = new MatrixTypeReference();
        copy.Copy(arrayTypeReference, this.internFactory);
        return copy;
      }
    }

    /// <summary>
    /// Returns a shallow copy of the given assembly.
    /// </summary>
    public Assembly Copy(IAssembly assembly) {
      Contract.Requires(assembly != null);
      Contract.Ensures(Contract.Result<Assembly>() != null);

      var copy = new Assembly();
      copy.Copy(assembly, this.internFactory);
      this.targetUnit = copy;
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given assembly reference.
    /// </summary>
    public AssemblyReference Copy(IAssemblyReference assemblyReference) {
      Contract.Requires(assemblyReference != null);
      Contract.Ensures(Contract.Result<AssemblyReference>() != null);

      var copy = new AssemblyReference();
      copy.Copy(assemblyReference, this.internFactory);
      copy.Host = this.targetHost;
      copy.ReferringUnit = this.targetUnit;
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given custom attribute.
    /// </summary>
    public CustomAttribute Copy(ICustomAttribute customAttribute) {
      Contract.Requires(customAttribute != null);
      Contract.Ensures(Contract.Result<CustomAttribute>() != null);

      var copy = new CustomAttribute();
      copy.Copy(customAttribute, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given custom modifier.
    /// </summary>
    public CustomModifier Copy(ICustomModifier customModifier) {
      Contract.Requires(customModifier != null);
      Contract.Ensures(Contract.Result<CustomModifier>() != null);

      var copy = new CustomModifier();
      copy.Copy(customModifier, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given event definition.
    /// </summary>
    public EventDefinition Copy(IEventDefinition eventDefinition) {
      Contract.Requires(eventDefinition != null);
      Contract.Ensures(Contract.Result<EventDefinition>() != null);

      eventDefinition.Dispatch(this.Dispatcher);
      return (EventDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given event definition.
    /// </summary>
    public EventDefinition CopyUnspecialized(IEventDefinition eventDefinition) {
      var copy = new EventDefinition();
      copy.Copy(eventDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given field definition.
    /// </summary>
    public FieldDefinition Copy(IFieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);
      Contract.Ensures(Contract.Result<FieldDefinition>() != null);

      fieldDefinition.Dispatch(this.Dispatcher);
      return (FieldDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given field definition.
    /// </summary>
    private FieldDefinition CopyUnspecialized(IFieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);
      Contract.Ensures(Contract.Result<FieldDefinition>() != null);

      var copy = new FieldDefinition();
      copy.Copy(fieldDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given field reference.
    /// </summary>
    public FieldReference Copy(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Ensures(Contract.Result<FieldReference>() != null);

      fieldReference.DispatchAsReference(this.Dispatcher);
      return (FieldReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given field reference.
    /// </summary>
    private FieldReference CopyUnspecialized(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Ensures(Contract.Result<FieldReference>() != null);

      var copy = new FieldReference();
      copy.Copy(fieldReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given file reference.
    /// </summary>
    public FileReference Copy(IFileReference fileReference) {
      Contract.Requires(fileReference != null);
      Contract.Ensures(Contract.Result<FileReference>() != null);

      var copy = new FileReference();
      copy.Copy(fileReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given function pointer type reference.
    /// </summary>
    public FunctionPointerTypeReference Copy(IFunctionPointerTypeReference functionPointerTypeReference) {
      Contract.Requires(functionPointerTypeReference != null);
      Contract.Ensures(Contract.Result<FunctionPointerTypeReference>() != null);

      var copy = new FunctionPointerTypeReference();
      copy.Copy(functionPointerTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given generic method instance reference.
    /// </summary>
    public GenericMethodInstanceReference Copy(IGenericMethodInstanceReference genericMethodInstanceReference) {
      Contract.Requires(genericMethodInstanceReference != null);
      Contract.Ensures(Contract.Result<GenericMethodInstanceReference>() != null);

      var copy = new GenericMethodInstanceReference();
      copy.Copy(genericMethodInstanceReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given generic method parameter.
    /// </summary>
    public GenericMethodParameter Copy(IGenericMethodParameter genericMethodParameter) {
      Contract.Requires(genericMethodParameter != null);
      Contract.Ensures(Contract.Result<GenericMethodParameter>() != null);

      var copy = new GenericMethodParameter();
      copy.Copy(genericMethodParameter, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given generic method parameter reference.
    /// </summary>
    public GenericMethodParameterReference Copy(IGenericMethodParameterReference genericMethodParameterReference) {
      Contract.Requires(genericMethodParameterReference != null);
      Contract.Ensures(Contract.Result<GenericMethodParameterReference>() != null);

      var copy = new GenericMethodParameterReference();
      copy.Copy(genericMethodParameterReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given generic type instance reference.
    /// </summary>
    public GenericTypeInstanceReference Copy(IGenericTypeInstanceReference genericTypeInstanceReference) {
      Contract.Requires(genericTypeInstanceReference != null);
      Contract.Ensures(Contract.Result<GenericTypeInstanceReference>() != null);

      var copy = new GenericTypeInstanceReference();
      copy.Copy(genericTypeInstanceReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given generic type parameter.
    /// </summary>
    public GenericTypeParameter Copy(IGenericTypeParameter genericTypeParameter) {
      Contract.Requires(genericTypeParameter != null);
      Contract.Ensures(Contract.Result<GenericTypeParameter>() != null);

      var copy = new GenericTypeParameter();
      copy.Copy(genericTypeParameter, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given generic type parameter reference.
    /// </summary>
    public GenericTypeParameterReference Copy(IGenericTypeParameterReference genericTypeParameterReference) {
      Contract.Requires(genericTypeParameterReference != null);
      Contract.Ensures(Contract.Result<GenericTypeParameterReference>() != null);

      var copy = new GenericTypeParameterReference();
      copy.Copy(genericTypeParameterReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given global field definition.
    /// </summary>
    public GlobalFieldDefinition Copy(IGlobalFieldDefinition globalFieldDefinition) {
      Contract.Requires(globalFieldDefinition != null);
      Contract.Ensures(Contract.Result<GlobalFieldDefinition>() != null);

      var copy = new GlobalFieldDefinition();
      copy.Copy(globalFieldDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given global method definition.
    /// </summary>
    public GlobalMethodDefinition Copy(IGlobalMethodDefinition globalMethodDefinition) {
      Contract.Requires(globalMethodDefinition != null);
      Contract.Ensures(Contract.Result<GlobalMethodDefinition>() != null);

      var copy = new GlobalMethodDefinition();
      copy.Copy(globalMethodDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified local definition.
    /// </summary>
    public LocalDefinition Copy(ILocalDefinition localDefinition) {
      Contract.Requires(localDefinition != null);
      Contract.Ensures(Contract.Result<LocalDefinition>() != null);

      var mutable = localDefinition as LocalDefinition;
      if (mutable != null)
        return mutable.Clone();
      var copy = new LocalDefinition();
      copy.Copy(localDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given managed pointer type reference.
    /// </summary>
    public ManagedPointerTypeReference Copy(IManagedPointerTypeReference managedPointerTypeReference) {
      Contract.Requires(managedPointerTypeReference != null);
      Contract.Ensures(Contract.Result<ManagedPointerTypeReference>() != null);

      var copy = new ManagedPointerTypeReference();
      copy.Copy(managedPointerTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given marshalling information.
    /// </summary>
    public MarshallingInformation Copy(IMarshallingInformation marshallingInformation) {
      Contract.Requires(marshallingInformation != null);
      Contract.Ensures(Contract.Result<MarshallingInformation>() != null);

      var copy = new MarshallingInformation();
      copy.Copy(marshallingInformation, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given metadata constant.
    /// </summary>
    public MetadataConstant Copy(IMetadataConstant constant) {
      Contract.Requires(constant != null);
      Contract.Ensures(Contract.Result<MetadataConstant>() != null);

      var copy = new MetadataConstant();
      copy.Copy(constant, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given metadata array creation expression.
    /// </summary>
    public MetadataCreateArray Copy(IMetadataCreateArray createArray) {
      Contract.Requires(createArray != null);
      Contract.Ensures(Contract.Result<MetadataCreateArray>() != null);

      var copy = new MetadataCreateArray();
      copy.Copy(createArray, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given metadata expression.
    /// </summary>
    public MetadataExpression Copy(IMetadataExpression expression) {
      Contract.Requires(expression != null);
      Contract.Ensures(Contract.Result<MetadataExpression>() != null);

      expression.Dispatch(this.Dispatcher);
      return (MetadataExpression)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given metadata named argument expression.
    /// </summary>
    public MetadataNamedArgument Copy(IMetadataNamedArgument namedArgument) {
      Contract.Requires(namedArgument != null);
      Contract.Ensures(Contract.Result<MetadataNamedArgument>() != null);

      var copy = new MetadataNamedArgument();
      copy.Copy(namedArgument, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given metadata typeof expression.
    /// </summary>
    public MetadataTypeOf Copy(IMetadataTypeOf typeOf) {
      Contract.Requires(typeOf != null);
      Contract.Ensures(Contract.Result<MetadataTypeOf>() != null);

      var copy = new MetadataTypeOf();
      copy.Copy(typeOf, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given method body.
    /// </summary>
    public MethodBody Copy(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<MethodBody>() != null);

      var copy = new MethodBody();
      copy.Copy(methodBody, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given method definition.
    /// </summary>
    public MethodDefinition Copy(IMethodDefinition method) {
      Contract.Requires(method != null);
      Contract.Ensures(Contract.Result<MethodDefinition>() != null);

      method.Dispatch(this.Dispatcher);
      return (MethodDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given method definition.
    /// </summary>
    private MethodDefinition CopyUnspecialized(IMethodDefinition method) {
      var copy = new MethodDefinition();
      copy.Copy(method, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given method implementation.
    /// </summary>
    public MethodImplementation Copy(IMethodImplementation methodImplementation) {
      Contract.Requires(methodImplementation != null);
      Contract.Ensures(Contract.Result<MethodImplementation>() != null);

      var copy = new MethodImplementation();
      copy.Copy(methodImplementation, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given method reference.
    /// </summary>
    public MethodReference Copy(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Ensures(Contract.Result<MethodReference>() != null);

      methodReference.DispatchAsReference(this.Dispatcher);
      return (MethodReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given method reference.
    /// </summary>
    private MethodReference CopyUnspecialized(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Ensures(Contract.Result<MethodReference>() != null);

      var copy = new MethodReference();
      copy.Copy(methodReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given modified type reference.
    /// </summary>
    public ModifiedTypeReference Copy(IModifiedTypeReference modifiedTypeReference) {
      Contract.Requires(modifiedTypeReference != null);
      Contract.Ensures(Contract.Result<ModifiedTypeReference>() != null);

      var copy = new ModifiedTypeReference();
      copy.Copy(modifiedTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given module.
    /// </summary>
    public Module Copy(IModule module) {
      Contract.Requires(module != null);
      Contract.Ensures(Contract.Result<Module>() != null);

      var assembly = module as IAssembly;
      if (assembly != null) return this.Copy(assembly);
      var copy = new Module();
      copy.Copy(module, this.internFactory);
      this.targetUnit = copy;
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given module reference.
    /// </summary>
    public ModuleReference Copy(IModuleReference moduleReference) {
      Contract.Requires(moduleReference != null);
      Contract.Ensures(Contract.Result<ModuleReference>() != null);

      var assemblyReference = moduleReference as IAssemblyReference;
      if (assemblyReference != null) return this.Copy(assemblyReference);
      var copy = new ModuleReference();
      copy.Copy(moduleReference, this.internFactory);
      copy.Host = this.targetHost;
      copy.ReferringUnit = this.targetUnit;
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified named type definition.
    /// </summary>
    public NamedTypeDefinition Copy(INamedTypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);
      Contract.Ensures(Contract.Result<NamedTypeDefinition>() != null);

      typeDefinition.Dispatch(this.Dispatcher);
      return (NamedTypeDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given alias for a namespace type definition.
    /// </summary>
    public NamespaceAliasForType Copy(INamespaceAliasForType namespaceAliasForType) {
      Contract.Requires(namespaceAliasForType != null);
      Contract.Ensures(Contract.Result<NamespaceAliasForType>() != null);

      var copy = new NamespaceAliasForType();
      copy.Copy(namespaceAliasForType, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified unit namespace.
    /// </summary>
    public UnitNamespace Copy(IUnitNamespace unitNamespace) {
      Contract.Requires(unitNamespace != null);
      Contract.Ensures(Contract.Result<UnitNamespace>() != null);

      unitNamespace.Dispatch(this.Dispatcher);
      return (UnitNamespace)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the specified unit namespace.
    /// </summary>
    public UnitNamespaceReference Copy(IUnitNamespaceReference unitNamespace) {
      Contract.Requires(unitNamespace != null);
      Contract.Ensures(Contract.Result<UnitNamespaceReference>() != null);

      unitNamespace.DispatchAsReference(this.Dispatcher);
      return (UnitNamespaceReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given namespace type definition.
    /// </summary>
    public NamespaceTypeDefinition Copy(INamespaceTypeDefinition namespaceTypeDefinition) {
      Contract.Requires(namespaceTypeDefinition != null);
      Contract.Ensures(Contract.Result<NamespaceTypeDefinition>() != null);

      var copy = new NamespaceTypeDefinition();
      copy.Copy(namespaceTypeDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given namespace type reference.
    /// </summary>
    public NamespaceTypeReference Copy(INamespaceTypeReference namespaceTypeReference) {
      Contract.Requires(namespaceTypeReference != null);
      Contract.Ensures(Contract.Result<NamespaceTypeReference>() != null);

      var copy = new NamespaceTypeReference();
      copy.Copy(namespaceTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested type alias.
    /// </summary>
    public NestedAliasForType Copy(INestedAliasForType nestedAliasForType) {
      Contract.Requires(nestedAliasForType != null);
      Contract.Ensures(Contract.Result<NestedAliasForType>() != null);

      var copy = new NestedAliasForType();
      copy.Copy(nestedAliasForType, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested type definition.
    /// </summary>
    public NestedTypeDefinition Copy(INestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);
      Contract.Ensures(Contract.Result<NestedTypeDefinition>() != null);

      nestedTypeDefinition.Dispatch(this.Dispatcher);
      return (NestedTypeDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested type definition.
    /// </summary>
    private NestedTypeDefinition CopyUnspecialized(INestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);
      Contract.Ensures(Contract.Result<NestedTypeDefinition>() != null);

      var copy = new NestedTypeDefinition();
      copy.Copy(nestedTypeDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested type reference.
    /// </summary>
    public NestedTypeReference Copy(INestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Ensures(Contract.Result<NestedTypeReference>() != null);

      nestedTypeReference.DispatchAsReference(this.Dispatcher);
      return (NestedTypeReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested type reference.
    /// </summary>
    private NestedTypeReference CopyUnspecialized(INestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Ensures(Contract.Result<NestedTypeReference>() != null);

      var copy = new NestedTypeReference();
      copy.Copy(nestedTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested unit namespace.
    /// </summary>
    public NestedUnitNamespace Copy(INestedUnitNamespace nestedUnitNamespace) {
      Contract.Requires(nestedUnitNamespace != null);
      Contract.Ensures(Contract.Result<NestedUnitNamespace>() != null);

      var copy = new NestedUnitNamespace();
      copy.Copy(nestedUnitNamespace, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given nested unit namespace reference.
    /// </summary>
    public NestedUnitNamespaceReference Copy(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      Contract.Requires(nestedUnitNamespaceReference != null);
      Contract.Ensures(Contract.Result<NestedUnitNamespaceReference>() != null);

      var copy = new NestedUnitNamespaceReference();
      copy.Copy(nestedUnitNamespaceReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified operation.
    /// </summary>
    public Operation Copy(IOperation operation) {
      Contract.Requires(operation != null);
      Contract.Ensures(Contract.Result<Operation>() != null);

      var copy = new Operation();
      copy.Copy(operation, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified operation exception information.
    /// </summary>
    public OperationExceptionInformation Copy(IOperationExceptionInformation operationExceptionInformation) {
      Contract.Requires(operationExceptionInformation != null);
      Contract.Ensures(Contract.Result<OperationExceptionInformation>() != null);

      var copy = new OperationExceptionInformation();
      copy.Copy(operationExceptionInformation, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given parameter definition.
    /// </summary>
    public ParameterDefinition Copy(IParameterDefinition parameterDefinition) {
      Contract.Requires(parameterDefinition != null);
      Contract.Ensures(Contract.Result<ParameterDefinition>() != null);

      var copy = new ParameterDefinition();
      copy.Copy(parameterDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given parameter type information.
    /// </summary>
    public ParameterTypeInformation Copy(IParameterTypeInformation parameterTypeInformation) {
      Contract.Requires(parameterTypeInformation != null);
      Contract.Ensures(Contract.Result<ParameterTypeInformation>() != null);

      var copy = new ParameterTypeInformation();
      copy.Copy(parameterTypeInformation, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given parameter type information.
    /// </summary>
    public PESection Copy(IPESection peSection) {
      Contract.Requires(peSection != null);
      Contract.Ensures(Contract.Result<PESection>() != null);

      var copy = new PESection();
      copy.Copy(peSection, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified platform invoke information.
    /// </summary>
    public PlatformInvokeInformation Copy(IPlatformInvokeInformation platformInvokeInformation) {
      Contract.Requires(platformInvokeInformation != null);
      Contract.Ensures(Contract.Result<PlatformInvokeInformation>() != null);

      var copy = new PlatformInvokeInformation();
      copy.Copy(platformInvokeInformation, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given pointer type reference.
    /// </summary>
    public PointerTypeReference Copy(IPointerTypeReference pointerTypeReference) {
      Contract.Requires(pointerTypeReference != null);
      Contract.Ensures(Contract.Result<PointerTypeReference>() != null);

      var copy = new PointerTypeReference();
      copy.Copy(pointerTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given property definition.
    /// </summary>
    public PropertyDefinition Copy(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

      propertyDefinition.Dispatch(this.Dispatcher);
      return (PropertyDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given property definition.
    /// </summary>
    private PropertyDefinition CopyUnspecialized(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

      var copy = new PropertyDefinition();
      copy.Copy(propertyDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given root unit namespace.
    /// </summary>
    public RootUnitNamespace Copy(IRootUnitNamespace rootUnitNamespace) {
      Contract.Requires(rootUnitNamespace != null);
      Contract.Ensures(Contract.Result<RootUnitNamespace>() != null);

      var copy = new RootUnitNamespace();
      copy.Copy(rootUnitNamespace, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given root unit namespace.
    /// </summary>
    public RootUnitNamespaceReference Copy(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      Contract.Requires(rootUnitNamespaceReference != null);
      Contract.Ensures(Contract.Result<RootUnitNamespaceReference>() != null);

      var copy = new RootUnitNamespaceReference();
      copy.Copy(rootUnitNamespaceReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given reference to a manifest resource.
    /// </summary>
    public ResourceReference Copy(IResourceReference resourceReference) {
      Contract.Requires(resourceReference != null);
      Contract.Ensures(Contract.Result<ResourceReference>() != null);

      var copy = new ResourceReference();
      copy.Copy(resourceReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given security attribute.
    /// </summary>
    public SecurityAttribute Copy(ISecurityAttribute securityAttribute) {
      Contract.Requires(securityAttribute != null);
      Contract.Ensures(Contract.Result<SecurityAttribute>() != null);

      var copy = new SecurityAttribute();
      copy.Copy(securityAttribute, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized event definition.
    /// </summary>
    /// <param name="specializedEventDefinition">The specialized event definition.</param>
    public SpecializedEventDefinition Copy(ISpecializedEventDefinition specializedEventDefinition) {
      Contract.Requires(specializedEventDefinition != null);
      Contract.Ensures(Contract.Result<SpecializedEventDefinition>() != null);

      var copy = new SpecializedEventDefinition();
      copy.Copy(specializedEventDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized field definition.
    /// </summary>
    /// <param name="specializedFieldDefinition">The specialized field definition.</param>
    public SpecializedFieldDefinition Copy(ISpecializedFieldDefinition specializedFieldDefinition) {
      Contract.Requires(specializedFieldDefinition != null);
      Contract.Ensures(Contract.Result<SpecializedFieldDefinition>() != null);

      var copy = new SpecializedFieldDefinition();
      copy.Copy(specializedFieldDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized field reference.
    /// </summary>
    public SpecializedFieldReference Copy(ISpecializedFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Ensures(Contract.Result<SpecializedFieldReference>() != null);

      var copy = new SpecializedFieldReference();
      copy.Copy(fieldReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized method definition.
    /// </summary>
    /// <param name="specializedMethodDefinition">The specialized method definition.</param>
    public SpecializedMethodDefinition Copy(ISpecializedMethodDefinition specializedMethodDefinition) {
      Contract.Requires(specializedMethodDefinition != null);
      Contract.Ensures(Contract.Result<SpecializedMethodDefinition>() != null);

      var copy = new SpecializedMethodDefinition();
      copy.Copy(specializedMethodDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized method reference.
    /// </summary>
    public SpecializedMethodReference Copy(ISpecializedMethodReference specializedMethodReference) {
      Contract.Requires(specializedMethodReference != null);
      Contract.Ensures(Contract.Result<SpecializedMethodReference>() != null);

      var copy = new SpecializedMethodReference();
      copy.Copy(specializedMethodReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized nested type definition.
    /// </summary>
    public SpecializedNestedTypeDefinition Copy(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
      Contract.Requires(specializedNestedTypeDefinition != null);
      Contract.Ensures(Contract.Result<SpecializedNestedTypeDefinition>() != null);

      var copy = new SpecializedNestedTypeDefinition();
      copy.Copy(specializedNestedTypeDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized nested type reference.
    /// </summary>
    public SpecializedNestedTypeReference Copy(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      Contract.Requires(specializedNestedTypeReference != null);
      Contract.Ensures(Contract.Result<SpecializedNestedTypeReference>() != null);

      var copy = new SpecializedNestedTypeReference();
      copy.Copy(specializedNestedTypeReference, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the given specialized property definition.
    /// </summary>
    /// <param name="specializedPropertyDefinition">The specialized property definition.</param>
    public SpecializedPropertyDefinition Copy(ISpecializedPropertyDefinition specializedPropertyDefinition) {
      Contract.Requires(specializedPropertyDefinition != null);
      Contract.Ensures(Contract.Result<SpecializedPropertyDefinition>() != null);

      var copy = new SpecializedPropertyDefinition();
      copy.Copy(specializedPropertyDefinition, this.internFactory);
      return copy;
    }

    /// <summary>
    /// Returns a shallow copy of the specified type definition.
    /// </summary>
    public ITypeDefinition Copy(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);

      typeDefinition.Dispatch(this.Dispatcher);
      return (ITypeDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the specified type member.
    /// </summary>
    public TypeDefinitionMember Copy(ITypeDefinitionMember typeMember) {
      Contract.Requires(typeMember != null);
      Contract.Ensures(Contract.Result<TypeDefinitionMember>() != null);

      typeMember.Dispatch(this.Dispatcher);
      return (TypeDefinitionMember)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the specified type reference.
    /// </summary>
    public TypeReference Copy(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<TypeReference>() != null);

      typeReference.DispatchAsReference(this.Dispatcher);
      return (TypeReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the specified unit reference.
    /// </summary>
    public UnitReference Copy(IUnitReference unitReference) {
      Contract.Requires(unitReference != null);
      Contract.Ensures(Contract.Result<UnitReference>() != null);

      unitReference.DispatchAsReference(this.Dispatcher);
      return (UnitReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given Win32 resource.
    /// </summary>
    public Win32Resource Copy(IWin32Resource win32Resource) {
      Contract.Requires(win32Resource != null);
      Contract.Ensures(Contract.Result<Win32Resource>() != null);

      var copy = new Win32Resource();
      copy.Copy(win32Resource, this.internFactory);
      return copy;
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public class MetadataDeepCopier {

    /// <summary>
    /// </summary>
    /// <param name="targetHost">An object representing the application that will host the copies made by this copier.</param>
    public MetadataDeepCopier(IMetadataHost targetHost)
      : this(targetHost, new MetadataShallowCopier(targetHost)) {
      Contract.Requires(targetHost != null);
    }

    /// <summary>
    /// </summary>
    /// <param name="targetHost">An object representing the application that will host the copies made by this copier.</param>
    /// <param name="targetUnit">The unit of metadata into which copies made by this copier will be inserted.</param>
    public MetadataDeepCopier(IMetadataHost targetHost, IUnit targetUnit)
      : this(targetHost, new MetadataShallowCopier(targetHost, targetUnit)) {
      Contract.Requires(targetHost != null);
      Contract.Requires(targetUnit != null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetHost"></param>
    /// <param name="shallowCopier"></param>
    protected MetadataDeepCopier(IMetadataHost targetHost, MetadataShallowCopier shallowCopier) {
      Contract.Requires(targetHost != null);
      Contract.Requires(shallowCopier != null);

      this.targetHost = targetHost;
      this.shallowCopier = shallowCopier;
      this.internFactory = targetHost.InternFactory;
    }

    IMetadataHost targetHost;
    MetadataShallowCopier shallowCopier;
    IInternFactory internFactory;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.targetHost != null);
      Contract.Invariant(this.shallowCopier != null);
      Contract.Invariant(this.internFactory != null);
    }

    /// <summary>
    /// Returns a map from original definition objects to the copies that were made by this copier.
    /// </summary>
    public Hashtable<object, object> CopyFor {
      get { return this.SubstituteCopiesForOriginals.DefinitionCache; }
    }

    /// <summary>
    /// Returns a map from copied definition objects to the original definitions from which the copies were constructed.
    /// </summary>
    public Hashtable<object, object> OriginalFor {
      get {
        if (this.originalFor == null) {
          var copyFor = this.SubstituteCopiesForOriginals.DefinitionCache;
          var originalFor = this.originalFor = new Hashtable<object, object>(copyFor.Count);
          foreach (var keyValPair in copyFor) {
            originalFor[keyValPair.value] = keyValPair.key;
          }
        }
        return this.originalFor;
      }
    }
    Hashtable<object, object> originalFor;

    Substitutor SubstituteCopiesForOriginals {
      get {
        Contract.Ensures(Contract.Result<Substitutor>() != null);
        if (this.substituteCopiesForOriginals == null)
          this.substituteCopiesForOriginals = new Substitutor(targetHost, shallowCopier, this);
        return this.substituteCopiesForOriginals;
      }
    }
    Substitutor substituteCopiesForOriginals;

    MetadataTraverser TraverseAndPopulateDefinitionCacheWithCopies {
      get {
        Contract.Ensures(Contract.Result<MetadataTraverser>() != null);
        if (this.traverseAndPopulateDefinitionCacheWithCopies == null) {
          var populator = new Populator(this.SubstituteCopiesForOriginals.shallowCopier, this.SubstituteCopiesForOriginals.DefinitionCache);
          this.traverseAndPopulateDefinitionCacheWithCopies = new MetadataTraverser() { PreorderVisitor = populator };
        }
        return this.traverseAndPopulateDefinitionCacheWithCopies;
      }
    }
    MetadataTraverser traverseAndPopulateDefinitionCacheWithCopies;

    MetadataDispatcher Dispatcher {
      get {
        Contract.Ensures(Contract.Result<MetadataDispatcher>() != null);
        if (this.dispatcher == null)
          this.dispatcher = new MetadataDispatcher() { copier = this };
        return this.dispatcher;
      }
    }
    MetadataDispatcher dispatcher;

#pragma warning disable 1591
    protected class MetadataDispatcher : MetadataVisitor {
      internal MetadataDeepCopier copier;
      internal object result;

      public override void Visit(IArrayTypeReference arrayTypeReference) {
        this.result = this.copier.Copy(arrayTypeReference);
      }

      public override void Visit(IEventDefinition eventDefinition) {
        this.result = this.copier.CopyUnspecialized(eventDefinition);
      }

      public override void Visit(IFieldDefinition fieldDefinition) {
        this.result = this.copier.CopyUnspecialized(fieldDefinition);
      }

      public override void Visit(IFieldReference fieldReference) {
        this.result = this.copier.CopyUnspecialized(fieldReference);
      }

      public override void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
        this.result = this.copier.Copy(functionPointerTypeReference);
      }

      public override void Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
        this.result = this.copier.Copy(genericMethodInstanceReference);
      }

      public override void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
        this.result = this.copier.Copy(genericMethodParameterReference);
      }

      public override void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
        this.result = this.copier.Copy(genericTypeInstanceReference);
      }

      public override void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
        this.result = this.copier.Copy(genericTypeParameterReference);
      }

      public override void Visit(IGlobalFieldDefinition globalFieldDefinition) {
        this.result = this.copier.Copy(globalFieldDefinition);
      }

      public override void Visit(IGlobalMethodDefinition globalMethodDefinition) {
        this.result = this.copier.Copy(globalMethodDefinition);
      }

      public override void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
        this.result = this.copier.Copy(managedPointerTypeReference);
      }

      public override void Visit(IMetadataConstant constant) {
        this.result = this.copier.Copy(constant);
      }

      public override void Visit(IMetadataCreateArray createArray) {
        this.result = this.copier.Copy(createArray);
      }

      public override void Visit(IMetadataNamedArgument namedArgument) {
        this.result = this.copier.Copy(namedArgument);
      }

      public override void Visit(IMetadataTypeOf typeOf) {
        this.result = this.copier.Copy(typeOf);
      }

      public override void Visit(IMethodDefinition method) {
        this.result = this.copier.CopyUnspecialized(method);
      }

      public override void Visit(IMethodReference methodReference) {
        this.result = this.copier.CopyUnspecialized(methodReference);
      }

      public override void Visit(IModifiedTypeReference modifiedTypeReference) {
        this.result = this.copier.Copy(modifiedTypeReference);
      }

      public override void Visit(INamespaceAliasForType namespaceAliasForType) {
        this.result = this.copier.Copy(namespaceAliasForType);
      }

      public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
        this.result = this.copier.Copy(namespaceTypeDefinition);
      }

      public override void Visit(INamespaceTypeReference namespaceTypeReference) {
        this.result = this.copier.Copy(namespaceTypeReference);
      }

      public override void Visit(INestedAliasForType nestedAliasForType) {
        this.result = this.copier.Copy(nestedAliasForType);
      }

      public override void Visit(INestedTypeDefinition nestedTypeDefinition) {
        this.result = this.copier.CopyUnspecialized(nestedTypeDefinition);
      }

      public override void Visit(INestedTypeReference nestedTypeReference) {
        this.result = this.copier.CopyUnspecialized(nestedTypeReference);
      }

      public override void Visit(INestedUnitNamespace nestedUnitNamespace) {
        this.result = this.copier.Copy(nestedUnitNamespace);
      }

      public override void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
        this.result = this.copier.Copy(nestedUnitNamespaceReference);
      }

      public override void Visit(IPointerTypeReference pointerTypeReference) {
        this.result = this.copier.Copy(pointerTypeReference);
      }

      public override void Visit(IPropertyDefinition propertyDefinition) {
        this.result = this.copier.CopyUnspecialized(propertyDefinition);
      }

      public override void Visit(IRootUnitNamespace rootUnitNamespace) {
        this.result = this.copier.Copy(rootUnitNamespace);
      }

      public override void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
        this.result = this.copier.Copy(rootUnitNamespaceReference);
      }

      public override void Visit(ISpecializedEventDefinition specializedEventDefinition) {
        this.result = this.copier.CopySpecialized(specializedEventDefinition);
      }

      public override void Visit(ISpecializedFieldDefinition specializedFieldDefinition) {
        this.result = this.copier.CopySpecialized(specializedFieldDefinition);
      }

      public override void Visit(ISpecializedFieldReference specializedFieldReference) {
        this.result = this.copier.Copy(specializedFieldReference);
      }

      public override void Visit(ISpecializedMethodDefinition specializedMethodDefinition) {
        this.result = this.copier.CopySpecialized(specializedMethodDefinition);
      }

      public override void Visit(ISpecializedMethodReference specializedMethodReference) {
        this.result = this.copier.Copy(specializedMethodReference);
      }

      public override void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition) {
        this.result = this.copier.CopySpecialized(specializedPropertyDefinition);
      }

      public override void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
        this.result = this.copier.CopySpecialized(specializedNestedTypeDefinition);
      }

      public override void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
        this.result = this.copier.Copy(specializedNestedTypeReference);
      }

    }
#pragma warning restore 1591

    /// <summary>
    /// 
    /// </summary>
    class Populator : IMetadataVisitor {

      internal Populator(MetadataShallowCopier shallowCopier, Hashtable<object, object> definitionCache) {
        this.shallowCopier = shallowCopier;
        this.definitionCache = definitionCache;
      }

      MetadataShallowCopier shallowCopier;
      Hashtable<object, object> definitionCache;

#pragma warning disable 1591

      //Only copy and cache definitions during this pass. Method bodies are not visited.
      //Only cache definitions that can be reached via more than one edge. (The others can be copied when their one and only instance in the graph is reached in the second pass.)

      public void Visit(IArrayTypeReference arrayTypeReference) {
      }

      public void Visit(IAssembly assembly) {
        if (this.definitionCache.ContainsKey(assembly)) return;
        var copy = this.shallowCopier.Copy(assembly);
        this.definitionCache.Add(assembly, copy);
      }

      public void Visit(IAssemblyReference assemblyReference) {
      }

      public void Visit(ICustomAttribute customAttribute) {
      }

      public void Visit(ICustomModifier customModifier) {
      }

      public void Visit(IEventDefinition eventDefinition) {
      }

      public void Visit(IFieldDefinition fieldDefinition) {
        if (this.definitionCache.ContainsKey(fieldDefinition)) return;
        var copy = this.shallowCopier.Copy(fieldDefinition);
        this.definitionCache.Add(fieldDefinition, copy);
      }

      public void Visit(IFieldReference fieldReference) {
      }

      public void Visit(IFileReference fileReference) {
      }

      public void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
      }

      public void Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
      }

      public void Visit(IGenericMethodParameter genericMethodParameter) {
        if (this.definitionCache.ContainsKey(genericMethodParameter)) return;
        var copy = this.shallowCopier.Copy(genericMethodParameter);
        this.definitionCache.Add(genericMethodParameter, copy);
      }

      public void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
      }

      public void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
      }

      public void Visit(IGenericTypeParameter genericTypeParameter) {
        if (this.definitionCache.ContainsKey(genericTypeParameter)) return;
        var copy = this.shallowCopier.Copy(genericTypeParameter);
        this.definitionCache.Add(genericTypeParameter, copy);
      }

      public void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
      }

      public void Visit(IGlobalFieldDefinition globalFieldDefinition) {
        if (this.definitionCache.ContainsKey(globalFieldDefinition)) return;
        var copy = this.shallowCopier.Copy(globalFieldDefinition);
        this.definitionCache.Add(globalFieldDefinition, copy);
      }

      public void Visit(IGlobalMethodDefinition globalMethodDefinition) {
        if (this.definitionCache.ContainsKey(globalMethodDefinition)) return;
        var copy = this.shallowCopier.Copy(globalMethodDefinition);
        this.definitionCache.Add(globalMethodDefinition, copy);
      }

      public void Visit(ILocalDefinition localDefinition) {
      }

      public void VisitReference(ILocalDefinition localDefinition) {
      }

      public void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
      }

      public void Visit(IMarshallingInformation marshallingInformation) {
      }

      public void Visit(IMetadataConstant constant) {
      }

      public void Visit(IMetadataCreateArray createArray) {
      }

      public void Visit(IMetadataExpression expression) {
      }

      public void Visit(IMetadataNamedArgument namedArgument) {
      }

      public void Visit(IMetadataTypeOf typeOf) {
      }

      public void Visit(IMethodBody methodBody) {
      }

      public void Visit(IMethodDefinition method) {
        if (this.definitionCache.ContainsKey(method)) return;
        var copy = this.shallowCopier.Copy(method);
        this.definitionCache.Add(method, copy);
      }

      public void Visit(IMethodImplementation methodImplementation) {
      }

      public void Visit(IMethodReference methodReference) {
      }

      public void Visit(IModifiedTypeReference modifiedTypeReference) {
      }

      public void Visit(IModule module) {
        if (this.definitionCache.ContainsKey(module)) return;
        var copy = this.shallowCopier.Copy(module);
        this.definitionCache.Add(module, copy);
      }

      public void Visit(IModuleReference moduleReference) {
      }

      public void Visit(INamespaceAliasForType namespaceAliasForType) {
        if (this.definitionCache.ContainsKey(namespaceAliasForType)) return;
        var copy = this.shallowCopier.Copy(namespaceAliasForType);
        this.definitionCache.Add(namespaceAliasForType, copy);
      }

      public void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
        if (this.definitionCache.ContainsKey(namespaceTypeDefinition)) return;
        var copy = this.shallowCopier.Copy(namespaceTypeDefinition);
        this.definitionCache.Add(namespaceTypeDefinition, copy);
      }

      public void Visit(INamespaceTypeReference namespaceTypeReference) {
      }

      public void Visit(INestedAliasForType nestedAliasForType) {
        if (this.definitionCache.ContainsKey(nestedAliasForType)) return;
        var copy = this.shallowCopier.Copy(nestedAliasForType);
        this.definitionCache.Add(nestedAliasForType, copy);
      }

      public void Visit(INestedTypeDefinition nestedTypeDefinition) {
        if (this.definitionCache.ContainsKey(nestedTypeDefinition)) return;
        var copy = this.shallowCopier.Copy(nestedTypeDefinition);
        this.definitionCache.Add(nestedTypeDefinition, copy);
      }

      public void Visit(INestedTypeReference nestedTypeReference) {
      }

      public void Visit(INestedUnitNamespace nestedUnitNamespace) {
        if (this.definitionCache.ContainsKey(nestedUnitNamespace)) return;
        var copy = this.shallowCopier.Copy(nestedUnitNamespace);
        this.definitionCache.Add(nestedUnitNamespace, copy);
      }

      public void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      }

      public void Visit(INestedUnitSetNamespace nestedUnitSetNamespace) {
      }

      public void Visit(IOperation operation) {
      }

      public void Visit(IOperationExceptionInformation operationExceptionInformation) {
      }

      public void Visit(IParameterDefinition parameterDefinition) {
        if (this.definitionCache.ContainsKey(parameterDefinition)) return;
        var copy = this.shallowCopier.Copy(parameterDefinition);
        this.definitionCache.Add(parameterDefinition, copy);
      }

      public void VisitReference(IParameterDefinition parameterDefinition) {
      }

      public void Visit(IParameterTypeInformation parameterTypeInformation) {
      }

      public void Visit(IPESection peSection) {
      }

      public void Visit(IPlatformInvokeInformation platformInvokeInformation) {
      }

      public void Visit(IPointerTypeReference pointerTypeReference) {
      }

      public void Visit(IPropertyDefinition propertyDefinition) {
        //properties are referenced by the parent pointers in their parameters.
        if (this.definitionCache.ContainsKey(propertyDefinition)) return;
        var copy = this.shallowCopier.Copy(propertyDefinition);
        this.definitionCache.Add(propertyDefinition, copy);
      }

      public void Visit(IResourceReference resourceReference) {
      }

      public void Visit(IRootUnitNamespace rootUnitNamespace) {
        if (this.definitionCache.ContainsKey(rootUnitNamespace)) return;
        var copy = this.shallowCopier.Copy(rootUnitNamespace);
        this.definitionCache.Add(rootUnitNamespace, copy);
      }

      public void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      }

      public void Visit(IRootUnitSetNamespace rootUnitSetNamespace) {
      }

      public void Visit(ISecurityAttribute securityAttribute) {
      }

      public void Visit(ISpecializedEventDefinition specializedEventDefinition) {
      }

      public void Visit(ISpecializedFieldDefinition specializedFieldDefinition) {
        if (this.definitionCache.ContainsKey(specializedFieldDefinition)) return;
        var copy = this.shallowCopier.Copy(specializedFieldDefinition);
        this.definitionCache.Add(specializedFieldDefinition, copy);
      }

      public void Visit(ISpecializedFieldReference specializedFieldReference) {
      }

      public void Visit(ISpecializedMethodDefinition specializedMethodDefinition) {
        if (this.definitionCache.ContainsKey(specializedMethodDefinition)) return;
        var copy = this.shallowCopier.Copy(specializedMethodDefinition);
        this.definitionCache.Add(specializedMethodDefinition, copy);
      }

      public void Visit(ISpecializedMethodReference specializedMethodReference) {
      }

      public void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition) {
        //properties are referenced by the parent pointers in their parameters.
        if (this.definitionCache.ContainsKey(specializedPropertyDefinition)) return;
        var copy = this.shallowCopier.Copy(specializedPropertyDefinition);
        this.definitionCache.Add(specializedPropertyDefinition, copy);
      }

      public void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
        if (this.definitionCache.ContainsKey(specializedNestedTypeDefinition)) return;
        var copy = this.shallowCopier.Copy(specializedNestedTypeDefinition);
        this.definitionCache.Add(specializedNestedTypeDefinition, copy);
      }

      public void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      }

      public void Visit(IUnitSet unitSet) {
      }

      public void Visit(IWin32Resource win32Resource) {
      }
#pragma warning restore 1591

    }

    /// <summary>
    /// 
    /// </summary>
    class Substitutor {

      internal Substitutor(IMetadataHost host, MetadataShallowCopier shallowCopier, MetadataDeepCopier deepCopier) {
        this.host = host;
        this.shallowCopier = shallowCopier;
        this.deepCopier = deepCopier;
      }

      internal MetadataShallowCopier shallowCopier;

      MetadataDeepCopier deepCopier;

      /// <summary>
      /// Unless a definition is outside the cone to be copied, it must have an entry in this table.
      /// Consult this to find containing definitions.
      /// </summary>
      internal Hashtable<object, object> DefinitionCache {
        get {
          if (this.definitionCache == null)
            this.definitionCache = new Hashtable<object, object>();
          return this.definitionCache;
        }
      }
      Hashtable<object, object> definitionCache;

      /// <summary>
      /// A cache of references that have already been encountered and copied. Once in the cache, the cached value is just returned unchanged.
      /// </summary>
      Hashtable<object, object> ReferenceCache {
        get {
          if (this.referenceCache == null)
            this.referenceCache = new Hashtable<object, object>();
          return this.referenceCache;
        }
      }
      Hashtable<object, object> referenceCache;

      /// <summary>
      /// 
      /// </summary>
      IMetadataHost host;

      MetadataDispatcher Dispatcher {
        get {
          if (this.dispatcher == null)
            this.dispatcher = new MetadataDispatcher() { substitutor = this };
          return this.dispatcher;
        }
      }
      MetadataDispatcher dispatcher;

      class MetadataDispatcher : MetadataVisitor {
        internal Substitutor substitutor;

        internal object result;

        public override void Visit(IAliasForType aliasForType) {
          this.result = this.substitutor.Substitute(aliasForType);
        }

        public override void Visit(IArrayTypeReference arrayTypeReference) {
          this.result = this.substitutor.Substitute(arrayTypeReference);
        }

        public override void Visit(IAssembly assembly) {
          this.result = this.substitutor.Substitute(assembly);
        }

        public override void Visit(IAssemblyReference assemblyReference) {
          this.result = this.substitutor.Substitute(assemblyReference);
        }

        public override void Visit(IEventDefinition eventDefinition) {
          this.result = this.substitutor.Substitute(eventDefinition);
        }

        public override void Visit(IFieldDefinition fieldDefinition) {
          this.result = this.substitutor.Substitute(fieldDefinition);
        }

        public override void Visit(IFieldReference fieldReference) {
          this.result = this.substitutor.Substitute(fieldReference);
        }

        public override void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
          this.result = this.substitutor.Substitute(functionPointerTypeReference);
        }

        public override void Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
          this.result = this.substitutor.Substitute(genericMethodInstanceReference);
        }

        public override void Visit(IGenericMethodParameter genericMethodParameter) {
          this.result = this.substitutor.Substitute(genericMethodParameter);
        }

        public override void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
          this.result = this.substitutor.Substitute(genericMethodParameterReference);
        }

        public override void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
          this.result = this.substitutor.Substitute(genericTypeInstanceReference);
        }

        public override void Visit(IGenericTypeParameter genericTypeParameter) {
          this.result = this.substitutor.Substitute(genericTypeParameter);
        }

        public override void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
          this.result = this.substitutor.Substitute(genericTypeParameterReference);
        }

        public override void Visit(IGlobalFieldDefinition globalFieldDefinition) {
          this.result = this.substitutor.Substitute(globalFieldDefinition);
        }

        public override void Visit(IGlobalMethodDefinition globalMethodDefinition) {
          this.result = this.substitutor.Substitute(globalMethodDefinition);
        }

        public override void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
          this.result = this.substitutor.Substitute(managedPointerTypeReference);
        }

        public override void Visit(IMetadataConstant constant) {
          this.result = this.substitutor.Substitute(constant);
        }

        public override void Visit(IMetadataCreateArray createArray) {
          this.result = this.substitutor.Substitute(createArray);
        }

        public override void Visit(IMetadataNamedArgument namedArgument) {
          this.result = this.substitutor.Substitute(namedArgument);
        }

        public override void Visit(IMetadataTypeOf typeOf) {
          this.result = this.substitutor.Substitute(typeOf);
        }

        public override void Visit(IMethodDefinition method) {
          this.result = this.substitutor.Substitute(method);
        }

        public override void Visit(IMethodReference methodReference) {
          this.result = this.substitutor.Substitute(methodReference);
        }

        public override void Visit(IModifiedTypeReference modifiedTypeReference) {
          this.result = this.substitutor.Substitute(modifiedTypeReference);
        }

        public override void Visit(IModule module) {
          this.result = this.substitutor.Substitute(module);
        }

        public override void Visit(IModuleReference moduleReference) {
          this.result = this.substitutor.Substitute(moduleReference);
        }

        public override void Visit(INamespaceAliasForType namespaceAliasForType) {
          this.result = this.substitutor.Substitute(namespaceAliasForType);
        }

        public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
          this.result = this.substitutor.Substitute(namespaceTypeDefinition);
        }

        public override void Visit(INamespaceTypeReference namespaceTypeReference) {
          this.result = this.substitutor.Substitute(namespaceTypeReference);
        }

        public override void Visit(INestedAliasForType nestedAliasForType) {
          this.result = this.substitutor.Substitute(nestedAliasForType);
        }

        public override void Visit(INestedTypeDefinition nestedTypeDefinition) {
          this.result = this.substitutor.Substitute(nestedTypeDefinition);
        }

        public override void Visit(INestedTypeReference nestedTypeReference) {
          this.result = this.substitutor.Substitute(nestedTypeReference);
        }

        public override void Visit(INestedUnitNamespace nestedUnitNamespace) {
          this.result = this.substitutor.Substitute(nestedUnitNamespace);
        }

        public override void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
          this.result = this.substitutor.Substitute(nestedUnitNamespaceReference);
        }

        public override void Visit(INestedUnitSetNamespace nestedUnitSetNamespace) {
          this.result = this.substitutor.Substitute(nestedUnitSetNamespace);
        }

        public override void Visit(IPointerTypeReference pointerTypeReference) {
          this.result = this.substitutor.Substitute(pointerTypeReference);
        }

        public override void Visit(IPropertyDefinition propertyDefinition) {
          this.result = this.substitutor.Substitute(propertyDefinition);
        }

        public override void Visit(IRootUnitNamespace rootUnitNamespace) {
          this.result = this.substitutor.Substitute(rootUnitNamespace);
        }

        public override void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
          this.result = this.substitutor.Substitute(rootUnitNamespaceReference);
        }

        public override void Visit(IRootUnitSetNamespace rootUnitSetNamespace) {
          this.result = this.substitutor.Substitute(rootUnitSetNamespace);
        }

        public override void Visit(ISpecializedEventDefinition specializedEventDefinition) {
          this.result = this.substitutor.Substitute(specializedEventDefinition);
        }

        public override void Visit(ISpecializedFieldDefinition specializedFieldDefinition) {
          this.result = this.substitutor.Substitute(specializedFieldDefinition);
        }

        public override void Visit(ISpecializedFieldReference specializedFieldReference) {
          this.result = this.substitutor.Substitute(specializedFieldReference);
        }

        public override void Visit(ISpecializedMethodDefinition specializedMethodDefinition) {
          this.result = this.substitutor.Substitute(specializedMethodDefinition);
        }

        public override void Visit(ISpecializedMethodReference specializedMethodReference) {
          this.result = this.substitutor.Substitute(specializedMethodReference);
        }

        public override void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition) {
          this.result = this.substitutor.Substitute(specializedPropertyDefinition);
        }

        public override void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
          this.result = this.substitutor.Substitute(specializedNestedTypeDefinition);
        }

        public override void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
          this.result = this.substitutor.Substitute(specializedNestedTypeReference);
        }

      }

      internal IArrayTypeReference Substitute(IArrayTypeReference arrayTypeReference) {
        if (arrayTypeReference is Dummy) return arrayTypeReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(arrayTypeReference, out copy)) return (ArrayTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(arrayTypeReference);
        this.ReferenceCache.Add(arrayTypeReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        mutableCopy.ElementType = this.SubstituteViaDispatcher(mutableCopy.ElementType);
        return mutableCopy;
      }

      internal IAssembly Substitute(IAssembly assembly) {
        if (assembly is Dummy) return Dummy.Assembly;
        var mutableCopy = (Assembly)this.DefinitionCache[assembly];
        this.Substitute((Module)mutableCopy);
        this.SubstituteElements(mutableCopy.AssemblyAttributes);
        this.SubstituteElements(mutableCopy.ExportedTypes);
        this.SubstituteElements(mutableCopy.Files);
        this.SubstituteElements(mutableCopy.MemberModules);
        this.SubstituteElements(mutableCopy.Resources);
        this.SubstituteElements(mutableCopy.SecurityAttributes);
        return mutableCopy;
      }

      internal IAssemblyReference Substitute(IAssemblyReference assemblyReference, bool keepAsDefinition = true) {
        Contract.Requires(assemblyReference != null);

        if (assemblyReference is Dummy) return assemblyReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(assemblyReference, out copy)) return (IAssemblyReference)copy;
        if (this.ReferenceCache.TryGetValue(assemblyReference, out copy)) return (AssemblyReference)copy;
        var mutableCopy = this.shallowCopier.Copy(assemblyReference);
        this.ReferenceCache.Add(assemblyReference, mutableCopy);
        mutableCopy.ContainingAssembly = mutableCopy;
        return mutableCopy;
      }

      internal ICustomAttribute Substitute(ICustomAttribute customAttribute) {
        if (customAttribute is Dummy) return customAttribute;
        var mutableCopy = this.shallowCopier.Copy(customAttribute);
        this.SubstituteElements(mutableCopy.Arguments);
        mutableCopy.Constructor = this.SubstituteViaDispatcher(mutableCopy.Constructor);
        this.SubstituteElements(mutableCopy.NamedArguments);
        return mutableCopy;
      }

      internal ICustomModifier Substitute(ICustomModifier customModifier) {
        if (customModifier is Dummy) return customModifier;
        var mutableCopy = this.shallowCopier.Copy(customModifier);
        mutableCopy.Modifier = this.SubstituteViaDispatcher(mutableCopy.Modifier);
        return mutableCopy;
      }

      internal IDefinition Substitute(IDefinition definition) {
        if (definition is Dummy) return definition;
        definition.Dispatch(this.Dispatcher);
        return (IDefinition)this.Dispatcher.result;
      }

      internal IEventDefinition Substitute(IEventDefinition eventDefinition) {
        if (eventDefinition is Dummy) return eventDefinition;
        var mutableCopy = this.shallowCopier.Copy(eventDefinition);
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IFieldDefinition Substitute(IFieldDefinition fieldDefinition) {
        if (fieldDefinition is Dummy) return fieldDefinition;
        var mutableCopy = (FieldDefinition)this.DefinitionCache[fieldDefinition];
        if (!(mutableCopy is IGlobalFieldDefinition)) this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IFieldReference Substitute(IFieldReference fieldReference, bool keepAsDefinition = true) {
        if (fieldReference is Dummy) return fieldReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(fieldReference, out copy)) return (IFieldReference)copy;
        if (this.ReferenceCache.TryGetValue(fieldReference, out copy)) return (FieldReference)copy;
        var mutableCopy = this.shallowCopier.Copy(fieldReference);
        this.ReferenceCache.Add(fieldReference, mutableCopy);
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IFileReference Substitute(IFileReference fileReference) {
        if (fileReference is Dummy) return fileReference;
        var mutableCopy = this.shallowCopier.Copy(fileReference);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingAssembly, out copy))
          mutableCopy.ContainingAssembly = (IAssembly)copy;
        return mutableCopy;
      }

      internal IFunctionPointerTypeReference Substitute(IFunctionPointerTypeReference functionPointerTypeReference) {
        if (functionPointerTypeReference is Dummy) return functionPointerTypeReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(functionPointerTypeReference, out copy)) return (FunctionPointerTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(functionPointerTypeReference);
        this.ReferenceCache.Add(functionPointerTypeReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        this.SubstituteElements(mutableCopy.ExtraArgumentTypes);
        this.SubstituteElements(mutableCopy.Parameters);
        if (mutableCopy.ReturnValueIsModified)
          this.SubstituteElements(mutableCopy.ReturnValueCustomModifiers);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
        return mutableCopy;
      }

      internal IGenericMethodInstanceReference Substitute(IGenericMethodInstanceReference genericMethodInstanceReference) {
        if (genericMethodInstanceReference is Dummy) return genericMethodInstanceReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(genericMethodInstanceReference, out copy)) return (GenericMethodInstanceReference)copy;
        var mutableCopy = this.shallowCopier.Copy(genericMethodInstanceReference);
        this.ReferenceCache.Add(genericMethodInstanceReference, mutableCopy);
        this.Substitute((MethodReference)mutableCopy);
        this.SubstituteElements(mutableCopy.GenericArguments);
        mutableCopy.GenericMethod = this.SubstituteViaDispatcher(mutableCopy.GenericMethod);
        return mutableCopy;
      }

      internal IGenericMethodParameter Substitute(IGenericMethodParameter genericMethodParameter) {
        if (genericMethodParameter is Dummy) return genericMethodParameter;
        var mutableCopy = (GenericMethodParameter)this.DefinitionCache[genericMethodParameter];
        this.Substitute((GenericParameter)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.DefiningMethod, out copy))
          mutableCopy.DefiningMethod = (IMethodDefinition)copy;
        return mutableCopy;
      }

      internal IGenericMethodParameterReference Substitute(IGenericMethodParameterReference genericMethodParameterReference, bool keepAsDefinition = true) {
        if (genericMethodParameterReference is Dummy) return genericMethodParameterReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(genericMethodParameterReference, out copy)) return (IGenericMethodParameterReference)copy;
        if (this.ReferenceCache.TryGetValue(genericMethodParameterReference, out copy)) return (GenericMethodParameterReference)copy;
        var mutableCopy = this.shallowCopier.Copy(genericMethodParameterReference);
        this.ReferenceCache.Add(genericMethodParameterReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        mutableCopy.DefiningMethod = this.SubstituteViaDispatcher(mutableCopy.DefiningMethod);
        return mutableCopy;
      }

      internal IGenericTypeInstanceReference Substitute(IGenericTypeInstanceReference genericTypeInstanceReference) {
        if (genericTypeInstanceReference is Dummy) return genericTypeInstanceReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(genericTypeInstanceReference, out copy)) return (GenericTypeInstanceReference)copy;
        var mutableCopy = this.shallowCopier.Copy(genericTypeInstanceReference);
        this.ReferenceCache.Add(genericTypeInstanceReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        this.SubstituteElements(mutableCopy.GenericArguments);
        mutableCopy.GenericType = this.SubstituteViaDispatcher(mutableCopy.GenericType);
        return mutableCopy;
      }

      internal IGenericTypeParameter Substitute(IGenericTypeParameter genericParameter) {
        if (genericParameter is Dummy) return genericParameter;
        var mutableCopy = (GenericTypeParameter)this.DefinitionCache[genericParameter];
        this.Substitute((GenericParameter)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.DefiningType, out copy))
          mutableCopy.DefiningType = (ITypeDefinition)copy;
        return mutableCopy;
      }

      internal IGenericTypeParameterReference Substitute(IGenericTypeParameterReference genericTypeParameterReference, bool keepAsDefinition = true) {
        if (genericTypeParameterReference is Dummy) return genericTypeParameterReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(genericTypeParameterReference, out copy)) return (IGenericTypeParameterReference)copy;
        if (this.ReferenceCache.TryGetValue(genericTypeParameterReference, out copy)) return (GenericTypeParameterReference)copy;
        var mutableCopy = this.shallowCopier.Copy(genericTypeParameterReference);
        this.ReferenceCache.Add(genericTypeParameterReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        mutableCopy.DefiningType = this.SubstituteViaDispatcher(mutableCopy.DefiningType);
        return mutableCopy;
      }

      internal IGlobalFieldDefinition Substitute(IGlobalFieldDefinition globalFieldDefinition) {
        if (globalFieldDefinition is Dummy) return globalFieldDefinition;
        var mutableCopy = (GlobalFieldDefinition)this.DefinitionCache[globalFieldDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IGlobalMethodDefinition Substitute(IGlobalMethodDefinition globalMethodDefinition) {
        if (globalMethodDefinition is Dummy) return globalMethodDefinition;
        var mutableCopy = (GlobalMethodDefinition)this.DefinitionCache[globalMethodDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal ILocalDefinition Substitute(ILocalDefinition localDefinition) {
        if (localDefinition is Dummy) return localDefinition;
        var mutableCopy = this.shallowCopier.Copy(localDefinition);
        this.DefinitionCache.Add(localDefinition, mutableCopy);
        if (mutableCopy.IsConstant)
          mutableCopy.CompileTimeValue = this.Substitute(mutableCopy.CompileTimeValue);
        if (mutableCopy.IsModified)
          this.SubstituteElements(mutableCopy.CustomModifiers);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.MethodDefinition, out copy))
          mutableCopy.MethodDefinition = (IMethodDefinition)copy;
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
        return mutableCopy;
      }

      internal IManagedPointerTypeReference Substitute(IManagedPointerTypeReference managedPointerTypeReference) {
        if (managedPointerTypeReference is Dummy) return managedPointerTypeReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(managedPointerTypeReference, out copy)) return (ManagedPointerTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(managedPointerTypeReference);
        this.ReferenceCache.Add(managedPointerTypeReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        mutableCopy.TargetType = this.SubstituteViaDispatcher(mutableCopy.TargetType);
        return mutableCopy;
      }

      internal IMarshallingInformation Substitute(IMarshallingInformation marshallingInformation) {
        if (marshallingInformation is Dummy) return marshallingInformation;
        var mutableCopy = this.shallowCopier.Copy(marshallingInformation);
        if (mutableCopy.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler)
          mutableCopy.CustomMarshaller = this.SubstituteViaDispatcher(mutableCopy.CustomMarshaller);
        if (mutableCopy.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.SafeArray && 
        (mutableCopy.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_DISPATCH ||
        mutableCopy.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_UNKNOWN ||
        mutableCopy.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_RECORD))
          mutableCopy.SafeArrayElementUserDefinedSubtype = this.SubstituteViaDispatcher(mutableCopy.SafeArrayElementUserDefinedSubtype);
        return mutableCopy;
      }

      internal IMetadataConstant Substitute(IMetadataConstant constant) {
        if (constant is Dummy) return constant;
        var mutableCopy = this.shallowCopier.Copy(constant);
        this.Substitute((MetadataExpression)mutableCopy);
        return mutableCopy;
      }

      internal IMetadataCreateArray Substitute(IMetadataCreateArray createArray) {
        if (createArray is Dummy) return createArray;
        var mutableCopy = this.shallowCopier.Copy(createArray);
        this.Substitute((MetadataExpression)mutableCopy);
        mutableCopy.ElementType = this.SubstituteViaDispatcher(mutableCopy.ElementType);
        this.SubstituteElements(mutableCopy.Initializers);
        return mutableCopy;
      }

      internal IMetadataNamedArgument Substitute(IMetadataNamedArgument namedArgument) {
        if (namedArgument is Dummy) return namedArgument;
        var mutableCopy = this.shallowCopier.Copy(namedArgument);
        this.Substitute((MetadataExpression)mutableCopy);
        mutableCopy.ArgumentValue = this.SubstituteViaDispatcher(mutableCopy.ArgumentValue);
        mutableCopy.ResolvedDefinition = null;
        return mutableCopy;
      }

      internal IMetadataTypeOf Substitute(IMetadataTypeOf typeOf) {
        if (typeOf is Dummy) return typeOf;
        var mutableCopy = this.shallowCopier.Copy(typeOf);
        this.Substitute((MetadataExpression)mutableCopy);
        mutableCopy.TypeToGet = this.SubstituteViaDispatcher(mutableCopy.TypeToGet);
        return mutableCopy;
      }

      internal IMethodBody Substitute(IMethodBody methodBody, IMethodDefinition method) {
        if (methodBody is Dummy) return methodBody;
        return this.deepCopier.CopyMethodBody(methodBody, method);
      }

      internal IMethodBody Substitute(MethodBody mutableCopy) {
        this.SubstituteElements(mutableCopy.LocalVariables);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.MethodDefinition, out copy))
          mutableCopy.MethodDefinition = (IMethodDefinition)copy;
        if (!mutableCopy.MethodDefinition.IsAbstract && !mutableCopy.MethodDefinition.IsExternal && mutableCopy.MethodDefinition.IsCil) {
          this.SubstituteElements(mutableCopy.Operations);
          this.SubstituteElements(mutableCopy.OperationExceptionInformation);
        }
        return mutableCopy;
      }

      internal IMethodDefinition Substitute(IMethodDefinition method) {
        if (method is Dummy) return method;
        var mutableCopy = (MethodDefinition)this.DefinitionCache[method];
        if (!(mutableCopy is IGlobalMethodDefinition)) this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IMethodImplementation Substitute(IMethodImplementation methodImplementation) {
        if (methodImplementation is Dummy) return methodImplementation;
        var mutableCopy = this.shallowCopier.Copy(methodImplementation);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingType, out copy))
          mutableCopy.ContainingType = (ITypeDefinition)copy;
        mutableCopy.ImplementedMethod = this.SubstituteViaDispatcher(mutableCopy.ImplementedMethod);
        mutableCopy.ImplementingMethod = this.SubstituteViaDispatcher(mutableCopy.ImplementingMethod);
        return mutableCopy;
      }

      internal IMethodReference Substitute(IMethodReference methodReference, bool keepAsDefinition = true) {
        if (methodReference is Dummy) return methodReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(methodReference, out copy)) return (IMethodReference)copy;
        if (this.ReferenceCache.TryGetValue(methodReference, out copy)) return (MethodReference)copy;
        var mutableCopy = this.shallowCopier.Copy(methodReference);
        this.ReferenceCache.Add(methodReference, mutableCopy);
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IModifiedTypeReference Substitute(IModifiedTypeReference modifiedTypeReference) {
        if (modifiedTypeReference is Dummy) return modifiedTypeReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(modifiedTypeReference, out copy)) return (ModifiedTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(modifiedTypeReference);
        this.ReferenceCache.Add(modifiedTypeReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        this.SubstituteElements(mutableCopy.CustomModifiers);
        mutableCopy.UnmodifiedType = this.SubstituteViaDispatcher(mutableCopy.UnmodifiedType);
        return mutableCopy;
      }

      internal IModule Substitute(IModule module) {
        if (module is Dummy) return module;
        var mutableCopy = (Module)this.DefinitionCache[module];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IModuleReference Substitute(IModuleReference moduleReference, bool keepAsDefinition = true) {
        if (moduleReference is Dummy) return moduleReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(moduleReference, out copy)) return (IModuleReference)copy;
        if (this.ReferenceCache.TryGetValue(moduleReference, out copy)) return (ModuleReference)copy;
        var mutableCopy = this.shallowCopier.Copy(moduleReference);
        this.ReferenceCache.Add(moduleReference, mutableCopy);
        if (mutableCopy.ContainingAssembly != null)
          mutableCopy.ContainingAssembly = this.Substitute(mutableCopy.ContainingAssembly);
        return mutableCopy;
      }

      internal INamespaceAliasForType Substitute(INamespaceAliasForType namespaceAliasForType) {
        if (namespaceAliasForType is Dummy) return namespaceAliasForType;
        var mutableCopy = (NamespaceAliasForType)this.DefinitionCache[namespaceAliasForType];
        this.Substitute((AliasForType)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingNamespace, out copy))
          mutableCopy.ContainingNamespace = (INamespaceDefinition)copy;
        return mutableCopy;
      }

      internal INamespaceTypeDefinition Substitute(INamespaceTypeDefinition namespaceTypeDefinition) {
        if (namespaceTypeDefinition is Dummy) return namespaceTypeDefinition;
        var mutableCopy = (NamespaceTypeDefinition)this.DefinitionCache[namespaceTypeDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal INamespaceTypeReference Substitute(INamespaceTypeReference namespaceTypeReference, bool keepAsDefinition = true) {
        if (namespaceTypeReference is Dummy) return namespaceTypeReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(namespaceTypeReference, out copy)) return (INamespaceTypeReference)copy;
        if (this.ReferenceCache.TryGetValue(namespaceTypeReference, out copy)) return (NamespaceTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(namespaceTypeReference);
        this.ReferenceCache.Add(namespaceTypeReference, mutableCopy);
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal INestedAliasForType Substitute(INestedAliasForType nestedAliasForType) {
        if (nestedAliasForType is Dummy) return nestedAliasForType;
        var mutableCopy = (NestedAliasForType)this.DefinitionCache[nestedAliasForType];
        this.Substitute((AliasForType)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingAlias, out copy))
          mutableCopy.ContainingAlias = (IAliasForType)copy;
        return mutableCopy;
      }

      internal INestedTypeDefinition Substitute(INestedTypeDefinition nestedTypeDefinition) {
        if (nestedTypeDefinition is Dummy) return nestedTypeDefinition;
        var mutableCopy = (NestedTypeDefinition)this.DefinitionCache[nestedTypeDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal INestedTypeReference Substitute(INestedTypeReference nestedTypeReference, bool keepAsDefinition = true) {
        if (nestedTypeReference is Dummy) return nestedTypeReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(nestedTypeReference, out copy)) return (INestedTypeReference)copy;
        if (this.ReferenceCache.TryGetValue(nestedTypeReference, out copy)) return (NestedTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(nestedTypeReference);
        this.ReferenceCache.Add(nestedTypeReference, mutableCopy);
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal INestedUnitNamespace Substitute(INestedUnitNamespace nestedUnitNamespace) {
        if (nestedUnitNamespace is Dummy) return nestedUnitNamespace;
        var mutableCopy = (NestedUnitNamespace)this.DefinitionCache[nestedUnitNamespace];
        this.Substitute((UnitNamespace)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingUnitNamespace, out copy))
          mutableCopy.ContainingUnitNamespace = (IUnitNamespace)copy;
        return mutableCopy;
      }

      internal INestedUnitNamespaceReference Substitute(INestedUnitNamespaceReference nestedUnitNamespaceReference, bool keepAsDefinition = true) {
        if (nestedUnitNamespaceReference is Dummy) return nestedUnitNamespaceReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(nestedUnitNamespaceReference, out copy)) return (INestedUnitNamespaceReference)copy;
        if (this.ReferenceCache.TryGetValue(nestedUnitNamespaceReference, out copy)) return (NestedUnitNamespaceReference)copy;
        var mutableCopy = this.shallowCopier.Copy(nestedUnitNamespaceReference);
        this.ReferenceCache.Add(nestedUnitNamespaceReference, mutableCopy);
        this.Substitute((UnitNamespaceReference)mutableCopy);
        mutableCopy.ContainingUnitNamespace = this.SubstituteViaDispatcher(mutableCopy.ContainingUnitNamespace);
        return mutableCopy;
      }

      internal IOperation Substitute(IOperation operation) {
        if (operation is Dummy) return Dummy.Operation;
        var mutableCopy = this.shallowCopier.Copy(operation);
        if (mutableCopy.Value == null) return mutableCopy;
        var typeReference = mutableCopy.Value as ITypeReference;
        if (typeReference != null)
          mutableCopy.Value = this.SubstituteViaDispatcher(typeReference);
        else {
          var fieldReference = mutableCopy.Value as IFieldReference;
          if (fieldReference != null)
            mutableCopy.Value = this.SubstituteViaDispatcher(fieldReference);
          else {
            var methodReference = mutableCopy.Value as IMethodReference;
            if (methodReference != null)
              mutableCopy.Value = this.SubstituteViaDispatcher(methodReference);
            else {
              var parameter = mutableCopy.Value as IParameterDefinition;
              if (parameter != null)
                mutableCopy.Value = this.SubstituteReference(parameter);
              else {
                var local = mutableCopy.Value as ILocalDefinition;
                if (local != null)
                  mutableCopy.Value = this.SubstituteReference(local);
              }
            }
          }
        }
        return mutableCopy;
      }

      internal IOperationExceptionInformation Substitute(IOperationExceptionInformation operationExceptionInformation) {
        if (operationExceptionInformation is Dummy) return operationExceptionInformation;
        var mutableCopy = this.shallowCopier.Copy(operationExceptionInformation);
        mutableCopy.ExceptionType = this.SubstituteViaDispatcher(mutableCopy.ExceptionType);
        return mutableCopy;
      }

      internal IParameterDefinition Substitute(IParameterDefinition parameterDefinition) {
        if (parameterDefinition is Dummy) return parameterDefinition;
        var mutableCopy = (ParameterDefinition)this.DefinitionCache[parameterDefinition];
        this.SubstituteElements(mutableCopy.Attributes);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingSignature, out copy))
          mutableCopy.ContainingSignature = (ISignature)copy;
        if (mutableCopy.IsModified)
          this.SubstituteElements(mutableCopy.CustomModifiers);
        if (mutableCopy.HasDefaultValue)
          mutableCopy.DefaultValue = this.Substitute(mutableCopy.DefaultValue);
        if (mutableCopy.IsMarshalledExplicitly)
          mutableCopy.MarshallingInformation = this.Substitute(mutableCopy.MarshallingInformation);
        if (mutableCopy.IsParameterArray)
          mutableCopy.ParamArrayElementType = this.SubstituteViaDispatcher(mutableCopy.ParamArrayElementType);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
        return mutableCopy;
      }

      internal IParameterTypeInformation Substitute(IParameterTypeInformation parameterTypeInformation, bool keepAsDefinition = true) {
        if (parameterTypeInformation is Dummy) return parameterTypeInformation;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(parameterTypeInformation, out copy)) return (IParameterTypeInformation)copy;
        if (this.ReferenceCache.TryGetValue(parameterTypeInformation, out copy)) return (ParameterTypeInformation)copy;
        var mutableCopy = this.shallowCopier.Copy(parameterTypeInformation);
        this.ReferenceCache.Add(parameterTypeInformation, mutableCopy);
        var method = mutableCopy.ContainingSignature as IMethodReference;
        if (method != null)
          mutableCopy.ContainingSignature = this.SubstituteViaDispatcher(method);
        else
          mutableCopy.ContainingSignature = this.Substitute((IFunctionPointerTypeReference)mutableCopy.ContainingSignature);
        if (mutableCopy.IsModified)
          this.SubstituteElements(mutableCopy.CustomModifiers);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
        return mutableCopy;
      }

      internal IPESection Substitute(IPESection peSection) {
        if (peSection is Dummy) return peSection;
        var mutableCopy = this.shallowCopier.Copy(peSection);
        return mutableCopy;
      }

      internal IPlatformInvokeInformation Substitute(IPlatformInvokeInformation platformInvokeInformation) {
        if (platformInvokeInformation is Dummy) return platformInvokeInformation;
        var mutableCopy = this.shallowCopier.Copy(platformInvokeInformation);
        mutableCopy.ImportModule = this.Substitute(mutableCopy.ImportModule);
        return mutableCopy;
      }

      internal IPointerTypeReference Substitute(IPointerTypeReference pointerTypeReference) {
        if (pointerTypeReference is Dummy) return pointerTypeReference;
        object copy;
        if (this.ReferenceCache.TryGetValue(pointerTypeReference, out copy)) return (PointerTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(pointerTypeReference);
        this.ReferenceCache.Add(pointerTypeReference, mutableCopy);
        this.Substitute((TypeReference)mutableCopy);
        mutableCopy.TargetType = this.SubstituteViaDispatcher(mutableCopy.TargetType);
        return mutableCopy;
      }

      internal IPropertyDefinition Substitute(IPropertyDefinition propertyDefinition) {
        if (propertyDefinition is Dummy) return propertyDefinition;
        PropertyDefinition mutableCopy = (PropertyDefinition)this.DefinitionCache[propertyDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal IResourceReference Substitute(IResourceReference resourceReference) {
        if (resourceReference is Dummy) return Dummy.Resource;
        var mutableCopy = this.shallowCopier.Copy(resourceReference);
        this.SubstituteElements(mutableCopy.Attributes);
        mutableCopy.DefiningAssembly = this.Substitute(mutableCopy.DefiningAssembly);
        return mutableCopy;
      }

      internal IRootUnitNamespace Substitute(IRootUnitNamespace rootUnitNamespace) {
        if (rootUnitNamespace is Dummy) return rootUnitNamespace;
        var mutableCopy = (RootUnitNamespace)this.DefinitionCache[rootUnitNamespace];
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.Unit, out copy))
          mutableCopy.Unit = (IUnit)copy;
        this.Substitute((UnitNamespace)mutableCopy);
        return mutableCopy;
      }

      internal IRootUnitNamespaceReference Substitute(IRootUnitNamespaceReference rootUnitNamespaceReference, bool keepAsDefinition = true) {
        if (rootUnitNamespaceReference is Dummy) return rootUnitNamespaceReference;
        object copy;
        if (keepAsDefinition && this.DefinitionCache.TryGetValue(rootUnitNamespaceReference, out copy)) return (IRootUnitNamespaceReference)copy;
        if (this.ReferenceCache.TryGetValue(rootUnitNamespaceReference, out copy)) return (RootUnitNamespaceReference)copy;
        var mutableCopy = this.shallowCopier.Copy(rootUnitNamespaceReference);
        this.ReferenceCache.Add(rootUnitNamespaceReference, mutableCopy);
        this.Substitute((UnitNamespaceReference)mutableCopy);
        mutableCopy.Unit = this.SubstituteViaDispatcher(mutableCopy.Unit);
        return mutableCopy;
      }

      internal ISecurityAttribute Substitute(ISecurityAttribute securityAttribute) {
        if (securityAttribute is Dummy) return securityAttribute;
        var mutableCopy = this.shallowCopier.Copy(securityAttribute);
        this.SubstituteElements(mutableCopy.Attributes);
        return mutableCopy;
      }

      internal ISpecializedEventDefinition Substitute(ISpecializedEventDefinition specializedEventDefinition) {
        if (specializedEventDefinition is Dummy) return specializedEventDefinition;
        var mutableCopy = this.shallowCopier.Copy(specializedEventDefinition);
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal ISpecializedFieldDefinition Substitute(ISpecializedFieldDefinition specializedFieldDefinition) {
        if (specializedFieldDefinition is Dummy) return specializedFieldDefinition;
        SpecializedFieldDefinition mutableCopy = (SpecializedFieldDefinition)this.DefinitionCache[specializedFieldDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal ISpecializedFieldReference Substitute(ISpecializedFieldReference fieldReference) {
        if (fieldReference is Dummy) return fieldReference;
        object copy;
        if (this.DefinitionCache.TryGetValue(fieldReference, out copy)) return (ISpecializedFieldReference)copy;
        if (this.ReferenceCache.TryGetValue(fieldReference, out copy)) return (SpecializedFieldReference)copy;
        var mutableCopy = this.shallowCopier.Copy(fieldReference);
        this.ReferenceCache.Add(fieldReference, mutableCopy);
        this.Substitute((FieldReference)mutableCopy);
        mutableCopy.UnspecializedVersion = this.Substitute(mutableCopy.UnspecializedVersion);
        return mutableCopy;
      }

      internal ISpecializedMethodDefinition Substitute(ISpecializedMethodDefinition specializedMethodDefinition) {
        if (specializedMethodDefinition is Dummy) return specializedMethodDefinition;
        SpecializedMethodDefinition mutableCopy = (SpecializedMethodDefinition)this.DefinitionCache[specializedMethodDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal ISpecializedMethodReference Substitute(ISpecializedMethodReference methodReference) {
        if (methodReference is Dummy) return methodReference;
        object copy;
        if (this.DefinitionCache.TryGetValue(methodReference, out copy)) return (ISpecializedMethodReference)copy;
        if (this.ReferenceCache.TryGetValue(methodReference, out copy)) return (SpecializedMethodReference)copy;
        var mutableCopy = this.shallowCopier.Copy(methodReference);
        this.ReferenceCache.Add(methodReference, mutableCopy);
        this.Substitute((MethodReference)mutableCopy);
        mutableCopy.UnspecializedVersion = this.Substitute(mutableCopy.UnspecializedVersion);
        return mutableCopy;
      }

      internal ISpecializedPropertyDefinition Substitute(ISpecializedPropertyDefinition specializedPropertyDefinition) {
        if (specializedPropertyDefinition is Dummy) return specializedPropertyDefinition;
        SpecializedPropertyDefinition mutableCopy = (SpecializedPropertyDefinition)this.DefinitionCache[specializedPropertyDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal ISpecializedNestedTypeDefinition Substitute(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
        if (specializedNestedTypeDefinition is Dummy) return specializedNestedTypeDefinition;
        SpecializedNestedTypeDefinition mutableCopy = (SpecializedNestedTypeDefinition)this.DefinitionCache[specializedNestedTypeDefinition];
        this.Substitute(mutableCopy);
        return mutableCopy;
      }

      internal ISpecializedNestedTypeReference Substitute(ISpecializedNestedTypeReference nestedTypeReference) {
        if (nestedTypeReference is Dummy) return nestedTypeReference;
        object copy;
        if (this.DefinitionCache.TryGetValue(nestedTypeReference, out copy)) return (ISpecializedNestedTypeReference)copy;
        if (this.ReferenceCache.TryGetValue(nestedTypeReference, out copy)) return (SpecializedNestedTypeReference)copy;
        var mutableCopy = this.shallowCopier.Copy(nestedTypeReference);
        this.ReferenceCache.Add(nestedTypeReference, mutableCopy);
        this.Substitute((NestedTypeReference)mutableCopy);
        mutableCopy.UnspecializedVersion = this.Substitute(mutableCopy.UnspecializedVersion);
        return mutableCopy;
      }

      internal IWin32Resource Substitute(IWin32Resource win32Resource) {
        if (win32Resource is Dummy) return win32Resource;
        var mutableCopy = this.shallowCopier.Copy(win32Resource);
        return mutableCopy;
      }

      private void Substitute(AliasForType mutableCopy) {
        mutableCopy.AliasedType = this.SubstituteViaDispatcher(mutableCopy.AliasedType);
        this.SubstituteElements(mutableCopy.Attributes);
        this.SubstituteElements(mutableCopy.Members);
      }

      private void Substitute(EventDefinition mutableCopy) {
        this.Substitute((TypeDefinitionMember)mutableCopy);
        this.SubstituteElements(mutableCopy.Accessors);
        mutableCopy.Adder = this.SubstituteViaDispatcher(mutableCopy.Adder);
        if (mutableCopy.Caller != null)
          mutableCopy.Caller = this.SubstituteViaDispatcher(mutableCopy.Caller);
        mutableCopy.Remover = this.SubstituteViaDispatcher(mutableCopy.Remover);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(SpecializedEventDefinition mutableCopy) {
        this.Substitute((EventDefinition)mutableCopy);
        mutableCopy.UnspecializedVersion = this.SubstituteReference(mutableCopy.UnspecializedVersion);
      }

      private void Substitute(FieldDefinition mutableCopy) {
        this.Substitute((TypeDefinitionMember)mutableCopy);
        if (!(mutableCopy.CompileTimeValue is Dummy))
          mutableCopy.CompileTimeValue = this.Substitute(mutableCopy.CompileTimeValue);
        if (mutableCopy.IsModified)
          this.SubstituteElements(mutableCopy.CustomModifiers);
        if (mutableCopy.IsMarshalledExplicitly)
          mutableCopy.MarshallingInformation = this.Substitute(mutableCopy.MarshallingInformation);
        mutableCopy.InternFactory = this.host.InternFactory;
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(SpecializedFieldDefinition mutableCopy) {
        this.Substitute((FieldDefinition)mutableCopy);
        mutableCopy.UnspecializedVersion = this.SubstituteReference(mutableCopy.UnspecializedVersion);
      }

      private void Substitute(GlobalFieldDefinition mutableCopy) {
        this.Substitute((FieldDefinition)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingNamespace, out copy))
          mutableCopy.ContainingNamespace = (INamespaceDefinition)copy;
      }

      private void Substitute(GlobalMethodDefinition mutableCopy) {
        this.Substitute((MethodDefinition)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingNamespace, out copy))
          mutableCopy.ContainingNamespace = (INamespaceDefinition)copy;
      }

      private void Substitute(FieldReference mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
        mutableCopy.ContainingType = this.SubstituteViaDispatcher(mutableCopy.ContainingType);
        if (mutableCopy.IsModified)
          this.SubstituteElements(mutableCopy.CustomModifiers);
        mutableCopy.InternFactory = this.host.InternFactory;
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(GenericParameter mutableCopy) {
        this.Substitute((NamedTypeDefinition)mutableCopy);
        this.SubstituteElements(mutableCopy.Constraints);
      }

      private void Substitute(MetadataExpression mutableCopy) {
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(MethodDefinition mutableCopy) {
        this.Substitute((TypeDefinitionMember)mutableCopy);
        if (!mutableCopy.IsAbstract && !mutableCopy.IsExternal)
          mutableCopy.Body = this.Substitute(mutableCopy.Body, mutableCopy);
        if (mutableCopy.IsGeneric)
          this.SubstituteElements(mutableCopy.GenericParameters);
        mutableCopy.InternFactory = this.host.InternFactory;
        this.SubstituteElements(mutableCopy.Parameters);
        if (mutableCopy.IsPlatformInvoke)
          mutableCopy.PlatformInvokeData = this.Substitute(mutableCopy.PlatformInvokeData);
        this.SubstituteElements(mutableCopy.ReturnValueAttributes);
        if (mutableCopy.ReturnValueIsModified)
          this.SubstituteElements(mutableCopy.ReturnValueCustomModifiers);
        if (mutableCopy.ReturnValueIsMarshalledExplicitly)
          mutableCopy.ReturnValueMarshallingInformation = this.Substitute(mutableCopy.ReturnValueMarshallingInformation);
        this.SubstituteElements(mutableCopy.SecurityAttributes);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(SpecializedMethodDefinition mutableCopy) {
        this.Substitute((MethodDefinition)mutableCopy);
        mutableCopy.UnspecializedVersion = this.SubstituteReference(mutableCopy.UnspecializedVersion);
      }

      private void Substitute(MethodReference mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
        mutableCopy.ContainingType = this.SubstituteViaDispatcher(mutableCopy.ContainingType);
        this.SubstituteElements(mutableCopy.ExtraParameters);
        mutableCopy.InternFactory = this.host.InternFactory;
        this.SubstituteElements(mutableCopy.Parameters);
        if (mutableCopy.ReturnValueIsModified)
          this.SubstituteElements(mutableCopy.ReturnValueCustomModifiers);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(Module mutableCopy) {
        this.SubstituteElements(mutableCopy.AssemblyReferences);
        if (mutableCopy.ContainingAssembly != null && !(mutableCopy is Assembly)) {
          object copy;
          if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingAssembly, out copy))
            mutableCopy.ContainingAssembly = (IAssembly)copy;
        }
        if (mutableCopy.Kind == ModuleKind.ConsoleApplication || mutableCopy.Kind == ModuleKind.WindowsApplication)
          mutableCopy.EntryPoint = this.SubstituteViaDispatcher(mutableCopy.EntryPoint);
        this.SubstituteElements(mutableCopy.ModuleAttributes);
        this.SubstituteElements(mutableCopy.ModuleReferences);
        this.SubstituteElements(mutableCopy.Win32Resources);
        mutableCopy.PlatformType = this.host.PlatformType;
        mutableCopy.UnitNamespaceRoot = this.Substitute(mutableCopy.UnitNamespaceRoot);
        var allTypes = mutableCopy.AllTypes;
        var n = allTypes.Count;
        INamedTypeDefinition moduleType = null;
        if (n > 0) moduleType = allTypes[0];
        for (int i = 0; i < n; i++) {
          var type = allTypes[i];
          object copy;
          if (this.DefinitionCache.TryGetValue(type, out copy))
            allTypes[i] = (INamedTypeDefinition)copy;
          else {
            //Dealing with a type that cannot be reached via UnitNamespaceRoot. Typically this is the <Module> type.
            var mutableType = this.shallowCopier.Copy(type);
            if (i != 0) this.Substitute(mutableType);
            allTypes[i] = mutableType;
          }
        }
        if (moduleType != null) this.Substitute(moduleType); //This will not have been visited via Substitute(mutableCopy.UnitNamespaceRoot);
        var typeReferences = mutableCopy.TypeReferences;
        if (typeReferences != null) {
          for (int i = 0; i < typeReferences.Count; i++) {
            var typeReference = typeReferences[i];
            object copy;
            if (this.ReferenceCache.TryGetValue(typeReference, out copy))
              typeReferences[i] = (ITypeReference)copy;
            else
              typeReferences.RemoveAt(i--);
          }
        }
        var typeMemberReferences = mutableCopy.TypeMemberReferences;
        if (typeMemberReferences != null) {
          for (int i = 0; i < typeMemberReferences.Count; i++) {
            var typeMemberReference = typeMemberReferences[i];
            object copy;
            if (this.ReferenceCache.TryGetValue(typeMemberReference, out copy))
              typeMemberReferences[i] = (ITypeMemberReference)copy;
            else
              typeMemberReferences.RemoveAt(i--);
          }
        }

      }

      private void Substitute(NamespaceTypeDefinition mutableCopy) {
        this.Substitute((NamedTypeDefinition)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingUnitNamespace, out copy))
          mutableCopy.ContainingUnitNamespace = (IUnitNamespace)copy;
      }

      private void Substitute(NamespaceTypeReference namespaceTypeReference) {
        this.Substitute((TypeReference)namespaceTypeReference);
        namespaceTypeReference.ContainingUnitNamespace = this.SubstituteViaDispatcher(namespaceTypeReference.ContainingUnitNamespace);
      }

      private void Substitute(NestedTypeDefinition mutableCopy) {
        this.Substitute((NamedTypeDefinition)mutableCopy);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingTypeDefinition, out copy))
          mutableCopy.ContainingTypeDefinition = (ITypeDefinition)copy;
      }

      private void Substitute(SpecializedNestedTypeDefinition mutableCopy) {
        this.Substitute((NestedTypeDefinition)mutableCopy);
        mutableCopy.UnspecializedVersion = this.SubstituteReference(mutableCopy.UnspecializedVersion);
      }

      private void Substitute(NestedTypeReference mutableCopy) {
        this.Substitute((TypeReference)mutableCopy);
        mutableCopy.ContainingType = this.SubstituteViaDispatcher(mutableCopy.ContainingType);
      }

      private void Substitute(PropertyDefinition mutableCopy) {
        this.Substitute((TypeDefinitionMember)mutableCopy);
        this.SubstituteElements(mutableCopy.Accessors);
        if (mutableCopy.HasDefaultValue)
          mutableCopy.DefaultValue = this.Substitute(mutableCopy.DefaultValue);
        if (mutableCopy.Getter != null)
          mutableCopy.Getter = this.SubstituteViaDispatcher(mutableCopy.Getter);
        this.SubstituteElements(mutableCopy.Parameters);
        if (mutableCopy.ReturnValueIsModified)
          this.SubstituteElements(mutableCopy.ReturnValueCustomModifiers);
        if (mutableCopy.Setter != null)
          mutableCopy.Setter = this.Substitute(mutableCopy.Setter);
        mutableCopy.Type = this.SubstituteViaDispatcher(mutableCopy.Type);
      }

      private void Substitute(SpecializedPropertyDefinition mutableCopy) {
        this.Substitute((PropertyDefinition)mutableCopy);
        mutableCopy.UnspecializedVersion = this.SubstituteReference(mutableCopy.UnspecializedVersion);
      }

      private void Substitute(NamedTypeDefinition mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
        this.SubstituteElements(mutableCopy.BaseClasses);
        this.SubstituteElements(mutableCopy.Events);
        this.SubstituteElements(mutableCopy.ExplicitImplementationOverrides);
        this.SubstituteElements(mutableCopy.Fields);
        if (mutableCopy.IsGeneric)
          this.SubstituteElements(mutableCopy.GenericParameters);
        this.SubstituteElements(mutableCopy.Interfaces);
        mutableCopy.InternFactory = this.host.InternFactory;
        this.SubstituteElements(mutableCopy.Methods);
        this.SubstituteElements(mutableCopy.NestedTypes);
        mutableCopy.PlatformType = this.host.PlatformType;
        this.SubstituteElements(mutableCopy.Properties);
        if (mutableCopy.HasDeclarativeSecurity)
          this.SubstituteElements(mutableCopy.SecurityAttributes);
        if (mutableCopy.IsEnum)
          mutableCopy.UnderlyingType = this.SubstituteViaDispatcher(mutableCopy.UnderlyingType);
      }

      private void Substitute(TypeDefinitionMember mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
        object copy;
        if (this.DefinitionCache.TryGetValue(mutableCopy.ContainingTypeDefinition, out copy))
          mutableCopy.ContainingTypeDefinition = (ITypeDefinition)copy;
      }

      private void Substitute(TypeReference mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
        mutableCopy.InternFactory = this.host.InternFactory;
        mutableCopy.PlatformType = this.host.PlatformType;
      }

      private void Substitute(UnitNamespace mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
        this.SubstituteElements(mutableCopy.Members);
      }

      private void Substitute(UnitNamespaceReference mutableCopy) {
        this.SubstituteElements(mutableCopy.Attributes);
      }

      private void SubstituteElements(List<IAliasForType>/*?*/ aliasesForTypes) {
        if (aliasesForTypes == null) return;
        for (int i = 0, n = aliasesForTypes.Count; i < n; i++)
          aliasesForTypes[i] = this.SubstituteViaDispatcher(aliasesForTypes[i]);
      }

      private void SubstituteElements(List<IAliasMember>/*?*/ aliasesMembers) {
        if (aliasesMembers == null) return;
        for (int i = 0, n = aliasesMembers.Count; i < n; i++) {
          var member = aliasesMembers[i];
          object copy;
          if (this.DefinitionCache.TryGetValue(member, out copy))
            aliasesMembers[i] = (IAliasMember)copy;
        }
      }

      private void SubstituteElements(List<IAssemblyReference>/*?*/ assemblyReferences) {
        if (assemblyReferences == null) return;
        for (int i = 0, n = assemblyReferences.Count; i < n; i++)
          assemblyReferences[i] = this.Substitute(assemblyReferences[i]);
      }

      private void SubstituteElements(List<ICustomAttribute>/*?*/ customAttributes) {
        if (customAttributes == null) return;
        for (int i = 0, n = customAttributes.Count; i < n; i++)
          customAttributes[i] = this.Substitute(customAttributes[i]);
      }

      private void SubstituteElements(List<ICustomModifier>/*?*/ customModifiers) {
        if (customModifiers == null) return;
        for (int i = 0, n = customModifiers.Count; i < n; i++)
          customModifiers[i] = this.Substitute(customModifiers[i]);
      }

      private void SubstituteElements(List<IEventDefinition>/*?*/ events) {
        if (events == null) return;
        for (int i = 0, n = events.Count; i < n; i++)
          events[i] = this.Substitute(events[i]);
      }

      private void SubstituteElements(List<IFieldDefinition>/*?*/ fields) {
        if (fields == null) return;
        for (int i = 0, n = fields.Count; i < n; i++)
          fields[i] = this.Substitute(fields[i]);
      }

      private void SubstituteElements(List<IFileReference>/*?*/ fileReferences) {
        if (fileReferences == null) return;
        for (int i = 0, n = fileReferences.Count; i < n; i++)
          fileReferences[i] = this.Substitute(fileReferences[i]);
      }

      private void SubstituteElements(List<IGenericMethodParameter>/*?*/ genericParameters) {
        if (genericParameters == null) return;
        for (int i = 0, n = genericParameters.Count; i < n; i++)
          genericParameters[i] = this.Substitute(genericParameters[i]);
      }

      private void SubstituteElements(List<IGenericTypeParameter>/*?*/ genericParameters) {
        if (genericParameters == null) return;
        for (int i = 0, n = genericParameters.Count; i < n; i++)
          genericParameters[i] = this.Substitute(genericParameters[i]);
      }

      private void SubstituteElements(List<ILocalDefinition>/*?*/ localDefinitions) {
        if (localDefinitions == null) return;
        for (int i = 0, n = localDefinitions.Count; i < n; i++)
          localDefinitions[i] = this.Substitute(localDefinitions[i]);
      }

      private void SubstituteElements(List<IMetadataExpression>/*?*/ expressions) {
        if (expressions == null) return;
        for (int i = 0, n = expressions.Count; i < n; i++)
          expressions[i] = this.SubstituteViaDispatcher(expressions[i]);
      }

      private void SubstituteElements(List<IMetadataNamedArgument>/*?*/ namedArguments) {
        if (namedArguments == null) return;
        for (int i = 0, n = namedArguments.Count; i < n; i++)
          namedArguments[i] = this.Substitute(namedArguments[i]);
      }

      private void SubstituteElements(List<IMethodDefinition>/*?*/ methods) {
        if (methods == null) return;
        for (int i = 0, n = methods.Count; i < n; i++)
          methods[i] = this.Substitute(methods[i]);
      }

      private void SubstituteElements(List<IMethodImplementation>/*?*/ methodImplementations) {
        if (methodImplementations == null) return;
        for (int i = 0, n = methodImplementations.Count; i < n; i++)
          methodImplementations[i] = this.Substitute(methodImplementations[i]);
      }

      private void SubstituteElements(List<IMethodReference>/*?*/ methodReferences) {
        if (methodReferences == null) return;
        for (int i = 0, n = methodReferences.Count; i < n; i++)
          methodReferences[i] = this.Substitute(methodReferences[i]);
      }

      private void SubstituteElements(List<IModule>/*?*/ modules) {
        if (modules == null) return;
        for (int i = 0, n = modules.Count; i < n; i++)
          modules[i] = this.Substitute(modules[i]);
      }

      private void SubstituteElements(List<IModuleReference>/*?*/ moduleReferences) {
        if (moduleReferences == null) return;
        for (int i = 0, n = moduleReferences.Count; i < n; i++)
          moduleReferences[i] = this.Substitute(moduleReferences[i]);
      }

      private void SubstituteElements(List<INamespaceMember>/*?*/ namespaceMembers) {
        if (namespaceMembers == null) return;
        for (int i = 0, n = namespaceMembers.Count; i < n; i++)
          namespaceMembers[i] = this.SubstituteViaDispatcher(namespaceMembers[i]);
      }

      private void SubstituteElements(List<INestedTypeDefinition>/*?*/ nestedTypes) {
        if (nestedTypes == null) return;
        for (int i = 0, n = nestedTypes.Count; i < n; i++)
          nestedTypes[i] = this.Substitute(nestedTypes[i]);
      }

      private void SubstituteElements(List<IOperation>/*?*/ operations) {
        if (operations == null) return;
        for (int i = 0, n = operations.Count; i < n; i++)
          operations[i] = this.Substitute(operations[i]);
      }

      private void SubstituteElements(List<IOperationExceptionInformation>/*?*/ operationExceptionInformations) {
        if (operationExceptionInformations == null) return;
        for (int i = 0, n = operationExceptionInformations.Count; i < n; i++)
          operationExceptionInformations[i] = this.Substitute(operationExceptionInformations[i]);
      }

      private void SubstituteElements(List<IParameterDefinition>/*?*/ parameters) {
        if (parameters == null) return;
        for (int i = 0, n = parameters.Count; i < n; i++)
          parameters[i] = this.Substitute(parameters[i]);
      }

      private void SubstituteElements(List<IParameterTypeInformation>/*?*/ parameterTypeInformations) {
        if (parameterTypeInformations == null) return;
        for (int i = 0, n = parameterTypeInformations.Count; i < n; i++)
          parameterTypeInformations[i] = this.Substitute(parameterTypeInformations[i]);
      }

      private void SubstituteElements(List<IPropertyDefinition>/*?*/ properties) {
        if (properties == null) return;
        for (int i = 0, n = properties.Count; i < n; i++)
          properties[i] = this.Substitute(properties[i]);
      }

      private void SubstituteElements(List<IResourceReference>/*?*/ resourceReferences) {
        if (resourceReferences == null) return;
        for (int i = 0, n = resourceReferences.Count; i < n; i++)
          resourceReferences[i] = this.Substitute(resourceReferences[i]);
      }

      private void SubstituteElements(List<ISecurityAttribute>/*?*/ securityAttributes) {
        if (securityAttributes == null) return;
        for (int i = 0, n = securityAttributes.Count; i < n; i++)
          securityAttributes[i] = this.Substitute(securityAttributes[i]);
      }

      private void SubstituteElements(List<ITypeDefinitionMember>/*?*/ typeMembers) {
        if (typeMembers == null) return;
        for (int i = 0, n = typeMembers.Count; i < n; i++)
          typeMembers[i] = this.SubstituteViaDispatcher(typeMembers[i]);
      }

      private void SubstituteElements(List<ITypeReference>/*?*/ typeReferences) {
        if (typeReferences == null) return;
        for (int i = 0, n = typeReferences.Count; i < n; i++)
          typeReferences[i] = this.SubstituteViaDispatcher(typeReferences[i]);
      }

      private void SubstituteElements(List<IWin32Resource>/*?*/ win32Resources) {
        if (win32Resources == null) return;
        for (int i = 0, n = win32Resources.Count; i < n; i++)
          win32Resources[i] = this.Substitute(win32Resources[i]);
      }

      internal IEventDefinition SubstituteReference(IEventDefinition eventDefinition) {
        if (eventDefinition is Dummy) return eventDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(eventDefinition, out copy)) return (IEventDefinition)copy;
        //If we get here, the event is outside of the cone and we just use it as is since events do not have independent reference objects. 
        return eventDefinition;
      }

      internal IFieldDefinition SubstituteReference(IFieldDefinition fieldDefinition) {
        if (fieldDefinition is Dummy) return fieldDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(fieldDefinition, out copy)) return (IFieldDefinition)copy;
        //If we get here, the field is outside of the cone and we just use it as is since we need an object of type IFieldDefinition, not IFieldReference. 
        return fieldDefinition;
      }

      private ILocalDefinition SubstituteReference(ILocalDefinition localDefinition) {
        if (localDefinition is Dummy) return localDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(localDefinition, out copy)) return (ILocalDefinition)copy;
        //If we get here, the local is outside of the cone and we just use it as is since locals do not have independent reference objects. 
        return localDefinition;
      }

      internal IMethodDefinition SubstituteReference(IMethodDefinition methodDefinition) {
        if (methodDefinition is Dummy) return methodDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(methodDefinition, out copy)) return (IMethodDefinition)copy;
        //If we get here, the method is outside of the cone and we just use it as is since we need an object of type IMethodDefinition, not IMethodReference. 
        return methodDefinition;
      }

      private INamespaceAliasForType SubstituteReference(INamespaceAliasForType namespaceAliasForType) {
        if (namespaceAliasForType is Dummy) return namespaceAliasForType;
        object copy;
        if (this.DefinitionCache.TryGetValue(namespaceAliasForType, out copy)) return (NamespaceAliasForType)copy;
        //If we get here, the alias is outside of the cone and we just use it as is since aliases do not have independent reference objects. 
        return namespaceAliasForType;
      }

      private INestedAliasForType SubstituteReference(INestedAliasForType nestedAliasForType) {
        if (nestedAliasForType is Dummy) return nestedAliasForType;
        object copy;
        if (this.DefinitionCache.TryGetValue(nestedAliasForType, out copy)) return (NestedAliasForType)copy;
        //If we get here, the alias is outside of the cone and we just use it as is since aliases do not have independent reference objects. 
        return nestedAliasForType;
      }

      internal INestedTypeDefinition SubstituteReference(INestedTypeDefinition nestedTypeDefinition) {
        if (nestedTypeDefinition is Dummy) return nestedTypeDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(nestedTypeDefinition, out copy)) return (INestedTypeDefinition)copy;
        //If we get here, the nested type is outside of the cone and we just use it as is since we need an object of type INestedTypeDefinition, not INestedTypeReference. 
        return nestedTypeDefinition;
      }

      internal IParameterDefinition SubstituteReference(IParameterDefinition parameterDefinition) {
        if (parameterDefinition is Dummy) return parameterDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(parameterDefinition, out copy)) return (IParameterDefinition)copy;
        //If we get here, the parameter is outside of the cone and we just use it as is since parameters do not have independent reference objects. 
        return parameterDefinition;
      }

      internal IPropertyDefinition SubstituteReference(IPropertyDefinition propertyDefinition) {
        if (propertyDefinition is Dummy) return propertyDefinition;
        object copy;
        if (this.DefinitionCache.TryGetValue(propertyDefinition, out copy)) return (IPropertyDefinition)copy;
        //If we get here, the property is outside of the cone and we just use it as is since properties do not have independent reference objects. 
        return propertyDefinition;
      }

      private IAliasForType SubstituteViaDispatcher(IAliasForType aliasForType) {
        if (aliasForType is Dummy) return aliasForType;
        aliasForType.Dispatch(this.Dispatcher);
        return (IAliasForType)this.Dispatcher.result;
      }

      private IFieldReference SubstituteViaDispatcher(IFieldReference fieldReference) {
        if (fieldReference is Dummy) return fieldReference;
        fieldReference.DispatchAsReference(this.Dispatcher);
        return (IFieldReference)this.Dispatcher.result;
      }

      private IMetadataExpression SubstituteViaDispatcher(IMetadataExpression metadataExpression) {
        if (metadataExpression is Dummy) return metadataExpression;
        metadataExpression.Dispatch(this.Dispatcher);
        return (IMetadataExpression)this.Dispatcher.result;
      }

      private IMethodReference SubstituteViaDispatcher(IMethodReference methodReference) {
        if (methodReference is Dummy) return methodReference;
        methodReference.DispatchAsReference(this.Dispatcher);
        return (IMethodReference)this.Dispatcher.result;
      }

      private INamedTypeDefinition SubstituteViaDispatcher(INamedTypeDefinition typeDefinition) {
        if (typeDefinition is Dummy) return typeDefinition;
        typeDefinition.Dispatch(this.Dispatcher);
        return (INamedTypeDefinition)this.Dispatcher.result;
      }

      private INamedTypeReference SubstituteViaDispatcher(INamedTypeReference namedTypeReference) {
        if (namedTypeReference is Dummy) return namedTypeReference;
        namedTypeReference.DispatchAsReference(this.Dispatcher);
        return (INamedTypeReference)this.Dispatcher.result;
      }

      private INamespaceMember SubstituteViaDispatcher(INamespaceMember namespaceMember) {
        if (namespaceMember is Dummy) return namespaceMember;
        namespaceMember.Dispatch(this.Dispatcher);
        return (INamespaceMember)this.Dispatcher.result;
      }

      private ITypeDefinitionMember SubstituteViaDispatcher(ITypeDefinitionMember typeDefinitionMember) {
        if (typeDefinitionMember is Dummy) return typeDefinitionMember;
        typeDefinitionMember.Dispatch(this.Dispatcher);
        return (ITypeDefinitionMember)this.Dispatcher.result;
      }

      private ITypeReference SubstituteViaDispatcher(ITypeReference typeReference) {
        if (typeReference is Dummy) return typeReference;
        typeReference.DispatchAsReference(this.Dispatcher);
        return (ITypeReference)this.Dispatcher.result;
      }

      private IUnitNamespaceReference SubstituteViaDispatcher(IUnitNamespaceReference unitNamespaceReference) {
        if (unitNamespaceReference is Dummy) return unitNamespaceReference;
        unitNamespaceReference.DispatchAsReference(this.Dispatcher);
        return (IUnitNamespaceReference)this.Dispatcher.result;
      }

      private IUnitReference SubstituteViaDispatcher(IUnitReference unitReference) {
        if (unitReference is Dummy) return unitReference;
        unitReference.DispatchAsReference(this.Dispatcher);
        return (IUnitReference)this.Dispatcher.result;
      }

    }

    /// <summary>
    /// Returns a deep copy of the specified alias for type.
    /// </summary>
    public AliasForType Copy(IAliasForType aliasForType) {
      Contract.Requires(aliasForType != null);
      Contract.Requires(!(aliasForType is Dummy));
      Contract.Ensures(Contract.Result<AliasForType>() != null);

      aliasForType.Dispatch(this.Dispatcher);
      return (AliasForType)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a shallow copy of the given array type reference.
    /// </summary>
    public ArrayTypeReference Copy(IArrayTypeReference arrayTypeReference) {
      Contract.Requires(arrayTypeReference != null);
      Contract.Requires(!(arrayTypeReference is Dummy));
      Contract.Ensures(Contract.Result<ArrayTypeReference>() != null);

      return (ArrayTypeReference)this.SubstituteCopiesForOriginals.Substitute(arrayTypeReference);
    }

    /// <summary>
    /// Returns a deep copy of the given assembly.
    /// </summary>
    public Assembly Copy(IAssembly assembly) {
      Contract.Requires(assembly != null);
      Contract.Requires(!(assembly is Dummy));
      Contract.Ensures(Contract.Result<Assembly>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(assembly); //Does not traverse the types
      foreach (var type in assembly.GetAllTypes())
        this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(type);
      foreach (var module in assembly.MemberModules) {
        foreach (var mtype in module.GetAllTypes())
          this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(mtype);
      }
      return (Assembly)this.SubstituteCopiesForOriginals.Substitute(assembly);
    }

    /// <summary>
    /// Returns a deep copy of the given assembly reference.
    /// </summary>
    public AssemblyReference Copy(IAssemblyReference assemblyReference) {
      Contract.Requires(assemblyReference != null);
      Contract.Requires(!(assemblyReference is Dummy));
      Contract.Ensures(Contract.Result<AssemblyReference>() != null);

      return (AssemblyReference)this.SubstituteCopiesForOriginals.Substitute(assemblyReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given custom attribute.
    /// </summary>
    public CustomAttribute Copy(ICustomAttribute customAttribute) {
      Contract.Requires(customAttribute != null);
      Contract.Requires(!(customAttribute is Dummy));
      Contract.Ensures(Contract.Result<CustomAttribute>() != null);

      return (CustomAttribute)this.SubstituteCopiesForOriginals.Substitute(customAttribute);
    }

    /// <summary>
    /// Returns a deep copy of the given custom modifier.
    /// </summary>
    public CustomModifier Copy(ICustomModifier customModifier) {
      Contract.Requires(customModifier != null);
      Contract.Requires(!(customModifier is Dummy));
      Contract.Ensures(Contract.Result<CustomModifier>() != null);

      return (CustomModifier)this.SubstituteCopiesForOriginals.Substitute(customModifier);
    }

    /// <summary>
    /// Returns a deep copy of the given event definition.
    /// </summary>
    public EventDefinition Copy(IEventDefinition eventDefinition) {
      Contract.Requires(eventDefinition != null);
      Contract.Requires(!(eventDefinition is Dummy));
      Contract.Ensures(Contract.Result<EventDefinition>() != null);

      var specializedEventDefinition = eventDefinition as ISpecializedEventDefinition;
      if (specializedEventDefinition != null) return this.Copy(specializedEventDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(eventDefinition);
      eventDefinition.Dispatch(this.Dispatcher);
      return (EventDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given event definition.
    /// </summary>
    private EventDefinition CopyUnspecialized(IEventDefinition eventDefinition) {
      Contract.Requires(eventDefinition != null);
      Contract.Requires(!(eventDefinition is Dummy || eventDefinition is ISpecializedEventDefinition));
      Contract.Ensures(Contract.Result<EventDefinition>() != null);

      return (EventDefinition)this.SubstituteCopiesForOriginals.Substitute(eventDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given method body.
    /// </summary>
    protected virtual IMethodBody CopyMethodBody(IMethodBody methodBody, IMethodDefinition method) {
      Contract.Requires(methodBody != null);
      Contract.Requires(method != null);
      Contract.Requires(!(methodBody is Dummy));
      Contract.Ensures(Contract.Result<IMethodBody>() != null);

      return this.SubstituteCopiesForOriginals.Substitute(this.SubstituteCopiesForOriginals.shallowCopier.Copy(methodBody));
    }

    /// <summary>
    /// Returns a deep copy of the given field definition.
    /// </summary>
    public FieldDefinition Copy(IFieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);
      Contract.Requires(!(fieldDefinition is Dummy));
      Contract.Ensures(Contract.Result<FieldDefinition>() != null);

      var specializedFieldDefinition = fieldDefinition as ISpecializedFieldDefinition;
      if (specializedFieldDefinition != null) return this.Copy(specializedFieldDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(fieldDefinition);
      fieldDefinition.Dispatch(this.Dispatcher);
      return (FieldDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given field definition.
    /// </summary>
    private FieldDefinition CopyUnspecialized(IFieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);
      Contract.Requires(!(fieldDefinition is Dummy || fieldDefinition is ISpecializedFieldDefinition));
      Contract.Ensures(Contract.Result<FieldDefinition>() != null);

      return (FieldDefinition)this.SubstituteCopiesForOriginals.Substitute(fieldDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given field reference.
    /// </summary>
    public FieldReference Copy(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Requires(!(fieldReference is Dummy));
      Contract.Ensures(Contract.Result<FieldReference>() != null);

      fieldReference.DispatchAsReference(this.Dispatcher);
      return (FieldReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given field reference.
    /// </summary>
    private FieldReference CopyUnspecialized(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Ensures(Contract.Result<FieldReference>() != null);

      return (FieldReference)this.SubstituteCopiesForOriginals.Substitute(fieldReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given file reference.
    /// </summary>
    public FileReference Copy(IFileReference fileReference) {
      Contract.Requires(fileReference != null);
      Contract.Requires(!(fileReference is Dummy));
      Contract.Ensures(Contract.Result<FileReference>() != null);

      return (FileReference)this.SubstituteCopiesForOriginals.Substitute(fileReference);
    }

    /// <summary>
    /// Returns a deep copy of the given function pointer type reference.
    /// </summary>
    public FunctionPointerTypeReference Copy(IFunctionPointerTypeReference functionPointerTypeReference) {
      Contract.Requires(functionPointerTypeReference != null);
      Contract.Requires(!(functionPointerTypeReference is Dummy));
      Contract.Ensures(Contract.Result<FunctionPointerTypeReference>() != null);

      return (FunctionPointerTypeReference)this.SubstituteCopiesForOriginals.Substitute(functionPointerTypeReference);
    }

    /// <summary>
    /// Returns a deep copy of the given generic method instance reference.
    /// </summary>
    public GenericMethodInstanceReference Copy(IGenericMethodInstanceReference genericMethodInstanceReference) {
      Contract.Requires(!(genericMethodInstanceReference is Dummy));
      return (GenericMethodInstanceReference)this.SubstituteCopiesForOriginals.Substitute(genericMethodInstanceReference);
    }

    /// <summary>
    /// Returns a deep copy of the given generic method parameter reference.
    /// </summary>
    public GenericMethodParameter Copy(IGenericMethodParameter genericMethodParameter) {
      Contract.Requires(genericMethodParameter != null);
      Contract.Requires(!(genericMethodParameter is Dummy));
      Contract.Ensures(Contract.Result<GenericMethodParameter>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(genericMethodParameter);
      return (GenericMethodParameter)this.SubstituteCopiesForOriginals.Substitute(genericMethodParameter);
    }

    /// <summary>
    /// Returns a deep copy of the given generic method parameter reference.
    /// </summary>
    public GenericMethodParameterReference Copy(IGenericMethodParameterReference genericMethodParameterReference) {
      Contract.Requires(genericMethodParameterReference != null);
      Contract.Requires(!(genericMethodParameterReference is Dummy));
      Contract.Ensures(Contract.Result<GenericMethodParameterReference>() != null);

      return (GenericMethodParameterReference)this.SubstituteCopiesForOriginals.Substitute(genericMethodParameterReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given generic type parameter.
    /// </summary>
    public GenericTypeParameter Copy(IGenericTypeParameter genericTypeParameter) {
      Contract.Requires(genericTypeParameter != null);
      Contract.Requires(!(genericTypeParameter is Dummy));
      Contract.Ensures(Contract.Result<GenericTypeParameter>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(genericTypeParameter);
      return (GenericTypeParameter)this.SubstituteCopiesForOriginals.Substitute(genericTypeParameter);
    }

    /// <summary>
    /// Returns a deep copy of the given generic type instance reference.
    /// </summary>
    public GenericTypeInstanceReference Copy(IGenericTypeInstanceReference genericTypeInstanceReference) {
      Contract.Requires(!(genericTypeInstanceReference is Dummy));
      return (GenericTypeInstanceReference)this.SubstituteCopiesForOriginals.Substitute(genericTypeInstanceReference);
    }

    /// <summary>
    /// Returns a deep copy of the given generic type parameter reference.
    /// </summary>
    public GenericTypeParameterReference Copy(IGenericTypeParameterReference genericTypeParameterReference) {
      Contract.Requires(genericTypeParameterReference != null);
      Contract.Requires(!(genericTypeParameterReference is Dummy));
      Contract.Ensures(Contract.Result<GenericTypeParameterReference>() != null);

      return (GenericTypeParameterReference)this.SubstituteCopiesForOriginals.Substitute(genericTypeParameterReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given global field definition.
    /// </summary>
    public GlobalFieldDefinition Copy(IGlobalFieldDefinition globalFieldDefinition) {
      Contract.Requires(globalFieldDefinition != null);
      Contract.Requires(!(globalFieldDefinition is Dummy));
      Contract.Ensures(Contract.Result<GlobalFieldDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(globalFieldDefinition);
      return (GlobalFieldDefinition)this.SubstituteCopiesForOriginals.Substitute(globalFieldDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given global method definition.
    /// </summary>
    public GlobalMethodDefinition Copy(IGlobalMethodDefinition globalMethodDefinition) {
      Contract.Requires(globalMethodDefinition != null);
      Contract.Requires(!(globalMethodDefinition is Dummy));
      Contract.Ensures(Contract.Result<GlobalMethodDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(globalMethodDefinition);
      return (GlobalMethodDefinition)this.SubstituteCopiesForOriginals.Substitute(globalMethodDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the specified local definition.
    /// </summary>
    public LocalDefinition Copy(ILocalDefinition localDefinition) {
      Contract.Requires(localDefinition != null);
      Contract.Requires(!(localDefinition is Dummy));
      Contract.Ensures(Contract.Result<LocalDefinition>() != null);

      return (LocalDefinition)this.SubstituteCopiesForOriginals.Substitute(localDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given managed pointer type reference.
    /// </summary>
    public ManagedPointerTypeReference Copy(IManagedPointerTypeReference managedPointerTypeReference) {
      Contract.Requires(managedPointerTypeReference != null);
      Contract.Requires(!(managedPointerTypeReference is Dummy));
      Contract.Ensures(Contract.Result<ManagedPointerTypeReference>() != null);

      return (ManagedPointerTypeReference)this.SubstituteCopiesForOriginals.Substitute(managedPointerTypeReference);
    }

    /// <summary>
    /// Returns a deep copy of the given marshalling information.
    /// </summary>
    public MarshallingInformation Copy(IMarshallingInformation marshallingInformation) {
      Contract.Requires(marshallingInformation != null);
      Contract.Requires(!(marshallingInformation is Dummy));
      Contract.Ensures(Contract.Result<MarshallingInformation>() != null);

      return (MarshallingInformation)this.SubstituteCopiesForOriginals.Substitute(marshallingInformation);
    }

    /// <summary>
    /// Returns a deep copy of the given metadata constant.
    /// </summary>
    public MetadataConstant Copy(IMetadataConstant constant) {
      Contract.Requires(constant != null);
      Contract.Requires(!(constant is Dummy));
      Contract.Ensures(Contract.Result<MetadataConstant>() != null);

      return (MetadataConstant)this.SubstituteCopiesForOriginals.Substitute(constant);
    }

    /// <summary>
    /// Returns a deep copy of the given metadata array creation expression.
    /// </summary>
    public MetadataCreateArray Copy(IMetadataCreateArray createArray) {
      Contract.Requires(createArray != null);
      Contract.Requires(!(createArray is Dummy));
      Contract.Ensures(Contract.Result<MetadataCreateArray>() != null);

      return (MetadataCreateArray)this.SubstituteCopiesForOriginals.Substitute(createArray);
    }

    /// <summary>
    /// Returns a deep copy of the given metadata expression.
    /// </summary>
    public MetadataExpression Copy(IMetadataExpression expression) {
      Contract.Requires(expression != null);
      Contract.Requires(!(expression is Dummy));
      Contract.Ensures(Contract.Result<MetadataExpression>() != null);

      expression.Dispatch(this.Dispatcher);
      return (MetadataExpression)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given metadata named argument expression.
    /// </summary>
    public MetadataNamedArgument Copy(IMetadataNamedArgument namedArgument) {
      Contract.Requires(namedArgument != null);
      Contract.Requires(!(namedArgument is Dummy));
      Contract.Ensures(Contract.Result<MetadataNamedArgument>() != null);

      return (MetadataNamedArgument)this.SubstituteCopiesForOriginals.Substitute(namedArgument);
    }

    /// <summary>
    /// Returns a deep copy of the given metadata typeof expression.
    /// </summary>
    public MetadataTypeOf Copy(IMetadataTypeOf typeOf) {
      Contract.Requires(typeOf != null);
      Contract.Requires(!(typeOf is Dummy));
      Contract.Ensures(Contract.Result<MetadataTypeOf>() != null);

      return (MetadataTypeOf)this.SubstituteCopiesForOriginals.Substitute(typeOf);
    }

    /// <summary>
    /// Returns a deep copy of the given method body.
    /// </summary>
    public MethodBody Copy(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      Contract.Requires(!(methodBody is Dummy));
      Contract.Ensures(Contract.Result<MethodBody>() != null);

      return (MethodBody)this.SubstituteCopiesForOriginals.Substitute(methodBody, methodBody.MethodDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given method definition.
    /// </summary>
    public MethodDefinition Copy(IMethodDefinition method) {
      Contract.Requires(method != null);
      Contract.Requires(!(method is Dummy));
      Contract.Ensures(Contract.Result<MethodDefinition>() != null);

      var specializedMethodDefinition = method as ISpecializedMethodDefinition;
      if (specializedMethodDefinition != null) return this.Copy(specializedMethodDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(method);
      method.Dispatch(this.Dispatcher);
      return (MethodDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given method definition.
    /// </summary>
    private MethodDefinition CopyUnspecialized(IMethodDefinition method) {
      Contract.Requires(method != null);
      Contract.Ensures(Contract.Result<MethodDefinition>() != null);

      return (MethodDefinition)this.SubstituteCopiesForOriginals.Substitute(method);
    }

    /// <summary>
    /// Returns a deep copy of the given method implementation.
    /// </summary>
    public MethodImplementation Copy(IMethodImplementation methodImplementation) {
      Contract.Requires(methodImplementation != null);
      Contract.Requires(!(methodImplementation is Dummy));
      Contract.Ensures(Contract.Result<MethodImplementation>() != null);

      return (MethodImplementation)this.SubstituteCopiesForOriginals.Substitute(methodImplementation);
    }

    /// <summary>
    /// Returns a deep copy of the given method reference.
    /// </summary>
    public MethodReference Copy(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Requires(!(methodReference is Dummy));
      Contract.Ensures(Contract.Result<MethodReference>() != null);

      methodReference.DispatchAsReference(this.Dispatcher);
      return (MethodReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given method reference.
    /// </summary>
    private MethodReference CopyUnspecialized(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Ensures(Contract.Result<MethodReference>() != null);

      return (MethodReference)this.SubstituteCopiesForOriginals.Substitute(methodReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given modified type reference.
    /// </summary>
    public ModifiedTypeReference Copy(IModifiedTypeReference modifiedTypeReference) {
      Contract.Requires(modifiedTypeReference != null);
      Contract.Requires(!(modifiedTypeReference is Dummy));
      Contract.Ensures(Contract.Result<ModifiedTypeReference>() != null);

      return (ModifiedTypeReference)this.SubstituteCopiesForOriginals.Substitute(modifiedTypeReference);
    }

    /// <summary>
    /// Returns a deep copy of the given module.
    /// </summary>
    public Module Copy(IModule module) {
      Contract.Requires(module != null);
      Contract.Requires(!(module is Dummy));
      Contract.Ensures(Contract.Result<Module>() != null);

      var assembly = module as IAssembly;
      if (assembly != null) return this.Copy(assembly);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(module);
      foreach (var type in module.GetAllTypes())
        this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(type);
      return (Module)this.SubstituteCopiesForOriginals.Substitute(module);
    }

    /// <summary>
    /// Returns a deep copy of the given module reference.
    /// </summary>
    public ModuleReference Copy(IModuleReference moduleReference) {
      Contract.Requires(moduleReference != null);
      Contract.Requires(!(moduleReference is Dummy));
      Contract.Ensures(Contract.Result<ModuleReference>() != null);

      var assemblyReference = moduleReference as IAssemblyReference;
      if (assemblyReference != null) return this.Copy(assemblyReference);
      return (ModuleReference)this.SubstituteCopiesForOriginals.Substitute(moduleReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the specified named type definition.
    /// </summary>
    public NamedTypeDefinition Copy(INamedTypeDefinition namedTypeDefinition) {
      Contract.Requires(namedTypeDefinition != null);
      Contract.Requires(!(namedTypeDefinition is Dummy));
      Contract.Ensures(Contract.Result<NamedTypeDefinition>() != null);

      namedTypeDefinition.Dispatch(this.Dispatcher);
      return (NamedTypeDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given alias for a namespace type definition.
    /// </summary>
    public NamespaceAliasForType Copy(INamespaceAliasForType namespaceAliasForType) {
      Contract.Requires(namespaceAliasForType != null);
      Contract.Requires(!(namespaceAliasForType is Dummy));
      Contract.Ensures(Contract.Result<NamespaceAliasForType>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(namespaceAliasForType);
      return (NamespaceAliasForType)this.SubstituteCopiesForOriginals.Substitute(namespaceAliasForType);
    }

    /// <summary>
    /// Returns a deep copy of the given namespace type definition.
    /// </summary>
    public NamespaceTypeDefinition Copy(INamespaceTypeDefinition namespaceTypeDefinition) {
      Contract.Requires(namespaceTypeDefinition != null);
      Contract.Requires(!(namespaceTypeDefinition is Dummy));
      Contract.Ensures(Contract.Result<NamespaceTypeDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(namespaceTypeDefinition);
      return (NamespaceTypeDefinition)this.SubstituteCopiesForOriginals.Substitute(namespaceTypeDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given namespace type reference.
    /// </summary>
    public NamespaceTypeReference Copy(INamespaceTypeReference namespaceTypeReference) {
      Contract.Requires(namespaceTypeReference != null);
      Contract.Requires(!(namespaceTypeReference is Dummy));
      Contract.Ensures(Contract.Result<NamespaceTypeReference>() != null);

      return (NamespaceTypeReference)this.SubstituteCopiesForOriginals.Substitute(namespaceTypeReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given nested type alias.
    /// </summary>
    public NestedAliasForType Copy(INestedAliasForType nestedAliasForType) {
      Contract.Requires(nestedAliasForType != null);
      Contract.Requires(!(nestedAliasForType is Dummy));
      Contract.Ensures(Contract.Result<NestedAliasForType>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(nestedAliasForType);
      return (NestedAliasForType)this.SubstituteCopiesForOriginals.Substitute(nestedAliasForType);
    }

    /// <summary>
    /// Returns a deep copy of the given nested type definition.
    /// </summary>
    public NestedTypeDefinition Copy(INestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);
      Contract.Requires(!(nestedTypeDefinition is Dummy));
      Contract.Ensures(Contract.Result<NestedTypeDefinition>() != null);

      var specializedNestedTypeDefinition = nestedTypeDefinition as ISpecializedNestedTypeDefinition;
      if (specializedNestedTypeDefinition != null) return this.Copy(specializedNestedTypeDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(nestedTypeDefinition);
      nestedTypeDefinition.Dispatch(this.Dispatcher);
      return (NestedTypeDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given nested type definition.
    /// </summary>
    private NestedTypeDefinition CopyUnspecialized(INestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);
      Contract.Requires(!(nestedTypeDefinition is Dummy || nestedTypeDefinition is ISpecializedNestedTypeDefinition));
      Contract.Ensures(Contract.Result<NestedTypeDefinition>() != null);

      return (NestedTypeDefinition)this.SubstituteCopiesForOriginals.Substitute(nestedTypeDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given nested type reference.
    /// </summary>
    public NestedTypeReference Copy(INestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Requires(!(nestedTypeReference is Dummy));
      Contract.Ensures(Contract.Result<NestedTypeReference>() != null);

      nestedTypeReference.DispatchAsReference(this.Dispatcher);
      return (NestedTypeReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given nested type reference.
    /// </summary>
    private NestedTypeReference CopyUnspecialized(INestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Ensures(Contract.Result<NestedTypeReference>() != null);

      return (NestedTypeReference)this.SubstituteCopiesForOriginals.Substitute(nestedTypeReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given nested unit namespace.
    /// </summary>
    public NestedUnitNamespace Copy(INestedUnitNamespace nestedUnitNamespace) {
      Contract.Requires(nestedUnitNamespace != null);
      Contract.Requires(!(nestedUnitNamespace is Dummy));
      Contract.Ensures(Contract.Result<NestedUnitNamespace>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(nestedUnitNamespace);
      return (NestedUnitNamespace)this.SubstituteCopiesForOriginals.Substitute(nestedUnitNamespace);
    }

    /// <summary>
    /// Returns a deep copy of the given nested unit namespace reference.
    /// </summary>
    public NestedUnitNamespaceReference Copy(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      Contract.Requires(nestedUnitNamespaceReference != null);
      Contract.Requires(!(nestedUnitNamespaceReference is Dummy));
      Contract.Ensures(Contract.Result<NestedUnitNamespaceReference>() != null);

      return (NestedUnitNamespaceReference)this.SubstituteCopiesForOriginals.Substitute(nestedUnitNamespaceReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the specified operation.
    /// </summary>
    public Operation Copy(IOperation operation) {
      Contract.Requires(operation != null);
      Contract.Requires(!(operation is Dummy));
      Contract.Ensures(Contract.Result<Operation>() != null);

      return (Operation)this.SubstituteCopiesForOriginals.Substitute(operation);
    }

    /// <summary>
    /// Returns a deep copy of the specified operation exception information.
    /// </summary>
    public OperationExceptionInformation Copy(IOperationExceptionInformation operationExceptionInformation) {
      Contract.Requires(operationExceptionInformation != null);
      Contract.Requires(!(operationExceptionInformation is Dummy));
      Contract.Ensures(Contract.Result<OperationExceptionInformation>() != null);

      return (OperationExceptionInformation)this.SubstituteCopiesForOriginals.Substitute(operationExceptionInformation);
    }

    /// <summary>
    /// Returns a deep copy of the given parameter definition.
    /// </summary>
    public ParameterDefinition Copy(IParameterDefinition parameterDefinition) {
      Contract.Requires(parameterDefinition != null);
      Contract.Requires(!(parameterDefinition is Dummy));
      Contract.Ensures(Contract.Result<ParameterDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(parameterDefinition);
      return (ParameterDefinition)this.SubstituteCopiesForOriginals.Substitute(parameterDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given parameter type information.
    /// </summary>
    public ParameterTypeInformation Copy(IParameterTypeInformation parameterTypeInformation) {
      Contract.Requires(parameterTypeInformation != null);
      Contract.Requires(!(parameterTypeInformation is Dummy));
      Contract.Ensures(Contract.Result<ParameterTypeInformation>() != null);

      return (ParameterTypeInformation)this.SubstituteCopiesForOriginals.Substitute(parameterTypeInformation, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given PE section.
    /// </summary>
    public PESection Copy(IPESection peSection) {
      Contract.Requires(peSection != null);
      Contract.Requires(!(peSection is Dummy));
      Contract.Ensures(Contract.Result<PESection>() != null);

      return (PESection)this.SubstituteCopiesForOriginals.Substitute(peSection);
    }

    /// <summary>
    /// Returns a deep copy of the specified platform invoke information.
    /// </summary>
    public PlatformInvokeInformation Copy(IPlatformInvokeInformation platformInvokeInformation) {
      Contract.Requires(platformInvokeInformation != null);
      Contract.Requires(!(platformInvokeInformation is Dummy));
      Contract.Ensures(Contract.Result<PlatformInvokeInformation>() != null);

      return (PlatformInvokeInformation)this.SubstituteCopiesForOriginals.Substitute(platformInvokeInformation);
    }

    /// <summary>
    /// Returns a deep copy of the given pointer type reference.
    /// </summary>
    public PointerTypeReference Copy(IPointerTypeReference pointerTypeReference) {
      Contract.Requires(pointerTypeReference != null);
      Contract.Requires(!(pointerTypeReference is Dummy));
      Contract.Ensures(Contract.Result<PointerTypeReference>() != null);

      return (PointerTypeReference)this.SubstituteCopiesForOriginals.Substitute(pointerTypeReference);
    }

    /// <summary>
    /// Returns a deep copy of the given property definition.
    /// </summary>
    public PropertyDefinition Copy(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Requires(!(propertyDefinition is Dummy));
      Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

      var specializedPropertyDefinition = propertyDefinition as ISpecializedPropertyDefinition;
      if (specializedPropertyDefinition != null) return this.Copy(specializedPropertyDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(propertyDefinition);
      propertyDefinition.Dispatch(this.Dispatcher);
      return (PropertyDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given property definition.
    /// </summary>
    private PropertyDefinition CopyUnspecialized(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Requires(!(propertyDefinition is Dummy || propertyDefinition is ISpecializedPropertyDefinition));
      Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

      return (PropertyDefinition)this.SubstituteCopiesForOriginals.Substitute(propertyDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given reference to a manifest resource.
    /// </summary>
    public ResourceReference Copy(IResourceReference resourceReference) {
      Contract.Requires(resourceReference != null);
      Contract.Requires(!(resourceReference is Dummy));
      Contract.Ensures(Contract.Result<ResourceReference>() != null);

      return (ResourceReference)this.SubstituteCopiesForOriginals.Substitute(resourceReference);
    }

    /// <summary>
    /// Returns a deep copy of the given root unit namespace.
    /// </summary>
    public RootUnitNamespace Copy(IRootUnitNamespace rootUnitNamespace) {
      Contract.Requires(rootUnitNamespace != null);
      Contract.Requires(!(rootUnitNamespace is Dummy));
      Contract.Ensures(Contract.Result<RootUnitNamespace>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(rootUnitNamespace);
      return (RootUnitNamespace)this.SubstituteCopiesForOriginals.Substitute(rootUnitNamespace);
    }

    /// <summary>
    /// Returns a deep copy of the given root unit namespace.
    /// </summary>
    public RootUnitNamespaceReference Copy(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      Contract.Requires(rootUnitNamespaceReference != null);
      Contract.Requires(!(rootUnitNamespaceReference is Dummy));
      Contract.Ensures(Contract.Result<RootUnitNamespaceReference>() != null);

      return (RootUnitNamespaceReference)this.SubstituteCopiesForOriginals.Substitute(rootUnitNamespaceReference, keepAsDefinition: false);
    }

    /// <summary>
    /// Returns a deep copy of the given security attribute.
    /// </summary>
    public SecurityAttribute Copy(ISecurityAttribute securityAttribute) {
      Contract.Requires(securityAttribute != null);
      Contract.Requires(!(securityAttribute is Dummy));
      Contract.Ensures(Contract.Result<SecurityAttribute>() != null);

      return (SecurityAttribute)this.SubstituteCopiesForOriginals.Substitute(securityAttribute);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized event definition.
    /// </summary>
    public SpecializedEventDefinition Copy(ISpecializedEventDefinition specializedEventDefinition) {
      Contract.Requires(specializedEventDefinition != null);
      Contract.Requires(!(specializedEventDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedEventDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse((IEventDefinition)specializedEventDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(specializedEventDefinition.UnspecializedVersion);
      return (SpecializedEventDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedEventDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized event definition.
    /// </summary>
    private SpecializedEventDefinition CopySpecialized(ISpecializedEventDefinition specializedEventDefinition) {
      Contract.Requires(specializedEventDefinition != null);
      Contract.Requires(!(specializedEventDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedEventDefinition>() != null);

      return (SpecializedEventDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedEventDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized field definition.
    /// </summary>
    public SpecializedFieldDefinition Copy(ISpecializedFieldDefinition specializedFieldDefinition) {
      Contract.Requires(specializedFieldDefinition != null);
      Contract.Requires(!(specializedFieldDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedFieldDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse((IFieldDefinition)specializedFieldDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(specializedFieldDefinition.UnspecializedVersion);
      return (SpecializedFieldDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedFieldDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized field definition.
    /// </summary>
    private SpecializedFieldDefinition CopySpecialized(ISpecializedFieldDefinition specializedFieldDefinition) {
      Contract.Requires(specializedFieldDefinition != null);
      Contract.Requires(!(specializedFieldDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedFieldDefinition>() != null);

      return (SpecializedFieldDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedFieldDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized field reference.
    /// </summary>
    public SpecializedFieldReference Copy(ISpecializedFieldReference specializedFieldReference) {
      Contract.Requires(specializedFieldReference != null);
      Contract.Requires(!(specializedFieldReference is Dummy));
      Contract.Ensures(Contract.Result<SpecializedFieldReference>() != null);

      return (SpecializedFieldReference)this.SubstituteCopiesForOriginals.Substitute(specializedFieldReference);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized method definition.
    /// </summary>
    public SpecializedMethodDefinition Copy(ISpecializedMethodDefinition specializedMethodDefinition) {
      Contract.Requires(specializedMethodDefinition != null);
      Contract.Requires(!(specializedMethodDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedMethodDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse((IMethodDefinition)specializedMethodDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(specializedMethodDefinition.UnspecializedVersion);
      return (SpecializedMethodDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedMethodDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized method definition.
    /// </summary>
    private SpecializedMethodDefinition CopySpecialized(ISpecializedMethodDefinition specializedMethodDefinition) {
      Contract.Requires(specializedMethodDefinition != null);
      Contract.Requires(!(specializedMethodDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedMethodDefinition>() != null);

      return (SpecializedMethodDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedMethodDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized method reference.
    /// </summary>
    public SpecializedMethodReference Copy(ISpecializedMethodReference specializedMethodReference) {
      Contract.Requires(specializedMethodReference != null);
      Contract.Requires(!(specializedMethodReference is Dummy));
      Contract.Ensures(Contract.Result<SpecializedMethodReference>() != null);

      return (SpecializedMethodReference)this.SubstituteCopiesForOriginals.Substitute(specializedMethodReference);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized nested type definition.
    /// </summary>
    public SpecializedNestedTypeDefinition Copy(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
      Contract.Requires(specializedNestedTypeDefinition != null);
      Contract.Requires(!(specializedNestedTypeDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedNestedTypeDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse((INestedTypeDefinition)specializedNestedTypeDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(specializedNestedTypeDefinition.UnspecializedVersion);
      return (SpecializedNestedTypeDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedNestedTypeDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized nested type definition.
    /// </summary>
    private SpecializedNestedTypeDefinition CopySpecialized(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
      Contract.Requires(specializedNestedTypeDefinition != null);
      Contract.Requires(!(specializedNestedTypeDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedNestedTypeDefinition>() != null);

      return (SpecializedNestedTypeDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedNestedTypeDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized nested type reference.
    /// </summary>
    public SpecializedNestedTypeReference Copy(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      Contract.Requires(specializedNestedTypeReference != null);
      Contract.Requires(!(specializedNestedTypeReference is Dummy));
      Contract.Ensures(Contract.Result<SpecializedNestedTypeReference>() != null);

      return (SpecializedNestedTypeReference)this.SubstituteCopiesForOriginals.Substitute(specializedNestedTypeReference);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized property definition.
    /// </summary>
    public SpecializedPropertyDefinition Copy(ISpecializedPropertyDefinition specializedPropertyDefinition) {
      Contract.Requires(specializedPropertyDefinition != null);
      Contract.Requires(!(specializedPropertyDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedPropertyDefinition>() != null);

      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse((IPropertyDefinition)specializedPropertyDefinition);
      this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(specializedPropertyDefinition.UnspecializedVersion);
      return (SpecializedPropertyDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedPropertyDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the given specialized property definition.
    /// </summary>
    private SpecializedPropertyDefinition CopySpecialized(ISpecializedPropertyDefinition specializedPropertyDefinition) {
      Contract.Requires(specializedPropertyDefinition != null);
      Contract.Requires(!(specializedPropertyDefinition is Dummy));
      Contract.Ensures(Contract.Result<SpecializedPropertyDefinition>() != null);

      return (SpecializedPropertyDefinition)this.SubstituteCopiesForOriginals.Substitute(specializedPropertyDefinition);
    }

    /// <summary>
    /// Returns a deep copy of the specified type definition.
    /// </summary>
    public ITypeDefinition Copy(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);
      Contract.Requires(!(typeDefinition is Dummy));
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);

      typeDefinition.Dispatch(this.Dispatcher);
      return (ITypeDefinition)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the specified type member.
    /// </summary>
    public TypeDefinitionMember Copy(ITypeDefinitionMember typeMember) {
      Contract.Requires(typeMember != null);
      Contract.Requires(!(typeMember is Dummy));
      Contract.Ensures(Contract.Result<TypeDefinitionMember>() != null);

      typeMember.Dispatch(this.Dispatcher);
      return (TypeDefinitionMember)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the specified type reference.
    /// </summary>
    public TypeReference Copy(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Requires(!(typeReference is Dummy));
      Contract.Ensures(Contract.Result<TypeReference>() != null);

      typeReference.DispatchAsReference(this.Dispatcher);
      return (TypeReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the specified unit namespace.
    /// </summary>
    public UnitNamespace Copy(IUnitNamespace unitNamespace) {
      Contract.Requires(unitNamespace != null);
      Contract.Requires(!(unitNamespace is Dummy));
      Contract.Ensures(Contract.Result<UnitNamespace>() != null);

      unitNamespace.Dispatch(this.Dispatcher);
      return (UnitNamespace)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the specified unit namespace.
    /// </summary>
    public UnitNamespaceReference Copy(IUnitNamespaceReference unitNamespace) {
      Contract.Requires(unitNamespace != null);
      Contract.Requires(!(unitNamespace is Dummy));
      Contract.Ensures(Contract.Result<UnitNamespaceReference>() != null);

      unitNamespace.DispatchAsReference(this.Dispatcher);
      return (UnitNamespaceReference)this.Dispatcher.result;
    }

    /// <summary>
    /// Returns a deep copy of the given Win32 resource.
    /// </summary>
    public Win32Resource Copy(IWin32Resource win32Resource) {
      Contract.Requires(win32Resource != null);
      Contract.Requires(!(win32Resource is Dummy));
      Contract.Ensures(Contract.Result<Win32Resource>() != null);

      return (Win32Resource)this.SubstituteCopiesForOriginals.Substitute(win32Resource);
    }

    /// <summary>
    /// Replaces each element of the given list with a deep copy of itself.
    /// </summary>
    /// <param name="definitions"></param>
    public void Copy(List<IDefinition> definitions) {
      Contract.Requires(definitions != null);

      var n = definitions.Count;
      for (int i = 0; i < n; i++)
        this.TraverseAndPopulateDefinitionCacheWithCopies.Traverse(definitions[i]);
      for (int i = 0; i < n; i++)
        definitions[i] = this.SubstituteCopiesForOriginals.Substitute(definitions[i]);
    }

    /// <summary>
    /// If the parameter has already been copied, return the copy. If not, assume that it is outside the cone and return this original.
    /// </summary>
    protected IParameterDefinition GetExistingCopyIfInsideCone(IParameterDefinition parameter) {
      Contract.Requires(parameter != null);
      Contract.Ensures(Contract.Result<IParameterDefinition>() != null);

      return this.SubstituteCopiesForOriginals.SubstituteReference(parameter);
    }

    /// <summary>
    /// If the property has already been copied, return the copy. If not, assume that it is outside the cone and return this original.
    /// </summary>
    protected IPropertyDefinition GetExistingCopyIfInsideCone(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Ensures(Contract.Result<IPropertyDefinition>() != null);

      return this.SubstituteCopiesForOriginals.SubstituteReference(propertyDefinition);
    }

  }

  /// <summary>
  /// A class that produces a mutable deep copy a given metadata model node. 
  /// 
  /// </summary>
  /// <remarks>
  /// The copy provided by this class is achieved in two phases. First, we define a mapping of cones by calling AddDefinition(A)
  /// for def-node(s) A(s) that is the root(s) of the cone. Then we call Substitute(A) for an A that is in the cone to get a copy of A. 
  /// 
  /// The mapping is reflected in the cache, which is a mapping from object to objects. It has dual roles. The first is to define the new
  /// copies of the def-nodes in the cone. The second, which is to ensure isomorphism of the two graphs, will be discussed later. 
  ///
  /// The is populated for definitions in the setup phase either by the contructor or by a sequence of calls to AddDefinition(). After 
  /// the return from the constructor (with a list of IDefinitions), or after the first Substitute method, the cone has been fixed. 
  /// Further calls to addDefinitions will result in an ApplicationException. 
  /// 
  /// [Substitute method]
  /// Given a cache c and a definition A, Substitute(c, A) returns a deep copy of A in which any node B is replaced by c(B). An exception 
  /// is thrown if c(B) is not already defined. 
  /// 
  /// If A is a reference, Substitute(c, A) return a copy of A in which all the references to a node B in the domain of c now point to
  /// c(B). 
  /// 
  /// [Internal Working and Auxiliary Copy Functions]
  /// When AddDefinition(A) is called, a traverser is created to populate c with pairs (B, B') for sub def nodes of A, causing c changed to c1.
  /// 
  /// In the substitution phase, DeepCopy methods are used. For a def-node B, DeepCopy(c1, B) first tries to look up in the cache for B, then
  /// it calls GetMutableShallowCopy(B) on a cache miss. Either way, we get a mutable B', DeepCopy is performed recursively on the sub nodes
  /// of B' (which, should be B's subnodes at the moment). The fact that we are seeing B as a definition normally means that B must be in someone's cone. 
  /// If B is not in c1's domain, an error must have been committed. A few exceptions are when a subnode contains a pointer to a parent def-node (but not
  /// a ref node), in which case GetMutableShallowCopyIfExists are used, because the parent is allowed to be outside the cone. Examples include
  /// a method body points to its method definition.
  /// 
  /// To deep copy a ref-node C, unless it is a short cut, a copy is made and its subnodes recursively deep copied.
  /// 
  /// </remarks>
  public class MetadataCopier {

    /// <summary>
    /// Create a copier with an empty mapping. The copier will maintain the isomorphism between the original and the copy. 
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    public MetadataCopier(IMetadataHost host) {
      this.host = host;
      this.cache = new Dictionary<object, object>();
    }

    /// <summary>
    /// Create a copier with a mapping betweem subdefinitions of rootOfCone and their new copy.   
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    /// <param name="rootOfCone">An definition that defines one (of the possibly many) root of the cone.</param>
    /// <param name="newTypes">Copies of the the type definitions in the cone under rootOfCone. This collection of type defs will be useful
    /// for the computation of all the types in the module. </param>
    public MetadataCopier(IMetadataHost host, IDefinition rootOfCone, out List<INamedTypeDefinition> newTypes)
      : this(host) {
      this.AddDefinition(rootOfCone, out newTypes);
      this.coneAlreadyFixed = true;
    }

    private MetadataTraverser definitionCollector;
    /// <summary>
    /// Given a root of cone as an IDefinition, call the right AddDefinition method according to the type of the root. 
    /// </summary>
    /// <param name="rootOfCone">The root node of a cone, which we will copy later on.</param>
    /// <param name="newTypes">Copies of the the old type definitions in the cone. This collection of type defs will be useful
    /// for the computation of all the types in the module. </param>
    public void AddDefinition(IDefinition rootOfCone, out List<INamedTypeDefinition> newTypes) {
      if (this.coneAlreadyFixed)
        throw new ApplicationException("cone is fixed.");
      newTypes = new List<INamedTypeDefinition>();
      if (this.definitionCollector == null) {
        this.definitionCollector = new MetadataTraverser();
        this.definitionCollector.PreorderVisitor = new CollectAndShallowCopyDefinitions(this, newTypes);
      }
      IAssembly assembly = rootOfCone as IAssembly;
      if (assembly != null) {
        this.AddDefinition(assembly);
        return;
      }
      IModule module = rootOfCone as IModule;
      if (module != null) {
        this.AddDefinition(module);
        return;
      }
      IRootUnitNamespace rootUnitNamespace = rootOfCone as IRootUnitNamespace;
      if (rootUnitNamespace != null) {
        this.definitionCollector.Traverse(rootUnitNamespace);
        return;
      }
      INestedUnitNamespace nestedUnitNamespace = rootOfCone as INestedUnitNamespace;
      if (nestedUnitNamespace != null) {
        this.definitionCollector.Traverse(nestedUnitNamespace);
        return;
      }
      ITypeDefinition typeDefinition = rootOfCone as ITypeDefinition;
      if (typeDefinition != null) {
        this.definitionCollector.Traverse(typeDefinition);
        return;
      }
      IGlobalFieldDefinition globalFieldDefinition = rootOfCone as IGlobalFieldDefinition;
      if (globalFieldDefinition != null) {
        this.definitionCollector.Traverse(globalFieldDefinition);
        return;
      }
      IFieldDefinition fieldDefinition = rootOfCone as IFieldDefinition;
      if (fieldDefinition != null) {
        this.definitionCollector.Traverse(fieldDefinition);
        return;
      }
      IGlobalMethodDefinition globalMethodDefinition = rootOfCone as IGlobalMethodDefinition;
      if (globalMethodDefinition != null) {
        this.definitionCollector.Traverse(globalMethodDefinition);
        return;
      }
      IMethodDefinition methodDefinition = rootOfCone as IMethodDefinition;
      if (methodDefinition != null) {
        this.definitionCollector.Traverse(methodDefinition);
        return;
      }
      IPropertyDefinition propertyDefinition = rootOfCone as IPropertyDefinition;
      if (propertyDefinition != null) {
        this.definitionCollector.Traverse(propertyDefinition);
        return;
      }
      IParameterDefinition parameterDefinition = rootOfCone as IParameterDefinition;
      if (parameterDefinition!= null) {
        this.definitionCollector.Traverse(parameterDefinition);
        return;
      }
      IGenericParameter genericParameter = rootOfCone as IGenericParameter;
      if (genericParameter != null) {
        this.definitionCollector.Traverse(genericParameter);
        return;
      }
      IEventDefinition eventDefinition = rootOfCone as IEventDefinition;
      if (eventDefinition != null) {
        this.definitionCollector.Traverse(eventDefinition);
        return;
      }
      Debug.Assert(false);
    }

    /// <summary>
    /// Maintains the mapping between nodes and their copies. 
    /// </summary>
    protected internal Dictionary<object, object> cache;
    /// <summary>
    /// All the types created during copying. 
    /// </summary>
    protected internal List<INamedTypeDefinition> flatListOfTypes = new List<INamedTypeDefinition>();
    /// <summary>
    /// A metadata host.
    /// </summary>
    protected internal IMetadataHost host;

    #region GetMutableShallowCopy Functions

    /// <summary>
    /// Create a mutable shallow copy according to the type of aliasForType.
    /// </summary>
    /// <param name="aliasForType"></param>
    /// <returns></returns>
    private AliasForType GetMutableShallowCopy(IAliasForType aliasForType) {
      INamespaceAliasForType namespaceAliasForType = aliasForType as INamespaceAliasForType;
      if (namespaceAliasForType != null) {
        var copy = new NamespaceAliasForType();
        copy.Copy(namespaceAliasForType, this.host.InternFactory);
        return copy;
      }
      INestedAliasForType nestedAliasForType = aliasForType as INestedAliasForType;
      if (nestedAliasForType != null) {
        var copy = new NestedAliasForType();
        copy.Copy(nestedAliasForType, this.host.InternFactory);
        return copy;
      }
      throw new InvalidOperationException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyReference"></param>
    /// <returns></returns>
    private AssemblyReference GetMutableShallowCopy(IAssemblyReference assemblyReference) {
      AssemblyReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(assemblyReference, out cachedValue);
      result = cachedValue as AssemblyReference;
      if (result != null) return result;
      result = new AssemblyReference();
      //TODO: pass in the host and let the mutable assembly reference try to resolve itself.
      result.Copy(assemblyReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customAttribute"></param>
    /// <returns></returns>
    private CustomAttribute GetMutableShallowCopy(ICustomAttribute customAttribute) {
      CustomAttribute result = null;
      result = new CustomAttribute();
      result.Copy(customAttribute, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customModifier"></param>
    /// <returns></returns>
    private CustomModifier GetMutableShallowCopy(ICustomModifier customModifier) {
      CustomModifier result = null;
      result = new CustomModifier();
      result.Copy(customModifier, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventDefinition"></param>
    /// <returns></returns>
    private EventDefinition GetMutableShallowCopy(IEventDefinition eventDefinition) {
      EventDefinition result = null;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(eventDefinition, out cachedValue);
      result = cachedValue as EventDefinition;
      if (result != null) return result;
      Debug.Assert(false);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fieldDefinition"></param>
    /// <returns></returns>
    private FieldDefinition GetMutableShallowCopy(IFieldDefinition fieldDefinition) {
      FieldDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(fieldDefinition, out cachedValue);
      result = cachedValue as FieldDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fieldReference"></param>
    /// <returns></returns>
    private FieldReference GetMutableShallowCopy(IFieldReference fieldReference) {
      FieldReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(fieldReference, out cachedValue);
      result = cachedValue as FieldReference;
      if (result != null) return result;
      result = new FieldReference();
      this.cache.Add(fieldReference, result);
      this.cache.Add(result, result);
      result.Copy(fieldReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileReference"></param>
    /// <returns></returns>
    private FileReference GetMutableShallowCopy(IFileReference fileReference) {
      FileReference result = null;
      result = new FileReference();
      result.Copy(fileReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="functionPointerTypeReference"></param>
    /// <returns></returns>
    private FunctionPointerTypeReference GetMutableShallowCopy(IFunctionPointerTypeReference functionPointerTypeReference) {
      FunctionPointerTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(functionPointerTypeReference, out cachedValue);
      result = cachedValue as FunctionPointerTypeReference;
      if (result != null) return result;
      result = new FunctionPointerTypeReference();
      this.cache.Add(functionPointerTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(functionPointerTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <returns></returns>
    private GenericMethodInstanceReference GetMutableShallowCopy(IGenericMethodInstanceReference genericMethodInstanceReference) {
      GenericMethodInstanceReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericMethodInstanceReference, out cachedValue);
      result = cachedValue as GenericMethodInstanceReference;
      if (result != null) return result;
      result = new GenericMethodInstanceReference();
      this.cache.Add(genericMethodInstanceReference, result);
      this.cache.Add(result, result);
      result.Copy(genericMethodInstanceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodParameter"></param>
    /// <returns></returns>
    private GenericMethodParameter GetMutableShallowCopy(IGenericMethodParameter genericMethodParameter) {
      GenericMethodParameter/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericMethodParameter, out cachedValue);
      result = cachedValue as GenericMethodParameter;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodParameterReference"></param>
    /// <returns></returns>
    private GenericMethodParameterReference GetMutableShallowCopy(IGenericMethodParameterReference genericMethodParameterReference) {
      GenericMethodParameterReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericMethodParameterReference, out cachedValue);
      result = cachedValue as GenericMethodParameterReference;
      if (result != null) return result;
      result = new GenericMethodParameterReference();
      this.cache.Add(genericMethodParameterReference, result);
      this.cache.Add(result, result);
      result.Copy(genericMethodParameterReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericTypeInstanceReference"></param>
    /// <returns></returns>
    private GenericTypeInstanceReference GetMutableShallowCopy(IGenericTypeInstanceReference genericTypeInstanceReference) {
      GenericTypeInstanceReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericTypeInstanceReference, out cachedValue);
      result = cachedValue as GenericTypeInstanceReference;
      if (result != null) return result;
      result = new GenericTypeInstanceReference();
      this.cache.Add(genericTypeInstanceReference, result);
      this.cache.Add(result, result);
      result.Copy(genericTypeInstanceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericTypeParameter"></param>
    /// <returns></returns>
    private GenericTypeParameter GetMutableShallowCopy(IGenericTypeParameter genericTypeParameter) {
      GenericTypeParameter/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericTypeParameter, out cachedValue);
      result = cachedValue as GenericTypeParameter;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericTypeParameterReference"></param>
    /// <returns></returns>
    private GenericTypeParameterReference GetMutableShallowCopy(IGenericTypeParameterReference genericTypeParameterReference) {
      GenericTypeParameterReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericTypeParameterReference, out cachedValue);
      result = cachedValue as GenericTypeParameterReference;
      if (result != null) return result;
      result = new GenericTypeParameterReference();
      this.cache.Add(genericTypeParameterReference, result);
      this.cache.Add(result, result);
      result.Copy(genericTypeParameterReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalFieldDefinition"></param>
    /// <returns></returns>
    private GlobalFieldDefinition GetMutableShallowCopy(IGlobalFieldDefinition globalFieldDefinition) {
      GlobalFieldDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(globalFieldDefinition, out cachedValue);
      result = cachedValue as GlobalFieldDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalMethodDefinition"></param>
    /// <returns></returns>
    private GlobalMethodDefinition GetMutableShallowCopy(IGlobalMethodDefinition globalMethodDefinition) {
      GlobalMethodDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(globalMethodDefinition, out cachedValue);
      result = cachedValue as GlobalMethodDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="localDefinition"></param>
    /// <returns></returns>
    private LocalDefinition GetMutableShallowCopy(ILocalDefinition localDefinition) {
      LocalDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(localDefinition, out cachedValue);
      result = cachedValue as LocalDefinition;
      if (result != null) return result;
      result = new LocalDefinition();
      this.cache.Add(localDefinition, result);
      this.cache.Add(result, result);
      result.Copy(localDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="managedPointerTypeReference"></param>
    /// <returns></returns>
    private ManagedPointerTypeReference GetMutableShallowCopy(IManagedPointerTypeReference managedPointerTypeReference) {
      ManagedPointerTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(managedPointerTypeReference, out cachedValue);
      result = cachedValue as ManagedPointerTypeReference;
      if (result != null) return result;
      result = new ManagedPointerTypeReference();
      this.cache.Add(managedPointerTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(managedPointerTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="marshallingInformation"></param>
    /// <returns></returns>
    private MarshallingInformation GetMutableShallowCopy(IMarshallingInformation marshallingInformation) {
      MarshallingInformation result = null;
      result = new MarshallingInformation();
      result.Copy(marshallingInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataConstant"></param>
    /// <returns></returns>
    private MetadataConstant GetMutableShallowCopy(IMetadataConstant metadataConstant) {
      MetadataConstant result = null;
      result = new MetadataConstant();
      result.Copy(metadataConstant, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataCreateArray"></param>
    /// <returns></returns>
    private MetadataCreateArray GetMutableShallowCopy(IMetadataCreateArray metadataCreateArray) {
      MetadataCreateArray result = null;
      result = new MetadataCreateArray();
      result.Copy(metadataCreateArray, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataNamedArgument"></param>
    /// <returns></returns>
    private MetadataNamedArgument GetMutableShallowCopy(IMetadataNamedArgument metadataNamedArgument) {
      MetadataNamedArgument result = null;
      result = new MetadataNamedArgument();
      result.Copy(metadataNamedArgument, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataTypeOf"></param>
    /// <returns></returns>
    private MetadataTypeOf GetMutableShallowCopy(IMetadataTypeOf metadataTypeOf) {
      MetadataTypeOf result = null;
      result = new MetadataTypeOf();
      result.Copy(metadataTypeOf, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodDefinition"></param>
    /// <returns></returns>
    private MethodDefinition GetMutableShallowCopy(IMethodDefinition methodDefinition) {
      MethodDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(methodDefinition, out cachedValue);
      result = cachedValue as MethodDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodBody"></param>
    /// <returns></returns>
    private MethodBody GetMutableShallowCopy(IMethodBody methodBody) {
      MethodBody result = null;
      result = new MethodBody();
      result.Copy(methodBody, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodImplementation"></param>
    /// <returns></returns>
    private MethodImplementation GetMutableShallowCopy(IMethodImplementation methodImplementation) {
      MethodImplementation result = null;
      result = new MethodImplementation();
      result.Copy(methodImplementation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    private MethodReference GetMutableShallowCopy(IMethodReference methodReference) {
      MethodReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(methodReference, out cachedValue);
      result = cachedValue as MethodReference;
      if (result != null) return result;
      result = new MethodReference();
      this.cache.Add(methodReference, result);
      this.cache.Add(result, result);
      result.Copy(methodReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifiedTypeReference"></param>
    /// <returns></returns>
    private ModifiedTypeReference GetMutableShallowCopy(IModifiedTypeReference modifiedTypeReference) {
      ModifiedTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(modifiedTypeReference, out cachedValue);
      result = cachedValue as ModifiedTypeReference;
      if (result != null) return result;
      result = new ModifiedTypeReference();
      this.cache.Add(modifiedTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(modifiedTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    private Module GetMutableShallowCopy(IModule module) {
      Module/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(module, out cachedValue);
      result = cachedValue as Module;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moduleReference"></param>
    /// <returns></returns>
    private ModuleReference GetMutableShallowCopy(IModuleReference moduleReference) {
      ModuleReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(moduleReference, out cachedValue);
      result = cachedValue as ModuleReference;
      if (result != null) return result;
      result = new ModuleReference();
      this.cache.Add(moduleReference, result);
      this.cache.Add(result, result);
      result.Copy(moduleReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceAliasForType"></param>
    /// <returns></returns>
    private NamespaceAliasForType GetMutableShallowCopy(INamespaceAliasForType namespaceAliasForType) {
      NamespaceAliasForType result = null;
      result = new NamespaceAliasForType();
      result.Copy(namespaceAliasForType, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceTypeDefinition"></param>
    /// <returns></returns>
    private NamespaceTypeDefinition GetMutableShallowCopy(INamespaceTypeDefinition namespaceTypeDefinition) {
      NamespaceTypeDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(namespaceTypeDefinition, out cachedValue);
      result = cachedValue as NamespaceTypeDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceTypeReference"></param>
    /// <returns></returns>
    private NamespaceTypeReference GetMutableShallowCopy(INamespaceTypeReference namespaceTypeReference) {
      NamespaceTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(namespaceTypeReference, out cachedValue);
      result = cachedValue as NamespaceTypeReference;
      if (result != null) return result;
      result = new NamespaceTypeReference();
      this.cache.Add(namespaceTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(namespaceTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedAliasForType"></param>
    /// <returns></returns>
    private NestedAliasForType GetMutableShallowCopy(INestedAliasForType nestedAliasForType) {
      NestedAliasForType result = null;
      result = new NestedAliasForType();
      result.Copy(nestedAliasForType, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedTypeDefinition"></param>
    /// <returns></returns>
    private NestedTypeDefinition GetMutableShallowCopy(INestedTypeDefinition nestedTypeDefinition) {
      NestedTypeDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(nestedTypeDefinition, out cachedValue);
      result = cachedValue as NestedTypeDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedTypeReference"></param>
    /// <returns></returns>
    private NestedTypeReference GetMutableShallowCopy(INestedTypeReference nestedTypeReference) {
      NestedTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(nestedTypeReference, out cachedValue);
      result = cachedValue as NestedTypeReference;
      if (result != null) return result;
      result = new NestedTypeReference();
      this.cache.Add(nestedTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(nestedTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedUnitNamespace"></param>
    /// <returns></returns>
    private NestedUnitNamespace GetMutableShallowCopy(INestedUnitNamespace nestedUnitNamespace) {
      NestedUnitNamespace/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(nestedUnitNamespace, out cachedValue);
      result = cachedValue as NestedUnitNamespace;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedUnitNamespaceReference"></param>
    /// <returns></returns>
    private NestedUnitNamespaceReference GetMutableShallowCopy(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      NestedUnitNamespaceReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(nestedUnitNamespaceReference, out cachedValue);
      result = cachedValue as NestedUnitNamespaceReference;
      if (result != null) return result;
      result = new NestedUnitNamespaceReference();
      this.cache.Add(nestedUnitNamespaceReference, result);
      this.cache.Add(result, result);
      result.Copy(nestedUnitNamespaceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    private Operation GetMutableShallowCopy(IOperation operation) {
      Operation result = null;
      result = new Operation();
      result.Copy(operation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operationExceptionInformation"></param>
    /// <returns></returns>
    private OperationExceptionInformation GetMutableShallowCopy(IOperationExceptionInformation operationExceptionInformation) {
      OperationExceptionInformation result = null;
      result = new OperationExceptionInformation();
      result.Copy(operationExceptionInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterDefinition"></param>
    /// <returns></returns>
    private ParameterDefinition GetMutableShallowCopy(IParameterDefinition parameterDefinition) {
      ParameterDefinition/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(parameterDefinition, out cachedValue);
      result = cachedValue as ParameterDefinition;
      if (result != null) return result;
      Debug.Assert(false);
      result = new ParameterDefinition();
      this.cache.Add(parameterDefinition, result);
      this.cache.Add(result, result);
      result.Copy(parameterDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterTypeInformation"></param>
    /// <returns></returns>
    private ParameterTypeInformation GetMutableShallowCopy(IParameterTypeInformation parameterTypeInformation) {
      ParameterTypeInformation result = null;
      result = new ParameterTypeInformation();
      result.Copy(parameterTypeInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="peSection"></param>
    /// <returns></returns>
    private PESection GetMutableShallowCopy(IPESection peSection) {
      PESection result = null;
      result = new PESection();
      result.Copy(peSection, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformInvokeInformation"></param>
    /// <returns></returns>
    private PlatformInvokeInformation GetMutableShallowCopy(IPlatformInvokeInformation platformInvokeInformation) {
      PlatformInvokeInformation result = null;
      result = new PlatformInvokeInformation();
      result.Copy(platformInvokeInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointerTypeReference"></param>
    /// <returns></returns>
    private PointerTypeReference GetMutableShallowCopy(IPointerTypeReference pointerTypeReference) {
      PointerTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(pointerTypeReference, out cachedValue);
      result = cachedValue as PointerTypeReference;
      if (result != null) return result;
      result = new PointerTypeReference();
      this.cache.Add(pointerTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(pointerTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyDefinition"></param>
    /// <returns></returns>
    private PropertyDefinition GetMutableShallowCopy(IPropertyDefinition propertyDefinition) {
      PropertyDefinition result = null;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(propertyDefinition, out cachedValue);
      result = cachedValue as PropertyDefinition;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceReference"></param>
    /// <returns></returns>
    private ResourceReference GetMutableShallowCopy(IResourceReference resourceReference) {
      ResourceReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(resourceReference, out cachedValue);
      result = cachedValue as ResourceReference;
      if (result != null) return result;
      result = new ResourceReference();
      this.cache.Add(resourceReference, result);
      this.cache.Add(result, result);
      result.Copy(resourceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootUnitNamespace"></param>
    /// <returns></returns>
    private RootUnitNamespace GetMutableShallowCopy(IRootUnitNamespace rootUnitNamespace) {
      RootUnitNamespace/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(rootUnitNamespace, out cachedValue);
      result = cachedValue as RootUnitNamespace;
      Debug.Assert(result != null);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootUnitNamespaceReference"></param>
    /// <returns></returns>
    private RootUnitNamespaceReference GetMutableShallowCopy(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      RootUnitNamespaceReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(rootUnitNamespaceReference, out cachedValue);
      result = cachedValue as RootUnitNamespaceReference;
      if (result != null) return result;
      result = new RootUnitNamespaceReference();
      this.cache.Add(rootUnitNamespaceReference, result);
      this.cache.Add(result, result);
      result.Copy(rootUnitNamespaceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sectionBlock"></param>
    /// <returns></returns>
    private SectionBlock GetMutableShallowCopy(ISectionBlock sectionBlock) {
      SectionBlock/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(sectionBlock, out cachedValue);
      result = cachedValue as SectionBlock;
      if (result != null) return result;
      result = new SectionBlock();
      this.cache.Add(sectionBlock, result);
      this.cache.Add(result, result);
      result.Copy(sectionBlock, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="securityAttribute"></param>
    /// <returns></returns>
    private SecurityAttribute GetMutableShallowCopy(ISecurityAttribute securityAttribute) {
      SecurityAttribute result = null;
      result = new SecurityAttribute();
      result.Copy(securityAttribute, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedFieldReference"></param>
    /// <returns></returns>
    private SpecializedFieldReference GetMutableShallowCopy(ISpecializedFieldReference specializedFieldReference) {
      SpecializedFieldReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(specializedFieldReference, out cachedValue);
      result = cachedValue as SpecializedFieldReference;
      if (result != null) return result;
      result = new SpecializedFieldReference();
      this.cache.Add(specializedFieldReference, result);
      this.cache.Add(result, result);
      result.Copy(specializedFieldReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedMethodReference"></param>
    /// <returns></returns>
    private SpecializedMethodReference GetMutableShallowCopy(ISpecializedMethodReference specializedMethodReference) {
      SpecializedMethodReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(specializedMethodReference, out cachedValue);
      result = cachedValue as SpecializedMethodReference;
      if (result != null) return result;
      result = new SpecializedMethodReference();
      this.cache.Add(specializedMethodReference, result);
      this.cache.Add(result, result);
      result.Copy(specializedMethodReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedNestedTypeReference"></param>
    /// <returns></returns>
    private SpecializedNestedTypeReference GetMutableShallowCopy(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      SpecializedNestedTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(specializedNestedTypeReference, out cachedValue);
      result = cachedValue as SpecializedNestedTypeReference;
      if (result != null) return result;
      result = new SpecializedNestedTypeReference();
      this.cache.Add(specializedNestedTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(specializedNestedTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="win32Resource"></param>
    /// <returns></returns>
    private Win32Resource GetMutableShallowCopy(IWin32Resource win32Resource) {
      Win32Resource result = null;
      result = new Win32Resource();
      result.Copy(win32Resource, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixTypeReference"></param>
    /// <returns></returns>
    private MatrixTypeReference GetMutableMatrixShallowCopy(IArrayTypeReference matrixTypeReference) {
      MatrixTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(matrixTypeReference, out cachedValue);
      result = cachedValue as MatrixTypeReference;
      if (result != null) return result;
      result = new MatrixTypeReference();
      this.cache.Add(matrixTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(matrixTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vectorTypeReference"></param>
    /// <returns></returns>
    private VectorTypeReference GetMutableVectorShallowCopy(IArrayTypeReference vectorTypeReference) {
      VectorTypeReference/*?*/ result;
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(vectorTypeReference, out cachedValue);
      result = cachedValue as VectorTypeReference;
      if (result != null) return result;
      result = new VectorTypeReference();
      this.cache.Add(vectorTypeReference, result);
      this.cache.Add(result, result);
      result.Copy(vectorTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// Get a mutable copy of the given method reference. 
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    private IMethodReference GetTypeSpecificMutableShallowCopy(IMethodReference methodReference) {
      ISpecializedMethodReference/*?*/ specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null)
        return this.GetMutableShallowCopy(specializedMethodReference);
      IGenericMethodInstanceReference/*?*/ genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null)
        return this.GetMutableShallowCopy(genericMethodInstanceReference);
      else {
        return this.GetMutableShallowCopy(methodReference);
      }
    }

    #endregion

    #region Deep Copy
    /// <summary>
    /// Visit alias for type.
    /// </summary>
    /// <param name="aliasForType"></param>
    /// <returns></returns>
    protected virtual AliasForType DeepCopy(AliasForType aliasForType) {
      aliasForType.AliasedType = (INamedTypeReference)this.DeepCopy(aliasForType.AliasedType);
      aliasForType.Members = this.DeepCopy(aliasForType.Members);
      return aliasForType;
    }

    /// <summary>
    /// Visits the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    protected virtual Assembly DeepCopy(Assembly assembly) {
      assembly.AssemblyAttributes = this.DeepCopy(assembly.AssemblyAttributes);
      assembly.ExportedTypes = this.DeepCopy(assembly.ExportedTypes);
      assembly.Files = this.DeepCopy(assembly.Files);
      assembly.MemberModules = this.DeepCopy(assembly.MemberModules);
      assembly.Resources = this.DeepCopy(assembly.Resources);
      assembly.SecurityAttributes = this.DeepCopy(assembly.SecurityAttributes);
      this.DeepCopy((Module)assembly);
      return assembly;
    }

    /// <summary>
    /// Visits the specified assembly reference.
    /// </summary>
    /// <param name="assemblyReference">The assembly reference.</param>
    /// <returns></returns>
    protected virtual AssemblyReference DeepCopy(AssemblyReference assemblyReference) {
      if (!(assemblyReference.ResolvedAssembly is Dummy)) { //TODO: make AssemblyReference smart enough to resolve itself.
        object/*?*/ mutatedResolvedAssembly = null;
        if (this.cache.TryGetValue(assemblyReference.ResolvedAssembly, out mutatedResolvedAssembly)) {
          assemblyReference.ResolvedAssembly = (IAssembly)mutatedResolvedAssembly;
        }
      }
      assemblyReference.Host = this.host;
      return assemblyReference; //a shallow copy is also deep in this case.
    }

    /// <summary>
    /// Visits the specified custom attribute.
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    /// <returns></returns>
    protected virtual CustomAttribute DeepCopy(CustomAttribute customAttribute) {
      customAttribute.Arguments = this.DeepCopy(customAttribute.Arguments);
      customAttribute.Constructor = this.DeepCopy(customAttribute.Constructor);
      customAttribute.NamedArguments = this.DeepCopy(customAttribute.NamedArguments);
      return customAttribute;
    }

    /// <summary>
    /// Visits the specified custom modifier.
    /// </summary>
    /// <param name="customModifier">The custom modifier.</param>
    /// <returns></returns>
    protected virtual CustomModifier DeepCopy(CustomModifier customModifier) {
      customModifier.Modifier = this.DeepCopy(customModifier.Modifier);
      return customModifier;
    }

    /// <summary>
    /// Visits the specified event definition.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <returns></returns>
    protected virtual EventDefinition DeepCopy(EventDefinition eventDefinition) {
      this.DeepCopy((TypeDefinitionMember)eventDefinition);
      // Make sure adder and remover are in accessors.
      int adderIndex = -1, removerIndex = -1;
      Debug.Assert(eventDefinition.Accessors.Count >= 0);
      Debug.Assert(eventDefinition.Accessors.Count <= 2);
      if (eventDefinition.Accessors.Count > 0) {
        if (eventDefinition.Adder == eventDefinition.Accessors[0]) adderIndex = 0;
        else if (eventDefinition.Remover == eventDefinition.Accessors[0]) removerIndex = 0;
      }
      if (eventDefinition.Accessors.Count > 1) {
        if (eventDefinition.Adder == eventDefinition.Accessors[1]) adderIndex = 1;
        else if (eventDefinition.Remover == eventDefinition.Accessors[1]) removerIndex = 1;
      }
      eventDefinition.Accessors = this.DeepCopy(eventDefinition.Accessors);
      if (adderIndex != -1)
        eventDefinition.Adder = eventDefinition.Accessors[adderIndex];
      else
        eventDefinition.Adder = this.DeepCopy(eventDefinition.Adder);
      if (eventDefinition.Caller != null)
        eventDefinition.Caller = this.DeepCopy(eventDefinition.Caller);
      if (removerIndex != -1)
        eventDefinition.Remover = eventDefinition.Accessors[removerIndex];
      else
        eventDefinition.Remover = this.DeepCopy(eventDefinition.Remover);
      eventDefinition.Type = this.DeepCopy(eventDefinition.Type);
      return eventDefinition;
    }

    /// <summary>
    /// Visits the specified field definition.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <returns></returns>
    protected virtual FieldDefinition DeepCopy(FieldDefinition fieldDefinition) {
      this.DeepCopy((TypeDefinitionMember)fieldDefinition);
      if (fieldDefinition.IsCompileTimeConstant)
        fieldDefinition.CompileTimeValue = this.DeepCopy(this.GetMutableShallowCopy(fieldDefinition.CompileTimeValue));
      if (fieldDefinition.IsMapped)
        fieldDefinition.FieldMapping = this.DeepCopy(this.GetMutableShallowCopy(fieldDefinition.FieldMapping));
      if (fieldDefinition.IsModified)
        fieldDefinition.CustomModifiers = this.DeepCopy(fieldDefinition.CustomModifiers);
      if (fieldDefinition.IsMarshalledExplicitly)
        fieldDefinition.MarshallingInformation = this.DeepCopy(this.GetMutableShallowCopy(fieldDefinition.MarshallingInformation));
      fieldDefinition.Type = this.DeepCopy(fieldDefinition.Type);
      return fieldDefinition;
    }

    /// <summary>
    /// Visits the specified field reference.
    /// </summary>
    /// <param name="fieldReference">The field reference.</param>
    /// <returns></returns>
    protected virtual FieldReference DeepCopy(FieldReference fieldReference) {
      fieldReference.Attributes = this.DeepCopy(fieldReference.Attributes);
      if (fieldReference.IsModified)
        fieldReference.CustomModifiers = this.DeepCopy(fieldReference.CustomModifiers);
      fieldReference.ContainingType = this.DeepCopy(fieldReference.ContainingType);
      fieldReference.Locations = this.DeepCopy(fieldReference.Locations);
      fieldReference.Type = this.DeepCopy(fieldReference.Type);
      return fieldReference;
    }

    /// <summary>
    /// Visits the specified file reference.
    /// </summary>
    /// <param name="fileReference">The file reference.</param>
    /// <returns></returns>
    protected virtual FileReference DeepCopy(FileReference fileReference) {
      return fileReference;
    }

    /// <summary>
    /// Visits the specified function pointer type reference.
    /// </summary>
    /// <param name="functionPointerTypeReference">The function pointer type reference.</param>
    /// <returns></returns>
    protected virtual FunctionPointerTypeReference DeepCopy(FunctionPointerTypeReference functionPointerTypeReference) {
      this.DeepCopy((TypeReference)functionPointerTypeReference);
      functionPointerTypeReference.ExtraArgumentTypes = this.DeepCopy(functionPointerTypeReference.ExtraArgumentTypes);
      functionPointerTypeReference.Parameters = this.DeepCopy(functionPointerTypeReference.Parameters);
      if (functionPointerTypeReference.ReturnValueIsModified)
        functionPointerTypeReference.ReturnValueCustomModifiers = this.DeepCopy(functionPointerTypeReference.ReturnValueCustomModifiers);
      functionPointerTypeReference.Type = this.DeepCopy(functionPointerTypeReference.Type);
      return functionPointerTypeReference;
    }

    /// <summary>
    /// Visits the specified generic method instance reference.
    /// </summary>
    /// <param name="genericMethodInstanceReference">The generic method instance reference.</param>
    /// <returns></returns>
    protected virtual GenericMethodInstanceReference DeepCopy(GenericMethodInstanceReference genericMethodInstanceReference) {
      this.DeepCopy((MethodReference)genericMethodInstanceReference);
      genericMethodInstanceReference.GenericArguments = this.DeepCopy(genericMethodInstanceReference.GenericArguments);
      genericMethodInstanceReference.GenericMethod = this.DeepCopy(genericMethodInstanceReference.GenericMethod);
      return genericMethodInstanceReference;
    }

    /// <summary>
    /// Visits the specified generic method parameters.
    /// </summary>
    /// <param name="genericMethodParameters">The generic method parameters.</param>
    /// <param name="declaringMethod">The declaring method.</param>
    /// <returns></returns>
    protected virtual List<IGenericMethodParameter>/*?*/ DeepCopy(List<IGenericMethodParameter>/*?*/ genericMethodParameters, IMethodDefinition declaringMethod) {
      if (genericMethodParameters == null) return null;
      for (int i = 0, n = genericMethodParameters.Count; i < n; i++)
        genericMethodParameters[i] = this.DeepCopy(this.GetMutableShallowCopy(genericMethodParameters[i]));
      return genericMethodParameters;
    }

    /// <summary>
    /// Visits the specified generic method parameter.
    /// </summary>
    /// <param name="genericMethodParameter">The generic method parameter.</param>
    /// <returns></returns>
    protected virtual GenericMethodParameter DeepCopy(GenericMethodParameter genericMethodParameter) {
      this.DeepCopy((GenericParameter)genericMethodParameter);
      genericMethodParameter.DefiningMethod = this.GetMutableCopyIfItExists(genericMethodParameter.DefiningMethod);
      return genericMethodParameter;
    }

    /// <summary>
    /// Visits the specified generic method parameter reference.
    /// 
    /// </summary>
    /// <remarks>
    /// Avoid circular copy. 
    /// </remarks>
    /// <param name="genericMethodParameterReference">The generic method parameter reference.</param>
    /// <returns></returns>
    protected virtual GenericMethodParameterReference DeepCopy(GenericMethodParameterReference genericMethodParameterReference) {
      this.DeepCopy((TypeReference)genericMethodParameterReference);
      if (this.currentMethodReference == null) {
        // We are not copying a method reference.
        var definingMethod = this.GetTypeSpecificMutableShallowCopy(genericMethodParameterReference.DefiningMethod);
        if (definingMethod != genericMethodParameterReference.DefiningMethod) {
          genericMethodParameterReference.DefiningMethod = this.DeepCopy(definingMethod);
        }
      } else {
        // If we are, use the cached reference. TODO: a more systematic way of caching references. 
        genericMethodParameterReference.DefiningMethod = this.currentMethodReference;
      }
      return genericMethodParameterReference;
    }

    /// <summary>
    /// Visits the specified generic type parameter reference.
    /// </summary>
    /// <param name="genericTypeParameterReference">The generic type parameter reference.</param>
    /// <returns></returns>
    protected virtual GenericTypeParameterReference DeepCopy(GenericTypeParameterReference genericTypeParameterReference) {
      this.DeepCopy((TypeReference)genericTypeParameterReference);
      genericTypeParameterReference.DefiningType = this.DeepCopy(genericTypeParameterReference.DefiningType);
      return genericTypeParameterReference;
    }

    /// <summary>
    /// Visits the specified global field definition.
    /// </summary>
    /// <param name="globalFieldDefinition">The global field definition.</param>
    /// <returns></returns>
    protected virtual GlobalFieldDefinition DeepCopy(GlobalFieldDefinition globalFieldDefinition) {
      this.DeepCopy((FieldDefinition)globalFieldDefinition);
      globalFieldDefinition.ContainingNamespace = this.GetMutableCopyIfItExists(globalFieldDefinition.ContainingNamespace);
      return globalFieldDefinition;
    }

    /// <summary>
    /// Visits the specified global method definition.
    /// </summary>
    /// <param name="globalMethodDefinition">The global method definition.</param>
    /// <returns></returns>
    protected virtual GlobalMethodDefinition DeepCopy(GlobalMethodDefinition globalMethodDefinition) {
      this.DeepCopy((MethodDefinition)globalMethodDefinition);
      globalMethodDefinition.ContainingNamespace = this.GetMutableCopyIfItExists(globalMethodDefinition.ContainingNamespace);
      return globalMethodDefinition;
    }

    /// <summary>
    /// Visits the specified generic type instance reference.
    /// </summary>
    /// <param name="genericTypeInstanceReference">The generic type instance reference.</param>
    /// <returns></returns>
    protected virtual GenericTypeInstanceReference DeepCopy(GenericTypeInstanceReference genericTypeInstanceReference) {
      this.DeepCopy((TypeReference)genericTypeInstanceReference);
      genericTypeInstanceReference.GenericArguments = this.DeepCopy(genericTypeInstanceReference.GenericArguments);
      genericTypeInstanceReference.GenericType = (INamedTypeReference)this.DeepCopy(genericTypeInstanceReference.GenericType);
      return genericTypeInstanceReference;
    }

    /// <summary>
    /// Visits the specified generic parameter.
    /// </summary>
    /// <param name="genericParameter">The generic parameter.</param>
    /// <returns></returns>
    protected virtual GenericParameter DeepCopy(GenericParameter genericParameter) {
      genericParameter.Attributes = this.DeepCopy(genericParameter.Attributes);
      genericParameter.Constraints = this.DeepCopy(genericParameter.Constraints);
      return genericParameter;
    }

    /// <summary>
    /// Visits the specified generic type parameter.
    /// </summary>
    /// <param name="genericTypeParameter">The generic type parameter.</param>
    /// <returns></returns>
    protected virtual GenericTypeParameter DeepCopy(GenericTypeParameter genericTypeParameter) {
      this.DeepCopy((GenericParameter)genericTypeParameter);
      genericTypeParameter.DefiningType = this.GetMutableCopyIfItExists(genericTypeParameter.DefiningType);
      return genericTypeParameter;
    }

    /// <summary>
    /// Deep copy an alias for type.
    /// </summary>
    /// <param name="aliasForType"></param>
    /// <returns></returns>
    protected virtual IAliasForType DeepCopy(IAliasForType aliasForType) {
      return this.DeepCopy(this.GetMutableShallowCopy(aliasForType));
    }

    /// <summary>
    /// Deep copy an alias member. 
    /// </summary>
    /// <param name="aliasMember"></param>
    /// <returns></returns>
    protected virtual IAliasMember DeepCopy(IAliasMember aliasMember) {
      var nestedAliasForType = (INestedAliasForType)aliasMember;
      return this.DeepCopy(this.GetMutableShallowCopy(nestedAliasForType));
    }

    /// <summary>
    /// Visits the specified assembly reference.
    /// </summary>
    /// <param name="assemblyReference">The assembly reference.</param>
    /// <returns></returns>
    protected virtual IAssemblyReference DeepCopy(IAssemblyReference assemblyReference) {
      var assembly = assemblyReference as IAssembly;
      if (assembly != null) {
        object copy;
        if (this.cache.TryGetValue(assembly, out copy)) {
          return (IAssemblyReference)copy;
        }
        //if we get here, we are not referencing something inside the sub graph being copied,
        //so we need to make an explicit reference.
      }
      return this.DeepCopy(this.GetMutableShallowCopy(assemblyReference));
    }

    /// <summary>
    /// Visits the specified array type reference.
    /// </summary>
    /// <param name="arrayTypeReference">The array type reference.</param>
    /// <returns></returns>
    /// <remarks>Array types are not nominal types, so always visit the reference, even if it is a definition.</remarks>
    protected virtual IArrayTypeReference DeepCopy(IArrayTypeReference arrayTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(arrayTypeReference, out cachedValue)) {
        return (IArrayTypeReference)cachedValue;
      }
      if (arrayTypeReference.IsVector)
        return this.DeepCopy(this.GetMutableVectorShallowCopy(arrayTypeReference));
      else
        return this.DeepCopy(this.GetMutableMatrixShallowCopy(arrayTypeReference));
    }

    /// <summary>
    /// Deep copy an event definition. 
    /// </summary>
    /// <param name="eventDefinition"></param>
    /// <returns></returns>
    protected virtual IEventDefinition DeepCopy(IEventDefinition eventDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(eventDefinition));
    }

    /// <summary>
    /// Deep copy a field definition. 
    /// </summary>
    /// <param name="fieldDefinition"></param>
    /// <returns></returns>
    protected virtual IFieldDefinition DeepCopy(IFieldDefinition fieldDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(fieldDefinition));
    }

    /// <summary>
    /// Visits the specified field reference.
    /// </summary>
    /// <param name="fieldReference">The field reference.</param>
    /// <returns></returns>
    protected virtual IFieldReference DeepCopy(IFieldReference fieldReference) {
      object copy;
      if (this.cache.TryGetValue(fieldReference, out copy)) {
        return (IFieldReference)copy;
      }
      if (fieldReference is Dummy) return Dummy.FieldReference;
      ISpecializedFieldReference/*?*/ specializedFieldReference = fieldReference as ISpecializedFieldReference;
      if (specializedFieldReference != null)
        return this.DeepCopy(this.GetMutableShallowCopy(specializedFieldReference));
      return this.DeepCopy(this.GetMutableShallowCopy(fieldReference));
    }

    /// <summary>
    /// Visit the generic method instance reference. 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <returns></returns>
    protected virtual IGenericMethodInstanceReference DeepCopy(IGenericMethodInstanceReference genericMethodInstanceReference) {
      object cachedValue;
      if (this.cache.TryGetValue(genericMethodInstanceReference, out cachedValue)) {
        return (IGenericMethodInstanceReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(genericMethodInstanceReference));
    }

    /// <summary>
    /// Visits the specified generic method parameter reference.
    /// </summary>
    /// <param name="genericMethodParameterReference">The generic method parameter reference.</param>
    /// <returns></returns>
    protected virtual IGenericMethodParameterReference DeepCopy(IGenericMethodParameterReference genericMethodParameterReference) {
      //IGenericMethodParameter/*?*/ genericMethodParameter = genericMethodParameterReference as IGenericMethodParameter;
      //if (genericMethodParameter != null)
      //  return this.GetMutableShallowCopy(genericMethodParameter);
      object cachedValue;
      if (this.cache.TryGetValue(genericMethodParameterReference, out cachedValue)) {
        return (IGenericMethodParameterReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(genericMethodParameterReference));
    }

    /// <summary>
    /// Visits the specified generic type parameter reference.
    /// </summary>
    /// <param name="genericTypeParameterReference">The generic type parameter reference.</param>
    /// <returns></returns>
    protected virtual IGenericTypeParameterReference DeepCopy(IGenericTypeParameterReference genericTypeParameterReference) {
      //IGenericTypeParameter/*?*/ genericTypeParameter = genericTypeParameterReference as IGenericTypeParameter;
      //if (genericTypeParameter != null)
      //  return this.GetMutableShallowCopy(genericTypeParameter);
      object cachedValue;
      if (this.cache.TryGetValue(genericTypeParameterReference, out cachedValue)) {
        return (IGenericTypeParameterReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(genericTypeParameterReference));
    }

    /// <summary>
    /// Visits the specified generic type instance reference.
    /// </summary>
    /// <param name="genericTypeInstanceReference">The generic type instance reference.</param>
    /// <returns></returns>
    protected virtual IGenericTypeInstanceReference DeepCopy(IGenericTypeInstanceReference genericTypeInstanceReference) {
      object cachedValue;
      if (this.cache.TryGetValue(genericTypeInstanceReference, out cachedValue)) {
        return (IGenericTypeInstanceReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(genericTypeInstanceReference));
    }

    /// <summary>
    /// Visits the specified global field definition.
    /// </summary>
    /// <param name="globalFieldDefinition">The global field definition.</param>
    /// <returns></returns>
    protected virtual IGlobalFieldDefinition DeepCopy(IGlobalFieldDefinition globalFieldDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(globalFieldDefinition));
    }

    /// <summary>
    /// Visits the specified global method definition.
    /// </summary>
    /// <param name="globalMethodDefinition">The global method definition.</param>
    /// <returns></returns>
    protected virtual IGlobalMethodDefinition DeepCopy(IGlobalMethodDefinition globalMethodDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(globalMethodDefinition));
    }

    /// <summary>
    /// Visits the specified location.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns></returns>
    protected virtual ILocation DeepCopy(ILocation location) {
      return location;
    }

    /// <summary>
    /// Visits the specified method definition.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns></returns>
    protected virtual IMethodDefinition DeepCopy(IMethodDefinition methodDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(methodDefinition));
    }

    /// <summary>
    /// Visits the specified method reference.
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns></returns>
    protected virtual IMethodReference DeepCopy(IMethodReference methodReference) {
      object cachedValue;
      if (this.cache.TryGetValue(methodReference, out cachedValue)) {
        return (IMethodReference)cachedValue;
      }
      if (methodReference is Dummy) return Dummy.MethodReference;
      ISpecializedMethodReference/*?*/ specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null)
        return this.DeepCopy(this.GetMutableShallowCopy(specializedMethodReference));
      IGenericMethodInstanceReference/*?*/ genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null)
        return this.DeepCopy(this.GetMutableShallowCopy(genericMethodInstanceReference));
      else {
        return this.DeepCopy(this.GetMutableShallowCopy(methodReference));
      }
    }

    /// <summary>
    /// Makes a deep copy of the specified namespace type alias.
    /// </summary>
    /// <param name="namespaceAliasForType"></param>
    /// <returns></returns>
    protected virtual INamespaceAliasForType DeepCopy(INamespaceAliasForType namespaceAliasForType) {
      object cachedValue;
      if (this.cache.TryGetValue(namespaceAliasForType, out cachedValue)) {
        return (INamespaceAliasForType)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(namespaceAliasForType));
    }

    /// <summary>
    /// Visits the specified namespace member.
    /// </summary>
    /// <param name="namespaceMember">The namespace member.</param>
    /// <returns></returns>
    protected virtual INamespaceMember DeepCopy(INamespaceMember namespaceMember) {
      INamespaceTypeDefinition/*?*/ namespaceTypeDefinition = namespaceMember as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) return this.DeepCopy(namespaceTypeDefinition);
      INestedUnitNamespace/*?*/ nestedUnitNamespace = namespaceMember as INestedUnitNamespace;
      if (nestedUnitNamespace != null) return this.DeepCopy(nestedUnitNamespace);
      IGlobalMethodDefinition/*?*/ globalMethodDefinition = namespaceMember as IGlobalMethodDefinition;
      if (globalMethodDefinition != null) return this.DeepCopy(globalMethodDefinition);
      IGlobalFieldDefinition/*?*/ globalFieldDefinition = namespaceMember as IGlobalFieldDefinition;
      if (globalFieldDefinition != null) return this.DeepCopy(globalFieldDefinition);
      INamespaceAliasForType/*?*/ namespaceAliasForType = namespaceMember as INamespaceAliasForType;
      if (namespaceMember != null) return this.DeepCopy(namespaceAliasForType);
      return namespaceMember;
    }

    /// <summary>
    /// Visits the specified namespace type reference.
    /// </summary>
    /// <param name="namespaceTypeReference">The namespace type reference.</param>
    /// <returns></returns>
    protected virtual INamespaceTypeReference DeepCopy(INamespaceTypeReference namespaceTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(namespaceTypeReference, out cachedValue)) {
        return (INamespaceTypeReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(namespaceTypeReference));
    }

    /// <summary>
    /// Visits the specified nested type reference.
    /// </summary>
    /// <param name="nestedTypeReference">The nested type reference.</param>
    /// <returns></returns>
    protected virtual INestedTypeReference DeepCopy(INestedTypeReference nestedTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(nestedTypeReference, out cachedValue)) {
        return (INestedTypeReference)cachedValue;
      }
      ISpecializedNestedTypeReference/*?*/ specializedNestedTypeReference = nestedTypeReference as ISpecializedNestedTypeReference;
      if (specializedNestedTypeReference != null)
        return this.DeepCopy(this.GetMutableShallowCopy(specializedNestedTypeReference));
      return this.DeepCopy(this.GetMutableShallowCopy(nestedTypeReference));
    }

    /// <summary>
    /// Visits the specified type definition member.
    /// </summary>
    /// <param name="typeDefinitionMember">The type definition member.</param>
    /// <returns></returns>
    protected virtual ITypeDefinitionMember DeepCopy(ITypeDefinitionMember typeDefinitionMember) {
      IEventDefinition/*?*/ eventDef = typeDefinitionMember as IEventDefinition;
      if (eventDef != null) return this.DeepCopy(eventDef);
      IFieldDefinition/*?*/ fieldDef = typeDefinitionMember as IFieldDefinition;
      if (fieldDef != null) return this.DeepCopy(fieldDef);
      IMethodDefinition/*?*/ methodDef = typeDefinitionMember as IMethodDefinition;
      if (methodDef != null) return this.DeepCopy(methodDef);
      INestedTypeDefinition/*?*/ nestedTypeDef = typeDefinitionMember as INestedTypeDefinition;
      if (nestedTypeDef != null) return this.DeepCopy(nestedTypeDef);
      IPropertyDefinition/*?*/ propertyDef = typeDefinitionMember as IPropertyDefinition;
      if (propertyDef != null) return this.DeepCopy(propertyDef);
      Debug.Assert(false);
      return typeDefinitionMember;
    }


    /// <summary>
    /// Visits the specified aliases for types.
    /// </summary>
    /// <param name="aliasesForTypes">The aliases for types.</param>
    /// <returns></returns>
    protected virtual List<IAliasForType>/*?*/ DeepCopy(List<IAliasForType>/*?*/ aliasesForTypes) {
      if (aliasesForTypes == null) return null;
      for (int i = 0, n = aliasesForTypes.Count; i < n; i++)
        aliasesForTypes[i] = this.DeepCopy(aliasesForTypes[i]);
      return aliasesForTypes;
    }

    /// <summary>
    /// Deep copy a list of alias member without copying the list. 
    /// </summary>
    /// <param name="aliasMembers"></param>
    /// <returns></returns>
    protected virtual List<IAliasMember>/*?*/ DeepCopy(List<IAliasMember>/*?*/ aliasMembers) {
      if (aliasMembers == null) return null;
      for (int i = 0, n = aliasMembers.Count; i < n; i++) {
        aliasMembers[i] = this.DeepCopy(aliasMembers[i]);
      }
      return aliasMembers;
    }

    /// <summary>
    /// Visits the specified assembly references.
    /// </summary>
    /// <param name="assemblyReferences">The assembly references.</param>
    /// <returns></returns>
    protected virtual List<IAssemblyReference>/*?*/ DeepCopy(List<IAssemblyReference>/*?*/ assemblyReferences) {
      if (assemblyReferences == null) return null;
      for (int i = 0, n = assemblyReferences.Count; i < n; i++)
        assemblyReferences[i] = this.DeepCopy(assemblyReferences[i]);
      return assemblyReferences;
    }

    /// <summary>
    /// Visits the specified custom attributes.
    /// </summary>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns></returns>
    protected virtual List<ICustomAttribute>/*?*/ DeepCopy(List<ICustomAttribute>/*?*/ customAttributes) {
      if (customAttributes == null) return null;
      for (int i = 0, n = customAttributes.Count; i < n; i++)
        customAttributes[i] = this.DeepCopy(this.GetMutableShallowCopy(customAttributes[i]));
      return customAttributes;
    }

    /// <summary>
    /// Visits the specified custom modifiers.
    /// </summary>
    /// <param name="customModifiers">The custom modifiers.</param>
    /// <returns></returns>
    protected virtual List<ICustomModifier>/*?*/ DeepCopy(List<ICustomModifier>/*?*/ customModifiers) {
      if (customModifiers == null) return null;
      for (int i = 0, n = customModifiers.Count; i < n; i++)
        customModifiers[i] = this.DeepCopy(this.GetMutableShallowCopy(customModifiers[i]));
      return customModifiers;
    }

    /// <summary>
    /// Visits the specified event definitions.
    /// </summary>
    /// <param name="eventDefinitions">The event definitions.</param>
    /// <returns></returns>
    protected virtual List<IEventDefinition>/*?*/ DeepCopy(List<IEventDefinition>/*?*/ eventDefinitions) {
      if (eventDefinitions == null) return null;
      for (int i = 0, n = eventDefinitions.Count; i < n; i++)
        eventDefinitions[i] = this.DeepCopy(eventDefinitions[i]);
      return eventDefinitions;
    }

    /// <summary>
    /// Visits the specified field definitions.
    /// </summary>
    /// <param name="fieldDefinitions">The field definitions.</param>
    /// <returns></returns>
    protected virtual List<IFieldDefinition>/*?*/ DeepCopy(List<IFieldDefinition>/*?*/ fieldDefinitions) {
      if (fieldDefinitions == null) return null;
      for (int i = 0, n = fieldDefinitions.Count; i < n; i++)
        fieldDefinitions[i] = this.DeepCopy(fieldDefinitions[i]);
      return fieldDefinitions;
    }

    /// <summary>
    /// Visits the specified file references.
    /// </summary>
    /// <param name="fileReferences">The file references.</param>
    /// <returns></returns>
    protected virtual List<IFileReference>/*?*/ DeepCopy(List<IFileReference>/*?*/ fileReferences) {
      if (fileReferences == null) return null;
      for (int i = 0, n = fileReferences.Count; i < n; i++)
        fileReferences[i] = this.DeepCopy(this.GetMutableShallowCopy(fileReferences[i]));
      return fileReferences;
    }

    /// <summary>
    /// Visits the specified generic type parameters.
    /// </summary>
    /// <param name="genericTypeParameters">The generic type parameters.</param>
    /// <returns></returns>
    protected virtual List<IGenericTypeParameter>/*?*/ DeepCopy(List<IGenericTypeParameter>/*?*/ genericTypeParameters) {
      if (genericTypeParameters == null) return null;
      for (int i = 0, n = genericTypeParameters.Count; i < n; i++)
        genericTypeParameters[i] = this.DeepCopy(this.GetMutableShallowCopy(genericTypeParameters[i]));
      return genericTypeParameters;
    }

    /// <summary>
    /// Visits the specified expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    protected virtual IMetadataExpression DeepCopy(IMetadataExpression expression) {
      IMetadataConstant/*?*/ metadataConstant = expression as IMetadataConstant;
      if (metadataConstant != null) return this.DeepCopy(this.GetMutableShallowCopy(metadataConstant));
      IMetadataCreateArray/*?*/ metadataCreateArray = expression as IMetadataCreateArray;
      if (metadataCreateArray != null) return this.DeepCopy(this.GetMutableShallowCopy(metadataCreateArray));
      IMetadataTypeOf/*?*/ metadataTypeOf = expression as IMetadataTypeOf;
      if (metadataTypeOf != null) return this.DeepCopy(this.GetMutableShallowCopy(metadataTypeOf));
      return expression;
    }

    /// <summary>
    /// Visits the specified module references.
    /// </summary>
    /// <param name="moduleReferences">The module references.</param>
    /// <returns></returns>
    protected virtual List<IModuleReference>/*?*/ DeepCopy(List<IModuleReference>/*?*/ moduleReferences) {
      if (moduleReferences == null) return null;
      for (int i = 0, n = moduleReferences.Count; i < n; i++)
        moduleReferences[i] = this.DeepCopy(moduleReferences[i]);
      return moduleReferences;
    }


    /// <summary>
    /// Visits the specified locations.
    /// </summary>
    /// <param name="locations">The locations.</param>
    /// <returns></returns>
    protected virtual List<ILocation>/*?*/ DeepCopy(List<ILocation>/*?*/ locations) {
      if (locations == null) return locations;
      for (int i = 0, n = locations.Count; i < n; i++)
        locations[i] = this.DeepCopy(locations[i]);
      return locations;
    }

    /// <summary>
    /// Visits the specified locals.
    /// </summary>
    /// <param name="locals">The locals.</param>
    /// <returns></returns>
    protected virtual List<ILocalDefinition>/*?*/ DeepCopy(List<ILocalDefinition>/*?*/ locals) {
      if (locals == null) return null;
      for (int i = 0, n = locals.Count; i < n; i++)
        locals[i] = this.DeepCopy(this.GetMutableShallowCopy(locals[i]));
      return locals;
    }

    /// <summary>
    /// Visits the specified metadata expressions.
    /// </summary>
    /// <param name="metadataExpressions">The metadata expressions.</param>
    /// <returns></returns>
    protected virtual List<IMetadataExpression>/*?*/ DeepCopy(List<IMetadataExpression>/*?*/ metadataExpressions) {
      if (metadataExpressions == null) return null;
      for (int i = 0, n = metadataExpressions.Count; i < n; i++)
        metadataExpressions[i] = this.DeepCopy(metadataExpressions[i]);
      return metadataExpressions;
    }

    /// <summary>
    /// Visits the specified named arguments.
    /// </summary>
    /// <param name="namedArguments">The named arguments.</param>
    /// <returns></returns>
    protected virtual List<IMetadataNamedArgument>/*?*/ DeepCopy(List<IMetadataNamedArgument>/*?*/ namedArguments) {
      if (namedArguments == null) return null;
      for (int i = 0, n = namedArguments.Count; i < n; i++)
        namedArguments[i] = this.DeepCopy(this.GetMutableShallowCopy(namedArguments[i]));
      return namedArguments;
    }

    /// <summary>
    /// Visits the specified method definitions.
    /// </summary>
    /// <param name="methodDefinitions">The method definitions.</param>
    /// <returns></returns>
    protected virtual List<IMethodDefinition>/*?*/ DeepCopy(List<IMethodDefinition>/*?*/ methodDefinitions) {
      if (methodDefinitions == null) return null;
      for (int i = 0, n = methodDefinitions.Count; i < n; i++)
        methodDefinitions[i] = this.DeepCopy(methodDefinitions[i]);
      return methodDefinitions;
    }

    /// <summary>
    /// Visits the specified method implementations.
    /// </summary>
    /// <param name="methodImplementations">The method implementations.</param>
    /// <returns></returns>
    protected virtual List<IMethodImplementation>/*?*/ DeepCopy(List<IMethodImplementation>/*?*/ methodImplementations) {
      if (methodImplementations == null) return null;
      for (int i = 0, n = methodImplementations.Count; i < n; i++)
        methodImplementations[i] = this.DeepCopy(this.GetMutableShallowCopy(methodImplementations[i]));
      return methodImplementations;
    }

    /// <summary>
    /// Visits the specified method references.
    /// </summary>
    /// <param name="methodReferences">The method references.</param>
    /// <returns></returns>
    protected virtual List<IMethodReference>/*?*/ DeepCopy(List<IMethodReference>/*?*/ methodReferences) {
      if (methodReferences == null) return null;
      for (int i = 0, n = methodReferences.Count; i < n; i++)
        methodReferences[i] = this.DeepCopy(methodReferences[i]);
      return methodReferences;
    }

    /// <summary>
    /// Visits the specified modules.
    /// </summary>
    /// <param name="modules">The modules.</param>
    /// <returns></returns>
    protected virtual List<IModule>/*?*/ DeepCopy(List<IModule>/*?*/ modules) {
      if (modules == null) return null;
      for (int i = 0, n = modules.Count; i < n; i++) {
        modules[i] = this.DeepCopy(this.GetMutableShallowCopy(modules[i]));
        this.flatListOfTypes.Clear();
      }
      return modules;
    }

    /// <summary>
    /// Visits the private helper members.
    /// </summary>
    /// <param name="typeDefinitions">The type definitions.</param>
    protected virtual void VisitPrivateHelperMembers(List<INamedTypeDefinition>/*?*/ typeDefinitions) {
      if (typeDefinitions == null) return;
      for (int i = 0, n = typeDefinitions.Count; i < n; i++) {
        NamedTypeDefinition/*?*/ typeDef = typeDefinitions[i] as NamedTypeDefinition;
        if (typeDef == null) continue;
        typeDef.PrivateHelperMembers = this.DeepCopy(typeDef.PrivateHelperMembers);
      }
    }

    /// <summary>
    /// Visits the specified namespace members.
    /// </summary>
    /// <param name="namespaceMembers">The namespace members.</param>
    /// <returns></returns>
    protected virtual List<INamespaceMember>/*?*/ DeepCopy(List<INamespaceMember>/*?*/ namespaceMembers) {
      if (namespaceMembers == null) return null;
      for (int i = 0, n = namespaceMembers.Count; i < n; i++)
        namespaceMembers[i] = this.DeepCopy(namespaceMembers[i]);
      return namespaceMembers;
    }

    /// <summary>
    /// Visits the specified nested type definitions.
    /// </summary>
    /// <param name="nestedTypeDefinitions">The nested type definitions.</param>
    /// <returns></returns>
    protected virtual List<INestedTypeDefinition>/*?*/ DeepCopy(List<INestedTypeDefinition>/*?*/ nestedTypeDefinitions) {
      if (nestedTypeDefinitions == null) return null;
      for (int i = 0, n = nestedTypeDefinitions.Count; i < n; i++)
        nestedTypeDefinitions[i] = this.DeepCopy(nestedTypeDefinitions[i]);
      return nestedTypeDefinitions;
    }

    /// <summary>
    /// Visits the specified operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <returns></returns>
    protected virtual List<IOperation>/*?*/ DeepCopy(List<IOperation>/*?*/ operations) {
      if (operations == null) return null;
      for (int i = 0, n = operations.Count; i < n; i++)
        operations[i] = this.DeepCopy(this.GetMutableShallowCopy(operations[i]));
      return operations;
    }

    /// <summary>
    /// Visits the specified exception informations.
    /// </summary>
    /// <param name="exceptionInformations">The exception informations.</param>
    /// <returns></returns>
    protected virtual List<IOperationExceptionInformation>/*?*/ DeepCopy(List<IOperationExceptionInformation>/*?*/ exceptionInformations) {
      if (exceptionInformations == null) return null;
      for (int i = 0, n = exceptionInformations.Count; i < n; i++)
        exceptionInformations[i] = this.DeepCopy(this.GetMutableShallowCopy(exceptionInformations[i]));
      return exceptionInformations;
    }

    /// <summary>
    /// Visits the specified type definition members.
    /// </summary>
    /// <param name="typeDefinitionMembers">The type definition members.</param>
    /// <returns></returns>
    protected virtual List<ITypeDefinitionMember>/*?*/ DeepCopy(List<ITypeDefinitionMember>/*?*/ typeDefinitionMembers) {
      if (typeDefinitionMembers == null) return null;
      for (int i = 0, n = typeDefinitionMembers.Count; i < n; i++)
        typeDefinitionMembers[i] = this.DeepCopy(typeDefinitionMembers[i]);
      return typeDefinitionMembers;
    }

    /// <summary>
    /// Visits the specified win32 resources.
    /// </summary>
    /// <param name="win32Resources">The win32 resources.</param>
    /// <returns></returns>
    protected virtual List<IWin32Resource>/*?*/ DeepCopy(List<IWin32Resource>/*?*/ win32Resources) {
      if (win32Resources == null) return null;
      for (int i = 0, n = win32Resources.Count; i < n; i++)
        win32Resources[i] = this.DeepCopy(this.GetMutableShallowCopy(win32Resources[i]));
      return win32Resources;
    }

    /// <summary>
    /// Visits the specified local definition.
    /// </summary>
    /// <param name="localDefinition">The local definition.</param>
    /// <returns></returns>
    protected virtual LocalDefinition DeepCopy(LocalDefinition localDefinition) {
      localDefinition.CustomModifiers = this.DeepCopy(localDefinition.CustomModifiers);
      localDefinition.Type = this.DeepCopy(localDefinition.Type);
      return localDefinition;
    }

    /// <summary>
    /// Visits the specified managed pointer type reference.
    /// </summary>
    /// <param name="managedPointerTypeReference">The pointer type reference.</param>
    /// <returns></returns>
    protected virtual ManagedPointerTypeReference DeepCopy(ManagedPointerTypeReference managedPointerTypeReference) {
      this.DeepCopy((TypeReference)managedPointerTypeReference);
      managedPointerTypeReference.TargetType = this.DeepCopy(managedPointerTypeReference.TargetType);
      return managedPointerTypeReference;
    }

    /// <summary>
    /// Visits the specified marshalling information.
    /// </summary>
    /// <param name="marshallingInformation">The marshalling information.</param>
    /// <returns></returns>
    protected virtual MarshallingInformation DeepCopy(MarshallingInformation marshallingInformation) {
      if (marshallingInformation.UnmanagedType == UnmanagedType.CustomMarshaler)
        marshallingInformation.CustomMarshaller = this.DeepCopy(marshallingInformation.CustomMarshaller);
      if (marshallingInformation.UnmanagedType == UnmanagedType.SafeArray &&
      (marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_DISPATCH ||
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_UNKNOWN ||
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_RECORD))
        marshallingInformation.SafeArrayElementUserDefinedSubtype = this.DeepCopy(marshallingInformation.SafeArrayElementUserDefinedSubtype);
      return marshallingInformation;
    }

    /// <summary>
    /// Visits the specified constant.
    /// </summary>
    /// <param name="constant">The constant.</param>
    /// <returns></returns>
    protected virtual MetadataConstant DeepCopy(MetadataConstant constant) {
      constant.Locations = this.DeepCopy(constant.Locations);
      constant.Type = this.DeepCopy(constant.Type);
      return constant;
    }

    /// <summary>
    /// Visits the specified create array.
    /// </summary>
    /// <param name="createArray">The create array.</param>
    /// <returns></returns>
    protected virtual MetadataCreateArray DeepCopy(MetadataCreateArray createArray) {
      createArray.ElementType = this.DeepCopy(createArray.ElementType);
      createArray.Initializers = this.DeepCopy(createArray.Initializers);
      createArray.Locations = this.DeepCopy(createArray.Locations);
      createArray.Type = this.DeepCopy(createArray.Type);
      return createArray;
    }

    /// <summary>
    /// Visits the specified method definition.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns></returns>
    protected virtual MethodDefinition DeepCopy(MethodDefinition methodDefinition) {
      this.DeepCopy((TypeDefinitionMember)methodDefinition);
      if (methodDefinition.IsGeneric)
        methodDefinition.GenericParameters = this.DeepCopy(methodDefinition.GenericParameters, methodDefinition);
      methodDefinition.Parameters = this.DeepCopy(methodDefinition.Parameters);
      if (methodDefinition.IsPlatformInvoke)
        methodDefinition.PlatformInvokeData = this.DeepCopy(this.GetMutableShallowCopy(methodDefinition.PlatformInvokeData));
      methodDefinition.ReturnValueAttributes = this.DeepCopyMethodReturnValueAttributes(methodDefinition.ReturnValueAttributes);
      if (methodDefinition.ReturnValueIsModified)
        methodDefinition.ReturnValueCustomModifiers = this.DeepCopyMethodReturnValueCustomModifiers(methodDefinition.ReturnValueCustomModifiers);
      if (methodDefinition.ReturnValueIsMarshalledExplicitly)
        methodDefinition.ReturnValueMarshallingInformation = this.DeepCopyMethodReturnValueMarshallingInformation(this.GetMutableShallowCopy(methodDefinition.ReturnValueMarshallingInformation));
      if (methodDefinition.HasDeclarativeSecurity)
        methodDefinition.SecurityAttributes = this.DeepCopy(methodDefinition.SecurityAttributes);
      methodDefinition.Type = this.DeepCopy(methodDefinition.Type);
      if (!methodDefinition.IsAbstract && !methodDefinition.IsExternal)
        // This is the hook so that the CodeCopier (or subtype) can get control and
        // prevent the body being overwritten with a metadata method body (i.e., Operations only,
        // not Code Model).
        methodDefinition.Body = this.Substitute(methodDefinition.Body);
      return methodDefinition;
    }

    /// <summary>
    /// Visits the specified named argument.
    /// </summary>
    /// <param name="namedArgument">The named argument.</param>
    /// <returns></returns>
    protected virtual MetadataNamedArgument DeepCopy(MetadataNamedArgument namedArgument) {
      namedArgument.ArgumentValue = this.DeepCopy(namedArgument.ArgumentValue);
      namedArgument.Locations = this.DeepCopy(namedArgument.Locations);
      namedArgument.Type = this.DeepCopy(namedArgument.Type);
      return namedArgument;
    }

    /// <summary>
    /// Visits the specified type of.
    /// </summary>
    /// <param name="typeOf">The type of.</param>
    /// <returns></returns>
    protected virtual MetadataTypeOf DeepCopy(MetadataTypeOf typeOf) {
      typeOf.Locations = this.DeepCopy(typeOf.Locations);
      typeOf.Type = this.DeepCopy(typeOf.Type);
      typeOf.TypeToGet = this.DeepCopy(typeOf.TypeToGet);
      return typeOf;
    }

    /// <summary>
    /// Visits the specified matrix type reference.
    /// </summary>
    /// <param name="matrixTypeReference">The matrix type reference.</param>
    /// <returns></returns>
    protected virtual MatrixTypeReference DeepCopy(MatrixTypeReference matrixTypeReference) {
      this.DeepCopy((TypeReference)matrixTypeReference);
      matrixTypeReference.ElementType = this.DeepCopy(matrixTypeReference.ElementType);
      return matrixTypeReference;
    }

    /// <summary>
    /// Visits the specified method body.
    /// </summary>
    /// <param name="methodBody">The method body.</param>
    /// <returns></returns>
    protected virtual MethodBody DeepCopy(MethodBody methodBody) {
      methodBody.MethodDefinition = this.GetMutableCopyIfItExists(methodBody.MethodDefinition);
      methodBody.LocalVariables = this.DeepCopy(methodBody.LocalVariables);
      methodBody.Operations = this.DeepCopy(methodBody.Operations);
      methodBody.OperationExceptionInformation = this.DeepCopy(methodBody.OperationExceptionInformation);
      return methodBody;
    }

    /// <summary>
    /// Visits the specified method implementation.
    /// </summary>
    /// <param name="methodImplementation">The method implementation.</param>
    /// <returns></returns>
    protected virtual MethodImplementation DeepCopy(MethodImplementation methodImplementation) {
      methodImplementation.ContainingType = this.GetMutableCopyIfItExists(methodImplementation.ContainingType);
      methodImplementation.ImplementedMethod = this.DeepCopy(methodImplementation.ImplementedMethod);
      methodImplementation.ImplementingMethod = this.DeepCopy(methodImplementation.ImplementingMethod);
      return methodImplementation;
    }

    /// <summary>
    /// Current method reference being visited. 
    /// </summary>
    protected IMethodReference currentMethodReference;
    /// <summary>
    /// Visits the specified method reference.
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns></returns>
    protected virtual MethodReference DeepCopy(MethodReference methodReference) {
      IMethodReference savedMethodReference = this.currentMethodReference;
      this.currentMethodReference = methodReference;
      try {
        methodReference.Attributes = this.DeepCopy(methodReference.Attributes);
        methodReference.ContainingType = this.DeepCopy(methodReference.ContainingType);
        methodReference.ExtraParameters = this.DeepCopy(methodReference.ExtraParameters);
        methodReference.Locations = this.DeepCopy(methodReference.Locations);
        methodReference.Parameters = this.DeepCopy(methodReference.Parameters);
        if (methodReference.ReturnValueIsModified)
          methodReference.ReturnValueCustomModifiers = this.DeepCopy(methodReference.ReturnValueCustomModifiers);
        methodReference.Type = this.DeepCopy(methodReference.Type);
      } finally {
        this.currentMethodReference = savedMethodReference;
      }
      return methodReference;
    }

    /// <summary>
    /// Visits the specified modified type reference.
    /// </summary>
    /// <param name="modifiedTypeReference">The modified type reference.</param>
    /// <returns></returns>
    protected virtual ModifiedTypeReference DeepCopy(ModifiedTypeReference modifiedTypeReference) {
      this.DeepCopy((TypeReference)modifiedTypeReference);
      modifiedTypeReference.CustomModifiers = this.DeepCopy(modifiedTypeReference.CustomModifiers);
      modifiedTypeReference.UnmodifiedType = this.DeepCopy(modifiedTypeReference.UnmodifiedType);
      return modifiedTypeReference;
    }

    /// <summary>
    /// Visits the specified module.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <returns></returns>
    protected virtual Module DeepCopy(Module module) {
      module.AssemblyReferences = this.DeepCopy(module.AssemblyReferences);
      module.Locations = this.DeepCopy(module.Locations);
      module.ModuleAttributes = this.DeepCopy(module.ModuleAttributes);
      module.ModuleReferences = this.DeepCopy(module.ModuleReferences);
      module.Win32Resources = this.DeepCopy(module.Win32Resources);
      module.UnitNamespaceRoot = this.DeepCopy(this.GetMutableShallowCopy((IRootUnitNamespace)module.UnitNamespaceRoot));
      // TODO: find a way to populate AllTypes[0] and AllTypes[1] in CollectAndShallowCopyDefinitions. 
      if (module.AllTypes.Count > 0)
        this.DeepCopy(this.GetMutableShallowCopy((INamespaceTypeDefinition)module.AllTypes[0]));
      if (module.AllTypes.Count > 1) {
        INamespaceTypeDefinition globalsType = module.AllTypes[1] as INamespaceTypeDefinition;
        if (globalsType != null && globalsType.Name.Value == "__Globals__")
          this.DeepCopy(this.GetMutableShallowCopy(globalsType));
      }
      if (!(module.EntryPoint is Dummy))
        module.EntryPoint = this.DeepCopy(module.EntryPoint);
      this.VisitPrivateHelperMembers(this.flatListOfTypes);
      this.flatListOfTypes.Sort(new TypeOrderPreserver(module.AllTypes));
      module.AllTypes = this.flatListOfTypes;
      this.flatListOfTypes = new List<INamedTypeDefinition>();
      module.TypeMemberReferences = null;
      module.TypeReferences = null;
      return module;
    }

    /// <summary>
    /// Visits the specified module reference.
    /// </summary>
    /// <param name="moduleReference">The module reference.</param>
    /// <returns></returns>
    protected virtual ModuleReference DeepCopy(ModuleReference moduleReference) {
      if (!(moduleReference.ResolvedModule is Dummy)) {
        object/*?*/ mutatedResolvedModule = null;
        if (this.cache.TryGetValue(moduleReference.ResolvedModule, out mutatedResolvedModule))
          moduleReference.ResolvedModule = (IModule)mutatedResolvedModule;
      }
      moduleReference.Host = this.host;
      return moduleReference;
    }

    /// <summary>
    /// Visits the specified namespace alias for type.
    /// </summary>
    /// <param name="namespaceAliasForType">Type of the namespace alias for.</param>
    /// <returns></returns>
    protected virtual NamespaceAliasForType DeepCopy(NamespaceAliasForType namespaceAliasForType) {
      namespaceAliasForType.AliasedType = (INamedTypeReference)this.DeepCopy(namespaceAliasForType.AliasedType);
      namespaceAliasForType.Attributes = this.DeepCopy(namespaceAliasForType.Attributes);
      namespaceAliasForType.Locations = this.DeepCopy(namespaceAliasForType.Locations);
      namespaceAliasForType.Members = this.DeepCopy(namespaceAliasForType.Members);
      return namespaceAliasForType;
    }

    /// <summary>
    /// Visits the specified namespace type definition.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The namespace type definition.</param>
    /// <returns></returns>
    protected virtual NamespaceTypeDefinition DeepCopy(NamespaceTypeDefinition namespaceTypeDefinition) {
      this.DeepCopy((NamedTypeDefinition)namespaceTypeDefinition);
      namespaceTypeDefinition.ContainingUnitNamespace = this.GetMutableCopyIfItExists(namespaceTypeDefinition.ContainingUnitNamespace);
      return namespaceTypeDefinition;
    }

    /// <summary>
    /// Visits the specified namespace type reference.
    /// </summary>
    /// <param name="namespaceTypeReference">The namespace type reference.</param>
    /// <returns></returns>
    protected virtual NamespaceTypeReference DeepCopy(NamespaceTypeReference namespaceTypeReference) {
      this.DeepCopy((TypeReference)namespaceTypeReference);
      namespaceTypeReference.ContainingUnitNamespace = this.DeepCopy(namespaceTypeReference.ContainingUnitNamespace);
      return namespaceTypeReference;
    }

    /// <summary>
    /// Visits the specified nested alias for type.
    /// </summary>
    /// <param name="nestedAliasForType">Type of the nested alias for.</param>
    /// <returns></returns>
    protected virtual NestedAliasForType DeepCopy(NestedAliasForType nestedAliasForType) {
      nestedAliasForType.AliasedType = (INamedTypeReference)this.DeepCopy(nestedAliasForType.AliasedType);
      nestedAliasForType.Attributes = this.DeepCopy(nestedAliasForType.Attributes);
      nestedAliasForType.Locations = this.DeepCopy(nestedAliasForType.Locations);
      nestedAliasForType.ContainingAlias = this.GetMutableShallowCopy(nestedAliasForType.ContainingAlias);
      return nestedAliasForType;
    }

    /// <summary>
    /// Visits the specified operation exception information.
    /// </summary>
    /// <param name="operationExceptionInformation">The operation exception information.</param>
    /// <returns></returns>
    protected virtual OperationExceptionInformation DeepCopy(OperationExceptionInformation operationExceptionInformation) {
      operationExceptionInformation.ExceptionType = this.DeepCopy(operationExceptionInformation.ExceptionType);
      return operationExceptionInformation;
    }

    /// <summary>
    /// Visits the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <returns></returns>
    protected virtual Operation DeepCopy(Operation operation) {
      ITypeReference/*?*/ typeReference = operation.Value as ITypeReference;
      if (typeReference != null)
        operation.Value = this.DeepCopy(typeReference);
      else {
        IFieldReference/*?*/ fieldReference = operation.Value as IFieldReference;
        if (fieldReference != null)
          operation.Value = this.DeepCopy(fieldReference);
        else {
          IMethodReference/*?*/ methodReference = operation.Value as IMethodReference;
          if (methodReference != null)
            operation.Value = this.DeepCopy(methodReference);
          else {
            IParameterDefinition/*?*/ parameterDefinition = operation.Value as IParameterDefinition;
            if (parameterDefinition != null)
              operation.Value = this.GetMutableCopyIfItExists(parameterDefinition);
            else {
              ILocalDefinition/*?*/ localDefinition = operation.Value as ILocalDefinition;
              if (localDefinition != null)
                operation.Value = this.GetMutableCopyIfItExists(localDefinition);
            }
          }
        }
      }
      return operation;
    }

    /// <summary>
    /// Visits the specified nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition">The nested type definition.</param>
    /// <returns></returns>
    protected virtual NestedTypeDefinition DeepCopy(NestedTypeDefinition nestedTypeDefinition) {
      this.DeepCopy((NamedTypeDefinition)nestedTypeDefinition);
      nestedTypeDefinition.ContainingTypeDefinition = this.GetMutableCopyIfItExists(nestedTypeDefinition.ContainingTypeDefinition);
      return nestedTypeDefinition;
    }

    /// <summary>
    /// Visits the specified nested type reference.
    /// </summary>
    /// <param name="nestedTypeReference">The nested type reference.</param>
    /// <returns></returns>
    protected virtual NestedTypeReference DeepCopy(NestedTypeReference nestedTypeReference) {
      this.DeepCopy((TypeReference)nestedTypeReference);
      nestedTypeReference.ContainingType = this.DeepCopy(nestedTypeReference.ContainingType);
      return nestedTypeReference;
    }

    /// <summary>
    /// Visits the specified specialized field reference.
    /// </summary>
    /// <param name="specializedFieldReference">The specialized field reference.</param>
    /// <returns></returns>
    protected virtual SpecializedFieldReference DeepCopy(SpecializedFieldReference specializedFieldReference) {
      this.DeepCopy((FieldReference)specializedFieldReference);
      specializedFieldReference.UnspecializedVersion = this.DeepCopy(specializedFieldReference.UnspecializedVersion);
      return specializedFieldReference;
    }

    /// <summary>
    /// Visits the specified specialized method reference.
    /// </summary>
    /// <param name="specializedMethodReference">The specialized method reference.</param>
    /// <returns></returns>
    protected virtual SpecializedMethodReference DeepCopy(SpecializedMethodReference specializedMethodReference) {
      this.DeepCopy((MethodReference)specializedMethodReference);
      specializedMethodReference.UnspecializedVersion = this.DeepCopy(specializedMethodReference.UnspecializedVersion);
      return specializedMethodReference;
    }

    /// <summary>
    /// Visits the specified specialized nested type reference.
    /// </summary>
    /// <param name="specializedNestedTypeReference">The specialized nested type reference.</param>
    /// <returns></returns>
    protected virtual SpecializedNestedTypeReference DeepCopy(SpecializedNestedTypeReference specializedNestedTypeReference) {
      this.DeepCopy((NestedTypeReference)specializedNestedTypeReference);
      specializedNestedTypeReference.UnspecializedVersion = (INestedTypeReference)this.DeepCopy(specializedNestedTypeReference.UnspecializedVersion);
      return specializedNestedTypeReference;
    }

    /// <summary>
    /// Replaces the child nodes of the given mutable type definition with the results of running the mutator over them. 
    /// Note that when overriding this method, care must be taken to add the given mutable type definition to this.flatListOfTypes.
    /// </summary>
    /// <param name="typeDefinition">A mutable type definition.</param>
    protected virtual void DeepCopy(NamedTypeDefinition typeDefinition) {
      this.flatListOfTypes.Add(typeDefinition);
      typeDefinition.Attributes = this.DeepCopy(typeDefinition.Attributes);
      typeDefinition.BaseClasses = this.DeepCopy(typeDefinition.BaseClasses);
      typeDefinition.ExplicitImplementationOverrides = this.DeepCopy(typeDefinition.ExplicitImplementationOverrides);
      typeDefinition.GenericParameters = this.DeepCopy(typeDefinition.GenericParameters);
      typeDefinition.Interfaces = this.DeepCopy(typeDefinition.Interfaces);
      typeDefinition.Locations = this.DeepCopy(typeDefinition.Locations);
      typeDefinition.Events = this.DeepCopy(typeDefinition.Events);
      typeDefinition.Fields = this.DeepCopy(typeDefinition.Fields);
      typeDefinition.Methods = this.DeepCopy(typeDefinition.Methods);
      typeDefinition.NestedTypes = this.DeepCopy(typeDefinition.NestedTypes);
      typeDefinition.Properties = this.DeepCopy(typeDefinition.Properties);
      if (typeDefinition.HasDeclarativeSecurity)
        typeDefinition.SecurityAttributes = this.DeepCopy(typeDefinition.SecurityAttributes);
      if (typeDefinition.IsEnum)
        typeDefinition.UnderlyingType = this.DeepCopy(typeDefinition.UnderlyingType);
    }

    // TODO: maybe change to alphabetical order later. 

    /// <summary>
    /// Visits the specified type references.
    /// </summary>
    /// <param name="typeReferences">The type references.</param>
    /// <returns></returns>
    protected virtual List<ITypeReference>/*?*/ DeepCopy(List<ITypeReference>/*?*/ typeReferences) {
      if (typeReferences == null) return null;
      for (int i = 0, n = typeReferences.Count; i < n; i++)
        typeReferences[i] = this.DeepCopy(typeReferences[i]);
      return typeReferences;
    }

    /// <summary>
    /// Visits the specified pointer type reference.
    /// </summary>
    /// <param name="pointerTypeReference">The pointer type reference.</param>
    /// <returns></returns>
    /// <remarks>
    /// Pointer types are not nominal types, so always visit the reference, even if
    /// it is a definition.
    /// </remarks>
    protected virtual IPointerTypeReference DeepCopy(IPointerTypeReference pointerTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(pointerTypeReference, out cachedValue)) {
        return (IPointerTypeReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(pointerTypeReference));
    }

    /// <summary>
    /// Visits the specified function pointer type reference.
    /// </summary>
    /// <param name="functionPointerTypeReference">The function pointer type reference.</param>
    /// <returns></returns>
    protected virtual IFunctionPointerTypeReference DeepCopy(IFunctionPointerTypeReference functionPointerTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(functionPointerTypeReference, out cachedValue)) {
        return (IFunctionPointerTypeReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(functionPointerTypeReference));
    }

    /// <summary>
    /// Visits the specified managed pointer type reference.
    /// </summary>
    /// <param name="managedPointerTypeReference">The managed pointer type reference.</param>
    /// <returns></returns>
    /// <remarks>
    /// Managed pointer types are not nominal types, so always visit the reference, even if
    /// it is a definition.
    /// </remarks>
    protected virtual IManagedPointerTypeReference DeepCopy(IManagedPointerTypeReference managedPointerTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(managedPointerTypeReference, out cachedValue)) {
        return (IManagedPointerTypeReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(managedPointerTypeReference));
    }

    /// <summary>
    /// Visits the specified modified type reference.
    /// </summary>
    /// <param name="modifiedTypeReference">The modified type reference.</param>
    /// <returns></returns>
    protected virtual IModifiedTypeReference DeepCopy(IModifiedTypeReference modifiedTypeReference) {
      object cachedValue;
      if (this.cache.TryGetValue(modifiedTypeReference, out cachedValue)) {
        return (IModifiedTypeReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(modifiedTypeReference));
    }

    /// <summary>
    /// Visits the specified module reference.
    /// </summary>
    /// <param name="moduleReference">The module reference.</param>
    /// <returns></returns>
    protected virtual IModuleReference DeepCopy(IModuleReference moduleReference) {
      object cachedValue;
      if (this.cache.TryGetValue(moduleReference, out cachedValue)) {
        return (IModuleReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(moduleReference));
    }

    /// <summary>
    /// Visits the specified namespace type definition.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The namespace type definition.</param>
    /// <returns></returns>
    protected virtual INamespaceTypeDefinition DeepCopy(INamespaceTypeDefinition namespaceTypeDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(namespaceTypeDefinition));
    }

    /// <summary>
    /// Visits the specified nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition">The nested type definition.</param>
    /// <returns></returns>
    protected virtual INestedTypeDefinition DeepCopy(INestedTypeDefinition nestedTypeDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(nestedTypeDefinition));
    }

    /// <summary>
    /// Visits the specified type reference.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns></returns>
    protected virtual ITypeReference DeepCopy(ITypeReference typeReference) {
      INamespaceTypeReference/*?*/ namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference != null)
        return this.DeepCopy(namespaceTypeReference);
      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      if (nestedTypeReference != null)
        return this.DeepCopy(nestedTypeReference);
      IGenericMethodParameterReference/*?*/ genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
      if (genericMethodParameterReference != null)
        return this.DeepCopy(genericMethodParameterReference);
      IArrayTypeReference/*?*/ arrayTypeReference = typeReference as IArrayTypeReference;
      if (arrayTypeReference != null)
        return this.DeepCopy(arrayTypeReference);
      IGenericTypeParameterReference/*?*/ genericTypeParameterReference = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterReference != null)
        return this.DeepCopy(genericTypeParameterReference);
      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null)
        return this.DeepCopy(genericTypeInstanceReference);
      IPointerTypeReference/*?*/ pointerTypeReference = typeReference as IPointerTypeReference;
      if (pointerTypeReference != null)
        return this.DeepCopy(pointerTypeReference);
      IFunctionPointerTypeReference/*?*/ functionPointerTypeReference = typeReference as IFunctionPointerTypeReference;
      if (functionPointerTypeReference != null)
        return this.DeepCopy(functionPointerTypeReference);
      IModifiedTypeReference/*?*/ modifiedTypeReference = typeReference as IModifiedTypeReference;
      if (modifiedTypeReference != null)
        return this.DeepCopy(modifiedTypeReference);
      IManagedPointerTypeReference/*?*/ managedPointerTypeReference = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeReference != null)
        return this.DeepCopy(managedPointerTypeReference);
      //TODO: error
      return typeReference;
    }

    /// <summary>
    /// Visits the specified unit namespace reference.
    /// </summary>
    /// <param name="unitNamespaceReference">The unit namespace reference.</param>
    /// <returns></returns>
    protected virtual IUnitNamespaceReference DeepCopy(IUnitNamespaceReference unitNamespaceReference) {
      object cachedValue;
      if (this.cache.TryGetValue(unitNamespaceReference, out cachedValue)) {
        return (IUnitNamespaceReference)cachedValue;
      }
      IRootUnitNamespaceReference/*?*/ rootUnitNamespaceReference = unitNamespaceReference as IRootUnitNamespaceReference;
      if (rootUnitNamespaceReference != null)
        return this.DeepCopy(rootUnitNamespaceReference);
      INestedUnitNamespaceReference/*?*/ nestedUnitNamespaceReference = unitNamespaceReference as INestedUnitNamespaceReference;
      if (nestedUnitNamespaceReference != null)
        return this.DeepCopy(nestedUnitNamespaceReference);
      //TODO: error
      return unitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified nested unit namespace.
    /// </summary>
    /// <param name="nestedUnitNamespace">The nested unit namespace.</param>
    /// <returns></returns>
    protected virtual INestedUnitNamespace DeepCopy(INestedUnitNamespace nestedUnitNamespace) {
      return this.DeepCopy(this.GetMutableShallowCopy(nestedUnitNamespace));
    }

    /// <summary>
    /// Visits the specified nested unit namespace reference.
    /// </summary>
    /// <param name="nestedUnitNamespaceReference">The nested unit namespace reference.</param>
    /// <returns></returns>
    protected virtual INestedUnitNamespaceReference DeepCopy(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      object cachedValue;
      if (this.cache.TryGetValue(nestedUnitNamespaceReference, out cachedValue)) {
        return (INestedUnitNamespaceReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(nestedUnitNamespaceReference));
    }

    /// <summary>
    /// Visits the specified nested unit namespace.
    /// </summary>
    /// <param name="nestedUnitNamespace">The nested unit namespace.</param>
    /// <returns></returns>
    protected virtual NestedUnitNamespace DeepCopy(NestedUnitNamespace nestedUnitNamespace) {
      this.DeepCopy((UnitNamespace)nestedUnitNamespace);
      nestedUnitNamespace.ContainingUnitNamespace = this.GetMutableCopyIfItExists(nestedUnitNamespace.ContainingUnitNamespace);
      return nestedUnitNamespace;
    }

    /// <summary>
    /// Visits the specified nested unit namespace reference.
    /// </summary>
    /// <param name="nestedUnitNamespaceReference">The nested unit namespace reference.</param>
    /// <returns></returns>
    protected virtual NestedUnitNamespaceReference DeepCopy(NestedUnitNamespaceReference nestedUnitNamespaceReference) {
      this.DeepCopy((UnitNamespaceReference)nestedUnitNamespaceReference);
      nestedUnitNamespaceReference.ContainingUnitNamespace = this.DeepCopy(nestedUnitNamespaceReference.ContainingUnitNamespace);
      return nestedUnitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified unit reference.
    /// </summary>
    /// <param name="unitReference">The unit reference.</param>
    /// <returns></returns>
    protected virtual IUnitReference DeepCopy(IUnitReference unitReference) {
      object cachedValue;
      if (this.cache.TryGetValue(unitReference, out cachedValue)) {
        return (IUnitReference)cachedValue;
      }
      IAssemblyReference/*?*/ assemblyReference = unitReference as IAssemblyReference;
      if (assemblyReference != null)
        return this.DeepCopy(assemblyReference);
      IModuleReference/*?*/ moduleReference = unitReference as IModuleReference;
      if (moduleReference != null)
        return this.DeepCopy(moduleReference);
      //TODO: error
      return unitReference;
    }

    /// <summary>
    /// Visits the specified parameter definitions.
    /// </summary>
    /// <param name="parameterDefinitions">The parameter definitions.</param>
    /// <returns></returns>
    protected virtual List<IParameterDefinition>/*?*/ DeepCopy(List<IParameterDefinition>/*?*/ parameterDefinitions) {
      if (parameterDefinitions == null) return null;
      for (int i = 0, n = parameterDefinitions.Count; i < n; i++)
        parameterDefinitions[i] = this.DeepCopy(this.GetMutableShallowCopy(parameterDefinitions[i]));
      return parameterDefinitions;
    }

    /// <summary>
    /// Visits the specified parameter definition.
    /// </summary>
    /// <param name="parameterDefinition">The parameter definition.</param>
    /// <returns></returns>
    protected virtual ParameterDefinition DeepCopy(ParameterDefinition parameterDefinition) {
      parameterDefinition.Attributes = this.DeepCopy(parameterDefinition.Attributes);
      parameterDefinition.ContainingSignature = this.GetMutableCopyIfItExists(parameterDefinition.ContainingSignature);
      if (parameterDefinition.HasDefaultValue)
        parameterDefinition.DefaultValue = this.DeepCopy(this.GetMutableShallowCopy(parameterDefinition.DefaultValue));
      if (parameterDefinition.IsModified)
        parameterDefinition.CustomModifiers = this.DeepCopy(parameterDefinition.CustomModifiers);
      parameterDefinition.Locations = this.DeepCopy(parameterDefinition.Locations);
      if (parameterDefinition.IsMarshalledExplicitly)
        parameterDefinition.MarshallingInformation = this.DeepCopy(this.GetMutableShallowCopy(parameterDefinition.MarshallingInformation));
      parameterDefinition.Type = this.DeepCopy(parameterDefinition.Type);
      return parameterDefinition;
    }

    /// <summary>
    /// Visits the specified parameter type information list.
    /// </summary>
    /// <param name="parameterTypeInformationList">The parameter type information list.</param>
    /// <returns></returns>
    protected virtual List<IParameterTypeInformation>/*?*/ DeepCopy(List<IParameterTypeInformation>/*?*/ parameterTypeInformationList) {
      if (parameterTypeInformationList == null) return null;
      for (int i = 0, n = parameterTypeInformationList.Count; i < n; i++)
        parameterTypeInformationList[i] = this.DeepCopy(this.GetMutableShallowCopy(parameterTypeInformationList[i]));
      return parameterTypeInformationList;
    }

    /// <summary>
    /// Visits the specified parameter type information.
    /// </summary>
    /// <param name="parameterTypeInformation">The parameter type information.</param>
    /// <returns></returns>
    protected virtual ParameterTypeInformation DeepCopy(ParameterTypeInformation parameterTypeInformation) {
      if (parameterTypeInformation.IsModified)
        parameterTypeInformation.CustomModifiers = this.DeepCopy(parameterTypeInformation.CustomModifiers);
      parameterTypeInformation.Type = this.DeepCopy(parameterTypeInformation.Type);
      return parameterTypeInformation;
    }

    /// <summary>
    /// Visits the specified platform invoke information.
    /// </summary>
    /// <param name="platformInvokeInformation">The platform invoke information.</param>
    /// <returns></returns>
    protected virtual PlatformInvokeInformation DeepCopy(PlatformInvokeInformation platformInvokeInformation) {
      platformInvokeInformation.ImportModule = this.DeepCopy(this.GetMutableShallowCopy(platformInvokeInformation.ImportModule));
      return platformInvokeInformation;
    }

    /// <summary>
    /// Visits the specified property definitions.
    /// </summary>
    /// <param name="propertyDefinitions">The property definitions.</param>
    /// <returns></returns>
    protected virtual List<IPropertyDefinition> DeepCopy(List<IPropertyDefinition> propertyDefinitions) {
      if (propertyDefinitions == null) return null;
      for (int i = 0, n = propertyDefinitions.Count; i < n; i++)
        propertyDefinitions[i] = this.DeepCopy(propertyDefinitions[i]);
      return propertyDefinitions;
    }

    /// <summary>
    /// Visits the specified property definition.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns></returns>
    protected virtual IPropertyDefinition DeepCopy(IPropertyDefinition propertyDefinition) {
      return this.DeepCopy(this.GetMutableShallowCopy(propertyDefinition));
    }

    /// <summary>
    /// Visits the specified property definition.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns></returns>
    protected virtual PropertyDefinition DeepCopy(PropertyDefinition propertyDefinition) {
      this.DeepCopy((TypeDefinitionMember)propertyDefinition);
      int getterIndex = -1, setterIndex = -1;
      if (propertyDefinition.Accessors != null) {
        if (propertyDefinition.Accessors.Count > 0) {
          if (propertyDefinition.Getter == propertyDefinition.Accessors[0]) getterIndex = 0;
          else if (propertyDefinition.Setter == propertyDefinition.Accessors[0]) setterIndex = 0;
        }
        if (propertyDefinition.Accessors.Count > 1) {
          if (propertyDefinition.Getter == propertyDefinition.Accessors[1]) getterIndex = 1;
          else if (propertyDefinition.Setter == propertyDefinition.Accessors[1]) setterIndex = 1;
        }
      }
      propertyDefinition.Accessors = this.DeepCopy(propertyDefinition.Accessors);
      if (propertyDefinition.HasDefaultValue)
        propertyDefinition.DefaultValue = this.DeepCopy(this.GetMutableShallowCopy(propertyDefinition.DefaultValue));
      if (propertyDefinition.Getter != null) {
        if (getterIndex != -1)
          propertyDefinition.Getter = propertyDefinition.Accessors[getterIndex];
        else
          propertyDefinition.Getter = this.DeepCopy(propertyDefinition.Getter);
      }
      propertyDefinition.Parameters = this.DeepCopy(propertyDefinition.Parameters);
      if (propertyDefinition.ReturnValueIsModified)
        propertyDefinition.ReturnValueCustomModifiers = this.DeepCopy(propertyDefinition.ReturnValueCustomModifiers);
      if (propertyDefinition.Setter != null) {
        if (setterIndex != -1)
          propertyDefinition.Setter = propertyDefinition.Accessors[setterIndex];
        else
          propertyDefinition.Setter = this.DeepCopy(propertyDefinition.Setter);
      }
      propertyDefinition.Type = this.DeepCopy(propertyDefinition.Type);
      return propertyDefinition;
    }

    /// <summary>
    /// Visits the specified pointer type reference.
    /// </summary>
    /// <param name="pointerTypeReference">The pointer type reference.</param>
    /// <returns></returns>
    protected virtual PointerTypeReference DeepCopy(PointerTypeReference pointerTypeReference) {
      this.DeepCopy((TypeReference)pointerTypeReference);
      pointerTypeReference.TargetType = this.DeepCopy(pointerTypeReference.TargetType);
      return pointerTypeReference;
    }

    /// <summary>
    /// Visits the specified resource references.
    /// </summary>
    /// <param name="resourceReferences">The resource references.</param>
    /// <returns></returns>
    protected virtual List<IResourceReference> DeepCopy(List<IResourceReference> resourceReferences) {
      if (resourceReferences == null) return null;
      for (int i = 0, n = resourceReferences.Count; i < n; i++)
        resourceReferences[i] = this.DeepCopy(this.GetMutableShallowCopy(resourceReferences[i]));
      return resourceReferences;
    }

    /// <summary>
    /// Visits the specified resource reference.
    /// </summary>
    /// <param name="resourceReference">The resource reference.</param>
    /// <returns></returns>
    protected virtual ResourceReference DeepCopy(ResourceReference resourceReference) {
      resourceReference.Attributes = this.DeepCopy(resourceReference.Attributes);
      resourceReference.DefiningAssembly = this.DeepCopy(resourceReference.DefiningAssembly);
      return resourceReference;
    }

    /// <summary>
    /// Visits the specified security attributes.
    /// </summary>
    /// <param name="securityAttributes">The security attributes.</param>
    /// <returns></returns>
    protected virtual List<ISecurityAttribute> DeepCopy(List<ISecurityAttribute> securityAttributes) {
      if (securityAttributes == null) return null;
      for (int i = 0, n = securityAttributes.Count; i < n; i++)
        securityAttributes[i] = this.DeepCopy(this.GetMutableShallowCopy(securityAttributes[i]));
      return securityAttributes;
    }

    /// <summary>
    /// Visits the specified root unit namespace reference.
    /// </summary>
    /// <param name="rootUnitNamespaceReference">The root unit namespace reference.</param>
    /// <returns></returns>
    protected virtual IRootUnitNamespaceReference DeepCopy(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      //IRootUnitNamespace/*?*/ rootUnitNamespace = rootUnitNamespaceReference as IRootUnitNamespace;
      //if (rootUnitNamespace != null)
      //  return this.GetMutableShallowCopy(rootUnitNamespace);
      object cachedValue;
      if (this.cache.TryGetValue(rootUnitNamespaceReference, out cachedValue)) {
        return (IRootUnitNamespaceReference)cachedValue;
      }
      return this.DeepCopy(this.GetMutableShallowCopy(rootUnitNamespaceReference));
    }

    /// <summary>
    /// Visits the specified root unit namespace.
    /// </summary>
    /// <param name="rootUnitNamespace">The root unit namespace.</param>
    /// <returns></returns>
    protected virtual RootUnitNamespace DeepCopy(RootUnitNamespace rootUnitNamespace) {
      rootUnitNamespace.Unit = this.GetMutableCopyIfItExists(rootUnitNamespace.Unit);
      this.DeepCopy((UnitNamespace)rootUnitNamespace);
      return rootUnitNamespace;
    }

    /// <summary>
    /// Visits the specified root unit namespace reference.
    /// </summary>
    /// <param name="rootUnitNamespaceReference">The root unit namespace reference.</param>
    /// <returns></returns>
    protected virtual RootUnitNamespaceReference DeepCopy(RootUnitNamespaceReference rootUnitNamespaceReference) {
      rootUnitNamespaceReference.Unit = this.DeepCopy(rootUnitNamespaceReference.Unit);
      return rootUnitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified security customAttribute.
    /// </summary>
    /// <param name="securityAttribute">The security customAttribute.</param>
    /// <returns></returns>
    protected virtual SecurityAttribute DeepCopy(SecurityAttribute securityAttribute) {
      securityAttribute.Attributes = this.DeepCopy(securityAttribute.Attributes);
      return securityAttribute;
    }

    /// <summary>
    /// Visits the specified section block.
    /// </summary>
    /// <param name="sectionBlock">The section block.</param>
    /// <returns></returns>
    protected virtual SectionBlock DeepCopy(SectionBlock sectionBlock) {
      return sectionBlock;
    }

    /// <summary>
    /// Visits the specified type definition member.
    /// </summary>
    /// <param name="typeDefinitionMember">The type definition member.</param>
    /// <returns></returns>
    protected virtual ITypeDefinitionMember DeepCopy(TypeDefinitionMember typeDefinitionMember) {
      typeDefinitionMember.Attributes = this.DeepCopy(typeDefinitionMember.Attributes);
      typeDefinitionMember.ContainingTypeDefinition = this.GetMutableCopyIfItExists(typeDefinitionMember.ContainingTypeDefinition);
      typeDefinitionMember.Locations = this.DeepCopy(typeDefinitionMember.Locations);
      return typeDefinitionMember;
    }

    /// <summary>
    /// Visits the specified type reference.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns></returns>
    protected virtual TypeReference DeepCopy(TypeReference typeReference) {
      typeReference.Attributes = this.DeepCopy(typeReference.Attributes);
      typeReference.Locations = this.DeepCopy(typeReference.Locations);
      return typeReference;
    }

    /// <summary>
    /// Visits the specified unit.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <returns></returns>
    protected virtual Unit DeepCopy(Unit unit) {
      unit.Attributes = this.DeepCopy(unit.Attributes);
      unit.Locations = this.DeepCopy(unit.Locations);
      unit.UnitNamespaceRoot = this.DeepCopy(this.GetMutableShallowCopy((IRootUnitNamespace)unit.UnitNamespaceRoot));
      return unit;
    }

    /// <summary>
    /// Visits the specified unit namespace.
    /// </summary>
    /// <param name="unitNamespace">The unit namespace.</param>
    /// <returns></returns>
    protected virtual UnitNamespace DeepCopy(UnitNamespace unitNamespace) {
      unitNamespace.Attributes = this.DeepCopy(unitNamespace.Attributes);
      unitNamespace.Locations = this.DeepCopy(unitNamespace.Locations);
      unitNamespace.Members = this.DeepCopy(unitNamespace.Members);
      return unitNamespace;
    }

    /// <summary>
    /// Visits the specified unit namespace reference.
    /// </summary>
    /// <param name="unitNamespaceReference">The unit namespace reference.</param>
    /// <returns></returns>
    protected virtual UnitNamespaceReference DeepCopy(UnitNamespaceReference unitNamespaceReference) {
      unitNamespaceReference.Attributes = this.DeepCopy(unitNamespaceReference.Attributes);
      unitNamespaceReference.Locations = this.DeepCopy(unitNamespaceReference.Locations);
      return unitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified vector type reference.
    /// </summary>
    /// <param name="vectorTypeReference">The vector type reference.</param>
    /// <returns></returns>
    protected virtual VectorTypeReference DeepCopy(VectorTypeReference vectorTypeReference) {
      this.DeepCopy((TypeReference)vectorTypeReference);
      vectorTypeReference.ElementType = this.DeepCopy(vectorTypeReference.ElementType);
      return vectorTypeReference;
    }

    /// <summary>
    /// Visits the specified win32 resource.
    /// </summary>
    /// <param name="win32Resource">The win32 resource.</param>
    /// <returns></returns>
    protected virtual Win32Resource DeepCopy(Win32Resource win32Resource) {
      return win32Resource;
    }
    #endregion

    #region GetMutableCopyIfItExists

    /// <summary>
    /// Gets the mutable copy if it exists. Use the ifItExists method a subnode contains (points to) a parent node that 
    /// is a definition, or a local or a property definition is used in the code. 
    /// </summary>
    /// <param name="localDefinition">The local definition.</param>
    /// <returns></returns>
    protected virtual ILocalDefinition GetMutableCopyIfItExists(ILocalDefinition localDefinition) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(localDefinition, out cachedValue);
      var result = cachedValue as ILocalDefinition;
      if (result == null) result = localDefinition;
      return result;
    }

    /// <summary>
    /// Get a mutable copy of a method definition if it exists. Use the ifItExists method a subnode contains (points to) a parent node that 
    /// is a definition, or a local or a property definition is used in the code. 
    /// </summary>
    /// <param name="methodDefinition"></param>
    /// <returns></returns>
    protected virtual IMethodDefinition GetMutableCopyIfItExists(IMethodDefinition methodDefinition) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(methodDefinition, out cachedValue);
      var result = cachedValue as IMethodDefinition;
      if (result == null) result = methodDefinition;
      return result;
    }

    /// <summary>
    /// Get a mutable copy of a namespace definition. Use the ifItExists method a subnode contains (points to) a parent node that 
    /// is a definition, or a local or a property definition is used in the code. 
    /// </summary>
    /// <param name="namespaceDefinition"></param>
    /// <returns></returns>
    protected virtual INamespaceDefinition GetMutableCopyIfItExists(INamespaceDefinition namespaceDefinition) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(namespaceDefinition, out cachedValue);
      var result = cachedValue as INamespaceDefinition;
      if (result == null) result = namespaceDefinition;
      return result;
    }

    /// <summary>
    /// Gets the mutable copy of a parameter definition if it exists. Use the ifItExists method a subnode contains (points to) a parent node that 
    /// is a definition, or a local or a property definition is used in the code. 
    /// </summary>
    /// <param name="parameterDefinition">The parameter definition.</param>
    /// <returns></returns>
    protected virtual IParameterDefinition GetMutableCopyIfItExists(IParameterDefinition parameterDefinition) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(parameterDefinition, out cachedValue);
      var result = cachedValue as IParameterDefinition;
      if (result == null) result = parameterDefinition;
      return result;
    }

    /// <summary>
    /// Gets the mutable copy  of a signature. if it exists. Use the ifItExists method a subnode contains (points to) a parent node that 
    /// is a definition, or a local or a property definition is used in the code. 
    /// </summary>
    ///<param name="signature"></param>
    /// <returns></returns>
    protected virtual ISignature GetMutableCopyIfItExists(ISignature signature) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(signature, out cachedValue);
      var result = cachedValue as ISignature;
      if (result == null) result = signature;
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeDefinition"></param>
    /// <returns></returns>
    protected ITypeDefinition GetMutableCopyIfItExists(ITypeDefinition typeDefinition) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(typeDefinition, out cachedValue);
      var result = cachedValue as ITypeDefinition;
      if (result == null) result = typeDefinition;
      return result;
    }

    /// <summary>
    /// Gets the mutable copy of a type definition. if it exists. Use the ifItExists method a subnode contains (points to) a parent node that 
    /// is a definition, or a local or a property definition is used in the code. 
    /// </summary>
    /// <param name="unitNamespace"></param>
    /// <returns></returns>
    protected virtual IUnit GetMutableCopyIfItExists(IUnit unitNamespace) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(unitNamespace, out cachedValue);
      var result = cachedValue as IUnit;
      if (result == null) result = unitNamespace;
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitNamespace"></param>
    /// <returns></returns>
    protected virtual IUnitNamespace GetMutableCopyIfItExists(IUnitNamespace unitNamespace) {
      object/*?*/ cachedValue;
      this.cache.TryGetValue(unitNamespace, out cachedValue);
      var result = cachedValue as IUnitNamespace;
      if (result == null) result = unitNamespace;
      return result;
    }

    #endregion GetMutableCopyIfItExists

    class TypeOrderPreserver : Comparer<INamedTypeDefinition> {

      Dictionary<string, int> oldOrder = new Dictionary<string, int>();

      internal TypeOrderPreserver(List<INamedTypeDefinition> oldTypeList) {
        for (int i = 0, n = oldTypeList.Count; i < n; i++)
          this.oldOrder.Add(TypeHelper.GetTypeName(oldTypeList[i], NameFormattingOptions.TypeParameters), i);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public override int Compare(INamedTypeDefinition x, INamedTypeDefinition y) {
        int xi = 0;
        int yi = int.MaxValue;
        string xn = TypeHelper.GetTypeName(x, NameFormattingOptions.TypeParameters);
        string yn = TypeHelper.GetTypeName(y, NameFormattingOptions.TypeParameters);
        if (!this.oldOrder.TryGetValue(xn, out xi)) xi = int.MaxValue;
        if (!this.oldOrder.TryGetValue(yn, out yi)) yi = int.MaxValue;
        return xi - yi;
      }
    }

    /// <summary>
    /// Visits the method return value attributes.
    /// </summary>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns></returns>
    protected List<ICustomAttribute> DeepCopyMethodReturnValueAttributes(List<ICustomAttribute> customAttributes) {
      return this.DeepCopy(customAttributes);
    }

    /// <summary>
    /// Visits the method return value custom modifiers.
    /// </summary>
    /// <param name="customModifers">The custom modifers.</param>
    /// <returns></returns>
    protected List<ICustomModifier> DeepCopyMethodReturnValueCustomModifiers(List<ICustomModifier> customModifers) {
      return this.DeepCopy(customModifers);
    }

    /// <summary>
    /// Visits the method return value marshalling information.
    /// </summary>
    /// <param name="marshallingInformation">The marshalling information.</param>
    /// <returns></returns>
    protected IMarshallingInformation DeepCopyMethodReturnValueMarshallingInformation(MarshallingInformation marshallingInformation) {
      return this.DeepCopy(marshallingInformation);
    }

    #region Public copy methods

    /// <summary>
    /// Returns a mutable deep copy of the given assembly.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references. For the purposes of this call, the
    /// table for interning is what is needed.</param>
    /// <param name="assembly">The assembly to copied.</param>
    public static Assembly DeepCopy(IMetadataHost host, IAssembly assembly) {
      List<INamedTypeDefinition> newTypes;
      return (Assembly)new MetadataCopier(host, assembly, out newTypes).Substitute(assembly);
    }

    /// <summary>
    /// Returns a mutable deep copy of the given method reference.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references. For the purposes of this call, the
    /// table for interning is what is needed.</param>
    /// <param name="methodReference">The method reference to copied.</param>
    public static MethodReference DeepCopy(IMetadataHost host, IMethodReference methodReference) {
      return (MethodReference)new MetadataCopier(host).Substitute(methodReference);
    }

    /// <summary>
    /// Returns a mutable deep copy of the given module.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references. For the purposes of this call, the
    /// table for interning is what is needed.</param>
    /// <param name="module">The module to copied.</param>
    public static Module DeepCopy(IMetadataHost host, IModule module) {
      List<INamedTypeDefinition> newTypes;
      return (Module)new MetadataCopier(host, module, out newTypes).Substitute(module);
    }

    /// <summary>
    /// Returns a mutable deep copy of the given type reference.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references. For the purposes of this call, the
    /// table for interning is what is needed.</param>
    /// <param name="typeReference">The type reference to copied.</param>
    public static TypeReference DeepCopy(IMetadataHost host, ITypeReference typeReference) {
      return (TypeReference)new MetadataCopier(host).Substitute(typeReference);
    }

    /// <summary>
    /// Makes a deep copy of the specified alias for type.
    /// </summary>
    public virtual IAliasForType Substitute(IAliasForType aliasForType) {
      this.coneAlreadyFixed = true;
      INamespaceAliasForType/*?*/ namespaceAliasForType = aliasForType as INamespaceAliasForType;
      if (namespaceAliasForType != null) return this.DeepCopy(this.GetMutableShallowCopy(namespaceAliasForType));
      INestedAliasForType/*?*/ nestedAliasForType = aliasForType as INestedAliasForType;
      if (nestedAliasForType != null) return this.DeepCopy(this.GetMutableShallowCopy(nestedAliasForType));
      throw new InvalidOperationException();
    }

    /// <summary>
    /// Makes a deep copy of the specified array type reference.
    /// </summary>
    public virtual IArrayTypeReference Substitute(IArrayTypeReference arrayTypeReference) {
      this.coneAlreadyFixed = true;
      if (arrayTypeReference.IsVector)
        return this.DeepCopy(this.GetMutableVectorShallowCopy(arrayTypeReference));
      else
        return this.DeepCopy(this.GetMutableMatrixShallowCopy(arrayTypeReference));
    }

    /// <summary>
    /// True iff all the def nodes in the cone have been collected. After this flag is set to 
    /// true, future call to AddDefinition will raise an ApplicationException. 
    /// </summary>
    protected bool coneAlreadyFixed;

    /// <summary>
    /// Assembly is the root of the cone. collect all sub def-nodes. 
    /// </summary>
    /// <param name="assembly"></param>
    private void AddDefinition(IAssembly assembly) {
      if (this.coneAlreadyFixed) {
        throw new ApplicationException("Cone already fixed.");
      }
      var copy = new Assembly();
      this.cache.Add(assembly, copy);
      this.cache.Add(copy, copy);
      copy.Copy(assembly, this.host.InternFactory);
      // Globals and VCC support, not reachable from C# compiler generated assemblies. 
      if (copy.AllTypes.Count > 0) {
        this.definitionCollector.Traverse(copy.AllTypes[0]);
      }
      if (copy.AllTypes.Count > 1) {
        var globals = copy.AllTypes[1];
        if (globals != null && globals.Name.Value == "__Globals__") {
          this.definitionCollector.Traverse(globals);
        }
      }
      this.definitionCollector.Traverse(assembly);
    }

    /// <summary>
    /// Makes a deep copy of the specified assembly.
    /// </summary>
    public virtual IAssembly Substitute(IAssembly assembly) {
      //^ requires this.cache.ContainsKey(assembly);
      //^ requires this.cache[assembly] is Assembly;
      this.coneAlreadyFixed = true;
      var copy = (Assembly)this.cache[assembly];
      this.DeepCopy(copy);
      return copy;
    }

    /// <summary>
    /// Makes a deep copy of the specified assembly reference.
    /// </summary>
    public virtual IAssemblyReference Substitute(IAssemblyReference assemblyReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(assemblyReference);
    }

    /// <summary>
    /// Makes a deep copy of the specified custom attribute.
    /// </summary>
    public virtual ICustomAttribute Substitute(ICustomAttribute customAttribute) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(customAttribute));
    }

    /// <summary>
    /// Makes a deep copy of the specified custom modifier.
    /// </summary>
    public virtual ICustomModifier Substitute(ICustomModifier customModifier) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(customModifier));
    }

    /// <summary>
    /// Makes a deep copy of the specified event.
    /// </summary>
    public virtual IEventDefinition Substitute(IEventDefinition eventDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(eventDefinition));
    }


    /// <summary>
    /// Substitute a definition inside the cone. 
    /// </summary>
    /// <param name="fieldDefinition"></param>
    /// <returns></returns>
    public virtual IFieldDefinition Substitute(IFieldDefinition fieldDefinition) {
      // ^ requires !(methodDefinition is ISpecializedFieldDefinition);
      this.coneAlreadyFixed = true;
      IGlobalFieldDefinition globalFieldDefinition = fieldDefinition as IGlobalFieldDefinition;
      if (globalFieldDefinition != null)
        return this.DeepCopy(this.GetMutableShallowCopy(globalFieldDefinition));
      return this.DeepCopy(this.GetMutableShallowCopy(fieldDefinition));
    }

    /// <summary>
    /// Substitute a field reference according to its kind. 
    /// </summary>
    /// <param name="fieldReference"></param>
    /// <returns></returns>
    public virtual IFieldReference Substitute(IFieldReference fieldReference) {
      this.coneAlreadyFixed = true;
      ISpecializedFieldReference specializedFieldReference = fieldReference as ISpecializedFieldReference;
      if (specializedFieldReference != null) {
        return this.DeepCopy(specializedFieldReference);
      }
      return this.DeepCopy(fieldReference);
    }

    /// <summary>
    /// Substitute a file reference.
    /// </summary>
    /// <param name="fileReference"></param>
    /// <returns></returns>
    public virtual IFileReference Substitute(IFileReference fileReference) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Substitute a function pointer type reference. 
    /// </summary>
    /// <param name="functionPointerTypeReference"></param>
    /// <returns></returns>
    public virtual IFunctionPointerTypeReference Substitute(IFunctionPointerTypeReference functionPointerTypeReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(functionPointerTypeReference);
    }

    /// <summary>
    /// Substitute a generic method instance reference. 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <returns></returns>
    public virtual IGenericMethodInstanceReference Substitute(IGenericMethodInstanceReference genericMethodInstanceReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(genericMethodInstanceReference);
    }

    /// <summary>
    /// Substitute a generic method parameter. 
    /// </summary>
    /// <param name="genericMethodParameter"></param>
    /// <returns></returns>
    public virtual IGenericMethodParameter Substitute(IGenericMethodParameter genericMethodParameter) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(genericMethodParameter));
    }

    /// <summary>
    /// Substitute a generic method parameter reference. 
    /// </summary>
    /// <param name="genericMethodParameterReference"></param>
    /// <returns></returns>
    public virtual IGenericMethodParameterReference Substitute(IGenericMethodParameterReference genericMethodParameterReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(genericMethodParameterReference);
    }

    /// <summary>
    /// Substitute a global field defintion. 
    /// </summary>
    /// <param name="globalFieldDefinition"></param>
    /// <returns></returns>
    public virtual IGlobalFieldDefinition Substitute(IGlobalFieldDefinition globalFieldDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(globalFieldDefinition));
    }

    /// <summary>
    /// Substitute a global method definition. 
    /// </summary>
    /// <param name="globalMethodDefinition"></param>
    /// <returns></returns>
    public virtual IGlobalMethodDefinition Substitute(IGlobalMethodDefinition globalMethodDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(globalMethodDefinition));
    }

    /// <summary>
    /// Substitute a generic type instance reference. 
    /// </summary>
    /// <param name="genericTypeInstanceReference"></param>
    /// <returns></returns>
    public virtual IGenericTypeInstanceReference Substitute(IGenericTypeInstanceReference genericTypeInstanceReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(genericTypeInstanceReference);
    }

    /// <summary>
    /// Substitute a generic type parameter. 
    /// </summary>
    /// <param name="genericTypeParameter"></param>
    /// <returns></returns>
    public virtual IGenericTypeParameter Substitute(IGenericTypeParameter genericTypeParameter) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(genericTypeParameter));
    }

    /// <summary>
    /// Substitute a generic type parameter reference. 
    /// </summary>
    /// <param name="genericTypeParameterReference"></param>
    /// <returns></returns>
    public virtual IGenericTypeParameterReference Substitute(IGenericTypeParameterReference genericTypeParameterReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(genericTypeParameterReference);
    }

    /// <summary>
    /// Substitute a managed pointer type reference.
    /// </summary>
    /// <param name="managedPointerTypeReference"></param>
    /// <returns></returns>
    public virtual IManagedPointerTypeReference Substitute(IManagedPointerTypeReference managedPointerTypeReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(managedPointerTypeReference));
    }

    /// <summary>
    /// Substitute a marshalling information object. 
    /// </summary>
    /// <param name="marshallingInformation"></param>
    /// <returns></returns>
    public virtual IMarshallingInformation Substitute(IMarshallingInformation marshallingInformation) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(marshallingInformation));
    }

    /// <summary>
    /// Substitute a metadata constant. 
    /// </summary>
    /// <param name="constant"></param>
    /// <returns></returns>
    public virtual IMetadataConstant Substitute(IMetadataConstant constant) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(constant));
    }

    /// <summary>
    /// Substitute a metadata create array. 
    /// </summary>
    /// <param name="createArray"></param>
    /// <returns></returns>
    public virtual IMetadataCreateArray Substitute(IMetadataCreateArray createArray) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(createArray));
    }

    /// <summary>
    /// Substitute a named argument. 
    /// </summary>
    /// <param name="namedArgument"></param>
    /// <returns></returns>
    public virtual IMetadataNamedArgument Substitute(IMetadataNamedArgument namedArgument) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(namedArgument));
    }

    /// <summary>
    /// Substitute a meta data type of node. 
    /// </summary>
    /// <param name="typeOf"></param>
    /// <returns></returns>
    public virtual IMetadataTypeOf Substitute(IMetadataTypeOf typeOf) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(typeOf));
    }

    /// <summary>
    /// Substitute a method body.
    /// </summary>
    /// <param name="methodBody"></param>
    /// <returns></returns>
    public virtual IMethodBody Substitute(IMethodBody methodBody) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(methodBody));
    }

    /// <summary>
    /// Substitute a method definition. 
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public virtual IMethodDefinition Substitute(IMethodDefinition method) {
      //^ requires !(method is ISpecializedMethodDefinition);
      this.coneAlreadyFixed = true;
      IGlobalMethodDefinition globalMethodDefinition = method as IGlobalMethodDefinition;
      if (globalMethodDefinition != null)
        return this.DeepCopy(this.GetMutableShallowCopy(globalMethodDefinition));
      return this.DeepCopy(this.GetMutableShallowCopy(method));
    }

    /// <summary>
    /// Substitute a method implementation. 
    /// </summary>
    /// <param name="methodImplementation"></param>
    /// <returns></returns>
    public virtual IMethodImplementation Substitute(IMethodImplementation methodImplementation) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(methodImplementation));
    }

    /// <summary>
    /// Substitute a method reference. 
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    public virtual IMethodReference Substitute(IMethodReference methodReference) {
      this.coneAlreadyFixed = true;
      ISpecializedMethodReference specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null) {
        return this.DeepCopy(specializedMethodReference);
      }
      IGenericMethodInstanceReference genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null) {
        return this.DeepCopy(genericMethodInstanceReference);
      }
      return this.DeepCopy(methodReference);
    }

    /// <summary>
    /// Substitute a modified type reference. 
    /// </summary>
    /// <param name="modifiedTypeReference"></param>
    /// <returns></returns>
    public virtual IModifiedTypeReference Substitute(IModifiedTypeReference modifiedTypeReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(modifiedTypeReference));
    }

    /// <summary>
    /// Add sub def-nodes of module to the cone. 
    /// </summary>
    /// <param name="module"></param>
    private void AddDefinition(IModule module) {
      if (this.coneAlreadyFixed) {
        throw new ApplicationException("cone has already fixed.");
      }
      IAssembly assembly = module as IAssembly;
      if (assembly != null) {
        var copy = new Assembly();
        this.cache.Add(assembly, copy);
        this.cache.Add(copy, copy);
        copy.Copy(assembly, this.host.InternFactory);
        this.definitionCollector.Traverse(assembly);
        // Globals and VCC support, not reachable from C# compiler generated assemblies. 
        if (((Module)copy).AllTypes.Count > 0) {
          this.definitionCollector.Traverse(((Module)copy).AllTypes[0]);
        }
        if (((Module)copy).AllTypes.Count > 1) {
          var globals = ((Module)copy).AllTypes[1];
          if (globals != null && globals.Name.Value == "__Globals__") {
            this.definitionCollector.Traverse(globals);
          }
        }
      } else {
        var copy = new Module();
        this.cache.Add(module, copy);
        this.cache.Add(copy, copy);
        copy.Copy(module, this.host.InternFactory);
        this.definitionCollector.Traverse(module);
        // Globals and VCC support, not reachable from C# compiler generated assemblies. 
        if (copy.AllTypes.Count > 0) {
          this.definitionCollector.Traverse(copy.AllTypes[0]);
        }
        if (copy.AllTypes.Count > 1) {
          var globals = copy.AllTypes[1];
          if (globals != null && globals.Name.Value == "__Globals__") {
            this.definitionCollector.Traverse(globals);
          }
        }
      }
    }

    /// <summary>
    /// Substitution over a module. 
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public virtual IModule Substitute(IModule module) {
      //^ requires this.cache.ContainsKey(module);
      //^ requires this.cache[module] is Module;
      var assembly = module as IAssembly;
      if (assembly != null) return this.Substitute(assembly);
      this.coneAlreadyFixed = true;
      Module copy = (Module)this.cache[module];
      return this.DeepCopy(copy);
    }

    /// <summary>
    /// Substitute a module reference. 
    /// </summary>
    /// <param name="moduleReference"></param>
    /// <returns></returns>
    public virtual IModuleReference Substitute(IModuleReference moduleReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(moduleReference);
    }

    /// <summary>
    /// Subsitute a namesapce alias for type. 
    /// </summary>
    /// <param name="namespaceAliasForType"></param>
    /// <returns></returns>
    public virtual INamespaceAliasForType Substitute(INamespaceAliasForType namespaceAliasForType) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(namespaceAliasForType));
    }

    /// <summary>
    /// Substitute a namespace type definition. 
    /// </summary>
    /// <param name="namespaceTypeDefinition"></param>
    /// <returns></returns>
    public virtual INamespaceTypeDefinition Substitute(INamespaceTypeDefinition namespaceTypeDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(namespaceTypeDefinition));
    }

    /// <summary>
    /// Substitute a namespace type reference. 
    /// </summary>
    /// <param name="namespaceTypeReference"></param>
    /// <returns></returns>
    public virtual INamespaceTypeReference Substitute(INamespaceTypeReference namespaceTypeReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(namespaceTypeReference);
    }

    /// <summary>
    /// Substitute a nested alias for type. 
    /// </summary>
    /// <param name="nestedAliasForType"></param>
    /// <returns></returns>
    public virtual INestedAliasForType Substitute(INestedAliasForType nestedAliasForType) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(nestedAliasForType));
    }

    /// <summary>
    /// Subsitute a nested type definition. 
    /// </summary>
    /// <param name="nestedTypeDefinition"></param>
    /// <returns></returns>
    public virtual INestedTypeDefinition Substitute(INestedTypeDefinition nestedTypeDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy((NestedTypeDefinition)this.cache[nestedTypeDefinition]);
    }

    /// <summary>
    /// Substitute a nested type reference. 
    /// </summary>
    /// <param name="nestedTypeReference"></param>
    /// <returns></returns>
    public virtual INestedTypeReference Substitute(INestedTypeReference nestedTypeReference) {
      this.coneAlreadyFixed = true;
      ISpecializedNestedTypeReference specializedNesetedTypeReference = nestedTypeReference as ISpecializedNestedTypeReference;
      if (specializedNesetedTypeReference != null)
        return this.DeepCopy(specializedNesetedTypeReference);
      return this.DeepCopy(nestedTypeReference);
    }

    /// <summary>
    /// Substitute a nested unit namespace. 
    /// </summary>
    /// <param name="nestedUnitNamespace"></param>
    /// <returns></returns>
    public virtual INestedUnitNamespace Substitute(INestedUnitNamespace nestedUnitNamespace) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(nestedUnitNamespace);
    }

    /// <summary>
    /// Substitute a nested unit namespace reference. 
    /// </summary>
    /// <param name="nestedUnitNamespaceReference"></param>
    /// <returns></returns>
    public virtual INestedUnitNamespaceReference Substitute(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(nestedUnitNamespaceReference);
    }

    /// <summary>
    /// Substitute a nested unit set namespace. Not implemented. 
    /// </summary>
    /// <param name="nestedUnitSetNamespace"></param>
    /// <returns></returns>
    public virtual INestedUnitSetNamespace Substitute(INestedUnitSetNamespace nestedUnitSetNamespace) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Substitute a parameter definition.
    /// </summary>
    /// <param name="parameterDefinition"></param>
    /// <returns></returns>
    public virtual IParameterDefinition Substitute(IParameterDefinition parameterDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(parameterDefinition));
    }

    /// <summary>
    /// Substitute a parameter type information object. 
    /// </summary>
    /// <param name="parameterTypeInformation"></param>
    /// <returns></returns>
    public virtual IParameterTypeInformation Substitute(IParameterTypeInformation parameterTypeInformation) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(parameterTypeInformation));
    }

    /// <summary>
    /// Substitute a pointer type reference. 
    /// </summary>
    /// <param name="pointerTypeReference"></param>
    /// <returns></returns>
    public virtual IPointerTypeReference Substitute(IPointerTypeReference pointerTypeReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(pointerTypeReference);
    }

    /// <summary>
    /// Substitute a property definition. 
    /// </summary>
    /// <param name="propertyDefinition"></param>
    /// <returns></returns>
    public virtual IPropertyDefinition Substitute(IPropertyDefinition propertyDefinition) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(propertyDefinition));
    }

    /// <summary>
    /// Substitute a resource reference. 
    /// </summary>
    /// <param name="resourceReference"></param>
    /// <returns></returns>
    public virtual IResourceReference Substitute(IResourceReference resourceReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(resourceReference));
    }

    /// <summary>
    /// Substitute a root unit namespace. 
    /// </summary>
    /// <param name="rootUnitNamespace"></param>
    /// <returns></returns>
    public virtual IRootUnitNamespace Substitute(IRootUnitNamespace rootUnitNamespace) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(rootUnitNamespace));
    }

    /// <summary>
    /// Substitute a root unit namespace reference. 
    /// </summary>
    /// <param name="rootUnitNamespaceReference"></param>
    /// <returns></returns>
    public virtual IRootUnitNamespaceReference Substitute(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(rootUnitNamespaceReference);
    }

    /// <summary>
    /// Substitute a root unit set namespace. Not implemented. 
    /// </summary>
    /// <param name="rootUnitSetNamespace"></param>
    /// <returns></returns>
    public virtual IRootUnitSetNamespace Substitute(IRootUnitSetNamespace rootUnitSetNamespace) {
      this.coneAlreadyFixed = true;
      throw new NotImplementedException();
    }

    /// <summary>
    /// Substitute a security attribute. 
    /// </summary>
    /// <param name="securityAttribute"></param>
    /// <returns></returns>
    public virtual ISecurityAttribute Substitute(ISecurityAttribute securityAttribute) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(securityAttribute));
    }

    /// <summary>
    /// Substitute a type reference according to its kind. 
    /// </summary>
    /// <param name="typeReference"></param>
    /// <returns></returns>
    public virtual ITypeReference Substitute(ITypeReference typeReference) {
      INamespaceTypeReference/*?*/ namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference != null)
        return this.Substitute(namespaceTypeReference);
      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      if (nestedTypeReference != null)
        return this.Substitute(nestedTypeReference);
      IGenericMethodParameterReference/*?*/ genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
      if (genericMethodParameterReference != null)
        return this.Substitute(genericMethodParameterReference);
      IArrayTypeReference/*?*/ arrayTypeReference = typeReference as IArrayTypeReference;
      if (arrayTypeReference != null)
        return this.Substitute(arrayTypeReference);
      IGenericTypeParameterReference/*?*/ genericTypeParameterReference = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterReference != null)
        return this.Substitute(genericTypeParameterReference);
      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null)
        return this.Substitute(genericTypeInstanceReference);
      IPointerTypeReference/*?*/ pointerTypeReference = typeReference as IPointerTypeReference;
      if (pointerTypeReference != null)
        return this.Substitute(pointerTypeReference);
      IFunctionPointerTypeReference/*?*/ functionPointerTypeReference = typeReference as IFunctionPointerTypeReference;
      if (functionPointerTypeReference != null)
        return this.Substitute(functionPointerTypeReference);
      IModifiedTypeReference/*?*/ modifiedTypeReference = typeReference as IModifiedTypeReference;
      if (modifiedTypeReference != null)
        return this.Substitute(modifiedTypeReference);
      IManagedPointerTypeReference/*?*/ managedPointerTypeReference = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeReference != null)
        return this.Substitute(managedPointerTypeReference);
      //TODO: error
      return typeReference;
    }

    /// <summary>
    /// Substitute a unit set. Not implemented. 
    /// </summary>
    /// <param name="unitSet"></param>
    /// <returns></returns>
    public virtual IUnitSet Substitute(IUnitSet unitSet) {
      this.coneAlreadyFixed = true;
      throw new NotImplementedException();
    }

    /// <summary>
    /// Substitute a Win32 resource.
    /// </summary>
    /// <param name="win32Resource"></param>
    /// <returns></returns>
    public virtual IWin32Resource Substitute(IWin32Resource win32Resource) {
      this.coneAlreadyFixed = true;
      return this.DeepCopy(this.GetMutableShallowCopy(win32Resource));
    }
    #endregion public copy methods

  }

  /// <summary>
  /// A helper class that makes copies of every definition that may be referenced
  /// before it will be encountered during a standard traversal. This excludes structural
  /// definitions that exist only as the result of resolving references to structural types.
  /// In other words, this makes early (and shallow) copies of fields, methods and types
  /// so that they can be referenced before being deep copied.
  /// </summary>
  /// <remarks>
  /// CollectAndShallowCopyDefinitions visits namespace and below. Assembly and module 
  /// should be cached by the caller to make sure the right kind (assembly or module) is copied. 
  /// 
  /// </remarks>
  internal class CollectAndShallowCopyDefinitions : MetadataVisitor {
    MetadataCopier copier;
    List<INamedTypeDefinition> newTypes;

    /// <summary>
    /// A helper class that makes copies of every definition that may be referenced
    /// before it will be encountered during a standard traversal. This excludes structural
    /// definitions that exist only as the result of resolving references to structural types.
    /// In other words, this makes early (and shallow) copies of fields, methods and types
    /// so that they can be referenced before being deep copied.
    /// </summary>
    /// <param name="copier"></param>
    /// <param name="newTypes"></param>
    internal CollectAndShallowCopyDefinitions(MetadataCopier copier, List<INamedTypeDefinition> newTypes) {
      this.copier = copier;
      this.newTypes = newTypes;
    }

    /// <summary>
    /// Visit a field definition. 
    /// </summary>
    /// <param name="fieldDefinition"></param>
    public override void Visit(IFieldDefinition fieldDefinition) {
      if (!this.copier.cache.ContainsKey(fieldDefinition)) {
        var copy = new FieldDefinition();
        copy.Copy(fieldDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(fieldDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a global field definition. 
    /// </summary>
    /// <param name="globalFieldDefinition"></param>
    public override void Visit(IGlobalFieldDefinition globalFieldDefinition) {
      if (!this.copier.cache.ContainsKey(globalFieldDefinition)) {
        var copy = new GlobalFieldDefinition();
        copy.Copy(globalFieldDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(globalFieldDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a global method definition. 
    /// </summary>
    /// <param name="globalMethodDefinition"></param>
    public override void Visit(IGlobalMethodDefinition globalMethodDefinition) {
      if (!this.copier.cache.ContainsKey(globalMethodDefinition)) {
        var copy = new GlobalMethodDefinition();
        copy.Copy(globalMethodDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(globalMethodDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a method definition.
    /// </summary>
    /// <param name="method"></param>
    public override void Visit(IMethodDefinition method) {
      if (!this.copier.cache.ContainsKey(method)) {
        var copy = new MethodDefinition();
        copy.Copy(method, this.copier.host.InternFactory);
        this.copier.cache.Add(method, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a namespace type definition. 
    /// </summary>
    /// <param name="namespaceTypeDefinition"></param>
    public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
      if (!this.copier.cache.ContainsKey(namespaceTypeDefinition)) {
        var copy = new NamespaceTypeDefinition();
        copy.Copy(namespaceTypeDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(namespaceTypeDefinition, copy);
        this.copier.cache.Add(copy, copy);
        this.newTypes.Add(copy);
      }
    }

    /// <summary>
    /// Visit a nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition"></param>
    public override void Visit(INestedTypeDefinition nestedTypeDefinition) {
      if (!this.copier.cache.ContainsKey(nestedTypeDefinition)) {
        var copy = new NestedTypeDefinition();
        copy.Copy(nestedTypeDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(nestedTypeDefinition, copy);
        this.copier.cache.Add(copy, copy);
        this.newTypes.Add(copy);
      }
    }

    /// <summary>
    /// Visit an generic method parameter. 
    /// </summary>
    /// <param name="genericMethodParameter"></param>
    public override void Visit(IGenericMethodParameter genericMethodParameter) {
      if (!this.copier.cache.ContainsKey(genericMethodParameter)) {
        var copy = new GenericMethodParameter();
        copy.Copy(genericMethodParameter, this.copier.host.InternFactory);
        this.copier.cache.Add(genericMethodParameter, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a generic type parameter.
    /// </summary>
    /// <param name="genericTypeParameter"></param>
    public override void Visit(IGenericTypeParameter genericTypeParameter) {
      if (!this.copier.cache.ContainsKey(genericTypeParameter)) {
        var copy = new GenericTypeParameter();
        copy.Copy(genericTypeParameter, this.copier.host.InternFactory);
        this.copier.cache.Add(genericTypeParameter, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit an event definition. 
    /// </summary>
    /// <param name="eventDefinition"></param>
    public override void Visit(IEventDefinition eventDefinition) {
      if (!this.copier.cache.ContainsKey(eventDefinition)) {
        var copy = new EventDefinition();
        copy.Copy(eventDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(eventDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a local definition. 
    /// </summary>
    /// <param name="localDefinition"></param>
    public override void Visit(ILocalDefinition localDefinition) {
      if (!this.copier.cache.ContainsKey(localDefinition)) {
        var copy = new LocalDefinition();
        copy.Copy(localDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(localDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit a root unit namespace.
    /// </summary>
    /// <param name="rootUnitNamespace"></param>
    public override void Visit(IRootUnitNamespace rootUnitNamespace) {
      if (!this.copier.cache.ContainsKey(rootUnitNamespace)) {
        var copy = new RootUnitNamespace();
        copy.Copy(rootUnitNamespace, this.copier.host.InternFactory);
        this.copier.cache.Add(rootUnitNamespace, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit an INestedUnitNamespace
    /// </summary>
    /// <param name="nestedUnitNamespace"></param>
    public override void Visit(INestedUnitNamespace nestedUnitNamespace) {
      if (!this.copier.cache.ContainsKey(nestedUnitNamespace)) {
        var copy = new NestedUnitNamespace();
        copy.Copy(nestedUnitNamespace, this.copier.host.InternFactory);
        this.copier.cache.Add(nestedUnitNamespace, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit an IParameterDefinition. Create a mutable parameter definition if one is not already created. 
    /// </summary>
    /// <param name="parameterDefinition"></param>
    public override void Visit(IParameterDefinition parameterDefinition) {
      if (!this.copier.cache.ContainsKey(parameterDefinition)) {
        var copy = new ParameterDefinition();
        copy.Copy(parameterDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(parameterDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

    /// <summary>
    /// Visit an IPropertyDefinition. Create a mutable PropertyDefinition if one is not already created. 
    /// </summary>
    /// <param name="propertyDefinition"></param>
    public override void Visit(IPropertyDefinition propertyDefinition) {
      if (!this.copier.cache.ContainsKey(propertyDefinition)) {
        var copy = new PropertyDefinition();
        copy.Copy(propertyDefinition, this.copier.host.InternFactory);
        this.copier.cache.Add(propertyDefinition, copy);
        this.copier.cache.Add(copy, copy);
      }
    }

  }
}
