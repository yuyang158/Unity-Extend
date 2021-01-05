using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Switcher.Action {
	[Serializable]
	public class GraphicMaterialSwitcherAction : SwitcherAction {
		[SerializeField]
		private Graphic m_graphic;

		[SerializeField]
		private Material m_material;
		public override void ActiveAction() {
			m_graphic.material = m_material;
		}
	}
}