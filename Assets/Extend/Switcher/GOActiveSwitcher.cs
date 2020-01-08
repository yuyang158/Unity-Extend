using System;
using UnityEngine;

namespace Extend.Switcher {
	[Serializable]
	public class GOActiveSwitcher : ISwitcher {
		public GameObject GO;
		public bool Active;
		
		public void ActiveSwitcher() {
			GO.SetActive(Active);
		}
	}
}