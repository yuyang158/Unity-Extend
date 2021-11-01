using Extend.Common;
using Extend.SceneManagement.SpatialStructure;
using UnityEngine;

namespace Extend.SceneManagement {
	public class SceneArrangement : MonoBehaviour {
		[SerializeField]
		private SpatialAbstract m_spatial;

		private void Start() {
			m_spatial.Build();
		}

		private void Update() {
			StatService.Get().Set(StatService.StatName.CULL_PROCESS, 0);
			StatService.Get().Set(StatService.StatName.TOTAL_RENDERERS_TO_CULL, m_spatial.RendererCount);
			m_spatial.CullVisible(Camera.main);
		}
	}
}