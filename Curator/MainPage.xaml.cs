using Curator.Data;
using Curator.Models;
using System.Collections.ObjectModel;

namespace Curator
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Collection> Collections { get; set; }

        private readonly CuratorDatabase curatorDatabase;

        public MainPage()
        {
            InitializeComponent();

            curatorDatabase = new CuratorDatabase();

            Collections = new ObservableCollection<Collection>
            {
                new Collection { Name = "Books", ItemCount = 10, IsFolder = false },
                new Collection { Name = "Games", ItemCount = 5, IsFolder = true },
                new Collection { Name = "Movies", ItemCount = 20, IsFolder = false }
            };

            //Set the BindingContext to the MainPage instance so that the XAML can bind to the Collections property.
            BindingContext = this;
        }

        private async void OnAddCollectionClicked(object? sender, EventArgs e)
        {
            string collectionName = await DisplayPromptAsync(
                "New Collection",
                "Enter a name for the collection:");

            if (string.IsNullOrWhiteSpace(collectionName))
            {
                return;
            }

            Collection newCollection = new Collection
            {
                Name = collectionName.Trim(),
                ItemCount = 0,
                IsFolder = false
            };

            // Write the new object into the SQLite database file.
            await curatorDatabase.SaveCollectionAsync(newCollection);

            // Add the same object to the visible list on the screen.
            Collections.Add(newCollection);
        }

    }


}
