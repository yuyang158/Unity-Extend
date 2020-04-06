using System;
using DG.Tweening;

namespace Extend.UI {
	public class UIViewDoTween : UIViewBase {
		public UIAnimation ShowAnimation;
		public UIAnimation HideAnimation;
		public UIAnimation LoopAnimation;

		private Tween[] currentTweens;

		protected override void OnShow() {
			if( ShowAnimation.Enabled ) {
				enabled = true;
				currentTweens = ShowAnimation.Active(transform);
			}
			else {
				Loop();
			}
		}

		protected override void OnHide() {
			if( HideAnimation.Enabled ) {
				enabled = true;
				HideAnimation.Active(transform);
			}
			else {
				OnClosed();
			}
		}

		protected override void OnLoop() {
			enabled = false;
			if( LoopAnimation.Enabled ) {
				LoopAnimation.Active(transform);
			}
		}

		private void Update() {
			if( currentTweens == null ) {
				enabled = false;
				return;
			}

			foreach( var tween in currentTweens ) {
				if( tween != null && !tween.IsComplete() ) {
					return;
				}
			}

			currentTweens = null;
			if( ViewStatus == Status.Showing ) {
				Loop();
			}
			else if( ViewStatus == Status.Hiding ) {
				OnClosed();
			}
		}
	}
}