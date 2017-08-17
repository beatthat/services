
namespace BeatThat
{
	/// <summary>
	/// TODO: change so that the request can be disposed immediately and the loaded clip disposed elsewhere/later
	/// </summary>
	public class ResourceDisposableItemRequest<T> : ResourceItemRequest<Disposable<T>, T>, Request<Disposable<T>>
		where T : UnityEngine.Object
	{
		public ResourceDisposableItemRequest(string path) : base(path, ResourceDisposableItemRequest<T>.AssetToItem) {}

		private static Disposable<T> AssetToItem(T asset)
		{
			return new ResourceAssetDisposable<T>(asset);
		}
	}
}