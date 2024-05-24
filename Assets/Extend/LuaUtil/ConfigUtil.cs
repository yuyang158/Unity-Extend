using System.Globalization;
using System.IO;
using Extend.Asset;
using Extend.Common;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class ConfigUtil {
		private const string CONFIG_PATH_PREFIX = "Xlsx/";

		private static LuaTable ConvertStringArrayToLua(string[] values) {
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var luaArr = luaVM.NewTable();
			for( var i = 0; i < values.Length; i++ ) {
				luaArr.Set(i + 1, values[i]);
			}

			return luaArr;
		}

		private static JToken ProcessConfigColumnToJson(string type, string val) {
			switch( type ) {
				case "int":
					return int.Parse(val);
				case "string":
					return val;
				case "number":
					return float.Parse(val, NumberStyles.Float, CultureInfo.InvariantCulture);
				case "boolean":
				case "bool":
					return val == "true" || val == "1";
				case "json":
					return JToken.Parse(val);
			}

			return null;
		}

		public static JObject LoadConfigToJson(string filename) {
			var json = new JObject();
			var path = "Assets/Res/" + CONFIG_PATH_PREFIX + filename + ".tsv";
#if UNITY_EDITOR
			using( var reader = new StringReader(File.ReadAllText(path)) ) {
#else
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			if( !service.Exist(path) )
				return null;
			var assetRef = service.Load<TextAsset>(path);
			var asset = assetRef.GetTextAsset();
			using( var reader = new StringReader( asset.text ) ) {
#endif
				var keys = reader.ReadLine();
				var types = reader.ReadLine();

				var keyArr = keys.Split('\t');
				var typeArr = types.Split('\t');

				while( true ) {
					var row = reader.ReadLine();
					if( string.IsNullOrEmpty(row) ) {
						break;
					}

					var rowDataArr = row.Split('\t');
					Assert.IsTrue(keyArr.Length == rowDataArr.Length, $"Table {filename} key count {keyArr.Length} != data count {rowDataArr.Length}");

					var rowJson = new JObject();
					for( int i = 0; i < rowDataArr.Length; i++ ) {
						rowJson[keyArr[i]] = ProcessConfigColumnToJson(typeArr[i], rowDataArr[i]);
					}

					var id = rowDataArr[0];
					json[id] = rowJson;
				}
			}

			return json;
		}

		public static LuaTable LoadConfigFile(string filename) {
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			var path = CONFIG_PATH_PREFIX + filename + ".tsv";
			string tsvContent = null;
#if UNITY_EDITOR
			if( File.Exists(path) ) {
				tsvContent = File.ReadAllText(path);
			}
#endif
			if( string.IsNullOrEmpty(tsvContent) ) {
				if( !service.Exist(path) )
					return null;
				var assetRef = service.Load<TextAsset>(path);
				var asset = assetRef.GetTextAsset();
				tsvContent = asset.text;
				assetRef.Dispose();
			}
			using( var reader = new StringReader(tsvContent) ) {
				var keys = reader.ReadLine();
				var types = reader.ReadLine();

				var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var LuaTable = luaVM.NewTable();
				var keyArr = keys.Split('\t');
				var typeArr = types.Split('\t');
				Assert.IsTrue(keyArr.Length == typeArr.Length, $"Table {filename} key count {keyArr.Length} != type count {typeArr.Length}");

				LuaTable.Set("keys", ConvertStringArrayToLua(keyArr));
				LuaTable.Set("types", ConvertStringArrayToLua(typeArr));

				var dataTable = luaVM.NewTable();
				LuaTable.Set("rows", dataTable);

				var dataIndex = 1;
				while( true ) {
					var row = reader.ReadLine();
					if( string.IsNullOrEmpty(row) ) {
						break;
					}

					var rowDataArr = row.Split('\t');
					// Assert.IsTrue(keyArr.Length == rowDataArr.Length, $"Table {filename} key count {keyArr.Length} != data count {rowDataArr.Length}");
					dataTable.Set(dataIndex, ConvertStringArrayToLua(rowDataArr));
					dataIndex++;
				}

				return LuaTable;
			}
		}
	}
}
