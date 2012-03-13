using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace S33M3CoreComponents.Meshes.Factories.Helpers
{
    /// <summary>
    /// Exceptions that are thrown by this namespace and the Scanner Class
    /// </summary>
    class ScanException : Exception
    {
        public ScanException()
            : base()
        {
        }

        public ScanException(string message)
            : base(message)
        {
        }

        public ScanException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ScanException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
