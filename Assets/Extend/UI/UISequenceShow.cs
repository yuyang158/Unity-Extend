using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp]
	public class UISequenceShow : MonoBehaviour {
		public float Delay = 0.1f;

		private WaitForSeconds m_wait;
		private Coroutine m_showRoutine;
		private readonly Queue<Transform> m_hiddenItems = new Queue<Transform>(16);

		private int m_childCount;
		private int m_hiddenCount;
		private bool m_showingElement;

		private void Awake() {
			m_wait = new WaitForSeconds(Delay);
		}

		private void OnTransformChildrenChanged() {
			if( m_childCount >= transform.childCount ) {
				return;
			}
			for( int i = m_childCount; i < transform.childCount; i++ ) {
				var t = transform.GetChild(i);
				if( !m_hiddenItems.Contains(t) ) {
					var canvas = t.GetComponent<Canvas>();
					canvas.enabled = false;
					m_hiddenItems.Enqueue(t);
				}
			}
			m_childCount = transform.childCount;
		}

		private void Update() {
			if( m_showingElement ) {
				return;
			}
			if( m_hiddenItems.Count > 0 ) {
				var t = m_hiddenItems.Dequeue();
				var canvas = t.GetComponent<Canvas>();
				canvas.enabled = true;
				var view = t.GetComponent<UIViewBase>();
				view.Show(() => {
					m_showingElement = false;
				});
				m_showingElement = true;
			}
		}
	}
}