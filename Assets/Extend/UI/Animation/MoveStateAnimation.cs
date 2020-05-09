using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extend.UI.Animation {
	[Serializable]
	public class MoveStateAnimation : StateAnimation {
		[SerializeField]
		private Vector3 m_move;
		public Vector3 Move {
			get => m_move;
			set {
				dirty = true;
				m_move = value;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOAnchorPos3D(t.anchoredPosition3D + Move, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}