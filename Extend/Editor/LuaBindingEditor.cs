using System;
using System.Collections.Generic;
using System.IO;
using Extend.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend {
	[CustomEditor(typeof(LuaBinding))]
	public class LuaBindingEditor : UnityEditor.Editor {
		private LuaBinding binding;
		private static readonly Dictionary<string, Action<string>> specialTypeActions = new Dictionary<string, Action<string>>();
		private LuaClassDescriptor descriptor;

		public LuaBindingEditor() {
			specialTypeActions.Add("number", (name) => {
				var val = binding.BindingContainer[name];
				binding.BindingContainer[name] = EditorGUILayout.DoubleField(name, (double)val);
			});
			
			specialTypeActions.Add("string", (name) => {
				var val = binding.BindingContainer[name];
				binding.BindingContainer[name] = EditorGUILayout.TextField(name, (string)val);
			});
		}

		private void OnEnable() {
			binding = target as LuaBinding;

			if( string.IsNullOrEmpty(binding.LuaFile) || !File.Exists(binding.LuaFile) ) {
				return;
			}

			descriptor = LuaClassEditorFactory.GetDescriptorWithFilePath(binding.LuaFile);
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if( string.IsNullOrEmpty(binding.LuaFile) || !File.Exists(binding.LuaFile) ) {
				EditorGUILayout.HelpBox("需要设置Lua文件！", MessageType.Error);
			}
			else {
				if( descriptor == null ) {
					return;
				}

				foreach( var field in descriptor.Fields ) {
					if(field.FieldName.StartsWith("_"))
						continue;
					
					if( field.FieldType.Contains("[]") ) {

					}
					else if( field.FieldType.StartsWith("CS.") ) {
						
					}
					else {
						
					}
				}
			}

			if( GUILayout.Button("重新加载Lua文件") ) {
				LuaClassEditorFactory.ReloadDescriptor(descriptor.ClassName);
			}
		}
	}
}