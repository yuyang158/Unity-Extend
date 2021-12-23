using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.Jobs {
	public struct DrawInstance {
		public Matrix4x4 World;
		public Bounds Bounds;
		public int MaterialIndex;
		public int Index;
		public bool Visible;
		public int InstanceID;
	}

	public interface IMatrixBuildJob {
		void Init(DrawInstance[] instances);
		void Reset();
		Matrix4x4[] GetVisibleWorldArray();
		int GetVisibleCount();
		int GetTotalCount();
		void SetVisible(int index, bool visible);
		void Dispose();
		JobHandle StartSchedule();
	}
	
	[BurstCompile(OptimizeFor = OptimizeFor.Performance, CompileSynchronously = true)]
	public struct DrawMatrixBuildSingleJob : IJob, IMatrixBuildJob {
		[ReadOnly]
		private NativeArray<DrawInstance> m_instances;
		private NativeList<Matrix4x4> m_visibleInstances;

		public void Init(DrawInstance[] instances) {
			m_instances = new NativeArray<DrawInstance>(instances, Allocator.Persistent);
			m_visibleInstances = new NativeList<Matrix4x4>(instances.Length, Allocator.Persistent);
		}

		public void Reset() {
			m_visibleInstances.Clear();
		}

		public void Execute() {
			foreach( var instance in m_instances ) {
				if( instance.Visible ) {
					m_visibleInstances.Add(instance.World);
				}
			}
		}

		public Matrix4x4 GetWorldMatrix(int index) {
			return m_instances[index].World;
		}

		public Matrix4x4[] GetVisibleWorldArray() {
			return m_visibleInstances.ToArray();
		}

		public int GetVisibleCount() {
			return m_visibleInstances.Length;
		}

		public int GetTotalCount() {
			return m_instances.Length;
		}

		public void SetVisible(int index, bool visible) {
			var instance = m_instances[index];
			instance.Visible = visible;
			m_instances[index] = instance;
		}
		
		public bool GetVisible(int index) {
			var instance = m_instances[index];
			return instance.Visible;
		}

		public void Dispose() {
			m_instances.Dispose();
			m_visibleInstances.Dispose();
		}

		public JobHandle StartSchedule() {
			return this.Schedule();
		}
	}
}