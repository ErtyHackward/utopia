﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace S33M3_CoreComponents.GUI.Nuclex.Support
{
    /// <summary>Supporting functions for the plugin classes</summary>
    public static class PluginHelper
    {
        /// <summary>Determines whether the given type has a default constructor</summary>
        /// <param name="type">Type which is to be checked</param>
        /// <returns>True if the type has a default constructor</returns>
        public static bool HasDefaultConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();

            foreach (ConstructorInfo constructor in constructors)
                if (constructor.IsPublic && (constructor.GetParameters().Length == 0))
                    return true;

            return false;
        }

    }
}
