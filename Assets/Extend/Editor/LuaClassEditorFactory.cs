using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public readonly List<string> DebugMethods = new List<string>();

		private LuaClassDescriptor baseClass;

		public LuaClassDescriptor(TextReader reader) {
			Reload(reader);
		}

		public LuaClassField FindField(string fieldName) {
			return Fields.Find(field => field.FieldName == fieldName);
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
					string declareField;
					var comment = string.Empty;
					var index = line.LastIndexOf("@", StringComparison.Ordinal);
					if( index >= 4 ) {
						declareField = line.Substring(0, index);
						comment = line.Substring(index + 1);
						if( comment.StartsWith("ignore") ) {
							continue;
						}
					}
					else {
						declareField = line;
					}

					statements = declareField.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
					if( statements.Length < 3 ) {
						Debug.LogWarning($"Can not recognized field line : {line}");
						continue;
					}

					if( statements.Length >= 4 && statements[1] != "public" ) {
						continue;
					}

					Fields.Add(new LuaClassField {
						FieldName = statements.Length == 3 ? statements[1] : statements[2],
						FieldType = statements.Length == 3 ? statements[2] : statements[3],
						Comment = comment
					});
				}
				else {
					const string methodStart = @"function M:";
					var match = line.StartsWith(methodStart);
					if( !match ) {
						continue;
					}

					var length = line.IndexOf('(') - methodStart.Length;
					if( length < 0 ) {
						continue;
					}

					var methodName = line.Substring(methodStart.Length, length);
					if( !methodName.StartsWith("_") && methodName[0] == char.ToUpper(methodName[0]) ) {
						if( methodName.StartsWith("DEBUG") ) {
							DebugMethods.Add(methodName);
						}
						else {
							Methods.Add(methodName);
						}
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
		private static string LoadLuaTextFile(string path) {
			path = path.Replace('.', '/') + ".lua";
			while( File.Exists(path) ) {
				try {
					using( var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read) )
					using( var reader = new StreamReader(stream) ) {
						return reader.ReadToEnd();
					}
				}
				catch( Exception ) {
					// ignored
				}
			}

			return null;
		}

		private static readonly Dictionary<string, LuaClassDescriptor> descriptors = new Dictionary<string, LuaClassDescriptor>();

		public static LuaClassDescriptor GetDescriptor(string className) {
			className = className.Replace('.', '/');
			if( descriptors.TryGetValue(className, out var descriptor) ) {
				return descriptor;
			}

			var text = LoadLuaTextFile("Lua/" + className);
			if( text == null )
				return null;

			if( !text.Contains("---@class") ) {
				return null;
			}

			using( var reader = new StringReader(text) ) {
				descriptor = new LuaClassDescriptor(reader);
				descriptors.Add(className, descriptor);
				return descriptor;
			}
		}

		public static LuaClassDescriptor GetDescriptorWithFilePath(string path) {
			var text = LoadLuaTextFile("Lua/" + path);
			if( text == null )
				return null;
			using( var reader = new StringReader(text) ) {
				while( true ) {
					var line = reader.ReadLine();
					if( line == null )
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
				var text = LoadLuaTextFile("Lua/" + path);

				if( text == null ) {
					return null;
				}

				using( var reader = new StringReader(text) ) {
					descriptor.Reload(reader);
					return descriptor;
				}
			}

			return GetDescriptor(className);
		}
	}
}