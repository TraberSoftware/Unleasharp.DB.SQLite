using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;
using Unleasharp.ExtensionMethods;

namespace Unleasharp.DB.SQLite;

public class QueryBuilder : Base.QueryBuilder<QueryBuilder, Connector, Query, SQLiteConnection, SQLiteConnectionStringBuilder> {
    public QueryBuilder(Connector dbConnector) : base(dbConnector) { }

    public QueryBuilder(Connector dbConnector, Query query) : base(dbConnector, query) { }

    #region Query execution
    protected override bool _Execute() {
        this.DBQuery.RenderPrepared();

        try {
            using (SQLiteCommand queryCommand = new SQLiteCommand(this.DBQuery.QueryPreparedString, this.Connector.Connection)) {
                switch (this.DBQuery.QueryType) {
                    case Base.QueryBuilding.QueryType.COUNT:
                        this._PrepareDbCommand(queryCommand);

                        if (queryCommand.ExecuteScalar().TryConvert<int>(out int scalarCount)) {
                            this.TotalCount = scalarCount;
                        }
                        return true;
                    case Base.QueryBuilding.QueryType.SELECT:
						this._PrepareDbCommand(queryCommand);

						using (SQLiteDataReader queryReader = queryCommand.ExecuteReader()) {
                            this._HandleQueryResult(queryReader);
                        }
                        return true;
                    case Base.QueryBuilding.QueryType.UPDATE:
                    default:
						this._PrepareDbCommand(queryCommand);

						this.AffectedRows = queryCommand.ExecuteNonQuery();

                        return true;
                }
            }
        }
        catch (Exception ex) {
            this._OnQueryException(ex);
        }

        return false;
    }

    private void _PrepareDbCommand(SQLiteCommand queryCommand) {
		foreach (string queryPreparedDataKey in this.DBQuery.QueryPreparedData.Keys) {
            if (this.DBQuery.QueryPreparedData[queryPreparedDataKey].Value is Enum) {
                queryCommand.Parameters.AddWithValue(queryPreparedDataKey, ((Enum)this.DBQuery.QueryPreparedData[queryPreparedDataKey].Value).GetDescription());
                continue;
            }
            queryCommand.Parameters.AddWithValue(queryPreparedDataKey, this.DBQuery.QueryPreparedData[queryPreparedDataKey].Value);
		}
		queryCommand.Prepare();
	}

	private void _HandleQueryResult(SQLiteDataReader queryReader) {
        this.Result = new DataTable();

        for (int i = 0; i < queryReader.FieldCount; i++) {
            this.Result.Columns.Add(new DataColumn(queryReader.GetName(i), queryReader.GetFieldType(i)));
        }

        object[] rowData = new object[this.Result.Columns.Count];

        this.Result.BeginLoadData();
        while (queryReader.Read()) {
            queryReader.GetValues(rowData);
            this.Result.LoadDataRow(rowData, true);

            // Reinstanciate the row data holder
            rowData = new object[this.Result.Columns.Count];
        }
        this.Result.EndLoadData();
    }
    #endregion
}
