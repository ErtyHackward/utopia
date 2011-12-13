using ProtoBuf;

namespace LostIsland.Shared.Web.Responces
{
    [ProtoContract]
    public class UserAuthenticationResponce
    {
        [ProtoMember(1)]
        public string Email { get; set; }
        [ProtoMember(2)]
        public bool Valid { get; set; }
    }
}