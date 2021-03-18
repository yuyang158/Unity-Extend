using System;
using System.Collections.Generic;
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
		[BlackList]
		public string AssetGUID => m_assetGUID;
#endif
		public AssetRefObject.AssetStatus AssetStatus => Asset?.Status ?? AssetRefObject.AssetStatus.NONE;
		public bool IsFinished => Asset?.IsFinished ?? false;
		private AssetInstance m_asset;

		public AssetInstance Asset {
			get => m_asset;
			private set {
				if( m_asset == value )
					return;

				m_asset?.Release();
				m_asset = value;
				m_asset?.IncRef();
			}
		}

		public AssetReference() {
			m_asset = null;
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
				Debug.LogError($"Load failed : {Asset.AssetPath}   {Asset.Status}");
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
			private AssetReference m_ref;

			[BlackList]
			private AssetReference Ref {
				set {
					m_ref = value;
					if( !GetAssetReady() ) {
						m_ref.LoadAsync(typeof(GameObject));
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
				return Ref.Asset != null && Ref.Asset.IsFinished;
			}

			public void Instantiate() {
				if( Cancel )
					return;
				if( !( Ref.Asset is PrefabAssetInstance prefabAsset ) ) {
					Debug.LogError($"{Ref.Asset.AssetPath} is not a prefab!");
					return;
				}

				var go = m_ctorType == 1 ? prefabAsset.Instantiate(m_parent, m_stayWorldPosition) : 
					prefabAsset.Instantiate(m_position, m_rotation, m_parent);
				Callback?.Invoke(go);
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