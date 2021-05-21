using Extend.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XLua;

namespace Extend.Render {
	[LuaCallCSharp]
	public class RenderFeatureService : IService {
		public static RenderFeatureService Get() {
			return CSharpServiceManager.Get<RenderFeatureService>(CSharpServiceManager.ServiceType.RENDER_FEATURE);
		}

		public int ServiceType => (int)CSharpServiceManager.ServiceType.RENDER_FEATURE;
		private RenderTexture m_targetTexture;

		[BlackList]
		public void Initialize() {
		}

		[BlackList]
		public void Destroy() {
			if( m_targetTexture != null ) {
				Object.Destroy(m_targetTexture);
			}
		}

		public void SetFeatureActive(Camera camera, string name, bool active) {
			if( !camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraDataComponent) )
				return;

			var features = cameraDataComponent.scriptableRenderer.rendererFeatures;
			foreach( var feature in features ) {
				if( feature.name == name ) {
					feature.SetActive(active);
				}
			}
		}

		public T GetFeature<T>(Camera camera, string name) where T : ScriptableRendererFeature {
			if( !camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraDataComponent) )
				return null;

			var features = cameraDataComponent.scriptableRenderer.rendererFeatures;
			foreach( var feature in features ) {
				if( feature.name == name ) {
					return feature as T;
				}
			}

			return null;
		}

		public void SwitchRenderer(Camera camera, int index) {
			if( !camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraDataComponent) )
				return;

			cameraDataComponent.SetRenderer(index);
		}

		private static bool TryFindVolumeComponentWithName(Volume volume, string name, out VolumeComponent component) {
			foreach( var c in volume.profile.components ) {
				if( c.GetType().Name == name ) {
					component = c;
					return true;
				}
			}

			component = null;
			return false;
		}

		private static bool TryOverrideVolumeComponentField<T>(Volume volume, string componentName, string field, T value) {
			if( !TryFindVolumeComponentWithName(volume, componentName, out var component) ) {
				Debug.LogWarning($"Not found postprocess component : {componentName}");
				return false;
			}

			var fieldInfo = component.GetType().GetField(field);
			if( fieldInfo == null ) {
				Debug.LogWarning($"Field {field} not exist in component : {componentName}");
				return false;
			}

			var parameter = fieldInfo.GetValue(component);
			if( parameter == null ) {
				Debug.LogWarning($"Not found component {componentName} parameter : {field}");
				return false;
			}

			if( !( parameter is VolumeParameter<T> typedParam ) ) {
				Debug.LogWarning($"component {componentName} parameter : {field} is not {nameof(VolumeParameter<T>)}");
				return false;
			}

			typedParam.Override(value);
			return true;
		}

		public void SetVolumeComponentActive(Volume volume, string name, bool active) {
			if( !TryFindVolumeComponentWithName(volume, name, out var component) ) {
				Debug.LogWarning($"Not found postprocess component : {name}");
				return;
			}

			component.active = active;
		}

		public void SetVolumeComponentIntParam(Volume volume, string componentName, string field, int value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}

		public void SetVolumeComponentFloatParam(Volume volume, string componentName, string field, float value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}

		public void SetVolumeComponentVector2Param(Volume volume, string componentName, string field, Vector2 value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}

		public void SetVolumeComponentVector3Param(Volume volume, string componentName, string field, Vector3 value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}

		public void SetVolumeComponentVector4Param(Volume volume, string componentName, string field, Vector4 value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}

		public void SetVolumeComponentColorParam(Volume volume, string componentName, string field, Color value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}

		public void SetVolumeComponentTextureParam(Volume volume, string componentName, string field, Texture value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}
		
		public void SetVolumeComponentBoolParam(Volume volume, string componentName, string field, bool value) {
			TryOverrideVolumeComponentField(volume, componentName, field, value);
		}
	}
}