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
    public class ScriptHanaBuilder : ScriptCommon, IScriptBuilder
    {
        public IDataReader ExecuteReader(string commandText, IDbConnection dbConnection)
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = commandText;

            var wasClosed = dbConnection.State == ConnectionState.Closed;
            if (wasClosed) dbConnection.Open();
            var reader = command.ExecuteReader();

            if (wasClosed) dbConnection.Close();

            return reader;
        }

        public void Execute(string commandText, IDbConnection dbConnection)
        {
            var command = dbConnection.CreateCommand();
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

            var sql = $"delete \"{entityName}\" where Id={key}";
            return sql;
        }

        public string GetInsertCommand<T>(T obj, bool includeKey = false) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

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

                strBuilderSql.Append($"{DataFormatter.GetValue(item, obj)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(")");
            }

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public object GetLastId<T>(T model, IDbConnection dbConnection)
        {
            var entityName = model.GetType().Name;
            var propertyKey = GetKeyProperty(model.GetType().GetProperties());
            var scriptColumnId = $"select \"COLUMN_ID\" from table_columns where table_name = '{entityName}' and column_name='{propertyKey.Name}'";
            var readerColumnId = ExecuteReader(scriptColumnId, dbConnection);
            var columnId = readerColumnId.Read() ? readerColumnId.GetInt32(0) : throw new Exception("Could not get column id in Hana schema");

            string scriptSelectSequenceName = $"select \"SEQUENCE_NAME\" from sequences where \"SEQUENCE_NAME\" like '%" + columnId + "%'";
            var readerSeqName = ExecuteReader(scriptSelectSequenceName, dbConnection);
            var sequenceName = readerSeqName.Read() ? readerSeqName.GetString(0) : throw new Exception("Could not get sequence name in Hana schema");

            string scriptSelectCurrentValueId = "select \"" + sequenceName + "\".currval as \"ValueField\"from \"DUMMY\"";
            var readerId = ExecuteReader(scriptSelectCurrentValueId, dbConnection);
            if (readerId.Read())
                return readerId.GetInt32(0);
            else
                throw new Exception($"Could not get geranted Id in Hana sequence for table {entityName}");
        }

        public string GetSelectCommand<T>(T obj) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

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
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyProperty = GetKeyProperty(allProperties);
            if (keyProperty == null)
                throw new Exception($"Key column not found for {entityName}");

            var strBuilderSql = new StringBuilder($"update \"{entityName}\" ");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    continue;

                strBuilderSql.Append($"set \"{item.Name}\"={DataFormatter.GetValue(item, obj)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
            }
            strBuilderSql.Append($" where {keyProperty.Name}={DataFormatter.GetValue(keyProperty, obj)}");

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public string GetCreateTableCommand<T>(T obj) where T : class, new()
        {
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

            var keyProperty = GetKeyProperty(allProperties);

            var strBuilderSql = new StringBuilder($"create table \"{entityName}\" (");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    strBuilderSql.Append($"\"{item.Name}\" {GetTypeHana(item)} primary key generated by default as IDENTITY");
                else
                    strBuilderSql.Append($"\"{item.Name}\" {GetTypeHana(item)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");
                else
                    strBuilderSql.Append(")");
            }


            var sql = strBuilderSql.ToString();
            return sql;
        }

        private string GetTypeHana(PropertyInfo item)
        {
            switch (item.PropertyType.Name)
            {
                case "String":
                    return "VARCHAR(255)";
                case "Int32":
                    return "INTEGER not null";
                case "Int64":
                    return "BIGINT";
                case "Byte[]":
                    return "VARBINARY";
                case "Boolean":
                    return "BOOLEAN";
                case "Decimal":
                    return "DECIMAL(18,6)";
                case "Double":
                    return "DOUBLE";
                case "Nullable`1":
                    if (item.PropertyType.AssemblyQualifiedName.Contains("System.Int32"))
                        return "INTEGER";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Boolean"))
                        return "BOOLEAN";
                    else if (item.PropertyType.AssemblyQualifiedName.Contains("System.Decimal"))
                        return "DECIMAL(18,6)";
                    else
                        return "INTEGER NOT NULL";
                default:
                    return "VARCHAR (255)";
            }
        }
    }
}
