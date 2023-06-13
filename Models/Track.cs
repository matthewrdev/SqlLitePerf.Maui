using System;
using SQLite;

namespace SqlUsage.Models
{
	public class Track
	{
		[PrimaryKey]
		public int TrackId { get; set; }

        public string Name { get; set; }

        [Indexed]
        public int AlbumId  { get; set; }

        [Indexed]
        public int MediaTypeId { get; set; }

        [Indexed]
        public int GenreId { get; set; }

        public string Composer { get; set; }

        public int Milliseconds { get; set; }

        public int Bytes { get; set; }

        public double UnitPrice { get; set; }

        [SQLite.Ignore]
        public string IgnoreMe { get; set; }
    }
}

