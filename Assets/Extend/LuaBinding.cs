using Extend.Common;
using Extend.LuaBindingData;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public sealed class LuaBinding : MonoBehaviour {
		[LuaFileAttribute, BlackList]
		public string LuaFile;

		public LuaTable LuaInstance { get; private set; }
		public LuaTable LuaClass { get; private set; }

		private LuaClassCache.LuaClass m_cachedClass;
		public LuaClassCache.LuaClass CachedClass => m_cachedClass;

		private void Awake() {
			if( string.IsNullOrEmpty(LuaFile) )
				return;
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var ret = luaVM.LoadFileAtPath(LuaFile);
			if( !( ret[0] is LuaTable klass ) )
				return;
			LuaClass = klass;
			m_cachedClass = luaVM.GetLuaClass(klass);
			
			var constructor = m_cachedClass.GetLuaMethod<LuaBindingClassNew>("new");
			var luaInstance = constructor?.Invoke(gameObject);
			Bind(luaInstance);

			var awake = m_cachedClass.GetLuaMethod<LuaUnityEventFunction>("awake");
			awake?.Invoke(luaInstance);
		}

		private void Start() {
			var start = m_cachedClass.GetLuaMethod<LuaUnityEventFunction>("start");
			start?.Invoke(LuaInstance);
		}

		private void OnDestroy() {
			var destroy = m_cachedClass.GetLuaMethod<LuaUnityEventFunction>("destroy");
			destroy?.Invoke(LuaInstance);
			LuaInstance?.Dispose();
			LuaInstance = null;

			foreach( var binding in LuaData ) {
				binding.Destroy();
			}
		}

		public void Bind(LuaTable instance) {
			LuaInstance = instance;
			LuaInstance.SetInPath("__CSBinding", this);
			foreach( var binding in LuaData ) {
				binding.ApplyToLuaInstance(instance);
			}
		}

		[BlackList, HideInInspector, SerializeReference]
		public LuaBindingDataBase[] LuaData;

		[Button(ButtonSize.Small)]
		public void Sync() {
			if( !Application.isPlaying )
				return;
			foreach( var binding in LuaData ) {
				binding.ApplyToLuaInstance(LuaInstance);
			}
		}
	}
}