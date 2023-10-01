using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class TweenAnimationAction : SwitcherAction {
		[SerializeField]
		private DOTweenAnimation[] m_animations;

		public override void ActiveAction() {
			foreach( DOTweenAnimation animation in m_animations ) {
				animation.RecreateTween();
				animation.tween.Play();
			}
		}

		public override void DeactiveAction() {
			foreach( DOTweenAnimation animation in m_animations ) {
				animation.tween.Kill(false);
			}
		}
	}
}