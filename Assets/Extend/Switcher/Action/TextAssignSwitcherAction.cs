using System;
using TMPro;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class TextAssignSwitcherAction : SwitcherAction {
		[SerializeField]
		private TextMeshProUGUI m_textGUI;

		[SerializeField]
		private string m_text;
		
		public override void ActiveAction() {
			m_textGUI.text = m_text;
		}

		public override void DeactiveAction() {
		}
	}
}