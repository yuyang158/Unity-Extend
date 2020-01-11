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
		
		private delegate LuaTable MVVMGetArrayElement(LuaTable self, object index);
		[CSharpCallLua]
		private MVVMGetArrayElement mvvmGet;
		
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

				mvvmGet = arrayData.Get<MVVMGetArrayElement>("get");
			}
		}

		private void DoGenerate() {
			var prefab = Asset.GetGameObject();
			int finalIndex;
			for( var i = 1; ; i++ ) {
				var dataContext = mvvmGet(arrayData, i);
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
				generatedAsset.RemoveAt(generatedAsset.Count - 1);
			}
		}
	}
}