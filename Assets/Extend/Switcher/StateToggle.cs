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

		protected override void Start() {
			base.Start();
			group = GetComponentInParent<ToggleGroup>();
		}

		protected override void DoStateTransition(SelectionState state, bool instant) {
			// base.DoStateTransition(state, instant);

			if( !gameObject.activeInHierarchy || !Application.isPlaying ) {
				return;
			}

			m_selectableSwitcher.CurrentState = state switch {
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