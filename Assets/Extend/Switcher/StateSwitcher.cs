using System;
using Extend.Switcher.Action;
using UnityEngine;
using XLua;

namespace Extend.Switcher {
	[LuaCallCSharp, DisallowMultipleComponent]
	public class StateSwitcher : MonoBehaviour {
		[Serializable]
		public class State {
			public string StateName;

			[SerializeReference]
			public ISwitcherAction[] SwitcherActions;

			public void Switch(bool mute = false) {
				foreach( var action in SwitcherActions ) {
					if( mute && action is SoundPlayAction ) {
						continue;
					}
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
		public string DefaultStateName;

		public string CurrentState {
			get => m_currentState;
			set => Switch(value);
		}

		private void Start() {
			if( string.IsNullOrEmpty(DefaultStateName) ) {
				return;
			}
			Switch(DefaultStateName, true);
		}

		private State m_activateState;

		public void Switch(string stateName, bool mute = false) {
			if( m_currentState == stateName )
				return;

			m_activateState?.Exit();

			var result = Array.Find(States, state => state.StateName == stateName);
			if( result == null ) {
				Debug.LogError($"Can not find state {stateName}", this);
				return;
			}

			m_activateState = result;
			result.Switch(mute);
			m_currentState = stateName;
		}
	}
}