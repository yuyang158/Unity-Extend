using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server {
	public static class Router {
		public static readonly List<DeviceTcpClient> Devices = new List<DeviceTcpClient>();

		public static void Register(UserHttpServer server) {
			server.RegisterRouter("devices", (context, body) => {
				var jDevices = new JArray();
				lock( Devices ) {
					for( var i = 0; i < Devices.Count;  ) {
						if( !Devices[i].Connected ) {
							Devices.RemoveAt(i);
						}
						else {
							i++;
						}
					}
					
					foreach( var device in Devices ) {
						jDevices.Add(new JObject {
							["uid"] = device.Guid,
							["name"] = device.Name
						});
					}
				}

				return jDevices.ToString(Formatting.None);
			});
			
			server.RegisterRouter("lua", (context, body) => {
				if(string.IsNullOrEmpty(body))
					return string.Empty;
				var device = context.Request.QueryString["device"];
				// ReSharper disable once InconsistentlySynchronizedField
				var client = Devices.Find(c => c.Guid == device);
				var task = client.RequestDevice(body);
				task.Wait(5000);
				return task.Result;
			});
		}
	}
}