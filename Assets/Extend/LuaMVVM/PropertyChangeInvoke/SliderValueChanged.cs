using Extend.LuaUtil;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	[RequireComponent(typeof(Slider))]
	public class SliderValueChanged : MonoBehaviour, IUnityPropertyChanged {
		public event PropertyChangedAction OnPropertyChanged;
		public object ProvideCurrentValue() {
			return m_slider.value;
		}

		private Slider m_slider;

		private void Awake() {
			m_slider = GetComponent<Slider>();
			m_slider.onValueChanged.AddListener((val) => {
				OnPropertyChanged?.Invoke(m_slider, val);
			});
		}
	}
}