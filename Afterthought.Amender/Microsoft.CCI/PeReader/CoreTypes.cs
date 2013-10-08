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
using Microsoft.Cci.MetadataReader.PEFile;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.MetadataReader {
  using Microsoft.Cci.MetadataReader.ObjectModelImplementation;
  using Microsoft.Cci.MetadataReader.PEFileFlags;

  /// <summary>
  /// These types are used to implement properties such as ITypeDefinition.IsEnum, which relies on checking
  /// that the base type is a well known type form the core assembly (mscorlib, for example). This module
  /// may refer to these types via a reference assembly which forwards type references to the real core assembly.
  /// Since we should not resolve any type references, and since the host's platform types may be referencing
  /// the real core assembly, we need to set up references to types from this module's idea of the core assembly.
  /// </summary>
  internal sealed class CoreTypes {
    internal readonly IMetadataReaderNamedTypeReference SystemEnum;
    internal readonly IMetadataReaderNamedTypeReference SystemValueType;
    internal readonly IMetadataReaderNamedTypeReference SystemMulticastDelegate;
    internal readonly IMetadataReaderNamedTypeReference SystemType;
    internal readonly IMetadataReaderNamedTypeReference SystemParamArrayAttribute;

    //  Caller should lock peFileToObjectModel
    internal CoreTypes(PEFileToObjectModel peFileToObjectModel) {
      INameTable nameTable = peFileToObjectModel.NameTable;
      PEFileReader peFileReader = peFileToObjectModel.PEFileReader;
      PeReader peReader = peFileToObjectModel.ModuleReader;
      Module module = peFileToObjectModel.Module;
      AssemblyIdentity/*?*/ assemblyIdentity = module.ModuleIdentity as AssemblyIdentity;

      //This does more than just initialize the five types exposed above, since it is also
      //necessary to initialize any typedefs and typerefs to types with short forms
      //in such a way that they have the correct type codes.

      int systemName = nameTable.System.UniqueKey;
      int voidName = nameTable.Void.UniqueKey;
      int booleanName = nameTable.Boolean.UniqueKey;
      int charName = nameTable.Char.UniqueKey;
      int byteName = nameTable.Byte.UniqueKey;
      int sByteName = nameTable.SByte.UniqueKey;
      int int16Name = nameTable.Int16.UniqueKey;
      int uint16Name = nameTable.UInt16.UniqueKey;
      int int32Name = nameTable.Int32.UniqueKey;
      int uint32Name = nameTable.UInt32.UniqueKey;
      int int64Name = nameTable.Int64.UniqueKey;
      int uint64Name = nameTable.UInt64.UniqueKey;
      int stringName = nameTable.String.UniqueKey;
      int intPtrName = nameTable.IntPtr.UniqueKey;
      int uintPtrName = nameTable.UIntPtr.UniqueKey;
      int objectName = nameTable.Object.UniqueKey;
      int singleName = nameTable.Single.UniqueKey;
      int doubleName = nameTable.Double.UniqueKey;
      int decimalName = nameTable.Decimal.UniqueKey;
      int typedReference = nameTable.TypedReference.UniqueKey;
      int enumName = nameTable.Enum.UniqueKey;
      int valueTypeName = nameTable.ValueType.UniqueKey;
      int multicastDelegateName = nameTable.MulticastDelegate.UniqueKey;
      int typeName = nameTable.Type.UniqueKey;
      int arrayName = nameTable.Array.UniqueKey;
      int paramArrayAttributeName = peReader.ParamArrayAttribute.UniqueKey;
      if (assemblyIdentity != null && assemblyIdentity.Equals(peReader.metadataReaderHost.CoreAssemblySymbolicIdentity)) {
        peReader.RegisterCoreAssembly(module as Assembly);
        uint numberOfTypeDefs = peFileReader.TypeDefTable.NumberOfRows;
        for (uint i = 1; i <= numberOfTypeDefs; ++i) {
          TypeDefRow typeDefRow = peFileReader.TypeDefTable[i];
          if (!typeDefRow.IsNested) {
            int namespaceName = peFileToObjectModel.GetNameFromOffset(typeDefRow.Namespace).UniqueKey;
            if (namespaceName == systemName) {
              int typeDefName = peFileToObjectModel.GetNameFromOffset(typeDefRow.Name).UniqueKey;
              if (typeDefName == voidName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Void);
              else if (typeDefName == booleanName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Boolean);
              else if (typeDefName == charName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Char);
              else if (typeDefName == byteName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Byte);
              else if (typeDefName == sByteName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.SByte);
              else if (typeDefName == int16Name)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Int16);
              else if (typeDefName == uint16Name)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.UInt16);
              else if (typeDefName == int32Name)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Int32);
              else if (typeDefName == uint32Name)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.UInt32);
              else if (typeDefName == int64Name)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Int64);
              else if (typeDefName == uint64Name)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.UInt64);
              else if (typeDefName == stringName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.String);
              else if (typeDefName == intPtrName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.IntPtr);
              else if (typeDefName == uintPtrName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.UIntPtr);
              else if (typeDefName == objectName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Object);
              else if (typeDefName == singleName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Single);
              else if (typeDefName == doubleName)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.Double);
              else if (typeDefName == typedReference)
                peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.TypedReference);
              else if (typeDefName == enumName)
                this.SystemEnum = peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
              else if (typeDefName == valueTypeName)
                this.SystemValueType = peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
              else if (typeDefName == multicastDelegateName)
                this.SystemMulticastDelegate = peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
              else if (typeDefName == typeName)
                this.SystemType = peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
              else if (typeDefName == paramArrayAttributeName)
                this.SystemParamArrayAttribute = peFileToObjectModel.GetPredefinedTypeDefinitionAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
            }
          }
        }
      } else {
        uint numberOfTypeRefs = peFileReader.TypeRefTable.NumberOfRows;
        AssemblyReference/*?*/ coreAssemblyRef = peFileToObjectModel.FindAssemblyReference(peFileToObjectModel.CoreAssemblySymbolicIdentity);
        if (coreAssemblyRef == null) {
          //  Error...
          coreAssemblyRef = new AssemblyReference(peFileToObjectModel, 1, peFileToObjectModel.CoreAssemblySymbolicIdentity, AssemblyFlags.Retargetable);
        }
        uint coreAssemblyRefToken = coreAssemblyRef.TokenValue;
        for (uint i = 1; i <= numberOfTypeRefs; ++i) {
          TypeRefRow typeRefRow = peFileReader.TypeRefTable[i];
          if (typeRefRow.ResolutionScope != coreAssemblyRefToken)
            continue;
          int namespaceName = peFileToObjectModel.GetNameFromOffset(typeRefRow.Namespace).UniqueKey;
          if (namespaceName == systemName) {
            int typeDefName = peFileToObjectModel.GetNameFromOffset(typeRefRow.Name).UniqueKey;
            if (typeDefName == voidName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Void);
            else if (typeDefName == booleanName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Boolean);
            else if (typeDefName == charName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Char);
            else if (typeDefName == byteName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Byte);
            else if (typeDefName == sByteName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.SByte);
            else if (typeDefName == int16Name)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Int16);
            else if (typeDefName == uint16Name)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.UInt16);
            else if (typeDefName == int32Name)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Int32);
            else if (typeDefName == uint32Name)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.UInt32);
            else if (typeDefName == int64Name)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Int64);
            else if (typeDefName == uint64Name)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.UInt64);
            else if (typeDefName == stringName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.String);
            else if (typeDefName == intPtrName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.IntPtr);
            else if (typeDefName == uintPtrName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.UIntPtr);
            else if (typeDefName == objectName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Object);
            else if (typeDefName == singleName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Single);
            else if (typeDefName == doubleName)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.Double);
            else if (typeDefName == typedReference)
              peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.TypedReference);
            else if (typeDefName == enumName)
              this.SystemEnum = peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
            else if (typeDefName == valueTypeName)
              this.SystemValueType = peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
            else if (typeDefName == multicastDelegateName)
              this.SystemMulticastDelegate = peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
            else if (typeDefName == typeName)
              this.SystemType = peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
            else if (typeDefName == paramArrayAttributeName)
              this.SystemParamArrayAttribute = peFileToObjectModel.GetPredefinedTypeRefReferenceAtRowWorker(i, MetadataReaderSignatureTypeCode.NotModulePrimitive);
          }
        }
        NamespaceReference systemNSR = peFileToObjectModel.GetNamespaceReferenceForString(coreAssemblyRef, nameTable.System);
        if (this.SystemEnum == null || (peFileToObjectModel.SystemEnumAssembly != null && coreAssemblyRef != peFileToObjectModel.SystemEnumAssembly))
          this.SystemEnum = peFileToObjectModel.typeCache.CreateCoreTypeReference(peFileToObjectModel.SystemEnumAssembly??coreAssemblyRef, systemNSR, nameTable.Enum, MetadataReaderSignatureTypeCode.NotModulePrimitive);
        if (this.SystemValueType == null || (peFileToObjectModel.SystemValueTypeAssembly != null && coreAssemblyRef != peFileToObjectModel.SystemValueTypeAssembly))
          this.SystemValueType = peFileToObjectModel.typeCache.CreateCoreTypeReference(peFileToObjectModel.SystemValueTypeAssembly??coreAssemblyRef, systemNSR, nameTable.ValueType, MetadataReaderSignatureTypeCode.NotModulePrimitive);
        if (this.SystemMulticastDelegate == null || (peFileToObjectModel.SystemMulticastDelegateAssembly != null && coreAssemblyRef != peFileToObjectModel.SystemMulticastDelegateAssembly))
          this.SystemMulticastDelegate = peFileToObjectModel.typeCache.CreateCoreTypeReference(peFileToObjectModel.SystemMulticastDelegateAssembly??coreAssemblyRef, systemNSR, nameTable.MulticastDelegate, MetadataReaderSignatureTypeCode.NotModulePrimitive);
        if (this.SystemType == null || (peFileToObjectModel.SystemTypeAssembly != null && coreAssemblyRef != peFileToObjectModel.SystemTypeAssembly))
          this.SystemType = peFileToObjectModel.typeCache.CreateCoreTypeReference(peFileToObjectModel.SystemTypeAssembly??coreAssemblyRef, systemNSR, nameTable.Type, MetadataReaderSignatureTypeCode.NotModulePrimitive);
        if (this.SystemParamArrayAttribute == null || (peFileToObjectModel.SystemParamArrayAttributeAssembly != null && coreAssemblyRef != peFileToObjectModel.SystemParamArrayAttributeAssembly))
          this.SystemParamArrayAttribute = peFileToObjectModel.typeCache.CreateCoreTypeReference(peFileToObjectModel.SystemParamArrayAttributeAssembly??coreAssemblyRef, systemNSR, peReader.ParamArrayAttribute, MetadataReaderSignatureTypeCode.NotModulePrimitive);
      }
    }
  }

}