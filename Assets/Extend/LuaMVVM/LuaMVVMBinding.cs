using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	public class LuaMVVMBinding : MonoBehaviour, ILuaMVVM {
		[LuaMVVMBindOptions]
		public LuaMVVMBindingOptions BindingOptions;

		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized )
				return;
			foreach( var option in BindingOptions.Options ) {
				option.Destroy();
			}
		}

		public void SetDataContext(LuaTable dataSource) {
			foreach( var option in BindingOptions.Options ) {
				option.Bind(dataSource);
			}
		}

		public void Detach() {
			foreach( var option in BindingOptions.Options ) {
				option.TryDetach();
			}
		}

		private void Awake() {
			foreach( var option in BindingOptions.Options ) {
				option.Start();
			}
		}
	}
}