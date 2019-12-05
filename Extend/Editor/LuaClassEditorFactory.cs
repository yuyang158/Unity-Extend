using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
		public readonly List<LuaClassField> Fields = new List<LuaClassField>();
		public readonly List<string> Methods = new List<string>();
		private LuaClassDescriptor baseClass;

		public LuaClassDescriptor(TextReader reader) {
			Reload(reader);
		}

		public void Reload(TextReader reader) {
			Fields.Clear();
			Methods.Clear();

			while( true ) {
				var line = reader.ReadLine();
				if( line == null )
					break;

				if( line == string.Empty )
					continue;

				string[] statements;
				if( line.StartsWith("---@class") ) {
					statements = line.Split(' ');
					ClassName = statements[1];
					if( statements.Length == 4 ) {
						var baseClassName = statements[3];
						baseClass = LuaClassEditorFactory.GetDescriptor(baseClassName);
					}
				}
				else if( line.StartsWith("---@field") ) {
					statements = line.Split(' ');
					if( statements.Length < 3 ) {
						Debug.LogWarning($"Can not recognized line : {line}");
						continue;
					}

					Fields.Add(new LuaClassField {
						FieldName = statements[1],
						FieldType = statements[2],
						Comment = statements.Length > 3 ? statements[3] : ""
					});
				}
				else {
					var match = Regex.Match(line, @"M.\w+");
					if( !match.Success ) {
						continue;
					}

					var methodName = match.Value.Substring(2);
					if( !methodName.StartsWith("_") && methodName[0] == char.ToUpper(methodName[0]) ) {
						Methods.Add(methodName);
					}
				}
			}

			if( baseClass != null ) {
				foreach( var baseMethodName in baseClass.Methods.Where(baseMethodName => !Methods.Contains(baseMethodName)) ) {
					Methods.Add(baseMethodName);
				}

				Fields.AddRange(baseClass.Fields);
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
			var asset = Resources.Load<TextAsset>("Lua/" + path);
			if( !asset )
				return null;

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
			var asset = Resources.Load<TextAsset>("Lua/" + path);
			if( !asset )
				return null;
			using( var reader = new StringReader(asset.text) ) {
				while( true ) {
					var line = reader.ReadLine();
					if( string.IsNullOrEmpty(line) )
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
			var path = className.Replace('.', '/');
			if( descriptors.TryGetValue(className, out var descriptor) ) {
				var asset = Resources.Load<TextAsset>("Lua/" + path);

				if( !asset ) {
					return null;
				}

				using( var reader = new StringReader(asset.text) ) {
					descriptor.Reload(reader);
					return descriptor;
				}
			}

			return GetDescriptor(className);
		}
	}
}