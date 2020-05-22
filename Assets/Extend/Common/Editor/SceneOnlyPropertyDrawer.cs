using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	[CustomPropertyDrawer(typeof(SceneOnlyAttribute))]
	public class SceneOnlyPropertyDrawer : AssetPopupPreviewDrawer {
		private Object m_object;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			m_object = property.objectReferenceValue;
			OnQuickInspectorGUI(ref position);
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(position, property, label);
			if( EditorGUI.EndChangeCheck() ) {
				if( !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(property.objectReferenceValue)) ) {
					property.objectReferenceValue = null;
				}
			}
		}

		protected override Object asset => m_object;
	}
}