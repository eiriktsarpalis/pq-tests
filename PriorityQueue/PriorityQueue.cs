using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PriorityQueue
{
    public class PriorityQueue<TElement, TPriority> : IReadOnlyCollection<(TElement Element, TPriority Priority)>, ICollection
    {
        private const int DefaultCapacity = 4;

        private readonly IComparer<TPriority> _priorityComparer;

        private TPriority[] _priorities;
        private TElement[] _elements;
        private int _count;
        private int _version;

        #region Constructors
        public PriorityQueue() : this(0, null)
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
            _priorityComparer = comparer ?? Comparer<TPriority>.Default;
            _priorities = Array.Empty<TPriority>();
            _elements = Array.Empty<TElement>();
            _count = 0;

            AppendRaw(values);
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

        public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> values)
        {
            _version++;
            if (_count == 0)
            {
                AppendRaw(values);
                Heapify();
            }
            else
            {
                foreach((TElement element, TPriority priority) in values)
                {
                    if (_count == _priorities.Length)
                    {
                        Resize(ref _priorities, ref _elements);
                    }

                    SiftUp(index: _count++, in element, in priority);
                }
            }
        }

        public TElement Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException();
            }

            return _elements[0];
        }

        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            element = _elements[0];
            priority = _priorities[0];
            return true;
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

        public void TrimExcess()
        {
            Debug.Assert(_priorities.Length == _elements.Length);

            int count = _count;
            int threshold = (int)(((double)_elements.Length) * 0.9);
            if (count < threshold)
            {
                Array.Resize(ref _elements, count);
                Array.Resize(ref _priorities, count);
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1)
                throw new ArgumentException("SR.Arg_RankMultiDimNotSupported", nameof(array));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "SR.ArgumentOutOfRange_Index");

            int arrayLen = array.Length;
            if (arrayLen - index < _count)
                throw new ArgumentException("SR.Argument_InvalidOffLen");

            int numToCopy = _count;
            TElement[] elements = _elements;
            TPriority[] priorities = _priorities;

            for (int i = 0; i < numToCopy; i++)
            {
                array.SetValue((elements[i], priorities[i]), index + i);
            }
        }

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

        public ElementCollection Elements => new ElementCollection(this);

        public class ElementCollection : IReadOnlyCollection<TElement>, ICollection
        {
            private readonly PriorityQueue<TElement, TPriority> _priorityQueue;

            internal ElementCollection(PriorityQueue<TElement, TPriority> priorityQueue)
            {
                _priorityQueue = priorityQueue;
            }

            public int Count => _priorityQueue.Count;
            public bool IsSynchronized => false;
            public object SyncRoot => _priorityQueue;

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (array.Rank != 1)
                    throw new ArgumentException("SR.Arg_RankMultiDimNotSupported", nameof(array));
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "SR.ArgumentOutOfRange_Index");

                int arrayLen = array.Length;
                if (arrayLen - index < _priorityQueue._count)
                    throw new ArgumentException("SR.Argument_InvalidOffLen");

                Array.Copy(_priorityQueue._elements, 0, array, index, _priorityQueue._count);
            }

            public IEnumerator<TElement> GetEnumerator() => new Enumerator(_priorityQueue);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_priorityQueue);

            public struct Enumerator : IEnumerator<TElement>, IEnumerator
            {
                private readonly PriorityQueue<TElement, TPriority> _queue;
                private readonly int _version;
                private int _index;
                private TElement _current;

                internal Enumerator(PriorityQueue<TElement, TPriority> queue)
                {
                    _version = queue._version;
                    _queue = queue;
                    _index = 0;
                    _current = default!;
                }

                public bool MoveNext()
                {
                    PriorityQueue<TElement, TPriority> queue = _queue;

                    if (queue._version == _version && _index < queue._count)
                    {
                        _current = queue._elements[_index];
                        _index++;
                        return true;
                    }

                    if (queue._version != _version)
                    {
                        throw new InvalidOperationException("collection was modified");
                    }

                    return false;
                }

                public TElement Current => _current;
                object? IEnumerator.Current => _current;

                public void Reset()
                {
                    if (_queue._version != _version)
                    {
                        throw new InvalidOperationException("collection was modified");
                    }

                    _index = 0;
                    _current = default!;
                }

                public void Dispose()
                {
                }
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

        private void AppendRaw(IEnumerable<(TElement Element, TPriority Priority)> values)
        {
            // TODO: specialize on ICollection types
            var priorities = _priorities;
            var elements = _elements;
            int count = _count;

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
            _count = count;
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