using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using ORM.Utils;

namespace ORM.Core.SqlQuery
{
    public class SqlQuerySB
    {
        private StringBuilder sqlQuerySb;

        public SqlQuerySB()
        {
            this.sqlQuerySb = new StringBuilder();
        }

        public string SqlQuery { get { return this.sqlQuerySb.ToString(); } }

        public void Clear()
        {
            this.sqlQuerySb.Clear();
        }

        public void Add(string query)
        {
            this.sqlQuerySb.Append(query);
        }

        public void Add(KeyValuePair<string, string> queryPair)
        {
            this.sqlQuerySb.Append($"\t[{queryPair.Key}] {queryPair.Value},\n");
        }

        public void Add<T>(List<T> values)
        {
            this.OpenBrackets();
            this.AddNoBrackets(values);
            this.CloseBrackets();
        }

        public void AddNoBrackets<T>(List<T> values)
        {
            foreach (var value in values)
            {
                this.sqlQuerySb.Append($"\t'{value}',\n");
            }
        }

        public void AddParams<T>(List<T> values)
        {
            this.OpenBrackets();
            foreach (var value in values)
            {
                this.sqlQuerySb.Append($"\t{value},\n");
            }
            this.CloseBrackets();
        }

        public void AddTableColumn(KeyValuePair<PropertyInfo, List<string>> pair)
        {
            this.sqlQuerySb.Append($"\t[{pair.Key.Name}] {pair.Key.PropertyType.ToSqlType()} {String.Join(" ", pair.Value)},\n");
        }

        public void AddConstraints(List<string> constraints)
        {
            foreach (var constraint in constraints)
            {
                this.sqlQuerySb.Append($"\t{constraint},\n");
            }
        }

        public void AddTableColumnNames<T>(List<T> columns)
        {
            this.OpenBrackets();
            foreach (var column in columns)
            {
                this.sqlQuerySb.Append($"\t{column},\n");
            }

            this.CloseBrackets();
        }

        public void AddTableColumnNames(List<PropertyInfo> columns)
        {
            this.OpenBrackets();
            foreach (var column in columns)
            {
                this.sqlQuerySb.Append($"\t{column.Name},\n");
            }

            this.CloseBrackets();
        }

        public void AddForUpdate(Dictionary<string, object> values)
        {
            foreach (var pair in values)
            {
                this.sqlQuerySb.Append($"\t[{pair.Key}] = '{pair.Value}',\n");
            }

            this.TryRemoveComma();
        }

        public void AddForUpdate(Dictionary<string, PropertyInfo> values)
        {
            foreach (var pair in values)
            {
                this.sqlQuerySb.Append($"\t[{pair.Value.Name}] = {pair.Key},\n");
            }

            this.TryRemoveComma();
        }

        public void Space()
        {
            this.sqlQuerySb.Append(" ");
        }

        public void OpenBrackets()
        {
            this.sqlQuerySb.Append("(\n");
        }

        public void CloseBrackets()
        {
            this.TryRemoveComma();
            this.sqlQuerySb.Append("\n)");
        }

        private void TryRemoveComma()
        {
            if (this.sqlQuerySb[this.sqlQuerySb.Length - 2] == ',' &&
                (this.sqlQuerySb[this.sqlQuerySb.Length - 1] == ' ' ||
                this.sqlQuerySb[this.sqlQuerySb.Length - 1] == '\n'))
            {
                this.sqlQuerySb.Remove(this.sqlQuerySb.Length - 2, 2);
            }
        }
    }
}
