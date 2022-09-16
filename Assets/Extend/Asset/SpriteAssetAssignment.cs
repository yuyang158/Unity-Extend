using UnityEngine;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp]
	public abstract class SpriteAssetAssignment : MonoBehaviour {
		public bool Sync;
		private SpriteAssetService.SpriteLoadingHandle m_loadingHandle;
		private string m_spritePath;

		public string SpritePath {
			get => m_spritePath;
			set {
				if( m_spritePath == value )
					return;
				ApplyNewKey(value);
			}
		}

		protected void ApplyNewKey(string key) {
			PreLoad();
			m_loadingHandle?.GiveUp();
			m_loadingHandle = null;

			m_spritePath = key;
			if( string.IsNullOrEmpty(m_spritePath) ) {
				Apply(null);
			}
			else {
				m_loadingHandle = SpriteAssetService.Get().RequestSprite(this, m_spritePath, Sync);
			}
		}

		protected abstract void PreLoad();
		protected abstract void PostLoad();

		public virtual void Apply(Sprite sprite) {
			PostLoad();
		}

		private void OnDestroy() {
			m_loadingHandle?.GiveUp();
			if( string.IsNullOrEmpty(m_spritePath) )
				return;
			
			
#if UNITY_EDITOR
			if( !Common.CSharpServiceManager.Initialized ) {
				return;
			}
#endif
			m_spritePath = null;
		}
	}
}