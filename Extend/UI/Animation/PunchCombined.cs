using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class PunchCombined {
		public MovePunchAnimation Move;
		public RotationPunchAnimation Rotation;
		public ScalePunchAnimation Scale;

		public Tween[] AllTween { get; private set; } = new Tween[3];

		public void Active(Transform t) {
			BuildAllTween(t);
			if( Application.isPlaying ) {
				foreach( var tweener in AllTween ) {
					tweener?.Restart();
				}
			}
		}

		private void BuildAllTween(Transform t) {
			AllTween[0] = Move.Active(t);
			AllTween[1] = Rotation.Active(t);
			AllTween[2] = Scale.Active(t);
		}

		public void Stop() {
			foreach( var tweener in AllTween ) {
				tweener.Complete();
			}
		}
	}
}