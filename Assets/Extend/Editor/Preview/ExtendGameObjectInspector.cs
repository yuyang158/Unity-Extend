using System.Reflection;
using Extend.Editor.Preview;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.Editor.InspectorGUI {
	// [CustomEditor(typeof(GameObject), true), CanEditMultipleObjects]
	public class ExtendGameObjectInspector : OverrideEditor {
		private bool m_ShowParticlePreview;

		private int m_DefaultHasPreview;

		private ObjectPreview m_Preview;

		private ObjectPreview preview {
			get {
				if( m_Preview == null ) {
					m_Preview = CustomPreviewProcessor.TryGeneratePreview(target as GameObject);
					if( m_Preview == null )
						return null;
					m_Preview.Initialize(targets);
				}

				return m_Preview;
			}
		}

		protected override UnityEditor.Editor GetBaseEditor() {
			UnityEditor.Editor editor = null;
			var baseType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
			CreateCachedEditor(targets, baseType, ref editor);
			return editor;
		}

		private void OnEnable() {
			m_ShowParticlePreview = true;
		}

		private void OnDisable() {
			if( preview != null ) {
				var method = preview.GetType().GetMethod("OnDestroy");
				method.Invoke(preview, null);	
			}
			
			// DestroyImmediate(baseEditor);
		}

		private bool HasParticleSystemPreview() {
			return preview != null && preview.HasPreviewGUI();
		}

		private bool HasBasePreview() {
			if( m_DefaultHasPreview == 0 ) {
				m_DefaultHasPreview = baseEditor.HasPreviewGUI() ? 1 : -1;
			}

			return m_DefaultHasPreview == 1;
		}

		private bool IsShowParticleSystemPreview() {
			return HasParticleSystemPreview() && m_ShowParticlePreview;
		}

		public override bool HasPreviewGUI() {
			return HasParticleSystemPreview() || HasBasePreview();
		}

		public override GUIContent GetPreviewTitle() {
			return IsShowParticleSystemPreview() ? preview.GetPreviewTitle() : baseEditor.GetPreviewTitle();
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			if( IsShowParticleSystemPreview() ) {
				preview.OnPreviewGUI(r, background);
			}
			else {
				var pipeline = GraphicsSettings.renderPipelineAsset;
				GraphicsSettings.renderPipelineAsset = null;
				baseEditor.OnPreviewGUI(r, background);
				GraphicsSettings.renderPipelineAsset = pipeline;
			}
		}

		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
			if( IsShowParticleSystemPreview() ) {
				preview?.OnInteractivePreviewGUI(r, background);
			}
			else {
				baseEditor.OnInteractivePreviewGUI(r, background);
			}
		}

		public override void OnPreviewSettings() {
			if( IsShowParticleSystemPreview() ) {
				preview?.OnPreviewSettings();
			}
			else {
				baseEditor.OnPreviewSettings();
			}
		}

		public override string GetInfoString() {
			return IsShowParticleSystemPreview() ? preview?.GetInfoString() : baseEditor.GetInfoString();
		}

		public override void ReloadPreviewInstances() {
			if( IsShowParticleSystemPreview() ) {
				preview?.ReloadPreviewInstances();
			}
			else {
				baseEditor.ReloadPreviewInstances();
			}
		}

		/// <summary>
		/// 需要调用 GameObjectInspector 的场景拖曳，否则无法拖动物体到 Scene 视图
		/// </summary>
		/// <param name="sceneView"></param>
		public void OnSceneDrag(SceneView sceneView) {
			var t = baseEditor.GetType();
			var onSceneDragMi = t.GetMethod("OnSceneDrag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if( onSceneDragMi != null ) {
				onSceneDragMi.Invoke(baseEditor, new object[] {sceneView});
			}
		}
	}
}