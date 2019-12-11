using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

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
		private FieldInfo fieldInfo;

		public void Start() {
			fieldInfo = BindTarget.GetType().GetField(BindTargetProp, BindingFlags.Instance | BindingFlags.Public);
			Assert.IsNotNull(fieldInfo, BindTargetProp);
		}

		public void UpdateToSource() {
			if(dataSource == null)
				return;
			
			if(Mode != BindMode.TWO_WAY && Mode != BindMode.ONE_WAY_TO_SOURCE)
				return;

			var val = dataSource.GetInPath<object>(Path);
			var fieldVal = fieldInfo.GetValue(BindTarget);
			if( val != fieldVal ) {
				dataSource.SetInPath(Path, fieldVal);
			}
		}

		public void Bind(LuaTable dataContext) {
			dataSource = dataContext;
			if( dataSource == null ) {
				return;
			}

			var val = dataContext.GetInPath<object>(Path);
			if( val == null ) {
				Debug.LogError($"Not found value in path {Path}");
			}

			if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY || Mode == BindMode.ONE_TIME ) {
				fieldInfo.SetValue(BindTarget, val);
			}
			else if( Mode == BindMode.ONE_WAY_TO_SOURCE ) {
				dataSource.SetInPath(Path, fieldInfo.GetValue(BindTarget));
			}
		}
	}
}