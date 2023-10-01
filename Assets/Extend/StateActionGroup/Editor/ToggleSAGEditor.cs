using UnityEditor;

namespace Extend.StateActionGroup.Editor {
	[CustomEditor(typeof(ToggleSAG))]
	public class ToggleSAGEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			var stateSAGProp = serializedObject.FindProperty("m_stateSAG");
			EditorGUILayout.PropertyField(stateSAGProp);
			var onOffSAGProp = serializedObject.FindProperty("m_onOffSAG");
			EditorGUILayout.PropertyField(onOffSAGProp);

			serializedObject.ApplyModifiedProperties();
		}
	}
}