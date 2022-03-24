using Extend.Asset.Attribute;
using Extend.Common;
using Extend.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor {
	[CustomPropertyDrawer(typeof(AssetReference), true)]
	public class AssetReferencePropertyDrawer : AssetPopupPreviewDrawer {
		private Object m_object;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var assetRefProperty = property.FindPropertyRelative("m_assetRef");
			EditorGUI.PropertyField(position, assetRefProperty, label);
		}

		protected override Object asset => m_object;
	}
}