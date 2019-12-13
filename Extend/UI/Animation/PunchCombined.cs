using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class PunchCombined {
		public MovePunchAnimation Move;
		public RotationPunchAnimation Rotation;
		public ScalePunchAnimation Scale;

		private Tweener[] tweenerArray;

		public void Active(Transform t) {
			if( tweenerArray == null ) {
				tweenerArray = new Tweener[3];
				if( Move != null ) {
					tweenerArray[0] = Move.Active(t);
				}

				if( Rotation != null ) {
					tweenerArray[1] = Rotation.Active(t);
				}

				if( Scale != null ) {
					tweenerArray[2] = Scale.Active(t);
				}
			}

			foreach( var tweener in tweenerArray ) {
				tweener?.Restart();
			}
		}

		public void Stop() {
			if( tweenerArray == null )
				return;

			foreach( var tweener in tweenerArray ) {
				tweener.Complete();
			}
		}
	}
}