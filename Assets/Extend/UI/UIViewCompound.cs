using UnityEngine;
using UnityEngine.Playables;

namespace Extend.UI {
	[RequireComponent(typeof(PlayableDirector))]
	public class UIViewCompound : UIViewBase {
		public UIViewBase[] Views;

		protected override void OnShow() {
			foreach( var view in Views ) {
				view.Show();
			}
		}

		protected override void OnHide() {
			foreach( var view in Views ) {
				view.Hide();
			}
		}

		protected override void OnLoop() {
			
		}
	}
}