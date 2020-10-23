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
		private AsyncInstantiateGroup m_group;

		private void OnDestroy() {
			Asset?.Dispose();
			m_arrayData?.Dispose();
			m_arrayData = null;
			m_group.Clear();
		}

		public LuaTable LuaArrayData {
			set {
				m_arrayData?.Dispose();
				m_arrayData = value;
				if( m_arrayData == null )
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
							m_group = new AsyncInstantiateGroup(Asset);
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
			m_group.Clear();
			if( m_arrayData == null ) {
				foreach( var go in m_generatedAsset ) {
					AssetService.Recycle(go);
				}

				m_generatedAsset.Clear();
				return;
			}

			var finalIndex = m_arrayData.Length;
			for( var i = 1; i <= finalIndex; i++ ) {
				if( m_generatedAsset.Count >= i ) {
					var dataContext = m_arrayData.Get<int, LuaTable>(i);
					var go = m_generatedAsset[i - 1];
					var mvvmBinding = go.GetComponent<ILuaMVVM>();
					mvvmBinding.SetDataContext(dataContext);
				}
				else {
					var index = i;
					var context = m_group.InstantiateAsync(transform);
					context.Callback += go => {
						var dataContext = m_arrayData.Get<int, LuaTable>(index);
						go.name = $"{go.name}_{index}";
						m_generatedAsset.Add(go);
						var mvvmBinding = go.GetComponent<ILuaMVVM>();
						mvvmBinding.SetDataContext(dataContext);
					};
				}
			}

			for( var i = finalIndex; i < m_generatedAsset.Count; ) {
				var go = m_generatedAsset[m_generatedAsset.Count - 1];
				AssetService.Recycle(go);
				m_generatedAsset.RemoveAt(m_generatedAsset.Count - 1);
			}
		}

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}
	}
}