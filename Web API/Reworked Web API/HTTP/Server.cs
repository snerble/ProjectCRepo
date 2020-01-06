using MimeKit;
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
		/// Gets the <see cref="HttpListenerRequest"/> sent by a client.
		/// </summary>
		protected HttpListenerRequest Request { get; private set; }
		/// <summary>
		/// Gets the <see cref="HttpListenerResponse"/> directed to a client.
		/// </summary>
		protected HttpListenerResponse Response { get; private set; }

		/// <summary>
		/// Creates a new instance of <see cref="Server"/>.
		/// </summary>
		/// <param name="queue">The source of <see cref="HttpListenerContext"/> objects to parse.</param>
		public Server(BlockingCollection<HttpListenerContext> queue)
		{
			this.thread = new Thread(Run);
			thread.Name = GetType().Name + ":" + thread.ManagedThreadId;
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
					// Wait for a new request
					var context = queue.Take();
					Request = context.Request;
					Response = context.Response;

					// Always deny requests with invalid urls
					if (Request.Url.AbsolutePath.Contains(".."))
					{
						SendError(HttpStatusCode.Forbidden);
						continue;
					}

					// Try to execute main function
					try
					{
						Main();
					}
					catch (ThreadInterruptedException)
					{
						return;
					}
					catch (Exception e)
					{
						e = e.InnerException ?? e;
						Program.Log.Error($"{e.GetType().Name} in {GetType().Name}.Main(): {e.Message}", e, true);
					}
					// If it isn't already closed, send an internal server error
					try { SendError(HttpStatusCode.InternalServerError); }
					catch (HttpListenerException) { } // connection was closed
					catch (ObjectDisposedException) { } // connection is already closed

					Request = null;
					Response = null;
				}
			}
			catch (ThreadInterruptedException) { }
		}

		/// <summary>
		/// The function that is called when this <see cref="Server"/> instance received a request.
		/// </summary>
		protected abstract void Main();

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
		/// Writes a byte array to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="data">The array of bytes to send.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		public virtual void Send(byte[] data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			Response.StatusCode = (int)statusCode;
			if (data != null)
			{
				Response.ContentLength64 = data.Length;
				Response.OutputStream.Write(data, 0, data.Length);
			}
			Response.Close();
		}
		/// <summary>
		/// Writes plain text to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="text">The string of text to send.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		/// <param name="encoding">The encoding of the text. <see cref="Encoding.UTF8"/> by default.</param>
		public virtual void SendText(string text, HttpStatusCode statusCode = HttpStatusCode.OK, Encoding encoding = null)
			=> Send((encoding ?? Encoding.UTF8).GetBytes(text), statusCode);
		/// <summary>
		/// Writes a <see cref="JObject"/> to the specified <see cref="HttpListenerResponse"/>.
		/// </summary>
		/// <param name="json">The <see cref="JObject"/> to send to the client.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		public virtual void SendJSON(JObject json, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			if (json == null)
			{
				Send(null, statusCode);
				return;
			}

            Response.ContentType = "application/json";
            var mem = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(mem)))
                json.WriteTo(writer);
            Send(mem.ToArray(), statusCode);
        }
		/// <summary>
		/// Sends all the data of the specified file and automatically provides the correct MIME type to the client.
		/// </summary>
		/// <param name="path">The path to the file to send.</param>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to send to the client.</param>
		public virtual void SendFile(string path, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			if (!File.Exists(path)) throw new FileNotFoundException("File does not exist.", path);
			Response.ContentType = MimeTypes.GetMimeType(Path.GetExtension(path));
			Send(File.ReadAllBytes(path), statusCode);
		}
		/// <summary>
		/// Sends just an <see cref="HttpStatusCode"/> to the client.
		/// </summary>
		/// <param name="statusCode">The <see cref="HttpStatusCode"/> to specify.</param>
		public virtual void SendError(HttpStatusCode statusCode)
			=> Send(null, statusCode);
	}
}
