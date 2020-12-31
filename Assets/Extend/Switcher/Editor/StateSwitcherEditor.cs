using System;
using System.Collections.Generic;
using Extend.Common.Editor.InspectorGUI;
using Extend.Editor;
using Extend.Switcher.Action;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Switcher.Editor {
	[CustomEditor(typeof(StateSwitcher))]
	public class StateSwitcherEditor : UnityEditor.Editor {
		private ReorderableList m_switcherActionList;
		private SerializedProperty m_statesProp;

		private static TypeCache.TypeCollection m_switchActionTypes;

		static StateSwitcherEditor() {
			m_switchActionTypes = TypeCache.GetTypesDerivedFrom<ISwitcherAction>();
		}

		private void OnEnable() {
			m_statesProp = serializedObject.FindProperty("States");
		}

		private readonly List<string> m_states = new List<string>();
		private string m_selected;
		private static readonly GUIContent CURRENT_SELECTED_LABEL = new GUIContent("Current");

		private static void OnMenuClicked(object ctx) {
			var pair = (KeyValuePair<Type, SerializedProperty>)ctx;
			var property = pair.Value;
			var index = property.arraySize;
			property.InsertArrayElementAtIndex(index);
			property.serializedObject.ApplyModifiedProperties();

			var states = property.GetPropertyObject() as ISwitcherAction[];
			states[index] = Activator.CreateInstance(pair.Key) as ISwitcherAction;
			property.serializedObject.UpdateIfRequiredOrScript();
		}

		private ReorderableList CreateNewListUI(SerializedProperty switcherActionsProp) {
			return new ReorderableList(switcherActionsProp.serializedObject, switcherActionsProp) {
				onAddDropdownCallback = (rect, list) => {
					var menu = new GenericMenu();
					foreach( var type in m_switchActionTypes ) {
						menu.AddItem(new GUIContent(type.Name), false, OnMenuClicked, 
							new KeyValuePair<Type, SerializedProperty>(type, switcherActionsProp));
					}

					menu.ShowAsContext();
				},
				elementHeightCallback = index => {
					var elementAtIndex = switcherActionsProp.GetArrayElementAtIndex(index);
					var switcher = elementAtIndex.GetPropertyObject() as ISwitcherAction;
					return switcher.GetEditorHeight(elementAtIndex);
				},
				drawElementCallback = (rect, index, active, focused) => {
					var elementAtIndex = switcherActionsProp.GetArrayElementAtIndex(index);
					var switcher = elementAtIndex.GetPropertyObject() as ISwitcherAction;
					switcher.OnEditorGUI(rect, elementAtIndex);
				} 
			};
		}

		public override void OnInspectorGUI() {
			var switcher = target as StateSwitcher;

			m_states.Clear();
			if( switcher.States != null ) {
				foreach( var state in switcher.States ) {
					m_states.Add(state.StateName);
				}
			}

			if( GUILayout.Button("Add") ) {
				var input = InputWindow.CreateWindow("Create new state");
				input.Callback += s => {
					if( string.IsNullOrEmpty(s) ) {
						return;
					}

					if( m_states.IndexOf(s) != -1 ) {
						EditorUtility.DisplayDialog("ERROR", $"Duplicate state name : {s}", "OK");
						return;
					}

					var index = m_statesProp.arraySize;
					m_statesProp.InsertArrayElementAtIndex(index);
					var arrElement = m_statesProp.GetArrayElementAtIndex(index);
					var stateNameProp = arrElement.FindPropertyRelative("StateName");
					stateNameProp.stringValue = s;
					m_statesProp.serializedObject.ApplyModifiedProperties();
				};
				input.ShowModal();
			}

			var selected = EditorGUILayout.Popup(CURRENT_SELECTED_LABEL, m_states.IndexOf(m_selected), m_states.ToArray());
			if( selected >= 0 ) {
				m_selected = m_states[selected];
			}

			if( m_switcherActionList == null && selected >= 0 ) {
				var property = m_statesProp.GetArrayElementAtIndex(selected);
				var switcherActionsProp = property.FindPropertyRelative("SwitcherActions");
				m_switcherActionList = CreateNewListUI(switcherActionsProp);
			}
			m_switcherActionList?.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}