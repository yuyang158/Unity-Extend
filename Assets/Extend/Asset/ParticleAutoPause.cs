using System;
using UnityEngine;

namespace Extend.Asset {
	/// <summary>
	/// 开始后自动暂停粒子
	/// </summary>
	[RequireComponent(typeof(AutoRecycle))]
	public class ParticleAutoPause : MonoBehaviour {
		[SerializeField]
		private ParticleSystem[] m_particles;

		[SerializeField]
		private float m_timeout = 1;

		private float m_timeLeft;

		private void OnEnable() {
			m_timeLeft = m_timeout;
		}

		public void Stop() {
			foreach( ParticleSystem particle in m_particles ) {
				particle.Play();
			}
		}

		private void Update() {
			if( m_timeLeft < 0 ) {
				return;
			}

			m_timeLeft -= Time.deltaTime;
			if( m_timeLeft < 0 ) {
				foreach( ParticleSystem particle in m_particles ) {
					particle.Pause();
				}
			}
		}

		public bool IsContainsParticle(ParticleSystem ps) {
			return Array.IndexOf(m_particles, ps) != -1;
		}
	}
}