#if !UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.Preview {
	[Common.CustomPreview(typeof(ParticleSystem), true)]
	public class ParticleSystemPreview : PreviewBase {
		protected override float PlaybackSpeed {
			get => base.PlaybackSpeed;
			set {
				base.PlaybackSpeed = value;
				if( !m_PreviewInstance ) {
					return;
				}

				var particleSystems = m_PreviewInstance.GetComponentsInChildren<ParticleSystem>(true);
				foreach( var particleSystem in particleSystems ) {
					var main = particleSystem.main;
					main.simulationSpeed = value;
				}
			}
		}

		protected override bool HasStaticPreview() {
			if( target == null ) {
				return false;
			}

			var gameObject = target as GameObject;
			return gameObject.GetComponentInChildren<ParticleSystem>(true);
		}

		protected override void SimulateDisable() {
			ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
			ParticleSystemEditorUtilsReflect.editorPlaybackTime = 0f;
			ParticleSystemEditorUtilsReflect.StopEffect();

			base.SimulateDisable();
			if( m_pss.Count > 0 ) {
			}
		}

		protected override void SimulateEnable() {
			base.SimulateEnable();
			var particleSystem = m_PreviewInstance.GetComponentsInChildren<ParticleSystem>(true);
			foreach( var ps in particleSystem ) {
				ps.Play();
				ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
			}
		}

		protected override void SetSimulateMode() {
			base.SetSimulateMode();
			if( m_PreviewInstance ) {
				var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
				if( !particleSystem )
					return;
				if( ParticleSystemEditorUtilsReflect.lockedParticleSystem != particleSystem ) {
					ParticleSystemEditorUtilsReflect.lockedParticleSystem = particleSystem;
				}
			}
		}

		private readonly List<ParticleSystem> m_pss = new List<ParticleSystem>();
		private readonly List<int> m_psMaxCount = new List<int>();

		public override void OnPreviewSettings() {
			if( GUILayout.Button("Optimize Max") ) {
				SimulateEnable();
				m_pss.Clear();
				m_psMaxCount.Clear();
				m_PreviewInstance.GetComponentsInChildren(m_pss);
				m_psMaxCount.Capacity = m_pss.Count;
				foreach( var particleSystem in m_pss ) {
					var main = particleSystem.main;
					main.maxParticles = 1000;
					m_psMaxCount.Add(0);
				}
			}

			if( m_pss.Count > 0 && GUILayout.Button("Apply") ) {
				var go = target as GameObject;
				var pss = new List<ParticleSystem>();
				go.GetComponentsInChildren(pss);

				for( var i = 0; i < pss.Count; i++ ) {
					var ps = pss[i];
					var main = ps.main;
					main.maxParticles = m_psMaxCount[i];
				}
			}

			base.OnPreviewSettings();
		}

		private readonly StringBuilder m_builder = new StringBuilder(256);

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			base.OnPreviewGUI(r, background);
			if( m_PreviewInstance == null )
				return;

			if( m_pss.Count == 0 )
				return;

			m_builder.Clear();
			for( var i = 0; i < m_pss.Count; i++ ) {
				var ps = m_pss[i];
				if( ps.particleCount > m_psMaxCount[i] ) {
					m_psMaxCount[i] = ps.particleCount;
				}

				if( i == m_pss.Count - 1 ) {
					m_builder.Append($"{ps.name} : {m_psMaxCount[i].ToString()}");
				}
				else {
					m_builder.AppendLine($"{ps.name} : {m_psMaxCount[i].ToString()}");
				}
			}

			var style = (GUIStyle)"HelpBox";
			var content = new GUIContent(m_builder.ToString());
			var size = style.CalcSize(content);
			EditorGUI.LabelField(new Rect(r.position, size), content, style);
		}

		protected override void SimulateUpdate() {
			Repaint();
		}

		protected override string ExtraHUDText() {
			var pss = m_PreviewInstance.GetComponentsInChildren<ParticleSystem>();
			int count = pss.Sum(particleSystem => particleSystem.particleCount);
			return $"PS Count : {count}";
		}

		/// <summary>
		/// 解锁粒子
		/// </summary>
		private void ClearLockedParticle() {
			if( !m_PreviewInstance ) {
				return;
			}

			var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
			if( particleSystem && ParticleSystemEditorUtilsReflect.lockedParticleSystem == particleSystem ) {
				ParticleSystemEditorUtilsReflect.lockedParticleSystem = null;
			}
		}

		public override void OnDestroy() {
			ClearLockedParticle();
			base.OnDestroy();
		}
	}
}
#endif