using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Tests
{
    public partial class UnitTestExtentions
    {
        [TestMethod]
        public void SQLServerUpdateModel()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();

            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

            cliente.Id = id;
            cliente.Nome = "Moisés Miranda";
            conn.Update<Cliente>(cliente);

            conn.Execute("drop table [Cliente]");
        }

        [TestMethod]
        public void SQLServerExecuteReader()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();

            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

            cliente.Id = id;
            cliente.Nome = "Moisés Miranda";
            conn.Insert<Cliente>(cliente);

            var reader = scriptBuilder.ExecuteReader("select * from Cliente", conn);
            var data = new System.Data.DataTable();
            data.Load(reader);

            conn.Execute("drop table [Cliente]");

            Assert.AreEqual(2, data.Rows.Count);
        }

        [TestMethod]
        public void SQLServerInsertModel()
        {
            
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);
            Assert.AreEqual(1, id);
            conn.Execute("drop table [Cliente]");
        }

        [TestMethod]
        public void SQLServerInsertModelFillingId()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            conn.Insert<Cliente>(cliente);
            Assert.AreEqual(1, cliente.Id);
            conn.Execute("drop table [Cliente]");
        }

        [TestMethod]
        public void SQLServerExecuteSqlCommand()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            conn.Insert<Cliente>(cliente);
            conn.Insert<Cliente>(cliente);
            conn.Insert<Cliente>(cliente);
            var clientes = conn.Select<Cliente>("select * from Cliente");
            Assert.AreEqual(3, clientes.Count());
            conn.Execute("drop table [Cliente]");
        }


        [TestMethod]
        public void SQLServerSelectModel()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

            var clientes = conn.GetAll<Cliente>();

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table [Cliente]");
        }

        [TestMethod]
        public void SQLServerSelectWithWhere()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

            var clientes = conn.Select<Cliente>(c=>c.Id==1);

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table [Cliente]");
        }

        [TestMethod]
        public void SQLServerSelectWithWhereStringField()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

            var clientes = conn.Select<Cliente>(c => c.Nome == "Miranda");

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table [Cliente]");
        }

        [TestMethod]
        public void SQLServerDeleteModel()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);
            cliente.Id = id;
            conn.Delete(cliente);

            conn.Execute("drop table [Cliente]");
        }
    }
}
