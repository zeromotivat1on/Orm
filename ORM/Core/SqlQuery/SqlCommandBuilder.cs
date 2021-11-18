using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using ORM.Utils;

namespace ORM.Core.SqlQuery
{
    public class SqlCommandBuilder<T> where T: class, new()
    {
        public SqlCommandBuilder()
        {
            this.SqlCommand = new SqlCommand();
            this.SqlQueryBuilder = new SqlQueryBuilder<T>();
        }

        public SqlCommandBuilder(SqlConnection connection) : this()
        {
            this.SqlCommand.Connection = connection;
        }

        public SqlCommand SqlCommand { get; private set; }
        public SqlQueryBuilder<T> SqlQueryBuilder { get; private set; }

        public SqlCommand CommandAdd(T model)
        {
            this.SqlQueryBuilder.Model = model;
            var sql = this.SqlQueryBuilder.QueryAdd();
            this.SqlCommand.CommandText = sql;
            this.AddParametersWithValues();
            return this.SqlCommand;
        }

        public SqlCommand CommandUpdate(T newModel)
        {
            this.SqlQueryBuilder.Model = newModel;
            var sql = this.SqlQueryBuilder.QueryUpdate();
            this.SqlCommand.CommandText = sql;
            this.AddParametersWithValues();
            return this.SqlCommand;
        }

        public SqlCommand CommandDelete(int id)
        {
            this.SqlQueryBuilder.ModelId = id;
            var sql = this.SqlQueryBuilder.QueryDelete();
            this.SqlCommand.CommandText = sql;
            this.AddParametersWithValues();
            return this.SqlCommand;
        }

        public SqlCommand CommandGet(int id)
        {
            this.SqlQueryBuilder.ModelId = id;
            var sql = this.SqlQueryBuilder.QueryGetById();
            this.SqlCommand.CommandText = sql;
            this.AddParametersWithValues();
            return this.SqlCommand;
        }

        private void AddParametersWithValues()
        {
            this.AddParameters();
            this.AddParametersValues();
        }

        private void AddParameters()
        {
            foreach (var pair in this.SqlQueryBuilder.QueryPropByParameter)
            {
                this.SqlCommand.Parameters.Add(pair.Key, pair.Value.PropertyType.ToSqlType());
            }
        }

        private void AddParametersValues()
        {
            foreach (var pair in this.SqlQueryBuilder.QueryPropValueByParameter)
            {
                Console.WriteLine($"{pair.Key} - {pair.Value}");
                this.SqlCommand.Parameters[pair.Key].Value = pair.Value;
            }
        }
    }
}
