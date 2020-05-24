using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	public class AssetPool : IDisposable {
		public int PreferSize { get; private set; }

		public int MaxSize { get; private set; }

		private readonly List<GameObject> m_cached;
		private readonly Transform m_poolNode;

		public AssetPool(string name, int prefer, int max) {
			PreferSize = prefer;
			MaxSize = max;
			m_cached = new List<GameObject>(MaxSize);
			var root = AssetService.Get().PoolNode;
			var go = new GameObject(name);
			go.transform.SetParent(root, false);
			m_poolNode = go.transform;
		}

		public bool Cache(GameObject go) {
			if( m_cached.Count >= MaxSize ) {
				Object.Destroy(go);
				return false;
			}

			go.transform.SetParent(m_poolNode, false);
			m_cached.Add(go);
			return true;
		}

		public GameObject Get() {
			GameObject go = null;
			if( m_cached.Count > 0 ) {
				go = m_cached[0];
				m_cached.RemoveSwapAt(0);
			}

			return go;
		}

		public void Dispose() {
			m_cached.Clear();
			Object.Destroy(m_poolNode.gameObject);
		}
	}
}