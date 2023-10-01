using Extend.StateActionGroup.Behaviour;
using UnityEngine;
using XLua;

namespace Extend.StateActionGroup {
	[LuaCallCSharp]
	public class SAG : MonoBehaviour {
		[SerializeReference]
		public BehaviourBase[] Behaviours;

		public StateDataGroup[] DataGroups;

		private bool m_started;

		private string m_activeStateName;

		public string ActiveStateName {
			get => m_activeStateName;
			set => Switch(value);
		}

		public void Switch(string stateName) {
			var dataGroup = FindDataGroup(stateName);
			if( dataGroup == null ) {
				return;
			}

			m_activeStateName = stateName;
			ApplyDataAndStart(dataGroup);
		}

		private void ApplyDataAndStart(StateDataGroup dataGroup) { 
			if( m_started ) {
				foreach( BehaviourBase behaviour in Behaviours ) {
					behaviour.Exit();
				}
			}

			m_started = true;
			foreach( BehaviourBase behaviour in Behaviours ) {
				var behaviourData = dataGroup.FindById(behaviour.Id);
				if( behaviourData == null ) {
					continue;
				}
				behaviour.Data = behaviourData;
				behaviour.Start();
			}
		}

		private StateDataGroup FindDataGroup(string dataGroupName) {
			foreach( StateDataGroup dataGroup in DataGroups ) {
				if( dataGroup.StateName == dataGroupName ) {
					return dataGroup;
				}
			}

			return null;
		}

		public BehaviourBase FindBehaviourById(int id) {
			foreach( BehaviourBase behaviour in Behaviours ) {
				if( behaviour.Id == id ) {
					return behaviour;
				}
			}

			return null;
		}
	}
}
