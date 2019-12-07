using API.Database;
using System.Linq;
using System.Net;

namespace API.HTTP.Filters
{
	/// <summary>
	/// A filter class that prevents unauthorized access to backend utilities.
	/// </summary>
	[FilterUrl("/dev/")]
	public sealed class BackendFilter : Filter
	{
		protected override void Main()
		{
			// Get session from cookies
			var sessionId = Request.Cookies["session"]?.Value;
			var session = sessionId != null ? Utils.GetSession(sessionId) : null;

			// Check if the user associated with the session has appropriate access
			if (session != null && session.User.HasValue)
			{
				// If a user was found and it's accesslevel is admin, allow the endpoint parsing to continue
				var user = Utils.GetDatabase().Select<User>($"`id` = {session.User}").FirstOrDefault();
				if (user != null && user.AccessLevel == AccessLevel.Admin)
					return;
			}

			// Send 404 if the user does not have access and interrupt the endpoint parsing process
			Server.SendError(HttpStatusCode.NotFound);
			Interrupt();
		}
	}
}
