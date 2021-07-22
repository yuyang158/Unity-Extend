using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	[LuaCallCSharp]
	public class LuaMVVMBinding : MonoBehaviour, ILuaMVVM {
		[LuaMVVMBindOptions, BlackList]
		public LuaMVVMBindingOptions BindingOptions;

		[TextArea(3, 5)]
		public string ExtraInfo;

		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized )
				return;
			DataSource?.Dispose();
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
			m_dataSource?.Dispose();
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
			BindingOptions.Sort();
			foreach( var option in BindingOptions.Options ) {
				option.Prepare(gameObject);
			}
		}
	}
}