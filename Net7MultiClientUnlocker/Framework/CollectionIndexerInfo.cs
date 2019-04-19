namespace Net7MultiClientUnlocker.Framework
{
    using System;

    public struct CollectionIndexerInfo
    {
        internal const int NotAnIndex = -1;

        private int itemIndex;

        public CollectionIndexerInfo(string key)
            : this()
        {
            this.itemIndex = NotAnIndex;
            this.ParseKey(key);
        }

        public string CollectionKey { get; private set; }

        public string Indexer { get; private set; }

        public int ItemIndex => this.itemIndex;

        public string ItemKey { get; private set; }

        public string ItemValue { get; private set; }

        public bool IsCollectionIndexer => !String.IsNullOrWhiteSpace(this.CollectionKey);

        public bool HasIndex => this.ItemIndex != NotAnIndex;

        public bool HasKeyValuePair => !String.IsNullOrWhiteSpace(this.ItemKey);

        private void ParseKey(string key)
        {
            if (key == null)
            {
                return;
            }

            var indexerPatternStart = key.IndexOf('{');

            // Check if the key has a { in it (after first character).
            if (indexerPatternStart <= 0)
            {
                return;
            }

            var indexerPattern = key.Substring(indexerPatternStart);

            // Confirm that the indexerPattern also ends with '}'.
            if (!indexerPattern.EndsWith("}"))
            {
                return;
            }

            // Pattern confirmed. Set the collection key and indexer.
            this.CollectionKey = key.Substring(0, indexerPatternStart);
            this.Indexer = indexerPattern.Substring(1, indexerPattern.Length - 2);

            // See if the pattern contains an index.
            if (Int32.TryParse(this.Indexer, out this.itemIndex))
            {
                return;
            }

            // Reset itemIndex to -1, to indicate that the pattern does not contain an index.
            this.itemIndex = -1;

            // See if the pattern contains a key/value pair separator (after first character).
            var keyValuePairSeparatorIndex = this.Indexer.IndexOf('=');
            if (keyValuePairSeparatorIndex <= 0)
            {
                return;
            }

            this.ItemKey = this.Indexer.Substring(0, keyValuePairSeparatorIndex);
            this.ItemValue = this.Indexer.Substring(keyValuePairSeparatorIndex + 1);
        }
    }
}
