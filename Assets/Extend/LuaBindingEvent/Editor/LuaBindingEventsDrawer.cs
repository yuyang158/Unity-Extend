using System;
using Extend.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.LuaBindingEvent.Editor {
	[CustomPropertyDrawer(typeof(LuaEventsAttribute))]
	public class LuaBindingEventsDrawer : PropertyDrawer {
		private ReorderableList reList;
		private const float ROW_CONTROL_MARGIN = 5;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if( reList == null )
				return 0;
			return reList.headerHeight + reList.elementHeight * (reList.count == 0 ? 1 : reList.count) + reList.footerHeight + 5;
		}

		public override void OnGUI(Rect rect, SerializedProperty arrProp, GUIContent label) {
			arrProp = arrProp.FindPropertyRelative("Events");
			if( reList != null ) {
				reList.serializedProperty = arrProp;
				reList.DoList(rect);
				return;
			}

			var eventsAttr = (LuaEventsAttribute)attribute;
			reList = new ReorderableList(arrProp.serializedObject, arrProp);
			reList.drawHeaderCallback += position => { EditorGUI.LabelField(position, eventsAttr.EvtName); };
			reList.elementHeight = ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ) * 2 + EditorGUIUtility.standardVerticalSpacing;
			reList.drawElementCallback += (position, index, active, focused) => {
				var property = reList.serializedProperty.GetArrayElementAtIndex(index);
				position.height = EditorGUIUtility.singleLineHeight;
				position.y += EditorGUIUtility.standardVerticalSpacing;
				var totalWidth = position.width;
				position.width = totalWidth * .3f;
				var bindingProp = property.FindPropertyRelative("Binding");
				EditorGUI.ObjectField(position, bindingProp, GUIContent.none);
				if( bindingProp.objectReferenceValue == null ) {
					return;
				}

				var binding = bindingProp.objectReferenceValue as LuaBinding;
				var descriptor = LuaClassEditorFactory.GetDescriptor(binding.LuaFile);
				if( descriptor == null )
					return;

				var oriX = position.x;
				position.x += position.width + ROW_CONTROL_MARGIN;
				position.width = totalWidth * .7f - ROW_CONTROL_MARGIN;
				var methodNameProp = property.FindPropertyRelative("LuaMethodName");
				var selected = descriptor.Methods.IndexOf(methodNameProp.stringValue);
				var newIndex = EditorGUI.Popup(position, selected, descriptor.Methods.ToArray());
				if( newIndex != selected ) {
					methodNameProp.stringValue = descriptor.Methods[newIndex];
				}

				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				position.y += EditorGUIUtility.standardVerticalSpacing;
				position.x = oriX;
				position.width = totalWidth * .3f;
				var paramProp = property.FindPropertyRelative("Param");
				var paramTypeProp = paramProp.FindPropertyRelative("Type");
				paramTypeProp.intValue = EditorGUI.Popup(position, paramTypeProp.intValue, paramTypeProp.enumDisplayNames);
				var paramType = (LuaBindingEventData.EventParam.ParamType)paramTypeProp.intValue;
				position.x += position.width + ROW_CONTROL_MARGIN;
				position.width = totalWidth * .7f - ROW_CONTROL_MARGIN;
				switch( paramType ) {
					case LuaBindingEventData.EventParam.ParamType.Int:
						var paramIntProp = paramProp.FindPropertyRelative("Int");
						paramIntProp.intValue = EditorGUI.IntField(position, paramIntProp.intValue);
						break;
					case LuaBindingEventData.EventParam.ParamType.Float:
						var paramFloatProp = paramProp.FindPropertyRelative("Float");
						paramFloatProp.floatValue = EditorGUI.FloatField(position, paramFloatProp.floatValue);
						break;
					case LuaBindingEventData.EventParam.ParamType.String:
						var paramStrProp = paramProp.FindPropertyRelative("Str");
						paramStrProp.stringValue = EditorGUI.TextField(position, paramStrProp.stringValue);
						break;
					case LuaBindingEventData.EventParam.ParamType.None:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
			reList.DoList(rect);
		}
	}
}