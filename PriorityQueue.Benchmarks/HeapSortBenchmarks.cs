using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace PriorityQueue.Benchmarks
{
    [MemoryDiagnoser]
    public class HeapSortBenchmarks
    {
        [Params(30, 300, 3000, 30_000)]
        public int Size;

        private int[] _priorities;
        private PriorityQueue<int, int> _priorityQueue;
        private PrioritySet<int, int> _prioritySet;
        private int[] _linqSortBuffer;

        [GlobalSetup]
        public void Initialize()
        {
            var random = new Random(42);
            _priorities = new int[Size];
            for (int i = 0; i < Size; i++)
            {
                _priorities[i] = random.Next(20);
            }

            _priorityQueue = new PriorityQueue<int, int>(initialCapacity: Size);
            _prioritySet = new PrioritySet<int, int>(initialCapacity: Size);
            _linqSortBuffer = new int[Size];
        }

        [Benchmark(Baseline = true)]
        public void LinqSort()
        {
            int i = 0;
            foreach (int priority in _priorities.OrderBy(x => x))
            {
                _linqSortBuffer[i++] = priority;
            }
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

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
        }
    }
}
