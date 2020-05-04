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

		private Canvas cachedCanvas;
		private CanvasGroup cachedCanvasGroup;

		protected virtual void Awake() {
			cachedCanvasGroup = GetComponent<CanvasGroup>();
			cachedCanvas = GetComponent<Canvas>();
			cachedCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
		}

		public Canvas canvas => cachedCanvas;
		public CanvasGroup canvasGroup => cachedCanvasGroup;

		private Status viewStatus;

		public Status ViewStatus {
			get => viewStatus;
			protected set {
				viewStatus = value;
				switch( viewStatus ) {
					case Status.Showing:
					case Status.Hiding:
						cachedCanvasGroup.interactable = false;
						break;
					case Status.Loop:
						cachedCanvasGroup.interactable = true;
						break;
					case Status.Hidden:
						if( cachedCanvas )
							cachedCanvas.enabled = false;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void SetVisible(bool visible) {
			cachedCanvas.enabled = visible;
			canvasGroup.blocksRaycasts = visible;
		}

		protected abstract void OnShow();

		public void Show() {
			if( cachedCanvas )
				cachedCanvas.enabled = true;
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