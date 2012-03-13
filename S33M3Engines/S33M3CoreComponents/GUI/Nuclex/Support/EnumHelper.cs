using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace S33M3CoreComponents.GUI.Nuclex.Support
{
    /// <summary>Helper methods for enumerations</summary>
    public static class EnumHelper
    {
        /// <summary>Returns the highest value encountered in an enumeration</summary>
        /// <typeparam name="TEnum">
        ///   Enumeration of which the highest value will be returned
        /// </typeparam>
        /// <returns>The highest value in the enumeration</returns>
        public static TEnum GetHighestValue<TEnum>() where TEnum : IComparable
        {
            TEnum[] values = GetValues<TEnum>();

            // If the enumeration is empty, return nothing
            if (values.Length == 0)
            {
                return default(TEnum);
            }

            // Look for the highest value in the enumeration. We initialize the highest value
            // to the first enumeration value so we don't have to use some arbitrary starting
            // value which might actually appear in the enumeration.
            TEnum highestValue = values[0];
            for (int index = 1; index < values.Length; ++index)
            {
                if (values[index].CompareTo(highestValue) > 0)
                {
                    highestValue = values[index];
                }
            }

            return highestValue;
        }

        /// <summary>Returns the lowest value encountered in an enumeration</summary>
        /// <typeparam name="EnumType">
        ///   Enumeration of which the lowest value will be returned
        /// </typeparam>
        /// <returns>The lowest value in the enumeration</returns>
        public static EnumType GetLowestValue<EnumType>() where EnumType : IComparable
        {
            EnumType[] values = GetValues<EnumType>();

            // If the enumeration is empty, return nothing
            if (values.Length == 0)
            {
                return default(EnumType);
            }

            // Look for the lowest value in the enumeration. We initialize the lowest value
            // to the first enumeration value so we don't have to use some arbitrary starting
            // value which might actually appear in the enumeration.
            EnumType lowestValue = values[0];
            for (int index = 1; index < values.Length; ++index)
            {
                if (values[index].CompareTo(lowestValue) < 0)
                {
                    lowestValue = values[index];
                }
            }

            return lowestValue;
        }

        /// <summary>Retrieves a list of all values contained in an enumeration</summary>
        /// <typeparam name="TEnum">
        ///   Type of the enumeration whose values will be returned
        /// </typeparam>
        /// <returns>All values contained in the specified enumeration</returns>
        /// <remarks>
        ///   This method produces collectable garbage so it's best to only call it once
        ///   and cache the result.
        /// </remarks>
        public static TEnum[] GetValues<TEnum>()
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        /// <summary>Retrieves a list of all values contained in an enumeration</summary>
        /// <typeparam name="TEnum">
        ///   Type of the enumeration whose values will be returned
        /// </typeparam>
        /// <returns>All values contained in the specified enumeration</returns>
        internal static TEnum[] GetValuesXbox360<TEnum>()
        {
            Type enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(
                  "The provided type needs to be an enumeration", "EnumType"
                );
            }

            // Use reflection to get all fields in the enumeration      
            FieldInfo[] fieldInfos = enumType.GetFields(
              BindingFlags.Public | BindingFlags.Static
            );

            // Create an array to hold the enumeration values and copy them over from
            // the fields we just retrieved
            TEnum[] values = new TEnum[fieldInfos.Length];
            for (int index = 0; index < fieldInfos.Length; ++index)
            {
                values[index] = (TEnum)fieldInfos[index].GetValue(null);
            }

            return values;
        }

    }
}
