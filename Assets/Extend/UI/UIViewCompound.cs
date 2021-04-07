using System.Linq;

namespace Extend.UI {
	public class UIViewCompound : UIViewBase {
		public UIViewBase[] Views;

		protected override void Awake() {
			base.Awake();
			foreach( var view in Views ) {
				view.Shown += ShownCheck;
				view.Hidden += HiddenCheck;
			}
		}

		private void ShownCheck() {
			if( Views.All(view => view.ViewStatus == Status.Loop) ) {
				ViewStatus = Status.Loop;
			}
		}

		private void HiddenCheck() {
			if( Views.All(view => view.ViewStatus == Status.Hidden) ) {
				ViewStatus = Status.Hidden;
			}
		}

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