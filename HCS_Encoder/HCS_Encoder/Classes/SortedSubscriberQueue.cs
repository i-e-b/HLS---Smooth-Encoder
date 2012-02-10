using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic {
	public class SortedSubscriberQueue<T> where T:IComparable<T> {
		private object SyncRoot; // used to lock threads
		private SortedList<T, int> Data; //  Stored object => dummy value
		private Dictionary<object, int> Subscribers; // Subscribed object => current index

		/// <summary>
		/// Create a new sorted subscriber queue
		/// </summary>
		public SortedSubscriberQueue () {
			Data = new SortedList<T, int>();
			Subscribers = new Dictionary<object, int>();
			SyncRoot = new object();
		}

		/// <summary>
		/// Returns true if the object is registered as a subscriber of this queue
		/// </summary>
		public bool IsSubscribed (object subscriber) {
			lock (SyncRoot) {
				return Subscribers.ContainsKey(subscriber);
			}
		}

		/// <summary>
		/// Add a new item to the queue.
		/// It will have an access index based on it's IComparable order
		/// </summary>
		public void Enqueue (T value) {
			lock (SyncRoot) {
				if (Data.ContainsKey(value)) throw new ArgumentException("Can't add duplicate items to SortedSubscriberQueue");
				Data.Add(value, 0);
			}
		}

		/// <summary>
		/// Dequeue the next item in order for this subscriber
		/// </summary>
		/// <remarks>
		/// Objects will not be removed from the collection until all subscribers have dequeued past it.<br/>
		/// Objects whose sort order places them before already dequeued items will not be available to dequeue.
		/// </remarks>
		public T Dequeue (object subscriber) {
			T item = default(T);
			lock (SyncRoot) {
				if (!DataAvailable(subscriber))
					throw new Exception("Can't Dequeue");
				int i = Subscribers[subscriber];
				Subscribers[subscriber] = i + 1;
				item = Data.Keys[i];
			}

			ClearOldData();

			return item;
		}

		/// <summary>
		/// Return the next item in the queue, without dequeuing it.
		/// </summary>
		public T Peek (object subscriber) {
			T item = default(T);
			lock (SyncRoot) {
				if (!DataAvailable(subscriber))
					throw new Exception("No data to peek at");
				int i = Subscribers[subscriber];
				item = Data.Keys[i];
			}
			return item;
		}

		/// <summary>
		/// Wipe any data items that have been passed
		/// </summary>
		private void ClearOldData () {
			lock (SyncRoot) {
				int min_idx = Subscribers.Values.Min();

				if (min_idx < 1) return; // nothing to clear;

				for (int i = 0; i < min_idx; i++) { // clear old data
					Data.RemoveAt(i);
				}
				/*
				foreach (var key in Subscribers.Keys) { // adjust subscriber positions
					Subscribers[key] -= min_idx;
				}*/

				// A simple index accessor would help!
				var keys = Subscribers.Keys.ToArray();
				foreach (var key in keys) {
					Subscribers[key] -= min_idx;
				}
			}
		}

		/// <summary>
		/// Returns true if the subscriber has any data to dequeue.
		/// </summary>
		public bool DataAvailable (object subscriber) {
			lock (SyncRoot) {
				if (!Subscribers.ContainsKey(subscriber)) throw new Exception("Subscriber has not been registered");

				int i = Subscribers[subscriber];
				if (i >= 0 && i < Data.Keys.Count)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Remove all queued items and reset all subscribers.
		/// </summary>
		public void Clear () {
			lock (SyncRoot) {
				List<object> keys = new List<object>(Subscribers.Keys);
				foreach (var key in keys) {
					Subscribers[key] = 0;
				}
				Data.Clear();
			}
		}

		/// <summary>
		/// Subscribe to this queue.
		/// Items will not be removed from this collection until all subscribers have dequeued
		/// past the items' indices.
		/// </summary>
		public void Subscribe (object sender) {
			lock (SyncRoot) {
				if (Subscribers.ContainsKey(sender)) throw new ArgumentException("This object is already subscribed");
				Subscribers.Add(sender, 0);
			}
		}

		/// <summary>
		/// Remove queue subscription.
		/// Items will no longer wait for this subscriber before being removed.
		/// </summary>
		public void Unsubscribe (object sender) {
			lock (SyncRoot) {
				if (!Subscribers.ContainsKey(sender)) throw new ArgumentException("This object is not subscribed");
				Subscribers.Remove(sender);
			}
		}

		/// <summary>
		/// Returns count of items available to dequeue for the given subscriber
		/// </summary>
		public int Count (object subscriber) {
			lock (SyncRoot) {
				if (!Subscribers.ContainsKey(subscriber)) throw new Exception("Subscriber was not registed to this queue");

				int i = Subscribers[subscriber];
				if (Data.Keys.Count > i) return Data.Keys.Count - i;
				else return 0;
			}
		}

		/// <summary>
		/// Move the given subscriber to the head of the list (so that available items = 0)
		/// </summary>
		public void MoveToHead (object subscriber) {
			lock (SyncRoot) {
				if (!Subscribers.ContainsKey(subscriber)) throw new Exception("Subscriber was not registed to this queue");
				Subscribers[subscriber] = Data.Keys.Count;
			}
		}
	}
}
