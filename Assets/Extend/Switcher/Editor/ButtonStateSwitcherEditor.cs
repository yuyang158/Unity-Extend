using UnityEditor;

namespace Extend.Switcher.Editor {
	[CustomEditor(typeof(ButtonStateSwitcher))]
	public class ButtonStateSwitcherEditor : StateSwitcherEditor {
		protected override void OnEnable() {
			m_canAddState = false;

			var buttonStateSwitcher = target as ButtonStateSwitcher;
			if( !( buttonStateSwitcher.States is {Length: 5} ) ) {
				buttonStateSwitcher.States = new StateSwitcher.State[5];
				buttonStateSwitcher.States[0] = new StateSwitcher.State() {
					StateName = "Normal"
				};
				buttonStateSwitcher.States[1] = new StateSwitcher.State() {
					StateName = "Pressed"
				};
				buttonStateSwitcher.States[2] = new StateSwitcher.State() {
					StateName = "Disabled"
				};
				buttonStateSwitcher.States[3] = new StateSwitcher.State() {
					StateName = "Highlighted"
				};
				buttonStateSwitcher.States[4] = new StateSwitcher.State() {
					StateName = "Selected"
				};
			}
			serializedObject.Update();
			base.OnEnable();
		}
	}
}