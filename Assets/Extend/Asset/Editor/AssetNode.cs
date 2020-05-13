using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extend.Asset.Editor.Process;
using UnityEditor;
using UnityEngine.Assertions;

namespace Extend.Asset.Editor {
	public class AssetNode {
		public string AssetPath => importer.assetPath;

		private string AssetName {
			get {
				var assetName = Path.GetDirectoryName(importer.assetPath) + "/" + Path.GetFileNameWithoutExtension(importer.assetPath);
				assetName = assetName.Replace('\\', '/');
				return assetName;
			}
		}

		public string AssetBundleName {
			get => importer.assetBundleName;
			private set {
				Assert.IsFalse(Calculated);
				Calculated = true;
				var abName = value.ToLower();
				if( importer.assetBundleName == abName )
					return;
				importer.assetBundleName = abName;
			}
		}

		public string GUID => AssetDatabase.AssetPathToGUID(AssetPath);

		private readonly List<AssetNode> referenceNodes = new List<AssetNode>();
		private readonly AssetImporter importer;

		public bool Calculated { private get; set; }

		public AssetNode(string path) {
			importer = AssetImporter.GetAtPath(path);
			AssetCustomProcesses.Process(importer);
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

			if( OuterLink ) {
				if( referenceNodes.Count > 0 ) {
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
			}

			AssetBundleName = AssetName;
		}

		public int ReferenceCount => referenceNodes.Count;
	}
}