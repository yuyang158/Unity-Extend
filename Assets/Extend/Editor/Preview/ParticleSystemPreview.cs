using UnityEngine;

namespace Extend.Editor.Preview {
	[Common.CustomPreview(typeof(ParticleSystem), true)]
	public class ParticleSystemPreview : PreviewBase {
		private bool m_LockParticleSystem = true;
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
			if( m_LockParticleSystem ) {
				ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
				ParticleSystemEditorUtilsReflect.editorPlaybackTime = 0f;
				ParticleSystemEditorUtilsReflect.StopEffect();
			}
			
			base.SimulateDisable();
		}

		protected override void SimulateEnable() {
			base.SimulateEnable();
			if( m_LockParticleSystem ) {
				var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
				if( particleSystem ) {
					particleSystem.Play();
					ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
				}
			}
		}

		protected override void SetSimulateMode() {
			base.SetSimulateMode();
			if( m_PreviewInstance ) {
				var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
				if( !particleSystem )
					return;
				if( m_LockParticleSystem ) {
					if( ParticleSystemEditorUtilsReflect.lockedParticleSystem != particleSystem ) {
						ParticleSystemEditorUtilsReflect.lockedParticleSystem = particleSystem;
					}
				}
				else {
					ParticleSystemEditorUtilsReflect.lockedParticleSystem = null;
				}
			}
		}

		protected override void SimulateUpdate() {
			if( m_LockParticleSystem ) {
				Repaint();
				return;
			}

			var gameObject = m_PreviewInstance;
			var particleSystem = gameObject.GetComponentInChildren<ParticleSystem>(true);
			if( particleSystem ) {
				particleSystem.Simulate(m_RunningTime, true);
				Repaint();
			}
		}
		
		/// <summary>
		/// 解锁粒子
		/// </summary>
		private void ClearLockedParticle() {
			if( m_PreviewInstance ) {
				var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
				if( particleSystem ) {
					if( m_LockParticleSystem && ParticleSystemEditorUtilsReflect.lockedParticleSystem == particleSystem ) {
						ParticleSystemEditorUtilsReflect.lockedParticleSystem = null;
					}
				}
			}
		}

		public override void OnDestroy() {
			ClearLockedParticle();
			base.OnDestroy();
		}
	}
}