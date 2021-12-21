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
using XLua;

namespace Assets.SimpleSlider.Scripts {
	[RequireComponent(typeof(ScrollRect))]
	public class LuaMVVMHorizontalScrollSnap : LuaBindingEventBase, IBeginDragHandler, IEndDragHandler, IMVVMAssetReference, ILuaMVVM {
		[ReorderList, LabelText("On Page Changed ()"), SerializeField]
		private List<BindingEvent> m_onPageChangedEvent;
		public int SwipeThreshold = 50;
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Cell;

		private ScrollRect m_scroll;
		private bool m_drag;
		private bool m_lerp;
		private int m_page;

		public int Page {
			get => m_page;
			set {
				if( m_page == value ) {
					return;
				}
				m_page = value;
				TriggerPointerEvent("OnPageChanged", m_onPageChangedEvent, m_page);
			}
		}

		private LuaMVVMScrollViewComponent m_component;

		private void Awake() {
			m_scroll = GetComponent<ScrollRect>();
			m_component = new LuaMVVMScrollViewComponent(Cell, m_scroll);
		}

		public void Update() {
			if( !m_lerp || m_drag ) return;

			var horizontalNormalizedPosition = (float)Page / ( m_scroll.content.childCount - 1 );

			m_scroll.horizontalNormalizedPosition = Mathf.Lerp(m_scroll.horizontalNormalizedPosition, horizontalNormalizedPosition, 5 * Time.deltaTime);

			if( Math.Abs(m_scroll.horizontalNormalizedPosition - horizontalNormalizedPosition) < 0.001f ) {
				m_scroll.horizontalNormalizedPosition = horizontalNormalizedPosition;
				m_lerp = false;
			}
		}

		public void SlideNext() {
			Slide(1);
		}

		public void SlidePrev() {
			Slide(-1);
		}

		private void Slide(int direction) {
			direction = Math.Sign(direction);

			if( Page == 0 && direction == -1 || Page == m_scroll.content.childCount - 1 && direction == 1 ) return;

			m_lerp = true;
			Page += direction;
		}

		private int GetCurrentPage() {
			return Mathf.RoundToInt(m_scroll.horizontalNormalizedPosition * ( m_scroll.content.childCount - 1 ));
		}

		public void OnBeginDrag(PointerEventData eventData) {
			m_drag = true;
		}

		private Vector2 m_dragPosition;
		private Vector2 m_lastDragPosition;
		public void OnDrag(PointerEventData eventData) {
			m_lastDragPosition = m_dragPosition;
			m_dragPosition = eventData.position;
		}

		public void OnEndDrag(PointerEventData eventData) {
			var delta = m_lastDragPosition.x - eventData.position.x;

			if( Mathf.Abs(delta) > SwipeThreshold ) {
				var direction = Math.Sign(delta);
				Slide(direction);
			}
			else {
				var page = GetCurrentPage();
				if( page != Page ) {
					Slide(page - Page);
				}
			}

			m_drag = false;
			m_lerp = true;
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