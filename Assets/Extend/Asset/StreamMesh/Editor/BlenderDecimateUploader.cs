using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.StreamMesh.Editor {
	public static class BlenderDecimateUploader {
		[MenuItem("Tools/Upload")]
		private static void Upload() {
			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if( Path.GetExtension(path).ToLower() != ".fbx" ) {
				return;
			}

			UploadFbx(path, 50);
		}

		private const string BASE_URL = "http://localhost:8666";
		public static IEnumerator UploadFbx(string fbxFile, int radio) {
			var client = new WebClient();
			var buffer = client.UploadFile(new Uri($"{BASE_URL}/upload?radio={radio}&name={Path.GetFileNameWithoutExtension(fbxFile)}"), "POST", fbxFile);
			var result = Encoding.UTF8.GetString(buffer);
			if( result != "success" ) {
				EditorUtility.DisplayDialog("Upload error", result, "ok");
				yield break;
			}
			
			Debug.Log("Upload Success");
			EditorCoroutineUtility.StartCoroutineOwnerless(DownloadFbx(fbxFile, radio));
		}

		private static IEnumerator DownloadFbx(string fbxFile, int radio) {
			while( true ) {
				try {
					var req = WebRequest.CreateHttp($"{BASE_URL}/status?name={Path.GetFileNameWithoutExtension(fbxFile)}");
					var response = req.GetResponse();
					var result = new byte[response.ContentLength];
					using( var stream = response.GetResponseStream() ) {
						stream.Read(result, 0, result.Length);
						if( Encoding.UTF8.GetString(result) == "none" ) {
							Debug.Log("Remote process finished");
							break;
						}
					}
				}
				catch( Exception ex ) {
					Debug.LogWarning(ex);
				}
				yield return new EditorWaitForSeconds(2);
			}
			var client = new WebClient();
			var target = fbxFile.Substring(0, fbxFile.Length - 4) + $"-{radio}.fbx";
			Debug.Log("Download processed file : " + target);
			client.DownloadFile(new Uri($"{BASE_URL}/download?radio={radio}&name={Path.GetFileNameWithoutExtension(fbxFile)}"), target);
		}
	}
}