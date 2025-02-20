using System;
using UnityEngine;

namespace Extend.Switcher {
	[RequireComponent(typeof(StateSwitcher))]
	public class ColliderStateTrigger : MonoBehaviour {
		private StateSwitcher m_switcher;
		[SerializeField]
		private string m_triggerEnterStateName;
		[SerializeField]
		private string m_triggerExitStateName;

		private void Awake() {
			m_switcher = GetComponent<StateSwitcher>();
		}

		private void OnTriggerEnter(Collider other) {
			m_switcher.Switch(m_triggerEnterStateName);
		}

		private void OnTriggerExit(Collider other) {
			m_switcher.Switch(m_triggerExitStateName);
		}
	}
}