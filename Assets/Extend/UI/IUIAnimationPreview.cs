using DG.Tweening;
using UnityEngine;

namespace Extend.UI {
	public interface IUIAnimationPreview {
		void Cache(Transform transform);

		Tween[] CollectPreviewTween(Transform transform);
	}
}