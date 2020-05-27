namespace Extend.Asset {
	public class DirectDestroyGO : AssetServiceManagedGO {
		public override void Recycle() {
			Destroy(gameObject);
			base.Recycle();
		}
	}
}