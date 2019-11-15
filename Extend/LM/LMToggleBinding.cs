using UnityEngine;
using UnityEngine.UI;

namespace Extend.LM {
	[RequireComponent( typeof(Toggle) )]
	public class LMToggleBinding : LMBooleanBinding {
		private Toggle toggle;

		private void Awake() {
			if( !toggle ) {
				toggle = GetComponent<Toggle>();
			}
		}

		protected override void ChangeBoolean(bool b) {
			toggle.isOn = b;
		}
	}
}