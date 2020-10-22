using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScaleStateAnimation : StateAnimation {
		[SerializeField]
		private Vector3 m_scale = Vector3.one;
		public Vector3 Scale {
			get => m_scale;
			set {
				m_dirty = true;
				m_scale = value;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOScale(start + Scale, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}