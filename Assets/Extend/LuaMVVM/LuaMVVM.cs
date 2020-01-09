using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[CSharpCallLua]
	public class LuaMVVM : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.MVVM_SERVICE;

		private delegate void SetupLuaNotification(LuaTable t, string key, Action<object> callback);
		private delegate void RawSetLuaDataSource(LuaTable t, string key, object val);

		private SetupLuaNotification setupLuaNotification;
		private RawSetLuaDataSource rawSetLuaDataSource;
		
		public static LuaTable MVVMPlaceholder { private set; get; }
		
		public void Initialize() {
			var ret = LuaVM.Default.LoadFileAtPath("mvvm/mvvm");
			var module = ret[0] as LuaTable;
			setupLuaNotification = module.GetInPath<SetupLuaNotification>("SetupLuaNotification");
			rawSetLuaDataSource = module.GetInPath<RawSetLuaDataSource>("RawSetLuaDataSource");

			MVVMPlaceholder = module.GetInPath<LuaTable>("placeholder");
		}

		public void SetupBindNotification(LuaTable dataSource, string key, Action<object> callback) {
			if( string.IsNullOrEmpty(key) ) {
				Debug.LogError($"Bind key error : {key}");
				return;
			}
			setupLuaNotification(dataSource, key, callback);
		}

		public void RawSetDataSource(LuaTable dataSource, string key, object val) {
			rawSetLuaDataSource(dataSource, key, val);
		}

		public void Destroy() {
		}
	}
}