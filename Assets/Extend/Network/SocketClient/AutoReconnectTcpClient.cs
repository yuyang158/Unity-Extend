using System;
using System.Net.Sockets;
using Extend.Common;
using Extend.DebugUtil;
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
		
		[CSharpCallLua]
		public delegate void LuaUpdate(LuaTable owner);
		public readonly LuaUpdate updateCallback;

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
			updateCallback = callback.Get<LuaUpdate>("OnUpdate");

			var service = CSharpServiceManager.Get<NetworkService>(CSharpServiceManager.ServiceType.NETWORK_SERVICE);
			service.RegisterTcpClient(this);
		}

		public void Connect(string host, int port) {
			connectionContext.Host = host;
			connectionContext.Port = port;
			reconnectTime = 0;
			DoConnect();
		}

		public async void Send(byte[] buffer) {
			try {
				StatService.Get().Increase(StatService.StatName.TCP_SENT, buffer.Length);
				await client.GetStream().WriteAsync(buffer, 0, buffer.Length);
				await client.GetStream().FlushAsync();
			}
			catch( Exception e ) {
				Debug.LogWarning($"Socket write exception : {e}");
				TcpStatus = Status.DISCONNECTED;
			}
		}

		public void Destroy() {
			client.Close();
			var service = CSharpServiceManager.Get<NetworkService>(CSharpServiceManager.ServiceType.NETWORK_SERVICE);
			service.UnregisterTcpClient(this);
		}

		private async void DoConnect() {
			TcpStatus = Status.RECONNECT;
			try {
				Debug.LogWarning($"Start async connect to server : {connectionContext.Host}:{connectionContext.Port}");
				await client.ConnectAsync(connectionContext.Host, connectionContext.Port);
			}
			catch( Exception e ) {
				Debug.Log($"Connection fail with exception : {e}");
			}
			finally {
				TcpStatus = client.Connected ? Status.CONNECTED : Status.DISCONNECTED;
			}
		}

		private async void DoReceive() {
			var stream = client.GetStream();
			while( client.Connected && Application.isPlaying ) {
				if( stream.CanRead ) {
					int recvCount;
					try {
						recvCount = await stream.ReadAsync(receiveBuffer, receiveOffset, receiveBuffer.Length - receiveOffset);
					}
					catch( Exception e ) {
						Debug.LogWarning($"Socket receive exception : {e}");
						TcpStatus = Status.DISCONNECTED;
						return;
					}
					receiveOffset += recvCount;
					StatService.Get().Increase(StatService.StatName.TCP_RECEIVED, recvCount);

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

			updateCallback(callback);
			statusTimeLast += Time.deltaTime;
			if( TcpStatus == Status.RECONNECT ) {
				if( CONNECTING_TIMEOUT_DURATION < statusTimeLast ) {
					client.Close();
				}
			}
		}
	}
}