using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Collections.Generic;
using AlgoKit.Collections.Heaps;

namespace PriorityQueue.Benchmarks
{
    [MemoryDiagnoser]
    public class UpdateBenchmarks
    {
        [Params(10, 50, 150, 500, 1000, 10_000, 1_000_000)]
        public int Size;

        private int[] _priorities;
        private PrioritySet<int, int> _prioritySet;
        private PairingHeap<int, int> _pairingHeap;
        private Dictionary<int, PairingHeapNode<int, int>> _pairingHeapNodes;

        [GlobalSetup]
        public void Initialize()
        {
            var random = new Random(42);
            _priorities = new int[Size];
            for (int i = 0; i < Size; i++)
            {
                _priorities[i] = random.Next();
            }

            _prioritySet = new PrioritySet<int, int>(initialCapacity: Size);
            _pairingHeap = new PairingHeap<int, int>(Comparer<int>.Default);
            _pairingHeapNodes = new Dictionary<int, PairingHeapNode<int, int>>(Size);
        }

        // [IterationSetup]
        // public void IterSetup()
        // {
        //     var prioritySet = _prioritySet;
        //     var priorities = _priorities;
        //     var pairingHeap = _pairingHeap;
        //     var pairingHeapNodes = _pairingHeapNodes;

        //     for (int i = 0; i < Size; i++)
        //     {
        //         prioritySet.Enqueue(i, priorities[i]);
        //         PairingHeapNode<int, int> node = pairingHeap.Add(priorities[i], i);
        //         pairingHeapNodes.Add(i, node);
        //     }
        // }

        // [IterationCleanup]
        // public void IterCleanup()
        // {
        //     _prioritySet.Clear();
        //     _pairingHeap.Clear();
        //     _pairingHeapNodes.Clear();
        // }

        [Benchmark]
        public void PrioritySet()
        {
            var queue = _prioritySet;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                queue.Enqueue(i, priorities[i]);
            }

            for (int i = 0; i < Size; i++)
            {
                queue.TryUpdate(i, -priorities[i]);
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
            var heapIndex = _pairingHeapNodes;
            var priorities = _priorities;

            for (int i = 0; i < Size; i++)
            {
                PairingHeapNode<int, int> node = heap.Add(priorities[i], i);
                heapIndex[i] = node;
            }

            for (int i = 0; i < Size; i++)
            {
                heap.Update(heapIndex[i], -priorities[i]);
            }

            while (heap.Count > 0)
            {
                heap.Pop();
            }
        }
    }
}
