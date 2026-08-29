using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
//using Javax.Security.Auth;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
//using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace Curator;

public partial class CollectionPage : ContentPage
{
	private readonly Collection _collection;

	private readonly CuratorDatabase _curatorDatabase = new();

    public ObservableCollection<Item> Items { get; set; } = new();

	public CollectionPage(Collection collection)
	{
        InitializeComponent();
		BindingContext = this;
		_collection = collection;

        //Items.Add(new Item { Name = "Sample Item 1" });
    }

    // Load items when page appears
    protected override async void OnAppearing()
    {
        //_curatorDatabase = new CuratorDatabase();
        Items = new ObservableCollection<Item>();

        base.OnAppearing();
        await LoadItemsAsync();
    }

    // Load items for the current collection
    private async Task LoadItemsAsync()
    {
        List<Item> savedItems =
            await _curatorDatabase.GetItemsAsync(_collection.Id);

        Items.Clear();

        foreach (Item item in savedItems)
        {
            Items.Add(item);
        }
    }
}