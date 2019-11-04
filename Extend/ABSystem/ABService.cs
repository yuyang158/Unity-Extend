using System.Collections.Generic;
using System.IO;
using ABSystem;
using Common;
using UnityEngine;
using UnityEngine.Assertions;

namespace ABSystem {
	public class ABService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.AB_SERVICE;

		private bool usingAssetBundle;
		private AssetBundleManifest manifest;
		private readonly Dictionary<string, string> resources2ABMapping = new Dictionary<string, string>();
		private readonly Dictionary<string, AssetInstance> loadedAsset = new Dictionary<string, AssetInstance>();
		private readonly Dictionary<string, ABInstance> loadedAssetBundles = new Dictionary<string, ABInstance>();

		private string persistentABDirectory;
		private string streamingAssetsABDirectory;

		public void Initialize() {
			if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor ) {
				usingAssetBundle = false;
				return;
			}

			usingAssetBundle = true;

			var manifestBundleName = string.Empty;
			switch( Application.platform ) {
				case RuntimePlatform.Android:
					manifestBundleName = "Android";
					break;
				case RuntimePlatform.IPhonePlayer:
					manifestBundleName = "iOS";
					break;
			}

			string path;
			persistentABDirectory = Application.persistentDataPath + "/ABBuild/" + manifestBundleName + "/";
			if( !File.Exists( persistentABDirectory ) ) {
				streamingAssetsABDirectory = Application.streamingAssetsPath + "/ABBuild/" + manifestBundleName + "/";
				path = streamingAssetsABDirectory;
			}
			else {
				path = persistentABDirectory;
			}

			var ab = AssetBundle.LoadFromFile( path + manifestBundleName );
			manifest = ab.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" );

			using( var reader = new StreamReader( path + "package.conf" ) ) {
				var line = reader.ReadLine();
				while( !string.IsNullOrEmpty( line ) ) {
					var strings = line.Split( '|' );
					var assetPath = Path.Combine( Path.GetDirectoryName( strings[0] ) + Path.GetFileNameWithoutExtension( strings[0] ) );
					var abPath = strings[1];
					resources2ABMapping.Add( assetPath, abPath );
				}
			}
		}

		public T Load<T>(string path) where T : Object {
			if( !usingAssetBundle ) return Resources.Load<T>( path );
			path = "assets/resources/" + path.ToLower();
			if( loadedAsset.TryGetValue( path, out var asset ) ) {
				asset.IncRef();
				return asset.UnityObject as T;
			}

			if( !resources2ABMapping.TryGetValue( path, out var abPath ) ) return null;
			var ab = LoadSingleABInstance( abPath );
			var unityObject = ab.AB.LoadAsset<T>( path );
			var assetInstance = new AssetInstance(unityObject, path, ab);
			loadedAsset.Add( path, assetInstance );
			assetInstance.IncRef();
			return assetInstance.UnityObject as T;

		}

		public void Release( Object unityObj ) {
			var path = unityObj.name;
			if( loadedAsset.TryGetValue( path, out var asset ) ) {
				Assert.AreEqual( asset.UnityObject, unityObj );
				asset.Release();
			}
		}

		public void RemoveAsset(string assetPath) {
			loadedAsset.Remove( assetPath );
		}

		public void RemoveAB(string abPath) {
			loadedAssetBundles.Remove( abPath );
		}
		

		private ABInstance LoadSingleABInstance(string path) {
			if( loadedAssetBundles.TryGetValue( path, out var ab ) ) {
				return ab;
			}
			
			var persistentAbPath = persistentABDirectory + path;
			var bundle = File.Exists( persistentAbPath ) ? AssetBundle.LoadFromFile( persistentAbPath ) : AssetBundle.LoadFromFile( streamingAssetsABDirectory + path );

			var dependencies = manifest.GetDirectDependencies( path );
			var dependencyInstances = new ABInstance[dependencies.Length];
			for( var i = 0; i < dependencies.Length; i++ ) {
				dependencyInstances[i] = LoadSingleABInstance( dependencies[i] );
			}

			ab = new ABInstance( bundle, path, dependencyInstances );
			loadedAssetBundles.Add( path, ab );
			return ab;
		}

		public void Destroy() {
		}

		public void Update() {
		}
	}
}