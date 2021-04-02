using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Extend.Common {
	public interface IService {
		CSharpServiceManager.ServiceType ServiceType { get; }
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
			COUNT
		}

		public static bool Initialized { get; private set; }
		public static CSharpServiceManager Instance { get; private set; }

		public static void Initialize() {
			if( Initialized ) {
				throw new Exception("CSharpServiceManager already initialized");
			}

			Initialized = true;
			var go = new GameObject("CSharpServiceManager");
			DontDestroyOnLoad(go);
			Instance = go.AddComponent<CSharpServiceManager>();

			Application.quitting += Clear;
		}

		private static readonly IService[] services = new IService[(int)ServiceType.COUNT];
		private static readonly List<IServiceUpdate> updateableServices = new List<IServiceUpdate>();

		public static void Register(IService service) {
			Assert.IsTrue(Initialized);
			if( services[(int)service.ServiceType] != null ) {
				throw new Exception($"Service {service.ServiceType} exist.");
			}

			try {
				services[(int)service.ServiceType] = service;
				service.Initialize();
				if( service is IServiceUpdate update ) {
					updateableServices.Add(update);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
		}

		public static void Unregister(ServiceType type) {
			Assert.IsTrue(Initialized);
			if( services[(int)type] != null ) {
				var service = services[(int)type];
				service.Destroy();
				services[(int)type] = null;
			}
		}

		public static T Get<T>(ServiceType typ) where T : class {
			Assert.IsTrue(Initialized);
			var service = services[(int)typ];
			if( service == null ) {
				Debug.LogError($"Service {typ} not exist!");
			}

			return (T)service;
		}

		private void Update() {
			foreach( var service in updateableServices ) {
				service.Update();
			}
		}

		private static void Clear() {
			updateableServices.Clear();
			for( int i = (int)ServiceType.COUNT - 1; i >= 0; i-- ) {
				Unregister((ServiceType)i);
			}
			Initialized = false;
		}

		private void OnDestroy() {
			Application.quitting -= Clear;
		}
	}
}