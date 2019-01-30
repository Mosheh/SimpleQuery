using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    public class DbSimpleParameter : System.Data.Common.DbParameter
    {
        public DbSimpleParameter(string name, DbType type, int size, object value)
        {
            this.ParameterName = name;
            this.DbType = type;
            this.Size = size;
            this.Value = value;
            SetIsNullable();
        }

        private void SetIsNullable()
        {
            
        }

        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; }
        public override int Size { get; set; }
        public override string SourceColumn { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override DataRowVersion SourceVersion { get; set; }
        public override object Value { get; set; }

        public override void ResetDbType()
        {
            
        }
    }
}
