using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Extend.PixelPerfect {
	[RequireComponent(typeof(PixelPerfectCamera))]
	public class CameraScaler : MonoBehaviour {
		[SerializeField]
		private int m_width;
		[SerializeField]
		private int m_height;

		public static readonly Vector2 StandardResolution = new Vector2(640, 360);
		
		private void Update() {
			if( m_width != Screen.width || m_height != Screen.height ) {
				m_width = Screen.width;
				m_height = Screen.height;
				
				
				int verticalZoom = (int)(m_height / StandardResolution.y);
				int horizontalZoom = (int)(m_width / StandardResolution.x);
				var zoom = Math.Max(1, Math.Min(verticalZoom, horizontalZoom));
				var pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
				pixelPerfectCamera.assetsPPU = Mathf.RoundToInt((m_height * 0.5f) / (zoom * 5.62f));
			}
		}
	}
}
