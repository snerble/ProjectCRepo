using System.Collections.Generic;
using System.Net;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// An abstract subclass of <see cref="Endpoint"/>. Implementations of this class are specifically meant to handle page requests.
	/// </summary>
	/// <remarks>
	/// To make actual HTML endpoints you must extend this class and implement the abstract classes. At least one of these functions
	/// must write data to the response object. If nothing is sent, a 501 will be sent instead.
	/// Attributes must be used to specify the target url.
	/// </remarks>
	public abstract class HTMLEndpoint : Endpoint
	{
		/// <summary>
		/// Initializes a new instance of <see cref="HTMLEndpoint"/> and immediately calls the right http method function.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> object to pass to this endpoint.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object to pass to this endpoint.</param>
		public HTMLEndpoint(HttpListenerRequest request, HttpListenerResponse response) : base(request, response)
		{
			var parameters = SplitQuery(request.Url.Query);

			// Invoke the right http method function
			var method = GetType().GetMethod(request.HttpMethod.ToUpper());
			if (method == null) Server.SendError(response, HttpStatusCode.NotImplemented);
			else method.Invoke(this, new object[] { parameters });
		}

		/// <summary>
		/// Endpoint for the http GET method. This must be implemented.
		/// </summary>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public abstract void GET(Dictionary<string, string> parameters);
		/// <summary>
		/// Endpoint for the http POST method.
		/// </summary>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public virtual void POST(Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http DELETE method.
		/// </summary>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public virtual void DELETE(Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http PATCH method.
		/// </summary>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public virtual void PATCH(Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
	}
}
