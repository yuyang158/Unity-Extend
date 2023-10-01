using System;
using Extend.Switcher.Action;
using UnityEngine;
using XLua;

namespace Extend.Switcher {
	[LuaCallCSharp]
	public class StateSwitcher : MonoBehaviour {
		[Serializable]
		public class State {
			public string StateName;

			[SerializeReference]
			public ISwitcherAction[] SwitcherActions;

			public void Switch() {
				foreach( var action in SwitcherActions ) {
					action.ActiveAction();
				}
			}

			public void Exit() {
				foreach( ISwitcherAction action in SwitcherActions ) {
					action.DeactiveAction();
				}
			}

			public override string ToString() {
				return StateName;
			}
		}

		public State[] States;
		private string m_currentState;

		public string CurrentState {
			get => m_currentState;
			set => Switch(value);
		}

		private State m_activateState;

		public void Switch(string stateName) {
			if( m_currentState == stateName )
				return;

			m_activateState?.Exit();

			var result = Array.Find(States, state => state.StateName == stateName);
			if( result == null ) {
				Debug.LogError($"Can not find state {stateName}");
				return;
			}

			m_activateState = result;
			result.Switch();
			m_currentState = stateName;
		}
	}
}