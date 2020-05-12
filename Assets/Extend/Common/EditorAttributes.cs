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
	
	public abstract class SpecialCaseAttribute : PropertyAttribute, IExtendAttribute {
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class AssetOnlyAttribute : PropertyAttribute {
		
	}
}