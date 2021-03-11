using System;
using System.Linq;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class AutoRecycle : MonoBehaviour {
		public enum SelfDestroy {
			NONE,
			DURATION,
			PARTICLE_DISAPPEAR,
			ANIMATION_EVENT
		}

		[Flags]
		public enum ResourceFlag {
			NONE = 0,
			PARTICLE = 1 << 0,
			TRAIL = 2 << 0
		}

		[SerializeField]
		private ResourceFlag m_resFlag = ResourceFlag.TRAIL | ResourceFlag.PARTICLE;

		public SelfDestroy SelfDestroyMode = SelfDestroy.DURATION;

		[ShowIf("SelfDestroyMode", SelfDestroy.DURATION)]
		public float DestroyDuration = 1;

		[ShowIf("SelfDestroyMode", SelfDestroy.ANIMATION_EVENT)]
		public string DestroyEvent;

		private ParticleSystem[] m_particles;
		private bool[] m_particleAlive;
		private TrailRenderer[] m_trails;
		private float m_timeLast;

		private void Awake() {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 )
				m_particles = GetComponentsInChildren<ParticleSystem>();
			if( ( m_resFlag & ResourceFlag.TRAIL ) != 0 )
				m_trails = GetComponentsInChildren<TrailRenderer>();

			if( m_particles == null ) return;
			m_particleAlive = new bool[m_particles.Length];
		}

		private bool waitForPS;
		private float trailDelay;

		[BlackList]
		public void MarkParticleStopped(int index) {
			Debug.Assert(m_particleAlive.Length > index && index >= 0);
			m_particleAlive[index] = true;
		}

		public void ResetAll() {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 ) {
				for( var i = 0; i < m_particles.Length; i++ ) {
					var ps = m_particles[i];
					ps.Play();
					var main = ps.main;
					m_particleAlive[i] = false;

					if( main.ringBufferMode == ParticleSystemRingBufferMode.Disabled ) {
						if( !main.loop ) {
							var callback = ps.gameObject.AddComponent<ParticleStopCallback>();
							callback.Context = this;
							callback.ParticleIndex = i;
							main.cullingMode = ParticleSystemCullingMode.Automatic;
							main.stopAction = ParticleSystemStopAction.Callback;
							continue;
						}
					}

					m_particleAlive[i] = true;
				}

				foreach( var ps in m_particles ) {
					if( ps.subEmitters.subEmittersCount <= 0 ) continue;
					for( var subIndex = 0; subIndex < ps.subEmitters.subEmittersCount; subIndex++ ) {
						var sub = ps.subEmitters.GetSubEmitterSystem(subIndex);
						var subPsIndex = Array.IndexOf(m_particles, sub);
						if( subPsIndex < 0 ) {
							continue;
						}

						m_particleAlive[subPsIndex] = true;
					}
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
					p.Stop(true);
				}

				trailDelay = -1;
				foreach( var trail in m_trails ) {
					trail.emitting = false;
					trailDelay = Mathf.Max(trailDelay, trail.time);
				}

				if( SelfDestroyMode != SelfDestroy.DURATION ) {
					waitForPS = true;
				}
			}
			else {
				AssetService.Recycle(gameObject);
				for( var i = 0; i < m_particles.Length; i++ ) {
					m_particleAlive[i] = false;
				}
			}
		}

		private void Update() {
			if( waitForPS ) {
				trailDelay -= Time.deltaTime;
				var all0 = m_particleAlive.Aggregate(true, (current, v) => current & v);
				if( all0 ) {
					all0 = trailDelay < 0;
					if( all0 ) {
						Recycle(true);
					}
				}
			}
			else {
				switch( SelfDestroyMode ) {
					case SelfDestroy.DURATION:
						m_timeLast += Time.deltaTime;
						if( m_timeLast >= DestroyDuration ) {
							Recycle(true);
						}
						break;
					case SelfDestroy.PARTICLE_DISAPPEAR:
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