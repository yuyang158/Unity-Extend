#if !UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Extend.Editor.Preview {
	[Common.CustomPreview(typeof(Animator))]
	public class AnimatorPreview : PreviewBase {
		protected override float PlaybackSpeed {
			get => base.PlaybackSpeed;
			set {
				base.PlaybackSpeed = value;
				var animator = m_PreviewInstance.GetComponent<Animator>();
				animator.speed = value;
			}
		}

		protected override bool HasStaticPreview() {
			if( target == null )
				return false;

			var go = target as GameObject;
			return go.GetComponent<Animator>();
		}

		public override void OnPreviewSettings() {
			if(m_PreviewInstance == null)
				return;
			var animator = m_PreviewInstance.GetComponent<Animator>();
			var controller = animator.runtimeAnimatorController as AnimatorController;
			if(controller == null)
				return;
			var layer = controller.layers[0];

			var names = new List<string>();
			var hashes = new List<int>();
			foreach( var state in layer.stateMachine.states ) {
				names.Add(state.state.name);
				hashes.Add(state.state.nameHash);
			}

			var currentState = animator.GetCurrentAnimatorStateInfo(0);
			var selected = hashes.IndexOf(currentState.shortNameHash);
			EditorGUI.BeginChangeCheck();
			selected = EditorGUILayout.Popup(selected, names.ToArray(), GUILayout.Width(80));
			if( EditorGUI.EndChangeCheck() ) {
				animator.Play(hashes[selected], 0);
				animator.Update(0);
			}
			
			base.OnPreviewSettings();
		}

		protected override void SimulateUpdate() {
			Repaint();
			var animator = m_PreviewInstance.GetComponent<Animator>();
			animator.Update(m_deltaTime);
		}
	}
}
#endif