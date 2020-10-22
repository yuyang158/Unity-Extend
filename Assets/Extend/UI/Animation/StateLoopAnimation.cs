using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class StateLoopAnimation : StateAnimation {
		[SerializeField]
		private int m_loops = -1;
		public int Loops {
			get => m_loops;
			set {
				m_loops = value;
				m_dirty = true;
			}
		}

		[SerializeField]
		private LoopType m_loopType;
		public LoopType LoopType {
			get => m_loopType;
			set {
				m_loopType = value;
				m_dirty = true;
			} 
		}
	}
}