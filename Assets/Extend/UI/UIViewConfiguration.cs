using System;
using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using XLua;

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
		}

		[SerializeField, FormerlySerializedAs("configurations")]
		private Configuration[] m_configurations;

		public Configuration[] Configurations => m_configurations;

		private Dictionary<string, Configuration> hashedConfigurations;
		public const string FILE_PATH = "Config/UIViewConfiguration";

		private void OnEnable() {
			if( m_configurations == null ) {
				m_configurations = new[] { new Configuration() };
			}
		}

		public UIViewConfiguration ConvertData() {
			hashedConfigurations = new Dictionary<string, Configuration>(m_configurations.Length);
			foreach( var configuration in m_configurations ) {
				hashedConfigurations.Add(configuration.Name, configuration);
			}

			return this;
		}

		public Configuration GetOne(string configName) {
			if( !hashedConfigurations.TryGetValue(configName, out var configuration) ) {
				Debug.LogError($"No UIView config named : {configName}");
				return null;
			}

			return configuration;
		}

		public static UIViewConfiguration Load() {
			var assetRef = AssetService.Get().Load(FILE_PATH, typeof(UIViewConfiguration));
			Assert.IsTrue(assetRef.AssetStatus == AssetRefObject.AssetStatus.DONE);

			return assetRef.GetScriptableObject<UIViewConfiguration>().ConvertData();
		}
	}
}