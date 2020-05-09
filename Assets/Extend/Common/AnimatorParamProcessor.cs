using System;
using UnityEngine;
using UnityEngine.Serialization;

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
		private AnimatorParameterValue m_paramValue;

		public AnimatorParameterValue ParameterValue => m_paramValue;

		public void Apply() {
			foreach( var parameter in Ani.parameters ) {
				if( parameter.nameHash != m_paramValue.NameHash ) continue;
				switch( parameter.type ) {
					case AnimatorControllerParameterType.Float:
						Ani.SetFloat(m_paramValue.NameHash, m_paramValue.fV);
						break;
					case AnimatorControllerParameterType.Int:
						Ani.SetInteger(m_paramValue.NameHash, m_paramValue.iV);
						break;
					case AnimatorControllerParameterType.Bool:
						Ani.SetBool(m_paramValue.NameHash, m_paramValue.bV);
						break;
					case AnimatorControllerParameterType.Trigger:
						Ani.SetTrigger(m_paramValue.NameHash);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}