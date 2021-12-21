using System;
using Extend.Common;
using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	[RequireComponent(typeof(LuaBinding))]
	public class LuaEnableBinding : MonoBehaviour {
		private LuaBinding m_binding;

		private void Awake() {
			m_binding = GetComponent<LuaBinding>();
		}

		private void OnEnable() {
			var function = m_binding.GetLuaMethod<LuaUnityEventFunction>("enable");
			function?.Invoke(m_binding.LuaInstance);
		}

		private void OnDisable() {
			if( !CSharpServiceManager.Initialized ) {
				return;
			}
			var function = m_binding.GetLuaMethod<LuaUnityEventFunction>("disable");
			function?.Invoke(m_binding.LuaInstance);
		}
	}
}