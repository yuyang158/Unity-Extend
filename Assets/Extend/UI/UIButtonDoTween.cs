using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Extend.UI {
	[RequireComponent(typeof(Button))]
	public class UIButtonDoTween : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler {
		public UIAnimation PointerDown;
		public UIAnimation PointerClick;
		public UIAnimation PointerUp;

		private Tween[] m_currentTweens;

		private Tween[] CurrentTweens {
			get => m_currentTweens;
			set {
				if( CurrentTweens != null ) {
					foreach( var tween in CurrentTweens ) {
						tween.Kill(true);
					}
				}

				m_currentTweens = value;
			}
		}

		private Button button;

		private void Awake() {
			button = GetComponent<Button>();
			if( PointerDown.Enabled ) {
				PointerDown.CacheStartValue(transform);
			}

			if( PointerClick.Enabled ) {
				PointerClick.CacheStartValue(transform);
			}

			if( PointerUp.Enabled ) {
				PointerUp.CacheStartValue(transform);
			}
		}

		public void OnPointerDown(PointerEventData eventData) {
			if( PointerDown.Enabled && button.interactable ) {
				CurrentTweens = PointerDown.Active(transform);
			}
			else {
				CurrentTweens = null;
			}
		}

		public void OnPointerClick(PointerEventData eventData) {
			if( PointerClick.Enabled && button.interactable ) {
				CurrentTweens = PointerClick.Active(transform);
			}
			else {
				CurrentTweens = null;
			}
		}

		public void OnPointerUp(PointerEventData eventData) {
			if( PointerUp.Enabled && button.interactable ) {
				CurrentTweens = PointerUp.Active(transform);
			}
			else {
				CurrentTweens = null;
			}
		}

		private void OnDisable() {
			CurrentTweens = null;
		}
	}
}