using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    public class ScriptSqlServerBuilder : ScriptCommon, IScriptBuilder
    {
        public IDataReader ExecuteReader(string command, IDbConnection dbConnection)
        {
            throw new NotImplementedException();
        }

        public void Execute(string command, IDbConnection dbConnection)
        {
            throw new NotImplementedException();
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
            var allProperties = obj.GetType().GetProperties();
            var entityName = obj.GetType().Name;

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
            var reader = ExecuteReader("SELECT SCOPE_IDENTITY()", dbConnection);
            return reader.GetInt32(0);
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

            var strBuilderSql = new StringBuilder($"update [{entityName}] ");
            foreach (var item in allProperties)
            {
                if (keyProperty == item)
                    continue;

                strBuilderSql.Append($"set [{item.Name}]={DataFormatter.GetValue(item, obj)}");

                if (item != allProperties.Last())
                    strBuilderSql.Append(", ");                                    
            }
            strBuilderSql.Append($" where {keyProperty.Name}={DataFormatter.GetValue(keyProperty, obj)}");

            var sql = strBuilderSql.ToString();
            return sql;
        }

        public string GetCreateTableCommand<T>(T obj) where T : class, new()
        {
            throw new NotImplementedException();
        }
    }
}
