using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
using System.Collections.ObjectModel;

namespace Curator;

public partial class MainPage : ContentPage
{
    private string _currentFolderName = "";
    public string CurrentLocation
    {
        get
        {
            if (_currentFolderId == null)
                return "Collections";

            return $"Collections > {_currentFolderName}";
        }
    }
    private int? _currentFolderId;
    public ObservableCollection<Collection> Library { get; }
    private readonly CuratorDatabase _curatorDatabase;

    // Initialize page dependencies and data binding
    public MainPage()
    {
        InitializeComponent();

        _curatorDatabase = new CuratorDatabase();
        Library = new ObservableCollection<Collection>();

        // Use MainPage as the binding source for XAML properties
        BindingContext = this;
    }

    // Load collections when page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCollectionsAsync();
    }

    // Load collections for the current location
    private async Task LoadCollectionsAsync()
    {
        List<Collection> savedCollections =
            await _curatorDatabase.GetCollectionsAsync(_currentFolderId);

        Library.Clear();

        foreach (Collection collection in savedCollections)
        {
            if (collection.IsFolder)
            {
                collection.ItemCount = await _curatorDatabase.GetCollectionCountAsync(collection.Id);
            }

            Library.Add(collection);
        }
    }

    // Handle adding collections and folders
    private async void OnAddCollectionClicked(object? sender, EventArgs e)
    {
        string result = await DisplayActionSheetAsync("New", "Cancel", null, "Collection", "Folder");

        switch (result)
        {
            case "Collection":
                string? collectionName = await DisplayPromptAsync(
                "New Collection",
                "Enter a name for the collection:");

                if (string.IsNullOrWhiteSpace(collectionName))
                {
                    return;
                }

                if (await _curatorDatabase.CollectionExistsAsync(collectionName.Trim(), _currentFolderId))
                {
                    await DisplayAlertAsync(
                        "Duplicate Name",
                        "A collection with this name already exists in the current location. Please choose a different name.",
                        "OK");
                    return;
                }

                Collection newCollection = new Collection
                {
                    Name = collectionName.Trim(),
                    ItemCount = 0,
                    IsFolder = false,
                    CollectionType = "Collection",
                    ParentCollectionId = _currentFolderId
                };

                await _curatorDatabase.SaveCollectionAsync(newCollection);

                await BackToRootAsync();

                break;
            case "Folder":

                string? folderName = await DisplayPromptAsync(
                "New Folder",
                "Enter a name for the folder:");

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    return;
                }

                if (await _curatorDatabase.CollectionExistsAsync(folderName.Trim(), null))
                {
                    await DisplayAlertAsync(
                        "Duplicate Name",
                        "A folder with this name already exists in the current location. Please choose a different name.",
                        "OK");
                    return;
                }

                Collection newFolder = new Collection
                {
                    Name = folderName.Trim(),
                    ItemCount = 0,
                    IsFolder = true,
                    CollectionType = "Folder",
                    ParentCollectionId = null // Folders are always at the root level for now. 
                };

                await _curatorDatabase.SaveCollectionAsync(newFolder);

                await BackToRootAsync();

                break;
            default:
                break;
        }
    }


    // Handle collection long-press options
    private async void OnCollectionLongPressed(object? sender, LongPressCompletedEventArgs e)
    {
        // Prevent long-press navigation from leaving the UI inside a folder being modified
        if (e.LongPressCommandParameter is Collection collectionClicked)
        {
            if (collectionClicked.IsFolder && collectionClicked.ParentCollectionId == null)
            {
                await BackToRootAsync();
            }
            else
            {
                await LoadCollectionsAsync();
            }
        }

        string result = await DisplayActionSheetAsync(
            "Collection Options",
            "Cancel",
            null,
            "Delete",
            "Rename",
            "Move");

        switch (result)
        {
            case "Delete":
                if (e.LongPressCommandParameter is Collection collectionToDelete)
                {
                    if (collectionToDelete.ItemCount > 0)
                    {
                        await DisplayAlertAsync(
                            "Cannot Delete",
                            "This collection is not empty. Please remove all items before deleting.",
                            "OK");
                        return;
                    }

                    var confirmDelete = await DisplayAlertAsync(
                        "Confirm Delete",
                        $"Are you sure you want to delete the collection '{collectionToDelete.Name}'?",
                        "Yes",
                        "No");
                    if (confirmDelete)
                    {
                        await _curatorDatabase.DeleteCollectionAsync(collectionToDelete);
                        await LoadCollectionsAsync();
                    }
                }
                break;
            case "Rename":
                if (e.LongPressCommandParameter is Collection collectionToRename)
                {
                    string? newName = await DisplayPromptAsync(
                        "Rename Collection",
                        "Enter a new name for the collection:",
                        initialValue: collectionToRename.Name);

                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        return;
                    }

                    if (await _curatorDatabase
                        .CollectionExistsAsync(
                            newName.Trim(),
                            collectionToRename.ParentCollectionId,
                            collectionToRename.Id))
                    {
                        await DisplayAlertAsync(
                            "Duplicate Name",
                            "A collection with this name already exists in the current folder. Please choose a different name.",
                            "OK");
                        return;
                    }

                    collectionToRename.Name = newName.Trim();
                    await _curatorDatabase.SaveCollectionAsync(collectionToRename);
                    await LoadCollectionsAsync();
                }
                break;
            case "Move":
                if (e.LongPressCommandParameter is Collection collectionToMove)
                {
                    if (collectionToMove.ParentCollectionId != null)
                    {
                        result = await DisplayActionSheetAsync(
                            "Move Collection",
                            "Cancel",
                            null,
                            "Move from Folder");
                        if (result == "Move from Folder")
                        {
                            if (await _curatorDatabase
                                .CollectionExistsAsync(
                                    collectionToMove.Name,
                                    null,
                                    collectionToMove.Id))
                            {
                                await DisplayAlertAsync(
                                    "Duplicate Name",
                                    "A collection with this name already exists in the root location. Please choose a different name or rename the collection before moving.",
                                    "OK");
                                return;
                            }

                            // Move to root
                            collectionToMove.ParentCollectionId = null;
                            await _curatorDatabase
                                .SaveCollectionAsync(collectionToMove);
                            await LoadCollectionsAsync();
                        }
                    }
                    else
                    {
                        var potentialParents = Library
                            .Where(c => c.IsFolder && c.Id != collectionToMove.Id)
                            .ToList();
                        if (collectionToMove.IsFolder)
                        {
                            await DisplayAlertAsync(
                                "Unable to Move",
                                "Curator does not yet support moving folders.",
                                "OK");
                            return;
                        }

                        if (potentialParents.Count == 0)
                        {
                            await DisplayAlertAsync(
                                "No Folders Available",
                                "There are no available folders to move this collection into.",
                                "OK");
                            return;
                        }

                        var folderNames = potentialParents
                            .Select(f => f.Name)
                            .ToArray();
                        string? selectedFolderName = await DisplayActionSheetAsync(
                            "Move Collection",
                            "Cancel",
                            null,
                            folderNames);

                        if (string.IsNullOrWhiteSpace(selectedFolderName) || selectedFolderName == "Cancel")
                        {
                            return;
                        }

                        var selectedFolder = potentialParents
                            .FirstOrDefault(f => f.Name == selectedFolderName);
                        if (selectedFolder == null)
                        {
                            return;
                        }

                        if (await _curatorDatabase
                            .CollectionExistsAsync(
                                collectionToMove.Name,
                                selectedFolder.Id,
                                collectionToMove.Id))
                        {
                            await DisplayAlertAsync(
                                "Duplicate Name",
                                "A collection with this name already exists in the selected folder. Please choose a different name or rename the collection before moving.",
                                "OK");
                            return;
                        }

                        collectionToMove.ParentCollectionId = selectedFolder.Id;
                        await _curatorDatabase.SaveCollectionAsync(collectionToMove);
                        await LoadCollectionsAsync();
                    }
                }
                break;
            default:
                break;
        }
    }

    // Open the selected folder
    private async void OpenFolderCollectionClicked(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Collection collectionToOpen &&
            collectionToOpen.IsFolder)
        {
            _currentFolderId = collectionToOpen.Id;
            _currentFolderName = collectionToOpen.Name;
            HeaderLabel.Text = CurrentLocation;
            await LoadCollectionsAsync();
        }
    }

    // Return to the root collection list
    private async Task BackToRootAsync()
    {
        _currentFolderId = null;
        _currentFolderName = "";
        HeaderLabel.Text = CurrentLocation;
        await LoadCollectionsAsync();
    }

    // Handle returning to the root collection list
    private async void BackToRootClicked(object? sender, EventArgs e)
    {
        await BackToRootAsync();
    }
}
