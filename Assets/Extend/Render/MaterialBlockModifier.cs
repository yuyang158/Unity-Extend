using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Extend.Render {
	[LuaCallCSharp, DisallowMultipleComponent]
	public class MaterialBlockModifier : MonoBehaviour {
		public Renderer[] Renderers;

		private MaterialPropertyBlock[] m_blocks;
		private bool m_materialCloned;

		interface ITweenProcess {
			bool Process(MaterialBlockModifier modifier);
		}

		public class MaterialFloatPropertyTween : ITweenProcess {
			public int PropId;
			public float StartValue;
			public float EndValue;
			public float Duration;
			public float TimeLast;

			public bool Process(MaterialBlockModifier modifier) {
				TimeLast += Time.deltaTime;
				if( TimeLast > Duration ) {
					modifier.SetFloat(PropId, EndValue);
					return true;
				}
				else {
					modifier.SetFloat(PropId, Mathf.Lerp(StartValue, EndValue, TimeLast / Duration));
					return false;
				}
			}
		}

		public class MaterialColorPropertyTween : ITweenProcess {
			public int PropId;
			public Color StartValue;
			public Color EndValue;
			public float Duration;
			public float TimeLast;

			public bool Process(MaterialBlockModifier modifier) {
				TimeLast += Time.deltaTime;
				if( TimeLast > Duration ) {
					modifier.SetColor(PropId, EndValue);
					return true;
				}
				else {
					modifier.SetColor(PropId, Color.Lerp(StartValue, EndValue, TimeLast / Duration));
					return false;
				}
			}
		}

		private readonly List<ITweenProcess> m_propertyTween = new List<ITweenProcess>();

		private void Awake() {
			m_blocks = new MaterialPropertyBlock[Renderers.Length];
			for( int i = 0; i < Renderers.Length; i++ ) {
				m_blocks[i] = new MaterialPropertyBlock();
			}
		}

		public void SetVisible(bool bVisible) {
			foreach( var renderer in Renderers ) {
				renderer.forceRenderingOff = !bVisible;
			}
		}

		public void SetFloat(int propId, float value) {
			foreach( MaterialPropertyBlock block in m_blocks ) {
				block.SetFloat(propId, value);
			}
		}

		public void SetVector(int propId, Vector4 value) {
			foreach( MaterialPropertyBlock block in m_blocks ) {
				block.SetVector(propId, value);
			}
		}

		public void SetColor(int propId, Color value) {
			foreach( MaterialPropertyBlock block in m_blocks ) {
				block.SetColor(propId, value);
			}
		}

		public void StartTweenFloat(int propId, float startValue, float endValue, float duration) {
			m_propertyTween.Add(new MaterialFloatPropertyTween() {
				PropId = propId,
				StartValue = startValue,
				EndValue = endValue,
				Duration = duration
			});
		}

		public void StartTweenColor(int propId, Color startValue, Color endValue, float duration) {
			m_propertyTween.Add(new MaterialColorPropertyTween() {
				PropId = propId,
				StartValue = startValue,
				EndValue = endValue,
				Duration = duration
			});
		}

		public void ModifySingleKeyword(string keyword, bool active, int index) {
			var materials = new List<Material>();
			var r = Renderers[index];
			r.GetMaterials(materials);
			r.SetMaterials(materials);

			foreach( Material material in materials ) {
				if( active ) {
					material.EnableKeyword(keyword);
				}
				else {
					material.DisableKeyword(keyword);
				}
			}
		}

		public void ModifyKeyword(string keyword, bool active) {
			try {
				var materials = new List<Material>();
				for( int i = 0; i < Renderers.Length; i++ ) {
					materials.Clear();
					var r = Renderers[i];
					if( !m_materialCloned ) {
						r.GetMaterials(materials);
						r.SetMaterials(materials);
					}
					else {
						r.GetSharedMaterials(materials);
					}

					foreach( Material material in materials ) {
						if( active ) {
							material.EnableKeyword(keyword);
						}
						else {
							material.DisableKeyword(keyword);
						}
					}
				}

				m_materialCloned = true;
			}
			catch( Exception e ) {
				Debug.LogException(e, this);
			}
		}

		private void LateUpdate() {
			if( m_blocks.Length == 0 ) {
				return;
			}

			for( int i = 0; i < m_blocks.Length; i++ ) {
				var block = m_blocks[i];
				if( block.isEmpty ) {
					continue;
				}
				
				var r = Renderers[i];
				r.SetPropertyBlock(block);
			}

			for( int i = 0; i < m_propertyTween.Count; ) {
				ITweenProcess propertyTween = m_propertyTween[i];
				if(propertyTween.Process(this)) {
					m_propertyTween.RemoveAt(i);
				}
				else {
					i++;
				}
			}
		}
	}
}