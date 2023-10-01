namespace Extend.LuaUtil {
	public class AnimatorParameterLuaCommunicator_RootMotionControl : AnimatorParameterLuaCommunicator {
		private OnRootMotionUpdate m_rootMotionUpdate;

		private void OnAnimatorMove() {
			m_rootMotionUpdate?.Invoke(Animator.deltaPosition, Animator.deltaRotation);
		}

		public void SetRootMotionActivate(bool activate, OnRootMotionUpdate rootMotionUpdate = null) {
			Animator.applyRootMotion = activate;
			m_rootMotionUpdate = rootMotionUpdate;
		}
	}
}