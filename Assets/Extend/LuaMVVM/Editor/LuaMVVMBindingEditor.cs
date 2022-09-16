using System.Linq;
using Extend.Common;
using Extend.LuaBindingEvent;
using Extend.UI.Scroll;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaMVVM.Editor {
	[CustomEditor(typeof(LuaMVVMBinding))]
	public class LuaMVVMBindingEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if( GUILayout.Button("Generate Binding & Button Event") ) {
				// Button
				var binding = target as LuaMVVMBinding;
				var buttons = binding.GetComponentsInChildren<Button>(true);
				foreach( Button button in buttons ) {
					button.GetOrAddComponent<LuaBindingClickEvent>();
				}
				
				// Dropdown
				FindBinding<TMP_Dropdown, LuaMVVMDropdown>(binding, "LuaArrayData");
				// ScrollRect
				FindBinding<ScrollRect, LuaMVVMSystemScroll>(binding, "LuaArrayData");
				// LoopRect
				FindBinding<LoopScrollRect, LuaMVVMLoopScroll>(binding, "LuaArrayData");

				serializedObject.UpdateIfRequiredOrScript();
			}
		}

		private static void FindBinding<ComponentT, MvvmT>(LuaMVVMBinding binding, string propertyName) 
			where ComponentT : Component 
			where MvvmT : Component {
			var dropdowns = binding.GetComponentsInChildren<ComponentT>();
			foreach( ComponentT dropdown in dropdowns ) {
				var mvvmDropdown = dropdown.GetOrAddComponent<MvvmT>();
				var exist = binding.BindingOptions.Options.Any(option => option.BindTarget == mvvmDropdown);
				if( !exist ) {
					ArrayUtility.Add(ref binding.BindingOptions.Options, new LuaMVVMBindingOption() {
						BindTarget = mvvmDropdown,
						Mode = LuaMVVMBindingOption.BindMode.ONE_TIME,
						BindTargetProp = propertyName
					});
				}
			}
		}
	}
}