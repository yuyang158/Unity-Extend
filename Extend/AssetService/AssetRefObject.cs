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

		private AssetStatus status = AssetStatus.NONE;
		public AssetStatus Status {
			get => status;
			set {
				status = value;
				OnStatusChanged?.Invoke(Status, this);
			}
		}

		public event Action<AssetStatus, AssetRefObject> OnStatusChanged;

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