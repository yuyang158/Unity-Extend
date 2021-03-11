using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public static class DoTweenUtil {
		public static Tweener DoPosition(Component c, Vector3 end, float duration, bool autoKill = false) {
			return c.transform.DOMove(end, duration).SetAutoKill(autoKill);
		}

		public static Tweener DoLocalPosition(Component c, Vector3 end, float duration, bool autoKill = false) {
			return c.transform.DOLocalMove(end, duration).SetAutoKill(autoKill);
		}

		public static Tweener DoArchoredPosition(Component c, Vector2 end, float duration, bool autoKill = false)
		{
			RectTransform t = c.transform as RectTransform;
			return t.DOAnchorPos(end, duration).SetAutoKill(autoKill);
		}

		public static Tweener DoGraphicFade(Graphic t, float end, float duration, float delay = 0, bool autoKill = false)
		{
			return t.DOFade(end, duration).SetAutoKill(autoKill).SetDelay(delay);
		}

		public static void KillTween(Tween t) {
			t.Kill();
		}

		public static void DoBezierPath(Transform t, Vector3 startPoint, System.Action onEnd,
			float endPointX,float endPointY,float endPointZ,
			float duration,float height,
			float pointPara
			)
		{
            //var startPoint = pos[0];
            var endPoint = new Vector3(endPointX, endPointY, endPointZ);
			//float duration = floatPara[3];
			//float height = floatPara[4];

			Vector3 forward = Vector3.Normalize(endPoint - startPoint);
			var bezierControlPoint = (startPoint + endPoint) * 0.5f + (Vector3.up * height);

			Vector3[] path = new Vector3[6];
			path[0] = bezierControlPoint;
			path[1] = startPoint + Vector3.Normalize(Vector3.up + forward);
			path[2] = bezierControlPoint - forward * pointPara;
			path[3] = endPoint;
			path[4] = bezierControlPoint + forward * pointPara;
			path[5] = endPoint - Vector3.Normalize(-Vector3.up + forward);
			t.position = startPoint;
			t.LookAt(path[1]);
			t.DOPath(path, duration, PathType.CubicBezier).OnComplete(()=>onEnd()).SetLookAt(0.01f);
		}

	}
}