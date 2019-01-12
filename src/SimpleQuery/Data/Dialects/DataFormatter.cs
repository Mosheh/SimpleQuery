using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SimpleQuery.Data.Dialects
{
    public static class DataFormatter
    {
        internal static int GetColumnHash(IDataReader reader, int startBound = 0, int length = -1)
        {
            unchecked
            {
                int max = length < 0 ? reader.FieldCount : startBound + length;
                int hash = (-37 * startBound) + max;
                for (int i = startBound; i < max; i++)
                {
                    object tmp = reader.GetName(i);
                    hash = (-79 * ((hash * 31) + (tmp?.GetHashCode() ?? 0))) + (reader.GetFieldType(i)?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
        public static  object GetValue<T>(PropertyInfo item, T obj) where T : class, new()
        {

            switch (item.PropertyType.Name)
            {

                case "String":
                    return $"'{item.GetValue(obj)}'";
                case "Boolean":
                    return ((bool)item.GetValue(obj)) == true ? "true" : "false";
                case "Nullable`1":
                    return GetNullableValue(item, obj);
                case "Double":
                    var nfiDouble = new NumberFormatInfo();
                    nfiDouble.NumberDecimalSeparator = ".";
                    return Convert.ToDouble(item.GetValue(obj)).ToString(nfiDouble);
                case "Decimal":
                    var nfiDecimal = new NumberFormatInfo();
                    nfiDecimal.NumberDecimalSeparator = ".";
                    return Convert.ToDecimal(item.GetValue(obj)).ToString(nfiDecimal);
                default:
                    return item.GetValue(obj);

            }

        }

        private static  object GetNullableValue<T>(PropertyInfo item, T obj) where T : class, new()
        {
            var nfiDecimal = new NumberFormatInfo();
            nfiDecimal.NumberDecimalSeparator = ".";

            var value = item.GetValue(obj);
            if (value is int)
                return value;
            else if (value == null)
                return "null";
            else if (value is true)
                return "true";
            else if (value is false)
                return "false";

            else if (value is decimal)
                return Convert.ToDecimal(item.GetValue(obj)).ToString(nfiDecimal);
            else if (value is double)
                return Convert.ToDouble(item.GetValue(obj)).ToString(nfiDecimal);

            return ((bool)item.GetValue(obj)) == true ? 1 : 0;
        }
    }
}
