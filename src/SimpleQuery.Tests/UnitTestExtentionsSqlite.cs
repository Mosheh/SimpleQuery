using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Tests
{
    public partial class UnitTestExtentions
    {
        [TestInitialize]
        public void Setup()
        {
            var fileName = GetFileNameDb();
            File.Delete(fileName);
            SQLiteConnection.CreateFile(fileName);
        }

        [TestCleanup]
        public void Clear()
        {
            var fileName = GetFileNameDb();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        [TestMethod]
        public void SqliteUpdateModel()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);

            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            cliente.Id = id;
            cliente.Nome = "Moisés Miranda";
            conn.Update<Cliente>(cliente);

            conn.Execute("drop table [Cliente]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SqliteInsertModel()
        {

            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");

            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);
            Assert.AreEqual(1, id);
            conn.Execute("drop table [Cliente]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private string GetFileNameDb()
        {
            var appFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileDb = "Test.db";
            var fileName = Path.Combine(appFolder, fileDb);
            return fileName;
        }

        [TestMethod]
        public void SqliteInsertModelFillingId()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            conn.Insert<Cliente>(cliente);
            Assert.AreEqual(1, cliente.Id);
            conn.Execute("drop table [Cliente]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SqliteInsertContractModelFillingId()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>(contract);
            conn.Execute(createTableScript);
            conn.Insert<Contract>(contract);
            Assert.AreEqual(1, contract.ID);
            conn.Execute("drop table [Contract]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SqliteSelectModel()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            var clientes = conn.GetAll<Cliente>();

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table [Cliente]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SqliteSelectWithWhereCondition()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);

            var clientes = conn.Select<Cliente>(c=> c.Id==1);

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("Miranda", clientes.ToList()[0].Nome);
            conn.Execute("drop table [Cliente]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SqliteSelectContractModel()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var contract = TestData.GetContract();

            var createTableScript = scriptBuilder.GetCreateTableCommand<Contract>(contract);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Contract>(contract);

            var clientes = conn.GetAll<Contract>();

            Assert.AreEqual(1, clientes.Count());
            Assert.AreEqual("MOISÉS J. MIRANDA", clientes.ToList()[0].BusinessPartnerName);
            conn.Execute("drop table [Contract]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SqliteDeleteModel()
        {
            var conn = new SQLiteConnection($"Data Source={GetFileNameDb()}");
            var scriptBuilder = conn.GetScriptBuild();

            var cliente = new Cliente() { Nome = "Miranda" };

            var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
            conn.Execute(createTableScript);
            var id = conn.InsertRereturnId<Cliente>(cliente);
            cliente.Id = id;
            conn.Delete(cliente);

            conn.Execute("drop table [Cliente]");

            conn.ReleaseMemory();
            conn.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
