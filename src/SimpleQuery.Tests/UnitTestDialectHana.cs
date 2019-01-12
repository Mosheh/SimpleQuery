using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SimpleQuery.Tests
{
    [TestClass]
    public class UnitTestDialectHana
    {
        [TestMethod]
        public void TestHanaDeleteScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlDelete = builder.GetDeleteCommand<Cliente>(cliente, 1);
            var resultadoEsperado = "delete \"Cliente\" where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestHanaInsertScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 20, ValorTotalNotasFiscais = 2000.95, Credito = 10, UltimoValorDeCompra = 1000.95m };

            var sqlDelete = builder.GetInsertCommand<Cliente>(cliente);
            var resultadoEsperado = "insert into \"Cliente\" (\"Nome\", \"Ativo\", \"TotalPedidos\", \"ValorTotalNotasFiscais\", \"Credito\", \"UltimoValorDeCompra\") values ('Moisés', true, 20, 2000.95, 10, 1000.95)";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestHanaUpdateScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update \"Cliente\" set \"Nome\"='Moisés', set \"Ativo\"=true, set \"TotalPedidos\"=55, set \"ValorTotalNotasFiscais\"=1000.55, set \"Credito\"=2000.53, set \"UltimoValorDeCompra\"=1035.22 where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestHanaUpdateScriptComValorNulo()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = null };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update \"Cliente\" set \"Nome\"='Moisés', set \"Ativo\"=true, set \"TotalPedidos\"=55, set \"ValorTotalNotasFiscais\"=1000.55, set \"Credito\"=2000.53, set \"UltimoValorDeCompra\"=null where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestHanaSelectScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var sqlUpdate = builder.GetSelectCommand<Cliente>(cliente);
            var resultadoEsperado = "select \"Id\", \"Nome\", \"Ativo\", \"TotalPedidos\", \"ValorTotalNotasFiscais\", \"Credito\", \"UltimoValorDeCompra\" from \"Cliente\"";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestInsertOperationHana()
        {
            var hanaConnection = new Sap.Data.Hana.HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            hanaConnection.Open();
            var trans = hanaConnection.BeginTransaction();
            using (var conn = hanaConnection)
            {
                IScriptBuilder builder = new ScriptHanaBuilder();

                var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>(cliente);
                builder.Execute(createTableScript, conn);

                var lastId = conn.InsertRereturnId<Cliente>(cliente);
                Assert.AreEqual(1, lastId);

                trans.Rollback();
                builder.Execute("drop table \"Cliente\"", hanaConnection);
            }
        }

        [TestMethod]
        public void TestSelectOperationHana()
        {
            var hanaConnection = new Sap.Data.Hana.HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            hanaConnection.Open();
            var trans = hanaConnection.BeginTransaction();
            using (var conn = hanaConnection)
            {
                IScriptBuilder builder = new ScriptHanaBuilder();

                var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };
                var cliente2 = new Cliente() { Id = 2, Nome = "José", Ativo = true };

                var createTableScript = builder.GetCreateTableCommand<Cliente>(cliente);
                var insertScript1 = builder.GetInsertCommand<Cliente>(cliente);
                var insertScript2 = builder.GetInsertCommand<Cliente>(cliente2);
                builder.Execute(createTableScript, conn);
                builder.Execute(insertScript1, conn);
                builder.Execute(insertScript2, conn);
                
                var clientes = conn.GetAll<Cliente>(cliente);
                Assert.AreEqual(2, clientes.Count());
                Assert.AreEqual("Moisés", clientes.ToList()[0].Nome);
                Assert.AreEqual("José", clientes.ToList()[1].Nome);

                trans.Rollback();
                builder.Execute("drop table \"Cliente\"", hanaConnection);
            }
        }

        [TestMethod]
        public void TestCreateTableHana()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var createTableScript = builder.GetCreateTableCommand<Cliente>(cliente);
            var resultadoEsperado = "create table \"Cliente\" (\"Id\" INTEGER not null primary key generated by default as IDENTITY, \"Nome\" VARCHAR(255), \"Ativo\" BOOLEAN, \"TotalPedidos\" INTEGER, \"ValorTotalNotasFiscais\" DOUBLE, \"Credito\" DECIMAL(18,6), \"UltimoValorDeCompra\" DECIMAL(18,6))";

            Assert.AreEqual(resultadoEsperado, createTableScript);
        }

        public class Cliente
        {
            private string HashId { get; set; }
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
