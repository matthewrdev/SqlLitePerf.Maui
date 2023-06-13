using System;
using SQLite;

namespace SqlUsage.Models
{
	public class Artist
    {
        [PrimaryKey]
        public int ArtistId { get; set; }

		public string Name { get; set; }
	}
}

