using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.Asset {
	[Serializable]
	public enum BundleUnloadStrategy : byte {
		Normal,
		DontUnload
	}

	public class AssetContainer {
		private readonly List<AssetRefObject> m_assets = new List<AssetRefObject>(1024);
		private readonly Dictionary<int, AssetRefObject> m_hashAssetDic = new Dictionary<int, AssetRefObject>(1024);
		private int m_checkStartIndex;
		private const float MAX_ASSET_ZERO_REF_DURATION = 10;
		private const int SINGLE_FRAME_CHECK_COUNT = 1;

		public AssetContainer() {
			Application.lowMemory += () => { Collect(true); };
		}

		public void Put(AssetRefObject asset) {
			m_assets.Add(asset);

			if( m_hashAssetDic.ContainsKey(asset.GetHashCode()) ) {
				Debug.LogError("An asset with same hash code has already been add.");
				return;
			}

			m_hashAssetDic.Add(asset.GetHashCode(), asset);
		}

		public AssetRefObject TryGetAsset(int hash) {
			m_hashAssetDic.TryGetValue(hash, out var assetRef);
			return assetRef;
		}

		public void DirectUnload(int hash) {
			var asset = m_hashAssetDic[hash];
			Assert.IsNotNull(asset);
			asset.Destroy();
			m_hashAssetDic.Remove(hash);
		}

		public void Collect(bool ignoreTime = false) {
			for( var i = m_checkStartIndex; i < m_checkStartIndex + SINGLE_FRAME_CHECK_COUNT; ) {
				if( i >= m_assets.Count ) {
					m_checkStartIndex = 0;
					return;
				}

				var asset = m_assets[i];
				if( asset.Status == AssetRefObject.AssetStatus.DONE && asset.GetRefCount() <= 0 &&
				    ( ignoreTime || Time.time - asset.ZeroRefTimeStart > MAX_ASSET_ZERO_REF_DURATION ) ) {
#if ASSET_LOG
					Debug.LogWarning("Release : " + asset.DebugNameCache);
#endif
					try {
						asset.Destroy();
					}
					catch( Exception e ) {
						Debug.LogException(e);
					}

					m_assets.RemoveSwapAt(i);
					m_hashAssetDic.Remove(asset.GetHashCode());
				}
				else {
					i++;
				}
			}

			m_checkStartIndex += SINGLE_FRAME_CHECK_COUNT;
		}

		public void FullCollect() {
			m_checkStartIndex = 0;
			do {
				Collect(true);
			} while( m_checkStartIndex != 0 );

			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		public void Clear() {
			m_assets.Clear();
			m_hashAssetDic.Clear();
		}

		public void Dump() {
			foreach( var asset in m_hashAssetDic.Values ) {
				StatService.Get().LogStat("AssetDump", asset.ToString(), "");
			}
		}
	}
}