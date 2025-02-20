using System.Collections.Generic;
using Extend.Common.Editor.InspectorGUI;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor {
	[CustomEditor(typeof(AutoRecycle), true)]
	public class AutoRecycleEditor : ExtendInspector {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			var autoRecycle = target as AutoRecycle;
			var flagProp = serializedObject.FindProperty("m_resFlag");
			if( autoRecycle.GetComponentInChildren<ParticleSystem>() ) {
				flagProp.intValue |= (int)AutoRecycle.ResourceFlag.PARTICLE;
			}
			else {
				flagProp.intValue &= ~(int)AutoRecycle.ResourceFlag.PARTICLE;
			}
			
			if( autoRecycle.GetComponentInChildren<TrailRenderer>() ) {
				flagProp.intValue |= (int)AutoRecycle.ResourceFlag.TRAIL;
			}
			else {
				flagProp.intValue &= ~(int)AutoRecycle.ResourceFlag.TRAIL;
			}

			serializedObject.ApplyModifiedProperties();
		}
		
		[MenuItem("Tools/Auto Recycle Check")]
		private static void AutoRecycleTopNodeCheck() {
			var prefabs = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Res" });
			var autoRecycles = new List<AutoRecycle>();
			foreach( var guid in prefabs ) {
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				autoRecycles.Clear();
				go.GetComponentsInChildren(true, autoRecycles);

				foreach( AutoRecycle autoRecycle in autoRecycles ) {
					if( autoRecycle.transform.parent != null ) {
						Debug.LogError($"Auto Recycle Check Error {assetPath}", go);
					}
				}
			}
		}
	}
}