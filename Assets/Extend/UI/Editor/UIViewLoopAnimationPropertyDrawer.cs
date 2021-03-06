using Extend.Common.Editor;
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
					moveParamGUI.GetRow(0).Add("m_duration", 0.25f)
						.Add("m_delay", 0.25f)
						.Add("m_loops", 0.25f)
						.Add("m_loopType", 0.25f);
					moveParamGUI.GetRow(1).Add("m_moveBy", 1);
					moveParamGUI.GetRow(2).Add("m_ease", 1);
					stateParamGUIs[0] = moveParamGUI;

					var rotateParamGUI = new UIAnimationParamCombine(3, 1);
					rotateParamGUI.GetRow(0).Add("m_duration", 0.25f)
						.Add("m_delay", 0.25f)
						.Add("m_loops", 0.25f)
						.Add("m_loopType", 0.25f);
					rotateParamGUI.GetRow(1)
						.Add("m_rotateBy", 1);
					rotateParamGUI.GetRow(2)
						.Add("m_ease", 0.5f)
						.Add("m_rotateMode", 0.5f);
					stateParamGUIs[1] = rotateParamGUI;

					var scaleParamGUI = new UIAnimationParamCombine(3, 2);
					scaleParamGUI.GetRow(0).Add("m_duration", 0.25f)
						.Add("m_delay", 0.25f)
						.Add("m_loops", 0.25f)
						.Add("m_loopType", 0.25f);
					scaleParamGUI.GetRow(1).Add("m_scaleFrom", 0.5f)
						.Add("m_scaleTo", 0.5f);
					scaleParamGUI.GetRow(2).Add("m_ease", 1);
					stateParamGUIs[2] = scaleParamGUI;

					var fadeParamGUI = new UIAnimationParamCombine(3, 3);
					fadeParamGUI.GetRow(0).Add("m_duration", 0.25f)
						.Add("m_delay", 0.25f)
						.Add("m_loops", 0.25f)
						.Add("m_loopType", 0.25f);
					fadeParamGUI.GetRow(1)
						.Add("m_from", 0.5f)
						.Add("m_to", 0.5f);
					fadeParamGUI.GetRow(2)
						.Add("m_ease", 1);
					stateParamGUIs[3] = fadeParamGUI;
				}

				return stateParamGUIs;
			}
		}

		protected override float SingleDoTweenHeight => UIEditorUtil.LINE_HEIGHT * 3;
		protected override string[] Mode => UIEditorUtil.STATE_ANIMATION_MODE;
	}
}