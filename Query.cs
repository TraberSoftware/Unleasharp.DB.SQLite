﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unleasharp.DB.Base.QueryBuilding;
using Unleasharp.DB.Base.SchemaDefinition;
using Unleasharp.ExtensionMethods;

namespace Unleasharp.DB.SQLite;

public class Query : Unleasharp.DB.Base.Query<Query> {
    #region Custom SQLite query data
    #endregion

    public const string FieldDelimiter = "\"";
    public const string ValueDelimiter = "'";

    #region Query rendering
    #region Query fragment rendering
    public override void _RenderPrepared(bool force) {
        this._Render(force);

        string rendered = this.QueryPreparedString;
        foreach (KeyValuePair<string, PreparedValue> preparedDataItem in this.QueryPreparedData) {
            if (preparedDataItem.Value.Value == null) {
                rendered = rendered.Replace(preparedDataItem.Key, "NULL");
            }
            else {
                rendered = rendered.Replace(preparedDataItem.Key, this.__RenderWhereValue(preparedDataItem.Value.Value, preparedDataItem.Value.EscapeValue));
            }
        }

        this.QueryRendered = rendered;
    }

    public string RenderSelect(Select<Query> fragment) {
        if (fragment.Subquery != null) {
            return "(" + fragment.Subquery.WithParentQuery(this.ParentQuery != null ? this.ParentQuery : this).Render() + ")";
        }

        return fragment.Field.Render() + (!string.IsNullOrWhiteSpace(fragment.Alias) ? $" AS {fragment.Alias}" : "");
    }

    public string RenderFrom(From<Query> fragment) {
        if (fragment.Subquery != null) {
            return "(" + fragment.Subquery.WithParentQuery(this.ParentQuery != null ? this.ParentQuery : this).Render() + ")";
        }

        string rendered = string.Empty;

        if (!string.IsNullOrWhiteSpace(fragment.Table)) {
            if (fragment.EscapeTable) {
                rendered = FieldDelimiter + fragment.Table + FieldDelimiter;
            }
            else {
                rendered = fragment.Table;
            }
        }

        return rendered + (fragment.TableAlias != string.Empty ? $" {fragment.TableAlias}" : "");
    }

    public string RenderJoin(Join<Query> fragment) {
        return $"{(fragment.EscapeTable ? FieldDelimiter + fragment.Table + FieldDelimiter : fragment.Table)} ON {this.RenderWhere(fragment.Condition)}";
    }

    public string RenderGroupBy(GroupBy fragment) {
        List<string> toRender = new List<string>();

        if (!string.IsNullOrWhiteSpace(fragment.Field.Table)) {
            if (fragment.Field.Escape) {
                toRender.Add(FieldDelimiter + fragment.Field.Table + FieldDelimiter);
            }
            else {
                toRender.Add(fragment.Field.Table);
            }
        }

        if (!string.IsNullOrWhiteSpace(fragment.Field.Field)) {
            if (fragment.Field.Escape) {
                toRender.Add(FieldDelimiter + fragment.Field.Field + FieldDelimiter);
            }
            else {
                toRender.Add(fragment.Field.Field);
            }
        }

        return String.Join('.', toRender);
    }

    public string RenderWhere(Where<Query> fragment) {
        if (fragment.Subquery != null) {
            return $"{fragment.Field.Render()} {fragment.Comparer.GetDescription()} ({fragment.Subquery.WithParentQuery(this.ParentQuery != null ? this.ParentQuery : this).Render()})";
        }

        List<string> toRender = new List<string>();

        toRender.Add(fragment.Field.Render());

        // We are comparing fields, not values
        if (fragment.ValueField != null) {
            toRender.Add(fragment.ValueField.Render());
        }
        else {
            if (fragment.Value == null) {
                fragment.Comparer = WhereComparer.IS;
                toRender.Add("NULL");
            }
            else {
                if (fragment.EscapeValue) {
                    toRender.Add(this.PrepareQueryValue(fragment.Value, fragment.EscapeValue));
                }
                else {
                    toRender.Add(this.__RenderWhereValue(fragment.Value, false));
                }
            }
        }

        return String.Join(fragment.Comparer.GetDescription(), toRender);
    }

    public string RenderWhereIn(WhereIn<Query> fragment) {
        if (fragment.Subquery != null) {
            return $"{fragment.Field.Render()} IN ({fragment.Subquery.WithParentQuery(this.ParentQuery != null ? this.ParentQuery : this).Render()})";
        }

        if (fragment.Values == null || fragment.Values.Count == 0) {
            return String.Empty;
        }

        List<string> toRender = new List<string>();

        foreach (dynamic fragmentValue in fragment.Values) {
            if (fragment.EscapeValue) {
                toRender.Add(this.PrepareQueryValue(fragmentValue, fragment.EscapeValue));
            }
            else {
                toRender.Add(__RenderWhereValue(fragmentValue, fragment.EscapeValue));
            }
        }

        return $"{fragment.Field.Render()} IN ({String.Join(",", toRender)})";
    }

    public string RenderFieldSelector(FieldSelector fragment) {
        List<string> toRender = new List<string>();

        if (!string.IsNullOrWhiteSpace(fragment.Table)) {
            if (fragment.Escape) {
                toRender.Add(FieldDelimiter + fragment.Table + FieldDelimiter);
            }
            else {
                toRender.Add(fragment.Table);
            }
        }

        if (!string.IsNullOrWhiteSpace(fragment.Field)) {
            if (fragment.Escape) {
                toRender.Add(FieldDelimiter + fragment.Field + FieldDelimiter);
            }
            else {
                toRender.Add(fragment.Field);
            }
        }

        return String.Join('.', toRender);
    }
    #endregion

    #region Query sentence rendering
    protected override string _RenderCountSentence() {
        return "SELECT COUNT(*)";
    }

    protected override string _RenderSelectSentence() {
        List<string> rendered = new List<string>();

        if (this.QuerySelect.Count > 0) {
            foreach (Select<Query> queryFragment in this.QuerySelect) {
                rendered.Add(this.RenderSelect(queryFragment));
            }
        }
        else {
            rendered.Add("*");
        }

        return "SELECT " + string.Join(',', rendered);
    }

    protected override string _RenderFromSentence() {
        List<string> rendered = new List<string>();

        foreach (From<Query> queryFragment in this.QueryFrom) {
            rendered.Add(this.RenderFrom(queryFragment));
        }

        return (rendered.Count > 0 ? "FROM " + string.Join(',', rendered) : "");
    }

    protected override string _RenderJoinSentence() {
        List<string> rendered = new List<string>();
        foreach (Join<Query> queryFragment in this.QueryJoin) {
            rendered.Add(this.RenderJoin(queryFragment));
        }

        return (rendered.Count > 0 ? "JOIN " + string.Join(',', rendered) : "");
    }

    protected override string _RenderWhereSentence() {
        List<string> rendered = new List<string>();
        foreach (Where<Query> queryFragment in this.QueryWhere) {
            if (rendered.Any()) {
                rendered.Add(queryFragment.Operator.GetDescription());
            }
            rendered.Add(this.RenderWhere(queryFragment));
        }
        foreach (WhereIn<Query> queryFragment in this.QueryWhereIn) {
            if (rendered.Any()) {
                rendered.Add(queryFragment.Operator.GetDescription());
            }
            rendered.Add(this.RenderWhereIn(queryFragment));
        }

        return (rendered.Count > 0 ? "WHERE " + string.Join(' ', rendered) : "");
    }

    protected override string _RenderGroupSentence() {
        List<string> rendered = new List<string>();

        foreach (GroupBy queryFragment in this.QueryGroup) {
            rendered.Add(this.RenderGroupBy(queryFragment));
        }

        return (rendered.Count > 0 ? "GROUP BY " + string.Join(',', rendered) : "");
    }

    protected override string _RenderHavingSentence() {
        List<string> rendered = new List<string>();

        foreach (Where<Query> queryFragment in this.QueryHaving) {
            rendered.Add(this.RenderWhere(queryFragment));
        }

        return (rendered.Count > 0 ? "HAVING " + string.Join(',', rendered) : "");
    }

    protected override string _RenderOrderSentence() {
        List<string> rendered = new List<string>();

        if (this.QueryOrder != null) {
            foreach (OrderBy queryOrderItem in this.QueryOrder) {
                List<string> renderedSubset = new List<string>();

                renderedSubset.Add(queryOrderItem.Field.Render());

                if (queryOrderItem.Direction != OrderDirection.NONE) {
                    renderedSubset.Add(queryOrderItem.Direction.GetDescription());
                }

                rendered.Add(string.Join(' ', renderedSubset));
            }
        }

        return (rendered.Count > 0 ? "ORDER BY " + string.Join(',', rendered) : "");
    }

    protected override string _RenderLimitSentence() {
        List<string> rendered = new List<string>();
        if (this.QueryLimit != null) {
            if (this.QueryLimit.Offset >= 0) {
                rendered.Add(this.QueryLimit.Offset.ToString());
            }
            if (this.QueryLimit.Count > 0) {
                rendered.Add(this.QueryLimit.Count.ToString());
            }
        }

        return (rendered.Count > 0 ? "LIMIT " + string.Join(',', rendered) : "");
    }


    protected override string _RenderDeleteSentence() {
        From<Query> from = this.QueryFrom.FirstOrDefault();

        if (from != null) {
            return $"DELETE FROM {from.Table}{(!string.IsNullOrWhiteSpace(from.TableAlias) ? $" AS {from.TableAlias}" : "")}";
        }

        return string.Empty;
    }
    protected override string _RenderUpdateSentence() { 
        From<Query> from = this.QueryFrom.FirstOrDefault();

        if (from != null) {
            return $"UPDATE {this.RenderFrom(from)}";
        }

        return string.Empty;
    }

    protected override string _RenderSetSentence() {
        List<string> rendered = new List<string>();

        if (this.QueryOrder != null) {
            foreach (Where<Query> querySetItem in this.QuerySet) {
                querySetItem.Comparer = WhereComparer.EQUALS;

                rendered.Add(this.RenderWhere(querySetItem));
            }
        }

        return (rendered.Count > 0 ? "SET " + string.Join(',', rendered) : "");
    }

    protected override string _RenderInsertIntoSentence() { 
        From<Query> from = this.QueryFrom.FirstOrDefault();

        if (from != null) {
            return $"INSERT INTO {from.Table} ({string.Join(',', this.QueryColumns)})";
        }

        return string.Empty;
    }

    protected override string _RenderInsertValuesSentence() {
        List<string> rendered = new List<string>();

        if (this.QueryValues != null) {
            foreach (Dictionary<string, dynamic> queryValue in QueryValues) {
                List<string> toRender = new List<string>();

                // In order to get a valid query, insert the values in the same column order
                foreach (string queryColumn in this.QueryColumns) {
                    if (queryValue.ContainsKey(queryColumn) && queryValue[queryColumn] != null) {
                        toRender.Add(this.PrepareQueryValue(queryValue[queryColumn], true));
                    }
                    else {
                        toRender.Add("NULL");
                    }
                }

                rendered.Add($"({string.Join(",", toRender)})");
            }
        }

        return (rendered.Count > 0 ? "VALUES " + string.Join(',', rendered) : "");
    }


    protected override string _RenderCreateSentence<T>() {
        return this._RenderCreateSentence(typeof(T));
    }

    protected override string _RenderCreateSentence(Type tableType) {
        Table typeTable = tableType.GetCustomAttribute<Table>();
        if (typeTable == null) {
            throw new InvalidOperationException("Missing [Table] attribute");
        }

        StringBuilder rendered = new StringBuilder();

        rendered.Append("CREATE ");
        if (typeTable.Temporary) {
            rendered.Append("TEMPORARY ");
        }

        rendered.Append("TABLE ");
        if (typeTable.IfNotExists) {
            rendered.Append("IF NOT EXISTS ");
        }
        rendered.Append($"{Query.FieldDelimiter}{typeTable.Name}{Query.FieldDelimiter} (");

        PropertyInfo[] tableProperties = tableType.GetProperties();
        FieldInfo   [] tableFields     = tableType.GetFields();

        IEnumerable<string?> tableEntries = tableProperties.Select(tableProperty => {
            return this.__GetColumnDefinition(tableProperty, tableProperty.GetCustomAttribute<Column>());
        }).Where(renderedColumn => renderedColumn != null);
        IEnumerable<string?> tableKeys = tableType.GetCustomAttributes<Key>().Select(tableKey => {
            return this.__GetKeyDefinition(tableKey);
        }).Where(renderedColumn => renderedColumn != null);

        rendered.Append(string.Join(",", tableEntries.Concat(tableKeys ?? Enumerable.Empty<string>())));
        rendered.Append(")");

        // Table options
        var tableOptions = tableType.GetCustomAttributes<TableOption>();
        foreach (TableOption tableOption in tableOptions) {
            rendered.Append($" {tableOption.Name}={tableOption.Value}");
        }

        return rendered.ToString();
    }

    private string? __GetKeyDefinition(Key tableKey) {
        if (tableKey == null || tableKey.KeyType == KeyType.PRIMARY) {
            return null;
        }

        string keyName = tableKey.Name ?? string.Join("_", tableKey.Fields);

        switch (tableKey.KeyType) {
            case KeyType.NONE:
                return 
                    $"KEY {Query.FieldDelimiter}{tableKey.Name}{Query.FieldDelimiter} " + 
                    $"({string.Join(", ", tableKey.Fields.Select(field => $"{Query.FieldDelimiter}{field}{Query.FieldDelimiter}"))}) " + 
                    $"USING {tableKey.IndexType}"
                ;
            case KeyType.PRIMARY:
            case KeyType.UNIQUE:
                return
                    $"{tableKey.KeyType.GetDescription()} KEY {Query.FieldDelimiter}pk_{tableKey.Name}{Query.FieldDelimiter} " +
                    $"({string.Join(", ", tableKey.Fields.Select(field => $"{Query.FieldDelimiter}{field}{Query.FieldDelimiter}"))})"
                ;
            case KeyType.FOREIGN:
                StringBuilder fkBuilder = new StringBuilder();

                fkBuilder.Append($"CONSTRAINT {Query.FieldDelimiter}fk_{tableKey.Name}{Query.FieldDelimiter} ");
                fkBuilder.Append($"REFERENCES {Query.FieldDelimiter}{tableKey.References.Table}{Query.FieldDelimiter}");
                fkBuilder.Append($"({Query.FieldDelimiter}{tableKey.References.Field}{Query.FieldDelimiter}) ");
                if (!string.IsNullOrWhiteSpace(tableKey.OnDelete)) {
                    fkBuilder.Append($"ON DELETE {tableKey.OnDelete}");
                }
                if (!string.IsNullOrWhiteSpace(tableKey.OnUpdate)) {
                    fkBuilder.Append($"ON UPDATE {tableKey.OnUpdate}");
                }

                return fkBuilder.ToString();
        }

        return null;
    }

    private string? __GetColumnDefinition(PropertyInfo property, Column tableColumn) {
        if (tableColumn == null) {
            return null;
        }

        Type columnType = property.PropertyType;
        if (Nullable.GetUnderlyingType(columnType) != null) {
            columnType = Nullable.GetUnderlyingType(columnType);
        }

        StringBuilder columnBuilder = new StringBuilder($"{Query.FieldDelimiter}{tableColumn.Name}{Query.FieldDelimiter} {tableColumn.DataType}");
        if (tableColumn.Length > 0)
            columnBuilder.Append($" ({tableColumn.Length}{(tableColumn.Precision > 0 ? $",{tableColumn.Precision}" : "")})");
        if (columnType.IsEnum) {

            List<string> enumValues = new List<string>();

			bool first = true;
            foreach (Enum enumValue in Enum.GetValues(columnType)) {
                if (first) {
                    first = false;
                    continue;
                }
                string enumValueString = enumValue.ToString();

                if (!string.IsNullOrWhiteSpace(enumValue.GetDescription())) {
                    enumValues.Add(this.__RenderWhereValue(enumValue.GetDescription(), true));
                }
                else {
                    enumValues.Add(this.__RenderWhereValue(enumValue.ToString(), true));
                }
            }

            columnBuilder.Append($" CHECK({Query.FieldDelimiter}{tableColumn.Name}{Query.FieldDelimiter} IN({string.Join(',', enumValues)}))");
        }

        if (tableColumn.PrimaryKey)
            columnBuilder.Append(" PRIMARY KEY");
		if (tableColumn.AutoIncrement)
			columnBuilder.Append(" AUTOINCREMENT");
		if (tableColumn.NotNull)
            columnBuilder.Append(" NOT NULL");
        if (tableColumn.Unique && !tableColumn.PrimaryKey)
            columnBuilder.Append(" UNIQUE");
        if (tableColumn.Default != null)
            columnBuilder.Append($" DEFAULT {tableColumn.Default}");
        if (tableColumn.Comment != null)
            columnBuilder.Append($" COMMENT '{tableColumn.Comment}'");
        return columnBuilder.ToString();
    }

    protected override string _RenderSelectExtraSentence() {
        return string.Empty;
    }
    #endregion

    #endregion

    #region Helper functions
    public string __RenderWhereValue(dynamic value, bool escape) {
        if (value is string
            ||
            value is DateTime
        ) {
            if (escape) {
                return $"{ValueDelimiter}{value}{ValueDelimiter}";
            }
        }
        if (value is Enum) {
			return $"{ValueDelimiter}{((Enum)value).GetDescription()}{ValueDelimiter}";
		}

		return value.ToString();
    }
    #endregion
}
