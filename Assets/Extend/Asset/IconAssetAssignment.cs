using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[RequireComponent(typeof(Image)), LuaCallCSharp]
	public class IconAssetAssignment : MonoBehaviour {
		[SerializeField]
		private string m_iconPath;
		private Image m_img;

		private Image GetImage() {
			if( !m_img ) {
				m_img = GetComponent<Image>();
			}

			return m_img;
		}
		private void Awake() {
			if( string.IsNullOrEmpty(m_iconPath) ) {
				GetImage().enabled = false;
			}
			else {
				Refresh(m_iconPath);
			}
		}

		public string IconPath {
			get => m_iconPath;
			set {
				if( m_iconPath == value )
					return;
				Refresh(value);
			}
		}

		private void Refresh(string iconPath) {
			if( m_element != null ) {
				m_element.OnSpriteLoaded -= OnSpriteLoaded;
			}

			m_iconPath = iconPath;
			if( string.IsNullOrEmpty(m_iconPath) ) {
				GetImage().sprite = null;
				m_element?.Release();
				m_element = null;
				GetImage().enabled = false;
			}
			else {
				m_element?.Release();
				GetImage().enabled = true;
				var service = SpriteAssetService.Get();
				service.ApplyLoadingFx(GetImage());
				m_element = service.RequestIcon(m_iconPath);
				m_element.Acquire();
				m_element.OnSpriteLoaded += OnSpriteLoaded;
			}
		}


		private PackedSprite.SpriteElement m_element;

		private void OnSpriteLoaded(PackedSprite.SpriteElement element) {
			m_element.OnSpriteLoaded -= OnSpriteLoaded;
			SpriteAssetService.Get().ClearLoadingFx(GetImage());
			GetImage().sprite = element.Sprite;
		}

		private void OnDestroy() {
			IconPath = null;
		}
	}
}
