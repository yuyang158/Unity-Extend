using Extend.Common;
using TMPro;
using UnityEngine;

namespace Extend.UI.i18n {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class StaticI18nText : MonoBehaviour {
		[SerializeField]
		private string m_key;

		public string Key {
			get => m_key;
			set => m_key = value;
		}
		
		private void Awake() {
			var txt = GetComponent<TextMeshProUGUI>();
			var i18NService = CSharpServiceManager.Get<I18nService>(CSharpServiceManager.ServiceType.I18N);
			txt.text = i18NService.GetText(m_key);
		}
	}
}
