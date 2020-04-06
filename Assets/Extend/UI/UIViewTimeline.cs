using UnityEngine;
using UnityEngine.Playables;

namespace Extend.UI {
	[RequireComponent(typeof(PlayableDirector))]
	public class UIViewTimeline : UIViewBase {
		public PlayableDirector ShowDirector;
		public PlayableDirector HideDirector;
		public PlayableDirector LoopDirector;

		protected override void OnShow() {
			if( !ShowDirector ) {
				Loop();
				return;
			}

			ShowDirector.Play();
		}

		protected override void OnHide() {
			if( !HideDirector ) {
				OnClosed();
				return;
			}

			HideDirector.Play();
		}

		protected override void OnLoop() {
			if( !LoopDirector ) {
				return;
			}

			LoopDirector.Play();
		}

		public void OnFinishSignal() {
			if( ViewStatus == Status.Showing ) {
				Loop();
			}
			else if( ViewStatus == Status.Hiding ) {
				OnClosed();
			}
		}
	}
}