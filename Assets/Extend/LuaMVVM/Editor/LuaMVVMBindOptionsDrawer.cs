using System;
using System.Collections.Generic;
using Extend.Common.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.LuaMVVM.Editor {
	[CustomPropertyDrawer(typeof(LuaMVVMBindOptionsAttribute))]
	public class LuaMVVMBindOptionsDrawer : PropertyDrawer {
		private ReorderableList mvvmBindList;
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if( mvvmBindList == null )
				return 0;
			return mvvmBindList.headerHeight + mvvmBindList.elementHeight * (mvvmBindList.count == 0 ? 1 : mvvmBindList.count) + mvvmBindList.footerHeight + 5;
		}
		
		private static Rect[] GetRowRects(Rect rect) {
			var rects = new Rect[5];

			rect.height = EditorGUIUtility.singleLineHeight;
			rect.y += 2;

			var enabledRect = rect;
			enabledRect.width *= 0.3f;

			var goRect = enabledRect;
			goRect.y += UIEditorUtil.LINE_HEIGHT;

			var functionRect = rect;
			functionRect.xMin = goRect.xMax + 5;

			var argRect = functionRect;
			argRect.y += UIEditorUtil.LINE_HEIGHT;
			argRect.xMax -= 20;

			var globalRect = argRect;
			globalRect.xMin = argRect.xMax + 5;
			globalRect.xMax = functionRect.xMax;

			rects[0] = enabledRect;
			rects[1] = goRect;
			rects[2] = functionRect;
			rects[3] = argRect;
			rects[4] = globalRect;
			return rects;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			property = property.FindPropertyRelative("Options");
			if( mvvmBindList != null ) {
				mvvmBindList.serializedProperty = property;
				mvvmBindList.DoList(position);
				return;
			}

			mvvmBindList = new ReorderableList(property.serializedObject, property);
			mvvmBindList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "MVVM Binding"); };
			mvvmBindList.elementHeight = UIEditorUtil.LINE_HEIGHT * 2;
			mvvmBindList.drawElementCallback += (rect, index, active, focused) => {
				rect.y++;
				var subRects = GetRowRects(rect);
				var enabledRect = subRects[0];
				var functionRect = subRects[1];
				var goRect = subRects[2];
				var argRect = subRects[3];
				var globalRect = subRects[4];

				var prop = property.GetArrayElementAtIndex(index);
				var bindTargetProp = prop.FindPropertyRelative("BindTarget");
				EditorGUI.PropertyField(enabledRect, bindTargetProp, GUIContent.none);
				if( bindTargetProp.objectReferenceValue != null ) {
					var typ = bindTargetProp.objectReferenceValue.GetType();
					var propertyInfos = typ.GetProperties();
					var names = new List<string>();
					foreach( var propertyInfo in propertyInfos ) {
						if( propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.PropertyType.IsPublic && 
						    propertyInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0 ) {
							names.Add(propertyInfo.Name);
						}
					}
					var bindTargetPropProp = prop.FindPropertyRelative("BindTargetProp");
					var nameArr = names.ToArray();
					var select = EditorGUI.Popup(goRect, Array.IndexOf(nameArr, bindTargetPropProp.stringValue), nameArr);
					if( select >= 0 && select < names.Count ) {
						bindTargetPropProp.stringValue = names[select];
					}
					else {
						bindTargetPropProp.stringValue = "";
					}
				}
				
				var bindModeProp = prop.FindPropertyRelative("Mode");
				EditorGUI.PropertyField(functionRect, bindModeProp, GUIContent.none);
				var bindPathProp = prop.FindPropertyRelative("Path");
				EditorGUI.PropertyField(argRect, bindPathProp, GUIContent.none);
				
				var globalProp = prop.FindPropertyRelative("Global");
				EditorGUI.PropertyField(globalRect, globalProp, GUIContent.none);
			};
		}
	}
}