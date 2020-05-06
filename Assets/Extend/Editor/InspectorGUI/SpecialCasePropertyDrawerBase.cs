using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	public abstract class SpecialCasePropertyDrawerBase {
		public void OnGUI(SerializedProperty property) {
			OnGUI_Internal(property, new GUIContent(property.GetLabel()));
		}

		protected abstract void OnGUI_Internal(SerializedProperty property, GUIContent label);
	}

	public static class SpecialCaseAttributeExtensions {
		private static readonly Dictionary<Type, SpecialCasePropertyDrawerBase> drawers = new Dictionary<Type, SpecialCasePropertyDrawerBase>();

		static SpecialCaseAttributeExtensions() {
			drawers.Add(typeof(ReorderListAttribute), ReorderListPropertyDrawer.Instance);
		}

		public static SpecialCasePropertyDrawerBase GetDrawer(this SpecialCaseAttribute attr) {
			drawers.TryGetValue(attr.GetType(), out var drawer);
			return drawer;
		}
	}
}