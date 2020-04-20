using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
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

		private void OnDestroy() {
			Asset?.Dispose();
			arrayData?.Dispose();
			arrayData = null;
		}

		public LuaTable LuaArrayData {
			get => arrayData;
			set {
				arrayData?.Dispose();
				arrayData = value;
				if(arrayData == null)
					return;
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
			if( arrayData == null ) {
				foreach( var go in generatedAsset ) {
					Destroy(go);
				}
				generatedAsset.Clear();
				return;
			}
			
			var prefab = Asset.GetGameObject();
			int finalIndex;
			for( var i = 1; ; i++ ) {
				var dataContext = arrayData.Get<int, LuaTable>(i);
				if( dataContext == null ) {
					finalIndex = i;
					break;
				}
				GameObject go;
				if( generatedAsset.Count >= i ) {
					go = generatedAsset[i - 1];
				}
				else {
					go = Instantiate(prefab, transform, false);
					go.name = $"{prefab.name}_{i}";
					generatedAsset.Add(go);
				}
				
				var mvvmBinding = go.GetComponent<LuaMVVMBinding>();
				mvvmBinding.SetDataContext(dataContext);
			}

			for( var i = finalIndex - 1; i < generatedAsset.Count; ) {
				var go = generatedAsset[generatedAsset.Count - 1];
				Destroy(go);
				generatedAsset.RemoveAt(generatedAsset.Count - 1);
			}
		}
	}
}