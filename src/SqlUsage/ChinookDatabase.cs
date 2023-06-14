using System;
using SQLite;
using SQLitePCL;
using SqlUsage.Helpers;
using SqlUsage.Models;

namespace SqlUsage
{
	public class ChinookDatabase
	{
        private readonly SQLiteConnection connection;

        public ChinookDatabase(string databaseFilePath)
		{
			connection = new SQLite.SQLiteConnection(databaseFilePath);
            connection.Trace = true;
            connection.Tracer = TraceSqlQuery;

            connection.CreateTable<Artist>();
            connection.CreateTable<Album>();
            connection.CreateTable<Artist>();
        }

        private void TraceSqlQuery(string query)
        {
            Console.WriteLine("Execute SQLite Query: " + query);
        }

        private static readonly Dictionary<string, object> emptyParameters = new Dictionary<string, object>();

        internal List<Album> GetAlbums_Mapped()
        {
            //const string query = $"SELECT AlbumId, Title, ArtistId FROM {nameof(Album)}";
            const string query = $"SELECT * FROM {nameof(Album)}";

            return SqlHelper.ExecuteCancellableQuery<Album>(connection, query, emptyParameters, MapAlbum, CancellationToken.None);
        }

        private Album MapAlbum(sqlite3_stmt statement)
        {
            var albumId = SQLite3.ColumnInt(statement, 0);
            var title = SQLite3.ColumnString(statement, 1);
            var artistId = SQLite3.ColumnInt(statement, 2);

            return new Album()
            {
                AlbumId = albumId,
                ArtistId = artistId,
                Title = title
            };
        }

        internal List<Album> GetAlbums_ORM()
        {
            return connection.Query<Album>($"SELECT * FROM {nameof(Album)}");
        }

        internal List<Artist> GetArtists_Mapped()
        {
            //const string query = $"SELECT ArtistId, Name FROM {nameof(Artist)}";
            const string query = $"SELECT * FROM {nameof(Artist)}";

            return SqlHelper.ExecuteCancellableQuery<Artist>(connection, query, emptyParameters, MapArtist, CancellationToken.None);
        }

        private Artist MapArtist(sqlite3_stmt statement)
        {
            var artistId = SQLite3.ColumnInt(statement, 0);
            var name = SQLite3.ColumnString(statement, 1);

            return new Artist()
            {
                ArtistId = artistId,
                Name = name
            };
        }

        internal List<Artist> GetArtists_ORM()
        {
            return connection.Query<Artist>($"SELECT * FROM {nameof(Artist)}");
        }

        internal List<Track> GetTracks_Mapped()
        {
            //const string query = $"SELECT TrackId, Name, AlbumId, MediaTypeId, GenreId, Composer, Milliseconds, Bytes, UnitPrice FROM {nameof(Track)}";
            const string query = $"SELECT * FROM {nameof(Track)}";

            return SqlHelper.ExecuteCancellableQuery<Track>(connection, query, emptyParameters, MapTrack, CancellationToken.None);
        }

        private Track MapTrack(sqlite3_stmt statement)
        {
            var TrackId = SQLite3.ColumnInt(statement, 0);
            var Name = SQLite3.ColumnString(statement, 1);
            var AlbumId = SQLite3.ColumnInt(statement, 2);
            var MediaTypeId = SQLite3.ColumnInt(statement, 3);
            var GenreId = SQLite3.ColumnInt(statement, 4);
            var Composer = SQLite3.ColumnString(statement, 5);
            var Milliseconds = SQLite3.ColumnInt(statement, 6);
            var Bytes = SQLite3.ColumnInt(statement, 7);
            var UnitPrice = SQLite3.ColumnDouble(statement, 8);

            return new Track()
            {
                TrackId = TrackId,
                Name = Name,
                AlbumId = AlbumId,
                MediaTypeId = MediaTypeId,
                GenreId = GenreId,
                Composer = Composer,
                Milliseconds = Milliseconds,
                Bytes = Bytes,
                UnitPrice = UnitPrice,
            };
        }

        internal List<Track> GetTracks_ORM()
        {
            return connection.Query<Track>($"SELECT * FROM {nameof(Track)}");
        }
    }
}

		