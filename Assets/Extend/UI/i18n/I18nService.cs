using System;
using System.Collections.Generic;
using System.Xml;
using Extend.AssetService;
using Extend.Common;
using UnityEngine;

namespace UI.i18n {
	public class I18nService : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.I18N;
		private int currentLang = -1;
		private string currentLangName;
		private string[] supportedLang;
		private readonly Dictionary<string, string> languageText = new Dictionary<string, string>(10240);

		public event Action OnLanguageChanged;

		public void Initialize() {
			RefreshLanguageText();
		}

		private void RefreshLanguageText() {
			var assetService = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			using( var assetRef = assetService.Load("Config/static-i18n", typeof(TextAsset)) ) {
				var doc = new XmlDocument();
				doc.LoadXml(assetRef.GetTextAsset().text);
				var rootElement = doc.DocumentElement;
				if( supportedLang == null ) {
					var supportLangAttr = rootElement.Attributes["support-lang"];
					supportedLang = supportLangAttr.Value.Split(';');
					if( string.IsNullOrEmpty(currentLangName) ) {
						currentLangName = supportedLang[0];
						currentLang = 0;
					}
					else {
						currentLang = Array.IndexOf(supportedLang, currentLangName);
					}
				}

				foreach( XmlNode node in rootElement.ChildNodes ) {
					if( node.NodeType != XmlNodeType.Element ) {
						continue;
					}

					var key = node.Name;
					var val = node.Attributes[currentLang].Value;
					languageText.Add(key, val);
				}
			}
		}

		public void ChangeCurrentLanguage(string lang) {
			var selected = Array.IndexOf(supportedLang, lang);
			if( selected == -1 ) {
				throw new Exception($"Not supported language : {lang}");
			}

			if( currentLang == selected )
				return;

			currentLang = selected;
			languageText.Clear();
			RefreshLanguageText();
			
			OnLanguageChanged?.Invoke();
		}

		public void Destroy() {
			languageText.Clear();
		}

		public string GetText(string key) {
			if( !languageText.TryGetValue(key, out var ret) ) {
				Debug.LogWarning($"key {key} not present in static-i18n config");
				ret = string.Empty;
			}
			return string.Intern(ret);
		}
	}
}