using SimpleLogger;
using SimpleLogger.Logging.Handlers;
using System;

namespace Server {
	internal static class Program {
		private static void Main(string[] args) {
			Logger.LoggerHandlerManager
				.AddHandler(new ConsoleLoggerHandler())
				.AddHandler(new FileLoggerHandler());
			Logger.DebugOn();

			var httpServer = new UserHttpServer();
			Router.Register(httpServer);

			_ = new DeviceRegisterTcpServer();

			Console.ReadLine();
		}
	}
}