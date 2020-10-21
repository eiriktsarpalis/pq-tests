using System;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PriorityQueue.Tests
{
    public static class PrioritySetTests
    {
        [Fact]
        public static void Simple_Priority_Queue()
        {
            var pq = new PrioritySet<string, int>();

            pq.Enqueue("John", 1940);
            pq.Enqueue("Paul", 1942);
            pq.Enqueue("George", 1943);
            pq.Enqueue("Ringo", 1940);

            Assert.Equal("John", pq.Dequeue());
            Assert.Equal("Ringo", pq.Dequeue());
            Assert.Equal("Paul", pq.Dequeue());
            Assert.Equal("George", pq.Dequeue());
        }

        [Property(MaxTest = 10_000, Arbitrary = new Type[] { typeof(RandomGenerators) })]
        public static void HeapSort_Should_Work(int[] inputs)
        {
            int[] expected = inputs.OrderBy(inp => inp).ToArray();
            int[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<int> HeapSort(int[] inputs)
            {
                var pq = new PrioritySet<int, int>();
                pq.ValidateInternalState();

                foreach (int input in inputs)
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

        [Property(MaxTest = 10_000, Arbitrary = new Type[] { typeof(RandomGenerators) })]
        public static void HeapSort_Ctor_Should_Work(int[] inputs)
        {
            int[] expected = inputs.OrderBy(inp => inp).ToArray();
            int[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<int> HeapSort(int[] inputs)
            {
                var pq = new PrioritySet<int, int>(inputs.Select(x => (x, x)));

                pq.ValidateInternalState();
                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    pq.ValidateInternalState();
                }
            }
        }

        [Property(MaxTest = 10_000, Arbitrary = new Type[] { typeof(RandomGenerators) })]
        public static void Removing_Elements_Should_Work(int[] inputs)
        {
            var pq = new PrioritySet<int, int>(inputs.Select(x => (x, x)));
            pq.ValidateInternalState();

            for (int i = 0; i < inputs.Length; i++)
            {
                Assert.True(pq.TryRemove(inputs[i]));
                Assert.Equal(inputs.Length - i - 1, pq.Count);
                pq.ValidateInternalState();
            }

            Assert.Empty(pq);
        }

        [Property(MaxTest = 10_000, Arbitrary = new Type[] { typeof(RandomGenerators) })]
        public static void HeapSort_UsingUpdate_Should_Work(int[] inputs)
        {
            var expected = inputs.OrderByDescending(x => x).ToArray();
            var actual = HeapSort(inputs).ToArray();

            static IEnumerable<int> HeapSort(int[] inputs)
            {
                var pq = new PrioritySet<int, int>(inputs.Select(x => (x, 0)));
                pq.ValidateInternalState();

                for (int i = 0; i < inputs.Length; i++)
                {
                    Assert.True(pq.TryUpdate(inputs[i], inputs[i]));
                    Assert.Equal(inputs.Length, pq.Count);
                    pq.ValidateInternalState();
                }

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    pq.ValidateInternalState();
                }
            }
        }

        public static class RandomGenerators
        {
            public static Arbitrary<string[]> DeduplicatedStringArray() => DeDuplicatedArray<string>();
            public static Arbitrary<int[]> DeduplicatedIntArray() => DeDuplicatedArray<int>();

            private static Arbitrary<T[]> DeDuplicatedArray<T>()
            {
                Arbitrary<T[]> defaultArray = Arb.Default.Array<T>();

                Gen<T[]> dedupedArray =
                    from array in defaultArray.Generator
                    select DeDup(array);

                return Arb.From(dedupedArray, Shrink);

                IEnumerable<T[]> Shrink(T[] input)
                {
                    foreach (T[] shrunk in defaultArray.Shrinker(input))
                    {
                        yield return DeDup(shrunk);
                    }
                }

                T[] DeDup (T[] input)
                {
                    return input
                        .Where(input => input is not null)
                        .Distinct()
                        .ToArray();
                }
            }
        }
    }
}
