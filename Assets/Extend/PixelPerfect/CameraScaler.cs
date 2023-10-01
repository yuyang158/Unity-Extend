using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Extend.PixelPerfect {
	[RequireComponent(typeof(PixelPerfectCamera))]
	public class CameraScaler : MonoBehaviour {
		private int m_width;
		private int m_height;

		public Vector2 StandardResolution = new Vector2(1920, 1080);
		
		private void Update() {
			if( m_width != Screen.width || m_height != Screen.height ) {
				m_width = Screen.width;
				m_height = Screen.height;

				var minEdge = Mathf.Min(m_width / StandardResolution.x, m_height / StandardResolution.y);
				var pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
				pixelPerfectCamera.assetsPPU = Mathf.RoundToInt(minEdge * 32);
			}
		}
	}
}
