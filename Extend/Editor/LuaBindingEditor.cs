using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extend.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend {
	[CustomEditor(typeof(LuaBinding))]
	public class LuaBindingEditor : UnityEditor.Editor {
		private LuaBinding binding;
		private static readonly Dictionary<string, Action<string, string>> specialTypeActions = new Dictionary<string, Action<string, string>>();
		private LuaClassDescriptor descriptor;

		public LuaBindingEditor() {
			specialTypeActions.Clear();
			specialTypeActions.Add("number", (label, fieldName) => {
				var val = binding.BindingContainer.ContainsKey(fieldName) ? binding.BindingContainer[fieldName] : default(double);
				binding.BindingContainer[fieldName] = EditorGUILayout.DoubleField(label, (double)val);
			});
			
			specialTypeActions.Add("string", (label, fieldName) => {
				var val = binding.BindingContainer.ContainsKey(fieldName) ? binding.BindingContainer[fieldName] : string.Empty;
				binding.BindingContainer[fieldName] = EditorGUILayout.TextField(label, (string)val);
			});
			
			specialTypeActions.Add("boolean", (label, fieldName) => {
				var val = binding.BindingContainer.ContainsKey(fieldName) ? binding.BindingContainer[fieldName] : default(bool);
				binding.BindingContainer[fieldName] = EditorGUILayout.Toggle(label, (bool)val);
			});
		}

		private void OnEnable() {
			binding = target as LuaBinding;
			
			if( string.IsNullOrEmpty(binding.LuaFile) || !File.Exists(Application.dataPath + "/Resources/" + binding.LuaFile + ".lua") ) {
				return;
			}

			descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(binding.LuaFile);
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if( string.IsNullOrEmpty(binding.LuaFile) || !File.Exists(Application.dataPath + "/Resources/" + binding.LuaFile + ".lua") ) {
				EditorGUILayout.HelpBox("需要设置Lua文件！", MessageType.Error);
			}
			else {
				if( descriptor == null ) {
					descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(binding.LuaFile);
					return;
				}

				if( binding.BindingContainer == null ) {
					binding.BindingContainer = new LuaBinding.BindingDictionaryContainer();
				}

				foreach( var field in descriptor.Fields ) {
					if(field.FieldName.StartsWith("_"))
						continue;

					binding.BindingContainer.TryGetValue(field.FieldName, out var val);
					var label = string.IsNullOrEmpty(field.Comment) ? field.FieldName : field.Comment;
					if( field.FieldType.Contains("[]") ) {
						
					}
					else if( field.FieldType.StartsWith("CS.") ) {
						var typeName = field.FieldType.Substring(3);
						var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).First(x => x.FullName == typeName);
						if( type == null ) {
							EditorGUILayout.HelpBox($"Can not find type : {typeName}", MessageType.Error);
						}
						else {
							val = EditorGUILayout.ObjectField(label, (UnityEngine.Object)val, type, true);
							binding.BindingContainer[field.FieldName] = val;
						}
					}
					else {
						if( specialTypeActions.TryGetValue(field.FieldType, out var processAction) ) {
							processAction(label, field.FieldName);
						}
						else {
							val = EditorGUILayout.ObjectField(label, (UnityEngine.Object)val, typeof(LuaBinding), true);
							binding.BindingContainer[field.FieldName] = val;	
						}
					}
				}
			}

			if( GUILayout.Button("重新加载Lua文件") ) {
				LuaClassEditorFactory.ReloadDescriptor(descriptor.ClassName);
			}
		}
	}
}