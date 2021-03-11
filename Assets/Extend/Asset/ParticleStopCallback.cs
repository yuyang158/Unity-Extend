using System;
using UnityEngine;

namespace Extend.Asset {
	public class ParticleStopCallback : MonoBehaviour {
		public AutoRecycle Context { private get; set; }
		public int ParticleIndex { private get; set; }
		private void OnParticleSystemStopped() {
			Context.MarkParticleStopped( ParticleIndex );
		}
	}
}