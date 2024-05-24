using System;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	[CustomEditor(typeof(ScreenAreaAdapter))]
	public class ScreenAreaAdapterEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if( Application.isPlaying ) {
				if( ScreenAreaAdapter.Areas == null ) {
					return;
				}
				var names = new string[ScreenAreaAdapter.Areas.Length];
				for( int i = 0; i < ScreenAreaAdapter.Areas.Length; i++ ) {
					names[i] = ScreenAreaAdapter.Areas[i].Name;
				}

				var index = EditorGUILayout.Popup(Array.IndexOf(ScreenAreaAdapter.Areas, ScreenAreaAdapter.ForceArea), names);
				if( index != -1 ) {
					ScreenAreaAdapter.ForceArea = ScreenAreaAdapter.Areas[index];
				}
			}
		}
	}
}
