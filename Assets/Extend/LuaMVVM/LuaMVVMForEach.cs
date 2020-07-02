using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	public class LuaMVVMForEach : MonoBehaviour, ILuaMVVM {
		[SerializeField]
		private bool m_syncLoad;
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		private LuaTable m_arrayData;
		private readonly List<GameObject> m_generatedAsset = new List<GameObject>();

		private void OnDestroy() {
			Asset?.Dispose();
			m_arrayData?.Dispose();
			m_arrayData = null;
		}

		public LuaTable LuaArrayData {
			set {
				m_arrayData?.Dispose();
				m_arrayData = value;
				if(m_arrayData == null)
					return;

				if( Asset == null || !Asset.GUIDValid ) {
					var dataCount = m_arrayData.Length;
					if( dataCount > transform.childCount ) {
						Debug.LogError($"Data count greater then children count. {dataCount} < {transform.childCount}");
					}
					for( var i = 0; i < transform.childCount; i++ ) {
						var child = transform.GetChild(i);
						if( i >= dataCount ) {
							child.gameObject.SetActive(false);
						}
						else {
							var mvvm = child.GetComponent<LuaMVVMBinding>();
							mvvm.SetDataContext(m_arrayData.Get<int, LuaTable>(i + 1));
						}
					}
				}
				else {
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
				var dataContext = m_arrayData.Get<int, LuaTable>(i);
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
				
				var mvvmBinding = go.GetComponent<ILuaMVVM>();
				mvvmBinding.SetDataContext(dataContext);
			}

			for( var i = finalIndex - 1; i < m_generatedAsset.Count; ) {
				var go = m_generatedAsset[m_generatedAsset.Count - 1];
				Destroy(go);
				m_generatedAsset.RemoveAt(m_generatedAsset.Count - 1);
			}
		}

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}
	}
}