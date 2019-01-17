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
            var keyProperty = allProperties.ToList().Find(c => c.Name.ToUpper() == "ID" || c.Name.ToUpper() == "DocEntry" ||
            c.GetCustomAttributes().Any(p=>p is System.ComponentModel.DataAnnotations.KeyAttribute));

            return keyProperty;
        }

        public virtual PropertyInfo GetKeyProperty(IEnumerable<PropertyInfo> allProperties)
        {
            var keyProperty = GetKeyProperty(allProperties.ToArray<PropertyInfo>());

            return keyProperty;
        }

        public virtual PropertyInfo GetKeyPropertyModel<T>() where T : class, new()
        {
            return GetKeyProperty(new T().GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList());
        }

        internal static bool IsIgnoreProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes().Where(c => c is System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute).Any();
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
            typeof(bool),
            typeof(bool?),
            typeof(byte),
            typeof(sbyte),
            typeof(ushort),
            typeof(short),
            typeof(short?),
            typeof(uint),
            typeof(int),
            typeof(int?),
            typeof(ulong),
            typeof(long),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?),
            typeof(DateTime),
            typeof(DateTime?)
        }.Contains(t);
        }
    }
}
