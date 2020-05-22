using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	public abstract class AssetPopupPreviewDrawer : PropertyDrawer {
		private static GUIContent quickEditPen;

		private static GUIContent QuickEditPen {
			get {
				if( quickEditPen == null ) {
					var texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/icon/QuickEditPen.png");
					quickEditPen = new GUIContent(texture2D, "Click to edit in a new inspector window");
				}

				return quickEditPen;
			}
		}
		
		protected abstract Object asset { get; }

		protected void OnQuickInspectorGUI(ref Rect rect) {
			rect.xMax -= EditorGUIUtility.singleLineHeight;
			var position = rect;
			position.xMax += EditorGUIUtility.singleLineHeight;
			position.xMin = position.xMax - EditorGUIUtility.singleLineHeight;
			var enabled = GUI.enabled;
			GUI.enabled = asset != null && enabled;
			if( GUI.Button(position, QuickEditPen, GUI.skin.box) && asset != null ) {
				var originObj = Selection.objects;
				// Retrieve the existing Inspector tab, or create a new one if none is open
				var inspectorWindow = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
				// Get the size of the currently window
				var size = new Vector2(inspectorWindow.position.width, inspectorWindow.position.height);
				// Clone the inspector tab (optional step)
				inspectorWindow = Object.Instantiate(inspectorWindow);
				// Set min size, and focus the window
				inspectorWindow.minSize = size;
				inspectorWindow.Show();
				inspectorWindow.Focus();
				Selection.activeObject = asset;

				inspectorWindow.GetType()
					.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Static | 
					                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
					.GetSetMethod().Invoke(inspectorWindow, new[] {(object)true});

				Selection.objects = originObj;
			}
			GUI.enabled = enabled;
		}
	}
}