using System.IO;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class ConfigUtil {
		private const string CONFIG_PATH_PREFIX = "Config/";

		private static LuaTable ConvertStringArrayToLua(string[] values) {
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var luaArr = luaVM.Default.NewTable();
			for( var i = 0; i < values.Length; i++ ) {
				luaArr.Set( i + 1, values[i] );
			}
			
			return luaArr;
		}

		public static LuaTable LoadConfigFile(string filename) {
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			var assetRef = service.Load( CONFIG_PATH_PREFIX + filename, typeof(TextAsset) );
			var asset = assetRef.GetTextAsset();
			using( var reader = new StringReader( asset.text ) ) {
				var keys = reader.ReadLine();
				var types = reader.ReadLine();
				reader.ReadLine();

				var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var luaTable = luaVM.Default.NewTable();
				var keyArr = keys.Split( '\t' );
				var typeArr = types.Split( '\t' );
				Assert.IsTrue( keyArr.Length == typeArr.Length, $"Table {filename} key count {keyArr.Length} != type count {typeArr.Length}" );

				luaTable.Set( "keys", ConvertStringArrayToLua(keyArr) );
				luaTable.Set( "types", ConvertStringArrayToLua(typeArr) );

				var dataTable = luaVM.Default.NewTable();
				luaTable.Set( "rows", dataTable );

				var dataIndex = 1;
				while( true ) {
					var row = reader.ReadLine();
					if( string.IsNullOrEmpty( row ) ) {
						break;
					}

					var rowDataArr = row.Split( '\t' );
					Assert.IsTrue( keyArr.Length == rowDataArr.Length, $"Table {filename} key count {keyArr.Length} != data count {rowDataArr.Length}" );
					dataTable.Set( dataIndex, ConvertStringArrayToLua( rowDataArr ) );
					dataIndex++;
				}

				Resources.UnloadAsset( asset );
				return luaTable;
			}
		}
	}
}