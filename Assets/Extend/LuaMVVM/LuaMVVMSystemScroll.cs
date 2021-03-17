using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using Extend.Common;
using Extend.LuaBindingEvent;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaMVVM {
	[RequireComponent(typeof(ScrollRect))]
	public class LuaMVVMSystemScroll : LuaBindingEventBase {
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Cell;

		private void Awake() {
			m_scroll = GetComponent<ScrollRect>();
			m_scroll.onValueChanged.AddListener(position => {
				if( m_scroll.horizontal && position.x > 0.9999f ) {
					TriggerPointerEvent("OnScrollToEnd", m_onScrollEndEvent, null);
				}

				if( m_scroll.vertical && position.y > 0.9999f ) {
					TriggerPointerEvent("OnScrollToEnd", m_onScrollEndEvent, null);
				}
			});
		}

		private ScrollRect m_scroll;
		private LuaTable m_arrayData;
		private readonly List<GameObject> m_items = new List<GameObject>(16);
		private readonly List<AssetReference.InstantiateAsyncContext> m_loadContexts = new List<AssetReference.InstantiateAsyncContext>(16);

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData = value;
				int length = m_arrayData?.Length ?? 0;
				for( int i = 0; i < length; i++ ) {
					var index = i;
					if( i >= m_items.Count + m_loadContexts.Count ) {
						var context = Cell.InstantiateAsync(m_scroll.content);
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

		[ReorderList, LabelText("On Scroll End ()"), SerializeField]
		private List<BindingEvent> m_onScrollEndEvent;

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
			Cell.Dispose();
		}
	}
}