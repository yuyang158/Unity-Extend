using System;
using UnityEngine;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp]
	public abstract class UIViewBase : MonoBehaviour {
		public event Action Showing;
		public event Action Shown;
		public event Action Hiding;
		public event Action Hidden;

		public bool FullScreen;
		
		public enum LayerName {
			Scene,
			Dialog,
			Popup,
			Tip,
			MostTop
		}

		public LayerName Layer;

		public enum Status {
			Showing,
			Loop,
			Hiding,
			Hidden
		}

		public Status ViewStatus {
			get;
			protected set;
		}

		protected abstract void OnShow();

		public void Show() {
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