using System;
using System.Linq;
using Extend.Editor.InspectorGUI;
using UnityEditor;
using UnityEngine;

namespace Extend.Switcher.Editor {
	[CustomEditor(typeof(StateSwitcher))]
	public class StateSwitcherEditor : ExtendInspector {
		private string stateName;
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if(!Application.isPlaying)
				return;
			
			EditorGUILayout.Space();
			
			var stateSwitcher = target as StateSwitcher;
			var index = Array.FindIndex(stateSwitcher.States, state => state.StateName == stateName);
			var displayOptions = stateSwitcher.States.Select(state => state.StateName).ToArray();
			var selected = EditorGUILayout.Popup("State Test", index, displayOptions);
			if( selected != index ) {
				if( selected < 0 || selected >= displayOptions.Length ) {
					stateName = null;
				}
				else {
					stateName = displayOptions[selected];
					var result = Array.Find(stateSwitcher.States, state => state.StateName == stateName);
					result?.Init();
					stateSwitcher.Switch(stateName);
				}
			}
		}
	}
}