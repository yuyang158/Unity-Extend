using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Extend.Editor {
	[InitializeOnLoad]
	public class ToolbarTimeScale {
		static ToolbarTimeScale() {
			ToolbarExtender.LeftToolbarGUI.Add(DrawTimeScale);
		}

		private static void DrawTimeScale() {
			GUILayout.Space(10);
			GUILayout.Label("Time Scale");
			GUILayout.Space(5);
			Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 3, GUILayout.Width(80));
			GUILayout.Space(5);
			Time.timeScale = float.Parse(GUILayout.TextField(Time.timeScale.ToString("0.00"), GUILayout.Width(40)));
		}
	}
}