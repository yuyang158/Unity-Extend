using System;
using UnityEngine;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp, RequireComponent(typeof(CanvasGroup))]
	public abstract class UIViewBase : MonoBehaviour {
		public event Action Showing;
		public event Action Shown;
		public event Action Hiding;
		public event Action Hidden;

		public enum Status {
			Showing,
			Loop,
			Hiding,
			Hidden
		}

		private Canvas m_cachedCanvas;
		private CanvasGroup m_cachedCanvasGroup;

		protected virtual void Awake() {
			m_cachedCanvasGroup = GetComponent<CanvasGroup>();
			m_cachedCanvas = GetComponent<Canvas>();
			m_cachedCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
		}

		public Canvas canvas => m_cachedCanvas;
		public CanvasGroup canvasGroup => m_cachedCanvasGroup;

		private Status viewStatus;

		public Status ViewStatus {
			get => viewStatus;
			protected set {
				viewStatus = value;
				switch( viewStatus ) {
					case Status.Showing:
					case Status.Hiding:
						m_cachedCanvasGroup.interactable = false;
						break;
					case Status.Loop:
						m_cachedCanvasGroup.interactable = true;
						break;
					case Status.Hidden:
						if( m_cachedCanvas )
							m_cachedCanvas.enabled = false;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void SetVisible(bool visible) {
			m_cachedCanvas.enabled = visible;
			canvasGroup.blocksRaycasts = visible;
		}

		protected abstract void OnShow();

		public void Show() {
			if( m_cachedCanvas )
				m_cachedCanvas.enabled = true;
			Showing?.Invoke();
			ViewStatus = Status.Showing;
			OnShow();
		}

		protected abstract void OnHide();

		public void Hide() {
			Hiding?.Invoke();
			ViewStatus = Status.Hiding;
			OnHide();
		}

		protected abstract void OnLoop();

		protected void Loop() {
			Shown?.Invoke();
			ViewStatus = Status.Loop;
			OnLoop();
		}

		protected void ClearEvents() {
			Showing = null;
			Shown = null;
			Hiding = null;
			Hidden = null;
		}

		protected virtual void OnClosed() {
			ViewStatus = Status.Hidden;
			Hidden?.Invoke();
		}
	}
}