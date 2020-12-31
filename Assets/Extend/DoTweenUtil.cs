using DG.Tweening;
using UnityEngine;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public static class DoTweenUtil {
		public static Tweener DoPosition(Component c, Vector3 end, float duration, bool autoKill = false) {
			return c.transform.DOMove(end, duration).SetAutoKill(autoKill);
		}

		public static void KillTween(Tween t) {
			t.Kill();
		}
	}
}