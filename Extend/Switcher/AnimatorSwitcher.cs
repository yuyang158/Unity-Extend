using System;
using UnityEngine;

namespace Extend.Switcher {
	[Serializable]
	public class AnimatorSwitcher : ISwitcher {
		public Animator Ani;
		public AnimatorParameterValue Value;

		[Serializable]
		public class AnimatorParameterValue {
			public int NameHash;
			public float fV;
			public int iV;
			public bool bV;
		}

		public void ActiveSwitcher() {
			foreach( var parameter in Ani.parameters ) {
				if( parameter.nameHash != Value.NameHash ) continue;
				switch( parameter.type ) {
					case AnimatorControllerParameterType.Float:
						Ani.SetFloat(Value.NameHash, Value.fV);
						break;
					case AnimatorControllerParameterType.Int:
						Ani.SetInteger(Value.NameHash, Value.iV);
						break;
					case AnimatorControllerParameterType.Bool:
						Ani.SetBool(Value.NameHash, Value.bV);
						break;
					case AnimatorControllerParameterType.Trigger:
						Ani.SetTrigger(Value.NameHash);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}