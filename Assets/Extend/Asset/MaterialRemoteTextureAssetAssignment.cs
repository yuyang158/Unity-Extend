using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XLua;

namespace Extend.Asset
{
	[LuaCallCSharp]
	public class MaterialRemoteTextureAssetAssignment : MonoBehaviour
	{
		public string Property;
		private Material _material;
		private string _textureRemotePath;
		private UnityWebRequest _request;
		private Texture _downloadedTexture;

		private void Awake()
		{
			_material = GetComponent<Renderer>().material;
		}

		public string TextureRemotePath
		{
			get => _textureRemotePath;
			set
			{
				if (_textureRemotePath == value || string.IsNullOrEmpty(value))
					return;
				_textureRemotePath = value;
				StartCoroutine(DoRequestTexture());
			}
		}

		private void OnDisable()
		{
			_request?.Abort();
		}

		private IEnumerator DoRequestTexture()
		{
			if (_downloadedTexture)
			{
				Destroy(_downloadedTexture);
				_downloadedTexture = null;
			}
			using (_request = UnityWebRequestTexture.GetTexture(_textureRemotePath))
			{
				yield return _request.SendWebRequest();
				if (_request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogWarning($"Remote image request error : {_request.error}.");
					_request = null;
					yield break;
				}
				var texture = DownloadHandlerTexture.GetContent(_request);
				_request = null;
				if (!string.IsNullOrEmpty(Property))
					_material.SetTexture(Property, texture);
			}
		}

		private void OnDestroy()
		{
			if (_downloadedTexture)
			{
				Destroy(_downloadedTexture);
			}
		}
	}
}
