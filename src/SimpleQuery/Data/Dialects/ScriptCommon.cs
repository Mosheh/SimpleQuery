using SimpleQuery.Anottations;
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
            var keyProperty = allProperties.ToList().Find(c => c.Name.ToUpper() == "ID" || c.Name.ToUpper() == "DocEntry");

            return keyProperty;
        }

        public virtual PropertyInfo GetKeyPropertyModel<T>() where T : class, new()
        {
            return GetKeyProperty(new T().GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public));
        }

        internal static bool IsIgnoreProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes().Where(c => c is Ignore).Any();

        }

        internal static IEnumerable<PropertyInfo> GetValidProperty<T>() where T:class, new()
        {
            return new T().GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList().Where(
                p=> IsPrimitive(p.PropertyType)
                && !IsIgnoreProperty(p));
        }

        internal static bool IsPrimitive(Type t)
        {
            // TODO: put any type here that you consider as primitive as I didn't
            // quite understand what your definition of primitive type is
            return new[] {
            typeof(string),
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(ushort),
            typeof(short),
            typeof(uint),
            typeof(int),
            typeof(ulong),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
        }.Contains(t);
        }
    }
}
