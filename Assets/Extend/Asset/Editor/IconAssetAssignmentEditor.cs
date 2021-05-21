using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Asset.Editor {
	[CustomEditor(typeof(IconAssetAssignment))]
	public class IconAssetAssignmentEditor : UnityEditor.Editor {
		private static Dictionary<string, Texture> m_cachedTexture = new Dictionary<string, Texture>(32);
		[InitializeOnLoadMethod]
		private static void Init() {
			EditorApplication.projectWindowItemOnGUI += (guid, rect) => {
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if( !assetPath.StartsWith("Assets/Resources/UI/Icon") )
					return;
				
				if( Path.GetExtension(assetPath) != ".bytes" ) {
					return;
				}
				if( rect.width > rect.height ) {
					rect.width = rect.height;
				}
				else {
					rect.height = rect.width;
				}
				EditorGUI.DrawTextureTransparent(rect, LoadIconTexture(assetPath));
			};
		}

		private static Texture2D LoadIconTexture(string assetPath) {
			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
			var tex = new Texture2D(2, 2);
			tex.LoadImage(asset.bytes);
			return tex;
		}

		private string m_currentIcon;
		private IconAssetAssignment m_iconAssignment;
		private void OnEnable() {
			m_iconAssignment = target as IconAssetAssignment;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if( m_currentIcon != m_iconAssignment.IconPath ) {
				m_currentIcon = m_iconAssignment.IconPath;
				var path = "Assets/Resources/" + m_currentIcon + ".bytes";
				if( string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)) ) {
					return;
				}
				var tex = LoadIconTexture(path);
				var img = m_iconAssignment.GetComponent<Image>();
				img.sprite = Sprite.Create(tex, new Rect(0,0, tex.width, tex.height), Vector2.one * 0.5f);
			}
		}
	}
}