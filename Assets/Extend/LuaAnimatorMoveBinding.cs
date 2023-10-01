using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	[RequireComponent(typeof(Animator))]
	public class LuaAnimatorMoveBinding : MonoBehaviour {
		private Animator m_animator;
		private LuaBinding m_binding;
		private NoneEventAction m_animatorMove;
		
		public void AssignLuaBinding(LuaBinding binding) {
			m_binding = binding;
			if( !m_binding ) {
				m_animatorMove = null;
				return;
			}

			m_animatorMove = m_binding.GetLuaMethod<NoneEventAction>("animator_move");
		}

		private void Start() {
			m_animator = GetComponent<Animator>();
			AssignLuaBinding(GetComponent<LuaBinding>());
		}

		private void OnAnimatorMove() {
			m_animatorMove?.Invoke(m_binding.LuaInstance, m_animator);
		}
	}
}