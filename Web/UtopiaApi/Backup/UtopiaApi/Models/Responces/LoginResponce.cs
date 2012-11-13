using ProtoBuf;

namespace UtopiaApi.Models.Responces
{
    [ProtoContract]
    public class LoginResponce
    {
        [ProtoMember(1)]
        public bool Logged { get; set; }
        [ProtoMember(2)]
        public string DisplayName { get; set; }
        [ProtoMember(3)]
        public string Token { get; set; }
        [ProtoMember(4)]
        public string Error { get; set; }
        [ProtoMember(5)]
        public string Notice { get; set; }
    }
}