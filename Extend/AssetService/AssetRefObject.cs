using System;
using UnityEngine;

namespace Extend.AssetService {
	public abstract class AssetRefObject {
		public enum AssetStatus {
			NONE,
			ASYNC_LOADING,
			DONE,
			FAIL
		}

		private AssetStatus status = AssetStatus.DONE;
		public AssetStatus Status {
			get => status;
			protected set {
				status = value;
				if( status == AssetStatus.FAIL ) {
					throw new Exception($"Load asset fail : {ToString()}");
				}
			}
		}

		public int ContainerLocation;

		public float ZeroRefTimeStart {
			get;
			private set;
		}

		private int refCount;
		public void IncRef() {
			refCount++;
		}

		public int Release() {
			refCount--;
			if( refCount <= 0 ) {
				ZeroRefTimeStart = Time.time;
			}
			return refCount;
		}

		public int GetRefCount() {
			return refCount;
		}

		public abstract void Destroy();
	}
}