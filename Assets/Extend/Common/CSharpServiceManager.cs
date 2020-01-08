using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = System.Object;

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
			ASSET_SERVICE,
			SPRITE_ASSET_SERVICE,
			MVVM_SERVICE,
			TICK_SERVICE,
			COROUTINE_SERVICE
		}

		public static bool Initialized { get; private set; }

		public static void Initialize() {
			if( Initialized ) {
				throw new Exception( "CSharpServiceManager already exist" );
			}

			Initialized = true;
			var go = new GameObject( "CSharpServiceManager" );
			DontDestroyOnLoad(go);
			go.AddComponent<CSharpServiceManager>();
		}

		private static readonly Dictionary<ServiceType, IService> services = new Dictionary<ServiceType, IService>();
		private static readonly List<IServiceUpdate> updatableServices = new List<IServiceUpdate>();

		public static void Register(IService service) {
			Assert.IsTrue(Initialized);
			if( services.ContainsKey( service.ServiceType ) ) {
				throw new Exception( $"Service {service.ServiceType} exist." );
			}

			services.Add( service.ServiceType, service );
			service.Initialize();
			if( service is IServiceUpdate update ) {
				updatableServices.Add( update );
			}
		}

		public static void Unregister(ServiceType type) {
			Assert.IsTrue(Initialized);
			if( services.ContainsKey( type ) ) {
				var service = services[type];
				service.Destroy();
				services.Remove( type );
			}
			else {
				throw  new Exception($"Service {type} not exist");
			}
		}

		public static T Get<T>( ServiceType typ ) where T : IService {
			Assert.IsTrue(Initialized);
			return (T)services[typ];
		}

		private void Update() {
			foreach( var service in updatableServices ) {
				service.Update();
			}
		}
	}
}