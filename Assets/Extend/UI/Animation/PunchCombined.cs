using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class PunchCombined {
		public MovePunchAnimation Move;
		public RotatePunchAnimation Rotate;
		public ScalePunchAnimation Scale;

		private Vector3 m_startMove;
		private Vector3 m_startRotate;
		private Vector3 m_startScale;
		private float m_startAlpha;
		private bool m_cached;
		public Tween[] AllTween { get; private set; } = new Tween[3];

		public void Active(Transform t) {
			BuildAllTween(t);
			if( Application.isPlaying ) {
				foreach( var tweener in AllTween ) {
					tweener?.Restart();
				}
			}
		}
		
		public void CacheStartValue(Transform t) {
			if( !m_cached || !Application.isPlaying ) {
				var rectTransform = t as RectTransform;
				m_startMove = rectTransform.anchoredPosition3D;
				m_startRotate = rectTransform.localRotation.eulerAngles;
				m_startScale = rectTransform.localScale;
				var group = rectTransform.GetComponent<CanvasGroup>();
				m_startAlpha = group ? group.alpha : 1;
				m_cached = true;
			}
		}

		public void Editor_Recovery(Transform t) {
			var rectTransform = t as RectTransform;
			rectTransform.anchoredPosition3D = m_startMove;
			rectTransform.localRotation = Quaternion.Euler(m_startRotate);
			rectTransform.localScale = m_startScale;
			var group = rectTransform.GetComponent<CanvasGroup>();
			if( group ) {
				group.alpha = m_startAlpha;
			}
		}

		private void BuildAllTween(Transform t) {
			AllTween[0] = Move.Active(t);
			AllTween[1] = Rotate.Active(t);
			AllTween[2] = Scale.Active(t);
		}

		public void Stop() {
			foreach( var tweener in AllTween ) {
				tweener.Complete();
			}
		}

		public void Destroy() {
			foreach( var tween in AllTween ) {
				tween?.Kill();
			}
		}
	}
}