using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI {
	[RequireComponent(typeof(Button))]
	public class UIButton : MonoBehaviour, IUIAnimationPreview {
		public UIAnimation Animation;
		
		public Tween[] CollectPreviewTween() {
			return Animation.Active(transform);
		}
	}
}