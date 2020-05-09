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

	public abstract class SpecialCaseAttribute : PropertyAttribute, IExtendAttribute {
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class AssetOnlyAttribute : PropertyAttribute {
		
	}
}