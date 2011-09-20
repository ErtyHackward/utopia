using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    public enum ExitReason
    {
        UserRequest,
        Error
    }

    public struct GameExitReasonMessage
    {
        public ExitReason GameExitReason;
        public string MainMessage;
        public string DetailedMessage;
    }
}
