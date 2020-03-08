using System;
using DG.Tweening;
using Extend.Common;
using Extend.UI.Animation;
using UnityEngine;

namespace Extend.UI {
	[Serializable]
	public class UIAnimation : IUIAnimationPreview {
		public enum AnimationMode {
			PUNCH,
			STATE,
			ANIMATOR
		}

		public AnimationMode Mode;

		[SerializeField, HideInInspector]
		private bool enabled;

		public bool Enabled => enabled;

		[SerializeField, HideInInspector]
		private PunchCombined punch;

		[SerializeField, HideInInspector]
		private StateCombine state;

		[SerializeField, HideInInspector]
		private AnimatorParamProcessor processor;

		public Tween[] Active(Transform t) {
			switch( Mode ) {
				case AnimationMode.PUNCH:
					punch.Active(t);
					return punch.AllTween;
				case AnimationMode.STATE:
					state.Active(t);
					return state.AllTween;
				case AnimationMode.ANIMATOR:
					processor.Apply();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return null;
		}

		public Tween[] CollectPreviewTween(Transform transform) {
			return Active(transform);
		}
	}
}