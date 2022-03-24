using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Extend.Common {
	public interface IService {
		int ServiceType { get; }
		void Initialize();
		void Destroy();
	}

	public interface IServiceUpdate {
		void Update();
	}

	[CSharpCallLua]
	public delegate void LuaCommandDelegate(string[] param);

	public class CSharpServiceManager : MonoBehaviour {
		public enum ServiceType : byte {
			ERROR_LOG_TO_FILE,
			STAT,
			ASSET_FULL_STAT,
			ASSET_SERVICE,
			GAME_SYSTEM_SERVICE,
			RENDER_FEATURE,
			SPRITE_ASSET_SERVICE,
			TICK_SERVICE,
			COROUTINE_SERVICE,
			NETWORK_SERVICE,
			IN_GAME_CONSOLE,
			LUA_SERVICE,
			I18N,
			SCENE_LOAD,
			DEBUG_DRAW,
			COUNT
		}

		public static bool Initialized { get; private set; }
		public static CSharpServiceManager Instance { get; private set; }

		public static void Initialize() {
			if( Initialized ) {
				throw new Exception("CSharpServiceManager already initialized");
			}

			Initialized = true;
			Application.quitting += CleanUp;
		}

		public static void InitializeServiceGameObject() {
			var go = new GameObject("CSharpServiceManager", typeof(UnityMainThreadDispatcher), typeof(CSharpServiceManager));
			DontDestroyOnLoad(go);
			Instance = go.GetComponent<CSharpServiceManager>();
		}

		private static readonly IService[] services = new IService[64];
		private static readonly List<IServiceUpdate> updateableServices = new();

		public static void Register(IService service) {
			Assert.IsTrue(Initialized);
			if( services[service.ServiceType] != null ) {
				throw new Exception($"Service {service.ServiceType} exist.");
			}

			try {
				services[service.ServiceType] = service;
				service.Initialize();
				if( service is IServiceUpdate update ) {
					updateableServices.Add(update);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
		}

		public static void Unregister(int type) {
			Assert.IsTrue(Initialized);
			if( services[type] != null ) {
				var service = services[type];
				if( service is IServiceUpdate update ) {
					updateableServices.Remove(update);
				}

				service.Destroy();
				services[type] = null;
			}
		}

		public static T Get<T>(ServiceType typ) where T : class {
			return Get<T>((int)typ);
		}
		
		public static T Get<T>(int index) where T : class {
			Assert.IsTrue(Initialized);
			var service = services[index];
			if( service == null ) {
				Debug.LogError($"Service {index} not exist!");
			}

			return (T)service;
		}
		
		public static T TryGetAtIndex<T>(int index) where T : class {
			Assert.IsTrue(Initialized);
			var service = services[index];
			return (T)service;
		}

		private void Update() {
			foreach( var service in updateableServices ) {
				service.Update();
			}
		}

		private static void CleanUp() {
			Application.quitting -= CleanUp;
			updateableServices.Clear();
			for( int i = services.Length - 1; i >= 0; i-- ) {
				var service = services[i];
				if( service == null )
					continue;
				Unregister(service.ServiceType);
			}

			Initialized = false;
			Debug.LogWarning("Game Exit!");
		}
	}
}