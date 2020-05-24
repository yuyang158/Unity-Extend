using System.Reflection;
using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor.InspectorGUI {
	public class ExtendAttributeProcess {
		public virtual bool Hide => false;

		public virtual void Process(SerializedProperty property, GUIContent label, IExtendAttribute attribute) {
		}

		public virtual void PostProcess() {
		}
	}

	// ReSharper disable once UnusedType.Global
	public class LabelTextAttributeProcess : ExtendAttributeProcess {
		public override void Process(SerializedProperty property, GUIContent label, IExtendAttribute attribute) {
			label.text = ((LabelTextAttribute)attribute).Text;
		}
	}

	// ReSharper disable once UnusedType.Global
	public class HideIfAttributeProcess : ExtendAttributeProcess {
		private bool m_hide;
		public override bool Hide => m_hide;

		public override void Process(SerializedProperty property, GUIContent label, IExtendAttribute attr) {
			var attribute = attr as HideIfAttribute;
			var target = property.GetPropertyParentObject();
			var fieldInfo = target.GetType().GetField(attribute.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var val = fieldInfo.GetValue(target);
			m_hide = val.Equals(attribute.Value);
		}
	}

	// ReSharper disable once UnusedType.Global
	public class ShowIfAttributeProcess : ExtendAttributeProcess {
		private bool m_hide;
		public override bool Hide => m_hide;

		public override void Process(SerializedProperty property, GUIContent label, IExtendAttribute attr) {
			var attribute = attr as ShowIfAttribute;
			var target = property.GetPropertyParentObject();
			var fieldInfo = target.GetType().GetField(attribute.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var val = fieldInfo.GetValue(target);
			m_hide = !val.Equals(attribute.Value);
		}
	}

	// ReSharper disable once UnusedType.Global
	public class RequireAttributeProcess : ExtendAttributeProcess {
		public override void Process(SerializedProperty property, GUIContent label, IExtendAttribute attr) {
			if( property.propertyType == SerializedPropertyType.String ) {
				if( string.IsNullOrEmpty(property.stringValue) ) {
					EditorGUILayout.HelpBox($"{property.name} is required", MessageType.Error);
				}
			}
			else if( property.propertyType == SerializedPropertyType.ObjectReference ) {
				if( !property.objectReferenceValue ) {
					EditorGUILayout.HelpBox($"{property.name} is required", MessageType.Error);
				}
			}
			else {
				Debug.LogError($"only string or unity object type support require attribute");
			}
		}
	}

	// ReSharper disable once UnusedType.Global
	public class EnableIfAttributeProcess : ExtendAttributeProcess {
		public override void Process(SerializedProperty property, GUIContent label, IExtendAttribute attr) {
			var attribute = attr as EnableIfAttribute;
			var target = property.GetPropertyParentObject();
			var fieldInfo = target.GetType().GetField(attribute.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var val = fieldInfo.GetValue(target);
			if( !val.Equals(attribute.Value) ) {
				GUI.enabled = false;
			}
		}

		public override void PostProcess() {
			GUI.enabled = true;
		}
	}
}