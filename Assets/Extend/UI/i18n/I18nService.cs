using System;
using System.Collections.Generic;
using System.Xml;
using Extend.Asset;
using Extend.Common;
using UnityEngine;

namespace Extend.UI.i18n {
	public class I18nService : IService {
		public int ServiceType => (int)CSharpServiceManager.ServiceType.I18N;
		private int m_currentLang = -1;
		private string m_currentLangName;
		private string[] m_supportedLang;
		private readonly Dictionary<string, string> m_languageText = new(10240);

		public event Action OnLanguageChanged;

		public void Initialize() {
			RefreshLanguageText();
		}

		private void RefreshLanguageText() {
			var assetService = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			/*using( var assetRef = assetService.Load("Config/static-i18n", typeof(TextAsset)) ) {
				var doc = new XmlDocument();
				doc.LoadXml(assetRef.GetTextAsset().text);
				var rootElement = doc.DocumentElement;
				if( m_supportedLang == null ) {
					var supportLangAttr = rootElement.Attributes["support-lang"];
					m_supportedLang = supportLangAttr.Value.Split(';');
					if( string.IsNullOrEmpty(m_currentLangName) ) {
						m_currentLangName = m_supportedLang[0];
						m_currentLang = 0;
					}
					else {
						m_currentLang = Array.IndexOf(m_supportedLang, m_currentLangName);
					}
				}

				foreach( XmlNode node in rootElement.ChildNodes ) {
					if( node.NodeType != XmlNodeType.Element ) {
						continue;
					}

					var key = node.Attributes["guid"].Value;
					var val = node.Attributes[m_currentLang].Value;
					m_languageText.Add(key, val);
				}
			}*/
		}

		public void ChangeCurrentLanguage(string lang) {
			var selected = Array.IndexOf(m_supportedLang, lang);
			if( selected == -1 ) {
				throw new Exception($"Not supported language : {lang}");
			}

			if( m_currentLang == selected )
				return;

			m_currentLang = selected;
			m_languageText.Clear();
			RefreshLanguageText();
			
			OnLanguageChanged?.Invoke();
		}

		public void Destroy() {
			m_languageText.Clear();
		}

		public string GetText(string key) {
			if( !m_languageText.TryGetValue(key, out var ret) ) {
				Debug.LogWarning($"key {key} not present in static-i18n config");
				ret = string.Empty;
			}
			return string.Intern(ret);
		}
	}
}