using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class StateCombine {
		public MoveStateAnimation Move;
		public RotationStateAnimation Rotation;
		public ScaleStateAnimation Scale;
		public FadeStateAnimation Fade;

		public Tween[] AllTween { get; private set; } = new Tween[4];

		public void Active(Transform t) {
			BuildAllTween(t);
			if( Application.isPlaying ) {
				foreach( var tween in AllTween ) {
					tween?.Restart();
				}
			}
		}

		private bool cacheStart;
		private Vector3 startMove;
		private Vector3 startRotate;
		private Vector3 startScale;
		private RectTransform rectTransform;

		private void BuildAllTween(Transform t) {
			if( !cacheStart ) {
				rectTransform = t as RectTransform;
				startMove = rectTransform.anchoredPosition3D;
				startRotate = rectTransform.localRotation.eulerAngles;
				startScale = rectTransform.localScale;
				cacheStart = true;
			}
			AllTween[0] = Move.Active(rectTransform, startMove);
			AllTween[1] = Rotation.Active(rectTransform, startRotate);
			AllTween[2] = Scale.Active(rectTransform, startScale);
			AllTween[3] = Fade.Active(rectTransform, Vector3.zero);
		}

		public void Stop() {
			foreach( var tween in AllTween ) {
				tween.Complete();
			}
		}
	}
}