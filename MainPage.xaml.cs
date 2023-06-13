using System.Diagnostics;
using System.Reflection;
using SqlUsage.Helpers;
using SqlUsage.Models;

namespace SqlUsage;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    public ChinookDatabase Database => App.Instance.Database;

    void RunOrmTest(System.Object sender, System.EventArgs e)
    {
        try
        {
            busyIndicator.IsVisible = true;

            List<Artist> artists = null;
            using (Profiler.Profile("Fetch Artists - ORM"))
            {
                artists = Database.GetArtists_ORM();
            }

            List<Album> albums = null;
            using (Profiler.Profile("Fetch Albums - ORM"))
            {
                albums = Database.GetAlbums_ORM();
            }

            List<Track> tracks = null;
            using (Profiler.Profile("Fetch Tracks - ORM"))
            {
                tracks = Database.GetTracks_ORM();
            }
        }
        finally
        {
            busyIndicator.IsVisible = false;
        }
    }

    void RunMappingTest(System.Object sender, System.EventArgs e)
    {
        try
        {
            busyIndicator.IsVisible = true;

            List<Artist> artists = null;
            using (Profiler.Profile("Fetch Artists - Mapped"))
            {
                artists = Database.GetArtists_Mapped();
            }

            List<Album> albums = null;
            using (Profiler.Profile("Fetch Albums - Mapped"))
            {
                albums = Database.GetAlbums_Mapped();
            }

            List<Track> tracks = null;
            using (Profiler.Profile("Fetch Tracks - Mapped"))
            {
                tracks = Database.GetTracks_Mapped();
            }
        }
        finally
        {
            busyIndicator.IsVisible = false;
        }
    }
}