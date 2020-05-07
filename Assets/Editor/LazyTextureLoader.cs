using UnityEditor;
using UnityEngine;

public static class LazyTextureLoader {
	private static GUIContent quickEditPen;
	public static GUIContent QuickEditPen {
		get {
			if( quickEditPen == null ) {
				var texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/icon/QuickEditPen.png");
				quickEditPen = new GUIContent(texture2D, "Click to edit in a new inspector window");
			}

			return quickEditPen;
		}
	}
}