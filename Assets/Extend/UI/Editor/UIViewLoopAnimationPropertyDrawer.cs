using UnityEditor;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIViewLoopAnimation))]
	public class UIViewLoopAnimationPropertyDrawer : UIAnimationPropertyBaseDrawer {
		private static UIAnimationParamCombine[] stateParamGUIs;

		protected override UIAnimationParamCombine[] CurrentAnimation {
			get {
				if( stateParamGUIs == null ) {
					stateParamGUIs = new UIAnimationParamCombine[Mode.Length];
					var moveParamGUI = new UIAnimationParamCombine(3, 0);
					moveParamGUI.GetRow(0).Add("duration", 0.25f).Add("delay", 0.25f).Add("loops", 0.25f).Add("loopType", 0.25f);
					moveParamGUI.GetRow(1).Add("moveBy", 1);
					moveParamGUI.GetRow(2).Add("ease", 1);
					stateParamGUIs[0] = moveParamGUI;

					var rotateParamGUI = new UIAnimationParamCombine(3, 1);
					rotateParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f).Add("loops", 0.25f).Add("loopType", 0.25f);
					rotateParamGUI.GetRow(1).Add("rotateBy", 1);
					rotateParamGUI.GetRow(2).Add("ease", 0.5f).Add("rotateMode", 0.5f);
					stateParamGUIs[1] = rotateParamGUI;

					var scaleParamGUI = new UIAnimationParamCombine(3, 2);
					scaleParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f).Add("loops", 0.25f).Add("loopType", 0.25f);
					scaleParamGUI.GetRow(1).Add("scaleFrom", 0.5f).Add("scaleTo", 0.5f);
					scaleParamGUI.GetRow(2).Add("ease", 1);
					stateParamGUIs[2] = scaleParamGUI;

					var fadeParamGUI = new UIAnimationParamCombine(3, 3);
					fadeParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f).Add("loops", 0.25f).Add("loopType", 0.25f);
					fadeParamGUI.GetRow(1).Add("from", 0.5f).Add("to", 0.5f);
					fadeParamGUI.GetRow(2).Add("ease", 1);
					stateParamGUIs[3] = fadeParamGUI;
				}

				return stateParamGUIs;
			}
		}

		protected override float SingleDoTweenHeight => lineHeight * 3;
		protected override string[] Mode => UIEditorUtil.STATE_ANIMATION_MODE;
	}
}