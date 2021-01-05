using System;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	public class InputWindow : EditorWindow {
		public static InputWindow CreateWindow(string title) {
			var window = GetWindow<InputWindow>();
			window.titleContent = new GUIContent(title);
			window.minSize = new Vector2(250, 80);
			window.maxSize = window.minSize;
			return window;
		}

		private string m_text;
		public Action<string> Callback;

		private void OnGUI() {
			EditorGUILayout.LabelField("Input Text:");
			EditorGUILayout.Space();
			m_text = EditorGUILayout.TextField(m_text);

			var rect = new Rect(0, 0, position.width, position.height);
			rect.y += UIEditorUtil.LINE_HEIGHT * 3;
			rect.height = EditorGUIUtility.singleLineHeight;

			var okRect = UIEditorUtil.CalcMultiColumnRect(rect, 0, 2);
			if( GUI.Button(okRect, "OK") ) {
				Callback?.Invoke(m_text);
				Close();
			}

			var cancelRect = UIEditorUtil.CalcMultiColumnRect(rect, 1, 2);
			if( GUI.Button(cancelRect, "Cancel") ) {
				Close();
			}
		}
	}
}