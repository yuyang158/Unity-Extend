using System;
using System.IO;
using Extend.Asset;
using Extend.Common;
using Extend.LuaBindingData;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public sealed class LuaBinding : MonoBehaviour, IRecyclable {
		[LuaFileAttribute, BlackList]
		public string LuaFile;

		public LuaTable LuaInstance { get; set; }

		private LuaTable m_luaClass;

		public LuaTable LuaClass {
			get => m_luaClass;
			set {
				m_luaClass = value;
				var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				CachedClass = luaVM.GetLuaClass(m_luaClass);
			}
		}

		public LuaClassCache.LuaClass CachedClass { get; private set; }

		private void Awake() {
			if( string.IsNullOrEmpty(LuaFile) )
				return;
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var ret = luaVM.LoadFileAtPath(LuaFile);
			if( ret[0] is not LuaTable klass )
				return;
			LuaClass = klass;

			var constructor = CachedClass.GetLuaMethod<LuaBindingClassNew>("new");
			var luaInstance = constructor?.Invoke(gameObject);
			luaInstance.SetInPath("name", Path.GetFileName(LuaFile[( LuaFile.LastIndexOf('.') + 1 )..]));
			luaInstance.SetInPath("fullname", LuaFile);
			Bind(luaInstance);

			var awake = CachedClass.GetLuaMethod<LuaUnityEventFunction>("awake");
			awake?.Invoke(luaInstance);
		}

		public void Start() {
			var start = CachedClass.GetLuaMethod<LuaUnityEventFunction>("start");
			start?.Invoke(LuaInstance);
		}

		public void OnRecycle() {
			var recycle = CachedClass.GetLuaMethod<LuaUnityEventFunction>("recycle");
			recycle?.Invoke(LuaInstance);
		}

		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized )
				return;

			var destroy = CachedClass.GetLuaMethod<LuaUnityEventFunction>("destroy");
			destroy?.Invoke(LuaInstance);
#if UNITY_DEBUG
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			LuaInstance.SetMetaTable(luaVm.DestroyedTableMeta);
#endif
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

		[BlackList]
		public T GetLuaMethod<T>(string methodName) where T : Delegate {
			return CachedClass.GetLuaMethod<T>(methodName);
		}
	}
}