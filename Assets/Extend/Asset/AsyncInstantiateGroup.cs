using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class AsyncInstantiateGroup {
		private AssetReference m_ref;
		private List<AssetReference.InstantiateAsyncContext> m_contexts = new List<AssetReference.InstantiateAsyncContext>();
		
		public AsyncInstantiateGroup(AssetReference @m_ref) {
		}

		public void Clear() {
			if(m_contexts.Count == 0)
				return;
			foreach( var context in m_contexts ) {
				context.Cancel = true;
			}
			m_contexts.Clear();
		}

		private void CacheContext(AssetReference.InstantiateAsyncContext context) {
			m_contexts.Insert(0, context);
			context.Callback += go => {
				m_contexts.Remove(context);
			};
		}

		public AssetReference.InstantiateAsyncContext InstantiateAsync(Transform parent = null, bool stayWorldPosition = false) {
			var context = m_ref.InstantiateAsync(parent, stayWorldPosition);
			CacheContext(context);
			return context;
		}
		
		public AssetReference.InstantiateAsyncContext InstantiateAsync(Vector3 position) {
			var context = m_ref.InstantiateAsync(position, Quaternion.identity);
			CacheContext(context);
			return context;
		}
		
		public AssetReference.InstantiateAsyncContext InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null) {
			var context = m_ref.InstantiateAsync(position, rotation, parent);
			CacheContext(context);
			return context;
		}
	}
}