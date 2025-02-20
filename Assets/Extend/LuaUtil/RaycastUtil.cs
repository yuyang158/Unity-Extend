using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class RaycastUtil {
		private static readonly RaycastHit[] _hits = new RaycastHit[16];
		private static readonly Collider[] _colliders = new Collider[16];

		public static Vector3 GetPlanePoint(Ray mouseRay, Vector3 source) {
			var plane = new Plane(Vector3.up, source);
			plane.Raycast(mouseRay, out var enter);
			return mouseRay.GetPoint(enter);
		}

		public static int RaycastWithPlane(Ray mouseRay, Vector3 source, float maxDistance, int layerMask) {
			var plane = new Plane(Vector3.up, source);
			plane.Raycast(mouseRay, out var enter);
			var point = mouseRay.GetPoint(enter);
			return Physics.SphereCastNonAlloc(source, 1, ( point - source ).normalized, _hits, maxDistance, layerMask);
		}

		public static bool Raycast(Ray ray, float maxDistance, int layerMask, out RaycastHit hit) {
			return Physics.Raycast(ray, out hit, maxDistance, layerMask, QueryTriggerInteraction.Collide);
		}

		public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask,
			out RaycastHit hit) {
			return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);
		}

		public static int RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask) {
			return Physics.RaycastNonAlloc(origin, direction, _hits, maxDistance, layerMask);
		}

		public static int CapsuleCastAll(Vector3 bottom, Vector3 top, float radius, Vector3 direction,
			float maxDistance, int layerMask) {
			return Physics.CapsuleCastNonAlloc(bottom, top, radius, direction, _hits, maxDistance, layerMask);
		}

		public static LuaTable GetResult(int count) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var ret = luaVm.NewTable();

			for( int i = 0; i < count; i++ ) {
				ret.Set(i + 1, _hits[i]);
			}

			return ret;
		}

		public static RaycastHit[] Hits => _hits;
		public static Collider[] Overlaps => _colliders;

		public static int ColliderCastNonAlloc(Collider collider, Vector3 direction, float maxDistance, int layerMask) {
			if( collider is BoxCollider boxCollider ) {
				return BoxCastNonAlloc(boxCollider, direction, maxDistance, layerMask);
			}
			if( collider is CapsuleCollider capsuleCollider ) {
				return CapsuleCastNonAlloc(capsuleCollider, direction, maxDistance, layerMask);
			}
			if( collider is SphereCollider sphereCollider ) {
				return SphereCastNonAlloc(sphereCollider, direction, maxDistance, layerMask);
			}
			throw new NotImplementedException();
		}
		//
		// Box
		//

		public static bool BoxCast(BoxCollider box, Vector3 direction, float maxDistance = Mathf.Infinity,
			int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
			return Physics.BoxCast(center, halfExtents, direction, orientation, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static bool BoxCast(BoxCollider box, Vector3 direction, out RaycastHit hitInfo,
			float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
			return Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static int BoxCastNonAlloc(BoxCollider box, Vector3 direction,
			float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
			return Physics.BoxCastNonAlloc(center, halfExtents, direction, _hits, orientation, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static bool CheckBox(BoxCollider box, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
			return Physics.CheckBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
		}

		public static int OverlapBoxNonAlloc(BoxCollider box, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
			return Physics.OverlapBoxNonAlloc(center, halfExtents, _colliders, orientation, layerMask,
				queryTriggerInteraction);
		}

		public static void ToWorldSpaceBox(this BoxCollider box, out Vector3 center, out Vector3 halfExtents,
			out Quaternion orientation) {
			orientation = box.transform.rotation;
			center = box.transform.TransformPoint(box.center);
			var lossyScale = box.transform.lossyScale;
			var scale = AbsVec3(lossyScale);
			halfExtents = Vector3.Scale(scale, box.size) * 0.5f;
		}

		//
		// Sphere
		//

		public static bool SphereCast(SphereCollider sphere, Vector3 direction, out RaycastHit hitInfo,
			float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			sphere.ToWorldSpaceSphere(out Vector3 center, out var radius);
			return Physics.SphereCast(center, radius, direction, out hitInfo, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static int SphereCastNonAlloc(SphereCollider sphere, Vector3 direction,
			float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			sphere.ToWorldSpaceSphere(out Vector3 center, out var radius);
			return Physics.SphereCastNonAlloc(center, radius, direction, _hits, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static bool CheckSphere(SphereCollider sphere, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			sphere.ToWorldSpaceSphere(out Vector3 center, out var radius);
			return Physics.CheckSphere(center, radius, layerMask, queryTriggerInteraction);
		}

		public static int OverlapSphereNonAlloc(SphereCollider sphere, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			sphere.ToWorldSpaceSphere(out Vector3 center, out var radius);
			return Physics.OverlapSphereNonAlloc(center, radius, _colliders, layerMask, queryTriggerInteraction);
		}

		public static void ToWorldSpaceSphere(this SphereCollider sphere, out Vector3 center, out float radius) {
			center = sphere.transform.TransformPoint(sphere.center);
			radius = sphere.radius * MaxVec3(AbsVec3(sphere.transform.lossyScale));
		}

		//
		// Capsule
		//

		public static bool CapsuleCast(CapsuleCollider capsule, Vector3 direction, out RaycastHit hitInfo,
			float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out var radius);
			return Physics.CapsuleCast(point0, point1, radius, direction, out hitInfo, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static int CapsuleCastNonAlloc(CapsuleCollider capsule, Vector3 direction,
			float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out var radius);
			return Physics.CapsuleCastNonAlloc(point0, point1, radius, direction, _hits, maxDistance, layerMask,
				queryTriggerInteraction);
		}

		public static bool CheckCapsule(CapsuleCollider capsule, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out var radius);
			return Physics.CheckCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
		}

		public static int OverlapCapsuleNonAlloc(CapsuleCollider capsule, int layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
			capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out var radius);
			return Physics.OverlapCapsuleNonAlloc(point0, point1, radius, _colliders, layerMask, queryTriggerInteraction);
		}

		public static void ToWorldSpaceCapsule(this CapsuleCollider capsule, out Vector3 point0, out Vector3 point1,
			out float radius) {
			var center = capsule.transform.TransformPoint(capsule.center);
			radius = 0f;
			float height = 0f;
			Vector3 lossyScale = AbsVec3(capsule.transform.lossyScale);
			Vector3 dir = Vector3.zero;

			switch( capsule.direction ) {
				case 0: // x
					radius = Mathf.Max(lossyScale.y, lossyScale.z) * capsule.radius;
					height = lossyScale.x * capsule.height;
					dir = capsule.transform.TransformDirection(Vector3.right);
					break;
				case 1: // y
					radius = Mathf.Max(lossyScale.x, lossyScale.z) * capsule.radius;
					height = lossyScale.y * capsule.height;
					dir = capsule.transform.TransformDirection(Vector3.up);
					break;
				case 2: // z
					radius = Mathf.Max(lossyScale.x, lossyScale.y) * capsule.radius;
					height = lossyScale.z * capsule.height;
					dir = capsule.transform.TransformDirection(Vector3.forward);
					break;
			}

			if( height < radius * 2f ) {
				dir = Vector3.zero;
			}

			point0 = center + dir * ( height * 0.5f - radius );
			point1 = center - dir * ( height * 0.5f - radius );
		}

		//  
		// Util
		//

		public static void SortClosestToFurthest(RaycastHit[] hits, int hitCount = -1) {
			if( hitCount == 0 ) {
				return;
			}

			if( hitCount < 0 ) {
				hitCount = hits.Length;
			}

			Array.Sort<RaycastHit>(hits, 0, hitCount, ascendDistance);
		}

		//
		// Private 
		//

		private class AscendingDistanceComparer : IComparer<RaycastHit> {
			public int Compare(RaycastHit h1, RaycastHit h2) {
				return h1.distance < h2.distance ? -1 : ( h1.distance > h2.distance ? 1 : 0 );
			}
		}

		private static AscendingDistanceComparer ascendDistance = new AscendingDistanceComparer();

		private static Vector3 AbsVec3(Vector3 v) {
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		private static float MaxVec3(Vector3 v) {
			return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
		}
	}
}