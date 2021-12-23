using System;
using UnityEngine;

namespace Extend.SceneManagement.Jobs {
	public class BatchMeshMaterialMap : MonoBehaviour {
		public static readonly int MESH_MATERIAL_OFFSET = 1000000;
		
		[NonSerialized]
		public int[] CombinedIDs;
	}
}