using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace Extend.DebugUtil {
	public static class LuaProfiler {
		[Conditional("UNITY_EDITOR")]
		public static void BeginSample(int id) {
			_showNames.TryGetValue(id, out var name);
			name ??= string.Empty;

			Profiler.BeginSample(name);
			++_sampleDepth;
		}

		[Conditional("UNITY_EDITOR")]
		public static void BeginSample(int id, string name) {
			name ??= string.Empty;
			_showNames[id] = name;

			Profiler.BeginSample(name);
			++_sampleDepth;
		}

		[Conditional("UNITY_EDITOR")]
		internal static void BeginSample(string name) {
			name ??= string.Empty;
			Profiler.BeginSample(name);
			++_sampleDepth;
		}

		[Conditional("UNITY_EDITOR")]
		public static void EndSample() {
			if( _sampleDepth > 0 ) {
				--_sampleDepth;
				Profiler.EndSample();
			}
		}

		private static int _sampleDepth;

		// private const int  _maxSampleDepth = 100;
		private static readonly Dictionary<int, string> _showNames = new Dictionary<int, string>();
	}
}