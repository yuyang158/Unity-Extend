using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using Object = UnityEngine.Object;

namespace Extend.Common {
	public interface IService {
		int ServiceType { get; }
		void Initialize();
		void Destroy();
	}

	public interface IServiceUpdate {
		void Update();
	}

	public interface IServiceLateUpdate {
		void LateUpdate();
	}

	[CSharpCallLua]
	public delegate void LuaCommandDelegate(string[] param);

	public class CSharpServiceManager : MonoBehaviour {
		public enum ServiceType : byte {
			ERROR_LOG_TO_FILE,
			STAT,
			ASSET_FULL_STAT,
			ASSET_SERVICE,
			SCENE_LOAD,
			GAME_SYSTEM_SERVICE,
			RENDER_FEATURE,
			SPRITE_ASSET_SERVICE,
			TICK_SERVICE,
			COROUTINE_SERVICE,
			NETWORK_SERVICE,
			IN_GAME_CONSOLE,
			LUA_SERVICE,
			I18N,
			DEBUG_DRAW,
			DOWNLOAD,
			VERSION,
			SFX,
			COUNT,
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

		private static readonly IService[] m_services = new IService[64];
		private static readonly List<IServiceUpdate> m_updateableServices = new List<IServiceUpdate>();
		private static readonly List<IServiceLateUpdate> m_lateUpdateableServices = new List<IServiceLateUpdate>();

		public static void Register(IService service) {
			Assert.IsTrue(Initialized);
			if( m_services[service.ServiceType] != null ) {
				throw new Exception($"Service {service.ServiceType} exist.");
			}

			try {
				m_services[service.ServiceType] = service;
				service.Initialize();
				if( service is IServiceUpdate update ) {
					m_updateableServices.Add(update);
				}

				if( service is IServiceLateUpdate lateUpdate ) {
					m_lateUpdateableServices.Add(lateUpdate);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
		}

		public static void Unregister(int type) {
			Assert.IsTrue(Initialized);
			if( m_services[type] != null ) {
				var service = m_services[type];
				if( service is IServiceUpdate update ) {
					m_updateableServices.Remove(update);
				}

				service.Destroy();
				m_services[type] = null;
			}
		}

		public static T Get<T>(ServiceType typ) where T : class {
			return Get<T>((int) typ);
		}

		public static T Get<T>(int index) where T : class {
			Assert.IsTrue(Initialized);
			var service = m_services[index];
			if( service == null ) {
				Debug.LogError($"Service {index} not exist!");
			}

			return (T) service;
		}

		public static T TryGetAtIndex<T>(int index) where T : class {
			Assert.IsTrue(Initialized);
			var service = m_services[index];
			return (T) service;
		}

		private void Update() {
			foreach( var service in m_updateableServices ) {
				service.Update();
			}
		}

		private void LateUpdate() {
			foreach( var updateableService in m_lateUpdateableServices ) {
				updateableService.LateUpdate();
			}
		}

		private static void CleanUp() {
			Application.quitting -= CleanUp;
			m_updateableServices.Clear();
			m_lateUpdateableServices.Clear();
			for( int i = m_services.Length - 1; i >= 0; i-- ) {
				var service = m_services[i];
				if( service == null )
					continue;
				Unregister(service.ServiceType);
			}
			Initialized = false;
			Debug.LogWarning("Game Exit!");
		}

		public static void Shutdown() {
			CleanUp();
		}
	}
}