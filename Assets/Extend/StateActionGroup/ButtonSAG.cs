using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.StateActionGroup {
	[RequireComponent(typeof(SAG))]
	public class ButtonSAG : Button {
		private SAG m_sag;
		protected override void Awake() {
			base.Awake();
			m_sag = GetComponent<SAG>();
		}

		protected override void DoStateTransition(SelectionState state, bool instant) {
			// base.DoStateTransition(state, instant);
			
			if( !gameObject.activeInHierarchy || !Application.isPlaying ) {
				return;
			}

			var stateName = Enum.GetName(state.GetType(), state);
			m_sag.Switch(stateName == "Selected" ? "Normal" : stateName);
		}
	}
}