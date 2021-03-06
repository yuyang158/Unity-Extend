﻿using System;
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

		public LuaClassCache.LuaClass CachedClass { get; private set; }

		private void Awake() {
			if( string.IsNullOrEmpty(LuaFile) )
				return;
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var ret = luaVM.LoadFileAtPath(LuaFile);
			if( !( ret[0] is LuaTable klass ) )
				return;
			LuaClass = klass;
			CachedClass = luaVM.GetLuaClass(klass);
			
			var constructor = CachedClass.GetLuaMethod<LuaBindingClassNew>("new");
			var luaInstance = constructor?.Invoke(gameObject);
			Bind(luaInstance);

			var awake = CachedClass.GetLuaMethod<LuaUnityEventFunction>("awake");
			awake?.Invoke(luaInstance);
		}

		private void Start() {
			var start = CachedClass.GetLuaMethod<LuaUnityEventFunction>("start");
			start?.Invoke(LuaInstance);
		}

		private void OnDestroy() {
			if(!CSharpServiceManager.Initialized)
				return;
			var destroy = CachedClass.GetLuaMethod<LuaUnityEventFunction>("destroy");
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

		[BlackList]
		public T GetLuaMethod<T>(string methodName) where T : Delegate {
			return CachedClass.GetLuaMethod<T>(methodName);
		}
	}
}