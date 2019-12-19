using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class MoveStateAnimation : StateAnimation {
		[SerializeField]
		private Vector3 move;
		public Vector3 Move {
			get => move;
			set {
				dirty = true;
				move = value;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOAnchorPos3D(t.anchoredPosition3D + Move, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}