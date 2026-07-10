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

    }

}
