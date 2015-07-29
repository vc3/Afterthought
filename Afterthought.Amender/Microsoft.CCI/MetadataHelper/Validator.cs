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
using Microsoft.Cci;
using Microsoft.Cci.UtilityDataStructures;
using System.Diagnostics.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// A class that checks an object model node (and its children) for validity according to the rules of Partition II or the ECMA-335 Standard.
  /// </summary>
  public class MetadataValidator {

    /// <summary>
    /// A class that checks an object model node (and its children) for validity according to the rules of Partition II or the ECMA-335 Standard.
    /// </summary>
    /// <param name="host">A standard abstraction over the applications that host components that provide or consume objects from the metadata model.</param>
    public MetadataValidator(IMetadataHost host)
      : this(host, new ValidatingTraverser() { PreorderVisitor = new ValidatingVisitor(), TraverseIntoMethodBodies = true }) {
    }

    /// <summary>
    /// A class that checks an object model node (and its children) for validity according to the rules of Partition II or the ECMA-335 Standard.
    /// </summary>
    /// <param name="host">A standard abstraction over the applications that host components that provide or consume objects from the metadata model.</param>
    /// <param name="traverser">A traverser that will invoke a validating visitor on each node to be validated and that tracks the traversal path.</param>
    protected MetadataValidator(IMetadataHost host, ValidatingTraverser traverser) {
      this.host = host;
      this.traverser = traverser;
      traverser.validator = this;
      ((ValidatingVisitor)traverser.PreorderVisitor).validator = this;
    }

    /// <summary>
    /// The assembly that is being validated. May be null if a module is being validated.
    /// </summary>
    protected IAssembly/*?*/ currentAssembly;

    /// <summary>
    /// The current metadata definition that is being validated. For example a module, type or method.
    /// </summary>
    protected IDefinition currentDefinition;

    /// <summary>
    /// The current module that is being validated.
    /// </summary>
    protected IModule currentModule;

    /// <summary>
    /// The current security attribute that is being validated. May be null.
    /// </summary>
    protected ISecurityAttribute/*?*/ currentSecurityAttribute;

    /// <summary>
    /// An instance of IDocument that corresponds to this.currentModule and that is becomes part of any error messages.
    /// </summary>
    protected MetadataDocument document;

    /// <summary>
    /// An empty enumeration of related error locations.
    /// </summary>
    protected readonly static IEnumerable<ILocation> emptyLocations = Enumerable<ILocation>.Empty;

    /// <summary>
    /// A standard abstraction over the applications that host components that provide or consume objects from the metadata model.
    /// </summary>
    protected readonly IMetadataHost host;

    /// <summary>
    /// A traverser that will invoke a validating visitor on each node to be validated and that tracks the traversal path.
    /// </summary>
    ValidatingTraverser traverser;

    /// <summary>
    /// A visitor that checks each node it visits for validity according to the rules of Partition II or the ECMA-335 Standard.
    /// </summary>
    protected internal class ValidatingVisitor : IMetadataVisitor {

      /// <summary>
      /// The validator using this visitor.
      /// </summary>
      protected internal MetadataValidator validator;

      /// <summary>
      /// tracks definitions 
      /// </summary>
      protected readonly SetOfObjects definitionsAlreadyVisited = new SetOfObjects();

      /// <summary>
      /// 
      /// </summary>
      protected readonly SetOfObjects allTypes = new SetOfObjects();

      /// <summary>
      /// Visits the specified alias for type.
      /// </summary>
      public void Visit(IAliasForType aliasForType) {
        //We should only get here by traversing the ExportedTypes collection of an assembly.
        Contract.Assume(this.validator.currentAssembly != null);
        if (aliasForType.AliasedType is Dummy) {
          this.ReportError(MetadataError.IncompleteNode, aliasForType, "AliasedType");
          return;
        }
        var definingModule = TypeHelper.GetDefiningUnitReference(aliasForType.AliasedType) as IModuleReference;
        if (definingModule == null) {
          this.ReportError(MetadataError.IncompleteNode, aliasForType, "AliasedType");
          return;
        }
        if (definingModule.ContainingAssembly == null) {
          //an alias whose module reference is not an assembly, should use a module reference
          //to a member module of the current assembly. That reference is incomplete if it does
          //not have a containing assembly.
          this.ReportError(MetadataError.IncompleteNode, definingModule, "ContainingAssembly");
          return;
        }
        if (definingModule.ModuleIdentity.Equals(this.validator.currentModule.ModuleIdentity)) {
          this.ReportError(MetadataError.ExportedTypeBelongsToManifestModule, aliasForType, definingModule);
          return;
        }
        if (!(definingModule is IAssemblyReference) && //this is an alias for an exported type, not a forwarded type.
        !(definingModule.ContainingAssembly.AssemblyIdentity.Equals(this.validator.currentAssembly.AssemblyIdentity))) {
          this.ReportError(MetadataError.AliasedTypeDoesNotBelongToAModule, aliasForType, definingModule);
          return;
        }
        foreach (var aliasMember in aliasForType.Members) { //TODO: make the members element type be INestedAliasForType
          var nestedAlias = aliasMember as INestedAliasForType;
          if (nestedAlias == null)
            this.ReportError(MetadataError.UnexpectedAliasMember, aliasMember, aliasForType);
        }
      }

      /// <summary>
      /// Performs some computation with the given array type reference.
      /// </summary>
      public void Visit(IArrayTypeReference arrayTypeReference) {
        this.Visit((ITypeReference)arrayTypeReference);
        if (arrayTypeReference.ElementType is Dummy)
          this.ReportError(MetadataError.IncompleteNode, arrayTypeReference, "ElementType");
      }

      /// <summary>
      /// Performs some computation with the given assembly.
      /// </summary>
      public void Visit(IAssembly assembly) {
        this.Visit((IModule)assembly);
        if ((assembly.Flags & ~(0x0001|0x0100|0x4000|0x8000|0x0010|0x0020|0x0030|0x0040|0x0070|0x0080)) != 0)
          this.ReportError(MetadataError.UnknownAssemblyFlags, assembly);
        if (assembly.Name.Value.Length == 0)
          this.ReportError(MetadataError.EmptyName, assembly);
        //if (assembly.Name.Value.IndexOfAny(badAssemblyNameChars) > 0)
        if (assembly.Name.Value.IndexOfAny(badPosixNameChars) > 0)
          this.ReportError(MetadataError.NotPosixAssemblyName, assembly, assembly.Name.Value);
        if (assembly.Culture != string.Empty && validCultureNames.Find(x => string.Compare(assembly.Culture, x, StringComparison.OrdinalIgnoreCase) == 0) == null)
          this.ReportError(MetadataError.InvalidCulture, assembly, assembly.Culture);
        if (IteratorHelper.EnumerableIsEmpty(assembly.Files)) {
          foreach (var typeAlias in assembly.ExportedTypes) { //TODO: simplify this when type forwarders are put in their own collection.
            if (TypeHelper.GetDefiningUnitReference(typeAlias.AliasedType).UnitIdentity.Equals(assembly.AssemblyIdentity)) {
              this.ReportError(MetadataError.SingleFileAssemblyHasExportedTypes, assembly);
              break;
            }
          }
        } else {
          var fileTable = new Hashtable();
          foreach (var file in assembly.Files) {
            var key = (uint)file.FileName.UniqueKeyIgnoringCase;
            if (fileTable.Find(key) == 0)
              fileTable.Add(key, key);
            else
              this.ReportError(MetadataError.DuplicateFileReference, assembly, file);
          }
        }
        this.CheckResourcesForUniqueness(assembly);
      }

      private void CheckResourcesForUniqueness(IAssembly assembly) {
        Hashtable resourceTable = null;
        foreach (var resource in assembly.Resources) {
          if (resourceTable == null) resourceTable = new Hashtable();
          var key = (uint)resource.Name.UniqueKey;
          if (resourceTable.Find(key) != 0)
            this.ReportError(MetadataError.DuplicateResource, resource, assembly);
          else
            resourceTable.Add(key, key);
        }
      }

      /// <summary>
      /// Performs some computation with the given assembly reference.
      /// </summary>
      public void Visit(IAssemblyReference assemblyReference) {
        this.Visit((IModuleReference)assemblyReference);
        //if (assemblyReference.Name.Value.IndexOfAny(badAssemblyNameChars) > 0)
        if (assemblyReference.Name.Value.IndexOfAny(badPosixNameChars) > 0)
          this.ReportError(MetadataError.NotPosixAssemblyName, assemblyReference, assemblyReference.Name.Value);
        if (assemblyReference.Culture != string.Empty && validCultureNames.Find(x => string.Compare(assemblyReference.Culture, x, StringComparison.OrdinalIgnoreCase) == 0) == null)
          this.ReportError(MetadataError.InvalidCulture, assemblyReference, assemblyReference.Culture);
      }

      /// <summary>
      /// Performs some computation with the given custom attribute.
      /// </summary>
      public void Visit(ICustomAttribute customAttribute) {
        if (customAttribute.Constructor is Dummy)
          this.ReportError(MetadataError.IncompleteNode, customAttribute, "Constructor");
        else if (customAttribute.Type is Dummy)
          this.ReportError(MetadataError.IncompleteNode, customAttribute, "Type");
        else {
          if (!TypeHelper.TypesAreEquivalent(customAttribute.Constructor.ContainingType, customAttribute.Type))
            this.ReportError(MetadataError.CustomAttributeTypeIsNotConstructorContainer, customAttribute);
          if (!(customAttribute.Constructor.ResolvedMethod is Dummy)) {
            if (!customAttribute.Constructor.ResolvedMethod.IsConstructor)
              this.ReportError(MetadataError.CustomAttributeConstructorIsBadReference, customAttribute);
            if (!IteratorHelper.EnumerableHasLength(customAttribute.Arguments, customAttribute.Constructor.ResolvedMethod.ParameterCount)) {
              if (this.validator.currentSecurityAttribute == null || customAttribute.Constructor.ResolvedMethod.ParameterCount != 1 ||
                IteratorHelper.EnumerableIsNotEmpty(customAttribute.Arguments))
                this.ReportError(MetadataError.EnumerationCountIsInconsistentWithCountProperty, customAttribute, "Arguments");
            }
            //TODO: check that args match the types of the construtor param
          }
        }
        if (!IteratorHelper.EnumerableHasLength(customAttribute.NamedArguments, customAttribute.NumberOfNamedArguments))
          this.ReportError(MetadataError.EnumerationCountIsInconsistentWithCountProperty, customAttribute, "NamedArguments");
        //TODO: check that named args match the types of the attribute field/property.
      }

      /// <summary>
      /// Performs some computation with the given custom modifier.
      /// </summary>
      public void Visit(ICustomModifier customModifier) {
        if (customModifier.Modifier is Dummy)
          this.ReportError(MetadataError.IncompleteNode, customModifier, "Modifier");
        else if (!(customModifier.Modifier is INamedTypeReference))
          this.ReportError(MetadataError.InvalidCustomModifier, customModifier);
      }

      /// <summary>
      /// Performs some computation with the given event definition.
      /// </summary>
      public void Visit(IEventDefinition eventDefinition) {
        this.Visit((ITypeDefinitionMember)eventDefinition);

        if (eventDefinition.Adder is Dummy)
          this.ReportError(MetadataError.IncompleteNode, eventDefinition, "Adder");
        else if (!IteratorHelper.EnumerableContains(eventDefinition.Accessors, eventDefinition.Adder))
          this.ReportError(MetadataError.AccessorListInconsistent, eventDefinition, "Adder");
        if (eventDefinition.Caller != null)
          if (eventDefinition.Caller is Dummy)
            this.ReportError(MetadataError.IncompleteNode, eventDefinition, "Caller");
          else if (!IteratorHelper.EnumerableContains(eventDefinition.Accessors, eventDefinition.Caller))
            this.ReportError(MetadataError.AccessorListInconsistent, eventDefinition, "Caller");
        if (eventDefinition.Remover is Dummy)
          this.ReportError(MetadataError.IncompleteNode, eventDefinition, "Remover");
        else if (!IteratorHelper.EnumerableContains(eventDefinition.Accessors, eventDefinition.Remover))
          this.ReportError(MetadataError.AccessorListInconsistent, eventDefinition, "Remover");
        if (eventDefinition.IsRuntimeSpecial && !eventDefinition.IsSpecialName)
          this.ReportError(MetadataError.RuntimeSpecialMustAlsoBeSpecialName, eventDefinition);
        if (eventDefinition.Type != null) {
          if (eventDefinition.Type is Dummy)
            this.ReportError(MetadataError.IncompleteNode, eventDefinition, "Type");
          else {
            var etype = eventDefinition.Type.ResolvedType;
            if (!(etype is Dummy) && (etype.IsInterface || etype.IsValueType))
              this.ReportError(MetadataError.EventTypeMustBeClass, eventDefinition.Type, eventDefinition);
          }
        }
        foreach (var a in eventDefinition.Accessors) {
          // if an accessor meets the naming conventions, then it should be the accessor it claims to be
          var methodDefinition = a.ResolvedMethod;
          if (MemberHelper.IsAdder(methodDefinition) && a != eventDefinition.Adder)
            this.ReportError(MetadataError.EventPropertyNamingPatternWarning, methodDefinition);
          if (MemberHelper.IsRemover(methodDefinition) && a != eventDefinition.Remover)
            this.ReportError(MetadataError.EventPropertyNamingPatternWarning, methodDefinition);
          if (MemberHelper.IsCaller(methodDefinition) && a != eventDefinition.Caller)
            this.ReportError(MetadataError.EventPropertyNamingPatternWarning, methodDefinition);
        }
      }

      /// <summary>
      /// Performs some computation with the given field definition.
      /// </summary>
      public void Visit(IFieldDefinition fieldDefinition) {
        this.Visit((ITypeDefinitionMember)fieldDefinition);
        if (fieldDefinition.InternedKey == 0)
          this.ReportError(MetadataError.IncompleteNode, fieldDefinition, "InternedKey");
        if (fieldDefinition.Type is Dummy)
          this.ReportError(MetadataError.IncompleteNode, fieldDefinition, "Type");
        if (fieldDefinition.IsCompileTimeConstant) {
          if (fieldDefinition.IsReadOnly)
            this.ReportError(MetadataError.FieldMayNotBeConstantAndReadonly, fieldDefinition);
          if (!fieldDefinition.IsStatic)
            this.ReportError(MetadataError.ConstantFieldMustBeStatic, fieldDefinition);
          var fieldType = fieldDefinition.Type;
          if (fieldType.IsEnum && !(fieldType.ResolvedType is Dummy)) fieldType = fieldType.ResolvedType.UnderlyingType;
          if (CompileTimeConstantTypeDoesNotMatchDefinitionType(fieldDefinition.CompileTimeValue,  fieldType))
            this.ReportError(MetadataError.MetadataConstantTypeMismatch, fieldDefinition.CompileTimeValue, fieldDefinition);
        }
        if (fieldDefinition.IsRuntimeSpecial && !fieldDefinition.IsSpecialName)
          this.ReportError(MetadataError.RuntimeSpecialMustAlsoBeSpecialName, fieldDefinition);
        if (fieldDefinition.ContainingTypeDefinition.Layout == LayoutKind.Explicit) {
          if (fieldDefinition.Offset > 0 && fieldDefinition.IsStatic)
            this.ReportError(MetadataError.StaticFieldsMayNotHaveLayout, fieldDefinition);
          if (fieldDefinition.Type.TypeCode == PrimitiveTypeCode.NotPrimitive && (fieldDefinition.Offset % this.validator.host.PointerSize) != 0)
            this.ReportError(MetadataError.FieldOffsetNotNaturallyAlignedForObjectRef, fieldDefinition);
        }
        if (fieldDefinition.IsMapped) {
          if (!fieldDefinition.Type.IsValueType) //TODO: also check, recursively, that all fields are public and have types that do not reference heap objects.
            this.ReportError(MetadataError.MappedFieldDoesNotHaveAValidType, fieldDefinition);
        }
        if (fieldDefinition.IsMarshalledExplicitly) {
          if (fieldDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray) {
            if (fieldDefinition.MarshallingInformation.ParamIndex != null)
              this.ReportError(MetadataError.ArraysMarshalledToFieldsCannotSpecifyElementCountParameter, fieldDefinition.MarshallingInformation, fieldDefinition);
          }
          if (fieldDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray ||
            fieldDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray ||
            fieldDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValTStr) {
            if (fieldDefinition.MarshallingInformation.NumberOfElements == 0)
              this.ReportError(MetadataError.MarshalledArraysMustHaveSizeKnownAtCompileTime, fieldDefinition.MarshallingInformation, fieldDefinition);
          }
        }
      }

      /// <summary>
      /// Returns true if the given compile time constant does not match the type of the definition that it provides the initial value for.
      /// </summary>
      private static bool CompileTimeConstantTypeDoesNotMatchDefinitionType(IMetadataConstant compileTimeConstant, ITypeReference definitionType) {
        Contract.Requires(compileTimeConstant != null);
        Contract.Requires(definitionType != null);

        if (TypeHelper.TypesAreEquivalent(compileTimeConstant.Type, definitionType)) return false;
        if (definitionType.IsEnum || definitionType.ResolvedType.IsEnum)
          return CompileTimeConstantTypeDoesNotMatchDefinitionType(compileTimeConstant, definitionType.ResolvedType.UnderlyingType);
        if (compileTimeConstant.Value == null && TypeHelper.TypesAreEquivalent(compileTimeConstant.Type, compileTimeConstant.Type.PlatformType.SystemObject)) {
          if (definitionType.IsValueType || definitionType.ResolvedType.IsValueType) {
            var genericInstance = definitionType as IGenericTypeInstanceReference;
            if (genericInstance == null) return true;
            return !TypeHelper.TypesAreEquivalent(genericInstance.GenericType, definitionType.PlatformType.SystemNullable);
          }
          return false;
        }
        return true;
      }

      /// <summary>
      /// Performs some computation with the given field reference.
      /// </summary>
      public void Visit(IFieldReference fieldReference) {
        this.Visit((ITypeMemberReference)fieldReference);
        if (fieldReference.InternedKey == 0)
          this.ReportError(MetadataError.IncompleteNode, fieldReference, "InternedKey");
        if (fieldReference.Type is Dummy)
          this.ReportError(MetadataError.IncompleteNode, fieldReference, "Type");
        var resolvedField = fieldReference.ResolvedField;
        if (!(resolvedField is Dummy) && fieldReference.InternedKey != resolvedField.InternedKey)
          this.ReportError(MetadataError.FieldReferenceResolvesToDifferentField, fieldReference);
      }

      /// <summary>
      /// Performs some computation with the given file reference.
      /// </summary>
      public void Visit(IFileReference fileReference) {
        if (fileReference.FileName.Value.IndexOfAny(badPosixNameChars) > 0)
          this.ReportError(MetadataError.NotPosixAssemblyName, fileReference, fileReference.FileName.Value);
        if (fileReference.FileName.UniqueKeyIgnoringCase == this.validator.currentModule.ModuleName.UniqueKeyIgnoringCase)
          this.ReportError(MetadataError.SelfReference, fileReference);
      }

      /// <summary>
      /// Performs some computation with the given function pointer type reference.
      /// </summary>
      public void Visit(IFunctionPointerTypeReference functionPointerTypeReference) {
        this.Visit((ITypeReference)functionPointerTypeReference);
        if (functionPointerTypeReference.Type is Dummy)
          this.ReportError(MetadataError.IncompleteNode, functionPointerTypeReference, "Type");
        if ((functionPointerTypeReference.CallingConvention & CallingConvention.ExplicitThis) != 0 && functionPointerTypeReference.IsStatic)
          this.ReportError(MetadataError.MethodsCalledWithExplicitThisParametersMustNotBeStatic, functionPointerTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given generic method instance reference.
      /// </summary>
      public void Visit(IGenericMethodInstanceReference genericMethodInstanceReference) {
        this.Visit((IMethodReference)genericMethodInstanceReference);
      }

      /// <summary>
      /// Performs some computation with the given generic method parameter.
      /// </summary>
      public void Visit(IGenericMethodParameter genericMethodParameter) {
        this.Visit((IGenericParameter)genericMethodParameter);
      }

      /// <summary>
      /// Performs some computation with the given generic method parameter reference.
      /// </summary>
      public void Visit(IGenericMethodParameterReference genericMethodParameterReference) {
        this.Visit((IGenericParameterReference)genericMethodParameterReference);

      }

      /// <summary>
      /// Performs some computation with the given generic parameter.
      /// </summary>
      public void Visit(IGenericParameter genericParameter) {
        this.Visit((INamedTypeDefinition)genericParameter);
        Hashtable constraintTable = null;
        foreach (var constraint in genericParameter.Constraints) {
          if (constraint.TypeCode == PrimitiveTypeCode.Void)
            this.ReportError(MetadataError.ConstraintMayNotBeVoid, constraint, genericParameter);
          if (constraintTable == null) constraintTable = new Hashtable();
          var key = constraint.InternedKey;
          if (constraintTable.Find(key) == 0)
            constraintTable.Add(key, key);
          else
            this.ReportError(MetadataError.DuplicateConstraint, constraint, genericParameter);
        }
      }

      /// <summary>
      /// Performs some computation with the given generic parameter.
      /// </summary>
      public void Visit(IGenericParameterReference genericParameterReference) {
        var mfmv = this.validator.currentModule.MetadataFormatMajorVersion;
        if (mfmv < 2)
          this.ReportError(MetadataError.InvalidMetadataFormatVersionForGenerics, genericParameterReference, mfmv.ToString());
        this.Visit((ITypeReference)genericParameterReference);
      }

      /// <summary>
      /// Performs some computation with the given generic type instance reference.
      /// </summary>
      public void Visit(IGenericTypeInstanceReference genericTypeInstanceReference) {
        var mfmv = this.validator.currentModule.MetadataFormatMajorVersion;
        if (mfmv < 2)
          this.ReportError(MetadataError.InvalidMetadataFormatVersionForGenerics, genericTypeInstanceReference, mfmv.ToString());
        this.Visit((ITypeReference)genericTypeInstanceReference);
      }

      /// <summary>
      /// Performs some computation with the given generic parameter.
      /// </summary>
      public void Visit(IGenericTypeParameter genericTypeParameter) {
        this.Visit((IGenericParameter)genericTypeParameter);
      }

      /// <summary>
      /// Performs some computation with the given generic type parameter reference.
      /// </summary>
      public void Visit(IGenericTypeParameterReference genericTypeParameterReference) {
        this.Visit((IGenericParameterReference)genericTypeParameterReference);
      }

      /// <summary>
      /// Performs some computation with the given global field definition.
      /// </summary>
      public void Visit(IGlobalFieldDefinition globalFieldDefinition) {
        this.Visit((IFieldDefinition)globalFieldDefinition);
        switch (globalFieldDefinition.Visibility) {
          case TypeMemberVisibility.Public:
          case TypeMemberVisibility.Private:
          case TypeMemberVisibility.Other:
            break;
          default:
            this.ReportError(MetadataError.InvalidGlobalFieldVisibility, globalFieldDefinition);
            break;
        }
        if (!globalFieldDefinition.IsStatic)
          this.ReportError(MetadataError.GlobalFieldNotStatic, globalFieldDefinition);
      }

      /// <summary>
      /// Performs some computation with the given global method definition.
      /// </summary>
      public void Visit(IGlobalMethodDefinition globalMethodDefinition) {
        this.Visit((IMethodDefinition)globalMethodDefinition);
        switch (globalMethodDefinition.Visibility) {
          case TypeMemberVisibility.Public:
          case TypeMemberVisibility.Private:
          case TypeMemberVisibility.Other:
            break;
          default:
            this.ReportError(MetadataError.InvalidGlobalFieldVisibility, globalMethodDefinition);
            break;
        }
        if (!globalMethodDefinition.IsStatic)
          this.ReportError(MetadataError.GlobalFieldNotStatic, globalMethodDefinition);
      }

      /// <summary>
      /// Performs some computation with the given local definition.
      /// </summary>
      public void Visit(ILocalDefinition localDefinition) {
      }

      /// <summary>
      /// Performs some computation with the given local definition.
      /// </summary>
      public void VisitReference(ILocalDefinition localDefinition) {
      }

      /// <summary>
      /// Performs some computation with the given managed pointer type reference.
      /// </summary>
      public void Visit(IManagedPointerTypeReference managedPointerTypeReference) {
        this.Visit((ITypeReference)managedPointerTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given marshalling information.
      /// </summary>
      public void Visit(IMarshallingInformation marshallingInformation) {
        CheckUnmanagedType(marshallingInformation, (uint)marshallingInformation.UnmanagedType, "UnmanagedType");
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler) {
          if (marshallingInformation.CustomMarshaller is Dummy)
            this.ReportError(MetadataError.IncompleteNode, marshallingInformation, "CustomMarshaller");
        }
        if (marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray ||
          marshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray) {
          CheckUnmanagedType(marshallingInformation, (uint)marshallingInformation.ElementType, "ElementType");
        }
      }

      private void CheckUnmanagedType(IMarshallingInformation marshallingInformation, uint type, string propertyName) {
        if (type < 0x02 || (0x0c < type && type < 0x13 && type != 0x0f) || type == 0x18 || type == 0x21 || type == 0x27 || type == 0x29 ||
          (0x2d < type && type != 0x50)) {
          this.ReportError(MetadataError.MarshallingInformationIsInvalid, marshallingInformation, propertyName);
        }
      }

      /// <summary>
      /// Performs some computation with the given metadata constant.
      /// </summary>
      public void Visit(IMetadataConstant constant) {
        this.Visit((IMetadataExpression)constant);
        ITypeReference ctype = constant.Type;
        var rctype = constant.Type.ResolvedType;
        if (!(rctype is Dummy)) {
          if (rctype.IsEnum)
            ctype = rctype.UnderlyingType;
          else
            ctype = rctype;
        }
        bool validValue = false;
        switch (ctype.TypeCode) {
          case PrimitiveTypeCode.Boolean:
            validValue = constant.Value is bool; break;
          case PrimitiveTypeCode.Char:
            validValue = constant.Value is char; break;
          case PrimitiveTypeCode.Int8:
            validValue = constant.Value is sbyte; break;
          case PrimitiveTypeCode.UInt8:
            validValue = constant.Value is byte; break;
          case PrimitiveTypeCode.Int16:
            validValue = constant.Value is short; break;
          case PrimitiveTypeCode.UInt16:
            validValue = constant.Value is ushort; break;
          case PrimitiveTypeCode.Int32:
            validValue = constant.Value is int; break;
          case PrimitiveTypeCode.UInt32:
            validValue = constant.Value is uint; break;
          case PrimitiveTypeCode.Int64:
            validValue = constant.Value is long; break;
          case PrimitiveTypeCode.UInt64:
            validValue = constant.Value is ulong; break;
          case PrimitiveTypeCode.Float32:
            validValue = constant.Value is float; break;
          case PrimitiveTypeCode.Float64:
            validValue = constant.Value is double; break;
          case PrimitiveTypeCode.String:
            validValue = constant.Value is string || constant.Value == null; break;
          case PrimitiveTypeCode.NotPrimitive:
            validValue = constant.Value == null || rctype is Dummy; break; //TODO: check that value can be enum val
        }
        if (!validValue)
          this.ReportError(MetadataError.InvalidMetadataConstant, constant);
      }

      /// <summary>
      /// Performs some computation with the given metadata array creation expression.
      /// </summary>
      public void Visit(IMetadataCreateArray createArray) {
        this.Visit((IMetadataExpression)createArray);
      }

      /// <summary>
      /// Performs some computation with the given metadata expression.
      /// </summary>
      public void Visit(IMetadataExpression expression) {
      }

      /// <summary>
      /// Performs some computation with the given metadata named argument expression.
      /// </summary>
      public void Visit(IMetadataNamedArgument namedArgument) {
        //TODO: check for completeness
        this.Visit((IMetadataExpression)namedArgument);
        if (namedArgument.ResolvedDefinition != null) {
          ITypeReference type;
          IName name;
          if (namedArgument.IsField) {
            var field = (IFieldDefinition)namedArgument.ResolvedDefinition;
            type = field.Type;
            name = field.Name;
          } else {
            var property = (IPropertyDefinition)namedArgument.ResolvedDefinition;
            type = property.Type;
            name = property.Name;
          }
          if (name.UniqueKey != namedArgument.ArgumentName.UniqueKey)
            this.ReportError(MetadataError.NamedArgumentNameDoesNotMatchNameOfResolvedFieldOrProperty, namedArgument);
          if (!TypeHelper.TypesAreEquivalent(type, namedArgument.Type))
            this.ReportError(MetadataError.NamedArgumentTypeDoesNotMatchTypeOfResolvedFieldOrProperty, namedArgument);
        }
      }

      /// <summary>
      /// Performs some computation with the given metadata typeof expression.
      /// </summary>
      public void Visit(IMetadataTypeOf typeOf) {
        this.Visit((IMetadataExpression)typeOf);
      }

      /// <summary>
      /// Performs some computation with the given method body.
      /// </summary>
      public void Visit(IMethodBody methodBody) {
        if (!methodBody.LocalsAreZeroed) {
          foreach (var l in methodBody.LocalVariables) {
            this.ReportError(MetadataError.InitLocalsMustBeTrueIfLocalVariables, methodBody.MethodDefinition);
            break;
          }
        }
      }

      /// <summary>
      /// Performs some computation with the given method definition.
      /// </summary>
      public void Visit(IMethodDefinition method) {
        //TODO: check for completeness
        this.Visit((ITypeDefinitionMember)method);
        if (method.IsConstructor) {
          if (method.IsGeneric)
            this.ReportError(MetadataError.GenericConstructor, method);
          if (method.ContainingTypeDefinition.IsInterface)
            this.ReportError(MetadataError.ConstructorInInterface, method);
        }
        if (method.IsStatic && (method.IsSealed || method.IsVirtual || method.IsNewSlot))
          this.ReportError(MetadataError.StaticMethodMayNotBeSealedVirtualOrNewSlot, method);
        if (method.IsAbstract && (method.IsSealed || method.IsPlatformInvoke || method.IsForwardReference))
          this.ReportError(MetadataError.AbstractMethodMayNotBeSealedPlatformInvokeOrForwardReference, method);
        if (method.Visibility == TypeMemberVisibility.Other && (method.IsSpecialName || method.IsRuntimeSpecial))
          this.ReportError(MetadataError.SpecialMethodsMayNotHaveCompilerControlledVisibility, method);
        if (method.IsRuntimeSpecial && !method.IsSpecialName)
          this.ReportError(MetadataError.RuntimeSpecialMustAlsoBeSpecialName, method);
        if (method.IsAbstract && !method.IsVirtual)
          this.ReportError(MetadataError.AbstractMethodsMustBeVirtual, method);
        if (method.HasDeclarativeSecurity) {
          if (IteratorHelper.EnumerableIsEmpty(method.SecurityAttributes) && 
          !AttributeHelper.Contains(method.Attributes, this.validator.host.PlatformType.SystemSecuritySuppressUnmanagedCodeSecurityAttribute))
            this.ReportError(MetadataError.MethodMarkedAsHavingDeclarativeSecurityHasNoSecurityAttributes, method);
        } else {
          if (IteratorHelper.EnumerableIsNotEmpty(method.SecurityAttributes) || 
          AttributeHelper.Contains(method.Attributes, this.validator.host.PlatformType.SystemSecuritySuppressUnmanagedCodeSecurityAttribute))
            this.ReportError(MetadataError.MethodWithSecurityAttributesMustBeMarkedAsHavingDeclarativeSecurity, method);
        }
        if (method.IsSynchronized && method.ContainingTypeDefinition.IsValueType)
          this.ReportError(MetadataError.SynchronizedValueTypeMethod, method);
        if ((method.IsSealed || method.IsNewSlot || method.IsAccessCheckedOnOverride) && !method.IsVirtual)
          this.ReportError(MetadataError.SealedNewSlotOrOverrideMethodsMustAlsoBeVirtual, method);
        if ((method.Name == this.validator.host.NameTable.Cctor || method.Name == this.validator.host.NameTable.Ctor)) {
          if (!method.IsRuntimeSpecial)
            this.ReportError(MetadataError.MethodsNamedLikeConstructorsMustBeMarkedAsRuntimeSpecial, method);
          if (method.Type.TypeCode != PrimitiveTypeCode.Void)
            this.ReportError(MetadataError.ConstructorsMustNotReturnValues, method);
          if (method.IsStatic) {
            if (method.Name != this.validator.host.NameTable.Cctor)
              this.ReportError(MetadataError.InstanceConstructorMayNotBeStatic, method);
            if (method.ParameterCount > 0)
              this.ReportError(MetadataError.StaticConstructorMayNotHaveParameters, method);
          } else if (method.Name != this.validator.host.NameTable.Ctor)
            this.ReportError(MetadataError.StaticConstructorMustBeStatic, method);
        }
        if (method.IsPlatformInvoke) {
          if (!method.IsStatic)
            this.ReportError(MetadataError.NonStaticPlatformInvokeMethod, method);
          switch (method.PlatformInvokeData.PInvokeCallingConvention) {
            case PInvokeCallingConvention.CDecl:
            case PInvokeCallingConvention.StdCall:
            case PInvokeCallingConvention.WinApi:
              break;
            default:
              this.ReportError(MetadataError.InvalidPInvokeCallingConvention, method.PlatformInvokeData, method);
              break;
          }
          if (method.PlatformInvokeData.ImportName.Value == string.Empty)
            this.ReportError(MetadataError.EmptyName, method.PlatformInvokeData, "ImportName");
        }
        if (!IteratorHelper.EnumerableHasLength(method.Parameters, method.ParameterCount))
          this.ReportError(MetadataError.MethodParameterCountDoesNotAgreeWithTheActualNumberOfParameters, method);
        this.CheckGenericMethodTypeParameterUniqueness(method);
        if (method.ReturnValueIsMarshalledExplicitly) {
        }
        if (method.IsGeneric) {
          var mfmv = this.validator.currentModule.MetadataFormatMajorVersion;
          if (mfmv < 2)
            this.ReportError(MetadataError.InvalidMetadataFormatVersionForGenerics, method, mfmv.ToString());
        }
        // TODO: Check if method conforms to the naming conventions for event/property accessor. If so, make sure
        // it really is the accessor it claims to be.
      }

      private void CheckGenericMethodTypeParameterUniqueness(IMethodDefinition methodDefinition) {
        int count = 0;
        Hashtable gparTable = null;
        foreach (var gpar in methodDefinition.GenericParameters) {
          var key = gpar.InternedKey;
          if (gparTable == null) gparTable = new Hashtable();
          if (gparTable.Find(key) != 0)
            this.ReportError(MetadataError.DuplicateMethodGenericTypeParameter, gpar);
          else
            gparTable.Add(key, key);
          count++;
        }
        if (count != methodDefinition.GenericParameterCount)
          this.ReportError(MetadataError.MethodGenericTypeParameterCountMismatch, methodDefinition);
      }

      /// <summary>
      /// Performs some computation with the given method implementation.
      /// </summary>
      public void Visit(IMethodImplementation methodImplementation) {
        if (methodImplementation.ContainingType is Dummy)
          this.ReportError(MetadataError.IncompleteNode, methodImplementation, "ContainingType");
        if (methodImplementation.ImplementedMethod is Dummy)
          this.ReportError(MetadataError.IncompleteNode, methodImplementation, "ImplementedMethod");
        if (methodImplementation.ImplementingMethod is Dummy)
          this.ReportError(MetadataError.IncompleteNode, methodImplementation, "ImplementingMethod");
        var resolvedImplementedMethod = methodImplementation.ImplementedMethod.ResolvedMethod;
        if (!(resolvedImplementedMethod is Dummy)) {
          if (!resolvedImplementedMethod.IsVirtual || resolvedImplementedMethod.IsSealed || resolvedImplementedMethod.ContainingTypeDefinition.IsSealed)
            this.ReportError(MetadataError.MethodCannotBeAnOverride, resolvedImplementedMethod, methodImplementation);
          if (resolvedImplementedMethod.IsAccessCheckedOnOverride) {
            switch (resolvedImplementedMethod.Visibility) {
              case TypeMemberVisibility.Public:
              case TypeMemberVisibility.Family:
              case TypeMemberVisibility.FamilyOrAssembly:
                break;
              case TypeMemberVisibility.Assembly:
              case TypeMemberVisibility.FamilyAndAssembly:
                var implementedUnit = TypeHelper.GetDefiningUnit(resolvedImplementedMethod.ContainingTypeDefinition);
                if (implementedUnit is Dummy) break; //Complaining might be a false positive. Wait until runtime.
                if (implementedUnit.UnitIdentity.Equals(this.validator.currentModule.ModuleIdentity)) break;
                var implementingAssembly = this.validator.currentModule as IAssembly;
                var implementedAssembly = implementedUnit as IAssembly;
                if (implementingAssembly != null && implementedAssembly != null &&
              UnitHelper.AssemblyOneAllowsAssemblyTwoToAccessItsInternals(implementedAssembly, implementingAssembly))
                  break;
                goto default;
              default:
                this.ReportError(MetadataError.MayNotOverrideInaccessibleMethod, resolvedImplementedMethod, methodImplementation);
                break;
            }
          }
        }
        //check that implemented method is inherited from a base class or interface
        //check for local or inherited implementer
        var resolvedImplementingMethod = methodImplementation.ImplementingMethod.ResolvedMethod;
        if (!(resolvedImplementedMethod is Dummy)) {
          if (!(resolvedImplementedMethod.IsVirtual || resolvedImplementedMethod.IsAbstract || resolvedImplementedMethod.IsExternal))
            this.ReportError(MetadataError.MethodCannotBeAnOverride, resolvedImplementedMethod, methodImplementation);
        }

        if (methodImplementation.ImplementingMethod.IsGeneric) {
          if (!MemberHelper.GenericMethodSignaturesAreEqual(methodImplementation.ImplementedMethod, methodImplementation.ImplementingMethod))
            this.ReportError(MetadataError.ExplicitOverrideDoesNotMatchSignatureOfOverriddenMethod, methodImplementation);
        } else {
          if (!MemberHelper.SignaturesAreEqual(methodImplementation.ImplementedMethod, methodImplementation.ImplementingMethod))
            this.ReportError(MetadataError.ExplicitOverrideDoesNotMatchSignatureOfOverriddenMethod, methodImplementation);
        }
      }

      /// <summary>
      /// Performs some computation with the given method reference.
      /// </summary>
      public void Visit(IMethodReference methodReference) {
      }

      /// <summary>
      /// Performs some computation with the given modified type reference.
      /// </summary>
      public void Visit(IModifiedTypeReference modifiedTypeReference) {
        this.Visit((ITypeReference)modifiedTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given module.
      /// </summary>
      public void Visit(IModule module) {
        if (module.ModuleName.Value.IndexOfAny(badPosixNameChars) > 0)
          this.ReportError(MetadataError.NotPosixAssemblyName, module, module.ModuleName.Value);
        foreach (var type in module.GetAllTypes()) {
          if (this.allTypes.Contains(type)) {
            this.ReportError(MetadataError.DuplicateEntryInAllTypes, module);
            continue;
          }
          this.allTypes.Add(type);
        }
        this.Visit((IUnit)module);
        //check for duplicate assembly references
        var refsSeenSoFar = new Dictionary<AssemblyIdentity, IAssemblyReference>();
        foreach (var assemblyReference in module.AssemblyReferences) {
          IAssemblyReference duplicate = null;
          if (refsSeenSoFar.TryGetValue(assemblyReference.AssemblyIdentity, out duplicate))
            this.ReportError(MetadataError.DuplicateAssemblyReference, assemblyReference, duplicate);
          else
            refsSeenSoFar.Add(assemblyReference.AssemblyIdentity, assemblyReference);
        }
        //TODO: other kinds of refs?
        foreach (var typeMemberReference in module.GetTypeMemberReferences())
          this.Visit(typeMemberReference);
        foreach (var typeReference in module.GetTypeReferences())
          this.Visit(typeReference);
      }

      /// <summary>
      /// Performs some computation with the given module reference.
      /// </summary>
      public void Visit(IModuleReference moduleReference) {
        this.Visit((IUnitReference)moduleReference);
      }

      /// <summary>
      /// Performs some computation with the given named type definition.
      /// </summary>
      public void Visit(INamedTypeDefinition namedTypeDefinition) {
        if (namedTypeDefinition.IsValueType && TypeHelper.SizeOfType(namedTypeDefinition) == 0)
          this.ReportError(MetadataError.StructSizeMustBeNonZero, namedTypeDefinition);

        this.Visit((ITypeDefinition)namedTypeDefinition);
      }

      /// <summary>
      /// Performs some computation with the given named type reference.
      /// </summary>
      public void Visit(INamedTypeReference namedTypeReference) {
        this.Visit((ITypeReference)namedTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given alias for a namespace type definition.
      /// </summary>
      public void Visit(INamespaceAliasForType namespaceAliasForType) {
        this.Visit((IAliasForType)namespaceAliasForType);
        if (!namespaceAliasForType.IsPublic && !(TypeHelper.GetDefiningUnitReference(namespaceAliasForType.AliasedType) is IAssemblyReference))
          this.ReportError(MetadataError.NonPublicTypeAlias, namespaceAliasForType);
      }

      /// <summary>
      /// Visits the specified namespace definition.
      /// </summary>
      public void Visit(INamespaceDefinition namespaceDefinition) {
      }

      /// <summary>
      /// Visits the specified namespace member.
      /// </summary>
      public void Visit(INamespaceMember namespaceMember) {
        if (this.definitionsAlreadyVisited.Contains(namespaceMember)) {
          this.ReportError(MetadataError.DuplicateDefinition, namespaceMember);
          return;
        }
        this.definitionsAlreadyVisited.Add(namespaceMember);
        if (namespaceMember.Name.Value == string.Empty)
          this.ReportError(MetadataError.EmptyName, namespaceMember);
        if (namespaceMember.ContainingNamespace is Dummy)
          this.ReportError(MetadataError.IncompleteNode, namespaceMember, "ContainingNamespace");
        if (!this.definitionsAlreadyVisited.Contains(namespaceMember.ContainingNamespace) && !(namespaceMember is ITypeDefinitionMember))
          this.ReportError(MetadataError.ContainingNamespaceDefinitionNotVisited, namespaceMember);
      }

      /// <summary>
      /// Performs some computation with the given namespace type definition.
      /// </summary>
      public void Visit(INamespaceTypeDefinition namespaceTypeDefinition) {
        this.Visit((INamedTypeDefinition)namespaceTypeDefinition);
        if (!this.allTypes.Contains(namespaceTypeDefinition))
          this.ReportError(MetadataError.GetAllTypesIsIncomplete, namespaceTypeDefinition);
      }

      /// <summary>
      /// Performs some computation with the given namespace type reference.
      /// </summary>
      public void Visit(INamespaceTypeReference namespaceTypeReference) {
        this.Visit((INamedTypeReference)namespaceTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given alias to a nested type definition.
      /// </summary>
      public void Visit(INestedAliasForType nestedAliasForType) {
        this.Visit((IAliasForType)nestedAliasForType);
        if (nestedAliasForType.Visibility != TypeMemberVisibility.Public && !(TypeHelper.GetDefiningUnitReference(nestedAliasForType.AliasedType) is IAssemblyReference))
          this.ReportError(MetadataError.NonPublicTypeAlias, nestedAliasForType);
        if (nestedAliasForType.ContainingAlias is Dummy) {
          this.ReportError(MetadataError.IncompleteNode, nestedAliasForType, "ContainingAlias");
        } else if (!IteratorHelper.EnumerableContains(this.validator.currentModule.ContainingAssembly.ExportedTypes, nestedAliasForType.ContainingAlias))
          this.ReportError(MetadataError.ContainingAliasNotListedInExportedTypes, nestedAliasForType.ContainingAlias, this.validator.currentModule.ContainingAssembly);
      }

      /// <summary>
      /// Performs some computation with the given nested type definition.
      /// </summary>
      public void Visit(INestedTypeDefinition nestedTypeDefinition) {
        this.Visit((INamedTypeDefinition)nestedTypeDefinition);
        if (!this.allTypes.Contains(nestedTypeDefinition))
          this.ReportError(MetadataError.GetAllTypesIsIncomplete, nestedTypeDefinition);
      }

      /// <summary>
      /// Performs some computation with the given nested type reference.
      /// </summary>
      public void Visit(INestedTypeReference nestedTypeReference) {
        this.Visit((INamedTypeReference)nestedTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given nested unit namespace.
      /// </summary>
      public void Visit(INestedUnitNamespace nestedUnitNamespace) {
        this.Visit((IUnitNamespace)nestedUnitNamespace);
      }

      /// <summary>
      /// Performs some computation with the given nested unit namespace reference.
      /// </summary>
      public void Visit(INestedUnitNamespaceReference nestedUnitNamespaceReference) {
        this.Visit((IUnitNamespaceReference)nestedUnitNamespaceReference);
      }

      /// <summary>
      /// Performs some computation with the given nested unit set namespace.
      /// </summary>
      public void Visit(INestedUnitSetNamespace nestedUnitSetNamespace) {
        this.Visit((IUnitSetNamespace)nestedUnitSetNamespace);
      }

      /// <summary>
      /// Performs some computation with the given IL operation.
      /// </summary>
      public void Visit(IOperation operation) {
      }

      /// <summary>
      /// Performs some computation with the given IL operation exception information instance.
      /// </summary>
      public void Visit(IOperationExceptionInformation operationExceptionInformation) {
      }

      /// <summary>
      /// Performs some computation with the given parameter definition.
      /// </summary>
      public void Visit(IParameterDefinition parameterDefinition) {
        if (parameterDefinition.HasDefaultValue) {
          var parameterType = parameterDefinition.Type;
          if (parameterType.IsEnum && !(parameterType.ResolvedType is Dummy)) parameterType = parameterType.ResolvedType.UnderlyingType;
          if (CompileTimeConstantTypeDoesNotMatchDefinitionType(parameterDefinition.DefaultValue, parameterType))
            this.ReportError(MetadataError.MetadataConstantTypeMismatch, parameterDefinition.DefaultValue, parameterDefinition);
        }
        if (parameterDefinition.IsMarshalledExplicitly) {
          if (parameterDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray) {
            if (parameterDefinition.MarshallingInformation.ParamIndex != null) {
              var index = parameterDefinition.MarshallingInformation.ParamIndex.Value;
              if (index >= IteratorHelper.EnumerableCount(parameterDefinition.ContainingSignature.Parameters))
                this.ReportError(MetadataError.ParameterIndexIsInvalid, parameterDefinition.MarshallingInformation, parameterDefinition);
              if (parameterDefinition.MarshallingInformation.NumberOfElements > 0)
                this.ReportError(MetadataError.NumberOfElementsSpecifiedExplicitlyAsWellAsByAParameter, parameterDefinition);
            } else {
              //The ECMA spec seems to suggest that this check is needed.
              //The MSDN documentation claims that these values are ignored when marshalling from the CLR to COM.
              //Actual code found in the .NET Framework fail this test. So disable it for now.
              //if (parameterDefinition.MarshallingInformation.NumberOfElements == 0 && parameterDefinition.IsOut)
              //  this.ReportError(MetadataError.ParameterMarshalledArraysMustHaveSizeKnownAtCompileTime, parameterDefinition.MarshallingInformation, parameterDefinition);
            }
          }
          if (parameterDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray) {
            this.ReportError(MetadataError.ParameterCannotBeMarshalledAsByValArray, parameterDefinition.MarshallingInformation, parameterDefinition);
          }
          if (parameterDefinition.MarshallingInformation.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValTStr) {
            if (parameterDefinition.MarshallingInformation.NumberOfElements == 0)
              this.ReportError(MetadataError.ParameterMarshalledArraysMustHaveSizeKnownAtCompileTime, parameterDefinition.MarshallingInformation, parameterDefinition);
          }
        }
      }

      /// <summary>
      /// Performs some computation with the given parameter definition.
      /// </summary>
      public void VisitReference(IParameterDefinition parameterDefinition) {
      }

      /// <summary>
      /// Performs some computation with the given property definition.
      /// </summary>
      public void Visit(IPropertyDefinition propertyDefinition) {
        this.Visit((ITypeDefinitionMember)propertyDefinition);
        if (propertyDefinition.IsRuntimeSpecial && !propertyDefinition.IsSpecialName)
          this.ReportError(MetadataError.RuntimeSpecialMustAlsoBeSpecialName, propertyDefinition);
        if (propertyDefinition.HasDefaultValue) {
          var propertyType = propertyDefinition.Type;
          if (propertyType.IsEnum && !(propertyType.ResolvedType is Dummy)) propertyType = propertyType.ResolvedType.UnderlyingType;
          if (CompileTimeConstantTypeDoesNotMatchDefinitionType(propertyDefinition.DefaultValue, propertyDefinition.Type))
            this.ReportError(MetadataError.MetadataConstantTypeMismatch, propertyDefinition.DefaultValue, propertyDefinition);
        }
        if (propertyDefinition.Getter != null) {
          if (propertyDefinition.Getter is Dummy)
            this.ReportError(MetadataError.IncompleteNode, propertyDefinition, "Getter");
          else if (!IteratorHelper.EnumerableContains(propertyDefinition.Accessors, propertyDefinition.Getter))
            this.ReportError(MetadataError.AccessorListInconsistent, propertyDefinition, "Getter");
        }
        if (propertyDefinition.Setter != null) {
          if (propertyDefinition.Setter is Dummy)
            this.ReportError(MetadataError.IncompleteNode, propertyDefinition, "Setter");
          else if (!IteratorHelper.EnumerableContains(propertyDefinition.Accessors, propertyDefinition.Setter))
            this.ReportError(MetadataError.AccessorListInconsistent, propertyDefinition, "Setter");
        }

        foreach (var a in propertyDefinition.Accessors) {
          if (MemberHelper.IsGetter(a.ResolvedMethod) && a != propertyDefinition.Getter)
            this.ReportError(MetadataError.EventPropertyNamingPatternWarning, a);
          if (MemberHelper.IsSetter(a.ResolvedMethod) && a != propertyDefinition.Setter)
            this.ReportError(MetadataError.EventPropertyNamingPatternWarning, a);
        }
      }

      /// <summary>
      /// Performs some computation with the given parameter type information.
      /// </summary>
      public void Visit(IParameterTypeInformation parameterTypeInformation) {
      }

      /// <summary>
      /// Performs some compuation with the given PE section.
      /// </summary>
      public void Visit(IPESection peSection) {
      }

      /// <summary>
      /// Performs some compuation with the given platoform invoke information.
      /// </summary>
      public void Visit(IPlatformInvokeInformation platformInvokeInformation) {
      }

      /// <summary>
      /// Performs some computation with the given pointer type reference.
      /// </summary>
      public void Visit(IPointerTypeReference pointerTypeReference) {
        this.Visit((ITypeReference)pointerTypeReference);
      }

      /// <summary>
      /// Performs some computation with the given reference to a manifest resource.
      /// </summary>
      public void Visit(IResourceReference resourceReference) {
        if (resourceReference.Name.Value == string.Empty)
          this.ReportError(MetadataError.EmptyName, resourceReference);
      }

      /// <summary>
      /// Performs some computation with the given root unit namespace.
      /// </summary>
      public void Visit(IRootUnitNamespace rootUnitNamespace) {
        this.Visit((IUnitNamespace)rootUnitNamespace);
      }

      /// <summary>
      /// Performs some computation with the given root unit namespace reference.
      /// </summary>
      public void Visit(IRootUnitNamespaceReference rootUnitNamespaceReference) {
        this.Visit((IUnitNamespaceReference)rootUnitNamespaceReference);
      }

      /// <summary>
      /// Performs some computation with the given root unit set namespace.
      /// </summary>
      public void Visit(IRootUnitSetNamespace rootUnitSetNamespace) {
        this.Visit((IUnitSetNamespace)rootUnitSetNamespace);
      }

      /// <summary>
      /// Performs some computation with the given security attribute.
      /// </summary>
      public void Visit(ISecurityAttribute securityAttribute) {
        var currentType = this.validator.currentDefinition as ITypeDefinition;
        if (currentType != null && currentType.IsInterface)
          this.ReportError(MetadataError.SecurityAttributeOnInterface, securityAttribute, currentType);
        switch (securityAttribute.Action) {
          case SecurityAction.Assert:
          case SecurityAction.Demand:
          case SecurityAction.Deny:
          case SecurityAction.InheritanceDemand:
          case SecurityAction.LinkDemand:
          case SecurityAction.PermitOnly:
            if (!(this.validator.currentDefinition is IMethodDefinition  || this.validator.currentDefinition is ITypeDefinition))
              this.ReportError(MetadataError.SecurityActionMismatch, securityAttribute, this.validator.currentDefinition);
            //TODO: The specified attribute shall derive from System.Security.Permissions.CodeAccess-SecurityAttribute
            break;
          case SecurityAction.NonCasDemand:
          case SecurityAction.NonCasLinkDemand:
            if (!(this.validator.currentDefinition is IMethodDefinition  || this.validator.currentDefinition is ITypeDefinition))
              this.ReportError(MetadataError.SecurityActionMismatch, securityAttribute, this.validator.currentDefinition);
            //The attribute shall derive from System.Security.Permissions.SecurityAttribute, but shall not derive from System.Security.Permissions.CodeAccessSecurityAttribute
            break;
          case SecurityAction.ActionNil:
          case SecurityAction.NonCasInheritance:
          case SecurityAction.PrejitDenied:
          case SecurityAction.Request:
            break;
          case SecurityAction.PrejitGrant:
          case SecurityAction.RequestMinimum:
          case SecurityAction.RequestOptional:
          case SecurityAction.RequestRefuse:
            if (!(this.validator.currentDefinition is IAssembly))
              this.ReportError(MetadataError.SecurityActionMismatch, securityAttribute, this.validator.currentDefinition);
            break;
          default:
            this.ReportError(MetadataError.InvalidSecurityAction, securityAttribute);
            break;
        }
      }

      /// <summary>
      /// Performs some computation with the given specialized event definition.
      /// </summary>
      public void Visit(ISpecializedEventDefinition specializedEventDefinition) {
        this.Visit((IEventDefinition)specializedEventDefinition);
      }

      /// <summary>
      /// Performs some computation with the given specialized field definition.
      /// </summary>
      public void Visit(ISpecializedFieldDefinition specializedFieldDefinition) {
        this.Visit((IFieldDefinition)specializedFieldDefinition);
      }

      /// <summary>
      /// Performs some computation with the given specialized field reference.
      /// </summary>
      public void Visit(ISpecializedFieldReference specializedFieldReference) {
        this.Visit((IFieldReference)specializedFieldReference);
      }

      /// <summary>
      /// Performs some computation with the given specialized method definition.
      /// </summary>
      public void Visit(ISpecializedMethodDefinition specializedMethodDefinition) {
        this.Visit((IMethodDefinition)specializedMethodDefinition);
      }

      /// <summary>
      /// Performs some computation with the given specialized method reference.
      /// </summary>
      public void Visit(ISpecializedMethodReference specializedMethodReference) {
        this.Visit((IMethodReference)specializedMethodReference);
      }

      /// <summary>
      /// Performs some computation with the given specialized propperty definition.
      /// </summary>
      public void Visit(ISpecializedPropertyDefinition specializedPropertyDefinition) {
        this.Visit((IPropertyDefinition)specializedPropertyDefinition);
      }

      /// <summary>
      /// Performs some computation with the given specialized nested type definition.
      /// </summary>
      public void Visit(ISpecializedNestedTypeDefinition specializedNestedTypeDefinition) {
        this.Visit((INestedTypeDefinition)specializedNestedTypeDefinition);
      }

      /// <summary>
      /// Performs some computation with the given specialized nested type reference.
      /// </summary>
      public void Visit(ISpecializedNestedTypeReference specializedNestedTypeReference) {
        this.Visit((INestedTypeReference)specializedNestedTypeReference);
      }

      /// <summary>
      /// Visits the specified type definition.
      /// </summary>
      public void Visit(ITypeDefinition typeDefinition) {
        this.validator.currentDefinition = typeDefinition;
        if (this.definitionsAlreadyVisited.Contains(typeDefinition)) {
          this.ReportError(MetadataError.DuplicateDefinition, typeDefinition);
          return;
        }
        this.definitionsAlreadyVisited.Add(typeDefinition);
        if (typeDefinition.Alignment > 0) {
          if (typeDefinition.Layout != LayoutKind.Sequential) {
            //work around bug in c# compiler
            if (typeDefinition.Layout != LayoutKind.Explicit)
              this.ReportError(MetadataError.OnlySequentialLayoutTypesCanSpecificyAlignment, typeDefinition);
          }
          if (!validAlignments.Contains(typeDefinition.Alignment))
            this.ReportError(MetadataError.InvalidAlignment, typeDefinition, typeDefinition.Alignment.ToString());
        }
        if (typeDefinition.SizeOf > 0) {
          if (typeDefinition.Layout == LayoutKind.Auto)
            this.ReportError(MetadataError.AutoLayoutTypesCannotSpecifySize, typeDefinition);
          else if (typeDefinition.SizeOf >= 0x100000 && typeDefinition.IsStruct)
            this.ReportError(MetadataError.StructsSizeMustBeLessThanOneMegaByte, typeDefinition, typeDefinition.SizeOf.ToString());
        }
        if (typeDefinition.Layout != LayoutKind.Auto) {
          //base classes must have the same layout if a derived class is not Auto.
          foreach (var baseClass in typeDefinition.BaseClasses) this.CheckLayoutConsistency(baseClass.ResolvedType, typeDefinition);
        }
        if (typeDefinition.HasDeclarativeSecurity) {
          if (typeDefinition.IsInterface)
            this.ReportError(MetadataError.DeclarativeSecurityOnInterfacesIsIgnored, typeDefinition);
        }
        if (typeDefinition.IsEnum) {
          bool foundInstanceField = false;
          foreach (var field in typeDefinition.Fields) {
            if (field.IsStatic) continue;
            if (foundInstanceField) {
              this.ReportError(MetadataError.EnumInstanceFieldNotUnique, field, typeDefinition);
              continue;
            }
            if (!TypeHelper.IsPrimitiveInteger(field.Type))
              this.ReportError(MetadataError.EnumInstanceFieldTypeNotIntegral, field, typeDefinition);
            foundInstanceField = true;
          }
          if (!foundInstanceField)
            this.ReportError(MetadataError.EnumDoesNotHaveAnInstanceField, typeDefinition);
        }
        foreach (var member in typeDefinition.Members) {
          if (!(member.ContainingTypeDefinition is Dummy) && member.ContainingTypeDefinition != typeDefinition)
            this.ReportError(MetadataError.MemberDisagreesAboutContainer, member, typeDefinition);
        }
        if (typeDefinition.IsRuntimeSpecial && !typeDefinition.IsSpecialName)
          this.ReportError(MetadataError.RuntimeSpecialMustAlsoBeSpecialName, typeDefinition);
        if (typeDefinition.IsGeneric) {
          var mfmv = this.validator.currentModule.MetadataFormatMajorVersion;
          if (mfmv < 2)
            this.ReportError(MetadataError.InvalidMetadataFormatVersionForGenerics, typeDefinition, mfmv.ToString());
        }
        this.CheckEventNameUniqueness(typeDefinition);
        this.CheckFieldUniqueness(typeDefinition);
        this.CheckInterfaceUniqueness(typeDefinition);
        this.CheckMethodUniqueness(typeDefinition);
        this.CheckTypeParameterUniqueness(typeDefinition);
      }

      private void CheckEventNameUniqueness(ITypeDefinition typeDefinition) {
        Hashtable eventNameTable = null;
        foreach (var ev in typeDefinition.Events) {
          var key = (uint)ev.Name.UniqueKey;
          if (eventNameTable == null) eventNameTable = new Hashtable();
          if (eventNameTable.Find(key) != 0)
            this.ReportError(MetadataError.DuplicateEvent, ev);
          else
            eventNameTable.Add(key, key);
        }
      }

      private void CheckFieldUniqueness(ITypeDefinition typeDefinition) {
        Hashtable fieldTable = null;
        foreach (var field in typeDefinition.Fields) {
          var key = field.InternedKey;
          if (fieldTable == null) fieldTable = new Hashtable();
          if (fieldTable.Find(key) != 0) {
            if (field.Visibility != TypeMemberVisibility.Other)
              this.ReportError(MetadataError.DuplicateField, field);
          } else
            fieldTable.Add(key, key);
        }
      }

      private void CheckInterfaceUniqueness(ITypeDefinition typeDefinition) {
        Hashtable interfaceTable = null;
        foreach (var iface in typeDefinition.Interfaces) {
          var key = iface.InternedKey;
          if (interfaceTable == null) interfaceTable = new Hashtable();
          if (interfaceTable.Find(key) != 0)
            this.ReportError(MetadataError.DuplicateInterface, iface, typeDefinition);
          else
            interfaceTable.Add(key, key);
        }
      }

      private void CheckMethodUniqueness(ITypeDefinition typeDefinition) {
        Hashtable methodTable = null;
        foreach (var method in typeDefinition.Methods) {
          var key = method.InternedKey;
          if (methodTable == null) methodTable = new Hashtable();
          if (methodTable.Find(key) != 0) {
            if (method.Visibility != TypeMemberVisibility.Other)
              this.ReportError(MetadataError.DuplicateMethod, method, typeDefinition);
          } else
            methodTable.Add(key, key);
        }
      }

      private void CheckTypeParameterUniqueness(ITypeDefinition typeDefinition) {
        if (!typeDefinition.IsGeneric) return;
        Hashtable gparTable = null;
        int count = 0;
        foreach (var gpar in typeDefinition.GenericParameters) {
          count++;
          var key = gpar.InternedKey;
          if (gparTable == null) gparTable = new Hashtable();
          if (gparTable.Find(key) != 0)
            this.ReportError(MetadataError.DuplicateGenericTypeParameter, gpar);
          else
            gparTable.Add(key, key);
        }
        if (count != typeDefinition.GenericParameterCount)
          this.ReportError(MetadataError.GenericParameterCountDoesNotMatchGenericParameters, typeDefinition);
      }

      private void CheckLayoutConsistency(ITypeDefinition baseTypeDefinition, ITypeDefinition derivedTypeDefinition) {
        if (baseTypeDefinition.Layout == derivedTypeDefinition.Layout) return;
        if (baseTypeDefinition is Dummy || 
          TypeHelper.TypesAreEquivalent(baseTypeDefinition, derivedTypeDefinition.PlatformType.SystemObject) ||
          TypeHelper.TypesAreEquivalent(baseTypeDefinition, derivedTypeDefinition.PlatformType.SystemValueType)) return;
        this.ReportError(MetadataError.DerivedTypeHasDifferentLayoutFromBaseType, derivedTypeDefinition, baseTypeDefinition);
      }

      static List<int> validAlignments = new List<int>(new int[] { 1, 2, 4, 8, 16, 32, 64, 128 });

      /// <summary>
      /// Visits the specified type member.
      /// </summary>
      public void Visit(ITypeDefinitionMember typeMember) {
        if (this.definitionsAlreadyVisited.Contains(typeMember)) {
          this.ReportError(MetadataError.DuplicateDefinition, typeMember);
          return;
        }
        this.definitionsAlreadyVisited.Add(typeMember);
        if (typeMember.Name.Value == string.Empty)
          this.ReportError(MetadataError.EmptyName, typeMember);
        if (typeMember.ContainingTypeDefinition is Dummy)
          this.ReportError(MetadataError.IncompleteNode, typeMember, "ContainingTypeDefinition");
        if (!this.definitionsAlreadyVisited.Contains(typeMember.ContainingTypeDefinition) && !(typeMember is INamespaceMember))
          this.ReportError(MetadataError.ContainingTypeDefinitionNotVisited, typeMember);
        switch (typeMember.Visibility) {
          case TypeMemberVisibility.Assembly:
          case TypeMemberVisibility.Family:
          case TypeMemberVisibility.FamilyAndAssembly:
          case TypeMemberVisibility.FamilyOrAssembly:
          case TypeMemberVisibility.Other:
          case TypeMemberVisibility.Private:
          case TypeMemberVisibility.Public:
            break;
          default:
            this.ReportError(MetadataError.InvalidTypeMemberVisibility, typeMember);
            break;
        }
      }

      /// <summary>
      /// Visits the specified type member reference.
      /// </summary>
      public void Visit(ITypeMemberReference typeMember) {
        var resolvedReference = typeMember.ResolvedTypeDefinitionMember;
        if (!(resolvedReference is Dummy) && typeMember != resolvedReference) {
          if (resolvedReference.Visibility == TypeMemberVisibility.Other)
            this.ReportError(MetadataError.ReferenceToTypeMemberWithOtherVisibility, typeMember, resolvedReference);
        }
      }

      /// <summary>
      /// Visits the specified type reference.
      /// </summary>
      public void Visit(ITypeReference typeReference) {
        if (typeReference.InternedKey == 0)
          this.ReportError(MetadataError.IncompleteNode, typeReference, "InternedKey");
        var resolvedType = typeReference.ResolvedType;
        if (resolvedType is Dummy) {
          if (!(resolvedType is Dummy)) {
            //TODO: report error
          }
        } else if (typeReference.InternedKey != resolvedType.InternedKey) {
          var assemRef = TypeHelper.GetDefiningUnitReference(typeReference) as IAssemblyReference;
          if (assemRef != null && assemRef.UnifiedAssemblyIdentity != assemRef.AssemblyIdentity) {
            //The intern keys will differ because of unification. Let's be happy if the names match.
            if (TypeHelper.GetTypeName(typeReference) == TypeHelper.GetTypeName(resolvedType)) return;
          }
          //If we get here the type had better be an alias
          if (!(typeReference is IGenericTypeInstanceReference)) {
            if (!typeReference.IsAlias)
              this.ReportError(MetadataError.TypeReferenceResolvesToDifferentType, typeReference);
            else if (typeReference.AliasForType.AliasedType.ResolvedType.InternedKey != resolvedType.InternedKey)
              this.ReportError(MetadataError.TypeReferenceResolvesToDifferentTypeFromAlias, typeReference);
          }
        }
      }

      /// <summary>
      /// Visits the specified unit.
      /// </summary>
      public void Visit(IUnit unit) {
      }

      /// <summary>
      /// Visits the specified unit reference.
      /// </summary>
      public void Visit(IUnitReference unitReference) {
        if (unitReference.Name.Value == string.Empty) {
          this.ReportError(MetadataError.EmptyName, unitReference);
        }
      }

      /// <summary>
      /// Visits the specified unit namespace.
      /// </summary>
      public void Visit(IUnitNamespace unitNamespace) {
        this.Visit((INamespaceDefinition)unitNamespace);
      }

      /// <summary>
      /// Visits the specified unit namespace reference.
      /// </summary>
      public void Visit(IUnitNamespaceReference unitNamespaceReference) {
      }

      /// <summary>
      /// Performs some computation with the given unit set.
      /// </summary>
      public void Visit(IUnitSet unitSet) {
      }

      /// <summary>
      /// Visits the specified unit set namespace.
      /// </summary>
      public void Visit(IUnitSetNamespace unitSetNamespace) {
      }

      /// <summary>
      /// Performs some computation with the given Win32 resource.
      /// </summary>
      public void Visit(IWin32Resource win32Resource) {
      }

      /// <summary>
      /// Constructs an IErrorMessage instance that encapsulates the given information and reports the error to the host application.
      /// </summary>
      /// <param name="error">The kind of error.</param>
      /// <param name="node">The node where the error was discovered.</param>
      /// <param name="relatedNodes">Any other nodes that relate to this error.</param>
      private void ReportError(MetadataError error, object node, params object[] relatedNodes) {
        var message = new ErrorMessage() { Error = error, ErrorReporter = this, Location = new MetadataNode() { Document = this.validator.document, Node = node } };
        if (relatedNodes.Length > 0) {
          var relatedLocations = new List<ILocation>(relatedNodes.Length);
          foreach (var relatedNode in relatedNodes) relatedLocations.Add(new MetadataNode() { Document = this.validator.document, Node = relatedNode });
          message.RelatedLocations = relatedLocations.AsReadOnly();
        } else
          message.RelatedLocations = emptyLocations;
        this.validator.host.ReportError(message);
      }

      /// <summary>
      /// Constructs an IErrorMessage instance that encapsulates the given information and reports the error to the host application.
      /// </summary>
      /// <param name="error">The kind of error.</param>
      /// <param name="node">The node where the error was discovered.</param>\
      /// <param name="messageParameter">A string that is inserted into the error message to provide more information.</param>
      /// <param name="relatedNodes">Any other nodes that relate to this error.</param>
      private void ReportError(MetadataError error, object node, string messageParameter, params object[] relatedNodes) {
        var message = new ErrorMessage() { Error = error, ErrorReporter = this, MessageParameter = messageParameter, Location = new MetadataNode() { Document = this.validator.document, Node = node } };
        if (relatedNodes.Length > 0) {
          var relatedLocations = new List<ILocation>(relatedNodes.Length);
          foreach (var relatedNode in relatedNodes) relatedLocations.Add(new MetadataNode() { Document = this.validator.document, Node = relatedNode });
          message.RelatedLocations = relatedLocations.AsReadOnly();
        } else
          message.RelatedLocations = emptyLocations;
        this.validator.host.ReportError(message);
      }

      //static char[] badAssemblyNameChars = new char[] { ':', '\\', '/', '.' };

      static char[] badPosixNameChars = new char[] { ':', '\\', '/' };

      static List<string> validCultureNames = new List<string>(
        new string[] {
    "ar-SA",	"ar-IQ",	"ar-EG",	"ar-LY",
"ar-DZ",	"ar-MA",	"ar-TN",	"ar-OM",
"ar-YE",	"ar-SY",	"ar-JO",	"ar-LB",
"ar-KW",	"ar-AE",	"ar-BH",	"ar-QA",
"bg-BG",	"ca-ES",	"zh-TW",	"zh-CN",
"zh-HK",	"zh-SG",	"zh-MO",	"cs-CZ",
"da-DK",	"de-DE",	"de-CH",	"de-AT",
"de-LU",	"de-LI",	"el-GR",	"en-US",
"en-GB",	"en-AU",	"en-CA",	"en-NZ",
"en-IE",	"en-ZA",	"en-JM",	"en-CB",
"en-BZ",	"en-TT",	"en-ZW",	"en-PH",
"es-ES-Ts",	"es-MX",	"es-ES-Is",	"es-GT",
"es-CR",	"es-PA",	"es-DO",	"es-VE",
"es-CO",	"es-PE",	"es-AR",	"es-EC",
"es-CL",	"es-UY",	"es-PY",	"es-BO",
"es-SV",	"es-HN",	"es-NI",	"es-PR",
"fi-FI",	"fr-FR",	"fr-BE",	"fr-CA",
"fr-CH",	"fr-LU",	"fr-MC",	"he-IL",
"hu-HU",	"is-IS",	"it-IT",	"it-CH",
"ja-JP",	"ko-KR",	"nl-NL",	"nl-BE",
"nb-NO",	"nn-NO",	"pl-PL",	"pt-BR",
"pt-PT",	"ro-RO",	"ru-RU",	"hr-HR",
"lt-sr-SP",	"cy-sr-SP",	"sk-SK",	"sq-AL",
"sv-SE",	"sv-FI",	"th-TH",	"tr-TR",
"ur-PK",	"id-ID",	"uk-UA",	"be-BY",
"sl-SI",	"et-EE",	"lv-LV",	"lt-LT",
"fa-IR",	"vi-VN",	"hy-AM",	"lt-az-AZ",
"cy-az-AZ",	"eu-ES",	"mk-MK",	"af-ZA",
"ka-GE",	"fo-FO",	"hi-IN",	"ms-MY",
"ms-BN",	"kk-KZ",	"ky-KZ",	"sw-KE",
"lt-uz-UZ",	"cy-uz-UZ",	"tt-TA",	"pa-IN",
"gu-IN",	"ta-IN",	"te-IN",	"kn-IN",
"mr-IN",	"sa-IN",	"mn-MN",	"gl-ES",
"kok-IN",	"syr-SY",	"div-MV"});



    }

    /// <summary>
    /// A traverser that keeps track of things like the current assembly, module, type, and method being validated.
    /// </summary>
    protected internal class ValidatingTraverser : MetadataTraverser {

      /// <summary>
      /// A traverser that keeps track of things like the current assembly, module, type, and method being validated.
      /// </summary>
      protected internal MetadataValidator validator;

      /// <summary>
      /// Traverses the given assembly.
      /// </summary>
      public override void TraverseChildren(IAssembly assembly) {
        this.validator.currentAssembly = assembly;
        this.validator.currentModule = assembly;
        this.validator.currentDefinition = assembly;
        base.TraverseChildren(assembly);
      }

      /// <summary>
      /// Traverses the given method definition.
      /// </summary>
      public override void TraverseChildren(IMethodDefinition method) {
        var savedCurrentDefinition = this.validator.currentDefinition;
        this.validator.currentDefinition = method;
        base.TraverseChildren(method);
        this.validator.currentDefinition = savedCurrentDefinition;
      }

      /// <summary>
      /// Traverses the given module.
      /// </summary>
      /// <param name="module"></param>
      public override void TraverseChildren(IModule module) {
        this.validator.currentModule = module;
        this.validator.currentDefinition = module;
        base.TraverseChildren(module);
      }

      /// <summary>
      /// Traverses the security attribute.
      /// </summary>
      public override void TraverseChildren(ISecurityAttribute securityAttribute) {
        this.validator.currentSecurityAttribute = securityAttribute;
        base.TraverseChildren(securityAttribute);
        this.validator.currentSecurityAttribute = null;
      }

      /// <summary>
      /// Traverses the specified type definition.
      /// </summary>
      /// <param name="typeDefinition"></param>
      public override void TraverseChildren(ITypeDefinition typeDefinition) {
        base.TraverseChildren(typeDefinition);
      }


    }

    /// <summary>
    /// An enumeration of errors that can occur in a metadata model.
    /// </summary>
    protected enum MetadataError {
      /// <summary>
      /// An abstract method may not be marked as being sealed or as a platform invoke method or as being a forward reference.
      /// </summary>
      AbstractMethodMayNotBeSealedPlatformInvokeOrForwardReference,
      /// <summary>
      /// An abstract method must be marked as being virtual.
      /// </summary>
      AbstractMethodsMustBeVirtual,
      /// <summary>
      /// An event's (property's) accessor list must be consistent with its Adder/Remover/Caller (Getter/Setter).
      /// </summary>
      AccessorListInconsistent,
      /// <summary>
      /// The type referenced by this alias does not come from a module of this assembly.
      /// </summary>
      AliasedTypeDoesNotBelongToAModule,
      /// <summary>
      /// It makes no sense for the marshalling information for a field to specify which parameter to use for the element count of an array value.
      /// </summary>
      ArraysMarshalledToFieldsCannotSpecifyElementCountParameter,
      /// <summary>
      /// The size of a type can only be specified if the type does not have its LayoutKind set to Auto.
      /// </summary>
      AutoLayoutTypesCannotSpecifySize,
      /// <summary>
      /// This field is a compile time constant but it is not marked as static.
      /// </summary>
      ConstantFieldMustBeStatic,
      /// <summary>
      /// A constraint on a generic parameter may not reference System.Void.
      /// </summary>
      ConstraintMayNotBeVoid,
      /// <summary>
      /// Constructor methods may not be members of interface types.
      /// </summary>
      ConstructorInInterface,
      /// <summary>
      /// Constructor methods may not return values. I.e. their return type must be System.Void.
      /// </summary>
      ConstructorsMustNotReturnValues,
      /// <summary>
      /// This type alias is referenced as the parent of an exported type alias, but does not itself appear in the ExportedTypes property of the assembly.
      /// </summary>
      ContainingAliasNotListedInExportedTypes,
      /// <summary>
      /// This namespace member is being visited before its parent is being visited, which can only happen if its ContainingNamespace value is not valid.
      /// </summary>
      ContainingNamespaceDefinitionNotVisited,
      /// <summary>
      /// This type member is being visited before its parent is being visited, which can only happen if its ContainingTypeDefinition value is not valid.
      /// </summary>
      ContainingTypeDefinitionNotVisited,
      /// <summary>
      /// The custom attribute's Constructor property references a method that resolves to something other than a constructor.
      /// </summary>
      CustomAttributeConstructorIsBadReference,
      /// <summary>
      /// The Type of the custom attribute is not the same as the containing type of the custom attribute's Constructor.
      /// </summary>
      CustomAttributeTypeIsNotConstructorContainer,
      /// <summary>
      /// Declarative security annotations on interface types are ignored by the CLR.
      /// </summary>
      DeclarativeSecurityOnInterfacesIsIgnored,
      /// <summary>
      /// A derived type with LayoutKind other than Auto must have the same LayoutKind as its base type, unless its base type is System.Object.
      /// </summary>
      DerivedTypeHasDifferentLayoutFromBaseType,
      /// <summary>
      /// The module's assembly references list contains a duplicate entry.
      /// </summary>
      DuplicateAssemblyReference,
      /// <summary>
      /// This constraint has already been encountered during the traversal of its defining generic parameters' Constraints collection.
      /// </summary>
      DuplicateConstraint,
      /// <summary>
      /// This definition has already been encountered during the traversal of the module being validated.
      /// </summary>
      DuplicateDefinition,
      /// <summary>
      /// This event is listed more than once, or has the same name as another event of its containing type definition.
      /// </summary>
      DuplicateEvent,
      /// <summary>
      /// The list of types returned by calling GetAllTypes on the module being validated contains a duplicate entry.
      /// </summary>
      DuplicateEntryInAllTypes,
      /// <summary>
      /// This field is listed more than once, or has the same name and signature as another field of its containing type definition.
      /// </summary>
      DuplicateField,
      /// <summary>
      /// This file reference instance occurs more than once, or has the same file name as another file reference in the Files collection of its contaiing assembly.
      /// </summary>
      DuplicateFileReference,
      /// <summary>
      /// This generic type parameter occurs more than once, or has the same InternedKey as another type parameter, in the GenericParameters collection of its defining type.
      /// </summary>
      DuplicateGenericTypeParameter,
      /// <summary>
      /// This interface occurs more than once, or there is another interface with the same InternedKey, in the Interfaces collection of the type definition.
      /// </summary>
      DuplicateInterface,
      /// <summary>
      /// This method occurs more than once, or there is another method with the same InternedKeay, in the Methods collection of its type definition.
      /// </summary>
      DuplicateMethod,
      /// <summary>
      /// This generic method type parameter occurs more than once, or has the same InternedKey as another type parameter, in the GenericParameters collection of its defining method.
      /// </summary>
      DuplicateMethodGenericTypeParameter,
      /// <summary>
      /// This resource reference occurs more than once, or there is another reference with the same name, in the Resources collection of the assembly.
      /// </summary>
      DuplicateResource,
      /// <summary>
      /// This node may not have an empty name.
      /// </summary>
      EmptyName,
      /// <summary>
      /// The actual number of elements is not the same as the number of elements specified by the {0} property.
      /// </summary>
      EnumerationCountIsInconsistentWithCountProperty,
      /// <summary>
      /// An enum type must have a single instance field of an integral type.
      /// </summary>
      EnumDoesNotHaveAnInstanceField,
      /// <summary>
      /// An enum type may only have a single instance field.
      /// </summary>
      EnumInstanceFieldNotUnique,
      /// <summary>
      /// The instance field of an enum must be of an integral type.
      /// </summary>
      EnumInstanceFieldTypeNotIntegral,
      /// <summary>
      /// The method used for an event/property accessor should follow the standard naming patterns.
      /// </summary>
      EventPropertyNamingPatternWarning,
      /// <summary>
      /// The type of an event may not be an interface or a value type.
      /// </summary>
      EventTypeMustBeClass,
      /// <summary>
      /// This type alias references a type that is defined in the module that contains the assembly manifest. The public types
      /// of such modules are already exported and should not be explicitly exported via the ExportedTypes collection of the assembly.
      /// </summary>
      ExportedTypeBelongsToManifestModule,
      /// <summary>
      /// This method implementation (or explicit override) has an implemeting method that does not match the signature of the implemented method.
      /// </summary>
      ExplicitOverrideDoesNotMatchSignatureOfOverriddenMethod,
      /// <summary>
      /// A field may not be both a compile time constant and readonly.
      /// </summary>
      FieldMayNotBeConstantAndReadonly,
      /// <summary>
      /// The offset of this field must be naturally aligned because its value is an object reference.
      /// </summary>
      FieldOffsetNotNaturallyAlignedForObjectRef,
      /// <summary>
      /// This field reference resolves to a field definition with a different InternedKey value.
      /// </summary>
      FieldReferenceResolvesToDifferentField,
      /// <summary>
      /// This method is a constructor but is also generic. That is not allowed.
      /// </summary>
      GenericConstructor,
      /// <summary>
      /// The value of GenericParameterCount does not match the number of parameters in the GenericParameters collection.
      /// </summary>
      GenericParameterCountDoesNotMatchGenericParameters,
      /// <summary>
      /// A type definition is being visited that is not an element of the list returned by calling GetAllTypes on the module being validated.
      /// </summary>
      GetAllTypesIsIncomplete,
      /// <summary>
      /// This global field is not marked static.
      /// </summary>
      GlobalFieldNotStatic,
      /// <summary>
      /// The node has not been fully initialized. Property {0} has a dummy value.
      /// </summary>
      IncompleteNode,
      /// <summary>
      /// Peverify complains if init locals is not set but there are local variables.
      /// </summary>
      InitLocalsMustBeTrueIfLocalVariables,
      /// <summary>
      /// An instance constructor (a method with the name .ctor) may not be marked as static. After all, it is supposed initialize its this object...
      /// </summary>
      InstanceConstructorMayNotBeStatic,
      /// <summary>
      /// The given type alignment, {0}, is invalid. A valid alignment is one of 0, 1, 2, 4, 8, 16, 32, 64, 128.
      /// </summary>
      InvalidAlignment,
      /// <summary>
      /// The assembly culture string "{0}" does not match one of the strings allowed by the specification.
      /// </summary>
      InvalidCulture,
      /// <summary>
      /// Only references to named types may appear in custom modifiers.
      /// </summary>
      InvalidCustomModifier,
      /// <summary>
      /// This global field has a visibility that only makes sense for type members.
      /// </summary>
      InvalidGlobalFieldVisibility,
      /// <summary>
      /// The value of a IMetadataConstant instance must be a bool, char, number, string, or a null object. 
      /// </summary>
      InvalidMetadataConstant,
      /// <summary>
      /// Bad module metadata format version. Found '{0}', should be at least 2.
      /// </summary>
      InvalidMetadataFormatVersionForGenerics,
      /// <summary>
      /// The PInvokeCallingConvention property
      /// </summary>
      InvalidPInvokeCallingConvention,
      /// <summary>
      /// The security action value is not valid.
      /// </summary>
      InvalidSecurityAction,
      /// <summary>
      /// The value of the ITypeMember.Visibility is not valid.
      /// </summary>
      InvalidTypeMemberVisibility,
      /// <summary>
      /// This field is mapped to a static data area in its PE file, but its type is not a value type or it has fields
      /// that are not public or that contain pointers into the managed heap.
      /// </summary>
      MappedFieldDoesNotHaveAValidType,
      /// <summary>
      /// Fields whose values are marshalled to unmanaged arrays, must specify the size of the array at compile time 
      /// since there is no place in the unmanaged array for the marshaller to store the number of elements.
      /// </summary>
      MarshalledArraysMustHaveSizeKnownAtCompileTime,
      /// <summary>
      /// IMarshallingInformation.{0} has an invalid value.
      /// </summary>
      MarshallingInformationIsInvalid,
      /// <summary>
      /// The method being implemented by an explicit override is not visible to the class doing the overriding, which is not allowed.
      /// </summary>
      MayNotOverrideInaccessibleMethod,
      /// <summary>
      /// This member definition thinks its containing definition is a different object than the one listing it as a member.
      /// </summary>
      MemberDisagreesAboutContainer,
      /// <summary>
      /// This metadata constant value has a type that is incompatible with the type of the field, parameter or property.
      /// </summary>
      MetadataConstantTypeMismatch,
      /// <summary>
      /// The value of GenericParameterCount does not match the number of parameters in the GenericParameters collection of the method.
      /// </summary>
      MethodGenericTypeParameterCountMismatch,
      /// <summary>
      /// This method definition is marked has having declarative security, but it has no security attributes and no SuppressUnmanagedCodeSecurityAttribute.
      /// </summary>
      MethodMarkedAsHavingDeclarativeSecurityHasNoSecurityAttributes,
      /// <summary>
      /// A method that are called with an explicit this parameter (via a function pointer) must actually have a this parameter.
      /// </summary>
      MethodsCalledWithExplicitThisParametersMustNotBeStatic,
      /// <summary>
      /// This method is not virtual, or it has no body, so it cannot serve as the explicit override or implementation of a base class or interface method.
      /// </summary>
      MethodCannotBeAnOverride,
      /// <summary>
      /// A method named .ctor or .cctor must also be marked as special name and runtime special.
      /// </summary>
      MethodsNamedLikeConstructorsMustBeMarkedAsRuntimeSpecial,
      /// <summary>
      /// The value of ParameterCount does not match the number of parameters in the Parameters collection.
      /// </summary>
      MethodParameterCountDoesNotAgreeWithTheActualNumberOfParameters,
      /// <summary>
      /// The method definition is not marked as having declarative security, but it has security attributes or a SuppressUnmanagedCodeSecurityAttribute.
      /// </summary>
      MethodWithSecurityAttributesMustBeMarkedAsHavingDeclarativeSecurity,
      /// <summary>
      /// The named argument's name does not match the name of the field or property that the argument resolves to.
      /// </summary>
      NamedArgumentNameDoesNotMatchNameOfResolvedFieldOrProperty,
      /// <summary>
      /// The named argument's Type does not match the Type of the field or property that the argument resolves to.
      /// </summary>
      NamedArgumentTypeDoesNotMatchTypeOfResolvedFieldOrProperty,
      /// <summary>
      /// A can only be aliased (exported) if it is public.
      /// </summary>
      NonPublicTypeAlias,
      /// <summary>
      /// This method has platform invoke information but it is not marked as static.
      /// </summary>
      NonStaticPlatformInvokeMethod,
      /// <summary>
      /// The assembly name "{0}" is not POSIX compliant because it contains a colon, forward-slash, backslash, or period.
      /// </summary>
      NotPosixAssemblyName,
      /// <summary>
      /// The name "{0}" is not POSIX compliant because it contains a colon, forward-slash, backslash.
      /// </summary>
      NotPosixName,
      /// <summary>
      /// The size of the array passed in this parameter is specified explicitly as well as via another (size) parameter. This is probably a mistake.
      /// </summary>
      NumberOfElementsSpecifiedExplicitlyAsWellAsByAParameter,
      /// <summary>
      /// Only types that have LayoutKind set to SequentialLayout are permitted to specify a non zero value for Alignment.
      /// </summary>
      OnlySequentialLayoutTypesCanSpecificyAlignment,
      /// <summary>
      /// Only field values can be marshalled as ByVal (fixed length) arrays.
      /// </summary>
      ParameterCannotBeMarshalledAsByValArray,
      /// <summary>
      /// The index of the parameter to contain the size of the variable portion of an array that is marshalled as an unmanaged array is out of range.
      /// </summary>
      ParameterIndexIsInvalid,
      /// <summary>
      /// Parameters whose values are marshalled to unmanaged arrays and for which the marshalling information does not specify a parameter to 
      /// convey the number of array elements, must specify the size of the array at compile time 
      /// since there is no place in the unmanaged array for the marshaller to store the number of elements.
      /// </summary>
      ParameterMarshalledArraysMustHaveSizeKnownAtCompileTime,
      /// <summary>
      /// Imported type aliases may not appear directly in the metadata model. Instead refer to the aliased type
      /// via the value of the IAliasForType.AliasedType property.
      /// </summary>
      ReferenceToTypeAlias,
      /// <summary>
      /// The given type member reference resolves to a type member (i.e. method or field) that has Other visibility (visible only to the compiler).
      /// Such members cannot be referenced across modules and intra module references must use the definitions directly.
      /// </summary>
      ReferenceToTypeMemberWithOtherVisibility,
      /// <summary>
      /// A RuntimeSpecial member must also have its SpecialName flag set.
      /// </summary>
      RuntimeSpecialMustAlsoBeSpecialName,
      /// <summary>
      /// It only makes sense to mark a method as sealed, new slot or access checked on override, if the method is virtual.
      /// </summary>
      SealedNewSlotOrOverrideMethodsMustAlsoBeVirtual,
      /// <summary>
      /// The security action of the security attribute is not compatible with the definition to which the attribute is applied.
      /// </summary>
      SecurityActionMismatch,
      /// <summary>
      /// Security attributes on interfaces are ignored by the security system.
      /// </summary>
      SecurityAttributeOnInterface,
      /// <summary>
      /// An assembly may not reference the file that contains the assembly's manifest module (the one being analyzed).
      /// </summary>
      SelfReference,
      /// <summary>
      /// A single file assembly may not explicitly export types. The public types of the manifest module of any assembly are exported by default.
      /// </summary>
      SingleFileAssemblyHasExportedTypes,
      /// <summary>
      /// A method marked as SpecialName or as RuntimeSpecial may not be visible only to the compiler.
      /// </summary>
      SpecialMethodsMayNotHaveCompilerControlledVisibility,
      /// <summary>
      /// A static constructor may not have any parameters since it is invoked implicitly.
      /// </summary>
      StaticConstructorMayNotHaveParameters,
      /// <summary>
      /// Uhm, a static constructor (a method with the name .cctor) must be marked as being static, because well, its a static constructor.
      /// </summary>
      StaticConstructorMustBeStatic,
      /// <summary>
      /// A static field should not have layout information since it does not contribute to the layout of its containing type.
      /// </summary>
      StaticFieldsMayNotHaveLayout,
      /// <summary>
      /// A static method may not be marked as sealed, virtual, new slot.
      /// </summary>
      StaticMethodMayNotBeSealedVirtualOrNewSlot,
      /// <summary>
      /// The given size, {0}, is too large for a value type (struct). The size must be less than 1 MByte (0x100000).
      /// </summary>
      StructsSizeMustBeLessThanOneMegaByte,
      /// <summary>
      /// A struct must have a positive size.
      /// </summary>
      StructSizeMustBeNonZero,
      /// <summary>
      /// A method defined by a value type operates on an object without identity, therefore it makes no sense to mark it as synchronized.
      /// </summary>
      SynchronizedValueTypeMethod,
      /// <summary>
      /// Resolving the type reference results in a type with a different InternedKey from the type reference and no aliasing is involved.
      /// </summary>
      TypeReferenceResolvesToDifferentType,
      /// <summary>
      /// Resolving the type reference results in a type with a different InternedKey from the resolved value of the TypeAlias property.
      /// </summary>
      TypeReferenceResolvesToDifferentTypeFromAlias,
      /// <summary>
      /// This type alias member is not itself a nested type alias.
      /// </summary>
      UnexpectedAliasMember,
      /// <summary>
      /// The assembly's Flags property has bits set that are not valid according to the specification.
      /// </summary>
      UnknownAssemblyFlags,

    }

    /// <summary>
    /// Error information relating to a node in a metadata model.
    /// </summary>
    protected sealed class ErrorMessage : IErrorMessage {

      /// <summary>
      /// The object reporting the error. This can be used to filter out errors coming from non interesting sources.
      /// </summary>
      public object ErrorReporter { get; internal set; }

      /// <summary>
      /// A short identifier for the reporter of the error, suitable for use in human interfaces. For example "CS" in the case of a C# language error.
      /// </summary>
      public string ErrorReporterIdentifier {
        get { return "MDV"; }
      }

      /// <summary>
      /// The error this message pertains to.
      /// </summary>
      public MetadataError Error { get; internal set; }

      /// <summary>
      /// A code that corresponds to this error. This code is the same for all cultures.
      /// </summary>
      public long Code {
        get { return (long)this.Error; }
      }

      /// <summary>
      /// True if the error message should be treated as an informational warning rather than as an indication that the associated
      /// compilation has failed and no useful executable output has been generated. The value of this property does
      /// not depend solely on this.Code but can be influenced by compiler options such as the csc /warnaserror option.
      /// </summary>
      public bool IsWarning {
        get {
          switch (this.Error) {
            case MetadataError.MetadataConstantTypeMismatch:
              return true;
            case MetadataError.NumberOfElementsSpecifiedExplicitlyAsWellAsByAParameter:
              return true;
            case MetadataError.InitLocalsMustBeTrueIfLocalVariables:
            case MetadataError.EventPropertyNamingPatternWarning:
              return true;
            default:
              return false;
          }
        }
      }

      /// <summary>
      /// A description of the error suitable for user interaction. Localized to the current culture.
      /// </summary>
      public string Message {
        get {
          System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("Microsoft.Cci.MetadataHelper.ErrorMessages", typeof(ErrorMessage).Assembly);
          string messageKey = this.Error.ToString();
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
          if (localizedString == null)
            localizedString = messageKey;
          else if (this.MessageParameter != null)
            localizedString = string.Format(localizedString, this.MessageParameter);
          return localizedString;
        }
      }

      /// <summary>
      /// If not null, this strings parameterizes the error message.
      /// </summary>
      public string/*?*/ MessageParameter { get; internal set; }

      /// <summary>
      /// The location of the error.
      /// </summary>
      public ILocation Location { get; internal set; }

      /// <summary>
      /// Zero ore more locations that are related to this error.
      /// </summary>
      public IEnumerable<ILocation> RelatedLocations { get; internal set; }
    }

    /// <summary>
    /// Provides information about a location (node) in a metadata.
    /// </summary>
    protected sealed class MetadataNode : ILocation {

      /// <summary>
      /// The document containing this location.
      /// </summary>
      public IDocument Document { get; internal set; }

      /// <summary>
      /// A Metadata model object instance.
      /// </summary>
      public object Node { get; internal set; }

      /// <summary>
      /// Returns a <see cref="System.String"/> that represents this instance.
      /// </summary>
      /// <returns>
      /// A <see cref="System.String"/> that represents this instance.
      /// </returns>
      public override string ToString() {
        return this.Node.ToString(); //TODO: something a bit more informative
      }
    }

    /// <summary>
    /// An object that projects an IModule instance as a "document" for the purposes of error reporting.
    /// </summary>
    protected sealed class MetadataDocument : IDocument {

      /// <summary>
      /// The metadata module. Could also be an assembly.
      /// </summary>
      public IModule Module { get; internal set; }

      /// <summary>
      /// The location of the module that is invalid. For example a file system path.
      /// </summary>
      public string Location {
        get { return this.Module.Location; }
      }

      /// <summary>
      /// The name of the module.
      /// </summary>
      public IName Name {
        get { return this.Module.Name; }
      }
    }

    /// <summary>
    /// Traverses the given assembly, checking each visited node for validity according the rules for the object model and Partition II or the ECMA-335 Standard.
    /// </summary>
    /// <param name="assembly">The assembly to validate.</param>
    public virtual void Validate(IAssembly assembly) {
      this.IntializeDocument(assembly);
      this.traverser.Traverse(assembly);
    }

    private void IntializeDocument(IModule moduleToValidate) {
      this.document = new MetadataDocument() { Module = moduleToValidate };
    }
  }

}

