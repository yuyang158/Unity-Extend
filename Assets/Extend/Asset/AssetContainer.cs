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
		private int m_tickIndex;
		private const float MAX_ASSET_ZERO_REF_DURATION = 10;
		private const int SINGLE_FRAME_CHECK_COUNT = 1;
		private readonly Dictionary<string, BundleUnloadStrategy> m_abStrategy = new Dictionary<string, BundleUnloadStrategy>();

		public void Put(AssetRefObject asset) {
			m_assets.Add(asset);

			if( m_hashAssetDic.ContainsKey(asset.GetHashCode()) ) {
				Debug.LogError("An asset with same hash code has already been add.");
				return;
			}
			m_hashAssetDic.Add(asset.GetHashCode(), asset);
		}

		public void PutAB(AssetBundleInstance ab) {
			if( m_abStrategy.TryGetValue(ab.ABPath, out var strategy) ) {
				if( strategy == BundleUnloadStrategy.Normal ) {
					m_assets.Add(ab);
				}
			}
			else {
				m_assets.Add(ab);
			}

			m_hashAssetDic.Add(ab.GetHashCode(), ab);
		}

		public void AddAssetBundleStrategy(string path, BundleUnloadStrategy strategy) {
			if( m_abStrategy.ContainsKey(path) )
				return;

			m_abStrategy.Add(path, strategy);
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
			for( var i = m_tickIndex; i < m_tickIndex + SINGLE_FRAME_CHECK_COUNT; i++ ) {
				if( i >= m_assets.Count ) {
					m_tickIndex = 0;
					return;
				}

				var asset = m_assets[i];
				if( asset.Status == AssetRefObject.AssetStatus.DONE && asset.GetRefCount() <= 0 &&
				    ( ignoreTime || Time.time - asset.ZeroRefTimeStart > MAX_ASSET_ZERO_REF_DURATION ) ) {
					asset.Destroy();
					var last = m_assets[m_assets.Count - 1];
					m_assets[i] = last;
					m_assets.RemoveAt(m_assets.Count - 1);
					m_hashAssetDic.Remove(asset.GetHashCode());
					i--;
				}
			}

			m_tickIndex += SINGLE_FRAME_CHECK_COUNT;
		}

		public void FullCollect() {
			m_tickIndex = 0;
			do {
				Collect(true);
			} while( m_tickIndex != 0 );

			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		public void Clear() {
			foreach( var asset in m_assets ) {
				asset.Destroy();
			}
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