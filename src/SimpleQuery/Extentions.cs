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
using System.Transactions;

namespace SimpleQuery
{
    public static class Extentions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        public static int InsertRereturnId<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
            where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
            var insertCommand = scripBuilder.GetInsertCommand<T>(model, false);

            var command = dbConnection.CreateCommand();

            if (dbTransaction != null) command.Transaction = dbTransaction;

            command.CommandText = insertCommand;
            var rowsCount = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsCount} affected rows");

            var lastId = scripBuilder.GetLastId<T>(model, dbConnection, dbTransaction);

            if (wasClosed) dbConnection.Close();

            return Convert.ToInt32(lastId);
        }

        public static T Insert<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
           where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
            var insertCommand = scripBuilder.GetInsertCommand<T>(model, false);

            var command = dbConnection.CreateCommand();

            if (dbTransaction != null) command.Transaction = dbTransaction;

            command.CommandText = insertCommand;
            var rowsCount = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsCount} affected rows");

            var lastId = scripBuilder.GetLastId<T>(model, dbConnection, dbTransaction);

            if (wasClosed) dbConnection.Close();

            var keyProperty = scripBuilder.GetKeyPropertyModel<T>();
            if (keyProperty != null)
            {
                var convertedValue = Convert.ChangeType(lastId, keyProperty.PropertyType);
                keyProperty.SetValue(model, convertedValue);
            }
            return model;
        }

        public static void Update<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
           where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
            var updateCommandText = scripBuilder.GetUpdateCommand<T>(model);

            var command = dbConnection.CreateCommand();

            if (dbTransaction != null) command.Transaction = dbTransaction;

            command.CommandText = updateCommandText;
            var rowsCount = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsCount} affected rows");            

            if (wasClosed) dbConnection.Close();
            
        }

        public static void Delete<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
          where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
            var keyProperty = scripBuilder.GetKeyPropertyModel<T>();
            if (keyProperty == null)
                throw new Exception("Can't detarminated key property");
            var keyValue = keyProperty.GetValue(model);
            var deleteCommandText = scripBuilder.GetDeleteCommand<T>(model, keyValue);

            var command = dbConnection.CreateCommand();

            if (dbTransaction != null) command.Transaction = dbTransaction;

            command.CommandText = deleteCommandText;
            var rowsCount = command.ExecuteNonQuery();
            Console.WriteLine($"{rowsCount} affected rows");

            if (wasClosed) dbConnection.Close();

        }

        /// <summary>
        /// Execute text command in database and return coutn affected rows
        /// </summary>
        /// <param name="dbConnection">Connection</param>
        /// <param name="commandText">sql text command</param>
        /// <returns></returns>
        public static int Execute(this IDbConnection dbConnection, string commandText)
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);

            var command = dbConnection.CreateCommand();
            command.CommandText = commandText;
            var rowsCount = command.ExecuteNonQuery();

            Console.WriteLine($"{rowsCount} affected rows");

            return rowsCount;
        }       

        public static IEnumerable<T> GetAll<T>(this IDbConnection dbConnection)
           where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var type = typeof(T);
            var cacheType = typeof(List<T>);

            IScriptBuilder scriptBuilder = GetScriptBuild(dbConnection);
            var selectScript = scriptBuilder.GetSelectCommand<T>(new T());

            var reader = scriptBuilder.ExecuteReader(selectScript, dbConnection);
            var list = GetTypedList<T>(reader);
            if (wasClosed) dbConnection.Close();

            reader.Close();

            return list;
        }

        static IEnumerable<T> GetTypedList<T>(IDataReader reader) where T: class, new()
        {
            
            var listModel = new List<T>();

            var dataTable = new DataTable();
            dataTable.Load(reader);

            foreach (DataRow row in dataTable.Rows)
            {
                var newModel = GetModelByDataRow<T>(row);
                listModel.Add(newModel);
            }

            return listModel;
        }

        public static IEnumerable<T> GetAll<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction)
          where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var type = typeof(T);
            var cacheType = typeof(List<T>);

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
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

            reader.Close();

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
                    item.SetValue(model, row[item.Name] == DBNull.Value ? null : Convert.ChangeType( row[item.Name], item.PropertyType));
                }
            }

            return model;
        }

        public static IScriptBuilder GetScriptBuild(this IDbConnection dbConnection)
        {
            var name = dbConnection.GetType().Namespace;
            if (name.ToLower().Contains("System.Data.SqlClient"))
                return new ScriptSqlServerBuilder();
            else if (name.ToLower().Contains("hana"))
                return new ScriptHanaBuilder();
            else if (name.ToLower().Contains("npgsql"))
                return new ScriptPostGresBuilder();
            else if (name.ToLower().Contains("mysqlclient"))
                return new ScriptMySqlBuilder();
            else if (name.ToLower().Contains("system.data.sqlite"))
                return new ScriptSqliteBuilder();
            else
                return new ScriptSqlServerBuilder();
        }
    }
}
