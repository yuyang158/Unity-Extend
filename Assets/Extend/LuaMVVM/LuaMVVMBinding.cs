using Extend.Common.Lua;
using UnityEngine;

namespace Extend.LuaMVVM {
	public class LuaMVVMBinding : MonoBehaviour {
		[LuaMVVMBindOptions]
		public LuaMVVMBindingOptions BindingOptions;

		private void OnDestroy() {
			foreach( var option in BindingOptions.Options ) {
				option.Destroy();
			}
		}

		public void SetDataContext(ILuaTable dataSource) {
			foreach( var option in BindingOptions.Options ) {
				option.Bind(dataSource);
			}
		}

		private void Awake() {
			foreach( var option in BindingOptions.Options ) {
				option.Start();
			}
		}

		private void Update() {
			foreach( var option in BindingOptions.Options ) {
				option.UpdateToSource();
			}
		}
	}
}