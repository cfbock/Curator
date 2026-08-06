using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
using System.Collections.ObjectModel;

namespace Curator
{
    public partial class MainPage : ContentPage
    {
        // An ObservableCollection property that will hold the list of collections to display in the UI.
        public ObservableCollection<Collection> Library { get; set; }
        // A private readonly field that provides access to the SQLite database.
        private readonly CuratorDatabase curatorDatabase;

        public MainPage()
        {
            // Call the InitializeComponent method to initialize the UI components defined in the XAML file.
            InitializeComponent();
            // Create a new instance of the CuratorDatabase class to manage database operations.
            curatorDatabase = new CuratorDatabase();
            //Initialize the ObservableCollection to an empty collection.
            Library = new ObservableCollection<Collection>();

            //Set the BindingContext to the MainPage instance so that the XAML can bind to the Library property.
            BindingContext = this;
        }
        // Override the OnAppearing method to load the collections from the database when the page appears.
        protected override async void OnAppearing()
        {
            // Call the base class's OnAppearing method to ensure that any base functionality is executed.
            base.OnAppearing();
            // Load the collections from the database and update the Library ObservableCollection.
            await LoadCollectionsAsync();
        }

        // A private method that loads the collections from the database and updates the Library ObservableCollection.
        private async Task LoadCollectionsAsync()
        {
            // Retrieve the list of collections from the SQLite database.
            List<Collection> savedCollections =
                await curatorDatabase.GetCollectionsAsync();
            // Clear the existing items in the Library ObservableCollection to avoid duplicates.
            Library.Clear();
            // Add each collection retrieved from the database to the Library ObservableCollection.
            foreach (Collection collection in savedCollections)
            {
                Library.Add(collection);
            }
        }

        // ---- An event handler for the "Add Collection" button click event. ----
        private async void OnAddCollectionClicked(object? sender, EventArgs e)
        {
            // Prompt the user to enter a name for the new collection.
            string? collectionName = await DisplayPromptAsync(
                "New Collection",
                "Enter a name for the collection:");
            // If the user cancels the prompt or enters an empty name, do not proceed with adding the collection.
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                return;
            }
            // Create a new Collection object with the provided name, an initial item count of 0, and IsFolder set to false.
            Collection newCollection = new Collection
            {
                Name = collectionName.Trim(),
                ItemCount = 0,
                IsFolder = false
            };

            // Write the new object into the SQLite database file.
            await curatorDatabase.SaveCollectionAsync(newCollection);

            // Add the same object to the visible list on the screen.
            Library.Add(newCollection);
        }

        // ---- An event handler for the long press event on a collection item in the UI. ----
        private async void OnCollectionLongPressed(object? sender, LongPressCompletedEventArgs e)
        {
            await DisplayAlertAsync(
            "Long press",
            "A collection card was long-pressed.",
            "OK");
        }

    }
}
