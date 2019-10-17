using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// An abstract subclass of <see cref="Endpoint"/>. This contains functions specifically made to handle a JSON type request.
	/// </summary>
	/// <remarks>
	/// To make actual JSON endpoints you must extend this class and implement the abstract classes. At least one of these functions
	/// must write data to the response object. If nothing is sent, a 501 will be sent instead.
	/// Attributes must be used to specify the target url.
	/// Because reflection is used to invoke a particular HTTP method, additional HTTP method support can be implemented by simply
	/// making a new public function, whose name is the HTTP method it represents in all upper case. Note that they must take the
	/// same parameters as the other functions.
	/// </remarks>
	public abstract class JsonEndpoint : Endpoint
	{
		/// <summary>
		/// Initializes a new instance of <see cref="JsonEndpoint"/> and immediately calls the right http method function.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> object to pass to this endpoint.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object to pass to this endpoint.</param>
		public JsonEndpoint(HttpListenerRequest request, HttpListenerResponse response) : base(request, response)
		{
			// Read the inputstream of the request and try to convert it to a JObject
			JObject content;
			try
			{
				// If content length is 0 (no content) then use blank JObject
				if (request.ContentLength64 == 0) content = new JObject();
				else
				{
					using var streamReader = new StreamReader(Request.InputStream, Request.ContentEncoding);
					content = JObject.Parse(streamReader.ReadToEnd());
				}
			}
			catch (Exception)
			{
				// Send BadRequest if it doesn't contain a readable JSON
				Server.SendError(Response, HttpStatusCode.BadRequest);
				return;
			}
			var parameters = SplitQuery(Request.Url.Query);

			// Invoke the right http method function
			var method = GetType().GetMethod(request.HttpMethod.ToUpper());
			if (method == null) Server.SendError(response, HttpStatusCode.NotImplemented);
			else method.Invoke(this, new object[] { content, parameters });
		}

		/// <summary>
		/// Endpoint for the http GET method. This must be implemented.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public abstract void GET(JObject json, Dictionary<string, string> parameters);
		/// <summary>
		/// Endpoint for the http POST method.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public virtual void POST(JObject json, Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http DELETE method.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public virtual void DELETE(JObject json, Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http PATCH method.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		public virtual void PATCH(JObject json, Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
	}
}
