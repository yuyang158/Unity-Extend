using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class StateAnimationTemplate<MoveT, RotateT, ScaleT, FadeT> 
		where MoveT : StateAnimation 
		where RotateT : StateAnimation 
		where ScaleT : StateAnimation 
		where FadeT : StateAnimation {
		public MoveT Move;
		public RotateT Rotate;
		public ScaleT Scale;
		public FadeT Fade;

		public Tween[] AllTween { get; private set; } = new Tween[4];

		private bool m_cached;
		private Vector3 m_startMove;
		private Vector3 m_startRotate;
		private Vector3 m_startScale;
		private float m_startAlpha;
		private RectTransform m_rectTransform;

		public void Active(Transform t) {
			BuildAllTween(t);
			if( Application.isPlaying ) {
				foreach( var tween in AllTween ) {
					tween?.Restart();
				}
			}
		}

		public void CacheStartValue(Transform t) {
			if( !m_cached || !Application.isPlaying ) {
				m_rectTransform = t as RectTransform;
				m_startMove = m_rectTransform.anchoredPosition3D;
				m_startRotate = m_rectTransform.localRotation.eulerAngles;
				m_startScale = m_rectTransform.localScale;
				var group = m_rectTransform.GetComponent<CanvasGroup>();
				m_startAlpha = group ? group.alpha : 1;
				m_cached = true;
			}
		}

		public void Editor_Recovery(Transform t) {
			m_rectTransform = t as RectTransform;
			m_rectTransform.anchoredPosition3D = m_startMove;
			m_rectTransform.localRotation = Quaternion.Euler(m_startRotate);
			m_rectTransform.localScale = m_startScale;
			var group = m_rectTransform.GetComponent<CanvasGroup>();
			if( group ) {
				group.alpha = m_startAlpha;
			}
		}

		private void BuildAllTween(Transform t) {
			m_rectTransform = t as RectTransform;
			AllTween[0] = Move.Active(m_rectTransform, m_startMove);
			AllTween[1] = Rotate.Active(m_rectTransform, m_startRotate);
			AllTween[2] = Scale.Active(m_rectTransform, m_startScale);
			AllTween[3] = Fade.Active(m_rectTransform, Vector3.one * m_startAlpha);
		}

		public void Stop() {
			foreach( var tween in AllTween ) {
				tween?.Complete();
			}
		}

		public void Destroy() {
			foreach( var tween in AllTween ) {
				tween?.Kill();
			}
		}
	}
}