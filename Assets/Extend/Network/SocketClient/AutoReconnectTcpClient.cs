using System;
using System.Net.Sockets;
using Extend.Common;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend.Network.SocketClient {
	[LuaCallCSharp, CSharpCallLua]
	public class AutoReconnectTcpClient {
		private readonly TcpClient m_client;

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

		private ConnectionContext m_connectionContext;
		private Status m_tcpStatus = Status.NONE;
		private float m_statusTimeLast;
		private int m_reconnectTime;

		private readonly LuaTable m_callback;
		private readonly OnSocketStatusChanged m_statusChangedCallback;
		private readonly OnRecvData m_receivePackageCallback;

		[CSharpCallLua]
		public delegate void MoonUpdate(LuaTable owner);

		public readonly MoonUpdate updateCallback;

		private readonly byte[] m_receiveBuffer = new byte[65536];
		private int m_receiveOffset;

		private const float CONNECTING_TIMEOUT_DURATION = 2;
		private const int TOTAL_RETRY_TIME = 5;
		private const int PbLenOffsetA = 1 << 23;
		private const int PbLenOffsetB = 1 << 15;
		private const int PbLenOffsetC = 1 << 7;

		public Status TcpStatus {
			get => m_tcpStatus;
			set {
				if( m_tcpStatus == value )
					return;

				m_statusTimeLast = 0;
				if( m_tcpStatus == Status.RECONNECT && value == Status.DISCONNECTED ) {
					m_reconnectTime++;
					if( m_reconnectTime <= TOTAL_RETRY_TIME ) {
						DoConnect();
						return;
					}
				}

				m_tcpStatus = value;
				if( CSharpServiceManager.Initialized )
					m_statusChangedCallback(m_callback, value);
				if( m_tcpStatus == Status.CONNECTED ) {
					DoReceive();
				}
				else if( m_tcpStatus == Status.DISCONNECTED && m_client.Client != null && m_client.Connected ) {
					m_client.Close();
				}
			}
		}

		private byte[] m_cyphertextBuffer = new byte[65536];
		private int m_cyphertextOffset;

		public AutoReconnectTcpClient(LuaTable luaCallback) {
			m_client = new TcpClient(AddressFamily.InterNetwork) {
				NoDelay = true
			};
			m_connectionContext = new ConnectionContext();
			m_callback = luaCallback;
			m_statusChangedCallback = m_callback.Get<OnSocketStatusChanged>("OnStatusChanged");
			m_receivePackageCallback = m_callback.Get<OnRecvData>("OnRecvPackage");
			updateCallback = m_callback.Get<MoonUpdate>("OnUpdate");

			var service = CSharpServiceManager.Get<NetworkService>(CSharpServiceManager.ServiceType.NETWORK_SERVICE);
			service.RegisterTcpClient(this);
		}

		public void Connect(string host, int port) {
			m_connectionContext.Host = host;
			m_connectionContext.Port = port;
			m_reconnectTime = 0;
			DoConnect();
		}

		public async void Send(byte[] buffer) {
			try {
				StatService.Get().Increase(StatService.StatName.TCP_SENT, buffer.Length);
				await m_client.GetStream().WriteAsync(buffer, 0, buffer.Length);
				await m_client.GetStream().FlushAsync();
			}
			catch( Exception e ) {
				Debug.LogWarning($"Socket write exception : {e}");
				TcpStatus = Status.DISCONNECTED;
			}
		}

		public void Destroy() {
			m_client.Close();
			var service = CSharpServiceManager.Get<NetworkService>(CSharpServiceManager.ServiceType.NETWORK_SERVICE);
			service.UnregisterTcpClient(this);
		}

		private async void DoConnect() {
			TcpStatus = Status.RECONNECT;
			try {
				Debug.LogWarning($"Start async connect to server : {m_connectionContext.Host}:{m_connectionContext.Port}");
				await m_client.ConnectAsync(m_connectionContext.Host, m_connectionContext.Port);
			}
			catch( Exception e ) {
				Debug.Log($"Connection fail with exception : {e}");
			}
			finally {
				TcpStatus = m_client.Client != null && m_client.Connected ? Status.CONNECTED : Status.DISCONNECTED;
			}
		}

		private async void DoReceive() {
			var stream = m_client.GetStream();
			while( m_client.Connected && Application.isPlaying ) {
				if( stream.CanRead ) {
					int count = 0;
					try {
						count = await stream.ReadAsync(m_receiveBuffer, 0, m_cyphertextBuffer.Length);
					}
					catch( Exception ) {
						TcpStatus = Status.DISCONNECTED;
						return;
					}
					StatService.Get().Increase(StatService.StatName.TCP_RECEIVED, count);

					var readOffset = 0;
					while( true ) {
						var packageSize = m_receiveBuffer[readOffset] * PbLenOffsetA + m_receiveBuffer[readOffset + 1] * PbLenOffsetB +
						                  m_receiveBuffer[readOffset + 2] * PbLenOffsetC + m_receiveBuffer[readOffset + 3];
						if( packageSize + 4 > m_receiveOffset - readOffset ) {
							break;
						}

						var packageBuffer = new byte[packageSize];
						Array.Copy(m_receiveBuffer, readOffset + 4, packageBuffer, 0, packageSize);
						m_receivePackageCallback(m_callback, packageBuffer);
						readOffset += packageSize + 4;
					}

					if( readOffset > 0 && m_receiveOffset - readOffset >= 0 ) {
						for( var i = readOffset; i < m_receiveOffset; i++ ) {
							m_receiveBuffer[i - readOffset] = m_receiveBuffer[i];
						}

						m_receiveOffset -= readOffset;
					}
				}
			}
		}

		[BlackList]
		public void Update() {
			if( TcpStatus == Status.NONE )
				return;

			updateCallback(m_callback);
			m_statusTimeLast += Time.deltaTime;
			if( TcpStatus == Status.RECONNECT ) {
				if( CONNECTING_TIMEOUT_DURATION < m_statusTimeLast ) {
					m_client.Close();
				}
			}
		}
	}
}