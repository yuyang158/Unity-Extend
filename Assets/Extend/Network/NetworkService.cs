using System.Collections.Generic;
using Extend.Common;
using Extend.Network.SocketClient;

namespace Extend.Network {
	public class NetworkService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.NETWORK_SERVICE;
		private readonly List<AutoReconnectTcpClient> m_clients = new List<AutoReconnectTcpClient>();

		public void RegisterTcpClient(AutoReconnectTcpClient client) {
			m_clients.Add(client);
		}

		public void UnregisterTcpClient(AutoReconnectTcpClient client) {
			m_clients.Remove(client);
		}

		public void Initialize() {
		}

		public void Destroy() {
			while( m_clients.Count > 0 ) {
				var client = m_clients[0];
				client.Destroy();
			}
		}

		public void Update() {
			foreach( var client in m_clients ) {
				client.Update();
			}
		}
	}
}