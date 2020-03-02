using SimpleQuery.Domain.Data;
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

        public static object GetValue<T>(PropertyInfo item, T obj, DbServerType dbServerType) where T : class, new()
        {
            var value = item.GetValue(obj);
            switch (item.PropertyType.Name)
            {
                case "String":
                    if (value is null)
                        return "null";
                    else
                        return $"'{value.ToString().Replace("'", "''")}'";
                case "Boolean":
                    if (dbServerType == DbServerType.Hana || dbServerType == DbServerType.PostGres)
                        return ((bool)item.GetValue(obj)) == true ? "true" : "false";
                    else
                        return ((bool)item.GetValue(obj)) == true ? "1" : "0";
                case "DateTime":
                    return $"'{((DateTime)item.GetValue(obj)).ToString("yyyy-MM-dd HH:mm:ss")}'";

                case "Nullable`1":
                    return GetNullableValue(item, obj, dbServerType);
                case "Double":
                    var nfiDouble = new NumberFormatInfo();
                    nfiDouble.NumberDecimalSeparator = ".";
                    return Convert.ToDouble(item.GetValue(obj)).ToString(nfiDouble);
                case "Decimal":
                    var nfiDecimal = new NumberFormatInfo();
                    nfiDecimal.NumberDecimalSeparator = ".";
                    return Convert.ToDecimal(item.GetValue(obj)).ToString(nfiDecimal);
                case "Byte[]":
                    return (item.GetValue(obj));
                default:
                    return item.GetValue(obj);

            }

        }

        public static object GetValueForParameter(PropertyInfo item, object instanceModel, DbServerType dbServerType)
        {
            var value = item.GetValue(instanceModel);
            switch (item.PropertyType.Name)
            {
                case "String":
                    if (value is null)
                        return "null";
                    else
                        return $"{item.GetValue(instanceModel)}";
                case "Boolean":
                    if (dbServerType == DbServerType.Hana || dbServerType == DbServerType.PostGres)
                        return ((bool)item.GetValue(instanceModel)) == true ? "true" : "false";
                    else
                        return ((bool)item.GetValue(instanceModel)) == true ? "1" : "0";
                case "DateTime":
                    return $"{((DateTime)item.GetValue(instanceModel)).ToString("yyyy-MM-dd HH:mm:ss")}";

                case "Nullable`1":
                    return GetNullableValue(item, instanceModel, dbServerType);
                case "Double":
                    var nfiDouble = new NumberFormatInfo();
                    nfiDouble.NumberDecimalSeparator = ".";
                    return Convert.ToDouble(item.GetValue(instanceModel)).ToString(nfiDouble);
                case "Decimal":
                    var nfiDecimal = new NumberFormatInfo();
                    nfiDecimal.NumberDecimalSeparator = ".";
                    return Convert.ToDecimal(item.GetValue(instanceModel)).ToString(nfiDecimal);
                case "Byte[]":
                    return (item.GetValue(instanceModel));
                default:
                    return item.GetValue(instanceModel);

            }

        }

        private static object GetNullableValue<T>(PropertyInfo item, T obj, DbServerType dbServerType) where T : class, new()
        {
            var nfiDecimal = new NumberFormatInfo();
            nfiDecimal.NumberDecimalSeparator = ".";

            var value = item.GetValue(obj);
            if (value is int)
                return value;
            else if (value == null)
                return "null";
            else if (value is true && (dbServerType == DbServerType.SqlServer || dbServerType == DbServerType.Sqlite))
                return "1";
            else if (value is false && (dbServerType == DbServerType.SqlServer || dbServerType == DbServerType.Sqlite))
                return "0";
            else if (value is true)
                return "true";
            else if (value is false)
                return "false";

            else if (value is decimal)
                return Convert.ToDecimal(item.GetValue(obj)).ToString(nfiDecimal);
            else if (value is double)
                return Convert.ToDouble(item.GetValue(obj)).ToString(nfiDecimal);
            else if (value is DateTime)
                return $"'{ Convert.ToDateTime(value).ToString("yyyy-MM-dd")}'";
            return ((bool)item.GetValue(obj)) == true ? 1 : 0;
        }
    }
}
