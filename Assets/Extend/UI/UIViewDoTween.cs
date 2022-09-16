using System.Linq;
using DG.Tweening;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp]
	public class UIViewDoTween : UIViewBase {
		public UIViewInAnimation ShowAnimation;
		public UIViewOutAnimation HideAnimation;
		public UIViewLoopAnimation LoopAnimation;

		private Tween[] m_currentTweens;

		private Tween[] CurrentTweens {
			get => m_currentTweens;
			set {
				if( CurrentTweens != null ) {
					foreach( var tween in CurrentTweens ) {
						tween?.Complete();
					}
				}

				m_currentTweens = value;
			}
		}

		protected override void Awake() {
			base.Awake();
			if( ShowAnimation is {Enabled: true} ) {
				ShowAnimation.CacheStartValue(transform);
			}

			if( HideAnimation is {Enabled: true} ) {
				HideAnimation.CacheStartValue(transform);
			}

			if( LoopAnimation is {Enabled: true} ) {
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

			if( CurrentTweens.Any(tween => tween != null && !tween.IsComplete()) ) {
				return;
			}

			m_currentTweens = null;
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

		private void OnDestroy() {
			ShowAnimation?.Destroy();
			HideAnimation?.Destroy();
			LoopAnimation?.Destroy();
		}
	}
}