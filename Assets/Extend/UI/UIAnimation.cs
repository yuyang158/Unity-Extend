using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Extend.Common;
using Extend.UI.Animation;
using Extend.UI.Attributes;
using UnityEngine;

namespace Extend.UI {
	[Serializable]
	public class UIAnimation : IUIAnimationPreview, IUITriggerPreview {
		public enum AnimationMode {
			ANIMATOR,
			PUNCH,
			STATE
		}

		public AnimationMode Mode = AnimationMode.PUNCH;

		[SerializeField]
		private bool m_enabled;

		public bool Enabled => m_enabled;

		[SerializeField]
		private PunchCombined m_punch;

		[SerializeField]
		private StateCombine m_state;

		[SerializeField]
		private AnimatorParamProcessor m_processor;
		
		[SerializeReference]
		private IUITriggerExecutor[] m_executors;

		private HashSet<UITriggerMode> m_triggerModes;
		public UIAnimation()
		{
			// 自行添加所需trigger
			m_triggerModes = new HashSet<UITriggerMode>() {UITriggerMode.Criware, };
			UITriggerUtil.UITriggerConstructorHandler(out m_executors, m_triggerModes);
		}

		public Tween[] Active(Transform t) {
			switch( Mode ) {
				case AnimationMode.PUNCH:
					m_punch.Active(t);
					return m_punch.AllTween;
				case AnimationMode.STATE:
					m_state.Active(t);
					return m_state.AllTween;
				case AnimationMode.ANIMATOR:
					m_processor.Apply();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return null;
		}

		public void CacheStartValue(Transform t) {
			if( Mode == AnimationMode.STATE ) {
				m_state.CacheStartValue(t);
			}
			else if( Mode == AnimationMode.PUNCH ) {
				m_punch.CacheStartValue(t);
			}
		}

		public Tween[] CollectPreviewTween(Transform transform) {
			return Active(transform);
		}

		public void Editor_Recovery(Transform transform) {
			if( Mode == AnimationMode.STATE ) {
				m_state.Editor_Recovery(transform);
			}
			else if( Mode == AnimationMode.PUNCH ) {
				m_punch.Editor_Recovery(transform);
			}
		}

		public void ExecuteAtTrigger()
		{
			foreach (var executor in m_executors)
			{
				executor?.Execute();
			}
		}
	}
}