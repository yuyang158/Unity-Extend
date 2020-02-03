using System;
using Extend.Common;

namespace Extend.Switcher {
	[Serializable]
	public class AnimatorSwitcher : ISwitcher {
		public AnimatorParamProcessor Processor; 

		public void ActiveSwitcher() {
			Processor.Apply();
		}
	}
}