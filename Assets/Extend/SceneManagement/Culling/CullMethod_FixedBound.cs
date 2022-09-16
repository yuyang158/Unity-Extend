using UnityEngine;
using XLua;

namespace Extend.SceneManagement.Culling {
	[LuaCallCSharp]
	public class CullMethod_FixedBound : CullMethodBase {
		[SerializeField]
		private Vector3 m_cullExtend;

		[SerializeField]
		private Vector3 m_offset;
		private Bounds m_cullBounds;

		public Transform AttachTransform { private get; set; }
		public override Vector3 BoundsCenter => m_cullBounds.center;

		private void Awake() {
			m_cullBounds = new Bounds(m_offset, m_cullExtend);
		}

		public override bool Cull(Bounds bounds) {
			return m_cullBounds.Intersects(bounds);
		}

		private void Update() {
			if( !AttachTransform ) {
				m_cullBounds = new Bounds(m_offset, m_cullExtend);
				return;
			}
			m_cullBounds = new Bounds(AttachTransform.position + m_offset, m_cullExtend);
		}

		private void OnDrawGizmosSelected() {
			Gizmos.DrawWireCube((AttachTransform ? AttachTransform.position : transform.position) + m_offset, m_cullExtend);
		}
	}
}