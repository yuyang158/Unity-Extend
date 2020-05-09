using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace UI.i18n.Editor {
	[CustomEditor(typeof(StaticI18nText))]
	public class StaticI18nTextEditor : UnityEditor.Editor {
		private const string DEFAULT_EDITOR_LANG = "zh-s";
		
		private struct StaticText {
			public string GUID;
			public string Text;
		}
		private static XmlDocument i18nXml;
		private static XmlElement rootElement;
		private StaticI18nText staticText;
		private TextMeshProUGUI txt;

		private readonly List<StaticText> existTexts = new List<StaticText>();
		private const string xmlConfigPath = "Assets/Resources/Config/static-i18n.xml";
		private SerializedObject textMeshSlObject;
		private SerializedProperty textMeshTextProp;

		private void OnEnable() {
			if( i18nXml == null ) {
				i18nXml = new XmlDocument();

				var i18nXmlAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(xmlConfigPath);
				try {
					i18nXml.LoadXml(i18nXmlAsset.text);
					rootElement = i18nXml.DocumentElement;
					
					foreach( XmlNode childNode in rootElement.ChildNodes ) {
						if(childNode.NodeType != XmlNodeType.Element)
							continue;
						var element = childNode as XmlElement;
						existTexts.Add(new StaticText() {
							GUID = element.Name,
							Text = element.Attributes[DEFAULT_EDITOR_LANG].Value 
						});
					}
				}
				catch(Exception e) {
					Debug.LogWarning(e);
					var declaration = i18nXml.CreateXmlDeclaration("1.0", "utf-8", null);
					i18nXml.AppendChild(declaration);
					rootElement = i18nXml.CreateElement("root");
					rootElement.SetAttribute("support-lang", DEFAULT_EDITOR_LANG);
					i18nXml.AppendChild(rootElement);
					i18nXml.Save(xmlConfigPath);
				}
			}

			staticText = target as StaticI18nText;
			txt = staticText.GetComponent<TextMeshProUGUI>();
			textMeshSlObject = new SerializedObject(txt);
			textMeshTextProp = textMeshSlObject.FindProperty("m_text");
		}

		private string FindGUID(string text) {
			return ( from exist in existTexts where exist.Text == text select exist.GUID ).FirstOrDefault();
		}

		public override void OnInspectorGUI() {
			var keyProp = serializedObject.FindProperty("m_key");
			EditorGUILayout.LabelField("Key: ", keyProp.stringValue);
			EditorGUILayout.PropertyField(textMeshTextProp);
			textMeshSlObject.ApplyModifiedProperties();
			while( GUILayout.Button("Apply to i18n config") ) {
				var guid = keyProp.stringValue;
				if( string.IsNullOrEmpty(guid) ) {
					guid = FindGUID(textMeshTextProp.stringValue);
				}
				if( string.IsNullOrEmpty(guid) ) {
					guid = "a" + GUID.Generate();
					keyProp.stringValue = guid;
					serializedObject.ApplyModifiedProperties();
					NewElement(guid);
					break;
				}
				var elements = rootElement.GetElementsByTagName(guid);
				if( elements.Count != 0 ) {
					var element = elements[0] as XmlElement;
					element.Attributes[DEFAULT_EDITOR_LANG].Value = textMeshTextProp.stringValue;
					i18nXml.Save(xmlConfigPath);
					break;
				}

				NewElement(guid);
			}

			while( GUILayout.Button("Cancel") ) {
				if( !string.IsNullOrEmpty(keyProp.stringValue) ) {
					var elements = rootElement.GetElementsByTagName(keyProp.stringValue);
					if( elements.Count != 0 ) {
						var element = elements[0] as XmlElement;
						txt.text = element.Attributes[DEFAULT_EDITOR_LANG].Value;
					}
					else {
						txt.text = string.Empty;
					}
				}
				else {
					txt.text = string.Empty;
				}
			}
		}

		private void NewElement(string guid) {
			var newEle = i18nXml.CreateElement(guid);
			var zhsAttribute = i18nXml.CreateAttribute("zh-s");
			zhsAttribute.Value = textMeshTextProp.stringValue;
			newEle.Attributes.Append(zhsAttribute);
			rootElement.AppendChild(newEle);
			i18nXml.Save(xmlConfigPath);
		}
	}
}