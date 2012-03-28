using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Tools
{
    public static class DataTypes
    {
        public enum typeFamilly
        {
            Unknown,
            IntegerNumber,
            FloatNumber,
            String,
            Boolean
        }

        public static typeFamilly GetTypeFamilly(Type type)
        {
            if (type == typeof(Int16)) return typeFamilly.IntegerNumber;
            if (type == typeof(Int32)) return typeFamilly.IntegerNumber;
            if (type == typeof(Int64)) return typeFamilly.IntegerNumber;
            if (type == typeof(float)) return typeFamilly.FloatNumber;
            if (type == typeof(double)) return typeFamilly.FloatNumber;
            if (type == typeof(string)) return typeFamilly.String;
            if (type == typeof(bool)) return typeFamilly.Boolean;
            return typeFamilly.Unknown;
        }
    }
}
