using System.Collections.Generic;
using UnityEngine;

namespace Extend.Asset {
	public class AssetContainer {
		private readonly List<AssetRefObject> m_assets = new List<AssetRefObject>(1024);
		private readonly Dictionary<int, AssetRefObject> m_hashAssetDic = new Dictionary<int, AssetRefObject>();
		private int m_tickIndex;
		private const float MAX_ASSET_ZERO_REF_DURATION = 10;
		private const int SINGLE_FRAME_CHECK_COUNT = 1;

		public void Put(AssetRefObject asset) {
			m_assets.Add(asset);
			m_hashAssetDic.Add(asset.GetHashCode(), asset);	
		}

		public AssetRefObject TryGetAsset(int hash) {
			m_hashAssetDic.TryGetValue(hash, out var assetRef);
			return assetRef;
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

		public void CollectAll() {
			m_tickIndex = 0;
			do {
				Collect(true);
			} 
			while( m_tickIndex != 0 );

			Resources.UnloadUnusedAssets();
		}
	}
}