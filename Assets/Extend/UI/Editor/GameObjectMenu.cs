using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI.Editor {
	public static class GameObjectMenu {
		private const string kStandardSpritePath       = "UI/Skin/UISprite.psd";
		private const string kBackgroundSpritePath     = "UI/Skin/Background.psd";
		private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
		private const string kKnobPath                 = "UI/Skin/Knob.psd";
		private const string kCheckmarkPath            = "UI/Skin/Checkmark.psd";
		private const string kDropdownArrowPath        = "UI/Skin/DropdownArrow.psd";
		private const string kMaskPath                 = "UI/Skin/UIMask.psd";

		static private DefaultControls.Resources s_StandardResources;

		static private DefaultControls.Resources GetStandardResources()
		{
			if (s_StandardResources.standard == null)
			{
				s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
				s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
				s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
				s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
				s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
				s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
				s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
			}
			return s_StandardResources;
		}
		
		[MenuItem("GameObject/Extend UI/UI Button", false, 0)]
		static void CreateUIButton() {
			var active = Selection.activeObject as GameObject;
			if(!active)
				return;

			var root = active.transform;
			var go = DefaultControls.CreateButton(GetStandardResources());
			go.AddComponent<UIButton>();
			go.transform.SetParent(root);
		}
	}
}