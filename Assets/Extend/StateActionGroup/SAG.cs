using System;
using System.Collections.Generic;
using Extend.StateActionGroup.Behaviour;
using UnityEngine;
using XLua;

namespace Extend.StateActionGroup {
	[LuaCallCSharp]
	public class SAG : MonoBehaviour {
		[SerializeReference]
		public BehaviourBase[] Behaviours;
		[SerializeField]
		private string[] m_blackLists;

		private Dictionary<string, string> m_blackListTransition;
		public StateDataGroup[] DataGroups;

		private bool m_started;

		private string m_activeStateName;

		public string ActiveStateName {
			get => m_activeStateName;
			set => Switch(value);
		}

		private void Awake() {
			if( m_blackLists is {Length: > 0} ) {
				m_blackListTransition = new Dictionary<string, string>(m_blackLists.Length);
				foreach( var blackList in m_blackLists ) {
					var pair = blackList.Split(';');
					m_blackListTransition.Add(pair[0], pair[1]);
				}
			}
		}

		public bool HasGroup(string stateName) {
			return FindDataGroup(stateName) != null;
		}

		public void Switch(string stateName) {
			var dataGroup = FindDataGroup(stateName);
			if( dataGroup == null ) {
				return;
			}

			if( m_blackListTransition != null ) {
				if( ActiveStateName == null ) {
					if( m_blackListTransition.TryGetValue(string.Empty, out var next) && next == stateName ) {
						return;
					}
				}
				else {
					if( m_blackListTransition.TryGetValue(ActiveStateName, out var next) && next == stateName ) {
						return;
					}
				}
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
				if( dataGroup.StateName.Contains(',') ) {
					var index = dataGroup.StateName.IndexOf(dataGroupName, StringComparison.InvariantCulture);
					if( index == -1 ) {
						continue;
					}

					if( index + dataGroupName.Length == dataGroup.StateName.Length || dataGroup.StateName[index + dataGroupName.Length] == ',' ) {
						return dataGroup;
					}
				}
				else {
					if( dataGroup.StateName == dataGroupName ) {
						return dataGroup;
					}
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