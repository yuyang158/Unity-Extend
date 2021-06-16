using System;
using System.Threading;
using Extend.Asset.AssetProvider;
using Extend.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	public class AssetBundleInstance : AssetRefObject {
		private AssetBundle AB { get; set; }
		public string ABPath { get; }
		private AssetBundleInstance[] m_dependencies;
		private const int SYNC_LOAD_PRIORITY = 10;

		public AssetBundleInstance(string abPath) {
			ABPath = string.Intern(abPath);
			AssetService.Get().Container.PutAB(this);
			SetupDependencies(AssetBundleLoadProvider.Manifest.GetDirectDependencies(abPath));
		}

		private void SetupDependencies(string[] dependencies) {
			if( dependencies == null || dependencies.Length == 0 ) {
				return;
			}

			if( dependencies.Length > 63 ) {
				throw new Exception($"{ABPath} dependencies count > 64.");
			}

			m_dependencies = new AssetBundleInstance[dependencies.Length];
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			for( int i = 0; i < m_dependencies.Length; i++ ) {
				var hash = GenerateHash(dependencies[i]);

				if( !( service.Container.TryGetAsset(hash) is AssetBundleInstance dep ) ) {
					dep = new AssetBundleInstance(dependencies[i]);
				}

				dep.IncRef();
				m_dependencies[i] = dep;
			}
		}

		public void SetAssetBundle(AssetBundle ab) {
#if ASSET_LOG
			Debug.LogWarning($"Asset Bundle Loaded : {ABPath}");
#endif
			AB = ab;
			Status = AB ? AssetStatus.DONE : AssetStatus.FAIL;
		}

		public override void Destroy() {
			if( Status != AssetStatus.DONE )
				return;

			if( m_dependencies != null ) {
				foreach( var dependency in m_dependencies ) {
					dependency.Release();
				}
			}

			AB.Unload(false);
			Object.Destroy(AB);
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.Increase(StatService.StatName.ASSET_BUNDLE_COUNT, -1);
		}

		public static int GenerateHash(string path) {
			var hash = path + ".ab";
			return hash.GetHashCode();
		}

		public void Load() {
			if( IsFinished )
				throw new Exception($"Repeat loading : {ABPath}");

			Status = AssetStatus.ASYNC_LOADING;
			if( m_dependencies != null ) {
				foreach( var dependency in m_dependencies ) {
					if( dependency.IsFinished || dependency.Status == AssetStatus.ASYNC_LOADING )
						continue;
					dependency.Load();
				}
			}

			byte[] offsetData = System.Text.Encoding.UTF8.GetBytes(ABPath.GetHashCode().ToString());
			var ab = AssetBundle.LoadFromFile(AssetBundleLoadProvider.DetermineLocation(ABPath), 0, (ulong)offsetData.Length);
			SetAssetBundle(ab);
		}

		public void LoadAsync(Action callback) {
			if( IsFinished )
				throw new Exception($"Repeat loading : {ABPath}");

			ulong flag = 0;
			bool selfLoaded = false;

			void FinishCheck() {
				if( flag == 0 && selfLoaded ) {
					callback?.Invoke();
				}
			}

			if( Status == AssetStatus.ASYNC_LOADING ) {
				void Callback(AssetRefObject refObject) {
					if( !IsFinished )
						return;
					OnStatusChanged -= Callback;
					callback?.Invoke();
				}

				OnStatusChanged += Callback;
				return;
			}

			Status = AssetStatus.ASYNC_LOADING;
			const ulong mask = 1;
			if( m_dependencies != null ) {
				for( int i = 0; i < m_dependencies.Length; i++ ) {
					var index = i;
					var dependency = m_dependencies[i];
					if( dependency.IsFinished )
						continue;

					flag |= mask << i;
					if( dependency.Status == AssetStatus.ASYNC_LOADING ) {
						dependency.OnStatusChanged += _ => {
							if( dependency.IsFinished ) {
								// ReSharper disable once AccessToModifiedClosure
								flag &= ~( mask << index );
								FinishCheck();
							}
						};
					}
					else {
						dependency.LoadAsync(() => {
							// ReSharper disable once AccessToModifiedClosure
							flag &= ~( mask << index );
							FinishCheck();
						});
					}
				}
			}

			byte[] offsetData = System.Text.Encoding.UTF8.GetBytes(ABPath.GetHashCode().ToString());
			var req = AssetBundle.LoadFromFileAsync(AssetBundleLoadProvider.DetermineLocation(ABPath), 0, (ulong)offsetData.Length);
			req.completed += _ => {
				selfLoaded = true;
				SetAssetBundle(req.assetBundle);
				FinishCheck();
			};
		}

		public string GetScenePath() {
			return AB.GetAllScenePaths()[0];
		}

		public Object LoadAsset(string assetPath, Type type) {
			if( !IsFinished ) {
				Load();
			}

			return AB.LoadAsset(assetPath, type);
		}

		public void LoadAssetAsync(string assetPath, Action<Object> callback, Type type) {
			if( !IsFinished ) {
				LoadAsync(() => {
					var request = AB.LoadAssetAsync(assetPath, type);
					request.completed += operation => { callback(request.asset); };
				});
			}
			else {
				var req = AB.LoadAssetAsync(assetPath, type);
				req.completed += operation => { callback(req.asset); };
			}
		}

		public override int GetHashCode() {
			return GenerateHash(ABPath);
		}

		public override string ToString() {
			return ABPath;
		}
	}
}