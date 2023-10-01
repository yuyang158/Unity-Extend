using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI.Screens {
	[RequireComponent(typeof(CanvasScaler)), ExecuteAlways]
	public class PixelPerfectScaler : MonoBehaviour {
		private CanvasScaler m_scaler;
		private Vector2Int m_screenResolution = Vector2Int.one;

		[SerializeField]
		private int m_standardSize = 540;

		private void Awake() {
			m_scaler = GetComponent<CanvasScaler>();
		}

		private void Update() {
			var resolution = Screen.currentResolution;
			if( resolution.width != m_screenResolution.x || resolution.height != m_screenResolution.y ) {
				if( Screen.orientation == ScreenOrientation.Portrait ) {
					var scale = resolution.width / m_standardSize;
					m_scaler.scaleFactor = scale;
				}
				else {
					var scale = resolution.height / m_standardSize;
					m_scaler.scaleFactor = scale;
				}

				m_screenResolution = new Vector2Int(resolution.width, resolution.height);
			}
		}
	}
}