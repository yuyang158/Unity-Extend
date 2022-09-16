using System.Collections.Generic;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaMVVM {
	public sealed class LuaMVVMScrollViewComponent {
		private readonly AssetReference m_cell;
		private readonly ScrollRect m_scroll;
		
		public LuaMVVMScrollViewComponent(AssetReference cell, ScrollRect scroll) {
			m_cell = cell;
			m_scroll = scroll;
		}
		
		private LuaTable m_arrayData;
		private readonly List<ILuaMVVM> m_items = new List<ILuaMVVM>(16);
		private readonly List<AssetReference.InstantiateAsyncContext> m_loadContexts = new List<AssetReference.InstantiateAsyncContext>(16);

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData = value;
				int length = m_arrayData?.Length ?? 0;
				for( int i = 0; i < length; i++ ) {
					var index = i;
					if( i >= m_items.Count + m_loadContexts.Count ) {
						AssetReference.InstantiateAsyncContext context;
						if( m_cell is {GUIDValid: true} ) {
							context = m_cell.InstantiateAsync(m_scroll.content);
						}
						else {
							var luaBindValue = m_arrayData.Get<int, LuaTable>(i + 1);
							var assetRef = luaBindValue.GetInPath<AssetReference>("assetRef");
							context = assetRef.InstantiateAsync(m_scroll.content);
						}
						context.Callback += go => {
							var luaData = LuaArrayData.Get<int, LuaTable>(index + 1);
							if( luaData == null ) {
								AssetService.Recycle(go);
							}
							else {
								go.name = index.ToString();
								var mvvm = go.GetComponent<ILuaMVVM>();
								mvvm.SetDataContext(luaData);
								m_items.Add(mvvm);
							}
							m_loadContexts.RemoveSwap(context);
						};
						m_loadContexts.Add(context);
					}
					else if( i < m_items.Count ) {
						var mvvm = m_items[i];
						var luaData = LuaArrayData.Get<int, LuaTable>(index + 1);
						mvvm.SetDataContext(luaData);
					}
				}

				while( m_items.Count > length ) {
					var last = m_items.Count - 1;
					Recycle(m_items[last]);
					m_items.RemoveAt(last);
				}
			}
		}

		private static void Recycle(IMVVMDetach mvvm) {
			mvvm.Detach();
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