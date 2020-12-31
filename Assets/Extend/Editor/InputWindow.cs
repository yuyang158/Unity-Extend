using System;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public class InputWindow : EditorWindow {
		public static InputWindow CreateWindow(string title) {
			var window = GetWindow<InputWindow>();
			window.titleContent = new GUIContent(title);
			window.position = new Rect((float)Screen.width / 2, (float)Screen.height / 2, 250, 80);
			return window;
		}

		private string m_text;
		public Action<string> Callback;
		private void OnGUI() {
			EditorGUILayout.LabelField("Input Text:");
			m_text = EditorGUILayout.TextField(m_text);

			if( GUILayout.Button("OK") ) {
				Callback?.Invoke(m_text);
				Close();
			}

			if( GUILayout.Button("Cancel") ) {
				Close();
			}
		}
	}
}