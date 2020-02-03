using System.Collections.Generic;
using UnityEngine;

namespace Extend.AssetService {
	public class AssetContainer {
		private readonly List<AssetRefObject> assets = new List<AssetRefObject>(1024);
		private readonly Dictionary<int, AssetRefObject> hashAssetDic = new Dictionary<int, AssetRefObject>();
		private int tickIndex;
		private const float MAX_ASSET_ZERO_REF_DURATION = 10;
		private const int SINGLE_FRAME_CHECK_COUNT = 1;

		public void Put(AssetRefObject asset) {
			assets.Add(asset);
			hashAssetDic.Add(asset.GetHashCode(), asset);	
		}

		public AssetRefObject TryGetAsset(int hash) {
			hashAssetDic.TryGetValue(hash, out var assetRef);
			return assetRef;
		}

		public void Collect(bool ignoreTime = false) {
			for( var i = tickIndex; i < tickIndex + SINGLE_FRAME_CHECK_COUNT; i++ ) {
				if( i >= assets.Count ) {
					tickIndex = 0;
					return;
				}

				var asset = assets[i];
				if( asset.Status == AssetRefObject.AssetStatus.DONE && asset.GetRefCount() <= 0 &&
				    ( ignoreTime || Time.time - asset.ZeroRefTimeStart > MAX_ASSET_ZERO_REF_DURATION ) ) {
					asset.Destroy();
					var last = assets[assets.Count - 1];
					assets[i] = last;
					assets.RemoveAt(assets.Count - 1);
					hashAssetDic.Remove(asset.GetHashCode());
					i--;
				}
			}

			tickIndex += SINGLE_FRAME_CHECK_COUNT;
		}

		public void CollectAll() {
			tickIndex = 0;
			do {
				Collect();
			} 
			while( tickIndex != 0 );
		}
	}
}