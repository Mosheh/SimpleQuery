using SimpleQuery.Domain.Data;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    public class ScriptSqliteBuilder : ScriptCommon, IScriptBuilder
    {
        public DbServerType DbServerType => throw new NotImplementedException();

        public void Execute(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(string commandText, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public string GetCreateTableCommand<T>(T obj) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public string GetDeleteCommand<T>(T obj, object key) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public string GetInsertCommand<T>(T obj, bool includeKey = false) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public object GetLastId<T>(T model, IDbConnection dbConnection, IDbTransaction transaction = null, string sequenceName = null)
        {
            throw new NotImplementedException();
        }

        public string GetSelectCommand<T>(T obj) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public string GetUpdateCommand<T>(T obj) where T : class, new()
        {
            throw new NotImplementedException();
        }
    }
}
