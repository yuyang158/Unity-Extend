using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using UnityEngine.Serialization;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend.UI {
	[Serializable, LuaCallCSharp]
	public enum UILayer {
		Scene,
		Dialog,
		Popup,
		Tip,
		Transition,
		MostTop,
		Count
	}

	[Serializable, LuaCallCSharp]
	public enum CloseOption {
		None,
		AnyWhere,
		Outside,
		Button
	}

	[LuaCallCSharp]
	public class UIViewConfiguration : ScriptableObject {
		[Serializable, LuaCallCSharp]
		public class Configuration {
			public string Name = "Default";

			[AssetReferenceAssetType(AssetType = typeof(UIViewBase))]
			public AssetReference UIView;

			[AssetReferenceAssetType(AssetType = typeof(UIViewBase))]
			public AssetReference BackgroundFx;

			public bool FullScreen;

			[AssetReferenceAssetType(AssetType = typeof(UIViewBase))]
			public AssetReference Transition;

			public UILayer AttachLayer;

			public CloseOption CloseMethod = CloseOption.None;

			public string CloseButtonPath;

			public override string ToString() {
				return Name;
			}
		}

		[SerializeField, FormerlySerializedAs("configurations")]
		private Configuration[] m_configurations;

		public Configuration[] Configurations => m_configurations;

		private Dictionary<string, Configuration> m_hashedConfigurations;
		public const string FILE_PATH = "Config/UIViewConfiguration";

		private void OnEnable() {
			if( m_configurations == null ) {
				m_configurations = new[] {new Configuration()};
			}
		}

		private UIViewConfiguration ConvertData() {
			m_hashedConfigurations = new Dictionary<string, Configuration>(m_configurations.Length);
			foreach( var configuration in m_configurations ) {
				m_hashedConfigurations.Add(configuration.Name, configuration);
			}

			return this;
		}

		public Configuration GetOne(string configName) {
			if( !m_hashedConfigurations.TryGetValue(configName, out var configuration) ) {
				Debug.LogError($"No UIView config named : {configName}");
				return null;
			}

			return configuration;
		}

		public void Save() {
#if UNITY_EDITOR
			var document = new XmlDocument();
			var filepath = $"Assets/Resources/{FILE_PATH}.xml";
			using( var writer = new FileStream(filepath, FileMode.OpenOrCreate) ) {
				var declaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
				document.AppendChild(declaration);
				var rootElement = document.CreateElement("Configurations");
				document.AppendChild(rootElement);
				foreach( var configuration in m_configurations ) {
					var element = document.CreateElement("UIView");
					element.SetAttribute("Name", configuration.Name);
					element.SetAttribute("UIView", configuration.UIView?.AssetGUID);
					element.SetAttribute("BackgroundFx", configuration.BackgroundFx?.AssetGUID);
					element.SetAttribute("FullScreen", configuration.FullScreen ? "1" : "0");
					element.SetAttribute("Transition", configuration.Transition?.AssetGUID);
					element.SetAttribute("AttachLayer", configuration.AttachLayer.ToString());
					element.SetAttribute("CloseMethod", configuration.CloseMethod.ToString());
					element.SetAttribute("CloseButtonPath", configuration.CloseButtonPath);

					rootElement.AppendChild(element);
				}

				document.Save(writer);
			}
#endif
		}

		private static AssetReference CreateAssetRef(string assetGUID) {
			return string.IsNullOrEmpty(assetGUID) ? null : new AssetReference(assetGUID);
		}

		public static UIViewConfiguration Load() {
			var document = new XmlDocument();
#if UNITY_EDITOR
			if( !File.Exists($"Assets/Resources/{FILE_PATH}.xml") ) {
				var defaultInstance = CreateInstance<UIViewConfiguration>();
				defaultInstance.m_configurations = new Configuration[0];
				return defaultInstance.ConvertData();
			}

			using( var stream = File.OpenRead($"Assets/Resources/{FILE_PATH}.xml") ) {
				document.Load(stream);
			}
#else
			var assetRef = AssetService.Get().Load(FILE_PATH, typeof(TextAsset));
			UnityEngine.Assertions.Assert.IsTrue(assetRef.AssetStatus == AssetRefObject.AssetStatus.DONE);
			document.LoadXml(assetRef.GetTextAsset().text);
			assetRef.Dispose();
#endif
			var rootElement = document.DocumentElement;
			List<Configuration> configurations = new List<Configuration>();
			foreach( XmlElement childElement in rootElement ) {
				configurations.Add(new Configuration() {
					Name = childElement.GetAttribute("Name"),
					UIView = CreateAssetRef(childElement.GetAttribute("UIView")),
					BackgroundFx = CreateAssetRef(childElement.GetAttribute("BackgroundFx")),
					FullScreen = childElement.GetAttribute("FullScreen") == "1",
					Transition = CreateAssetRef(childElement.GetAttribute("Transition")),
					AttachLayer = (UILayer)Enum.Parse(typeof(UILayer), childElement.GetAttribute("AttachLayer")),
					CloseMethod = (CloseOption)Enum.Parse(typeof(CloseOption), childElement.GetAttribute("CloseMethod")),
					CloseButtonPath = childElement.GetAttribute("CloseButtonPath")
				});
			}

			var instance = CreateInstance<UIViewConfiguration>();
			instance.m_configurations = configurations.ToArray();
			return instance.ConvertData();
		}
	}
}