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
using System.Diagnostics.Contracts;

namespace Microsoft.Cci.UtilityDataStructures {

#pragma warning disable 1591
  public static class HashHelper {
    public static uint HashInt1(uint key) {
      unchecked {
        uint a = 0x9e3779b9 + key;
        uint b = 0x9e3779b9;
        uint c = 16777619;
        a -= b; a -= c; a ^= (c >> 13);
        b -= c; b -= a; b ^= (a << 8);
        c -= a; c -= b; c ^= (b >> 13);
        a -= b; a -= c; a ^= (c >> 12);
        b -= c; b -= a; b ^= (a << 16);
        c -= a; c -= b; c ^= (b >> 5);
        a -= b; a -= c; a ^= (c >> 3);
        b -= c; b -= a; b ^= (a << 10);
        c -= a; c -= b; c ^= (b >> 15);
        return c;
      }
    }
    public static uint HashInt2(uint key) {
      unchecked {
        uint hash = 0xB1635D64 + key;
        hash += (hash << 3);
        hash ^= (hash >> 11);
        hash += (hash << 15);
        hash |= 0x00000001; //  To make sure that this is relatively prime with power of 2
        return hash;
      }
    }
    public static uint HashDoubleInt1(
      uint key1,
      uint key2
    ) {
      unchecked {
        uint a = 0x9e3779b9 + key1;
        uint b = 0x9e3779b9 + key2;
        uint c = 16777619;
        a -= b; a -= c; a ^= (c >> 13);
        b -= c; b -= a; b ^= (a << 8);
        c -= a; c -= b; c ^= (b >> 13);
        a -= b; a -= c; a ^= (c >> 12);
        b -= c; b -= a; b ^= (a << 16);
        c -= a; c -= b; c ^= (b >> 5);
        a -= b; a -= c; a ^= (c >> 3);
        b -= c; b -= a; b ^= (a << 10);
        c -= a; c -= b; c ^= (b >> 15);
        return c;
      }
    }
    public static uint HashDoubleInt2(
      uint key1,
      uint key2
    ) {
      unchecked {
        uint hash = 0xB1635D64 + key1;
        hash += (hash << 10);
        hash ^= (hash >> 6);
        hash += key2;
        hash += (hash << 3);
        hash ^= (hash >> 11);
        hash += (hash << 15);
        hash |= 0x00000001; //  To make sure that this is relatively prime with power of 2
        return hash;
      }
    }
    public static uint StartHash(uint key) {
      uint hash = 0xB1635D64 + key;
      hash += (hash << 3);
      hash ^= (hash >> 11);
      hash += (hash << 15);
      return hash;
    }
    public static uint ContinueHash(uint prevHash, uint key) {
      unchecked {
        uint hash = prevHash + key;
        hash += (hash << 10);
        hash ^= (hash >> 6);
        return hash;
      }
    }
#pragma warning restore 1591
  }

  /// <summary>
  /// Hashtable that can host multiple values for the same uint key.
  /// </summary>
  /// <typeparam name="InternalT"></typeparam>
  public sealed class MultiHashtable<InternalT> where InternalT : class {
    struct KeyValuePair {
      internal uint Key;
      internal InternalT Value;
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of 2
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for MultiHashtable
    /// </summary>
    public MultiHashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for MultiHashtable
    /// </summary>
    public MultiHashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Count of elements in MultiHashtable
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        uint key = oldKeyValueTable[i].Key;
        InternalT value = oldKeyValueTable[i].Value;
        if (value != null)
          this.AddInternal(key, value);
      }
    }

    void AddInternal(uint key, InternalT value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == value)
            return;
          tableIndex = (tableIndex + hash2) & mask;
        }
        keyValueTable[tableIndex].Key = key;
        keyValueTable[tableIndex].Value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Add element to MultiHashtable
    /// </summary>
    public void Add(uint key, InternalT value) {
      Contract.Requires(value != null);

      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key, value);
    }

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      if (this.count == 0) return;
      var table = this.keyValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i) {
        table[i].Key = 0;
        table[i].Value = null;
      }
      this.count = 0;
    }

    /// <summary>
    /// Checks if key and value is present in the MultiHashtable
    /// </summary>
    public bool Contains(uint key, InternalT value) {
      Contract.Requires(value != null);

      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == value)
            return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// Checks if key is present in the MultiHashtable
    /// </summary>
    public bool ContainsKey(uint key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key)
            return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// Returns the number of entries that are associated with the key
    /// </summary>
    public int NumberOfEntries(uint key) {
      unchecked {
        int count = 0;
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key)
            count++;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return count;
      }
    }

    /// <summary>
    /// Updates the hashtable so that newValue shows up in the place of oldValue.
    /// </summary>
    public void ReplaceEntry(uint key, InternalT oldValue, InternalT newValue) {
      Contract.Requires(oldValue != null);
      Contract.Requires(newValue != null);

      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == oldValue) {
            keyValueTable[tableIndex].Value = newValue;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
      }
    }

    /// <summary>
    /// Enumerator to enumerate values with given key.
    /// </summary>
    public struct KeyedValuesEnumerator {

      internal KeyedValuesEnumerator(MultiHashtable<InternalT> multiHashtable, uint key) {
        this.MultiHashtable = multiHashtable;
        this.Key = key;
        this.Hash1 = HashHelper.HashInt1(key);
        this.Hash2 = HashHelper.HashInt2(key);
        this.CurrentIndex = 0xFFFFFFFF;
      }

      MultiHashtable<InternalT> MultiHashtable;
      uint Key;
      uint Hash1;
      uint Hash2;
      uint CurrentIndex;

      /// <summary>
      /// Get the current element.
      /// </summary>
      /// <returns></returns>
      public InternalT Current {
        [ContractVerification(false)]
        get {
          Contract.Ensures(Contract.Result<InternalT>() != null);
          return this.MultiHashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext() {
        unchecked {
          uint size = this.MultiHashtable.size;
          uint mask = size - 1;
          uint key = this.Key;
          uint hash1 = this.Hash1;
          uint hash2 = this.Hash2;
          KeyValuePair[] keyValueTable = this.MultiHashtable.keyValueTable;
          uint currentIndex = this.CurrentIndex;
          if (currentIndex == 0xFFFFFFFF)
            currentIndex = hash1 & mask;
          else
            currentIndex = (currentIndex + hash2) & mask;
          while (keyValueTable[currentIndex].Value != null) {
            if (keyValueTable[currentIndex].Key == key)
              break;
            currentIndex = (currentIndex + hash2) & mask;
          }
          this.CurrentIndex = currentIndex;
          return keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumeration.
      /// </summary>
      /// <returns></returns>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable to enumerate values with given key.
    /// </summary>
    public struct KeyedValuesEnumerable {
      MultiHashtable<InternalT> MultiHashtable;
      uint Key;

      internal KeyedValuesEnumerable(MultiHashtable<InternalT> multiHashtable, uint key) {
        this.MultiHashtable = multiHashtable;
        this.Key = key;
      }

      /// <summary>
      /// Return the enumerator.
      /// </summary>
      /// <returns></returns>
      public KeyedValuesEnumerator GetEnumerator() {
        return new KeyedValuesEnumerator(this.MultiHashtable, this.Key);
      }
    }

    /// <summary>
    /// Enumeration to return all the values associated with the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public KeyedValuesEnumerable GetValuesFor(uint key) {
      return new KeyedValuesEnumerable(this, key);
    }

    /// <summary>
    /// Enumerator to enumerate all values.
    /// </summary>
    public struct ValuesEnumerator {
      MultiHashtable<InternalT> MultiHashtable;
      uint CurrentIndex;

      internal ValuesEnumerator(MultiHashtable<InternalT> multiHashtable) {
        this.MultiHashtable = multiHashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Get the current element.
      /// </summary>
      /// <returns></returns>
      public InternalT Current {
        [ContractVerification(false)]
        get {
          Contract.Ensures(Contract.Result<InternalT>() != null);
          return this.MultiHashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext() {
        unchecked {
          uint size = this.MultiHashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          KeyValuePair[] keyValueTable = this.MultiHashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].Value == null) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumeration.
      /// </summary>
      /// <returns></returns>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable to enumerate all values.
    /// </summary>
    public struct ValuesEnumerable {
      MultiHashtable<InternalT> MultiHashtable;

      internal ValuesEnumerable(
        MultiHashtable<InternalT> multiHashtable
      ) {
        this.MultiHashtable = multiHashtable;
      }

      /// <summary>
      /// Return the enumerator.
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.MultiHashtable);
      }
    }

    /// <summary>
    /// Enumeration of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }

  }

  /// <summary>
  /// Hashtable that can host multiple values for the same uint key.
  /// </summary>
  /// <typeparam name="InternalT"></typeparam>
  /// <typeparam name="Key"></typeparam>
  public sealed class MultiHashtable<Key, InternalT> 
    where Key : class 
    where InternalT : class {

    struct KeyValuePair {
      internal Key Key;
      internal InternalT Value;
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of 2
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for MultiHashtable
    /// </summary>
    public MultiHashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for MultiHashtable
    /// </summary>
    public MultiHashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Count of elements in MultiHashtable
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        var key = oldKeyValueTable[i].Key;
        InternalT value = oldKeyValueTable[i].Value;
        if (value != null)
          this.AddInternal(key, value);
      }
    }

    void AddInternal(Key key, InternalT value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint hash2 = HashHelper.HashInt2(hash);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == value)
            return;
          tableIndex = (tableIndex + hash2) & mask;
        }
        keyValueTable[tableIndex].Key = key;
        keyValueTable[tableIndex].Value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Add element to MultiHashtable
    /// </summary>
    public void Add(Key key, InternalT value) {
      Contract.Requires(key != null);
      Contract.Requires(value != null);

      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key, value);
    }

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      if (this.count == 0) return;
      var table = this.keyValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i) {
        table[i].Key = null;
        table[i].Value = null;
      }
      this.count = 0;
    }

    /// <summary>
    /// Checks if key and value is present in the MultiHashtable
    /// </summary>
    public bool Contains(Key key, InternalT value) {
      Contract.Requires(key != null);
      Contract.Requires(value != null);

      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint hash2 = HashHelper.HashInt2(hash);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == value)
            return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// Checks if key is present in the MultiHashtable
    /// </summary>
    public bool ContainsKey(Key key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint hash2 = HashHelper.HashInt2(hash);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key)
            return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// Returns the number of entries that are associated with the key
    /// </summary>
    public int NumberOfEntries(Key key) {
      unchecked {
        int count = 0;
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint hash2 = HashHelper.HashInt2(hash);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key)
            count++;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return count;
      }
    }

    /// <summary>
    /// Updates the hashtable so that newValue shows up in the place of oldValue.
    /// </summary>
    public void ReplaceEntry(Key key, InternalT oldValue, InternalT newValue) {
      Contract.Requires(oldValue != null);
      Contract.Requires(newValue != null);

      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint hash2 = HashHelper.HashInt2(hash);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == oldValue) {
            keyValueTable[tableIndex].Value = newValue;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
      }
    }

    /// <summary>
    /// Enumerator to enumerate values with given key.
    /// </summary>
    public struct KeyedValuesEnumerator {
      MultiHashtable<Key, InternalT> MultiHashtable;
      Key Key;
      uint Hash1;
      uint Hash2;
      uint CurrentIndex;
      internal KeyedValuesEnumerator(
        MultiHashtable<Key, InternalT> multiHashtable,
        Key key
      ) {
        this.MultiHashtable = multiHashtable;
        this.Key = key;
        var hash = (uint)key.GetHashCode();
        this.Hash1 = HashHelper.HashInt1(hash);
        this.Hash2 = HashHelper.HashInt2(hash);
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Get the current element.
      /// </summary>
      /// <returns></returns>
      public InternalT Current {
        get {
          return this.MultiHashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext() {
        unchecked {
          uint size = this.MultiHashtable.size;
          uint mask = size - 1;
          Key key = this.Key;
          uint hash1 = this.Hash1;
          uint hash2 = this.Hash2;
          KeyValuePair[] keyValueTable = this.MultiHashtable.keyValueTable;
          uint currentIndex = this.CurrentIndex;
          if (currentIndex == 0xFFFFFFFF)
            currentIndex = hash1 & mask;
          else
            currentIndex = (currentIndex + hash2) & mask;
          while (keyValueTable[currentIndex].Value != null) {
            if (keyValueTable[currentIndex].Key == key)
              break;
            currentIndex = (currentIndex + hash2) & mask;
          }
          this.CurrentIndex = currentIndex;
          return keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumeration.
      /// </summary>
      /// <returns></returns>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable to enumerate values with given key.
    /// </summary>
    public struct KeyedValuesEnumerable {
      MultiHashtable<Key, InternalT> MultiHashtable;
      Key Key;

      internal KeyedValuesEnumerable(
        MultiHashtable<Key, InternalT> multiHashtable,
        Key key
      ) {
        this.MultiHashtable = multiHashtable;
        this.Key = key;
      }

      /// <summary>
      /// Return the enumerator.
      /// </summary>
      /// <returns></returns>
      public KeyedValuesEnumerator GetEnumerator() {
        return new KeyedValuesEnumerator(this.MultiHashtable, this.Key);
      }
    }

    /// <summary>
    /// Enumeration to return all the values associated with the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public KeyedValuesEnumerable GetValuesFor(Key key) {
      return new KeyedValuesEnumerable(this, key);
    }

    /// <summary>
    /// Enumerator to enumerate all values.
    /// </summary>
    public struct ValuesEnumerator {
      MultiHashtable<Key, InternalT> MultiHashtable;
      uint CurrentIndex;

      internal ValuesEnumerator(
        MultiHashtable<Key, InternalT> multiHashtable
      ) {
        this.MultiHashtable = multiHashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Get the current element.
      /// </summary>
      /// <returns></returns>
      public InternalT Current {
        get {
          return this.MultiHashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext() {
        unchecked {
          uint size = this.MultiHashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          KeyValuePair[] keyValueTable = this.MultiHashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].Value == null) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumeration.
      /// </summary>
      /// <returns></returns>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable to enumerate all values.
    /// </summary>
    public struct ValuesEnumerable {
      MultiHashtable<Key, InternalT> MultiHashtable;

      internal ValuesEnumerable(
        MultiHashtable<Key, InternalT> multiHashtable
      ) {
        this.MultiHashtable = multiHashtable;
      }

      /// <summary>
      /// Return the enumerator.
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.MultiHashtable);
      }
    }

    /// <summary>
    /// Enumeration of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }

  }

  /// <summary>
  /// Hashtable that can hold only single value per key.
  /// </summary>
  public sealed class Hashtable<Key, Value>
    where Key : class
    where Value : class, new() {
    static Value dummyObject = new Value();
    /// <summary>
    /// 
    /// </summary>
    public struct KeyValuePair {
      /// <summary>
      /// 
      /// </summary>
      public Key key;
      /// <summary>
      /// 
      /// </summary>
      public Value value;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() {
        return "Key = "+this.key+", value = "+this.value;
      }
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of two
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public Hashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public Hashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Constructs a hashtable that is a copy of the given hashtable.
    /// </summary>
    /// <param name="tableToClone"></param>
    public Hashtable(Hashtable<Key, Value> tableToClone) {
      Contract.Requires(tableToClone != null);

      this.size = tableToClone.size;
      this.resizeCount = tableToClone.resizeCount;
      this.count = tableToClone.count;
      var keyValueTable = this.keyValueTable = new KeyValuePair[tableToClone.keyValueTable.Length];
      for (int i = 0, n = keyValueTable.Length; i < n; i++)
        keyValueTable[i] = tableToClone.keyValueTable[i];
    }

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      var table = this.keyValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i) {
        table[i].key = null;
        table[i].value = null;
      }
      this.count = 0;
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        var key = oldKeyValueTable[i].key;
        var value = oldKeyValueTable[i].value;
        if (value != null && value != dummyObject)
          this.AddInternal(key, value);
      }
    }

    void AddInternal(Key key, Value value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        if (keyValueTable[tableIndex].value != null) {
          if (object.ReferenceEquals(keyValueTable[tableIndex].key, key)) {
            keyValueTable[tableIndex].value = value;
            return;
          }
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while (keyValueTable[tableIndex].value != null) {
            if (object.ReferenceEquals(keyValueTable[tableIndex].key, key)) {
              keyValueTable[tableIndex].value = value;
              return;
            }
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
        keyValueTable[tableIndex].key = key;
        keyValueTable[tableIndex].value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Add element to the Hashtable
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(Key key, Value value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(Key key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        if (object.ReferenceEquals(keyValueTable[tableIndex].key, key) && keyValueTable[tableIndex].value != dummyObject)
          return true;
        uint hash2 = HashHelper.HashInt2(hash);
        tableIndex = (tableIndex + hash2) & mask;
        while (object.ReferenceEquals(keyValueTable[tableIndex].key, key)) {
          if (keyValueTable[tableIndex].value != dummyObject)
            return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    public void Remove(Key key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint hash2 = HashHelper.HashInt2(hash);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].value != null) {
          if (object.ReferenceEquals(keyValueTable[tableIndex].key, key)) {
            keyValueTable[tableIndex].value = dummyObject;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Value this[Key key] {
      get {
        Contract.Requires(key != null);
        unchecked {
          uint mask = this.size - 1;
          var keyValueTable = this.keyValueTable;
          var hash = (uint)key.GetHashCode();
          uint hash1 = HashHelper.HashInt1(hash);
          uint tableIndex = hash1 & mask;
          Key tableKey = keyValueTable[tableIndex].key;
          if (tableKey == null) return default(Value);
          if (object.ReferenceEquals(tableKey, key)) return keyValueTable[tableIndex].value;
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((tableKey = keyValueTable[tableIndex].key) != null) {
            if (object.ReferenceEquals(tableKey, key)) return keyValueTable[tableIndex].value;
            tableIndex = (tableIndex + hash2) & mask;
          }
          return default(Value);
        }
      }
      set {
        Contract.Requires(key != null);
        if (this.count >= this.resizeCount) this.Expand();
        this.AddInternal(key, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(Key key, out Value value) {
      Contract.Requires(key != null);
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        Key tableKey = keyValueTable[tableIndex].key;
        if (tableKey == null) {
          value = default(Value);
          return false;
        }
        if (object.ReferenceEquals(tableKey, key)) {
          value = keyValueTable[tableIndex].value;
          return value != dummyObject;
        }
        uint hash2 = HashHelper.HashInt2(hash);
        tableIndex = (tableIndex + hash2) & mask;
        while ((tableKey = keyValueTable[tableIndex].key) != null) {
          if (object.ReferenceEquals(tableKey, key)) {
            value = keyValueTable[tableIndex].value;
            return value != dummyObject;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        value = default(Value);
        return false;
      }
    }

    /// <summary>
    /// Enumerator for key value pairs.
    /// </summary>
    public struct KeyValuePairEnumerator {

      Hashtable<Key, Value> Hashtable;
      uint CurrentIndex;

      internal KeyValuePairEnumerator(Hashtable<Key, Value> hashtable) {
        this.Hashtable = hashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Current element
      /// </summary>
      public KeyValuePair Current {
        get {
          return this.Hashtable.keyValueTable[this.CurrentIndex];
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          uint size = this.Hashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          var keyValueTable = this.Hashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].value == null) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].value != null;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Get the enumerator
    /// </summary>
    /// <returns></returns>
    public KeyValuePairEnumerator GetEnumerator() {
      return new KeyValuePairEnumerator(this);
    }
  }

  /// <summary>
  /// Hashtable that can hold only single non zero uint per key.
  /// </summary>
#if !__MonoCS__
  [DebuggerTypeProxy(typeof(HashtableForUintValues<>.DebugView))]
#endif
  public sealed class HashtableForUintValues<Key> where Key : class {
    /// <summary>
    /// 
    /// </summary>
    public struct KeyValuePair {
      internal Key key;
      internal uint value;

      /// <summary>
      /// Returns a string containing the key and value.
      /// </summary>
      public override string ToString() {
        return "key = "+this.key+", value = "+this.value;
      }
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of two
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public HashtableForUintValues()
      : this(16) {
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public HashtableForUintValues(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      var table = this.keyValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i)
        table[i].key = null;
      this.count = 0;
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        var key = oldKeyValueTable[i].key;
        if (key != null)
          this.AddInternal(key, oldKeyValueTable[i].value);
      }
    }

    void AddInternal(Key key, uint value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        Key tableKey = keyValueTable[tableIndex].key;
        if (tableKey == null) {
          keyValueTable[tableIndex].key = key;
          keyValueTable[tableIndex].value = value;
          this.count++;
          return;
        }
        if (object.ReferenceEquals(tableKey, key)) {
          keyValueTable[tableIndex].value = value;
          return;
        }
        uint hash2 = HashHelper.HashInt2(hash);
        tableIndex = (tableIndex + hash2) & mask;
        while ((tableKey = keyValueTable[tableIndex].key) != null) {
          if (object.ReferenceEquals(tableKey, key)) {
            keyValueTable[tableIndex].value = value;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keyValueTable[tableIndex].key = key;
        keyValueTable[tableIndex].value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Sets this[key] = value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(Key key, uint value) {
      Contract.Requires(key != null);
      if (this.count >= this.resizeCount) this.Expand();
      this.AddInternal(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(Key key) {
      Contract.Requires(key != null);
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        Key tableKey = keyValueTable[tableIndex].key;
        if (tableKey == null) return false;
        if (object.ReferenceEquals(tableKey, key)) return true;
        uint hash2 = HashHelper.HashInt2(hash);
        tableIndex = (tableIndex + hash2) & mask;
        while ((tableKey = keyValueTable[tableIndex].key) != null) {
          if (object.ReferenceEquals(tableKey, key)) return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public uint this[Key key] {
      get {
        Contract.Requires(key != null);
        unchecked {
          uint mask = this.size - 1;
          var keyValueTable = this.keyValueTable;
          var hash = (uint)key.GetHashCode();
          uint hash1 = HashHelper.HashInt1(hash);
          uint tableIndex = hash1 & mask;
          Key tableKey = keyValueTable[tableIndex].key;
          if (tableKey == null) return 0;
          if (object.ReferenceEquals(tableKey, key)) return keyValueTable[tableIndex].value;
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((tableKey = keyValueTable[tableIndex].key) != null) {
            if (object.ReferenceEquals(tableKey, key)) return keyValueTable[tableIndex].value;
            tableIndex = (tableIndex + hash2) & mask;
          }
          return 0;
        }
      }
      set {
        Contract.Requires(key != null);
        if (this.count >= this.resizeCount) this.Expand();
        this.AddInternal(key, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(Key key, out uint value) {
      Contract.Requires(key != null);
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        var hash = (uint)key.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        Key tableKey = keyValueTable[tableIndex].key;
        if (tableKey == null) {
          value = 0;
          return false;
        }
        if (object.ReferenceEquals(tableKey, key)) {
          value = keyValueTable[tableIndex].value;
          return true;
        }
        uint hash2 = HashHelper.HashInt2(hash);
        tableIndex = (tableIndex + hash2) & mask;
        while ((tableKey = keyValueTable[tableIndex].key) != null) {
          if (object.ReferenceEquals(tableKey, key)) {
            value = keyValueTable[tableIndex].value;
            return true;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        value = 0;
        return false;
      }
    }

    internal class DebugView {

      public DebugView(HashtableForUintValues<Key> hashTable) {
        var numEntries = hashTable.Count;
        this.entries = new KeyValuePair[numEntries];
        var len = hashTable.keyValueTable.Length;
        var j = 0;
        for (int i = 0; i < len; i++) {
          this.entries[j++] = hashTable.keyValueTable[i];
        }
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public KeyValuePair[] entries;

    }
  }

  /// <summary>
  /// Hashtable that can hold only single non null value per uint key.
  /// </summary>
  /// <typeparam name="InternalT"></typeparam>
  public sealed class Hashtable<InternalT> where InternalT : class {
    /// <summary>
    /// 
    /// </summary>
    public struct KeyValuePair {
      /// <summary>
      /// 
      /// </summary>
      public uint Key;
      /// <summary>
      /// 
      /// </summary>
      public InternalT Value;
      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() {
        return "key = "+this.Key+", value = "+this.Value;
      }
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of two
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public Hashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public Hashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        uint key = oldKeyValueTable[i].Key;
        InternalT value = oldKeyValueTable[i].Value;
        if (value != null)
          this.AddInternal(key, value);
      }
    }

    void AddInternal(uint key, InternalT value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key) {
            keyValueTable[tableIndex].Value = value;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keyValueTable[tableIndex].Key = key;
        keyValueTable[tableIndex].Value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Add element to the Hashtable
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(uint key, InternalT value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key, value);
    }

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      if (this.count == 0) return;
      var table = this.keyValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i) {
        table[i].Key = 0;
        table[i].Value = null;
      }
      this.count = 0;
    }

    /// <summary>
    /// Find element in the Hashtable. Returns null if the element is not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public InternalT/*?*/ Find(uint key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint tableIndex = hash1 & mask;
        if (keyValueTable[tableIndex].Key == key) return keyValueTable[tableIndex].Value;
        uint hash2 = HashHelper.HashInt2(key);
        tableIndex = (tableIndex + hash2) & mask;
        InternalT result = null;
        while ((result = keyValueTable[tableIndex].Value) != null) {
          if (keyValueTable[tableIndex].Key == key) return result;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return null;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public InternalT this[uint key] {
      get {
        unchecked {
          uint mask = this.size - 1;
          var keyValueTable = this.keyValueTable;
          uint hash1 = HashHelper.HashInt1(key);
          uint tableIndex = hash1 & mask;
          if (keyValueTable[tableIndex].Key == key) return keyValueTable[tableIndex].Value;
          uint hash2 = HashHelper.HashInt2(key);
          tableIndex = (tableIndex + hash2) & mask;
          InternalT result = null;
          while ((result = keyValueTable[tableIndex].Value) != null) {
            if (keyValueTable[tableIndex].Key == key) return result;
            tableIndex = (tableIndex + hash2) & mask;
          }
          return null;
        }
      }
      set {
        if (this.count >= this.resizeCount) this.Expand();
        this.AddInternal(key, value);
      }
    }

    /// <summary>
    /// Enumerator for key value pairs.
    /// </summary>
    public struct KeyValuePairEnumerator {

      Hashtable<InternalT> Hashtable;
      uint CurrentIndex;

      internal KeyValuePairEnumerator(Hashtable<InternalT> hashtable) {
        this.Hashtable = hashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Current element
      /// </summary>
      public KeyValuePair Current {
        get {
          return this.Hashtable.keyValueTable[this.CurrentIndex];
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          uint size = this.Hashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          var keyValueTable = this.Hashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].Value == null) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Get the enumerator
    /// </summary>
    /// <returns></returns>
    public KeyValuePairEnumerator GetEnumerator() {
      return new KeyValuePairEnumerator(this);
    }

    /// <summary>
    /// Enumerator for elements
    /// </summary>
    public struct ValuesEnumerator {
      Hashtable<InternalT> Hashtable;
      uint CurrentIndex;
      internal ValuesEnumerator(
        Hashtable<InternalT> hashtable
      ) {
        this.Hashtable = hashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Current element
      /// </summary>
      public InternalT Current {
        get {
          return this.Hashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          uint size = this.Hashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          KeyValuePair[] keyValueTable = this.Hashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].Value == null) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable for elements
    /// </summary>
    public struct ValuesEnumerable {
      Hashtable<InternalT> Hashtable;

      internal ValuesEnumerable(
        Hashtable<InternalT> hashtable
      ) {
        this.Hashtable = hashtable;
      }

      /// <summary>
      /// Get the enumerator
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.Hashtable);
      }
    }

    /// <summary>
    /// Enumerable of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }
  }

  /// <summary>
  /// Hashtable that can hold only single non null value per ulong key.
  /// </summary>
  /// <typeparam name="InternalT"></typeparam>
  public sealed class HashtableUlong<InternalT> where InternalT : class {
    struct KeyValuePair {
      internal ulong Key;
      internal InternalT Value;
      public override string ToString() {
        return "key = "+this.Key+", value = "+this.Value;
      }
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of two
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public HashtableUlong()
      : this(16) {
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public HashtableUlong(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        var key = oldKeyValueTable[i].Key;
        InternalT value = oldKeyValueTable[i].Value;
        if (value != null)
          this.AddInternal(key, value);
      }
    }

    void AddInternal(ulong key, InternalT value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint key1 = (uint)(key >> 32);
        uint key2 = (uint)key;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != null) {
          if (keyValueTable[tableIndex].Key == key) {
            keyValueTable[tableIndex].Value = value;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keyValueTable[tableIndex].Key = key;
        keyValueTable[tableIndex].Value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Add element to the Hashtable
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(ulong key, InternalT value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key, value);
    }

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      if (this.count == 0) return;
      var table = this.keyValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i) {
        table[i].Key = 0;
        table[i].Value = null;
      }
      this.count = 0;
    }

    /// <summary>
    /// Find element in the Hashtable. Returns null if the element is not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public InternalT/*?*/ Find(ulong key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint key1 = (uint)(key >> 32);
        uint key2 = (uint)key;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint tableIndex = hash1 & mask;
        if (keyValueTable[tableIndex].Key == key) return keyValueTable[tableIndex].Value;
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        tableIndex = (tableIndex + hash2) & mask;
        InternalT result = null;
        while ((result = keyValueTable[tableIndex].Value) != null) {
          if (keyValueTable[tableIndex].Key == key) return result;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return null;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public InternalT this[ulong key] {
      get {
        unchecked {
          uint mask = this.size - 1;
          var keyValueTable = this.keyValueTable;
          uint key1 = (uint)(key >> 32);
          uint key2 = (uint)key;
          uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
          uint tableIndex = hash1 & mask;
          if (keyValueTable[tableIndex].Key == key) return keyValueTable[tableIndex].Value;
          uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
          tableIndex = (tableIndex + hash2) & mask;
          InternalT result = null;
          while ((result = keyValueTable[tableIndex].Value) != null) {
            if (keyValueTable[tableIndex].Key == key) return result;
            tableIndex = (tableIndex + hash2) & mask;
          }
          return null;
        }
      }
      set {
        if (this.count >= this.resizeCount) this.Expand();
        this.AddInternal(key, value);
      }
    }

    /// <summary>
    /// Enumerator for elements
    /// </summary>
    public struct ValuesEnumerator {
      HashtableUlong<InternalT> Hashtable;
      uint CurrentIndex;
      internal ValuesEnumerator(
        HashtableUlong<InternalT> hashtable
      ) {
        this.Hashtable = hashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Current element
      /// </summary>
      public InternalT Current {
        get {
          return this.Hashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          uint size = this.Hashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          KeyValuePair[] keyValueTable = this.Hashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].Value == null) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].Value != null;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable for elements
    /// </summary>
    public struct ValuesEnumerable {
      HashtableUlong<InternalT> Hashtable;

      internal ValuesEnumerable(
        HashtableUlong<InternalT> hashtable
      ) {
        this.Hashtable = hashtable;
      }

      /// <summary>
      /// Get the enumerator
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.Hashtable);
      }
    }

    /// <summary>
    /// Enumerable of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }
  }

  /// <summary>
  /// Hashtable that can hold only single uint value per uint key.
  /// </summary>
  public sealed class Hashtable {
    struct KeyValuePair {
      internal uint Key;
      internal uint Value;
    }
    KeyValuePair[] keyValueTable;
    uint size; //always a power of two
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public Hashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for Hashtable
    /// </summary>
    public Hashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keyValueTable = new KeyValuePair[this.size];
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      KeyValuePair[] oldKeyValueTable = this.keyValueTable;
      this.keyValueTable = new KeyValuePair[this.size*2];
      lock (this) { //force this.keyValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeyValueTable.Length;
      for (int i = 0; i < len; ++i) {
        uint key = oldKeyValueTable[i].Key;
        uint value = oldKeyValueTable[i].Value;
        if (value != 0)
          this.AddInternal(key, value);
      }
    }

    void AddInternal(uint key, uint value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != 0) {
          if (keyValueTable[tableIndex].Key == key) {
            Debug.Assert(keyValueTable[tableIndex].Value == value);
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keyValueTable[tableIndex].Key = key;
        keyValueTable[tableIndex].Value = value;
        this.count++;
      }
    }

    /// <summary>
    /// Add element to the Hashtable
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(uint key, uint value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(uint key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != 0) {
          if (keyValueTable[tableIndex].Key == key)
            return true;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return false;
      }
    }

    /// <summary>
    /// Find element in the Hashtable
    /// </summary>
    /// <param name="key"></param>
    public uint Find(uint key) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != 0) {
          if (keyValueTable[tableIndex].Key == key)
            return keyValueTable[tableIndex].Value;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return 0;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public uint this[uint key] {
      get {
        unchecked {
          uint mask = this.size - 1;
          var keyValueTable = this.keyValueTable;
          uint hash1 = HashHelper.HashInt1(key);
          uint hash2 = HashHelper.HashInt2(key);
          uint tableIndex = hash1 & mask;
          while (keyValueTable[tableIndex].Value != 0) {
            if (keyValueTable[tableIndex].Key == key)
              return keyValueTable[tableIndex].Value;
            tableIndex = (tableIndex + hash2) & mask;
          }
          return 0;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(uint key, out uint value) {
      unchecked {
        uint mask = this.size - 1;
        var keyValueTable = this.keyValueTable;
        uint hash1 = HashHelper.HashInt1(key);
        uint hash2 = HashHelper.HashInt2(key);
        uint tableIndex = hash1 & mask;
        while (keyValueTable[tableIndex].Value != 0) {
          if (keyValueTable[tableIndex].Key == key) {
            value = keyValueTable[tableIndex].Value;
            return true;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        value = 0;
        return false;
      }
    }

    /// <summary>
    /// Enumerator for elements
    /// </summary>
    public struct ValuesEnumerator {
      Hashtable Hashtable;
      uint CurrentIndex;
      internal ValuesEnumerator(
        Hashtable hashtable
      ) {
        this.Hashtable = hashtable;
        this.CurrentIndex = 0xFFFFFFFF;
      }

      /// <summary>
      /// Current element
      /// </summary>
      public uint Current {
        get {
          return this.Hashtable.keyValueTable[this.CurrentIndex].Value;
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          uint size = this.Hashtable.size;
          uint currentIndex = this.CurrentIndex + 1;
          if (currentIndex >= size) {
            return false;
          }
          KeyValuePair[] keyValueTable = this.Hashtable.keyValueTable;
          while (currentIndex < size && keyValueTable[currentIndex].Value == 0) {
            currentIndex++;
          }
          this.CurrentIndex = currentIndex;
          return currentIndex < size && keyValueTable[currentIndex].Value != 0;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.CurrentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable for elements
    /// </summary>
    public struct ValuesEnumerable {
      Hashtable Hashtable;

      internal ValuesEnumerable(
        Hashtable hashtable
      ) {
        this.Hashtable = hashtable;
      }

      /// <summary>
      /// Get the enumerator
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.Hashtable);
      }
    }

    /// <summary>
    /// Enumerable of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }
  }

  /// <summary>
  /// Hashtable that has two uints as its key. Its value is also uint
  /// </summary>
  public sealed class DoubleHashtable {
    struct Key1Key2ValueTriple {
      internal uint Key1;
      internal uint Key2;
      internal uint Value;
    }
    Key1Key2ValueTriple[] keysValueTable;
    uint size; //always a power of two
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (uint)(expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for DoubleHashtable
    /// </summary>
    public DoubleHashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for DoubleHashtable
    /// </summary>
    public DoubleHashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keysValueTable = new Key1Key2ValueTriple[this.size];
    }

    /// <summary>
    /// Count of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      Key1Key2ValueTriple[] oldKeysValueTable = this.keysValueTable;
      this.keysValueTable = new Key1Key2ValueTriple[this.size*2];
      lock (this) { //force this.keysValueTable into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeysValueTable.Length;
      for (int i = 0; i < len; ++i) {
        uint key1 = oldKeysValueTable[i].Key1;
        uint key2 = oldKeysValueTable[i].Key2;
        uint value = oldKeysValueTable[i].Value;
        if (value != 0) {
          bool ret = this.AddInternal(key1, key2, value);
          Debug.Assert(ret);
        }
      }
    }

    bool AddInternal(uint key1, uint key2, uint value) {
      unchecked {
        uint mask = this.size - 1;
        var keysValueTable = this.keysValueTable;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        uint tableIndex = hash1 & mask;
        while (keysValueTable[tableIndex].Value != 0) {
          if (keysValueTable[tableIndex].Key1 == key1 && keysValueTable[tableIndex].Key2 == key2) {
            return false;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keysValueTable[tableIndex].Key1 = key1;
        keysValueTable[tableIndex].Key2 = key2;
        keysValueTable[tableIndex].Value = value;
        this.count++;
        return true;
      }
    }

    /// <summary>
    /// Add element to the Hashtable
    /// </summary>
    public bool Add(uint key1, uint key2, uint value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      return this.AddInternal(key1, key2, value);
    }

    /// <summary>
    /// Find element in the Hashtable
    /// </summary>
    public uint Find(uint key1, uint key2) {
      unchecked {
        uint mask = this.size - 1;
        var keysValueTable = this.keysValueTable;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        uint tableIndex = hash1 & mask;
        while (keysValueTable[tableIndex].Value != 0) {
          if (keysValueTable[tableIndex].Key1 == key1 && keysValueTable[tableIndex].Key2 == key2)
            return keysValueTable[tableIndex].Value;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return 0;
      }
    }
  }

  /// <summary>
  /// Hashtable that has two uints as its key.
  /// </summary>
  public sealed class DoubleHashtable<T> where T : class {
    struct Key1Key2ValueTriple {
      internal uint Key1;
      internal uint Key2;
      internal T Value;
    }
    Key1Key2ValueTriple[] keysValueTable;
    uint size;
    uint resizeCount;
    uint count;
    const int loadPercent = 60;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (uint)(expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for DoubleHashtable
    /// </summary>
    public DoubleHashtable()
      : this(16) {
    }

    /// <summary>
    /// Constructor for DoubleHashtable
    /// </summary>
    public DoubleHashtable(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.keysValueTable = new Key1Key2ValueTriple[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Count of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      Key1Key2ValueTriple[] oldKeysValueTable = this.keysValueTable;
      this.keysValueTable = new Key1Key2ValueTriple[this.size*2];
      lock (this) { //Force this.keysValueTable to get updated before anyone can read the new Size value
        this.size <<= 1;
      }
      this.count = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldKeysValueTable.Length;
      for (int i = 0; i < len; ++i) {
        uint key1 = oldKeysValueTable[i].Key1;
        uint key2 = oldKeysValueTable[i].Key2;
        T value = oldKeysValueTable[i].Value;
        if (value != null) {
          bool ret = this.AddInternal(key1, key2, value);
          Debug.Assert(ret);
        }
      }
    }

    bool AddInternal(uint key1, uint key2, T value) {
      unchecked {
        var keysValueTable = this.keysValueTable;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        uint mask = this.size - 1;
        uint tableIndex = hash1 & mask;
        while (keysValueTable[tableIndex].Value != null) {
          if (keysValueTable[tableIndex].Key1 == key1 && keysValueTable[tableIndex].Key2 == key2) {
            return false;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keysValueTable[tableIndex].Key1 = key1;
        keysValueTable[tableIndex].Key2 = key2;
        keysValueTable[tableIndex].Value = value;
        this.count++;
        return true;
      }
    }

    /// <summary>
    /// Add element to the DoubleHashtable
    /// </summary>
    public bool Add(uint key1, uint key2, T value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      return this.AddInternal(key1, key2, value);
    }

    /// <summary>
    /// Find element in DoubleHashtable
    /// </summary>
    public T/*?*/ Find(uint key1, uint key2) {
      unchecked {
        uint mask = this.size - 1;
        var keysValueTable = this.keysValueTable;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        uint tableIndex = hash1 & mask;
        while (keysValueTable[tableIndex].Value != null) {
          if (keysValueTable[tableIndex].Key1 == key1 && keysValueTable[tableIndex].Key2 == key2)
            return keysValueTable[tableIndex].Value;
          tableIndex = (tableIndex + hash2) & mask;
        }
        return null;
      }
    }
  }

  /// <summary>
  /// A hash table used to keep track of a set of objects, providing methods to add objects to the set and to determine if an object is a member of the set.
  /// </summary>
  public sealed class SetOfObjects {
    object[] elements;
    uint size;
    uint resizeCount;
    uint count;
    uint dummyCount;
    const int loadPercent = 60;
    // ^ invariant (this.Size&(this.Size-1)) == 0;

    static object dummyObject = new object();

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for SetOfObjects
    /// </summary>
    public SetOfObjects()
      : this(16) {
    }

    /// <summary>
    /// Constructor for SetOfObjects
    /// </summary>
    public SetOfObjects(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.elements = new object[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Makes a clone of the given set.
    /// </summary>
    public SetOfObjects(SetOfObjects setToClone) {
      Contract.Requires(setToClone != null);
      this.size = setToClone.size;
      this.resizeCount = setToClone.resizeCount;
      this.count = setToClone.count;
      this.dummyCount = setToClone.dummyCount;
      var n = setToClone.elements.Length;
      this.elements = new object[n];
      for (int i = 0; i < n; i++)
        this.elements[i] = setToClone.elements[i];     
    }

    /// <summary>
    /// Removes all elements from the set.
    /// </summary>
    public void Clear() {
      this.count = 0;
      for (int i = 0, n = this.elements.Length; i < n; i++)
        this.elements[i] = null;
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      var oldElements = this.elements;
      this.elements = new object[this.size*2];
      lock (this) { //force this.elements into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.dummyCount = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldElements.Length;
      for (int i = 0; i < len; ++i) {
        var element = oldElements[i];
        if (element != null && element != dummyObject)
          this.AddInternal(element);
      }
    }

    bool AddInternal(object element) {
      unchecked {
        uint mask = this.size - 1;
        var elements = this.elements;
        var hash = (uint)element.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        var elem = elements[tableIndex];
        if (elem != null) {
          if (object.ReferenceEquals(elem, dummyObject)) {
            elements[tableIndex] = element;
            this.count++;
            this.dummyCount--;
            return true;
          }
          if (object.ReferenceEquals(elem, element)) return false;
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((elem = elements[tableIndex]) != null) {
            if (object.ReferenceEquals(elem, dummyObject)) {
              elements[tableIndex] = element;
              this.count++;
              this.dummyCount--;
              return true;
            }
            if (object.ReferenceEquals(elem, element)) return false;
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
        elements[tableIndex] = element;
        this.count++;
        return true;
      }
    }

    /// <summary>
    /// Returns false if the element is already in the set. Otherwise returns true and adds the element.
    /// </summary>
    /// <param name="element"></param>
    public bool Add(object element) {
      if (this.count+this.dummyCount >= this.resizeCount) this.Expand();
      return this.AddInternal(element);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool Contains(object element) {
      unchecked {
        uint mask = this.size - 1;
        var elements = this.elements;
        var hash = (uint)element.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        var elem = elements[tableIndex];
        if (elem != null) {
          if (object.ReferenceEquals(elem, element)) return true;
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((elem = elements[tableIndex]) != null) {
            if (object.ReferenceEquals(elem, element)) return true;
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    public void Remove(object element) {
      unchecked {
        uint mask = this.size - 1;
        var elements = this.elements;
        var hash = (uint)element.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        var elem = elements[tableIndex];
        if (elem != null) {
          if (object.ReferenceEquals(elem, element)) {
            elements[tableIndex] = dummyObject;
            this.count--;
            this.dummyCount++;
            return;
          }
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((elem = elements[tableIndex]) != null) {
            if (object.ReferenceEquals(elem, element)) {
              elements[tableIndex] = dummyObject;
              this.count--;
              this.dummyCount++;
              return;
            }
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
      }
    }

    /// <summary>
    /// Enumerator for elements
    /// </summary>
    public struct ValuesEnumerator {

      internal ValuesEnumerator(
        SetOfObjects setOfObjects
      ) {
        this.setOfObjects = setOfObjects;
        this.currentIndex = 0xFFFFFFFF;
      }

      SetOfObjects setOfObjects;
      uint currentIndex;

      /// <summary>
      /// Current element
      /// </summary>
      public object Current {
        get {
          return this.setOfObjects.elements[this.currentIndex];
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          var elements = this.setOfObjects.elements;
          uint size = (uint)elements.Length;
          uint currentIndex = this.currentIndex + 1;
          if (currentIndex >= size) return false;
          while (currentIndex < size) {
            var elem = elements[currentIndex];
            if (elem != null && elem != dummyObject) {
              this.currentIndex = currentIndex;
              return true;
            }
            currentIndex++;
          }
          this.currentIndex = currentIndex;
          return false;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.currentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable for elements
    /// </summary>
    public struct ValuesEnumerable {
      internal ValuesEnumerable(SetOfObjects setOfObjects) {
        this.setOfObjects = setOfObjects;
      }

      SetOfObjects setOfObjects;

      /// <summary>
      /// Get the enumerator
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.setOfObjects);
      }
    }

    /// <summary>
    /// Enumerable of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }
  }

  /// <summary>
  /// A hash table used to keep track of a set of non zero uint values, providing methods to add values to the set and to determine if an value is a member of the set.
  /// </summary>
  public sealed class SetOfUints {
    uint[] elements;
    uint size;
    uint resizeCount;
    uint count;
    uint dummyCount;
    const int loadPercent = 60;
    // ^ invariant (this.Size&(this.Size-1)) == 0;

    static uint SizeFromExpectedEntries(uint expectedEntries) {
      uint expectedSize = (expectedEntries * 10) / 6; ;
      uint initialSize = 16;
      while (initialSize < expectedSize && initialSize > 0) initialSize <<= 1;
      return initialSize;
    }

    /// <summary>
    /// Constructor for SetOfUints
    /// </summary>
    public SetOfUints()
      : this(16) {
    }

    /// <summary>
    /// Constructor for SetOfObjects
    /// </summary>
    public SetOfUints(uint expectedEntries) {
      this.size = SizeFromExpectedEntries(expectedEntries);
      this.resizeCount = this.size * 6 / 10;
      this.elements = new uint[this.size];
      this.count = 0;
    }

    /// <summary>
    /// Removes all elements from the set.
    /// </summary>
    public void Clear() {
      this.count = 0;
      for (int i = 0, n = this.elements.Length; i < n; i++)
        this.elements[i] = 0;
    }

    /// <summary>
    /// Number of elements
    /// </summary>
    public uint Count {
      get {
        return this.count;
      }
    }

    void Expand() {
      var oldElements = this.elements;
      this.elements = new uint[this.size*2];
      lock (this) { //force this.elements into memory before this.size gets increased.
        this.size <<= 1;
      }
      this.count = 0;
      this.dummyCount = 0;
      this.resizeCount = this.size * 6 / 10;
      int len = oldElements.Length;
      for (int i = 0; i < len; ++i) {
        var element = oldElements[i];
        if (element != 0 && element != uint.MaxValue)
          this.AddInternal(element);
      }
    }

    bool AddInternal(uint element) {
      unchecked {
        uint mask = this.size - 1;
        var elements = this.elements;
        var hash = element;
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        var elem = elements[tableIndex];
        if (elem != 0) {
          if (elem == uint.MaxValue) {
            elements[tableIndex] = element;
            this.dummyCount--;
            this.count++;
            return true;
          }
          if (elem == element) return false;
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((elem = elements[tableIndex]) != 0) {
            if (elem == uint.MaxValue) {
              elements[tableIndex] = element;
              this.dummyCount--;
              this.count++;
              return true;
            }
            if (elem == element) return false;
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
        elements[tableIndex] = element;
        this.count++;
        return true;
      }
    }

    /// <summary>
    /// Returns false if the element is already in the set. Otherwise returns true and adds the element.
    /// </summary>
    /// <param name="element"></param>
    public bool Add(uint element) {
      if (this.count+this.dummyCount >= this.resizeCount) this.Expand();
      return this.AddInternal(element);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool Contains(uint element) {
      unchecked {
        uint mask = this.size - 1;
        var elements = this.elements;
        var hash = element;
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        var elem = elements[tableIndex];
        if (elem != 0) {
          if (elem == element) return true;
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((elem = elements[tableIndex]) != 0) {
            if (elem == element) return true;
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    public void Remove(uint element) {
      unchecked {
        uint mask = this.size - 1;
        var elements = this.elements;
        var hash = (uint)element.GetHashCode();
        uint hash1 = HashHelper.HashInt1(hash);
        uint tableIndex = hash1 & mask;
        var elem = elements[tableIndex];
        if (elem != 0) {
          if (elem == element) {
            elements[tableIndex] = uint.MaxValue;
            this.count--;
            this.dummyCount++;
            return;
          }
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((elem = elements[tableIndex]) != 0) {
            if (elem == element) {
              elements[tableIndex] = uint.MaxValue;
              this.count--;
              this.dummyCount++;
              return;
            }
            tableIndex = (tableIndex + hash2) & mask;
          }
        }
      }
    }

    /// <summary>
    /// Enumerator for elements
    /// </summary>
    public struct ValuesEnumerator {

      internal ValuesEnumerator(SetOfUints setOfUints) {
        this.setOfUints = setOfUints;
        this.currentIndex = 0xFFFFFFFF;
      }

      SetOfUints setOfUints;
      uint currentIndex;

      /// <summary>
      /// Current element
      /// </summary>
      public uint Current {
        get {
          return this.setOfUints.elements[this.currentIndex];
        }
      }

      /// <summary>
      /// Move to next element
      /// </summary>
      public bool MoveNext() {
        unchecked {
          var elements = this.setOfUints.elements;
          uint size = (uint)elements.Length;
          uint currentIndex = this.currentIndex + 1;
          if (currentIndex >= size) return false;
          while (currentIndex < size) {
            var elem = elements[currentIndex];
            if (elem != 0 && elem != uint.MaxValue) {
              this.currentIndex = currentIndex;
              return true;
            }
            currentIndex++;
          }
          this.currentIndex = currentIndex;
          return false;
        }
      }

      /// <summary>
      /// Reset the enumerator
      /// </summary>
      public void Reset() {
        this.currentIndex = 0xFFFFFFFF;
      }
    }

    /// <summary>
    /// Enumerable for elements
    /// </summary>
    public struct ValuesEnumerable {
      internal ValuesEnumerable(SetOfUints setOfUints) {
        this.setOfUints = setOfUints;
      }

      SetOfUints setOfUints;

      /// <summary>
      /// Get the enumerator
      /// </summary>
      /// <returns></returns>
      public ValuesEnumerator GetEnumerator() {
        return new ValuesEnumerator(this.setOfUints);
      }
    }

    /// <summary>
    /// Enumerable of all the values
    /// </summary>
    public ValuesEnumerable Values {
      get {
        return new ValuesEnumerable(this);
      }
    }
  }

  /// <summary>
  /// A list of elements represented as a sublist of a master list. Use this to avoid allocating lots of little list objects.
  /// </summary>
  [ContractVerification(true)]
#if !__MonoCS__
  [DebuggerTypeProxy(typeof(Sublist<>.SublistView))]
#endif
  public struct Sublist<T> {

    /// <summary>
    /// A list of elements represented as a sublist of a master list. Use this to avoid allocating lots of little list objects.
    /// </summary>
    /// <param name="masterList">A list that contains this list as a sublist.</param>
    /// <param name="offset">The offset from masterList where this sublist starts.</param>
    /// <param name="count">The number of elements in this sublist.</param>
    public Sublist(List<T> masterList, int offset, int count) {
      Contract.Requires(masterList != null);
      Contract.Requires(offset >= 0);
      Contract.Requires(count >= 0);
      //Contract.Requires(offset+count >= 0); //no overflow
      Contract.Requires(offset+count <= masterList.Count);

      Contract.Assume(Contract.ForAll(masterList, e => e != null)); //Too hard for clients to prove right now.
      this.masterList = masterList;
      this.offset = offset;
      this.count = count;
    }

    List<T>/*?*/ masterList;
    int offset;
    int count;

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.count == 0 || this.masterList != null);
      //Contract.Invariant(this.masterList == null || Contract.ForAll(this.masterList, (e) => e != null));
      Contract.Invariant(this.offset >= 0);
      Contract.Invariant(this.count >= 0);
      Contract.Invariant(this.masterList == null || this.count <= this.masterList.Count);
      Contract.Invariant(this.offset+this.count >= 0);
      Contract.Invariant(this.masterList == null || this.offset+this.count <= this.masterList.Count);
    }


    internal class SublistView {

      public SublistView(Sublist<T> list) {
        var numEntries = list.Count;
        this.elements = new T[numEntries];
        for (int i = 0; i < numEntries; i++) {
          this.elements[i] = list[i];
        }
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public T[] elements;

    }

    /// <summary>
    /// The number of elements in this list.
    /// </summary>
    public int Count {
      get {
        Contract.Ensures(Contract.Result<int>() >= 0);
        return this.count;
      }
    }

    /// <summary>
    /// Returns the index of the given element if it occurs in this list. If not, returns -1.
    /// The comparision uses object identity will, so do not use this method if the element type is a value type.
    /// </summary>
    [ContractVerification(false)]
    public int Find(T element) {
      for (int i = 0, n = this.count; i < n; i++) {
        if (object.ReferenceEquals(this.masterList[this.offset+i], element))
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Returns a sublist of this sublist.
    /// </summary>
    /// <param name="offset">An offset from the start of this sublist.</param>
    /// <param name="count">The number of elements that should be in the resulting list.</param>
    [ContractVerification(false)]
    public Sublist<T> GetSublist(int offset, int count) {
      Contract.Requires(offset >= 0);
      Contract.Requires(count >= 0);
      Contract.Requires(offset < this.Count);
      Contract.Requires(offset+count <= this.Count);

      return new Sublist<T>(this.masterList, this.offset+offset, count);
    }

    /// <summary>
    /// The i'th element of this list.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public T this[int i] {
      [ContractVerification(false)]
      get {
        Contract.Requires(i >= 0);
        Contract.Requires(i < this.Count);
        Contract.Ensures(Contract.Result<T>() != null);
        Contract.Assume(this.masterList[this.offset+i] != null);
        return this.masterList[this.offset+i];
      }
      [ContractVerification(false)]
      set {
        Contract.Requires(i >= 0);
        Contract.Requires(i < this.Count);
        Contract.Requires(value != null);
        this.masterList[this.offset+i] = value;
      }
    }

    /// <summary>
    /// Returns an object that can enumerate the elements of this list.
    /// </summary>
    /// <returns></returns>
    [ContractVerification(false)]
    public Enumerator GetEnumerator() {
      return new Enumerator(this.masterList, this.offset, this.offset+this.count-1);
    }

    /// <summary>
    /// An enumerator for the elements of a Sublist.
    /// </summary>
    public struct Enumerator {

      /// <summary>
      /// An enumerator for the elements of a Sublist.
      /// </summary>
      /// <param name="masterList">A list of basic blocks that contains a sublist equal to the list that this enumerator will enumerate.</param>
      /// <param name="first">The index of the first element in the list to enumerate.</param>
      /// <param name="last">The index of the last element in the list to enumerate. If the list is empty, this is -1.</param>
      public Enumerator(List<T>/*?*/ masterList, int first, int last) {
        Contract.Requires(0 <= first);
        Contract.Requires(first == 0 || masterList != null);
        Contract.Requires(first == 0 || first <= masterList.Count);
        Contract.Requires(first <= last || last == first-1);
        Contract.Requires(last == -1 || masterList != null);
        Contract.Requires(last == -1 || last < masterList.Count);

        this.masterList = masterList;
        this.first = first-1;
        this.last = last;
      }

      List<T>/*?*/ masterList;
      int first;
      int last;

      [ContractInvariantMethod]
      private void ObjectInvariant() {
        Contract.Invariant(-1 <= this.first);
        Contract.Invariant(this.first == -1 || this.masterList != null);
        Contract.Invariant(this.first == -1 || this.first <= this.masterList.Count);
        Contract.Invariant(-1 <= this.last);
        Contract.Invariant(this.last == -1 || this.masterList != null);
        Contract.Invariant(this.last == -1 || this.last < this.masterList.Count);
      }

      /// <summary>
      /// True if the last call to MoveNext returned true, which means it is valid to Current to get the current element of the enumeration.
      /// </summary>
      public bool IsValid {
        get {
          Contract.Ensures(!Contract.Result<bool>() || this.first >= 0);
          Contract.Ensures(!Contract.Result<bool>() || this.masterList != null);
          Contract.Ensures(!Contract.Result<bool>() || this.first < this.masterList.Count);
          return 0 <= this.first && this.first <= this.last;
        }
      }

      /// <summary>
      /// True if there is another element in the enumeration and it is now valid to call Current to obtain this element.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext() {
        Contract.Ensures(!Contract.Result<bool>() || this.IsValid);
        if (this.first < this.last) {
          Contract.Assert(this.last != -1);
          Contract.Assert(this.masterList != null);
          this.first++;
          Contract.Assume(this.IsValid);
          return true;
        }
        return false;
      }

      /// <summary>
      /// The current element of the enumeration.
      /// </summary>
      public T Current {
        get {
          Contract.Requires(this.IsValid);
          return this.masterList[this.first];
        }
      }
    }


  }


}
