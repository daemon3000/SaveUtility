//	Based on the work of Thomas Levesque
//	http://stackoverflow.com/a/1269311

using System;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility
{
	public class ReadOnlyDictionaryException : Exception
	{
		public ReadOnlyDictionaryException() :
			base("This dictionary is read-only.") { }
	}
	
	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private IDictionary<TKey, TValue> _dictionary;
		
		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}
		
		#region [IDictionary Members]
		public ICollection<TKey> Keys 
		{
			get { return _dictionary.Keys; }
		}
		
		public ICollection<TValue> Values 
		{
			get { return _dictionary.Values; }
		}
		
		public TValue this[TKey key] 
		{
			get 
			{
				return _dictionary [key];
			}
		}
		
		TValue IDictionary<TKey, TValue>.this[TKey key] 
		{
			get { return this[key]; }
			set { throw new ReadOnlyDictionaryException(); }
		}
		
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			throw new ReadOnlyDictionaryException();
		}
		
		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}
		
		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			throw new ReadOnlyDictionaryException();
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}
		#endregion
		
		#region [ICollection Members]
		public int Count 
		{
			get { return _dictionary.Count; }
		}
		
		public bool IsReadOnly 
		{
			get { return true; }
		}
		
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			throw new ReadOnlyDictionaryException();
		}
		
		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			throw new ReadOnlyDictionaryException();
		}
		
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _dictionary.Contains(item);
		}
		
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_dictionary.CopyTo(array, arrayIndex);
		}
		
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new ReadOnlyDictionaryException();
		}
		#endregion
		
		#region [IEnumerable Members]
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator();
		}
		#endregion
	}
}