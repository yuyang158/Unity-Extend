﻿using System.Collections.Generic;
using Extend.Common;
using Extend.LuaBindingEvent;
using Extend.UI.Scroll;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[RequireComponent(typeof(LoopScrollRect))]
	public class LuaMVVMLoopScroll : LuaBindingEventBase, ILoopScrollDataProvider {
		private void Awake() {
			m_scroll = GetComponent<LoopScrollRect>();
			m_scroll.dataSource = this;
		}

		private LoopScrollRect m_scroll;
		private LuaTable m_arrayData;

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData = value;
				m_scroll.ClearCells();
				m_scroll.totalCount = m_arrayData.Length;
				m_scroll.RefillCells();
			}
		}

		[ReorderList, LabelText("On Scroll End ()"), SerializeField]
		private List<BindingEvent> m_onScrollEndEvent;

		public void ProvideData(Transform t, int index) {
			var binding = t.GetComponent<ILuaMVVM>();
			binding.SetDataContext(m_arrayData.Get<int, LuaTable>(index + 1));
			if( m_scroll.totalCount - 1 == index ) {
				TriggerPointerEvent("OnScrollToEnd", m_onScrollEndEvent, null);
			}
		}
	}
}