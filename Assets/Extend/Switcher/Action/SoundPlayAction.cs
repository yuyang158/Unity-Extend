using System;
using InGame.Sound;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class SoundPlayAction : SwitcherAction {
		[SerializeField]
		private SoundEffectPlayer m_audioSource;

		[SerializeField]
		private string m_clipName;
		public override void ActiveAction() {
			m_audioSource.PlayOneShot(m_clipName);
		}

		public override void DeactiveAction() {
		}
	}
}