using System;
using System.Collections.Generic;
using Extend.DebugUtil;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.Common {
	public interface IService {
		CSharpServiceManager.ServiceType ServiceType { get; }
		void Initialize();
		void Destroy();
	}

	public interface IServiceUpdate {
		void Update();
	}


	public class CSharpServiceManager : MonoBehaviour {
		public enum ServiceType {
			ERROR_LOG_TO_FILE,
			STAT,
			ASSET_SERVICE,
			SPRITE_ASSET_SERVICE,
			TICK_SERVICE,
			COROUTINE_SERVICE,
			NETWORK_SERVICE,
			IN_GAME_CONSOLE,
			LUA_SERVICE,
			I18N,
		}

		private static bool Initialized { get; set; }

		public static void Initialize() {
			if( Initialized ) {
				throw new Exception("CSharpServiceManager already initialized");
			}

			Initialized = true;
			var go = new GameObject("CSharpServiceManager");
			DontDestroyOnLoad(go);
			go.AddComponent<CSharpServiceManager>();
			Register(go.AddComponent<InGameConsole>());

			Application.quitting += () => {
				var serviceList = new List<IService>(services.Values);
				serviceList.Sort((a, b) => -a.ServiceType.CompareTo(b.ServiceType));
				
				foreach( var service in serviceList ) {
					service.Destroy();
				}

				var luaService = services[ServiceType.LUA_SERVICE] as IDisposable;
				luaService.Dispose();
			};
		}

		private static readonly Dictionary<ServiceType, IService> services = new Dictionary<ServiceType, IService>();
		private static readonly List<IServiceUpdate> updateableServices = new List<IServiceUpdate>();

		public static void Register(IService service) {
			Assert.IsTrue(Initialized);
			if( services.ContainsKey(service.ServiceType) ) {
				throw new Exception($"Service {service.ServiceType} exist.");
			}

			services.Add(service.ServiceType, service);
			service.Initialize();
			if( service is IServiceUpdate update ) {
				updateableServices.Add(update);
			}
		}

		public static void Unregister(ServiceType type) {
			Assert.IsTrue(Initialized);
			if( services.ContainsKey(type) ) {
				var service = services[type];
				service.Destroy();
				services.Remove(type);
			}
			else {
				throw new Exception($"Service {type} not exist");
			}
		}

		public static T Get<T>(ServiceType typ) where T : IService {
			Assert.IsTrue(Initialized);
			if( !services.TryGetValue(typ, out var service) ) {
				Debug.LogError($"Service {typ} not exist!");
			}
			return (T)service;
		}

		private void Update() {
			foreach( var service in updateableServices ) {
				service.Update();
			}
		}
	}
}