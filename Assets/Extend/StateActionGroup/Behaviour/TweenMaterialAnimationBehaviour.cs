using System;
using DG.Tweening;
using Extend.DoTween;
using UnityEngine;

namespace Extend.StateActionGroup.Behaviour {
	[Serializable]
	public class TweenMaterialAnimationBehaviourData : BehaviourDataBase {
		public float delay;
		public float duration = 1;
		public Ease easeType = Ease.OutQuad;

		public float endValueFloat;
		public Vector4 endValueV4;
		public Color endValueColor = new Color(1, 1, 1, 1);

		public override void ApplyToBehaviour(BehaviourBase behaviour) {
			var tweenMaterialAnimationBehaviour = behaviour as TweenMaterialAnimationBehaviour;
			if( !tweenMaterialAnimationBehaviour.MaterialPlayer )
				return;
			tweenMaterialAnimationBehaviour.MaterialPlayer.FloatEndValue = endValueFloat;
			tweenMaterialAnimationBehaviour.MaterialPlayer.ColorEndValue = endValueColor;
			tweenMaterialAnimationBehaviour.MaterialPlayer.VectorEndValue = endValueV4;
			tweenMaterialAnimationBehaviour.MaterialPlayer.Duration = duration;
			tweenMaterialAnimationBehaviour.MaterialPlayer.Ease = easeType;
			tweenMaterialAnimationBehaviour.MaterialPlayer.Delay = delay;
		}

		public override void CopySourceBehaviour(BehaviourBase behaviour) {
			var tweenMaterialAnimationBehaviour = behaviour as TweenMaterialAnimationBehaviour;
			if( !tweenMaterialAnimationBehaviour.MaterialPlayer )
				return;

			endValueFloat = tweenMaterialAnimationBehaviour.MaterialPlayer.FloatEndValue;
			endValueColor = tweenMaterialAnimationBehaviour.MaterialPlayer.ColorEndValue;
			endValueV4 = tweenMaterialAnimationBehaviour.MaterialPlayer.VectorEndValue;
			duration = tweenMaterialAnimationBehaviour.MaterialPlayer.Duration;
			easeType = tweenMaterialAnimationBehaviour.MaterialPlayer.Ease;
			delay = tweenMaterialAnimationBehaviour.MaterialPlayer.Delay;
		}
	}

	public class TweenMaterialAnimationBehaviour : BehaviourBase {
		[SerializeField]
		private DoTweenMaterialPlayer m_materialPlayer;

		public DoTweenMaterialPlayer MaterialPlayer => m_materialPlayer;

		public override void Start() {
			m_data.ApplyToBehaviour(this);
			m_materialPlayer.Play();
		}

		public override void Exit() {
			m_materialPlayer.Stop();
		}

		public override bool Complete => m_materialPlayer.Complete;

		public override BehaviourDataBase CreateDefaultData() {
			var tweenData = new TweenMaterialAnimationBehaviourData();
			tweenData.CopySourceBehaviour(this);
			tweenData.TargetId = Id;
			return tweenData;
		}
	}
}
