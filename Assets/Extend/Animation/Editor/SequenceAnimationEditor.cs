using System;
using System.IO;
using System.Linq;
using Extend.Common.Editor;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Animation.Editor {
	[CustomEditor(typeof(SequenceAnimator))]
	public class SequenceAnimatorEditor : UnityEditor.Editor {
		private ReorderableList m_animationsList;
		private void OnEnable() {
			var animationsProp = serializedObject.FindProperty("Animations");
			m_animationsList = new ReorderableList(serializedObject, animationsProp) {
				elementHeight = UIEditorUtil.LINE_HEIGHT * 2,
				drawElementCallback = (rect, index, active, focused) => {
					rect.height = EditorGUIUtility.singleLineHeight;
					var animationProp = animationsProp.GetArrayElementAtIndex(index);
					var nameProp = animationProp.FindPropertyRelative("m_name");
					EditorGUI.PropertyField(rect, nameProp);
					
					rect.y += UIEditorUtil.LINE_HEIGHT;
					var loopProp = animationProp.FindPropertyRelative("m_loop");
					EditorGUI.PropertyField(rect, loopProp);
				}
			};
		}

		private const float m_interval = 0.033333333f;
		private float m_timeLast;
		private int m_frame;
		private readonly GUIContent m_previewContent = new GUIContent();

		public override void OnInspectorGUI() {
			m_animationsList.DoLayoutList();
			var defaultAnimationProp = serializedObject.FindProperty("DefaultAnimation");
			EditorGUILayout.PropertyField(defaultAnimationProp);

			var atlas = serializedObject.FindProperty("Atlas");
			EditorGUILayout.PropertyField(atlas);

			serializedObject.ApplyModifiedProperties();
			if( m_animationsList.index == -1 ) {
				return;
			}

			var animator = target as SequenceAnimator;
			if( !animator.Atlas ) {
				return;
			}

			if( animator.Atlas.spriteCount == 0 ) {
				SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
				// EditorGUILayout.HelpBox("Atlas 需要Build否则无法预览。", MessageType.Warning);
				return;
			}
			var animation = animator.Animations[m_animationsList.index];
			var sprite = animator.Atlas.GetSprite(animator.name + "_" + animation.Name + m_frame);
			if( !sprite ) {
				m_frame = 0;
				sprite = animator.Atlas.GetSprite(animator.name + "_" + animation.Name + m_frame);
			}
			else if( Time.realtimeSinceStartup - m_timeLast > m_interval ) {
				m_frame++;
				m_timeLast = Time.realtimeSinceStartup;
			}
			
			m_previewContent.image = AssetPreview.GetAssetPreview(sprite);
			EditorGUILayout.LabelField(m_previewContent, GUILayout.Width(200), GUILayout.Height(200));
			Repaint();
			DestroyImmediate(sprite);
		}
	}
}
