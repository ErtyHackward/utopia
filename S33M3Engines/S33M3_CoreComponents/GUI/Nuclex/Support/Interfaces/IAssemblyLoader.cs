using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace S33M3_CoreComponents.GUI.Nuclex.Support.Interfaces
{
    /// <summary>Interface for an assembly loading helper</summary>
    public interface IAssemblyLoader
    {
        /// <summary>Tries to loads an assembly from a file</summary>
        /// <param name="path">Path to the file that is loaded as an assembly</param>
        /// <param name="loadedAssembly">
        ///   Output parameter that receives the loaded assembly or null
        /// </param>
        /// <returns>True if the assembly was loaded successfully, otherwise false</returns>
        bool TryLoadFile(string path, out Assembly loadedAssembly);

    }
}
