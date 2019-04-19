namespace Net7MultiClientUnlocker.Framework
{
    using System.Collections.ObjectModel;

    public static class ObservableCollectionExtensions
    {
        public static void MoveItemUp<T>(this ObservableCollection<T> baseCollection, int selectedIndex)
        {
            if (selectedIndex <= 0)
            {
                return;
            }

            baseCollection.Move(selectedIndex - 1, selectedIndex);
        }

        public static void MoveItemDown<T>(this ObservableCollection<T> baseCollection, int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex + 1 >= baseCollection.Count)
            {
                return;
            }

            baseCollection.Move(selectedIndex + 1, selectedIndex);
        }

        public static void MoveItemDown<T>(this ObservableCollection<T> baseCollection, T selectedItem)
        {
            baseCollection.MoveItemDown(baseCollection.IndexOf(selectedItem));
        }

        public static void MoveItemUp<T>(this ObservableCollection<T> baseCollection, T selectedItem)
        {
            baseCollection.MoveItemUp(baseCollection.IndexOf(selectedItem));
        }
    }
}
