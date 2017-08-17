using System;


namespace BeatThat
{
	public class LocalListRequest<T> : RequestBase, ListRequest<T> where T : class
	{
		public LocalListRequest(T[] items) 
		{
			this.allowCompleteFromCompletedStatus = true;
			this.items = items;
			UpdateStatus(RequestStatus.DONE);
		}

		public T[] items { get; private set; }

//		override public void Cancel() {}
		override protected void DisposeRequest() 
		{
			this.items = null;
		}
		
		override protected void ExecuteRequest()
		{
			CompleteRequest();
//			Actions.Exec(callback);
		}

		virtual public void Execute(Action<ListRequest<T>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			ListRequestExecutionPool<T>.Get().Execute(this, callback);
		}
	}
}