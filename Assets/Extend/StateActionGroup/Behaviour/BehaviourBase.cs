using System;
using UnityEngine;

namespace Extend.StateActionGroup.Behaviour {
	public interface IBehaviourData {
		void ApplyToBehaviour(BehaviourBase behaviour);

		void CopySourceBehaviour(BehaviourBase behaviour);

		public int GetTargetId();
	}

	[Serializable]
	public abstract class BehaviourDataBase : IBehaviourData {
		public abstract void ApplyToBehaviour(BehaviourBase behaviour);
		public abstract void CopySourceBehaviour(BehaviourBase behaviour);

		[SerializeField]
		private int m_targetId;

		public int GetTargetId() {
			return TargetId;
		}

		public int TargetId {
			get => m_targetId;
			set => m_targetId = value;
		}
	}
	
	[Serializable]
	public abstract class BehaviourBase {
		protected IBehaviourData m_data;
		public int Id;

		public IBehaviourData Data {
			set => m_data = value;
		}

		public abstract void Start();

		public abstract void Exit();
		
		public abstract bool Complete { get; }

		public abstract BehaviourDataBase CreateDefaultData();
	}
}