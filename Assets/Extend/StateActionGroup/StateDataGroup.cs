using System;
using Extend.StateActionGroup.Behaviour;
using UnityEngine;

namespace Extend.StateActionGroup {
	[Serializable]
	public class StateDataGroup {
		public string StateName;

		[SerializeReference]
		public BehaviourDataBase[] DataArray;

		public IBehaviourData FindById(int id) {
			foreach( IBehaviourData data in DataArray ) {
				if( data.GetTargetId() == id ) {
					return data;
				}
			}

			return null;
		}

		public void CopyBehaviourData(SAG sag) {
			DataArray = new BehaviourDataBase[sag.Behaviours.Length];
			for( int i = 0; i < sag.Behaviours.Length; i++ ) {
				DataArray[i] = sag.Behaviours[i].CreateDefaultData();
			}
		}
	}
}