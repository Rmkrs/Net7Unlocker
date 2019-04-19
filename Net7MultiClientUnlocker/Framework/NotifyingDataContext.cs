namespace Net7MultiClientUnlocker.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    using Net7MultiClientUnlocker.Domain;

    public class NotifyingDataContext : INotifyPropertyChanged
    {
        private const string CanBeDirtyKey = "_CanBeDirty_";

        private const string CheckDirtyChildrenKey = "_CheckDirtyChildren_";

        private const string CheckDirtyCollectionsKey = "_CheckDirtyCollections_";

        private const string DirtyCountKey = "_Count_";

        private const string IgnoreKey = "_Ignore_";

        private static readonly NotifyingDataContext GlobalContextInstance = new NotifyingDataContext();

        private readonly IList<string> missingKeys = new List<string>();

        private IDictionary<string, object> data;

        private IDictionary<string, object> originalData;

        private IDictionary<string, NotifyingDataContext> childContexts;

        private IDictionary<string, ObservableCollection<NotifyingDataContext>> childCollectionContexts;

        private IEnumerable<string> ignoreKeys;

        private bool isDirty;

        public NotifyingDataContext()
        {
            this.data = new Dictionary<string, object>();
            this.originalData = new Dictionary<string, object>();
            this.childContexts = new Dictionary<string, NotifyingDataContext>();
            this.childCollectionContexts = new Dictionary<string, ObservableCollection<NotifyingDataContext>>();
            this.ignoreKeys = new string[0];
            this.isDirty = false;
        }

        public NotifyingDataContext(IDictionary<DataPath, object> data)
        {
            this.SetData(data.Keys.ToDictionary(dataPath => dataPath.ToString(), dataPath => data[dataPath]));
        }

        public NotifyingDataContext(IDictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.SetData(data);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyingDataContext GlobalContext => GlobalContextInstance;

        public bool IsDirty
        {
            get => this.CheckDirtyRecursive();
            set => this.isDirty = value;
        }

        public NotifyingDataContext Parent { get; set; }

        public string ParentKey { get; set; }

        public string FullPath => this.Parent != null ? this.ConcatenateParentPath(this.Parent.FullPath, this.ParentKey) : String.Empty;

        public bool IsCollectionItem { get; set; }

        protected IEnumerable<string> DirtyKeys
        {
            get
            {
                var fields = new List<string>();

                if (this.data.Count != this.originalData.Count)
                {
                    fields.Add(DirtyCountKey);
                }

                var dataEnumerator = this.data.GetEnumerator();

                while (dataEnumerator.MoveNext())
                {
                    fields.AddRange(this.GetDirtyKeys(dataEnumerator.Current.Key));
                }

                dataEnumerator.Dispose();
                return fields;
            }
        }

        protected IEnumerable<string> MissingKeys => this.FindRoot().GetMissingKeys();

        public object this[DataPath dataPath]
        {
            get => this[dataPath.ToString()];
            set => this[dataPath.ToString()] = value;
        }

        public object this[string name]
        {
            get
            {
                if (Debugger.IsAttached && name == "*DEBUG*")
                {
                    Debugger.Break();
                }

                // Get to the corresponding context first.
                var checkResult = this.CheckKeyPath(name);
                var context = checkResult.Item1;
                name = checkResult.Item2;

                // If we find a context, use that one instead of the local one.
                if (context != null)
                {
                    return context[name];
                }

                // If we get to this point, means we are in the target data context, so we must use the local data items.

                // If the key points to a collection item, use that.
                var collectionInfo = this.GetCollectionInfo(name);
                if (collectionInfo.IsCollectionIndexer)
                {
                    // Verify and return indicated collection item.
                    return this.VerifySingleCollectionItem(collectionInfo);
                }

                if (this.data.ContainsKey(name))
                {
                    return this.data[name];
                }

                // If we dont already know about this missing key, add it.
                if (!this.missingKeys.Contains(name))
                {
                    this.missingKeys.Add(name);
                }

                return null;
            }

            set
            {
                // Get to the corresponding context first.
                var checkResult = this.CheckKeyPath(name);
                var context = checkResult.Item1;
                name = checkResult.Item2;

                // If we find a context, use that one instead of the local one.
                if (context != null)
                {
                    context[name] = value;
                    return;
                }

                // If we get to this point, means we are in the target data context, so we must use the local data items.

                // If the key points to a collection item, use that.
                var collectionInfo = this.GetCollectionInfo(name);
                if (collectionInfo.IsCollectionIndexer)
                {
                    // Verify and return indicated collection item.
                    var collectionItem = this.VerifySingleCollectionItem(collectionInfo);

                    if (collectionItem != null)
                    {
                        var collectionIndex = this.GetCollectionIndex(collectionInfo.CollectionKey, collectionItem);

                        // Dont need to update child contexts, since this is a data context inside a collection, meaning direct "value".
                        this.childCollectionContexts[collectionInfo.CollectionKey][collectionIndex] = (NotifyingDataContext)value;
                        this.OnPropertyChanged();
                    }
                }
                else
                {
                    this.data[name] = value;
                    this.UpdateChildContext(name, value);
                    this.OnPropertyChanged();
                }

                this.isDirty = this.CheckDirty();
            }
        }

        public static void UpdateGlobalContext(IEnumerable<KeyValuePair<string, object>> globalContextData)
        {
            if (globalContextData == null)
            {
                return;
            }

            var dataEnumerator = globalContextData.GetEnumerator();

            // Must lock the static global context instance to make it thread safe for setting values.
            lock (GlobalContextInstance)
            {
                while (dataEnumerator.MoveNext())
                {
                    var value = dataEnumerator.Current.Value;
                    if (value is IDictionary<string, object>)
                    {
                        value = new NotifyingDataContext((IDictionary<string, object>)value);
                    }

                    GlobalContextInstance[dataEnumerator.Current.Key] = value;
                }

                dataEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Finds the root data context by walking the tree upwards, visiting all parents, or this if there is no (more) parent.
        /// </summary>
        /// <returns>The root data context.</returns>
        public NotifyingDataContext FindRoot()
        {
            return this.Parent != null ? this.Parent.FindRoot() : this;
        }

        /// <summary>
        /// Checks if the name is contained in this instance.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Value indicating whether the name is contained in this instance.
        /// </returns>
        public bool ContainsKey(string name)
        {
            // Get to the corresponding context first.
            var checkResult = this.CheckKeyPath(name);
            var context = checkResult.Item1;
            name = checkResult.Item2;

            // If we find a context, use that one instead of the local one.
            return context?.ContainsKey(name) ?? this.data.ContainsKey(name);

            // If we get to this point, means we are in the target data context, so we must use the local data items.
        }

        public IDictionary<string, object> Export()
        {
            return this.data;
        }

        /// <summary>
        /// Clears the dirty indication for the specified key. If no key is specified, clears the dirty for all keys in the data context.
        /// </summary>
        /// <param name="key">The key.</param>
        public void ClearDirty(string key = null)
        {
            if (!String.IsNullOrWhiteSpace(key))
            {
                this.originalData[key] = this.data[key];
                this.isDirty = this.CheckDirty();
                return;
            }

            foreach (string dataKey in this.data.Keys)
            {
                this.originalData[dataKey] = this.data[dataKey];
            }

            this.isDirty = false;
        }

        /// <summary>
        /// Gets the dirty keys.
        /// </summary>
        /// <param name="prefixPath">The prefix path.</param>
        /// <returns>The dirty keys.</returns>
        protected IEnumerable<string> GetDirtyKeys(string prefixPath)
        {
            // Find the child context keys that are dirty.
            var childContext = this.childContexts.Where(c => c.Key == prefixPath).Select(s => s.Value).FirstOrDefault();
            if (childContext != null)
            {
                return childContext.DirtyKeys.Select(d => prefixPath + "." + d);
            }

            // Find the child collection contexts with their dirty keys.
            var childCollections = this.childCollectionContexts.Where(c => c.Key == prefixPath).Select(s => s.Value).FirstOrDefault();
            if (childCollections != null)
            {
                var keys = new List<string>();
                foreach (NotifyingDataContext notifyingDataContext in childCollections)
                {
                    keys.AddRange(notifyingDataContext.DirtyKeys.Select(d => prefixPath + "." + d));
                }

                return keys;
            }

            // Field is dirty, return key name.
            return !this.originalData.Contains(this.data.Single(d => d.Key == prefixPath)) ? new[] { prefixPath } : new string[0];
        }

        /// <summary>
        /// Gets the missing keys.
        /// </summary>
        /// <returns>The missing keys.</returns>
        protected IEnumerable<string> GetMissingKeys()
        {
            var keys = new List<string>();
            foreach (KeyValuePair<string, NotifyingDataContext> childContext in this.childContexts)
            {
                keys.AddRange(childContext.Value.GetMissingKeys());
            }

            foreach (KeyValuePair<string, ObservableCollection<NotifyingDataContext>> childCollectionContext in this.childCollectionContexts)
            {
                foreach (NotifyingDataContext notifyingDataContext in childCollectionContext.Value)
                {
                    keys.AddRange(notifyingDataContext.GetMissingKeys());
                }
            }

            keys.AddRange(this.missingKeys.Select(m => this.FullPath + "\\" + m));

            return keys;
        }

        protected virtual void OnPropertyChanged()
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
        }

        private void SetData(IDictionary<string, object> dataToSet)
        {
            this.data = dataToSet;

            // Check if ignore keys has been set to something we can use.
            if (this.data.ContainsKey(IgnoreKey) && this.data[IgnoreKey] != null)
            {
                this.ignoreKeys = this.data[IgnoreKey] as IEnumerable<string>;
            }

            // If not we will initialize it "empty".
            if (this.ignoreKeys == null)
            {
                this.ignoreKeys = new string[0];
            }

            this.childContexts = new Dictionary<string, NotifyingDataContext>();
            this.childCollectionContexts = new Dictionary<string, ObservableCollection<NotifyingDataContext>>();

            this.InjectNotifyingContainers(this.data);

            this.originalData = new Dictionary<string, object>();

            var dataEnumerator = this.data.GetEnumerator();

            while (dataEnumerator.MoveNext())
            {
                this.originalData.Add(dataEnumerator.Current);
                this.UpdateChildContext(dataEnumerator.Current.Key, dataEnumerator.Current.Value);
            }

            dataEnumerator.Dispose();
            this.isDirty = false;
        }

        private bool CheckDirty()
        {
            // First check the child collections for equality, then the child contexts finally the data keys.
            return this.CheckNotEqualChildCollections() || (this.CheckNotEqualChildContexts() || this.CheckNotEqualsData());
        }

        private bool CheckNotEqualsData()
        {
            // If we should not check dirty for the data keys, we are not dirty.
            if (!this.AsBoolean(CanBeDirtyKey))
            {
                return false;
            }

            // Filter out the non applicable keys, which means all ignored keys, all child context keys and all child collection keys.
            bool ApplicablePredicate(string key) => !this.ignoreKeys.Contains(key) && !this.childCollectionContexts.Keys.Contains(key) && !this.childContexts.Keys.Contains(key);
            var applicableKeys = this.data.Keys.Where(ApplicablePredicate).ToList();
            var applicableOriginalKeys = this.originalData.Keys.Where(ApplicablePredicate).ToList();

            // Different count means not equal.
            if (applicableKeys.Count != applicableOriginalKeys.Count)
            {
                return true;
            }

            // If the applicable key is not found in the applicable original keys, we are not equal.
            // Finally use object equals to check if the value in data and original dat is the same.
            // ReSharper disable once StyleCop.SA1126
            return applicableKeys.Any(a => !applicableOriginalKeys.Contains(a)) || applicableKeys.Any(key => !Equals(this.originalData[key], this.data[key]));
        }

        private bool CheckNotEqualChildContexts()
        {
            // Only check the dirty children if needed.
            if (!this.AsBoolean(CheckDirtyChildrenKey))
            {
                return false;
            }

            var applicableChildContexts = this.childContexts.Where(c => !this.ignoreKeys.Contains(c.Key)).ToList();

            // Check if any of the keys in the applicable child contexts is not contained in the original data. If so, we are not equal.
            if (applicableChildContexts.Any(applicableChild => !this.originalData.ContainsKey(applicableChild.Key)))
            {
                return true;
            }

            // Check if the the data does not equal the originalData for the applicable child contexts we need to check.
            // ReSharper disable once StyleCop.SA1126
            return applicableChildContexts.Any(applicableChild => !Equals(this.data[applicableChild.Key], this.originalData[applicableChild.Key]));
        }

        private bool CheckNotEqualChildCollections()
        {
            // Only check the dirty collections if needed.
            if (!this.AsBoolean(CheckDirtyCollectionsKey))
            {
                return false;
            }

            // First find all collections that are not on the ignore list.
            var collections = this.childCollectionContexts.Where(c => !this.ignoreKeys.Contains(c.Key)).Select(s => s.Key).ToList();

            // If we have no collections we are equal.
            if (!collections.Any())
            {
                return false;
            }

            // If the collections contains a key that is not present in the original data, we are not equal.
            if (collections.Any(key => !this.originalData.ContainsKey(key)))
            {
                return true;
            }

            // Check all the non-ignored child collections to see if any of those have changed, optimized => first on count, second on object equals per item.
            if (collections.Any(
                childCollection =>
                ((ObservableCollection<NotifyingDataContext>)this.data[childCollection]).Count
                != ((ObservableCollection<NotifyingDataContext>)this.originalData[childCollection]).Count))
            {
                return true;
            }

            // Now check all the non-ignored child collections and loop through all values inside them.
            // ReSharper disable once StyleCop.SA1126
            return (from childCollection in collections
                    let dataCollection = (ObservableCollection<NotifyingDataContext>)this.data[childCollection]
                    let originalDataCollection = (ObservableCollection<NotifyingDataContext>)this.originalData[childCollection]
                    where dataCollection.Where((t, i) => !Equals(t, originalDataCollection[i])).Any()
                    select dataCollection).Any();
        }

        private bool CheckDirtyRecursive()
        {
            return (this.AsBoolean(CanBeDirtyKey) && this.isDirty)
                || (this.AsBoolean(CheckDirtyChildrenKey) && this.CheckDirtyChildContexts())
                || (this.AsBoolean(CheckDirtyCollectionsKey) && this.CheckDirtyCollections());
        }

        private bool CheckDirtyChildContexts()
        {
            return this.childContexts.Where(c => !this.ignoreKeys.Contains(c.Key)).Select(s => s.Value).Any(v => v.IsDirty);
        }

        private bool CheckDirtyCollections()
        {
            return this.childCollectionContexts.Where(c => !this.ignoreKeys.Contains(c.Key)).Select(select => select.Value).Any(values => values.Any(value => value.IsDirty));
        }

        private bool AsBoolean(string key)
        {
            if (!this.data.ContainsKey(key))
            {
                return true;
            }

            // ReSharper disable once InlineOutVariableDeclaration
            bool result;
            return !bool.TryParse(this[key].ToString(), out result) || result;
        }

        private void UpdateChildContext(string key, object value)
        {
            if (this.ignoreKeys.Contains(key))
            {
                return;
            }

            if (this.childContexts.ContainsKey(key))
            {
                this.childContexts[key].Parent = null;
                this.childContexts[key].ParentKey = null;
                this.childContexts.Remove(key);
            }

            var context = this.GetDataContext(value);
            if (context != null)
            {
                this.childContexts[key] = context;
                context.Parent = this;
                context.ParentKey = key;
                return;
            }

            this.UpdateChildCollectionContexts(key, value);
        }

        private void UpdateChildCollectionContexts(string key, object value)
        {
            if (this.childCollectionContexts.ContainsKey(key))
            {
                // Remove current collection
                var currentCollection = this.childCollectionContexts[key];
                foreach (var context in currentCollection)
                {
                    context.Parent = null;
                    context.ParentKey = null;
                    context.IsCollectionItem = false;
                }

                this.childCollectionContexts.Remove(key);
            }

            // Add new collecion
            var collection = value as ObservableCollection<NotifyingDataContext>;
            if (collection == null)
            {
                return;
            }

            foreach (var context in collection)
            {
                context.Parent = this;
                context.ParentKey = key;
                context.IsCollectionItem = true;
            }

            this.childCollectionContexts[key] = collection;
        }

        private NotifyingDataContext GetDataContext(object value)
        {
            var element = value as FrameworkElement;
            if (element != null)
            {
                return this.GetDataContext(element.DataContext);
            }

            return value as NotifyingDataContext;
        }

        private void InjectNotifyingContainers(IDictionary<string, object> rawData)
        {
            foreach (string key in rawData.Keys.ToList())
            {
                var handled = this.ReplaceDictionary(rawData, key);
                if (handled)
                {
                    continue;
                }

                this.ReplaceCollection(rawData, key);
            }
        }

        private bool ReplaceDictionary(IDictionary<string, object> rawData, string key)
        {
            var value = rawData[key] as IDictionary<string, object>;
            if (value == null)
            {
                return false;
            }

            rawData[key] = new NotifyingDataContext(value);
            return true;
        }

        private void ReplaceCollection(IDictionary<string, object> rawData, string key)
        {
            // Let's only support ICollection<IDictionary<string, object>> and ICollection<Dictionary<string, object>> for now.
            var rawDataValue = rawData[key] as ICollection<IDictionary<string, object>>;
            if (rawDataValue == null)
            {
                var dictionaryData = rawData[key] as ICollection<Dictionary<string, object>>;
                if (dictionaryData == null)
                {
                    return;
                }

                rawDataValue = new Collection<IDictionary<string, object>>();
                foreach (Dictionary<string, object> dictionary in dictionaryData)
                {
                    rawDataValue.Add(dictionary);
                }
            }

            rawData[key] = this.CreateObservableCollection(rawDataValue);
        }

        private ObservableCollection<NotifyingDataContext> CreateObservableCollection(IEnumerable<IDictionary<string, object>> collection)
        {
            // No need to bind to CollectionChanged event for updating childCollectionContexts, because the collection IS the value.
            return new ObservableCollection<NotifyingDataContext>(collection.Select(d => new NotifyingDataContext(d)));
        }

        private string ConcatenateParentPath(string fullPath, string parentKey)
        {
            var collectionSuffix = String.Empty;

            if (this.IsCollectionItem)
            {
                collectionSuffix = String.Concat("{", this.Parent.GetCollectionIndex(parentKey, this), "}");
            }

            return String.Concat(fullPath, String.IsNullOrWhiteSpace(fullPath) ? String.Empty : "\\", parentKey, collectionSuffix);
        }

        private int GetCollectionIndex(string collectionKey, NotifyingDataContext collectionItem)
        {
            return this.childCollectionContexts[collectionKey].IndexOf(collectionItem);
        }

        private Tuple<NotifyingDataContext, string> CheckKeyPath(string name)
        {
            if (!name.Contains('\\'))
            {
                return new Tuple<NotifyingDataContext, string>(null, name);
            }

            var keys = name.Split('\\');
            return this.ParseKeyPath(keys, keys.GetUpperBound(0), 0);
        }

        private Tuple<NotifyingDataContext, string> ParseKeyPath(string[] keys, int maxLevel, int level)
        {
            // If we are at max level we need to return this and the last key bit as name.
            if (level == maxLevel)
            {
                return new Tuple<NotifyingDataContext, string>(this, keys[maxLevel]);
            }

            var key = keys[level];

            // If the very first key in the keys collection is empty, means we need to go to the root context.
            if (String.IsNullOrWhiteSpace(key) && level == 0)
            {
                return this.FindRoot().ParseKeyPath(keys, maxLevel, ++level);
            }

            // If they key equals .. we need to get to our parent.
            if (key == "..")
            {
                if (this.Parent == null)
                {
                    throw new InvalidOperationException($"Cannot find parent context specified in binding '{String.Join("\\", keys)}' for level '{level}'.");
                }

                return this.Parent.ParseKeyPath(keys, maxLevel, ++level);
            }

            // If the key contains {} we need to parse the indexer out and use it to find the correct data context.
            var collectionInfo = this.GetCollectionInfo(key);
            if (!collectionInfo.IsCollectionIndexer)
            {
                // No root, no parent, no collection item, means it has to be a child context.
                // ReSharper disable once AssignNullToNotNullAttribute
                return this.childContexts[key].ParseKeyPath(keys, maxLevel, ++level);
            }

            var collectionItem = this.VerifySingleCollectionItem(collectionInfo);
            return collectionItem.ParseKeyPath(keys, maxLevel, ++level);

        }

        private CollectionIndexerInfo GetCollectionInfo(string key)
        {
            return new CollectionIndexerInfo(key);
        }

        private NotifyingDataContext VerifySingleCollectionItem(CollectionIndexerInfo collectionInfo)
        {
            if (!this.childCollectionContexts.ContainsKey(collectionInfo.CollectionKey))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "ChildCollectionContext with key [{0}] does not exist.", collectionInfo.CollectionKey));
            }

            if (collectionInfo.HasIndex)
            {
                return this.childCollectionContexts[collectionInfo.CollectionKey][collectionInfo.ItemIndex];
            }

            if (collectionInfo.HasKeyValuePair)
            {
                var matchingItems = this.childCollectionContexts[collectionInfo.CollectionKey].Where(ndc => this.IsMatchingCollectionItem(collectionInfo, ndc)).DefaultIfEmpty().ToList();
                int numberOfMatchingItems = matchingItems.Count;
                if (numberOfMatchingItems > 1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "ChildCollectionContext with key [{0}] contains several items matching [{1}={2}]", collectionInfo.CollectionKey, collectionInfo.ItemKey, collectionInfo.ItemValue));
                }

                return matchingItems[0];
            }

            return null;
        }

        private bool IsMatchingCollectionItem(CollectionIndexerInfo collectionInfo, NotifyingDataContext ndc)
        {
            var hasItemValue = !String.IsNullOrWhiteSpace(collectionInfo.ItemValue);
            var itemValue = ndc[collectionInfo.ItemKey];

            if (itemValue == null)
            {
                return !hasItemValue;
            }

            var stringValue = itemValue as string;
            if (stringValue != null && String.IsNullOrWhiteSpace(stringValue))
            {
                return !hasItemValue;
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}", ndc[collectionInfo.ItemKey]).Equals(collectionInfo.ItemValue);
        }
    }
}
