using Extend.Common.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.Tween.Editor {
	[CustomEditor(typeof(RendererTweenContainer))]
	public class RendererTweenContainerEditor : UnityEditor.Editor {
		private ReorderableList m_reList;

		private void OnEnable() {
			var valuesProperty = serializedObject.FindProperty("m_values");
			m_reList = new ReorderableList(serializedObject, valuesProperty) {
				elementHeightCallback = index => {
					var rendererProperty = serializedObject.FindProperty("m_renderer");
					if( !rendererProperty.objectReferenceValue ) {
						return 0;
					}
					var valueProperty = valuesProperty.GetArrayElementAtIndex(index);
					var editorFoldProperty = valueProperty.FindPropertyRelative("m_editorFold");
					if( !editorFoldProperty.boolValue ) {
						return UIEditorUtil.LINE_HEIGHT;
					}
					return UIEditorUtil.LINE_HEIGHT * 4;
				},
				drawElementCallback = (rect, index, isActive, _) => {
					var rendererProperty = serializedObject.FindProperty("m_renderer");
					if( !rendererProperty.objectReferenceValue ) {
						return;
					}

					rect.height = EditorGUIUtility.singleLineHeight;
					var valueProperty = valuesProperty.GetArrayElementAtIndex(index);
					var materialPropertyName = valueProperty.FindPropertyRelative("m_materialPropertyName");
					var foldRect = rect;
					foldRect.xMin += 10;

					var editorFoldProperty = valueProperty.FindPropertyRelative("m_editorFold");
					editorFoldProperty.boolValue = EditorGUI.Foldout(foldRect, editorFoldProperty.boolValue, materialPropertyName.stringValue);
					if( !editorFoldProperty.boolValue ) {
						return;
					}
					rect.y += UIEditorUtil.LINE_HEIGHT;
					var leftPartRect = rect;
					leftPartRect.width *= 0.5f;
					EditorGUIUtility.labelWidth = leftPartRect.width * 0.5f;
					EditorGUI.PropertyField(leftPartRect, valueProperty.FindPropertyRelative("m_startValue"));
					var rightPartRect = rect;
					rightPartRect.xMin = leftPartRect.xMax;
					EditorGUIUtility.labelWidth = rightPartRect.width * 0.5f;
					EditorGUI.PropertyField(rightPartRect, valueProperty.FindPropertyRelative("m_endValue"));
					rect.y += UIEditorUtil.LINE_HEIGHT;

					var part1Rect = UIEditorUtil.CalcMultiColumnRect(rect, 0, 3);
					EditorGUIUtility.labelWidth = part1Rect.width * 0.5f;
					EditorGUI.PropertyField(part1Rect, valueProperty.FindPropertyRelative("m_ease"));

					var part2Rect = UIEditorUtil.CalcMultiColumnRect(rect, 1, 3);
					EditorGUI.PropertyField(part2Rect, valueProperty.FindPropertyRelative("m_delay"));

					var part3Rect = UIEditorUtil.CalcMultiColumnRect(rect, 2, 3);
					EditorGUI.PropertyField(part3Rect, valueProperty.FindPropertyRelative("m_duration"));

					rect.y += UIEditorUtil.LINE_HEIGHT;
					part1Rect = UIEditorUtil.CalcMultiColumnRect(rect, 0, 2);
					EditorGUIUtility.labelWidth = part1Rect.width * 0.5f;
					EditorGUI.PropertyField(part1Rect, valueProperty.FindPropertyRelative("m_loop"));
					part2Rect = UIEditorUtil.CalcMultiColumnRect(rect, 1, 2);
					EditorGUI.PropertyField(part2Rect, valueProperty.FindPropertyRelative("m_loopType"));
				},
				onAddDropdownCallback = (buttonRect, list) => {
					var rendererProperty = serializedObject.FindProperty("m_renderer");
					var renderer = rendererProperty.objectReferenceValue as Renderer;
					if( !renderer ) {
						return;
					}

					if( !renderer.sharedMaterial ) {
						return;
					}

					var shader = renderer.sharedMaterial.shader;
					var propertyCount = ShaderUtil.GetPropertyCount(shader);
					var menu = new GenericMenu();
					for( int i = 0; i < propertyCount; i++ ) {
						var flag = shader.GetPropertyFlags(i);
						if( flag == ShaderPropertyFlags.HideInInspector ) {
							continue;
						}
						var propertyType = ShaderUtil.GetPropertyType(shader, i);
						if( propertyType == ShaderUtil.ShaderPropertyType.Color ) {
							var nameContent = new GUIContent(ShaderUtil.GetPropertyDescription(shader, i));
							menu.AddItem(nameContent, false, AddTweenValue, i);
						}
						else if( propertyType == ShaderUtil.ShaderPropertyType.Vector ) {
							var nameContent = new GUIContent(ShaderUtil.GetPropertyDescription(shader, i));
							menu.AddItem(nameContent, false, AddTweenValue, i);
						}
					}
					menu.ShowAsContext();
				} 
			};
		}

		private void AddTweenValue(object state) {
			int propertyIndex = (int)state;
			var material = GetMaterial();
			if( !material ) {
				return;
			}
			var propertyName = ShaderUtil.GetPropertyName(material.shader, propertyIndex);
			var container = target as RendererTweenContainer;
			var propertyType = ShaderUtil.GetPropertyType(material.shader, propertyIndex);
			ITweenValue[] values = container.Values;
			if( propertyType == ShaderUtil.ShaderPropertyType.Color ) {
				var color = material.GetColor(propertyName);
				ArrayUtility.Add(ref values, new MaterialColorTweenValue {
					PropertyName = propertyName,
					StartValue = color,
					EndValue = color
				});
			}
			else if( propertyType == ShaderUtil.ShaderPropertyType.Vector ) {
				var vector = material.GetVector(propertyName);
				ArrayUtility.Add(ref values, new MaterialVector4TweenValue {
					PropertyName = propertyName,
					StartValue = vector,
					EndValue = vector
				});
			}
			else if( propertyType == ShaderUtil.ShaderPropertyType.Float ) {
				var f = material.GetFloat(propertyName);
				ArrayUtility.Add(ref values, new MaterialFloatTweenValue {
					PropertyName = propertyName,
					StartValue = f,
					EndValue = f
				});
			}

			container.Values = values;
			serializedObject.Update();
		}

		private Material GetMaterial() {
			var rendererProperty = serializedObject.FindProperty("m_renderer");
			var renderer = rendererProperty.objectReferenceValue as Renderer;
			if( !renderer ) {
				return null;
			}

			return renderer.sharedMaterial;
		}

		private Shader GetShader() {
			var rendererProperty = serializedObject.FindProperty("m_renderer");
			var renderer = rendererProperty.objectReferenceValue as Renderer;
			if( !renderer ) {
				return null;
			}

			return !renderer.sharedMaterial ? null : renderer.sharedMaterial.shader;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUI.BeginChangeCheck();
			m_reList.DoLayoutList();
			if( EditorGUI.EndChangeCheck() ) {
				serializedObject.ApplyModifiedProperties();
			}

			if( Application.isPlaying ) {
				if( GUILayout.Button("Play") ) {
					var container = target as RendererTweenContainer;
					container.Play();
				}
			}
		}
	}
}