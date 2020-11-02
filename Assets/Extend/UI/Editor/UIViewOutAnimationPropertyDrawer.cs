using Extend.Common.Editor;
using UnityEditor;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIViewOutAnimation))]
	public class UIViewOutAnimationPropertyDrawer : UIAnimationPropertyBaseDrawer {
		private static UIAnimationParamCombine[] stateParamGUIs;

		protected override float SingleDoTweenHeight => UIEditorUtil.LINE_HEIGHT * 3;
		protected override string[] Mode => UIEditorUtil.STATE_ANIMATION_MODE;

		protected override UIAnimationParamCombine[] CurrentAnimation {
			get {
				if( stateParamGUIs == null ) {
					stateParamGUIs = new UIAnimationParamCombine[Mode.Length];

					var moveParamGUI = new UIAnimationParamCombine(4, 0);
					moveParamGUI.GetRow(0).Add("m_duration", 0.35f).Add("m_delay", 0.35f).
						Add("m_customFromTo", 0.3f, "", 0.9f);
					moveParamGUI.GetRow(1).Add("m_moveOutDirection", 1).Condition("m_customFromTo", false);
					moveParamGUI.GetRow(2).Add("m_moveFrom", 0.5f).
						Add("m_moveTo", 0.5f).Condition("m_customFromTo", true);
					moveParamGUI.GetRow(3).Add("m_ease", 1);
					stateParamGUIs[0] = moveParamGUI;

					var rotateParamGUI = new UIAnimationParamCombine(4, 1);
					rotateParamGUI.GetRow(0).Add("m_duration", 0.35f).Add("m_delay", 0.35f).
						Add("m_customFromTo", 0.3f, "", 0.9f);
					rotateParamGUI.GetRow(1).Add("m_rotateTo", 1).Condition("m_customFromTo", false);
					rotateParamGUI.GetRow(2).Add("m_rotateFrom", 0.5f).
						Add("m_rotateTo", 0.5f).Condition("m_customFromTo", true);
					rotateParamGUI.GetRow(3).Add("m_ease", 0.5f).Add("m_rotateMode", 0.5f);
					stateParamGUIs[1] = rotateParamGUI;

					var scaleParamGUI = new UIAnimationParamCombine(4, 2);
					scaleParamGUI.GetRow(0).Add("m_duration", 0.35f).Add("m_delay", 0.35f).
						Add("m_customFromTo", 0.3f, "", 0.9f);
					scaleParamGUI.GetRow(1).Add("m_scaleTo", 1).Condition("m_customFromTo", false);
					scaleParamGUI.GetRow(2).Add("m_scaleFrom", 0.5f).
						Add("m_scaleTo", 0.5f).Condition("m_customFromTo", true);
					scaleParamGUI.GetRow(3).Add("m_ease", 1);
					stateParamGUIs[2] = scaleParamGUI;

					var fadeParamGUI = new UIAnimationParamCombine(4, 3);
					fadeParamGUI.GetRow(0).Add("m_duration", 0.35f).Add("m_delay", 0.35f).
						Add("m_customFromTo", 0.3f, "", 0.9f);
					fadeParamGUI.GetRow(1).Add("m_fadeTo", 1).Condition("m_customFromTo", false);
					fadeParamGUI.GetRow(2).Add("m_fadeFrom", 0.5f).
						Add("m_fadeTo", 0.5f).Condition("m_customFromTo", true);
					fadeParamGUI.GetRow(3).Add("m_ease", 1);
					stateParamGUIs[3] = fadeParamGUI;
				}

				return stateParamGUIs;
			}
		}
	}
}