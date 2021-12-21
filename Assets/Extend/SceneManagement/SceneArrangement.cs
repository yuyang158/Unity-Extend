using Extend.Common;
using Extend.SceneManagement.SpatialStructure;
using UnityEngine;
using UnityEngine.Profiling;

namespace Extend.SceneManagement {
	public class SceneArrangement : MonoBehaviour {
		[SerializeField]
		private SpatialAbstract m_spatial;

		[SerializeField, Range(0, 4)]
		private float m_updateTriggerThreshold = 0.25f;

		private Vector3 m_cameraRecordPosition = Vector3.negativeInfinity;
		private readonly Plane[] m_frustumPlanes = new Plane[6];

		private void Start() {
			m_spatial.Build(m_spatial.JobSchedule);
			for( int i = 0; i < 6; i++ ) {
				m_frustumPlanes[i] = new Plane();
			}
		}

		private void Update() {
			StatService.Get().Set(StatService.StatName.CULL_PROCESS, 0);
			StatService.Get().Set(StatService.StatName.TOTAL_RENDERERS_TO_CULL, m_spatial.RendererCount);

			Profiler.BeginSample("Scene Culling");
			var main = Camera.main;

			if( Vector3.SqrMagnitude(main.transform.position - m_cameraRecordPosition) > m_updateTriggerThreshold ) {
				GeometryUtility.CalculateFrustumPlanes(main, m_frustumPlanes);
				m_cameraRecordPosition = main.transform.position;
				m_spatial.CullVisible(m_frustumPlanes);
				m_spatial.JobSchedule.Schedule();
			}

			Profiler.EndSample();
		}

		private void LateUpdate() {
			m_spatial.JobSchedule.Complete();
		}
	}
}