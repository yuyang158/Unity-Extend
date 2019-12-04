using System;
using UnityEngine;

namespace Extend.LuaBindingData {
	public class AssetPathAttribute : PropertyAttribute {
		public Type AssetType;
		public string RootDir;
		public string Extension;
	}
}