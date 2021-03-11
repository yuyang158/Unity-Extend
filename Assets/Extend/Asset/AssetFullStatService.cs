using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Extend.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Extend.Asset {
	public class AssetFullStatService : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_FULL_STAT;
		public static readonly int LISTEN_PORT = 34320;
		private Thread m_listenThread;
		private TcpListener m_tcpListener;

		public void Initialize() {
			m_listenThread = new Thread(Listen);
			m_listenThread.Start();
		}

		private readonly List<AssetInstance> m_loadedAssets = new List<AssetInstance>(128);
		private readonly Dictionary<int, Tuple<string, GameObject>> m_loadedGameObjects = 
			new Dictionary<int, Tuple<string, GameObject>>(256);

		public void OnAssetLoaded(AssetInstance asset) {
			lock( m_loadedAssets ) {
				m_loadedAssets.Add(asset);
			}
		}

		public void OnAssetUnloaded(AssetInstance asset) {
			lock( m_loadedAssets ) {
				var index = m_loadedAssets.IndexOf(asset);
				if( index == -1 )
					return;
				m_loadedAssets.RemoveSwapAt(index);
			}
		}

		public void OnInstantiateGameObject(GameObject go) {
			go.AddComponent<DestroyGOStatHandle>();
			lock( m_loadedGameObjects ) {
				m_loadedGameObjects.Add(go.GetInstanceID(), new Tuple<string, GameObject>(go.name, go));
			}
		}

		public void OnDestroyGameObject(GameObject go) {
			lock( m_loadedGameObjects ) {
				m_loadedGameObjects.Remove(go.GetInstanceID());
			}
		}

		public IEnumerable<GameObject> GetInstantiateGOArray() {
			lock( m_loadedGameObjects ) {
				var goArray = new GameObject[m_loadedGameObjects.Count];
				var index = 0;
				foreach( var value in m_loadedGameObjects.Values ) {
					goArray[index] = value.Item2;
					index++;
				}
				return goArray;
			}
		}

		public AssetInstance[] GetLoadedAssets() {
			lock( m_loadedAssets ) {
				return m_loadedAssets.ToArray();
			}
		}

		private void Listen() {
			try {
				m_tcpListener = new TcpListener(IPAddress.Any, LISTEN_PORT);
				m_tcpListener.Start();

				while( true ) {
					var connection = m_tcpListener.AcceptTcpClient();
					var stream = connection.GetStream();
					try {
						lock( m_loadedAssets ) {
							lock( m_loadedGameObjects ) {
								var loadedAssetsJson = new JArray();
								foreach( var asset in m_loadedAssets ) {
									loadedAssetsJson.Add(asset.AssetPath);
								}

								var loadedGameObjectJson = new JArray();
								foreach( var loadedGameObject in m_loadedGameObjects ) {
									loadedGameObjectJson.Add($"{loadedGameObject.Key}@{loadedGameObject.Value.Item1}");
								}

								var content = new JObject() {
									{"assets", loadedAssetsJson},
									{"gameObjects", loadedGameObjectJson}
								};
								var buffer = Encoding.UTF8.GetBytes(content.ToString(Formatting.None));
								stream.Write(buffer, 0, buffer.Length);
							}
						}
					}
					catch( Exception e ) {
						Debug.LogException(e);
					}
					finally {
						connection.Close();
					}
				}
			}
			catch( Exception ) {
				// ignored
			}
		}

		public void Destroy() {
			m_tcpListener.Stop();
			lock( m_loadedAssets ) {
				m_loadedAssets.Clear();
			}
			lock( m_loadedGameObjects ) {
				m_loadedGameObjects.Clear();
			}
		}
	}
}