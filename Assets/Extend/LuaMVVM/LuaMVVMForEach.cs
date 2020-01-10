using System.Collections.Generic;
using Extend.AssetService;
using Extend.AssetService.Attribute;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	public class LuaMVVMForEach : MonoBehaviour {
		[SerializeField]
		private bool syncLoad;
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		private LuaTable arrayData;
		private readonly List<GameObject> generatedAsset = new List<GameObject>();

		public LuaTable LuaArrayData {
			get => arrayData;
			set {
				arrayData = value;
				if( !Asset.IsFinished && !syncLoad ) {
					var handle = Asset.LoadAsync(typeof(GameObject));
					handle.OnComplete += loadHandle => {
						DoGenerate();
					};
				}
				else {
					DoGenerate();
				}
			}
		}

		private void DoGenerate() {
			foreach( var asset in generatedAsset ) {
				Destroy(asset);
			}
			var prefab = Asset.GetGameObject();
			generatedAsset.Clear();
			for( var i = 1; ; i++ ) {
				var dataContext = LuaArrayData.Get<int, LuaTable>(i);
				if(dataContext == null)
					break;
				var go = Instantiate(prefab, transform, false);
				go.name = $"{prefab.name}_{i}";
				generatedAsset.Add(go);
				var mvvmBinding = go.GetComponent<LuaMVVMBinding>();
				mvvmBinding.SetDataContext(dataContext);
			}
		}
	}
}