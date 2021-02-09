using System;
using Xunit;
using FsCheck.Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PriorityQueue.Tests
{
    public class PriorityQueue_BinaryTests
    {
        [Fact]
        public static void Simple_Priority_Queue()
        {
            var pq = new PriorityQueue_Binary<string, int>();

            pq.Enqueue("John", 1940);
            pq.Enqueue("Paul", 1942);
            pq.Enqueue("George", 1943);
            pq.Enqueue("Ringo", 1941);

            Assert.Equal("John", pq.Dequeue());
            Assert.Equal("Ringo", pq.Dequeue());
            Assert.Equal("Paul", pq.Dequeue());
            Assert.Equal("George", pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Heapify()
        {
            var pq = new PriorityQueue_Binary<string, int>(new (string, int)[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1941) });

            Assert.Equal("John", pq.Dequeue());
            Assert.Equal("Ringo", pq.Dequeue());
            Assert.Equal("Paul", pq.Dequeue());
            Assert.Equal("George", pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Enumeration()
        {
            var pq = new PriorityQueue_Binary<string, int>(new (string, int)[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1941) });

            (string, int)[] expected = new[] { ("John", 1940), ("Ringo", 1941), ("George", 1943), ("Paul", 1942) };
            Assert.Equal(expected, pq.UnorderedItems.ToArray());
        }

        [Property(MaxTest = 10_000)]
        public static void HeapSort_Should_Work(string[] inputs)
        {
            string[] expected = inputs.OrderBy(inp => inp).ToArray();
            string[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<string> HeapSort(string[] inputs)
            {
                var pq = new PriorityQueue_Binary<string, string>();
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
                var pq = new PriorityQueue_Binary<string, string>(inputs.Select(x => (x, x)));

                ValidateState(pq);
                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    ValidateState(pq);
                }
            }
        }

        private static void ValidateState<TElement, TPriority>(PriorityQueue_Binary<TElement, TPriority> pq)
        {
#if DEBUG
            pq.ValidateInternalState();
#endif
        }
    }
}
