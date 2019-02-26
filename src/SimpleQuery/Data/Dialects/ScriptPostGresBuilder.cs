using SimpleQuery.Data.Linq;
using SimpleQuery.Domain.Data;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    public class ScriptPostGresBuilder : ScriptCommon, IScriptBuilder
    {
        public DbServerType DbServerType => DbServerType.PostGres;

        public IReadOnlyList<DbSimpleParameter> Parameters => throw new NotImplementedException();

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

        public IDataReader ExecuteReader(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            var command = dbConnection.CreateCommand();
            if (transaction != null) command.Transaction = transaction;

            command.CommandText = commandText;

            var wasClosed = dbConnection.State == ConnectionState.Closed;
            if (wasClosed) dbConnection.Open();
            var reader = command.ExecuteReader();

            if (wasClosed) dbConnection.Close();

            return reader;
        }

        public string GetCreateTableCommand<T>() where T : class, new()
        {
            var model = new T();
            var allProperties = GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyProperty = GetKeyProperty(allProperties);


            var strBuilderSql = new StringBuilder();
            if (keyProperty != null)
            {
                strBuilderSql.AppendLine($"create sequence \"{GetSequenceName(entityName, keyProperty.Name)}\";");
            }
            strBuilderSql.Append($"create table \"{entityName}\" (");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    strBuilderSql.Append($"\"{item.Name}\" {GetTypePostGres(item)} primary key not null default nextval ('{GetSequenceName(entityName, keyProperty.Name)}')");
                else
                    strBuilderSql.Append($"\"{item.Name}\" {GetTypePostGres(item)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.AppendLine(");");
            }

            if (keyProperty != null)
            {
                strBuilderSql.Append($"alter sequence {GetSequenceName(entityName, keyProperty.Name)} owned by \"{entityName}\".\"{keyProperty.Name}\";");
            }

            var sql = strBuilderSql.ToString();
            return sql;
        }

        private string GetSequenceName(string entityName, string columnName)
        {
            return $"sequence_{entityName}_{columnName}".ToLower();
        }

        public string GetDeleteCommand<T>(T obj, object key) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = GetEntityName<T>();

            var keyProperty = GetKeyProperty(allProperties);
            if (keyProperty == null)
                throw new Exception($"Key column not found for {entityName}");

            if (key is string)
                key = $"'{key}'";

            var sql = $"delete from \"{entityName}\" where \"{keyProperty.Name}\"={key}";
            return sql;
        }

        public string GetInsertCommand<T>(T obj, bool includeKey = false) where T : class, new()
        {
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyName = GetKeyProperty(allProperties);
            if (keyName == null && includeKey)
                throw new Exception($"Key column not found for {entityName}");

            var strBuilderSql = new StringBuilder($"insert into \"{entityName}\" (");
            foreach (var item in allProperties)
            {
                if (keyName == item && !includeKey)
                    continue;

                strBuilderSql.Append($"\"{item.Name}\"");

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
            var entityName = GetEntityName<T>();
            var propertyKey = GetKeyProperty(model.GetType().GetProperties());
         
            string scriptSelectCurrentValueId = $"SELECT currval('{GetSequenceName(entityName, propertyKey.Name)}');";
            var readerId = ExecuteReader(scriptSelectCurrentValueId, dbConnection);
            if (readerId.Read())
            {

                var id = readerId.GetInt32(0);
                readerId.Close();
                return id;
            }
            else
            {
                throw new Exception($"Could not get geranted Id in PostGres sequence for table {entityName}");
            }
            
        }

        public string GetSelectCommand<T>(T obj) where T : class, new()
        {
            var allProperties = GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var strBuilderSql = new StringBuilder($"select ");
            foreach (var item in allProperties)
            {
                strBuilderSql.Append($"\"{item.Name}\"");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append($" from \"{entityName}\"");
            }

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public string GetUpdateCommand<T>(T obj) where T : class, new()
        {
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyProperty = GetKeyProperty(allProperties);
            if (keyProperty == null)
                throw new Exception($"Key column not found for {entityName}");

            var strBuilderSql = new StringBuilder($"update \"{entityName}\" set ");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    continue;

                strBuilderSql.Append($"\"{item.Name}\"={DataFormatter.GetValue(item, obj, this.DbServerType)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
            }
            strBuilderSql.Append($" where \"{keyProperty.Name}\"={DataFormatter.GetValue(keyProperty, obj, this.DbServerType)}");

            var sql = strBuilderSql.ToString();
            return sql;
        }

        private string GetTypePostGres(PropertyInfo item)
        {
            switch (item.PropertyType.Name)
            {
                case "String":
                    return "character varying(255)";
                case "Int32":
                    return "integer";
                case "Int64":
                    return "bigint";
                case "Byte[]":
                    return "bytea";
                case "Boolean":
                    return "boolean";
                case "Decimal":
                    return "numeric(18,6)";
                case "DateTime":
                    return "Date";
                case "Double":
                    return "double precision";
                case "Nullable`1":
                    if (item.PropertyType.AssemblyQualifiedName.Contains("System.Int32"))
                        return "integer null";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Boolean"))
                        return "boolean null";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Decimal"))
                        return "numeric(18,6)";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Double"))
                        return "decimal(18,6)";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.DateTime"))
                        return "Date";
                    else
                        return "double precision";
                default:
                    return "character(255)";
            }
        }

        public object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null, string sequenceName = null)
        {
            if (string.IsNullOrEmpty(sequenceName))
            {
                return GetLastId<T>(model, dbConnection, transaction);
            }
            else
            {
                string scriptSelectCurrentValueId = $"SELECT currval('{sequenceName}');";
                var readerId = ExecuteReader(scriptSelectCurrentValueId, dbConnection);
                if (readerId.Read())
                    return readerId.GetInt32(0);
                else
                    throw new Exception($"Could not get geranted Id in PostGres sequence for table {model.GetType().Name}");
            }
        }

        public string GetWhereCommand<T>(Expression<Func<T, bool>> expression) where T: class, new()
        {
            return ExpressionQueryTranslator.GetWhereCommand<T>(expression, this.DbServerType);
        }

        public string GetSelectCommand<T>(T obj, Expression<Func<T, bool>> expression) where T : class, new()
        {
            var select = GetSelectCommand<T>(obj);
            var where = GetWhereCommand<T>(expression);
            return select + " " + where;
        }

        public Tuple<string, IEnumerable<DbSimpleParameter>> GetInsertCommandParameters<T>(T obj, bool includeKey = false) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public string GetUpdateCommandParameters<T>(T obj) where T : class, new()
        {
            throw new NotImplementedException();
        }

        Tuple<string, IEnumerable<DbSimpleParameter>> IScriptBuilder.GetUpdateCommandParameters<T>(T obj)
        {
            throw new NotImplementedException();
        }
    }
}
