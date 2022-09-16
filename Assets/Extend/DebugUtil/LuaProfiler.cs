using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace Extend.DebugUtil {
	public static class LuaProfiler {
		[Conditional("UNITY_EDITOR")]
		public static void BeginSample(int id) {
			m_showNames.TryGetValue(id, out var name);
			name = name ?? string.Empty;

			Profiler.BeginSample(name);
			++m_sampleDepth;
		}

		[Conditional("UNITY_EDITOR")]
		public static void BeginSample(int id, string name) {
			name = name ?? string.Empty;
			m_showNames[id] = name;

			Profiler.BeginSample(name);
			++m_sampleDepth;
		}

		[Conditional("UNITY_EDITOR")]
		internal static void BeginSample(string name) {
			name = name ?? string.Empty;
			Profiler.BeginSample(name);
			++m_sampleDepth;
		}

		[Conditional("UNITY_EDITOR")]
		public static void EndSample() {
			if( m_sampleDepth > 0 ) {
				--m_sampleDepth;
				Profiler.EndSample();
			}
		}

		private static int m_sampleDepth;

		// private const int  _maxSampleDepth = 100;
		private static readonly Dictionary<int, string> m_showNames = new Dictionary<int, string>();
	}
}