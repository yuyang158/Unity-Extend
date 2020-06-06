namespace Extend.Asset {
	public class DirectDestroyGO : AssetServiceManagedGO {
		internal override void Recycle() {
			Destroy(gameObject);
			base.Recycle();
		}
	}
}