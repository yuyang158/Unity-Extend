using System;
using Extend.Asset;

namespace Extend.EventAsset
{
	[Serializable]
	public class EventParam {
		public enum ParamType : byte {
			None,
			Int,
			Float,
			String,
			AssetRef
		}

		public int Int;
		public float Float;
		public string Str;
		public AssetReference AssetRef;
		public ParamType Type;
	}
}