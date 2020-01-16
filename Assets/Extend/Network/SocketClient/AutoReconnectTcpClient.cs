using System;
using System.Net.Sockets;
using UnityEngine;
using XLua;

namespace Extend.Network.SocketClient {
	[LuaCallCSharp, CSharpCallLua]
	public class AutoReconnectTcpClient {
		private readonly TcpClient client;

		public enum Status {
			NONE,
			CONNECTED,
			DISCONNECTED,
			RECONNECT,
		}

		private struct ConnectionContext {
			public string Host;
			public int Port;
		}

		private ConnectionContext connectionContext;
		private Status tcpStatus = Status.NONE;
		private float statusTimeLast;
		private int reconnectTime;
		
		private readonly LuaTable callback;
		private readonly LuaFunction statusChangedCallback;
		private readonly LuaFunction receivePackageCallback;
		private readonly LuaFunction updateCallback;

		private readonly byte[] receiveBuffer = new byte[65536];
		private int receiveOffset;

		private const float CONNECTING_TIMEOUT_DURATION = 2;
		private const int TOTAL_RETRY_TIME = 5;

		public Status TcpStatus {
			get => tcpStatus;
			set {
				if( tcpStatus == value )
					return;

				statusTimeLast = 0;
				if( tcpStatus == Status.RECONNECT && value == Status.DISCONNECTED ) {
					reconnectTime++;
					if( reconnectTime <= TOTAL_RETRY_TIME ) {
						DoConnect();
						return;
					}
				}

				tcpStatus = value;
				statusChangedCallback.Call(callback, value);
				if( tcpStatus == Status.CONNECTED ) {
					DoReceive();
				}
				else if( tcpStatus == Status.DISCONNECTED && client.Connected ) {
					client.Close();
				}
			}
		}

		public AutoReconnectTcpClient(LuaTable luaCallback) {
			client = new TcpClient(AddressFamily.InterNetwork) {
				NoDelay = true
			};
			connectionContext = new ConnectionContext();
			callback = luaCallback;
			statusChangedCallback = callback.Get<LuaFunction>("OnStatusChanged");
			receivePackageCallback = callback.Get<LuaFunction>("OnRecvPackage");
			updateCallback = callback.Get<LuaFunction>("OnUpdate");
		}

		public void Connect(string host, int port) {
			connectionContext.Host = host;
			connectionContext.Port = port;
			reconnectTime = 0;
			DoConnect();
		}

		public async void Send(byte[] buffer) {
			var task = client.GetStream().WriteAsync(buffer, 0, buffer.Length);
			await task;
			if( task.IsCanceled || task.IsFaulted ) {
				TcpStatus = Status.DISCONNECTED;
			}
		}

		public void Close() {
			TcpStatus = Status.DISCONNECTED;
		}

		private async void DoConnect() {
			TcpStatus = Status.RECONNECT;
			await client.ConnectAsync(connectionContext.Host, connectionContext.Port);
			TcpStatus = client.Connected ? Status.CONNECTED : Status.DISCONNECTED;
		}

		private async void DoReceive() {
			var stream = client.GetStream();
			while( client.Connected ) {
				if( stream.CanRead ) {
					int readCount;
					try {
						readCount = await stream.ReadAsync(receiveBuffer, receiveOffset, receiveBuffer.Length - receiveOffset);
					}
					catch( Exception ) {
						TcpStatus = Status.DISCONNECTED;
						return;
					}
					receiveOffset += readCount;

					var readOffset = 0;
					while( true ) {
						var packageSize = receiveBuffer[readOffset] * 256 + receiveBuffer[readOffset + 1];
						if( packageSize + 2 > receiveOffset - readOffset ) {
							break;
						}

						var packageBuffer = new byte[packageSize];
						Array.Copy(receiveBuffer, readOffset + 2, packageBuffer, 0, packageSize);
						receivePackageCallback.Call(callback, packageBuffer);
						readOffset += packageSize + 2;
					}
					if( readOffset > 0 && receiveOffset - readOffset >= 0 ) {
						for( var i = readOffset; i < receiveOffset; i++ ) {
							receiveBuffer[i - readOffset] = receiveBuffer[i];
						}
						receiveOffset -= readOffset;
					}
				}
			}
		}

		[BlackList]
		public void Update() {
			if( TcpStatus == Status.NONE )
				return;

			updateCallback.Call(callback, Time.deltaTime);

			statusTimeLast += Time.deltaTime;
			if( TcpStatus == Status.RECONNECT ) {
				if( CONNECTING_TIMEOUT_DURATION < statusTimeLast ) {
					client.Close();
				}

				return;
			}

			if( TcpStatus == Status.CONNECTED ) {
			}
		}
	}
}