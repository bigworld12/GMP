using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows.Threading;

namespace System.Collections.ObjectModel
{
    public class ObservableCollectionEx<T>
        : IList<T>, IList
        , INotifyCollectionChanged
        , INotifyPropertyChanged
    {
        #region Fields

        private readonly IList<T> collection = new List<T>();
        private readonly Dispatcher dispatcher;
        private readonly ReaderWriterLock sync = new ReaderWriterLock();
        private readonly List<T> _InternalList = new List<T>();

        [NonSerialized]
        private object _syncRoot;

        #endregion

        #region Constructors

        public ObservableCollectionEx()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollectionEx(IEnumerable<T> createFrom)
            : this()
        {
            Source = createFrom;
        }

        #endregion

        #region Properties

        #region Filter

        private Func<T , bool> _Filter;

        public Func<T , bool> Filter
        {
            get { return _Filter; }
            set
            {
                //ignore if values are equal
                if (value == _Filter) return;

                _Filter = value;

                ApplyFilter();

                RaisePropertyChanged(() => Filter);
            }
        }

        #endregion

        #region Source

        private IEnumerable<T> _Source;

        public IEnumerable<T> Source
        {
            get
            {
                return _Source;
            }
            set
            {
                //ignore if values are equal
                if (value == _Source) return;

                if (_Source is INotifyCollectionChanged)
                    (_Source as INotifyCollectionChanged).CollectionChanged -= Source_CollectionChanged;

                _Source = value;

                InitFrom(_Source);

                if (_Source is INotifyCollectionChanged)
                    (_Source as INotifyCollectionChanged).CollectionChanged += Source_CollectionChanged;

                RaisePropertyChanged(() => Source);
            }
        }

        #endregion

        #endregion

        #region Methods

        public void Add(T item)
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                DoAdd(item);
            else
                dispatcher.BeginInvoke((Action)(() => DoAdd(item)));
        }

        private int DoAdd(T item)
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            var index = DoAddInternal(item , true);

            sync.ReleaseWriterLock();

            return index;
        }

        private int DoAddInternal(T item , bool attachMonitorChanges)
        {
            // Attach to PropertyChanged event for monitoring future properties' changes
            if (attachMonitorChanges)
                AttachPropertyChanged(item);

            // Check if it should be here
            if (!ShouldBeHere(item))
                return -1;

            // Add item to collection
            var index = collection.Count;
            collection.Add(item);

            // Notify about collection's changes
            RaisePropertyChanged(() => Count);
            RaisePropertyChanged("Item[]");
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add , item , index));

            return index;
        }

        public int Add(object item)
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                return DoAdd((T)item);
            else
            {
                var op = dispatcher.BeginInvoke(new Func<T , int>(DoAdd) , item);
                if (op == null || op.Result == null)
                    return -1;
                return (int)op.Result;
            }
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public void Clear()
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                DoClear();
            else
                dispatcher.BeginInvoke((Action)(DoClear));
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index , object value)
        {
            Insert(index , (T)value);
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        private void DoClear()
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            DoClearInternal();

            sync.ReleaseWriterLock();
        }

        private void DoClearInternal()
        {
            // Detach from PropertyChanged events
            foreach (var item in _InternalList.ToArray())
                DetachPropertyChanged(item);

            // Clear collection
            collection.Clear();

            // Notify about collection's changes
            RaisePropertyChanged(() => Count);
            RaisePropertyChanged("Item[]");
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            sync.AcquireReaderLock(Timeout.Infinite);
            var result = ContainsInternal(item);
            sync.ReleaseReaderLock();
            return result;
        }

        private bool ContainsInternal(T item)
        {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array , int arrayIndex)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            collection.CopyTo(array , arrayIndex);
            sync.ReleaseWriterLock();
        }

        public void CopyTo(Array array , int index)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            for (var i = 0; i < collection.Count; i++)
            {
                array.SetValue(collection[i] , index + i);
            }
            sync.ReleaseWriterLock();
        }

        public int Count
        {
            get
            {
                sync.AcquireReaderLock(Timeout.Infinite);
                var result = collection.Count;
                sync.ReleaseReaderLock();
                return result;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    var c = collection as ICollection;
                    if (c != null)
                        _syncRoot = c.SyncRoot;
                    else
                        Interlocked.CompareExchange<object>(ref _syncRoot , new object() , null);
                }
                return _syncRoot;
            }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                return DoRemove(item);
            else
            {
                var op = dispatcher.BeginInvoke(new Func<T , bool>(DoRemove) , item);
                if (op == null || op.Result == null)
                    return false;
                return (bool)op.Result;
            }
        }

        private bool DoRemove(T item)
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            var result = DoRemoveInternal(item , true);

            sync.ReleaseWriterLock();

            return result;
        }

        private bool DoRemoveInternal(T item , bool detachMonitorChanges)
        {
            // Check if item is still at collection
            var index = collection.IndexOf(item);
            if (index == -1)
                return false;

            // Detach from PropertyChanged event
            if (detachMonitorChanges)
                DetachPropertyChanged(item);

            // Remove item from collection
            var result = collection.Remove(item);

            // Notify about collection's changes
            if (result)
            {
                RaisePropertyChanged(() => Count);
                RaisePropertyChanged("Item[]");
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove , item ,
                                                                            index));
            }

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            sync.AcquireReaderLock(Timeout.Infinite);
            var result = collection.IndexOf(item);
            sync.ReleaseReaderLock();
            return result;
        }

        public void Insert(int index , T item)
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                DoInsert(index , item);
            else
                dispatcher.BeginInvoke((Action)(() => DoInsert(index , item)));
        }

        private void DoInsert(int index , T item)
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            // Attach to PropertyChanged event for monitoring future properties' changes
            AttachPropertyChanged(item);

            // Check if it should be here
            if (!ShouldBeHere(item))
                return;

            // Insert item in collection
            collection.Insert(index , item);

            // Notify about collection's changes
            RaisePropertyChanged(() => Count);
            RaisePropertyChanged("Item[]");
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add , item , index));

            sync.ReleaseWriterLock();
        }

        public void RemoveAt(int index)
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                DoRemoveAt(index);
            else
                dispatcher.BeginInvoke((Action)(() => DoRemoveAt(index)));
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        private void DoRemoveAt(int index)
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            if (collection.Count == 0 || collection.Count <= index)
            {
                sync.ReleaseWriterLock();
                return;
            }

            var item = collection[index];

            // Detach from PropertyChanged event
            DetachPropertyChanged(item);

            // Remove item from collection
            collection.RemoveAt(index);

            // Notify about collection's changes
            RaisePropertyChanged(() => Count);
            RaisePropertyChanged("Item[]");
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove , item , index));

            sync.ReleaseWriterLock();

        }

        public T this[int index]
        {
            get
            {
                sync.AcquireReaderLock(Timeout.Infinite);
                if (index >= Count || index <= 0) return default(T);

                var result = collection[index];
                sync.ReleaseReaderLock();
                return result;
            }
            set
            {
                sync.AcquireWriterLock(Timeout.Infinite);

                if (collection.Count == 0 || collection.Count <= index)
                {
                    sync.ReleaseWriterLock();
                    return;
                }

                var oldItem = collection[index];

                DetachPropertyChanged(oldItem);
                AttachPropertyChanged(value);

                if (ShouldBeHere(value))
                {
                    collection[index] = value;

                    // Notify about collection's changes
                    RaisePropertyChanged("Item[]");
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace , value , oldItem , index));
                }
                else
                {
                    //
                    // Remove current item from collection
                    //

                    // Detach from PropertyChanged event
                    DetachPropertyChanged(oldItem);

                    // Remove item from collection
                    collection.RemoveAt(index);

                    // Notify about collection's changes
                    RaisePropertyChanged(() => Count);
                    RaisePropertyChanged("Item[]");
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove , oldItem , index));
                }

                sync.ReleaseWriterLock();
            }
        }

        #region Internal methods

        private void AttachPropertyChanged(T item)
        {
            _InternalList.Add(item);
            if (item is INotifyPropertyChanged)
            {
                (item as INotifyPropertyChanged).PropertyChanged += Item_PropertyChanged;
            }
        }

        private void DetachPropertyChanged(T item)
        {
            _InternalList.Remove(item);
            if (item is INotifyPropertyChanged)
            {
                (item as INotifyPropertyChanged).PropertyChanged -= Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender , PropertyChangedEventArgs e)
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            var item = (T)sender;

            var containsAfter = ApplyFilter(item);

            if (containsAfter)
                ItemPropertyChanged?.Invoke(sender , e);

            sync.ReleaseWriterLock();
        }

        private bool ApplyFilter(T item)
        {
            var contains = ContainsInternal(item);
            var containsAfter = contains;

            if (ShouldBeHere(item))
            {
                if (!contains)
                {
                    DoAddInternal(item , false);
                    containsAfter = true;
                }
            }
            else
            {
                if (contains)
                {
                    DoRemoveInternal(item , false);
                    containsAfter = false;
                }
            }
            return containsAfter;
        }

        private bool ShouldBeHere(T item)
        {
            return (Filter == null) || Filter(item);
        }

        private void RaisePropertyChanged<TSource>(Expression<Func<TSource>> propertyExpression)
        {
            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            RaisePropertyChanged(propertyName);
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propertyName));
        }

        private void InitFrom(IEnumerable<T> source)
        {
            if (source == null)
            {
                Clear();
                return;
            }

            sync.AcquireWriterLock(Timeout.Infinite);

            foreach (var item in source)
            {
                DoAddInternal(item , true);
            }

            sync.ReleaseWriterLock();
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {            
            CollectionChanged?.Invoke(this , e);
        }

        public void ApplyFilter()
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            foreach (var item in _InternalList)
            {
                ApplyFilter(item);
            }

            sync.ReleaseWriterLock();
        }

        #endregion

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler ItemPropertyChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Event handlers

        void Source_CollectionChanged(object sender , NotifyCollectionChangedEventArgs e)
        {
            sync.AcquireWriterLock(Timeout.Infinite);

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.OfType<T>())
                {
                    DoRemoveInternal(oldItem , true);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.OfType<T>())
                {
                    DoAddInternal(newItem , true);
                }
            }

            sync.ReleaseWriterLock();
        }


        #endregion
    }
}
