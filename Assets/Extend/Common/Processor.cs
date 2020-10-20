using System;
using DG.Tweening;
using UnityEngine;
using XLua;

namespace Extend.Common {
	[LuaCallCSharp]
	public class Processor : MonoBehaviour {
		public float MinValue;
		public float MaxValue = 1;

		[SerializeField]
		private float m_value;

		[SerializeField]
		private bool m_animateValue;

		public float Duration;

		public Ease Ease = DOTween.defaultEaseType;

		private float m_displayValue;

		public float Value {
			get => m_value;
			set {
				if( value < MinValue ) {
					value = MinValue;
				}

				if( value > MaxValue ) {
					value = MaxValue;
				}

				m_value = value;
				if( !m_animateValue ) {
					m_displayValue = value;
					ApplyValue();
				}
			}
		}

		[SerializeField, ReorderList]
		private ProcessorTargetBase[] m_targets;

		private void OnEnable() {
			m_displayValue = Value;
			ApplyValue();
		}

		private void ApplyValue() {
			if( !enabled )
				return;
			foreach( var target in m_targets ) {
				target.ApplyProgress(m_displayValue);
			}
		}

		private void Update() {
			if( Math.Abs(m_displayValue - Value) < Mathf.Epsilon )
				return;

			DOTween.To(() => m_displayValue, x => {
				m_displayValue = x;
				ApplyValue();
			}, Value, Duration).SetEase(Ease);
		}
	}
}