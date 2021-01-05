using System;
using UnityEngine;

namespace Extend.Switcher.Action {
	public interface ISwitcherAction {
		void ActiveAction();
	}

	[Serializable]
	public abstract class SwitcherAction : ISwitcherAction {
		public abstract void ActiveAction();

#if UNITY_EDITOR
		[SerializeField]
		private bool m_fold;
#endif
	}
}