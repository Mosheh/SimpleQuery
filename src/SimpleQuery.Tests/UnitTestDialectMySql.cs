using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;

namespace SimpleQuery.Tests
{
    [TestClass]
    public class UnitTestDialectMySql
    {
        [TestMethod]
        public void TestMySqlDeleteScript()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlDelete = builder.GetDeleteCommand<Cliente>(cliente, 1);
            var resultadoEsperado = "delete from `Cliente` where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestMySqlInsertScript()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 20, ValorTotalNotasFiscais = 2000.95, Credito = 10, UltimoValorDeCompra = 1000.95m };

            var sqlDelete = builder.GetInsertCommand<Cliente>(cliente);
            var resultadoEsperado = "insert into `Cliente` (`Nome`, `Ativo`, `TotalPedidos`, `ValorTotalNotasFiscais`, `Credito`, `UltimoValorDeCompra`) values ('Moisés', 1, 20, 2000.95, 10, 1000.95)";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestMySqlUpdateScript()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update `Cliente` set `Nome`='Moisés', `Ativo`=1, `TotalPedidos`=55, `ValorTotalNotasFiscais`=1000.55, `Credito`=2000.53, `UltimoValorDeCompra`=1035.22 where `Id`=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestMySqlUpdateScriptComValorNulo()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = null };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update `Cliente` set `Nome`='Moisés', `Ativo`=1, `TotalPedidos`=55, `ValorTotalNotasFiscais`=1000.55, `Credito`=2000.53, `UltimoValorDeCompra`=null where `Id`=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestMySqlSelectScript()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlUpdate = builder.GetSelectCommand<Cliente>(cliente);
            var resultadoEsperado = "select `Id`, `Nome`, `Ativo`, `TotalPedidos`, `ValorTotalNotasFiscais`, `Credito`, `UltimoValorDeCompra` from `Cliente`";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestMySqlSelectWithWhereScript()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlUpdate = builder.GetSelectCommand<Cliente>(cliente, c=>c.Id == 1);
            var resultadoEsperado = "select `Id`, `Nome`, `Ativo`, `TotalPedidos`, `ValorTotalNotasFiscais`, `Credito`, `UltimoValorDeCompra` from `Cliente` where (`Id` = 1)";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestInsertOperationMySql()
        {
            var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
            connection.Open();

            var trans = connection.BeginTransaction();
            using (var conn = connection)
            {
                IScriptBuilder builder = new ScriptMySqlBuilder();

                var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>();
                builder.Execute(createTableScript, conn, trans);

                var lastId = conn.InsertReturningId<Cliente>(cliente, trans);
                Assert.AreEqual(1, lastId);

                trans.Rollback();

                conn.Execute("drop table `Cliente`");
            }
        }



        [TestMethod]
        public void TestUpdateOperationMySql()
        {
            var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
            connection.Open();

            var trans = connection.BeginTransaction();
            using (var conn = connection)
            {
                IScriptBuilder builder = new ScriptMySqlBuilder();

                var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>();
                builder.Execute(createTableScript, conn, trans);

                var lastId = conn.InsertReturningId<Cliente>(cliente, trans);
                Assert.AreEqual(1, lastId);

                cliente.Id = lastId;
                cliente.Nome = "João";
                conn.Update<Cliente>(cliente);
                trans.Rollback();

                conn.Execute("drop table `Cliente`");
            }
        }

        [TestMethod]
        public void TestSelectOperationMySql()
        {
            var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
            connection.Open();

            using (var scope = new TransactionScope())
            {
                using (var conn = connection)
                {
                    IScriptBuilder builder = new ScriptMySqlBuilder();

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

                    conn.Execute("drop table `Cliente`");
                }
            }
        }

        [TestMethod]
        public void TestMySqlCreateTable()
        {
            IScriptBuilder builder = new ScriptMySqlBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var createTableScript = builder.GetCreateTableCommand<Cliente>();
            var resultadoEsperado = "create table `Cliente` (`Id` int not null auto_increment, `Nome` nvarchar(255), `Ativo` boolean, `TotalPedidos` int, `ValorTotalNotasFiscais` double, `Credito` decimal(18,6), `UltimoValorDeCompra` decimal(18,6) null, primary key (`Id`))";

            Assert.AreEqual(resultadoEsperado, createTableScript);
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
        }
    }
}
