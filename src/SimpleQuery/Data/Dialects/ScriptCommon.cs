using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleQuery.Data.Dialects
{
    public abstract class ScriptCommon
    {
        public virtual PropertyInfo GetKeyProperty(PropertyInfo[] allProperties)
        {
            var keyProperty =  allProperties.ToList().Find(c => c.Name.ToUpper() == "ID" || c.Name.ToUpper() == "DocEntry");
            
            return keyProperty;
        }

        public virtual PropertyInfo GetKeyPropertyModel<T>() where T: class, new()
        {
            return GetKeyProperty(new T().GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public));
        }
    }
}
