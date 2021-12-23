using System.Collections.Generic;
using Extend.Common;
using Extend.SceneManagement.Jobs;
using UnityEngine;
using XLua;

namespace Extend.SceneManagement.Modifier {
	[LuaCallCSharp]
	public class SceneWallBlockModifier : ISceneModifier {
		public Vector3 Point1 { get; set; }
		public Vector3 Point2 { get; set; }
		public float Radius  { get; set; }
		public Vector3 Direction  { get; set; }
		public float MaxDistance  { get; set; }
		public int LayerMask  { get; set; }

		private readonly RaycastHit[] m_hits = new RaycastHit[64];

		public SceneWallBlockModifier() {
			for( int i = 0; i < m_hits.Length; i++ ) {
				m_hits[i] = new RaycastHit();
			}
		}

		private readonly List<BatchMeshMaterialMap> m_batchMeshMaterials = new(64);
		private readonly List<int> m_modifiedIDs = new(128);
		private readonly List<int> m_lastFrameModifiedIDs = new(128);

		public void Update(SceneArrangement arrangement) {
			foreach( var combinedID in m_modifiedIDs ) {
				var meshMaterialIndex = combinedID / BatchMeshMaterialMap.MESH_MATERIAL_OFFSET;
				var index = combinedID % BatchMeshMaterialMap.MESH_MATERIAL_OFFSET;
				var meshMaterial = arrangement.Spatial.JobSchedule.GetMeshMaterial(meshMaterialIndex);
				meshMaterial.SetVisible(index, true);
			}
			m_lastFrameModifiedIDs.AddRange(m_modifiedIDs);
			m_modifiedIDs.Clear();
			
			var count = Physics.CapsuleCastNonAlloc(Point1, Point2, Radius + .5f, Direction, m_hits, MaxDistance, LayerMask);
			for( int i = 0; i < count; i++ ) {
				var hit = m_hits[i];
				hit.transform.GetComponentsInChildren(m_batchMeshMaterials);
				foreach( var batchMeshMaterial in m_batchMeshMaterials ) {
					foreach( var combinedID in batchMeshMaterial.CombinedIDs ) {
						var meshMaterialIndex = combinedID / BatchMeshMaterialMap.MESH_MATERIAL_OFFSET;
						var index = combinedID % BatchMeshMaterialMap.MESH_MATERIAL_OFFSET;
						var meshMaterial = arrangement.Spatial.JobSchedule.GetMeshMaterial(meshMaterialIndex);
						if( !meshMaterial.GetVisible(index) ) {
							continue;
						}
						meshMaterial.SetVisible(index, false);
						meshMaterial.DrawOneMeshInTransparent(index);
						m_modifiedIDs.Add(combinedID);
						m_lastFrameModifiedIDs.RemoveSwap(combinedID);
					}
				}
			}

			foreach( var combinedID in m_lastFrameModifiedIDs ) {
				var meshMaterialIndex = combinedID / BatchMeshMaterialMap.MESH_MATERIAL_OFFSET;
				var index = combinedID % BatchMeshMaterialMap.MESH_MATERIAL_OFFSET;
				var meshMaterial = arrangement.Spatial.JobSchedule.GetMeshMaterial(meshMaterialIndex);
				meshMaterial.DrawOneMesh(index);
			}
			m_lastFrameModifiedIDs.Clear();
		}
	}
}