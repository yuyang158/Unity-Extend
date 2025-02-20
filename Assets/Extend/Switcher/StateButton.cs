using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Switcher {
	[RequireComponent(typeof(ButtonStateSwitcher))]
	public class StateButton : Button {
		private ButtonStateSwitcher m_switcher;
		
		protected override void Awake() {
			base.Awake();
			m_switcher = GetComponent<ButtonStateSwitcher>();
		}

		protected override void DoStateTransition(SelectionState state, bool instant) {
			// base.DoStateTransition(state, instant);

			if( !gameObject.activeInHierarchy || !Application.isPlaying ) {
				return;
			}

			// Debug.LogWarning($"{name} Current State : {state}");
			m_switcher.CurrentState = state switch {
				SelectionState.Normal => "Normal",
				SelectionState.Highlighted => "Highlighted",
				SelectionState.Pressed => "Pressed",
				SelectionState.Selected => "Selected",
				SelectionState.Disabled => "Disabled",
				_ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
			};
		}
	}
}