using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[RequireComponent(typeof(Image)), LuaCallCSharp]
	public class IconAssetAssignment : MonoBehaviour {
		[SerializeField]
		private string m_iconPath;
		private Image m_img;

		private void Awake() {
			m_img = GetComponent<Image>();
			if( string.IsNullOrEmpty(m_iconPath) ) {
				m_img.enabled = false;
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
				m_img.sprite = null;
				m_element?.Release();
				m_element = null;
				m_img.enabled = false;
			}
			else {
				m_element?.Release();
				m_img.enabled = true;
				var service = SpriteAssetService.Get();
				service.ApplyLoadingFx(m_img);
				m_element = service.RequestIcon(m_iconPath);
				m_element.Acquire();
				m_element.OnSpriteLoaded += OnSpriteLoaded;
			}
		}


		private PackedSprite.SpriteElement m_element;

		private void OnSpriteLoaded(PackedSprite.SpriteElement element) {
			m_element.OnSpriteLoaded -= OnSpriteLoaded;
			SpriteAssetService.Get().ClearLoadingFx(m_img);
			m_img.sprite = element.Sprite;
		}

		private void OnDestroy() {
			IconPath = null;
		}
	}
}