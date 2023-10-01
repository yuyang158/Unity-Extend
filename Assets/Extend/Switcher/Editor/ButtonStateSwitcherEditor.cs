using UnityEditor;

namespace Extend.Switcher.Editor {
	[CustomEditor(typeof(ButtonStateSwitcher))]
	public class ButtonStateSwitcherEditor : StateSwitcherEditor {
		protected override void OnEnable() {
			m_canAddState = false;

			var buttonStateSwitcher = target as ButtonStateSwitcher;
			if( !( buttonStateSwitcher.States is {Length: 4} ) ) {
				buttonStateSwitcher.States = new StateSwitcher.State[4];
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
			}
			serializedObject.Update();
			base.OnEnable();
		}
	}
}