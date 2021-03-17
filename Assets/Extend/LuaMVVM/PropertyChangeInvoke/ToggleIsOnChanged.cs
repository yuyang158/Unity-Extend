using Extend.LuaUtil;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	[RequireComponent(typeof(Toggle))]
	public class ToggleIsOnChanged : MonoBehaviour, IUnityPropertyChanged {
		public event PropertyChangedAction OnPropertyChanged;
		private Toggle m_toggle;
		public object ProvideCurrentValue() {
			return m_toggle.isOn;
		}

		private void Awake() {
			m_toggle = GetComponent<Toggle>();
			m_toggle.onValueChanged.AddListener((val) => {
				OnPropertyChanged?.Invoke(m_toggle, val);
			});
		}
	}
}