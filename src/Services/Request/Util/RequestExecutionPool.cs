using System;
using UnityEngine;
using System.Collections.Generic;

namespace BeatThat
{
	/// <summary>
	/// Removes the closure verbage (and allocation) from this pattern:
	/// 
	/// public Request ServiceCall(Action<Request> callback) {
	/// 	var req = // get a request;
	/// 	req.Execute(() => { callback(req); });
	/// 	return req;
	/// }
	/// 
	/// ... using the pool, it changes to this:
	/// 
	/// public Request ServiceCall(Action<Request> callback) {
	/// 	var req = // get a request;
	/// 	req.Execute(callback); // NO MORE CLOSURE
	/// 	return req;
	/// }
	/// 
	/// ...probably not significant performancewise, since there are so many allocations involved in most Request usage,
	/// but since Requests are so heavily used, added anyway.
	/// </summary>
	public static class RequestExecutionPool
	{
		private static int m_createdCount;

		public static RequestExecution Get()
		{
			if(m_pool.Count > 0) {
				var e = m_pool[0];
				m_pool.RemoveAt(0);
				return e;
			}

			if(++m_createdCount > 100) { 
				Debug.LogWarning("[" + Time.frameCount + "] RequestExecutionPool::Get has created " 
					+ m_createdCount + " pool objects. There may be a leak (a Request type that never calls completion callback?)");
			}

			return new RequestExecution();
		}

		public static void Return(RequestExecution e)
		{
			if(m_pool.Contains(e)) {
				Debug.LogWarning("ExecutionPool::Release called for a list that's already in the pool");
				return;
			}
			e.Reset();
			m_pool.Add(e);
		}

		// Analysis disable StaticFieldInGenericType
		private static readonly List<RequestExecution> m_pool = new List<RequestExecution>(1);
		// Analysis restore StaticFieldInGenericType
	}

	public static class RequestExecutionPool<T>
	{
		// Analysis disable StaticFieldInGenericType
		private static int m_createdCount;
		// Analysis restore StaticFieldInGenericType

		public static RequestExecution<T> Get()
		{
			if(m_pool.Count > 0) {
				var e = m_pool[0];
				m_pool.RemoveAt(0);
				return e;
			}

			if(++m_createdCount > 100) { 
				Debug.LogWarning("[" + Time.frameCount + "] RequestExecutionPool<" + typeof(T).Name + ">::Get has created " 
					+ m_createdCount + " pool objects. There may be a leak (a Request type that never calls completion callback?)");
			}

			return new RequestExecution<T>();
		}

		public static void Return(RequestExecution<T> e)
		{
			if(m_pool.Contains(e)) {
				Debug.LogWarning("ExecutionPool::Release called for a list that's already in the pool");
				return;
			}
			e.Reset();
			m_pool.Add(e);
		}

		// Analysis disable StaticFieldInGenericType
		private static readonly List<RequestExecution<T>> m_pool = new List<RequestExecution<T>>(1);
		// Analysis restore StaticFieldInGenericType
	}

	public static class RequestExecutionPool<ItemType, RequestType> 
		where RequestType : Request<ItemType>
	{
		// Analysis disable StaticFieldInGenericType
		private static int m_createdCount;
		// Analysis restore StaticFieldInGenericType

		public static RequestExecution<ItemType, RequestType> Get()
		{
			if(m_pool.Count > 0) {
				var e = m_pool[0];
				m_pool.RemoveAt(0);
				return e;
			}

			if(++m_createdCount > 100) { 
				Debug.LogWarning("[" + Time.frameCount + "] RequestExecutionPool<" 
					+ typeof(ItemType).Name + "," + typeof(RequestType).Name + ">::Get has created " 
					+ m_createdCount + " pool objects. There may be a leak (a Request type that never calls completion callback?)");
			}

			return new RequestExecution<ItemType, RequestType>();
		}

		public static void Return(RequestExecution<ItemType, RequestType> e)
		{
			if(m_pool.Contains(e)) {
				Debug.LogWarning("ExecutionPool::Release called for a list that's already in the pool");
				return;
			}
			e.Reset();
			m_pool.Add(e);
		}

		// Analysis disable StaticFieldInGenericType
		private static readonly List<RequestExecution<ItemType, RequestType>> m_pool = new List<RequestExecution<ItemType, RequestType>>(1);
		// Analysis restore StaticFieldInGenericType
	}

	public static class ListRequestExecutionPool<T>
	{
		// Analysis disable StaticFieldInGenericType
		private static int m_createdCount;
		// Analysis restore StaticFieldInGenericType

		public static ListRequestExecution<T> Get()
		{
			if(m_pool.Count > 0) {
				var e = m_pool[0];
				m_pool.RemoveAt(0);
				return e;
			}

			if(++m_createdCount > 100) { 
				Debug.LogWarning("[" + Time.frameCount + "] ListRequestExecutionPool<" + typeof(T).Name + ">::Get has created " 
					+ m_createdCount + " pool objects. There may be a leak (a Request type that never calls completion callback?)");
			}

			return new ListRequestExecution<T>();
		}

		public static void Return(ListRequestExecution<T> e)
		{
			if(m_pool.Contains(e)) {
				Debug.LogWarning("ExecutionPool::Release called for a list that's already in the pool");
				return;
			}
			e.Reset();
			m_pool.Add(e);
		}

		// Analysis disable StaticFieldInGenericType
		private static readonly List<ListRequestExecution<T>> m_pool = new List<ListRequestExecution<T>>(1);
		// Analysis restore StaticFieldInGenericType
	}


	public class RequestExecution : IDisposable
	{
		#region IDisposable implementation
		public void Dispose ()
		{
			RequestExecutionPool.Return(this);
		}
		#endregion

		public void Reset()
		{
			this.req = null;
			this.callback = null;
		}

		public void Execute(Request r, Action<Request> callback)
		{
			this.req = r;
			this.callback = callback;
			r.Execute(this.executeCallbackAction);
		}

		private void OnExecuteCallback()
		{
			if(this.callback == null) {
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + "::OnExecuteCallback no callback set!");
				Dispose();
				return;
			}

			this.callback(this.req);
			Dispose();
		}

		private Request req { get; set; }
		private Action<Request> callback { get; set; }

		private Action executeCallbackAction { get{ return  m_executeCallbackAction?? (m_executeCallbackAction = this.OnExecuteCallback); } }
		private Action m_executeCallbackAction;
	}

	public class RequestExecution<ItemType, RequestType> : IDisposable
		where RequestType : Request<ItemType>
	{
		#region IDisposable implementation
		public void Dispose ()
		{
			RequestExecutionPool<ItemType, RequestType>.Return(this);
		}
		#endregion

		public void Reset()
		{
			this.req = default(RequestType);
			this.callback = null;
		}

		public void Execute(RequestType r, Action<RequestType> callback)
		{
			this.req = r;
			this.callback = callback;
			r.Execute(this.executeCallbackAction);
		}

		private void OnExecuteCallback()
		{
			if(this.callback == null) {
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + "::OnExecuteCallback no callback set!");
				Dispose();
				return;
			}

			this.callback(this.req);
			Dispose();
		}

		private RequestType req { get; set; }
		private Action<RequestType> callback { get; set; }

		private Action executeCallbackAction { get{ return  m_executeCallbackAction?? (m_executeCallbackAction = this.OnExecuteCallback); } }
		private Action m_executeCallbackAction;
	}

	public class RequestExecution<T> : RequestExecution<T, Request<T>> 
	{
	}

	public class ListRequestExecution<T> : IDisposable
	{
		#region IDisposable implementation
		public void Dispose ()
		{
			ListRequestExecutionPool<T>.Return(this);
		}
		#endregion

		public void Reset()
		{
			this.req = null;
			this.callback = null;
		}

		public void Execute(ListRequest<T> r, Action<ListRequest<T>> callback)
		{
			this.req = r;
			this.callback = callback;
			r.Execute(this.executeCallbackAction);
		}

		private void OnExecuteCallback()
		{
			if(this.callback == null) {
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + "::OnExecuteCallback no callback set!");
				Dispose();
				return;
			}

			this.callback(this.req);
			Dispose();
		}

		private ListRequest<T> req { get; set; }
		private Action<ListRequest<T>> callback { get; set; }

		private Action executeCallbackAction { get{ return  m_executeCallbackAction?? (m_executeCallbackAction = this.OnExecuteCallback); } }
		private Action m_executeCallbackAction;
	}
}
