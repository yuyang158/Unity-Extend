using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.VFX;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, DisallowMultipleComponent]
	public class AutoRecycle : MonoBehaviour {
		public enum SelfDestroy {
			NONE,
			DURATION,
			PARTICLE_DISAPPEAR,
			ANIMATION_EVENT,
			MANUAL,
			DURATION_AFTER_STOP
		}

		[Flags]
		public enum ResourceFlag {
			NONE = 0,
			PARTICLE = 1,
			TRAIL = 2,
			X_WEAPON = 4
		}

		[SerializeField]
		private ResourceFlag m_resFlag = ResourceFlag.NONE;

		public SelfDestroy SelfDestroyMode = SelfDestroy.DURATION;

		// [ShowIf("SelfDestroyMode", SelfDestroy.DURATION)]
		public float DestroyDuration = 1;

		[ShowIf("SelfDestroyMode", SelfDestroy.ANIMATION_EVENT)]
		public string DestroyEvent;

		private ParticleSystem[] m_particles;
		private TrailRenderer[] m_trails;
		private IActivateComponent[] m_activates;
		private float m_timeLast;
		private ParticleAutoPause m_autoPause;

		[CSharpCallLua]
		public delegate void ParticleClearAction(int key);

		public static ParticleClearAction ClearAction;

		private void Awake() {
			m_autoPause = GetComponent<ParticleAutoPause>();
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 ) {
				m_particles = GetComponentsInChildren<ParticleSystem>();
				foreach( ParticleSystem particle in m_particles ) {
					var main = particle.main;
					main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
				}
			}

			if( ( m_resFlag & ResourceFlag.TRAIL ) != 0 )
				m_trails = GetComponentsInChildren<TrailRenderer>();
			if( ( m_resFlag & ResourceFlag.X_WEAPON ) != 0 )
				m_activates = GetComponentsInChildren<IActivateComponent>();
		}

		private bool waitForPS;
		private float trailDelay;

		public int ParticleKey { get; set; } = -1;

		public void ResetAll() {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 ) {
				foreach( ParticleSystem ps in m_particles ) {
					ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
					ps.Play();
				}
			}

			if( ( m_resFlag & ResourceFlag.TRAIL ) != 0 ) {
				foreach( var trail in m_trails ) {
					trail.Clear();
					trail.emitting = true;
				}
			}

			if( ( m_resFlag & ResourceFlag.X_WEAPON ) != 0 ) {
				foreach( IActivateComponent activate in m_activates ) {
					var c = activate as Component;
					c.gameObject.SetActive(true);
				}
			}
				

			m_timeLast = 0;
			waitForPS = false;
			if( SelfDestroyMode == SelfDestroy.DURATION_AFTER_STOP ) {
				enabled = false;
			}

			var audioSource = GetComponent<AudioSource>();
			if( audioSource && audioSource.clip != null ) {
				audioSource.Play();
			}
		}

		private void Recycle(bool force = false) {
			if( ( m_resFlag & ResourceFlag.PARTICLE ) != 0 && !force ) {
				foreach( var p in m_particles ) {
					if( m_autoPause && m_autoPause.IsContainsParticle(p) ) {
						continue;
					}

					p.Stop(false,
						p.main.ringBufferMode != ParticleSystemRingBufferMode.Disabled
							? ParticleSystemStopBehavior.StopEmittingAndClear
							: ParticleSystemStopBehavior.StopEmitting);
				}

				if( m_autoPause ) {
					m_autoPause.Stop();
				}

				trailDelay = -1;
				if( m_trails != null ) {
					foreach( var trail in m_trails ) {
						trail.emitting = false;
						trailDelay = Mathf.Max(trailDelay, trail.time);
					}
				}

				if( SelfDestroyMode != SelfDestroy.DURATION && SelfDestroyMode != SelfDestroy.DURATION_AFTER_STOP ) {
					waitForPS = true;
				}

				enabled = true;
				/*if( transform.parent ) {
					transform.SetParent(null, true);
				}*/
			}
			else {
				try {
					if( ParticleKey != -1 ) {
						ClearAction?.Invoke(ParticleKey);
						ParticleKey = -1;
					}
					AssetService.Recycle(gameObject);
					if( gameObject.activeInHierarchy ) {
						gameObject.SetActive(false);
					}
				}
				catch( Exception e ) {
					Debug.LogError($"{gameObject.name} : {e.Message}", this);
					Destroy(gameObject);
				}
			}
		}

		public void Stop() {
			Recycle();
		}

		private void Update() {
			if( waitForPS ) {
				trailDelay -= Time.deltaTime;
				if( m_particles != null ) {
					foreach( ParticleSystem ps in m_particles ) {
						if( ps.particleCount > 0 ) {
							return;
						}
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
						if( m_timeLast > 0.5f )
							waitForPS = true;
						break;
					case SelfDestroy.DURATION_AFTER_STOP:
						if( m_timeLast > DestroyDuration ) {
							Recycle(true);
						}
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