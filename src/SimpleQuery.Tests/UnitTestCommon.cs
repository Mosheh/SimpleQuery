using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQuery.Data.Dialects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Tests
{
    [TestClass]
    public class UnitTestCommon
    {
        [TestMethod]
        public void TestKeyAttrib()
        {
            var conn = new SqlConnection();
            var keyProp = conn.GetScriptBuild().GetKeyPropertyModel<Dog>();

            Assert.AreEqual("Code", keyProp.Name);
        }

        public class Dog
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int Code { get; set; }
            public string Name { get; set; }

            [System.ComponentModel.DataAnnotations.Schema.NotMapped]
            public int Age { get; set; }
        }
    }
}
