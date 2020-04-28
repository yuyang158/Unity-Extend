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

		private readonly List<Param> parameters = new List<Param>();

		public UIAnimationInlineParam Add(string fieldName, float percent, string displayName = "") {
			parameters.Add(new Param {
				FieldName = fieldName,
				WidthInPercent = percent,
				Label = new GUIContent(displayName)
			});
			return this;
		}

		public void OnGUI(Rect position, SerializedProperty property) {
			Assert.IsTrue(parameters.Count > 0);
			if( parameters.Count == 1 ) {
				DrawGUI(position, parameters[0], property);
			}
			else {
				var originLabelWidth = EditorGUIUtility.labelWidth;
				for( var i = 0; i < parameters.Count; i++ ) {
					var parameter = parameters[i];
					var rect = position;
					rect.width *= parameter.WidthInPercent;
					if( i < parameters.Count - 1 )
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
			if( string.IsNullOrEmpty(parameter.Label.text) ) {
				EditorGUI.PropertyField(position, p);
			}
			else {
				EditorGUI.PropertyField(position, p, parameter.Label);
			}
		}
	}

	public class UIAnimationParamCombine {
		private static readonly float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		private readonly UIAnimationInlineParam[] _params;
		private readonly int _modeIndex;

		public UIAnimationParamCombine(int rowCount, int modeIndex) {
			_params = new UIAnimationInlineParam[rowCount];
			for( var i = 0; i < rowCount; i++ ) {
				_params[i] = new UIAnimationInlineParam();
			}

			_modeIndex = modeIndex;
		}

		public UIAnimationInlineParam GetRow(int index) {
			Assert.IsTrue(index >= 0 && index < _params.Length);
			return _params[index];
		}

		public Rect OnGUI(Rect position, SerializedProperty property) {
			var bgColor = GUI.backgroundColor;
			GUI.backgroundColor = UIEditorUtil.UI_ANIMATION_COLORS[_modeIndex];
			var bgRect = position;
			bgRect.height = lineHeight * _params.Length;
			bgRect.x = 0;
			bgRect.xMax = EditorGUIUtility.currentViewWidth;
			EditorGUI.DrawRect(bgRect, GUI.backgroundColor);
			foreach( var inlineParam in _params ) {
				inlineParam.OnGUI(position, property);
				position.y += lineHeight;
			}

			GUI.backgroundColor = bgColor;
			return position;
		}
	}
}