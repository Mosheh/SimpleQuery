﻿using SimpleQuery.Domain.Data;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    public class ScriptMySqlBuilder : ScriptCommon, IScriptBuilder
    {
        public DbServerType DbServerType => DbServerType.MySql;

        public void Execute(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            var command = dbConnection.CreateCommand();
            if (transaction != null)
                command.Transaction = transaction;
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

        public string GetCreateTableCommand<T>(T obj) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyProperty = GetKeyProperty(allProperties);

            var strBuilderSql = new StringBuilder($"create table `{entityName}` (");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    strBuilderSql.Append($"`{item.Name}` {GetTypeMySql(item)} auto_increment");
                else
                    strBuilderSql.Append($"`{item.Name}` {GetTypeMySql(item)}");

                if (item != allProperties.Last())
                {
                    strBuilderSql.Append(", ");
                }
                else
                {
                    if (keyProperty != null && keyProperty.PropertyType.Name.Equals("Int32"))
                        strBuilderSql.Append($", primary key (`{keyProperty.Name}`)");
                    strBuilderSql.Append(")");
                }
            }


            var sql = strBuilderSql.ToString();
            return sql;
        }

        private object GetTypeMySql(PropertyInfo item)
        {
            switch (item.PropertyType.Name)
            {
                case "String":
                    return "nvarchar(255)";
                case "Int32":
                    return "int not null";
                case "Int64":
                    return "bigint";
                case "Byte``":
                    return "varbinary";
                case "Boolean":
                    return "boolean";
                case "Decimal":
                    return "decimal(18,6)";
                case "Double":
                    return "double";
                case "Nullable`1":
                    if (item.PropertyType.AssemblyQualifiedName.Contains("System.Int32"))
                        return "int";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Boolean"))
                        return "boolean null";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Decimal"))
                        return "decimal(18,6) null";
                    else
                        return "int not null";
                default:
                    return "nvarchar(255)";
            }
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

            var sql = $"delete from `{entityName}` where Id={key}";
            return sql;
        }

        public string GetInsertCommand<T>(T obj, bool includeKey = false) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyName = GetKeyProperty(allProperties);
            if (keyName == null && includeKey)
                throw new Exception($"Key column not found for {entityName}");

            var strBuilderSql = new StringBuilder($"insert into `{entityName}` (");
            foreach (var item in allProperties)
            {
                if (keyName == item && !includeKey)
                    continue;

                strBuilderSql.Append($"`{item.Name}`");

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
            var reader = ExecuteReader("SELECT LAST_INSERT_ID()", dbConnection, transaction);
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
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var strBuilderSql = new StringBuilder($"select ");
            foreach (var item in allProperties)
            {
                strBuilderSql.Append($"`{item.Name}`");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append($" from `{entityName}`");
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

            var strBuilderSql = new StringBuilder($"update `{entityName}` ");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    continue;

                strBuilderSql.Append($"set `{item.Name}`={DataFormatter.GetValue(item, obj, this.DbServerType)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
            }
            strBuilderSql.Append($" where `{keyProperty.Name}`={DataFormatter.GetValue(keyProperty, obj, this.DbServerType)}");

            var sql = strBuilderSql.ToString();
            return sql;
        }
    }
}
