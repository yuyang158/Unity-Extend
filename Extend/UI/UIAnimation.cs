using System;
using Extend.UI.Animation;
using UnityEngine;

namespace UI.Animation {
	[Serializable]
	public class UIAnimation {
		public enum AnimationMode {
			PUNCH,
			STATE,
			ANIMATOR
		}

		public AnimationMode Mode;
		[SerializeField, HideInInspector]
		private PunchCombined punch;

		[SerializeField, HideInInspector]
		private Animator animator;

		private AnimatorControllerParameter param;
	}
}