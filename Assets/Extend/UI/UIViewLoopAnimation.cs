using System;
using DG.Tweening;
using Extend.Common;
using Extend.UI.Animation;
using UnityEngine;

namespace Extend.UI {
	[Serializable]
	public class UIViewLoopAnimation : IUIAnimationPreview {
		public enum AnimationMode {
			ANIMATOR,
			STATE
		}

		public AnimationMode Mode = AnimationMode.STATE;

		[SerializeField]
		private bool m_enabled;

		public bool Enabled => m_enabled;

		[SerializeField]
		private ViewLoopStateCombine m_state;

		[SerializeField]
		private AnimatorParamProcessor m_processor;

		public Tween[] Active(Transform t) {
			switch( Mode ) {
				case AnimationMode.STATE:
					m_state.Active(t);
					return m_state.AllTween;
				case AnimationMode.ANIMATOR:
					m_processor.Apply();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return null;
		}

		public void CacheStartValue(Transform t) {
			if( Mode == AnimationMode.STATE ) {
				m_state.CacheStartValue(t);
			}
		}

		public Tween[] CollectPreviewTween(Transform transform) {
			return Active(transform);
		}

		public void Editor_Recovery(Transform transform) {
			if( Mode == AnimationMode.STATE ) {
				m_state.Editor_Recovery(transform);
			}
		}
	}
}