using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Tests
{
    [TestClass]
    public partial class UnitTestExtentions
    {
        [TestMethod]
        public void CheckProviders()
        {
            DataTable table = DbProviderFactories.GetFactoryClasses();

            // Display each row and column value.
            foreach (DataRow row in table.Rows)
            {
                Console.WriteLine(row["Name"]);
                System.Diagnostics.Debug.WriteLine(row["Name"]);
            }
        }


        [TestMethod]
        public void PostGresUpdateModel()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            cliente.Id = id;
            cliente.Nome = "Moisés Miranda";
            conn.Update<Cliente>(cliente);
            
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void PostGresInsertModel()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);
            Assert.AreEqual(1, id);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void PostGresInsertModelFillingId()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            conn.Insert<Cliente>(cliente);
            Assert.AreEqual(1, cliente.Id);
            conn.Execute("drop table \"Cliente\"");
        }


        [TestMethod]
        public void PostGresSelectModel()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            var clientes = conn.GetAll<Cliente>();

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void PostGresDeleteModel()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);
            cliente.Id = id;
            conn.Delete(cliente);

            conn.Execute("drop table \"Cliente\"");
        }
    }
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public Endereco Endereco { get; set; }
    }

    public class Endereco
    {
        public int ClienteId { get; set; }
        public string AddressDescription { get; set; }
    }
}
