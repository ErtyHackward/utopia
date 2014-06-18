using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared
{
    /// <summary>
    /// Класс позволяет выводить данные о размере файлов
    /// </summary>
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        private static readonly FileSizeFormatProvider instance = new FileSizeFormatProvider();

        public static FileSizeFormatProvider Instance
        {
            get
            {
                return instance;
            }
        }

        public object GetFormat(Type formatType)
        {
            if (typeof (ICustomFormatter).IsAssignableFrom(formatType))
                return this;
            return null;
        }

        private const string fileSizeFormat = "fs";

        private static readonly string[] letters = new string[] {" B", " KB", " MB", " GB", " TB", " PB"};

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(fileSizeFormat))
            {
                return defaultFormat(format, arg, formatProvider);
            }

            Decimal size;
            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            byte i = 0;
            while ((size >= 1024) && (i < letters.Length - 1))
            {
                i++;
                size /= 1024;
            }

            string precision = format.Substring(2);
            if (String.IsNullOrEmpty(precision)) precision = "2";

            return String.Format("{0:N" + precision + "}{1}", size, letters[i]);

        }

        private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }

            return arg.ToString();
        }
    }

    public static class BytesHelper
    {
        /// <summary>
        /// Translates size to string representations
        /// </summary>
        /// <param name="value">amount of bytes</param>
        /// <returns>String like 1 KB</returns>
        public static string FormatBytes(long value)
        {
            return string.Format(FileSizeFormatProvider.Instance, "{0:fs}", value);
        }

        /// <summary>
        /// Translates size to string representations
        /// </summary>
        /// <param name="value">amount of bytes</param>
        /// <returns>String like 1 KB</returns>
        public static string FormatBytes(double value)
        {
            return string.Format(FileSizeFormatProvider.Instance, "{0:fs}", value);
        }
    }

    public static class OtherHelper
    {
        /// <summary>
        /// Tells if the collection has at least the number of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool CountAtLeast<T>(this IEnumerable<T> collection, int number)
        {
            return collection.Take(number).Count() == number;
        }
    }
}
