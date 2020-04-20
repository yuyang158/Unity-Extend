using System;

namespace Extend.Asset.AssetOperator {
	public class AssetOperators {
		private int currentOpIndex;
		public AssetOperatorBase[] Operators;

		public bool IsDone => currentOpIndex >= Operators.Length;

		public float Progress => currentOpIndex / (float)Operators.Length;

		private void DoExecute(AssetAsyncLoadHandle handle, Type typ) {
			if(currentOpIndex >= Operators.Length)
				return;
			
			Operators[currentOpIndex].Execute(handle, typ);
		}

		public void Execute(AssetAsyncLoadHandle handle, Type typ) {
			foreach( var op in Operators ) {
				op.OnComplete += _ => {
					currentOpIndex += 1;
					DoExecute(handle, typ);
				};
			}

			DoExecute(handle, typ);
		}
	}
	
	public abstract class AssetOperatorBase {
		public Action<AssetOperatorBase> OnComplete;
		public abstract void Execute(AssetAsyncLoadHandle handle, Type typ);
	}
}