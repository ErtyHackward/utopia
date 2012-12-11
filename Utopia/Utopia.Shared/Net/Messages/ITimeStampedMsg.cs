using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Net.Messages
{
    public interface ITimeStampedMsg
    {
        /// <summary>
        /// Value filled in with the datetime at message creation time
        /// </summary>
        DateTime MessageRecTime { get; set; }
    }
}
