using System;

namespace Server {
	internal static class Program {
		private static void Main(string[] args) {
			var httpServer = new UserHttpServer();
			Router.Register(httpServer);

			var deviceServer = new DeviceRegisterTcpServer();

			Console.ReadLine();
		}
	}
}