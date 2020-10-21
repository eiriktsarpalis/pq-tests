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

        [Property(MaxTest = 10_000)]
        public static void HeapSort_Should_Work(string[] inputs)
        {
            string[] expected = inputs.OrderBy(inp => inp).ToArray();
            string[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<string> HeapSort(string[] inputs)
            {
                var pq = new PriorityQueue<string, string>();
                pq.ValidateInternalState();

                foreach (string input in inputs)
                {
                    pq.Enqueue(input, input);
                    pq.ValidateInternalState();
                }

                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    pq.ValidateInternalState();
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

                pq.ValidateInternalState();
                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    pq.ValidateInternalState();
                }
            }
        }
    }
}
