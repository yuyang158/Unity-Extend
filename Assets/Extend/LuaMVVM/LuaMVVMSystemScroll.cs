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
	public class LuaMVVMSystemScroll : LuaBindingEventBase, IMVVMAssetReference {
		[ReorderList, LabelText("On Scroll End ()"), SerializeField]
		private List<BindingEvent> m_onScrollEndEvent;

		[ReorderList, LabelText("On Scroll Value Changed ()"), SerializeField]
		private List<BindingEvent> m_onScrollValueChanged;

		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Cell;
		private LuaMVVMScrollViewComponent m_component;

		private void Awake() {
			var scrollRect = GetComponent<ScrollRect>();
			scrollRect.onValueChanged.AddListener(position => {
				if( scrollRect.horizontal && position.x > 0.9999f ) {
					TriggerPointerEvent("OnScrollToEnd", m_onScrollEndEvent, null);
				}

				if( scrollRect.vertical && position.y > 0.9999f ) {
					TriggerPointerEvent("OnScrollToEnd", m_onScrollEndEvent, null);
				}
				TriggerPointerEvent("OnScrollValueChanged", m_onScrollValueChanged, position);
			});
			m_component = new LuaMVVMScrollViewComponent(Cell, scrollRect);
		}

		public LuaTable LuaArrayData {
			get => m_component.LuaArrayData;
			set => m_component.LuaArrayData = value;
		}

		private void OnDestroy() {
			m_component.OnDestroy();
			Cell.Dispose();
		}

		public AssetReference GetMVVMReference() {
			return Cell;
		}
	}
}