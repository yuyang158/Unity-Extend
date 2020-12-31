using Extend.Common;
using Extend.UI.Attributes;
using Extend.UI.i18n;
using TMPro;
using UnityEngine;

namespace Extend.Switcher.Action {
	public class TextAssignSwitcherAction : ISwitcherAction {
		[SerializeField]
		private TextMeshProUGUI m_text;

		[StaticI18nKey, SerializeField]
		private string m_key;
		
		public void ActiveSwitcher() {
			var i18NService = CSharpServiceManager.Get<I18nService>(CSharpServiceManager.ServiceType.I18N);
			m_text.text = i18NService.GetText(m_key);
		}

#if UNITY_EDITOR
		public void OnEditorGUI(Rect rect, UnityEditor.SerializedProperty property) {
			rect.height = UnityEditor.EditorGUIUtility.singleLineHeight;
			var textProperty = property.FindPropertyRelative("m_text");
			UnityEditor.EditorGUI.PropertyField(rect, textProperty);

			rect.y += Common.Editor.UIEditorUtil.LINE_HEIGHT;
			var keyProp = property.FindPropertyRelative("m_key");
			UnityEditor.EditorGUI.PropertyField(rect, keyProp);
		}

		public float GetEditorHeight(UnityEditor.SerializedProperty property) {
			return Common.Editor.UIEditorUtil.LINE_HEIGHT * 6;
		}
#endif
	}
}