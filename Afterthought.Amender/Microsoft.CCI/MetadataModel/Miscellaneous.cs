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
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// A helper class for efficiently getting hold of empty enumerables.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public static class Enumerable<T> {
    /// <summary>
    /// An empty enumerable of element type T.
    /// </summary>
    public static IEnumerable<T> Empty {
      [ContractVerification(false)] //ensures unproven: Contract.ForAll(Contract.Result<IEnumerable<T>>(), x => x != null)
      get {
        Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<T>>(), x => x != null));
        return empty;
      }
    }
    static IEnumerable<T> empty = new EmptyEnumerable();

    sealed class EmptyEnumerable : IEnumerable<T>, IEnumerator<T> {
      #region IEnumerable<T> Members

      public IEnumerator<T> GetEnumerator() {
        return this;
      }

      #endregion

      #region IEnumerable Members

      IEnumerator IEnumerable.GetEnumerator() {
        return this;
      }

      #endregion

      #region IEnumerator<T> Members

      public T Current {
        get { throw new InvalidOperationException(); }
      }

      #endregion

      #region IDisposable Members

      public void Dispose() {
      }

      #endregion

      #region IEnumerator Members

      object IEnumerator.Current {
        get { throw new InvalidOperationException(); }
      }

      public bool MoveNext() {
        return false;
      }

      public void Reset() {
      }

      #endregion
    }
  }

  /// <summary>
  /// A Function that takes a single argument of type P and returns a value of type R.
  /// </summary>
  public delegate R Function<P, R>(P p);

  /// <summary>
  /// A Function that takes an argument of type P, an argument of type Q, and returns a value of type R.
  /// </summary>
  public delegate R Function<P, Q, R>(P p, Q q);

  /// <summary>
  /// An action that takes an argument of type P and an argument of type R.
  /// </summary>
  public delegate void Action<P, R>(P p, R r);

  /// <summary>
  /// A container for static helper methods that are used for manipulating and computing iterators.
  /// </summary>
  public static class IteratorHelper {

    /// <summary>
    /// Compares two enumerations of elements for equality by calling the Equals method on each pair of elements.
    /// The enumerations must be of equal length, or must both be null, in order to be considered equal.
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="left">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
    /// <param name="right">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
    public static bool EnumerablesAreEqual<T>(IEnumerable<T/*!*/>/*?*/ left, IEnumerable<T/*!*/>/*?*/ right) {
      if (left == null) return right == null || !right.GetEnumerator().MoveNext();
      IEnumerator<T/*!*/> leftEnum = left.GetEnumerator();
      if (right == null) return !leftEnum.MoveNext();
      IEnumerator<T/*!*/> rightEnum = right.GetEnumerator();
      while (leftEnum.MoveNext()) {
        if (!rightEnum.MoveNext()) return false;
        //^ assume false; //The verifier can't work out that the Current property is never null, despite the type annotation
        if (!leftEnum.Current.Equals(rightEnum.Current)) return false;
      }
      return !rightEnum.MoveNext();
    }

    /// <summary>
    /// Compares two enumerations of elements for equality by calling the Equals method on each pair of elements.
    /// The enumerations must be of equal length, or must both be null, in order to be considered equal.
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="left">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
    /// <param name="right">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
    /// <param name="comparer">An object that compares two enumeration elements for equality.</param>
    public static bool EnumerablesAreEqual<T>(IEnumerable<T/*!*/>/*?*/ left, IEnumerable<T/*!*/>/*?*/ right, IEqualityComparer<T> comparer) {
      Contract.Requires(comparer != null);
      if (left == null) return right == null || !right.GetEnumerator().MoveNext();
      IEnumerator<T/*!*/> leftEnum = left.GetEnumerator();
      if (right == null) return !leftEnum.MoveNext();
      IEnumerator<T/*!*/> rightEnum = right.GetEnumerator();
      while (leftEnum.MoveNext()) {
        if (!rightEnum.MoveNext()) return false;
        //^ assume false; //The verifier can't work out that the Current property is never null, despite the type annotation
        if (!comparer.Equals(leftEnum.Current, rightEnum.Current)) return false;
      }
      return !rightEnum.MoveNext();
    }

    /// <summary>
    /// Returns an array whose elements are those returned by the given enumerable.
    /// </summary>
    public static T[] GetAsArray<T>(IEnumerable<T> enumerable) {
      Contract.Ensures(Contract.Result<T[]>() != null);
      if (enumerable == null) return new T[0];
      var collection = enumerable as ICollection<T>;
      if (collection != null) {
        var n = collection.Count;
        var a = new T[n];
        collection.CopyTo(a, 0);
        return a;
      }
      var list = new List<T>(enumerable);
      return list.ToArray();
    }

    /// <summary>
    /// Returns an enumerable containing no objects.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Please use Enumerable<T>.Empty instead")]
    public static IEnumerable<T> GetEmptyEnumerable<T>() {
      yield break;
    }

    /// <summary>
    /// Returns an enumerable wrapper for the array that cannot be cast back down to the array. If the array is null, the result is null.
    /// </summary>
    /// <typeparam name="T">The element type of the array.</typeparam>
    /// <param name="array">The array to wrap. May be null.</param>
    public static IEnumerable<T>/*?*/ GetReadonly<T>(T[]/*?*/ array) {
      Contract.Ensures(array == null || Contract.Result<IEnumerable<T>>() != null);

      if (array == null) return null;
      if (array.Length == 1) return IteratorHelper.GetSingletonEnumerable(array[0]);
      return new ReaonlyOnlyArrayWrapper<T>(array);
    }

    sealed class ReaonlyOnlyArrayWrapper<T> : ICollection<T> {

      internal ReaonlyOnlyArrayWrapper(T[] array) {
        Contract.Requires(array != null);
        this.array = array;
      }

      T[] array;

      [ContractInvariantMethod]
      private void ObjectInvariant() {
        Contract.Invariant(this.array != null);
      }

      public IEnumerator<T> GetEnumerator() {
        return new Enumerator(this.array);
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
      }

      struct Enumerator : IEnumerator<T> {

        internal Enumerator(T[] array) {
          Contract.Requires(array != null);

          this.array = array;
          this.index = -1;
        }

        T[] array;
        int index;

        [ContractInvariantMethod]
        private void ObjectInvariant() {
          Contract.Invariant(this.array != null);
        }

        #region IEnumerator<T> Members

        public T Current {
          [ContractVerification(false)] 
          get { return this.array[this.index]; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
          get { return this.Current; }
        }

        public bool MoveNext() {
          return ++this.index < this.array.Length;
        }

        public void Reset() {
        }

        #endregion
      }


      #region ICollection<T> Members

      public void Add(T item) {
        throw new InvalidOperationException();
      }

      public void Clear() {
        throw new InvalidOperationException();
      }

      [ContractVerification(false)] //ensures unproven: !Contract.Result<bool>() || this.Count > 0
      public bool Contains(T item) {
        foreach (var elem in this.array) if (elem.Equals(item)) return true;
        return false;
      }

      public void CopyTo(T[] array, int arrayIndex) {
        Contract.Assume(this.array.Length+arrayIndex <= array.Length);
        for (int i = 0, n = this.array.Length; i < n; i++)
          array[i+arrayIndex] = this.array[i];
      }

      public int Count {
        get { return this.array.Length; }
      }

      public bool IsReadOnly {
        get { return true; }
      }

      public bool Remove(T item) {
        throw new InvalidOperationException();
      }

      #endregion
    }

    /// <summary>
    /// Returns an enumerable containing single object.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<T> GetSingletonEnumerable<T>(T t) {
      Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

      yield return t;
    }

    /// <summary>
    /// Returns an enumerable that acts like cast on enumeration.
    /// </summary>
    /// <returns></returns>
    [ContractVerification(false)] //ensures unproven: Contract.ForAll(Contract.Result<IEnumerable<TargetType>>(), x => x != null)
    public static IEnumerable<TargetType> GetConversionEnumerable<SourceType, TargetType>(IEnumerable<SourceType> sourceEnumeration) where SourceType : TargetType {
      Contract.Requires(sourceEnumeration != null);
      Contract.Requires(Contract.ForAll(sourceEnumeration, x => x != null));
      Contract.Ensures(Contract.Result<IEnumerable<TargetType>>() != null);
      Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<TargetType>>(), x => x != null));

      foreach (SourceType s in sourceEnumeration) {
        yield return s;
      }
    }

    /// <summary>
    /// Given an enumerable <paramref name="sourceEnumeration"/> the elements of which is of type <typeparamref name="SourceType"/> and a convertion 
    /// method <paramref name="convert"/> that computes a value of type <typeparamref name="TargetType"/> from a value of type <typeparamref name="SourceType"/>,
    /// return an enumerable of <typeparamref name="TargetType"/> elements. Basically, map over enumerables. 
    /// </summary>
    /// <typeparam name="SourceType"></typeparam>
    /// <typeparam name="TargetType"></typeparam>
    /// <param name="sourceEnumeration"></param>
    /// <param name="convert"></param>
    /// <returns></returns>
    public static IEnumerable<TargetType> GetConversionEnumerable<SourceType, TargetType>(IEnumerable<SourceType> sourceEnumeration, Func<SourceType, TargetType> convert) {
      foreach (SourceType s in sourceEnumeration) {
        yield return convert(s);
      }
    }

    /// <summary>
    /// Returns an enumerable that acts like cast on enumeration.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<TargetType> GetFilterEnumerable<SourceType, TargetType>(IEnumerable<SourceType> sourceEnumeration)
      where SourceType : class
      where TargetType : class {
      foreach (SourceType s in sourceEnumeration) {
        TargetType/*?*/ t = s as TargetType;
        if (t != null)
          yield return t;
      }
    }

    /// <summary>
    /// True if the given enumerable is not null and contains at least one element.
    /// </summary>
    [Pure]
    public static bool EnumerableIsNotEmpty<T>(IEnumerable<T>/*?*/ enumerable) {
      if (enumerable == null || enumerable == Enumerable<T>.Empty) return false;
      var collection = enumerable as ICollection<T>;
      if (collection != null) return collection.Count > 0;
      return enumerable.GetEnumerator().MoveNext();
    }

    /// <summary>
    /// True if the given enumerable is null or contains no elements
    /// </summary>
    [Pure]
    public static bool EnumerableIsEmpty<T>(IEnumerable<T>/*?*/ enumerable) {
      if (enumerable == null || enumerable == Enumerable<T>.Empty) return true;
      var collection = enumerable as ICollection<T>;
      if (collection != null) return collection.Count == 0;
      return !enumerable.GetEnumerator().MoveNext();
    }

    /// <summary>
    /// True if the given enumerable is not null and contains the given element.
    /// </summary>
    [Pure]
    public static bool EnumerableContains<T>(IEnumerable<T>/*?*/ enumerable, T element)
      where T : class
      // ^ ensures enumerable == null ==> result == false;
      // ^ ensures enumerable != null ==> result == exists{T t in enumerable; t == element};
    {
      if (enumerable == null) return false;
      foreach (T elem in enumerable)
        if (object.ReferenceEquals(elem, element)) return true;
      return false;
    }

    /// <summary>
    /// Returns the number of elements in the given enumerable. A null enumerable is allowed and results in 0.
    /// </summary>
    [Pure]
    public static uint EnumerableCount<T>(IEnumerable<T>/*?*/ enumerable) {
      Contract.Ensures(Contract.Result<uint>() >= 0);
      if (enumerable == null) return 0;
      var collection = enumerable as ICollection<T>;
      if (collection != null) return (uint)collection.Count;
      uint result = 0;
      IEnumerator<T> enumerator = enumerable.GetEnumerator();
      while (enumerator.MoveNext()) result++;
      return result & 0x7FFFFFFF;
    }

    /// <summary>
    /// Returns true if the number of elements in the given enumerable equals the specified length. A null enumerable is allowed and
    /// has length 0.
    /// </summary>
    [Pure]
    public static bool EnumerableHasLength<T>(IEnumerable<T>/*?*/ enumerable, ulong length) {
      if (enumerable == null) return (length == 0);
      var collection = enumerable as ICollection<T>;
      if (collection != null) return length == (ulong)collection.Count;
      IEnumerator<T> enumerator = enumerable.GetEnumerator();
      while (length > 0) {
        if (!enumerator.MoveNext()) return false;
        length--;
      }
      return (!enumerator.MoveNext());
    }

    /// <summary>
    /// Returns the single element from the given single element collection.
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="enumerable">An enumeration of elements.</param>
    /// <returns>The single element from the enumerable</returns>
    public static T Single<T>(IEnumerable<T> enumerable)
      //^ requires IteratorHelper.EnumerableHasLength(enumerable, 1);
    {
      Contract.Requires(enumerable != null);
      Contract.Requires(IteratorHelper.EnumerableHasLength(enumerable, 1));
      var e = enumerable.GetEnumerator();
      e.MoveNext();
      return e.Current;
    }

    /// <summary>
    /// Returns the first element from the given non-empty collection.
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="enumerable">An enumeration of elements.</param>
    /// <returns>The first element from the enumerable</returns>
    public static T First<T>(IEnumerable<T> enumerable)
      //^ requires IteratorHelper.EnumerableIsNotEmpty(enumerable);
    {
      Contract.Requires(enumerable != null);
      Contract.Requires(IteratorHelper.EnumerableIsNotEmpty(enumerable));
      var e = enumerable.GetEnumerator();
      e.MoveNext();
      return e.Current;
    }

    /// <summary>
    /// Returns true if any element of the sequence satisfies the predicate
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="enumerable">An enumeration of elements.</param>
    /// <param name="pred">The predicate to apply.</param>
    /// <returns>true if and only if pred was true for at least one element</returns>
    public static bool Any<T>(IEnumerable<T> enumerable, Predicate<T> pred) {
      Contract.Requires(enumerable != null);
      Contract.Requires(pred != null);
      foreach (var t in enumerable)
        if (pred(t))
          return true;
      return false;
    }

    /// <summary>
    /// Returns enumeration being a concatenation of parameters.
    /// </summary>
    /// <typeparam name="T">The element type of the collection</typeparam>
    /// <param name="left">An enumeration of elements. The enumeration may be null.</param>
    /// <param name="right">An enumeration of elements. The enumeration may be null.</param>
    public static IEnumerable<T> Concat<T>(IEnumerable<T>/*?*/ left, IEnumerable<T>/*?*/ right) {
      if (left != null)
        foreach (T e in left)
          yield return e;
      if (right != null)
        foreach (T e in right)
          yield return e;
    }

    /// <summary>
    /// A zip join implementation that walks two enumerables performing some function
    /// on corresponding elements and returning an enumerable of the result of the
    /// function.
    /// </summary>
    public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Function<TFirst, TSecond, TResult> resultSelector) {
      Contract.Requires(first != null);
      Contract.Requires(second != null);
      Contract.Requires(resultSelector != null);
      using (IEnumerator<TFirst> e1 = first.GetEnumerator())
      using (IEnumerator<TSecond> e2 = second.GetEnumerator())
        while (e1.MoveNext() && e2.MoveNext())
          yield return resultSelector(e1.Current, e2.Current);
    }
    /// <summary>
    /// A zip join implementation that walks two enumerables performing some action
    /// on corresponding elements.
    /// </summary>
    public static void Zip<TFirst, TSecond>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Action<TFirst, TSecond> action) {
      Contract.Requires(first != null);
      Contract.Requires(second != null);
      Contract.Requires(action != null);
      using (IEnumerator<TFirst> e1 = first.GetEnumerator())
      using (IEnumerator<TSecond> e2 = second.GetEnumerator())
        while (e1.MoveNext() && e2.MoveNext())
          action(e1.Current, e2.Current);
    } 

  }

  /// <summary>
  /// A container for static helper methods that are used to test identities for equality.
  /// </summary>
  public static class ObjectModelHelper {

    /// <summary>
    /// Returns a hash code based on the string content. Strings that differ only in case will always have the same hash code.
    /// </summary>
    /// <param name="s">The string to hash.</param>
    public static int CaseInsensitiveStringHash(string s) {
      Contract.Requires(s != null);
      int hashCode = 0;
      for (int i = 0, n = s.Length; i < n; i++) {
        char ch = s[i];
        ch = Char.ToLower(ch, CultureInfo.InvariantCulture);
        hashCode = hashCode * 17 + ch;
      }
      return hashCode;
    }

    /// <summary>
    /// Returns a hash code based on the string content. Strings that differ only in case will always have the same hash code.
    /// </summary>
    /// <param name="s">The string to hash.</param>
    public static int CaseSensitiveStringHash(string s) {
      Contract.Requires(s != null);
      int hashCode = 0;
      for (int i = 0, n = s.Length; i < n; i++) {
        char ch = s[i];
        hashCode = hashCode * 17 + ch;
      }
      return hashCode;
    }

  }

  /// <summary>
  /// A single CLR IL operation.
  /// </summary>
  [ContractClass(typeof(IOperationContract))]
  public interface IOperation {

    /// <summary>
    /// The actual value of the operation code
    /// </summary>
    OperationCode OperationCode { get; }

    /// <summary>
    /// The offset from the start of the operation stream of a method
    /// </summary>
    uint Offset { get; }

    /// <summary>
    /// The location that corresponds to this instruction.
    /// </summary>
    ILocation Location { get; }

    /// <summary>
    /// Immediate data such as a string, the address of a branch target, or a metadata reference, such as a Field
    /// </summary>
    object/*?*/ Value {
      get;
    }
  }

  #region IOperation contract binding
  [ContractClassFor(typeof(IOperation))]
  abstract class IOperationContract : IOperation {
    public OperationCode OperationCode {
      get { throw new NotImplementedException(); }
    }

    public uint Offset {
      get { throw new NotImplementedException(); }
    }

    public ILocation Location {
      get {
        Contract.Ensures(Contract.Result<ILocation>() != null);
        throw new NotImplementedException(); 
      }
    }

    public object Value {
      get { throw new NotImplementedException(); }
    }
  }
  #endregion


  /// <summary>
  /// A metadata custom attribute.
  /// </summary>
  [ContractClass(typeof(ICustomAttributeContract))]
  public interface ICustomAttribute {

    /// <summary>
    /// Zero or more positional arguments for the attribute constructor.
    /// </summary>
    IEnumerable<IMetadataExpression> Arguments { get; }

    /// <summary>
    /// A reference to the constructor that will be used to instantiate this custom attribute during execution (if the attribute is inspected via Reflection).
    /// </summary>
    IMethodReference Constructor { get; }

    /// <summary>
    /// Zero or more named arguments that specify values for fields and properties of the attribute.
    /// </summary>
    IEnumerable<IMetadataNamedArgument> NamedArguments { get; }

    /// <summary>
    /// The number of named arguments.
    /// </summary>
    ushort NumberOfNamedArguments {
      get;
      //^ ensures result == IteratorHelper.EnumerableCount(this.NamedArguments);
    }

    /// <summary>
    /// The type of the attribute. For example System.AttributeUsageAttribute.
    /// </summary>
    ITypeReference Type { get; }
  }

  #region ICustomAttribute contract binding
  [ContractClassFor(typeof(ICustomAttribute))]
  abstract class ICustomAttributeContract : ICustomAttribute {
    #region ICustomAttribute Members

    public IEnumerable<IMetadataExpression> Arguments {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IMetadataExpression>>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IMethodReference Constructor {
      get {
        Contract.Ensures(Contract.Result<IMethodReference>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IEnumerable<IMetadataNamedArgument> NamedArguments {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<IMetadataNamedArgument>>() != null);
        throw new NotImplementedException();
      }
    }

    public ushort NumberOfNamedArguments {
      get { 
        throw new NotImplementedException();
      }
    }

    public ITypeReference Type {
      get {
        Contract.Ensures(Contract.Result<ITypeReference>() != null);
        throw new NotImplementedException();
      }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// Represents a file referenced by an assembly.
  /// </summary>
  public interface IFileReference {
    /// <summary>
    /// The assembly that references this file.
    /// </summary>
    IAssembly ContainingAssembly { get; }

    /// <summary>
    /// True if the file has metadata.
    /// </summary>
    bool HasMetadata { get; }

    /// <summary>
    /// Name of the file.
    /// </summary>
    IName FileName { get; }

    /// <summary>
    /// A hash of the file contents.
    /// </summary>
    IEnumerable<byte> HashValue { get; }
  }

  /// <summary>
  /// A named data resource that is stored as part of CLR metadata.
  /// </summary>
  public interface IResource : IResourceReference {

    /// <summary>
    /// The resource data.
    /// </summary>
    IEnumerable<byte> Data { get; }

    /// <summary>
    /// The resource is in external file
    /// </summary>
    bool IsInExternalFile { get; }

    /// <summary>
    /// The external file that contains the resource.
    /// </summary>
    IFileReference ExternalFile {
      get;
      //^ requires this.IsInExternalFile;
    }

  }

  /// <summary>
  /// A reference to an IResource instance.
  /// </summary>
  [ContractClass(typeof(IResourceReferenceContract))]
  public interface IResourceReference {

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this resource.
    /// </summary>
    IEnumerable<ICustomAttribute> Attributes { get; }

    /// <summary>
    /// A symbolic reference to the IAssembly that defines the resource.
    /// </summary>
    IAssemblyReference DefiningAssembly { get; }

    /// <summary>
    /// Specifies whether other code from other assemblies may access this resource.
    /// </summary>
    bool IsPublic { get; }

    /// <summary>
    /// The name of the resource.
    /// </summary>
    IName Name { get; }

    /// <summary>
    /// The referenced resource.
    /// </summary>
    IResource Resource { get; }
  }

  [ContractClassFor(typeof(IResourceReference))]
  abstract class IResourceReferenceContract : IResourceReference {
    public IEnumerable<ICustomAttribute> Attributes {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<ICustomAttribute>>() != null);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICustomAttribute>>(), x => x != null));
        throw new NotImplementedException();
      }
    }

    public IAssemblyReference DefiningAssembly {
      get {
        Contract.Ensures(Contract.Result<IAssemblyReference>() != null);
        throw new NotImplementedException();
      }
    }

    public bool IsPublic {
      get { throw new NotImplementedException(); }
    }

    public IName Name {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IResource Resource {
      get {
        Contract.Ensures(Contract.Result<IResource>() != null);
        throw new NotImplementedException();
      }
    }
  }

  /// <summary>
  /// Enum specifying the kinds of security actions
  /// </summary>
  public enum SecurityAction {
    /// <summary>
    /// No Security Action.
    /// </summary>
    ActionNil=0x0000,

    /// <summary>
    /// Specify the security requried to run.
    /// </summary>
    Request=0x0001,

    /// <summary>
    /// Check that all callers in the call chain have been granted specified permissions, all of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    Demand=0x0002,

    /// <summary>
    /// Without further checks, satisfy Demand for the specified permissions, all of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    Assert=0x0003,

    /// <summary>
    /// Without further checks refuse Demand for the specified permissions, all of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    Deny=0x0004,

    /// <summary>
    /// Without further checks, refuse Demand for all permissions other than those specified, all of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    PermitOnly=0x0005,

    /// <summary>
    /// Check that immediate caller has been granted the specified permissions, all of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    LinkDemand=0x0006,

    /// <summary>
    /// Check that all derived classes have been granted the specified permissions, all of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    InheritanceDemand=0x0007,

    /// <summary>
    /// Specify the minimum permissions required to run.
    /// </summary>
    RequestMinimum=0x0008,

    /// <summary>
    /// Specify the optional permissions to grant.
    /// </summary>
    RequestOptional=0x0009,

    /// <summary>
    /// Specify the permissions not to be granted.
    /// </summary>
    RequestRefuse=0x000A,

    /// <summary>
    /// Reserved for implementation specific use.
    /// </summary>
    PrejitGrant=0x000B,

    /// <summary>
    /// Reserved for implementation specific use.
    /// </summary>
    PrejitDenied=0x000C,

    /// <summary>
    /// Check that all callers in the call chain have been granted the specified permissions, none of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    NonCasDemand=0x000D,

    /// <summary>
    /// Check that the immediate caller has been granted the specified permissions, none of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    NonCasLinkDemand=0x000E,

    /// <summary>
    /// Check that all derived classes have been granted the specified permissions, none of which derive from System.Security.Permissions.CodeAccessPermission.
    /// </summary>
    NonCasInheritance=0x000F,
  }

  /// <summary>
  /// A declarative specification of a security action applied to a set of permissions. Used by the CLR loader to enforce security restrictions.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public interface ISecurityAttribute {
    /// <summary>
    /// Specifies the security action that can be performed using declarative security. For example the action could be Deny.
    /// </summary>
    SecurityAction Action { get; }

    /// <summary>
    /// Custom attributes that collectively define the permission set to which the action is applied. Each attribute represents a serialized permission
    /// or permission set. The union of the sets, together with the individual permissions, define the set to which the action applies.
    /// </summary>
    IEnumerable<ICustomAttribute> Attributes { get; }

  }

  /// <summary>
  /// Information about how values of managed types should be marshalled to and from unmanaged types.
  /// </summary>
  public interface IMarshallingInformation {
    /// <summary>
    /// A reference to the type implementing the custom marshaller.
    /// </summary>
    ITypeReference CustomMarshaller {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler;
    }

    /// <summary>
    /// An argument string (cookie) passed to the custom marshaller at run time.
    /// </summary>
    string CustomMarshallerRuntimeArgument {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.CustomMarshaler;
    }

    /// <summary>
    /// The unmanaged element type of the unmanaged array.
    /// </summary>
    System.Runtime.InteropServices.UnmanagedType ElementType {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray ||
      //^ this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray;
    }

    /// <summary>
    /// Specifies the index of the parameter that contains the value of the Interface Identifier (IID) of the marshalled object.
    /// </summary>
    uint IidParameterIndex {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.Interface;
    }

    /// <summary>
    /// The unmanaged type to which the managed type will be marshalled. This can be be UnmanagedType.CustomMarshaler, in which case the unmanaged type
    /// is decided at runtime.
    /// </summary>
    System.Runtime.InteropServices.UnmanagedType UnmanagedType { get; }

    /// <summary>
    /// The number of elements in the fixed size portion of the unmanaged array.
    /// </summary>
    uint NumberOfElements {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValArray ||
      //^ this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.ByValTStr ||
      //^ this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray;
    }

    /// <summary>
    /// The zero based index of the parameter in the unmanaged method that contains the number of elements in the variable portion of unmanaged array.
    /// If the index is null, the variable portion is of size zero, or the caller conveys the size of the variable portion of the array to the unmanaged method in some other way.
    /// </summary>
    uint? ParamIndex {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.LPArray;
    }

    /// <summary>
    /// The type to which the variant values of all elements of the safe array must belong. See also SafeArrayElementUserDefinedSubtype.
    /// (The element type of a safe array is VARIANT. The "sub type" specifies the value of all of the tag fields (vt) of the element values. )
    /// </summary>
    System.Runtime.InteropServices.VarEnum SafeArrayElementSubtype {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.SafeArray;
    }

    /// <summary>
    /// A reference to the user defined type to which the variant values of all elements of the safe array must belong.
    /// (The element type of a safe array is VARIANT. The tag fields will all be either VT_DISPATCH or VT_UNKNOWN or VT_RECORD.
    /// The "user defined sub type" specifies the type of value the ppdispVal/ppunkVal/pvRecord fields of the element values may point to.)
    /// </summary>
    ITypeReference SafeArrayElementUserDefinedSubtype {
      get;
      //^ requires this.UnmanagedType == System.Runtime.InteropServices.UnmanagedType.SafeArray;
      //^ requires this.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_DISPATCH ||
      //^          this.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_UNKNOWN ||
      //^          this.SafeArrayElementSubtype == System.Runtime.InteropServices.VarEnum.VT_RECORD;
    }

  }

  /// <summary>
  /// The name of an entity. Typically name instances come from a common pool. Within the pool no two distinct instances will have the same Value or UniqueKey.
  /// </summary>
  [ContractClass(typeof(INameContract))]
  public interface IName {
    /// <summary>
    /// An integer that is unique within the pool from which the name instance has been allocated. Useful as a hashtable key.
    /// </summary>
    int UniqueKey {
      get;
      //^ ensures result > 0;
    }

    /// <summary>
    /// An integer that is unique within the pool from which the name instance has been allocated. Useful as a hashtable key.
    /// All name instances in the pool that have the same string value when ignoring the case of the characters in the string
    /// will have the same key value.
    /// </summary>
    int UniqueKeyIgnoringCase {
      get;
      //^ ensures result > 0;
    }

    /// <summary>
    /// The string value corresponding to this name.
    /// </summary>
    string Value { get; }
  }

  [ContractClassFor(typeof(IName))]
  abstract class INameContract : IName {
    public int UniqueKey {
      get {
        Contract.Ensures(Contract.Result<int>() > 0);
        throw new NotImplementedException();
      }
    }

    public int UniqueKeyIgnoringCase {
      get {
        Contract.Ensures(Contract.Result<int>() > 0);
        throw new NotImplementedException();
      }
    }

    public string Value {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        throw new NotImplementedException();
      }
    }
  }


  /// <summary>
  /// Implemented by any entity that has a name.
  /// </summary>
  [ContractClass(typeof(INamedEntityContract))]
  public interface INamedEntity {
    /// <summary>
    /// The name of the entity.
    /// </summary>
    IName Name { get; }
  }

  [ContractClassFor(typeof(INamedEntity))]
  abstract class INamedEntityContract : INamedEntity {
    public IName Name {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }
  }


  /// <summary>
  /// An extensible collection of IName instances that represent names that are commonly used during compilation.
  /// </summary>
  [ContractClass(typeof(INameTableContract))]
  public interface INameTable {

    /// <summary>
    /// A name with no characters. Often used as a dummy.
    /// </summary>
    IName EmptyName { get; }

    /// <summary>
    /// Gets a cached IName instance corresponding to the given string. If no cached instance exists, a new instance is created.
    /// The method is only available to fully trusted code since it allows the caller to cause new objects to be added to the cache.
    /// </summary>
    [Pure]
    IName GetNameFor(string name);
    //^ ensures result.Value == name;

    /// <summary>
    /// "Address"
    /// </summary>
    IName Address { get; }

    /// <summary>
    /// "AllowMultiple"
    /// </summary>
    IName AllowMultiple { get; }

    /// <summary>
    /// "BeginInvoke"
    /// </summary>
    IName BeginInvoke { get; }

    /// <summary>
    /// "bool op bool"
    /// </summary>
    IName BoolOpBool { get; }

    /// <summary>
    /// "Combine"
    /// </summary>
    IName Combine { get; }

    /// <summary>
    /// "Concat"
    /// </summary>
    IName Concat { get; }

    /// <summary>
    /// "decimal op decimal"
    /// </summary>
    IName DecimalOpDecimal { get; }

    /// <summary>
    /// "delegate op delegate"
    /// </summary>
    IName DelegateOpDelegate { get; }

    /// <summary>
    /// "EndInvoke"
    /// </summary>
    IName EndInvoke { get; }

    /// <summary>
    /// "enum op enum"
    /// </summary>
    IName EnumOpEnum { get; }

    /// <summary>
    /// "enum op num"
    /// </summary>
    IName EnumOpNum { get; }

    /// <summary>
    /// "Equals"
    /// </summary>
    IName Equals { get; }

    /// <summary>
    /// "float32 op float32"
    /// </summary>
    IName Float32OpFloat32 { get; }

    /// <summary>
    /// "float64 op float64"
    /// </summary>
    IName Float64OpFloat64 { get; }

    /// <summary>
    /// "Get"
    /// </summary>
    IName Get { get; }

    /// <summary>
    /// "global"
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="Member")]
    IName global { get; }

    /// <summary>
    /// "HasValue"
    /// </summary>
    IName HasValue { get; }

    /// <summary>
    /// "Inherited"
    /// </summary>
    IName Inherited { get; }

    /// <summary>
    /// "Invoke"
    /// </summary>
    IName Invoke { get; }

    /// <summary>
    /// "int16 op int16"
    /// </summary>
    IName Int16OpInt16 { get; }

    /// <summary>
    /// "int32 op int32"
    /// </summary>
    IName Int32OpInt32 { get; }

    /// <summary>
    /// "int32 op uint32"
    /// </summary>
    IName Int32OpUInt32 { get; }

    /// <summary>
    /// "int64 op int32"
    /// </summary>
    IName Int64OpInt32 { get; }

    /// <summary>
    /// "int64 op uint32"
    /// </summary>
    IName Int64OpUInt32 { get; }

    /// <summary>
    /// "int64 op uint64"
    /// </summary>
    IName Int64OpUInt64 { get; }

    /// <summary>
    /// "int64 op int64"
    /// </summary>
    IName Int64OpInt64 { get; }

    /// <summary>
    /// "int8 op int8"
    /// </summary>
    IName Int8OpInt8 { get; }

    /// <summary>
    /// "operator ??(object, object)"
    /// </summary>
    IName NullCoalescing { get; }

    /// <summary>
    /// "num op enum"
    /// </summary>
    IName NumOpEnum { get; }

    /// <summary>
    /// "object op object"
    /// </summary>
    IName ObjectOpObject { get; }

    /// <summary>
    /// "object op string"
    /// </summary>
    IName ObjectOpString { get; }

    /// <summary>
    /// "op_Addition"
    /// </summary>
    IName OpAddition { get; }

    /// <summary>
    /// "op_BitwiseAnd"
    /// </summary>
    IName OpBitwiseAnd { get; }

    /// <summary>
    /// op_BitwiseOr
    /// </summary>
    IName OpBitwiseOr { get; }

    /// <summary>
    /// op enum
    /// </summary>
    IName OpEnum { get; }

    /// <summary>
    /// "op_Equality"
    /// </summary>
    IName OpEquality { get; }

    /// <summary>
    /// op_Explicit
    /// </summary>
    IName OpExplicit { get; }

    /// <summary>
    /// "op_Implicit"
    /// </summary>
    IName OpImplicit { get; }

    /// <summary>
    /// op_Inequality
    /// </summary>
    IName OpInequality { get; }

    /// <summary>
    /// "op int8"
    /// </summary>
    IName OpInt8 { get; }

    /// <summary>
    /// "op int16"
    /// </summary>
    IName OpInt16 { get; }

    /// <summary>
    /// "op int32"
    /// </summary>
    IName OpInt32 { get; }

    /// <summary>
    /// "op int64"
    /// </summary>
    IName OpInt64 { get; }

    /// <summary>
    /// "op_Comma"
    /// </summary>
    IName OpComma { get; }

    /// <summary>
    /// "op_Concatentation"
    /// </summary>
    IName OpConcatentation { get; }

    /// <summary>
    /// "op_Division"
    /// </summary>
    IName OpDivision { get; }

    /// <summary>
    /// "op_ExclusiveOr"
    /// </summary>
    IName OpExclusiveOr { get; }

    /// <summary>
    /// "op_Exponentiation"
    /// </summary>
    IName OpExponentiation { get; }

    /// <summary>
    /// "op_False"
    /// </summary>
    IName OpFalse { get; }

    /// <summary>
    /// "op float32"
    /// </summary>
    IName OpFloat32 { get; }

    /// <summary>
    /// "op float64"
    /// </summary>
    IName OpFloat64 { get; }

    /// <summary>
    /// "op_GreaterThan"
    /// </summary>
    IName OpGreaterThan { get; }

    /// <summary>
    /// "op_GreaterThanOrEqual"
    /// </summary>
    IName OpGreaterThanOrEqual { get; }

    /// <summary>
    /// "op_IntegerDivision"
    /// </summary>
    IName OpIntegerDivision { get; }

    /// <summary>
    /// "op_LeftShift"
    /// </summary>
    IName OpLeftShift { get; }

    /// <summary>
    /// "op_LessThan"
    /// </summary>
    IName OpLessThan { get; }

    /// <summary>
    /// "op_LessThanOrEqual"
    /// </summary>
    IName OpLessThanOrEqual { get; }

    /// <summary>
    /// "op_Like"
    /// </summary>
    IName OpLike { get; }

    /// <summary>
    /// "op_LogicalNot"
    /// </summary>
    IName OpLogicalNot { get; }

    /// <summary>
    /// "op_LogicalOr"
    /// </summary>
    IName OpLogicalOr { get; }

    /// <summary>
    /// "op_Modulus"
    /// </summary>
    IName OpModulus { get; }

    /// <summary>
    /// "op_Multiply"
    /// </summary>
    IName OpMultiply { get; }

    /// <summary>
    /// "op_OnesComplement"
    /// </summary>
    IName OpOnesComplement { get; }

    /// <summary>
    /// "op boolean"
    /// </summary>
    IName OpBoolean { get; }

    /// <summary>
    /// "op char"
    /// </summary>
    IName OpChar { get; }

    /// <summary>
    /// "op decimal"
    /// </summary>
    IName OpDecimal { get; }

    /// <summary>
    /// "op_Decrement"
    /// </summary>
    IName OpDecrement { get; }

    /// <summary>
    /// "op_Increment"
    /// </summary>
    IName OpIncrement { get; }

    /// <summary>
    /// op_RightShift
    /// </summary>
    IName OpRightShift { get; }

    /// <summary>
    /// "op_Subtraction"
    /// </summary>
    IName OpSubtraction { get; }

    /// <summary>
    /// "op_True"
    /// </summary>
    IName OpTrue { get; }

    /// <summary>
    /// "op uint8"
    /// </summary>
    IName OpUInt8 { get; }

    /// <summary>
    /// "op uint16"
    /// </summary>
    IName OpUInt16 { get; }

    /// <summary>
    /// "op uint32"
    /// </summary>
    IName OpUInt32 { get; }

    /// <summary>
    /// "op uint64"
    /// </summary>
    IName OpUInt64 { get; }

    /// <summary>
    /// "op_UnaryNegation"
    /// </summary>
    IName OpUnaryNegation { get; }

    /// <summary>
    /// "op_UnaryPlus"
    /// </summary>
    IName OpUnaryPlus { get; }

    /// <summary>
    /// "string op string"
    /// </summary>
    IName StringOpString { get; }

    /// <summary>
    /// "string op object"
    /// </summary>
    IName StringOpObject { get; }

    /// <summary>
    /// "uintPtr op uintPtr"
    /// </summary>
    IName UIntPtrOpUIntPtr { get; }

    /// <summary>
    /// "uint32 op int32"
    /// </summary>
    IName UInt32OpInt32 { get; }

    /// <summary>
    /// "uint32 op uint32"
    /// </summary>
    IName UInt32OpUInt32 { get; }

    /// <summary>
    /// "uint64 op int32"
    /// </summary>
    IName UInt64OpInt32 { get; }

    /// <summary>
    /// "uint64 op uint32"
    /// </summary>
    IName UInt64OpUInt32 { get; }

    /// <summary>
    /// "uint64 op uint64"
    /// </summary>
    IName UInt64OpUInt64 { get; }

    /// <summary>
    /// "value"
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="Member")]
    IName value { get; }

    /// <summary>
    /// "System"
    /// </summary>
    IName System { get; }

    /// <summary>
    /// Void
    /// </summary>
    IName Void { get; }

    /// <summary>
    /// void* op void*
    /// </summary>
    IName VoidPtrOpVoidPtr { get; }

    /// <summary>
    /// "Boolean"
    /// </summary>
    IName Boolean { get; }

    /// <summary>
    /// ".cctor"
    /// </summary>
    IName Cctor { get; }

    /// <summary>
    /// Char
    /// </summary>
    IName Char { get; }

    /// <summary>
    /// ".ctor"
    /// </summary>
    IName Ctor { get; }

    /// <summary>
    /// "Byte"
    /// </summary>
    IName Byte { get; }

    /// <summary>
    /// "SByte"
    /// </summary>
    IName SByte { get; }

    /// <summary>
    /// "Int16"
    /// </summary>
    IName Int16 { get; }

    /// <summary>
    /// "UInt16"
    /// </summary>
    IName UInt16 { get; }

    /// <summary>
    /// "Int32"
    /// </summary>
    IName Int32 { get; }

    /// <summary>
    /// "UInt32"
    /// </summary>
    IName UInt32 { get; }

    /// <summary>
    /// "Int64"
    /// </summary>
    IName Int64 { get; }

    /// <summary>
    /// "UInt64"
    /// </summary>
    IName UInt64 { get; }

    /// <summary>
    /// "String"
    /// </summary>
    IName String { get; }

    /// <summary>
    /// "IntPtr"
    /// </summary>
    IName IntPtr { get; }

    /// <summary>
    /// "UIntPtr"
    /// </summary>
    IName UIntPtr { get; }

    /// <summary>
    /// "Object"
    /// </summary>
    IName Object { get; }

    /// <summary>
    /// "Remove"
    /// </summary>
    IName Remove { get; }

    /// <summary>
    /// "result"
    /// </summary>
    IName Result { get; }

    /// <summary>
    /// "Set"
    /// </summary>
    IName Set { get; }

    /// <summary>
    /// "Single"
    /// </summary>
    IName Single { get; }

    /// <summary>
    /// "Double"
    /// </summary>
    IName Double { get; }

    /// <summary>
    /// "TypedReference"
    /// </summary>
    IName TypedReference { get; }

    /// <summary>
    /// "Enum"
    /// </summary>
    IName Enum { get; }

    /// <summary>
    /// "Length"
    /// </summary>
    IName Length { get; }

    /// <summary>
    /// "LongLength"
    /// </summary>
    IName LongLength { get; }

    /// <summary>
    /// "MulticastDelegate"
    /// </summary>
    IName MulticastDelegate { get; }

    /// <summary>
    /// "ValueType"
    /// </summary>
    IName ValueType { get; }

    /// <summary>
    /// "Type"
    /// </summary>
    IName Type { get; }

    /// <summary>
    /// "Array"
    /// </summary>
    IName Array { get; }

    /// <summary>
    /// "Attribute"
    /// </summary>
    IName Attribute { get; }

    /// <summary>
    /// "AttributeUsageAttribute"
    /// </summary>
    IName AttributeUsageAttribute { get; }

    /// <summary>
    /// "DateTime"
    /// </summary>
    IName DateTime { get; }

    /// <summary>
    /// "DebuggerHiddenAttribute"
    /// </summary>
    IName DebuggerHiddenAttribute { get; }

    /// <summary>
    /// "Decimal"
    /// </summary>
    IName Decimal { get; }

    /// <summary>
    /// "Delegate"
    /// </summary>
    IName Delegate { get; }

    /// <summary>
    /// "Diagnostics"
    /// </summary>
    IName Diagnostics { get; }

    /// <summary>
    /// "DBNull"
    /// </summary>
    IName DBNull { get; }

    /// <summary>
    /// "Nullable"
    /// </summary>
    IName Nullable { get; }

  }

  [ContractClassFor(typeof(INameTable))]
  abstract class INameTableContract : INameTable {
    public IName EmptyName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName GetNameFor(string name) {
      Contract.Ensures(Contract.Result<IName>() != null);
      Contract.Ensures(Contract.Result<IName>().Value == name);
      throw new NotImplementedException();
    }

    public IName Address {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName AllowMultiple {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName BeginInvoke {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName BoolOpBool {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Combine {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Concat {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName DecimalOpDecimal {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName DelegateOpDelegate {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName EndInvoke {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName EnumOpEnum {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName EnumOpNum {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public new IName Equals {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Float32OpFloat32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Float64OpFloat64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Get {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName global {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName HasValue {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Inherited {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Invoke {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int16OpInt16 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int32OpInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int32OpUInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int64OpInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int64OpUInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int64OpUInt64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int64OpInt64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int8OpInt8 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName NullCoalescing {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName NumOpEnum {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName ObjectOpObject {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName ObjectOpString {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpAddition {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpBitwiseAnd {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpBitwiseOr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpEnum {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpEquality {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpExplicit {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpImplicit {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpInequality {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpInt8 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpInt16 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpInt64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpComma {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpConcatentation {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpDivision {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpExclusiveOr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpExponentiation {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpFalse {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpFloat32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpFloat64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpGreaterThan {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpGreaterThanOrEqual {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpIntegerDivision {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpLeftShift {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpLessThan {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpLessThanOrEqual {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpLike {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpLogicalNot {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpLogicalOr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpModulus {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpMultiply {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpOnesComplement {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpBoolean {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpChar {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpDecimal {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpDecrement {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpIncrement {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpRightShift {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpSubtraction {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpTrue {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpUInt8 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpUInt16 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpUInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpUInt64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpUnaryNegation {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName OpUnaryPlus {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName StringOpString {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName StringOpObject {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UIntPtrOpUIntPtr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt32OpInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt32OpUInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt64OpInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt64OpUInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt64OpUInt64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName value {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName System {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Void {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName VoidPtrOpVoidPtr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Boolean {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Cctor {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Char {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Ctor {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Byte {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName SByte {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int16 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt16 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt32 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Int64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UInt64 {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName String {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName IntPtr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName UIntPtr {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Object {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Remove {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Result {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Set {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Single {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Double {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName TypedReference {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Enum {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Length {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName LongLength {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName MulticastDelegate {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName ValueType {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Type {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Array {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Attribute {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName AttributeUsageAttribute {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName DateTime {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName DebuggerHiddenAttribute {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Decimal {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Delegate {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Diagnostics {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName DBNull {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }

    public IName Nullable {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException();
      }
    }
  }

  /// <summary>
  /// An enumeration of all of the operation codes that are used in the CLI Common Intermediate Language.
  /// </summary>
  public enum OperationCode {
    /// <summary>
    /// 
    /// </summary>
    Nop=0x00,
    /// <summary>
    /// 
    /// </summary>
    Break=0x01,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldarg_0=0x02,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldarg_1=0x03,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldarg_2=0x04,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldarg_3=0x05,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldloc_0=0x06,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldloc_1=0x07,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldloc_2=0x08,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldloc_3=0x09,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stloc_0=0x0a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stloc_1=0x0b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stloc_2=0x0c,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stloc_3=0x0d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldarg_S=0x0e,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldarga_S=0x0f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Starg_S=0x10,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldloc_S=0x11,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldloca_S=0x12,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stloc_S=0x13,
    /// <summary>
    /// 
    /// </summary>
    Ldnull=0x14,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_M1=0x15,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_0=0x16,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_1=0x17,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_2=0x18,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_3=0x19,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_4=0x1a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_5=0x1b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_6=0x1c,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_7=0x1d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_8=0x1e,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4_S=0x1f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I4=0x20,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_I8=0x21,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_R4=0x22,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldc_R8=0x23,
    /// <summary>
    /// 
    /// </summary>
    Dup=0x25,
    /// <summary>
    /// 
    /// </summary>
    Pop=0x26,
    /// <summary>
    /// 
    /// </summary>
    Jmp=0x27,
    /// <summary>
    /// 
    /// </summary>
    Call=0x28,
    /// <summary>
    /// 
    /// </summary>
    Calli=0x29,
    /// <summary>
    /// 
    /// </summary>
    Ret=0x2a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Br_S=0x2b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Brfalse_S=0x2c,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Brtrue_S=0x2d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Beq_S=0x2e,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bge_S=0x2f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bgt_S=0x30,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ble_S=0x31,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Blt_S=0x32,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bne_Un_S=0x33,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bge_Un_S=0x34,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bgt_Un_S=0x35,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ble_Un_S=0x36,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Blt_Un_S=0x37,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    Br=0x38,
    /// <summary>
    /// 
    /// </summary>
    Brfalse=0x39,
    /// <summary>
    /// 
    /// </summary>
    Brtrue=0x3a,
    /// <summary>
    /// 
    /// </summary>
    Beq=0x3b,
    /// <summary>
    /// 
    /// </summary>
    Bge=0x3c,
    /// <summary>
    /// 
    /// </summary>
    Bgt=0x3d,
    /// <summary>
    /// 
    /// </summary>
    Ble=0x3e,
    /// <summary>
    /// 
    /// </summary>
    Blt=0x3f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bne_Un=0x40,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bge_Un=0x41,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Bgt_Un=0x42,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ble_Un=0x43,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Blt_Un=0x44,
    /// <summary>
    /// 
    /// </summary>
    Switch=0x45,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_I1=0x46,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_U1=0x47,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_I2=0x48,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_U2=0x49,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_I4=0x4a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_U4=0x4b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_I8=0x4c,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_I=0x4d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_R4=0x4e,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_R8=0x4f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldind_Ref=0x50,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_Ref=0x51,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_I1=0x52,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_I2=0x53,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_I4=0x54,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_I8=0x55,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_R4=0x56,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_R8=0x57,
    /// <summary>
    /// 
    /// </summary>
    Add=0x58,
    /// <summary>
    /// 
    /// </summary>
    Sub=0x59,
    /// <summary>
    /// 
    /// </summary>
    Mul=0x5a,
    /// <summary>
    /// 
    /// </summary>
    Div=0x5b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Div_Un=0x5c,
    /// <summary>
    /// 
    /// </summary>
    Rem=0x5d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Rem_Un=0x5e,
    /// <summary>
    /// 
    /// </summary>
    And=0x5f,
    /// <summary>
    /// 
    /// </summary>
    Or=0x60,
    /// <summary>
    /// 
    /// </summary>
    Xor=0x61,
    /// <summary>
    /// 
    /// </summary>
    Shl=0x62,
    /// <summary>
    /// 
    /// </summary>
    Shr=0x63,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Shr_Un=0x64,
    /// <summary>
    /// 
    /// </summary>
    Neg=0x65,
    /// <summary>
    /// 
    /// </summary>
    Not=0x66,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_I1=0x67,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_I2=0x68,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_I4=0x69,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_I8=0x6a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_R4=0x6b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_R8=0x6c,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_U4=0x6d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_U8=0x6e,
    /// <summary>
    /// 
    /// </summary>
    Callvirt=0x6f,
    /// <summary>
    /// 
    /// </summary>
    Cpobj=0x70,
    /// <summary>
    /// 
    /// </summary>
    Ldobj=0x71,
    /// <summary>
    /// 
    /// </summary>
    Ldstr=0x72,
    /// <summary>
    /// 
    /// </summary>
    Newobj=0x73,
    /// <summary>
    /// 
    /// </summary>
    Castclass=0x74,
    /// <summary>
    /// 
    /// </summary>
    Isinst=0x75,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_R_Un=0x76,
    /// <summary>
    /// 
    /// </summary>
    Unbox=0x79,
    /// <summary>
    /// 
    /// </summary>
    Throw=0x7a,
    /// <summary>
    /// 
    /// </summary>
    Ldfld=0x7b,
    /// <summary>
    /// 
    /// </summary>
    Ldflda=0x7c,
    /// <summary>
    /// 
    /// </summary>
    Stfld=0x7d,
    /// <summary>
    /// 
    /// </summary>
    Ldsfld=0x7e,
    /// <summary>
    /// 
    /// </summary>
    Ldsflda=0x7f,
    /// <summary>
    /// 
    /// </summary>
    Stsfld=0x80,
    /// <summary>
    /// 
    /// </summary>
    Stobj=0x81,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I1_Un=0x82,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I2_Un=0x83,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I4_Un=0x84,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I8_Un=0x85,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U1_Un=0x86,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U2_Un=0x87,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U4_Un=0x88,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U8_Un=0x89,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I_Un=0x8a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U_Un=0x8b,
    /// <summary>
    /// 
    /// </summary>
    Box=0x8c,
    /// <summary>
    /// 
    /// </summary>
    Newarr=0x8d,
    /// <summary>
    /// 
    /// </summary>
    Ldlen=0x8e,
    /// <summary>
    /// 
    /// </summary>
    Ldelema=0x8f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_I1=0x90,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_U1=0x91,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_I2=0x92,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_U2=0x93,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_I4=0x94,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_U4=0x95,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_I8=0x96,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_I=0x97,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_R4=0x98,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_R8=0x99,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Ldelem_Ref=0x9a,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_I=0x9b,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_I1=0x9c,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_I2=0x9d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_I4=0x9e,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_I8=0x9f,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_R4=0xa0,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_R8=0xa1,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stelem_Ref=0xa2,
    /// <summary>
    /// 
    /// </summary>
    Ldelem=0xa3,
    /// <summary>
    /// 
    /// </summary>
    Stelem=0xa4,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Unbox_Any=0xa5,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I1=0xb3,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U1=0xb4,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I2=0xb5,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U2=0xb6,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I4=0xb7,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U4=0xb8,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I8=0xb9,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U8=0xba,
    /// <summary>
    /// 
    /// </summary>
    Refanyval=0xc2,
    /// <summary>
    /// 
    /// </summary>
    Ckfinite=0xc3,
    /// <summary>
    /// 
    /// </summary>
    Mkrefany=0xc6,
    /// <summary>
    /// 
    /// </summary>
    Ldtoken=0xd0,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_U2=0xd1,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_U1=0xd2,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_I=0xd3,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_I=0xd4,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_Ovf_U=0xd5,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Add_Ovf=0xd6,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Add_Ovf_Un=0xd7,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Mul_Ovf=0xd8,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Mul_Ovf_Un=0xd9,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Sub_Ovf=0xda,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Sub_Ovf_Un=0xdb,
    /// <summary>
    /// 
    /// </summary>
    Endfinally=0xdc,
    /// <summary>
    /// 
    /// </summary>
    Leave=0xdd,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Leave_S=0xde,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Stind_I=0xdf,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Conv_U=0xe0,
    /// <summary>
    /// 
    /// </summary>
    Arglist=0xfe00,
    /// <summary>
    /// 
    /// </summary>
    Ceq=0xfe01,
    /// <summary>
    /// 
    /// </summary>
    Cgt=0xfe02,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Cgt_Un=0xfe03,
    /// <summary>
    /// 
    /// </summary>
    Clt=0xfe04,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Clt_Un=0xfe05,
    /// <summary>
    /// 
    /// </summary>
    Ldftn=0xfe06,
    /// <summary>
    /// 
    /// </summary>
    Ldvirtftn=0xfe07,
    /// <summary>
    /// 
    /// </summary>
    Ldarg=0xfe09,
    /// <summary>
    /// 
    /// </summary>
    Ldarga=0xfe0a,
    /// <summary>
    /// 
    /// </summary>
    Starg=0xfe0b,
    /// <summary>
    /// 
    /// </summary>
    Ldloc=0xfe0c,
    /// <summary>
    /// 
    /// </summary>
    Ldloca=0xfe0d,
    /// <summary>
    /// 
    /// </summary>
    Stloc=0xfe0e,
    /// <summary>
    /// 
    /// </summary>
    Localloc=0xfe0f,
    /// <summary>
    /// 
    /// </summary>
    Endfilter=0xfe11,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Unaligned_=0xfe12,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Volatile_=0xfe13,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Tail_=0xfe14,
    /// <summary>
    /// 
    /// </summary>
    Initobj=0xfe15,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Constrained_=0xfe16,
    /// <summary>
    /// 
    /// </summary>
    Cpblk=0xfe17,
    /// <summary>
    /// 
    /// </summary>
    Initblk=0xfe18,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    No_=0xfe19,
    /// <summary>
    /// 
    /// </summary>
    Rethrow=0xfe1a,
    /// <summary>
    /// 
    /// </summary>
    Sizeof=0xfe1c,
    /// <summary>
    /// 
    /// </summary>
    Refanytype=0xfe1d,
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Readonly_=0xfe1e,

    /// <summary>
    /// Instruction to create multidimensional array. The value will be Type reference to the matrix type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Array_Create,
    /// <summary>
    /// Instruction to create multidimensional array. The value will be Type reference to the matrix type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Array_Create_WithLowerBound,
    /// <summary>
    /// Instruction to read from element of a multidimensional array. The value will be Type reference to the matrix type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Array_Get,
    /// <summary>
    /// Instruction to store into element of a multidimensional array. The value will be Type reference to the matrix type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Array_Set,
    /// <summary>
    /// Instruction to get the address of element of a multidimensional array. The value will be Type reference to the matrix type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId="Member")]
    Array_Addr,
    /// <summary>
    /// Not a valid operation.
    /// </summary>
    Invalid,
  }

  /// <summary>
  /// Implemented by an entity that is always a member of a particular parameter list, such as an IParameterDefinition.
  /// Provides a way to determine the position where the entity appears in the parameter list.
  /// </summary>
  public interface IParameterListEntry {
    /// <summary>
    /// The position in the parameter list where this instance can be found.
    /// </summary>
    ushort Index { get; }
  }

  /// <summary>
  /// Calling convention for the PInvoke
  /// </summary>
  public enum PInvokeCallingConvention {
    /// <summary>
    /// Windows API call.
    /// Also platformapi in ilasm.
    /// </summary>
    WinApi,
    /// <summary>
    /// Standard C style call.
    /// </summary>
    CDecl,
    /// <summary>
    /// Standard C++ style call
    /// </summary>
    StdCall,
    /// <summary>
    /// method accepts implicit this pointer.
    /// </summary>
    ThisCall,
    /// <summary>
    /// C style fast call.
    /// </summary>
    FastCall,
  }

  /// <summary>
  /// Information that describes how a method from the underlying Platform is to be invoked.
  /// </summary>
  [ContractClass(typeof(IPlatformInvokeInformationContract))]
  public interface IPlatformInvokeInformation {
    /// <summary>
    /// Name of the method/field providing the implementation.
    /// </summary>
    IName ImportName { get; }

    /// <summary>
    /// Module providing the method/field.
    /// </summary>
    IModuleReference ImportModule { get; }

    /// <summary>
    /// Marshalling of the Strings for this method.
    /// </summary>
    StringFormatKind StringFormat { get; }

    /// <summary>
    /// If the PInvoke should use the name specified as is.
    /// </summary>
    bool NoMangle { get; }

    /// <summary>
    /// The calling convention of the PInvoke call.
    /// </summary>
    PInvokeCallingConvention PInvokeCallingConvention { get; }

    /// <summary>
    /// If the target function supports getting last error.
    /// </summary>
    bool SupportsLastError { get; }

    /// <summary>
    /// Enables or disables best-fit mapping behavior when converting Unicode characters to ANSI characters.
    /// </summary>
    bool? UseBestFit { get; }

    /// <summary>
    /// Enables or disables the throwing of an exception on an unmappable Unicode character that is converted to an ANSI "?" character.
    /// </summary>
    bool? ThrowExceptionForUnmappableChar { get; }

  }

  #region IPlatformInvokeInformation contract binding
  [ContractClassFor(typeof(IPlatformInvokeInformation))]
  abstract class IPlatformInvokeInformationContract : IPlatformInvokeInformation {
    #region IPlatformInvokeInformation Members

    public IName ImportName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException(); 
      }
    }

    public IModuleReference ImportModule {
      get {
        Contract.Ensures(Contract.Result<IModuleReference>() != null);
        throw new NotImplementedException();
      }
    }

    public StringFormatKind StringFormat {
      get { throw new NotImplementedException(); }
    }

    public bool NoMangle {
      get { throw new NotImplementedException(); }
    }

    public PInvokeCallingConvention PInvokeCallingConvention {
      get { throw new NotImplementedException(); }
    }

    public bool SupportsLastError {
      get { throw new NotImplementedException(); }
    }

    public bool? UseBestFit {
      get { throw new NotImplementedException(); }
    }

    public bool? ThrowExceptionForUnmappableChar {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A resource file formatted according to Win32 API conventions and typically obtained from a Portable Executable (PE) file.
  /// </summary>
  public interface IWin32Resource {

    /// <summary>
    /// A string that identifies what type of resource this is. Only valid if this.TypeId &lt; 0.
    /// </summary>
    string TypeName {
      get;
      //^ requires this.TypeId < 0;
    }

    /// <summary>
    /// An integer tag that identifies what type of resource this is. If the value is less than 0, this.TypeName should be used instead.
    /// </summary>
    int TypeId {
      get;
    }

    /// <summary>
    /// The name of the resource. Only valid if this.Id &lt; 0.
    /// </summary>
    string Name {
      get;
      //^ requires this.Id < 0; 
    }

    /// <summary>
    /// An integer tag that identifies this resource. If the value is less than 0, this.Name should be used instead.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// The language for which this resource is appropriate.
    /// </summary>
    uint LanguageId { get; }

    /// <summary>
    /// The code page for which this resource is appropriate.
    /// </summary>
    uint CodePage { get; }

    /// <summary>
    /// The data of the resource.
    /// </summary>
    IEnumerable<byte> Data { get; }

  }

  /// <summary>
  /// Flags for IL No Operation
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32"), Flags]
  public enum OperationCheckFlags : byte {
    /// <summary>
    /// No type check needs to be performed for next operation
    /// </summary>
    NoTypeCheck=0x01,
    /// <summary>
    /// No range check needs to be performed for next operation
    /// </summary>
    NoRangeCheck=0x02,
    /// <summary>
    /// No null check needs to be performed for next operation
    /// </summary>
    NoNullCheck=0x04,
  }

  /// <summary>
  /// A section of the PE file that does not contain any objects that can be referenced in another way
  /// using this object model. The data of such a section is not decompiled by metadata reader and 
  /// is written out unchanged by metadata writers. Presumably it is meaningful in some way and is 
  /// thus exposed for use by other tools.
  /// </summary>
  [ContractClass(typeof(IPESectionContract))]
  public interface IPESection {

    /// <summary>
    /// The name of the section. Should be exactly eight characters long.
    /// </summary>
    IName SectionName { get; }

    /// <summary>
    /// A set of bits that describes the purpose of a section and how it behaves when loaded.
    /// </summary>
    PESectionCharacteristics Characteristics { get; }

    /// <summary>
    /// For executable images this is the address of the first byte of the section, when loaded into memory, relative to the image base. 
    /// </summary>
    int VirtualAddress { get; }

    /// <summary>
    /// Total size of the section in bytes. If this value is greater than SizeOfRawData, the section is zero-padded.
    /// </summary>
    int VirtualSize { get; }

    /// <summary>
    /// Size of the initialized data on disk in bytes, shall be a multiple of FileAlignment from the PE header.
    /// If this is less than VirtualSize the remainder of the section is zero filled.
    /// Because this field is rounded while the VirtualSize field is not it is possible for this to be greater than VirtualSize as well. 
    /// When a section contains only uninitialized data, this field should be 0.
    /// </summary>
    int SizeOfRawData { get; }

    /// <summary>
    /// The data, if any, with which the section will be initialized when loaded.
    /// </summary>
    IEnumerable<byte> Rawdata { get; }


  }

  #region IPESection contract binding
  [ContractClassFor(typeof(IPESection))]
  abstract class IPESectionContract : IPESection {
    #region IPESection Members

    public IName SectionName {
      get {
        Contract.Ensures(Contract.Result<IName>() != null);
        throw new NotImplementedException(); 
      }
    }

    public PESectionCharacteristics Characteristics {
      get { throw new NotImplementedException(); }
    }

    public int VirtualAddress {
      get { throw new NotImplementedException(); }
    }

    public int VirtualSize {
      get { throw new NotImplementedException(); }
    }

    public int SizeOfRawData {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<byte> Rawdata {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<byte>>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A set of bits that describes the purpose of a section and how it behaves when loaded.
  /// </summary>
  [Flags]
  public enum PESectionCharacteristics {
    /// <summary>
    /// Indicates this section contains machine instructions.
    /// </summary>
    ContainsExecutableCode = 0x00000020,
    /// <summary>
    /// Indicates this section will be initialized from the contents of the PE file when loaded.
    /// </summary>
    ContainsInitializedData = 0x00000040,
    /// <summary>
    /// Indicates this section simply reserves an uninitialized address space in the loaded image of the PE file.
    /// </summary>
    ContainsUninitializedData = 0x00000080,
    /// <summary>
    /// Indicates the part of the loaded PE file image that corresponds to this section will marked as being executable.
    /// </summary>
    IsExecutable = 0x20000000,
    /// <summary>
    /// Indicates the part of the loaded PE file image that corresponds to this section will marked as being readable.
    /// </summary>
    IsReadable = 0x40000000,
    /// <summary>
    /// Indicates the part of the loaded PE file image that corresponds to this section will marked as being writable.
    /// </summary>
    IsWritable = unchecked((int)0x80000000),
  }

  /// <summary>
  /// An enumeration indicating the section inside the PE File
  /// </summary>
  public enum PESectionKind {
    /// <summary>
    /// Section is unrecognized
    /// </summary>
    Illegal,
    /// <summary>
    /// Section for initialized constant data.
    /// </summary>
    ConstantData,
    /// <summary>
    /// Section for code coverage data.
    /// </summary>
    CoverageData,
    /// <summary>
    /// Section for initialized writable data, called .datax rather than .sdata for some or other reason.
    /// </summary>
    ExtendedData,
    /// <summary>
    /// Section for intialized writable data.
    /// </summary>
    StaticData,
    /// <summary>
    /// Section for IL and Metadata.
    /// </summary>
    Text,
    /// <summary>
    /// Section for initialized thread local storage.
    /// </summary>
    ThreadLocalStorage,
  }

  /// <summary>
  /// Represents a block of data stored at a given offset within a specified section of the PE file.
  /// </summary>
  public interface ISectionBlock {
    /// <summary>
    /// Section where the block resides.
    /// </summary>
    PESectionKind PESectionKind { get; }

    /// <summary>
    /// Offset into section where the block resides.
    /// </summary>
    uint Offset { get; }

    /// <summary>
    /// Size of the block.
    /// </summary>
    uint Size { get; }

    /// <summary>
    /// Byte information stored in the block.
    /// </summary>
    IEnumerable<byte> Data { get; }
  }
}
