﻿using System;
using System.Collections;
using System.Collections.Generic;

/*
 Usage: 
 *  //the buffer contains 100 points
     private CircularBuffer<Vector3> buffer = new CircularBuffer<Vector3>(100);
 */

public class CircularBuffer<T>
{
    private Queue<T> queue;
    private int count;

    public CircularBuffer(int count)
    {
        queue = new Queue<T>(count);
        this.count = count;
    }

    public void Add(T obj)
    {
        if (queue.Count == count)
        {
            queue.Dequeue();
            queue.Enqueue(obj);
        }
        else
            queue.Enqueue(obj);
    }


    public T Read()
    {
        return queue.Dequeue();
    }

    public T Peek()
    {
        return queue.Peek();
    }

    public T[] ToArray()
    {
        return queue.ToArray();
    }

    public int Count
    {
        get
        {
            return count;
        }
    }
}