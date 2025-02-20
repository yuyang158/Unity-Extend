using System;
using System.Collections.Generic;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaMVVM {
	public sealed class LuaMVVMScrollViewComponent {
		private readonly AssetReference m_cell;
		private readonly Transform m_itemRoot;
		private readonly bool m_usingExistItem;
		
		public LuaMVVMScrollViewComponent(AssetReference cell, Transform itemRoot) {
			m_cell = cell;
			m_itemRoot = itemRoot;
			m_usingExistItem = itemRoot.childCount > 0 && !cell.GUIDValid;
			if( m_usingExistItem ) {
				for( int i = 0; i < itemRoot.childCount; i++ ) {
					var mvvm = itemRoot.GetChild(i).GetComponent<ILuaMVVM>();
					if( mvvm != null ) {
						m_items.Add(mvvm);
					}
				}
			}
		}
		
		private LuaTable m_arrayData;
		private readonly List<ILuaMVVM> m_items = new(16);
		private readonly List<AssetReference.InstantiateAsyncContext> m_loadContexts = new(16);

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData = value;
				if( !m_itemRoot.gameObject.activeInHierarchy ) {
					return;
				}
				
				int length = m_arrayData?.Length ?? 0;
				for( int i = 0; i < length; i++ ) {
					var index = i;
					if( i >= m_items.Count + m_loadContexts.Count ) {
						if( m_usingExistItem ) {
							throw new Exception($"Data count out of child count. {length} -> {m_items.Count}");
						}
						AssetReference.InstantiateAsyncContext context;
						if( m_cell is {GUIDValid: true} ) {
							context = m_cell.InstantiateAsync(m_itemRoot);
						}
						else {
							var luaBindValue = m_arrayData.Get<int, LuaTable>(i + 1);
							var assetRef = luaBindValue.GetInPath<AssetReference>("assetRef");
							context = assetRef.InstantiateAsync(m_itemRoot);
						}
						context.Callback += go => {
							if( LuaArrayData == null ) {
								AssetService.Recycle(go);
								return;
							}
							var luaData = LuaArrayData.Get<int, LuaTable>(index + 1);
							if( luaData == null ) {
								AssetService.Recycle(go);
							}
							else {
								go.name = index.ToString();
								var mvvm = go.GetComponent<ILuaMVVM>();

								mvvm.SetDataContext(luaData);

								var callback = luaData.GetInPath<LuaFunction>("InstantiateCallback");
								callback?.Action(luaData, go);
								m_items.Add(mvvm);
							}
							m_loadContexts.RemoveSwap(context);
						};
						m_loadContexts.Add(context);
					}
					else if( i < m_items.Count ) {
						var mvvm = m_items[i];
						var component = mvvm as Component;
						component.gameObject.SetActive(true);
						var luaData = LuaArrayData.Get<int, LuaTable>(index + 1);
						mvvm.SetDataContext(luaData);

						var callback = luaData.GetInPath<LuaFunction>("InstantiateCallback");
						callback?.Action(luaData, component.gameObject);
					}
				}

				if( m_usingExistItem ) {
					for( int i = length; i < m_items.Count; i++ ) {
						var component = m_items[i] as Component;
						component.gameObject.SetActive(false);
					}
				}
				else {
					while( m_items.Count > length ) {
						var last = m_items.Count - 1;
						Recycle(m_items[last]);
						m_items.RemoveAt(last);
					}
				}
			}
		}

		private void Recycle(IMVVMDetach mvvm) {
			if(!CSharpServiceManager.Initialized)
				return;
			mvvm.Detach();
			if(!m_usingExistItem)
				AssetService.Recycle(mvvm as Component);
		}

		public void OnDestroy() {
			foreach( var mvvm in m_items ) {
				Recycle(mvvm);
			}

			m_items.Clear();
			foreach( var context in m_loadContexts ) {
				context.Cancel = true;
			}

			m_loadContexts.Clear();
			m_cell.Dispose();
			m_arrayData?.Dispose();
		}
	}
}
