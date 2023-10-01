using System;
using Extend.Common;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class AnimatorSwitcherAction : SwitcherAction {
		[SerializeField]
		private AnimatorParamProcessor m_processor;

		public override void ActiveAction() {
			m_processor.Apply();
		}

		public override void DeactiveAction() {
		}
	}
}