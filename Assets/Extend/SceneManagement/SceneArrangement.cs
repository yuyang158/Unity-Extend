using System.Collections.Generic;
using Extend.Common;
using Extend.SceneManagement.Culling;
using Extend.SceneManagement.Modifier;
using Extend.SceneManagement.SpatialStructure;
using UnityEngine;
using UnityEngine.Profiling;
using XLua;

namespace Extend.SceneManagement {
	[LuaCallCSharp, DisallowMultipleComponent]
	public class SceneArrangement : MonoBehaviour {
		[SerializeField]
		private SpatialAbstract m_spatial;
		[SerializeField]
		private CullMethodBase m_cullMethod;
		[SerializeField, Range(0, 4)]
		private float m_updateTriggerThreshold = 0.25f;
		[SerializeField]
		private bool m_forceNoDraw;

		public SpatialAbstract Spatial => m_spatial;
		public CullMethodBase CullMode => m_cullMethod;

		private Vector3 m_recordPosition = Vector3.negativeInfinity;
		private readonly Plane[] m_frustumPlanes = new Plane[6];
		private readonly List<ISceneModifier> m_modifiers = new List<ISceneModifier>();

		private void Start() {
			m_spatial.Build();
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

			if( Vector3.SqrMagnitude(m_cullMethod.BoundsCenter - m_recordPosition) > m_updateTriggerThreshold ) {
				m_recordPosition = m_cullMethod.BoundsCenter;
				m_spatial.CullVisible(m_cullMethod);
			}
			m_spatial.JobSchedule.Schedule();
			Profiler.EndSample();
		}

		private void LateUpdate() {
			m_spatial.JobSchedule.Complete();
			foreach( var modifier in m_modifiers ) {
				modifier.Update(this);
			}

			if( m_forceNoDraw ) {
				return;
			}
			m_spatial.JobSchedule.Draw();
		}
	}
}