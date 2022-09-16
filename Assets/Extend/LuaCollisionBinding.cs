using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public class LuaCollisionBinding : MonoBehaviour {
		private LuaBinding m_binding;

		private LuaUnityCollisionEventFunction m_collisionEnter;
		private LuaUnityCollisionEventFunction m_collisionExit;

		private NoneEventAction m_triggerEnter;
		private NoneEventAction m_triggerExit;

		public void AssignLuaBinding(LuaBinding binding) {
			m_binding = binding;
		}

		private void Awake() {
			m_binding ??= GetComponent<LuaBinding>();
		}

		private void Start() {
			if (m_binding == null)
			{
				return;
			}
			m_collisionEnter = m_binding.GetLuaMethod<LuaUnityCollisionEventFunction>("collision_enter");
			m_collisionExit = m_binding.GetLuaMethod<LuaUnityCollisionEventFunction>("collision_exit");
			m_triggerEnter = m_binding.GetLuaMethod<NoneEventAction>("trigger_enter");
			m_triggerExit = m_binding.GetLuaMethod<NoneEventAction>("trigger_exit");
		}

		private void OnCollisionEnter(Collision other) {
			m_collisionEnter?.Invoke(m_binding.LuaInstance, other);
		}

		private void OnCollisionExit(Collision other) {
			m_collisionExit?.Invoke(m_binding.LuaInstance, other);
		}

		private void OnTriggerEnter(Collider other) {
			m_triggerEnter?.Invoke(m_binding.LuaInstance, other);
		}

		private void OnTriggerExit(Collider other) {
			m_triggerExit?.Invoke(m_binding.LuaInstance, other);
		}
	}
}