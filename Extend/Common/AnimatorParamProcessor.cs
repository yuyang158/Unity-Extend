using System;
using UnityEngine;

namespace Extend.Common {
	[Serializable]
	public class AnimatorParamProcessor {
		[Serializable]
		public class AnimatorParameterValue {
			public int NameHash;
			public float fV;
			public int iV;
			public bool bV;
		}
		
		[SerializeField, HideInInspector]
		private Animator ani;

		public Animator Ani => ani;

		[SerializeField, HideInInspector]
		private AnimatorParameterValue paramValue;

		public AnimatorParameterValue ParameterValue => paramValue;

		public void Apply() {
			foreach( var parameter in Ani.parameters ) {
				if( parameter.nameHash != paramValue.NameHash ) continue;
				switch( parameter.type ) {
					case AnimatorControllerParameterType.Float:
						Ani.SetFloat(paramValue.NameHash, paramValue.fV);
						break;
					case AnimatorControllerParameterType.Int:
						Ani.SetInteger(paramValue.NameHash, paramValue.iV);
						break;
					case AnimatorControllerParameterType.Bool:
						Ani.SetBool(paramValue.NameHash, paramValue.bV);
						break;
					case AnimatorControllerParameterType.Trigger:
						Ani.SetTrigger(paramValue.NameHash);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}