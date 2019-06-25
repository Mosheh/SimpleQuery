using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace SimpleQuery.Tests
{
    [TestClass]
    public class UnitTestPerformance
    {
        [TestMethod]
        public void TestDapperSqliteInsert()
        {
            var conn = new SQLiteConnection($"Data Source={UnitTestExtentions. GetFileNameDb()}");
           
            CreateTableCliente(conn);

            var dateTimeBefore = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                var cliente = new Cliente { Nome = $"Cliente {i}" };
                var insertCommand = conn.GetScriptBuild().GetInsertCommand<Cliente>(cliente);
                conn.Execute("insert into Cliente (Nome) values (@a)", new[] { new { a = "" } });
            }
            var dateTimeAfter = DateTime.Now;

            System.Diagnostics.Debug.WriteLine(dateTimeAfter.Subtract(dateTimeBefore).Seconds);

            DropTableCliente(conn);
        }

        private void DropTableCliente(SQLiteConnection conn)
        {
            conn.Execute("Drop table Cliente"); 
        }

        private void CreateTableCliente(SQLiteConnection connection)
        {
            var scriptBuilder = connection.GetScriptBuild();
            connection.Execute(scriptBuilder.GetCreateTableCommand<Cliente>());
        }
        [TestMethod]
        public void TestSimpleQuerySqliteInsert()
        {
            var conn = new SQLiteConnection($"Data Source={UnitTestExtentions.GetFileNameDb()}");

            CreateTableCliente(conn);

            var dateTimeBefore = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                var cliente = new Cliente { Nome = $"Cliente {i}" };
                conn.InsertReturningId(cliente);
            }
            var dateTimeAfter = DateTime.Now;

            DropTableCliente(conn);

            System.Diagnostics.Debug.WriteLine(dateTimeAfter.Subtract(dateTimeBefore).Seconds);
        }
    }
}
