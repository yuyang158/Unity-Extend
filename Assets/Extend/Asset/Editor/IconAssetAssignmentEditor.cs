using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Asset.Editor {
	public class IconAssetAssignmentProjectPreview : AssetPostprocessor {
		private static readonly Dictionary<string, Texture> m_cachedTexture = new Dictionary<string, Texture>(32);

		[InitializeOnLoadMethod]
		private static void Init() {
			EditorApplication.projectWindowItemOnGUI += (guid, rect) => {
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if( !assetPath.StartsWith("Assets/StreamingAssets/UI/Icon") )
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

				if( !m_cachedTexture.TryGetValue(guid, out var texture) ) {
					texture = LoadIconTexture(assetPath);
					if( texture == null ) {
						Debug.LogError("Load texture error : " + assetPath);
						return;
					}

					m_cachedTexture.Add(guid, texture);
				}

				EditorGUI.DrawTextureTransparent(rect, texture);
			};
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach( var asset in importedAssets ) {
				var guid = AssetDatabase.AssetPathToGUID(asset);
				m_cachedTexture.Remove(guid);
			}

			foreach( var asset in deletedAssets ) {
				var guid = AssetDatabase.AssetPathToGUID(asset);
				m_cachedTexture.Remove(guid);
			}
		}

		public static Texture2D LoadIconTexture(string assetPath) {
			var bytes = File.ReadAllBytes(assetPath);
			var tex = new Texture2D(2, 2);
			tex.LoadImage(bytes);
			return tex;
		}
	}

	[CustomEditor(typeof(IconAssetAssignment))]
	public class IconAssetAssignmentEditor : UnityEditor.Editor {
		private string m_currentIcon;
		private IconAssetAssignment m_iconAssignment;

		private void OnEnable() {
			m_iconAssignment = target as IconAssetAssignment;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if( m_currentIcon == m_iconAssignment.IconPath ) {
				return;
			}

			m_currentIcon = m_iconAssignment.IconPath;
			var path = "Assets/StreamingAssets/" + m_currentIcon + ".bytes";
			if( !File.Exists(path) ) {
				return;
			}

			var tex = IconAssetAssignmentProjectPreview.LoadIconTexture(path);
			var img = m_iconAssignment.GetComponent<Image>();
			img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
		}
	}
}