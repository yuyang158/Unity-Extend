using System;
using System.Collections;
using System.Net.Sockets;
using Extend.Common;
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
		
		private LuaTable callback;
		private LuaFunction statusChangedCallback;

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
				if( tcpStatus == Status.RECONNECT && tcpStatus == Status.DISCONNECTED ) {
					reconnectTime++;
					if( reconnectTime <= TOTAL_RETRY_TIME ) {
						DoConnect();
						return;
					}
				}

				tcpStatus = value;
				statusChangedCallback.Call(callback, value);
				if( tcpStatus == Status.CONNECTED ) {
					var coroutineService = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
					coroutineService.StartCoroutine(DoReceive());
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
			statusChangedCallback = callback.GetInPath<LuaFunction>("OnStatusChanged");
		}

		public void Connect(string host, int port) {
			connectionContext.Host = host;
			connectionContext.Port = port;
			reconnectTime = 0;
			DoConnect();
		}

		private void DoConnect() {
			TcpStatus = Status.RECONNECT;
			client.BeginConnect(connectionContext.Host, connectionContext.Port, ar => {
				client.EndConnect(ar);
				TcpStatus = client.Connected ? Status.CONNECTED : Status.DISCONNECTED;
			}, null);
		}

		private IEnumerator DoReceive() {
			var stream = client.GetStream();
			while( client.Connected ) {
				if( stream.CanRead ) {
					var readCount = stream.Read(receiveBuffer, receiveOffset, receiveBuffer.Length - receiveOffset);
					receiveOffset += readCount;

					var readOffset = 0;
					while( true ) {
						var packageSize = receiveBuffer[readOffset] * 256 + receiveBuffer[readOffset + 1];
						if( packageSize > receiveOffset ) {
							break;
						}

						var packageBuffer = new byte[packageSize];
						Array.Copy(receiveBuffer, readOffset, packageBuffer, 0, packageSize);
						
						readOffset += packageSize;
					}
					if( readOffset > 0 && receiveOffset - readOffset > 0 ) {
						for( var i = readOffset; i < receiveOffset; i++ ) {
							receiveBuffer[i - readOffset] = receiveBuffer[i];
						}

						receiveOffset -= readOffset;
					}
				}

				yield return null;
			}
		}

		[BlackList]
		public void Update() {
			if( TcpStatus == Status.NONE )
				return;

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