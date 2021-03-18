using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor {
	public class IconSpritePostProcessor : AssetPostprocessor {
		private void OnPostprocessTexture(Texture2D texture) {
			var path = assetPath;
			if( !path.StartsWith("Assets/Resources/UI/Icon") )
				return;

			var pngBuffer = texture.EncodeToPNG();
			var pngPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".bytes";
			using( var stream = new FileStream(pngPath, FileMode.OpenOrCreate)) {
				stream.Write(pngBuffer, 0, pngBuffer.Length);
			}

			AssetDatabase.DeleteAsset(path);
		}
	}
}