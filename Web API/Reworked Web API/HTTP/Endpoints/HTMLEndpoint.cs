using API.Database;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace API.HTTP.Endpoints
{
	/// <summary>
	/// An abstract subclass of <see cref="Endpoint"/>. Implementations of this class are specifically meant to handle page requests.
	/// </summary>
	/// <remarks>
	/// To make actual HTML endpoints you must extend this class and implement the abstract classes. If the specified HTTP method is not
	/// found, a 501 will be sent.
	/// Attributes must be used to specify the target url.
	/// Because reflection is used to invoke a particular HTTP method, additional HTTP method support can be implemented by simply
	/// making a new public function, whose name is the HTTP method it represents in all upper case. Note that they must take the
	/// same parameters as the other functions.
	/// </remarks>
	public abstract class HTMLEndpoint : Endpoint
	{
		/// <summary>
		/// Gets the <see cref="User"/> instance that is requesting this endpoint.
		/// </summary>
		protected User CurrentUser { get; private set; }
		/// <summary>
		/// Gets whether or not the client who requested this endpoint is logged in.
		/// </summary>
		protected bool IsLoggedIn => CurrentUser != null;

		/// <summary>
		/// Extracts the parameters from the request object and calls the specified HTTP method function.
		/// </summary>
		protected override void Main()
		{
			// Get the session from the cookies (if it exists)
			var sessionId = Request.Cookies["session"]?.Value;
			var session = sessionId == null ? null : Utils.GetSession(sessionId);

			// Get the url (or payload) parameters
			var parameters = SplitQuery(Request);

			// Invoke the right http method function
			var method = GetType().GetMethod(Request.HttpMethod.ToUpper());
			if (method == null)
			{
				// Send a 501 not implemented if the method does exist
				Server.SendError(HttpStatusCode.NotImplemented);
			}
			else
			{
				// Check if the endpoint requires login info
				if (GetType().GetCustomAttribute<RequiresLoginAttribute>() != null)
				{
					if (session != null && session.User.HasValue)
					{
						// Get the user associated with the session
						CurrentUser = Program.Database.Select<User>($"`id` = {session.User}").FirstOrDefault();
					}
					if (!IsLoggedIn)
					{
						// Send a 401 status code if the login data is missing
						Server.SendError(HttpStatusCode.Unauthorized);
						return;
					}
				}
				// Run the requested endpoint method
				method.Invoke(this, new object[] { parameters });
			}
		}

		/// <summary>
		/// Endpoint for the http GET method. This must be implemented.
		/// </summary>
		/// <param name="parameters">A dictionary containing all url parameters.</param>
		public abstract void GET(Dictionary<string, string> parameters);
		/// <summary>
		/// Endpoint for the http POST method.
		/// </summary>
		/// <param name="parameters">A dictionary containing all url parameters.</param>
		public virtual void POST(Dictionary<string, string> parameters) => Server.SendError(HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http DELETE method.
		/// </summary>
		/// <param name="parameters">A dictionary containing all url parameters.</param>
		public virtual void DELETE(Dictionary<string, string> parameters) => Server.SendError(HttpStatusCode.NotImplemented);
		/// <summary>
		/// Endpoint for the http PATCH method.
		/// </summary>
		/// <param name="parameters">A dictionary containing all url parameters.</param>
		public virtual void PATCH(Dictionary<string, string> parameters) => Server.SendError(HttpStatusCode.NotImplemented);
	}
}
