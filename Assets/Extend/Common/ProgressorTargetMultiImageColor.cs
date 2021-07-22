using UnityEngine;
using UnityEngine.UI;

namespace Extend.Common {
	public sealed class ProgressorTargetMultiImageColor : ProgressorTargetBase {
		public Color[] Colors;
		private Image[] m_images;
		private Color[] m_originColors;
		private int m_previousIndex;
		private void Awake() {
			m_images = GetComponentsInChildren<Image>();
			m_originColors = new Color[m_images.Length];
			for( int i = 0; i < m_images.Length; i++ ) {
				m_originColors[i] = m_images[i].color;
			}
			m_previousIndex = 0;
		}

		public override void ApplyProgress(float value) {
			var index = Mathf.FloorToInt(value * m_images.Length);
			for( int i = 0; i < index; i++ ) {
				m_images[i].color = i >= Colors.Length ? Colors[Colors.Length - 1] : Colors[i];
			}
			for (int i = index; i < m_previousIndex; i++)
			{
				m_images[i].color = m_originColors[i];
			}

			m_previousIndex = index;
		}
	}
}