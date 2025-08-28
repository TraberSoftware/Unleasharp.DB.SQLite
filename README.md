# 💾 Unleasharp.DB.SQLite

[![NuGet version (Unleasharp.DB.SQLite)](https://img.shields.io/nuget/v/Unleasharp.DB.SQLite.svg?style=flat-square)](https://www.nuget.org/packages/Unleasharp.DB.SQLite/)
[![GitHub Wiki](https://img.shields.io/badge/Wiki-Documentation-blue)](https://github.com/TraberSoftware/Unleasharp.DB.Base/wiki)

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
<PackageReference Include="Unleasharp.DB.SQLite" Version="1.4.0" />
```

## 🎯 Features

- **SQLite-Specific Query Rendering**: Custom query building and rendering tailored for SQLite
- **Connection Management**: Robust connection handling through ConnectorManager
- **Query Builder Integration**: Seamless integration with the base QueryBuilder
- **Schema Definition Support**: Full support for table and column attributes

## 🚀 Kickstart
```csharp
var db  = new ConnectorManager("Data Source=unleasharp.db;Version=3;")
var row = db.QueryBuilder().Build(query => query
    .From<ExampleTable>()
    .OrderBy<ExampleTable>(row => row.Id, OrderDirection.DESC)
    .Limit(1)
    .Select()
).FirstOrDefault<ExampleTable>();
```

## 📖 Documentation Resources

- 📚 **[GitHub Wiki](https://github.com/TraberSoftware/Unleasharp.DB.Base/wiki)** - Complete documentation
- 🎯 **[Getting Started Guide](https://github.com/TraberSoftware/Unleasharp.DB.Base/wiki/2.-Getting-Started)** - Quick start guide

## 📦 Dependencies

- [Unleasharp.DB.Base](https://github.com/TraberSoftware/Unleasharp.DB.Base) - Base abstraction layer
- [System.Data.SQLite.Core](https://system.data.sqlite.org/home/doc/trunk/www/index.md) - SQLite driver for .NET

## 📋 Version Compatibility

This library targets .NET 6.0 and later versions. For specific version requirements, please check the package dependencies.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

*For more information about Unleasharp.DB.Base, visit: [Unleasharp.DB.Base](https://github.com/TraberSoftware/Unleasharp.DB.Base)*