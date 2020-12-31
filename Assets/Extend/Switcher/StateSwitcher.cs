using System;
using Extend.Switcher.Action;
using UnityEngine;
using XLua;

namespace Extend.Switcher {
	[LuaCallCSharp]
	public class StateSwitcher : MonoBehaviour {
		[Serializable, BlackList]
		public class State {
			public string StateName;

			[SerializeReference]
			public ISwitcherAction[] SwitcherActions;

			public void Switch() {
				foreach( var s in SwitcherActions ) {
					s.ActiveSwitcher();
				}
			}
		}

		[BlackList]
		public State[] States;
		private string m_currentState;

		public string CurrentState {
			get => m_currentState;
			set {
				if( m_currentState == value )
					return;
				m_currentState = value;
				Switch(m_currentState);
			}
		}

		public void Switch(string stateName) {
			if( m_currentState == stateName )
				return;

			var result = Array.Find(States, state => state.StateName == stateName);
			if( result == null ) {
				Debug.LogError($"Can not find state {stateName}");
				return;
			}

			result.Switch();
			m_currentState = stateName;
		}
	}
}