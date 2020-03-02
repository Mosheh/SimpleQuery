using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SimpleQuery
{
    public static partial class Extentions
    {
        /// <summary>
        /// Insert model and return last id
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="model">Instance model</param>
        /// <param name="dbTransaction">Transaction database</param>
        /// <returns></returns>
        public static int InsertReturningId<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
            where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
                var insertCommand = scripBuilder.GetInsertCommand<T>(model, false);

                var command = dbConnection.CreateCommand();

                if (dbTransaction != null) command.Transaction = dbTransaction;

                command.CommandText = insertCommand;
                var rowsCount = ExecuteNonQuery(command);
                Console.WriteLine($"{rowsCount} affected rows");

                var lastId = scripBuilder.GetLastId<T>(model, dbConnection, dbTransaction);

                return Convert.ToInt32(lastId);
            }
            catch (Exception e)
            {
                throw e.InnerException is null ? e : e.InnerException;
            }
            finally
            {
                if (dbTransaction is null)
                {
                    dbConnection.Close();
                }
            }
        }

        /// <summary>
        /// Insert model in the database
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="model">Instance model</param>
        /// <param name="dbTransaction">Transaction database</param>
        /// <returns></returns>
        public static T Insert<T>(this IDbConnection dbConnection, T model, bool includeKey = false, IDbTransaction dbTransaction = null)
           where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed) dbConnection.Open();

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);

                var command = GetCommandInsert<T>(dbConnection, model, dbTransaction, scripBuilder, includeKey);

                var rowsCount = ExecuteNonQuery(command);
                Console.WriteLine($"{rowsCount} affected rows");

                var lastId = scripBuilder.GetLastId<T>(model, dbConnection, dbTransaction);

                var keyProperty = scripBuilder.GetKeyPropertyModel<T>();
                if (keyProperty != null)
                {
                    var convertedValue = Convert.ChangeType(lastId, keyProperty.PropertyType);
                    if (convertedValue != null)
                        keyProperty.SetValue(model, convertedValue);
                }

                return model;
            }
            catch (Exception e)
            {
                throw e.InnerException is null ? e : e.InnerException;
            }
            finally
            {
                if (dbTransaction is null)
                {
                    dbConnection.Close();
                }
            }
        }

        private static IDbCommand GetCommandInsert<T>(IDbConnection dbConnection, T model, IDbTransaction dbTransaction, IScriptBuilder scripBuilder, bool includeKey = false) where T : class, new()
        {
            var command = dbConnection.CreateCommand();

            if (scripBuilder.DbServerType == Domain.Data.DbServerType.Sqlite)
            {
                var insertCommand = scripBuilder.GetInsertCommandParameters<T>(model, false);

                foreach (var item in insertCommand.Item2)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = item.ParameterName;
                    param.DbType = item.DbType;
                    param.Value = item.Value;
                    param.Size = item.Size;
                    command.Parameters.Add(param);
                }

                if (dbTransaction != null) command.Transaction = dbTransaction;

                command.CommandText = insertCommand.Item1;

                return command;
            }
            else
            {
                var insertCommand = scripBuilder.GetInsertCommand<T>(model, includeKey);

                if (dbTransaction != null) command.Transaction = dbTransaction;

                command.CommandText = insertCommand;

                return command;
            }
        }
        /// <summary>
        /// Update data model
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="model"></param>
        /// <param name="dbTransaction"></param>
        public static void Update<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
           where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);

                var command = GetCommandUpdate<T>(dbConnection, model, dbTransaction, scripBuilder);

                if (dbTransaction != null) command.Transaction = dbTransaction;

                var rowsCount = ExecuteNonQuery(command);
                Console.WriteLine($"{rowsCount} affected rows");
            }
            catch (Exception e)
            {
                throw e.InnerException is null ? e : e.InnerException;
            }
            finally
            {
                if (dbTransaction is null)
                {
                    dbConnection.Close();
                }
            }
        }

        private static IDbCommand GetCommandUpdate<T>(IDbConnection dbConnection, T model, IDbTransaction dbTransaction, IScriptBuilder scripBuilder) where T : class, new()
        {
            var command = dbConnection.CreateCommand();

            if (scripBuilder.DbServerType == Domain.Data.DbServerType.Sqlite)
            {
                var updateCommand = scripBuilder.GetUpdateCommandParameters<T>(model);

                foreach (var item in updateCommand.Item2)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = item.ParameterName;
                    param.DbType = item.DbType;
                    param.Value = item.Value;
                    param.Size = item.Size;

                    command.Parameters.Add(param);
                }
                if (dbTransaction != null) command.Transaction = dbTransaction;

                command.CommandText = updateCommand.Item1;

                return command;
            }
            else
            {
                var updateCommandText = scripBuilder.GetUpdateCommand<T>(model);

                if (dbTransaction != null) command.Transaction = dbTransaction;

                command.CommandText = updateCommandText;

                return command;
            }
        }
        /// <summary>
        /// Delete model from database
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="model">Instance model</param>
        /// <param name="dbTransaction">Transaction database</param>
        public static void Delete<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
          where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
                var keyProperty = scripBuilder.GetKeyPropertyModel<T>();
                if (keyProperty == null)
                    throw new Exception("Can't detarminated key property");
                var keyValue = keyProperty.GetValue(model);
                var deleteCommandText = scripBuilder.GetDeleteCommand<T>(model, keyValue);

                var command = dbConnection.CreateCommand();

                if (dbTransaction != null) command.Transaction = dbTransaction;

                command.CommandText = deleteCommandText;
                var rowsCount = ExecuteNonQuery(command);
                Console.WriteLine($"{rowsCount} affected rows");
            }
            catch (Exception e)
            {
                throw e.InnerException is null ? e : e.InnerException;
            }
            finally
            {
                if (dbTransaction is null)
                {
                    dbConnection.Close();
                }
            }
        }

        /// <summary>
        /// Execute text command in database and return coutn affected rows
        /// </summary>
        /// <param name="dbConnection">Connection</param>
        /// <param name="commandText">sql text command</param>
        /// <returns></returns>
        public static int Execute(this IDbConnection dbConnection, string commandText, IDbTransaction transaction = null)
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var command = dbConnection.CreateCommand(); if (transaction != null) command.Transaction = transaction;
            command.CommandText = commandText;
            var rowsCount = ExecuteNonQuery(command);

            Console.WriteLine($"{rowsCount} affected rows");

            return rowsCount;
        }

        /// <summary>
        /// Get all
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection dbConnection)
           where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                IScriptBuilder scriptBuilder = GetScriptBuild(dbConnection);
                var selectScript = scriptBuilder.GetSelectCommand<T>(new T());

                var reader = scriptBuilder.ExecuteReader(selectScript, dbConnection);
                var list = GetTypedList<T>(reader);

                reader.Close();

                return list;
            }
            catch (Exception e)
            {
                throw e.InnerException is null ? e : e.InnerException;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static IEnumerable<T> GetTypedList<T>(IDataReader reader) where T : class, new()
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

        /// <summary>
        /// Get all records from database in typed model
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="model">Instance model</param>
        /// <param name="dbTransaction">Transaction database</param>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection dbConnection, T model, IDbTransaction dbTransaction = null)
          where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
                var selectScript = scripBuilder.GetSelectCommand<T>(model);

                var reader = scripBuilder.ExecuteReader(selectScript, dbConnection, dbTransaction);
                var listModel = new List<T>();

                var dataTable = new DataTable();
                dataTable.Load(reader);

                foreach (DataRow row in dataTable.Rows)
                {
                    var newModel = GetModelByDataRow<T>(row);
                    listModel.Add(newModel);
                }
                reader.Close();

                return listModel;
            }
            catch (Exception e)
            {
                throw e.InnerException is null ? e : e.InnerException;
            }
            finally
            {
                if (dbTransaction is null)
                {
                    dbConnection.Close();
                }
            }
        }

        private static T GetModelByDataRow<T>(DataRow row) where T : class, new()
        {
            var model = new T();

            var properties = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var item in properties)
            {
                if (row.Table.Columns.Cast<DataColumn>().Any(c => c.ColumnName == item.Name))
                {
                    var rowValue = row[item.Name];
                    var value = ChangeType(rowValue, item.PropertyType);

                    item.SetValue(model, row[item.Name] == DBNull.Value ? null : value);
                }
            }

            return model;

        }
        /// <summary>
        /// Convert a type object to another
        /// </summary>
        /// <param name="value">Source object</param>
        /// <param name="conversion">Destination object</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }
            if (value is DBNull)
            {
                if (conversion.AssemblyQualifiedName.Contains("System.DateTime") ||
                    conversion.AssemblyQualifiedName.Contains("System.Int32") ||
                    conversion.AssemblyQualifiedName.Contains("System.Int64") ||
                    conversion.AssemblyQualifiedName.Contains("System.Decimal") ||
                    conversion.AssemblyQualifiedName.Contains("System.Double") ||
                    conversion.AssemblyQualifiedName.Contains("System.Boolean") ||
                    conversion.AssemblyQualifiedName.Contains("System.String") ||
                    conversion.AssemblyQualifiedName.Contains("System.Byte[]"))
                    return null;

                throw new Exception("Type value is not mapped");
            }
            return Convert.ChangeType(value, t);
        }

        /// <summary>
        /// Get script builder instance
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        public static IScriptBuilder GetScriptBuild(this IDbConnection dbConnection)
        {
            var name = dbConnection.GetType().Namespace;
            if (name.ToLower().Contains("system.data.sqlclient"))
                return new ScriptSqlServerBuilder();
            if (name.ToLower().Contains("hana"))
                return new ScriptHanaBuilder();
            if (name.ToLower().Contains("npgsql"))
                return new ScriptPostGresBuilder();
            if (name.ToLower().Contains("mysqlclient"))
                return new ScriptMySqlBuilder();
            if (name.ToLower().Contains("system.data.sqlite"))
                return new ScriptSqliteBuilder();

            return new ScriptAnsiBuilder();
        }

        public static int ExecuteNonQuery(IDbCommand dbCommand)
        {
            var name = dbCommand.GetType().Namespace;

            return name != null && name.ToLower().Contains("hana") ? new CommandHana().ExecuteNonQuery(dbCommand) : dbCommand.ExecuteNonQuery();
        }

        public static IDataReader ExecuteReader(IDbCommand dbCommand)
        {
            var name = dbCommand.GetType().Namespace;

            return name != null && name.ToLower().Contains("hana") ? new CommandHana().ExecuteReader(dbCommand) : dbCommand.ExecuteReader();
        }

        public static object ExecuteScalar(IDbCommand dbCommand)
        {
            var name = dbCommand.GetType().Namespace;

            return name != null && name.ToLower().Contains("hana") ? new CommandHana().ExecuteScalar(dbCommand) : dbCommand.ExecuteScalar();
        }
    }
}
