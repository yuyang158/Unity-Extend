﻿using System.Collections.Generic;
using Extend.Asset;
using Extend.Common;
using Extend.LuaBindingEvent;
using Extend.UI.Scroll;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaMVVM {
	[CSharpCallLua]
	public delegate AssetReference ProvideAssetReferenceCallback(LuaTable t, int index);
	
	[RequireComponent(typeof(LoopScrollRect))]
	public class LuaMVVMLoopScroll : LuaBindingEventBase, LoopScrollPrefabSource, LoopScrollDataSource, IMVVMAssetReference, ILuaMVVM {
		[ReorderList, LabelText("On Scroll Value Changed ()"), SerializeField]
		private List<BindingEvent> m_onScrollValueChanged;

		public AssetReference ScrollCellAsset;

		protected override void Awake() {
			base.Awake();
			m_scroll = GetComponent<LoopScrollRect>();
			m_scroll.dataSource = this;
			m_scroll.prefabSource = this;
			m_scroll.onValueChanged.AddListener(value => { TriggerPointerEvent("OnScrollValueChanged", m_onScrollValueChanged, value); });
		}

		private void OnDestroy() {
			m_arrayData?.Dispose();
		}

		private LoopScrollRect m_scroll;
		private LuaTable m_arrayData;
		
		private void OnEnable() {
			LuaArrayData = LuaArrayData;
		}

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData?.Dispose();
				m_arrayData = value;

				if( !(ScrollCellAsset is {GUIDValid: true}) ) {
					m_scroll.ClearCells();
				}

				if( m_arrayData == null ) {
					m_scroll.totalCount = 0;
				}
				else {
					m_scroll.totalCount = m_arrayData.Length;
					m_scroll.RefillCells();
				}
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

		private ProvideAssetReferenceCallback m_referenceProvideCallback;
		public AssetReference ProvideAssetReference(int index) {
			m_referenceProvideCallback ??= LuaArrayData.GetInPath<ProvideAssetReferenceCallback>("ReferenceProvideCallback");
			return m_referenceProvideCallback?.Invoke(LuaArrayData, index);
		}

		public AssetReference GetMVVMReference() {
			return ScrollCellAsset;
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

		public GameObject GetObject(int index) {
			if( ScrollCellAsset.GUIDValid ) {
				return ScrollCellAsset.Instantiate();
			}

			var reference = ProvideAssetReference(index);
			return reference.Instantiate();
		}

		public void ReturnObject(Transform trans) {
			AssetService.Recycle(trans);
		}
	}
}
