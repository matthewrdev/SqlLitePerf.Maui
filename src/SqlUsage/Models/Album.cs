using System;
using SQLite;

namespace SqlUsage.Models
{
	public class Album
    {
        [PrimaryKey]
        public int AlbumId { get; set; }

		public string Title { get; set; }

		[Indexed]
		public int ArtistId { get; set; }
	}
}

