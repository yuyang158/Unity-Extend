using UnityEditor;
using UnityEngine;

namespace Extend.Animation.Editor {
	[CustomEditor(typeof(SequencePlayerBase), true)]
	public class SequencePlayerBaseEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			var playerBase = target as SequencePlayerBase;
			GUI.enabled = false;
			EditorGUILayout.TextField("Playing", playerBase.AnimationName);
			GUI.enabled = true;
		}
	}
}
