using CSObjectWrapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using XLua;

public static class EmmyLuaAPIMaker {
	#region member
	private static readonly string m_apiDir = Application.dataPath + "/../UnityLuaAPI";
	private static string m_argsText;
	#endregion

	[MenuItem("Tools/程序/EmmyLua/Generate All API")]
	public static void GenAll() {
		GenCSApi();
		GenConfigApi();
		// GenerateProtocolBufApi();
		EditorUtility.ClearProgressBar();
		Debug.Log("转换完成");
	}

	#region C# API

	private static void GenCSApi() {
		try {
			var template_ref = ScriptableObject.CreateInstance<TemplateRef>();
			if( template_ref.LuaClassWrap == null ) {
				return;
			}

			Generator.GetGenConfig(Utils.GetAllTypes());
			ExportLuaApi(Generator.LuaCallCSharp);
		}
		catch(Exception e) {
			// ignored
			Debug.LogException(e);
		}
	}

	private static string TypeDecl(Type t, bool isFull = true) {
		string result;
		if( t.IsGenericType ) {
			string ret = GenericBaseName(t, isFull);

			string gs = "";
			gs += "<";
			Type[] types = t.GetGenericArguments();
			for( int n = 0; n < types.Length; n++ ) {
				gs += TypeDecl(types[n]);
				if( n < types.Length - 1 )
					gs += ",";
			}

			gs += ">";

			ret = Regex.Replace(ret, @"`\d", gs);

			result = ret;
		}
		else if( t.IsArray ) {
			result = TypeDecl(t.GetElementType()) + "[]";
		}
		else {
			result = RemoveRef(t.ToString(), false);
		}

		result = result.Replace("<", "_")
			.Replace(",", "_").Replace(">", "");
		return "CS." + result;
	}

	private static string GenericBaseName(Type t, bool isFull) {
		string n;
		if( isFull ) {
			n = t.FullName;
		}
		else {
			n = t.Name;
		}

		if( n.IndexOf('[') > 0 ) {
			n = n.Substring(0, n.IndexOf('['));
		}

		return n.Replace("+", ".");
	}

	private static readonly string[] prefix = {"System.Collections.Generic"};

	private static string RemoveRef(string s, bool removearray = true) {
		if( s.EndsWith("&") ) s = s.Substring(0, s.Length - 1);
		if( s.EndsWith("[]") && removearray ) s = s.Substring(0, s.Length - 2);
		if( s.StartsWith(prefix[0]) ) s = s.Substring(prefix[0].Length + 1, s.Length - prefix[0].Length - 1);

		s = s.Replace("+", ".");
		if( s.Contains("`") ) {
			string regstr = @"`\d";
			var r = new Regex(regstr, RegexOptions.None);
			s = r.Replace(s, "");
			s = s.Replace("[", "<");
			s = s.Replace("]", ">");
		}

		return s;
	}

	private static void AppendCR(this StringBuilder sb, string str) {
		sb.Append(str + "\r\n");
	}

	private static XmlDocument loadXML(string fileName) {
		string filepath = fileName;
		if( File.Exists(filepath) ) {
			var xmlDoc = new XmlDocument();
			//根据路径将XML读取出来
			xmlDoc.Load(filepath);
			return xmlDoc;
		}

		return null;
	}

	private static IEnumerable<Type> type_has_extension_methods;

	private static IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType) {
		var LuaCallCSharp = Generator.LuaCallCSharp;
		if( type_has_extension_methods == null ) {
			type_has_extension_methods = from type in LuaCallCSharp
				where type.GetMethods(BindingFlags.Static | BindingFlags.Public)
					.Any(method => method.IsDefined(typeof(ExtensionAttribute), false))
				select type;
		}

		return from type in type_has_extension_methods
			where type.IsSealed && !type.IsGenericType && !type.IsNested
			from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
			where isSupportedExtensionMethod(method, extendedType)
			select method;
	}

	private static bool isSupportedExtensionMethod(MethodBase method, Type extendedType) {
		if( !method.IsDefined(typeof(ExtensionAttribute), false) )
			return false;
		var methodParameters = method.GetParameters();
		if( methodParameters.Length < 1 )
			return false;

		var hasValidGenericParameter = false;
		for( var i = 0; i < methodParameters.Length; i++ ) {
			var parameterType = methodParameters[i].ParameterType;
			if( i == 0 ) {
				if( parameterType.IsGenericParameter ) {
					var parameterConstraints = parameterType.GetGenericParameterConstraints();
					if( parameterConstraints.Length == 0 ) return false;
					bool firstParamMatch = false;
					foreach( var parameterConstraint in parameterConstraints ) {
						if( parameterConstraint != typeof(ValueType) && parameterConstraint.IsAssignableFrom(extendedType) ) {
							firstParamMatch = true;
						}
					}

					if( !firstParamMatch ) return false;

					hasValidGenericParameter = true;
				}
				else if( parameterType != extendedType )
					return false;
			}
			else if( parameterType.IsGenericParameter ) {
				var parameterConstraints = parameterType.GetGenericParameterConstraints();
				if( parameterConstraints.Length == 0 ) return false;
				foreach( var parameterConstraint in parameterConstraints ) {
					if( !parameterConstraint.IsClass || ( parameterConstraint == typeof(ValueType) ) || hasGenericParameter(parameterConstraint) )
						return false;
				}

				hasValidGenericParameter = true;
			}
		}

		return hasValidGenericParameter || !method.ContainsGenericParameters;
	}

	private static bool hasGenericParameter(Type type) {
		if( type.IsByRef || type.IsArray ) {
			return hasGenericParameter(type.GetElementType());
		}

		return type.IsGenericType ? type.GetGenericArguments().Any(hasGenericParameter) : type.IsGenericParameter;
	}

	private static void ExportLuaApi(List<Type> classList) {
		type_has_extension_methods = null;
		GlobalAPI.Clear();

		// add class here
		var bindType = BindingFlags.DeclaredOnly |
		               BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
		string path = m_apiDir;
		if( path == "" ) {
			return;
		}

		if( Directory.Exists(path) ) {
			Directory.Delete(path, true);
		}
		Directory.CreateDirectory(path);

		for( int i = 0; i < classList.Count; i++ ) {
			var t = classList[i];
			EditorUtility.DisplayProgressBar("CS Type", t.FullName, i / (float)classList.Count);
			string name = TypeDecl(t);
			GlobalAPI.AddType(name);
			try {
				var fs = new FileStream(path + "/" + name.Replace(".", "_") + ".lua", FileMode.Create);
				var utf8WithoutBom = new UTF8Encoding(false);
				var sw = new StreamWriter(fs, utf8WithoutBom);

				if( t.BaseType != null && t.BaseType != typeof(object) ) {
					string baseName = TypeDecl(t.BaseType);
					sw.WriteLine($"---@class {name} : {baseName}");
				}
				else {
					sw.WriteLine($"---@class {name}");
				}

				FieldInfo[] fields = t.GetFields(bindType);
				foreach( var field in fields ) {
					if( IsObsolete(field) ) {
						continue;
					}
					WriteField(sw, field.FieldType, field.Name);
				}

				PropertyInfo[] properties = t.GetProperties(bindType);
				foreach( var property in properties ) {
					if( IsObsolete(property) ) {
						continue;
					}
					WriteField(sw, property.PropertyType, property.Name);
				}

				sw.WriteLine("");

				sw.WriteLine($"---@type {name}");
				sw.WriteLine(name + " = { }");

				#region constructor

				var constructors = new List<ConstructorInfo>(t.GetConstructors(bindType));
				constructors.Sort((left, right) => { return left.GetParameters().Length - right.GetParameters().Length; });
				bool isDefineTable = false;

				ParameterInfo[] paramInfos;
				int delta;
				if( constructors.Count > 0 ) {
					WriteCtorComment(sw, constructors);
					paramInfos = constructors[constructors.Count - 1].GetParameters();
					delta = paramInfos.Length - constructors[0].GetParameters().Length;
					WriteFun(sw, delta, paramInfos, t, "", name, true);
					isDefineTable = true;
				}

				#endregion

				#region method

				var methods = new List<MethodInfo>(t.GetMethods(bindType));
				methods.AddRange(GetExtensionMethods(t));

				Dictionary<string, List<MethodInfo>> methodDict = new Dictionary<string, List<MethodInfo>>();
				foreach( var method in methods ) {
					string methodName = method.Name;
					if( IsObsolete(method) ) {
						continue;
					}

					if( method.IsGenericMethod ) {
						continue;
					}

					if( !method.IsPublic ) continue;
					if( methodName.StartsWith("get_") || methodName.StartsWith("set_") ) continue;
					if( methodName.StartsWith("add_") || methodName.StartsWith("remove_") ) continue;

					if( !methodDict.TryGetValue(methodName, out var list) ) {
						list = new List<MethodInfo>();
						methodDict.Add(methodName, list);
					}

					paramInfos = method.GetParameters();
					var arrayParam = paramInfos.Any(paramInfo => paramInfo.GetType().IsArray);
					if(!arrayParam && !method.ReturnType.IsArray) {
						list.Add(method);
					}
				}

				var itr = methodDict.GetEnumerator();
				while( itr.MoveNext() ) {
					List<MethodInfo> list = itr.Current.Value;
					RemoveRewriteFunHasTypeAndString(list);
					
					if(list.Count == 0)
						break;

					list.Sort((left, right) => {
						int leftLen = left.GetParameters().Length;
						int rightLen = right.GetParameters().Length;
						if( left.IsDefined(typeof(ExtensionAttribute), false) ) {
							leftLen--;
						}

						if( right.IsDefined(typeof(ExtensionAttribute), false) ) {
							rightLen--;
						}
						return leftLen - rightLen;
					});
					WriteFunComment(sw, list);
					paramInfos = list[list.Count - 1].GetParameters();

					if( list[list.Count - 1].IsDefined(typeof(ExtensionAttribute), false) ) {
						var newParamInfos = new List<ParameterInfo>(paramInfos);
						newParamInfos.RemoveAt(0);
						paramInfos = newParamInfos.ToArray();
					}

					var methodInfo = list[0];
					delta = paramInfos.Length - methodInfo.GetParameters().Length;
					var staticMethod = methodInfo.IsDefined(typeof(ExtensionAttribute), false) == false && methodInfo.IsStatic;
					WriteFun(sw, delta, paramInfos, list[0].ReturnType, list[0].Name, name, staticMethod);
				}

				itr.Dispose();

				#endregion

				var events = t.GetEvents();
				foreach( var eventInfo in events ) {
					WriteEvent(sw, eventInfo.AddMethod.GetParameters(), eventInfo.Name, name);
				}

				if( methods.Count != 0 || isDefineTable ) {
					sw.WriteLine("return " + name);
				}

				//清空缓冲区
				sw.Flush();
				//关闭流
				sw.Close();
				fs.Close();
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
		}
	}

	public class GlobalAPI {
		#region static

		private static string m_globalFile = Application.dataPath + "/../UnityLuaAPI/cs_global.lua";
		private static readonly Dictionary<string, GlobalAPI> m_dict = new Dictionary<string, GlobalAPI>();
		private static GlobalAPI root;

		public static void Clear() {
			m_dict.Clear();
			root = null;
		}

		public static void WriteToFile() {
			if( File.Exists(m_globalFile) ) {
				File.Delete(m_globalFile);
			}

			File.WriteAllText(m_globalFile, root.ToString());
		}

		public static void AddType(string fullName) {
			FindFahter(fullName);
		}

		public static void GetFatherAndSelfName(string fullName, out string fatherFullName, out string self) {
			fatherFullName = "";
			self = "";
			string[] nameList = fullName.Split(new char[] {'.'});
			if( nameList.Length > 0 ) {
				self = nameList[nameList.Length - 1];
			}

			if( nameList.Length > 1 ) {
				for( int i = 0, imax = nameList.Length - 1; i < imax; i++ ) {
					fatherFullName += nameList[i];
					if( i != nameList.Length - 2 ) {
						fatherFullName += ".";
					}
				}
			}
		}

		public static GlobalAPI FindFahter(string fullName) {
			GlobalAPI aPI = null;

			if( !m_dict.TryGetValue(fullName, out aPI) ) {
				string selfName;
				string fatherFullName;
				GetFatherAndSelfName(fullName, out fatherFullName, out selfName);

				if( !string.IsNullOrEmpty(selfName) ) {
					aPI = new GlobalAPI(selfName);
					m_dict.Add(fullName, aPI);
					if( string.IsNullOrEmpty(fatherFullName) && root == null ) {
						root = aPI;
					}
				}

				//not root
				if( !string.IsNullOrEmpty(fatherFullName) ) {
					var fatherAPI = FindFahter(fatherFullName);
					fatherAPI.AddChild(aPI);
				}
			}

			return aPI;
		}

		#endregion

		public string className { private set; get; }
		public GlobalAPI father { private set; get; }

		private string m_fullName;

		public string fullName {
			get {
				if( m_fullName == null ) {
					m_fullName = father == null ? className : father.fullName + "." + className;
				}

				return m_fullName;
			}
		}

		private readonly List<GlobalAPI> childList = new List<GlobalAPI>();

		private GlobalAPI(string className) {
			this.className = className;
		}

		private void AddChild(GlobalAPI child) {
			child.father = this;
			childList.Add(child);
		}

		public override string ToString() {
			var sb = new StringBuilder();
			sb.AppendCR($"---@class {fullName}");

			for( int i = 0, imax = childList.Count; i < imax; i++ ) {
				var item = childList[i];
				sb.AppendCR($"---@field public {item.className} {item.fullName}");
			}

			if( root == this ) {
				sb.AppendCR(fullName + " = { }");
			}

			sb.AppendCR("");

			for( int i = 0, imax = childList.Count; i < imax; i++ ) {
				var item = childList[i];
				if( item.childList.Count > 0 ) {
					sb.Append(item.ToString());
				}
			}

			return sb.ToString();
		}
	}

	//扔掉 重载函数中参数数量相同，且其中只有一个参数是type类型另一个是string类型
	private static void RemoveRewriteFunHasTypeAndString(List<MethodInfo> methodList) {
		for( int i = methodList.Count - 1; i >= 0; i-- ) {
			var method1 = methodList[i];
			for( int j = i - 1; j >= 0; j-- ) {
				var method2 = methodList[j];
				if( MethodParamHasTypeOrString(method1, method2, out var isMt1Type) ) {
					methodList.RemoveAt(isMt1Type ? j : i);
					break;
				}
			}
		}
	}

	private static bool CheckType(ParameterInfo info) {
		return info.ParameterType == typeof(Type) || info.ParameterType == typeof(string);
	}

	private static bool MethodParamHasTypeOrString(MethodInfo method1, MethodInfo method2, out bool isMt1Type) {
		bool result = false;
		isMt1Type = false;

		ParameterInfo[] paramInfos1 = method1.GetParameters();
		ParameterInfo[] paramInfos2 = method2.GetParameters();

		bool isFirstCheck = false;
		if( paramInfos1.Length != paramInfos2.Length ) goto Exit0;
		for( int i = 0, imax = paramInfos1.Length; i < imax; i++ ) {
			if( paramInfos1[i].ParameterType != paramInfos2[i].ParameterType ) {
				if( isFirstCheck ) goto Exit0;
				if( !CheckType(paramInfos1[i]) ) goto Exit0;
				if( !CheckType(paramInfos2[i]) ) goto Exit0;
				isMt1Type = paramInfos1[i].ParameterType == typeof(Type);
				isFirstCheck = true;
			}
		}

		result = true;
		Exit0:
		return result;
	}

	private static bool IsObsolete(MemberInfo mb) {
		object[] attrs = mb.GetCustomAttributes(true);
		return attrs.Select(t1 => t1.GetType()).Any(t => t == typeof(ObsoleteAttribute) || t == typeof(BlackListAttribute));
	}

	private static void WriteField(TextWriter sw, Type returnType, string fieldName) {
		sw.WriteLine($"---@field public {fieldName} {ConvertToLuaType(returnType)}");
	}

	private static void WriteFunComment(StreamWriter sw, List<MethodInfo> list) {
		for( int i = 0, imax = list.Count; i < imax - 1; i++ ) {
			WriteOneComment(sw, list[i]);
		}
	}

	private static void WriteCtorComment(StreamWriter sw, List<ConstructorInfo> list) {
		for( int i = 0, imax = list.Count; i < imax - 1; i++ ) {
			WriteOneComment(sw, list[i]);
		}
	}

	private static void WriteOneComment(StreamWriter sw, MethodBase method) {
		string argsStr = "";
		ParameterInfo[] paramInfos = method.GetParameters();
		if( method.IsDefined(typeof(ExtensionAttribute), false) ) {
			var newParamInfos = new List<ParameterInfo>(paramInfos);
			newParamInfos.RemoveAt(0);
			paramInfos = newParamInfos.ToArray();
		}

		for( int i = 0, imax = paramInfos.Length; i < imax; i++ ) {
			if( i != 0 ) {
				argsStr += ", ";
			}

			argsStr += ReplaceLuaKeyWord(paramInfos[i].Name) + ":" + ConvertToLuaType(paramInfos[i].ParameterType);
		}

		Type t;
		if( method is MethodInfo info ) {
			t = info.ReturnType;
			string tStr = typeof(void) == t ? "" : ": " + ConvertToLuaType(t);
			sw.WriteLine("---@overload fun({0}){1}", argsStr, tStr);
		}
		else if( method is ConstructorInfo ) {
			t = method.ReflectedType;
			string tStr = typeof(void) == t ? "" : ": " + ConvertToLuaType(t);
			sw.WriteLine("---@overload fun({0}){1}", argsStr, tStr);
		}
	}

	private static void WriteEvent(TextWriter sw, ParameterInfo[] paramInfos, string methodName, string className) {
		sw.WriteLine($"---@param addOrRemove string | \"'+'\" | \"'-'\"");
		for( int i = 0, imax = paramInfos.Length; i < imax; i++ ) {
			sw.WriteLine($"---@param {ReplaceLuaKeyWord(paramInfos[i].Name)} {ConvertToLuaType(paramInfos[i].ParameterType)}");
		}

		string argsStr = "addOrRemove, ";
		for( int i = 0, imax = paramInfos.Length; i < imax; i++ ) {
			if( i != 0 ) {
				argsStr += ", ";
			}

			argsStr += ReplaceLuaKeyWord(paramInfos[i].Name);
		}

		sw.WriteLine($"function {className}:{methodName}({argsStr}) end");
	}

	private static void WriteFun(StreamWriter sw, int delta, ParameterInfo[] paramInfos, Type methodReturnType, string methodName, string className,
		bool isStatic) {
		string typeStr = ConvertToLuaType(methodReturnType);
		if( methodReturnType != typeof(void) ) {
			sw.WriteLine($"---@return {typeStr}");
		}

		for( int i = 0, imax = paramInfos.Length; i < imax; i++ ) {
			var pInfo = paramInfos[i];
			if(pInfo.IsOut) {
				continue;
			}
			sw.WriteLine($"---@param {ReplaceLuaKeyWord(pInfo.Name)} {ConvertToLuaType(pInfo.ParameterType)}");
		}

		string argsStr = "";
		for( int i = 0, imax = paramInfos.Length; i < imax; i++ ) {
			if( i != 0 ) {
				argsStr += ", ";
			}

			argsStr += ReplaceLuaKeyWord(paramInfos[i].Name);
		}

		string dot = string.IsNullOrEmpty(methodName) ? "" : isStatic ? "." : ":";
		sw.WriteLine($"function {className}{dot}{methodName}({argsStr}) end");
	}

	private static string ConvertToLuaType(Type methodReturnType) {
		string result = "";

		if( methodReturnType != typeof(void) ) {
			if( methodReturnType == typeof(bool) ) {
				result = "boolean";
			}
			else if( methodReturnType == typeof(long) || methodReturnType == typeof(ulong) ) {
				result = "number";
			}
			else if( methodReturnType.IsPrimitive || methodReturnType.IsEnum ) {
				result = "number";
			}
			else if( methodReturnType == typeof(string) ) {
				result = "string";
			}
			else if( methodReturnType == typeof(LuaFunction) || methodReturnType.IsSubclassOf(typeof(Delegate)) ) {
				if( methodReturnType.IsSubclassOf(typeof(Delegate)) ) {
					var invokeType = methodReturnType.GetMethod("Invoke");
					var returnType = invokeType.ReturnType;
					var parameters = invokeType.GetParameters();

					string returnStr = ConvertToLuaType(returnType);
					if( string.IsNullOrEmpty(returnStr) ) {
						returnStr = "void";
					}

					string parameterStr = "";
					for( int i = 0, imax = parameters.Length; i < imax; i++ ) {
						var item = parameters[i];
						parameterStr += item.Name + ":" + ConvertToLuaType(item.ParameterType);
						if( i != imax - 1 ) {
							parameterStr += ", ";
						}

						if( item.IsOut || item.IsRetval ) {
							returnStr += ", " + ConvertToLuaType(item.ParameterType);
						}
					}

					result = $"fun({parameterStr})" + (returnStr == "void" ? "" : $": {returnStr}");
				}
				else {
					result = "fun()";
				}
			}
			else if( methodReturnType == typeof(Type) ) {
				result = "string";
			}
			else if( methodReturnType.IsArray || methodReturnType.IsSubclassOf(typeof(IList)) ) {
				var t = methodReturnType.GetElementType();
				result = t.Name + "[]";
			}
			else if( methodReturnType == typeof(LuaTable) ) {
				result = "table";
			}
			else {
				result = TypeDecl(methodReturnType);
			}
		}

		return result;
	}

	private static string ReplaceLuaKeyWord(string name) {
		if( name == "table" ) {
			name = "tb";
		}
		else if( name == "function" ) {
			name = "func";
		}
		else if( name == "fun" ) {
			name = "func";
		}
		else if( name == "type" ) {
			name = "t";
		}
		else if( name == "end" ) {
			name = "ed";
		}
		else if( name == "local" ) {
			name = "loc";
		}
		else if( name == "and" ) {
			name = "ad";
		}
		else if( name == "or" ) {
			name = "orz";
		}
		else if( name == "not" ) {
			name = "no";
		}

		return name;
	}

	#endregion

	private static void GenConfigApi() {
		var xlsxDir = Application.dataPath + "/Res/Xlsx";
		var extendRelations = new Dictionary<string, string>();
		var extendConfigFile = xlsxDir + "/extendsInfo.tsv"; 
		if( File.Exists(extendConfigFile) ) {
			var lines = File.ReadAllLines(extendConfigFile);
			for( int i = 2; i < lines.Length; i++ ) {
				var line = lines[i];
				var segments = line.Split('\t');
				var baseName = segments[1];
				var extendTypeNames = segments[2].Split(',');
				foreach( var typeName in extendTypeNames ) {
					extendRelations.Add(typeName, baseName);
				}
			}
		}

		var tsvFiles = Directory.GetFiles(xlsxDir, "*.tsv");
		for( int i = 0; i < tsvFiles.Length; i++ ) {
			var tsvFile = tsvFiles[i];
			if( tsvFile.Contains("_i18n") )
				continue;

			EditorUtility.DisplayProgressBar("Xlsx process", tsvFile, i / (float)tsvFiles.Length);
			var className = Path.GetFileNameWithoutExtension(tsvFile);
			if( className == "extendsInfo" )
				continue;

			using( var reader = new StreamReader(tsvFile) ) {
				var keyRow = reader.ReadLine();
				var typeRow = reader.ReadLine();
				var keys = keyRow.Split('\t');
				var types = typeRow.Split('\t');
				using( var writer = new StreamWriter(m_apiDir + $"/Config_{className}.lua", false, Encoding.UTF8) ) {
					if( extendRelations.TryGetValue(className, out var baseTypeName) ) {
						writer.WriteLine($"---@class Config_{className} : Config_{baseTypeName}");
					}
					else {
						writer.WriteLine($"---@class Config_{className}");
					}

					for( int j = 0; j < keys.Length; j++ ) {
						var key = keys[j];
						var type = types[j];
						writer.WriteLine($"---@field public {key} {ConvertTsvType(type, key)}");
					}
				}
			}
		}
	}

	private static string ConvertTsvType(string tsvType, string key) {
		return tsvType switch {
			"string" => "string",
			"int" => "integer",
			"number" => "number",
			"json" => "table",
			"link" => $"Config_{key}",
			"links" => $"Config_{key}[]",
			"boolean" => "boolean",
			"translate" => "string",
			"asset" => "CS.Extend.Asset.AssetReference",
			"js" => "fun():number",
			"color" => "CS.UnityEngine.Color",
			_ => throw new Exception($"Unknown type : {tsvType} : {key}")
		};
	}
}