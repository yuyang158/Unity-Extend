using UnityEngine;
using UnityEngine.AI;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class NavMeshUtil {
		public static Vector3 Raycast(Vector3 start, Vector3 end, int areaMask) {
			return NavMesh.Raycast(start, end, out var hit, areaMask) ? hit.position : end;
		}

		private static readonly Quaternion[] m_directionRotations = {
			Quaternion.identity,
			Quaternion.Euler(0, 15, 0),
			Quaternion.Euler(0, -15, 0),
			Quaternion.Euler(0, 30, 0),
			Quaternion.Euler(0, -30, 0)
		};

		public static bool CalculateValidDirection(Vector3 start, Vector3 end, int areaMask, out Vector3 result) {
			var vOffset = end - start;
			for( int i = 0; i < 5; i++ ) {
				var vDirection = m_directionRotations[i] * vOffset;
				var tmpEnd = start + vDirection;
				var success = NavMesh.Raycast(start, tmpEnd, out var hit, areaMask);
				if( !success ) {
					result = vDirection;
					return true;
				}
			}

			result = Vector3.zero;
			return false;
		}

		public static Vector3 CalculateRandomPointOnRing(Vector3 ringCenter, float innerRadius, float outerRadius) {
			Vector3 randomPoint = Vector3.zero;
			bool foundValidPoint = false;

			while( !foundValidPoint ) {
				// Generate random angle
				float angle = Random.Range(0f, 360f);

				// Convert angle to radians
				float radians = angle * Mathf.Deg2Rad;

				// Generate random radius between inner and outer radius
				float radius = Random.Range(innerRadius, outerRadius);

				// Calculate cartesian coordinates
				float x = ringCenter.x + radius * Mathf.Cos(radians);
				float z = ringCenter.z + radius * Mathf.Sin(radians);

				randomPoint = new Vector3(x, ringCenter.y, z);

				// Check if the random point is on the NavMesh
				if( NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, 1) ) {
					randomPoint = hit.position;
					foundValidPoint = true;
				}
			}

			return randomPoint;
		}
	}
}