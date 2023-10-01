using System;
using UnityEngine;

namespace Extend.Switcher.Action {
	public interface ISwitcherAction {
		void ActiveAction();

		void DeactiveAction();
	}

	[Serializable, UnityEngine.Scripting.Preserve]
	public abstract class SwitcherAction : ISwitcherAction {
		public abstract void ActiveAction();

		public abstract void DeactiveAction();

#if UNITY_EDITOR
		[SerializeField]
		private bool m_fold;
#endif
	}
}