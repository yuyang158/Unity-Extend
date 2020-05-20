using System;
using UnityEngine;

namespace Extend.Common {
	public class AssetPathAttribute : PropertyAttribute {
		public Type AssetType;
		public string RootDir;
		public string Extension;
	}
}