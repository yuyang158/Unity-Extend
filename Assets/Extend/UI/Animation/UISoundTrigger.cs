using System;
using Extend.Asset;
using Extend.Asset.Attribute;
using UnityEngine;
using UnityEngine.Audio;

namespace Extend.UI.Animation {
	[Serializable]
	public class UISoundTrigger {
		[AssetReferenceAssetType(AssetType = typeof(AudioClip))]
		private AssetReference m_audioClip;

		private AssetReference m_audioMixer;

		public void Active() {
			var clip = m_audioClip.GetAudioClip();
			
		}
	}
}