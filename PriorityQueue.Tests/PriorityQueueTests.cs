using System;
using Xunit;
using FsCheck.Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PriorityQueue.Tests
{
    public class PriorityQueueTests
    {
        [Fact]
        public static void Simple_Priority_Queue()
        {
            var pq = new PriorityQueue<string, int>();

            pq.Enqueue("John", 1940);
            pq.Enqueue("Paul", 1942);
            pq.Enqueue("George", 1943);
            pq.Enqueue("Ringo", 1940);

            Assert.Equal("John", pq.Dequeue());
            Assert.Equal("Ringo", pq.Dequeue());
            Assert.Equal("Paul", pq.Dequeue());
            Assert.Equal("George", pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Heapify()
        {
            var pq = new PriorityQueue<string, int>(new (string, int)[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1940) });

            Assert.Equal("John", pq.Dequeue());
            Assert.Equal("Ringo", pq.Dequeue());
            Assert.Equal("Paul", pq.Dequeue());
            Assert.Equal("George", pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Enumeration()
        {
            var pq = new PriorityQueue<string, int>(new (string, int)[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1940) });

            (string, int)[] expected = new[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1940) };
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
                var pq = new PriorityQueue<string, string>();
                ValidateState(pq);

                foreach (string input in inputs)
                {
                    pq.Enqueue(input, input);
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
                var pq = new PriorityQueue<string, string>(inputs.Select(x => (x,x)));

                ValidateState(pq);
                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    ValidateState(pq);
                }
            }
        }

        private static void ValidateState<TElement, TPriority>(PriorityQueue<TElement, TPriority> pq)
        {
#if DEBUG
            pq.ValidateInternalState();
#endif
        }
    }
}
