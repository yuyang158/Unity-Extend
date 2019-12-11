using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[CSharpCallLua]
	public class LuaMVVM : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.MVVM_SERVICE;

		private delegate void SetupLuaNotification(LuaTable t, string key, Action<object> callback);

		private SetupLuaNotification setupLuaNotification;
		public void Initialize() {
			var ret = LuaVM.Default.LoadFileAtPath("mvvm");
			var module = ret[0] as LuaTable;
			setupLuaNotification = module.GetInPath<SetupLuaNotification>("SetupLuaNotification");
		}

		public void SetupBindNotification(LuaTable dataSource, string key, Action<object> callback) {
			if( string.IsNullOrEmpty(key) ) {
				Debug.LogError($"Bind key error : {key}");
				return;
			}
			setupLuaNotification(dataSource, key, callback);
		}

		public void Destroy() {
		}
	}
}