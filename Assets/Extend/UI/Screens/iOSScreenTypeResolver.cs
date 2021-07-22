using System.Linq;

namespace Extend.UI.Screens {
	public static class iOSScreenTypeResolver {
		public static iOSScreen Resolve() {
			return iOSScreenType.Values.FirstOrDefault(t => t.IsCurrentScreen()) ?? iOSScreenType.Unknown();
		}
	}
}