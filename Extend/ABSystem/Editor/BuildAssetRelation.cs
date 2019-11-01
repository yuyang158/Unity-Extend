using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace ABSystem.Editor {
	public static class BuildAssetRelation {
		private static readonly Dictionary<string, AssetNode> assetNodes = new Dictionary<string, AssetNode>();
		private static readonly Dictionary<string, AssetNode> allAssetNodes = new Dictionary<string, AssetNode>( 40960 );

		private static readonly string[] ignoreExtensions = {
			".cs",
			".lua",
			".meta"
		};

		public static AssetNode GetNode(string filePath) {
			var extension = Path.GetExtension( filePath );
			if( Array.IndexOf( ignoreExtensions, extension ) >= 0 )
				return null;
			if( !allAssetNodes.TryGetValue( filePath, out var dependencyNode ) ) {
				dependencyNode = new AssetNode( filePath );
				if( !dependencyNode.IsValid )
					return null;
				allAssetNodes.Add( filePath, dependencyNode );
				dependencyNode.BuildRelation();
			}

			return dependencyNode;
		}

		public static void BuildRelation() {
			assetNodes.Clear();
			allAssetNodes.Clear();

			var files = Directory.GetFiles( "Assets/Resources", "*", SearchOption.AllDirectories );
			EditorUtility.DisplayProgressBar( "Process resources asset", "", 0 );
			EditorCoroutineUtility.StartCoroutineOwnerless( RelationProcess( files ) );
		}

		private static IEnumerator RelationProcess(IReadOnlyCollection<string> files) {
			var progress = 0;
			foreach( var filePath in files ) {
				var extension = Path.GetExtension( filePath );
				progress++;
				if( Array.IndexOf( ignoreExtensions, extension ) >= 0 || filePath.Contains( ".svn" ) )
					continue;

				var formatPath = filePath.Replace( '\\', '/' );
				var node = new AssetNode( formatPath );
				if(!node.IsValid)
					 continue;
				assetNodes.Add( formatPath, node );
				allAssetNodes.Add( formatPath, node );
				if( progress % 5 == 0 ) {
					EditorUtility.DisplayProgressBar( "Process resources asset", $"{progress} / {files.Count}", progress / (float) files.Count );
					yield return null;
				}
			}

			// 获取依赖关系
			progress = 0;
			foreach( var assetNode in assetNodes ) {
				progress++;
				assetNode.Value.BuildRelation();
				if( progress % 10 == 0 ) {
					EditorUtility.DisplayProgressBar( "Calculate resources relations", $"{progress} / {assetNodes.Count}", progress / (float) assetNodes.Count );
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
				if( string.IsNullOrEmpty( node.Value.AssetBundleName ) ) {
					node.Value.CalculateABName();
				} 
				progress++;
				EditorUtility.DisplayProgressBar( "Assign bundle name", $"{progress} / {allAssetNodes.Count}", progress / (float) allAssetNodes.Count );
				yield return null;
			}
			//Debug.Log( sb.ToString() );
			EditorUtility.ClearProgressBar();
		}
	}
}