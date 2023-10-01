using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.DoTween {
	public class DoTweenMaterialPlayer : MonoBehaviour {
		[SerializeField]
		private Renderer m_targetRenderer;
		[SerializeField]
		private int m_propertyId;
		[SerializeField]
		private ShaderPropertyType m_propertyType;
		[SerializeField]
		private float m_floatEndValue;
		[SerializeField]
		private Vector4 m_vectorEndValue;
		[SerializeField]
		private Color m_colorEndValue = Color.white;

		private float m_floatStartValue;
		private Vector4 m_vectorStartValue;
		private Color m_colorStartValue = Color.white;

		[SerializeField]
		private float m_duration = 1;
		[SerializeField]
		private Ease m_ease = Ease.Linear;
		[SerializeField]
		private float m_delay;
		[SerializeField]
		private bool m_autoKill;

		private Tweener m_tweener;
		private MaterialPropertyBlock m_propertyBlock;

		public Renderer TargetRenderer => m_targetRenderer;

		public bool Complete => m_tweener == null || m_tweener.IsComplete();

		public float FloatEndValue {
			get => m_floatEndValue;
			set => m_floatEndValue = value;
		}

		public Vector4 VectorEndValue {
			get => m_vectorEndValue;
			set => m_vectorEndValue = value;
		}

		public Color ColorEndValue {
			get => m_colorEndValue;
			set => m_colorEndValue = value;
		}

		public float Duration {
			get => m_duration;
			set => m_duration = value;
		}

		public Ease Ease {
			get => m_ease;
			set => m_ease = value;
		}

		public float Delay {
			get => m_delay;
			set => m_delay = value;
		}

		public bool AutoKill {
			get => m_autoKill;
			set => m_autoKill = value;
		}

		private void Awake() {
			m_propertyBlock = MaterialPropertyBlockPool.RequestBlock(m_targetRenderer);
		}

		public void Play() {
			if( m_tweener is {active: true} ) {
				m_tweener.Kill(true);
				m_tweener = null;
			}
			
			switch( m_propertyType ) {
				case ShaderPropertyType.Color:
					m_colorStartValue = m_propertyBlock.GetColor(m_propertyId);
					m_tweener = DOTween.To(() => m_colorStartValue, x => m_propertyBlock.SetColor(m_propertyId, x), ColorEndValue, Duration);
					break;
				case ShaderPropertyType.Vector:
					m_vectorStartValue = m_propertyBlock.GetVector(m_propertyId);
					m_tweener = DOTween.To(() => m_vectorStartValue, x => m_propertyBlock.SetVector(m_propertyId, x), VectorEndValue, Duration);
					break;
				case ShaderPropertyType.Float:
				case ShaderPropertyType.Range:
					m_floatStartValue = m_propertyBlock.GetFloat(m_propertyId);
					m_tweener = DOTween.To(() => m_floatStartValue, x => {
						m_propertyBlock.SetFloat(m_propertyId, x);
						TargetRenderer.SetPropertyBlock(m_propertyBlock);
					}, FloatEndValue, Duration);
					break;
				case ShaderPropertyType.Texture:
				case ShaderPropertyType.Int:
				default:
					throw new ArgumentOutOfRangeException();
			}

			if( m_tweener == null ) {
				return;
			}
			m_tweener.SetEase(Ease).SetDelay(Delay).SetAutoKill(AutoKill);
			m_tweener.onComplete += () => {
				switch( m_propertyType ) {
					case ShaderPropertyType.Color:
						m_propertyBlock.SetColor(m_propertyId, m_colorStartValue);
						break;
					case ShaderPropertyType.Vector:
						m_propertyBlock.SetVector(m_propertyId, m_vectorStartValue);
						break;
					case ShaderPropertyType.Float:
					case ShaderPropertyType.Range:
						m_propertyBlock.SetFloat(m_propertyId, m_floatStartValue);
						break;
					case ShaderPropertyType.Texture:
					case ShaderPropertyType.Int:
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
			m_tweener.Play();
		}

		public void Stop() {
			if( m_tweener is {active: true} ) {
				m_tweener.Kill(true);
				m_tweener = null;
			}
		}

		private void OnDestroy() {
			MaterialPropertyBlockPool.GiveUpRenderer(m_targetRenderer);
		}
	}
}