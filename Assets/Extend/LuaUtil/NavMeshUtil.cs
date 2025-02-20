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

		public static Vector3 RaycastDirection(Vector3 start, Vector3 end, int areaMask) {
			if( NavMesh.Raycast(start, end, out var hit, areaMask) ) {
				return hit.position;
			}

			return end;
		}

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

		public static Vector3 CalculateRandomPointOnRing(Vector3 ringCenter, float innerRadius, float outerRadius,
			float angleMin = 0, float angleMax = 360) {
			Vector3 randomPoint = ringCenter;
			bool foundValidPoint = false;

			// Generate random angle
			float angle = Random.Range(angleMin, angleMax);
			// Generate random radius between inner and outer radius
			float radius = Random.Range(innerRadius, outerRadius);
			var tryTimes = 0;
			while( !foundValidPoint ) {
				// Convert angle to radians
				float radians = angle * Mathf.Deg2Rad;

				// Calculate cartesian coordinates
				float x = ringCenter.x + radius * Mathf.Cos(radians);
				float z = ringCenter.z + radius * Mathf.Sin(radians);

				randomPoint = new Vector3(x, ringCenter.y, z);
				NavMesh.Raycast(ringCenter, randomPoint, out var hit, 1);
				if( hit.distance > radius * 0.75f ) {
					foundValidPoint = true;
					randomPoint = hit.position;
				}

				tryTimes++;
				if( tryTimes > 10 ) {
					foundValidPoint = true;
					randomPoint = ringCenter;
				}

				angle += 30;
			}

			return randomPoint;
		}
		
		public static Vector3 CalculatePointOnRingCameraForward(Vector3 ringCenter, float innerRadius, float outerRadius) {
			float radius = Random.Range(innerRadius, outerRadius);
			var cameraForward = Camera.main.transform.forward;
			cameraForward.y = 0;
			cameraForward.Normalize();
			for( int i = 0; i < 7; i++ ) {
				float angle = i * 30;
				Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
				var offset = q * cameraForward;
				
				NavMesh.Raycast(ringCenter, ringCenter + offset, out var hit, 1);
				if( hit.distance > radius * 0.75f ) {
					return hit.position;
				}
			}
			return ringCenter;
		}
	}
}