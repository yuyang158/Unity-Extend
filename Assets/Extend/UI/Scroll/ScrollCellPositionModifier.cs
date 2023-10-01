using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI.Scroll {
	[DisallowMultipleComponent]
	public class ScrollCellPositionModifier : MonoBehaviour {
		[SerializeField]
		private AnimationCurve m_positionX;
		[SerializeField]
		private AnimationCurve m_positionY;
		
		private RectTransform m_scrollViewport;
		private bool m_vertical;
		private void Awake() {
			var scroll = GetComponentInParent<LoopScrollRect>();
			m_vertical = scroll.vertical;
			m_scrollViewport = scroll.viewport;
			var rectTransform = transform as RectTransform;
			scroll.onValueChanged.AddListener((_) => {
				float time = 0;
				if( m_vertical ) {
					time = m_scrollViewport.InverseTransformVector(transform.position).y / m_scrollViewport.rect.height + 0.5f;
					rectTransform.anchoredPosition = new Vector2(m_positionX.length > 0 ? m_positionX.Evaluate(time) : 0, 0);
				}
				else {
					time = m_scrollViewport.InverseTransformVector(transform.position).x / m_scrollViewport.rect.width + 0.5f;
					rectTransform.anchoredPosition = new Vector2(0, m_positionY.length > 0 ? m_positionY.Evaluate(time) : 0);
				}
			});
		}
	}
}