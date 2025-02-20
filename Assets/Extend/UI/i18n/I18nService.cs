using Extend.Common;
using Newtonsoft.Json.Linq;
using UnityEngine;
using XLua;

namespace Extend.UI.i18n {
	[LuaCallCSharp]
	public class I18nService : IService {
		public int ServiceType => (int)CSharpServiceManager.ServiceType.I18N;
		private string m_currentLang = "cn";
		private readonly JObject m_i18nContent;

		public static I18nService Get() {
			return CSharpServiceManager.Get<I18nService>(CSharpServiceManager.ServiceType.I18N);
		}

		public I18nService(JObject i18nContent) {
			m_i18nContent = i18nContent;
		}

		public void Initialize() {
		}

		public void ChangeCurrentLanguage(string lang) {
			m_currentLang = lang;
			Debug.LogWarning($"Static text language : {lang}");
		}

		public void Destroy() {
		}

		public bool GetText(string key, out string text) {
			if( !m_i18nContent.TryGetValue(key, out var ret) ) {
				text = string.Empty;
				return false;
			}

			text = ret[m_currentLang].ToString().Replace("\\n", "\n");
			return true;
		}
	}
}
