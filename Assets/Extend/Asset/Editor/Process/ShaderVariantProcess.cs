using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.Asset.Editor.Process {
	[InitializeOnLoad]
	public class ShaderVariantProcess : IBuildAssetProcess {
		static ShaderVariantProcess() {
			AssetCustomProcesses.RegisterProcess(new ShaderVariantProcess());
		}
		
		public static readonly ShaderKeyword[] STRIP_BUILD_IN_KEYWORDS = {
			new ShaderKeyword("FOG_EXP"),
			new ShaderKeyword("FOG_EXP2"),
			new ShaderKeyword("POINT_COOKIE"),
			new ShaderKeyword("DIRECTIONAL_COOKIE"),
			new ShaderKeyword("SHADOWS_SOFT"),
			new ShaderKeyword("VERTEXLIGHT_ON"), 
			new ShaderKeyword("DYNAMICLIGHTMAP_ON")
		};

		private class ShaderVariantContext {
			public readonly Dictionary<string, int> KeywordPassType = new Dictionary<string, int>(128);
			public void AddNewVariant(string keywords, int type) {
				if( !KeywordPassType.TryGetValue(keywords, out var passTypeSum) ) {
					passTypeSum = 0;
					KeywordPassType[keywords] = passTypeSum;
				}

				KeywordPassType[keywords] = passTypeSum | 1 << type;
			}
		}

		private class UserKeywordMap : Dictionary<string, ShaderVariantContext> {
			public readonly HashSet<string> UserDefineKeywords = new HashSet<string>();
		}

		private const char JOIN_SPLITTER = ';';
		public Type ProcessType => typeof(AssetImporter);

		private readonly Dictionary<string, UserKeywordMap> collectShaderUserKeywords = new Dictionary<string, UserKeywordMap>();
		private readonly HashSet<string> keywordMap = new HashSet<string>();
		private readonly Dictionary<string, UserKeywordMap> collectedShaderKeyword = new Dictionary<string, UserKeywordMap>();
		private readonly ShaderVariantCollection EMPTY_COLLECTION;

		public ShaderVariantProcess() {
			var assembly = typeof(EditorApplication).Assembly;
			var shaderUtilType = assembly.GetType("UnityEditor.ShaderUtil");
			getShaderVariantEntries = shaderUtilType.GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.Static | BindingFlags.NonPublic);

			EMPTY_COLLECTION = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Assets/Shaders/Tools.shadervariants");
		}

		private readonly List<string> userKeywordFilter = new List<string>();

		public void Process(AssetImporter importer, TextWriter writer) {
			if(Path.GetExtension(importer.assetPath) != ".mat")
				return;
			var material = AssetDatabase.LoadAssetAtPath<Material>(importer.assetPath);
			var keywords = material.shaderKeywords;
			if( keywords.Length == 0 )
				return;

			
			var shader = material.shader;
			if( !EMPTY_COLLECTION.Contains(new ShaderVariantCollection.ShaderVariant(shader, PassType.Normal)) ) {
				EMPTY_COLLECTION.Add(new ShaderVariantCollection.ShaderVariant(shader, PassType.Normal));
			}
			var shaderKeywordMap = GetShaderContext(shader);
			if( shaderKeywordMap.UserDefineKeywords.Count == 0 )
				return;

			userKeywordFilter.Clear();
			foreach( var keyword in keywords ) {
				if( shaderKeywordMap.UserDefineKeywords.Contains(keyword) ) {
					userKeywordFilter.Add(keyword);
				}
			}

			if( userKeywordFilter.Count == 0 ) {
				return;
			}
			userKeywordFilter.Sort();
			var path = AssetDatabase.GetAssetPath(shader);
			if( !collectedShaderKeyword.TryGetValue(path, out var collection) ) {
				collection = new UserKeywordMap();
				collectedShaderKeyword.Add(path, collection);
			}
			var userKeywordForMatch = string.Join(JOIN_SPLITTER.ToString(), userKeywordFilter);
			if( collection.ContainsKey(userKeywordForMatch) ) {
				return;
			}

			if( shaderKeywordMap.TryGetValue(userKeywordForMatch, out var context) ) {
				collection.Add(userKeywordForMatch, context);
			}
			else {
				Debug.Log($"{userKeywordForMatch} --> {material.name}");
			}
		}

		private UserKeywordMap GetShaderContext(Shader shader) {
			if( !collectShaderUserKeywords.TryGetValue(shader.name, out var shaderKeywordMap) ) {
				GetShaderVariantEntries(shader, EMPTY_COLLECTION, out var types, out var keywordCombines);
				shaderKeywordMap = new UserKeywordMap();
				var splitKeywords = new List<string>();
				var shaderKeywords = new Dictionary<string, ShaderKeywordType>();
				for( var i = 0; i < keywordCombines.Length; i++ ) {
					var keywordCombine = keywordCombines[i];
					if( string.IsNullOrEmpty(keywordCombine) )
						continue;
					splitKeywords.Clear();
					var keywordArray = keywordCombine.Split(' ');
					foreach( var keyword in keywordArray ) {
						if( !shaderKeywords.TryGetValue(keyword, out var shaderKeywordType) ) {
							if( STRIP_BUILD_IN_KEYWORDS.Any(stripKeyword => ShaderKeyword.GetKeywordName(shader, stripKeyword) == keyword) ) {
								break;
							}

							var shaderKeyword = new ShaderKeyword(keyword);
							shaderKeywordType = ShaderKeyword.GetKeywordType(shader, shaderKeyword);
							shaderKeywords.Add(keyword, shaderKeywordType);
						}

						if( shaderKeywordType == ShaderKeywordType.UserDefined ) {
							keywordMap.Add(keyword);
							if( !shaderKeywordMap.UserDefineKeywords.Contains(keyword) ) {
								shaderKeywordMap.UserDefineKeywords.Add(keyword);
							}
						}
					}

					if( splitKeywords.Count == 0 )
						continue;

					splitKeywords.Sort();
					var sortedKeywordCombine = string.Join(JOIN_SPLITTER.ToString(), splitKeywords);
					if( !shaderKeywordMap.TryGetValue(sortedKeywordCombine, out var context) ) {
						context = new ShaderVariantContext();
						shaderKeywordMap.Add(sortedKeywordCombine, context);
					}

					context.AddNewVariant(string.Join(JOIN_SPLITTER.ToString(), keywordArray), types[i]);
				}

				collectShaderUserKeywords.Add(shader.name, shaderKeywordMap);
			}

			return shaderKeywordMap;
		}

		private const string VARIANT_EXTENSION = "_variants.shadervariants";

		public void PostProcess() {
			foreach( var item in collectedShaderKeyword ) {
				var shaderPath = item.Key;
				var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
				var collection = new ShaderVariantCollection();
				foreach( var p in item.Value ) {
					foreach( var _item in p.Value.KeywordPassType ) {
						var keywords = _item.Key.Split(JOIN_SPLITTER);
						for( var i = 0; i < 15; i++ ) {
							if( ( ( 1 << i ) & _item.Value ) != 0 ) {
								var variant = new ShaderVariantCollection.ShaderVariant(shader, (PassType)i, keywords);
								if( !collection.Contains(variant) ) {
									collection.Add(variant);
								}
							}
						}
					}
				}

				var variantPath = shaderPath.Substring(0, shaderPath.LastIndexOf('.')) + VARIANT_EXTENSION;
				AssetDatabase.CreateAsset(collection, variantPath);

				var shaderImporter = AssetImporter.GetAtPath(shaderPath);
				var variantImporter = AssetImporter.GetAtPath(variantPath);
				variantImporter.assetBundleName = shaderImporter.assetBundleName;
				variantImporter.assetBundleVariant = shaderImporter.assetBundleVariant;
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		[MenuItem("Tools/Show Importer")]
		private static void ShowImporter() {
			if( !Selection.activeObject )
				return;

			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			var importer = AssetImporter.GetAtPath(path);
			if(!importer)
				return;
			Debug.Log($"Importer : {importer.GetType().FullName}");
		}


		private static MethodInfo getShaderVariantEntries;

		private static void GetShaderVariantEntries(Shader shader, ShaderVariantCollection skipAlreadyInCollection, out int[] types, out string[] keywords) {
			var filterKeywords = new string[] {};
			types = new int[] { };
			keywords = new string[] { };
			var remainingKeywords = new string[] { };
			var parameters = new object[] {shader, 256, filterKeywords,  skipAlreadyInCollection, types, keywords, remainingKeywords};
			getShaderVariantEntries.Invoke(null, parameters);
		}
	}
}