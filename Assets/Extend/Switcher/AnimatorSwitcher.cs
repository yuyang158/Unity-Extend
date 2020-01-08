using System;
using Extend.Common;
using UnityEngine;

namespace Extend.Switcher {
	[Serializable]
	public class AnimatorSwitcher : ISwitcher {
		public AnimatorParamProcessor Processor; 

		public void ActiveSwitcher() {
			Processor.Apply();
		}
	}
}