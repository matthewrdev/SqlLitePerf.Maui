# Write Blazing Fast SQL in MAUI
Improve database reads by 300-500% in MAUI apps that use [sqlite-net](https://github.com/praeclarum/sqlite-net).

This is achieved through by-passing the use of reflection to map a tables columns to an objects properties.

The source code for this can be found at:

 * [SqlHelper](src/SqlUsage/Helpers/SqlHelper.cs).
 * [ChinookDatabase](src/SqlUsage/ChinookDatabase.cs).

On a Pixel 6, in release mode, the query changes yield the following results:

|  Query | ORM Mapped Queries  |  Manually Mapped Queries |
|---|---|---|
|  Fetch Artists | ~10ms  | ~3ms  |
|  Fetch Albums |  ~16ms |  ~4ms |
|  Fetch Tracks | ~240ms  |  ~35ms |

Mapped queries also have the following benefits:

 * **Less Memory Churn**: By avoiding reflection, less objects are created and the garbage collector less likely to be triggered.
 * **Less CPU pressure**: Reflection requires more "work" to figure out the property types and then do the property mapping.

This approach has the following drawbacks:

 * Data retrieval code may become brittle and inflexible. If you add a new column/property, you must update the associated mapper.
 * Requires deterministic order of column retrieval; the mapping code is tightly coupled to the order that columns are retrieved.

This approach is **RECOMMENDED** if:

 * Your ORM models are "well-baked" and rarely change.
 * You have identified your app requires optimisation.
 * You are willing to accept increased code complexity to achieve a significant performance jump in your database access.
 * You have unit tests in place to prevent regressions that this code may introduce.

This approach is **NOT RECOMMENDED** if:

 * Your ORM models are rapidly changing.
 * Your app is young and rapidly changing.
 * You do not have unit test coverage to prevent regressions.
 * Your data retrieval code is not isolated into a data layer via the [Repository Pattern](https://deviq.com/design-patterns/repository-pattern).


## Example

We can use `SqlHelper.ExecuteCancellableQuery` and provide a `Mapper` function:

```
public List<Album> GetAlbums()
{
    const string query = $"SELECT AlbumId, Title, ArtistId FROM {nameof(Album)}";

    return SqlHelper.ExecuteCancellableQuery<Album>(connection,
                                                    query,
                                                    emptyParameters,
                                                    MapAlbum, // This function accepts a sqlite3_stmt statment and does manual mapping.
                                                    CancellationToken.None);
}

private Album MapAlbum(sqlite3_stmt statement)
{
    // Use the appropriate sqlite read to specifically retrieve each column and then directly map via `new Album`.
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
