using API.Database;
using System;
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
		protected User CurrentUser
		{
			get
			{
				// Skip if the session is null or does not have a userid
				if (CurrentSession == null || !CurrentSession.User.HasValue) return null;
				// Set the cache with a user from the database if it isn't already set
				if (_CurrentUser == null) _CurrentUser = Database.Select<User>($"`id` = {CurrentSession.User}").FirstOrDefault();
				// Return the cache
				return _CurrentUser;
			}
		}
		private User _CurrentUser;
		/// <summary>
		/// Gets the <see cref="Session"/> instance associated with the <see cref="Endpoint.Request"/>.
		/// </summary>
		protected Session CurrentSession { get; private set; }
		/// <summary>
		/// Gets the <see cref="AppDatabase"/> of the current thread.
		/// </summary>
		protected AppDatabase Database => Utils.GetDatabase();

		/// <summary>
		/// Extracts the parameters from the request object and calls the specified HTTP method function.
		/// </summary>
		protected override void Main()
		{
			// Set current user cache to null
			_CurrentUser = null;

			// Get the session from the cookies (if it exists)
			var sessionId = Request.Cookies["session"]?.Value;
			CurrentSession = sessionId == null ? null : Utils.GetSession(sessionId);

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
					// Get the current user from the property
					if (CurrentUser == null)
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

		/// <summary>
		/// Validates an array of parameters by checking if they are present and match the predicate. A response is
		/// immediately sent to the client if this function returns false.
		/// </summary>
		/// <param name="parameters">The dictionary from which to get the values.</param>
		/// <param name="predicates">An array of tuples containing the parameter name and a predicate.</param>
		/// <returns>True if the validation succeeded. False otherwise.</returns>
		protected static bool ValidateParams(Dictionary<string, string> parameters, params (string, Func<string, bool>)[] predicates)
			=> ValidateParams(parameters, ValidationMode.Required, predicates);
		/// <summary>
		/// Validates an array of parameters by checking if they are present and match the predicate. A response is
		/// immediately sent to the client if this function returns false.
		/// </summary>
		/// <param name="parameters">The dictionary from which to get the values.</param>
		/// <param name="mode">The type of validation checking to use.</param>
		/// <param name="predicates">An array of tuples containing the parameter name and a predicate.</param>
		/// <returns>True if the validation succeeded. False otherwise.</returns>
		protected static bool ValidateParams(Dictionary<string, string> parameters, ValidationMode mode, params (string, Func<string, bool>)[] predicates)
		{
			if (mode == ValidationMode.Required)
			{
				// Check if none are missing if the mode is 'Required'
				if (predicates.Any(x => !parameters.ContainsKey(x.Item1)))
					return false;
			}
			else if (mode == ValidationMode.Options)
			{
				// Check if at least one parameter is present if the mode is 'Options'
				if (predicates.All(x => !parameters.ContainsKey(x.Item1)))
					return false;
			}

			// Check predicates for every value that isn't missing
			foreach (var (name, predicate) in predicates.Where(x => x.Item2 != null && !parameters.ContainsKey(x.Item1)))
				if (!predicate(parameters[name]))
					return false;

			// Return true to indicate successfull validation
			return true;
		}

		/// <summary>
		/// Specifies how the parameter validation will behave.
		/// </summary>
		protected enum ValidationMode
		{
			/// <summary>
			/// All specified parameters are required and must pass validation.
			/// </summary>
			Required,
			/// <summary>
			/// At least one of the specified parameters are required and all given parameters must pass validation.
			/// </summary>
			Options,
			/// <summary>
			/// None of the parameters are required, but all given parameters must pass validation.
			/// </summary>
			Optional
		}
	}
}
