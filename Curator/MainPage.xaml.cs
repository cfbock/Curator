using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
using System.Collections.ObjectModel;

namespace Curator
{
    public partial class MainPage : ContentPage
    {
        // A private field to keep track of the current folder name for display purposes.
        private string currentFolderName = "";
        public string CurrentLocation
        {
            get
            {
                if (currentFolderId == null)
                    return "Collections";

                return $"Collections > {currentFolderName}";
            }
        }
        // A private field to keep track of the current folder ID for navigation purposes.
        private int? currentFolderId = null;
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
                await curatorDatabase.GetCollectionsAsync(currentFolderId);
            // Clear the existing items in the Library ObservableCollection to avoid duplicates.
            Library.Clear();
            // Add each collection retrieved from the database to the Library ObservableCollection.
            foreach (Collection collection in savedCollections)
            {
                if (collection.IsFolder)
                {
                    // If the collection is a folder, get the count of items in that folder.
                    collection.ItemCount = await curatorDatabase.GetCollectionCountAsync(collection.Id);
                }

                Library.Add(collection);
            }
        }

        // ---- An event handler for the "Add Collection" button click event. ----
        private async void OnAddCollectionClicked(object? sender, EventArgs e)
        {
            string result = await DisplayActionSheetAsync("New", "Cancel", null, "Collection", "Folder");

            switch (result)
            {
                case "Collection":
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
                        IsFolder = false,
                        CollectionType = "Collection",
                        ParentCollectionId = null // Set ParentCollectionId to null for root collections
                    };
                    if (newCollection != null)
                    {
                        // Write the new object into the SQLite database file.
                        await curatorDatabase.SaveCollectionAsync(newCollection);

                        await BackToRootAsync();
                        //currentFolderId = null; // Reset to root after adding a new collection
                        //currentFolderName = ""; // Reset folder name to root
                        //HeaderLabel.Text = CurrentLocation; // Update the header label to reflect the current location

                        //await LoadCollectionsAsync(); // Reload the collections to reflect the changes
                        // Add the same object to the visible list on the screen.
                        //Library.Add(newCollection);
                    }
                    break;
                case "Folder":
                    // Prompt the user to enter a name for the new folder.
                    string? folderName = await DisplayPromptAsync(
                    "New Folder",
                    "Enter a name for the folder:");
                    // If the user cancels the prompt or enters an empty name, do not proceed with adding the folder.
                    if (string.IsNullOrWhiteSpace(folderName))
                    {
                        return;
                    }
                    // Create a new Collection object with the provided name, an initial item count of 0, and IsFolder set to true.
                    Collection newFolder = new Collection
                    {
                        Name = folderName.Trim(),
                        ItemCount = 0,
                        IsFolder = true,
                        CollectionType = "Folder",
                        ParentCollectionId = null // Set ParentCollectionId to null for root folders
                    };
                    if (newFolder != null)
                    {
                        // Write the new folder into the SQLite database file.
                        await curatorDatabase.SaveCollectionAsync(newFolder);

                        await BackToRootAsync();
                        // Add the same folder to the visible list on the screen.
                        //Library.Add(newFolder);
                    }
                    break;
                default:
                    // Cancel or unrecognized option
                    break;
            }
        }


        // ---- An event handler for the long press event on a collection item in the UI. ----
        private async void OnCollectionLongPressed(object? sender, LongPressCompletedEventArgs e)
        {
            //await DisplayAlertAsync(
            //"Long press",
            //"A collection card was long-pressed.",
            //"OK");

            string result = await DisplayActionSheetAsync("Collection Options", "Cancel", null, "Delete", "Rename", "Move");

            //await DisplayAlertAsync(
            //    "Debug",
            //    e.LongPressCommandParameter?.GetType().FullName ?? "Parameter is null",
            //    "OK"
            //    );

            switch (result)
            {
                case "Delete":
                    if (e.LongPressCommandParameter is Collection collectionToDelete)
                    {
                        var confirmDelete = await DisplayAlertAsync(
                            "Confirm Delete",
                            $"Are you sure you want to delete the collection '{collectionToDelete.Name}'?",
                            "Yes",
                            "No");
                        if (confirmDelete)
                        {
                            await curatorDatabase.DeleteCollectionAsync(collectionToDelete);
                            //Library.Remove(collectionToDelete);
                            if (collectionToDelete.IsFolder)
                            {
                                await BackToRootAsync(); // Reload the collections to reflect the changes
                            }
                            else
                            {
                                await LoadCollectionsAsync(); // Reload the collections to reflect the changes
                            }
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

                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            collectionToRename.Name = newName.Trim();
                            await curatorDatabase.SaveCollectionAsync(collectionToRename);

                            if (collectionToRename.IsFolder)
                            {
                                await BackToRootAsync(); // Reload the collections to reflect the changes
                            }
                            else
                            {
                                await LoadCollectionsAsync(); // Reload the collections to reflect the changes
                            }
                            // Refresh the UI by removing and re-adding the renamed collection
                            //Library.Remove(collectionToRename);
                            //Library.Add(collectionToRename);
                        }
                    }
                    break;
                case "Move":
                    if (e.LongPressCommandParameter is Collection collectionToMove)
                    {
                        if (collectionToMove.ParentCollectionId != null)
                        {
                            result = await DisplayActionSheetAsync("Move Collection", "Cancel", null, "Move from Folder");
                            if (result == "Move from Folder")
                            {
                                var currentParent = Library.FirstOrDefault(c => c.Id == collectionToMove.ParentCollectionId);
                                collectionToMove.ParentCollectionId = null; // Move to root
                                await curatorDatabase.SaveCollectionAsync(collectionToMove);
                                if (currentParent != null)
                                {
                                    // Update the item count of the current parent folder
                                    currentParent.ItemCount = await curatorDatabase.GetCollectionCountAsync(currentParent.Id);

                                }
                                await LoadCollectionsAsync(); // Reload the collections to reflect the changes
                            }
                        }
                        else
                        {
                            // Get a list of potential parent collections (folders) to move into
                            var potentialParents = Library.Where(c => c.IsFolder && c.Id != collectionToMove.Id).ToList();
                            if (collectionToMove.IsFolder)
                            {
                                await DisplayAlertAsync(
                                    "Unable to Move",
                                    "Curator does not yet support moving folders.",
                                    "OK");
                                return;
                            }
                            else if (potentialParents.Count == 0)
                            {
                                await DisplayAlertAsync(
                                    "No Folders Available",
                                    "There are no available folders to move this collection into.",
                                    "OK");
                                return;
                            }
                            // Create a list of folder names for the action sheet
                            var folderNames = potentialParents.Select(f => f.Name).ToArray();
                            string? selectedFolderName = await DisplayActionSheetAsync(
                                "Move Collection",
                                "Cancel",
                                null,
                                folderNames);

                            if (!string.IsNullOrWhiteSpace(selectedFolderName) && selectedFolderName != "Cancel")
                            {
                                var selectedFolder = potentialParents.FirstOrDefault(f => f.Name == selectedFolderName);
                                if (selectedFolder != null)
                                {
                                    collectionToMove.ParentCollectionId = selectedFolder.Id;
                                    await curatorDatabase.SaveCollectionAsync(collectionToMove);
                                    // Update the item count of the current parent folder
                                    selectedFolder.ItemCount = await curatorDatabase.GetCollectionCountAsync(selectedFolder.Id);

                                    await LoadCollectionsAsync(); // Reload the collections to reflect the changes
                                }
                            }
                        }
                    }
                    break;
                default:
                    // Cancel or unrecognized option
                    break;
            }
        }
        private async void OpenFolderCollectionClicked(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is Collection collectionToOpen &&
                collectionToOpen.IsFolder)
            {
                currentFolderId = collectionToOpen.Id;
                currentFolderName = collectionToOpen.Name;
                HeaderLabel.Text = CurrentLocation;
                await LoadCollectionsAsync();
            }
        }

        private async Task BackToRootAsync()
        {
            currentFolderId = null;
            currentFolderName = "";
            HeaderLabel.Text = CurrentLocation;
            await LoadCollectionsAsync();
        }

        private async void BackToRootClicked(object? sender, EventArgs e)
        {
            await BackToRootAsync();
        }
    } //MainPage class end
} //namespace end
