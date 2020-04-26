using UnityEngine;

namespace Extend.Common {
	public class LabelText : PropertyAttribute {
		public string Text { get; }

		public LabelText(string text) {
			Text = text;
		}
	}
}