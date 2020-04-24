﻿using UnityEngine;

namespace Extend.UI {
	[RequireComponent(typeof(Animator))]
	public class UIViewAnimator : UIViewBase {
		private static readonly int SHOW_HASH = Animator.StringToHash("Show");
		private static readonly int HIDE_HASH = Animator.StringToHash("Hide");
		private static readonly int LOOP_HASH = Animator.StringToHash("Loop");

		private Animator animator;
		private void Awake() {
			animator = GetComponent<Animator>();
		}

		private void PlayHashAndUpdate(int hash) {
			animator.enabled = true;
			animator.Play(hash);
			animator.Update(0);
		}

		protected override void OnShow() {
			if( !animator.HasState(0, SHOW_HASH) ) {
				Loop();
				return;
			}
			PlayHashAndUpdate(SHOW_HASH);
		}

		protected override void OnHide() {
			if( !animator.HasState(0, HIDE_HASH) ) {
				OnClosed();
				return;
			}
			PlayHashAndUpdate(HIDE_HASH);
		}

		protected override void OnLoop() {
			if( !animator.HasState(0, LOOP_HASH) ) {
				return;
			}
			PlayHashAndUpdate(LOOP_HASH);
		}

		public void OnEvent(string evt) {
			if( evt == "Finish" ) {
				animator.enabled = false;
				if( ViewStatus == Status.Showing ) {
					Loop();
				}
				else if( ViewStatus == Status.Hiding ) {
					OnClosed();
				}
			}
		}
	}
}