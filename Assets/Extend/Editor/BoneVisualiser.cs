using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public static class BoneVisualiser {
		private static SkinnedMeshRenderer[] m_renderers;
		private static readonly Color[] m_boneColors = {Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.white};

		private const string MENU_NAME = "Tools/Bone Visualiser Active";
		private static bool m_active;

		[MenuItem(MENU_NAME)]
		private static void BoneVisualiserActive() {
			m_active = !m_active;
			Menu.SetChecked(MENU_NAME, m_active);
		}
		
		[InitializeOnLoadMethod]
		private static void BoneVisualiserRegister() {
			EditorApplication.delayCall += () => {
				m_active = EditorPrefs.GetBool(MENU_NAME, true);
				Menu.SetChecked(MENU_NAME, m_active);
			};
			
			Selection.selectionChanged += () => {
				m_renderers = null;
				if( !Selection.activeGameObject ) {
					return;
				}

				var path = AssetDatabase.GetAssetPath(Selection.activeGameObject);
				if( !string.IsNullOrEmpty(path) ) {
					return;
				}

				var skinnedMeshRenderers = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				if( skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0 ) {
					m_renderers = Selection.activeGameObject.GetComponentsInParent<SkinnedMeshRenderer>();
				}
				else {
					m_renderers = skinnedMeshRenderers;
				}
			};

			SceneView.duringSceneGui += _ => {
				if( m_renderers == null || m_renderers.Length == 0 || !m_active ) {
					return;
				}

				for( int i = 0; i < m_renderers.Length; i++ ) {
					Handles.color = m_boneColors[i % m_boneColors.Length];
					var renderer = m_renderers[i];
					if( !renderer )
						continue;
					foreach( var bone in renderer.bones ) {
						if( !bone || !bone.parent ) {
							continue;
						}

						var start = bone.parent.position;
						var end = bone.position;
						if( Handles.Button(bone.position, Quaternion.identity, 0.03f, 0.03f, Handles.SphereHandleCap) ) {
							Selection.activeGameObject = bone.gameObject;
						}
						
						if (bone.parent.childCount == 1)
							Handles.DrawAAPolyLine(5f, start, end);
						else
							Handles.DrawDottedLine(start, end, 0.5f);
					}
				}
			};
		}
	}
}