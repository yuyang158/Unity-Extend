using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.DoTween.Editor {
	[CustomEditor(typeof(DoTweenMaterialPlayer))]
	public class DoTweenMaterialPlayerEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			serializedObject.UpdateIfRequiredOrScript();
			var targetRendererProp = serializedObject.FindProperty("m_targetRenderer");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(targetRendererProp);

			if( EditorGUI.EndChangeCheck() ) {
				serializedObject.ApplyModifiedProperties();
			}

			var materialPlayer = target as DoTweenMaterialPlayer;
			if( !materialPlayer.TargetRenderer ) {
				return;
			}
			if( !materialPlayer.TargetRenderer.sharedMaterial ) {
				return;
			}

			EditorGUI.BeginChangeCheck();
			var material = materialPlayer.TargetRenderer.sharedMaterial;
			var propCount = material.shader.GetPropertyCount();
			var shaderPropNames = new string[propCount];
			var shaderPropIds = new int[propCount];
			var shaderPropTypes = new ShaderPropertyType[propCount];
			for( int i = 0; i < propCount; i++ ) {
				shaderPropTypes[i] = material.shader.GetPropertyType(i);
				shaderPropNames[i] = material.shader.GetPropertyDescription(i);
				shaderPropIds[i] = material.shader.GetPropertyNameId(i);
			}

			var propertyIdProp = serializedObject.FindProperty("m_propertyId");
			var propertyTypeProp = serializedObject.FindProperty("m_propertyType");
			var index = EditorGUILayout.Popup("Shader Property", Array.IndexOf(shaderPropIds, propertyIdProp.intValue), shaderPropNames);
			if( index != -1 ) {
				propertyIdProp.intValue = shaderPropIds[index];
				propertyTypeProp.intValue = (int)(shaderPropTypes[index]);
			}

			if( propertyIdProp.intValue != 0 ) {
				switch( (ShaderPropertyType)propertyTypeProp.intValue ) {
					case ShaderPropertyType.Color:
						var propertyColorProp = serializedObject.FindProperty("m_colorEndValue");
						EditorGUILayout.PropertyField(propertyColorProp);
						break;
					case ShaderPropertyType.Vector:
						var propertyVectorProp = serializedObject.FindProperty("m_vectorEndValue");
						EditorGUILayout.PropertyField(propertyVectorProp);
						break;
					case ShaderPropertyType.Float:
					case ShaderPropertyType.Range:
						var propertyFloatProp = serializedObject.FindProperty("m_floatEndValue");
						EditorGUILayout.PropertyField(propertyFloatProp);
						break;
					case ShaderPropertyType.Texture:
					case ShaderPropertyType.Int:
					default:
						break;
				}

				var durationProp = serializedObject.FindProperty("m_duration");
				EditorGUILayout.PropertyField(durationProp);
				var easeProp = serializedObject.FindProperty("m_ease");
				EditorGUILayout.PropertyField(easeProp);
				var delayProp = serializedObject.FindProperty("m_delay");
				EditorGUILayout.PropertyField(delayProp);
				var autoKillProp = serializedObject.FindProperty("m_autoKill");
				EditorGUILayout.PropertyField(autoKillProp);
			}

			if( EditorGUI.EndChangeCheck() ) {
				serializedObject.ApplyModifiedProperties();
			}

			if( Application.isPlaying && GUILayout.Button("Preview") ) {
				var player = target as DoTweenMaterialPlayer;
				player.Play();
			}
		}
	}
}
