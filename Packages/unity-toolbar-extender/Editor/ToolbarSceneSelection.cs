using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityToolbarExtender {
	[InitializeOnLoad]
	public class ToolbarSceneSelection {
		static ToolbarSceneSelection() {
			ToolbarExtender.RightToolbarGUI.Add(DrawSceneList);
		}

		private static void DrawSceneList() {
			var scenes = EditorBuildSettings.scenes;
			List<string> scenePaths = new List<string>();
			foreach( var scene in scenes ) {
				scenePaths.Add(scene.path);
			}
			var sceneFiles = Directory.GetFiles($"Assets/Scenes", "*.unity", SearchOption.AllDirectories);
			foreach( var sceneFile in sceneFiles ) {
				var formatSceneFilePath = sceneFile.Replace('\\', '/');
				if( scenePaths.Contains(formatSceneFilePath) ) {
					continue;
				}
				
				scenePaths.Add(formatSceneFilePath);
			}
			var activeScene = SceneManager.GetActiveScene();

			var index = scenePaths.IndexOf(activeScene.path);
			var sceneNames = scenePaths.Select(e => Path.GetFileNameWithoutExtension(e)).ToArray();
			
			var newIndex = EditorGUILayout.Popup(GUIContent.none, index, sceneNames, GUILayout.Width(120));
			if( newIndex != index ) {
				EditorSceneManager.OpenScene(scenePaths[newIndex], OpenSceneMode.Single);
			}
		}
	}
}