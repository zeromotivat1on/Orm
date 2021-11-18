using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using ORM.Utils;
using ORM.Core.SqlQuery;
using ORM.Repository;

namespace ORM.Core
{
    public sealed class ModelSet<T> : IBaseRepository<T> where T : class, new()
    {
        public delegate void ModelSetHandler(object sender, ModelSetEventArgs e);
        public event ModelSetHandler DbChangesEvent;
        private string connectionString;

        public ModelSet(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string ConnectionString { get { return this.connectionString; } }

        public T this[int id]
        {
            get
            {
                return this.Get(id);
            }
            set
            {
                typeof(T).GetProperties()[0].SetValue(value, id);
                this.Update(value);
            }
        }

        #region Sql Commands
        public void CreateTable()
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                var builder = new SqlQueryBuilder<T>();
                var createCommand = new SqlCommand(builder.QueryCreateTable(), connection);
                Console.WriteLine(createCommand.CommandText);
                createCommand.ExecuteNonQuery();
                this.DbChangesEvent?.Invoke(
                    this,
                    new ModelSetEventArgs($"Table [{builder.TableName}] for model of type [{builder.ModelType.Name}] was created"));
            }
        }

        public void Add(T model)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var commandBuilder = new SqlCommandBuilder<T>(connection);
                var addCommand = commandBuilder.CommandAdd(model);
                Console.WriteLine(addCommand.CommandText);
                connection.Open();
                int modelId = (int)addCommand.ExecuteScalar();
                commandBuilder.SqlQueryBuilder.ModelIdProp.SetValue(model, modelId);
                this.DbChangesEvent?.Invoke(
                    this,
                    new ModelSetEventArgs(
                        $"Model of type [{commandBuilder.SqlQueryBuilder.ModelType.Name}] has been added to db",
                        model));
            }
        }

        public void Update(T newModel)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var commandBuilder = new SqlCommandBuilder<T>(connection);
                var updateCommand = commandBuilder.CommandUpdate(newModel);
                Console.WriteLine(updateCommand.CommandText);
                if (!RecordExists(commandBuilder.SqlQueryBuilder))
                {
                    CustomErrors.RecordNotFound(commandBuilder.SqlQueryBuilder.ModelId);
                    return;
                }

                connection.Open();
                updateCommand.ExecuteNonQuery();
                this.DbChangesEvent?.Invoke(
                    this,
                    new ModelSetEventArgs(
                        $"Model of type [{commandBuilder.SqlQueryBuilder.ModelType.Name}] has been updated in db", 
                        commandBuilder.SqlQueryBuilder.Model));
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var commandBuilder = new SqlCommandBuilder<T>(connection);
                var deleteCommand = commandBuilder.CommandDelete(id);
                Console.WriteLine(deleteCommand.CommandText);
                if (!RecordExists(commandBuilder.SqlQueryBuilder))
                {
                    CustomErrors.RecordNotFound(commandBuilder.SqlQueryBuilder.ModelId);
                    return;
                }

                connection.Open();
                deleteCommand.ExecuteNonQuery();
                this.DbChangesEvent?.Invoke(
                    this,
                    new ModelSetEventArgs(
                        $"Model of type [{commandBuilder.SqlQueryBuilder.ModelType.Name}] has been deleted from db", 
                        commandBuilder.SqlQueryBuilder.ModelId));
            }
        }

        public T Get(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var commandBuilder = new SqlCommandBuilder<T>(connection);
                var getCommand = commandBuilder.CommandGet(id);
                Console.WriteLine(getCommand.CommandText);
                if (!RecordExists(commandBuilder.SqlQueryBuilder))
                {
                    CustomErrors.RecordNotFound(commandBuilder.SqlQueryBuilder.ModelId);
                    return null;
                }

                connection.Open();
                var reader = getCommand.ExecuteReader();
                var retreivedPropsValues = new List<object>();
                if (reader.Read())
                {
                    for(int i = 0; i < commandBuilder.SqlQueryBuilder.RestrictsByConvertiblePropLen; ++i)
                    {
                        var readerValue = reader.GetValue(i);
                        var readerValueType = readerValue.GetType();
                        if (readerValueType == typeof(DBNull))
                        {
                            retreivedPropsValues.Add(readerValueType.IsNumeric() ? 0 : null);
                        }
                        else
                        {
                            retreivedPropsValues.Add(reader.GetValue(i));
                        }
                    }
                }

                var retreivedModel = (T)Activator.CreateInstance(commandBuilder.SqlQueryBuilder.ModelType);
                for(int i = 0; i < commandBuilder.SqlQueryBuilder.RestrictsByConvertiblePropLen; ++i)
                {
                    commandBuilder.SqlQueryBuilder.RestrictsByConvertibleProp.Keys.ToList()[i].SetValue(retreivedModel, retreivedPropsValues[i]);
                }

                this.DbChangesEvent?.Invoke(
                    this,
                    new ModelSetEventArgs($"Model of type [{commandBuilder.SqlQueryBuilder.ModelType.Name}] has been obtained from db", retreivedModel));

                return retreivedModel;
            }
        }

        public List<T> GetAll()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var commandBuilder = new SqlCommandBuilder<T>(connection);
                var sql = $"SELECT * FROM [dbo].[{commandBuilder.SqlQueryBuilder.TableName}]";
                var getAllCommand = new SqlCommand(sql, connection);

                connection.Open();
                var reader = getAllCommand.ExecuteReader();
                var ids = new List<int>();
                while (reader.Read())
                {
                    ids.Add((int)reader.GetValue(0));
                }

                var result = new List<T>();
                foreach(int id in ids)
                {
                    result.Add(Get(id));
                }

                return result;
            }
        }
        #endregion

        #region Table Record Validation
        private bool RecordExists(SqlQueryBuilder<T> builder)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = $"SELECT * FROM [dbo].[{builder.TableName}] WHERE [{builder.ModelIdProp.Name}] = {builder.ModelId}";
                var getCommand = new SqlCommand(sql, connection);
                object obtainedModelId = getCommand.ExecuteScalar();
                if (obtainedModelId != null &&
                    (int)obtainedModelId == builder.ModelId)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
