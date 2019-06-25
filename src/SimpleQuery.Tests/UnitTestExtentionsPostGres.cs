using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();

            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

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

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);
            Assert.AreEqual(1, id);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestCategory("CRUD")]
        [TestMethod]
        public void PostGresSelectWithWherePrimitiveTypes()
        {
            var connection = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            connection.Open();


            using (var conn = connection)
            {
                IScriptBuilder builder = conn.GetScriptBuild();

                User user = new User() { Name = "Moisés", Email = "moises@gmail.com", Ratting = 10, Scores = 20 };
                User user2 = new User() { Name = "Miranda", Email = "miranda@gmail.com", Ratting = 20, Scores = 50 };
                User user3 = new User() { Name = "Moshe", Email = "moshe@gmail.com", Ratting = 20, Scores = 21, System = true };

                var createTableScript = builder.GetCreateTableCommand<User>();
                builder.Execute(createTableScript, conn);
                conn.Insert(user);
                conn.Insert(user2);
                conn.Insert(user3);

                var userFirst = conn.Select<User>(c => c.Email == user.Email);
                var userSecond = conn.Select<User>(c => c.Id == 2);
                var userThird = conn.Select<User>(c => c.System == true);
                var noSystem = conn.Select<User>(c => c.System == false);
                var userRatting20 = conn.Select<User>(c => c.Ratting == 20);
                var usersScore21 = conn.Select<User>(c => c.Scores == 21);

                Assert.AreEqual(1, userFirst.Count());
                Assert.AreEqual("Miranda", userSecond.ToList()[0].Name);
                Assert.AreEqual("Moshe", userThird.ToList()[0].Name);
                Assert.AreEqual(2, noSystem.Count());
                Assert.AreEqual(2, userRatting20.Count());
                Assert.AreEqual("Moshe", usersScore21.ToList()[0].Name);

                conn.Execute("drop table \"User\"");

                conn.Close();

            }
        }

        [TestMethod]
        public void PostGresInsertModelFillingId()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
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

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);

            var clientes = conn.GetAll<Cliente>();

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void PostGresTestOperators()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente1 = new Cliente() { Nome = "Miranda" };
            var cliente2 = new Cliente() { Nome = "Moshe" };
            var cliente3 = new Cliente() { Nome = "Moshe" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente1);
            conn.Insert(cliente2);
            conn.Insert(cliente3);

            var clientesWithIdGreaterThan1 = conn.Select<Cliente>(c=>c.Id>1);
            var clientesIdOtherThan3 = conn.Select<Cliente>(c => c.Id != 3);
            var clientesWithIdLessThan2 = conn.Select<Cliente>(c => c.Id < 2);

            Assert.AreEqual(2, clientesWithIdGreaterThan1.Count());
            Assert.AreEqual("Miranda", clientesIdOtherThan3.ToList()[0].Nome);
            Assert.AreEqual(2, clientesIdOtherThan3.ToList()[1].Id);
            Assert.AreEqual(1, clientesWithIdLessThan2.Count());

            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void PostGresDeleteModel()
        {
            var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings["postgres"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertReturningId<Cliente>(cliente);
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

    public class Undertaking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? Avaiable { get; set; }
    }

    [Table("Invoices")]
    public class Invoice
    {
        public int Id { get; set; }
        public string Customer{ get; set; }
        public double Value { get; set; }
        public Endereco Endereco { get; set; }
    }

    public class Endereco
    {
        public int ClienteId { get; set; }
        public string AddressDescription { get; set; }
    }
}
