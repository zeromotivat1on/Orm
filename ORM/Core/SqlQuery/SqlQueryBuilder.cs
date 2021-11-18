using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ORM.CustomAttributes;
using ORM.Utils;

namespace ORM.Core.SqlQuery
{
    public class SqlQueryBuilder<T> where T: class, new()
    {
        private T model;
        private int modelId;

        public SqlQueryBuilder()
        {
            this.ModelType = typeof(T);
            this.TableName = this.ModelType.Name.ToConventionTableName();
            this.SqlQuerySB = new SqlQuerySB();
            this.QueryConstraints = new List<string>();
            this.QueryRestrictsByConvertibleProp = new Dictionary<PropertyInfo, List<string>>();
            this.QueryPropByParameter = new Dictionary<string, PropertyInfo>();
            this.QueryPropValueByParameter = new Dictionary<string, object>();
            this.SetModelConcernedProps();
        }

        public SqlQueryBuilder(int id) : this()
        {
            this.ModelId = id;
        }

        public SqlQueryBuilder(T model) : this()
        {
            this.Model = model;

        }

        public SqlQuerySB SqlQuerySB { get; private set; }

        #region Model Concerned Props
        public T Model { 
            get 
            {
                return this.model;
            } 
            set 
            {
                this.model = value;
                this.SetModelConcernedProps();
            }
        }
        public int ModelId {
            get 
            {
                return this.modelId;
            }
            set 
            {
                if (this.modelId != default(int))
                {
                    throw new ArgumentException($"Model already has id = [{this.modelId}]");
                }

                if (value == default(int) || value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Attempt to set [{nameof(this.modelId)}] with default or unexpected value [{default(int)}]"
                    );
                }

                this.modelId = value;
            } 
        }
        public PropertyInfo ModelIdProp { get; private set; }
        public Type ModelType { get; private set; }
        public string TableName { get; private set; }
        #endregion

        #region Collections Concerned Props
        public List<string> QueryConstraints { get; private set; }
        public Dictionary<PropertyInfo, List<string>> RestrictsByConvertibleProp { get; private set; }
        public int RestrictsByConvertiblePropLen { get; private set; }
        public List<PropertyInfo> ConvertibleModelProps { get; private set; }
        public Dictionary<PropertyInfo, List<string>> QueryRestrictsByConvertibleProp { get; private set; }
        public List<PropertyInfo> QueryModelProps { get; private set; }
        public int QueryRestrictsByConvertiblePropLen { get; private set; }
        public List<PropertyInfo> ForeignCollectionProps { get; private set; }
        public PropertyInfo ForeignCollectionPropOfModelTypes { get; private set; }
        public Dictionary<string, PropertyInfo> QueryPropByParameter { get; private set; }
        public Dictionary<string, object> QueryPropValueByParameter { get; private set; }
        #endregion

        #region Sql Queries
        public string QueryCreateTable()
        {
            var sqlSb = new SqlQuerySB();
            sqlSb.Add($"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{this.TableName}' AND xtype='U')\n");
            sqlSb.Add($"CREATE TABLE [dbo].[{this.TableName}]");
            sqlSb.Space();
            sqlSb.OpenBrackets();
            foreach (var pair in this.RestrictsByConvertibleProp)
            {
                sqlSb.AddTableColumn(pair);
            }

            TryAddForeignKey();
            sqlSb.AddConstraints(this.QueryConstraints);
            sqlSb.CloseBrackets();
            return sqlSb.SqlQuery;
        }

        public string QueryAdd()
        {
            this.SqlQuerySB.Clear();
            List<PropertyInfo> propsWithoutKeys = this.RemovePrimaryKeyFrom(this.QueryModelProps);
            List<object> modelValues = this.GetValuesFrom(propsWithoutKeys);
            int propsWithoutKeysCount = propsWithoutKeys.Count;
            for (int i = 0; i < propsWithoutKeysCount; ++i)
            {
                var paramName = $"@{propsWithoutKeys[i].Name}";
                this.QueryPropByParameter.Add(paramName, propsWithoutKeys[i]);
                this.QueryPropValueByParameter.Add(paramName, modelValues[i]);
            }

            this.SqlQuerySB.Add($"INSERT INTO [dbo].[{this.TableName}]");
            this.SqlQuerySB.Space();
            this.SqlQuerySB.AddTableColumnNames(this.QueryPropByParameter.Values.ToList());
            this.SqlQuerySB.Add("\nVALUES");
            this.SqlQuerySB.Space();
            this.SqlQuerySB.AddParams(this.QueryPropValueByParameter.Keys.ToList());
            this.SqlQuerySB.Add($";\nSELECT CAST(SCOPE_IDENTITY() AS INT);");
            return this.SqlQuerySB.SqlQuery;
        }

        public string QueryUpdate()
        {
            this.SqlQuerySB.Clear();
            List<PropertyInfo> propsWithoutKeys = this.RemovePrimaryKeyFrom(this.QueryModelProps);
            List<PropertyInfo> propsWithoutDefaults = this.RemoveDefaultsFrom(propsWithoutKeys);
            List<object> modelValues = this.GetValuesFrom(propsWithoutDefaults);
            var dictionary = new Dictionary<string, object>();
            string paramName = "";
            int propsWithoutDefaultsCount = propsWithoutDefaults.Count;
            for (int i = 0; i < propsWithoutDefaultsCount; ++i)
            {
                paramName = $"@{propsWithoutDefaults[i].Name}";
                this.QueryPropByParameter.Add(paramName, propsWithoutDefaults[i]);
                this.QueryPropValueByParameter.Add(paramName, modelValues[i]);
                dictionary.Add(propsWithoutDefaults[i].Name, modelValues[i]);
            }

            this.SqlQuerySB.Add($"UPDATE [dbo].[{this.TableName}] SET\n");
            this.SqlQuerySB.AddForUpdate(this.QueryPropByParameter);
            paramName = $"@{this.ModelIdProp.Name}";
            this.QueryPropByParameter.Add(paramName, this.ModelIdProp);
            this.QueryPropValueByParameter.Add(paramName, this.ModelId);
            this.SqlQuerySB.Add($"\nWHERE [{this.ModelIdProp.Name}] = {paramName}");
            return this.SqlQuerySB.SqlQuery;
        }

        public string QueryDelete()
        {
            this.SqlQuerySB.Clear();
            var paramName = $"@{this.ModelIdProp.Name}";
            this.QueryPropByParameter.Add(paramName, this.ModelIdProp);
            this.QueryPropValueByParameter.Add(paramName, this.ModelId);
            this.SqlQuerySB.Add($"DELETE FROM [dbo].[{this.TableName}] WHERE [{this.ModelIdProp.Name}] = {paramName}");
            return this.SqlQuerySB.SqlQuery;
        }

        public string QueryGetById()
        {
            this.SqlQuerySB.Clear();
            var paramName = $"@{this.ModelIdProp.Name}";
            this.QueryPropByParameter.Add(paramName, this.ModelIdProp);
            this.QueryPropValueByParameter.Add(paramName, this.ModelId);
            this.SqlQuerySB.Add($"SELECT * FROM [dbo].[{this.TableName}] WHERE [{this.ModelIdProp.Name}] = {paramName}");
            return this.SqlQuerySB.SqlQuery;
        }
        #endregion

        private void SetModelConcernedProps()
        {
            this.SetRestrictsByConvertibleProp();
            this.ModelIdProp = this.GetModelIdProp();
            this.ConvertibleModelProps = this.RestrictsByConvertibleProp.Keys.ToList();
            this.RestrictsByConvertiblePropLen = this.RestrictsByConvertibleProp.Count;
            this.SetQueryRestrictsByConvertibleProp();
            this.QueryRestrictsByConvertiblePropLen = this.QueryRestrictsByConvertibleProp.Count;
            this.QueryModelProps = this.QueryRestrictsByConvertibleProp.Keys.ToList();

            if(this.model != null)
            {
                int modelId = (int)this.ModelIdProp.GetValue(this.model);
                if (modelId > 0)
                {
                    this.ModelId = modelId;
                }
            }
        }

        #region Prop Restricts Logic
        private void SetRestrictsByConvertibleProp()
        {
            this.RestrictsByConvertibleProp = new Dictionary<PropertyInfo, List<string>>();
            var props = this.ModelType.GetProperties();
            var propsLen = props.Length;
            for(int i = 0; i < propsLen; ++i)
            {
                if (props[i].PropertyType.IsConvertibleToSql() && props[i].GetCustomAttribute<IgnoreAttribute>() == null)
                {
                    this.RestrictsByConvertibleProp.Add(props[i], this.TryGetPropertyRestrictions(props[i]));
                }
            }

        }

        private void SetQueryRestrictsByConvertibleProp()
        {
            this.QueryRestrictsByConvertibleProp = new Dictionary<PropertyInfo, List<string>>();
            foreach (var pair in this.RestrictsByConvertibleProp)
            {
                if (this.NoPropInQueryConstraints(pair.Key))
                {
                    this.QueryRestrictsByConvertibleProp.Add(pair.Key, pair.Value);
                }
            }
        }

        private List<string> TryGetPropertyRestrictions(PropertyInfo prop)
        {
            var restricts = new List<string>();

            if (prop.GetCustomAttribute<RangeAttribute>() != null &&
                TypeManager.IsNumericType(prop.PropertyType))
            {
                var attribute = prop.GetCustomAttribute<RangeAttribute>();
                restricts.Add($"CHECK([{prop.Name}] BETWEEN {attribute.Minimum} AND {attribute.Maximum})");
            }
            else if (prop.GetCustomAttribute<MaxLengthAttribute>() != null &&
                (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(Array)))
            {
                restricts.Add($"({prop.GetCustomAttribute<MaxLengthAttribute>().Length})");
            }
            else if (prop.GetCustomAttribute<StringLengthAttribute>() != null &&
                prop.PropertyType == typeof(string))
            {
                var attribute = prop.GetCustomAttribute<StringLengthAttribute>();
                restricts.Add($"({attribute.MinimumLength}, {attribute.MaximumLength})");
            }

            if (prop.GetCustomAttribute<RequiredAttribute>() != null)
            {
                restricts.Add("NOT NULL");
            }

            if (prop.GetCustomAttribute<UniqueAttribute>() != null)
            {
                restricts.Add("UNIQUE");
            }

            if (prop.GetCustomAttribute<KeyAttribute>() != null)
            {
                restricts.Add("IDENTITY(1,1) NOT NULL");
                this.QueryConstraints.Add($"CONSTRAINT [PK_{this.TableName}] PRIMARY KEY CLUSTERED([{prop.Name}] ASC)");
            }
            else if (prop.GetCustomAttribute<ForeignKeyAttribute>() != null)
            {
                restricts.Add("NOT NULL");
            }

            return restricts;
        }
        #endregion

        #region Foreign Key Logic
        private void TryAddForeignKey()
        {
            var navProps = this.TryGetNavigationalProps();
            if (navProps.Count <= 0)
            {
                return;
            }

            var foreignKeyByNavProp = new Dictionary<PropertyInfo, PropertyInfo>();
            foreach (var prop in navProps)
            {
                var foreignKey = this.TryGetForeignKey(prop);
                if(foreignKey != null)
                {
                    foreignKeyByNavProp.Add(prop, foreignKey);
                }
            }

            foreach (var pair in foreignKeyByNavProp)
            {
                this.TryAddForeignKeyConstraint(pair);
            }
        }

        private void TryAddForeignKeyConstraint(KeyValuePair<PropertyInfo, PropertyInfo> navPropForeignKey)
        {
            var navProp = navPropForeignKey.Key;
            var foreignKeyProp = navPropForeignKey.Value;
            var foreignTableName = $"{navProp.Name.ToConventionTableName()}";
            var collectionProps = navProp.PropertyType
                .GetProperties()
                .Where(prop => prop.PropertyType.GetInterface(nameof(ICollection<int>)) != null)
                .ToList();
            this.ForeignCollectionProps = collectionProps;
            foreach (var collectionProp in collectionProps)
            {
                var modelTypeFromGeneric = collectionProp.PropertyType.GetGenericArguments()[0];
                if (modelTypeFromGeneric == this.ModelType)
                {
                    this.ForeignCollectionPropOfModelTypes = collectionProp;
                    this.QueryConstraints.Add(
                        $"CONSTRAINT [FK_{this.TableName}_{foreignTableName}_{foreignKeyProp.Name}] " +
                        $"FOREIGN KEY ([{foreignKeyProp.Name}]) " +
                        $"REFERENCES [dbo].[{foreignTableName}] ([{navProp.PropertyType.GetProperties()[0].Name}]) " +
                        $"ON DELETE CASCADE");
                }
            }
        }

        private List<PropertyInfo> TryGetNavigationalProps()
        {
            var navProps = new List<PropertyInfo>();
            foreach (var prop in this.ModelType.GetProperties())
            {
                if (prop.PropertyType.IsUserDefinedClass())
                {
                    navProps.Add(prop);
                }
            }

            return navProps;
        }

        private PropertyInfo TryGetForeignKey(PropertyInfo navProp)
        {
            var foreignKey = $"{navProp.Name}Id";
            return this.ModelType.GetProperty(foreignKey);
        }
        #endregion

        #region Utils
        private List<object> GetValuesFrom(List<PropertyInfo> props)
        {
            var propVals = new List<object>();
            foreach (var prop in props)
            {
                propVals.Add(prop.GetValue(this.Model));
            }

            return propVals;
        }

        private List<PropertyInfo> RemovePrimaryKeyFrom(List<PropertyInfo> props)
        {
            var copy = new List<PropertyInfo>();
            foreach (var prop in props)
            {
                if (prop.Name.Contains("Id") &&
                    prop.GetCustomAttribute(typeof(KeyAttribute)) != null)
                {
                    continue;
                }

                copy.Add(prop);
            }

            return copy;
        }

        private List<PropertyInfo> RemoveDefaultsFrom(List<PropertyInfo> props)
        {
            var copy = new List<PropertyInfo>();
            foreach (var prop in props)
            {
                if (prop.GetValue(this.Model).Equals(prop.PropertyType.DefaultValue()))
                {
                    continue;
                }

                copy.Add(prop);
            }

            return copy;
        }

        private bool NoPropInQueryConstraints(PropertyInfo prop)
        {
            bool noProp = false;
            foreach(var constraint in this.QueryConstraints)
            {
                if (!constraint.Contains(prop.Name))
                {
                    noProp = true;
                }
            }

            return noProp;
        }

        private PropertyInfo GetModelIdProp()
        {
            foreach (var pair in this.RestrictsByConvertibleProp)
            {
                if (pair.Key.GetCustomAttribute(typeof(KeyAttribute)) != null)
                {
                    return pair.Key;
                }
            }

            return null;
        }
        #endregion
    }
}
