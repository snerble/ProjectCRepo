using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

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
			thread.Name = GetType().Name + "#" + thread.ManagedThreadId;
			this.queue = queue;
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
					Main(context.Request, context.Response);
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
		/// Blocks the calling thread until the thread represented by this instance terminates, while continuing to
		/// perform standard COM and SendMessage pumping.
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
		protected static void Send(HttpListenerResponse response, byte[] buffer, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			response.StatusCode = (int)statusCode;
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
		protected static void SendText(HttpListenerResponse response, string text, HttpStatusCode statusCode = HttpStatusCode.OK, Encoding encoding = null)
			=> Send(response, (encoding ?? Encoding.UTF8).GetBytes(text), statusCode);
		/// <summary>
		/// Writes a <see cref="JObject"/> to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="response">The <see cref="HttpListenerResponse"/> to send data to.</param>
		/// <param name="json">The <see cref="JObject"/> to send to the client.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		protected static void SendJSON(HttpListenerResponse response, JObject json, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			response.ContentType = "application/json";
			response.StatusCode = (int)statusCode;
			using var writer = new JsonTextWriter(new StreamWriter(response.OutputStream));
			json.WriteTo(writer);
		}
	}
}
