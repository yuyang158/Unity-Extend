using Extend.LuaUtil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	[RequireComponent(typeof(InputField))]
	public sealed class UGUI_InputTextChanged : MonoBehaviour, IUnityPropertyChanged {
		public event PropertyChangedAction OnPropertyChanged;
		public object ProvideCurrentValue() {
			return m_inputField.text;
		}
		private InputField m_inputField;

		private void Awake() {
			m_inputField = GetComponent<InputField>();
			m_inputField.onValueChanged.AddListener(val => {
				OnPropertyChanged?.Invoke(m_inputField, val);
			});
		}
	}
}