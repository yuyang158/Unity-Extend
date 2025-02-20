using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using XLua;

namespace Extend.UI.GamePad {
	[DisallowMultipleComponent, LuaCallCSharp]
	public class UIViewGamePadSupport : MonoBehaviour {
		[SerializeField]
		private GameObject m_firstSelectGameObject;

		[SerializeField]
		private bool m_pauseGame;

		[SerializeField]
		private bool m_clearSelect;

		public bool PauseGame => m_pauseGame;

		public GameObject FirstSelectGameObject {
			get => m_firstSelectGameObject;
			set {
				m_firstSelectGameObject = value;
				SelectGameObject();
			}
		}

		private GameObject m_topElement;
		public void TopElement() {
			if( m_topElement ) {
				EventSystem.current.SetSelectedGameObject(m_topElement);
				return;
			}
			SelectGameObject();
		}

		public void RecordTopElement() {
			m_topElement = EventSystem.current.currentSelectedGameObject;
		}

		private void Start() {
			var input = FindObjectOfType<PlayerInput>();
			input.controlsChangedEvent.AddListener(_ => {
				SelectGameObject();
			});

			SelectGameObject();
		}

		private void OnEnable() {
			if( m_clearSelect ) {
				EventSystem.current.SetSelectedGameObject(null);
			}
			SelectGameObject();
		}

		private void SelectGameObject() {
			if( m_firstSelectGameObject ) {
				EventSystem.current.SetSelectedGameObject(m_firstSelectGameObject);
			}
		}
	}
}