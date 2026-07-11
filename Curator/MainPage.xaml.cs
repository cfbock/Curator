using Curator.Models;
using System.Collections.ObjectModel;

namespace Curator
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Collection> Collections { get; set; }

        public MainPage()
        {
            InitializeComponent();

            Collections = new ObservableCollection<Collection>
                {
                new Collection { Name = "Books", ItemCount = 10, IsFolder = false },
                new Collection { Name = "Games", ItemCount = 5, IsFolder = true },
                new Collection { Name = "Movies", ItemCount = 20, IsFolder = false }
            };

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

            Collections.Add(new Collection
            {
                Name = collectionName.Trim(),
                ItemCount = 0,
                IsFolder = false
            });
        }

    }


}
