using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleQuery.Tests
{
    [TestClass]
    public class UnitTestDialectSqlite
    {
        [TestInitialize]
        public void Setup()
        {
            var fileName = GetFileNameDb();

            SQLiteConnection.CreateFile(fileName);
        }

        private string GetFileNameDb()
        {
            var fileDbTest = "SqliteTest.db";
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var fileName = Path.Combine(path, fileDbTest);
            return fileName;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(GetFileNameDb());
        }

        [TestMethod]
        public void TestSqliteDeleteScript()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlDelete = builder.GetDeleteCommand<Cliente>(cliente, 1);
            var resultadoEsperado = "delete from [Cliente] where [Id]=1";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestSqliteInsertScript()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, DataCadastro = DateTime.Now, Nome = "Moisés", Ativo = true, TotalPedidos = 20, ValorTotalNotasFiscais = 2000.95, Credito = 10, UltimoValorDeCompra = 1000.95m };

            var insertScript = builder.GetInsertCommand<Cliente>(cliente);
            var resultadoEsperado = $"insert into [Cliente] ([DataCadastro], [Nome], [Ativo], [TotalPedidos], [ValorTotalNotasFiscais], [Credito], [UltimoValorDeCompra]) values ('{cliente.DataCadastro.ToString("yyyy-MM-dd")}', 'Moisés', 1, 20, 2000.95, 10, 1000.95)";

            Assert.AreEqual(resultadoEsperado, insertScript);
        }

        [TestMethod]
        public void TestSqliteUpdateScript()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update [Cliente] set [DataCadastro]='0001-01-01', [Nome]='Moisés', [Ativo]=1, [TotalPedidos]=55, [ValorTotalNotasFiscais]=1000.55, [Credito]=2000.53, [UltimoValorDeCompra]=1035.22 where [Id]=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestSqliteUpdateScriptComValorNulo()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = null };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update [Cliente] set [DataCadastro]='0001-01-01', [Nome]='Moisés', [Ativo]=1, [TotalPedidos]=55, [ValorTotalNotasFiscais]=1000.55, [Credito]=2000.53, [UltimoValorDeCompra]=null where [Id]=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestSqliteSelectScript()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlUpdate = builder.GetSelectCommand<Cliente>(cliente);
            var resultadoEsperado = "select [Id], [DataCadastro], [Nome], [Ativo], [TotalPedidos], [ValorTotalNotasFiscais], [Credito], [UltimoValorDeCompra] from [Cliente]";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }
        [TestCategory("CRUD")]
        [TestMethod]
        public void TestInsertOperationSqlite()
        {
            var connection = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            connection.Open();

            var trans = connection.BeginTransaction();
            using (var conn = connection)
            {
                IScriptBuilder builder = new ScriptSqliteBuilder();

                var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>();
                builder.Execute(createTableScript, conn, trans);

                var lastId = conn.InsertRereturnId<Cliente>(cliente, trans);
                Assert.AreEqual(1, lastId);
                trans.Rollback();
                conn.ReleaseMemory();

                conn.Close();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        [TestCategory("CRUD")]
        [TestMethod]
        public void TestDeleteOperation()
        {
            var connection = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            connection.Open();

            var trans = connection.BeginTransaction();
            using (var conn = connection)
            {
                IScriptBuilder builder = new ScriptSqliteBuilder();

                var cliente = new Cliente() { Nome = "Moisés", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>();
                builder.Execute(createTableScript, conn, trans);

                var lastId = conn.InsertRereturnId<Cliente>(cliente, trans);
                Assert.AreEqual(1, lastId);

                var clienteByDatabase = conn.Select<Cliente>(c => c.Id == lastId).First();

                conn.Delete<Cliente>(clienteByDatabase);
                trans.Rollback();
                conn.ReleaseMemory();

                conn.Close();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        [TestCategory("CRUD")]
        [TestMethod]
        public void TestSelectOperationSqlite()
        {
            var connection = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    IScriptBuilder builder = new ScriptSqliteBuilder();

                    var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, DataCadastro = DateTime.Now };
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
                    conn.ReleaseMemory();
                    conn.Close();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
        [TestCategory("CRUD")]
        [TestMethod]
        public void TestSelectWithWhereMailOperationSqlite()
        {
            var connection = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    IScriptBuilder builder = new ScriptSqliteBuilder();

                    User user = new User() { Id = 1, Name = "Moisés", Email = "moises@gmail.com" };
                    User user2 = new User() { Id = 1, Name = "Miranda", Email = "miranda@gmail.com" };

                    var createTableScript = builder.GetCreateTableCommand<User>();
                    builder.Execute(createTableScript, conn);
                    conn.Insert(user);
                    conn.Insert(user2);
                    var users = conn.Select<User>(c => c.Email == user.Email);
                    Assert.AreEqual(1, users.Count());
                    Assert.AreEqual("Moisés", users.ToList()[0].Name);
                    Assert.AreEqual(1, users.ToList()[0].Id);
                    
                    conn.Execute("drop table [User]");
                    conn.ReleaseMemory();
                    conn.Close();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        [TestCategory("CRUD")]
        [TestMethod]
        public void TestSelectWithWherePrimitiveTypesSqlite()
        {
            var connection = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    IScriptBuilder builder = new ScriptSqliteBuilder();

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
                    var userThird = conn.Select<User>(c => c.System==true);
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
                    conn.ReleaseMemory();
                    conn.Close();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        [TestMethod]
        public void TestCreateTableSqlite()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var createTableScript = builder.GetCreateTableCommand<Cliente>();
            var resultadoEsperado = "create table [Cliente] ([Id] integer not null primary key autoincrement, [DataCadastro] datetime, [Nome] text, [Ativo] boolean, [TotalPedidos] integer null, [ValorTotalNotasFiscais] double, [Credito] decimal(18,6), [UltimoValorDeCompra] decimal(18,6) null)";

            Assert.AreEqual(resultadoEsperado, createTableScript);
        }

        [TestMethod]
        public void TestInsertFileSqlite()
        {
            var connection = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            
            connection.Open();

            using (var conn = connection)
            {
                var builder = conn.GetScriptBuild();
                var archive = new ArchiveModel() { Content = Properties.Resources.conduites_1 };

                var createTableScript = builder.GetCreateTableCommand<ArchiveModel>();
                conn.Execute(createTableScript);

                conn.Insert<ArchiveModel>(archive);

                conn.Execute("drop table [ArchiveModel]");

                conn.ReleaseMemory();
                conn.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public class Cliente
        {
            public int Id { get; set; }
            public DateTime DataCadastro { get; set; }
            public string Nome { get; set; }
            public bool Ativo { get; set; }
            public int? TotalPedidos { get; set; }
            public double ValorTotalNotasFiscais { get; set; }
            public decimal Credito { get; set; }
            public decimal? UltimoValorDeCompra { get; set; }
            public ClienteEndereco Endereco { get; set; }
            public List<int> MyProperty { get; set; }
        }

        public class ClienteEndereco
        {
            public ClienteEndereco()
            {

            }
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

        public class ArchiveModel
        {
            public int Id { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
