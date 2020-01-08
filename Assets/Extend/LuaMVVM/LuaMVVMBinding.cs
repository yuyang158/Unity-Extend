using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	public class LuaMVVMBinding : MonoBehaviour {
		[LuaMVVMBindOptions]
		public LuaMVVMBindingOptions BindingOptions;
		
		public void SetDataContext(LuaTable dataSource) {
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