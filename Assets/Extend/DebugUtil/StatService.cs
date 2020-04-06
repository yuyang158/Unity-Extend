using System.IO;
using Extend.Common;
using UnityEngine;

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
		private TextWriter writer;

		public void Increase(StatName name, long value) {
			stats[(int)name] += value;
		}

		public void Set(StatName name, long value) {
			stats[(int)name] += value;
		}

		public void LogStat(string type, string key, object value) {
			var line = string.Join("\t", type, key, value);
			writer.WriteLineAsync(line);
		}
		
		public void Initialize() {
			var statFilePath = Application.persistentDataPath + "/stat.log";
			writer = new StreamWriter(statFilePath, false); 
		}

		public void Destroy() {
			for( var i = 0; i < (int)StatName.COUNT; i++ ) {
				var type = (StatName)i;
				LogStat("Stat", type.ToString(), stats[i]);
			}
			writer.Close();
		}
	}
}