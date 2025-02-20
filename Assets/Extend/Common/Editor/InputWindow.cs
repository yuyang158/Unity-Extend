using System;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	public class InputWindow : EditorWindow {
		public static InputWindow CreateWindow(string title, bool closeOnOK = true, bool singleLine = true) {
			var window = GetWindow<InputWindow>();
			window.titleContent = new GUIContent(title);
			window.minSize = new Vector2(250, 80);
			if( singleLine ) {
				window.maxSize = window.minSize;
			}
			else {
				window.maxSize = new Vector2(250, 380);
			}
			window.SingleLine = singleLine;
			window.CloseOnOK = closeOnOK;
			return window;
		}

		private string m_text;
		public bool CloseOnOK;
		public bool SingleLine;
		public Action<string> Callback;

		private void OnGUI() {
			EditorGUILayout.LabelField("Input Text:");
			EditorGUILayout.Space();
			m_text = SingleLine ? EditorGUILayout.TextField(m_text) :
				EditorGUILayout.TextArea(m_text, GUILayout.Height(this.position.height - 50));

			var rect = new Rect(0, 0, position.width, position.height) {
				y = position.height - UIEditorUtil.LINE_HEIGHT,
				height = EditorGUIUtility.singleLineHeight
			};

			var okRect = UIEditorUtil.CalcMultiColumnRect(rect, 0, 2);
			if( GUI.Button(okRect, "OK") ) {
				Callback?.Invoke(m_text);
				if(CloseOnOK)
					Close();
			}

			var cancelRect = UIEditorUtil.CalcMultiColumnRect(rect, 1, 2);
			if( GUI.Button(cancelRect, "Cancel") ) {
				Close();
			}
		}
	}
}