using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp]
	public abstract class SpriteAssetAssignment : MonoBehaviour {
		public bool Sync;
		private string m_spriteKey;
		private SpriteAssetService.SpriteLoadingHandle m_loadingHandle;

		protected void ApplyNewKey(string key) {
			m_loadingHandle?.GiveUp();
			m_loadingHandle = null;
			if( !string.IsNullOrEmpty(m_spriteKey) ) {
				SpriteAssetService.Get().Release(m_spriteKey);
			}

			m_spriteKey = key;
			if( string.IsNullOrEmpty(m_spriteKey) ) {
				Apply(null);
			}
			else {
				m_loadingHandle = SpriteAssetService.Get().SetUIImage(this, m_spriteKey, Sync);
			}
		}

		public abstract void Apply(Sprite sprite);

		private void OnDestroy() {
			m_loadingHandle?.GiveUp();
			if( string.IsNullOrEmpty(m_spriteKey) )
				return;
			SpriteAssetService.Get().Release(m_spriteKey);
			m_spriteKey = null;
		}
	}
}