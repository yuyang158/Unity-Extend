using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extend.Switcher {
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

		private void Awake() {
			foreach( var state in States ) {
				state.Init();
			}
		}

		public void Switch(string stateName) {
			var result = Array.Find(States, state => state.StateName == stateName);
			if( result == null ) {
				Debug.LogError($"Can not find state {stateName}");
				return;
			}
			
			result.Switch();
		}
	}
}