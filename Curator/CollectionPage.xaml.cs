using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
//using Javax.Security.Auth;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
//using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace Curator;

public partial class CollectionPage : ContentPage, IQueryAttributable
{
	private Collection? _collection;

    private readonly CuratorDatabase _curatorDatabase;

    public ObservableCollection<Item> Items { get; set; } = new();

	public CollectionPage()
	{
        _curatorDatabase = new CuratorDatabase();
        InitializeComponent();
		BindingContext = this;
    }

    // Implement IQueryAttributable to receive query parameters
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["Collection"] is Collection collection)
        {
            _collection = collection;
            Title = _collection.Name;
        }
    }

    // Handle the Add Item button click event
    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        if (_collection is null)
            return;

        string? name = await DisplayPromptAsync(
            "Add Item",
            "Item name:");

        if (string.IsNullOrWhiteSpace(name))
            return;

        Item item = new Item
        {
            Name = name.Trim(),
            CollectionId = _collection.Id
        };

        await _curatorDatabase.SaveItemAsync(item);

        await LoadItemsAsync();
    }

    // Load items when page appears
    protected override async void OnAppearing()
    {
        //Items = new ObservableCollection<Item>();

        base.OnAppearing();
        await LoadItemsAsync();
    }

    // Load items for the current collection
    private async Task LoadItemsAsync()
    {
        if (_collection is null)
            return;

        List<Item> savedItems =
            await _curatorDatabase.GetItemsAsync(_collection.Id);

        Items.Clear();

        foreach (Item item in savedItems)
        {
            Items.Add(item);
        }
    }

    // Open the selected Item
    private async void OpenItemClicked(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Item itemToOpen)
        {
            var navigationParameter = new Dictionary<string, object>
                {
                    { "Item", itemToOpen}
                };

            await Shell.Current.GoToAsync(nameof(ExhibitPage), navigationParameter);
        }
    }
}
                