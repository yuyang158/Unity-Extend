using UnityEditor;

namespace Extend.Switcher.Editor {
	[CustomEditor(typeof(ToggleStateSwitcher))]
	public class ToggleStateSwitcherEditor : StateSwitcherEditor {
		protected override void OnEnable() {
			m_canAddState = false;

			var toggleStateSwitcher = target as ButtonStateSwitcher;
			if( !( toggleStateSwitcher.States is {Length: 2} ) ) {
				toggleStateSwitcher.States = new StateSwitcher.State[2];
				toggleStateSwitcher.States[0] = new StateSwitcher.State() {
					StateName = "On"
				};
				toggleStateSwitcher.States[1] = new StateSwitcher.State() {
					StateName = "Off"
				};
			}
			serializedObject.Update();
			base.OnEnable();
		}
	}
}