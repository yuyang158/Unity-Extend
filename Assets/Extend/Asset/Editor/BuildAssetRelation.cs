using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Extend.Asset.Editor.Process;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor {
	public static class BuildAssetRelation {
		private static readonly Dictionary<string, AssetNode> resourcesNodes = new Dictionary<string, AssetNode>();
		private static readonly Dictionary<string, AssetNode> allAssetNodes = new Dictionary<string, AssetNode>(40960);
		private static StaticABSetting[] manualSettings;

		public static readonly string[] IgnoreExtensions = {
			".cs",
			".meta",
			".dll"
		};

		public static AssetNode GetNode(string filePath) {
			var extension = Path.GetExtension(filePath);
			if( Array.IndexOf(IgnoreExtensions, extension) >= 0 )
				return null;

			if( !filePath.StartsWith("assets", true, CultureInfo.InvariantCulture) )
				return null;

			var guid = AssetDatabase.AssetPathToGUID(filePath);
			if( !allAssetNodes.TryGetValue(guid, out var dependencyNode) ) {
				dependencyNode = new AssetNode(filePath) {
					Calculated = ContainInManualSettingDirectory(filePath)
				};
				if( !dependencyNode.IsValid )
					return null;
				allAssetNodes.Add(guid, dependencyNode);
				dependencyNode.BuildRelation();
			}

			return dependencyNode;
		}

		public static IEnumerable<AssetNode> ResourcesNodes => resourcesNodes.Values;
		public static void Clear() {
			resourcesNodes.Clear();
			allAssetNodes.Clear();
		}

		private static Dictionary<string, BundleUnloadStrategy> s_specialAB;

		public static void BuildRelation(StaticABSettings abSetting) {
			AssetCustomProcesses.Init();
			AssetNodeCollector.Clear();

			manualSettings = abSetting.Settings;
			s_specialAB = new Dictionary<string, BundleUnloadStrategy>();
			foreach( var setting in manualSettings ) {
				Debug.LogWarning($"Setting Path : {setting.Path}");
				var settingFiles = Directory.GetFiles(setting.Path, "*.*", SearchOption.AllDirectories);
				foreach( var filePath in settingFiles ) {
					if( Array.IndexOf(IgnoreExtensions, Path.GetExtension(filePath)) >= 0 )
						continue;

					var importer = AssetImporter.GetAtPath(filePath);
					if( !importer )
						continue;

					var abName = string.Empty;
					var directoryName = Path.GetDirectoryName(filePath) ?? "";
					switch( setting.Op ) {
						case StaticABSetting.Operation.ALL_IN_ONE:
							abName = FormatPath(setting.Path);
							break;
						case StaticABSetting.Operation.STAY_RESOURCES:
							break;
						case StaticABSetting.Operation.EACH_FOLDER_ONE:
							abName = FormatPath(directoryName);
							break;
						case StaticABSetting.Operation.EACH_A_AB:
							abName = FormatPath(Path.Combine(directoryName, Path.GetFileNameWithoutExtension(filePath)));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					var node = new AssetNode(importer.assetPath, abName);
					var s = Array.Find(setting.UnloadStrategies, (strategy) => strategy.BundleName == abName);
					AddNewAssetNode(node);
					// 同名资源处理
					if( !s_specialAB.ContainsKey(node.AssetName) ) {
						s_specialAB.Add(node.AssetName, s?.UnloadStrategy ?? BundleUnloadStrategy.Normal);
					}
					if( Path.GetExtension(node.AssetPath) == ".spriteatlas" ) {
						var dependencies = AssetDatabase.GetDependencies(node.AssetPath);
						foreach( var dependency in dependencies ) {
							if( dependency == importer.assetPath ) {
								continue;
							}

							var depNode = new AssetNode(dependency, abName);
							AddNewAssetNode(depNode);
							if( s_specialAB.ContainsKey(depNode.AssetName) ) {
								Debug.LogError($"one sprite in multi atlas : {depNode.AssetName}");
								continue;
							}
							s_specialAB.Add(depNode.AssetName, s?.UnloadStrategy ?? BundleUnloadStrategy.Normal);
						}
					}
				}
			}

			var files = Directory.GetFiles("Assets/Resources", "*", SearchOption.AllDirectories);
			var otherFiles = new string[abSetting.ExtraDependencyAssets.Length];
			for( var i = 0; i < abSetting.ExtraDependencyAssets.Length; i++ ) {
				var asset = abSetting.ExtraDependencyAssets[i];
				var path = AssetDatabase.GetAssetPath(asset);
				otherFiles[i] = path;
			}

			files = files.Concat(otherFiles).ToArray();
			EditorUtility.DisplayProgressBar("Process resources asset", "", 0);
			RelationProcess(files);
		}

		private static void AddNewAssetNode(AssetNode node) {
			var guid = AssetDatabase.AssetPathToGUID(node.AssetPath);
			if( node.AssetPath.ToLower().Contains("resources/") ) {
				if(!resourcesNodes.ContainsKey(guid))
					resourcesNodes.Add(guid, node);
			}

			if( !allAssetNodes.ContainsKey(guid) ) {
				allAssetNodes.Add(guid, node);
			}
			else {
				Debug.LogWarning($"Duplicate asset path : {node.AssetPath}");
			}
		}

		// \\ -> /
		private static string FormatPath(string path) {
			return path.Replace('\\', '/').ToLower();
		}

		private static bool ContainInManualSettingDirectory(string path) {
			return s_specialAB.ContainsKey(path.ToLower());
		}

		private static void ExportResourcesPackageConf() {
			// generate resources file to ab map config file
			using( var writer = new StreamWriter($"{StaticAssetBundleWindow.OutputPath}/package.conf") ) {
				foreach( var resourcesNode in ResourcesNodes ) {
					var guid = AssetDatabase.AssetPathToGUID(resourcesNode.AssetPath);
					var assetPath = resourcesNode.AssetPath.ToLower();

					writer.WriteLine(s_specialAB.TryGetValue(resourcesNode.AssetBundleName, out var strategy)
						? $"{assetPath}|{resourcesNode.AssetBundleName + AssetNode.AB_EXTENSION}|{guid}|{(int)strategy}"
						: $"{assetPath}|{resourcesNode.AssetBundleName + AssetNode.AB_EXTENSION}|{guid}");
				}
			}
		}

		private static void RelationProcess(ICollection<string> files) {
			var progress = 0;
			foreach( var filePath in files ) {
				var extension = Path.GetExtension(filePath);
				progress++;
				if( Array.IndexOf(IgnoreExtensions, extension) >= 0 || filePath.Contains(".svn") )
					continue;

				var formatPath = FormatPath(filePath);
				var node = new AssetNode(formatPath);
				if( s_specialAB.ContainsKey(node.AssetName) ) {
					continue;
				}

				var guid = AssetDatabase.AssetPathToGUID(formatPath);
				if( !allAssetNodes.ContainsKey(guid) ) {
					if( string.IsNullOrEmpty(guid) ) {
						continue;
					}

					node.Calculated = ContainInManualSettingDirectory(node.AssetName);
					if( !node.IsValid )
						continue;
					resourcesNodes.Add(guid, node);
					allAssetNodes.Add(guid, node);
				}
				else {
					resourcesNodes.Add(guid, allAssetNodes[guid]);
				}

				if( progress % 5 == 0 ) {
					EditorUtility.DisplayProgressBar("Process resources asset", $"{progress} / {files.Count}", progress / (float)files.Count);
				}
			}

			// 获取依赖关系
			progress = 0;
			foreach( var assetNode in resourcesNodes ) {
				progress++;
				assetNode.Value.BuildRelation();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar("Calculate resources relations", $"{progress} / {resourcesNodes.Count}",
						progress / (float)resourcesNodes.Count);
				}
			}

			progress = 0;
			foreach( var node in allAssetNodes ) {
				progress++;
				node.Value.RemoveShorterLink();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar("Remove shorter link", $"{progress} / {allAssetNodes.Count}", progress / (float)allAssetNodes.Count);
				}
			}

			progress = 0;
			//var sb = new StringBuilder();
			foreach( var node in allAssetNodes ) {
				//sb.Append( node.Value.BuildGraphviz() );
				node.Value.CalculateABName();
				progress++;
				EditorUtility.DisplayProgressBar("Assign bundle name", $"{progress} / {allAssetNodes.Count}", progress / (float)allAssetNodes.Count);
			}

			//Debug.Log( sb.ToString() );
			EditorUtility.ClearProgressBar();
			ExportResourcesPackageConf();
			AssetCustomProcesses.PostProcess();
		}
	}
}