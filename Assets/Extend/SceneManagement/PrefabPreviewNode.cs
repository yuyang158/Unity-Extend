using System;
using UnityEngine;

namespace Extend.SceneManagement {
	[ExecuteAlways]
	public class PrefabPreviewNode : MonoBehaviour {
		[SerializeField]
		private GameObject Prefab;

		private GameObject m_prefabInstance;
		private GameObject m_originPrefab;

#if UNITY_EDITOR
		private void Update() {
			if( Application.isPlaying )
				return;

			if( m_originPrefab != Prefab ) {
				m_originPrefab = Prefab;
				if( !Prefab ) {
					DestroyImmediate(m_prefabInstance);
					m_prefabInstance = null;
				}
				else {
					var go = Instantiate(Prefab, transform, false);
					go.hideFlags = HideFlags.HideAndDontSave;
					var t = go.transform;
					t.localPosition = Vector3.zero;
					t.localRotation = Quaternion.identity;

					m_prefabInstance = go;
				}
			}
		}
#endif
	}
}