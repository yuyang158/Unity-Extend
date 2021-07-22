using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Extend.UI {
	[RequireComponent(typeof(CanvasRenderer))]
	public class DummyImage : Graphic {
		protected override void Awake() {
			base.Awake();
			raycastTarget = true;
		}

		protected override void OnPopulateMesh(VertexHelper vh) {
			vh.Clear();
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(DummyImage))]
	public class DummyImageEditor : Editor {
		public override void OnInspectorGUI() {
			EditorGUILayout.LabelField("No Draw Mesh Component");
		}
	}
#endif
}