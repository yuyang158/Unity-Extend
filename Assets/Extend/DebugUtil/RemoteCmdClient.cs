using System;
using System.Net.Sockets;
using System.Text;
using Extend;
using Extend.Common;
using Extend.Common.Lua;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public static class RemoteCmdClient {
	private static readonly TcpClient tcpClient = new TcpClient {NoDelay = true};
	public static void Restart() {
		try {
			tcpClient.Close();
		}
		finally {
			Start();
		}
	}
	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static async void Start() {
		try {
			if(!Debug.isDebugBuild || Application.isEditor)
				return;
			await tcpClient.ConnectAsync("192.144.187.92", 4101);

			var id = $"{SystemInfo.deviceName} - {SystemInfo.deviceModel}";
			var protocol = new byte[] {1};
			await tcpClient.GetStream().WriteAsync(protocol, 0, protocol.Length);
			var buffer = Encoding.UTF8.GetBytes(id);
			var size = BitConverter.GetBytes((short)buffer.Length);
			await tcpClient.GetStream().WriteAsync(size, 0, size.Length);
			await tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);

			while( true ) {
				await tcpClient.GetStream().ReadAsync(size, 0, 2);
				var luaSize = BitConverter.ToInt16(size, 0);

				var recvCount = 0;
				buffer = new byte[luaSize];
				while( recvCount < luaSize ) {
					var count = await tcpClient.GetStream().ReadAsync(buffer, recvCount, buffer.Length - recvCount);
					if(count == 0)
						return;
					recvCount += count;
				}

				var lua = Encoding.UTF8.GetString(buffer);
				Debug.LogWarning($"REMOTE DEBUG REQUEST : {lua}");
				var luaService = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var func = luaService.Default.Global.Get<LuaFunction>("Global_DebugFunction");
				var ret = func.Call(lua)[0].ToString();
				
				protocol = new byte[] {2};
				await tcpClient.GetStream().WriteAsync(protocol, 0, protocol.Length);
				size = BitConverter.GetBytes((short)ret.Length);
				await tcpClient.GetStream().WriteAsync(size, 0, size.Length);
				buffer = Encoding.UTF8.GetBytes(ret);
				await tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
				await tcpClient.GetStream().FlushAsync();
				Debug.LogWarning($"REMOTE DEBUG RESPONSE : {ret}");
			}
		}
		catch( Exception ex ) {
			// ignored
			Debug.LogWarning(ex);
		}
	}
}
