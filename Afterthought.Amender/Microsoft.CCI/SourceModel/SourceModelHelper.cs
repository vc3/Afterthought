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
using Microsoft.Cci.UtilityDataStructures;

namespace Microsoft.Cci {
  /// <summary>
  /// A provider that aggregates a set of providers in order to
  /// map offsets in an IL stream to source locations.
  /// </summary>
  [ContractVerification(false)]
  public sealed class AggregatingSourceLocationProvider : ISourceLocationProvider, IDisposable {

    Dictionary<IUnit, ISourceLocationProvider> unit2Provider = new Dictionary<IUnit, ISourceLocationProvider>();

    /// <summary>
    /// Copies the contents of the table
    /// </summary>
    public AggregatingSourceLocationProvider(IDictionary<IUnit, ISourceLocationProvider> unit2ProviderMap) {
      foreach (var keyValuePair in unit2ProviderMap) {
        this.unit2Provider.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }

    /// <summary>
    /// Uses the given dictionary to find the appropriate provider for a query.
    /// </summary>
    public AggregatingSourceLocationProvider(Dictionary<IUnit, ISourceLocationProvider> unit2ProviderMap) {
      this.unit2Provider = unit2ProviderMap;
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

    private IMethodDefinition lastUsedMethod = Dummy.MethodDefinition;
    private ISourceLocationProvider lastUsedProvider = default(ISourceLocationProvider);

    private ISourceLocationProvider/*?*/ GetProvider(IMethodDefinition methodDefinition) {
      Contract.Requires(methodDefinition != null);
      if (methodDefinition == lastUsedMethod) return lastUsedProvider;
      ISourceLocationProvider provider = null;
      var definingUnit = TypeHelper.GetDefiningUnit(methodDefinition.ContainingTypeDefinition);
      this.unit2Provider.TryGetValue(definingUnit, out provider);
      if (provider != null) {
        this.lastUsedMethod = methodDefinition;
        this.lastUsedProvider = provider;
        return provider;
      }
      foreach (var location in methodDefinition.Locations) {
        var ilLocation = location as IILLocation;
        if (ilLocation == null || ilLocation.MethodDefinition == methodDefinition) continue;
        return this.GetProvider(ilLocation.MethodDefinition);
      }
      return null;
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
  [ContractVerification(false)]
  public sealed class AggregatingLocalScopeProvider : ILocalScopeProvider, IDisposable {

    readonly Dictionary<IUnit, ILocalScopeProvider> unit2Provider = new Dictionary<IUnit, ILocalScopeProvider>();

    /// <summary>
    /// Copies the contents of the table
    /// </summary>
    public AggregatingLocalScopeProvider(IDictionary<IUnit, ILocalScopeProvider> unit2ProviderMap) {
      Contract.Requires(unit2ProviderMap != null);

      foreach (var keyValuePair in unit2ProviderMap) {
        this.unit2Provider.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }

    /// <summary>
    /// Uses the given dictionary to find the appropriate provider for a query.
    /// </summary>
    /// <param name="unit2ProviderMap"></param>
    public AggregatingLocalScopeProvider(Dictionary<IUnit, ILocalScopeProvider> unit2ProviderMap) {
      Contract.Requires(unit2ProviderMap != null);

      this.unit2Provider = unit2ProviderMap;
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

    /// <summary>
    /// If the given method body is the "MoveNext" method of the state class of an asynchronous method, the returned
    /// object describes where synchronization points occur in the IL operations of the "MoveNext" method. Otherwise
    /// the result is null.
    /// </summary>
    public ISynchronizationInformation/*?*/ GetSynchronizationInformation(IMethodBody methodBody) {
      var provider = this.GetProvider(methodBody.MethodDefinition);
      if (provider == null) return null;
      return provider.GetSynchronizationInformation(methodBody);
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

    private IMethodDefinition lastUsedMethod = Dummy.MethodDefinition;
    private ILocalScopeProvider lastUsedProvider = null;

    private ILocalScopeProvider/*?*/ GetProvider(IMethodDefinition methodDefinition) {
      Contract.Requires(methodDefinition != null);

      if (methodDefinition == lastUsedMethod) return lastUsedProvider;
      ILocalScopeProvider provider = null;
      var definingUnit = TypeHelper.GetDefiningUnit(methodDefinition.ContainingTypeDefinition);
      this.unit2Provider.TryGetValue(definingUnit, out provider);
      if (provider != null) {
        this.lastUsedMethod = methodDefinition;
        this.lastUsedProvider = provider;
        return provider;
      }
      foreach (var location in methodDefinition.Locations) {
        var ilLocation = location as IILLocation;
        if (ilLocation == null || ilLocation.MethodDefinition == methodDefinition) continue;
        return this.GetProvider(ilLocation.MethodDefinition);
      }
      return null;
    }

    private ILocalScopeProvider/*?*/ GetProvider(IILLocation/*?*/ mbLocation) {
      if (mbLocation == null) return null;
      return this.GetProvider(mbLocation.MethodDefinition);
    }

    private ILocalScopeProvider/*?*/ GetProvider(ILocalDefinition localDefinition) {
      Contract.Requires(localDefinition != null);

      return this.GetProvider(localDefinition.MethodDefinition);
    }

    #endregion

  }

  /// <summary>
  /// A local scope provider that can be used together with an object model that is a deep copy made by MetadataDeepCopier. It ensures that all
  /// metadata definitions obtained from the original ILocalScopeProvider have been mapped to their copies and does the reverse map before
  /// passing queries along to the original ILocalScopeProvider.
  /// </summary>
  public sealed class CopiedLocalScopeProvider : ILocalScopeProvider {

    /// <summary>
    /// A local scope provider that can be used together with an object model that is a deep copy made by MetadataDeepCopier. It ensures that all
    /// metadata definitions obtained from the original ILocalScopeProvider have been mapped to their copies and does the reverse map before
    /// passing queries along to the original ILocalScopeProvider.
    /// </summary>
    /// <param name="mapFromCopyToOriginal">A map from copied definition objects to the original definitions from which the copies were constructed.</param>
    /// <param name="mapFromOriginalToCopy">A map from original definition objects to the copied definitions.</param>
    /// <param name="providerForOriginal">An ILocalScopeProvider associated with the original object model.</param>
    public CopiedLocalScopeProvider(Hashtable<object, object> mapFromCopyToOriginal, Hashtable<object, object> mapFromOriginalToCopy, ILocalScopeProvider providerForOriginal) {
      Contract.Requires(mapFromCopyToOriginal != null);
      Contract.Requires(mapFromOriginalToCopy != null);
      Contract.Requires(providerForOriginal != null);

      this.mapFromCopyToOriginal = mapFromCopyToOriginal;
      this.mapFromOriginalToCopy = mapFromOriginalToCopy;
      this.providerForOriginal = providerForOriginal;
    }

    /// <summary>
    /// A map from copied definition objects to the original definitions from which the copies were constructed.
    /// </summary>
    private Hashtable<object, object> mapFromCopyToOriginal;

    /// <summary>
    /// A map from original definition objects to the copied definitions.
    /// </summary>
    private Hashtable<object, object> mapFromOriginalToCopy;

    /// <summary>
    /// An ILocalScopeProvider associated with the original object model.
    /// </summary>
    ILocalScopeProvider providerForOriginal;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.mapFromCopyToOriginal != null);
      Contract.Invariant(this.mapFromOriginalToCopy != null);
      Contract.Invariant(this.providerForOriginal != null);
    }

    #region ILocalScopeProvider Members

    IEnumerable<ILocalScope> ILocalScopeProvider.GetIteratorScopes(IMethodBody methodBody) {
      var originalMethod = (this.mapFromCopyToOriginal[methodBody.MethodDefinition] as IMethodDefinition)??methodBody.MethodDefinition;
      Contract.Assume(!originalMethod.IsAbstract && !originalMethod.IsExternal);
      foreach (var localScope in this.providerForOriginal.GetIteratorScopes(originalMethod.Body)) {
        Contract.Assume(localScope != null);
        yield return new CopiedLocalScope(localScope, methodBody);
      }
    }

    IEnumerable<ILocalScope> ILocalScopeProvider.GetLocalScopes(IMethodBody methodBody) {
      var originalMethod = (this.mapFromCopyToOriginal[methodBody.MethodDefinition] as IMethodDefinition)??methodBody.MethodDefinition;
      Contract.Assume(!originalMethod.IsAbstract && !originalMethod.IsExternal);
      foreach (var localScope in this.providerForOriginal.GetLocalScopes(originalMethod.Body)) {
        Contract.Assume(localScope != null);
        yield return new CopiedLocalScope(localScope, methodBody);
      }
    }

    IEnumerable<INamespaceScope> ILocalScopeProvider.GetNamespaceScopes(IMethodBody methodBody) {
      var originalMethod = (this.mapFromCopyToOriginal[methodBody.MethodDefinition] as IMethodDefinition)??methodBody.MethodDefinition;
      Contract.Assume(!originalMethod.IsAbstract && !originalMethod.IsExternal);
      return this.providerForOriginal.GetNamespaceScopes(originalMethod.Body);
    }

    IEnumerable<ILocalDefinition> ILocalScopeProvider.GetConstantsInScope(ILocalScope scope) {
      Contract.Assume(!scope.MethodDefinition.IsAbstract && !scope.MethodDefinition.IsExternal);
      IMethodBody body = scope.MethodDefinition.Body;
      var copiedScope = scope as CopiedLocalScope;
      if (copiedScope != null) scope = copiedScope.OriginalScope;
      foreach (var localDef in this.providerForOriginal.GetConstantsInScope(scope)) {
        Contract.Assume(localDef != null);
        yield return this.GetCorrespondingLocal(localDef);
      }
    }

    IEnumerable<ILocalDefinition> ILocalScopeProvider.GetVariablesInScope(ILocalScope scope) {
      Contract.Assume(!scope.MethodDefinition.IsAbstract && !scope.MethodDefinition.IsExternal);
      IMethodBody body = scope.MethodDefinition.Body;
      var copiedScope = scope as CopiedLocalScope;
      if (copiedScope != null) scope = copiedScope.OriginalScope;
      foreach (var localDef in this.providerForOriginal.GetVariablesInScope(scope)) {
        Contract.Assume(localDef != null);
        yield return this.GetCorrespondingLocal(localDef);
      }
    }

    private ILocalDefinition GetCorrespondingLocal(ILocalDefinition originalLocalDef) {
      Contract.Requires(originalLocalDef != null);
      Contract.Ensures(Contract.Result<ILocalDefinition>() != null);

      return (this.mapFromOriginalToCopy[originalLocalDef] as ILocalDefinition)??originalLocalDef;
    }

    bool ILocalScopeProvider.IsIterator(IMethodBody methodBody) {
      var originalMethod = (this.mapFromCopyToOriginal[methodBody.MethodDefinition] as IMethodDefinition)??methodBody.MethodDefinition;
      Contract.Assume(!originalMethod.IsAbstract && !originalMethod.IsExternal);
      return this.providerForOriginal.IsIterator(originalMethod.Body);
    }

    [ContractVerification(false)]
    ISynchronizationInformation/*?*/ ILocalScopeProvider.GetSynchronizationInformation(IMethodBody methodBody) {
      var originalMethod = (this.mapFromCopyToOriginal[methodBody.MethodDefinition] as IMethodDefinition)??methodBody.MethodDefinition;
      Contract.Assume(!originalMethod.IsAbstract && !originalMethod.IsExternal);
      var originalSyncInfo = this.providerForOriginal.GetSynchronizationInformation(originalMethod.Body);
      if (originalSyncInfo == null) return null;
      return new CopiedSynchronizationInformation(originalSyncInfo, this.mapFromOriginalToCopy);
    }

    #endregion
  }

  internal sealed class CopiedLocalScope : ILocalScope {

    internal CopiedLocalScope(ILocalScope originalScope, IMethodBody copiedMethodBody) {
      Contract.Requires(originalScope != null);
      Contract.Requires(copiedMethodBody != null);
      this.originalScope = originalScope;
      this.copiedMethodBody = copiedMethodBody;
    }

    IMethodBody copiedMethodBody;
    ILocalScope originalScope;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.copiedMethodBody != null);
      Contract.Invariant(this.originalScope != null);
    }

    uint ILocalScope.Length {
      get { return this.originalScope.Length; }
    }

    IMethodDefinition ILocalScope.MethodDefinition {
      get { return this.copiedMethodBody.MethodDefinition; }
    }

    uint ILocalScope.Offset {
      get { return this.originalScope.Offset; } 
    }

    /// <summary>
    /// The scope that was copied to make this one.
    /// </summary>
    internal ILocalScope OriginalScope {
      get {
        Contract.Ensures(Contract.Result<ILocalScope>() != null);
        return this.originalScope;
      }
    }

  }

  internal sealed class CopiedSynchronizationInformation : ISynchronizationInformation {

    internal CopiedSynchronizationInformation(ISynchronizationInformation originalSyncrhonizationInformation, Hashtable<object, object> mapFromOriginaltoCopy) {
      Contract.Requires(originalSyncrhonizationInformation != null);
      Contract.Requires(mapFromOriginaltoCopy != null);
      this.originalSyncrhonizationInformation = originalSyncrhonizationInformation;
      this.mapFromOriginaltoCopy = mapFromOriginaltoCopy;
    }

    Hashtable<object, object> mapFromOriginaltoCopy;
    ISynchronizationInformation originalSyncrhonizationInformation;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.mapFromOriginaltoCopy != null);
      Contract.Invariant(this.originalSyncrhonizationInformation != null);
    }

    #region ISynchronizationInformation Members

    IMethodDefinition ISynchronizationInformation.AsyncMethod {
      get {
        var originalMethod = this.originalSyncrhonizationInformation.AsyncMethod;
        return (this.mapFromOriginaltoCopy[originalMethod] as IMethodDefinition)??originalMethod;
      }
    }

    IMethodDefinition ISynchronizationInformation.MoveNextMethod {
      get {
        var originalMethod = this.originalSyncrhonizationInformation.MoveNextMethod;
        return (this.mapFromOriginaltoCopy[originalMethod] as IMethodDefinition)??originalMethod;
      }
    }

    uint ISynchronizationInformation.GeneratedCatchHandlerOffset {
      get { return this.originalSyncrhonizationInformation.GeneratedCatchHandlerOffset; }
    }

    IEnumerable<ISynchronizationPoint> ISynchronizationInformation.SynchronizationPoints {
      get {
        foreach (var syncPoint in this.originalSyncrhonizationInformation.SynchronizationPoints)
          yield return new CopiedSynchronizationPoint(syncPoint, this.mapFromOriginaltoCopy);
      }
    }

    #endregion
  }

  internal sealed class CopiedSynchronizationPoint : ISynchronizationPoint {

    internal CopiedSynchronizationPoint(ISynchronizationPoint original, Hashtable<object, object> mapFromOriginaltoCopy) {
      Contract.Requires(original != null);
      Contract.Requires(mapFromOriginaltoCopy != null);
      this.original = original;
      this.mapFromOriginaltoCopy = mapFromOriginaltoCopy;
    }

    Hashtable<object, object> mapFromOriginaltoCopy;
    ISynchronizationPoint original;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.mapFromOriginaltoCopy != null);
      Contract.Invariant(this.original != null);
    }

    #region ISynchronizationPoint Members

    uint ISynchronizationPoint.SynchronizeOffset {
      get { return this.original.SynchronizeOffset; }
    }

    IMethodDefinition/*?*/ ISynchronizationPoint.ContinuationMethod {
      get {
        var originalMethod = this.original.ContinuationMethod;
        if (originalMethod == null) return null;
        return (this.mapFromOriginaltoCopy[originalMethod] as IMethodDefinition)??originalMethod;
      }
    }

    uint ISynchronizationPoint.ContinuationOffset {
      get { return this.original.ContinuationOffset; }
    }

    #endregion
  }
}