using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	public class AssetDepUploader : AssetPostprocessor {
		private const string PROJ_NAME = "GOH";

		[MenuItem("Assets/Find Referenced By", false, 30)]
		private static async void FindReferenceBy() {
			if( !Selection.activeObject )
				return;
			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if( string.IsNullOrEmpty(path) )
				return;

			var guid = AssetDatabase.AssetPathToGUID(path);
			var req = WebRequest.CreateHttp("http://pw-linux.private-tunnel.site:4300/" +
			                                $"get-dependency?project={PROJ_NAME}&source={guid}");
			var response = await req.GetResponseAsync();
			var buffer = new byte[response.ContentLength];
			var stream = response.GetResponseStream();
			var readCount = 0;
			while( readCount < response.ContentLength ) {
				readCount += await stream.ReadAsync(buffer, readCount, buffer.Length - readCount);
			}

			var result = JObject.Parse(Encoding.UTF8.GetString(buffer));
			Debug.Log(result);
			var refs = result["results"] as JArray;
			Object[] selections = new Object[refs.Count];
			for( int i = 0; i < refs.Count; i++ ) {
				path = AssetDatabase.GUIDToAssetPath(refs[i]["source"].ToString());
				if( string.IsNullOrEmpty(path) ) {
					continue;
				}

				selections[i] = AssetDatabase.LoadAssetAtPath<Object>(path);
			}

			Selection.objects = selections;
		}

#if NOTIFICATION_WHEN_COMPILE_ERROR
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if( deletedAssets.Length > 0 ) {
				var guids = new JArray();
				var content = new JObject {
					{"project", PROJ_NAME},
					{"guids", guids}
				};
				foreach( var deletedAsset in deletedAssets ) {
					if( !deletedAsset.StartsWith("Assets") )
						continue;
				
					guids.Add(AssetDatabase.AssetPathToGUID(deletedAsset));
				}
				PostRequest("http://pw-linux.private-tunnel.site:4300/delete-item", content);
			}

			foreach( var importedAsset in importedAssets ) {
				if( !importedAsset.StartsWith("Assets") ) {
					continue;
				}

				var guid = AssetDatabase.AssetPathToGUID(importedAsset);
				var deps = AssetDatabase.GetDependencies(importedAsset);
				if( deps.Length <= 1 ) {
					continue;
				}

				var relations = new JArray();
				var content = new JObject {
					{"project", PROJ_NAME},
					{"relations", relations},
					{"source", guid}
				};

				foreach( var dep in deps ) {
					if( dep == guid || dep.StartsWith("Packages") )
						continue;
					relations.Add(AssetDatabase.AssetPathToGUID(dep));
				}

				if( relations.Count == 0 )
					continue;

				PostRequest("http://pw-linux.private-tunnel.site:4300/upload-dependency", content);
			}
		}

		private static void PostRequest(string URL, JToken jcontent) {
			var req = WebRequest.Create(URL);
			req.Method = "POST";
			var stream = req.GetRequestStream();
			req.ContentType = "application/json";
			var content = Encoding.UTF8.GetBytes(jcontent.ToString(Formatting.None));
			stream.Write(content, 0, content.Length);

			var response = req.GetResponse();
			var buffer = new byte[response.ContentLength];
			stream = response.GetResponseStream();
			var readCount = 0;
			while( readCount < response.ContentLength ) {
				readCount += stream.Read(buffer, readCount, buffer.Length - readCount);
			}

			Debug.Log(Encoding.UTF8.GetString(buffer));
		}
#endif
	}
}