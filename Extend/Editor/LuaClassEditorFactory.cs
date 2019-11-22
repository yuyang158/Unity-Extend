using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public struct LuaClassField {
		public string FieldName;
		public string FieldType;
		public string Comment;
	}

	public class LuaClassDescriptor {
		public string ClassName;
		public List<LuaClassField> Fields;
		public LuaClassDescriptor BaseClass;

		public LuaClassDescriptor(TextReader reader) {
			Fields = new List<LuaClassField>();

			var line = reader.ReadLine();
			while( !string.IsNullOrEmpty(line) ) {
				string[] statements;
				if( line.StartsWith("---@class") ) {
					statements = line.Split(' ');
					ClassName = statements[1];
					if( statements.Length == 4 ) {
						var baseClassName = statements[3];
						BaseClass = LuaClassEditorFactory.GetDescriptor(baseClassName);
					}

					continue;
				}

				if( !line.StartsWith("---@field") )
					continue;

				statements = line.Split(' ');
				if( statements.Length < 3 ) {
					Debug.LogWarning($"Can not recognized line : {line}");
					continue;
				}

				Fields.Add(new LuaClassField() {
					FieldName = statements[1],
					FieldType = statements[2],
					Comment = statements.Length > 3 ? statements[3] : ""
				});
			}
		}
	}

	public static class LuaClassEditorFactory {
		private static readonly Dictionary<string, LuaClassDescriptor> descriptors = new Dictionary<string, LuaClassDescriptor>();

		public static LuaClassDescriptor GetDescriptor(string className) {
			if( descriptors.TryGetValue(className, out var descriptor) ) {
				return descriptor;
			}

			var path = className.Replace('.', '/');
			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Extend/Example/Lua/" + path + ".lua");

			if( !asset.text.Contains("---@class") ) {
				return null;
			}

			using( var reader = new StringReader(asset.text) ) {
				descriptor = new LuaClassDescriptor(reader);
				descriptors.Add(className, descriptor);
				return descriptor;
			}
		}

		public static LuaClassDescriptor GetDescriptorWithFilePath(string path) {
			var asset = Resources.Load<TextAsset>(path);
			if( !asset )
				return null;
			using( var reader = new StringReader(asset.text) ) {
				while( true ) {
					var line = reader.ReadLine();
					if(string.IsNullOrEmpty(line))
						break;

					if( !line.StartsWith("---@class") ) continue;
					var statements = line.Split(' ');
					if( statements.Length < 2 ) {
						break;
					} 
					var className = statements[1];
					return GetDescriptor(className);
				}
			}

			return null;
		}

		public static LuaClassDescriptor ReloadDescriptor(string className) {
			descriptors.Remove(className);
			return GetDescriptor(className);
		}
	}
}