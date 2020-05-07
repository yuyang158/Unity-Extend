using UnityEditor;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIViewInAnimation))]
	public class UIViewInAnimationPropertyDrawer : UIAnimationPropertyBaseDrawer {
		private static UIAnimationParamCombine[] stateParamGUIs;

		protected override float SingleDoTweenHeight => lineHeight * 3;
		protected override string[] Mode => UIEditorUtil.STATE_ANIMATION_MODE;

		protected override UIAnimationParamCombine[] CurrentAnimation {
			get {
				if( stateParamGUIs == null ) {
					stateParamGUIs = new UIAnimationParamCombine[Mode.Length];
					var moveParamGUI = new UIAnimationParamCombine(3, 0);
					moveParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f);
					moveParamGUI.GetRow(1).Add("moveInDirection", 1);
					moveParamGUI.GetRow(2).Add("ease", 1);
					stateParamGUIs[0] = moveParamGUI;

					var rotateParamGUI = new UIAnimationParamCombine(3, 1);
					rotateParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f);
					rotateParamGUI.GetRow(1).Add("rotateFrom", 1);
					rotateParamGUI.GetRow(2).Add("ease", 0.5f).Add("rotateMode", 0.5f);
					stateParamGUIs[1] = rotateParamGUI;

					var scaleParamGUI = new UIAnimationParamCombine(3, 2);
					scaleParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f);
					scaleParamGUI.GetRow(1).Add("scaleFrom", 1);
					scaleParamGUI.GetRow(2).Add("ease", 1);
					stateParamGUIs[2] = scaleParamGUI;

					var fadeParamGUI = new UIAnimationParamCombine(3, 3);
					fadeParamGUI.GetRow(0).Add("duration", 0.5f).Add("delay", 0.5f);
					fadeParamGUI.GetRow(1).Add("fadeFrom", 1);
					fadeParamGUI.GetRow(2).Add("ease", 1);
					stateParamGUIs[3] = fadeParamGUI;
				}

				return stateParamGUIs;
			}
		}
	}
}