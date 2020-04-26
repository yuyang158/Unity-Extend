using System.Linq;
using DG.Tweening;

namespace Extend.UI {
	public class UIViewDoTween : UIViewBase {
		public UIViewInAnimation ShowAnimation;
		public UIAnimation HideAnimation;
		public UIViewLoopAnimation LoopAnimation;

		private Tween[] currentTweens;

		private void Awake() {
			if( ShowAnimation != null && ShowAnimation.Enabled ) {
				ShowAnimation.CacheStartValue(transform);
			}

			if( HideAnimation != null && HideAnimation.Enabled ) {
				HideAnimation.CacheStartValue(transform);
			}

			if( LoopAnimation != null && LoopAnimation.Enabled ) {
				LoopAnimation.CacheStartValue(transform);
			}
		}

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

			if( currentTweens.Any(tween => tween != null && !tween.IsComplete()) ) {
				return;
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