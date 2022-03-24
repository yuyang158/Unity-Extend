using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.Tween {
	public interface IMaterialTweenSetup {
		void Setup(Material material, MaterialPropertyBlock block);
	}
	
	[Serializable]
	public abstract class MaterialTweenValue<T> : TweenValueBase<T>, IMaterialTweenSetup {
		protected MaterialPropertyBlock m_block;

		[SerializeField]
		private string m_materialPropertyName;

		public string PropertyName {
			set => m_materialPropertyName = value;
		}

		protected int m_materialPropertyHash;

		public virtual void Setup(Material material, MaterialPropertyBlock block) {
			m_block = block;
			m_materialPropertyHash = Shader.PropertyToID(m_materialPropertyName);
		}
	}

	[Serializable]
	public class MaterialVector4TweenValue : MaterialTweenValue<Vector4> {
		public override void Setup(Material material, MaterialPropertyBlock block) {
			base.Setup(material, block);
			m_startValue = material.GetVector(m_materialPropertyHash);
		}

		protected override void Reset() {
			m_block.SetVector(m_materialPropertyHash, m_startValue);
		}

		protected override Vector4 Getter() {
			return m_block.GetVector(m_materialPropertyHash);
		}

		protected override void Setter(Vector4 val) {
			m_block.SetVector(m_materialPropertyHash, val);
		}

		protected override Tweener DoPlay() {
			return DOTween.To(Getter, Setter, m_endValue, m_duration);
		}
	}
	
	[Serializable]
	public class MaterialColorTweenValue : MaterialTweenValue<Color> {
		public override void Setup(Material material, MaterialPropertyBlock block) {
			base.Setup(material, block);
			m_startValue = material.GetColor(m_materialPropertyHash);
		}

		protected override void Reset() {
			m_block.SetColor(m_materialPropertyHash, m_startValue);
		}

		protected override Color Getter() {
			return m_block.GetColor(m_materialPropertyHash);
		}

		protected override void Setter(Color val) {
			m_block.SetColor(m_materialPropertyHash, val);
		}

		protected override Tweener DoPlay() {
			return DOTween.To(Getter, Setter, m_endValue, m_duration);
		}
	}
	
	[Serializable]
	public class MaterialFloatTweenValue : MaterialTweenValue<float> {
		public override void Setup(Material material, MaterialPropertyBlock block) {
			base.Setup(material, block);
			m_startValue = material.GetFloat(m_materialPropertyHash);
		}

		protected override void Reset() {
			m_block.SetFloat(m_materialPropertyHash, m_startValue);
		}

		protected override float Getter() {
			return m_block.GetFloat(m_materialPropertyHash);
		}

		protected override void Setter(float val) {
			m_block.SetFloat(m_materialPropertyHash, val);
		}

		protected override Tweener DoPlay() {
			return DOTween.To(Getter, Setter, m_endValue, m_duration);
		}
	}
}