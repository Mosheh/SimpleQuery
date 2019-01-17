using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Anottations
{
    public class Identity : Attribute
    {
        public string identyKey { get; private set; }
        public Identity(string key)
        {
            identyKey = key;
        }
    }
}
