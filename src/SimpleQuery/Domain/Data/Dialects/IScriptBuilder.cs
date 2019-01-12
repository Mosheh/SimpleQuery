using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace SimpleQuery.Domain.Data.Dialects
{
    public interface IScriptBuilder
    {
        string GetInsertCommand<T>(T obj,bool includeKey= false) where T : class, new();

        string GetUpdateCommand<T>(T obj) where T : class, new();

        string GetSelectCommand<T>(T obj) where T : class, new();

        string GetDeleteCommand<T>(T obj, object key) where T : class, new();

        string GetCreateTableCommand<T>(T obj) where T : class, new();

        object GetLastId<T>(T model, IDbConnection dbConnection);

        PropertyInfo GetKeyProperty(PropertyInfo[] allProperties);
        IDataReader ExecuteReader(string command, IDbConnection dbConnection);
        void Execute(string command, IDbConnection dbConnection);
    }
}
