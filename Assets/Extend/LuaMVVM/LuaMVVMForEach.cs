using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[LuaCallCSharp]
	public class LuaMVVMForEach : MonoBehaviour, ILuaMVVM, IMVVMAssetReference {
		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		private LuaMVVMScrollViewComponent m_component;

		private void Awake() {
			m_component = new LuaMVVMScrollViewComponent(Asset, transform);
		}

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}

		public LuaTable GetDataContext() {
			return LuaArrayData;
		}

		public LuaTable LuaArrayData {
			get => m_component.LuaArrayData;
			set => m_component.LuaArrayData = value;
		}

		private void OnDestroy() {
			m_component.OnDestroy();
		}

		[BlackList]
		public void Detach() {
			LuaArrayData = null;
		}

		public AssetReference GetMVVMReference() {
			return Asset;
		}
	}
}