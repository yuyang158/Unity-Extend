using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.StreamMesh.Editor {
	public class BlenderDecimateUploader : EditorWindow {
		[MenuItem("Tools/Asset/Blender Decimate")]
		private static void GetWindow() {
			var window = GetWindow<BlenderDecimateUploader>();
			window.titleContent = new GUIContent("Blender Decimate");
			window.Show();
		}
		
		private const string BASE_URL = "http://pw-win.private-tunnel.site:8666";
		private static IEnumerator UploadFbx(string fbxFile, int radio) {
			var client = new WebClient();
			var buffer = client.UploadFile(new Uri($"{BASE_URL}/upload?radio={radio}&name={Path.GetFileNameWithoutExtension(fbxFile)}"), "POST", fbxFile);
			var result = Encoding.UTF8.GetString(buffer);
			if( result != "success" ) {
				EditorUtility.DisplayDialog("Upload error", result, "ok");
				yield break;
			}
			
			Debug.Log("Upload Success");
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(DownloadFbx(fbxFile, radio));
		}

		private static IEnumerator DownloadFbx(string fbxFile, int radio) {
			while( true ) {
				var resultString = string.Empty;
				try {
					var req = WebRequest.CreateHttp($"{BASE_URL}/status?name={Path.GetFileNameWithoutExtension(fbxFile)}");
					var response = req.GetResponse();
					var result = new byte[response.ContentLength];
					using( var stream = response.GetResponseStream() ) {
						stream.Read(result, 0, result.Length);
						resultString = Encoding.UTF8.GetString(result);
						if( resultString == "none" ) {
							Debug.Log("Remote process finished");
							EditorUtility.ClearProgressBar();
							break;
						}
					}
				}
				catch( Exception ex ) {
					Debug.LogWarning(ex);
				}
				EditorUtility.DisplayProgressBar("Remote processing", resultString, 0.5f);
				yield return new EditorWaitForSeconds(0.5f);
			}

			var request = WebRequest.CreateHttp(new Uri($"{BASE_URL}/download?radio={radio}&name={Path.GetFileNameWithoutExtension(fbxFile)}"));
			var target = fbxFile.Substring(0, fbxFile.Length - 4) + $"-{radio}.fbx";

			var res = request.GetResponse();
			var length = res.ContentLength;
			using( var stream = res.GetResponseStream() )
			using( var fileStream = new FileStream(target, FileMode.OpenOrCreate) ) {
				var buffer = new byte[10240];
				while( length > fileStream.Position ) {
					var count = stream.Read(buffer, 0, 10240);
					fileStream.Write(buffer, 0, count);
					EditorUtility.DisplayProgressBar("Downloading", target, (float)fileStream.Position / length);
					yield return null;
				}
			}
			EditorUtility.ClearProgressBar();
		}

		private GameObject m_selectedFbx;
		private int m_decimate;
		private static readonly GUIContent m_fbxLabel = new GUIContent("FBX");
		private static readonly GUIContent m_decimateLabel = new GUIContent("Decimate Radio");

		private void OnGUI() {
			m_selectedFbx = (GameObject)EditorGUILayout.ObjectField(m_fbxLabel, m_selectedFbx, typeof(GameObject), false);
			m_decimate = EditorGUILayout.IntSlider(m_decimateLabel, m_decimate, 10, 99);

			if( m_selectedFbx ) {
				var filters = m_selectedFbx.GetComponentsInChildren<MeshFilter>();
				long totalVertex = 0;
				long totalTriangle = 0;
				foreach( var filter in filters ) {
					var mesh = filter.sharedMesh;
					totalVertex += mesh.vertexCount;
					for( int i = 0; i < mesh.subMeshCount; i++ ) {
						totalTriangle += mesh.GetIndexCount(i) / 3;
					}
				}
				EditorGUILayout.LabelField("Vertex", totalVertex.ToString());
				EditorGUILayout.LabelField("Triangle", totalTriangle.ToString());
			}

			if( GUILayout.Button("Process", GUILayout.Width(120)) && m_selectedFbx ) {
				var path = AssetDatabase.GetAssetPath(m_selectedFbx);
				if( string.IsNullOrEmpty(path) ) {
					return;
				}

				if( Path.GetExtension(path).ToLower() != ".fbx" ) {
					return;
				}

				EditorCoroutineUtility.StartCoroutineOwnerless(UploadFbx(path, m_decimate));
			}
		}
	}
}