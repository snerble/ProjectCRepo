using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace API.HTTP
{
	/// <summary>
	/// A class that will continuously listen for HTTP requests on a a specified address.
	/// </summary>
	public sealed class Listener
	{
		private readonly HttpListener listener;
		private readonly Thread thread;

		/// <summary>
		/// Gets the <see cref="BlockingCollection{T}"/> that gets filled with requests that this <see cref="Listener"/> receives.
		/// </summary>
		public BlockingCollection<HttpListenerContext> Queue { get; } = new BlockingCollection<HttpListenerContext>();

		private Listener()
		{
			thread = new Thread(Run);
			thread.Name = GetType().Name + "#" + thread.ManagedThreadId;
		}
		/// <summary>
		/// Creates a new instance of <see cref="Listener"/>.
		/// </summary>
		/// <param name="listener">A <see cref="HttpListener"/> object to receive requests from.</param>
		public Listener(HttpListener listener) : this()
		{
			this.listener = listener;
		}
		/// <summary>
		/// Creates a new instance of <see cref="Listener"/> that listens for requests on the specified addresses.
		/// </summary>
		/// <param name="addresses">An array of addresses to listen to.</param>
		public Listener(params string[] addresses) : this()
		{
			listener = new HttpListener();
			foreach (var address in addresses)
				listener.Prefixes.Add($"http://{address}/");
		}

		public void Start()
		{
			listener.Start();
			thread.Start();
		}
		public void Interrupt()
		{
			thread.Interrupt();
			listener.Stop();
		}

		/// <summary>
		/// Main loop for <see cref="thread"/>.
		/// </summary>
		private void Run()
		{
			while (true)
			{
				Queue.Add(listener.GetContext());
				Program.Log.Info("Received and enqueued a request.");
			}
		}
	}
}
