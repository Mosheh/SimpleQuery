using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SimpleQuery
{
    public static class Extentions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        public static int InsertRereturnId<T>(this IDbConnection dbConnection, T model)
            where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection.ConnectionString);
            var insertCommand = scripBuilder.GetInsertCommand<T>(model, true);

            var command = dbConnection.CreateCommand();
            command.CommandText = insertCommand;
            command.ExecuteNonQuery();

            var propertyKey = scripBuilder.GetKeyProperty(model.GetType().GetProperties());
            var lastId = scripBuilder.GetLastId<T>(model, dbConnection);

            if (wasClosed) dbConnection.Close();

            return Convert.ToInt32(lastId);
        }

        public static IEnumerable<T> GetAll<T>(this IDbConnection dbConnection, T model)
           where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var type = typeof(T);
            var cacheType = typeof(List<T>);

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection.ConnectionString);
            var selectScript = scripBuilder.GetSelectCommand<T>(model);

            var reader = scripBuilder.ExecuteReader(selectScript, dbConnection);
            var listModel = new List<T>();

            var dataTable = new DataTable();
            dataTable.Load(reader);

            foreach (DataRow row in dataTable.Rows)
            {
                var newModel = GetModelByDataRow<T>(row);
                listModel.Add(newModel);
            }

            if (wasClosed) dbConnection.Close();

            return listModel;
        }

        private static T GetModelByDataRow<T>(DataRow row) where T : class, new()
        {
            var model = new T();
            var properties = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var item in properties)
            {
                if (row.Table.Columns.Cast<DataColumn>().Any(c => c.ColumnName == item.Name))
                {
                    item.SetValue(model, row[item.Name]==DBNull.Value? null: row[item.Name]);
                }
            }

            return model;
        }

        private static IScriptBuilder GetScriptBuild(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            if (connectionString.ToLower().Contains("current schema"))
                return new ScriptHanaBuilder();
            else
                return new ScriptSqlServerBuilder();
        }
    }
}
