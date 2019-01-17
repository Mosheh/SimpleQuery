using SimpleQuery.Domain.Data;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    public class ScriptSqlServerBuilder : ScriptCommon, IScriptBuilder
    {
        public Domain.Data.DbServerType DbServerType => Domain.Data.DbServerType.SqlServer;

        public IDataReader ExecuteReader(string commandText, IDbConnection dbConnection, IDbTransaction dbTransaction = null)
        {
            var command = dbConnection.CreateCommand();
            if (dbTransaction != null) command.Transaction = dbTransaction;

            command.CommandText = commandText;

            var wasClosed = dbConnection.State == ConnectionState.Closed;
            if (wasClosed) dbConnection.Open();
            var reader = command.ExecuteReader();

            if (wasClosed) dbConnection.Close();

            return reader;
        }

        public void Execute(string commandText, IDbConnection dbConnection, IDbTransaction dbTransaction = null)
        {
            var command = dbConnection.CreateCommand();
            if (dbTransaction != null)
                command.Transaction = dbTransaction;
            command.CommandText = commandText;

            var wasClosed = dbConnection.State == ConnectionState.Closed;
            if (wasClosed) dbConnection.Open();
            var rowsCount = command.ExecuteNonQuery();
            var dataTable = new DataTable();
            Console.WriteLine($"{rowsCount} affected rows");
            if (wasClosed) dbConnection.Close();
        }

        public string GetDeleteCommand<T>(T obj, object key) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyName = GetKeyProperty(allProperties);
            if (keyName == null)
                throw new Exception($"Key column not found for {entityName}");

            if (key is string)
                key = $"'{key}'";

            var sql = $"delete [{entityName}] where Id={key}";
            return sql;
        }

        public string GetInsertCommand<T>(T obj, bool includeKey = false) where T : class, new()
        {
            var allProperties = GetValidProperty<T>();
            var entityName = obj.GetType().Name;

            var keyName = GetKeyProperty(allProperties.ToArray<PropertyInfo>());
            if (keyName == null && includeKey)
                throw new Exception($"Key column not found for {entityName}");

            var strBuilderSql = new StringBuilder($"insert into [{entityName}] (");
            foreach (var item in allProperties)
            {
                if (keyName == item && !includeKey)
                    continue;

                strBuilderSql.Append($"[{item.Name}]");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(") values (");
            }

            foreach (var item in allProperties)
            {
                if (keyName == item && !includeKey)
                    continue;

                strBuilderSql.Append($"{DataFormatter.GetValue(item, obj, this.DbServerType)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(")");
            }

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            var reader = ExecuteReader("SELECT SCOPE_IDENTITY()", dbConnection, transaction);
            if (reader.Read())
            {
                var value = reader.GetDecimal(0);
                reader.Close();
                return value;
            }
            else
            {
                reader.Close();
                return 0;
            }
        }

        public string GetSelectCommand<T>(T obj) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var strBuilderSql = new StringBuilder($"select ");
            foreach (var item in allProperties)
            {
                strBuilderSql.Append($"[{item.Name}]");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append($" from [{entityName}]");
            }

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public string GetUpdateCommand<T>(T obj) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyProperty = GetKeyProperty(allProperties);
            if (keyProperty == null)
                throw new Exception($"Key column not found for {entityName}");

            var strBuilderSql = new StringBuilder($"update [{entityName}] set ");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    continue;

                strBuilderSql.Append($"[{item.Name}]={DataFormatter.GetValue(item, obj, this.DbServerType)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
            }
            strBuilderSql.Append($" where {keyProperty.Name}={DataFormatter.GetValue(keyProperty, obj, this.DbServerType)}");

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public string GetCreateTableCommand<T>(T obj) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyProperty = GetKeyProperty(allProperties);

            var strBuilderSql = new StringBuilder($"create table [{entityName}] (");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    strBuilderSql.Append($"[{item.Name}] {GetTypeSqlServer(item)} identity");
                else
                    strBuilderSql.Append($"[{item.Name}] {GetTypeSqlServer(item)}");

                if (item != allProperties.Last())
                {
                    strBuilderSql.Append(", ");
                }
                else
                {
                    if (keyProperty != null && keyProperty.PropertyType.Name.Equals("Int32"))
                        strBuilderSql.Append($", primary key ([{keyProperty.Name}])");
                    strBuilderSql.Append(")");
                }
            }


            var sql = strBuilderSql.ToString();
            return sql;
        }

        private string GetTypeSqlServer(PropertyInfo item)
        {
            switch (item.PropertyType.Name)
            {
                case "String":
                    return "nvarchar(255)";
                case "Int32":
                    return "int not null";
                case "Int64":
                    return "bigint";
                case "Byte[]":
                    return "varbinary";
                case "Boolean":
                    return "bit";
                case "Decimal":
                    return "decimal(18,6)";
                case "Double":
                    return "float";
                case "DateTime":
                    return "DateTime";
                case "Nullable`1":
                    if (item.PropertyType.AssemblyQualifiedName.Contains("System.Int32"))
                        return "int";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Boolean"))
                        return "bit";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Decimal"))
                        return "decimal(18,6)";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.DateTime"))
                        return "DateTime null";
                    else
                        return "int not null";
                default:
                    return "nvarchar(255)";
            }
        }

        public object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null, string sequenceName = null)
        {
            return GetLastId<T>(model, dbConnection, transaction);
        }

        public string GetWhereCommand<T>(Expression<Func<T, bool>> expression, T instance) where T: class, new()
        {
            throw new NotImplementedException();
        }
    }
}
