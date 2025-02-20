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

		public virtual bool RootMotionCommunicator => false;

		private int m_enabledFrame;
		public int EnabledFrame => m_enabledFrame;

		private void OnEnable() {
			m_enabledFrame = Time.frameCount;
		}

		private void OnDestroy() {
			ParameterSummary.Dispose();
		}

		private void Awake() {
			Animator = GetComponent<Animator>();

			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			ParameterSummary = luaVM.NewTable();
			if( !Animator.runtimeAnimatorController ) {
				return;
			}
			var originController = Animator.runtimeAnimatorController as AnimatorOverrideController;
			if( originController ) {
				List<KeyValuePair<AnimationClip, AnimationClip>> overrides =
					new List<KeyValuePair<AnimationClip, AnimationClip>>(originController.overridesCount);
				originController.GetOverrides(overrides);

				var clonedController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
				clonedController.ApplyOverrides(overrides);
				Animator.runtimeAnimatorController = clonedController;
			}
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

		public void Play(int nameHash, int layer = 0) {
			if( !Animator.HasState(layer, nameHash) ) {
				return;
			}
			Animator.Play(nameHash, layer);
		}

		public void Evaluate(float deltaTime) {
			if( !gameObject.activeInHierarchy ) {
				return;
			}

			Animator.Update(deltaTime);
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

		public void ResetTrigger(int nameHash) {
			Animator.ResetTrigger(nameHash);
		}

		public void ChangeAnimatorController(RuntimeAnimatorController controller) {
			Animator.runtimeAnimatorController = controller;
		}

		public void ChangeClip(string clipName, AnimationClip clip) {
			var controller = Animator.runtimeAnimatorController as AnimatorOverrideController;
			controller[clipName] = clip;
		}

		public bool IsInTransition(int layerIndex) {
			return Animator.IsInTransition(layerIndex);
		}

		public void MoveToAnimationClipEvent(AnimationClip clip, string eventName, int layer = -1) {
			var events = clip.events;
			foreach( AnimationEvent e in events ) {
				var evtInstance = e.objectReferenceParameter as EventInstance;
				if( evtInstance.name == eventName ) {
					var state = e.animatorStateInfo;
					Animator.Play(state.fullPathHash, -1, e.time / clip.length);
					Animator.Update(0);
					break;
				}
			}
		}

		public void OnEvent(EventInstance evt) {
			if( !evt ) {
				Debug.LogError("Animation Event Instance Is Null." + name);
				return;
			}

			if( m_pauseEventFrame == Time.frameCount ) {
				return;
			}

			var callback = ParameterSummary.Get<LuaFunction>(evt.EventName);
			callback?.Action(evt.Value);
		}

		public void OnEvent_String(string evt) {
			var lastIndex = evt.LastIndexOf('_');
			var clipName = evt[..lastIndex];
			var value = evt[( lastIndex + 1 )..];
			var callback = ParameterSummary.Get<LuaFunction>("OnEvent_Check");
			callback?.Action(value, clipName);
		}

		public int GetLayerCurrentStateNameHash(int layerIndex) {
			var stateInfo = Animator.GetCurrentAnimatorStateInfo(layerIndex);
			return stateInfo.shortNameHash;
		}

		public void GetLayerCurrentStateFullNameHashAndTime(int layerIndex, out int fullNameHash,
			out float normalizedTime) {
			if( Animator.IsInTransition(layerIndex) ) {
				var stateInfo = Animator.GetNextAnimatorStateInfo(layerIndex);
				fullNameHash = stateInfo.fullPathHash;
				normalizedTime = stateInfo.normalizedTime;
			}
			else {
				var stateInfo = Animator.GetCurrentAnimatorStateInfo(layerIndex);
				fullNameHash = stateInfo.fullPathHash;
				normalizedTime = stateInfo.normalizedTime;
			}
		}

		public void SetLayerWeight(int layer, float weight) {
			if( layer >= Animator.layerCount ) {
				return;
			}
			Animator.SetLayerWeight(layer, weight);
		}

		public void SetUpdateMode(int updateMode) {
			Animator.updateMode = (AnimatorUpdateMode) updateMode;
		}

		public bool HasState(int layer, int stateID) {
			return Animator.HasState(layer, stateID);
		}

		private int m_pauseEventFrame;

		public void PauseOneFrameFireEvent() {
			m_pauseEventFrame = Time.frameCount;
		}

		public float Speed {
			get => Animator.speed;
			set => Animator.speed = value;
		}

		public virtual void SetRootMotionActivate(bool activate, OnRootMotionUpdate rootMotionUpdate = null) {
		}
	}
}