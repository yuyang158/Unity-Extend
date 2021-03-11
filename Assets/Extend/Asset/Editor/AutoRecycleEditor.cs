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
	}
}