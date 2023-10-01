using Extend.StateActionGroup;
using Extend.StateActionGroup.Behaviour;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp]
	public class UIViewSAG : UIViewBase {
		private SAG m_sag;
		
		protected override void Awake() {
			base.Awake();
			m_sag = GetComponent<SAG>();
		}

		protected override void OnShow() {
			m_sag.Switch("Show");
		}

		protected override void OnHide() {
			m_sag.Switch("Hide");
		}

		protected override void OnLoop() {
			m_sag.Switch("Loop");
		}

		private void Update() {
			var complete = true;
			foreach( BehaviourBase behaviour in m_sag.Behaviours ) {
				complete &= behaviour.Complete;
			}

			if( !complete ) {
				return;
			}
			if( ViewStatus == Status.Showing ) {
				Loop();
			}
			else if( ViewStatus == Status.Hiding ) {
				OnClosed();
			}
		}
	}
}