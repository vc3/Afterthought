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
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == value) {
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
          if (keyValueTable[tableIndex].Key == key && keyValueTable[tableIndex].Value == value) {
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
        Key tableKey = keyValueTable[tableIndex].key;
        if (object.ReferenceEquals(tableKey, key))
          return keyValueTable[tableIndex].value != dummyObject;
        uint hash2 = HashHelper.HashInt2(hash);
        tableIndex = (tableIndex + hash2) & mask;
        while ((tableKey = keyValueTable[tableIndex].key) != null) {
          if (object.ReferenceEquals(tableKey, key))
            return keyValueTable[tableIndex].value != dummyObject;
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
          if (object.ReferenceEquals(tableKey, key)) {
            var value = keyValueTable[tableIndex].value;
            if (value == dummyObject) value = default(Value);
            return value;
          }
          uint hash2 = HashHelper.HashInt2(hash);
          tableIndex = (tableIndex + hash2) & mask;
          while ((tableKey = keyValueTable[tableIndex].key) != null) {
            if (object.ReferenceEquals(tableKey, key)) {
              var value = keyValueTable[tableIndex].value;
              if (value == dummyObject) value = default(Value);
              return value;
            }
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
        var sortedList = new SortedList<Key, uint>();
        this.entries = new KeyValuePair[numEntries];
        var len = hashTable.keyValueTable.Length;
        for (int i = 0; i < len; i++) {
          if (hashTable.keyValueTable[i].value > 0)
            sortedList.Add(hashTable.keyValueTable[i].key, hashTable.keyValueTable[i].value);
        }
        var j = 0;
        foreach (var entry in sortedList) {
          this.entries[j++] = new KeyValuePair() { key = entry.Key, value = entry.Value };
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
#if !__MonoCS__
  [DebuggerTypeProxy(typeof(Hashtable<>.DebugView))]
#endif
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

    internal class DebugView {

      public DebugView(Hashtable<InternalT> hashTable) {
        var numEntries = hashTable.Count;
        this.entries = new KeyValuePair[numEntries];
        var len = hashTable.keyValueTable.Length;
        var j = 0;
        for (int i = 0; i < len; i++) {
          if (hashTable.keyValueTable[i].Value != null)
            this.entries[j++] = hashTable.keyValueTable[i];
        }
        Array.Sort(this.entries, (x, y) => (int)x.Key - (int)y.Key);
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public KeyValuePair[] entries;

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
#if !__MonoCS__
  [DebuggerTypeProxy(typeof(Hashtable.DebugView))]
#endif
  public sealed class Hashtable {
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
      public uint Value;
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
      set {
        this.Add(key, value);
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

    internal class DebugView {

      public DebugView(Hashtable hashTable) {
        var numEntries = hashTable.Count;
        this.entries = new KeyValuePair[numEntries*2];
        var len = hashTable.keyValueTable.Length;
        var j = 0;
        for (int i = 0; i < len; i++) {
          if (hashTable.keyValueTable[i].Value != 0)
            this.entries[j++] = hashTable.keyValueTable[i];
        }
        Array.Sort(this.entries, (x, y) => (int)x.Key - (int)y.Key);
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public KeyValuePair[] entries;

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

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear() {
      if (this.count == 0) return;
      var table = this.keysValueTable;
      int len = table.Length;
      for (int i = 0; i < len; ++i) {
        table[i].Key1 = 0;
        table[i].Key2 = 0;
        table[i].Value = null;
      }
      this.count = 0;
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
          this.AddInternal(key1, key2, value);
        }
      }
    }

    void AddInternal(uint key1, uint key2, T value) {
      unchecked {
        var keysValueTable = this.keysValueTable;
        uint hash1 = HashHelper.HashDoubleInt1(key1, key2);
        uint hash2 = HashHelper.HashDoubleInt2(key1, key2);
        uint mask = this.size - 1;
        uint tableIndex = hash1 & mask;
        while (keysValueTable[tableIndex].Value != null) {
          if (keysValueTable[tableIndex].Key1 == key1 && keysValueTable[tableIndex].Key2 == key2) {
            keysValueTable[tableIndex].Value = value;
            return;
          }
          tableIndex = (tableIndex + hash2) & mask;
        }
        keysValueTable[tableIndex].Key1 = key1;
        keysValueTable[tableIndex].Key2 = key2;
        keysValueTable[tableIndex].Value = value;
        this.count++;
        return;
      }
    }

    /// <summary>
    /// Add element to the DoubleHashtable
    /// </summary>
    public void Add(uint key1, uint key2, T value) {
      if (this.count >= this.resizeCount) {
        this.Expand();
      }
      this.AddInternal(key1, key2, value);
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
#if !__MonoCS__
  [DebuggerTypeProxy(typeof(SetOfUints.DebugView))]
#endif
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

    internal class DebugView {

      public DebugView(SetOfUints setOfUints) {
        var numEntries = setOfUints.Count;
        this.elements = new uint[numEntries];
        var len = setOfUints.elements.Length;
        var j = 0;
        for (int i = 0; i < len; i++) {
          var elem = setOfUints.elements[i];
          if (elem != 0 && elem != uint.MaxValue)
            this.elements[j++] = elem;
        }
        Array.Sort(this.elements, (x, y) => (int)x - (int)y);
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      readonly public uint[] elements;

    }

  }

  /// <summary>
  /// A list with a count and a readonly indexer. No other functionality is provided.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [ContractClass(typeof(ISimpleReadonlyListContract<>))]
  public interface ISimpleReadonlyList<out T> {
    /// <summary>
    /// The number of elements in this list.
    /// </summary>
    int Count {
      get;
    }

    /// <summary>
    /// The i'th element of this list.
    /// </summary>
    T this[int i] {
      get;
    }

  }

  #region ISimpleReadonlyList contract binding
  [ContractClassFor(typeof(ISimpleReadonlyList<>))]
  abstract class ISimpleReadonlyListContract<T> : ISimpleReadonlyList<T> {
    #region ISimpleReadonlyList<T> Members

    public int Count {
      get {
        Contract.Ensures(Contract.Result<int>() >= 0);
        throw new NotImplementedException(); 
      }
    }

    public T this[int i] {
      get {
        Contract.Requires(i >= 0);
        Contract.Requires(i < this.Count);
        Contract.Ensures(Contract.Result<T>() != null);
        throw new NotImplementedException(); 
      }
    }

    #endregion
  }
  #endregion


  /// <summary>
  /// A list of elements represented as a sublist of a master list. Use this to avoid allocating lots of little list objects.
  /// </summary>
  [ContractVerification(true)]
#if !__MonoCS__
  [DebuggerTypeProxy(typeof(Sublist<>.DebugView))]
#endif
  public struct Sublist<T> : ISimpleReadonlyList<T> {

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

    readonly List<T>/*?*/ masterList;
    readonly int offset;
    readonly int count;

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


    internal class DebugView {

      public DebugView(Sublist<T> list) {
        var numEntries = list.Count;
        this.elements = new T[numEntries];
        for (int i = 0; i < numEntries; i++) {
          this.elements[i] = list[i];
        }
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      readonly public T[] elements;

    }

    /// <summary>
    /// The number of elements in this list.
    /// </summary>
    public int Count {
      get {
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

      readonly List<T>/*?*/ masterList;
      int first;
      readonly int last;

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

  /// <summary>
  /// Functional List Extension methods
  /// </summary>
  [ContractVerification(true)]
  public static class FList
  {
    /// <summary>
    /// Returns a new list representing the appended lists
    /// </summary>
    [Pure]
    public static FList<T>/*?*/ Append<T>(this FList<T>/*?*/ l1, FList<T>/*?*/ l2)
    {
      Contract.Ensures((l1 == null || l2 == null) || Contract.Result<FList<T>>() != null);
      Contract.Ensures(l1 != null || Contract.Result<FList<T>>() == l2);
      Contract.Ensures(l2 != null || Contract.Result<FList<T>>() == l1);

      if (l1 == null) return l2;

      if (l2 == null) return l1;

      return l1.Tail.Append(l2).Cons(l1.Head);
    }

    /// <summary>
    /// Gives the length of the list
    /// </summary>
    [Pure]
    [ContractVerification(false)]
    public static int Length<T>(this FList<T>/*?*/ l)
    {
      Contract.Ensures(Contract.Result<int>() >= 0);
      Contract.Ensures(Contract.Result<int>() >= 1 || l == null);
      Contract.Ensures(Contract.Result<int>() <= 0 || l != null);
      Contract.Ensures(Contract.Result<int>() <= 1 || l.Tail != null);

      return FList<T>.Length(l);
    }

    /// <summary>
    /// Construct a new list by consing the element to the head of tail list
    /// </summary>
    [Pure]
    public static FList<T> Cons<T>(this FList<T>/*?*/ rest, T elem)
    {
      Contract.Ensures(Contract.Result<FList<T>>() != null);

      return FList<T>.Cons(elem, rest);
    }

    /// <summary>
    /// Constructs a new list that represents the reversed original list
    /// </summary>
    [Pure]
    public static FList<T>/*?*/ Reverse<T>(this FList<T>/*?*/ list)
    {
      Contract.Ensures(list == null || Contract.Result<FList<T>>() != null);

      return FList<T>.Reverse(list);
    }

    /// <summary>
    /// Constructs a new list that represents the reversed original list with the applied conversion
    /// </summary>
    [Pure]
    public static FList<R>/*?*/ Reverse<T, R>(this FList<T>/*?*/ list, Func<T, R> converter)
    {
      Contract.Ensures(list == null || Contract.Result<FList<R>>() != null);

      return FList<T>.Reverse(list, converter);
    }

    /// <summary>
    /// Return a list that only contains elements satisfying predicate in the same order 
    /// </summary>
    /// <param name="list">List from which to filter</param>
    /// <param name="predicate">Predicate to be satisfied to be in result</param>
    [Pure]
    public static FList<T> Filter<T>(this FList<T> list, Predicate<T> predicate)
    {
      Contract.Requires(predicate != null);

      if (list == null) return null;
      var tail = Filter(list.Tail, predicate);
      if (!predicate(list.Head)) return tail;
      if (tail == list.Tail) return list;
      return tail.Cons(list.Head);
    }

    /// <summary>
    /// Applies action to each element in list
    /// </summary>
    /// <param name="list">List iterated over</param>
    /// <param name="action">Action called on each element</param>
    public static void Apply<T>(this FList<T>/*?*/ list, Action<T> action)
    {
      FList<T>.Apply(list, action);
    }

    /// <summary>
    /// Enumerable over a functional list
    /// </summary>
    [Pure]
    public static IEnumerable<T> GetEnumerable<T>(this FList<T>/*?*/ list)
    {
      Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

      return FList<T>.PrivateGetEnumerable(list);
    }

    /// <summary>
    /// Is the list empty
    /// </summary>
    [Pure]
    public static bool IsEmpty<T>(this FList<T> list)
    {
      Contract.Ensures(Contract.Result<bool>() == (list == null));
      return list == null;
    }


    /// <summary>
    /// Returns last element of the non-empty list
    /// </summary>
    [Pure]
    public static T Last<T>(this FList<T> list)
    {
      Contract.Requires(!list.IsEmpty());
      if (list.Tail == null)
      {
        return list.Head;
      }
      return Last(list.Tail);
    }

    /// <summary>
    /// Returns second to the last element in a list of at least 2 elements.
    /// </summary>
    [Pure]
    // Needed when looking at access paths
    public static T ButLast<T>(this FList<T> list)
    {
      Contract.Requires(!list.IsEmpty());
      Contract.Requires(list.Tail != null);
      return ButLastInternal(list.Tail, list.Head);
    }

    [Pure]
    private static T ButLastInternal<T>(this FList<T> list, T butLast)
    {
      Contract.Requires(list != null);

      if (list.Tail == null)
        return butLast;
      return ButLastInternal(list.Tail, list.Head);
    }

    /// <summary>
    /// Coerce the list elements from S to T
    /// </summary>
    [Pure]
    public static FList<T> Coerce<S, T>(this FList<S> list) where S : T
    {
      return list.Map(orig => (T)orig);
    }

    /// <summary>
    /// True if the list contains the value
    /// </summary>
    [Pure]
    public static bool Contains<T>(this FList<T> list, T value) where T : IEquatable<T>
    {
      for (; list != null; list = list.Tail)
      {
        if (list.Head.Equals(value)) return true;
      }
      return false;
    }

    /// <summary>
    /// Return a list containing just that value
    /// </summary>
    [Pure]
    public static FList<T> Singleton<T>(this T elem)
    {
      Contract.Ensures(Contract.Result<FList<T>>() != null);

      return Cons(elem, null);
    }

    /// <summary>
    /// Add the value to the head of the list and return this new list
    /// </summary>
    [Pure]
    public static FList<T> Cons<T>(T elem, FList<T> tail)
    {
      Contract.Ensures(Contract.Result<FList<T>>() != null);

      return tail.Cons(elem);
    }

    /// <summary>
    /// Given a list of A's and a list of B's, pair up each A with each B via the mapper to produce a C and return this C list
    /// </summary>
    /// <param name="alist">List of A's</param>
    /// <param name="blist">List of B's</param>
    /// <param name="mapper">function combining one A and one B into a C</param>
    /// <param name="accumulator">Is the tail of the returned list.</param>
    /// <returns></returns>
    [Pure]
    public static FList<C> Product<A, B, C>(FList<A> alist, FList<B> blist, Func<A, B, C> mapper, FList<C> accumulator = null)
    {
      Contract.Requires(mapper != null);

      var alist2 = alist;
      while (alist2 != null)
      {
        var blist2 = blist;
        while (blist2 != null)
        {
          accumulator = accumulator.Cons(mapper(alist2.Head, blist2.Head));
          blist2 = blist2.Tail;
        }
        alist2 = alist2.Tail;
      }
      return accumulator;
    }

    /// <summary>
    /// Produces all tuples of A's, one from each list in the argument lists and passes it to the combine function
    /// whose results are accumulated and returned in the final list.
    /// If the combine function is the identity, it will produce the product of the original lists.
    /// </summary>
    [Pure]
    public static FList<B> Product<A, B>(this FList<FList<A>> lists, Func<FList<A>, B> combine, FList<B> accumulator = null)
    {
      if (lists == null) return accumulator;
      var first = lists.Head;
      if (lists.Tail == null) { return first.Map(e => combine(Singleton(e)), accumulator); }
      while (first != null)
      {
        var subProducts = Product(lists.Tail, l => l);
        Contract.Assume(first != null, "for some reason first gets havoced after product");
        accumulator = subProducts.Map(rest => combine(rest.Cons(first.Head)), accumulator);
        first = first.Tail;
      }
      return accumulator;
    }

    /// <summary>
    /// Produces a list by mapping the mapper over an enumerable. The elements in the list are in the same order as
    /// in the enumerable
    /// </summary>
    [Pure]
    public static FList<B> Map<A, B>(this IEnumerable<A> enumerable, Func<A, B> mapper)
    {
      Contract.Requires(enumerable != null);
      Contract.Requires(mapper != null);

      FList<B> result = null;
      foreach (var a in enumerable)
      {
        result = result.Cons(mapper(a));
      }
      return result.Reverse();
    }

    /// <summary>
    /// Transforms a T list into an S list using the converter
    /// </summary>
    [Pure]
    public static FList<S>/*?*/ Map<T, S>(this FList<T>/*?*/ list, Converter<T, S> converter, FList<S> accumulator = null)
    {
      Contract.Requires(converter != null);

      if (list == null) return accumulator;
      FList<S>/*?*/ tail = list.Tail.Map(converter, accumulator);
      return tail.Cons(converter(list.Head));
    }

    /// <summary>
    /// Produce an array containing the elements of the list in the same order
    /// </summary>
    [Pure]
    public static T[] ToArray<T>(this FList<T> list)
    {
      Contract.Ensures(Contract.Result<T[]>() != null);

      var result = new T[list.Length()];
      var i = 0;
      while (list != null)
      {
        Contract.Assume(i < result.Length, "need to relate list length with iteration");
        result[i++] = list.Head;
        list = list.Tail;
      }
      return result;
    }

    /// <summary>
    /// Return the longest common tail of the two lists, possibly null if nothing is in common.
    /// Uses pointer equality to determine that the tails are equal.
    /// </summary>
    public static FList<T> LongestCommonTail<T>(this FList<T> l1, FList<T> l2)
    {
      if (l1 == l2) return l1;
      if (l1 == null || l2 == null) return null;
      if (l1.Length() > l2.Length())
      {
        return LongestCommonTail(l1.Tail, l2);
      }
      else if (l1.Length() < l2.Length())
      {
        return LongestCommonTail(l1, l2.Tail);
      }
      else return LongestCommonTail(l1.Tail, l2.Tail);
    }
  }

  /// <summary>
  /// Functional lists. null represents the empty list.
  /// </summary>
  [Serializable]
  public class FList<T>
  {

    #region Privates
    private T elem;

    private FList<T>/*?*/ tail;

    private int count;
    #endregion

    /// <summary>
    /// Produces a new list consisting of the element at the head and the list at the tail
    /// </summary>
    public static FList<T> Cons(T elem, FList<T> tail)
    {
      Contract.Ensures(Contract.Result<FList<T>>() != null);

      return new FList<T>(elem, tail);
    }

    FList(T elem, FList<T>/*?*/ tail)
    {
      this.elem = elem;
      this.tail = tail;
      this.count = Length(tail) + 1;
    }

    /// <summary>
    /// The head element of the list
    /// </summary>
    public T Head { get { return this.elem; } }

    /// <summary>
    /// The tail of the list
    /// </summary>
    public FList<T>/*?*/ Tail
    {
      get { return this.tail; }
    }

    /// <summary>
    /// Reusable Empty list representation
    /// </summary>
    public const FList<T>/*?*/ Empty = null;

    /// <summary>
    /// Constructs a new list that represents the reversed original list
    /// </summary>
    public static FList<T>/*?*/ Reverse(FList<T>/*?*/ list)
    {
      Contract.Ensures(Contract.Result<FList<T>>() != null || list == null);

      if (list == null || list.Tail == null) return list;

      FList<T>/*?*/ tail = null;

      while (list != null)
      {
        tail = tail.Cons(list.elem);
        list = list.tail;
      }
      return tail;
    }

    /// <summary>
    /// Constructs a new list that represents the reversed original list
    /// </summary>
    public static FList<R>/*?*/ Reverse<R>(FList<T>/*?*/ list, Func<T, R> converter)
    {
      Contract.Ensures(Contract.Result<FList<R>>() != null || list == null);

      if (list == null) return null;

      FList<R>/*?*/ result = null;

      while (list != null)
      {
        result = result.Cons(converter(list.elem));
        list = list.tail;
      }
      return result;
    }


    internal static IEnumerable<T> PrivateGetEnumerable(FList<T>/*?*/ list)
    {
      Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

      FList<T>/*?*/ current = list;
      while (current != null)
      {
        T next = current.Head;
        current = current.Tail;
        yield return next;
      }
      yield break;
    }



    /// <summary>
    /// Query the list for the presence of an element
    /// </summary>
    public static bool Contains(FList<T>/*?*/ l, T/*!*/ o)
    {
      if (l == null) return false;

      if (o is IEquatable<T>) { if (((IEquatable<T>)o).Equals(l.elem)) return true; }
      else if (o.Equals(l.elem)) return true;

      return Contains(l.tail, o);
    }


    /// <summary>
    /// Applies action to each element in list
    /// </summary>
    /// <param name="list">List iterated over</param>
    /// <param name="action">Action called on each element</param>
    public static void Apply(FList<T>/*?*/ list, Action<T> action)
    {
      while (list != null)
      {
        action(list.Head);
        list = list.Tail;
      }
    }


    /// <summary>
    /// Given two sorted lists, compute their intersection
    /// </summary>
    /// <param name="l1">sorted list</param>
    /// <param name="l2">sorted list</param>
    /// <returns>sorted intersection</returns>
    public static FList<T>/*?*/ Intersect(FList<T>/*?*/ l1, FList<T>/*?*/ l2)
    {
      if (l1 == null || l2 == null) return null;

      int comp = System.Collections.Comparer.Default.Compare(l1.Head, l2.Head);
      if (comp < 0)
      {
        return Intersect(l1.Tail, l2);
      }
      if (comp > 0)
      {
        return Intersect(l1, l2.Tail);
      }
      // equal
      return Intersect(l1.Tail, l2.Tail).Cons(l1.Head);
    }

    /// <summary>
    /// Returns a new list that contains the elements of the argument list in increasing order
    /// </summary>
    public static FList<T>/*?*/ Sort(FList<T>/*?*/ l)
    {
      return Sort(l, null);
    }

    private static FList<T>/*?*/ Sort(FList<T>/*?*/ l, FList<T>/*?*/ tail)
    {
      // quicksort
      if (l == null) return tail;

      T pivot = l.Head;

      FList<T>/*?*/ less;
      FList<T>/*?*/ more;
      Partition(l.Tail, pivot, out less, out more);

      return Sort(less, Sort(more, tail).Cons(pivot));
    }

    private static void Partition(FList<T>/*?*/ l, T pivot, out FList<T>/*?*/ less, out FList<T>/*?*/ more)
    {
      less = null;
      more = null;
      if (l == null)
      {
        return;
      }
      foreach (T value in l.GetEnumerable())
      {
        if (System.Collections.Comparer.Default.Compare(value, pivot) <= 0)
        {
          less = less.Cons(value);
        }
        else
        {
          more = more.Cons(value);
        }
      }
    }
    /// <summary>
    /// Gives the length of the list
    /// </summary>
    [Pure]
    public static int Length(FList<T>/*?*/ l)
    {
      Contract.Ensures(Contract.Result<int>() >= 0);

      if (l == null) { return 0; }
      else return l.count;
    }

    /// <summary>
    /// Generates a string representing the list
    /// </summary>
    /// <returns></returns>
    //^ [Confined]
    public override string ToString()
    {
      var sb = new System.Text.StringBuilder();

      this.BuildString(sb);
      return sb.ToString();
    }

    /// <summary>
    /// Adds a string representation of the list to the StringBuilder
    /// </summary>
    public void BuildString(System.Text.StringBuilder sb)
    {
      string elemStr = this.elem == null ? "<null>" : this.elem.ToString();
      if (this.tail != null)
      {
        sb.AppendFormat("{0},", elemStr);
        this.tail.BuildString(sb);
      }
      else
      {
        sb.Append(elemStr);
      }
    }


  }

  /// <summary>
  /// A comparer for objects implementing IInternedKey
  /// </summary>
  public class InternedComparer<T> : IEqualityComparer<T>
    where T : IInternedKey
  {
    /// <summary>
    /// A comparer for objects implementing IInternedKey
    /// </summary>
    public static readonly InternedComparer<T> Default = new InternedComparer<T>();

    #region IEqualityComparer<T> Members

    bool IEqualityComparer<T>.Equals(T x, T y)
    {
      return x.InternedKey == y.InternedKey;
    }

    int IEqualityComparer<T>.GetHashCode(T obj)
    {
      return (int)obj.InternedKey;
    }

    #endregion
  }

  /// <summary>
  /// A set datastructure for objects implementing IInternedKey
  /// </summary>
  public class InternedSet<T> : HashSet<T>
    where T:IInternedKey
  {

    /// <summary>
    /// Create an empty set
    /// </summary>
    public InternedSet()
      : base(InternedComparer<T>.Default)
    {
    }
  }

  /// <summary>
  /// A table whose keys are objects implementing IInternedKey
  /// </summary>
  public class InternedTable<K, V> : Dictionary<K, V>
    where K : IInternedKey
  {
    /// <summary>
    /// Create atable whose keys are objects implementing IInternedKey
    /// </summary>
    public InternedTable()
      : base(InternedComparer<K>.Default)
    {
    }
  }

  /// <summary>
  /// A functional map from K to V. Requires function from K to int.
  /// </summary>
  public class FMap<K, V> : IEnumerable<KeyValuePair<K,V>>
  {
    /// <summary>
    /// Create an empty map.
    /// </summary>
    /// <param name="converter"></param>
    public FMap(Func<K, int> converter)
    {
      this.converter = converter;
    }

    private FIntMap<V> content;
    private FIntMap<K> keys;
    private Func<K, int> converter;

    /// <summary>
    /// Produces a new map from the old map with the given key value mapping inserted. Does not change original map.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public FMap<K, V> Insert(K key, V value)
    {
      Contract.Ensures(Contract.Result<FMap<K, V>>() != null);

      var kval = this.converter(key);
      return new FMap<K, V>(this.converter) { content = this.content.Insert(kval, value), keys = this.keys.Insert(kval, key) };
    }

    /// <summary>
    /// Lookup the key in the map and return the value if found.
    /// </summary>
    public bool TryGetValue(K key, out V value)
    {
      return this.content.TryGetValue(this.converter(key), out value);
    }

    #region IEnumerable<KeyValuePair<K,V>> Members

    /// <summary>
    /// Return list of key value pairs ordered by increasing key number
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
      List<KeyValuePair<K, V>> acc = new List<KeyValuePair<K, V>>();
      this.keys.Apply((i, k) => acc.Add(new KeyValuePair<K, V>(k, this.content[i])));
      return acc.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Extensions for functional map datastructure
  /// </summary>
  public static class FMap
  {
    /// <summary>
    /// Insert the key value into the map. Returns the new map without modifying existing map.
    /// 
    /// Null is the empty map
    /// </summary>
    public static FIntMap<T> Insert<T>(this FIntMap<T> map, int key, T value)
    {
      bool pushUp;
      return Insert(map, key, value, out pushUp);
    }

    /// <summary>
    /// Returns true if the given key is in the map.
    /// </summary>
    public static bool Contains<T>(this FIntMap<T> map, int key)
    {
      if (map == null) { return false; }
      if (key < map.key1) return map.Less.Contains(key);
      if (key == map.key1) { return true; }
      if (!map.IsThreeNode) return map.More.Contains(key);
      if (key < map.key2) return map.Mid.Contains(key);
      if (key == map.key2) { return true; }
      return map.More.Contains(key);
    }

    /// <summary>
    /// Depth of map tree
    /// </summary>
    public static int Depth<T>(this FIntMap<T> map)
    {
      if (map == null) return 0;
      var less = map.Less.Depth();
      var more = map.More.Depth();
      var mid = map.Mid.Depth();
      return 1 + Math.Max(less, Math.Max(mid, more));
    }

    /// <summary>
    /// Number of elements in map
    /// </summary>
    public static int Count<T>(this FIntMap<T> map)
    {
      if (map == null) return 0;
      var local = map.IsThreeNode ? 2 : 1;
      var less = map.Less.Count();
      var mid = map.Mid.Count();
      var more = map.More.Count();
      return local + less + mid + more;
    }

    /// <summary>
    /// Lookup a key in the map. Returns false if not in the map, otherwise retrieves the value associated with the key.
    /// </summary>
    public static bool TryGetValue<T>(this FIntMap<T> map, int key, out T value)
    {
      if (map == null) { value = default(T); return false; }
      if (key < map.key1) return map.Less.TryGetValue(key, out value);
      if (key == map.key1) { value = map.value1; return true; }
      if (!map.IsThreeNode) return map.More.TryGetValue(key, out value);
      if (key < map.key2) return map.Mid.TryGetValue(key, out value);
      if (key == map.key2) { value = map.value2; return true; }
      return map.More.TryGetValue(key, out value);
    }

    /// <summary>
    /// Apply given function over all elements of the map in increasing key order
    /// </summary>
    public static void Apply<T>(this FIntMap<T> map, Action<int, T> todo)
    {
      if (map == null) { return; }
      map.Less.Apply(todo);
      todo(map.key1, map.value1);
      if (map.IsThreeNode)
      {
        map.Mid.Apply(todo);
        todo(map.key2, map.value2);
      }
      map.More.Apply(todo);
    }

    internal static FIntMap<T> Insert<T>(this FIntMap<T> map, int key, T value, out bool pushUp)
    {
      if (map == null)
      {
        pushUp = true;
        return new FIntMap<T>(key, value);
      }
      if (map.key1 == key)
      {
        pushUp = false;
        var result = map.Clone();
        result.value1 = value;
        return result;
      }
      if (map.IsThreeNode && map.key2 == key)
      {
        pushUp = false;
        var result = map.Clone();
        result.value2 = value;
        return result;
      }
      bool localPushUp;
      FIntMap<T> toInsert;
      if (key < map.key1)
      {
        toInsert = Insert(map.Less, key, value, out localPushUp);
        if (!localPushUp)
        {
          pushUp = false;
          var result = map.Clone();
          result.Less = toInsert;
          return result;
        }
      }
      else
      {
        if (!map.IsThreeNode || key > map.key2)
        {
          toInsert = Insert(map.More, key, value, out localPushUp);
          if (!localPushUp)
          {
            pushUp = false;
            var result = map.Clone();
            result.More = toInsert;
            return result;
          }
        }
        else // mid insert
        {
          toInsert = Insert(map.Mid, key, value, out localPushUp);
          if (!localPushUp)
          {
            pushUp = false;
            var result = map.Clone();
            result.Mid = toInsert;
            return result;
          }
        }
      }
      return Insert(map, toInsert, out pushUp);
    }

    private static FIntMap<T> Insert<T>(FIntMap<T> node, FIntMap<T> toInsert, out bool pushUp)
    {
      pushUp = node.IsThreeNode;
      if (!pushUp)
      {
        if (toInsert.key1 < node.key1)
        {
          return new FIntMap<T>(toInsert.key1, toInsert.value1, node.key1, node.value1,
              toInsert.Less,
              toInsert.More,
              node.More
          );
        }
        else
        {
          return new FIntMap<T>(node.key1, node.value1, toInsert.key1, toInsert.value1,
              node.Less,
              toInsert.Less,
              toInsert.More
          );
        }
      }
      else return Split(node, toInsert);
    }

    /// <summary>
    /// Knowing that the node needs to be split create the new nodes
    /// </summary>
    /// <param name="node">Node to split</param>
    /// <param name="toInsert">Is a 1 node</param>
    private static FIntMap<T> Split<T>(FIntMap<T> node, FIntMap<T> toInsert)
    {
      if (toInsert.key1 < node.key1)
      {
        var left = toInsert;
        var right = new FIntMap<T>(node.key2, node.value2, node.Mid, node.More);
        return new FIntMap<T>(node.key1, node.value1, left, right);
      }
      if (toInsert.key1 < node.key2)
      {
        var left = new FIntMap<T>(node.key1, node.value1, node.Less, toInsert.Less);
        var right = new FIntMap<T>(node.key2, node.value2, toInsert.More, node.More);
        return new FIntMap<T>(toInsert.key1, toInsert.value1, left, right);
      }
      else
      {
        var left = new FIntMap<T>(node.key1, node.value1, node.Less, node.Mid);
        var right = toInsert;
        return new FIntMap<T>(node.key2, node.value2, left, right);
      }
    }

    /// <summary>
    /// Print the map data structure to console out (for debugging)
    /// </summary>
    public static void Print<T>(this FIntMap<T> node, string prefix)
    {
      if (node == null)
      {
        Console.WriteLine("{0}null", prefix);
      }
      else
      {
        if (!node.IsThreeNode)
        {
          Console.WriteLine("{0}{1} -> '{2}'", prefix, node.key1, node.value1);
        }
        else
        {
          Console.WriteLine("{0}{1} -> '{2}', {3} -> '{4}'", prefix, node.key1, node.value1, node.key2, node.value2);
        }
        var newprefix = prefix + "  ";
        Console.WriteLine("{0}Left:", prefix);
        node.Less.Print(newprefix);
        Console.WriteLine("{0}Mid:", prefix);
        node.Mid.Print(newprefix);
        Console.WriteLine("{0}More:", prefix);
        node.More.Print(newprefix);
      }
    }
  }


  /// <summary>
  /// Functional Int key map Type. Null is the empty map. All methods are extension methods.
  /// </summary>
  public class FIntMap<T>
  {
    internal int key1;
    internal T value1;
    internal int key2;
    internal T value2;
    internal FIntMap<T> Less;
    private FIntMap<T> mid;
    internal FIntMap<T> More;

    internal FIntMap() { }

    internal FIntMap(int key, T value)
    {
      this.key1 = key;
      this.value1 = value;
    }

    internal FIntMap(int key, T value, FIntMap<T> less, FIntMap<T> more)
    {
      this.key1 = key;
      this.value1 = value;
      this.Less = less;
      this.More = more;
    }

    internal FIntMap<T> Clone()
    {
      var result = (FIntMap<T>)this.MemberwiseClone();
      if (result.mid == this) { result.mid = result; }
      return result;
    }

    internal FIntMap(int key1, T value1, int key2, T value2)
    {
      this.key1 = key1;
      this.value1 = value1;
      this.key2 = key2;
      this.value2 = value2;
      // mark this as a two value leaf:
      this.mid = this;
    }

    internal FIntMap(int key1, T value1, int key2, T value2, FIntMap<T> less, FIntMap<T> mid, FIntMap<T> more)
    {
      this.key1 = key1;
      this.value1 = value1;
      this.key2 = key2;
      this.value2 = value2;
      // mark this as a two value leaf:
      this.mid = this;
      this.Less = less;
      this.More = more;
      if (mid != null) { this.mid = mid; }
    }

    internal bool IsThreeNode { get { return mid != null; } }

    internal FIntMap<T> Mid
    {
      get
      {
        if (this.mid == this) return null;
        return this.mid;
      }

      set
      {
        this.mid = value;
      }
    }

    /// <summary>
    /// If key is known to be in the map, one can use this to look up the value. Throws exception otherwise.
    /// </summary>
    public T this[int key]
    {
      get
      {
        T result;
        if (this.TryGetValue(key, out result)) return result;
        throw new KeyNotFoundException();
      }
    }

  }

  /// <summary>
  /// Functional set
  /// </summary>
  /// <typeparam name="K"></typeparam>
  public class FSet<K> : IEnumerable<K>
  {
    /// <summary>
    /// Create an empty set.
    /// </summary>
    /// <param name="converter"></param>
    public FSet(Func<K, int> converter)
    {
      this.converter = converter;
    }

    private FIntMap<K> keys;
    private Func<K, int> converter;

    /// <summary>
    /// Produces a new set from the old set with the given key inserted. Does not change original set.
    /// </summary>
    public FSet<K> Add(K key)
    {
      Contract.Ensures(Contract.Result<FSet<K>>() != null);

      var kval = this.converter(key);
      return new FSet<K>(this.converter) { keys = this.keys.Insert(kval, key) };
    }

    /// <summary>
    /// Return true if the key is in the set
    /// </summary>
    public bool Contains(K key)
    {
      K dummy;
      return this.keys.TryGetValue(this.converter(key), out dummy);
    }

    #region IEnumerable<K> Members

    /// <summary>
    /// Return list of key value pairs ordered by increasing key number
    /// </summary>
    /// <returns></returns>
    public IEnumerator<K> GetEnumerator()
    {
      List<K> acc = new List<K>();
      this.keys.Apply((i, k) => acc.Add(k));
      return acc.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

  }
}
