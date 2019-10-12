using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

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
		/// A list containing tuples with a predicate function and a unique queue.
		/// </summary>
		private List<(Func<HttpListenerContext, bool>, BlockingCollection<HttpListenerContext>)> customQueues = new List<(Func<HttpListenerContext, bool>, BlockingCollection<HttpListenerContext>)>();

		/// <summary>
		/// Gets the <see cref="BlockingCollection{T}"/> that gets filled with requests that this <see cref="Listener"/> receives.
		/// </summary>
		/// <remarks>
		/// If a custom queue has been made with <see cref="GetCustomQueue(Func{HttpListenerContext, bool})"/>, then this
		/// queue will contain all contexts that have not been sorted in any custom queues.
		/// </remarks>
		public BlockingCollection<HttpListenerContext> Queue { get; } = new BlockingCollection<HttpListenerContext>();

		/// <summary>
		/// Returns a <see cref="BlockingCollection{T}"/> that contains only elements that satisfy a specific condition.
		/// </summary>
		/// <remarks>
		/// Predicates of older queues override newer queues since a context can only enter one queue.
		/// Contexts that fail the predicates of all custom queues end up in <see cref="Queue"/>. These should also be handled.
		/// </remarks>
		/// <param name="predicate">A function to test each new element.</param>
		public BlockingCollection<HttpListenerContext> GetCustomQueue(Func<HttpListenerContext, bool> predicate)
		{
			if (predicate == null) throw new ArgumentNullException("predicate");
			if (customQueues.Exists((x) => x.Item1 == predicate)) throw new ArgumentException("This predicate is already registered for a custom queue.");
			var queue = new BlockingCollection<HttpListenerContext>();
			customQueues.Add((predicate, queue));
			return queue;
		}

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

		/// <summary>
		/// Starts this <see cref="Listener"/>'s thread and underlying <see cref="HttpListener"/>.
		/// </summary>
		public void Start()
		{
			listener.Start();
			thread.Start();
		}
		/// <summary>
		/// Stops this <see cref="Listener"/>'s underlying <see cref="HttpListener"/>.
		/// </summary>
		public void Stop()
		{
			listener.Stop();
			thread.Interrupt();
		}
		/// <summary>
		/// Blocks the current thread untill the underlying thread has terminated.
		/// </summary>
		public void Join() => thread.Join();

		/// <summary>
		/// Main loop for <see cref="thread"/>.
		/// </summary>
		private void Run()
		{
			try
			{
				while (listener.IsListening)
				{
					var context = listener.GetContext();
					// Loop through custom queues and check if the predicate returns true
					bool foundQueue = false;
					foreach (var customQueue in customQueues)
					{
						// If the predicate is satisfied, add the context to the queue.
						if (customQueue.Item1(context))
						{
							foundQueue = true;
							customQueue.Item2.Add(context);
							break;
						}
					}
					// Add to the default queue if no custom queue was satisfied
					if (!foundQueue) Queue.Add(context);
					Program.Log.Info("Received and enqueued a request.");
				}
			}
			catch (ThreadInterruptedException) { }
		}
	}
}
