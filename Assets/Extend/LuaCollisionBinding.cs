using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend {
	[LuaCallCSharp, DisallowMultipleComponent]
	public class LuaCollisionBinding : MonoBehaviour {
		private LuaBinding m_binding;

		private LuaUnityCollisionEventFunction m_collisionEnter;
		private LuaUnityCollisionEventFunction m_collisionExit;

		private NoneEventAction m_triggerEnter;
		private NoneEventAction m_triggerExit;

		public void AssignLuaBinding(LuaBinding binding) {
			m_binding = binding;
			if( !m_binding ) {
				m_collisionEnter = null;
				m_collisionExit = null;
				m_triggerEnter = null;
				m_triggerExit = null;
				return;
			}

			m_collisionEnter = m_binding.GetLuaMethod<LuaUnityCollisionEventFunction>("collision_enter");
			m_collisionExit = m_binding.GetLuaMethod<LuaUnityCollisionEventFunction>("collision_exit");
			m_triggerEnter = m_binding.GetLuaMethod<NoneEventAction>("trigger_enter");
			m_triggerExit = m_binding.GetLuaMethod<NoneEventAction>("trigger_exit");
		}

		private void Start() {
			m_binding ??= GetComponentInParent<LuaBinding>();
			if( m_binding ) {
				AssignLuaBinding(m_binding);
			}
		}

		public void OnCollisionEnter(Collision other) {
			m_collisionEnter?.Invoke(m_binding.LuaInstance, other);
		}

		public void OnCollisionExit(Collision other) {
			m_collisionExit?.Invoke(m_binding.LuaInstance, other);
		}

		public void OnTriggerEnter(Collider other) {
			m_triggerEnter?.Invoke(m_binding.LuaInstance, other);
		}

		public void OnTriggerExit(Collider other) {
			m_triggerExit?.Invoke(m_binding.LuaInstance, other);
		}

		public LuaBinding Binding => m_binding;
	}
}