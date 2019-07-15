using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Domain.Data
{
    public enum DbServerType
    {
        SqlServer,
        Sqlite,
        Hana,
        PostGres,
        MySql,
        Oracle,
        FbAdapter,
        MaxDb,
        Ansi
    }
}
