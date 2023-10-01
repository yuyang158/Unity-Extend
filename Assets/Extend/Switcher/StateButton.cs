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

			Debug.LogWarning($"{name} Current State : {state}");
			switch( state ) {
				case SelectionState.Normal:
					m_switcher.CurrentState = "Normal";
					break;
				case SelectionState.Highlighted:
					m_switcher.CurrentState = "Highlighted";
					break;
				case SelectionState.Pressed:
					m_switcher.CurrentState = "Pressed";
					break;
				case SelectionState.Selected:
					m_switcher.CurrentState = "Normal";
					break;
				case SelectionState.Disabled:
					m_switcher.CurrentState = "Disabled";
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
	}
}