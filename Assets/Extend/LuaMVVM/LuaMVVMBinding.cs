using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[LuaCallCSharp]
	public class LuaMVVMBinding : MonoBehaviour, ILuaMVVM {
		[LuaMVVMBindOptions, BlackList]
		public LuaMVVMBindingOptions BindingOptions;

		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized )
				return;
			foreach( var option in BindingOptions.Options ) {
				option.Destroy();
			}
		}

		private LuaTable m_dataSource;

		public LuaTable DataSource {
			get => m_dataSource;
			set => SetDataContext(value);
		}
		
		public void SetDataContext(LuaTable dataSource) {
			m_dataSource = dataSource;
			foreach( var option in BindingOptions.Options ) {
				option.Bind(dataSource);
			}
		}

		public LuaTable GetDataContext() {
			return m_dataSource;
		}

		[BlackList]
		public void Detach() {
			foreach( var option in BindingOptions.Options ) {
				option.TryDetach();
			}
		}

		private void Awake() {
			foreach( var option in BindingOptions.Options ) {
				option.Prepare(gameObject);
			}
		}
	}
}