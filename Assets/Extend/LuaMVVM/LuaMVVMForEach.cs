using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using Extend.Common.Lua;
using UnityEngine;

namespace Extend.LuaMVVM {
	public class LuaMVVMForEach : MonoBehaviour {
		[SerializeField]
		private bool m_syncLoad;
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		private ILuaTable m_arrayData;
		private readonly List<GameObject> m_generatedAsset = new List<GameObject>();

		private void OnDestroy() {
			Asset?.Dispose();
			m_arrayData?.Dispose();
			m_arrayData = null;
		}

		public ILuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData?.Dispose();
				m_arrayData = value;
				if(m_arrayData == null)
					return;
				if( !Asset.IsFinished && !m_syncLoad ) {
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
			if( m_arrayData == null ) {
				foreach( var go in m_generatedAsset ) {
					Destroy(go);
				}
				m_generatedAsset.Clear();
				return;
			}
			
			var prefab = Asset.GetGameObject();
			int finalIndex;
			for( var i = 1; ; i++ ) {
				var dataContext = m_arrayData.Get<int, ILuaTable>(i);
				if( dataContext == null ) {
					finalIndex = i;
					break;
				}
				GameObject go;
				if( m_generatedAsset.Count >= i ) {
					go = m_generatedAsset[i - 1];
				}
				else {
					go = Instantiate(prefab, transform, false);
					go.name = $"{prefab.name}_{i}";
					m_generatedAsset.Add(go);
				}
				
				var mvvmBinding = go.GetComponent<LuaMVVMBinding>();
				mvvmBinding.SetDataContext(dataContext);
			}

			for( var i = finalIndex - 1; i < m_generatedAsset.Count; ) {
				var go = m_generatedAsset[m_generatedAsset.Count - 1];
				Destroy(go);
				m_generatedAsset.RemoveAt(m_generatedAsset.Count - 1);
			}
		}
	}
}