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
		private object value;

		public void Start() {
			propertyInfo = BindTarget.GetType().GetProperty(BindTargetProp);
			Assert.IsNotNull(propertyInfo, BindTargetProp);
		}

		public void UpdateToSource() {
			if(dataSource == null)
				return;
			
			if(Mode != BindMode.TWO_WAY && Mode != BindMode.ONE_WAY_TO_SOURCE)
				return;

			var fieldVal = propertyInfo.GetValue(BindTarget);
			if( value != fieldVal ) {
				var mvvm = CSharpServiceManager.Get<LuaMVVM>(CSharpServiceManager.ServiceType.MVVM_SERVICE);
				mvvm.RawSetDataSource(dataSource, Path, value);
				value = fieldVal;
			}
		}

		private void SetPropertyValue(object val) {
			if(Equals(val, LuaMVVM.MVVMPlaceholder))
				return;
			value = val;
			propertyInfo.SetValue(BindTarget, val);
		}

		public void Bind(LuaTable dataContext) {
			dataSource = dataContext;
			if( dataSource == null ) {
				return;
			}

			var val = dataContext.GetInPath<object>(Path);
			if( val == null ) {
				Debug.LogWarning($"Not found value in path {Path}");
				return;
			}

			switch( Mode ) {
				case BindMode.ONE_WAY:
				case BindMode.TWO_WAY:
				case BindMode.ONE_TIME: {
					SetPropertyValue(val);
					if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
						var mvvm = CSharpServiceManager.Get<LuaMVVM>(CSharpServiceManager.ServiceType.MVVM_SERVICE);
						mvvm.SetupBindNotification(dataContext, Path, SetPropertyValue);
						if( Mode == BindMode.TWO_WAY ) {
							value = val;
						}
					}
					break;
				}
				case BindMode.ONE_WAY_TO_SOURCE:
					value = propertyInfo.GetValue(BindTarget);
					dataSource.SetInPath(Path, value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}