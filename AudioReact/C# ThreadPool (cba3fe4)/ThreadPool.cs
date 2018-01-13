using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadPool
{
    private Thread[] threadsActive;
    private Queue<Thread> threadsQueue;
    public int MaxThreads { get; private set; }
    public int MaxJoinTime { get; private set; }

    public ThreadPool(int maxThreads, int maxJoinTime)
    {
        MaxThreads = maxThreads;
        MaxJoinTime = maxJoinTime;
        threadsActive = new Thread[MaxThreads];
        threadsQueue = new Queue<Thread>();
    }

    ~ThreadPool()
    {
        for (int i = 0; i < MaxThreads; i++)
        {
            KillThread(i);
        }
    }

    public void OnUpdate()
    {
        for (int i = 0; i < MaxThreads; i++)
        {
            if (threadsActive[i] != null)
            {
                if (!threadsActive[i].IsAlive)
                {
                    KillThread(i);
                }
            }
            else
            {
                if (threadsQueue.Count > 0)
                {
                    threadsActive[i] = threadsQueue.Dequeue();
                    threadsActive[i].Start();
                }
            }
        }
    }

    private void KillThread(int index)
    {
        if (threadsActive[index] != null)
        {
            threadsActive[index].Interrupt();

            if (!threadsActive[index].Join(MaxJoinTime))
            {
                threadsActive[index].Abort();
            }

            threadsActive[index] = null;
        }

        return;
    }

    public void CreateThread(Action action, string name)
    {
        Thread thread = new Thread(new ThreadStart(action));
        thread.Name = name;
        thread.IsBackground = true;

        int freeElement = Array.IndexOf(threadsActive, null);

        if (freeElement != -1)
        {
            threadsActive[freeElement] = thread;
            thread.Start();
        }
        else
        {
            threadsQueue.Enqueue(thread);
        }
    }

    public void JoinThread(int index)
    {
        if (threadsActive[index] != null)
        {
            threadsActive[index].Join();
        }
    }

    public string GetThreadName(int index)
    {
        if (threadsActive[index] != null)
        {
            return threadsActive[index].Name;
        }

        return null;
    }
}