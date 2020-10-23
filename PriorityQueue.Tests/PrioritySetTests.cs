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

        [Fact]
        public static void Simple_Priority_Queue_Heapify()
        {
            var pq = new PrioritySet<string, int>(new (string, int)[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1940) });

            Assert.Equal("John", pq.Dequeue());
            Assert.Equal("Ringo", pq.Dequeue());
            Assert.Equal("Paul", pq.Dequeue());
            Assert.Equal("George", pq.Dequeue());
        }

        [Fact]
        public static void Simple_Priority_Queue_Enumeration()
        {
            var pq = new PrioritySet<string, int>(new (string, int)[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1940) });

            (string, int)[] expected = new[] { ("John", 1940), ("Paul", 1942), ("George", 1943), ("Ringo", 1940) };
            Assert.Equal(expected, pq.ToArray());
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
                ValidateState(pq);

                foreach (int input in inputs)
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

        [Property(MaxTest = 10_000, Arbitrary = new Type[] { typeof(RandomGenerators) })]
        public static void HeapSort_Ctor_Should_Work(int[] inputs)
        {
            int[] expected = inputs.OrderBy(inp => inp).ToArray();
            int[] actual = HeapSort(inputs).ToArray();
            Assert.Equal(expected, actual);

            static IEnumerable<int> HeapSort(int[] inputs)
            {
                var pq = new PrioritySet<int, int>(inputs.Select(x => (x, x)));

                ValidateState(pq);
                Assert.Equal(inputs.Length, pq.Count);

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    ValidateState(pq);
                }
            }
        }

        [Property(MaxTest = 10_000, Arbitrary = new Type[] { typeof(RandomGenerators) })]
        public static void Removing_Elements_Should_Work(int[] inputs)
        {
            var pq = new PrioritySet<int, int>(inputs.Select(x => (x, x)));
            ValidateState(pq);

            for (int i = 0; i < inputs.Length; i++)
            {
                Assert.True(pq.TryRemove(inputs[i]));
                Assert.Equal(inputs.Length - i - 1, pq.Count);
                ValidateState(pq);
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
                ValidateState(pq);

                for (int i = 0; i < inputs.Length; i++)
                {
                    Assert.True(pq.TryUpdate(inputs[i], inputs[i]));
                    Assert.Equal(inputs.Length, pq.Count);
                    ValidateState(pq);
                }

                while (pq.Count > 0)
                {
                    yield return pq.Dequeue();
                    ValidateState(pq);
                }
            }
        }

        private static void ValidateState<TElement, TPriority>(PrioritySet<TElement, TPriority> ps)
        {
#if DEBUG
            ps.ValidateInternalState();
#endif
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
