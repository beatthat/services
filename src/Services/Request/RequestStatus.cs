namespace BeatThat
{
	/// <summary>
	/// NONE: request has not been executed
	/// QUEUED: Request::Execute has been called and the request *will* execute but the underlying request has not been executed (e.g. for a web request that gets queued by a runner)
	/// IN_PROGRESS: Request::Execute has been called and the request is in progress but has not completed yet
	/// CANCELLED: Request::Execute was called and then the request was cancelled before completion
	/// DONE: the request is done, with or without error
	/// </summary>
	public enum RequestStatus { 
		NONE = 0,
		QUEUED = 1, 
		IN_PROGRESS = 2,
		CANCELLED = 3, 
		DONE = 4 };

}