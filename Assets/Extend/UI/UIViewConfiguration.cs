using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
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
		[LuaCallCSharp, Serializable]
		public class Configuration {
			[BlackList]
			public enum PreloadMethod {
				AssetBundle,
				Instance
			}

			[BlackList]
			public struct UIViewRelation {
				public Guid RelationViewGuid;
				public PreloadMethod Method;
			}

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

			// [BlackList]
			// public Guid ViewGuid;

			[BlackList]
			public UIViewRelation[] Relations;

			public int FrameRate = 60;

#if UNITY_EDITOR
			public Configuration() {
				// ViewGuid = Guid.NewGuid();
			}
#endif

			public override string ToString() {
				return Name;
			}
		}

		[SerializeField]
		private Configuration[] m_configurations;

		// private readonly Dictionary<Guid, Configuration> m_guidHashedConfigurations = new Dictionary<Guid, Configuration>();

		public Configuration[] Configurations => m_configurations;

		private Dictionary<string, Configuration> m_hashedConfigurations;
		public static UIViewConfiguration GlobalInstance { get; private set; }
		private const string FILE_PATH = "Config/UIViewConfiguration";

		/*public Configuration FindWithGuid(Guid guid) {
			return m_configurations.FirstOrDefault(configuration => configuration.ViewGuid == guid);
		}*/

		private void OnEnable() {
			m_configurations ??= new[] {new Configuration()};
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
			var filepath = $"Assets/StreamingAssets/{FILE_PATH}.xml";
			using( var writer = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite) ) {
				var declaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
				document.AppendChild(declaration);
				var rootElement = document.CreateElement("Configurations");
				document.AppendChild(rootElement);
				foreach( var configuration in m_configurations ) {
					var element = document.CreateElement("UIView");
					// element.SetAttribute("Guid", configuration.ViewGuid.ToString());
					element.SetAttribute("Name", configuration.Name);
					element.SetAttribute("UIView", configuration.UIView?.AssetGUID);
					element.SetAttribute("BackgroundFx", configuration.BackgroundFx?.AssetGUID);
					element.SetAttribute("FullScreen", configuration.FullScreen ? "1" : "0");
					element.SetAttribute("Transition", configuration.Transition?.AssetGUID);
					element.SetAttribute("AttachLayer", configuration.AttachLayer.ToString());
					element.SetAttribute("CloseMethod", configuration.CloseMethod.ToString());
					element.SetAttribute("CloseButtonPath", configuration.CloseButtonPath);
					element.SetAttribute("FrameRate", configuration.FrameRate.ToString());

					if( configuration.Relations != null && configuration.Relations.Length > 0 ) {
						foreach( var relation in configuration.Relations ) {
							var relationElement = document.CreateElement("Relation");
							relationElement.SetAttribute("Guid", relation.RelationViewGuid.ToString());
							relationElement.SetAttribute("Method", relation.Method.ToString());
							element.AppendChild(relationElement);
						}
					}

					rootElement.AppendChild(element);
				}

				document.Save(writer);
			}
			Debug.Log($"{filepath} saved.");
#endif
		}

		private static AssetReference CreateAssetRef(string assetGUID) {
			return string.IsNullOrEmpty(assetGUID) ? null : new AssetReference(assetGUID);
		}

		public static UIViewConfiguration Load() {
			List<Configuration> configurations = new List<Configuration>();
#if UNITY_EDITOR
			if( !File.Exists($"{Application.streamingAssetsPath}/{FILE_PATH}.xml") ) {
				var newInstance = CreateInstance<UIViewConfiguration>();
				newInstance.m_configurations = configurations.ToArray();
				/*foreach( var configuration in configurations ) {
					newInstance.m_guidHashedConfigurations.Add(configuration.ViewGuid, configuration);
				}*/

				GlobalInstance = newInstance;
				return newInstance.ConvertData();
			}
#endif
			var document = new XmlDocument();
			using( var stream = FileLoader.LoadFileSync($"{FILE_PATH}.xml") ) {
				document.Load(stream);
			}

			var rootElement = document.DocumentElement;
			foreach( XmlElement childElement in rootElement ) {
				var configuration = new Configuration {
					Name = childElement.GetAttribute("Name"),
					UIView = CreateAssetRef(childElement.GetAttribute("UIView")),
					BackgroundFx = CreateAssetRef(childElement.GetAttribute("BackgroundFx")),
					FullScreen = childElement.GetAttribute("FullScreen") == "1",
					Transition = CreateAssetRef(childElement.GetAttribute("Transition")),
					AttachLayer = (UILayer)Enum.Parse(typeof(UILayer), childElement.GetAttribute("AttachLayer")),
					CloseMethod = (CloseOption)Enum.Parse(typeof(CloseOption), childElement.GetAttribute("CloseMethod")),
					CloseButtonPath = childElement.GetAttribute("CloseButtonPath")
				};
				/*if( childElement.HasAttribute("Guid") ) {
					configuration.ViewGuid = Guid.Parse(childElement.GetAttribute("Guid"));
				}*/

				configurations.Add(configuration);

				if( childElement.HasChildNodes ) {
					var count = childElement.ChildNodes.Count;
					configuration.Relations = new Configuration.UIViewRelation[count];
					for( int i = 0; i < count; i++ ) {
						var relationElement = childElement.ChildNodes[i] as XmlElement;
						configuration.Relations[i] = new Configuration.UIViewRelation {
							Method = (Configuration.PreloadMethod)Enum.Parse(typeof(Configuration.PreloadMethod), relationElement.GetAttribute("Method")),
							RelationViewGuid = Guid.Parse(relationElement.GetAttribute("Guid"))
						};
					}
				}
			}

			var instance = CreateInstance<UIViewConfiguration>();
			instance.m_configurations = configurations.ToArray();
			/*foreach( var configuration in configurations ) {
				instance.m_guidHashedConfigurations.Add(configuration.ViewGuid, configuration);
			}*/

			GlobalInstance = instance;
			return instance.ConvertData();
		}
	}
}