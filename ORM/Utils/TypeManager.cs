using System;
using System.Data;

namespace ORM.Utils
{
    public static class TypeManager
    {
        public static SqlDbType ToSqlType(this Type type)
        {
            return GetSqlType(Type.GetTypeCode(type));
        }

        public static SqlDbType GetSqlType(Type type)
        {
            return GetSqlType(Type.GetTypeCode(type));
        }

        public static bool IsCharBasedSqlType(SqlDbType sqlType)
        {
            return  sqlType == SqlDbType.NVarChar   ||
                    sqlType == SqlDbType.NChar      ||
                    sqlType == SqlDbType.VarChar    ||
                    sqlType == SqlDbType.Char       ||
                    sqlType == SqlDbType.Text;
        }

        public static bool IsNumeric(this Type type)
        {
            return IsNumericType(type);
        }

        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        public static object DefaultValue(this Type type)
        {
            return GetDefaultValue(type);
        }

        public static bool IsUserDefinedClass(this Type type)
        {
            return type.IsClass &&
                    !type.FullName.StartsWith("System.");
        }

        public static SqlDbType GetSqlType(TypeCode type)
        {
            switch (type)
            {
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    {
                        return SqlDbType.SmallInt;
                    }
                case TypeCode.UInt32:
                case TypeCode.Int32:
                    {
                        return SqlDbType.Int;
                    }
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    {
                        return SqlDbType.BigInt;
                    }
                case TypeCode.SByte:
                case TypeCode.Byte:
                    {
                        return SqlDbType.Binary;
                    }
                case TypeCode.Boolean:
                    {
                        return SqlDbType.Bit;
                    }
                case TypeCode.String:
                    {
                        return SqlDbType.NVarChar;
                    }
                case TypeCode.Char:
                    {
                        return SqlDbType.NChar;
                    }
                case TypeCode.DateTime:
                    {
                        return SqlDbType.DateTime;
                    }
                case TypeCode.Decimal:
                    {
                        return SqlDbType.Decimal;
                    }
                case TypeCode.Double:
                    {
                        return SqlDbType.Float;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
        }

        public static bool ConvertibleToSql(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.DateTime:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsConvertibleToSql(this Type type)
        {
            return ConvertibleToSql(type);
        }
    }
}
