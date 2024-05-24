using System;
using UnityEngine;

namespace Extend.Common {
	public interface IExtendAttribute {
		
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class LabelTextAttribute : PropertyAttribute, IExtendAttribute {
		public string Text { get; }

		public LabelTextAttribute(string text) {
			Text = text;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ReorderListAttribute : SpecialCaseAttribute {
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class HideIfAttribute : PropertyAttribute, IExtendAttribute {
		public readonly string FieldName;
		public readonly object Value;
		public HideIfAttribute(string fieldName, object value) {
			FieldName = fieldName;
			Value = value;
		}
	}

	
	[AttributeUsage(AttributeTargets.Field)]
	public class ShowIfAttribute : PropertyAttribute, IExtendAttribute {
		public readonly string FieldName;
		public readonly object Value;
		public ShowIfAttribute(string fieldName, object value) {
			FieldName = fieldName;
			Value = value;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class EnableIfAttribute : PropertyAttribute, IExtendAttribute {
		public readonly string FieldName;
		public readonly object Value;
		public EnableIfAttribute(string fieldName, object value) {
			FieldName = fieldName;
			Value = value;
		}
	}

	public enum ButtonSize {
		Small,
		Medium,
		Large
	}
	
	[AttributeUsage(AttributeTargets.Method)]
	public class ButtonAttribute : PropertyAttribute, IExtendAttribute {
		public readonly string ButtonName;
		public readonly ButtonSize Size;
		public ButtonAttribute(string buttonName, ButtonSize size = ButtonSize.Small) {
			ButtonName = buttonName;
			Size = size;
		}

		public ButtonAttribute(ButtonSize size) {
			Size = size;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class RequireAttribute : PropertyAttribute, IExtendAttribute {
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class EnumToggleButtonsAttribute : PropertyAttribute {
		
	}
	
	public abstract class SpecialCaseAttribute : PropertyAttribute, IExtendAttribute {
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class AssetOnlyAttribute : PropertyAttribute, IExtendAttribute {
		
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class SceneOnlyAttribute : PropertyAttribute, IExtendAttribute {
		
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class CustomPreviewAttribute : Attribute {
		public readonly Type PreviewBehaviour;
		public readonly bool IncludeChildNode;
		public CustomPreviewAttribute(Type previewBehaviour, bool includeChildNode = false) {
			PreviewBehaviour = previewBehaviour;
			IncludeChildNode = includeChildNode;
		}
	}
}
