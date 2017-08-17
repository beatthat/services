using UnityEngine;
using System;
using System.Collections;

namespace BeatThat
{
	public class ResourceItemRequest<ItemType, ResourceType> : RequestBase, Request<ItemType> 
		where ItemType : class
		where ResourceType : UnityEngine.Object
	{
		public delegate ItemType AssetToItemDelegate(ResourceType asset);

		public ResourceItemRequest(string path, AssetToItemDelegate assetToItem) 
		{
			this.path = path;
			this.assetToItem = assetToItem;
		}

		protected AssetToItemDelegate assetToItem { get; set; }


		public object GetItem() { return this.item; } 

		public ResourceType asset { get; private set; }
		public ItemType item { get; private set; }

		public string path { get; private set; }

		private ResourceRequest req { get; set; }
		private GameObject runner { get; set; }

		#region ServiceRequest implementation

		override public float progress 
		{
			get {
				switch(this.status) {
				case RequestStatus.IN_PROGRESS:
					if(this.req == null) {
						return 0f;
					}
					return this.req.progress;
				case RequestStatus.DONE:
					return 1f;
				default:
					return 0f;
				}
			}
		}

		override protected void AfterCancel()
		{
			if(this.runner != null) {
				UnityEngine.Object.DestroyImmediate(this.runner);
			}

			UnityEngine.Object assetToUnload = this.asset;
			this.asset = null;
			this.item = null;

			// NOTE: handling of Dispose/Cancel is wrong (dispose should not unload asset)?

			// only unload if the object type is a file asset, e.g. a texure or audio file
			if(assetToUnload != null && !(assetToUnload is GameObject) && !(assetToUnload is Component)) {
				Resources.UnloadAsset(assetToUnload);
			}
		}

		override protected void ExecuteRequest()
		{
			this.runner = new GameObject("ResourceLoader-" + this.path);
			this.runner.AddComponent<RequestCoroutine>().StartCoroutine(RunExecute());
		}

		#endregion

		private IEnumerator RunExecute()
		{
			UpdateToInProgress();

			this.req = Resources.LoadAsync<ResourceType>(this.path);

			yield return req;

			if(req.asset == null) {
				CompleteWithError("Failed to load resource at path '" + this.path + "'");
				yield break;
			}

			this.asset = this.req.asset as ResourceType;
			if(this.asset == null) {
				CompleteWithError("Failed to cast resource at path '" + this.path + "' to type " + typeof(ResourceType));
				yield break;
			}

			this.item = this.assetToItem(this.asset);
			if(this.item == null) {
				CompleteWithError("Failed to convert resource at path '" + this.path
					+ "' from asset type" + typeof(ResourceType) + " to item type " + typeof(ItemType));
				
				yield break;
			}


			CompleteRequest();
		}

	}

	public class ResourceItemRequest<T> : ResourceItemRequest<T, T> where T : UnityEngine.Object
	{
		public ResourceItemRequest(string path) : base(path, ResourceItemRequest<T>.AssetToItem) {}

		private static T AssetToItem(T asset) { return asset; }
	}
}
