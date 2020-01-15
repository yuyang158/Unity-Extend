using System.Collections.Generic;
using Extend.Common;

namespace Extend.Network.SocketClient {
	public class NetworkService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.NETWORK_SERVICE;
		private readonly List<AutoReconnectTcpClient> clients = new List<AutoReconnectTcpClient>();
		public void RegisterTcpClient(AutoReconnectTcpClient client) {
			clients.Add(client);
		}

		public void UnregisterTcpClient(AutoReconnectTcpClient client) {
			clients.Remove(client);
		}
		
		public void Initialize() {
			
		}

		public void Destroy() {
			
		}

		public void Update() {
			foreach( var client in clients ) {
				client.Update();
			}
		}
	}
}