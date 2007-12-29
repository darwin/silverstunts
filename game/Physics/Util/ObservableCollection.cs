using System;
using System.Collections.Generic;

namespace Physics
{
	public class ObservableCollection<T>: IList<T> {

		private List<T> items = new List<T>();
		public event NotifyCollectionChangedEventHandler<T> CollectionChanged;

		public ObservableCollection() {
		}

		public void Add(T item) {
			this.items.Add(item);
			this.FireCollectionChanged(NotifyCollectionChangedAction.Add, item);
		}

		public void AddRange(IEnumerable<T> items) {
			this.items.AddRange(items);
			this.FireCollectionChanged(NotifyCollectionChangedAction.Add, items);
		}

		public void Clear() {
			this.items.Clear();
			this.FireCollectionChanged(NotifyCollectionChangedAction.Reset, null);
		}

		public bool Contains(T item) {
			return this.items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			this.items.CopyTo(array, arrayIndex);
		}

		public int Count {
			get { return this.items.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove(T item) {
			bool removed = this.items.Remove(item);
			this.FireCollectionChanged(NotifyCollectionChangedAction.Remove, item);
			return removed;
		}

		public T this[int index] {
			get { return this.items[index]; }
			set {
				this.FireCollectionChanged(NotifyCollectionChangedAction.Remove, this.items[index]);
				this.items[index] = value;
				this.FireCollectionChanged(NotifyCollectionChangedAction.Add, value);
			}
		}

		public int IndexOf(T item) {
			return this.items.IndexOf(item);
		}

		public void Insert(int index, T item) {
			this.items.Insert(index, item);
		}

		public void RemoveAt(int index) {
			this.items.RemoveAt(index);
		}

		public IEnumerator<T> GetEnumerator() {
			return this.items.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.items.GetEnumerator();
		}

		protected void FireCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> changedItems) {
			if (this.CollectionChanged != null)
				this.CollectionChanged(this, new NotifyCollectionChangedEventArgs<T>(action, changedItems));
		}

		protected void FireCollectionChanged(NotifyCollectionChangedAction action, T changedItem) {
			if (this.CollectionChanged != null)
				this.CollectionChanged(this, new NotifyCollectionChangedEventArgs<T>(action, changedItem));
		}
	}

	public delegate void NotifyCollectionChangedEventHandler<T>(object sender, NotifyCollectionChangedEventArgs<T> e);

	public class NotifyCollectionChangedEventArgs<T>: EventArgs {
		private IList<T> changedItems;
		private NotifyCollectionChangedAction action;


		public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IEnumerable<T> changedItems ) {
			this.action = action;
			List<T> changedList = new List<T>();
			if (changedItems != null)
				changedList.AddRange(changedItems);
			this.changedItems = changedList;
		}

		public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, T changedItem) {
			this.action = action;
			this.changedItems = new List<T>();
			this.changedItems.Add(changedItem);
		}

		public NotifyCollectionChangedAction Action {
			get { return this.action; }
		}

		public IEnumerable<T> NewItems {
			get {
				if (this.action != NotifyCollectionChangedAction.Add)
					return EmptyEnumerable<T>.Instance;
				return this.changedItems;
			}
		}

		public IEnumerable<T> OldItems {
			get {
				if (this.action != NotifyCollectionChangedAction.Remove)
					return EmptyEnumerable<T>.Instance;
				return this.changedItems;
			}
		}

	}

	public enum NotifyCollectionChangedAction {
		Add,
		Remove,
		Reset
	}
}
