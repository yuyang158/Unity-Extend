using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using LuaAPI = XLua.LuaDLL.Lua;
using Object = UnityEngine.Object;

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

		public Object BindTarget;
		public string BindTargetProp;

		public BindMode Mode = BindMode.ONE_TIME;
		public string Path;

		private LuaTable dataSource;
		private PropertyInfo propertyInfo;
		private object value;

		private delegate void WatchCallback(LuaTable self, object val);
		private delegate void Detach(LuaTable self, string path, WatchCallback callback);

		[CSharpCallLua]
		private WatchCallback watchCallback;
		
		[CSharpCallLua]
		private Detach detach;

		public void Start() {
			propertyInfo = BindTarget.GetType().GetProperty(BindTargetProp);
			Assert.IsNotNull(propertyInfo, BindTargetProp);
			watchCallback = SetPropertyValue;
		}

		public void Destroy() {
			TryDetach();
		}

		public void UpdateToSource() {
			if( dataSource == null )
				return;

			if( Mode != BindMode.TWO_WAY && Mode != BindMode.ONE_WAY_TO_SOURCE )
				return;

			var fieldVal = propertyInfo.GetValue(BindTarget);
			if( !Equals(value, fieldVal) ) {
				dataSource.SetInPath(Path, value);
				value = fieldVal;
			}
		}

		private void SetPropertyValue(LuaTable _, object val) {
			if( propertyInfo.PropertyType == typeof(string) ) {
				value = val == null ? "" : val.ToString();
			}
			else {
				value = val;
			}
			propertyInfo.SetValue(BindTarget, value);
		}

		private void TryDetach() {
			if( detach != null && Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
				detach(dataSource, Path, watchCallback);
				detach = null;
			}
		}

		public void Bind(LuaTable dataContext) {
			if( dataSource != null ) {
				TryDetach();
			}
			dataSource = dataContext;
			if( dataSource == null ) {
				return;
			}

			var watch = dataContext.GetInPath<LuaFunction>("watch");
			var val = dataContext.GetInPath<object>(Path);
			if( val == null ) {
				Debug.LogWarning($"Not found value in path {Path}");
				return;
			}

			switch( Mode ) {
				case BindMode.ONE_WAY:
				case BindMode.TWO_WAY:
				case BindMode.ONE_TIME: {
					SetPropertyValue(dataContext, val);
					if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
						watch.Call(dataContext, Path, watchCallback);
						detach = dataContext.Get<Detach>("detach");
						if( Mode == BindMode.TWO_WAY ) {
							value = val;
						}
					}
					break;
				}
				case BindMode.ONE_WAY_TO_SOURCE:
					value = propertyInfo.GetValue(BindTarget);
					dataContext.SetInPath(Path, value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}