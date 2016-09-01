using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.ObjectModel {
  /// <summary>
  /// Represents a read-only, generic collection of key/value pairs.
  /// </summary>
  /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
  /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
  public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> {
    private readonly IDictionary<TKey, TValue> dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyDictionary{TKey,TValue}"/> class.
    /// </summary>
    public ReadOnlyDictionary() {
      this.dictionary = new Dictionary<TKey, TValue>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyDictionary{TKey,TValue}"/> class that is a wrapper around the specified dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) {
      this.dictionary = dictionary;
    }

    #region IDictionary<TKey,TValue> Members

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
      throw ReadOnlyException();
    }

    public bool ContainsKey(TKey key) {
      return dictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys {
      get { return dictionary.Keys; }
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key) {
      throw ReadOnlyException();
    }

    public bool TryGetValue(TKey key, out TValue value) {
      return dictionary.TryGetValue(key, out value);
    }

    public ICollection<TValue> Values {
      get { return dictionary.Values; }
    }

    public TValue this[TKey key] {
      get {
        return dictionary[key];
      }
    }

    TValue IDictionary<TKey, TValue>.this[TKey key] {
      get {
        return this[key];
      }
      set {
        throw ReadOnlyException();
      }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
      throw ReadOnlyException();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
      throw ReadOnlyException();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      return dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      dictionary.CopyTo(array, arrayIndex);
    }

    public int Count {
      get { return dictionary.Count; }
    }

    public bool IsReadOnly {
      get { return true; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
      throw ReadOnlyException();
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      return dictionary.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    #endregion

    private static Exception ReadOnlyException() {
      return new NotSupportedException("This dictionary is read-only");
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys {
      get { return this.Keys; }
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
      get { return this.Values; }
    }
  }
}
