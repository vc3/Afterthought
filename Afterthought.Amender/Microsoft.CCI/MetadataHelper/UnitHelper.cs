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
using System.IO;
using System.Text;
using System.Globalization;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {
  /// <summary>
  /// Class containing helper routines for Units
  /// </summary>
  public static class UnitHelper {

    /// <summary>
    /// True if assembly1 has an attribute that allows assembly2 to access internal members of assembly1.
    /// </summary>
    /// <param name="assembly1">The assembly whose attribute is to be inspected.</param>
    /// <param name="assembly2">The assembly that must be mentioned in the attribute of assembly1.</param>
    /// <returns></returns>
    public static bool AssemblyOneAllowsAssemblyTwoToAccessItsInternals(IAssembly assembly1, IAssembly assembly2) {
      var name2 = assembly2.Name.Value;
      var name2Length = assembly2.Name.Value.Length;
      foreach (var attribute in assembly1.AssemblyAttributes) {
        if (!TypeHelper.TypesAreEquivalent(attribute.Type, assembly1.PlatformType.SystemRuntimeCompilerServicesInternalsVisibleToAttribute)) continue;
        foreach (var argument in attribute.Arguments) {
          var metadataConst = argument as IMetadataConstant;
          if (metadataConst == null) break;
          var assemblyName = metadataConst.Value as string;
          if (assemblyName == null) break;
          var assemblyNameLength = assemblyName.Length;
          if (assemblyNameLength < name2Length) break;
          if (assemblyNameLength > name2Length) {
            var len = assemblyName.IndexOf(' ');
            if (len < 0) len = assemblyName.IndexOf(',');
            if (len != name2Length) break;
          }
          if (string.Compare(assemblyName, 0, assembly2.Name.Value, 0, name2Length, StringComparison.OrdinalIgnoreCase) == 0) return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns the Assembly identity for the assembly name.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="metadataHost"></param>
    /// <returns></returns>
    public static AssemblyIdentity GetAssemblyIdentity(System.Reflection.AssemblyName assemblyName, IMetadataHost metadataHost) {
      Contract.Requires(assemblyName != null);
      Contract.Requires(metadataHost != null);
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);

      string culture = assemblyName.CultureInfo == null || assemblyName.CultureInfo == System.Globalization.CultureInfo.InvariantCulture ? "neutral" : assemblyName.CultureInfo.ToString();
      string name = assemblyName.Name;
      if (name == null) name = string.Empty;
      Version version = assemblyName.Version;
      if (version == null) version = Dummy.Version;
      byte[] token = assemblyName.GetPublicKeyToken();
      if (token == null) token = new byte[0];
      var codeBase = assemblyName.CodeBase;
      if (codeBase == null) codeBase = string.Empty;
      return new AssemblyIdentity(metadataHost.NameTable.GetNameFor(name), culture, version, token, codeBase);
    }

    /// <summary>
    /// Allocates an object that identifies a .NET assembly, using the IAssembly object
    /// </summary>
    /// <param name="assembly"></param>
    public static AssemblyIdentity GetAssemblyIdentity(IAssembly assembly) {
      Contract.Requires(assembly != null);
      Contract.Ensures(Contract.Result<AssemblyIdentity>() != null);

      byte[] pKey = new List<byte>(assembly.PublicKey).ToArray();
      if (pKey.Length != 0) {
        return new AssemblyIdentity(assembly.Name, assembly.Culture, assembly.Version, UnitHelper.ComputePublicKeyToken(pKey), assembly.Location);
      } else {
        return new AssemblyIdentity(assembly.Name, assembly.Culture, assembly.Version, new byte[0], assembly.Location);
      }
    }

    /// <summary>
    /// Constructs module identity for the given module
    /// </summary>
    /// <param name="module">Module for which the identity is desired.</param>
    /// <returns>The module identity corresponding to the passed module.</returns>
    public static ModuleIdentity GetModuleIdentity(IModule module) {
      Contract.Requires(module != null);
      Contract.Ensures(Contract.Result<ModuleIdentity>() != null);

      if (module.ContainingAssembly != null) {
        return new ModuleIdentity(module.Name, module.Location, UnitHelper.GetAssemblyIdentity(module.ContainingAssembly));
      } else {
        return new ModuleIdentity(module.Name, module.Location);
      }
    }

    /// <summary>
    /// Computes the public key token for the given public key
    /// </summary>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    public static byte[] ComputePublicKeyToken(IEnumerable<byte> publicKey) {
      Contract.Requires(publicKey != null);
      Contract.Ensures(Contract.Result<byte[]>() != null);

      byte[] pKey = new List<byte>(publicKey).ToArray();
      if (pKey.Length == 0)
        return pKey;
      System.Security.Cryptography.SHA1 sha1Algo = System.Security.Cryptography.SHA1.Create("SHA1");
      byte[] hash = sha1Algo.ComputeHash(pKey);
      byte[] publicKeyToken = new byte[8];
      int startIndex = hash.Length - 8;
      Contract.Assume(0 <= startIndex && startIndex < hash.Length);
      Contract.Assert(startIndex + 8 <= hash.GetLowerBound(0) + hash.Length);
      Array.Copy(hash, startIndex, publicKeyToken, 0, 8);
      Array.Reverse(publicKeyToken, 0, 8);
      return publicKeyToken;
    }

    /// <summary>
    /// Returns a unit namespace definition, nested in the given unitNamespace if possible,
    /// otherwise nested in the given unit if possible, otherwise nested in the given host if possible, otherwise it returns Dummy.UnitNamespace.
    /// If unitNamespaceReference is a root namespace, the result is equal to the given unitNamespace if not null,
    /// otherwise the result is the root namespace of the given unit if not null, 
    /// otherwise the result is root namespace of unitNamespaceReference.Unit, if that can be resolved via the given host, 
    /// otherwise the result is Dummy.UnitNamespace.
    /// </summary>
    public static IUnitNamespace Resolve(IUnitNamespaceReference unitNamespaceReference, IMetadataHost host, IUnit unit = null, IUnitNamespace unitNamespace = null) {
      Contract.Requires(unitNamespaceReference != null);
      Contract.Requires(host != null);
      Contract.Requires(unit != null || unitNamespace == null);
      Contract.Ensures(Contract.Result<IUnitNamespace>() != null);

      var rootNsRef = unitNamespaceReference as IRootUnitNamespaceReference;
      if (rootNsRef != null) {
        if (unitNamespace != null) return unitNamespace;
        if (unit != null) return unit.UnitNamespaceRoot;
        unit = UnitHelper.Resolve(unitNamespaceReference.Unit, host);
        if (unit is Dummy) return Dummy.UnitNamespace;
        return unit.UnitNamespaceRoot;
      }
      var nestedNsRef = unitNamespaceReference as INestedUnitNamespaceReference;
      if (nestedNsRef == null) return Dummy.UnitNamespace;
      var containingNsDef = UnitHelper.Resolve(nestedNsRef.ContainingUnitNamespace, host, unit, unitNamespace);
      if (containingNsDef is Dummy) return Dummy.UnitNamespace;
      foreach (var nsMem in containingNsDef.GetMembersNamed(nestedNsRef.Name, ignoreCase: false)) {
        var neNsDef = nsMem as INestedUnitNamespace;
        if (neNsDef != null) return neNsDef;
      }
      return Dummy.UnitNamespace;
    }

    /// <summary>
    /// Returns a unit, loaded into the given host, that matches unitReference. The host will load the unit, if necessary and possible.
    /// If the reference cannot be resolved a dummy unit is returned.
    /// </summary>
    public static IUnit Resolve(IUnitReference unitReference, IMetadataHost host) {
      Contract.Requires(unitReference != null);
      Contract.Requires(host != null);
      Contract.Ensures(Contract.Result<IUnit>() != null);

      return host.LoadUnit(unitReference.UnitIdentity);
    }

    /// <summary>
    /// Computes the string representing the strong name of the given assembly reference.
    /// </summary>
    public static string StrongName(IAssemblyReference assemblyReference) {
      Contract.Requires(assemblyReference != null);
      Contract.Ensures(Contract.Result<string>() != null);

      StringBuilder sb = new StringBuilder();
      sb.Append(assemblyReference.Name.Value);
      sb.AppendFormat(CultureInfo.InvariantCulture, ", Version={0}.{1}.{2}.{3}", assemblyReference.Version.Major, assemblyReference.Version.Minor, assemblyReference.Version.Build, assemblyReference.Version.Revision);
      if (assemblyReference.Culture.Length > 0)
        sb.AppendFormat(CultureInfo.InvariantCulture, ", Culture={0}", assemblyReference.Culture);
      else
        sb.Append(", Culture=neutral");
      sb.AppendFormat(CultureInfo.InvariantCulture, ", PublicKeyToken=");
      if (IteratorHelper.EnumerableIsNotEmpty(assemblyReference.PublicKeyToken)) {
        foreach (byte b in assemblyReference.PublicKeyToken) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
      } else {
        sb.Append("null");
      }
      if (assemblyReference.IsRetargetable)
        sb.Append(", Retargetable=Yes");
      if (assemblyReference.ContainsForeignTypes)
        sb.Append(", ContentType=WindowsRuntime");
      return sb.ToString();
    }

    /// <summary>
    /// Computes the string representing the strong name of the given assembly reference.
    /// </summary>
    public static string StrongName(AssemblyIdentity assemblyIdentity) {
      Contract.Requires(assemblyIdentity != null);
      Contract.Ensures(Contract.Result<string>() != null);

      StringBuilder sb = new StringBuilder();
      sb.Append(assemblyIdentity.Name.Value);
      sb.AppendFormat(CultureInfo.InvariantCulture, ", Version={0}.{1}.{2}.{3}", assemblyIdentity.Version.Major, assemblyIdentity.Version.Minor, assemblyIdentity.Version.Build, assemblyIdentity.Version.Revision);
      if (assemblyIdentity.Culture.Length > 0)
        sb.AppendFormat(CultureInfo.InvariantCulture, ", Culture={0}", assemblyIdentity.Culture);
      else
        sb.Append(", Culture=neutral");
      sb.AppendFormat(CultureInfo.InvariantCulture, ", PublicKeyToken=");
      if (IteratorHelper.EnumerableIsNotEmpty(assemblyIdentity.PublicKeyToken)) {
        foreach (byte b in assemblyIdentity.PublicKeyToken) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
      } else {
        sb.Append("null");
      }
      return sb.ToString();
    }

    /// <summary>
    /// Finds a type in the given module using the given type name, expressed in C# notation with dots separating both namespaces and types.
    /// If no such type can be found Dummy.NamespaceTypeDefinition is returned.
    /// </summary>
    /// <param name="nameTable">A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.</param>
    /// <param name="unit">The unit of metadata to search for the type.</param>
    /// <param name="typeName">A string containing the fully qualified type name, using C# formatting conventions.</param>
    public static INamedTypeDefinition FindType(INameTable nameTable, IUnit unit, string typeName) {
      Contract.Requires(nameTable != null);
      Contract.Requires(unit != null);
      Contract.Requires(typeName != null);
      Contract.Ensures(Contract.Result<INamedTypeDefinition>() != null);

      int offset = 0;
      INamedTypeDefinition/*?*/ result = GetType(nameTable, unit.UnitNamespaceRoot, typeName, 0, ref offset);
      if (result != null) return result;
      return Dummy.NamespaceTypeDefinition;
    }

    /// <summary>
    /// Finds a type in the given module using the given type name, expressed in C# notation with dots separating both namespaces and types.
    /// If no such type can be found Dummy.NamespaceTypeDefinition is returned.
    /// </summary>
    /// <param name="nameTable">A collection of IName instances that represent names that are commonly used during compilation.
    /// This is a provided as a parameter to the host environment in order to allow more than one host
    /// environment to co-exist while agreeing on how to map strings to IName instances.</param>
    /// <param name="unit">The unit of metadata to search for the type.</param>
    /// <param name="typeName">A string containing the fully qualified type name, using C# formatting conventions.</param>
    /// <param name="genericParameterCount">The total number of generic parameters (including inherited parameters) that the returned type should have.</param>
    public static INamedTypeDefinition FindType(INameTable nameTable, IUnit unit, string typeName, int genericParameterCount) {
      Contract.Requires(nameTable != null);
      Contract.Requires(unit != null);
      Contract.Requires(typeName != null);
      Contract.Requires(genericParameterCount >= 0);
      Contract.Ensures(Contract.Result<INamedTypeDefinition>() != null);

      int offset = 0;
      INamedTypeDefinition/*?*/ result = GetType(nameTable, unit.UnitNamespaceRoot, typeName, genericParameterCount, ref offset);
      if (result != null) return result;
      return Dummy.NamespaceTypeDefinition;
    }

    private static INamedTypeDefinition/*?*/ GetType(INameTable nameTable, INamespaceDefinition namespaceDefinition, string typeName, int genericParameterCount, ref int offset) {
      Contract.Requires(nameTable != null);
      Contract.Requires(namespaceDefinition != null);
      Contract.Requires(typeName != null);
      Contract.Requires(genericParameterCount >= 0);
      Contract.Requires(offset >= 0);
      Contract.Ensures(offset >= 0);

      int len = typeName.Length;
      if (offset >= len) return null;

      int savedOffset = offset;
      var nestedNamespaceDefinition = GetNamespace(nameTable, namespaceDefinition, typeName, ref offset);
      if (nestedNamespaceDefinition != null) {
        var naType = GetType(nameTable, nestedNamespaceDefinition, typeName, genericParameterCount, ref offset);
        if (naType != null) return naType;
      }
      offset = savedOffset;
      int dotPos = typeName.IndexOf('.', offset);
      Contract.Assume(dotPos < 0 || (dotPos >= offset && dotPos < len));
      if (dotPos < 0) dotPos = len;
      IName tName = nameTable.GetNameFor(typeName.Substring(offset, dotPos-offset));
      foreach (var member in namespaceDefinition.GetMembersNamed(tName, false)) {
        var namespaceType = member as INamespaceTypeDefinition;
        if (namespaceType == null) continue;
        if (dotPos == len) {
          if (namespaceType.GenericParameterCount != genericParameterCount) continue;
          return namespaceType;
        } else {
          if (namespaceType.GenericParameterCount > genericParameterCount) continue;
          offset = dotPos+1;
          var nt = GetNestedType(nameTable, namespaceType, typeName, genericParameterCount-namespaceType.GenericParameterCount, ref offset);
          if (nt != null) return nt;
        }
      }
      return null;
    }

    private static INestedUnitNamespace/*?*/ GetNamespace(INameTable nameTable, INamespaceDefinition namespaceDefinition, string typeName, ref int offset) {
      Contract.Requires(nameTable != null);
      Contract.Requires(namespaceDefinition != null);
      Contract.Requires(typeName != null);
      Contract.Requires(offset >= 0);
      Contract.Ensures(offset >= 0);

      int len = typeName.Length;
      if (offset >= len) return null;
      int dotPos = typeName.IndexOf('.', offset);
      if (dotPos < 0) return null;
      Contract.Assume(dotPos >= offset); //if a dot has been found, it must be at offset or later
      IName neName = nameTable.GetNameFor(typeName.Substring(offset, dotPos-offset));
      foreach (var member in namespaceDefinition.GetMembersNamed(neName, false)) {
        var nestedNamespace = member as INestedUnitNamespace;
        if (nestedNamespace == null) continue;
        offset = dotPos+1;
        return nestedNamespace;
      }
      return null;
    }

    private static INestedTypeDefinition/*?*/ GetNestedType(INameTable nameTable, ITypeDefinition typeDefinition, string typeName, int genericParameterCount, ref int offset) {
      Contract.Requires(nameTable != null);
      Contract.Requires(typeDefinition != null);
      Contract.Requires(typeName != null);
      Contract.Requires(genericParameterCount >= 0);
      Contract.Requires(offset >= 0);
      Contract.Ensures(offset >= 0);

      int len = typeName.Length;
      if (offset >= len) return null;
      int dotPos = typeName.IndexOf('.', offset);
      Contract.Assume(dotPos < 0 || (dotPos >= offset && dotPos < len));
      if (dotPos < 0) dotPos = len;
      IName tName = nameTable.GetNameFor(typeName.Substring(offset, dotPos-offset));
      foreach (var member in typeDefinition.GetMembersNamed(tName, false)) {
        var nestedType = member as INestedTypeDefinition;
        if (nestedType == null) continue;
        if (dotPos == len) {
          if (nestedType.GenericParameterCount != genericParameterCount) continue;
          return nestedType;
        }
        if (nestedType.GenericParameterCount > genericParameterCount) continue;
        offset = dotPos+1;
        var nt = GetNestedType(nameTable, nestedType, typeName, genericParameterCount-nestedType.GenericParameterCount, ref offset);
        if (nt != null) return nt;
      }
      return null;
    }

    /// <summary>
    /// Searches for the resource with given name in the given assembly.
    /// </summary>
    public static IResourceReference/*?*/ FindResourceNamed(IAssembly assembly, IName resourceName) {
      Contract.Requires(assembly != null);
      Contract.Requires(resourceName != null);

      foreach (IResourceReference resourceRef in assembly.Resources) {
        if (resourceRef.Name.UniqueKey == resourceName.UniqueKey)
          return resourceRef;
      }
      return null;
    }

    /// <summary>
    /// Returns true if the given two assembly references are to be considered equivalent.
    /// </summary>
    public static bool AssembliesAreEquivalent(IAssemblyReference/*?*/ assembly1, IAssemblyReference/*?*/ assembly2) {
      if (assembly1 == null || assembly2 == null)
        return false;
      if (assembly1 == assembly2)
        return true;
      if (assembly1.Name.UniqueKeyIgnoringCase != assembly2.Name.UniqueKeyIgnoringCase)
        return false;
      if (!assembly1.Version.Equals(assembly2.Version))
        return false;
      if (!assembly1.Culture.Equals(assembly2.Culture))
        return false;
      return IteratorHelper.EnumerablesAreEqual<byte>(assembly1.PublicKeyToken, assembly2.PublicKeyToken);
    }

    /// <summary>
    /// Returns true if the given two module references are to be considered equivalent.
    /// </summary>
    public static bool ModulesAreEquivalent(IModuleReference/*?*/ module1, IModuleReference/*?*/ module2) {
      if (module1 == null || module2 == null)
        return false;
      if (module1 == module2)
        return true;
      if (module1.ContainingAssembly != null) {
        if (module2.ContainingAssembly == null)
          return false;
        if (!UnitHelper.AssembliesAreEquivalent(module1.ContainingAssembly, module2.ContainingAssembly))
          return false;
      }
      return module1.Name.UniqueKeyIgnoringCase == module2.Name.UniqueKeyIgnoringCase;
    }

    /// <summary>
    /// Returns true if the given two unit references are to be considered equivalent.
    /// </summary>
    public static bool UnitsAreEquivalent(IUnitReference/*?*/ unit1, IUnitReference/*?*/ unit2) {
      if (unit1 == null || unit2 == null)
        return false;
      if (unit1 == unit2)
        return true;
      if (UnitHelper.AssembliesAreEquivalent(unit1 as IAssemblyReference, unit2 as IAssemblyReference))
        return true;
      if (UnitHelper.ModulesAreEquivalent(unit1 as IModuleReference, unit2 as IModuleReference))
        return true;
      return false;
    }

    /// <summary>
    /// Returns true if the given two unit references are to be considered equivalent as containers.
    /// </summary>
    public static bool UnitsAreContainmentEquivalent(IUnitReference/*?*/ unit1, IUnitReference/*?*/ unit2) {
      if (unit1 == null || unit2 == null)
        return false;
      if (unit1 == unit2)
        return true;
      IModuleReference/*?*/ moduleRef1 = unit1 as IModuleReference;
      IModuleReference/*?*/ moduleRef2 = unit2 as IModuleReference;
      if (moduleRef1 != null && moduleRef2 != null) {
        if (UnitHelper.AssembliesAreEquivalent(moduleRef1.ContainingAssembly, moduleRef2.ContainingAssembly))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Returns true if the given two unit namespaces are to be considered equivalent as containers.
    /// </summary>
    public static bool UnitNamespacesAreEquivalent(IUnitNamespaceReference/*?*/ unitNamespace1, IUnitNamespaceReference/*?*/ unitNamespace2) {
      if (unitNamespace1 == null || unitNamespace2 == null)
        return false;
      if (unitNamespace1 == unitNamespace2)
        return true;
      INestedUnitNamespaceReference/*?*/ nstUnitNamespace1 = unitNamespace1 as INestedUnitNamespaceReference;
      INestedUnitNamespaceReference/*?*/ nstUnitNamespace2 = unitNamespace2 as INestedUnitNamespaceReference;
      if (nstUnitNamespace1 != null && nstUnitNamespace2 != null) {
        return nstUnitNamespace1.Name.UniqueKey == nstUnitNamespace2.Name.UniqueKey
          && UnitHelper.UnitNamespacesAreEquivalent(nstUnitNamespace1.ContainingUnitNamespace, nstUnitNamespace2.ContainingUnitNamespace);
      }
      if (nstUnitNamespace1 != null || nstUnitNamespace2 != null)
        return false;
      return UnitHelper.UnitsAreContainmentEquivalent(unitNamespace1.Unit, unitNamespace2.Unit);
    }

  }
}

namespace Microsoft.Cci.Immutable {

  /// <summary>
  /// A set of units that all contribute to a unified root namespace. For example the set of assemblies referenced by a C# project.
  /// </summary>
  public sealed class UnitSet : IUnitSet {

    /// <summary>
    /// Constructs a unit set made up of the given (non empty) list of units.
    /// </summary>
    public UnitSet(IEnumerable<IUnit> units)
      // ^ requires EnumerationHelper.EnumerableIsNotEmpty(units); //TODO: it seems impossible to establish this precondition, even with an assumption just before a call.
    {
      this.units = units;
    }

    /// <summary>
    /// Determines if the given unit belongs to this set of units.
    /// </summary>
    public bool Contains(IUnit unit) {
      bool result = IteratorHelper.EnumerableContains(this.units, unit);
      //^ assume result == exists{IUnit u in this.Units; u == unit}; //TODO: Boogie: need a working postcodition on EnumerableContains
      return result;
    }

    /// <summary>
    /// Enumerates the units making up this set of units.
    /// </summary>
    public IEnumerable<IUnit> Units {
      get {
        return this.units;
      }
    }
    readonly IEnumerable<IUnit> units;

    /// <summary>
    /// A unified root namespace for this set of units. It contains nested namespaces as well as top level types and anything else that implements INamespaceMember.
    /// </summary>
    public IUnitSetNamespace UnitSetNamespaceRoot {
      get
        //^ ensures result == null || result.RootOwner == this;
        //^ ensures result == null || result.UnitSet == this;
      {
        if (this.unitSetNamespaceRoot == null) {
          lock (GlobalLock.LockingObject) {
            if (this.unitSetNamespaceRoot == null) {
              IName rootName = Dummy.Name;
              foreach (IUnit unit in this.Units) { rootName = unit.NamespaceRoot.Name; break; }
              this.unitSetNamespaceRoot = new RootUnitSetNamespace(rootName, this);
              this.unitSetNamespaceRoot.Locations.GetEnumerator().MoveNext();
            }
          }
        }
        //^ assume false;
        return this.unitSetNamespaceRoot;
      }
    }
    IRootUnitSetNamespace/*?*/ unitSetNamespaceRoot;
    //^ invariant unitSetNamespaceRoot == null || unitSetNamespaceRoot.RootOwner == this;
    //^ invariant unitSetNamespaceRoot == null || unitSetNamespaceRoot.UnitSet == this;

    #region INamespaceRootOwner Members

    INamespaceDefinition INamespaceRootOwner.NamespaceRoot {
      get { return this.UnitSetNamespaceRoot; }
    }

    #endregion
  }

  /// <summary>
  /// A namespace definition whose members are aggregations of the members of a collection of containers of the given container type.
  /// </summary>
  /// <typeparam name="ContainerType">The type of container that provides the members (or parts of members) for this namespace. For example NamespaceDeclaration.</typeparam>
  /// <typeparam name="ContainerMemberType">The base type for members supplied by the container. For example IAggregatableNamespaceDeclarationMember.</typeparam>
  public abstract class AggregatedNamespace<ContainerType, ContainerMemberType> : AggregatedScope<INamespaceMember, ContainerType, ContainerMemberType>, INamespaceDefinition
    where ContainerType : class, IContainer<ContainerMemberType>
    where ContainerMemberType : class, IContainerMember<ContainerType> {

    /// <summary>
    /// Allocates a namespace definition whose members are aggregations of the members of a collection of containers of the given container type.
    /// </summary>
    /// <param name="name">The name of this namespace definition.</param>
    protected AggregatedNamespace(IName name) {
      this.name = name;
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference. The dispatch method does nothing else.
    /// </summary>
    public abstract void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IReference, which is not derived from IDefinition. For example an object implemeting IArrayType will
    /// call visitor.Visit(IArrayTypeReference) and not visitor.Visit(IArrayType).
    /// The dispatch method does nothing else.
    /// </summary>
    public abstract void DispatchAsReference(IMetadataVisitor visitor);

    #region IContainer<ITypeDefinitionMember>

    IEnumerable<INamespaceMember> IContainer<INamespaceMember>.Members {
      get {
        return this.Members;
      }
    }

    #endregion

    #region INamespaceDefinition Members

    IEnumerable<INamespaceMember> INamespaceDefinition.Members {
      get {
        return this.Members;
      }
    }

    /// <summary>
    /// The name of this namespace definition.
    /// </summary>
    public IName Name {
      get {
        return this.name;
      }
    }
    private IName name;

    /// <summary>
    /// The object associated with the namespace. For example an IUnit or IUnitSet instance. This namespace is either the root namespace of that object
    /// or it is a nested namespace that is directly of indirectly nested in the root namespace.
    /// </summary>
    public abstract INamespaceRootOwner RootOwner {
      get;
    }

    #endregion

    #region IDefinition Members

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance.
    /// </summary>
    public abstract IEnumerable<ILocation> Locations {
      get;
    }

    #endregion

    #region IDefinition Members

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition.
    /// </summary>
    public virtual IEnumerable<ICustomAttribute> Attributes {
      get { return Enumerable<ICustomAttribute>.Empty; }
    }

    #endregion
  }


  /// <summary>
  /// A scope whose members are aggregations of the members of a collection of "containers". For example, a symbol table type definition whose members
  /// are the aggregations of the members of a collection of source type declarations.
  /// </summary>
  /// <typeparam name="ScopeMemberType">The base type for members of the aggregated scope. For example ITypeDefinitionMember.</typeparam>
  /// <typeparam name="ContainerType">The type of container that provides the members (or parts of members) for this scope. For example ITypeDeclaration.</typeparam>
  /// <typeparam name="ContainerMemberType">The base type for members supplied by the container. For example ITypeDeclarationMember.</typeparam>
  public abstract class AggregatedScope<ScopeMemberType, ContainerType, ContainerMemberType> : Scope<ScopeMemberType>
    where ScopeMemberType : class, IScopeMember<IScope<ScopeMemberType>>
    where ContainerType : class, IContainer<ContainerMemberType>
    where ContainerMemberType : class, IContainerMember<ContainerType> {

    /// <summary>
    /// Takes a container member, gets a corresponding aggregated member for it and adds the latter to the member collection of this scope (if necessary).
    /// Usually, the container member is added to the declarations collection of the aggregated member. This behavior is overridable. See GetAggregatedMember.
    /// </summary>
    /// <param name="member">The container member to aggregate. The aggregation gets cached and shows up in the Members collection of this scope.</param>
    private void AddContainerMemberToCache(ContainerMemberType/*!*/ member) {
      this.AddMemberToCache(this.GetAggregatedMember(member));
    }

    /// <summary>
    /// Adds all of the members of the given container to this scope, after aggregating the members with members from other containers.
    /// </summary>
    /// <param name="container">A collection of members to aggregate with members from other containers and add to the members collection of this scope.</param>
    protected virtual void AddContainer(ContainerType/*!*/ container) {
      foreach (ContainerMemberType containerMember in container.Members) {
        if (containerMember == null) continue; //TODO: see if the type system can preclude this
        this.AddContainerMemberToCache(containerMember);
      }
    }

    /// <summary>
    /// Finds or creates an aggregated member instance corresponding to the given member. Usually this should result in the given member being added to the declarations
    /// collection of the aggregated member.
    /// </summary>
    /// <param name="member">The member to aggregate.</param>
    protected abstract ScopeMemberType/*!*/ GetAggregatedMember(ContainerMemberType/*!*/ member);

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class RootUnitSetNamespace : UnitSetNamespace, IRootUnitSetNamespace {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="unitSet"></param>
    public RootUnitSetNamespace(IName name, UnitSet unitSet)
      : base(name, unitSet) {
    }

    /// <summary>
    /// Calls the visitor.Visit(IRootUnitSetNamespace) method.
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Throws an invalid operation exception since it makes no sense to have a reference to unit set namespace.
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    //^ [Confined]
    public override string ToString() {
      return TypeHelper.GetNamespaceName(this, NameFormattingOptions.None);
    }

  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class UnitSetNamespace : AggregatedNamespace<INamespaceDefinition, INamespaceMember>, IUnitSetNamespace {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="unitSet"></param>
    protected UnitSetNamespace(IName name, UnitSet unitSet)
      : base(name) {
      this.unitSet = unitSet;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="unitSet"></param>
    /// <param name="nestedUnitNamespaces"></param>
    protected UnitSetNamespace(IName name, IUnitSet unitSet, List<IUnitNamespace> nestedUnitNamespaces)
      : base(name) {
      this.unitSet = unitSet;
      this.unitNamespaces = nestedUnitNamespaces;
    }

    /// <summary>
    /// 
    /// </summary>
    public override IEnumerable<ILocation> Locations {
      get {
        foreach (IUnitNamespace unitNamespace in this.UnitNamespaces)
          foreach (ILocation location in unitNamespace.Locations)
            yield return location;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    protected override INamespaceMember GetAggregatedMember(INamespaceMember member) {
      INestedUnitNamespace/*?*/ nestedUnitNamespace = member as INestedUnitNamespace;
      if (nestedUnitNamespace == null) return member;
      NestedUnitSetNamespace/*?*/ result;
      if (!this.nestedUnitNamespaceToNestedUnitSetNamespaceMap.TryGetValue(nestedUnitNamespace.Name.UniqueKey, out result)) {
        result = new NestedUnitSetNamespace(this, member.Name, this.unitSet, new List<IUnitNamespace>());
        this.nestedUnitNamespaceToNestedUnitSetNamespaceMap.Add(nestedUnitNamespace.Name.UniqueKey, result);
      }
      //^ assume result != null && result.unitNamespaces != null;
      result.unitNamespaces.Add(nestedUnitNamespace);
      //TODO: thread safety
      return result;
    }
    readonly Dictionary<int, NestedUnitSetNamespace> nestedUnitNamespaceToNestedUnitSetNamespaceMap = new Dictionary<int, NestedUnitSetNamespace>();

    /// <summary>
    /// 
    /// </summary>
    public override INamespaceRootOwner RootOwner {
      get { return this.unitSet; }
    }

    /// <summary>
    /// 
    /// </summary>
    protected List<IUnitNamespace> UnitNamespaces {
      get {
        if (this.unitNamespaces == null) {
          lock (GlobalLock.LockingObject) {
            if (this.unitNamespaces == null) {
              List<IUnitNamespace> unitNamespaces = this.unitNamespaces = new List<IUnitNamespace>();
              foreach (IUnit unit in unitSet.Units) {
                unitNamespaces.Add(unit.UnitNamespaceRoot);
                this.AddContainer(unit.UnitNamespaceRoot);
              }
            }
          }
        }
        return this.unitNamespaces;
      }
    }
    List<IUnitNamespace>/*?*/ unitNamespaces;

    /// <summary>
    /// 
    /// </summary>
    public IUnitSet UnitSet {
      get { return this.unitSet; }
    }
    readonly IUnitSet unitSet;

  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class NestedUnitSetNamespace : UnitSetNamespace, INamespaceMember, INestedUnitSetNamespace {

    internal NestedUnitSetNamespace(UnitSetNamespace containingNamespace, IName name, IUnitSet unitSet, List<IUnitNamespace> nestedUnitNamepaces)
      : base(name, unitSet, nestedUnitNamepaces) {
      this.containingNamespace = containingNamespace;
    }

    /// <summary>
    /// Calls the visitor.Visit(INestedUnitSetNamespace) method.
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Throws an invalid operation exception since it makes no sense to have a reference to unit set namespace.
    /// </summary>
    public override void DispatchAsReference(IMetadataVisitor visitor) {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void InitializeIfNecessary() {
      if (this.isInitialized) return;
      lock (GlobalLock.LockingObject) {
        if (this.isInitialized) return;
        foreach (IUnitNamespace unitNamespace in this.UnitNamespaces)
          foreach (INamespaceMember member in unitNamespace.Members)
            this.AddMemberToCache(this.GetAggregatedMember(member));
        this.isInitialized = true;
      }
    }
    private bool isInitialized;

    /// <summary>
    /// 
    /// </summary>
    public UnitSetNamespace ContainingNamespace {
      get { return this.containingNamespace; }
    }
    readonly UnitSetNamespace containingNamespace;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    //^ [Confined]
    public override string ToString() {
      return TypeHelper.GetNamespaceName(this, NameFormattingOptions.None);
    }

    #region INamespaceMember Members

    INamespaceDefinition INamespaceMember.ContainingNamespace {
      get { return this.ContainingNamespace; }
    }

    #endregion

    #region IScopeMember<IScope<INamespaceMember>> Members

    /// <summary>
    /// 
    /// </summary>
    public IScope<INamespaceMember> ContainingScope {
      get { return this.ContainingNamespace; }
    }

    #endregion

    #region IContainerMember<INamespaceDefinition> Members

    /// <summary>
    /// 
    /// </summary>
    public INamespaceDefinition Container {
      get { return this.ContainingNamespace; }
    }

    IName IContainerMember<INamespaceDefinition>.Name {
      get { return this.Name; }
    }

    #endregion

    #region INestedUnitSetNamespace Members

    /// <summary>
    /// 
    /// </summary>
    public IUnitSetNamespace ContainingUnitSetNamespace {
      get { return this.containingNamespace; }
    }

    #endregion
  }
}
