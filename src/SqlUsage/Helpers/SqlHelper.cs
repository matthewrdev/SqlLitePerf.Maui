using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace SqlUsage.Helpers
{
    /// <summary>
    /// A collection of helper methods for working with SQLite databases.
    /// </summary>
	public static class SqlHelper
    {
        public static Task<List<TResult>> ExecuteCancellableQueryAsync<TResult>(SQLiteAsyncConnection connection,
                                                                                         string sql,
                                                                                         Dictionary<string, object> parameters,
                                                                                         Func<SQLitePCL.sqlite3_stmt, TResult> mapper,
                                                                                         CancellationToken cancellationToken) where TResult : class
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
            }

            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return Task.Factory.StartNew(() => {
                var conn = connection.GetConnection();
                using (conn.Lock())
                {
                    return ExecuteCancellableQuery(conn, sql, parameters, mapper, cancellationToken);
                }
            }, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public static Task<List<TResult>> ExecuteCancellableQueryAsync<TResult>(SQLiteConnection connection,
                                                                                        string sql,
                                                                                        Dictionary<string, object> parameters,
                                                                                        Func<SQLitePCL.sqlite3_stmt, TResult> mapper,
                                                                                        CancellationToken cancellationToken)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
            }

            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return Task.Factory.StartNew(() =>
            {
                return ExecuteCancellableQuery(connection, sql, parameters, mapper, cancellationToken);
            }, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public static List<TResult> ExecuteCancellableQuery<TResult>(SQLiteConnection connection,
                                                                     string sql,
                                                                     Dictionary<string, object> parameters,
                                                                     Func<SQLitePCL.sqlite3_stmt, TResult> mapper,
                                                                     CancellationToken cancellationToken)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
            }

            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            var results = new List<TResult>();

            var statement = CreateStatement(connection, sql, parameters);

            try
            {

                while (SQLite3.Step(statement) == SQLite3.Result.Row)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var element = mapper(statement);

                    if (element != null)
                    {
                        results.Add(element);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                throw tce;
            }
            catch (OperationCanceledException oex)
            {
                throw oex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                SQLite3.Finalize(statement);
            }

            return results;
        }

        private static readonly Lazy<MethodInfo> prepareMethod = new Lazy<MethodInfo>(() => typeof(SQLiteCommand).GetMethod("Prepare", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
        private static readonly Dictionary<string, object> emptyParameters = new Dictionary<string, object>();

        public static SQLitePCL.sqlite3_stmt CreateStatement(SQLiteConnection connection, string sql, Dictionary<string, object> parameters)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
            }

            parameters = parameters ?? emptyParameters;

            var command = connection.CreateCommand(sql, parameters);

            var result = prepareMethod.Value.Invoke(command, null);

            return (SQLitePCL.sqlite3_stmt)result;
        }


        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Checks if a table with the given <paramref name="tableName"/> exists in the database <paramref name="connection"/>.
        /// </summary>
        public static bool TableExists(SQLiteConnection connection, string tableName)
        {
            const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";

            var cmd = connection.CreateCommand(cmdText, tableName);
            return cmd.ExecuteScalar<string>() != null;
        }

        /// <summary>
        /// Gets all the tables in the provided <paramref name="connection"/>.
        /// </summary>
		public static List<string> Tables(this SQLiteConnection connection)
        {
            const string GET_TABLES_QUERY = "SELECT NAME from sqlite_master";

            var tables = new List<string>();

            var statement = SQLite3.Prepare2(connection.Handle, GET_TABLES_QUERY);

            try
            {
                var done = false;
                while (!done)
                {
                    var result = SQLite3.Step(statement);

                    if (result == SQLite3.Result.Row)
                    {

                        var tableName = SQLite3.ColumnString(statement, 0);

                        tables.Add(tableName);
                    }
                    else if (result == SQLite3.Result.Done)
                    {
                        done = true;
                    }
                    else
                    {
                        throw SQLiteException.New(result, SQLite3.GetErrmsg(connection.Handle));
                    }
                }
            }
            finally
            {
                SQLite3.Finalize(statement);
            }

            return tables;
        }

        /// <summary>
        /// Gets all the columns for the <paramref name="tableName"/> in the provided <paramref name="connection"/>.
        /// </summary>
        /// <returns>The for table.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="tableName">Table name.</param>
		public static List<string> ColumnsForTable(this SQLiteConnection connection, string tableName)
        {
            const string GET_COLUMNS_QUERY = "PRAGMA table_info({0})";

            var query = string.Format(GET_COLUMNS_QUERY, tableName);
            var columns = new List<string>();

            var statement = SQLite3.Prepare2(connection.Handle, query);

            try
            {
                var done = false;
                while (!done)
                {
                    var result = SQLite3.Step(statement);

                    if (result == SQLite3.Result.Row)
                    {

                        var columnName = SQLite3.ColumnString(statement, 1);

                        columns.Add(columnName);
                    }
                    else if (result == SQLite3.Result.Done)
                    {
                        done = true;
                    }
                    else
                    {
                        throw SQLiteException.New(result, SQLite3.GetErrmsg(connection.Handle));
                    }
                }
            }
            finally
            {
                SQLite3.Finalize(statement);
            }

            return columns;
        }
    }
}

