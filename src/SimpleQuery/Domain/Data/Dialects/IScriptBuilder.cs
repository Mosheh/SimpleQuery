using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

        /// <summary>
        /// Return update command
        /// </summary>
        /// <typeparam name="T">Model (entity)</typeparam>
        /// <param name="obj">Instace model</param>
        /// <returns>sql update command</returns>
        string GetUpdateCommand<T>(T obj) where T : class, new();

        string GetSelectCommand<T>(T obj) where T : class, new();

        string GetDeleteCommand<T>(T obj, object key) where T : class, new();

        string GetCreateTableCommand<T>(T obj) where T : class, new();

        object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null);
        object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null, string sequenceName=null);

        PropertyInfo GetKeyProperty(PropertyInfo[] allProperties);
        PropertyInfo GetKeyPropertyModel<T>() where T : class, new();
        IDataReader ExecuteReader(string commandText, IDbConnection dbConnection, IDbTransaction transaction=null);
        void Execute(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null);
        DbServerType DbServerType { get; }
    }
}
