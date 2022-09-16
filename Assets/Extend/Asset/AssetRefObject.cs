using System;
using UnityEngine;

namespace Extend.Asset {
	public abstract class AssetRefObject {
		public enum AssetStatus : byte {
			NONE,
			ASYNC_LOADING,
			DONE,
			FAIL,
			DESTROYED
		}

		protected string m_debugNameCache;
		public string DebugNameCache => m_debugNameCache;

		private AssetStatus m_status = AssetStatus.NONE;
		public AssetStatus Status {
			get => m_status;
			set {
				m_status = value;
				OnStatusChanged?.Invoke(this);
			}
		}

		public bool IsFinished => m_status == AssetStatus.DONE || m_status == AssetStatus.FAIL;

		public event Action<AssetRefObject> OnStatusChanged;

		public float ZeroRefTimeStart {
			get;
			private set;
		}

		private int m_refCount;
		public void IncRef() {
			m_refCount++;
		}

		public int Release() {
			m_refCount--;
			if( m_refCount <= 0 ) {
				ZeroRefTimeStart = Time.time;
			}
			return m_refCount;
		}

		public int GetRefCount() {
			return m_refCount;
		}

		public abstract void Destroy();
	}
}