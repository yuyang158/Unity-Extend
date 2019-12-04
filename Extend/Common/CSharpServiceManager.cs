using System;
using System.Collections.Generic;
using UnityEngine;

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
			MVVM_SERVICE,
			TICK_SERVICE
		}

		private static bool initialized;

		public static void Initialize() {
			if( initialized ) {
				throw new Exception( "CSharpServiceManager already exist" );
			}

			initialized = true;
			var go = new GameObject( "CSharpServiceManager" );
			go.AddComponent<CSharpServiceManager>();
		}

		private static readonly Dictionary<ServiceType, IService> services = new Dictionary<ServiceType, IService>();
		private static readonly List<IServiceUpdate> updatableServices = new List<IServiceUpdate>();

		public static void Register(IService service) {
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
			return (T)services[typ];
		}

		private void Update() {
			foreach( var service in updatableServices ) {
				service.Update();
			}
		}
	}
}