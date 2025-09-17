using System;
using System.Data.SQLite;
using Unleasharp.DB.Base;

namespace Unleasharp.DB.SQLite;

/// <summary>
/// Represents a connector for managing connections to a SQLite database.
/// </summary>
/// <remarks>This class provides functionality to establish, manage, and terminate connections to a SQLite
/// database. It extends the base functionality provided by <see cref="Unleasharp.DB.Base.Connector{TConnector,
/// TConnectionStringBuilder}"/>. Use this class to interact with a SQLite database by providing a connection string or a
/// pre-configured <see cref="SQLiteConnectionStringBuilder"/>.</remarks>
public class Connector : Unleasharp.DB.Base.Connector<Connector, SQLiteConnection, SQLiteConnectionStringBuilder> {
    #region Default constructors
    /// <inheritdoc />
    public Connector(SQLiteConnectionStringBuilder stringBuilder) : base(stringBuilder) { }
    /// <inheritdoc />
    public Connector(string connectionString)                     : base(connectionString) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Connector"/> class using the specified SQLite connection.
    /// </summary>
    /// <param name="connection">The <see cref="SQLiteConnection"/> instance to be used by the connector. Cannot be <see langword="null"/>.</param>
    public Connector(SQLiteConnection connection) {
        this.Connection = connection;
    }
    #endregion
}
