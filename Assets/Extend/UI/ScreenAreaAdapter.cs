using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_EDITOR && UNITY_IOS
using Extend.UI.Screens;
#endif

namespace Extend.UI {
	[RequireComponent(typeof(RectTransform))]
	public class ScreenAreaAdapter : MonoBehaviour {
		public bool UpdateEveryFrame = true;

		private RectTransform m_rectTf;
		private Rect m_lastSafeArea;

#if UNITY_EDITOR
		public static Device ForceArea;

		public class Device {
			public Rect Area;
			public string Name;
		}

		public static Device[] Areas;
#endif
		private void Awake() {
			m_rectTf = GetComponent<RectTransform>();

#if UNITY_EDITOR
			if( Areas != null ) {
				return;
			}
			var deviceFiles = Directory.GetFiles(
				"Library/PackageCache/com.unity.device-simulator.devices@1.0.0/Editor/Devices",
				"*.device", SearchOption.TopDirectoryOnly);

			Areas = new Device[deviceFiles.Length];
			for( int i = 0; i < deviceFiles.Length; i++ ) {
				var deviceFile = deviceFiles[i];
				var deviceJson = JObject.Parse(File.ReadAllText(deviceFile));
				var screens = deviceJson["screens"][0]["orientations"] as JArray;
				var rectJson = screens[2]["safeArea"];
				var device = new Device() {
					Name = Path.GetFileNameWithoutExtension(deviceFile),
					Area = new Rect(rectJson["x"]!.Value<float>(), rectJson["y"]!.Value<float>(),
						rectJson["width"]!.Value<float>(), rectJson["height"]!.Value<float>())
				};
				Areas[i] = device;
			}
#endif
		}

		private void Start() {
			UpdateRect();
		}

		private void Update() {
			if( UpdateEveryFrame || Application.isEditor ) {
				UpdateRect();
			}
		}

		public void UpdateRect() {
#if UNITY_EDITOR
			var safeArea = ForceArea?.Area ?? Screen.safeArea;
#else
			var safeArea = Screen.safeArea;
#endif
			ApplySafeArea(safeArea);
		}

		private void ApplySafeArea(Rect safeArea) {
			if( safeArea == m_lastSafeArea ) return;
			m_rectTf.anchoredPosition = Vector2.zero;
			m_rectTf.sizeDelta = Vector2.zero;

			var anchorMin = safeArea.position;
			var anchorMax = safeArea.position + safeArea.size;

			var sameIndent = Mathf.Max(safeArea.xMin, Screen.width - safeArea.xMax);

			anchorMin.x = sameIndent / Screen.width;
			anchorMin.y = 0;
			anchorMax.x = ( Screen.width - sameIndent ) / Screen.width;
			anchorMax.y /= Screen.height;
			m_rectTf.anchorMin = anchorMin;
			m_rectTf.anchorMax = anchorMax;

			m_lastSafeArea = safeArea;
		}
	}
}
