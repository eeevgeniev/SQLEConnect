# SQLEConnect 
Project site - https://github.com/eeevgeniev/SQLEConnect

Table of contents

1. [Use cases](#use-cases)
2. [Supported types](#supported-types)
3. [Repository description](#reposity-description)
4. [Some considerations](#some-considerations)


SQLEConnect is wrapper library for `System.Data.Common.DbConnection` for parsing SQL query results to supported by the library .NET types. 

## Use cases

### Query for multiple results:

```
using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString, useTransaction, isolationLevel, timeout);

List<Model> models = connection.Query<Model>(commandText, new List<SqlEParameter>() { new SqlEParameter("@id", 1) }, isStoredProcedure, prepareCommand);
```

Where:
1. `connectionString` is SQL connection string;
2. `useTransaction` is `boolean` value, which by default is `false`. If `true` the `Connection` instance will wrap all queries in transaction. This transaction can be completed either by calling `CommitTransaction` of the `Connection` instance or when this instance is disposed by the `Dispose` method;
3. `isolationLevel` - the transaction isolation level, by default is `null`;
4. `timeout` - the `CommandTimeout`, by default is `null` and it will use the default value of `CommandTimeout` property of `DbCommand` instance;
5. commandText for example "SELECT * FROM models WHERE id = @id;" is the query command or stored procedure name if `isStoredProcedure` is `true`;
6. SQL parameters of type `IEnumerable<SqlEParameter>` or `null`;
7. `isStoredProcedure` is `boolean` value which is by default `false`. If set to `true` the `Connection` `Query` method will use stored procedure;
8. `prepareCommand` is `boolean` value, which is by default `false`. Used to prepare the command if possible;

### Query for single result:

```
using Connection<SqlConnection> connection = new Connection<SqlConnection>(connectionString);

var (hasResult, value) = connection.Single<int>("SELECT TOP 1 number FROM table;", null);
```
Where:
1. `hasResult` is `boolean` and it is `true` if there is a result;
2. `value` - the actual value, in the example it is of type `int`. If `hasResult` is **false** the `value` will be the default value for the type;

### Query with no results:

```
using Connection<SqliteConnection> connection = new Connection<SqliteConnection>(connectionString);

int deleted = await connection.NonQueryAsync("DELETE FROM table WHERE id < @id;", new List<SqlEParameter>() { new SqlEParameter("@id", 4) });
```
Where the result is normally the count of the affected fields.

### Queries in different query statement/command:

```
using Connection<MySqlConnection> connection = new Connection<MySqlConnection>(connectionString);

List<Model> models = connection.Query<Model>("SELECT * FROM models WHERE id < @id;", new List<SqlEParameter>() { new SqlEParameter("@id", 11) });

List<int> numbers = connection.Query<int>("SELECT id FROM models WHERE id < @id;", new List<SqlEParameter>() { new SqlEParameter("@id", 21) });
```

### Multiple queries in single query statement/command:

```
using Connection<OracleConnection> connection = new Connection<OracleConnection>(connectionString);

var (models, numbers) = await connection.QueryMultipleAsync<Model, int>(
    "SELECT * FROM models WHERE id < @id; SELECT id FROM models WHERE id < @id;", 
    new List<SqlEParameter>() { new SqlEParameter("@id", 1) })
```
Where
1. `models` is of type `List<Model>` and `numbers` is of type `List<int>`

**Note** currently up to 10 results are supported.

**Note** every mehtod has alternative async method with the same name and postfix 'Async'.

### Adding or replacing parser with custom one:

Instance method:
```
using Connection<SqliteConnection> connection = new Connection<SqliteConnection>(connectionString);

connection.AddOrUpdate(new ModelParser());
```

Static method
```
Connection<SqliteConnection>.AddOrUpdateParser(new ModelParser());
```

Where the parser must implement interface `SQLEConnect.Interfaces.IParser<TModel>`. With the following structure:

```
// for multiple results
IEnumerable<TModel> Parse(DbDataReader dbDataReader);

// for single result
(bool hasResult, TModel result) ParseSingle(DbDataReader dbDataReader);

Type Type();
```

**Note** The parsers are stored in static container to be used by all instances.

### Dynamic and object support
Support for `dynamic` and `object` with `System.Dynamic.ExpandoObject`. 
```
    using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString);
    var models = connection.Query<dynamic>("SELECT * FROM models;", null);
```

```
    using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString);
    var objects = connection.Query<object>("SELECT * FROM models;", null);
```

**Note** if you want to access List<object> you must cast it to `System.Dynamic.ExpandoObject` or `dynamic`.

For single query
```
    (bool hasResult, dynamic result) x = connection.Single<dynamic>("SELECT * FROM models ORDER BY id LIMIT 1;", null);
```

### Clearing all parsers

Instance method:
```
using Connection<SqlConnection> connection = new Connection<SqlConnection>(connectionString);

connection.ClearParsers();
```

Static method:
```
Connection<SqliteConnection>.ClearAllParsers();
```

## Supported types

Currently the supported types are:
1. String - with `DbDataReader.GetString`;
2. Char - with DbDataReader.GetString(). **Note** the result is received from `GetString` method, it must be `Null` or white space or string with length of 1 otherwise an `InvalidOperationException` is throw;
3. Char? - with DbDataReader.GetString() **Note** the result is received from `GetString`, it must be `Null` or white space or string with length of 1 otherwise an `InvalidOperationException` is throw;
4. Byte - with `DbDataReader.GetByte`;
5. Byte? - with `DbDataReader.GetByte`;
6. SByte - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToSByte`;
7. SByte? - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToSByte`;
8. Short - with `DbDataReader.GetInt16`;
9. Short? - with `DbDataReader.GetInt16`;
10. UShort - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToUInt16`;
11. UShort? - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToUInt16`;
12. Int - with `DbDataReader.GetInt32`;
13. Int? - with `DbDataReader.GetInt32`;
14. UInt - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToUInt32`;
15. UInt? - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToUInt32`;
16. Long - with `DbDataReader.GetInt64`;
17. Long? - with `DbDataReader.GetInt64`;
18. ULong - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToUInt64`;
19. ULong? - with `DbDataReader.GetValue`. **Note** Converted with `Convert.ToUInt64`;
20. Float - with `DbDataReader.GetFloat`;
21. Float? - with `DbDataReader.GetFloat`;
22. Double - with `DbDataReader.GetDouble`;
23. Double? - with `DbDataReader.GetDouble`;
24. Decimal - with `DbDataReader.GetDecimal`;
25. Decimal? - with `DbDataReader.GetDecimal`;
26. DateTime - with `DbDataReader.GetDateTime`;
27. DateTime? - with `DbDataReader.GetDateTime`;
28. Boolean - with `DbDataReader.GetBoolean`;
29. Boolean? - with `DbDataReader.GetBoolean`;
30. Guid - with `DbDataReader.GetGuid`;
31. Guid? - with `DbDataReader.GetGuid`;
32. Byte[] - with `DbDataReader.GetBytes`;
33. Char[] - with `DbDataReader.GetChars`;
34. Stream - with `DbDataReader.GetStream`. **Note** the result is stored in `MemoryStream`;
35. Custom classes - custom classes/models;
36. Custom structs - custom classes/models;
37. Dictionary<string, object> - generic result;
38. Object and dynamic with `System.Dynamic.ExpandoObject`

For all types except `Object`, `Struct` and `Dictionary<string, object>` the first selected column from the SQL query must be from compatible type. Only the first column is checked.
For example:

```
List<int> numbers = connection.Query<int>("SELECT Id, Name FROM models;", null)
```

`Id` column must be compatible with `Integer` type. The `Name` column is never checked.

The `Dictionary<string, object>` is used when there is no need to save the results in specific model. The results are parsed based on the `DbDataReader` -> `GetFieldType` method.

Example:
```
using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString, useTransaction, isolationLevel, timeout);

List<Dictionary<string, object>> models = connection.Query<Dictionary<string, object>>("SELECT * FROM models WHERE id = @id;", new List<SqlEParameter>() { new SqlEParameter("@id", 1) });
```

For every record in the `DbDataReader` a new `Dictionary<string, object>` is created where the key is the column name and the value is the row value of the `DbDataReader`.

`Object` and `Struct` results are parsed based on either their public type properties or public type fields. First are checked the public properties if there are no public properties then are checked the public fields. When the instance of class or struct is created, it is used the constructor with least parameters. This constructor must accept default values for parameters.

## Reposity description

The solution contains 3 projects:
1. `SQLEConnect` - main project;
2. `SQLEConnectTests` - project with test against databases;
3. `SQLEConnectUnitTests` - project with unit tests;

`EConnect` contains:
* Directory `Infrastructure` - contains various helpers;
* Directory `Interfaces` - contains various `Interfaces`;
* Directory `ParserFactories` - contains the main class for creating parsers;
* Directory `Parsers` - contains all classes for parsing;
* Class `Connection` - main class used by the library;
* Class `SqlEParameter` - main class for using SQL parameters;

`SQLEConnectTests` contains:
* `SQLEConnectTests.Benchmarks` project - used to run benchmarks in either `Develop` environment or `Release` environment. In `Release` environment the test are run with `BenchmarkDotNet` library;
* `SQLEConnectTests.Models` project - with some model-classes used by the other projects;
* `SQLEConnectTests.MySQLQueries` project - with tests for MySQL;
* `SQLEConnectTests.OracleDatabaseQueries` project - with tesst for Oracle;
* `SQLEConnectTests.PostgreSQLQueries` project - with tests for PostgreSQL;
* `SQLEConnectTests.SettingParser` project - for reading `settings.json`;
* `SQLEConnectTests.Settings` project - `settings.json` model;
* `SQLEConnectTests.SQLiteQueries` project - with tests for SQLite;
* `SQLEConnectTests.SQLServerQueries` project - with tests for SQLServer;

**Note** every project has `settings.json` with property `ConnectionString` where the connection string must be entered. It's empty by default.

`SQLEConnectUnitTests` contains:
Projects with unit tests which are organized by type. For example: `QueryBoolTests` contains tests for `Boolean` and `Boolean?` types and all queries. And `SingleBoolTests` contains tests for `Boolean` and `Boolean?` but only for single queries. There are some helper directories like:
* `Mockups` for some mockups;
* `TestModels` for some tests models;

## Some considerations

Because this library is wrapper for `System.Data.Common.DbConnection` it depends on different implementations of `DbDataReader`, for example: SQL Server (`SqlConnection`) or SQLite (`SqliteConnection`). This may lead to some methods to be not implemented, for example: the `GetChar`, or `GetGuid` methods. It's possible if you change the SQL provider some models to not work.