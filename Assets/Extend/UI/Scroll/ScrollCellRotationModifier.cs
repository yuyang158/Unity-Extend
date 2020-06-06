using UnityEngine;

namespace Extend.UI.Scroll {
	[DisallowMultipleComponent]
	public class ScrollCellRotationModifier : MonoBehaviour {
		[SerializeField]
		private AnimationCurve m_rotationX;
		[SerializeField]
		private AnimationCurve m_rotationY;
		[SerializeField]
		private AnimationCurve m_rotationZ;

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

				var angles = transform.rotation.eulerAngles;
				angles.x = m_rotationX.length > 0 ? m_rotationX.Evaluate(time) : angles.x;
				angles.y = m_rotationY.length > 0 ? m_rotationY.Evaluate(time) : angles.y;
				angles.z = m_rotationZ.length > 0 ? m_rotationZ.Evaluate(time) : angles.z;
				transform.rotation = Quaternion.Euler(angles);
			});
		}
	}
}