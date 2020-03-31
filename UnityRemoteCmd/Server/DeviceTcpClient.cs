using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server {
	public class DeviceTcpClient {
		private readonly TcpClient tcpClient;
		private readonly byte[] buffer = new byte[2048];
		public string Name { get; private set; }
		public string Guid { get; }
		public bool Connected { get; private set; } = true;
		
		private static int UID = 1;
		public DeviceTcpClient(TcpClient client) {
			tcpClient = client;
#pragma warning disable 4014
			StartRecv();
#pragma warning restore 4014

			Guid = UID.ToString();
			UID++;
		}

		private async Task ReadWithSize(int count) {
			var total = count;
			var offset = 0;
			var readCount = 0;
			while( true ) {
				var c = await tcpClient.GetStream().ReadAsync(buffer, offset, count);
				if( c <= 0 ) {
					Disconnect();
					break;
				}
				readCount += c;
				if( readCount < total ) {
					offset = readCount;
					count = total - readCount;
				}
				else {
					break;
				}
			}
		}

		private string luaRet;

		public Task<string> RequestDevice(string lua) {
			luaRet = "";
			try {
				Console.WriteLine($"REQUEST LUA : {lua}");
				tcpClient.GetStream().Write(BitConverter.GetBytes((short)lua.Length));
				tcpClient.GetStream().Write(Encoding.UTF8.GetBytes(lua));
				return Task.Run(() => {
					var counter = 0;
					while( string.IsNullOrEmpty(luaRet) && counter < 50 ) {
						Thread.Sleep(100);
						counter++;
					}
					return luaRet;
				});
			}
			catch( Exception e ) {
				Disconnect();
				return Task.Run(() => e.Message);
			}
		}

		private void Disconnect() {
			lock( Router.Devices ) {
				Router.Devices.Remove(this);
				Console.WriteLine($"Disconnect : {Guid}");
				Connected = false;
			}
		}

		private async Task StartRecv() {
			try {
				await Process();
			}
			catch( Exception ) {
				Disconnect();
			}
		}

		private async Task Process() {
			while( true ) {
				await ReadWithSize(1);
				if(!Connected)
					break;
				var protocol = buffer[0];
				await ReadWithSize(2);
				if(!Connected)
					break;
				var sizeBuff = new byte[2];
				Array.Copy(buffer, 0, sizeBuff, 0, 2);
				var size = BitConverter.ToInt16(sizeBuff);
				await ReadWithSize(size);
				if(!Connected)
					break;
				switch( protocol ) {
					case 1:
						Name = Encoding.UTF8.GetString(buffer, 0, size);
						Console.WriteLine($"DEVICE {Name} Connect, {DateTime.Now.ToLongTimeString()}");
						break;
					case 2:
						luaRet = Encoding.UTF8.GetString(buffer, 0, size);
						break;
				}
			}
		}
	}
}