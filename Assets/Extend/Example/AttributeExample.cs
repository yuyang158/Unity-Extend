using Extend.Asset;
using Extend.Asset.Attribute;
using Extend.Common;
using UnityEngine;

namespace Extend.Example {
	public class AttributeExample : MonoBehaviour {
		public enum EnumType {
			One,
			Two,
			Three,
			Four
		}
		
		[HideIf("B", 2)]
		public int A;

		[ShowIf("C", 1)]
		public int B;

		[LabelText("Label Text")]
		public int C;

		[ReorderList]
		public int[] D;

		[Require, AssetOnly]
		public GameObject E;

		[AssetReferenceAssetType(AssetType = typeof(GameObject))]
		public AssetReference Asset;

		[EnumToggleButtons]
		public EnumType EnumValue;

		[Button(ButtonSize.Medium)]
		public static void Click() {
			Debug.Log("Click!");
		}
	}
}