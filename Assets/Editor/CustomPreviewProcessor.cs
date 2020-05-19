using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	[InitializeOnLoad]
	public static class CustomPreviewProcessor {
		private static readonly Dictionary<Extend.Common.CustomPreviewAttribute, Type> m_previewTypes = new Dictionary<Extend.Common.CustomPreviewAttribute, Type>();
		static CustomPreviewProcessor() {
			var types = typeof(CustomPreviewProcessor).Assembly.GetTypes();
			foreach( var type in types ) {
				if( type.IsSubclassOf(typeof(ObjectPreview)) ) {
					var attributes = type.GetCustomAttributes(typeof(Extend.Common.CustomPreviewAttribute)).ToArray();
					if(attributes.Length == 0) 
						continue;

					var previewAttribute = attributes[0] as Extend.Common.CustomPreviewAttribute;
					m_previewTypes.Add(previewAttribute, type);
				}
			}
			
			ParticleSystemEditorUtilsReflect.InitType();
		}

		public static ObjectPreview TryGeneratePreview(GameObject target) {
			if( !target )
				return null;
			foreach( var previewTypePair in m_previewTypes ) {
				var attribute = previewTypePair.Key;
				bool exist;
				if( attribute.IncludeChildNode ) {
					var components = target.GetComponentsInChildren(attribute.PreviewBehaviour);
					exist = components.Length > 0;
				}
				else {
					exist = target.GetComponent(attribute.PreviewBehaviour);
				}

				if( exist ) {
					return Activator.CreateInstance(previewTypePair.Value) as ObjectPreview;
				}
			}
			
			return null;
		}
	}
}