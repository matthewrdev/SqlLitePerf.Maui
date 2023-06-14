# Write Blazing Fast SQL in MAUI
Gain up to a 300-500% data read improvement in MAUI apps that use [sqlite-net](https://github.com/praeclarum/sqlite-net).

This is achieved by bye-passing the use of reflection to map an objects properties to columns

The source code for this can be found at:

 * [SqlHelper](Helpers/SqlHelper.cs).
 * [ChinookDatabase](ChinookDatabase.cs).

On a Pixel 6, in release mode, the query changes yield the following results:

**ORM Mapped Queries**

 * Fetch Artists: ~10ms
 * Fetch Albums: ~16ms
 * Fetch Tracks: ~240ms

**Manually Mapped Queries**

 * Fetch Artists: ~3ms
 * Fetch Albums: ~4ms
 * Fetch Tracks: ~35ms

It's important to note the **Mapped* queries also have the following benefits:

 * Less Memory Churn: By avoiding reflection, less objects are created and the garbage collector less likely to be triggered.
 * Less CPU pressure: Reflection requires more "work" to figure out the property types and then do the property mapping.

This approach has the following drawbacks:

 * Data retrieval code may become brittle and inflexible. If you add a new column/property, you must update the associated mapper.
 * Requires deterministic order of column retrieval.

This approach is **RECOMMENDED** if:

 * Your ORM models are "well-baked" and no longer change too often.
 * You have identified that your app requires optimisation.
 * You are willing to accept some technical debt in order to achieve a significant performance jump.
 * You have unit tests in place to prevent regressions that this code may introduce.

This approach is **NOT RECOMMENDED** if:

 * Your ORM models are rapidly changing.
 * Your app is young and rapidly changing.
 * You do not have unit test coverage to prevent regressions.
 * Your data retrieval code is not isolated into a data layer via the [Repository Pattern](https://deviq.com/design-patterns/repository-pattern).


## Example

We can use the `SqlHelper.ExecuteCancellableQuery` and provide a `Mapper` function to use this:

```
public List<Album> GetAlbums()
{
    const string query = $"SELECT AlbumId, Title, ArtistId FROM {nameof(Album)}";

    return SqlHelper.ExecuteCancellableQuery<Album>(connection,
                                                    query,
                                                    emptyParameters,
                                                    MapAlbum, // This function accepts a sqlite3_stmt statment and does manaul mapping 
                                                    CancellationToken.None);
}

private Album MapAlbum(sqlite3_stmt statement)
{
    // Use the appropriate sqlite read to specifical retrieve each column and then directly map via `new Album`.
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
```
