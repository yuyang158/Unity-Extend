using System;
using UnityEngine;

namespace Extend.StateActionGroup.Behaviour {
	[Serializable]
	public class GameObjectActiveBehaviourData : BehaviourDataBase {
		[SerializeField]
		private bool m_active;

		public override void ApplyToBehaviour(BehaviourBase behaviour) {
			var activeBehaviour = behaviour as GameObjectActiveBehaviour;
			if(!activeBehaviour.GOToActive)
				return;
			activeBehaviour.GOToActive.SetActive(m_active);
		}

		public override void CopySourceBehaviour(BehaviourBase behaviour) {
			var activeBehaviour = behaviour as GameObjectActiveBehaviour;
			if(!activeBehaviour.GOToActive)
				return;
			m_active = activeBehaviour.GOToActive.activeSelf;
		}
	}
	
	[Serializable]
	public class GameObjectActiveBehaviour : BehaviourBase {
		public GameObject GOToActive;
		
		public override void Start() {
			m_data.ApplyToBehaviour(this);
		}

		public override void Exit() {
		}

		public override bool Complete => true;

		public override BehaviourDataBase CreateDefaultData() {
			var data = new GameObjectActiveBehaviourData();
			data.CopySourceBehaviour(this);
			data.TargetId = Id;
			return data;
		}
	}
}