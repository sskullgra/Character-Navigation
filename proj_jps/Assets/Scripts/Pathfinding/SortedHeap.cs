using System;
using System.Collections.Generic;

public class SortedHeap<T> {
    private List<T> hList;
	private IComparer<T> hComparer = null;
    public SortedHeap(IComparer<T> cmp)
    {
        if (cmp == null) throw new ArgumentNullException("Comparer invalid!");
        hComparer = cmp;
        hList = new List<T>();
    }

    public int Count{
        get {return hList.Count;}
    }

    public T this[int Index]{
        get
        {
            if (Index >= hList.Count || Index < 0)
                throw new ArgumentOutOfRangeException("Index is less than zero or Index is greater than Count.");
            return hList[Index];
        }
    }

    public bool Contains(T item){
        return hList.BinarySearch(item, hComparer) >= 0;
    }

    public bool Contains(T item, ref int index)
    {
        index = hList.BinarySearch(item, hComparer);
        return index >= 0;
    }

    public T Pop()
    {
        if (hList.Count == 0)
            throw new InvalidOperationException("The heap is empty.");
        T item = hList[hList.Count - 1];
        hList.RemoveAt(hList.Count - 1);
        return (item);
    }

    public void Push(T item){
        int position = hList.BinarySearch(item,hComparer);
        if(position < 0)hList.Insert(-position-1, item);       
    }


    public void Remove(T item){
        hList.Remove(item);
    }

    public void RemoveAt(int index){
        hList.RemoveAt(index);
    }
}

