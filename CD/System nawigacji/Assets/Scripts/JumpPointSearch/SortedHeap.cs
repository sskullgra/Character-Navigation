using System;
using System.Collections.Generic;

namespace JumpPointSearach
{
	/// <summary>
	/// Sorted heap.
	/// Author: Radosław Bigaj
	/// POLITECHNIKA ŚLĄSKA 
	/// </summary>
	/// <exception cref='ArgumentOutOfRangeException'>
	/// Is thrown when the argument out of range exception.
	/// </exception>
	/// <exception cref='ArgumentNullException'>
	/// Is thrown when the argument null exception.
	/// </exception>
	/// <exception cref='InvalidOperationException'>
	/// Is thrown when the invalid operation exception.
	/// </exception>
	public class SortedHeap<T>
	{
	
	#region Fields
		/// <summary>
		/// The  heap list.
		/// </summary>
		private List<T> mHeapList;
		/// <summary>
		/// The heap comparer.
		/// </summary>
		private IComparer<T> mHeapComparer = null;
	
	#endregion 
	
	#region Property
		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		public int Count {
			get { return mHeapList.Count;}
		}
	
	#endregion
	
	#region Indexer
		/// <summary>
		/// Gets the <see cref="JumpPointSearach.SortedHeap`1"/> with the specified Index.
		/// </summary>
		/// <param name='Index'>
		/// Index.
		/// </param>
		/// <exception cref='ArgumentOutOfRangeException'>
		/// Is thrown when the argument out of range exception.
		/// </exception>
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
		/// <summary>
		/// Initializes a new instance of the <see cref="JumpPointSearach.SortedHeap`1"/> class.
		/// </summary>
		/// <param name='comparer'>
		/// Comparer.
		/// </param>
		/// <exception cref='ArgumentNullException'>
		/// Is thrown when the argument null exception.
		/// </exception>
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
		/// <summary>
		/// Contains the specified item.
		/// </summary>
		/// <param name='item'>
		/// If set to <c>true</c> item.
		/// </param>
		public bool Contains (T item)
		{
			return mHeapList.BinarySearch (item, mHeapComparer) >= 0;
		}
		/// <summary>
		/// Contains the specified item and index.
		/// </summary>
		/// <param name='item'>
		/// If set to <c>true</c> item.
		/// </param>
		/// <param name='index'>
		/// If set to <c>true</c> index.
		/// </param>
		public bool Contains (T item, ref int index)
		{
			index = mHeapList.BinarySearch (item, mHeapComparer);
			return index >= 0;
		}
		/// <summary>
		/// Pop this instance.
		/// </summary>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when the invalid operation exception.
		/// </exception>
		public T Pop ()
		{
			if (mHeapList.Count == 0) {
				throw new InvalidOperationException (emptyHeap);
			}
		
			T item = mHeapList [mHeapList.Count - 1];
			mHeapList.RemoveAt (mHeapList.Count - 1);
			return (item);
		}
		/// <summary>
		/// Push the specified item.
		/// </summary>
		/// <param name='item'>
		/// Item.
		/// </param>
		public void Push (T item)
		{
			int position = mHeapList.BinarySearch (item, mHeapComparer);
			if (position < 0) {
				mHeapList.Insert (-position - 1, item);
			}
		}
		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name='item'>
		/// Item.
		/// </param>
		public void Remove (T item)
		{
			mHeapList.Remove (item);
		}
		/// <summary>
		/// Removes at index.
		/// </summary>
		/// <param name='index'>
		/// Index.
		/// </param>
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