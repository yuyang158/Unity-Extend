using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class StateLoopAnimation : StateAnimation {
		[SerializeField]
		private int loops = -1;
		public int Loops {
			get => loops;
			set {
				loops = value;
				dirty = true;
			}
		}

		[SerializeField]
		private LoopType loopType;
		public LoopType LoopType {
			get => loopType;
			set {
				loopType = value;
				dirty = true;
			} 
		}
	}
}