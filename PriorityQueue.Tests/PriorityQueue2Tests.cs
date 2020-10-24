using System;
using Xunit;
using FsCheck.Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PriorityQueue.Tests
{
    public class PriorityQueue2Tests
    {
        [Fact]
        public static void Simple_Priority_Queue()
        {
            var pq = new PriorityQueue<int>();

            pq.Enqueue(1940);
            pq.Enqueue(1942);
            pq.Enqueue(1943);
            pq.Enqueue(1940);

            Assert.Equal(1940, pq.Dequeue());
            Assert.Equal(1940, pq.Dequeue());
            Assert.Equal(1942, pq.Dequeue());
            Assert.Equal(1943, pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Heapify()
        {
            var pq = new PriorityQueue<int>(new int[] { 1940, 1942, 1943, 1940 });

            Assert.Equal(1940, pq.Dequeue());
            Assert.Equal(1940, pq.Dequeue());
            Assert.Equal(1942, pq.Dequeue());
            Assert.Equal(1943, pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Enumeration()
        {
            var pq = new PriorityQueue<int>(new int[] { 1940, 1942, 1943, 1940 });

            int[] expected = new int[] { 1940, 1942, 1943, 1940 };
            Assert.Equal(expected, pq.ToArray());
        }

        [Property(MaxTest = 10_000)]
        public static void HeapSort_Should_Work(string[] inputs)
        {
            string[] expected = inputs.OrderBy(inp => inp).ToArray();
            string[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<string> HeapSort(string[] inputs)
            {
                var pq = new PriorityQueue<string>();
                ValidateState(pq);

                foreach (string input in inputs)
                {
                    pq.Enqueue(input);
                    ValidateState(pq);
                }

                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    ValidateState(pq);
                }
            }
        }

        [Property(MaxTest = 10_000)]
        public static void HeapSort_Ctor_Should_Work(string[] inputs)
        {
            string[] expected = inputs.OrderBy(inp => inp).ToArray();
            string[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<string> HeapSort(string[] inputs)
            {
                var pq = new PriorityQueue<string>(inputs);

                ValidateState(pq);
                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    ValidateState(pq);
                }
            }
        }

        private static void ValidateState<TElement>(PriorityQueue<TElement> pq)
        {
#if DEBUG
            pq.ValidateInternalState();
#endif
        }
    }
}
