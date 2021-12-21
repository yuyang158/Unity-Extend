using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[LuaCallCSharp]
	public class LuaMVVMForEach : MonoBehaviour, ILuaMVVM, IMVVMAssetReference {
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		private LuaTable m_arrayData;
		private readonly List<GameObject> m_items = new(16);
		private readonly List<AssetReference.InstantiateAsyncContext> m_loadContexts = new(16);

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}

		public LuaTable GetDataContext() {
			return LuaArrayData;
		}

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData = value;
				int elementCount = m_arrayData?.Length ?? 0;
				if( Asset is not {GUIDValid: true} ) {
					SetupWithExistElement(elementCount);
				}
				else {
					SetupWithAssetReference(elementCount);
				}
			}
		}

		private void SetupWithExistElement(int dataContextCount) {
			if( dataContextCount > transform.childCount ) {
				Debug.LogError($"Data count greater then children count. {dataContextCount} < {transform.childCount}");
			}

			for( var i = 0; i < transform.childCount; i++ ) {
				var child = transform.GetChild(i);
				var mvvm = child.GetComponent<ILuaMVVM>();
				if( i >= dataContextCount ) {
					child.gameObject.SetActive(false);
					mvvm.Detach();
				}
				else {
					child.gameObject.SetActive(true);
					if( mvvm == null ) {
						Debug.LogError($"ILuaMVVM not found in {child.name}.");
						continue;
					}
					mvvm.SetDataContext(m_arrayData.Get<int, LuaTable>(i + 1));
				}
			}
		}

		private void SetupWithAssetReference(int dataContextCount) {
			for( int i = 0; i < dataContextCount; i++ ) {
				var index = i;
				if( i >= m_items.Count + m_loadContexts.Count ) {
					var context = Asset.InstantiateAsync(transform);
					context.Callback += go => {
						var luaData = LuaArrayData.Get<int, LuaTable>(index + 1);
						if( luaData == null ) {
							Recycle(go);
						}
						else {
							var mvvm = go.GetComponent<ILuaMVVM>();
							mvvm.SetDataContext(luaData);
							m_items.Add(go);
						}

						m_loadContexts.Remove(context);
					};
					m_loadContexts.Add(context);
				}
				else if( i < m_items.Count ) {
					var go = m_items[i];
					var mvvm = go.GetComponent<ILuaMVVM>();
					var luaData = LuaArrayData.Get<int, LuaTable>(index + 1);
					mvvm.SetDataContext(luaData);
				}
			}

			while( m_items.Count > dataContextCount ) {
				var last = m_items.Count - 1;
				Recycle(m_items[last]);
				m_items.RemoveAt(last);
			}
		}

		private static void Recycle(GameObject go) {
			if( !CSharpServiceManager.Initialized )
				return;

			var mvvm = go.GetComponent<ILuaMVVM>();
			mvvm.Detach();
			AssetService.Recycle(go);
		}

		private void OnDestroy() {
			foreach( var go in m_items ) {
				Recycle(go);
			}

			m_items.Clear();
			foreach( var context in m_loadContexts ) {
				context.Cancel = true;
			}

			m_loadContexts.Clear();
			Asset.Dispose();
		}

		[BlackList]
		public void Detach() {
			LuaArrayData = null;
		}

		public AssetReference GetMVVMReference() {
			return Asset;
		}
	}
}