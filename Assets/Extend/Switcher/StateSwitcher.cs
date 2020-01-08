using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Extend.Switcher {
	[LuaCallCSharp]
	public class StateSwitcher : MonoBehaviour {
		[Serializable]
		public class State {
			public string StateName;
			public GOActiveSwitcher[] GOActiveSwitchers;
			public AnimatorSwitcher[] AnimatorSwitchers;

			private List<ISwitcher> switchers;

			public void Init() {
				switchers = new List<ISwitcher>(GOActiveSwitchers.Length);
				switchers.AddRange(GOActiveSwitchers);
				switchers.AddRange(AnimatorSwitchers);
			}

			public void Switch() {
				foreach( var s in switchers ) {
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