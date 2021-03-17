using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	public class LuaMVVMForEach : MonoBehaviour, ILuaMVVM {
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		private LuaTable m_arrayData;
		private readonly List<GameObject> m_items = new List<GameObject>(16);
		private readonly List<AssetReference.InstantiateAsyncContext> m_loadContexts = new List<AssetReference.InstantiateAsyncContext>(16);

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData = value;
				int length = m_arrayData?.Length ?? 0;
				if( Asset == null || !Asset.GUIDValid ) {
					if( length > transform.childCount ) {
						Debug.LogError($"Data count greater then children count. {length} < {transform.childCount}");
					}

					for( var i = 0; i < transform.childCount; i++ ) {
						var child = transform.GetChild(i);
						if( i >= length ) {
							child.gameObject.SetActive(false);
						}
						else {
							child.gameObject.SetActive(true);
							var mvvm = child.GetComponent<ILuaMVVM>();
							mvvm.SetDataContext(m_arrayData.Get<int, LuaTable>(i + 1));
						}
					}
				}
				else {
					for( int i = 0; i < length; i++ ) {
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

					while( m_items.Count > length ) {
						var last = m_items.Count - 1;
						AssetService.Recycle(m_items[last]);
						m_items.RemoveAt(last);
					}
				}
			}
		}

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}

		private void Recycle(GameObject go) {
			var bindings = go.GetComponentsInChildren<ILuaMVVM>();
			foreach( var binding in bindings ) {
				binding.Detach();
			}

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

		public void Detach() {
		}
	}
}