using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.UI.Editor {
	public class UIAnimationInlineParam {
		private class Param {
			public string FieldName;
			public float WidthInPercent;
			public GUIContent Label;
		}

		private readonly List<Param> m_parameters = new List<Param>();

		public UIAnimationInlineParam Add(string fieldName, float percent, string displayName = "") {
			m_parameters.Add(new Param {
				FieldName = fieldName,
				WidthInPercent = percent,
				Label = new GUIContent(displayName)
			});
			return this;
		}

		public void OnGUI(Rect position, SerializedProperty property) {
			Assert.IsTrue(m_parameters.Count > 0);
			if( m_parameters.Count == 1 ) {
				DrawGUI(position, m_parameters[0], property);
			}
			else {
				var originLabelWidth = EditorGUIUtility.labelWidth;
				for( var i = 0; i < m_parameters.Count; i++ ) {
					var parameter = m_parameters[i];
					var rect = position;
					rect.width *= parameter.WidthInPercent;
					if( i < m_parameters.Count - 1 )
						rect.xMax -= 5;
					EditorGUIUtility.labelWidth = rect.width * 0.5f;
					DrawGUI(rect, parameter, property);
					position.x = rect.xMax + 5;
				}

				EditorGUIUtility.labelWidth = originLabelWidth;
			}
		}

		private static void DrawGUI(Rect position, Param parameter, SerializedProperty property) {
			var p = property.FindPropertyRelative(parameter.FieldName);
			if( p == null ) {
				Debug.LogError($"field with name : {parameter.FieldName} not exist");
				return;
			}
			if( string.IsNullOrEmpty(parameter.Label.text) ) {
				EditorGUI.PropertyField(position, p);
			}
			else {
				EditorGUI.PropertyField(position, p, parameter.Label);
			}
		}
	}

	public class UIAnimationParamCombine {
		private readonly UIAnimationInlineParam[] m_params;
		private readonly int m_modeIndex;

		public UIAnimationParamCombine(int rowCount, int modeIndex) {
			m_params = new UIAnimationInlineParam[rowCount];
			for( var i = 0; i < rowCount; i++ ) {
				m_params[i] = new UIAnimationInlineParam();
			}

			m_modeIndex = modeIndex;
		}

		public UIAnimationInlineParam GetRow(int index) {
			Assert.IsTrue(index >= 0 && index < m_params.Length);
			return m_params[index];
		}

		public Rect OnGUI(Rect position, SerializedProperty property) {
			var bgColor = GUI.backgroundColor;
			GUI.backgroundColor = UIEditorUtil.UI_ANIMATION_COLORS[m_modeIndex];
			var bgRect = position;
			bgRect.height = UIEditorUtil.LINE_HEIGHT * m_params.Length;
			bgRect.x = 0;
			bgRect.xMax = EditorGUIUtility.currentViewWidth;
			EditorGUI.DrawRect(bgRect, GUI.backgroundColor);
			foreach( var inlineParam in m_params ) {
				inlineParam.OnGUI(position, property);
				position.y += UIEditorUtil.LINE_HEIGHT;
			}

			GUI.backgroundColor = bgColor;
			return position;
		}
	}
}