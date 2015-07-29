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
  /// A class that traverses a metadata model in depth first, left to right order,
  /// rewriting each mutable node it visits by updating the node's children with recursivly rewritten nodes.
  /// </summary>
  public class MetadataRewriter {

    /// <summary>
    /// A class that traverses a metadata model in depth first, left to right order,
    /// rewriting each mutable node it visits by updating the node's children with recursivly rewritten nodes.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this rewriter. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    /// <param name="copyAndRewriteImmutableReferences">If true, the rewriter replace frozen or immutable references with shallow copies.</param>
    public MetadataRewriter(IMetadataHost host, bool copyAndRewriteImmutableReferences = false) {
      Contract.Requires(host != null);
      this.host = host;
      this.internFactory = host.InternFactory;
      this.dispatchingVisitor = new Dispatcher() { rewriter = this };
      if (copyAndRewriteImmutableReferences)
        this.shallowCopier = new MetadataShallowCopier(host);
    }

    /// <summary>
    /// An object representing the application that is hosting this rewriter. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.
    /// </summary>
    protected readonly IMetadataHost host;
    IInternFactory internFactory;
    MetadataShallowCopier/*?*/ shallowCopier;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.host != null);
      Contract.Invariant(this.internFactory != null);
    }

    Dispatcher dispatchingVisitor;
    class Dispatcher : MetadataVisitor {
      internal MetadataRewriter rewriter;
      internal object result;

      public override void Visit(IArrayTypeReference arrayTypeReference) {
        this.result = this.rewriter.Rewrite(arrayTypeReference);
      }

      public override void Visit(IAssembly assembly) {
        this.result = this.rewriter.Rewrite(assembly);
      }

      public override void Visit(IAssemblyReference assemblyReference) {
        this.result = this.rewriter.Rewrite(assemblyReference);
      }

      public override void Visit(IEventDefinition eventDefinition) {
        Contract.Assume(!(eventDefinition is ISpecializedEventDefinition));
        this.result = this.rewriter.RewriteUnspecialized(eventDefinition);
      }

      public override void Visit(IFieldDefinition fieldDefinition) {
        Contract.Assume(!(fieldDefinition is ISpecializedFieldDefinition));
        this.result = this.rewriter.RewriteUnspecialized(fieldDefinition);
      }

      public override void Visit(IFieldReference fieldReference) {
        Contract.Assume(!(fieldReference is ISpecializedFieldReference));
        this.result = this.rewriter.RewriteUnspecialized(fieldReference);
      }

      public override void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
        this.result = this.rewriter.Rewrite(functionPointerTypeReference);
      }

      public override void Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
        this.result = this.rewriter.Rewrite(genericMethodInstanceReference);
      }

      public override void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
        this.result = this.rewriter.Rewrite(genericMethodParameterReference);
      }

      public override void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
        this.result = this.rewriter.Rewrite(genericTypeInstanceReference);
      }

      public override void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
        this.result = this.rewriter.Rewrite(genericTypeParameterReference);
      }

      public override void Visit(IGlobalFieldDefinition globalFieldDefinition) {
        this.result = this.rewriter.Rewrite(globalFieldDefinition);
      }

      public override void Visit(IGlobalMethodDefinition globalMethodDefinition) {
        this.result = this.rewriter.Rewrite(globalMethodDefinition);
      }

      public override void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
        this.result = this.rewriter.Rewrite(managedPointerTypeReference);
      }

      public override void Visit(IMetadataConstant constant) {
        this.result = this.rewriter.Rewrite(constant);
      }

      public override void Visit(IMetadataCreateArray createArray) {
        this.result = this.rewriter.Rewrite(createArray);
      }

      public override void Visit(IMetadataNamedArgument namedArgument) {
        this.result = this.rewriter.Rewrite(namedArgument);
      }

      public override void Visit(IMetadataTypeOf typeOf) {
        this.result = this.rewriter.Rewrite(typeOf);
      }

      public override void Visit(IMethodDefinition method) {
        Contract.Assume(!(method is ISpecializedMethodDefinition));
        this.result = this.rewriter.RewriteUnspecialized(method);
      }

      public override void Visit(IMethodReference methodReference) {
        Contract.Assume(!(methodReference is IGenericMethodInstanceReference));
        Contract.Assume(!(methodReference is ISpecializedMethodReference));
        this.result = this.rewriter.RewriteUnspecialized(methodReference);
      }

      public override void Visit(IModifiedTypeReference modifiedTypeReference) {
        this.result = this.rewriter.Rewrite(modifiedTypeReference);
      }

      public override void Visit(IModule module) {
        this.result = this.rewriter.Rewrite(module);
      }

      public override void Visit(IModuleReference moduleReference) {
        this.result = this.rewriter.Rewrite(moduleReference);
      }

      public override void Visit(INamespaceAliasForType namespaceAliasForType) {
        this.result = this.rewriter.Rewrite(namespaceAliasForType);
      }

      public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
        this.result = this.rewriter.Rewrite(namespaceTypeDefinition);
      }

      public override void Visit(INamespaceTypeReference namespaceTypeReference) {
        this.result = this.rewriter.Rewrite(namespaceTypeReference);
      }

      public override void Visit(INestedAliasForType nestedAliasForType) {
        this.result = this.rewriter.Rewrite(nestedAliasForType);
      }

      public override void Visit(INestedTypeDefinition nestedTypeDefinition) {
        Contract.Assume(!(nestedTypeDefinition is ISpecializedNestedTypeDefinition));
        this.result = this.rewriter.RewriteUnspecialized(nestedTypeDefinition);
      }

      public override void Visit(INestedTypeReference nestedTypeReference) {
        Contract.Assume(!(nestedTypeReference is ISpecializedNestedTypeReference));
        this.result = this.rewriter.RewriteUnspecialized(nestedTypeReference);
      }

      public override void Visit(INestedUnitNamespace nestedUnitNamespace) {
        this.result = this.rewriter.Rewrite(nestedUnitNamespace);
      }

      public override void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
        this.result = this.rewriter.Rewrite(nestedUnitNamespaceReference);
      }

      public override void Visit(IPointerTypeReference pointerTypeReference) {
        this.result = this.rewriter.Rewrite(pointerTypeReference);
      }

      public override void Visit(IPropertyDefinition propertyDefinition) {
        Contract.Assume(!(propertyDefinition is ISpecializedPropertyDefinition));
        this.result = this.rewriter.RewriteUnspecialized(propertyDefinition);
      }

      public override void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
        this.result = this.rewriter.Rewrite(rootUnitNamespaceReference);
      }

      public override void Visit(ISpecializedEventDefinition specializedEventDefinition) {
        this.result = this.rewriter.Rewrite(specializedEventDefinition);
      }

      public override void Visit(ISpecializedFieldDefinition specializedFieldDefinition) {
        this.result = this.rewriter.Rewrite(specializedFieldDefinition);
      }

      public override void Visit(ISpecializedFieldReference specializedFieldReference) {
        this.result = this.rewriter.Rewrite(specializedFieldReference);
      }

      public override void Visit(ISpecializedMethodDefinition specializedMethodDefinition) {
        this.result = this.rewriter.Rewrite(specializedMethodDefinition);
      }

      public override void Visit(ISpecializedMethodReference specializedMethodReference) {
        this.result = this.rewriter.Rewrite(specializedMethodReference);
      }

      public override void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition) {
        this.result = this.rewriter.Rewrite(specializedPropertyDefinition);
      }

      public override void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
        this.result = this.rewriter.Rewrite(specializedNestedTypeDefinition);
      }

      public override void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
        this.result = this.rewriter.Rewrite(specializedNestedTypeReference);
      }


    }

    /// <summary>
    /// A map from reference to rewritten reference. Can be used to avoid rewriting the same reference more than once.
    /// </summary>
    protected Hashtable<IReference, object> referenceRewrites = new Hashtable<IReference, object>();

    /// <summary>
    /// Rewrites the alias for type
    /// </summary>
    public virtual IAliasForType Rewrite(IAliasForType aliasForType) {
      Contract.Requires(aliasForType != null);
      Contract.Ensures(Contract.Result<IAliasForType>() != null);

      if (aliasForType is Dummy) return aliasForType;
      aliasForType.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IAliasForType)??aliasForType;
    }

    /// <summary>
    /// Rewrites the alias for type member.
    /// </summary>
    public virtual IAliasMember Rewrite(IAliasMember aliasMember) {
      Contract.Requires(aliasMember != null);
      Contract.Ensures(Contract.Result<IAliasMember>() != null);

      if (aliasMember is Dummy) return aliasMember;
      aliasMember.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IAliasMember)??aliasMember;
    }

    /// <summary>
    /// Rewrites the array type reference.
    /// </summary>
    public virtual IArrayTypeReference Rewrite(IArrayTypeReference arrayTypeReference) {
      Contract.Requires(arrayTypeReference != null);
      Contract.Ensures(Contract.Result<IArrayTypeReference>() != null);

      if (arrayTypeReference is Dummy) return arrayTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(arrayTypeReference, out result)) return (IArrayTypeReference)result;
      var mutableArrayTypeReference = arrayTypeReference as ArrayTypeReference;
      if (mutableArrayTypeReference == null || mutableArrayTypeReference.IsFrozen) {
        if (this.shallowCopier == null) return arrayTypeReference;
        mutableArrayTypeReference = this.shallowCopier.Copy(arrayTypeReference);
      }
      this.referenceRewrites[arrayTypeReference] = mutableArrayTypeReference;
      this.RewriteChildren(mutableArrayTypeReference);
      return mutableArrayTypeReference;
    }

    /// <summary>
    /// Rewrites the given assembly.
    /// </summary>
    public virtual IAssembly Rewrite(IAssembly assembly) {
      Contract.Requires(assembly != null);
      Contract.Ensures(Contract.Result<IAssembly>() != null);

      if (assembly is Dummy) return assembly;
      var mutableAssembly = assembly as Assembly;
      if (mutableAssembly == null) return assembly;
      this.RewriteChildren(mutableAssembly);
      return mutableAssembly;
    }

    /// <summary>
    /// Rewrites the given assembly reference.
    /// </summary>
    public virtual IAssemblyReference Rewrite(IAssemblyReference assemblyReference) {
      Contract.Requires(assemblyReference != null);
      Contract.Ensures(Contract.Result<IAssemblyReference>() != null);

      if (assemblyReference is Dummy) return assemblyReference;
      object result;
      if (this.referenceRewrites.TryGetValue(assemblyReference, out result)) return (IAssemblyReference)result;
      var mutableAssemblyReference = assemblyReference as AssemblyReference;
      if (mutableAssemblyReference == null || mutableAssemblyReference.IsFrozen) {
        if (this.shallowCopier == null || assemblyReference is Assembly) return assemblyReference;
        mutableAssemblyReference = this.shallowCopier.Copy(assemblyReference);
      }
      this.referenceRewrites[assemblyReference] = mutableAssemblyReference;
      this.RewriteChildren(mutableAssemblyReference);
      return mutableAssemblyReference;
    }

    /// <summary>
    /// Rewrites the given custom attribute.
    /// </summary>
    public virtual ICustomAttribute Rewrite(ICustomAttribute customAttribute) {
      Contract.Requires(customAttribute != null);
      Contract.Ensures(Contract.Result<ICustomAttribute>() != null);

      if (customAttribute is Dummy) return customAttribute;
      var mutableCustomAttribute = customAttribute as CustomAttribute;
      if (mutableCustomAttribute == null) return customAttribute;
      this.RewriteChildren(mutableCustomAttribute);
      return mutableCustomAttribute;
    }

    /// <summary>
    /// Rewrites the given custom modifier.
    /// </summary>
    public virtual ICustomModifier Rewrite(ICustomModifier customModifier) {
      Contract.Requires(customModifier != null);
      Contract.Ensures(Contract.Result<ICustomModifier>() != null);

      if (customModifier is Dummy) return customModifier;
      var mutableCustomModifier = customModifier as CustomModifier;
      if (mutableCustomModifier == null) return customModifier;
      this.RewriteChildren(mutableCustomModifier);
      return mutableCustomModifier;
    }

    /// <summary>
    /// Rewrites the given event definition.
    /// </summary>
    public virtual IEventDefinition Rewrite(IEventDefinition eventDefinition) {
      Contract.Requires(eventDefinition != null);
      Contract.Ensures(Contract.Result<IEventDefinition>() != null);

      if (eventDefinition is Dummy) return eventDefinition;
      eventDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IEventDefinition)??eventDefinition;
    }

    /// <summary>
    /// Rewrites the given unspecialized event definition.
    /// </summary>
    protected virtual IEventDefinition RewriteUnspecialized(IEventDefinition eventDefinition) {
      Contract.Requires(eventDefinition != null);
      Contract.Requires(!(eventDefinition is ISpecializedEventDefinition));
      Contract.Ensures(Contract.Result<IEventDefinition>() != null);

      var mutableEventDefinition = eventDefinition as EventDefinition;
      if (mutableEventDefinition == null) return eventDefinition;
      this.RewriteChildren(mutableEventDefinition);
      return mutableEventDefinition;
    }

    /// <summary>
    /// Rewrites the given field definition.
    /// </summary>
    public virtual IFieldDefinition Rewrite(IFieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);
      Contract.Ensures(Contract.Result<IFieldDefinition>() != null);

      if (fieldDefinition is Dummy) return fieldDefinition;
      fieldDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IFieldDefinition)??fieldDefinition;
    }

    /// <summary>
    /// Rewrites the given unspecialized field definition.
    /// </summary>
    protected virtual IFieldDefinition RewriteUnspecialized(IFieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);
      Contract.Requires(!(fieldDefinition is ISpecializedFieldDefinition));
      Contract.Ensures(Contract.Result<IFieldDefinition>() != null);

      var mutableFieldDefinition = fieldDefinition as FieldDefinition;
      if (mutableFieldDefinition == null) return fieldDefinition;
      this.RewriteChildren(mutableFieldDefinition);
      return mutableFieldDefinition;
    }

    /// <summary>
    /// Rewrites the given field reference.
    /// </summary>
    public virtual IFieldReference Rewrite(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Ensures(Contract.Result<IFieldReference>() != null);

      if (fieldReference is Dummy) return fieldReference;
      fieldReference.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IFieldReference)??fieldReference;
    }

    /// <summary>
    /// Rewrites the given field reference.
    /// </summary>
    protected virtual IFieldReference RewriteUnspecialized(IFieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Requires(!(fieldReference is ISpecializedFieldReference));
      Contract.Ensures(Contract.Result<IFieldReference>() != null);

      if (fieldReference is Dummy) return fieldReference;
      object result;
      if (this.referenceRewrites.TryGetValue(fieldReference, out result)) return (IFieldReference)result;
      var mutableFieldReference = fieldReference as FieldReference;
      if (mutableFieldReference == null || mutableFieldReference.IsFrozen) {
        if (this.shallowCopier == null || fieldReference is FieldDefinition) return fieldReference;
        mutableFieldReference = this.shallowCopier.Copy(fieldReference);
      }
      this.referenceRewrites[fieldReference] = mutableFieldReference;
      this.RewriteChildren(mutableFieldReference);
      return mutableFieldReference;
    }

    /// <summary>
    /// Rewrites the reference to a local definition.
    /// </summary>
    public virtual object RewriteReference(ILocalDefinition localDefinition) {
      Contract.Requires(localDefinition != null);
      Contract.Ensures(Contract.Result<object>() != null);

      return localDefinition;
    }

    /// <summary>
    /// Rewrites the reference to a parameter.
    /// </summary>
    public virtual object RewriteReference(IParameterDefinition parameterDefinition) {
      Contract.Requires(parameterDefinition != null);
      Contract.Ensures(Contract.Result<object>() != null);

      return parameterDefinition;
    }

    /// <summary>
    /// Rewrites the given file reference.
    /// </summary>
    public virtual IFileReference Rewrite(IFileReference fileReference) {
      Contract.Requires(fileReference != null);
      Contract.Ensures(Contract.Result<IFileReference>() != null);

      if (fileReference is Dummy) return fileReference;
      var mutableFileReference = fileReference as FileReference;
      if (mutableFileReference == null) return fileReference;
      this.RewriteChildren(mutableFileReference);
      return mutableFileReference;
    }

    /// <summary>
    /// Rewrites the given function pointer type reference.
    /// </summary>
    public virtual IFunctionPointerTypeReference Rewrite(IFunctionPointerTypeReference functionPointerTypeReference) {
      Contract.Requires(functionPointerTypeReference != null);
      Contract.Ensures(Contract.Result<IFunctionPointerTypeReference>() != null);

      if (functionPointerTypeReference is Dummy) return functionPointerTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(functionPointerTypeReference, out result)) return (IFunctionPointerTypeReference)result;
      var mutableFunctionPointerTypeReference = functionPointerTypeReference as FunctionPointerTypeReference;
      if (mutableFunctionPointerTypeReference == null || mutableFunctionPointerTypeReference.IsFrozen) {
        if (this.shallowCopier == null) return functionPointerTypeReference;
        mutableFunctionPointerTypeReference = this.shallowCopier.Copy(functionPointerTypeReference);
      }
      this.referenceRewrites[functionPointerTypeReference] = mutableFunctionPointerTypeReference;
      this.RewriteChildren(mutableFunctionPointerTypeReference);
      return mutableFunctionPointerTypeReference;
    }

    /// <summary>
    /// Rewrites the given generic method instance reference.
    /// </summary>
    public virtual IGenericMethodInstanceReference Rewrite(IGenericMethodInstanceReference genericMethodInstanceReference) {
      Contract.Requires(genericMethodInstanceReference != null);
      Contract.Ensures(Contract.Result<IGenericMethodInstanceReference>() != null);

      if (genericMethodInstanceReference is Dummy) return genericMethodInstanceReference;
      object result;
      if (this.referenceRewrites.TryGetValue(genericMethodInstanceReference, out result)) return (IGenericMethodInstanceReference)result;
      var mutableGenericMethodInstanceReference = genericMethodInstanceReference as GenericMethodInstanceReference;
      if (mutableGenericMethodInstanceReference == null || mutableGenericMethodInstanceReference.IsFrozen) {
        if (this.shallowCopier == null) return genericMethodInstanceReference;
        mutableGenericMethodInstanceReference = this.shallowCopier.Copy(genericMethodInstanceReference);
      }
      this.referenceRewrites[genericMethodInstanceReference] = mutableGenericMethodInstanceReference;
      this.RewriteChildren(mutableGenericMethodInstanceReference);
      return mutableGenericMethodInstanceReference;
    }

    /// <summary>
    /// Rewrites the given generic method parameter reference.
    /// </summary>
    public virtual IGenericMethodParameter Rewrite(IGenericMethodParameter genericMethodParameter) {
      Contract.Requires(genericMethodParameter != null);
      Contract.Ensures(Contract.Result<IGenericMethodParameter>() != null);

      if (genericMethodParameter is Dummy) return genericMethodParameter;
      var mutableGenericMethodParameter = genericMethodParameter as GenericMethodParameter;
      if (mutableGenericMethodParameter == null) return genericMethodParameter;
      this.RewriteChildren(mutableGenericMethodParameter);
      return mutableGenericMethodParameter;
    }

    /// <summary>
    /// Rewrites the given generic method parameter reference.
    /// </summary>
    public virtual ITypeReference Rewrite(IGenericMethodParameterReference genericMethodParameterReference) {
      Contract.Requires(genericMethodParameterReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      if (genericMethodParameterReference is Dummy) return genericMethodParameterReference;
      object result;
      if (this.referenceRewrites.TryGetValue(genericMethodParameterReference, out result)) return (IGenericMethodParameterReference)result;
      var mutableGenericMethodInstanceReference = genericMethodParameterReference as GenericMethodParameterReference;
      if (mutableGenericMethodInstanceReference == null || mutableGenericMethodInstanceReference.IsFrozen) {
        if (this.shallowCopier == null || genericMethodParameterReference is GenericMethodParameter) return genericMethodParameterReference;
        mutableGenericMethodInstanceReference = this.shallowCopier.Copy(genericMethodParameterReference);
      }
      this.referenceRewrites[genericMethodParameterReference] = mutableGenericMethodInstanceReference;
      this.RewriteChildren(mutableGenericMethodInstanceReference);
      return mutableGenericMethodInstanceReference;
    }

    /// <summary>
    /// Rewrites the given generic type instance reference.
    /// </summary>
    public virtual ITypeReference Rewrite(IGenericTypeInstanceReference genericTypeInstanceReference) {
      Contract.Requires(genericTypeInstanceReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      if (genericTypeInstanceReference is Dummy) return genericTypeInstanceReference;
      object result;
      if (this.referenceRewrites.TryGetValue(genericTypeInstanceReference, out result)) return (IGenericTypeInstanceReference)result;
      var mutableGenericTypeInstanceReference = genericTypeInstanceReference as GenericTypeInstanceReference;
      if (mutableGenericTypeInstanceReference == null || mutableGenericTypeInstanceReference.IsFrozen) {
        if (this.shallowCopier == null) return genericTypeInstanceReference;
        mutableGenericTypeInstanceReference = this.shallowCopier.Copy(genericTypeInstanceReference);
      }
      this.referenceRewrites[genericTypeInstanceReference] = mutableGenericTypeInstanceReference;
      this.RewriteChildren(mutableGenericTypeInstanceReference);
      return mutableGenericTypeInstanceReference;
    }

    /// <summary>
    /// Rewrites the given generic type parameter reference.
    /// </summary>
    public virtual IGenericTypeParameter Rewrite(IGenericTypeParameter genericTypeParameter) {
      Contract.Requires(genericTypeParameter != null);
      Contract.Ensures(Contract.Result<IGenericTypeParameter>() != null);

      if (genericTypeParameter is Dummy) return genericTypeParameter;
      var mutableGenericTypeParameter = genericTypeParameter as GenericTypeParameter;
      if (mutableGenericTypeParameter == null) return genericTypeParameter;
      this.RewriteChildren(mutableGenericTypeParameter);
      return mutableGenericTypeParameter;
    }

    /// <summary>
    /// Rewrites the given generic type parameter reference.
    /// </summary>
    public virtual ITypeReference Rewrite(IGenericTypeParameterReference genericTypeParameterReference) {
      Contract.Requires(genericTypeParameterReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      if (genericTypeParameterReference is Dummy) return genericTypeParameterReference;
      object result;
      if (this.referenceRewrites.TryGetValue(genericTypeParameterReference, out result)) return (IGenericTypeParameterReference)result;
      var mutableGenericTypeParameterReference = genericTypeParameterReference as GenericTypeParameterReference;
      if (mutableGenericTypeParameterReference == null || mutableGenericTypeParameterReference.IsFrozen) {
        if (this.shallowCopier == null || genericTypeParameterReference is GenericTypeParameter) return genericTypeParameterReference;
        mutableGenericTypeParameterReference = this.shallowCopier.Copy(genericTypeParameterReference);
      }
      this.referenceRewrites[genericTypeParameterReference] = mutableGenericTypeParameterReference;
      this.RewriteChildren(mutableGenericTypeParameterReference);
      return mutableGenericTypeParameterReference;
    }

    /// <summary>
    /// Rewrites the specified global field definition.
    /// </summary>
    public virtual IGlobalFieldDefinition Rewrite(IGlobalFieldDefinition globalFieldDefinition) {
      Contract.Requires(globalFieldDefinition != null);
      Contract.Ensures(Contract.Result<IGlobalFieldDefinition>() != null);

      if (globalFieldDefinition is Dummy) return globalFieldDefinition;
      var mutableGlobalFieldDefinition = globalFieldDefinition as GlobalFieldDefinition;
      if (mutableGlobalFieldDefinition == null) return globalFieldDefinition;
      this.RewriteChildren(mutableGlobalFieldDefinition);
      return mutableGlobalFieldDefinition;
    }

    /// <summary>
    /// Rewrites the specified global method definition.
    /// </summary>
    public virtual IGlobalMethodDefinition Rewrite(IGlobalMethodDefinition globalMethodDefinition) {
      Contract.Requires(globalMethodDefinition != null);
      Contract.Ensures(Contract.Result<IGlobalMethodDefinition>() != null);

      if (globalMethodDefinition is Dummy) return globalMethodDefinition;
      var mutableGlobalMethodDefinition = globalMethodDefinition as GlobalMethodDefinition;
      if (mutableGlobalMethodDefinition == null) return globalMethodDefinition;
      this.RewriteChildren(mutableGlobalMethodDefinition);
      return mutableGlobalMethodDefinition;
    }

    /// <summary>
    /// Rewrites the specified local definition.
    /// </summary>
    public virtual ILocalDefinition Rewrite(ILocalDefinition localDefinition) {
      Contract.Requires(localDefinition != null);
      Contract.Ensures(Contract.Result<ILocalDefinition>() != null);

      if (localDefinition is Dummy) return localDefinition;
      var mutableLocalDefinition = localDefinition as LocalDefinition;
      if (mutableLocalDefinition == null) return localDefinition;
      this.RewriteChildren(mutableLocalDefinition);
      return mutableLocalDefinition;
    }

    /// <summary>
    /// Rewrites the given managed pointer type reference.
    /// </summary>
    public virtual IManagedPointerTypeReference Rewrite(IManagedPointerTypeReference managedPointerTypeReference) {
      Contract.Requires(managedPointerTypeReference != null);
      Contract.Ensures(Contract.Result<IManagedPointerTypeReference>() != null);

      if (managedPointerTypeReference is Dummy) return managedPointerTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(managedPointerTypeReference, out result)) return (IManagedPointerTypeReference)result;
      var mutableManagedPointerTypeReference = managedPointerTypeReference as ManagedPointerTypeReference;
      if (mutableManagedPointerTypeReference == null || mutableManagedPointerTypeReference.IsFrozen) {
        if (this.shallowCopier == null) return managedPointerTypeReference;
        mutableManagedPointerTypeReference = this.shallowCopier.Copy(managedPointerTypeReference);
      }
      this.referenceRewrites[managedPointerTypeReference] = mutableManagedPointerTypeReference;
      this.RewriteChildren(mutableManagedPointerTypeReference);
      return mutableManagedPointerTypeReference;
    }

    /// <summary>
    /// Rewrites the given marshalling information.
    /// </summary>
    public virtual IMarshallingInformation Rewrite(IMarshallingInformation marshallingInformation) {
      Contract.Requires(marshallingInformation != null);
      Contract.Ensures(Contract.Result<IMarshallingInformation>() != null);

      if (marshallingInformation is Dummy) return marshallingInformation;
      var mutableMarshallingInformation = marshallingInformation as MarshallingInformation;
      if (mutableMarshallingInformation == null) return marshallingInformation;
      this.RewriteChildren(mutableMarshallingInformation);
      return mutableMarshallingInformation;
    }

    /// <summary>
    /// Rewrites the given metadata constant.
    /// </summary>
    public virtual IMetadataConstant Rewrite(IMetadataConstant constant) {
      Contract.Requires(constant != null);
      Contract.Ensures(Contract.Result<IMetadataConstant>() != null);

      if (constant is Dummy) return constant;
      var mutableMetadataConstant = constant as MetadataConstant;
      if (mutableMetadataConstant == null) return constant;
      this.RewriteChildren(mutableMetadataConstant);
      return mutableMetadataConstant;
    }

    /// <summary>
    /// Rewrites the given metadata array creation expression.
    /// </summary>
    public virtual IMetadataCreateArray Rewrite(IMetadataCreateArray metadataCreateArray) {
      Contract.Requires(metadataCreateArray != null);
      Contract.Ensures(Contract.Result<IMetadataCreateArray>() != null);

      if (metadataCreateArray is Dummy) return metadataCreateArray;
      var mutableMetadataCreateArray = metadataCreateArray as MetadataCreateArray;
      if (mutableMetadataCreateArray == null) return metadataCreateArray;
      this.RewriteChildren(mutableMetadataCreateArray);
      return mutableMetadataCreateArray;
    }

    /// <summary>
    /// Rewrites the given metadata expression.
    /// </summary>
    public virtual IMetadataExpression Rewrite(IMetadataExpression metadataExpression) {
      Contract.Requires(metadataExpression != null);
      Contract.Ensures(Contract.Result<IMetadataExpression>() != null);

      if (metadataExpression is Dummy) return metadataExpression;
      metadataExpression.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IMetadataExpression)??metadataExpression;
    }

    /// <summary>
    /// Rewrites the given metadata named argument expression.
    /// </summary>
    public virtual IMetadataNamedArgument Rewrite(IMetadataNamedArgument namedArgument) {
      Contract.Requires(namedArgument != null);
      Contract.Ensures(Contract.Result<IMetadataNamedArgument>() != null);

      if (namedArgument is Dummy) return namedArgument;
      var mutableMetadataNamedArgument = namedArgument as MetadataNamedArgument;
      if (mutableMetadataNamedArgument == null) return namedArgument;
      this.RewriteChildren(mutableMetadataNamedArgument);
      return mutableMetadataNamedArgument;
    }

    /// <summary>
    /// Rewrites the given metadata typeof expression.
    /// </summary>
    public virtual IMetadataTypeOf Rewrite(IMetadataTypeOf metadataTypeOf) {
      Contract.Requires(metadataTypeOf != null);
      Contract.Ensures(Contract.Result<IMetadataTypeOf>() != null);

      if (metadataTypeOf is Dummy) return metadataTypeOf;
      var mutableMetadataTypeOf = metadataTypeOf as MetadataTypeOf;
      if (mutableMetadataTypeOf == null) return metadataTypeOf;
      this.RewriteChildren(mutableMetadataTypeOf);
      return mutableMetadataTypeOf;
    }

    /// <summary>
    /// Rewrites the given method body.
    /// </summary>
    public virtual IMethodBody Rewrite(IMethodBody methodBody) {
      Contract.Requires(methodBody != null);
      Contract.Ensures(Contract.Result<IMethodBody>() != null);

      if (methodBody is Dummy) return methodBody;
      var mutableMethodBody = methodBody as MethodBody;
      if (mutableMethodBody == null) return methodBody;
      this.RewriteChildren(mutableMethodBody);
      return mutableMethodBody;
    }

    /// <summary>
    /// Rewrites the given method definition.
    /// </summary>
    public virtual IMethodDefinition Rewrite(IMethodDefinition methodDefinition) {
      Contract.Requires(methodDefinition != null);
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      if (methodDefinition is Dummy) return methodDefinition;
      var mutableMethodDefinition = methodDefinition as MethodDefinition;
      if (mutableMethodDefinition == null) return methodDefinition;
      this.RewriteChildren(mutableMethodDefinition);
      return mutableMethodDefinition;
    }

    /// <summary>
    /// Rewrites the given method definition.
    /// </summary>
    protected virtual IMethodDefinition RewriteUnspecialized(IMethodDefinition methodDefinition) {
      Contract.Requires(methodDefinition != null);
      Contract.Requires(!(methodDefinition is ISpecializedMethodDefinition));
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      if (methodDefinition is Dummy) return methodDefinition;
      methodDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IMethodDefinition)??methodDefinition;
    }

    /// <summary>
    /// Rewrites the given method implementation.
    /// </summary>
    public virtual IMethodImplementation Rewrite(IMethodImplementation methodImplementation) {
      Contract.Requires(methodImplementation != null);
      Contract.Ensures(Contract.Result<IMethodImplementation>() != null);

      if (methodImplementation is Dummy) return methodImplementation;
      var mutableMethodImplementation = methodImplementation as MethodImplementation;
      if (mutableMethodImplementation == null) return methodImplementation;
      this.RewriteChildren(mutableMethodImplementation);
      return mutableMethodImplementation;
    }

    /// <summary>
    /// Rewrites the given method reference.
    /// </summary>
    public virtual IMethodReference Rewrite(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Ensures(Contract.Result<IMethodReference>() != null);

      if (methodReference is Dummy) return methodReference;
      methodReference.DispatchAsReference(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IMethodReference)??methodReference;
    }

    /// <summary>
    /// Rewrites the given method reference.
    /// </summary>
    public virtual IMethodReference RewriteUnspecialized(IMethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Requires(!(methodReference is ISpecializedMethodReference));
      Contract.Ensures(Contract.Result<IMethodReference>() != null);

      if (methodReference is Dummy) return methodReference;
      object result;
      if (this.referenceRewrites.TryGetValue(methodReference, out result)) return (IMethodReference)result;
      var mutableMethodReference = methodReference as MethodReference;
      if (mutableMethodReference == null || mutableMethodReference.IsFrozen) {
        if (this.shallowCopier == null || methodReference is MethodDefinition) return methodReference;
        mutableMethodReference = this.shallowCopier.Copy(methodReference);
      }
      this.referenceRewrites[methodReference] = mutableMethodReference;
      this.RewriteChildren(mutableMethodReference);
      return mutableMethodReference;
    }

    /// <summary>
    /// Rewrites the given modified type reference.
    /// </summary>
    public virtual IModifiedTypeReference Rewrite(IModifiedTypeReference modifiedTypeReference) {
      Contract.Requires(modifiedTypeReference != null);
      Contract.Ensures(Contract.Result<IModifiedTypeReference>() != null);

      if (modifiedTypeReference is Dummy) return modifiedTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(modifiedTypeReference, out result)) return (IModifiedTypeReference)result;
      var mutableModifiedTypeReference = modifiedTypeReference as ModifiedTypeReference;
      if (mutableModifiedTypeReference == null || mutableModifiedTypeReference.IsFrozen) {
        if (this.shallowCopier == null) return modifiedTypeReference;
        mutableModifiedTypeReference = this.shallowCopier.Copy(modifiedTypeReference);
      }
      this.referenceRewrites[modifiedTypeReference] = mutableModifiedTypeReference;
      this.RewriteChildren(mutableModifiedTypeReference);
      return mutableModifiedTypeReference;
    }

    /// <summary>
    /// Rewrites the given module.
    /// </summary>
    public virtual IModule Rewrite(IModule module) {
      Contract.Requires(module != null);
      Contract.Ensures(Contract.Result<IModule>() != null);

      if (module is Dummy) return module;
      var assembly = module as IAssembly;
      if (assembly != null) return this.Rewrite(assembly);
      var mutableModule = module as Module;
      if (mutableModule == null) return module;
      this.RewriteChildren(mutableModule);
      return mutableModule;
    }

    /// <summary>
    /// Rewrites the given module reference.
    /// </summary>
    public virtual IModuleReference Rewrite(IModuleReference moduleReference) {
      Contract.Requires(moduleReference != null);
      Contract.Ensures(Contract.Result<IModuleReference>() != null);

      if (moduleReference is Dummy) return moduleReference;
      object result;
      if (this.referenceRewrites.TryGetValue(moduleReference, out result)) return (IModuleReference)result;
      var mutableModuleReference = moduleReference as ModuleReference;
      if (mutableModuleReference == null || mutableModuleReference.IsFrozen) {
        if (this.shallowCopier == null || moduleReference is Module) return moduleReference;
        mutableModuleReference = this.shallowCopier.Copy(moduleReference);
      }
      this.referenceRewrites[moduleReference] = mutableModuleReference;
      this.RewriteChildren(mutableModuleReference);
      return mutableModuleReference;
    }

    /// <summary>
    /// Rewrites the named specified type reference.
    /// </summary>
    public virtual INamedTypeDefinition Rewrite(INamedTypeDefinition namedTypeDefinition) {
      Contract.Requires(namedTypeDefinition != null);
      Contract.Ensures(Contract.Result<INamedTypeDefinition>() != null);

      if (namedTypeDefinition is Dummy) return namedTypeDefinition;
      namedTypeDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as INamedTypeDefinition)??namedTypeDefinition;
    }

    /// <summary>
    /// Rewrites the named specified type reference.
    /// </summary>
    public virtual INamedTypeReference Rewrite(INamedTypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<INamedTypeReference>() != null);

      if (typeReference is Dummy) return typeReference;
      typeReference.DispatchAsReference(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as INamedTypeReference)??typeReference;
    }

    /// <summary>
    /// Rewrites the namespace alias for type.
    /// </summary>
    public virtual INamespaceAliasForType Rewrite(INamespaceAliasForType namespaceAliasForType) {
      Contract.Requires(namespaceAliasForType != null);
      Contract.Ensures(Contract.Result<INamespaceAliasForType>() != null);

      if (namespaceAliasForType is Dummy) return namespaceAliasForType;
      var mutableNamespaceAliasForType = namespaceAliasForType as NamespaceAliasForType;
      if (mutableNamespaceAliasForType == null) return namespaceAliasForType;
      this.RewriteChildren(mutableNamespaceAliasForType);
      return mutableNamespaceAliasForType;
    }

    /// <summary>
    /// Rewrites the namespace definition.
    /// </summary>
    public virtual INamespaceDefinition Rewrite(INamespaceDefinition namespaceDefinition) {
      Contract.Requires(namespaceDefinition != null);
      Contract.Ensures(Contract.Result<INamespaceDefinition>() != null);

      if (namespaceDefinition is Dummy) return namespaceDefinition;
      namespaceDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as INamespaceDefinition)??namespaceDefinition;
    }

    /// <summary>
    /// Rewrites the specified namespace member.
    /// </summary>
    public virtual INamespaceMember Rewrite(INamespaceMember namespaceMember) {
      Contract.Requires(namespaceMember != null);
      Contract.Ensures(Contract.Result<INamespaceMember>() != null);

      if (namespaceMember is Dummy) return namespaceMember;
      namespaceMember.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as INamespaceMember)??namespaceMember;
    }

    /// <summary>
    /// Rewrites the given namespace type definition.
    /// </summary>
    public virtual INamespaceTypeDefinition Rewrite(INamespaceTypeDefinition namespaceTypeDefinition) {
      Contract.Requires(namespaceTypeDefinition != null);
      Contract.Ensures(Contract.Result<INamespaceTypeDefinition>() != null);

      if (namespaceTypeDefinition is Dummy) return namespaceTypeDefinition;
      var mutableNamespaceTypeDefinition = namespaceTypeDefinition as NamespaceTypeDefinition;
      if (mutableNamespaceTypeDefinition == null) return namespaceTypeDefinition;
      this.RewriteChildren(mutableNamespaceTypeDefinition);
      return mutableNamespaceTypeDefinition;
    }

    /// <summary>
    /// Rewrites the given namespace type reference.
    /// </summary>
    public virtual INamespaceTypeReference Rewrite(INamespaceTypeReference namespaceTypeReference) {
      Contract.Requires(namespaceTypeReference != null);
      Contract.Ensures(Contract.Result<INamespaceTypeReference>() != null);

      if (namespaceTypeReference is Dummy) return namespaceTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(namespaceTypeReference, out result)) return (INamespaceTypeReference)result;
      var mutableNamespaceTypeReference = namespaceTypeReference as NamespaceTypeReference;
      if (mutableNamespaceTypeReference == null || mutableNamespaceTypeReference.IsFrozen) {
        if (this.shallowCopier == null || namespaceTypeReference is NamespaceTypeDefinition) return namespaceTypeReference;
        mutableNamespaceTypeReference = this.shallowCopier.Copy(namespaceTypeReference);
      }
      this.referenceRewrites[namespaceTypeReference] = mutableNamespaceTypeReference;
      this.RewriteChildren(mutableNamespaceTypeReference);
      return mutableNamespaceTypeReference;
    }

    /// <summary>
    /// Rewrites the nested alias for type
    /// </summary>
    public virtual INestedAliasForType Rewrite(INestedAliasForType nestedAliasForType) {
      Contract.Requires(nestedAliasForType != null);
      Contract.Ensures(Contract.Result<INestedAliasForType>() != null);

      if (nestedAliasForType is Dummy) return nestedAliasForType;
      var mutableNestedAliasForType = nestedAliasForType as NestedAliasForType;
      if (mutableNestedAliasForType == null) return nestedAliasForType;
      this.RewriteChildren(mutableNestedAliasForType);
      return mutableNestedAliasForType;
    }

    /// <summary>
    /// Rewrites the given nested type definition.
    /// </summary>
    public virtual INestedTypeDefinition Rewrite(INestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);
      Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);

      if (nestedTypeDefinition is Dummy) return nestedTypeDefinition;
      nestedTypeDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as INestedTypeDefinition)??nestedTypeDefinition;
    }

    /// <summary>
    /// Rewrites the given unspecialized nested type definition.
    /// </summary>
    protected virtual INestedTypeDefinition RewriteUnspecialized(INestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);
      Contract.Requires(!(nestedTypeDefinition is ISpecializedNestedTypeDefinition));
      Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);

      if (nestedTypeDefinition is Dummy) return nestedTypeDefinition;
      var mutableNestedTypeDefinition = nestedTypeDefinition as NestedTypeDefinition;
      if (mutableNestedTypeDefinition == null) return nestedTypeDefinition;
      this.RewriteChildren(mutableNestedTypeDefinition);
      return mutableNestedTypeDefinition;
    }

    /// <summary>
    /// Rewrites the given namespace type reference.
    /// </summary>
    public virtual INestedTypeReference Rewrite(INestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Ensures(Contract.Result<INestedTypeReference>() != null);

      if (nestedTypeReference is Dummy) return nestedTypeReference;
      nestedTypeReference.DispatchAsReference(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as INestedTypeReference)??nestedTypeReference;
    }

    /// <summary>
    /// Rewrites the given namespace type reference.
    /// </summary>
    public virtual INestedTypeReference RewriteUnspecialized(INestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Requires(!(nestedTypeReference is ISpecializedNestedTypeReference));
      Contract.Ensures(Contract.Result<INestedTypeReference>() != null);

      if (nestedTypeReference is Dummy) return nestedTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(nestedTypeReference, out result)) return (INestedTypeReference)result;
      var mutableNestedTypeReference = nestedTypeReference as NestedTypeReference;
      if (mutableNestedTypeReference == null || mutableNestedTypeReference.IsFrozen) {
        if (this.shallowCopier == null || nestedTypeReference is NestedTypeDefinition) return nestedTypeReference;
        mutableNestedTypeReference = this.shallowCopier.Copy(nestedTypeReference);
      }
      this.referenceRewrites[nestedTypeReference] = mutableNestedTypeReference;
      this.RewriteChildren(mutableNestedTypeReference);
      return mutableNestedTypeReference;
    }

    /// <summary>
    /// Rewrites the specified nested unit namespace.
    /// </summary>
    public virtual INestedUnitNamespace Rewrite(INestedUnitNamespace nestedUnitNamespace) {
      Contract.Requires(nestedUnitNamespace != null);
      Contract.Ensures(Contract.Result<INestedUnitNamespace>() != null);

      if (nestedUnitNamespace is Dummy) return nestedUnitNamespace;
      var mutableNestedUnitNamespace = nestedUnitNamespace as NestedUnitNamespace;
      if (mutableNestedUnitNamespace == null) return nestedUnitNamespace;
      this.RewriteChildren(mutableNestedUnitNamespace);
      return mutableNestedUnitNamespace;
    }

    /// <summary>
    /// Rewrites the specified reference to a nested unit namespace.
    /// </summary>
    public virtual INestedUnitNamespaceReference Rewrite(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      Contract.Requires(nestedUnitNamespaceReference != null);
      Contract.Ensures(Contract.Result<INestedUnitNamespaceReference>() != null);

      if (nestedUnitNamespaceReference is Dummy) return nestedUnitNamespaceReference;
      object result;
      if (this.referenceRewrites.TryGetValue(nestedUnitNamespaceReference, out result)) return (INestedUnitNamespaceReference)result;
      var mutableNestedUnitNamespaceReference = nestedUnitNamespaceReference as NestedUnitNamespaceReference;
      if (mutableNestedUnitNamespaceReference == null || mutableNestedUnitNamespaceReference.IsFrozen) {
        if (this.shallowCopier == null || nestedUnitNamespaceReference is NestedUnitNamespace) return nestedUnitNamespaceReference;
        mutableNestedUnitNamespaceReference = this.shallowCopier.Copy(nestedUnitNamespaceReference);
      }
      this.referenceRewrites[nestedUnitNamespaceReference] = mutableNestedUnitNamespaceReference;
      this.RewriteChildren(mutableNestedUnitNamespaceReference);
      return mutableNestedUnitNamespaceReference;
    }

    /// <summary>
    /// Rewrites the specified operation.
    /// </summary>
    public virtual IOperation Rewrite(IOperation operation) {
      Contract.Requires(operation != null);
      Contract.Ensures(Contract.Result<IOperation>() != null);

      if (operation is Dummy) return operation;
      var mutableOperation = operation as Operation;
      if (mutableOperation == null) return operation;
      this.RewriteChildren(mutableOperation);
      return mutableOperation;
    }

    /// <summary>
    /// Rewrites the specified operation exception information.
    /// </summary>
    public virtual IOperationExceptionInformation Rewrite(IOperationExceptionInformation operationExceptionInformation) {
      Contract.Requires(operationExceptionInformation != null);
      Contract.Ensures(Contract.Result<IOperationExceptionInformation>() != null);

      if (operationExceptionInformation is Dummy) return operationExceptionInformation;
      var mutableOperationExceptionInformation = operationExceptionInformation as OperationExceptionInformation;
      if (mutableOperationExceptionInformation == null) return operationExceptionInformation;
      this.RewriteChildren(mutableOperationExceptionInformation);
      return mutableOperationExceptionInformation;
    }

    /// <summary>
    /// Rewrites the given parameter definition.
    /// </summary>
    public virtual IParameterDefinition Rewrite(IParameterDefinition parameterDefinition) {
      Contract.Requires(parameterDefinition != null);
      Contract.Ensures(Contract.Result<IParameterDefinition>() != null);

      if (parameterDefinition is Dummy) return parameterDefinition;
      var mutableParameterDefinition = parameterDefinition as ParameterDefinition;
      if (mutableParameterDefinition == null) return parameterDefinition;
      this.RewriteChildren(mutableParameterDefinition);
      return mutableParameterDefinition;
    }

    /// <summary>
    /// Rewrites the given parameter type information.
    /// </summary>
    public virtual IParameterTypeInformation Rewrite(IParameterTypeInformation parameterTypeInformation) {
      Contract.Requires(parameterTypeInformation != null);
      Contract.Ensures(Contract.Result<IParameterTypeInformation>() != null);

      if (parameterTypeInformation is Dummy) return parameterTypeInformation;
      var mutableParameterTypeInformation = parameterTypeInformation as ParameterTypeInformation;
      if (mutableParameterTypeInformation == null) return parameterTypeInformation;
      this.RewriteChildren(mutableParameterTypeInformation);
      return mutableParameterTypeInformation;
    }

    /// <summary>
    /// Rewrites the given PE section.
    /// </summary>
    public virtual IPESection Rewrite(IPESection peSection) {
      Contract.Requires(peSection != null);
      Contract.Ensures(Contract.Result<IPESection>() != null);

      if (peSection is Dummy) return peSection;
      var mutablePESection = peSection as PESection;
      if (mutablePESection == null) return peSection;
      this.RewriteChildren(mutablePESection);
      return mutablePESection;
    }

    /// <summary>
    /// Rewrites the specified platform invoke information.
    /// </summary>
    public virtual IPlatformInvokeInformation Rewrite(IPlatformInvokeInformation platformInvokeInformation) {
      Contract.Requires(platformInvokeInformation != null);
      Contract.Ensures(Contract.Result<IPlatformInvokeInformation>() != null);

      if (platformInvokeInformation is Dummy) return platformInvokeInformation;
      var mutablePlatformInvokeInformation = platformInvokeInformation as PlatformInvokeInformation;
      if (mutablePlatformInvokeInformation == null) return platformInvokeInformation;
      this.RewriteChildren(mutablePlatformInvokeInformation);
      return mutablePlatformInvokeInformation;
    }

    /// <summary>
    /// Rewrites the given pointer type reference.
    /// </summary>
    public virtual IPointerTypeReference Rewrite(IPointerTypeReference pointerTypeReference) {
      Contract.Requires(pointerTypeReference != null);
      Contract.Ensures(Contract.Result<IPointerTypeReference>() != null);

      if (pointerTypeReference is Dummy) return pointerTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(pointerTypeReference, out result)) return (IPointerTypeReference)result;
      var mutablePointerTypeReference = pointerTypeReference as PointerTypeReference;
      if (mutablePointerTypeReference == null || mutablePointerTypeReference.IsFrozen) {
        if (this.shallowCopier == null) return pointerTypeReference;
        mutablePointerTypeReference = this.shallowCopier.Copy(pointerTypeReference);
      }
      this.referenceRewrites[pointerTypeReference] = mutablePointerTypeReference;
      this.RewriteChildren(mutablePointerTypeReference);
      return mutablePointerTypeReference;
    }

    /// <summary>
    /// Rewrites the given property definition.
    /// </summary>
    public virtual IPropertyDefinition Rewrite(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Ensures(Contract.Result<IPropertyDefinition>() != null);

      if (propertyDefinition is Dummy) return propertyDefinition;
      propertyDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IPropertyDefinition)??propertyDefinition;
    }

    /// <summary>
    /// Rewrites the given property definition.
    /// </summary>
    protected virtual IPropertyDefinition RewriteUnspecialized(IPropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);
      Contract.Requires(!(propertyDefinition is ISpecializedPropertyDefinition));
      Contract.Ensures(Contract.Result<IPropertyDefinition>() != null);

      if (propertyDefinition is Dummy) return propertyDefinition;
      var mutablePropertyDefinition = propertyDefinition as PropertyDefinition;
      if (mutablePropertyDefinition == null) return propertyDefinition;
      this.RewriteChildren(mutablePropertyDefinition);
      return mutablePropertyDefinition;
    }

    /// <summary>
    /// Rewrites the given reference to a manifest resource.
    /// </summary>
    public virtual IResourceReference Rewrite(IResourceReference resourceReference) {
      Contract.Requires(resourceReference != null);
      Contract.Ensures(Contract.Result<IResourceReference>() != null);

      if (resourceReference is Dummy) return resourceReference;
      var mutableResourceReference = resourceReference as ResourceReference;
      if (mutableResourceReference == null) return resourceReference;
      this.RewriteChildren(mutableResourceReference);
      return mutableResourceReference;
    }

    /// <summary>
    /// Rewrites the given root unit namespace.
    /// </summary>
    public virtual IRootUnitNamespace Rewrite(IRootUnitNamespace rootUnitNamespace) {
      Contract.Requires(rootUnitNamespace != null);
      Contract.Ensures(Contract.Result<IRootUnitNamespace>() != null);

      if (rootUnitNamespace is Dummy) return rootUnitNamespace;
      var mutableRootUnitNamespace = rootUnitNamespace as RootUnitNamespace;
      if (mutableRootUnitNamespace == null) return rootUnitNamespace;
      this.RewriteChildren(mutableRootUnitNamespace);
      return mutableRootUnitNamespace;
    }

    /// <summary>
    /// Rewrites the given reference to a root unit namespace.
    /// </summary>
    public virtual IRootUnitNamespaceReference Rewrite(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      Contract.Requires(rootUnitNamespaceReference != null);
      Contract.Ensures(Contract.Result<IRootUnitNamespaceReference>() != null);

      if (rootUnitNamespaceReference is Dummy) return rootUnitNamespaceReference;
      object result;
      if (this.referenceRewrites.TryGetValue(rootUnitNamespaceReference, out result)) return (IRootUnitNamespaceReference)result;
      var mutableRootUnitNamespaceReference = rootUnitNamespaceReference as RootUnitNamespaceReference;
      if (mutableRootUnitNamespaceReference == null || mutableRootUnitNamespaceReference.IsFrozen) {
        if (this.shallowCopier == null || rootUnitNamespaceReference is RootUnitNamespace) return rootUnitNamespaceReference;
        mutableRootUnitNamespaceReference = this.shallowCopier.Copy(rootUnitNamespaceReference);
      }
      this.referenceRewrites[rootUnitNamespaceReference] = mutableRootUnitNamespaceReference;
      this.RewriteChildren(mutableRootUnitNamespaceReference);
      return mutableRootUnitNamespaceReference;
    }

    /// <summary>
    /// Rewrites the given security attribute.
    /// </summary>
    public virtual ISecurityAttribute Rewrite(ISecurityAttribute securityAttribute) {
      Contract.Requires(securityAttribute != null);
      Contract.Ensures(Contract.Result<ISecurityAttribute>() != null);

      if (securityAttribute is Dummy) return securityAttribute;
      var mutableSecurityAttribute = securityAttribute as SecurityAttribute;
      if (mutableSecurityAttribute == null) return securityAttribute;
      this.RewriteChildren(mutableSecurityAttribute);
      return mutableSecurityAttribute;
    }

    /// <summary>
    /// Rewrites the given specialized event definition.
    /// </summary>
    public virtual IEventDefinition Rewrite(ISpecializedEventDefinition specializedEventDefinition) {
      Contract.Requires(specializedEventDefinition != null);
      Contract.Ensures(Contract.Result<IEventDefinition>() != null);

      var mutableSpecializedEventDefinition = specializedEventDefinition as SpecializedEventDefinition;
      if (mutableSpecializedEventDefinition == null) return specializedEventDefinition;
      this.RewriteChildren(mutableSpecializedEventDefinition);
      return mutableSpecializedEventDefinition;
    }

    /// <summary>
    /// Rewrites the given specialized field definition.
    /// </summary>
    public virtual IFieldDefinition Rewrite(ISpecializedFieldDefinition specializedFieldDefinition) {
      Contract.Requires(specializedFieldDefinition != null);
      Contract.Ensures(Contract.Result<IFieldDefinition>() != null);

      var mutableSpecializedFieldDefinition = specializedFieldDefinition as SpecializedFieldDefinition;
      if (mutableSpecializedFieldDefinition == null) return specializedFieldDefinition;
      this.RewriteChildren(mutableSpecializedFieldDefinition);
      return mutableSpecializedFieldDefinition;
    }

    /// <summary>
    /// Rewrites the given specialized field reference.
    /// </summary>
    public virtual IFieldReference Rewrite(ISpecializedFieldReference specializedFieldReference) {
      Contract.Requires(specializedFieldReference != null);
      Contract.Ensures(Contract.Result<IFieldReference>() != null);

      if (specializedFieldReference is Dummy) return specializedFieldReference;
      object result;
      if (this.referenceRewrites.TryGetValue(specializedFieldReference, out result)) return (ISpecializedFieldReference)result;
      var mutableSpecializedFieldReference = specializedFieldReference as SpecializedFieldReference;
      if (mutableSpecializedFieldReference == null || mutableSpecializedFieldReference.IsFrozen) {
        if (this.shallowCopier == null) return specializedFieldReference;
        mutableSpecializedFieldReference = this.shallowCopier.Copy(specializedFieldReference);
      }
      this.referenceRewrites[specializedFieldReference] = mutableSpecializedFieldReference;
      this.RewriteChildren(mutableSpecializedFieldReference);
      return mutableSpecializedFieldReference;
    }

    /// <summary>
    /// Rewrites the given specialized method definition.
    /// </summary>
    public virtual IMethodDefinition Rewrite(ISpecializedMethodDefinition specializedMethodDefinition) {
      Contract.Requires(specializedMethodDefinition != null);
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      var mutableSpecializedMethodDefinition = specializedMethodDefinition as SpecializedMethodDefinition;
      if (mutableSpecializedMethodDefinition == null) return specializedMethodDefinition;
      this.RewriteChildren(mutableSpecializedMethodDefinition);
      return mutableSpecializedMethodDefinition;
    }

    /// <summary>
    /// Rewrites the given specialized method reference.
    /// </summary>
    public virtual IMethodReference Rewrite(ISpecializedMethodReference specializedMethodReference) {
      Contract.Requires(specializedMethodReference != null);
      Contract.Ensures(Contract.Result<IMethodReference>() != null);

      if (specializedMethodReference is Dummy) return specializedMethodReference;
      object result;
      if (this.referenceRewrites.TryGetValue(specializedMethodReference, out result)) return (IMethodReference)result;
      var mutableSpecializedMethodReference = specializedMethodReference as SpecializedMethodReference;
      if (mutableSpecializedMethodReference == null || mutableSpecializedMethodReference.IsFrozen) {
        if (this.shallowCopier == null) return specializedMethodReference;
        mutableSpecializedMethodReference = this.shallowCopier.Copy(specializedMethodReference);
      }
      this.referenceRewrites[specializedMethodReference] = mutableSpecializedMethodReference;
      this.RewriteChildren(mutableSpecializedMethodReference);
      return mutableSpecializedMethodReference;
    }

    /// <summary>
    /// Rewrites the given specialized nested type definition.
    /// </summary>
    public virtual INestedTypeDefinition Rewrite(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
      Contract.Requires(specializedNestedTypeDefinition != null);
      Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);

      var mutableSpecializedNestedTypeDefinition = specializedNestedTypeDefinition as SpecializedNestedTypeDefinition;
      if (mutableSpecializedNestedTypeDefinition == null) return specializedNestedTypeDefinition;
      this.RewriteChildren(mutableSpecializedNestedTypeDefinition);
      return mutableSpecializedNestedTypeDefinition;
    }

    /// <summary>
    /// Rewrites the given specialized nested type reference.
    /// </summary>
    public virtual INestedTypeReference Rewrite(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      Contract.Requires(specializedNestedTypeReference != null);
      Contract.Ensures(Contract.Result<INestedTypeReference>() != null);

      if (specializedNestedTypeReference is Dummy) return specializedNestedTypeReference;
      object result;
      if (this.referenceRewrites.TryGetValue(specializedNestedTypeReference, out result)) return (ISpecializedNestedTypeReference)result;
      var mutableSpecializedNestedTypeReference = specializedNestedTypeReference as SpecializedNestedTypeReference;
      if (mutableSpecializedNestedTypeReference == null || mutableSpecializedNestedTypeReference.IsFrozen) {
        if (this.shallowCopier == null || specializedNestedTypeReference is SpecializedNestedTypeDefinition) return specializedNestedTypeReference;
        mutableSpecializedNestedTypeReference = this.shallowCopier.Copy(specializedNestedTypeReference);
      }
      this.referenceRewrites[specializedNestedTypeReference] = mutableSpecializedNestedTypeReference;
      this.RewriteChildren(mutableSpecializedNestedTypeReference);
      return mutableSpecializedNestedTypeReference;
    }

    /// <summary>
    /// Rewrites the given specialized property definition.
    /// </summary>
    public virtual IPropertyDefinition Rewrite(ISpecializedPropertyDefinition specializedPropertyDefinition) {
      Contract.Requires(specializedPropertyDefinition != null);
      Contract.Ensures(Contract.Result<IPropertyDefinition>() != null);

      if (specializedPropertyDefinition is Dummy) return specializedPropertyDefinition;
      var mutableSpecializedPropertyDefinition = specializedPropertyDefinition as SpecializedPropertyDefinition;
      if (mutableSpecializedPropertyDefinition == null) return specializedPropertyDefinition;
      this.RewriteChildren(mutableSpecializedPropertyDefinition);
      return mutableSpecializedPropertyDefinition;
    }

    /// <summary>
    /// Rewrites the given type definition.
    /// </summary>
    public virtual ITypeDefinition Rewrite(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);
      Contract.Requires(!(typeDefinition is IGenericTypeInstance), "Generic type instances should be reconstructed, not rewritten.");
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);

      if (typeDefinition is Dummy) return typeDefinition;
      typeDefinition.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as ITypeDefinition)??typeDefinition;
    }

    /// <summary>
    /// Rewrites the specified type member.
    /// </summary>
    public virtual ITypeDefinitionMember Rewrite(ITypeDefinitionMember typeMember) {
      Contract.Requires(typeMember != null);
      Contract.Ensures(Contract.Result<ITypeDefinitionMember>() != null);

      if (typeMember is Dummy) return typeMember;
      typeMember.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as ITypeDefinitionMember)??typeMember;
    }

    /// <summary>
    /// Rewrites the specified type reference.
    /// </summary>
    public virtual ITypeReference Rewrite(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      if (typeReference is Dummy) return typeReference;
      typeReference.DispatchAsReference(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as ITypeReference)??typeReference;
    }

    /// <summary>
    /// Rewrites the specified unit.
    /// </summary>
    public virtual IUnit Rewrite(IUnit unit) {
      Contract.Requires(unit != null);
      Contract.Ensures(Contract.Result<IUnit>() != null);

      if (unit is Dummy) return unit;
      unit.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IUnit)??unit;
    }

    /// <summary>
    /// Rewrites the specified unit namespace.
    /// </summary>
    public virtual IUnitNamespace Rewrite(IUnitNamespace unitNamespace) {
      Contract.Requires(unitNamespace != null);
      Contract.Ensures(Contract.Result<IUnitNamespace>() != null);

      if (unitNamespace is Dummy) return unitNamespace;
      unitNamespace.Dispatch(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IUnitNamespace)??unitNamespace;
    }

    /// <summary>
    /// Rewrites the specified reference to a unit namespace.
    /// </summary>
    public virtual IUnitNamespaceReference Rewrite(IUnitNamespaceReference unitNamespaceReference) {
      Contract.Requires(unitNamespaceReference != null);
      Contract.Ensures(Contract.Result<IUnitNamespaceReference>() != null);

      if (unitNamespaceReference is Dummy) return unitNamespaceReference;
      unitNamespaceReference.DispatchAsReference(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IUnitNamespaceReference)??unitNamespaceReference;
    }

    /// <summary>
    /// Rewrites the specified unit reference.
    /// </summary>
    public virtual IUnitReference Rewrite(IUnitReference unitReference) {
      Contract.Requires(unitReference != null);
      Contract.Ensures(Contract.Result<IUnitReference>() != null);

      if (unitReference is Dummy) return unitReference;
      unitReference.DispatchAsReference(this.dispatchingVisitor);
      return (this.dispatchingVisitor.result as IUnitReference)??unitReference;
    }

    /// <summary>
    /// Rewrites the given Win32 resource.
    /// </summary>
    public virtual IWin32Resource Rewrite(IWin32Resource win32Resource) {
      Contract.Requires(win32Resource != null);
      Contract.Ensures(Contract.Result<IWin32Resource>() != null);

      if (win32Resource is Dummy) return win32Resource;
      var mutableWin32Resource = win32Resource as Win32Resource;
      if (mutableWin32Resource == null) return win32Resource;
      this.RewriteChildren(mutableWin32Resource);
      return mutableWin32Resource;
    }

    /// <summary>
    /// Rewrites the list of aliases for types.
    /// </summary>
    public virtual List<IAliasForType>/*?*/ Rewrite(List<IAliasForType>/*?*/ aliasesForTypes) {
      if (aliasesForTypes == null) return null;
      for (int i = 0, n = aliasesForTypes.Count; i < n; i++)
        aliasesForTypes[i] = this.Rewrite(aliasesForTypes[i]);
      return aliasesForTypes;
    }

    /// <summary>
    /// Rewrites the list of members of a type alias.
    /// </summary>
    public virtual List<IAliasMember>/*?*/ Rewrite(List<IAliasMember>/*?*/ aliasMembers) {
      if (aliasMembers == null) return null;
      for (int i = 0, n = aliasMembers.Count; i < n; i++)
        aliasMembers[i] = this.Rewrite(aliasMembers[i]);
      return aliasMembers;
    }

    /// <summary>
    /// Rewrites the specified assembly references.
    /// </summary>
    public virtual List<IAssemblyReference>/*?*/ Rewrite(List<IAssemblyReference>/*?*/ assemblyReferences) {
      if (assemblyReferences == null) return null;
      for (int i = 0, n = assemblyReferences.Count; i < n; i++)
        assemblyReferences[i] = this.Rewrite(assemblyReferences[i]);
      return assemblyReferences;
    }

    /// <summary>
    /// Rewrites the specified custom attributes.
    /// </summary>
    public virtual List<ICustomAttribute>/*?*/ Rewrite(List<ICustomAttribute>/*?*/ customAttributes) {
      if (customAttributes == null) return null;
      for (int i = 0, n = customAttributes.Count; i < n; i++)
        customAttributes[i] = this.Rewrite(customAttributes[i]);
      return customAttributes;
    }

    /// <summary>
    /// Rewrites the specified custom modifiers.
    /// </summary>
    public virtual List<ICustomModifier>/*?*/ Rewrite(List<ICustomModifier>/*?*/ customModifiers) {
      if (customModifiers == null) return null;
      for (int i = 0, n = customModifiers.Count; i < n; i++)
        customModifiers[i] = this.Rewrite(customModifiers[i]);
      return customModifiers;
    }

    /// <summary>
    /// Rewrites the specified events.
    /// </summary>
    public virtual List<IEventDefinition>/*?*/ Rewrite(List<IEventDefinition>/*?*/ events) {
      if (events == null) return null;
      for (int i = 0, n = events.Count; i < n; i++)
        events[i] = this.Rewrite(events[i]);
      return events;
    }

    /// <summary>
    /// Rewrites the specified fields.
    /// </summary>
    public virtual List<IFieldDefinition>/*?*/ Rewrite(List<IFieldDefinition>/*?*/ fields) {
      if (fields == null) return null;
      for (int i = 0, n = fields.Count; i < n; i++)
        fields[i] = this.Rewrite(fields[i]);
      return fields;
    }

    /// <summary>
    /// Rewrites the specified file references.
    /// </summary>
    public virtual List<IFileReference>/*?*/ Rewrite(List<IFileReference>/*?*/ fileReferences) {
      if (fileReferences == null) return null;
      for (int i = 0, n = fileReferences.Count; i < n; i++)
        fileReferences[i] = this.Rewrite(fileReferences[i]);
      return fileReferences;
    }

    /// <summary>
    /// Rewrites the specified generic parameters.
    /// </summary>
    public virtual List<IGenericMethodParameter>/*?*/ Rewrite(List<IGenericMethodParameter>/*?*/ genericParameters) {
      if (genericParameters == null) return null;
      for (int i = 0, n = genericParameters.Count; i < n; i++)
        genericParameters[i] = this.Rewrite(genericParameters[i]);
      return genericParameters;
    }

    /// <summary>
    /// Rewrites the specified generic parameters.
    /// </summary>
    public virtual List<IGenericTypeParameter>/*?*/ Rewrite(List<IGenericTypeParameter>/*?*/ genericParameters) {
      if (genericParameters == null) return null;
      for (int i = 0, n = genericParameters.Count; i < n; i++)
        genericParameters[i] = this.Rewrite(genericParameters[i]);
      return genericParameters;
    }

    /// <summary>
    /// Rewrites the specified local definitions.
    /// </summary>
    public virtual List<ILocalDefinition>/*?*/ Rewrite(List<ILocalDefinition>/*?*/ localDefinitions) {
      if (localDefinitions == null) return null;
      for (int i = 0, n = localDefinitions.Count; i < n; i++)
        localDefinitions[i] = this.Rewrite(localDefinitions[i]);
      return localDefinitions;
    }

    /// <summary>
    /// Rewrites the specified expressions.
    /// </summary>
    public virtual List<IMetadataExpression>/*?*/ Rewrite(List<IMetadataExpression>/*?*/ expressions) {
      if (expressions == null) return null;
      for (int i = 0, n = expressions.Count; i < n; i++)
        expressions[i] = this.Rewrite(expressions[i]);
      return expressions;
    }

    /// <summary>
    /// Rewrites the specified named arguments.
    /// </summary>
    public virtual List<IMetadataNamedArgument>/*?*/ Rewrite(List<IMetadataNamedArgument>/*?*/ namedArguments) {
      if (namedArguments == null) return null;
      for (int i = 0, n = namedArguments.Count; i < n; i++)
        namedArguments[i] = this.Rewrite(namedArguments[i]);
      return namedArguments;
    }

    /// <summary>
    /// Rewrites the specified methods.
    /// </summary>
    public virtual List<IMethodDefinition>/*?*/ Rewrite(List<IMethodDefinition>/*?*/ methods) {
      if (methods == null) return null;
      for (int i = 0, n = methods.Count; i < n; i++)
        methods[i] = this.Rewrite(methods[i]);
      return methods;
    }

    /// <summary>
    /// Rewrites the specified method implementations.
    /// </summary>
    public virtual List<IMethodImplementation>/*?*/ Rewrite(List<IMethodImplementation>/*?*/ methodImplementations) {
      if (methodImplementations == null) return null;
      for (int i = 0, n = methodImplementations.Count; i < n; i++)
        methodImplementations[i] = this.Rewrite(methodImplementations[i]);
      return methodImplementations;
    }

    /// <summary>
    /// Rewrites the specified method references.
    /// </summary>
    public virtual List<IMethodReference>/*?*/ Rewrite(List<IMethodReference>/*?*/ methodReferences) {
      if (methodReferences == null) return null;
      for (int i = 0, n = methodReferences.Count; i < n; i++)
        methodReferences[i] = this.Rewrite(methodReferences[i]);
      return methodReferences;
    }

    /// <summary>
    /// Rewrites the specified modules.
    /// </summary>
    public virtual List<IModule>/*?*/ Rewrite(List<IModule>/*?*/ modules) {
      if (modules == null) return null;
      for (int i = 0, n = modules.Count; i < n; i++)
        modules[i] = this.Rewrite(modules[i]);
      return modules;
    }

    /// <summary>
    /// Rewrites the specified module references.
    /// </summary>
    public virtual List<IModuleReference>/*?*/ Rewrite(List<IModuleReference>/*?*/ moduleReferences) {
      if (moduleReferences == null) return null;
      for (int i = 0, n = moduleReferences.Count; i < n; i++)
        moduleReferences[i] = this.Rewrite(moduleReferences[i]);
      return moduleReferences;
    }

    /// <summary>
    /// Rewrites the specified types.
    /// </summary>
    public virtual List<INamedTypeDefinition>/*?*/ Rewrite(List<INamedTypeDefinition>/*?*/ types) {
      for (int i = 0, n = types.Count; i < n; i++)
        types[i] = (INamedTypeDefinition)this.Rewrite(types[i]);
      return types;
    }

    /// <summary>
    /// Rewrites the specified namespace members.
    /// </summary>
    public virtual List<INamespaceMember>/*?*/ Rewrite(List<INamespaceMember>/*?*/ namespaceMembers) {
      if (namespaceMembers == null) return null;
      for (int i = 0, n = namespaceMembers.Count; i < n; i++)
        namespaceMembers[i] = this.Rewrite(namespaceMembers[i]);
      return namespaceMembers;
    }

    /// <summary>
    /// Rewrites the specified nested types.
    /// </summary>
    public virtual List<INestedTypeDefinition>/*?*/ Rewrite(List<INestedTypeDefinition>/*?*/ nestedTypes) {
      if (nestedTypes == null) return null;
      for (int i = 0, n = nestedTypes.Count; i < n; i++)
        nestedTypes[i] = this.Rewrite(nestedTypes[i]);
      return nestedTypes;
    }

    /// <summary>
    /// Rewrites the specified operations.
    /// </summary>
    public virtual List<IOperation>/*?*/ Rewrite(List<IOperation>/*?*/ operations) {
      if (operations == null) return null;
      for (int i = 0, n = operations.Count; i < n; i++)
        operations[i] = this.Rewrite(operations[i]);
      return operations;
    }

    /// <summary>
    /// Rewrites the specified operation exception informations.
    /// </summary>
    public virtual List<IOperationExceptionInformation>/*?*/ Rewrite(List<IOperationExceptionInformation>/*?*/ operationExceptionInformations) {
      if (operationExceptionInformations == null) return null;
      for (int i = 0, n = operationExceptionInformations.Count; i < n; i++)
        operationExceptionInformations[i] = this.Rewrite(operationExceptionInformations[i]);
      return operationExceptionInformations;
    }

    /// <summary>
    /// Rewrites the specified parameters.
    /// </summary>
    public virtual List<IParameterDefinition>/*?*/ Rewrite(List<IParameterDefinition>/*?*/ parameters) {
      if (parameters == null) return null;
      for (int i = 0, n = parameters.Count; i < n; i++)
        parameters[i] = this.Rewrite(parameters[i]);
      return parameters;
    }

    /// <summary>
    /// Rewrites the specified parameter type informations.
    /// </summary>
    public virtual List<IParameterTypeInformation>/*?*/ Rewrite(List<IParameterTypeInformation>/*?*/ parameterTypeInformations) {
      if (parameterTypeInformations == null) return null;
      for (int i = 0, n = parameterTypeInformations.Count; i < n; i++)
        parameterTypeInformations[i] = this.Rewrite(parameterTypeInformations[i]);
      return parameterTypeInformations;
    }

    /// <summary>
    /// Rewrites the specified PE sections.
    /// </summary>
    public virtual List<IPESection>/*?*/ Rewrite(List<IPESection>/*?*/ peSections) {
      if (peSections == null) return null;
      for (int i = 0, n = peSections.Count; i < n; i++)
        peSections[i] = this.Rewrite(peSections[i]);
      return peSections;
    }

    /// <summary>
    /// Rewrites the specified properties.
    /// </summary>
    public virtual List<IPropertyDefinition>/*?*/ Rewrite(List<IPropertyDefinition>/*?*/ properties) {
      if (properties == null) return null;
      for (int i = 0, n = properties.Count; i < n; i++)
        properties[i] = this.Rewrite(properties[i]);
      return properties;
    }

    /// <summary>
    /// Rewrites the specified resource references.
    /// </summary>
    public virtual List<IResourceReference>/*?*/ Rewrite(List<IResourceReference>/*?*/ resourceReferences) {
      if (resourceReferences == null) return null;
      for (int i = 0, n = resourceReferences.Count; i < n; i++)
        resourceReferences[i] = this.Rewrite(resourceReferences[i]);
      return resourceReferences;
    }

    /// <summary>
    /// Rewrites the specified security attributes.
    /// </summary>
    public virtual List<ISecurityAttribute>/*?*/ Rewrite(List<ISecurityAttribute>/*?*/ securityAttributes) {
      if (securityAttributes == null) return null;
      for (int i = 0, n = securityAttributes.Count; i < n; i++)
        securityAttributes[i] = this.Rewrite(securityAttributes[i]);
      return securityAttributes;
    }

    /// <summary>
    /// Rewrites the specified type members.
    /// </summary>
    /// <remarks>Not used by the rewriter itself.</remarks>
    public virtual List<ITypeDefinitionMember>/*?*/ Rewrite(List<ITypeDefinitionMember>/*?*/ typeMembers) {
      if (typeMembers == null) return null;
      for (int i = 0, n = typeMembers.Count; i < n; i++)
        typeMembers[i] = this.Rewrite(typeMembers[i]);
      return typeMembers;
    }

    /// <summary>
    /// Rewrites the specified type references.
    /// </summary>
    public virtual List<ITypeReference>/*?*/ Rewrite(List<ITypeReference>/*?*/ typeReferences) {
      if (typeReferences == null) return null;
      for (int i = 0, n = typeReferences.Count; i < n; i++)
        typeReferences[i] = this.Rewrite(typeReferences[i]);
      return typeReferences;
    }

    /// <summary>
    /// Rewrites the specified type references.
    /// </summary>
    public virtual List<IWin32Resource>/*?*/ Rewrite(List<IWin32Resource>/*?*/ win32Resources) {
      if (win32Resources == null) return null;
      for (int i = 0, n = win32Resources.Count; i < n; i++)
        win32Resources[i] = this.Rewrite(win32Resources[i]);
      return win32Resources;
    }

    /// <summary>
    /// Rewrites the children of the alias for type
    /// </summary>
    public virtual void RewriteChildren(AliasForType aliasForType) {
      Contract.Requires(aliasForType != null);

      aliasForType.AliasedType = this.Rewrite(aliasForType.AliasedType);
      aliasForType.Attributes = this.Rewrite(aliasForType.Attributes);
      aliasForType.Members = this.Rewrite(aliasForType.Members);
    }

    /// <summary>
    /// Rewrites the children of the array type reference.
    /// </summary>
    public virtual void RewriteChildren(ArrayTypeReference arrayTypeReference) {
      Contract.Requires(arrayTypeReference != null);
      Contract.Requires(!arrayTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)arrayTypeReference);
      arrayTypeReference.ElementType = this.Rewrite(arrayTypeReference.ElementType);
    }

    /// <summary>
    /// Rewrites the children of the given assembly.
    /// </summary>
    public virtual void RewriteChildren(Assembly assembly) {
      Contract.Requires(assembly != null);

      assembly.AssemblyAttributes = this.Rewrite(assembly.AssemblyAttributes);
      assembly.ExportedTypes = this.Rewrite(assembly.ExportedTypes);
      assembly.Files = this.Rewrite(assembly.Files);
      assembly.MemberModules = this.Rewrite(assembly.MemberModules);
      assembly.Resources = this.Rewrite(assembly.Resources);
      assembly.SecurityAttributes = this.Rewrite(assembly.SecurityAttributes);
      this.RewriteChildren((Module)assembly);
    }

    /// <summary>
    /// Rewrites the children of the given assembly reference.
    /// </summary>
    public virtual void RewriteChildren(AssemblyReference assemblyReference) {
      Contract.Requires(assemblyReference != null);
      Contract.Requires(!assemblyReference.IsFrozen);

      this.RewriteChildren((ModuleReference)assemblyReference);
    }

    /// <summary>
    /// Rewrites the children of the given custom attribute.
    /// </summary>
    public virtual void RewriteChildren(CustomAttribute customAttribute) {
      Contract.Requires(customAttribute != null);

      customAttribute.Arguments = this.Rewrite(customAttribute.Arguments);
      customAttribute.Constructor = this.Rewrite(customAttribute.Constructor);
      customAttribute.NamedArguments = this.Rewrite(customAttribute.NamedArguments);
    }

    /// <summary>
    /// Rewrites the children of the given custom modifier.
    /// </summary>
    public virtual void RewriteChildren(CustomModifier customModifier) {
      Contract.Requires(customModifier != null);

      customModifier.Modifier = this.Rewrite(customModifier.Modifier);
    }

    /// <summary>
    /// Rewrites the children of the given event definition.
    /// </summary>
    public virtual void RewriteChildren(EventDefinition eventDefinition) {
      Contract.Requires(eventDefinition != null);

      this.RewriteChildren((TypeDefinitionMember)eventDefinition);
      eventDefinition.Accessors = this.Rewrite(eventDefinition.Accessors);
      eventDefinition.Adder = this.Rewrite(eventDefinition.Adder);
      if (eventDefinition.Caller != null)
        eventDefinition.Caller = this.Rewrite(eventDefinition.Caller);
      eventDefinition.Remover = this.Rewrite(eventDefinition.Remover);
      eventDefinition.Type = this.Rewrite(eventDefinition.Type);
    }

    /// <summary>
    /// Rewrites the children of the given field definition.
    /// </summary>
    public virtual void RewriteChildren(FieldDefinition fieldDefinition) {
      Contract.Requires(fieldDefinition != null);

      this.RewriteChildren((TypeDefinitionMember)fieldDefinition);
      if (fieldDefinition.IsCompileTimeConstant)
        fieldDefinition.CompileTimeValue = this.Rewrite(fieldDefinition.CompileTimeValue);
      if (fieldDefinition.IsModified)
        fieldDefinition.CustomModifiers = this.Rewrite(fieldDefinition.CustomModifiers);
      if (fieldDefinition.IsMarshalledExplicitly)
        fieldDefinition.MarshallingInformation = this.Rewrite(fieldDefinition.MarshallingInformation);
      fieldDefinition.InternFactory = this.internFactory;
      fieldDefinition.Type = this.Rewrite(fieldDefinition.Type);
    }

    /// <summary>
    /// Rewrites the chidren of the given field reference.
    /// </summary>
    public virtual void RewriteChildren(FieldReference fieldReference) {
      Contract.Requires(fieldReference != null);
      Contract.Requires(!fieldReference.IsFrozen);

      fieldReference.Attributes = this.Rewrite(fieldReference.Attributes);
      fieldReference.ContainingType = this.Rewrite(fieldReference.ContainingType);
      if (fieldReference.IsModified)
        fieldReference.CustomModifiers = this.Rewrite(fieldReference.CustomModifiers);
      fieldReference.Type = this.Rewrite(fieldReference.Type);
    }

    /// <summary>
    /// Rewrites the children the given file reference.
    /// </summary>
    public virtual void RewriteChildren(FileReference fileReference) {
      Contract.Requires(fileReference != null);
    }

    /// <summary>
    /// Rewrites the children the given function pointer type reference.
    /// </summary>
    public virtual void RewriteChildren(FunctionPointerTypeReference functionPointerTypeReference) {
      Contract.Requires(functionPointerTypeReference != null);
      Contract.Requires(!functionPointerTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)functionPointerTypeReference);
      functionPointerTypeReference.ExtraArgumentTypes = this.Rewrite(functionPointerTypeReference.ExtraArgumentTypes);
      functionPointerTypeReference.Parameters = this.Rewrite(functionPointerTypeReference.Parameters);
      if (functionPointerTypeReference.ReturnValueIsModified)
        functionPointerTypeReference.ReturnValueCustomModifiers = this.Rewrite(functionPointerTypeReference.ReturnValueCustomModifiers);
      functionPointerTypeReference.Type = this.Rewrite(functionPointerTypeReference.Type);
    }

    /// <summary>
    /// Rewrites the children of the given generic method instance reference.
    /// </summary>
    public virtual void RewriteChildren(GenericMethodInstanceReference genericMethodInstanceReference) {
      Contract.Requires(genericMethodInstanceReference != null);
      Contract.Requires(!genericMethodInstanceReference.IsFrozen);

      this.RewriteChildren((MethodReference)genericMethodInstanceReference);
      genericMethodInstanceReference.GenericArguments = this.Rewrite(genericMethodInstanceReference.GenericArguments);
      genericMethodInstanceReference.GenericMethod = this.Rewrite(genericMethodInstanceReference.GenericMethod);
    }

    /// <summary>
    /// Rewrites the children of the given generic method parameter reference.
    /// </summary>
    public virtual void RewriteChildren(GenericMethodParameter genericMethodParameter) {
      Contract.Requires(genericMethodParameter != null);

      this.RewriteChildren((GenericParameter)genericMethodParameter);
    }

    /// <summary>
    /// Rewrites the children the given generic method parameter reference.
    /// </summary>
    public virtual void RewriteChildren(GenericMethodParameterReference genericMethodParameterReference) {
      Contract.Requires(genericMethodParameterReference != null);
      Contract.Requires(!genericMethodParameterReference.IsFrozen);

      this.RewriteChildren((TypeReference)genericMethodParameterReference);
      genericMethodParameterReference.DefiningMethod = this.Rewrite(genericMethodParameterReference.DefiningMethod);
    }

    /// <summary>
    /// Rewrites the children of the given generic parameter.
    /// </summary>
    public virtual void RewriteChildren(GenericParameter genericParameter) {
      Contract.Requires(genericParameter != null);

      this.RewriteChildren((NamedTypeDefinition)genericParameter);
      genericParameter.Constraints = this.Rewrite(genericParameter.Constraints);
    }

    /// <summary>
    /// Rewrites the children of the given generic type instance reference.
    /// </summary>
    public virtual void RewriteChildren(GenericTypeInstanceReference genericTypeInstanceReference) {
      Contract.Requires(genericTypeInstanceReference != null);
      Contract.Requires(!genericTypeInstanceReference.IsFrozen);

      this.RewriteChildren((TypeReference)genericTypeInstanceReference);
      genericTypeInstanceReference.GenericArguments = this.Rewrite(genericTypeInstanceReference.GenericArguments);
      genericTypeInstanceReference.GenericType = this.Rewrite(genericTypeInstanceReference.GenericType);
    }

    /// <summary>
    /// Rewrites the children of the given generic type parameter .
    /// </summary>
    public virtual void RewriteChildren(GenericTypeParameter genericTypeParameter) {
      Contract.Requires(genericTypeParameter != null);

      this.RewriteChildren((GenericParameter)genericTypeParameter);
    }

    /// <summary>
    /// Rewrites the children of the given generic type parameter reference.
    /// </summary>
    public virtual void RewriteChildren(GenericTypeParameterReference genericTypeParameterReference) {
      Contract.Requires(genericTypeParameterReference != null);
      Contract.Requires(!genericTypeParameterReference.IsFrozen);

      this.RewriteChildren((TypeReference)genericTypeParameterReference);
      genericTypeParameterReference.DefiningType = this.Rewrite(genericTypeParameterReference.DefiningType);
    }

    /// <summary>
    /// Rewrites the children of the given global field definition.
    /// </summary>
    public virtual void RewriteChildren(GlobalFieldDefinition globalFieldDefinition) {
      Contract.Requires(globalFieldDefinition != null);

      this.RewriteChildren((FieldDefinition)globalFieldDefinition);
    }

    /// <summary>
    /// Rewrites the children of the given global method definition.
    /// </summary>
    public virtual void RewriteChildren(GlobalMethodDefinition globalMethodDefinition) {
      Contract.Requires(globalMethodDefinition != null);

      this.RewriteChildren((MethodDefinition)globalMethodDefinition);
    }

    /// <summary>
    /// Rewrites the children of the specified local definition.
    /// </summary>
    public virtual void RewriteChildren(LocalDefinition localDefinition) {
      Contract.Requires(localDefinition != null);

      if (localDefinition.IsConstant)
        localDefinition.CompileTimeValue = this.Rewrite(localDefinition.CompileTimeValue);
      if (localDefinition.IsModified)
        localDefinition.CustomModifiers = this.Rewrite(localDefinition.CustomModifiers);
      localDefinition.Type = this.Rewrite(localDefinition.Type);
    }

    /// <summary>
    /// Rewrites the children of the given managed pointer type reference.
    /// </summary>
    public virtual void RewriteChildren(ManagedPointerTypeReference managedPointerTypeReference) {
      Contract.Requires(managedPointerTypeReference != null);
      Contract.Requires(!managedPointerTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)managedPointerTypeReference);
      managedPointerTypeReference.TargetType = this.Rewrite(managedPointerTypeReference.TargetType);
    }

    /// <summary>
    /// Rewrites the children of the given marshalling information.
    /// </summary>
    public virtual void RewriteChildren(MarshallingInformation marshallingInformation) {
      Contract.Requires(marshallingInformation != null);

      if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler)
        marshallingInformation.CustomMarshaller = this.Rewrite(marshallingInformation.CustomMarshaller);
      if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.SafeArray && 
      (marshallingInformation.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_DISPATCH ||
      marshallingInformation.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_UNKNOWN ||
      marshallingInformation.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_RECORD))
        marshallingInformation.SafeArrayElementUserDefinedSubtype = this.Rewrite(marshallingInformation.SafeArrayElementUserDefinedSubtype);
    }

    /// <summary>
    /// Rewrites the children of the given metadata constant.
    /// </summary>
    public virtual void RewriteChildren(MetadataConstant constant) {
      Contract.Requires(constant != null);

      this.RewriteChildren((MetadataExpression)constant);
      constant.Type = this.Rewrite(constant.Type);
    }

    /// <summary>
    /// Rewrites the children of the given metadata array creation expression.
    /// </summary>
    public virtual void RewriteChildren(MetadataCreateArray createArray) {
      Contract.Requires(createArray != null);

      this.RewriteChildren((MetadataExpression)createArray);
      createArray.ElementType = this.Rewrite(createArray.ElementType);
      createArray.Initializers = this.Rewrite(createArray.Initializers);
    }

    /// <summary>
    /// Rewrites the children the given metadata expression.
    /// </summary>
    public virtual void RewriteChildren(MetadataExpression expression) {
      Contract.Requires(expression != null);

      expression.Type = this.Rewrite(expression.Type);
    }

    /// <summary>
    /// Rewrites the children of the given metadata named argument expression.
    /// </summary>
    public virtual void RewriteChildren(MetadataNamedArgument namedArgument) {
      Contract.Requires(namedArgument != null);

      this.RewriteChildren((MetadataExpression)namedArgument);
      namedArgument.ArgumentValue = this.Rewrite(namedArgument.ArgumentValue);
    }

    /// <summary>
    /// Rewrites the given metadata typeof expression.
    /// </summary>
    public virtual void RewriteChildren(MetadataTypeOf metadataTypeOf) {
      Contract.Requires(metadataTypeOf != null);

      this.RewriteChildren((MetadataExpression)metadataTypeOf);
      metadataTypeOf.TypeToGet = this.Rewrite(metadataTypeOf.TypeToGet);
    }

    /// <summary>
    /// Rewrites the children of the given method body.
    /// </summary>
    public virtual void RewriteChildren(MethodBody methodBody) {
      Contract.Requires(methodBody != null);

      methodBody.LocalVariables = this.Rewrite(methodBody.LocalVariables);
      methodBody.Operations = this.Rewrite(methodBody.Operations);
      methodBody.OperationExceptionInformation = this.Rewrite(methodBody.OperationExceptionInformation);
    }

    /// <summary>
    /// Rewrites the children of the given method definition.
    /// </summary>
    public virtual void RewriteChildren(MethodDefinition method) {
      Contract.Requires(method != null);

      this.RewriteChildren((TypeDefinitionMember)method);
      if (method.IsGeneric)
        method.GenericParameters = this.Rewrite(method.GenericParameters);
      method.InternFactory = this.internFactory;
      method.Parameters = this.Rewrite(method.Parameters);
      if (method.IsPlatformInvoke)
        method.PlatformInvokeData = this.Rewrite(method.PlatformInvokeData);
      method.ReturnValueAttributes = this.Rewrite(method.ReturnValueAttributes);
      if (method.ReturnValueIsModified)
        method.ReturnValueCustomModifiers = this.Rewrite(method.ReturnValueCustomModifiers);
      if (method.ReturnValueIsMarshalledExplicitly)
        method.ReturnValueMarshallingInformation = this.Rewrite(method.ReturnValueMarshallingInformation);
      if (method.HasDeclarativeSecurity)
        method.SecurityAttributes = this.Rewrite(method.SecurityAttributes);
      method.Type = this.Rewrite(method.Type);
      if (!method.IsAbstract && !method.IsExternal)
        method.Body = this.Rewrite(method.Body);
    }

    /// <summary>
    /// Rewrites the children of the given method implementation.
    /// </summary>
    public virtual void RewriteChildren(MethodImplementation methodImplementation) {
      Contract.Requires(methodImplementation != null);

      methodImplementation.ImplementedMethod = this.Rewrite(methodImplementation.ImplementedMethod);
      methodImplementation.ImplementingMethod = this.Rewrite(methodImplementation.ImplementingMethod);
    }

    /// <summary>
    /// Rewrites the children of the given method reference.
    /// </summary>
    public virtual void RewriteChildren(MethodReference methodReference) {
      Contract.Requires(methodReference != null);
      Contract.Requires(!methodReference.IsFrozen);

      methodReference.Attributes = this.Rewrite(methodReference.Attributes);
      methodReference.ContainingType = this.Rewrite(methodReference.ContainingType);
      methodReference.ExtraParameters = this.Rewrite(methodReference.ExtraParameters);
      methodReference.InternFactory = this.internFactory;
      methodReference.Parameters = this.Rewrite(methodReference.Parameters);
      if (methodReference.ReturnValueIsModified)
        methodReference.ReturnValueCustomModifiers = this.Rewrite(methodReference.ReturnValueCustomModifiers);
      methodReference.Type = this.Rewrite(methodReference.Type);
    }

    /// <summary>
    /// Rewrites the children of the given modified type reference.
    /// </summary>
    public virtual void RewriteChildren(ModifiedTypeReference modifiedTypeReference) {
      Contract.Requires(modifiedTypeReference != null);
      Contract.Requires(!modifiedTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)modifiedTypeReference);
      modifiedTypeReference.CustomModifiers = this.Rewrite(modifiedTypeReference.CustomModifiers);
      modifiedTypeReference.UnmodifiedType = this.Rewrite(modifiedTypeReference.UnmodifiedType);
    }

    /// <summary>
    /// Rewrites the children of the given module.
    /// </summary>
    public virtual void RewriteChildren(Module module) {
      Contract.Requires(module != null);

      this.RewriteChildren((Unit)module);
      module.AssemblyReferences = this.Rewrite(module.AssemblyReferences);
      if (module.Kind == ModuleKind.ConsoleApplication || module.Kind == ModuleKind.WindowsApplication)
        module.EntryPoint = this.Rewrite(module.EntryPoint);
      module.ModuleAttributes = this.Rewrite(module.ModuleAttributes);
      module.ModuleReferences = this.Rewrite(module.ModuleReferences);
      module.Win32Resources = this.Rewrite(module.Win32Resources);
      module.UnitNamespaceRoot = this.Rewrite(module.UnitNamespaceRoot);
      var typeReferences = module.TypeReferences;
      if (typeReferences != null) {
        for (int i = 0; i < typeReferences.Count; i++) {
          var typeReference = typeReferences[i];
          object copy;
          if (this.referenceRewrites.TryGetValue(typeReference, out copy))
            typeReferences[i] = (ITypeReference)copy;
          else
            typeReferences.RemoveAt(i--);
        }
      }
      var typeMemberReferences = module.TypeMemberReferences;
      if (typeMemberReferences != null) {
        for (int i = 0; i < typeMemberReferences.Count; i++) {
          var typeMemberReference = typeMemberReferences[i];
          object copy;
          if (this.referenceRewrites.TryGetValue(typeMemberReference, out copy))
            typeMemberReferences[i] = (ITypeMemberReference)copy;
          else
            typeMemberReferences.RemoveAt(i--);
        }
      }
    }

    /// <summary>
    /// Rewrites the children the given module reference.
    /// </summary>
    public virtual void RewriteChildren(ModuleReference moduleReference) {
      Contract.Requires(moduleReference != null);

      this.RewriteChildren((UnitReference)moduleReference);
      if (moduleReference.ContainingAssembly != null)
        moduleReference.ContainingAssembly = this.Rewrite(moduleReference.ContainingAssembly);
    }

    /// <summary>
    /// Rewrites the namespace alias for type
    /// </summary>
    public virtual void RewriteChildren(NamespaceAliasForType namespaceAliasForType) {
      Contract.Requires(namespaceAliasForType != null);

      this.RewriteChildren((AliasForType)namespaceAliasForType);
    }

    /// <summary>
    /// Rewrites the given namespace type reference.
    /// </summary>
    public virtual void RewriteChildren(NamespaceTypeDefinition namespaceTypeDefinition) {
      Contract.Requires(namespaceTypeDefinition != null);

      this.RewriteChildren((NamedTypeDefinition)namespaceTypeDefinition);
    }

    /// <summary>
    /// Rewrites the given namespace type reference.
    /// </summary>
    public virtual void RewriteChildren(NamespaceTypeReference namespaceTypeReference) {
      Contract.Requires(namespaceTypeReference != null);
      Contract.Requires(!namespaceTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)namespaceTypeReference);
      namespaceTypeReference.ContainingUnitNamespace = this.Rewrite(namespaceTypeReference.ContainingUnitNamespace);
    }

    /// <summary>
    /// Rewrites the nested alias for type
    /// </summary>
    public virtual void RewriteChildren(NestedAliasForType nestedAliasForType) {
      Contract.Requires(nestedAliasForType != null);

      this.RewriteChildren((AliasForType)nestedAliasForType);
    }

    /// <summary>
    /// Rewrites the children of given nested type definition.
    /// </summary>
    public virtual void RewriteChildren(NestedTypeDefinition nestedTypeDefinition) {
      Contract.Requires(nestedTypeDefinition != null);

      this.RewriteChildren((NamedTypeDefinition)nestedTypeDefinition);
    }

    /// <summary>
    /// Rewrites the given nested type reference.
    /// </summary>
    public virtual void RewriteChildren(NestedTypeReference nestedTypeReference) {
      Contract.Requires(nestedTypeReference != null);
      Contract.Requires(!nestedTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)nestedTypeReference);
      nestedTypeReference.ContainingType = this.Rewrite(nestedTypeReference.ContainingType);
    }

    /// <summary>
    /// Rewrites the given nested unit namespace.
    /// </summary>
    public virtual void RewriteChildren(NestedUnitNamespace nestedUnitNamespace) {
      Contract.Requires(nestedUnitNamespace != null);

      this.RewriteChildren((UnitNamespace)nestedUnitNamespace);
    }

    /// <summary>
    /// Rewrites the given nested unit namespace reference.
    /// </summary>
    public virtual void RewriteChildren(NestedUnitNamespaceReference nestedUnitNamespaceReference) {
      Contract.Requires(nestedUnitNamespaceReference != null);
      Contract.Requires(!nestedUnitNamespaceReference.IsFrozen);

      this.RewriteChildren((UnitNamespaceReference)nestedUnitNamespaceReference);
      nestedUnitNamespaceReference.ContainingUnitNamespace = this.Rewrite(nestedUnitNamespaceReference.ContainingUnitNamespace);
    }

    /// <summary>
    /// Rewrites the children the specified operation.
    /// </summary>
    public virtual void RewriteChildren(Operation operation) {
      Contract.Requires(operation != null);

      var typeReference = operation.Value as ITypeReference;
      if (typeReference != null)
        operation.Value = this.Rewrite(typeReference);
      else {
        var fieldReference = operation.Value as IFieldReference;
        if (fieldReference != null)
          operation.Value = this.Rewrite(fieldReference);
        else {
          var methodReference = operation.Value as IMethodReference;
          if (methodReference != null)
            operation.Value = this.Rewrite(methodReference);
          else {
            var local = operation.Value as ILocalDefinition;
            if (local != null)
              operation.Value = this.RewriteReference(local);
            else {
              var parameter = operation.Value as IParameterDefinition;
              if (parameter != null)
                operation.Value = this.RewriteReference(parameter);
            }
          }
        }
      }
    }

    /// <summary>
    /// Rewrites the children of the specified operation exception information.
    /// </summary>
    public virtual void RewriteChildren(OperationExceptionInformation operationExceptionInformation) {
      Contract.Requires(operationExceptionInformation != null);

      if (operationExceptionInformation.HandlerKind == HandlerKind.Catch || operationExceptionInformation.HandlerKind == HandlerKind.Filter)
        operationExceptionInformation.ExceptionType = this.Rewrite(operationExceptionInformation.ExceptionType);
    }

    /// <summary>
    /// Rewrites the children of the given parameter definition.
    /// </summary>
    public virtual void RewriteChildren(ParameterDefinition parameterDefinition) {
      Contract.Requires(parameterDefinition != null);

      parameterDefinition.Attributes = this.Rewrite(parameterDefinition.Attributes);
      if (parameterDefinition.IsModified)
        parameterDefinition.CustomModifiers = this.Rewrite(parameterDefinition.CustomModifiers);
      if (parameterDefinition.HasDefaultValue)
        parameterDefinition.DefaultValue = this.Rewrite(parameterDefinition.DefaultValue);
      if (parameterDefinition.IsMarshalledExplicitly)
        parameterDefinition.MarshallingInformation = this.Rewrite(parameterDefinition.MarshallingInformation);
      parameterDefinition.Type = this.Rewrite(parameterDefinition.Type);
    }

    /// <summary>
    /// Rewrites the given parameter type information.
    /// </summary>
    public virtual void RewriteChildren(ParameterTypeInformation parameterTypeInformation) {
      Contract.Requires(parameterTypeInformation != null);

      if (parameterTypeInformation.IsModified)
        parameterTypeInformation.CustomModifiers = this.Rewrite(parameterTypeInformation.CustomModifiers);
      parameterTypeInformation.Type = this.Rewrite(parameterTypeInformation.Type);
    }

    /// <summary>
    /// Rewrites the given PE section.
    /// </summary>
    /// <param name="peSection"></param>
    public virtual void RewriteChildren(PESection peSection) {
    }

    /// <summary>
    /// Rewrites the children of the specified platform invoke information.
    /// </summary>
    public virtual void RewriteChildren(PlatformInvokeInformation platformInvokeInformation) {
      Contract.Requires(platformInvokeInformation != null);

      platformInvokeInformation.ImportModule = this.Rewrite(platformInvokeInformation.ImportModule);
    }

    /// <summary>
    /// Rewrites the given pointer type reference.
    /// </summary>
    public virtual void RewriteChildren(PointerTypeReference pointerTypeReference) {
      Contract.Requires(pointerTypeReference != null);
      Contract.Requires(!pointerTypeReference.IsFrozen);

      this.RewriteChildren((TypeReference)pointerTypeReference);
      pointerTypeReference.TargetType = this.Rewrite(pointerTypeReference.TargetType);
    }

    /// <summary>
    /// Rewrites the given property definition.
    /// </summary>
    public virtual void RewriteChildren(PropertyDefinition propertyDefinition) {
      Contract.Requires(propertyDefinition != null);

      this.RewriteChildren((TypeDefinitionMember)propertyDefinition);
      propertyDefinition.Accessors = this.Rewrite(propertyDefinition.Accessors);
      if (propertyDefinition.HasDefaultValue)
        propertyDefinition.DefaultValue = this.Rewrite((MetadataConstant)propertyDefinition.DefaultValue);
      if (propertyDefinition.Getter != null)
        propertyDefinition.Getter = this.Rewrite(propertyDefinition.Getter);
      propertyDefinition.Parameters = this.Rewrite(propertyDefinition.Parameters);
      if (propertyDefinition.ReturnValueIsModified)
        propertyDefinition.ReturnValueCustomModifiers = this.Rewrite(propertyDefinition.ReturnValueCustomModifiers);
      if (propertyDefinition.Setter != null)
        propertyDefinition.Setter = this.Rewrite(propertyDefinition.Setter);
      propertyDefinition.Type = this.Rewrite(propertyDefinition.Type);
    }

    /// <summary>
    /// Rewrites the children of the given reference to a manifest resource.
    /// </summary>
    public virtual void RewriteChildren(ResourceReference resourceReference) {
      Contract.Requires(resourceReference != null);

      resourceReference.Attributes = this.Rewrite(resourceReference.Attributes);
      resourceReference.DefiningAssembly = this.Rewrite(resourceReference.DefiningAssembly);
    }

    /// <summary>
    /// Rewrites the children of the specified root unit namespace.
    /// </summary>
    public virtual void RewriteChildren(RootUnitNamespace rootUnitNamespace) {
      Contract.Requires(rootUnitNamespace != null);

      this.RewriteChildren((UnitNamespace)rootUnitNamespace);
    }

    /// <summary>
    /// Rewrites the children of given reference to a root unit namespace.
    /// </summary>
    public virtual void RewriteChildren(RootUnitNamespaceReference rootUnitNamespaceReference) {
      Contract.Requires(rootUnitNamespaceReference != null);
      Contract.Requires(!rootUnitNamespaceReference.IsFrozen);

      this.RewriteChildren((UnitNamespaceReference)rootUnitNamespaceReference);
      rootUnitNamespaceReference.Unit = this.Rewrite(rootUnitNamespaceReference.Unit);
    }

    /// <summary>
    /// Rewrites the children of given security attribute.
    /// </summary>
    public virtual void RewriteChildren(SecurityAttribute securityAttribute) {
      Contract.Requires(securityAttribute != null);

      securityAttribute.Attributes = this.Rewrite(securityAttribute.Attributes);
    }

    /// <summary>
    /// Rewrites the children of given specialized event definition.
    /// </summary>
    public virtual void RewriteChildren(SpecializedEventDefinition specializedEventDefinition) {
      Contract.Requires(specializedEventDefinition != null);

      this.RewriteChildren((EventDefinition)specializedEventDefinition);
      specializedEventDefinition.UnspecializedVersion = this.Rewrite(specializedEventDefinition.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized field definition.
    /// </summary>
    public virtual void RewriteChildren(SpecializedFieldDefinition specializedFieldDefinition) {
      Contract.Requires(specializedFieldDefinition != null);

      this.RewriteChildren((FieldDefinition)specializedFieldDefinition);
      specializedFieldDefinition.UnspecializedVersion = this.Rewrite(specializedFieldDefinition.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized field reference.
    /// </summary>
    public virtual void RewriteChildren(SpecializedFieldReference specializedFieldReference) {
      Contract.Requires(specializedFieldReference != null);
      Contract.Requires(!specializedFieldReference.IsFrozen);

      this.RewriteChildren((FieldReference)specializedFieldReference);
      specializedFieldReference.UnspecializedVersion = this.Rewrite(specializedFieldReference.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized method definition.
    /// </summary>
    public virtual void RewriteChildren(SpecializedMethodDefinition specializedMethodDefinition) {
      Contract.Requires(specializedMethodDefinition != null);

      this.RewriteChildren((MethodDefinition)specializedMethodDefinition);
      specializedMethodDefinition.UnspecializedVersion = this.Rewrite(specializedMethodDefinition.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized method reference.
    /// </summary>
    public virtual void RewriteChildren(SpecializedMethodReference specializedMethodReference) {
      Contract.Requires(specializedMethodReference != null);
      Contract.Requires(!specializedMethodReference.IsFrozen);

      this.RewriteChildren((MethodReference)specializedMethodReference);
      specializedMethodReference.UnspecializedVersion = this.Rewrite(specializedMethodReference.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized nested type definition.
    /// </summary>
    public virtual void RewriteChildren(SpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
      Contract.Requires(specializedNestedTypeDefinition != null);

      this.RewriteChildren((NestedTypeDefinition)specializedNestedTypeDefinition);
      specializedNestedTypeDefinition.UnspecializedVersion = this.Rewrite(specializedNestedTypeDefinition.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized nested type reference.
    /// </summary>
    public virtual void RewriteChildren(SpecializedNestedTypeReference specializedNestedTypeReference) {
      Contract.Requires(specializedNestedTypeReference != null);
      Contract.Requires(!specializedNestedTypeReference.IsFrozen);

      this.RewriteChildren((NestedTypeReference)specializedNestedTypeReference);
      specializedNestedTypeReference.UnspecializedVersion = this.Rewrite(specializedNestedTypeReference.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of given specialized property definition.
    /// </summary>
    public virtual void RewriteChildren(SpecializedPropertyDefinition specializedPropertyDefinition) {
      Contract.Requires(specializedPropertyDefinition != null);

      this.RewriteChildren((PropertyDefinition)specializedPropertyDefinition);
      specializedPropertyDefinition.UnspecializedVersion = this.Rewrite(specializedPropertyDefinition.UnspecializedVersion);
    }

    /// <summary>
    /// Rewrites the children of the specified named type definition.
    /// </summary>
    public virtual void RewriteChildren(NamedTypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);

      typeDefinition.Attributes = this.Rewrite(typeDefinition.Attributes);
      typeDefinition.BaseClasses = this.Rewrite(typeDefinition.BaseClasses);
      typeDefinition.ExplicitImplementationOverrides = this.Rewrite(typeDefinition.ExplicitImplementationOverrides);
      if (typeDefinition.IsGeneric)
        typeDefinition.GenericParameters = this.Rewrite(typeDefinition.GenericParameters);
      if (typeDefinition.HasDeclarativeSecurity)
        typeDefinition.SecurityAttributes = this.Rewrite(typeDefinition.SecurityAttributes);
      if (typeDefinition.IsEnum)
        typeDefinition.UnderlyingType = this.Rewrite(typeDefinition.UnderlyingType);
      typeDefinition.InternFactory = this.internFactory;
      typeDefinition.PlatformType = this.host.PlatformType;
      typeDefinition.Interfaces = this.Rewrite(typeDefinition.Interfaces);
      typeDefinition.Events = this.Rewrite(typeDefinition.Events);
      typeDefinition.Fields = this.Rewrite(typeDefinition.Fields);
      typeDefinition.Methods = this.Rewrite(typeDefinition.Methods);
      typeDefinition.NestedTypes = this.Rewrite(typeDefinition.NestedTypes);
      typeDefinition.Properties = this.Rewrite(typeDefinition.Properties);
    }

    /// <summary>
    /// Rewrites the children of the given type definition member.
    /// </summary>
    public virtual void RewriteChildren(TypeDefinitionMember typeDefinitionMember) {
      Contract.Requires(typeDefinitionMember != null);

      typeDefinitionMember.Attributes = this.Rewrite(typeDefinitionMember.Attributes);
    }

    /// <summary>
    /// Rewrites the children of the given type reference.
    /// </summary>
    public virtual void RewriteChildren(TypeReference typeReference) {
      Contract.Requires(typeReference != null);

      typeReference.Attributes = this.Rewrite(typeReference.Attributes);
      typeReference.InternFactory = this.internFactory;
      typeReference.PlatformType = this.host.PlatformType;
    }

    /// <summary>
    /// Rewrites the children of the specified unit.
    /// </summary>
    public virtual void RewriteChildren(Unit unit) {
      Contract.Requires(unit != null);

      this.RewriteChildren((UnitReference)unit);
      unit.PlatformType = this.host.PlatformType;
      unit.UninterpretedSections = this.Rewrite(unit.UninterpretedSections);
    }

    /// <summary>
    /// Rewrites the specified unit.
    /// </summary>
    public virtual void RewriteChildren(UnitReference unitReference) {
      Contract.Requires(unitReference != null);
      Contract.Requires(!(unitReference.IsFrozen));

      unitReference.Attributes = this.Rewrite(unitReference.Attributes);
    }

    /// <summary>
    /// Rewrites the children of the specified unit namespace.
    /// </summary>
    public virtual void RewriteChildren(UnitNamespace unitNamespace) {
      Contract.Requires(unitNamespace != null);

      unitNamespace.Attributes = this.Rewrite(unitNamespace.Attributes);
      unitNamespace.Members = this.Rewrite(unitNamespace.Members);
    }

    /// <summary>
    /// Rewrites the children of the specified reference to a unit namespace.
    /// </summary>
    public virtual void RewriteChildren(UnitNamespaceReference unitNamespaceReference) {
      Contract.Requires(unitNamespaceReference != null);

      unitNamespaceReference.Attributes = this.Rewrite(unitNamespaceReference.Attributes);
    }

    /// <summary>
    /// Rewrites the children of the given Win32 resource.
    /// </summary>
    public virtual void RewriteChildren(Win32Resource win32Resource) {
      Contract.Requires(win32Resource != null);
    }


  }

  /// <summary>
  /// Implemented by mutable objects that provide a method that makes the mutable object a copy of a given immutable object.
  /// </summary>
  [ContractClass(typeof(ICopyFromContract<>))]
  public interface ICopyFrom<ImmutableObject> {
    /// <summary>
    /// Makes this mutable object a copy of the given immutable object.
    /// </summary>
    /// <param name="objectToCopy">An immutable object that implements the same object model interface as this mutable object.</param>
    /// <param name="internFactory">The intern factory to use for computing the interned identity (if applicable) of this mutable object.</param>
    void Copy(ImmutableObject objectToCopy, IInternFactory internFactory);
  }

  [ContractClassFor(typeof(ICopyFrom<>))]
  abstract class ICopyFromContract<ImmutableObject> : ICopyFrom<ImmutableObject> {
    public void Copy(ImmutableObject objectToCopy, IInternFactory internFactory) {
      Contract.Requires(objectToCopy != null);
      Contract.Requires(internFactory != null);
    }
  }

  /// <summary>
  /// This class is obsolete because it is too hard to use correctly. Please use MutatingVisitor and MetadataCopier instead.
  /// A visitor that produces a mutable copy a given metadata model. Derived classes can override the visit methods
  /// to intervene in the copying process, usually resulting in a copy that is not equivalent to the original model.
  /// For instance, the new copy might have additional types and methods, or additional calls to instrumentation routines.
  /// </summary>
  /// <remarks>While the model is being copied, the resulting model is incomplete and or inconsistent. It should not be traversed
  /// independently nor should any of its computed properties, such as ResolvedType be evaluated. Scenarios that need such functionality
  /// should be implemented by first making a mutable copy of the entire assembly and then running a second pass over the mutable result.
  /// The new classes MetadataDeepCopier and MetadataRewriter are meant to facilitate such scenarios.
  /// </remarks>
  [Obsolete("This class has been superceded by MetadataDeepCopier and MetadataRewriter, used in combination. It will go away in the future.")]
  public class MetadataMutator {

    /// <summary>
    /// Allocates a visitor that produces a mutable copy a given metadata model. Derived classes can override the visit methods
    /// to intervene in the copying process, usually resulting in a copy that is not equivalent to the original model.
    /// For instance, the new copy might have additional types and methods, or additional calls to instrumentation routines.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    public MetadataMutator(IMetadataHost host) {
      this.host = host;
    }

    /// <summary>
    /// Allocates a visitor that produces a mutable copy a given metadata model. Derived classes can override the visit methods
    /// to intervene in the copying process, usually resulting in a copy that is not equivalent to the original model.
    /// For instance, the new copy might have additional types and methods, or additional calls to instrumentation routines.
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    /// <param name="copyOnlyIfNotAlreadyMutable">True if the mutator should try and perform mutations in place, rather than mutating new copies.</param>
    public MetadataMutator(IMetadataHost host, bool copyOnlyIfNotAlreadyMutable) {
      this.host = host;
      this.copyOnlyIfNotAlreadyMutable = copyOnlyIfNotAlreadyMutable;
    }

    /// <summary>
    /// Duplicates are cached, both to save space and to detect when the traversal of a cycle should stop.
    /// </summary>
    protected Dictionary<object, object> cache = new Dictionary<object, object>();

    /// <summary>
    /// True if the mutator should try and perform mutations in place, rather than mutating new copies.
    /// </summary>
    protected readonly bool copyOnlyIfNotAlreadyMutable;

    /// <summary>
    ///Since definitions are also references, it can happen that a definition is visited as both a definition and as a reference.
    ///If so, the cache may contain a duplicated definition when a reference is expected, or vice versa.
    ///To prevent this, reference duplicates are always cached separately.
    /// </summary>
    protected Dictionary<object, object> referenceCache = new Dictionary<object, object>();

    /// <summary>
    /// 
    /// </summary>
    protected List<INamedTypeDefinition> flatListOfTypes = new List<INamedTypeDefinition>();

    /// <summary>
    /// 
    /// </summary>
    protected IMetadataHost host;

    /// <summary>
    /// 
    /// </summary>
    protected System.Collections.Stack path = new System.Collections.Stack();

    /// <summary>
    /// 
    /// </summary>
    protected bool stopTraversal;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IMethodDefinition GetCurrentMethod() {
      foreach (object parent in this.path) {
        IMethodDefinition/*?*/ method = parent as IMethodDefinition;
        if (method != null) return method;
      }
      return Dummy.MethodDefinition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IUnitNamespace GetCurrentNamespace() {
      foreach (object parent in this.path) {
        IUnitNamespace/*?*/ ns = parent as IUnitNamespace;
        if (ns != null) return ns;
      }
      return Dummy.UnitNamespace;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ISignature GetCurrentSignature() {
      foreach (object parent in this.path) {
        ISignature/*?*/ signature = parent as ISignature;
        if (signature != null) return signature;
      }
      return Dummy.Signature;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ITypeDefinition GetCurrentType() {
      foreach (object parent in this.path) {
        ITypeDefinition/*?*/ type = parent as ITypeDefinition;
        if (type != null) return type;
      }
      return Dummy.TypeDefinition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IUnit GetCurrentUnit() {
      foreach (object parent in this.path) {
        IUnit/*?*/ unit = parent as IUnit;
        if (unit != null) return unit;
      }
      return Dummy.Unit;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public virtual Assembly GetMutableCopy(IAssembly assembly) {
      Assembly/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = assembly as Assembly;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(assembly, out cachedValue);
      result = cachedValue as Assembly;
      if (result != null) return result;
      result = new Assembly();
      this.cache.Add(assembly, result);
      this.cache.Add(result, result);
      result.Copy(assembly, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyReference"></param>
    /// <returns></returns>
    public virtual AssemblyReference GetMutableCopy(IAssemblyReference assemblyReference) {
      AssemblyReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = assemblyReference as AssemblyReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(assemblyReference, out cachedValue);
      result = cachedValue as AssemblyReference;
      if (result != null) return result;
      result = new AssemblyReference();
      this.referenceCache.Add(assemblyReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(assemblyReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customAttribute"></param>
    /// <returns></returns>
    public virtual CustomAttribute GetMutableCopy(ICustomAttribute customAttribute) {
      CustomAttribute result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = customAttribute as CustomAttribute;
        if (result != null) return result;
      }
      result = new CustomAttribute();
      result.Copy(customAttribute, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customModifier"></param>
    /// <returns></returns>
    public virtual CustomModifier GetMutableCopy(ICustomModifier customModifier) {
      CustomModifier result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = customModifier as CustomModifier;
        if (result != null) return result;
      }
      result = new CustomModifier();
      result.Copy(customModifier, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventDefinition"></param>
    /// <returns></returns>
    public virtual EventDefinition GetMutableCopy(IEventDefinition eventDefinition) {
      EventDefinition result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = eventDefinition as EventDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(eventDefinition, out cachedValue);
      result = cachedValue as EventDefinition;
      if (result != null) return result;
      result = new EventDefinition();
      this.cache.Add(eventDefinition, result);
      this.cache.Add(result, result);
      result.Copy(eventDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// Get a mutable copy of the given field definition. 
    /// </summary>
    /// <param name="fieldDefinition"></param>
    /// <returns></returns>
    public virtual FieldDefinition GetMutableCopy(IFieldDefinition fieldDefinition) {
      FieldDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = fieldDefinition as FieldDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(fieldDefinition, out cachedValue);
      result = cachedValue as FieldDefinition;
      if (result != null) return result;
      result = new FieldDefinition();
      this.cache.Add(fieldDefinition, result);
      this.cache.Add(result, result);
      result.Copy(fieldDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sectionBlock"></param>
    /// <returns></returns>
    public virtual SectionBlock GetMutableCopy(ISectionBlock sectionBlock) {
      SectionBlock/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = sectionBlock as SectionBlock;
        if (result != null) return result;
      }
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
    /// Get a mutable copy of the given field reference. 
    /// </summary>
    /// <param name="fieldReference"></param>
    /// <returns></returns>
    public virtual FieldReference GetMutableCopy(IFieldReference fieldReference) {
      FieldReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = fieldReference as FieldReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(fieldReference, out cachedValue);
      result = cachedValue as FieldReference;
      if (result != null) return result;
      result = new FieldReference();
      this.referenceCache.Add(fieldReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(fieldReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileReference"></param>
    /// <returns></returns>
    public virtual FileReference GetMutableCopy(IFileReference fileReference) {
      FileReference result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = fileReference as FileReference;
        if (result != null) return result;
      }
      result = new FileReference();
      result.Copy(fileReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="functionPointerTypeReference"></param>
    /// <returns></returns>
    public virtual FunctionPointerTypeReference GetMutableCopy(IFunctionPointerTypeReference functionPointerTypeReference) {
      FunctionPointerTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = functionPointerTypeReference as FunctionPointerTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(functionPointerTypeReference, out cachedValue);
      result = cachedValue as FunctionPointerTypeReference;
      if (result != null) return result;
      result = new FunctionPointerTypeReference();
      this.referenceCache.Add(functionPointerTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(functionPointerTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <returns></returns>
    public virtual GenericMethodInstanceReference GetMutableCopy(IGenericMethodInstanceReference genericMethodInstanceReference) {
      GenericMethodInstanceReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = genericMethodInstanceReference as GenericMethodInstanceReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(genericMethodInstanceReference, out cachedValue);
      result = cachedValue as GenericMethodInstanceReference;
      if (result != null) return result;
      result = new GenericMethodInstanceReference();
      this.referenceCache.Add(genericMethodInstanceReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(genericMethodInstanceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodParameter"></param>
    /// <returns></returns>
    public virtual GenericMethodParameter GetMutableCopy(IGenericMethodParameter genericMethodParameter) {
      GenericMethodParameter/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = genericMethodParameter as GenericMethodParameter;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericMethodParameter, out cachedValue);
      result = cachedValue as GenericMethodParameter;
      if (result != null) return result;
      result = new GenericMethodParameter();
      this.cache.Add(genericMethodParameter, result);
      this.cache.Add(result, result);
      result.Copy(genericMethodParameter, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericMethodParameterReference"></param>
    /// <returns></returns>
    public virtual GenericMethodParameterReference GetMutableCopy(IGenericMethodParameterReference genericMethodParameterReference) {
      GenericMethodParameterReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = genericMethodParameterReference as GenericMethodParameterReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(genericMethodParameterReference, out cachedValue);
      result = cachedValue as GenericMethodParameterReference;
      if (result != null) return result;
      result = new GenericMethodParameterReference();
      this.referenceCache.Add(genericMethodParameterReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(genericMethodParameterReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericTypeInstanceReference"></param>
    /// <returns></returns>
    public virtual GenericTypeInstanceReference GetMutableCopy(IGenericTypeInstanceReference genericTypeInstanceReference) {
      GenericTypeInstanceReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = genericTypeInstanceReference as GenericTypeInstanceReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(genericTypeInstanceReference, out cachedValue);
      result = cachedValue as GenericTypeInstanceReference;
      if (result != null) return result;
      result = new GenericTypeInstanceReference();
      this.referenceCache.Add(genericTypeInstanceReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(genericTypeInstanceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericTypeParameter"></param>
    /// <returns></returns>
    public virtual GenericTypeParameter GetMutableCopy(IGenericTypeParameter genericTypeParameter) {
      GenericTypeParameter/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = genericTypeParameter as GenericTypeParameter;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(genericTypeParameter, out cachedValue);
      result = cachedValue as GenericTypeParameter;
      if (result != null) return result;
      result = new GenericTypeParameter();
      this.cache.Add(genericTypeParameter, result);
      this.cache.Add(result, result);
      result.Copy(genericTypeParameter, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="genericTypeParameterReference"></param>
    /// <returns></returns>
    public virtual GenericTypeParameterReference GetMutableCopy(IGenericTypeParameterReference genericTypeParameterReference) {
      GenericTypeParameterReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = genericTypeParameterReference as GenericTypeParameterReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(genericTypeParameterReference, out cachedValue);
      result = cachedValue as GenericTypeParameterReference;
      if (result != null) return result;
      result = new GenericTypeParameterReference();
      this.referenceCache.Add(genericTypeParameterReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(genericTypeParameterReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// Get a mutable copy of the given global method definition. 
    /// </summary>
    /// <param name="globalFieldDefinition"></param>
    /// <returns></returns>
    public virtual GlobalFieldDefinition GetMutableCopy(IGlobalFieldDefinition globalFieldDefinition) {
      GlobalFieldDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = globalFieldDefinition as GlobalFieldDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(globalFieldDefinition, out cachedValue);
      result = cachedValue as GlobalFieldDefinition;
      if (result != null) return result;
      result = new GlobalFieldDefinition();
      this.cache.Add(globalFieldDefinition, result);
      this.cache.Add(result, result);
      result.Copy(globalFieldDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalMethodDefinition"></param>
    /// <returns></returns>
    public virtual GlobalMethodDefinition GetMutableCopy(IGlobalMethodDefinition globalMethodDefinition) {
      GlobalMethodDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = globalMethodDefinition as GlobalMethodDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(globalMethodDefinition, out cachedValue);
      result = cachedValue as GlobalMethodDefinition;
      if (result != null) return result;
      result = new GlobalMethodDefinition();
      this.cache.Add(globalMethodDefinition, result);
      this.cache.Add(result, result);
      result.Copy(globalMethodDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="localDefinition"></param>
    /// <returns></returns>
    public virtual LocalDefinition GetMutableCopy(ILocalDefinition localDefinition) {
      LocalDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = localDefinition as LocalDefinition;
        if (result != null) return result;
      }
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
    public virtual ManagedPointerTypeReference GetMutableCopy(IManagedPointerTypeReference managedPointerTypeReference) {
      ManagedPointerTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = managedPointerTypeReference as ManagedPointerTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(managedPointerTypeReference, out cachedValue);
      result = cachedValue as ManagedPointerTypeReference;
      if (result != null) return result;
      result = new ManagedPointerTypeReference();
      this.referenceCache.Add(managedPointerTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(managedPointerTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="marshallingInformation"></param>
    /// <returns></returns>
    public virtual MarshallingInformation GetMutableCopy(IMarshallingInformation marshallingInformation) {
      MarshallingInformation result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = marshallingInformation as MarshallingInformation;
        if (result != null) return result;
      }
      result = new MarshallingInformation();
      result.Copy(marshallingInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataConstant"></param>
    /// <returns></returns>
    public virtual MetadataConstant GetMutableCopy(IMetadataConstant metadataConstant) {
      MetadataConstant result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = metadataConstant as MetadataConstant;
        if (result != null) return result;
      }
      result = new MetadataConstant();
      result.Copy(metadataConstant, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataCreateArray"></param>
    /// <returns></returns>
    public virtual MetadataCreateArray GetMutableCopy(IMetadataCreateArray metadataCreateArray) {
      MetadataCreateArray result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = metadataCreateArray as MetadataCreateArray;
        if (result != null) return result;
      }
      result = new MetadataCreateArray();
      result.Copy(metadataCreateArray, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataNamedArgument"></param>
    /// <returns></returns>
    public virtual MetadataNamedArgument GetMutableCopy(IMetadataNamedArgument metadataNamedArgument) {
      MetadataNamedArgument result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = metadataNamedArgument as MetadataNamedArgument;
        if (result != null) return result;
      }
      result = new MetadataNamedArgument();
      result.Copy(metadataNamedArgument, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadataTypeOf"></param>
    /// <returns></returns>
    public virtual MetadataTypeOf GetMutableCopy(IMetadataTypeOf metadataTypeOf) {
      MetadataTypeOf result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = metadataTypeOf as MetadataTypeOf;
        if (result != null) return result;
      }
      result = new MetadataTypeOf();
      result.Copy(metadataTypeOf, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodDefinition"></param>
    /// <returns></returns>
    public virtual MethodDefinition GetMutableCopy(IMethodDefinition methodDefinition) {
      MethodDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = methodDefinition as MethodDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(methodDefinition, out cachedValue);
      result = cachedValue as MethodDefinition;
      if (result != null) return result;
      result = new MethodDefinition();
      this.cache.Add(methodDefinition, result);
      this.cache.Add(result, result);
      result.Copy(methodDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodBody"></param>
    /// <returns></returns>
    public virtual MethodBody GetMutableCopy(IMethodBody methodBody) {
      MethodBody result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = methodBody as MethodBody;
        if (result != null) return result;
      }
      result = new MethodBody();
      result.Copy(methodBody, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodImplementation"></param>
    /// <returns></returns>
    public virtual MethodImplementation GetMutableCopy(IMethodImplementation methodImplementation) {
      MethodImplementation result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = methodImplementation as MethodImplementation;
        if (result != null) return result;
      }
      result = new MethodImplementation();
      result.Copy(methodImplementation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// Get a mutable copy of the given method reference. 
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    public virtual MethodReference GetMutableCopy(IMethodReference methodReference) {
      MethodReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = methodReference as MethodReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(methodReference, out cachedValue);
      result = cachedValue as MethodReference;
      if (result != null) return result;
      result = new MethodReference();
      this.referenceCache.Add(methodReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(methodReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifiedTypeReference"></param>
    /// <returns></returns>
    public virtual ModifiedTypeReference GetMutableCopy(IModifiedTypeReference modifiedTypeReference) {
      ModifiedTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = modifiedTypeReference as ModifiedTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(modifiedTypeReference, out cachedValue);
      result = cachedValue as ModifiedTypeReference;
      if (result != null) return result;
      result = new ModifiedTypeReference();
      this.referenceCache.Add(modifiedTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(modifiedTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public virtual Module GetMutableCopy(IModule module) {
      Module/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = module as Module;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(module, out cachedValue);
      result = cachedValue as Module;
      if (result != null) return result;
      result = new Module();
      this.cache.Add(module, result);
      this.cache.Add(result, result);
      result.Copy(module, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moduleReference"></param>
    /// <returns></returns>
    public virtual ModuleReference GetMutableCopy(IModuleReference moduleReference) {
      ModuleReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = moduleReference as ModuleReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(moduleReference, out cachedValue);
      result = cachedValue as ModuleReference;
      if (result != null) return result;
      result = new ModuleReference();
      this.referenceCache.Add(moduleReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(moduleReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceAliasForType"></param>
    /// <returns></returns>
    public virtual NamespaceAliasForType GetMutableCopy(INamespaceAliasForType namespaceAliasForType) {
      NamespaceAliasForType result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = namespaceAliasForType as NamespaceAliasForType;
        if (result != null) return result;
      }
      result = new NamespaceAliasForType();
      result.Copy(namespaceAliasForType, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceTypeDefinition"></param>
    /// <returns></returns>
    public virtual NamespaceTypeDefinition GetMutableCopy(INamespaceTypeDefinition namespaceTypeDefinition) {
      NamespaceTypeDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = namespaceTypeDefinition as NamespaceTypeDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(namespaceTypeDefinition, out cachedValue);
      result = cachedValue as NamespaceTypeDefinition;
      if (result != null) return result;
      result = new NamespaceTypeDefinition();
      this.cache.Add(namespaceTypeDefinition, result);
      this.cache.Add(result, result);
      result.Copy(namespaceTypeDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namespaceTypeReference"></param>
    /// <returns></returns>
    public virtual NamespaceTypeReference GetMutableCopy(INamespaceTypeReference namespaceTypeReference) {
      NamespaceTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = namespaceTypeReference as NamespaceTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(namespaceTypeReference, out cachedValue);
      result = cachedValue as NamespaceTypeReference;
      if (result != null) return result;
      result = new NamespaceTypeReference();
      this.referenceCache.Add(namespaceTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(namespaceTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedAliasForType"></param>
    /// <returns></returns>
    public virtual NestedAliasForType GetMutableCopy(INestedAliasForType nestedAliasForType) {
      NestedAliasForType result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = nestedAliasForType as NestedAliasForType;
        if (result != null) return result;
      }
      result = new NestedAliasForType();
      result.Copy(nestedAliasForType, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedTypeDefinition"></param>
    /// <returns></returns>
    public virtual NestedTypeDefinition GetMutableCopy(INestedTypeDefinition nestedTypeDefinition) {
      NestedTypeDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = nestedTypeDefinition as NestedTypeDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(nestedTypeDefinition, out cachedValue);
      result = cachedValue as NestedTypeDefinition;
      if (result != null) return result;
      result = new NestedTypeDefinition();
      this.cache.Add(nestedTypeDefinition, result);
      this.cache.Add(result, result);
      result.Copy(nestedTypeDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedTypeReference"></param>
    /// <returns></returns>
    public virtual NestedTypeReference GetMutableCopy(INestedTypeReference nestedTypeReference) {
      NestedTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = nestedTypeReference as NestedTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(nestedTypeReference, out cachedValue);
      result = cachedValue as NestedTypeReference;
      if (result != null) return result;
      result = new NestedTypeReference();
      this.referenceCache.Add(nestedTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(nestedTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedUnitNamespace"></param>
    /// <returns></returns>
    public virtual NestedUnitNamespace GetMutableCopy(INestedUnitNamespace nestedUnitNamespace) {
      NestedUnitNamespace/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = nestedUnitNamespace as NestedUnitNamespace;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(nestedUnitNamespace, out cachedValue);
      result = cachedValue as NestedUnitNamespace;
      if (result != null) return result;
      result = new NestedUnitNamespace();
      this.cache.Add(nestedUnitNamespace, result);
      this.cache.Add(result, result);
      result.Copy(nestedUnitNamespace, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nestedUnitNamespaceReference"></param>
    /// <returns></returns>
    public virtual NestedUnitNamespaceReference GetMutableCopy(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      NestedUnitNamespaceReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = nestedUnitNamespaceReference as NestedUnitNamespaceReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(nestedUnitNamespaceReference, out cachedValue);
      result = cachedValue as NestedUnitNamespaceReference;
      if (result != null) return result;
      result = new NestedUnitNamespaceReference();
      this.referenceCache.Add(nestedUnitNamespaceReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(nestedUnitNamespaceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public virtual Operation GetMutableCopy(IOperation operation) {
      Operation result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = operation as Operation;
        if (result != null) return result;
      }
      result = new Operation();
      result.Copy(operation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operationExceptionInformation"></param>
    /// <returns></returns>
    public virtual OperationExceptionInformation GetMutableCopy(IOperationExceptionInformation operationExceptionInformation) {
      OperationExceptionInformation result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = operationExceptionInformation as OperationExceptionInformation;
        if (result != null) return result;
      }
      result = new OperationExceptionInformation();
      result.Copy(operationExceptionInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterDefinition"></param>
    /// <returns></returns>
    public virtual ParameterDefinition GetMutableCopy(IParameterDefinition parameterDefinition) {
      ParameterDefinition/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = parameterDefinition as ParameterDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(parameterDefinition, out cachedValue);
      result = cachedValue as ParameterDefinition;
      if (result != null) return result;
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
    public virtual ParameterTypeInformation GetMutableCopy(IParameterTypeInformation parameterTypeInformation) {
      ParameterTypeInformation result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = parameterTypeInformation as ParameterTypeInformation;
        if (result != null) return result;
      }
      result = new ParameterTypeInformation();
      result.Copy(parameterTypeInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="peSection"></param>
    /// <returns></returns>
    public virtual PESection GetMutableCopy(IPESection peSection) {
      PESection result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = peSection as PESection;
        if (result != null) return result;
      }
      result = new PESection();
      result.Copy(peSection, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformInvokeInformation"></param>
    /// <returns></returns>
    public virtual PlatformInvokeInformation GetMutableCopy(IPlatformInvokeInformation platformInvokeInformation) {
      PlatformInvokeInformation result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = platformInvokeInformation as PlatformInvokeInformation;
        if (result != null) return result;
      }
      result = new PlatformInvokeInformation();
      result.Copy(platformInvokeInformation, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointerTypeReference"></param>
    /// <returns></returns>
    public virtual PointerTypeReference GetMutableCopy(IPointerTypeReference pointerTypeReference) {
      PointerTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = pointerTypeReference as PointerTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(pointerTypeReference, out cachedValue);
      result = cachedValue as PointerTypeReference;
      if (result != null) return result;
      result = new PointerTypeReference();
      this.referenceCache.Add(pointerTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(pointerTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyDefinition"></param>
    /// <returns></returns>
    public virtual PropertyDefinition GetMutableCopy(IPropertyDefinition propertyDefinition) {
      PropertyDefinition result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = propertyDefinition as PropertyDefinition;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(propertyDefinition, out cachedValue);
      result = cachedValue as PropertyDefinition;
      if (result != null) return result;
      result = new PropertyDefinition();
      this.cache.Add(propertyDefinition, result);
      this.cache.Add(result, result);
      result.Copy(propertyDefinition, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceReference"></param>
    /// <returns></returns>
    public virtual ResourceReference GetMutableCopy(IResourceReference resourceReference) {
      ResourceReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = resourceReference as ResourceReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(resourceReference, out cachedValue);
      result = cachedValue as ResourceReference;
      if (result != null) return result;
      result = new ResourceReference();
      this.referenceCache.Add(resourceReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(resourceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootUnitNamespace"></param>
    /// <returns></returns>
    public virtual RootUnitNamespace GetMutableCopy(IRootUnitNamespace rootUnitNamespace) {
      RootUnitNamespace/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = rootUnitNamespace as RootUnitNamespace;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.cache.TryGetValue(rootUnitNamespace, out cachedValue);
      result = cachedValue as RootUnitNamespace;
      if (result != null) return result;
      result = new RootUnitNamespace();
      this.cache.Add(rootUnitNamespace, result);
      this.cache.Add(result, result);
      result.Copy(rootUnitNamespace, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootUnitNamespaceReference"></param>
    /// <returns></returns>
    public virtual RootUnitNamespaceReference GetMutableCopy(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      RootUnitNamespaceReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = rootUnitNamespaceReference as RootUnitNamespaceReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(rootUnitNamespaceReference, out cachedValue);
      result = cachedValue as RootUnitNamespaceReference;
      if (result != null) return result;
      result = new RootUnitNamespaceReference();
      this.referenceCache.Add(rootUnitNamespaceReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(rootUnitNamespaceReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="securityAttribute"></param>
    /// <returns></returns>
    public virtual SecurityAttribute GetMutableCopy(ISecurityAttribute securityAttribute) {
      SecurityAttribute result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = securityAttribute as SecurityAttribute;
        if (result != null) return result;
      }
      result = new SecurityAttribute();
      result.Copy(securityAttribute, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedFieldReference"></param>
    /// <returns></returns>
    public virtual SpecializedFieldReference GetMutableCopy(ISpecializedFieldReference specializedFieldReference) {
      SpecializedFieldReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = specializedFieldReference as SpecializedFieldReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(specializedFieldReference, out cachedValue);
      result = cachedValue as SpecializedFieldReference;
      if (result != null) return result;
      result = new SpecializedFieldReference();
      this.referenceCache.Add(specializedFieldReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(specializedFieldReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedMethodReference"></param>
    /// <returns></returns>
    public virtual SpecializedMethodReference GetMutableCopy(ISpecializedMethodReference specializedMethodReference) {
      SpecializedMethodReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = specializedMethodReference as SpecializedMethodReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(specializedMethodReference, out cachedValue);
      result = cachedValue as SpecializedMethodReference;
      if (result != null) return result;
      result = new SpecializedMethodReference();
      this.referenceCache.Add(specializedMethodReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(specializedMethodReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="specializedNestedTypeReference"></param>
    /// <returns></returns>
    public virtual SpecializedNestedTypeReference GetMutableCopy(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      SpecializedNestedTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = specializedNestedTypeReference as SpecializedNestedTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(specializedNestedTypeReference, out cachedValue);
      result = cachedValue as SpecializedNestedTypeReference;
      if (result != null) return result;
      result = new SpecializedNestedTypeReference();
      this.referenceCache.Add(specializedNestedTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(specializedNestedTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="win32Resource"></param>
    /// <returns></returns>
    public virtual Win32Resource GetMutableCopy(IWin32Resource win32Resource) {
      Win32Resource result = null;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = win32Resource as Win32Resource;
        if (result != null) return result;
      }
      result = new Win32Resource();
      result.Copy(win32Resource, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixTypeReference"></param>
    /// <returns></returns>
    public virtual MatrixTypeReference GetMutableMatrixCopy(IArrayTypeReference matrixTypeReference) {
      MatrixTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = matrixTypeReference as MatrixTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(matrixTypeReference, out cachedValue);
      result = cachedValue as MatrixTypeReference;
      if (result != null) return result;
      result = new MatrixTypeReference();
      this.referenceCache.Add(matrixTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(matrixTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vectorTypeReference"></param>
    /// <returns></returns>
    public virtual VectorTypeReference GetMutableVectorCopy(IArrayTypeReference vectorTypeReference) {
      VectorTypeReference/*?*/ result;
      if (this.copyOnlyIfNotAlreadyMutable) {
        result = vectorTypeReference as VectorTypeReference;
        if (result != null) return result;
      }
      object/*?*/ cachedValue = null;
      this.referenceCache.TryGetValue(vectorTypeReference, out cachedValue);
      result = cachedValue as VectorTypeReference;
      if (result != null) return result;
      result = new VectorTypeReference();
      this.referenceCache.Add(vectorTypeReference, result);
      this.referenceCache.Add(result, result);
      result.Copy(vectorTypeReference, this.host.InternFactory);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    public virtual IMethodReference GetTypeSpecificMutableCopy(IMethodReference methodReference) {
      ISpecializedMethodReference/*?*/ specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null)
        return this.GetMutableCopy(specializedMethodReference);
      IGenericMethodInstanceReference/*?*/ genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null)
        return this.GetMutableCopy(genericMethodInstanceReference);
      else {
        IGlobalMethodDefinition/*?*/ globalMethodDefinition = methodReference as IGlobalMethodDefinition;
        if (globalMethodDefinition != null)
          return this.GetMutableCopy(globalMethodDefinition);
        IMethodDefinition/*?*/ methodDefinition = methodReference as IMethodDefinition;
        if (methodDefinition != null) {
          return this.GetMutableCopy(methodDefinition);
        }
        return this.GetMutableCopy(methodReference);
      }
    }

    /// <summary>
    /// Visits the specified aliases for types.
    /// </summary>
    /// <param name="aliasesForTypes">The aliases for types.</param>
    /// <returns></returns>
    public virtual List<IAliasForType>/*?*/ Visit(List<IAliasForType/*?*/> aliasesForTypes) {
      if (this.stopTraversal || aliasesForTypes == null) return aliasesForTypes;
      for (int i = 0, n = aliasesForTypes.Count; i < n; i++)
        aliasesForTypes[i] = this.Visit(aliasesForTypes[i]);
      return aliasesForTypes;
    }


    /// <summary>
    /// Visits the specified alias for type.
    /// </summary>
    /// <param name="aliasForType">Type of the alias for.</param>
    /// <returns></returns>
    public virtual IAliasForType Visit(IAliasForType aliasForType) {
      if (this.stopTraversal) return aliasForType;
      INamespaceAliasForType/*?*/ namespaceAliasForType = aliasForType as INamespaceAliasForType;
      if (namespaceAliasForType != null) return this.Visit(this.GetMutableCopy(namespaceAliasForType));
      INestedAliasForType/*?*/ nestedAliasForType = aliasForType as INestedAliasForType;
      if (nestedAliasForType != null) return this.Visit(this.GetMutableCopy(nestedAliasForType));
      //TODO: error
      return aliasForType;
    }

    /// <summary>
    /// Makes a (shallow) mutable copy of the given assembly and then visits the copy, which generally
    /// results in a deep mutable copy, depending on how subclasses override the behavior of the methods
    /// of this base class.
    /// </summary>
    /// <param name="assembly">The assembly to copy.</param>
    public virtual Assembly Visit(IAssembly assembly) {
      return this.Visit(this.GetMutableCopy(assembly));
    }

    /// <summary>
    /// Visits the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public virtual Assembly Visit(Assembly assembly) {
      if (this.stopTraversal) return assembly;
      this.path.Push(assembly);
      assembly.AssemblyAttributes = this.Visit(assembly.AssemblyAttributes);
      assembly.ExportedTypes = this.Visit(assembly.ExportedTypes);
      assembly.Files = this.Visit(assembly.Files);
      assembly.MemberModules = this.Visit(assembly.MemberModules);
      assembly.Resources = this.Visit(assembly.Resources);
      assembly.SecurityAttributes = this.Visit(assembly.SecurityAttributes);
      this.path.Pop();
      this.Visit((Module)assembly);
      return assembly;
    }

    /// <summary>
    /// Visits the specified assembly references.
    /// </summary>
    /// <param name="assemblyReferences">The assembly references.</param>
    /// <returns></returns>
    public virtual List<IAssemblyReference>/*?*/ Visit(List<IAssemblyReference>/*?*/ assemblyReferences) {
      if (this.stopTraversal || assemblyReferences == null) return assemblyReferences;
      for (int i = 0, n = assemblyReferences.Count; i < n; i++)
        assemblyReferences[i] = this.Visit(assemblyReferences[i]);
      return assemblyReferences;
    }

    /// <summary>
    /// Visits the specified assembly reference.
    /// </summary>
    /// <param name="assemblyReference">The assembly reference.</param>
    /// <returns></returns>
    public virtual IAssemblyReference Visit(IAssemblyReference assemblyReference) {
      return this.Visit(this.GetMutableCopy(assemblyReference));
    }

    /// <summary>
    /// Visits the specified assembly reference.
    /// </summary>
    /// <param name="assemblyReference">The assembly reference.</param>
    /// <returns></returns>
    public virtual AssemblyReference Visit(AssemblyReference assemblyReference) {
      if (!(assemblyReference.ResolvedAssembly is Dummy)) {
        object/*?*/ mutatedResolvedAssembly = null;
        if (this.cache.TryGetValue(assemblyReference.ResolvedAssembly, out mutatedResolvedAssembly))
          assemblyReference.ResolvedAssembly = (IAssembly)mutatedResolvedAssembly;
      }
      assemblyReference.Host = this.host;
      assemblyReference.ReferringUnit = this.GetCurrentUnit();
      return assemblyReference;
    }

    /// <summary>
    /// Visits the specified custom attributes.
    /// </summary>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns></returns>
    public virtual List<ICustomAttribute>/*?*/ Visit(List<ICustomAttribute>/*?*/ customAttributes) {
      if (this.stopTraversal || customAttributes == null) return customAttributes;
      for (int i = 0, n = customAttributes.Count; i < n; i++)
        customAttributes[i] = this.Visit(this.GetMutableCopy(customAttributes[i]));
      return customAttributes;
    }

    /// <summary>
    /// Visits the specified custom attribute.
    /// </summary>
    /// <param name="customAttribute">The custom attribute.</param>
    /// <returns></returns>
    public virtual CustomAttribute Visit(CustomAttribute customAttribute) {
      if (this.stopTraversal) return customAttribute;
      this.path.Push(customAttribute);
      customAttribute.Arguments = this.Visit(customAttribute.Arguments);
      customAttribute.Constructor = this.Visit(customAttribute.Constructor);
      customAttribute.NamedArguments = this.Visit(customAttribute.NamedArguments);
      this.path.Pop();
      return customAttribute;
    }

    /// <summary>
    /// Visits the specified custom modifiers.
    /// </summary>
    /// <param name="customModifiers">The custom modifiers.</param>
    /// <returns></returns>
    public virtual List<ICustomModifier>/*?*/ Visit(List<ICustomModifier>/*?*/ customModifiers) {
      if (this.stopTraversal || customModifiers == null) return customModifiers;
      for (int i = 0, n = customModifiers.Count; i < n; i++)
        customModifiers[i] = this.Visit(this.GetMutableCopy(customModifiers[i]));
      return customModifiers;
    }

    /// <summary>
    /// Visits the specified custom modifier.
    /// </summary>
    /// <param name="customModifier">The custom modifier.</param>
    /// <returns></returns>
    public virtual CustomModifier Visit(CustomModifier customModifier) {
      if (this.stopTraversal) return customModifier;
      this.path.Push(customModifier);
      customModifier.Modifier = this.Visit(customModifier.Modifier);
      this.path.Pop();
      return customModifier;
    }

    /// <summary>
    /// Visits the specified event definitions.
    /// </summary>
    /// <param name="eventDefinitions">The event definitions.</param>
    /// <returns></returns>
    public virtual List<IEventDefinition>/*?*/ Visit(List<IEventDefinition>/*?*/ eventDefinitions) {
      if (this.stopTraversal || eventDefinitions == null) return eventDefinitions;
      for (int i = 0, n = eventDefinitions.Count; i < n; i++)
        eventDefinitions[i] = this.Visit(eventDefinitions[i]);
      return eventDefinitions;
    }

    /// <summary>
    /// Visits the specified event definition.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <returns></returns>
    public virtual IEventDefinition Visit(IEventDefinition eventDefinition) {
      return this.Visit(this.GetMutableCopy(eventDefinition));
    }

    /// <summary>
    /// Visits the specified event definition.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <returns></returns>
    public virtual EventDefinition Visit(EventDefinition eventDefinition) {
      if (this.stopTraversal) return eventDefinition;
      this.Visit((TypeDefinitionMember)eventDefinition);
      this.path.Push(eventDefinition);
      eventDefinition.Accessors = this.Visit(eventDefinition.Accessors);
      eventDefinition.Adder = this.Visit(eventDefinition.Adder);
      if (eventDefinition.Caller != null)
        eventDefinition.Caller = this.Visit(eventDefinition.Caller);
      eventDefinition.Remover = this.Visit(eventDefinition.Remover);
      eventDefinition.Type = this.Visit(eventDefinition.Type);
      this.path.Pop();
      return eventDefinition;
    }

    /// <summary>
    /// Visits the specified field definitions.
    /// </summary>
    /// <param name="fieldDefinitions">The field definitions.</param>
    /// <returns></returns>
    public virtual List<IFieldDefinition>/*?*/ Visit(List<IFieldDefinition>/*?*/ fieldDefinitions) {
      if (this.stopTraversal || fieldDefinitions == null) return fieldDefinitions;
      for (int i = 0, n = fieldDefinitions.Count; i < n; i++)
        fieldDefinitions[i] = this.Visit(fieldDefinitions[i]);
      return fieldDefinitions;
    }

    /// <summary>
    /// Visits the specified field definition.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <returns></returns>
    public virtual FieldDefinition Visit(FieldDefinition fieldDefinition) {
      if (this.stopTraversal) return fieldDefinition;
      this.Visit((TypeDefinitionMember)fieldDefinition);
      this.path.Push(fieldDefinition);
      if (fieldDefinition.IsCompileTimeConstant)
        fieldDefinition.CompileTimeValue = this.Visit(this.GetMutableCopy(fieldDefinition.CompileTimeValue));
      if (fieldDefinition.IsMapped)
        fieldDefinition.FieldMapping = this.Visit(this.GetMutableCopy(fieldDefinition.FieldMapping));
      if (fieldDefinition.IsMarshalledExplicitly)
        fieldDefinition.MarshallingInformation = this.Visit(this.GetMutableCopy(fieldDefinition.MarshallingInformation));
      if (fieldDefinition.IsModified)
        fieldDefinition.CustomModifiers = this.Visit(fieldDefinition.CustomModifiers);
      fieldDefinition.Type = this.Visit(fieldDefinition.Type);
      this.path.Pop();
      return fieldDefinition;
    }

    /// <summary>
    /// Visits the specified field definition.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <returns></returns>
    public virtual IFieldDefinition Visit(IFieldDefinition fieldDefinition) {
      return this.Visit(this.GetMutableCopy(fieldDefinition));
    }

    /// <summary>
    /// Visits the specified field reference.
    /// </summary>
    /// <param name="fieldReference">The field reference.</param>
    /// <returns></returns>
    public virtual IFieldReference Visit(IFieldReference fieldReference) {
      if (this.stopTraversal) return fieldReference;
      if (fieldReference is Dummy) return Dummy.FieldReference;
      ISpecializedFieldReference/*?*/ specializedFieldReference = fieldReference as ISpecializedFieldReference;
      if (specializedFieldReference != null)
        return this.Visit(this.GetMutableCopy(specializedFieldReference));
      IGlobalFieldDefinition/*?*/ globalFieldDefinition = fieldReference as IGlobalFieldDefinition;
      if (globalFieldDefinition != null)
        return this.GetMutableCopy(globalFieldDefinition);
      IFieldDefinition/*?*/ fieldDefinition = fieldReference as IFieldDefinition;
      if (fieldDefinition != null)
        return this.GetMutableCopy(fieldDefinition);
      return this.Visit(this.GetMutableCopy(fieldReference));
    }

    /// <summary>
    /// Visits the specified field reference.
    /// </summary>
    /// <param name="fieldReference">The field reference.</param>
    /// <returns></returns>
    public virtual FieldReference Visit(FieldReference fieldReference) {
      if (this.stopTraversal) return fieldReference;
      this.path.Push(fieldReference);
      fieldReference.Attributes = this.Visit(fieldReference.Attributes);
      fieldReference.ContainingType = this.Visit(fieldReference.ContainingType);
      if (fieldReference.IsModified)
        fieldReference.CustomModifiers = this.Visit(fieldReference.CustomModifiers);
      fieldReference.Locations = this.Visit(fieldReference.Locations);
      fieldReference.Type = this.Visit(fieldReference.Type);
      this.path.Pop();
      return fieldReference;
    }

    /// <summary>
    /// Visits the specified file references.
    /// </summary>
    /// <param name="fileReferences">The file references.</param>
    /// <returns></returns>
    public virtual List<IFileReference>/*?*/ Visit(List<IFileReference>/*?*/ fileReferences) {
      if (this.stopTraversal || fileReferences == null) return fileReferences;
      for (int i = 0, n = fileReferences.Count; i < n; i++)
        fileReferences[i] = this.Visit(this.GetMutableCopy(fileReferences[i]));
      return fileReferences;
    }

    /// <summary>
    /// Visits the specified file reference.
    /// </summary>
    /// <param name="fileReference">The file reference.</param>
    /// <returns></returns>
    public virtual FileReference Visit(FileReference fileReference) {
      return fileReference;
    }

    /// <summary>
    /// Visits the specified function pointer type reference.
    /// </summary>
    /// <param name="functionPointerTypeReference">The function pointer type reference.</param>
    /// <returns></returns>
    public virtual FunctionPointerTypeReference Visit(FunctionPointerTypeReference functionPointerTypeReference) {
      if (this.stopTraversal) return functionPointerTypeReference;
      this.Visit((TypeReference)functionPointerTypeReference);
      this.path.Push(functionPointerTypeReference);
      functionPointerTypeReference.ExtraArgumentTypes = this.Visit(functionPointerTypeReference.ExtraArgumentTypes);
      functionPointerTypeReference.Parameters = this.Visit(functionPointerTypeReference.Parameters);
      if (functionPointerTypeReference.ReturnValueIsModified)
        functionPointerTypeReference.ReturnValueCustomModifiers = this.Visit(functionPointerTypeReference.ReturnValueCustomModifiers);
      functionPointerTypeReference.Type = this.Visit(functionPointerTypeReference.Type);
      this.path.Pop();
      return functionPointerTypeReference;
    }

    /// <summary>
    /// Visits the specified generic method instance reference.
    /// </summary>
    /// <param name="genericMethodInstanceReference">The generic method instance reference.</param>
    /// <returns></returns>
    public virtual GenericMethodInstanceReference Visit(GenericMethodInstanceReference genericMethodInstanceReference) {
      if (this.stopTraversal) return genericMethodInstanceReference;
      this.Visit((MethodReference)genericMethodInstanceReference);
      this.path.Push(genericMethodInstanceReference);
      genericMethodInstanceReference.GenericArguments = this.Visit(genericMethodInstanceReference.GenericArguments);
      genericMethodInstanceReference.GenericMethod = this.Visit(genericMethodInstanceReference.GenericMethod);
      this.path.Pop();
      return genericMethodInstanceReference;
    }

    /// <summary>
    /// Visits the specified generic method parameters.
    /// </summary>
    /// <param name="genericMethodParameters">The generic method parameters.</param>
    /// <param name="declaringMethod">The declaring method.</param>
    /// <returns></returns>
    public virtual List<IGenericMethodParameter>/*?*/ Visit(List<IGenericMethodParameter>/*?*/ genericMethodParameters, IMethodDefinition declaringMethod) {
      if (this.stopTraversal || genericMethodParameters == null) return genericMethodParameters;
      for (int i = 0, n = genericMethodParameters.Count; i < n; i++)
        genericMethodParameters[i] = this.Visit(this.GetMutableCopy(genericMethodParameters[i]));
      return genericMethodParameters;
    }

    /// <summary>
    /// Visits the specified generic method parameter.
    /// </summary>
    /// <param name="genericMethodParameter">The generic method parameter.</param>
    /// <returns></returns>
    public virtual GenericMethodParameter Visit(GenericMethodParameter genericMethodParameter) {
      if (this.stopTraversal) return genericMethodParameter;
      this.Visit((GenericParameter)genericMethodParameter);
      genericMethodParameter.DefiningMethod = this.GetCurrentMethod();
      return genericMethodParameter;
    }

    /// <summary>
    /// Visits the specified generic method parameter reference.
    /// </summary>
    /// <param name="genericMethodParameterReference">The generic method parameter reference.</param>
    /// <returns></returns>
    public virtual GenericMethodParameterReference Visit(GenericMethodParameterReference genericMethodParameterReference) {
      if (this.stopTraversal) return genericMethodParameterReference;
      this.Visit((TypeReference)genericMethodParameterReference);
      this.path.Push(genericMethodParameterReference);
      var definingMethod = this.GetTypeSpecificMutableCopy(genericMethodParameterReference.DefiningMethod);
      if (definingMethod != genericMethodParameterReference.DefiningMethod) {
        genericMethodParameterReference.DefiningMethod = definingMethod;
        genericMethodParameterReference.DefiningMethod = this.Visit(definingMethod);
      }
      this.path.Pop();
      return genericMethodParameterReference;
    }

    /// <summary>
    /// Visits the specified generic type parameter reference.
    /// </summary>
    /// <param name="genericTypeParameterReference">The generic type parameter reference.</param>
    /// <returns></returns>
    public virtual GenericTypeParameterReference Visit(GenericTypeParameterReference genericTypeParameterReference) {
      if (this.stopTraversal) return genericTypeParameterReference;
      this.Visit((TypeReference)genericTypeParameterReference);
      this.path.Push(genericTypeParameterReference);
      genericTypeParameterReference.DefiningType = this.Visit(genericTypeParameterReference.DefiningType);
      this.path.Pop();
      return genericTypeParameterReference;
    }

    /// <summary>
    /// Visits the specified global field definition.
    /// </summary>
    /// <param name="globalFieldDefinition">The global field definition.</param>
    /// <returns></returns>
    public virtual GlobalFieldDefinition Visit(GlobalFieldDefinition globalFieldDefinition) {
      if (this.stopTraversal) return globalFieldDefinition;
      this.path.Push(this.Visit(globalFieldDefinition.ContainingTypeDefinition));
      this.Visit((FieldDefinition)globalFieldDefinition);
      this.path.Pop();
      globalFieldDefinition.ContainingNamespace = this.GetCurrentNamespace();
      return globalFieldDefinition;
    }

    /// <summary>
    /// Visits the specified global method definition.
    /// </summary>
    /// <param name="globalMethodDefinition">The global method definition.</param>
    /// <returns></returns>
    public virtual GlobalMethodDefinition Visit(GlobalMethodDefinition globalMethodDefinition) {
      if (this.stopTraversal) return globalMethodDefinition;
      this.path.Push(this.Visit(globalMethodDefinition.ContainingTypeDefinition));
      this.Visit((MethodDefinition)globalMethodDefinition);
      this.path.Pop();
      globalMethodDefinition.ContainingNamespace = this.GetCurrentNamespace();
      return globalMethodDefinition;
    }

    /// <summary>
    /// Visits the specified generic type instance reference.
    /// </summary>
    /// <param name="genericTypeInstanceReference">The generic type instance reference.</param>
    /// <returns></returns>
    public virtual GenericTypeInstanceReference Visit(GenericTypeInstanceReference genericTypeInstanceReference) {
      if (this.stopTraversal) return genericTypeInstanceReference;
      this.Visit((TypeReference)genericTypeInstanceReference);
      this.path.Push(genericTypeInstanceReference);
      genericTypeInstanceReference.GenericArguments = this.Visit(genericTypeInstanceReference.GenericArguments);
      genericTypeInstanceReference.GenericType = (INamedTypeReference)this.Visit(genericTypeInstanceReference.GenericType);
      this.path.Pop();
      return genericTypeInstanceReference;
    }

    /// <summary>
    /// Visits the specified generic parameter.
    /// </summary>
    /// <param name="genericParameter">The generic parameter.</param>
    /// <returns></returns>
    public virtual GenericParameter Visit(GenericParameter genericParameter) {
      if (this.stopTraversal) return genericParameter;
      this.path.Push(genericParameter);
      genericParameter.Attributes = this.Visit(genericParameter.Attributes);
      genericParameter.Constraints = this.Visit(genericParameter.Constraints);
      this.path.Pop();
      return genericParameter;
    }

    /// <summary>
    /// Visits the specified generic type parameters.
    /// </summary>
    /// <param name="genericTypeParameters">The generic type parameters.</param>
    /// <returns></returns>
    public virtual List<IGenericTypeParameter>/*?*/ Visit(List<IGenericTypeParameter>/*?*/ genericTypeParameters) {
      if (this.stopTraversal || genericTypeParameters == null) return genericTypeParameters;
      for (int i = 0, n = genericTypeParameters.Count; i < n; i++)
        genericTypeParameters[i] = this.Visit(this.GetMutableCopy(genericTypeParameters[i]));
      return genericTypeParameters;
    }

    /// <summary>
    /// Visits the specified generic type parameter.
    /// </summary>
    /// <param name="genericTypeParameter">The generic type parameter.</param>
    /// <returns></returns>
    public virtual GenericTypeParameter Visit(GenericTypeParameter genericTypeParameter) {
      if (this.stopTraversal) return genericTypeParameter;
      this.Visit((GenericParameter)genericTypeParameter);
      genericTypeParameter.DefiningType = this.GetCurrentType();
      return genericTypeParameter;
    }

    /// <summary>
    /// Visits the specified locations.
    /// </summary>
    /// <param name="locations">The locations.</param>
    /// <returns></returns>
    public virtual List<ILocation>/*?*/ Visit(List<ILocation>/*?*/ locations) {
      if (this.stopTraversal || locations == null) return locations;
      for (int i = 0, n = locations.Count; i < n; i++)
        locations[i] = this.Visit(locations[i]);
      return locations;
    }

    /// <summary>
    /// Visits the specified local definition.
    /// </summary>
    /// <param name="localDefinition">The local definition to visit.</param>
    public virtual ILocalDefinition Visit(ILocalDefinition localDefinition) {
      return this.Visit(this.GetMutableCopy(localDefinition));
    }

    /// <summary>
    /// Visits a reference to the specified local definition.
    /// </summary>
    /// <param name="localDefinition">The referenced local definition to visit.</param>
    public virtual ILocalDefinition VisitReferenceTo(ILocalDefinition localDefinition) {
      //The referrer must refer to the same copy of the local definition that was (or will be) produced by a visit to the actual definition.
      return this.GetMutableCopy(localDefinition);
    }

    /// <summary>
    /// Visits the specified location.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns></returns>
    public virtual ILocation Visit(ILocation location) {
      return location;
    }

    /// <summary>
    /// Visits the specified local definition.
    /// </summary>
    /// <param name="localDefinition">The local definition.</param>
    /// <returns></returns>
    public virtual LocalDefinition Visit(LocalDefinition localDefinition) {
      if (this.stopTraversal) return localDefinition;
      this.path.Push(localDefinition);
      localDefinition.CustomModifiers = this.Visit(localDefinition.CustomModifiers);
      localDefinition.Type = this.Visit(localDefinition.Type);
      this.path.Pop();
      return localDefinition;
    }

    /// <summary>
    /// Visits the specified managed pointer type reference.
    /// </summary>
    /// <param name="managedPointerTypeReference">The pointer type reference.</param>
    /// <returns></returns>
    public virtual ManagedPointerTypeReference Visit(ManagedPointerTypeReference managedPointerTypeReference) {
      if (this.stopTraversal) return managedPointerTypeReference;
      this.Visit((TypeReference)managedPointerTypeReference);
      this.path.Push(managedPointerTypeReference);
      managedPointerTypeReference.TargetType = this.Visit(managedPointerTypeReference.TargetType);
      this.path.Pop();
      return managedPointerTypeReference;
    }

    /// <summary>
    /// Visits the specified marshalling information.
    /// </summary>
    /// <param name="marshallingInformation">The marshalling information.</param>
    /// <returns></returns>
    public virtual MarshallingInformation Visit(MarshallingInformation marshallingInformation) {
      if (this.stopTraversal) return marshallingInformation;
      this.path.Push(marshallingInformation);
      if (marshallingInformation.UnmanagedType == UnmanagedType.CustomMarshaler)
        marshallingInformation.CustomMarshaller = this.Visit(marshallingInformation.CustomMarshaller);
      if (marshallingInformation.UnmanagedType == UnmanagedType.SafeArray && 
      (marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_DISPATCH || 
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_UNKNOWN || 
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_RECORD))
        marshallingInformation.SafeArrayElementUserDefinedSubtype = this.Visit(marshallingInformation.SafeArrayElementUserDefinedSubtype);
      this.path.Pop();
      return marshallingInformation;
    }

    /// <summary>
    /// Visits the specified constant.
    /// </summary>
    /// <param name="constant">The constant.</param>
    /// <returns></returns>
    public virtual MetadataConstant Visit(MetadataConstant constant) {
      if (this.stopTraversal) return constant;
      this.path.Push(constant);
      constant.Locations = this.Visit(constant.Locations);
      constant.Type = this.Visit(constant.Type);
      this.path.Pop();
      return constant;
    }

    /// <summary>
    /// Visits the specified create array.
    /// </summary>
    /// <param name="createArray">The create array.</param>
    /// <returns></returns>
    public virtual MetadataCreateArray Visit(MetadataCreateArray createArray) {
      if (this.stopTraversal) return createArray;
      this.path.Push(createArray);
      createArray.ElementType = this.Visit(createArray.ElementType);
      createArray.Initializers = this.Visit(createArray.Initializers);
      createArray.Locations = this.Visit(createArray.Locations);
      createArray.Type = this.Visit(createArray.Type);
      this.path.Pop();
      return createArray;
    }

    /// <summary>
    /// Visits the specified metadata expressions.
    /// </summary>
    /// <param name="metadataExpressions">The metadata expressions.</param>
    /// <returns></returns>
    public virtual List<IMetadataExpression>/*?*/ Visit(List<IMetadataExpression>/*?*/ metadataExpressions) {
      if (this.stopTraversal || metadataExpressions == null) return metadataExpressions;
      for (int i = 0, n = metadataExpressions.Count; i < n; i++)
        metadataExpressions[i] = this.Visit(metadataExpressions[i]);
      return metadataExpressions;
    }

    /// <summary>
    /// Visits the specified expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public virtual IMetadataExpression Visit(IMetadataExpression expression) {
      if (this.stopTraversal) return expression;
      IMetadataConstant/*?*/ metadataConstant = expression as IMetadataConstant;
      if (metadataConstant != null) return this.Visit(this.GetMutableCopy(metadataConstant));
      IMetadataCreateArray/*?*/ metadataCreateArray = expression as IMetadataCreateArray;
      if (metadataCreateArray != null) return this.Visit(this.GetMutableCopy(metadataCreateArray));
      IMetadataTypeOf/*?*/ metadataTypeOf = expression as IMetadataTypeOf;
      if (metadataTypeOf != null) return this.Visit(this.GetMutableCopy(metadataTypeOf));
      return expression;
    }

    /// <summary>
    /// Visits the specified named arguments.
    /// </summary>
    /// <param name="namedArguments">The named arguments.</param>
    /// <returns></returns>
    public virtual List<IMetadataNamedArgument>/*?*/ Visit(List<IMetadataNamedArgument>/*?*/ namedArguments) {
      if (this.stopTraversal || namedArguments == null) return namedArguments;
      for (int i = 0, n = namedArguments.Count; i < n; i++)
        namedArguments[i] = this.Visit(this.GetMutableCopy(namedArguments[i]));
      return namedArguments;
    }

    /// <summary>
    /// Visits the specified named argument.
    /// </summary>
    /// <param name="namedArgument">The named argument.</param>
    /// <returns></returns>
    public virtual MetadataNamedArgument Visit(MetadataNamedArgument namedArgument) {
      if (this.stopTraversal) return namedArgument;
      this.path.Push(namedArgument);
      namedArgument.ArgumentValue = this.Visit(namedArgument.ArgumentValue);
      namedArgument.Locations = this.Visit(namedArgument.Locations);
      namedArgument.Type = this.Visit(namedArgument.Type);
      this.path.Pop();
      return namedArgument;
    }

    /// <summary>
    /// Visits the specified type of.
    /// </summary>
    /// <param name="typeOf">The type of.</param>
    /// <returns></returns>
    public virtual MetadataTypeOf Visit(MetadataTypeOf typeOf) {
      if (this.stopTraversal) return typeOf;
      this.path.Push(typeOf);
      typeOf.Locations = this.Visit(typeOf.Locations);
      typeOf.Type = this.Visit(typeOf.Type);
      typeOf.TypeToGet = this.Visit(typeOf.TypeToGet);
      this.path.Pop();
      return typeOf;
    }

    /// <summary>
    /// Visits the specified matrix type reference.
    /// </summary>
    /// <param name="matrixTypeReference">The matrix type reference.</param>
    /// <returns></returns>
    public virtual MatrixTypeReference Visit(MatrixTypeReference matrixTypeReference) {
      if (this.stopTraversal) return matrixTypeReference;
      this.Visit((TypeReference)matrixTypeReference);
      this.path.Push(matrixTypeReference);
      matrixTypeReference.ElementType = this.Visit(matrixTypeReference.ElementType);
      this.path.Pop();
      return matrixTypeReference;
    }

    /// <summary>
    /// Visits the specified method body.
    /// </summary>
    /// <param name="methodBody">The method body.</param>
    /// <returns></returns>
    public virtual MethodBody Visit(MethodBody methodBody) {
      if (this.stopTraversal) return methodBody;
      this.path.Push(methodBody);
      methodBody.MethodDefinition = this.GetCurrentMethod();
      methodBody.LocalVariables = this.Visit(methodBody.LocalVariables);
      methodBody.Operations = this.Visit(methodBody.Operations);
      methodBody.OperationExceptionInformation = this.Visit(methodBody.OperationExceptionInformation);
      this.path.Pop();
      return methodBody;
    }

    /// <summary>
    /// Visits the specified exception informations.
    /// </summary>
    /// <param name="exceptionInformations">The exception informations.</param>
    /// <returns></returns>
    public virtual List<IOperationExceptionInformation>/*?*/ Visit(List<IOperationExceptionInformation>/*?*/ exceptionInformations) {
      if (this.stopTraversal || exceptionInformations == null) return exceptionInformations;
      for (int i = 0, n = exceptionInformations.Count; i < n; i++)
        exceptionInformations[i] = this.Visit(this.GetMutableCopy(exceptionInformations[i]));
      return exceptionInformations;
    }

    /// <summary>
    /// Visits the specified operation exception information.
    /// </summary>
    /// <param name="operationExceptionInformation">The operation exception information.</param>
    /// <returns></returns>
    public virtual OperationExceptionInformation Visit(OperationExceptionInformation operationExceptionInformation) {
      if (this.stopTraversal) return operationExceptionInformation;
      this.path.Push(operationExceptionInformation);
      operationExceptionInformation.ExceptionType = this.Visit(operationExceptionInformation.ExceptionType);
      this.path.Pop();
      return operationExceptionInformation;
    }

    /// <summary>
    /// Visits the specified operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <returns></returns>
    public virtual List<IOperation>/*?*/ Visit(List<IOperation>/*?*/ operations) {
      if (this.stopTraversal || operations == null) return operations;
      for (int i = 0, n = operations.Count; i < n; i++)
        operations[i] = this.Visit(this.GetMutableCopy(operations[i]));
      return operations;
    }

    /// <summary>
    /// Visits the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <returns></returns>
    public virtual Operation Visit(Operation operation) {
      if (this.stopTraversal) return operation;
      this.path.Push(operation);
      ITypeReference/*?*/ typeReference = operation.Value as ITypeReference;
      if (typeReference != null)
        operation.Value = this.Visit(typeReference);
      else {
        IFieldReference/*?*/ fieldReference = operation.Value as IFieldReference;
        if (fieldReference != null)
          operation.Value = this.Visit(fieldReference);
        else {
          IMethodReference/*?*/ methodReference = operation.Value as IMethodReference;
          if (methodReference != null)
            operation.Value = this.Visit(methodReference);
          else {
            IParameterDefinition/*?*/ parameterDefinition = operation.Value as IParameterDefinition;
            if (parameterDefinition != null)
              operation.Value = this.VisitReferenceTo(parameterDefinition);
            else {
              ILocalDefinition/*?*/ localDefinition = operation.Value as ILocalDefinition;
              if (localDefinition != null)
                operation.Value = this.VisitReferenceTo(localDefinition);
            }
          }
        }
      }
      this.path.Pop();
      return operation;
    }

    /// <summary>
    /// Visits the specified locals.
    /// </summary>
    /// <param name="locals">The locals.</param>
    /// <returns></returns>
    public virtual List<ILocalDefinition>/*?*/ Visit(List<ILocalDefinition>/*?*/ locals) {
      if (this.stopTraversal || locals == null) return locals;
      for (int i = 0, n = locals.Count; i < n; i++)
        locals[i] = this.Visit(locals[i]);
      return locals;
    }

    /// <summary>
    /// Visits the specified method definitions.
    /// </summary>
    /// <param name="methodDefinitions">The method definitions.</param>
    /// <returns></returns>
    public virtual List<IMethodDefinition>/*?*/ Visit(List<IMethodDefinition>/*?*/ methodDefinitions) {
      if (this.stopTraversal || methodDefinitions == null) return methodDefinitions;
      for (int i = 0, n = methodDefinitions.Count; i < n; i++)
        methodDefinitions[i] = this.Visit(methodDefinitions[i]);
      return methodDefinitions;
    }

    /// <summary>
    /// Visits the specified global field definition.
    /// </summary>
    /// <param name="globalFieldDefinition">The global field definition.</param>
    /// <returns></returns>
    public virtual IGlobalFieldDefinition Visit(IGlobalFieldDefinition globalFieldDefinition) {
      return this.Visit(this.GetMutableCopy(globalFieldDefinition));
    }

    /// <summary>
    /// Visits the specified global method definition.
    /// </summary>
    /// <param name="globalMethodDefinition">The global method definition.</param>
    /// <returns></returns>
    public virtual IGlobalMethodDefinition Visit(IGlobalMethodDefinition globalMethodDefinition) {
      return this.Visit(this.GetMutableCopy(globalMethodDefinition));
    }

    /// <summary>
    /// Visits the specified method definition.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns></returns>
    public virtual IMethodDefinition Visit(IMethodDefinition methodDefinition) {
      return this.Visit(this.GetMutableCopy(methodDefinition));
    }

    /// <summary>
    /// Visits the specified method definition.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns></returns>
    public virtual MethodDefinition Visit(MethodDefinition methodDefinition) {
      if (this.stopTraversal) return methodDefinition;
      this.Visit((TypeDefinitionMember)methodDefinition);
      this.path.Push(methodDefinition);
      if (methodDefinition.IsGeneric)
        methodDefinition.GenericParameters = this.Visit(methodDefinition.GenericParameters, methodDefinition);
      methodDefinition.Parameters = this.Visit(methodDefinition.Parameters);
      if (methodDefinition.IsPlatformInvoke)
        methodDefinition.PlatformInvokeData = this.Visit(this.GetMutableCopy(methodDefinition.PlatformInvokeData));
      methodDefinition.ReturnValueAttributes = this.VisitMethodReturnValueAttributes(methodDefinition.ReturnValueAttributes);
      if (methodDefinition.ReturnValueIsModified)
        methodDefinition.ReturnValueCustomModifiers = this.VisitMethodReturnValueCustomModifiers(methodDefinition.ReturnValueCustomModifiers);
      if (methodDefinition.ReturnValueIsMarshalledExplicitly)
        methodDefinition.ReturnValueMarshallingInformation = this.VisitMethodReturnValueMarshallingInformation(this.GetMutableCopy(methodDefinition.ReturnValueMarshallingInformation));
      if (methodDefinition.HasDeclarativeSecurity)
        methodDefinition.SecurityAttributes = this.Visit(methodDefinition.SecurityAttributes);
      methodDefinition.Type = this.Visit(methodDefinition.Type);
      if (!methodDefinition.IsAbstract && !methodDefinition.IsExternal)
        methodDefinition.Body = this.Visit(methodDefinition.Body);
      this.path.Pop();
      return methodDefinition;
    }

    /// <summary>
    /// Visits the specified method implementations.
    /// </summary>
    /// <param name="methodImplementations">The method implementations.</param>
    /// <returns></returns>
    public virtual List<IMethodImplementation>/*?*/ Visit(List<IMethodImplementation>/*?*/ methodImplementations) {
      if (this.stopTraversal || methodImplementations == null) return methodImplementations;
      for (int i = 0, n = methodImplementations.Count; i < n; i++)
        methodImplementations[i] = this.Visit(this.GetMutableCopy(methodImplementations[i]));
      return methodImplementations;
    }

    /// <summary>
    /// Visits the specified method implementation.
    /// </summary>
    /// <param name="methodImplementation">The method implementation.</param>
    /// <returns></returns>
    public virtual MethodImplementation Visit(MethodImplementation methodImplementation) {
      if (this.stopTraversal) return methodImplementation;
      this.path.Push(methodImplementation);
      methodImplementation.ContainingType = this.GetCurrentType();
      methodImplementation.ImplementedMethod = this.Visit(methodImplementation.ImplementedMethod);
      methodImplementation.ImplementingMethod = this.Visit(methodImplementation.ImplementingMethod);
      this.path.Pop();
      return methodImplementation;
    }

    /// <summary>
    /// Visits the specified method references.
    /// </summary>
    /// <param name="methodReferences">The method references.</param>
    /// <returns></returns>
    public virtual List<IMethodReference>/*?*/ Visit(List<IMethodReference>/*?*/ methodReferences) {
      if (this.stopTraversal || methodReferences == null) return methodReferences;
      for (int i = 0, n = methodReferences.Count; i < n; i++)
        methodReferences[i] = this.Visit(methodReferences[i]);
      return methodReferences;
    }

    /// <summary>
    /// Visits the specified method body.
    /// </summary>
    /// <param name="methodBody">The method body.</param>
    /// <returns></returns>
    public virtual IMethodBody Visit(IMethodBody methodBody) {
      return this.Visit(this.GetMutableCopy(methodBody));
    }

    /// <summary>
    /// Visits the specified method reference.
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns></returns>
    public virtual IMethodReference Visit(IMethodReference methodReference) {
      if (this.stopTraversal) return methodReference;
      if (methodReference is Dummy) return Dummy.MethodReference;
      ISpecializedMethodReference/*?*/ specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null)
        return this.Visit(this.GetMutableCopy(specializedMethodReference));
      IGenericMethodInstanceReference/*?*/ genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null)
        return this.Visit(this.GetMutableCopy(genericMethodInstanceReference));
      else {
        IGlobalMethodDefinition/*?*/ globalMethodDefinition = methodReference as IGlobalMethodDefinition;
        if (globalMethodDefinition != null)
          return this.GetMutableCopy(globalMethodDefinition);
        IMethodDefinition/*?*/ methodDefinition = methodReference as IMethodDefinition;
        if (methodDefinition != null) {
          return this.GetMutableCopy(methodDefinition);
        }
        return this.Visit(this.GetMutableCopy(methodReference));
      }
    }

    /// <summary>
    /// Visits the specified method reference.
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns></returns>
    public virtual MethodReference Visit(MethodReference methodReference) {
      if (this.stopTraversal) return methodReference;
      this.path.Push(methodReference);
      methodReference.Attributes = this.Visit(methodReference.Attributes);
      methodReference.ContainingType = this.Visit(methodReference.ContainingType);
      methodReference.ExtraParameters = this.Visit(methodReference.ExtraParameters);
      methodReference.Locations = this.Visit(methodReference.Locations);
      methodReference.Parameters = this.Visit(methodReference.Parameters);
      if (methodReference.ReturnValueIsModified)
        methodReference.ReturnValueCustomModifiers = this.Visit(methodReference.ReturnValueCustomModifiers);
      methodReference.Type = this.Visit(methodReference.Type);
      this.path.Pop();
      return methodReference;
    }

    /// <summary>
    /// Visits the specified modules.
    /// </summary>
    /// <param name="modules">The modules.</param>
    /// <returns></returns>
    public virtual List<IModule>/*?*/ Visit(List<IModule>/*?*/ modules) {
      if (this.stopTraversal || modules == null) return modules;
      for (int i = 0, n = modules.Count; i < n; i++) {
        modules[i] = this.Visit(this.GetMutableCopy(modules[i]));
        this.flatListOfTypes.Clear();
      }
      return modules;
    }

    /// <summary>
    /// Visits the specified modified type reference.
    /// </summary>
    /// <param name="modifiedTypeReference">The modified type reference.</param>
    /// <returns></returns>
    public virtual ModifiedTypeReference Visit(ModifiedTypeReference modifiedTypeReference) {
      if (this.stopTraversal) return modifiedTypeReference;
      this.Visit((TypeReference)modifiedTypeReference);
      this.path.Push(modifiedTypeReference);
      modifiedTypeReference.CustomModifiers = this.Visit(modifiedTypeReference.CustomModifiers);
      modifiedTypeReference.UnmodifiedType = this.Visit(modifiedTypeReference.UnmodifiedType);
      this.path.Pop();
      return modifiedTypeReference;
    }

    /// <summary>
    /// Makes a (shallow) mutable copy of the given module and then visits the copy, which generally
    /// results in a deep mutable copy, depending on how subclasses override the behavior of the methods
    /// of this base class.
    /// </summary>
    /// <param name="module">The module to copy.</param>
    public virtual Module Visit(IModule module) {
      var assembly = module as IAssembly;
      if (assembly != null) return this.Visit(assembly);
      return this.Visit(this.GetMutableCopy(module));
    }

    /// <summary>
    /// Visits the specified module.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <returns></returns>
    public virtual Module Visit(Module module) {
      if (this.stopTraversal) return module;
      this.path.Push(module);
      module.AssemblyReferences = this.Visit(module.AssemblyReferences);
      module.Locations = this.Visit(module.Locations);
      module.UninterpretedSections = this.Visit(module.UninterpretedSections);
      module.ModuleAttributes = this.Visit(module.ModuleAttributes);
      module.ModuleReferences = this.Visit(module.ModuleReferences);
      module.Win32Resources = this.Visit(module.Win32Resources);
      module.UnitNamespaceRoot = this.Visit(this.GetMutableCopy((IRootUnitNamespace)module.UnitNamespaceRoot));
      this.path.Push(module.UnitNamespaceRoot);
      if (module.AllTypes.Count > 0)
        this.Visit(this.GetMutableCopy((INamespaceTypeDefinition)module.AllTypes[0]));
      if (module.AllTypes.Count > 1) {
        INamespaceTypeDefinition globalsType = module.AllTypes[1] as INamespaceTypeDefinition;
        if (globalsType != null && globalsType.Name.Value == "__Globals__")
          this.Visit(this.GetMutableCopy(globalsType));
      }
      this.path.Pop();
      if (!(module.EntryPoint is Dummy))
        module.EntryPoint = this.Visit(module.EntryPoint);
      this.VisitPrivateHelperMembers(this.flatListOfTypes);
      this.flatListOfTypes.Sort(new TypeOrderPreserver(module.AllTypes));
      module.AllTypes = this.flatListOfTypes;
      this.flatListOfTypes = new List<INamedTypeDefinition>();
      module.TypeMemberReferences = null;
      module.TypeReferences = null;
      this.path.Pop();
      return module;
    }


    /// <summary>
    /// A callback object provided to the sort routine to allow it to compare INamedTypeDefinition objects.
    /// The criteria used for the comparison is that if type x was before type y in the assembly of which a mutated copy is being constructed, then type x 
    /// should remain before type y in the mutated copy.
    /// </summary>
    public class TypeOrderPreserver : Comparer<INamedTypeDefinition> {

      Dictionary<string, int> oldOrder = new Dictionary<string, int>();

      /// <summary>
      /// A callback object provided to the sort routine to allow it to compare INamedTypeDefinition objects.
      /// The criteria used for the comparison is that if type x was before type y in the assembly of which a mutated copy is being constructed, then type x 
      /// should remain before type y in the mutated copy.
      /// </summary>
      /// <param name="oldTypeList">The list of types in assembly of which a mutated copy is being constructed. This implicitly specifies an ordering
      /// for the (preserved) types in the mutated copy.</param>
      public TypeOrderPreserver(List<INamedTypeDefinition> oldTypeList) {
        for (int i = 0, n = oldTypeList.Count; i < n; i++)
          this.oldOrder.Add(TypeHelper.GetTypeName(oldTypeList[i], NameFormattingOptions.TypeParameters), i);
      }

      /// <summary>
      /// Performs a comparison of two INamedTypeDefinition instances and returns a value indicating whether one object is less than, equal to, or greater than the other.
      /// The criteria used for the comparison is that if type x was before type y in the assembly of which a mutated copy is being constructed, then type x 
      /// should remain before type y in the mutated copy.
      /// </summary>
      /// <param name="x">The first INamedTypeDefinition instance to compare.</param>
      /// <param name="y">The second INamedTypeDefinition instance to compare.</param>
      /// <returns>
      /// Value Condition Less than zero <paramref name="x"/> is less than <paramref name="y"/>.Zero <paramref name="x"/> equals <paramref name="y"/>.Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
      /// </returns>
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
    /// Visits the specified module references.
    /// </summary>
    /// <param name="moduleReferences">The module references.</param>
    /// <returns></returns>
    public virtual List<IModuleReference>/*?*/ Visit(List<IModuleReference>/*?*/ moduleReferences) {
      if (this.stopTraversal || moduleReferences == null) return moduleReferences;
      for (int i = 0, n = moduleReferences.Count; i < n; i++)
        moduleReferences[i] = this.Visit(moduleReferences[i]);
      return moduleReferences;
    }

    /// <summary>
    /// Visits the specified module reference.
    /// </summary>
    /// <param name="moduleReference">The module reference.</param>
    /// <returns></returns>
    public virtual ModuleReference Visit(ModuleReference moduleReference) {
      if (!(moduleReference.ResolvedModule is Dummy)) {
        object/*?*/ mutatedResolvedModule = null;
        if (this.cache.TryGetValue(moduleReference.ResolvedModule, out mutatedResolvedModule))
          moduleReference.ResolvedModule = (IModule)mutatedResolvedModule;
      }
      moduleReference.Host = this.host;
      moduleReference.ReferringUnit = this.GetCurrentUnit();
      return moduleReference;
    }

    /// <summary>
    /// Visits the specified namespace members.
    /// </summary>
    /// <param name="namespaceMembers">The namespace members.</param>
    /// <returns></returns>
    public virtual List<INamespaceMember>/*?*/ Visit(List<INamespaceMember>/*?*/ namespaceMembers) {
      if (this.stopTraversal || namespaceMembers == null) return namespaceMembers;
      for (int i = 0, n = namespaceMembers.Count; i < n; i++)
        namespaceMembers[i] = this.Visit(namespaceMembers[i]);
      return namespaceMembers;
    }

    /// <summary>
    /// Visits the specified namespace member.
    /// </summary>
    /// <param name="namespaceMember">The namespace member.</param>
    /// <returns></returns>
    public virtual INamespaceMember Visit(INamespaceMember namespaceMember) {
      if (this.stopTraversal) return namespaceMember;
      INamespaceTypeDefinition/*?*/ namespaceTypeDefinition = namespaceMember as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) return this.Visit(namespaceTypeDefinition);
      INestedUnitNamespace/*?*/ nestedUnitNamespace = namespaceMember as INestedUnitNamespace;
      if (nestedUnitNamespace != null) return this.Visit(nestedUnitNamespace);
      IGlobalMethodDefinition/*?*/ globalMethodDefinition = namespaceMember as IGlobalMethodDefinition;
      if (globalMethodDefinition != null) return this.Visit(globalMethodDefinition);
      IGlobalFieldDefinition/*?*/ globalFieldDefinition = namespaceMember as IGlobalFieldDefinition;
      if (globalFieldDefinition != null) return this.Visit(globalFieldDefinition);
      return namespaceMember;
    }

    /// <summary>
    /// Visits the specified namespace alias for type.
    /// </summary>
    /// <param name="namespaceAliasForType">Type of the namespace alias for.</param>
    /// <returns></returns>
    public virtual NamespaceAliasForType Visit(NamespaceAliasForType namespaceAliasForType) {
      if (this.stopTraversal) return namespaceAliasForType;
      this.path.Push(namespaceAliasForType);
      namespaceAliasForType.AliasedType = (INamedTypeReference)this.Visit(namespaceAliasForType.AliasedType);
      namespaceAliasForType.Attributes = this.Visit(namespaceAliasForType.Attributes);
      namespaceAliasForType.Locations = this.Visit(namespaceAliasForType.Locations);
      //TODO: what about the containing namespace? Should that be a reference?
      this.path.Pop();
      return namespaceAliasForType;
    }

    /// <summary>
    /// Visits the specified namespace type definition.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The namespace type definition.</param>
    /// <returns></returns>
    public virtual NamespaceTypeDefinition Visit(NamespaceTypeDefinition namespaceTypeDefinition) {
      if (this.stopTraversal) return namespaceTypeDefinition;
      this.Visit((NamedTypeDefinition)namespaceTypeDefinition);
      namespaceTypeDefinition.ContainingUnitNamespace = this.GetCurrentNamespace();
      return namespaceTypeDefinition;
    }

    /// <summary>
    /// Visits the specified namespace type reference.
    /// </summary>
    /// <param name="namespaceTypeReference">The namespace type reference.</param>
    /// <returns></returns>
    public virtual NamespaceTypeReference Visit(NamespaceTypeReference namespaceTypeReference) {
      if (this.stopTraversal) return namespaceTypeReference;
      this.Visit((TypeReference)namespaceTypeReference);
      this.path.Push(namespaceTypeReference);
      namespaceTypeReference.ContainingUnitNamespace = this.Visit(namespaceTypeReference.ContainingUnitNamespace);
      this.path.Pop();
      return namespaceTypeReference;
    }

    /// <summary>
    /// Visits the specified nested alias for type.
    /// </summary>
    /// <param name="nestedAliasForType">Type of the nested alias for.</param>
    /// <returns></returns>
    public virtual NestedAliasForType Visit(NestedAliasForType nestedAliasForType) {
      if (this.stopTraversal) return nestedAliasForType;
      this.path.Push(nestedAliasForType);
      nestedAliasForType.AliasedType = (INamedTypeReference)this.Visit(nestedAliasForType.AliasedType);
      nestedAliasForType.Attributes = this.Visit(nestedAliasForType.Attributes);
      nestedAliasForType.Locations = this.Visit(nestedAliasForType.Locations);
      //TODO: what about the containing type? Should that be a reference?
      this.path.Pop();
      return nestedAliasForType;
    }

    /// <summary>
    /// Visits the specified nested type definitions.
    /// </summary>
    /// <param name="nestedTypeDefinitions">The nested type definitions.</param>
    /// <returns></returns>
    public virtual List<INestedTypeDefinition>/*?*/ Visit(List<INestedTypeDefinition>/*?*/ nestedTypeDefinitions) {
      if (this.stopTraversal || nestedTypeDefinitions == null) return nestedTypeDefinitions;
      for (int i = 0, n = nestedTypeDefinitions.Count; i < n; i++)
        nestedTypeDefinitions[i] = this.Visit(nestedTypeDefinitions[i]);
      return nestedTypeDefinitions;
    }

    /// <summary>
    /// Visits the specified nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition">The nested type definition.</param>
    /// <returns></returns>
    public virtual NestedTypeDefinition Visit(NestedTypeDefinition nestedTypeDefinition) {
      if (this.stopTraversal) return nestedTypeDefinition;
      this.Visit((NamedTypeDefinition)nestedTypeDefinition);
      nestedTypeDefinition.ContainingTypeDefinition = this.GetCurrentType();
      return nestedTypeDefinition;
    }

    /// <summary>
    /// Visits the specified nested type reference.
    /// </summary>
    /// <param name="nestedTypeReference">The nested type reference.</param>
    /// <returns></returns>
    public virtual NestedTypeReference Visit(NestedTypeReference nestedTypeReference) {
      if (this.stopTraversal) return nestedTypeReference;
      this.Visit((TypeReference)nestedTypeReference);
      this.path.Push(nestedTypeReference);
      nestedTypeReference.ContainingType = this.Visit(nestedTypeReference.ContainingType);
      this.path.Pop();
      return nestedTypeReference;
    }

    /// <summary>
    /// Visits the specified specialized field reference.
    /// </summary>
    /// <param name="specializedFieldReference">The specialized field reference.</param>
    /// <returns></returns>
    public virtual SpecializedFieldReference Visit(SpecializedFieldReference specializedFieldReference) {
      if (this.stopTraversal) return specializedFieldReference;
      this.Visit((FieldReference)specializedFieldReference);
      this.path.Push(specializedFieldReference);
      specializedFieldReference.UnspecializedVersion = this.Visit(specializedFieldReference.UnspecializedVersion);
      this.path.Pop();
      return specializedFieldReference;
    }

    /// <summary>
    /// Visits the specified specialized method reference.
    /// </summary>
    /// <param name="specializedMethodReference">The specialized method reference.</param>
    /// <returns></returns>
    public virtual SpecializedMethodReference Visit(SpecializedMethodReference specializedMethodReference) {
      if (this.stopTraversal) return specializedMethodReference;
      this.Visit((MethodReference)specializedMethodReference);
      this.path.Push(specializedMethodReference);
      specializedMethodReference.UnspecializedVersion = this.Visit(specializedMethodReference.UnspecializedVersion);
      this.path.Pop();
      return specializedMethodReference;
    }

    /// <summary>
    /// Visits the specified specialized nested type reference.
    /// </summary>
    /// <param name="specializedNestedTypeReference">The specialized nested type reference.</param>
    /// <returns></returns>
    public virtual SpecializedNestedTypeReference Visit(SpecializedNestedTypeReference specializedNestedTypeReference) {
      if (this.stopTraversal) return specializedNestedTypeReference;
      this.Visit((NestedTypeReference)specializedNestedTypeReference);
      this.path.Push(specializedNestedTypeReference);
      specializedNestedTypeReference.UnspecializedVersion = (INestedTypeReference)this.Visit(specializedNestedTypeReference.UnspecializedVersion);
      this.path.Pop();
      return specializedNestedTypeReference;
    }

    /// <summary>
    /// Replaces the child nodes of the given mutable type definition with the results of running the mutator over them. 
    /// Note that when overriding this method, care must be taken to add the given mutable type definition to this.flatListOfTypes.
    /// </summary>
    /// <param name="typeDefinition">A mutable type definition.</param>
    protected virtual void Visit(NamedTypeDefinition typeDefinition) {
      if (this.stopTraversal) return;
      this.flatListOfTypes.Add(typeDefinition);
      this.path.Push(typeDefinition);
      typeDefinition.Attributes = this.Visit(typeDefinition.Attributes);
      typeDefinition.BaseClasses = this.Visit(typeDefinition.BaseClasses);
      typeDefinition.ExplicitImplementationOverrides = this.Visit(typeDefinition.ExplicitImplementationOverrides);
      typeDefinition.GenericParameters = this.Visit(typeDefinition.GenericParameters);
      typeDefinition.Interfaces = this.Visit(typeDefinition.Interfaces);
      typeDefinition.Locations = this.Visit(typeDefinition.Locations);
      typeDefinition.Events = this.Visit(typeDefinition.Events);
      typeDefinition.Fields = this.Visit(typeDefinition.Fields);
      typeDefinition.Methods = this.Visit(typeDefinition.Methods);
      typeDefinition.NestedTypes = this.Visit(typeDefinition.NestedTypes);
      typeDefinition.Properties = this.Visit(typeDefinition.Properties);
      if (typeDefinition.HasDeclarativeSecurity)
        typeDefinition.SecurityAttributes = this.Visit(typeDefinition.SecurityAttributes);
      if (typeDefinition.IsEnum)
        typeDefinition.UnderlyingType = this.Visit(typeDefinition.UnderlyingType);
      this.path.Pop();
    }

    /// <summary>
    /// Visits the private helper members.
    /// </summary>
    /// <param name="typeDefinitions">The type definitions.</param>
    public virtual void VisitPrivateHelperMembers(List<INamedTypeDefinition>/*?*/ typeDefinitions) {
      if (this.stopTraversal || typeDefinitions == null) return;
      for (int i = 0, n = typeDefinitions.Count; i < n; i++) {
        NamedTypeDefinition/*?*/ typeDef = typeDefinitions[i] as NamedTypeDefinition;
        if (typeDef == null) continue;
        this.path.Push(typeDef);
        typeDef.PrivateHelperMembers = this.Visit(typeDef.PrivateHelperMembers);
        this.path.Pop();
      }
    }

    /// <summary>
    /// Visits the specified type definition members.
    /// </summary>
    /// <param name="typeDefinitionMembers">The type definition members.</param>
    /// <returns></returns>
    public virtual List<ITypeDefinitionMember>/*?*/ Visit(List<ITypeDefinitionMember>/*?*/ typeDefinitionMembers) {
      if (this.stopTraversal || typeDefinitionMembers == null) return typeDefinitionMembers;
      for (int i = 0, n = typeDefinitionMembers.Count; i < n; i++)
        typeDefinitionMembers[i] = this.Visit(typeDefinitionMembers[i]);
      return typeDefinitionMembers;
    }

    /// <summary>
    /// Visits the specified type definition member.
    /// </summary>
    /// <param name="typeDefinitionMember">The type definition member.</param>
    /// <returns></returns>
    public virtual ITypeDefinitionMember Visit(ITypeDefinitionMember typeDefinitionMember) {
      IEventDefinition/*?*/ eventDef = typeDefinitionMember as IEventDefinition;
      if (eventDef != null) return this.Visit(eventDef);
      IFieldDefinition/*?*/ fieldDef = typeDefinitionMember as IFieldDefinition;
      if (fieldDef != null) return this.Visit(fieldDef);
      IMethodDefinition/*?*/ methodDef = typeDefinitionMember as IMethodDefinition;
      if (methodDef != null) return this.Visit(methodDef);
      INestedTypeDefinition/*?*/ nestedTypeDef = typeDefinitionMember as INestedTypeDefinition;
      if (nestedTypeDef != null) return this.Visit(nestedTypeDef);
      IPropertyDefinition/*?*/ propertyDef = typeDefinitionMember as IPropertyDefinition;
      if (propertyDef != null) return this.Visit(propertyDef);
      Debug.Assert(false);
      return typeDefinitionMember;
    }

    /// <summary>
    /// Visits the specified type references.
    /// </summary>
    /// <param name="typeReferences">The type references.</param>
    /// <returns></returns>
    public virtual List<ITypeReference>/*?*/ Visit(List<ITypeReference>/*?*/ typeReferences) {
      if (this.stopTraversal || typeReferences == null) return typeReferences;
      for (int i = 0, n = typeReferences.Count; i < n; i++)
        typeReferences[i] = this.Visit(typeReferences[i]);
      return typeReferences;
    }

    /// <summary>
    /// Visits the specified namespace type reference.
    /// </summary>
    /// <param name="namespaceTypeReference">The namespace type reference.</param>
    /// <returns></returns>
    public virtual INamespaceTypeReference Visit(INamespaceTypeReference namespaceTypeReference) {
      INamespaceTypeDefinition/*?*/ namespaceTypeDefinition = namespaceTypeReference as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null)
        return this.GetMutableCopy(namespaceTypeDefinition);
      return this.Visit(this.GetMutableCopy(namespaceTypeReference));
    }

    /// <summary>
    /// Visits the specified nested type reference.
    /// </summary>
    /// <param name="nestedTypeReference">The nested type reference.</param>
    /// <returns></returns>
    public virtual INestedTypeReference Visit(INestedTypeReference nestedTypeReference) {
      ISpecializedNestedTypeReference/*?*/ specializedNestedTypeReference = nestedTypeReference as ISpecializedNestedTypeReference;
      if (specializedNestedTypeReference != null)
        return this.Visit(this.GetMutableCopy(specializedNestedTypeReference));
      INestedTypeDefinition/*?*/ nestedTypeDefinition = nestedTypeReference as INestedTypeDefinition;
      if (nestedTypeDefinition != null)
        return this.GetMutableCopy(nestedTypeDefinition);
      return this.Visit(this.GetMutableCopy(nestedTypeReference));
    }

    /// <summary>
    /// Visits the specified generic method parameter reference.
    /// </summary>
    /// <param name="genericMethodParameterReference">The generic method parameter reference.</param>
    /// <returns></returns>
    public virtual IGenericMethodParameterReference Visit(IGenericMethodParameterReference genericMethodParameterReference) {
      IGenericMethodParameter/*?*/ genericMethodParameter = genericMethodParameterReference as IGenericMethodParameter;
      if (genericMethodParameter != null)
        return this.GetMutableCopy(genericMethodParameter);
      return this.Visit(this.GetMutableCopy(genericMethodParameterReference));
    }

    /// <summary>
    /// Visits the specified array type reference.
    /// </summary>
    /// <param name="arrayTypeReference">The array type reference.</param>
    /// <returns></returns>
    /// <remarks>Array types are not nominal types, so always visit the reference, even if it is a definition.</remarks>
    public virtual IArrayTypeReference Visit(IArrayTypeReference arrayTypeReference) {
      if (arrayTypeReference.IsVector)
        return this.Visit(this.GetMutableVectorCopy(arrayTypeReference));
      else
        return this.Visit(this.GetMutableMatrixCopy(arrayTypeReference));
    }

    /// <summary>
    /// Visits the specified generic type parameter reference.
    /// </summary>
    /// <param name="genericTypeParameterReference">The generic type parameter reference.</param>
    /// <returns></returns>
    public virtual IGenericTypeParameterReference Visit(IGenericTypeParameterReference genericTypeParameterReference) {
      IGenericTypeParameter/*?*/ genericTypeParameter = genericTypeParameterReference as IGenericTypeParameter;
      if (genericTypeParameter != null)
        return this.GetMutableCopy(genericTypeParameter);
      return this.Visit(this.GetMutableCopy(genericTypeParameterReference));
    }

    /// <summary>
    /// Visits the specified generic type instance reference.
    /// </summary>
    /// <param name="genericTypeInstanceReference">The generic type instance reference.</param>
    /// <returns></returns>
    public virtual IGenericTypeInstanceReference Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
      return this.Visit(this.GetMutableCopy(genericTypeInstanceReference));
    }

    /// <summary>
    /// Visits the specified parameter definition.
    /// </summary>
    /// <param name="parameterDefinition">The parameter definition to visit.</param>
    public virtual IParameterDefinition Visit(IParameterDefinition parameterDefinition) {
      return this.Visit(this.GetMutableCopy(parameterDefinition));
    }

    /// <summary>
    /// Visits a parameter definition that is being referenced.
    /// </summary>
    /// <param name="parameterDefinition">The referenced parameter definition.</param>
    public virtual IParameterDefinition VisitReferenceTo(IParameterDefinition parameterDefinition) {
      //The referrer must refer to the same copy of the parameter definition that was (or will be) produced by a visit to the actual definition.
      return this.GetMutableCopy(parameterDefinition);
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
    public virtual IPointerTypeReference Visit(IPointerTypeReference pointerTypeReference) {
      return this.Visit(this.GetMutableCopy(pointerTypeReference));
    }

    /// <summary>
    /// Visits the specified function pointer type reference.
    /// </summary>
    /// <param name="functionPointerTypeReference">The function pointer type reference.</param>
    /// <returns></returns>
    public virtual IFunctionPointerTypeReference Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
      return this.Visit(this.GetMutableCopy(functionPointerTypeReference));
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
    public virtual IManagedPointerTypeReference Visit(IManagedPointerTypeReference managedPointerTypeReference) {
      return this.Visit(this.GetMutableCopy(managedPointerTypeReference));
    }

    /// <summary>
    /// Visits the specified modified type reference.
    /// </summary>
    /// <param name="modifiedTypeReference">The modified type reference.</param>
    /// <returns></returns>
    public virtual IModifiedTypeReference Visit(IModifiedTypeReference modifiedTypeReference) {
      return this.Visit(this.GetMutableCopy(modifiedTypeReference));
    }

    /// <summary>
    /// Visits the specified module reference.
    /// </summary>
    /// <param name="moduleReference">The module reference.</param>
    /// <returns></returns>
    public virtual IModuleReference Visit(IModuleReference moduleReference) {
      return this.Visit(this.GetMutableCopy(moduleReference));
    }

    /// <summary>
    /// Visits the specified namespace type definition.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The namespace type definition.</param>
    /// <returns></returns>
    public virtual INamespaceTypeDefinition Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
      return this.Visit(this.GetMutableCopy(namespaceTypeDefinition));
    }

    /// <summary>
    /// Visits the specified nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition">The nested type definition.</param>
    /// <returns></returns>
    public virtual INestedTypeDefinition Visit(INestedTypeDefinition nestedTypeDefinition) {
      return this.Visit(this.GetMutableCopy(nestedTypeDefinition));
    }

    /// <summary>
    /// Visits the specified type reference.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns></returns>
    public virtual ITypeReference Visit(ITypeReference typeReference) {
      INamespaceTypeReference/*?*/ namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference != null)
        return this.Visit(namespaceTypeReference);
      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      if (nestedTypeReference != null)
        return this.Visit(nestedTypeReference);
      IGenericMethodParameterReference/*?*/ genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
      if (genericMethodParameterReference != null)
        return this.Visit(genericMethodParameterReference);
      IArrayTypeReference/*?*/ arrayTypeReference = typeReference as IArrayTypeReference;
      if (arrayTypeReference != null)
        return this.Visit(arrayTypeReference);
      IGenericTypeParameterReference/*?*/ genericTypeParameterReference = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterReference != null)
        return this.Visit(genericTypeParameterReference);
      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null)
        return this.Visit(genericTypeInstanceReference);
      IPointerTypeReference/*?*/ pointerTypeReference = typeReference as IPointerTypeReference;
      if (pointerTypeReference != null)
        return this.Visit(pointerTypeReference);
      IFunctionPointerTypeReference/*?*/ functionPointerTypeReference = typeReference as IFunctionPointerTypeReference;
      if (functionPointerTypeReference != null)
        return this.Visit(functionPointerTypeReference);
      IModifiedTypeReference/*?*/ modifiedTypeReference = typeReference as IModifiedTypeReference;
      if (modifiedTypeReference != null)
        return this.Visit(modifiedTypeReference);
      IManagedPointerTypeReference/*?*/ managedPointerTypeReference = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeReference != null)
        return this.Visit(managedPointerTypeReference);
      //TODO: error
      return typeReference;
    }

    /// <summary>
    /// Visits the specified unit namespace reference.
    /// </summary>
    /// <param name="unitNamespaceReference">The unit namespace reference.</param>
    /// <returns></returns>
    public virtual IUnitNamespaceReference Visit(IUnitNamespaceReference unitNamespaceReference) {
      IRootUnitNamespaceReference/*?*/ rootUnitNamespaceReference = unitNamespaceReference as IRootUnitNamespaceReference;
      if (rootUnitNamespaceReference != null)
        return this.Visit(rootUnitNamespaceReference);
      INestedUnitNamespaceReference/*?*/ nestedUnitNamespaceReference = unitNamespaceReference as INestedUnitNamespaceReference;
      if (nestedUnitNamespaceReference != null)
        return this.Visit(nestedUnitNamespaceReference);
      //TODO: error
      return unitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified nested unit namespace.
    /// </summary>
    /// <param name="nestedUnitNamespace">The nested unit namespace.</param>
    /// <returns></returns>
    public virtual INestedUnitNamespace Visit(INestedUnitNamespace nestedUnitNamespace) {
      return this.Visit(this.GetMutableCopy(nestedUnitNamespace));
    }

    /// <summary>
    /// Visits the specified nested unit namespace reference.
    /// </summary>
    /// <param name="nestedUnitNamespaceReference">The nested unit namespace reference.</param>
    /// <returns></returns>
    public virtual INestedUnitNamespaceReference Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      INestedUnitNamespace/*?*/ nestedUnitNamespace = nestedUnitNamespaceReference as INestedUnitNamespace;
      if (nestedUnitNamespace != null)
        return this.GetMutableCopy(nestedUnitNamespace);
      return this.Visit(this.GetMutableCopy(nestedUnitNamespaceReference));
    }

    /// <summary>
    /// Visits the specified nested unit namespace.
    /// </summary>
    /// <param name="nestedUnitNamespace">The nested unit namespace.</param>
    /// <returns></returns>
    public virtual NestedUnitNamespace Visit(NestedUnitNamespace nestedUnitNamespace) {
      if (this.stopTraversal) return nestedUnitNamespace;
      this.Visit((UnitNamespace)nestedUnitNamespace);
      nestedUnitNamespace.ContainingUnitNamespace = this.GetCurrentNamespace();
      return nestedUnitNamespace;
    }

    /// <summary>
    /// Visits the specified nested unit namespace reference.
    /// </summary>
    /// <param name="nestedUnitNamespaceReference">The nested unit namespace reference.</param>
    /// <returns></returns>
    public virtual NestedUnitNamespaceReference Visit(NestedUnitNamespaceReference nestedUnitNamespaceReference) {
      if (this.stopTraversal) return nestedUnitNamespaceReference;
      this.Visit((UnitNamespaceReference)nestedUnitNamespaceReference);
      nestedUnitNamespaceReference.ContainingUnitNamespace = this.Visit(nestedUnitNamespaceReference.ContainingUnitNamespace);
      return nestedUnitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified unit reference.
    /// </summary>
    /// <param name="unitReference">The unit reference.</param>
    /// <returns></returns>
    public virtual IUnitReference Visit(IUnitReference unitReference) {
      IAssemblyReference/*?*/ assemblyReference = unitReference as IAssemblyReference;
      if (assemblyReference != null)
        return this.Visit(assemblyReference);
      IModuleReference/*?*/ moduleReference = unitReference as IModuleReference;
      if (moduleReference != null)
        return this.Visit(moduleReference);
      //TODO: error
      return unitReference;
    }

    /// <summary>
    /// Visits the specified parameter definitions.
    /// </summary>
    /// <param name="parameterDefinitions">The parameter definitions.</param>
    /// <returns></returns>
    public virtual List<IParameterDefinition>/*?*/ Visit(List<IParameterDefinition>/*?*/ parameterDefinitions) {
      if (this.stopTraversal || parameterDefinitions == null) return parameterDefinitions;
      for (int i = 0, n = parameterDefinitions.Count; i < n; i++)
        parameterDefinitions[i] = this.Visit(parameterDefinitions[i]);
      return parameterDefinitions;
    }

    /// <summary>
    /// Visits the specified parameter definition.
    /// </summary>
    /// <param name="parameterDefinition">The parameter definition.</param>
    /// <returns></returns>
    public virtual ParameterDefinition Visit(ParameterDefinition parameterDefinition) {
      if (this.stopTraversal) return parameterDefinition;
      this.path.Push(parameterDefinition);
      parameterDefinition.Attributes = this.Visit(parameterDefinition.Attributes);
      parameterDefinition.ContainingSignature = this.GetCurrentSignature();
      if (parameterDefinition.HasDefaultValue)
        parameterDefinition.DefaultValue = this.Visit(this.GetMutableCopy(parameterDefinition.DefaultValue));
      if (parameterDefinition.IsModified)
        parameterDefinition.CustomModifiers = this.Visit(parameterDefinition.CustomModifiers);
      parameterDefinition.Locations = this.Visit(parameterDefinition.Locations);
      if (parameterDefinition.IsMarshalledExplicitly)
        parameterDefinition.MarshallingInformation = this.Visit(this.GetMutableCopy(parameterDefinition.MarshallingInformation));
      parameterDefinition.Type = this.Visit(parameterDefinition.Type);
      this.path.Pop();
      return parameterDefinition;
    }

    /// <summary>
    /// Visits the specified parameter type information list.
    /// </summary>
    /// <param name="parameterTypeInformationList">The parameter type information list.</param>
    /// <returns></returns>
    public virtual List<IParameterTypeInformation>/*?*/ Visit(List<IParameterTypeInformation>/*?*/ parameterTypeInformationList) {
      if (this.stopTraversal || parameterTypeInformationList == null) return parameterTypeInformationList;
      for (int i = 0, n = parameterTypeInformationList.Count; i < n; i++)
        parameterTypeInformationList[i] = this.Visit(this.GetMutableCopy(parameterTypeInformationList[i]));
      return parameterTypeInformationList;
    }

    /// <summary>
    /// Visits the specified parameter type information.
    /// </summary>
    /// <param name="parameterTypeInformation">The parameter type information.</param>
    /// <returns></returns>
    public virtual ParameterTypeInformation Visit(ParameterTypeInformation parameterTypeInformation) {
      if (this.stopTraversal) return parameterTypeInformation;
      this.path.Push(parameterTypeInformation);
      if (parameterTypeInformation.IsModified)
        parameterTypeInformation.CustomModifiers = this.Visit(parameterTypeInformation.CustomModifiers);
      parameterTypeInformation.Type = this.Visit(parameterTypeInformation.Type);
      this.path.Pop();
      return parameterTypeInformation;
    }

    /// <summary>
    /// Visits the specified platform invoke information.
    /// </summary>
    /// <param name="platformInvokeInformation">The platform invoke information.</param>
    /// <returns></returns>
    public virtual PlatformInvokeInformation Visit(PlatformInvokeInformation platformInvokeInformation) {
      if (this.stopTraversal) return platformInvokeInformation;
      this.path.Push(platformInvokeInformation);
      platformInvokeInformation.ImportModule = this.Visit(this.GetMutableCopy(platformInvokeInformation.ImportModule));
      this.path.Pop();
      return platformInvokeInformation;
    }

    /// <summary>
    /// Visits the specified property definitions.
    /// </summary>
    /// <param name="propertyDefinitions">The property definitions.</param>
    /// <returns></returns>
    public virtual List<IPropertyDefinition>/*?*/ Visit(List<IPropertyDefinition>/*?*/ propertyDefinitions) {
      if (this.stopTraversal || propertyDefinitions == null) return propertyDefinitions;
      for (int i = 0, n = propertyDefinitions.Count; i < n; i++)
        propertyDefinitions[i] = this.Visit(propertyDefinitions[i]);
      return propertyDefinitions;
    }

    /// <summary>
    /// Visits the specified property definition.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns></returns>
    public virtual IPropertyDefinition Visit(IPropertyDefinition propertyDefinition) {
      return this.Visit(this.GetMutableCopy(propertyDefinition));
    }

    /// <summary>
    /// Visits the specified property definition.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns></returns>
    public virtual PropertyDefinition Visit(PropertyDefinition propertyDefinition) {
      if (this.stopTraversal) return propertyDefinition;
      this.Visit((TypeDefinitionMember)propertyDefinition);
      this.path.Push(propertyDefinition);
      propertyDefinition.Accessors = this.Visit(propertyDefinition.Accessors);
      if (propertyDefinition.HasDefaultValue)
        propertyDefinition.DefaultValue = this.Visit(this.GetMutableCopy(propertyDefinition.DefaultValue));
      if (propertyDefinition.Getter != null)
        propertyDefinition.Getter = this.Visit(propertyDefinition.Getter);
      propertyDefinition.Parameters = this.Visit(propertyDefinition.Parameters);
      if (propertyDefinition.ReturnValueIsModified)
        propertyDefinition.ReturnValueCustomModifiers = this.Visit(propertyDefinition.ReturnValueCustomModifiers);
      if (propertyDefinition.Setter != null)
        propertyDefinition.Setter = this.Visit(propertyDefinition.Setter);
      propertyDefinition.Type = this.Visit(propertyDefinition.Type);
      this.path.Pop();
      return propertyDefinition;
    }

    /// <summary>
    /// Visits the specified pointer type reference.
    /// </summary>
    /// <param name="pointerTypeReference">The pointer type reference.</param>
    /// <returns></returns>
    public virtual PointerTypeReference Visit(PointerTypeReference pointerTypeReference) {
      if (this.stopTraversal) return pointerTypeReference;
      this.Visit((TypeReference)pointerTypeReference);
      this.path.Push(pointerTypeReference);
      pointerTypeReference.TargetType = this.Visit(pointerTypeReference.TargetType);
      this.path.Pop();
      return pointerTypeReference;
    }

    /// <summary>
    /// Visits the specified resource references.
    /// </summary>
    /// <param name="resourceReferences">The resource references.</param>
    /// <returns></returns>
    public virtual List<IResourceReference>/*?*/ Visit(List<IResourceReference>/*?*/ resourceReferences) {
      if (this.stopTraversal || resourceReferences == null) return resourceReferences;
      for (int i = 0, n = resourceReferences.Count; i < n; i++)
        resourceReferences[i] = this.Visit(this.GetMutableCopy(resourceReferences[i]));
      return resourceReferences;
    }

    /// <summary>
    /// Visits the specified resource reference.
    /// </summary>
    /// <param name="resourceReference">The resource reference.</param>
    /// <returns></returns>
    public virtual IResourceReference Visit(ResourceReference resourceReference) {
      if (this.stopTraversal) return resourceReference;
      resourceReference.Attributes = this.Visit(resourceReference.Attributes);
      resourceReference.DefiningAssembly = this.Visit(resourceReference.DefiningAssembly);
      return resourceReference;
    }

    /// <summary>
    /// Visits the specified security attributes.
    /// </summary>
    /// <param name="securityAttributes">The security attributes.</param>
    /// <returns></returns>
    public virtual List<ISecurityAttribute>/*?*/ Visit(List<ISecurityAttribute>/*?*/ securityAttributes) {
      if (this.stopTraversal || securityAttributes == null) return securityAttributes;
      for (int i = 0, n = securityAttributes.Count; i < n; i++)
        securityAttributes[i] = this.Visit(this.GetMutableCopy(securityAttributes[i]));
      return securityAttributes;
    }

    /// <summary>
    /// Visits the specified root unit namespace reference.
    /// </summary>
    /// <param name="rootUnitNamespaceReference">The root unit namespace reference.</param>
    /// <returns></returns>
    public virtual IRootUnitNamespaceReference Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      IRootUnitNamespace/*?*/ rootUnitNamespace = rootUnitNamespaceReference as IRootUnitNamespace;
      if (rootUnitNamespace != null)
        return this.GetMutableCopy(rootUnitNamespace);
      return this.Visit(this.GetMutableCopy(rootUnitNamespaceReference));
    }

    /// <summary>
    /// Visits the specified root unit namespace.
    /// </summary>
    /// <param name="rootUnitNamespace">The root unit namespace.</param>
    /// <returns></returns>
    public virtual RootUnitNamespace Visit(RootUnitNamespace rootUnitNamespace) {
      if (this.stopTraversal) return rootUnitNamespace;
      rootUnitNamespace.Unit = this.GetCurrentUnit();
      this.Visit((UnitNamespace)rootUnitNamespace);
      return rootUnitNamespace;
    }

    /// <summary>
    /// Visits the specified root unit namespace reference.
    /// </summary>
    /// <param name="rootUnitNamespaceReference">The root unit namespace reference.</param>
    /// <returns></returns>
    public virtual RootUnitNamespaceReference Visit(RootUnitNamespaceReference rootUnitNamespaceReference) {
      if (this.stopTraversal) return rootUnitNamespaceReference;
      rootUnitNamespaceReference.Unit = this.Visit(rootUnitNamespaceReference.Unit);
      return rootUnitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified security attribute.
    /// </summary>
    /// <param name="securityAttribute">The security attribute.</param>
    /// <returns></returns>
    public virtual SecurityAttribute Visit(SecurityAttribute securityAttribute) {
      if (this.stopTraversal) return securityAttribute;
      this.path.Push(securityAttribute);
      securityAttribute.Attributes = this.Visit(securityAttribute.Attributes);
      this.path.Pop();
      return securityAttribute;
    }

    /// <summary>
    /// Visits the specified section block.
    /// </summary>
    /// <param name="sectionBlock">The section block.</param>
    /// <returns></returns>
    public virtual SectionBlock Visit(SectionBlock sectionBlock) {
      return sectionBlock;
    }

    /// <summary>
    /// Visits the specified type definition member.
    /// </summary>
    /// <param name="typeDefinitionMember">The type definition member.</param>
    /// <returns></returns>
    public virtual ITypeDefinitionMember Visit(TypeDefinitionMember typeDefinitionMember) {
      if (this.stopTraversal) return typeDefinitionMember;
      this.path.Push(typeDefinitionMember);
      typeDefinitionMember.Attributes = this.Visit(typeDefinitionMember.Attributes);
      typeDefinitionMember.ContainingTypeDefinition = this.GetCurrentType();
      typeDefinitionMember.Locations = this.Visit(typeDefinitionMember.Locations);
      this.path.Pop();
      return typeDefinitionMember;
    }

    /// <summary>
    /// Visits the specified type reference.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns></returns>
    public virtual TypeReference Visit(TypeReference typeReference) {
      if (this.stopTraversal) return typeReference;
      this.path.Push(typeReference);
      typeReference.Attributes = this.Visit(typeReference.Attributes);
      typeReference.Locations = this.Visit(typeReference.Locations);
      this.path.Pop();
      return typeReference;
    }

    /// <summary>
    /// Visits the specified PE sections.
    /// </summary>
    /// <param name="peSections">The PE section.</param>
    public virtual List<IPESection>/*?*/ Visit(List<IPESection>/*?*/ peSections) {
      if (this.stopTraversal || peSections == null) return peSections;
      for (int i = 0, n = peSections.Count; i < n; i++)
        peSections[i] = this.Visit(this.GetMutableCopy(peSections[i]));
      return peSections;
    }

    /// <summary>
    /// Visits the specifed PE section.
    /// </summary>
    /// <param name="peSection">The PE section</param>
    /// <returns></returns>
    public virtual PESection Visit(PESection peSection) {
      return peSection;
    }

    /// <summary>
    /// Visits the specified unit.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <returns></returns>
    public virtual Unit Visit(Unit unit) {
      if (this.stopTraversal) return unit;
      this.path.Push(unit);
      unit.Attributes = this.Visit(unit.Attributes);
      unit.Locations = this.Visit(unit.Locations);
      unit.UninterpretedSections = this.Visit(unit.UninterpretedSections);
      unit.UnitNamespaceRoot = this.Visit(this.GetMutableCopy((IRootUnitNamespace)unit.UnitNamespaceRoot));
      this.path.Pop();
      return unit;
    }

    /// <summary>
    /// Visits the specified unit namespace.
    /// </summary>
    /// <param name="unitNamespace">The unit namespace.</param>
    /// <returns></returns>
    public virtual UnitNamespace Visit(UnitNamespace unitNamespace) {
      if (this.stopTraversal) return unitNamespace;
      this.path.Push(unitNamespace);
      unitNamespace.Attributes = this.Visit(unitNamespace.Attributes);
      unitNamespace.Locations = this.Visit(unitNamespace.Locations);
      unitNamespace.Members = this.Visit(unitNamespace.Members);
      this.path.Pop();
      return unitNamespace;
    }

    /// <summary>
    /// Visits the specified unit namespace reference.
    /// </summary>
    /// <param name="unitNamespaceReference">The unit namespace reference.</param>
    /// <returns></returns>
    public virtual UnitNamespaceReference Visit(UnitNamespaceReference unitNamespaceReference) {
      if (this.stopTraversal) return unitNamespaceReference;
      this.path.Push(unitNamespaceReference);
      unitNamespaceReference.Attributes = this.Visit(unitNamespaceReference.Attributes);
      unitNamespaceReference.Locations = this.Visit(unitNamespaceReference.Locations);
      this.path.Pop();
      return unitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified vector type reference.
    /// </summary>
    /// <param name="vectorTypeReference">The vector type reference.</param>
    /// <returns></returns>
    public virtual VectorTypeReference Visit(VectorTypeReference vectorTypeReference) {
      if (this.stopTraversal) return vectorTypeReference;
      this.Visit((TypeReference)vectorTypeReference);
      this.path.Push(vectorTypeReference);
      vectorTypeReference.ElementType = this.Visit(vectorTypeReference.ElementType);
      this.path.Pop();
      return vectorTypeReference;
    }

    /// <summary>
    /// Visits the specified win32 resources.
    /// </summary>
    /// <param name="win32Resources">The win32 resources.</param>
    /// <returns></returns>
    public virtual List<IWin32Resource>/*?*/ Visit(List<IWin32Resource>/*?*/ win32Resources) {
      if (this.stopTraversal || win32Resources == null) return win32Resources;
      for (int i = 0, n = win32Resources.Count; i < n; i++)
        win32Resources[i] = this.Visit(this.GetMutableCopy(win32Resources[i]));
      return win32Resources;
    }

    /// <summary>
    /// Visits the specified win32 resource.
    /// </summary>
    /// <param name="win32Resource">The win32 resource.</param>
    /// <returns></returns>
    public virtual Win32Resource Visit(Win32Resource win32Resource) {
      return win32Resource;
    }

    /// <summary>
    /// Visits the method return value attributes.
    /// </summary>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns></returns>
    public virtual List<ICustomAttribute> VisitMethodReturnValueAttributes(List<ICustomAttribute> customAttributes) {
      return this.Visit(customAttributes);
    }

    /// <summary>
    /// Visits the method return value custom modifiers.
    /// </summary>
    /// <param name="customModifers">The custom modifers.</param>
    /// <returns></returns>
    public virtual List<ICustomModifier> VisitMethodReturnValueCustomModifiers(List<ICustomModifier> customModifers) {
      return this.Visit(customModifers);
    }

    /// <summary>
    /// Visits the method return value marshalling information.
    /// </summary>
    /// <param name="marshallingInformation">The marshalling information.</param>
    /// <returns></returns>
    public virtual IMarshallingInformation VisitMethodReturnValueMarshallingInformation(MarshallingInformation marshallingInformation) {
      return this.Visit(marshallingInformation);
    }

  }

  /// <summary>
  /// A visitor that visits a mutable metadata model. Derived classes can override the Mutate() methods
  /// to intervene in the visiting process, usually changes subnodes in place. Definitions that are used as refereces are 
  /// visited as references. Because references may be shared between parts that are visited and parts that are not, 
  /// this visitor may make a copy of a reference. The same references will be replaced by this copy
  /// whenever seen by the visitor.
  /// 
  /// </summary>
  /// <remarks>
  /// 1) When making copies, for type references, copies are made only when any of the subnodes changed. This optimization
  /// is not available for method, fields, namespace and other references. 
  /// 2) Subclasses, if want to change a node, override the Mutate method. For ITypeReference, IAssemblyReference and their
  /// subclasses, Mutate takes an interface value as argument. Otherwise, Mutate method mutates a mutable model object. 
  /// 3) References are identified by their object ID in this class. A better alternative could use interned ID.
  /// </remarks>
  /// 
  [Obsolete("Please use MetadataRewriter")]
  public class MutatingVisitor {

    /// <summary>
    /// A visitor that visits a mutable metadata model. Derived classes can override the visit methods
    /// to intervene in the visiting process, usually changes subnodes in place. By default this visitor
    /// will not visit non-mutable model nodes. This behavior can be changed by setting the visitImmutableNodes
    /// flag to be true.
    /// 
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    public MutatingVisitor(IMetadataHost host) {
      this.host = host;
    }

    /// <summary>
    /// A visitor that visits a mutable metadata model. Derived classes can override the visit methods
    /// to intervene in the visiting process, usually changes subnodes in place. By default this visitor
    /// will not visit non-mutable model nodes. This behavior can be changed by setting the visitImmutableNodes
    /// flag to be true;
    /// 
    /// </summary>
    /// <param name="host">An object representing the application that is hosting this mutator. It is used to obtain access to some global
    /// objects and services such as the shared name table and the table for interning references.</param>
    /// <param name="visitImmutableNodes">
    /// When we see an immutable node, whether we descend to visit its children. 
    /// </param>
    public MutatingVisitor(IMetadataHost host, bool visitImmutableNodes)
      : this(host) {
      this.visitImmutableNodes = visitImmutableNodes;
    }

    /// <summary>
    /// True if the mutator should try and perform mutations in place, rather than mutating new copies.
    /// </summary>
    protected readonly bool visitImmutableNodes;

    /// <summary>
    /// The metadata host.
    /// </summary>
    protected IMetadataHost host;

    /// <summary>
    /// The path that starts from the root of the cone, containing every container node that enclosing the node
    /// that is currently visited. 
    /// </summary>
    protected System.Collections.Stack path = new System.Collections.Stack();

    /// <summary>
    /// Set this flag to stop further traversing. 
    /// </summary>
    protected bool stopTraversal;

    /// <summary>
    /// A cache for references, which may be copied. There is one copy for an original reference ref; the cache always contains
    /// (ref, copy) and (copy, copy) for every reference that has been copied. 
    /// </summary>
    protected Dictionary<IReference, IReference> referenceCache = new Dictionary<IReference, IReference>();

    /// <summary>
    /// If we haven't finished visiting a method definition, returns that method. Otherwise, returns dummy. 
    /// </summary>
    /// <returns></returns>
    public IMethodDefinition GetCurrentMethod() {
      foreach (object parent in this.path) {
        IMethodDefinition/*?*/ method = parent as IMethodDefinition;
        if (method != null) return method;
      }
      return Dummy.MethodDefinition;
    }

    /// <summary>
    /// If we havent finished visiting a namespace, returns the most recent such a namespace. Otherwise, returns dummy.
    /// </summary>
    /// <returns></returns>
    public IUnitNamespace GetCurrentNamespace() {
      foreach (object parent in this.path) {
        IUnitNamespace/*?*/ ns = parent as IUnitNamespace;
        if (ns != null) return ns;
      }
      return Dummy.UnitNamespace;
    }

    /// <summary>
    /// If we havent finished visiting a signature, returns that signature. Otherwise, returns dummy. 
    /// </summary>
    /// <returns></returns>
    public ISignature GetCurrentSignature() {
      foreach (object parent in this.path) {
        ISignature/*?*/ signature = parent as ISignature;
        if (signature != null) return signature;
      }
      return Dummy.Signature;
    }

    /// <summary>
    /// If we havent finished visiting a type, returns the most recent such a type. Otherwise, returns dummy. 
    /// </summary>
    /// <returns></returns>
    public ITypeDefinition GetCurrentType() {
      foreach (object parent in this.path) {
        ITypeDefinition/*?*/ type = parent as ITypeDefinition;
        if (type != null) return type;
      }
      return Dummy.TypeDefinition;
    }

    /// <summary>
    /// If we havent finished visiting a unit, returns that unit. Otherwise, returns dummy. 
    /// </summary>
    /// <returns></returns>
    public IUnit GetCurrentUnit() {
      foreach (object parent in this.path) {
        IUnit/*?*/ unit = parent as IUnit;
        if (unit != null) return unit;
      }
      return Dummy.Unit;
    }
    // For visiting subnodes of an immutable node 
    #region Visitor for IEnumerables
    /// <summary>
    /// Visits an immutable collection of attributes
    /// </summary>
    private void Visit(IEnumerable<ICustomAttribute> attributes) {
      if (this.visitImmutableNodes) {
        foreach (var attr in attributes) {
          this.Visit(attr);
        }
      }
    }
    /// <summary>
    /// Visits an immutable collection of locations.
    /// </summary>
    /// <param name="locations"></param>
    private void Visit(IEnumerable<ILocation> locations) {
      if (this.visitImmutableNodes) {
        foreach (var loc in locations) {
          this.Visit(loc);
        }
      }
    }

    /// <summary>
    /// Visits an immutable collection of metadata expressions. 
    /// </summary>
    /// <param name="metadataExpressions"></param>
    private void Visit(IEnumerable<IMetadataExpression> metadataExpressions) {
      foreach (var me in metadataExpressions) {
        this.Dispatch(me);
      }
    }

    /// <summary>
    /// Visits an immutable collection of (metadata) named arguments. 
    /// </summary>
    /// <param name="namedArguments"></param>
    private void Visit(IEnumerable<IMetadataNamedArgument> namedArguments) {
      foreach (var namedArg in namedArguments) {
        this.Dispatch(namedArg);
      }
    }

    /// <summary>
    /// Visits an immutable collection of aliasForTypes. 
    /// </summary>
    /// <param name="aliasForTypes"></param>
    private void Visit(IEnumerable<IAliasForType> aliasForTypes) {
      foreach (var aliasForType in aliasForTypes) {
        this.Visit(aliasForType);
      }
    }

    /// <summary>
    /// Visits an immutable collection of custom modifiers.
    /// </summary>
    /// <param name="modifiers"></param>
    private void Visit(IEnumerable<ICustomModifier> modifiers) {
      foreach (var modifier in modifiers) this.Visit(modifier);
    }

    /// <summary>
    /// Visits an immutable collection of events.
    /// </summary>
    /// <param name="events"></param>
    private void Visit(IEnumerable<IEventDefinition> events) {
      foreach (var e in events) this.Visit(e);
    }

    /// <summary>
    /// Visits an immutable collection of fields. 
    /// </summary>
    /// <param name="fields"></param>
    private void Visit(IEnumerable<IFieldDefinition> fields) {
      foreach (var f in fields) this.Visit(f);
    }

    /// <summary>
    /// Visits an immutable collection of generic type parameters. 
    /// </summary>
    /// <param name="genericTypeParameters"></param>
    private void Visit(IEnumerable<IGenericTypeParameter> genericTypeParameters) {
      foreach (var gtp in genericTypeParameters) this.Visit(gtp);
    }

    /// <summary>
    /// Visits an immutable collection of files. 
    /// </summary>
    /// <param name="files"></param>
    private void Visit(IEnumerable<IFileReference> files) {
      foreach (var file in files)
        this.Visit(file);
    }

    /// <summary>
    /// Visits an immutable collection of methods. 
    /// </summary>
    /// <param name="methods"></param>
    private void Visit(IEnumerable<IMethodDefinition> methods) {
      foreach (var m in methods) this.Visit(m);
    }

    /// <summary>
    /// Visits an immutable collection of method implementation relations. 
    /// </summary>
    /// <param name="methodImpls"></param>
    private void Visit(IEnumerable<IMethodImplementation> methodImpls) {
      foreach (var mi in methodImpls) this.Visit(mi);
    }

    /// <summary>
    /// Visits an immutable collection of method references. 
    /// </summary>
    /// <param name="methods"></param>
    private void Visit(IEnumerable<IMethodReference> methods) {
      foreach (var m in methods) this.Visit(m);
    }

    /// <summary>
    /// Visits an immutable collection of modules. 
    /// </summary>
    /// <param name="modules"></param>
    private void Visit(IEnumerable<IModule> modules) {
      foreach (var module in modules)
        this.Visit(module);
    }

    /// <summary>
    /// Visits an immutable collection of namespace members, i.e., types, global fields, global methods, etc. 
    /// </summary>
    /// <param name="members"></param>
    private void Visit(IEnumerable<INamespaceMember> members) {
      foreach (var member in members) {
        this.Dispatch(member);
      }
    }

    /// <summary>
    /// Visits an immutable collection of nested type definitions. 
    /// </summary>
    /// <param name="nestedTypes"></param>
    private void Visit(IEnumerable<INestedTypeDefinition> nestedTypes) {
      foreach (var nt in nestedTypes) this.Visit(nt);
    }

    /// <summary>
    /// Visits an immutable collection of parameter definitions. 
    /// </summary>
    /// <param name="parameters"></param>
    private void Visit(IEnumerable<IParameterDefinition> parameters) {
      foreach (var p in parameters) this.Visit(p);
    }

    /// <summary>
    /// Visits an immutable collection of parameter type information objects.
    /// </summary>
    /// <param name="parameterTypes"></param>
    private void Visit(IEnumerable<IParameterTypeInformation> parameterTypes) {
      foreach (var pt in parameterTypes) this.Visit(pt);
    }

    /// <summary>
    /// Visits an immutable collection of properties. 
    /// </summary>
    /// <param name="properties"></param>
    private void Visit(IEnumerable<IPropertyDefinition> properties) {
      foreach (var p in properties) this.Visit(p);
    }

    /// <summary>
    /// Visits an immutable collection of resource references. 
    /// </summary>
    /// <param name="resources"></param>
    private void Visit(IEnumerable<IResourceReference> resources) {
      foreach (var resource in resources)
        this.Visit(resource);
    }

    /// <summary>
    /// Visits an immutable collection of security attributes.
    /// </summary>
    /// <param name="securityAttributes"></param>
    private void Visit(IEnumerable<ISecurityAttribute> securityAttributes) {
      foreach (var sa in securityAttributes)
        this.Visit(sa);
    }

    /// <summary>
    /// Visits an immutable collection of type references.
    /// </summary>
    /// <param name="typeReferences"></param>
    private void Visit(IEnumerable<ITypeReference> typeReferences) {
      foreach (var typeRef in typeReferences) this.Visit(typeRef);
    }

    #endregion Visit IEnumerable of nodes

    #region Visit an INode object
    /// <summary>
    /// Visits an ICustomAttribute. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    public virtual ICustomAttribute Visit(ICustomAttribute customAttribute) {
      if (this.stopTraversal) return customAttribute;
      CustomAttribute mutable = customAttribute as CustomAttribute;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.Visit(customAttribute.Arguments);
        this.Visit(customAttribute.Constructor);
        this.Visit(customAttribute.NamedArguments);
      }
      return customAttribute;
    }

    /// <summary>
    /// Visits an ICustomModifier. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="customModifier"></param>
    /// <returns></returns>
    public virtual ICustomModifier Visit(ICustomModifier customModifier) {
      if (this.stopTraversal) return customModifier;
      var mutable = customModifier as CustomModifier;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(customModifier);
        this.Visit(customModifier.Modifier);
        this.path.Pop();
      }
      return customModifier;
    }

    /// <summary>
    /// Visits the specified aliases for types.
    /// </summary>
    /// <param name="aliasesForTypes">The aliases for types.</param>
    /// <returns></returns>
    public virtual List<IAliasForType>/*?*/ Mutate(List<IAliasForType>/*?*/ aliasesForTypes) {
      if (this.stopTraversal || aliasesForTypes == null) return aliasesForTypes;
      for (int i = 0, n = aliasesForTypes.Count; i < n; i++)
        aliasesForTypes[i] = this.Visit(aliasesForTypes[i]);
      return aliasesForTypes;
    }

    /// <summary>
    /// Visit an INamespaceAliasForType. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="namespaceAliasForType"></param>
    /// <returns></returns>
    public virtual INamespaceAliasForType Visit(INamespaceAliasForType namespaceAliasForType) {
      if (this.stopTraversal) return namespaceAliasForType;
      NamespaceAliasForType mutable = namespaceAliasForType as NamespaceAliasForType;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.Visit(namespaceAliasForType.AliasedType);
        this.Visit(namespaceAliasForType.Attributes);
        this.Visit(namespaceAliasForType.Locations);
      }
      return namespaceAliasForType;
    }

    /// <summary>
    /// Visit an INestedAliasForType. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="nestedAliasForType"></param>
    /// <returns></returns>
    public virtual INestedAliasForType Visit(INestedAliasForType nestedAliasForType) {
      if (this.stopTraversal) return nestedAliasForType;
      NestedAliasForType mutable = nestedAliasForType as NestedAliasForType;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.Visit(nestedAliasForType.AliasedType);
        this.Visit(nestedAliasForType.Attributes);
        this.Visit(nestedAliasForType.Locations);
      }
      return nestedAliasForType;
    }

    /// <summary>
    /// Visits the specified alias for type. This is a dispatcher. Concrete subclass object is visited.
    /// </summary>
    /// <param name="aliasForType">Type of the alias for.</param>
    /// <returns></returns>
    public virtual IAliasForType Visit(IAliasForType aliasForType) {
      // pure dispatcher method
      if (this.stopTraversal) return aliasForType;
      INamespaceAliasForType/*?*/ namespaceAliasForType = aliasForType as INamespaceAliasForType;
      if (namespaceAliasForType != null) return this.Visit(namespaceAliasForType);
      INestedAliasForType/*?*/ nestedAliasForType = aliasForType as INestedAliasForType;
      if (nestedAliasForType != null) return this.Visit(nestedAliasForType);
      Debug.Assert(false);
      return aliasForType;
    }

    /// <summary>
    /// Visits an IAssembly. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="assembly">The assembly to copy.</param>
    public virtual IAssembly Visit(IAssembly assembly) {
      if (this.stopTraversal) return assembly;
      Assembly mutable = assembly as Assembly;
      if (mutable != null) {
        return this.Mutate(mutable);
      }
      if (this.visitImmutableNodes) {
        this.Visit(assembly.AssemblyAttributes);
        this.Visit(assembly.ExportedTypes);
        this.Visit(assembly.Files);
        this.Visit(assembly.MemberModules);
        this.Visit(assembly.Resources);
        this.Visit(assembly.SecurityAttributes);
        this.Visit((IModule)assembly);
      }
      return assembly;
    }

    /// <summary>
    /// Visits a mutable assembly. 
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public virtual Assembly Mutate(Assembly assembly) {
      if (this.stopTraversal) return assembly;
      this.path.Push(assembly);
      assembly.AssemblyAttributes = this.Mutate(assembly.AssemblyAttributes);
      assembly.ExportedTypes = this.Mutate(assembly.ExportedTypes);
      assembly.Files = this.Mutate(assembly.Files);
      assembly.MemberModules = this.Mutate(assembly.MemberModules);
      assembly.Resources = this.Mutate(assembly.Resources);
      assembly.SecurityAttributes = this.Mutate(assembly.SecurityAttributes);
      this.path.Pop();
      this.Mutate((Module)assembly);
      return assembly;
    }

    /// <summary>
    /// Visits the specified assembly references.
    /// </summary>
    /// <param name="assemblyReferences">The assembly references.</param>
    /// <returns></returns>
    public virtual List<IAssemblyReference>/*?*/ Mutate(List<IAssemblyReference>/*?*/ assemblyReferences) {
      if (this.stopTraversal || assemblyReferences == null) return assemblyReferences;
      for (int i = 0, n = assemblyReferences.Count; i < n; i++)
        assemblyReferences[i] = this.Visit(assemblyReferences[i]);
      return assemblyReferences;
    }

    /// <summary>
    /// Visits an IAssemblyReference. We first check in the cache and return the cached value if there is one. Then we 
    /// call the Mutate method where a copy may be produced if anything changes. 
    /// </summary>
    /// <param name="assemblyReference">The assembly reference.</param>
    /// <returns></returns>
    public virtual IAssemblyReference Visit(IAssemblyReference assemblyReference) {
      if (this.stopTraversal) return assemblyReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(assemblyReference, out cachedValue)) {
        return (IAssemblyReference)cachedValue;
      }
      IAssemblyReference result = this.Mutate(assemblyReference);
      return result;
    }

    /// <summary>
    /// Visits the specified assembly reference. May make a copy if this.copyReferences is true and this reference has 
    /// not been copied. 
    /// 
    /// Override this method instead of the corresponding Visit method if a subclass expects to change the node. 
    /// </summary>
    /// <param name="assemblyReference">The assembly reference.</param>
    /// <returns>Simply returns the assembly reference unchanged.</returns>
    public virtual IAssemblyReference Mutate(IAssemblyReference assemblyReference) {
      return assemblyReference;
    }

    /// <summary>
    /// Visits the specified custom attributes.
    /// </summary>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns></returns>
    public virtual List<ICustomAttribute>/*?*/ Mutate(List<ICustomAttribute>/*?*/ customAttributes) {
      if (this.stopTraversal || customAttributes == null) return customAttributes;
      for (int i = 0, n = customAttributes.Count; i < n; i++)
        customAttributes[i] = this.Visit(customAttributes[i]);
      return customAttributes;
    }

    /// <summary>
    /// Visits the mutable custom attribute.
    /// </summary>
    /// <param name="customAttribute">The custom customAttribute.</param>
    /// <returns></returns>
    public virtual CustomAttribute Mutate(CustomAttribute customAttribute) {
      if (this.stopTraversal) return customAttribute;
      this.path.Push(customAttribute);
      customAttribute.Arguments = this.Mutate(customAttribute.Arguments);
      customAttribute.Constructor = this.Visit(customAttribute.Constructor);
      customAttribute.NamedArguments = this.Mutate(customAttribute.NamedArguments);
      this.path.Pop();
      return customAttribute;
    }

    /// <summary>
    /// Visits the specified custom modifiers.
    /// </summary>
    /// <param name="customModifiers">The custom modifiers.</param>
    /// <returns></returns>
    public virtual List<ICustomModifier>/*?*/ Mutate(List<ICustomModifier>/*?*/ customModifiers) {
      if (this.stopTraversal || customModifiers == null) return customModifiers;
      for (int i = 0, n = customModifiers.Count; i < n; i++)
        customModifiers[i] = this.Visit(customModifiers[i]);
      return customModifiers;
    }

    /// <summary>
    /// Visits a mutable custom modifier.
    /// </summary>
    /// <param name="customModifier">The custom modifier.</param>
    /// <returns></returns>
    public virtual CustomModifier Mutate(CustomModifier customModifier) {
      if (this.stopTraversal) return customModifier;
      this.path.Push(customModifier);
      customModifier.Modifier = this.Visit(customModifier.Modifier);
      this.path.Pop();
      return customModifier;
    }

    /// <summary>
    /// Visits the specified event definitions.
    /// </summary>
    /// <param name="eventDefinitions">The event definitions.</param>
    /// <returns></returns>
    public virtual List<IEventDefinition>/*?*/ Mutate(List<IEventDefinition>/*?*/ eventDefinitions) {
      if (this.stopTraversal || eventDefinitions == null) return eventDefinitions;
      for (int i = 0, n = eventDefinitions.Count; i < n; i++)
        eventDefinitions[i] = this.Visit(eventDefinitions[i]);
      return eventDefinitions;
    }

    /// <summary>
    /// Visits an IEventDefinition. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <returns></returns>
    public virtual IEventDefinition Visit(IEventDefinition eventDefinition) {
      if (this.stopTraversal) return eventDefinition;
      var mutable = eventDefinition as EventDefinition;
      if (mutable != null) {
        this.Mutate(mutable);
      }
      if (this.visitImmutableNodes) {
        this.VisitITypeDefinitionMember(eventDefinition);
        this.Visit(eventDefinition.Accessors);
        this.Visit(eventDefinition.Adder);
        this.Visit(eventDefinition.Remover);
        this.Visit(eventDefinition.Type);
        if (eventDefinition.Caller != null)
          this.Visit(eventDefinition.Caller);
      }
      return eventDefinition;
    }

    /// <summary>
    /// Visits a mutable event definition.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <returns></returns>
    public virtual EventDefinition Mutate(EventDefinition eventDefinition) {
      if (this.stopTraversal) return eventDefinition;
      this.VisitTypeDefinitionMember(eventDefinition);
      this.path.Push(eventDefinition);
      eventDefinition.Accessors = this.Mutate(eventDefinition.Accessors);
      eventDefinition.Adder = this.Visit(eventDefinition.Adder);
      if (eventDefinition.Caller != null)
        eventDefinition.Caller = this.Visit(eventDefinition.Caller);
      eventDefinition.Remover = this.Visit(eventDefinition.Remover);
      eventDefinition.Type = this.Visit(eventDefinition.Type);
      this.path.Pop();
      return eventDefinition;
    }

    /// <summary>
    /// Visits the specified field definitions.
    /// </summary>
    /// <param name="fieldDefinitions">The field definitions.</param>
    /// <returns></returns>
    public virtual List<IFieldDefinition>/*?*/ Mutate(List<IFieldDefinition>/*?*/ fieldDefinitions) {
      if (this.stopTraversal || fieldDefinitions == null) return fieldDefinitions;
      for (int i = 0, n = fieldDefinitions.Count; i < n; i++)
        fieldDefinitions[i] = this.Visit(fieldDefinitions[i]);
      return fieldDefinitions;
    }

    /// <summary>
    /// Visits a mutable field definition.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <returns></returns>
    public virtual FieldDefinition Mutate(FieldDefinition fieldDefinition) {
      if (this.stopTraversal) return fieldDefinition;
      this.VisitTypeDefinitionMember(fieldDefinition);
      this.path.Push(fieldDefinition);
      if (fieldDefinition.IsCompileTimeConstant)
        fieldDefinition.CompileTimeValue = (IMetadataConstant)this.Visit(fieldDefinition.CompileTimeValue);
      if (fieldDefinition.IsMapped)
        fieldDefinition.FieldMapping = this.Visit(fieldDefinition.FieldMapping);
      if (fieldDefinition.IsMarshalledExplicitly)
        fieldDefinition.MarshallingInformation = this.Visit(fieldDefinition.MarshallingInformation);
      fieldDefinition.Type = this.Visit(fieldDefinition.Type);
      this.path.Pop();
      return fieldDefinition;
    }

    /// <summary>
    /// Visits (interface)IFieldDefinition. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <returns></returns>
    public virtual IFieldDefinition Visit(IFieldDefinition fieldDefinition) {
      if (this.stopTraversal) return fieldDefinition;
      var mutable = fieldDefinition as FieldDefinition;
      if (mutable != null) {
        this.Mutate(mutable);
      }
      if (this.visitImmutableNodes) {
        this.path.Push(fieldDefinition);
        this.VisitITypeDefinitionMember(fieldDefinition);
        if (fieldDefinition.IsCompileTimeConstant)
          this.Visit(fieldDefinition.CompileTimeValue);
        if (fieldDefinition.IsMapped)
          this.Visit(fieldDefinition.FieldMapping);
        if (fieldDefinition.IsMarshalledExplicitly)
          this.Visit(fieldDefinition.MarshallingInformation);
        this.Visit(fieldDefinition.Type);
        this.path.Pop();
      }
      return fieldDefinition;
    }

    /// <summary>
    /// Visits an (interface) field reference. We first see the reference has been cached. If so we return the 
    /// cached value. Then we mutate a copy of the input as a reference, if the input is a definition or already
    /// a mutable reference. If the copy is an immutable, non short-cut reference, we may choose to visit its subnodes
    /// according to the flag this.visitImmutableNodes. 
    /// 
    /// Note that mutable nodes that implement the IFieldReference include those implement ISpecializedFieldReference (
    /// that is SpecializedFieldReference and SpecializedFieldDefinition), IGlobalFieldDefinition(GlobalFieldDefinition)
    /// and IFieldDefinition(excluding IGlobalFieldDefinitions, that is, only FieldDefinition). 
    /// </summary>
    /// <param name="fieldReference">The field reference.</param>
    /// <returns></returns>
    public virtual IFieldReference Visit(IFieldReference fieldReference) {
      if (this.stopTraversal) return fieldReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(fieldReference, out cachedValue)) {
        return (IFieldReference)cachedValue;
      }
      if (fieldReference is Dummy) return Dummy.FieldReference;
      ISpecializedFieldReference/*?*/ specializedFieldReference = fieldReference as ISpecializedFieldReference;
      if (specializedFieldReference != null)
        return this.Visit(specializedFieldReference);
      IGlobalFieldDefinition/*?*/ globalFieldDefinition = fieldReference as IGlobalFieldDefinition;
      if (globalFieldDefinition != null)
        return this.Mutate(this.GetReferenceCopy(globalFieldDefinition));
      IFieldDefinition/*?*/ fieldDefinition = fieldReference as IFieldDefinition;
      if (fieldDefinition != null)
        return this.Mutate(this.GetReferenceCopy(fieldDefinition));
      var mutable = fieldReference as FieldReference;
      if (mutable != null) {
        return this.Mutate(this.GetReferenceCopy(mutable));
      }
      if (this.visitImmutableNodes) {
        this.path.Push(fieldReference);
        this.Visit(fieldReference.Attributes);
        this.Visit(fieldReference.ContainingType);
        this.Visit(fieldReference.Locations);
        this.Visit(fieldReference.Type);
        this.path.Pop();
      }
      return fieldReference;
    }

    /// <summary>
    /// Visits the specified field reference. 
    /// </summary>
    /// <param name="fieldReference">The field reference.</param>
    /// <returns></returns>
    public virtual FieldReference Mutate(FieldReference fieldReference) {
      if (this.stopTraversal) return fieldReference;
      this.path.Push(fieldReference);
      fieldReference.Attributes = this.Mutate(fieldReference.Attributes);
      fieldReference.ContainingType = this.Visit(fieldReference.ContainingType);
      fieldReference.Locations = this.Mutate(fieldReference.Locations);
      fieldReference.Type = this.Visit(fieldReference.Type);
      this.path.Pop();
      return fieldReference;
    }

    /// <summary>
    /// Visits the specified file references.
    /// </summary>
    /// <param name="fileReferences">The file references.</param>
    /// <returns></returns>
    public virtual List<IFileReference>/*?*/ Mutate(List<IFileReference>/*?*/ fileReferences) {
      if (this.stopTraversal || fileReferences == null) return fileReferences;
      for (int i = 0, n = fileReferences.Count; i < n; i++)
        fileReferences[i] = this.Visit(fileReferences[i]);
      return fileReferences;
    }

    /// <summary>
    /// Visits the specified file reference. This method simply returns the file reference. 
    /// </summary>
    /// <param name="fileReference">The file reference.</param>
    /// <returns></returns>
    public virtual IFileReference Visit(IFileReference fileReference) {
      return fileReference;
    }

    /// <summary>
    /// Visit a generic method instance reference. Note that IGenericMethodInstanceReference may be implemented by
    /// mutable node GenericMethodInstance or GenericMethodInstanceReference. 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <returns></returns>
    public virtual IGenericMethodInstanceReference Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
      if (this.stopTraversal) return genericMethodInstanceReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(genericMethodInstanceReference, out cachedValue)) {
        return (IGenericMethodInstanceReference)cachedValue;
      }
      var mutable1 = genericMethodInstanceReference as GenericMethodInstanceReference;
      if (mutable1 != null) return this.Mutate(this.GetReferenceCopy(mutable1));
      var mutable2 = genericMethodInstanceReference as Immutable.GenericMethodInstance;
      if (mutable2 != null) return this.Mutate(this.GetReferenceCopy(mutable2));
      if (this.visitImmutableNodes) {
        this.path.Push(genericMethodInstanceReference);
        this.Visit(genericMethodInstanceReference.Attributes);
        this.Visit(genericMethodInstanceReference.ContainingType);
        this.Visit(genericMethodInstanceReference.ExtraParameters);
        this.Visit(genericMethodInstanceReference.Locations);
        this.Visit(genericMethodInstanceReference.Parameters);
        if (genericMethodInstanceReference.ReturnValueIsModified)
          this.Visit(genericMethodInstanceReference.ReturnValueCustomModifiers);
        this.Visit(genericMethodInstanceReference.Type);
        this.Visit(genericMethodInstanceReference.GenericArguments);
        this.Visit(genericMethodInstanceReference.GenericMethod);
        this.path.Pop();
      }
      return genericMethodInstanceReference;
    }

    /// <summary>
    /// Visits the mutable generic method instance reference.
    /// </summary>
    /// <param name="genericMethodInstanceReference">The generic method instance reference.</param>
    /// <returns></returns>
    public virtual GenericMethodInstanceReference Mutate(GenericMethodInstanceReference genericMethodInstanceReference) {
      if (this.stopTraversal) return genericMethodInstanceReference;
      this.Mutate((MethodReference)genericMethodInstanceReference);
      this.path.Push(genericMethodInstanceReference);
      genericMethodInstanceReference.GenericArguments = this.Mutate(genericMethodInstanceReference.GenericArguments);
      genericMethodInstanceReference.GenericMethod = this.Visit(genericMethodInstanceReference.GenericMethod);
      this.path.Pop();
      return genericMethodInstanceReference;
    }

    /// <summary>
    /// Visits the specified generic method parameters.
    /// </summary>
    /// <param name="genericMethodParameters">The generic method parameters.</param>
    /// <returns></returns>
    public virtual List<IGenericMethodParameter>/*?*/ Mutate(List<IGenericMethodParameter>/*?*/ genericMethodParameters) {
      if (this.stopTraversal || genericMethodParameters == null) return genericMethodParameters;
      for (int i = 0, n = genericMethodParameters.Count; i < n; i++)
        genericMethodParameters[i] = this.Visit(genericMethodParameters[i]);
      return genericMethodParameters;
    }

    /// <summary>
    /// Visits the (interface) IGenericMethodParameter Node. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes).  
    /// </summary>
    /// <param name="genericMethodParameter"></param>
    /// <returns></returns>
    public virtual IGenericMethodParameter Visit(IGenericMethodParameter genericMethodParameter) {
      if (this.stopTraversal) return genericMethodParameter;
      var mutable = genericMethodParameter as GenericMethodParameter;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        if (this.stopTraversal) return genericMethodParameter;
        this.path.Push(genericMethodParameter);
        this.Visit(genericMethodParameter.Attributes);
        foreach (var constr in genericMethodParameter.Constraints)
          this.Visit(constr);
        this.path.Pop();
      }
      return genericMethodParameter;
    }

    /// <summary>
    /// Visits the mutable generic method parameter.
    /// </summary>
    /// <param name="genericMethodParameter">The generic method parameter.</param>
    /// <returns></returns>
    public virtual GenericMethodParameter Mutate(GenericMethodParameter genericMethodParameter) {
      if (this.stopTraversal) return genericMethodParameter;
      this.VisitGenericParameter(genericMethodParameter);
      var m = this.GetCurrentMethod();
      if (!(m is Dummy))
        genericMethodParameter.DefiningMethod = m;
      return genericMethodParameter;
    }

    /// <summary>
    /// Visits the mutable global field definition.
    /// </summary>
    /// <param name="globalFieldDefinition">The global field definition.</param>
    /// <returns></returns>
    public virtual GlobalFieldDefinition Mutate(GlobalFieldDefinition globalFieldDefinition) {
      if (this.stopTraversal) return globalFieldDefinition;
      this.path.Push(this.Visit(globalFieldDefinition.ContainingTypeDefinition));
      this.Mutate((FieldDefinition)globalFieldDefinition);
      this.path.Pop();
      var n = this.GetCurrentNamespace();
      if (!(n is Dummy)) {
        globalFieldDefinition.ContainingNamespace = n;
      }
      return globalFieldDefinition;
    }

    /// <summary>
    /// Visits the mutable global method definition.
    /// </summary>
    /// <param name="globalMethodDefinition">The global method definition.</param>
    /// <returns></returns>
    public virtual GlobalMethodDefinition Mutate(GlobalMethodDefinition globalMethodDefinition) {
      if (this.stopTraversal) return globalMethodDefinition;
      this.path.Push(this.Visit(globalMethodDefinition.ContainingTypeDefinition));
      this.Mutate((MethodDefinition)globalMethodDefinition);
      this.path.Pop();
      var n = this.GetCurrentNamespace();
      if (!(n is Dummy)) {
        // the enclosing namespace is visited before. 
        globalMethodDefinition.ContainingNamespace = n;
      }
      return globalMethodDefinition;
    }

    /// <summary>
    /// Visits the mutable generic parameter node. 
    /// </summary>
    /// <param name="genericParameter">The generic parameter.</param>
    /// <returns></returns>
    public virtual GenericParameter VisitGenericParameter(GenericParameter genericParameter) {
      if (this.stopTraversal) return genericParameter;
      this.path.Push(genericParameter);
      genericParameter.Attributes = this.Mutate(genericParameter.Attributes);
      genericParameter.Constraints = this.Mutate(genericParameter.Constraints);
      this.path.Pop();
      return genericParameter;
    }

    /// <summary>
    /// Visits the specified generic type parameters.
    /// </summary>
    /// <param name="genericTypeParameters">The generic type parameters.</param>
    /// <returns></returns>
    public virtual List<IGenericTypeParameter>/*?*/ Mutate(List<IGenericTypeParameter>/*?*/ genericTypeParameters) {
      if (this.stopTraversal || genericTypeParameters == null) return genericTypeParameters;
      for (int i = 0, n = genericTypeParameters.Count; i < n; i++)
        genericTypeParameters[i] = this.Visit(genericTypeParameters[i]);
      return genericTypeParameters;
    }

    /// <summary>
    /// Visits the interface IGenericTypeParameter. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="genericTypeParameter"></param>
    /// <returns></returns>
    public virtual IGenericTypeParameter Visit(IGenericTypeParameter genericTypeParameter) {
      if (this.stopTraversal) return genericTypeParameter;
      var mutable = genericTypeParameter as GenericTypeParameter;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(genericTypeParameter);
        this.Visit(genericTypeParameter.Attributes);
        foreach (var c in genericTypeParameter.Constraints)
          this.Visit(c);
        this.path.Pop();
      }
      return genericTypeParameter;
    }

    /// <summary>
    /// Visits the mutable generic type parameter.
    /// </summary>
    /// <param name="genericTypeParameter">The generic type parameter.</param>
    /// <returns></returns>
    public virtual GenericTypeParameter Mutate(GenericTypeParameter genericTypeParameter) {
      if (this.stopTraversal) return genericTypeParameter;
      this.VisitGenericParameter(genericTypeParameter);
      var t = this.GetCurrentType();
      if (!(t is Dummy)) {
        genericTypeParameter.DefiningType = t;
      }
      return genericTypeParameter;
    }

    /// <summary>
    /// Visits the specified locations.
    /// </summary>
    /// <param name="locations">The locations.</param>
    /// <returns></returns>
    public virtual List<ILocation>/*?*/ Mutate(List<ILocation>/*?*/ locations) {
      if (this.stopTraversal || locations == null) return locations;
      for (int i = 0, n = locations.Count; i < n; i++)
        locations[i] = this.Visit(locations[i]);
      return locations;
    }

    /// <summary>
    /// Visits the specified location. Simply return the location. 
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns></returns>
    public virtual ILocation Visit(ILocation location) {
      return location;
    }

    /// <summary>
    /// Visits the (interface) ILocalDefinition. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="localDefinition"></param>
    /// <returns></returns>
    public virtual ILocalDefinition Visit(ILocalDefinition localDefinition) {
      if (this.stopTraversal) return localDefinition;
      var mutable = localDefinition as LocalDefinition;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(localDefinition);
        if (localDefinition.IsModified)
          this.Visit(localDefinition.CustomModifiers);
        this.Visit(localDefinition.Type);
        this.path.Pop();
      }
      return localDefinition;
    }

    /// <summary>
    /// Visits a mutable local definition.
    /// </summary>
    /// <param name="localDefinition">The local definition.</param>
    /// <returns></returns>
    public virtual LocalDefinition Mutate(LocalDefinition localDefinition) {
      if (this.stopTraversal) return localDefinition;
      this.path.Push(localDefinition);
      localDefinition.CustomModifiers = this.Mutate(localDefinition.CustomModifiers);
      localDefinition.Type = this.Visit(localDefinition.Type);
      this.path.Pop();
      return localDefinition;
    }

    /// <summary>
    /// Visits a reference to the specified local definition.
    /// </summary>
    /// <param name="localDefinition">The referenced local definition to visit.</param>
    public virtual ILocalDefinition VisitReferenceTo(ILocalDefinition localDefinition) {
      return localDefinition;
    }

    /// <summary>
    /// Visits the (interface) IMarshallingInformation. We first see if it is a mutable model node, if so, we 
    /// visit the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="marshallingInformation"></param>
    /// <returns></returns>
    public virtual IMarshallingInformation Visit(IMarshallingInformation marshallingInformation) {
      if (this.stopTraversal) return marshallingInformation;
      var mutable = marshallingInformation as MarshallingInformation;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(marshallingInformation);
        if (marshallingInformation.UnmanagedType == UnmanagedType.CustomMarshaler)
          this.Visit(marshallingInformation.CustomMarshaller);
        if (marshallingInformation.UnmanagedType == UnmanagedType.SafeArray &&
        (marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_DISPATCH ||
        marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_UNKNOWN ||
        marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_RECORD))
          this.Visit(marshallingInformation.SafeArrayElementUserDefinedSubtype);
        this.path.Pop();
      }
      return marshallingInformation;
    }

    /// <summary>
    /// Visits a mutable marshalling information.
    /// </summary>
    /// <param name="marshallingInformation">The marshalling information.</param>
    /// <returns></returns>
    public virtual MarshallingInformation Mutate(MarshallingInformation marshallingInformation) {
      if (this.stopTraversal) return marshallingInformation;
      this.path.Push(marshallingInformation);
      if (marshallingInformation.UnmanagedType == UnmanagedType.CustomMarshaler)
        marshallingInformation.CustomMarshaller = this.Visit(marshallingInformation.CustomMarshaller);
      if (marshallingInformation.UnmanagedType == UnmanagedType.SafeArray &&
      (marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_DISPATCH ||
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_UNKNOWN ||
      marshallingInformation.SafeArrayElementSubtype == VarEnum.VT_RECORD))
        marshallingInformation.SafeArrayElementUserDefinedSubtype = this.Visit(marshallingInformation.SafeArrayElementUserDefinedSubtype);
      this.path.Pop();
      return marshallingInformation;
    }

    /// <summary>
    /// Visits a mutable constant node.
    /// </summary>
    /// <param name="constant">The constant.</param>
    /// <returns></returns>
    public virtual MetadataConstant Mutate(MetadataConstant constant) {
      if (this.stopTraversal) return constant;
      this.path.Push(constant);
      constant.Locations = this.Mutate(constant.Locations);
      constant.Type = this.Visit(constant.Type);
      this.path.Pop();
      return constant;
    }

    /// <summary>
    /// Visits the mutable create array.
    /// </summary>
    /// <param name="createArray">The create array.</param>
    /// <returns></returns>
    public virtual MetadataCreateArray Mutate(MetadataCreateArray createArray) {
      if (this.stopTraversal) return createArray;
      this.path.Push(createArray);
      createArray.ElementType = this.Visit(createArray.ElementType);
      createArray.Initializers = this.Mutate(createArray.Initializers);
      createArray.Locations = this.Mutate(createArray.Locations);
      createArray.Type = this.Visit(createArray.Type);
      this.path.Pop();
      return createArray;
    }

    /// <summary>
    /// Visits the specified metadata expressions.
    /// </summary>
    /// <param name="metadataExpressions">The metadata expressions.</param>
    /// <returns></returns>
    public virtual List<IMetadataExpression>/*?*/ Mutate(List<IMetadataExpression>/*?*/ metadataExpressions) {
      if (this.stopTraversal || metadataExpressions == null) return metadataExpressions;
      for (int i = 0, n = metadataExpressions.Count; i < n; i++)
        metadataExpressions[i] = this.Dispatch(metadataExpressions[i]);
      return metadataExpressions;
    }

    /// <summary>
    /// Visits the (interface) IMetadataConstant node. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="constant"></param>
    /// <returns></returns>
    public virtual IMetadataConstant Visit(IMetadataConstant constant) {
      if (this.stopTraversal) return constant;
      MetadataConstant mutable = constant as MetadataConstant;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(constant);
        this.Visit(constant.Locations);
        this.Visit(constant.Type);
        this.path.Pop();
      }
      return constant;
    }

    /// <summary>
    /// Visits the (interface) IMetadataCreateArray node. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="createArray"></param>
    /// <returns></returns>
    public virtual IMetadataCreateArray Visit(IMetadataCreateArray createArray) {
      if (this.stopTraversal) return createArray;
      var mutable = createArray as MetadataCreateArray;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(createArray);
        this.Visit(createArray.ElementType);
        this.Visit(createArray.Initializers);
        this.Visit(createArray.Locations);
        this.Visit(createArray.Type);
        this.path.Pop();
      }
      return createArray;
    }

    /// <summary>
    /// Visits the (interface) IMetadataTypeOf node. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="typeOf"></param>
    /// <returns></returns>
    public virtual IMetadataTypeOf Visit(IMetadataTypeOf typeOf) {
      var mutable = typeOf as MetadataTypeOf;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        if (this.stopTraversal) return typeOf;
        this.path.Push(typeOf);
        this.Visit(typeOf.Locations);
        this.Visit(typeOf.Type);
        this.Visit(typeOf.TypeToGet);
        this.path.Pop();
      }
      return typeOf;
    }

    /// <summary>
    /// Visit a metadata named argument.
    /// </summary>
    /// <param name="namedArgument"></param>
    /// <returns></returns>
    public virtual IMetadataNamedArgument Visit(IMetadataNamedArgument namedArgument) {
      if (this.stopTraversal) return namedArgument;
      var mutable = namedArgument as MetadataNamedArgument;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(namedArgument);
        this.Dispatch(namedArgument.ArgumentValue);
        this.Visit(namedArgument.Type);
        this.path.Pop();
      }
      return namedArgument;
    }

    /// <summary>
    /// Visits the specified expression. 
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public virtual IMetadataExpression Dispatch(IMetadataExpression expression) {
      if (this.stopTraversal) return expression;
      IMetadataConstant/*?*/ metadataConstant = expression as IMetadataConstant;
      if (metadataConstant != null) return this.Visit(metadataConstant);
      IMetadataCreateArray/*?*/ metadataCreateArray = expression as IMetadataCreateArray;
      if (metadataCreateArray != null) return this.Visit(metadataCreateArray);
      IMetadataTypeOf/*?*/ metadataTypeOf = expression as IMetadataTypeOf;
      if (metadataTypeOf != null) return this.Visit(metadataTypeOf);
      IMetadataNamedArgument metadataNamedArgument = expression as IMetadataNamedArgument;
      if (metadataNamedArgument != null) return this.Visit(metadataNamedArgument);
      return expression;
    }

    /// <summary>
    /// Visits the specified named arguments.
    /// </summary>
    /// <param name="namedArguments">The named arguments.</param>
    /// <returns></returns>
    public virtual List<IMetadataNamedArgument>/*?*/ Mutate(List<IMetadataNamedArgument>/*?*/ namedArguments) {
      if (this.stopTraversal || namedArguments == null) return namedArguments;
      for (int i = 0, n = namedArguments.Count; i < n; i++)
        namedArguments[i] = (IMetadataNamedArgument)this.Dispatch(namedArguments[i]);
      return namedArguments;
    }

    /// <summary>
    /// Visits the specified named argument.
    /// </summary>
    /// <param name="namedArgument">The named argument.</param>
    /// <returns></returns>
    public virtual MetadataNamedArgument Mutate(MetadataNamedArgument namedArgument) {
      if (this.stopTraversal) return namedArgument;
      this.path.Push(namedArgument);
      namedArgument.ArgumentValue = this.Dispatch(namedArgument.ArgumentValue);
      namedArgument.Locations = this.Mutate(namedArgument.Locations);
      namedArgument.Type = this.Visit(namedArgument.Type);
      this.path.Pop();
      return namedArgument;
    }

    /// <summary>
    /// Visits the mutable metadata type-of node.
    /// </summary>
    /// <param name="typeOf">The type of.</param>
    /// <returns></returns>
    public virtual MetadataTypeOf Mutate(MetadataTypeOf typeOf) {
      if (this.stopTraversal) return typeOf;
      this.path.Push(typeOf);
      typeOf.Locations = this.Mutate(typeOf.Locations);
      typeOf.Type = this.Visit(typeOf.Type);
      typeOf.TypeToGet = this.Visit(typeOf.TypeToGet);
      this.path.Pop();
      return typeOf;
    }

    /// <summary>
    /// Visits a mutable method body.
    /// </summary>
    /// <param name="methodBody">The method body.</param>
    /// <returns></returns>
    public virtual MethodBody Mutate(MethodBody methodBody) {
      if (this.stopTraversal) return methodBody;
      this.path.Push(methodBody);
      var m = this.GetCurrentMethod();
      if (!(m is Dummy)) {
        methodBody.MethodDefinition = m;
      }
      methodBody.LocalVariables = this.Mutate(methodBody.LocalVariables);
      methodBody.Operations = this.Mutate(methodBody.Operations);
      methodBody.OperationExceptionInformation = this.Mutate(methodBody.OperationExceptionInformation);
      this.path.Pop();
      return methodBody;
    }

    /// <summary>
    /// Visits the specified exception informations.
    /// </summary>
    /// <param name="exceptionInformations">The exception informations.</param>
    /// <returns></returns>
    public virtual List<IOperationExceptionInformation>/*?*/ Mutate(List<IOperationExceptionInformation>/*?*/ exceptionInformations) {
      if (this.stopTraversal || exceptionInformations == null) return exceptionInformations;
      for (int i = 0, n = exceptionInformations.Count; i < n; i++)
        exceptionInformations[i] = this.Visit(exceptionInformations[i]);
      return exceptionInformations;
    }

    /// <summary>
    /// Visit an (interface) IOperationExceptionInformation node. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="operationExceptionInformation"></param>
    /// <returns></returns>
    public virtual IOperationExceptionInformation Visit(IOperationExceptionInformation operationExceptionInformation) {
      if (this.stopTraversal) return operationExceptionInformation;
      var mutable = operationExceptionInformation as OperationExceptionInformation;
      if (mutable != null)
        return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(operationExceptionInformation);
        this.Visit(operationExceptionInformation.ExceptionType);
        this.path.Pop();
      }
      return operationExceptionInformation;
    }

    /// <summary>
    /// Visits an mutable operation exception information node. 
    /// </summary>
    /// <param name="operationExceptionInformation">The operation exception information.</param>
    /// <returns></returns>
    public virtual OperationExceptionInformation Mutate(OperationExceptionInformation operationExceptionInformation) {
      if (this.stopTraversal) return operationExceptionInformation;
      this.path.Push(operationExceptionInformation);
      operationExceptionInformation.ExceptionType = this.Visit(operationExceptionInformation.ExceptionType);
      this.path.Pop();
      return operationExceptionInformation;
    }

    /// <summary>
    /// Visits the specified operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <returns></returns>
    public virtual List<IOperation>/*?*/ Mutate(List<IOperation>/*?*/ operations) {
      if (this.stopTraversal || operations == null) return operations;
      for (int i = 0, n = operations.Count; i < n; i++)
        operations[i] = this.Visit(operations[i]);
      return operations;
    }

    /// <summary>
    /// Visit an interface IOperation node. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public virtual IOperation Visit(IOperation operation) {
      if (this.stopTraversal) return operation;
      var mutable = operation as Operation;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        ITypeReference/*?*/ typeReference = operation.Value as ITypeReference;
        if (typeReference != null)
          this.Visit(typeReference);
        else {
          IFieldReference/*?*/ fieldReference = operation.Value as IFieldReference;
          if (fieldReference != null)
            this.Visit(fieldReference);
          else {
            IMethodReference/*?*/ methodReference = operation.Value as IMethodReference;
            if (methodReference != null)
              this.Visit(methodReference);
          }
        }
      }
      return operation;
    }

    /// <summary>
    /// Visits the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <returns></returns>
    public virtual Operation Mutate(Operation operation) {
      if (this.stopTraversal) return operation;
      this.path.Push(operation);
      ITypeReference/*?*/ typeReference = operation.Value as ITypeReference;
      if (typeReference != null)
        operation.Value = this.Visit(typeReference);
      else {
        IFieldReference/*?*/ fieldReference = operation.Value as IFieldReference;
        if (fieldReference != null)
          operation.Value = this.Visit(fieldReference);
        else {
          IMethodReference/*?*/ methodReference = operation.Value as IMethodReference;
          if (methodReference != null)
            operation.Value = this.Visit(methodReference);
          else {
            IParameterDefinition/*?*/ parameterDefinition = operation.Value as IParameterDefinition;
            if (parameterDefinition != null)
              operation.Value = this.VisitReferenceTo(parameterDefinition);
            else {
              ILocalDefinition/*?*/ localDefinition = operation.Value as ILocalDefinition;
              if (localDefinition != null)
                operation.Value = this.VisitReferenceTo(localDefinition);
            }
          }
        }
      }
      this.path.Pop();
      return operation;
    }

    /// <summary>
    /// Visits the specified locals.
    /// </summary>
    /// <param name="locals">The locals.</param>
    /// <returns></returns>
    public virtual List<ILocalDefinition>/*?*/ Mutate(List<ILocalDefinition>/*?*/ locals) {
      if (this.stopTraversal || locals == null) return locals;
      for (int i = 0, n = locals.Count; i < n; i++)
        locals[i] = this.Visit(locals[i]);
      return locals;
    }

    /// <summary>
    /// Visits the specified method definitions.
    /// </summary>
    /// <param name="methodDefinitions">The method definitions.</param>
    /// <returns></returns>
    public virtual List<IMethodDefinition>/*?*/ Mutate(List<IMethodDefinition>/*?*/ methodDefinitions) {
      if (this.stopTraversal || methodDefinitions == null) return methodDefinitions;
      for (int i = 0, n = methodDefinitions.Count; i < n; i++)
        methodDefinitions[i] = this.Visit(methodDefinitions[i]);
      return methodDefinitions;
    }

    /// <summary>
    /// Visits an (interface) global field definition node. We first see if it is a mutable model node, if so, we visit
    /// the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="globalFieldDefinition">The global field definition.</param>
    /// <returns></returns>
    public virtual IGlobalFieldDefinition Visit(IGlobalFieldDefinition globalFieldDefinition) {
      if (this.stopTraversal) return globalFieldDefinition;
      var mutable = globalFieldDefinition as GlobalFieldDefinition;
      if (mutable != null)
        return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(this.Visit(globalFieldDefinition.ContainingTypeDefinition));
        if (globalFieldDefinition.IsCompileTimeConstant)
          this.Visit(globalFieldDefinition.CompileTimeValue);
        if (globalFieldDefinition.IsMapped)
          this.Visit(globalFieldDefinition.FieldMapping);
        if (globalFieldDefinition.IsMarshalledExplicitly)
          this.Visit(globalFieldDefinition.MarshallingInformation);
        this.Visit(globalFieldDefinition.Type);
        this.path.Pop();
      }
      return globalFieldDefinition;
    }

    /// <summary>
    /// Visits an (interface) global method definition. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="globalMethodDefinition">The global method definition.</param>
    /// <returns></returns>
    public virtual IGlobalMethodDefinition Visit(IGlobalMethodDefinition globalMethodDefinition) {
      if (this.stopTraversal) return globalMethodDefinition;
      var mutable = globalMethodDefinition as GlobalMethodDefinition;
      if (mutable != null)
        return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(globalMethodDefinition);
        if (globalMethodDefinition.IsGeneric) {
          foreach (var gp in globalMethodDefinition.GenericParameters)
            this.Visit(gp);
        }
        this.Visit(globalMethodDefinition.Parameters);
        if (globalMethodDefinition.IsPlatformInvoke)
          this.Visit(globalMethodDefinition.PlatformInvokeData);
        this.Visit(globalMethodDefinition.ReturnValueAttributes);
        if (globalMethodDefinition.ReturnValueIsModified)
          this.Visit(globalMethodDefinition.ReturnValueCustomModifiers);
        if (globalMethodDefinition.ReturnValueIsMarshalledExplicitly)
          this.Visit(globalMethodDefinition.ReturnValueMarshallingInformation);
        if (globalMethodDefinition.HasDeclarativeSecurity)
          this.Visit(globalMethodDefinition.SecurityAttributes);
        this.Visit(globalMethodDefinition.Type);
        if (!globalMethodDefinition.IsAbstract && !globalMethodDefinition.IsExternal)
          this.Visit(globalMethodDefinition.Body);
        this.path.Pop();
      }
      return globalMethodDefinition;
    }

    /// <summary>
    /// Visits interface method definition node. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns></returns>
    public virtual IMethodDefinition Visit(IMethodDefinition methodDefinition) {
      // No need to dispatch to methodDefinition
      if (this.stopTraversal) return methodDefinition;
      MethodDefinition mutable = methodDefinition as MethodDefinition;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(methodDefinition);
        if (methodDefinition.IsGeneric) {
          foreach (var gp in methodDefinition.GenericParameters)
            this.Visit(gp);
        }
        this.Visit(methodDefinition.Parameters);
        if (methodDefinition.IsPlatformInvoke)
          this.Visit(methodDefinition.PlatformInvokeData);
        this.Visit(methodDefinition.ReturnValueAttributes);
        if (methodDefinition.ReturnValueIsModified)
          this.Visit(methodDefinition.ReturnValueCustomModifiers);
        if (methodDefinition.ReturnValueIsMarshalledExplicitly)
          this.Visit(methodDefinition.ReturnValueMarshallingInformation);
        if (methodDefinition.HasDeclarativeSecurity)
          this.Visit(methodDefinition.SecurityAttributes);
        this.Visit(methodDefinition.Type);
        if (!methodDefinition.IsAbstract && !methodDefinition.IsExternal)
          this.Visit(methodDefinition.Body);
        this.path.Pop();
      }
      return methodDefinition;
    }

    /// <summary>
    /// Visits the mutable method definition.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns></returns>
    public virtual MethodDefinition Mutate(MethodDefinition methodDefinition) {
      if (this.stopTraversal) return methodDefinition;
      this.path.Push(methodDefinition);
      this.VisitTypeDefinitionMember(methodDefinition);
      if (methodDefinition.IsGeneric)
        methodDefinition.GenericParameters = this.Mutate(methodDefinition.GenericParameters);
      methodDefinition.Parameters = this.Mutate(methodDefinition.Parameters);
      if (methodDefinition.IsPlatformInvoke)
        methodDefinition.PlatformInvokeData = this.Visit(methodDefinition.PlatformInvokeData);
      methodDefinition.ReturnValueAttributes = this.VisitMethodReturnValueAttributes(methodDefinition.ReturnValueAttributes);
      if (methodDefinition.ReturnValueIsModified)
        methodDefinition.ReturnValueCustomModifiers = this.VisitMethodReturnValueCustomModifiers(methodDefinition.ReturnValueCustomModifiers);
      if (methodDefinition.ReturnValueIsMarshalledExplicitly)
        methodDefinition.ReturnValueMarshallingInformation = this.Visit(methodDefinition.ReturnValueMarshallingInformation);
      if (methodDefinition.HasDeclarativeSecurity)
        methodDefinition.SecurityAttributes = this.Mutate(methodDefinition.SecurityAttributes);
      methodDefinition.Type = this.Visit(methodDefinition.Type);
      if (!methodDefinition.IsAbstract && !methodDefinition.IsExternal)
        methodDefinition.Body = this.Visit(methodDefinition.Body);
      this.path.Pop();
      return methodDefinition;
    }

    /// <summary>
    /// Visits the specified method implementations.
    /// </summary>
    /// <param name="methodImplementations">The method implementations.</param>
    /// <returns></returns>
    public virtual List<IMethodImplementation>/*?*/ Mutate(List<IMethodImplementation>/*?*/ methodImplementations) {
      if (this.stopTraversal || methodImplementations == null) return methodImplementations;
      for (int i = 0, n = methodImplementations.Count; i < n; i++)
        methodImplementations[i] = this.Visit(methodImplementations[i]);
      return methodImplementations;
    }

    /// <summary>
    /// Visit an (interface) IMethodImplementation. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="methodImplementation"></param>
    /// <returns></returns>
    public virtual IMethodImplementation Visit(IMethodImplementation methodImplementation) {
      if (this.stopTraversal) return methodImplementation;
      var mutable = methodImplementation as MethodImplementation;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(methodImplementation);
        this.Visit(methodImplementation.ImplementedMethod);
        this.Visit(methodImplementation.ImplementingMethod);
        this.path.Pop();
      }
      return methodImplementation;
    }

    /// <summary>
    /// Visits a mutable method implementation node.
    /// </summary>
    /// <param name="methodImplementation">The method implementation.</param>
    /// <returns></returns>
    public virtual MethodImplementation Mutate(MethodImplementation methodImplementation) {
      if (this.stopTraversal) return methodImplementation;
      this.path.Push(methodImplementation);
      var t = this.GetCurrentType();
      if (!(t is Dummy)) {
        methodImplementation.ContainingType = t;
      }
      methodImplementation.ImplementedMethod = this.Visit(methodImplementation.ImplementedMethod);
      methodImplementation.ImplementingMethod = this.Visit(methodImplementation.ImplementingMethod);
      this.path.Pop();
      return methodImplementation;
    }

    /// <summary>
    /// Visits the specified method references.
    /// </summary>
    /// <param name="methodReferences">The method references.</param>
    /// <returns></returns>
    public virtual List<IMethodReference>/*?*/ Mutate(List<IMethodReference>/*?*/ methodReferences) {
      if (this.stopTraversal || methodReferences == null) return methodReferences;
      for (int i = 0, n = methodReferences.Count; i < n; i++)
        methodReferences[i] = this.Visit(methodReferences[i]);
      return methodReferences;
    }

    /// <summary>
    /// Visits an (interface) method body. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="methodBody">The method body.</param>
    /// <returns></returns>
    public virtual IMethodBody Visit(IMethodBody methodBody) {
      if (this.stopTraversal) return methodBody;
      var mutable = methodBody as MethodBody;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        foreach (var loc in methodBody.LocalVariables)
          this.Visit(loc);
        foreach (var operation in methodBody.Operations)
          this.Visit(operation);
        foreach (var exceptionInfo in methodBody.OperationExceptionInformation)
          this.Visit(exceptionInfo);
      }
      return methodBody;
    }

    /// <summary>
    /// Visits an (interface) method reference. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// 
    /// Note that mutable nodes that implement IMethodReference include those who implement ISpecializedMethodReference (SpecializedMethodDefinition
    /// and SpecializedMethodReference), who implements IGenericMethodInstanceReference(GenericMethodInstance and GenericMethodInstanceReference), 
    /// who implements IGlobalMethodDefinition (GlobalMethodDefinition), who implements IMethodDefinition otherwise( MethodDefinition) and
    /// MethodReference. For definition nodes, we have to visit them as reference nodes. 
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns></returns>
    public virtual IMethodReference Visit(IMethodReference methodReference) {
      if (this.stopTraversal) return methodReference;
      if (methodReference is Dummy) return Dummy.MethodReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(methodReference, out cachedValue)) {
        return (IMethodReference)cachedValue;
      }
      ISpecializedMethodReference/*?*/ specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null)
        return this.Visit(specializedMethodReference);
      IGenericMethodInstanceReference/*?*/ genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null)
        return this.Visit(genericMethodInstanceReference);
      else {
        IGlobalMethodDefinition/*?*/ globalMethodDefinition = methodReference as IGlobalMethodDefinition;
        if (globalMethodDefinition != null)
          return this.Mutate(this.GetReferenceCopy(globalMethodDefinition));
        IMethodDefinition/*?*/ methodDefinition = methodReference as IMethodDefinition;
        if (methodDefinition != null) {
          return this.Mutate(this.GetReferenceCopy(methodDefinition));
        }
        var mutable = methodReference as MethodReference;
        if (mutable != null) return this.Mutate(this.GetReferenceCopy(mutable));
        if (this.visitImmutableNodes) {
          this.path.Push(methodReference);
          this.Visit(methodReference.Attributes);
          this.Visit(methodReference.ContainingType);
          this.Visit(methodReference.ExtraParameters);
          this.Visit(methodReference.Locations);
          this.Visit(methodReference.Parameters);
          if (methodReference.ReturnValueIsModified)
            this.Visit(methodReference.ReturnValueCustomModifiers);
          this.Visit(methodReference.Type);
          this.path.Pop();
        }
        return methodReference;
      }
    }

    /// <summary>
    /// Visits a mutable method reference.
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns></returns>
    public virtual MethodReference Mutate(MethodReference methodReference) {
      if (this.stopTraversal) return methodReference;
      this.path.Push(methodReference);
      methodReference.Attributes = this.Mutate(methodReference.Attributes);
      methodReference.ContainingType = this.Visit(methodReference.ContainingType);
      methodReference.ExtraParameters = this.Mutate(methodReference.ExtraParameters);
      methodReference.Locations = this.Mutate(methodReference.Locations);
      methodReference.Parameters = this.Mutate(methodReference.Parameters);
      if (methodReference.ReturnValueIsModified)
        methodReference.ReturnValueCustomModifiers = this.Mutate(methodReference.ReturnValueCustomModifiers);
      methodReference.Type = this.Visit(methodReference.Type);
      this.path.Pop();
      return methodReference;
    }

    /// <summary>
    /// Visits the specified modules.
    /// </summary>
    /// <param name="modules">The modules.</param>
    /// <returns></returns>
    public virtual List<IModule>/*?*/ Mutate(List<IModule>/*?*/ modules) {
      if (this.stopTraversal || modules == null) return modules;
      for (int i = 0, n = modules.Count; i < n; i++) {
        modules[i] = this.Visit(modules[i]);
      }
      return modules;
    }

    /// <summary>
    /// Visit an interface module. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="module">The module.</param>
    public virtual IModule Visit(IModule module) {
      if (this.stopTraversal) return module;
      var assembly = module as IAssembly;
      if (assembly != null) return this.Visit(assembly);
      var mutable = module as Module;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        if (this.stopTraversal) return module;
        this.path.Push(module);
        foreach (var aref in module.AssemblyReferences)
          this.Visit(aref);
        this.Visit(module.Locations);
        this.Visit(module.ModuleAttributes);
        foreach (var mref in module.ModuleReferences)
          this.Visit(mref);
        foreach (var wres in module.Win32Resources)
          this.Visit(wres);
        this.Visit(module.UnitNamespaceRoot);
        this.path.Pop();
      }
      return module;
    }

    /// <summary>
    /// Visits a mutable module.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <returns></returns>
    public virtual Module Mutate(Module module) {
      if (this.stopTraversal) return module;
      this.path.Push(module);
      module.AssemblyReferences = this.Mutate(module.AssemblyReferences);
      module.Locations = this.Mutate(module.Locations);
      module.UninterpretedSections = this.Mutate(module.UninterpretedSections);
      module.ModuleAttributes = this.Mutate(module.ModuleAttributes);
      module.ModuleReferences = this.Mutate(module.ModuleReferences);
      module.Win32Resources = this.Mutate(module.Win32Resources);
      module.UnitNamespaceRoot = this.Visit((IRootUnitNamespace)module.UnitNamespaceRoot);
      this.path.Push(module.UnitNamespaceRoot);
      if (module.AllTypes.Count > 0)
        this.Visit((INamespaceTypeDefinition)module.AllTypes[0]);
      if (module.AllTypes.Count > 1) {
        INamespaceTypeDefinition globalsType = module.AllTypes[1] as INamespaceTypeDefinition;
        if (globalsType != null && globalsType.Name.Value == "__Globals__")
          this.Visit(globalsType);
      }
      this.VisitPrivateHelperMembers(module.AllTypes);
      module.TypeMemberReferences = null;
      module.TypeReferences = null;
      module.EntryPoint = this.Visit(module.EntryPoint);
      this.path.Pop();
      this.path.Pop();
      return module;
    }

    /// <summary>
    /// Visits the specified module references.
    /// </summary>
    /// <param name="moduleReferences">The module references.</param>
    /// <returns></returns>
    public virtual List<IModuleReference>/*?*/ Mutate(List<IModuleReference>/*?*/ moduleReferences) {
      if (this.stopTraversal || moduleReferences == null) return moduleReferences;
      for (int i = 0, n = moduleReferences.Count; i < n; i++)
        moduleReferences[i] = this.Visit(moduleReferences[i]);
      return moduleReferences;
    }

    /// <summary>
    /// Visits the specified module reference. Will not make a copy. 
    /// </summary>
    /// <param name="moduleReference">The module reference.</param>
    /// <returns></returns>
    public virtual ModuleReference Mutate(ModuleReference moduleReference) {
      if (this.stopTraversal) return moduleReference;
      if (!(moduleReference.ResolvedModule is Dummy)) {
        IReference/*?*/ mutatedResolvedModule = null;
        if (this.referenceCache.TryGetValue(moduleReference.ResolvedModule, out mutatedResolvedModule))
          moduleReference.ResolvedModule = (IModule)mutatedResolvedModule;
      }
      moduleReference.Host = this.host;
      moduleReference.ReferringUnit = this.GetCurrentUnit();
      return moduleReference;
    }

    /// <summary>
    /// Visits the specified namespace members.
    /// </summary>
    /// <param name="namespaceMembers">The namespace members.</param>
    /// <returns></returns>
    public virtual List<INamespaceMember>/*?*/ Mutate(List<INamespaceMember>/*?*/ namespaceMembers) {
      if (this.stopTraversal || namespaceMembers == null) return namespaceMembers;
      for (int i = 0, n = namespaceMembers.Count; i < n; i++)
        namespaceMembers[i] = this.Dispatch(namespaceMembers[i]);
      return namespaceMembers;
    }

    /// <summary>
    /// Visits the specified namespace member. Dispatch to type specific visiting methods. 
    /// </summary>
    /// <param name="namespaceMember">The namespace member.</param>
    /// <returns></returns>
    public INamespaceMember Dispatch(INamespaceMember namespaceMember) {
      if (this.stopTraversal) return namespaceMember;
      INamespaceTypeDefinition/*?*/ namespaceTypeDefinition = namespaceMember as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) return this.Visit(namespaceTypeDefinition);
      INestedUnitNamespace/*?*/ nestedUnitNamespace = namespaceMember as INestedUnitNamespace;
      if (nestedUnitNamespace != null) return this.Visit(nestedUnitNamespace);
      IGlobalMethodDefinition/*?*/ globalMethodDefinition = namespaceMember as IGlobalMethodDefinition;
      if (globalMethodDefinition != null) return this.Visit(globalMethodDefinition);
      IGlobalFieldDefinition/*?*/ globalFieldDefinition = namespaceMember as IGlobalFieldDefinition;
      if (globalFieldDefinition != null) return this.Visit(globalFieldDefinition);
      return namespaceMember;
    }

    /// <summary>
    /// Visits a mutable namespace alias for type.
    /// </summary>
    /// <param name="namespaceAliasForType">Type of the namespace alias for.</param>
    /// <returns></returns>
    public virtual NamespaceAliasForType Mutate(NamespaceAliasForType namespaceAliasForType) {
      if (this.stopTraversal) return namespaceAliasForType;
      this.path.Push(namespaceAliasForType);
      namespaceAliasForType.AliasedType = (INamedTypeReference)this.Visit(namespaceAliasForType.AliasedType);
      namespaceAliasForType.Attributes = this.Mutate(namespaceAliasForType.Attributes);
      namespaceAliasForType.Locations = this.Mutate(namespaceAliasForType.Locations);
      //TODO: what about the containing namespace? Should that be a reference?
      this.path.Pop();
      return namespaceAliasForType;
    }

    /// <summary>
    /// Visits a mutable namespace type definition.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The namespace type definition.</param>
    /// <returns></returns>
    public virtual NamespaceTypeDefinition Mutate(NamespaceTypeDefinition namespaceTypeDefinition) {
      if (this.stopTraversal) return namespaceTypeDefinition;
      this.VisitTypeDefinition(namespaceTypeDefinition);
      var n = this.GetCurrentNamespace();
      if (!(n is Dummy)) {
        namespaceTypeDefinition.ContainingUnitNamespace = n;
      }
      return namespaceTypeDefinition;
    }

    /// <summary>
    /// Visits a mutable node for nested alias for type.
    /// </summary>
    /// <param name="nestedAliasForType">Type of the nested alias for.</param>
    /// <returns></returns>
    public virtual NestedAliasForType Mutate(NestedAliasForType nestedAliasForType) {
      if (this.stopTraversal) return nestedAliasForType;
      this.path.Push(nestedAliasForType);
      nestedAliasForType.AliasedType = (INamedTypeReference)this.Visit(nestedAliasForType.AliasedType);
      nestedAliasForType.Attributes = this.Mutate(nestedAliasForType.Attributes);
      nestedAliasForType.Locations = this.Mutate(nestedAliasForType.Locations);
      //TODO: what about the containing type? Should that be a reference?
      this.path.Pop();
      return nestedAliasForType;
    }

    /// <summary>
    /// Visits the specified nested type definitions.
    /// </summary>
    /// <param name="nestedTypeDefinitions">The nested type definitions.</param>
    /// <returns></returns>
    public virtual List<INestedTypeDefinition>/*?*/ Mutate(List<INestedTypeDefinition>/*?*/ nestedTypeDefinitions) {
      if (this.stopTraversal || nestedTypeDefinitions == null) return nestedTypeDefinitions;
      for (int i = 0, n = nestedTypeDefinitions.Count; i < n; i++)
        nestedTypeDefinitions[i] = this.Visit(nestedTypeDefinitions[i]);
      return nestedTypeDefinitions;
    }

    /// <summary>
    /// Visits the mutable nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition">The nested type definition.</param>
    /// <returns></returns>
    public virtual NestedTypeDefinition Mutate(NestedTypeDefinition nestedTypeDefinition) {
      if (this.stopTraversal) return nestedTypeDefinition;
      this.VisitTypeDefinition(nestedTypeDefinition);
      var t = this.GetCurrentType();
      if (!(t is Dummy)) {
        nestedTypeDefinition.ContainingTypeDefinition = t;
      }
      return nestedTypeDefinition;
    }

    /// <summary>
    /// Visits a mutable nested type reference. 
    /// 
    /// </summary>
    /// <param name="nestedTypeReference">The nested type reference.</param>
    /// <returns></returns>
    public virtual NestedTypeReference VisitNestedTypeReference(NestedTypeReference nestedTypeReference) {
      if (this.stopTraversal) return nestedTypeReference;
      this.VisitTypeReference((TypeReference)nestedTypeReference);
      this.path.Push(nestedTypeReference);
      nestedTypeReference.ContainingType = this.Visit(nestedTypeReference.ContainingType);
      this.path.Pop();
      return nestedTypeReference;
    }

    /// <summary>
    /// Visits the interface ISpecializedFieldReference. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// 
    /// Note that mutable nodes that implement ISpecializedFieldReference include SpecializedFieldReference and SpecializedFieldDefinition. 
    /// </summary>
    /// <param name="specializedFieldReference"></param>
    /// <returns></returns>
    public virtual ISpecializedFieldReference Visit(ISpecializedFieldReference specializedFieldReference) {
      if (this.stopTraversal) return specializedFieldReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(specializedFieldReference, out cachedValue)) {
        return (ISpecializedFieldReference)cachedValue;
      }
      var mutable1 = specializedFieldReference as SpecializedFieldReference;
      if (mutable1 != null) return this.Mutate(this.GetReferenceCopy(mutable1));
      var mutable2 = specializedFieldReference as Immutable.SpecializedFieldDefinition;
      if (mutable2 != null) return this.Mutate(this.GetReferenceCopy(mutable2));
      if (this.visitImmutableNodes) {
        this.path.Push(specializedFieldReference);
        this.Visit(specializedFieldReference.Attributes);
        this.Visit(specializedFieldReference.ContainingType);
        this.Visit(specializedFieldReference.Locations);
        this.Visit(specializedFieldReference.Type);
        this.Visit(specializedFieldReference.UnspecializedVersion);
        this.path.Pop();
      }
      return specializedFieldReference;
    }

    /// <summary>
    /// Visits a mutable specialized field reference.
    /// </summary>
    /// <param name="specializedFieldReference">The specialized field reference.</param>
    /// <returns></returns>
    public virtual SpecializedFieldReference Mutate(SpecializedFieldReference specializedFieldReference) {
      if (this.stopTraversal) return specializedFieldReference;
      this.Mutate((FieldReference)specializedFieldReference);
      this.path.Push(specializedFieldReference);
      specializedFieldReference.UnspecializedVersion = this.Visit(specializedFieldReference.UnspecializedVersion);
      this.path.Pop();
      return specializedFieldReference;
    }

    /// <summary>
    /// Visits an (interface) ISpecializedMethodReference. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// 
    /// Note that mutable nodes that implement ISpecializedMethodReference include SpecializedMethodReference and 
    /// SpecializedMethodDefinition.
    /// </summary>
    /// <param name="specializedMethodReference"></param>
    /// <returns></returns>
    public virtual ISpecializedMethodReference Visit(ISpecializedMethodReference specializedMethodReference) {
      if (this.stopTraversal) return specializedMethodReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(specializedMethodReference, out cachedValue)) {
        return (ISpecializedMethodReference)specializedMethodReference;
      }
      var mutable1 = specializedMethodReference as SpecializedMethodReference;
      if (mutable1 != null) return this.Mutate(this.GetReferenceCopy(mutable1));
      var mutable2 = specializedMethodReference as Immutable.SpecializedMethodDefinition;
      if (mutable2 != null) return this.Mutate(this.GetReferenceCopy(mutable2));
      if (this.visitImmutableNodes) {
        this.path.Push(specializedMethodReference);
        this.Visit(specializedMethodReference.Attributes);
        this.Visit(specializedMethodReference.ContainingType);
        this.Visit(specializedMethodReference.ExtraParameters);
        this.Visit(specializedMethodReference.Locations);
        this.Visit(specializedMethodReference.Parameters);
        if (specializedMethodReference.ReturnValueIsModified)
          this.Visit(specializedMethodReference.ReturnValueCustomModifiers);
        this.Visit(specializedMethodReference.Type);
        this.Visit(specializedMethodReference.UnspecializedVersion);
        this.path.Pop();
      }
      return specializedMethodReference;
    }

    /// <summary>
    /// Visits a mutable specialized method reference.
    /// </summary>
    /// <param name="specializedMethodReference">The specialized method reference.</param>
    /// <returns></returns>
    public virtual SpecializedMethodReference Mutate(SpecializedMethodReference specializedMethodReference) {
      if (this.stopTraversal) return specializedMethodReference;
      this.Mutate((MethodReference)specializedMethodReference);
      this.path.Push(specializedMethodReference);
      specializedMethodReference.UnspecializedVersion = this.Visit(specializedMethodReference.UnspecializedVersion);
      this.path.Pop();
      return specializedMethodReference;
    }

    /// <summary>
    /// Visits an (interface) ISpecializedNestedTypeReference node. We first check in the cache and return the cached value if there 
    /// is one. Then we call the Mutate method where a copy may be produced if anything changes. If not, we either return, 
    /// or continue to visit the immutable subnodes, according to the flag (this.visitImmutableNodes). 
    /// 
    /// If a subclass want to change the node, override the corresponding Mutate method.
    /// 
    /// Note that mutable nodes that implement ISpecializedNestedTypeReference include SpecializedNestedTypeReference and 
    /// SpecializedNestedTypeDefinition. 
    /// </summary>
    /// <param name="specializedNestedTypeReference"></param>
    /// <returns></returns>
    public virtual ISpecializedNestedTypeReference Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      if (this.stopTraversal) return specializedNestedTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(specializedNestedTypeReference, out cachedValue)) {
        return (ISpecializedNestedTypeReference)cachedValue;
      }
      var result = this.Mutate(specializedNestedTypeReference);
      this.referenceCache.Add(specializedNestedTypeReference, result);
      if (specializedNestedTypeReference != result)
        this.referenceCache.Add(result, result);
      return result;
    }

    /// <summary>
    /// Mutate an ISpecializedNestedTypeReference. 
    /// </summary>
    /// <param name="specializedNestedTypeReference"></param>
    /// <returns></returns>
    public virtual ISpecializedNestedTypeReference Mutate(ISpecializedNestedTypeReference specializedNestedTypeReference) {
      //^ requires !this.referenceCache.ContainsKey(specializedNestedTypeReference);
      //^ ensures !this.referenceCache.ContainsKey(specializedNestedTypeReference);
      if (this.stopTraversal) return specializedNestedTypeReference;
      var nestedTypeReference = this.SubstituteNestedTypeReference(specializedNestedTypeReference);
      var unspecialized = this.Visit(specializedNestedTypeReference.UnspecializedVersion);
      if (nestedTypeReference != specializedNestedTypeReference || unspecialized != specializedNestedTypeReference.UnspecializedVersion) {
        var copy = MutableModelHelper.GetSpecializedNestedTypeReference(nestedTypeReference.ContainingType, nestedTypeReference.GenericParameterCount, nestedTypeReference.MangleName, nestedTypeReference.Name, unspecialized, this.host.InternFactory, nestedTypeReference);
        return copy;
      }
      return specializedNestedTypeReference;
    }

    private INestedTypeReference SubstituteNestedTypeReference(INestedTypeReference nestedTypeReference) {
      var containingType = this.Visit(nestedTypeReference.ContainingType);
      if (containingType != nestedTypeReference.ContainingType) {
        var copy = new NestedTypeReference();
        copy.Copy(nestedTypeReference, this.host.InternFactory);
        copy.ContainingType = containingType;
        return copy;
      }
      return nestedTypeReference;
    }

    /// <summary>
    /// Visits the mutable specialized nested type reference.
    /// </summary>
    /// <param name="specializedNestedTypeReference">The specialized nested type reference.</param>
    /// <returns></returns>
    public virtual SpecializedNestedTypeReference Visit(SpecializedNestedTypeReference specializedNestedTypeReference) {
      if (this.stopTraversal) return specializedNestedTypeReference;
      this.VisitNestedTypeReference((NestedTypeReference)specializedNestedTypeReference);
      this.path.Push(specializedNestedTypeReference);
      specializedNestedTypeReference.UnspecializedVersion = (INestedTypeReference)this.Visit(specializedNestedTypeReference.UnspecializedVersion);
      this.path.Pop();
      return specializedNestedTypeReference;
    }

    /// <summary>
    /// Replaces the child nodes of the given mutable type definition with the results of running the mutator over them. 
    /// </summary>
    /// <param name="typeDefinition">A mutable type definition.</param>
    protected virtual void VisitTypeDefinition(NamedTypeDefinition typeDefinition) {
      if (this.stopTraversal) return;
      this.path.Push(typeDefinition);
      typeDefinition.Attributes = this.Mutate(typeDefinition.Attributes);
      typeDefinition.BaseClasses = this.Mutate(typeDefinition.BaseClasses);
      typeDefinition.ExplicitImplementationOverrides = this.Mutate(typeDefinition.ExplicitImplementationOverrides);
      typeDefinition.GenericParameters = this.Mutate(typeDefinition.GenericParameters);
      typeDefinition.Interfaces = this.Mutate(typeDefinition.Interfaces);
      typeDefinition.Locations = this.Mutate(typeDefinition.Locations);
      typeDefinition.Events = this.Mutate(typeDefinition.Events);
      typeDefinition.Fields = this.Mutate(typeDefinition.Fields);
      typeDefinition.Methods = this.Mutate(typeDefinition.Methods);
      typeDefinition.NestedTypes = this.Mutate(typeDefinition.NestedTypes);
      typeDefinition.Properties = this.Mutate(typeDefinition.Properties);
      if (typeDefinition.HasDeclarativeSecurity)
        typeDefinition.SecurityAttributes = this.Mutate(typeDefinition.SecurityAttributes);
      if (typeDefinition.IsEnum)
        typeDefinition.UnderlyingType = this.Visit(typeDefinition.UnderlyingType);
      this.path.Pop();
    }

    /// <summary>
    /// Visits the private helper members.
    /// </summary>
    /// <param name="typeDefinitions">The type definitions.</param>
    public virtual void VisitPrivateHelperMembers(List<INamedTypeDefinition> typeDefinitions) {
      if (this.stopTraversal) return;
      for (int i = 0, n = typeDefinitions.Count; i < n; i++) {
        NamedTypeDefinition/*?*/ typeDef = typeDefinitions[i] as NamedTypeDefinition;
        if (typeDef == null) continue;
        this.path.Push(typeDef);
        typeDef.PrivateHelperMembers = this.Mutate(typeDef.PrivateHelperMembers);
        this.path.Pop();
      }
    }

    /// <summary>
    /// Visits the specified PE sections.
    /// </summary>
    /// <param name="peSections">The PE sections.</param>
    /// <returns></returns>
    public virtual List<IPESection>/*?*/ Mutate(List<IPESection>/*?*/ peSections) {
      if (this.stopTraversal || peSections == null) return peSections;
      for (int i = 0, n = peSections.Count; i < n; i++)
        peSections[i] = this.Visit(peSections[i]);
      return peSections;
    }

    /// <summary>
    /// Visits the specified type definition members.
    /// </summary>
    /// <param name="typeDefinitionMembers">The type definition members.</param>
    /// <returns></returns>
    public virtual List<ITypeDefinitionMember>/*?*/ Mutate(List<ITypeDefinitionMember>/*?*/ typeDefinitionMembers) {
      if (this.stopTraversal || typeDefinitionMembers == null) return typeDefinitionMembers;
      for (int i = 0, n = typeDefinitionMembers.Count; i < n; i++)
        typeDefinitionMembers[i] = this.Dispatch(typeDefinitionMembers[i]);
      return typeDefinitionMembers;
    }

    /// <summary>
    /// Visits the specified type definition member.
    /// </summary>
    /// <param name="typeDefinitionMember">The type definition member.</param>
    /// <returns></returns>
    public ITypeDefinitionMember Dispatch(ITypeDefinitionMember typeDefinitionMember) {
      IEventDefinition/*?*/ eventDef = typeDefinitionMember as IEventDefinition;
      if (eventDef != null) return this.Visit(eventDef);
      IFieldDefinition/*?*/ fieldDef = typeDefinitionMember as IFieldDefinition;
      if (fieldDef != null) return this.Visit(fieldDef);
      IMethodDefinition/*?*/ methodDef = typeDefinitionMember as IMethodDefinition;
      if (methodDef != null) return this.Visit(methodDef);
      INestedTypeDefinition/*?*/ nestedTypeDef = typeDefinitionMember as INestedTypeDefinition;
      if (nestedTypeDef != null) return this.Visit(nestedTypeDef);
      IPropertyDefinition/*?*/ propertyDef = typeDefinitionMember as IPropertyDefinition;
      if (propertyDef != null) return this.Visit(propertyDef);
      Debug.Assert(false);
      return typeDefinitionMember;
    }

    /// <summary>
    /// Visit the common members of a type definition member. 
    /// </summary>
    /// <param name="typeDefinitionMember"></param>
    /// <returns></returns>
    public ITypeDefinitionMember VisitITypeDefinitionMember(ITypeDefinitionMember typeDefinitionMember) {
      this.Visit(typeDefinitionMember.Attributes);
      this.Visit(typeDefinitionMember.Locations);
      return typeDefinitionMember;
    }

    /// <summary>
    /// Visits the specified type references.
    /// </summary>
    /// <param name="typeReferences">The type references.</param>
    /// <returns></returns>
    public virtual List<ITypeReference>/*?*/ Mutate(List<ITypeReference>/*?*/ typeReferences) {
      if (this.stopTraversal || typeReferences == null) return typeReferences;
      for (int i = 0, n = typeReferences.Count; i < n; i++)
        typeReferences[i] = this.Visit(typeReferences[i]);
      return typeReferences;
    }

    /// <summary>
    /// Visits the specified namespace member.
    /// </summary>
    /// <param name="namespaceMember">The namespace member.</param>
    /// <returns></returns>
    public virtual INamespaceMember Visit(INamespaceMember namespaceMember) {
      if (this.stopTraversal) return namespaceMember;
      INamespaceTypeDefinition/*?*/ namespaceTypeDefinition = namespaceMember as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) return this.Visit(namespaceTypeDefinition);
      INestedUnitNamespace/*?*/ nestedUnitNamespace = namespaceMember as INestedUnitNamespace;
      if (nestedUnitNamespace != null) return this.Visit(nestedUnitNamespace);
      IGlobalMethodDefinition/*?*/ globalMethodDefinition = namespaceMember as IGlobalMethodDefinition;
      if (globalMethodDefinition != null) return this.Visit(globalMethodDefinition);
      IGlobalFieldDefinition/*?*/ globalFieldDefinition = namespaceMember as IGlobalFieldDefinition;
      if (globalFieldDefinition != null) return this.Visit(globalFieldDefinition);
      return namespaceMember;
    }

    /// <summary>
    /// Visits the specified namespace type reference. We first check in the cache and return the cached value if there 
    /// is one. Then we call the Substitute method where a copy may be produced if anything changes. 
    /// 
    /// If a subclass want to change the node, override the corresponding Substitute method.
    /// </summary>
    /// <param name="namespaceTypeReference">The namespace type reference.</param>
    /// <returns></returns>
    public virtual INamespaceTypeReference Visit(INamespaceTypeReference namespaceTypeReference) {
      if (this.stopTraversal) return namespaceTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(namespaceTypeReference, out cachedValue)) {
        return (INamespaceTypeReference)cachedValue;
      }
      var result = this.Mutate(namespaceTypeReference);
      this.referenceCache.Add(namespaceTypeReference, result);
      if (result != namespaceTypeReference)
        this.referenceCache.Add(result, result);
      return result;
    }

    /// <summary>
    /// Creates a new copy of the reference node only if any its sub nodes are changed during recursive
    /// visit. Returns the input reference otherwise.
    /// 
    /// Override this method in the subclass if changes to this node are planned. 
    /// </summary>
    /// <param name="namespaceTypeReference"></param>
    /// <returns></returns>
    public virtual INamespaceTypeReference Mutate(INamespaceTypeReference namespaceTypeReference) {
      if (this.stopTraversal) return namespaceTypeReference;
      var containingUnit = this.Visit(namespaceTypeReference.ContainingUnitNamespace);
      if (containingUnit != namespaceTypeReference.ContainingUnitNamespace) {
        //var copy = new NamespaceTypeReference();
        //copy.Copy(namespaceTypeReference, this.host.InternFactory);
        //copy.ContainingUnitNamespace = containingUnit;
        var copy = MutableModelHelper.GetNamespaceTypeReference(containingUnit, namespaceTypeReference.GenericParameterCount, namespaceTypeReference.MangleName, namespaceTypeReference.Name, this.host.InternFactory,
          namespaceTypeReference);
        return copy;
      }
      return namespaceTypeReference;
    }

    /// <summary>
    /// Visits the specified nested type reference.
    /// </summary>
    /// <param name="nestedTypeReference">The nested type reference.</param>
    /// <returns></returns>
    public virtual INestedTypeReference Visit(INestedTypeReference nestedTypeReference) {
      if (this.stopTraversal) return nestedTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(nestedTypeReference, out cachedValue)) {
        return (INestedTypeReference)cachedValue;
      }
      ISpecializedNestedTypeReference/*?*/ specializedNestedTypeReference = nestedTypeReference as ISpecializedNestedTypeReference;
      if (specializedNestedTypeReference != null)
        return this.Visit(specializedNestedTypeReference);
      // no matter whether this nestedTypeReference is a nested type definition or not, mutate and cache it. 
      var result = this.Mutate(nestedTypeReference);
      this.referenceCache.Add(nestedTypeReference, result);
      if (nestedTypeReference != result)
        this.referenceCache.Add(result, result);
      return result;
    }

    /// <summary>
    /// Mutate an INestedTypeReference.
    /// </summary>
    /// <param name="nestedTypeReference"></param>
    /// <returns></returns>
    public virtual INestedTypeReference Mutate(INestedTypeReference nestedTypeReference) {
      //^ requires !this.referenceCache.ContainsKey(nestedTypeReference);
      //^ ensures !this.referenceCache.ContainsKey(nestedTypeReference);
      if (this.stopTraversal) return nestedTypeReference;
      var containingType = this.Visit(nestedTypeReference.ContainingType);
      if (containingType != nestedTypeReference.ContainingType) {
        return MutableModelHelper.GetNestedTypeReference(containingType, nestedTypeReference.GenericParameterCount, nestedTypeReference.MangleName, nestedTypeReference.Name, this.host.InternFactory, nestedTypeReference);
      }
      return nestedTypeReference;
    }

    /// <summary>
    /// Visits the specified generic method parameter reference.
    /// </summary>
    /// <param name="genericMethodParameterReference">The generic method parameter reference.</param>
    /// <returns></returns>
    public virtual IGenericMethodParameterReference Visit(IGenericMethodParameterReference genericMethodParameterReference) {
      if (this.stopTraversal) return genericMethodParameterReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(genericMethodParameterReference, out cachedValue)) {
        return (IGenericMethodParameterReference)cachedValue;
      }
      var mutable = genericMethodParameterReference as GenericMethodParameterReference;
      if (mutable != null) {
        var copy = this.Mutate(mutable);
        this.referenceCache.Add(genericMethodParameterReference, copy);
        if (copy != genericMethodParameterReference) {
          this.referenceCache.Add(copy, copy);
        }
        return copy;
      }
      return genericMethodParameterReference;
    }

    /// <summary>
    /// Visit a generic method parameter reference. 
    /// </summary>
    /// <param name="genericMethodParameterReference"></param>
    /// <returns></returns>
    public virtual GenericMethodParameterReference Mutate(GenericMethodParameterReference genericMethodParameterReference) {
      if (this.stopTraversal) return genericMethodParameterReference;
      Debug.Assert(!this.referenceCache.ContainsKey(genericMethodParameterReference));
      var definingMethod = this.GetTypeSpecificReferenceCopy(genericMethodParameterReference.DefiningMethod);
      if (definingMethod != genericMethodParameterReference.DefiningMethod) {
        var copy = new GenericMethodParameterReference();
        copy.Copy(genericMethodParameterReference, this.host.InternFactory);
        copy.DefiningMethod = definingMethod;
        return copy;
      }
      return genericMethodParameterReference;
    }

    /// <summary>
    /// Get a type specific reference copy of a method reference. 
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    private IMethodReference GetTypeSpecificReferenceCopy(IMethodReference methodReference) {
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(methodReference, out cachedValue)) {
        return (IMethodReference)cachedValue;
      }
      ISpecializedMethodReference specializedMethodReference = methodReference as ISpecializedMethodReference;
      if (specializedMethodReference != null)
        return this.GetReferenceCopy(specializedMethodReference);
      IGenericMethodInstanceReference genericMethodInstanceReference = methodReference as IGenericMethodInstanceReference;
      if (genericMethodInstanceReference != null)
        return this.GetReferenceCopy(genericMethodInstanceReference);
      return this.GetReferenceCopy(methodReference);
    }

    /// <summary>
    /// Visits the specified array type reference.
    /// </summary>
    /// <param name="arrayTypeReference">The array type reference.</param>
    /// <returns></returns>
    /// <remarks>Array types are not nominal types, so always visit the reference, even if it is a definition.</remarks>
    public virtual IArrayTypeReference Visit(IArrayTypeReference arrayTypeReference) {
      if (this.stopTraversal) return arrayTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(arrayTypeReference, out cachedValue)) {
        return (IArrayTypeReference)cachedValue;
      }
      if (arrayTypeReference.IsVector) {
        VectorTypeReference vectorTypeReference = arrayTypeReference as VectorTypeReference;
        if (vectorTypeReference != null) {
          var copy = this.Mutate(vectorTypeReference);
          this.referenceCache.Add(arrayTypeReference, copy);
          if (copy != arrayTypeReference)
            this.referenceCache.Add(copy, copy);
          return copy;
        }
      } else {
        MatrixTypeReference matrixTypeReference = arrayTypeReference as MatrixTypeReference;
        if (matrixTypeReference != null) {
          var copy = this.Mutate(matrixTypeReference);
          this.referenceCache.Add(matrixTypeReference, copy);
          if (matrixTypeReference != copy)
            this.referenceCache.Add(copy, copy);
          return copy;
        }
      }
      if (this.visitImmutableNodes) {
        this.VisitTypeReference(arrayTypeReference);
        this.path.Push(arrayTypeReference);
        this.Visit(arrayTypeReference.ElementType);
        this.path.Pop();
      }
      return arrayTypeReference;
    }

    /// <summary>
    /// Mutate a VectorTypeRefence.
    /// </summary>
    /// <param name="vectorType"></param>
    /// <returns></returns>
    public virtual IArrayTypeReference Mutate(VectorTypeReference vectorType) {
      if (this.stopTraversal) return vectorType;
      var t = this.Visit(vectorType.ElementType);
      if (t != vectorType.ElementType) {
        var copy = new VectorTypeReference();
        copy.Copy(vectorType, this.host.InternFactory);
        copy.ElementType = t;
        return copy;
      }
      return vectorType;
    }

    /// <summary>
    /// Mutate a matrix type reference.
    /// </summary>
    /// <param name="matrixType"></param>
    /// <returns></returns>
    public virtual IArrayTypeReference Mutate(MatrixTypeReference matrixType) {
      if (this.stopTraversal) return matrixType;
      var t = this.Visit(matrixType.ElementType);
      if (t != matrixType.ElementType) {
        var copy = new MatrixTypeReference();
        copy.Copy(matrixType, this.host.InternFactory);
        copy.ElementType = t;
        return copy;
      }
      return matrixType;
    }

    /// <summary>
    /// Visits the specified generic type parameter reference.
    /// </summary>
    /// <param name="genericTypeParameterReference">The generic type parameter reference.</param>
    /// <returns></returns>
    public virtual IGenericTypeParameterReference Visit(IGenericTypeParameterReference genericTypeParameterReference) {
      if (this.stopTraversal) return genericTypeParameterReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(genericTypeParameterReference, out cachedValue)) {
        return (IGenericTypeParameterReference)cachedValue;
      }
      var copy = this.Mutate(genericTypeParameterReference);
      if (copy != genericTypeParameterReference)
        this.referenceCache.Add(copy, copy);
      this.referenceCache.Add(genericTypeParameterReference, copy);
      return copy;
    }

    /// <summary>
    /// Mutate an IGenericTypeParameterReference.
    /// </summary>
    /// <param name="genericTypeParameterReference"></param>
    /// <returns></returns>
    public virtual IGenericTypeParameterReference Mutate(IGenericTypeParameterReference genericTypeParameterReference) {
      //^ requires !this.referenceCache.ContainsKey(genericTypeParameterReference);
      //^ ensures !this.referenceCache.ContainsKey(genericTypeParameterReference);
      var defType = this.Visit(genericTypeParameterReference.DefiningType);
      if (defType != genericTypeParameterReference.DefiningType) {
        return MutableModelHelper.GetGenericTypeParameterReference(defType, genericTypeParameterReference.Name, genericTypeParameterReference.Index, this.host.InternFactory, genericTypeParameterReference);
      }
      return genericTypeParameterReference;
    }
    /// <summary>
    /// Visits the specified generic type instance reference.
    /// </summary>
    /// <param name="genericTypeInstanceReference">The generic type instance reference.</param>
    /// <returns></returns>
    public virtual IGenericTypeInstanceReference Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
      if (this.stopTraversal) return genericTypeInstanceReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(genericTypeInstanceReference, out cachedValue)) {
        return (IGenericTypeInstanceReference)cachedValue;
      }
      var copy = this.Mutate(genericTypeInstanceReference);
      this.referenceCache.Add(genericTypeInstanceReference, copy);
      if (copy != genericTypeInstanceReference)
        this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Mutate an IGenericTypeInstanceReference.
    /// </summary>
    /// <param name="genericTypeInstanceReference"></param>
    /// <returns></returns>
    public virtual IGenericTypeInstanceReference Mutate(IGenericTypeInstanceReference genericTypeInstanceReference) {
      //^ requires !this.referenceCache.ContainsKey(genericTypeInstanceReference);
      //^ ensures !this.referenceCache.ContainsKey(genericTypeInstanceReference);
      if (this.stopTraversal) return genericTypeInstanceReference;
      var args = this.Mutate(genericTypeInstanceReference.GenericArguments);
      var typ = (INamedTypeReference)this.Visit(genericTypeInstanceReference.GenericType);
      if (args != genericTypeInstanceReference.GenericArguments || typ != genericTypeInstanceReference.GenericType) {
        return MutableModelHelper.GetGenericTypeInstanceReference(new List<ITypeReference>(args), typ, this.host.InternFactory, genericTypeInstanceReference);
      }
      return genericTypeInstanceReference;
    }

    /// <summary>
    /// Mutate a collection of types. 
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public virtual IEnumerable<ITypeReference> Mutate(IEnumerable<ITypeReference> types) {
      if (this.stopTraversal) return types;
      List<ITypeReference> result = new List<ITypeReference>();
      bool changed = false;
      foreach (ITypeReference typ in types) {
        var newTyp = this.Visit(typ);
        if (newTyp != typ) changed = true;
        result.Add(newTyp);
      }
      if (changed) return result;
      else return types;
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
    public virtual IPointerTypeReference Visit(IPointerTypeReference pointerTypeReference) {
      if (this.stopTraversal) return pointerTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(pointerTypeReference, out cachedValue)) {
        return (IPointerTypeReference)cachedValue;
      }
      var copy = this.Mutate(pointerTypeReference);
      if (pointerTypeReference != copy)
        this.referenceCache.Add(copy, copy);
      this.referenceCache.Add(pointerTypeReference, copy);
      return copy;
    }

    /// <summary>
    /// Mutate an IPointerTypeReference.
    /// </summary>
    /// <param name="pointerTypeReference"></param>
    /// <returns></returns>
    public virtual IPointerTypeReference Mutate(IPointerTypeReference pointerTypeReference) {
      //^ requires !this.referenceCache.ContainsKey(pointerTypeReference);
      //^ ensures !this.referenceCache.ContainsKey(pointerTypeReference);
      if (this.stopTraversal) return pointerTypeReference;
      var pointeeType = this.Visit(pointerTypeReference.TargetType);
      if (pointeeType != pointerTypeReference.TargetType) {
        var copy = MutableModelHelper.GetPointerTypeReference(pointeeType, this.host.InternFactory, pointerTypeReference);
        return copy;
      }
      return pointerTypeReference;
    }
    /// <summary>
    /// Visits the specified function pointer type reference.
    /// </summary>
    /// <param name="functionPointerTypeReference">The function pointer type reference.</param>
    /// <returns></returns>
    public virtual IFunctionPointerTypeReference Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
      if (this.stopTraversal) return functionPointerTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(functionPointerTypeReference, out cachedValue)) {
        return (IFunctionPointerTypeReference)cachedValue;
      }
      var copy = this.Mutate(functionPointerTypeReference);
      this.referenceCache.Add(functionPointerTypeReference, copy);
      if (copy != functionPointerTypeReference)
        this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Mutate a collection of modifiers. 
    /// </summary>
    /// <param name="modifiers"></param>
    /// <returns></returns>
    public IEnumerable<ICustomModifier> Mutate(IEnumerable<ICustomModifier> modifiers) {
      if (this.stopTraversal) return modifiers;
      var result = new List<ICustomModifier>();
      bool changed = false;
      foreach (var mod in modifiers) {
        var newmod = this.Visit(mod);
        result.Add(newmod);
        if (newmod != mod) changed = true;
      }
      if (changed) return result;
      return modifiers;
    }

    /// <summary>
    /// Mutate an IFunctionPointerTypeReference. 
    /// </summary>
    /// <param name="functionPointerTypeReference"></param>
    /// <returns></returns>
    public virtual IFunctionPointerTypeReference Mutate(IFunctionPointerTypeReference functionPointerTypeReference) {
      //^ requires !this.referenceCache.ContainsKey(functionPointerTypeReference);
      //^ ensures !this.referenceCache.ContainsKey(functionPointerTypeReference);
      if (this.stopTraversal) return functionPointerTypeReference;
      var typ = this.Visit(functionPointerTypeReference.Type);
      var pars = this.Mutate(functionPointerTypeReference.Parameters);
      var extraArgumentTypes = this.Mutate(functionPointerTypeReference.ExtraArgumentTypes);
      IEnumerable<ICustomModifier> customModifiers;
      if (functionPointerTypeReference.ReturnValueIsModified)
        customModifiers = this.Mutate(functionPointerTypeReference.ReturnValueCustomModifiers);
      else
        customModifiers = new List<ICustomModifier>(0);
      if (pars != functionPointerTypeReference.Parameters || typ != functionPointerTypeReference.Type
        || extraArgumentTypes != functionPointerTypeReference.ExtraArgumentTypes || (customModifiers != functionPointerTypeReference.ReturnValueCustomModifiers && functionPointerTypeReference.ReturnValueIsModified)
        ) {
        var copy = MutableModelHelper.GetFunctionPointerTypeReference(functionPointerTypeReference.CallingConvention, pars,
          extraArgumentTypes, functionPointerTypeReference.ReturnValueIsModified, customModifiers,
          functionPointerTypeReference.ReturnValueIsByRef, typ, this.host.InternFactory, functionPointerTypeReference);
        return copy;
      }
      return functionPointerTypeReference;
    }

    /// <summary>
    /// Mutate a collection of IParameterTypeInformation.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public virtual IEnumerable<IParameterTypeInformation>/*?*/ Mutate(IEnumerable<IParameterTypeInformation>/*?*/ parameters) {
      if (this.stopTraversal || parameters == null) return parameters;
      var result = new List<IParameterTypeInformation>();
      bool changed = false;
      foreach (IParameterTypeInformation par in parameters) {
        var newp = this.Visit(par);
        if (newp != par) changed = true;
        result.Add(newp);
      }
      if (changed)
        return result;
      return parameters;
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
    public virtual IManagedPointerTypeReference Visit(IManagedPointerTypeReference managedPointerTypeReference) {
      if (this.stopTraversal) return managedPointerTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(managedPointerTypeReference, out cachedValue)) {
        return (IManagedPointerTypeReference)cachedValue;
      }
      var copy = this.Mutate(managedPointerTypeReference);
      this.referenceCache.Add(managedPointerTypeReference, copy);
      if (managedPointerTypeReference != copy)
        this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Mutate an IManagedPointerTypeReference.
    /// </summary>
    /// <param name="managedPointerTypeReference"></param>
    /// <returns></returns>
    public virtual IManagedPointerTypeReference Mutate(IManagedPointerTypeReference managedPointerTypeReference) {
      //^ requires !this.referenceCache.ContainsKey(managedPointerTypeReference);
      //^ ensures !this.referenceCache.ContainsKey(managedPointerTypeReference);
      if (this.stopTraversal) return managedPointerTypeReference;
      var et = this.Visit(managedPointerTypeReference.TargetType);
      if (et != managedPointerTypeReference.TargetType) {
        return MutableModelHelper.GetManagedPointerTypeReference(et, this.host.InternFactory, managedPointerTypeReference);
      }
      return managedPointerTypeReference;
    }

    /// <summary>
    /// Visits the specified modified type reference.
    /// </summary>
    /// <param name="modifiedTypeReference">The modified type reference.</param>
    /// <returns></returns>
    public virtual IModifiedTypeReference Visit(IModifiedTypeReference modifiedTypeReference) {
      if (this.stopTraversal) return modifiedTypeReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(modifiedTypeReference, out cachedValue)) {
        return (IModifiedTypeReference)modifiedTypeReference;
      }
      var result = this.Mutate(modifiedTypeReference);
      this.referenceCache.Add(modifiedTypeReference, result);
      if (modifiedTypeReference != result)
        this.referenceCache.Add(result, result);
      return result;
    }

    /// <summary>
    /// Mutate an IModifiedTypeReference.
    /// </summary>
    /// <param name="modifiedTypeReference"></param>
    /// <returns></returns>
    public virtual IModifiedTypeReference Mutate(IModifiedTypeReference modifiedTypeReference) {
      //^ requires !this.referenceCache.ContainsKey(modifiedTypeReference);
      //^ ensures !this.referenceCache.ContainsKey(modifiedTypeReference);
      if (this.stopTraversal) return modifiedTypeReference;
      var umt = this.Visit(modifiedTypeReference.UnmodifiedType);
      var customModifiers = this.Mutate(modifiedTypeReference.CustomModifiers);
      if (umt != modifiedTypeReference.UnmodifiedType || customModifiers != modifiedTypeReference.CustomModifiers) {
        return MutableModelHelper.GetModifiedTypeReference(umt, customModifiers, this.host.InternFactory, modifiedTypeReference);
      }
      return modifiedTypeReference;
    }

    /// <summary>
    /// Visits the specified module reference.
    /// </summary>
    /// <param name="moduleReference">The module reference.</param>
    /// <returns></returns>
    public virtual IModuleReference Visit(IModuleReference moduleReference) {
      if (this.stopTraversal) return moduleReference;
      var mutable = moduleReference as ModuleReference;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        // do nothing. 
      }
      return moduleReference;
    }

    /// <summary>
    /// Visits the specified namespace type definition.
    /// </summary>
    /// <param name="namespaceTypeDefinition">The namespace type definition.</param>
    /// <returns></returns>
    public virtual INamespaceTypeDefinition Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
      if (this.stopTraversal) return namespaceTypeDefinition;
      var mutable = namespaceTypeDefinition as NamespaceTypeDefinition;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(namespaceTypeDefinition);
        this.Visit(namespaceTypeDefinition.Attributes);
        this.Visit(namespaceTypeDefinition.BaseClasses);
        this.Visit(namespaceTypeDefinition.ExplicitImplementationOverrides);
        this.Visit(namespaceTypeDefinition.GenericParameters);
        this.Visit(namespaceTypeDefinition.Interfaces);
        this.Visit(namespaceTypeDefinition.Locations);
        this.Visit(namespaceTypeDefinition.Events);
        this.Visit(namespaceTypeDefinition.Fields);
        this.Visit(namespaceTypeDefinition.Methods);
        this.Visit(namespaceTypeDefinition.NestedTypes);
        this.Visit(namespaceTypeDefinition.Properties);
        if (namespaceTypeDefinition.HasDeclarativeSecurity)
          this.Visit(namespaceTypeDefinition.SecurityAttributes);
        if (namespaceTypeDefinition.IsEnum)
          this.Visit(namespaceTypeDefinition.UnderlyingType);
        this.path.Pop();
      }
      return namespaceTypeDefinition;
    }

    /// <summary>
    /// Visits the specified nested type definition.
    /// </summary>
    /// <param name="nestedTypeDefinition">The nested type definition.</param>
    /// <returns></returns>
    public virtual INestedTypeDefinition Visit(INestedTypeDefinition nestedTypeDefinition) {
      if (this.stopTraversal) return nestedTypeDefinition;
      var mutable = nestedTypeDefinition as NestedTypeDefinition;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(nestedTypeDefinition);
        this.Visit(nestedTypeDefinition.Attributes);
        this.Visit(nestedTypeDefinition.BaseClasses);
        this.Visit(nestedTypeDefinition.ExplicitImplementationOverrides);
        this.Visit(nestedTypeDefinition.GenericParameters);
        this.Visit(nestedTypeDefinition.Interfaces);
        this.Visit(nestedTypeDefinition.Locations);
        this.Visit(nestedTypeDefinition.Events);
        this.Visit(nestedTypeDefinition.Fields);
        this.Visit(nestedTypeDefinition.Methods);
        this.Visit(nestedTypeDefinition.NestedTypes);
        this.Visit(nestedTypeDefinition.Properties);
        if (nestedTypeDefinition.HasDeclarativeSecurity)
          this.Visit(nestedTypeDefinition.SecurityAttributes);
        if (nestedTypeDefinition.IsEnum)
          this.Visit(nestedTypeDefinition.UnderlyingType);
        this.path.Pop();
      }
      return nestedTypeDefinition;
    }

    /// <summary>
    /// Visits the specified type reference.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns></returns>
    public virtual ITypeReference Visit(ITypeReference typeReference) {
      if (this.stopTraversal) return typeReference;
      INamespaceTypeReference/*?*/ namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference != null)
        return this.Visit(namespaceTypeReference);
      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      if (nestedTypeReference != null)
        return this.Visit(nestedTypeReference);
      IGenericMethodParameterReference/*?*/ genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
      if (genericMethodParameterReference != null)
        return this.Visit(genericMethodParameterReference);
      IArrayTypeReference/*?*/ arrayTypeReference = typeReference as IArrayTypeReference;
      if (arrayTypeReference != null)
        return this.Visit(arrayTypeReference);
      IGenericTypeParameterReference/*?*/ genericTypeParameterReference = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterReference != null)
        return this.Visit(genericTypeParameterReference);
      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null)
        return this.Visit(genericTypeInstanceReference);
      IPointerTypeReference/*?*/ pointerTypeReference = typeReference as IPointerTypeReference;
      if (pointerTypeReference != null)
        return this.Visit(pointerTypeReference);
      IFunctionPointerTypeReference/*?*/ functionPointerTypeReference = typeReference as IFunctionPointerTypeReference;
      if (functionPointerTypeReference != null)
        return this.Visit(functionPointerTypeReference);
      IModifiedTypeReference/*?*/ modifiedTypeReference = typeReference as IModifiedTypeReference;
      if (modifiedTypeReference != null)
        return this.Visit(modifiedTypeReference);
      IManagedPointerTypeReference/*?*/ managedPointerTypeReference = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeReference != null)
        return this.Visit(managedPointerTypeReference);
      Contract.Assume(typeReference is Dummy);
      return typeReference;
    }

    /// <summary>
    /// Visits the specified unit namespace reference.
    /// </summary>
    /// <param name="unitNamespaceReference">The unit namespace reference.</param>
    /// <returns></returns>
    public virtual IUnitNamespaceReference Visit(IUnitNamespaceReference unitNamespaceReference) {
      if (this.stopTraversal) return unitNamespaceReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(unitNamespaceReference, out cachedValue)) {
        return (IUnitNamespaceReference)cachedValue;
      }
      IRootUnitNamespaceReference/*?*/ rootUnitNamespaceReference = unitNamespaceReference as IRootUnitNamespaceReference;
      if (rootUnitNamespaceReference != null)
        return this.Visit(rootUnitNamespaceReference);
      INestedUnitNamespaceReference/*?*/ nestedUnitNamespaceReference = unitNamespaceReference as INestedUnitNamespaceReference;
      if (nestedUnitNamespaceReference != null)
        return this.Visit(nestedUnitNamespaceReference);
      Debug.Assert(false);
      return unitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified nested unit namespace.
    /// </summary>
    /// <param name="nestedUnitNamespace">The nested unit namespace.</param>
    /// <returns></returns>
    public virtual INestedUnitNamespace Visit(INestedUnitNamespace nestedUnitNamespace) {
      if (this.stopTraversal) return nestedUnitNamespace;
      var mutable = nestedUnitNamespace as NestedUnitNamespace;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        if (this.stopTraversal) return nestedUnitNamespace;
        this.path.Push(nestedUnitNamespace);
        this.Visit(nestedUnitNamespace.Attributes);
        this.Visit(nestedUnitNamespace.Locations);
        this.Visit(nestedUnitNamespace.Members);
        this.GetCurrentUnit();
        this.path.Pop();
      }
      return nestedUnitNamespace;
    }

    /// <summary>
    /// Visits the specified nested unit namespace reference.
    /// </summary>
    /// <param name="nestedUnitNamespaceReference">The nested unit namespace reference.</param>
    /// <returns></returns>
    public virtual INestedUnitNamespaceReference Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      if (this.stopTraversal) return nestedUnitNamespaceReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(nestedUnitNamespaceReference, out cachedValue)) {
        return (INestedUnitNamespaceReference)cachedValue;
      }
      return this.Mutate(nestedUnitNamespaceReference);
    }

    /// <summary>
    /// Visit a nested unit namespace reference. 
    /// </summary>
    /// <param name="nestedUnitNamespaceReference"></param>
    /// <returns></returns>
    public virtual INestedUnitNamespaceReference Mutate(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
      if (this.stopTraversal) return nestedUnitNamespaceReference;
      var t = this.Visit(nestedUnitNamespaceReference.ContainingUnitNamespace);
      if (t != nestedUnitNamespaceReference.ContainingUnitNamespace) {
        var copy = new NestedUnitNamespaceReference();
        copy.Copy(nestedUnitNamespaceReference, this.host.InternFactory);
        copy.ContainingUnitNamespace = t;
        this.referenceCache.Add(nestedUnitNamespaceReference, copy);
        this.referenceCache.Add(copy, copy);
        return copy;
      }
      return nestedUnitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified nested unit namespace.
    /// </summary>
    /// <param name="nestedUnitNamespace">The nested unit namespace.</param>
    /// <returns></returns>
    public virtual NestedUnitNamespace Mutate(NestedUnitNamespace nestedUnitNamespace) {
      if (this.stopTraversal) return nestedUnitNamespace;
      this.Mutate((UnitNamespace)nestedUnitNamespace);
      var n = this.GetCurrentNamespace();
      if (!(n is Dummy)) {
        nestedUnitNamespace.ContainingUnitNamespace = n;
      }
      return nestedUnitNamespace;
    }

    /// <summary>
    /// Visits the specified unit reference.
    /// </summary>
    /// <param name="unitReference">The unit reference.</param>
    /// <returns></returns>
    public virtual IUnitReference Visit(IUnitReference unitReference) {
      if (this.stopTraversal) return unitReference;
      IAssemblyReference/*?*/ assemblyReference = unitReference as IAssemblyReference;
      if (assemblyReference != null)
        return this.Visit(assemblyReference);
      IModuleReference/*?*/ moduleReference = unitReference as IModuleReference;
      if (moduleReference != null)
        return this.Visit(moduleReference);
      //TODO: error
      return unitReference;
    }

    /// <summary>
    /// Visits the specified parameter definitions.
    /// </summary>
    /// <param name="parameterDefinitions">The parameter definitions.</param>
    /// <returns></returns>
    public virtual List<IParameterDefinition>/*?*/ Mutate(List<IParameterDefinition>/*?*/ parameterDefinitions) {
      if (this.stopTraversal || parameterDefinitions == null) return parameterDefinitions;
      for (int i = 0, n = parameterDefinitions.Count; i < n; i++)
        parameterDefinitions[i] = this.Visit(parameterDefinitions[i]);
      return parameterDefinitions;
    }

    /// <summary>
    /// Visit a parameter definition. 
    /// </summary>
    /// <param name="parameterDefinition"></param>
    /// <returns></returns>
    public virtual IParameterDefinition Visit(IParameterDefinition parameterDefinition) {
      if (this.stopTraversal) return parameterDefinition;
      var mutable = parameterDefinition as ParameterDefinition;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(parameterDefinition);
        this.Visit(parameterDefinition.Attributes);
        if (parameterDefinition.HasDefaultValue)
          this.Visit(parameterDefinition.DefaultValue);
        if (parameterDefinition.IsModified)
          this.Visit(parameterDefinition.CustomModifiers);
        this.Visit(parameterDefinition.Locations);
        if (parameterDefinition.IsMarshalledExplicitly)
          this.Visit(parameterDefinition.MarshallingInformation);
        this.Visit(parameterDefinition.Type);
        this.path.Pop();
      }
      return parameterDefinition;
    }

    /// <summary>
    /// Visits the specified parameter definition.
    /// </summary>
    /// <param name="parameterDefinition">The parameter definition.</param>
    /// <returns></returns>
    public virtual ParameterDefinition Mutate(ParameterDefinition parameterDefinition) {
      if (this.stopTraversal) return parameterDefinition;
      this.path.Push(parameterDefinition);
      parameterDefinition.Attributes = this.Mutate(parameterDefinition.Attributes);
      var s = this.GetCurrentSignature();
      if (!(s is Dummy)) {
        parameterDefinition.ContainingSignature = s;
      }
      if (parameterDefinition.HasDefaultValue)
        parameterDefinition.DefaultValue = (IMetadataConstant)this.Visit(parameterDefinition.DefaultValue);
      if (parameterDefinition.IsModified)
        parameterDefinition.CustomModifiers = this.Mutate(parameterDefinition.CustomModifiers);
      parameterDefinition.Locations = this.Mutate(parameterDefinition.Locations);
      if (parameterDefinition.IsMarshalledExplicitly)
        parameterDefinition.MarshallingInformation = this.Visit(parameterDefinition.MarshallingInformation);
      parameterDefinition.Type = this.Visit(parameterDefinition.Type);
      this.path.Pop();
      return parameterDefinition;
    }

    /// <summary>
    /// Visits a parameter definition that is being referenced.
    /// </summary>
    /// <param name="parameterDefinition">The referenced parameter definition.</param>
    public virtual IParameterDefinition VisitReferenceTo(IParameterDefinition parameterDefinition) {
      IReference/*?*/ cachedValue;
      this.referenceCache.TryGetValue(parameterDefinition, out cachedValue);
      var cachedParameter = cachedValue as IParameterDefinition;
      return cachedParameter != null ? cachedParameter : parameterDefinition;
    }

    /// <summary>
    /// Visits the specified parameter type information list.
    /// </summary>
    /// <param name="parameterTypeInformationList">The parameter type information list.</param>
    /// <returns></returns>
    public virtual List<IParameterTypeInformation>/*?*/ Mutate(List<IParameterTypeInformation>/*?*/ parameterTypeInformationList) {
      if (this.stopTraversal || parameterTypeInformationList == null) return parameterTypeInformationList;
      for (int i = 0, n = parameterTypeInformationList.Count; i < n; i++)
        parameterTypeInformationList[i] = this.Visit(parameterTypeInformationList[i]);
      return parameterTypeInformationList;
    }

    /// <summary>
    /// Visit a parameter type information.
    /// </summary>
    /// <param name="parameterTypeInformation"></param>
    /// <returns></returns>
    public virtual IParameterTypeInformation Visit(IParameterTypeInformation parameterTypeInformation) {
      if (this.stopTraversal) return parameterTypeInformation;
      var mutable = parameterTypeInformation as ParameterTypeInformation;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(parameterTypeInformation);
        if (parameterTypeInformation.IsModified)
          foreach (var modifier in parameterTypeInformation.CustomModifiers)
            this.Visit(modifier);
        this.Visit(parameterTypeInformation.Type);
        this.path.Pop();
      }
      return parameterTypeInformation;
    }

    /// <summary>
    /// Visits the specified parameter type information.
    /// </summary>
    /// <param name="parameterTypeInformation">The parameter type information.</param>
    /// <returns></returns>
    public virtual ParameterTypeInformation Mutate(ParameterTypeInformation parameterTypeInformation) {
      if (this.stopTraversal) return parameterTypeInformation;
      this.path.Push(parameterTypeInformation);
      if (parameterTypeInformation.IsModified)
        parameterTypeInformation.CustomModifiers = this.Mutate(parameterTypeInformation.CustomModifiers);
      parameterTypeInformation.Type = this.Visit(parameterTypeInformation.Type);
      this.path.Pop();
      return parameterTypeInformation;
    }

    /// <summary>
    /// Visits the (interface) IPESection. We first see if it is a mutable model node, if so, we 
    /// visit the mutable node. If not, we just return the section.
    /// </summary>
    /// <param name="peSection"></param>
    /// <returns></returns>
    public virtual IPESection Visit(IPESection peSection) {
      if (this.stopTraversal) return peSection;
      var mutable = peSection as PESection;
      if (mutable != null) return this.Mutate(mutable);
      return peSection;
    }

    /// <summary>
    /// Visits a mutable PE section.
    /// </summary>
    /// <param name="peSection">The PE section.</param>
    /// <returns></returns>
    public virtual PESection Mutate(PESection peSection) {
      return peSection;
    }

    /// <summary>
    /// Visit p/invoke information.
    /// </summary>
    /// <param name="platformInvokeInformation"></param>
    /// <returns></returns>
    public virtual IPlatformInvokeInformation Visit(IPlatformInvokeInformation platformInvokeInformation) {
      if (this.stopTraversal) return platformInvokeInformation;
      var mutable = platformInvokeInformation as PlatformInvokeInformation;
      if (mutable != null) {
        return this.Mutate(mutable);
      }
      if (this.visitImmutableNodes) {
        this.path.Push(platformInvokeInformation);
        this.Visit(platformInvokeInformation.ImportModule);
        this.path.Pop();
      }
      return platformInvokeInformation;
    }


    /// <summary>
    /// Visits the specified platform invoke information.
    /// </summary>
    /// <param name="platformInvokeInformation">The platform invoke information.</param>
    /// <returns></returns>
    public virtual PlatformInvokeInformation Mutate(PlatformInvokeInformation platformInvokeInformation) {
      if (this.stopTraversal) return platformInvokeInformation;
      this.path.Push(platformInvokeInformation);
      platformInvokeInformation.ImportModule = this.Visit(platformInvokeInformation.ImportModule);
      this.path.Pop();
      return platformInvokeInformation;
    }

    /// <summary>
    /// Visits the specified property definitions.
    /// </summary>
    /// <param name="propertyDefinitions">The property definitions.</param>
    /// <returns></returns>
    public virtual List<IPropertyDefinition>/*?*/ Mutate(List<IPropertyDefinition>/*?*/ propertyDefinitions) {
      if (this.stopTraversal || propertyDefinitions == null) return propertyDefinitions;
      for (int i = 0, n = propertyDefinitions.Count; i < n; i++)
        propertyDefinitions[i] = this.Visit(propertyDefinitions[i]);
      return propertyDefinitions;
    }

    /// <summary>
    /// Visits the (interface) property definition. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns></returns>
    public virtual IPropertyDefinition Visit(IPropertyDefinition propertyDefinition) {
      if (this.stopTraversal) return propertyDefinition;
      PropertyDefinition mutable = propertyDefinition as PropertyDefinition;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.VisitITypeDefinitionMember(propertyDefinition);
        this.path.Push(propertyDefinition);
        this.Visit(propertyDefinition.Accessors);
        if (propertyDefinition.HasDefaultValue)
          this.Visit(propertyDefinition.DefaultValue);
        if (propertyDefinition.Getter != null)
          this.Visit(propertyDefinition.Getter);
        this.Visit(propertyDefinition.Parameters);
        if (propertyDefinition.ReturnValueIsModified)
          this.Visit(propertyDefinition.ReturnValueCustomModifiers);
        if (propertyDefinition.Setter != null)
          this.Visit(propertyDefinition.Setter);
        this.Visit(propertyDefinition.Type);
        this.path.Pop();
      }
      return propertyDefinition;
    }

    /// <summary>
    /// Visits the specified mutable property definition.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns></returns>
    public virtual PropertyDefinition Mutate(PropertyDefinition propertyDefinition) {
      if (this.stopTraversal) return propertyDefinition;
      this.VisitTypeDefinitionMember(propertyDefinition);
      this.path.Push(propertyDefinition);
      propertyDefinition.Accessors = this.Mutate(propertyDefinition.Accessors);
      if (propertyDefinition.HasDefaultValue)
        propertyDefinition.DefaultValue = (IMetadataConstant)this.Visit(propertyDefinition.DefaultValue);
      if (propertyDefinition.Getter != null)
        propertyDefinition.Getter = this.Visit(propertyDefinition.Getter);
      propertyDefinition.Parameters = this.Mutate(propertyDefinition.Parameters);
      if (propertyDefinition.ReturnValueIsModified)
        propertyDefinition.ReturnValueCustomModifiers = this.Mutate(propertyDefinition.ReturnValueCustomModifiers);
      if (propertyDefinition.Setter != null)
        propertyDefinition.Setter = this.Visit(propertyDefinition.Setter);
      propertyDefinition.Type = this.Visit(propertyDefinition.Type);
      this.path.Pop();
      return propertyDefinition;
    }

    /// <summary>
    /// Visits the specified resource references.
    /// </summary>
    /// <param name="resourceReferences">The resource references.</param>
    /// <returns></returns>
    public virtual List<IResourceReference>/*?*/ Mutate(List<IResourceReference>/*?*/ resourceReferences) {
      if (this.stopTraversal || resourceReferences == null) return resourceReferences;
      for (int i = 0, n = resourceReferences.Count; i < n; i++)
        resourceReferences[i] = this.Visit(resourceReferences[i]);
      return resourceReferences;
    }

    /// <summary>
    /// Visit resource reference. 
    /// </summary>
    /// <param name="resourceReference"></param>
    /// <returns></returns>
    public virtual IResourceReference Visit(IResourceReference resourceReference) {
      if (this.stopTraversal) return resourceReference;
      var mutable = resourceReference as ResourceReference;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.Visit(resourceReference.Attributes);
        this.Visit(resourceReference.DefiningAssembly);
      }
      return resourceReference;
    }

    /// <summary>
    /// Visits the specified resource reference. No copy is made. 
    /// </summary>
    /// <param name="resourceReference">The resource reference.</param>
    /// <returns></returns>
    public virtual ResourceReference Mutate(ResourceReference resourceReference) {
      if (this.stopTraversal) return resourceReference;
      resourceReference.Attributes = this.Mutate(resourceReference.Attributes);
      resourceReference.DefiningAssembly = this.Visit(resourceReference.DefiningAssembly);
      return resourceReference;
    }

    /// <summary>
    /// Visits the specified security attributes.
    /// </summary>
    /// <param name="securityAttributes">The security attributes.</param>
    /// <returns></returns>
    public virtual List<ISecurityAttribute>/*?*/ Mutate(List<ISecurityAttribute>/*?*/ securityAttributes) {
      if (this.stopTraversal || securityAttributes == null) return securityAttributes;
      for (int i = 0, n = securityAttributes.Count; i < n; i++)
        securityAttributes[i] = this.Visit(securityAttributes[i]);
      return securityAttributes;
    }

    /// <summary>
    /// Visits the interface root unit namespace reference. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// 
    /// Note that RootUnitNamespace also implements IRootUnitNamespaceReference.
    /// </summary>
    /// <param name="rootUnitNamespaceReference">The root unit namespace reference.</param>
    /// <returns></returns>
    public virtual IRootUnitNamespaceReference Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      if (this.stopTraversal) return rootUnitNamespaceReference;
      IReference cachedValue;
      if (this.referenceCache.TryGetValue(rootUnitNamespaceReference, out cachedValue)) {
        return (IRootUnitNamespaceReference)cachedValue;
      }
      IRootUnitNamespace/*?*/ rootUnitNamespace = rootUnitNamespaceReference as IRootUnitNamespace;
      if (rootUnitNamespace != null)
        return this.Mutate(this.GetReferenceCopy(rootUnitNamespace));
      var mutable = rootUnitNamespaceReference as RootUnitNamespaceReference;
      if (mutable != null) return this.Mutate(this.GetReferenceCopy(mutable));
      if (this.visitImmutableNodes) {
        if (this.stopTraversal) return rootUnitNamespaceReference;
        this.Visit(rootUnitNamespaceReference.Unit);
      }
      return rootUnitNamespaceReference;
    }

    /// <summary>
    /// Visits (interface) IRootUnitNamespace. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="rootUnitNamespace"></param>
    /// <returns></returns>
    public virtual IRootUnitNamespace Visit(IRootUnitNamespace rootUnitNamespace) {
      if (this.stopTraversal) return rootUnitNamespace;
      var mutable = rootUnitNamespace as RootUnitNamespace;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(rootUnitNamespace);
        this.Visit(rootUnitNamespace.Attributes);
        this.Visit(rootUnitNamespace.Locations);
        this.Visit(rootUnitNamespace.Members);
        this.path.Pop();
      }
      return rootUnitNamespace;
    }

    /// <summary>
    /// Visits the specified mutable root unit namespace.
    /// </summary>
    /// <param name="rootUnitNamespace">The root unit namespace.</param>
    /// <returns></returns>
    public virtual RootUnitNamespace Mutate(RootUnitNamespace rootUnitNamespace) {
      if (this.stopTraversal) return rootUnitNamespace;
      rootUnitNamespace.Unit = this.GetCurrentUnit();
      this.Mutate((UnitNamespace)rootUnitNamespace);
      return rootUnitNamespace;
    }

    /// <summary>
    /// Visits the specified mutable root unit namespace reference.
    /// </summary>
    /// <param name="rootUnitNamespaceReference">The root unit namespace reference.</param>
    /// <returns></returns>
    public virtual RootUnitNamespaceReference Mutate(RootUnitNamespaceReference rootUnitNamespaceReference) {
      if (this.stopTraversal) return rootUnitNamespaceReference;
      rootUnitNamespaceReference.Unit = this.Visit(rootUnitNamespaceReference.Unit);
      return rootUnitNamespaceReference;
    }

    /// <summary>
    /// Visits the (interface) ISecurityAttribute. We first see if it is a mutable model node, if so, we call the visit method
    /// on the mutable node. If not, we either return, or continue to visit the immutable subnodes, depending
    /// on the flag (this.visitImmutableNodes). 
    /// </summary>
    /// <param name="securityAttribute"></param>
    /// <returns></returns>
    public virtual ISecurityAttribute Visit(ISecurityAttribute securityAttribute) {
      if (this.stopTraversal) return securityAttribute;
      var mutable = securityAttribute as SecurityAttribute;
      if (mutable != null) return this.Mutate(mutable);
      if (this.visitImmutableNodes) {
        this.path.Push(securityAttribute);
        this.Visit(securityAttribute.Attributes);
        this.path.Pop();
      }
      return securityAttribute;
    }

    /// <summary>
    /// Visits the specified mutable security customAttribute.
    /// </summary>
    /// <param name="securityAttribute">The security customAttribute.</param>
    /// <returns></returns>
    public virtual SecurityAttribute Mutate(SecurityAttribute securityAttribute) {
      if (this.stopTraversal) return securityAttribute;
      this.path.Push(securityAttribute);
      securityAttribute.Attributes = this.Mutate(securityAttribute.Attributes);
      this.path.Pop();
      return securityAttribute;
    }

    /// <summary>
    /// Visits the interface section block. Simply return itself. 
    /// </summary>
    /// <param name="sectionBlock">The section block.</param>
    /// <returns></returns>
    public virtual ISectionBlock Visit(ISectionBlock sectionBlock) {
      return sectionBlock;
    }

    /// <summary>
    /// Visits the specified type definition member.
    /// </summary>
    /// <param name="typeDefinitionMember">The type definition member.</param>
    /// <returns></returns>
    public virtual TypeDefinitionMember VisitTypeDefinitionMember(TypeDefinitionMember typeDefinitionMember) {
      if (this.stopTraversal) return typeDefinitionMember;
      this.path.Push(typeDefinitionMember);
      typeDefinitionMember.Attributes = this.Mutate(typeDefinitionMember.Attributes);
      var t = this.GetCurrentType();
      if (!(t is Dummy)) {
        typeDefinitionMember.ContainingTypeDefinition = t;
      }
      typeDefinitionMember.Locations = this.Mutate(typeDefinitionMember.Locations);
      this.path.Pop();
      return typeDefinitionMember;
    }

    /// <summary>
    /// Visits the specified type reference.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <returns></returns>
    public virtual TypeReference VisitTypeReference(TypeReference typeReference) {
      if (this.stopTraversal) return typeReference;
      this.path.Push(typeReference);
      typeReference.Attributes = this.Mutate(typeReference.Attributes);
      typeReference.Locations = this.Mutate(typeReference.Locations);
      this.path.Pop();
      return typeReference;
    }

    /// <summary>
    /// Visit the common part of a type reference. 
    /// </summary>
    /// <param name="typeReference"></param>
    public virtual void VisitTypeReference(ITypeReference typeReference) {
      if (this.stopTraversal) return;
      this.path.Push(typeReference);
      this.Visit(typeReference.Attributes);
      this.Visit(typeReference.Locations);
      this.path.Pop();
    }

    /// <summary>
    /// Visits the specified unit.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <returns></returns>
    public virtual Unit Mutate(Unit unit) {
      if (this.stopTraversal) return unit;
      this.path.Push(unit);
      unit.Attributes = this.Mutate(unit.Attributes);
      unit.Locations = this.Mutate(unit.Locations);
      unit.UnitNamespaceRoot = this.Visit((IRootUnitNamespace)unit.UnitNamespaceRoot);
      this.path.Pop();
      return unit;
    }

    /// <summary>
    /// Visits the specified unit namespace.
    /// </summary>
    /// <param name="unitNamespace">The unit namespace.</param>
    /// <returns></returns>
    public virtual UnitNamespace Mutate(UnitNamespace unitNamespace) {
      if (this.stopTraversal) return unitNamespace;
      this.path.Push(unitNamespace);
      unitNamespace.Attributes = this.Mutate(unitNamespace.Attributes);
      unitNamespace.Locations = this.Mutate(unitNamespace.Locations);
      unitNamespace.Members = this.Mutate(unitNamespace.Members);
      this.path.Pop();
      return unitNamespace;
    }

    /// <summary>
    /// Visits the specified unit namespace reference. 
    /// </summary>
    /// <param name="unitNamespaceReference">The unit namespace reference.</param>
    /// <returns></returns>
    public virtual UnitNamespaceReference VisitUnitNamespaceReference(UnitNamespaceReference unitNamespaceReference) {
      if (this.stopTraversal) return unitNamespaceReference;
      this.path.Push(unitNamespaceReference);
      unitNamespaceReference.Attributes = this.Mutate(unitNamespaceReference.Attributes);
      unitNamespaceReference.Locations = this.Mutate(unitNamespaceReference.Locations);
      this.path.Pop();
      return unitNamespaceReference;
    }

    /// <summary>
    /// Visits the specified win32 resources.
    /// </summary>
    /// <param name="win32Resources">The win32 resources.</param>
    /// <returns></returns>
    public virtual List<IWin32Resource>/*?*/ Mutate(List<IWin32Resource>/*?*/ win32Resources) {
      if (this.stopTraversal || win32Resources == null) return win32Resources;
      for (int i = 0, n = win32Resources.Count; i < n; i++)
        win32Resources[i] = this.Visit(win32Resources[i]);
      return win32Resources;
    }

    /// <summary>
    /// Visits the specified win32 resource.
    /// </summary>
    /// <param name="win32Resource">The win32 resource.</param>
    /// <returns></returns>
    public virtual IWin32Resource Visit(IWin32Resource win32Resource) {
      return win32Resource;
    }

    /// <summary>
    /// Visits the method return value attributes.
    /// </summary>
    /// <param name="customAttributes">The custom attributes.</param>
    /// <returns></returns>
    public virtual List<ICustomAttribute>/*?*/ VisitMethodReturnValueAttributes(List<ICustomAttribute>/*?*/ customAttributes) {
      return this.Mutate(customAttributes);
    }

    /// <summary>
    /// Visits the method return value custom modifiers.
    /// </summary>
    /// <param name="customModifers">The custom modifers.</param>
    /// <returns></returns>
    public virtual List<ICustomModifier>/*?*/ VisitMethodReturnValueCustomModifiers(List<ICustomModifier>/*?*/ customModifers) {
      return this.Mutate(customModifers);
    }

    /// <summary>
    /// Visits the method return value marshalling information.
    /// </summary>
    /// <param name="marshallingInformation">The marshalling information.</param>
    /// <returns></returns>
    public virtual IMarshallingInformation VisitMethodReturnValueMarshallingInformation(MarshallingInformation marshallingInformation) {
      return this.Mutate(marshallingInformation);
    }
    #endregion

    #region VisitAsReference methods
    /// <summary>
    /// Create a copy for a field reference that is not specialized.
    /// </summary>
    /// <param name="fieldReference"></param>
    /// <returns></returns>
    private FieldReference GetReferenceCopy(IFieldReference fieldReference) {
      //^ requires fieldReference is FieldReference || fieldReference is FieldDefinition || fieldReference is GlobalFieldDefinition;
      //^ requires !this.referenceCache.Contains(fieldReference);
      var copy = new FieldReference();
      copy.Copy(fieldReference, this.host.InternFactory);
      this.referenceCache.Add(fieldReference, copy);
      this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Create a copy for a method reference that is not any specialized, instantiated method class.
    /// </summary>
    /// <param name="methodReference"></param>
    /// <returns></returns>
    private MethodReference GetReferenceCopy(IMethodReference methodReference) {
      //^ requires methodReference is MethodReference || methodReference is MethodDefinition || methodReference is GlobalMethodDefinition
      //^ requires !this.referenceCache.Contains(methodReference);
      var copy = new MethodReference();
      copy.Copy(methodReference, this.host.InternFactory);
      this.referenceCache.Add(methodReference, copy);
      this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Create a copy for root unit namespace reference. 
    /// </summary>
    /// <param name="rootUnitNamespaceReference"></param>
    /// <returns></returns>
    private RootUnitNamespaceReference GetReferenceCopy(IRootUnitNamespaceReference rootUnitNamespaceReference) {
      //^ requires !this.referenceCache.Contains(rootUnitNamespaceReference);
      //^ assert rootUnitNamespaceReference is RootUnitNamespace || rootUnitNamespaceReference is RootUnitNamespaceReference;
      var copy = new RootUnitNamespaceReference();
      copy.Copy(rootUnitNamespaceReference, this.host.InternFactory);
      this.referenceCache.Add(rootUnitNamespaceReference, copy);
      this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Create a copy for a specialized field reference.
    /// </summary>
    /// <param name="specializedFieldReference"></param>
    /// <returns></returns>
    private SpecializedFieldReference GetReferenceCopy(ISpecializedFieldReference specializedFieldReference) {
      //^ requires !this.referenceCache.Contains(specializedFieldReference);
      //^ assert specializedFieldReference is SpecializedFieldDefinition || specializedFieldReference is SpecializedFieldReference;
      var copy = new SpecializedFieldReference();
      copy.Copy(specializedFieldReference, this.host.InternFactory);
      this.referenceCache.Add(specializedFieldReference, copy);
      this.referenceCache.Add(copy, copy);
      return copy;

    }

    /// <summary>
    /// Create a copy for a specialized method reference.
    /// </summary>
    /// <param name="specializedMethodReference"></param>
    /// <returns></returns>
    private SpecializedMethodReference GetReferenceCopy(ISpecializedMethodReference specializedMethodReference) {
      //^ assert specialziedMethodReference is SpecializedMethodReference || specializedMethodReference is SpecializedMethodDefinition;
      //^ requires !this.referenceCache.Contains(genericMethodInstanceReference);
      var copy = new SpecializedMethodReference();
      copy.Copy(specializedMethodReference, this.host.InternFactory);
      this.referenceCache.Add(specializedMethodReference, copy);
      this.referenceCache.Add(copy, copy);
      return copy;
    }

    /// <summary>
    /// Create a copy for a generic method instance reference. 
    /// </summary>
    /// <param name="genericMethodInstanceReference"></param>
    /// <returns></returns>
    private GenericMethodInstanceReference GetReferenceCopy(IGenericMethodInstanceReference genericMethodInstanceReference) {
      //^ requires !this.referenceCache.Contains(genericMethodInstanceReference);
      //^ assert genericMethodInstanceReference is GenericMethodInstanceReference
      var copy = new GenericMethodInstanceReference();
      copy.Copy(genericMethodInstanceReference, this.host.InternFactory);
      this.referenceCache.Add(genericMethodInstanceReference, copy);
      this.referenceCache.Add(copy, copy);
      return copy;
    }
    #endregion
  }

  /// <summary>
  /// A helper class that provides functional versions of creating different type references. 
  /// </summary>
  public class MutableModelHelper {

    //TODO: need to add an IPlatformTypes parameter everywhere

    /// <summary>
    /// Functional version of creating a generic type instance reference. 
    /// </summary>
    /// <param name="genericArgs"></param>
    /// <param name="genericType"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly 
    /// created node. </param>
    /// <returns></returns>
    public static IGenericTypeInstanceReference GetGenericTypeInstanceReference(IEnumerable<ITypeReference> genericArgs, INamedTypeReference genericType, IInternFactory internFactory, ITypeReference/*?*/ original) {
      //^ ensures result is MutableModel.GenericTypeInstanceReference;
      var result = new GenericTypeInstanceReference();
      if (original != null)
        ((ICopyFrom<ITypeReference>)result).Copy(original, internFactory);
      result.InternFactory = internFactory;
      if (IteratorHelper.EnumerableIsNotEmpty(genericArgs))
        result.GenericArguments = new List<ITypeReference>(genericArgs);
      result.GenericType = genericType;
      return result;
    }

    /// <summary>
    /// Functional version of creating a generic type parameter reference. 
    /// </summary>
    /// <param name="definingType"></param>
    /// <param name="name"></param>
    /// <param name="index"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly
    /// created node.</param>
    /// <returns></returns>
    public static IGenericTypeParameterReference GetGenericTypeParameterReference(ITypeReference definingType, IName name, ushort index, IInternFactory internFactory, ITypeReference/*?*/ original) {
      //^ ensures result is MutableModel.GenericTypeParameterReference;
      var result = new GenericTypeParameterReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.InternFactory = internFactory;
      result.DefiningType = definingType;
      result.Name = name;
      result.Index = index;
      return result;
    }

    /// <summary>
    /// Functional version of creating a nested type reference. 
    /// </summary>
    /// <param name="containingType"></param>
    /// <param name="genericParameterCount"></param>
    /// <param name="mangleName"></param>
    /// <param name="name"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly
    /// created node.</param>
    /// <returns></returns>
    public static INestedTypeReference GetNestedTypeReference(ITypeReference containingType, ushort genericParameterCount, bool mangleName, IName name, IInternFactory internFactory, ITypeReference original) {
      //^ ensures result is MutableModel.NestedTypeReference;
      var result = new NestedTypeReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.InternFactory = internFactory;
      result.ContainingType = containingType;
      result.GenericParameterCount = genericParameterCount;
      result.MangleName = mangleName;
      result.Name = name;
      return result;
    }

    /// <summary>
    /// Functional version of creating a namespace type reference.
    /// </summary>
    /// <param name="containingUnitNamespace"></param>
    /// <param name="genericParameterCount"></param>
    /// <param name="mangleName"></param>
    /// <param name="name"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly
    /// created node.</param>
    /// <returns></returns>
    public static INamespaceTypeReference GetNamespaceTypeReference(IUnitNamespaceReference containingUnitNamespace, ushort genericParameterCount, bool mangleName, IName name, IInternFactory internFactory, ITypeReference original) {
      //^ ensures result is MutableModel.NamespaceTypeReference;
      var result = new NamespaceTypeReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.InternFactory = internFactory;
      result.ContainingUnitNamespace = containingUnitNamespace;
      result.GenericParameterCount = genericParameterCount;
      result.Name = name;
      result.MangleName = mangleName;
      return result;
    }

    /// <summary>
    /// Functional version of creating a specialized nested type reference.
    /// </summary>
    /// <param name="containingType"></param>
    /// <param name="genericParameterCount"></param>
    /// <param name="mangleName"></param>
    /// <param name="name"></param>
    /// <param name="unspecializedVersion"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly
    /// created node.</param>
    /// <returns></returns>
    public static ISpecializedNestedTypeReference GetSpecializedNestedTypeReference(ITypeReference containingType, ushort genericParameterCount, bool mangleName, IName name, INestedTypeReference unspecializedVersion, IInternFactory internFactory, ITypeReference original) {
      //^ ensures result is MutableModel.SpecializedNestedTypeReference;
      var result = new SpecializedNestedTypeReference();
      if (original != null) {
        result.Copy(original, internFactory);
      }
      result.InternFactory = internFactory;
      result.ContainingType = containingType;
      result.GenericParameterCount = genericParameterCount;
      result.MangleName = mangleName;
      result.Name = name;
      result.UnspecializedVersion = unspecializedVersion;
      return result;
    }

    /// <summary>
    /// Functional version of creating a pointer type reference.
    /// </summary>
    /// <param name="targetType"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly 
    /// created node.</param>
    /// <returns></returns>
    public static IPointerTypeReference GetPointerTypeReference(ITypeReference targetType, IInternFactory internFactory, ITypeReference original) {
      //^ ensures result is PointerTypeReference;
      var result = new PointerTypeReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.InternFactory = internFactory;
      result.TargetType = targetType;
      return result;
    }

    /// <summary>
    /// Functional version of creating a function pointer type reference. 
    /// </summary>
    /// <param name="callingConvention"></param>
    /// <param name="parameters"></param>
    /// <param name="extraArgumentTypes"></param>
    /// <param name="returnValueIsModified"></param>
    /// <param name="returnValueCustomModifiers"></param>
    /// <param name="returnValueIsByRef"></param>
    /// <param name="type"></param>
    /// <param name="internFactory"></param>
    /// <param name="original">If not null, contains auxiliary information such as attributes or locations, which we also copy to the newly 
    /// created node.</param>
    /// <returns></returns>
    public static IFunctionPointerTypeReference GetFunctionPointerTypeReference(CallingConvention callingConvention, IEnumerable<IParameterTypeInformation> parameters,
      IEnumerable<IParameterTypeInformation> extraArgumentTypes, bool returnValueIsModified, IEnumerable<ICustomModifier> returnValueCustomModifiers, bool returnValueIsByRef, ITypeReference type, IInternFactory internFactory,
      ITypeReference original) {
      //^ ensures result is MutableModel.FuntionPointerTypeReference;
      FunctionPointerTypeReference result = new FunctionPointerTypeReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.InternFactory = internFactory;
      result.CallingConvention =callingConvention;
      if (IteratorHelper.EnumerableIsNotEmpty(extraArgumentTypes))
        result.ExtraArgumentTypes = new List<IParameterTypeInformation>(extraArgumentTypes);
      else
        result.ExtraArgumentTypes = null;
      if (IteratorHelper.EnumerableIsNotEmpty(parameters))
        result.Parameters = new List<IParameterTypeInformation>(parameters);
      else
        result.Parameters = null;
      if (returnValueIsModified)
        result.ReturnValueCustomModifiers = new List<ICustomModifier>(returnValueCustomModifiers);
      else
        result.ReturnValueCustomModifiers = null;
      result.ReturnValueIsByRef = returnValueIsByRef;
      result.Type = type;
      return result;
    }

    /// <summary>
    /// Functional version of creating an IModifiedTypeReference using mutable model. 
    /// </summary>
    /// <param name="unmodifiedType"></param>
    /// <param name="modifiers"></param>
    /// <param name="internFactory"></param>
    /// <param name="original"></param>
    /// <returns></returns>
    public static IModifiedTypeReference GetModifiedTypeReference(ITypeReference unmodifiedType, IEnumerable<ICustomModifier> modifiers, IInternFactory internFactory, ITypeReference original) {
      //^ ensures result is MutableModel.ModifiedTypeReference;
      ModifiedTypeReference result = new ModifiedTypeReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.CustomModifiers = new List<ICustomModifier>(modifiers);
      result.UnmodifiedType = unmodifiedType;
      return result;
    }

    /// <summary>
    /// Functional version of creating an IManagedPointerTypeReference in mutable model. 
    /// </summary>
    /// <param name="pointeeType"></param>
    /// <param name="internFactory"></param>
    /// <param name="original"></param>
    /// <returns></returns>
    public static IManagedPointerTypeReference GetManagedPointerTypeReference(ITypeReference pointeeType, IInternFactory internFactory, ITypeReference original) {
      //^ ensures result is MutableModel.ManagedPointerTypeReference;
      ManagedPointerTypeReference result = new ManagedPointerTypeReference();
      if (original != null)
        result.Copy(original, internFactory);
      result.TargetType = pointeeType;
      return result;
    }
  }

}
