using System;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	[CSharpCallLua]
	public delegate void OnInstantiateComplete(GameObject go);

	[Serializable, LuaCallCSharp]
	public class AssetReference : IDisposable, ICloneable {
		[SerializeField, HideInInspector]
		private string m_assetGUID;

#if UNITY_EDITOR
		public string AssetGUID => m_assetGUID;
#endif
		public AssetRefObject.AssetStatus AssetStatus => Asset?.Status ?? AssetRefObject.AssetStatus.NONE;
		public bool IsFinished => Asset?.IsFinished ?? false;
		private AssetInstance m_asset;

		public AssetInstance Asset {
			get => m_asset;
			private set {
				m_asset?.Release();
				m_asset = value;
				m_asset?.IncRef();
			}
		}

		public AssetReference(AssetInstance instance) {
			Asset = instance;
		}

		public AssetReference(string assetGUID) {
			m_assetGUID = assetGUID;
#if UNITY_EDITOR
			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
			if( string.IsNullOrEmpty(path) ) {
				Debug.LogError($"GUID is not valid {assetGUID}");
			}
#endif
		}

		public bool GUIDValid {
			get {
#if UNITY_EDITOR
				return !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GUIDToAssetPath(m_assetGUID));
#else
				return !string.IsNullOrEmpty(m_assetGUID);
#endif
			}
		}

		private T GetAsset<T>() where T : Object {
			if( Asset == null ) {
				Asset = AssetService.Get().LoadAssetWithGUID<T>(m_assetGUID);
			}

			if( Asset.Status != AssetRefObject.AssetStatus.DONE ) {
				Debug.LogError($"Load failed : {Asset.AssetPath}");
			}

			return Asset.UnityObject as T;
		}

		public Object GetObject() {
			return GetAsset<Object>();
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

		public T GetScriptableObject<T>() where T : ScriptableObject {
			return GetAsset<T>();
		}

		public AssetAsyncLoadHandle LoadAsync(Type typ) {
			var handle = AssetService.Get().LoadAsyncWithGUID(m_assetGUID, typ);
			Assert.IsNotNull(handle.Asset);
			Asset = handle.Asset;
			return handle;
		}

		public GameObject Instantiate(Transform parent = null, bool stayWorldPosition = false) {
			if( Asset == null ) {
				Asset = AssetService.Get().LoadAssetWithGUID<GameObject>(m_assetGUID);
			}

			if( !( Asset is PrefabAssetInstance prefabAsset ) ) {
				Debug.LogError($"{Asset.AssetPath} is not a prefab!");
				return null;
			}

			return prefabAsset.Instantiate(parent, stayWorldPosition);
		}

		public GameObject Instantiate(Vector3 position) {
			return Instantiate(position, Quaternion.identity);
		}

		public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent = null) {
			if( Asset == null ) {
				Asset = AssetService.Get().LoadAssetWithGUID<GameObject>(m_assetGUID);
			}

			if( !( Asset is PrefabAssetInstance prefabAsset ) ) {
				Debug.LogError($"{Asset.AssetPath} is not a prefab!");
				return null;
			}

			return prefabAsset.Instantiate(position, rotation, parent);
		}

		[LuaCallCSharp]
		public class InstantiateAsyncContext {
			public AssetReference Ref;
			public OnInstantiateComplete Callback;
			public bool Cancel;

			public Transform Parent;
			public bool StayWorldPosition;
			public Vector3 Position;
			public Quaternion Rotation;

			private int m_ctorType;

			public InstantiateAsyncContext(Transform parent, bool stayWorldPosition) {
				Parent = parent;
				StayWorldPosition = stayWorldPosition;
				m_ctorType = 1;
			}

			public InstantiateAsyncContext(Vector3 position, Quaternion rotation, Transform parent) {
				m_ctorType = 2;
				Parent = parent;
				Position = position;
				Rotation = rotation;
			}

			public void Instantiate() {
				if( Cancel )
					return;
				if( !( Ref.Asset is PrefabAssetInstance prefabAsset ) ) {
					Debug.LogError($"{Ref.Asset.AssetPath} is not a prefab!");
					return;
				}

				var go = m_ctorType == 1 ? prefabAsset.Instantiate(Parent, StayWorldPosition) : 
					prefabAsset.Instantiate(Position, Rotation, Parent);
				Callback(go);
			}
		}

		public InstantiateAsyncContext InstantiateAsync(Transform parent = null, bool stayWorldPosition = false) {
			return new InstantiateAsyncContext(parent, stayWorldPosition);
		}

		public InstantiateAsyncContext InstantiateAsync(Vector3 position) {
			return new InstantiateAsyncContext(position, Quaternion.identity, null);
		}

		public InstantiateAsyncContext InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null) {
			return new InstantiateAsyncContext(position, rotation, parent);
		}

		public void InitPool(string name, int prefer, int max) {
			if( !( Asset is PrefabAssetInstance prefabAsset ) ) {
				Debug.LogError($"{Asset.AssetPath} is not a prefab!");
				return;
			}

			prefabAsset.InitPool(name, prefer, max);
		}

		public override string ToString() {
			return Asset == null || !Asset.UnityObject ? "Not loaded" : Asset.UnityObject.name;
		}

		public object Clone() {
			return new AssetReference(Asset) {
				m_assetGUID = m_assetGUID
			};
		}

		public void Dispose() {
			Asset = null;
		}
	}
}