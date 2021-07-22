using Extend.Common;
using Extend.UI.Attributes;
using TMPro;
using UnityEngine;

namespace Extend.UI.i18n {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class StaticI18nText : MonoBehaviour {
		[SerializeField, StaticI18nKey]
		private string m_key;
		
		private void Awake() {
			// var txt = GetComponent<TextMeshProUGUI>();
			// var i18NService = CSharpServiceManager.Get<I18nService>(CSharpServiceManager.ServiceType.I18N);
			// txt.text = i18NService.GetText(m_key);
		}
	}
}