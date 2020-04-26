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

		public void Active(Transform t) {
			BuildAllTween(t);
			if( Application.isPlaying ) {
				foreach( var tween in AllTween ) {
					tween?.Restart();
				}
			}
		}

		public void CacheStartValue(Transform t) {
			if( !cached || !Application.isPlaying ) {
				rectTransform = t as RectTransform;
				startMove = rectTransform.anchoredPosition3D;
				startRotate = rectTransform.localRotation.eulerAngles;
				startScale = rectTransform.localScale;
				var group = rectTransform.GetComponent<CanvasGroup>();
				startAlpha = group ? group.alpha : 1;
				cached = true;
			}
		}

		public void Editor_Recovery(Transform t) {
			rectTransform = t as RectTransform;
			rectTransform.anchoredPosition3D = startMove;
			rectTransform.localRotation = Quaternion.Euler(startRotate);
			rectTransform.localScale = startScale;
			var group = rectTransform.GetComponent<CanvasGroup>();
			if( group ) {
				group.alpha = startAlpha;
			}
		}

		private bool cached;
		private Vector3 startMove;
		private Vector3 startRotate;
		private Vector3 startScale;
		private float startAlpha;
		private RectTransform rectTransform;

		private void BuildAllTween(Transform t) {
			rectTransform = t as RectTransform;
			AllTween[0] = Move.Active(rectTransform, startMove);
			AllTween[1] = Rotate.Active(rectTransform, startRotate);
			AllTween[2] = Scale.Active(rectTransform, startScale);
			AllTween[3] = Fade.Active(rectTransform, Vector3.one * startAlpha);
		}

		public void Stop() {
			foreach( var tween in AllTween ) {
				tween?.Complete();
			}
		}
	}
}