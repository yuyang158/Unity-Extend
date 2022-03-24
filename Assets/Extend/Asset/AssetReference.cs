using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using XLua;
using Object = UnityEngine.Object;
using AddressableReference = UnityEngine.AddressableAssets.AssetReference;

namespace Extend.Asset {
	[CSharpCallLua]
	public delegate void OnInstantiateComplete(GameObject go);

	[Serializable, LuaCallCSharp]
	public class AssetReference : IDisposable {
		[SerializeField, HideInInspector]
		private AddressableReference m_assetRef;

		private AssetInstance m_asset;

		internal AssetInstance Asset {
			get => m_asset;
			private set {
				m_asset?.Release();
				m_asset = value;
				m_asset?.IncRef();
			}
		}

		public bool IsFinished => Asset is {IsFinished: true};
		public string AssetGUID => m_assetRef.AssetGUID;

		public AssetReference() {
		}

		public AssetReference(string guid) {
			m_assetRef = new AddressableReference(guid);
		}

		public AssetReference(AsyncOperationHandle handle) {
			Asset = AssetService.Get().Container.TryGetAsset(handle.GetHashCode()) as AssetInstance;
			Asset ??= handle.Result is GameObject ? new PrefabAssetInstance(handle) : new AssetInstance(handle);
			Addressables.ResourceManager.Release(handle);
		}

		public bool GUIDValid {
			get {
#if UNITY_EDITOR
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(m_assetRef.AssetGUID);
				return !string.IsNullOrEmpty(path);
#else
				return m_assetRef.IsValid();
#endif
			}
		}

		[BlackList]
		private T GetAsset<T>() where T : Object {
			if( Asset != null ) {
				return Asset.UnityObject as T;
			}

			if( !m_assetRef.IsDone ) {
				var handle = m_assetRef.LoadAssetAsync<T>();
				handle.WaitForCompletion();
			}

			Asset = AssetService.Get().Container.TryGetAsset(m_assetRef.OperationHandle.GetHashCode()) as AssetInstance;
			Asset ??= typeof(T) == typeof(GameObject) ? new PrefabAssetInstance(m_assetRef.OperationHandle) : new AssetInstance(m_assetRef.OperationHandle);

			return Asset.UnityObject as T;
		}

		public Object GetObject() {
			return GetAsset<Object>();
		}

		public Shader GetShader() {
			return GetAsset<Shader>();
		}

		public Sprite GetSprite() {
			return GetAsset<Sprite>();
		}

		public Texture GetTexture() {
			return GetAsset<Texture>();
		}

		public Texture3D GetTexture3D() {
			return GetAsset<Texture3D>();
		}

		public TextAsset GetTextAsset() {
			return GetAsset<TextAsset>();
		}

		public Material GetMaterial() {
			return GetAsset<Material>();
		}

		public GameObject GetGameObject() {
			return GetAsset<GameObject>();
		}

		public AudioClip GetAudioClip() {
			return GetAsset<AudioClip>();
		}

		public AnimationClip GetAnimationClip() {
			return GetAsset<AnimationClip>();
		}

		public Mesh GetMesh() {
			return GetAsset<Mesh>();
		}

		public T GetScriptableObject<T>() where T : ScriptableObject {
			return GetAsset<T>();
		}

		[BlackList]
		public AssetAsyncLoadHandle LoadAsync<T>() where T : Object {
			if( IsFinished ) {
				return null;
			}

			var handle = Addressables.LoadAssetAsync<T>(m_assetRef);
			var loadHandle = new AssetAsyncLoadHandle(AssetService.Get().Container, handle);
			loadHandle.OnComplete += _ => {
				Asset = loadHandle.Asset;
				Addressables.Release(handle);
#if ASSET_LOG
				Debug.LogWarning($"Load reference asset callback : {Asset.UnityObject}");
#endif
			};
#if ASSET_LOG
			Debug.LogWarning($"Async reference load asset : {m_assetRef.OperationHandle.DebugName}");
#endif
			return loadHandle;
		}

		public GameObject Instantiate(Transform parent = null, bool stayWorldPosition = false) {
			GetGameObject();
			return Asset is not PrefabAssetInstance prefabAsset ? null : prefabAsset.Instantiate(parent, stayWorldPosition);
		}

		public GameObject Instantiate(Vector3 position) {
			return Instantiate(position, Quaternion.identity);
		}

		public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent = null) {
			GetGameObject();
			if( Asset is not PrefabAssetInstance prefabAsset ) {
				return null;
			}

			return prefabAsset.Instantiate(position, rotation, parent);
		}

		[LuaCallCSharp]
		public class InstantiateAsyncContext {
			private AssetReference m_ref;

			[BlackList]
			private AssetReference Ref {
				set {
					m_ref = value;
					if( !GetAssetReady() ) {
						m_ref.LoadAsync<GameObject>();
					}
				}
				get => m_ref;
			}

			public event OnInstantiateComplete Callback;
			public bool Cancel;

			private readonly Transform m_parent;
			private readonly bool m_stayWorldPosition;
			private readonly Vector3 m_position;
			private readonly Quaternion m_rotation;

			private readonly int m_ctorType;

			public InstantiateAsyncContext(AssetReference reference, Transform parent, bool stayWorldPosition) {
				m_ctorType = 1;
				m_parent = parent;
				Ref = reference;
				m_stayWorldPosition = stayWorldPosition;
				AssetService.Get().AddDeferInstantiateContext(this);
			}

			public InstantiateAsyncContext(AssetReference reference, Vector3 position, Quaternion rotation, Transform parent) {
				m_ctorType = 2;
				m_parent = parent;
				Ref = reference;
				m_position = position;
				m_rotation = rotation;
				AssetService.Get().AddDeferInstantiateContext(this);
			}

			public bool GetAssetReady() {
				return Ref.IsFinished;
			}

			public void Instantiate() {
				if( Cancel )
					return;
				if( Ref.Asset is not PrefabAssetInstance prefabAsset ) {
					return;
				}

				if( !Ref.IsFinished ) {
					return;
				}

				var go = m_ctorType == 1 ? prefabAsset.Instantiate(m_parent, m_stayWorldPosition) : prefabAsset.Instantiate(m_position, m_rotation, m_parent);
				Callback?.Invoke(go);
			}

			public override string ToString() {
				return $"Context : {Ref.Asset}";
			}
		}

		public InstantiateAsyncContext InstantiateAsync(Transform parent = null, bool stayWorldPosition = false) {
			return new InstantiateAsyncContext(this, parent, stayWorldPosition);
		}

		public InstantiateAsyncContext InstantiateAsync(Vector3 position) {
			return new InstantiateAsyncContext(this, position, Quaternion.identity, null);
		}

		public InstantiateAsyncContext InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null) {
			return new InstantiateAsyncContext(this, position, rotation, parent);
		}

		public void InitPool(string name, int prefer, int max) {
			if( Asset is not PrefabAssetInstance prefabAsset ) {
				return;
			}

			prefabAsset.InitPool(name, prefer, max);
		}

		public override string ToString() {
			return Asset == null || !Asset.UnityObject ? "Not loaded" : Asset.UnityObject.name;
		}

		public override int GetHashCode() {
			return m_assetRef.GetHashCode();
		}

		public override bool Equals(object obj) {
			var other = (AssetReference) obj;
			return other != null && other.m_assetRef == m_assetRef;
		}

		public void Dispose() {
			Asset = null;
			if( m_assetRef == null || !m_assetRef.IsValid() )
				return;

			m_assetRef.ReleaseAsset();
			m_assetRef = null;
		}
	}
}