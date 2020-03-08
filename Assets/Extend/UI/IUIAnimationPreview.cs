using DG.Tweening;
using UnityEngine;

namespace Extend.UI {
	public interface IUIAnimationPreview {
		Tween[] CollectPreviewTween( Transform transform );
	}
}