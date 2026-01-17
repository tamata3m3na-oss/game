using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Stack<T> pool;
    private readonly Func<T> createFunc;
    private readonly Action<T> onGet;
    private readonly Action<T> onRelease;
    private readonly Action<T> onDestroy;
    private readonly bool collectionCheck;
    private readonly int maxSize;
    
    public int CountAll { get; private set; }
    public int CountActive { get; private set; }
    public int CountInactive => CountAll - CountActive;
    
    public ObjectPool(Func<T> createFunc, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 100)
    {
        if (createFunc == null)
        {
            throw new ArgumentNullException(nameof(createFunc));
        }
        
        this.createFunc = createFunc;
        this.onGet = onGet;
        this.onRelease = onRelease;
        this.onDestroy = onDestroy;
        this.collectionCheck = collectionCheck;
        this.maxSize = maxSize;
        
        pool = new Stack<T>(defaultCapacity);
    }
    
    public T Get()
    {
        T item;
        
        if (pool.Count == 0)
        {
            item = createFunc();
            CountAll++;
        }
        else
        {
            item = pool.Pop();
        }
        
        CountActive++;
        onGet?.Invoke(item);
        item.gameObject.SetActive(true);
        
        return item;
    }
    
    public void Release(T item)
    {
        if (pool.Count > maxSize)
        {
            onDestroy?.Invoke(item);
            CountAll--;
            return;
        }
        
        if (collectionCheck && pool.Contains(item))
        {
            throw new InvalidOperationException("Trying to release an item that is already in the pool");
        }
        
        CountActive--;
        onRelease?.Invoke(item);
        item.gameObject.SetActive(false);
        pool.Push(item);
    }
    
    public void Clear()
    {
        if (onDestroy != null)
        {
            foreach (var item in pool)
            {
                onDestroy(item);
            }
        }
        
        pool.Clear();
        CountActive = 0;
        CountAll = 0;
    }
}