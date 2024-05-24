using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.StateActionGroup {
	public class ToggleSAG : Toggle {
		[SerializeField]
		private SAG m_stateSAG;

		[SerializeField]
		private SAG m_onOffSAG;

		protected override void Awake() {
			base.Awake();
			if( !Application.isPlaying )
				return;

			onValueChanged.AddListener(check => { m_onOffSAG.Switch(check ? "On" : "Off"); });
		}

		protected override void OnEnable() {
			base.OnEnable();
			if( !transform.parent ) {
				group = null;
			}
			else {
				var parentGroup = transform.parent.GetComponent<ToggleGroup>();
				group = parentGroup;
			}
		}

		protected override void Start() {
			base.Start();
			if(!m_onOffSAG) return;
			m_onOffSAG.Switch(isOn ? "On" : "Off");
			if( !group ) {
				var parentGroup = transform.parent.GetComponent<ToggleGroup>();
				group = parentGroup;
			}
		}

		protected override void DoStateTransition(SelectionState state, bool instant) {
			if( !gameObject.activeInHierarchy || !Application.isPlaying || !m_stateSAG ) {
				return;
			}

			var stateName = Enum.GetName(state.GetType(), state);
			m_stateSAG.Switch(stateName == "Selected" ? "Normal" : stateName);
		}
	}
}