using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Tests
{
    public partial class UnitTestExtentions
    {
        [TestMethod]
        public void SapHanaUpdateModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();

            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            cliente.Id = id;
            cliente.Nome = "Moisés Miranda";
            conn.Update<Cliente>(cliente);

            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void SapHanaUpdateContractModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>();

            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Contract>(contract);

            contract.ID = id;
            contract.BusinessPartnerName = "Moisés José Miranda";
            conn.Update<Contract>(contract);

            conn.Execute("drop table \"Contract\"");
        }

        [TestMethod]
        public void SapHanaInsertModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);
            Assert.AreEqual(1, id);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void SapHanaInsertContractModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>();
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Contract>(contract);
            Assert.AreEqual(1, id);
            conn.Execute("drop table \"Contract\"");
        }

        [TestMethod]
        public void SapHanaInsertModelFillIdModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            conn.Insert<Cliente>(cliente);
            Assert.AreEqual(1, cliente.Id);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void SapHanaInsertContractModelFillId()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>();
            conn.Execute(createTableScript);
            conn.Insert<Contract>(contract);
            Assert.AreEqual(1, contract.ID);
            conn.Execute("drop table \"Contract\"");
        }

        [TestMethod]
        public void SapHanaSelectModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            var clientes = conn.GetAll<Cliente>();

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table \"Cliente\"");
        }

        [TestMethod]
        public void SapHanaSelectContractModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>();
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Contract>(contract);

            var contracts = conn.GetAll<Contract>();

            Assert.AreEqual(1, contracts.Count());
            Assert.AreEqual("MOISÉS J. MIRANDA", contracts.ToList()[0].BusinessPartnerName);
            conn.Execute("drop table \"Contract\"");
        }

        [TestMethod]
        public void SapHanaSelectContractWhereName()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>();
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Contract>(contract);

            var contracts = conn.Select<Contract>(c=>c.BusinessPartnerName == "MOISÉS J. MIRANDA");

            Assert.AreEqual(1, contracts.Count());
            Assert.AreEqual("MOISÉS J. MIRANDA", contracts.ToList()[0].BusinessPartnerName);
            conn.Execute("drop table \"Contract\"");
        }

        [TestMethod]
        public void SapHanaDeleteModel()
        {
            var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>();
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);
            cliente.Id = id;
            conn.Delete(cliente);

            conn.Execute("drop table \"Cliente\"");
        }
    }
}
