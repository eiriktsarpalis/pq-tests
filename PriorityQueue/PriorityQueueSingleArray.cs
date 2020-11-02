using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PriorityQueue
{
    public class PriorityQueueSingleArray<TElement, TPriority> : IReadOnlyCollection<(TElement Element, TPriority Priority)>, ICollection
    {
        private const int DefaultCapacity = 4;

        private readonly IComparer<TPriority> _priorityComparer;

        private (TElement Element, TPriority Priority)[] _heap;
        private int _count;
        private int _version;

        #region Constructors
        public PriorityQueueSingleArray() : this(0, null)
        {

        }

        public PriorityQueueSingleArray(int initialCapacity) : this(initialCapacity, null)
        {

        }

        public PriorityQueueSingleArray(IComparer<TPriority>? comparer) : this(0, comparer)
        {

        }

        public PriorityQueueSingleArray(int initialCapacity, IComparer<TPriority>? comparer)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            if (initialCapacity == 0)
            {
                _heap = Array.Empty<(TElement, TPriority)>();
            }
            else
            {
                _heap = new (TElement, TPriority)[initialCapacity];
            }

            _priorityComparer = comparer ?? Comparer<TPriority>.Default;
        }

        public PriorityQueueSingleArray(IEnumerable<(TElement Element, TPriority Priority)> values) : this(values, null)
        {

        }

        public PriorityQueueSingleArray(IEnumerable<(TElement Element, TPriority Priority)> values, IComparer<TPriority>? comparer)
        {
            _priorityComparer = comparer ?? Comparer<TPriority>.Default;
            _heap = Array.Empty<(TElement, TPriority)>();
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
            if (_count == _heap.Length)
            {
                Resize(ref _heap);
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
                foreach ((TElement element, TPriority priority) in values)
                {
                    if (_count == _heap.Length)
                    {
                        Resize(ref _heap);
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

            return _heap[0].Element;
        }

        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            (element, priority) = _heap[0];
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
            if (_count == 0 || _priorityComparer.Compare(priority, _heap[0].Priority) <= 0)
            {
                return element;
            }

            _version++;
            TElement minElement = _heap[0].Element;
            SiftDown(index: 0, in element, in priority);
            return minElement;
        }

        public void Clear()
        {
            _version++;
            if (_count > 0)
            {
                Array.Clear(_heap, 0, _count);
                _count = 0;
            }
        }

        public void TrimExcess()
        {
            int count = _count;
            int threshold = (int)(((double)_heap.Length) * 0.9);
            if (count < threshold)
            {
                Array.Resize(ref _heap, count);
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
            (TElement Element, TPriority Priority)[] heap = _heap;

            for (int i = 0; i < numToCopy; i++)
            {
                array.SetValue(heap[i], index + i);
            }
        }

        public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>, IEnumerator
        {
            private readonly PriorityQueueSingleArray<TElement, TPriority> _queue;
            private readonly int _version;
            private int _index;
            private (TElement Element, TPriority Priority) _current;

            internal Enumerator(PriorityQueueSingleArray<TElement, TPriority> queue)
            {
                _version = queue._version;
                _queue = queue;
                _index = 0;
                _current = default;
            }

            public bool MoveNext()
            {
                PriorityQueueSingleArray<TElement, TPriority> queue = _queue;

                if (queue._version == _version && _index < queue._count)
                {
                    _current = queue._heap[_index];
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
            private readonly PriorityQueueSingleArray<TElement, TPriority> _priorityQueue;

            internal ElementCollection(PriorityQueueSingleArray<TElement, TPriority> priorityQueue)
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
                
                int numToCopy = _priorityQueue._count;
                (TElement Element, TPriority Priority)[] heap = _priorityQueue._heap;

                for (int i = 0; i < numToCopy; i++)
                {
                    array.SetValue(heap[i], index + i);
                }
            }

            public IEnumerator<TElement> GetEnumerator() => new Enumerator(_priorityQueue);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_priorityQueue);

            public struct Enumerator : IEnumerator<TElement>, IEnumerator
            {
                private readonly PriorityQueueSingleArray<TElement, TPriority> _queue;
                private readonly int _version;
                private int _index;
                private TElement _current;

                internal Enumerator(PriorityQueueSingleArray<TElement, TPriority> queue)
                {
                    _version = queue._version;
                    _queue = queue;
                    _index = 0;
                    _current = default!;
                }

                public bool MoveNext()
                {
                    PriorityQueueSingleArray<TElement, TPriority> queue = _queue;

                    if (queue._version == _version && _index < queue._count)
                    {
                        _current = queue._heap[_index].Element;
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
            (TElement Element, TPriority Priority)[] heap = _heap;

            for (int i = (_count - 1) >> 2; i >= 0; i--)
            {
                (TElement element, TPriority priority) = heap[i];
                SiftDown(i, element, priority);
            }
        }

        private void AppendRaw(IEnumerable<(TElement Element, TPriority Priority)> values)
        {
            // TODO: specialize on ICollection types
            var heap = _heap;
            int count = _count;

            foreach ((TElement, TPriority) pair in values)
            {
                if (count == heap.Length)
                {
                    Resize(ref heap);
                }

                heap[count] = pair;
                count++;
            }

            _heap = heap;
            _count = count;
        }

        private void RemoveIndex(int index, out TElement element, out TPriority priority)
        {
            Debug.Assert(index < _count);

            (element, priority) = _heap[index];

            int lastElementPos = --_count;
            ref (TElement Element, TPriority Priority) lastElement = ref _heap[lastElementPos];

            if (lastElementPos > 0)
            {
                SiftDown(index, in lastElement.Element, in lastElement.Priority);
            }

            lastElement = default;
        }

        private void SiftUp(int index, in TElement element, in TPriority priority)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 2;
                ref (TElement Element, TPriority Priority) parent = ref _heap[parentIndex];

                if (_priorityComparer.Compare(parent.Priority, priority) <= 0)
                {
                    // parentPriority <= priority, heap property is satisfed
                    break;
                }

                _heap[index] = parent;
                index = parentIndex;
            }

            _heap[index] = (element, priority);
        }

        private void SiftDown(int index, in TElement element, in TPriority priority)
        {
            int minChildIndex;
            int count = _count;
            (TElement Element, TPriority Priority)[] heap = _heap;

            while ((minChildIndex = (index << 2) + 1) < count)
            {
                // find the child with the minimal priority
                ref (TElement Element, TPriority Priority) minChild = ref heap[minChildIndex];
                int childUpperBound = Math.Min(count, minChildIndex + 4);

                for (int nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
                {
                    ref (TElement Element, TPriority Priority) nextChild = ref heap[nextChildIndex];
                    if (_priorityComparer.Compare(nextChild.Priority, minChild.Priority) < 0)
                    {
                        minChildIndex = nextChildIndex;
                        minChild = ref nextChild;
                    }
                }

                // compare with inserted priority
                if (_priorityComparer.Compare(priority, minChild.Priority) <= 0)
                {
                    // priority <= childPriority, heap property is satisfied
                    break;
                }

                heap[index] = minChild;
                index = minChildIndex;
            }

            heap[index] = (element, priority);
        }

        private void Resize(ref (TElement, TPriority)[] heap)
        {
            int newSize = heap.Length == 0 ? DefaultCapacity : 2 * heap.Length;

            Array.Resize(ref heap, newSize);
        }

#if DEBUG
        public void ValidateInternalState()
        {
            if (_heap.Length < _count)
            {
                throw new Exception("invalid elements array length");
            }

            foreach ((var element, var idx) in _heap.Select((x, i) => (x.Element, i)).Skip(_count))
            {
                if (!IsDefault(element))
                {
                    throw new Exception($"Non-zero element '{element}' at index {idx}.");
                }
            }

            foreach ((var priority, var idx) in _heap.Select((x, i) => (x.Priority, i)).Skip(_count))
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