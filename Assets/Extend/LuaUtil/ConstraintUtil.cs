using UnityEngine;
using UnityEngine.Animations;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class ConstraintUtil {
		public static void SetConstraintActive(IConstraint constraint, bool active) {
			constraint.constraintActive = active;
		}

		public static void SetConstraintSource(IConstraint constraint, int index, Transform source, float weight = 1) {
			constraint.SetSource(index, new ConstraintSource() {
				sourceTransform = source,
				weight = weight
			});
		}
	}
}