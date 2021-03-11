using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Extend.Common;
using Extend.Common.Editor;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset.Editor {
	public class AssetFullStatServiceWindow : EditorWindow {
#if UNITY_EDITOR_WIN
		[MenuItem("Tools/Asset/Full Stat")]
#endif
		private static void ShowWindow() {
			var window = GetWindow<AssetFullStatServiceWindow>();
			window.titleContent = new GUIContent("Full Asset Stat");
			window.Show();
		}

		private string m_clientIPAddress;
		private const string AssetFullStatClientIPSaveKey = "AssetFullStatClientIP";

		private static readonly string[] REQUEST_TARGET = {
			"Local",
			"Remote"
		};

		private void OnEnable() {
			var address = EditorPrefs.GetString(AssetFullStatClientIPSaveKey);
			m_clientIPAddress = string.IsNullOrEmpty(address) ? "127.0.0.1" : address;
		}

		private static string RequestData() {
			var client = new TcpClient();
			var ret = string.Empty;
			using( var memoryStream = new MemoryStream() ) {
				try {
					client.Connect("127.0.0.1", AssetFullStatService.LISTEN_PORT);
					var buffer = new byte[512];
					while( true ) {
						var readCount = client.GetStream().Read(buffer, 0, 512);
						if( readCount <= 0 )
							break;
						memoryStream.Write(buffer, 0, readCount);
					}
				}
				catch( Exception ) {
					// ignored
				}
				finally {
					if( memoryStream.GetBuffer().Length != 0 ) {
						var assetsString = Encoding.UTF8.GetString(memoryStream.GetBuffer());
						ret = assetsString;
					}
				}
			}

			return ret;
		}

		private readonly List<Object> m_loadedAssets = new List<Object>();
		private readonly List<string> m_loadedGameObjects = new List<string>();
		private bool m_foldAssets;
		private bool m_foldGameObjects;
		private int m_requestEndPointMode;
		private Vector2 m_scrollContent;

		private void OnGUI() {
			var topArea = new Rect(5, 5, position.width - 10, UIEditorUtil.LINE_HEIGHT);
			GUILayout.BeginArea(topArea);
			GUILayout.BeginHorizontal();
			m_requestEndPointMode = EditorGUILayout.Popup(m_requestEndPointMode, REQUEST_TARGET);
			if( m_requestEndPointMode != 0 ) {
				m_clientIPAddress = EditorGUILayout.TextField(m_clientIPAddress);
				if( GUILayout.Button("Search", GUILayout.Width(80)) ) {
					m_loadedAssets.Clear();
					m_loadedGameObjects.Clear();
					EditorPrefs.SetString(AssetFullStatClientIPSaveKey, m_clientIPAddress);
					var json = RequestData();
					if( string.IsNullOrEmpty(json) ) {
						return;
					}

					var content = JObject.Parse(RequestData());
					var assets = content["assets"] as JArray;
					foreach( string assetPath in assets ) {
						var asset = assetPath.StartsWith("assets", true, CultureInfo.InvariantCulture)
							? AssetDatabase.LoadAssetAtPath<Object>(assetPath)
							: Resources.Load<Object>(assetPath);
						if( !asset ) {
							continue;
						}

						m_loadedAssets.Add(asset);
					}

					var goArray = content["gameObjects"] as JArray;
					foreach( string goName in goArray ) {
						m_loadedGameObjects.Add(goName);
					}
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			if( m_requestEndPointMode == 0 && !Application.isPlaying )
				return;

			var contentArea = new Rect {
				xMin = topArea.xMin, xMax = topArea.xMax,
				yMin = topArea.yMax + 5, yMax = topArea.yMax + position.height - topArea.height - 5
			};
			GUILayout.BeginArea(contentArea);
			m_scrollContent = EditorGUILayout.BeginScrollView(m_scrollContent);
			var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
			m_foldAssets = EditorGUILayout.Foldout(m_foldAssets, "Assets");
			if( m_foldAssets ) {
				if( m_requestEndPointMode != 0 ) {
					foreach( var asset in m_loadedAssets ) {
						EditorGUILayout.ObjectField(asset, asset.GetType(), false);
					}
				}
				else {
					foreach( var asset in service.GetLoadedAssets() ) {
						EditorGUILayout.ObjectField(asset.UnityObject, asset.UnityObject.GetType(), false);
					}
				}
			}

			m_foldGameObjects = EditorGUILayout.Foldout(m_foldGameObjects, "Instantiate");
			if( m_foldGameObjects ) {
				if( m_requestEndPointMode != 0 ) {
					foreach( var go in m_loadedGameObjects ) {
						EditorGUILayout.LabelField(go);
					}
				}
				else {
					var goArray = service.GetInstantiateGOArray();
					foreach( var gameObject in goArray ) {
						EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);
					}
				}
			}

			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}