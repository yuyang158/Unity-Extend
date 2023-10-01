using System;
using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using Extend.Common;
using Extend.LuaBindingEvent;
using Extend.LuaMVVM;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using XLua;

namespace Extend.LuaMVVM {
	[RequireComponent(typeof(ScrollSnapBase))]
	public class LuaMVVMScrollSnap : LuaBindingEventBase, IMVVMAssetReference, ILuaMVVM {
		[ReorderList, LabelText("On Page Changed ()"), SerializeField]
		private List<BindingEvent> m_onPageChangedEvent;

		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Cell;

		private ScrollSnapBase m_snap;

		public int Page {
			get => m_snap.CurrentPage;
			set {
				if( m_snap.CurrentPage == value ) {
					return;
				}
				m_snap.ChangePage(value);
				TriggerPointerEvent("OnPageChanged", m_onPageChangedEvent, m_snap.CurrentPage);
			}
		}

		private LuaMVVMScrollViewComponent m_component;

		protected override void Awake() {
			m_snap = GetComponent<ScrollSnapBase>();
			m_component = new LuaMVVMScrollViewComponent(Cell, GetComponent<ScrollRect>().content);
		}

		public void SlideNext() {
			Slide(1);
		}

		public void SlidePrev() {
			Slide(-1);
		}

		private void Slide(int direction) {
			direction = Math.Sign(direction);
			Page += direction;
		}

		public LuaTable LuaArrayData {
			get => m_component.LuaArrayData;
			set => m_component.LuaArrayData = value;
		}

		private void OnDestroy() {
			m_component.OnDestroy();
		}

		public AssetReference GetMVVMReference() {
			return Cell;
		}

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}

		public LuaTable GetDataContext() {
			return LuaArrayData;
		}

		public void Detach() {
			LuaArrayData = null;
		}
	}
}
