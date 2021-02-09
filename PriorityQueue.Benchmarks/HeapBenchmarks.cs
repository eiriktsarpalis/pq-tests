using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Collections.Generic;
using AlgoKit.Collections.Heaps;

namespace PriorityQueue.Benchmarks
{
    [MemoryDiagnoser]
    public class HeapBenchmarks
    {
        [Params(10, 50, 150, 500, 1000, 10_000, 1_000_000)]
        public int Size;

        private int[] _priorities;
        private PriorityQueue<int> _priorityQueue2;
        private PriorityQueue<int, int> _priorityQueue;
        private PriorityQueue_Binary<int, int> _priorityQueueBinary;
        private PriorityQueue_Comparable<int, int> _priorityQueueComparable;
        private PriorityQueue_InlineComparer<int, int> _priorityQueueInlineComparer;
        private PrioritySet<int, int> _prioritySet;
        private PairingHeap<int, int> _pairingHeap;

        [GlobalSetup]
        public void Initialize()
        {
            var random = new Random(42);
            _priorities = new int[2 * Size];
            for (int i = 0; i < 2 * Size; i++)
            {
                _priorities[i] = random.Next();
            }

            _priorityQueueBinary = new PriorityQueue_Binary<int, int>(initialCapacity: Size);
            _priorityQueueComparable = new PriorityQueue_Comparable<int, int>(initialCapacity: Size);
            _priorityQueueInlineComparer = new PriorityQueue_InlineComparer<int, int>(initialCapacity: Size);
            _priorityQueue2 = new PriorityQueue<int>(initialCapacity: Size);
            _priorityQueue = new PriorityQueue<int, int>(initialCapacity: Size);
            _prioritySet = new PrioritySet<int, int>(initialCapacity: Size);
            _pairingHeap = new PairingHeap<int, int>(Comparer<int>.Default);
        }

        [Benchmark]
        public void PriorityQueue()
        {
            var queue = _priorityQueue;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(i, priorities[i]);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                queue.Dequeue();
                queue.Enqueue(i, priorities[i]);
            }

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }

        [Benchmark]
        public void PriorityQueue_Binary()
        {
            var queue = _priorityQueueBinary;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(i, priorities[i]);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                queue.Dequeue();
                queue.Enqueue(i, priorities[i]);
            }

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }

        [Benchmark]
        public void PriorityQueue_InlineComparer()
        {
            var queue = _priorityQueueInlineComparer;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(i, priorities[i]);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                queue.Dequeue();
                queue.Enqueue(i, priorities[i]);
            }

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }

        [Benchmark]
        public void PriorityQueue_Comparable()
        {
            var queue = _priorityQueueComparable;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(i, priorities[i]);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                queue.Dequeue();
                queue.Enqueue(i, priorities[i]);
            }

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }

        [Benchmark]
        public void PriorityQueue2()
        {
            var queue = _priorityQueue2;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(priorities[i]);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                queue.Dequeue();
                queue.Enqueue(priorities[i]);
            }

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }

        [Benchmark]
        public void PrioritySet()
        {
            var queue = _prioritySet;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(i, priorities[i]);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                queue.Dequeue();
                queue.Enqueue(i, priorities[i]);
            }

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }

        [Benchmark]
        public void PairingHeap()
        {
            var heap = _pairingHeap;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                heap.Add(priorities[i], i);
            }

            for (int i = Size; i < 2 * Size; i++)
            {
                heap.Pop();
                heap.Add(priorities[i], i);
            }

            while (heap.Count > 0)
            {
                heap.Pop();
            }
        }
    }
}
