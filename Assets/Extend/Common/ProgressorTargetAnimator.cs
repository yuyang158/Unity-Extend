using UnityEngine;

namespace Extend.Common {
	[RequireComponent(typeof(Animator))]
	public sealed class ProgressorTargetAnimator : ProgressorTargetBase {
		[SerializeField]
		private string m_parameterName;

		private int m_hash;
		private Animator m_animator;

		private void Awake() {
			m_hash = Animator.StringToHash(m_parameterName);
			m_animator = GetComponent<Animator>();
		}

		public override void ApplyProgress(float value) {
			m_animator.SetFloat(m_hash, value);
		}
	}
}