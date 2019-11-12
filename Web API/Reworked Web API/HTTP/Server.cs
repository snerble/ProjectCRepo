using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace API.HTTP
{
	/// <summary>
	/// Abstract class for creating HTTP server threads.
	/// </summary>
	public abstract class Server
	{
		/// <summary>
		/// Source of http requests for this <see cref="Server"/>.
		/// </summary>
		private readonly BlockingCollection<HttpListenerContext> queue;
		/// <summary>
		/// The <see cref="Thread"/> on which this <see cref="Server"/> performs it's functions.
		/// </summary>
		protected readonly Thread thread;

		/// <summary>
		/// The name of the underlying thread.
		/// </summary>
		public string Name => thread.Name;

		/// <summary>
		/// Creates a new instance of <see cref="Server"/>.
		/// </summary>
		/// <param name="queue">The source of <see cref="HttpListenerContext"/> objects to parse.</param>
		public Server(BlockingCollection<HttpListenerContext> queue)
		{
			this.thread = new Thread(Run);
			thread.Name = GetType().Name + "::" + thread.ManagedThreadId;
			this.queue = queue;
			Program.Log.Config($"Created server {Name}");
		}

		/// <summary>
		/// Main loop for <see cref="thread"/>. Takes a <see cref="HttpListenerContext"/> from <see cref="queue"/>
		/// and calls <see cref="Main"/>.
		/// </summary>
		private void Run()
		{
			try
			{
				while (!queue.IsCompleted)
				{
					var context = queue.Take();

					// Always deny requests with invalid urls
					if (context.Request.Url.AbsolutePath.Contains(".."))
					{
						SendError(context.Response, HttpStatusCode.Forbidden);
						continue;
					}

					// Try to execute main function
					try
					{
						Main(context.Request, context.Response);
					}
					catch (Exception e)
					{
						// Try to send an internal server error on exception
						try { SendError(context.Response, HttpStatusCode.InternalServerError); }
						catch (ObjectDisposedException) { }
						Program.Log.Error($"{e.GetType().Name} in {GetType().Name}.Main(): {e.Message}", e, true);
					}
				}
			}
			catch (ThreadInterruptedException) { }
		}

		/// <summary>
		/// The function that is called when this <see cref="Server"/> instance received a request.
		/// </summary>
		/// <param name="request">The <see cref="HttpListenerRequest"/> that represents a client's request for a resource.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> object that will be sent to the client in response to the client's request.</param>
		protected abstract void Main(HttpListenerRequest request, HttpListenerResponse response);

		/// <summary>
		/// Starts the underlying thread.
		/// </summary>
		public void Start() => thread.Start();
		/// <summary>
		/// Blocks the current thread untill the underlying thread has terminated.
		/// </summary>
		public void Join() => thread.Join();
		/// <summary>
		/// Interrupts the underlying thread.
		/// </summary>
		public void Interrupt() => thread.Interrupt();

		/// <summary>
		/// Writes a byte buffer to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="response">The <see cref="HttpListenerResponse"/> to send data to.</param>
		/// <param name="buffer">The array of bytes to send.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		public static void Send(HttpListenerResponse response, byte[] buffer, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			response.StatusCode = (int)statusCode;
			response.ContentLength64 = buffer.Length;
			using var outStream = response.OutputStream;
			outStream.Write(buffer, 0, buffer.Length);
		}
		/// <summary>
		/// Writes plain text to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="response">The <see cref="HttpListenerResponse"/> to send data to.</param>
		/// <param name="text">The string of text to send.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		/// <param name="encoding">The encoding of the text. <see cref="Encoding.UTF8"/> by default.</param>
		public static void SendText(HttpListenerResponse response, string text, HttpStatusCode statusCode = HttpStatusCode.OK, Encoding encoding = null)
			=> Send(response, (encoding ?? Encoding.UTF8).GetBytes(text), statusCode);
		/// <summary>
		/// Writes the contents of an html file to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="response">The <see cref="HttpListenerResponse"/> to send data to.</param>
		/// <param name="htmlFile">The path of the html file, relative to the source directory.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		public static void SendHTML(HttpListenerResponse response, string htmlFile, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			htmlFile = Program.Config.HTMLSourceDir + htmlFile;
			response.Headers.Set("Content-Type", "text/html");
			Send(response, File.ReadAllBytes(htmlFile), statusCode);
		}
		/// <summary>
		/// Writes a <see cref="JObject"/> to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="response">The <see cref="HttpListenerResponse"/> to send data to.</param>
		/// <param name="json">The <see cref="JObject"/> to send to the client.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		public static void SendJSON(HttpListenerResponse response, JObject json, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			response.ContentType = "application/json";
			response.StatusCode = (int)statusCode;
			using var writer = new JsonTextWriter(new StreamWriter(response.OutputStream));
			json.WriteTo(writer);
		}
		/// <summary>
		/// Sends just a <see cref="HttpStatusCode"/> to the client.
		/// </summary>
		/// <remarks>
		/// Simply sets the statuscode of the response and closes it's outputstream.
		/// </remarks>
		/// <param name="response">The <see cref="HttpListenerResponse"/> to send the errorcode to.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to specify.</param>
		public static void SendError(HttpListenerResponse response, HttpStatusCode statusCode)
		{
			response.StatusCode = (int)statusCode;
			response.Close();
		}
		/// <summary>
		/// Adds a cookie to the response object.
		/// </summary>
		/// <param name="response">The response object to add a cookie to.</param>
		/// <param name="name">The name of the cookie.</param>
		/// <param name="value">The value of the cookie.</param>
		public static void AddCookie(HttpListenerResponse response, string name, object value)
			=> response.Headers.Add("Set-Cookie", $"{name}={value}");
	}
}
