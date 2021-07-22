using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public static class DoTweenUtil {
		public static Tweener DoPosition(Component c, Vector3 end, float duration, bool autoKill = false, System.Action onEnd = null) {
			return c.transform.DOMove(end, duration).SetAutoKill(autoKill).OnComplete(() => onEnd?.Invoke());
		}

		public static Tweener DoLocalPosition(Component c, Vector3 end, float duration, bool autoKill = false, System.Action onEnd = null) {
			return c.transform.DOLocalMove(end, duration).SetAutoKill(autoKill).OnComplete(() => onEnd?.Invoke());
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

		public static Tweener DoSpriteRenderFade(SpriteRenderer t, float end, float duration, float delay = 0, bool autoKill = false)
		{
			return t.DOFade(end, duration).SetAutoKill(autoKill).SetDelay(delay);
		}

		public static void KillTween(Tween t) {
			t.Kill();
		}


		public static void DoPath(Transform t, Vector3 startPoint, System.Action onEnd,
			Vector3 endPoint, float duration, float delay)
		{
			Vector3[] path = new Vector3[2];
			path[0] = startPoint;
			path[1] = endPoint;
			t.LookAt(path[1]);
			t.DOPath(path, duration, PathType.Linear).SetDelay(delay).OnComplete(() => onEnd?.Invoke());
		}

		public static void DoLocalBezierPath(Transform t, Vector3 startPoint,
			Vector3 endPoint, float duration,float height, float pointPara, float delay)
		{
			Vector3 tempStartPoint = new Vector3(startPoint.x, startPoint.y, startPoint.z);
			Vector3 tempEndPoint = new Vector3(endPoint.x, endPoint.y, endPoint.z);
			//发射朝向
			Vector3 forward = Vector3.Normalize(tempEndPoint - tempStartPoint);

			//中心点的最高处
			var bezierControlPoint = (tempStartPoint + tempEndPoint) * 0.5f + (Vector3.up * height);
			if(bezierControlPoint.y < startPoint.y + 0.5f)
            {
				bezierControlPoint = new Vector3(bezierControlPoint.x, startPoint.y + 0.5f, bezierControlPoint.z);
			}

			Vector3[] path = new Vector3[6];
			//最高处 坐标点
			path[0] = bezierControlPoint;
			//最高处的反向朝向坐标
			path[2] = bezierControlPoint - forward * pointPara;
			//终点
			path[3] = endPoint;
			//最高处的正向朝向坐标
			path[4] = bezierControlPoint + forward * pointPara;

			//初始点的朝向坐标
			//path[1] = startPoint + Vector3.Normalize(Vector3.up + forward);
			path[1] = startPoint + Vector3.Normalize(path[2] - startPoint);
			//终点的反向朝向坐标
			//path[5] = endPoint - Vector3.Normalize(-Vector3.up + forward);
			path[5] = endPoint + Vector3.Normalize(path[4] - endPoint);
			//把对象挪到初始点
			t.localPosition = startPoint;
			t.DOLocalPath(path, duration, PathType.CubicBezier).SetDelay(delay).SetAutoKill(true);
		}

		public static void DoBezierPath(Transform t, Vector3 startPoint, System.Action onEnd,
			Vector3 endPoint, float duration,float height, float pointPara, float delay)
		{
			Vector3 tempStartPoint = new Vector3(startPoint.x, 0, startPoint.z);
			Vector3 tempEndPoint = new Vector3(endPoint.x, 0, endPoint.z);
			//发射朝向
			Vector3 forward = Vector3.Normalize(tempEndPoint - tempStartPoint);

			//中心点的最高处
			var bezierControlPoint = (tempStartPoint + tempEndPoint) * 0.5f + (Vector3.up * height);
			if(bezierControlPoint.y < startPoint.y + 0.5f)
            {
				bezierControlPoint = new Vector3(bezierControlPoint.x, startPoint.y + 0.5f, bezierControlPoint.z);
			}

			Vector3[] path = new Vector3[6];
			//最高处 坐标点
			path[0] = bezierControlPoint;
			//最高处的反向朝向坐标
			path[2] = bezierControlPoint - forward * pointPara;
			//终点
			path[3] = endPoint;
			//最高处的正向朝向坐标
			path[4] = bezierControlPoint + forward * pointPara;


			//初始点的朝向坐标
			//path[1] = startPoint + Vector3.Normalize(Vector3.up + forward);
			path[1] = startPoint + Vector3.Normalize(path[2] - startPoint);
			//终点的反向朝向坐标
			//path[5] = endPoint - Vector3.Normalize(-Vector3.up + forward);
			path[5] = endPoint + Vector3.Normalize(path[4] - endPoint);
			//把对象挪到初始点
			t.position = startPoint;
			t.LookAt(path[1]);
			t.DOPath(path, duration, PathType.CubicBezier).OnComplete(()=>onEnd?.Invoke()).SetDelay(delay).SetLookAt(0.01f);
		}
		public static void DoRotate(Transform t, float endValueX, float endValueY, float endValueZ, float duration)
		{
			t.DORotate(new Vector3(endValueX, endValueY, endValueZ), duration);
		}
		public static void DoLocalRotate(Transform transform, float endValueX, float endValueY, float endValueZ, 
			float duration,System.Action onEnd = null)
        {
			transform.DOLocalRotate(new Vector3(endValueX, endValueY, endValueZ), 
				duration, RotateMode.FastBeyond360).OnComplete(() => onEnd?.Invoke());
		}
		public static void DoScale(Transform transform, float endValue, float duration, System.Action onEnd = null)
		{
			transform.DOScale(endValue, duration).OnComplete(() => onEnd?.Invoke());
		}
		public static void DoLocalJump(Transform transform, float endValueX, float endValueY, float endValueZ,
			float jumpPower, int numJumps, float duration, bool snapping = false)
        {
			transform.DOLocalJump(new Vector3(endValueX, endValueY, endValueZ), jumpPower, numJumps, duration, snapping);
		}

		public static void DoImageFade(Image img, float value, float duration)
		{
			img.DOFade(value, duration);
		}
		public static void DoTextFade(Text text, float value, float duration)
		{
			text.DOFade(value, duration);
		}
		public static void DoGroupFade(CanvasGroup canvasGroup, float value, float duration)
		{
			canvasGroup.DOFade(value, duration);
		}
	}
}