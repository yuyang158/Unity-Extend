using System;
using Extend.Common.Editor.InspectorGUI;
using Extend.StateActionGroup.Behaviour;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.StateActionGroup.Editor {
	[CustomEditor(typeof(SAG))]
	public class SAGEditor : UnityEditor.Editor {
		private ReorderableList m_behaviourList;
		private ReorderableList m_stateDataList;

		private int GetMaxId(BehaviourBase[] existBehaviours) {
			var maxId = 0;
			foreach( var existBehaviour in existBehaviours ) {
				if( maxId < existBehaviour.Id ) {
					maxId = existBehaviour.Id;
				}
			}
			return maxId;
		}
		
		private void OnEnable() {
			var subBehaviourTypes = TypeCache.GetTypesDerivedFrom<BehaviourBase>();
			
			var behavioursProp = serializedObject.FindProperty("Behaviours");
			m_behaviourList = new ReorderableList(serializedObject, behavioursProp) {
				onAddDropdownCallback = (rect, list) => {
					var menu = new GenericMenu();
					foreach( var type in subBehaviourTypes ) {
						var typeDisplayName = ObjectNames.NicifyVariableName(type.Name);
						menu.AddItem(new GUIContent(typeDisplayName), false, OnAddBehaviourMenuClicked, type);
					}

					menu.ShowAsContext();
				},
				drawElementCallback = (rect, index, active, focused) => {
					rect.height = EditorGUIUtility.singleLineHeight;
					var behaviourProp = behavioursProp.GetArrayElementAtIndex(index);
					var depth = behaviourProp.depth;
					foreach( SerializedProperty behaviourFieldProp in behaviourProp ) {
						if( behaviourFieldProp.depth == depth + 1 && behaviourFieldProp.name != "Id" ) {
							EditorGUI.PropertyField(rect, behaviourFieldProp);
						}
					}
				},
				drawHeaderCallback = rect => {
					EditorGUI.LabelField(rect, "Behaviours");
				}
			};

			var sag = target as SAG;
			var dataGroupsProp = serializedObject.FindProperty("DataGroups");
			m_stateDataList = new ReorderableList(serializedObject, dataGroupsProp) {
				drawHeaderCallback = rect => {
					EditorGUI.LabelField(rect, "States");
				},
				drawElementCallback = (rect, index, active, focused) => {
					rect.height = EditorGUIUtility.singleLineHeight;
					var stateProp = dataGroupsProp.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(rect, stateProp.FindPropertyRelative("StateName"));
				},
				onSelectCallback = list => {
					if( list.index == -1 ) {
						return;
					}

					var selectedStateDataGroupProp = dataGroupsProp.GetArrayElementAtIndex(list.index);
					var sdg = selectedStateDataGroupProp.GetPropertyObject() as StateDataGroup;
					if( sdg.DataArray == null ) {
						return;
					}
					foreach( BehaviourDataBase data in sdg.DataArray ) {
						var behaviour = sag.FindBehaviourById(data.TargetId);
						if( behaviour == null ) {
							continue;
						}
						data.ApplyToBehaviour(behaviour);
					}
				}
			};
		}

		private void OnAddBehaviourMenuClicked(object state) {
			var sag = target as SAG;
			if( !(state is Type type) ) {
				return;
			}

			sag.Behaviours ??= Array.Empty<BehaviourBase>();
			var maxId = GetMaxId(sag.Behaviours);
			var behaviour = Activator.CreateInstance(type) as BehaviourBase;
			behaviour.Id = maxId + 1;
			ArrayUtility.Add(ref sag.Behaviours, behaviour);
			serializedObject.UpdateIfRequiredOrScript();
		}

		public override void OnInspectorGUI() {
			EditorGUI.BeginChangeCheck();
			m_behaviourList.DoLayoutList();
			m_stateDataList.DoLayoutList();

			var blackListsProp = serializedObject.FindProperty("m_blackLists");
			EditorGUILayout.PropertyField(blackListsProp);
			if( EditorGUI.EndChangeCheck() ) {
				serializedObject.ApplyModifiedProperties();
			}
			var sag = target as SAG;
			if( m_stateDataList.index != -1 && m_stateDataList.serializedProperty.arraySize > m_stateDataList.index) {
				var stateProp = m_stateDataList.serializedProperty.GetArrayElementAtIndex(m_stateDataList.index);
				var stateDataGroup = stateProp.GetPropertyObject() as StateDataGroup;
				stateDataGroup.DataArray ??= Array.Empty<BehaviourDataBase>();

				if( GUILayout.Button("Copy Prop") ) {
					stateDataGroup.CopyBehaviourData(sag);
					serializedObject.UpdateIfRequiredOrScript();
					EditorUtility.SetDirty(target);
				}
			}

			if( Application.isPlaying ) {
				GUI.enabled = false;
				EditorGUILayout.TextField("Active State", sag.ActiveStateName);
				GUI.enabled = true;
			}
		}
	}
}
