﻿using System;
using System.Collections;

namespace Extend.AssetService.AssetOperator {
	public class AssetOperators {
		private int currentOpIndex;
		public AssetOperatorBase[] Operators;

		public bool IsDone => currentOpIndex >= Operators.Length;

		public float Progress => currentOpIndex / (float)Operators.Length;

		private void DoExecute(AssetAsyncLoadHandle handle) {
			if(currentOpIndex >= Operators.Length)
				return;
			
			Operators[currentOpIndex].Execute(handle);
		}

		public void Execute(AssetAsyncLoadHandle handle) {
			foreach( var op in Operators ) {
				op.OnComplete += _ => {
					currentOpIndex += 1;
					DoExecute(handle);
				};
			}

			DoExecute(handle);
		}
	}
	
	public abstract class AssetOperatorBase {
		public Action<AssetOperatorBase> OnComplete;
		public abstract void Execute(AssetAsyncLoadHandle handle);
	}
}