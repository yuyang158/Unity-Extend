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
			m_processWriter = new StreamWriter($"{Application.dataPath}/../asset_build_warning_report.txt");
			var typeCollection = TypeCache.GetTypesDerivedFrom<IBuildAssetProcess>();
			foreach( var type in typeCollection ) {
				RegisterProcess(Activator.CreateInstance(type) as IBuildAssetProcess);
			}
			var files = Directory.GetFiles("Assets/Shaders", "*.shadervariants", SearchOption.AllDirectories);
			foreach( var file in files ) {
				File.Delete(file);
			}
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
			foreach( var process in extensionProcessesMap.Values.SelectMany(processes => processes) ) {
				process.PostProcess();
			}
		}

		public static void Shutdown() {
			m_processWriter.Close();
			extensionProcessesMap.Clear();
		}
	}
}