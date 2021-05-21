using System.Linq;

namespace Extend.UI {
	public class UIViewCompound : UIViewBase {
		public UIViewBase[] Views;

		private void ShownCheck() {
			if( Views.All(view => view.ViewStatus == Status.Loop) ) {
				ViewStatus = Status.Loop;
			}
		}

		private void HiddenCheck() {
			if( Views.All(view => view.ViewStatus == Status.Hidden) ) {
				ViewStatus = Status.Hidden;
				OnClosed();
			}
		}

		protected override void OnShow() {
			foreach( var view in Views ) {
				view.Shown += ShownCheck;
			}
			foreach( var view in Views ) {
				view.Show();
			}
		}

		protected override void OnHide() {
			foreach( var view in Views ) {
				view.Hidden += HiddenCheck;
			}
			foreach( var view in Views ) {
				view.Hide();
			}
		}

		protected override void OnLoop() {
		}
	}
}