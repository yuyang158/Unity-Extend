using System;
using Extend.Common;
using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	[RequireComponent(typeof(LuaBinding))]
	public class LuaEnableBinding : MonoBehaviour {
		private LuaBinding m_binding;
		private LuaUnityEventFunction m_enable;
		private LuaUnityEventFunction m_disable;

		private void Awake() {
			m_binding = GetComponent<LuaBinding>();
		}

		private void Start() {
			m_enable = m_binding.GetLuaMethod<LuaUnityEventFunction>("enable");
			m_disable = m_binding.GetLuaMethod<LuaUnityEventFunction>("disable");
		}

		private void OnEnable() {
			m_enable?.Invoke(m_binding.LuaInstance);
		}

		private void OnDisable() {
			if( !CSharpServiceManager.Initialized ) {
				return;
			}
			m_disable?.Invoke(m_binding.LuaInstance);
		}
	}
}