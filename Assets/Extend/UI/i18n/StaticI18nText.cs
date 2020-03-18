using Extend.Common;
using TMPro;
using UnityEngine;

namespace UI.i18n {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class StaticI18nText : MonoBehaviour {
		[SerializeField]
		private string key;
		
		private void Awake() {
			var txt = GetComponent<TextMeshProUGUI>();
			var i18NService = CSharpServiceManager.Get<I18nService>(CSharpServiceManager.ServiceType.I18N);
			txt.text = i18NService.GetText(key);
		}
	}
}