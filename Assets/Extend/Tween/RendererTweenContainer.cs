using System;
using UnityEngine;

namespace Extend.Tween {
	public class RendererTweenContainer : TweenContainer {
		[SerializeField]
		private Renderer m_renderer;

		private MaterialPropertyBlock m_block;

		[SerializeReference, HideInInspector]
		private ITweenValue[] m_values;
		public ITweenValue[] Values {
			set => m_values = value;
			get => m_values;
		}

		private void Awake() {
			m_block = new MaterialPropertyBlock();
			m_renderer.GetPropertyBlock(m_block);
			foreach( var tweenValue in m_values ) {
				var value = tweenValue as IMaterialTweenSetup;
				value.Setup(m_renderer.sharedMaterial, m_block);
			}

			enabled = false;
		}

		public void Play() {
			m_renderer.SetPropertyBlock(m_block);
			foreach( var value in m_values ) {
				value.Play();
			}
			enabled = true;
		}

		private void Update() {
			m_renderer.SetPropertyBlock(m_block);
			foreach( var value in m_values ) {
				if( !value.Complete() ) {
					return;
				}
			}

			enabled = false;
		}
	}
}