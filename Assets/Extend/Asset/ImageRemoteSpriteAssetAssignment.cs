using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(Image))]
	public class ImageRemoteSpriteAssetAssignment : MonoBehaviour {
		private Image m_img;
		private string m_spriteRemotePath;
		private UnityWebRequest m_request;
		private Sprite m_downloadedSprite;

		private void Awake() {
			m_img = GetComponent<Image>();
		}
		
		public string SpriteRemotePath {
			get => m_spriteRemotePath;
			set {
				if( m_spriteRemotePath == value )
					return;
				m_spriteRemotePath = value;
				StartCoroutine(DoRequestTexture());
			}
		}

		private void OnDisable() {
			m_request?.Abort();
		}

		private IEnumerator DoRequestTexture() {
			if( m_downloadedSprite ) {
				Destroy(m_downloadedSprite);
				m_downloadedSprite = null;
			}

			m_request?.Abort();
			using( m_request = UnityWebRequestTexture.GetTexture(m_spriteRemotePath) ) {
				yield return m_request.SendWebRequest();
				if( m_request.result != UnityWebRequest.Result.Success ) {
					Debug.LogWarning($"Remote image {m_request.url} request error : {m_request.error}.");
					m_request = null;
					yield break;
				}
				var texture2D = DownloadHandlerTexture.GetContent(m_request);
				m_request = null;
				m_img.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
				m_downloadedSprite = m_img.sprite;
			}
		}

		private void OnDestroy() {
			if( m_downloadedSprite ) {
				Destroy(m_downloadedSprite);
			}
		}
	}
}