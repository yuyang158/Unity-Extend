using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Extend.Asset.Editor.Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.Asset.Editor {
	internal static class AssetNodeCollector {
		private static readonly Dictionary<string, List<AssetNode>> m_sameBundleAssets = new Dictionary<string, List<AssetNode>>();
		private static readonly Dictionary<string, AssetNode> m_duplicateCheck = new Dictionary<string, AssetNode>();

		public static void Add(string assetBundleName, AssetNode assetNode) {
			if( !m_sameBundleAssets.TryGetValue(assetBundleName, out var assetNodes) ) {
				assetNodes = new List<AssetNode>();
				m_sameBundleAssets.Add(assetBundleName, assetNodes);
			}

			if( assetNodes.Contains(assetNode) )
				return;
			assetNodes.Add(assetNode);
			if( assetBundleName != assetNode.AssetBundleName ) {
				Debug.LogError($"{assetNode.AssetName} : {assetBundleName} --> {assetNode.AssetBundleName}");
			}

			if( m_duplicateCheck.ContainsKey(assetNode.AssetName) ) {
				// Debug.LogError($"ERROR : Asset duplicate : {assetNode.AssetName}");
			}
			else {
				m_duplicateCheck.Add(assetNode.AssetName, assetNode);
			}
		}

		public static string GetAssetBundleName(string assetName) {
			return m_duplicateCheck.TryGetValue(assetName, out var assetNode) ? assetNode.AssetBundleName : "";
		}

		public static void Clear() {
			m_sameBundleAssets.Clear();
			m_duplicateCheck.Clear();
		}

		public static AssetBundleBuild[] Output() {
			var builds = new AssetBundleBuild[m_sameBundleAssets.Count];
			int index = 0;
			foreach( var bundlePair in m_sameBundleAssets ) {
				var assets = bundlePair.Value;
				var build = new AssetBundleBuild() {
					addressableNames = new string[assets.Count],
					assetNames = new string[assets.Count],
					assetBundleName = bundlePair.Key + ".ab"
				};
				for( int i = 0; i < assets.Count; i++ ) {
					var asset = assets[i];
					build.addressableNames[i] = asset.AssetName;
					build.assetNames[i] = asset.AssetPath;
				}

				builds[index] = build;
				index++;
			}

			return builds;
		}
	}

	public class AssetNode {
		public const string AB_EXTENSION = ".ab";
		public string AssetPath => importer.assetPath;

		public string AssetName {
			get {
				if(!importer) {
					Debug.Log("Missing : " + m_path);
					return string.Empty;
				}

				var assetName = Path.GetDirectoryName(importer.assetPath) + "/" + Path.GetFileNameWithoutExtension(importer.assetPath);
				assetName = assetName.Replace('\\', '/');
				return assetName.ToLower();
			}
		}

		private string m_assetBundleName;

		public string AssetBundleName {
			get => m_assetBundleName;
			private set {
				Assert.IsTrue(string.IsNullOrEmpty(m_assetBundleName));
				Assert.IsFalse(Calculated);
				Calculated = true;
				m_assetBundleName = value;
				AssetNodeCollector.Add(value, this);
			}
		}

		public string GUID => AssetDatabase.AssetPathToGUID(AssetPath);

		private readonly List<AssetNode> referenceNodes = new List<AssetNode>();
		private readonly AssetImporter importer;

		private bool Calculated { get; set; }
		private string m_path;

		public AssetNode(string path, string abName = "") {
			if( !path.StartsWith("assets", true, CultureInfo.InvariantCulture) ) {
				Debug.LogError("Asset in not under assets : " + path);
				return;
			}
			m_path = path;
			importer = AssetImporter.GetAtPath(path);
			if( !importer ) {
				throw new Exception("Path is not valid : " + path);
			}

			AssetCustomProcesses.Process(importer);
			if( !string.IsNullOrEmpty(abName) ) {
				AssetBundleName = abName;
			}
		}

		public bool IsValid => importer != null;

		private void AddReferenceNode(AssetNode node) {
			if( referenceNodes.Contains(node) || node == this ) {
				return;
			}

			referenceNodes.Add(node);
		}

		public void BuildRelation() {
			var dependencies = AssetDatabase.GetDependencies(AssetPath);
			foreach( var filePath in dependencies ) {
				var extension = Path.GetExtension(filePath);
				if( Array.IndexOf(BuildAssetRelation.IgnoreExtensions, extension) >= 0 )
					continue;
				var dependencyNode = BuildAssetRelation.GetNode(filePath);
				dependencyNode?.AddReferenceNode(this);
			}
		}

		private static void DeepFirstSearch(AssetNode node, ICollection<AssetNode> collector) {
			if( collector.Contains(node) ) {
				return;
			}

			collector.Add(node);
			foreach( var referenceNode in node.referenceNodes ) {
				DeepFirstSearch(referenceNode, collector);
			}
		}

		private readonly List<AssetNode> collector = new List<AssetNode>();

		public void RemoveShorterLink() {
			collector.Clear();
			foreach( var node in referenceNodes.SelectMany(referenceNode => referenceNode.referenceNodes) ) {
				DeepFirstSearch(node, collector);
			}

			foreach( var index in collector.Select(node => referenceNodes.IndexOf(node)).Where(index => index >= 0) ) {
				referenceNodes.RemoveAt(index);
			}
		}

		public string BuildGraphviz() {
			var sb = new StringBuilder();
			foreach( var node in referenceNodes ) {
				sb.AppendLine($"{AssetName} -> {node.AssetName}");
			}

			return sb.ToString();
		}

		private bool OuterLink {
			get {
				if( Path.GetExtension(AssetPath) == ".prefab" )
					return false;

				if( importer is TextureImporter textureImporter ) {
					if( textureImporter.textureType == TextureImporterType.Sprite ) {
						return false;
					}
				}

				return true;
			}
		}

		public void CalculateABName() {
			if( Calculated )
				return;

			if( OuterLink && referenceNodes.Count > 0 ) {
				var abName = "";
				foreach( var referenceNode in referenceNodes ) {
					referenceNode.CalculateABName();
					if( string.IsNullOrEmpty(abName) ) {
						abName = referenceNode.AssetBundleName;
					}
					else if( abName != referenceNode.AssetBundleName ) {
						abName = AssetName;
						break;
					}
				}

				AssetBundleName = abName;
				return;
			}

			AssetBundleName = AssetName;
		}

		public override int GetHashCode() {
			return AssetName.GetHashCode();
		}

		public override bool Equals(object obj) {
			if( obj == null )
				return false;
			var other = obj as AssetNode;
			return AssetName == other.AssetName;
		}

		public int ReferenceCount => referenceNodes.Count;
	}
}