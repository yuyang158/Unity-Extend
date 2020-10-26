using System.Collections.Specialized;
using System.IO;
using System.Text;
using DG.Tweening;
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
			ACTIVE_TWEEN,
			COUNT
		}

		private static readonly string[] STAT_DESCRIPTIONS = {
			"Tcp Received",
			"Tcp Sent",
			"Asset Bundle",
			"Asset",
			"In Used GO",
			"In Pool GO",
			"Active Tween Count"
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

		public void LogStat(string category, string key, object value) {
			if( m_writer == null )
				return;

			var line = string.Join("\t", Time.frameCount, category, key, value);
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
			Set(StatName.ACTIVE_TWEEN, DOTween.TotalPlayingTweens());
			for( var i = 0; i < (int)StatName.COUNT; i++ ) {
				builder.AppendLine($"{STAT_DESCRIPTIONS[i]} : {m_stats[i].ToString()}");
			}
		}
		
		public static void Upload() {
			var releaseType = IniRead.SystemSetting.GetString("GAME", "Mode");
			var qs = new NameValueCollection {
				{"device", SystemInfo.deviceName}, {"os", SystemInfo.operatingSystem}, {"version", Application.version}, {"other", releaseType}
			};
			var url = IniRead.SystemSetting.GetString("DEBUG", "LogUploadUrl");
			Utility.HttpFileUpload(url, qs, Application.persistentDataPath + "/stat.log");
		}
	}
}