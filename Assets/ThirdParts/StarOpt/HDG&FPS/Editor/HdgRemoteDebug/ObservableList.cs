using System;
using System.Collections.Generic;

namespace GameEditor.Hdg
{
	public class ObservableList<T> : List<T>
	{

		public event Action<ObservableList<T>> ListChanged;

		public new void Add(T item)
		{
			base.Add(item);
			ListChanged(this);
		}

		public new void Remove(T item)
		{
			base.Remove(item);
			ListChanged(this);
		}

		public new void AddRange(IEnumerable<T> collection)
		{
			base.AddRange(collection);
			ListChanged(this);
		}

		public new void RemoveRange(int index, int count)
		{
			base.RemoveRange(index, count);
			ListChanged(this);
		}

		public void ReplaceAll(T item)
		{
			base.Clear();
			base.Add(item);
			ListChanged(this);
		}

		public void ReplaceAll(IEnumerable<T> collection)
		{
			base.Clear();
			base.AddRange(collection);
			ListChanged(this);
		}

		public new void Clear()
		{
			base.Clear();
			ListChanged(this);
		}

		public new void Insert(int index, T item)
		{
			base.Insert(index, item);
			ListChanged(this);
		}

		public new void InsertRange(int index, IEnumerable<T> collection)
		{
			base.InsertRange(index, collection);
			ListChanged(this);
		}

		public new void RemoveAll(Predicate<T> match)
		{
			base.RemoveAll(match);
			ListChanged(this);
		}

		public new T this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				ListChanged(this);
			}
		}
	}
}
