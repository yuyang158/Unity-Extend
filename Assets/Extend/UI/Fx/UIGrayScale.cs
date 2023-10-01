using Extend.Asset;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI.Fx {
	public class UIGrayScale : MonoBehaviour {
		private Image[] m_cachedImages;
		private Material[] m_imageOriginMaterial;
		private static AssetReference m_imgGrayMat;

		private void Awake() {
			m_cachedImages = GetComponentsInChildren<Image>(true);
			m_imageOriginMaterial = new Material[m_cachedImages.Length];
			m_imgGrayMat ??= AssetService.Get().Load<Material>("Materials/UI/UI_ImageGray.mat");
		}

		private void OnEnable() {
			for( int i = 0; i < m_cachedImages.Length; i++ ) {
				m_imageOriginMaterial[i] = m_cachedImages[i].material;
				m_cachedImages[i].material = m_imgGrayMat.GetMaterial();
			}
		}

		private void OnDisable() {
			for( int i = 0; i < m_cachedImages.Length; i++ ) {
				m_cachedImages[i].material = m_imageOriginMaterial[i];
			}
		}
	}
}