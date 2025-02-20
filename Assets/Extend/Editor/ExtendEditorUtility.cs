using System.IO;
using System.Linq;
using Extend.Common.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extend.Editor {
	public static class ExtendEditorUtility {
		[MenuItem("Tools/常用工具/打开Log目录")]
		private static void OpenPersistencePath() {
			EditorUtility.RevealInFinder(Application.persistentDataPath);
		}
		
		[MenuItem("Tools/常用工具/Find Missing MonoBehaviour In Scene")]
		private static void FindMissingMonoBehaviourInScene() {
			for( int i = 0; i < SceneManager.sceneCount; i++ ) {
				var scene = SceneManager.GetSceneAt(i);
				var rootGameObjects = scene.GetRootGameObjects();
				foreach( var rootGameObject in rootGameObjects ) {
					FindMissingMonoBehaviourInTransform(rootGameObject.transform);
				}
			}
		}
		
		[MenuItem("Tools/常用工具/Find Missing MonoBehaviour In Prefab")]
		private static void FindMissingMonoBehaviourInPrefab() {
			var prefabs = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
			for( int i = 0; i < prefabs.Length; i++ ) {
				var prefabPath = prefabs[i].Substring(Application.dataPath.Length - 6);
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				FindMissingMonoBehaviourInTransform(go.transform, true);
				EditorUtility.DisplayProgressBar("Missing Finding", $"Progress {i + 1} / {prefabs.Length}", (i + 1) / (float)prefabs.Length);
			}
			
			EditorUtility.ClearProgressBar();
		}

		
		[MenuItem("Tools/常用工具/Remove Missing MonoBehaviour In Select")]
		private static void RemoveMissingMonoBehaviourInSelect() {
			var go = Selection.activeGameObject;
			var count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
			Debug.Log(count);
		}
		
		private static void FindMissingMonoBehaviourInTransform(Transform t, bool selectPrefabRoot = false) {
			var components = t.GetComponents<Component>();
			if( components.Any(component => !component) ) {
				if( selectPrefabRoot ) {
					var root = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
					Selection.activeObject = root;
					EditorGUIUtility.PingObject(root);
				}
				else {
					Selection.activeObject = t.gameObject;
					EditorGUIUtility.PingObject(Selection.activeObject);
				}
				return;
			}

			for( int i = 0; i < t.childCount; i++ ) {
				FindMissingMonoBehaviourInTransform(t.GetChild(i), selectPrefabRoot);
			}
		}

		[MenuItem("Tools/Asset/GUID Convert")]
		public static void GUIDConvert() {
			var input = InputWindow.CreateWindow("Input GUID");
			input.Callback += s => {
				var path = AssetDatabase.GUIDToAssetPath(s);
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			};
			input.ShowModal();
		}

		[MenuItem("Tools/Asset/Duplicate Select Asset")]
		public static void DuplicateSelectAsset() {
			var input = InputWindow.CreateWindow("Input Names", true, false);
			input.Callback += s => {
				var path = AssetDatabase.GetAssetPath(Selection.activeObject);
				using( var reader = new StringReader(s) ) {
					var line = reader.ReadLine();
					while( !string.IsNullOrEmpty(line) ) {
						AssetDatabase.CopyAsset(path, Path.Combine(Path.GetDirectoryName(path) ??
							string.Empty, line + Path.GetExtension(path)));
						line = reader.ReadLine();
					}
				}
			};
			input.ShowModal();
		}

		[MenuItem("Tools/Animation/Find Blend Tree Parameter")]
		public static void FindBlendTreeParameter() {
			var controller = Selection.activeObject as AnimatorController;
			if( !controller ) {
				return;
			}

			foreach( var layer in controller.layers ) {
				foreach( var sm in layer.stateMachine.stateMachines ) {
					FindInStateMachine(sm, layer);
				}
			}
		}
		
		[MenuItem("Tools/Animation/Animator Hash Convert")]
		public static void AnimatorHashConvert() {
			var window = InputWindow.CreateWindow("Animator Hash Convert");
			window.Callback += s => {
				Debug.Log(Animator.StringToHash(s));
			};
			window.ShowModal();
		}

		[MenuItem("Tools/Animation/Convert Animator Name")]
		public static void ConvertAnimatorName() {
			var input = InputWindow.CreateWindow("Animator Name", false);
			input.Callback += s => {
				Debug.Log("Animator Hash : " + Animator.StringToHash(s));
			};
			input.Show();
		}

		private static void FindInStateMachine(ChildAnimatorStateMachine sm, AnimatorControllerLayer layer) {
			foreach( var subSM in sm.stateMachine.stateMachines ) {
				FindInStateMachine(subSM, layer);
			}
			var states = sm.stateMachine.states;
			foreach( var state in states ) {
				var blendTree = state.state.motion as BlendTree;
				if( !blendTree ) {
					continue;
				}

				foreach( var childMotion in blendTree.children ) {
					var childTree = childMotion.motion as BlendTree;
					if( !childTree ) {
						continue;
					}
					if( childTree.blendParameter == "AbilityFloatData" || childTree.blendParameterY == "AbilityFloatData" ) {
						Debug.Log($"Layer : {layer.name} {sm.stateMachine}.{state.state.name} parameter use AbilityFloatData");
					}
				}
				
				if( blendTree.blendParameter == "AbilityFloatData" || blendTree.blendParameterY == "AbilityFloatData" ) {
					Debug.Log($"Layer : {layer.name} {sm.stateMachine}.{state.state.name} parameter use AbilityFloatData");
				}
			}
		}

		[MenuItem("Tools/Asset/Show Material Keywords")]
		private static void ShowWindow() {
			var mat = Selection.activeObject as Material;
			if( !mat )
				return;

			Debug.Log(string.Join(";", mat.shaderKeywords));
		}
	}
}