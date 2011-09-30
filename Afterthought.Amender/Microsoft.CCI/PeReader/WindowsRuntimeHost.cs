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
using Microsoft.Cci;
using Microsoft.Cci.UtilityDataStructures;
using Microsoft.Cci.Immutable;
using System.IO;
using Microsoft.Cci.MetadataReader.ObjectModelImplementation;
using Microsoft.Cci.MetadataReader.PEFileFlags;

namespace Microsoft.Cci.MetadataReader {

  /// <summary>
  /// A simple host environment using default settings inherited from WindowsRuntimeMetadataReaderHost and that
  /// uses PeReader as its metadata reader.
  /// </summary>
  public class DefaultWindowsRuntimeHost : WindowsRuntimeMetadataReaderHost {
    PeReader peReader;

    /// <summary>
    /// Allocates a simple host environment using default settings inherited from WindowsRuntimeMetadataReaderHost and that
    /// uses PeReader as its metadata reader.
    /// </summary>
    /// <param name="projectToCLRTypes">True if the host should project references to certain Windows Runtime types and methods
    /// to corresponding CLR types and methods, in order to emulate the runtime behavior of the CLR.</param>
    public DefaultWindowsRuntimeHost(bool projectToCLRTypes = true)
      : base(new NameTable(), new InternFactory(), 0, null, true, projectToCLRTypes) {
      this.peReader = new PeReader(this);
    }

    /// <summary>
    /// Allocates a simple host environment using default settings inherited from MetadataReaderHost and that
    /// uses PeReader as its metadata reader.
    /// </summary>
    /// <param name="nameTable">
    /// A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.
    /// </param>
    /// <param name="projectToCLRTypes">True if the host should project references to certain Windows Runtime types and methods
    /// to corresponding CLR types and methods, in order to emulate the runtime behavior of the CLR.</param>
    public DefaultWindowsRuntimeHost(INameTable nameTable, bool projectToCLRTypes = true)
      : base(nameTable, new InternFactory(), 0, null, false, projectToCLRTypes) {
      this.peReader = new PeReader(this);
    }

    /// <summary>
    /// Returns the unit that is stored at the given location, or a dummy unit if no unit exists at that location or if the unit at that location is not accessible.
    /// </summary>
    /// <param name="location">A path to the file that contains the unit of metdata to load.</param>
    public override IUnit LoadUnitFrom(string location) {
      IUnit result = this.peReader.OpenModule(
        BinaryDocument.GetBinaryDocumentForFile(location, this));
      this.RegisterAsLatest(result);
      return result;
    }
  }

  /// <summary>
  /// A base class for an object provided by the application hosting the metadata reader. The object allows the host application
  /// to control how assembly references are unified, where files are found, how Windows Runtime types and methods are projected to CLR types and methods
  /// and so on. The object also controls the lifetime of things such as memory mapped files and blocks of unmanaged memory. Be sure to call Dispose on the object when
  /// it is no longer needed and the associated locks and/or memory must be released immediately.
  /// </summary>
  public abstract class WindowsRuntimeMetadataReaderHost : MetadataReaderHost {

    /// <summary>
    /// A base class for an object provided by the application hosting the metadata reader. The object allows the host application
    /// to control how assembly references are unified, where files are found, how Windows Runtime types and methods are projected to CLR types and methods
    /// and so on. The object also controls the lifetime of things such as memory mapped files and blocks of unmanaged memory. Be sure to call Dispose on the object when
    /// it is no longer needed and the associated locks and/or memory must be released immediately.
    /// </summary>
    /// <param name="nameTable">
    /// A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.
    /// </param>
    /// <param name="factory">
    /// The intern factory to use when generating keys. When comparing two or more assemblies using
    /// TypeHelper, MemberHelper, etc. it is necessary to make the hosts use the same intern factory.
    /// </param>
    /// <param name="pointerSize">The size of a pointer on the runtime that is the target of the metadata units to be loaded
    /// into this metadta host. This parameter only matters if the host application wants to work out what the exact layout
    /// of a struct will be on the target runtime. The framework uses this value in methods such as TypeHelper.SizeOfType and
    /// TypeHelper.TypeAlignment. If the host application does not care about the pointer size it can provide 0 as the value
    /// of this parameter. In that case, the first reference to IMetadataHost.PointerSize will probe the list of loaded assemblies
    /// to find an assembly that either requires 32 bit pointers or 64 bit pointers. If no such assembly is found, the default is 32 bit pointers.
    /// </param>
    /// <param name="searchPaths">
    /// A collection of strings that are interpreted as valid paths which are used to search for units.
    /// </param>
    /// <param name="searchInGAC">
    /// Whether the GAC (Global Assembly Cache) should be searched when resolving references.
    /// </param>
    /// <param name="projectToCLRTypes">True if the host should project references to certain Windows Runtime types and methods
    /// to corresponding CLR types and methods, in order to emulate the runtime behavior of the CLR.</param>
    protected WindowsRuntimeMetadataReaderHost(INameTable nameTable, IInternFactory factory, byte pointerSize, IEnumerable<string> searchPaths,
      bool searchInGAC, bool projectToCLRTypes)
      : base(nameTable, factory, pointerSize, searchPaths, searchInGAC) {
      Contract.Requires(pointerSize == 0 || pointerSize == 4 || pointerSize == 8);
      this.projectToCLRTypes = projectToCLRTypes;
      this.AllowMultiple = nameTable.GetNameFor("AllowMultiple");
      this.AllowMultipleAttribute = nameTable.GetNameFor("AllowMultipleAttribute");
      this.Animation = nameTable.GetNameFor("Animation");
      this.Collections = nameTable.GetNameFor("Collections");
      this.Controls = nameTable.GetNameFor("Controls");
      this.CultureName = nameTable.GetNameFor("CultureName");
      this.Data = nameTable.GetNameFor("Data");
      this.Foundation = nameTable.GetNameFor("Foundation");
      this.HResult = nameTable.GetNameFor("HResult");
      this.IBindableIterable = nameTable.GetNameFor("IBindableIterable");
      this.IBindableVector = nameTable.GetNameFor("IBindableVector");
      this.IIterable = nameTable.GetNameFor("IIterable");
      this.IKeyValuePair = nameTable.GetNameFor("IKeyValuePair");
      this.IMap = nameTable.GetNameFor("IMap");
      this.IMapView = nameTable.GetNameFor("IMapView");
      this.INotifyCollectionChanged = nameTable.GetNameFor("INotifyCollectionChanged");
      this.INotifyPropertyChanged = nameTable.GetNameFor("INotifyPropertyChanged");
      this.Interop = nameTable.GetNameFor("Interop");
      this.IReference = nameTable.GetNameFor("IReference");
      this.IVector = nameTable.GetNameFor("IVector");
      this.IVectorView = nameTable.GetNameFor("IVectorView");
      this.Media = nameTable.GetNameFor("Media");
      this.Media3D = nameTable.GetNameFor("Media3D");
      this.Metadata = nameTable.GetNameFor("Metadata");
      this.NotifyCollectionChangedAction = nameTable.GetNameFor("NotifyCollectionChangedAction");
      this.NotifyCollectionChangedEventArgs = nameTable.GetNameFor("NotifyCollectionChangedEventArgs");
      this.NotifyCollectionChangedEventHandler = nameTable.GetNameFor("NotifyCollectionChangedEventHandler");
      this.Primitives = nameTable.GetNameFor("Primitives");
      this.PropertyChangedEventArgs = nameTable.GetNameFor("PropertyChangedEventArgs");
      this.PropertyChangedEventHandler = nameTable.GetNameFor("PropertyChangedEventHandler");
      this.TypeName = nameTable.GetNameFor("TypeName");
      this.UI = nameTable.GetNameFor("UI");
      this.Windows = nameTable.GetNameFor("Windows");
      this.Xaml = nameTable.GetNameFor("Xaml");
    }

    bool projectToCLRTypes;
    IName AllowMultiple;
    IName AllowMultipleAttribute;
    IName Animation;
    IName Collections;
    IName Controls;
    IName CultureName;
    IName Data;
    IName Foundation;
    IName HResult;
    IName IBindableIterable;
    IName IBindableVector;
    IName IIterable;
    IName IKeyValuePair;
    IName IMap;
    IName IMapView;
    IName INotifyCollectionChanged;
    IName INotifyPropertyChanged;
    IName Interop;
    IName IReference;
    IName IVector;
    IName IVectorView;
    IName Media;
    IName Media3D;
    IName Metadata;
    IName NotifyCollectionChangedAction;
    IName NotifyCollectionChangedEventArgs;
    IName NotifyCollectionChangedEventHandler;
    IName Primitives;
    IName PropertyChangedEventArgs;
    IName PropertyChangedEventHandler;
    IName TypeName;
    IName UI;
    IName Windows;
    IName Xaml;
    WindowsRuntimePlatform platformType;

    /// <summary>
    /// Returns an object that provides a collection of references to types from the core platform, such as System.Object and System.String.
    /// </summary>
    /// <returns></returns>
    protected override IPlatformType GetPlatformType() {
      return this.platformType = new WindowsRuntimePlatform(this);
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute one method definition for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="containingUnit">The unit that is defines the method.</param>
    /// <param name="methodDefinition">A method definition encountered during metadata reading.</param>
    /// <returns>
    /// Usually the value in methodDefinition, but occassionally something else.
    /// </returns>
    public override IMethodDefinition Rewrite(IUnit containingUnit, IMethodDefinition methodDefinition) {
      var methodDef = methodDefinition as MethodDefinition;
      if (methodDef == null) return methodDefinition;
      var containingType = methodDefinition.ContainingTypeDefinition as INamespaceTypeDefinition;
      if (containingType == null || !containingType.IsForeignObject) return methodDefinition;
      if (containingType.IsDelegate) {
        methodDef.MethodFlags &= ~MethodFlags.AccessMask;
        methodDef.MethodFlags |= MethodFlags.PublicAccess;
        methodDef.MethodImplFlags |= MethodImplFlags.RuntimeCodeType;
      } else {
        methodDef.MethodImplFlags |= MethodImplFlags.RuntimeCodeType|MethodImplFlags.InternalCall;
      }
      return methodDefinition;
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute one type reference for another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the type.</param>
    /// <param name="typeReference">A type reference encountered during metadata reading.</param>
    /// <returns>
    /// Usually the value in typeReference, but occassionally something else.
    /// </returns>
    public override ITypeReference Redirect(IUnit referringUnit, ITypeReference typeReference) {
      if (!this.projectToCLRTypes) return typeReference;
      var platformType = (WindowsRuntimePlatform)this.PlatformType;
      var referringAssembly = referringUnit as IAssembly;
      if (referringAssembly == null || !(referringAssembly.ContainsForeignTypes)) return typeReference;
      var namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference == null) return typeReference;
      var namespaceReference = namespaceTypeReference.ContainingUnitNamespace as INestedUnitNamespaceReference;
      if (namespaceReference == null) return typeReference;
      if (this.IsWindowsFoundationMetadata(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.SystemAttributeUsageAttribute.Name) return platformType.SystemAttributeUsageAttribute;
        if (namespaceTypeReference.Name == platformType.SystemAttributeTargets.Name) return platformType.SystemAttributeTargets;
      } else if (this.IsWindowsFoundation(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.WindowsFoundationColor.Name) return platformType.WindowsFoundationColor;
        if (namespaceTypeReference.Name == platformType.SystemDateTime.Name) return platformType.SystemDateTimeOffset;
        if (namespaceTypeReference.Name == platformType.SystemEventHandler1.Name && namespaceTypeReference.GenericParameterCount == 1)
          return platformType.SystemEventHandler1;
        if (namespaceTypeReference.Name == platformType.SystemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken.Name)
          return platformType.SystemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken;
        if (namespaceTypeReference.Name == this.HResult) return platformType.SystemException;
        if (namespaceTypeReference.Name == this.IReference && namespaceTypeReference.GenericParameterCount == 1)
          return platformType.SystemNullable1;
        if (namespaceTypeReference.Name == platformType.WindowsFoundationPoint.Name) return platformType.WindowsFoundationPoint;
        if (namespaceTypeReference.Name == platformType.WindowsFoundationRect.Name) return platformType.WindowsFoundationRect;
        if (namespaceTypeReference.Name == platformType.WindowsFoundationSize.Name) return platformType.WindowsFoundationSize;
        if (namespaceTypeReference.Name == platformType.SystemTimeSpan.Name) return platformType.SystemTimeSpan;
        if (namespaceTypeReference.Name == platformType.SystemUri.Name) return platformType.SystemUri;
      } else if (this.IsWindowsFoundationCollections(namespaceReference)) {
        if (namespaceTypeReference.Name == this.IIterable) return platformType.SystemCollectionsGenericIEnumerable;
        if (namespaceTypeReference.Name == this.IVector) return platformType.SystemCollectionsGenericIList;
        if (namespaceTypeReference.Name == this.IVectorView) return platformType.SystemCollectionsGenericReadOnlyList;
        if (namespaceTypeReference.Name == this.IMap) return platformType.SystemCollectionsGenericIDictionary;
        if (namespaceTypeReference.Name == this.IMapView) return platformType.SystemCollectionsGenericReadOnlyDictionary;
        if (namespaceTypeReference.Name == this.IKeyValuePair) return platformType.SystemCollectionsGenericKeyValuePair;
      } else if (this.IsWindowsUIXaml(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUICornerRadius.Name) return platformType.WindowsUIDirectUICornerRadius;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIDuration.Name) return platformType.WindowsUIDirectUIDuration;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIDurationType.Name) return platformType.WindowsUIDirectUIDurationType;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIGridLength.Name) return platformType.WindowsUIDirectUIGridLength;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIGridUnitType.Name) return platformType.WindowsUIDirectUIGridUnitType;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIThickness.Name) return platformType.WindowsUIDirectUIThickness;
      } else if (this.IsWindowsUIXamlData(namespaceReference)) {
        if (namespaceTypeReference.Name == this.INotifyPropertyChanged) return platformType.SystemComponentModelINotifyPropertyChanged;
        if (namespaceTypeReference.Name == this.PropertyChangedEventArgs) return platformType.SystemComponentModelPropertyChangedEventArgs;
        if (namespaceTypeReference.Name == this.PropertyChangedEventHandler) return platformType.SystemComponentModelPropertyChangedEventHandler;
      } else if (this.IsWindowsUIXamlInterop(namespaceReference)) {
        if (namespaceTypeReference.Name == this.CultureName) return platformType.SystemGlobalizationCultureInfo;
        if (namespaceTypeReference.Name == this.IBindableIterable) return platformType.SystemCollectionsIEnumerable;
        if (namespaceTypeReference.Name == this.IBindableVector) return platformType.SystemCollectionsIList;
        if (namespaceTypeReference.Name == this.INotifyCollectionChanged) return platformType.SystemCollectionsSpecializedINotifyColletionChanged;
        if (namespaceTypeReference.Name == this.NotifyCollectionChangedAction) return platformType.SystemCollectionsSpecializedNotifyCollectionChangedAction;
        if (namespaceTypeReference.Name == this.NotifyCollectionChangedEventArgs) return platformType.SystemCollectionsSpecializedNotifyCollectionChangedEventArgs;
        if (namespaceTypeReference.Name == this.NotifyCollectionChangedEventHandler) return platformType.SystemCollectionsSpecializedNotifyCollectionChangedEventHandler;
        if (namespaceTypeReference.Name == this.TypeName) return platformType.SystemType;
      } else if (this.IsWindowsUIXamlControlsPrimitives(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIControlsPrimitivesGeneratorPosition.Name)
          return platformType.WindowsUIDirectUIControlsPrimitivesGeneratorPosition;
      } else if (this.IsWindowsUIXamlMedia(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIMediaMatrix.Name) return platformType.WindowsUIDirectUIMediaMatrix;
      } else if (this.IsWindowsUIXamlMediaAnimation(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIMediaAnimationKeyTime.Name)
          return platformType.WindowsUIDirectUIMediaAnimationKeyTime;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIMediaAnimationRepeatBehavior.Name)
          return platformType.WindowsUIDirectUIMediaAnimationRepeatBehavior;
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIMediaAnimationRepeatBehaviorType.Name)
          return platformType.WindowsUIDirectUIMediaAnimationRepeatBehaviorType;
      } else if (this.IsWindowsUIXamlMediaMedia3D(namespaceReference)) {
        if (namespaceTypeReference.Name == platformType.WindowsUIDirectUIMediaMedia3DMatrix3D.Name)
          return platformType.WindowsUIDirectUIMediaMedia3DMatrix3D;
      }
      return typeReference;
    }

    /// <summary>
    /// Provides the host with an opportunity to substitute a custom attribute with another during metadata reading.
    /// This avoids the cost of rewriting the entire unit in order to make such changes.
    /// </summary>
    /// <param name="referringUnit">The unit that is referencing the type.</param>
    /// <param name="customAttribute">The custom attribute to rewrite (fix up).</param>
    /// <returns>
    /// Usually the value in customAttribute, but occassionally another custom attribute.
    /// </returns>
    public override ICustomAttribute Rewrite(IUnit referringUnit, ICustomAttribute customAttribute) {
      CustomAttribute customAttr = customAttribute as CustomAttribute;
      if (customAttr == null) return customAttribute;
      var referringAssembly = referringUnit as IAssembly;
      if (referringAssembly == null || !(referringAssembly.ContainsForeignTypes)) return customAttribute;
      if (!TypeHelper.TypesAreEquivalent(customAttribute.Type, this.PlatformType.SystemAttributeUsageAttribute)) return customAttribute;
      //The custom attribute constructor has been redirected from Windows.Foundation.AttributeUsageAttribute, which has a different
      //set of flags from System.AttributeUsageAttribute for its first and only constructor parameter and also does not have an AllowMultiple property. 
      var argArray = customAttr.Arguments;
      if (argArray == null || argArray.Length != 1) return customAttribute;
      var argConst = argArray[0] as ConstantExpression;
      if (argConst == null || !(argConst.value is int)) return customAttribute;
      int clrEnumValue = 0;
      switch ((int)argConst.Value) {
        case 0x00000001: clrEnumValue = 0x00001000; break;
        case 0x00000002: clrEnumValue = 0x00000010; break;
        case 0x00000004: clrEnumValue = 0x00000200; break;
        case 0x00000008: clrEnumValue = 0x00000100; break;
        case 0x00000010: clrEnumValue = 0x00000400; break;
        case 0x00000020: clrEnumValue = 0x00000000; break;
        case 0x00000040: clrEnumValue = 0x00000040; break;
        case 0x00000080: clrEnumValue = 0x00000800; break;
        case 0x00000100: clrEnumValue = 0x00000080; break;
        case 0x00000200: clrEnumValue = 0x00000004; break;
        case 0x00000400: clrEnumValue = 0x00000008; break;
        case 0x00000800: clrEnumValue = 0x00000000; break;
        case -1: clrEnumValue = 0x00007FFF; break;
      }
      argConst.value = clrEnumValue;
      if (this.FellowCustomAttributeIncludeAllowMultiple(customAttr)) {
        if (customAttr.NamedArguments != null) return customAttribute;
        var trueVal = new ConstantExpression(this.PlatformType.SystemBoolean, true);
        var namedArgArray = new IMetadataNamedArgument[1];
        namedArgArray[0] = new FieldOrPropertyNamedArgumentExpression(this.AllowMultiple, Dummy.Type, false, this.PlatformType.SystemBoolean, trueVal);
        customAttr.NamedArguments = namedArgArray;
      }
      return customAttribute;
    }

    private bool FellowCustomAttributeIncludeAllowMultiple(CustomAttribute customAttr) {
      foreach (var customAttribute in customAttr.PEFileToObjectModel.GetAttributesForSameParentAs(customAttr.TokenValue)) {
        var caType = customAttribute.Type as INamespaceTypeReference;
        if (caType != null && caType.Name == this.AllowMultipleAttribute && this.IsWindowsFoundation(caType.ContainingUnitNamespace as INestedUnitNamespaceReference))
          return true;
      }
      return false;
    }

    private bool IsWindows(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Windows) return false;
      return namespaceReference.ContainingUnitNamespace is IRootUnitNamespaceReference;
    }

    private bool IsWindowsFoundation(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Foundation) return false;
      return IsWindows(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsFoundationCollections(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Collections) return false;
      return IsWindowsFoundation(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsFoundationMetadata(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Metadata) return false;
      return IsWindowsFoundation(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUI(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.UI) return false;
      return IsWindows(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXaml(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Xaml) return false;
      return IsWindowsUI(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlControls(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Controls) return false;
      return IsWindowsUIXaml(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlControlsPrimitives(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Primitives) return false;
      return IsWindowsUIXamlControls(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlData(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Data) return false;
      return IsWindowsUIXaml(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlInterop(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Interop) return false;
      return IsWindowsUIXaml(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlMedia(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Media) return false;
      return IsWindowsUIXaml(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlMediaAnimation(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Animation) return false;
      return IsWindowsUIXamlMedia(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

    private bool IsWindowsUIXamlMediaMedia3D(INestedUnitNamespaceReference/*?*/ namespaceReference) {
      if (namespaceReference == null || namespaceReference.Name != this.Media3D) return false;
      return IsWindowsUIXamlMedia(namespaceReference.ContainingUnitNamespace as INestedUnitNamespaceReference);
    }

  }

  class WindowsRuntimePlatform : PlatformType {

    /// <summary>
    /// Allocates a collection of references to types from the core platform, such as System.Object and System.String.
    /// </summary>
    /// <param name="host">
    /// An object that provides a standard abstraction over the applications that host components that provide or consume objects from the metadata model.
    /// </param>
    internal WindowsRuntimePlatform(IMetadataHost host)
      : base(host) {
    }

    /// <summary>
    /// A reference to the CLR System assembly
    /// </summary>
    internal IAssemblyReference System {
      get {
        if (this.system == null)
          this.system = new Microsoft.Cci.Immutable.AssemblyReference(this.host, this.GetSystemSymbolicIdentity());
        return this.system;
      }
    }
    private IAssemblyReference/*?*/ system;

    /// <summary>
    /// Returns an identity that is the same as CoreAssemblyIdentity, except that the name is "System".
    /// </summary>
    private AssemblyIdentity GetSystemSymbolicIdentity() {
      var core = this.host.CoreAssemblySymbolicIdentity;
      var name = this.host.NameTable.System;
      var location = core.Location;
      if (location != null)
        location = Path.Combine(Path.GetDirectoryName(location), "System.dll");
      return new AssemblyIdentity(name, core.Culture, core.Version, core.PublicKeyToken, location);
    }

    /// <summary>
    /// A reference to the assembly that contains CLR types to substitute for Windows Runtime types.
    /// </summary>
    internal IAssemblyReference SystemRuntimeWindowsRuntime {
      get {
        if (this.systemRuntimeWindowsRuntime == null)
          this.systemRuntimeWindowsRuntime = new Microsoft.Cci.Immutable.AssemblyReference(this.host, this.GetSystemRuntimeWindowsRuntimeSymbolicIdentity());
        return this.systemRuntimeWindowsRuntime;
      }
    }
    private IAssemblyReference/*?*/ systemRuntimeWindowsRuntime;

    /// <summary>
    /// Returns an identity that is the same as CoreAssemblyIdentity, except that the name is "System.Runtime.WindowsRuntime" and the version is at least 4.0.
    /// </summary>
    private AssemblyIdentity GetSystemRuntimeWindowsRuntimeSymbolicIdentity() {
      var core = this.host.CoreAssemblySymbolicIdentity;
      var name = this.host.NameTable.GetNameFor("System.Runtime.WindowsRuntime");
      var location = core.Location;
      if (location != null)
        location = Path.Combine(Path.GetDirectoryName(location), "System.Runtime.WindowsRuntime.dll");
      var version = new Version(4, 0, 0, 0);
      if (version < core.Version) version = core.Version;
      return new AssemblyIdentity(name, core.Culture, version, core.PublicKeyToken, location);
    }

    /// <summary>
    /// System.AttributeTargets
    /// </summary>
    internal INamespaceTypeReference SystemAttributeTargets {
      get {
        if (this.systemAttributeTargets == null) {
          this.systemAttributeTargets = this.CreateReference(this.CoreAssemblyRef, true, "System", "AttributeTargets");
        }
        return this.systemAttributeTargets;
      }
    }
    INamespaceTypeReference/*?*/ systemAttributeTargets;

    /// <summary>
    /// System.Collections.Generic.IDictionary
    /// </summary>
    internal INamespaceTypeReference SystemCollectionsGenericIDictionary {
      get {
        if (this.systemCollectionsGenericIDictionary == null) {
          this.systemCollectionsGenericIDictionary = this.CreateReference(this.CoreAssemblyRef, 2, "System", "Collections", "Generic", "IDictionary");
        }
        return this.systemCollectionsGenericIDictionary;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsGenericIDictionary;

    /// <summary>
    /// System.Collections.Generic.KeyValuePair
    /// </summary>
    internal INamespaceTypeReference SystemCollectionsGenericKeyValuePair {
      get {
        if (this.systemCollectionsGenericKeyValuePair == null) {
          this.systemCollectionsGenericKeyValuePair = 
            this.CreateReference(this.CoreAssemblyRef, true, 2, PrimitiveTypeCode.NotPrimitive, "System", "Collections", "Generic", "KeyValuePair");
        }
        return this.systemCollectionsGenericKeyValuePair;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsGenericKeyValuePair;

    /// <summary>
    /// System.Collections.Generic.ReadOnlyDictionary
    /// </summary>
    internal INamespaceTypeReference SystemCollectionsGenericReadOnlyDictionary {
      get {
        if (this.systemCollectionsGenericReadOnlyDictionary == null) {
          this.systemCollectionsGenericReadOnlyDictionary = this.CreateReference(this.CoreAssemblyRef, 2, "System", "Collections", "Generic", "ReadOnlyDictionary");
        }
        return this.systemCollectionsGenericReadOnlyDictionary;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsGenericReadOnlyDictionary;

    /// <summary>
    /// System.Collections.Generic.ReadOnlyList
    /// </summary>
    public INamespaceTypeReference SystemCollectionsGenericReadOnlyList {
      get {
        if (this.systemCollectionsGenericReadOnlyList == null) {
          this.systemCollectionsGenericReadOnlyList = this.CreateReference(this.CoreAssemblyRef, 1, "System", "Collections", "Generic", "ReadOnlyList");
        }
        return this.systemCollectionsGenericReadOnlyList;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsGenericReadOnlyList;

    /// <summary>
    /// System.Collections.Specialized.INotifyCollectionChanged
    /// </summary>
    public INamespaceTypeReference SystemCollectionsSpecializedINotifyColletionChanged {
      get {
        if (this.systemCollectionsSpecializedINotifyColletionChanged == null) {
          this.systemCollectionsSpecializedINotifyColletionChanged = this.CreateReference(this.System, 1, "System", "Collections", "Specialized", "INotifyCollectionChanged");
        }
        return this.systemCollectionsSpecializedINotifyColletionChanged;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsSpecializedINotifyColletionChanged;

    /// <summary>
    /// System.Collections.Specialized.NotifyCollectionChangedAction
    /// </summary>
    public INamespaceTypeReference SystemCollectionsSpecializedNotifyCollectionChangedAction {
      get {
        if (this.systemCollectionsSpecializedNotifyCollectionChangedAction == null) {
          this.systemCollectionsSpecializedNotifyCollectionChangedAction = this.CreateReference(this.System, 1, "System", "Collections", "Specialized", "NotifyCollectionChangedAction");
        }
        return this.systemCollectionsSpecializedNotifyCollectionChangedAction;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsSpecializedNotifyCollectionChangedAction;

    /// <summary>
    /// System.Collections.Specialized.NotifyCollectionChangedEventArgs
    /// </summary>
    public INamespaceTypeReference SystemCollectionsSpecializedNotifyCollectionChangedEventArgs {
      get {
        if (this.systemCollectionsSpecializedNotifyCollectionChangedEventArgs == null) {
          this.systemCollectionsSpecializedNotifyCollectionChangedEventArgs = this.CreateReference(this.System, 1, "System", "Collections", "Specialized", "NotifyCollectionChangedEventArgs");
        }
        return this.systemCollectionsSpecializedNotifyCollectionChangedEventArgs;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsSpecializedNotifyCollectionChangedEventArgs;

    /// <summary>
    /// System.Collections.Specialized.NotifyCollectionChangedEventHandler
    /// </summary>
    public INamespaceTypeReference SystemCollectionsSpecializedNotifyCollectionChangedEventHandler {
      get {
        if (this.systemCollectionsSpecializedNotifyCollectionChangedEventHandler == null) {
          this.systemCollectionsSpecializedNotifyCollectionChangedEventHandler = this.CreateReference(this.System, 1, "System", "Collections", "Specialized", "NotifyCollectionChangedEventHandler");
        }
        return this.systemCollectionsSpecializedNotifyCollectionChangedEventHandler;
      }
    }
    INamespaceTypeReference/*?*/ systemCollectionsSpecializedNotifyCollectionChangedEventHandler;

    /// <summary>
    /// System.ComponentModel.INotifyPropertyChanged
    /// </summary>
    public INamespaceTypeReference SystemComponentModelINotifyPropertyChanged {
      get {
        if (this.systemComponentModelINotifyPropertyChanged == null) {
          this.systemComponentModelINotifyPropertyChanged = this.CreateReference(this.System, "System", "ComponentModel", "INotifyPropertyChanged");
        }
        return this.systemComponentModelINotifyPropertyChanged;
      }
    }
    INamespaceTypeReference/*?*/ systemComponentModelINotifyPropertyChanged;

    /// <summary>
    /// System.ComponentModel.PropertyChangedEventArgs
    /// </summary>
    public INamespaceTypeReference SystemComponentModelPropertyChangedEventArgs {
      get {
        if (this.systemComponentModelPropertyChangedEventArgs == null) {
          this.systemComponentModelPropertyChangedEventArgs = this.CreateReference(this.System, "System", "ComponentModel", "PropertyChangedEventArgs");
        }
        return this.systemComponentModelPropertyChangedEventArgs;
      }
    }
    INamespaceTypeReference/*?*/ systemComponentModelPropertyChangedEventArgs;

    /// <summary>
    /// System.ComponentModel.PropertyChangedEventHandler
    /// </summary>
    public INamespaceTypeReference SystemComponentModelPropertyChangedEventHandler {
      get {
        if (this.systemComponentModelPropertyChangedEventHandler == null) {
          this.systemComponentModelPropertyChangedEventHandler = this.CreateReference(this.System, "System", "ComponentModel", "PropertyChangedEventHandler");
        }
        return this.systemComponentModelPropertyChangedEventHandler;
      }
    }
    INamespaceTypeReference/*?*/ systemComponentModelPropertyChangedEventHandler;

    /// <summary>
    /// System.EventHandler`1
    /// </summary>
    public INamespaceTypeReference SystemEventHandler1 {
      get {
        if (this.systemEventHandler1 == null) {
          this.systemEventHandler1 = this.CreateReference(this.CoreAssemblyRef, true, 1, PrimitiveTypeCode.NotPrimitive, "System", "EventHandler");
        }
        return this.systemEventHandler1;
      }
    }
    INamespaceTypeReference/*?*/ systemEventHandler1;

    /// <summary>
    /// System.Nullable`1
    /// </summary>
    internal INamespaceTypeReference SystemNullable1 {
      get {
        if (this.systemNullable1 == null) {
          this.systemNullable1 = this.CreateReference(this.CoreAssemblyRef, true, 1, PrimitiveTypeCode.NotPrimitive, "System", "Nullable");
        }
        return this.systemNullable1;
      }
    }
    INamespaceTypeReference/*?*/ systemNullable1;

    /// <summary>
    /// System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken
    /// </summary>
    internal INamespaceTypeReference SystemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken {
      get {
        if (this.systemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken == null) {
          this.systemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken = 
            this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "System", "Runtime", "InteropServices", "WindowsRuntime", "EventRegistrationToken");
        }
        return this.systemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken;
      }
    }
    INamespaceTypeReference/*?*/ systemRuntimeInteropServicesWindowsRuntimeEventRegistrationToken;

    /// <summary>
    /// System.TimeSpan
    /// </summary>
    internal INamespaceTypeReference SystemTimeSpan {
      get {
        if (this.systemTimeSpan == null) {
          this.systemTimeSpan = this.CreateReference(this.CoreAssemblyRef, true, "System", "TimeSpan");
        }
        return this.systemTimeSpan;
      }
    }
    INamespaceTypeReference/*?*/ systemTimeSpan;

    /// <summary>
    /// System.Uri
    /// </summary>
    internal INamespaceTypeReference SystemUri {
      get {
        if (this.systemUri == null) {
          this.systemUri = this.CreateReference(this.System, "System", "Uri");
        }
        return this.systemUri;
      }
    }
    INamespaceTypeReference/*?*/ systemUri;

    /// <summary>
    /// Windows.Foundation.Color
    /// </summary>
    public INamespaceTypeReference WindowsFoundationColor {
      get {
        if (this.windowsFoundationColor == null) {
          this.windowsFoundationColor = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "Foundation", "Color");
        }
        return this.windowsFoundationColor;
      }
    }
    INamespaceTypeReference/*?*/ windowsFoundationColor;

    /// <summary>
    /// Windows.Foundation.Point
    /// </summary>
    public INamespaceTypeReference WindowsFoundationPoint {
      get {
        if (this.windowsFoundationPoint == null) {
          this.windowsFoundationPoint = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "Foundation", "Point");
        }
        return this.windowsFoundationPoint;
      }
    }
    INamespaceTypeReference/*?*/ windowsFoundationPoint;

    /// <summary>
    /// Windows.Foundation.Rect
    /// </summary>
    public INamespaceTypeReference WindowsFoundationRect {
      get {
        if (this.windowsFoundationRect == null) {
          this.windowsFoundationRect = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "Foundation", "Rect");
        }
        return this.windowsFoundationRect;
      }
    }
    INamespaceTypeReference/*?*/ windowsFoundationRect;

    /// <summary>
    /// Windows.Foundation.Size
    /// </summary>
    public INamespaceTypeReference WindowsFoundationSize {
      get {
        if (this.windowsFoundationSize == null) {
          this.windowsFoundationSize = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "Foundation", "Size");
        }
        return this.windowsFoundationSize;
      }
    }
    INamespaceTypeReference/*?*/ windowsFoundationSize;

    /// <summary>
    /// Windows.UI.DirectUI.CornerRadius
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUICornerRadius {
      get {
        if (this.windowsUIDirectUICornerRadius == null) {
          this.windowsUIDirectUICornerRadius = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "CornerRadius");
        }
        return this.windowsUIDirectUICornerRadius;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUICornerRadius;

    /// <summary>
    /// Windows.UI.DirectUI.Duration
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIDuration {
      get {
        if (this.windowsUIDirectUIDuration == null) {
          this.windowsUIDirectUIDuration = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Duration");
        }
        return this.windowsUIDirectUIDuration;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIDuration;

    /// <summary>
    /// Windows.UI.DirectUI.DurationType
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIDurationType {
      get {
        if (this.windowsUIDirectUIDurationType == null) {
          this.windowsUIDirectUIDurationType = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "DurationType");
        }
        return this.windowsUIDirectUIDurationType;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIDurationType;

    /// <summary>
    /// Windows.UI.DirectUI.GridLength
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIGridLength {
      get {
        if (this.windowsUIDirectUIGridLength == null) {
          this.windowsUIDirectUIGridLength = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "GridLength");
        }
        return this.windowsUIDirectUIGridLength;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIGridLength;

    /// <summary>
    /// Windows.UI.DirectUI.GridUnitType
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIGridUnitType {
      get {
        if (this.windowsUIDirectUIGridUnitType == null) {
          this.windowsUIDirectUIGridUnitType = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "GridUnitType");
        }
        return this.windowsUIDirectUIGridUnitType;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIGridUnitType;

    /// <summary>
    /// Windows.UI.DirectUI.Thickness
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIThickness {
      get {
        if (this.windowsUIDirectUIThickness == null) {
          this.windowsUIDirectUIThickness = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Thickness");
        }
        return this.windowsUIDirectUIThickness;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIThickness;

    /// <summary>
    /// Windows.UI.DirectUI.Controls.Primitives.GeneratorPosition
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIControlsPrimitivesGeneratorPosition {
      get {
        if (this.windowsUIDirectUIControlsPrimitivesGeneratorPosition == null) {
          this.windowsUIDirectUIControlsPrimitivesGeneratorPosition = 
            this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Controls", "Primitives", "GeneratorPosition");
        }
        return this.windowsUIDirectUIControlsPrimitivesGeneratorPosition;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIControlsPrimitivesGeneratorPosition;

    /// <summary>
    /// Windows.UI.DirectUI.Media.Matrix
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIMediaMatrix {
      get {
        if (this.windowsUIDirectUIMediaMatrix == null) {
          this.windowsUIDirectUIMediaMatrix = this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Media", "Matrix");
        }
        return this.windowsUIDirectUIMediaMatrix;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIMediaMatrix;

    /// <summary>
    /// Windows.UI.DirectUI.Media.Animation.KeyTime
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIMediaAnimationKeyTime {
      get {
        if (this.windowsUIDirectUIMediaAnimationKeyTime == null) {
          this.windowsUIDirectUIMediaAnimationKeyTime = 
            this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Media", "Animation", "KeyTime");
        }
        return this.windowsUIDirectUIMediaAnimationKeyTime;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIMediaAnimationKeyTime;

    /// <summary>
    /// Windows.UI.DirectUI.Media.Animation.RepeatBehavior
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIMediaAnimationRepeatBehavior {
      get {
        if (this.windowsUIDirectUIMediaAnimationRepeatBehavior == null) {
          this.windowsUIDirectUIMediaAnimationRepeatBehavior = 
            this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Media", "Animation", "RepeatBehavior");
        }
        return this.windowsUIDirectUIMediaAnimationRepeatBehavior;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIMediaAnimationRepeatBehavior;

    /// <summary>
    /// Windows.UI.DirectUI.Media.Animation.RepeatBehaviorType
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIMediaAnimationRepeatBehaviorType {
      get {
        if (this.windowsUIDirectUIMediaAnimationRepeatBehaviorType == null) {
          this.windowsUIDirectUIMediaAnimationRepeatBehaviorType = 
            this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Media", "Animation", "RepeatBehaviorType");
        }
        return this.windowsUIDirectUIMediaAnimationRepeatBehaviorType;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIMediaAnimationRepeatBehaviorType;

    /// <summary>
    /// Windows.UI.DirectUI.Media.Media3D.Matrix3D
    /// </summary>
    public INamespaceTypeReference WindowsUIDirectUIMediaMedia3DMatrix3D {
      get {
        if (this.windowsUIDirectUIMediaMedia3DMatrix3D == null) {
          this.windowsUIDirectUIMediaMedia3DMatrix3D = 
            this.CreateReference(this.SystemRuntimeWindowsRuntime, true, "Windows", "UI", "DirectUI", "Media", "Media3D", "Matrix3D");
        }
        return this.windowsUIDirectUIMediaMedia3DMatrix3D;
      }
    }
    INamespaceTypeReference/*?*/ windowsUIDirectUIMediaMedia3DMatrix3D;

  }

  internal sealed partial class PEFileToObjectModel {

    internal IEnumerable<ICustomAttribute> GetAttributesForSameParentAs(uint tokenValue) {
      //We get here after being called from this.GetCustomAttributeInfo via the host, hence this.currentOwningObject
      //is the owning object of the custom attribute identified by tokenValue.
      var index = tokenValue & TokenTypeIds.RIDMask;
      var table = this.PEFileReader.CustomAttributeTable;
      var parent = table[index].Parent;
      var i = index;
      var len = table.NumberOfRows;
      while (i > 1 && table[i-1].Parent == parent) i--;
      while (i < len) {
        if (i == index) { i++; continue; }
        if (table[i].Parent != parent) yield break;
        yield return this.GetCustomAttributeAtRow(this.currentOwningObject, parent, i);
        i++;
      }
    }
  }


}