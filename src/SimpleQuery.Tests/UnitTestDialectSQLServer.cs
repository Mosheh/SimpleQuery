using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using static SimpleQuery.Tests.UnitTestDialectSqlite;

namespace SimpleQuery.Tests
{
    [TestClass]
    public class UnitTestDialectSQLServer
    {
        [TestMethod]
        public void TestSqlServerDeleteScript()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlDelete = builder.GetDeleteCommand<Cliente>(cliente, 1);
            var resultadoEsperado = "delete [Cliente] where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestSqlServerInsertScript()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 20, ValorTotalNotasFiscais = 2000.95, Credito = 10, UltimoValorDeCompra = 1000.95m };

            var sqlDelete = builder.GetInsertCommand<Cliente>(cliente);
            var resultadoEsperado = "insert into [Cliente] ([Nome], [Ativo], [TotalPedidos], [ValorTotalNotasFiscais], [Credito], [UltimoValorDeCompra]) values ('Moisés', 1, 20, 2000.95, 10, 1000.95)";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestSqlServerUpdateScript()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update [Cliente] set [Nome]='Moisés', [Ativo]=1, [TotalPedidos]=55, [ValorTotalNotasFiscais]=1000.55, [Credito]=2000.53, [UltimoValorDeCompra]=1035.22 where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestSqlServerUpdateScriptComValorNulo()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = null };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update [Cliente] set [Nome]='Moisés', [Ativo]=1, [TotalPedidos]=55, [ValorTotalNotasFiscais]=1000.55, [Credito]=2000.53, [UltimoValorDeCompra]=null where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestSqlServerSelectScript()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlUpdate = builder.GetSelectCommand<Cliente>(cliente);
            var resultadoEsperado = "select [Id], [Nome], [Ativo], [TotalPedidos], [ValorTotalNotasFiscais], [Credito], [UltimoValorDeCompra] from [Cliente]";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestExecuteQuerySqlServer()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    User user = new User() { Name = "Moisés", Email = "moises@gmail.com", Ratting = 10, Scores = 20 };
                    User user2 = new User() { Name = "Moisés", Email = "moises@gmail.com", Ratting = 10, Scores = 20 };
                    var builder = conn.GetScriptBuild();

                    conn.Execute(builder.GetCreateTableCommand<User>());

                    conn.Insert<User>(user);
                    conn.Insert<User>(user2);

                    var users = conn.Query<User>("select * from [User]");

                    Assert.AreEqual(2, users.Count());

                    conn.Execute("drop table [User]");
                }
            }
        }

        [TestCategory("CRUD")]
        [TestMethod]
        public void TestSelectWithWherePrimitiveTypesSqlServer()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    IScriptBuilder builder = new ScriptSqlServerBuilder();

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

                    conn.Execute("drop table [User]");

                    conn.Close();

                }
            }
        }

        [TestMethod]
        public void TestInsertOperationSqlServer()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();

            var trans = connection.BeginTransaction();
            using (var conn = connection)
            {
                IScriptBuilder builder = new ScriptSqlServerBuilder();

                var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>();
                builder.Execute(createTableScript, conn, trans);

                var lastId = conn.InsertReturningId<Cliente>(cliente, trans);
                Assert.AreEqual(1, lastId);

                trans.Rollback();
            }
        }

        [TestMethod]
        public void TestInsertOperationSqlServerNoEntityKey()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();

            var trans = connection.BeginTransaction();
            using (var conn = connection)
            {
                IScriptBuilder builder = new ScriptSqlServerBuilder();

                var conta = new ContaContabil() { Chave = "1.1", Name = "Ativo imobilizado" };

                var createTableScript = builder.GetCreateTableCommand<ContaContabil>();
                builder.Execute(createTableScript, conn, trans);

                conn.Insert<ContaContabil>(conta, false, trans);
                Assert.AreEqual("1.1", conta.Chave);

                trans.Rollback();
            }
        }

        [TestMethod]
        public void TestInsertOperationWithDatetimeSqlServer()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();


            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
            {
                conn.Open();
                var trans = conn.BeginTransaction();
                IScriptBuilder builder = new ScriptSqlServerBuilder();

                var doc = new Doc() { DocDate = new DateTime(2019, 02, 15, 23, 29, 29), Value = 1000 };

                var createTableScript = builder.GetCreateTableCommand<Doc>();
                builder.Execute(createTableScript, conn, trans);

                var lastId = conn.InsertReturningId<Doc>(doc, trans);
                Assert.AreEqual(1, lastId);

                trans.Rollback();

            }


        }

        [TestMethod]
        public void TestUpdateOperationWithDatetimeSqlServer()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();

            using (var transaction = new TransactionScope())
            {
                IScriptBuilder builder = new ScriptSqlServerBuilder();

                var doc = new Doc() { DocDate = new DateTime(2019, 02, 15, 23, 29, 29), Value = 1000 };

                var createTableScript = builder.GetCreateTableCommand<Doc>();
                builder.Execute(createTableScript, connection);

                var lastId = connection.Insert<Doc>(doc);

                doc.Value = 2000;
                connection.Update<Doc>(doc);
                var docFromDatabase = connection.Select<Doc>(c => c.Id == 1).FirstOrDefault();

                Assert.AreEqual(2000, docFromDatabase.Value);
                connection.Execute("drop table [Doc]");
                transaction.Complete();
            }
        }

        [TestMethod]
        public void TestSelectOperationSqlServer()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    IScriptBuilder builder = new ScriptSqlServerBuilder();

                    var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };
                    var cliente2 = new Cliente() { Id = 2, Nome = "José", Ativo = true };

                    var createTableScript = builder.GetCreateTableCommand<Cliente>();
                    var insertScript1 = builder.GetInsertCommand<Cliente>(cliente);
                    var insertScript2 = builder.GetInsertCommand<Cliente>(cliente2);
                    builder.Execute(createTableScript, conn);
                    builder.Execute(insertScript1, conn);
                    builder.Execute(insertScript2, conn);

                    var clientes = conn.GetAll<Cliente>();
                    Assert.AreEqual(2, clientes.Count());
                    Assert.AreEqual("Moisés", clientes.ToList()[0].Nome);
                    Assert.AreEqual("José", clientes.ToList()[1].Nome);

                    conn.Execute("drop table [Cliente]");
                }
            }
        }
        public string connstring => ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString;


        public void SaveCustomer()
        {
            var connection = new SqlConnection(connstring);
            var customer = new Customer { Name = "John" };

            connection.Insert(customer);

            customer.Name = "John Lennon";

            connection.Update(customer);

            var customers = connection.GetAll<Customer>();
        }


        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestCreateTableSqlServer()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var createTableScript = builder.GetCreateTableCommand<Cliente>();
            var resultadoEsperado = "create table [Cliente] ([Id] int not null identity, [Nome] nvarchar(255), [Ativo] bit, [TotalPedidos] int, [ValorTotalNotasFiscais] float, [Credito] decimal(18,6), [UltimoValorDeCompra] decimal(18,6), primary key ([Id]))";

            Assert.AreEqual(resultadoEsperado, createTableScript);
        }


        [TestMethod]
        public void TestInsertOperationWithStringKeySqlServer()
        {
            var carrier = new Carrier();

            using (var tran = new TransactionScope())
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    var createTableScript = conn.GetScriptBuild().GetCreateTableCommand<Carrier>();
                    conn.Execute(createTableScript);

                    conn.Insert<Carrier>(carrier);

                    conn.Execute("Drop table Carrier");

                }

                tran.Complete();
            }
        }

        [TestMethod]
        public void TestSelectWithSelectExentions()
        {
            var carrier = new Carrier();
            var createDate = new DateTime(2019, 07, 19);
            var updateDate = new DateTime(2019, 07, 25);
            using (var tran = new TransactionScope())
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString))
                {
                    var createTableScript = conn.GetScriptBuild().GetCreateTableCommand<Carrier>();
                    conn.Execute(createTableScript);

                    conn.Insert<Carrier>(carrier);

                    var result = conn.Select<Carrier>(c => c.CreateDate > createDate && c.UpdateDate <= updateDate && c.Name == "Teste");

                    conn.Execute("Drop table Carrier");

                }

                tran.Complete();
            }
        }

        public class Doc
        {
            public int Id { get; set; }
            public DateTime DocDate { get; set; }
            public decimal Value { get; set; }
        }

        public class Cliente
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public bool Ativo { get; set; }
            public int? TotalPedidos { get; set; }
            public double ValorTotalNotasFiscais { get; set; }
            public decimal Credito { get; set; }
            public decimal? UltimoValorDeCompra { get; set; }

            public List<string> Obs { get; set; }
            public ClienteEndereco Endereco { get; set; }
        }

        public class ClienteEndereco
        {

        }
    }

    public class Carrier
    {
        public Carrier()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Name = "Teste";
            this.CreateDate = new DateTime(2019, 07 , 20);
            this.UpdateDate = new DateTime(2019, 07, 25);
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// If true indicate the can't be removed
        /// </summary>
        public bool System { get; set; }

        public decimal Scores { get; set; } = 1m;
        public double Ratting { get; set; } = 1;

        public void SetPassword(string password, string confirmPassword)
        {

            if (password != confirmPassword)
                throw new Exception("Invalid password or passwords not matched");

        }
    }
}
