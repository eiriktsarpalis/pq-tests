using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PriorityQueue
{
    public class PriorityQueue<TElement> : IReadOnlyCollection<TElement>
    {
        private const int DefaultCapacity = 4;

        private readonly IComparer<TElement> _priorityComparer;

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

        public PriorityQueue(IComparer<TElement>? comparer) : this(0, comparer)
        {

        }

        public PriorityQueue(int initialCapacity, IComparer<TElement>? comparer)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            if (initialCapacity == 0)
            {
                _elements = Array.Empty<TElement>();
            }
            else
            {
                _elements = new TElement[initialCapacity];
            }

            _priorityComparer = comparer ?? Comparer<TElement>.Default;
        }

        public PriorityQueue(IEnumerable<TElement> values) : this(values, null)
        {

        }

        public PriorityQueue(IEnumerable<TElement> values, IComparer<TElement>? comparer)
        {
            var elements = Array.Empty<TElement>();
            int count = 0;

            foreach (TElement element in values)
            {
                if (count == elements.Length)
                {
                    Resize(ref elements);
                }

                elements[count] = element;
                count++;
            }

            _elements = elements;
            _priorityComparer = comparer ?? Comparer<TElement>.Default;
            _count = count;

            Heapify();
        }
        #endregion

        public int Count => _count;
        public IComparer<TElement> Comparer => _priorityComparer;

        public void Enqueue(TElement element)
        {
            _version++;
            if (_count == _elements.Length)
            {
                Resize(ref _elements);
            }

            SiftUp(index: _count++, in element);
        }

        public TElement Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException();
            }

            return _elements[0];
        }

        public bool TryPeek(out TElement element)
        {
            if (_count == 0)
            {
                element = default!;
                return false;
            }

            element = _elements[0];
            return true;
        }

        public TElement Dequeue()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException();
            }

            _version++;
            RemoveIndex(index: 0, out TElement result);
            return result;
        }

        public bool TryDequeue(out TElement element)
        {
            if (_count == 0)
            {
                element = default!;
                return false;
            }

            _version++;
            RemoveIndex(index: 0, out element);
            return true;
        }

        public TElement EnqueueDequeue(TElement element)
        {
            if (_count == 0 || _priorityComparer.Compare(element, _elements[0]) <= 0)
            {
                return element;
            }

            _version++;
            TElement minElement = _elements[0];
            SiftDown(index: 0, in element);
            return minElement;
        }

        public void Clear()
        {
            _version++;
            if (_count > 0)
            {
                Array.Clear(_elements, 0, _count);
                _count = 0;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<TElement>, IEnumerator
        {
            private readonly PriorityQueue<TElement> _queue;
            private readonly int _version;
            private int _index;
            private TElement _current;

            internal Enumerator(PriorityQueue<TElement> queue)
            {
                _version = queue._version;
                _queue = queue;
                _index = 0;
                _current = default!;
            }

            public bool MoveNext()
            {
                PriorityQueue<TElement> queue = _queue;

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
            object IEnumerator.Current => _current!;

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

        #region Private Methods
        private void Heapify()
        {
            for (int i = (_count - 1) >> 2; i >= 0; i--)
            {
                TElement element = _elements[i];
                SiftDown(i, element);
            }
        }

        private void RemoveIndex(int index, out TElement element)
        {
            Debug.Assert(index < _count);

            element = _elements[index];

            int lastElementPos = --_count;
            ref TElement lastElement = ref _elements[lastElementPos];

            if (lastElementPos > 0)
            {
                SiftDown(index, in lastElement);
            }

            lastElement = default!;
        }

        private void SiftUp(int index, in TElement element)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 2;
                ref TElement parentElement = ref _elements[parentIndex];

                if (_priorityComparer.Compare(parentElement, element) <= 0)
                {
                    // parentPriority <= priority, heap property is satisfed
                    break;
                }

                _elements[index] = parentElement;
                index = parentIndex;
            }

            _elements[index] = element;
        }

        private void SiftDown(int index, in TElement element)
        {
            int minChildIndex;
            int count = _count;
            TElement[] elements = _elements;

            while ((minChildIndex = (index << 2) + 1) < count)
            {
                // find the child with the minimal priority
                ref TElement minChildElement = ref elements[minChildIndex];
                int childUpperBound = Math.Min(count, minChildIndex + 4);

                for (int nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
                {
                    ref TElement nextChildPriority = ref elements[nextChildIndex];
                    if (_priorityComparer.Compare(nextChildPriority, minChildElement) < 0)
                    {
                        minChildIndex = nextChildIndex;
                        minChildElement = ref nextChildPriority;
                    }
                }

                // compare with inserted priority
                if (_priorityComparer.Compare(element, minChildElement) <= 0)
                {
                    // priority <= childPriority, heap property is satisfied
                    break;
                }

                elements[index] = minChildElement;
                index = minChildIndex;
            }

            elements[index] = element;
        }

        private void Resize(ref TElement[] elements)
        {
            Debug.Assert(elements.Length == elements.Length);

            int newSize = elements.Length == 0 ? DefaultCapacity : 2 * elements.Length;

            Array.Resize(ref elements, newSize);
        }

#if DEBUG
        public void ValidateInternalState()
        {
            if (_elements.Length < _count)
            {
                throw new Exception("invalid elements array length");
            }

            foreach ((var element, var idx) in _elements.Select((x, i) => (x, i)).Skip(_count))
            {
                if (!IsDefault(element))
                {
                    throw new Exception($"Non-zero element '{element}' at index {idx}.");
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