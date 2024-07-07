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

		public bool PauseGame => m_pauseGame;

		public GameObject FirstSelectGameObject {
			get => m_firstSelectGameObject;
			set => m_firstSelectGameObject = value;
		}

		private void Start() {
			var input = FindObjectOfType<PlayerInput>();
			input.controlsChangedEvent.AddListener(SelectGameObject);

			SelectGameObject(input);
		}

		private void SelectGameObject(PlayerInput input) {
			if( EventSystem.current.currentSelectedGameObject ) {
				return;
			}
			
			if( input.currentControlScheme == "Gamepad" && m_firstSelectGameObject ) {
				EventSystem.current.SetSelectedGameObject(m_firstSelectGameObject);
			}
		}
	}
}