using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class GraphicMaterialSwitcherAction : SwitcherAction {
		[SerializeField]
		private Graphic m_graphic;

		[SerializeField]
		private Material m_material;
		public override void ActiveAction() {
			m_graphic.material = m_material;
		}

        public override void DeactiveAction()
        {
            m_graphic.material = null;
        }
    }
}