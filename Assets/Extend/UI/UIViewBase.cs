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

		protected virtual void Awake() {
			CanvasGroup = GetComponent<CanvasGroup>();
			Canvas = GetComponent<Canvas>();
			if( Canvas )
				Canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
		}

		public Canvas Canvas { get; private set; }
		public CanvasGroup CanvasGroup { get; private set; }

		private Status viewStatus;

		public Status ViewStatus {
			get => viewStatus;
			protected set {
				viewStatus = value;
				switch( viewStatus ) {
					case Status.Showing:
					case Status.Hiding:
						CanvasGroup.interactable = false;
						break;
					case Status.Loop:
						CanvasGroup.interactable = true;
						break;
					case Status.Hidden:
						if( Canvas )
							Canvas.enabled = false;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void SetVisible(bool visible) {
			CanvasGroup.alpha = visible ? 1 : 0;
			CanvasGroup.blocksRaycasts = visible;
		}

		protected abstract void OnShow();

		public void Show() {
			if( Canvas )
				Canvas.enabled = true;
			ViewStatus = Status.Showing;
			Showing?.Invoke();
			OnShow();
		}

		protected abstract void OnHide();

		public void Hide() {
			ViewStatus = Status.Hiding;
			Hiding?.Invoke();
			OnHide();
		}

		protected abstract void OnLoop();

		protected void Loop() {
			ViewStatus = Status.Loop;
			Shown?.Invoke();
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