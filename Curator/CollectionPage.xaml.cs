using CommunityToolkit.Maui.Core;
using Curator.Data;
using Curator.Models;
using System.Collections.ObjectModel;

namespace Curator;

public partial class CollectionPage : ContentPage
{
	private readonly Collection collection;

	private readonly CuratorDatabase curatorDatabase;
}