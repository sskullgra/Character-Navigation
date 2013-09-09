using System;
using System.Collections.Generic;

namespace JumpPointSearach
{
	public class SortedHeap<T>
	{
	
	#region Fields
	
		private List<T> mHeapList;
		private IComparer<T> mHeapComparer = null;
	
	#endregion 
	
	#region Property
	
		public int Count {
			get { return mHeapList.Count;}
		}
	
	#endregion
	
	#region Indexer
	
		public T this [int Index] {
			get {
				if (Index >= mHeapList.Count || Index < 0) {
					throw new ArgumentOutOfRangeException (indexOutOfRange);
				}
			
				return mHeapList [Index];
			}
		}
	
	#endregion
	
	#region Constructor
	
		public SortedHeap (IComparer<T> comparer)
		{
			if (comparer == null) {
				throw new ArgumentNullException (nullComparer);
			}
		
			mHeapComparer = comparer;
			mHeapList = new List<T> ();
		}
	
	#endregion
	
	#region Methods
	
		public bool Contains (T item)
		{
			return mHeapList.BinarySearch (item, mHeapComparer) >= 0;
		}
	
		public bool Contains (T item, ref int index)
		{
			index = mHeapList.BinarySearch (item, mHeapComparer);
			return index >= 0;
		}
	
		public T Pop ()
		{
			if (mHeapList.Count == 0) {
				throw new InvalidOperationException (emptyHeap);
			}
		
			T item = mHeapList [mHeapList.Count - 1];
			mHeapList.RemoveAt (mHeapList.Count - 1);
			return (item);
		}
	
		public void Push (T item)
		{
			int position = mHeapList.BinarySearch (item, mHeapComparer);
			if (position < 0) {
				mHeapList.Insert (-position - 1, item);
			}
		}
	
		public void Remove (T item)
		{
			mHeapList.Remove (item);
		}
	
		public void RemoveAt (int index)
		{
			mHeapList.RemoveAt (index);
		}
	
	#endregion
	
	#region Logs
	
		string nullComparer = "Comparer is null!";
		string indexOutOfRange = "Index is out of range";
		string emptyHeap = "Empty heap!";
	
	#endregion
	
	}

}