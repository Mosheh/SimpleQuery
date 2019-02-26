using SimpleQuery.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace SimpleQuery.Domain.Data.Dialects
{
    public interface IScriptBuilder
    {
        /// <summary>
        /// Return insert command
        /// </summary>
        /// <typeparam name="T">Model (entity)</typeparam>
        /// <param name="obj">Instance model</param>
        /// <param name="includeKey">if true include column identity </param>
        /// <returns>sql insert command</returns>
        string GetInsertCommand<T>(T obj,bool includeKey= false) where T : class, new();

        Tuple<string, IEnumerable<DbSimpleParameter>> GetInsertCommandParameters<T>(T obj, bool includeKey = false) where T : class, new();

        /// <summary>
        /// Return update command
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="obj">Instace model</param>
        /// <returns>sql update command</returns>
        string GetUpdateCommand<T>(T obj) where T : class, new();

        Tuple<string, IEnumerable<DbSimpleParameter>> GetUpdateCommandParameters<T>(T obj) where T : class, new();

        /// <summary>
        /// Return select command from instance model
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="obj">Instance model</param>
        /// <returns></returns>
        string GetSelectCommand<T>(T obj) where T : class, new();

        /// <summary>
        /// Return table name based in Table Attribute or Class name
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns></returns>
        string GetEntityName<T>();

        /// <summary>
        /// Return select command from instance model
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="obj">Instance model</param>
        /// <returns></returns>
        string GetSelectCommand<T>(T obj, Expression<Func<T, bool>> expression) where T : class, new();

        /// <summary>
        /// Return delete command from instance model
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="obj">Instance model</param>
        /// <param name="key">key value in database</param>
        /// <returns></returns>
        string GetDeleteCommand<T>(T obj, object key) where T : class, new();

        /// <summary>
        /// Return create table command
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="obj">Instance model</param>
        /// <returns></returns>
        string GetCreateTableCommand<T>() where T : class, new();

        /// <summary>
        /// Return the generated key
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="model">Instance model</param>
        /// <param name="dbConnection">Connection</param>
        /// <param name="transaction">Transaction</param>
        /// <returns></returns>
        object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null);

        /// <summary>
        /// Return the generated key
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="model">Instance model</param>
        /// <param name="dbConnection">Connection</param>
        /// <param name="transaction">Transaction</param>
        /// <param name="sequenceName">Name of the string used in some databases</param>
        /// <returns></returns>
        object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null, string sequenceName=null);

        /// <summary>
        /// Return the property representing the primary key in table. Based on the criteria: If the name of the property is named "Id" or "DocEntry"
        /// </summary>
        /// <param name="allProperties"></param>
        /// <returns></returns>
        PropertyInfo GetKeyProperty(PropertyInfo[] allProperties);

        /// <summary>
        /// Return the property representing the primary key in table. Based on the criteria: If the name of the property is named "Id" or "DocEntry"
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <returns></returns>
        PropertyInfo GetKeyPropertyModel<T>() where T : class, new();

        /// <summary>
        /// Execute sql command in database
        /// </summary>
        /// <param name="commandText">Sql command</param>
        /// <param name="dbConnection">Connection</param>
        /// <param name="transaction">Transaction</param>
        /// <returns>Select result</returns>
        IDataReader ExecuteReader(string commandText, IDbConnection dbConnection, IDbTransaction transaction=null);

        /// <summary>
        /// Return where command
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="expression">Expression to translate</param>
        /// <returns></returns>
        string GetWhereCommand<T>(Expression<Func<T, bool>> expression) where T: class, new();

        /// <summary>
        /// Execute sql command in database
        /// </summary>
        /// <param name="commandText">Sql command</param>
        /// <param name="dbConnection">Connection</param>
        /// <param name="transaction">Transaction</param>
        void Execute(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null);
        /// <summary>
        /// Database server type
        /// </summary>
        DbServerType DbServerType { get; }
    }
}
