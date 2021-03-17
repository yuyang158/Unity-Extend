using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	[RequireComponent(typeof(LuaBinding)), LuaCallCSharp]
	public class LuaTriggerPropertyChanged : MonoBehaviour, IUnityPropertyChanged {
		public event PropertyChangedAction OnPropertyChanged;
		public object ProvideCurrentValue() {
			return m_provideValueMethod.Invoke(m_binding.LuaInstance);
		}

		public void Trigger(object value) {
			OnPropertyChanged?.Invoke(this, value);
		}

		private LuaBinding m_binding;
		private GetLuaValue m_provideValueMethod;
		private void Awake() {
			m_binding = GetComponent<LuaBinding>();
			m_provideValueMethod = m_binding.CachedClass.GetLuaMethod<GetLuaValue>("ProvideCurrentValue");
		}
	}
}