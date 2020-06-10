using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Extend.Common {
	public class StatService : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.STAT;

		public static StatService Get() {
			return CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
		}
		
		public enum StatName {
			TCP_RECEIVED,
			TCP_SENT,
			ASSET_BUNDLE_COUNT,
			ASSET_COUNT,
			IN_USE_GO,
			IN_POOL_GO,
			COUNT
		}

		private static readonly string[] STAT_DESCRIPTIONS = new[] {
			"Tcp Received",
			"Tcp Sent",
			"Asset Bundle",
			"Asset",
			"In Used GO",
			"In Pool GO"
		};

		private readonly long[] m_stats = new long[(int)StatName.COUNT];
		private TextWriter m_writer;

		public void Increase(StatName name, long value) {
			m_stats[(int)name] += value;
		}

		public void Set(StatName name, long value) {
			m_stats[(int)name] += value;
		}

		public long Get(StatName name) {
			return m_stats[(int)name];
		}

		public void LogStat(string type, string key, object value) {
			var line = string.Join("\t", type, key, value);
			m_writer.WriteLine(line);
		}
		
		public void Initialize() {
			var statFilePath = Application.persistentDataPath + "/stat.log";
			m_writer = new StreamWriter(statFilePath, false); 
		}

		public void Destroy() {
			for( var i = 0; i < (int)StatName.COUNT; i++ ) {
				var type = (StatName)i;
				LogStat("Stat", type.ToString(), m_stats[i].ToString());
			}
			m_writer.Close();
		}

		public void Output(StringBuilder builder) {
			for( var i = 0; i < (int)StatName.COUNT; i++ ) {
				builder.AppendLine($"{STAT_DESCRIPTIONS[i]} : {m_stats[i].ToString()}");
			}
		}
	}
}