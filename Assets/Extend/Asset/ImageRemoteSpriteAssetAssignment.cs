using Extend.Network.HttpClient;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(Image))]
	public class ImageRemoteSpriteAssetAssignment : MonoBehaviour {
		private Image m_img;
		private string m_spriteRemotePath;
		private Sprite m_downloadedSprite;
		private Sprite m_defaultSprite;
		private void Awake() {
			m_img = GetComponent<Image>();
			m_defaultSprite = m_img.sprite;
		}
		
		public string SpriteRemotePath {
			get => m_spriteRemotePath;
			set {
				if( m_spriteRemotePath == value )
					return;
				m_spriteRemotePath = value;
				if( string.IsNullOrEmpty(m_spriteRemotePath) ) {
					m_img.sprite = m_defaultSprite;
					return;
				}
				var fileRequest = new HttpFileRequest();
				fileRequest.RequestImage(m_spriteRemotePath, texture => {
					if (!m_img) {
						return;
					}

					if (texture != null)
					{
						m_img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
						m_downloadedSprite = m_img.sprite;
					}
					else
					{
						m_img.sprite = m_defaultSprite;
					}
				});
			}
		}

		private void OnDestroy() {
			if( m_downloadedSprite ) {
				Destroy(m_downloadedSprite);
			}
		}
	}
}