using System;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class GOActiveSwitcherAction : SwitcherAction {
		[SerializeField]
		private GameObject m_go;
		[SerializeField]
		private bool m_active;
		
		public override void ActiveAction() {
			m_go.SetActive(m_active);
		}
	}
}