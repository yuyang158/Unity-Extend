using UnityEngine;
using UnityEngine.UI;

namespace Extend.Common {
	[RequireComponent(typeof(Image))]
	public class ProcessorTargetImage : ProcessorTargetBase {
		private Image m_image;
		private void Awake() {
			m_image = GetComponent<Image>();
		}

		public override void ApplyProgress(float value) {
			m_image.fillAmount = value;
		}
	}
}