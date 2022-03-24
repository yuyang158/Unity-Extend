using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Extend.Common;
using Extend.Common.Editor.InspectorGUI;
using Extend.LuaBindingData;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Extend.Editor {
	[CustomEditor(typeof(LuaBinding), true)]
	public class LuaBindingEditor : ExtendInspector {
		private LuaBinding binding;
		private static readonly string[] basicTypes = {"string", "number", "boolean", "integer"};

		private static readonly Type[] basicBindingTypes = {
			typeof(LuaBindingStringData),
			typeof(LuaBindingNumberData),
			typeof(LuaBindingBooleanData),
			typeof(LuaBindingIntegerData)
		};

		private LuaClassDescriptor descriptor;
		private readonly List<LuaBindingDataBase> isUsedBinding = new();

		private void OnEnable() {
			binding = target as LuaBinding;
			var luaPathProp = serializedObject.FindProperty("LuaFile");
			if( string.IsNullOrEmpty(luaPathProp.stringValue) || !File.Exists(Application.dataPath + "/Resources/Lua/" + luaPathProp.stringValue + ".lua") ) {
				return;
			}

			descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(luaPathProp.stringValue);
		}

		private LuaBindingDataBase CheckBinding(LuaClassField field, Type dataBindType) {
			LuaBindingDataBase matched = null;

			foreach( var bind in binding.LuaData ) {
				if( bind.FieldName != field.FieldName ) continue;
				if( bind.GetType() != dataBindType ) continue;
				matched = bind;
				break;
			}

			if( matched == null ) {
				matched = Activator.CreateInstance(dataBindType) as LuaBindingDataBase;
				matched.FieldName = field.FieldName;
				ArrayUtility.Add(ref binding.LuaData, matched);
				serializedObject.Update();
				EditorUtility.SetDirty(target);
			}

			isUsedBinding.Add(matched);
			matched.FieldType = field.FieldType;
			return matched;
		}

		private T CheckBinding<T>(LuaClassField field) where T : LuaBindingDataBase {
			return CheckBinding(field, typeof(T)) as T;
		}

		public override void OnInspectorGUI() {
			binding.LuaData ??= Array.Empty<LuaBindingDataBase>();

			var luaPathProp = serializedObject.FindProperty("LuaFile");
			if( string.IsNullOrEmpty(luaPathProp.stringValue) ) {
				EditorGUILayout.PropertyField(luaPathProp);
				EditorGUILayout.HelpBox("需要设置Lua文件！", MessageType.Error);
				serializedObject.ApplyModifiedProperties();
				return;
			}

			if( descriptor == null ) {
				EditorGUILayout.PropertyField(luaPathProp);
				EditorGUILayout.HelpBox("需要设置Lua文件！", MessageType.Error);
				descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(luaPathProp.stringValue);
				serializedObject.ApplyModifiedProperties();
				return;
			}
			
			isUsedBinding.Clear();
			foreach( var field in descriptor.Fields.Where(field => !field.FieldName.StartsWith("_")) ) {
				if( field.FieldType.Contains("[]") ) {
					CheckBinding<LuaBindingUOArrayData>(field);
				}
				else if( field.FieldType.StartsWith("CS.") ) {
					var typeName = field.FieldType[3..];
					var type = String2TypeCache.GetType(typeName);
					if( type == null ) {
						EditorGUILayout.HelpBox($"Can not find type : {typeName}", MessageType.Error);
					}
					else {
						if( field.FieldType == "CS.Extend.Asset.AssetReference" ) {
							var match = CheckBinding<LuaBindingAssetReferenceData>(field);
							if( !string.IsNullOrEmpty(field.Comment) && field.Comment.StartsWith("CS.") ) {
								match.AssetType = String2TypeCache.GetType(field.Comment[3..]);
							}
						}
						else {
							CheckBinding<LuaBindingUOData>(field);
						}
					}
				}
				else {
					var index = Array.IndexOf(basicTypes, field.FieldType);
					if( index >= 0 ) {
						var typ = basicBindingTypes[index];
						CheckBinding(field, typ);
					}
					else {
						CheckBinding<LuaBindingUOData>(field);
					}
				}
			}

			for( var i = 0; i < binding.LuaData.Length; ) {
				var bindingData = binding.LuaData[i];
				if( isUsedBinding.Contains(bindingData) ) {
					i++;
				}
				else {
					ArrayUtility.RemoveAt(ref binding.LuaData, i);
				}
			}

			serializedObject.UpdateIfRequiredOrScript();
			var luaDataProp = serializedObject.FindProperty("LuaData");
			for( var i = 0; i < binding.LuaData.Length; i++ ) {
				var arrElem = binding.LuaData[i];
				var elementProp = luaDataProp.GetArrayElementAtIndex(i);
				var dataProp = elementProp.FindPropertyRelative("Data");
				var fieldNameProp = elementProp.FindPropertyRelative("FieldName");
				var fieldName = fieldNameProp.stringValue;
				var field = descriptor.FindField(fieldName);
				arrElem.OnPropertyDrawer(dataProp, field.Comment);
			}

			if( Application.isPlaying ) {
				foreach( var methodName in descriptor.DebugMethods ) {
					if( GUILayout.Button(methodName) && binding.LuaInstance != null ) {
						var func = binding.LuaInstance.Get<Action<LuaTable>>(methodName);
						func(binding.LuaInstance);
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
			base.OnInspectorGUI();
			GUILayout.BeginHorizontal();
			if( GUILayout.Button("重新加载Lua文件") ) {
				if( descriptor == null ) return;
				descriptor = LuaClassEditorFactory.ReloadDescriptor(descriptor.ClassName.Replace('.', '/'));
			}

			if( GUILayout.Button("在编辑器中打开") ) {
				string idePath = EditorPrefs.GetString("kScriptsDefaultApp_h2657262712");
				var luaPath = $"{Application.dataPath}/../Lua/{luaPathProp.stringValue.Replace('.', '/')}.lua";
				if( idePath.Contains("Rider") ) {
					Process.Start($"\"{idePath}\"", $"--line 0 {luaPath}");
				}
				else if( idePath.Contains("Code") ) {
					string binPath = idePath.Replace("Code.exe", "bin/code");
					Process.Start($"\"{binPath}\"", $"-r -g \"{luaPath}:0\"");
				}
			}

			GUILayout.EndHorizontal();
		}
	}
}