using System.Collections.Generic;
using UnityEngine;

public abstract class PollingPool<T> where T : Component
{
    private readonly T prefab;
 
    private readonly Stack<T> pool = new ();
    private readonly LinkedList<T> inuse = new ();
    private readonly Stack<LinkedListNode<T>> nodePool = new ();
 
    private int lastCheckFrame = -1;
 
    protected PollingPool(T prefab)
    {
        this.prefab = prefab;
    }
 
    private void CheckInUse()
    {
        var node = inuse.First;
        while (node != null)
        {
            var current = node;
            node = node.Next;
 
            if (!IsActive(current.Value))
            {
                current.Value.gameObject.SetActive(false);
                pool.Push(current.Value);
                inuse.Remove(current);
                nodePool.Push(current);
            }
        }
    }
 
    protected T _Get()
    {
        T item;
 
        if (lastCheckFrame != Time.frameCount)
        {
            lastCheckFrame = Time.frameCount;
            CheckInUse();
        }
 
        if (pool.Count == 0)
            item = GameObject.Instantiate(prefab);
        else
            item = pool.Pop();
 
        if (nodePool.Count == 0)
            inuse.AddLast(item);
        else
        {
            var node = nodePool.Pop();
            node.Value = item;
            inuse.AddLast(node);
        }
         
        item.gameObject.SetActive(true);
 
        return item;
    }

    protected void PreWarm(int i)
    {
        while (pool.Count < i) 
            pool.Push(GameObject.Instantiate(prefab));
    }
 
    protected abstract bool IsActive(T component);
}