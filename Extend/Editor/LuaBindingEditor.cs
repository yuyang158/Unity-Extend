using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Extend.LuaBindingData;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Extend.Editor {
	[CustomEditor(typeof(LuaBinding))]
	public class LuaBindingEditor : UnityEditor.Editor {
		private LuaBinding binding;
		private static readonly string[] basicTypes = {"string", "number", "boolean", "integer"};

		private static readonly Type[] basicBindingTypes = {
			typeof(LuaBindingStringData),
			typeof(LuaBindingNumberData),
			typeof(LuaBindingBooleanData),
			typeof(LuaBindingIntegerData)
		};

		private LuaClassDescriptor descriptor;
		private readonly List<LuaBindingDataBase> isUsedBinding = new List<LuaBindingDataBase>();

		private void OnEnable() {
			binding = target as LuaBinding;
			var luaPathProp = serializedObject.FindProperty("LuaFile");
			if( string.IsNullOrEmpty(luaPathProp.stringValue) || !File.Exists(Application.dataPath + "/Resources/Lua/" + luaPathProp.stringValue + ".lua") ) {
				return;
			}

			descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(luaPathProp.stringValue);
		}

		private void CheckBinding(LuaClassField field, Type dataBindType) {
			LuaBindingDataBase matched = null;
			foreach( var bind in binding.BindingContainer ) {
				if( bind.FieldName != field.FieldName ) continue;
				if( bind.GetType() != dataBindType ) continue;
				matched = bind;
				break;
			}

			if( matched == null ) {
				matched = Activator.CreateInstance(dataBindType) as LuaBindingDataBase;
				matched.FieldName = field.FieldName;
				binding.BindingContainer.Add(matched);
			}

			isUsedBinding.Add(matched);
			matched.FieldType = field.FieldType;
		}

		private void CheckBinding<T>(LuaClassField field) where T : LuaBindingDataBase {
			CheckBinding(field, typeof(T));
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			var luaPathProp = serializedObject.FindProperty("LuaFile");
			if( string.IsNullOrEmpty(luaPathProp.stringValue) ) {
				EditorGUILayout.HelpBox("需要设置Lua文件！", MessageType.Error);
				return;
			}

			if( descriptor == null ) {
				EditorGUILayout.HelpBox("需要设置Lua文件！", MessageType.Error);
				descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(luaPathProp.stringValue);
				return;
			}

			if( binding.BindingContainer == null ) {
				binding.BindingContainer = new List<LuaBindingDataBase>();
			}

			isUsedBinding.Clear();
			foreach( var field in descriptor.Fields ) {
				if( field.FieldName.StartsWith("_") )
					continue;

				if( field.FieldType.Contains("[]") ) {
					CheckBinding<LuaBindingUOArrayData>(field);
				}
				else if( field.FieldType.StartsWith("CS.") ) {
					var typeName = field.FieldType.Substring(3);
					var type = String2TypeCache.GetType(typeName);
					if( type == null ) {
						EditorGUILayout.HelpBox($"Can not find type : {typeName}", MessageType.Error);
					}
					else {
						CheckBinding<LuaBindingUOData>(field);
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

			if( binding.BindingContainer.Count != isUsedBinding.Count ) {
				binding.BindingContainer.Clear();
				binding.BindingContainer.AddRange(isUsedBinding);
			}

			serializedObject.UpdateIfRequiredOrScript();
			var fields = target.GetType().GetFields();
			foreach( var fieldInfo in fields ) {
				if( !fieldInfo.IsPublic || !fieldInfo.FieldType.IsArray || !fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(LuaBindingDataBase)))
					continue;
				var prop = serializedObject.FindProperty(fieldInfo.Name);
				if( prop == null || prop.isArray == false )
					continue;

				var arr = fieldInfo.GetValue(target) as Array;
				for( var i = 0; i < prop.arraySize; i++ ) {
					var arrElem = arr.GetValue(i) as LuaBindingDataBase;
					var elementProp = prop.GetArrayElementAtIndex(i);
					var dataProp = elementProp.FindPropertyRelative("Data");
					arrElem.OnPropertyDrawer(dataProp);
				}
			}

			var bindOptionsProp = serializedObject.FindProperty("BindingOptions");
			EditorGUILayout.PropertyField(bindOptionsProp);

			serializedObject.ApplyModifiedProperties();

			if( GUILayout.Button("重新加载Lua文件") ) {
				if( descriptor == null ) return;
				descriptor = LuaClassEditorFactory.ReloadDescriptor(descriptor.ClassName);
			}
		}
	}
}