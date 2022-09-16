using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.EventAsset;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public delegate void OnRootMotionUpdate(Vector3 deltaPosition, Quaternion deltaRotation);
	
	[RequireComponent(typeof(Animator)), LuaCallCSharp]
	public class AnimatorParameterLuaCommunicator : MonoBehaviour {
		public LuaTable ParameterSummary { get; private set; }
		public Animator Animator { get; private set; }

		private OnRootMotionUpdate m_rootMotionUpdate;

		private void Awake() {
			Animator = GetComponent<Animator>();
			var originController = Animator.runtimeAnimatorController as AnimatorOverrideController;
			List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(originController.overridesCount);
			originController.GetOverrides(overrides);

			var clonedController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
			clonedController.ApplyOverrides(overrides);
			Animator.runtimeAnimatorController = clonedController;
			
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			ParameterSummary = luaVM.NewTable();
			for( int i = 0; i < Animator.parameterCount; i++ ) {
				AnimatorControllerParameter parameter = Animator.GetParameter(i);
				LuaTable context = luaVM.NewTable();
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

		private void OnAnimatorMove() {
			m_rootMotionUpdate?.Invoke(Animator.deltaPosition, Animator.deltaRotation);
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

		public void ChangeAnimatorController(RuntimeAnimatorController controller) {
			Animator.runtimeAnimatorController = controller;
		}

		public void ChangeClip(string clipName, AnimationClip clip) {
			var controller = Animator.runtimeAnimatorController as AnimatorOverrideController;
			controller[clipName] = clip;
		}

		public void SetRootMotionActivate(bool activate, OnRootMotionUpdate rootMotionUpdate = null) {
			Animator.applyRootMotion = activate;
			m_rootMotionUpdate = rootMotionUpdate;
		}

		public void OnEvent(EventInstance evt) {
			var callback = ParameterSummary.Get<Action<LuaTable, object>>(evt.EventName);
			callback?.Invoke(ParameterSummary, evt.Value);
		}

		public int GetLayerCurrentStateNameHash(int layerIndex) {
			var stateInfo = Animator.GetCurrentAnimatorStateInfo(layerIndex);
			return stateInfo.shortNameHash;
		}
	}
}