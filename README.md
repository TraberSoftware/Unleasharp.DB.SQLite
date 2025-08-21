# 💾 Unleasharp.DB.SQLite

[![Unleasharp.DB.SQLite](https://socialify.git.ci/TraberSoftware/Unleasharp.DB.SQLite/image?description=1&font=Inter&logo=https%3A%2F%2Fraw.githubusercontent.com%2FTraberSoftware%2FUnleasharp%2Frefs%2Fheads%2Fmain%2Fassets%2Flogo-small.png&name=1&owner=1&pattern=Circuit+Board&theme=Light)](https://github.com/TraberSoftware/Unleasharp.DB.SQLite)

SQLite implementation of Unleasharp.DB.Base. This repository provides a SQLite-specific implementation that leverages the base abstraction layer for common database operations.

## 📦 Installation

Install the NuGet package using one of the following methods:

### Package Manager Console
```powershell
Install-Package Unleasharp.DB.SQLite
```

### .NET CLI
```bash
dotnet add package Unleasharp.DB.SQLite
```

### PackageReference (Manual)
```xml
<PackageReference Include="Unleasharp.DB.SQLite" Version="1.1.1" />
```

## 🎯 Features

- **SQLite-Specific Query Rendering**: Custom query building and rendering tailored for SQLite
- **Connection Management**: Robust connection handling through ConnectorManager
- **Query Builder Integration**: Seamless integration with the base QueryBuilder
- **Schema Definition Support**: Full support for table and column attributes

## 🚀 Connection Initialization

The `ConnectorManager` handles database connections. You can initialize it using a connection string or `SQLiteConnectionStringBuilder`.

### Using Connection String
```csharp
ConnectorManager DBConnector = new ConnectorManager("Data Source=unleasharp.db;Version=3;");
```

### Using Fluent Configuration
```csharp
ConnectorManager DBConnector = new ConnectorManager()
    .WithAutomaticConnectionRenewal(true)
    .WithAutomaticConnectionRenewalInterval(TimeSpan.FromHours(1))
    .Configure(config => {
        config.ConnectionString = "Data Source=unleasharp.db;Version=3;";
    });
```

### Using SQLiteConnectionStringBuilder
```csharp
ConnectorManager DBConnector = new ConnectorManager(
    new SQLiteConnectionStringBuilder("Data Source=unleasharp.db;Version=3;")
);
```

## 📝 Usage Examples

### Sample Table Structure

```csharp
using System.ComponentModel;
using Unleasharp.DB.Base.SchemaDefinition;

namespace Unleasharp.DB.SQLite.Sample;

[Table("example_table")]
public class ExampleTable 
{
    [Column("id", "integer", Unsigned = true, PrimaryKey = true, AutoIncrement = true, NotNull = true)]
    public long? Id         { get; set; }

    [Column("_mediumtext", "text")]
    public string MediumText { get; set; }

    [Column("_longtext", "text")]
    public string Longtext  { get; set; }

    [Column("_json", "text")]
    public string Json      { get; set; }

    [Column("_longblob", "blob")]
    public byte[] CustomFieldName { get; set; }

    [Column("_enum", "text")]
    public EnumExample? Enum { get; set; }

    [Column("_varchar", "varchar", Length = 255)]
    public string Varchar { get; set; }
}

public enum EnumExample 
{
    NONE,
    Y,
    [Description("NEGATIVE")]
    N
}
```

### Sample Program

```csharp
using System;
using System.Collections.Generic;
using Unleasharp.DB.SQLite;
using Unleasharp.DB.Base.QueryBuilding;

namespace Unleasharp.DB.SQLite.Sample;

internal class Program 
{
    static void Main(string[] args) 
    {
        // Initialize database connection
        ConnectorManager DBConnector = new ConnectorManager("Data Source=unleasharp.db;Version=3;");
        
        // Create table
        DBConnector.QueryBuilder().Build(Query => Query.Create<ExampleTable>()).Execute();
        
        // Insert data
        DBConnector.QueryBuilder().Build(Query => { Query
            .From<ExampleTable>()
            .Value(new ExampleTable {
                MediumText = "Medium text example value",
                _enum      = EnumExample.N
            })
            .Values(new List<ExampleTable> {
                new ExampleTable {
                    _json           = @"{""sample_json_field"": ""sample_json_value""}",
                    _enum           = EnumExample.Y,
                    CustomFieldName = new byte[8] { 81,47,15,21,12,16,23,39 }
                },
                new ExampleTable {
                    _longtext = "Long text example value",
                    ID        = 999 // RandomID placeholder
                }
            })
            .Insert();
        }).Execute();
        
        // Select single row
        ExampleTable Row = DBConnector.QueryBuilder().Build(Query => Query
            .From("example_table")
            .OrderBy("id", OrderDirection.ASC)
            .Limit(1)
            .Select()
        ).FirstOrDefault<ExampleTable>();
        
        // Select multiple rows with different class naming
        List<example_table> Rows = DBConnector.QueryBuilder().Build(Query => Query
            .From("example_table")
            .OrderBy("id", OrderDirection.DESC)
            .Select()
        ).ToList<example_table>();
    }
}
```

### Sample Query Rendering

```csharp
// Complex query demonstration with subqueries
Query VeryComplexQuery = Query.GetInstance()
    .Select("query_field")
    .Select($"COUNT({new FieldSelector("table_x", "table_y")})", true)
    .From("query_from")
    .Where("field", "value")
    .WhereIn(
        "field_list",
        Query.GetInstance()
            .Select("*", false)
            .From("subquery_table")
            .Where("subquery_field", true)
            .WhereIn(
                "subquery_in_field",
                Query.GetInstance()
                    .Select("subquery_subquery_in_field")
                    .From("subquery_subquery_in_table")
                    .Where("subquery_subquery_in_where", true)
            )
            .Limit(100)
    )
    .WhereIn("field_list", new List<dynamic> { null, 123, 456, "789" })
    .Join("another_table", new FieldSelector("table_x", "field_x"), new FieldSelector("table_y", "field_y"))
    .OrderBy(new OrderBy {
        Field     = new FieldSelector("order_field"),
        Direction = OrderDirection.DESC
    })
    .GroupBy("group_first")
    .GroupBy("group_second")
    .Limit(100);

// Render raw SQL query
Console.WriteLine(VeryComplexQuery.Render());

// Render prepared statement query (with placeholders)
Console.WriteLine(VeryComplexQuery.RenderPrepared());
```

## 📦 Dependencies

- [Unleasharp.DB.Base](https://github.com/TraberSoftware/Unleasharp.DB.Base) - Base abstraction layer
- [System.Data.SQLite.Core](https://system.data.sqlite.org/home/doc/trunk/www/index.md) - SQLite driver for .NET

## 📋 Version Compatibility

This library targets .NET 8.0 and later versions. For specific version requirements, please check the package dependencies.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

*For more information about Unleasharp.DB.Base, visit: [Unleasharp.DB.Base](https://github.com/TraberSoftware/Unleasharp.DB.Base)*