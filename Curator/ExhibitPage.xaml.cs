using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
//using Javax.Security.Auth;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
//using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace Curator;

public partial class ExhibitPage : ContentPage, IQueryAttributable
{
    private Item? _item;
    private readonly CuratorDatabase _curatorDatabase;

    public ExhibitPage()
    {
        _curatorDatabase = new CuratorDatabase();
        InitializeComponent();
        BindingContext = this;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["Item"] is Item item)
        {
            _item = item;
            Title = _item.Name;
        }
    }

    public async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (_item is null)
            return;

        var confirmDelete = await DisplayAlertAsync(
                       "Confirm Delete",
                       $"Are you sure you want to delete this Item '{_item.Name}'?",
                       "Yes",
                       "No");
        if (confirmDelete)
        {
            await _curatorDatabase.DeleteItemAsync(_item);
            await Shell.Current.GoToAsync("..");
        }
    } 
    
    public async void OnEditItemClicked(object sender, EventArgs e)
    {
        if (_item is null)
            return;
        // Implement edit logic here
    }
}