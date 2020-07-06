using Extend.LuaUtil;
using TMPro;
using UnityEngine;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	[RequireComponent(typeof(TMP_InputField))]
	public sealed class TMP_InputTextChanged : MonoBehaviour, IUnityPropertyChanged {
		public event PropertyChangedAction OnPropertyChanged;
		public object ProvideCurrentValue() {
			return m_inputField.text;
		}
		private TMP_InputField m_inputField;

		private void Awake() {
			m_inputField = GetComponent<TMP_InputField>();
			m_inputField.onValueChanged.AddListener(val => {
				OnPropertyChanged?.Invoke(m_inputField, val);
			});
		}
	}
}