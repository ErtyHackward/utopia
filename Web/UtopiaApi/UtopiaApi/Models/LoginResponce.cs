using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProtoBuf;

namespace UtopiaApi.Models
{
    [ProtoContract]
    public class LoginResponce
    {
        [ProtoMember(1)]
        public bool Logged { get; set; }
        [ProtoMember(2)]
        public string Token { get; set; }
        [ProtoMember(3)]
        public string Error { get; set; }
        [ProtoMember(4)]
        public string Notice { get; set; }
    }
}