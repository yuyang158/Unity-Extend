using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using XLua;

namespace Extend.Editor
{
    public class ConsoleLuaJumper
    {
        private class LogEditorConfig
        {
            public string logScriptPath = "";   //自定义日志脚本路径
            public string logTypeName = "";     //脚本type
            public int instanceID = 0;

            public LogEditorConfig(string logScriptPath, System.Type logType)
            {
                this.logScriptPath = logScriptPath;
                this.logTypeName = logType.FullName;
            }
        }

        //能够打印出日志的代码，可以根据需求，继续添加成一个列表
        private static LogEditorConfig[] _logEditorConfig = new LogEditorConfig[]
        {
            new LogEditorConfig("Assets/XLua/Src/LuaEnv.cs",typeof(XLua.LuaEnv)),
            new LogEditorConfig("Assets/XLua/Src/StaticLuaCallbacks.cs", typeof(StaticLuaCallbacks)),
        };

        //处理从ConsoleWindow双击跳转
        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            for (int i = _logEditorConfig.Length - 1; i >= 0; --i)
            {
                var configTmp = _logEditorConfig[i];
                UpdateLogInstanceID(configTmp);
                if (instanceID == configTmp.instanceID)
                {
                    var stackTrace = GetStackTrace();
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        string traceFirstLine = GetTraceFirstLine(stackTrace);
                        string lineStr, filepath;
                        ParseFilepathAndLineInFirstLine(traceFirstLine, out filepath, out lineStr);
                        string apppath = EditorPrefs.GetString("kScriptsDefaultApp_h2657262712");
                        // ide 需要事先设置命令好绑定
                        if (apppath.Contains("Rider"))
                        {
                            string binpath = apppath.Replace("rider64.exe", "rider.bat");
                            System.Diagnostics.Process.Start($"\"{binpath}\"", $"--line {lineStr} \"{filepath}\"");
                            return true;
                        }
                        else if (apppath.Contains("Code"))
                        {
                            string binpath = apppath.Replace("Code.exe", "bin/code");
                            System.Diagnostics.Process.Start($"\"{binpath}\"", $"-n -g \"{filepath}:{lineStr}\"");
                            return true;
                        }
                    }
                    break;
                }
            }

            return false;//*/
        }
        /// <summary>
        /// 打开指定文件，但是只能打开处于Assets文件夹下的文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        static void OpenFileOnSpecificLineAndColumn(string filePath, int line, int column)
        {
            Assembly.GetAssembly(typeof(EditorApplication))
                .GetType("UnityEditor.LogEntries")
                .GetMethod(nameof(OpenFileOnSpecificLineAndColumn), BindingFlags.Static | BindingFlags.Public)
                .Invoke(null, new object[] { filePath, line, column });
        }
        
        /// <summary>
        /// 反射出日志堆栈
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null != consoleWindowInstance)
            {
                if ((object)EditorWindow.focusedWindow == consoleWindowInstance)
                {
                    fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
                    return activeText;
                }
            }
            return "";
        }

        private static void UpdateLogInstanceID(LogEditorConfig config)
        {
            if (config.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.logScriptPath);
            if (null == assetLoadTmp)
            {
                throw new System.Exception("not find asset by path=" + config.logScriptPath);
            }
            config.instanceID = assetLoadTmp.GetInstanceID();
        }

        private static string GetTraceFirstLine(string allContent)
        {
            string[] lines = allContent.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("\t") && lines[i].Contains("lua"))
                {
                    return lines[i];
                }
            }
            return string.Empty;
        }
        
        private static void ParseFilepathAndLineInFirstLine(string firstLine, out string filepath, out string line)
        {
            string[] parts = firstLine.Split(':');

            filepath = Application.dataPath.Replace("Assets", "Lua/") + parts[0].Replace("\t", "");
            line = parts[1];
        }
        /*
        public static string InsertHyperLink(string origin)
        {
            string path;
            int line;
            ParserLuaStackLine(origin, out path, out line);
            StringBuilder textWithHyperLink = new StringBuilder();
            textWithHyperLink.Append('\t');
            textWithHyperLink.Append("<a href=\"" + path + "\"" + " line=\"" + line + "\">");
            textWithHyperLink.Append(filePath + ":" + lineString);
            textWithHyperLink.Append("</a>)\n");
        }
        */
    }
}