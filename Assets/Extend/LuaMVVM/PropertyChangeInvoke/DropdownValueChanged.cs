using Extend.LuaUtil;
using TMPro;
using UnityEngine;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	[RequireComponent(typeof(TMP_Dropdown))]
	public class DropdownValueChanged : MonoBehaviour, IUnityPropertyChanged {
		private TMP_Dropdown m_dropdown;
		private void Awake() {
			m_dropdown = GetComponent<TMP_Dropdown>();
			m_dropdown.onValueChanged.AddListener((value) => {
				OnPropertyChanged?.Invoke(m_dropdown, value);
			});
		}

		public event PropertyChangedAction OnPropertyChanged;
		public object ProvideCurrentValue() {
			return m_dropdown.value;
		}
	}
}