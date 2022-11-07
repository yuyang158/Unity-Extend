using SimpleLogger;
using System.Net;
using System.Net.Sockets;

namespace Server {
	public class DeviceRegisterTcpServer {
		private readonly TcpListener listener;
		private const int TCP_PORT = 5301;

		public DeviceRegisterTcpServer() {
			listener = new TcpListener(IPAddress.Parse("0.0.0.0"), TCP_PORT);
			listener.Start();
			Logger.Log(Logger.Level.Info, $"TCP Server started : {TCP_PORT}");

			var listenThread = new Thread(StartAccept);
			listenThread.Start();

			var pingThread = new Thread(StartPing);
			pingThread.Start();
		}

		private void StartAccept() {
			try {
				while( true ) {
					var deviceClient = listener.AcceptTcpClient();
					var device = new DeviceTcpClient(deviceClient);
					lock( Router.Devices ) {
						Router.Devices.Add(device);	
					}
				}
			}
			catch( Exception e ) {
				Logger.Log(Logger.Level.Info, e.Message);
			}
		}

		private void StartPing() {
			while( true ) {
				try {
					lock( Router.Devices ) {
						for (int i = 0; i < Router.Devices.Count; i++) {
							var device = Router.Devices[i];
							device.Ping();
						}
					}
				}
				catch(Exception e) {
					Logger.Log(e);
				}
				Thread.Sleep(1000 * 30);
			}
		}
	}
}