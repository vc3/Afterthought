#if !COMPACTFX
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
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.PeWriterInternal {

  internal struct COR_FIELD_OFFSET {
    public uint ridOfField;
    public uint ulOffset;

    //Only here to shut up the warning about fields never being assigned to.
    internal COR_FIELD_OFFSET(object dummy) {
      this.ridOfField = 0;
      this.ulOffset = 0;
    }
  }

  [ComVisible(true), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BA3FEE4C-ECB9-4e41-83B7-183FA41CD859")]
  unsafe internal interface IMetaDataEmit {
    void SetModuleProps(string szName);
    void Save(string szFile, uint dwSaveFlags);
    void SaveToStream(void* pIStream, uint dwSaveFlags);
    uint GetSaveSize(uint fSave);
    uint DefineTypeDef(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements);
    uint DefineNestedType(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements, uint tdEncloser);
    void SetHandler([MarshalAs(UnmanagedType.IUnknown), In]object pUnk);
    uint DefineMethod(uint td, char* zName, uint dwMethodFlags, byte* pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags);
    void DefineMethodImpl(uint td, uint tkBody, uint tkDecl);
    uint DefineTypeRefByName(uint tkResolutionScope, char* szName);
    uint DefineImportType(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport,
      uint tdImport, IntPtr pAssemEmit);
    uint DefineMemberRef(uint tkImport, string szName, byte* pvSigBlob, uint cbSigBlob);
    uint DefineImportMember(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
      IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent);
    uint DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods);
    void SetClassLayout(uint td, uint dwPackSize, COR_FIELD_OFFSET* rFieldOffsets, uint ulClassSize);
    void DeleteClassLayout(uint td);
    void SetFieldMarshal(uint tk, byte* pvNativeType, uint cbNativeType);
    void DeleteFieldMarshal(uint tk);
    uint DefinePermissionSet(uint tk, uint dwAction, void* pvPermission, uint cbPermission);
    void SetRVA(uint md, uint ulRVA);
    uint GetTokenFromSig(byte* pvSig, uint cbSig);
    uint DefineModuleRef(string szName);
    void SetParent(uint mr, uint tk);
    uint GetTokenFromTypeSpec(byte* pvSig, uint cbSig);
    void SaveToMemory(void* pbData, uint cbData);
    uint DefineUserString(string szString, uint cchString);
    void DeleteToken(uint tkObj);
    void SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags);
    void SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements);
    void SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods);
    uint SetPermissionSetProps(uint tk, uint dwAction, void* pvPermission, uint cbPermission);
    void DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);
    void SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);
    void DeletePinvokeMap(uint tk);
    uint DefineCustomAttribute(uint tkObj, uint tkType, void* pCustomAttribute, uint cbCustomAttribute);
    void SetCustomAttributeValue(uint pcv, void* pCustomAttribute, uint cbCustomAttribute);
    uint DefineField(uint td, string szName, uint dwFieldFlags, byte* pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
    uint DefineProperty(uint td, string szProperty, uint dwPropFlags, byte* pvSig, uint cbSig, uint dwCPlusTypeFlag,
      void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods);
    uint DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
    void SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
    void SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods);
    void SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
    uint DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs);
    void ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)]object pImport);
    uint TranslateSigWithScope(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
      IMetaDataImport import, byte* pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, byte* pvTranslatedSig, uint cbTranslatedSigMax);
    void SetMethodImplFlags(uint md, uint dwImplFlags);
    void SetFieldRVA(uint fd, uint ulRVA);
    void Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)]object pHandler);
    void MergeEnd();
  }

  [ComVisible(true), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
  unsafe internal interface IMetaDataImport {
    [PreserveSig]
    void CloseEnum(uint hEnum);
    uint CountEnum(uint hEnum);
    void ResetEnum(uint hEnum, uint ulPos);
    uint EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeDefs, uint cMax);
    uint EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rImpls, uint cMax);
    uint EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeRefs, uint cMax);
    uint FindTypeDefByName(string szTypeDef, uint tkEnclosingClass);
    Guid GetScopeProps(StringBuilder szName, uint cchName, out uint pchName);
    uint GetModuleFromScope();
    uint GetTypeDefProps(uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags);
    uint GetInterfaceImplProps(uint iiImpl, out uint pClass);
    uint GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName);
    uint ResolveTypeRef(uint tr, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIScope);
    uint EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMembers, uint cMax);
    uint EnumMembersWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMembers, uint cMax);
    uint EnumMethods(ref uint phEnum, uint cl, uint* rMethods, uint cMax);
    uint EnumMethodsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethods, uint cMax);
    uint EnumFields(ref uint phEnum, uint cl, uint* rFields, uint cMax);
    uint EnumFieldsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rFields, uint cMax);
    uint EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rParams, uint cMax);
    uint EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMemberRefs, uint cMax);
    uint EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodBody,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodDecl, uint cMax);
    uint EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rPermission,
      uint cMax);
    uint FindMember(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
    uint FindMethod(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
    uint FindField(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
    uint FindMemberRef(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
    uint GetMethodProps(uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr,
      IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA);
    unsafe uint GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out byte* ppvSigBlob);
    uint EnumProperties(ref uint phEnum, uint td, uint* rProperties, uint cMax);
    uint EnumEvents(ref uint phEnum, uint td, uint* rEvents, uint cMax);
    uint GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags,
      out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=11)] uint[] rmdOtherMethod, uint cMax);
    uint EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rEventProp, uint cMax);
    uint GetMethodSemantics(uint mb, uint tkEventProp);
    uint GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] COR_FIELD_OFFSET[] rFieldOffset, uint cMax, out uint pcFieldOffset);
    unsafe uint GetFieldMarshal(uint tk, out byte* ppvNativeType);
    uint GetRVA(uint tk, out uint pulCodeRVA);
    unsafe uint GetPermissionSetProps(uint pm, out uint pdwAction, out void* ppvPermission);
    unsafe uint GetSigFromToken(uint mdSig, out byte* ppvSig);
    uint GetModuleRefProps(uint mur, StringBuilder szName, uint cchName);
    uint EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rModuleRefs, uint cmax);
    unsafe uint GetTypeSpecFromToken(uint typespec, out byte* ppvSig);
    uint GetNameFromToken(uint tk);
    uint EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rMethods, uint cMax);
    uint GetUserString(uint stk, StringBuilder szString, uint cchString);
    uint GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName);
    uint EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rSignatures, uint cmax);
    uint EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeSpecs, uint cmax);
    uint EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rStrings, uint cmax);
    [PreserveSig]
    int GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam);
    uint EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rCustomAttributes, uint cMax);
    uint GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out void* ppBlob);
    uint FindTypeRef(uint tkResolutionScope, string szName);
    uint GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr,
      out byte* ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out void* ppValue);
    uint GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr,
      out byte* ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out void* ppValue);
    uint GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags,
      out byte* ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out void* ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter,
      out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=14)] uint[] rmdOtherMethod, uint cMax);
    uint GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName,
      out uint pdwAttr, out uint pdwCPlusTypeFlag, out void* ppValue);
    uint GetCustomAttributeByName(uint tkObj, string szName, out void* ppData);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsValidToken(uint tk);
    uint GetNestedClassProps(uint tdNestedClass);
    uint GetNativeCallConvFromSig(void* pvSig, uint cbSig);
    int IsGlobal(uint pd);
  }

  //The emit interface is only needed because the unmanaged pdb writer does a QueryInterface for it and fails if the wrapper does not implement it.
  //None of its methods are called.
  [SuppressUnmanagedCodeSecurity]
  internal class MetadataWrapper : IMetaDataEmit, IMetaDataImport {
    PeWriter writer;

    internal MetadataWrapper(PeWriter writer) {
      this.writer = writer;
    }

    void IMetaDataEmit.SetModuleProps(string szName) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.Save(string szFile, uint dwSaveFlags) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SaveToStream(void* pIStream, uint dwSaveFlags) {
      throw new NotImplementedException();
    }
    uint IMetaDataEmit.GetSaveSize(uint fSave) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineTypeDef(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineNestedType(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements, uint tdEncloser) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetHandler([MarshalAs(UnmanagedType.IUnknown), In]object pUnk) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineMethod(uint td, char* zName, uint dwMethodFlags, byte* pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.DefineMethodImpl(uint td, uint tkBody, uint tkDecl) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineTypeRefByName(uint tkResolutionScope, char* szName) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineImportType(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport,
      uint tdImport, IntPtr pAssemEmit) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineMemberRef(uint tkImport, string szName, byte* pvSigBlob, uint cbSigBlob) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineImportMember(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
      IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetClassLayout(uint td, uint dwPackSize, COR_FIELD_OFFSET* rFieldOffsets, uint ulClassSize) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.DeleteClassLayout(uint td) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetFieldMarshal(uint tk, byte* pvNativeType, uint cbNativeType) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.DeleteFieldMarshal(uint tk) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefinePermissionSet(uint tk, uint dwAction, void* pvPermission, uint cbPermission) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetRVA(uint md, uint ulRVA) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.GetTokenFromSig(byte* pvSig, uint cbSig) {
      throw new NotImplementedException();
    }
    uint IMetaDataEmit.DefineModuleRef(string szName) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetParent(uint mr, uint tk) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.GetTokenFromTypeSpec(byte* pvSig, uint cbSig) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SaveToMemory(void* pbData, uint cbData) {
      throw new NotImplementedException();
    }
    uint IMetaDataEmit.DefineUserString(string szString, uint cchString) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.DeleteToken(uint tkObj) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.SetPermissionSetProps(uint tk, uint dwAction, void* pvPermission, uint cbPermission) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.DeletePinvokeMap(uint tk) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineCustomAttribute(uint tkObj, uint tkType, void* pCustomAttribute, uint cbCustomAttribute) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetCustomAttributeValue(uint pcv, void* pCustomAttribute, uint cbCustomAttribute) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineField(uint td, string szName, uint dwFieldFlags, byte* pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag,
      void* pValue, uint cchValue) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineProperty(uint td, string szProperty, uint dwPropFlags, byte* pvSig, uint cbSig, uint dwCPlusTypeFlag,
      void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods) {
      throw new NotImplementedException();
    }
    unsafe void IMetaDataEmit.SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue) {
      throw new NotImplementedException();
    }
    uint IMetaDataEmit.DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)]object pImport) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataEmit.TranslateSigWithScope(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
      IMetaDataImport import, byte* pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, byte* pvTranslatedSig, uint cbTranslatedSigMax) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetMethodImplFlags(uint md, uint dwImplFlags) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.SetFieldRVA(uint fd, uint ulRVA) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)]object pHandler) {
      throw new NotImplementedException();
    }
    void IMetaDataEmit.MergeEnd() {
      throw new NotImplementedException();
    }
    [PreserveSig]
    void IMetaDataImport.CloseEnum(uint hEnum) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.CountEnum(uint hEnum) {
      throw new NotImplementedException();
    }
    void IMetaDataImport.ResetEnum(uint hEnum, uint ulPos) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeDefs, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rImpls, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeRefs, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.FindTypeDefByName(string szTypeDef, uint tkEnclosingClass) {
      throw new NotImplementedException();
    }
    Guid IMetaDataImport.GetScopeProps(StringBuilder szName, uint cchName, out uint pchName) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetModuleFromScope() {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetTypeDefProps(uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags) {
      pchTypeDef = 0;
      if (td == 0) return 0;
      ITypeReference t = null;
      if ((td & 0xFF000000) == 0x1B000000) {
        t = this.writer.typeSpecList[(int)(td & 0xFFFFFF)-1];
        IGenericTypeInstanceReference gt = t as IGenericTypeInstanceReference;
        if (gt != null) t = gt.GenericType;
      } else
        t = this.writer.typeDefList[(int)(td & 0xFFFFFF)-1];
      string tName;
      uint parentToken = 0;
      if (this.lastTd == td) {
        tName = this.lastTName;
        parentToken = this.lastParentToken;
      } else {
        tName = TypeHelper.GetTypeName(t, NameFormattingOptions.UseGenericTypeNameSuffix|NameFormattingOptions.OmitContainingType);
        this.lastTd = td;
        this.lastTName = tName;
        ITypeReference bc = null;
        foreach (ITypeReference baseClassRef in t.ResolvedType.BaseClasses) bc = baseClassRef;
        if (bc != null) parentToken = (uint)this.writer.GetTypeToken(bc);
        this.lastParentToken = parentToken;
      }
      pchTypeDef = (uint)tName.Length;
      if (pchTypeDef >= cchTypeDef) pchTypeDef = cchTypeDef-1;
      char* pTypeDef = (char*)szTypeDef.ToPointer();
      for (int i = 0; i < pchTypeDef; i++) *(pTypeDef+i) = tName[i];
      *(pTypeDef+pchTypeDef) = (char)0;
      uint* pFlags = (uint*)pdwTypeDefFlags.ToPointer();
      *(pFlags) = PeWriter.GetTypeDefFlags(t.ResolvedType);
      return parentToken;
    }
    uint lastTd;
    string lastTName;
    uint lastParentToken;

    uint IMetaDataImport.GetInterfaceImplProps(uint iiImpl, out uint pClass) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.ResolveTypeRef(uint tr, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIScope) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMembers, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumMembersWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMembers, uint cMax) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.EnumMethods(ref uint phEnum, uint cl, uint* rMethods, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumMethodsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethods, uint cMax) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.EnumFields(ref uint phEnum, uint cl, uint* rFields, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumFieldsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rFields, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rParams, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMemberRefs, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodBody,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodDecl, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rPermission,
      uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.FindMember(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.FindMethod(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.FindField(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.FindMemberRef(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetMethodProps(uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr,
      IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA) {
      IMethodReference m = null;
      if ((mb & 0xFF000000) == 0x0A000000)
        m = this.writer.memberRefList[(int)(mb & 0xFFFFFF)-1] as IMethodReference;
      else
        m = this.writer.methodDefList[(int)(mb & 0xFFFFFF)-1];
      pchMethod = 0;
      pClass = 0;
      pClass = this.writer.GetTypeToken(m.ContainingType);
      string methName = m.Name.Value;
      pchMethod = (uint)methName.Length;
      if (pchMethod > cchMethod) pchMethod = cchMethod-1;
      char* pMethName = (char*)szMethod.ToPointer();
      for (int i = 0; i < pchMethod; i++) *(pMethName+i) = methName[i];
      *(pMethName+pchMethod) = (char)0;
      return 0;
    }
    unsafe uint IMetaDataImport.GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out byte* ppvSigBlob) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.EnumProperties(ref uint phEnum, uint td, uint* rProperties, uint cMax) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.EnumEvents(ref uint phEnum, uint td, uint* rEvents, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags,
      out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=11)] uint[] rmdOtherMethod, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rEventProp, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetMethodSemantics(uint mb, uint tkEventProp) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] COR_FIELD_OFFSET[] rFieldOffset, uint cMax, out uint pcFieldOffset) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetFieldMarshal(uint tk, out byte* ppvNativeType) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetRVA(uint tk, out uint pulCodeRVA) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetPermissionSetProps(uint pm, out uint pdwAction, out void* ppvPermission) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetSigFromToken(uint mdSig, out byte* ppvSig) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetModuleRefProps(uint mur, StringBuilder szName, uint cchName) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rModuleRefs, uint cmax) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetTypeSpecFromToken(uint typespec, out byte* ppvSig) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetNameFromToken(uint tk) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rMethods, uint cMax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetUserString(uint stk, StringBuilder szString, uint cchString) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rSignatures, uint cmax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeSpecs, uint cmax) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rStrings, uint cmax) {
      throw new NotImplementedException();
    }
    [PreserveSig]
    int IMetaDataImport.GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rCustomAttributes, uint cMax) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out void* ppBlob) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.FindTypeRef(uint tkResolutionScope, string szName) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr,
      out byte* ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out void* ppValue) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr,
      out byte* ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out void* ppValue) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags,
      out byte* ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out void* ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter,
      out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=14)] uint[] rmdOtherMethod, uint cMax) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName,
      out uint pdwAttr, out uint pdwCPlusTypeFlag, out void* ppValue) {
      throw new NotImplementedException();
    }
    unsafe uint IMetaDataImport.GetCustomAttributeByName(uint tkObj, string szName, out void* ppData) {
      throw new NotImplementedException();
    }
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IMetaDataImport.IsValidToken(uint tk) {
      throw new NotImplementedException();
    }
    uint IMetaDataImport.GetNestedClassProps(uint tdNestedClass) {
      ITypeReference t = null;
      if ((tdNestedClass & 0xFF000000) == 0x1B000000)
        t = this.writer.typeSpecList[(int)(tdNestedClass & 0xFFFFFF)-1];
      else
        t = this.writer.typeDefList[(int)(tdNestedClass & 0xFFFFFF)-1];
      INestedTypeReference nt = t as INestedTypeReference;
      if (nt == null) return 0;
      return this.writer.GetTypeToken(nt.ContainingType);
    }
    unsafe uint IMetaDataImport.GetNativeCallConvFromSig(void* pvSig, uint cbSig) {
      throw new NotImplementedException();
    }
    int IMetaDataImport.IsGlobal(uint pd) {
      throw new NotImplementedException();
    }
  }
}
#endif
