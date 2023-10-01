using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Switcher {
	[RequireComponent(typeof(ToggleStateSwitcher), typeof(ButtonStateSwitcher))]
	public class StateToggle : Toggle {
		private ButtonStateSwitcher m_selectableSwitcher;
		private ToggleStateSwitcher m_toggleSwitcher;
		
		protected override void Awake() {
			base.Awake();
			m_selectableSwitcher = GetComponent<ButtonStateSwitcher>();
			m_toggleSwitcher = GetComponent<ToggleStateSwitcher>();
			onValueChanged.AddListener(_ => {
				if(!Application.isPlaying)
					return;
				m_toggleSwitcher.CurrentState = isOn ? "On" : "Off";
			});
		}
		
		protected override void DoStateTransition(SelectionState state, bool instant) {
			// base.DoStateTransition(state, instant);

			if( !gameObject.activeInHierarchy || !Application.isPlaying ) {
				return;
			}

			switch( state ) {
				case SelectionState.Normal:
					m_selectableSwitcher.CurrentState = "Normal";
					break;
				case SelectionState.Highlighted:
					m_selectableSwitcher.CurrentState = "Highlighted";
					break;
				case SelectionState.Pressed:
					m_selectableSwitcher.CurrentState = "Pressed";
					break;
				case SelectionState.Selected:
					break;
				case SelectionState.Disabled:
					m_selectableSwitcher.CurrentState = "Disabled";
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
	}
}