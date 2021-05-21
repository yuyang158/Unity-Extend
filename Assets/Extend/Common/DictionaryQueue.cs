using System;
using System.Collections;
using System.Collections.Generic;

namespace Extend.Common {
	public class DictionaryQueue<TKey, TValue> : IEnumerator<TValue> where TValue : class {
		private readonly Dictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>> m_dictionary;
		private readonly LinkedList<Tuple<TKey, TValue>> m_list;

		public DictionaryQueue(int capacity) {
			m_dictionary = new Dictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>(capacity);
			m_list = new LinkedList<Tuple<TKey, TValue>>();
		}

		public void Add(TKey key, TValue value) {
			if( m_dictionary.ContainsKey(key) ) {
				throw new ArgumentException("Key exist");
			}
			var node = m_list.AddFirst(new Tuple<TKey, TValue>(key, value));
			m_dictionary.Add(key, node);
		}

		public void Remove(TKey key) {
			if( !m_dictionary.TryGetValue(key, out var node) ) {
				return;
			}

			m_dictionary.Remove(key);
			m_list.Remove(node);
		}

		public void Enqueue(TKey key, TValue value) {
			Add(key, value);
		}

		public TValue Dequeue() {
			if( m_list.Count == 0 ) {
				return null;
			}
			
			var node = m_list.Last;
			Remove(node.Value.Item1);
			return node.Value.Item2;
		}

		public bool TryGetValue(TKey key, out TValue value) {
			var success = m_dictionary.TryGetValue(key, out var node);
			if( success ) {
				value = node.Value.Item2;
			}
			else {
				value = null;
			}

			return success;
		}

		public int Count => m_list.Count;

		private LinkedListNode<Tuple<TKey, TValue>> m_current;
		public bool MoveNext() {
			m_current = m_current.Next;
			return m_current != null;
		}

		public void Reset() {
			m_current = m_list.First;
		}

		public TValue Current => m_current?.Value.Item2;

		object IEnumerator.Current => Current;

		public void Dispose() {
			
		}
	}
}