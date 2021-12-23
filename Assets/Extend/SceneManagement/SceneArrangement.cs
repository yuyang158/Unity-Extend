using System.Collections.Generic;
using Extend.Common;
using Extend.SceneManagement.Modifier;
using Extend.SceneManagement.SpatialStructure;
using UnityEngine;
using UnityEngine.Profiling;
using XLua;

namespace Extend.SceneManagement {
	[LuaCallCSharp]
	public class SceneArrangement : MonoBehaviour {
		[SerializeField]
		private SpatialAbstract m_spatial;

		public SpatialAbstract Spatial => m_spatial;

		[SerializeField, Range(0, 4)]
		private float m_updateTriggerThreshold = 0.25f;

		private Vector3 m_cameraRecordPosition = Vector3.negativeInfinity;
		private readonly Plane[] m_frustumPlanes = new Plane[6];
		private readonly List<ISceneModifier> m_modifiers = new();

		private void Start() {
			m_spatial.Build(m_spatial.JobSchedule);
			for( int i = 0; i < 6; i++ ) {
				m_frustumPlanes[i] = new Plane();
			}
		}

		public void AddModifier(ISceneModifier modifier) {
			m_modifiers.Add(modifier);
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
			}
			m_spatial.JobSchedule.Schedule();
			Profiler.EndSample();
		}

		private void LateUpdate() {
			m_spatial.JobSchedule.Complete();
			foreach( var modifier in m_modifiers ) {
				modifier.Update(this);
			}

			m_spatial.JobSchedule.Draw();
		}
	}
}