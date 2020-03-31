using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server {
	public class DeviceRegisterTcpServer {
		private readonly TcpListener listener;
		private const int TCP_PORT = 4101;

		public DeviceRegisterTcpServer() {
			listener = new TcpListener(IPAddress.Parse("0.0.0.0"), TCP_PORT);
			listener.Start();
			Console.WriteLine($"TCP Server started : {TCP_PORT}");

			var listenThread = new Thread(StartAccept);
			listenThread.Start();
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
				Console.WriteLine(e);
			}
		}
	}
}