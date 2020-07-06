using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor.Process {
	public interface IBuildAssetProcess {
		Type ProcessType { get; }

		void Process(AssetImporter importer, TextWriter writer);

		void PostProcess();
	}

	public static class AssetCustomProcesses {
		private static readonly Dictionary<Type, List<IBuildAssetProcess>> extensionProcessesMap = new Dictionary<Type, List<IBuildAssetProcess>>();
		private static TextWriter m_processWriter;

		public static void Init() {
			m_processWriter = new StreamWriter($"{Application.dataPath}/../asset_build_{DateTime.Now.ToLongTimeString().Replace(':', '_')}.txt");
		}

		public static void RegisterProcess(IBuildAssetProcess process) {
			if( !extensionProcessesMap.TryGetValue(process.ProcessType, out var processes) ) {
				processes = new List<IBuildAssetProcess>();
				extensionProcessesMap.Add(process.ProcessType, processes);
			}
			processes.Add(process);
		}

		public static void Process(AssetImporter importer) {
			if( !extensionProcessesMap.TryGetValue(importer.GetType(), out var processes) ) {
				return;
			}

			foreach( var process in processes ) {
				process.Process(importer, m_processWriter);
			}
		}

		public static void PostProcess() {
			m_processWriter.Close();
			foreach( var process in extensionProcessesMap.Values.SelectMany(processes => processes) ) {
				process.PostProcess();
			}
		}

		public static void Shutdown() {
			extensionProcessesMap.Clear();
		}
	}
}