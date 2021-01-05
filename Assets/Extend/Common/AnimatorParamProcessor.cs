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
		
		[SerializeField]
		private Animator m_ani;

		[SerializeField]
		private AnimatorParameterValue m_paramValue;

		public void Apply() {
			foreach( var parameter in m_ani.parameters ) {
				if( parameter.nameHash != m_paramValue.NameHash ) continue;
				switch( parameter.type ) {
					case AnimatorControllerParameterType.Float:
						m_ani.SetFloat(m_paramValue.NameHash, m_paramValue.fV);
						break;
					case AnimatorControllerParameterType.Int:
						m_ani.SetInteger(m_paramValue.NameHash, m_paramValue.iV);
						break;
					case AnimatorControllerParameterType.Bool:
						m_ani.SetBool(m_paramValue.NameHash, m_paramValue.bV);
						break;
					case AnimatorControllerParameterType.Trigger:
						m_ani.SetTrigger(m_paramValue.NameHash);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}