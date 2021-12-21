using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	public class LuaCollisionBinding : MonoBehaviour {
		private LuaBinding m_binding;
		private void Awake() {
			m_binding = GetComponent<LuaBinding>();
		}

		private void OnCollisionEnter(Collision other) {
			var function = m_binding.GetLuaMethod<LuaUnityCollisionEventFunction>("collision_enter");
			function?.Invoke(m_binding.LuaInstance, other);
		}

		private void OnCollisionExit(Collision other) {
			var function = m_binding.GetLuaMethod<LuaUnityCollisionEventFunction>("collision_exit");
			function?.Invoke(m_binding.LuaInstance, other);
		}

		private void OnTriggerEnter(Collider other) {
			var function = m_binding.GetLuaMethod<NoneEventAction>("trigger_enter");
			function?.Invoke(m_binding.LuaInstance, other);
		}

		private void OnTriggerExit(Collider other) {
			var function = m_binding.GetLuaMethod<NoneEventAction>("trigger_exit");
			function?.Invoke(m_binding.LuaInstance, other);
		}
	}
}