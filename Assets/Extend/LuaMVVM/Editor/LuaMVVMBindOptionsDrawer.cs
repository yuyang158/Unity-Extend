using System;
using System.Collections.Generic;
using Extend.Asset;
using Extend.Common.Editor;
using Extend.Common.Editor.InspectorGUI;
using Extend.LuaBindingEvent;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaMVVM.Editor {
	[CustomPropertyDrawer(typeof(LuaMVVMBindOptionsAttribute))]
	public class LuaMVVMBindOptionsDrawer : PropertyDrawer {
		private static readonly Dictionary<Type, string> m_componentTypePropertyShortcut = new() {
			{typeof(TextMeshProUGUI), "text"},
			{typeof(Text), "text"},
			{typeof(Image), "sprite"},
			{typeof(SpriteRenderer), "sprite"},
			{typeof(Slider), "value"},
			{typeof(LuaMVVMImageSpriteNSelectOne), "SelectIndex"},
			{typeof(TMP_Dropdown), "value"},
			{typeof(ImageSpriteAssetAssignment), "SpritePath"},
			{typeof(ImageRemoteSpriteAssetAssignment), "SpriteRemotePath"}
		};

		private static readonly Dictionary<Type, string[]> m_bindingEventNames = new() {
			{typeof(LuaBindingClickEvent), new[] {"OnClick"}},
			{typeof(LuaBindingSelectEvent), new[] {"OnSelect", "OnDeselect"}},
			{typeof(LuaBindingFingerClickEvent), new[] {"OnClick"}},
			{typeof(LuaBindingClickLongTapEvent), new[] {"OnClick", "OnLongTap"}},
			{typeof(LuaBindingDragEvent), new[] {"OnBeginDrag", "OnDrag", "OnEndDrag"}},
			{typeof(LuaBindingToggleEvent), new[] {"OnCheck"}},
			{typeof(LuaBindingUpDownMoveEvent), new[] {"OnDown", "OnUp", "OnDrag"}},
			{typeof(LuaBindingEnterLeaveEvent), new[] {"OnEnter", "OnExit"}},
			{typeof(LuaBindingTextLinkEvent), new[] {"OnLink", "OnLinkClick"}},
			{typeof(LuaBindingPressAndHoldEvent), new[] {"OnHold"}},
		};

		private ReorderableList m_mvvmBindList;
		private LuaMVVMBinding m_extraMVVMBinding;
		private float m_optionsListHeight;
		private float m_extraHeight;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if( m_mvvmBindList == null )
				return 0;
			var lineHeight = m_mvvmBindList.headerHeight +
			                 ( m_mvvmBindList.elementHeight + EditorGUIUtility.standardVerticalSpacing ) *
			                 ( m_mvvmBindList.count == 0 ? 1 : m_mvvmBindList.count ) +
			                 m_mvvmBindList.footerHeight + 5;

			m_optionsListHeight = lineHeight;
			if( m_extraMVVMBinding ) {
				m_extraHeight = m_extraMVVMBinding.BindingOptions.Options.Length * UIEditorUtil.LINE_HEIGHT;
				lineHeight += m_extraHeight + 10;
			}

			return lineHeight;
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
			if( m_mvvmBindList != null ) {
				m_mvvmBindList.serializedProperty = property;
				m_mvvmBindList.DoList(position);

				if( !m_extraMVVMBinding )
					return;
				position.yMin += m_optionsListHeight + 5;
				GUI.Box(position, "");
				position.yMin += 5;
				position.height = UIEditorUtil.LINE_HEIGHT;
				foreach( var option in m_extraMVVMBinding.BindingOptions.Options ) {
					EditorGUI.TextField(position, $"{option.BindTarget.name}.{option.BindTargetProp}", option.Path);
					position.y += UIEditorUtil.LINE_HEIGHT;
				}

				return;
			}

			m_mvvmBindList = new ReorderableList(property.serializedObject, property);
			m_mvvmBindList.onAddCallback += list => {
				var index = property.arraySize;
				property.InsertArrayElementAtIndex(index);
				var newElementProp = property.GetArrayElementAtIndex(index);
				var bindModeProp = newElementProp.FindPropertyRelative("Mode");
				bindModeProp.intValue = (int) LuaMVVMBindingOption.BindMode.ONE_TIME;
			};
			m_mvvmBindList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "MVVM Binding"); };
			m_mvvmBindList.elementHeight = UIEditorUtil.LINE_HEIGHT * 2;
			m_mvvmBindList.drawElementCallback += (rect, index, active, focused) => {
				rect.y++;
				var subRects = GetRowRects(rect);
				var enabledRect = subRects[0];
				var functionRect = subRects[1];
				var goRect = subRects[2];
				var argRect = subRects[3];
				var globalRect = subRects[4];

				var prop = property.GetArrayElementAtIndex(index);
				var bindTargetProp = prop.FindPropertyRelative("BindTarget");
				var bindTargetPropProp = prop.FindPropertyRelative("BindTargetProp");
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(enabledRect, bindTargetProp, GUIContent.none);
				if( EditorGUI.EndChangeCheck() && bindTargetProp.objectReferenceValue ) {
					var menu = new GenericMenu();

					var component = bindTargetProp.objectReferenceValue as Component;
					List<Component> components = new List<Component>();
					if( component != null ) {
						component.GetComponents(components);
					}
					else {
						var go = bindTargetProp.objectReferenceValue as GameObject;
						if( go ) {
							go.GetComponents(components);
						}
					}

					if( components.Count > 0 ) {
						foreach( var c in components ) {
							menu.AddItem(new GUIContent(c.GetType().Name), false, () => {
								bindTargetProp.objectReferenceValue = c;
								if( m_componentTypePropertyShortcut.TryGetValue(c.GetType(), out var propertyName) ) {
									bindTargetPropProp.stringValue = propertyName;
								}

								bindTargetProp.serializedObject.ApplyModifiedProperties();
							});
						}

						menu.ShowAsContext();
					}
				}

				var bindModeProp = prop.FindPropertyRelative("Mode");
				EditorGUI.PropertyField(functionRect, bindModeProp, GUIContent.none);

				if( bindTargetProp.objectReferenceValue != null ) {
					List<string> names;
					if( bindModeProp.intValue == (int) LuaMVVMBindingOption.BindMode.EVENT ) {
						var typ = bindTargetProp.objectReferenceValue.GetType();
						names = new List<string>(8);
						if( m_bindingEventNames.TryGetValue(typ, out var eventNames) ) {
							names.AddRange(eventNames);
						}
						else {
							Debug.LogError("Unknown event type : " + typ.FullName);
						}
					}
					else {
						var typ = bindTargetProp.objectReferenceValue.GetType();
						var propertyInfos = typ.GetProperties();
						names = new List<string>(128) {"SetActive"};
						foreach( var propertyInfo in propertyInfos ) {
							if( propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.PropertyType.IsPublic &&
							    propertyInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0 ) {
								if( propertyInfo.Name == "LuaArrayData" ) {
									bindTargetPropProp.stringValue = "LuaArrayData";
								}

								names.Add(propertyInfo.Name);
							}
						}
					}

					var nameArr = names.ToArray();
					var select = EditorGUI.Popup(goRect, Array.IndexOf(nameArr, bindTargetPropProp.stringValue),
						nameArr);
					if( select >= 0 && select < names.Count ) {
						bindTargetPropProp.stringValue = names[select];
					}
					else {
						bindTargetPropProp.stringValue = "";
					}
				}

				var option = prop.GetPropertyObject() as LuaMVVMBindingOption;
				if( Application.isPlaying && option.LuaValue != null ) {
					argRect.width *= 0.5f;
					var bindPathProp = prop.FindPropertyRelative("Path");
					EditorGUI.PropertyField(argRect, bindPathProp, GUIContent.none);

					var width = argRect.width;
					argRect.xMin = argRect.xMax;
					argRect.width = width;
					EditorGUI.LabelField(argRect, option.LuaValue.ToString());
				}
				else {
					var bindPathProp = prop.FindPropertyRelative("Path");
					EditorGUI.PropertyField(argRect, bindPathProp, GUIContent.none);

					var expressionProp = prop.FindPropertyRelative("m_expression");
					EditorGUI.PropertyField(globalRect, expressionProp, GUIContent.none);
				}
			};

			m_mvvmBindList.onSelectCallback += list => {
				m_extraMVVMBinding = null;
				if( list.index == -1 )
					return;

				if( list.index >= list.serializedProperty.arraySize ) {
					return;
				}

				var mvvmProp = list.serializedProperty.GetArrayElementAtIndex(list.index);
				var option = mvvmProp.GetPropertyObject() as LuaMVVMBindingOption;
				if( option != null && option.BindTarget ) {
					EditorGUIUtility.PingObject(option.BindTarget);
				}
				if( option.BindTarget is not IMVVMAssetReference refGetter )
					return;

				var assetRef = refGetter.GetMVVMReference();
				if( assetRef is not {GUIDValid: true} )
					return;

				var assetPath = AssetDatabase.GUIDToAssetPath(assetRef.AssetGUID);
				if( string.IsNullOrEmpty(assetPath) )
					return;

				var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				if( !go )
					return;

				m_extraMVVMBinding = go.GetComponent<LuaMVVMBinding>();
				if( !m_extraMVVMBinding && go.transform.childCount > 0 ) {
					m_extraMVVMBinding = go.transform.GetChild(0).GetComponent<LuaMVVMBinding>();
				}
			};
		}
	}
}
