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

		private static readonly XmlDocument m_i18nXml;
		private static readonly XmlElement m_rootElement;

		private static readonly List<StaticText> m_existTexts = new List<StaticText>();
		private const string xmlConfigPath = "Assets/Resources/Config/static-i18n.xml";

		static StaticI18nKeyPropertyDrawer() {
			if( m_i18nXml == null ) {
				m_i18nXml = new XmlDocument();
				var i18nXmlAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(xmlConfigPath);
				try {
					m_i18nXml.LoadXml(i18nXmlAsset.text);
					m_rootElement = m_i18nXml.DocumentElement;

					foreach( XmlNode childNode in m_rootElement.ChildNodes ) {
						if( childNode.NodeType != XmlNodeType.Element )
							continue;
						var element = childNode as XmlElement;
						var guid = element.Attributes["guid"].Value;

						if( !m_existTexts.Exists(t => t.GUID == guid) ) {
							m_existTexts.Add(new StaticText {
								GUID = guid,
								Text = element.Attributes[DEFAULT_EDITOR_LANG].Value
							});
						}
					}
				}
				catch( Exception e ) {
					Debug.LogWarning(e);
					var declaration = m_i18nXml.CreateXmlDeclaration("1.0", "utf-8", null);
					m_i18nXml.AppendChild(declaration);
					m_rootElement = m_i18nXml.CreateElement("root");
					m_rootElement.SetAttribute("support-lang", DEFAULT_EDITOR_LANG);
					m_i18nXml.AppendChild(m_rootElement);
					m_i18nXml.Save(xmlConfigPath);
				}
			}
		}

		public static string FindText(string guid) {
			foreach( var staticText in m_existTexts ) {
				if( staticText.GUID == guid )
					return staticText.Text;
			}

			return "None";
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
					bool found = false;
					foreach( XmlElement i18nElement in m_rootElement ) {
						if( i18nElement.GetAttribute("guid") == guid ) {
							i18nElement.Attributes[DEFAULT_EDITOR_LANG].Value = m_text;
							m_i18nXml.Save(xmlConfigPath);

							var t = m_existTexts.Find(text => text.GUID == guid);
							if( t == null ) {
								m_existTexts.Add(new StaticText() {
									Text = m_text,
									GUID = guid
								});
							}
							else {
								t.Text = m_text;
							}
							
							found = true;
							break;
						}
					}

					if( !found ) {
						NewElement(guid);
					}
				}

				var component = property.serializedObject.targetObject as Component;
				if( component ) {
					var textMesh = component.GetComponent<TextMeshProUGUI>();
					if( textMesh ) {
						textMesh.text = m_text;
					}

					var txt = component.GetComponent<Text>();
					if( txt ) {
						txt.text = m_text;
					}
				}
			}

			var cancelRect = UIEditorUtil.CalcMultiColumnRect(rect, 1, 2);
			if( GUI.Button(cancelRect, "Cancel") ) {
				var guid = property.stringValue;
				if( string.IsNullOrEmpty(guid) ) {
					m_text = string.Empty;
					return;
				}

				var elements = m_rootElement.GetElementsByTagName(guid);
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
			var t = m_existTexts.Find(staticText => guid == staticText.GUID);
			return t == null ? string.Empty : t.Text;
		}

		private void NewElement(string guid) {
			var newEle = m_i18nXml.CreateElement("Text");
			var zhsAttribute = m_i18nXml.CreateAttribute("zh-s");
			zhsAttribute.Value = m_text;
			
			m_existTexts.Add(new StaticText() {
				Text = m_text,
				GUID = guid
			});

			var guidAttribute = m_i18nXml.CreateAttribute("guid");
			guidAttribute.Value = guid;

			newEle.Attributes.Append(zhsAttribute);
			newEle.Attributes.Append(guidAttribute);

			m_rootElement.AppendChild(newEle);
			m_i18nXml.Save(xmlConfigPath);
		}
	}
}