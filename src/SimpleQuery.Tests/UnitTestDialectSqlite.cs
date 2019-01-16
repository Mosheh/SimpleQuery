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

            var cliente = new Cliente() { Id = 1, DataCadastro=DateTime.Now, Nome = "Moisés", Ativo = true, TotalPedidos = 20, ValorTotalNotasFiscais = 2000.95, Credito = 10, UltimoValorDeCompra = 1000.95m };

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

                var createTableScript = builder.GetCreateTableCommand<Cliente>(cliente);
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

                    var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, DataCadastro=DateTime.Now };
                    var cliente2 = new Cliente() { Id = 2, Nome = "José", Ativo = true };

                    var createTableScript = builder.GetCreateTableCommand<Cliente>(cliente);
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

        [TestMethod]
        public void TestCreateTableSqlite()
        {
            IScriptBuilder builder = new ScriptSqliteBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var createTableScript = builder.GetCreateTableCommand<Cliente>(cliente);
            var resultadoEsperado = "create table [Cliente] ([Id] integer not null primary key autoincrement, [DataCadastro] datetime, [Nome] text, [Ativo] boolean, [TotalPedidos] integer null, [ValorTotalNotasFiscais] double, [Credito] decimal(18,6), [UltimoValorDeCompra] decimal(18,6) null)";

            Assert.AreEqual(resultadoEsperado, createTableScript);
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
        }
    }
}
