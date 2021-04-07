using DG.Tweening;
using UnityEngine;

namespace Extend.UI {
	public interface IUIAnimationPreview {
		void CacheStartValue(Transform transform);

		Tween[] CollectPreviewTween(Transform transform);

		void Editor_Recovery(Transform transform);
	}
}