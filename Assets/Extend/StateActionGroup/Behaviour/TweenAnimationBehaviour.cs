using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

namespace Extend.StateActionGroup.Behaviour {
	[Serializable]
	public class TweenAnimationBehaviourData : BehaviourDataBase {
		public float delay;
		public float duration = 1;
		public Ease easeType = Ease.OutQuad;
		
		public float endValueFloat;
		public Vector3 endValueV3;
		public Vector2 endValueV2;
		public Color endValueColor = new Color(1, 1, 1, 1);
		public string endValueString = "";
		public Rect endValueRect = new Rect(0, 0, 0, 0);
		public Transform endValueTransform;
		public override void ApplyToBehaviour(BehaviourBase behaviour) {
			var tweenAnimationBehaviour = behaviour as TweenAnimationBehaviour;
			var tweenAnimation = tweenAnimationBehaviour.TweenAnimation;
			if(!tweenAnimation)
				return;
			tweenAnimation.delay = delay;
			tweenAnimation.duration = duration;
			tweenAnimation.easeType = easeType;

			tweenAnimation.endValueFloat = endValueFloat;
			tweenAnimation.endValueV3 = endValueV3;
			tweenAnimation.endValueV2 = endValueV2;
			tweenAnimation.endValueColor = endValueColor;
			tweenAnimation.endValueString = endValueString;
			tweenAnimation.endValueRect = endValueRect;
			tweenAnimation.endValueTransform = endValueTransform;
		}

		public override void CopySourceBehaviour(BehaviourBase behaviour) {
			var tweenAnimationBehaviour = behaviour as TweenAnimationBehaviour;
			var tweenAnimation = tweenAnimationBehaviour.TweenAnimation;
			if(!tweenAnimation)
				return;
			delay = tweenAnimation.delay;
			duration = tweenAnimation.duration;
			easeType = tweenAnimation.easeType;

			endValueFloat = tweenAnimation.endValueFloat;
			endValueV3 = tweenAnimation.endValueV3;
			endValueV2 = tweenAnimation.endValueV2;
			endValueColor = tweenAnimation.endValueColor;
			endValueString = tweenAnimation.endValueString;
			endValueRect = tweenAnimation.endValueRect;
			endValueTransform = tweenAnimation.endValueTransform;
		}
	}
	
	[Serializable, Preserve]
	public class TweenAnimationBehaviour : BehaviourBase {
		[SerializeField]
		private DOTweenAnimation m_tweenAnimation;

		public DOTweenAnimation TweenAnimation => m_tweenAnimation;
		private bool m_complete;

		public override void Start() {
			if(!TweenAnimation)
				return;
			m_data.ApplyToBehaviour(this);
			TweenAnimation.isFrom = false;
			TweenAnimation.RecreateTween();
			TweenAnimation.tween.Play();
			m_complete = false;
			TweenAnimation.tween.onComplete += () => {
				m_complete = true;
			};
		}

		public override void Exit() {
			if(!TweenAnimation)
				return;
			TweenAnimation.tween.Kill();
			m_complete = false;
		}

		public override bool Complete => !m_tweenAnimation || m_complete;

		public override BehaviourDataBase CreateDefaultData() {
			var tweenData = new TweenAnimationBehaviourData();
			tweenData.CopySourceBehaviour(this);
			tweenData.TargetId = Id;
			return tweenData;
		}
	}
}