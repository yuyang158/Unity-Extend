using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Extend.Asset.Editor.Process {
	public interface IBuildAssetProcess {
		string ProcessType { get; }

		void Process(AssetImporter importer);

		void PostProcess();
	}

	public static class AssetCustomProcesses {
		private static Dictionary<string, List<IBuildAssetProcess>> extensionProcessesMap = new Dictionary<string, List<IBuildAssetProcess>>();
		public static void Init() {
			var processInterface = typeof(IBuildAssetProcess);
			var types = processInterface.Assembly.GetTypes();
			foreach( var type in types ) {
				if( !type.IsSubclassOf(processInterface) ) {
					continue;
				}

				var process = Activator.CreateInstance(type) as IBuildAssetProcess;
				if( !extensionProcessesMap.TryGetValue(process.ProcessType, out var processes) ) {
					processes = new List<IBuildAssetProcess>();
					extensionProcessesMap.Add(process.ProcessType, processes);
				}
				
				processes.Add(process);
			}
		}

		public static void Process(AssetImporter importer) {
			var extension = Path.GetExtension(importer.assetPath);
			if( !extensionProcessesMap.TryGetValue(extension, out var processes) ) {
				return;
			}

			foreach( var process in processes ) {
				process.Process(importer);
			}
		}

		public static void PostProcess() {
			foreach( var process in extensionProcessesMap.Values.SelectMany(processes => processes) ) {
				process.PostProcess();
			}
		}

		public static void Shutdown() {
			extensionProcessesMap.Clear();
		}
	}
}