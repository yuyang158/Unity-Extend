using UnityEngine;

namespace Extend.UI.Scroll {
	[DisallowMultipleComponent]
	public class ScrollCellScaleModifier : MonoBehaviour {
		[SerializeField]
		private AnimationCurve m_scale;

		private RectTransform m_scrollViewport;
		private bool m_vertical;
		private void Awake() {
			var scroll = GetComponentInParent<LoopScrollRect>();
			m_vertical = scroll.vertical;
			m_scrollViewport = scroll.viewport;
			scroll.onValueChanged.AddListener((_) => {
				float time = 0;
				if( m_vertical ) {
					time = m_scrollViewport.InverseTransformVector(transform.position).y / m_scrollViewport.rect.height + 0.5f;
				}
				else {
					time = m_scrollViewport.InverseTransformVector(transform.position).x / m_scrollViewport.rect.width + 0.5f;
				}

				var val = m_scale.Evaluate(time);
				transform.localScale = new Vector3(val, val, 1);
			});
		}
	}
}