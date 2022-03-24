using System.Collections.Specialized;
using System.IO;
using System.Text;
using DG.Tweening;
using UnityEngine;

namespace Extend.Common {
#if UNITY_DEBUG
	public class StatService : IService, IServiceUpdate {
#else
	public class StatService : IService {
#endif
		public int ServiceType => (int)CSharpServiceManager.ServiceType.STAT;

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
			MVVM_DISPATCH,
			CULL_PROCESS,
			TOTAL_RENDERERS_TO_CULL,
			COUNT
		}

		private static readonly string[] STAT_DESCRIPTIONS = {
			"Tcp Received",
			"Tcp Sent",
			"Asset Bundle",
			"Asset",
			"In Used GO",
			"In Pool GO",
			"Active Tween Count",
			"Mvvm Change",
			"Static Cull Count",
			"Total Renderers To Cull"
		};

		private readonly long[] m_stats = new long[(int)StatName.COUNT];
		private TextWriter m_writer;

		public void Increase(StatName name, long value) {
			m_stats[(int)name] += value;
		}

		public void Set(StatName name, long value) {
			m_stats[(int)name] = value;
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
			DOTween.ClearCachedTweens();
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

#if UNITY_DEBUG
		public void Update() {
			var mvvmValueChangeCount = Get(StatName.MVVM_DISPATCH);
			if( mvvmValueChangeCount > 200 ) {
				Debug.LogWarning($"Current frame : {Time.frameCount} trigger mvvm value change : {mvvmValueChangeCount}");
			}

			Set(StatName.MVVM_DISPATCH, 0);
		}
#endif
	}
}