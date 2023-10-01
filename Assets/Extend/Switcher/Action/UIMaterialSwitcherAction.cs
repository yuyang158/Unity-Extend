using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class UIMaterialSwitcherAction : SwitcherAction {
		[SerializeField]
		private Graphic m_graphic;

		[SerializeField]
		private Material m_material;

		private Material m_originMaterial;
		public override void ActiveAction() {
			m_originMaterial = m_graphic.material;
			m_graphic.material = m_material;
		}

		public override void DeactiveAction() {
			m_graphic.material = m_originMaterial;
		}
	}
}