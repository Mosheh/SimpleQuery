using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQuery.Data.Dialects;
using SimpleQuery.Domain.Data.Dialects;

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
            var resultadoEsperado = "insert into [Cliente] ([Nome], [Ativo], [TotalPedidos], [ValorTotalNotasFiscais], [Credito], [UltimoValorDeCompra]) values ('Moisés', true, 20, 2000.95, 10, 1000.95)";

            Assert.AreEqual(resultadoEsperado, sqlDelete);
        }

        [TestMethod]
        public void TestSqlServerUpdateScript()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update [Cliente] set [Nome]='Moisés', set [Ativo]=true, set [TotalPedidos]=55, set [ValorTotalNotasFiscais]=1000.55, set [Credito]=2000.53, set [UltimoValorDeCompra]=1035.22 where Id=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }



        [TestMethod]
        public void TestSqlServerUpdateScriptComValorNulo()
        {
            IScriptBuilder builder = new ScriptSqlServerBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = null };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update [Cliente] set [Nome]='Moisés', set [Ativo]=true, set [TotalPedidos]=55, set [ValorTotalNotasFiscais]=1000.55, set [Credito]=2000.53, set [UltimoValorDeCompra]=null where Id=1";

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
