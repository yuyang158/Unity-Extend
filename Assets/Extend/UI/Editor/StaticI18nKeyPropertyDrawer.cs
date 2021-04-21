using System;
using System.Collections.Generic;
using System.Xml;
using Extend.Common.Editor;
using Extend.UI.Attributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(StaticI18nKeyAttribute))]
	public class StaticI18nKeyPropertyDrawer : PropertyDrawer {
		private const string DEFAULT_EDITOR_LANG = "zh-s";

		private class StaticText {
			public string GUID;
			public string Text;
		}

		private static readonly XmlDocument i18nXml;
		private static readonly XmlElement rootElement;

		private static readonly List<StaticText> existTexts = new List<StaticText>();
		private const string xmlConfigPath = "Assets/Resources/Config/static-i18n.xml";

		static StaticI18nKeyPropertyDrawer() {
			if( i18nXml == null ) {
				i18nXml = new XmlDocument();

				var i18nXmlAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(xmlConfigPath);
				try {
					i18nXml.LoadXml(i18nXmlAsset.text);
					rootElement = i18nXml.DocumentElement;

					foreach( XmlNode childNode in rootElement.ChildNodes ) {
						if( childNode.NodeType != XmlNodeType.Element )
							continue;
						var element = childNode as XmlElement;
						existTexts.Add(new StaticText() {
							GUID = element.Attributes["guid"].Value,
							Text = element.Attributes[DEFAULT_EDITOR_LANG].Value
						});
					}
				}
				catch( Exception e ) {
					Debug.LogWarning(e);
					var declaration = i18nXml.CreateXmlDeclaration("1.0", "utf-8", null);
					i18nXml.AppendChild(declaration);
					rootElement = i18nXml.CreateElement("root");
					rootElement.SetAttribute("support-lang", DEFAULT_EDITOR_LANG);
					i18nXml.AppendChild(rootElement);
					i18nXml.Save(xmlConfigPath);
				}
			}
		}

		private string m_text;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var rect = position;
			rect.height = EditorGUIUtility.singleLineHeight;

			EditorGUI.LabelField(rect, "Key", property.stringValue);
			rect.y += UIEditorUtil.LINE_HEIGHT;

			if( string.IsNullOrEmpty(m_text) && !string.IsNullOrEmpty(property.stringValue) ) {
				m_text = FindGUID(property.stringValue);
			}

			rect.height = UIEditorUtil.LINE_HEIGHT * 2 + EditorGUIUtility.singleLineHeight;
			m_text = EditorGUI.TextArea(rect, m_text);

			rect.y += UIEditorUtil.LINE_HEIGHT * 3;
			rect.height = EditorGUIUtility.singleLineHeight;

			var applyRect = UIEditorUtil.CalcMultiColumnRect(rect, 0, 2);
			if( GUI.Button(applyRect, "Apply To Config") ) {
				var guid = property.stringValue;
				if( string.IsNullOrEmpty(guid) ) {
					guid = FindGUID(m_text);
				}

				if( string.IsNullOrEmpty(guid) ) {
					guid = GUID.Generate().ToString();
					property.stringValue = guid;
					NewElement(guid);
				}
				else {
					var elements = rootElement.GetElementsByTagName(guid);
					if( elements.Count != 0 ) {
						var element = elements[0] as XmlElement;
						element.Attributes[DEFAULT_EDITOR_LANG].Value = m_text;
						i18nXml.Save(xmlConfigPath);
					}
					else {
						NewElement(guid);
					}
				}

				var component = property.serializedObject.targetObject as Component;
				var textMesh = component.GetComponent<TextMeshProUGUI>();
				if( textMesh ) {
					textMesh.text = m_text;
				}

				var txt = component.GetComponent<Text>();
				if( txt ) {
					txt.text = m_text;
				}
			}

			var cancelRect = UIEditorUtil.CalcMultiColumnRect(rect, 1, 2);
			if( GUI.Button(cancelRect, "Cancel") ) {
				var guid = property.stringValue;
				if( string.IsNullOrEmpty(guid) ) {
					m_text = string.Empty;
					return;
				}

				var elements = rootElement.GetElementsByTagName(guid);
				if( elements.Count != 0 ) {
					var element = elements[0] as XmlElement;
					m_text = element.Attributes[DEFAULT_EDITOR_LANG].Value;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return UIEditorUtil.LINE_HEIGHT * 5;
		}

		private static string FindGUID(string guid) {
			var t = existTexts.Find(staticText => guid == staticText.GUID);
			return t == null ? string.Empty : t.Text;
		}

		private void NewElement(string guid) {
			var newEle = i18nXml.CreateElement("Text");
			var zhsAttribute = i18nXml.CreateAttribute("zh-s");
			zhsAttribute.Value = m_text;

			var guidAttribute = i18nXml.CreateAttribute("guid");
			guidAttribute.Value = guid;

			newEle.Attributes.Append(zhsAttribute);
			newEle.Attributes.Append(guidAttribute);

			rootElement.AppendChild(newEle);
			i18nXml.Save(xmlConfigPath);
		}
	}
}