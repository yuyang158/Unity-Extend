using Extend.EventAsset;
using UnityEngine;

namespace Extend.UI {
	[RequireComponent(typeof(Animator))]
	public class UIViewAnimator : UIViewBase {
		private static readonly int SHOW_HASH = Animator.StringToHash("Show");
		private static readonly int HIDE_HASH = Animator.StringToHash("Hide");
		private static readonly int LOOP_HASH = Animator.StringToHash("Loop");

		private Animator m_animator;

		protected override void Awake() {
			base.Awake();
			m_animator = GetComponent<Animator>();
			m_animator.enabled = false;
		}

		private void PlayHashAndUpdate(int hash) {
			m_animator.enabled = true;
			m_animator.Play(hash);
			m_animator.Update(0);
		}

		protected override void OnShow() {
			if( !m_animator.HasState(0, SHOW_HASH) ) {
				Loop();
				return;
			}
			PlayHashAndUpdate(SHOW_HASH);
		}

		protected override void OnHide() {
			if( !m_animator.HasState(0, HIDE_HASH) ) {
				OnClosed();
				return;
			}
			PlayHashAndUpdate(HIDE_HASH);
		}

		protected override void OnLoop() {
			if( !m_animator.HasState(0, LOOP_HASH) ) {
				return;
			}
			PlayHashAndUpdate(LOOP_HASH);
		}

		public void OnEvent(EventInstance evt)
		{
			if (evt.EventName != "Finish") return;
			m_animator.enabled = false;
			if( ViewStatus == Status.Showing ) {
				Loop();
			}
			else if( ViewStatus == Status.Hiding ) {
				OnClosed();
			}
		}
	}
}