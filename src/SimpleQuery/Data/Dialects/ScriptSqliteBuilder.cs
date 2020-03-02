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

namespace SimpleQuery.Data.Dialects
{
    public class ScriptSqliteBuilder : ScriptCommon, IScriptBuilder
    {
        public DbServerType DbServerType => DbServerType.Sqlite;

        public void Execute(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            var command = dbConnection.CreateCommand();
            if (transaction != null)
                command.Transaction = transaction;
            command.CommandText = commandText;

            var wasClosed = dbConnection.State == ConnectionState.Closed;
            if (wasClosed)
            {
                dbConnection.Open();
            }
            var rowsCount = Extentions.ExecuteNonQuery(command);
            Console.WriteLine($"{rowsCount} affected rows");
        }

        public IDataReader ExecuteReader(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            var command = dbConnection.CreateCommand();
            if (transaction != null) command.Transaction = transaction;

            command.CommandText = commandText;

            var wasClosed = dbConnection.State == ConnectionState.Closed;
            if (wasClosed)
            {
                dbConnection.Open();
            }
            var reader = Extentions.ExecuteReader(command);

            return reader;
        }

        public string GetCreateTableCommand<T>() where T : class, new()
        {
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyProperty = GetKeyProperty(allProperties);

            var strBuilderSql = new StringBuilder($"create table [{entityName}] (");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    strBuilderSql.Append($"[{item.Name}] {GetTypeSqlite(item)} primary key autoincrement");
                else
                    strBuilderSql.Append($"[{item.Name}] {GetTypeSqlite(item)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(")");
            }


            var sql = strBuilderSql.ToString();
            return sql;
        }

        private string GetTypeSqlite(PropertyInfo item)
        {
            switch (item.PropertyType.Name)
            {
                case "String":
                    return "text";
                case "Int32":
                    return "integer not null";
                case "Int64":
                    return "integer";
                case "Byte[]":
                    return "blob";
                case "Boolean":
                    return "boolean";
                case "Decimal":
                    return "decimal(18,6)";
                case "Double":
                    return "double";
                case "DateTime":
                    return "datetime";
                case "Nullable`1":
                    if (item.PropertyType.AssemblyQualifiedName.Contains("System.Int32"))
                        return "integer null";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Boolean"))
                        return "boolean null";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Decimal"))
                        return "decimal(18,6) null";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.DateTime"))
                        return "datetime null";
                    else
                        return "integer null";
                default:
                    return "text";
            }
        }

        public string GetDeleteCommand<T>(T obj, object key) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = GetEntityName<T>();

            var keyName = GetKeyProperty(allProperties);
            if (keyName == null)
                throw new Exception($"Key column not found for {entityName}");

            if (key is string)
                key = $"'{key}'";

            var sql = $"delete from [{entityName}] where [Id]={key}";
            return sql;
        }

        public string GetInsertCommand<T>(T obj, bool includeKey = false) where T : class, new()
        {
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyName = GetKeyProperty(allProperties);
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
            //Values
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
            var reader = ExecuteReader("SELECT last_insert_rowid()", dbConnection, transaction);
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

        public object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null, string sequenceName = null)
        {
            return GetLastId<T>(model, dbConnection, transaction);
        }

        public string GetSelectCommand<T>(T obj) where T : class, new()
        {
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

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
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

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
            strBuilderSql.Append($" where [{keyProperty.Name}]={DataFormatter.GetValue(keyProperty, obj, this.DbServerType)}");

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public string GetWhereCommand<T>(Expression<Func<T, bool>> expression)
             where T : class, new()
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
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyName = GetKeyProperty(allProperties);
            if (keyName == null && includeKey)
                throw new Exception($"Key column not found for {entityName}");

            List<DbSimpleParameter> parameters = new List<DbSimpleParameter>();
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
            //Values
            foreach (var item in allProperties)
            {
                if (keyName == item && !includeKey)
                    continue;

                strBuilderSql.Append($"@{item.Name}");
                parameters.Add(new DbSimpleParameter(item.Name, GetParamType(item), GetParamSize(item), DataFormatter.GetValueForParameter(item, obj, this.DbServerType)));
                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(")");
            }

            var sql = strBuilderSql.ToString();
            return new Tuple<string, IEnumerable<DbSimpleParameter>>(sql, parameters);
        }

        Tuple<string, IEnumerable<DbSimpleParameter>> IScriptBuilder.GetUpdateCommandParameters<T>(T obj)
        {
            var allProperties = ScriptCommon.GetValidProperty<T>();
            var entityName = GetEntityName<T>();

            var keyProperty = GetKeyProperty(allProperties);
            if (keyProperty == null)
                throw new Exception($"Key column not found for {entityName}");

            List<DbSimpleParameter> parameters = new List<DbSimpleParameter>();

            var strBuilderSql = new StringBuilder($"update [{entityName}] set ");
            foreach (var item in allProperties)
            {
                if (keyProperty == item )
                    continue;

                strBuilderSql.Append($"[{item.Name}]=@{item.Name}");
                parameters.Add(new DbSimpleParameter($"@{item.Name}", GetParamType(item), GetParamSize(item), DataFormatter.GetValueForParameter(item, obj, this.DbServerType)));

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(" ");
            }
            
            strBuilderSql.Append($" where [{keyProperty.Name}]={DataFormatter.GetValue(keyProperty, obj, this.DbServerType)}");

            var sql = strBuilderSql.ToString();
            return new Tuple<string, IEnumerable<DbSimpleParameter>> (sql, parameters);
        }
    }
}
