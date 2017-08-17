using System.Collections;
using UnityEngine;
using System;

namespace BeatThat
{
	public class LocalRequest: RequestBase 
	{
		/// <summary>
		/// Create a local request with an optional execution time.
		/// </summary>
		/// <param name="execDuration">
		/// If set > 0, then execution will take this duration instead of returning immediately.
		/// This is not implemented to be efficient. It's a convenience for test scenarios.
		/// </param>
		public LocalRequest(float execDuration = 0f) 
		{
			this.allowCompleteFromCompletedStatus = true;
			this.execDuration = execDuration;
			if(execDuration <= 0f) {
				UpdateStatus(RequestStatus.DONE);
			}
		}

		public float execDuration { get; set; }

		override protected void ExecuteRequest() 
		{ 
			if(this.execDuration <= 0f) {
				CompleteRequest();
				return;
			}

			UpdateToInProgress();
			var runner = new GameObject("LocalRequestRunner").AddComponent<RequestCoroutine>();
			runner.StartCoroutine(ExecuteWithDuration(runner));
		}

		/// <summary>
		/// Uses a one-off created GameObject to run a coroutine and fake delay the execution.
		/// This is sloppyish. Really intended only for test scenarios
		/// </summary>
		private IEnumerator ExecuteWithDuration(MonoBehaviour coroutineOwner)
		{
			yield return new WaitForSeconds(this.execDuration);
			CompleteRequest();
			UnityEngine.Object.Destroy(coroutineOwner.gameObject);
		}
	}

	public class LocalRequest<T> : LocalRequest, Request<T> 
	{
		/// <summary>
		/// Create a local request with an optional execution time.
		/// </summary>
		/// <param name="item">the result item (as if it had been fetched)</param>
		/// <param name="execDuration">
		/// If set > 0, then execution will take this duration instead of returning immediately.
		/// This is not implemented to be efficient. It's a convenience for test scenarios.
		/// </param>
		public LocalRequest(T item, float execDuration = 0f) : base(execDuration)
		{
			this.item = item;
		}

		public object GetItem() { return this.item; } 
		public T item { get; private set; }

		public void Execute(Action<Request<T>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<T>.Get().Execute(this, callback);
		}
	}
}