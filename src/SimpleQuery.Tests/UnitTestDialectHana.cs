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
            var resultadoEsperado = "delete from \"Cliente\" where \"Id\"=1";

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
        public void TestHanaInsertContractTableScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var contract = TestData.GetContract();

            var sqlInsertCommand = builder.GetInsertCommand(contract);
            var resultadoEsperado = $"insert into \"Contract\" (\"Status\", \"ContractDate\", \"Proposal\", \"BusinessPartner\", \"BusinessPartnerName\", \"TypeContract\", \"Undertaking\", \"UndertakingName\", \"UndertakingBlock\", \"UndertakingUnit\", \"UndertakingUnitName\", \"Property\", \"PropertyRate\", \"Classification\", \"ClassificationDate\", \"RenegotiationRate\", \"AnticipationRate\", \"AnticipationRateTP\", \"TypeFineResidue\", \"FineRateResidue\", \"DeliveryDate\", \"SignatureDate\", \"CostCenter\", \"ContractOrigin\", \"ProRataLow\", \"ProRataDiscount\", \"ProRataLowDelay\", \"ProRataResidue\", \"ProRataAnticipation\", \"DirectComission\", \"TotalComission\", \"GenerateCreeditLetterOnOverPayment\", \"StatusDate\", \"Comments\", \"TypeCalcMora\", \"DocTotal\", \"SaleValue\", \"AssignmentRights\", \"AdjustmentOnlyCalculateDueDate\", \"NotGenerateNegativeMonetaryCorrection\", \"LagOfInterestDay\", \"LagOfFine\", \"AccPeriodId\", \"MonthsOfGrowthForCorrectionYearly\", \"AccountNumber\", \"Accounted\", \"InstructionBoE\", \"BlockPaymentWithFutureDate\", \"CostUnit\", \"EnableDiscountPunctuality\", \"NumberInstallmentsPunctuality\", \"RealEstateTransferDays\", \"AdministrationRate\", \"RentalTransferGuaranteed\") values (5, '{DateTime.Now.ToString("yyyy-MM-dd")}', 5, 'C001', 'MOISÉS J. MIRANDA', 1, 2, 'Gran Ville', '15', 21, 'GRAN House', 5, 1.2, 5, null, 2, 1.5, 1.6, null, 2, '2019-01-16', '{DateTime.Now.ToString("yyyy-MM-dd")}', '1.1', null, true, false, null, null, true, 5000, 1000, false, '{DateTime.Now.ToString("yyyy-MM-dd")}', 'comments', 1, 25382000.99, 30000000.00, false, true, true, 0, 0, 1, 1, 123, true, 'payment credit card', true, 830, true, 12, 10, 2, false)";

            Assert.AreEqual(resultadoEsperado, sqlInsertCommand);
        }

        [TestMethod]
        public void TestHanaWhere()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var whereScript =  builder.GetWhereCommand<Cliente>(c=>c.Id == 1);
            var resultadoEsperado = "where (\"Id\" = 1)";

            Assert.AreEqual(resultadoEsperado, whereScript);
        }

        [TestMethod]
        public void TestHanaUpdateScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = 1035.22m };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update \"Cliente\" set \"Nome\"='Moisés', \"Ativo\"=true, \"TotalPedidos\"=55, \"ValorTotalNotasFiscais\"=1000.55, \"Credito\"=2000.53, \"UltimoValorDeCompra\"=1035.22 where \"Id\"=1";

            Assert.AreEqual(resultadoEsperado, sqlUpdate);
        }

        [TestMethod]
        public void TestHanaUpdateScriptComValorNulo()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true, TotalPedidos = 55, ValorTotalNotasFiscais = 1000.55, Credito = 2000.53m, UltimoValorDeCompra = null };

            var sqlUpdate = builder.GetUpdateCommand<Cliente>(cliente);
            var resultadoEsperado = "update \"Cliente\" set \"Nome\"='Moisés', \"Ativo\"=true, \"TotalPedidos\"=55, \"ValorTotalNotasFiscais\"=1000.55, \"Credito\"=2000.53, \"UltimoValorDeCompra\"=null where \"Id\"=1";

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
        public void TestHanaSelectWithWhereScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var selectWithWhereCommand = builder.GetSelectCommand<Cliente>(cliente, c=>c.Id == 1);
            var resultadoEsperado = "select \"Id\", \"Nome\", \"Ativo\", \"TotalPedidos\", \"ValorTotalNotasFiscais\", \"Credito\", \"UltimoValorDeCompra\" from \"Cliente\" where (\"Id\" = 1)";

            Assert.AreEqual(resultadoEsperado, selectWithWhereCommand);
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

                var createTableScript = builder.GetCreateTableCommand<Cliente>();
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

                trans.Rollback();
                builder.Execute("drop table \"Cliente\"", hanaConnection);
            }
        }

        [TestMethod]
        public void TestHanaCreateTableScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var cliente = new Cliente() { Id = 1, Nome = "Moisés", Ativo = true };

            var createTableScript = builder.GetCreateTableCommand<Cliente>();
            var resultadoEsperado = "create table \"Cliente\" (\"Id\" INTEGER not null primary key generated by default as IDENTITY, \"Nome\" VARCHAR(255), \"Ativo\" BOOLEAN, \"TotalPedidos\" INTEGER, \"ValorTotalNotasFiscais\" DOUBLE, \"Credito\" DECIMAL(18,6), \"UltimoValorDeCompra\" DECIMAL(18,6))";

            Assert.AreEqual(resultadoEsperado, createTableScript);
        }

        [TestMethod]
        public void TestHanaCreateContractTableScript()
        {
            IScriptBuilder builder = new ScriptHanaBuilder();

            var contract = new Contract() { ID = 1, BusinessPartner = "123", DocTotal = 10000, SignatureDate = DateTime.Now, ContractDate = DateTime.Now };

            var createTableScript = builder.GetCreateTableCommand<Contract>();
            var resultadoEsperado = "create table \"Contract\" (\"ID\" INTEGER not null primary key generated by default as IDENTITY, \"Status\" INTEGER, \"ContractDate\" Date, \"Proposal\" INTEGER, \"BusinessPartner\" VARCHAR(255), \"BusinessPartnerName\" VARCHAR(255), \"TypeContract\" INTEGER, \"Undertaking\" INTEGER, \"UndertakingName\" VARCHAR(255), \"UndertakingBlock\" VARCHAR(255), \"UndertakingUnit\" INTEGER, \"UndertakingUnitName\" VARCHAR(255), \"Property\" DECIMAL(18,6), \"PropertyRate\" DECIMAL(18,6), \"Classification\" INTEGER, \"ClassificationDate\" Date, \"RenegotiationRate\" DECIMAL(18,6), \"AnticipationRate\" DECIMAL(18,6), \"AnticipationRateTP\" DECIMAL(18,6), \"TypeFineResidue\" INTEGER, \"FineRateResidue\" DECIMAL(18,6), \"DeliveryDate\" Date, \"SignatureDate\" Date, \"CostCenter\" VARCHAR(255), \"ContractOrigin\" INTEGER, \"ProRataLow\" BOOLEAN, \"ProRataDiscount\" BOOLEAN, \"ProRataLowDelay\" BOOLEAN, \"ProRataResidue\" BOOLEAN, \"ProRataAnticipation\" BOOLEAN, \"DirectComission\" DECIMAL(18,6), \"TotalComission\" DECIMAL(18,6), \"GenerateCreeditLetterOnOverPayment\" BOOLEAN, \"StatusDate\" Date, \"Comments\" VARCHAR(255), \"TypeCalcMora\" INTEGER, \"DocTotal\" DECIMAL(18,6), \"SaleValue\" DECIMAL(18,6), \"AssignmentRights\" BOOLEAN, \"AdjustmentOnlyCalculateDueDate\" BOOLEAN, \"NotGenerateNegativeMonetaryCorrection\" BOOLEAN, \"LagOfInterestDay\" INTEGER, \"LagOfFine\" INTEGER, \"AccPeriodId\" INTEGER, \"MonthsOfGrowthForCorrectionYearly\" INTEGER, \"AccountNumber\" INTEGER, \"Accounted\" BOOLEAN, \"InstructionBoE\" VARCHAR(255), \"BlockPaymentWithFutureDate\" BOOLEAN, \"CostUnit\" DECIMAL(18,6), \"EnableDiscountPunctuality\" BOOLEAN, \"NumberInstallmentsPunctuality\" INTEGER, \"RealEstateTransferDays\" INTEGER, \"AdministrationRate\" DECIMAL(18,6), \"RentalTransferGuaranteed\" BOOLEAN)";

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
