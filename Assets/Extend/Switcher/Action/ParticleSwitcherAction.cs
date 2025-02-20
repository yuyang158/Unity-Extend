using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class ParticleSwitcherAction : SwitcherAction {
		[SerializeField]
		private GameObject m_psRootGo;

		[SerializeField]
		private bool m_isOn;

		private static List<ParticleSystem> m_allPs = new(16);
		
		public override void ActiveAction() {
			m_allPs.Clear();
			if( !m_psRootGo ) {
				Debug.LogError($"{nameof(m_psRootGo)} is null");
				return;
			}
			m_psRootGo.GetComponentsInChildren(m_allPs);
			if( m_isOn ) {
				foreach( ParticleSystem ps in m_allPs ) {
					ps.Play();
				}
			}
			else {
				foreach( ParticleSystem ps in m_allPs ) {
					ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
				}
			} 
		}

		public override void DeactiveAction() {
		}
	}
}