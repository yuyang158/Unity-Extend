using System;
using System.Collections.Generic;
using Extend.Common.Editor;
using Extend.Common.Editor.InspectorGUI;
using Extend.Switcher.Action;
using Extend.Switcher.Action.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Switcher.Editor {
	[CustomEditor(typeof(StateSwitcher))]
	public class StateSwitcherEditor : UnityEditor.Editor {
		private ReorderableList m_switcherActionList;
		private SerializedProperty m_statesProp;
		private ReorderableList m_statesList;

		private static TypeCache.TypeCollection m_switchActionTypes;

		static StateSwitcherEditor() {
			m_switchActionTypes = TypeCache.GetTypesDerivedFrom<SwitcherAction>();
		}

		protected bool m_canAddState = true;

		protected virtual void OnEnable() {
			m_statesProp = serializedObject.FindProperty("States");
			m_statesList = new ReorderableList(serializedObject, m_statesProp) {
				onAddCallback = list => {
					var input = InputWindow.CreateWindow("Create new state");
					input.Callback += stateName => {
						if( string.IsNullOrEmpty(stateName) ) {
							return;
						}

						var switcher = target as StateSwitcher;
						if( switcher.States != null && Array.Find(switcher.States, state => state.StateName == stateName) != null ) {
							EditorUtility.DisplayDialog("ERROR", $"Duplicate state name : {stateName}", "OK");
							return;
						}

						var index = m_statesProp.arraySize;
						m_statesProp.InsertArrayElementAtIndex(index);
						var arrElement = m_statesProp.GetArrayElementAtIndex(index);
						var stateNameProp = arrElement.FindPropertyRelative("StateName");
						stateNameProp.stringValue = stateName;

						var actionsProp = arrElement.FindPropertyRelative("SwitcherActions");
						actionsProp.ClearArray();

						m_statesProp.serializedObject.ApplyModifiedProperties();
					};
					input.ShowModal();
				},
				onSelectCallback = list => {
					if( list.index < 0 ) {
						m_switcherActionList = null;
					}
					else {
						var property = m_statesProp.GetArrayElementAtIndex(list.index);
						var switcherActionsProp = property.FindPropertyRelative("SwitcherActions");
						m_switcherActionList = CreateNewListUI(switcherActionsProp);
					}
				},
				drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "State List"); },
				onRemoveCallback = list => {
					m_statesProp.DeleteArrayElementAtIndex(list.index);
					m_switcherActionList = null;
				},
				displayAdd = m_canAddState,
				displayRemove = m_canAddState
			};
		}

		private static void OnMenuClicked(object ctx) {
			var pair = (KeyValuePair<Type, SerializedProperty>) ctx;
			var property = pair.Value;
			var index = property.arraySize;
			property.InsertArrayElementAtIndex(index);
			property.serializedObject.ApplyModifiedProperties();

			var states = property.GetPropertyObject() as ISwitcherAction[];
			states[index] = Activator.CreateInstance(pair.Key) as ISwitcherAction;
			property.serializedObject.UpdateIfRequiredOrScript();
		}

		private static ReorderableList CreateNewListUI(SerializedProperty switcherActionsProp) {
			return new ReorderableList(switcherActionsProp.serializedObject, switcherActionsProp) {
				onAddDropdownCallback = (rect, list) => {
					var menu = new GenericMenu();
					foreach( var type in m_switchActionTypes ) {
						var name = ObjectNames.NicifyVariableName(type.Name.Substring(0, type.Name.Length - 6));
						menu.AddItem(new GUIContent(name), false, OnMenuClicked,
							new KeyValuePair<Type, SerializedProperty>(type, switcherActionsProp));
					}

					menu.ShowAsContext();
				},
				elementHeightCallback = index => {
					var elementPropAtIndex = switcherActionsProp.GetArrayElementAtIndex(index);
					var switcher = elementPropAtIndex.GetPropertyObject() as ISwitcherAction;
					var drawer = ActionDrawer.GetDrawer(switcher.GetType());
					var foldProp = elementPropAtIndex.FindPropertyRelative("m_fold");
					return foldProp.boolValue ? drawer.GetEditorHeight(elementPropAtIndex) : UIEditorUtil.LINE_HEIGHT;
				},
				drawElementCallback = (rect, index, active, focused) => {
					var elementPropAtIndex = switcherActionsProp.GetArrayElementAtIndex(index);
					var switcher = elementPropAtIndex.GetPropertyObject() as ISwitcherAction;
					var drawer = ActionDrawer.GetDrawer(switcher.GetType());
					var foldProp = elementPropAtIndex.FindPropertyRelative("m_fold");
					rect.height = EditorGUIUtility.singleLineHeight;
					var foldRect = rect;
					foldRect.x += 10;
					var name = ObjectNames.NicifyVariableName(switcher.GetType().Name);
					foldProp.boolValue = EditorGUI.Foldout(foldRect, foldProp.boolValue, foldProp.boolValue ? string.Empty : name);
					if( !foldProp.boolValue ) {
						return;
					}

					EditorGUI.indentLevel += 2;
					drawer.OnEditorGUI(rect, elementPropAtIndex);
					EditorGUI.indentLevel -= 2;
				},
				drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Action List"); }
			};
		}

		public override void OnInspectorGUI() {
			m_statesList.DoLayoutList();
			m_switcherActionList?.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}