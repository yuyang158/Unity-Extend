using System;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIAnimation))]
	public class UIAnimationPropertyDrawer : UIAnimationPropertyBaseDrawer {
		private static UIAnimationParamCombine[] punchParamGUIs;
		private static UIAnimationParamCombine[] stateParamGUIs;

		protected override UIAnimationParamCombine[] CurrentAnimation {
			get {
				if( punchParamGUIs == null ) {
					punchParamGUIs = new UIAnimationParamCombine[UIEditorUtil.PUNCH_ANIMATION_MODE.Length];
					for( var i = 0; i < UIEditorUtil.PUNCH_ANIMATION_MODE.Length; i++ ) {
						var punchParamGUI = new UIAnimationParamCombine(2, i);
						punchParamGUI.GetRow(0).Add("punch", 1, UIEditorUtil.PUNCH_ANIMATION_MODE[i]);
						punchParamGUI.GetRow(1)
							.Add("duration", 0.25f)
							.Add("vibrato", 0.25f)
							.Add("elasticity", 0.25f)
							.Add("delay", 0.25f);

						punchParamGUIs[i] = punchParamGUI;
					}
				
					stateParamGUIs = new UIAnimationParamCombine[UIEditorUtil.STATE_ANIMATION_MODE.Length];
					for( var i = 0; i < UIEditorUtil.STATE_ANIMATION_MODE.Length; i++ ) {
						var stateParamGUI = new UIAnimationParamCombine(3, i);
						var modeTypeName = UIEditorUtil.STATE_ANIMATION_MODE[i];
						stateParamGUI.GetRow(0).Add(modeTypeName.ToLower(), 1);
						stateParamGUI.GetRow(1)
							.Add("duration", 0.5f)
							.Add("delay", 0.5f);
						stateParamGUI.GetRow(2).Add("ease", 1);
						stateParamGUIs[i] = stateParamGUI;
					}
				}
				var modeProp = animationProperty.FindPropertyRelative("Mode");
				var mode = (UIAnimation.AnimationMode)modeProp.intValue;
				switch( mode ) {
					case UIAnimation.AnimationMode.ANIMATOR:
						break;
					case UIAnimation.AnimationMode.PUNCH:
						return punchParamGUIs;
					case UIAnimation.AnimationMode.STATE:
						return stateParamGUIs;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return null;
			}
		}

		protected override float SingleDoTweenHeight {
			get {
				var modeProp = animationProperty.FindPropertyRelative("Mode");
				var mode = (UIAnimation.AnimationMode)modeProp.intValue;
				switch( mode ) {
					case UIAnimation.AnimationMode.ANIMATOR:
						return 0;
					case UIAnimation.AnimationMode.PUNCH:
						return lineHeight * 2;
					case UIAnimation.AnimationMode.STATE:
						return lineHeight * 3;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		protected override string[] Mode {
			get {
				var modeProp = animationProperty.FindPropertyRelative("Mode");
				var mode = (UIAnimation.AnimationMode)modeProp.intValue;
				switch( mode ) {
					case UIAnimation.AnimationMode.ANIMATOR:
						return null;
					case UIAnimation.AnimationMode.PUNCH:
						return UIEditorUtil.PUNCH_ANIMATION_MODE;
					case UIAnimation.AnimationMode.STATE:
						return UIEditorUtil.STATE_ANIMATION_MODE;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		protected override string GetAnimationFieldName(int mode) {
			if( mode == 1 ) {
				return "punch";
			}
			else {
				return "state";
			}
		}
	}
}