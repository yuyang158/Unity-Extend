using System;
using System.Reflection;
using Extend.Common;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using LuaAPI = XLua.LuaDLL.Lua;

namespace Extend.LuaMVVM {
	[Serializable]
	public class LuaMVVMBindingOptions {
		public LuaMVVMBindingOption[] Options;
	}
	
	[Serializable]
	public class LuaMVVMBindingOption {
		public enum BindMode {
			ONE_WAY,
			TWO_WAY,
			ONE_WAY_TO_SOURCE,
			ONE_TIME
		}

		public UnityEngine.Object BindTarget;
		public string BindTargetProp;

		public BindMode Mode;
		public string Path;

		private LuaTable dataSource;
		private PropertyInfo propertyInfo;

		public void Start() {
			propertyInfo = BindTarget.GetType().GetProperty(BindTargetProp);
			Assert.IsNotNull(propertyInfo, BindTargetProp);
		}

		public void UpdateToSource() {
			if(dataSource == null)
				return;
			
			if(Mode != BindMode.TWO_WAY && Mode != BindMode.ONE_WAY_TO_SOURCE)
				return;

			var val = dataSource.GetInPath<object>(Path);
			var fieldVal = propertyInfo.GetValue(BindTarget);
			if( val != fieldVal ) {
				dataSource.SetInPath(Path, fieldVal);
			}
		}

		private void SetPropertyValue(object val) {
			propertyInfo.SetValue(BindTarget, val);
		}

		public void Bind(LuaTable dataContext) {
			dataSource = dataContext;
			if( dataSource == null ) {
				return;
			}

			var val = dataContext.GetInPath<object>(Path);
			if( val == null ) {
				Debug.LogError($"Not found value in path {Path}");
				return;
			}

			if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY || Mode == BindMode.ONE_TIME ) {
				propertyInfo.SetValue(BindTarget, val);
				if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
					var mvvm = CSharpServiceManager.Get<LuaMVVM>(CSharpServiceManager.ServiceType.MVVM_SERVICE);
					mvvm.SetupBindNotification(dataContext, Path, SetPropertyValue);
				}
			}
			else if( Mode == BindMode.ONE_WAY_TO_SOURCE ) {
				dataSource.SetInPath(Path, propertyInfo.GetValue(BindTarget));
			}
		}
	}
}