using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using API.Database;
using Newtonsoft.Json.Linq;

namespace API.HTTP.Endpoints
{
	[EndpointUrl("/session")]
	public sealed class JSession : JsonEndpoint
	{
		public override void GET(JObject json, Dictionary<string, string> parameters)
		{
			// Start transaction and commit once the session was sent
			var transaction = Program.Database.Connection.BeginTransaction();

			// Create new Session
			var session = Utils.CreateSession();

			// Upload session and cache
			Program.Database.Insert(session);
			
			// Send the entire session and respond with 201
			Server.SendJSON(session, HttpStatusCode.Created);
			
			// Once done with sending, commit database changes
			transaction.Commit();
			// Cache the session
			Utils.Sessions.Add(session);
		}
	}
}
