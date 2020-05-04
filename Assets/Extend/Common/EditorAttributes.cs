using System;
using UnityEngine;

namespace Extend.Common {
	public class LabelTextAttribute : PropertyAttribute, IExtendAttribute {
		public string Text { get; }

		public LabelTextAttribute(string text) {
			Text = text;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ReorderListAttribute : SpecialCaseAttribute {
	}

	public class SpecialCaseAttribute : PropertyAttribute, IExtendAttribute {
	}
}