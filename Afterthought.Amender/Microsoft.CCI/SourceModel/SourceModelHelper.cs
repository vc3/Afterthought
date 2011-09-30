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

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {
  /// <summary>
  /// A provider that aggregates a set of providers in order to
  /// map offsets in an IL stream to source locations.
  /// </summary>
  public sealed class AggregatingSourceLocationProvider : ISourceLocationProvider, IDisposable {

    Dictionary<IUnit, ISourceLocationProvider> unit2Provider = new Dictionary<IUnit, ISourceLocationProvider>();

    /// <summary>
    /// Copies the contents of the table
    /// </summary>
    public AggregatingSourceLocationProvider(Dictionary<IUnit, ISourceLocationProvider> unit2ProviderMap) {
      foreach (var keyValuePair in unit2ProviderMap) {
        this.unit2Provider.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }

    #region ISourceLocationProvider Members

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to one or more of the given derived (non primary) document locations.
    /// </summary>
    /// <param name="locations">Zero or more locations in documents that have been derived from one or more source documents.</param>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(IEnumerable<ILocation> locations) {
      foreach (ILocation location in locations) {
        foreach (var psloc in this.MapLocationToSourceLocations(location))
          yield return psloc;
      }
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the given derived (non primary) document location.
    /// </summary>
    /// <param name="location">A location in a document that have been derived from one or more source documents.</param>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsFor(ILocation location) {
      var psloc = location as IPrimarySourceLocation;
      if (psloc != null)
        return IteratorHelper.GetSingletonEnumerable(psloc);
      else {
        return this.MapLocationToSourceLocations(location);
      }
    }

    /// <summary>
    /// Return zero or more locations in primary source documents that correspond to the definition of the given local.
    /// </summary>
    public IEnumerable<IPrimarySourceLocation> GetPrimarySourceLocationsForDefinitionOf(ILocalDefinition localDefinition) {
      ISourceLocationProvider/*?*/ provider = this.GetProvider(localDefinition);
      if (provider == null)
        return Enumerable<IPrimarySourceLocation>.Empty;
      else
        return provider.GetPrimarySourceLocationsForDefinitionOf(localDefinition);
    }

    /// <summary>
    /// Returns the source name of the given local definition, if this is available.
    /// Otherwise returns the value of the Name property and sets isCompilerGenerated to true.
    /// </summary>
    public string GetSourceNameFor(ILocalDefinition localDefinition, out bool isCompilerGenerated) {
      ISourceLocationProvider/*?*/ provider = this.GetProvider(localDefinition);
      if (provider == null) {
        isCompilerGenerated = false;
        return "";
      } else {
        return provider.GetSourceNameFor(localDefinition, out isCompilerGenerated);
      }
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Disposes all aggregated providers.
    /// </summary>
    public void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Disposes all aggregated providers that implement the IDisposable interface. 
    /// </summary>
    ~AggregatingSourceLocationProvider() {
      this.Close();
    }

    private void Close() {
      foreach (var p in this.unit2Provider.Values) {
        IDisposable d = p as IDisposable;
        if (d != null)
          d.Dispose();
      }
    }

    #region Helper methods

    private IMethodDefinition lastUsedMethod = Dummy.Method;
    private ISourceLocationProvider lastUsedProvider = default(ISourceLocationProvider);
    private ISourceLocationProvider/*?*/ GetProvider(IMethodDefinition methodDefinition) {
      if (methodDefinition == lastUsedMethod) return lastUsedProvider;
      ISourceLocationProvider provider = null;
      var definingUnit = TypeHelper.GetDefiningUnit(methodDefinition.ResolvedMethod.ContainingTypeDefinition);
      this.unit2Provider.TryGetValue(definingUnit, out provider);
      if (provider != null) {
        this.lastUsedMethod = methodDefinition;
        this.lastUsedProvider = provider;
      }
      return provider;
    }

    private ISourceLocationProvider/*?*/ GetProvider(IILLocation/*?*/ mbLocation) {
      if (mbLocation == null) return null;
      return this.GetProvider(mbLocation.MethodDefinition);
    }

    private ISourceLocationProvider/*?*/ GetProvider(ILocalDefinition localDefinition) {
      return this.GetProvider(localDefinition.MethodDefinition);
    }

    private IEnumerable<IPrimarySourceLocation/*?*/> MapLocationToSourceLocations(ILocation location) {
      IILLocation/*?*/ mbLocation = location as IILLocation;
      ISourceLocationProvider provider = this.GetProvider(mbLocation);
      if (provider != null)
        foreach (var psloc in provider.GetPrimarySourceLocationsFor(location))
          yield return psloc;
    }

    #endregion

  }

  /// <summary>
  /// A provider that aggregates a set of providers in order to
  /// map offsets in an IL stream to block scopes.
  /// </summary>
  public sealed class AggregatingLocalScopeProvider : ILocalScopeProvider, IDisposable {

    Dictionary<IUnit, ILocalScopeProvider> unit2Provider = new Dictionary<IUnit, ILocalScopeProvider>();

    /// <summary>
    /// Copies the contents of the table
    /// </summary>
    public AggregatingLocalScopeProvider(Dictionary<IUnit, ILocalScopeProvider> unit2ProviderMap) {
      foreach (var keyValuePair in unit2ProviderMap) {
        this.unit2Provider.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }

    #region ILocalScopeProvider Members

    /// <summary>
    /// Returns zero or more local (block) scopes, each defining an IL range in which an iterator local is defined.
    /// The scopes are returned by the MoveNext method of the object returned by the iterator method.
    /// The index of the scope corresponds to the index of the local. Specifically local scope i corresponds
    /// to the local stored in field &lt;localName&gt;x_i of the class used to store the local values in between
    /// calls to MoveNext.
    /// </summary>
    public IEnumerable<ILocalScope> GetIteratorScopes(IMethodBody methodBody) {
      ILocalScopeProvider/*?*/ provider = this.GetProvider(methodBody.MethodDefinition);
      if (provider == null) {
        return Enumerable<ILocalScope>.Empty;
      } else {
        return provider.GetIteratorScopes(methodBody);
      }
    }

    /// <summary>
    /// Returns zero or more local (block) scopes into which the CLR IL operations in the given method body is organized.
    /// </summary>
    public IEnumerable<ILocalScope> GetLocalScopes(IMethodBody methodBody) {
      ILocalScopeProvider/*?*/ provider = this.GetProvider(methodBody.MethodDefinition);
      if (provider == null) {
        return Enumerable<ILocalScope>.Empty;
      } else {
        return provider.GetLocalScopes(methodBody);
      }
    }

    /// <summary>
    /// Returns zero or more namespace scopes into which the namespace type containing the given method body has been nested.
    /// These scopes determine how simple names are looked up inside the method body. There is a separate scope for each dotted
    /// component in the namespace type name. For istance namespace type x.y.z will have two namespace scopes, the first is for the x and the second
    /// is for the y.
    /// </summary>
    public IEnumerable<INamespaceScope> GetNamespaceScopes(IMethodBody methodBody) {
      ILocalScopeProvider/*?*/ provider = this.GetProvider(methodBody.MethodDefinition);
      if (provider == null) {
        return Enumerable<INamespaceScope>.Empty;
      } else {
        return provider.GetNamespaceScopes(methodBody);
      }
    }

    /// <summary>
    /// Returns zero or more local constant definitions that are local to the given scope.
    /// </summary>
    public IEnumerable<ILocalDefinition> GetConstantsInScope(ILocalScope scope) {
      ILocalScopeProvider/*?*/ provider = this.GetProvider(scope.MethodDefinition);
      if (provider == null) {
        return Enumerable<ILocalDefinition>.Empty;
      } else {
        return provider.GetConstantsInScope(scope);
      }
    }

    /// <summary>
    /// Returns zero or more local variable definitions that are local to the given scope.
    /// </summary>
    public IEnumerable<ILocalDefinition> GetVariablesInScope(ILocalScope scope) {
      ILocalScopeProvider/*?*/ provider = this.GetProvider(scope.MethodDefinition);
      if (provider == null) {
        return Enumerable<ILocalDefinition>.Empty;
      } else {
        return provider.GetVariablesInScope(scope);
      }
    }

    /// <summary>
    /// Returns true if the method body is an iterator.
    /// </summary>
    public bool IsIterator(IMethodBody methodBody) {
      var provider = this.GetProvider(methodBody.MethodDefinition);
      if (provider == null) return false;
      return provider.IsIterator(methodBody);
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Calls Dispose on all aggregated providers.
    /// </summary>
    public void Dispose() {
      this.Close();
      GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Finalizer for the aggregrating local scope provider. Calls Dispose on
    /// all aggregated providers.
    /// </summary>
    ~AggregatingLocalScopeProvider() {
      this.Close();
    }

    private void Close() {
      foreach (var p in this.unit2Provider.Values) {
        IDisposable d = p as IDisposable;
        if (d != null)
          d.Dispose();
      }
    }

    #region Helper methods

    private IMethodDefinition lastUsedMethod = Dummy.Method;
    private ILocalScopeProvider lastUsedProvider = null;
    private ILocalScopeProvider/*?*/ GetProvider(IMethodDefinition methodDefinition) {
      if (methodDefinition == lastUsedMethod) return lastUsedProvider;
      ILocalScopeProvider provider = null;
      var definingUnit = TypeHelper.GetDefiningUnit(methodDefinition.ResolvedMethod.ContainingTypeDefinition);
      this.unit2Provider.TryGetValue(definingUnit, out provider);
      if (provider != null) {
        this.lastUsedMethod = methodDefinition;
        this.lastUsedProvider = provider;
      }
      return provider;
    }

    private ILocalScopeProvider/*?*/ GetProvider(IILLocation/*?*/ mbLocation) {
      if (mbLocation == null) return null;
      return this.GetProvider(mbLocation.MethodDefinition);
    }

    private ILocalScopeProvider/*?*/ GetProvider(ILocalDefinition localDefinition) {
      return this.GetProvider(localDefinition.MethodDefinition);
    }

    #endregion

  }
}