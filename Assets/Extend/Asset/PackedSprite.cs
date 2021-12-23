using System;
using System.Collections;
using System.Collections.Generic;
using Extend.Common;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	public class PackedSprite : IDisposable {
		public class SpriteElement {
			public Sprite Sprite;
			public event Action<SpriteElement> OnSpriteLoaded;
			private int m_refCount;
			private readonly PackedSprite m_owner;

			public SpriteElement(PackedSprite owner) {
				m_owner = owner;
			}

			public string Path { get; set; }

			public void Acquire() {
				m_refCount++;
			}

			public void Release() {
				m_refCount--;
				if( m_refCount == 0 ) {
					if( m_owner == null ) {
						Object.Destroy(Sprite);
					}
					else {
						m_owner.RemoveActive(Path);
					}
				}
			}

			public void Trigger() {
				GlobalCoroutineRunnerService.Get().StartCoroutine(ExecTrigger());
			}

			private IEnumerator ExecTrigger() {
				yield return null;
				OnSpriteLoaded?.Invoke(this);
				OnSpriteLoaded = null;
			}
		}

		private const int DEFAULT_PADDING = 1;
		private readonly Texture2D m_packTexture;

		private readonly DictionaryQueue<string, SpriteElement> m_freeList;
		private readonly Dictionary<string, SpriteElement> m_inUsed;
		private readonly List<SpriteElement> m_notInUsed;
		private readonly int m_spriteSize;

		public PackedSprite(int spriteSize, bool halfPrecision = false, int width = 1024, int height = 1024) {
			TextureFormat format;
			if( halfPrecision && SystemInfo.SupportsTextureFormat(TextureFormat.RGBA4444) ) {
				format = TextureFormat.RGBA4444;
			}
			else {
				format = TextureFormat.RGBA32;
			}

			m_packTexture = new Texture2D(width, height, format, false) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Bilinear,
				name = $"Packed Sprite Texture : {spriteSize}"
			};
			NativeArray<Color32> color32s = new NativeArray<Color32>(width * height, Allocator.Temp);
			for( int i = 0; i < width * height; i++ ) {
				color32s[i] = new Color32(0, 0, 0, 0);
			}
			m_packTexture.SetPixelData(color32s, 0);
			m_spriteSize = spriteSize;

			var sizeWithPadding = spriteSize + DEFAULT_PADDING;
			var columnCount = width / sizeWithPadding;
			var rowCount = height / sizeWithPadding;
			var totalCount = columnCount * rowCount;
			m_notInUsed = new List<SpriteElement>(totalCount);
			m_freeList = new DictionaryQueue<string, SpriteElement>(totalCount);
			m_inUsed = new Dictionary<string, SpriteElement>(totalCount);

			for( int i = 0; i < totalCount; i++ ) {
				var x = i % columnCount;
				var y = i / columnCount;
				var rect = new Rect(sizeWithPadding * x, sizeWithPadding * y, spriteSize, spriteSize);
				m_notInUsed.Add(new SpriteElement(this) {
					Sprite = Sprite.Create(m_packTexture, rect, Vector2.one * 0.5f, 100, 1, SpriteMeshType.Tight)
				});
			}
		}

		public int FreeCount => m_freeList.Count;
		public SpriteElement Request(string path) {
			if( m_inUsed.TryGetValue(path, out var element) ) {
				element.Trigger();
				return element;
			}

			if( m_freeList.TryGetValue(path, out element) ) {
				m_freeList.Remove(path);
				m_inUsed.Add(path, element);
				element.Trigger();
				return element;
			}

			var packed = true;
			if( m_notInUsed.Count > 0 ) {
				element = m_notInUsed[^1];
				m_notInUsed.RemoveAt(m_notInUsed.Count - 1);
			}
			else if( m_freeList.Count > 0 ) {
				element = m_freeList.Dequeue();
			}
			else {
				packed = false;
				element = new SpriteElement(null);
			}
			element.Path = path;

			if( packed ) {
				m_inUsed[path] = element;
			}
			
			FileLoader.LoadFileAsync(path + ".bytes", bytes => {
				if( bytes == null ) {
					element.Trigger();
					return;
				}
				var texture = new Texture2D(m_spriteSize, m_spriteSize);
				texture.LoadImage(bytes, false);

				if( m_spriteSize != texture.width || m_spriteSize != texture.height ) {
					Object.Destroy(texture);
					Debug.LogError($"Texture {path} size : {texture.width} x {texture.height} not match request : {m_spriteSize}");
				}
				else {
					if( packed ) {
						var iconColors = texture.GetPixels32();
						var rect = element.Sprite.rect;
						m_packTexture.SetPixels32((int)rect.x, (int)rect.y, m_spriteSize, m_spriteSize, iconColors);
						m_packTexture.Apply();
						Object.Destroy(texture);
					}
					else {
						Debug.LogWarning($"Not empty space for {path}. Create an independent sprite.");
						var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
						element.Sprite = sprite;
					}
					element.Trigger();
				}
			});
			return element;
		}

		private static void ReleaseSprite(IEnumerator<SpriteElement> enumerator) {
			enumerator.Reset();
			while( enumerator.Current != null ) {
				Object.Destroy(enumerator.Current.Sprite);
				enumerator.MoveNext();
			}
		}

		public void Dispose() {
			ReleaseSprite(m_notInUsed.GetEnumerator());
			ReleaseSprite(m_inUsed.Values.GetEnumerator());
			ReleaseSprite(m_freeList);

			Object.Destroy(m_packTexture);
		}

		private void RemoveActive(string key) {
			if( !m_inUsed.TryGetValue(key, out var element) ) {
				return;
			}

			m_inUsed.Remove(key);
			m_freeList.Add(key, element);
		}
	}
}