namespace Extend.Asset {
	public class DirectDestroyGO : AssetServiceManagedGO {
		internal override void Recycle() {
			DestroyImmediate(gameObject);
			base.Recycle();
		}
	}
}