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
	/// must write data to the response object. If nothing is sent, a 404 will be sent instead.
	/// Attributes must be used to specify the target url.
	/// </remarks>
	public abstract class JsonEndpoint : Endpoint
	{
		/// <summary>
		/// Initializes a new instance of <see cref="JsonEndpoint"/>.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> object to pass to this endpoint.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object to pass to this endpoint.</param>
		public JsonEndpoint(HttpListenerRequest request, HttpListenerResponse response) : base(request, response) { }

		/// <summary>
		/// Invokes this <see cref="JsonEndpoint"/>.
		/// </summary>
		/// <remarks>
		/// This function cannot be overridden.
		/// </remarks>
		public sealed override void Invoke()
		{
			// Read the inputstream of the request and convert it to a JObject
			//var content = JObject.Parse(new StreamReader(Request.InputStream, Request.ContentEncoding).ReadToEnd());
			var content = new JObject();
			var parameters = SplitQuery(Request.Url.Query);
			switch (Request.HttpMethod)
			{
				case "GET": GET(content, parameters); break;
				case "POST": POST(content, parameters); break;
				case "DELETE": DELETE(content, parameters); break;
				case "PATCH": PATCH(content, parameters); break;
				default: Server.SendError(Response, HttpStatusCode.NotImplemented); break;
			}
		}

		/// <summary>
		/// Endpoint for the http GET method. This must be implemented.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		protected abstract void GET(JObject json, Dictionary<string, string> parameters);
		/// <summary>
		/// Endpoint for the http POST method.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		protected virtual void POST(JObject json, Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http DELETE method.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		protected virtual void DELETE(JObject json, Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http PATCH method.
		/// </summary>
		/// <param name="json">The json sent by the client.</param>
		/// <param name="parameters">A dictionary containing any url parameters.</param>
		protected virtual void PATCH(JObject json, Dictionary<string, string> parameters) => Server.SendError(Response, HttpStatusCode.NotImplemented);
	}
}
