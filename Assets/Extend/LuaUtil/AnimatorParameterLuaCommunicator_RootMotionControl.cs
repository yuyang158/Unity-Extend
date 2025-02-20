using UnityEngine;

namespace Extend.LuaUtil {
	public class AnimatorParameterLuaCommunicator_RootMotionControl : AnimatorParameterLuaCommunicator {
		private OnRootMotionUpdate m_rootMotionUpdate;

		[SerializeField]
		private bool m_rootMotionActive;
		public override bool RootMotionCommunicator => true;

		private void OnAnimatorMove() {
			if(!m_rootMotionActive) return;
			m_rootMotionUpdate?.Invoke(Animator.deltaPosition, Animator.deltaRotation);
		}

		public void ManualAnimatorMove(float offset) {
			if(m_rootMotionActive) return;
			m_rootMotionUpdate?.Invoke(offset * transform.forward, Quaternion.identity);
		}

		public override void SetRootMotionActivate(bool activate, OnRootMotionUpdate rootMotionUpdate = null) {
			m_rootMotionActive = activate;
			Animator.applyRootMotion = activate;
			m_rootMotionUpdate = rootMotionUpdate;
		}
	}
}