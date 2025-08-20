using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Unleasharp.DB.Base;

namespace Unleasharp.DB.SQLite;

public class ConnectorManager : 
    ConnectorManager<ConnectorManager, Connector, SQLiteConnectionStringBuilder>, 
    IConnectorManager<QueryBuilder, Connector, Query, SQLiteConnection, SQLiteConnectionStringBuilder> 
{
    public ConnectorManager()                                            : base() { }
    public ConnectorManager(SQLiteConnectionStringBuilder stringBuilder) : base(stringBuilder) { }
    public ConnectorManager(string connectionString)                     : base(connectionString) { }

    public QueryBuilder QueryBuilder() {
        return new QueryBuilder(this.GetInstance());
    }

    public QueryBuilder DetachedQueryBuilder() {
        return new QueryBuilder(this.GetDetachedInstance());
    }

    public QueryBuilder QueryBuilder(Query query) {
        return new QueryBuilder(this.GetInstance(), query);
    }

    public QueryBuilder DetachedQueryBuilder(Query query) {
        return new QueryBuilder(this.GetDetachedInstance(), query);
    }
}
