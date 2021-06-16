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
		public string AssetPath => m_path;

		public string AssetName {
			get {
				if( string.IsNullOrEmpty(m_guid) ) {
					Debug.Log("Missing : " + m_path);
					return string.Empty;
				}

				var assetName = Path.GetDirectoryName(m_path) + "/" + Path.GetFileNameWithoutExtension(m_path);
				return assetName;
			}
		}

		private string m_assetBundleName;

		public string AssetBundleName {
			get => m_assetBundleName;
			private set {
				var val = FormatPath(value);
				Assert.IsTrue(string.IsNullOrEmpty(m_assetBundleName));
				Assert.IsFalse(Calculated);
				Calculated = true;
				m_assetBundleName = val;
				AssetNodeCollector.Add(m_assetBundleName, this);
			}
		}

		public string GUID => AssetDatabase.AssetPathToGUID(AssetPath);

		private readonly List<AssetNode> referenceNodes = new List<AssetNode>();

		private bool Calculated { get; set; }
		private readonly string m_path;
		private readonly string m_guid;

		private static string FormatPath(string path) {
			return path.Replace('\\', '/').ToLower();
		}

		public static void Clear() {
			AllNodes.Clear();
			ResourcesNodes.Clear();
		}

		public static Dictionary<string, AssetNode> AllNodes { get; } = new Dictionary<string, AssetNode>();
		public static Dictionary<string, AssetNode> ResourcesNodes { get; } = new Dictionary<string, AssetNode>();

		public static AssetNode GetOrCreate(string path, string abName = "") {
			path = FormatPath(path);
			var guid = AssetDatabase.AssetPathToGUID(path);
			if( string.IsNullOrEmpty(guid) ) {
				throw new Exception("Can not find asset for path : " + path);
			}

			if( AllNodes.TryGetValue(guid, out var node) ) {
				return node;
			}

			node = new AssetNode(path, guid, abName);
			AllNodes.Add(guid, node);
			if( path.Contains("/resources/") ) {
				ResourcesNodes.Add(guid, node);
			}
			else if( Path.GetExtension(path) == ".spriteatlas" ) {
				ResourcesNodes.Add(guid, node);
			}

			return node;
		}

		private AssetNode(string path, string guid, string abName) {
			if( !path.StartsWith("assets", true, CultureInfo.InvariantCulture) ) {
				Debug.LogError("Asset in not under assets : " + path);
				return;
			}

			m_path = path;
			m_guid = guid;
			if( string.IsNullOrEmpty(m_guid) ) {
				throw new Exception("Path is invalid : " + path);
			}

			AssetCustomProcesses.Process(AssetImporter.GetAtPath(path));
			if( !string.IsNullOrEmpty(abName) ) {
				AssetBundleName = abName;
			}
		}

		public bool IsValid => !string.IsNullOrEmpty(m_guid);

		private void AddReferenceNode(AssetNode node) {
			if( referenceNodes.Contains(node) || Equals(node, this) ) {
				return;
			}

			referenceNodes.Add(node);
		}

		public void BuildRelation() {
			var importer = AssetImporter.GetAtPath(AssetName);
			if( importer && importer is TrueTypeFontImporter ) {
				return;
			}
			
			var dependencies = AssetDatabase.GetDependencies(AssetPath, false);
			foreach( var filePath in dependencies ) {
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

				var importer = AssetImporter.GetAtPath(m_path);
				return !( importer is TextureImporter {textureType: TextureImporterType.Sprite} );
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