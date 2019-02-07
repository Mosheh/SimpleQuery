using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery
{
    public static partial class Extentions
    {
        public static IEnumerable<T> Select<T>(this IDbConnection dbConnection, Expression<Func<T, bool>> whereExpression)
         where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var type = typeof(T);
            var cacheType = typeof(List<T>);
            var instanceModel = new T();

            IScriptBuilder scripBuilder = GetScriptBuild(dbConnection);
            var selectScript = scripBuilder.GetSelectCommand<T>(instanceModel);
            var whereScript = scripBuilder.GetWhereCommand<T>(whereExpression);
            var selectAndWhere = selectScript+" "+whereScript;
            var reader = scripBuilder.ExecuteReader(selectAndWhere, dbConnection);
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

        public static IEnumerable<T> Select<T>(this IDbConnection dbConnection, string sqlCommandText)
         where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var type = typeof(T);
            var cacheType = typeof(List<T>);
            var instanceModel = new T();

            var command = dbConnection.CreateCommand();
            command.CommandText = sqlCommandText;
            var reader = command.ExecuteReader();
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

        public static IEnumerable<T> Query<T>(this IDbConnection dbConnection, string commandText)
        where T : class, new()
        {
            var wasClosed = dbConnection.State == ConnectionState.Closed;

            if (wasClosed) dbConnection.Open();

            var type = typeof(T);
            var cacheType = typeof(List<T>);
            var instanceModel = new T();

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

            if (wasClosed) dbConnection.Close();

            reader.Close();

            return listModel;
        }
    }
}
