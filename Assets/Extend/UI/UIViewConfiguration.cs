using System;
using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp]
	public class UIViewConfiguration : ScriptableObject {
		[Serializable, LuaCallCSharp]
		public class Configuration {
			[Serializable]
			public enum Layer {
				Scene,
				Dialog,
				Popup,
				Tip,
				MostTop
			}
			
			public string Name = "Default";

			[AssetReferenceAssetType(AssetType = typeof(UIViewBase))]
			public AssetReference UIView;

			[AssetReferenceAssetType(AssetType = typeof(UIViewBase))]
			public AssetReference BackgroundFx;

			public bool FullScreen;

			[AssetReferenceAssetType(AssetType = typeof(UIViewBase))]
			public AssetReference Transition;

			public Layer AttachLayer;
		}

		[SerializeField]
		private Configuration[] configurations;

		public Configuration[] Configurations => configurations;

		private Dictionary<string, Configuration> hashedConfigurations;
		public const string FILE_PATH = "Config/UIViewConfiguration.asset";

		private void OnEnable() {
			if( configurations == null ) {
				configurations = new[] { new Configuration() };
			}
			
			if(!Application.isPlaying)
				return;
			hashedConfigurations = new Dictionary<string, Configuration>(configurations.Length);
			foreach( var configuration in configurations ) {
				hashedConfigurations.Add(configuration.Name, configuration);
			}
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
			Assert.IsFalse(assetRef != null && assetRef.IsFinished);

			return assetRef.GetScriptableObject<UIViewConfiguration>();
		}
	}
}