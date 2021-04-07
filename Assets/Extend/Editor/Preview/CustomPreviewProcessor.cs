﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CustomPreviewAttribute = Extend.Common.CustomPreviewAttribute;

namespace Extend.Editor.Preview {
	[InitializeOnLoad]
	public static class CustomPreviewProcessor {
		private static readonly Dictionary<CustomPreviewAttribute, Type> m_previewTypes = new Dictionary<CustomPreviewAttribute, Type>();

		static CustomPreviewProcessor() {
			var typeCollection = TypeCache.GetTypesDerivedFrom<ObjectPreview>();
			foreach( var type in typeCollection ) {
				var attributes = type.GetCustomAttributes(typeof(CustomPreviewAttribute)).ToArray();
				if( attributes.Length == 0 )
					continue;

				var previewAttribute = attributes[0] as CustomPreviewAttribute;
				if( previewAttribute == null )
					continue;
				m_previewTypes.Add(previewAttribute, type);
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