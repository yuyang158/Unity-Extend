using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Extend.Asset.Editor.Process;
using UnityEditor;
using UnityEngine;
using Zeus.Core.FileSystem;

namespace Extend.Asset.Editor {
	public static class BuildAssetRelation {
		private static StaticABSetting[] manualSettings;
		private static readonly string[] IgnoreExtensions = {
			".cs",
			".meta",
			".dll",
			".cginc",
			".hlsl",
			".shadersubgraph"
		};

		public static AssetNode GetNode(string filePath) {
			var extension = Path.GetExtension(filePath);
			if( Array.IndexOf(IgnoreExtensions, extension) >= 0 )
				return null;

			var importer = AssetImporter.GetAtPath(filePath);
			if( importer is TrueTypeFontImporter )
				return null;
			var node = !filePath.StartsWith("assets", true, CultureInfo.InvariantCulture) ? null : AssetNode.GetOrCreate(filePath);
			if( importer ) {
				AssetCustomProcesses.Process(importer);
			}

			return node;
		}

		public static IEnumerable<AssetNode> ResourcesNodes => AssetNode.ResourcesNodes.Values;

		public static void Clear() {
			AssetNode.Clear();
		}

		private static Dictionary<string, BundleUnloadStrategy> s_specialAB;

		public static void BuildRelation(StaticABSettings abSetting) {
			AssetCustomProcesses.Init();
			AssetNodeCollector.Clear();

			manualSettings = abSetting.Settings;
			s_specialAB = new Dictionary<string, BundleUnloadStrategy>();
			foreach( var setting in manualSettings ) {
				Debug.LogWarning($"Setting Path : {setting.Path}");
				var settingFiles = Directory.GetFiles(setting.Path, "*", SearchOption.AllDirectories);
				foreach( var filePath in settingFiles ) {
					if( Array.IndexOf(IgnoreExtensions, Path.GetExtension(filePath)) >= 0 || filePath.Contains(".svn") )
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

					var s = Array.Find(setting.UnloadStrategies, strategy => strategy.BundleName == abName);
					var node = AssetNode.GetOrCreate(filePath, abName);
					// 同名资源处理
					if( !s_specialAB.ContainsKey(node.AssetName) ) {
						s_specialAB.Add(node.AssetName, s?.UnloadStrategy ?? BundleUnloadStrategy.Normal);
					}

					if( Path.GetExtension(node.AssetPath) == ".spriteatlas" ) {
						var dependencies = AssetDatabase.GetDependencies(node.AssetPath);
						foreach( var dependency in dependencies ) {
							var depNode = AssetNode.GetOrCreate(dependency, abName);
							if( Equals(depNode, node) ) {
								continue;
							}

							if( s_specialAB.ContainsKey(depNode.AssetName) ) {
								Debug.LogError($"one sprite in multi atlas : {depNode.AssetName}");
								continue;
							}

							s_specialAB.Add(depNode.AssetName, s?.UnloadStrategy ?? BundleUnloadStrategy.Normal);
						}
						node.ForceAddToResourcesNode();
					}
				}
			}

			var resourcesFiles = Directory.GetFiles("Assets/Resources", "*", SearchOption.AllDirectories);
			List<string> filteredFiles = new List<string>(resourcesFiles.Length);
			filteredFiles.AddRange(resourcesFiles.Where(file => !file.Contains(".svn") && 
				Array.IndexOf(IgnoreExtensions, Path.GetExtension(file)) == -1));
			resourcesFiles = filteredFiles.ToArray();
			var otherFiles = new string[abSetting.ExtraDependencyAssets.Length];
			for( var i = 0; i < abSetting.ExtraDependencyAssets.Length; i++ ) {
				var asset = abSetting.ExtraDependencyAssets[i];
				var path = AssetDatabase.GetAssetPath(asset);
				otherFiles[i] = path;
			}

			foreach( var sceneAsset in abSetting.Scenes ) {
				if(!sceneAsset) {
					Debug.LogError($"Scene asset is null");
					continue;
				}
				var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
				var sceneAbName = scenePath.Substring(0, scenePath.Length - 6).ToLower();
				var sceneNode = AssetNode.GetOrCreate(scenePath, sceneAbName);
				sceneNode.ForceAddToResourcesNode();
				var dependencies = AssetDatabase.GetDependencies(scenePath, false);
				foreach( var dependency in dependencies ) {
					if( Array.IndexOf(IgnoreExtensions, Path.GetExtension(dependency)) >= 0 )
						continue;

					if( !dependency.StartsWith("assets", true, CultureInfo.InvariantCulture) ) {
						Debug.LogWarning($"{scenePath} Depend asset : {dependency} is not under Assets folder");
						continue;
					}
					AssetNode.GetOrCreate(dependency, sceneAbName + "_scene");
				}
			}

			resourcesFiles = resourcesFiles.Concat(otherFiles).ToArray();
			EditorUtility.DisplayProgressBar("Process resources asset", "", 0);
			RelationProcess(resourcesFiles);
			AssetCustomProcesses.PostProcess();

			ExportResourcesPackageConf();
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

		private static void RelationProcess(ICollection<string> resourcesFolderFiles) {
			var progress = 0;
			foreach( var filePath in resourcesFolderFiles ) {
				var extension = Path.GetExtension(filePath);
				progress++;
				if( Array.IndexOf(IgnoreExtensions, extension) >= 0 || filePath.Contains(".svn") )
					continue;
				AssetNode.GetOrCreate(filePath);

				if( progress % 5 == 0 ) {
					EditorUtility.DisplayProgressBar("Process resources asset", $"{progress} / {resourcesFolderFiles.Count}",
						progress / (float)resourcesFolderFiles.Count);
				}
			}

			// 获取依赖关系
			progress = 0;
			var totalCount = AssetNode.ResourcesNodes.Count;
			foreach( var assetNode in AssetNode.ResourcesNodes ) {
				progress++;
				assetNode.Value.BuildRelation();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar("Calculate resources relations", $"{progress} / {totalCount}",
						progress / (float)totalCount);
				}
			}

			progress = 0;
			totalCount = AssetNode.AllNodes.Count;
			foreach( var node in AssetNode.AllNodes ) {
				progress++;
				node.Value.RemoveShorterLink();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar("Remove shorter link", $"{progress} / {totalCount}", progress / (float)totalCount);
				}
			}

			progress = 0;
			//var sb = new StringBuilder();
			foreach( var node in AssetNode.AllNodes ) {
				//sb.Append( node.Value.BuildGraphviz() );
				node.Value.CalculateABName();
				progress++;
				EditorUtility.DisplayProgressBar("Assign bundle name", $"{progress} / {totalCount}", progress / (float)totalCount);
			}

			//Debug.Log( sb.ToString() );
			EditorUtility.ClearProgressBar();
		}

		public static void ExportRedundantFileCheckSumInfoByPath(BuildTarget target)
		{
			string packageConfPathDir = Path.Combine(Application.streamingAssetsPath, "ABBuild", $"{target}");
			DirectoryInfo soundRoot = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "Sounds"));
			RedundantFileCheckSumInfo checkSumInfo = RedundantFileCheckSumInfo.CreateNewOrLoadInfos();
			// 根据生成的package.conf文件，计算ab的md5值
			foreach (var line in File.ReadLines($"{packageConfPathDir}/package.conf"))
			{
				var key = line.Split('|')[1];
				var fullPath = Path.Combine($"{packageConfPathDir}", key);
				checkSumInfo.Update(key, fullPath);
			}
			// 计算声音文件夹中的文件的md5值，可拓展
			foreach (var fileInfo in soundRoot.GetFiles("*.*", SearchOption.AllDirectories).Where(fi => !fi.Name.EndsWith(".meta")))
			{
				checkSumInfo.Update(fileInfo.Name, fileInfo.FullName);
			}
			checkSumInfo.SaveFB();
		}
	}
}