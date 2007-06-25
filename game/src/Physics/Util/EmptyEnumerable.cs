using System;
using System.Collections.Generic;

namespace Physics
{
	public class EmptyEnumerable<T> : IEnumerable<T>, IEnumerator<T> {
		private static EmptyEnumerable<T> instance = new EmptyEnumerable<T>();

		protected EmptyEnumerable() {
		}

		public IEnumerator<T> GetEnumerator() {
			return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this;
		}

		public T Current {
			get { throw new ArgumentException(); }
		}

		public void Dispose() {
		}

		object System.Collections.IEnumerator.Current {
			get { throw new ArgumentException(); }
		}

		public bool MoveNext() {
			return false;
		}

		public void Reset() {
		}

		public static EmptyEnumerable<T> Instance {
			get { return EmptyEnumerable<T>.instance; }
		}
	}
}
