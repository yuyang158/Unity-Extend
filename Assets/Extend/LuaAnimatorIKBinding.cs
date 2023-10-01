using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	[RequireComponent(typeof(Animator))]
	public class LuaAnimatorIKBinding : MonoBehaviour {
		private Animator m_animator;
		private LuaBinding m_binding;
		private IntEventAction m_animatorIK;
		
		public void AssignLuaBinding(LuaBinding binding) {
			m_binding = binding;
			if( !m_binding ) {
				m_animatorIK = null;
				return;
			}

			m_animatorIK = m_binding.GetLuaMethod<IntEventAction>("animator_ik");
		}

		private void Start() {
			m_animator = GetComponent<Animator>();
			AssignLuaBinding(GetComponent<LuaBinding>());
		}

		private void OnAnimatorIK(int layerIndex) {
			m_animatorIK?.Invoke(m_binding.LuaInstance, m_animator, layerIndex);
		}
	}
}