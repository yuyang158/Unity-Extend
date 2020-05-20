using System;
using System.Collections.Generic;
using Extend.Common.Lua;
using UnityEngine;

namespace Extend.Switcher {
	[LuaCallCSharp]
	public class StateSwitcher : MonoBehaviour {
		[Serializable]
		public class State {
			public string StateName;
			public GOActiveSwitcher[] GOActiveSwitchers;
			public AnimatorSwitcher[] AnimatorSwitchers;

			private List<ISwitcher> m_switchers;

			public void Init() {
				m_switchers = new List<ISwitcher>(GOActiveSwitchers.Length);
				m_switchers.AddRange(GOActiveSwitchers);
				m_switchers.AddRange(AnimatorSwitchers);
			}

			public void Switch() {
				foreach( var s in m_switchers ) {
					s.ActiveSwitcher();
				}
			}
		}

		public State[] States;
		private string currentState;

		public string CurrentState {
			get => currentState;
			set {
				if(currentState == value)
					return;
				currentState = value;
				Switch(currentState);
			}
		}

		private void Awake() {
			foreach( var state in States ) {
				state.Init();
			}
		}

		public void Switch(string stateName) {
			if(currentState == stateName)
				return;
			
			var result = Array.Find(States, state => state.StateName == stateName);
			if( result == null ) {
				Debug.LogError($"Can not find state {stateName}");
				return;
			}
			
			result.Switch();
			currentState = stateName;
		}
	}
}