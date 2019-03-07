using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    /// <summary>
    /// Paramter for DbCommand
    /// </summary>
    public class DbSimpleParameter : System.Data.Common.DbParameter
    {
        /// <summary>
        /// Paramter for DbCommand
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter database type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="value">Paramter value</param>
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
        /// <summary>
        /// Database type
        /// </summary>
        public override DbType DbType { get; set; }
        /// <summary>
        /// Parameter direction
        /// </summary>
        public override ParameterDirection Direction { get; set; }
        /// <summary>
        /// Get or set if parameter is nullable
        /// </summary>
        public override bool IsNullable { get; set; }
        /// <summary>
        /// Parameter name
        /// </summary>
        public override string ParameterName { get; set; }
        /// <summary>
        /// Paramter size
        /// </summary>
        public override int Size { get; set; }
        /// <summary>
        /// Source column name
        /// </summary>
        public override string SourceColumn { get; set; }
        /// <summary>
        /// Source column for null value
        /// </summary>
        public override bool SourceColumnNullMapping { get; set; }
        /// <summary>
        /// Data row version
        /// </summary>
        public override DataRowVersion SourceVersion { get; set; }
        /// <summary>
        /// Parameter value
        /// </summary>
        public override object Value { get; set; }

        public override void ResetDbType()
        {
            
        }
    }
}
