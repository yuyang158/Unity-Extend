using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Extend.Common;
using UnityEngine;
using UnityEngine.Networking;
using XLua;

namespace Extend.Network.HttpClient {
	[LuaCallCSharp]
	public class HttpFileRequest {
		private static readonly string m_cacheDirectory = Application.persistentDataPath + "/Cache";
		private static readonly MD5 m_md5 = MD5.Create();
		private static readonly int m_maxCacheCount = 64;

		private UnityWebRequest m_request;
		private string m_localFilename;

		public float Progress => m_request.downloadProgress;

		public ulong downloadedInBytes => m_request.downloadedBytes;

		public long totalInBytes {
			get {
				if( m_request.GetResponseHeaders().TryGetValue("Content-Length", out string length) ) {
					return long.Parse(length);
				}

				return -1;
			}
		}


		public void RequestImage(string uri, Action<Texture2D> callback) {
			if( string.IsNullOrEmpty(uri) ) {
				Debug.LogError($"Request image url is null or empty.");
				return;
			}
			GlobalCoroutineRunnerService.Get().StartCoroutine(RequestTextureFile(uri, callback));
		}

		public void RequestAudio(string uri, AudioType audioType, Action<AudioClip> callback) {
			GlobalCoroutineRunnerService.Get().StartCoroutine(RequestAudioFile(uri, audioType, callback));
		}

		public void RequestText(string uri, Action<string> callback) {
			GlobalCoroutineRunnerService.Get().StartCoroutine(DoRequestFile(uri, callback, true));
		}

		public void RequestFile(string uri, Action<string> callback) {
			GlobalCoroutineRunnerService.Get().StartCoroutine(DoRequestFile(uri, callback, false));
		}


		public static void CacheFileExpireCheck() {
			try {
				if( !Directory.Exists(m_cacheDirectory) ) {
					Directory.CreateDirectory(m_cacheDirectory);
					return;
				}
				var directoryInfo = new DirectoryInfo(m_cacheDirectory);
				var fileInfos = directoryInfo.GetFiles();
				if( m_maxCacheCount >= fileInfos.Length ) {
					return;
				}
				Array.Sort(fileInfos, (a, b) => a.LastAccessTime.CompareTo(b.LastAccessTime));
				for( int i = 0; i < fileInfos.Length - m_maxCacheCount; i++ ) {
					File.Delete(fileInfos[i].FullName);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
		}

		private string _localFilePath;

		private bool LocalFileCheck(string uri, out string formatPath) {
			var uriBytes = Encoding.UTF8.GetBytes(uri);
			var md5Bytes = m_md5.ComputeHash(uriBytes);
			var sBuilder = new StringBuilder();

			foreach( var b in md5Bytes ) {
				sBuilder.Append(b.ToString("x2"));
			}

			var cacheFilename = sBuilder.ToString();
			_localFilePath = $"{m_cacheDirectory}/{cacheFilename}";
			if( File.Exists(_localFilePath) ) {
				formatPath = "file://" + _localFilePath;
				return false;
			}

			formatPath = uri;
			return true;
		}

		private void FileDownloadPostProcess(bool isRemoteFile) {
			if( m_request.result != UnityWebRequest.Result.Success ) {
				Debug.LogWarning($"Download file {m_request.url} failed, reason : {m_request.error}");
				m_request = null;
				return;
			}
				
			if( isRemoteFile ) {
				File.WriteAllBytes(_localFilePath, m_request.downloadHandler.data);
			}
		}

		private IEnumerator DoRequestFile(string uri, Action<string> callback, bool isTextFile) {
			var isRemoteFile = LocalFileCheck(uri, out string formatPath);
			m_request = UnityWebRequest.Get(formatPath);
			yield return m_request.SendWebRequest();
			FileDownloadPostProcess(isRemoteFile);
			if( m_request == null ) {
				callback(null);
				yield break;
			}

			callback(isTextFile ? m_request.downloadHandler.text : _localFilePath);
			m_request.Dispose();
		}

		private IEnumerator RequestAudioFile(string uri, AudioType audioType, Action<AudioClip> callback) {
			var isRemoteFile = LocalFileCheck(uri, out string formatPath);
			m_request = UnityWebRequestMultimedia.GetAudioClip(formatPath, audioType);
			yield return m_request.SendWebRequest();
			FileDownloadPostProcess(isRemoteFile);
			
			if( m_request == null ) {
				callback(null);
				yield break;
			}

			AudioClip clip = DownloadHandlerAudioClip.GetContent(m_request);
			callback(clip);
			m_request.Dispose();
		}

		private IEnumerator RequestTextureFile(string uri, Action<Texture2D> callback) {
			var isRemoteFile = LocalFileCheck(uri, out string formatPath);
			m_request = UnityWebRequestTexture.GetTexture(formatPath);
			yield return m_request.SendWebRequest();
			FileDownloadPostProcess(isRemoteFile);
			
			if( m_request == null ) {
				callback(null);
				yield break;
			}

			Texture2D texture = DownloadHandlerTexture.GetContent(m_request);
			callback(texture);
			m_request.Dispose();
		}
	}
}