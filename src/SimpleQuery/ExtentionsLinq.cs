﻿using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace SimpleQuery
{
    public static partial class Extentions
    {
        /// <summary>
        /// Get typed list model
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="whereExpression">where linq criterion, when pass it the same is translate to sql command</param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(this IDbConnection dbConnection, Expression<Func<T, bool>> whereExpression)
         where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                var instanceModel = new T();

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
                var selectScript = scripBuilder.GetSelectCommand<T>(instanceModel);
                var whereScript = scripBuilder.GetWhereCommand<T>(whereExpression);
                var selectAndWhere = selectScript + " " + whereScript;
                var reader = scripBuilder.ExecuteReader(selectAndWhere, dbConnection);
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
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Get type list model
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="sqlCommandText">Sql command</param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(this IDbConnection dbConnection, string sqlCommandText)
         where T : class, new()
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                var command = dbConnection.CreateCommand();
                command.CommandText = sqlCommandText;
                var reader = ExecuteReader(command);
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
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Get typed list model
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="commandText">Sql command</param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this IDbConnection dbConnection, string commandText)
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
                var reader = scripBuilder.ExecuteReader(commandText, dbConnection);
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
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Get primitive types as List
        /// </summary>
        /// <typeparam name="T">Primitive type like a struct</typeparam>
        /// <param name="dbConnection">DbConnection</param>
        /// <param name="commandText">Sql command</param>
        /// <returns></returns>
        public static IEnumerable<T> Scalar<T>(this IDbConnection dbConnection, string commandText)
            where T : struct
        {
            try
            {
                var wasClosed = dbConnection.State == ConnectionState.Closed;

                if (wasClosed)
                {
                    dbConnection.Open();
                }

                var type = typeof(T);
                var list = new List<T>();

                IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
                var reader = scripBuilder.ExecuteReader(commandText, dbConnection);
                while (reader.Read())
                {

                    object objectRead;
                    switch (type.Name)
                    {
                        case "Int32":
                            objectRead = ChangeType(reader.GetInt32(0), typeof(T));

                            break;
                        case "Decimal":
                            objectRead = ChangeType(reader.GetDecimal(0), typeof(T));

                            break;
                        case "Double":
                            objectRead = ChangeType(reader.GetDouble(0), typeof(T));

                            break;
                        case "Float":
                            objectRead = ChangeType(reader.GetFloat(0), typeof(T));
                            break;
                        case "String":
                            objectRead = ChangeType(reader.GetString(0), typeof(T));
                            break;
                        default:
                            throw new Exception($"The type { typeof(T).Name} is not mapped");
                    }

                    list.Add((T)objectRead);
                }

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
    }
}
