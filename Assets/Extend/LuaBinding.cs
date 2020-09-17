using Extend.Common;
using Extend.LuaBindingData;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public class LuaBinding : MonoBehaviour {
		[LuaFileAttribute, BlackList]
		public string LuaFile;

		public LuaTable LuaInstance { get; private set; }

		private void Awake() {
			if( string.IsNullOrEmpty(LuaFile) )
				return;
			var ret = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE).LoadFileAtPath(LuaFile);
			if( !( ret[0] is LuaTable luaClass ) )
				return;
			var constructor = luaClass.Get<LuaBindingClassNew>("new");
			var luaInstance = constructor?.Invoke(gameObject);
			Bind(luaInstance);

			var awake = luaInstance.Get<LuaUnityEventFunction>("awake");
			awake?.Invoke(luaInstance);
		}

		private void Start() {
			var start = LuaInstance.Get<LuaUnityEventFunction>("start");
			start?.Invoke(LuaInstance);
		}

		private void OnDestroy() {
			var destroy = LuaInstance.Get<LuaUnityEventFunction>("destroy");
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

		[BlackList, SerializeReference, HideInInspector]
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