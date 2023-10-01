using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI {
	[RequireComponent(typeof(CanvasRenderer))]
	public class MeshImage : MonoBehaviour {
		[SerializeField]
		private Mesh m_mesh;
		
		[SerializeField]
		private Material m_material;

		private void OnEnable() {
			if( !m_material || !m_mesh ) {
				return;
			}
			var canvasRenderer = GetComponent<CanvasRenderer>();
			canvasRenderer.materialCount = 1;
			canvasRenderer.SetMaterial(m_material, 0);
			canvasRenderer.SetMesh(m_mesh);
		}
	}
}