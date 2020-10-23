using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PriorityQueue
{
    public class PriorityQueue<TElement, TPriority> : IReadOnlyCollection<(TElement Element, TPriority Priority)>
    {
        private const int DefaultCapacity = 4;

        private readonly IComparer<TPriority> _priorityComparer;

        private TPriority[] _priorities;
        private TElement[] _elements;
        private int _count;
        private int _version;

        #region Constructors
        public PriorityQueue() : this(0, Comparer<TPriority>.Default)
        {

        }

        public PriorityQueue(int initialCapacity) : this(initialCapacity, null)
        {

        }

        public PriorityQueue(IComparer<TPriority>? comparer) : this(0, comparer)
        {

        }

        public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            if (initialCapacity == 0)
            {
                _priorities = Array.Empty<TPriority>();
                _elements = Array.Empty<TElement>();
            }
            else
            {
                _priorities = new TPriority[initialCapacity];
                _elements = new TElement[initialCapacity];
            }

            _priorityComparer = comparer ?? Comparer<TPriority>.Default;
        }

        public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> values) : this(values, null)
        {

        }

        public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> values, IComparer<TPriority>? comparer) 
        {
            var priorities = Array.Empty<TPriority>();
            var elements = Array.Empty<TElement>();
            int count = 0;

            foreach ((TElement element, TPriority priority) in values)
            {
                if (count == priorities.Length)
                {
                    Resize(ref priorities, ref elements);
                }

                priorities[count] = priority;
                elements[count] = element;
                count++;
            }

            _priorities = priorities;
            _elements = elements;
            _priorityComparer = comparer ?? Comparer<TPriority>.Default;
            _count = count;

            Heapify();
        }
        #endregion

        public int Count => _count;
        public IComparer<TPriority> Comparer => _priorityComparer;

        public void Enqueue(TElement element, TPriority priority)
        {
            _version++;
            if (_count == _priorities.Length)
            {
                Resize(ref _priorities, ref _elements);
            }

            SiftUp(index: _count++, in element, in priority);
        }

        public TElement EnqueueDequeue(TElement element, TPriority priority)
        {
            if (_count == 0 || _priorityComparer.Compare(priority, _priorities[0]) <= 0)
            {
                return element;
            }

            _version++;
            TElement minElement = _elements[0];
            SiftDown(index: 0, in element, in priority);
            return minElement;
        }

        public TElement Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException();
            }

            return _elements[0];
        }

        public TElement Dequeue()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException();
            }

            _version++;
            RemoveIndex(index: 0, out TElement result, out _);
            return result;
        }

        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            _version++;
            RemoveIndex(index: 0, out element, out priority);
            return true;
        }

        public void Clear()
        {
            _version++;
            if (_count > 0)
            {
                Array.Clear(_priorities, 0, _count);
                Array.Clear(_elements, 0, _count);
                _count = 0;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>, IEnumerator
        {
            private readonly PriorityQueue<TElement, TPriority> _queue;
            private readonly int _version;
            private int _index;
            private (TElement Element, TPriority Priority) _current;

            internal Enumerator(PriorityQueue<TElement, TPriority> queue)
            {
                _version = queue._version;
                _queue = queue;
                _index = 0;
                _current = default;
            }

            public bool MoveNext()
            {
                PriorityQueue<TElement, TPriority> queue = _queue;

                if (queue._version == _version && _index < queue._count)
                {
                    _current = (queue._elements[_index], queue._priorities[_index]);
                    _index++;
                    return true;
                }

                if (queue._version != _version)
                {
                    throw new InvalidOperationException("collection was modified");
                }

                return false;
            }

            public (TElement Element, TPriority Priority) Current => _current;
            object IEnumerator.Current => _current;

            public void Reset()
            {
                if (_queue._version != _version)
                {
                    throw new InvalidOperationException("collection was modified");
                }

                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
            }
        }

        #region Private Methods
        private void Heapify()
        {
            for (int i = (_count - 1) >> 2; i >= 0; i--)
            {
                TElement element = _elements[i];
                TPriority priority = _priorities[i];
                SiftDown(i, element, priority);
            }
        }

        private void RemoveIndex(int index, out TElement element, out TPriority priority)
        {
            Debug.Assert(index < _count);

            element = _elements[index];
            priority = _priorities[index];

            int lastElementPos = --_count;
            ref TElement lastElement = ref _elements[lastElementPos];
            ref TPriority lastPriority = ref _priorities[lastElementPos];

            if (lastElementPos > 0)
            {
                SiftDown(index, in lastElement, in lastPriority);
            }

            lastElement = default!;
            lastPriority = default!;
        }

        private void SiftUp(int index, in TElement element, in TPriority priority)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 2;
                ref TPriority parentPriority = ref _priorities[parentIndex];

                if (_priorityComparer.Compare(parentPriority, priority) <= 0)
                {
                    // parentPriority <= priority, heap property is satisfed
                    break;
                }

                _priorities[index] = parentPriority;
                _elements[index] = _elements[parentIndex];
                index = parentIndex;
            }

            _priorities[index] = priority;
            _elements[index] = element;
        }

        private void SiftDown(int index, in TElement element, in TPriority priority)
        {
            int minChildIndex;
            int count = _count;
            TPriority[] priorities = _priorities;
            TElement[] elements = _elements;

            while ((minChildIndex = (index << 2) + 1) < count)
            {
                // find the child with the minimal priority
                ref TPriority minChildPriority = ref priorities[minChildIndex];
                int childUpperBound = Math.Min(count, minChildIndex + 4);

                for (int nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
                {
                    ref TPriority nextChildPriority = ref priorities[nextChildIndex];
                    if (_priorityComparer.Compare(nextChildPriority, minChildPriority) < 0)
                    {
                        minChildIndex = nextChildIndex;
                        minChildPriority = ref nextChildPriority;
                    }
                }

                // compare with inserted priority
                if (_priorityComparer.Compare(priority, minChildPriority) <= 0)
                {
                    // priority <= childPriority, heap property is satisfied
                    break;
                }

                priorities[index] = minChildPriority;
                elements[index] = elements[minChildIndex];
                index = minChildIndex;
            }

            priorities[index] = priority;
            elements[index] = element;
        }

        private void Resize(ref TPriority[] priorities, ref TElement[] elements)
        {
            Debug.Assert(priorities.Length == elements.Length);

            int newSize = priorities.Length == 0 ? DefaultCapacity : 2 * priorities.Length;

            Array.Resize(ref priorities, newSize);
            Array.Resize(ref elements, newSize);
        }

#if DEBUG
        public void ValidateInternalState()
        {
            if (_elements.Length < _count)
            {
                throw new Exception("invalid elements array length");
            }

            if (_priorities.Length < _count)
            {
                throw new Exception("invalid priorities array length");
            }

            foreach ((var element, var idx) in _elements.Select((x, i) => (x, i)).Skip(_count))
            {
                if (!IsDefault(element))
                {
                    throw new Exception($"Non-zero element '{element}' at index {idx}.");
                }
            }

            foreach ((var priority, var idx) in _priorities.Select((x, i) => (x, i)).Skip(_count))
            {
                if (!IsDefault(priority))
                {
                    throw new Exception($"Non-zero priority '{priority}' at index {idx}.");
                }
            }

            static bool IsDefault<T>(T value)
            {
                T defaultVal = default;

                if (defaultVal is null)
                {
                    return value is null;
                }

                return value!.Equals(defaultVal);
            }
        }
#endif

        #endregion
    }
}