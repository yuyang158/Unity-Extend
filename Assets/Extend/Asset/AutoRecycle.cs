using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, DisallowMultipleComponent]
	public class AutoRecycle : MonoBehaviour {
		public enum SelfDestroy {
			NONE,
			DURATION,
			PARTICLE_DISAPPEAR,
			ANIMATION_EVENT,
			MANUAL
		}

		[Flags]
		public enum ResourceFlag {
			NONE = 0,
			PARTICLE = 1,
			TRAIL = 2
		}

		[SerializeField]
		private ResourceFlag m_resFlag = ResourceFlag.TRAIL | ResourceFlag.PARTICLE;

		public SelfDestroy SelfDestroyMode = SelfDestroy.DURATION;

		[ShowIf("SelfDestroyMode", SelfDestroy.DURATION)]
		public float DestroyDuration = 1;

		[ShowIf("SelfDestroyMode", SelfDestroy.ANIMATION_EVENT)]
		public string DestroyEvent;

		private ParticleSystem[] m_particles;
		private TrailRenderer[] m_trails;
		private float m_timeLast;

		private void Awake() {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 ) {
				m_particles = GetComponentsInChildren<ParticleSystem>();
			}
			if( ( m_resFlag & ResourceFlag.TRAIL ) != 0 )
				m_trails = GetComponentsInChildren<TrailRenderer>();
		}

		private bool waitForPS;
		private float trailDelay;

		public void ResetAll() {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 ) {
				for( var i = 0; i < m_particles.Length; i++ ) {
					var ps = m_particles[i];
					ps.Play();
				}
			}

			if( ( m_resFlag & ResourceFlag.TRAIL ) != 0 ) {
				foreach( var trail in m_trails ) {
					trail.emitting = true;
				}
			}

			m_timeLast = 0;
			waitForPS = false;
		}

		private void Recycle(bool force = false) {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 && !force ) {
				foreach( var p in m_particles ) {
					if( p.main.ringBufferMode != ParticleSystemRingBufferMode.Disabled ) {
						p.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
					}
					else {
						p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
					}
				}

				trailDelay = -1;
				if( m_trails != null ) {
					foreach( var trail in m_trails ) {
						trail.emitting = false;
						trailDelay = Mathf.Max(trailDelay, trail.time);
					}					
				}

				if( SelfDestroyMode != SelfDestroy.DURATION ) {
					waitForPS = true;
				}
			}
			else {
				AssetService.Recycle(gameObject);
			}
		}

		public void Stop() {
			Recycle();
		}

		private void Update() {
			if( waitForPS ) {
				trailDelay -= Time.deltaTime;
				for( int i = 0; i < m_particles.Length; i++ ) {
					if( m_particles[i].particleCount > 0 ) {
						return;
					}
				}
				if( trailDelay < 0 ) {
					Recycle(true);
				}
			}
			else {
				m_timeLast += Time.deltaTime;
				switch( SelfDestroyMode ) {
					case SelfDestroy.DURATION:
						if( m_timeLast >= DestroyDuration ) {
							Recycle(true);
						}
						break;
					case SelfDestroy.PARTICLE_DISAPPEAR:
						if(m_timeLast > 0.5f)
							waitForPS = true;
						break;
				}
			}
		}

		public void OnEvent() {
			if( SelfDestroyMode == SelfDestroy.ANIMATION_EVENT ) {
				Recycle();
			}
		}
	}
}