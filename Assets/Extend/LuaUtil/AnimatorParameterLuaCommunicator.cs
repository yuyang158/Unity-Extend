using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[RequireComponent(typeof(Animator)), LuaCallCSharp]
	public class AnimatorParameterLuaCommunicator : MonoBehaviour {
		public LuaTable ParameterSummary { get; private set; }
		private Animator Animator { get; set; }

		private void Awake() {
			Animator = GetComponent<Animator>();
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			ParameterSummary = luaVM.NewTable();

			foreach( var parameter in Animator.parameters ) {
				var context = luaVM.NewTable();
				ParameterSummary.Set(parameter.name, context);
				switch( parameter.type ) {
					case AnimatorControllerParameterType.Float:
						context.Set("type", 1);
						break;
					case AnimatorControllerParameterType.Int:
						context.Set("type", 2);
						break;
					case AnimatorControllerParameterType.Bool:
						context.Set("type", 3);
						break;
					case AnimatorControllerParameterType.Trigger:
						context.Set("type", 4);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				context.Set("hash", parameter.nameHash);
			}
		}

		public void Play(int nameHash, int layer = 0) {
			Animator.Play(nameHash, layer);
		}

		public void CrossFade(int nameHash, float transitionDuration, int layer = 0) {
			Animator.CrossFade(nameHash, transitionDuration, layer);
		}

		public float GetFloat(int nameHash) {
			return Animator.GetFloat(nameHash);
		}

		public int GetInteger(int nameHash) {
			return Animator.GetInteger(nameHash);
		}

		public bool GetBool(int nameHash) {
			return Animator.GetBool(nameHash);
		}

		public void SetFloat(int nameHash, float v) {
			Animator.SetFloat(nameHash, v);
		}
		
		public void SetInteger(int nameHash, int v) {
			Animator.SetInteger(nameHash, v);
		}
		
		public void SetBool(int nameHash, bool v) {
			Animator.SetBool(nameHash, v);
		}

		public void SetTrigger(int nameHash) {
			Animator.SetTrigger(nameHash);
		}
	}
}