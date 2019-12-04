using Extend.Common;

namespace Extend.AssetService {
	public interface IAssetService : IService, IServiceUpdate {
		AssetReference Load(string path);

		void RemoveAsset(string assetPath);
	}
}