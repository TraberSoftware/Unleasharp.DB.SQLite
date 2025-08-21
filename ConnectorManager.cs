using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Unleasharp.DB.Base;

namespace Unleasharp.DB.SQLite;

/// <summary>
/// Manager class for SQLite database connections that provides access to query builders
/// for constructing and executing SQL queries.
/// </summary>
public class ConnectorManager : 
    ConnectorManager<ConnectorManager, Connector, SQLiteConnectionStringBuilder, SQLiteConnection, QueryBuilder, Query>
{
	/// <inheritdoc />
	public ConnectorManager()                                            : base() { }

	/// <inheritdoc />
	public ConnectorManager(SQLiteConnectionStringBuilder stringBuilder) : base(stringBuilder) { }

	/// <inheritdoc />
	public ConnectorManager(string connectionString)                     : base(connectionString) { }
}
