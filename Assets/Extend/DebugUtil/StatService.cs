using Extend.Common;

namespace Extend.DebugUtil {
	public class StatService : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.STAT;

		public static StatService Get() {
			return CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
		}
		
		public enum StatName {
			TCP_RECEIVED,
			TCP_SENT,
			COUNT
		}
		
		private readonly long[] stats = new long[(int)StatName.COUNT];

		public void Increase(StatName name, long value) {
			stats[(int)name] += value;
		}

		public void Set(StatName name, long value) {
			stats[(int)name] += value;
		}
		
		public void Initialize() {
			
		}

		public void Destroy() {
			
		}
	}
}