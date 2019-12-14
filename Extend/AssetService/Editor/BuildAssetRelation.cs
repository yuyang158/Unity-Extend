using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace Extend.AssetService.Editor {
	public static class BuildAssetRelation {
		private static readonly Dictionary<string, AssetNode> resourcesNodes = new Dictionary<string, AssetNode>();
		private static readonly Dictionary<string, AssetNode> allAssetNodes = new Dictionary<string, AssetNode>( 40960 );
		private static StaticABSetting[] manualSettings;
		private static readonly List<string> needUpdateBundles = new List<string>();

		private static readonly string[] ignoreExtensions = {
			".cs",
			".meta"
		};

		public static AssetNode GetNode(string filePath) {
			var extension = Path.GetExtension( filePath );
			if( Array.IndexOf( ignoreExtensions, extension ) >= 0 )
				return null;

			var guid = AssetDatabase.AssetPathToGUID( filePath );
			if( !allAssetNodes.TryGetValue( guid, out var dependencyNode ) ) {
				dependencyNode = new AssetNode( filePath ) {
					Calculated = ContainInManualSettingDirectory( filePath )
				};
				if( !dependencyNode.IsValid )
					return null;
				allAssetNodes.Add( guid, dependencyNode );
				dependencyNode.BuildRelation();
			}

			return dependencyNode;
		}

		public static IEnumerable<AssetNode> ResourcesNodes => resourcesNodes.Values;
		public static IEnumerable<AssetNode> AllNodes => allAssetNodes.Values;

		public static IList<string> NeedUpdateBundles => needUpdateBundles;

		public static void Clear() {
			resourcesNodes.Clear();
			allAssetNodes.Clear();
		}

		public static HashSet<string> BuildBaseVersionData(string versionConfPath) {
			needUpdateBundles.Clear();
			var allAssetBundles = new HashSet<string>();
			using( var fileStream = new FileStream( versionConfPath, FileMode.Open ) ) {
				using( var reader = new BinaryReader( fileStream ) ) {
					while( fileStream.Position < fileStream.Length ) {
						var guid = reader.ReadString();
						var abName = reader.ReadString();
						var timestamp = reader.ReadUInt64();
						
						if( !allAssetBundles.Contains( abName ) ) {
							allAssetBundles.Add( abName );
						}
						
						var assetPath = AssetDatabase.GUIDToAssetPath( guid );
						if( string.IsNullOrEmpty( assetPath ) ) {
							continue;
						}

						var node = new AssetNode( assetPath ) {AssetBundleName = abName};
						if( !node.IsValid )
							continue;

						if( node.AssetTimeStamp > timestamp && !needUpdateBundles.Contains( abName ) ) {
							needUpdateBundles.Add( abName );
						}
						allAssetNodes.Add( guid, node );
					}
				}
			}

			return allAssetBundles;
		}

		private static HashSet<string> s_spritesInAtlas;
		public static void BuildRelation(StaticABSetting[] settings, HashSet<string> spritesInAtlas, Action completeCallback) {
			manualSettings = settings;
			s_spritesInAtlas = spritesInAtlas;
			foreach( var setting in manualSettings ) {
				var settingFiles = Directory.GetFiles( setting.Path, "*.*", SearchOption.AllDirectories );
				foreach( var filePath in settingFiles ) {
					if( Array.IndexOf( ignoreExtensions, Path.GetExtension( filePath ) ) >= 0 )
						continue;

					var importer = AssetImporter.GetAtPath( filePath );
					if( !importer )
						continue;
					
					if(spritesInAtlas.Contains(FormatPath(filePath.ToLower())))
						continue;

					var abName = string.Empty;
					var directoryName = Path.GetDirectoryName( filePath ) ?? "";
					switch( setting.Op ) {
						case StaticABSetting.Operation.ALL_IN_ONE:
							abName = setting.Path;
							break;
						case StaticABSetting.Operation.STAY_RESOURCES:
							break;
						case StaticABSetting.Operation.EACH_FOLDER_ONE:
							abName = FormatPath( directoryName );
							break;
						case StaticABSetting.Operation.EACH_A_AB:
							abName = FormatPath(Path.Combine(directoryName, Path.GetFileNameWithoutExtension(filePath)));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					importer.assetBundleName = abName;
				}
			}

			var files = Directory.GetFiles( "Assets/Resources", "*", SearchOption.AllDirectories );
			EditorUtility.DisplayProgressBar( "Process resources asset", "", 0 );
			EditorCoroutineUtility.StartCoroutineOwnerless( RelationProcess( files, completeCallback ) );
		}

		// \\ -> /
		private static string FormatPath(string path) {
			return path.Replace( '\\', '/' );
		}

		private static bool ContainInManualSettingDirectory(string path) {
			if( s_spritesInAtlas.Contains(path.ToLower()) )
				return true;

			return Array.Find(manualSettings, setting => path.Contains(setting.Path)) != null;
		}

		private static IEnumerator RelationProcess(IReadOnlyCollection<string> files, Action completeCallback) {
			var progress = 0;
			foreach( var filePath in files ) {
				var extension = Path.GetExtension( filePath );
				progress++;
				if( Array.IndexOf( ignoreExtensions, extension ) >= 0 || filePath.Contains( ".svn" ) )
					continue;

				var formatPath = FormatPath( filePath );
				var guid = AssetDatabase.AssetPathToGUID( formatPath );
				if( !allAssetNodes.ContainsKey( guid ) ) {
					if( string.IsNullOrEmpty( guid ) ) {
						continue;
					}

					var node = new AssetNode( formatPath ) {
						Calculated = ContainInManualSettingDirectory( formatPath )
					};
					if( !node.IsValid )
						continue;
					resourcesNodes.Add( guid, node );
					allAssetNodes.Add( guid, node );
				}
				else {
					resourcesNodes.Add( guid, allAssetNodes[guid] );
				}

				if( progress % 5 == 0 ) {
					EditorUtility.DisplayProgressBar( "Process resources asset", $"{progress} / {files.Count}", progress / (float) files.Count );
					yield return null;
				}
			}

			// 获取依赖关系
			progress = 0;
			foreach( var assetNode in resourcesNodes ) {
				progress++;
				assetNode.Value.BuildRelation();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar( "Calculate resources relations", $"{progress} / {resourcesNodes.Count}",
						progress / (float) resourcesNodes.Count );
					yield return null;
				}
			}

			progress = 0;
			foreach( var node in allAssetNodes ) {
				progress++;
				node.Value.RemoveShorterLink();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar( "Remove shorter link", $"{progress} / {allAssetNodes.Count}", progress / (float) allAssetNodes.Count );
					yield return null;
				}
			}

			progress = 0;
			//var sb = new StringBuilder();
			foreach( var node in allAssetNodes ) {
				//sb.Append( node.Value.BuildGraphviz() );
				node.Value.CalculateABName();
				progress++;
				EditorUtility.DisplayProgressBar( "Assign bundle name", $"{progress} / {allAssetNodes.Count}", progress / (float) allAssetNodes.Count );
				yield return null;
			}

			//Debug.Log( sb.ToString() );
			EditorUtility.ClearProgressBar();
			completeCallback();
		}
	}
}