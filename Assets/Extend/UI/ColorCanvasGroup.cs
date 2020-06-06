using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI {
	public class ColorCanvasGroup : MonoBehaviour {
		private Graphic[] m_cachedGraphics;
		private Color[] m_originColors;

		[SerializeField]
		private Color m_color;

		public Color Color {
			get => m_color;
			set {
				m_color = value;
				for( var i = 0; i < m_cachedGraphics.Length; i++ ) {
					m_cachedGraphics[i].color = m_originColors[i] * m_color;
				}
			}
		}
		
		private void Awake() {
			m_cachedGraphics = GetComponentsInChildren<Graphic>(true);
			m_originColors = new Color[m_cachedGraphics.Length];
			for( var i = 0; i < m_cachedGraphics.Length; i++ ) {
				m_originColors[i] = m_cachedGraphics[i].color;
				m_cachedGraphics[i].color = m_originColors[i] * m_color;
			}
		}
	}
}