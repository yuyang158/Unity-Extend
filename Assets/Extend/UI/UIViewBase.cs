using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp, RequireComponent(typeof(CanvasGroup))]
	public abstract class UIViewBase : MonoBehaviour {
		public event Action Showing;
		public event Action Shown;
		public event Action Hiding;
		public event Action Hidden;

		public bool ControlInteractable = true;

		[LuaCallCSharp]
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
						if(ControlInteractable)
							CanvasGroup.interactable = false;
						break;
					case Status.Loop:
						if(ControlInteractable)
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
			Canvas.enabled = visible;
			CanvasGroup.interactable = visible;
		}

		protected abstract void OnShow();

		public void Show(Action shown = null) {
			if( Canvas )
				Canvas.enabled = true;
			CanvasGroup.interactable = false;
			ViewStatus = Status.Showing;
			Showing?.Invoke();
			Showing = null;

			if( shown != null ) {
				Shown += shown;
			}
			
			OnShow();
		}

		protected abstract void OnHide();

		public void Hide(Action hidden = null) {
			CanvasGroup.interactable = false;
			ViewStatus = Status.Hiding;
			Hiding?.Invoke();
			Hiding = null;

			if( hidden != null ) {
				Hidden += hidden;
			}
			OnHide();
		}

		protected abstract void OnLoop();

		protected void Loop() {
			CanvasGroup.interactable = true;
			ViewStatus = Status.Loop;
			Shown?.Invoke();
			Shown = null;
			OnLoop();
		}

		protected virtual void OnClosed() {
			ViewStatus = Status.Hidden;
			Hidden?.Invoke();
			Hidden = null;
		}
	}
}
