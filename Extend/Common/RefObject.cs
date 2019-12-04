namespace Extend.Common {
	public abstract class RefObject {
		private int refCount;
		public void IncRef() {
			refCount++;
		}

		public int Release() {
			refCount--;
			if( refCount <= 0 ) {
				Destroy();
			}

			return refCount;
		}

		public abstract void Destroy();
	}
}