using System.Linq;
using DG.Tweening;

namespace Extend.UI {
	public class UIViewDoTween : UIViewBase {
		public UIViewInAnimation ShowAnimation;
		public UIAnimation HideAnimation;
		public UIViewLoopAnimation LoopAnimation;

		private Tween[] currentTweens;

		private Tween[] CurrentTweens {
			get => currentTweens;
			set {
				if( CurrentTweens != null ) {
					foreach( var tween in CurrentTweens ) {
						tween.Complete();
					}
				}

				currentTweens = value;
			}
		}

		protected override void Awake() {
			base.Awake();
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
				CurrentTweens = ShowAnimation.Active(transform);
			}
			else {
				Loop();
			}
		}

		protected override void OnHide() {
			if( HideAnimation.Enabled ) {
				enabled = true;
				CurrentTweens = HideAnimation.Active(transform);
			}
			else {
				OnClosed();
			}
		}

		protected override void OnLoop() {
			if( LoopAnimation.Enabled ) {
				enabled = true;
				CurrentTweens = LoopAnimation.Active(transform);
			}
		}

		private void Update() {
			if( CurrentTweens == null ) {
				enabled = false;
				return;
			}

			if( CurrentTweens.Any(tween => tween != null && tween.active) ) {
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

		private void OnDisable() {
			CurrentTweens = null;
		}
	}
}