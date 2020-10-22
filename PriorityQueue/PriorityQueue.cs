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

        #region Constructors
        public PriorityQueue() : this(0, Comparer<TPriority>.Default)
        {

        }

        public PriorityQueue(int initialCapacity) : this(initialCapacity, Comparer<TPriority>.Default)
        {

        }

        public PriorityQueue(IComparer<TPriority> comparer) : this(0, Comparer<TPriority>.Default)
        {

        }

        public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            if (comparer is null)
            {
                throw new ArgumentNullException(nameof(comparer));
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

            _priorityComparer = comparer;
        }

        public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> values) : this(values, Comparer<TPriority>.Default)
        {

        }

        public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> values, IComparer<TPriority> comparer) 
        {
            var priorities = new TPriority[DefaultCapacity];
            var elements = new TElement[DefaultCapacity];
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
            _priorityComparer = comparer;
            _count = count;

            Heapify();
        }
        #endregion

        public int Count => _count;
        public IComparer<TPriority> Comparer => _priorityComparer;

        public void Enqueue(TElement element, TPriority priority)
        {
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

            RemoveIndex(index: 0, out element, out priority);
            return true;
        }

        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_priorities, 0, _count);
                Array.Clear(_elements, 0, _count);
                _count = 0;
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

        public IEnumerator<(TElement Element, TPriority Priority)> GetEnumerator()
        {
            return GetEnumerableInner().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerableInner().GetEnumerator();
        }

        private IEnumerable<(TElement Element, TPriority Priority)> GetEnumerableInner()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return (_elements[i], _priorities[i]);
            }
        }

        public void ValidateInternalState()
        {
#if DEBUG
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
#endif
        }
        //#region Specific APIs
        //public TElement Dequeue() { throw null; }
        //public void Enqueue(TElement item, TPriority priority) { }
        //public TElement Peek() { throw null; }
        //public bool TryDequeue(out TElement item) { throw null; }
        //public bool TryPeek(out TElement item) { throw null; }
        //public void TrimExcess() { }
        //public bool Contains(TElement element) { }
        //public bool TryRemove(TElement element) { }
        //#endregion

        //#region IReadOnlyCollection
        //public int Count { get { throw null; } }
        //public IEnumerator GetEnumerator() { throw null; }

        //IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}
        //#endregion

        //#region Enumerator
        //public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>, IEnumerator, IDisposable
        //{
        //    public (TElement Element, TPriority Priority) Current { get { throw null; } }
        //    object System.Collections.IEnumerator.Current { get { throw null; } }
        //    public void Dispose() { }
        //    public bool MoveNext() { throw null; }
        //    void System.Collections.IEnumerator.Reset() { }
        //}
        //#endregion
        #endregion
    }
}