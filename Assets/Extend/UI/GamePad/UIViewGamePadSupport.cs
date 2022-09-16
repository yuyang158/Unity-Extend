using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.UI.GamePad {
	public class UIViewGamePadSupport : MonoBehaviour {
		[SerializeField]
		private GameObject m_firstSelectGameObject;

		private void Start() {
			EventSystem.current.SetSelectedGameObject(m_firstSelectGameObject);
		}
	}
}