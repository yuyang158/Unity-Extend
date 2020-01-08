/*   
* ==============================================================================
* Filename: EmmyLuaAPIMaker
* Created:  2016/4/5 12:38:24
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
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
using UnityEngine.SceneManagement;
using XLua;

static public class EmmyLuaAPIMaker
{

    #region member
    private static string m_apiDir = Application.dataPath + "/../UnityLuaAPI";
    private static string m_argsText;
    #endregion

    #region menu
    [MenuItem("Tools/程序/EmmyLua/Generate All API")]
    public static void GenAll()
    {
        GenWaperAPI();
    }

    public static void GenWaperAPI()
    {
        try
        {
            TemplateRef template_ref = ScriptableObject.CreateInstance<TemplateRef>();
            if (template_ref.LuaClassWrap == null)
            {
                return;
            }

            Generator.GetGenConfig(Utils.GetAllTypes());
            ExportLuaApi(Generator.LuaCallCSharp);
        }
        catch
        {

        }
    }

    static HashSet<GameObject> roots = new HashSet<GameObject>();
    #endregion

    #region export api

    #region type Name
    public static string TypeDecl(Type t, bool isFull = true)
    {
        string result = "";
        if (t.IsGenericType)
        {
            string ret = GenericBaseName(t, isFull);

            string gs = "";
            gs += "<";
            Type[] types = t.GetGenericArguments();
            for (int n = 0; n < types.Length; n++)
            {
                gs += TypeDecl(types[n]);
                if (n < types.Length - 1)
                    gs += ",";
            }
            gs += ">";

            ret = Regex.Replace(ret, @"`\d", gs);

            result = ret;
        }
        else if (t.IsArray)
        {
            result = TypeDecl(t.GetElementType()) + "[]";
        }
        else
        {
            result = RemoveRef(t.ToString(), false);
        }
        result = result.Replace("<", "_")
                .Replace(",", "_").Replace(">", "");
        return "CS." + result;
    }
    private static string GenericBaseName(Type t, bool isFull)
    {
        string n;
        if (isFull)
        {
            n = t.FullName;
        }
        else
        {
            n = t.Name;
        }
        if (n.IndexOf('[') > 0)
        {
            n = n.Substring(0, n.IndexOf('['));
        }
        return n.Replace("+", ".");
    }
    static string[] prefix = new string[] { "System.Collections.Generic" };
    private static string RemoveRef(string s, bool removearray = true)
    {
        if (s.EndsWith("&")) s = s.Substring(0, s.Length - 1);
        if (s.EndsWith("[]") && removearray) s = s.Substring(0, s.Length - 2);
        if (s.StartsWith(prefix[0])) s = s.Substring(prefix[0].Length + 1, s.Length - prefix[0].Length - 1);

        s = s.Replace("+", ".");
        if (s.Contains("`"))
        {
            string regstr = @"`\d";
            Regex r = new Regex(regstr, RegexOptions.None);
            s = r.Replace(s, "");
            s = s.Replace("[", "<");
            s = s.Replace("]", ">");
        }
        return s;
    }

    #endregion
    private static string GetTypeStr(string type)
    {
        if (type == "int" || type == "long" || type == "float" || type == "short")
        {
            type = "number";
        }
        return type;
    }

    public static void AppendCR(this StringBuilder sb, string str)
    {
        sb.Append(str + "\r\n");
    }
    private static XmlDocument loadXML(string fileName)
    {
        string filepath = fileName;
        if (File.Exists(filepath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            //根据路径将XML读取出来
            xmlDoc.Load(filepath);
            return xmlDoc;
        }
        return null;
    }
    static IEnumerable<Type> type_has_extension_methods = null;
    static IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType)
    {
        var LuaCallCSharp = Generator.LuaCallCSharp;
        if (type_has_extension_methods == null)
        {
            var gen_types = LuaCallCSharp;

            type_has_extension_methods = from type in gen_types
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

    static bool isSupportedExtensionMethod(MethodBase method, Type extendedType)
    {
        if (!method.IsDefined(typeof(ExtensionAttribute), false))
            return false;
        var methodParameters = method.GetParameters();
        if (methodParameters.Length < 1)
            return false;

        var hasValidGenericParameter = false;
        for (var i = 0; i < methodParameters.Length; i++)
        {
            var parameterType = methodParameters[i].ParameterType;
            if (i == 0)
            {
                if (parameterType.IsGenericParameter)
                {
                    var parameterConstraints = parameterType.GetGenericParameterConstraints();
                    if (parameterConstraints.Length == 0) return false;
                    bool firstParamMatch = false;
                    foreach (var parameterConstraint in parameterConstraints)
                    {
                        if (parameterConstraint != typeof(ValueType) && parameterConstraint.IsAssignableFrom(extendedType))
                        {
                            firstParamMatch = true;
                        }
                    }
                    if (!firstParamMatch) return false;

                    hasValidGenericParameter = true;
                }
                else if (parameterType != extendedType)
                    return false;
            }
            else if (parameterType.IsGenericParameter)
            {
                var parameterConstraints = parameterType.GetGenericParameterConstraints();
                if (parameterConstraints.Length == 0) return false;
                foreach (var parameterConstraint in parameterConstraints)
                {
                    if (!parameterConstraint.IsClass || (parameterConstraint == typeof(ValueType)) || hasGenericParameter(parameterConstraint))
                        return false;
                }
                hasValidGenericParameter = true;
            }
        }
        return hasValidGenericParameter || !method.ContainsGenericParameters;
    }

    static bool hasGenericParameter(Type type)
    {
        if (type.IsByRef || type.IsArray)
        {
            return hasGenericParameter(type.GetElementType());
        }
        if (type.IsGenericType)
        {
            foreach (var typeArg in type.GetGenericArguments())
            {
                if (hasGenericParameter(typeArg))
                {
                    return true;
                }
            }
            return false;
        }
        return type.IsGenericParameter;
    }

    public static void ExportLuaApi(List<Type> classList)
    {
        type_has_extension_methods = null;
        GlobalAPI.Clear();

        // add class here
        BindingFlags bindType = BindingFlags.DeclaredOnly |
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
        List<MethodInfo> methods;
        FieldInfo[] fields;
        PropertyInfo[] properties;
        List<ConstructorInfo> constructors;
        ParameterInfo[] paramInfos;
        int delta;
        string path = m_apiDir;
        if (path == "")
        {
            return;
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        foreach (Type t in classList)
        {
            string name = TypeDecl(t);
            GlobalAPI.AddType(name);
            try
            {
                FileStream fs = new FileStream((string)(path + "/" + name.Replace(".", "_") + ".lua"), FileMode.Create);
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                StreamWriter sw = new StreamWriter(fs, utf8WithoutBom);

                if (t.BaseType != null && t.BaseType != typeof(object))
                {
                    string baseName = TypeDecl(t.BaseType);
                    sw.WriteLine(string.Format("---@class {0} : {1}", (object)name, baseName));
                }
                else
                {
                    sw.WriteLine(string.Format("---@class {0}", (object)name));
                }

                #region field
                fields = t.GetFields(bindType);
                foreach (var field in fields)
                {
                    if (IsObsolete(field))
                    {
                        continue;
                    }
                    WriteField(sw, field.FieldType, field.Name);
                }

                properties = t.GetProperties(bindType);
                foreach (var property in properties)
                {
                    if (IsObsolete(property))
                    {
                        continue;
                    }
                    WriteField(sw, property.PropertyType, property.Name);
                }
                #endregion
                sw.WriteLine("");

                //string[] nameList = name.Split(new char[] { '.' });
                //string nameSpace = "";
                //for (int i = 0; i < nameList.Length - 1; i++)
                //{
                //    if (string.IsNullOrEmpty(nameSpace))
                //    {
                //        nameSpace = nameList[i];
                //    }
                //    else
                //    {
                //        nameSpace = nameSpace + "." + nameList[i];
                //    }
                //    sw.WriteLine(nameSpace + " = { }");
                //}

                sw.WriteLine(string.Format("---@type {0}", name));
                sw.WriteLine(name + " = { }");

                #region constructor
                constructors = new List<ConstructorInfo>(t.GetConstructors(bindType));
                constructors.Sort((left, right) => { return left.GetParameters().Length - right.GetParameters().Length; });
                bool isDefineTable = false;

                if (constructors.Count > 0)
                {
                    WriteCtorComment(sw, constructors);
                    paramInfos = constructors[constructors.Count - 1].GetParameters();
                    delta = paramInfos.Length - constructors[0].GetParameters().Length;
                    WriteFun(sw, delta, paramInfos, t, "New", name, true);
                    isDefineTable = true;
                }

                #endregion

                #region method
                methods = new List<MethodInfo>(t.GetMethods(bindType));
                var externMethod = GetExtensionMethods(t);

                foreach (var item in externMethod)
                {
                    methods.Add(item);
                }

                MethodInfo method;

                Dictionary<string, List<MethodInfo>> methodDict = new Dictionary<string, List<MethodInfo>>();

                for (int i = 0; i < methods.Count; i++)
                {
                    method = methods[i];
                    string methodName = method.Name;
                    if (IsObsolete(method))
                    {
                        continue;
                    }
                    if (method.IsGenericMethod) { continue; }
                    if (!method.IsPublic) continue;
                    if (methodName.StartsWith("get_") || methodName.StartsWith("set_")) continue;

                    List<MethodInfo> list;
                    if (!methodDict.TryGetValue(methodName, out list))
                    {
                        list = new List<MethodInfo>();
                        methodDict.Add(methodName, list);
                    }
                    list.Add(method);
                }

                var itr = methodDict.GetEnumerator();
                while (itr.MoveNext())
                {
                    List<MethodInfo> list = itr.Current.Value;
                    RemoveRewriteFunHasTypeAndString(list);

                    list.Sort((left, right) =>
                    {
                        int leftLen = left.GetParameters().Length;
                        int rightLen = right.GetParameters().Length;
                        if (left.IsDefined(typeof(ExtensionAttribute), false))
                        {
                            leftLen--;
                        }
                        if (right.IsDefined(typeof(ExtensionAttribute), false))
                        {
                            rightLen--;
                        }
                        return leftLen - rightLen;
                    });
                    WriteFunComment(sw, list);
                    paramInfos = list[list.Count - 1].GetParameters();

                    if (list[list.Count - 1].IsDefined(typeof(ExtensionAttribute), false))
                    {
                        var newParamInfos = new List<ParameterInfo>(paramInfos);
                        newParamInfos.RemoveAt(0);
                        paramInfos = newParamInfos.ToArray();
                    }

                    delta = paramInfos.Length - list[0].GetParameters().Length;
                    WriteFun(sw, delta, paramInfos, list[0].ReturnType, list[0].Name, name, list[0].IsStatic);
                }
                itr.Dispose();

                if (methods.Count != 0 || isDefineTable)
                {
                    sw.WriteLine("return " + name);
                }
                #endregion

                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(name + "\n" + e.Message);
            }

        }
        /*string zipName = path + ".zip";
        if (File.Exists(zipName))
        {
            File.Delete(zipName);
        }
        GlobalAPI.WriteToFile();*/
        //ZipTool.ZipDirectory(path, zipName, 7);
        //Directory.Delete(path, true);

        Debug.Log("转换完成");
    }

    public class GlobalAPI
    {
        #region static
        private static string m_globalFile = Application.dataPath + "/../UnityLuaAPI/cs_global.lua";
        private static readonly Dictionary<string, GlobalAPI> m_dict = new Dictionary<string, GlobalAPI>();
        private static GlobalAPI root;
        public static void Clear()
        {
            m_dict.Clear();
            root = null;
        }

        public static void WriteToFile()
        {
            if (File.Exists(m_globalFile))
            {
                File.Delete(m_globalFile);
            }
            File.WriteAllText(m_globalFile, root.ToString());
        }

        public static void AddType(string fullName)
        {
            FindFahter(fullName);
        }
        public static void GetFatherAndSelfName(string fullName, out string fatherFullName, out string self)
        {
            fatherFullName = "";
            self = "";
            string[] nameList = fullName.Split(new char[] { '.' });
            if (nameList.Length > 0)
            {
                self = nameList[nameList.Length - 1];
            }
            if (nameList.Length > 1)
            {
                for (int i = 0, imax = nameList.Length - 1; i < imax; i++)
                {
                    fatherFullName += nameList[i];
                    if (i != nameList.Length - 2)
                    {
                        fatherFullName += ".";
                    }
                }
            }
        }
        public static GlobalAPI FindFahter(string fullName)
        {
            GlobalAPI aPI = null;

            if (!m_dict.TryGetValue(fullName, out aPI))
            {
                string selfName;
                string fatherFullName;
                GetFatherAndSelfName(fullName, out fatherFullName, out selfName);

                if (!string.IsNullOrEmpty(selfName))
                {
                    aPI = new GlobalAPI(selfName);
                    m_dict.Add(fullName, aPI);
                    if (string.IsNullOrEmpty(fatherFullName) && root == null)
                    {
                        root = aPI;
                    }
                }

                //not root
                if (!string.IsNullOrEmpty(fatherFullName))
                {
                    GlobalAPI fatherAPI = FindFahter(fatherFullName);
                    fatherAPI.AddChild(aPI);
                }
            }

            return aPI;
        }
        #endregion

        public string className { private set; get; }
        public GlobalAPI father { private set; get; }

        private string m_fullName;
        public string fullName
        {
            get
            {
                if (m_fullName == null)
                {
                    m_fullName = father == null ? className : father.fullName + "." + className;
                }
                return m_fullName;
            }
        }
        private readonly List<GlobalAPI> childList = new List<GlobalAPI>();
        private GlobalAPI(string className)
        {
            this.className = className;
        }
        private void AddChild(GlobalAPI child)
        {
            child.father = this;
            childList.Add(child);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendCR(string.Format("---@class {0}", fullName));

            for (int i = 0, imax = childList.Count; i < imax; i++)
            {
                var item = childList[i];
                sb.AppendCR(string.Format("---@field public {0} {1}", item.className, item.fullName));
            }
            if (root == this)
            {
                sb.AppendCR(fullName + " = { }");
            }

            sb.AppendCR("");

            for (int i = 0, imax = childList.Count; i < imax; i++)
            {
                var item = childList[i];
                if (item.childList.Count > 0)
                {
                    sb.Append(item.ToString());
                }
            }

            return sb.ToString();
        }
    }

    //扔掉 重载函数中参数数量相同，且其中只有一个参数是type类型另一个是string类型
    static void RemoveRewriteFunHasTypeAndString(List<MethodInfo> methodList)
    {
        MethodInfo method1;
        MethodInfo method2;
        for (int i = methodList.Count - 1; i >= 0; i--)
        {
            method1 = methodList[i];
            for (int j = i - 1; j >= 0; j--)
            {
                method2 = methodList[j];
                bool isMt1Type;
                if (MethodParamHasTypeOrString(method1, method2, out isMt1Type))
                {
                    if (isMt1Type)
                    {
                        methodList.RemoveAt(j);
                    }
                    else
                    {
                        methodList.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
    static bool CheckType(ParameterInfo info)
    {
        return info.ParameterType == typeof(Type) || info.ParameterType == typeof(string);
    }

    static bool MethodParamHasTypeOrString(MethodInfo method1, MethodInfo method2, out bool isMt1Type)
    {
        bool result = false;
        isMt1Type = false;

        ParameterInfo[] paramInfos1 = method1.GetParameters();
        ParameterInfo[] paramInfos2 = method2.GetParameters();

        bool isFirstCheck = false;
        if (paramInfos1.Length != paramInfos2.Length) goto Exit0;
        for (int i = 0, imax = paramInfos1.Length; i < imax; i++)
        {
            if (paramInfos1[i].ParameterType != paramInfos2[i].ParameterType)
            {
                if (isFirstCheck) goto Exit0;
                if (!CheckType(paramInfos1[i])) goto Exit0;
                if (!CheckType(paramInfos2[i])) goto Exit0;
                isMt1Type = paramInfos1[i].ParameterType == typeof(Type);
                isFirstCheck = true;
            }
        }

        result = true;
    Exit0:
        return result;
    }

    public static bool IsObsolete(MemberInfo mb)
    {
        object[] attrs = mb.GetCustomAttributes(true);

        for (int j = 0; j < attrs.Length; j++)
        {
            Type t = attrs[j].GetType();

            if (t == typeof(System.ObsoleteAttribute) || t == typeof(XLua.BlackListAttribute))
            {
                return true;
            }
        }

        return false;
    }
    static void WriteField(StreamWriter sw, Type returnType, string fieldName)
    {
        sw.WriteLine(string.Format("---@field public {0} {1}", fieldName, ConvertToLuaType(returnType)));
    }
    static void WriteFunComment(StreamWriter sw, List<MethodInfo> list)
    {
        for (int i = 0, imax = list.Count; i < imax - 1; i++)
        {
            WriteOneComment(sw, list[i]);
        }
    }
    static void WriteCtorComment(StreamWriter sw, List<ConstructorInfo> list)
    {
        for (int i = 0, imax = list.Count; i < imax - 1; i++)
        {
            WriteOneComment(sw, list[i]);
        }
    }
    static void WriteOneComment(StreamWriter sw, MethodBase method)
    {
        ParameterInfo[] paramInfos;
        string argsStr = "";
        paramInfos = method.GetParameters();
        if (method.IsDefined(typeof(ExtensionAttribute), false))
        {
            var newParamInfos = new List<ParameterInfo>(paramInfos);
            newParamInfos.RemoveAt(0);
            paramInfos = newParamInfos.ToArray();
        }
        for (int i = 0, imax = paramInfos.Length; i < imax; i++)
        {
            if (i != 0)
            {
                argsStr += ", ";
            }
            argsStr += RepalceLuaKeyWord(paramInfos[i].Name) + ":" + ConvertToLuaType(paramInfos[i].ParameterType);
        }
        Type t;
        if (method is MethodInfo)
        {
            t = ((MethodInfo)method).ReturnType;
            string tStr = typeof(void) == t ? "void" : ConvertToLuaType(t);
            sw.WriteLine("---@overload fun({0}): {1}", argsStr, tStr);
        }
        else if (method is ConstructorInfo)
        {
            t = method.ReflectedType;
            string tStr = typeof(void) == t ? "void" : ConvertToLuaType(t);
            sw.WriteLine("---@overload fun({0}): {1}", argsStr, tStr);
        }
    }
    static void WriteFun(StreamWriter sw, int delta, ParameterInfo[] paramInfos, Type methodReturnType, string methodName, string className, bool isStatic)
    {
        string typeStr = ConvertToLuaType(methodReturnType);
        if (methodReturnType != typeof(void))
        {
            sw.WriteLine(string.Format("---@return {0}", typeStr));
        }
        for (int i = 0, imax = paramInfos.Length; i < imax; i++)
        {
            if (imax - i <= delta)
            {
                sw.WriteLine(string.Format("---@param optional {0} {1}", RepalceLuaKeyWord(paramInfos[i].Name), ConvertToLuaType(paramInfos[i].ParameterType)));
            }
            else
            {
                sw.WriteLine(string.Format("---@param {0} {1}", RepalceLuaKeyWord(paramInfos[i].Name), ConvertToLuaType(paramInfos[i].ParameterType)));
            }
        }
        string argsStr = "";
        for (int i = 0, imax = paramInfos.Length; i < imax; i++)
        {
            if (i != 0)
            {
                argsStr += ", ";
            }
            argsStr += RepalceLuaKeyWord(paramInfos[i].Name);
        }
        string dot = isStatic ? "." : ":";
        sw.WriteLine(string.Format("function {0}{1}{2}({3}) end", className, dot, methodName, argsStr));
    }
    static string ConvertToLuaType(Type methodReturnType)
    {
        string result = "";

        if (methodReturnType != typeof(void))
        {

            if (methodReturnType == typeof(bool))
            {
                result = "boolean";
            }
            else if (methodReturnType == typeof(long) || methodReturnType == typeof(ulong))
            {
                result = "number";
            }
            else if (methodReturnType.IsPrimitive || methodReturnType.IsEnum)
            {
                result = "number";
            }
            else if (methodReturnType == typeof(string))
            {
                result = "string";
            }
            else if (methodReturnType == typeof(LuaFunction) || methodReturnType.IsSubclassOf(typeof(Delegate)))
            {
                if (methodReturnType.IsSubclassOf(typeof(Delegate)))
                {
                    var invokeType = methodReturnType.GetMethod("Invoke");
                    var returnType = invokeType.ReturnType;
                    var paramters = invokeType.GetParameters();

                    string returnStr = ConvertToLuaType(returnType);
                    if (string.IsNullOrEmpty(returnStr))
                    {
                        returnStr = "void";
                    }
                    string paramterStr = "";
                    for (int i = 0, imax = paramters.Length; i < imax; i++)
                    {
                        var item = paramters[i];
                        paramterStr += item.Name + ":" + ConvertToLuaType(item.ParameterType);
                        if (i != imax - 1)
                        {
                            paramterStr += ", ";
                        }
                        if (item.IsOut || item.IsRetval)
                        {
                            returnStr += ", " + ConvertToLuaType(item.ParameterType);
                        }
                    }
                    result = string.Format("(fun({0}):{1})", paramterStr, returnStr);
                }
                else
                {
                    result = "(fun():void)";
                }
            }
            else if (methodReturnType == typeof(Type))
            {
                result = "string";
            }
            else if (methodReturnType.IsArray || methodReturnType.IsSubclassOf(typeof(IList)))
            {
                Type t = methodReturnType.GetElementType();
                result = t.Name + "[]";
            }
            else if (methodReturnType == typeof(LuaTable))
            {
                result = "table";
            }
            else
            {
                result = TypeDecl(methodReturnType);
            }
        }
        return result;
    }
    static string RepalceLuaKeyWord(string name)
    {
        if (name == "table")
        {
            name = "tb";
        }
        else if (name == "function")
        {
            name = "func";
        }
        else if (name == "fun")
        {
            name = "func";
        }
        else if (name == "type")
        {
            name = "t";
        }
        else if (name == "end")
        {
            name = "ed";
        }
        else if (name == "local")
        {
            name = "loc";
        }
        else if (name == "and")
        {
            name = "ad";
        }
        else if (name == "or")
        {
            name = "orz";
        }
        else if (name == "not")
        {
            name = "no";
        }
        return name;
    }
    #endregion
}
