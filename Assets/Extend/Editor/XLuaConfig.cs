/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using XLua;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class XLuaGenConfig {
	/***************如果你全lua编程，可以参考这份自动化配置***************/
	//--------------begin 纯lua编程配置参考----------------------------
	private static readonly List<string> exclude = new List<string> {
		"HideInInspector", "ExecuteInEditMode",
		"AddComponentMenu", "ContextMenu",
		"RequireComponent", "DisallowMultipleComponent",
		"SerializeField", "AssemblyIsEditorAssembly",
		"Attribute", "Types",
		"UnitySurrogateSelector", "TrackedReference",
		"TypeInferenceRules", "FFTWindow",
		"RPC", "Network", "MasterServer",
		"BitStream", "HostData",
		"ConnectionTesterStatus", "GUI", "EventType",
		"EventModifiers", "FontStyle", "TextAlignment",
		"TextEditor", "TextEditorDblClickSnapping",
		"TextGenerator", "TextClipping", "Gizmos", "Occlusion",
		"ADBannerView", "ADInterstitialAd",
		"Android", "Tizen", "jvalue", "Localiz",
		"iPhone", "iOS", "Windows", "CalendarIdentifier",
		"CalendarUnit", "CalendarUnit", "Editor",
		"ClusterInput", "FullScreenMovieControlMode",
		"FullScreenMovieScalingMode", "Handheld",
		"LocalNotification", "NotificationServices",
		"RemoteNotificationType", "RemoteNotification",
		"SamsungTV", "TextureCompressionQuality",
		"TouchScreenKeyboardType", "TouchScreenKeyboard",
		"MovieTexture", "UnityEngineInternal", "2D", "WWW",
		"Terrain", "Tree", "SplatPrototype",
		"DetailPrototype", "DetailRenderMode", "Wheel",
		"MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
		"SendMouseEvents", "Cursor", "Flash", "ActionScript",
		"OnRequestRebuild", "Ping", "DynamicGI", "AssetBundle",
		"ShaderVariantCollection", "Json",
		"CoroutineTween", "GraphicRebuildTracker", "InputManagerEntry", "InputRegistering",
		"Advertisements", "UnityEditor", "WSA", "StateMachineBehaviour",
		"EventProvider", "Apple", "Motion", "WindZone", "Subsystem",
		"UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
		"NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental", "MeshRenderer",
		"CanvasRenderer", "AnimatorControllerParameter", "AudioSetting", "Caching",
		"DrivenRectTransformTracker", "LightProbeGroup", "Animation", "DefaultControls",
		"UnityEngine.Light", "WebCam", "Human", "QualitySettings", "LOD", "ParticleSystem", "UIVertex",
		"ClusterSerialization", "DefaultExecutionOrder", "Audio", "FrameTimingManager", "Gyroscope"
	};

	private static bool isExcluded(Type type) {
		var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		var invalid = members.Length == 0;
		if( invalid ) {
			return true;
		}

		invalid = true;
		foreach( var member in members ) {
			switch( member.MemberType ) {
				case MemberTypes.All:
				case MemberTypes.Constructor:
				case MemberTypes.NestedType:
				case MemberTypes.Custom:
					break;
				case MemberTypes.Event:
				case MemberTypes.Field:
					invalid = false;
					break;
				case MemberTypes.Method:
					var method = member as MethodInfo;
					if( method.DeclaringType != type && !method.IsStatic ) {
						continue;
					}

					invalid = false;
					break;
				case MemberTypes.Property:
					var property = member as PropertyInfo;
					var get = property.GetMethod;
					if( get != null && get.DeclaringType != type && !get.IsStatic ) {
						continue;
					}

					var set = property.SetMethod;
					if( set != null && set.DeclaringType != type && !set.IsStatic ) {
						continue;
					}

					invalid = false;
					break;
				case MemberTypes.TypeInfo:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if( invalid ) {
			return true;
		}

		if( type.IsSubclassOf(typeof(Exception)) ) {
			return true;
		}

		if( type.IsSubclassOf(typeof(UnityEvent)) ) {
			return true;
		}

		var fullName = type.FullName;
		return exclude.Any(t => fullName.Contains(t));
	}

	private static readonly Type[] exportToLua = {
		typeof(Stopwatch),
		typeof(TextMeshProUGUI),
		typeof(TextMeshPro),
		typeof(TMP_InputField),
		typeof(Tweener),
		typeof(EventSystem),
		typeof(Volume)
	};

	[LuaCallCSharp]
	public static IEnumerable<Type> LuaCallCSharp {
		get {
			var namespaces = new List<string>() // 在这里添加名字空间
			{
				"UnityEngine",
				"UnityEngine.UI",
				"UnityEngine.AI"
			};
			var unityTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where !( assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder )
				from type in assembly.GetExportedTypes()
				where type.Namespace != null && namespaces.Contains(type.Namespace) && !isExcluded(type)
				      && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum && !type.IsValueType
				select type;
			
			var basicMathValueType = new[] {
				typeof(Vector2),
				typeof(Vector3),
				typeof(Vector4),
				typeof(Quaternion)
			};

			unityTypes = unityTypes.Concat(basicMathValueType);
			var customTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where !( assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder )
				from type in assembly.GetExportedTypes()
				where type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum && !isExcluded(type) &&
				      type.GetCustomAttributes(typeof(CSharpCallLuaAttribute), true).Length > 0
				select type;

			var arr = customTypes.ToArray();
			return unityTypes.Concat(arr).Concat(exportToLua);
		}
	}

	//自动把LuaCallCSharp涉及到的delegate加到CSharpCallLua列表，后续可以直接用lua函数做callback
	[CSharpCallLua]
	public static List<Type> CSharpCallLua {
		get {
			var lua_call_csharp = LuaCallCSharp;
			var delegate_types = new List<Type>();
			const BindingFlags flag = BindingFlags.Public | BindingFlags.Instance
			                                              | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;
			foreach( var field in ( from type in lua_call_csharp select type ).SelectMany(type => type.GetFields(flag)) ) {
				if( typeof(Delegate).IsAssignableFrom(field.FieldType) ) {
					delegate_types.Add(field.FieldType);
				}
			}

			foreach( var method in ( from type in lua_call_csharp select type ).SelectMany(type => type.GetMethods(flag)) ) {
				if( typeof(Delegate).IsAssignableFrom(method.ReturnType) ) {
					delegate_types.Add(method.ReturnType);
				}

				foreach( var param in method.GetParameters() ) {
					var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
					if( typeof(Delegate).IsAssignableFrom(paramType) ) {
						delegate_types.Add(paramType);
					}
				}
			}

			var list = delegate_types.Distinct().ToList();
			list.AddRange(
				new[] {
					typeof(Action),
					typeof(Action<float>),
					typeof(Action<bool>),
					typeof(WaitForSeconds),
					typeof(System.Collections.IEnumerator)
				});
			return list;
		}
	}
	//--------------end 纯lua编程配置参考----------------------------

	/***************热补丁可以参考这份自动化配置***************/
	//[Hotfix]
	//static IEnumerable<Type> HotfixInject
	//{
	//    get
	//    {
	//        return (from type in Assembly.Load("Assembly-CSharp").GetExportedTypes()
	//                           where type.Namespace == null || !type.Namespace.StartsWith("XLua")
	//                           select type);
	//    }
	//}
	//--------------begin 热补丁自动化配置-------------------------
	//static bool hasGenericParameter(Type type)
	//{
	//    if (type.IsGenericTypeDefinition) return true;
	//    if (type.IsGenericParameter) return true;
	//    if (type.IsByRef || type.IsArray)
	//    {
	//        return hasGenericParameter(type.GetElementType());
	//    }
	//    if (type.IsGenericType)
	//    {
	//        foreach (var typeArg in type.GetGenericArguments())
	//        {
	//            if (hasGenericParameter(typeArg))
	//            {
	//                return true;
	//            }
	//        }
	//    }
	//    return false;
	//}

	//static bool typeHasEditorRef(Type type)
	//{
	//    if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
	//    {
	//        return true;
	//    }
	//    if (type.IsNested)
	//    {
	//        return typeHasEditorRef(type.DeclaringType);
	//    }
	//    if (type.IsByRef || type.IsArray)
	//    {
	//        return typeHasEditorRef(type.GetElementType());
	//    }
	//    if (type.IsGenericType)
	//    {
	//        foreach (var typeArg in type.GetGenericArguments())
	//        {
	//            if (typeHasEditorRef(typeArg))
	//            {
	//                return true;
	//            }
	//        }
	//    }
	//    return false;
	//}

	//static bool delegateHasEditorRef(Type delegateType)
	//{
	//    if (typeHasEditorRef(delegateType)) return true;
	//    var method = delegateType.GetMethod("Invoke");
	//    if (method == null)
	//    {
	//        return false;
	//    }
	//    if (typeHasEditorRef(method.ReturnType)) return true;
	//    return method.GetParameters().Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
	//}

	// 配置某Assembly下所有涉及到的delegate到CSharpCallLua下，Hotfix下拿不准那些delegate需要适配到lua function可以这么配置
	//[CSharpCallLua]
	//static IEnumerable<Type> AllDelegate
	//{
	//    get
	//    {
	//        BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
	//        List<Type> allTypes = new List<Type>();
	//        var allAssemblys = new Assembly[]
	//        {
	//            Assembly.Load("Assembly-CSharp")
	//        };
	//        foreach (var t in (from assembly in allAssemblys from type in assembly.GetTypes() select type))
	//        {
	//            var p = t;
	//            while (p != null)
	//            {
	//                allTypes.Add(p);
	//                p = p.BaseType;
	//            }
	//        }
	//        allTypes = allTypes.Distinct().ToList();
	//        var allMethods = from type in allTypes
	//                         from method in type.GetMethods(flag)
	//                         select method;
	//        var returnTypes = from method in allMethods
	//                          select method.ReturnType;
	//        var paramTypes = allMethods.SelectMany(m => m.GetParameters()).Select(pinfo => pinfo.ParameterType.IsByRef ? pinfo.ParameterType.GetElementType() : pinfo.ParameterType);
	//        var fieldTypes = from type in allTypes
	//                         from field in type.GetFields(flag)
	//                         select field.FieldType;
	//        return (returnTypes.Concat(paramTypes).Concat(fieldTypes)).Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t) && !delegateHasEditorRef(t)).Distinct();
	//    }
	//}
	//--------------end 热补丁自动化配置-------------------------

	//黑名单
	[BlackList]
	public static List<List<string>> BlackList = new List<List<string>>() {
		new List<string>() {"System.Xml.XmlNodeList", "ItemOf"},
		new List<string>() {"UnityEngine.WWW", "movie"},
#if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
#endif
		new List<string>() {"UnityEngine.Texture2D", "alphaIsTransparency"},
		new List<string>() {"UnityEngine.UI.Graphic", "OnRebuildRequested"},
		new List<string>() {"UnityEngine.UI.Text", "OnRebuildRequested"},
		new List<string>() {"UnityEngine.Input", "IsJoystickPreconfigured", "System.String"},
		new List<string>() {"UnityEngine.Texture", "imageContentsHash"},
		new List<string>() {"UnityEngine.Security", "GetChainOfTrustValue"},
		new List<string>() {"UnityEngine.CanvasRenderer", "OnRequestRebuild"},
		new List<string>() {"UnityEngine.Light", "areaSize"},
		new List<string>() {"UnityEngine.Light", "lightmapBakeType"},
		new List<string>() {"UnityEngine.WWW", "MovieTexture"},
		new List<string>() {"UnityEngine.WWW", "GetMovieTexture"},
		new List<string>() {"UnityEngine.MeshRenderer", "receiveGI"},
		new List<string>() {"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
#if !UNITY_WEBPLAYER
		new List<string>() {"UnityEngine.Application", "ExternalEval"},
#endif
		new List<string>() {"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
		new List<string>() {"UnityEngine.Component", "networkView"}, //4.6.2 not support
		new List<string>() {"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
		new List<string>() {"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
		new List<string>() {"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
		new List<string>() {"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
		new List<string>() {"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
		new List<string>() {"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
		new List<string>() {"UnityEngine.MonoBehaviour", "runInEditMode"},
	};
}